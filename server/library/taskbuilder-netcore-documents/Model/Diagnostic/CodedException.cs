using System;
using System.Collections.Generic;
using System.Text;

namespace TaskBuilderNetCore.Documents.Diagnostic
{
    //====================================================================================    
    public class CodedException : ApplicationException
    {
        private Message messageCode;
        //-----------------------------------------------------------------------------------------------------
        public virtual string FullMessage { get => string.Format("source: {0} message: {1}", Source, MessageCode.CompleteMessage); }
        //-----------------------------------------------------------------------------------------------------
        public Message MessageCode { get => messageCode; set => messageCode = value; }

        //-----------------------------------------------------------------------------------------------------
        public CodedException(Message messageCode, object parameter, Exception innerException = null)
            : base(messageCode.Text, innerException)
        {
            this.messageCode = messageCode;
            this.messageCode.Text = string.Format(this.messageCode.Text, parameter);

        }
        //-----------------------------------------------------------------------------------------------------
        public CodedException(Message messageCode, object[] parameters, Exception innerException = null)
            : base(messageCode.Text, innerException)
        {
            this.messageCode = messageCode;
            this.messageCode.Text = string.Format(this.messageCode.Text, parameters);
        }
    }
}
