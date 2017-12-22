using InstaBotPrototype.Models;
using System.Configuration;
using System.Data.Common;

namespace InstaBotPrototype.Services
{
    public class AuthenticationService: IAuthenticationService
    {
        string connectionString = @"Data Source=MYDESKTOP\SQLEXPRESS;Initial Catalog = InstaBot; Integrated Security = True";
        DbProviderFactory factory = DbProviderFactories.GetFactory("System.Data.SqlClient");

        public int Login(LoginModel model)
        {
            var dbConnection = factory.CreateConnection();
            dbConnection.ConnectionString = connectionString;

            var select = factory.CreateCommand();
            select.Connection = dbConnection;
            select.CommandText = $"select Id from dbo.Users where Login = @login and Password = @password";

            var login = CreateParameter("@login", model.Login);
            var password = CreateParameter("@password", model.Password);

            select.Parameters.AddRange(new[] { login, password });

            dbConnection.Open();
            int id = -1;
            var reader = select.ExecuteReader();
            if (reader.HasRows)
            {
                reader.Read();
                id = reader.GetInt32(0);
            }
            /*var updateLastLogin = factory.CreateCommand();
             updateLastLogin.Connection = dbConnection;
             updateLastLogin.CommandText = $"update dbo.Users SET LastLogin = SYSDATETIME() where Id = @id";

             var pId = GetParameter("@id", id);

             updateLastLogin.Parameters.Add(pId);

             updateLastLogin.ExecuteNonQuery();*/
            reader.Close();
            dbConnection.Close();
            return id;
        }

        public int Register(LoginModel model)
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