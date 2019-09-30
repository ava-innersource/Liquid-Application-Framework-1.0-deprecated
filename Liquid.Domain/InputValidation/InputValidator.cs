using FluentValidation;

namespace Liquid.Domain
{
    public class InputValidator<T> : AbstractValidator<T>
    {
        public new InputResultValidation Validate(T input)
        {
            return new InputResultValidation(base.Validate(input));
        }
    }
}
