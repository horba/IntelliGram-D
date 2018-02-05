using System.Data.Common;

namespace DBMigrator.Migrations
{
    [Indexer(201801082224)]
    class M201801082224_CreateTagTable : DBMigration
    {
        public M201801082224_CreateTagTable(object factory, object connection) : base(factory as DbProviderFactory, connection as DbConnection)
        {
            ApplyCommand.CommandText = @"CREATE TABLE dbo.Tag
            (
            	TagID int IDENTITY(1,1),
            	Tag nvarchar(128) NOT NULL,
            	PRIMARY KEY(TagID)
            )";
            ReverseCommand.CommandText = "DROP TABLE dbo.Tag";
        }
    }
}