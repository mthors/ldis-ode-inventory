using System;
using System.Data.SQLite;
using System.IO;
using System.Reflection;

namespace LDIS.Core.Data
{
    public class DatabaseInitializer
    {
        private readonly DbConnectionFactory _connectionFactory;

        public DatabaseInitializer(DbConnectionFactory connectionFactory)
        {
            if (connectionFactory == null)
            {
                throw new ArgumentNullException("connectionFactory");
            }
            _connectionFactory = connectionFactory;
        }

        public int GetCurrentSchemaVersion()
        {
            using (var connection = _connectionFactory.CreateOpenConnection())
            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = "PRAGMA user_version;";
                var result = cmd.ExecuteScalar();
                return Convert.ToInt32(result);
            }
        }

        public void Initialize()
        {
            _connectionFactory.Config.EnsureDirectoryExists();

            int currentVersion = GetCurrentSchemaVersion();
            if (currentVersion == 0)
            {
                ApplyInitialSchema();
            }
        }

        private void ApplyInitialSchema()
        {
            string script = LoadEmbeddedScript("001_initial_schema.sql");

            using (var connection = _connectionFactory.CreateOpenConnection())
            using (var transaction = connection.BeginTransaction())
            {
                try
                {
                    using (var cmd = connection.CreateCommand())
                    {
                        cmd.Transaction = transaction;
                        cmd.CommandText = script;
                        cmd.ExecuteNonQuery();

                        // Set schema version to 1
                        cmd.CommandText = "PRAGMA user_version = 1;";
                        cmd.ExecuteNonQuery();
                    }

                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        private string LoadEmbeddedScript(string scriptFileName)
        {
            var assembly = Assembly.GetExecutingAssembly();
            string resourceName = string.Format("LDIS.Core.Data.Scripts.{0}", scriptFileName);

            using (var stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream == null)
                {
                    // Fallback to checking disk if not found in embedded resources
                    string fallbackPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "database", "schema.sql");
                    if (File.Exists(fallbackPath))
                    {
                        return File.ReadAllText(fallbackPath);
                    }

                    throw new InvalidOperationException(string.Format("Embedded migration script '{0}' not found in assembly.", resourceName));
                }

                using (var reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
        }
    }
}
