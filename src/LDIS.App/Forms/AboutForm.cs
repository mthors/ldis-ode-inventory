using System;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

namespace LDIS.App.Forms
{
    public class AboutForm : Form
    {
        public AboutForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            this.Text = "About LDIS";
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterParent;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.ShowInTaskbar = false;
            this.ClientSize = new Size(420, 420);
            this.BackColor = Color.White;
            this.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
            this.AutoScaleMode = AutoScaleMode.Font;
            this.AutoScaleDimensions = new SizeF(7F, 15F);

            // Try to assign the application icon
            try
            {
                this.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            }
            catch
            {
                // Fallback gracefully if icon extraction is unavailable
            }

            // Top Header Panel
            var pnlTop = new Panel
            {
                Dock = DockStyle.Top,
                Height = 82,
                BackColor = Color.FromArgb(45, 62, 80),
                Padding = new Padding(16, 12, 16, 12)
            };

            var picIcon = new PictureBox
            {
                Size = new Size(48, 48),
                Location = new Point(16, 16),
                SizeMode = PictureBoxSizeMode.CenterImage,
                BackColor = Color.Transparent
            };

            if (this.Icon != null)
            {
                try
                {
                    picIcon.Image = new Icon(this.Icon, new Size(48, 48)).ToBitmap();
                }
                catch
                {
                    picIcon.Image = this.Icon.ToBitmap();
                }
            }

            var lblHeaderTitle = new Label
            {
                Text = "LDIS",
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(74, 14),
                AutoSize = true
            };

            var lblHeaderSubtitle = new Label
            {
                Text = "Lightweight Desktop Inventory System",
                Font = new Font("Segoe UI", 9.5F, FontStyle.Regular),
                ForeColor = Color.LightGray,
                Location = new Point(76, 44),
                AutoSize = true
            };

            pnlTop.Controls.Add(picIcon);
            pnlTop.Controls.Add(lblHeaderTitle);
            pnlTop.Controls.Add(lblHeaderSubtitle);

            // Main Content Area
            var pnlBody = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(24, 16, 24, 16),
                BackColor = Color.White
            };

            // Version dynamically derived from executing assembly (Major.Minor.Build)
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            string versionStr = string.Format("Version {0}.{1}.{2}", version.Major, version.Minor, version.Build);

            var lblVersion = new Label
            {
                Text = versionStr,
                Font = new Font("Segoe UI", 10.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(41, 128, 185),
                Location = new Point(24, 16),
                AutoSize = true
            };

            var lblDescription = new Label
            {
                Text = "A lightweight offline inventory system\ndesigned for small warehouse operations.",
                Font = new Font("Segoe UI", 9.5F, FontStyle.Regular),
                ForeColor = Color.FromArgb(50, 50, 50),
                Location = new Point(24, 48),
                Size = new Size(370, 38)
            };

            var lblCreator = new Label
            {
                Text = "Created by Thor\n© 2026 Thor",
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                ForeColor = Color.FromArgb(70, 70, 70),
                Location = new Point(24, 98),
                Size = new Size(370, 36)
            };

            var pnlDivider = new Panel
            {
                Location = new Point(24, 148),
                Size = new Size(372, 1),
                BackColor = Color.FromArgb(220, 224, 230)
            };

            var lblTechTitle = new Label
            {
                Text = "Built with:",
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(100, 110, 120),
                Location = new Point(24, 160),
                AutoSize = true
            };

            var lblTechDetails = new Label
            {
                Text = "C# • .NET Framework 4.8 • WinForms • SQLite",
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                ForeColor = Color.FromArgb(52, 73, 94),
                Location = new Point(24, 182),
                AutoSize = true
            };

            // Bottom Panel for OK Button
            var pnlBottom = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 56,
                BackColor = Color.FromArgb(245, 247, 250),
                Padding = new Padding(0, 0, 24, 0)
            };

            var btnOk = new Button
            {
                Text = "OK",
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Size = new Size(90, 32),
                Location = new Point(306, 12),
                BackColor = Color.FromArgb(52, 73, 94),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                DialogResult = DialogResult.OK
            };
            btnOk.FlatAppearance.BorderColor = Color.FromArgb(70, 90, 110);

            pnlBottom.Controls.Add(btnOk);

            pnlBody.Controls.Add(lblVersion);
            pnlBody.Controls.Add(lblDescription);
            pnlBody.Controls.Add(lblCreator);
            pnlBody.Controls.Add(pnlDivider);
            pnlBody.Controls.Add(lblTechTitle);
            pnlBody.Controls.Add(lblTechDetails);

            this.Controls.Add(pnlBody);
            this.Controls.Add(pnlBottom);
            this.Controls.Add(pnlTop);

            this.AcceptButton = btnOk;
            this.CancelButton = btnOk;

            this.ResumeLayout(false);
        }
    }
}
