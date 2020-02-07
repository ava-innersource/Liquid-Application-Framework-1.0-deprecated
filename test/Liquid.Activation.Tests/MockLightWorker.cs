// Copyright (c) Avanade Inc. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Xunit;

namespace Liquid.Activation.Tests
{
    [MessageBus("asd")]
    public class MockLightWorker : LightWorker
    {
        public static List<(MethodInfo MethodInfo, TopicAttribute TopicAttribute)> TopicList => _topics
            .Select(kvp => (kvp.Key, kvp.Value))
            .ToList();

        public static List<(MethodInfo MethodInfo, QueueAttribute QueueAttribute)> QueueList => _queues
            .Select(kvp => (kvp.Key, kvp.Value))
            .ToList();

        [Topic("name", "subscriptionName", 10, true)]
        public static void TopicMethod()
        {
        }

        [Queue("name")]
        public static void QueueMethod()
        {
        }
    }
}
