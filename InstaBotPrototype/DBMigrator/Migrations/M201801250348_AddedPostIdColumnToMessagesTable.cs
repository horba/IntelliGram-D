using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;

namespace DBMigrator.Migrations
{
    [Indexer(201801250348)]
    class M201801250348_AddedPostIdColumnToMessagesTable : DBMigration
    {
        public M201801250348_AddedPostIdColumnToMessagesTable(object factory, object connection) : base(factory as DbProviderFactory, connection as DbConnection)
        {
            ApplyCommand.CommandText =
            @"  ALTER TABLE Messages
                ADD PostId nvarchar(50);";
            ReverseCommand.CommandText = 
            @"  ALTER TABLE Messages
                DROP  COLUMN PostId;";
        }
    }
}
