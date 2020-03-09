using System.Collections.Generic;
using FitsCs;

namespace FitsConverters
{
    public interface IFitsProducer<in T>
    {
        Block ConvertToBlock(T input, IReadOnlyCollection<IFitsValue> extraKeys);
    }
}
