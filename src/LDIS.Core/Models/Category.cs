namespace LDIS.Core.Models
{
    public class Category
    {
        public long CategoryID { get; set; }
        public string CategoryName { get; set; }

        public override string ToString()
        {
            return CategoryName ?? string.Empty;
        }
    }
}
