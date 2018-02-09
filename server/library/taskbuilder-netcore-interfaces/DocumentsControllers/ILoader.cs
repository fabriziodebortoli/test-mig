using System;
using TaskBuilderNetCore.Documents.Model.Interfaces;
using TaskBuilderNetCore.Interfaces;

namespace TaskBuilderNetCore.Documents.Controllers.Interfaces
{
    //====================================================================================    
    public interface ILoader
    {
        IDocumentServices DocumentServices { get; set; }
        IDocument GetDocument(ICallerContext callerContext);
        IComponent GetComponent(INameSpace nameSpace, ICallerContext callerContext, IDocument document = null);
    }
}
