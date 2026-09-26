using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using LDIS.Core.DTOs;
using LDIS.Core.Services;

namespace LDIS.App.Forms
{
    public class TransactionHistoryForm : Form
    {
        private readonly IStockService _stockService;
        private readonly IExportService _exportService;
        private readonly long? _filterItemId;
        private readonly string _filterItemSku;

        // UI Controls
        private Panel pnlHeader;
        private Label lblTitle;
        private Label lblSubtitle;
        private TextBox txtSearch;
        private Button btnSearch;
        private Button btnClearSearch;

        private Panel pnlFilterBar;
        private Label lblTypeFilter;
        private ComboBox cboTypeFilter;
        private Label lblDateFilter;
        private ComboBox cboDateFilter;
        private DateTimePicker dtpStart;
        private DateTimePicker dtpEnd;
        private Button btnApplyFilters;
        private Button btnResetFilters;
        private Button btnExport;
        private Button btnRefresh;

        private DataGridView dgvTransactions;
        private StatusStrip statusStrip;
        private ToolStripStatusLabel lblTotalRecords;
        private ToolStripStatusLabel lblInCount;
        private ToolStripStatusLabel lblOutCount;
        private ToolStripStatusLabel lblAdjCount;

        private DataGridViewTextBoxColumn colId;
        private DataGridViewTextBoxColumn colDate;
        private DataGridViewTextBoxColumn colType;
        private DataGridViewTextBoxColumn colSku;
        private DataGridViewTextBoxColumn colItemName;
        private DataGridViewTextBoxColumn colQty;
        private DataGridViewTextBoxColumn colUnitPrice;
        private DataGridViewTextBoxColumn colRef;
        private DataGridViewTextBoxColumn colParty;
        private DataGridViewTextBoxColumn colReason;
        private DataGridViewTextBoxColumn colNotes;
        private DataGridViewTextBoxColumn colCreatedBy;

        private List<TransactionListItemDto> _currentTransactions = new List<TransactionListItemDto>();

        public TransactionHistoryForm(IStockService stockService, long? itemId = null, string itemSku = null)
            : this(stockService, new ExportService(), itemId, itemSku)
        {
        }

