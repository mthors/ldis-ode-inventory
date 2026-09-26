namespace LDIS.Core.DTOs
{
    public class DashboardSummaryDto
    {
        public int TotalActiveProducts { get; set; }
        public int TotalUnitsInStock { get; set; }
        public int LowStockCount { get; set; }
        public int OutOfStockCount { get; set; }
    }
}
