using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using LDIS.Core.Data;
using LDIS.Core.Data.Repositories;
using LDIS.Core.DTOs;
using LDIS.Core.Export;
using LDIS.Core.Models;
using LDIS.Core.Services;

namespace LDIS.Core.Tests
{
    public class Milestone5ExportAndBackupTests
    {
        private static string CreateTempDbPath()
        {
            string tempDir = Path.Combine(Path.GetTempPath(), "LDIS_M5Tests_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(tempDir);
            return Path.Combine(tempDir, "test_m5.db");
        }

        private static void CleanupTempDir(string dirPath)
        {
            try
            {
                if (Directory.Exists(dirPath))
                {
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                    Directory.Delete(dirPath, true);
                }
            }
            catch
            {
                // Best effort cleanup in temp directory
            }
        }

        private static DbConnectionFactory CreateTestDatabase(out string dbPath)
        {
            dbPath = CreateTempDbPath();
            var config = new DatabaseConfig(dbPath);
            var factory = new DbConnectionFactory(config);
            var initializer = new DatabaseInitializer(factory);
            initializer.Initialize();
            return factory;
        }

        public void RunAllTests()
        {
            // CSV Writer tests
            RunTest("Test_CsvWriter_EscapesCommas", Test_CsvWriter_EscapesCommas);
            RunTest("Test_CsvWriter_EscapesQuotes", Test_CsvWriter_EscapesQuotes);
            RunTest("Test_CsvWriter_EscapesNewlines", Test_CsvWriter_EscapesNewlines);
            RunTest("Test_CsvWriter_EscapesWhitespaceWrapping", Test_CsvWriter_EscapesWhitespaceWrapping);
            RunTest("Test_CsvWriter_HandlesUnicodeAndAccents", Test_CsvWriter_HandlesUnicodeAndAccents);
            RunTest("Test_CsvWriter_InvariantNumericFormatting", Test_CsvWriter_InvariantNumericFormatting);
            RunTest("Test_CsvWriter_DateFormatting", Test_CsvWriter_DateFormatting);

            // Product Export tests
            RunTest("Test_ExportProducts_WritesCorrectHeaders", Test_ExportProducts_WritesCorrectHeaders);
            RunTest("Test_ExportProducts_WritesRowsCorrectly", Test_ExportProducts_WritesRowsCorrectly);
            RunTest("Test_ExportProducts_FormatsPricesAsIntegers", Test_ExportProducts_FormatsPricesAsIntegers);
            RunTest("Test_ExportProducts_IsActiveRepresentation", Test_ExportProducts_IsActiveRepresentation);
            RunTest("Test_ExportProducts_EmptyList_WritesHeaderOnly", Test_ExportProducts_EmptyList_WritesHeaderOnly);
            RunTest("Test_ExportProducts_ToFile_WithUtf8Bom", Test_ExportProducts_ToFile_WithUtf8Bom);

            // Transaction Export tests
            RunTest("Test_ExportTransactions_WritesCorrectHeaders", Test_ExportTransactions_WritesCorrectHeaders);
            RunTest("Test_ExportTransactions_SignedQuantity_Derivation", Test_ExportTransactions_SignedQuantity_Derivation);
            RunTest("Test_ExportTransactions_EscapingInReasonAndNotes", Test_ExportTransactions_EscapingInReasonAndNotes);
            RunTest("Test_ExportTransactions_EmptyList_WritesHeaderOnly", Test_ExportTransactions_EmptyList_WritesHeaderOnly);
            RunTest("Test_ExportTransactions_ToFile_WithUtf8Bom", Test_ExportTransactions_ToFile_WithUtf8Bom);

            // Database Backup tests
            RunTest("Test_BackupDatabase_CreatesValidSqliteDatabase", Test_BackupDatabase_CreatesValidSqliteDatabase);
            RunTest("Test_BackupDatabase_WhileActiveConnectionOpen", Test_BackupDatabase_WhileActiveConnectionOpen);
            RunTest("Test_BackupDatabase_DestinationSameAsSource_ThrowsException", Test_BackupDatabase_DestinationSameAsSource_ThrowsException);
            RunTest("Test_BackupDatabase_EmptyOrNullPath_ThrowsException", Test_BackupDatabase_EmptyOrNullPath_ThrowsException);
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
                throw new Exception("Assertion failed: " + message);
            }
        }

