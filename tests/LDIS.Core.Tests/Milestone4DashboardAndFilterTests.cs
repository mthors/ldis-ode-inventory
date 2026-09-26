using System;
using System.IO;
using System.Linq;
using LDIS.Core.Data;
using LDIS.Core.Data.Repositories;
using LDIS.Core.DTOs;
using LDIS.Core.Models;
using LDIS.Core.Services;

namespace LDIS.Core.Tests
{
    public class Milestone4DashboardAndFilterTests
    {
        private static string CreateTempDbPath()
        {
            string tempDir = Path.Combine(Path.GetTempPath(), "LDIS_M4Tests_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(tempDir);
            return Path.Combine(tempDir, "test_m4.db");
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
            RunTest("Test_DashboardSummary_EmptyDatabase_ReturnsZeros", Test_DashboardSummary_EmptyDatabase_ReturnsZeros);
            RunTest("Test_DashboardSummary_AccurateCounts_AndTotalUnits", Test_DashboardSummary_AccurateCounts_AndTotalUnits);
            RunTest("Test_DashboardSummary_ExcludesInactiveProducts", Test_DashboardSummary_ExcludesInactiveProducts);
            RunTest("Test_DashboardSummary_MinStockZero_NeverLowStock", Test_DashboardSummary_MinStockZero_NeverLowStock);
            RunTest("Test_DashboardSummary_TotalUnits_IncludesLowStockItems", Test_DashboardSummary_TotalUnits_IncludesLowStockItems);
            RunTest("Test_Search_FilterByStockStatus_NormalStock", Test_Search_FilterByStockStatus_NormalStock);
            RunTest("Test_Search_FilterByStockStatus_LowStock", Test_Search_FilterByStockStatus_LowStock);
            RunTest("Test_Search_FilterByStockStatus_OutOfStock", Test_Search_FilterByStockStatus_OutOfStock);
            RunTest("Test_Search_FilterByGender_ExactAndCaseInsensitive", Test_Search_FilterByGender_ExactAndCaseInsensitive);
            RunTest("Test_Search_FilterByGender_NoneUnspecified", Test_Search_FilterByGender_NoneUnspecified);
            RunTest("Test_Search_CombinedFilters_StockStatus_Gender_Category_SearchText", Test_Search_CombinedFilters_StockStatus_Gender_Category_SearchText);
            RunTest("Test_DashboardSummary_UpdatesAfterStockOperations", Test_DashboardSummary_UpdatesAfterStockOperations);
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
                Console.WriteLine("  Error: " + ex.Message);
                Console.ResetColor();
                throw;
            }
        }

        private static void Assert(bool condition, string message)
        {
            if (!condition)
            {
                throw new InvalidOperationException("Assertion Failed: " + message);
            }
        }

        private void Test_DashboardSummary_EmptyDatabase_ReturnsZeros()
        {
            string dbPath;
            var factory = CreateTestDatabase(out dbPath);
            try
            {
                var itemRepo = new ItemRepository(factory);
                var catRepo = new CategoryRepository(factory);
                var itemService = new ItemService(itemRepo, catRepo);

                var summary = itemService.GetDashboardSummary();
                Assert(summary.TotalActiveProducts == 0, "TotalActiveProducts should be 0 on empty DB.");
                Assert(summary.TotalUnitsInStock == 0, "TotalUnitsInStock should be 0 on empty DB.");
                Assert(summary.LowStockCount == 0, "LowStockCount should be 0 on empty DB.");
                Assert(summary.OutOfStockCount == 0, "OutOfStockCount should be 0 on empty DB.");
            }
            finally
            {
                CleanupTempDb(dbPath);
            }
        }

