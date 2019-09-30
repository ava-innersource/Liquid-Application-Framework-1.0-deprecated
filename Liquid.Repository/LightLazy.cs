using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Liquid.Repository
{
    public class LightLazy<T> : Lazy<Task<T>>
    {
        public LightLazy(Func<T> valueFactory) : base(() => Task.Factory.StartNew(valueFactory))
        { }

        public LightLazy(Func<Task<T>> taskFactory) : base(() => Task.Factory.StartNew(taskFactory).Unwrap())
        { }

        public TaskAwaiter<T> GetAwaiter()
        {
            return Value.GetAwaiter();
        }
    }
}
