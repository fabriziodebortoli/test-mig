using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Threading;
using Microarea.TaskBuilderNet.Core.Generic;

namespace Microarea.TaskBuilderNet.TbSenderBL
{
	/// <summary>
	/// Grazie al Dispose pattern permette di impostare una culture, 
	/// e ripristinarla in automatico alla fine, con una sola riga di codice
	/// </summary>
	public class CultureByCompany : IDisposable
	{
		readonly CultureInfo previousCulture;

		//---------------------------------------------------------------------
		public CultureByCompany(string company)
		{
			this.previousCulture = CultureInfo.CurrentUICulture;
			Thread.CurrentThread.CurrentUICulture = GetCompanyUICulture(company);
		}
		public CultureByCompany(CultureInfo desiredCulture)
		{
			this.previousCulture = CultureInfo.CurrentUICulture;
			Thread.CurrentThread.CurrentUICulture = desiredCulture;
		}

		//---------------------------------------------------------------------
		public void Dispose()
		{
			Thread.CurrentThread.CurrentUICulture = previousCulture;
		}

		//---------------------------------------------------------------------
		static private CultureInfo GetCompanyUICulture(string company)
		{
			TbSenderDatabaseInfo dbInfo = LoginManagerConnector.GetCompaniesInfo(company);
			if (dbInfo == null)
				return CultureInfo.CurrentUICulture;
			try
			{
				if (false == string.IsNullOrEmpty(dbInfo.CompanyCultureUI))
					return CultureInfo.GetCultureInfo(dbInfo.CompanyCultureUI);
				if (false == string.IsNullOrEmpty(dbInfo.CompanyCulture))
					return CultureInfo.GetCultureInfo(dbInfo.CompanyCulture);
				return CultureInfo.CurrentUICulture;
			}
			catch (CultureNotFoundException)
			{
				return CultureInfo.CurrentUICulture;
			}
		}
	}
}
