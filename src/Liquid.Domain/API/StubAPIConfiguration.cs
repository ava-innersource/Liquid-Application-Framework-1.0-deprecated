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
        private string _name;

        /// <summary>
        /// Gets or sets the name of the HTTP method (e.g. GET, PUT, POST).
        /// </summary>
        /// <remarks>By setting a value to this property, <see cref="WorkBenchServiceHttp"/> will also have its value updated.</remarks>
        public string Name
        {
            get => _name;
            set
            {
                if (!Enum.TryParse(value, ignoreCase: true, out WorkBenchServiceHttp workBenchServiceHttp))
                {
                    throw new ArgumentException($"The value '{value}' is not a valid HTTP method.", nameof(value));
                }

                _name = value;
                WorkBenchServiceHttp = workBenchServiceHttp;
            }
        }

        public string Route { get; set; }
        public JToken Request { get; set; }
        public JToken Response { get; set; }

        /// <summary>
        /// Gets the value of <see cref="Name"/> converted to an <see cref="Liquid.WorkBenchServiceHttp"/>.
        /// </summary>
        public WorkBenchServiceHttp WorkBenchServiceHttp { get; private set; }

        /// <summary>
        ///  The method used to validate settings retrieved from HostStubAPIConfiguration.
        /// </summary>
        public override void Validate()
        {
            RuleFor(x => x.Route).NotEmpty().WithMessage("The Host property should be informed on Stub API settings");
        }

    }
}