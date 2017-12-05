using System.Data.Common;

namespace DBMigrator.Migrations
{
    [Indexer(201712050618)]
    class AddLastLoginFieldToUsersTable:DBMigration
    {
        public AddLastLoginFieldToUsersTable(object factory, object connection) : base(factory as DbProviderFactory, connection as DbConnection)
        {
            ApplyCommand.CommandText = "alter table dbo.Users add LastLogin DateTime";
            ReverseCommand.CommandText = "alter table dbo.Users drop column LastLogin";
        }
    }
}