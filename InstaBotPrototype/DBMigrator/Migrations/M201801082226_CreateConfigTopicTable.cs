using System.Data.Common;

namespace DBMigrator.Migrations
{
    [Indexer(201801082226)]
    class M201801082226_CreateConfigTopicTable : DBMigration
    {
        public M201801082226_CreateConfigTopicTable(object factory, object connection) : base(factory as DbProviderFactory, connection as DbConnection)
        {
            ApplyCommand.CommandText = @"CREATE TABLE dbo.ConfigTopic
            (
	            ConfigID int NOT NULL, 
	            TopicID int NOT NULL,
	            PRIMARY KEY (ConfigID,TopicID),
                CONSTRAINT FK_ConfigTopicId
                FOREIGN KEY(ConfigID) REFERENCES Configuration(ConfigID),
                CONSTRAINT FK_TopicId
                FOREIGN KEY(TopicID) REFERENCES Topic(TopicID)
		    )";
            ReverseCommand.CommandText = "DROP TABLE dbo.ConfigTopic";
        }
    }
}