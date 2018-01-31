using InstaBotPrototype.Models;

namespace InstaBotPrototype.Services.DB
{
    interface IConfigService
    {
        void SaveConfig(ConfigurationModel config, String sessionId);
        ConfigurationModel GetConfig();
        ConfigurationModel GetConfig(int? id);
        bool IsLoggedIn(String sessionID);
    }
}