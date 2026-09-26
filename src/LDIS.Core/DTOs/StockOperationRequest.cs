using System;

namespace LDIS.Core.DTOs
{
    public class StockOperationRequest
    {
        public long ItemID { get; set; }
        public string OperationType { get; set; } // "IN", "OUT", "ADJUSTMENT"
        public int Quantity { get; set; }          // Positive for IN/OUT; Signed delta for ADJUSTMENT
        public long UnitPrice { get; set; }        // Whole Rupiah
        public string ReferenceNumber { get; set; }
        public string SupplierOrCustomer { get; set; }
        public string Reason { get; set; }
        public string Notes { get; set; }
        public DateTime? TransactionDate { get; set; }
        public string CreatedBy { get; set; }
    }
}
