using System;
using System.Collections.Generic;
using System.Text;
using TaskBuilderNetCore.Interfaces;

namespace TaskBuilderNetCore.EasyStudio.Interfaces
{
    public interface IDiagnosticProvider
    {
        void NotifyMessage(Exception exc);
        void Add (DiagnosticType type, string message);
        void Add(Exception ex);
        bool HasErrors { get; }
        bool HasWarnings { get; }
        bool IsEmpty { get; }
        string AsJson { get; }
        void Clear();
    }
}
