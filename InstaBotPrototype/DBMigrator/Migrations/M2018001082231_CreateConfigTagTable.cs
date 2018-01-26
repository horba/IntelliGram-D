using System.Data.Common;

namespace DBMigrator.Migrations
{
    [Indexer(2018001082231)]
    class M2018001082231_CreateConfigTagTable : DBMigration
    {
        public M2018001082231_CreateConfigTagTable(object factory, object connection) : base(factory as DbProviderFactory, connection as DbConnection)
        {
            ApplyCommand.CommandText = "create table dbo.ConfigTag(ConfigID int not null, TagID int not null, primary key(ConfigID,TagID))";
            ReverseCommand.CommandText = "drop table dbo.ConfigTag";
        }
    }
}