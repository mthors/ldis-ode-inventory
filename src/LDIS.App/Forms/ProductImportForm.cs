using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using LDIS.Core.DTOs;
using LDIS.Core.Services;

namespace LDIS.App.Forms
{
    public partial class ProductImportForm : Form
    {
        private readonly IProductImportService _importService;
        private ProductImportResult _lastResult;

        public ProductImportResult LastResult
        {
            get { return _lastResult; }
        }

        public ProductImportForm(IProductImportService importService)
        {
            if (importService == null)
            {
                throw new ArgumentNullException("importService");
            }

            _importService = importService;
            InitializeComponent();
            ResetState();
        }

        private void ResetState()
        {
            _lastResult = null;
            txtFilePath.Text = string.Empty;

            pnlBanner.BackColor = Color.FromArgb(235, 240, 245);
            lblBanner.ForeColor = Color.FromArgb(44, 62, 80);
            lblBanner.Text = "Select an Excel workbook (.xlsx) to inspect and import.";

            tabErrors.Text = "Validation Errors (0)";
            tabPreview.Text = "Valid Products Preview (0)";

            dgvErrors.Rows.Clear();
            dgvPreview.Rows.Clear();

            lblRowCountSummary.Text = "Total rows: 0 | Valid: 0 | Errors: 0";
            btnImport.Enabled = false;
            btnImport.Text = "Import Products";
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            using (var ofd = new OpenFileDialog())
            {
                ofd.Title = "Select Product Import Excel Workbook";
                ofd.Filter = "Excel Workbook (*.xlsx)|*.xlsx|All Files (*.*)|*.*";
                ofd.FilterIndex = 1;
                ofd.RestoreDirectory = true;

                if (ofd.ShowDialog(this) == DialogResult.OK)
                {
                    LoadAndValidateFile(ofd.FileName);
                }
            }
        }

