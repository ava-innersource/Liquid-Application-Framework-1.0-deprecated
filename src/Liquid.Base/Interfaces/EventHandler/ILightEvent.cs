using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Liquid.Base.Interfaces;

namespace Liquid.Interfaces
{
    /// <summary>
    /// Interface message inheritance to use a liquid framework
    /// </summary> 
    public interface ILightEvent : IWorkbenchHealthCheck
    {
        Task<T> SendToHub<T>(T model, string dataOperation);
    }
}
