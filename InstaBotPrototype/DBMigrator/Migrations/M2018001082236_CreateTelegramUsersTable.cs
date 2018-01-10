using System.Data.Common;

namespace DBMigrator.Migrations
{
    [Indexer(2018001082236)]
    class M2018001082236_CreateTelegramUsersTable: DBMigration
    {
        public M2018001082236_CreateTelegramUsersTable(object factory, object connection) : base(factory as DbProviderFactory, connection as DbConnection)
        {
            ApplyCommand.CommandText = "create table dbo.TelegramUsers(ChatID int not null, FirstName nvarchar(128) not null, LastName nvarchar(128) not null, VerificationKey int, UserID int)";
            ReverseCommand.CommandText = "drop table dbo.TelegramUsers";
        }
    }
}
