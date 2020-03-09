using System;
using System.Collections.Generic;
using FitsCs;
using ImageCore;

namespace FitsConverters.Converters
{
    internal class ImageConverter<T> : IFitsConverter<IImage<T>>
        where T : unmanaged, IComparable<T>, IEquatable<T>
    {
        public Block ConvertToBlock(IImage<T> input, IReadOnlyCollection<IFitsValue> extraKeys)
        {
            AllowedTypes.ValidateDataType<T>();
            // Throws if mandatory keys are present
            Extensions.CheckExtraFitsKeys(extraKeys);

            var bitPix = FitsCs.Extensions.ConvertTypeToBitPix<T>()
                         ?? throw new InvalidOperationException(
                             $"Failed to calculate BitPix value: type {typeof(T)} is unsupported.");
            
            var desc = new Descriptor(
                bitPix, extraKeys.Count,
                new [] {input.Width, input.Height});

            var block = Block<T>.CreateWithData(desc, extraKeys, x =>
            {
                input.GetView().CopyTo(x);
            });

            return block;
        }
    }
}
