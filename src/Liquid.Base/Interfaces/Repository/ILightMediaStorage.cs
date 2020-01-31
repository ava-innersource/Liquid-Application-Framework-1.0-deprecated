// Copyright (c) Avanade Inc. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading.Tasks;
using Liquid.Base.Interfaces;

namespace Liquid.Interfaces
{
    /// <summary>
    /// Defines an object capable of storing arbritary media.
    /// </summary>
    public interface ILightMediaStorage : IWorkbenchHealthCheck
    {
        /// <summary>
        /// Gets or sets the connection string for the storage provider.
        /// </summary>
        [Obsolete("This property will be removed in later version. Please refrain from accessing it.")]
        string Connection { get; set; }

        /// <summary>
        /// Gets or sets the container name used to store data in the provider.
        /// </summary>
        [Obsolete("This property will be removed in later version. Please refrain from accessing it.")]
        string Container { get; set; }

        /// <summary>
        /// Gets an <see cref="ILightAttachment"/> from storage.
        /// </summary>
        /// <param name="resourceId">The directory of the container.</param>
        /// <param name="id">The name of the media file.</param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains an object that encapsulates access to the data in the container entry.
        /// </returns>
        /// <exception cref="Exception">Throws when the resource doesn't exists.</exception>
        Task<ILightAttachment> GetAsync(string resourceId, string id);

        /// <summary>
        /// Upserts an <see cref="ILightAttachment"/> to the storage.
        /// </summary>
        /// <param name="attachment">Describes the data that will be stored/updated.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task InsertUpdateAsync(ILightAttachment attachment);

        /// <summary>
        /// Removes an item from the underlying storage.
        /// </summary>
        /// <param name="attachment">Describes the data that will be stored/updated.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task Remove(ILightAttachment attachment);
    }
}
