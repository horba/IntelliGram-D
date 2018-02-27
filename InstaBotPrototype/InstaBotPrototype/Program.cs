using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using log4net;
using InstaBotPrototype.Services;
using Microsoft.AspNetCore;
using Microsoft.Extensions.Configuration;

namespace InstaBotPrototype
{
    public class Program
    {
        private static readonly ILog log = Logger.GetLog<Program>();
        public static void Main(string[] args)
        {
            log.Debug("Application run");
            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((builderContext, config) =>
                {
                    config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
                })
                .UseStartup<Startup>()
                .Build();
    }
}