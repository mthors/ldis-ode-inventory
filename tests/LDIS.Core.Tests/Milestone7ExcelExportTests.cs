using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ClosedXML.Excel;
using LDIS.Core.DTOs;
using LDIS.Core.Services;

namespace LDIS.Core.Tests
{
    public class Milestone7ExcelExportTests
    {
        public void RunAllTests()
        {
            // Product Excel Export Tests
            RunTest("Test_ExportProducts_Excel_CreatesValidWorkbookAndHeaders", Test_ExportProducts_Excel_CreatesValidWorkbookAndHeaders);
            RunTest("Test_ExportProducts_Excel_NumericAndFormattedValues", Test_ExportProducts_Excel_NumericAndFormattedValues);
            RunTest("Test_ExportProducts_Excel_EmptyList_WritesHeadersOnly", Test_ExportProducts_Excel_EmptyList_WritesHeadersOnly);
            RunTest("Test_ExportProducts_Excel_ToFile_CreatesFileAndParentDirectory", Test_ExportProducts_Excel_ToFile_CreatesFileAndParentDirectory);
            RunTest("Test_ExportProducts_Excel_NullItemsSkippedSafely", Test_ExportProducts_Excel_NullItemsSkippedSafely);

            // Transaction Excel Export Tests
            RunTest("Test_ExportTransactions_Excel_CreatesValidWorkbookAndHeaders", Test_ExportTransactions_Excel_CreatesValidWorkbookAndHeaders);
            RunTest("Test_ExportTransactions_Excel_SignedQuantity_Derivation", Test_ExportTransactions_Excel_SignedQuantity_Derivation);
            RunTest("Test_ExportTransactions_Excel_DateTimeAndNumericCellTypes", Test_ExportTransactions_Excel_DateTimeAndNumericCellTypes);
            RunTest("Test_ExportTransactions_Excel_EmptyList_WritesHeadersOnly", Test_ExportTransactions_Excel_EmptyList_WritesHeadersOnly);
            RunTest("Test_ExportTransactions_Excel_ToFile_CreatesFileAndParentDirectory", Test_ExportTransactions_Excel_ToFile_CreatesFileAndParentDirectory);
            RunTest("Test_ExportTransactions_Excel_NullItemsSkippedSafely", Test_ExportTransactions_Excel_NullItemsSkippedSafely);
            RunTest("Test_ExportTransactions_Excel_UnassignedDate_HandledSafely", Test_ExportTransactions_Excel_UnassignedDate_HandledSafely);

            // Argument Validation Tests
            RunTest("Test_ExportService_ArgumentValidation_Products", Test_ExportService_ArgumentValidation_Products);
            RunTest("Test_ExportService_ArgumentValidation_Transactions", Test_ExportService_ArgumentValidation_Transactions);
        }

        private static void RunTest(string testName, Action testAction)
        {
            try
            {
                testAction();
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("  PASS: " + testName);
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("  FAIL: " + testName);
                Console.WriteLine("        " + ex.Message);
                Console.ResetColor();
                throw;
            }
        }

        private static void Assert(bool condition, string message)
        {
            if (!condition)
            {
                throw new Exception("Assertion Failed: " + message);
            }
        }

        // ==========================================
        // Product Excel Export Tests
        // ==========================================

