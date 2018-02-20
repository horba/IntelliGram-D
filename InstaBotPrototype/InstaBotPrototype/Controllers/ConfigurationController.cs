using System.Collections.Generic;
using System.Configuration;
using System.Data.Common;
using InstaBotPrototype.Models;
using InstaBotPrototype.Services.DB;
using Microsoft.AspNetCore.Mvc;

namespace InstaBotPrototype.Controllers
{
    [Route("api/[controller]")]
    [Route("api/[controller]/[action]")]
    public class ConfigurationController : Controller
    {
        IConfigService configService = new ConfigService();


        [HttpPost]
        public IActionResult Post([FromForm]ConfigurationModel model)
        {
            if (IsLoggedIn())
            {
                //configService.AddConfig(Request.Cookies["sessionID"]);
            }
            return Ok(model);
        }
        [HttpGet]
        public IActionResult VerifyKey()
        {
            return new ObjectResult(new { verifyKey = configService.GetVerifyKey(Request.Cookies["sessionID"]) });
        }

        public IEnumerable<string> GetTags()
        {
            var userId = configService.GetUserIdBySession(Request.Cookies["sessionID"]);
            if (userId.HasValue)
            {
                return configService.GetUserTags(userId.Value);
            }
            else
            {
                return null;
            }
        }
        public IEnumerable<string> GetTopics()
        {
            var userId = configService.GetUserIdBySession(Request.Cookies["sessionID"]);
            if (userId.HasValue) {
                return configService.GetUserTopics(userId.Value);
            }
            else {
                return null;
            }
        }
        [HttpPost]
        public void AddTag(string item)
        {
            var userId = configService.GetUserIdBySession(Request.Cookies["sessionID"]); 
            if (userId.HasValue)
            {
                var configId = configService.GetLatestConfig(userId.Value);
                configService.AddTag(item, configId.Value);
            }
        }
        [HttpPost]
        public void AddTopic(string item)
        {
            var userId = configService.GetUserIdBySession(Request.Cookies["sessionID"]);

            if (userId.HasValue)
            {
                var configId = configService.GetLatestConfig(userId.Value);
                configService.AddTopic(item, configId.Value);
            }
        }
        
        [HttpDelete]
        public void DeleteTopic(string item)
        {
            var userId = configService.GetUserIdBySession(Request.Cookies["sessionID"]);

            if (userId.HasValue)
            {
                var configId = configService.GetLatestConfig(userId.Value);
                configService.DeleteTopic(item, configId.Value);
            }
        }
        [HttpDelete]
        public void DeleteTag(string item)
        {
            var userId = configService.GetUserIdBySession(Request.Cookies["sessionID"]);

            if (userId.HasValue)
            {
                var configId = configService.GetLatestConfig(userId.Value);
                configService.DeleteTag(item, configId.Value);
            }
        }
        private bool IsLoggedIn()
        {
            return configService.IsLoggedIn(Request.Cookies["sessionID"]);
        }

    }
}
