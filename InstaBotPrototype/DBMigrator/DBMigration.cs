using System.Data.Common;
using static System.Console;

namespace DBMigrator
{
    abstract class DBMigration
    {
        public DBMigration(DbProviderFactory factory, DbConnection connection)
        {
            Factory = factory;

            Name = GetType().Name;

            ApplyCommand = Factory.CreateCommand();
            ReverseCommand = Factory.CreateCommand();

            this.connection = connection;

            var command = factory.CreateCommand();
            command.CommandText = $"select count(*) from dbo.Migrations where Name = '{Name}'";
            command.Connection = connection;

            try
            {
                if ((int)command.ExecuteScalar() == 1)
                {
                    IsApplied = true;
                }

            }
            catch
            {
                WriteLine("Error happened");
                WriteLine("Try restarting this application");
                WriteLine("If you continue getting this message, connect application distributor");
                WriteLine(string.Empty.PadRight(WindowWidth - 1, '-'));
            }
        }

        public DbCommand ApplyCommand { get; private set; }
        public DbCommand ReverseCommand { get; private set; }
        public DbProviderFactory Factory { get; private set; }
        public string Name { get; private set; }
        public bool IsApplied { get; private set; }
        DbConnection connection;

        public void Apply()
        {
            Run(ApplyCommand);
            AddLog();
        }

        public void Reverse()
        {
            Run(ReverseCommand);
            RemoveLog();
        }

        private void AddLog()
        {
            var command = Factory.CreateCommand();
            command.CommandText = $"INSERT INTO dbo.Migrations (Name, ApplyTime) VALUES ('{Name}', GetDate())";

            Run(command);
        }

        private void RemoveLog()
        {
            var command = Factory.CreateCommand();
            command.CommandText = $"delete from dbo.Migrations where Name='{Name}'";

            Run(command);
        }

        private void Run(DbCommand command)
        {
            command.Connection = connection;
            command.ExecuteNonQuery(); }
    }
}