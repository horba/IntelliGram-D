using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace InstaBotPrototype.Controllers
{
    [Route("/")]
    public class IndexController : Controller
    {
        // GET api/values
        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }

    }
}
