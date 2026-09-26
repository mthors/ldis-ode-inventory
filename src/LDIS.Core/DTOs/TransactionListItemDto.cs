using System;

namespace LDIS.Core.DTOs
{
    public class TransactionListItemDto
    {
        public long TransactionID { get; set; }
        public DateTime TransactionDate { get; set; }
        public long ItemID { get; set; }
        public string SKU { get; set; }
        public string ItemName { get; set; }
        public string TransactionType { get; set; } // "IN", "OUT", "ADJUSTMENT"
        public int Quantity { get; set; }
        public long UnitPrice { get; set; }
        public string ReferenceNumber { get; set; }
        public string SupplierOrCustomer { get; set; }
        public string Reason { get; set; }
        public string Notes { get; set; }
        public string CreatedBy { get; set; }
    }
}
