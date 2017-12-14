
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.TaskBuilderNet.Core.Generic
{
	//=========================================================================
	public sealed class TraceActionName
	{
		//---------------------------------------------------------------------
		private TraceActionName()
		{ }

		//---------------------------------------------------------------------
		public static string GetTraceVersionName(TraceActionType type)
		{
			if (type == TraceActionType.Login)
				return GenericStrings.Login;
			else if (type == TraceActionType.Logout)
				return GenericStrings.Logout;
			else if (type == TraceActionType.ChangePassword)
				return GenericStrings.ChangePassword;
			else if (type == TraceActionType.LoginFailed)
				return GenericStrings.LoginFailed;
			else if (type == TraceActionType.ChangePasswordFailed)
				return GenericStrings.ChangePasswordFailed;
			else if (type == TraceActionType.DeleteUser)
				return GenericStrings.DeleteUser;
			else if (type == TraceActionType.DeleteCompany)
				return GenericStrings.DeleteCompany;
			else if (type == TraceActionType.DeleteCompanyUser)
				return GenericStrings.DeleteCompanyUser;
			else if (type == TraceActionType.All)
				return GenericStrings.All;
			else
				return GenericStrings.Undefined;
		}
	}
}
