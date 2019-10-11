using Liquid.Base.Domain; 
namespace Liquid.Domain
{
    public abstract class LightService : LightDomain
    {
        /// <summary>
        /// Returns new instance of a domain LightService 
        /// responsible for delegate (business) functionality
        /// </summary>
        /// <typeparam name="T">the delegate LightDomain class</typeparam>
        /// <returns>Instance of the LightDomain class</returns>
        protected virtual T FactoryDelegate<T>() where T : LightService, new()
        {
            T domain = FactoryDomain<T>();
            domain.CritictHandler = CritictHandler;
            return domain;
        }

        internal override void ExternalInheritanceNotAllowed()
        { }
    }
}
