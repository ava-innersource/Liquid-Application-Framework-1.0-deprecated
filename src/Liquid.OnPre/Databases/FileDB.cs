using Liquid.Interfaces;
using Liquid.Repository;
using Liquid.Runtime.Configuration.Base;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Liquid.OnWindowsClient
{
    /// <summary>
    /// Provides a unique namespace to store and access your File data objects.
    /// </summary>
    public class FileDB : LightRepository
    {
        private readonly string _suffixName;
        private string _path;
        private FileDBConfiguration config;
        /// <summary>
        /// Creates a File instance from default settings provided on appsettings.
        /// </summary>
        public FileDB() : this("")
        {

        }

        /// <summary>
        /// Creates a File instance from specific settings to a given name provided on appsettings.
        /// The configuration should be provided like "File_{sufixName}".
        /// 
        /// </summary>
        /// <param name="suffixName">Name of configuration</param>
        public FileDB(string suffixName)
        {
            this._suffixName = suffixName;
        }

        /// <summary>
        /// Initialize File repository
        /// </summary>
        public override void Initialize()
        {

            if (string.IsNullOrEmpty(this._suffixName)) // Load specific settings if provided
                this.config = LightConfigurator.Config<FileDBConfiguration>($"{nameof(FileDB)}");
            else
                this.config = LightConfigurator.Config<FileDBConfiguration>($"{nameof(FileDB)}_{this._suffixName}");

            ///Initialize File instance with provided settings            
            LoadConfigurations();
        }

        /// <summary>
        /// Load Configutaion variables
        /// </summary>
        private void LoadConfigurations()
        {
            _setup = true;
            _path = this.config.Path;
            if (_path.Substring(_path.Length - 1, 1) != "\\")
            {
                _path += "\\";
            }
        }
        /// <summary>
        /// Create root folder 
        /// </summary>
        /// <param name="collection"></param>
        private void CreateFolder(string collection)
        {
            string fullPath = $"{ _path}{collection}";
            if (!Directory.Exists(fullPath))
            {
                Directory.CreateDirectory(fullPath);
            }
        }
        /// <summary>
        /// Creat Document
        /// </summary>
        /// <param name="collection"></param>
        private void CreateOrUpdateDocument(string collection, dynamic model)
        {
            CreateFolder(collection);
            string fullPath = $"{ _path}{collection}\\{model.id}.json";
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }
            using (StreamWriter str = new StreamWriter(fullPath))
            {
                str.Write(model.ToJsonCamelCase());
            }
        }
        /// <summary>
        /// Creat Document
        /// </summary>
        /// <param name="collection"></param>
        private async Task<T> GetDocumentAsync<T>(string collection, string entityId)
        {
            CreateFolder(collection);
            string fullPath = $"{ _path}{collection}\\{entityId}.json";
            T ligthModel = default(T);
            if (File.Exists(fullPath))
            {
                JToken json = JToken.Parse(File.ReadAllText(fullPath));
                ligthModel = json.ToObject<T>();
            }
            return ligthModel;
        }
        /// <summary>
        /// Creat Document
        /// </summary>
        /// <param name="collection"></param>
        private async Task<List<T>> GetAllDocumentAsync<T>(string collection)
        {
            CreateFolder(collection);
            string fullPath = $"{ _path}{collection}";
            List<T> ligthModels = new List<T>();
            FileInfo[] Files = new DirectoryInfo(_path).GetFiles(collection, SearchOption.TopDirectoryOnly);
            if (Files.Count() > 0)
            {
                foreach (FileInfo fileInfo in Files)
                {
                    string arquivo = fileInfo.FullName;
                    T ligthModel = default(T);
                    JToken json = JToken.Parse(File.ReadAllText(fileInfo.FullName));
                    ligthModel = json.ToObject<T>();
                    ligthModels.Add(ligthModel);
                }
            }
            return ligthModels;
        }
        /// <summary>
        /// Add or update the document
        /// Calls the base class because there may be some generic behavior in it
        /// </summary>
        /// <typeparam name="T">generic type</typeparam>
        /// <param name="model">generic model</param>
        /// <returns>The entity to upserted</returns>
        public override async Task<T> AddOrUpdateAsync<T>(T model)
        {
            await base.AddOrUpdateAsync(model);
            Guid id = Guid.NewGuid();
            dynamic lightModel = model;
            if (string.IsNullOrEmpty(lightModel.id))
                lightModel.id = id;

            CreateOrUpdateDocument(typeof(T).GetType().Name, lightModel);

            return await Task.FromResult<T>(lightModel);
        }

        /// <summary>
        /// Get the document by id
        /// Calls the base class because there may be some generic behavior in it
        /// </summary>
        /// <typeparam name="T">generic type</typeparam>
        /// <param name="id">document id</param>
        /// <returns>generic type of document</returns>
        public override async Task<T> GetByIdAsync<T>(string entityId)
        {
            await base.GetByIdAsync<T>(entityId);

            dynamic model = GetDocumentAsync<T>(typeof(T).Name, entityId);

            return await Task.FromResult<T>(model);
        }

        /// <summary>
        /// Calls the base class because there may be some generic behavior in it
        /// Counts the quantity of documents
        /// </summary>
        /// <typeparam name="T">generic type</typeparam>
        /// <returns>Quantity of Documents</returns>
        public override async Task<int> CountAsync<T>()
        {
            await base.CountAsync<T>();
            List<T> models = await GetAllDocumentAsync<T>(typeof(T).Name);
            return await Task.FromResult<int>(models.Count());
        }

        /// <summary>
        /// Calls the base class because there may be some generic behavior in it
        /// Counts the quantity of documents
        /// </summary>
        /// <typeparam name="T">generic type</typeparam>
        /// <param name="predicate">lambda expression</param>
        /// <returns>Quantity of Document</returns>
        public override async Task<int> CountAsync<T>(Expression<Func<T, bool>> predicate)
        {
            await base.CountAsync<T>(predicate); //Calls the base class because there may be some generic behavior in it
            dynamic models = await GetAllDocumentAsync<T>(typeof(T).Name);
            return await Task.FromResult<T>(models.Count(predicate));

        }
        /// <summary>
        /// Calls the base class because there may be some generic behavior in it
        /// Get the Generic object
        /// </summary>
        /// <typeparam name="T">generic type</typeparam>
        /// <param name="predicate">lambda expression</param>
        /// <returns>The generic object</returns>
        public override async Task<IQueryable<T>> GetAsync<T>(Expression<Func<T, bool>> predicate)
        {
            await base.GetAsync<T>(predicate);
            List<T> models = await GetAllDocumentAsync<T>(typeof(T).Name);
            dynamic query = models.AsQueryable().Where(predicate);
            return await Task.FromResult<IQueryable<T>>(query);
        }


        /// <summary>
        /// Add or update the document
        /// Calls the base class because there may be some generic behavior in it
        /// </summary>
        /// <typeparam name="T">generic type</typeparam>
        /// <param name="listModels">list generic models</param>
        /// <returns>Entities not processed due to errors</returns>
        public override async Task<IEnumerable<T>> AddOrUpdateAsync<T>(List<T> listModels)
        {
            List<T> errorEntities = new List<T>();
            throw new NotImplementedException();
        }
        /// <summary>
        /// Fill data on database
        /// </summary>
        /// <param name="initialData"></param>
        /// <param name="success"></param>
        /// <param name="database"></param>
        /// <param name="collection"></param>
        private static void InsertData(List<JObject> initialData, ref bool success, string database, string collection)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Creates a new database if doesn't exisists
        /// </summary>
        /// <returns></returns>
        public override async Task CreateDatabaseIfNotExistsAsync(string database, string collection)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns a new File Collection for creation
        /// </summary>
        /// <param name="collection"></param>
        private dynamic newDocumentCollection(string collection)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Creates a Collection if doesn't exisists
        /// </summary>
        /// <returns></returns>
        public override async Task CreateCollectionIfNotExistsAsync(string database, string collection)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Clean the database and fullfill with inicialdata 
        /// (only if is develop, integration and testing environment)
        /// </summary>
        /// <param name="query">name of file separetd with ','</param>
        /// <returns>Result from the operation in a message</returns>
        public override bool ResetData(string query)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Overload methods for alternative media storage settings
        /// </summary>
        /// <param name="mediaStorage"></param>
        public override void SetMediaStorage(ILightMediaStorage mediaStorage)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Azure File allows you to store binary blobs/media 
        /// either with Azure File or to your own remote media store. 
        /// Allows you to represent the metadata of a media
        /// in terms of a special document called attachment
        /// </summary>
        /// <param name="entityId">id of the entity</param>
        /// <param name="fileName"> the name of the file</param>
        /// <param name="attachment">the attachment</param>
        /// <returns>The attachtment</returns>
        private LightAttachment GetLightAttachment(string entityId, string fileName, Stream attachment)
        {
            LightAttachment lightAttachment = new LightAttachment()
            {
                Id = fileName,
                Name = fileName,
                MediaStream = attachment,
                MediaLink = fileName,
                ResourceId = entityId,
                ContentType = MimeMapping.MimeUtility.GetMimeMapping(fileName)
            };
            return lightAttachment;
        }

        /// <summary>
        /// Converts the Attachment type to a LightAttachment type attachment
        /// </summary>
        /// <param name="attachment">the Attachment</param>
        /// <returns>The attachment in LightAttatchment type</returns>
        private LightAttachment ConvertLightAttachment(dynamic attachment)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// Given a database, collection, document, and attachment id, this method creates an attachment link.
        /// </summary>
        /// <param name="entityId">document id</param>
        /// <param name="idAttachment">attachment id</param>
        /// <returns></returns>
        private Uri GetAttachmentUri(string entityId, string idAttachment)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// Azure File allows you to store binary blobs/media 
        /// either with Azure File or to your own remote media store. 
        /// allows you to represent the metadata of a media
        /// in terms of a special document called attachment
        /// </summary>
        /// <param name="lightAttachment">attachment came from LightAttachment</param>
        /// <returns>the attachment</returns>
        private dynamic GetAttachment(ILightAttachment lightAttachment)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// Add or update the attachment
        /// Calls the base class because there may be some generic behavior in it
        /// </summary>
        /// <param name="entityId">document id</param>
        /// <param name="fileName">name of the file</param>
        /// <param name="attachment">the attachment</param>
        /// <returns></returns>
        public override async Task<ILightAttachment> AddOrUpdateAttachmentAsync<T>(string entityId, string fileName, Stream attachment)
        {
            await base.AddOrUpdateAttachmentAsync<T>(entityId, fileName, attachment); //Calls the base class because there may be some generic behavior in it
            throw new NotImplementedException();
        }

        /// <summary>
        /// Calls the base class because there may be some generic behavior in it
        /// Get the attatchment by id and filename
        /// </summary>
        /// <param name="entityId">attachment id</param>
        /// <param name="fileName">name of the file</param>
        /// <returns>The attachment type LightAttachment</returns>
        public override async Task<ILightAttachment> GetAttachmentAsync<T>(string entityId, string fileName)
        {
            await base.GetAttachmentAsync<T>(entityId, fileName);
            throw new NotImplementedException();
        }

        /// <summary>
        /// Calls the base class because there may be some generic behavior in it
        /// List the Attachments by id
        /// </summary>
        /// <param name="entityId">attatchment id</param>
        /// <returns>A list of attachments</returns>
        public override async Task<IEnumerable<ILightAttachment>> ListAttachmentsByIdAsync<T>(string entityId)
        {
            await base.ListAttachmentsByIdAsync<T>(entityId);
            throw new NotImplementedException();
        }



        /// <summary>
        /// Calls the base class because there may be some generic behavior in it 
        /// Deletes the especific attachment
        /// </summary>
        /// <param name="entityId"> id of the document</param>
        /// <param name="fileName">id of the attachment</param>
        public override async Task DeleteAttachmentAsync<T>(string entityId, string fileName)
        {
            await base.DeleteAttachmentAsync<T>(entityId, fileName);
            throw new NotImplementedException();
        }

        /// <summary>
        /// Calls the base class because there may be some generic behavior in it
        /// Delete the specific document
        /// </summary>
        /// <param name="entityId">the id of the document</param>
        /// <returns></returns>
        public override async Task DeleteAsync<T>(string entityId)
        {
            await base.DeleteAsync<T>(entityId);

            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the generic object by page
        /// </summary>
        /// <typeparam name="T">generic type</typeparam>
        /// <param name="filter">filter to fetch the object</param>
        /// <param name="page">page</param>
        /// <param name="itemsPerPage">itens per page</param>
        /// <returns>An object with pagination</returns>
        public override async Task<ILightPaging<T>> GetByPageAsync<T>(string token, Expression<Func<T, bool>> filter, int page, int itemsPerPage)
        {
            await base.GetByPageAsync<T>(token, filter, page, itemsPerPage); //Calls the base class because there may be some generic behavior in it
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the generic object by page
        /// </summary>
        /// <typeparam name="T">generic type</typeparam>
        /// <param name="filter">filter to fetch the object</param>
        /// <param name="page">page</param>
        /// <param name="itemsPerPage">itens per page</param>
        /// <returns>An object with pagination</returns>
        public override async Task<ILightPaging<T>> GetByPageAsync<T>(Expression<Func<T, bool>> filter, int page, int itemsPerPage)
        {
            await base.GetByPageAsync<T>(filter, page, itemsPerPage); //Calls the base class because there may be some generic behavior in it
            throw new NotImplementedException();
        }

        /// <summary>
        /// Calls the base class because there may be some generic behavior in it
        /// This method creates a query for documents under a collection
        /// </summary>
        /// <param name="query">SQL statement with parameterized values</param>
        /// <returns>JSON object</returns>
        public override async Task<JObject> QueryAsyncJson<T>(string query)
        {
            await base.QueryAsyncJson<T>(query);

            throw new NotImplementedException();
        }

        /// <summary>
        /// Calls the base class because there may be some generic behavior in it
        /// This method creates a query for documents under a collection
        /// </summary>
        /// <typeparam name="T">generic type</typeparam>
        /// <param name="query">query to create the document</param>
        /// <returns></returns>
        public override async Task<IEnumerable<T>> QueryAsync<T>(string query)
        {
            await base.QueryAsync<T>(query);
            throw new NotImplementedException();

        }

        /// <summary>
        /// Method to run Health Check for File 
        /// </summary>
        /// <param name="serviceKey"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public override LightHealth.HealthCheck HealthCheck(string serviceKey, string value)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Implement a condition optimistic. In this case the value eTag will be used for control on optimist concurrency.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="model"></param>
        /// <returns></returns>
        public override dynamic AccessConditionOptimistic<T>(T model)
        {
            throw new NotImplementedException();
        }
    }
}