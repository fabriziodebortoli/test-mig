using System;

namespace Microarea.TaskBuilderNet.Interfaces
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
					return DiagnosticSimpleItemsStrings.Error;
				if ((Type & DiagnosticType.Warning) == DiagnosticType.Warning)
					return DiagnosticSimpleItemsStrings.Warning;
				if ((Type & DiagnosticType.Information) == DiagnosticType.Information)
					return DiagnosticSimpleItemsStrings.Info;

				return "";
			}
		}
		public override string ToString()
		{
			return string.Format("{0}:   {1}", TypeDesc, Message);
		}
	}
}
