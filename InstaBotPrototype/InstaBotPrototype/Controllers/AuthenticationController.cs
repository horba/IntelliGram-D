using InstaBotPrototype.Models;
using Microsoft.AspNetCore.Mvc;
using InstaBotPrototype.Services;
namespace InstaBotPrototype.Controllers
{
    [Route("api/[controller]/[action]")]
    public class AuthenticationController : Controller
    {
        private readonly IAuthenticationService _authenticationService;
        public AuthenticationController (IAuthenticationService service) {
            _authenticationService = service;
        }        
        [HttpGet]
        public IActionResult Login (LoginModel loginModel)
        {
            var loginResult = _authenticationService.Login(loginModel);
            if (loginResult != -1)
                return new ObjectResult(new {sessionID = loginResult});
            else
                return NotFound();
        }
        [HttpPost]
        public IActionResult Register(LoginModel loginModel)
        {
            var registerResult = _authenticationService.Register(loginModel);
            if (registerResult != -1)
                return new ObjectResult(registerResult);
            else
                return NotFound();
        }
    }
}