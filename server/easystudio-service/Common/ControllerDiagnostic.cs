using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Microarea.EasyStudio.Common
{
    public enum MsgType { Error, Success, Info };

    //=========================================================================
    public class ControllerDiagnosticMessage
    {
        //-------------------------------------------------------------------
        MsgType type;
        string message;

        //-------------------------------------------------------------------
        public MsgType Type { get => type; }
        public string Message { get => message; }

        //-------------------------------------------------------------------
        public ControllerDiagnosticMessage(MsgType type, string message)
        {
            this.type = type;
            this.message = message;
        }
    }

    //=========================================================================
    public class ControllerDiagnostic
    {
        //-------------------------------------------------------------------
        public static string ToJson(MsgType type, string text)
        {
            ControllerDiagnosticMessage message = new ControllerDiagnosticMessage(type, text);
            return  JsonConvert.SerializeObject(message);
        }

        //=========================================================================
        internal class Strings
        {
            internal static readonly string MissingApplicationName      = "Missing parameter applicationName";
            internal static readonly string MissingApplicationType      = "Missing parameter applicationType";
            internal static readonly string MissingModuleName           = "Missing parameter moduleName";
            internal static readonly string ObjectSuccessfullyCreated   = "Object Successfully Created";
            internal static readonly string ErrorCreatingObject         = "Error Creating Object";
            internal static readonly string ObjectSuccessfullyDeleted   = "Object Successfully Deleted";
            internal static readonly string ErrorDeletingObject         = "Error Deleting Object";
        }
    }
}
