using System;

namespace Liquid.Activation
{
    /// <summary>
    /// Attribute used for connect a Scheduler.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class SchedulerAttribute : Attribute
    {
        #region Private Var
        /// <summary>
        /// Private Name job
        /// </summary>
        private readonly string _name;

        #endregion

        #region Constructor
        /// <summary>
        /// Attribute of Scheduler
        /// </summary>
        /// <param name="name"></param> 
        public SchedulerAttribute()
        {
            _name = null;
        }
        /// <summary>
        /// Attribute of Scheduler
        /// </summary>
        /// <param name="name"></param> 
        public SchedulerAttribute(string name)
        {
            _name = name;
        }

        #endregion

        #region Properties
        /// <summary>
        /// Get a  Name.
        /// </summary>
        public virtual string Name
        {
            get { return _name; }
        }
        #endregion
    }
}
