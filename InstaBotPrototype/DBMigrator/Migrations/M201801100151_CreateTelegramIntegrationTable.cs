using System.Data.Common;
namespace DBMigrator.Migrations
{
    [Indexer(201801100151)]
    class M201801100151_CreateTelegramIntegrationTable : DBMigration
    {
        public M201801100151_CreateTelegramIntegrationTable(object factory, object connection) : base(factory as DbProviderFactory, connection as DbConnection)
        {
            ApplyCommand.CommandText =
            @"CREATE TABLE dbo.TelegramIntegration 
					(
						UserId int, 
						ChatId bigint NOT NULL,
                        FirstName nvarchar(50) NOT NULL,
                        LastName nvarchar(50) NULL,
						CONSTRAINT PK_TelegramIntegration PRIMARY KEY (ChatID), 
						CONSTRAINT FK_TelegramIntegration_Users FOREIGN KEY (UserId)     
						REFERENCES dbo.Users (Id)     
						ON DELETE CASCADE    
						ON UPDATE CASCADE
			);";
            ReverseCommand.CommandText = "drop table dbo.TelegramIntegration";
        }
    }
}