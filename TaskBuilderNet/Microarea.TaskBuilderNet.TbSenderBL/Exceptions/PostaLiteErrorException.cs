using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microarea.TaskBuilderNet.TbSenderBL.PostaLite;
using System.Globalization;

namespace Microarea.TaskBuilderNet.TbSenderBL.Exceptions
{
	/// <summary>
	/// Eccezioni da lanciare in seguito a un errore ricevuto dal WS remoto di PostaLite
	/// </summary>
	public class PostaLiteErrorException : PostaLiteException
	{
		public PostaLiteErrorException(int error, string company) 
			: base(GetErrorText(error, company), company) { }

		static private string GetErrorText(int error, string company)
		{
			string msg = string.Format(CultureInfo.CurrentCulture,
				LocalizedStrings.PostaLiteErrorExceptionMask, ErrorMessages.GetErrorMessage(error));
			if (false == string.IsNullOrEmpty(company))
				msg += " - " + string.Format(CultureInfo.CurrentCulture,
					LocalizedStrings.CompanyDetailMask, company);
			return msg;
		}
	}
}
