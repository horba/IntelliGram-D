using System.Data.Common;

namespace DBMigrator.Migrations
{
    [Indexer(201801082220)]
    class M2018001082220_CreateTopicTable : DBMigration
    {
        public M2018001082220_CreateTopicTable(object factory, object connection) : base(factory as DbProviderFactory, connection as DbConnection)
        {
            ApplyCommand.CommandText = @"CREATE TABLE dbo.Topic
            (
            	TopicID int identity(1,1),
            	Topic nvarchar(128) NOT NULL,
            	PRIMARY KEY(TopicID)
			)";
            ReverseCommand.CommandText = "DROP TABLE dbo.Topic";
        }
    }
}
