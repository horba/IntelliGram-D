using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InstaBotPrototype.Services.Autocompletion
{
    public interface IAutocompletionService
    {
        IEnumerable<string> GetTagСompletion(string tag);
        IEnumerable<string> GetTopicCompletion(string topic);
    }
}