        private void Test_DashboardSummary_AccurateCounts_AndTotalUnits()
        {
            string dbPath;
            var factory = CreateTestDatabase(out dbPath);
            try
            {
                var itemRepo = new ItemRepository(factory);
                var catRepo = new CategoryRepository(factory);
                var itemService = new ItemService(itemRepo, catRepo);
                var stockRepo = new InventoryTransactionRepository(factory);
                var stockService = new StockService(stockRepo, itemRepo);

                // 1. Normal Stock: Stock 10, MinStock 5
                long id1 = itemService.SaveItem(new Item
                {
                    SKU = "SKU-001",
                    Name = "Normal Stock Item",
                    MinStockLevel = 5,
                    PurchasePrice = 1000,
                    SellingPrice = 2000,
                    IsActive = true
                });
                stockService.StockIn(new StockOperationRequest
                {
                    ItemID = id1,
                    Quantity = 10
                });

                // 2. Low Stock: Stock 3, MinStock 5
                long id2 = itemService.SaveItem(new Item
                {
                    SKU = "SKU-002",
                    Name = "Low Stock Item",
                    MinStockLevel = 5,
                    PurchasePrice = 1000,
                    SellingPrice = 2000,
                    IsActive = true
                });
                stockService.StockIn(new StockOperationRequest
                {
                    ItemID = id2,
                    Quantity = 3
                });

                // 3. Out of Stock: Stock 0, MinStock 5
                itemService.SaveItem(new Item
                {
                    SKU = "SKU-003",
                    Name = "Out Of Stock Item",
                    MinStockLevel = 5,
                    PurchasePrice = 1000,
                    SellingPrice = 2000,
                    IsActive = true
                });

                // 4. Inactive Product: Stock 20, MinStock 5 (must be excluded from active KPIs)
                long id4 = itemService.SaveItem(new Item
                {
                    SKU = "SKU-004",
                    Name = "Inactive Item",
                    MinStockLevel = 5,
                    PurchasePrice = 1000,
                    SellingPrice = 2000,
                    IsActive = true
                });
                stockService.StockIn(new StockOperationRequest
                {
                    ItemID = id4,
                    Quantity = 20
                });
                itemService.DeactivateItem(id4);

                var summary = itemService.GetDashboardSummary();

                Assert(summary.TotalActiveProducts == 3, string.Format("Expected 3 active products, got {0}", summary.TotalActiveProducts));
                Assert(summary.TotalUnitsInStock == 13, string.Format("Expected 13 total units (10 + 3), got {0}", summary.TotalUnitsInStock));
                Assert(summary.LowStockCount == 1, string.Format("Expected 1 low stock item, got {0}", summary.LowStockCount));
                Assert(summary.OutOfStockCount == 1, string.Format("Expected 1 out of stock item, got {0}", summary.OutOfStockCount));
            }
            finally
            {
                CleanupTempDb(dbPath);
            }
        }

        private void Test_DashboardSummary_ExcludesInactiveProducts()
        {
            string dbPath;
            var factory = CreateTestDatabase(out dbPath);
            try
            {
                var itemRepo = new ItemRepository(factory);
                var catRepo = new CategoryRepository(factory);
                var itemService = new ItemService(itemRepo, catRepo);

                long id1 = itemService.SaveItem(new Item
                {
                    SKU = "SKU-INACT",
                    Name = "Inactive Product",
                    MinStockLevel = 10,
                    PurchasePrice = 500,
                    SellingPrice = 1000,
                    IsActive = true
                });
                itemService.DeactivateItem(id1);

                var summary = itemService.GetDashboardSummary();
                Assert(summary.TotalActiveProducts == 0, "Inactive item should not count in TotalActiveProducts.");
                Assert(summary.TotalUnitsInStock == 0, "Inactive item stock should not count in TotalUnitsInStock.");
                Assert(summary.LowStockCount == 0, "Inactive item should not count in LowStockCount.");
                Assert(summary.OutOfStockCount == 0, "Inactive item should not count in OutOfStockCount.");
            }
            finally
            {
                CleanupTempDb(dbPath);
            }
        }

