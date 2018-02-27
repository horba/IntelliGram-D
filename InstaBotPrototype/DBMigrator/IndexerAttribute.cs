using System;

namespace DBMigrator
{
    class IndexerAttribute : Attribute
    {
        public long Id { get; private set; }
        public IndexerAttribute(long id) => Id = id;
    }
}
