using Liquid.Domain;
using Liquid.Interfaces;

namespace Liquid.Base.Domain
{
    /// <summary>
    /// Class responsible for business logic and operations implemented as methods from the Domain Classes
    /// </summary>
    public abstract class LightDomain : ILightDomain
    {

        // TODO: StatusCode, BadRequest and GenericReturn were added by an Avanade internal project. We must investigate their real need

        protected int? StatusCode { get; set; }
        protected bool HasNotFoundError { get; set; }
        protected bool HasNotGenericReturn { get; set; }
        protected bool HasBadRequestError { get; set; }
        protected ILightRepository Repository => WorkBench.Repository;
        protected ILightMediaStorage MediaStorage => WorkBench.MediaStorage;
        public ILightTelemetry Telemetry { get; set; }
        public ILightContext Context { get; set; }
        public ILightCache Cache { get; set; }
        public ILightLogger Logger { get; set; }
        protected bool HasBusinessErrors => CritictHandler?.HasBusinessErrors == true;

        /// <summary>
        /// Responsible for managing issues concerning business logic
        /// </summary>
        public ICriticHandler CritictHandler { get; set; }
        internal abstract void ExternalInheritanceNotAllowed();

        /// <summary>
        /// Add to the scope that some generic return message and status code
        /// </summary>
        protected void AddGenericReturn(StatusCodes statusCode)
        {
            HasNotGenericReturn = true;
            StatusCode = (int)statusCode;
        }
        /// <summary>
        /// Add to the scope that some generic return message and status code
        /// </summary>
        protected void AddGenericReturn(string errorCode, StatusCodes statusCode)
        {
            AddGenericReturn(statusCode);
            AddBusinessError(errorCode);
        }
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
        /// Add to the scope that some critic has a bad request type of error
        /// </summary>
        protected void AddBadRequest()
        {
            HasBadRequestError = true;
        }

        /// <summary>
        /// Add to the scope that some critic has a bad request type of error
        /// <param name="errorCode">error code of the message</param>
        /// </summary>
        protected void AddBadRequest(string errorCode)
        {
            AddBadRequest();
            AddBusinessError(errorCode);
        }

        /// <summary>
        /// Method add the error code to the CriticHandler
        /// and add in Critics list to build the object InvalidInputException
        /// </summary>
        /// <param name="errorCode">error code of the message</param>
        protected void AddBusinessError(string errorCode)
        {
            CritictHandler?.AddBusinessError(errorCode);
        }

        /// <summary>
        /// Method add the error code to the CriticHandler
        /// and add in Critics list to build the object InvalidInputException
        /// </summary>
        /// <param name="errorCode">error code of the message</param>
        protected void AddBusinessError(string errorCode, params object[] args)
        {
            CritictHandler?.AddBusinessError(errorCode, args);
        }

        /// <summary>
        /// Method add the warning to the CriticHandler
        /// and add in Critics list to build the object InvalidInputException
        /// </summary>
        /// <param name="warningCode"></param>
        protected void AddBusinessWarning(string warningCode)
        {
            CritictHandler?.AddBusinessWarning(warningCode);
        }

        /// <summary>
        /// Method add the error code to the CriticHandler
        /// and add in Critics list to build the object InvalidInputException
        /// </summary>
        /// <param name="warningCode"></param>
        protected void AddBusinessWarning(string warningCode, params object[] args)
        {
            CritictHandler?.AddBusinessWarning(warningCode, args);
        }

        /// <summary>
        /// /// Method add the information to the Critic Handler
        /// and add in Critics list to build the object InvalidInputException
        /// </summary>
        /// <param name="infoCode"></param>
        protected void AddBusinessInfo(string infoCode)
        {
            CritictHandler?.AddBusinessInfo(infoCode);
        }

        /// <summary>
        /// Method add the error code to the CriticHandler
        /// and add in Critics list to build the object InvalidInputException
        /// </summary>
        /// <param name="infoCode"></param>
        protected void AddBusinessInfo(string infoCode, params object[] args)
        {
            CritictHandler?.AddBusinessInfo(infoCode, args);
        }

        /// <summary>
        /// Returns a DomainResponse class with data Serialize on JSON
        /// </summary>
        /// <typeparam name="T">The desired type LightViewModel</typeparam>
        /// <returns>Instance of the specified DomainResponse</returns>
        protected DomainResponse Response<T>(T data)
        {
            DomainResponse response = new DomainResponse();
            response.Critics = CritictHandler?.Critics?.ToJsonCamelCase();
            response.ModelData = data;
            response.NotFoundMessage = HasNotFoundError;
            response.BadRequestMessage = HasBadRequestError;
            response.GenericReturnMessage = HasNotGenericReturn;
            response.StatusCode = StatusCode;
            response.OperationId = System.Diagnostics.Activity.Current?.RootId;
            return response;
        }

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
    }
}
