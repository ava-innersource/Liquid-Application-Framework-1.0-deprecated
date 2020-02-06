using System;
using AutoFixture;
using AutoFixture.AutoNSubstitute;
using Xunit;
using Liquid.Runtime;

namespace Liquid.Runtime.Tests
{
    public class SecretsConfigurationTest
    {
        
        [Fact]
        public void WhenEnabledModuleShouldBeMandatory()
        {
            var configuration = new SecretsConfiguration();
            configuration.Enable = true;
            
            configuration.Validate();
            var results = configuration.Validator.Validate(configuration);

            Assert.False(results.IsValid);
            Assert.Contains(results.Errors,(value) => value == "'Module' on Secrets settings should not be empty.");
        }

        [Fact]
        public void WhenEnabledNameShouldBeMandatory()
        {
            var configuration = new SecretsConfiguration();
            configuration.Enable = true;

            configuration.Validate();
            var results = configuration.Validator.Validate(configuration);

            Assert.False(results.IsValid);
            Assert.Contains(results.Errors, (value) => value == "'Name' on Secrets settings should not be empty.");
        }

        [Fact]
        public void WhenEnabledModuleShouldEqualK8s()
        {
            var configuration = new SecretsConfiguration();
            configuration.Enable = true;
            configuration.Module = "Invalid";

            configuration.Validate();
            var results = configuration.Validator.Validate(configuration);

            Assert.False(results.IsValid);
            Assert.Contains(results.Errors, (value) => value == "'Module' on Secrets settings should be equal to K8S_SECRETS");
        }

        [Fact]
        public void WhenEnabledValidationPass()
        {
            var configuration = new SecretsConfiguration();
            configuration.Enable = true;
            configuration.Module = "K8S_SECRETS";
            configuration.Name = "Name";

            configuration.Validate();

            var results = configuration.Validator.Validate(configuration);

            Assert.True(results.IsValid);
            Assert.Empty(results.Errors);
        }

        [Fact]
        public void WhenNotEnabledValidationPass()
        {
            var configuration = new SecretsConfiguration();
            configuration.Enable = false;

            configuration.Validate();

            var results = configuration.Validator.Validate(configuration);

            Assert.True(results.IsValid);
            Assert.Empty(results.Errors);
        }
    }
}
