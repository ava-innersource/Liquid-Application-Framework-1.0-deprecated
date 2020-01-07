// Copyright (c) Avanade Inc. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Liquid
{
    /// <summary>
    /// List of HTTP Methods.
    /// </summary>
    public enum WorkBenchServiceHttp
    {
        /// <summary>
        /// Represents a HTTP protocol GET method. Commonly used to obtain informations regarding an entity.
        /// </summary>
        GET,

        /// <summary>
        /// Represents a HTTP protocol POST method. Commonly used to create a new entity.
        /// </summary>
        POST,

        /// <summary>
        /// Represents a HTTP protocol PUT method. Commonly used to update an existing entity.
        /// </summary>
        PUT,

        /// <summary>
        /// Represents a HTTP protocol OPTIONS method. Commonly used to find out what methods the entity accepts.
        /// </summary>
        OPTIONS,

        /// <summary>
        /// Represents a HTTP protocol DELETE method. Commonly used to delete an existing entity.
        /// </summary>
        DELETE,

        /// <summary>
        /// Represents a HTTP protocol HEAD method. Commonly used to check whether an entity exists without obtaining the actual entity data.
        /// </summary>
        HEAD,

        /// <summary>
        /// Represents an HTTP protocol TRACE method.
        /// </summary>
        TRACE,

        /// <summary>
        /// Represents an HTTP protocol CONNECT method.
        /// </summary>
        CONNECT,

        /// <summary>
        /// Represents an HTTP protocol PATH method.
        /// </summary>
        PATH,
    }
}
