using System.Data.Common;

namespace DBMigrator.Migrations
{
    [Indexer(201712051755)]
    class M201712051755_AddRegisterDateFieldToUsersTable : DBMigration
    {
        public M201712051755_AddRegisterDateFieldToUsersTable(object factory, object connection) : base(factory as DbProviderFactory, connection as DbConnection)
        {
            ApplyCommand.CommandText = "alter table dbo.Users add RegisterDate DateTime";
            ReverseCommand.CommandText = "alter table dbo.Users drop column RegisterDate";
        }
    }
}