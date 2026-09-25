namespace LDIS.Core.Models
{
    public class Item
    {
        public Item()
        {
            IsActive = true;
        }

        public long ItemID { get; set; }
        public string SKU { get; set; }
        public string Name { get; set; }
        public long? CategoryID { get; set; }
        public string Brand { get; set; }
        public string Color { get; set; }
        public string Size { get; set; }
        public string Gender { get; set; }
        public long PurchasePrice { get; set; }
        public long SellingPrice { get; set; }
        public int MinStockLevel { get; set; }
        public int CurrentStock { get; set; }
        public bool IsActive { get; set; }

        public override string ToString()
        {
            return string.IsNullOrEmpty(SKU) ? Name : string.Format("[{0}] {1}", SKU, Name);
        }
    }
}
