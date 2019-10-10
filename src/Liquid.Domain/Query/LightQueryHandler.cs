using Liquid.Base.Domain; 
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Liquid.Domain
{
    public abstract class LightQueryHandler<T> : LightDomain
    { 
        protected T Query { get; set; }

        public async Task<DomainResponse> Execute(T query)
        {

            //Injects the command and call business domain logic to handle it
            Query = query;

            //Calls Handle operation asyncronously
            return await Task.Run(() => Handle());
        }

        protected abstract DomainResponse Handle();

        internal override void ExternalInheritanceNotAllowed()
        { }
    }
}

