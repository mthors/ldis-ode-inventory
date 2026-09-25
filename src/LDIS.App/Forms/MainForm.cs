using System;
using System.Drawing;
using System.Windows.Forms;
using LDIS.Core.Data;

namespace LDIS.App.Forms
{
    public partial class MainForm : Form
    {
        private readonly DbConnectionFactory _connectionFactory;
        private readonly DatabaseInitializer _initializer;

        public MainForm(DbConnectionFactory connectionFactory, DatabaseInitializer initializer)
        {
            if (connectionFactory == null)
            {
                throw new ArgumentNullException("connectionFactory");
            }
            if (initializer == null)
            {
                throw new ArgumentNullException("initializer");
            }

            _connectionFactory = connectionFactory;
            _initializer = initializer;

            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            try
            {
                var config = _connectionFactory.Config;
                lblDbPath.Text = config.DatabaseFilePath;
                lblMode.Text = "Storage Mode: " + (config.IsPortableMode ? "Portable (Application Directory)" : "Standard (AppData)");

                int version = _initializer.GetCurrentSchemaVersion();
                lblSchemaVersion.Text = string.Format("Database Schema Version: {0} (Ready)", version);

                // Test an active query
                using (var conn = _connectionFactory.CreateOpenConnection())
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE type='table';";
                    var tableCount = Convert.ToInt32(cmd.ExecuteScalar());

                    lblConnectionState.Text = string.Format("Connection: Active & Verified ({0} system/user tables present)", tableCount);
                    lblConnectionState.ForeColor = Color.DarkGreen;
                }

                lblStatus.Text = "System Status: Foundation Ready";
            }
            catch (Exception ex)
            {
                lblConnectionState.Text = "Connection Error: " + ex.Message;
                lblConnectionState.ForeColor = Color.DarkRed;
                lblStatus.Text = "System Status: Initialization Error";
                MessageBox.Show(
                    "Failed to verify database connection:\n" + ex.Message,
                    "Database Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }
    }
}
