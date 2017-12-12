using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using Microarea.TaskBuilderNet.Core.WebServicesWrapper;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Interfaces;
using System.Reflection;

namespace Microarea.WebServices.TbServices
{
	//==================================================================================
	[WebService(Namespace = "http://microarea.it/TbServices/")]
	[System.ComponentModel.ToolboxItem(false)]

	public class ErpCustomersSuppliersDbl : System.Web.Services.WebService
	{
		[WebMethod]
		//---------------------------------------------------------------------------
		public bool ExistCustForTaxIdOrFiscalCode(string authenticationToken, string taxIdNumber, string fiscalCode)
		{
			if (!ErpCustomersSuppliersDblApplication.ErpCustomersSuppliersDblEngine.TryLockResources())
				throw new Exception(Strings.ResourcesTimeout);

			try
			{
				return ErpCustomersSuppliersDblApplication.ErpCustomersSuppliersDblEngine.ExistCustForTaxIdOrFiscalCode(authenticationToken, taxIdNumber, fiscalCode);
			}
			catch (Exception exc)
			{
				ErpCustomersSuppliersDblApplication.ErpCustomersSuppliersDblEngine.diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Warning, MethodInfo.GetCurrentMethod().Name + ": " + exc.ToString());
				throw;
			}
			finally
			{
				ErpCustomersSuppliersDblApplication.ErpCustomersSuppliersDblEngine.ReleaseResources();
			}
		}

		[WebMethod]
		//---------------------------------------------------------------------------
		public bool CheckForTaxIdNoWithCountryCode(string authenticationToken, string countryCode, string taxIdNumber)
		{
			if (!ErpCustomersSuppliersDblApplication.ErpCustomersSuppliersDblEngine.TryLockResources())
				throw new Exception(Strings.ResourcesTimeout);

			try
			{
				return ErpCustomersSuppliersDblApplication.ErpCustomersSuppliersDblEngine.CheckForTaxIdNoWithCountryCode(authenticationToken, countryCode, taxIdNumber);
			}
			catch (Exception exc)
			{
				ErpCustomersSuppliersDblApplication.ErpCustomersSuppliersDblEngine.diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Warning, MethodInfo.GetCurrentMethod().Name + ": " + exc.ToString());
				throw;
			}
			finally
			{
				ErpCustomersSuppliersDblApplication.ErpCustomersSuppliersDblEngine.ReleaseResources();
			}
		}

		[WebMethod]
		//---------------------------------------------------------------------------
		public bool FiscalCodeCheck(string authenticationToken, string taxIdNumber)
		{
			if (!ErpCustomersSuppliersDblApplication.ErpCustomersSuppliersDblEngine.TryLockResources())
				throw new Exception(Strings.ResourcesTimeout);

			try
			{
				return ErpCustomersSuppliersDblApplication.ErpCustomersSuppliersDblEngine.FiscalCodeCheck(authenticationToken, taxIdNumber);
			}
			catch (Exception exc)
			{
				ErpCustomersSuppliersDblApplication.ErpCustomersSuppliersDblEngine.diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Warning, MethodInfo.GetCurrentMethod().Name + ": " + exc.ToString());
				throw;
			}
			finally
			{
				ErpCustomersSuppliersDblApplication.ErpCustomersSuppliersDblEngine.ReleaseResources();
			}
		}

		[WebMethod]
		//---------------------------------------------------------------------------
		public bool TaxIdNoCheck(string authenticationToken, string taxIdNumber)
		{
			if (!ErpCustomersSuppliersDblApplication.ErpCustomersSuppliersDblEngine.TryLockResources())
				throw new Exception(Strings.ResourcesTimeout);

			try
			{
				return ErpCustomersSuppliersDblApplication.ErpCustomersSuppliersDblEngine.TaxIdNoCheck(authenticationToken, taxIdNumber);
			}
			catch (Exception exc)
			{
				ErpCustomersSuppliersDblApplication.ErpCustomersSuppliersDblEngine.diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Warning, MethodInfo.GetCurrentMethod().Name + ": " + exc.ToString());
				throw;
			}
			finally
			{
				ErpCustomersSuppliersDblApplication.ErpCustomersSuppliersDblEngine.ReleaseResources();
			}
		}

		[WebMethod]
		//---------------------------------------------------------------------------
		public bool TaxIdNoSecondCheck(string authenticationToken, string taxIdNumber)
		{
			if (!ErpCustomersSuppliersDblApplication.ErpCustomersSuppliersDblEngine.TryLockResources())
				throw new Exception(Strings.ResourcesTimeout);

			try
			{
				return ErpCustomersSuppliersDblApplication.ErpCustomersSuppliersDblEngine.TaxIdNoSecondCheck(authenticationToken, taxIdNumber);
			}
			catch (Exception exc)
			{
				ErpCustomersSuppliersDblApplication.ErpCustomersSuppliersDblEngine.diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Warning, MethodInfo.GetCurrentMethod().Name + ": " + exc.ToString());
				throw;
			}
			finally
			{
				ErpCustomersSuppliersDblApplication.ErpCustomersSuppliersDblEngine.ReleaseResources();
			}
		}
	}
}
