using Liquid.Base.Domain;
using Liquid.Domain;
using Liquid.Domain.Base;
using Liquid.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text.Encodings.Web;
using System.Text.RegularExpressions;

namespace Liquid.Activation
{    /// <summary>
     /// This Controller and its action method handles incoming browser requests, 
     /// retrieves necessary model data and returns appropriate responses.
     /// </summary>
    public abstract class LightController : Controller
    {
        protected LightContext _context;
        private readonly List<string> _inputValidationErrors = new List<string>();
        //Cloning TLightelemetry service singleton because it services multiple LightDomain instances from multiple threads with instance variables
        private readonly ILightTelemetry _telemetry = Workbench.Instance.Telemetry != null ? (ILightTelemetry)Workbench.Instance.Telemetry.CloneService() : null;
        protected ILightTelemetry Telemetry => _telemetry;
        protected ILightLogger Logger => Workbench.Instance.Logger;
        protected ILightCache Cache => Workbench.Instance.Cache;
        //Instance of CriticHandler to inject on the others classes
        private readonly CriticHandler _criticHandler = new CriticHandler();
        public IHttpContextAccessor _httpContextAccessor;
 
        private LightContext GetContext()
        {
            return new LightContext(_httpContextAccessor)
            {
                User = (HttpContext != null) ? HttpContext.User : null
            };
        }

        /// <summary>
        /// Method to build domain class
        /// </summary>
        /// <typeparam name="T">Generic Type</typeparam>
        /// <returns></returns>
        protected T Factory<T>() where T : LightDomain, new()
        {
            ///Verify if there's erros
            if (_inputValidationErrors.Count > 0)
            {
                /// Throws the error code from errors list of input validation to View Model
                throw new InvalidInputException(_inputValidationErrors);
            }

            var domain = LightDomain.FactoryDomain<T>();
            domain.Telemetry = _telemetry;
            domain.Logger = Logger;
            domain.Context = GetContext();
            domain.Cache = Cache;
            domain.CritictHandler = _criticHandler;
			
            return domain;
        }

        /// <summary>
        /// Verify the DomainResponse
        /// </summary>
        /// <param name="response"></param>
        /// <returns>IAResponsible</returns>
        protected IActionResult Result(DomainResponse response)
        {
            response.PayLoad = (response.ModelData != null) ? response.ModelData.ToJsonCamelCase() : null;

            if (Logger?.EnabledLogTrafic == true)
            {
                Logger.Info("Router: " + HttpContext.Request.Path + " \n Response: " + response.ToStringCamelCase());
            }

            if (response.NotFoundMessage)
            {
                return NotFound(response);
            }
            
			if (response.BadRequestMessage) return BadRequest(response);
            
            if(response.GenericReturnMessage) return StatusCode((int)response.StatusCode, response);
			
            return Ok(response);            
        }

        /// <summary>
        /// The method receives the error code to add on errors list of input validation.
        /// </summary>
        /// <param name="error">The code error</param>
        protected void AddInputValidationErrorCode(string error)
        {
            _inputValidationErrors.Add(error);
        }

        /// <summary>
        /// The method receives the ViewModel to input validation and add on errors list.
        /// (if there are errors after validation ViewModel.)
        /// </summary>
        /// <param name="viewModel">The ViewModel to input validation</param>
        protected void ValidateInput<T>(T viewModel) where T : LightViewModel<T>, ILightViewModel, new()
        {
            viewModel.InputErrors = _inputValidationErrors;
            viewModel.Validate();
            InputResultValidation result = viewModel.Validator.Validate(viewModel);
            if (!result.IsValid)
            {
                foreach (var error in result.Errors)
                {
                    /// The method receive the error code to add on errors list of input validation.
                    AddInputValidationErrorCode(error);
                }
            }
            //By reflection, browse viewModel by identifying all attributes and lists for validation.  
            foreach (PropertyInfo fieldInfo in viewModel.GetType().GetProperties())
            {
                dynamic child = fieldInfo.GetValue(viewModel);

                //Encoding of Special Characters
                if (child != null && child.GetType() == typeof(string))
                {
                    if (Regex.IsMatch((string)child, (@"[^a-zA-Z0-9]")))
                    {
                        var encoder = HtmlEncoder.Create(allowedRanges: new[] {
                            System.Text.Unicode.UnicodeRanges.BasicLatin,
                            System.Text.Unicode.UnicodeRanges.Latin1Supplement });

                        child = encoder.Encode(child);
                        fieldInfo.SetValue(viewModel, child);
                    }
                }

                //When the child is a list, validate each of its members  
                if (child is IList)
                {
                    var children = (IList)fieldInfo.GetValue(viewModel);
                    foreach (var item in children)
                    {

                        //Check, if the property is a Light ViewModel, only they will validation Lights ViewModel
                        if ((item.GetType().BaseType != typeof(object))
                             && (item.GetType().BaseType != typeof(System.ValueType))
                                && (item.GetType().BaseType.GetGenericTypeDefinition() == typeof(LightViewModel<>)))
                        {
                            dynamic obj = item;
                            //Check, if the attribute is null for verification of the type.
                            if (obj != null)
                                ValidateInput(obj);
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
                            child.GetType().BaseType.GetGenericTypeDefinition() == typeof(LightViewModel<>)))
                        {

                            ValidateInput(child);
                        }
                    }
                }
            }
        }
    }
}

