using FluentValidation.Results;
using System.Collections.Generic;

namespace Liquid.Domain
{
    /// <summary>
    /// Class responsible to receive the result from the input validation on ViewModel
    /// </summary>
    public class InputResultValidation
    {
        private readonly ValidationResult _validationResult;

        ///The method receives the propertie ValidationResult from the input validation from ViewModel
        ///and add on an errors list.
        public InputResultValidation(ValidationResult validationResult)
        {
            Errors = new List<string>();
            _validationResult = validationResult;
            foreach (ValidationFailure failure in _validationResult.Errors)
            {
                if (!string.IsNullOrEmpty(failure.ErrorCode))
                    Errors.Add(failure.ErrorCode);
                else
                    Errors.Add(failure.ErrorMessage);
            } 
        }

        public bool IsValid { get { return _validationResult.IsValid; } }

        public List<string> Errors { get; }
    }
}
