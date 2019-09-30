using Liquid.Runtime.Configuration; 

namespace Liquid.Runtime.Polly
{
    public class PollyConfiguration : LightConfig<PollyConfiguration>
    {
        public bool IsBackOff { get; set; }
        public int Retry { get; set; }
        public int Wait { get; set; }

        public override void Validate()
        {

        }
    }
}
