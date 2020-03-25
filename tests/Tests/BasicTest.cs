using System;
using System.Buffers;
using FitsConverters;
using FitsCs;
using ImageCore;
using NUnit.Framework;

namespace Tests
{
    public class BasicTest
    {
        private IFitsConverter<IImage<float>> _converter;
        [SetUp]
        public void Setup()
        {
            _converter = ConverterProvider.GetImageConverter(typeof(float)) as IFitsConverter<IImage<float>>;
        }

        [Test]
        public void Test()
        {
            const int n = 248;
            const int m = 496;
            var data = ArrayPool<float>.Shared.Rent(n * m);
            
            try
            {
                for (var i = 0; i < n; i++)
                for (var j = 0; j < m; j++)
                    data[m * i + j] = (float)Math.Sqrt(
                        (i - 50) * (i - 50) * (1 + Math.Pow(Math.Sin(Math.PI * (i + 23) * 10 / n), 2))
                        + (j / 2.0 - 100) * (j / 2.0 - 100) * (1.1 + Math.Sin(Math.PI * j * 10 / m)));

                data[247 * 496 + 495] = 0f;
                var src = Image.Create<float>(data.AsSpan(..(n * m)), n, m);

                var key = FitsKey.Create("DEBUG", "Debug key", "With a comment");
                var block = _converter.ConvertToBlock(src, key);

                var rec = _converter.ConvertFromBlock(block, out var keys);

                Assert.AreEqual(src, rec);
                Assert.AreEqual(data[247 * 496 + 495], src[247, 495]);
                Assert.AreEqual(data[247 * 496 + 495], rec[247, 495]);
                Assert.That(keys, Has.One.Items);
                Assert.That(keys, Contains.Item(key));

            }
            finally
            {
                ArrayPool<float>.Shared.Return(data);
            }
        }
    }
}