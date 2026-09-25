using System;
using System.Data.SQLite;

namespace LDIS.Core.Data
{
    public class DbConnectionFactory
    {
        private readonly DatabaseConfig _config;

        public DbConnectionFactory(DatabaseConfig config)
        {
            if (config == null)
            {
                throw new ArgumentNullException("config");
            }
            _config = config;
        }

        public DatabaseConfig Config
        {
            get { return _config; }
        }

        public SQLiteConnection CreateConnection()
        {
            return new SQLiteConnection(_config.ConnectionString);
        }

        public SQLiteConnection CreateOpenConnection()
        {
            var connection = CreateConnection();
            connection.Open();

            using (var cmd = connection.CreateCommand())
            {
                // PRD Section 8: Foreign-key enforcement must be enabled for every database connection
                cmd.CommandText = "PRAGMA foreign_keys = ON; PRAGMA busy_timeout = 5000;";
                cmd.ExecuteNonQuery();
            }

            return connection;
        }
    }
}
