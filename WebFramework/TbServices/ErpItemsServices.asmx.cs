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

	public class ErpItemsServices : System.Web.Services.WebService
	{
		[WebMethod]
		//---------------------------------------------------------------------------
		public string GetPathItemImageFromNamespace(string authenticationToken, string aNamespace)
		{
			if (!ErpItemsServicesApplication.ErpItemsServicesEngine.TryLockResources())
				throw new Exception(Strings.ResourcesTimeout);

			try
			{
				return ErpItemsServicesApplication.ErpItemsServicesEngine.GetPathItemImageFromNamespace(authenticationToken, aNamespace);
			}
			catch (Exception exc)
			{
				ErpItemsServicesApplication.ErpItemsServicesEngine.diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Warning, MethodInfo.GetCurrentMethod().Name + ": " + exc.ToString());
				throw;
			}
			finally
			{
				ErpItemsServicesApplication.ErpItemsServicesEngine.ReleaseResources();
			}
		}
	}
}
