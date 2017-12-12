using System;
using System.Collections.Specialized;
using System.Diagnostics;

namespace Microarea.TaskBuilderNet.Interfaces
{
	//=========================================================================
	public interface IDiagnosticItem
	{
		StringCollection Explain { get; }
		IExtendedInfo ExtendedInfo { get; }
		string FullExplain { get; }
		string Installation { get; set; }
		bool IsError { get; }
		bool IsInformation { get; }
		bool IsLogInfo { get; }
		bool IsWarning { get; }
		DateTime Occurred { get; set; }
		bool ShowExtendedInfos { get; set; }
		string ToString(LineSeparator separator);
		DiagnosticType Type { get; }
		bool WriteEventLogEntry(EventLog aEventLog);
	}
}