        private void Test_ExportProducts_Excel_CreatesValidWorkbookAndHeaders()
        {
            var service = new ExportService();
            var items = new List<ItemListItemDto>
            {
                new ItemListItemDto
                {
                    ItemID = 101, // Internal ID must be excluded
                    SKU = "DRS-001",
                    Name = "Silk Evening Dress",
                    CategoryName = "Dresses",
                    Brand = "Zara",
                    Color = "Navy",
                    Size = "M",
                    Gender = "Women",
                    PurchasePrice = 250000,
                    SellingPrice = 450000,
                    MinStockLevel = 5,
                    CurrentStock = 15,
                    IsActive = true
                }
            };

            using (var ms = new MemoryStream())
            {
                service.ExportProductsToExcel(items, ms);
                ms.Position = 0;

                using (var workbook = new XLWorkbook(ms))
                {
                    Assert(workbook.Worksheets.Count == 1, "Expected exactly 1 worksheet.");
                    var ws = workbook.Worksheet(1);
                    Assert(ws.Name == "Products", "Expected worksheet name 'Products', got: " + ws.Name);

                    string[] expectedHeaders = new[]
                    {
                        "SKU", "Name", "Category", "Brand", "Color", "Size",
                        "Gender", "Purchase Price", "Selling Price", "Min Stock", "Current Stock", "Status"
                    };

                    Assert(ws.LastColumnUsed().ColumnNumber() == 12, "Expected 12 columns.");
                    for (int i = 0; i < expectedHeaders.Length; i++)
                    {
                        string headerVal = ws.Cell(1, i + 1).GetString();
                        Assert(headerVal == expectedHeaders[i], string.Format("Column {0} header expected '{1}' but got '{2}'", i + 1, expectedHeaders[i], headerVal));
                    }

                    // Verify ItemID is NOT in any header
                    for (int col = 1; col <= 12; col++)
                    {
                        Assert(!ws.Cell(1, col).GetString().Equals("ItemID", StringComparison.OrdinalIgnoreCase), "ItemID must be excluded from product Excel headers.");
                    }

                    // Verify Header Styling
                    var headerCell = ws.Cell(1, 1);
                    Assert(headerCell.Style.Font.Bold, "Header font must be bold.");
                    Assert(headerCell.Style.Font.FontColor == XLColor.White, "Header font color must be white.");
                    Assert(headerCell.Style.Fill.BackgroundColor == XLColor.FromArgb(31, 78, 120), "Header background must be navy blue.");

                    // Verify AutoFilter is set
                    Assert(ws.AutoFilter != null && ws.AutoFilter.IsEnabled, "AutoFilter must be enabled on product worksheet.");
                }
            }
        }

        private void Test_ExportProducts_Excel_NumericAndFormattedValues()
        {
            var service = new ExportService();
            var items = new List<ItemListItemDto>
            {
                new ItemListItemDto
                {
                    ItemID = 1,
                    SKU = "TSH-001",
                    Name = "Basic Tee",
                    CategoryName = "Tops",
                    Brand = "Uniqlo",
                    Color = "White",
                    Size = "L",
                    Gender = "Unisex",
                    PurchasePrice = 75000,
                    SellingPrice = 129000,
                    MinStockLevel = 10,
                    CurrentStock = 42,
                    IsActive = true
                },
                new ItemListItemDto
                {
                    ItemID = 2,
                    SKU = "JNS-002",
                    Name = "Slim Jeans",
                    CategoryName = "Bottoms",
                    Brand = "Levi's",
                    Color = "Blue",
                    Size = "32",
                    Gender = "Men",
                    PurchasePrice = 300000,
                    SellingPrice = 550000,
                    MinStockLevel = 3,
                    CurrentStock = 0,
                    IsActive = false
                }
            };

            using (var ms = new MemoryStream())
            {
                service.ExportProductsToExcel(items, ms);
                ms.Position = 0;

                using (var workbook = new XLWorkbook(ms))
                {
                    var ws = workbook.Worksheet("Products");

                    // Row 2 (TSH-001)
                    Assert(ws.Cell(2, 1).GetString() == "TSH-001", "Row 2 SKU mismatch.");
                    Assert(ws.Cell(2, 8).DataType == XLDataType.Number, "PurchasePrice must be numeric cell.");
                    Assert(ws.Cell(2, 8).GetDouble() == 75000, "PurchasePrice value mismatch.");
                    Assert(ws.Cell(2, 8).Style.NumberFormat.Format == "#,##0", "PurchasePrice number format must be #,##0.");

                    Assert(ws.Cell(2, 9).DataType == XLDataType.Number, "SellingPrice must be numeric cell.");
                    Assert(ws.Cell(2, 9).GetDouble() == 129000, "SellingPrice value mismatch.");

                    Assert(ws.Cell(2, 10).GetDouble() == 10, "MinStockLevel value mismatch.");
                    Assert(ws.Cell(2, 11).GetDouble() == 42, "CurrentStock value mismatch.");
                    Assert(ws.Cell(2, 12).GetString() == "Active", "Active product status must be 'Active'.");

                    // Row 3 (JNS-002 - Inactive)
                    Assert(ws.Cell(3, 1).GetString() == "JNS-002", "Row 3 SKU mismatch.");
                    Assert(ws.Cell(3, 11).GetDouble() == 0, "Zero stock must be stored as 0.");
                    Assert(ws.Cell(3, 12).GetString() == "Inactive", "Inactive product status must be 'Inactive'.");
                }
            }
        }

