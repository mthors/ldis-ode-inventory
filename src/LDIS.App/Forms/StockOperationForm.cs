using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using LDIS.Core.DTOs;
using LDIS.Core.Models;
using LDIS.Core.Services;

namespace LDIS.App.Forms
{
    public class StockOperationForm : Form
    {
        private readonly IStockService _stockService;
        private readonly IItemService _itemService;
        private readonly long _initialItemId;
        private readonly string _initialOperationType; // "IN", "OUT", "ADJUSTMENT"

        private Item _selectedItem;

        // UI Controls
        private Label lblProduct;
        private ComboBox cboProduct;
        private Panel pnlProductSummary;
        private Label lblSummarySku;
        private Label lblSummaryName;
        private Label lblSummaryCurrentStock;
        private Label lblSummaryStockValue;

        private Label lblOpType;
        private RadioButton rbIn;
        private RadioButton rbOut;
        private RadioButton rbAdjustment;
        private FlowLayoutPanel pnlOpTypes;

        private Label lblQtyOrDelta;
        private NumericUpDown numQtyOrDelta;
        private Label lblResultingStock;
        private Label lblResultingStockValue;

        private Label lblUnitPrice;
        private TextBox txtUnitPrice;

        private Label lblRefNumber;
        private TextBox txtRefNumber;

        private Label lblParty; // Supplier or Customer
        private TextBox txtParty;

        private Label lblReason;
        private ComboBox cboReason;

        private Label lblNotes;
        private TextBox txtNotes;

        private Button btnSave;
        private Button btnCancel;
        private Panel pnlBottom;

        private ErrorProvider errorProvider;

        public long RecordedTransactionId { get; private set; }

