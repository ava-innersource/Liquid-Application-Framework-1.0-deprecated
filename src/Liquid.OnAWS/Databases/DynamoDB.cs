using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Liquid.Base;
using Liquid.Interfaces;
using Liquid.Repository;
using Liquid.Runtime.Configuration.Base;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Liquid.OnAWS
{
    /// <summary>
    /// Provides a unique namespace to store and access your DynamoDB data objects.
    /// </summary>
    public class DynamoDB : LightRepository, ILightDataBase<AmazonDynamoDBClient, object, CreateTableResponse>
    {     
        private AmazonDynamoDBClient DynamoDbClient { get; set; }
        private DynamoDBContext DynamoDbContext { get; set; }
        private readonly string _suffixName;
        private DynamoDBConfiguration config;
        private ILightMediaStorage _mediaStorage { get; set; }
        private readonly ILightEvaluate _evaluatePredicate = new EvaluateExpression();
        /// <summary>
        /// Creates a DynamoDB instance from default settings provided on appsettings.
        /// </summary>
        public DynamoDB() : this("") { }

        /// <summary>
        /// Creates a DynamoDB instance from specific settings to a given name provided on appsettings.
        /// The configuration should be provided like "DynamoDB_{sufixName}".
        /// </summary>
        /// <param name="suffixName">Name of configuration</param>
        public DynamoDB(string suffixName)
        {
            this._suffixName = suffixName;
        }
        /// <summary>
        /// Initialize Dynamo DB repository
        /// </summary>
        public override void Initialize()
        {
            //Change collection in Runtime
            base.TargetVarience(InjectTarget);
            if (string.IsNullOrEmpty(this._suffixName)) // Load specific settings if provided
                this.config = LightConfigurator.Config<DynamoDBConfiguration>($"{nameof(DynamoDB)}");
            else
                this.config = LightConfigurator.Config<DynamoDBConfiguration>($"{nameof(DynamoDB)}_{this._suffixName}");

            ///Initialize DynamoDB instance with provided settings            
            LoadConfigurations();
        }

        /// <summary>
        /// LoadCre Configutaion variables
        /// </summary>
        private void LoadConfigurations()
        {
            _setup = true;
            DynamoDbClient = GetConnection();
            DynamoDbContext = DynamoDbContext ?? new DynamoDBContext(DynamoDbClient, new DynamoDBContextConfig() { ConsistentRead = true });
        }
        /// <summary>
        /// Inject that target of entity
        /// </summary>
        /// <param name="T"></param>
        private void InjectTarget(Type T)
        {
            CreateTableResponse createTableResponse = default(CreateTableResponse);

            if (string.IsNullOrEmpty(this.DynamoDbClient.ListTablesAsync().Result.TableNames.Where(x => x.Equals(T.Name)).FirstOrDefault()))
            {
                createTableResponse = this.CreateDynamicTable(T.Name).Result;                
            }   
        }

        public async Task<CreateTableResponse> CreateDynamicTable(string name)
        {
            var createTableRequest = new CreateTableRequest
            {
                TableName = name,
                AttributeDefinitions = new List<AttributeDefinition>(),
                KeySchema = new List<KeySchemaElement>(),
                GlobalSecondaryIndexes = new List<GlobalSecondaryIndex>(),
                LocalSecondaryIndexes = new List<LocalSecondaryIndex>(),
                ProvisionedThroughput = new ProvisionedThroughput
                {
                    ReadCapacityUnits = 1,
                    WriteCapacityUnits = 1
                }
            };
            createTableRequest.KeySchema = new[]
            {
                new KeySchemaElement
                {
                    AttributeName = "Id",
                    KeyType = KeyType.HASH,
                },
            }.ToList();

            createTableRequest.AttributeDefinitions = new[]
            {
                new AttributeDefinition
                {
                    AttributeName = "Id",
                    AttributeType = ScalarAttributeType.S,
                }
            }.ToList();

            return await this.DynamoDbClient.CreateTableAsync(createTableRequest);
        }
        public AmazonDynamoDBClient GetConnection()
        {
            return new AmazonDynamoDBClient(this.config.AccessKeyID, this.config.SecretAccessKey, new AmazonDynamoDBConfig
            {
                ServiceURL = config.ServiceURL,
                UseHttp = config.UseHttp
            });
        }
        /// <summary>
        /// Calls the base class because there may be some generic behavior in it
        /// Get the Generic object
        /// </summary>
        /// <typeparam name="T">generic type</typeparam>
        /// <returns>The generic object</returns>
        public override async Task<IQueryable<T>> GetAsync<T>()
        {
            await base.GetAsync<T>();

            var result = await DynamoDbContext.ScanAsync<T>(new List<ScanCondition>()).GetRemainingAsync();

            return result.AsQueryable();
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

            return await DynamoDbContext.LoadAsync<T>(entityId);  
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

            List<ScanCondition> conditions = new List<ScanCondition>();

            IEnumerable<ExpressionAnalyzed> expressions = (IEnumerable<ExpressionAnalyzed>) _evaluatePredicate.Evaluate<T>(predicate);

            expressions.ToList().ForEach(x => {

                conditions.Add(new ScanCondition(x.PropertyName, this.ConvertEnum(x.OperatorBetweenPropAndValue.ExpressionType), x.PropertyValue ));

            });

            var result = await DynamoDbContext.ScanAsync<T>(conditions).GetRemainingAsync();

            return result.AsQueryable();
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
            await base.AddOrUpdateAsync<T>(model);

            await DynamoDbContext.SaveAsync(model);

            return await Task.FromResult<T>(model);
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

            var itemBatch = DynamoDbContext.CreateBatchWrite<T>();        

            listModels.ToList().ForEach(x => itemBatch.AddPutItem(x));

            await itemBatch.ExecuteAsync();

            return listModels;    
        }
  

        /// <summary>
        /// Calls the base class because there may be some generic behavior in it
        /// Delete the specific document
        /// </summary>
        /// <param name="entityId">the id of the document</param>
        /// <returns></returns>
        public override async Task DeleteAsync<T>(string entityId)
        {
            await this.DynamoDbContext.DeleteAsync<T>(entityId);
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
            string continuationToken = null;
            IQueryable<T> queryBase = null;
            if (filter != null) queryBase = queryBase.Where(filter);
            var finalQuery = default(dynamic);

            int queryPage = 0;

            if (finalQuery != null)
            {
                while (finalQuery.HasMoreResults && queryPage <= page)
                {
                    var data = await finalQuery.ExecuteNextAsync<T>();
                    if (queryPage++ == page)
                    {
                        continuationToken = data.ResponseContinuation;
                        list.AddRange(data.ToList());
                    }
                }
            }

            LightPaging<T> result = new LightPaging<T>()
            {
                Data = list,
                Page = page + 1,
                ItemsPerPage = itemsPerPage,
                TotalCount = await CountAsync<T>(),
                ContinuationToken = continuationToken
            };

            result.TotalPages = result.TotalCount / itemsPerPage;
            if (result.TotalCount % itemsPerPage > 0) result.TotalPages++;

            return result;
        }

        private ScanOperator ConvertEnum(ExpressionType type)
        {
            switch (type)
            {
                case ExpressionType.NotEqual:
                    return ScanOperator.NotEqual;
                    
                case ExpressionType.Equal:
                    return ScanOperator.Equal;

                case ExpressionType.GreaterThan:
                    return ScanOperator.GreaterThan;

                case ExpressionType.GreaterThanOrEqual:
                    return ScanOperator.GreaterThanOrEqual;

                case ExpressionType.LessThan:
                    return ScanOperator.GreaterThan;

                case ExpressionType.LessThanOrEqual:
                    return ScanOperator.LessThanOrEqual;

                default:
                    throw new LightException($"Type: {type} not recognized in workbench");
            }
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

           return this.DynamoDbContext.ScanAsync<T>(new List<ScanCondition>()).GetRemainingAsync().Result.Count;
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

            await base.CountAsync<T>(predicate);

            List<ScanCondition> conditions = new List<ScanCondition>();

            IEnumerable<ExpressionAnalyzed> expressions = (IEnumerable<ExpressionAnalyzed>)_evaluatePredicate.Evaluate<T>(predicate);

            expressions.ToList().ForEach(x => {

                conditions.Add(new ScanCondition(x.PropertyName, this.ConvertEnum(x.OperatorBetweenPropAndValue.ExpressionType), x.PropertyValue));

            });

            var result = await DynamoDbContext.ScanAsync<T>(conditions).GetRemainingAsync();

            return result.Count;
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
        /// Calls the base class because there may be some generic behavior in it 
        /// Deletes the especific attachment
        /// </summary>
        /// <param name="entityId"> id of the document</param>
        /// <param name="fileName">id of the attachment</param>
        public override async Task DeleteAttachmentAsync<T>(string entityId, string fileName)
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
        public override async Task<ILightPaging<T>> GetByPageAsync<T>(string token, Expression<Func<T, bool>> filter, int page, int itemsPerPage)
        {
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
            throw new NotImplementedException();
        }

        /// <summary>
        /// Calls the base class because there may be some generic behavior in it
        /// This method creates a query for documents under a collection
        /// </summary>
        /// <typeparam name="T">generic type</typeparam>
        /// <typeparam name="T">generic type</typeparam>
        /// <param name="query">query to create the document</param>
        /// <returns></returns>
        public override async Task<IEnumerable<T>> QueryAsync<T>(string query)
        {          

            throw new NotImplementedException();

        }

        public Task<CreateTableResponse> GetOrCreateCollectionAsync()
        {
            throw new NotImplementedException();
        }

        public Task<object> GetOrCreateDatabaseAsync()
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
            DynamoDbClient.ListTablesAsync().Result.TableNames.ForEach(async x =>
            {
                var request = new DeleteTableRequest
                {
                    TableName = x
                };

               await  DynamoDbClient.DeleteTableAsync(request);

            });

            return true;
        }

        /// <summary>
        /// Creates a Collection if doesn't exisists
        /// </summary>
        /// <returns></returns>
        public override Task CreateCollectionIfNotExistsAsync(string database, string collection)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// Creates a new database if doesn't exisists
        /// </summary>
        /// <returns></returns>
        public override Task CreateDatabaseIfNotExistsAsync(string database, string collection)
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
            if (base.IsOptimisticConcurrency(typeof(LightOptimisticModel<>), typeof(T)))
            {
                object eTag = model.GetType().BaseType.GetProperty("ETag").GetValue(model);            

            }
            throw new NotImplementedException();
        }


        /// <summary>
        /// Method to run Health Check for Dynamo DB Repository
        /// </summary>
        /// <param name="serviceKey"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public override LightHealth.HealthCheck HealthCheck(string serviceKey, string value)
        {
            try
            {
                var c = DynamoDbClient.ListTablesAsync();
                return LightHealth.HealthCheck.Healthy;
            }
            catch
            {
                return LightHealth.HealthCheck.Unhealthy;
            }   
        }
    }
}