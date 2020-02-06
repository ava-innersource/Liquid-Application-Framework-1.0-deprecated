using Liquid.Domain;
using Liquid.Interfaces;

namespace Liquid.Activation
{
    /// <summary>
    ///  Base class for messages exchanged between services.
    /// </summary> 
    public abstract class LightMessage<T> : LightViewModel<T>
        where T : LightViewModel<T>, ILightViewModel, new()
    {
        // TODO: do we actually need this type?
    }
}
