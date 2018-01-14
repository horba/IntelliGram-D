using System;
using InstaBotPrototype.Models;
namespace InstaBotPrototype.Services
{
    public interface IAuthenticationService
    {
        int Login(LoginModel model);
        int Register(LoginModel model);
    }
}
