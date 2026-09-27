using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using LDIS.Core.DTOs;
using LDIS.Core.Export;

namespace LDIS.Core.Services
{
    public class ExportService : IExportService
    {
        private static readonly Encoding Utf8WithBom = new UTF8Encoding(true);

        public void ExportProductsToCsv(IEnumerable<ItemListItemDto> items, TextWriter writer)
        {
            if (items == null) throw new ArgumentNullException("items");
            if (writer == null) throw new ArgumentNullException("writer");

            var csv = new CsvWriter(writer);

            // Write Header
            csv.WriteRow(
                "ItemID",
                "SKU",
                "Name",
                "Category",
                "Brand",
                "Color",
                "Size",
                "Gender",
                "PurchasePrice",
                "SellingPrice",
                "MinStockLevel",
                "CurrentStock",
                "IsActive"
            );

            // Write Data Rows
            foreach (var item in items)
            {
                if (item == null) continue;

                csv.WriteRow(
                    item.ItemID.ToString(CultureInfo.InvariantCulture),
                    item.SKU ?? string.Empty,
                    item.Name ?? string.Empty,
                    item.CategoryName ?? string.Empty,
                    item.Brand ?? string.Empty,
                    item.Color ?? string.Empty,
                    item.Size ?? string.Empty,
                    item.Gender ?? string.Empty,
                    item.PurchasePrice.ToString(CultureInfo.InvariantCulture),
                    item.SellingPrice.ToString(CultureInfo.InvariantCulture),
                    item.MinStockLevel.ToString(CultureInfo.InvariantCulture),
                    item.CurrentStock.ToString(CultureInfo.InvariantCulture),
                    item.IsActive ? "Active" : "Inactive"
                );
            }

            csv.Flush();
        }

        public void ExportProductsToCsv(IEnumerable<ItemListItemDto> items, string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("File path cannot be null or empty.", "filePath");

            string dir = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            using (var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.Read))
            using (var writer = new StreamWriter(stream, Utf8WithBom))
            {
                ExportProductsToCsv(items, writer);
            }
        }

        public void ExportTransactionsToCsv(IEnumerable<TransactionListItemDto> transactions, TextWriter writer)
        {
            if (transactions == null) throw new ArgumentNullException("transactions");
            if (writer == null) throw new ArgumentNullException("writer");

            var csv = new CsvWriter(writer);

            // Write Header
            csv.WriteRow(
                "TransactionID",
                "TransactionDate",
                "ItemID",
                "SKU",
                "ItemName",
                "TransactionType",
                "Quantity",
                "UnitPrice",
                "ReferenceNumber",
                "SupplierOrCustomer",
                "Reason",
                "Notes",
                "CreatedBy"
            );

            // Write Data Rows
            foreach (var t in transactions)
            {
                if (t == null) continue;

                // Explicitly derive signed stock delta according to M3 storage semantics:
                // IN: positive stock increase (+Quantity)
                // OUT: negative stock reduction (-Quantity), noting that stored DB Quantity is positive
                // ADJUSTMENT: signed stock delta as already established in M3
                int exportQuantity;
                string opType = (t.TransactionType ?? string.Empty).Trim().ToUpperInvariant();
                if (opType == "OUT")
                {
                    exportQuantity = -Math.Abs(t.Quantity);
                }
                else if (opType == "IN")
                {
                    exportQuantity = Math.Abs(t.Quantity);
                }
                else // ADJUSTMENT
                {
                    exportQuantity = t.Quantity;
                }

                csv.WriteRow(
                    t.TransactionID.ToString(CultureInfo.InvariantCulture),
                    t.TransactionDate.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture),
                    t.ItemID.ToString(CultureInfo.InvariantCulture),
                    t.SKU ?? string.Empty,
                    t.ItemName ?? string.Empty,
                    t.TransactionType ?? string.Empty,
                    exportQuantity.ToString(CultureInfo.InvariantCulture),
                    t.UnitPrice.ToString(CultureInfo.InvariantCulture),
                    t.ReferenceNumber ?? string.Empty,
                    t.SupplierOrCustomer ?? string.Empty,
                    t.Reason ?? string.Empty,
                    t.Notes ?? string.Empty,
                    t.CreatedBy ?? string.Empty
                );
            }

            csv.Flush();
        }

        public void ExportTransactionsToCsv(IEnumerable<TransactionListItemDto> transactions, string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("File path cannot be null or empty.", "filePath");

            string dir = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            using (var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.Read))
            using (var writer = new StreamWriter(stream, Utf8WithBom))
            {
                ExportTransactionsToCsv(transactions, writer);
            }
        }

        public void ExportProductsToExcel(IEnumerable<ItemListItemDto> items, Stream stream)
        {
            if (items == null) throw new ArgumentNullException("items");
            if (stream == null) throw new ArgumentNullException("stream");

            var excel = new ExcelWriter();
            excel.WriteProducts(items, stream);
        }

        public void ExportProductsToExcel(IEnumerable<ItemListItemDto> items, string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("File path cannot be null or empty.", "filePath");

            string dir = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            using (var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.Read))
            {
                ExportProductsToExcel(items, stream);
            }
        }

        public void ExportTransactionsToExcel(IEnumerable<TransactionListItemDto> transactions, Stream stream)
        {
            if (transactions == null) throw new ArgumentNullException("transactions");
            if (stream == null) throw new ArgumentNullException("stream");

            var excel = new ExcelWriter();
            excel.WriteTransactions(transactions, stream);
        }

        public void ExportTransactionsToExcel(IEnumerable<TransactionListItemDto> transactions, string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("File path cannot be null or empty.", "filePath");

            string dir = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            using (var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.Read))
            {
                ExportTransactionsToExcel(transactions, stream);
            }
        }
    }
}
