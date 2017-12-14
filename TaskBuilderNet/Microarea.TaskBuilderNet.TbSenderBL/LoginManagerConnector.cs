using System;
using System.Collections.Generic;
using System.Linq;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Core.WebServicesWrapper;

namespace Microarea.TaskBuilderNet.TbSenderBL
{
	public static class LoginManagerConnector // TODO togli singleton e usa IoC - così è intestabile!
	{
		static LoginManager lm = new LoginManager();
		static Dictionary<string, TbSenderDatabaseInfo> companiesInfo;
		private class CmpInfoLocker { }
		const string callGuid = "{4919D404-8B8D-4DC6-BDE0-45B81332A177}";

		//-------------------------------------------------------------------------------
		private static Dictionary<string, TbSenderDatabaseInfo> CompaniesInfo 
		{
			get 
			{
				lock (typeof(CmpInfoLocker))
				{
					if (companiesInfo == null)
					{
						List<TbSenderDatabaseInfo> list = lm.GetCompanyDatabasesInfo(callGuid);
						Dictionary<string, TbSenderDatabaseInfo> dic = new Dictionary<string, TbSenderDatabaseInfo>(StringComparer.InvariantCultureIgnoreCase);
						foreach (TbSenderDatabaseInfo dbInfo in list)
						{
							if (false == dbInfo.IsEnabled) // controlla sia un db sottoscritto al servizio PostaLite
								continue;
							if (dic.ContainsKey(dbInfo.Company))
								continue;
							dic[dbInfo.Company] = dbInfo;
						}
						companiesInfo = dic;
					}
					return companiesInfo;
				}
			} 
		}

		//-------------------------------------------------------------------------------
		static public List<string> GetSubscribedCompaniesDescriptors()
		{
			// TODO fai spirare il dizionario dopo un po'
			return CompaniesInfo.Keys.OrderBy(x => x).ToList();
		}

		//-------------------------------------------------------------------------------
		internal static void Restart()
		{
			lock (typeof(CmpInfoLocker))
				companiesInfo = null; // lo rilegge non appena ne ha bisogno
			//companiesInfo = lm.GetCompanyDatabasesInfo(callGuid);
		}

		//-------------------------------------------------------------------------------
		internal static TbSenderDatabaseInfo GetCompaniesInfo(string company)
		{
			if (company == null)
				return CompaniesInfo.Values.FirstOrDefault();

			TbSenderDatabaseInfo dbInfo;
			CompaniesInfo.TryGetValue(company, out dbInfo);
			return dbInfo;
		}

		//-------------------------------------------------------------------------------
		internal static void SendBalloon(string message)
		{
			string bodyMessage = message;
			List<string> recipients = null;
			lm.SendBalloon(callGuid, bodyMessage, Interfaces.MessageType.PostaLite, recipients);
		}

		//-------------------------------------------------------------------------------
		internal static string GetUserInfoID()
		{
			return lm.GetUserInfoID();
		}
	}
}