        private void Test_ExportProducts_Excel_EmptyList_WritesHeadersOnly()
        {
            var service = new ExportService();
            using (var ms = new MemoryStream())
            {
                service.ExportProductsToExcel(new List<ItemListItemDto>(), ms);
                ms.Position = 0;

                using (var workbook = new XLWorkbook(ms))
                {
                    var ws = workbook.Worksheet("Products");
                    Assert(ws.RowCount() >= 1, "Worksheet should contain header row.");
                    Assert(ws.Cell(1, 1).GetString() == "SKU", "Header row must exist even for empty list.");
                    Assert(ws.Cell(2, 1).IsEmpty(), "Data row must be empty when collection is empty.");
                }
            }
        }

        private void Test_ExportProducts_Excel_ToFile_CreatesFileAndParentDirectory()
        {
            var service = new ExportService();
            string tempDir = Path.Combine(Path.GetTempPath(), "LDIS_M7_ExportTest_" + Guid.NewGuid().ToString("N"), "SubFolder");
            string filePath = Path.Combine(tempDir, "products.xlsx");

            try
            {
                var items = new List<ItemListItemDto>
                {
                    new ItemListItemDto { ItemID = 1, SKU = "TST-01", Name = "File Test", IsActive = true }
                };

                service.ExportProductsToExcel(items, filePath);

                Assert(File.Exists(filePath), "Exported Excel file must exist on disk.");
                var fileInfo = new FileInfo(filePath);
                Assert(fileInfo.Length > 0, "Exported file must not be 0 bytes.");

                using (var workbook = new XLWorkbook(filePath))
                {
                    var ws = workbook.Worksheet("Products");
                    Assert(ws.Cell(2, 1).GetString() == "TST-01", "Data in written file mismatch.");
                }
            }
            finally
            {
                try
                {
                    string parentDir = Directory.GetParent(tempDir).FullName;
                    if (Directory.Exists(parentDir))
                    {
                        Directory.Delete(parentDir, true);
                    }
                }
                catch { }
            }
        }

        private void Test_ExportProducts_Excel_NullItemsSkippedSafely()
        {
            var service = new ExportService();
            var items = new List<ItemListItemDto>
            {
                new ItemListItemDto { ItemID = 1, SKU = "VALID-1", Name = "Valid 1", IsActive = true },
                null,
                new ItemListItemDto { ItemID = 2, SKU = "VALID-2", Name = "Valid 2", IsActive = true }
            };

            using (var ms = new MemoryStream())
            {
                service.ExportProductsToExcel(items, ms);
                ms.Position = 0;

                using (var workbook = new XLWorkbook(ms))
                {
                    var ws = workbook.Worksheet("Products");
                    Assert(ws.Cell(2, 1).GetString() == "VALID-1", "Row 2 must be VALID-1.");
                    Assert(ws.Cell(3, 1).GetString() == "VALID-2", "Row 3 must be VALID-2 (null skipped).");
                    Assert(ws.Cell(4, 1).IsEmpty(), "Row 4 must be empty.");
                }
            }
        }

        // ==========================================
        // Transaction Excel Export Tests
        // ==========================================

