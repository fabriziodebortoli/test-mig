using System;
using System.Text;
using System.Web.Services.Protocols;

namespace Microarea.TaskBuilderNet.Interfaces
{
    //=================================================================================
    public class TbLoaderClientInterfaceException : ApplicationException
    {
        private bool loginFailed;

        //-----------------------------------------------------------------------
        public bool LoginFailed
        {
            get { return loginFailed; }
            set { loginFailed = value; }
        }

        //-----------------------------------------------------------------------
        public TbLoaderClientInterfaceException() { }
        public TbLoaderClientInterfaceException(string message) : base(message) { }
        public TbLoaderClientInterfaceException(string message, Exception inner) : base(message, inner) { }

        //-----------------------------------------------------------------------
        public string ExtendedMessage
        {
            get
            {
                if (InnerException == null || InnerException.Message == null || InnerException.Message == String.Empty)
                    return Message;

                if (InnerException is System.Web.Services.Protocols.SoapException)
                    return Message + "\r\n(" + ((SoapException)InnerException).Detail.InnerText + ")";

                return Message + "\r\n(" + InnerException.Message + ")";
            }
        }
    }

    //================================================================================
    [Serializable]
    public class TbServicesException : ApplicationException, IDetailedException
    {
        DiagnosticSimpleItem[] items;

        //--------------------------------------------------------------------------------
        public TbServicesException(string message, DiagnosticSimpleItem[] items)
            : this(message, items, null)
        {
        }

        //--------------------------------------------------------------------------------
        public TbServicesException(string message, DiagnosticSimpleItem[] items, Exception innerException)
            : base(message, innerException)
        {
            this.items = items;
        }

        //--------------------------------------------------------------------------------
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(Message);
            foreach (DiagnosticSimpleItem item in items)
                sb.AppendLine(item.ToString());

            sb.AppendLine(base.ToString());
            return sb.ToString();
        }

         //--------------------------------------------------------------------------------
        public string Details
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                foreach (DiagnosticSimpleItem item in items)
                    sb.AppendLine(item.ToString());

                return sb.ToString();
            }
        }
    }

}
