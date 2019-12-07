namespace Liquid.Interfaces
{
    /// <summary>
    /// Base inteface for (business) domain classes
    /// </summary>
    public interface ILightDomain
    {
        /// <summary>
        /// Responsible for managing issues concerning business logic
        /// </summary>
        ICriticHandler CritictHandler { get; set; }
    }
}
