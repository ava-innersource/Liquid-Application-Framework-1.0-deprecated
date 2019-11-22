using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace Liquid.Sample.CarRegistry
{
#pragma warning disable S1118
    /// <summary>
    /// Class Program
    /// </summary> 
    public class Program
#pragma warning restore S1118
    {
        /// <summary>
        /// Main class of project
        /// </summary>
        /// <param name="args">args</param>
        public static void Main(string[] args)
        {
            BuildWebHost(args).Run();
        }

        /// <summary>
        /// Method to build a web host
        /// </summary>
        /// <param name="args">args</param>
        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .Build();
    }
}