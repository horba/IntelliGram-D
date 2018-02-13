﻿using System.Data.Common;

namespace DBMigrator.Migrations
{
    [Indexer(201711290745)]
    class M201711290745_CreateUsersTable : DBMigration
    {
        public M201711290745_CreateUsersTable(object factory, object connection) : base(factory as DbProviderFactory, connection as DbConnection)
        {
            ApplyCommand.CommandText = @"CREATE TABLE dbo.Users 
           								(Id int identity, 
            							Login nvarchar(60) NOT NULL,
             							Password nvarchar(128) NOT NULL,
             							RegisterDate DateTime,
             							Email nvarchar(100),
            							PRIMARY KEY(Id))";
            ReverseCommand.CommandText = "DROP TABLE dbo.Users";
        }
    }
}