using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace Liquid.Runtime
{
    public class ConfigValidator<T> : AbstractValidator<T>
    {
        public new ConfigResultValidation Validate(T input)
        {
            return new ConfigResultValidation(base.Validate(input));
        }
    }
}
