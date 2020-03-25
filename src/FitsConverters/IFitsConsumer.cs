using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using FitsCs;

namespace FitsConverters
{
    public interface IFitsConsumer<T>
    {
        T ConvertFromBlock(Block input, out ImmutableList<IFitsValue> extraKeys);

        ValueTask<(T Value, ImmutableList<IFitsValue> ExtraKeys)> ReadFromFitsAsync(FitsReader reader, CancellationToken token = default);
    }
}
