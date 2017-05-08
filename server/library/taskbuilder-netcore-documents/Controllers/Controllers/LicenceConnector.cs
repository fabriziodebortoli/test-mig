using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaskBuilderNetCore.Documents.Interfaces;
using TaskBuilderNetCore.Documents.Model;
using TaskBuilderNetCore.Interfaces;

namespace TaskBuilderNetCore.Documents.Controllers
{
    [Name("LicenceConnector"), Description("It manages communication with licence management.")]
    public class LicenceConnector : Controller, ILicenceConnector
    {
        public LicenceConnector()
        {
               
        }

        public bool IsActivated(INameSpace nameSpace)
        {
            return true;
        }
    }
}
