using System;
using InstaBotPrototype.Models;
using System.Configuration;
using System.Data.Common;

namespace InstaBotPrototype.Services
{
    public class AuthenticationService : IAuthenticationService

    {
        string connectionString = ConfigurationManager.ConnectionStrings[1].ConnectionString;
        DbProviderFactory factory = DbProviderFactories.GetFactory(ConfigurationManager.ConnectionStrings[1].ProviderName);

        public String Login(LoginModel model)
        {
            Guid? sessionID = null;
            using (var dbConnection = factory.CreateConnection())
            {
                dbConnection.ConnectionString = connectionString;
                dbConnection.Open();
                var selectID = factory.CreateCommand();
                selectID.Connection = dbConnection;
                selectID.CommandText = $"select Id from dbo.Users where Login = @login and Password = @password";
                var pLogin = CreateParameter("@login", model.Login);
                var pPassword = CreateParameter("@password", model.Password);
                selectID.Parameters.AddRange(new[] { pLogin, pPassword });
                var reader = selectID.ExecuteReader();
                if (reader.HasRows)
                {
                    reader.Read();
                    int id = reader.GetInt32(0);
                    reader.Close();
                    sessionID = Guid.NewGuid();
                    var updateLastLogin = factory.CreateCommand();
                    updateLastLogin.Connection = dbConnection;
                    updateLastLogin.CommandText = $"INSERT INTO dbo.Sessions (UserId,SessionId) VALUES (@id,@sessionID)";
                    var pId = CreateParameter("@id", id);
                    var pSessionId = CreateParameter("@sessionID", sessionID);
                    updateLastLogin.Parameters.Add(pId);
                    updateLastLogin.Parameters.Add(pSessionId);
                    updateLastLogin.ExecuteNonQuery();
                }
            }
            return sessionID?.ToString();
        }

        public String Register(LoginModel model)
        {
            var dbConnection = factory.CreateConnection();
            dbConnection.ConnectionString = connectionString;

            dbConnection.Open();

            var insert = factory.CreateCommand();
            insert.Connection = dbConnection;
            insert.CommandText = $"insert into table dbo.Users (Login, Email, Password, RegisterDate) values (@login, @email, @password, SYSDATETIME())";

            var login = CreateParameter("@login", model.Login);
            var email = CreateParameter("@email", model.Email);
            var password = CreateParameter("@password", model.Password);

            insert.Parameters.AddRange(new[] { login, email, password });

            dbConnection.Close();

            return Login(model);
        }

        DbParameter CreateParameter(string name, object value)
        {
            var parameter = factory.CreateParameter();
            parameter.ParameterName = name;
            parameter.Value = value;
            return parameter;
        }
    }
}