using System;
using InstaBotPrototype.Models;
namespace InstaBotPrototype.Services
{
    public interface IAuthenticationService
    {
        String Login(LoginModel model);
        String Register(LoginModel model);
    }
}
