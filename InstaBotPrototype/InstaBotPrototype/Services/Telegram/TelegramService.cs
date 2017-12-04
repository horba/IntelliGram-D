using System;
using System.IO;
using System.Configuration;
using System.Data.Common;
namespace InstaBotPrototype.Services.Telegram
{
    public class TelegramService : ITelegramService
    {
        public int Connect(string username)
        {
            throw new NotImplementedException();
        }

        public void SendMessage(string message, int userID)
        {
            var dataProvider = ConfigurationManager.ConnectionStrings["BotDB"].ProviderName;
            var connectionString = ConfigurationManager.ConnectionStrings["BotDB"].ConnectionString;

            var factory = DbProviderFactories.GetFactory(dataProvider);
            DbConnection dbConnection = factory.CreateConnection();
            dbConnection.ConnectionString = connectionString;
            dbConnection.Open();

            var getLogin = factory.CreateCommand();
            getLogin.Connection = dbConnection;
            getLogin.CommandText = "select Login from dbo.Users where ID = @id";
            var idParameter = factory.CreateParameter();
            idParameter.Value = userID;
            idParameter.ParameterName = "@id";
            String login = null;
            var reader = getLogin.ExecuteReader();
            if (reader.HasRows)
            {
                reader.Read();
                login = reader.GetString(0);
            }
            reader.Close();
            dbConnection.Close();

            if (login != null)
                File.AppendAllText("../../TelegramLog.txt", String.Format("[{0}] Receiver {1}: {2} {3}", DateTime.Now, login, message, Environment.NewLine));
            else
                throw new Exception("There is no user with this identifier");
        }
    }
}
