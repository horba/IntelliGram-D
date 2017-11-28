using System.Data.Common;

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
        }

        public DbCommand ApplyCommand { get; private set; }
        public DbCommand ReverseCommand { get; private set; }
        public DbProviderFactory Factory { get; set; }
        public string Name { get; private set; }
        public bool IsApplied { get; set; }
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