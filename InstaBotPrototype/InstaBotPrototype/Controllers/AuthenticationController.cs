using InstaBotPrototype.Models;
using Microsoft.AspNetCore.Mvc;
using InstaBotPrototype.Services;
using System.Net;

namespace InstaBotPrototype.Controllers
{
    [Route("api/[controller]/[action]")]
    public class AuthenticationController : Controller
    {
        private readonly IAuthenticationService _authenticationService;
        public AuthenticationController (IAuthenticationService service) {
            _authenticationService = service;
        }        
        [HttpPost]
        public IActionResult Login (LoginModel loginModel)
        {
            var loginResult = _authenticationService.Login(loginModel);
            if (loginResult != -1)
                return new ObjectResult(new { sessionID = loginResult, errorMessage = "Everything works"});
            else {
                ObjectResult result = new ObjectResult(new { errorMessage = "Wrong login or password" })
                {
                    StatusCode = (int )HttpStatusCode.NotFound
                };
                return result;
            }       
        }
        [HttpPost]
        public IActionResult Register(LoginModel loginModel)
        {
            var registerResult = _authenticationService.Register(loginModel);
            if (registerResult != -1)
                return new ObjectResult(registerResult);
            else
                return NotFound(new { errorMessage = "Wrong login or password" });
        }
    }
}