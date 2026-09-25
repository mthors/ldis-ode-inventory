using System;
using System.Windows.Forms;
using LDIS.App.Forms;
using LDIS.Core.Data;

namespace LDIS.App
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Top-level unhandled exception handling
            Application.ThreadException += delegate(object sender, System.Threading.ThreadExceptionEventArgs e)
            {
                MessageBox.Show(
                    "An unexpected error occurred:\n" + (e.Exception != null ? e.Exception.Message : "Unknown error"),
                    "Application Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            };

            AppDomain.CurrentDomain.UnhandledException += delegate(object sender, UnhandledExceptionEventArgs e)
            {
                Exception ex = e.ExceptionObject as Exception;
                MessageBox.Show(
                    "A critical unhandled error occurred:\n" + (ex != null ? ex.Message : "Unknown error"),
                    "Fatal Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            };

            try
            {
                // Initialize database
                DatabaseConfig config = new DatabaseConfig();
                DbConnectionFactory connectionFactory = new DbConnectionFactory(config);
                DatabaseInitializer initializer = new DatabaseInitializer(connectionFactory);

                initializer.Initialize();

                Application.Run(new MainForm(connectionFactory, initializer));
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Failed to initialize the application database:\n" + ex.Message,
                    "Startup Failure",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }
    }
}
