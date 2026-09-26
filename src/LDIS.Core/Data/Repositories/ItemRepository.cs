using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using LDIS.Core.DTOs;
using LDIS.Core.Models;

namespace LDIS.Core.Data.Repositories
{
    public class ItemRepository : IItemRepository
    {
        private readonly DbConnectionFactory _connectionFactory;

        public ItemRepository(DbConnectionFactory connectionFactory)
        {
            if (connectionFactory == null)
            {
                throw new ArgumentNullException("connectionFactory");
            }
            _connectionFactory = connectionFactory;
        }

        public Item GetById(long itemId)
        {
            using (var conn = _connectionFactory.CreateOpenConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
                    SELECT ItemID, SKU, Name, CategoryID, Brand, Color, Size, Gender,
                           PurchasePrice, SellingPrice, MinStockLevel, CurrentStock, IsActive
                    FROM Items
                    WHERE ItemID = @ItemID;";
                cmd.Parameters.AddWithValue("@ItemID", itemId);

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return MapItem(reader);
                    }
                }
            }

            return null;
        }

        public Item GetBySku(string sku)
        {
            if (string.IsNullOrWhiteSpace(sku))
            {
                return null;
            }

            using (var conn = _connectionFactory.CreateOpenConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
                    SELECT ItemID, SKU, Name, CategoryID, Brand, Color, Size, Gender,
                           PurchasePrice, SellingPrice, MinStockLevel, CurrentStock, IsActive
                    FROM Items
                    WHERE LOWER(SKU) = LOWER(@SKU);";
                cmd.Parameters.AddWithValue("@SKU", sku.Trim());

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return MapItem(reader);
                    }
                }
            }

            return null;
        }

        public bool ExistsSku(string sku, long? excludeItemId = null)
        {
            if (string.IsNullOrWhiteSpace(sku))
            {
                return false;
            }

            using (var conn = _connectionFactory.CreateOpenConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
                    SELECT COUNT(*) 
                    FROM Items 
                    WHERE LOWER(SKU) = LOWER(@SKU) 
                      AND (@ExcludeID IS NULL OR ItemID != @ExcludeID);";
                cmd.Parameters.AddWithValue("@SKU", sku.Trim());
                cmd.Parameters.AddWithValue("@ExcludeID", excludeItemId.HasValue ? (object)excludeItemId.Value : DBNull.Value);

                long count = Convert.ToInt64(cmd.ExecuteScalar());
                return count > 0;
            }
        }

        public long Create(Item item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }

            using (var conn = _connectionFactory.CreateOpenConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
                    INSERT INTO Items (
                        SKU, Name, CategoryID, Brand, Color, Size, Gender,
                        PurchasePrice, SellingPrice, MinStockLevel, CurrentStock, IsActive
                    ) VALUES (
                        @SKU, @Name, @CategoryID, @Brand, @Color, @Size, @Gender,
                        @PurchasePrice, @SellingPrice, @MinStockLevel, 0, @IsActive
                    );
                    SELECT last_insert_rowid();";

                BindItemParameters(cmd, item);

                long id = Convert.ToInt64(cmd.ExecuteScalar());
                item.ItemID = id;
                item.CurrentStock = 0;
                return id;
            }
        }

        public void Update(Item item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }

            using (var conn = _connectionFactory.CreateOpenConnection())
            using (var cmd = conn.CreateCommand())
            {
                // CurrentStock is deliberately omitted: stock modifications must happen strictly via stock operations.
                cmd.CommandText = @"
                    UPDATE Items SET
                        SKU = @SKU,
                        Name = @Name,
                        CategoryID = @CategoryID,
                        Brand = @Brand,
                        Color = @Color,
                        Size = @Size,
                        Gender = @Gender,
                        PurchasePrice = @PurchasePrice,
                        SellingPrice = @SellingPrice,
                        MinStockLevel = @MinStockLevel,
                        IsActive = @IsActive
                    WHERE ItemID = @ItemID;";

                BindItemParameters(cmd, item);
                cmd.Parameters.AddWithValue("@ItemID", item.ItemID);

                cmd.ExecuteNonQuery();
            }
        }

        public void SetActiveStatus(long itemId, bool isActive)
        {
            using (var conn = _connectionFactory.CreateOpenConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "UPDATE Items SET IsActive = @IsActive WHERE ItemID = @ItemID;";
                cmd.Parameters.AddWithValue("@ItemID", itemId);
                cmd.Parameters.AddWithValue("@IsActive", isActive ? 1 : 0);

                cmd.ExecuteNonQuery();
            }
        }

        public IEnumerable<ItemListItemDto> Search(ItemSearchCriteria criteria)
        {
            var list = new List<ItemListItemDto>();

            using (var conn = _connectionFactory.CreateOpenConnection())
            using (var cmd = conn.CreateCommand())
            {
                var query = @"
                    SELECT i.ItemID, i.SKU, i.Name, i.CategoryID, c.CategoryName,
                           i.Brand, i.Color, i.Size, i.Gender,
                           i.PurchasePrice, i.SellingPrice, i.MinStockLevel, i.CurrentStock, i.IsActive
                    FROM Items i
                    LEFT JOIN Categories c ON i.CategoryID = c.CategoryID
                    WHERE 1=1 ";

                if (criteria != null)
                {
                    if (!string.IsNullOrWhiteSpace(criteria.SearchText))
                    {
                        query += @" AND (
                            i.SKU LIKE @search OR
                            i.Name LIKE @search OR
                            i.Brand LIKE @search OR
                            i.Color LIKE @search OR
                            i.Size LIKE @search
                        ) ";
                        cmd.Parameters.AddWithValue("@search", "%" + criteria.SearchText.Trim() + "%");
                    }

                    if (criteria.CategoryID.HasValue && criteria.CategoryID.Value > 0)
                    {
                        query += " AND i.CategoryID = @categoryId ";
                        cmd.Parameters.AddWithValue("@categoryId", criteria.CategoryID.Value);
                    }

                    if (!string.IsNullOrWhiteSpace(criteria.Brand))
                    {
                        query += " AND LOWER(i.Brand) = LOWER(@brand) ";
                        cmd.Parameters.AddWithValue("@brand", criteria.Brand.Trim());
                    }

                    if (!string.IsNullOrWhiteSpace(criteria.Color))
                    {
                        query += " AND LOWER(i.Color) = LOWER(@color) ";
                        cmd.Parameters.AddWithValue("@color", criteria.Color.Trim());
                    }

                    if (!string.IsNullOrWhiteSpace(criteria.Size))
                    {
                        query += " AND LOWER(i.Size) = LOWER(@size) ";
                        cmd.Parameters.AddWithValue("@size", criteria.Size.Trim());
                    }

                    if (criteria.ActiveStatus == ActiveFilterStatus.ActiveOnly)
                    {
                        query += " AND i.IsActive = 1 ";
                    }
                    else if (criteria.ActiveStatus == ActiveFilterStatus.InactiveOnly)
                    {
                        query += " AND i.IsActive = 0 ";
                    }
                }

                query += " ORDER BY i.Name COLLATE NOCASE ASC, i.SKU COLLATE NOCASE ASC;";
                cmd.CommandText = query;

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new ItemListItemDto
                        {
                            ItemID = Convert.ToInt64(reader["ItemID"]),
                            SKU = reader["SKU"].ToString(),
                            Name = reader["Name"].ToString(),
                            CategoryID = reader["CategoryID"] != DBNull.Value ? Convert.ToInt64(reader["CategoryID"]) : (long?)null,
                            CategoryName = reader["CategoryName"] != DBNull.Value ? reader["CategoryName"].ToString() : null,
                            Brand = reader["Brand"] != DBNull.Value ? reader["Brand"].ToString() : string.Empty,
                            Color = reader["Color"] != DBNull.Value ? reader["Color"].ToString() : string.Empty,
                            Size = reader["Size"] != DBNull.Value ? reader["Size"].ToString() : string.Empty,
                            Gender = reader["Gender"] != DBNull.Value ? reader["Gender"].ToString() : string.Empty,
                            PurchasePrice = Convert.ToInt64(reader["PurchasePrice"]),
                            SellingPrice = Convert.ToInt64(reader["SellingPrice"]),
                            MinStockLevel = Convert.ToInt32(reader["MinStockLevel"]),
                            CurrentStock = Convert.ToInt32(reader["CurrentStock"]),
                            IsActive = Convert.ToInt32(reader["IsActive"]) == 1
                        });
                    }
                }
            }

            return list;
        }

        public ProductDistinctAttributesDto GetDistinctAttributes()
        {
            var dto = new ProductDistinctAttributesDto();

            using (var conn = _connectionFactory.CreateOpenConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
                    SELECT DISTINCT Brand FROM Items WHERE Brand IS NOT NULL AND TRIM(Brand) != '' ORDER BY Brand COLLATE NOCASE ASC;
                    SELECT DISTINCT Color FROM Items WHERE Color IS NOT NULL AND TRIM(Color) != '' ORDER BY Color COLLATE NOCASE ASC;
                    SELECT DISTINCT Size  FROM Items WHERE Size IS NOT NULL AND TRIM(Size) != '' ORDER BY Size COLLATE NOCASE ASC;";

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        dto.Brands.Add(reader.GetString(0));
                    }

                    if (reader.NextResult())
                    {
                        while (reader.Read())
                        {
                            dto.Colors.Add(reader.GetString(0));
                        }
                    }

                    if (reader.NextResult())
                    {
                        while (reader.Read())
                        {
                            dto.Sizes.Add(reader.GetString(0));
                        }
                    }
                }
            }

            return dto;
        }

        private static void BindItemParameters(SQLiteCommand cmd, Item item)
        {
            cmd.Parameters.AddWithValue("@SKU", item.SKU != null ? item.SKU.Trim() : string.Empty);
            cmd.Parameters.AddWithValue("@Name", item.Name != null ? item.Name.Trim() : string.Empty);
            cmd.Parameters.AddWithValue("@CategoryID", item.CategoryID.HasValue && item.CategoryID.Value > 0 ? (object)item.CategoryID.Value : DBNull.Value);
            cmd.Parameters.AddWithValue("@Brand", !string.IsNullOrWhiteSpace(item.Brand) ? (object)item.Brand.Trim() : DBNull.Value);
            cmd.Parameters.AddWithValue("@Color", !string.IsNullOrWhiteSpace(item.Color) ? (object)item.Color.Trim() : DBNull.Value);
            cmd.Parameters.AddWithValue("@Size", !string.IsNullOrWhiteSpace(item.Size) ? (object)item.Size.Trim() : DBNull.Value);
            cmd.Parameters.AddWithValue("@Gender", !string.IsNullOrWhiteSpace(item.Gender) ? (object)item.Gender.Trim() : DBNull.Value);
            cmd.Parameters.AddWithValue("@PurchasePrice", item.PurchasePrice);
            cmd.Parameters.AddWithValue("@SellingPrice", item.SellingPrice);
            cmd.Parameters.AddWithValue("@MinStockLevel", item.MinStockLevel);
            cmd.Parameters.AddWithValue("@IsActive", item.IsActive ? 1 : 0);
        }

        private static Item MapItem(IDataRecord reader)
        {
            return new Item
            {
                ItemID = Convert.ToInt64(reader["ItemID"]),
                SKU = reader["SKU"].ToString(),
                Name = reader["Name"].ToString(),
                CategoryID = reader["CategoryID"] != DBNull.Value ? Convert.ToInt64(reader["CategoryID"]) : (long?)null,
                Brand = reader["Brand"] != DBNull.Value ? reader["Brand"].ToString() : string.Empty,
                Color = reader["Color"] != DBNull.Value ? reader["Color"].ToString() : string.Empty,
                Size = reader["Size"] != DBNull.Value ? reader["Size"].ToString() : string.Empty,
                Gender = reader["Gender"] != DBNull.Value ? reader["Gender"].ToString() : string.Empty,
                PurchasePrice = Convert.ToInt64(reader["PurchasePrice"]),
                SellingPrice = Convert.ToInt64(reader["SellingPrice"]),
                MinStockLevel = Convert.ToInt32(reader["MinStockLevel"]),
                CurrentStock = Convert.ToInt32(reader["CurrentStock"]),
                IsActive = Convert.ToInt32(reader["IsActive"]) == 1
            };
        }
    }
}
