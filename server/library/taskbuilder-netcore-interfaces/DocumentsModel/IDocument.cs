using System;
using System.Collections.Generic;
using TaskBuilderNetCore.Interfaces;
using TaskBuilderNetCore.Documents.Controllers.Interfaces;
using System.ComponentModel;
using System.Collections.ObjectModel;

namespace TaskBuilderNetCore.Documents.Model.Interfaces
{
    //====================================================================================    
    public interface IDocument
    {
        #region events declarations

        event CancelEventHandler LoadingComponents;
        event EventHandler ComponentsLoaded;
        event CancelEventHandler AttachingDataModel;
        event EventHandler DataModelAttached;
        event CancelEventHandler DetachingDataModel;
        event EventHandler DataModelDetached;

        #endregion
        IDocumentServices DocumentServices { get; set; }
        INameSpace NameSpace { get; }
        ICallerContext CallerContext { get; }
        string Title { get; set; }
        ObservableCollection<IDocumentComponent> Components { get; }
        IDiagnostic Diagnostic { get; }
        // opening and closing operations
        void Clear();
        bool Initialize(ICallerContext callerContext, IDocumentServices documentServices);
        bool LoadComponents();
        bool AttachDataModel();
        bool DetachDataModel();
     }
}
