using System.Data.Common;

namespace DBMigrator.Migrations
{
    [Indexer(201712311235)]
    class M201712311235_CreateTableSessions : DBMigration
    {
        public M201712311235_CreateTableSessions(object factory, object connection) : base(factory as DbProviderFactory, connection as DbConnection)
        {
            ApplyCommand.CommandText =
            @"create table dbo.Sessions 
					(
						Id int NOT NULL, 
						SessionId uniqueidentifier NOT NULL, 
						LoginTime DateTime DEFAULT CURRENT_TIMESTAMP, 
						CONSTRAINT PK_Sessions PRIMARY KEY (Id,LoginTime), 
						CONSTRAINT FK_Sessions FOREIGN KEY (Id)     
						REFERENCES dbo.Users (Id)     
						ON DELETE CASCADE    
						ON UPDATE CASCADE
			);";
            ReverseCommand.CommandText = "drop table dbo.Sessions";
        }
    }
}