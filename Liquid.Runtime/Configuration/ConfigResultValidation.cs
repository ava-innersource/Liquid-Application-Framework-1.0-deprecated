using System.Collections.Generic;
using FluentValidation.Results;

namespace Liquid.Runtime
{
    public class ConfigResultValidation  
    {
        private readonly ValidationResult _validationResult;

        public ConfigResultValidation(ValidationResult validationResult)
        {
            Errors = new List<string>();
            _validationResult = validationResult;
            foreach (ValidationFailure failure in _validationResult.Errors)
            {
                Errors.Add(failure.ErrorMessage);
            }
        }

        public bool IsValid { get { return _validationResult.IsValid; } }

        public List<string> Errors { get; }
    }
}
