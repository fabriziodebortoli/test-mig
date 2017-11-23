using Microarea.AdminServer.Libraries;
using Microarea.AdminServer.Services;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace Microarea.AdminServer.Controllers.Helpers.All
{
	//================================================================================
	public class AuthorizationHelper
    {
		//--------------------------------------------------------------------------------
		public static OperationResult VerifyPermissionOnGWAM(string permissionToken, IHttpHelper httpHelper, string GWAMUrl)
		{
			OperationResult opRes = new OperationResult(true, String.Empty);

			try
			{
				Task<string> responseData = SecurityManager.ValidatePermission(permissionToken, httpHelper, GWAMUrl);

				if (responseData.Status == TaskStatus.Faulted)
				{
					opRes.Code = 500;
					opRes.Result = false;
					opRes.Message = "Permission token cannot be verified, operation aborted.";
					return opRes;
				}

				OperationResult validateRes = JsonConvert.DeserializeObject<OperationResult>(responseData.Result);

				if (validateRes == null || !validateRes.Result)
				{
					opRes.Code = 401;
					opRes.Result = false;
					opRes.Message = "Invalid permission token, operation aborted.";
					return opRes;
				}

				// everything's ok

				return opRes;
			}
			catch (Exception e)
			{
				opRes.Code = 500;
				opRes.Result = false;
				opRes.Message = "AuthorizationHelper.VerifyPermissionOnGWAM raised an error: " + e.Message;
				return opRes;
			}
		}
    }
}
