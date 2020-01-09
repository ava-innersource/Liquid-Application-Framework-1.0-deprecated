// Copyright (c) Avanade Inc. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.AutoNSubstitute;
using Liquid.Interfaces;
using Liquid.Repository;
using Liquid.Tests;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using NSubstitute;
using Xunit;

namespace Liquid.OnAzure.Tests
{
    public class AzureBlobTests : IDisposable
    {
        private const string ContentType = "text/plain";

        private static readonly IFixture _fixture = new Fixture().Customize(new AutoNSubstituteCustomization());

        private static readonly ILightRepository _fakeLightRepository = Substitute.For<ILightRepository>();

        private readonly string _expectedData;

        private readonly Stream _stream;

        private readonly ILightAttachment _lightAttachment;

        private readonly AzureBlob _sut;

        public AzureBlobTests()
        {
            Workbench.Instance.Reset();

            Workbench.Instance.AddToCache(WorkbenchServiceType.Repository, _fakeLightRepository);

            _sut = new AzureBlob(new MediaStorageConfiguration
            {
                ConnectionString = "UseDevelopmentStorage=true",
                Container = "testcontainer", // _fixture.Create<string>().ToLower(),
            });

            _expectedData = _fixture.Create<string>();

            _stream = ToMemoryStream(_expectedData);

            _lightAttachment = new LightAttachment
            {
                ContentType = ContentType,
                Id = _fixture.Create<string>() + ".txt",
                MediaLink = _fixture.Create<string>(),
                MediaStream = _stream,
                Name = _fixture.Create<string>(),
                ResourceId = _fixture.Create<string>(),
            };
        }

        [Fact]
        public void WhenConfigurationIsNullCtorThrows()
        {
            Assert.ThrowsAny<Exception>(() => new AzureBlob(null));
        }

        [Fact]
        public Task WhenAttachmentIsNullInsertThrows()
        {
            return Assert.ThrowsAnyAsync<ArgumentNullException>(() => _sut.InsertUpdateAsync(null));
        }

        [Fact]
        public async Task WhenAttachmentDoesntExistsGetThrows()
        {
            // TODO: Improve exception
            await Assert.ThrowsAnyAsync<StorageException>(async () => await _sut.GetAsync(_lightAttachment.ResourceId, _lightAttachment.Id));
        }

        [Fact]
        public async Task CanInsertAndGetAttachmentAsync()
        {
            await _sut.InsertUpdateAsync(_lightAttachment);

            var attachment = await _sut.GetAsync(_lightAttachment.ResourceId, _lightAttachment.Id);
            var actual = attachment.MediaStream.AsString();

            Assert.Equal(_expectedData, actual);
        }

        [Fact]
        public async Task RemoveThrowsIfAttachmentIsNullAsync()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await _sut.Remove(null));
        }

        [Fact]
        public async Task WhenResourceRemovedCantGet()
        {
            await _sut.InsertUpdateAsync(_lightAttachment);

            var attachment = await _sut.GetAsync(_lightAttachment.ResourceId, _lightAttachment.Id);
            var actual = attachment.MediaStream.AsString();

            Assert.Equal(_expectedData, actual);

            await _sut.Remove(_lightAttachment);

            await Assert.ThrowsAnyAsync<StorageException>(() => _sut.GetAsync(_lightAttachment.ResourceId, _lightAttachment.Id));
        }

        [Theory]
        [AutoSubstituteData]
        public async Task CreatesContainerOnInitializationWithDesiredAccessLevelAsync(BlobContainerPublicAccessType accessType)
        {
            // ARRANGE
            const string containerName = "removecontainer";
            var connectionString = "UseDevelopmentStorage=true";

            var client = CloudStorageAccount.Parse(connectionString).CreateCloudBlobClient();
            var container = client.GetContainerReference(containerName);

            await container.DeleteIfExistsAsync();

            var configuration = new MediaStorageConfiguration
            {
                ConnectionString = connectionString,
                Container = containerName,
                Permission = accessType.ToString(),
            };

            // ACT
            _ = new AzureBlob(configuration);

            // ASSERT
            container = client.GetContainerReference(containerName);

            Assert.True(await container.ExistsAsync());

            var blobContainerPermissions = await container.GetPermissionsAsync();

            Assert.Equal(accessType, blobContainerPermissions.PublicAccess);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool isDisposing)
        {
            if (isDisposing)
            {
                _stream?.Dispose();
            }
        }

        private static Stream ToMemoryStream(string data)
        {
            return new MemoryStream(Encoding.UTF8.GetBytes(data));
        }
    }
}
