using Liquid.Base;
using Liquid.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using Newtonsoft.Json.Schema.Generation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace Liquid.Activation
{
    public class ModelSpecification
    {
        /// <summary>
        /// Return the Model Specification
        /// </summary>
        /// <returns></returns>	
        public JObject GetModelSpecification()
        {
			// Retrive the assemblies that contains the attribute ModelSpecificationAttribute
			Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            var _classSigned = (from assembly in assemblies
                                where !assembly.IsDynamic
                                from type in assembly.ExportedTypes
                                where type.IsDefined(typeof(ModelSpecificationAttribute), false)
                                select type).FirstOrDefault();
            JSchema schema;

			//Check if some class contains the attribute ModelSpecificationAttribute
			if (_classSigned != null)
            {
				//Create a instance of Jschema generator.
                JSchemaGenerator generator = new JSchemaGenerator();

				//Setting some configurations.
				generator.DefaultRequired = Newtonsoft.Json.Required.Default;
				generator.SchemaReferenceHandling = SchemaReferenceHandling.None;
				generator.GenerationProviders.Add(new StringEnumGenerationProvider());

				//Generate the schema.
				schema = generator.Generate(_classSigned);

				//Recovery the object already got based on the name class
				var item = ModelSpecificationEngine.swaggerMetaDatas.Where(x => x.Name == _classSigned.Name).FirstOrDefault();

				//Check if the root class contains the description.
				if (!string.IsNullOrWhiteSpace(item.Description))
				{
					schema.Description = Regex.Replace(item.Description, ModelSpecificationEngine.PATTERN, string.Empty).Trim();
				}

				//Now its necessary check if have 
				foreach (var x in schema.Properties)
				{
					//Check if has more properties in this case will be necessary.
					if (x.Value.Properties.Count > 0)
					{
						//Call the recursive tree for get all properties from this object
						this.RecursiveTree(x.Key, x.Value.Properties);
					}

					//Now check if some properties have description. For type primite of the root parent write the descript without annotations.
					if (item.PropertyRutime.TryGetValue(x.Key, out Tuple<string, bool> descriptionProp))
					{
						x.Value.Description = Regex.Replace(descriptionProp.Item1.Replace(ModelSpecificationEngine.REQUIRED, string.Empty).Trim(), ModelSpecificationEngine.PATTERN, "").Trim();
					}
				}			
			}
            else
            {
                ///If there isn't Custom Attribute, will be throw exception.
                throw new LightException($"No Attribute ModelSpecification has been informed on the Model class");
            }

			JObject json = JObject.Parse(schema.ToString());

			Dictionary<string, Tuple<string, bool>> requireds = ModelSpecificationEngine.swaggerMetaDatas.Where(x => x.Name == _classSigned.Name).FirstOrDefault().PropertyRutime;

			List<string> variablesRequired = new List<string>();

			foreach (KeyValuePair<string, Tuple<string, bool>> entry in requireds)
			{
				if (entry.Value.Item2)
				{
					variablesRequired.Add(entry.Key);
				}

				//Add format to the root parent.
				if (Regex.Match(entry.Value.Item1, ModelSpecificationEngine.PATTERN).Success)
				{
					var retriveJsonProps = Regex.Match(entry.Value.Item1, ModelSpecificationEngine.PATTERN).Value.ToString();
					JObject obj = JObject.Parse(retriveJsonProps);

					var tokens = json.FindTokens(entry.Key).FirstOrDefault();

					if (tokens != null)
					{
						tokens.Last.AddAfterSelf(new JProperty("format", obj));
					}

				}
			}		

			//Populating the root
			json.Property("properties").AddAfterSelf(new JProperty("required", variablesRequired.ToArray()));

			ModelSpecificationEngine.projectMeta.project = _classSigned.Assembly.FullName.Split(',').FirstOrDefault();
            HubAttribute attribute = (HubAttribute)_classSigned.GetCustomAttributes(typeof(HubAttribute), false).FirstOrDefault();
            ModelSpecificationEngine.projectMeta.hub = string.IsNullOrEmpty(attribute.HubName) ? string.Empty : attribute.HubName;

            //Adding inside of meta.
            json.Property("properties").AddAfterSelf(new JProperty("meta", JObject.FromObject(ModelSpecificationEngine.projectMeta)));

			//Loop para adicionar required para cada elemento filho
			foreach (SwaggerMetaData swaggerMeta in ModelSpecificationEngine.swaggerMetaDatas.Where(x => x.Name != _classSigned.Name))
			{
				List<string> childRequireds = new List<string>();

				foreach (KeyValuePair<string, Tuple<string, bool>> props in swaggerMeta.PropertyRutime)
				{
					if (props.Value.Item2)
					{
						childRequireds.Add(props.Key);
					}

					//Loop for add in deep formats to childs
					if (Regex.Match(props.Value.Item1, ModelSpecificationEngine.PATTERN).Success)
					{
						var retriveJsonProps = Regex.Match(props.Value.Item1, ModelSpecificationEngine.PATTERN).Value.ToString();
						JObject obj = JObject.Parse(retriveJsonProps);

						var tokens = json.FindTokens(props.Key).FirstOrDefault();

						if (tokens != null)
						{
							tokens.Last.AddAfterSelf(new JProperty("format", obj));
						}

					}
				}

				if (childRequireds.Count > 0)
				{
					var tokens = json.FindTokens(swaggerMeta.Name).FirstOrDefault();

					if (tokens != null)
					{
						tokens.Last.AddAfterSelf(new JProperty("required", childRequireds.ToArray()));
					}
				}
			}

			return json;
        }

		/// <summary>
		/// Recursive tree is a method responsible for check the sub properties.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="props"></param>
		public void RecursiveTree(string name, IDictionary<string, JSchema> props)
		{
			//Read all properties for the sub properties
			foreach (var item in props)
			{
				if (item.Value.Properties.Count > 0 || item.Value.Items.Count > 0)
				{
					//Check if the sub properties has more properties
					if (item.Value.Properties.Count > 0)
					{
						//Call again passing the new props
						RecursiveTree(item.Key, item.Value.Properties);
					}
					else
					{
						//Call again if the object is a list or array
						RecursiveTree(item.Key, item.Value.Items[0].Properties);
					}
				}

				else
				{
					//Set the summary of properties
					var getContainer = ModelSpecificationEngine.swaggerMetaDatas.Where(x => x.Name == name).FirstOrDefault();
					
					if (getContainer != null)
					{
						//Try setting the description for the property
						if (getContainer.PropertyRutime.TryGetValue(item.Key, out Tuple<string, bool> descriptionProp))
						{
							item.Value.Description = Regex.Replace(descriptionProp.Item1.Replace(ModelSpecificationEngine.REQUIRED, string.Empty).Trim(), ModelSpecificationEngine.PATTERN, "");
						}
					}
					else
					{
						//Now if doesn't exist bind the property to the list for List or Arrays, we need figure out the correct way.
						ModelSpecificationEngine.swaggerMetaDatas.ForEach(x =>
						{
							if (x.PropertyRutime.TryGetValue(item.Key, out Tuple<string, bool> descriptionProp))
							{
								item.Value.Description = Regex.Replace(descriptionProp.Item1.Replace(ModelSpecificationEngine.REQUIRED, string.Empty).Trim(), ModelSpecificationEngine.PATTERN, "");
							}
						});
					}
				}

			}

		}

		/// <summary>
		/// Discovery the name of domain defined on the implementation of the Model Specification
		/// </summary>
		/// <param name="method">Method related the queue or topic</param>
		/// <returns>String key connection defined on the implementation of the LightWorker</returns>
		protected string GetKeyConnection(MethodInfo method)
        {
            var attributes = method.ReflectedType.CustomAttributes;
            string connectionKey = "";
            if (attributes.Any())
                connectionKey = attributes.ToArray()[0].ConstructorArguments[0].Value.ToString();
            return connectionKey;
        }
    }
}
