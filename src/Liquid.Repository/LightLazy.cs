using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Liquid.Repository
{
    /// <summary>
    /// Specialization of a <see cref="Lazy{T}"/> that calls the valueFactory on a Task. 
    /// </summary>
    /// <typeparam name="T">The type of the object that is lazyly produced.</typeparam>
    /// <remarks>
    /// Made obsolete since there are issues with this type.
    /// 
    /// There's a conceptual issue here - lazy objects are be loaded at the last minute, when necessary. This
    /// Lazy values are supposed to be loaded once, "just in time", and then available in memory. 
    /// This implementation brings the extra effort of having to always await on a type that will 
    /// almost always be available, which is a overhead that we should avoid. 
    /// </remarks>
    [Obsolete("Use System.Lazy instead.")]
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
