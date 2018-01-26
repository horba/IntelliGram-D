using System.Data.Common;

namespace DBMigrator.Migrations
{
    [Indexer(2018001082226)]
    class M2018001082226_CreateConfigTopicTable : DBMigration
    {
        public M2018001082226_CreateConfigTopicTable(object factory, object connection) : base(factory as DbProviderFactory, connection as DbConnection)
        {
            ApplyCommand.CommandText = "create table dbo.ConfigTopic(ConfigID int not null, TopicID int not null,primary key(ConfigID,TopicID))";
            ReverseCommand.CommandText = "drop table dbo.ConfigTopic";
        }
    }
}