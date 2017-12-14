using System;

namespace Microarea.TaskBuilderNet.Interfaces
{
	//=========================================================================
	public interface IDiagnosticElements
	{
		IDiagnosticItems AllMessages(DiagnosticType diagnosticType);
		IDiagnosticItems AllMessages();
		bool AreItemsOfTypePresent(DiagnosticType diagnosticType);
		void Clear();
		void Clear(DiagnosticType diagnosticType);
		int Count { get; }
		int CountOfType(DiagnosticType diagnosticType);
		bool Error { get; }
		int ErrorsCount { get; }
		bool Information { get; }
		int InformationsCount { get; }
		void InitDiagnosticItemsList();
		bool LogInfos { get; }
		int LogInfosCount { get; }
		IDiagnosticItems Messages(DiagnosticType diagnosticType, string[] selection);
		IDiagnosticItems MyMessages();
		IDiagnosticItems MyMessages(DiagnosticType diagnosticType);
		void Set(DiagnosticType diagnosticType, IDiagnosticElements diagnosticElement);
		void Set(IDiagnosticElements diagnosticElement);
		IDiagnosticItem Set(DiagnosticType diagnosticType, DateTime dateTime, System.Collections.Specialized.StringCollection explains);
		IDiagnosticItem Set(DiagnosticType diagnosticType, System.Collections.Specialized.StringCollection explains, int logEventID, short logCategory);
		IDiagnosticItem Set(DiagnosticType diagnosticType, System.Collections.Specialized.StringCollection explains);
		IDiagnosticItem Set(DiagnosticType diagnosticType, DateTime dateTime, System.Collections.Specialized.StringCollection explains, int logEventID, short logCategory);
		IDiagnosticItem Set(DiagnosticType diagnosticType, DateTime dateTime, System.Collections.Specialized.StringCollection explains, IExtendedInfo extendedInfo, int logEventID, short logCategory);
		IDiagnosticItem Set(DiagnosticType diagnosticType, System.Collections.Specialized.StringCollection explains, IExtendedInfo extendedInfo, int logEventID, short logCategory);
		bool Warning { get; }
		int WarningsCount { get; }
	}
}