        private void Test_ExportTransactions_Excel_CreatesValidWorkbookAndHeaders()
        {
            var service = new ExportService();
            var txs = new List<TransactionListItemDto>
            {
                new TransactionListItemDto
                {
                    TransactionID = 501,
                    TransactionDate = new DateTime(2026, 9, 27, 14, 30, 0),
                    ItemID = 10, // Internal ID must be excluded
                    SKU = "SHO-001",
                    ItemName = "Running Shoes",
                    TransactionType = "IN",
                    Quantity = 20,
                    UnitPrice = 450000,
                    ReferenceNumber = "PO-2026-001",
                    SupplierOrCustomer = "Nike Supplier",
                    Reason = "Initial inventory",
                    Notes = "Batch A",
                    CreatedBy = "Admin"
                }
            };

            using (var ms = new MemoryStream())
            {
                service.ExportTransactionsToExcel(txs, ms);
                ms.Position = 0;

                using (var workbook = new XLWorkbook(ms))
                {
                    Assert(workbook.Worksheets.Count == 1, "Expected exactly 1 worksheet.");
                    var ws = workbook.Worksheet(1);
                    Assert(ws.Name == "Transactions", "Expected worksheet name 'Transactions', got: " + ws.Name);

                    string[] expectedHeaders = new[]
                    {
                        "Transaction ID", "Date & Time", "SKU", "Item Name", "Type",
                        "Quantity", "Unit Price", "Reference #", "Supplier / Customer",
                        "Reason", "Notes", "Created By"
                    };

                    Assert(ws.LastColumnUsed().ColumnNumber() == 12, "Expected 12 columns.");
                    for (int i = 0; i < expectedHeaders.Length; i++)
                    {
                        string headerVal = ws.Cell(1, i + 1).GetString();
                        Assert(headerVal == expectedHeaders[i], string.Format("Column {0} header expected '{1}' but got '{2}'", i + 1, expectedHeaders[i], headerVal));
                    }

                    // Verify ItemID is NOT in any header
                    for (int col = 1; col <= 12; col++)
                    {
                        Assert(!ws.Cell(1, col).GetString().Equals("ItemID", StringComparison.OrdinalIgnoreCase), "ItemID must be excluded from transaction Excel headers.");
                    }

                    // Verify Header Styling & AutoFilter
                    Assert(ws.Cell(1, 1).Style.Font.Bold, "Header font must be bold.");
                    Assert(ws.AutoFilter != null && ws.AutoFilter.IsEnabled, "AutoFilter must be enabled on transaction worksheet.");
                }
            }
        }

        private void Test_ExportTransactions_Excel_SignedQuantity_Derivation()
        {
            var service = new ExportService();
            var txs = new List<TransactionListItemDto>
            {
                new TransactionListItemDto
                {
                    TransactionID = 1,
                    TransactionDate = DateTime.Now,
                    SKU = "ITEM-1",
                    TransactionType = "IN",
                    Quantity = 25 // Should export as +25
                },
                new TransactionListItemDto
                {
                    TransactionID = 2,
                    TransactionDate = DateTime.Now,
                    SKU = "ITEM-2",
                    TransactionType = "OUT",
                    Quantity = 10 // Stored as positive in DB, should export as -10
                },
                new TransactionListItemDto
                {
                    TransactionID = 3,
                    TransactionDate = DateTime.Now,
                    SKU = "ITEM-3",
                    TransactionType = "ADJUSTMENT",
                    Quantity = 5 // Positive adjustment should export as +5
                },
                new TransactionListItemDto
                {
                    TransactionID = 4,
                    TransactionDate = DateTime.Now,
                    SKU = "ITEM-4",
                    TransactionType = "ADJUSTMENT",
                    Quantity = -4 // Negative adjustment should export as -4
                }
            };

            using (var ms = new MemoryStream())
            {
                service.ExportTransactionsToExcel(txs, ms);
                ms.Position = 0;

                using (var workbook = new XLWorkbook(ms))
                {
                    var ws = workbook.Worksheet("Transactions");

                    // IN: +25
                    Assert(ws.Cell(2, 6).DataType == XLDataType.Number, "IN Quantity must be numeric.");
                    Assert(ws.Cell(2, 6).GetDouble() == 25, "IN Quantity must be positive 25.");

                    // OUT: -10
                    Assert(ws.Cell(3, 6).DataType == XLDataType.Number, "OUT Quantity must be numeric.");
                    Assert(ws.Cell(3, 6).GetDouble() == -10, "OUT Quantity must be negative -10.");

                    // ADJUSTMENT (+5): +5
                    Assert(ws.Cell(4, 6).GetDouble() == 5, "Positive adjustment Quantity must be 5.");

                    // ADJUSTMENT (-4): -4
                    Assert(ws.Cell(5, 6).GetDouble() == -4, "Negative adjustment Quantity must be -4.");
                }
            }
        }

