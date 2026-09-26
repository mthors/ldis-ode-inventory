namespace LDIS.App.Forms
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;

        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel lblTotalProducts;
        private System.Windows.Forms.ToolStripStatusLabel lblLowStock;
        private System.Windows.Forms.ToolStripStatusLabel lblOutOfStock;
        private System.Windows.Forms.ToolStripStatusLabel lblStatus;
        private System.Windows.Forms.ToolStripStatusLabel lblDbPath;

        private System.Windows.Forms.Panel pnlHeader;
        private System.Windows.Forms.Label lblAppTitle;
        private System.Windows.Forms.Label lblAppSubtitle;
        private System.Windows.Forms.TextBox txtSearch;
        private System.Windows.Forms.Button btnSearch;
        private System.Windows.Forms.Button btnClearSearch;
        private System.Windows.Forms.Button btnNewProduct;
        private System.Windows.Forms.Button btnCategories;

        private System.Windows.Forms.Panel pnlSidebar;
        private System.Windows.Forms.Label lblFiltersTitle;
        private System.Windows.Forms.Label lblFilterCategory;
        private System.Windows.Forms.ComboBox cboFilterCategory;
        private System.Windows.Forms.Label lblFilterStatus;
        private System.Windows.Forms.ComboBox cboFilterStatus;
        private System.Windows.Forms.Button btnResetFilters;

        private System.Windows.Forms.Panel pnlMainContent;
        private System.Windows.Forms.Panel pnlGridToolbar;
        private System.Windows.Forms.Button btnStockIn;
        private System.Windows.Forms.Button btnStockOut;
        private System.Windows.Forms.Button btnStockAdjust;
        private System.Windows.Forms.Button btnTransactions;
        private System.Windows.Forms.Button btnEditProduct;
        private System.Windows.Forms.Button btnToggleStatus;
        private System.Windows.Forms.Button btnRefresh;
        private System.Windows.Forms.Label lblGridSummary;
        private System.Windows.Forms.ContextMenuStrip ctxProductMenu;
        private System.Windows.Forms.ToolStripMenuItem mnuStockIn;
        private System.Windows.Forms.ToolStripMenuItem mnuStockOut;
        private System.Windows.Forms.ToolStripMenuItem mnuStockAdjust;
        private System.Windows.Forms.ToolStripSeparator mnuSeparator1;
        private System.Windows.Forms.ToolStripMenuItem mnuViewHistory;
        private System.Windows.Forms.ToolStripSeparator mnuSeparator2;
        private System.Windows.Forms.ToolStripMenuItem mnuEditProduct;
        private System.Windows.Forms.ToolStripMenuItem mnuToggleStatus;
        private System.Windows.Forms.DataGridView dgvProducts;

        private System.Windows.Forms.DataGridViewTextBoxColumn colSKU;
        private System.Windows.Forms.DataGridViewTextBoxColumn colName;
        private System.Windows.Forms.DataGridViewTextBoxColumn colCategory;
        private System.Windows.Forms.DataGridViewTextBoxColumn colBrand;
        private System.Windows.Forms.DataGridViewTextBoxColumn colColor;
        private System.Windows.Forms.DataGridViewTextBoxColumn colSize;
        private System.Windows.Forms.DataGridViewTextBoxColumn colGender;
        private System.Windows.Forms.DataGridViewTextBoxColumn colPurchasePrice;
        private System.Windows.Forms.DataGridViewTextBoxColumn colSellingPrice;
        private System.Windows.Forms.DataGridViewTextBoxColumn colMinStock;
        private System.Windows.Forms.DataGridViewTextBoxColumn colCurrentStock;
        private System.Windows.Forms.DataGridViewTextBoxColumn colStatus;

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
            System.Windows.Forms.DataGridViewCellStyle cellStyleRight = new System.Windows.Forms.DataGridViewCellStyle();
            cellStyleRight.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;

            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.lblTotalProducts = new System.Windows.Forms.ToolStripStatusLabel();
            this.lblLowStock = new System.Windows.Forms.ToolStripStatusLabel();
            this.lblOutOfStock = new System.Windows.Forms.ToolStripStatusLabel();
            this.lblStatus = new System.Windows.Forms.ToolStripStatusLabel();
            this.lblDbPath = new System.Windows.Forms.ToolStripStatusLabel();

            this.pnlHeader = new System.Windows.Forms.Panel();
            this.lblAppTitle = new System.Windows.Forms.Label();
            this.lblAppSubtitle = new System.Windows.Forms.Label();
            this.txtSearch = new System.Windows.Forms.TextBox();
            this.btnSearch = new System.Windows.Forms.Button();
            this.btnClearSearch = new System.Windows.Forms.Button();
            this.btnNewProduct = new System.Windows.Forms.Button();
            this.btnCategories = new System.Windows.Forms.Button();

            this.pnlSidebar = new System.Windows.Forms.Panel();
            this.lblFiltersTitle = new System.Windows.Forms.Label();
            this.lblFilterCategory = new System.Windows.Forms.Label();
            this.cboFilterCategory = new System.Windows.Forms.ComboBox();
            this.lblFilterStatus = new System.Windows.Forms.Label();
            this.cboFilterStatus = new System.Windows.Forms.ComboBox();
            this.btnResetFilters = new System.Windows.Forms.Button();

            this.pnlMainContent = new System.Windows.Forms.Panel();
            this.pnlGridToolbar = new System.Windows.Forms.Panel();
            this.btnStockIn = new System.Windows.Forms.Button();
            this.btnStockOut = new System.Windows.Forms.Button();
            this.btnStockAdjust = new System.Windows.Forms.Button();
            this.btnTransactions = new System.Windows.Forms.Button();
            this.btnEditProduct = new System.Windows.Forms.Button();
            this.btnToggleStatus = new System.Windows.Forms.Button();
            this.btnRefresh = new System.Windows.Forms.Button();
            this.lblGridSummary = new System.Windows.Forms.Label();
            this.ctxProductMenu = new System.Windows.Forms.ContextMenuStrip();
            this.mnuStockIn = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuStockOut = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuStockAdjust = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.mnuViewHistory = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.mnuEditProduct = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuToggleStatus = new System.Windows.Forms.ToolStripMenuItem();
            this.dgvProducts = new System.Windows.Forms.DataGridView();

            this.colSKU = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colCategory = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colBrand = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colColor = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colSize = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colGender = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colPurchasePrice = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colSellingPrice = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colMinStock = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colCurrentStock = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colStatus = new System.Windows.Forms.DataGridViewTextBoxColumn();

            this.statusStrip.SuspendLayout();
            this.pnlHeader.SuspendLayout();
            this.pnlSidebar.SuspendLayout();
            this.pnlMainContent.SuspendLayout();
            this.pnlGridToolbar.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvProducts)).BeginInit();
            this.SuspendLayout();

            // 
            // pnlHeader
            // 
            this.pnlHeader.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(62)))), ((int)(((byte)(80)))));
            this.pnlHeader.Controls.Add(this.btnCategories);
            this.pnlHeader.Controls.Add(this.btnNewProduct);
            this.pnlHeader.Controls.Add(this.btnClearSearch);
            this.pnlHeader.Controls.Add(this.btnSearch);
            this.pnlHeader.Controls.Add(this.txtSearch);
            this.pnlHeader.Controls.Add(this.lblAppSubtitle);
            this.pnlHeader.Controls.Add(this.lblAppTitle);
            this.pnlHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlHeader.Location = new System.Drawing.Point(0, 0);
            this.pnlHeader.Name = "pnlHeader";
            this.pnlHeader.Size = new System.Drawing.Size(1008, 64);
            this.pnlHeader.TabIndex = 0;

            // lblAppTitle
            this.lblAppTitle.AutoSize = true;
            this.lblAppTitle.Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblAppTitle.ForeColor = System.Drawing.Color.White;
            this.lblAppTitle.Location = new System.Drawing.Point(16, 10);
            this.lblAppTitle.Name = "lblAppTitle";
            this.lblAppTitle.Size = new System.Drawing.Size(55, 25);
            this.lblAppTitle.Text = "LDIS";

            // lblAppSubtitle
            this.lblAppSubtitle.AutoSize = true;
            this.lblAppSubtitle.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblAppSubtitle.ForeColor = System.Drawing.Color.LightGray;
            this.lblAppSubtitle.Location = new System.Drawing.Point(18, 38);
            this.lblAppSubtitle.Name = "lblAppSubtitle";
            this.lblAppSubtitle.Size = new System.Drawing.Size(121, 13);
            this.lblAppSubtitle.Text = "Product Management";

            // txtSearch
            this.txtSearch.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.txtSearch.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtSearch.Location = new System.Drawing.Point(340, 20);
            this.txtSearch.Name = "txtSearch";
            this.txtSearch.Size = new System.Drawing.Size(260, 25);
            this.txtSearch.TabIndex = 0;
            this.txtSearch.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtSearch_KeyDown);

            // btnSearch
            this.btnSearch.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSearch.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(152)))), ((int)(((byte)(219)))));
            this.btnSearch.FlatAppearance.BorderSize = 0;
            this.btnSearch.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSearch.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSearch.ForeColor = System.Drawing.Color.White;
            this.btnSearch.Location = new System.Drawing.Point(606, 19);
            this.btnSearch.Name = "btnSearch";
            this.btnSearch.Size = new System.Drawing.Size(65, 27);
            this.btnSearch.TabIndex = 1;
            this.btnSearch.Text = "Search";
            this.btnSearch.UseVisualStyleBackColor = false;
            this.btnSearch.Click += new System.EventHandler(this.btnSearch_Click);

            // btnClearSearch
            this.btnClearSearch.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClearSearch.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(127)))), ((int)(((byte)(140)))), ((int)(((byte)(141)))));
            this.btnClearSearch.FlatAppearance.BorderSize = 0;
            this.btnClearSearch.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnClearSearch.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnClearSearch.ForeColor = System.Drawing.Color.White;
            this.btnClearSearch.Location = new System.Drawing.Point(676, 19);
            this.btnClearSearch.Name = "btnClearSearch";
            this.btnClearSearch.Size = new System.Drawing.Size(55, 27);
            this.btnClearSearch.TabIndex = 2;
            this.btnClearSearch.Text = "Clear";
            this.btnClearSearch.UseVisualStyleBackColor = false;
            this.btnClearSearch.Click += new System.EventHandler(this.btnClearSearch_Click);

            // btnNewProduct
            this.btnNewProduct.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnNewProduct.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(204)))), ((int)(((byte)(113)))));
            this.btnNewProduct.FlatAppearance.BorderSize = 0;
            this.btnNewProduct.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnNewProduct.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnNewProduct.ForeColor = System.Drawing.Color.White;
            this.btnNewProduct.Location = new System.Drawing.Point(746, 18);
            this.btnNewProduct.Name = "btnNewProduct";
            this.btnNewProduct.Size = new System.Drawing.Size(130, 29);
            this.btnNewProduct.TabIndex = 3;
            this.btnNewProduct.Text = "+ New Product";
            this.btnNewProduct.UseVisualStyleBackColor = false;
            this.btnNewProduct.Click += new System.EventHandler(this.btnNewProduct_Click);

            // btnCategories
            this.btnCategories.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCategories.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(142)))), ((int)(((byte)(68)))), ((int)(((byte)(173)))));
            this.btnCategories.FlatAppearance.BorderSize = 0;
            this.btnCategories.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCategories.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnCategories.ForeColor = System.Drawing.Color.White;
            this.btnCategories.Location = new System.Drawing.Point(884, 18);
            this.btnCategories.Name = "btnCategories";
            this.btnCategories.Size = new System.Drawing.Size(108, 29);
            this.btnCategories.TabIndex = 4;
            this.btnCategories.Text = "Categories...";
            this.btnCategories.UseVisualStyleBackColor = false;
            this.btnCategories.Click += new System.EventHandler(this.btnCategories_Click);

            // 
            // pnlSidebar
            // 
            this.pnlSidebar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(243)))), ((int)(((byte)(246)))));
            this.pnlSidebar.Controls.Add(this.btnResetFilters);
            this.pnlSidebar.Controls.Add(this.cboFilterStatus);
            this.pnlSidebar.Controls.Add(this.lblFilterStatus);
            this.pnlSidebar.Controls.Add(this.cboFilterCategory);
            this.pnlSidebar.Controls.Add(this.lblFilterCategory);
            this.pnlSidebar.Controls.Add(this.lblFiltersTitle);
            this.pnlSidebar.Dock = System.Windows.Forms.DockStyle.Left;
            this.pnlSidebar.Location = new System.Drawing.Point(0, 64);
            this.pnlSidebar.Name = "pnlSidebar";
            this.pnlSidebar.Padding = new System.Windows.Forms.Padding(15);
            this.pnlSidebar.Size = new System.Drawing.Size(210, 643);
            this.pnlSidebar.TabIndex = 1;

            // lblFiltersTitle
            this.lblFiltersTitle.AutoSize = true;
            this.lblFiltersTitle.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblFiltersTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(62)))), ((int)(((byte)(80)))));
            this.lblFiltersTitle.Location = new System.Drawing.Point(15, 15);
            this.lblFiltersTitle.Name = "lblFiltersTitle";
            this.lblFiltersTitle.Size = new System.Drawing.Size(51, 19);
            this.lblFiltersTitle.Text = "Filters";

            // lblFilterCategory
            this.lblFilterCategory.AutoSize = true;
            this.lblFilterCategory.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblFilterCategory.Location = new System.Drawing.Point(15, 50);
            this.lblFilterCategory.Name = "lblFilterCategory";
            this.lblFilterCategory.Size = new System.Drawing.Size(58, 15);
            this.lblFilterCategory.Text = "Category:";

            // cboFilterCategory
            this.cboFilterCategory.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboFilterCategory.FormattingEnabled = true;
            this.cboFilterCategory.Location = new System.Drawing.Point(18, 70);
            this.cboFilterCategory.Name = "cboFilterCategory";
            this.cboFilterCategory.Size = new System.Drawing.Size(175, 23);
            this.cboFilterCategory.TabIndex = 0;
            this.cboFilterCategory.SelectedIndexChanged += new System.EventHandler(this.Filter_Changed);

            // lblFilterStatus
            this.lblFilterStatus.AutoSize = true;
            this.lblFilterStatus.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblFilterStatus.Location = new System.Drawing.Point(15, 110);
            this.lblFilterStatus.Name = "lblFilterStatus";
            this.lblFilterStatus.Size = new System.Drawing.Size(78, 15);
            this.lblFilterStatus.Text = "Status:";

            // cboFilterStatus
            this.cboFilterStatus.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboFilterStatus.FormattingEnabled = true;
            this.cboFilterStatus.Items.AddRange(new object[] {
            "Active Only",
            "Inactive Only",
            "All Products"});
            this.cboFilterStatus.Location = new System.Drawing.Point(18, 130);
            this.cboFilterStatus.Name = "cboFilterStatus";
            this.cboFilterStatus.Size = new System.Drawing.Size(175, 23);
            this.cboFilterStatus.TabIndex = 1;
            this.cboFilterStatus.SelectedIndexChanged += new System.EventHandler(this.Filter_Changed);

            // btnResetFilters
            this.btnResetFilters.Location = new System.Drawing.Point(18, 180);
            this.btnResetFilters.Name = "btnResetFilters";
            this.btnResetFilters.Size = new System.Drawing.Size(175, 28);
            this.btnResetFilters.TabIndex = 2;
            this.btnResetFilters.Text = "Reset Filters";
            this.btnResetFilters.UseVisualStyleBackColor = true;
            this.btnResetFilters.Click += new System.EventHandler(this.btnResetFilters_Click);

            // 
            // pnlMainContent
            // 
            this.pnlMainContent.Controls.Add(this.dgvProducts);
            this.pnlMainContent.Controls.Add(this.pnlGridToolbar);
            this.pnlMainContent.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlMainContent.Location = new System.Drawing.Point(210, 64);
            this.pnlMainContent.Name = "pnlMainContent";
            this.pnlMainContent.Padding = new System.Windows.Forms.Padding(10);
            this.pnlMainContent.Size = new System.Drawing.Size(798, 643);
            this.pnlMainContent.TabIndex = 2;

            // pnlGridToolbar
            this.pnlGridToolbar.Controls.Add(this.btnStockIn);
            this.pnlGridToolbar.Controls.Add(this.btnStockOut);
            this.pnlGridToolbar.Controls.Add(this.btnStockAdjust);
            this.pnlGridToolbar.Controls.Add(this.btnTransactions);
            this.pnlGridToolbar.Controls.Add(this.btnEditProduct);
            this.pnlGridToolbar.Controls.Add(this.btnToggleStatus);
            this.pnlGridToolbar.Controls.Add(this.btnRefresh);
            this.pnlGridToolbar.Controls.Add(this.lblGridSummary);
            this.pnlGridToolbar.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlGridToolbar.Location = new System.Drawing.Point(10, 10);
            this.pnlGridToolbar.Name = "pnlGridToolbar";
            this.pnlGridToolbar.Size = new System.Drawing.Size(778, 38);
            this.pnlGridToolbar.TabIndex = 0;

            // btnStockIn
            this.btnStockIn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(204)))), ((int)(((byte)(113)))));
            this.btnStockIn.FlatAppearance.BorderSize = 0;
            this.btnStockIn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnStockIn.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnStockIn.ForeColor = System.Drawing.Color.White;
            this.btnStockIn.Location = new System.Drawing.Point(0, 4);
            this.btnStockIn.Name = "btnStockIn";
            this.btnStockIn.Size = new System.Drawing.Size(92, 28);
            this.btnStockIn.TabIndex = 0;
            this.btnStockIn.Text = "+ Stock &IN";
            this.btnStockIn.UseVisualStyleBackColor = false;
            this.btnStockIn.Click += new System.EventHandler(this.btnStockIn_Click);

            // btnStockOut
            this.btnStockOut.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(126)))), ((int)(((byte)(34)))));
            this.btnStockOut.FlatAppearance.BorderSize = 0;
            this.btnStockOut.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnStockOut.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnStockOut.ForeColor = System.Drawing.Color.White;
            this.btnStockOut.Location = new System.Drawing.Point(97, 4);
            this.btnStockOut.Name = "btnStockOut";
            this.btnStockOut.Size = new System.Drawing.Size(98, 28);
            this.btnStockOut.TabIndex = 1;
            this.btnStockOut.Text = "- Stock &OUT";
            this.btnStockOut.UseVisualStyleBackColor = false;
            this.btnStockOut.Click += new System.EventHandler(this.btnStockOut_Click);

            // btnStockAdjust
            this.btnStockAdjust.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(142)))), ((int)(((byte)(68)))), ((int)(((byte)(173)))));
            this.btnStockAdjust.FlatAppearance.BorderSize = 0;
            this.btnStockAdjust.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnStockAdjust.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnStockAdjust.ForeColor = System.Drawing.Color.White;
            this.btnStockAdjust.Location = new System.Drawing.Point(200, 4);
            this.btnStockAdjust.Name = "btnStockAdjust";
            this.btnStockAdjust.Size = new System.Drawing.Size(80, 28);
            this.btnStockAdjust.TabIndex = 2;
            this.btnStockAdjust.Text = "&Adjust...";
            this.btnStockAdjust.UseVisualStyleBackColor = false;
            this.btnStockAdjust.Click += new System.EventHandler(this.btnStockAdjust_Click);

            // btnTransactions
            this.btnTransactions.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(152)))), ((int)(((byte)(219)))));
            this.btnTransactions.FlatAppearance.BorderSize = 0;
            this.btnTransactions.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnTransactions.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnTransactions.ForeColor = System.Drawing.Color.White;
            this.btnTransactions.Location = new System.Drawing.Point(285, 4);
            this.btnTransactions.Name = "btnTransactions";
            this.btnTransactions.Size = new System.Drawing.Size(85, 28);
            this.btnTransactions.TabIndex = 3;
            this.btnTransactions.Text = "&History...";
            this.btnTransactions.UseVisualStyleBackColor = false;
            this.btnTransactions.Click += new System.EventHandler(this.btnTransactions_Click);

            // btnEditProduct
            this.btnEditProduct.Location = new System.Drawing.Point(375, 4);
            this.btnEditProduct.Name = "btnEditProduct";
            this.btnEditProduct.Size = new System.Drawing.Size(88, 28);
            this.btnEditProduct.TabIndex = 4;
            this.btnEditProduct.Text = "&Edit Product";
            this.btnEditProduct.UseVisualStyleBackColor = true;
            this.btnEditProduct.Click += new System.EventHandler(this.btnEditProduct_Click);

            // btnToggleStatus
            this.btnToggleStatus.Location = new System.Drawing.Point(468, 4);
            this.btnToggleStatus.Name = "btnToggleStatus";
            this.btnToggleStatus.Size = new System.Drawing.Size(95, 28);
            this.btnToggleStatus.TabIndex = 5;
            this.btnToggleStatus.Text = "&Deactivate";
            this.btnToggleStatus.UseVisualStyleBackColor = true;
            this.btnToggleStatus.Click += new System.EventHandler(this.btnToggleStatus_Click);

            // btnRefresh
            this.btnRefresh.Location = new System.Drawing.Point(568, 4);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(85, 28);
            this.btnRefresh.TabIndex = 6;
            this.btnRefresh.Text = "&Refresh [F5]";
            this.btnRefresh.UseVisualStyleBackColor = true;
            this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);

            // lblGridSummary
            this.lblGridSummary.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblGridSummary.AutoSize = true;
            this.lblGridSummary.ForeColor = System.Drawing.SystemColors.GrayText;
            this.lblGridSummary.Location = new System.Drawing.Point(658, 11);
            this.lblGridSummary.Name = "lblGridSummary";
            this.lblGridSummary.Size = new System.Drawing.Size(115, 15);
            this.lblGridSummary.Text = "Right-click for menu";
            this.lblGridSummary.TextAlign = System.Drawing.ContentAlignment.MiddleRight;

            // ctxProductMenu
            this.ctxProductMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
                this.mnuStockIn,
                this.mnuStockOut,
                this.mnuStockAdjust,
                this.mnuSeparator1,
                this.mnuViewHistory,
                this.mnuSeparator2,
                this.mnuEditProduct,
                this.mnuToggleStatus
            });
            this.ctxProductMenu.Name = "ctxProductMenu";
            this.ctxProductMenu.Size = new System.Drawing.Size(215, 170);

            // mnuStockIn
            this.mnuStockIn.Text = "Stock &IN... (Ctrl+I)";
            this.mnuStockIn.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.mnuStockIn.Click += new System.EventHandler(this.btnStockIn_Click);

            // mnuStockOut
            this.mnuStockOut.Text = "Stock &OUT... (Ctrl+O)";
            this.mnuStockOut.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.mnuStockOut.Click += new System.EventHandler(this.btnStockOut_Click);

            // mnuStockAdjust
            this.mnuStockAdjust.Text = "Stock &Adjustment...";
            this.mnuStockAdjust.Click += new System.EventHandler(this.btnStockAdjust_Click);

            // mnuViewHistory
            this.mnuViewHistory.Text = "View &Transaction History... (Ctrl+T)";
            this.mnuViewHistory.Click += new System.EventHandler(this.btnTransactions_Click);

            // mnuEditProduct
            this.mnuEditProduct.Text = "&Edit Product Details...";
            this.mnuEditProduct.Click += new System.EventHandler(this.btnEditProduct_Click);

            // mnuToggleStatus
            this.mnuToggleStatus.Text = "&Deactivate Product";
            this.mnuToggleStatus.Click += new System.EventHandler(this.btnToggleStatus_Click);

            // dgvProducts
            this.dgvProducts.AllowUserToAddRows = false;
            this.dgvProducts.AllowUserToDeleteRows = false;
            this.dgvProducts.AllowUserToResizeRows = false;
            this.dgvProducts.BackgroundColor = System.Drawing.Color.White;
            this.dgvProducts.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.dgvProducts.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvProducts.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colSKU,
            this.colName,
            this.colCategory,
            this.colBrand,
            this.colColor,
            this.colSize,
            this.colGender,
            this.colPurchasePrice,
            this.colSellingPrice,
            this.colMinStock,
            this.colCurrentStock,
            this.colStatus});
            this.dgvProducts.ContextMenuStrip = this.ctxProductMenu;
            this.dgvProducts.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvProducts.Location = new System.Drawing.Point(10, 48);
            this.dgvProducts.MultiSelect = false;
            this.dgvProducts.Name = "dgvProducts";
            this.dgvProducts.ReadOnly = true;
            this.dgvProducts.RowHeadersVisible = false;
            this.dgvProducts.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvProducts.Size = new System.Drawing.Size(778, 585);
            this.dgvProducts.TabIndex = 1;
            this.dgvProducts.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvProducts_CellDoubleClick);
            this.dgvProducts.SelectionChanged += new System.EventHandler(this.dgvProducts_SelectionChanged);
            this.dgvProducts.CellFormatting += new System.Windows.Forms.DataGridViewCellFormattingEventHandler(this.dgvProducts_CellFormatting);

            // Columns setup
            this.colSKU.DataPropertyName = "SKU";
            this.colSKU.HeaderText = "SKU";
            this.colSKU.Width = 110;

            this.colName.DataPropertyName = "Name";
            this.colName.HeaderText = "Product Name";
            this.colName.Width = 180;

            this.colCategory.DataPropertyName = "CategoryName";
            this.colCategory.HeaderText = "Category";
            this.colCategory.Width = 110;

            this.colBrand.DataPropertyName = "Brand";
            this.colBrand.HeaderText = "Brand";
            this.colBrand.Width = 90;

            this.colColor.DataPropertyName = "Color";
            this.colColor.HeaderText = "Color";
            this.colColor.Width = 80;

            this.colSize.DataPropertyName = "Size";
            this.colSize.HeaderText = "Size";
            this.colSize.Width = 60;

            this.colGender.DataPropertyName = "Gender";
            this.colGender.HeaderText = "Gender";
            this.colGender.Width = 80;

            this.colPurchasePrice.DataPropertyName = "PurchasePriceFormatted";
            this.colPurchasePrice.DefaultCellStyle = cellStyleRight;
            this.colPurchasePrice.HeaderText = "Purchase (Rp)";
            this.colPurchasePrice.Width = 110;

            this.colSellingPrice.DataPropertyName = "SellingPriceFormatted";
            this.colSellingPrice.DefaultCellStyle = cellStyleRight;
            this.colSellingPrice.HeaderText = "Selling (Rp)";
            this.colSellingPrice.Width = 110;

            this.colMinStock.DataPropertyName = "MinStockLevel";
            this.colMinStock.DefaultCellStyle = cellStyleRight;
            this.colMinStock.HeaderText = "Min";
            this.colMinStock.Width = 55;

            this.colCurrentStock.DataPropertyName = "CurrentStock";
            this.colCurrentStock.DefaultCellStyle = cellStyleRight;
            this.colCurrentStock.HeaderText = "Stock";
            this.colCurrentStock.Width = 60;

            this.colStatus.DataPropertyName = "StatusText";
            this.colStatus.HeaderText = "Status";
            this.colStatus.Width = 75;

            // 
            // statusStrip
            // 
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.lblTotalProducts,
            this.lblLowStock,
            this.lblOutOfStock,
            this.lblStatus,
            this.lblDbPath});
            this.statusStrip.Location = new System.Drawing.Point(0, 707);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Size = new System.Drawing.Size(1008, 22);
            this.statusStrip.TabIndex = 3;

            // lblTotalProducts
            this.lblTotalProducts.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Right;
            this.lblTotalProducts.Name = "lblTotalProducts";
            this.lblTotalProducts.Size = new System.Drawing.Size(102, 17);
            this.lblTotalProducts.Text = "Total Products: 0";

            // lblLowStock
            this.lblLowStock.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Right;
            this.lblLowStock.Name = "lblLowStock";
            this.lblLowStock.Size = new System.Drawing.Size(76, 17);
            this.lblLowStock.Text = "Low Stock: 0";

            // lblOutOfStock
            this.lblOutOfStock.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Right;
            this.lblOutOfStock.Name = "lblOutOfStock";
            this.lblOutOfStock.Size = new System.Drawing.Size(89, 17);
            this.lblOutOfStock.Text = "Out of Stock: 0";

            // lblStatus
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(42, 17);
            this.lblStatus.Text = "Ready";

            // lblDbPath
            this.lblDbPath.Name = "lblDbPath";
            this.lblDbPath.Size = new System.Drawing.Size(684, 17);
            this.lblDbPath.Spring = true;
            this.lblDbPath.TextAlign = System.Drawing.ContentAlignment.MiddleRight;

            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(247)))), ((int)(((byte)(250)))));
            this.ClientSize = new System.Drawing.Size(1008, 729);
            this.Controls.Add(this.pnlMainContent);
            this.Controls.Add(this.pnlSidebar);
            this.Controls.Add(this.pnlHeader);
            this.Controls.Add(this.statusStrip);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.KeyPreview = true;
            this.MinimumSize = new System.Drawing.Size(1024, 768);
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "LDIS - Lightweight Desktop Inventory System";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.MainForm_KeyDown);

            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.pnlHeader.ResumeLayout(false);
            this.pnlHeader.PerformLayout();
            this.pnlSidebar.ResumeLayout(false);
            this.pnlSidebar.PerformLayout();
            this.pnlMainContent.ResumeLayout(false);
            this.pnlGridToolbar.ResumeLayout(false);
            this.pnlGridToolbar.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvProducts)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}
