using System;
using System.Collections.Generic;
using FitsCs;

namespace FitsConverters.Converters
{
    internal static class Extensions
    {
        public static void CheckExtraFitsKeys(IReadOnlyCollection<IFitsValue> keys)
        {
            foreach (var key in keys)
            {
                switch (key.Name)
                {
                    case "SIMPLE":
                    case "BITPIX":
                    case "NAXIS":
                    case "PCOUNT":
                    case "GCOUNT":
                    case string s when s.StartsWith("NAXIS", StringComparison.OrdinalIgnoreCase)
                        throw new ArgumentException($"Extra FITS keys cannot redefine automatically generated mandatory key, such as {key.Name}.", nameof(keys));
                }
            }
        }
    }
}
