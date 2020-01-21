// Copyright (c) Avanade Inc. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.IO;

namespace Liquid.Interfaces
{
    public interface ILightAttachment : IDisposable
    {
        string Id { get; set; }
        string Name { get; set; }
        string ContentType { get; set; }
        string MediaLink { get; set; }
        Stream MediaStream { get; set; }

        /// <summary>
        /// Gets or sets the directory where the attachment will be stored into.
        /// </summary>
        string ResourceId { get; set; }
    }
}
