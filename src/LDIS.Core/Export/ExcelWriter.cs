using System;
using System.Collections.Generic;
using System.IO;
using ClosedXML.Excel;
using LDIS.Core.DTOs;

namespace LDIS.Core.Export
{
    /// <summary>
    /// Generates structured OpenXML (.xlsx) workbooks for products and transactions.
    /// Uses ClosedXML with real numeric and date cell types, styled headers, frozen pane, and auto-filters.
    /// </summary>
    public class ExcelWriter
    {
        private static readonly XLColor HeaderBackground = XLColor.FromArgb(31, 78, 120); // Professional navy blue
        private static readonly XLColor HeaderFontColor = XLColor.White;

        /// <summary>
        /// Writes product list to an Excel workbook on the specified stream.
        /// Excludes internal implementation fields (ItemID) and exports 12 user-facing columns.
        /// </summary>
        public void WriteProducts(IEnumerable<ItemListItemDto> items, Stream stream)
        {
            if (items == null) throw new ArgumentNullException("items");
            if (stream == null) throw new ArgumentNullException("stream");

            using (var workbook = new XLWorkbook())
            {
                var ws = workbook.Worksheets.Add("Products");

                // Headers (12 user-facing columns)
                string[] headers = new[]
                {
                    "SKU",
                    "Name",
                    "Category",
                    "Brand",
                    "Color",
                    "Size",
                    "Gender",
                    "Purchase Price",
                    "Selling Price",
                    "Min Stock",
                    "Current Stock",
                    "Status"
                };

                for (int col = 0; col < headers.Length; col++)
                {
                    ws.Cell(1, col + 1).SetValue(headers[col]);
                }

                // Data Rows
                int row = 2;
                foreach (var item in items)
                {
                    if (item == null) continue;

                    ws.Cell(row, 1).SetValue(item.SKU ?? string.Empty);
                    ws.Cell(row, 2).SetValue(item.Name ?? string.Empty);
                    ws.Cell(row, 3).SetValue(item.CategoryName ?? string.Empty);
                    ws.Cell(row, 4).SetValue(item.Brand ?? string.Empty);
                    ws.Cell(row, 5).SetValue(item.Color ?? string.Empty);
                    ws.Cell(row, 6).SetValue(item.Size ?? string.Empty);
                    ws.Cell(row, 7).SetValue(item.Gender ?? string.Empty);

                    // Numeric values as real numbers
                    ws.Cell(row, 8).SetValue(item.PurchasePrice);
                    ws.Cell(row, 8).Style.NumberFormat.Format = "#,##0";

                    ws.Cell(row, 9).SetValue(item.SellingPrice);
                    ws.Cell(row, 9).Style.NumberFormat.Format = "#,##0";

                    ws.Cell(row, 10).SetValue(item.MinStockLevel);
                    ws.Cell(row, 10).Style.NumberFormat.Format = "#,##0";

                    ws.Cell(row, 11).SetValue(item.CurrentStock);
                    ws.Cell(row, 11).Style.NumberFormat.Format = "#,##0";

                    ws.Cell(row, 12).SetValue(item.IsActive ? "Active" : "Inactive");

                    row++;
                }

                int lastRow = Math.Max(1, row - 1);

                // Styling
                ApplyHeaderStyle(ws, headers.Length);

                // Freeze Header Row
                ws.SheetView.FreezeRows(1);

                // Auto-Filter
                ws.Range(1, 1, lastRow, headers.Length).SetAutoFilter();

                // Auto-fit columns with sensible minimum width
                ApplyColumnWidths(ws, headers.Length);

                workbook.SaveAs(stream);
            }
        }

