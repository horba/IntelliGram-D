using System.Data.Common;
namespace DBMigrator.Migrations
{
    [Indexer(201801102326)]
    class M201801102326_CreateTelegramVerificationTable : DBMigration
    {
        public M201801102326_CreateTelegramVerificationTable(object factory, object connection) : base(factory as DbProviderFactory, connection as DbConnection)
        {
            ApplyCommand.CommandText =
            @"CREATE TABLE dbo.TelegramVerification 
					(
						UserId int, 
						TelegramVerificationKey int DEFAULT (ABS(Checksum(NewID()) % 1000000000) + 1),
						CONSTRAINT PK_TelegramVerification  PRIMARY KEY (UserId), 
						CONSTRAINT FK_TelegramVerification_Users FOREIGN KEY (UserId)     
						REFERENCES dbo.Users (Id)     
						ON DELETE CASCADE    
						ON UPDATE CASCADE
			);
            ";
            ReverseCommand.CommandText = "drop table dbo.TelegramVerification";
        }
    }
}
