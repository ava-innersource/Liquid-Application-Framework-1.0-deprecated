using Liquid.Base.Domain;
using Liquid.Domain;
using System;
using System.Linq;
using System.Threading.Tasks;


namespace Liquid.Sample.CarRegistry
{
    /// <summary>
    /// Services of Car
    /// </summary>
    public class CarService : LightService
    {
        /// <summary>
        /// Get a record of Car
        /// </summary>
        /// <param name="id">Car Id</param>
        /// <returns></returns>
        public async Task<DomainResponse> GetAsync(string id)
        {
            Telemetry.TrackEvent("Get Record");
            var records = await Repository.GetByIdAsync<Car>(id);
            return Response(records);
        }

        /// <summary>
        /// Get All records of Car
        /// </summary>
        /// <returns></returns>
        public async Task<DomainResponse> GetAllAsync()
        {
            Telemetry.TrackEvent("GetAll Records");
            var records = await Repository.GetAsync<Car>(x => true);
            return Response(records);
        }

        /// <summary>
        /// Save a record of Car
        /// </summary>
        /// <param name="viewModel"></param>
        /// <returns></returns>
        public async Task<DomainResponse> SaveAsync(CarVM viewModel)
        {
            var model = new Car();
            model.MapFrom(viewModel);
            Telemetry.TrackEvent("Save Record");
            var records = await Repository.AddOrUpdateAsync(model);
            return Response(records);
        }

        /// <summary>
        /// Save a record using a worker
        /// </summary>
        /// <param name="viewModel"></param>
        /// <returns></returns>
        public async Task<DomainResponse> SaveWorker(LegacyCarVM viewModel)
        {
            var car = CarFactory.Create(viewModel);
            Telemetry.TrackEvent("Save Record");

            var query = Repository.GetAsync<Car>(x => x.Id.ToString() == viewModel.Id).Result;

            var carReturn = query.AsEnumerable().FirstOrDefault();

            if (carReturn != null)
            {
                car.Id = carReturn.Id;
            }

            var records = await Repository.AddOrUpdateAsync(car);
            return Response(records);
        }

        /// <summary>
        /// Deletes a record of Car
        /// </summary>
        /// <param name="id">Identifies the car to be removed</param>
        /// <returns>An empty <see cref="DomainResponse"/></returns>
        public async Task<DomainResponse> DeleteAsync(Guid id)
        {
            Telemetry.TrackEvent("Delete Record");
            await Repository.DeleteAsync<Car>(id.ToString());
            return Response(new DomainResponse());
        }
    }
}
