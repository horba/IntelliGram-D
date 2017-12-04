using InstaBotPrototype.Models;
using Microsoft.AspNetCore.Mvc;

namespace InstaBotPrototype.Controllers
{
    [Route("api/[controller]")]
    public class AuthenticationController : Controller
    {
        [HttpGet]
        IActionResult Login([FromBody]LoginModel loginModel)
        {
            return Ok();
        }

        [HttpPost]
        IActionResult Register([FromBody]LoginModel loginModel)
        {
            return Ok();
        }
    }
}