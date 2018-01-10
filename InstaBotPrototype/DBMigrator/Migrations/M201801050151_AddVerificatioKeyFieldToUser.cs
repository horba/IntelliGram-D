using System.Data.Common;

namespace DBMigrator.Migrations
{
    [Indexer(201801100151)]
    class M201801050151_AddVerificatioKeyFieldToUser: DBMigration
    {
        public M201801050151_AddVerificatioKeyFieldToUser(object factory, object connection) : base(factory as DbProviderFactory, connection as DbConnection)
        {
            ApplyCommand.CommandText = @"ALTER TABLE dbo.Users 
                                           ADD TelegramVerificationKey int DEFAULT (ABS(Checksum(NewID()) % 1000000000) + 1);";
            ReverseCommand.CommandText = @"ALTER TABLE dbo.Users 
                                           DROP COLUMN TelegramVerificationKey";
        }
    }
}
