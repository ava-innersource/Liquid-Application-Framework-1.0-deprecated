using Liquid.Base;
using Liquid.Interfaces;
using Liquid.Repository;
using Liquid.Runtime.Configuration.Base;
using Liquid.Runtime.Telemetry;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Liquid.OnAzure
{
    /// <summary>
    /// Provides a unique namespace to store and access your Azure Storage data objects.
    /// </summary>
    public class CosmosDB : LightRepository, ILightDataBase<DocumentClient, Database, DocumentCollection>
    {
        private static DocumentClient _client;
        private LightLazy<Database> _database;
        private string _databaseId;
        // BUG: This is reminiscent of the old, non thread-safe implementation. We must remove it.
        private string _collectionName;
        private readonly string _suffixName;
        private string _endpoint;
        private string _authKey;
        private ConnectionPolicy _connPolicy;
        private ILightMediaStorage _mediaStorage;
        private CosmosDBConfiguration config;

        /// <summary>
        /// Holds the collection were a type will be stored
        /// </summary>
        private readonly ConcurrentDictionary<Type, LightLazy<DocumentCollection>> _typeToCollectionMap = new ConcurrentDictionary<Type, LightLazy<DocumentCollection>>();

        /// <summary>
        /// Creates a CosmosDB instance from default settings provided on appsettings.
        /// </summary>
        public CosmosDB() : this("")
        {

        }

        /// <summary>
        /// Creates a CosmosDB instance from specific settings to a given name provided on appsettings.
        /// The configuration should be provided like "CosmosDB_{sufixName}".
        /// 
        /// </summary>
        /// <param name="suffixName">Name of configuration</param>
        public CosmosDB(string suffixName)
        {
            this._suffixName = suffixName;
        }

        /// <summary>
        /// Initialize Cosmos DB repository
        /// </summary>
        public override void Initialize()
        {
            //Change collection in Runtime
            base.TargetVarience(InjectTarget);

            if (string.IsNullOrEmpty(this._suffixName)) // Load specific settings if provided
                this.config = LightConfigurator.Config<CosmosDBConfiguration>($"{nameof(CosmosDB)}");
            else
                this.config = LightConfigurator.Config<CosmosDBConfiguration>($"{nameof(CosmosDB)}_{this._suffixName}");

            ///Initialize CosmosDB instance with provided settings            
            LoadConfigurations();
        }

        /// <summary>
        /// Load Configutaion variables
        /// </summary>
        private void LoadConfigurations()
        {
            _setup = true;
            _endpoint = this.config.Endpoint;
            _authKey = this.config.AuthKey;

            if (!string.IsNullOrEmpty(config.ConnectionMode) && !string.IsNullOrEmpty(config.ConnectionProtocol))
            {
                ConnectionMode connMode = (ConnectionMode)Enum.Parse(typeof(ConnectionMode), config.ConnectionMode, true);
                Protocol connProtocol = (Protocol)Enum.Parse(typeof(Protocol), config.ConnectionProtocol, true);
                _connPolicy = new ConnectionPolicy() { ConnectionMode = connMode, ConnectionProtocol = connProtocol };
            }
            else
                _connPolicy = new ConnectionPolicy();

            _databaseId = this.config.DatabaseId;
            _client = GetConnection();
            _database = new LightLazy<Database>(async () => await GetOrCreateDatabaseAsync());
        }

        /// <summary>
        /// Creates a new database if doesn't exisists
        /// </summary>
        /// <returns></returns>
        public override async Task CreateDatabaseIfNotExistsAsync(string database, string collection)
        {
            try
            {
                await _client.ReadDatabaseAsync(UriFactory.CreateDatabaseUri(database));
            }
            catch (DocumentClientException ex)
            {
                if (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
                    await _client.CreateDatabaseAsync(new Database { Id = database }, new RequestOptions { OfferThroughput = config.DatabaseRUs });
                else
                    throw;
            }
        }

        /// <summary>
        /// Returns a new ComosDB Collection for creation
        /// </summary>
        /// <param name="collection"></param>
        private DocumentCollection newDocumentCollection(string collection)
        {
            DocumentCollection documentCollection = new DocumentCollection { Id = collection };
            documentCollection.PartitionKey.Paths.Add("/" + collection);
            IndexingPolicy indexingPolicy = new IndexingPolicy(new RangeIndex(DataType.String) { Precision = -1 });
            documentCollection.IndexingPolicy = indexingPolicy;
            documentCollection.IndexingPolicy.IndexingMode = IndexingMode.Consistent;

            return documentCollection;
        }

        /// <summary>
        /// Creates a Collection if doesn't exisists
        /// </summary>
        /// <returns></returns>
        public override async Task CreateCollectionIfNotExistsAsync(string database, string collection)
        {
            DocumentCollection documentCollection = null;

            try
            {
                documentCollection = newDocumentCollection(collection);

                await _client.ReadDocumentCollectionAsync(UriFactory.CreateDocumentCollectionUri(database, collection));
                await _client.DeleteDocumentCollectionAsync(UriFactory.CreateDocumentCollectionUri(database, collection));
                await _client.CreateDocumentCollectionAsync(
                    UriFactory.CreateDatabaseUri(database),
                    documentCollection);
            }
            catch (DocumentClientException ex)
            {
                if (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    await _client.CreateDocumentCollectionAsync(
                    UriFactory.CreateDatabaseUri(database),
                    documentCollection);
                }
                else
                    throw;
            }
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
            foreach (JObject record in initialData)
            {
                _client.CreateDocumentAsync(UriFactory.CreateDocumentCollectionUri(database, collection), record);
                success = true;
            }
        }

        /// <summary>
        /// Clean the database and fullfill with inicialdata 
        /// (only if is develop, integration and testing environment)
        /// </summary>
        /// <param name="query">name of file separetd with ','</param>
        /// <returns>Result from the operation in a message</returns>
        public override bool ResetData(string query)
        {
            string database = this.config.DatabaseId;
            bool success = false;

            string[] parts = new string[0];
            if (!string.IsNullOrEmpty(query))
                parts = query.Split(new char[] { ',' });
            else
            {
                parts = GetFilesRessed();
            }
            DocumentCollection documentCollection = null;

            foreach (var fileName in parts)
            {
                string fileEntityName = fileName.Replace(".json", "");
                var json = base.GetMockData(database, fileEntityName);

                _client.DeleteDocumentCollectionAsync(UriFactory.CreateDocumentCollectionUri(database, fileEntityName)).Wait();

                documentCollection = newDocumentCollection(fileEntityName);

                _client.CreateDocumentCollectionAsync(
                    UriFactory.CreateDatabaseUri(database), documentCollection).Wait();

                InsertData(json.ToObject<List<JObject>>(), ref success, database, fileEntityName);
            }

            return success;
        }

        /// <summary>
        /// Overload methods for alternative media storage settings
        /// </summary>
        /// <param name="mediaStorage"></param>
        public override void SetMediaStorage(ILightMediaStorage mediaStorage)
        {
            _mediaStorage = mediaStorage;
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

            DocumentResponse<Document> DocResp = null;

            _typeToCollectionMap.TryGetValue(typeof(T), out var collection);

            try
            {
                DocResp = _client.ReadDocumentAsync<Document>(UriFactory.CreateDocumentUri(_databaseId, (await collection).Id, entityId), new RequestOptions() { PartitionKey = new PartitionKey(Undefined.Value) }).Result;
            }
            catch (AggregateException ae)
            {
                bool notFound = false;
                foreach (var e in ae.Flatten().InnerExceptions)
                {
                    if (e is DocumentClientException docexp && docexp.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        notFound = true;
                    }
                }
                if (!notFound)
                {
                    throw;
                }
            }
            catch (DocumentClientException ex)
            {
                if (ex.StatusCode != System.Net.HttpStatusCode.NotFound)
                {
                    throw;
                }
            }
            return (T)(dynamic)(DocResp == null ? null : DocResp.Document);
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

            _typeToCollectionMap.TryGetValue(typeof(T), out var _collection);

            T upsertedEntity;
            var upsertedDoc = await _client.UpsertDocumentAsync((await _collection).SelfLink, model, options: (RequestOptions)AccessConditionOptimistic(model));
            upsertedEntity = JsonConvert.DeserializeObject<T>(upsertedDoc.Resource.ToString());

            // Call LightEvent to raise events from LightModel's attributes
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            Task.Factory.StartNew(() => RaiseEvent(model, this._dataOperation));
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

            return upsertedEntity;
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

            _typeToCollectionMap.TryGetValue(typeof(T), out var _collection);

            foreach (var model in listModels)
            {
                try
                {
                    await base.AddOrUpdateAsync(listModels);
                    await _client.UpsertDocumentAsync((await _collection).SelfLink, model, options: (RequestOptions)AccessConditionOptimistic(model));
                }
                catch (Exception exRegister)
                {
                    ((LightTelemetry)WorkBench.Telemetry).TrackException(exRegister);
                    errorEntities.Add(model);
                }
            }
            return errorEntities;
        }

        /// <summary>
        /// Azure Cosmos DB allows you to store binary blobs/media 
        /// either with Azure Cosmos DB or to your own remote media store. 
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
        private LightAttachment ConvertLightAttachment(Attachment attachment)
        {
            LightAttachment lightAttachment = new LightAttachment()
            {
                Id = attachment.Id,
                Name = attachment.Id,
                MediaLink = attachment.MediaLink,
                ContentType = attachment.ContentType,
                ResourceId = attachment.ResourceId
            };
            return lightAttachment;
        }
        /// <summary>
        /// Given a database, collection, document, and attachment id, this method creates an attachment link.
        /// </summary>
        /// <param name="entityId">document id</param>
        /// <param name="idAttachment">attachment id</param>
        /// <returns></returns>
        private Uri GetAttachmentUri(string entityId, string idAttachment)
        {
            return UriFactory.CreateAttachmentUri(_databaseId, _collectionName, entityId, idAttachment);
        }
        /// <summary>
        /// Azure Cosmos DB allows you to store binary blobs/media 
        /// either with Azure Cosmos DB or to your own remote media store. 
        /// allows you to represent the metadata of a media
        /// in terms of a special document called attachment
        /// </summary>
        /// <param name="lightAttachment">attachment came from LightAttachment</param>
        /// <returns>the attachment</returns>
        private Attachment GetAttachment(ILightAttachment lightAttachment)
        {
            Attachment attachment = new Attachment()
            {
                Id = lightAttachment.Id,
                MediaLink = lightAttachment.MediaLink,
                ContentType = lightAttachment.ContentType,
                ResourceId = lightAttachment.ResourceId
            };
            return attachment;
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

            _typeToCollectionMap.TryGetValue(typeof(T), out var collection);

            Document doc = await GetByIdAsync<Document>(entityId);
            ILightAttachment upserted = GetLightAttachment(entityId, fileName, attachment);
            Attachment attachmentDB = GetAttachment(upserted);

            if (_mediaStorage != null)
            {
                attachmentDB.MediaLink = null;
                await _client.UpsertAttachmentAsync(doc.SelfLink, attachmentDB, new RequestOptions() { PartitionKey = new PartitionKey(Undefined.Value) });
                await _mediaStorage.InsertUpdateAsync(upserted);                       
            }
            else
            {
                await _client.UpsertAttachmentAsync(doc.SelfLink, attachmentDB);
            }

            return upserted;
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

            _typeToCollectionMap.TryGetValue(typeof(T), out var _collection);

            ILightAttachment lightAttachment;
            if (_mediaStorage != null)
            {
                Attachment attachment = (Attachment)await _client.ReadAttachmentAsync(GetAttachmentUri(entityId, fileName));
                lightAttachment = ConvertLightAttachment(attachment);
                ILightAttachment lightAttachmentStorage = await _mediaStorage.GetAsync(entityId, fileName);
                lightAttachment.MediaStream = lightAttachmentStorage.MediaStream;
            }
            else
                lightAttachment = await _client.ReadAttachmentAsync(((await _collection).SelfLink + "/" + fileName)) as ILightAttachment;

            return lightAttachment;
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

            ILightAttachment lightAttachmentStorage;
            List<ILightAttachment> list = new List<ILightAttachment>();
            Document doc = await GetByIdAsync<Document>(entityId);
            foreach (Attachment attachment in _client.CreateAttachmentQuery(doc.SelfLink))
            {
                LightAttachment att = new LightAttachment
                {
                    Id = attachment.Id,
                    ContentType = attachment.ContentType
                };
                if (_mediaStorage != null)
                {
                    lightAttachmentStorage = await _mediaStorage.GetAsync(entityId, att.Id);
                    att.MediaStream = lightAttachmentStorage.MediaStream;
                }
                else
                {
                    att.MediaStream = File.OpenRead(attachment.MediaLink);
                }
                list.Add(att);
            }
            return list;
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

            _typeToCollectionMap.TryGetValue(typeof(T), out var _collection);

            FeedOptions options = new FeedOptions { EnableCrossPartitionQuery = true, PartitionKey = new PartitionKey(Undefined.Value) };

            return _client.CreateDocumentQuery<T>((await _collection).SelfLink, options).Count();
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
            FeedOptions options = new FeedOptions { EnableCrossPartitionQuery = true, PartitionKey = new PartitionKey(Undefined.Value) };
            _typeToCollectionMap.TryGetValue(typeof(T), out var _collection);

            return _client.CreateDocumentQuery<T>((await _collection).SelfLink, options).Where(predicate).Count();
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
            Attachment attachment = _client.ReadAttachmentAsync(GetAttachmentUri(entityId, fileName)).Result;
            if (attachment != null)
            {
                await _client.DeleteAttachmentAsync(GetAttachmentUri(entityId, fileName));
                if (_mediaStorage != null)
                {
                    LightAttachment att = new LightAttachment
                    {
                        Id = fileName,
                        ResourceId = entityId
                    };
                    _mediaStorage.Remove(att);
                }
            }
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

            _typeToCollectionMap.TryGetValue(typeof(T), out var collection);
            try
            {
                var uri = UriFactory.CreateDocumentUri(_databaseId, (await collection).Id, entityId);
                var doc = _client.ReadDocumentAsync<Document>(uri, new RequestOptions() { PartitionKey = new PartitionKey(Undefined.Value) }).Result;

                if (doc != null)
                {
                    if (_mediaStorage != null)
                    {
                        foreach (Attachment attachment in _client.CreateAttachmentQuery(doc.Document.SelfLink, new FeedOptions() { PartitionKey = new PartitionKey(Undefined.Value) }))
                        {
                            await DeleteAttachmentAsync<T>(entityId, attachment.Id);
                        }
                    }

                    await _client.DeleteDocumentAsync(doc.Document.SelfLink, new RequestOptions() { PartitionKey = new PartitionKey(Undefined.Value) });

                    // Call LightEvent to raise events from LightModel's attributes
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    Task.Factory.StartNew(() => RaiseEvent<T>((T)(dynamic)doc.Document, this._dataOperation));
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                }
            }
            catch (Exception e)
            {
                if (e.InnerException is DocumentClientException)
                {
                    DocumentClientException ex = (DocumentClientException)e.InnerException;
                    if (ex.StatusCode != System.Net.HttpStatusCode.NotFound)
                        throw;
                }
                else
                    throw;
            }
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

            _typeToCollectionMap.TryGetValue(typeof(T), out var _collection);

            FeedOptions options = new FeedOptions { EnableCrossPartitionQuery = true };

            return _client.CreateDocumentQuery<T>((await _collection).SelfLink, options).Where(predicate).AsQueryable();
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

            _typeToCollectionMap.TryGetValue(typeof(T), out var _collection);

            List<T> list = new List<T>();
            string continuationToken = null;
            FeedOptions options = new FeedOptions { MaxItemCount = itemsPerPage, EnableCrossPartitionQuery = true, PartitionKey = new PartitionKey(Undefined.Value), RequestContinuation = token };
            IQueryable<T> queryBase = _client.CreateDocumentQuery<T>((await _collection).DocumentsLink, options);

            if (filter != null) queryBase = queryBase.Where(filter);

            var finalQuery = queryBase.AsDocumentQuery();

            if (finalQuery.HasMoreResults)
            {
                var data = await finalQuery.ExecuteNextAsync<T>();
                continuationToken = data.ResponseContinuation;
                list.AddRange(data.ToList());

            }

            LightPaging<T> result = new LightPaging<T>()
            {
                Data = list,
                Page = page + 1,
                ItemsPerPage = itemsPerPage,
                TotalCount = await CountAsync<T>(filter),
                ContinuationToken = continuationToken
            };

            result.TotalPages = result.TotalCount / itemsPerPage;
            if (result.TotalCount % itemsPerPage > 0) result.TotalPages++;

            return result;
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

            _typeToCollectionMap.TryGetValue(typeof(T), out var _collection);

            List<T> list = new List<T>();
            string continuationToken = null;
            FeedOptions options = new FeedOptions { MaxItemCount = itemsPerPage, EnableCrossPartitionQuery = true, PartitionKey = new PartitionKey(Undefined.Value) };
            IQueryable<T> queryBase = _client.CreateDocumentQuery<T>((await _collection).DocumentsLink, options);

            if (filter != null) queryBase = queryBase.Where(filter);

            var finalQuery = queryBase.AsDocumentQuery();

            int queryPage = 0;
            while (finalQuery.HasMoreResults && queryPage <= page)
            {
                var data = await finalQuery.ExecuteNextAsync<T>();
                if (queryPage++ == page)
                {
                    continuationToken = data.ResponseContinuation;
                    list.AddRange(data.ToList());
                }
            }

            LightPaging<T> result = new LightPaging<T>()
            {
                Data = list,
                Page = page + 1,
                ItemsPerPage = itemsPerPage,
                TotalCount = await CountAsync<T>(filter),
                ContinuationToken = continuationToken
            };

            result.TotalPages = result.TotalCount / itemsPerPage;
            if (result.TotalCount % itemsPerPage > 0) result.TotalPages++;

            return result;
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

            _typeToCollectionMap.TryGetValue(typeof(T), out var _collection);

            var ret = _client.CreateDocumentQuery((await _collection).SelfLink, query).AsQueryable();
            return JObject.Parse(JsonConvert.SerializeObject(ret.ToArray()));
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
            FeedOptions options = new FeedOptions { EnableCrossPartitionQuery = true };

            _typeToCollectionMap.TryGetValue(typeof(T), out var _collection);

            return _client.CreateDocumentQuery<T>((await _collection).SelfLink, query, options).AsQueryable();

        }

        /// <summary>
        /// Method to run Health Check for Cosmos DB 
        /// </summary>
        /// <param name="serviceKey"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public override LightHealth.HealthCheck HealthCheck(string serviceKey, string value)
        {
            try
            {
                var x = _client.OpenAsync();
                return LightHealth.HealthCheck.Healthy;
            }
            catch
            {
                return LightHealth.HealthCheck.Unhealthy;
            }
        }

        /// <summary>
        /// Implement a condition optimistic. In this case the value eTag will be used for control on optimist concurrency.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="model"></param>
        /// <returns></returns>
        public override dynamic AccessConditionOptimistic<T>(T model)
        {
            if (base.IsOptimisticConcurrency(typeof(LightOptimisticModel<>), typeof(T)))
            {
                object eTag = model.GetType().BaseType.GetProperty("ETag").GetValue(model);

                //Check obj is different null for convert to string.
                if (eTag != null)
                {
                    return new RequestOptions() { AccessCondition = new AccessCondition() { Condition = (string)eTag, Type = AccessConditionType.IfMatch } };
                }

            }
            return new RequestOptions() { AccessCondition = new AccessCondition() { Type = AccessConditionType.IfNoneMatch } };
        }

        /// <summary>
        /// Get Azure Cosmos DB Connection
        /// </summary>
        /// <returns></returns>
        public DocumentClient GetConnection()
        {
            return new DocumentClient(new Uri(_endpoint), _authKey, _connPolicy);
        }

        /// <summary>
        /// Get or Create a Database if not found.
        /// </summary>
        /// <returns></returns>
        public async Task<Database> GetOrCreateDatabaseAsync()
        {
            Database database = _client.CreateDatabaseQuery()
                .Where(db => db.Id == _databaseId).AsEnumerable().FirstOrDefault();

            if (database == null)
            {
                if (config.CreateIfNotExists)
                {
                    database = await _client.CreateDatabaseAsync(new Database { Id = _databaseId }, new RequestOptions { OfferThroughput = config.DatabaseRUs });
                }
                else
                {
                    throw new LightException($"DatabaseId on CosmosDB settings does not exists in Database [{config.Endpoint}].");
                }

            }

            return database;
        }

        /// <summary>
        /// Get or Create Collection if not found.
        /// </summary>
        /// <returns></returns>
        public async Task<DocumentCollection> GetOrCreateCollectionAsync()
        {
            DocumentCollection collection = _client.CreateDocumentCollectionQuery((await _database).SelfLink).Where(c => c.Id == _collectionName).AsEnumerable().FirstOrDefault();

            if (collection == null && config.CreateIfNotExists)
            {
                collection = newDocumentCollection(_collectionName);
                collection = await _client.CreateDocumentCollectionAsync((await _database).SelfLink, collection);
            }

            return collection;
        }

        /// <summary>
        /// Inject that target of entity
        /// </summary>
        /// <param name="T"></param>
        private void InjectTarget(Type T)
        {
            this._typeToCollectionMap.TryGetValue(T, out LightLazy<DocumentCollection> documentColletionRuntime);

            if (documentColletionRuntime == null)
            {
                _collectionName = T.Name;
                var _collection = new LightLazy<DocumentCollection>(async () => await GetOrCreateCollectionAsync());
                
                this._typeToCollectionMap.TryAdd(T, _collection);
            }
            else
                _collectionName = T.Name;

        }

        /// <summary>
        /// Return Environment Name
        /// </summary>
        /// <param name="jsonString">string contained the tags</param>
        /// <returns>Return the string with replace Tags</returns>
        public string ConvertDynamicTags(string jsonString)
        {
            DateTime today = DateTime.Today;
            DateTime now = DateTime.UtcNow;

            jsonString = jsonString.Replace("{{TODAY}}", today.ToString("yyyy-MM-dd"));
            jsonString = jsonString.Replace("{{NOW}}", now.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"));

            MatchCollection todays = Regex.Matches(jsonString, @"\{{TODAY[\-|\+][0-9]+}}", RegexOptions.None);
            foreach (Match item in todays)
            {
                DateTime newDate = today.AddDays(Double.Parse(Regex.Match(item.Value, @"[\-|\+][0-9]+").Value));
                jsonString = jsonString.Replace(item.Value, newDate.ToString("yyyy-MM-dd"));
            }

            MatchCollection nows = Regex.Matches(jsonString, @"\{{NOW[\-|\+][0-9]+}}", RegexOptions.None);
            foreach (Match item in nows)
            {
                DateTime newDate = now.AddMinutes(Double.Parse(Regex.Match(item.Value, @"[\-|\+][0-9]+").Value));
                jsonString = jsonString.Replace(item.Value, newDate.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"));
            }

            return jsonString;
        }

    }
}