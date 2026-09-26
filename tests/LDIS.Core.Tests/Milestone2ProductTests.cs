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
    public class Milestone2ProductTests
    {
        private static string CreateTempDbPath()
        {
            string tempDir = Path.Combine(Path.GetTempPath(), "LDIS_M2Tests_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(tempDir);
            return Path.Combine(tempDir, "test_m2.db");
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
            RunTest("Test_Category_CreateAndRetrieve", Test_Category_CreateAndRetrieve);
            RunTest("Test_Category_UniquenessEnforcement", Test_Category_UniquenessEnforcement);
            RunTest("Test_Category_InUseDetectionAndDeletionBlock", Test_Category_InUseDetectionAndDeletionBlock);
            RunTest("Test_Item_CreateAndRetrieve_DefaultStockIsZero", Test_Item_CreateAndRetrieve_DefaultStockIsZero);
            RunTest("Test_Item_SkuUniquenessEnforcement", Test_Item_SkuUniquenessEnforcement);
            RunTest("Test_Item_UpdatePreservesCurrentStock", Test_Item_UpdatePreservesCurrentStock);
            RunTest("Test_Item_DeactivateAndReactivate", Test_Item_DeactivateAndReactivate);
            RunTest("Test_Item_SearchMultiFieldKeyword", Test_Item_SearchMultiFieldKeyword);
            RunTest("Test_Item_SearchCategoryAndStatusFilters", Test_Item_SearchCategoryAndStatusFilters);
            RunTest("Test_ItemService_ValidationRules_PricesAndNames", Test_ItemService_ValidationRules_PricesAndNames);
            RunTest("Test_ItemService_ControlledGenderValidation", Test_ItemService_ControlledGenderValidation);
            RunTest("Test_Item_DistinctAttributesRetrieval", Test_Item_DistinctAttributesRetrieval);
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

        private void Test_Category_CreateAndRetrieve()
        {
            string dbPath;
            var factory = CreateTestDatabase(out dbPath);
            try
            {
                var repo = new CategoryRepository(factory);
                var cat = new Category { CategoryName = "Running Shoes" };
                long id = repo.Create(cat);

                if (id <= 0) throw new Exception("Category ID should be > 0.");

                var fetched = repo.GetById(id);
                if (fetched == null || fetched.CategoryName != "Running Shoes")
                {
                    throw new Exception("Failed to retrieve created category by ID.");
                }

                var fetchedByName = repo.GetByName("running shoes");
                if (fetchedByName == null || fetchedByName.CategoryID != id)
                {
                    throw new Exception("Case-insensitive category name lookup failed.");
                }

                cat.CategoryName = "Trail Running Shoes";
                repo.Update(cat);

                var updated = repo.GetById(id);
                if (updated.CategoryName != "Trail Running Shoes")
                {
                    throw new Exception("Failed to update category name.");
                }
            }
            finally
            {
                CleanupTempDb(dbPath);
            }
        }

        private void Test_Category_UniquenessEnforcement()
        {
            string dbPath;
            var factory = CreateTestDatabase(out dbPath);
            try
            {
                var repo = new CategoryRepository(factory);
                var service = new CategoryService(repo);

                service.CreateCategory("Apparel");

                try
                {
                    service.CreateCategory("apparel");
                    throw new Exception("Expected duplicate category validation exception was not thrown.");
                }
                catch (InvalidOperationException)
                {
                    // Expected
                }
            }
            finally
            {
                CleanupTempDb(dbPath);
            }
        }

        private void Test_Category_InUseDetectionAndDeletionBlock()
        {
            string dbPath;
            var factory = CreateTestDatabase(out dbPath);
            try
            {
                var catRepo = new CategoryRepository(factory);
                var itemRepo = new ItemRepository(factory);
                var catService = new CategoryService(catRepo);

                var cat = catService.CreateCategory("Boots");
                var item = new Item
                {
                    SKU = "BT-001",
                    Name = "Hiking Boot",
                    CategoryID = cat.CategoryID,
                    PurchasePrice = 250000,
                    SellingPrice = 350000,
                    MinStockLevel = 5,
                    IsActive = true
                };
                itemRepo.Create(item);

                string reason;
                bool canDelete = catService.CanDeleteCategory(cat.CategoryID, out reason);
                if (canDelete)
                {
                    throw new Exception("Category in use should NOT be deletable.");
                }

                try
                {
                    catService.DeleteCategory(cat.CategoryID);
                    throw new Exception("Expected exception when deleting in-use category was not thrown.");
                }
                catch (InvalidOperationException)
                {
                    // Expected
                }
            }
            finally
            {
                CleanupTempDb(dbPath);
            }
        }

        private void Test_Item_CreateAndRetrieve_DefaultStockIsZero()
        {
            string dbPath;
            var factory = CreateTestDatabase(out dbPath);
            try
            {
                var repo = new ItemRepository(factory);
                var item = new Item
                {
                    SKU = "AM270-BLK-42",
                    Name = "Air Max 270",
                    Brand = "Nike",
                    Color = "Black",
                    Size = "42",
                    Gender = GenderOptions.Men,
                    PurchasePrice = 1200000,
                    SellingPrice = 1750000,
                    MinStockLevel = 10,
                    CurrentStock = 999, // Should be ignored and forced to 0 on insert
                    IsActive = true
                };

                long id = repo.Create(item);
                var retrieved = repo.GetById(id);

                if (retrieved == null) throw new Exception("Failed to retrieve created item.");
                if (retrieved.SKU != "AM270-BLK-42") throw new Exception("SKU mismatch.");
                if (retrieved.CurrentStock != 0) throw new Exception("Initial CurrentStock must be 0.");
                if (retrieved.PurchasePrice != 1200000) throw new Exception("Purchase price mismatch.");
                if (retrieved.SellingPrice != 1750000) throw new Exception("Selling price mismatch.");
            }
            finally
            {
                CleanupTempDb(dbPath);
            }
        }

        private void Test_Item_SkuUniquenessEnforcement()
        {
            string dbPath;
            var factory = CreateTestDatabase(out dbPath);
            try
            {
                var repo = new ItemRepository(factory);
                var item1 = new Item
                {
                    SKU = "SKU-UNIQUE-1",
                    Name = "Item 1",
                    PurchasePrice = 10000,
                    SellingPrice = 15000
                };
                repo.Create(item1);

                if (!repo.ExistsSku("sku-unique-1"))
                {
                    throw new Exception("ExistsSku should return true for existing SKU case-insensitively.");
                }

                // Should not report exists when excluding self
                if (repo.ExistsSku("sku-unique-1", item1.ItemID))
                {
                    throw new Exception("ExistsSku should return false when excluding self item ID.");
                }

                var item2 = new Item
                {
                    SKU = "SKU-UNIQUE-1",
                    Name = "Item 2",
                    PurchasePrice = 20000,
                    SellingPrice = 30000
                };

                try
                {
                    repo.Create(item2);
                    throw new Exception("Expected SQLite unique constraint error for duplicate SKU was not thrown.");
                }
                catch (System.Data.SQLite.SQLiteException)
                {
                    // Expected SQLite UNIQUE constraint exception
                }

                // Verify case-insensitive check in service layer
                var service = new ItemService(repo, new CategoryRepository(factory));
                var item3 = new Item
                {
                    SKU = "sku-unique-1",
                    Name = "Item 3",
                    PurchasePrice = 20000,
                    SellingPrice = 30000
                };

                var validation = service.ValidateItem(item3, true);
                if (validation.IsValid || !validation.Errors.Any(e => e.Contains("already in use")))
                {
                    throw new Exception("Service layer failed to detect case-insensitive duplicate SKU.");
                }
            }
            finally
            {
                CleanupTempDb(dbPath);
            }
        }

        private void Test_Item_UpdatePreservesCurrentStock()
        {
            string dbPath;
            var factory = CreateTestDatabase(out dbPath);
            try
            {
                var repo = new ItemRepository(factory);
                var item = new Item
                {
                    SKU = "TEST-STOCK-PRESERVE",
                    Name = "Stock Preserve Item",
                    PurchasePrice = 10000,
                    SellingPrice = 20000
                };
                long id = repo.Create(item);

                // Directly simulate an existing stock level (as would be set by Milestone 3 stock-in)
                using (var conn = factory.CreateOpenConnection())
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "UPDATE Items SET CurrentStock = 45 WHERE ItemID = @id;";
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.ExecuteNonQuery();
                }

                // Update item metadata
                var toUpdate = repo.GetById(id);
                toUpdate.Name = "Updated Name";
                toUpdate.SellingPrice = 25000;
                toUpdate.CurrentStock = 0; // Attempting to change stock via Item model should NOT alter DB CurrentStock

                repo.Update(toUpdate);

                var reloaded = repo.GetById(id);
                if (reloaded.CurrentStock != 45)
                {
                    throw new Exception(string.Format("Item update corrupted CurrentStock! Expected 45, got {0}", reloaded.CurrentStock));
                }
                if (reloaded.Name != "Updated Name" || reloaded.SellingPrice != 25000)
                {
                    throw new Exception("Item metadata was not properly updated.");
                }
            }
            finally
            {
                CleanupTempDb(dbPath);
            }
        }

        private void Test_Item_DeactivateAndReactivate()
        {
            string dbPath;
            var factory = CreateTestDatabase(out dbPath);
            try
            {
                var repo = new ItemRepository(factory);
                var service = new ItemService(repo, new CategoryRepository(factory));

                var item = new Item
                {
                    SKU = "DEACT-001",
                    Name = "Seasonal Item",
                    PurchasePrice = 1000,
                    SellingPrice = 2000,
                    IsActive = true
                };
                long id = service.SaveItem(item);

                service.DeactivateItem(id);
                var deactivated = repo.GetById(id);
                if (deactivated.IsActive)
                {
                    throw new Exception("Item should be inactive after deactivation.");
                }

                service.ActivateItem(id);
                var reactivated = repo.GetById(id);
                if (!reactivated.IsActive)
                {
                    throw new Exception("Item should be active after reactivation.");
                }
            }
            finally
            {
                CleanupTempDb(dbPath);
            }
        }

        private void Test_Item_SearchMultiFieldKeyword()
        {
            string dbPath;
            var factory = CreateTestDatabase(out dbPath);
            try
            {
                var repo = new ItemRepository(factory);
                repo.Create(new Item { SKU = "NK-AIR-01", Name = "Air Max", Brand = "Nike", Color = "Red", Size = "42", IsActive = true });
                repo.Create(new Item { SKU = "AD-ULT-02", Name = "Ultraboost", Brand = "Adidas", Color = "Black", Size = "43", IsActive = true });
                repo.Create(new Item { SKU = "PM-SUE-03", Name = "Suede Classic", Brand = "Puma", Color = "Red", Size = "41", IsActive = true });

                // Search by brand
                var byBrand = repo.Search(new ItemSearchCriteria { SearchText = "Nike" }).ToList();
                if (byBrand.Count != 1 || byBrand[0].SKU != "NK-AIR-01")
                {
                    throw new Exception("Search by brand failed.");
                }

                // Search by color across multiple items
                var byColor = repo.Search(new ItemSearchCriteria { SearchText = "Red" }).ToList();
                if (byColor.Count != 2)
                {
                    throw new Exception("Search by color should return 2 items.");
                }

                // Search by size
                var bySize = repo.Search(new ItemSearchCriteria { SearchText = "43" }).ToList();
                if (bySize.Count != 1 || bySize[0].SKU != "AD-ULT-02")
                {
                    throw new Exception("Search by size failed.");
                }
            }
            finally
            {
                CleanupTempDb(dbPath);
            }
        }

        private void Test_Item_SearchCategoryAndStatusFilters()
        {
            string dbPath;
            var factory = CreateTestDatabase(out dbPath);
            try
            {
                var catRepo = new CategoryRepository(factory);
                long cat1 = catRepo.Create(new Category { CategoryName = "Footwear" });
                long cat2 = catRepo.Create(new Category { CategoryName = "Accessories" });

                var repo = new ItemRepository(factory);
                repo.Create(new Item { SKU = "FW-01", Name = "Shoe Active", CategoryID = cat1, IsActive = true });
                repo.Create(new Item { SKU = "FW-02", Name = "Shoe Inactive", CategoryID = cat1, IsActive = false });
                repo.Create(new Item { SKU = "AC-01", Name = "Sock Active", CategoryID = cat2, IsActive = true });

                // Default active only
                var activeFootwear = repo.Search(new ItemSearchCriteria { CategoryID = cat1, ActiveStatus = ActiveFilterStatus.ActiveOnly }).ToList();
                if (activeFootwear.Count != 1 || activeFootwear[0].SKU != "FW-01")
                {
                    throw new Exception("Active footwear filter returned unexpected results.");
                }

                // Inactive only
                var inactiveFootwear = repo.Search(new ItemSearchCriteria { CategoryID = cat1, ActiveStatus = ActiveFilterStatus.InactiveOnly }).ToList();
                if (inactiveFootwear.Count != 1 || inactiveFootwear[0].SKU != "FW-02")
                {
                    throw new Exception("Inactive footwear filter returned unexpected results.");
                }

                // All statuses
                var allFootwear = repo.Search(new ItemSearchCriteria { CategoryID = cat1, ActiveStatus = ActiveFilterStatus.All }).ToList();
                if (allFootwear.Count != 2)
                {
                    throw new Exception("All footwear filter should return 2 items.");
                }
            }
            finally
            {
                CleanupTempDb(dbPath);
            }
        }

        private void Test_ItemService_ValidationRules_PricesAndNames()
        {
            string dbPath;
            var factory = CreateTestDatabase(out dbPath);
            try
            {
                var repo = new ItemRepository(factory);
                var catRepo = new CategoryRepository(factory);
                var service = new ItemService(repo, catRepo);

                // Missing SKU
                var res1 = service.ValidateItem(new Item { SKU = "", Name = "Valid Name" }, true);
                if (res1.IsValid || !res1.Errors.Any(e => e.Contains("SKU is required")))
                {
                    throw new Exception("Validation failed to detect missing SKU.");
                }

                // Missing Name
                var res2 = service.ValidateItem(new Item { SKU = "VALID-SKU", Name = "" }, true);
                if (res2.IsValid || !res2.Errors.Any(e => e.Contains("name is required")))
                {
                    throw new Exception("Validation failed to detect missing Name.");
                }

                // Negative prices
                var res3 = service.ValidateItem(new Item { SKU = "VALID-SKU", Name = "Valid Name", PurchasePrice = -500 }, true);
                if (res3.IsValid || !res3.Errors.Any(e => e.Contains("Purchase price cannot be negative")))
                {
                    throw new Exception("Validation failed to detect negative purchase price.");
                }

                var res4 = service.ValidateItem(new Item { SKU = "VALID-SKU", Name = "Valid Name", SellingPrice = -100 }, true);
                if (res4.IsValid || !res4.Errors.Any(e => e.Contains("Selling price cannot be negative")))
                {
                    throw new Exception("Validation failed to detect negative selling price.");
                }
            }
            finally
            {
                CleanupTempDb(dbPath);
            }
        }

        private void Test_ItemService_ControlledGenderValidation()
        {
            string dbPath;
            var factory = CreateTestDatabase(out dbPath);
            try
            {
                var repo = new ItemRepository(factory);
                var catRepo = new CategoryRepository(factory);
                var service = new ItemService(repo, catRepo);

                // Valid predefined genders
                string[] validGenders = new string[] { "Unisex", "Men", "Women", "Kids", "None / Unspecified", null, "" };
                foreach (var g in validGenders)
                {
                    var res = service.ValidateItem(new Item { SKU = "SKU-" + Guid.NewGuid().ToString("N").Substring(0, 6), Name = "Test", Gender = g }, true);
                    if (!res.IsValid)
                    {
                        throw new Exception(string.Format("Valid gender '{0}' was rejected: {1}", g, res.ErrorMessage));
                    }
                }

                // Arbitrary / custom gender must be rejected
                var invalidRes = service.ValidateItem(new Item { SKU = "SKU-INV", Name = "Test", Gender = "CustomGenderOption" }, true);
                if (invalidRes.IsValid || !invalidRes.Errors.Any(e => e.Contains("Invalid gender")))
                {
                    throw new Exception("Service failed to reject arbitrary gender option.");
                }
            }
            finally
            {
                CleanupTempDb(dbPath);
            }
        }

        private void Test_Item_DistinctAttributesRetrieval()
        {
            string dbPath;
            var factory = CreateTestDatabase(out dbPath);
            try
            {
                var repo = new ItemRepository(factory);
                repo.Create(new Item { SKU = "IT-1", Name = "Item 1", Brand = "Nike", Color = "Red", Size = "M" });
                repo.Create(new Item { SKU = "IT-2", Name = "Item 2", Brand = "Adidas", Color = "Black", Size = "L" });
                repo.Create(new Item { SKU = "IT-3", Name = "Item 3", Brand = "Nike", Color = "White", Size = "M" });

                var distinct = repo.GetDistinctAttributes();

                if (distinct.Brands.Count != 2 || !distinct.Brands.Contains("Nike") || !distinct.Brands.Contains("Adidas"))
                {
                    throw new Exception("Distinct brands failed.");
                }
                if (distinct.Colors.Count != 3 || !distinct.Colors.Contains("Red") || !distinct.Colors.Contains("Black") || !distinct.Colors.Contains("White"))
                {
                    throw new Exception("Distinct colors failed.");
                }
                if (distinct.Sizes.Count != 2 || !distinct.Sizes.Contains("M") || !distinct.Sizes.Contains("L"))
                {
                    throw new Exception("Distinct sizes failed.");
                }
            }
            finally
            {
                CleanupTempDb(dbPath);
            }
        }
    }
}
