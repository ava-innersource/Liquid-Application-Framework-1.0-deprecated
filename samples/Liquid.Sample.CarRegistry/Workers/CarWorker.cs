using Liquid.Activation;

namespace Liquid.Sample.CarRegistry
{
    /// <summary>
    /// Worker of Car
    /// </summary>
    [MessageBus("BUS")]
    public class CarWorker : LightWorker
    {
        /// <summary>
        /// Method Get Message
        /// </summary>
        /// <param name="message">CarMessage Object</param>
        [Topic("car", "LegacyCarSubs", 10, false)]
        public void GetMessage(CarMessage message)
        {
            ValidateInput(message);
            //Calls domain (business) logic
            var response = Factory<CarService>().SaveWorker(message.LegacyCarVM);
            //Terminates the message according to domain response
            Terminate(response.Result);
        }
    }
}
