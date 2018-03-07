
using Microarea.Common.Generic;
using Microarea.Common.NameSolver;

namespace Microarea.TaskBuilderNet.Core.Generic
{
	public class MenuStaticFunctions
	{
		//--------------------------------------------------------------------------------------------------------------------------------
		public static string PingViaSMSUrl()
		{
			return PathFinder.PathFinderInstance.PingViaSMSPage;
		}

		//--------------------------------------------------------------------------------------------------------------------------------
        public static string ProducerSiteUrl()
		{
            return InstallationData.BrandLoader.GetBrandedStringBySourceString("ProducerSite");
		}

		////go to producer site = InstallationData.BrandLoader.GetBrandedStringBySourceString("ProducerSite");
		////--------------------------------------------------------------------------------------------------------------------------------
		//public static void GoToProducerSite()
		//{
		//	HelpManager.ConnectToProducerSite();
		//}

		////  /\
		////  questi due metodi invertiti come da mail di germano del 27/6/11
		////  \/
		////go to private area = PathFinder.PathFinderInstance.LoginManagerBaseUrl + "SitePrivateArea.aspx" + "?u=" + HttpUtility.UrlEncode(CreateTempToken(LoginManagerPrivateAreaUrl))
		////--------------------------------------------------------------------------------------------------------------------------------
		//public static void GoToPrivateArea(string authToken)
		//{
		//	HelpManager.ConnectToProducerSiteLoginPage(authToken);
		//}

		
	}
}
