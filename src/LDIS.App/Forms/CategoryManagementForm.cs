using System;
using System.ComponentModel;
using System.Windows.Forms;
using LDIS.Core.Models;
using LDIS.Core.Services;

namespace LDIS.App.Forms
{
    public class CategoryManagementForm : Form
    {
        private readonly ICategoryService _categoryService;

        private IContainer components = null;
        private ListBox lstCategories;
        private TextBox txtCategoryName;
        private Button btnAdd;
        private Button btnRename;
        private Button btnDelete;
        private Button btnClose;
        private Label lblName;
        private Label lblList;

        public CategoryManagementForm(ICategoryService categoryService)
        {
            if (categoryService == null)
            {
                throw new ArgumentNullException("categoryService");
            }
            _categoryService = categoryService;

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
            this.lstCategories = new ListBox();
            this.txtCategoryName = new TextBox();
            this.btnAdd = new Button();
            this.btnRename = new Button();
            this.btnDelete = new Button();
            this.btnClose = new Button();
            this.lblName = new Label();
            this.lblList = new Label();
            this.SuspendLayout();

            // lblList
            this.lblList.AutoSize = true;
            this.lblList.Location = new System.Drawing.Point(16, 16);
            this.lblList.Name = "lblList";
            this.lblList.Size = new System.Drawing.Size(117, 15);
            this.lblList.Text = "Existing Categories:";

            // lstCategories
            this.lstCategories.FormattingEnabled = true;
            this.lstCategories.ItemHeight = 15;
            this.lstCategories.Location = new System.Drawing.Point(19, 36);
            this.lstCategories.Name = "lstCategories";
            this.lstCategories.Size = new System.Drawing.Size(260, 244);
            this.lstCategories.TabIndex = 0;
            this.lstCategories.SelectedIndexChanged += new EventHandler(this.lstCategories_SelectedIndexChanged);

            // lblName
            this.lblName.AutoSize = true;
            this.lblName.Location = new System.Drawing.Point(295, 36);
            this.lblName.Name = "lblName";
            this.lblName.Size = new System.Drawing.Size(96, 15);
            this.lblName.Text = "Category Name:";

            // txtCategoryName
            this.txtCategoryName.Location = new System.Drawing.Point(298, 56);
            this.txtCategoryName.MaxLength = 100;
            this.txtCategoryName.Name = "txtCategoryName";
            this.txtCategoryName.Size = new System.Drawing.Size(200, 23);
            this.txtCategoryName.TabIndex = 1;
            this.txtCategoryName.KeyDown += new KeyEventHandler(this.txtCategoryName_KeyDown);

            // btnAdd
            this.btnAdd.Location = new System.Drawing.Point(298, 90);
            this.btnAdd.Name = "btnAdd";
            this.btnAdd.Size = new System.Drawing.Size(95, 28);
            this.btnAdd.TabIndex = 2;
            this.btnAdd.Text = "&Add New";
            this.btnAdd.UseVisualStyleBackColor = true;
            this.btnAdd.Click += new EventHandler(this.btnAdd_Click);

            // btnRename
            this.btnRename.Location = new System.Drawing.Point(403, 90);
            this.btnRename.Name = "btnRename";
            this.btnRename.Size = new System.Drawing.Size(95, 28);
            this.btnRename.TabIndex = 3;
            this.btnRename.Text = "&Rename";
            this.btnRename.UseVisualStyleBackColor = true;
            this.btnRename.Click += new EventHandler(this.btnRename_Click);

            // btnDelete
            this.btnDelete.Location = new System.Drawing.Point(298, 126);
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Size = new System.Drawing.Size(95, 28);
            this.btnDelete.TabIndex = 4;
            this.btnDelete.Text = "&Delete";
            this.btnDelete.UseVisualStyleBackColor = true;
            this.btnDelete.Click += new EventHandler(this.btnDelete_Click);

            // btnClose
            this.btnClose.DialogResult = DialogResult.Cancel;
            this.btnClose.Location = new System.Drawing.Point(403, 252);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(95, 28);
            this.btnClose.TabIndex = 5;
            this.btnClose.Text = "&Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new EventHandler(this.btnClose_Click);

            // CategoryManagementForm
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.CancelButton = this.btnClose;
            this.ClientSize = new System.Drawing.Size(519, 298);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.btnDelete);
            this.Controls.Add(this.btnRename);
            this.Controls.Add(this.btnAdd);
            this.Controls.Add(this.txtCategoryName);
            this.Controls.Add(this.lblName);
            this.Controls.Add(this.lstCategories);
            this.Controls.Add(this.lblList);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "CategoryManagementForm";
            this.ShowInTaskbar = false;
            this.StartPosition = FormStartPosition.CenterParent;
            this.Text = "Category Management";
            this.Load += new EventHandler(this.CategoryManagementForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private void CategoryManagementForm_Load(object sender, EventArgs e)
        {
            RefreshCategoryList();
        }

        private void RefreshCategoryList()
        {
            try
            {
                long? previousSelectedId = null;
                var currentSelected = lstCategories.SelectedItem as Category;
                if (currentSelected != null)
                {
                    previousSelectedId = currentSelected.CategoryID;
                }

                lstCategories.BeginUpdate();
                lstCategories.Items.Clear();

                var categories = _categoryService.GetAllCategories();
                Category toReselect = null;
                foreach (var cat in categories)
                {
                    lstCategories.Items.Add(cat);
                    if (previousSelectedId.HasValue && cat.CategoryID == previousSelectedId.Value)
                    {
                        toReselect = cat;
                    }
                }

                lstCategories.EndUpdate();

                if (toReselect != null)
                {
                    lstCategories.SelectedItem = toReselect;
                }
                else if (lstCategories.Items.Count > 0)
                {
                    lstCategories.SelectedIndex = 0;
                }
                else
                {
                    txtCategoryName.Text = string.Empty;
                    UpdateButtonStates();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Failed to load categories: " + ex.Message,
                    "Database Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        private void lstCategories_SelectedIndexChanged(object sender, EventArgs e)
        {
            var selected = lstCategories.SelectedItem as Category;
            if (selected != null)
            {
                txtCategoryName.Text = selected.CategoryName;
            }
            UpdateButtonStates();
        }

        private void UpdateButtonStates()
        {
            bool hasSelection = lstCategories.SelectedItem != null;
            btnRename.Enabled = hasSelection;
            btnDelete.Enabled = hasSelection;
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            string name = txtCategoryName.Text.Trim();
            if (string.IsNullOrWhiteSpace(name))
            {
                MessageBox.Show("Please enter a category name.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtCategoryName.Focus();
                return;
            }

            try
            {
                var created = _categoryService.CreateCategory(name);
                RefreshCategoryList();
                // Select newly created item
                for (int i = 0; i < lstCategories.Items.Count; i++)
                {
                    var cat = lstCategories.Items[i] as Category;
                    if (cat != null && cat.CategoryID == created.CategoryID)
                    {
                        lstCategories.SelectedIndex = i;
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Cannot Add Category", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtCategoryName.Focus();
            }
        }

        private void btnRename_Click(object sender, EventArgs e)
        {
            var selected = lstCategories.SelectedItem as Category;
            if (selected == null)
            {
                return;
            }

            string newName = txtCategoryName.Text.Trim();
            if (string.IsNullOrWhiteSpace(newName))
            {
                MessageBox.Show("Please enter a category name.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtCategoryName.Focus();
                return;
            }

            if (string.Equals(selected.CategoryName, newName, StringComparison.OrdinalIgnoreCase))
            {
                return; // Nothing changed
            }

            try
            {
                _categoryService.UpdateCategory(selected.CategoryID, newName);
                RefreshCategoryList();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Cannot Rename Category", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtCategoryName.Focus();
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            var selected = lstCategories.SelectedItem as Category;
            if (selected == null)
            {
                return;
            }

            string reason;
            if (!_categoryService.CanDeleteCategory(selected.CategoryID, out reason))
            {
                MessageBox.Show(reason, "Cannot Delete Category", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var confirm = MessageBox.Show(
                string.Format("Are you sure you want to delete category '{0}'?", selected.CategoryName),
                "Confirm Delete",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (confirm == DialogResult.Yes)
            {
                try
                {
                    _categoryService.DeleteCategory(selected.CategoryID);
                    RefreshCategoryList();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Delete Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void txtCategoryName_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
                if (lstCategories.SelectedItem == null)
                {
                    btnAdd.PerformClick();
                }
                else
                {
                    btnRename.PerformClick();
                }
            }
        }
    }
}
