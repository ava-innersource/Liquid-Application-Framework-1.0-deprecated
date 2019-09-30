using Liquid.Domain;
using Liquid.Interfaces;
using Newtonsoft.Json;

namespace Liquid.Base.Domain
{
    /// <summary>
    /// Class responsible for business logic and operations implemented as methods from the Domain Classes
    /// </summary>
    public abstract class LightDomain : ILightDomain
    {
        #region Protected Variables
        protected bool HasNotFoundError { get; set; }
        protected ILightRepository Repository => WorkBench.Repository;
        protected ILightMediaStorage MediaStorage => WorkBench.MediaStorage;
        public ILightTelemetry Telemetry { get; set; }
        public ILightContext Context { get; set; }
        public ILightCache Cache { get; set; }
        public ILightLogger Logger { get; set; }
        protected bool HasBusinessErrors => _criticHandler != null && _criticHandler.HasBusinessErrors;
        private ICriticHandler _criticHandler { get; set; }
        public ICriticHandler CritictHandler { get { return _criticHandler; } set { _criticHandler = value; } }
        #endregion

        #region Internal Methods
        internal abstract void ExternalInheritanceNotAllowed();

        #endregion


        #region Protected Methods
        /// <summary>
        /// Add to the scope that some critic has a not found type of error
        /// </summary>
        protected void AddNotFound()
        {
            HasNotFoundError = true;
        }
        /// <summary>
        /// Add to the scope that some critic has a not found type of error
        /// <param name="errorCode">error code of the message</param>
        /// </summary>
        protected void AddNotFound(string errorCode)
        {
            AddNotFound();
            AddBusinessError(errorCode);
        }

        /// <summary>
        /// Method add the error code to the CriticHandler
        /// and add in Critics list to build the object InvalidInputException
        /// </summary>
        /// <param name="errorCode">error code of the message</param>
        protected void AddBusinessError(string errorCode)
        {
            _criticHandler.AddBusinessError(errorCode);
        }

        /// <summary>
        /// Method add the error code to the CriticHandler
        /// and add in Critics list to build the object InvalidInputException
        /// </summary>
        /// <param name="errorCode">error code of the message</param>
        protected void AddBusinessError(string errorCode, params object[] args)
        {
            _criticHandler.AddBusinessError(errorCode, args);
        }

        /// <summary>
        /// Method add the warning to the CriticHandler
        /// and add in Critics list to build the object InvalidInputException
        /// </summary>
        /// <param name="warningCode"></param>
        protected void AddBusinessWarning(string warningCode)
        {
            _criticHandler.AddBusinessWarning(warningCode);
        }

        /// <summary>
        /// Method add the error code to the CriticHandler
        /// and add in Critics list to build the object InvalidInputException
        /// </summary>
        /// <param name="warningCode"></param>
        protected void AddBusinessWarning(string warningCode, params object[] args)
        {
            _criticHandler.AddBusinessWarning(warningCode, args);
        }

        /// <summary>
        /// /// Method add the information to the Critic Handler
        /// and add in Critics list to build the object InvalidInputException
        /// </summary>
        /// <param name="infoCode"></param>
        protected void AddBusinessInfo(string infoCode)
        {
            _criticHandler.AddBusinessInfo(infoCode);
        }

        /// <summary>
        /// Method add the error code to the CriticHandler
        /// and add in Critics list to build the object InvalidInputException
        /// </summary>
        /// <param name="infoCode"></param>
        protected void AddBusinessInfo(string infoCode, params object[] args)
        {
            _criticHandler.AddBusinessInfo(infoCode, args);
        }

        /// <summary>
        /// Returns a DomainResponse class with data Serialize on JSON
        /// </summary>
        /// <typeparam name="T">The desired type LightViewModel</typeparam>
        /// <returns>Instance of the specified DomainResponse</returns>
        protected DomainResponse Response<T>(T data)
        {
            DomainResponse response = new DomainResponse();
            response.Critics = (_criticHandler != null) ? _criticHandler.Critics.ToJsonCamelCase() : null;
            response.ModelData = data;
            response.NotFoundMessage = HasNotFoundError;
            return response;
        }

        #endregion

        #region Static Methods

        /// <summary>
        /// Returns a  instance of a LightDomain class for calling business domain logic
        /// </summary>
        /// <typeparam name="T">desired LightDomain subtype</typeparam>
        /// <returns>Instance of the specified LightDomain subtype</returns>
        public static T FactoryDomain<T>() where T : LightDomain, new()
        {
            ILightDomain service = (ILightDomain)new T();
            return (T)service;
        }

        #endregion

    }
}