        public StockOperationForm(
            IStockService stockService,
            IItemService itemService,
            long itemId = 0,
            string initialOperationType = "IN")
        {
            if (stockService == null) throw new ArgumentNullException("stockService");
            if (itemService == null) throw new ArgumentNullException("itemService");

            _stockService = stockService;
            _itemService = itemService;
            _initialItemId = itemId;
            _initialOperationType = initialOperationType;

            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.errorProvider = new ErrorProvider();

            this.lblProduct = new Label();
            this.cboProduct = new ComboBox();
            this.pnlProductSummary = new Panel();
            this.lblSummarySku = new Label();
            this.lblSummaryName = new Label();
            this.lblSummaryCurrentStock = new Label();
            this.lblSummaryStockValue = new Label();

            this.lblOpType = new Label();
            this.rbIn = new RadioButton();
            this.rbOut = new RadioButton();
            this.rbAdjustment = new RadioButton();
            this.pnlOpTypes = new FlowLayoutPanel();

            this.lblQtyOrDelta = new Label();
            this.numQtyOrDelta = new NumericUpDown();
            this.lblResultingStock = new Label();
            this.lblResultingStockValue = new Label();

            this.lblUnitPrice = new Label();
            this.txtUnitPrice = new TextBox();

            this.lblRefNumber = new Label();
            this.txtRefNumber = new TextBox();

            this.lblParty = new Label();
            this.txtParty = new TextBox();

            this.lblReason = new Label();
            this.cboReason = new ComboBox();

            this.lblNotes = new Label();
            this.txtNotes = new TextBox();

            this.btnSave = new Button();
            this.btnCancel = new Button();
            this.pnlBottom = new Panel();

            this.SuspendLayout();

            // Form properties
            this.Text = "Stock Operation";
            this.ClientSize = new Size(540, 560);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;
            this.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
            this.BackColor = Color.FromArgb(245, 247, 250);
            this.KeyPreview = true;
            this.KeyDown += StockOperationForm_KeyDown;

            // Product Selection
            this.lblProduct.Text = "Product:";
            this.lblProduct.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            this.lblProduct.Location = new Point(20, 16);
            this.lblProduct.Size = new Size(120, 20);

            this.cboProduct.Location = new Point(20, 38);
            this.cboProduct.Size = new Size(500, 25);
            this.cboProduct.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cboProduct.SelectedIndexChanged += cboProduct_SelectedIndexChanged;

            // Product Summary Panel
            this.pnlProductSummary.Location = new Point(20, 70);
            this.pnlProductSummary.Size = new Size(500, 52);
            this.pnlProductSummary.BackColor = Color.FromArgb(235, 240, 245);
            this.pnlProductSummary.BorderStyle = BorderStyle.FixedSingle;

            this.lblSummarySku.Location = new Point(10, 8);
            this.lblSummarySku.Size = new Size(180, 18);
            this.lblSummarySku.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            this.lblSummarySku.Text = "SKU: -";

            this.lblSummaryName.Location = new Point(10, 28);
            this.lblSummaryName.Size = new Size(280, 18);
            this.lblSummaryName.Text = "Name: -";

            this.lblSummaryCurrentStock.Location = new Point(310, 8);
            this.lblSummaryCurrentStock.Size = new Size(100, 18);
            this.lblSummaryCurrentStock.Text = "Current Stock:";

            this.lblSummaryStockValue.Location = new Point(410, 6);
            this.lblSummaryStockValue.Size = new Size(80, 24);
            this.lblSummaryStockValue.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            this.lblSummaryStockValue.ForeColor = Color.FromArgb(41, 128, 185);
            this.lblSummaryStockValue.Text = "0";
            this.lblSummaryStockValue.TextAlign = ContentAlignment.TopRight;

            this.pnlProductSummary.Controls.Add(this.lblSummarySku);
            this.pnlProductSummary.Controls.Add(this.lblSummaryName);
            this.pnlProductSummary.Controls.Add(this.lblSummaryCurrentStock);
            this.pnlProductSummary.Controls.Add(this.lblSummaryStockValue);

            // Operation Type
            this.lblOpType.Text = "Operation Type:";
            this.lblOpType.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            this.lblOpType.Location = new Point(20, 132);
            this.lblOpType.Size = new Size(120, 20);

            this.pnlOpTypes.Location = new Point(20, 154);
            this.pnlOpTypes.Size = new Size(500, 30);
            this.rbIn.Text = "Stock &IN (+)";
            this.rbIn.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            this.rbIn.ForeColor = Color.FromArgb(39, 174, 96);
            this.rbIn.Size = new Size(120, 24);
            this.rbIn.CheckedChanged += OpType_CheckedChanged;

            this.rbOut.Text = "Stock &OUT (-)";
            this.rbOut.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            this.rbOut.ForeColor = Color.FromArgb(211, 84, 0);
            this.rbOut.Size = new Size(130, 24);
            this.rbOut.CheckedChanged += OpType_CheckedChanged;

            this.rbAdjustment.Text = "Stock &ADJUSTMENT (±)";
            this.rbAdjustment.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            this.rbAdjustment.ForeColor = Color.FromArgb(142, 68, 173);
            this.rbAdjustment.Size = new Size(190, 24);
            this.rbAdjustment.CheckedChanged += OpType_CheckedChanged;

            this.pnlOpTypes.Controls.Add(this.rbIn);
            this.pnlOpTypes.Controls.Add(this.rbOut);
            this.pnlOpTypes.Controls.Add(this.rbAdjustment);

            // Quantity / Adjustment Delta
            this.lblQtyOrDelta.Text = "Quantity:";
            this.lblQtyOrDelta.Location = new Point(20, 194);
            this.lblQtyOrDelta.Size = new Size(140, 20);

            this.numQtyOrDelta.Location = new Point(20, 216);
            this.numQtyOrDelta.Size = new Size(140, 25);
            this.numQtyOrDelta.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            this.numQtyOrDelta.TextAlign = HorizontalAlignment.Right;
            this.numQtyOrDelta.ValueChanged += numQtyOrDelta_ValueChanged;

            // Resulting Stock Display
            this.lblResultingStock.Text = "Resulting Stock:";
            this.lblResultingStock.Location = new Point(180, 194);
            this.lblResultingStock.Size = new Size(120, 20);

            this.lblResultingStockValue.Location = new Point(180, 216);
            this.lblResultingStockValue.Size = new Size(120, 25);
            this.lblResultingStockValue.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            this.lblResultingStockValue.ForeColor = Color.FromArgb(45, 62, 80);
            this.lblResultingStockValue.Text = "0";

            // Unit Price
            this.lblUnitPrice.Text = "Unit Price (Rp):";
            this.lblUnitPrice.Location = new Point(320, 194);
            this.lblUnitPrice.Size = new Size(140, 20);

            this.txtUnitPrice.Location = new Point(320, 216);
            this.txtUnitPrice.Size = new Size(200, 25);
            this.txtUnitPrice.TextAlign = HorizontalAlignment.Right;

            // Reference Number
            this.lblRefNumber.Text = "Reference Number (e.g. PO / INV):";
            this.lblRefNumber.Location = new Point(20, 254);
            this.lblRefNumber.Size = new Size(220, 20);

            this.txtRefNumber.Location = new Point(20, 276);
            this.txtRefNumber.Size = new Size(220, 25);

            // Supplier or Customer Party
            this.lblParty.Text = "Supplier / Customer:";
            this.lblParty.Location = new Point(260, 254);
            this.lblParty.Size = new Size(220, 20);

            this.txtParty.Location = new Point(260, 276);
            this.txtParty.Size = new Size(260, 25);

            // Reason (Required for Adjustment)
            this.lblReason.Text = "Reason:";
            this.lblReason.Location = new Point(20, 314);
            this.lblReason.Size = new Size(200, 20);

            this.cboReason.Location = new Point(20, 336);
            this.cboReason.Size = new Size(500, 25);
            this.cboReason.Items.AddRange(new object[] {
                "Physical stock count",
                "Damaged goods",
                "Lost / Stolen",
                "Returned to supplier",
                "Found extra stock",
                "Data entry correction"
            });

            // Notes
            this.lblNotes.Text = "Notes:";
            this.lblNotes.Location = new Point(20, 374);
            this.lblNotes.Size = new Size(100, 20);

            this.txtNotes.Location = new Point(20, 396);
            this.txtNotes.Size = new Size(500, 56);
            this.txtNotes.Multiline = true;
            this.txtNotes.ScrollBars = ScrollBars.Vertical;

            // Bottom Panel & Buttons
            this.pnlBottom.Dock = DockStyle.Bottom;
            this.pnlBottom.Height = 55;
            this.pnlBottom.BackColor = Color.FromArgb(235, 238, 242);

            this.btnSave.Text = "&Save Operation";
            this.btnSave.Size = new Size(130, 34);
            this.btnSave.Location = new Point(260, 10);
            this.btnSave.BackColor = Color.FromArgb(46, 204, 113);
            this.btnSave.ForeColor = Color.White;
            this.btnSave.FlatStyle = FlatStyle.Flat;
            this.btnSave.FlatAppearance.BorderSize = 0;
            this.btnSave.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            this.btnSave.Click += btnSave_Click;

            this.btnCancel.Text = "Cancel";
            this.btnCancel.Size = new Size(100, 34);
            this.btnCancel.Location = new Point(400, 10);
            this.btnCancel.BackColor = Color.FromArgb(149, 165, 166);
            this.btnCancel.ForeColor = Color.White;
            this.btnCancel.FlatStyle = FlatStyle.Flat;
            this.btnCancel.FlatAppearance.BorderSize = 0;
            this.btnCancel.Click += delegate { this.DialogResult = DialogResult.Cancel; this.Close(); };

            this.pnlBottom.Controls.Add(this.btnSave);
            this.pnlBottom.Controls.Add(this.btnCancel);

            // Add all controls to Form
            this.Controls.Add(this.lblProduct);
            this.Controls.Add(this.cboProduct);
            this.Controls.Add(this.pnlProductSummary);
            this.Controls.Add(this.lblOpType);
            this.Controls.Add(this.pnlOpTypes);
            this.Controls.Add(this.lblQtyOrDelta);
            this.Controls.Add(this.numQtyOrDelta);
            this.Controls.Add(this.lblResultingStock);
            this.Controls.Add(this.lblResultingStockValue);
            this.Controls.Add(this.lblUnitPrice);
            this.Controls.Add(this.txtUnitPrice);
            this.Controls.Add(this.lblRefNumber);
            this.Controls.Add(this.txtRefNumber);
            this.Controls.Add(this.lblParty);
            this.Controls.Add(this.txtParty);
            this.Controls.Add(this.lblReason);
            this.Controls.Add(this.cboReason);
            this.Controls.Add(this.lblNotes);
            this.Controls.Add(this.txtNotes);
            this.Controls.Add(this.pnlBottom);

            this.ResumeLayout(false);
            this.PerformLayout();

            this.Load += StockOperationForm_Load;
        }

