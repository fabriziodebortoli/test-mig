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
	public class ErpPricePoliciesComponentsEngine
	{
		private const int timeout = 15000;
		internal Diagnostic diagnostic = new Diagnostic("ErpPricePoliciesComponentsEngine");  	//Gestione errori
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
						string.Format("Failed to lock LoginManager after {0} milliseconds. Method: {1}", timeout, locker));
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
		internal int DefaultSalePrices_Create(string authenticationToken)
		{
			using (TbLoaderClientInterface tb = InitializeTbLoader(authenticationToken))
				return tb.DefaultSalePrices_Create();
		}

		//---------------------------------------------------------------------------
		internal bool DefaultSalePrices_Dispose(string authenticationToken, int handle)
		{
			using (TbLoaderClientInterface tb = InitializeTbLoader(authenticationToken))
				return tb.DefaultSalePrices_Dispose(handle);
		}

		//---------------------------------------------------------------------------
		internal void DefaultSalePrices_GetDefaultDiscountPerc(string authenticationToken, int handle, string Customer, string Item, string UoM, double Quantity, ref double aDiscount1, ref double aDiscount2)
		{
			using (TbLoaderClientInterface tb = InitializeTbLoader(authenticationToken))
				tb.DefaultSalePrices_GetDefaultDiscountPerc(handle, Customer, Item, UoM, Quantity, ref aDiscount1, ref aDiscount2);
		}

		//---------------------------------------------------------------------------
		internal string DefaultSalePrices_GetDefaultDiscountString(string authenticationToken, int handle, string Customer, string Item, string UoM, double Quantity)
		{
			using (TbLoaderClientInterface tb = InitializeTbLoader(authenticationToken))
				return tb.DefaultSalePrices_GetDefaultDiscountString(handle, Customer, Item, UoM, Quantity);
		}

		//---------------------------------------------------------------------------
        internal double DefaultSalePrices_GetDefaultDiscountPercEx(string authenticationToken, int handle, string Customer, string Item, string UoM, double Quantity, ref double aDiscount1, ref double aDiscount2, ref string aDiscountFormula)
		{
			using (TbLoaderClientInterface tb = InitializeTbLoader(authenticationToken))
                return tb.DefaultSalePrices_GetDefaultDiscountPercEx(handle, Customer, Item, UoM, Quantity, ref aDiscount1, ref aDiscount2, ref aDiscountFormula);
		}

        //---------------------------------------------------------------------------
        internal double DefaultSalePrices_GetDefaultPrice(string authenticationToken, int handle, string Customer, string Item, string UoM, double Quantity)
        {
            using (TbLoaderClientInterface tb = InitializeTbLoader(authenticationToken))
                return tb.DefaultSalePrices_GetDefaultPrice(handle, Customer, Item, UoM, Quantity);
        }


		//---------------------------------------------------------------------------
		internal void DefaultSalePrices_SetCurrencyInfo(string authenticationToken, int handle, string Currency, string FixingDate, double Fixing, bool FixingIsManual)
		{
			using (TbLoaderClientInterface tb = InitializeTbLoader(authenticationToken))
				tb.DefaultSalePrices_SetCurrencyInfo(handle, Currency, FixingDate, Fixing, FixingIsManual);
		}

		//---------------------------------------------------------------------------
		internal void DefaultSalePrices_SetDocumentDate(string authenticationToken, int handle, string documentDate)
		{
			using (TbLoaderClientInterface tb = InitializeTbLoader(authenticationToken))
				tb.DefaultSalePrices_SetDocumentDate(handle, documentDate);
		}

		//---------------------------------------------------------------------------
		internal void DefaultSalePrices_SetNetOfTax(string authenticationToken, int handle, bool netOfTax)
		{
			using (TbLoaderClientInterface tb = InitializeTbLoader(authenticationToken))
				tb.DefaultSalePrices_SetNetOfTax(handle, netOfTax);
		}

		//---------------------------------------------------------------------------
		internal void DefaultSalePrices_SetPriceList(string authenticationToken, int handle, string priceList)
		{
			using (TbLoaderClientInterface tb = InitializeTbLoader(authenticationToken))
				tb.DefaultSalePrices_SetPriceList(handle, priceList);
		}

		//---------------------------------------------------------------------------
		internal void DefaultSalePrices_SetValidityDate(string authenticationToken, int handle, string validityDate)
		{
			using (TbLoaderClientInterface tb = InitializeTbLoader(authenticationToken))
				tb.DefaultSalePrices_SetValidityDate(handle, validityDate);
		}

		//---------------------------------------------------------------------------
		internal void ItemPriceFromPriceListToDate(string authenticationToken, ref string tPriceList, ref string item, ref double quantity, ref string date)
		{
			using (TbLoaderClientInterface tb = InitializeTbLoader(authenticationToken))
				tb.ItemPriceFromPriceListToDate(ref tPriceList, ref item, ref quantity, ref date);
		}
	}

	/// <summary>
	/// Callse che contiene l'istanza statica dell'applicazione
	/// </summary>
	//=========================================================================
	public class ErpPricePoliciesComponentApplication
	{
		public static ErpPricePoliciesComponentsEngine ErpPricePoliciesComponentsEngine = new ErpPricePoliciesComponentsEngine();
	}
}