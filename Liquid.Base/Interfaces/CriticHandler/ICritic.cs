namespace Liquid.Interfaces
{
    /// <summary>
    /// Enum delegates to create the Critic Type, There are three types, Error, Warning and Info
    /// </summary> 
    public enum CriticType { Error = 1, Warning = 2, Info = 3 };
    /// <summary>
    /// Interface delegates to create the Critic
    /// </summary> 
    public interface ICritic
    {
        string Code { get; }
        string Message { get; }
        CriticType Type { get; }

        /// <summary>
        /// Adding a business error to the critic
        /// </summary>
        /// <param name="code">Code to be added to critic</param>
        void AddError(string code);
        /// <summary>
        /// Adding a business info to the critic
        /// </summary>
        /// <param name="code">Code to be added to critic</param>
        void AddInfo(string code);
        /// <summary>
        /// Adding a business warning to the critic
        /// </summary>
        /// <param name="code">Code to be added to critic</param>
        void AddWarning(string code);
    }
}