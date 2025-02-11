﻿namespace NServiceBus.Core.Tests.DataBus.FileShare
{
    using System;
    using System.IO;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using NUnit.Framework;

    [TestFixture]
    public class AcceptanceTests
    {
        [SetUp]
        public void SetUp()
        {
            dataBus = new FileShareDataBusImplementation(basePath) { MaxMessageTimeToLive = TimeSpan.MaxValue };
        }

        FileShareDataBusImplementation dataBus;
        string basePath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

        async Task<string> Put(string content, TimeSpan timeToLive, CancellationToken cancellationToken = default)
        {
            var byteArray = Encoding.ASCII.GetBytes(content);
            using (var stream = new MemoryStream(byteArray))
            {
                return await dataBus.Put(stream, timeToLive, cancellationToken);
            }
        }

        [Test]
        public async Task Should_handle_be_able_to_read_stored_values()
        {
            const string content = "Test";

            var key = await Put(content, TimeSpan.MaxValue);
            using (var stream = await dataBus.Get(key))
            using (var streamReader = new StreamReader(stream))
            {
                Assert.AreEqual(await streamReader.ReadToEndAsync(), content);
            }
        }

        [Test]
        public async Task Should_handle_be_able_to_read_stored_values_concurrently()
        {
            const string content = "Test";

            var key = await Put(content, TimeSpan.MaxValue);

            Parallel.For(0, 10, async i =>
            {
                using (var stream = await dataBus.Get(key))
                using (var streamReader = new StreamReader(stream))
                {
                    Assert.AreEqual(await streamReader.ReadToEndAsync(), content);
                }
            });
        }

        [Test]
        public async Task Should_handle_max_ttl()
        {
            await Put("Test", TimeSpan.MaxValue);
            Assert.True(Directory.Exists(Path.Combine(basePath, DateTime.MaxValue.ToString("yyyy-MM-dd_HH"))));
        }

        [Test]
        public async Task Should_honor_the_ttl_limit()
        {
            dataBus.MaxMessageTimeToLive = TimeSpan.FromDays(1);

            await Put("Test", TimeSpan.MaxValue);
            Assert.True(Directory.Exists(Path.Combine(basePath, DateTime.UtcNow.AddDays(1).ToString("yyyy-MM-dd_HH"))));
        }
    }
}