using InstaBotPrototype.Models;
using System.Configuration;
using System.Data.Common;

namespace InstaBotPrototype.Services
{
    public class AuthenticationService
    {
        string connectionString = ConfigurationManager.ConnectionStrings[1].ConnectionString;
        DbProviderFactory factory = DbProviderFactories.GetFactory(ConfigurationManager.ConnectionStrings[1].ProviderName);

        public int Login(LoginModel model)
        {
            var dbConnection = factory.CreateConnection();
            dbConnection.ConnectionString = connectionString;

            var select = factory.CreateCommand();
            select.Connection = dbConnection;
            select.CommandText = $"select Id from dbo.Users where Login = @login and Password = @password";
            
            var login = GetParameter("@login", model.Login);
            var password = GetParameter("@password", model.Password);

            select.Parameters.AddRange(new[] { login, password });

            dbConnection.Open();

            var reader = select.ExecuteReader();
            reader.Read();
            var id = reader.GetValue(0) as int?;

            if (!id.HasValue)
            {
                id = -1;
            }
            else
            {
                var updateLastLogin = factory.CreateCommand();
                updateLastLogin.Connection = dbConnection;
                updateLastLogin.CommandText = $"update dbo.Users set LastLogin = SYSDATETIME() where Id = @id";

                var pId = GetParameter("@id", id.Value);

                updateLastLogin.Parameters.Add(pId);

                updateLastLogin.ExecuteNonQuery();
            }

            dbConnection.Close();

            return id.Value;
        }

        public int Register(LoginModel model)
        {
            var dbConnection = factory.CreateConnection();
            dbConnection.ConnectionString = connectionString;

            dbConnection.Open();

            var insert = factory.CreateCommand();
            insert.Connection = dbConnection;
            insert.CommandText = $"insert into table dbo.Users (Login, Email, Password, RegisterDate) values (@login, @email, @password, SYSDATETIME())";

            var login = GetParameter("@login", model.Login);
            var email = GetParameter("@email", model.Email);
            var password = GetParameter("@password", model.Password);

            insert.Parameters.AddRange(new[] { login, email, password });

            dbConnection.Close();

            return Login(model);
        }

        DbParameter GetParameter(string name, object value)
        {
            var parameter = factory.CreateParameter();
            parameter.ParameterName = name;
            parameter.Value = value;
            return parameter;
        }
    }
}