using InstaBotPrototype.Models;
using InstaBotPrototype.Services.DB;
using InstaBotPrototype.Services.Instagram;
using Microsoft.AspNetCore.Mvc;
using System.Configuration;
using System.Data.Common;

namespace InstaBotPrototype.Controllers
{
    [Route("api/Instagram")]
    public class InstagramController : Controller
    {
        string connectionString = ConfigurationManager.ConnectionStrings[1].ConnectionString;
        DbProviderFactory factory = DbProviderFactories.GetFactory(ConfigurationManager.ConnectionStrings[1].ProviderName);
        public ViewResult Index() => View();
        [HttpPost]
        public void Post(string token)
        {

            using (var dbConnection = factory.CreateConnection())
            {
                dbConnection.ConnectionString = connectionString;
                dbConnection.Open();

                var insertCmd = factory.CreateCommand();
                insertCmd.Connection = dbConnection;

                var tokenStr = token.Substring(token.IndexOf("#access_token") + 14);
                var id = long.Parse(tokenStr.Split('.')[0]);
                var insta = new InstagramService();
                var nickname = insta.GetUsername(tokenStr);

                insertCmd.CommandText = "INSERT INTO InstagramIntegration VALUES (@UserId,@InstaId,@Nick,@Token)";
                var userIdParam = factory.CreateParameter();
                userIdParam.ParameterName = "@InstaId";
                userIdParam.Value = id;

                var nickParam = factory.CreateParameter();
                nickParam.ParameterName = "@Nick";
                nickParam.Value = nickname;

                var tokenParam = factory.CreateParameter();
                tokenParam.ParameterName = "@Token";
                tokenParam.Value = tokenStr;
                IConfigService service = new ConfigService();
                var userid = service.GetUserIdBySession(Request.Cookies["sessionID"]);

                var idParam = factory.CreateParameter();
                idParam.ParameterName = "@UserId";
                idParam.Value = userid.Value;

                insertCmd.Parameters.AddRange(new[] { idParam, userIdParam, nickParam, tokenParam });
                insertCmd.ExecuteNonQuery();
            }

        }
    }
}