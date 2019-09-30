using Liquid.Base;

namespace Liquid.Interfaces
{
    /// <summary>
    /// Interface message inheritance to use a liquid framework
    /// </summary> 
    public interface ILightMessage
    {
        ILightContext Context { get; set; }
        string TokenJwt { get; set; }
    }
}