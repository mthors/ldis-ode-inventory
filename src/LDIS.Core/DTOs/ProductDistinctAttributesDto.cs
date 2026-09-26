using System.Collections.Generic;

namespace LDIS.Core.DTOs
{
    public class ProductDistinctAttributesDto
    {
        public List<string> Brands { get; set; }
        public List<string> Colors { get; set; }
        public List<string> Sizes { get; set; }

        public ProductDistinctAttributesDto()
        {
            Brands = new List<string>();
            Colors = new List<string>();
            Sizes = new List<string>();
        }
    }
}
