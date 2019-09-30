using Liquid.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Liquid.Repository
{
    public abstract class LightOptimisticModel<T> : LightModel<T> where T : LightModel<T>, ILightModel,new()
    {
        protected LightOptimisticModel(): base() {}
        public override abstract void Validate();
        [JsonProperty("_etag")]
        public string ETag { get; set; }

    }
}
