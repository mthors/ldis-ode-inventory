using System.Globalization;

namespace LDIS.Core.DTOs
{
    public class ItemListItemDto
    {
        public long ItemID { get; set; }
        public string SKU { get; set; }
        public string Name { get; set; }
        public long? CategoryID { get; set; }
        public string CategoryName { get; set; }
        public string Brand { get; set; }
        public string Color { get; set; }
        public string Size { get; set; }
        public string Gender { get; set; }
        public long PurchasePrice { get; set; }
        public long SellingPrice { get; set; }
        public int MinStockLevel { get; set; }
        public int CurrentStock { get; set; }
        public bool IsActive { get; set; }

        public string PurchasePriceFormatted
        {
            get
            {
                return string.Format(CultureInfo.GetCultureInfo("id-ID"), "Rp {0:N0}", PurchasePrice);
            }
        }

        public string SellingPriceFormatted
        {
            get
            {
                return string.Format(CultureInfo.GetCultureInfo("id-ID"), "Rp {0:N0}", SellingPrice);
            }
        }

        public string StatusText
        {
            get
            {
                return IsActive ? "Active" : "Inactive";
            }
        }
    }
}
