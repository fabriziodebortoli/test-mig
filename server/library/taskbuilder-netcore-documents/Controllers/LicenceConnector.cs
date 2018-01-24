using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaskBuilderNetCore.Documents.Controllers.Interfaces;
using TaskBuilderNetCore.Documents.Model;
using TaskBuilderNetCore.Interfaces;

namespace TaskBuilderNetCore.Documents.Controllers
{
    [Name("LicenceConnector"), Description("It manages communication with licence and activation management.")]
    //====================================================================================    
    public class LicenceConnector : Controller, ILicenceConnector
    {
        //-----------------------------------------------------------------------------------------------------
        public LicenceConnector()
        {
               
        }

        //-----------------------------------------------------------------------------------------------------
        public bool IsActivated(INameSpace nameSpace)
        {
            return IsActivated(nameSpace.Application, nameSpace.Module);
        }

        //-----------------------------------------------------------------------------------------------------
        public bool IsActivated(string activation)
        {
            return true;
        }

        //-----------------------------------------------------------------------------------------------------
        public bool IsActivated(string application, string moduleOrFunctionality)
        {
            return true;
        }
    }
}
