using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using LDIS.Core.DTOs;
using LDIS.Core.Models;

namespace LDIS.Core.Data.Repositories
{
    public class InventoryTransactionRepository : IInventoryTransactionRepository
    {
        private readonly DbConnectionFactory _connectionFactory;

        public InventoryTransactionRepository(DbConnectionFactory connectionFactory)
        {
            if (connectionFactory == null)
            {
                throw new ArgumentNullException("connectionFactory");
            }
            _connectionFactory = connectionFactory;
        }

        public long ExecuteStockOperation(InventoryTransaction transaction, int stockDelta)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException("transaction");
            }

            using (var conn = _connectionFactory.CreateOpenConnection())
            using (var trans = conn.BeginTransaction())
            {
                try
                {
                    // 1. Read current item state within transaction
                    long currentStock = 0;
                    bool isActive = false;

                    using (var cmdRead = conn.CreateCommand())
                    {
                        cmdRead.Transaction = trans;
                        cmdRead.CommandText = @"
                            SELECT CurrentStock, IsActive 
                            FROM Items 
                            WHERE ItemID = @ItemID;";
                        cmdRead.Parameters.AddWithValue("@ItemID", transaction.ItemID);

                        using (var reader = cmdRead.ExecuteReader())
                        {
                            if (!reader.Read())
                            {
                                throw new InvalidOperationException("Product not found.");
                            }

                            currentStock = Convert.ToInt64(reader["CurrentStock"]);
                            isActive = Convert.ToInt32(reader["IsActive"]) == 1;
                        }
                    }

                    // 2. Validate item status
                    if (!isActive)
                    {
                        throw new InvalidOperationException("Cannot perform stock operations on an inactive product.");
                    }

                    if (transaction.TransactionType == "OUT")
                    {
                        if (currentStock < transaction.Quantity)
                        {
                            throw new InvalidOperationException(string.Format(
                                "Insufficient stock. Current stock is {0}, but requested {1}.",
                                currentStock, transaction.Quantity));
                        }
                    }
                    else if (transaction.TransactionType == "ADJUSTMENT")
                    {
                        if (currentStock + stockDelta < 0)
                        {
                            throw new InvalidOperationException(string.Format(
                                "Adjustment would result in negative stock. Current stock is {0}, adjustment is {1}.",
                                currentStock, stockDelta));
                        }
                    }

                    // 3. Insert InventoryTransactions record
                    long transactionId;
                    using (var cmdInsert = conn.CreateCommand())
                    {
                        cmdInsert.Transaction = trans;
                        cmdInsert.CommandText = @"
                            INSERT INTO InventoryTransactions (
                                ItemID, TransactionType, Quantity, UnitPrice,
                                ReferenceNumber, SupplierOrCustomer, Reason, Notes,
                                TransactionDate, CreatedBy
                            ) VALUES (
                                @ItemID, @TransactionType, @Quantity, @UnitPrice,
                                @ReferenceNumber, @SupplierOrCustomer, @Reason, @Notes,
                                @TransactionDate, @CreatedBy
                            );
                            SELECT last_insert_rowid();";

                        cmdInsert.Parameters.AddWithValue("@ItemID", transaction.ItemID);
                        cmdInsert.Parameters.AddWithValue("@TransactionType", transaction.TransactionType);
                        cmdInsert.Parameters.AddWithValue("@Quantity", transaction.Quantity);
                        cmdInsert.Parameters.AddWithValue("@UnitPrice", transaction.UnitPrice);
                        cmdInsert.Parameters.AddWithValue("@ReferenceNumber", !string.IsNullOrWhiteSpace(transaction.ReferenceNumber) ? (object)transaction.ReferenceNumber.Trim() : DBNull.Value);
                        cmdInsert.Parameters.AddWithValue("@SupplierOrCustomer", !string.IsNullOrWhiteSpace(transaction.SupplierOrCustomer) ? (object)transaction.SupplierOrCustomer.Trim() : DBNull.Value);
                        cmdInsert.Parameters.AddWithValue("@Reason", !string.IsNullOrWhiteSpace(transaction.Reason) ? (object)transaction.Reason.Trim() : DBNull.Value);
                        cmdInsert.Parameters.AddWithValue("@Notes", !string.IsNullOrWhiteSpace(transaction.Notes) ? (object)transaction.Notes.Trim() : DBNull.Value);
                        cmdInsert.Parameters.AddWithValue("@TransactionDate", transaction.TransactionDate.ToString("yyyy-MM-dd HH:mm:ss"));
                        cmdInsert.Parameters.AddWithValue("@CreatedBy", !string.IsNullOrWhiteSpace(transaction.CreatedBy) ? (object)transaction.CreatedBy.Trim() : DBNull.Value);

                        transactionId = Convert.ToInt64(cmdInsert.ExecuteScalar());
                    }

                    // 4. Conditional stock UPDATE: enforces non-negative stock constraint directly at the database engine level
                    using (var cmdUpdate = conn.CreateCommand())
                    {
                        cmdUpdate.Transaction = trans;
                        cmdUpdate.CommandText = @"
                            UPDATE Items
                            SET CurrentStock = CurrentStock + @Delta
                            WHERE ItemID = @ItemID
                              AND CurrentStock + @Delta >= 0;";

                        cmdUpdate.Parameters.AddWithValue("@Delta", stockDelta);
                        cmdUpdate.Parameters.AddWithValue("@ItemID", transaction.ItemID);

                        int rowsAffected = cmdUpdate.ExecuteNonQuery();

                        // 5. Verify affected rows
                        if (rowsAffected != 1)
                        {
                            throw new InvalidOperationException(
                                "Stock update rejected: operation would result in negative stock.");
                        }
                    }

                    // 6. Commit transaction
                    trans.Commit();
                    transaction.TransactionID = transactionId;
                    return transactionId;
                }
                catch
                {
                    trans.Rollback();
                    throw;
                }
            }
        }

        public InventoryTransaction GetById(long transactionId)
        {
            using (var conn = _connectionFactory.CreateOpenConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
                    SELECT TransactionID, ItemID, TransactionType, Quantity, UnitPrice,
                           ReferenceNumber, SupplierOrCustomer, Reason, Notes,
                           TransactionDate, CreatedBy
                    FROM InventoryTransactions
                    WHERE TransactionID = @TransactionID;";
                cmd.Parameters.AddWithValue("@TransactionID", transactionId);

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return MapTransaction(reader);
                    }
                }
            }

            return null;
        }

        public IEnumerable<TransactionListItemDto> SearchTransactions(TransactionSearchCriteria criteria)
        {
            var list = new List<TransactionListItemDto>();

            using (var conn = _connectionFactory.CreateOpenConnection())
            using (var cmd = conn.CreateCommand())
            {
                var query = @"
                    SELECT t.TransactionID, t.TransactionDate, t.ItemID, i.SKU, i.Name AS ItemName,
                           t.TransactionType, t.Quantity, t.UnitPrice,
                           t.ReferenceNumber, t.SupplierOrCustomer, t.Reason, t.Notes, t.CreatedBy
                    FROM InventoryTransactions t
                    INNER JOIN Items i ON t.ItemID = i.ItemID
                    WHERE 1=1 ";

                if (criteria != null)
                {
                    if (!string.IsNullOrWhiteSpace(criteria.SearchText))
                    {
                        query += @" AND (
                            i.SKU LIKE @search OR
                            i.Name LIKE @search OR
                            t.ReferenceNumber LIKE @search OR
                            t.SupplierOrCustomer LIKE @search
                        ) ";
                        cmd.Parameters.AddWithValue("@search", "%" + criteria.SearchText.Trim() + "%");
                    }

                    if (criteria.ItemID.HasValue && criteria.ItemID.Value > 0)
                    {
                        query += " AND t.ItemID = @itemId ";
                        cmd.Parameters.AddWithValue("@itemId", criteria.ItemID.Value);
                    }

                    if (!string.IsNullOrWhiteSpace(criteria.TransactionType) &&
                        !criteria.TransactionType.Equals("ALL", StringComparison.OrdinalIgnoreCase))
                    {
                        query += " AND t.TransactionType = @transType ";
                        cmd.Parameters.AddWithValue("@transType", criteria.TransactionType.Trim().ToUpperInvariant());
                    }

                    if (criteria.StartDate.HasValue)
                    {
                        query += " AND t.TransactionDate >= @startDate ";
                        cmd.Parameters.AddWithValue("@startDate", criteria.StartDate.Value.ToString("yyyy-MM-dd 00:00:00"));
                    }

                    if (criteria.EndDate.HasValue)
                    {
                        query += " AND t.TransactionDate <= @endDate ";
                        cmd.Parameters.AddWithValue("@endDate", criteria.EndDate.Value.ToString("yyyy-MM-dd 23:59:59"));
                    }
                }

                query += " ORDER BY t.TransactionDate DESC, t.TransactionID DESC;";
                cmd.CommandText = query;

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new TransactionListItemDto
                        {
                            TransactionID = Convert.ToInt64(reader["TransactionID"]),
                            TransactionDate = DateTime.Parse(reader["TransactionDate"].ToString()),
                            ItemID = Convert.ToInt64(reader["ItemID"]),
                            SKU = reader["SKU"].ToString(),
                            ItemName = reader["ItemName"].ToString(),
                            TransactionType = reader["TransactionType"].ToString(),
                            Quantity = Convert.ToInt32(reader["Quantity"]),
                            UnitPrice = Convert.ToInt64(reader["UnitPrice"]),
                            ReferenceNumber = reader["ReferenceNumber"] != DBNull.Value ? reader["ReferenceNumber"].ToString() : string.Empty,
                            SupplierOrCustomer = reader["SupplierOrCustomer"] != DBNull.Value ? reader["SupplierOrCustomer"].ToString() : string.Empty,
                            Reason = reader["Reason"] != DBNull.Value ? reader["Reason"].ToString() : string.Empty,
                            Notes = reader["Notes"] != DBNull.Value ? reader["Notes"].ToString() : string.Empty,
                            CreatedBy = reader["CreatedBy"] != DBNull.Value ? reader["CreatedBy"].ToString() : string.Empty
                        });
                    }
                }
            }

            return list;
        }

        private static InventoryTransaction MapTransaction(IDataRecord reader)
        {
            return new InventoryTransaction
            {
                TransactionID = Convert.ToInt64(reader["TransactionID"]),
                ItemID = Convert.ToInt64(reader["ItemID"]),
                TransactionType = reader["TransactionType"].ToString(),
                Quantity = Convert.ToInt32(reader["Quantity"]),
                UnitPrice = Convert.ToInt64(reader["UnitPrice"]),
                ReferenceNumber = reader["ReferenceNumber"] != DBNull.Value ? reader["ReferenceNumber"].ToString() : null,
                SupplierOrCustomer = reader["SupplierOrCustomer"] != DBNull.Value ? reader["SupplierOrCustomer"].ToString() : null,
                Reason = reader["Reason"] != DBNull.Value ? reader["Reason"].ToString() : null,
                Notes = reader["Notes"] != DBNull.Value ? reader["Notes"].ToString() : null,
                TransactionDate = DateTime.Parse(reader["TransactionDate"].ToString()),
                CreatedBy = reader["CreatedBy"] != DBNull.Value ? reader["CreatedBy"].ToString() : null
            };
        }
    }
}
