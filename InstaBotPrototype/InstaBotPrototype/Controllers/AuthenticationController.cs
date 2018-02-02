using InstaBotPrototype.Models;
using InstaBotPrototype.Services;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace InstaBotPrototype.Controllers
{
    [Route("api/[controller]/[action]")]
    public class AuthenticationController : Controller
    {
        private readonly IAuthenticationService _authenticationService;
        public AuthenticationController(IAuthenticationService service) => _authenticationService = service;

        [HttpPost]
        public IActionResult Login(LoginModel loginModel)
        {
            var loginResult = _authenticationService.Login(loginModel);
            return loginResult != null
                ? new ObjectResult(new { sessionID = loginResult, verifyKey = _authenticationService.GetVerifyKey(loginModel) })
                : new ObjectResult(new { errorMessage = "Wrong login or password" })
                {
                    StatusCode = (int)HttpStatusCode.NotFound
                };
        }

        [HttpPost]
        public IActionResult Register(LoginModel loginModel)
        {
            var registerResult = _authenticationService.Register(loginModel);
            return registerResult != null
                ? new ObjectResult(new { sessionID = registerResult, verifyKey = _authenticationService.GetVerifyKey(loginModel) })
                : new ObjectResult(new { errorMessage = "Something wrong has happened during registrartion" })
                {
                    StatusCode = (int)HttpStatusCode.Unauthorized
                };
        }
    }
}