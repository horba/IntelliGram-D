using System.Data.Common;

namespace DBMigrator.Migrations
{
    [Indexer(201711290739)] // Apply attribute (index must apply format yyyymmddhhmm)
    class M201711290739_CreateMigrationsTable : DBMigration // Derive from base class
    {
        public M201711290739_CreateMigrationsTable(object factory, object connection) : base(factory as DbProviderFactory, connection as DbConnection) // Call base constructor
        {
            ApplyCommand.CommandText = "create table dbo.Migrations (Name nvarchar(100), ApplyTime DateTime)"; // Hardcode database apply ...
            ReverseCommand.CommandText = "drop table dbo.Migrations"; // ... and reverse commands
        }
    } // Well done!
}