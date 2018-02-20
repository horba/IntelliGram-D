using Microsoft.Extensions.Configuration;
using System.IO;

namespace InstaBotPrototype
{
    public class AppSettingsProvider
    {
        public static IConfiguration Config { get; private set; }
        static AppSettingsProvider() {
            Config = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory())
                                              .AddJsonFile("appsettings.json", true, true)
                                              .Build();
        }
    }
}
