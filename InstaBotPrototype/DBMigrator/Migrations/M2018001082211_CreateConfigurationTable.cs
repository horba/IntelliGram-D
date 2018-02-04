using System.Data.Common;


namespace DBMigrator.Migrations
{
    [Indexer(201801082211)]
    class M201801082211_CreateConfigurationTable : DBMigration
    {
        public M201801082211_CreateConfigurationTable(object factory, object connection) : base(factory as DbProviderFactory, connection as DbConnection)
        {
            ApplyCommand.CommandText = "create table dbo.Configuration (ConfigID int identity, InstaUsername nvarchar(128),InstaPassword nvarchar(128),TelegramUsername nvarchar(128),primary key(ConfigID))";
            ReverseCommand.CommandText = "drop table dbo.Configuration";
        }
    }
}