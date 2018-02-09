using InstaBotPrototype.Models;

namespace InstaBotPrototype.Services.DB
{
    public interface IConfigService
    {
        void SaveConfig(ConfigurationModel config, string sessionId);
        ConfigurationModel GetConfig();
        ConfigurationModel GetConfig(int? id);
        bool IsLoggedIn(string sessionID);
        void SaveInstagramToken(long id, string nickname, string tokenStr, string sessionId);
        bool IsUserVerifiedInInstagram(string sessionId);
        int? GetVerifyKey(string sessionId);
    }
}