using Microsoft.AspNetCore.Mvc;

namespace InstaBotPrototype.Controllers
{
    [Route("/")]
    public class IndexController : Controller
    {
        // GET api/values
        [HttpGet]
        public ActionResult Index() => View();
    }
}