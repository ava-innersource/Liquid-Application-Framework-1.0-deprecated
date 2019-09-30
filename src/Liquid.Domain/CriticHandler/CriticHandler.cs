using Liquid.Domain;
using Liquid.Interfaces;
using System.Collections.Generic;

namespace Liquid.Domain
{
    public class CriticHandler : ICriticHandler
    {

        public List<ICritic> Critics { get; } = new List<ICritic>();

        public bool HasBusinessErrors => Critics.Exists(c => c.Type == CriticType.Error);
        public bool HasBusinessWarning => Critics.Exists(c => c.Type == CriticType.Warning);
        public bool HasBusinessInfo => Critics.Exists(c => c.Type == CriticType.Info);

        /// <summary>
        /// Creates a list of object Critic. 
        /// Each Critic contains the error code.
        /// and the CriticType is a error.
        /// </summary>
        /// <param name="errorCode"></param>
        public void AddBusinessError(string errorCode)
        {
            string message = JsonStringLocalizer.Localize(errorCode);

            Critic critic = new Critic();
            critic.AddError(errorCode, message);
            Critics.Add(critic);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="errorCode"></param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        public void AddBusinessError(string errorCode, params object[] args)
        {
            string message = JsonStringLocalizer.Localize(errorCode, args);

            Critic critic = new Critic();
            critic.AddError(errorCode, message);
            Critics.Add(critic);
        }

        /// <summary>
        /// Creates a list of object Critic. 
        /// Each Critic contains the error code.
        /// and the CriticType is a Info.
        /// </summary>
        /// <param name="infoCode"></param>
        public void AddBusinessInfo(string infoCode)
        {
            string message = JsonStringLocalizer.Localize(infoCode);

            Critic critic = new Critic();
            critic.AddInfo(infoCode, message);
            Critics.Add(critic);
        }

        /// <summary>
        /// Creates a list of object Critic. 
        /// Each Critic contains the error code.
        /// and the CriticType is a Info.
        /// </summary>
        /// <param name="infoCode"></param>
        public void AddBusinessInfo(string infoCode, params object[] args)
        {
            string message = JsonStringLocalizer.Localize(infoCode, args);

            Critic critic = new Critic();
            critic.AddInfo(infoCode, message);
            Critics.Add(critic);
        }
        /// <summary>
        /// Creates a list of object Critic. 
        /// Each Critic contains the error code.
        /// and the CriticType is a Warning.
        /// </summary>
        /// <param name="warningCode"></param>
        public void AddBusinessWarning(string warningCode)
        {
            string message = JsonStringLocalizer.Localize(warningCode);

            Critic critic = new Critic();
            critic.AddWarning(warningCode, message);
            Critics.Add(critic);
        }
        /// <summary>
        /// Creates a list of object Critic. 
        /// Each Critic contains the error code.
        /// and the CriticType is a Warning.
        /// </summary>
        /// <param name="warningCode"></param>
        public void AddBusinessWarning(string warningCode, params object[] args)
        {
            string message = JsonStringLocalizer.Localize(warningCode, args);

            Critic critic = new Critic();
            critic.AddWarning(warningCode, message);
            Critics.Add(critic);
        }

    }
}
