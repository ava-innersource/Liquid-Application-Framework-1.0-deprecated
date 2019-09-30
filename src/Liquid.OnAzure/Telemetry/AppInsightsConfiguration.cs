using FluentValidation;
using Liquid.Runtime.Configuration;

namespace Liquid.OnAzure
{
    /// <summary>
    /// AppInsights Configurator will dynamically assign the authorization key for writing to AppInsights.
    /// The function caller should pass a configuration file containing the Azure key.
    /// </summary>
    public class AppInsightsConfiguration : LightConfig<AppInsightsConfiguration>
    {
        //Necessary key for sends data to telemetry. Otherwise no data will tracked.
        public string InstrumentationKey { get; set; }
        public bool EnableKubernetes { get; set; }
        //Send error when the InstrumentationKey its not configured.
        public override void Validate()
        {
            RuleFor(d => InstrumentationKey).NotEmpty().WithErrorCode("INSTRUMENTATION_KEY_MUSTNOT_BE_EMPTY'");
        }
    }
}