        private void Test_ExportTransactions_Excel_DateTimeAndNumericCellTypes()
        {
            var testDate = new DateTime(2026, 9, 27, 16, 45, 12);
            var service = new ExportService();
            var txs = new List<TransactionListItemDto>
            {
                new TransactionListItemDto
                {
                    TransactionID = 999,
                    TransactionDate = testDate,
                    SKU = "SKU-DT",
                    TransactionType = "IN",
                    Quantity = 1,
                    UnitPrice = 175000
                }
            };

            using (var ms = new MemoryStream())
            {
                service.ExportTransactionsToExcel(txs, ms);
                ms.Position = 0;

                using (var workbook = new XLWorkbook(ms))
                {
                    var ws = workbook.Worksheet("Transactions");

                    // TransactionID is numeric
                    Assert(ws.Cell(2, 1).DataType == XLDataType.Number, "TransactionID must be numeric cell.");
                    Assert(ws.Cell(2, 1).GetDouble() == 999, "TransactionID value mismatch.");

                    // TransactionDate is DateTime
                    Assert(ws.Cell(2, 2).DataType == XLDataType.DateTime, "TransactionDate must be DateTime cell.");
                    Assert(ws.Cell(2, 2).GetDateTime() == testDate, "TransactionDate value mismatch.");
                    Assert(ws.Cell(2, 2).Style.NumberFormat.Format == "yyyy-MM-dd HH:mm:ss", "TransactionDate format must be yyyy-MM-dd HH:mm:ss.");

                    // UnitPrice is numeric
                    Assert(ws.Cell(2, 7).DataType == XLDataType.Number, "UnitPrice must be numeric cell.");
                    Assert(ws.Cell(2, 7).GetDouble() == 175000, "UnitPrice value mismatch.");
                }
            }
        }

        private void Test_ExportTransactions_Excel_EmptyList_WritesHeadersOnly()
        {
            var service = new ExportService();
            using (var ms = new MemoryStream())
            {
                service.ExportTransactionsToExcel(new List<TransactionListItemDto>(), ms);
                ms.Position = 0;

                using (var workbook = new XLWorkbook(ms))
                {
                    var ws = workbook.Worksheet("Transactions");
                    Assert(ws.RowCount() >= 1, "Worksheet should contain header row.");
                    Assert(ws.Cell(1, 1).GetString() == "Transaction ID", "Header row must exist even for empty transactions.");
                    Assert(ws.Cell(2, 1).IsEmpty(), "Data row must be empty when transaction list is empty.");
                }
            }
        }

        private void Test_ExportTransactions_Excel_ToFile_CreatesFileAndParentDirectory()
        {
            var service = new ExportService();
            string tempDir = Path.Combine(Path.GetTempPath(), "LDIS_M7_TxExportTest_" + Guid.NewGuid().ToString("N"), "TxSub");
            string filePath = Path.Combine(tempDir, "transactions.xlsx");

            try
            {
                var txs = new List<TransactionListItemDto>
                {
                    new TransactionListItemDto { TransactionID = 1, TransactionDate = new DateTime(2026, 9, 27, 10, 0, 0), SKU = "TX-01", Quantity = 1, TransactionType = "IN" }
                };

                service.ExportTransactionsToExcel(txs, filePath);

                Assert(File.Exists(filePath), "Exported transaction Excel file must exist on disk.");
                var fileInfo = new FileInfo(filePath);
                Assert(fileInfo.Length > 0, "Exported file must not be 0 bytes.");

                using (var workbook = new XLWorkbook(filePath))
                {
                    var ws = workbook.Worksheet("Transactions");
                    Assert(ws.Cell(2, 3).GetString() == "TX-01", "Data in written file mismatch.");
                }
            }
            finally
            {
                try
                {
                    string parentDir = Directory.GetParent(tempDir).FullName;
                    if (Directory.Exists(parentDir))
                    {
                        Directory.Delete(parentDir, true);
                    }
                }
                catch { }
            }
        }

