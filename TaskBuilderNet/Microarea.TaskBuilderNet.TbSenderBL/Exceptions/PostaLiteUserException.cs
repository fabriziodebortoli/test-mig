using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;

namespace Microarea.TaskBuilderNet.TbSenderBL.Exceptions
{
	//=========================================================================
	public abstract class PostaLiteUserException : PostaLiteException
	{
		public PostaLiteUserException(string message, string company) 
			: base(message, company) { }
	}

	//=========================================================================
	public class UserNonActiveException : PostaLiteUserException
	{
		public UserNonActiveException(string company) : base(GetText(company), company) { }
		private static string GetText(string company)
		{
			return string.Format(CultureInfo.CurrentCulture,
				LocalizedStrings.UserNonActiveExceptionMask, company);
		}
	}

	//=========================================================================
	public class UserSuspendedException : PostaLiteUserException
	{
		public UserSuspendedException(string company) : base(GetText(company), company) { }
		private static string GetText(string company)
		{
			return string.Format(CultureInfo.CurrentCulture,
				LocalizedStrings.UserSuspendedExceptionMask, company);
		}
	}
}
