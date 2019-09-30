using System;
using System.Linq.Expressions;
using FluentValidation;
using Liquid.Interfaces;

namespace Liquid.Repository
{
    public class Validator<T>  : AbstractValidator<T> 
    {
        public new ResultValidation Validate(T input) 
        { 
            return new ResultValidation(base.Validate(input));
        } 
    }
}
