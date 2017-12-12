using System;

namespace Microarea.Library.TBWizardProjects
{
	#region TBWizardException class
	
	//=================================================================================
	/// <summary>
	/// Summary description for TBWizardException.
	/// </summary>
	public class TBWizardException : ApplicationException 
	{
		public TBWizardException(string message, Exception inner): base(message, inner)
		{
		}
		public TBWizardException(string message) : this(message, null)
		{
		}
		public TBWizardException() : this(String.Empty, null)
		{
		}

		//-----------------------------------------------------------------------
		public string ExtendedMessage
		{
			get
			{
				if (InnerException == null || InnerException.Message == null || InnerException.Message.Length == 0)
					return Message;
				return Message + "\n(" + InnerException.Message + ")";
			}
		}
	}
	
	#endregion
}
