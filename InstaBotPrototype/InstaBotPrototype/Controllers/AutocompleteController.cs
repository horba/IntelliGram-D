using System.Collections.Generic;
using InstaBotPrototype.Services.Autocompletion;
using Microsoft.AspNetCore.Mvc;

namespace InstaBotPrototype.Controllers
{
    [Route("api/Autocomplete/[action]")]
    public class AutocompleteController : Controller
    {
        private IAutocompletionService autocompletionService = new AutocompletionService();

        [HttpGet]
        public IEnumerable<string> GetTags(string item)
        {
            return autocompletionService.GetTagСompletion(item);
        }

        [HttpGet]
        public IEnumerable<string> GetTopics(string item)
        {
            return autocompletionService.GetTopicCompletion(item);
        }
    }
}