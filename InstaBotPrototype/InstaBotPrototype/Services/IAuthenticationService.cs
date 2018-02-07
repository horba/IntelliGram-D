using InstaBotPrototype.Models;

namespace InstaBotPrototype.Services
{
    public interface IAuthenticationService
    {
        string Login(LoginModel model);
        string Register(LoginModel model);
    }
}