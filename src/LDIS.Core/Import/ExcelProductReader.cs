using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using ClosedXML.Excel;
using LDIS.Core.DTOs;

namespace LDIS.Core.Import
{
    /// <summary>
    /// Raw unvalidated row read from the 'Products' worksheet.
    /// </summary>
    public class RawProductImportRow
    {
        public int RowNumber { get; set; }
        public string RawSku { get; set; }
        public string RawName { get; set; }
        public string RawCategory { get; set; }
        public string RawBrand { get; set; }
        public string RawColor { get; set; }
        public string RawSize { get; set; }
        public string RawGender { get; set; }
        public string RawPurchasePrice { get; set; }
        public string RawSellingPrice { get; set; }
        public string RawMinStockLevel { get; set; }
        public List<ProductImportValidationError> CellErrors { get; set; }

        public RawProductImportRow()
        {
            CellErrors = new List<ProductImportValidationError>();
        }
    }

    /// <summary>
    /// Result of reading the workbook structure and raw cell contents.
    /// </summary>
    public class ExcelReadResult
    {
        public bool Success
        {
            get { return StructuralErrors != null && StructuralErrors.Count == 0; }
        }
        public List<ProductImportValidationError> StructuralErrors { get; set; }
        public List<RawProductImportRow> Rows { get; set; }

        public ExcelReadResult()
        {
            StructuralErrors = new List<ProductImportValidationError>();
            Rows = new List<RawProductImportRow>();
        }
    }

    /// <summary>
    /// Reads and parses OpenXML (.xlsx) product import workbooks using ClosedXML.
    /// Strictly targets the 'Products' worksheet and safely handles text, numeric, and formula cells.
    /// </summary>
    public class ExcelProductReader
    {
        private static readonly string[] RecognizedErrors = new[]
        {
            "#REF!", "#DIV/0!", "#VALUE!", "#NAME?", "#N/A", "#NUM!", "#NULL!"
        };

        // Standard column identifiers
        public const string ColSku = "SKU";
        public const string ColName = "Name";
        public const string ColCategory = "Category";
        public const string ColBrand = "Brand";
        public const string ColColor = "Color";
        public const string ColSize = "Size";
        public const string ColGender = "Gender";
        public const string ColPurchasePrice = "Purchase Price";
        public const string ColSellingPrice = "Selling Price";
        public const string ColMinStockLevel = "Min Stock Level";

        private static readonly string[] RequiredHeaderNames = new[]
        {
            ColSku,
            ColName,
            ColCategory,
            ColBrand,
            ColColor,
            ColSize,
            ColGender,
            ColPurchasePrice,
            ColSellingPrice,
            ColMinStockLevel
        };

