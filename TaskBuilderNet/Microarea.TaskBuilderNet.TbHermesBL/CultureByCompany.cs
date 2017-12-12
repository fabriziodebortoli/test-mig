using System;
using System.Globalization;
using System.Threading;
using Microarea.TaskBuilderNet.Core.Generic;

namespace Microarea.TaskBuilderNet.TbHermesBL
{
	// TODO Analoga all'omonima in TbSenderBL, ma senza dipendenza da static.
	// TODO sposta in Microarea.TaskBuilderNet.Core, e specializzando (TbSender e TbHermes) con parametro funcGetCompanyInfo
	// Serve qualora la logica di business dovesse creare dei messaggi localizzati nella lingua impostata per la company

	/// <summary>
	/// Grazie al Dispose pattern permette di impostare una culture, 
	/// e ripristinarla in automatico alla fine, con una sola riga di codice
	/// </summary>
	public class CultureByCompany : IDisposable
	{
		readonly CultureInfo previousCulture;
		readonly Func<string, TbSenderDatabaseInfo> funcGetCompanyInfo;

		//---------------------------------------------------------------------
		public CultureByCompany(string company, Func<string, TbSenderDatabaseInfo> funcGetCompanyInfo)
		{
			this.previousCulture = CultureInfo.CurrentUICulture;
			this.funcGetCompanyInfo = funcGetCompanyInfo;
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
		private CultureInfo GetCompanyUICulture(string company)
		{
			TbSenderDatabaseInfo dbInfo = this.funcGetCompanyInfo(company);
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
