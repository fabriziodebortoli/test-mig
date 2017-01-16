using System;
using System.Text;
using TaskBuilderNetCore.Interfaces;

namespace TaskBuilderNetCore.Interfaces
{
    //=================================================================================
    public class TbLoaderClientInterfaceException : Exception
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

               

                return Message + "\r\n(" + InnerException.Message + ")";
            }
        }
    }

    //================================================================================
    [Serializable]
    public class TbServicesException : Exception, IDetailedException
    {
        IDiagnosticSimpleItem[] items;

        //--------------------------------------------------------------------------------
        public TbServicesException(string message, IDiagnosticSimpleItem[] items)
            : this(message, items, null)
        {
        }

        //--------------------------------------------------------------------------------
        public TbServicesException(string message, IDiagnosticSimpleItem[] items, Exception innerException)
            : base(message, innerException)
        {
            this.items = items;
        }

        //--------------------------------------------------------------------------------
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(Message);
            foreach (IDiagnosticSimpleItem item in items)
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
                foreach (IDiagnosticSimpleItem item in items)
                    sb.AppendLine(item.ToString());

                return sb.ToString();
            }
        }
    }

}
