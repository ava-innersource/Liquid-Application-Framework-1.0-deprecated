using System;
using System.Threading.Tasks;
using Liquid.Base.Domain;
using Liquid.Interfaces;

namespace Liquid.Domain
{
    public abstract class LightQuery<T> : LightViewModel<T> where T : LightQuery<T>, new()
    {
    }
}
