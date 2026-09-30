using System.Collections.Generic;
using System.IO;
using LDIS.Core.DTOs;

namespace LDIS.Core.Services
{
    /// <summary>
    /// Service for validating and importing product master catalogues from Excel workbooks.
    /// </summary>
    public interface IProductImportService
    {
        /// <summary>
        /// Validates an Excel workbook file against product business rules and existing database constraints.
        /// Performs zero database modifications.
        /// </summary>
        ProductImportResult ValidateWorkbook(string filePath);

        /// <summary>
        /// Validates an Excel workbook stream against product business rules and existing database constraints.
        /// Performs zero database modifications.
        /// </summary>
        ProductImportResult ValidateWorkbook(Stream stream);

        /// <summary>
        /// Atomically inserts all valid product rows into the database within a single transaction.
        /// Sets CurrentStock = 0 and IsActive = true for all imported products.
        /// </summary>
        int ExecuteImport(IEnumerable<ProductImportRowDto> validRows);

        /// <summary>
        /// Generates the standard LDIS product import template (.xlsx) file.
        /// </summary>
        void GenerateTemplate(string filePath);

        /// <summary>
        /// Generates the standard LDIS product import template (.xlsx) into the provided stream.
        /// </summary>
        void GenerateTemplate(Stream stream);
    }
}
