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
		String Conection { get; set; }
        String Container { get; set; }
        Task<ILightAttachment> GetAsync(string resourceId, string id);
        void InsertUpdateAsync(ILightAttachment attachment); 
        void Remove(ILightAttachment attachment); 
    }
}
