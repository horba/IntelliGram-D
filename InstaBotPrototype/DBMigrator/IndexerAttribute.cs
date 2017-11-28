using System;

namespace DBMigrator
{
    class IndexerAttribute : Attribute
    {
        public int Id { get; set; }
        public IndexerAttribute(int id) => Id = id;
    }
}