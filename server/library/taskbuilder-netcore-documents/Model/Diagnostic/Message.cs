using System;
using System.Collections.Generic;
using System.Text;

namespace TaskBuilderNetCore.Documents.Diagnostic
{  //====================================================================================    
    public class Message
    {
        int code;
        string text;

        //-----------------------------------------------------------------------------------------------------
        public int Code { get => code; set => code = value; }
        //-----------------------------------------------------------------------------------------------------
        public string Text { get => text; set => text = value; }
        public string CompleteMessage { get => string.Format("code: {0} message: {1}", Code, Text); }
        //-----------------------------------------------------------------------------------------------------
        public Message(int code, string text)
        {
            this.code = code;
            this.text = text;
        }
    }
}
