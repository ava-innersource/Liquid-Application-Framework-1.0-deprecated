using FluentValidation;
using Liquid.Runtime.Polly;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace Liquid.Runtime.Configuration
{
    /// <summary>
    /// Validates the host property from HostStubAPIConfiguration
    /// </summary>
    public class StubAPIConfiguration : LightConfig<StubAPIConfiguration>
    {
        public List<HostStubAPIConfiguration> Hosts { get; set; }

        /// <summary>
        ///  The method used to validate settings retrieved from StubAPIConfiguration.
        /// </summary>
        public override void Validate()
        {
            RuleFor(x => x.Hosts).NotEmpty().WithMessage("The Host property should be informed on Stub API settings");
        }
    }


    /// <summary>
    /// Validates the host property from HostStubAPIConfiguration
    /// </summary>
    public class HostStubAPIConfiguration : LightConfig<HostStubAPIConfiguration>
    {
        public string Name { get; set; }
        public List<MethodStubAPIConfiguration> Methods { get; set; }

        /// <summary>
        ///  The method used to validate settings retrieved from HostStubAPIConfiguration.
        /// </summary>
        public override void Validate()
        {
            RuleFor(x => x.Methods).NotEmpty().WithMessage("The Host property should be informed on Stub API settings");
        }

    }


    /// <summary>
    /// Validates the host property from MethodStubAPIConfiguration
    /// </summary>
    public class MethodStubAPIConfiguration : LightConfig<MethodStubAPIConfiguration>
    {
        public string Name { get; set; }
        public string Route { get; set; }
        public JToken Request { get; set; }
        public JToken Response { get; set; }

        public WorkBenchServiceHttp WorkBenchServiceHttp
        {
            get
            {
                if (Name.ToUpper() == "GET")
                    return WorkBenchServiceHttp.GET;
                else if (Name.ToUpper() == "POST")
                    return WorkBenchServiceHttp.POST;
                else if (Name.ToUpper() == "PUT")
                    return WorkBenchServiceHttp.PUT;
                else if (Name.ToUpper() == "OPTIONS")
                    return WorkBenchServiceHttp.OPTIONS;
                else if (Name.ToUpper() == "DELETE")
                    return WorkBenchServiceHttp.DELETE;
                else if (Name.ToUpper() == "HEAD")
                    return WorkBenchServiceHttp.HEAD;
                else if (Name.ToUpper() == "TRACE")
                    return WorkBenchServiceHttp.TRACE;
                else if (Name.ToUpper() == "CONNECT")
                    return WorkBenchServiceHttp.CONNECT;
                else if (Name.ToUpper() == "PATH")
                    return WorkBenchServiceHttp.PATH;
                else
                    return WorkBenchServiceHttp.GET;
            }
        }

        /// <summary>
        ///  The method used to validate settings retrieved from HostStubAPIConfiguration.
        /// </summary>
        public override void Validate()
        {
            RuleFor(x => x.Route).NotEmpty().WithMessage("The Host property should be informed on Stub API settings");
        }

    }
}