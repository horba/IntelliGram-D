﻿using System.Data.Common;

namespace DBMigrator.Migrations
{
    [Indexer(201711290745)]
    class M201711290745_CreateUsersTable : DBMigration
    {
        public M201711290745_CreateUsersTable(object factory, object connection) : base(factory as DbProviderFactory, connection as DbConnection)
        {
            ApplyCommand.CommandText = "create table dbo.Users (Id int identity, Login nvarchar(100), Password nvarchar(100), primary key(Id))";
            ReverseCommand.CommandText = "drop table dbo.Users";
        }
    }
}