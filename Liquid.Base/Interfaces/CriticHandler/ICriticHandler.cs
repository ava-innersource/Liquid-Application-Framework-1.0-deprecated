using System.Collections.Generic;

namespace Liquid.Interfaces
{
    /// <summary>
    /// Interface delegates to the handler that creates the Critics lists
    /// </summary> 
    public interface ICriticHandler
    {
        bool HasBusinessErrors { get; }
        bool HasBusinessWarning { get; }
        bool HasBusinessInfo { get; }

        /// <summary>
        /// Creates a list of object Critic. 
        /// Each Critic contains the error code.
        /// and the CriticType is a error.
        /// </summary>
        /// <param name="errorCode"></param>
        void AddBusinessError(string errorCode);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="errorCode"></param>
        /// <param name="args"></param>
        void AddBusinessError(string errorCode, params object[] args);
        /// <summary>
        /// Creates a list of object Critic. 
        /// Each Critic contains the error code.
        /// and the CriticType is a Warning.
        /// </summary>
        /// <param name="warningCode"></param>
        void AddBusinessWarning(string warningCode);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="warningCode"></param>
        /// <param name="args"></param>
        void AddBusinessWarning(string warningCode, params object[] args);
        /// <summary>
        /// Creates a list of object Critic. 
        /// Each Critic contains the error code.
        /// and the CriticType is a Info.
        /// </summary>
        /// <param name="infoCode"></param>
        void AddBusinessInfo(string infoCode);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="infoCode"></param>
        /// <param name="args"></param>
        void AddBusinessInfo(string infoCode, params object[] args);
        List<ICritic> Critics { get; }
    }
}