using System.Diagnostics;
using System;
using System.IO;

namespace Microarea.TaskBuilderNet.TbHermesBL
{
    public class MailLogListener : TextWriterTraceListener
	{
        public bool bEnabled { get; set; }

        public MailLogListener()
            :base()
        {
            bEnabled = false;
        }

        public MailLogListener(string fileName)
	        : base(fileName)
	    {
            bEnabled = true;
	    }

        public void Initialize(string fileName)
        {
            this.Writer = new StreamWriter(fileName, true);  // eccezioni gestite dal chiamante

            bEnabled = true;
        }
    
        public override void Write(string message, string category)
	    {
            if (!bEnabled) return;

	        if (category == "Mail.dll")
	            base.Write(message, category);

            if (category == "Hermes")
                base.Write(DateTime.Now.ToLongTimeString() + ": " + message, category);

            Flush();
        }
	 
	    public override void WriteLine(string message, string category)
	    {
            if (!bEnabled) return;
            
            if (category == "Mail.dll")
	            base.WriteLine(message, category);

            if ((category == "Hermes") ||
                (category == "TICK--"))
                base.WriteLine(DateTime.Now.ToLongTimeString() + ": " + message, category);

            Flush();
        }
	}

}
