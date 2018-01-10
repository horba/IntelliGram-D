using System;
using System.Configuration;
using System.Data.Common;
using InstaBotPrototype.Models;
using Microsoft.AspNetCore.Mvc;

namespace InstaBotPrototype.Controllers
{
    [Route("api/[controller]")]
    public class ConfigurationController : Controller
    {
        string connectionString = ConfigurationManager.ConnectionStrings[1].ConnectionString;
        DbProviderFactory factory = DbProviderFactories.GetFactory(ConfigurationManager.ConnectionStrings[1].ProviderName);

        // GET api/configuration
        [HttpGet]
        public ConfigurationModel Get()
        {
            if (IsLoggedIn())
            {
                // code here
                return new ConfigurationModel { InstaUsername = "JohnSmith", InstaPassword = "Passw0rd", TelegramUsername = "telegaN", Tags = "cats", Topics = "Nature, Lake" };
            }

            return null;
        }

        // GET api/configuration/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            if (IsLoggedIn())
            {
                // code here

                return "value";
            }

            return null;
        }

        // POST api/configuration
        [HttpPost]
        public IActionResult Post([FromForm]ConfigurationModel model)
        {

            if (IsLoggedIn())
            {
                // code here
            }

            return Ok(model);
        }

        // PUT api/configuration/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
            if (IsLoggedIn())
            {
                // code here
            }
        }

        // DELETE api/configuration/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
            if (IsLoggedIn())
            {
                // code here
            }
        }

        bool IsLoggedIn()
        {
            DbConnection conn = null;
            try
            {
                var id = Request.Cookies["Id"];
                if (id != null)
                {
                    var cookieId = Convert.ToInt32(Request.Cookies["Id"]);

                    conn = factory.CreateConnection();
                    conn.ConnectionString = connectionString;

                    var param = factory.CreateParameter();
                    param.ParameterName = "@Id";
                    param.Value = id;

                    var check = factory.CreateCommand();
                    check.CommandText = "select count(Id) from dbo.Sessions where Id = @Id";
                    check.Parameters.Add(param);
                    check.Connection = conn;

                    conn.Open();

                    if (Convert.ToInt32(check.ExecuteScalar()) == 1)
                    {
                        return true;
                    }
                }
            }
            catch
            {
            }
            finally
            {
                conn?.Close();
            }
            return false;
        }
    }
}