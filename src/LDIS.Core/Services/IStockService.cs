using System.Collections.Generic;
using LDIS.Core.DTOs;
using LDIS.Core.Models;

namespace LDIS.Core.Services
{
    public interface IStockService
    {
        ValidationResult ValidateOperation(StockOperationRequest request);
        long StockIn(StockOperationRequest request);
        long StockOut(StockOperationRequest request);
        long StockAdjustment(StockOperationRequest request);
        long ExecuteOperation(StockOperationRequest request);
        IEnumerable<TransactionListItemDto> SearchTransactions(TransactionSearchCriteria criteria);
        InventoryTransaction GetTransactionById(long transactionId);
    }
}
