using System.Data.Common;


namespace DBMigrator.Migrations
{
    [Indexer(201801082211)]
    class M201801082211_CreateConfigurationTable : DBMigration
    {
        public M201801082211_CreateConfigurationTable(object factory, object connection) : base(factory as DbProviderFactory, connection as DbConnection)
        {
            ApplyCommand.CommandText = @"CREATE TABLE Configuration 
            (
            	ConfigID int IDENTITY NOT NULL PRIMARY KEY,
            	UserId int FOREIGN KEY REFERENCES Users(Id)
            	ON DELETE CASCADE
            )";
            ReverseCommand.CommandText = "DROP DELETE dbo.Configuration";
        }
    }
}