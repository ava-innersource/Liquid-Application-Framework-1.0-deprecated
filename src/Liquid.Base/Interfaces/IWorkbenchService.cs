using System;

namespace Liquid
{
    /// <summary>
    /// A service used by Liquid that is initializable.
    /// </summary>
    public interface IWorkbenchService
    {
        /// <summary>
        /// Called by Liquid during initialization.
        /// </summary>
        void Initialize();
    }

    /// <summary>
    /// A service used by Liquid that is initializable.
    /// </summary>
    [Obsolete("Use the correct spelled class Liquid.IWorkbenchService")]
    public interface IWorkBenchService : IWorkbenchService
    {
    }
}
