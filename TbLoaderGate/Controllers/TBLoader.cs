using System;
using Microsoft.AspNetCore.Mvc;

namespace ClassLibrary
{
    public class TBLoaderController : Controller
    {
        public IActionResult Index()
        {
            return new ObjectResult("TBLoader Gate");
        }

    }
}

