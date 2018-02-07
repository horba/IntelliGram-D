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
                    insertSession.CommandText = $"INSERT INTO dbo.Sessions (UserId,SessionId) VALUES (@id,@sessionID)";
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

            var checkUserExists = factory.CreateCommand();
            checkUserExists.Connection = dbConnection;
            checkUserExists.CommandText = $"SELECT COUNT(Login) FROM dbo.Users WHERE Login = @login";
            var login = CreateParameter("@login", model.Login);
            checkUserExists.Parameters.Add(login);
            int usersCount = Convert.ToInt32(checkUserExists.ExecuteScalar());
            if (usersCount == 0)
            {
                var insert = factory.CreateCommand();
                insert.Connection = dbConnection;
                insert.CommandText = $"INSERT INTO dbo.Users (Login, Email, Password, RegisterDate) VALUES (@login, @email, @password, SYSDATETIME())";

                login = CreateParameter("@login", model.Login);
                var email = CreateParameter("@email", model.Email);
                var password = CreateParameter("@password", model.Password);

                insert.Parameters.AddRange(new[] { login, email, password });
                insert.ExecuteNonQuery();
                dbConnection.Close();

                return Login(model);
            }
            else
            {
                return null;
            }
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