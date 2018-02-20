using InstaBotPrototype.Models;
using Microsoft.Extensions.Configuration;
using Scrypt;
using System;
using System.Data.SqlClient;
using System.IO;

namespace InstaBotPrototype.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private string connectionString = AppSettingsProvider.Config["connectionString"];
        private readonly ScryptEncoder encoder = new ScryptEncoder();
        private readonly string pepper = null;

        public string Login(LoginModel model)
        {
            Guid? sessionID = null;
            using (var dbConnection = new SqlConnection())
            {
                dbConnection.ConnectionString = connectionString;
                dbConnection.Open();
                var selectCmd = new SqlCommand();
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
                    var insertSession = new SqlCommand();
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
            var dbConnection = new SqlConnection();
            dbConnection.ConnectionString = connectionString;
            dbConnection.Open();

            var checkUserExists = new SqlCommand();
            checkUserExists.Connection = dbConnection;
            checkUserExists.CommandText = $"SELECT COUNT(Login) FROM dbo.Users WHERE Login = @login";
            var login = CreateParameter("@login", model.Login);
            checkUserExists.Parameters.Add(login);
            int usersCount = Convert.ToInt32(checkUserExists.ExecuteScalar());
            if (usersCount == 0)
            {
                var insert = new SqlCommand();
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

        private SqlParameter CreateParameter(string name, object value)
        {
            var parameter = new SqlParameter
            {
                ParameterName = name,
                Value = value
            };
            return parameter;
        }
    }
}