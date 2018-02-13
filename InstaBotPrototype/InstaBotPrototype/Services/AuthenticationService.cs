using InstaBotPrototype.Models;
using Scrypt;
using System;
using System.Configuration;
using System.Data.Common;

namespace InstaBotPrototype.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private string connectionString = ConfigurationManager.ConnectionStrings[1].ConnectionString;
        private DbProviderFactory factory = DbProviderFactories.GetFactory(ConfigurationManager.ConnectionStrings[1].ProviderName);
        private readonly ScryptEncoder encoder = new ScryptEncoder();
        private readonly string pepper = ConfigurationManager.AppSettings["Pepper"];

        public string Login(LoginModel model)
        {
            Guid? sessionID = null;
            using (var dbConnection = factory.CreateConnection())
            {
                dbConnection.ConnectionString = connectionString;
                dbConnection.Open();
                var selectCmd = factory.CreateCommand();
                selectCmd.Connection = dbConnection;
                selectCmd.CommandText = $"SELECT Id,Password FROM dbo.Users WHERE Login = @login";
                var login = CreateParameter("@login", model.Login);
                selectCmd.Parameters.AddRange(new[] { login });
                var reader = selectCmd.ExecuteReader();
                if (reader.HasRows)
                {
                    reader.Read();
                    var id = reader.GetInt32(0);
                    var passwordHash = reader.GetString(1);
                    reader.Close();

                    if (!encoder.Compare(model.Password + pepper, passwordHash))
                    {
                        return null;
                    }

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
                var password = CreateParameter("@password", encoder.Encode(model.Password + pepper));

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