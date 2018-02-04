using InstaBotPrototype.Models;
using InstaBotPrototype.Services.DB;
using Microsoft.AspNetCore.Mvc;
using System.Configuration;
using System.Data.Common;

namespace InstaBotPrototype.Controllers
{
    [Route("api/[controller]")]
    public class ConfigurationController : Controller
    {

        static string connectionString = ConfigurationManager.ConnectionStrings[1].ConnectionString;
        IConfigService configService = new ConfigService(connectionString);
        DbProviderFactory factory = DbProviderFactories.GetFactory(ConfigurationManager.ConnectionStrings[1].ProviderName);

        // GET api/configuration
        [HttpGet]
        public ConfigurationModel Get()
        {
            if (IsLoggedIn())
            {
                return configService.GetConfig();
            }
            return null;
        }

        // GET api/configuration/5
        [HttpGet("{id}")]
        public ConfigurationModel Get(int id)
        {
            if (IsLoggedIn())
            {
                return configService.GetConfig(id);
            }
            return null;
        }

        // POST api/configuration
        [HttpPost]
        public IActionResult Post([FromForm]ConfigurationModel model)
        {
            if (IsLoggedIn())
            {
                configService.SaveConfig(model, Request.Cookies["sessionID"]);
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
        private bool IsLoggedIn() => configService.IsLoggedIn(Request.Cookies["sessionID"]);

    }
}