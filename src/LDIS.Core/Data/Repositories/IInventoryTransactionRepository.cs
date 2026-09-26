using System.Collections.Generic;
using LDIS.Core.DTOs;
using LDIS.Core.Models;

namespace LDIS.Core.Data.Repositories
{
    public interface IInventoryTransactionRepository
    {
        long ExecuteStockOperation(InventoryTransaction transaction, int stockDelta);
        InventoryTransaction GetById(long transactionId);
        IEnumerable<TransactionListItemDto> SearchTransactions(TransactionSearchCriteria criteria);
    }
}
