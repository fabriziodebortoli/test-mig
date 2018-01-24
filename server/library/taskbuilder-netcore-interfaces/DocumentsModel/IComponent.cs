using System;
using System.Collections.Generic;
using System.Text;

namespace TaskBuilderNetCore.Documents.Model.Interfaces
{
    //====================================================================================    
    public interface IComponent : IDisposable
    {
        IActivatorService ActivatorService { get; set; }
        ICallerContext CallerContext { get; }
        bool CanBeLoaded(ICallerContext callerContext);
        bool Initialize(ICallerContext callerContext);
        void Clear();
    }
}
