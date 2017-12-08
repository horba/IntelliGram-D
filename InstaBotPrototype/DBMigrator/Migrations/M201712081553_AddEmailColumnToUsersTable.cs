using System.Data.Common;

namespace DBMigrator.Migrations
{
    [Indexer(201712081553)]
    class M201712081553_AddEmailColumnToUsersTable : DBMigration
    {
        public M201712081553_AddEmailColumnToUsersTable(object factory, object connection) : base(factory as DbProviderFactory, connection as DbConnection)
        {
            ApplyCommand.CommandText = "alter table dbo.Users add Email nvarchar(100)";
            ReverseCommand.CommandText = "alter table dbo.Users drop column Email";
        }
    }
}