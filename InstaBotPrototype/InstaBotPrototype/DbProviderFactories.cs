using System.Data.Common;
namespace InstaBotPrototype
{
    public class DbProviderFactories
    {
        public static DbProviderFactory GetFactoryByProvider(string provider) {
            if (provider != null && provider.Equals("System.Data.SqlClient"))
                return System.Data.SqlClient.SqlClientFactory.Instance;
            else
                return Microsoft.Data.Sqlite.SqliteFactory.Instance;
        }
    }
}
