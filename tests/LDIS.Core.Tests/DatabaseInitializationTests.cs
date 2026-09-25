using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using LDIS.Core.Data;

namespace LDIS.Core.Tests
{
    public class DatabaseInitializationTests
    {
        private static string CreateTempDbPath()
        {
            string tempDir = Path.Combine(Path.GetTempPath(), "LDIS_Tests_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(tempDir);
            return Path.Combine(tempDir, "test_inventory.db");
        }

        private static void CleanupTempDb(string dbPath)
        {
            try
            {
                string dir = Path.GetDirectoryName(dbPath);
                if (Directory.Exists(dir))
                {
                    // Force garbage collection to release any lingering SQLite handles
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                    Directory.Delete(dir, true);
                }
            }
            catch
            {
                // Best effort cleanup in temp
            }
        }

        public void RunAllTests()
        {
            RunTest("Test_FreshDatabaseCreation_CreatesFileAndAppliesSchema", Test_FreshDatabaseCreation_CreatesFileAndAppliesSchema);
            RunTest("Test_IdempotentInitialization", Test_IdempotentInitialization);
            RunTest("Test_ForeignKeyEnforcement_ItemsToCategories", Test_ForeignKeyEnforcement_ItemsToCategories);
            RunTest("Test_ForeignKeyEnforcement_TransactionsToItems", Test_ForeignKeyEnforcement_TransactionsToItems);
            RunTest("Test_TransactionRollbackIntegrity", Test_TransactionRollbackIntegrity);
            RunTest("Test_CheckConstraint_TransactionType", Test_CheckConstraint_TransactionType);
            RunTest("Test_PriceRange_Int64LargeMonetaryValue", Test_PriceRange_Int64LargeMonetaryValue);
        }

        private void RunTest(string testName, Action testAction)
        {
            Console.Write(string.Format("[TEST] {0} ... ", testName));
            try
            {
                testAction();
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("PASSED");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("FAILED");
                Console.ResetColor();
                Console.WriteLine(string.Format("  Error: {0}", ex.Message));
                Console.WriteLine(string.Format("  Stack: {0}", ex.StackTrace));
                throw;
            }
        }

        private void Test_FreshDatabaseCreation_CreatesFileAndAppliesSchema()
        {
            string dbPath = CreateTempDbPath();
            try
            {
                var config = new DatabaseConfig(dbPath);
                var factory = new DbConnectionFactory(config);
                var initializer = new DatabaseInitializer(factory);

                initializer.Initialize();

                Assert(File.Exists(dbPath), "Database file was not created on disk.");

                int version = initializer.GetCurrentSchemaVersion();
                Assert(version == 1, string.Format("Expected schema version 1, but found {0}.", version));

                // Verify tables in sqlite_master
                var tables = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                var indexes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                using (var conn = factory.CreateOpenConnection())
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT type, name FROM sqlite_master WHERE type IN ('table', 'index');";
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string type = reader.GetString(0);
                            string name = reader.GetString(1);
                            if (type.Equals("table", StringComparison.OrdinalIgnoreCase))
                                tables.Add(name);
                            else if (type.Equals("index", StringComparison.OrdinalIgnoreCase))
                                indexes.Add(name);
                        }
                    }
                }

                Assert(tables.Contains("Categories"), "Categories table missing.");
                Assert(tables.Contains("Items"), "Items table missing.");
                Assert(tables.Contains("InventoryTransactions"), "InventoryTransactions table missing.");

