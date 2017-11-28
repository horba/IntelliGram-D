using Microsoft.AspNetCore.Mvc;

namespace InstaBotPrototype.Controllers
{
    [Route("api/[controller]")]
    public class AuthenticationController : Controller
    {
        [HttpGet]
        int Login([FromBody]string login, [FromBody]string password)
        {
            return 0;
        }

        [HttpPost]
        int Register([FromBody]string login, [FromBody]string password)
        {
            return 0;
        }
    }
}