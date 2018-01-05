using System.Data.Common;

namespace DBMigrator.Migrations
{
    [Indexer(201712310814)]
    class M201712310814_CreateSessionsTable : DBMigration
    {
        public M201712310814_CreateSessionsTable(object factory, object connection) : base(factory as DbProviderFactory, connection as DbConnection)
        {
            ApplyCommand.CommandText = "create table dbo.Sessions (Id int identity primary key, UserId int FOREIGN KEY REFERENCES Users(Id), CreationDate DateTime, LastActive DateTime)";
            ReverseCommand.CommandText = "drop table dbo.Sessions";
        }
    }
}