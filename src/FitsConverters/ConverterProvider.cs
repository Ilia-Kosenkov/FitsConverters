#nullable enable
using System;
using FitsConverters.Converters;
using ImageCore;


namespace FitsConverters
{
    public static class ConverterProvider
    {
        public static IFitsConverter<IImage<T>> GetImageConverter<T>()
            where T : unmanaged, IComparable<T>, IEquatable<T> => new ImageConverter<T>();

        public static object GetImageConverter(Type dataType)
        {
            _ = dataType ?? throw new ArgumentNullException(SR.NullArgument, nameof(dataType));

            if(!Image.IsTypeAllowed(dataType))
                throw new InvalidOperationException(string.Format(SR.TypeNotSupported, dataType));

            return Activator.CreateInstance(typeof(ImageConverter<>).MakeGenericType(dataType));
        }
    }
}
