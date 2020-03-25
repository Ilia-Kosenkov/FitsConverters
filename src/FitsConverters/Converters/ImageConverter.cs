#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using FitsCs;
using ImageCore;

namespace FitsConverters.Converters
{
    internal sealed class ImageConverter<T> : IFitsConverter<IImage<T>>
        where T : unmanaged, IComparable<T>, IEquatable<T>
    {
        public Block ConvertToBlock(IImage<T> input, IEnumerable<IFitsValue> extraKeys)
        {
            if(input is null)
                throw new ArgumentNullException(nameof(input), SR.NullArgument);
            
            AllowedTypes.ValidateDataType<T>();

            var extraKeysInst = (extraKeys ?? Enumerable.Empty<IFitsValue>()).ToList();

            // Throws if mandatory keys are present
            Extensions.CheckExtraFitsKeys(extraKeysInst);

            var bitPix = FitsCs.Extensions.ConvertTypeToBitPix<T>()
                         ?? throw new InvalidOperationException(string.Format(SR.TypeNotSupported, typeof(T)));

            var keys = new List<IFitsValue>(extraKeysInst.Count + 10)
            {
                FitsKey.Create(@"SIMPLE", true),
                FitsKey.Create(@"BITPIX", (int)bitPix),
                FitsKey.Create(@"NAXIS", 2),
                FitsKey.Create(@"NAXIS1", input.Height),
                FitsKey.Create(@"NAXIS2", input.Width)
            };
            keys.AddRange(extraKeysInst);
            keys.Add(FitsKey.CreateEnd());


            var desc = new Descriptor(
                bitPix, keys.Count,
                new [] {input.Height, input.Width});

            var block = Block<T>.CreateWithData(desc, keys, x =>
            {
                input.GetView().CopyTo(x);
            });

            return block;
        }

        public ValueTask WriteToFitsAsync(IImage<T> input, FitsWriter writer, IEnumerable<IFitsValue> extraKeys, CancellationToken token = default)
        {

            if (input is null)
                throw new ArgumentNullException(nameof(input), SR.NullArgument);

            if(writer is null)
                throw new ArgumentNullException(nameof(writer), SR.NullArgument);

            AllowedTypes.ValidateDataType<T>();

            var extraKeysInst = (extraKeys ?? Enumerable.Empty<IFitsValue>()).ToList();

            // Throws if mandatory keys are present
            Extensions.CheckExtraFitsKeys(extraKeysInst);

            var bitPix = FitsCs.Extensions.ConvertTypeToBitPix<T>()
                         ?? throw new InvalidOperationException(string.Format(SR.TypeNotSupported, typeof(T)));

            var keys = new List<IFitsValue>(extraKeysInst.Count + 10)
            {
                FitsKey.Create(@"SIMPLE", true),
                FitsKey.Create(@"BITPIX", (int)bitPix),
                FitsKey.Create(@"NAXIS", 2),
                FitsKey.Create(@"NAXIS1", input.Width),
                FitsKey.Create(@"NAXIS2", input.Height)
            };
            keys.AddRange(extraKeysInst);
            keys.Add(FitsKey.CreateEnd());


            var desc = new Descriptor(
                bitPix, keys.Count,
                new[] { input.Width, input.Height });

            var block = Block<T>.CreateWithData(desc, keys, x =>
            {
                input
                    .GetView().CopyTo(x);
            });

            return writer.WriteBlockAsync(block, token);
        }

        public IImage<T> ConvertFromBlock(Block block, out ImmutableList<IFitsValue> extraKeys)
        {
            if (block.Descriptor.Dimensions.Length != 2)
                throw new ArgumentException(SR.FitsDimensionMismatch, nameof(block));

            var (height, width) = (block.Descriptor.Dimensions[0], block.Descriptor.Dimensions[1]);

            extraKeys = Extensions.GetExtraKeys(block.Keys);

            return Image.CreateRaw<T>(span => block.RawData.Slice(0, height * width * block.Descriptor.ItemSizeInBytes).CopyTo(span), height, width);
        }

        public async ValueTask<(IImage<T> Value, ImmutableList<IFitsValue> ExtraKeys)> ReadFromFitsAsync(
            FitsReader reader, CancellationToken token = default) 
        => (ConvertFromBlock(
                await (reader ?? throw new ArgumentNullException(SR.NullArgument, nameof(reader)))
                    .ReadBlockAsync(token) ?? throw new IOException(SR.ReadFailed),
                out var keys),
            keys);
    }
}
