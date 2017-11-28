using System.Data.Common;

namespace DBMigrator
{
    [Indexer(0)]
    class CreateMigrationsTable : DBMigration
    {
        public CreateMigrationsTable(object factory, object connection) : base(factory as DbProviderFactory, connection as DbConnection)
        {
            ApplyCommand.CommandText = "create table dbo.Migrations (Name nvarchar(100), ApplyTime DateTime)";
            ReverseCommand.CommandText = "drop table dbo.Migrations";
        }
    }
}