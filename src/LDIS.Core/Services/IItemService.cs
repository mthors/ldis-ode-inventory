using System.Collections.Generic;
using LDIS.Core.DTOs;
using LDIS.Core.Models;

namespace LDIS.Core.Services
{
    public interface IItemService
    {
        Item GetItem(long itemId);
        Item GetItemBySku(string sku);
        ValidationResult ValidateItem(Item item, bool isNew);
        long SaveItem(Item item);
        void DeactivateItem(long itemId);
        void ActivateItem(long itemId);
        IEnumerable<ItemListItemDto> SearchItems(ItemSearchCriteria criteria);
        ProductDistinctAttributesDto GetDistinctAttributes();
        DashboardSummaryDto GetDashboardSummary();
    }
}
