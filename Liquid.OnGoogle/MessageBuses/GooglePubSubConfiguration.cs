using FluentValidation;
using Liquid.Runtime.Configuration;
using System;

namespace Liquid.OnGoogle
{
    /// <summary>
    ///  Configuration of the for connect a Service Bus (Queue / Topic).
    /// </summary>
    public class GooglePubSubConfiguration : LightConfig<GooglePubSubConfiguration>
    {
        public string ProjectID { get; set; }    
        public override void Validate()
        {
            RuleFor(d => ProjectID).NotEmpty().WithMessage("ProjectID settings should not be empty.");        
        }
    }
}