        public TransactionHistoryForm(IStockService stockService, IExportService exportService, long? itemId = null, string itemSku = null)
        {
            if (stockService == null) throw new ArgumentNullException("stockService");

            _stockService = stockService;
            _exportService = exportService ?? new ExportService();
            _filterItemId = itemId;
            _filterItemSku = itemSku;

            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.pnlHeader = new Panel();
            this.lblTitle = new Label();
            this.lblSubtitle = new Label();
            this.txtSearch = new TextBox();
            this.btnSearch = new Button();
            this.btnClearSearch = new Button();

            this.pnlFilterBar = new Panel();
            this.lblTypeFilter = new Label();
            this.cboTypeFilter = new ComboBox();
            this.lblDateFilter = new Label();
            this.cboDateFilter = new ComboBox();
            this.dtpStart = new DateTimePicker();
            this.dtpEnd = new DateTimePicker();
            this.btnApplyFilters = new Button();
            this.btnResetFilters = new Button();
            this.btnExport = new Button();
            this.btnRefresh = new Button();

            this.dgvTransactions = new DataGridView();
            this.statusStrip = new StatusStrip();
            this.lblTotalRecords = new ToolStripStatusLabel();
            this.lblInCount = new ToolStripStatusLabel();
            this.lblOutCount = new ToolStripStatusLabel();
            this.lblAdjCount = new ToolStripStatusLabel();

            this.colId = new DataGridViewTextBoxColumn();
            this.colDate = new DataGridViewTextBoxColumn();
            this.colType = new DataGridViewTextBoxColumn();
            this.colSku = new DataGridViewTextBoxColumn();
            this.colItemName = new DataGridViewTextBoxColumn();
            this.colQty = new DataGridViewTextBoxColumn();
            this.colUnitPrice = new DataGridViewTextBoxColumn();
            this.colRef = new DataGridViewTextBoxColumn();
            this.colParty = new DataGridViewTextBoxColumn();
            this.colReason = new DataGridViewTextBoxColumn();
            this.colNotes = new DataGridViewTextBoxColumn();
            this.colCreatedBy = new DataGridViewTextBoxColumn();

            this.SuspendLayout();

            // Form
            this.Text = string.IsNullOrEmpty(_filterItemSku) ? "Transaction History" : string.Format("Transaction History - [{0}]", _filterItemSku);
            this.ClientSize = new Size(1100, 600);
            this.MinimumSize = new Size(900, 500);
            this.StartPosition = FormStartPosition.CenterParent;
            this.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
            this.KeyPreview = true;
            this.KeyDown += TransactionHistoryForm_KeyDown;

            // Header Panel
            this.pnlHeader.Dock = DockStyle.Top;
            this.pnlHeader.Height = 56;
            this.pnlHeader.BackColor = Color.FromArgb(45, 62, 80);

            this.lblTitle.Text = "Inventory Transactions";
            this.lblTitle.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            this.lblTitle.ForeColor = Color.White;
            this.lblTitle.Location = new Point(16, 8);
            this.lblTitle.Size = new Size(240, 22);

            this.lblSubtitle.Text = string.IsNullOrEmpty(_filterItemSku) ? "Audit trail of all stock movements" : string.Format("Filtered for product: {0}", _filterItemSku);
            this.lblSubtitle.Font = new Font("Segoe UI", 8.25F);
            this.lblSubtitle.ForeColor = Color.LightGray;
            this.lblSubtitle.Location = new Point(18, 30);
            this.lblSubtitle.Size = new Size(350, 18);

            this.txtSearch.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            this.txtSearch.Location = new Point(690, 16);
            this.txtSearch.Size = new Size(230, 23);
            this.txtSearch.KeyDown += txtSearch_KeyDown;

            this.btnSearch.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            this.btnSearch.Text = "Search";
            this.btnSearch.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            this.btnSearch.BackColor = Color.FromArgb(52, 152, 219);
            this.btnSearch.ForeColor = Color.White;
            this.btnSearch.FlatStyle = FlatStyle.Flat;
            this.btnSearch.FlatAppearance.BorderSize = 0;
            this.btnSearch.Location = new Point(926, 15);
            this.btnSearch.Size = new Size(70, 25);
            this.btnSearch.Click += btnSearch_Click;

            this.btnClearSearch.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            this.btnClearSearch.Text = "Clear";
            this.btnClearSearch.BackColor = Color.FromArgb(127, 140, 141);
            this.btnClearSearch.ForeColor = Color.White;
            this.btnClearSearch.FlatStyle = FlatStyle.Flat;
            this.btnClearSearch.FlatAppearance.BorderSize = 0;
            this.btnClearSearch.Location = new Point(1002, 15);
            this.btnClearSearch.Size = new Size(60, 25);
            this.btnClearSearch.Click += btnClearSearch_Click;

            this.pnlHeader.Controls.Add(this.lblTitle);
            this.pnlHeader.Controls.Add(this.lblSubtitle);
            this.pnlHeader.Controls.Add(this.txtSearch);
            this.pnlHeader.Controls.Add(this.btnSearch);
            this.pnlHeader.Controls.Add(this.btnClearSearch);

            // Filter Bar
            this.pnlFilterBar.Dock = DockStyle.Top;
            this.pnlFilterBar.Height = 44;
            this.pnlFilterBar.BackColor = Color.FromArgb(240, 243, 246);
            this.pnlFilterBar.BorderStyle = BorderStyle.FixedSingle;

            this.lblTypeFilter.Text = "Type:";
            this.lblTypeFilter.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            this.lblTypeFilter.Location = new Point(16, 12);
            this.lblTypeFilter.Size = new Size(40, 20);

            this.cboTypeFilter.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cboTypeFilter.Items.AddRange(new object[] { "All Types", "Stock IN", "Stock OUT", "Adjustment" });
            this.cboTypeFilter.SelectedIndex = 0;
            this.cboTypeFilter.Location = new Point(60, 9);
            this.cboTypeFilter.Size = new Size(110, 23);
            this.cboTypeFilter.SelectedIndexChanged += Filter_Changed;

            this.lblDateFilter.Text = "Date:";
            this.lblDateFilter.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            this.lblDateFilter.Location = new Point(190, 12);
            this.lblDateFilter.Size = new Size(40, 20);

            this.cboDateFilter.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cboDateFilter.Items.AddRange(new object[] { "All Time", "Today", "Yesterday", "Last 7 Days", "This Month", "Custom Range" });
            this.cboDateFilter.SelectedIndex = 0;
            this.cboDateFilter.Location = new Point(234, 9);
            this.cboDateFilter.Size = new Size(120, 23);
            this.cboDateFilter.SelectedIndexChanged += cboDateFilter_SelectedIndexChanged;

            this.dtpStart.Format = DateTimePickerFormat.Short;
            this.dtpStart.Location = new Point(364, 9);
            this.dtpStart.Size = new Size(95, 23);
            this.dtpStart.Visible = false;
            this.dtpStart.ValueChanged += Filter_Changed;

            this.dtpEnd.Format = DateTimePickerFormat.Short;
            this.dtpEnd.Location = new Point(465, 9);
            this.dtpEnd.Size = new Size(95, 23);
            this.dtpEnd.Visible = false;
            this.dtpEnd.ValueChanged += Filter_Changed;

            this.btnResetFilters.Text = "Reset Filters";
            this.btnResetFilters.Location = new Point(570, 8);
            this.btnResetFilters.Size = new Size(90, 26);
            this.btnResetFilters.Click += btnResetFilters_Click;

            this.btnExport.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            this.btnExport.Text = "Export &CSV...";
            this.btnExport.Location = new Point(870, 8);
            this.btnExport.Size = new Size(100, 26);
            this.btnExport.Click += btnExport_Click;

            this.btnRefresh.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            this.btnRefresh.Text = "Refresh (F5)";
            this.btnRefresh.Location = new Point(980, 8);
            this.btnRefresh.Size = new Size(95, 26);
            this.btnRefresh.Click += delegate { LoadTransactions(); };

            this.pnlFilterBar.Controls.Add(this.lblTypeFilter);
            this.pnlFilterBar.Controls.Add(this.cboTypeFilter);
            this.pnlFilterBar.Controls.Add(this.lblDateFilter);
            this.pnlFilterBar.Controls.Add(this.cboDateFilter);
            this.pnlFilterBar.Controls.Add(this.dtpStart);
            this.pnlFilterBar.Controls.Add(this.dtpEnd);
            this.pnlFilterBar.Controls.Add(this.btnResetFilters);
            this.pnlFilterBar.Controls.Add(this.btnExport);
            this.pnlFilterBar.Controls.Add(this.btnRefresh);

            // DataGridView
            this.dgvTransactions.Dock = DockStyle.Fill;
            this.dgvTransactions.AllowUserToAddRows = false;
            this.dgvTransactions.AllowUserToDeleteRows = false;
            this.dgvTransactions.AllowUserToResizeRows = false;
            this.dgvTransactions.ReadOnly = true;
            this.dgvTransactions.MultiSelect = false;
            this.dgvTransactions.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            this.dgvTransactions.RowHeadersVisible = false;
            this.dgvTransactions.AutoGenerateColumns = false;
            this.dgvTransactions.BackgroundColor = Color.White;
            this.dgvTransactions.BorderStyle = BorderStyle.None;
            this.dgvTransactions.CellFormatting += dgvTransactions_CellFormatting;

            // Columns definition
            this.colId.DataPropertyName = "TransactionID";
            this.colId.HeaderText = "ID";
            this.colId.Width = 55;
            this.colId.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            this.colDate.DataPropertyName = "TransactionDate";
            this.colDate.HeaderText = "Date";
            this.colDate.Width = 125;

            this.colType.DataPropertyName = "TransactionType";
            this.colType.HeaderText = "Type";
            this.colType.Width = 85;
            this.colType.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            this.colSku.DataPropertyName = "SKU";
            this.colSku.HeaderText = "SKU";
            this.colSku.Width = 110;

            this.colItemName.DataPropertyName = "ItemName";
            this.colItemName.HeaderText = "Item Name";
            this.colItemName.Width = 170;

            this.colQty.DataPropertyName = "Quantity";
            this.colQty.HeaderText = "Quantity";
            this.colQty.Width = 75;
            this.colQty.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;

            this.colUnitPrice.DataPropertyName = "UnitPrice";
            this.colUnitPrice.HeaderText = "Unit Price";
            this.colUnitPrice.Width = 100;
            this.colUnitPrice.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            this.colUnitPrice.DefaultCellStyle.Format = "N0";

            this.colRef.DataPropertyName = "ReferenceNumber";
            this.colRef.HeaderText = "Reference #";
            this.colRef.Width = 110;

            this.colParty.DataPropertyName = "SupplierOrCustomer";
            this.colParty.HeaderText = "Supplier / Customer";
            this.colParty.Width = 150;

            this.colReason.DataPropertyName = "Reason";
            this.colReason.HeaderText = "Reason";
            this.colReason.Width = 140;

            this.colNotes.DataPropertyName = "Notes";
            this.colNotes.HeaderText = "Notes";
            this.colNotes.Width = 160;

            this.colCreatedBy.DataPropertyName = "CreatedBy";
            this.colCreatedBy.HeaderText = "Created By";
            this.colCreatedBy.Width = 90;

            this.dgvTransactions.Columns.AddRange(new DataGridViewColumn[] {
                this.colId,
                this.colDate,
                this.colType,
                this.colSku,
                this.colItemName,
                this.colQty,
                this.colUnitPrice,
                this.colRef,
                this.colParty,
                this.colReason,
                this.colNotes,
                this.colCreatedBy
            });

            // Status Strip
            this.statusStrip.Items.AddRange(new ToolStripItem[] {
                this.lblTotalRecords,
                this.lblInCount,
                this.lblOutCount,
                this.lblAdjCount
            });

            this.lblTotalRecords.Text = "Total Records: 0";
            this.lblInCount.Text = "Stock IN: 0";
            this.lblInCount.ForeColor = Color.FromArgb(39, 174, 96);
            this.lblOutCount.Text = "Stock OUT: 0";
            this.lblOutCount.ForeColor = Color.FromArgb(211, 84, 0);
            this.lblAdjCount.Text = "Adjustments: 0";
            this.lblAdjCount.ForeColor = Color.FromArgb(142, 68, 173);

            // Form composition
            this.Controls.Add(this.dgvTransactions);
            this.Controls.Add(this.statusStrip);
            this.Controls.Add(this.pnlFilterBar);
            this.Controls.Add(this.pnlHeader);

            this.ResumeLayout(false);
            this.PerformLayout();

            this.Load += TransactionHistoryForm_Load;
        }