                Assert(indexes.Contains("IX_Transactions_ItemID"), "IX_Transactions_ItemID index missing.");
                Assert(indexes.Contains("IX_Transactions_Date"), "IX_Transactions_Date index missing.");
                Assert(indexes.Contains("IX_Transactions_Type"), "IX_Transactions_Type index missing.");
                Assert(indexes.Contains("IX_Items_Category"), "IX_Items_Category index missing.");
                Assert(indexes.Contains("IX_Items_Name"), "IX_Items_Name index missing.");
            }
            finally
            {
                CleanupTempDb(dbPath);
            }
        }

        private void Test_IdempotentInitialization()
        {
            string dbPath = CreateTempDbPath();
            try
            {
                var config = new DatabaseConfig(dbPath);
                var factory = new DbConnectionFactory(config);
                var initializer = new DatabaseInitializer(factory);

                initializer.Initialize();
                // Second call must not throw or alter schema
                initializer.Initialize();

                int version = initializer.GetCurrentSchemaVersion();
                Assert(version == 1, string.Format("Expected schema version 1, got {0}.", version));
            }
            finally
            {
                CleanupTempDb(dbPath);
            }
        }

        private void Test_ForeignKeyEnforcement_ItemsToCategories()
        {
            string dbPath = CreateTempDbPath();
            try
            {
                var config = new DatabaseConfig(dbPath);
                var factory = new DbConnectionFactory(config);
                new DatabaseInitializer(factory).Initialize();

                using (var conn = factory.CreateOpenConnection())
                using (var cmd = conn.CreateCommand())
                {
                    // Attempt to insert Item with non-existent CategoryID 9999
                    cmd.CommandText = "INSERT INTO Items (SKU, Name, CategoryID) VALUES ('TEST-01', 'Test Item', 9999);";

                    bool threwForeignKeyException = false;
                    try
                    {
                        cmd.ExecuteNonQuery();
                    }
                    catch (SQLiteException ex)
                    {
                        if (ex.ResultCode == SQLiteErrorCode.Constraint || ex.Message.IndexOf("FOREIGN KEY", StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            threwForeignKeyException = true;
                        }
                    }

                    Assert(threwForeignKeyException, "Foreign key constraint was not enforced when inserting invalid CategoryID.");
                }
            }
            finally
            {
                CleanupTempDb(dbPath);
            }
        }

        private void Test_ForeignKeyEnforcement_TransactionsToItems()
        {
            string dbPath = CreateTempDbPath();
            try
            {
                var config = new DatabaseConfig(dbPath);
                var factory = new DbConnectionFactory(config);
                new DatabaseInitializer(factory).Initialize();

                using (var conn = factory.CreateOpenConnection())
                using (var cmd = conn.CreateCommand())
                {
                    // Attempt to insert InventoryTransaction with non-existent ItemID 9999
                    cmd.CommandText = "INSERT INTO InventoryTransactions (ItemID, TransactionType, Quantity) VALUES (9999, 'IN', 10);";

                    bool threwForeignKeyException = false;
                    try
                    {
                        cmd.ExecuteNonQuery();
                    }
                    catch (SQLiteException ex)
                    {
                        if (ex.ResultCode == SQLiteErrorCode.Constraint || ex.Message.IndexOf("FOREIGN KEY", StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            threwForeignKeyException = true;
                        }
                    }

                    Assert(threwForeignKeyException, "Foreign key constraint was not enforced when inserting invalid ItemID into transactions.");
                }
            }
            finally
            {
                CleanupTempDb(dbPath);
            }
        }

        private void Test_TransactionRollbackIntegrity()
        {
            string dbPath = CreateTempDbPath();
            try
            {
                var config = new DatabaseConfig(dbPath);
                var factory = new DbConnectionFactory(config);
                new DatabaseInitializer(factory).Initialize();

                using (var conn = factory.CreateOpenConnection())
                {
                    using (var trans = conn.BeginTransaction())
                    {
                        using (var cmd = conn.CreateCommand())
                        {
                            cmd.Transaction = trans;
                            cmd.CommandText = "INSERT INTO Categories (CategoryName) VALUES ('Rollback Category');";
                            cmd.ExecuteNonQuery();
                        }

                        // Explicit rollback
                        trans.Rollback();
                    }

                    // Verify row was rolled back
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = "SELECT COUNT(*) FROM Categories WHERE CategoryName = 'Rollback Category';";
                        long count = Convert.ToInt64(cmd.ExecuteScalar());
                        Assert(count == 0, "Rolled back category still exists in database.");
                    }
                }
            }
            finally
            {
                CleanupTempDb(dbPath);
            }
        }

        private void Test_CheckConstraint_TransactionType()
        {
            string dbPath = CreateTempDbPath();
            try
            {
                var config = new DatabaseConfig(dbPath);
                var factory = new DbConnectionFactory(config);
                new DatabaseInitializer(factory).Initialize();

                using (var conn = factory.CreateOpenConnection())
                using (var cmd = conn.CreateCommand())
                {
                    // First create valid item
                    cmd.CommandText = "INSERT INTO Items (SKU, Name) VALUES ('SKU-CHECK', 'Check Item'); SELECT last_insert_rowid();";
                    long itemId = Convert.ToInt64(cmd.ExecuteScalar());

                    // Attempt invalid TransactionType
                    cmd.CommandText = string.Format("INSERT INTO InventoryTransactions (ItemID, TransactionType, Quantity) VALUES ({0}, 'INVALID_TYPE', 5);", itemId);

                    bool threwConstraintException = false;
                    try
                    {
                        cmd.ExecuteNonQuery();
                    }
                    catch (SQLiteException ex)
                    {
                        if (ex.ResultCode == SQLiteErrorCode.Constraint || ex.Message.IndexOf("CHECK", StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            threwConstraintException = true;
                        }
                    }

                    Assert(threwConstraintException, "CHECK constraint on TransactionType was not enforced.");
                }
            }
            finally
            {
                CleanupTempDb(dbPath);
            }
        }

        private void Test_PriceRange_Int64LargeMonetaryValue()
        {
            string dbPath = CreateTempDbPath();
            try
            {
                var config = new DatabaseConfig(dbPath);
                var factory = new DbConnectionFactory(config);
                new DatabaseInitializer(factory).Initialize();

                long largePrice = 5000000000L; // 5 Billion Rupiah (> int.MaxValue)

                using (var conn = factory.CreateOpenConnection())
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "INSERT INTO Items (SKU, Name, PurchasePrice, SellingPrice) VALUES ('EXP-01', 'High Value Item', @pPrice, @sPrice);";
                    cmd.Parameters.AddWithValue("@pPrice", largePrice);
                    cmd.Parameters.AddWithValue("@sPrice", largePrice + 500000000L);
                    cmd.ExecuteNonQuery();

                    cmd.CommandText = "SELECT PurchasePrice, SellingPrice FROM Items WHERE SKU = 'EXP-01';";
                    using (var reader = cmd.ExecuteReader())
                    {
                        Assert(reader.Read(), "Failed to read inserted item.");
                        long readPurchasePrice = reader.GetInt64(0);
                        long readSellingPrice = reader.GetInt64(1);

                        Assert(readPurchasePrice == largePrice, string.Format("PurchasePrice mismatch: expected {0}, got {1}.", largePrice, readPurchasePrice));
                        Assert(readSellingPrice == largePrice + 500000000L, string.Format("SellingPrice mismatch: expected {0}, got {1}.", largePrice + 500000000L, readSellingPrice));
                    }
                }
            }
            finally
            {
                CleanupTempDb(dbPath);
            }
        }

        private static void Assert(bool condition, string message)
        {
            if (!condition)
            {
                throw new InvalidOperationException("Assertion Failed: " + message);
            }
        }
    }
}
