using System;
using InstaBotPrototype.Models;
namespace InstaBotPrototype.Services
{
    public interface IAuthenticationService
    {
        int? GetVerifyKey(LoginModel model);
        String Login(LoginModel model);
        String Register(LoginModel model);
    }
}