        private void Test_DashboardSummary_MinStockZero_NeverLowStock()
        {
            string dbPath;
            var factory = CreateTestDatabase(out dbPath);
            try
            {
                var itemRepo = new ItemRepository(factory);
                var catRepo = new CategoryRepository(factory);
                var itemService = new ItemService(itemRepo, catRepo);
                var stockRepo = new InventoryTransactionRepository(factory);
                var stockService = new StockService(stockRepo, itemRepo);

                // MinStockLevel = 0 with stock 0 -> Out of Stock (never Low Stock)
                itemService.SaveItem(new Item
                {
                    SKU = "SKU-MIN0-ZERO",
                    Name = "MinStock Zero with 0 stock",
                    MinStockLevel = 0,
                    PurchasePrice = 100,
                    SellingPrice = 200,
                    IsActive = true
                });

                // MinStockLevel = 0 with stock 5 -> Normal Stock (CurrentStock > MinStockLevel, never Low Stock)
                long id2 = itemService.SaveItem(new Item
                {
                    SKU = "SKU-MIN0-POS",
                    Name = "MinStock Zero with 5 stock",
                    MinStockLevel = 0,
                    PurchasePrice = 100,
                    SellingPrice = 200,
                    IsActive = true
                });
                stockService.StockIn(new StockOperationRequest
                {
                    ItemID = id2,
                    Quantity = 5
                });

                var summary = itemService.GetDashboardSummary();
                Assert(summary.TotalActiveProducts == 2, "Expected 2 active products.");
                Assert(summary.TotalUnitsInStock == 5, "Expected 5 total units in stock.");
                Assert(summary.LowStockCount == 0, "Low stock count must be 0 when MinStockLevel is 0.");
                Assert(summary.OutOfStockCount == 1, "Expected 1 out of stock product.");
            }
            finally
            {
                CleanupTempDb(dbPath);
            }
        }

        private void Test_DashboardSummary_TotalUnits_IncludesLowStockItems()
        {
            string dbPath;
            var factory = CreateTestDatabase(out dbPath);
            try
            {
                var itemRepo = new ItemRepository(factory);
                var catRepo = new CategoryRepository(factory);
                var itemService = new ItemService(itemRepo, catRepo);
                var stockRepo = new InventoryTransactionRepository(factory);
                var stockService = new StockService(stockRepo, itemRepo);

                // Normal Stock: 100 units (Min 50)
                long id1 = itemService.SaveItem(new Item
                {
                    SKU = "SKU-NORM",
                    Name = "Normal Product",
                    MinStockLevel = 50,
                    PurchasePrice = 10,
                    SellingPrice = 20,
                    IsActive = true
                });
                stockService.StockIn(new StockOperationRequest
                {
                    ItemID = id1,
                    Quantity = 100
                });

                // Low Stock: 15 units (Min 20)
                long id2 = itemService.SaveItem(new Item
                {
                    SKU = "SKU-LOW",
                    Name = "Low Stock Product",
                    MinStockLevel = 20,
                    PurchasePrice = 10,
                    SellingPrice = 20,
                    IsActive = true
                });
                stockService.StockIn(new StockOperationRequest
                {
                    ItemID = id2,
                    Quantity = 15
                });

                var summary = itemService.GetDashboardSummary();
                Assert(summary.TotalUnitsInStock == 115, "Total units must include units from low stock items (100 + 15 = 115).");
            }
            finally
            {
                CleanupTempDb(dbPath);
            }
        }

        private void Test_Search_FilterByStockStatus_NormalStock()
        {
            string dbPath;
            var factory = CreateTestDatabase(out dbPath);
            try
            {
                var itemRepo = new ItemRepository(factory);
                var catRepo = new CategoryRepository(factory);
                var itemService = new ItemService(itemRepo, catRepo);
                var stockRepo = new InventoryTransactionRepository(factory);
                var stockService = new StockService(stockRepo, itemRepo);

                // Normal: 10 > 5
                long idNorm = itemService.SaveItem(new Item { SKU = "NORM", Name = "Norm", MinStockLevel = 5, PurchasePrice = 10, SellingPrice = 20, IsActive = true });
                stockService.StockIn(new StockOperationRequest { ItemID = idNorm, Quantity = 10 });

                // Low: 5 <= 5 (and > 0)
                long idLow = itemService.SaveItem(new Item { SKU = "LOW", Name = "Low", MinStockLevel = 5, PurchasePrice = 10, SellingPrice = 20, IsActive = true });
                stockService.StockIn(new StockOperationRequest { ItemID = idLow, Quantity = 5 });

                // Out: 0
                itemService.SaveItem(new Item { SKU = "OUT", Name = "Out", MinStockLevel = 5, PurchasePrice = 10, SellingPrice = 20, IsActive = true });

                var results = itemService.SearchItems(new ItemSearchCriteria
                {
                    StockStatus = StockFilterStatus.NormalStock
                }).ToList();

                Assert(results.Count == 1, string.Format("Normal stock filter should return 1 item, returned {0}", results.Count));
                Assert(results[0].SKU == "NORM", "Expected NORM item.");
            }
            finally
            {
                CleanupTempDb(dbPath);
            }
        }

