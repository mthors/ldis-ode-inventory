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

                // Initialize repositories and services
                var categoryRepository = new LDIS.Core.Data.Repositories.CategoryRepository(connectionFactory);
                var itemRepository = new LDIS.Core.Data.Repositories.ItemRepository(connectionFactory);
                var transactionRepository = new LDIS.Core.Data.Repositories.InventoryTransactionRepository(connectionFactory);

                var categoryService = new LDIS.Core.Services.CategoryService(categoryRepository);
                var itemService = new LDIS.Core.Services.ItemService(itemRepository, categoryRepository);
                var stockService = new LDIS.Core.Services.StockService(transactionRepository, itemRepository);

                Application.Run(new MainForm(connectionFactory, initializer, itemService, categoryService, stockService));
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