        private class ProductComboItem
        {
            public long ItemID { get; set; }
            public string DisplayText { get; set; }

            public override string ToString()
            {
                return DisplayText;
            }
        }

        private void StockOperationForm_Load(object sender, EventArgs e)
        {
            LoadProducts();

            // Set initial operation type
            if (_initialOperationType == "OUT")
            {
                rbOut.Checked = true;
            }
            else if (_initialOperationType == "ADJUSTMENT")
            {
                rbAdjustment.Checked = true;
            }
            else
            {
                rbIn.Checked = true;
            }

            ConfigureFormForCurrentOpType();
        }

        private void LoadProducts()
        {
            cboProduct.BeginUpdate();
            cboProduct.Items.Clear();

            var criteria = new ItemSearchCriteria { ActiveStatus = ActiveFilterStatus.ActiveOnly };
            var products = _itemService.SearchItems(criteria);

            ProductComboItem toSelect = null;

            foreach (var p in products)
            {
                var item = new ProductComboItem
                {
                    ItemID = p.ItemID,
                    DisplayText = string.Format("[{0}] {1} (Stock: {2})", p.SKU, p.Name, p.CurrentStock)
                };
                cboProduct.Items.Add(item);

                if (_initialItemId > 0 && p.ItemID == _initialItemId)
                {
                    toSelect = item;
                }
            }

            cboProduct.EndUpdate();

            if (toSelect != null)
            {
                cboProduct.SelectedItem = toSelect;
            }
            else if (cboProduct.Items.Count > 0)
            {
                cboProduct.SelectedIndex = 0;
            }
        }

