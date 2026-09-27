using System.Collections.Generic;
using System.IO;
using LDIS.Core.DTOs;

namespace LDIS.Core.Services
{
    public interface IExportService
    {
        // CSV Exports (Preserved exactly as M5)
        void ExportProductsToCsv(IEnumerable<ItemListItemDto> items, TextWriter writer);
        void ExportProductsToCsv(IEnumerable<ItemListItemDto> items, string filePath);

        void ExportTransactionsToCsv(IEnumerable<TransactionListItemDto> transactions, TextWriter writer);
        void ExportTransactionsToCsv(IEnumerable<TransactionListItemDto> transactions, string filePath);

        // Excel Exports (M7)
        void ExportProductsToExcel(IEnumerable<ItemListItemDto> items, Stream stream);
        void ExportProductsToExcel(IEnumerable<ItemListItemDto> items, string filePath);

        void ExportTransactionsToExcel(IEnumerable<TransactionListItemDto> transactions, Stream stream);
        void ExportTransactionsToExcel(IEnumerable<TransactionListItemDto> transactions, string filePath);
    }
}
