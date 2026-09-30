using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using LDIS.Core.Data.Repositories;
using LDIS.Core.DTOs;
using LDIS.Core.Export;
using LDIS.Core.Import;
using LDIS.Core.Models;

namespace LDIS.Core.Services
{
    public class ProductImportService : IProductImportService
    {
        private readonly IItemRepository _itemRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly ExcelProductReader _reader;
        private readonly ExcelTemplateWriter _templateWriter;

        public ProductImportService(
            IItemRepository itemRepository,
            ICategoryRepository categoryRepository,
            ExcelProductReader reader = null,
            ExcelTemplateWriter templateWriter = null)
        {
            if (itemRepository == null) throw new ArgumentNullException("itemRepository");
            if (categoryRepository == null) throw new ArgumentNullException("categoryRepository");

            _itemRepository = itemRepository;
            _categoryRepository = categoryRepository;
            _reader = reader ?? new ExcelProductReader();
            _templateWriter = templateWriter ?? new ExcelTemplateWriter();
        }

        public ProductImportResult ValidateWorkbook(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("File path cannot be null or empty.", "filePath");

            if (!File.Exists(filePath))
                throw new FileNotFoundException("Excel file not found.", filePath);

            using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                var result = ValidateWorkbook(stream);
                result.FilePath = filePath;
                return result;
            }
        }

