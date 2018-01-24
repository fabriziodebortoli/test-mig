using System.Collections.Generic;
using TaskBuilderNetCore.Interfaces;

namespace TaskBuilderNetCore.Documents.Model.Interfaces
{
    public enum ExecutionMode { Interactive, Unattended, Scheduled, Design };

    //====================================================================================    
    public interface ICallerContext
    {
        INameSpace NameSpace { get; }
        string AuthToken { get; }
        string Company { get; }
        string Identity { get; }
        IDiagnostic Diagnostic { get; set;  }
        ExecutionMode Mode { get; set; }
        List<object> Parameters { get; set; }
    }
}
