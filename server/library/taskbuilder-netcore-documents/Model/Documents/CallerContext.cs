using Microarea.Common.DiagnosticManager;
using Microarea.Common.Generic;
using Newtonsoft.Json;
using System;
using TaskBuilderNetCore.Documents.Model.Interfaces;
using TaskBuilderNetCore.Interfaces;
using System.Collections.Generic;

namespace TaskBuilderNetCore.Documents.Model
{
    /// <summary>
    /// It contains context from caller user including identity, parameters or aux objects
    /// </summary>
    //====================================================================================    
    public class CallerContext : ICallerContext
    {
        INameSpace nameSpace;
        string authToken;
        string company;
        IDiagnostic diagnostic;
        ExecutionMode mode;
        List<object> parameters;

        //-----------------------------------------------------------------------------------------------------
        public string ObjectName { get => nameSpace?.FullNameSpace; set => nameSpace = new NameSpace(value); }
        //-----------------------------------------------------------------------------------------------------
        [JsonIgnore]
        public INameSpace NameSpace  { get => nameSpace;  set => nameSpace = value; }

        //-----------------------------------------------------------------------------------------------------
        public string AuthToken { get => authToken;  set => authToken = value; }

        //-----------------------------------------------------------------------------------------------------
        public string Company { get => company; set => company = value; }

        //-----------------------------------------------------------------------------------------------------
        [JsonIgnore]
        public string Identity
        {
            get => string.Concat(nameSpace.FullNameSpace, " ", authToken, " ", company);
        }

        //-----------------------------------------------------------------------------------------------------
        public IDiagnostic Diagnostic
        {
            get
            {
                if (diagnostic == null)
                    diagnostic = new Microarea.Common.DiagnosticManager.Diagnostic(ObjectName);

                return diagnostic;         
            }
            set => diagnostic = value;
        }
        //-----------------------------------------------------------------------------------------------------
        public ExecutionMode Mode { get => mode; set => mode = value; }
        //-----------------------------------------------------------------------------------------------------
        public List<object> Parameters
        {
            get
            {
                if (parameters == null)
                    parameters = new List<object>();

                return parameters;
            }
            set => parameters = value;
        }
    }
}