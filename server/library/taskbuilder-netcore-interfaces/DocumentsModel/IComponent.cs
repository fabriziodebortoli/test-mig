using System;
using System.Collections.Generic;
using System.Text;

namespace TaskBuilderNetCore.Documents.Model.Interfaces
{
    //====================================================================================    
    public interface IComponent : IDisposable
    {
        IDocumentServices DocumentServices { get; }
        ICallerContext CallerContext { get; }
        bool CanBeLoaded(ICallerContext callerContext);
        bool Initialize(ICallerContext callerContext, IDocumentServices documentServices);
        void Clear();
    }
}
