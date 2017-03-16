using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace manufacturing_service.Controllers
{
    [Route("manufacturing-service")]
    public class ValuesController : Controller
    {
        // GET api/values
        [Route("prova")]
        public IEnumerable<string> Prova()
        {
            return new string[] { "value1", "value2" };
        }
    }
}
