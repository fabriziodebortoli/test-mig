using System.Collections.Specialized;

namespace Microarea.WebServices.LockManager
{
	/// <summary>
	/// Summary description for LoginCache.
	/// </summary>
	//----------------------------------------------------------------------
	public class LoginCache
	{
		private StringCollection validTokens	=   new StringCollection();
		private StringCollection inValidTokens	=   new StringCollection();

		private Microarea.TaskBuilderNet.Core.WebServicesWrapper.LoginManager loginManager;

		//----------------------------------------------------------------------
		public LoginCache(string loginUrl)
		{
			loginManager = new Microarea.TaskBuilderNet.Core.WebServicesWrapper.LoginManager(loginUrl, 12000);
		}

		//----------------------------------------------------------------------
		public bool CheckToken(string authenticationToken)
		{
			if (validTokens.Contains(authenticationToken))
				return true;

			if (inValidTokens.Contains(authenticationToken))
				return false;

			bool valid = false;
			try
			{
				valid = loginManager.IsValidToken(authenticationToken);
			}
			catch 
			{
			}
			if (valid)
				validTokens.Add(authenticationToken);
			else
				inValidTokens.Add(authenticationToken);

			return valid;
		}
	}
}
