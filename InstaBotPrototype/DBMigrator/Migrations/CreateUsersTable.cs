using System.Data.Common;

namespace DBMigrator
{
    [Indexer(1)]
    class CreateUsersTable : DBMigration
    {
        public CreateUsersTable(object factory, object connection) : base(factory as DbProviderFactory, connection as DbConnection)
        {
            ApplyCommand.CommandText = "create table dbo.Users (Id integer, Login nvarchar(100), Password nvarchar(100), primary key(Id))";
            ReverseCommand.CommandText = "drop table dbo.Users";
        }
    }
}