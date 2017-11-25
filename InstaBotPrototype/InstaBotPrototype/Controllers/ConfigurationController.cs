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
        ConfigService configService = new ConfigService();

        // GET api/configuration
        [HttpGet]
        public ConfigurationModel Get()
        {
            return configService.GetConfig();
        }

        // GET api/configuration/5
        [HttpGet("{id}")]
        public ConfigurationModel Get(int id)
        {
            if (id > 0)
            {
                try
                {
                    return configService.GetConfig(id);
                }
                catch
                {

                    return null;
                }
                
            }
            else
            {
                return configService.GetConfig();
            }
        }

        // POST api/configuration
        [HttpPost]
        public IActionResult Post([FromForm]ConfigurationModel model)
        {
            try
            {
                configService.SaveConfig(model);
                return Ok(200);
            }
            catch
            {

                return null;
            }
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
