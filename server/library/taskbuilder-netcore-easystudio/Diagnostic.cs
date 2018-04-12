using System;
using TaskBuilderNetCore.EasyStudio.Interfaces;
using TaskBuilderNetCore.Interfaces;
using Microarea.Common.DiagnosticManager;
using System.Diagnostics;
using Newtonsoft.Json;

namespace TaskBuilderNetCore.EasyStudio
{
    //=========================================================================
    public class DiagnosticProvider : IDiagnosticProvider
    {
        Diagnostic diagnostic;

        //---------------------------------------------------------------
        public string AsJson
        {
            get
            {
                IDiagnosticItems items = diagnostic.AllMessages();
                return JsonConvert.SerializeObject(items);
            }
        }

        public bool HasErrors { get => diagnostic.Error; }
        public bool HasWarnings { get => diagnostic.Warning; }
        public bool IsEmpty { get => diagnostic.AllItems.Length == 0; }

        //---------------------------------------------------------------
        public DiagnosticProvider(string name)
        {
            diagnostic = new Diagnostic(name);
        }

        //---------------------------------------------------------------
        public void Add(DiagnosticType type, string message)
        {
            diagnostic.Set(type, message);
        }

        //---------------------------------------------------------------
        public void Add(Exception ex)
        {
            diagnostic.Set(DiagnosticType.Error, ex.Message);
        }

        //---------------------------------------------------------------
        public void NotifyMessage(Exception ex)
        {
            Debug.WriteLine(ex.Message);
            Add(DiagnosticType.FatalError, ex.Message);
        }

        //---------------------------------------------------------------
        public void Clear()
        {
            diagnostic.Clear();
        }
    }
}
