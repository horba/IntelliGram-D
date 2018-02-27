using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;

namespace DBMigrator.Migrations
{
    [Indexer(201802271817)]
    class M201802271817_CreateMessageTagTable:DBMigration
    {
        public M201802271817_CreateMessageTagTable(object factory, object connection) : base(factory as DbProviderFactory, connection as DbConnection)
        {
            ApplyCommand.CommandText =
            @"  CREATE TABLE MessageTag
                    (
                        MessageID int NOT NULL,
                        TagID int NOT NULL,
                        PRIMARY KEY(MessageID,TagID)
                    );";
            ReverseCommand.CommandText =
            @"  DROP TABLE MessageTag;";
        }
    }
}
