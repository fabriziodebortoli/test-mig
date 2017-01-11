using System;

using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.RSWeb.DiagnosticManager
{
	//=========================================================================
	/// <summary>
	/// usata per il passaggio 'snello' di elementi di diagnostica nelle chiamate a web services
	/// </summary>
	[Serializable]
	public struct DiagnosticSimpleItem : IDiagnosticSimpleItem
	{
		public DiagnosticType Type { get; set; }
		public string Message { get; set; }
		public string TypeDesc
		{
			get
			{
				if ((Type & DiagnosticType.Error) == DiagnosticType.Error)
					return DiagnosticManagerStrings.Error;
				if ((Type & DiagnosticType.Warning) == DiagnosticType.Warning)
					return DiagnosticManagerStrings.Warning;
				if ((Type & DiagnosticType.Information) == DiagnosticType.Information)
					return DiagnosticManagerStrings.Info;

				return "";
			}
		}
		public override string ToString()
		{
			return string.Format("{0}:   {1}", TypeDesc, Message);
		}
	}
}
