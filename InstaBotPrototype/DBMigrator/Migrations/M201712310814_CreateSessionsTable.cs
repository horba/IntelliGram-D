using System.Data.Common;

namespace DBMigrator.Migrations
{
    [Indexer(201712310814)]
    class M201712310814_CreateSessionsTable : DBMigration
    {
        public M201712310814_CreateSessionsTable(object factory, object connection) : base(factory as DbProviderFactory, connection as DbConnection)
        {
            ApplyCommand.CommandText =
            @"create table dbo.Sessions 
					(
						UserId int NOT NULL, 
						SessionId uniqueidentifier NOT NULL, 
						LoginTime DateTime DEFAULT SYSDATETIME(), 
                        LastActive DateTime DEFAULT SYSDATETIME(),
						CONSTRAINT PK_Sessions PRIMARY KEY (UserId,LoginTime), 
						CONSTRAINT FK_Sessions_Users FOREIGN KEY (UserId)     
						REFERENCES dbo.Users (Id)     
						ON DELETE CASCADE    
						ON UPDATE CASCADE
			);";
            ReverseCommand.CommandText = "drop table dbo.Sessions";
        }
    }
}