using System;

namespace LDIS.Core.Models
{
    public static class GenderOptions
    {
        public const string Unisex = "Unisex";
        public const string Men = "Men";
        public const string Women = "Women";
        public const string Kids = "Kids";
        public const string None = "None / Unspecified";

        public static readonly string[] All = new string[]
        {
            Unisex,
            Men,
            Women,
            Kids,
            None
        };

        public static bool IsValid(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return true;
            }

            foreach (var option in All)
            {
                if (string.Equals(option, value.Trim(), StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        public static string Normalize(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return None;
            }

            foreach (var option in All)
            {
                if (string.Equals(option, value.Trim(), StringComparison.OrdinalIgnoreCase))
                {
                    return option;
                }
            }

            return None;
        }
    }
}
