using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FitsCs;

namespace FitsConverters
{
    public interface IFitsProducer<in T>
    {
        Block ConvertToBlock(T input, IEnumerable<IFitsValue> extraKeys, bool isXtension = false);

        Block ConvertToBlock(T input, params IFitsValue[] extraKeys) =>
            ConvertToBlock(input, extraKeys, false);

        Block ConvertToBlock(T input, bool isXtension = false) => ConvertToBlock(input, Enumerable.Empty<IFitsValue>(), isXtension);

        ValueTask WriteToFitsAsync(T input, FitsWriter writer, IEnumerable<IFitsValue> extraKeys, bool isXtension = false, CancellationToken token = default);

        ValueTask WriteToFitsAsync(T input, FitsWriter writer, params IFitsValue[] extraKeys) =>
            WriteToFitsAsync(input, writer, extraKeys, false);

        ValueTask WriteToFitsAsync(T input, FitsWriter writer, bool isXtension = false, CancellationToken token = default) =>
            WriteToFitsAsync(input, writer, Enumerable.Empty<IFitsValue>(), isXtension, token);

    }
}
