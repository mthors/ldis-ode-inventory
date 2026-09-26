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
    public class Milestone3StockOperationTests
    {
        private static string CreateTempDbPath()
        {
            string tempDir = Path.Combine(Path.GetTempPath(), "LDIS_M3Tests_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(tempDir);
            return Path.Combine(tempDir, "test_m3.db");
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
            RunTest("Test_StockIn_IncreasesCurrentStock_CreatesTransaction", Test_StockIn_IncreasesCurrentStock_CreatesTransaction);
            RunTest("Test_StockOut_DecreasesCurrentStock_CreatesTransaction", Test_StockOut_DecreasesCurrentStock_CreatesTransaction);
            RunTest("Test_StockOut_ExceedingCurrentStock_IsRejected", Test_StockOut_ExceedingCurrentStock_IsRejected);
            RunTest("Test_StockAdjustment_Positive_IncreasesStock", Test_StockAdjustment_Positive_IncreasesStock);
            RunTest("Test_StockAdjustment_Negative_DecreasesStock", Test_StockAdjustment_Negative_DecreasesStock);
            RunTest("Test_StockAdjustment_NegativeResultingStock_IsRejected", Test_StockAdjustment_NegativeResultingStock_IsRejected);
            RunTest("Test_StockAdjustment_RequiresReason", Test_StockAdjustment_RequiresReason);
            RunTest("Test_StockOperation_QuantityValidation_RejectsZeroAndNegativeForInOut", Test_StockOperation_QuantityValidation_RejectsZeroAndNegativeForInOut);
            RunTest("Test_StockOperation_InactiveItem_IsRejected", Test_StockOperation_InactiveItem_IsRejected);
            RunTest("Test_StockOperation_NonexistentItem_IsRejected", Test_StockOperation_NonexistentItem_IsRejected);
            RunTest("Test_AtomicDatabaseRollback_OnConditionalUpdateFailure", Test_AtomicDatabaseRollback_OnConditionalUpdateFailure);
            RunTest("Test_FailedOperation_LeavesStockAndHistoryUnchanged", Test_FailedOperation_LeavesStockAndHistoryUnchanged);
            RunTest("Test_SequentialStockOperations_ProduceCorrectFinalStockAndAuditHistory", Test_SequentialStockOperations_ProduceCorrectFinalStockAndAuditHistory);
            RunTest("Test_TransactionHistory_FilteringAndSearch", Test_TransactionHistory_FilteringAndSearch);
            RunTest("Test_TransactionData_ContainsExpectedValues", Test_TransactionData_ContainsExpectedValues);
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

        private static Item CreateSampleItem(IItemRepository itemRepo, string sku, string name, int initialStock = 0)
        {
            var item = new Item
            {
                SKU = sku,
                Name = name,
                PurchasePrice = 50000,
                SellingPrice = 75000,
                MinStockLevel = 5,
                IsActive = true
            };
            long id = itemRepo.Create(item);
            return itemRepo.GetById(id);
        }

        private void Test_StockIn_IncreasesCurrentStock_CreatesTransaction()
        {
            string dbPath;
            var factory = CreateTestDatabase(out dbPath);
            try
            {
                var itemRepo = new ItemRepository(factory);
                var transRepo = new InventoryTransactionRepository(factory);
                var stockService = new StockService(transRepo, itemRepo);

                var item = CreateSampleItem(itemRepo, "IN-001", "Stock In Product");
                Assert(item.CurrentStock == 0, "Initial stock should be 0.");

                long transId = stockService.StockIn(new StockOperationRequest
                {
                    ItemID = item.ItemID,
                    Quantity = 25,
                    UnitPrice = 50000,
                    ReferenceNumber = "PO-2026-001",
                    SupplierOrCustomer = "PT Supplier Utama",
                    Notes = "Initial batch delivery",
                    CreatedBy = "Admin"
                });

                Assert(transId > 0, "Transaction ID should be > 0.");

                var updatedItem = itemRepo.GetById(item.ItemID);
                Assert(updatedItem.CurrentStock == 25, string.Format("CurrentStock should be 25, got {0}.", updatedItem.CurrentStock));

                var trans = transRepo.GetById(transId);
                Assert(trans != null, "Transaction record was not created.");
                Assert(trans.TransactionType == "IN", "Transaction type should be IN.");
                Assert(trans.Quantity == 25, "Transaction quantity should be 25.");
                Assert(trans.UnitPrice == 50000, "UnitPrice should be 50000.");
                Assert(trans.ReferenceNumber == "PO-2026-001", "ReferenceNumber mismatch.");
                Assert(trans.SupplierOrCustomer == "PT Supplier Utama", "SupplierOrCustomer mismatch.");
            }
            finally
            {
                CleanupTempDb(dbPath);
            }
        }

        private void Test_StockOut_DecreasesCurrentStock_CreatesTransaction()
        {
            string dbPath;
            var factory = CreateTestDatabase(out dbPath);
            try
            {
                var itemRepo = new ItemRepository(factory);
                var transRepo = new InventoryTransactionRepository(factory);
                var stockService = new StockService(transRepo, itemRepo);

                var item = CreateSampleItem(itemRepo, "OUT-001", "Stock Out Product");

                // Stock In 40 first
                stockService.StockIn(new StockOperationRequest
                {
                    ItemID = item.ItemID,
                    Quantity = 40,
                    UnitPrice = 50000
                });

                // Stock Out 15
                long transId = stockService.StockOut(new StockOperationRequest
                {
                    ItemID = item.ItemID,
                    Quantity = 15,
                    UnitPrice = 75000,
                    ReferenceNumber = "INV-001",
                    SupplierOrCustomer = "Toko Sepatu Berkah",
                    Notes = "Sales order fulfillment",
                    CreatedBy = "Kasir1"
                });

                Assert(transId > 0, "Transaction ID should be > 0.");

                var updatedItem = itemRepo.GetById(item.ItemID);
                Assert(updatedItem.CurrentStock == 25, string.Format("CurrentStock should be 25 (40 - 15), got {0}.", updatedItem.CurrentStock));

                var trans = transRepo.GetById(transId);
                Assert(trans != null, "Transaction record was not created.");
                Assert(trans.TransactionType == "OUT", "Transaction type should be OUT.");
                Assert(trans.Quantity == 15, "Transaction quantity should be 15.");
                Assert(trans.UnitPrice == 75000, "UnitPrice should be 75000.");
                Assert(trans.ReferenceNumber == "INV-001", "ReferenceNumber mismatch.");
                Assert(trans.SupplierOrCustomer == "Toko Sepatu Berkah", "Customer mismatch.");
            }
            finally
            {
                CleanupTempDb(dbPath);
            }
        }

        private void Test_StockOut_ExceedingCurrentStock_IsRejected()
        {
            string dbPath;
            var factory = CreateTestDatabase(out dbPath);
            try
            {
                var itemRepo = new ItemRepository(factory);
                var transRepo = new InventoryTransactionRepository(factory);
                var stockService = new StockService(transRepo, itemRepo);

                var item = CreateSampleItem(itemRepo, "OUT-OVR", "Stock Out Over Limit");

                // Stock In 10
                stockService.StockIn(new StockOperationRequest
                {
                    ItemID = item.ItemID,
                    Quantity = 10,
                    UnitPrice = 50000
                });

                // Attempt to Stock Out 11 (exceeds current stock 10)
                bool threwException = false;
                try
                {
                    stockService.StockOut(new StockOperationRequest
                    {
                        ItemID = item.ItemID,
                        Quantity = 11,
                        UnitPrice = 75000
                    });
                }
                catch (InvalidOperationException)
                {
                    threwException = true;
                }

                Assert(threwException, "Expected InvalidOperationException when Stock Out exceeds CurrentStock.");

                // Verify stock remains exactly 10
                var reloadedItem = itemRepo.GetById(item.ItemID);
                Assert(reloadedItem.CurrentStock == 10, string.Format("CurrentStock should remain 10, got {0}.", reloadedItem.CurrentStock));

                // Verify only 1 transaction exists (the Stock In)
                var history = transRepo.SearchTransactions(new TransactionSearchCriteria { ItemID = item.ItemID }).ToList();
                Assert(history.Count == 1, string.Format("Expected exactly 1 transaction in history, got {0}.", history.Count));
            }
            finally
            {
                CleanupTempDb(dbPath);
            }
        }

        private void Test_StockAdjustment_Positive_IncreasesStock()
        {
            string dbPath;
            var factory = CreateTestDatabase(out dbPath);
            try
            {
                var itemRepo = new ItemRepository(factory);
                var transRepo = new InventoryTransactionRepository(factory);
                var stockService = new StockService(transRepo, itemRepo);

                var item = CreateSampleItem(itemRepo, "ADJ-POS", "Positive Adjustment Product");

                // Stock In 20
                stockService.StockIn(new StockOperationRequest
                {
                    ItemID = item.ItemID,
                    Quantity = 20,
                    UnitPrice = 50000
                });

                // Positive adjustment (+5)
                long transId = stockService.StockAdjustment(new StockOperationRequest
                {
                    ItemID = item.ItemID,
                    Quantity = 5,
                    Reason = "Physical stock count surplus found on shelf",
                    Notes = "Found unopened carton behind warehouse rack A",
                    CreatedBy = "AuditTeam"
                });

                Assert(transId > 0, "Transaction ID should be > 0.");

                var updatedItem = itemRepo.GetById(item.ItemID);
                Assert(updatedItem.CurrentStock == 25, string.Format("CurrentStock should be 25 (20 + 5), got {0}.", updatedItem.CurrentStock));

                var trans = transRepo.GetById(transId);
                Assert(trans != null, "Transaction record was not created.");
                Assert(trans.TransactionType == "ADJUSTMENT", "Transaction type should be ADJUSTMENT.");
                Assert(trans.Quantity == 5, "Transaction quantity should be 5.");
                Assert(trans.Reason == "Physical stock count surplus found on shelf", "Reason mismatch.");
            }
            finally
            {
                CleanupTempDb(dbPath);
            }
        }

        private void Test_StockAdjustment_Negative_DecreasesStock()
        {
            string dbPath;
            var factory = CreateTestDatabase(out dbPath);
            try
            {
                var itemRepo = new ItemRepository(factory);
                var transRepo = new InventoryTransactionRepository(factory);
                var stockService = new StockService(transRepo, itemRepo);

                var item = CreateSampleItem(itemRepo, "ADJ-NEG", "Negative Adjustment Product");

                // Stock In 25
                stockService.StockIn(new StockOperationRequest
                {
                    ItemID = item.ItemID,
                    Quantity = 25,
                    UnitPrice = 50000
                });

                // Negative adjustment (-3)
                long transId = stockService.StockAdjustment(new StockOperationRequest
                {
                    ItemID = item.ItemID,
                    Quantity = -3,
                    Reason = "Physical stock count - damaged packaging during handling",
                    Notes = "Water damaged in storage area B",
                    CreatedBy = "WarehouseLead"
                });

                Assert(transId > 0, "Transaction ID should be > 0.");

                var updatedItem = itemRepo.GetById(item.ItemID);
                Assert(updatedItem.CurrentStock == 22, string.Format("CurrentStock should be 22 (25 - 3), got {0}.", updatedItem.CurrentStock));

                var trans = transRepo.GetById(transId);
                Assert(trans != null, "Transaction record was not created.");
                Assert(trans.TransactionType == "ADJUSTMENT", "Transaction type should be ADJUSTMENT.");
                Assert(trans.Quantity == -3, "Transaction quantity should be -3.");
                Assert(trans.Reason.Contains("damaged packaging"), "Reason mismatch.");
            }
            finally
            {
                CleanupTempDb(dbPath);
            }
        }

        private void Test_StockAdjustment_NegativeResultingStock_IsRejected()
        {
            string dbPath;
            var factory = CreateTestDatabase(out dbPath);
            try
            {
                var itemRepo = new ItemRepository(factory);
                var transRepo = new InventoryTransactionRepository(factory);
                var stockService = new StockService(transRepo, itemRepo);

                var item = CreateSampleItem(itemRepo, "ADJ-OVR", "Adjustment Below Zero Product");

                // Stock In 5
                stockService.StockIn(new StockOperationRequest
                {
                    ItemID = item.ItemID,
                    Quantity = 5,
                    UnitPrice = 50000
                });

                // Attempt adjustment of -6 (would result in -1 stock)
                bool threwException = false;
                try
                {
                    stockService.StockAdjustment(new StockOperationRequest
                    {
                        ItemID = item.ItemID,
                        Quantity = -6,
                        Reason = "Lost inventory claim"
                    });
                }
                catch (InvalidOperationException)
                {
                    threwException = true;
                }

                Assert(threwException, "Expected InvalidOperationException when adjustment would cause negative stock.");

                var reloadedItem = itemRepo.GetById(item.ItemID);
                Assert(reloadedItem.CurrentStock == 5, string.Format("CurrentStock should remain 5, got {0}.", reloadedItem.CurrentStock));

                var history = transRepo.SearchTransactions(new TransactionSearchCriteria { ItemID = item.ItemID }).ToList();
                Assert(history.Count == 1, "Failed adjustment must not record any transaction history.");
            }
            finally
            {
                CleanupTempDb(dbPath);
            }
        }

        private void Test_StockAdjustment_RequiresReason()
        {
            string dbPath;
            var factory = CreateTestDatabase(out dbPath);
            try
            {
                var itemRepo = new ItemRepository(factory);
                var transRepo = new InventoryTransactionRepository(factory);
                var stockService = new StockService(transRepo, itemRepo);

                var item = CreateSampleItem(itemRepo, "ADJ-RSN", "Reason Test Product");

                stockService.StockIn(new StockOperationRequest
                {
                    ItemID = item.ItemID,
                    Quantity = 10,
                    UnitPrice = 50000
                });

                // Null reason
                bool threwNullReason = false;
                try
                {
                    stockService.StockAdjustment(new StockOperationRequest
                    {
                        ItemID = item.ItemID,
                        Quantity = 2,
                        Reason = null
                    });
                }
                catch (InvalidOperationException ex)
                {
                    threwNullReason = ex.Message.IndexOf("reason", StringComparison.OrdinalIgnoreCase) >= 0;
                }
                Assert(threwNullReason, "Adjustment with null reason should be rejected.");

                // Whitespace reason
                bool threwWhitespaceReason = false;
                try
                {
                    stockService.StockAdjustment(new StockOperationRequest
                    {
                        ItemID = item.ItemID,
                        Quantity = 2,
                        Reason = "   "
                    });
                }
                catch (InvalidOperationException ex)
                {
                    threwWhitespaceReason = ex.Message.IndexOf("reason", StringComparison.OrdinalIgnoreCase) >= 0;
                }
                Assert(threwWhitespaceReason, "Adjustment with whitespace reason should be rejected.");
            }
            finally
            {
                CleanupTempDb(dbPath);
            }
        }

        private void Test_StockOperation_QuantityValidation_RejectsZeroAndNegativeForInOut()
        {
            string dbPath;
            var factory = CreateTestDatabase(out dbPath);
            try
            {
                var itemRepo = new ItemRepository(factory);
                var transRepo = new InventoryTransactionRepository(factory);
                var stockService = new StockService(transRepo, itemRepo);

                var item = CreateSampleItem(itemRepo, "QTY-VAL", "Quantity Validation Product");

                // Stock In with 0
                bool threwZeroIn = false;
                try
                {
                    stockService.StockIn(new StockOperationRequest { ItemID = item.ItemID, Quantity = 0 });
                }
                catch (InvalidOperationException)
                {
                    threwZeroIn = true;
                }
                Assert(threwZeroIn, "Stock In with quantity 0 must be rejected.");

                // Stock In with negative
                bool threwNegIn = false;
                try
                {
                    stockService.StockIn(new StockOperationRequest { ItemID = item.ItemID, Quantity = -5 });
                }
                catch (InvalidOperationException)
                {
                    threwNegIn = true;
                }
                Assert(threwNegIn, "Stock In with negative quantity must be rejected.");

                // Stock Out with 0
                bool threwZeroOut = false;
                try
                {
                    stockService.StockOut(new StockOperationRequest { ItemID = item.ItemID, Quantity = 0 });
                }
                catch (InvalidOperationException)
                {
                    threwZeroOut = true;
                }
                Assert(threwZeroOut, "Stock Out with quantity 0 must be rejected.");

                // Adjustment with 0
                bool threwZeroAdj = false;
                try
                {
                    stockService.StockAdjustment(new StockOperationRequest { ItemID = item.ItemID, Quantity = 0, Reason = "Zero delta" });
                }
                catch (InvalidOperationException)
                {
                    threwZeroAdj = true;
                }
                Assert(threwZeroAdj, "Stock Adjustment with delta 0 must be rejected.");
            }
            finally
            {
                CleanupTempDb(dbPath);
            }
        }

        private void Test_StockOperation_InactiveItem_IsRejected()
        {
            string dbPath;
            var factory = CreateTestDatabase(out dbPath);
            try
            {
                var itemRepo = new ItemRepository(factory);
                var transRepo = new InventoryTransactionRepository(factory);
                var stockService = new StockService(transRepo, itemRepo);

                var item = CreateSampleItem(itemRepo, "INACT-01", "Inactive Product");
                itemRepo.SetActiveStatus(item.ItemID, false);

                bool threwIn = false;
                try
                {
                    stockService.StockIn(new StockOperationRequest { ItemID = item.ItemID, Quantity = 10 });
                }
                catch (InvalidOperationException)
                {
                    threwIn = true;
                }
                Assert(threwIn, "Stock In on inactive item must be rejected.");

                bool threwOut = false;
                try
                {
                    stockService.StockOut(new StockOperationRequest { ItemID = item.ItemID, Quantity = 5 });
                }
                catch (InvalidOperationException)
                {
                    threwOut = true;
                }
                Assert(threwOut, "Stock Out on inactive item must be rejected.");

                bool threwAdj = false;
                try
                {
                    stockService.StockAdjustment(new StockOperationRequest { ItemID = item.ItemID, Quantity = 5, Reason = "Check" });
                }
                catch (InvalidOperationException)
                {
                    threwAdj = true;
                }
                Assert(threwAdj, "Stock Adjustment on inactive item must be rejected.");
            }
            finally
            {
                CleanupTempDb(dbPath);
            }
        }

        private void Test_StockOperation_NonexistentItem_IsRejected()
        {
            string dbPath;
            var factory = CreateTestDatabase(out dbPath);
            try
            {
                var itemRepo = new ItemRepository(factory);
                var transRepo = new InventoryTransactionRepository(factory);
                var stockService = new StockService(transRepo, itemRepo);

                bool threw = false;
                try
                {
                    stockService.StockIn(new StockOperationRequest { ItemID = 999999, Quantity = 10 });
                }
                catch (InvalidOperationException)
                {
                    threw = true;
                }
                Assert(threw, "Stock In on nonexistent item must be rejected.");
            }
            finally
            {
                CleanupTempDb(dbPath);
            }
        }

        private void Test_AtomicDatabaseRollback_OnConditionalUpdateFailure()
        {
            string dbPath;
            var factory = CreateTestDatabase(out dbPath);
            try
            {
                var itemRepo = new ItemRepository(factory);
                var transRepo = new InventoryTransactionRepository(factory);

                var item = CreateSampleItem(itemRepo, "ROLL-001", "Rollback Product");

                // Execute a valid initial Stock IN directly on repository
                var inTrans = new InventoryTransaction
                {
                    ItemID = item.ItemID,
                    TransactionType = "IN",
                    Quantity = 10,
                    UnitPrice = 50000,
                    TransactionDate = DateTime.UtcNow
                };
                transRepo.ExecuteStockOperation(inTrans, 10);

                var itemAfterIn = itemRepo.GetById(item.ItemID);
                Assert(itemAfterIn.CurrentStock == 10, "Stock should be 10.");

                // Now test atomic database rollback by invoking ExecuteStockOperation with a negative delta
                // that passes the initial read check or bypasses service-level validation
                // e.g. delta = -15 where CurrentStock = 10.
                var badTrans = new InventoryTransaction
                {
                    ItemID = item.ItemID,
                    TransactionType = "OUT",
                    Quantity = 15,
                    UnitPrice = 50000,
                    TransactionDate = DateTime.UtcNow
                };

                bool threwException = false;
                try
                {
                    transRepo.ExecuteStockOperation(badTrans, -15);
                }
                catch (InvalidOperationException)
                {
                    threwException = true;
                }

                Assert(threwException, "Expected InvalidOperationException during repository execution.");

                // Verify ATOMIC DATABASE INTEGRITY:
                // 1. CurrentStock must remain strictly unchanged (10)
                var reloadedItem = itemRepo.GetById(item.ItemID);
                Assert(reloadedItem.CurrentStock == 10, string.Format("CurrentStock must remain 10 after rollback, got {0}.", reloadedItem.CurrentStock));

                // 2. InventoryTransactions must have exactly 1 record (the initial IN), NO row for the failed OUT!
                using (var conn = factory.CreateOpenConnection())
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT COUNT(*) FROM InventoryTransactions WHERE ItemID = @ItemID;";
                    cmd.Parameters.AddWithValue("@ItemID", item.ItemID);
                    long count = Convert.ToInt64(cmd.ExecuteScalar());
                    Assert(count == 1, string.Format("Expected 1 transaction in DB, but found {0} (partial record was not rolled back!).", count));
                }
            }
            finally
            {
                CleanupTempDb(dbPath);
            }
        }

        private void Test_FailedOperation_LeavesStockAndHistoryUnchanged()
        {
            string dbPath;
            var factory = CreateTestDatabase(out dbPath);
            try
            {
                var itemRepo = new ItemRepository(factory);
                var transRepo = new InventoryTransactionRepository(factory);
                var stockService = new StockService(transRepo, itemRepo);

                var item = CreateSampleItem(itemRepo, "FAIL-01", "Fail Safety Product");

                // Stock In 20
                stockService.StockIn(new StockOperationRequest
                {
                    ItemID = item.ItemID,
                    Quantity = 20,
                    UnitPrice = 50000
                });

                // Multiple invalid operations
                try { stockService.StockOut(new StockOperationRequest { ItemID = item.ItemID, Quantity = 25 }); } catch { }
                try { stockService.StockAdjustment(new StockOperationRequest { ItemID = item.ItemID, Quantity = -21, Reason = "Check" }); } catch { }
                try { stockService.StockAdjustment(new StockOperationRequest { ItemID = item.ItemID, Quantity = 5, Reason = "" }); } catch { }

                var reloaded = itemRepo.GetById(item.ItemID);
                Assert(reloaded.CurrentStock == 20, "Stock should remain 20 after multiple failed attempts.");

                var history = transRepo.SearchTransactions(new TransactionSearchCriteria { ItemID = item.ItemID }).ToList();
                Assert(history.Count == 1, "Only 1 valid transaction should exist in history.");
            }
            finally
            {
                CleanupTempDb(dbPath);
            }
        }

        private void Test_SequentialStockOperations_ProduceCorrectFinalStockAndAuditHistory()
        {
            string dbPath;
            var factory = CreateTestDatabase(out dbPath);
            try
            {
                var itemRepo = new ItemRepository(factory);
                var transRepo = new InventoryTransactionRepository(factory);
                var stockService = new StockService(transRepo, itemRepo);

                var item = CreateSampleItem(itemRepo, "SEQ-001", "Sequential Test Product");

                // Sequence of operations:
                // 1. IN +100  -> Stock = 100
                // 2. OUT -30  -> Stock = 70
                // 3. ADJ -10  -> Stock = 60
                // 4. IN +40   -> Stock = 100
                // 5. OUT -50  -> Stock = 50
                // 6. ADJ +15  -> Stock = 65

                stockService.StockIn(new StockOperationRequest { ItemID = item.ItemID, Quantity = 100, UnitPrice = 50000 });
                stockService.StockOut(new StockOperationRequest { ItemID = item.ItemID, Quantity = 30, UnitPrice = 75000 });
                stockService.StockAdjustment(new StockOperationRequest { ItemID = item.ItemID, Quantity = -10, Reason = "Damaged in transport" });
                stockService.StockIn(new StockOperationRequest { ItemID = item.ItemID, Quantity = 40, UnitPrice = 52000 });
                stockService.StockOut(new StockOperationRequest { ItemID = item.ItemID, Quantity = 50, UnitPrice = 75000 });
                stockService.StockAdjustment(new StockOperationRequest { ItemID = item.ItemID, Quantity = 15, Reason = "Warehouse recount surplus" });

                var finalItem = itemRepo.GetById(item.ItemID);
                Assert(finalItem.CurrentStock == 65, string.Format("Expected final stock 65, got {0}.", finalItem.CurrentStock));

                var history = transRepo.SearchTransactions(new TransactionSearchCriteria { ItemID = item.ItemID }).ToList();
                Assert(history.Count == 6, string.Format("Expected 6 transaction records, got {0}.", history.Count));

                // Verify types in reverse chronological order (newest first)
                Assert(history[0].TransactionType == "ADJUSTMENT" && history[0].Quantity == 15, "Step 6 mismatch.");
                Assert(history[1].TransactionType == "OUT" && history[1].Quantity == 50, "Step 5 mismatch.");
                Assert(history[2].TransactionType == "IN" && history[2].Quantity == 40, "Step 4 mismatch.");
                Assert(history[3].TransactionType == "ADJUSTMENT" && history[3].Quantity == -10, "Step 3 mismatch.");
                Assert(history[4].TransactionType == "OUT" && history[4].Quantity == 30, "Step 2 mismatch.");
                Assert(history[5].TransactionType == "IN" && history[5].Quantity == 100, "Step 1 mismatch.");
            }
            finally
            {
                CleanupTempDb(dbPath);
            }
        }

        private void Test_TransactionHistory_FilteringAndSearch()
        {
            string dbPath;
            var factory = CreateTestDatabase(out dbPath);
            try
            {
                var itemRepo = new ItemRepository(factory);
                var transRepo = new InventoryTransactionRepository(factory);
                var stockService = new StockService(transRepo, itemRepo);

                var itemA = CreateSampleItem(itemRepo, "SHO-BLK-42", "Nike Air Black 42");
                var itemB = CreateSampleItem(itemRepo, "SHO-WHT-42", "Adidas Superstar White 42");

                stockService.StockIn(new StockOperationRequest
                {
                    ItemID = itemA.ItemID,
                    Quantity = 50,
                    UnitPrice = 500000,
                    ReferenceNumber = "PO-NIKE-01",
                    SupplierOrCustomer = "Nike Official"
                });

                stockService.StockOut(new StockOperationRequest
                {
                    ItemID = itemA.ItemID,
                    Quantity = 10,
                    UnitPrice = 800000,
                    ReferenceNumber = "INV-SALE-01",
                    SupplierOrCustomer = "Customer Budi"
                });

                stockService.StockIn(new StockOperationRequest
                {
                    ItemID = itemB.ItemID,
                    Quantity = 30,
                    UnitPrice = 450000,
                    ReferenceNumber = "PO-ADI-01",
                    SupplierOrCustomer = "Adidas Distributor"
                });

                stockService.StockAdjustment(new StockOperationRequest
                {
                    ItemID = itemB.ItemID,
                    Quantity = -2,
                    Reason = "Display model wear"
                });

                // 1. Search by SKU
                var resultsSku = stockService.SearchTransactions(new TransactionSearchCriteria { SearchText = "SHO-BLK" }).ToList();
                Assert(resultsSku.Count == 2, string.Format("Expected 2 transactions for SHO-BLK, got {0}.", resultsSku.Count));

                // 2. Filter by TransactionType IN
                var resultsIn = stockService.SearchTransactions(new TransactionSearchCriteria { TransactionType = "IN" }).ToList();
                Assert(resultsIn.Count == 2, string.Format("Expected 2 IN transactions, got {0}.", resultsIn.Count));

                // 3. Filter by TransactionType OUT
                var resultsOut = stockService.SearchTransactions(new TransactionSearchCriteria { TransactionType = "OUT" }).ToList();
                Assert(resultsOut.Count == 1, string.Format("Expected 1 OUT transaction, got {0}.", resultsOut.Count));
                Assert(resultsOut[0].SKU == "SHO-BLK-42", "OUT SKU should match.");

                // 4. Filter by TransactionType ADJUSTMENT
                var resultsAdj = stockService.SearchTransactions(new TransactionSearchCriteria { TransactionType = "ADJUSTMENT" }).ToList();
                Assert(resultsAdj.Count == 1, string.Format("Expected 1 ADJUSTMENT transaction, got {0}.", resultsAdj.Count));
                Assert(resultsAdj[0].Reason == "Display model wear", "Adjustment reason should match.");

                // 5. Filter by ItemID
                var resultsItemB = stockService.SearchTransactions(new TransactionSearchCriteria { ItemID = itemB.ItemID }).ToList();
                Assert(resultsItemB.Count == 2, string.Format("Expected 2 transactions for Item B, got {0}.", resultsItemB.Count));
            }
            finally
            {
                CleanupTempDb(dbPath);
            }
        }

        private void Test_TransactionData_ContainsExpectedValues()
        {
            string dbPath;
            var factory = CreateTestDatabase(out dbPath);
            try
            {
                var itemRepo = new ItemRepository(factory);
                var transRepo = new InventoryTransactionRepository(factory);
                var stockService = new StockService(transRepo, itemRepo);

                var item = CreateSampleItem(itemRepo, "VAL-CHECK", "Validation Check Product");

                long transId = stockService.StockIn(new StockOperationRequest
                {
                    ItemID = item.ItemID,
                    Quantity = 100,
                    UnitPrice = 125000,
                    ReferenceNumber = "REF-9988",
                    SupplierOrCustomer = "PT Supplier Mitra",
                    Reason = "Restock",
                    Notes = "Delivered on pallet 4",
                    CreatedBy = "Operator1"
                });

                var trans = transRepo.GetById(transId);
                Assert(trans != null, "Transaction should exist.");
                Assert(trans.ItemID == item.ItemID, "ItemID mismatch.");
                Assert(trans.TransactionType == "IN", "TransactionType mismatch.");
                Assert(trans.Quantity == 100, "Quantity mismatch.");
                Assert(trans.UnitPrice == 125000, "UnitPrice mismatch.");
                Assert(trans.ReferenceNumber == "REF-9988", "ReferenceNumber mismatch.");
                Assert(trans.SupplierOrCustomer == "PT Supplier Mitra", "SupplierOrCustomer mismatch.");
                Assert(trans.Reason == "Restock", "Reason mismatch.");
                Assert(trans.Notes == "Delivered on pallet 4", "Notes mismatch.");
                Assert(trans.CreatedBy == "Operator1", "CreatedBy mismatch.");
                Assert(trans.TransactionDate <= DateTime.UtcNow, "TransactionDate should be in the past or now.");
            }
            finally
            {
                CleanupTempDb(dbPath);
            }
        }
    }
}
