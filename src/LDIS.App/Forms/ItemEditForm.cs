using System;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using LDIS.Core.DTOs;
using LDIS.Core.Models;
using LDIS.Core.Services;

namespace LDIS.App.Forms
{
    public class ItemEditForm : Form
    {
        private readonly IItemService _itemService;
        private readonly ICategoryService _categoryService;
        private readonly long _itemId; // 0 for new item
        private Item _item;

        private IContainer components = null;
        private ErrorProvider errorProvider;

        private Label lblSKU;
        private TextBox txtSKU;
        private Label lblName;
        private TextBox txtName;
        private Label lblCategory;
        private ComboBox cboCategory;
        private Button btnManageCategories;
        private Label lblBrand;
        private TextBox txtBrand;
        private Label lblColor;
        private TextBox txtColor;
        private Label lblSize;
        private TextBox txtSize;
        private Label lblGender;
        private ComboBox cboGender;
        private Label lblPurchasePrice;
        private TextBox txtPurchasePrice;
        private Label lblSellingPrice;
        private TextBox txtSellingPrice;
        private Label lblMinStock;
        private NumericUpDown numMinStock;
        private Label lblCurrentStock;
        private TextBox txtCurrentStock;
        private Label lblStockNote;
        private CheckBox chkIsActive;

        private Button btnSave;
        private Button btnCancel;
        private Panel pnlBottom;

        public Item SavedItem
        {
            get { return _item; }
        }

        public ItemEditForm(IItemService itemService, ICategoryService categoryService, long itemId = 0)
        {
            if (itemService == null) throw new ArgumentNullException("itemService");
            if (categoryService == null) throw new ArgumentNullException("categoryService");

            _itemService = itemService;
            _categoryService = categoryService;
            _itemId = itemId;

            InitializeComponent();
        }

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
            this.errorProvider = new ErrorProvider(this.components);

            this.lblSKU = new Label();
            this.txtSKU = new TextBox();
            this.lblName = new Label();
            this.txtName = new TextBox();
            this.lblCategory = new Label();
            this.cboCategory = new ComboBox();
            this.btnManageCategories = new Button();
            this.lblBrand = new Label();
            this.txtBrand = new TextBox();
            this.lblColor = new Label();
            this.txtColor = new TextBox();
            this.lblSize = new Label();
            this.txtSize = new TextBox();
            this.lblGender = new Label();
            this.cboGender = new ComboBox();
            this.lblPurchasePrice = new Label();
            this.txtPurchasePrice = new TextBox();
            this.lblSellingPrice = new Label();
            this.txtSellingPrice = new TextBox();
            this.lblMinStock = new Label();
            this.numMinStock = new NumericUpDown();
            this.lblCurrentStock = new Label();
            this.txtCurrentStock = new TextBox();
            this.lblStockNote = new Label();
            this.chkIsActive = new CheckBox();

            this.pnlBottom = new Panel();
            this.btnSave = new Button();
            this.btnCancel = new Button();

            ((ISupportInitialize)(this.errorProvider)).BeginInit();
            ((ISupportInitialize)(this.numMinStock)).BeginInit();
            this.pnlBottom.SuspendLayout();
            this.SuspendLayout();

            int leftCol1 = 25;
            int inputCol1 = 150;

            // SKU
            this.lblSKU.Location = new Point(leftCol1, 25);
            this.lblSKU.Size = new Size(120, 23);
            this.lblSKU.Text = "SKU: *";
            this.lblSKU.TextAlign = ContentAlignment.MiddleLeft;

            this.txtSKU.Location = new Point(inputCol1, 25);
            this.txtSKU.Size = new Size(200, 23);
            this.txtSKU.MaxLength = 50;

            // Product Name
            this.lblName.Location = new Point(leftCol1, 60);
            this.lblName.Size = new Size(120, 23);
            this.lblName.Text = "Product Name: *";
            this.lblName.TextAlign = ContentAlignment.MiddleLeft;

