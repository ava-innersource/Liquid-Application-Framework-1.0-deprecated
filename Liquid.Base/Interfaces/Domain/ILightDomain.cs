namespace Liquid.Interfaces
{
    /// <summary>
    /// Base inteface for (business) domain classes
    /// </summary>
    public interface ILightDomain
    {
        ICriticHandler CritictHandler { get; set; }
    }
}
