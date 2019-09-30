using FluentValidation;
using Liquid.Runtime.Configuration;
using System.Collections.Generic;

namespace Liquid.Runtime
{
    /// <summary>
    /// Model of Swagger Configuration details
    /// </summary>
    public class SwaggerConfiguration : LightConfig<SwaggerConfiguration>
    {
        public string ActiveVersion { get; set; }
        public List<SwaggerVersion> Versions { get; set; }
        public string BasePath { get; set; }
        public string Host { get; set; }
        public string[] Schemes { get; set; }
        public string[] ExcludingSwaggerList { get; set; }

        public override void Validate()
        {

        }
    }

    /// <summary>
    /// Model of Swagger Version details
    /// </summary>
    public class SwaggerVersion : LightConfig<SwaggerVersion>
    {
        public string Name { get; set; }
        public SwaggerInfo Info { get; set; }
        public bool IsActiveVersion { get; set; }

        public override void Validate()
        {
            
        }
    }

    /// <summary>
    /// Model of Swagger Info details
    /// </summary>
    public class SwaggerInfo : LightConfig<SwaggerInfo>
    {
        public string Description { get; set; }
        public string Version { get; set; }
        public string Title { get; set; }
        public string TermsOfService { get; set; }
        public SwaggerContact Contact { get; set; }
        public SwaggerLicense License { get; set; }
        public override void Validate()
        {

        }
    }

    /// <summary>
    /// Model of Swagger Contact details
    /// </summary>
    public class SwaggerContact : LightConfig<SwaggerContact>
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Url { get; set; }
        public override void Validate()
        {

        }
    }

    /// <summary>
    /// Model of Swagger License details
    /// </summary>
    public class SwaggerLicense : LightConfig<SwaggerLicense>
    {
        public string Name { get; set; }
        public string Url { get; set; }

        public override void Validate()
        {
           
        }
    }
}