            this.txtName.Location = new Point(inputCol1, 60);
            this.txtName.Size = new Size(330, 23);
            this.txtName.MaxLength = 150;

            // Category
            this.lblCategory.Location = new Point(leftCol1, 95);
            this.lblCategory.Size = new Size(120, 23);
            this.lblCategory.Text = "Category:";
            this.lblCategory.TextAlign = ContentAlignment.MiddleLeft;

            this.cboCategory.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cboCategory.Location = new Point(inputCol1, 95);
            this.cboCategory.Size = new Size(230, 23);

            this.btnManageCategories.Location = new Point(388, 94);
            this.btnManageCategories.Size = new Size(92, 25);
            this.btnManageCategories.Text = "Manage...";
            this.btnManageCategories.UseVisualStyleBackColor = true;
            this.btnManageCategories.Click += new EventHandler(this.btnManageCategories_Click);

            // Brand
            this.lblBrand.Location = new Point(leftCol1, 130);
            this.lblBrand.Size = new Size(120, 23);
            this.lblBrand.Text = "Brand:";
            this.lblBrand.TextAlign = ContentAlignment.MiddleLeft;

            this.txtBrand.Location = new Point(inputCol1, 130);
            this.txtBrand.Size = new Size(230, 23);
            this.txtBrand.MaxLength = 50;

            // Color
            this.lblColor.Location = new Point(leftCol1, 165);
            this.lblColor.Size = new Size(120, 23);
            this.lblColor.Text = "Color:";
            this.lblColor.TextAlign = ContentAlignment.MiddleLeft;

            this.txtColor.Location = new Point(inputCol1, 165);
            this.txtColor.Size = new Size(230, 23);
            this.txtColor.MaxLength = 50;

            // Size
            this.lblSize.Location = new Point(leftCol1, 200);
            this.lblSize.Size = new Size(120, 23);
            this.lblSize.Text = "Size:";
            this.lblSize.TextAlign = ContentAlignment.MiddleLeft;

            this.txtSize.Location = new Point(inputCol1, 200);
            this.txtSize.Size = new Size(150, 23);
            this.txtSize.MaxLength = 50;

            // Gender (Controlled DropDownList)
            this.lblGender.Location = new Point(leftCol1, 235);
            this.lblGender.Size = new Size(120, 23);
            this.lblGender.Text = "Gender:";
            this.lblGender.TextAlign = ContentAlignment.MiddleLeft;

            this.cboGender.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cboGender.Location = new Point(inputCol1, 235);
            this.cboGender.Size = new Size(180, 23);
            foreach (var g in GenderOptions.All)
            {
                this.cboGender.Items.Add(g);
            }
            this.cboGender.SelectedItem = GenderOptions.None;

            // Purchase Price
            this.lblPurchasePrice.Location = new Point(leftCol1, 270);
            this.lblPurchasePrice.Size = new Size(120, 23);
            this.lblPurchasePrice.Text = "Purchase Price (Rp):";
            this.lblPurchasePrice.TextAlign = ContentAlignment.MiddleLeft;

            this.txtPurchasePrice.Location = new Point(inputCol1, 270);
            this.txtPurchasePrice.Size = new Size(180, 23);
            this.txtPurchasePrice.TextAlign = HorizontalAlignment.Right;
            this.txtPurchasePrice.Leave += new EventHandler(this.txtPrice_Leave);

            // Selling Price
            this.lblSellingPrice.Location = new Point(leftCol1, 305);
            this.lblSellingPrice.Size = new Size(120, 23);
            this.lblSellingPrice.Text = "Selling Price (Rp):";
            this.lblSellingPrice.TextAlign = ContentAlignment.MiddleLeft;

            this.txtSellingPrice.Location = new Point(inputCol1, 305);
            this.txtSellingPrice.Size = new Size(180, 23);
            this.txtSellingPrice.TextAlign = HorizontalAlignment.Right;
            this.txtSellingPrice.Leave += new EventHandler(this.txtPrice_Leave);