        public ProductImportResult ValidateWorkbook(Stream stream)
        {
            if (stream == null) throw new ArgumentNullException("stream");

            var result = new ProductImportResult();

            // 1. Parse Excel structure
            var readResult = _reader.Read(stream);
            if (!readResult.Success)
            {
                result.Errors.AddRange(readResult.StructuralErrors);
                result.TotalRowsRead = 0;
                result.ValidRowCount = 0;
                result.ErrorRowCount = 0;
                return result;
            }

            var rawRows = readResult.Rows;
            result.TotalRowsRead = rawRows.Count;

            // 2. Pre-fetch existing categories for efficient case-insensitive lookup
            var existingCategories = new Dictionary<string, Category>(StringComparer.OrdinalIgnoreCase);
            foreach (var cat in _categoryRepository.GetAll())
            {
                if (cat != null && !string.IsNullOrWhiteSpace(cat.CategoryName))
                {
                    existingCategories[cat.CategoryName.Trim()] = cat;
                }
            }

            // 3. Track SKUs for workbook uniqueness
            var workbookSkus = new Dictionary<string, List<int>>(StringComparer.OrdinalIgnoreCase);
            var parsedRowDtos = new List<ProductImportRowDto>();

            // 4. Validate each raw row
            foreach (var raw in rawRows)
            {
                var rowDto = new ProductImportRowDto
                {
                    RowNumber = raw.RowNumber
                };

                // Add any low-level cell read/formula errors
                foreach (var cellErr in raw.CellErrors)
                {
                    result.AddRowError(raw.RowNumber, cellErr.ColumnName, cellErr.RawValue, cellErr.ErrorMessage);
                    rowDto.IsValid = false;
                    rowDto.ErrorMessages.Add(cellErr.ErrorMessage);
                }

                // SKU validation
                string rawSku = raw.RawSku != null ? raw.RawSku.Trim() : string.Empty;
                if (string.IsNullOrEmpty(rawSku))
                {
                    result.AddRowError(raw.RowNumber, ExcelProductReader.ColSku, raw.RawSku, "SKU is required.");
                    rowDto.IsValid = false;
                    rowDto.ErrorMessages.Add("SKU is required.");
                }
                else
                {
                    rowDto.SKU = rawSku;
                    if (rawSku.Length > 50)
                    {
                        result.AddRowError(raw.RowNumber, ExcelProductReader.ColSku, rawSku, "SKU must not exceed 50 characters.");
                        rowDto.IsValid = false;
                        rowDto.ErrorMessages.Add("SKU must not exceed 50 characters.");
                    }

                    // Track for duplicate workbook check
                    if (!workbookSkus.ContainsKey(rawSku))
                    {
                        workbookSkus[rawSku] = new List<int>();
                    }
                    workbookSkus[rawSku].Add(raw.RowNumber);
                }

                // Name validation
                string rawName = raw.RawName != null ? raw.RawName.Trim() : string.Empty;
                if (string.IsNullOrEmpty(rawName))
                {
                    result.AddRowError(raw.RowNumber, ExcelProductReader.ColName, raw.RawName, "Product name is required.");
                    rowDto.IsValid = false;
                    rowDto.ErrorMessages.Add("Product name is required.");
                }
                else
                {
                    rowDto.Name = rawName;
                    if (rawName.Length > 150)
                    {
                        result.AddRowError(raw.RowNumber, ExcelProductReader.ColName, rawName, "Product name must not exceed 150 characters.");
                        rowDto.IsValid = false;
                        rowDto.ErrorMessages.Add("Product name must not exceed 150 characters.");
                    }
                }

                // Category validation (Optional, but must exist if specified)
                string rawCategory = raw.RawCategory != null ? raw.RawCategory.Trim() : string.Empty;
                if (!string.IsNullOrEmpty(rawCategory))
                {
                    Category matchedCategory;
                    if (existingCategories.TryGetValue(rawCategory, out matchedCategory))
                    {
                        rowDto.CategoryID = matchedCategory.CategoryID;
                        rowDto.CategoryName = matchedCategory.CategoryName;
                    }
                    else
                    {
                        string msg = string.Format("Category '{0}' does not exist. Please create it in Category Management before importing.", rawCategory);
                        result.AddRowError(raw.RowNumber, ExcelProductReader.ColCategory, rawCategory, msg);
                        rowDto.IsValid = false;
                        rowDto.ErrorMessages.Add(msg);
                    }
                }

                // Brand (Optional, max 50 chars)
                string rawBrand = raw.RawBrand != null ? raw.RawBrand.Trim() : null;
                if (!string.IsNullOrEmpty(rawBrand))
                {
                    rowDto.Brand = rawBrand;
                    if (rawBrand.Length > 50)
                    {
                        result.AddRowError(raw.RowNumber, ExcelProductReader.ColBrand, rawBrand, "Brand must not exceed 50 characters.");
                        rowDto.IsValid = false;
                        rowDto.ErrorMessages.Add("Brand must not exceed 50 characters.");
                    }
                }

                // Color (Optional, max 50 chars)
                string rawColor = raw.RawColor != null ? raw.RawColor.Trim() : null;
                if (!string.IsNullOrEmpty(rawColor))
                {
                    rowDto.Color = rawColor;
                    if (rawColor.Length > 50)
                    {
                        result.AddRowError(raw.RowNumber, ExcelProductReader.ColColor, rawColor, "Color must not exceed 50 characters.");
                        rowDto.IsValid = false;
                        rowDto.ErrorMessages.Add("Color must not exceed 50 characters.");
                    }
                }

                // Size (Optional, max 50 chars)
                string rawSize = raw.RawSize != null ? raw.RawSize.Trim() : null;
                if (!string.IsNullOrEmpty(rawSize))
                {
                    rowDto.Size = rawSize;
                    if (rawSize.Length > 50)
                    {
                        result.AddRowError(raw.RowNumber, ExcelProductReader.ColSize, rawSize, "Size must not exceed 50 characters.");
                        rowDto.IsValid = false;
                        rowDto.ErrorMessages.Add("Size must not exceed 50 characters.");
                    }
                }

                // Gender (Optional, controlled options, blank normalizes to None)
                string rawGender = raw.RawGender != null ? raw.RawGender.Trim() : string.Empty;
                if (string.IsNullOrEmpty(rawGender))
                {
                    rowDto.Gender = GenderOptions.None;
                }
                else
                {
                    if (GenderOptions.IsValid(rawGender))
                    {
                        rowDto.Gender = GenderOptions.Normalize(rawGender);
                    }
                    else
                    {
                        string msg = string.Format("Invalid gender '{0}'. Allowed values: Unisex, Men, Women, Kids, None / Unspecified.", rawGender);
                        result.AddRowError(raw.RowNumber, ExcelProductReader.ColGender, rawGender, msg);
                        rowDto.IsValid = false;
                        rowDto.ErrorMessages.Add(msg);
                    }
                }

                // Purchase Price (Optional, whole integer >= 0, blank defaults to 0)
                long purchasePrice;
                string purError;
                if (TryParseWholeLong(raw.RawPurchasePrice, ExcelProductReader.ColPurchasePrice, out purchasePrice, out purError))
                {
                    rowDto.PurchasePrice = purchasePrice;
                }
                else
                {
                    result.AddRowError(raw.RowNumber, ExcelProductReader.ColPurchasePrice, raw.RawPurchasePrice, purError);
                    rowDto.IsValid = false;
                    rowDto.ErrorMessages.Add(purError);
                }

                // Selling Price (Optional, whole integer >= 0, blank defaults to 0)
                long sellingPrice;
                string sellError;
                if (TryParseWholeLong(raw.RawSellingPrice, ExcelProductReader.ColSellingPrice, out sellingPrice, out sellError))
                {
                    rowDto.SellingPrice = sellingPrice;
                }
                else
                {
                    result.AddRowError(raw.RowNumber, ExcelProductReader.ColSellingPrice, raw.RawSellingPrice, sellError);
                    rowDto.IsValid = false;
                    rowDto.ErrorMessages.Add(sellError);
                }

                // Min Stock Level (Optional, whole integer >= 0, blank defaults to 0)
                int minStock;
                string minStockError;
                if (TryParseWholeInt(raw.RawMinStockLevel, ExcelProductReader.ColMinStockLevel, out minStock, out minStockError))
                {
                    rowDto.MinStockLevel = minStock;
                }
                else
                {
                    result.AddRowError(raw.RowNumber, ExcelProductReader.ColMinStockLevel, raw.RawMinStockLevel, minStockError);
                    rowDto.IsValid = false;
                    rowDto.ErrorMessages.Add(minStockError);
                }

                parsedRowDtos.Add(rowDto);
            }

            // 5. Cross-row validation: Duplicate SKU within the workbook
            foreach (var kvp in workbookSkus)
            {
                if (kvp.Value.Count > 1)
                {
                    string rowsList = string.Join(", ", kvp.Value);
                    string msg = string.Format("Duplicate SKU '{0}' found in workbook (rows {1}).", kvp.Key, rowsList);

                    foreach (int rNum in kvp.Value)
                    {
                        result.AddRowError(rNum, ExcelProductReader.ColSku, kvp.Key, msg);
                        var targetRow = parsedRowDtos.FirstOrDefault(r => r.RowNumber == rNum);
                        if (targetRow != null)
                        {
                            targetRow.IsValid = false;
                            targetRow.ErrorMessages.Add(msg);
                        }
                    }
                }
            }

            // 6. Cross-database validation: Existing SKU collisions
            foreach (var kvp in workbookSkus)
            {
                if (_itemRepository.ExistsSku(kvp.Key))
                {
                    string msg = string.Format("SKU '{0}' is already in use by an existing product in the database.", kvp.Key);

                    foreach (int rNum in kvp.Value)
                    {
                        result.AddRowError(rNum, ExcelProductReader.ColSku, kvp.Key, msg);
                        var targetRow = parsedRowDtos.FirstOrDefault(r => r.RowNumber == rNum);
                        if (targetRow != null)
                        {
                            targetRow.IsValid = false;
                            targetRow.ErrorMessages.Add(msg);
                        }
                    }
                }
            }

            // 7. Aggregate valid vs invalid rows
            foreach (var r in parsedRowDtos)
            {
                if (r.IsValid)
                {
                    result.ValidRows.Add(r);
                }
            }

            result.ValidRowCount = result.ValidRows.Count;
            result.ErrorRowCount = parsedRowDtos.Count(r => !r.IsValid);

            return result;
        }

