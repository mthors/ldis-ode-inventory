using System;
using System.IO;

namespace LDIS.Core.Data
{
    public class DatabaseConfig
    {
        private const string DatabaseFileName = "inventory.db";
        private readonly string _databaseFilePath;
        private readonly bool _isPortableMode;

        public DatabaseConfig() : this(null)
        {
        }

        public DatabaseConfig(string customPath)
        {
            if (!string.IsNullOrWhiteSpace(customPath))
            {
                _databaseFilePath = Path.GetFullPath(customPath);
                _isPortableMode = false;
            }
            else
            {
                // Check if portable mode directory exists: .\database next to executable
                string appDir = AppDomain.CurrentDomain.BaseDirectory;
                string portableDir = Path.Combine(appDir, "database");

                if (Directory.Exists(portableDir))
                {
                    _databaseFilePath = Path.Combine(portableDir, DatabaseFileName);
                    _isPortableMode = true;
                }
                else
                {
                    // Default to LocalAppData: %LocalAppData%\LDIS\inventory.db
                    string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                    string defaultDir = Path.Combine(localAppData, "LDIS");
                    _databaseFilePath = Path.Combine(defaultDir, DatabaseFileName);
                    _isPortableMode = false;
                }
            }

            EnsureDirectoryExists();
        }

        public string DatabaseFilePath
        {
            get { return _databaseFilePath; }
        }

        public bool IsPortableMode
        {
            get { return _isPortableMode; }
        }

        public string ConnectionString
        {
            get { return string.Format("Data Source={0};Version=3;Foreign Keys=True;", _databaseFilePath); }
        }

        public void EnsureDirectoryExists()
        {
            string dir = Path.GetDirectoryName(_databaseFilePath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
        }
    }
}
