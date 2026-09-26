using System.Collections.Generic;
using System.IO;
using LDIS.Core.DTOs;

namespace LDIS.Core.Services
{
    public interface IExportService
    {
        void ExportProductsToCsv(IEnumerable<ItemListItemDto> items, TextWriter writer);
        void ExportProductsToCsv(IEnumerable<ItemListItemDto> items, string filePath);

        void ExportTransactionsToCsv(IEnumerable<TransactionListItemDto> transactions, TextWriter writer);
        void ExportTransactionsToCsv(IEnumerable<TransactionListItemDto> transactions, string filePath);
    }
}
