using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace DataService.Controllers
{
    [Route("ds")]
    public class HomeController : Controller
    {
        [Route("data-service")]
        public IActionResult GetData()
        {
            return new ContentResult { Content = "e mo ci vogliono i dati", ContentType = "application/json" };
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}
