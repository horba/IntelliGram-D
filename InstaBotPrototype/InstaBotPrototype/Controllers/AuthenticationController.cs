using InstaBotPrototype.Models;
using Microsoft.AspNetCore.Mvc;
using InstaBotPrototype.Services;
namespace InstaBotPrototype.Controllers
{
    [Route("/[controller]/[action]")]
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
            if (loginResult != -1) {
                var cookieOptions = new Microsoft.AspNetCore.Http.CookieOptions
                {
                    Expires = System.DateTime.Now.AddDays(1)
                };
                Response.Cookies.Append("UserIdentifier",loginResult.ToString(),cookieOptions);
                return Content("You have signed in successfully!");
               
            }
            return Content("Wrong login or password!");
        }
        [HttpPost]
        public IActionResult Register(LoginModel loginModel)
        {
            var registerResult = _authenticationService.Register(loginModel);
            if (registerResult != -1)
                return Content("You have signed up successfully! Now you can sign in using your login and password.");
            else
                return Content("Oooops! Something have gone wrong. Please, sign up again!");
        }
    }
}