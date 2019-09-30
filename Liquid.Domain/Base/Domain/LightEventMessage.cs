using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace Liquid.Base.Domain
{
    public class LightEventMessage
    {
        public string MessageType { get; set; }

        public JToken PayLoad { get; set; }
    }
}
