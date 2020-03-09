namespace FitsConverters
{
    public interface IFitsConverter<T> : IFitsConsumer<T>, IFitsProducer<T>
    {
    }
}
