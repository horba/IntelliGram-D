﻿using InstaBotPrototype.Models;
using InstaBotPrototype.Services;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace InstaBotPrototype.Controllers
{
    [Route("api/[controller]/[action]")]
    public class AuthenticationController : Controller
    {
        private readonly IAuthenticationService _authenticationService;
        public AuthenticationController(IAuthenticationService authenticationService)
        {
            _authenticationService = authenticationService;
        }
        [HttpPost]
        public IActionResult Login(LoginModel loginModel)
        {
            var loginResult = _authenticationService.Login(loginModel);
            if (loginResult != null)
            {
                return new ObjectResult(new { sessionID = loginResult });
            }
            else
            {
                var result = new ObjectResult(new { errorMessage = "Wrong login or password" })
                {
                    StatusCode = (int)HttpStatusCode.NotFound
                };
                return result;
            }
        }

        [HttpPost]
        public IActionResult Register(LoginModel loginModel)
        {
            var registerResult = _authenticationService.Register(loginModel);
            if (registerResult != null)
            {
                return new ObjectResult(new { sessionID = registerResult });
            }
            else
            {
                var result = new ObjectResult(new { errorMessage = "User with such login already exists!" })
                {
                    StatusCode = (int)HttpStatusCode.NotFound
                };
                return result;
            }
        }
    }
}