
namespace Microarea.TaskBuilderNet.Core.DiagnosticUI
{
	//=========================================================================
	public class Constants
	{
		public const string XmlExtension			= ".xml";
		public const string XmlDeclarationEncoding	= "UTF-8";
		public const string XmlDeclarationVersion	= "1.0";
	}

	//=========================================================================
	public class MessagesXML
	{
		//---------------------------------------------------------------------
		public class Element
		{
			public const string File			= "File";
			public const string Messages		= "Messages";
			public const string Message			= "Message";
			public const string MessageText     = "MessageText";
			public const string ExtendedInfos	= "ExtendedInfos";
			public const string ExtendedInfo	= "ExtendedInfo";
		}

		//---------------------------------------------------------------------
		public class Attribute
		{
			public const string CreationDate	= "creationdate";
			public const string LogType			= "logtype";
			public const string Type			= "type";
			public const string Time			= "time";
			public const string Text			= "text";
			public const string Name			= "name";
			public const string Value			= "value";
		}
	}
}
