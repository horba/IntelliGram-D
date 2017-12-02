using System.Configuration;
using System.Data.Common;

namespace InstaBotPrototype.Services
{
    public class AuthenticationService
    {
        string connectionString = ConfigurationManager.ConnectionStrings[1].ConnectionString;
        DbProviderFactory factory = DbProviderFactories.GetFactory(ConfigurationManager.ConnectionStrings[1].ProviderName);

        public int Login(string login, string password)
        {
            var dbConnection = factory.CreateConnection();
            dbConnection.ConnectionString = connectionString;

            var select = factory.CreateCommand();
            select.Connection = dbConnection;
            select.CommandText = $"select Id from dbo.Users where Login = '{login}' and Password = '{password}'";


            dbConnection.Open();

            var reader = select.ExecuteReader();
            reader.Read();
            var id = reader.GetValue(0) as int?;

            if (id == null)
            {
                id = -1;
            }
            else
            {
            var updateLastLogin = factory.CreateCommand();
            updateLastLogin.Connection = dbConnection;
            updateLastLogin.CommandText = $"update dbo.Users LastLogin = 'current_timestamp' where Id = '{id}'";
                updateLastLogin.ExecuteNonQuery();
            }

            dbConnection.Close();

            return (int)id;
        }

        public int Register(string login, string password)
        {
            var dbConnection = factory.CreateConnection();
            dbConnection.ConnectionString = connectionString;

            dbConnection.Open();

            var insert = factory.CreateCommand();
            insert.Connection = dbConnection;
            insert.CommandText = $"insert into table dbo.Users (Login, Password) values ('{login}', '{password}')";

            dbConnection.Close();

            return Login(login, password);
        }
    }
}