        private void TransactionHistoryForm_Load(object sender, EventArgs e)
        {
            // Enable double buffering for smooth DataGridView scrolling on Windows 7
            typeof(DataGridView).InvokeMember(
                "DoubleBuffered",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.SetProperty,
                null,
                dgvTransactions,
                new object[] { true }
            );

            dtpStart.Value = DateTime.Today.AddDays(-30);
            dtpEnd.Value = DateTime.Today;

            LoadTransactions();
        }

        private void TransactionHistoryForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                this.Close();
            }
            else if (e.KeyCode == Keys.F5)
            {
                LoadTransactions();
            }
            else if (e.Control && e.KeyCode == Keys.F)
            {
                txtSearch.Focus();
                txtSearch.SelectAll();
            }
        }

        private void cboDateFilter_SelectedIndexChanged(object sender, EventArgs e)
        {
            bool isCustom = cboDateFilter.SelectedIndex == 5;
            dtpStart.Visible = isCustom;
            dtpEnd.Visible = isCustom;
            LoadTransactions();
        }

        private void Filter_Changed(object sender, EventArgs e)
        {
            LoadTransactions();
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            LoadTransactions();
        }

        private void btnClearSearch_Click(object sender, EventArgs e)
        {
            txtSearch.Text = string.Empty;
            LoadTransactions();
            txtSearch.Focus();
        }

        private void txtSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
                LoadTransactions();
            }
        }

        private void btnResetFilters_Click(object sender, EventArgs e)
        {
            txtSearch.Text = string.Empty;
            cboTypeFilter.SelectedIndex = 0;
            cboDateFilter.SelectedIndex = 0;
            LoadTransactions();
        }

        private void LoadTransactions()
        {
            try
            {
                var criteria = new TransactionSearchCriteria
                {
                    SearchText = txtSearch.Text.Trim(),
                    ItemID = _filterItemId
                };

                // Type filter
                int typeIdx = cboTypeFilter.SelectedIndex;
                if (typeIdx == 1) criteria.TransactionType = "IN";
                else if (typeIdx == 2) criteria.TransactionType = "OUT";
                else if (typeIdx == 3) criteria.TransactionType = "ADJUSTMENT";
                else criteria.TransactionType = null;

                // Date filter
                int dateIdx = cboDateFilter.SelectedIndex;
                if (dateIdx == 1) // Today
                {
                    criteria.StartDate = DateTime.Today;
                    criteria.EndDate = DateTime.Today;
                }
                else if (dateIdx == 2) // Yesterday
                {
                    criteria.StartDate = DateTime.Today.AddDays(-1);
                    criteria.EndDate = DateTime.Today.AddDays(-1);
                }
                else if (dateIdx == 3) // Last 7 Days
                {
                    criteria.StartDate = DateTime.Today.AddDays(-6);
                    criteria.EndDate = DateTime.Today;
                }
                else if (dateIdx == 4) // This Month
                {
                    criteria.StartDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
                    criteria.EndDate = DateTime.Today;
                }
                else if (dateIdx == 5) // Custom Range
                {
                    criteria.StartDate = dtpStart.Value.Date;
                    criteria.EndDate = dtpEnd.Value.Date;
                }
                else // All Time
                {
                    criteria.StartDate = null;
                    criteria.EndDate = null;
                }

                _currentTransactions = _stockService.SearchTransactions(criteria).ToList();

                dgvTransactions.DataSource = null;
                dgvTransactions.DataSource = _currentTransactions;

                UpdateStatusBar();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to load transaction history:\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UpdateStatusBar()
        {
            int total = _currentTransactions.Count;
            int countIn = _currentTransactions.Count(t => t.TransactionType == "IN");
            int countOut = _currentTransactions.Count(t => t.TransactionType == "OUT");
            int countAdj = _currentTransactions.Count(t => t.TransactionType == "ADJUSTMENT");

            lblTotalRecords.Text = string.Format("Total Records: {0:N0}   |   ", total);
            lblInCount.Text = string.Format("Stock IN: {0:N0}   |   ", countIn);
            lblOutCount.Text = string.Format("Stock OUT: {0:N0}   |   ", countOut);
            lblAdjCount.Text = string.Format("Adjustments: {0:N0}", countAdj);
        }

        private void dgvTransactions_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex >= 0 && e.RowIndex < dgvTransactions.Rows.Count)
            {
                var item = dgvTransactions.Rows[e.RowIndex].DataBoundItem as TransactionListItemDto;
                if (item != null)
                {
                    if (dgvTransactions.Columns[e.ColumnIndex] == colDate && e.Value is DateTime)
                    {
                        DateTime dt = (DateTime)e.Value;
                        e.Value = dt.ToLocalTime().ToString("yyyy-MM-dd HH:mm");
                        e.FormattingApplied = true;
                    }
                    else if (dgvTransactions.Columns[e.ColumnIndex] == colType)
                    {
                        if (item.TransactionType == "IN")
                        {
                            e.CellStyle.ForeColor = Color.FromArgb(39, 174, 96);
                            e.CellStyle.Font = new Font(dgvTransactions.Font, FontStyle.Bold);
                        }
                        else if (item.TransactionType == "OUT")
                        {
                            e.CellStyle.ForeColor = Color.FromArgb(211, 84, 0);
                            e.CellStyle.Font = new Font(dgvTransactions.Font, FontStyle.Bold);
                        }
                        else if (item.TransactionType == "ADJUSTMENT")
                        {
                            e.CellStyle.ForeColor = Color.FromArgb(142, 68, 173);
                            e.CellStyle.Font = new Font(dgvTransactions.Font, FontStyle.Bold);
                        }
                    }
                    else if (dgvTransactions.Columns[e.ColumnIndex] == colQty)
                    {
                        if (item.TransactionType == "IN")
                        {
                            e.Value = string.Format("+{0:N0}", item.Quantity);
                            e.FormattingApplied = true;
                        }
                        else if (item.TransactionType == "OUT")
                        {
                            e.Value = string.Format("-{0:N0}", item.Quantity);
                            e.FormattingApplied = true;
                        }
                        else if (item.TransactionType == "ADJUSTMENT")
                        {
                            e.Value = string.Format("{0:+0;-0;0}", item.Quantity);
                            e.FormattingApplied = true;
                        }
                    }
                }
            }
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            try
            {
                if (_currentTransactions == null || _currentTransactions.Count == 0)
                {
                    MessageBox.Show(
                        this,
                        "There are no transactions to export matching the current criteria.",
                        "Export Transactions",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information
                    );
                    return;
                }

                using (var sfd = new SaveFileDialog())
                {
                    sfd.Title = "Export Transactions to CSV";
                    sfd.Filter = "CSV Files (*.csv)|*.csv|All Files (*.*)|*.*";
                    sfd.FilterIndex = 1;
                    sfd.DefaultExt = "csv";
                    sfd.AddExtension = true;
                    sfd.OverwritePrompt = true;
                    sfd.RestoreDirectory = true;
                    sfd.FileName = string.Format("transactions_export_{0:yyyyMMdd_HHmmss}.csv", DateTime.Now);

                    if (sfd.ShowDialog(this) == DialogResult.OK)
                    {
                        _exportService.ExportTransactionsToCsv(_currentTransactions, sfd.FileName);
                        MessageBox.Show(
                            this,
                            string.Format("Successfully exported {0:N0} transactions to:\n{1}", _currentTransactions.Count, sfd.FileName),
                            "Export Completed",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information
                        );
                    }
                }
            }
            catch (System.IO.IOException ioEx)
            {
                MessageBox.Show(
                    this,
                    "The file could not be saved because it is currently in use by another program (such as Microsoft Excel) or inaccessible.\n\nDetails: " + ioEx.Message,
                    "Export Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
            }
            catch (UnauthorizedAccessException authEx)
            {
                MessageBox.Show(
                    this,
                    "Access denied to the selected location. Please choose a folder where you have write permissions (such as Documents or Desktop).\n\nDetails: " + authEx.Message,
                    "Export Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    this,
                    "An unexpected error occurred during transaction export:\n" + ex.Message,
                    "Export Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }
    }
}
