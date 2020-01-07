// Copyright (c) Avanade Inc. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;

namespace Liquid
{
    /// <summary>
    /// Types of services that the <see cref="Workbench"/> manages.
    /// </summary>
    public enum WorkbenchServiceType
    {
        /// <summary>
        /// Service used to persist objects.
        /// </summary>
        Repository,

        /// <summary>
        /// Service used to record events and general data in telemetry.
        /// </summary>
        Telemetry,

        /// <summary>
        /// Service used to process messages.
        /// </summary>
        Worker,

        /// <summary>
        /// Service used to store media attachments.
        /// </summary>
        MediaStorage,

        /// <summary>
        /// Service used to cache data.
        /// </summary>
        Cache,

        /// <summary>
        /// Service used to handle events (messages).
        /// </summary>
        EventHandler,

        /// <summary>
        /// Service used to perform logging.
        /// </summary>
        Logger,
    }

    /// <summary>
    /// Types of services that the <see cref="Workbench"/> manages.
    /// </summary>
    [Obsolete("Prefer the correct spelled enum Liquid.Workbench")]
    public enum WorkBenchServiceType
    {
        /// <summary>
        /// Service used to persist objects.
        /// </summary>
        Repository = WorkbenchServiceType.Repository,

        /// <summary>
        /// Service used to record events and general data in telemetry.
        /// </summary>
        Telemetry = WorkbenchServiceType.Telemetry,

        /// <summary>
        /// Service used to process messages.
        /// </summary>
        Worker = WorkbenchServiceType.Worker,

        /// <summary>
        /// Service used to store media attachments.
        /// </summary>
        MediaStorage = WorkbenchServiceType.MediaStorage,

        /// <summary>
        /// Service used to cache data.
        /// </summary>
        Cache = WorkbenchServiceType.Cache,

        /// <summary>
        /// Service used to handle events (messages).
        /// </summary>
        EventHandler = WorkbenchServiceType.EventHandler,

        /// <summary>
        /// Service used to perform logging.
        /// </summary>
        Logger = WorkbenchServiceType.Logger,
    }
}
