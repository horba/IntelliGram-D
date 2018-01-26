using System.Data.Common;
namespace DBMigrator.Migrations
{
    [Indexer(201801211350)]
    class M201801211350_CreateInstagramIntegrationTable : DBMigration
    {
        public M201801211350_CreateInstagramIntegrationTable(object factory, object connection) : base(factory as DbProviderFactory, connection as DbConnection)
        {
            ApplyCommand.CommandText =
            @"CREATE TABLE dbo.InstagramIntegration 
					(
						UserId int NOT NULL, 
						InstagramId bigint NOT NULL,
                        Nickname nvarchar(50) NOT NULL,
                        AccessToken nvarchar(100) NOT NULL,
						CONSTRAINT PK_InstagramIntegration PRIMARY KEY (InstagramId), 
						CONSTRAINT FK_InstagramIntegration_Users FOREIGN KEY (UserId)     
						REFERENCES dbo.Users (Id)     
						ON DELETE CASCADE    
						ON UPDATE CASCADE
			);";
            ReverseCommand.CommandText = "drop table dbo.InstagramIntegration";
        }
    }
}