using Liquid.Interfaces;
using System.IO;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Liquid.Runtime
{
    /// <summary>
    /// Include support of Cache, that processing data included on Configuration file.
    /// </summary>
    public abstract class LightCache : ILightCache
    {
        /// <summary>
        /// Initialize support of Cache
        /// </summary>
        public abstract void Initialize();
        /// <summary>
        /// Get Key on the server cache
        /// </summary>
        /// <typeparam name="T">Type of object</typeparam>
        /// <param name="key">Key of object</param>
        /// <returns>object</returns>
        public abstract T Get<T>(string key);
        /// <summary>
        /// Get Key Async on the server cache
        /// </summary>
        /// <typeparam name="T">Type of object</typeparam>
        /// <param name="key">Key of object</param>
        /// <returns>Task with object</returns>
        public abstract Task<T> GetAsync<T>(string key);
        /// <summary>
        /// Set Key  and value on the server cache
        /// </summary>
        /// <typeparam name="T">Type of object</typeparam>
        /// <param name="key">Key of object</param>
        /// <returns>object</returns>
        public abstract void Set<T>(string key, T value);
        /// <summary>
        /// Set Key and value Async on the server cache
        /// </summary>
        /// <typeparam name="T">Type of object</typeparam>
        /// <param name="key">Key of object</param>
        /// <returns>Task with object</returns>
        public abstract Task SetAsync<T>(string key, T value);

        /// <summary>
        /// Refresh key get on the server cache
        /// </summary>
        /// <param name="key">Key of object</param>
        public abstract void Refresh(string key);
        /// <summary>
        /// Refresh async key get on the server cache
        /// </summary>
        /// <param name="key">Key of object</param>
        /// <returns>Task</returns>
        public abstract Task RefreshAsync(string key);
        /// <summary>
        ///  Remove key on the server cache
        /// </summary>
        /// <param name="key">Key of object</param>
        public abstract void Remove(string key);
        /// <summary>
        ///  Remove async key on the server cache
        /// </summary>
        /// <param name="key">Key of object</param>
        /// <returns>Task</returns>
        public abstract Task RemoveAsync(string key);

        /// <summary>
        /// Convert object to ByteArray
        /// </summary>
        /// <typeparam name="T">Type of object</typeparam>
        /// <param name="obj">object</param>
        /// <returns>Array of byte</returns>
        public static byte[] ToByteArray(object obj)
        {
            using (var m = new MemoryStream())
            {
                var ser = new DataContractSerializer(obj.GetType());
                ser.WriteObject(m, obj);
                return m.ToArray();
            }
        }
        /// <summary>
        /// Convert Array of byte to object
        /// </summary>
        /// <typeparam name="T">Type of object</typeparam>
        /// <param name="data">Array of byte</param>
        /// <returns>object</returns>
        public static T FromByteArray<T>(byte[] data)
        {
            if (data != null)
            {
                using (var m = new MemoryStream(data))
                {
                    var ser = new DataContractSerializer(typeof(T));
                    return (T)ser.ReadObject(m);
                }
            }
            else
            {
                return default(T);
            }
        }
    }
}