        private void Test_Search_FilterByStockStatus_LowStock()
        {
            string dbPath;
            var factory = CreateTestDatabase(out dbPath);
            try
            {
                var itemRepo = new ItemRepository(factory);
                var catRepo = new CategoryRepository(factory);
                var itemService = new ItemService(itemRepo, catRepo);
                var stockRepo = new InventoryTransactionRepository(factory);
                var stockService = new StockService(stockRepo, itemRepo);

                // Normal: 10 > 5
                long idNorm = itemService.SaveItem(new Item { SKU = "NORM", Name = "Norm", MinStockLevel = 5, PurchasePrice = 10, SellingPrice = 20, IsActive = true });
                stockService.StockIn(new StockOperationRequest { ItemID = idNorm, Quantity = 10 });

                // Low: 4 <= 5 (and > 0)
                long idLow = itemService.SaveItem(new Item { SKU = "LOW", Name = "Low", MinStockLevel = 5, PurchasePrice = 10, SellingPrice = 20, IsActive = true });
                stockService.StockIn(new StockOperationRequest { ItemID = idLow, Quantity = 4 });

                // Out: 0
                itemService.SaveItem(new Item { SKU = "OUT", Name = "Out", MinStockLevel = 5, PurchasePrice = 10, SellingPrice = 20, IsActive = true });

                var results = itemService.SearchItems(new ItemSearchCriteria
                {
                    StockStatus = StockFilterStatus.LowStock
                }).ToList();

                Assert(results.Count == 1, string.Format("Low stock filter should return 1 item, returned {0}", results.Count));
                Assert(results[0].SKU == "LOW", "Expected LOW item.");
            }
            finally
            {
                CleanupTempDb(dbPath);
            }
        }

        private void Test_Search_FilterByStockStatus_OutOfStock()
        {
            string dbPath;
            var factory = CreateTestDatabase(out dbPath);
            try
            {
                var itemRepo = new ItemRepository(factory);
                var catRepo = new CategoryRepository(factory);
                var itemService = new ItemService(itemRepo, catRepo);
                var stockRepo = new InventoryTransactionRepository(factory);
                var stockService = new StockService(stockRepo, itemRepo);

                // Normal
                long idNorm = itemService.SaveItem(new Item { SKU = "NORM", Name = "Norm", MinStockLevel = 5, PurchasePrice = 10, SellingPrice = 20, IsActive = true });
                stockService.StockIn(new StockOperationRequest { ItemID = idNorm, Quantity = 10 });

                // Out 1 (MinStock 5, Stock 0)
                itemService.SaveItem(new Item { SKU = "OUT1", Name = "Out 1", MinStockLevel = 5, PurchasePrice = 10, SellingPrice = 20, IsActive = true });

                // Out 2 (MinStock 0, Stock 0)
                itemService.SaveItem(new Item { SKU = "OUT2", Name = "Out 2", MinStockLevel = 0, PurchasePrice = 10, SellingPrice = 20, IsActive = true });

                var results = itemService.SearchItems(new ItemSearchCriteria
                {
                    StockStatus = StockFilterStatus.OutOfStock
                }).ToList();

                Assert(results.Count == 2, string.Format("Out of stock filter should return 2 items, returned {0}", results.Count));
                Assert(results.Any(x => x.SKU == "OUT1"), "Expected OUT1.");
                Assert(results.Any(x => x.SKU == "OUT2"), "Expected OUT2.");
            }
            finally
            {
                CleanupTempDb(dbPath);
            }
        }

