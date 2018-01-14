using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InstaBotPrototype.Models;

namespace InstaBotPrototype.Services.DB
{
    interface IConfigService
    {
        void SaveConfig(ConfigurationModel config);
        ConfigurationModel GetConfig(int? id);
    }
}
