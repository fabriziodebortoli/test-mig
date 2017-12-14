using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microarea.TaskBuilderNet.TbSenderBL.Exceptions
{
	/// <summary>
	/// Eccezioni da lanciare unicamente nei casi di mancanza di credito PostaLite sufficiente
	/// a completare un'operazione richiesta o prevista
	/// </summary>
	public class CreditException : TbSenderException
	{
		public CreditException(string message, string company) 
			: base(GetText(message, company), company) { }

		private static string GetText(string message, string company)
		{
			if (string.IsNullOrEmpty(company))
				return message;
			return string.Format(System.Globalization.CultureInfo.CurrentCulture,
				LocalizedStrings.CreditExceptionMask, message, company);
		}
	}
}