        public int ExecuteImport(IEnumerable<ProductImportRowDto> validRows)
        {
            if (validRows == null)
            {
                throw new ArgumentNullException("validRows");
            }

            var rowList = validRows.ToList();
            if (rowList.Count == 0)
            {
                throw new InvalidOperationException("No valid product rows provided for import.");
            }

            // Confirm that all rows are marked valid
            if (rowList.Any(r => !r.IsValid))
            {
                throw new InvalidOperationException("Cannot execute import: one or more product rows contain validation errors.");
            }

            // Map DTOs to Item domain models
            var itemsToInsert = new List<Item>();
            foreach (var r in rowList)
            {
                itemsToInsert.Add(new Item
                {
                    SKU = r.SKU.Trim(),
                    Name = r.Name.Trim(),
                    CategoryID = r.CategoryID,
                    Brand = !string.IsNullOrWhiteSpace(r.Brand) ? r.Brand.Trim() : null,
                    Color = !string.IsNullOrWhiteSpace(r.Color) ? r.Color.Trim() : null,
                    Size = !string.IsNullOrWhiteSpace(r.Size) ? r.Size.Trim() : null,
                    Gender = r.Gender,
                    PurchasePrice = r.PurchasePrice,
                    SellingPrice = r.SellingPrice,
                    MinStockLevel = r.MinStockLevel,
                    CurrentStock = 0,
                    IsActive = true
                });
            }

            // Execute atomic batch insert via repository
            return _itemRepository.CreateBatch(itemsToInsert);
        }

