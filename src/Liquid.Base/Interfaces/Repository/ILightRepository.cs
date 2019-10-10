using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Liquid.Base.Interfaces;

namespace Liquid.Interfaces
{
    /// <summary>
    /// Public interface for all NoSql Database Implementations
    /// </summary>
    public interface ILightRepository : IWorkBenchHealthCheck
    {

        void SetMediaStorage(ILightMediaStorage mediaStorage);        
        Task<T> AddOrUpdateAsync<T>(T model) where T : ILightModel;
        Task<IEnumerable<T>> AddOrUpdateAsync<T>(List<T> listModels) where T : ILightModel;
        Task<ILightAttachment> AddOrUpdateAttachmentAsync<T>(string entityId, string fileName, Stream attachment);
        Task<ILightAttachment> GetAttachmentAsync<T>(string entityId, string fileName);
        Task<IEnumerable<ILightAttachment>> ListAttachmentsByIdAsync<T>(string entityId);
        Task DeleteAsync<T>(string entityId);
        Task DeleteAttachmentAsync<T>(string entityId, string fileName);
        Task<int> CountAsync<T>();
        Task<int> CountAsync<T>(Expression<Func<T, bool>> predicate);
        Task<JObject> QueryAsyncJson<T>(string query);
        Task<IEnumerable<T>> QueryAsync<T>(string query);
        Task<T> GetByIdAsync<T>(string entityId) where T : new();
        Task<IQueryable<T>> GetAsync<T>(Expression<Func<T, bool>> predicate);
        Task<IQueryable<T>> GetAsync<T>();
        Task<ILightPaging<T>> GetByPageAsync<T>(string token, Expression<Func<T, bool>> filter,
            int page, int itemsPerPage);
        Task<ILightPaging<T>> GetByPageAsync<T>(Expression<Func<T, bool>> filter,
            int page, int itemsPerPage);
        bool ResetData(string query);
        dynamic AccessConditionOptimistic<T>(T model);
    }
}
