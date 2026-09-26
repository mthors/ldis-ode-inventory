using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using LDIS.Core.Data;
using LDIS.Core.DTOs;
using LDIS.Core.Models;
using LDIS.Core.Services;

namespace LDIS.App.Forms
{
    public partial class MainForm : Form
    {
        private readonly DbConnectionFactory _connectionFactory;
        private readonly DatabaseInitializer _initializer;
        private readonly IItemService _itemService;
        private readonly ICategoryService _categoryService;
        private readonly IStockService _stockService;
        private readonly IExportService _exportService;
        private readonly IBackupService _backupService;

        private List<ItemListItemDto> _currentProducts = new List<ItemListItemDto>();
        private bool _isInitialLoading = true;

        public MainForm(
            DbConnectionFactory connectionFactory,
            DatabaseInitializer initializer,
            IItemService itemService,
            ICategoryService categoryService,
            IStockService stockService,
            IExportService exportService = null,
            IBackupService backupService = null)
        {
            if (connectionFactory == null) throw new ArgumentNullException("connectionFactory");
            if (initializer == null) throw new ArgumentNullException("initializer");
            if (itemService == null) throw new ArgumentNullException("itemService");
            if (categoryService == null) throw new ArgumentNullException("categoryService");
            if (stockService == null) throw new ArgumentNullException("stockService");

            _connectionFactory = connectionFactory;
            _initializer = initializer;
            _itemService = itemService;
            _categoryService = categoryService;
            _stockService = stockService;
            _exportService = exportService ?? new ExportService();
            _backupService = backupService ?? new BackupService(connectionFactory);

            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            try
            {
                var config = _connectionFactory.Config;
                lblDbPath.Text = string.Format("DB: {0} ({1})",
                    config.DatabaseFilePath,
                    config.IsPortableMode ? "Portable" : "Standard");

                // Enable double buffering for smooth DataGridView scrolling
                typeof(DataGridView).InvokeMember(
                    "DoubleBuffered",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.SetProperty,
                    null,
                    dgvProducts,
                    new object[] { true }
                );

                LoadCategoryFilter();
                cboFilterStockStatus.SelectedIndex = 0; // All Stock
                cboFilterGender.SelectedIndex = 0;      // All Genders
                cboFilterStatus.SelectedIndex = 0;      // Active Only

                _isInitialLoading = false;
                RefreshProductList();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error initializing main screen:\n" + ex.Message, "Startup Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.N)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
                btnNewProduct.PerformClick();
            }
            else if (e.Control && e.KeyCode == Keys.I)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
                btnStockIn.PerformClick();
            }
            else if (e.Control && e.KeyCode == Keys.O)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
                btnStockOut.PerformClick();
            }
            else if (e.Control && e.KeyCode == Keys.T)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
                btnTransactions.PerformClick();
            }
            else if (e.Control && e.KeyCode == Keys.F)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
                txtSearch.Focus();
                txtSearch.SelectAll();
            }
            else if (e.KeyCode == Keys.F5)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
                btnRefresh.PerformClick();
            }
        }

        private class CategoryFilterItem
        {
            public long? CategoryID { get; set; }
            public string Name { get; set; }

            public override string ToString()
            {
                return Name;
            }
        }

        private void LoadCategoryFilter()
        {
            long? selectedId = null;
            var current = cboFilterCategory.SelectedItem as CategoryFilterItem;
            if (current != null)
            {
                selectedId = current.CategoryID;
            }

            cboFilterCategory.BeginUpdate();
            cboFilterCategory.Items.Clear();

            cboFilterCategory.Items.Add(new CategoryFilterItem { CategoryID = null, Name = "All Categories" });

            try
            {
                var categories = _categoryService.GetAllCategories();
                CategoryFilterItem toSelect = null;

                foreach (var cat in categories)
                {
                    var item = new CategoryFilterItem { CategoryID = cat.CategoryID, Name = cat.CategoryName };
                    cboFilterCategory.Items.Add(item);

                    if (selectedId.HasValue && cat.CategoryID == selectedId.Value)
                    {
                        toSelect = item;
                    }
                }

                if (toSelect != null)
                {
                    cboFilterCategory.SelectedItem = toSelect;
                }
                else
                {
                    cboFilterCategory.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Failed to load categories: " + ex.Message;
            }
            finally
            {
                cboFilterCategory.EndUpdate();
            }
        }

        private void RefreshProductList()
        {
            if (_isInitialLoading) return;

            try
            {
                lblStatus.Text = "Loading products...";

                var criteria = new ItemSearchCriteria
                {
                    SearchText = txtSearch.Text.Trim()
                };

                var catItem = cboFilterCategory.SelectedItem as CategoryFilterItem;
                if (catItem != null && catItem.CategoryID.HasValue)
                {
                    criteria.CategoryID = catItem.CategoryID.Value;
                }

                // Stock status filter
                int stockStatusIndex = cboFilterStockStatus.SelectedIndex;
                if (stockStatusIndex == 1)
                {
                    criteria.StockStatus = StockFilterStatus.NormalStock;
                }
                else if (stockStatusIndex == 2)
                {
                    criteria.StockStatus = StockFilterStatus.LowStock;
                }
                else if (stockStatusIndex == 3)
                {
                    criteria.StockStatus = StockFilterStatus.OutOfStock;
                }
                else
                {
                    criteria.StockStatus = StockFilterStatus.All;
                }

                // Gender filter
                int genderIndex = cboFilterGender.SelectedIndex;
                if (genderIndex > 0 && genderIndex < cboFilterGender.Items.Count)
                {
                    criteria.Gender = cboFilterGender.SelectedItem.ToString();
                }

                // Status filter
                int statusIndex = cboFilterStatus.SelectedIndex;
                if (statusIndex == 0)
                {
                    criteria.ActiveStatus = ActiveFilterStatus.ActiveOnly;
                }
                else if (statusIndex == 1)
                {
                    criteria.ActiveStatus = ActiveFilterStatus.InactiveOnly;
                }
                else
                {
                    criteria.ActiveStatus = ActiveFilterStatus.All;
                }

                _currentProducts = _itemService.SearchItems(criteria).ToList();

                // Preserve selected item if possible
                long? previousSelectedId = GetSelectedProductId();

                dgvProducts.DataSource = null;
                dgvProducts.DataSource = _currentProducts;
                dgvProducts.Invalidate();

                if (previousSelectedId.HasValue)
                {
                    for (int i = 0; i < dgvProducts.Rows.Count; i++)
                    {
                        var rowItem = dgvProducts.Rows[i].DataBoundItem as ItemListItemDto;
                        if (rowItem != null && rowItem.ItemID == previousSelectedId.Value)
                        {
                            dgvProducts.Rows[i].Selected = true;
                            break;
                        }
                    }
                }

                UpdateStatusSummary();
                UpdateActionButtons();
                RefreshDashboardKPIs();

                if (_currentProducts.Count == 0)
                {
                    lblGridSummary.Text = "0 products found";
                    lblStatus.Text = "No products match the selected filters.";
                }
                else if (_currentProducts.Count == 1)
                {
                    lblGridSummary.Text = "1 product found | Right-click for menu";
                    lblStatus.Text = "Ready";
                }
                else
                {
                    lblGridSummary.Text = string.Format("{0:N0} products found | Right-click for menu", _currentProducts.Count);
                    lblStatus.Text = "Ready";
                }
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Error loading products: " + ex.Message;
                MessageBox.Show("Failed to load products:\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void RefreshDashboardKPIs()
        {
            try
            {
                var summary = _itemService.GetDashboardSummary();

                lblCardTotalProductsValue.Text = summary.TotalActiveProducts.ToString("N0");
                lblCardTotalUnitsValue.Text = summary.TotalUnitsInStock.ToString("N0");
                lblCardLowStockValue.Text = summary.LowStockCount.ToString("N0");
                lblCardOutOfStockValue.Text = summary.OutOfStockCount.ToString("N0");

                lblTotalProducts.Text = string.Format("Active Products: {0:N0}", summary.TotalActiveProducts);
                lblLowStock.Text = string.Format("Low Stock: {0:N0}", summary.LowStockCount);
                lblOutOfStock.Text = string.Format("Out of Stock: {0:N0}", summary.OutOfStockCount);
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Failed to refresh dashboard: " + ex.Message;
            }
        }

        private void UpdateStatusSummary()
        {
            // Kept for backward compatibility; status bar labels are updated via RefreshDashboardKPIs()
        }

        private ItemListItemDto GetSelectedProduct()
        {
            if (dgvProducts.SelectedRows.Count > 0)
            {
                return dgvProducts.SelectedRows[0].DataBoundItem as ItemListItemDto;
            }
            return null;
        }

        private long? GetSelectedProductId()
        {
            var item = GetSelectedProduct();
            return item != null ? (long?)item.ItemID : null;
        }

        private void UpdateActionButtons()
        {
            var selected = GetSelectedProduct();
            bool hasSelection = (selected != null);
            bool isActiveProduct = (hasSelection && selected.IsActive);

            btnStockIn.Enabled = isActiveProduct;
            btnStockOut.Enabled = (isActiveProduct && selected.CurrentStock > 0);
            btnStockAdjust.Enabled = isActiveProduct;

            mnuStockIn.Enabled = isActiveProduct;
            mnuStockOut.Enabled = (isActiveProduct && selected.CurrentStock > 0);
            mnuStockAdjust.Enabled = isActiveProduct;
            mnuViewHistory.Enabled = hasSelection;

            btnEditProduct.Enabled = hasSelection;
            mnuEditProduct.Enabled = hasSelection;
            btnToggleStatus.Enabled = hasSelection;
            mnuToggleStatus.Enabled = hasSelection;

            if (hasSelection)
            {
                if (selected.IsActive)
                {
                    btnToggleStatus.Text = "&Deactivate";
                    mnuToggleStatus.Text = "&Deactivate Product";
                }
                else
                {
                    btnToggleStatus.Text = "&Reactivate";
                    mnuToggleStatus.Text = "&Reactivate Product";
                }
            }
            else
            {
                btnToggleStatus.Text = "&Deactivate";
                mnuToggleStatus.Text = "&Deactivate Product";
            }
        }

        private void dgvProducts_SelectionChanged(object sender, EventArgs e)
        {
            UpdateActionButtons();
        }

        private void dgvProducts_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                EditSelectedProduct();
            }
        }

        private void dgvProducts_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex >= 0 && e.RowIndex < dgvProducts.Rows.Count)
            {
                var item = dgvProducts.Rows[e.RowIndex].DataBoundItem as ItemListItemDto;
                if (item != null && !item.IsActive)
                {
                    e.CellStyle.ForeColor = Color.DarkGray;
                    e.CellStyle.Font = new Font(dgvProducts.Font, FontStyle.Italic);
                }
            }
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            RefreshProductList();
        }

        private void btnClearSearch_Click(object sender, EventArgs e)
        {
            txtSearch.Text = string.Empty;
            RefreshProductList();
            txtSearch.Focus();
        }

        private void txtSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
                RefreshProductList();
            }
        }

        private void Filter_Changed(object sender, EventArgs e)
        {
            RefreshProductList();
        }

        private void btnResetFilters_Click(object sender, EventArgs e)
        {
            txtSearch.Text = string.Empty;
            cboFilterCategory.SelectedIndex = 0;
            cboFilterStockStatus.SelectedIndex = 0;
            cboFilterGender.SelectedIndex = 0;
            cboFilterStatus.SelectedIndex = 0; // Active Only
            RefreshProductList();
        }

        private void CardTotalProducts_Click(object sender, EventArgs e)
        {
            cboFilterStockStatus.SelectedIndex = 0; // All Stock
            cboFilterStatus.SelectedIndex = 0;      // Active Only
            RefreshProductList();
        }

        private void CardTotalUnits_Click(object sender, EventArgs e)
        {
            cboFilterStockStatus.SelectedIndex = 0; // All Stock
            cboFilterStatus.SelectedIndex = 0;      // Active Only
            RefreshProductList();
        }

        private void CardLowStock_Click(object sender, EventArgs e)
        {
            cboFilterStockStatus.SelectedIndex = 2; // Low Stock
            cboFilterStatus.SelectedIndex = 0;      // Active Only
            RefreshProductList();
        }

        private void CardOutOfStock_Click(object sender, EventArgs e)
        {
            cboFilterStockStatus.SelectedIndex = 3; // Out of Stock
            cboFilterStatus.SelectedIndex = 0;      // Active Only
            RefreshProductList();
        }

        private void dgvProducts_Paint(object sender, PaintEventArgs e)
        {
            if (dgvProducts.Rows.Count == 0)
            {
                string message = "No products found matching the selected filters.";
                using (var brush = new SolidBrush(Color.FromArgb(127, 140, 141)))
                using (var font = new Font("Segoe UI", 10F, FontStyle.Regular))
                using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                {
                    e.Graphics.DrawString(message, font, brush, dgvProducts.ClientRectangle, sf);
                }
            }
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            LoadCategoryFilter();
            RefreshProductList();
        }

        private void btnNewProduct_Click(object sender, EventArgs e)
        {
            using (var form = new ItemEditForm(_itemService, _categoryService, 0))
            {
                if (form.ShowDialog(this) == DialogResult.OK)
                {
                    RefreshProductList();

                    // Select newly created item
                    if (form.SavedItem != null)
                    {
                        for (int i = 0; i < dgvProducts.Rows.Count; i++)
                        {
                            var rowItem = dgvProducts.Rows[i].DataBoundItem as ItemListItemDto;
                            if (rowItem != null && rowItem.ItemID == form.SavedItem.ItemID)
                            {
                                dgvProducts.Rows[i].Selected = true;
                                break;
                            }
                        }
                    }
                }
            }
        }

        private void btnEditProduct_Click(object sender, EventArgs e)
        {
            EditSelectedProduct();
        }

        private void EditSelectedProduct()
        {
            var selected = GetSelectedProduct();
            if (selected == null)
            {
                return;
            }

            using (var form = new ItemEditForm(_itemService, _categoryService, selected.ItemID))
            {
                if (form.ShowDialog(this) == DialogResult.OK)
                {
                    RefreshProductList();
                }
            }
        }

        private void btnToggleStatus_Click(object sender, EventArgs e)
        {
            var selected = GetSelectedProduct();
            if (selected == null)
            {
                return;
            }

            if (selected.IsActive)
            {
                var confirm = MessageBox.Show(
                    string.Format("Are you sure you want to deactivate product:\n[{0}] {1}?", selected.SKU, selected.Name),
                    "Confirm Deactivation",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question
                );

                if (confirm == DialogResult.Yes)
                {
                    try
                    {
                        _itemService.DeactivateItem(selected.ItemID);
                        RefreshProductList();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Failed to deactivate product: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            else
            {
                try
                {
                    _itemService.ActivateItem(selected.ItemID);
                    RefreshProductList();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed to reactivate product: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnCategories_Click(object sender, EventArgs e)
        {
            using (var form = new CategoryManagementForm(_categoryService))
            {
                form.ShowDialog(this);
            }
            LoadCategoryFilter();
            RefreshProductList();
        }

        private void btnStockIn_Click(object sender, EventArgs e)
        {
            OpenStockOperation("IN");
        }

        private void btnStockOut_Click(object sender, EventArgs e)
        {
            OpenStockOperation("OUT");
        }

        private void btnStockAdjust_Click(object sender, EventArgs e)
        {
            OpenStockOperation("ADJUSTMENT");
        }

        private void OpenStockOperation(string operationType)
        {
            long itemId = 0;
            var selected = GetSelectedProduct();
            if (selected != null)
            {
                itemId = selected.ItemID;
            }

            using (var form = new StockOperationForm(_stockService, _itemService, itemId, operationType))
            {
                if (form.ShowDialog(this) == DialogResult.OK)
                {
                    RefreshProductList();
                }
            }
        }

        private void btnTransactions_Click(object sender, EventArgs e)
        {
            long? itemId = null;
            string sku = null;

            // If triggered from item context menu, filter to that item
            if (sender == mnuViewHistory)
            {
                var selected = GetSelectedProduct();
                if (selected != null)
                {
                    itemId = selected.ItemID;
                    sku = selected.SKU;
                }
            }

            using (var form = new TransactionHistoryForm(_stockService, _exportService, itemId, sku))
            {
                form.ShowDialog(this);
            }
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            try
            {
                if (_currentProducts == null || _currentProducts.Count == 0)
                {
                    MessageBox.Show(
                        this,
                        "There are no products to export matching the current filter criteria.",
                        "Export Products",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information
                    );
                    return;
                }

                using (var sfd = new SaveFileDialog())
                {
                    sfd.Title = "Export Products to CSV";
                    sfd.Filter = "CSV Files (*.csv)|*.csv|All Files (*.*)|*.*";
                    sfd.FilterIndex = 1;
                    sfd.DefaultExt = "csv";
                    sfd.AddExtension = true;
                    sfd.OverwritePrompt = true;
                    sfd.RestoreDirectory = true;
                    sfd.FileName = string.Format("products_export_{0:yyyyMMdd_HHmmss}.csv", DateTime.Now);

                    if (sfd.ShowDialog(this) == DialogResult.OK)
                    {
                        _exportService.ExportProductsToCsv(_currentProducts, sfd.FileName);
                        MessageBox.Show(
                            this,
                            string.Format("Successfully exported {0:N0} products to:\n{1}", _currentProducts.Count, sfd.FileName),
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
                    "An unexpected error occurred during product export:\n" + ex.Message,
                    "Export Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        private void btnBackup_Click(object sender, EventArgs e)
        {
            try
            {
                using (var sfd = new SaveFileDialog())
                {
                    sfd.Title = "Backup Database (Consistent Online Backup)";
                    sfd.Filter = "SQLite Database (*.db)|*.db|All Files (*.*)|*.*";
                    sfd.FilterIndex = 1;
                    sfd.DefaultExt = "db";
                    sfd.AddExtension = true;
                    sfd.OverwritePrompt = true;
                    sfd.RestoreDirectory = true;
                    sfd.FileName = string.Format("inventory_backup_{0:yyyyMMdd_HHmmss}.db", DateTime.Now);

                    if (sfd.ShowDialog(this) == DialogResult.OK)
                    {
                        _backupService.BackupDatabase(sfd.FileName);
                        MessageBox.Show(
                            this,
                            string.Format("Consistent SQLite online backup completed successfully to:\n{0}", sfd.FileName),
                            "Backup Completed",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information
                        );
                    }
                }
            }
            catch (InvalidOperationException invEx)
            {
                MessageBox.Show(
                    this,
                    invEx.Message,
                    "Backup Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
            }
            catch (System.IO.IOException ioEx)
            {
                MessageBox.Show(
                    this,
                    "The backup could not be written because the destination file is locked or the disk is unavailable.\n\nDetails: " + ioEx.Message,
                    "Backup Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
            }
            catch (UnauthorizedAccessException authEx)
            {
                MessageBox.Show(
                    this,
                    "Access denied to the destination path. Please choose a folder with write permissions.\n\nDetails: " + authEx.Message,
                    "Backup Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    this,
                    "Failed to complete database backup:\n" + ex.Message,
                    "Backup Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }
    }
}
