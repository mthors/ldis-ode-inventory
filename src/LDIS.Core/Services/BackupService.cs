using System;
using System.Data.SQLite;
using System.IO;
using LDIS.Core.Data;

namespace LDIS.Core.Services
{
    /// <summary>
    /// Service responsible for performing consistent SQLite online backups/snapshots
    /// using the native SQLite online backup API via System.Data.SQLite.
    /// </summary>
    public class BackupService : IBackupService
    {
        private readonly DbConnectionFactory _connectionFactory;

        public BackupService(DbConnectionFactory connectionFactory)
        {
            if (connectionFactory == null) throw new ArgumentNullException("connectionFactory");
            _connectionFactory = connectionFactory;
        }

        public void BackupDatabase(string destinationFilePath)
        {
            if (string.IsNullOrWhiteSpace(destinationFilePath))
            {
                throw new ArgumentException("Destination file path cannot be null or empty.", "destinationFilePath");
            }

            string sourcePath = _connectionFactory.Config.DatabaseFilePath;
            if (!File.Exists(sourcePath))
            {
                throw new FileNotFoundException("Active database file does not exist: " + sourcePath);
            }

            string fullDestPath = Path.GetFullPath(destinationFilePath);
            string fullSourcePath = Path.GetFullPath(sourcePath);

            if (string.Equals(fullDestPath, fullSourcePath, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Backup destination cannot be the same as the active database file.");
            }

            string destDir = Path.GetDirectoryName(fullDestPath);
            if (!string.IsNullOrEmpty(destDir) && !Directory.Exists(destDir))
            {
                Directory.CreateDirectory(destDir);
            }

            // Using the SQLite native online backup mechanism (sqlite3_backup_* API)
            // Available overload in System.Data.SQLite 1.0.118.0:
            // BackupDatabase(SQLiteConnection destination, string destinationName, string sourceName, int pages, SQLiteBackupCallback callback, int retryMilliseconds)
            using (var sourceConn = _connectionFactory.CreateOpenConnection())
            {
                string destConnString = string.Format("Data Source={0};Version=3;", fullDestPath);
                using (var destConn = new SQLiteConnection(destConnString))
                {
                    destConn.Open();
                    // pages = -1 copies all pages in a single consistent online snapshot
                    sourceConn.BackupDatabase(destConn, "main", "main", -1, null, 0);
                    destConn.Close();
                }
            }
        }
    }
}
