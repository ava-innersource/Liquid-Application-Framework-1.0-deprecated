using Google.Cloud.Firestore;
using Liquid.Interfaces;
using Liquid.Repository;
using Liquid.Runtime.Configuration.Base;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Reflection;
using System.Text;

namespace Liquid.OnGoogle
{
    /// <summary>
    /// Provides a unique namespace to store and access your Google FireStore objects.
    /// </summary>
    public class GoogleFireStore : LightRepository, ILightDataBase<FirestoreDb, FirestoreDb, CollectionReference>
    {
        private string _collectionName;
        private static FirestoreDb _client;
        private string projectID = "amaw-216213";
        public string databaseID = "";
        private readonly string _suffixName;
        private CollectionReference _collection;
        private ILightMediaStorage _mediaStorage;
        private ILightEvaluate _evaluatePredicate = new EvaluateExpression();
        private readonly IDictionary<Type, CollectionReference> _collectionsRegistred = new Dictionary<Type, CollectionReference>();
        public GoogleFireStoreConfiguration Config { get; private set; }

        /// <summary>
        /// Creates a Firestore instance from default settings provided on appsettings.
        /// </summary>
        public GoogleFireStore() : this("")
        {

        }

        /// <summary>
        /// Creates a Firestore instance from specific settings to a given name provided on appsettings.
        /// The configuration should be provided like "GoogleFirestore_{sufixName}".
        /// 
        /// </summary>
        /// <param name="suffixName">Name of configuration</param>
        public GoogleFireStore(string suffixName)
        {
            _suffixName = suffixName;
        }

        /// <summary>
        /// Initialize Cosmos DB repository
        /// </summary>
        public override void Initialize()
        {
            //Change collection in Runtime
            base.TargetVarience(InjectTarget);

            if (string.IsNullOrEmpty(this._suffixName)) // Load specific settings if provided
                this.Config = LightConfigurator.Config<GoogleFireStoreConfiguration>($"{nameof(GoogleFireStore)}");
            else
                this.Config = LightConfigurator.Config<GoogleFireStoreConfiguration>($"{nameof(GoogleFireStore)}_{this._suffixName}");

            ///Initialize CosmosDB instance with provided settings            
            LoadConfigurationsAsync();
        }

        /// <summary>
        /// Load Configutaion variables
        /// </summary>
        private async void LoadConfigurationsAsync()
        {
            _setup = true;
            _collectionName = this.Config.CollectionName;
            projectID = this.Config.ProjectID;
            databaseID = this.Config.DatabaseID;
            _client = FirestoreDb.Create(projectID);
            _collection = await GetOrCreateCollectionAsync();
        }

