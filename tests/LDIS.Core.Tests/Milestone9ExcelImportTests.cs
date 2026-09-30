using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ClosedXML.Excel;
using LDIS.Core.Data;
using LDIS.Core.Data.Repositories;
using LDIS.Core.DTOs;
using LDIS.Core.Export;
using LDIS.Core.Models;
using LDIS.Core.Services;

namespace LDIS.Core.Tests
{
    public class Milestone9ExcelImportTests
    {
        private static string CreateTempDbPath()
        {
            string tempDir = Path.Combine(Path.GetTempPath(), "LDIS_M9Tests_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(tempDir);
            return Path.Combine(tempDir, "test_m9.db");
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
                // Best effort cleanup
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

        public void RunAllTests()
        {
            // Template generation tests (Items 1, 2, 3, 4)
            RunTest("Test_Template_GeneratesCorrectly_With10Headers_AndInstructionsSheet", Test_Template_GeneratesCorrectly_With10Headers_AndInstructionsSheet);
            RunTest("Test_Template_ProductsSheet_HasNoImportableSampleRows", Test_Template_ProductsSheet_HasNoImportableSampleRows);

            // Structural / Header validation tests (Items 5, 6, 7, 8, 9)
            RunTest("Test_Import_MissingRequiredColumn_FailsValidationWithClearMessage", Test_Import_MissingRequiredColumn_FailsValidationWithClearMessage);
            RunTest("Test_Import_DuplicateHeaders_FailsValidation", Test_Import_DuplicateHeaders_FailsValidation);
            RunTest("Test_Import_UnexpectedExtraHeaders_FailsValidation", Test_Import_UnexpectedExtraHeaders_FailsValidation);
            RunTest("Test_Import_AcceptsMinStockAlias_ForMinStockLevel", Test_Import_AcceptsMinStockAlias_ForMinStockLevel);
            RunTest("Test_Import_EmptyWorkbook_ZeroProductRows_FailsValidation", Test_Import_EmptyWorkbook_ZeroProductRows_FailsValidation);
            RunTest("Test_Import_AllBlankRows_IgnoredCleanly", Test_Import_AllBlankRows_IgnoredCleanly);

            // Field-level validation tests (Items 10, 11, 12, 13, 14, 15)
            RunTest("Test_Import_MissingSku_FailsValidationWithRowNumber", Test_Import_MissingSku_FailsValidationWithRowNumber);
            RunTest("Test_Import_MissingName_FailsValidationWithRowNumber", Test_Import_MissingName_FailsValidationWithRowNumber);
            RunTest("Test_Import_SkuAndName_ExceedingMaxLength_FailsValidation", Test_Import_SkuAndName_ExceedingMaxLength_FailsValidation);
            RunTest("Test_Import_DuplicateSkuWithinWorkbook_FailsValidation", Test_Import_DuplicateSkuWithinWorkbook_FailsValidation);
            RunTest("Test_Import_ExistingDatabaseSkuCollision_FailsValidation", Test_Import_ExistingDatabaseSkuCollision_FailsValidation);

            // Category & Gender tests (Items 16, 17, 18, 19)
            RunTest("Test_Import_NonexistentCategory_FailsValidationWithoutAutoCreate", Test_Import_NonexistentCategory_FailsValidationWithoutAutoCreate);
            RunTest("Test_Import_ValidCategory_ResolvesCategoryIdCorrectly", Test_Import_ValidCategory_ResolvesCategoryIdCorrectly);
            RunTest("Test_Import_InvalidGender_FailsValidation", Test_Import_InvalidGender_FailsValidation);
            RunTest("Test_Import_ValidGenderAndBlankGender_NormalizesCorrectly", Test_Import_ValidGenderAndBlankGender_NormalizesCorrectly);

            // Numeric & Format tests (Items 20, 21, 22, 23, 24, 25)
            RunTest("Test_Import_InvalidPurchasePrice_NegativeOrDecimalOrText_FailsValidation", Test_Import_InvalidPurchasePrice_NegativeOrDecimalOrText_FailsValidation);
            RunTest("Test_Import_InvalidSellingPrice_NegativeOrDecimalOrText_FailsValidation", Test_Import_InvalidSellingPrice_NegativeOrDecimalOrText_FailsValidation);
            RunTest("Test_Import_InvalidMinStockLevel_NegativeOrDecimalOrText_FailsValidation", Test_Import_InvalidMinStockLevel_NegativeOrDecimalOrText_FailsValidation);
            RunTest("Test_Import_BlankPricesAndMinStock_DefaultToZero", Test_Import_BlankPricesAndMinStock_DefaultToZero);
            RunTest("Test_Import_NumericAndDoubleCellTypes_ParsedAsWholeNumbers", Test_Import_NumericAndDoubleCellTypes_ParsedAsWholeNumbers);
            RunTest("Test_Import_UnicodeAndSpecialCharacters_PreservedCorrectly", Test_Import_UnicodeAndSpecialCharacters_PreservedCorrectly);
            RunTest("Test_Import_FormulaAndCalculationErrors_HandledSafely", Test_Import_FormulaAndCalculationErrors_HandledSafely);

            // Integration, Atomicity, and Safety tests (Items 26, 27, 28, 29, 30, 31)
            RunTest("Test_Import_ValidMultiRowWorkbook_PassesValidationWithPreview", Test_Import_ValidMultiRowWorkbook_PassesValidationWithPreview);
            RunTest("Test_Import_ValidationErrors_CauseZeroDatabaseModifications", Test_Import_ValidationErrors_CauseZeroDatabaseModifications);
            RunTest("Test_Import_ExecuteImport_InsertsAllProductsAtomicallyInSingleTransaction", Test_Import_ExecuteImport_InsertsAllProductsAtomicallyInSingleTransaction);
            RunTest("Test_Import_ExecuteImport_SetsCurrentStockZeroAndIsActiveTrue", Test_Import_ExecuteImport_SetsCurrentStockZeroAndIsActiveTrue);
            RunTest("Test_Import_DatabaseFailure_RollsBackEntireBatch", Test_Import_DatabaseFailure_RollsBackEntireBatch);
            RunTest("Test_Import_ImportedProducts_ImmediatelySearchableAndFilterable", Test_Import_ImportedProducts_ImmediatelySearchableAndFilterable);
        }

        private static MemoryStream CreateWorkbookStream(Action<XLWorkbook> buildAction)
        {
            var ms = new MemoryStream();
            using (var wb = new XLWorkbook())
            {
                buildAction(wb);
                wb.SaveAs(ms);
            }
            ms.Position = 0;
            return ms;
        }

        private static void SetupStandardHeaders(IXLWorksheet ws)
        {
            string[] headers = ExcelTemplateWriter.ProductHeaders;
            for (int i = 0; i < headers.Length; i++)
            {
                ws.Cell(1, i + 1).SetValue(headers[i]);
            }
        }

        // --- 1. Template Generation Tests ---

        private void Test_Template_GeneratesCorrectly_With10Headers_AndInstructionsSheet()
        {
            string dbPath;
            var factory = CreateTestDatabase(out dbPath);
            try
            {
                var itemRepo = new ItemRepository(factory);
                var catRepo = new CategoryRepository(factory);
                var service = new ProductImportService(itemRepo, catRepo);

                using (var ms = new MemoryStream())
                {
                    service.GenerateTemplate(ms);
                    ms.Position = 0;

                    using (var wb = new XLWorkbook(ms))
                    {
                        Assert(wb.Worksheets.Count >= 2, "Template must contain at least 2 worksheets (Products and Instructions).");

                        var wsProd = wb.Worksheet("Products");
                        Assert(wsProd != null, "Products worksheet must exist.");

                        for (int i = 0; i < ExcelTemplateWriter.ProductHeaders.Length; i++)
                        {
                            string expected = ExcelTemplateWriter.ProductHeaders[i];
                            string actual = wsProd.Cell(1, i + 1).GetString().Trim();
                            Assert(string.Equals(expected, actual, StringComparison.OrdinalIgnoreCase),
                                string.Format("Header {0} mismatch: expected '{1}', found '{2}'", i + 1, expected, actual));
                        }

                        var wsInstr = wb.Worksheet("Instructions");
                        Assert(wsInstr != null, "Instructions worksheet must exist.");
                        Assert(!string.IsNullOrWhiteSpace(wsInstr.Cell("A1").GetString()), "Instructions worksheet must contain text guidance.");
                    }
                }
            }
            finally
            {
                CleanupTempDb(dbPath);
            }
        }

        private void Test_Template_ProductsSheet_HasNoImportableSampleRows()
        {
            string dbPath;
            var factory = CreateTestDatabase(out dbPath);
            try
            {
                var itemRepo = new ItemRepository(factory);
                var catRepo = new CategoryRepository(factory);
                var service = new ProductImportService(itemRepo, catRepo);

                using (var ms = new MemoryStream())
                {
                    service.GenerateTemplate(ms);
                    ms.Position = 0;

                    using (var wb = new XLWorkbook(ms))
                    {
                        var wsProd = wb.Worksheet("Products");
                        var lastRow = wsProd.LastRowUsed();
                        int rowNum = lastRow != null ? lastRow.RowNumber() : 0;
                        Assert(rowNum <= 1, string.Format("Products worksheet must contain NO data rows (CORRECTION A), found rows up to {0}", rowNum));
                    }
                }
            }
            finally
            {
                CleanupTempDb(dbPath);
            }
        }

        // --- 2. Structural & Header Validation Tests ---

        private void Test_Import_MissingRequiredColumn_FailsValidationWithClearMessage()
        {
            string dbPath;
            var factory = CreateTestDatabase(out dbPath);
            try
            {
                var itemRepo = new ItemRepository(factory);
                var catRepo = new CategoryRepository(factory);
                var service = new ProductImportService(itemRepo, catRepo);

                using (var ms = CreateWorkbookStream(wb =>
                {
                    var ws = wb.Worksheets.Add("Products");
                    // Omit Selling Price and SKU
                    ws.Cell("A1").SetValue("Name");
                    ws.Cell("B1").SetValue("Category");
                    ws.Cell("C1").SetValue("Brand");
                    ws.Cell("D1").SetValue("Color");
                    ws.Cell("E1").SetValue("Size");
                    ws.Cell("F1").SetValue("Gender");
                    ws.Cell("G1").SetValue("Purchase Price");
                    ws.Cell("H1").SetValue("Min Stock Level");
                }))
                {
                    var result = service.ValidateWorkbook(ms);
                    Assert(!result.CanImport, "Validation must fail when required columns are missing.");
                    Assert(result.Errors.Any(e => e.ErrorMessage.Contains("Missing required column header(s)") && e.ErrorMessage.Contains("SKU")),
                        "Error message must specify that SKU is missing.");
                }
            }
            finally
            {
                CleanupTempDb(dbPath);
            }
        }

        private void Test_Import_DuplicateHeaders_FailsValidation()
        {
            string dbPath;
            var factory = CreateTestDatabase(out dbPath);
            try
            {
                var itemRepo = new ItemRepository(factory);
                var catRepo = new CategoryRepository(factory);
                var service = new ProductImportService(itemRepo, catRepo);

                using (var ms = CreateWorkbookStream(wb =>
                {
                    var ws = wb.Worksheets.Add("Products");
                    SetupStandardHeaders(ws);
                    // Add duplicate SKU header in column 11
                    ws.Cell(1, 11).SetValue("SKU");
                }))
                {
                    var result = service.ValidateWorkbook(ms);
                    Assert(!result.CanImport, "Validation must fail when duplicate headers exist.");
                    Assert(result.Errors.Any(e => e.ErrorMessage.Contains("Duplicate column header")),
                        "Error message must report duplicate column headers.");
                }
            }
            finally
            {
                CleanupTempDb(dbPath);
            }
        }

        private void Test_Import_UnexpectedExtraHeaders_FailsValidation()
        {
            string dbPath;
            var factory = CreateTestDatabase(out dbPath);
            try
            {
                var itemRepo = new ItemRepository(factory);
                var catRepo = new CategoryRepository(factory);
                var service = new ProductImportService(itemRepo, catRepo);

                using (var ms = CreateWorkbookStream(wb =>
                {
                    var ws = wb.Worksheets.Add("Products");
                    SetupStandardHeaders(ws);
                    // Add 11th unexpected column (e.g. Current Stock from export)
                    ws.Cell(1, 11).SetValue("Current Stock");
                }))
                {
                    var result = service.ValidateWorkbook(ms);
                    Assert(!result.CanImport, "Validation must fail on unexpected extra columns (CORRECTION B).");
                    Assert(result.Errors.Any(e => e.ErrorMessage.Contains("Unexpected column(s)")),
                        "Error message must report unexpected columns.");
                }
            }
            finally
            {
                CleanupTempDb(dbPath);
            }
        }

        private void Test_Import_AcceptsMinStockAlias_ForMinStockLevel()
        {
            string dbPath;
            var factory = CreateTestDatabase(out dbPath);
            try
            {
                var itemRepo = new ItemRepository(factory);
                var catRepo = new CategoryRepository(factory);
                var service = new ProductImportService(itemRepo, catRepo);

                using (var ms = CreateWorkbookStream(wb =>
                {
                    var ws = wb.Worksheets.Add("Products");
                    for (int i = 0; i < 9; i++)
                    {
                        ws.Cell(1, i + 1).SetValue(ExcelTemplateWriter.ProductHeaders[i]);
                    }
                    // 10th column uses alias "Min Stock" instead of "Min Stock Level"
                    ws.Cell(1, 10).SetValue("Min Stock");

                    // Add one valid product row
                    ws.Cell(2, 1).SetValue("ALIAS-01");
                    ws.Cell(2, 2).SetValue("Alias Test Product");
                    ws.Cell(2, 10).SetValue(15);
                }))
                {
                    var result = service.ValidateWorkbook(ms);
                    Assert(result.CanImport, "Validation must accept 'Min Stock' as an approved alias for 'Min Stock Level'.");
                    Assert(result.ValidRows[0].MinStockLevel == 15, "MinStockLevel must be parsed correctly via alias.");
                }
            }
            finally
            {
                CleanupTempDb(dbPath);
            }
        }

        private void Test_Import_EmptyWorkbook_ZeroProductRows_FailsValidation()
        {
            string dbPath;
            var factory = CreateTestDatabase(out dbPath);
            try
            {
                var itemRepo = new ItemRepository(factory);
                var catRepo = new CategoryRepository(factory);
                var service = new ProductImportService(itemRepo, catRepo);

                using (var ms = CreateWorkbookStream(wb =>
                {
                    var ws = wb.Worksheets.Add("Products");
                    SetupStandardHeaders(ws);
                    // No data rows
                }))
                {
                    var result = service.ValidateWorkbook(ms);
                    Assert(!result.CanImport, "Validation must fail on empty workbook with no data rows.");
                    Assert(result.Errors.Any(e => e.ErrorMessage.Contains("no product rows to import")),
                        "Error message must specify that workbook contains no product rows.");
                }
            }
            finally
            {
                CleanupTempDb(dbPath);
            }
        }

        private void Test_Import_AllBlankRows_IgnoredCleanly()
        {
            string dbPath;
            var factory = CreateTestDatabase(out dbPath);
            try
            {
                var itemRepo = new ItemRepository(factory);
                var catRepo = new CategoryRepository(factory);
                var service = new ProductImportService(itemRepo, catRepo);

                using (var ms = CreateWorkbookStream(wb =>
                {
                    var ws = wb.Worksheets.Add("Products");
                    SetupStandardHeaders(ws);
                    // Put blank spaces in row 2 and row 3
                    ws.Cell(2, 1).SetValue("   ");
                    ws.Cell(2, 2).SetValue("");
                    ws.Cell(3, 1).SetValue("");
                }))
                {
                    var result = service.ValidateWorkbook(ms);
                    Assert(!result.CanImport, "Validation must fail when all rows are blank.");
                    Assert(result.TotalRowsRead == 0, "Blank rows must be skipped and not counted as data rows.");
                }
            }
            finally
            {
                CleanupTempDb(dbPath);
            }
        }

        // --- 3. Field-Level Validation Tests ---

        private void Test_Import_MissingSku_FailsValidationWithRowNumber()
        {
            string dbPath;
            var factory = CreateTestDatabase(out dbPath);
            try
            {
                var itemRepo = new ItemRepository(factory);
                var catRepo = new CategoryRepository(factory);
                var service = new ProductImportService(itemRepo, catRepo);

                using (var ms = CreateWorkbookStream(wb =>
                {
                    var ws = wb.Worksheets.Add("Products");
                    SetupStandardHeaders(ws);
                    ws.Cell(2, 1).SetValue(""); // Missing SKU
                    ws.Cell(2, 2).SetValue("Valid Product Name");
                }))
                {
                    var result = service.ValidateWorkbook(ms);
                    Assert(!result.CanImport, "Validation must fail when SKU is missing.");
                    var err = result.Errors.FirstOrDefault(e => e.RowNumber == 2 && e.ColumnName == "SKU");
                    Assert(err != null && err.ErrorMessage.Contains("SKU is required"), "Must report SKU is required for Row 2.");
                }
            }
            finally
            {
                CleanupTempDb(dbPath);
            }
        }

        private void Test_Import_MissingName_FailsValidationWithRowNumber()
        {
            string dbPath;
            var factory = CreateTestDatabase(out dbPath);
            try
            {
                var itemRepo = new ItemRepository(factory);
                var catRepo = new CategoryRepository(factory);
                var service = new ProductImportService(itemRepo, catRepo);

                using (var ms = CreateWorkbookStream(wb =>
                {
                    var ws = wb.Worksheets.Add("Products");
                    SetupStandardHeaders(ws);
                    ws.Cell(2, 1).SetValue("SKU-001");
                    ws.Cell(2, 2).SetValue("   "); // Missing Name
                }))
                {
                    var result = service.ValidateWorkbook(ms);
                    Assert(!result.CanImport, "Validation must fail when Name is missing.");
                    var err = result.Errors.FirstOrDefault(e => e.RowNumber == 2 && e.ColumnName == "Name");
                    Assert(err != null && err.ErrorMessage.Contains("Product name is required"), "Must report Product name is required for Row 2.");
                }
            }
            finally
            {
                CleanupTempDb(dbPath);
            }
        }

        private void Test_Import_SkuAndName_ExceedingMaxLength_FailsValidation()
        {
            string dbPath;
            var factory = CreateTestDatabase(out dbPath);
            try
            {
                var itemRepo = new ItemRepository(factory);
                var catRepo = new CategoryRepository(factory);
                var service = new ProductImportService(itemRepo, catRepo);

                using (var ms = CreateWorkbookStream(wb =>
                {
                    var ws = wb.Worksheets.Add("Products");
                    SetupStandardHeaders(ws);
                    ws.Cell(2, 1).SetValue(new string('X', 51)); // 51 chars
                    ws.Cell(2, 2).SetValue(new string('Y', 151)); // 151 chars
                }))
                {
                    var result = service.ValidateWorkbook(ms);
                    Assert(!result.CanImport, "Validation must fail when fields exceed max length.");
                    Assert(result.Errors.Any(e => e.RowNumber == 2 && e.ColumnName == "SKU" && e.ErrorMessage.Contains("50 characters")),
                        "Must flag SKU exceeding 50 characters.");
                    Assert(result.Errors.Any(e => e.RowNumber == 2 && e.ColumnName == "Name" && e.ErrorMessage.Contains("150 characters")),
                        "Must flag Name exceeding 150 characters.");
                }
            }
            finally
            {
                CleanupTempDb(dbPath);
            }
        }

        private void Test_Import_DuplicateSkuWithinWorkbook_FailsValidation()
        {
            string dbPath;
            var factory = CreateTestDatabase(out dbPath);
            try
            {
                var itemRepo = new ItemRepository(factory);
                var catRepo = new CategoryRepository(factory);
                var service = new ProductImportService(itemRepo, catRepo);

                using (var ms = CreateWorkbookStream(wb =>
                {
                    var ws = wb.Worksheets.Add("Products");
                    SetupStandardHeaders(ws);
                    ws.Cell(2, 1).SetValue("DUP-SKU-01");
                    ws.Cell(2, 2).SetValue("First Product");

                    ws.Cell(3, 1).SetValue("dup-sku-01"); // Case-insensitive duplicate on row 3
                    ws.Cell(3, 2).SetValue("Second Product");
                }))
                {
                    var result = service.ValidateWorkbook(ms);
                    Assert(!result.CanImport, "Validation must fail when duplicate SKUs exist within workbook.");
                    Assert(result.Errors.Any(e => e.ErrorMessage.Contains("Duplicate SKU 'DUP-SKU-01' found in workbook")),
                        "Must report duplicate SKU within workbook with row references.");
                }
            }
            finally
            {
                CleanupTempDb(dbPath);
            }
        }

        private void Test_Import_ExistingDatabaseSkuCollision_FailsValidation()
        {
            string dbPath;
            var factory = CreateTestDatabase(out dbPath);
            try
            {
                var itemRepo = new ItemRepository(factory);
                var catRepo = new CategoryRepository(factory);
                var service = new ProductImportService(itemRepo, catRepo);

                // Seed existing item in database
                itemRepo.Create(new Item
                {
                    SKU = "DB-EXISTING",
                    Name = "Existing Product in DB",
                    PurchasePrice = 1000,
                    SellingPrice = 2000,
                    IsActive = true
                });

                using (var ms = CreateWorkbookStream(wb =>
                {
                    var ws = wb.Worksheets.Add("Products");
                    SetupStandardHeaders(ws);
                    ws.Cell(2, 1).SetValue("db-existing"); // Collision with DB record
                    ws.Cell(2, 2).SetValue("New Product with existing SKU");
                }))
                {
                    var result = service.ValidateWorkbook(ms);
                    Assert(!result.CanImport, "Validation must fail on SKU collision with existing database item.");
                    Assert(result.Errors.Any(e => e.ErrorMessage.Contains("already in use by an existing product in the database")),
                        "Must report SKU is already in use in database.");
                }
            }
            finally
            {
                CleanupTempDb(dbPath);
            }
        }

        // --- 4. Category & Gender Validation Tests ---

        private void Test_Import_NonexistentCategory_FailsValidationWithoutAutoCreate()
        {
            string dbPath;
            var factory = CreateTestDatabase(out dbPath);
            try
            {
                var itemRepo = new ItemRepository(factory);
                var catRepo = new CategoryRepository(factory);
                var service = new ProductImportService(itemRepo, catRepo);

                using (var ms = CreateWorkbookStream(wb =>
                {
                    var ws = wb.Worksheets.Add("Products");
                    SetupStandardHeaders(ws);
                    ws.Cell(2, 1).SetValue("PROD-CAT-01");
                    ws.Cell(2, 2).SetValue("Product With Missing Category");
                    ws.Cell(2, 3).SetValue("NonExistentCategoryName");
                }))
                {
                    var result = service.ValidateWorkbook(ms);
                    Assert(!result.CanImport, "Validation must fail when category does not exist.");
                    Assert(result.Errors.Any(e => e.ColumnName == "Category" && e.ErrorMessage.Contains("does not exist")),
                        "Must report category does not exist.");

                    // Verify category was not auto-created
                    var allCats = catRepo.GetAll();
                    Assert(!allCats.Any(c => c.CategoryName == "NonExistentCategoryName"),
                        "Nonexistent category must NOT be automatically created in database.");
                }
            }
            finally
            {
                CleanupTempDb(dbPath);
            }
        }

        private void Test_Import_ValidCategory_ResolvesCategoryIdCorrectly()
        {
            string dbPath;
            var factory = CreateTestDatabase(out dbPath);
            try
            {
                var itemRepo = new ItemRepository(factory);
                var catRepo = new CategoryRepository(factory);
                var service = new ProductImportService(itemRepo, catRepo);

                // Create category
                long catId = catRepo.Create(new Category { CategoryName = "Apparel" });

                using (var ms = CreateWorkbookStream(wb =>
                {
                    var ws = wb.Worksheets.Add("Products");
                    SetupStandardHeaders(ws);
                    ws.Cell(2, 1).SetValue("PROD-CAT-02");
                    ws.Cell(2, 2).SetValue("Apparel Product");
                    ws.Cell(2, 3).SetValue("apparel"); // Case-insensitive matching
                }))
                {
                    var result = service.ValidateWorkbook(ms);
                    Assert(result.CanImport, "Validation must succeed for existing category.");
                    Assert(result.ValidRows[0].CategoryID == catId, "CategoryID must be resolved correctly to existing ID.");
                    Assert(result.ValidRows[0].CategoryName == "Apparel", "CategoryName must be canonicalized.");
                }
            }
            finally
            {
                CleanupTempDb(dbPath);
            }
        }

        private void Test_Import_InvalidGender_FailsValidation()
        {
            string dbPath;
            var factory = CreateTestDatabase(out dbPath);
            try
            {
                var itemRepo = new ItemRepository(factory);
                var catRepo = new CategoryRepository(factory);
                var service = new ProductImportService(itemRepo, catRepo);

                using (var ms = CreateWorkbookStream(wb =>
                {
                    var ws = wb.Worksheets.Add("Products");
                    SetupStandardHeaders(ws);
                    ws.Cell(2, 1).SetValue("GEN-01");
                    ws.Cell(2, 2).SetValue("Gender Test Product");
                    ws.Cell(2, 7).SetValue("Martian"); // Invalid Gender
                }))
                {
                    var result = service.ValidateWorkbook(ms);
                    Assert(!result.CanImport, "Validation must fail for uncontrolled gender.");
                    Assert(result.Errors.Any(e => e.ColumnName == "Gender" && e.ErrorMessage.Contains("Invalid gender 'Martian'")),
                        "Must report invalid gender error.");
                }
            }
            finally
            {
                CleanupTempDb(dbPath);
            }
        }

        private void Test_Import_ValidGenderAndBlankGender_NormalizesCorrectly()
        {
            string dbPath;
            var factory = CreateTestDatabase(out dbPath);
            try
            {
                var itemRepo = new ItemRepository(factory);
                var catRepo = new CategoryRepository(factory);
                var service = new ProductImportService(itemRepo, catRepo);

                using (var ms = CreateWorkbookStream(wb =>
                {
                    var ws = wb.Worksheets.Add("Products");
                    SetupStandardHeaders(ws);

                    ws.Cell(2, 1).SetValue("GEN-MEN");
                    ws.Cell(2, 2).SetValue("Men Item");
                    ws.Cell(2, 7).SetValue("men"); // Case-insensitive 'Men'

                    ws.Cell(3, 1).SetValue("GEN-BLANK");
                    ws.Cell(3, 2).SetValue("Blank Gender Item");
                    ws.Cell(3, 7).SetValue(""); // Blank gender
                }))
                {
                    var result = service.ValidateWorkbook(ms);
                    Assert(result.CanImport, "Validation must pass for valid and blank gender values.");
                    Assert(result.ValidRows[0].Gender == "Men", "Gender 'men' must normalize to 'Men'.");
                    Assert(result.ValidRows[1].Gender == "None / Unspecified", "Blank gender must normalize to 'None / Unspecified'.");
                }
            }
            finally
            {
                CleanupTempDb(dbPath);
            }
        }

        // --- 5. Numeric & Formatting Validation Tests ---

        private void Test_Import_InvalidPurchasePrice_NegativeOrDecimalOrText_FailsValidation()
        {
            string dbPath;
            var factory = CreateTestDatabase(out dbPath);
            try
            {
                var itemRepo = new ItemRepository(factory);
                var catRepo = new CategoryRepository(factory);
                var service = new ProductImportService(itemRepo, catRepo);

                using (var ms = CreateWorkbookStream(wb =>
                {
                    var ws = wb.Worksheets.Add("Products");
                    SetupStandardHeaders(ws);

                    ws.Cell(2, 1).SetValue("PRICE-NEG");
                    ws.Cell(2, 2).SetValue("Negative Price Item");
                    ws.Cell(2, 8).SetValue(-5000);

                    ws.Cell(3, 1).SetValue("PRICE-DEC");
                    ws.Cell(3, 2).SetValue("Decimal Price Item");
                    ws.Cell(3, 8).SetValue("25000.50");

                    ws.Cell(4, 1).SetValue("PRICE-TXT");
                    ws.Cell(4, 2).SetValue("Text Price Item");
                    ws.Cell(4, 8).SetValue("abc");
                }))
                {
                    var result = service.ValidateWorkbook(ms);
                    Assert(!result.CanImport, "Validation must reject negative, decimal, and non-numeric purchase prices.");
                    Assert(result.Errors.Any(e => e.RowNumber == 2 && e.ColumnName == "Purchase Price" && e.ErrorMessage.Contains("negative")),
                        "Must flag negative purchase price.");
                    Assert(result.Errors.Any(e => e.RowNumber == 3 && e.ColumnName == "Purchase Price" && e.ErrorMessage.Contains("decimal")),
                        "Must flag decimal purchase price.");
                    Assert(result.Errors.Any(e => e.RowNumber == 4 && e.ColumnName == "Purchase Price" && e.ErrorMessage.Contains("whole number")),
                        "Must flag non-numeric purchase price.");
                }
            }
            finally
            {
                CleanupTempDb(dbPath);
            }
        }

        private void Test_Import_InvalidSellingPrice_NegativeOrDecimalOrText_FailsValidation()
        {
            string dbPath;
            var factory = CreateTestDatabase(out dbPath);
            try
            {
                var itemRepo = new ItemRepository(factory);
                var catRepo = new CategoryRepository(factory);
                var service = new ProductImportService(itemRepo, catRepo);

                using (var ms = CreateWorkbookStream(wb =>
                {
                    var ws = wb.Worksheets.Add("Products");
                    SetupStandardHeaders(ws);

                    ws.Cell(2, 1).SetValue("SELL-NEG");
                    ws.Cell(2, 2).SetValue("Negative Selling Item");
                    ws.Cell(2, 9).SetValue(-100);

                    ws.Cell(3, 1).SetValue("SELL-DEC");
                    ws.Cell(3, 2).SetValue("Decimal Selling Item");
                    ws.Cell(3, 9).SetValue("45000.75");
                }))
                {
                    var result = service.ValidateWorkbook(ms);
                    Assert(!result.CanImport, "Validation must reject negative and decimal selling prices.");
                    Assert(result.Errors.Any(e => e.RowNumber == 2 && e.ColumnName == "Selling Price"), "Must flag negative selling price.");
                    Assert(result.Errors.Any(e => e.RowNumber == 3 && e.ColumnName == "Selling Price"), "Must flag decimal selling price.");
                }
            }
            finally
            {
                CleanupTempDb(dbPath);
            }
        }

        private void Test_Import_InvalidMinStockLevel_NegativeOrDecimalOrText_FailsValidation()
        {
            string dbPath;
            var factory = CreateTestDatabase(out dbPath);
            try
            {
                var itemRepo = new ItemRepository(factory);
                var catRepo = new CategoryRepository(factory);
                var service = new ProductImportService(itemRepo, catRepo);

                using (var ms = CreateWorkbookStream(wb =>
                {
                    var ws = wb.Worksheets.Add("Products");
                    SetupStandardHeaders(ws);

                    ws.Cell(2, 1).SetValue("MIN-NEG");
                    ws.Cell(2, 2).SetValue("Negative Min Stock Item");
                    ws.Cell(2, 10).SetValue(-1);

                    ws.Cell(3, 1).SetValue("MIN-DEC");
                    ws.Cell(3, 2).SetValue("Decimal Min Stock Item");
                    ws.Cell(3, 10).SetValue("5.5");
                }))
                {
                    var result = service.ValidateWorkbook(ms);
                    Assert(!result.CanImport, "Validation must reject negative and decimal min stock levels.");
                    Assert(result.Errors.Any(e => e.RowNumber == 2 && e.ColumnName == "Min Stock Level"), "Must flag negative min stock.");
                    Assert(result.Errors.Any(e => e.RowNumber == 3 && e.ColumnName == "Min Stock Level"), "Must flag decimal min stock.");
                }
            }
            finally
            {
                CleanupTempDb(dbPath);
            }
        }

        private void Test_Import_BlankPricesAndMinStock_DefaultToZero()
        {
            string dbPath;
            var factory = CreateTestDatabase(out dbPath);
            try
            {
                var itemRepo = new ItemRepository(factory);
                var catRepo = new CategoryRepository(factory);
                var service = new ProductImportService(itemRepo, catRepo);

                using (var ms = CreateWorkbookStream(wb =>
                {
                    var ws = wb.Worksheets.Add("Products");
                    SetupStandardHeaders(ws);
                    ws.Cell(2, 1).SetValue("BLANK-NUMS");
                    ws.Cell(2, 2).SetValue("Item with Blank Prices");
                    ws.Cell(2, 8).SetValue(""); // Blank purchase
                    ws.Cell(2, 9).SetValue(""); // Blank selling
                    ws.Cell(2, 10).SetValue(""); // Blank min stock
                }))
                {
                    var result = service.ValidateWorkbook(ms);
                    Assert(result.CanImport, "Validation must succeed when optional numeric fields are blank.");
                    var row = result.ValidRows[0];
                    Assert(row.PurchasePrice == 0, "Blank purchase price must default to 0.");
                    Assert(row.SellingPrice == 0, "Blank selling price must default to 0.");
                    Assert(row.MinStockLevel == 0, "Blank min stock level must default to 0.");
                }
            }
            finally
            {
                CleanupTempDb(dbPath);
            }
        }

        private void Test_Import_NumericAndDoubleCellTypes_ParsedAsWholeNumbers()
        {
            string dbPath;
            var factory = CreateTestDatabase(out dbPath);
            try
            {
                var itemRepo = new ItemRepository(factory);
                var catRepo = new CategoryRepository(factory);
                var service = new ProductImportService(itemRepo, catRepo);

                using (var ms = CreateWorkbookStream(wb =>
                {
                    var ws = wb.Worksheets.Add("Products");
                    SetupStandardHeaders(ws);
                    ws.Cell(2, 1).SetValue("NUM-TYPES");
                    ws.Cell(2, 2).SetValue("Number Types Item");
                    // Assign numeric double values without decimals
                    ws.Cell(2, 8).Value = 25000.0;
                    ws.Cell(2, 9).Value = 50000.0;
                    ws.Cell(2, 10).Value = 10.0;
                }))
                {
                    var result = service.ValidateWorkbook(ms);
                    Assert(result.CanImport, "Validation must succeed for numeric cell types without fractional part.");
                    var row = result.ValidRows[0];
                    Assert(row.PurchasePrice == 25000, "25000.0 must parse as 25000.");
                    Assert(row.SellingPrice == 50000, "50000.0 must parse as 50000.");
                    Assert(row.MinStockLevel == 10, "10.0 must parse as 10.");
                }
            }
            finally
            {
                CleanupTempDb(dbPath);
            }
        }

        private void Test_Import_UnicodeAndSpecialCharacters_PreservedCorrectly()
        {
            string dbPath;
            var factory = CreateTestDatabase(out dbPath);
            try
            {
                var itemRepo = new ItemRepository(factory);
                var catRepo = new CategoryRepository(factory);
                var service = new ProductImportService(itemRepo, catRepo);

                string unicodeSku = "SKU-BÉBÉ-01";
                string unicodeName = "Café & Cœur T-Shirt — 日本語 👕";
                string unicodeBrand = "Marque Français";
                string unicodeColor = "Bleu Nuit";

                using (var ms = CreateWorkbookStream(wb =>
                {
                    var ws = wb.Worksheets.Add("Products");
                    SetupStandardHeaders(ws);
                    ws.Cell(2, 1).SetValue(unicodeSku);
                    ws.Cell(2, 2).SetValue(unicodeName);
                    ws.Cell(2, 4).SetValue(unicodeBrand);
                    ws.Cell(2, 5).SetValue(unicodeColor);
                }))
                {
                    var result = service.ValidateWorkbook(ms);
                    Assert(result.CanImport, "Validation must succeed for valid unicode characters.");
                    var row = result.ValidRows[0];
                    Assert(row.SKU == unicodeSku, "Unicode SKU must be preserved.");
                    Assert(row.Name == unicodeName, "Unicode Name must be preserved.");
                    Assert(row.Brand == unicodeBrand, "Unicode Brand must be preserved.");
                    Assert(row.Color == unicodeColor, "Unicode Color must be preserved.");
                }
            }
            finally
            {
                CleanupTempDb(dbPath);
            }
        }

        private void Test_Import_FormulaAndCalculationErrors_HandledSafely()
        {
            string dbPath;
            var factory = CreateTestDatabase(out dbPath);
            try
            {
                var itemRepo = new ItemRepository(factory);
                var catRepo = new CategoryRepository(factory);
                var service = new ProductImportService(itemRepo, catRepo);

                using (var ms = CreateWorkbookStream(wb =>
                {
                    var ws = wb.Worksheets.Add("Products");
                    SetupStandardHeaders(ws);

                    // Row 2: Valid formula
                    ws.Cell(2, 1).SetValue("FORMULA-01");
                    ws.Cell(2, 2).SetValue("Formula Product");
                    ws.Cell(2, 8).FormulaA1 = "=20000*2"; // Evaluates to 40000

                    // Row 3: Calculation error string
                    ws.Cell(3, 1).SetValue("ERR-CELL-01");
                    ws.Cell(3, 2).SetValue("Broken Calculation Product");
                    ws.Cell(3, 8).SetValue("#REF!");
                }))
                {
                    var result = service.ValidateWorkbook(ms);
                    Assert(!result.CanImport, "Validation must fail when formula errors exist.");
                    Assert(result.ValidRows.Any(r => r.SKU == "FORMULA-01" && r.PurchasePrice == 40000),
                        "Valid formula =20000*2 must be evaluated safely to 40000.");
                    Assert(result.Errors.Any(e => e.RowNumber == 3 && e.ErrorMessage.Contains("Excel calculation error")),
                        "Broken cell calculation #REF! must be reported cleanly as calculation error.");
                }
            }
            finally
            {
                CleanupTempDb(dbPath);
            }
        }

        // --- 6. Integration, Atomicity, and Safety Tests ---

        private void Test_Import_ValidMultiRowWorkbook_PassesValidationWithPreview()
        {
            string dbPath;
            var factory = CreateTestDatabase(out dbPath);
            try
            {
                var itemRepo = new ItemRepository(factory);
                var catRepo = new CategoryRepository(factory);
                var service = new ProductImportService(itemRepo, catRepo);

                long catId = catRepo.Create(new Category { CategoryName = "Footwear" });

                using (var ms = CreateWorkbookStream(wb =>
                {
                    var ws = wb.Worksheets.Add("Products");
                    SetupStandardHeaders(ws);

                    ws.Cell(2, 1).SetValue("SHOE-001");
                    ws.Cell(2, 2).SetValue("Running Shoes");
                    ws.Cell(2, 3).SetValue("Footwear");
                    ws.Cell(2, 4).SetValue("RunnerCo");
                    ws.Cell(2, 5).SetValue("Blue");
                    ws.Cell(2, 6).SetValue("42");
                    ws.Cell(2, 7).SetValue("Men");
                    ws.Cell(2, 8).SetValue(150000);
                    ws.Cell(2, 9).SetValue(300000);
                    ws.Cell(2, 10).SetValue(5);

                    ws.Cell(3, 1).SetValue("SHOE-002");
                    ws.Cell(3, 2).SetValue("Walking Shoes");
                    ws.Cell(3, 3).SetValue("Footwear");
                    ws.Cell(3, 7).SetValue("Women");
                    ws.Cell(3, 8).SetValue(120000);
                    ws.Cell(3, 9).SetValue(250000);
                    ws.Cell(3, 10).SetValue(10);
                }))
                {
                    var result = service.ValidateWorkbook(ms);
                    Assert(result.CanImport, "Multi-row valid workbook must pass validation.");
                    Assert(result.TotalRowsRead == 2, "Total rows read must be 2.");
                    Assert(result.ValidRowCount == 2, "Valid row count must be 2.");
                    Assert(result.ErrorRowCount == 0, "Error row count must be 0.");
                    Assert(result.ValidRows[0].SKU == "SHOE-001" && result.ValidRows[0].CategoryID == catId, "Row 1 must be parsed correctly.");
                    Assert(result.ValidRows[1].SKU == "SHOE-002" && result.ValidRows[1].CategoryID == catId, "Row 2 must be parsed correctly.");
                }
            }
            finally
            {
                CleanupTempDb(dbPath);
            }
        }

        private void Test_Import_ValidationErrors_CauseZeroDatabaseModifications()
        {
            string dbPath;
            var factory = CreateTestDatabase(out dbPath);
            try
            {
                var itemRepo = new ItemRepository(factory);
                var catRepo = new CategoryRepository(factory);
                var service = new ProductImportService(itemRepo, catRepo);

                // Initial count
                int initialCount = itemRepo.Search(new ItemSearchCriteria()).Count();
                Assert(initialCount == 0, "Initial item count must be 0.");

                using (var ms = CreateWorkbookStream(wb =>
                {
                    var ws = wb.Worksheets.Add("Products");
                    SetupStandardHeaders(ws);

                    // Row 2 is valid
                    ws.Cell(2, 1).SetValue("VALID-01");
                    ws.Cell(2, 2).SetValue("Valid Item");

                    // Row 3 is invalid (missing SKU)
                    ws.Cell(3, 1).SetValue("");
                    ws.Cell(3, 2).SetValue("Invalid Item");
                }))
                {
                    var result = service.ValidateWorkbook(ms);
                    Assert(!result.CanImport, "Workbook with any error cannot be imported.");

                    // Check database count remains 0
                    int postValidationCount = itemRepo.Search(new ItemSearchCriteria()).Count();
                    Assert(postValidationCount == 0, "Database must have 0 modifications during validation.");
                }
            }
            finally
            {
                CleanupTempDb(dbPath);
            }
        }

        private void Test_Import_ExecuteImport_InsertsAllProductsAtomicallyInSingleTransaction()
        {
            string dbPath;
            var factory = CreateTestDatabase(out dbPath);
            try
            {
                var itemRepo = new ItemRepository(factory);
                var catRepo = new CategoryRepository(factory);
                var service = new ProductImportService(itemRepo, catRepo);

                long catId = catRepo.Create(new Category { CategoryName = "Electronics" });

                using (var ms = CreateWorkbookStream(wb =>
                {
                    var ws = wb.Worksheets.Add("Products");
                    SetupStandardHeaders(ws);

                    ws.Cell(2, 1).SetValue("ELEC-01");
                    ws.Cell(2, 2).SetValue("USB Cable");
                    ws.Cell(2, 3).SetValue("Electronics");
                    ws.Cell(2, 8).SetValue(15000);
                    ws.Cell(2, 9).SetValue(35000);
                    ws.Cell(2, 10).SetValue(20);

                    ws.Cell(3, 1).SetValue("ELEC-02");
                    ws.Cell(3, 2).SetValue("Power Adapter");
                    ws.Cell(3, 3).SetValue("Electronics");
                    ws.Cell(3, 8).SetValue(45000);
                    ws.Cell(3, 9).SetValue(95000);
                    ws.Cell(3, 10).SetValue(10);
                }))
                {
                    var result = service.ValidateWorkbook(ms);
                    Assert(result.CanImport, "Workbook must be valid.");

                    int inserted = service.ExecuteImport(result.ValidRows);
                    Assert(inserted == 2, "ExecuteImport must return 2 inserted items.");

                    var items = itemRepo.Search(new ItemSearchCriteria()).ToList();
                    Assert(items.Count == 2, "Database must now contain exactly 2 items.");

                    var item1 = items.First(i => i.SKU == "ELEC-01");
                    Assert(item1.Name == "USB Cable" && item1.CategoryID == catId, "Item 1 fields must match.");

                    var item2 = items.First(i => i.SKU == "ELEC-02");
                    Assert(item2.Name == "Power Adapter" && item2.CategoryID == catId, "Item 2 fields must match.");
                }
            }
            finally
            {
                CleanupTempDb(dbPath);
            }
        }

        private void Test_Import_ExecuteImport_SetsCurrentStockZeroAndIsActiveTrue()
        {
            string dbPath;
            var factory = CreateTestDatabase(out dbPath);
            try
            {
                var itemRepo = new ItemRepository(factory);
                var catRepo = new CategoryRepository(factory);
                var service = new ProductImportService(itemRepo, catRepo);

                using (var ms = CreateWorkbookStream(wb =>
                {
                    var ws = wb.Worksheets.Add("Products");
                    SetupStandardHeaders(ws);
                    ws.Cell(2, 1).SetValue("STOCK-ZERO");
                    ws.Cell(2, 2).SetValue("Stock Zero Product");
                }))
                {
                    var result = service.ValidateWorkbook(ms);
                    service.ExecuteImport(result.ValidRows);

                    var item = itemRepo.GetBySku("STOCK-ZERO");
                    Assert(item != null, "Item must be created in database.");
                    Assert(item.CurrentStock == 0, "Imported product CurrentStock MUST be 0.");
                    Assert(item.IsActive == true, "Imported product IsActive MUST be true.");
                }
            }
            finally
            {
                CleanupTempDb(dbPath);
            }
        }

        private void Test_Import_DatabaseFailure_RollsBackEntireBatch()
        {
            string dbPath;
            var factory = CreateTestDatabase(out dbPath);
            try
            {
                var itemRepo = new ItemRepository(factory);

                // Create a batch where second item violates SKU uniqueness against the first
                var items = new List<Item>
                {
                    new Item { SKU = "BATCH-01", Name = "First Item", PurchasePrice = 100, SellingPrice = 200 },
                    new Item { SKU = "BATCH-01", Name = "Conflicting Item", PurchasePrice = 100, SellingPrice = 200 }
                };

                bool exceptionThrown = false;
                try
                {
                    itemRepo.CreateBatch(items);
                }
                catch
                {
                    exceptionThrown = true;
                }

                Assert(exceptionThrown, "Database constraint violation must throw exception.");

                // Verify entire batch was rolled back - zero items inserted
                var allItems = itemRepo.Search(new ItemSearchCriteria()).ToList();
                Assert(allItems.Count == 0, "All items must be rolled back on database failure; found: " + allItems.Count);
            }
            finally
            {
                CleanupTempDb(dbPath);
            }
        }

        private void Test_Import_ImportedProducts_ImmediatelySearchableAndFilterable()
        {
            string dbPath;
            var factory = CreateTestDatabase(out dbPath);
            try
            {
                var itemRepo = new ItemRepository(factory);
                var catRepo = new CategoryRepository(factory);
                var service = new ProductImportService(itemRepo, catRepo);

                long catId = catRepo.Create(new Category { CategoryName = "Jackets" });

                using (var ms = CreateWorkbookStream(wb =>
                {
                    var ws = wb.Worksheets.Add("Products");
                    SetupStandardHeaders(ws);

                    ws.Cell(2, 1).SetValue("JKT-WINTER");
                    ws.Cell(2, 2).SetValue("Heavy Winter Parka Jacket");
                    ws.Cell(2, 3).SetValue("Jackets");
                    ws.Cell(2, 4).SetValue("NordicGear");
                    ws.Cell(2, 5).SetValue("Navy");
                    ws.Cell(2, 6).SetValue("XL");
                    ws.Cell(2, 7).SetValue("Unisex");
                    ws.Cell(2, 8).SetValue(250000);
                    ws.Cell(2, 9).SetValue(550000);
                    ws.Cell(2, 10).SetValue(5);
                }))
                {
                    var result = service.ValidateWorkbook(ms);
                    service.ExecuteImport(result.ValidRows);

                    // 1. Keyword Search
                    var searchRes = itemRepo.Search(new ItemSearchCriteria { SearchText = "Parka" }).ToList();
                    Assert(searchRes.Count == 1, "Imported item must be searchable by keyword.");
                    Assert(searchRes[0].SKU == "JKT-WINTER", "Search result SKU must match.");

                    // 2. Category Filter
                    var catRes = itemRepo.Search(new ItemSearchCriteria { CategoryID = catId }).ToList();
                    Assert(catRes.Count == 1, "Imported item must be filterable by CategoryID.");

                    // 3. Gender Filter
                    var genderRes = itemRepo.Search(new ItemSearchCriteria { Gender = "Unisex" }).ToList();
                    Assert(genderRes.Count == 1, "Imported item must be filterable by Gender.");

                    // 4. Out of Stock Dashboard filter (since CurrentStock = 0)
                    var oosRes = itemRepo.Search(new ItemSearchCriteria { StockStatus = StockFilterStatus.OutOfStock }).ToList();
                    Assert(oosRes.Count == 1, "Imported item with 0 stock must appear in OutOfStock filter.");
                }
            }
            finally
            {
                CleanupTempDb(dbPath);
            }
        }
    }
}
