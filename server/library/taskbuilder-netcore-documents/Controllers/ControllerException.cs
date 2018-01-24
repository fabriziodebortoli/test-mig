using System;
using TaskBuilderNetCore.Documents.Controllers.Interfaces;
using TaskBuilderNetCore.Documents.Diagnostic;

namespace TaskBuilderNetCore.Documents.Controllers
{
    //====================================================================================    
    public class ControllerException : CodedException
    {
        private IController controller;

        //-----------------------------------------------------------------------------------------------------
        public override string FullMessage { get => string.Format("controller: {0} message: {1}", controller.Name, base.FullMessage); }

        //-----------------------------------------------------------------------------------------------------
        public ControllerException(IController controller, Message messageCode, object parameter, Exception innerException = null)
            : base(messageCode, parameter, innerException)
        {
            this.controller = controller;
        }
        
        //-----------------------------------------------------------------------------------------------------
        public ControllerException(IController controller, Message messageCode, object[] parameters, Exception innerException = null)
            : base(messageCode, parameters, innerException)
        {
            this.controller = controller;
        }
    }
}
