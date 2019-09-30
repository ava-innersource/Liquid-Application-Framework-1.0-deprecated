using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Liquid.Repository
{
    public interface ILightDataBase<T, U, V>
    {
        T GetConnection();
        Task<U> GetOrCreateDatabaseAsync();
        Task<V> GetOrCreateCollectionAsync();
    }
}
