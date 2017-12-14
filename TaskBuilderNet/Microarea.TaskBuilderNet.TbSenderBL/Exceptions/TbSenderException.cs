using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;

namespace Microarea.TaskBuilderNet.TbSenderBL.Exceptions
{
	//=========================================================================
	public class TbSenderException : ApplicationException
	{
		//public TbSenderException() : base() { } // per serializzazione
		public TbSenderException(string message, string company) : base(message)
		{
			this.Company = company;
		}
		public string Company { get; private set; }
	}

	//=========================================================================
	public class CompanyNotEnabledException : TbSenderException
	{
		public CompanyNotEnabledException(string company) : base(GetText(company), company) { }
		private static string GetText(string company)
		{
			return string.Format(CultureInfo.CurrentCulture, LocalizedStrings.CompanyNotEnabledExceptionMask, company);
		}
	}

	//=========================================================================
	public class NoSubscriptionDataException : TbSenderException
	{
		public NoSubscriptionDataException(string company) : base(GetText(company), company) { }
		private static string GetText(string company)
		{
			return string.Format(CultureInfo.CurrentCulture, LocalizedStrings.NoSubscriptionDataExceptionMask, company);
		}
	}

	//=========================================================================
	public class EnvelopeAlreadySentException : TbSenderException
	{
		public EnvelopeAlreadySentException(TB_MsgLots lot, string company) : base(GetText(lot, company), company) { }
		private static string GetText(TB_MsgLots lot, string company)
		{
			return string.Format(CultureInfo.CurrentCulture, LocalizedStrings.EnvelopeAlreadySentExceptionMask, lot.LotID, company);
		}
	}
}
