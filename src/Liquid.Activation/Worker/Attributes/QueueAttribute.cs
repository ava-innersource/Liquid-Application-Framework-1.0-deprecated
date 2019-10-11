using System;
using System.Collections.Generic;
using System.Text;

namespace Liquid.Activation
{
    /// <summary>
    /// Attribute used for connect a Queue.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class QueueAttribute : Attribute
    {
        /// <summary>
        /// Private Queue Name.
        /// </summary>
        private readonly string _queue;
        /// <summary>
        /// Private Delete after Read
        /// </summary>
        private readonly bool _deleteAfterRead;
        /// <summary>
        /// Private Take Quantity
        /// </summary>
        private readonly int _takeQuantity;



        /// <summary>
        /// Constructor used to inform a Queue name.
        /// </summary>
        /// <param name="queue">Queue Name</param>
        /// <param name="takeQuantity">Quantity to take in unit process, by default 10</param>
        /// <param name="deleteAfterRead">Delete after read the message? by default true</param>
        public QueueAttribute(string queue, int takeQuantity = 10, bool deleteAfterRead = true) {
            _queue = queue;
            _takeQuantity = takeQuantity;
            _deleteAfterRead = deleteAfterRead;
        }


        /// <summary>
        /// Get a Queue Name.
        /// </summary>
        public virtual string QueueName
        {
            get { return _queue; }
        }

        /// <summary>
        /// Take Quantity
        /// </summary>
        public virtual int TakeQuantity
        {
            get { return _takeQuantity; }
        }

        /// <summary>
        /// Delete after Read
        /// </summary>
        public virtual bool DeleteAfterRead
        {
            get { return _deleteAfterRead; }
        }

    }
}
