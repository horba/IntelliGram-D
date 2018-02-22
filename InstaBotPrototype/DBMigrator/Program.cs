using InstaBotPrototype;
using System;
using System.Data.Common;
using System.Linq;
using System.Reflection;

namespace DBMigrator
{
    internal class Program
    {
        private static void Main()
        {
            string connectionString = AppSettingsProvider.Config["connectionString"];
            DbProviderFactory factory = DbProviderFactories.GetFactoryByProvider(AppSettingsProvider.Config["dataProvider"]);
            var dbConnection = factory.CreateConnection();
            dbConnection.ConnectionString = connectionString;
            dbConnection.Open();
            var migrations = (from type in Assembly.GetCallingAssembly().GetTypes() where type.IsSubclassOf(typeof(DBMigration)) orderby type.GetCustomAttribute<IndexerAttribute>().Id select Activator.CreateInstance(type, new object[] { factory, dbConnection }) as DBMigration).ToList();
            foreach (var migration in migrations)
            {
                long id = migration.GetType().GetCustomAttribute<IndexerAttribute>().Id;
                if (migration.IsApplied)
                {
                    Console.WriteLine($"Migration {id} {migration.Name} was applied");
                }
                else
                {
                    Console.WriteLine($"Migration {id} {migration.Name} wasn't applied");
                    Console.WriteLine("Applying...");

                    migration.Apply();

                    Console.WriteLine($"Migration {id} {migration.Name} is applied");
                }

                Console.WriteLine(string.Empty.PadRight(Console.WindowWidth - 1, '-'));
            }

            dbConnection.Close();
        }
    }
}