        private void Test_Search_FilterByGender_ExactAndCaseInsensitive()
        {
            string dbPath;
            var factory = CreateTestDatabase(out dbPath);
            try
            {
                var itemRepo = new ItemRepository(factory);
                var catRepo = new CategoryRepository(factory);
                var itemService = new ItemService(itemRepo, catRepo);

                itemService.SaveItem(new Item { SKU = "MEN-1", Name = "Shirt Men", Gender = "Men", PurchasePrice = 10, SellingPrice = 20, IsActive = true });
                itemService.SaveItem(new Item { SKU = "WOMEN-1", Name = "Dress Women", Gender = "Women", PurchasePrice = 10, SellingPrice = 20, IsActive = true });
                itemService.SaveItem(new Item { SKU = "KIDS-1", Name = "Shorts Kids", Gender = "Kids", PurchasePrice = 10, SellingPrice = 20, IsActive = true });

                var resultsMen = itemService.SearchItems(new ItemSearchCriteria { Gender = "men" }).ToList();
                Assert(resultsMen.Count == 1, "Expected 1 Men item.");
                Assert(resultsMen[0].SKU == "MEN-1", "Expected MEN-1.");

                var resultsWomen = itemService.SearchItems(new ItemSearchCriteria { Gender = "Women" }).ToList();
                Assert(resultsWomen.Count == 1, "Expected 1 Women item.");
                Assert(resultsWomen[0].SKU == "WOMEN-1", "Expected WOMEN-1.");
            }
            finally
            {
                CleanupTempDb(dbPath);
            }
        }

        private void Test_Search_FilterByGender_NoneUnspecified()
        {
            string dbPath;
            var factory = CreateTestDatabase(out dbPath);
            try
            {
                var itemRepo = new ItemRepository(factory);
                var catRepo = new CategoryRepository(factory);
                var itemService = new ItemService(itemRepo, catRepo);

                itemService.SaveItem(new Item { SKU = "NONE-1", Name = "No Gender Specified", Gender = GenderOptions.None, PurchasePrice = 10, SellingPrice = 20, IsActive = true });
                itemService.SaveItem(new Item { SKU = "UNISEX-1", Name = "Unisex Cap", Gender = GenderOptions.Unisex, PurchasePrice = 10, SellingPrice = 20, IsActive = true });

                var results = itemService.SearchItems(new ItemSearchCriteria { Gender = GenderOptions.None }).ToList();
                Assert(results.Count == 1, "Expected 1 item for None / Unspecified.");
                Assert(results[0].SKU == "NONE-1", "Expected NONE-1.");
            }
            finally
            {
                CleanupTempDb(dbPath);
            }
        }

        private void Test_Search_CombinedFilters_StockStatus_Gender_Category_SearchText()
        {
            string dbPath;
            var factory = CreateTestDatabase(out dbPath);
            try
            {
                var itemRepo = new ItemRepository(factory);
                var catRepo = new CategoryRepository(factory);
                var itemService = new ItemService(itemRepo, catRepo);
                var stockRepo = new InventoryTransactionRepository(factory);
                var stockService = new StockService(stockRepo, itemRepo);

                long catShirts = catRepo.Create(new Category { CategoryName = "Shirts" });
                long catPants = catRepo.Create(new Category { CategoryName = "Pants" });

                // Item 1: Shirts, Men, Low Stock (Stock 3, Min 5), Name "Oxford Shirt"
                long id1 = itemService.SaveItem(new Item
                {
                    SKU = "SHIRT-M-01",
                    Name = "Oxford Shirt",
                    CategoryID = catShirts,
                    Gender = GenderOptions.Men,
                    MinStockLevel = 5,
                    PurchasePrice = 10,
                    SellingPrice = 20,
                    IsActive = true
                });
                stockService.StockIn(new StockOperationRequest { ItemID = id1, Quantity = 3 });

                // Item 2: Shirts, Men, Normal Stock (Stock 10, Min 5), Name "Flannel Shirt"
                long id2 = itemService.SaveItem(new Item
                {
                    SKU = "SHIRT-M-02",
                    Name = "Flannel Shirt",
                    CategoryID = catShirts,
                    Gender = GenderOptions.Men,
                    MinStockLevel = 5,
                    PurchasePrice = 10,
                    SellingPrice = 20,
                    IsActive = true
                });
                stockService.StockIn(new StockOperationRequest { ItemID = id2, Quantity = 10 });

                // Item 3: Pants, Men, Low Stock (Stock 2, Min 5), Name "Chino Pants"
                long id3 = itemService.SaveItem(new Item
                {
                    SKU = "PANTS-M-01",
                    Name = "Chino Pants",
                    CategoryID = catPants,
                    Gender = GenderOptions.Men,
                    MinStockLevel = 5,
                    PurchasePrice = 10,
                    SellingPrice = 20,
                    IsActive = true
                });
                stockService.StockIn(new StockOperationRequest { ItemID = id3, Quantity = 2 });

                // Test combined filter: Category = Shirts, Gender = Men, StockStatus = LowStock, SearchText = "Oxford"
                var criteria = new ItemSearchCriteria
                {
                    CategoryID = catShirts,
                    Gender = GenderOptions.Men,
                    StockStatus = StockFilterStatus.LowStock,
                    SearchText = "Oxford"
                };

                var results = itemService.SearchItems(criteria).ToList();
                Assert(results.Count == 1, string.Format("Expected 1 result for combined filter, got {0}", results.Count));
                Assert(results[0].SKU == "SHIRT-M-01", "Expected SHIRT-M-01.");

                // Test combined filter with no matches (e.g. searching "Silk")
                criteria.SearchText = "Silk";
                var zeroResults = itemService.SearchItems(criteria).ToList();
                Assert(zeroResults.Count == 0, "Expected 0 results for non-matching search term.");
            }
            finally
            {
                CleanupTempDb(dbPath);
            }
        }

