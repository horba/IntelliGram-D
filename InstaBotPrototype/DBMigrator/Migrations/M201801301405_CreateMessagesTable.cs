using System.Data.Common;

namespace DBMigrator.Migrations
{
    [Indexer(201801301405)] 
    class M201801301405_CreateMessagesTable : DBMigration 
    {
        public M201801301405_CreateMessagesTable(object factory, object connection) : base(factory as DbProviderFactory, connection as DbConnection) 
        {
            ApplyCommand.CommandText = @"CREATE TABLE dbo.Messages
                                    (
	                                    Id int IDENTITY NOT NULL,
	                                    ChatId bigint NOT NULL,
	                                    Message varchar(200) NOT NULL,
	                                    Timestamp datetime NOT NULL,
	                                    Send datetime NULL,
	                                    PRIMARY KEY(Id),
	                                    FOREIGN KEY(ChatId) REFERENCES TelegramIntegration(ChatId)
                                    )";
            ReverseCommand.CommandText = "drop table dbo.Messages";
        }
    } 
}