        private void cboProduct_SelectedIndexChanged(object sender, EventArgs e)
        {
            var selected = cboProduct.SelectedItem as ProductComboItem;
            if (selected != null)
            {
                _selectedItem = _itemService.GetItem(selected.ItemID);
                UpdateProductSummary();
                UpdateDefaultPrice();
                CalculateResultingStock();
            }
        }

        private void UpdateProductSummary()
        {
            if (_selectedItem != null)
            {
                lblSummarySku.Text = "SKU: " + _selectedItem.SKU;
                lblSummaryName.Text = "Name: " + _selectedItem.Name;
                lblSummaryStockValue.Text = _selectedItem.CurrentStock.ToString("N0");

                if (_selectedItem.CurrentStock == 0)
                {
                    lblSummaryStockValue.ForeColor = Color.FromArgb(231, 76, 60);
                }
                else if (_selectedItem.CurrentStock <= _selectedItem.MinStockLevel)
                {
                    lblSummaryStockValue.ForeColor = Color.FromArgb(243, 156, 18);
                }
                else
                {
                    lblSummaryStockValue.ForeColor = Color.FromArgb(41, 128, 185);
                }
            }
            else
            {
                lblSummarySku.Text = "SKU: -";
                lblSummaryName.Text = "Name: -";
                lblSummaryStockValue.Text = "0";
            }
        }

        private void UpdateDefaultPrice()
        {
            if (_selectedItem == null) return;

            if (rbIn.Checked)
            {
                txtUnitPrice.Text = _selectedItem.PurchasePrice.ToString();
            }
            else if (rbOut.Checked)
            {
                txtUnitPrice.Text = _selectedItem.SellingPrice.ToString();
            }
            else
            {
                txtUnitPrice.Text = "0";
            }
        }

        private void OpType_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton rb = sender as RadioButton;
            if (rb != null && rb.Checked)
            {
                ConfigureFormForCurrentOpType();
                UpdateDefaultPrice();
                CalculateResultingStock();
            }
        }

        private void ConfigureFormForCurrentOpType()
        {
            if (rbIn.Checked)
            {
                this.Text = "Record Stock IN";
                lblQtyOrDelta.Text = "Quantity to Receive (+):";
                numQtyOrDelta.Minimum = 1;
                numQtyOrDelta.Maximum = 1000000;
                if (numQtyOrDelta.Value <= 0) numQtyOrDelta.Value = 1;

                lblUnitPrice.Text = "Unit Cost (Rp):";
                lblParty.Text = "Supplier:";
                lblReason.Text = "Reason (optional):";
                btnSave.BackColor = Color.FromArgb(46, 204, 113);
                btnSave.Text = "Save Stock &IN";
            }
            else if (rbOut.Checked)
            {
                this.Text = "Record Stock OUT";
                lblQtyOrDelta.Text = "Quantity to Issue (-):";
                numQtyOrDelta.Minimum = 1;
                numQtyOrDelta.Maximum = 1000000;
                if (numQtyOrDelta.Value <= 0) numQtyOrDelta.Value = 1;

                lblUnitPrice.Text = "Unit Selling Price (Rp):";
                lblParty.Text = "Customer / Destination:";
                lblReason.Text = "Reason (optional):";
                btnSave.BackColor = Color.FromArgb(211, 84, 0);
                btnSave.Text = "Save Stock &OUT";
            }
            else // rbAdjustment.Checked
            {
                this.Text = "Record Stock ADJUSTMENT";
                lblQtyOrDelta.Text = "Adjustment Delta (±):";
                // Allow positive or negative integer delta
                numQtyOrDelta.Minimum = -1000000;
                numQtyOrDelta.Maximum = 1000000;
                if (numQtyOrDelta.Value == 0) numQtyOrDelta.Value = -1;

                lblUnitPrice.Text = "Unit Price (Rp, optional):";
                lblParty.Text = "Related Party (optional):";
                lblReason.Text = "Reason (REQUIRED):";
                lblReason.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
                lblReason.ForeColor = Color.FromArgb(192, 57, 43);
                btnSave.BackColor = Color.FromArgb(142, 68, 173);
                btnSave.Text = "Save &ADJUSTMENT";
            }
        }

