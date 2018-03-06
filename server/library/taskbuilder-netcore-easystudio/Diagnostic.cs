using System;
using TaskBuilderNetCore.EasyStudio.Interfaces;
using TaskBuilderNetCore.Interfaces;
using Microarea.Common.DiagnosticManager;
using System.Diagnostics;
using Newtonsoft.Json;

namespace TaskBuilderNetCore.EasyStudio
{
    public class DiagnosticProvider : IDiagnosticProvider
    {
        Diagnostic diagnostic;

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
            Debug.Fail(ex.Message);
            Add(DiagnosticType.FatalError, ex.Message);
        }

        //---------------------------------------------------------------
        public string ToJson()
        {
            IDiagnosticItems items = diagnostic.AllMessages();
            return JsonConvert.SerializeObject(items);
        }
    }
}