            // Min Stock Level
            this.lblMinStock.Location = new Point(leftCol1, 340);
            this.lblMinStock.Size = new Size(120, 23);
            this.lblMinStock.Text = "Min Stock Level:";
            this.lblMinStock.TextAlign = ContentAlignment.MiddleLeft;

            this.numMinStock.Location = new Point(inputCol1, 340);
            this.numMinStock.Size = new Size(100, 23);
            this.numMinStock.Maximum = 1000000;
            this.numMinStock.TextAlign = HorizontalAlignment.Right;

            // Current Stock (Read-only)
            this.lblCurrentStock.Location = new Point(leftCol1, 375);
            this.lblCurrentStock.Size = new Size(120, 23);
            this.lblCurrentStock.Text = "Current Stock:";
            this.lblCurrentStock.TextAlign = ContentAlignment.MiddleLeft;

            this.txtCurrentStock.Location = new Point(inputCol1, 375);
            this.txtCurrentStock.Size = new Size(100, 23);
            this.txtCurrentStock.ReadOnly = true;
            this.txtCurrentStock.TabStop = false;
            this.txtCurrentStock.BackColor = SystemColors.Control;
            this.txtCurrentStock.TextAlign = HorizontalAlignment.Right;

            this.lblStockNote.Location = new Point(260, 375);
            this.lblStockNote.Size = new Size(240, 23);
            this.lblStockNote.Text = "(Modified via Stock Operations)";
            this.lblStockNote.ForeColor = SystemColors.GrayText;
            this.lblStockNote.TextAlign = ContentAlignment.MiddleLeft;

            // IsActive
            this.chkIsActive.Location = new Point(inputCol1, 410);
            this.chkIsActive.Size = new Size(150, 24);
            this.chkIsActive.Text = "Active Product";
            this.chkIsActive.Checked = true;

            // Bottom Panel
            this.pnlBottom.Dock = DockStyle.Bottom;
            this.pnlBottom.Height = 55;
            this.pnlBottom.BackColor = SystemColors.Control;
            this.pnlBottom.Controls.Add(this.btnSave);
            this.pnlBottom.Controls.Add(this.btnCancel);

            // btnSave
            this.btnSave.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            this.btnSave.Location = new Point(310, 12);
            this.btnSave.Size = new Size(100, 32);
            this.btnSave.Text = "&Save [Ctrl+S]";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new EventHandler(this.btnSave_Click);

            // btnCancel
            this.btnCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            this.btnCancel.DialogResult = DialogResult.Cancel;
            this.btnCancel.Location = new Point(418, 12);
            this.btnCancel.Size = new Size(85, 32);
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new EventHandler(this.btnCancel_Click);

