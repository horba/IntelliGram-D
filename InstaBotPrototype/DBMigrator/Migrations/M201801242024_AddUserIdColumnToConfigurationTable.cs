using System.Data.Common;

namespace DBMigrator.Migrations
{
    [Indexer(201801242024)]
    class M201801242024_AddUserIdColumnToConfigurationTable : DBMigration
    {
        public M201801242024_AddUserIdColumnToConfigurationTable(object factory, object connection) : base(factory as DbProviderFactory, connection as DbConnection)
        {
            ApplyCommand.CommandText =
            @"ALTER TABLE Configuration ADD UserId int NOT NULL
                     CONSTRAINT FK_Configuration_Users FOREIGN KEY (UserId)     
				     REFERENCES dbo.Users (Id)     
				     ON DELETE CASCADE    
				     ON UPDATE CASCADE";
            ReverseCommand.CommandText = "ALTER TABLE dbo.Configuration DROP COLUMN UserId";
        }
    }
    
}

