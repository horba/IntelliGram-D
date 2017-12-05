using System.Data.Common;

namespace DBMigrator.Migrations
{
    [Indexer(201711290745)]
    class CreateUsersTable : DBMigration
    {
        public CreateUsersTable(object factory, object connection) : base(factory as DbProviderFactory, connection as DbConnection)
        {
            ApplyCommand.CommandText = "create table dbo.Users (Id int identity, Login nvarchar(100), Password nvarchar(100), primary key(Id))";
            ReverseCommand.CommandText = "drop table dbo.Users";
        }
    }
}