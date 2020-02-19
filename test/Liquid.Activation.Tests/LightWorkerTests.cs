// Copyright (c) Avanade Inc. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Threading.Tasks;
using AutoFixture;
using Liquid.Base;
using Liquid.Interfaces;
using Newtonsoft.Json;
using NSubstitute;
using Xunit;

namespace Liquid.Activation.Tests
{
    public class LightWorkerTests
    {
        private readonly Fixture _fixture = new Fixture();

        public LightWorkerTests()
        {
            Workbench.Instance.Reset();
            Workbench.Instance.AddToCache(WorkbenchServiceType.Telemetry, Substitute.For<ILightTelemetry>());
        }

        [Fact]
        public void InitializeWhenMockLightWorkerPresentThenQueueAndTopicsAreDiscovered()
        {
            var sut = new MockLightWorker();
            sut.Initialize();

            Assert.Contains(
                MockLightWorker.TopicList,
                _ => _.MethodInfo.ReflectedType == typeof(MockLightWorker)
                && _.MethodInfo.Name == nameof(MockLightWorker.TopicMethod));

            Assert.Contains(
                MockLightWorker.QueueList,
                _ => _.MethodInfo.ReflectedType == typeof(MockLightWorker)
                && _.MethodInfo.Name == nameof(MockLightWorker.QueueMethod));

            // Given the static nature of LightWorker, we couldn't make this an isolated assertion
            // TODO: Refactor LightWorker and then make this isolated
            Assert.Throws<LightException>(() => new MockLightWorker().Initialize());
        }

        [Theory]
        [InlineData("anything")]
        [InlineData(null)]
        [InlineData(1)]
        public void InvokeProcessWhenMethodIsNullReturnsExpectedValue(object message)
        {
            MethodsCollection.Value = _fixture.Create<string>();

            var method = typeof(MethodsCollection).GetMethod(nameof(MethodsCollection.ConstantMethod));

            var actual = LightWorker.InvokeProcess(method, ToJsonByteStream(message));

            Assert.Equal(MethodsCollection.Value, actual);
        }

        [Fact]
        public void InvokeProcessWhenMethodIsEchoMessageIsEmptyReturnsNull()
        {
            var method = typeof(MethodsCollection).GetMethod(nameof(MethodsCollection.EchoMethod));

            var actual = LightWorker.InvokeProcess(method, Array.Empty<byte>());

            Assert.Null(actual);
        }

        [Fact]
        public void InvokeProcessWhenMethodHasOneParametersAndMessageIsntValidJsonThrows()
        {
            var method = typeof(MethodsCollection).GetMethod(nameof(MethodsCollection.EchoMethod));

            var message = ToJsonByteStream("anything");

            message[0] = _fixture.Create<byte>();

            Assert.ThrowsAny<Exception>(() => LightWorker.InvokeProcess(method, message));
        }

        [Fact]
        public void InvokeProcessWhenMethodHasZeroParametersAndMessageIsntValidReturnsExpectedValue()
        {
            var method = typeof(MethodsCollection).GetMethod(nameof(MethodsCollection.ConstantMethod));

            var message = ToJsonByteStream("anything");

            message[0] = _fixture.Create<byte>();

            MethodsCollection.Value = _fixture.Create<string>();

            var actual = LightWorker.InvokeProcess(method, message);

            Assert.Equal(MethodsCollection.Value, actual);
        }

        [Fact]
        public void InvokeProcessWhenMessageIsValidJsonParsesItCorrectly()
        {
            // ARRANGE
            var anonymous = new Foobar { Foo = "Bar" };
            var anonymousAsByteStream = ToJsonByteStream(anonymous);

            var method = typeof(MethodsCollection).GetMethod(nameof(MethodsCollection.EchoMethod));

            // ACT
            var actual = (Foobar)LightWorker.InvokeProcess(method, anonymousAsByteStream);

            // ASSERT
            Assert.Equal(anonymous.Foo, actual.Foo);
        }

        [Fact]
        public void InvokeProcessWhenMethodHasZeroParametersDoesntParseMessage()
        {
            // ARRANGE
            var mi = typeof(MethodsCollection).GetMethod(nameof(MethodsCollection.ConstantMethod));

            // ACT
            var actual = (string)LightWorker.InvokeProcess(mi, null);

            // ASSERT
            Assert.Equal(MethodsCollection.Value, actual);
        }

        [Fact]
        public void InvokeProcessWhenMethodThrowsAsyncThrows()
        {
            // ARRANGE
            var mi = typeof(MethodsCollection).GetMethod(nameof(MethodsCollection.ThrowsAsync));

            var anonymous = new Foobar { Foo = "Bar" };
            var anonymousAsByteStream = ToJsonByteStream(anonymous);

            // ACT & ASSERT
            Assert.ThrowsAsync<MethodsCollection.TestException>(() => (Task)LightWorker.InvokeProcess(mi, anonymousAsByteStream));
        }

        /// <summary>
        /// Serialize any object to a JSON string and then convert it to a bytestream.
        /// </summary>
        /// <param name="obj">The object to serialize.</param>
        /// <returns>A byestream containing the object as UTF8 bytes.</returns>
        private byte[] ToJsonByteStream(object obj)
        {
            var anonymousAsString = JsonConvert.SerializeObject(obj);
            var anonymousAsByteStream = Encoding.UTF8.GetBytes(anonymousAsString);

            return anonymousAsByteStream;
        }

        [SuppressMessage(
           "Design",
           "CA1034:Nested types should not be visible",
           Justification = "Must be public so LightWorker access the class")]
        public class Foobar
        {
            public string Foo { get; set; } = "Bar";
        }

        private class MethodsCollection
        {
            public static string Value { get; set; } = "string";

            public string ConstantMethod()
            {
                return Value;
            }

            public Foobar EchoMethod(Foobar foobar)
            {
                return foobar;
            }

            public Task ThrowsAsync(Foobar foobar)
            {
                return Task.FromException(new TestException(string.Empty));
            }

            // Used to test throwing from a method
            public class TestException : Exception
            {
                public TestException(string message)
                    : base(message)
                {
                }

                public TestException(string message, Exception innerException)
                    : base(message, innerException)
                {
                }

                public TestException()
                {
                }
            }
        }
    }
}
