using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaskBuilderNetCore.Documents.Model;
using TaskBuilderNetCore.Documents.Interfaces;

namespace TaskBuilderNetCore.Documents.Controllers
{
    [Name("WebConnector"), Description("It manages communication with user interface.")]
    public class WebConnector : Controller, IWebConnector
    {
        public WebConnector()
        {

        }

        public void PushToClient()
        {

        }
    }
}
