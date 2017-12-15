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
	public class ErpCreditLimitComponentsEngine
    {
		private const int timeout = 15000;
		internal Diagnostic diagnostic = new Diagnostic("ErpCreditLimitComponentsEngine");  	//Gestione errori
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
						string.Format("Failed to lock ErpCreditLimitComponents after {0} milliseconds. Method: {1}", timeout, locker));
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
        internal int CreditLimitManager_Create(string authenticationToken)
        {
            using (TbLoaderClientInterface tb = InitializeTbLoader(authenticationToken))
            return tb.CreditLimitManager_Create();

        }

        //---------------------------------------------------------------------------
        internal bool CreditLimitManager_Dispose(string authenticationToken, int handle)
        {
           using (TbLoaderClientInterface tb = InitializeTbLoader(authenticationToken))
               return tb.CreditLimitManager_Dispose(handle); 
        }

        //---------------------------------------------------------------------------
        internal bool CreditLimitManager_GetData(
                                                  string authenticationToken,
                                                  int handle,
                                                  ref string Customer,
                                                  ref bool CreditLimitManage,
                                                  ref bool Blocked,
                                                  ref double OrderedExposure,
                                                  ref double OrderedMargin,
                                                  ref double TotalExposure,
                                                  ref double TotalExposureMargin)
        {
            using (TbLoaderClientInterface tb = InitializeTbLoader(authenticationToken))
                 return tb.CreditLimitManager_GetData(handle, Customer, ref CreditLimitManage, ref Blocked, ref OrderedExposure, ref OrderedMargin, ref TotalExposure, ref TotalExposureMargin);
   
        }
        	
    
	}

	/// <summary>
	/// Callse che contiene l'istanza statica dell'applicazione
	/// </summary>
	//=========================================================================
	public class ErpCreditLimitComponentsApplication
    {
		public static ErpCreditLimitComponentsEngine ErpCreditLimitComponentsEngine = new ErpCreditLimitComponentsEngine();
	}
}