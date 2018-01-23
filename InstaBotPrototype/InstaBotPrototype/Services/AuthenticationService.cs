using InstaBotPrototype.Models;
using System;
using System.Configuration;
using System.Data.Common;

namespace InstaBotPrototype.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        string connectionString = ConfigurationManager.ConnectionStrings[1].ConnectionString;
        DbProviderFactory factory = DbProviderFactories.GetFactory(ConfigurationManager.ConnectionStrings[1].ProviderName);

        public int? GetVerifyKey(LoginModel model)
        {
            int? verifyKey = null;
            using (var dbConnection = factory.CreateConnection())
            {
                dbConnection.ConnectionString = connectionString;
                dbConnection.Open();
                var selectID = factory.CreateCommand();
                selectID.Connection = dbConnection;
                selectID.CommandText = $"SELECT Id FROM dbo.Users WHERE Login = @login and Password = @password";
                var login = CreateParameter("@login", model.Login);
                var password = CreateParameter("@password", model.Password);
                selectID.Parameters.AddRange(new[] { login, password });
                var readerID = selectID.ExecuteReader();
                if (readerID.HasRows)
                {
                    readerID.Read();
                    var id = readerID.GetInt32(0);
                    readerID.Close();
                    var selectTelegram = factory.CreateCommand();
                    selectTelegram.Connection = dbConnection;
                    selectTelegram.CommandText = $"SELECT TelegramVerificationKey FROM dbo.TelegramVerification WHERE UserId = @id";
                    selectTelegram.Parameters.Add(CreateParameter("@id", id));
                    var readerKey = selectTelegram.ExecuteReader();
                    if (readerKey.HasRows)
                    {
                        readerKey.Read();
                        verifyKey = readerKey.GetInt32(0);
                    }
                    readerKey.Close();
                }
            }
            return verifyKey;
        }

        public string Login(LoginModel model)
        {
            Guid? sessionID = null;
            using (var dbConnection = factory.CreateConnection())
            {
                dbConnection.ConnectionString = connectionString;
                dbConnection.Open();
                var selectID = factory.CreateCommand();
                selectID.Connection = dbConnection;
                selectID.CommandText = $"SELECT Id FROM dbo.Users WHERE Login = @login and Password = @password";
                var login = CreateParameter("@login", model.Login);
                var password = CreateParameter("@password", model.Password);
                selectID.Parameters.AddRange(new[] { login, password });
                var reader = selectID.ExecuteReader();
                if (reader.HasRows)
                {
                    reader.Read();
                    var id = reader.GetInt32(0);
                    reader.Close();
                    sessionID = Guid.NewGuid();
                    var insertSession = factory.CreateCommand();
                    insertSession.Connection = dbConnection;
                    insertSession.CommandText = $"INSERT INTO dbo.Sessions (UserId,SessionId) VALUES (@id,@sessionID)"; ;
                    insertSession.Parameters.Add(CreateParameter("@id", id));
                    insertSession.Parameters.Add(CreateParameter("@sessionID", sessionID));
                    insertSession.ExecuteNonQuery();
                }
            }
            return sessionID?.ToString();
        }

        public string Register(LoginModel model)
        {
            var dbConnection = factory.CreateConnection();
            dbConnection.ConnectionString = connectionString;

            dbConnection.Open();

            var insert = factory.CreateCommand();
            insert.Connection = dbConnection;
            insert.CommandText = $"INSERT INTO dbo.Users (Login, Email, Password, RegisterDate) VALUES (@login, @email, @password, SYSDATETIME())";

            var login = CreateParameter("@login", model.Login);
            var email = CreateParameter("@email", model.Email);
            var password = CreateParameter("@password", model.Password);

            insert.Parameters.AddRange(new[] { login, email, password });
            insert.ExecuteNonQuery();
            dbConnection.Close();

            return Login(model);
        }

        private DbParameter CreateParameter(string name, object value)
        {
            var parameter = factory.CreateParameter();
            parameter.ParameterName = name;
            parameter.Value = value;
            return parameter;
        }
    }
}