        private void Test_DashboardSummary_UpdatesAfterStockOperations()
        {
            string dbPath;
            var factory = CreateTestDatabase(out dbPath);
            try
            {
                var itemRepo = new ItemRepository(factory);
                var catRepo = new CategoryRepository(factory);
                var itemService = new ItemService(itemRepo, catRepo);
                var stockRepo = new InventoryTransactionRepository(factory);
                var stockService = new StockService(stockRepo, itemRepo);

                long id = itemService.SaveItem(new Item
                {
                    SKU = "SKU-DYN",
                    Name = "Dynamic Stock Item",
                    MinStockLevel = 10,
                    PurchasePrice = 10,
                    SellingPrice = 20,
                    IsActive = true
                });

                // Initially out of stock: stock = 0
                var s1 = itemService.GetDashboardSummary();
                Assert(s1.TotalActiveProducts == 1, "1 active product.");
                Assert(s1.TotalUnitsInStock == 0, "0 units in stock.");
                Assert(s1.OutOfStockCount == 1, "1 out of stock.");
                Assert(s1.LowStockCount == 0, "0 low stock.");

                // Stock IN 5 -> Low Stock (5 <= 10)
                stockService.StockIn(new StockOperationRequest
                {
                    ItemID = id,
                    Quantity = 5
                });
                var s2 = itemService.GetDashboardSummary();
                Assert(s2.TotalUnitsInStock == 5, "5 units in stock.");
                Assert(s2.OutOfStockCount == 0, "0 out of stock.");
                Assert(s2.LowStockCount == 1, "1 low stock.");

                // Stock IN 10 -> Normal Stock (15 > 10)
                stockService.StockIn(new StockOperationRequest
                {
                    ItemID = id,
                    Quantity = 10
                });
                var s3 = itemService.GetDashboardSummary();
                Assert(s3.TotalUnitsInStock == 15, "15 units in stock.");
                Assert(s3.OutOfStockCount == 0, "0 out of stock.");
                Assert(s3.LowStockCount == 0, "0 low stock.");

                // Stock OUT 15 -> Back to Out of Stock (0)
                stockService.StockOut(new StockOperationRequest
                {
                    ItemID = id,
                    Quantity = 15
                });
                var s4 = itemService.GetDashboardSummary();
                Assert(s4.TotalUnitsInStock == 0, "0 units in stock.");
                Assert(s4.OutOfStockCount == 1, "1 out of stock.");
                Assert(s4.LowStockCount == 0, "0 low stock.");
            }
            finally
            {
                CleanupTempDb(dbPath);
            }
        }
    }
}
