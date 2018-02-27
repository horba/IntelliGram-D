using System.Collections.Generic;
using InstaBotPrototype.Services.Autocompletion;
using Microsoft.AspNetCore.Mvc;

namespace InstaBotPrototype.Controllers
{
    [Route("api/Autocomplete/[action]")]
    public class AutocompleteController : Controller
    {
        private readonly IAutocompletionService _autocompletionService;
        public AutocompleteController(IAutocompletionService autocompletionService)
        {
            _autocompletionService = autocompletionService;
        }
        [HttpGet]
        public IEnumerable<string> GetTags(string item)
        {
            return _autocompletionService.GetTagСompletion(item);
        }

        [HttpGet]
        public IEnumerable<string> GetTopics(string item)
        {
            return _autocompletionService.GetTopicCompletion(item);
        }
    }
}