        // ==========================================
        // CSV Writer Unit Tests
        // ==========================================

        private void Test_CsvWriter_EscapesCommas()
        {
            var sb = new StringBuilder();
            using (var writer = new StringWriter(sb))
            {
                var csv = new CsvWriter(writer);
                csv.WriteRow("Nike, Inc.", "Normal");
            }

            string result = sb.ToString();
            Assert(result.Contains("\"Nike, Inc.\",Normal\r\n"), "Field containing comma must be quoted.");
        }

        private void Test_CsvWriter_EscapesQuotes()
        {
            var sb = new StringBuilder();
            using (var writer = new StringWriter(sb))
            {
                var csv = new CsvWriter(writer);
                csv.WriteRow("Sneakers 10\"", "Shoe");
            }

            string result = sb.ToString();
            Assert(result.Contains("\"Sneakers 10\"\"\",Shoe\r\n"), "Field containing quotes must be quoted with internal quotes doubled.");
        }

        private void Test_CsvWriter_EscapesNewlines()
        {
            var sb = new StringBuilder();
            using (var writer = new StringWriter(sb))
            {
                var csv = new CsvWriter(writer);
                csv.WriteRow("Line1\nLine2", "Normal");
            }

            string result = sb.ToString();
            Assert(result.Contains("\"Line1\nLine2\",Normal\r\n"), "Field containing newline must be quoted.");
        }

        private void Test_CsvWriter_EscapesWhitespaceWrapping()
        {
            var sb = new StringBuilder();
            using (var writer = new StringWriter(sb))
            {
                var csv = new CsvWriter(writer);
                csv.WriteRow(" LeadingSpace", "TrailingSpace ", " Clean ");
            }

            string result = sb.ToString();
            Assert(result.Contains("\" LeadingSpace\",\"TrailingSpace \",\" Clean \"\r\n"), "Fields with leading or trailing whitespace must be quoted.");
        }

        private void Test_CsvWriter_HandlesUnicodeAndAccents()
        {
            var sb = new StringBuilder();
            using (var writer = new StringWriter(sb))
            {
                var csv = new CsvWriter(writer);
                csv.WriteRow("Kaos Kaki Élégant", "Rp 50.000", "Batik Cirebon — Merah");
            }

            string result = sb.ToString();
            Assert(result.Contains("Kaos Kaki Élégant,Rp 50.000,Batik Cirebon — Merah\r\n"), "Unicode characters must be preserved faithfully.");
        }

        private void Test_CsvWriter_InvariantNumericFormatting()
        {
            string formattedInt = CsvWriter.FormatNumber(150000);
            string formattedLong = CsvWriter.FormatNumber((long)250000);

            Assert(formattedInt == "150000", "Integer formatting must be invariant without commas or periods.");
            Assert(formattedLong == "250000", "Long formatting must be invariant without commas or periods.");
        }

        private void Test_CsvWriter_DateFormatting()
        {
            var dt = new DateTime(2026, 9, 27, 14, 30, 45);
            string formatted = CsvWriter.FormatDate(dt);
            Assert(formatted == "2026-09-27 14:30:45", "Date formatting must match yyyy-MM-dd HH:mm:ss.");
        }

        // ==========================================
        // Product Export Tests
        // ==========================================

        private void Test_ExportProducts_WritesCorrectHeaders()
        {
            var service = new ExportService();
            var sb = new StringBuilder();
            using (var writer = new StringWriter(sb))
            {
                service.ExportProductsToCsv(new List<ItemListItemDto>(), writer);
            }

            string[] lines = sb.ToString().Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            Assert(lines.Length == 1, "Expected exactly header line for empty product export.");
            string expectedHeader = "ItemID,SKU,Name,Category,Brand,Color,Size,Gender,PurchasePrice,SellingPrice,MinStockLevel,CurrentStock,IsActive";
            Assert(lines[0] == expectedHeader, "Product export header did not match expected: " + lines[0]);
        }