        public void GenerateTemplate(string filePath)
        {
            _templateWriter.WriteTemplate(filePath);
        }

        public void GenerateTemplate(Stream stream)
        {
            _templateWriter.WriteTemplate(stream);
        }

        private static bool TryParseWholeLong(string input, string fieldName, out long result, out string error)
        {
            result = 0;
            error = null;

            if (string.IsNullOrWhiteSpace(input))
            {
                return true; // Blank defaults to 0
            }

            string clean = input.Trim().Replace(",", "");

            decimal dec;
            if (!decimal.TryParse(clean, NumberStyles.Float, CultureInfo.InvariantCulture, out dec))
            {
                error = string.Format("{0} must be a valid whole number.", fieldName);
                return false;
            }

            if (dec < 0)
            {
                error = string.Format("{0} cannot be negative.", fieldName);
                return false;
            }

            if (dec % 1 != 0)
            {
                error = string.Format("{0} cannot contain decimal places.", fieldName);
                return false;
            }

            if (dec > long.MaxValue)
            {
                error = string.Format("{0} exceeds the maximum allowed monetary value.", fieldName);
                return false;
            }

            result = (long)dec;
            return true;
        }

        private static bool TryParseWholeInt(string input, string fieldName, out int result, out string error)
        {
            result = 0;
            error = null;

            if (string.IsNullOrWhiteSpace(input))
            {
                return true; // Blank defaults to 0
            }

            string clean = input.Trim().Replace(",", "");

            decimal dec;
            if (!decimal.TryParse(clean, NumberStyles.Float, CultureInfo.InvariantCulture, out dec))
            {
                error = string.Format("{0} must be a valid whole number.", fieldName);
                return false;
            }

            if (dec < 0)
            {
                error = string.Format("{0} cannot be negative.", fieldName);
                return false;
            }

            if (dec % 1 != 0)
            {
                error = string.Format("{0} cannot contain decimal places.", fieldName);
                return false;
            }

            if (dec > int.MaxValue)
            {
                error = string.Format("{0} exceeds the maximum allowed stock level.", fieldName);
                return false;
            }

            result = (int)dec;
            return true;
        }
    }
}
