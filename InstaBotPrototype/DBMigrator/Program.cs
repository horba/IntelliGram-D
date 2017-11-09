using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
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
                    Console.WriteLine($"Migration №{i} was applied");
                }
                else
                {
                    Console.WriteLine($"Migration №{i} wasn't applied");
                    Console.WriteLine("Applying...");

                    if (migration.Apply())
                    {
                        Console.WriteLine($"Migration №{i} is applied");
                    }
                    else
                    {
                        Console.WriteLine("Something went wrong...");
                        Console.ReadLine();
                        return;
                    }
                }

                Console.WriteLine(string.Empty.PadRight(20, '-'));
            }
        }

        private static DBMigration[] GetMigrations()
        {
            List<DBMigration> list = new List<DBMigration>();

            string connString = ConfigurationManager.ConnectionStrings[0].ConnectionString;
            string provider = ConfigurationManager.ConnectionStrings[0].ProviderName;

            DbProviderFactory factory = DbProviderFactories.GetFactory(provider);

            DbConnection dbConnection = factory.CreateConnection();
            dbConnection.ConnectionString = connString;

            DirectoryInfo directory = new DirectoryInfo(Directory.GetCurrentDirectory() + "/Migrations");

            foreach (FileInfo file in directory.GetFiles("*.txt"))
            {
                using (StreamReader streamReader = new StreamReader(file.OpenRead()))
                {
                    DbCommand apply = factory.CreateCommand();
                    apply.Connection = dbConnection;
                    apply.CommandText = streamReader.ReadLine();

                    DbCommand reverse = factory.CreateCommand();
                    reverse.Connection = dbConnection;
                    reverse.CommandText = streamReader.ReadLine();

                    string name = file.Name;

                    DBMigration migration = new DBMigration(apply, reverse, name, dbConnection);

                    list.Add(migration);
                }
            }

            DbCommand command = factory.CreateCommand();
            command.CommandText = "select * from dbo.Migrations";
            command.Connection = dbConnection;

            dbConnection.Open();

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

            dbConnection.Close();

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