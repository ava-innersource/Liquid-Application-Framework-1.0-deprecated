using System;
using System.Linq.Expressions;

namespace Liquid.Activation
{
    /// <summary>
    /// Attribute used for connect a Topic and Subscription.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class TopicAttribute : Attribute
    {
        /// <summary>
        /// Private Topic Name
        /// </summary>
        private readonly string _topic;
        /// <summary>
        /// Private Subscription Name
        /// </summary>
        private readonly string _subscription;
        /// <summary>
        /// Private Sql Filter string
        /// </summary>
        private readonly string _sqlFilter;
        /// <summary>
        /// Private Delete after Read
        /// </summary>
        private readonly bool _deleteAfterRead;
        /// <summary>
        /// Private Take Quantity
        /// </summary>
        private readonly int _takeQuantity;



        /// <summary>
        /// Constructor used to inform a Topic, Subscription name and Sql Filter.
        /// </summary>
        /// <param name="topicName">Topic Name</param>
        /// <param name="subscriberName">Subscription Name</param>
        /// <param name="sqlfilter">SQL Filter</param>
        public TopicAttribute(string topicName, string subscriberName, int takeQuantity = 10, bool deleteAfterRead = true, string sqlfilter = "")
        {
            _topic = topicName;
            _subscription = subscriberName;
            _sqlFilter = sqlfilter;
            _takeQuantity = takeQuantity;
            _deleteAfterRead = deleteAfterRead;
        }


        /// <summary>
        /// Topic Name
        /// </summary>
        public virtual string TopicName
        {
            get { return _topic; }
        }

        /// <summary>
        /// Subscription Name
        /// </summary>
        public virtual string Subscription
        {
            get { return _subscription; }
        }

        /// <summary>
        /// SQL Filter string
        /// </summary>
        public virtual string SqlFilter
        {
            get { return _sqlFilter; }
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