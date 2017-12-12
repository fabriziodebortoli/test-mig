
using Microarea.TaskBuilderNet.Core.DiagnosticManager;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.Console.Core.FileConverter
{
	public interface ILogWriter
	{
		void Message (string message, string details, DiagnosticType type, IExtendedInfo ei);
		void Clear();

		bool Error();
		bool Warning();
	}

	//================================================================================
	public class LogManager
	{	
		private ILogWriter logWriter = null;
		
		//---------------------------------------------------------------------------------------------------
		public ILogWriter LogWriter
		{
			get { return logWriter; }
			set { logWriter = value; }
		}
		
		//---------------------------------------------------------------------------------------------------
		public LogManager(ILogWriter logWriter)
		{
			this.logWriter = logWriter;
		}

		//---------------------------------------------------------------------------------------------------
		public static string GetTypeDescription(DiagnosticType type)
		{
			switch(type)
			{
				case DiagnosticType.Information: return FileConverterStrings.InformationTag;
				case DiagnosticType.Warning: return FileConverterStrings.WarningTag;
				case DiagnosticType.Error: return FileConverterStrings.ErrorTag;
			}
			return string.Empty;
		}
		
		//---------------------------------------------------------------------------------------------------
		public void Clear()
		{
			if (logWriter != null) logWriter.Clear();
		}

		//---------------------------------------------------------------------------------------------------
		public void Message (string message)
		{
			Message(message, string.Empty, DiagnosticType.Error, null);
		}
		
		//---------------------------------------------------------------------------------------------------
		public void Message (string message, DiagnosticType type)
		{
			Message(message, string.Empty, type, null);
		}

		//---------------------------------------------------------------------------------------------------
		public void Message (string message, string details)
		{
			Message(message, details, DiagnosticType.Error, null);
		}

		//---------------------------------------------------------------------------------------------------
		public void Message (string message, string details, DiagnosticType type, IExtendedInfo extinfo)
		{
			if (logWriter != null) LogWriter.Message(message, details, type, extinfo);
		}

		//---------------------------------------------------------------------------------------------------
		public void AddMessage(Diagnostic diagnostic)
		{		
			IDiagnosticItems dis = diagnostic.AllMessages();
			if (dis == null) return;

			for (int j = 0; j < dis.Count; j++)
			{
				IDiagnosticItem di = dis[j]; 
				Message(di.FullExplain, string.Empty, di.Type, di.ExtendedInfo);
			}			
		}
	}
}
