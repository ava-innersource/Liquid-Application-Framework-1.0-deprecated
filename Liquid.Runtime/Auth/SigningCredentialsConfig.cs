using Liquid.Runtime.Configuration;

namespace Liquid.Runtime
{
    public class SigningCredentialsConfig : LightConfig<SigningCredentialsConfig>
    {
        public string KeyId { get; set; }

        public string D { get; set; }
        public string DP { get; set; }
        public string DQ { get; set; }
        public string Exponent { get; set; }
        public string InverseQ { get; set; }
        public string Modulus { get; set; }
        public string P { get; set; }
        public string Q { get; set; }

        public override void Validate()
        {
        }
    }
}
