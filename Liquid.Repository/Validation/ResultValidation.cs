using System.Collections.Generic;
using FluentValidation.Results;

namespace Liquid.Repository
{
    public class ResultValidation  
    {
        private readonly ValidationResult _validationResult;
        private readonly List<string> _errors ; 


        public ResultValidation(ValidationResult validationResult)
        {
            _errors = new List<string>();
            _validationResult = validationResult;
            foreach (ValidationFailure failure in _validationResult.Errors)
            {
                _errors.Add(failure.ErrorCode);
            }
        }

        public bool IsValid { get { return _validationResult.IsValid; } }

        public List<string> Errors {
            get { 
                return _errors;
            }
        } 
    }
}
