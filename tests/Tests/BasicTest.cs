#nullable enable
using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using FitsConverters;
using FitsCs;
using ImageCore;
using NUnit.Framework;

namespace Tests
{
    public class BasicTest
    {
        private Random _r;
        [SetUp]
        public void Setup()
        {
            _r = new Random(42);
        }

        [Test]
        [GenericTestCase(null!, TypeArguments = new[] { typeof(byte) })]
        [GenericTestCase(null!, TypeArguments = new[] { typeof(short) })]
        [GenericTestCase(null!, TypeArguments = new[] { typeof(int) })]
        [GenericTestCase(null!, TypeArguments = new[] { typeof(long) })]
        [GenericTestCase(null!, TypeArguments = new[] { typeof(float) })]
        [GenericTestCase(null!, TypeArguments = new[] { typeof(double) })]
        public void TestThroughBlocks<T>(object? __ = null) where T : unmanaged, IComparable<T>, IEquatable<T>
        {
            const int n = 248;
            const int m = 496;
            var data = ArrayPool<T>.Shared.Rent(n * m);
            var converter = ConverterProvider.GetImageConverter<T>();
            try
            {
                for (var i = 0; i < n; i++)
                for (var j = 0; j < m; j++)
                    data[m * i + j] = Internal.UnsafeNumerics.MathOps.DangerousCast<double, T>(Math.Sqrt(
                        (i - 50) * (i - 50) * (1 + Math.Pow(Math.Sin(Math.PI * (i + 23) * 10 / n), 2))
                        + (j / 2.0 - 100) * (j / 2.0 - 100) * (1.1 + Math.Sin(Math.PI * j * 10 / m))));

                data[247 * 496 + 495] = default;
                var src = Image.Create<T>(data.AsSpan(..(n * m)), n, m);

                var key = FitsKey.Create("DEBUG", "Debug key", "With a comment");
                var block = converter.ConvertToBlock(src, key);

                var rec = converter.ConvertFromBlock(block, out var keys);

                Assert.AreEqual(src, rec);
                Assert.AreEqual(data[247 * 496 + 495], src[247, 495]);
                Assert.AreEqual(data[247 * 496 + 495], rec[247, 495]);
                Assert.That(keys, Has.One.Items);
                Assert.That(keys, Contains.Item(key));

            }
            finally
            {
                ArrayPool<T>.Shared.Return(data);
            }
        }

        [Test]
        [GenericTestCase(null!, TypeArguments = new[] { typeof(byte) })]
        [GenericTestCase(null!, TypeArguments = new[] { typeof(short) })]
        [GenericTestCase(null!, TypeArguments = new[] { typeof(int) })]
        [GenericTestCase(null!, TypeArguments = new[] { typeof(long) })]
        [GenericTestCase(null!, TypeArguments = new[] { typeof(float) })]
        [GenericTestCase(null!, TypeArguments = new[] { typeof(double) })]
        public async Task ReadWriteTest<T>(object? __ = null) where T : unmanaged, IComparable<T>, IEquatable<T>
        {
            const int n = 248;
            const int m = 496;
            var converter = ConverterProvider.GetImageConverter<T>();

            var data = ArrayPool<T>.Shared.Rent(n * m);
            try
            {
                for (var i = 0; i < n * m; i++)
                    data[i] = Internal.UnsafeNumerics.MathOps.DangerousCast<double, T>(_r.NextDouble() * i);
                var src = Image.Create<T>(data.AsSpan(0, n * m), n, m);


                await using var fs = new FileStream($"multiple_records_{typeof(T).Name}.fits", FileMode.Create, FileAccess.ReadWrite);
                await using (var writer = new FitsWriter(fs))
                {
                    await converter.WriteToFitsAsync(src, writer);
                    await converter.WriteToFitsAsync(src, writer, true);
                }
                
                fs.Seek(0, SeekOrigin.Begin);
                var images = new List<IImage<T>>(2);
                await using (var reader = new FitsReader(fs))
                {
                    await foreach (var block in reader.EnumerateBlocksAsync())
                    {
                        images.Add(converter.ConvertFromBlock(block, out _));
                    }
                }

                Assert.That(images, Has.Count.EqualTo(2));
                Assert.AreEqual(src, images[0]);
                Assert.AreEqual(src, images[1]);
            }
            finally
            {
                ArrayPool<T>.Shared.Return(data);
            }
        }
    }
}