        private void Test_ExportProducts_WritesRowsCorrectly()
        {
            var items = new List<ItemListItemDto>
            {
                new ItemListItemDto
                {
                    ItemID = 1,
                    SKU = "TSH-001",
                    Name = "Basic T-Shirt, Black",
                    CategoryName = "Apparel",
                    Brand = "Nike",
                    Color = "Black",
                    Size = "L",
                    Gender = "Unisex",
                    PurchasePrice = 75000,
                    SellingPrice = 120000,
                    MinStockLevel = 5,
                    CurrentStock = 12,
                    IsActive = true
                }
            };

            var service = new ExportService();
            var sb = new StringBuilder();
            using (var writer = new StringWriter(sb))
            {
                service.ExportProductsToCsv(items, writer);
            }

            string[] lines = sb.ToString().Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            Assert(lines.Length == 2, "Expected header and 1 data row.");
            Assert(lines[1].Contains("\"Basic T-Shirt, Black\""), "Product name with comma must be quoted.");
            Assert(lines[1].Contains("TSH-001"), "SKU must be present.");
            Assert(lines[1].Contains("75000,120000,5,12,Active"), "Numeric values and active status must be formatted correctly.");
        }

        private void Test_ExportProducts_FormatsPricesAsIntegers()
        {
            var items = new List<ItemListItemDto>
            {
                new ItemListItemDto
                {
                    ItemID = 2,
                    SKU = "SHO-001",
                    Name = "Running Shoes",
                    PurchasePrice = 250000,
                    SellingPrice = 499000,
                    MinStockLevel = 3,
                    CurrentStock = 8,
                    IsActive = true
                }
            };

            var service = new ExportService();
            var sb = new StringBuilder();
            using (var writer = new StringWriter(sb))
            {
                service.ExportProductsToCsv(items, writer);
            }

            string result = sb.ToString();
            Assert(!result.Contains("Rp"), "Exported CSV must not contain formatted currency symbols like 'Rp'.");
            Assert(result.Contains("250000,499000"), "Prices must be exported as clean raw integer numbers.");
        }

        private void Test_ExportProducts_IsActiveRepresentation()
        {
            var items = new List<ItemListItemDto>
            {
                new ItemListItemDto { ItemID = 1, SKU = "A", Name = "ActiveItem", IsActive = true },
                new ItemListItemDto { ItemID = 2, SKU = "B", Name = "InactiveItem", IsActive = false }
            };

            var service = new ExportService();
            var sb = new StringBuilder();
            using (var writer = new StringWriter(sb))
            {
                service.ExportProductsToCsv(items, writer);
            }

            string[] lines = sb.ToString().Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            Assert(lines[1].EndsWith("Active"), "First item must end with 'Active'.");
            Assert(lines[2].EndsWith("Inactive"), "Second item must end with 'Inactive'.");
        }

        private void Test_ExportProducts_EmptyList_WritesHeaderOnly()
        {
            var service = new ExportService();
            var sb = new StringBuilder();
            using (var writer = new StringWriter(sb))
            {
                service.ExportProductsToCsv(new List<ItemListItemDto>(), writer);
            }

            string output = sb.ToString();
            Assert(output.StartsWith("ItemID,SKU,Name"), "Should write header even when empty.");
            string[] lines = output.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            Assert(lines.Length == 1, "Should contain exactly one line (the header).");
        }

