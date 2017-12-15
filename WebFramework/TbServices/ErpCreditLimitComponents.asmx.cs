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

    public class ErpCreditLimitComponents : System.Web.Services.WebService
    {
        [WebMethod]
        //---------------------------------------------------------------------------
        public int CreditLimitManager_Create(string authenticationToken)
        {
            if (!ErpCreditLimitComponentsApplication.ErpCreditLimitComponentsEngine.TryLockResources())
                throw new Exception(Strings.ResourcesTimeout);

            try
            {
                return ErpCreditLimitComponentsApplication.ErpCreditLimitComponentsEngine.CreditLimitManager_Create(authenticationToken);
            }
            catch (Exception exc)
            {
                ErpCreditLimitComponentsApplication.ErpCreditLimitComponentsEngine.diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Warning, MethodInfo.GetCurrentMethod().Name + ": " + exc.ToString());
                throw;
            }
            finally
            {
                ErpCreditLimitComponentsApplication.ErpCreditLimitComponentsEngine.ReleaseResources();
            }
        }

        [WebMethod]
        //---------------------------------------------------------------------------
        public bool CreditLimitManager_Dispose(string authenticationToken, int handle)
        {
            if (!ErpCreditLimitComponentsApplication.ErpCreditLimitComponentsEngine.TryLockResources())
                throw new Exception(Strings.ResourcesTimeout);

            try
            {
                return ErpCreditLimitComponentsApplication.ErpCreditLimitComponentsEngine.CreditLimitManager_Dispose(authenticationToken, handle);
            }
            catch (Exception exc)
            {
                ErpCreditLimitComponentsApplication.ErpCreditLimitComponentsEngine.diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Warning, MethodInfo.GetCurrentMethod().Name + ": " + exc.ToString());
                throw;
            }
            finally
            {
                ErpCreditLimitComponentsApplication.ErpCreditLimitComponentsEngine.ReleaseResources();
            }
        }
        [WebMethod]
        //---------------------------------------------------------------------------
        public bool CreditLimitManager_GetData(string authenticationToken,
                                                  int handle,
                                                  ref string Customer,
                                                  ref bool CreditLimitManage,
                                                  ref bool Blocked,
                                                  ref double OrderedExposure,
                                                  ref double OrderedMargin,
                                                  ref double TotalExposure,
                                                  ref double TotalExposureMargin)
        {
            if (!ErpCreditLimitComponentsApplication.ErpCreditLimitComponentsEngine.TryLockResources())
                throw new Exception(Strings.ResourcesTimeout);

            try
            {
                return ErpCreditLimitComponentsApplication.ErpCreditLimitComponentsEngine.CreditLimitManager_GetData(authenticationToken, handle, ref Customer, ref CreditLimitManage, ref Blocked, ref OrderedExposure, ref OrderedMargin, ref TotalExposure, ref TotalExposureMargin);
            }
            catch (Exception exc)
            {
                ErpCreditLimitComponentsApplication.ErpCreditLimitComponentsEngine.diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Warning, MethodInfo.GetCurrentMethod().Name + ": " + exc.ToString());
                throw;
            }
            finally
            {
                ErpCreditLimitComponentsApplication.ErpCreditLimitComponentsEngine.ReleaseResources();
            }
        }
    }
}
