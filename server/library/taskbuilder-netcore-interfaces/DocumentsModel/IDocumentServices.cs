using System;
using System.Collections.Generic;
using System.Text;
using TaskBuilderNetCore.Documents.Model.Interfaces;
using TaskBuilderNetCore.Interfaces;

namespace TaskBuilderNetCore.Documents.Model.Interfaces
{
    //====================================================================================    
    public interface IDocumentServices
    {
        IDocument GetDocument(ICallerContext callerContext);
        void CloseDocument(ICallerContext callerContext);
        bool ExecuteActivity(ICallerContext callerContext);
        bool IsActivated(INameSpace nameSpace);
        bool IsActivated(string activation);
    }
}
