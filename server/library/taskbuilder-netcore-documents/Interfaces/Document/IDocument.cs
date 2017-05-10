using System;
using System.Collections.Generic;
using TaskBuilderNetCore.Interfaces;

namespace TaskBuilderNetCore.Documents.Interfaces
{
    //====================================================================================    
    public interface IDocument
    {
        INameSpace NameSpace { get; }
        List<IExtension> Extensions { get; }
        IOrchestrator Orchestrator { get; }
        IValidator Validator { get; }
        IDiagnostic Diagnostic { get; }
        ICallerContext CallerContext { get; }

        // opening and closing operations
        void Clear();
        bool Initialize(IOrchestrator orchestrator, ICallerContext callerContext);
        bool AttachDataModel();
        bool DetachDataModel();

        // data operations
        bool ValidateData();
        void ClearData();
        bool LoadData();
        bool SaveData();
        bool DeleteData();
    }
}
