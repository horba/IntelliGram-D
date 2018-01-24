using InstaBotPrototype.Models;

namespace InstaBotPrototype.Services
{
    public interface IAuthenticationService
    {
        int? GetVerifyKey(LoginModel model);
        string Login(LoginModel model);
        string Register(LoginModel model);
    }
}