            // Form
            this.AcceptButton = this.btnSave;
            this.CancelButton = this.btnCancel;
            this.KeyPreview = true;
            this.AutoScaleDimensions = new SizeF(7F, 15F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(525, 510);
            this.Controls.Add(this.pnlBottom);
            this.Controls.Add(this.chkIsActive);
            this.Controls.Add(this.lblStockNote);
            this.Controls.Add(this.txtCurrentStock);
            this.Controls.Add(this.lblCurrentStock);
            this.Controls.Add(this.numMinStock);
            this.Controls.Add(this.lblMinStock);
            this.Controls.Add(this.txtSellingPrice);
            this.Controls.Add(this.lblSellingPrice);
            this.Controls.Add(this.txtPurchasePrice);
            this.Controls.Add(this.lblPurchasePrice);
            this.Controls.Add(this.cboGender);
            this.Controls.Add(this.lblGender);
            this.Controls.Add(this.txtSize);
            this.Controls.Add(this.lblSize);
            this.Controls.Add(this.txtColor);
            this.Controls.Add(this.lblColor);
            this.Controls.Add(this.txtBrand);
            this.Controls.Add(this.lblBrand);
            this.Controls.Add(this.btnManageCategories);
            this.Controls.Add(this.cboCategory);
            this.Controls.Add(this.lblCategory);
            this.Controls.Add(this.txtName);
            this.Controls.Add(this.lblName);
            this.Controls.Add(this.txtSKU);
            this.Controls.Add(this.lblSKU);
            this.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.ShowInTaskbar = false;
            this.StartPosition = FormStartPosition.CenterParent;
            this.Text = _itemId <= 0 ? "New Product" : "Edit Product";

            this.KeyDown += new KeyEventHandler(this.ItemEditForm_KeyDown);
            this.Load += new EventHandler(this.ItemEditForm_Load);

            ((ISupportInitialize)(this.errorProvider)).EndInit();
            ((ISupportInitialize)(this.numMinStock)).EndInit();
            this.pnlBottom.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private void ItemEditForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.S)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
                btnSave.PerformClick();
            }
        }

        private void ItemEditForm_Load(object sender, EventArgs e)
        {
            LoadCategories();
            SetupAutocomplete();

            if (_itemId > 0)
            {
                LoadExistingItem();
            }
            else
            {
                txtPurchasePrice.Text = "0";
                txtSellingPrice.Text = "0";
                txtCurrentStock.Text = "0";
                numMinStock.Value = 0;
                chkIsActive.Checked = true;
            }
        }

        private void SetupAutocomplete()
        {
            try
            {
                var attrs = _itemService.GetDistinctAttributes();

                if (attrs.Brands.Count > 0)
                {
                    var brandSource = new AutoCompleteStringCollection();
                    brandSource.AddRange(attrs.Brands.ToArray());
                    txtBrand.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
                    txtBrand.AutoCompleteSource = AutoCompleteSource.CustomSource;
                    txtBrand.AutoCompleteCustomSource = brandSource;
                }

                if (attrs.Colors.Count > 0)
                {
                    var colorSource = new AutoCompleteStringCollection();
                    colorSource.AddRange(attrs.Colors.ToArray());
                    txtColor.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
                    txtColor.AutoCompleteSource = AutoCompleteSource.CustomSource;
                    txtColor.AutoCompleteCustomSource = colorSource;
                }

                if (attrs.Sizes.Count > 0)
                {
                    var sizeSource = new AutoCompleteStringCollection();
                    sizeSource.AddRange(attrs.Sizes.ToArray());
                    txtSize.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
                    txtSize.AutoCompleteSource = AutoCompleteSource.CustomSource;
                    txtSize.AutoCompleteCustomSource = sizeSource;
                }
            }
            catch
            {
                // Autocomplete is convenience only; do not fail form if query fails
            }
        }

        private class CategoryComboItem
        {
            public long? CategoryID { get; set; }
            public string Name { get; set; }

            public override string ToString()
            {
                return Name;
            }
        }

        private void LoadCategories(long? selectCategoryId = null)
        {
            cboCategory.BeginUpdate();
            cboCategory.Items.Clear();

            cboCategory.Items.Add(new CategoryComboItem { CategoryID = null, Name = "(None / Uncategorized)" });

            var categories = _categoryService.GetAllCategories();
            CategoryComboItem toSelect = null;

            foreach (var cat in categories)
            {
                var item = new CategoryComboItem { CategoryID = cat.CategoryID, Name = cat.CategoryName };
                cboCategory.Items.Add(item);

                if (selectCategoryId.HasValue && cat.CategoryID == selectCategoryId.Value)
                {
                    toSelect = item;
                }
            }

            cboCategory.EndUpdate();

            if (toSelect != null)
            {
                cboCategory.SelectedItem = toSelect;
            }
            else
            {
                cboCategory.SelectedIndex = 0;
            }
        }

        private void LoadExistingItem()
        {
            try
            {
                _item = _itemService.GetItem(_itemId);
                if (_item == null)
                {
                    MessageBox.Show("Product not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    this.Close();
                    return;
                }

                this.Text = string.Format("Edit Product - [{0}] {1}", _item.SKU, _item.Name);
                txtSKU.Text = _item.SKU;
                txtName.Text = _item.Name;
                txtBrand.Text = _item.Brand ?? string.Empty;
                txtColor.Text = _item.Color ?? string.Empty;
                txtSize.Text = _item.Size ?? string.Empty;
                cboGender.SelectedItem = GenderOptions.Normalize(_item.Gender);
                txtPurchasePrice.Text = _item.PurchasePrice.ToString("N0", CultureInfo.GetCultureInfo("id-ID"));
                txtSellingPrice.Text = _item.SellingPrice.ToString("N0", CultureInfo.GetCultureInfo("id-ID"));
                numMinStock.Value = Math.Max(0, _item.MinStockLevel);
                txtCurrentStock.Text = _item.CurrentStock.ToString();
                chkIsActive.Checked = _item.IsActive;

                LoadCategories(_item.CategoryID);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to load product: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Close();
            }
        }

        private void btnManageCategories_Click(object sender, EventArgs e)
        {
            long? currentCatId = null;
            var selected = cboCategory.SelectedItem as CategoryComboItem;
            if (selected != null)
            {
                currentCatId = selected.CategoryID;
            }

            using (var catForm = new CategoryManagementForm(_categoryService))
            {
                catForm.ShowDialog(this);
            }

            LoadCategories(currentCatId);
        }

        private void txtPrice_Leave(object sender, EventArgs e)
        {
            var txt = sender as TextBox;
            if (txt != null)
            {
                long val = ParsePrice(txt.Text);
                txt.Text = val.ToString("N0", CultureInfo.GetCultureInfo("id-ID"));
            }
        }

        private static long ParsePrice(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return 0;
            }

            // Remove non-digit characters
            string clean = string.Empty;
            foreach (char c in input)
            {
                if (char.IsDigit(c))
                {
                    clean += c;
                }
            }

            long val;
            if (long.TryParse(clean, out val))
            {
                return val;
            }
            return 0;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            errorProvider.Clear();

            var selectedCategory = cboCategory.SelectedItem as CategoryComboItem;
            long? catId = selectedCategory != null ? selectedCategory.CategoryID : null;

            var itemToSave = _item != null ? _item : new Item();
            itemToSave.ItemID = _itemId;
            itemToSave.SKU = txtSKU.Text;
            itemToSave.Name = txtName.Text;
            itemToSave.CategoryID = catId;
            itemToSave.Brand = txtBrand.Text;
            itemToSave.Color = txtColor.Text;
            itemToSave.Size = txtSize.Text;
            itemToSave.Gender = cboGender.SelectedItem != null ? cboGender.SelectedItem.ToString() : GenderOptions.None;
            itemToSave.PurchasePrice = ParsePrice(txtPurchasePrice.Text);
            itemToSave.SellingPrice = ParsePrice(txtSellingPrice.Text);
            itemToSave.MinStockLevel = Convert.ToInt32(numMinStock.Value);
            itemToSave.IsActive = chkIsActive.Checked;

            bool isNew = (_itemId <= 0);
            var validation = _itemService.ValidateItem(itemToSave, isNew);

            if (!validation.IsValid)
            {
                // Highlight fields with errors
                foreach (var err in validation.Errors)
                {
                    if (err.IndexOf("SKU", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        errorProvider.SetError(txtSKU, err);
                    }
                    else if (err.IndexOf("name", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        errorProvider.SetError(txtName, err);
                    }
                    else if (err.IndexOf("Purchase price", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        errorProvider.SetError(txtPurchasePrice, err);
                    }
                    else if (err.IndexOf("Selling price", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        errorProvider.SetError(txtSellingPrice, err);
                    }
                }

                MessageBox.Show(
                    "Please correct the following validation errors:\n\n" + validation.ErrorMessage,
                    "Validation Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
                return;
            }

            try
            {
                _itemService.SaveItem(itemToSave);
                _item = itemToSave;

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to save product:\n" + ex.Message, "Error Saving Product", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
