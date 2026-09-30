using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace LDIS.App.Forms
{
    partial class ProductImportForm
    {
        private IContainer components = null;

        private Panel pnlTop;
        private Label lblFile;
        private TextBox txtFilePath;
        private Button btnBrowse;
        private Button btnDownloadTemplate;

        private Panel pnlBanner;
        private Label lblBanner;

        private TabControl tabResults;
        private TabPage tabErrors;
        private TabPage tabPreview;

        private DataGridView dgvErrors;
        private DataGridViewTextBoxColumn colErrRow;
        private DataGridViewTextBoxColumn colErrCol;
        private DataGridViewTextBoxColumn colErrVal;
        private DataGridViewTextBoxColumn colErrDesc;

        private DataGridView dgvPreview;
        private DataGridViewTextBoxColumn colPrevSku;
        private DataGridViewTextBoxColumn colPrevName;
        private DataGridViewTextBoxColumn colPrevCategory;
        private DataGridViewTextBoxColumn colPrevBrand;
        private DataGridViewTextBoxColumn colPrevColor;
        private DataGridViewTextBoxColumn colPrevSize;
        private DataGridViewTextBoxColumn colPrevGender;
        private DataGridViewTextBoxColumn colPrevPurPrice;
        private DataGridViewTextBoxColumn colPrevSellPrice;
        private DataGridViewTextBoxColumn colPrevMinStock;

        private Panel pnlBottom;
        private Label lblRowCountSummary;
        private Button btnImport;
        private Button btnCancel;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = new Container();

            this.pnlTop = new Panel();
            this.lblFile = new Label();
            this.txtFilePath = new TextBox();
            this.btnBrowse = new Button();
            this.btnDownloadTemplate = new Button();

            this.pnlBanner = new Panel();
            this.lblBanner = new Label();

            this.tabResults = new TabControl();
            this.tabErrors = new TabPage();
            this.tabPreview = new TabPage();

            this.dgvErrors = new DataGridView();
            this.colErrRow = new DataGridViewTextBoxColumn();
            this.colErrCol = new DataGridViewTextBoxColumn();
            this.colErrVal = new DataGridViewTextBoxColumn();
            this.colErrDesc = new DataGridViewTextBoxColumn();

            this.dgvPreview = new DataGridView();
            this.colPrevSku = new DataGridViewTextBoxColumn();
            this.colPrevName = new DataGridViewTextBoxColumn();
            this.colPrevCategory = new DataGridViewTextBoxColumn();
            this.colPrevBrand = new DataGridViewTextBoxColumn();
            this.colPrevColor = new DataGridViewTextBoxColumn();
            this.colPrevSize = new DataGridViewTextBoxColumn();
            this.colPrevGender = new DataGridViewTextBoxColumn();
            this.colPrevPurPrice = new DataGridViewTextBoxColumn();
            this.colPrevSellPrice = new DataGridViewTextBoxColumn();
            this.colPrevMinStock = new DataGridViewTextBoxColumn();

            this.pnlBottom = new Panel();
            this.lblRowCountSummary = new Label();
            this.btnImport = new Button();
            this.btnCancel = new Button();

            this.pnlTop.SuspendLayout();
            this.pnlBanner.SuspendLayout();
            this.tabResults.SuspendLayout();
            this.tabErrors.SuspendLayout();
            this.tabPreview.SuspendLayout();
            ((ISupportInitialize)(this.dgvErrors)).BeginInit();
            ((ISupportInitialize)(this.dgvPreview)).BeginInit();
            this.pnlBottom.SuspendLayout();
            this.SuspendLayout();

            // 
            // pnlTop
            // 
            this.pnlTop.BackColor = Color.FromArgb(245, 247, 250);
            this.pnlTop.BorderStyle = BorderStyle.FixedSingle;
            this.pnlTop.Controls.Add(this.btnDownloadTemplate);
            this.pnlTop.Controls.Add(this.btnBrowse);
            this.pnlTop.Controls.Add(this.txtFilePath);
            this.pnlTop.Controls.Add(this.lblFile);
            this.pnlTop.Dock = DockStyle.Top;
            this.pnlTop.Location = new Point(0, 0);
            this.pnlTop.Name = "pnlTop";
            this.pnlTop.Padding = new Padding(12, 10, 12, 10);
            this.pnlTop.Size = new Size(880, 56);
            this.pnlTop.TabIndex = 0;

            // lblFile
            this.lblFile.AutoSize = true;
            this.lblFile.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            this.lblFile.Location = new Point(12, 18);
            this.lblFile.Name = "lblFile";
            this.lblFile.Size = new Size(100, 15);
            this.lblFile.Text = "Excel Workbook:";

            // txtFilePath
            this.txtFilePath.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            this.txtFilePath.BackColor = Color.White;
            this.txtFilePath.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
            this.txtFilePath.Location = new Point(120, 15);
            this.txtFilePath.Name = "txtFilePath";
            this.txtFilePath.ReadOnly = true;
            this.txtFilePath.Size = new Size(470, 23);
            this.txtFilePath.TabIndex = 0;

            // btnBrowse
            this.btnBrowse.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            this.btnBrowse.BackColor = Color.FromArgb(52, 152, 219);
            this.btnBrowse.FlatAppearance.BorderSize = 0;
            this.btnBrowse.FlatStyle = FlatStyle.Flat;
            this.btnBrowse.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            this.btnBrowse.ForeColor = Color.White;
            this.btnBrowse.Location = new Point(598, 12);
            this.btnBrowse.Name = "btnBrowse";
            this.btnBrowse.Size = new Size(88, 28);
            this.btnBrowse.TabIndex = 1;
            this.btnBrowse.Text = "Browse...";
            this.btnBrowse.UseVisualStyleBackColor = false;
            this.btnBrowse.Click += new EventHandler(this.btnBrowse_Click);

            // btnDownloadTemplate
            this.btnDownloadTemplate.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            this.btnDownloadTemplate.BackColor = Color.FromArgb(52, 73, 94);
            this.btnDownloadTemplate.FlatAppearance.BorderSize = 0;
            this.btnDownloadTemplate.FlatStyle = FlatStyle.Flat;
            this.btnDownloadTemplate.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            this.btnDownloadTemplate.ForeColor = Color.White;
            this.btnDownloadTemplate.Location = new Point(694, 12);
            this.btnDownloadTemplate.Name = "btnDownloadTemplate";
            this.btnDownloadTemplate.Size = new Size(170, 28);
            this.btnDownloadTemplate.TabIndex = 2;
            this.btnDownloadTemplate.Text = "Download Template (.xlsx)";
            this.btnDownloadTemplate.UseVisualStyleBackColor = false;
            this.btnDownloadTemplate.Click += new EventHandler(this.btnDownloadTemplate_Click);

            // 
            // pnlBanner
            // 
            this.pnlBanner.BackColor = Color.FromArgb(235, 240, 245);
            this.pnlBanner.BorderStyle = BorderStyle.FixedSingle;
            this.pnlBanner.Controls.Add(this.lblBanner);
            this.pnlBanner.Dock = DockStyle.Top;
            this.pnlBanner.Location = new Point(0, 56);
            this.pnlBanner.Name = "pnlBanner";
            this.pnlBanner.Padding = new Padding(14, 8, 14, 8);
            this.pnlBanner.Size = new Size(880, 42);
            this.pnlBanner.TabIndex = 1;

            // lblBanner
            this.lblBanner.Dock = DockStyle.Fill;
            this.lblBanner.Font = new Font("Segoe UI", 9.25F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
            this.lblBanner.ForeColor = Color.FromArgb(44, 62, 80);
            this.lblBanner.Location = new Point(14, 8);
            this.lblBanner.Name = "lblBanner";
            this.lblBanner.Size = new Size(850, 24);
            this.lblBanner.Text = "Select an Excel workbook (.xlsx) to inspect and import.";
            this.lblBanner.TextAlign = ContentAlignment.MiddleLeft;

            // 
            // tabResults
            // 
            this.tabResults.Controls.Add(this.tabErrors);
            this.tabResults.Controls.Add(this.tabPreview);
            this.tabResults.Dock = DockStyle.Fill;
            this.tabResults.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
            this.tabResults.Location = new Point(0, 98);
            this.tabResults.Name = "tabResults";
            this.tabResults.SelectedIndex = 0;
            this.tabResults.Size = new Size(880, 448);
            this.tabResults.TabIndex = 2;

            // 
            // tabErrors
            // 
            this.tabErrors.Controls.Add(this.dgvErrors);
            this.tabErrors.Location = new Point(4, 24);
            this.tabErrors.Name = "tabErrors";
            this.tabErrors.Padding = new Padding(4);
            this.tabErrors.Size = new Size(872, 420);
            this.tabErrors.Text = "Validation Errors (0)";
            this.tabErrors.UseVisualStyleBackColor = true;

            // dgvErrors
            this.dgvErrors.AllowUserToAddRows = false;
            this.dgvErrors.AllowUserToDeleteRows = false;
            this.dgvErrors.AllowUserToResizeRows = false;
            this.dgvErrors.BackgroundColor = Color.White;
            this.dgvErrors.BorderStyle = BorderStyle.None;
            this.dgvErrors.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvErrors.Columns.AddRange(new DataGridViewColumn[] {
                this.colErrRow,
                this.colErrCol,
                this.colErrVal,
                this.colErrDesc
            });
            this.dgvErrors.Dock = DockStyle.Fill;
            this.dgvErrors.Location = new Point(4, 4);
            this.dgvErrors.Name = "dgvErrors";
            this.dgvErrors.ReadOnly = true;
            this.dgvErrors.RowHeadersVisible = false;
            this.dgvErrors.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            this.dgvErrors.Size = new Size(864, 412);
            this.dgvErrors.TabIndex = 0;

            // colErrRow
            this.colErrRow.HeaderText = "Row #";
            this.colErrRow.Name = "colErrRow";
            this.colErrRow.ReadOnly = true;
            this.colErrRow.Width = 65;

            // colErrCol
            this.colErrCol.HeaderText = "Field / Column";
            this.colErrCol.Name = "colErrCol";
            this.colErrCol.ReadOnly = true;
            this.colErrCol.Width = 140;

            // colErrVal
            this.colErrVal.HeaderText = "Value Entered";
            this.colErrVal.Name = "colErrVal";
            this.colErrVal.ReadOnly = true;
            this.colErrVal.Width = 150;

            // colErrDesc
            this.colErrDesc.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            this.colErrDesc.HeaderText = "Error Description";
            this.colErrDesc.Name = "colErrDesc";
            this.colErrDesc.ReadOnly = true;

            // 
            // tabPreview
            // 
            this.tabPreview.Controls.Add(this.dgvPreview);
            this.tabPreview.Location = new Point(4, 24);
            this.tabPreview.Name = "tabPreview";
            this.tabPreview.Padding = new Padding(4);
            this.tabPreview.Size = new Size(872, 420);
            this.tabPreview.Text = "Valid Products Preview (0)";
            this.tabPreview.UseVisualStyleBackColor = true;

            // dgvPreview
            this.dgvPreview.AllowUserToAddRows = false;
            this.dgvPreview.AllowUserToDeleteRows = false;
            this.dgvPreview.AllowUserToResizeRows = false;
            this.dgvPreview.BackgroundColor = Color.White;
            this.dgvPreview.BorderStyle = BorderStyle.None;
            this.dgvPreview.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvPreview.Columns.AddRange(new DataGridViewColumn[] {
                this.colPrevSku,
                this.colPrevName,
                this.colPrevCategory,
                this.colPrevBrand,
                this.colPrevColor,
                this.colPrevSize,
                this.colPrevGender,
                this.colPrevPurPrice,
                this.colPrevSellPrice,
                this.colPrevMinStock
            });
            this.dgvPreview.Dock = DockStyle.Fill;
            this.dgvPreview.Location = new Point(4, 4);
            this.dgvPreview.Name = "dgvPreview";
            this.dgvPreview.ReadOnly = true;
            this.dgvPreview.RowHeadersVisible = false;
            this.dgvPreview.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            this.dgvPreview.Size = new Size(864, 412);
            this.dgvPreview.TabIndex = 0;

            // colPrevSku
            this.colPrevSku.HeaderText = "SKU";
            this.colPrevSku.Name = "colPrevSku";
            this.colPrevSku.ReadOnly = true;
            this.colPrevSku.Width = 100;

            // colPrevName
            this.colPrevName.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            this.colPrevName.HeaderText = "Name";
            this.colPrevName.Name = "colPrevName";
            this.colPrevName.ReadOnly = true;

            // colPrevCategory
            this.colPrevCategory.HeaderText = "Category";
            this.colPrevCategory.Name = "colPrevCategory";
            this.colPrevCategory.ReadOnly = true;
            this.colPrevCategory.Width = 110;

            // colPrevBrand
            this.colPrevBrand.HeaderText = "Brand";
            this.colPrevBrand.Name = "colPrevBrand";
            this.colPrevBrand.ReadOnly = true;
            this.colPrevBrand.Width = 90;

            // colPrevColor
            this.colPrevColor.HeaderText = "Color";
            this.colPrevColor.Name = "colPrevColor";
            this.colPrevColor.ReadOnly = true;
            this.colPrevColor.Width = 70;

            // colPrevSize
            this.colPrevSize.HeaderText = "Size";
            this.colPrevSize.Name = "colPrevSize";
            this.colPrevSize.ReadOnly = true;
            this.colPrevSize.Width = 60;

            // colPrevGender
            this.colPrevGender.HeaderText = "Gender";
            this.colPrevGender.Name = "colPrevGender";
            this.colPrevGender.ReadOnly = true;
            this.colPrevGender.Width = 90;

            // colPrevPurPrice
            this.colPrevPurPrice.HeaderText = "Purchase (Rp)";
            this.colPrevPurPrice.Name = "colPrevPurPrice";
            this.colPrevPurPrice.ReadOnly = true;
            this.colPrevPurPrice.Width = 100;

            // colPrevSellPrice
            this.colPrevSellPrice.HeaderText = "Selling (Rp)";
            this.colPrevSellPrice.Name = "colPrevSellPrice";
            this.colPrevSellPrice.ReadOnly = true;
            this.colPrevSellPrice.Width = 100;

            // colPrevMinStock
            this.colPrevMinStock.HeaderText = "Min Stock";
            this.colPrevMinStock.Name = "colPrevMinStock";
            this.colPrevMinStock.ReadOnly = true;
            this.colPrevMinStock.Width = 80;

            // 
            // pnlBottom
            // 
            this.pnlBottom.BackColor = Color.FromArgb(245, 247, 250);
            this.pnlBottom.BorderStyle = BorderStyle.FixedSingle;
            this.pnlBottom.Controls.Add(this.btnCancel);
            this.pnlBottom.Controls.Add(this.btnImport);
            this.pnlBottom.Controls.Add(this.lblRowCountSummary);
            this.pnlBottom.Dock = DockStyle.Bottom;
            this.pnlBottom.Location = new Point(0, 546);
            this.pnlBottom.Name = "pnlBottom";
            this.pnlBottom.Padding = new Padding(14, 12, 14, 12);
            this.pnlBottom.Size = new Size(880, 54);
            this.pnlBottom.TabIndex = 3;

            // lblRowCountSummary
            this.lblRowCountSummary.AutoSize = true;
            this.lblRowCountSummary.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
            this.lblRowCountSummary.ForeColor = Color.FromArgb(70, 80, 95);
            this.lblRowCountSummary.Location = new Point(14, 18);
            this.lblRowCountSummary.Name = "lblRowCountSummary";
            this.lblRowCountSummary.Size = new Size(185, 15);
            this.lblRowCountSummary.Text = "Total rows: 0 | Valid: 0 | Errors: 0";

            // btnImport
            this.btnImport.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            this.btnImport.BackColor = Color.FromArgb(46, 204, 113);
            this.btnImport.Enabled = false;
            this.btnImport.FlatAppearance.BorderSize = 0;
            this.btnImport.FlatStyle = FlatStyle.Flat;
            this.btnImport.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            this.btnImport.ForeColor = Color.White;
            this.btnImport.Location = new Point(626, 11);
            this.btnImport.Name = "btnImport";
            this.btnImport.Size = new Size(150, 32);
            this.btnImport.TabIndex = 0;
            this.btnImport.Text = "Import Products";
            this.btnImport.UseVisualStyleBackColor = false;
            this.btnImport.Click += new EventHandler(this.btnImport_Click);

            // btnCancel
            this.btnCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            this.btnCancel.DialogResult = DialogResult.Cancel;
            this.btnCancel.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
            this.btnCancel.Location = new Point(784, 11);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new Size(82, 32);
            this.btnCancel.TabIndex = 1;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new EventHandler(this.btnCancel_Click);

            // 
            // ProductImportForm
            // 
            this.AcceptButton = this.btnImport;
            this.AutoScaleDimensions = new SizeF(7F, 15F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new Size(880, 600);
            this.Controls.Add(this.tabResults);
            this.Controls.Add(this.pnlBanner);
            this.Controls.Add(this.pnlTop);
            this.Controls.Add(this.pnlBottom);
            this.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new Size(760, 500);
            this.Name = "ProductImportForm";
            this.ShowInTaskbar = false;
            this.StartPosition = FormStartPosition.CenterParent;
            this.Text = "Import Products from Excel";

            this.pnlTop.ResumeLayout(false);
            this.pnlTop.PerformLayout();
            this.pnlBanner.ResumeLayout(false);
            this.tabResults.ResumeLayout(false);
            this.tabErrors.ResumeLayout(false);
            this.tabPreview.ResumeLayout(false);
            ((ISupportInitialize)(this.dgvErrors)).EndInit();
            ((ISupportInitialize)(this.dgvPreview)).EndInit();
            this.pnlBottom.ResumeLayout(false);
            this.pnlBottom.PerformLayout();
            this.ResumeLayout(false);
        }
    }
}