        /// <summary>
        /// Inject that target of entity
        /// </summary>
        /// <param name="T"></param>
        private async void InjectTarget(Type T)
        {
            this._collectionsRegistred.TryGetValue(T, out CollectionReference documentColletionRuntime);

            if (documentColletionRuntime == null)
            {
                _collectionName = T.Name;
                _collection = await GetOrCreateCollectionAsync();

                this._collectionsRegistred.Add(T, _collection);
            }
            else
            {
                _collectionName = T.Name;
                _collection = documentColletionRuntime;
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
            object eTag = null;
            if (base.IsOptimisticConcurrency(typeof(LightOptimisticModel<>), typeof(T)))
                eTag = model.GetType().BaseType.GetProperty("ETag").GetValue(model);

            return eTag;
        }

        /// <summary>
        /// Get FireStore Connection
        /// </summary>
        /// <returns></returns>
        public FirestoreDb GetConnection()
        {
            return FirestoreDb.Create(projectID);
        }

        /// <summary>
        /// Get or Create Collection
        /// </summary>
        /// <returns></returns>
        public async Task<CollectionReference> GetOrCreateCollectionAsync()
        {
            return await Task.Run(() => _client.Collection(_collectionName));
        }

        /// <summary>
        /// Method for reset data in database, to seed data.
        /// </summary>
        /// <param name="query">Files</param>
        /// <returns></returns>
        public override bool ResetData(string query)
        {
            string database = this.Config.DatabaseID;
            bool success = false;
            string[] parts = new string[0];
            if (!string.IsNullOrEmpty(query))
                parts = query.Split(new char[] { ',' });
            else
            {
                parts = GetFilesRessed();
            }

            foreach (var fileEntityName in parts)
            {
                var json = base.GetMockData(database, fileEntityName);
                DeleteCollection();
                InsertData(json, fileEntityName, ref success);
            }

            return success;
        }

        /// <summary>
        /// Insert seed data on collections
        /// </summary>
        /// <param name="json">Json file data</param>
        /// <param name="success">True or False</param>
        private void InsertData(dynamic json, string collection, ref bool success)
        {
            _client.Collection(collection).AddAsync(json);
            success = true;
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
        /// Get or create a Database on Google FireStore
        /// But it isn't necessary on Firestore
        /// </summary>
        /// <returns></returns>
        public Task<FirestoreDb> GetOrCreateDatabaseAsync()
        {
            // Don't necessary for Google FireStore
            throw new NotImplementedException();
        }

        /// <summary>
        /// Get or create a Database on Google FireStore
        /// But it isn't necessary on Firestore
        /// </summary>
        /// <param name="database"></param>
        /// <param name="collection"></param>
        /// <returns></returns>
        public override Task CreateCollectionIfNotExistsAsync(string database, string collection)
        {
            // Don't necessary for Google FireStore
            throw new NotImplementedException();
        }

        /// <summary>
        /// Create a Database on Google FireStore
        /// But it isn't necessary on Firestore
        /// </summary>
        /// <param name="database"></param>
        /// <param name="collection"></param>
        /// <returns></returns>
        public override Task CreateDatabaseIfNotExistsAsync(string database, string collection)
        {
            // Don't necessary for Google FireStore
            throw new NotImplementedException();
        }

        /// <summary>
        /// Calls the base class because there may be some generic behavior in it
        /// This method creates a query for documents under a collection
        /// But Google Firestore haven't this capability
        /// </summary>
        /// <param name="query">SQL statement with parameterized values</param>
        /// <returns>JSON object</returns>
        public override async Task<JObject> QueryAsyncJson<T>(string query)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Calls the base class because there may be some generic behavior in it
        /// This method creates a query for documents under a collection
        /// But Google Firestore haven't this capability
        /// </summary>
        /// <typeparam name="T">generic type</typeparam>
        /// <param name="query">query to create the document</param>
        /// <returns></returns>
        public override async Task<IEnumerable<T>> QueryAsync<T>(string query)
        {
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

            List<T> list = new List<T>();
            Query qry = _collection.OrderBy("Id");
            IEnumerable<ExpressionAnalyzed> expressions = (IEnumerable<ExpressionAnalyzed>)_evaluatePredicate.Evaluate<T>(filter);
            expressions.ToList().ForEach(x =>
            {
                switch (x.OperatorBetweenPropAndValue.ExpressionType)
                {
                    case ExpressionType.NotEqual:
                        qry.WhereEqualTo(x.PropertyName, x.PropertyValue);
                        break;
                    case ExpressionType.Equal:
                        qry.WhereEqualTo(x.PropertyName, x.PropertyValue);
                        break;
                    case ExpressionType.GreaterThan:
                        qry.WhereGreaterThan(x.PropertyName, x.PropertyValue);
                        break;
                    case ExpressionType.GreaterThanOrEqual:
                        qry.WhereGreaterThanOrEqualTo(x.PropertyName, x.PropertyValue);
                        break;
                    case ExpressionType.LessThan:
                        qry.WhereLessThan(x.PropertyName, x.PropertyValue);
                        break;
                    case ExpressionType.LessThanOrEqual:
                        qry.WhereLessThanOrEqualTo(x.PropertyName, x.PropertyValue);
                        break;
                    default:
                        qry.WhereEqualTo(x.PropertyName, x.PropertyValue);
                        break;
                }
            });
            qry.StartAfter((page == 1 ? 0 : page * itemsPerPage)).Limit(itemsPerPage);

            QuerySnapshot ret = await qry.GetSnapshotAsync();
            foreach (var item in ret.Documents)
            {
                list.Add(HydrateEntity<T>(item));
            }

            LightPaging<T> result = new LightPaging<T>()
            {
                Data = list,
                Page = page + 1,
                ItemsPerPage = itemsPerPage,
                TotalCount = await CountAsync<T>(),
                ContinuationToken = ""
            };

            result.TotalPages = result.TotalCount / itemsPerPage;
            if (result.TotalCount % itemsPerPage > 0) result.TotalPages++;

            return result;
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

            Query qry = _collection.WhereArrayContains("Id", entityId);
            QuerySnapshot ret = await qry.GetSnapshotAsync();
            return HydrateEntity<T>(ret.Documents.FirstOrDefault());
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
            DocumentReference upsertedDoc;

            if (string.IsNullOrEmpty(((ILightModel)model).id))
                upsertedDoc = await _collection.AddAsync(model);
            else
            {
                upsertedDoc = _collection.Document(((ILightModel)model).id);
                var updt = upsertedDoc.SetAsync(model, SetOptions.MergeAll);
            }

            return JsonConvert.DeserializeObject<T>(upsertedDoc.ToString());
        }

        /// <summary>
        /// Add or update the document
        /// Calls the base class because there may be some generic behavior in it
        /// </summary>
        /// <typeparam name="T">generic type</typeparam>
        /// <param name="listModels">list generic models</param>
        /// <returns>The upserted Entity</returns>
        public override async Task<IEnumerable<T>> AddOrUpdateAsync<T>(List<T> listModels)
        {
            await base.AddOrUpdateAsync(listModels);
            StringBuilder ListupsertedDoc = new StringBuilder();
            DocumentReference upsertedDoc;
            foreach (var model in listModels)
            {
                if (string.IsNullOrEmpty(((ILightModel)model).id))
                    upsertedDoc = await _collection.AddAsync(model);
                else
                {
                    upsertedDoc = _collection.Document(((ILightModel)model).id);
                    var updt = upsertedDoc.SetAsync(model, SetOptions.MergeAll);
                }
                ListupsertedDoc.Append(upsertedDoc.ToString());
            }
            return JsonConvert.DeserializeObject<IEnumerable<T>>(ListupsertedDoc.ToString());
        }

        /// <summary>
        /// Google Cloud Firestore doesn't support attachments
        /// </summary>
        /// <param name="entityId">document id</param>
        /// <param name="fileName">name of the file</param>
        /// <param name="attachment">the attachment</param>
        /// <returns></returns>
        public override async Task<ILightAttachment> AddOrUpdateAttachmentAsync<T>(string entityId, string fileName, Stream attachment)
        {
            await base.AddOrUpdateAttachmentAsync<T>(entityId, fileName, attachment); //Calls the base class because there may be some generic behavior in it
            ILightAttachment upserted = GetLightAttachment(entityId, fileName, attachment);

            if (_mediaStorage != null)
                _mediaStorage.InsertUpdateAsync(upserted);

            return upserted;
        }

        /// <summary>
        /// Google Firestore don't allows you to store binary blobs/media 
        /// and need to your own remote media store. 
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
        /// Get Attachments
        /// But Google Cloud Firestore doesn't support attachments
        /// </summary>
        /// <param name="entityId">attachment id</param>
        /// <param name="fileName">name of the file</param>
        /// <returns>The attachment type LightAttachment</returns>
        public override async Task<ILightAttachment> GetAttachmentAsync<T>(string entityId, string fileName)
        {
            await base.GetAttachmentAsync<T>(entityId, fileName);

            ILightAttachment lightAttachment = new LightAttachment();
            if (_mediaStorage != null)
            {
                var attachment = _mediaStorage.GetAsync("", "");
                ILightAttachment lightAttachmentStorage = await _mediaStorage.GetAsync(entityId, fileName);
                lightAttachment.MediaStream = lightAttachmentStorage.MediaStream;
            }
            else
                lightAttachment = null;

            return lightAttachment;
        }

        /// <summary>
        /// List all attachments
        /// But Google Cloud Firestore doesn't support attachments
        /// </summary>
        /// <param name="entityId">attatchment id</param>
        /// <returns>A list of attachments</returns>
        public override async Task<IEnumerable<ILightAttachment>> ListAttachmentsByIdAsync<T>(string entityId)
        {
            throw new NotImplementedException();
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

            Query qry = _collection.OrderBy("Id");
            QuerySnapshot ret = await qry.GetSnapshotAsync();
            return ret.Count;
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

            Query qry = _collection.OrderBy("Id");
            IEnumerable<ExpressionAnalyzed> expressions = (IEnumerable<ExpressionAnalyzed>)_evaluatePredicate.Evaluate<T>(predicate);
            expressions.ToList().ForEach(x =>
            {
                switch (x.OperatorBetweenPropAndValue.ExpressionType)
                {
                    case ExpressionType.NotEqual:
                        qry.WhereEqualTo(x.PropertyName, x.PropertyValue);
                        break;
                    case ExpressionType.Equal:
                        qry.WhereEqualTo(x.PropertyName, x.PropertyValue);
                        break;
                    case ExpressionType.GreaterThan:
                        qry.WhereGreaterThan(x.PropertyName, x.PropertyValue);
                        break;
                    case ExpressionType.GreaterThanOrEqual:
                        qry.WhereGreaterThanOrEqualTo(x.PropertyName, x.PropertyValue);
                        break;
                    case ExpressionType.LessThan:
                        qry.WhereLessThan(x.PropertyName, x.PropertyValue);
                        break;
                    case ExpressionType.LessThanOrEqual:
                        qry.WhereLessThanOrEqualTo(x.PropertyName, x.PropertyValue);
                        break;
                    default:
                        qry.WhereEqualTo(x.PropertyName, x.PropertyValue);
                        break;
                }
            });

            QuerySnapshot ret = await qry.GetSnapshotAsync();
            return ret.Count;
        }

        /// <summary>
        /// Delete attachment
        /// But Google Cloud Firestore doesn't support attachments
        /// </summary>
        /// <param name="entityId"> id of the document</param>
        /// <param name="fileName">id of the attachment</param>
        public override Task DeleteAttachmentAsync<T>(string entityId, string fileName)
        {
            return _mediaStorage.Remove(GetLightAttachment(entityId, fileName, null));
        }

        /// <summary>
        /// Calls the base class because there may be some generic behavior in it
        /// Delete the specific document
        /// </summary>
        /// <param name="entityId">the id of the document</param>
        /// <returns></returns>
        public override async Task DeleteAsync<T>(string entityId)
        {
            Query qry = _collection.WhereEqualTo("Id", entityId);
            QuerySnapshot ret = await qry.GetSnapshotAsync();
            foreach (DocumentSnapshot document in ret.Documents)
            {
                await document.Reference.DeleteAsync();
            }

            await base.DeleteAsync<T>(entityId);
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

            Query qry = _collection.OrderBy("Id");
            IEnumerable<ExpressionAnalyzed> expressions = (IEnumerable<ExpressionAnalyzed>)_evaluatePredicate.Evaluate<T>(predicate);
            expressions.ToList().ForEach(x =>
            {
                switch (x.OperatorBetweenPropAndValue.ExpressionType)
                {
                    case ExpressionType.NotEqual:
                        qry.WhereEqualTo(x.PropertyName, x.PropertyValue);
                        break;
                    case ExpressionType.Equal:
                        qry.WhereEqualTo(x.PropertyName, x.PropertyValue);
                        break;
                    case ExpressionType.GreaterThan:
                        qry.WhereGreaterThan(x.PropertyName, x.PropertyValue);
                        break;
                    case ExpressionType.GreaterThanOrEqual:
                        qry.WhereGreaterThanOrEqualTo(x.PropertyName, x.PropertyValue);
                        break;
                    case ExpressionType.LessThan:
                        qry.WhereLessThan(x.PropertyName, x.PropertyValue);
                        break;
                    case ExpressionType.LessThanOrEqual:
                        qry.WhereLessThanOrEqualTo(x.PropertyName, x.PropertyValue);
                        break;
                    default:
                        qry.WhereEqualTo(x.PropertyName, x.PropertyValue);
                        break;
                }
            });
            QuerySnapshot ret = await qry.GetSnapshotAsync();
            return HydrateEntityQueryable<T>(ret.Documents);
        }

        /// <summary>
        /// Delete all documents from collection
        /// </summary>
        private async void DeleteCollection()
        {
            QuerySnapshot snapshot = await _collection.Limit(1000).GetSnapshotAsync();
            IReadOnlyList<DocumentSnapshot> documents = snapshot.Documents;
            while (documents.Count > 0)
            {
                foreach (DocumentSnapshot document in documents)
                {
                    await document.Reference.DeleteAsync();
                }
                snapshot = await _collection.Limit(1000).GetSnapshotAsync();
                documents = snapshot.Documents;
            }
        }

        /// <summary>
        /// Fill a entity with DocumentSnapshot data
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="document"></param>
        /// <returns></returns>
        private T HydrateEntity<T>(DocumentSnapshot document)
        {
            return GetMatchingProperties<T>(document);
        }

        /// <summary>
        /// Fill a list of entity with DocumentSnapshot data
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="documents"></param>
        /// <returns></returns>
        private IQueryable<T> HydrateEntityQueryable<T>(IReadOnlyList<DocumentSnapshot> documents)
        {
            List<T> entityList = new List<T>();
            foreach (var doc in documents)
            {
                entityList.Add(GetMatchingProperties<T>(doc));
            }

            return entityList as IQueryable<T>;
        }

        /// <summary>
        /// Get properties values of Document
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <returns></returns>
        private T GetMatchingProperties<T>(DocumentSnapshot data)
        {
            T obj = (T)Activator.CreateInstance(typeof(T));

            foreach (PropertyInfo propertyInfo in data.GetType().GetProperties())
            {
                dynamic value = propertyInfo.GetValue(data);
                if (value != null)
                {
                    PropertyInfo field = GetPropertyByNameAndType(obj, propertyInfo.Name, propertyInfo.PropertyType.Name);
                    if (field != null)
                        field.SetValue(this, value);
                }
            }

            return obj;
        }

        /// <summary>
        /// Get a Name and Type of Property
        /// </summary>
        /// <param name="data"></param>
        /// <param name="name"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        private PropertyInfo GetPropertyByNameAndType(dynamic data, String name, String type)
        {
            PropertyInfo retorno = null;
            ///By reflection, browse viewModel by identifying all attributes and lists for validation.  
            foreach (PropertyInfo propertyInfo in data.GetType().GetProperties())
            {
                if (propertyInfo.Name.Equals(name) && propertyInfo.PropertyType.Name.Equals(type))
                {
                    retorno = propertyInfo;
                    break;
                }
            }
            return retorno;
        }

        /// <summary>
        /// Method to run Health Check for Google Firestore Repository
        /// </summary>
        /// <param name="serviceKey"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public override LightHealth.HealthCheck HealthCheck(string serviceKey, string value)
        {
            try
            {
                var x = _client.StartBatch();
                return LightHealth.HealthCheck.Healthy;
            }
            catch
            {
                return LightHealth.HealthCheck.Unhealthy;
            }
        }
    }
}
