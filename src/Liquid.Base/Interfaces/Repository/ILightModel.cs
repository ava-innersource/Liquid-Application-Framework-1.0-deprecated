namespace Liquid.Interfaces
{
    public interface ILightModel
    {
        string id { get; set; }
        /// <summary>
        /// The method used to input validation of ViewModel.
        /// </summary>
        void Validate();
    }
}
