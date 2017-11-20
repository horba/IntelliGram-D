using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InstaBotPrototype.Models;
using Microsoft.AspNetCore.Mvc;

namespace InstaBotPrototype.Controllers
{
    [Route("api/[controller]")]
    public class ConfigurationController : Controller
    {
        ConfigDataProvider configProvider;
        // GET api/configuration
        [HttpGet]
        public ConfigurationModel Get()
        {
            configProvider = new ConfigDataProvider();
            return configProvider.GetConfig();
        }

        // GET api/configuration/5
        [HttpGet("{id}")]
        public ConfigurationModel Get(int id)
        {
            configProvider = new ConfigDataProvider();
            return configProvider.GetConfig(id);
        }

        // POST api/configuration
        [HttpPost]
        public IActionResult Post([FromForm]ConfigurationModel model)
        {
            configProvider = new ConfigDataProvider();
            configProvider.UpdateConfig(model);
            return Ok();
        }

        // PUT api/configuration/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/configuration/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
