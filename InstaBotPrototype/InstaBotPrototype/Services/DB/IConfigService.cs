using InstaBotPrototype.Models;

namespace InstaBotPrototype.Services.DB
{
    interface IConfigService
    {
        void SaveConfig(ConfigurationModel config, string sessionId);
        ConfigurationModel GetConfig();
        ConfigurationModel GetConfig(int? id);
        bool IsLoggedIn(string sessionID);
        int? GetUserIdBySession(string sessionId);

    }
}