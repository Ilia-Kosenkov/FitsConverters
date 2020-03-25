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
        Block ConvertToBlock(T input, IEnumerable<IFitsValue> extraKeys);

        Block ConvertToBlock(T input, params IFitsValue[] extraKeys) =>
            ConvertToBlock(input, extraKeys as IEnumerable<IFitsValue>);

        Block ConvertToBlock(T input) => ConvertToBlock(input, Array.Empty<IFitsValue>());


        ValueTask WriteToFitsAsync(T input, FitsWriter writer, IEnumerable<IFitsValue> extraKeys, CancellationToken token = default);

        ValueTask WriteToFitsAsync(T input, FitsWriter writer, params IFitsValue[] extraKeys) =>
            WriteToFitsAsync(input, writer, extraKeys, default);

        ValueTask WriteToFitsAsync(T input, FitsWriter writer, CancellationToken token = default) =>
            WriteToFitsAsync(input, writer, Enumerable.Empty<IFitsValue>(), token);

    }
}
