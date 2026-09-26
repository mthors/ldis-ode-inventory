namespace LDIS.Core.DTOs
{
    public enum ActiveFilterStatus
    {
        ActiveOnly = 0,
        InactiveOnly = 1,
        All = 2
    }

    public enum StockFilterStatus
    {
        All = 0,
        NormalStock = 1,
        LowStock = 2,
        OutOfStock = 3
    }

    public class ItemSearchCriteria
    {
        public string SearchText { get; set; }
        public long? CategoryID { get; set; }
        public string Brand { get; set; }
        public string Color { get; set; }
        public string Size { get; set; }
        public string Gender { get; set; }
        public ActiveFilterStatus ActiveStatus { get; set; }
        public StockFilterStatus StockStatus { get; set; }

        public ItemSearchCriteria()
        {
            ActiveStatus = ActiveFilterStatus.ActiveOnly;
            StockStatus = StockFilterStatus.All;
        }
    }
}
