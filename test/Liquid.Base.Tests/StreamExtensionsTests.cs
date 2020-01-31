// Copyright (c) Avanade Inc. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using AutoFixture.Xunit2;
using Xunit;

namespace Liquid.Base.Tests
{
    public class StreamExtensionsTests
    {
        [Theory, AutoData]
        public void AsStringCorrectlyReadsTheData(string data, Encoding encoding)
        {
            var buffer = encoding.GetBytes(data);
            using (var ms = new MemoryStream(buffer))
            {
                Assert.Equal(data, ms.AsString());
            }
        }

        [Theory, AutoData]
        public void AsStringThrowsWhenStreamIsNull(Encoding encoding)
        {
            Assert.Throws<ArgumentNullException>(() => StreamExtensions.AsString(null, encoding));
        }

        [Fact]
        public void AsStringThrowsWhenEncodingIsNull()
        {
            using (var ms = new MemoryStream(1))
            {
                Assert.Throws<ArgumentNullException>(() => StreamExtensions.AsString(ms, null));
            }
        }

        [Theory, AutoData]
        public async Task AsStringAsyncCorrectlyReadsTheDataAsync(string data, Encoding encoding)
        {
            var buffer = encoding.GetBytes(data);
            using (var ms = new MemoryStream(buffer))
            {
                Assert.Equal(data, await ms.AsStringAsync());
            }
        }

        [Fact]
        public void AsStringAsyncEargelyThrowsWhenStreamIsNull()
        {
            // xUnit demands that I use an async check, which is exacly what I don't do in order to have
            // eager checking, so I did the check manually
            try
            {
                _ = StreamExtensions.AsStringAsync(null, Encoding.UTF8);

                throw new Exception("The method didn't eargely threw.");
            }
            catch (Exception e)
            {
                Assert.IsType<ArgumentNullException>(e);
            }
        }

        [Fact]
        public void AsStringAsyncEargelyThrowsWhenEncodingIsNull()
        {
            using (var ms = new MemoryStream(100))
            {
                // xUnit demands that I use an async check, which is exacly what I don't do in order to have
                // eager checking, so I did the check manually
                try
                {
                    _ = StreamExtensions.AsStringAsync(ms, null);

                    throw new Exception("The method didn't eargely threw.");
                }
                catch (Exception e)
                {
                    Assert.IsType<ArgumentNullException>(e);
                }
            }
        }
    }
}
