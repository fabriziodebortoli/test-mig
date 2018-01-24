using System;
using TaskBuilderNetCore.Documents.Model.Interfaces;
using TaskBuilderNetCore.Interfaces;

namespace TaskBuilderNetCore.Documents.Controllers.Interfaces
{
    //====================================================================================    
    public interface ILoader
    {
        IDocument GetDocument(ICallerContext callerContext, ILicenceConnector licenceConnector);
        IComponent GetComponent(INameSpace nameSpace, ICallerContext callerContext, ILicenceConnector licenceConnector, IDocument document = null);
    }
}