        public ExcelReadResult Read(Stream stream)
        {
            if (stream == null) throw new ArgumentNullException("stream");

            var result = new ExcelReadResult();

            XLWorkbook workbook;
            try
            {
                workbook = new XLWorkbook(stream);
            }
            catch (Exception ex)
            {
                result.StructuralErrors.Add(new ProductImportValidationError(
                    0, "File", string.Empty, "Unable to read Excel workbook: " + ex.Message));
                return result;
            }

            using (workbook)
            {
                // 1. Locate the 'Products' worksheet (Case-insensitive)
                IXLWorksheet ws = null;
                foreach (var sheet in workbook.Worksheets)
                {
                    if (string.Equals(sheet.Name.Trim(), "Products", StringComparison.OrdinalIgnoreCase))
                    {
                        ws = sheet;
                        break;
                    }
                }

                if (ws == null)
                {
                    result.StructuralErrors.Add(new ProductImportValidationError(
                        0, "Worksheet", string.Empty, "The workbook must contain a worksheet named 'Products'."));
                    return result;
                }

                // 2. Validate Headers (Row 1)
                var headerMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                var headerPositions = new Dictionary<int, string>();
                var duplicateHeaders = new List<string>();

                var firstRow = ws.Row(1);
                int lastUsedCol = ws.LastColumnUsed() != null ? ws.LastColumnUsed().ColumnNumber() : 0;

                if (lastUsedCol == 0)
                {
                    result.StructuralErrors.Add(new ProductImportValidationError(
                        1, "Header", string.Empty, "Worksheet 'Products' is empty. Header row 1 is missing."));
                    return result;
                }

                for (int col = 1; col <= lastUsedCol; col++)
                {
                    var cell = firstRow.Cell(col);
                    string dummyError;
                    string rawHeader = ExtractCellValue(cell, out dummyError);
                    string header = rawHeader != null ? rawHeader.Trim() : string.Empty;

                    if (string.IsNullOrEmpty(header))
                    {
                        // Blank column header in row 1
                        continue;
                    }

                    // Alias normalization: "Min Stock" -> "Min Stock Level"
                    string canonicalHeader = header;
                    if (string.Equals(header, "Min Stock", StringComparison.OrdinalIgnoreCase))
                    {
                        canonicalHeader = ColMinStockLevel;
                    }

                    if (headerMap.ContainsKey(canonicalHeader))
                    {
                        duplicateHeaders.Add(header);
                    }
                    else
                    {
                        headerMap[canonicalHeader] = col;
                        headerPositions[col] = canonicalHeader;
                    }
                }

                // Check duplicate headers
                if (duplicateHeaders.Count > 0)
                {
                    result.StructuralErrors.Add(new ProductImportValidationError(
                        1, "Header", string.Join(", ", duplicateHeaders),
                        string.Format("Duplicate column header(s) found in Row 1: {0}.", string.Join(", ", duplicateHeaders))));
                }

                // Check missing required headers
                var missingHeaders = new List<string>();
                foreach (var req in RequiredHeaderNames)
                {
                    if (!headerMap.ContainsKey(req))
                    {
                        missingHeaders.Add(req);
                    }
                }

                if (missingHeaders.Count > 0)
                {
                    result.StructuralErrors.Add(new ProductImportValidationError(
                        1, "Header", string.Empty,
                        string.Format("Missing required column header(s) in Row 1: {0}.", string.Join(", ", missingHeaders))));
                }

                // Check unexpected extra columns (Strictness per CORRECTION B)
                var unexpectedHeaders = new List<string>();
                foreach (var kvp in headerPositions)
                {
                    if (!RequiredHeaderNames.Contains(kvp.Value, StringComparer.OrdinalIgnoreCase))
                    {
                        unexpectedHeaders.Add(kvp.Value);
                    }
                }

                if (unexpectedHeaders.Count > 0)
                {
                    result.StructuralErrors.Add(new ProductImportValidationError(
                        1, "Header", string.Join(", ", unexpectedHeaders),
                        string.Format("Unexpected column(s) found in Row 1: {0}. Only the 10 approved template columns are supported.",
                            string.Join(", ", unexpectedHeaders))));
                }

                // If any structural error exists, halt parsing
                if (result.StructuralErrors.Count > 0)
                {
                    return result;
                }

                // 3. Read Data Rows (Row 2 to last used row)
                int lastUsedRow = ws.LastRowUsed() != null ? ws.LastRowUsed().RowNumber() : 1;
                if (lastUsedRow <= 1)
                {
                    result.StructuralErrors.Add(new ProductImportValidationError(
                        0, "Data", string.Empty, "Workbook contains no product rows to import. Please add products to the 'Products' sheet."));
                    return result;
                }

                for (int r = 2; r <= lastUsedRow; r++)
                {
                    var row = ws.Row(r);

                    // Read cells for all 10 columns
                    var rawRow = new RawProductImportRow { RowNumber = r };

                    string skuError = null;
                    string nameError = null;
                    string catError = null;
                    string brandError = null;
                    string colorError = null;
                    string sizeError = null;
                    string genderError = null;
                    string purPriceError = null;
                    string sellPriceError = null;
                    string minStockError = null;

                    rawRow.RawSku = ReadCellString(row, headerMap[ColSku], out skuError);
                    rawRow.RawName = ReadCellString(row, headerMap[ColName], out nameError);
                    rawRow.RawCategory = ReadCellString(row, headerMap[ColCategory], out catError);
                    rawRow.RawBrand = ReadCellString(row, headerMap[ColBrand], out brandError);
                    rawRow.RawColor = ReadCellString(row, headerMap[ColColor], out colorError);
                    rawRow.RawSize = ReadCellString(row, headerMap[ColSize], out sizeError);
                    rawRow.RawGender = ReadCellString(row, headerMap[ColGender], out genderError);
                    rawRow.RawPurchasePrice = ReadCellString(row, headerMap[ColPurchasePrice], out purPriceError);
                    rawRow.RawSellingPrice = ReadCellString(row, headerMap[ColSellingPrice], out sellPriceError);
                    rawRow.RawMinStockLevel = ReadCellString(row, headerMap[ColMinStockLevel], out minStockError);

                    // Check if entire row is blank
                    bool isRowEntirelyBlank =
                        string.IsNullOrWhiteSpace(rawRow.RawSku) &&
                        string.IsNullOrWhiteSpace(rawRow.RawName) &&
                        string.IsNullOrWhiteSpace(rawRow.RawCategory) &&
                        string.IsNullOrWhiteSpace(rawRow.RawBrand) &&
                        string.IsNullOrWhiteSpace(rawRow.RawColor) &&
                        string.IsNullOrWhiteSpace(rawRow.RawSize) &&
                        string.IsNullOrWhiteSpace(rawRow.RawGender) &&
                        string.IsNullOrWhiteSpace(rawRow.RawPurchasePrice) &&
                        string.IsNullOrWhiteSpace(rawRow.RawSellingPrice) &&
                        string.IsNullOrWhiteSpace(rawRow.RawMinStockLevel) &&
                        skuError == null && nameError == null && catError == null &&
                        brandError == null && colorError == null && sizeError == null &&
                        genderError == null && purPriceError == null && sellPriceError == null &&
                        minStockError == null;

                    if (isRowEntirelyBlank)
                    {
                        // Ignore completely blank row
                        continue;
                    }

                    // Record any cell extraction errors
                    if (skuError != null) rawRow.CellErrors.Add(new ProductImportValidationError(r, ColSku, rawRow.RawSku, skuError));
                    if (nameError != null) rawRow.CellErrors.Add(new ProductImportValidationError(r, ColName, rawRow.RawName, nameError));
                    if (catError != null) rawRow.CellErrors.Add(new ProductImportValidationError(r, ColCategory, rawRow.RawCategory, catError));
                    if (brandError != null) rawRow.CellErrors.Add(new ProductImportValidationError(r, ColBrand, rawRow.RawBrand, brandError));
                    if (colorError != null) rawRow.CellErrors.Add(new ProductImportValidationError(r, ColColor, rawRow.RawColor, colorError));
                    if (sizeError != null) rawRow.CellErrors.Add(new ProductImportValidationError(r, ColSize, rawRow.RawSize, sizeError));
                    if (genderError != null) rawRow.CellErrors.Add(new ProductImportValidationError(r, ColGender, rawRow.RawGender, genderError));
                    if (purPriceError != null) rawRow.CellErrors.Add(new ProductImportValidationError(r, ColPurchasePrice, rawRow.RawPurchasePrice, purPriceError));
                    if (sellPriceError != null) rawRow.CellErrors.Add(new ProductImportValidationError(r, ColSellingPrice, rawRow.RawSellingPrice, sellPriceError));
                    if (minStockError != null) rawRow.CellErrors.Add(new ProductImportValidationError(r, ColMinStockLevel, rawRow.RawMinStockLevel, minStockError));

                    result.Rows.Add(rawRow);
                }

                if (result.Rows.Count == 0)
                {
                    result.StructuralErrors.Add(new ProductImportValidationError(
                        0, "Data", string.Empty, "Workbook contains no product rows to import. All rows were blank."));
                }

                return result;
            }
        }

