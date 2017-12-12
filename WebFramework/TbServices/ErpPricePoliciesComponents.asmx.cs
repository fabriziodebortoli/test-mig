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

	public class ErpPricePoliciesComponents : System.Web.Services.WebService
	{
		[WebMethod]
		//---------------------------------------------------------------------------
		public int DefaultSalePrices_Create(string authenticationToken)
		{
			if (!ErpPricePoliciesComponentApplication.ErpPricePoliciesComponentsEngine.TryLockResources())
				throw new Exception(Strings.ResourcesTimeout);

			try
			{
				return ErpPricePoliciesComponentApplication.ErpPricePoliciesComponentsEngine.DefaultSalePrices_Create(authenticationToken);
			}
			catch (Exception exc)
			{
				ErpPricePoliciesComponentApplication.ErpPricePoliciesComponentsEngine.diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Warning, MethodInfo.GetCurrentMethod().Name + ": " + exc.ToString());
				throw;
			}
			finally
			{
				ErpPricePoliciesComponentApplication.ErpPricePoliciesComponentsEngine.ReleaseResources();
			}
		}

		[WebMethod]
		//---------------------------------------------------------------------------
		public bool DefaultSalePrices_Dispose(string authenticationToken, int handle)
		{
			if (!ErpPricePoliciesComponentApplication.ErpPricePoliciesComponentsEngine.TryLockResources())
				throw new Exception(Strings.ResourcesTimeout);

			try
			{
				return ErpPricePoliciesComponentApplication.ErpPricePoliciesComponentsEngine.DefaultSalePrices_Dispose(authenticationToken, handle);
			}
			catch (Exception exc)
			{
				ErpPricePoliciesComponentApplication.ErpPricePoliciesComponentsEngine.diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Warning, MethodInfo.GetCurrentMethod().Name + ": " + exc.ToString());
				throw;
			}
			finally
			{
				ErpPricePoliciesComponentApplication.ErpPricePoliciesComponentsEngine.ReleaseResources();
			}
		}

		[WebMethod]
		//---------------------------------------------------------------------------
		public void DefaultSalePrices_GetDefaultDiscountPerc(string authenticationToken, int handle, string Customer, string Item, string UoM, double Quantity, ref double aDiscount1, ref double aDiscount2)
		{
			if (!ErpPricePoliciesComponentApplication.ErpPricePoliciesComponentsEngine.TryLockResources())
				throw new Exception(Strings.ResourcesTimeout);

			try
			{
				ErpPricePoliciesComponentApplication.ErpPricePoliciesComponentsEngine.DefaultSalePrices_GetDefaultDiscountPerc(authenticationToken, handle, Customer, Item, UoM, Quantity, ref aDiscount1, ref aDiscount2);
			}
			catch (Exception exc)
			{
				ErpPricePoliciesComponentApplication.ErpPricePoliciesComponentsEngine.diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Warning, MethodInfo.GetCurrentMethod().Name + ": " + exc.ToString());
				throw;
			}
			finally
			{
				ErpPricePoliciesComponentApplication.ErpPricePoliciesComponentsEngine.ReleaseResources();
			}
		}

		[WebMethod]
		//---------------------------------------------------------------------------
		public string DefaultSalePrices_GetDefaultDiscountString(string authenticationToken, int handle, string Customer, string Item, string UoM, double Quantity)
		{
			if (!ErpPricePoliciesComponentApplication.ErpPricePoliciesComponentsEngine.TryLockResources())
				throw new Exception(Strings.ResourcesTimeout);

			try
			{
				return ErpPricePoliciesComponentApplication.ErpPricePoliciesComponentsEngine.DefaultSalePrices_GetDefaultDiscountString(authenticationToken, handle, Customer, Item, UoM, Quantity);
			}
			catch (Exception exc)
			{
				ErpPricePoliciesComponentApplication.ErpPricePoliciesComponentsEngine.diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Warning, MethodInfo.GetCurrentMethod().Name + ": " + exc.ToString());
				throw;
			}
			finally
			{
				ErpPricePoliciesComponentApplication.ErpPricePoliciesComponentsEngine.ReleaseResources();
			}
		}

		[WebMethod]
		//---------------------------------------------------------------------------
		public double DefaultSalePrices_GetDefaultPrice(string authenticationToken, int handle, string Customer, string Item, string UoM, double Quantity)
		{
			if (!ErpPricePoliciesComponentApplication.ErpPricePoliciesComponentsEngine.TryLockResources())
				throw new Exception(Strings.ResourcesTimeout);

			try
			{
				return ErpPricePoliciesComponentApplication.ErpPricePoliciesComponentsEngine.DefaultSalePrices_GetDefaultPrice(authenticationToken, handle, Customer, Item, UoM, Quantity);
			}
			catch (Exception exc)
			{
				ErpPricePoliciesComponentApplication.ErpPricePoliciesComponentsEngine.diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Warning, MethodInfo.GetCurrentMethod().Name + ": " + exc.ToString());
				throw;
			}
			finally
			{
				ErpPricePoliciesComponentApplication.ErpPricePoliciesComponentsEngine.ReleaseResources();
			}
		}

        [WebMethod]
        //---------------------------------------------------------------------------
        public double DefaultSalePrices_GetDefaultDiscountPercEx(string authenticationToken, int handle, string Customer, string Item, string UoM, double Quantity, ref double aDiscount1, ref double aDiscount2, ref string aDiscountFormula) 
        {
            if (!ErpPricePoliciesComponentApplication.ErpPricePoliciesComponentsEngine.TryLockResources())
                throw new Exception(Strings.ResourcesTimeout);

            try
            {
                return ErpPricePoliciesComponentApplication.ErpPricePoliciesComponentsEngine.DefaultSalePrices_GetDefaultDiscountPercEx(authenticationToken, handle, Customer, Item, UoM, Quantity, ref aDiscount1, ref aDiscount2, ref aDiscountFormula);
            }
            catch (Exception exc)
            {
                ErpPricePoliciesComponentApplication.ErpPricePoliciesComponentsEngine.diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Warning, MethodInfo.GetCurrentMethod().Name + ": " + exc.ToString());
                throw;
            }
            finally
            {
                ErpPricePoliciesComponentApplication.ErpPricePoliciesComponentsEngine.ReleaseResources();
            }
        }

		[WebMethod]
		//---------------------------------------------------------------------------
		public void DefaultSalePrices_SetCurrencyInfo(string authenticationToken, int handle, string Currency, string FixingDate, double Fixing, bool FixingIsManual)
		{
			if (!ErpPricePoliciesComponentApplication.ErpPricePoliciesComponentsEngine.TryLockResources())
				throw new Exception(Strings.ResourcesTimeout);

			try
			{
				ErpPricePoliciesComponentApplication.ErpPricePoliciesComponentsEngine.DefaultSalePrices_SetCurrencyInfo(authenticationToken, handle, Currency, FixingDate, Fixing, FixingIsManual);
			}
			catch (Exception exc)
			{
				ErpPricePoliciesComponentApplication.ErpPricePoliciesComponentsEngine.diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Warning, MethodInfo.GetCurrentMethod().Name + ": " + exc.ToString());
				throw;
			}
			finally
			{
				ErpPricePoliciesComponentApplication.ErpPricePoliciesComponentsEngine.ReleaseResources();
			}
		}

		[WebMethod]
		//---------------------------------------------------------------------------
		public void DefaultSalePrices_SetDocumentDate(string authenticationToken, int handle, string documentDate)
		{
			if (!ErpPricePoliciesComponentApplication.ErpPricePoliciesComponentsEngine.TryLockResources())
				throw new Exception(Strings.ResourcesTimeout);

			try
			{
				ErpPricePoliciesComponentApplication.ErpPricePoliciesComponentsEngine.DefaultSalePrices_SetDocumentDate(authenticationToken, handle, documentDate);
			}
			catch (Exception exc)
			{
				ErpPricePoliciesComponentApplication.ErpPricePoliciesComponentsEngine.diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Warning, MethodInfo.GetCurrentMethod().Name + ": " + exc.ToString());
				throw;
			}
			finally
			{
				ErpPricePoliciesComponentApplication.ErpPricePoliciesComponentsEngine.ReleaseResources();
			}
		}

		[WebMethod]
		//---------------------------------------------------------------------------
		public void DefaultSalePrices_SetNetOfTax(string authenticationToken, int handle, bool netOfTax)
		{
			if (!ErpPricePoliciesComponentApplication.ErpPricePoliciesComponentsEngine.TryLockResources())
				throw new Exception(Strings.ResourcesTimeout);

			try
			{
				ErpPricePoliciesComponentApplication.ErpPricePoliciesComponentsEngine.DefaultSalePrices_SetNetOfTax(authenticationToken, handle, netOfTax);
			}
			catch (Exception exc)
			{
				ErpPricePoliciesComponentApplication.ErpPricePoliciesComponentsEngine.diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Warning, MethodInfo.GetCurrentMethod().Name + ": " + exc.ToString());
				throw;
			}
			finally
			{
				ErpPricePoliciesComponentApplication.ErpPricePoliciesComponentsEngine.ReleaseResources();
			}
		}

		[WebMethod]
		//---------------------------------------------------------------------------
		public void DefaultSalePrices_SetPriceList(string authenticationToken, int handle, string priceList)
		{
			if (!ErpPricePoliciesComponentApplication.ErpPricePoliciesComponentsEngine.TryLockResources())
				throw new Exception(Strings.ResourcesTimeout);

			try
			{
				ErpPricePoliciesComponentApplication.ErpPricePoliciesComponentsEngine.DefaultSalePrices_SetPriceList(authenticationToken, handle, priceList);
			}
			catch (Exception exc)
			{
				ErpPricePoliciesComponentApplication.ErpPricePoliciesComponentsEngine.diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Warning, MethodInfo.GetCurrentMethod().Name + ": " + exc.ToString());
				throw;
			}
			finally
			{
				ErpPricePoliciesComponentApplication.ErpPricePoliciesComponentsEngine.ReleaseResources();
			}
		}

		[WebMethod]
		//---------------------------------------------------------------------------
		public void DefaultSalePrices_SetValidityDate(string authenticationToken, int handle, string validityDate)
		{
			if (!ErpPricePoliciesComponentApplication.ErpPricePoliciesComponentsEngine.TryLockResources())
				throw new Exception(Strings.ResourcesTimeout);

			try
			{
				ErpPricePoliciesComponentApplication.ErpPricePoliciesComponentsEngine.DefaultSalePrices_SetValidityDate(authenticationToken, handle, validityDate);
			}
			catch (Exception exc)
			{
				ErpPricePoliciesComponentApplication.ErpPricePoliciesComponentsEngine.diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Warning, MethodInfo.GetCurrentMethod().Name + ": " + exc.ToString());
				throw;
			}
			finally
			{
				ErpPricePoliciesComponentApplication.ErpPricePoliciesComponentsEngine.ReleaseResources();
			}
		}

		[WebMethod]
		//---------------------------------------------------------------------------
		public void ItemPriceFromPriceListToDate(string authenticationToken, ref string tPriceList, ref string item, ref double quantity, ref string date)
		{
			if (!ErpPricePoliciesComponentApplication.ErpPricePoliciesComponentsEngine.TryLockResources())
				throw new Exception(Strings.ResourcesTimeout);

			try
			{
				ErpPricePoliciesComponentApplication.ErpPricePoliciesComponentsEngine.ItemPriceFromPriceListToDate(authenticationToken, ref tPriceList, ref item, ref quantity, ref date);
			}
			catch (Exception exc)
			{
				ErpPricePoliciesComponentApplication.ErpPricePoliciesComponentsEngine.diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Warning, MethodInfo.GetCurrentMethod().Name + ": " + exc.ToString());
				throw;
			}
			finally
			{
				ErpPricePoliciesComponentApplication.ErpPricePoliciesComponentsEngine.ReleaseResources();
			}
		}
	}
}
