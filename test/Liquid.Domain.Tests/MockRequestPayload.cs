// Copyright (c) Avanade Inc. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Liquid.Domain.Tests
{
    /// <summary>
    /// Used by <see cref="LightApiTests"/>.
    /// </summary>
    public class MockRequestPayload
    {
        public int Message { get; set; }

        public override bool Equals(object other)
        {
            if (other == null)
            {
                return false;
            }

            if (!(other is MockRequestPayload otherRequest))
            {
                return false;
            }

            return Message == otherRequest.Message;
        }

        public override int GetHashCode()
        {
            return Message.GetHashCode();
        }
    }
}
