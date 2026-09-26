using System;
using System.Data.SQLite;
using System.IO;
using LDIS.Core.Data;
using LDIS.Core.Data.Repositories;
using LDIS.Core.DTOs;
using LDIS.Core.Models;
using LDIS.Core.Services;

namespace LDIS.Core.Tests
{
    public class Milestone6DeploymentAndRegressionTests
    {
        private static string CreateTempDbPath()
        {
            string tempDir = Path.Combine(Path.GetTempPath(), "LDIS_M6Tests_" + Guid.NewGuid().ToString("N"));
            // Intentionally do NOT create directory here to test EnsureDirectoryExists
            return Path.Combine(tempDir, "test_m6.db");
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

        public void RunAllTests()
        {
            RunTest("Test_FreshDatabaseInitialization_CreatesDirectoryAndDatabaseFile", Test_FreshDatabaseInitialization_CreatesDirectoryAndDatabaseFile);
            RunTest("Test_ExistingDatabase_UpgradeSafety_DoesNotResetData", Test_ExistingDatabase_UpgradeSafety_DoesNotResetData);
            RunTest("Test_AssemblyVersion_MatchesReleaseVersion", Test_AssemblyVersion_MatchesReleaseVersion);
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

        private void Test_FreshDatabaseInitialization_CreatesDirectoryAndDatabaseFile()
        {
            string dbPath = CreateTempDbPath();
            string dir = Path.GetDirectoryName(dbPath);

            try
            {
                Assert(!Directory.Exists(dir), "Directory should not exist prior to test.");

                var config = new DatabaseConfig(dbPath);
                var factory = new DbConnectionFactory(config);
                var initializer = new DatabaseInitializer(factory);

                // Initialize should create directory, file, and apply schema
                initializer.Initialize();

                Assert(Directory.Exists(dir), "Database directory must be created by initialization.");
                Assert(File.Exists(dbPath), "Database file must be created on disk.");

                // Verify user_version is 1 and all required tables exist
                using (var conn = factory.CreateOpenConnection())
                {
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = "PRAGMA user_version;";
                        int version = Convert.ToInt32(cmd.ExecuteScalar());
                        Assert(version == 1, "Expected schema user_version = 1 on freshly initialized database.");
                    }

                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"
                            SELECT COUNT(*) FROM sqlite_master 
                            WHERE type='table' AND name IN ('Categories', 'Items', 'InventoryTransactions');";
                        long tableCount = Convert.ToInt64(cmd.ExecuteScalar());
                        Assert(tableCount == 3, "Expected all 3 core tables (Categories, Items, InventoryTransactions) in database.");
                    }
                }
            }
            finally
            {
                CleanupTempDb(dbPath);
            }
        }

        private void Test_ExistingDatabase_UpgradeSafety_DoesNotResetData()
        {
            string dbPath = CreateTempDbPath();

            try
            {
                var config = new DatabaseConfig(dbPath);
                var factory = new DbConnectionFactory(config);
                var initializer = new DatabaseInitializer(factory);
                initializer.Initialize();

                // Seed user data: category, item, and stock transaction
                var catRepo = new CategoryRepository(factory);
                long catId = catRepo.Create(new Category { CategoryName = "Safety Wear" });

                var itemRepo = new ItemRepository(factory);
                long itemId = itemRepo.Create(new Item
                {
                    SKU = "SAF-001",
                    Name = "Safety Vest Neon",
                    CategoryID = catId,
                    Brand = "ProGuard",
                    PurchasePrice = 35000,
                    SellingPrice = 60000,
                    MinStockLevel = 10,
                    IsActive = true
                });

                var txRepo = new InventoryTransactionRepository(factory);
                var stockService = new StockService(txRepo, itemRepo);
                stockService.StockIn(new StockOperationRequest
                {
                    ItemID = itemId,
                    Quantity = 50,
                    UnitPrice = 35000,
                    SupplierOrCustomer = "PT Safety Supplies",
                    Notes = "Initial Stock"
                });

                // Simulate application relaunch / update by calling Initialize again
                initializer.Initialize();

                // Verify that user_version is still 1 and data was NOT wiped or re-initialized
                using (var conn = factory.CreateOpenConnection())
                {
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = "PRAGMA user_version;";
                        int version = Convert.ToInt32(cmd.ExecuteScalar());
                        Assert(version == 1, "user_version should remain 1.");
                    }

                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = "SELECT COUNT(*) FROM Items WHERE SKU = 'SAF-001';";
                        long count = Convert.ToInt64(cmd.ExecuteScalar());
                        Assert(count == 1, "Seeded item must remain intact after subsequent Initialize() call.");
                    }

                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = "SELECT CurrentStock FROM Items WHERE SKU = 'SAF-001';";
                        int stock = Convert.ToInt32(cmd.ExecuteScalar());
                        Assert(stock == 50, "CurrentStock must remain 50 without being reset.");
                    }

                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = "SELECT COUNT(*) FROM InventoryTransactions WHERE ItemID = @ItemID;";
                        cmd.Parameters.AddWithValue("@ItemID", itemId);
                        long txCount = Convert.ToInt64(cmd.ExecuteScalar());
                        Assert(txCount == 1, "Transaction history must remain intact.");
                    }
                }
            }
            finally
            {
                CleanupTempDb(dbPath);
            }
        }

        private void Test_AssemblyVersion_MatchesReleaseVersion()
        {
            var coreVersion = typeof(Item).Assembly.GetName().Version;
            Assert(coreVersion.Major == 1 && coreVersion.Minor == 0 && coreVersion.Build == 0,
                string.Format("Expected LDIS.Core assembly version 1.0.0.x, found: {0}", coreVersion));
        }
    }
}