        /// <summary>
        /// Writes transaction list to an Excel workbook on the specified stream.
        /// Preserves the existing export model and signed quantity semantics exactly.
        /// </summary>
        public void WriteTransactions(IEnumerable<TransactionListItemDto> transactions, Stream stream)
        {
            if (transactions == null) throw new ArgumentNullException("transactions");
            if (stream == null) throw new ArgumentNullException("stream");

            using (var workbook = new XLWorkbook())
            {
                var ws = workbook.Worksheets.Add("Transactions");

                // Headers (12 user-facing columns matching transaction export model)
                string[] headers = new[]
                {
                    "Transaction ID",
                    "Date & Time",
                    "SKU",
                    "Item Name",
                    "Type",
                    "Quantity",
                    "Unit Price",
                    "Reference #",
                    "Supplier / Customer",
                    "Reason",
                    "Notes",
                    "Created By"
                };

                for (int col = 0; col < headers.Length; col++)
                {
                    ws.Cell(1, col + 1).SetValue(headers[col]);
                }

                // Data Rows
                int row = 2;
                foreach (var t in transactions)
                {
                    if (t == null) continue;

                    // Signed quantity derivation matching M3/M5 semantics
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

                    ws.Cell(row, 1).SetValue(t.TransactionID);
                    ws.Cell(row, 1).Style.NumberFormat.Format = "0";

                    // Real DateTime cell (Excel date system supports dates >= 1900-01-01)
                    if (t.TransactionDate >= new DateTime(1900, 1, 1))
                    {
                        ws.Cell(row, 2).SetValue(t.TransactionDate);
                        ws.Cell(row, 2).Style.NumberFormat.Format = "yyyy-MM-dd HH:mm:ss";
                    }
                    else
                    {
                        ws.Cell(row, 2).SetValue(string.Empty);
                    }

                    ws.Cell(row, 3).SetValue(t.SKU ?? string.Empty);
                    ws.Cell(row, 4).SetValue(t.ItemName ?? string.Empty);
                    ws.Cell(row, 5).SetValue(t.TransactionType ?? string.Empty);

                    // Signed Quantity as real number
                    ws.Cell(row, 6).SetValue(exportQuantity);
                    ws.Cell(row, 6).Style.NumberFormat.Format = "#,##0;[Red]-#,##0;0";

                    // Unit Price as real number
                    ws.Cell(row, 7).SetValue(t.UnitPrice);
                    ws.Cell(row, 7).Style.NumberFormat.Format = "#,##0";

                    ws.Cell(row, 8).SetValue(t.ReferenceNumber ?? string.Empty);
                    ws.Cell(row, 9).SetValue(t.SupplierOrCustomer ?? string.Empty);
                    ws.Cell(row, 10).SetValue(t.Reason ?? string.Empty);
                    ws.Cell(row, 11).SetValue(t.Notes ?? string.Empty);
                    ws.Cell(row, 12).SetValue(t.CreatedBy ?? string.Empty);

                    row++;
                }

                int lastRow = Math.Max(1, row - 1);

                // Styling
                ApplyHeaderStyle(ws, headers.Length);

                // Freeze Header Row
                ws.SheetView.FreezeRows(1);

                // Auto-Filter
                ws.Range(1, 1, lastRow, headers.Length).SetAutoFilter();

                // Auto-fit columns with sensible minimum width
                ApplyColumnWidths(ws, headers.Length);

                workbook.SaveAs(stream);
            }
        }

        private static void ApplyHeaderStyle(IXLWorksheet ws, int columnCount)
        {
            var headerRange = ws.Range(1, 1, 1, columnCount);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Font.FontColor = HeaderFontColor;
            headerRange.Style.Fill.BackgroundColor = HeaderBackground;
            headerRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            ws.Row(1).Height = 24;
        }

        private static void ApplyColumnWidths(IXLWorksheet ws, int columnCount)
        {
            ws.Columns(1, columnCount).AdjustToContents();
            for (int col = 1; col <= columnCount; col++)
            {
                var column = ws.Column(col);
                if (column.Width < 12)
                {
                    column.Width = 12;
                }
            }
        }
    }
}
