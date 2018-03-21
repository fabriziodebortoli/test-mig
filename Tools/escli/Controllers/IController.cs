using System;
using System.Collections.Generic;
using System.Text;
using TaskBuilderNetCore.EasyStudio.Interfaces;

namespace escli.Controllers
{
    //=========================================================
    internal interface IController
    {
        bool ExecuteRequest(string action, string[] arParams);
        IDiagnosticProvider Diagnostic { get;  }
    }
}
