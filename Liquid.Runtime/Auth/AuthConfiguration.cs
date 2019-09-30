using Liquid.Runtime.Configuration;
using System.Collections.Generic;
using System.Linq;

namespace Liquid.Runtime
{
    public class AuthConfiguration : LightConfig<AuthConfiguration>
    {
        public string Authority { get; set; }
        public string Audiencies { get; set; }
        public bool RequireHttpsMetadata { get; set; }
        public IEnumerable<string> GetAudiences() => !string.IsNullOrEmpty(Audiencies) ? Audiencies.Split(',') : Enumerable.Empty<string>();

        public string SecurityKey { get; set; }

        public override void Validate()
        {
        }
    }
}