        private static string ReadCellString(IXLRow row, int colIndex, out string calculationError)
        {
            calculationError = null;
            if (colIndex <= 0) return string.Empty;

            var cell = row.Cell(colIndex);
            return ExtractCellValue(cell, out calculationError);
        }

        private static string ExtractCellValue(IXLCell cell, out string calculationError)
        {
            calculationError = null;
            if (cell == null || cell.IsEmpty())
            {
                return string.Empty;
            }

            try
            {
                object rawValue = null;
                try
                {
                    rawValue = cell.Value;
                }
                catch (Exception ex)
                {
                    calculationError = "Calculation error in cell formula: " + ex.Message;
                    return string.Empty;
                }

                if (rawValue == null)
                {
                    return string.Empty;
                }

                string strValue = Convert.ToString(rawValue, CultureInfo.InvariantCulture);

                if (!string.IsNullOrEmpty(strValue))
                {
                    strValue = strValue.Trim();

                    // Check for Excel calculation error strings
                    foreach (var err in RecognizedErrors)
                    {
                        if (string.Equals(strValue, err, StringComparison.OrdinalIgnoreCase))
                        {
                            calculationError = string.Format("Cell contains an Excel calculation error: {0}", err);
                            return strValue;
                        }
                    }
                }

                return strValue;
            }
            catch (Exception ex)
            {
                calculationError = "Error reading cell content: " + ex.Message;
                return string.Empty;
            }
        }
    }
}
