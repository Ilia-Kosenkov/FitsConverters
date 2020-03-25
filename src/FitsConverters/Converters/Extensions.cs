using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using FitsCs;

namespace FitsConverters.Converters
{
    internal static class Extensions
    {
        public static void CheckExtraFitsKeys(IReadOnlyCollection<IFitsValue> keys)
        {
            _ = keys ?? throw new ArgumentNullException(nameof(keys), SR.NullArgument);
            foreach (var key in keys)
            {
                switch (key.Name)
                {
                    case "SIMPLE":
                    case "BITPIX":
                    case "NAXIS":
                    case "PCOUNT":
                    case "GCOUNT":
                    case "END":
                    case { } s when s.StartsWith("NAXIS", StringComparison.OrdinalIgnoreCase):
                        throw new ArgumentException(string.Format(SR.IllegalKeysProvided, key.Name), nameof(keys));
                }
            }
        }

        public static ImmutableList<IFitsValue> GetExtraKeys(IReadOnlyCollection<IFitsValue> keys)
        {
            _ = keys ?? throw new ArgumentNullException(nameof(keys), SR.NullArgument);
            var result = ImmutableList.CreateBuilder<IFitsValue>();
            foreach (var key in keys)
            {
                switch (key.Name)
                {
                    case "SIMPLE":
                    case "BITPIX":
                    case "NAXIS":
                    case "PCOUNT":
                    case "GCOUNT":
                    case "END":
                    case { } s when s.StartsWith("NAXIS", StringComparison.OrdinalIgnoreCase):
                        continue;
                    default:
                        result.Add(key);
                        break;
                }
            }

            return result.ToImmutable();
        }
    }
}
