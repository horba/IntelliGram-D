using InstaBotPrototype.Models;

namespace InstaBotPrototype.Services.DB
{
    interface IConfigService
    {
        void SaveConfig(ConfigurationModel config);
        ConfigurationModel GetConfig(int? id);
    }
}