using System;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using System.Xml;
using Microarea.Tools.TBLocalizer.CommonUtilities;
using System.Diagnostics;
using System.Xml.XPath;
using System.Xml.Xsl;

namespace Microarea.Tools.TBLocalizer
{
	/// <summary>
	/// Datadocument specifico per la scrittura di file di log.
	/// </summary>
	//=========================================================================
	public class Logger : DataDocument, ILogger
	{
	

		private ProgressBar progressBar;
		private StatusBar statusBar;
		
		/// <summary>TextBox per la scrittura dell'output</summary>
		private	RichTextBox	txtOutput;
		
		/// <summary>Specifica se il file di log contiene solo messaggi di info e non di errore</summary>
		private	bool		hasOnlyInfo = true;
	

		//--------------------------------------------------------------------------------
		internal bool	HasOnlyInfo			{ get {return hasOnlyInfo;}	}
		
	
		//--------------------------------------------------------------------------------
		public string XslFile 
		{
			get
			{
				string path = Path.GetDirectoryName(FileName);
				return Path.Combine(path, AllStrings.XslFileName);
			}
		}

		//-----------------------------------------------------------------
		public Logger(ProgressBar pBar, StatusBar sBar, RichTextBox txtOutput)
		{
			this.progressBar = pBar;
			this.statusBar = sBar;
			this.txtOutput = txtOutput;

			if (txtOutput != null)
				txtOutput.Clear();
			
			FileName = String.Format(AllStrings.LOG, CommonFunctions.GetTimeStamp());

            CreateXslFile();
            InitDocument(AllStrings.messages, null, null, null, XslFile);						
		}
			
		//--------------------------------------------------------------------------------
		public void PerformStep()
		{
			if (progressBar != null)
				progressBar.PerformStep();
		}

		//--------------------------------------------------------------------------------
		public void SetRange(int maximum)
		{
			if (progressBar != null)
			{
				progressBar.Minimum = 0;
				progressBar.Value = 0;
				progressBar.Maximum = maximum;
				progressBar.Step = 1;
			}
		}

		//--------------------------------------------------------------------------------
		private void CreateXslFile()
		{			
			string path = XslFile;

			if (File.Exists(path)) return;

			if (!Directory.Exists(AllStrings.LOGPATH))
				Directory.CreateDirectory(AllStrings.LOGPATH);

			Stream s = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("Microarea.Tools.TBLocalizer.XSLT." + AllStrings.XslFileName);
			byte[] buffer = new byte[s.Length];
			s.Read(buffer, 0, (int) s.Length);
			FileStream fs = new FileStream(path, FileMode.CreateNew);
			fs.Write(buffer, 0, buffer.Length);
			fs.Close();
		}

		//-----------------------------------------------------------------
		public void WriteLog(string message)
		{
			WriteLog(message, TypeOfMessage.info);
		}

		//-----------------------------------------------------------------
		public delegate void WriteLogFunction(string message, TypeOfMessage typeOfMessage);
		public void WriteLogProcedure(string message, TypeOfMessage typeOfMessage)
		{
			resource = currentDocument.CreateElement(AllStrings.message);

			XmlAttribute type	= currentDocument.CreateAttribute(AllStrings.type);
			type.Value			= typeOfMessage.ToString();
			resource.Attributes.SetNamedItem(type);

			XmlAttribute text	= currentDocument.CreateAttribute(AllStrings.text);
			text.Value			= message;
			resource.Attributes.SetNamedItem(text);

			XmlAttribute time	= currentDocument.CreateAttribute(AllStrings.datetime);
			time.Value			= DateTime.Now.ToString();
			resource.Attributes.SetNamedItem(time);

			rootNode.AppendChild(resource);

			modified = true; 
			//se non  sono messaggi di errore ok, altrimenti poi lo segnalerò
			if (typeOfMessage != TypeOfMessage.info && 
				typeOfMessage != TypeOfMessage.state && 
				hasOnlyInfo)
				hasOnlyInfo = false;

			if (statusBar != null)
                statusBar.Text = message;
			
			if (txtOutput != null)
			{
				txtOutput.AppendText(message);
				txtOutput.AppendText(Environment.NewLine);
			}
			else
			{
				Console.Out.WriteLine(message);
			}
		}
		//-----------------------------------------------------------------
		public void WriteLog(string message, TypeOfMessage typeOfMessage)
		{
            if (txtOutput != null)
                txtOutput.BeginInvoke((ThreadStart)delegate { WriteLogProcedure(message, typeOfMessage); });
            else
                WriteLogProcedure(message, typeOfMessage);
		}

        //-----------------------------------------------------------------
        public void StatusBarLog(string message)
        {
            if (statusBar != null)
                statusBar.Text = message;
        }

		//---------------------------------------------------------------------
		public static void WriteLog (Logger logger, string message, TypeOfMessage type)
		{
			if(logger != null)
				logger.WriteLog(message, type);
		}

		/// <summary>
		/// Salva con nome il file di log e visulizza il link nell'output.
		/// </summary>
		//-----------------------------------------------------------------
		internal void SaveLog()
		{
			string logfile = String.Format(AllStrings.fileLink, Path.GetFileName(FileName));
			string message = String.Format(Strings.ReadLog, logfile);
			WriteLog(message, TypeOfMessage.info);
			string errorSaving = SaveXML(FileName, false);
			if (errorSaving != String.Empty && txtOutput != null) 
				txtOutput.AppendText(errorSaving);
		}
	}


    //-----------------------------------------------------------------
    //garbage collector dei file html che ostrano il log
	class LogViewer
    {
        private string log = "";
      
        //---------------------------------------------------------------------
        ~LogViewer()
        {
            SafeDelete();
        }
        //---------------------------------------------------------------------
        public void LogViewerExited(object sender, EventArgs args)
        {
            SafeDelete();
        }

        //---------------------------------------------------------------------
        private void SafeDelete()
        {
            try
            {
                if (File.Exists(log))
                    File.Delete(log);
            }
            catch { }
        }

        //---------------------------------------------------------------------
        public void ShowHtml(string xmlPath)
        {
            XPathDocument myXPathDoc = new XPathDocument(xmlPath);
            XslCompiledTransform myXslTrans = new XslCompiledTransform();
            Stream s = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("Microarea.Tools.TBLocalizer.XSLT." + AllStrings.XslFileName);
            myXslTrans.Load(new XmlTextReader(s));
            log = Path.ChangeExtension(xmlPath, AllStrings.htmExtension);
            using (XmlTextWriter myWriter = new XmlTextWriter(log, null))
                myXslTrans.Transform(myXPathDoc, null, myWriter);

            Process p = System.Diagnostics.Process.Start(log);
            p.Exited += new EventHandler(LogViewerExited);
        }
    }
}
