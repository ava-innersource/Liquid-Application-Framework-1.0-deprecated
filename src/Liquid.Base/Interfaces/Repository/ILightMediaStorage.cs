using Liquid.Base.Interfaces;
using System;
using System.Threading.Tasks;

namespace Liquid.Interfaces
{
    /// <summary>
    /// Public interface for all Media Storage Implementations 
    /// </summary>
    public interface ILightMediaStorage : IWorkBenchHealthCheck
    {	
		string Conection { get; set; }
        string Container { get; set; }
        Task<ILightAttachment> GetAsync(string resourceId, string id);
        Task InsertUpdateAsync(ILightAttachment attachment); 
        Task Remove(ILightAttachment attachment); 
    }
}
