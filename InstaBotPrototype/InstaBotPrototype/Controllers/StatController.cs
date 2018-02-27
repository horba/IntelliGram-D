using System.Collections.Generic;
using InstaBotPrototype.Services;
using InstaBotPrototype.Services.Autocompletion;
using InstaBotPrototype.Services.DB;
using Microsoft.AspNetCore.Mvc;

namespace InstaBotPrototype.Controllers
{
    [Route("api/Statistic/[action]")]
    public class StatController : Controller
    {
        private readonly IConfigService configService;
        private readonly IStatService statService;
        public StatController(IConfigService _configService,IStatService _statService)
        {
            configService = _configService;
            statService = _statService;
        }
        public int Photos() {
            var userId = configService.GetUserIdBySession(Request.Cookies["sessionID"]);
            return statService.CountPhotos(userId.Value);
        }
        public int Tags()
        {
            var userId = configService.GetUserIdBySession(Request.Cookies["sessionID"]);
            return statService.CountTags(userId.Value);
        }
        public int Topics()
        {
            var userId = configService.GetUserIdBySession(Request.Cookies["sessionID"]);
            return statService.CountTopics(userId.Value);
        }
    }
}
