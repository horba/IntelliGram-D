using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using System;
using System.IO;
using log4net;
using InstaBotPrototype.Services;

namespace InstaBotPrototype
{
    public class Program
    {
        private static readonly ILog log = Logger.GetLog<Program>();

        public static void Main(string[] args)
        {
            log.Debug("Entered Main method");

            var host = new WebHostBuilder()
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseIISIntegration()
                .UseStartup<Startup>()
                .UseApplicationInsights()
                .Build();

            log.Debug("Trying to run host");

            try
            {
                host.Run();
            }
            catch (Exception e)
            {
                log.Fatal("Exception in host.Run() method", e);
                throw;
            }
        }
    }
}