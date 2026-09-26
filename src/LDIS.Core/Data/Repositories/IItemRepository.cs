using System.Collections.Generic;
using LDIS.Core.DTOs;
using LDIS.Core.Models;

namespace LDIS.Core.Data.Repositories
{
    public interface IItemRepository
    {
        Item GetById(long itemId);
        Item GetBySku(string sku);
        bool ExistsSku(string sku, long? excludeItemId = null);
        long Create(Item item);
        void Update(Item item);
        void SetActiveStatus(long itemId, bool isActive);
        IEnumerable<ItemListItemDto> Search(ItemSearchCriteria criteria);
        ProductDistinctAttributesDto GetDistinctAttributes();
        DashboardSummaryDto GetDashboardSummary();
    }
}
