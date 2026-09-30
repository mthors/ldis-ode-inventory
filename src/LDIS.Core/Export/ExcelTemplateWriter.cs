using System;
using System.IO;
using ClosedXML.Excel;

namespace LDIS.Core.Export
{
    /// <summary>
    /// Generates the official LDIS Product Import Excel template (.xlsx).
    /// Creates a formatted 'Products' sheet with 10 import headers and blank data rows,
    /// plus a dedicated 'Instructions' sheet with concise data entry rules.
    /// </summary>
    public class ExcelTemplateWriter
    {
        private static readonly XLColor HeaderBackground = XLColor.FromArgb(31, 78, 120); // Professional navy blue
        private static readonly XLColor HeaderFontColor = XLColor.White;
        private static readonly XLColor InstructionsTitleBg = XLColor.FromArgb(41, 128, 185);

        public static readonly string[] ProductHeaders = new[]
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
            "Min Stock Level"
        };

        public void WriteTemplate(Stream stream)
        {
            if (stream == null) throw new ArgumentNullException("stream");

            using (var workbook = new XLWorkbook())
            {
                // 1. Products Sheet (Blank data rows - per CORRECTION A)
                var wsProducts = workbook.Worksheets.Add("Products");

                for (int col = 0; col < ProductHeaders.Length; col++)
                {
                    var cell = wsProducts.Cell(1, col + 1);
                    cell.SetValue(ProductHeaders[col]);
                }

                // Format text columns (1 to 7) as Text
                for (int col = 1; col <= 7; col++)
                {
                    wsProducts.Column(col).Style.NumberFormat.Format = "@";
                }

                // Format numeric columns (8 to 10) as Number
                for (int col = 8; col <= 10; col++)
                {
                    wsProducts.Column(col).Style.NumberFormat.Format = "#,##0";
                }

                // Style Header Row
                var headerRange = wsProducts.Range(1, 1, 1, ProductHeaders.Length);
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Font.FontColor = HeaderFontColor;
                headerRange.Style.Fill.BackgroundColor = HeaderBackground;
                headerRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                wsProducts.Row(1).Height = 24;

                // Freeze Header Row
                wsProducts.SheetView.FreezeRows(1);

                // Auto-fit column widths with sensible minimums
                wsProducts.Columns(1, ProductHeaders.Length).AdjustToContents();
                for (int col = 1; col <= ProductHeaders.Length; col++)
                {
                    if (wsProducts.Column(col).Width < 16)
                    {
                        wsProducts.Column(col).Width = 16;
                    }
                }

                // 2. Instructions Sheet
                var wsInstructions = workbook.Worksheets.Add("Instructions");

                // Title
                wsInstructions.Cell("A1").SetValue("LDIS — Product Master Onboarding Guide");
                wsInstructions.Cell("A1").Style.Font.Bold = true;
                wsInstructions.Cell("A1").Style.Font.FontSize = 14;
                wsInstructions.Cell("A1").Style.Font.FontColor = XLColor.White;
                wsInstructions.Range("A1:E1").Merge().Style.Fill.BackgroundColor = InstructionsTitleBg;
                wsInstructions.Row(1).Height = 28;

                wsInstructions.Cell("A2").SetValue("Please read these rules before filling in the 'Products' worksheet:");
                wsInstructions.Cell("A2").Style.Font.Italic = true;
                wsInstructions.Row(2).Height = 20;

                // Table Headers
                int row = 4;
                string[] instrHeaders = new[] { "Column Name", "Required?", "Max Length / Type", "Allowed Values / Format", "Example / Notes" };
                for (int c = 0; c < instrHeaders.Length; c++)
                {
                    var cell = wsInstructions.Cell(row, c + 1);
                    cell.SetValue(instrHeaders[c]);
                    cell.Style.Font.Bold = true;
                    cell.Style.Font.FontColor = HeaderFontColor;
                    cell.Style.Fill.BackgroundColor = HeaderBackground;
                }
                wsInstructions.Row(row).Height = 22;

                // Table Data
                var rules = new[]
                {
                    new[] { "SKU", "YES", "50 characters", "Unique text (case-insensitive)", "TSHIRT-BLK-01 (Must not already exist in LDIS)" },
                    new[] { "Name", "YES", "150 characters", "Text", "Cotton Crewneck T-Shirt" },
                    new[] { "Category", "No", "100 characters", "Existing category name", "Apparel (Must exist in LDIS first; will not auto-create)" },
                    new[] { "Brand", "No", "50 characters", "Text", "LDIS Basics" },
                    new[] { "Color", "No", "50 characters", "Text", "Black" },
                    new[] { "Size", "No", "50 characters", "Text", "L" },
                    new[] { "Gender", "No", "Controlled text", "Unisex, Men, Women, Kids, None / Unspecified", "Unisex (Blank defaults to 'None / Unspecified')" },
                    new[] { "Purchase Price", "No", "Whole integer >= 0", "Non-negative integer in Rupiah", "45000 (No decimals/commas; 0 if omitted)" },
                    new[] { "Selling Price", "No", "Whole integer >= 0", "Non-negative integer in Rupiah", "85000 (No decimals/commas; 0 if omitted)" },
                    new[] { "Min Stock Level", "No", "Whole integer >= 0", "Non-negative integer", "10 (Blank defaults to 0)" }
                };

                row++;
                foreach (var r in rules)
                {
                    for (int c = 0; c < r.Length; c++)
                    {
                        var cell = wsInstructions.Cell(row, c + 1);
                        cell.SetValue(r[c]);
                        if (c == 1 && r[c] == "YES")
                        {
                            cell.Style.Font.Bold = true;
                            cell.Style.Font.FontColor = XLColor.DarkRed;
                        }
                    }
                    wsInstructions.Row(row).Height = 20;
                    row++;
                }

                var tableRange = wsInstructions.Range(4, 1, row - 1, instrHeaders.Length);
                tableRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                tableRange.Style.Border.OutsideBorder = XLBorderStyleValues.Medium;

                // Additional Important Notes
                row += 2;
                wsInstructions.Cell(row, 1).SetValue("IMPORTANT SYSTEM RULES:");
                wsInstructions.Cell(row, 1).Style.Font.Bold = true;
                wsInstructions.Cell(row, 1).Style.Font.FontSize = 11;
                row++;

                var notes = new[]
                {
                    "1. CurrentStock is NOT imported: All imported products start with CurrentStock = 0. Use '+ Stock' (Stock IN) to record inventory receipts.",
                    "2. Categories: LDIS will not automatically create categories. If you specify a category, ensure it exists in 'Categories' management before importing.",
                    "3. No Overwrites: Import is for onboarding new products only. Existing SKUs will be rejected as errors.",
                    "4. Atomic Safety: If any row contains a validation error, the entire import is blocked until the error is corrected in your file.",
                    "5. Blank rows: Completely blank rows in the Products sheet are ignored automatically."
                };

                foreach (var note in notes)
                {
                    wsInstructions.Cell(row, 1).SetValue(note);
                    row++;
                }

                wsInstructions.Columns(1, instrHeaders.Length).AdjustToContents();
                wsInstructions.Column(1).Width = 18;
                wsInstructions.Column(5).Width = 55;

                // Ensure the Products sheet is the active sheet when opening the workbook
                wsProducts.Select();

                workbook.SaveAs(stream);
            }
        }

        public void WriteTemplate(string filePath)
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
                WriteTemplate(stream);
            }
        }
    }
}