        public void LoadAndValidateFile(string filePath)
        {
            try
            {
                Cursor = Cursors.WaitCursor;
                txtFilePath.Text = filePath;

                _lastResult = _importService.ValidateWorkbook(filePath);
                DisplayResults(_lastResult);
            }
            catch (Exception ex)
            {
                pnlBanner.BackColor = Color.FromArgb(253, 237, 237);
                lblBanner.ForeColor = Color.FromArgb(192, 57, 43);
                lblBanner.Text = "Error reading workbook: " + ex.Message;

                dgvErrors.Rows.Clear();
                dgvErrors.Rows.Add(0, "File", filePath, ex.Message);
                tabErrors.Text = "Validation Errors (1)";
                tabPreview.Text = "Valid Products Preview (0)";
                tabResults.SelectedTab = tabErrors;

                lblRowCountSummary.Text = "Total rows: 0 | Valid: 0 | Errors: 1";
                btnImport.Enabled = false;
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void DisplayResults(ProductImportResult result)
        {
            dgvErrors.Rows.Clear();
            dgvPreview.Rows.Clear();

            int errCount = result.Errors != null ? result.Errors.Count : 0;
            int validCount = result.ValidRows != null ? result.ValidRows.Count : 0;
            int totalCount = result.TotalRowsRead;

            tabErrors.Text = string.Format("Validation Errors ({0})", errCount);
            tabPreview.Text = string.Format("Valid Products Preview ({0})", validCount);

            lblRowCountSummary.Text = string.Format("Total rows: {0} | Valid: {1} | Errors: {2}", totalCount, validCount, errCount);

            // Populate Errors
            if (errCount > 0)
            {
                foreach (var err in result.Errors)
                {
                    dgvErrors.Rows.Add(
                        err.RowNumber > 0 ? (object)err.RowNumber : "-",
                        err.ColumnName ?? string.Empty,
                        err.RawValue ?? string.Empty,
                        err.ErrorMessage ?? string.Empty
                    );
                }
            }

            // Populate Preview
            if (validCount > 0)
            {
                foreach (var item in result.ValidRows)
                {
                    dgvPreview.Rows.Add(
                        item.SKU,
                        item.Name,
                        item.CategoryName ?? "-",
                        item.Brand ?? "-",
                        item.Color ?? "-",
                        item.Size ?? "-",
                        item.Gender ?? "-",
                        item.PurchasePrice.ToString("#,##0", CultureInfo.InvariantCulture),
                        item.SellingPrice.ToString("#,##0", CultureInfo.InvariantCulture),
                        item.MinStockLevel.ToString("#,##0", CultureInfo.InvariantCulture)
                    );
                }
            }

            if (errCount > 0)
            {
                pnlBanner.BackColor = Color.FromArgb(253, 237, 237);
                lblBanner.ForeColor = Color.FromArgb(192, 57, 43);
                lblBanner.Text = string.Format("❌ Validation Failed: {0} total rows read — {1} valid, {2} error(s) found. Fix errors in your file to proceed.",
                    totalCount, validCount, errCount);

                tabResults.SelectedTab = tabErrors;
                btnImport.Enabled = false;
                btnImport.Text = "Import Products";
            }
            else if (validCount > 0)
            {
                pnlBanner.BackColor = Color.FromArgb(234, 250, 234);
                lblBanner.ForeColor = Color.FromArgb(39, 174, 96);
                lblBanner.Text = string.Format("✅ Validation Succeeded: {0} valid products ready to import. 0 errors found.", validCount);

                tabResults.SelectedTab = tabPreview;
                btnImport.Enabled = true;
                btnImport.Text = string.Format("Import {0} Products", validCount);
            }
            else
            {
                pnlBanner.BackColor = Color.FromArgb(254, 249, 231);
                lblBanner.ForeColor = Color.FromArgb(183, 149, 11);
                lblBanner.Text = "⚠️ Workbook contains no product rows to import.";

                tabResults.SelectedTab = tabErrors;
                btnImport.Enabled = false;
                btnImport.Text = "Import Products";
            }
        }

        private void btnDownloadTemplate_Click(object sender, EventArgs e)
        {
            try
            {
                using (var sfd = new SaveFileDialog())
                {
                    sfd.Title = "Download Product Import Template";
                    sfd.Filter = "Excel Workbook (*.xlsx)|*.xlsx|All Files (*.*)|*.*";
                    sfd.FilterIndex = 1;
                    sfd.DefaultExt = "xlsx";
                    sfd.FileName = "LDIS_Product_Import_Template.xlsx";
                    sfd.OverwritePrompt = true;
                    sfd.RestoreDirectory = true;

                    if (sfd.ShowDialog(this) == DialogResult.OK)
                    {
                        _importService.GenerateTemplate(sfd.FileName);
                        MessageBox.Show(
                            this,
                            string.Format("Import template successfully saved to:\n{0}", sfd.FileName),
                            "Download Template",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information
                        );
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    this,
                    "Failed to generate import template:\n" + ex.Message,
                    "Download Template Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        private void btnImport_Click(object sender, EventArgs e)
        {
            if (_lastResult == null || !_lastResult.CanImport)
            {
                MessageBox.Show(
                    this,
                    "Cannot import: the workbook has not been validated or contains errors.",
                    "Import Blocked",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
                return;
            }

            int countToImport = _lastResult.ValidRows.Count;
            string confirmMsg = string.Format(
                "Are you sure you want to import {0} new products into the catalogue?\n\n" +
                "• All imported products will start with CurrentStock = 0.\n" +
                "• All imported products will be set to Active.\n" +
                "• This operation is atomic and cannot be undone automatically.",
                countToImport
            );

            var confirmResult = MessageBox.Show(
                this,
                confirmMsg,
                "Confirm Product Import",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (confirmResult != DialogResult.Yes)
            {
                return;
            }

            try
            {
                Cursor = Cursors.WaitCursor;
                int insertedCount = _importService.ExecuteImport(_lastResult.ValidRows);

                MessageBox.Show(
                    this,
                    string.Format("Successfully imported {0} products into the catalogue!", insertedCount),
                    "Import Complete",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    this,
                    "Failed to import products:\n" + ex.Message + "\n\nAll database changes have been rolled back.",
                    "Import Failed",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
