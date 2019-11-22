using Liquid.Activation;

namespace Liquid.Sample.CarRegistry
{
    /// <summary>
    /// Message responsible to traffic the Topic content
    /// </summary>
    public class CarMessage : LightMessage<CarMessage>
    {
        /// <summary>
        /// Object ViewModel 
        /// </summary>
        public LegacyCarVM LegacyCarVM { get; set; }

        /// <summary>
        /// Not implemented override funcion necessary
        /// </summary>
        public override void Validate()
        {

        }
    }
}
