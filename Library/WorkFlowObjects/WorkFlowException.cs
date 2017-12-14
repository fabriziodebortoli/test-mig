using System;
using System.Collections;
using System.Text;
using System.IO;

namespace Microarea.Library.WorkFlowObjects
{
	/// <summary>
	/// WorkFlowException.
	/// </summary>
	// ========================================================================
	public class WorkFlowException : ApplicationException 
	{
		//-----------------------------------------------------------------------
		public WorkFlowException()
		{
		}

		//-----------------------------------------------------------------------
		public WorkFlowException(string message) : base(message)
		{
		}

		//-----------------------------------------------------------------------
		public WorkFlowException(string message, Exception inner): base(message, inner)
		{
		}
		//-----------------------------------------------------------------------
		public string ExtendedMessage
		{
			get
			{
				if (InnerException == null || InnerException.Message == null || InnerException.Message == string.Empty)
					return Message;
				return Message + "\n(" + InnerException.Message + ")";
			}
		}
	}
}
