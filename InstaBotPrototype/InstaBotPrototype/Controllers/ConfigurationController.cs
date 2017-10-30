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
        // GET api/configuration
        [HttpGet]
        public ConfigurationModel Get()
        {
            return new ConfigurationModel { InstaUsername = "JohnSmith", InstaPassword = "Passw0rd", TelegramUsername = "telegaN", Tags = "cats", Topics = "Nature, Lake" };
        }

        // GET api/configuration/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/configuration
        [HttpPost]
        public IActionResult Post([FromForm]ConfigurationModel model)
        {
            return Ok(model);
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
