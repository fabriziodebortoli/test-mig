using System;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.TaskBuilderNet.Core.DiagnosticUI
{
	/// <summary>
	/// Argomenti passati al visualizzatore per disegnare 
	/// il contenuto della listbox.
	/// </summary>
	//=========================================================================
	public class ParseMessageEventArgs : System.EventArgs
	{
		#region Private members
		private DateTime		itemTime		= DateTime.UtcNow;
		private string			itemMessageText	= string.Empty;
		private DiagnosticType	itemType		= DiagnosticType.None;
		private IExtendedInfo	itemExtendedInfo= null;
		#endregion

		#region Public properties
		public DateTime			Time		{ get { return itemTime; }			set { itemTime = value; } }
		public IExtendedInfo		ExtendedInfo{ get { return itemExtendedInfo; }	set { itemExtendedInfo = value; } }
		public string			MessageText	{ get { return itemMessageText; }	set { itemMessageText = value; } }
		public DiagnosticType	MessageType	{ get { return itemType; }			set { itemType = value; } }
		#endregion

		#region Constructors
		//---------------------------------------------------------------------
		public ParseMessageEventArgs() : base()
		{
		}

		//---------------------------------------------------------------------
		public ParseMessageEventArgs(DiagnosticType	type, DateTime time, string MessageText) : this()
		{
			itemTime		= time;
			itemMessageText = MessageText;
			itemType		= type;
		}
		
		//---------------------------------------------------------------------
		public ParseMessageEventArgs
		(
			DiagnosticType type,
			DateTime time,
			string messageText,
			IExtendedInfo extendedInfo
		) : this(type, time, messageText)
		{		
			itemExtendedInfo = extendedInfo;
		}
		#endregion
	}
}
