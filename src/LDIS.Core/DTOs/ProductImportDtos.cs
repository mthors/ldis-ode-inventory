using System;
using System.Collections.Generic;

namespace LDIS.Core.DTOs
{
    /// <summary>
    /// Represents a single product row parsed from an Excel import workbook.
    /// </summary>
    public class ProductImportRowDto
    {
        public int RowNumber { get; set; }
        public string SKU { get; set; }
        public string Name { get; set; }
        public string CategoryName { get; set; }
        public long? CategoryID { get; set; }
        public string Brand { get; set; }
        public string Color { get; set; }
        public string Size { get; set; }
        public string Gender { get; set; }
        public long PurchasePrice { get; set; }
        public long SellingPrice { get; set; }
        public int MinStockLevel { get; set; }

        public bool IsValid { get; set; }
        public List<string> ErrorMessages { get; set; }

        public ProductImportRowDto()
        {
            IsValid = true;
            ErrorMessages = new List<string>();
        }
    }

    /// <summary>
    /// Represents an actionable validation error on a specific row and column of the import workbook.
    /// </summary>
    public class ProductImportValidationError
    {
        public int RowNumber { get; set; }
        public string ColumnName { get; set; }
        public string RawValue { get; set; }
        public string ErrorMessage { get; set; }

        public ProductImportValidationError()
        {
        }

        public ProductImportValidationError(int rowNumber, string columnName, string rawValue, string errorMessage)
        {
            RowNumber = rowNumber;
            ColumnName = columnName;
            RawValue = rawValue;
            ErrorMessage = errorMessage;
        }

        public override string ToString()
        {
            if (RowNumber > 0)
            {
                return string.Format("Row {0} [{1}]: {2} (Value: '{3}')", RowNumber, ColumnName, ErrorMessage, RawValue);
            }
            return string.Format("[{0}]: {1}", ColumnName, ErrorMessage);
        }
    }

    /// <summary>
    /// Aggregated result of inspecting and validating an Excel product import workbook.
    /// </summary>
    public class ProductImportResult
    {
        public string FilePath { get; set; }
        public int TotalRowsRead { get; set; }
        public int ValidRowCount { get; set; }
        public int ErrorRowCount { get; set; }

        public List<ProductImportValidationError> Errors { get; set; }
        public List<ProductImportRowDto> ValidRows { get; set; }

        public bool CanImport
        {
            get { return Errors != null && Errors.Count == 0 && ValidRows != null && ValidRows.Count > 0; }
        }

        public ProductImportResult()
        {
            Errors = new List<ProductImportValidationError>();
            ValidRows = new List<ProductImportRowDto>();
        }

        public void AddStructuralError(string columnName, string message)
        {
            Errors.Add(new ProductImportValidationError(0, columnName, string.Empty, message));
        }

        public void AddRowError(int rowNumber, string columnName, string rawValue, string message)
        {
            Errors.Add(new ProductImportValidationError(rowNumber, columnName, rawValue, message));
        }
    }
}
