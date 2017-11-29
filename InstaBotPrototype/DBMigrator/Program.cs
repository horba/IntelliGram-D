using System;
using System.Configuration;
using System.Data.Common;
using System.Linq;
using System.Reflection;

namespace DBMigrator
{
    internal class Program
    {
        private static void Main()
        {
            string connString = ConfigurationManager.ConnectionStrings[1].ConnectionString;
            string provider = ConfigurationManager.ConnectionStrings[1].ProviderName;

            var factory = DbProviderFactories.GetFactory(provider);

            var dbConnection = factory.CreateConnection();
            dbConnection.ConnectionString = connString;
            

            var migrations = (from type in Assembly.GetCallingAssembly().GetTypes() where type.IsSubclassOf(typeof(DBMigration)) orderby type.GetCustomAttribute<IndexerAttribute>().Id select Activator.CreateInstance(type, new object[] { factory, dbConnection }) as DBMigration).ToList();
            

            var command = factory.CreateCommand();
            command.CommandText = "select * from dbo.Migrations";
            command.Connection = dbConnection;

            dbConnection.Open();

            try
            {
                var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    string name = (string)reader.GetValue(0);

                    foreach (var migration in migrations)
                    {
                        if (migration.Name == name)
                        {
                            migration.IsApplied = true;
                            break;
                        }
                    }
                }
            }
            catch { }


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