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
        
        static string connectionString = ConfigurationManager.ConnectionStrings[1].ConnectionString;
        ConfigService config = new ConfigService(connectionString);
        DbProviderFactory factory = DbProviderFactories.GetFactory(ConfigurationManager.ConnectionStrings[1].ProviderName);

        // GET api/configuration
        [HttpGet]
        public ConfigurationModel Get()
        {
            if (IsLoggedIn())
            {
                return config.GetDefaultConfig();
            }

            return null;
        }

        // GET api/configuration/5
        [HttpGet("{id}")]
        public ConfigurationModel Get(int id)
        {
            if (IsLoggedIn())
            {
                try
                {
                    return config.GetConfig(id);
                }
                catch (Exception e)
                {
                    throw e;
                }
            }
            return null;
        }

        // POST api/configuration
        [HttpPost]
        public IActionResult Post([FromForm]ConfigurationModel model)
        {
            if (IsLoggedIn())
            {
                try
                {
                    config.SaveConfig(model);
                }
                catch(Exception e)
                {
                    throw e;
                }
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