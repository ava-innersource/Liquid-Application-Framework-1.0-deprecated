using Liquid.Domain.Base;
using Liquid.Activation;
using Liquid.Interfaces;
using Liquid.Runtime;
using Liquid.Runtime.Telemetry;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.Encodings.Web;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Liquid.Repository
{
    /// <summary>
    /// The Cosmos DB LightRepository
    /// This class is a wrapper around CosmosDBRepository setting up the client to make calls to Cosmos DB. 
    /// </summary>
    public abstract class LightRepository : ILightRepository
    {
        /// <summary>
        /// Constant to Insert or Update
        /// </summary>
        public const string InsertOrUpdateOperation = "InsertOrUpdate";
        /// <summary>
        /// Constant to Delete
        /// </summary>
        public const string DeleteOperation = "Delete";
        /// <summary>
        /// Disable Validation of Model
        /// </summary>
        public bool IgnoreValidate = false;
        protected string _dataOperation { get; set; }
        /// <summary>
        /// Set Media Storage for attachments
        /// </summary>
        /// <param name="mediaStorage"></param>
        public abstract void SetMediaStorage(ILightMediaStorage mediaStorage);

        protected bool _setup = false;
        /// <summary>
        /// Wrapper when change the collection name
        /// </summary>
        private Action<Type> _action { get; set; }

        public abstract void Initialize();
        /// <summary>
        /// Check if Cosmos DB Repository has been correcty setup 
        /// </summary>
        protected void CheckSetup<T>()
        {
            //Change the collection in runtime
            this.ExecuteVarience<T>();

            if (!_setup)
            {
                throw new BadRepositoryInitializationException(this.GetType().Name);
            }
        }

        /// <summary>
        /// Receive a action from a child and then will be executed in Runtime
        /// </summary>
        /// <param name="action">Wrapper on method child</param>
        protected void TargetVarience(Action<Type> action)
        {
            this._action = action;
        }

        /// <summary>
        /// Execute the method inside on child when the colletion was been change.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        private void ExecuteVarience<T>()
        {
            this._action(typeof(T));
        }

        /// <summary>
        /// Method to validate and check rules
        /// </summary>
        /// <param name="model"></param>
        private void ValidateAndCheck(dynamic model)
        {
            if (!IgnoreValidate)
            {
                List<string> _modelValidationErrors = new List<string>();
                Validate(model, _modelValidationErrors);
                if (_modelValidationErrors.Count > 0)
                {
                    throw new InvalidModelException(_modelValidationErrors);
                }
            }
        }

        /// <summary>
        /// Method to validate and check rules
        /// </summary>
        /// <param name="model"></param>
        private void ValidateAndCheckList(dynamic model)
        {
            if (!IgnoreValidate)
            {
                foreach (var x in model)
                {
                    ValidateAndCheck(x);
                }
            }
        }

        /// <summary>
        /// Evaluates the validation rules of the Model class (and its agregrates) and raise errors accordingly.
        /// </summary>
        /// <param name="model">The Model to input validation</param>
        private void Validate(dynamic model, List<string> _modelValidationErrors)
        {
            model.InputErrors = _modelValidationErrors;
            model.Validate();
            ResultValidation result = model.Validator.Validate(model);
            if (!result.IsValid)
            {
                foreach (var error in result.Errors)
                {
                    _modelValidationErrors.Add(error);
                }
            }
            //By reflection, browse viewModel by identifying all attributes and lists for validation.  
            foreach (PropertyInfo fieldInfo in model.GetType().GetProperties())
            {
                dynamic child = fieldInfo.GetValue(model);

                //Encoding of Special Characters
                if (child != null && child.GetType() == typeof(string))
                {
                    if (Regex.IsMatch((string)child, (@"[^a-zA-Z0-9]")))
                    {
                        var encoder = HtmlEncoder.Create(allowedRanges: new[] {
                            System.Text.Unicode.UnicodeRanges.BasicLatin,
                            System.Text.Unicode.UnicodeRanges.Latin1Supplement });

                        child = encoder.Encode(child);
                        fieldInfo.SetValue(model, child);
                    }
                }
                //When the child is a list, validate each of its members  
                if (child is IList)
                {
                    var children = (IList)fieldInfo.GetValue(model);
                    foreach (var item in children)
                    {

                        //Check, if the property is a Light ViewModel, only they will validation Lights ViewModel
                        if ((item.GetType().BaseType != typeof(object))
                             && (item.GetType().BaseType != typeof(System.ValueType))
                             && (child.GetType().BaseType.IsGenericType &&
                                item.GetType().BaseType.GetGenericTypeDefinition() == typeof(LightModel<>)))
                        {
                            dynamic obj = item;
                            //Check, if the attribute is null for verification of the type.
                            if (obj != null)
                                Validate(obj, _modelValidationErrors);
                        }
                    }
                }
                else
                {
                    //Otherwise, validate the very child once. 
                    if (child != null)
                    {
                        //Check, if the property is a Light ViewModel, only they will validation Lights ViewModel
                        if ((child.GetType().BaseType != typeof(object))
                           && (child.GetType().BaseType != typeof(System.ValueType))
                            && (child.GetType().BaseType.IsGenericType &&
                            child.GetType().BaseType.GetGenericTypeDefinition() == typeof(LightModel<>)))
                        {
                            Validate(child, _modelValidationErrors);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Add or update data
        /// </summary>
        /// <typeparam name="T">Generic Type</typeparam>
        /// <param name="model">Domain Model</param>
        /// <returns>Returns the result of the operation</returns>
        public virtual Task<T> AddOrUpdateAsync<T>(T model)
        {
            CheckSetup<T>();  //Prevents direct instantiation of a repository without a sufix
            ValidateAndCheck(model);

            // Inform the data operation to LightEvent
            this._dataOperation = LightRepository.InsertOrUpdateOperation;

            return Task.FromResult<T>(model);
        }

        /// <summary>
        /// Add or update data
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="listModels"></param>
        /// <returns>Returns the result of the operation</returns>
        public virtual Task<IEnumerable<T>> AddOrUpdateAsync<T>(List<T> listModels)
        {
            CheckSetup<T>();  //Prevents direct instantiation of a repository without a sufix
            ValidateAndCheckList(listModels);

            // Inform the data operation to LightEvent
            this._dataOperation = LightRepository.InsertOrUpdateOperation;

            return Task.FromResult<IEnumerable<T>>(listModels);
        }

        /// <summary>
        /// Deletes the especific attachment
        /// </summary>
        /// <param name="entityId">id of the attachment</param>
        /// <param name="fileName">name of the file</param>
        /// <returns>Returns the result of the operation</returns>
        public virtual Task DeleteAttachmentAsync<T>(string entityId, string fileName)
        {
            CheckSetup<T>();  //Prevents direct instantiation of a repository without a sufix

            // Inform the data operation to LightEvent
            this._dataOperation = LightRepository.DeleteOperation;

            return Task.FromResult(fileName);
        }

        /// <summary>
        /// Returns the result of the operation
        /// </summary>
        /// <param name="entityId">id of the document</param>
        /// <returns>Returns the result of the operation<</returns>
        public virtual Task DeleteAsync<T>(string entityId)
        {
            CheckSetup<T>();  //Prevents direct instantiation of a repository without a sufix

            // Inform the data operation to LightEvent
            this._dataOperation = LightRepository.DeleteOperation;

            return Task.FromResult(entityId);
        }

        /// <summary>
        /// Add or update the attachment
        /// Calls the base class because there may be some generic behavior in it
        /// </summary>
        /// <param name="entityId">document id</param>
        /// <param name="fileName">name of the file</param>
        /// <param name="attachment">the attachment</param>
        /// <returns></returns>
        public virtual Task<ILightAttachment> AddOrUpdateAttachmentAsync<T>(string entityId, string fileName, Stream attachment)
        {
            CheckSetup<T>();  //Prevents direct instantiation of a repository without a sufix

            // Inform the data operation to LightEvent
            this._dataOperation = LightRepository.InsertOrUpdateOperation;

            return Task.FromResult<ILightAttachment>(new LightAttachment());
        }

        /// <summary>
        /// Calls the base class because there may be some generic behavior in it
        /// Get the attatchment by id and filename
        /// </summary>
        /// <param name="entityId">attachment id</param>
        /// <param name="fileName">name of the file</param>
        /// <returns>The attachment type LightAttachment</returns>
        public virtual Task<ILightAttachment> GetAttachmentAsync<T>(string entityId, string fileName)
        {
            CheckSetup<T>();  //Prevents direct instantiation of a repository without a sufix

            return Task.FromResult<ILightAttachment>(new LightAttachment());
        }

        /// <summary>
        /// Calls the base class because there may be some generic behavior in it
        /// List the Attachments by id
        /// </summary>
        /// <param name="entityId">attatchment id</param>
        /// <returns>A list of attachments</returns>
        public virtual Task<IEnumerable<ILightAttachment>> ListAttachmentsByIdAsync<T>(string entityId)
        {
            CheckSetup<T>();  //Prevents direct instantiation of a repository without a sufix 
            return Task.FromResult<IEnumerable<ILightAttachment>>(new List<ILightAttachment>());
        }

        /// <summary>
        /// Calls the base class because there may be some generic behavior in it
        /// Counts the quantity of documents
        /// </summary>
        /// <typeparam name="T">generic type</typeparam>
        /// <returns>Quantity of Documents</returns>
        public virtual Task<int> CountAsync<T>()
        {
            CheckSetup<T>();  //Prevents direct instantiation of a repository without a sufix
            return Task.FromResult(0);
        }

        /// <summary>
        /// Calls the base class because there may be some generic behavior in it
        /// Counts the quantity of documents
        /// </summary>
        /// <typeparam name="T">generic type</typeparam>
        /// <param name="predicate">lambda expression</param>
        /// <returns>Quantity of Document</returns>
        public virtual Task<int> CountAsync<T>(Expression<Func<T, bool>> predicate)
        {
            CheckSetup<T>();  //Prevents direct instantiation of a repository without a sufix
            return Task.FromResult(0);
        }

        /// <summary>
        /// Calls the base class because there may be some generic behavior in it
        /// This method creates a query for documents under a collection
        /// </summary>
        /// <param name="sqlquery">SQL statement with parameterized values</param>
        /// <returns>JSON object</returns>
        public virtual Task<JObject> QueryAsyncJson<T>(string query)
        {
            CheckSetup<T>();  //Prevents direct instantiation of a repository without a sufix
            return Task.FromResult<JObject>(new JObject());
        }

        /// <summary>
        /// Get data by sql query
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <returns></returns>
        public virtual Task<IEnumerable<T>> QueryAsync<T>(string query)
        {
            CheckSetup<T>();  //Prevents direct instantiation of a repository without a sufix
            return Task.FromResult<IEnumerable<T>>(new List<T>());
        }

        /// <summary>
        /// Get the document by id
        /// Calls the base class because there may be some generic behavior in it
        /// </summary>
        /// <typeparam name="T">generic type</typeparam>
        /// <param name="id">document id</param>
        /// <returns>generic type of document</returns>
        public virtual Task<T> GetByIdAsync<T>(string entityId) where T : new()
        {
            CheckSetup<T>();  //Prevents direct instantiation of a repository without a sufix 
            return Task.FromResult<T>(new T());
        }

        /// <summary>
        /// Calls the base class because there may be some generic behavior in it
        /// Get the Generic object
        /// </summary>
        /// <typeparam name="T">generic type</typeparam>
        /// <param name="predicate">lambda expression</param>
        /// <returns>The generic object</returns>
        public virtual Task<IQueryable<T>> GetAsync<T>(Expression<Func<T, bool>> predicate)
        {
            CheckSetup<T>();  //Prevents direct instantiation of a repository without a sufix
            return Task.FromResult<IQueryable<T>>(new List<T>().AsQueryable());
        }

        /// <summary>
        /// Calls the base class because there may be some generic behavior in it
        /// Get the Generic object
        /// </summary>
        /// <typeparam name="T">generic type</typeparam>
        /// <param name="predicate">lambda expression</param>
        /// <returns>The generic object</returns>
        public virtual Task<IQueryable<T>> GetAsync<T>()
        {
            CheckSetup<T>();  //Prevents direct instantiation of a repository without a sufix
            return Task.FromResult<IQueryable<T>>(new List<T>().AsQueryable());
        }

        /// <summary>
        /// Calls the base class because there may be some generic behavior in it
        /// Get the Generic object
        /// </summary>
        /// <typeparam name="T">generic type</typeparam>
        /// <param name="token">Token for forward paging</param>
        /// <param name="filter">Filter expression</param>
        /// <param name="page">Page number</param>
        /// <param name="itemsPerPage">Itens per page</param>
        /// <returns></returns>
        public virtual Task<ILightPaging<T>> GetByPageAsync<T>(string token, Expression<Func<T, bool>> filter, int page, int itemsPerPage)
        {
            CheckSetup<T>();  //Prevents direct instantiation of a repository without a sufix
            return Task.FromResult<ILightPaging<T>>(new LightPaging<T>());
        }

        /// <summary>
        /// Reset all data in database
        /// </summary>
        /// <returns></returns>
        public abstract bool ResetData(string query);

        public string[] GetFilesRessed()
        {
            return MockData.GetMockFiles();
        }

        /// <summary>
        /// Get Mocked data file and create database, collection and fill database
        /// </summary>
        /// <param name="database"></param>
        /// <param name="collection"></param>
        /// <returns></returns>
        public virtual dynamic GetMockData(string database, string collection)
        {
            dynamic data = new MockData().GetMockData<dynamic>("reseed/" + collection);

            CreateDatabaseIfNotExistsAsync(database, collection).Wait();
            CreateCollectionIfNotExistsAsync(database, collection).Wait();

            return data;
        }

        /// <summary>
        /// creates a collection if doesn't exisists
        /// </summary>
        public abstract Task CreateCollectionIfNotExistsAsync(string database, string collection);

        /// <summary>
        /// creates a database if doesn't exisists
        /// </summary>
        public abstract Task CreateDatabaseIfNotExistsAsync(string database, string collection);

        /// <summary>
        /// Gets the generic object by page
        /// </summary>
        /// <typeparam name="T">generic type</typeparam>
        /// <param name="filter">filter to fetch the object</param>
        /// <param name="page">page</param>
        /// <param name="itemsPerPage">itens per page</param>
        /// <returns>An object with pagination</returns>
        public virtual Task<ILightPaging<T>> GetByPageAsync<T>(Expression<Func<T, bool>> filter, int page, int itemsPerPage)
        {
            CheckSetup<T>();  //Prevents direct instantiation of a repository without a sufix
            return Task.FromResult<ILightPaging<T>>(new LightPaging<T>());
        }

        private delegate bool RawGeneric(Type generic, Type tocheck);

        private readonly RawGeneric _rawGeneric = (generic, toCheck) =>
        {
            while (toCheck != null && toCheck != typeof(object))
            {
                var cur = toCheck.IsGenericType ? toCheck.GetGenericTypeDefinition() : toCheck;
                if (generic == cur)
                {
                    return true;
                }
                toCheck = toCheck.BaseType;
            }
            return false;
        };

        protected bool IsOptimisticConcurrency(Type generic, Type toCheck)
        {
            return _rawGeneric.Invoke(generic, toCheck);
        }

        public abstract dynamic AccessConditionOptimistic<T>(T model);

        /// <summary>
        /// Abstract method to force inherithed class to implement Health Check Method.
        /// </summary>
        /// <param name="serviceKey"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public abstract LightHealth.HealthCheck HealthCheck(string serviceKey, string value);



        /// <summary>
        /// While in abstract layer, this method calls the LightEvent per CRUD operations in repository
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="model"></param>
        /// <param name="dataOperation"></param>
        /// <returns></returns>
        public static Task RaiseEvent<T>(T model, string dataOperation)
        {
            try
            {
                LightEvent Event = (LightEvent)Workbench.Instance.GetRegisteredService(WorkbenchServiceType.EventHandler);

                if (Event != null)
                    Event.SendToHub(model, dataOperation);
            }
            catch (Exception exRegister)
            {
                ((LightTelemetry)Workbench.Instance.Telemetry).TrackException(exRegister);
            }

            return Task.FromResult<T>(default(T));
        }
    }
}
