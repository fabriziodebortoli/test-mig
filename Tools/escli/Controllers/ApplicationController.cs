using System;
using TaskBuilderNetCore.EasyStudio.Interfaces;
using TaskBuilderNetCore.EasyStudio.Services;
using TaskBuilderNetCore.Interfaces;

namespace escli.Controllers
{
    [Route("app")]
    //====================================================================
    internal class ApplicationController : IController
    {
        ApplicationService service;

        //-----------------------------------------------------
        public ApplicationController(IServiceManager serviceManager)
        {
            this.service = serviceManager.GetService<ApplicationService>();
        }

        //-----------------------------------------------------
        public IDiagnosticProvider Diagnostic => service.Diagnostic;

        //-----------------------------------------------------
        public bool ExecuteRequest(string action, string[] arParams)
        {
            switch (action.ToLower())
            {
                case "cre":
                case "create":
                    ApplicationType applicationType = ApplicationType.Customization;
                    return service.Create(arParams[0], arParams[1], applicationType, arParams[3]);
                case "del":
                case "delete":
                    return service.Delete(arParams[0], arParams[1]);
                default:
                    Console.WriteLine("Action not supported by Application Service");
                    return false;
            }
        }
    } 
}

