using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Common;
using System.IO;

namespace DBMigrator
{
    internal class Program
    {
        #region Private Methods

        private static void ApplyMigrations(params DBMigration[] migrations)
        {
            for (int i = 0; i < migrations.Length; i++)
            {
                DBMigration migration = migrations[i];
                if (migration.IsApplied)
                {
                    Console.WriteLine($"Migration {migration.Name} was applied");
                }
                else
                {
                    Console.WriteLine($"Migration {migration.Name} wasn't applied");
                    Console.WriteLine("Applying...");

                    if (migration.Apply())
                    {
                        Console.WriteLine($"Migration {migration.Name} is applied");
                    }
                    else
                    {
                        Console.WriteLine("Something went wrong...");
                    }
                }

                Console.WriteLine(string.Empty.PadRight(30, '-'));
            }
        }

        private static DBMigration[] GetMigrations()
        {
            List<DBMigration> list = new List<DBMigration>();

            string connString = ConfigurationManager.ConnectionStrings[1].ConnectionString;
            string provider = ConfigurationManager.ConnectionStrings[1].ProviderName;

            DbProviderFactory factory = DbProviderFactories.GetFactory(provider);

            DbConnection dbConnection = factory.CreateConnection();
            dbConnection.ConnectionString = connString;

            DirectoryInfo directory = new DirectoryInfo(Directory.GetCurrentDirectory() + "/Migrations");

            foreach (FileInfo file in directory.GetFiles("*.txt"))
            {
                using (StreamReader streamReader = new StreamReader(file.OpenRead()))
                {
                    DbCommand apply = factory.CreateCommand();
                    apply.CommandText = streamReader.ReadLine();

                    DbCommand reverse = factory.CreateCommand();
                    reverse.CommandText = streamReader.ReadLine();

                    string name = file.Name.Substring(0, file.Name.Length - 4);

                    DBMigration migration = new DBMigration(apply, reverse, name) { Factory = factory, ConnectionString = connString };

                    list.Add(migration);
                }
            }

            dbConnection.Close();

            DbCommand command = factory.CreateCommand();
            command.CommandText = "select * from dbo.Migrations";
            command.Connection = factory.CreateConnection();
            command.Connection.ConnectionString = connString;

            command.Connection.Open();

            try
            {
                DbDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    string name = (string)reader.GetValue(0);

                    foreach (DBMigration migration in list)
                    {
                        if (migration.Name == name)
                        {
                            migration.IsApplied = true;
                            break;
                        }
                    }
                }
            }
            catch
            {
            }
            finally
            {
                command?.Connection?.Close();
            }

            return list.ToArray();
        }

        private static void Main(string[] args)
        {
            DBMigration[] migrations = GetMigrations();
            ApplyMigrations(migrations);
        }

        #endregion Private Methods
    }
}