using System.Data.Common;

namespace DBMigrator.Migrations
{
    [Indexer(2018001082220)]
    class M2018001082220_CreateTopicTable : DBMigration
    {
        public M2018001082220_CreateTopicTable(object factory, object connection) : base(factory as DbProviderFactory, connection as DbConnection)
        {
            ApplyCommand.CommandText = "create table dbo.Topic(TopicID int identity(1,1), Topic nvarchar(128) not null, primary key(TopicID))";
            ReverseCommand.CommandText = "drop table dbo.Topic";
        }
    }
}
