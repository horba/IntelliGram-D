using System.Data.Common;

namespace DBMigrator.Migrations
{
    [Indexer(201712050618)]
    class M201712050618_AddLastLoginFieldToUsersTable : DBMigration
    {
        public M201712050618_AddLastLoginFieldToUsersTable(object factory, object connection) : base(factory as DbProviderFactory, connection as DbConnection)
        {
            ApplyCommand.CommandText = "alter table dbo.Users add LastLogin DateTime";
            ReverseCommand.CommandText = "alter table dbo.Users drop column LastLogin";
        }
    }
}