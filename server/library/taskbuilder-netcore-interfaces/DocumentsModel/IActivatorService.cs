using System;
using System.Collections.Generic;
using System.Text;
using TaskBuilderNetCore.Documents.Model.Interfaces;

namespace TaskBuilderNetCore.Documents.Model.Interfaces
{
    //====================================================================================    
    public interface IActivatorService
    {
        IDocument GetDocument(ICallerContext callerContext);
        void CloseDocument(ICallerContext callerContext);
    }
}
