// Copyright (c) Avanade Inc. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Linq;
using System.Text;
using Liquid.Base.Domain;
using Liquid.Domain.API;
using Liquid.Tests;
using Newtonsoft.Json;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;
using Xunit;

namespace Liquid.Domain.Tests
{
    public class LightApiTests
    {
        private readonly FluentMockServer _server = FluentMockServer.Start();

        private readonly LightApi _sut;

        public LightApiTests()
        {
            _sut = new LightApi(_server.Urls.First(), -1, string.Empty, string.Empty, null);
        }

        [Theory, AutoSubstituteData]
        public void GetOfTReturnsExpectedData(string path, MockResponsePayload payload)
        {
            var fullpath = $"/{path}";

            var body = ToDomainResponse(payload);

            _server
                .Given(
                    Request.Create().WithPath(fullpath).UsingGet())
                .RespondWith(
                    Response.Create().WithStatusCode(200).WithBodyAsJson(body, true));

            var result = _sut.Get<MockResponsePayload>(fullpath);

            Assert.Equal(payload?.Data, result.Data);
        }

        [Theory, AutoSubstituteData]
        public void GetReturnsExpectedData(string path, MockResponsePayload payload)
        {
            var fullpath = $"/{path}";

            var body = ToDomainResponse(payload);
            _server
                .Given(
                    Request.Create().WithPath(fullpath).UsingGet())
                .RespondWith(
                    Response.Create().WithStatusCode(200).WithBodyAsJson(body, true));

            var result = _sut.Get(fullpath);

            Assert.Equal(payload?.Data, result.SelectToken(".data"));
        }

        [Theory, AutoSubstituteData]
        public void PostOfTReturnsExpectedData(string path, MockRequestPayload requestPayload, MockResponsePayload responsePayload)
        {
            var fullpath = $"/{path}";

            var response = ToDomainResponse(responsePayload);

            _server
                .Given(
                    Request.Create().WithPath(fullpath).UsingPost().WithBody(EqualsMatcher(requestPayload)))
                .RespondWith(
                    Response.Create().WithStatusCode(200).WithBodyAsJson(response, true));

            var result = _sut.Post<MockResponsePayload>(fullpath, requestPayload.ToJsonCamelCase());

            Assert.Equal(responsePayload?.Data, result.Data);
        }

        [Theory, AutoSubstituteData]
        public void PostReturnsExpectedData(string path, MockRequestPayload requestPayload, MockResponsePayload payload)
        {
            var fullpath = $"/{path}";

            var body = ToDomainResponse(payload);

            _server
                .Given(
                    Request.Create().WithPath(fullpath).UsingPost().WithBody(EqualsMatcher(requestPayload)))
                .RespondWith(
                    Response.Create().WithStatusCode(200).WithBodyAsJson(body, true));

            var result = _sut.Post(fullpath, requestPayload.ToJsonCamelCase());

            Assert.Equal(payload?.Data, result.SelectToken(".data"));
        }

        [Theory, AutoSubstituteData]
        public void PutOfTReturnsExpectedData(string path, MockRequestPayload requestPayload, MockResponsePayload payload)
        {
            var fullpath = $"/{path}";

            var body = ToDomainResponse(payload);

            _server
                .Given(
                    Request.Create().WithPath(fullpath).UsingPut().WithBody(EqualsMatcher(requestPayload)))
                .RespondWith(
                    Response.Create().WithStatusCode(200).WithBodyAsJson(body, true));

            var result = _sut.Put<MockResponsePayload>(fullpath, requestPayload.ToJsonCamelCase());

            Assert.Equal(payload?.Data, result.Data);
        }

        [Theory, AutoSubstituteData]
        public void PutReturnsExpectedData(string path, MockRequestPayload requestPayload, MockResponsePayload payload)
        {
            var fullpath = $"/{path}";

            var body = ToDomainResponse(payload);

            _server
                .Given(
                    Request.Create().WithPath(fullpath).UsingPut().WithBody(EqualsMatcher(requestPayload)))
                .RespondWith(
                    Response.Create().WithStatusCode(200).WithBodyAsJson(body, true));

            var result = _sut.Put(fullpath, requestPayload.ToJsonCamelCase());

            Assert.Equal(payload?.Data, result.SelectToken(".data"));
        }

        [Theory, AutoSubstituteData]
        public void DeleteReturnsExpectedData(string path, MockResponsePayload payload)
        {
            var fullpath = $"/{path}";

            var body = ToDomainResponse(payload);

            _server
                .Given(
                    Request.Create().WithPath(fullpath).UsingDelete())
                .RespondWith(
                    Response.Create().WithStatusCode(200).WithBodyAsJson(body, true));

            var result = _sut.Delete(fullpath);

            // BUG: It's ahn... Well... Yeah, there's a DomainResponse inside a DomainResponse
            var payloadAsString = result.PayLoad.ToString();

            Assert.Equal(payload?.Data, JsonConvert.DeserializeObject<DomainResponse>(payloadAsString).PayLoad.SelectToken(".data"));
        }

        /// <summary>
        /// Creates a function that checks whether the serialized object is equivalent to
        /// the object <paramref name="expectedObject"/>.
        /// </summary>
        /// <param name="expectedObject">What the object was expected to be.</param>
        /// <returns>The matching function.</returns>
        /// <remarks>
        /// This function relies on the <see cref="object.Equals(object)"/> override of
        /// <paramref name="expectedObject"/>.
        /// </remarks>
        private static Func<byte[], bool> EqualsMatcher<T>(T expectedObject)
        {
            return actualBytes =>
            {
                var objAsStr = Encoding.UTF8.GetString(actualBytes);
                var received = JsonConvert.DeserializeObject<T>(objAsStr);

                return expectedObject.Equals(received);
            };
        }

        private DomainResponse ToDomainResponse(MockResponsePayload payload)
        {
            return new DomainResponse
            {
                PayLoad = payload.ToJsonCamelCase(),
            };
        }
    }
}
