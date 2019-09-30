using Newtonsoft.Json;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Liquid.Runtime
{
    public class SwaggerIgnoreFilter : ISchemaFilter
    {
        public void Apply(Schema schema, SchemaFilterContext context)
        {
            if (schema?.Properties == null)
                return;

            var excludedProperties = context.SystemType.GetProperties().Where(t => t.GetCustomAttribute<JsonIgnoreAttribute>() != null);

            foreach (var excludedProperty in excludedProperties)
            {
                var propertyToRemove = schema.Properties.Keys.SingleOrDefault(x => x.ToLower() == excludedProperty.Name.ToLower());
                if (propertyToRemove != null)
                    schema.Properties.Remove(propertyToRemove);
            }
        }
    }
}
