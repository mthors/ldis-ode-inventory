using System;

namespace LDIS.Core.DTOs
{
    public class TransactionSearchCriteria
    {
        public string SearchText { get; set; }
        public long? ItemID { get; set; }
        public string TransactionType { get; set; } // null/"ALL", "IN", "OUT", "ADJUSTMENT"
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
