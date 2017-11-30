using Microsoft.AspNetCore.Mvc;

namespace InstaBotPrototype.Controllers
{
    [Route("api/[controller]")]
    public class AuthenticationController : Controller
    {
        [HttpGet]
        IActionResult Login([FromBody]string login, [FromBody]string password)
        {
            return Ok();
        }

        [HttpPost]
        IActionResult Register([FromBody]string login, [FromBody]string password)
        {
            return Ok();
        }
    }
}