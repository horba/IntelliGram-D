﻿using System.Data.Common;

namespace DBMigrator.Migrations
{
    [Indexer(201801082231)]
    class M2018001082231_CreateConfigTagTable : DBMigration
    {
        public M2018001082231_CreateConfigTagTable(object factory, object connection) : base(factory as DbProviderFactory, connection as DbConnection)
        {
            ApplyCommand.CommandText = @"CREATE TABLE dbo.ConfigTag
            (
            ConfigID int NOT NULL,
            TagID int NOT NULL,
            PRIMARY KEY(ConfigID,TagID),
            CONSTRAINT FK_ConfigTagId
			FOREIGN KEY(ConfigID) REFERENCES Configuration(ConfigID),
			CONSTRAINT FK_TagId
			FOREIGN KEY(TagID) REFERENCES Tag(TagID)
            )";
            ReverseCommand.CommandText = "DROP TAB dbo.ConfigTag";
        }
    }
}