using System;
using System.Collections.Generic;
using System.Text;

namespace Liquid.Runtime.Configuration
{
    /// <summary>
    /// Class to access all configurations of Liquid Framework
    /// </summary>
    public class LightSettings
    {
        /// <summary>
        /// Database Settings
        /// </summary>
        public DatabaseSettings DatabaseSettings { get; set; }
        /// <summary>
        /// Service Bus settings
        /// </summary>
        public ServiceBusSettings ServiceBusSettings { get; set; }
    }

    public class DatabaseSettings
    {
        public CosmosConnection CosmosConnection { get; set; }
        public MongoConnection MongoConnection { get; set; }
        public DynamoConnection DynamoConnection { get; set; }

    }
    public class CosmosConnection
    {
        /// <summary>
        /// The Host string to connect the database
        /// </summary>
        public string Host { get; set; }
        /// <summary>
        /// The number Port to connect the database
        /// </summary>
        public string Port { get; set; }
        /// <summary>
        /// The User name to connect the database
        /// </summary>
        public string UserName { get; set; }
        /// <summary>
        /// The Password to connect the database
        /// </summary>
        public string PrimaryPassword { get; set; }
        /// <summary>
        /// The Connection string to connect the database
        /// </summary>
        public string PrimaryConnectionString { get; set; }
        /// <summary>
        /// The boolean string to apply SSL connection
        /// </summary>
        public string SSL { get; set; }
    }

    public class MongoConnection
    {
        /// <summary>
        /// The Connection string to connect the database
        /// </summary>
        public string ConnectionString { get; set; }
        /// <summary>
        /// The Database name
        /// </summary>
        public string Database { get; set; }
    }

    public class DynamoConnection
    {
        /// <summary>
        /// The Connection string to connect the database
        /// </summary>
        public string ConnectionString { get; set; }
        /// <summary>
        /// The Database name
        /// </summary>
        public string Database { get; set; }
    }

    public class ServiceBusSettings
    {
        /// <summary>
        /// The Address to connect the service
        /// </summary>
        public string Address { get; set; }
        /// <summary>
        /// The Server name to connect the service
        /// </summary>
        public string Server { get; set; }
        /// <summary>
        /// The User name to connect the service
        /// </summary>
        public string Username { get; set; }
        /// <summary>
        /// The Password to connect the service
        /// </summary>
        public string Password { get; set; }
    }
}
