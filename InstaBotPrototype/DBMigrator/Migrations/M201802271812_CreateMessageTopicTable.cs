using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;

namespace DBMigrator.Migrations
{
    [Indexer(201802271812)]
    class M201802271812_CreateMessageTopicTable : DBMigration
    {
        public M201802271812_CreateMessageTopicTable(object factory, object connection) : base(factory as DbProviderFactory, connection as DbConnection)
        {
            ApplyCommand.CommandText =
            @"  CREATE TABLE MessageTopic
                    (
                        MessageID int NOT NULL,
                        TopicID int NOT NULL,
                        PRIMARY KEY(MessageID,TopicID)
                    );";
            ReverseCommand.CommandText =
            @"  DROP TABLE MessageTopic;";
        }
    }
}
