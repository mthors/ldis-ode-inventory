using System;
using System.Collections.Generic;
using System.Data.SQLite;
using LDIS.Core.Models;

namespace LDIS.Core.Data.Repositories
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly DbConnectionFactory _connectionFactory;

        public CategoryRepository(DbConnectionFactory connectionFactory)
        {
            if (connectionFactory == null)
            {
                throw new ArgumentNullException("connectionFactory");
            }
            _connectionFactory = connectionFactory;
        }

        public IEnumerable<Category> GetAll()
        {
            var list = new List<Category>();

            using (var conn = _connectionFactory.CreateOpenConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT CategoryID, CategoryName FROM Categories ORDER BY CategoryName COLLATE NOCASE ASC;";
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new Category
                        {
                            CategoryID = Convert.ToInt64(reader["CategoryID"]),
                            CategoryName = reader["CategoryName"].ToString()
                        });
                    }
                }
            }

            return list;
        }

        public Category GetById(long categoryId)
        {
            using (var conn = _connectionFactory.CreateOpenConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT CategoryID, CategoryName FROM Categories WHERE CategoryID = @CategoryID;";
                cmd.Parameters.AddWithValue("@CategoryID", categoryId);

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return new Category
                        {
                            CategoryID = Convert.ToInt64(reader["CategoryID"]),
                            CategoryName = reader["CategoryName"].ToString()
                        };
                    }
                }
            }

            return null;
        }

        public Category GetByName(string categoryName)
        {
            if (string.IsNullOrWhiteSpace(categoryName))
            {
                return null;
            }

            using (var conn = _connectionFactory.CreateOpenConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT CategoryID, CategoryName FROM Categories WHERE LOWER(CategoryName) = LOWER(@CategoryName);";
                cmd.Parameters.AddWithValue("@CategoryName", categoryName.Trim());

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return new Category
                        {
                            CategoryID = Convert.ToInt64(reader["CategoryID"]),
                            CategoryName = reader["CategoryName"].ToString()
                        };
                    }
                }
            }

            return null;
        }

        public long Create(Category category)
        {
            if (category == null)
            {
                throw new ArgumentNullException("category");
            }

            using (var conn = _connectionFactory.CreateOpenConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
                    INSERT INTO Categories (CategoryName) 
                    VALUES (@CategoryName); 
                    SELECT last_insert_rowid();";
                cmd.Parameters.AddWithValue("@CategoryName", category.CategoryName.Trim());

                long id = Convert.ToInt64(cmd.ExecuteScalar());
                category.CategoryID = id;
                return id;
            }
        }

        public void Update(Category category)
        {
            if (category == null)
            {
                throw new ArgumentNullException("category");
            }

            using (var conn = _connectionFactory.CreateOpenConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "UPDATE Categories SET CategoryName = @CategoryName WHERE CategoryID = @CategoryID;";
                cmd.Parameters.AddWithValue("@CategoryName", category.CategoryName.Trim());
                cmd.Parameters.AddWithValue("@CategoryID", category.CategoryID);

                cmd.ExecuteNonQuery();
            }
        }

        public void Delete(long categoryId)
        {
            using (var conn = _connectionFactory.CreateOpenConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "DELETE FROM Categories WHERE CategoryID = @CategoryID;";
                cmd.Parameters.AddWithValue("@CategoryID", categoryId);

                cmd.ExecuteNonQuery();
            }
        }

        public bool IsCategoryInUse(long categoryId)
        {
            using (var conn = _connectionFactory.CreateOpenConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT COUNT(*) FROM Items WHERE CategoryID = @CategoryID;";
                cmd.Parameters.AddWithValue("@CategoryID", categoryId);

                long count = Convert.ToInt64(cmd.ExecuteScalar());
                return count > 0;
            }
        }
    }
}
