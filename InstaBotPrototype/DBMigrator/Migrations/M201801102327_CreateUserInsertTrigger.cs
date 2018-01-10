using System.Data.Common;

namespace DBMigrator.Migrations
{
    [Indexer(201801102327)]
    class M201801102327_CreateUserInsertTrigger : DBMigration
    {
        public M201801102327_CreateUserInsertTrigger(object factory, object connection) : base(factory as DbProviderFactory, connection as DbConnection)
        {
            ApplyCommand.CommandText =
            @"CREATE TRIGGER TR_Users_Insert
                ON dbo.Users AFTER INSERT
                AS IF UPDATE(Id)
            BEGIN
                DECLARE @userID INT
                SELECT @userID = (SELECT Id FROM inserted)
                INSERT INTO dbo.TelegramVerification (UserId) VALUES
                    (@userID)
            END";
            ReverseCommand.CommandText = "drop table dbo.TelegramVerification";
        }
    }
    }

