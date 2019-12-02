using Liquid.Activation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Liquid.Sample.CarRegistry
{
    /// <summary>
    /// Provides access to CRUD operations on Car records
    /// </summary>
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]/[action]")]
    public class CarController : LightController
    {
        /// <summary>
        /// Get All Records
        /// </summary>
        /// <remarks> 
        /// GET api/v1.0/GetAll 
        /// </remarks> 
        [HttpGet]
#if RELEASE
            [Authorize]
#endif
        [ApiExplorerSettings(GroupName = "v1")]
        public async Task<IActionResult> GetAll()
        {
            var data = await Factory<CarService>().GetAllAsync();
            return Result(data);
        }

        /// <summary>
        /// Get a Record
        /// </summary>
        /// <remarks> 
        /// GET api/v1.0/Get/1
        /// </remarks> 
        [HttpGet("{id}")]
#if RELEASE
            [Authorize]
#endif
        [ApiExplorerSettings(GroupName = "v1")]
        public async Task<IActionResult> Get(string id)
        {
            var data = await Factory<CarService>().GetAsync(id);
            return Result(data);
        }

        /// <summary>
        /// Post Record
        /// </summary>
        /// <remarks> 
        /// Post api/v1.0/Post 
        /// </remarks> 
        [HttpPost]
#if RELEASE
            [Authorize]
#endif
        [ApiExplorerSettings(GroupName = "v1")]
        public async Task<IActionResult> PostAsync(CarVM viewModel)
        {
            var data = await Factory<CarService>().SaveAsync(viewModel);
            return Result(data);
        }

        /// <summary>
        /// Delete Record
        /// </summary>
        /// <remarks> 
        /// Post api/v1.0/Delete
        /// </remarks> 
        [HttpDelete]
#if RELEASE
            [Authorize]
#endif
        [ApiExplorerSettings(GroupName = "v1")]
        public async Task<IActionResult> DeleteAsync(Guid id)
        {
            var data = await Factory<CarService>().DeleteAsync(id);
            return Result(data);
        }

        /// <summary>
        /// Put Record
        /// </summary>
        /// <remarks> 
        /// Post api/v1.0/Put 
        /// </remarks> 
        [HttpPut]
#if RELEASE
            [Authorize]
#endif
        [ApiExplorerSettings(GroupName = "v1")]
#pragma warning disable S4144
        public async Task<IActionResult> PutAsync(CarVM viewModel)
#pragma warning restore S4144
        {
            var data = await Factory<CarService>().SaveAsync(viewModel);
            return Result(data);
        }
    }
}