        private void numQtyOrDelta_ValueChanged(object sender, EventArgs e)
        {
            CalculateResultingStock();
        }

        private void CalculateResultingStock()
        {
            if (_selectedItem == null)
            {
                lblResultingStockValue.Text = "-";
                return;
            }

            int current = _selectedItem.CurrentStock;
            int delta = (int)numQtyOrDelta.Value;
            int resulting;

            if (rbIn.Checked)
            {
                resulting = current + Math.Abs(delta);
            }
            else if (rbOut.Checked)
            {
                resulting = current - Math.Abs(delta);
            }
            else // ADJUSTMENT
            {
                resulting = current + delta;
            }

            lblResultingStockValue.Text = resulting.ToString("N0");

            if (resulting < 0)
            {
                lblResultingStockValue.ForeColor = Color.FromArgb(231, 76, 60);
                errorProvider.SetError(numQtyOrDelta, "Operation results in negative stock!");
            }
            else
            {
                lblResultingStockValue.ForeColor = Color.FromArgb(45, 62, 80);
                errorProvider.SetError(numQtyOrDelta, string.Empty);
            }
        }

        private void StockOperationForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                this.DialogResult = DialogResult.Cancel;
                this.Close();
            }
            else if (e.Control && e.KeyCode == Keys.S)
            {
                btnSave.PerformClick();
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            errorProvider.Clear();

            if (_selectedItem == null)
            {
                errorProvider.SetError(cboProduct, "Please select a product.");
                return;
            }

            string opType = rbIn.Checked ? "IN" : (rbOut.Checked ? "OUT" : "ADJUSTMENT");
            int rawQty = (int)numQtyOrDelta.Value;

            if (opType == "IN" || opType == "OUT")
            {
                if (rawQty <= 0)
                {
                    errorProvider.SetError(numQtyOrDelta, "Quantity must be greater than zero.");
                    return;
                }
            }
            else if (opType == "ADJUSTMENT")
            {
                if (rawQty == 0)
                {
                    errorProvider.SetError(numQtyOrDelta, "Adjustment delta cannot be zero.");
                    return;
                }

                if (string.IsNullOrWhiteSpace(cboReason.Text))
                {
                    errorProvider.SetError(cboReason, "Reason is strictly required for stock adjustments.");
                    cboReason.Focus();
                    return;
                }
            }

            long unitPrice = 0;
            string cleanPrice = txtUnitPrice.Text.Replace(".", "").Replace(",", "").Trim();
            if (!string.IsNullOrEmpty(cleanPrice))
            {
                if (!long.TryParse(cleanPrice, out unitPrice) || unitPrice < 0)
                {
                    errorProvider.SetError(txtUnitPrice, "Invalid unit price.");
                    return;
                }
            }

            var request = new StockOperationRequest
            {
                ItemID = _selectedItem.ItemID,
                OperationType = opType,
                Quantity = rawQty,
                UnitPrice = unitPrice,
                ReferenceNumber = txtRefNumber.Text.Trim(),
                SupplierOrCustomer = txtParty.Text.Trim(),
                Reason = cboReason.Text.Trim(),
                Notes = txtNotes.Text.Trim(),
                CreatedBy = Environment.UserName
            };

            var validation = _stockService.ValidateOperation(request);
            if (!validation.IsValid)
            {
                MessageBox.Show(validation.ErrorMessage, "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                RecordedTransactionId = _stockService.ExecuteOperation(request);

                MessageBox.Show(
                    string.Format("Stock operation '{0}' recorded successfully!\nProduct: [{1}] {2}\nNew Stock: {3}",
                        opType, _selectedItem.SKU, _selectedItem.Name, lblResultingStockValue.Text),
                    "Success",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Stock operation failed:\n" + ex.Message, "Operation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
