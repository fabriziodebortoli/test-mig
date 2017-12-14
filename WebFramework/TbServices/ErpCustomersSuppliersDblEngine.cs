using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading;
using Microarea.TaskBuilderNet.Core.DiagnosticManager;
using Microarea.TaskBuilderNet.Interfaces;
using System.Diagnostics;
using Microarea.TaskBuilderNet.Core.WebServicesWrapper;
using Microarea.TaskBuilderNet.Core.NameSolver;

namespace Microarea.WebServices.TbServices
{
	//=========================================================================
	public class ErpCustomersSuppliersDblEngine
	{
		private const int timeout = 15000;
		internal Diagnostic diagnostic = new Diagnostic("ErpCustomersSuppliersDblEngine");  	//Gestione errori
		string locker = "No locking method";
			
		//---------------------------------------------------------------------------
		internal bool TryLockResources()
		{
			try
			{
				if (!Monitor.TryEnter(this, timeout))
				{
					diagnostic.Set
						(DiagnosticType.LogInfo | DiagnosticType.Warning,
						string.Format("Failed to lock ErpItemsServices after {0} milliseconds. Method: {1}", timeout, locker));
					return false;
				}

			}
			catch (Exception exc)
			{
				diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Warning, "TryLockResources: " + exc.ToString());
				throw exc;
			} try
			{

				locker = (new StackTrace()).GetFrame(1).GetMethod().Name;
			}
			catch { locker = "Unknown method"; }
			return true;
		}

		//---------------------------------------------------------------------------
		internal void ReleaseResources()
		{
			try
			{
				Monitor.Exit(this);
				locker = "No locking method";
			}
			catch (Exception exc)
			{
				diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Warning, "ReleaseResources: " + exc.ToString());
				throw exc;
			}
		}

		//---------------------------------------------------------------------------
		private TbLoaderClientInterface InitializeTbLoader(string authenticationToken)
		{
			string user = string.Empty;

			int tbPort = TbServicesApplication.TbServicesEngine.CreateTB(authenticationToken, DateTime.Now, true, out user);
			if (tbPort < 0)
				throw new Exception(string.Format(Strings.TbLoaderIstancingError, tbPort.ToString()));

			return new TbLoaderClientInterface(BasePathFinder.BasePathFinderInstance, tbPort, authenticationToken, TbLoaderInfo.TbWCFBinding);

		}

		//---------------------------------------------------------------------------
		internal bool ExistCustForTaxIdOrFiscalCode(string authenticationToken, string taxIdNumber, string fiscalCode)
		{
			using (TbLoaderClientInterface tb = InitializeTbLoader(authenticationToken))
				return tb.ExistCustForTaxIdOrFiscalCode(taxIdNumber, fiscalCode);
		}

		//---------------------------------------------------------------------------
		internal bool CheckForTaxIdNoWithCountryCode(string authenticationToken, string countryCode, string taxIdNumber)
		{
			using (TbLoaderClientInterface tb = InitializeTbLoader(authenticationToken))
				return tb.CheckForTaxIdNoWithCountryCode(countryCode, taxIdNumber);
		}

		//---------------------------------------------------------------------------
		internal bool FiscalCodeCheck(string authenticationToken, string taxIdNumber)
		{
			using (TbLoaderClientInterface tb = InitializeTbLoader(authenticationToken))
				return tb.FiscalCodeCheck(taxIdNumber);
		}

		//---------------------------------------------------------------------------
		internal bool TaxIdNoCheck(string authenticationToken, string taxIdNumber)
		{
			using (TbLoaderClientInterface tb = InitializeTbLoader(authenticationToken))
				return tb.TaxIdNoCheck(taxIdNumber);
		}

		//---------------------------------------------------------------------------
		internal bool TaxIdNoSecondCheck(string authenticationToken, string taxIdNumber)
		{
			using (TbLoaderClientInterface tb = InitializeTbLoader(authenticationToken))
				return tb.TaxIdNoSecondCheck(taxIdNumber);
		}
	}

	/// <summary>
	/// Callse che contiene l'istanza statica dell'applicazione
	/// </summary>
	//=========================================================================
	public class ErpCustomersSuppliersDblApplication
	{
		public static ErpCustomersSuppliersDblEngine ErpCustomersSuppliersDblEngine = new ErpCustomersSuppliersDblEngine();
	}
}