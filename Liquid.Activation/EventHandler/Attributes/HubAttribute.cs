using System;
using System.Collections.Generic;
using System.Text;

namespace Liquid.Activation
{
    /// <summary>
    /// Attribute used for connect a Hub.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class HubAttribute : Attribute
    {
        #region Private Var
        /// <summary>
        /// Private configuration name.
        /// </summary>
        private readonly string _config;

        /// <summary>
        /// Private Hub Name.
        /// </summary>
        private readonly string _hubname;

        #endregion

        #region Constructor

        /// <summary>
        /// Constructor used to inform a Hub name.
        /// </summary>
        /// <param name="hubname">Hub Name</param>
        public HubAttribute(string config, string hubname)
        {
            _config = config;
            _hubname = hubname;
        }

        #endregion

        #region Properties
        /// <summary>
        /// Get a Config Tag Hub Name.
        /// </summary>
        public virtual string ConfigTagName
        {
            get { return _config; }
        }

        /// <summary>
        /// Get a Hub Name.
        /// </summary>
        public virtual string HubName
        {
            get { return _hubname; }
        }

        #endregion
    }
}
