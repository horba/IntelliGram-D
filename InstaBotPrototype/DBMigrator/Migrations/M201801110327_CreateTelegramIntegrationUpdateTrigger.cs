using System.Data.Common;

namespace DBMigrator.Migrations
{
    [Indexer(201801110327)]
    class M201801110327_CreateTelegramIntegrationUpdateTrigger : DBMigration
    {
        public M201801110327_CreateTelegramIntegrationUpdateTrigger(object factory, object connection) : base(factory as DbProviderFactory, connection as DbConnection)
        {
            ApplyCommand.CommandText =
            @"CREATE TRIGGER TelegramIntegration_Update
                ON dbo.TelegramIntegration AFTER UPDATE
                AS IF UPDATE(UserId)
            BEGIN
                DECLARE @userID INT
                SELECT @userID = (SELECT UserId FROM inserted)
                DELETE FROM dbo.TelegramVerification WHERE UserId = @userID
            END";
            ReverseCommand.CommandText = "drop table dbo.TelegramVerification";
        }
    }
}