        private void Test_ExportProducts_ToFile_WithUtf8Bom()
        {
            string tempDir = Path.Combine(Path.GetTempPath(), "LDIS_ExportTest_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(tempDir);
            string filePath = Path.Combine(tempDir, "products.csv");

            try
            {
                var items = new List<ItemListItemDto>
                {
                    new ItemListItemDto { ItemID = 1, SKU = "TEST-01", Name = "Kaos Élégant", IsActive = true }
                };

                var service = new ExportService();
                service.ExportProductsToCsv(items, filePath);

                Assert(File.Exists(filePath), "Export file must be created on disk.");
                byte[] bytes = File.ReadAllBytes(filePath);

                // Verify UTF-8 BOM preamble (0xEF, 0xBB, 0xBF)
                Assert(bytes.Length >= 3, "File must contain at least 3 bytes.");
                Assert(bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF, "File must begin with UTF-8 BOM preamble.");

                string content = Encoding.UTF8.GetString(bytes);
                Assert(content.Contains("Kaos Élégant"), "Content must match UTF-8 text with accented characters.");
            }
            finally
            {
                CleanupTempDir(tempDir);
            }
        }

        // ==========================================
        // Transaction Export Tests
        // ==========================================

        private void Test_ExportTransactions_WritesCorrectHeaders()
        {
            var service = new ExportService();
            var sb = new StringBuilder();
            using (var writer = new StringWriter(sb))
            {
                service.ExportTransactionsToCsv(new List<TransactionListItemDto>(), writer);
            }

            string[] lines = sb.ToString().Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            Assert(lines.Length == 1, "Expected exactly 1 header line.");
            string expected = "TransactionID,TransactionDate,ItemID,SKU,ItemName,TransactionType,Quantity,UnitPrice,ReferenceNumber,SupplierOrCustomer,Reason,Notes,CreatedBy";
            Assert(lines[0] == expected, "Transaction header does not match expected: " + lines[0]);
        }

        private void Test_ExportTransactions_SignedQuantity_Derivation()
        {
            // Milestone 3 established:
            // IN: stored as positive (e.g. 10) -> stock increase: derived delta = +10
            // OUT: stored as positive (e.g. 4) -> stock decrease: derived delta = -4
            // ADJUSTMENT positive: stored as +3 -> derived delta = +3
            // ADJUSTMENT negative: stored as -2 -> derived delta = -2
            var txs = new List<TransactionListItemDto>
            {
                new TransactionListItemDto
                {
                    TransactionID = 1,
                    TransactionDate = new DateTime(2026, 9, 27, 10, 0, 0),
                    ItemID = 1,
                    SKU = "SKU-01",
                    ItemName = "Item 1",
                    TransactionType = "IN",
                    Quantity = 10
                },
                new TransactionListItemDto
                {
                    TransactionID = 2,
                    TransactionDate = new DateTime(2026, 9, 27, 11, 0, 0),
                    ItemID = 1,
                    SKU = "SKU-01",
                    ItemName = "Item 1",
                    TransactionType = "OUT",
                    Quantity = 4 // stored positive in DB for OUT
                },
                new TransactionListItemDto
                {
                    TransactionID = 3,
                    TransactionDate = new DateTime(2026, 9, 27, 12, 0, 0),
                    ItemID = 1,
                    SKU = "SKU-01",
                    ItemName = "Item 1",
                    TransactionType = "ADJUSTMENT",
                    Quantity = -2 // signed delta as stored in M3
                },
                new TransactionListItemDto
                {
                    TransactionID = 4,
                    TransactionDate = new DateTime(2026, 9, 27, 13, 0, 0),
                    ItemID = 1,
                    SKU = "SKU-01",
                    ItemName = "Item 1",
                    TransactionType = "ADJUSTMENT",
                    Quantity = 5 // signed delta as stored in M3
                }
            };

            var service = new ExportService();
            var sb = new StringBuilder();
            using (var writer = new StringWriter(sb))
            {
                service.ExportTransactionsToCsv(txs, writer);
            }

            string[] lines = sb.ToString().Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            Assert(lines.Length == 5, "Expected header and 4 transaction lines.");

            // Check IN quantity -> +10
            Assert(lines[1].Contains(",IN,10,"), "IN transaction quantity must be exported as positive 10.");

            // Check OUT quantity -> -4
            Assert(lines[2].Contains(",OUT,-4,"), "OUT transaction quantity must be explicitly derived as -4.");

            // Check ADJUSTMENT negative -> -2
            Assert(lines[3].Contains(",ADJUSTMENT,-2,"), "Negative ADJUSTMENT must retain -2 delta.");

            // Check ADJUSTMENT positive -> 5
            Assert(lines[4].Contains(",ADJUSTMENT,5,"), "Positive ADJUSTMENT must retain 5 delta.");
        }

        private void Test_ExportTransactions_EscapingInReasonAndNotes()
        {
            var txs = new List<TransactionListItemDto>
            {
                new TransactionListItemDto
                {
                    TransactionID = 1,
                    TransactionDate = new DateTime(2026, 9, 27, 10, 0, 0),
                    ItemID = 1,
                    SKU = "SKU-01",
                    ItemName = "Item 1",
                    TransactionType = "ADJUSTMENT",
                    Quantity = -1,
                    Reason = "Broken, \"damaged in transit\"",
                    Notes = "Audited by: John Doe\nApproved by: Manager"
                }
            };

            var service = new ExportService();
            var sb = new StringBuilder();
            using (var writer = new StringWriter(sb))
            {
                service.ExportTransactionsToCsv(txs, writer);
            }

            string result = sb.ToString();
            Assert(result.Contains("\"Broken, \"\"damaged in transit\"\"\""), "Reason with commas and quotes must be properly escaped.");
            Assert(result.Contains("\"Audited by: John Doe\nApproved by: Manager\""), "Notes with newlines must be quoted.");
        }

        private void Test_ExportTransactions_EmptyList_WritesHeaderOnly()
        {
            var service = new ExportService();
            var sb = new StringBuilder();
            using (var writer = new StringWriter(sb))
            {
                service.ExportTransactionsToCsv(new List<TransactionListItemDto>(), writer);
            }

            string output = sb.ToString();
            string[] lines = output.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            Assert(lines.Length == 1, "Empty transaction export must write header only.");
        }

        private void Test_ExportTransactions_ToFile_WithUtf8Bom()
        {
            string tempDir = Path.Combine(Path.GetTempPath(), "LDIS_TxExportTest_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(tempDir);
            string filePath = Path.Combine(tempDir, "transactions.csv");

            try
            {
                var txs = new List<TransactionListItemDto>
                {
                    new TransactionListItemDto
                    {
                        TransactionID = 1,
                        TransactionDate = DateTime.Now,
                        ItemID = 1,
                        SKU = "SKU-01",
                        ItemName = "Batik Élit",
                        TransactionType = "IN",
                        Quantity = 10,
                        SupplierOrCustomer = "Toko Sumber Rejeki"
                    }
                };

                var service = new ExportService();
                service.ExportTransactionsToCsv(txs, filePath);

                Assert(File.Exists(filePath), "Transaction export file must be written.");
                byte[] bytes = File.ReadAllBytes(filePath);

                // UTF-8 BOM check
                Assert(bytes.Length >= 3, "File must contain bytes.");
                Assert(bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF, "Must contain UTF-8 BOM.");

                string text = Encoding.UTF8.GetString(bytes);
                Assert(text.Contains("Batik Élit"), "UTF-8 characters must be preserved.");
                Assert(text.Contains("Toko Sumber Rejeki"), "Supplier text must be preserved.");
            }
            finally
            {
                CleanupTempDir(tempDir);
            }
        }

        // ==========================================
        // Database Backup Integration Tests
        // ==========================================

        private void Test_BackupDatabase_CreatesValidSqliteDatabase()
        {
            string sourceDbPath;
            var factory = CreateTestDatabase(out sourceDbPath);

            string tempBackupDir = Path.Combine(Path.GetTempPath(), "LDIS_BackupTest_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(tempBackupDir);
            string backupPath = Path.Combine(tempBackupDir, "backup.db");

            try
            {
                // Seed source database with category, item, and transaction
                var catRepo = new CategoryRepository(factory);
                long catId = catRepo.Create(new Category { CategoryName = "Apparel" });

                var itemRepo = new ItemRepository(factory);
                long itemId = itemRepo.Create(new Item
                {
                    SKU = "BAK-001",
                    Name = "Backup Test Shirt",
                    CategoryID = catId,
                    PurchasePrice = 50000,
                    SellingPrice = 90000,
                    MinStockLevel = 5,
                    CurrentStock = 20,
                    IsActive = true
                });

                var txRepo = new InventoryTransactionRepository(factory);
                var stockService = new StockService(txRepo, itemRepo);
                stockService.StockIn(new StockOperationRequest
                {
                    ItemID = itemId,
                    Quantity = 10,
                    UnitPrice = 50000,
                    SupplierOrCustomer = "Supplier A",
                    Notes = "Initial Batch"
                });

                // Execute Backup
                var backupService = new BackupService(factory);
                backupService.BackupDatabase(backupPath);

                // Verify backup file exists and is not empty
                Assert(File.Exists(backupPath), "Backup file must exist on disk.");
                var fileInfo = new FileInfo(backupPath);
                Assert(fileInfo.Length > 0, "Backup database file size must be greater than 0 bytes.");

                // Open backup file with a fresh SQLite connection and verify schema and data
                string backupConnString = string.Format("Data Source={0};Version=3;", backupPath);
                using (var backupConn = new SQLiteConnection(backupConnString))
                {
                    backupConn.Open();

                    // Query Items table count
                    using (var cmd = backupConn.CreateCommand())
                    {
                        cmd.CommandText = "SELECT COUNT(*) FROM Items WHERE SKU = 'BAK-001';";
                        long count = Convert.ToInt64(cmd.ExecuteScalar());
                        Assert(count == 1, "Backup database must contain the seeded item BAK-001.");
                    }

                    // Query CurrentStock
                    using (var cmd = backupConn.CreateCommand())
                    {
                        cmd.CommandText = "SELECT CurrentStock FROM Items WHERE SKU = 'BAK-001';";
                        int currentStock = Convert.ToInt32(cmd.ExecuteScalar());
                        Assert(currentStock == 10, "CurrentStock in backup database must be 10 (0 initial + 10 IN).");
                    }

                    // Query InventoryTransactions count
                    using (var cmd = backupConn.CreateCommand())
                    {
                        cmd.CommandText = "SELECT COUNT(*) FROM InventoryTransactions WHERE ItemID = @ItemID;";
                        cmd.Parameters.AddWithValue("@ItemID", itemId);
                        long txCount = Convert.ToInt64(cmd.ExecuteScalar());
                        Assert(txCount == 1, "Backup database must contain the stock transaction.");
                    }

                    backupConn.Close();
                }
            }
            finally
            {
                CleanupTempDb(sourceDbPath);
                CleanupTempDir(tempBackupDir);
            }
        }

        private void Test_BackupDatabase_WhileActiveConnectionOpen()
        {
            string sourceDbPath;
            var factory = CreateTestDatabase(out sourceDbPath);

            string tempBackupDir = Path.Combine(Path.GetTempPath(), "LDIS_BackupActiveConn_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(tempBackupDir);
            string backupPath = Path.Combine(tempBackupDir, "backup_live.db");

            try
            {
                // Hold an open connection reading from source database
                using (var activeConn = factory.CreateOpenConnection())
                {
                    using (var cmd = activeConn.CreateCommand())
                    {
                        cmd.CommandText = "SELECT COUNT(*) FROM Items;";
                        cmd.ExecuteScalar();
                    }

                    // Perform backup concurrently
                    var backupService = new BackupService(factory);
                    backupService.BackupDatabase(backupPath);
                }

                Assert(File.Exists(backupPath), "Backup must succeed even while another connection is open.");
                var fileInfo = new FileInfo(backupPath);
                Assert(fileInfo.Length > 0, "Backup file must be valid non-zero size.");
            }
            finally
            {
                CleanupTempDb(sourceDbPath);
                CleanupTempDir(tempBackupDir);
            }
        }

        private void Test_BackupDatabase_DestinationSameAsSource_ThrowsException()
        {
            string sourceDbPath;
            var factory = CreateTestDatabase(out sourceDbPath);

            try
            {
                var backupService = new BackupService(factory);
                bool threw = false;

                try
                {
                    backupService.BackupDatabase(sourceDbPath);
                }
                catch (InvalidOperationException)
                {
                    threw = true;
                }

                Assert(threw, "Backing up to the active database file itself must throw an InvalidOperationException.");
            }
            finally
            {
                CleanupTempDb(sourceDbPath);
            }
        }

        private void Test_BackupDatabase_EmptyOrNullPath_ThrowsException()
        {
            string sourceDbPath;
            var factory = CreateTestDatabase(out sourceDbPath);

            try
            {
                var backupService = new BackupService(factory);
                bool threw = false;

                try
                {
                    backupService.BackupDatabase("   ");
                }
                catch (ArgumentException)
                {
                    threw = true;
                }

                Assert(threw, "Backing up to empty path must throw ArgumentException.");
            }
            finally
            {
                CleanupTempDb(sourceDbPath);
            }
        }

        private static void CleanupTempDb(string dbPath)
        {
            try
            {
                string dir = Path.GetDirectoryName(dbPath);
                if (Directory.Exists(dir))
                {
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                    Directory.Delete(dir, true);
                }
            }
            catch
            {
                // Best effort cleanup in temp directory
            }
        }
    }
}
