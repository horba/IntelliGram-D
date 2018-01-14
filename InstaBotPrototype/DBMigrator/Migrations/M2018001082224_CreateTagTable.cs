using System.Data.Common;

namespace DBMigrator.Migrations
{
    [Indexer(2018001082224)]
    class M2018001082224_CreateTagTable : DBMigration
    {
        public M2018001082224_CreateTagTable(object factory, object connection) : base(factory as DbProviderFactory, connection as DbConnection)
        { 
            ApplyCommand.CommandText = "create table dbo.Tag(TagID int identity(1,1), Tag nvarchar(128) not null, primary key(TagID))";
            ReverseCommand.CommandText = "drop table dbo.Tag";
        }
    }
}
