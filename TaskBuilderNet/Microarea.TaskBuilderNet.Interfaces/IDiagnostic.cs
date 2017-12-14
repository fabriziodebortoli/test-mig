using System;

namespace Microarea.TaskBuilderNet.Interfaces
{
	//=========================================================================
	public interface IDiagnostic
	{
		IDiagnosticItem[] AllItems { get; }
		IDiagnosticItems AllMessages(DiagnosticType diagnosticType);
		IDiagnosticItems AllMessages();
		IDiagnosticSimpleItem[] AllSimpleItems { get; }
		bool AutoWriteLog { get; }
		void Clear();
		void Clear(DiagnosticType diagnosticType);
		IDiagnosticElements Elements { get; }
		bool Error { get; }
		bool Information { get; }
		string Installation { get; set; }
		bool LogInfos { get; }
		IDiagnosticItems Messages(DiagnosticType diagnosticType, string[] selection);
		IDiagnosticItems MyMessages(DiagnosticType diagnosticType);
		string OwnerName { get; }
		IDiagnosticItem Set(DiagnosticType diagnosticType, string explain, int logEventID, short logCategory);
		IDiagnosticItem Set(DiagnosticType diagnosticType, DateTime dateTime, string explain, int logEventID, short logCategory);
		IDiagnosticItem Set(DiagnosticType diagnosticType, string explain, string extendedMessage);
		IDiagnosticItem Set(DiagnosticType diagnosticType, DateTime dateTime, string explain, string extendedMessage);
		IDiagnosticItem Set(DiagnosticType diagnosticType, string explain, IExtendedInfo extendedInfo, int logEventID, short logCategory);
		IDiagnosticItem Set(DiagnosticType diagnosticType, DateTime dateTime, string explain, IExtendedInfo extendedInfo, int logEventID, short logCategory);
		IDiagnosticItem Set(DiagnosticType diagnosticType, DateTime dateTime, string explain, IExtendedInfo extendedInfo);
		IDiagnosticItem Set(DiagnosticType diagnosticType, string explain, IExtendedInfo extendedInfo);
		IDiagnosticItem Set(DiagnosticType diagnosticType, string explain);
		IDiagnosticItem Set(DiagnosticType diagnosticType, DateTime dateTime, System.Collections.Specialized.StringCollection explain, IExtendedInfo extendedInfo, int logEventID, short logCategory);
		IDiagnosticItem Set(DiagnosticType diagnosticType, System.Collections.Specialized.StringCollection explain, IExtendedInfo extendedInfo, int logEventID, short logCategory);
		void Set(DiagnosticType diagnosticType, IDiagnostic diagnostic);
		void Set(IDiagnostic diagnostic);
		IDiagnosticItem Set(DiagnosticType diagnosticType, System.Collections.Specialized.StringCollection explain, int logEventID, short logCategory);
		IDiagnosticItem Set(DiagnosticType diagnosticType, DateTime dateTime, System.Collections.Specialized.StringCollection explain);
		IDiagnosticItem Set(DiagnosticType diagnosticType, DateTime dateTime, string explain);
		IDiagnosticItem Set(DiagnosticType diagnosticType, DateTime dateTime, System.Collections.Specialized.StringCollection explain, int logEventID, short logCategory);
		IDiagnosticItem Set(DiagnosticType diagnosticType, System.Collections.Specialized.StringCollection explain);
		IDiagnosticItem SetError(string explain);
		IDiagnosticItem SetError(DateTime dateTime, string explain);
		IDiagnosticItem SetError(System.Collections.Specialized.StringCollection explain);
		IDiagnosticItem SetError(DateTime dateTime, System.Collections.Specialized.StringCollection explain);
		IDiagnosticItem SetInformation(DateTime dateTime, System.Collections.Specialized.StringCollection explain);
		IDiagnosticItem SetInformation(DateTime dateTime, string explain);
		IDiagnosticItem SetInformation(string explain);
		IDiagnosticItem SetInformation(System.Collections.Specialized.StringCollection explain);
		IDiagnosticItem SetLogInfo(System.Collections.Specialized.StringCollection explain);
		IDiagnosticItem SetLogInfo(DateTime dateTime, System.Collections.Specialized.StringCollection explain);
		IDiagnosticItem SetLogInfo(string explain);
		IDiagnosticItem SetLogInfo(DateTime dateTime, string explain);
		IDiagnosticItem SetWarning(System.Collections.Specialized.StringCollection explain);
		IDiagnosticItem SetWarning(string explain);
		IDiagnosticItem SetWarning(DateTime dateTime, string explain);
		IDiagnosticItem SetWarning(DateTime dateTime, System.Collections.Specialized.StringCollection explain);
		string ToString();
		int TotalErrors { get; }
		int TotalInformations { get; }
		int TotalLogInfos { get; }
		int TotalWarnings { get; }
		bool Warning { get; }
		bool WriteChildDiagnostic(IDiagnostic child, bool bClearChild);
		void WriteLogInfos();
		bool WriteLogInfos(string[] selection);

		event EventHandler AddedDiagnostic;

	}
}