        private void Test_ExportTransactions_Excel_NullItemsSkippedSafely()
        {
            var service = new ExportService();
            var txs = new List<TransactionListItemDto>
            {
                new TransactionListItemDto { TransactionID = 1, SKU = "TX-A", Quantity = 5, TransactionType = "IN" },
                null,
                new TransactionListItemDto { TransactionID = 2, SKU = "TX-B", Quantity = 3, TransactionType = "OUT" }
            };

            using (var ms = new MemoryStream())
            {
                service.ExportTransactionsToExcel(txs, ms);
                ms.Position = 0;

                using (var workbook = new XLWorkbook(ms))
                {
                    var ws = workbook.Worksheet("Transactions");
                    Assert(ws.Cell(2, 3).GetString() == "TX-A", "Row 2 must be TX-A.");
                    Assert(ws.Cell(3, 3).GetString() == "TX-B", "Row 3 must be TX-B (null skipped).");
                    Assert(ws.Cell(4, 1).IsEmpty(), "Row 4 must be empty.");
                }
            }
        }

        private void Test_ExportTransactions_Excel_UnassignedDate_HandledSafely()
        {
            var service = new ExportService();
            var txs = new List<TransactionListItemDto>
            {
                new TransactionListItemDto
                {
                    TransactionID = 1,
                    // TransactionDate defaults to DateTime.MinValue
                    SKU = "NO-DATE",
                    Quantity = 2,
                    TransactionType = "IN"
                }
            };

            using (var ms = new MemoryStream())
            {
                service.ExportTransactionsToExcel(txs, ms);
                ms.Position = 0;

                using (var workbook = new XLWorkbook(ms))
                {
                    var ws = workbook.Worksheet("Transactions");
                    Assert(ws.Cell(2, 3).GetString() == "NO-DATE", "Row 2 must be NO-DATE.");
                    Assert(ws.Cell(2, 2).GetString() == string.Empty, "Date cell for unassigned date must be empty.");
                }
            }
        }

        // ==========================================
        // Argument Validation Tests
        // ==========================================

        private void Test_ExportService_ArgumentValidation_Products()
        {
            var service = new ExportService();

            bool threwNullItems = false;
            try
            {
                service.ExportProductsToExcel(null, new MemoryStream());
            }
            catch (ArgumentNullException)
            {
                threwNullItems = true;
            }
            Assert(threwNullItems, "Null items must throw ArgumentNullException.");

            bool threwNullStream = false;
            try
            {
                service.ExportProductsToExcel(new List<ItemListItemDto>(), (Stream)null);
            }
            catch (ArgumentNullException)
            {
                threwNullStream = true;
            }
            Assert(threwNullStream, "Null stream must throw ArgumentNullException.");

            bool threwNullPath = false;
            try
            {
                service.ExportProductsToExcel(new List<ItemListItemDto>(), (string)null);
            }
            catch (ArgumentException)
            {
                threwNullPath = true;
            }
            Assert(threwNullPath, "Null path must throw ArgumentException.");

            bool threwWhitespacePath = false;
            try
            {
                service.ExportProductsToExcel(new List<ItemListItemDto>(), "   ");
            }
            catch (ArgumentException)
            {
                threwWhitespacePath = true;
            }
            Assert(threwWhitespacePath, "Whitespace path must throw ArgumentException.");
        }

        private void Test_ExportService_ArgumentValidation_Transactions()
        {
            var service = new ExportService();

            bool threwNullTxs = false;
            try
            {
                service.ExportTransactionsToExcel(null, new MemoryStream());
            }
            catch (ArgumentNullException)
            {
                threwNullTxs = true;
            }
            Assert(threwNullTxs, "Null transactions must throw ArgumentNullException.");

            bool threwNullStream = false;
            try
            {
                service.ExportTransactionsToExcel(new List<TransactionListItemDto>(), (Stream)null);
            }
            catch (ArgumentNullException)
            {
                threwNullStream = true;
            }
            Assert(threwNullStream, "Null stream must throw ArgumentNullException.");

            bool threwNullPath = false;
            try
            {
                service.ExportTransactionsToExcel(new List<TransactionListItemDto>(), (string)null);
            }
            catch (ArgumentException)
            {
                threwNullPath = true;
            }
            Assert(threwNullPath, "Null path must throw ArgumentException.");

            bool threwWhitespacePath = false;
            try
            {
                service.ExportTransactionsToExcel(new List<TransactionListItemDto>(), "   ");
            }
            catch (ArgumentException)
            {
                threwWhitespacePath = true;
            }
            Assert(threwWhitespacePath, "Whitespace path must throw ArgumentException.");
        }
    }
}
