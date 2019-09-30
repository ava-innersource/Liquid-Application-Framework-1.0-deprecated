using Liquid.Base.Domain;
using System.Threading.Tasks;

namespace Liquid.Domain
{
    public abstract class LightCommandHandler<T> : LightDomain
    { 
        protected T Command { get; set ; } 

        public async Task<DomainResponse> Execute(T command)
        {

            //Injects the command and call business domain logic to handle it
            Command = command;

            //Calls Handle operation asyncronously
            return await Task.Run(() => Handle());
        }

        protected abstract DomainResponse Handle();

        #region Internal Methods
        internal override void ExternalInheritanceNotAllowed()
        { }
        #endregion

    }
}

