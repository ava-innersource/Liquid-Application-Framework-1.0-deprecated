using Liquid.Runtime.Configuration.Base;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Localization;
using System.Collections.Generic;
using System.Globalization;

namespace Liquid.Runtime
{
    /// <summary>
    /// 
    /// </summary>
    public static class LocalizationExtensions
    {
        /// <summary>
        /// Add JWT support on authentication
        /// </summary>
        /// <param name="services"></param>
        public static void AddLocalization(this IApplicationBuilder builder)
        {
            var config = LightConfigurator.Config<LocalizationConfig>("Localization");
            if (config != null)
            {

                IList<CultureInfo> supportedCultures = new List<CultureInfo>();

                foreach (var item in config.SupportedCultures)
                {
                    supportedCultures.Add(new CultureInfo(item));
                }

                builder.UseRequestLocalization(new RequestLocalizationOptions
                {
                    DefaultRequestCulture = new RequestCulture(config.DefaultCulture),
                    SupportedCultures = supportedCultures,
                    SupportedUICultures = supportedCultures
                });
            }
        }
    }
}
