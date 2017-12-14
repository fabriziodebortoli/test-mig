using System;

namespace Microarea.TaskBuilderNet.UI.EasyLookCustomization
{
	//=================================================================================
	/// <summary>
	/// Summary description for EasyLookCustomSettingsException.
	/// </summary>
	public class EasyLookCustomSettingsException : ApplicationException 
	{
		public EasyLookCustomSettingsException()
		{
		}
		public EasyLookCustomSettingsException(string message) : base(message)
		{
		}
		public EasyLookCustomSettingsException(string message, Exception inner): base(message, inner)
		{
		}
		//-----------------------------------------------------------------------
		public string ExtendedMessage
		{
			get
			{
				if (InnerException == null || InnerException.Message == null || InnerException.Message == String.Empty)
					return Message;
				return Message + "\n(" + InnerException.Message + ")";
			}
		}
	}
}
