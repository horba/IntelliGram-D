using Microsoft.AspNetCore.Mvc;
using InstaBotPrototype.Services.Instagram;
using InstaBotPrototype.Services.DB;
namespace InstaBotPrototype.Controllers
{
    public class InstagramController : Controller
    {
        private readonly IInstagramService _instagramService;
        private readonly IConfigService _configService;
        public InstagramController(IInstagramService instagramService, IConfigService configService)
        {
            _instagramService = instagramService;
            _configService = configService;
        }
        [Route("api/Instagram")]
        public ActionResult Index()
        {
            if (!_configService.IsUserVerifiedInInstagram(Request.Cookies["sessionID"]))
                return View("Verify");
            else
                return View("Redirect");
        }
        [Route("api/Instagram/Verify")]
        public ActionResult Verify()
        {
            return Redirect("https://api.instagram.com/oauth/authorize?client_id=937fa7572cb244e9885382f8cedba3c8&redirect_uri=http://localhost:58687/api/Instagram&scope=basic+public_content+comments+likes+follower_list&response_type=token");
        }
        [Route("api/Instagram")]
        [HttpPost]
        public void Post(string token)
        {
            string pattern = "#access_token=";
            string tokenStr = token.Substring(token.IndexOf(pattern) + pattern.Length);
            var id = long.Parse(tokenStr.Split('.')[0]);
            string nickname = _instagramService.GetUsername(tokenStr);
            _configService.SaveInstagramToken(id, nickname, tokenStr, Request.Cookies["sessionID"]);

        }
    }
}
