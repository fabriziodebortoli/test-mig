using System;
using System.Collections.Generic;
using Microarea.TaskBuilderNet.TbSenderBL.postalite.api;
using PostaLiteService = Microarea.TaskBuilderNet.TbSenderBL.postalite.api.Service_Test;
using System.Net.NetworkInformation;

namespace Microarea.TaskBuilderNet.TbSenderBL.PostaLite
{
	public class PostaLiteServiceWrapper : IPostaLiteService
	{
		readonly PostaLiteService svc;

		public PostaLiteServiceWrapper()
		{
			svc = new PostaLiteService();
			//svc.Url = "http://localhost/PostaLiteEmulator/PostaLiteService.asmx"; // Emulatore
			//svc.Url = "http://www.PostaLite.it/webservices/service_test.asmx"; // WS di test
			//svc.Url = "https://www.PostaLite.it/webservices/service.asmx"; // WS di produzione
			svc.Url = System.Configuration.ConfigurationManager.AppSettings["PostaLiteWS"];
			svc.Timeout = 600000;
		}

		public IUserCredentials Subscribe(ISubscriberInfo subscriberInfo)
		{
			CheckNetworkAvailability();
			return svc.Subscribe((SubscriberInfo)subscriberInfo);
		}

		public ISubscriberInfo GetSubscriptionInfo(string login, string password, out IUserCredentials credentials)
		{
			CheckNetworkAvailability();
			UserCredentials cred;
			SubscriberInfo subscriberInfo = svc.GetSubscriptionInfo(login, password, out cred);
			credentials = cred;
			return subscriberInfo;
		}

		public ICreditState Charge(IUserCredentials credentials, IFileMessage paymentNote,
			decimal amountCharge, double vat, decimal amountActivation, decimal totalAmount)
		{
			CheckNetworkAvailability();
			return svc.Charge((UserCredentials)credentials, (FileMessage)paymentNote, 
				amountCharge, amountActivation, vat, totalAmount);
		}

		public ICreditState GetCreditState(IUserCredentials credentials)
		{
			CheckNetworkAvailability();
			return svc.GetCreditState((UserCredentials)credentials);
		}

		public IEstimate[] GetLotCostEstimate(IUserCredentials credentials, ILotInfo[] lots)
		{
			CheckNetworkAvailability();
			List<LotInfo> lotInfos = null;
			if (lots != null)
			{
				lotInfos = new List<LotInfo>();
				foreach (ILotInfo lot in lots)
					lotInfos.Add((LotInfo)lot);
			}
			return svc.GetLotCostsEstimates((UserCredentials)credentials, 
				lotInfos != null ? lotInfos.ToArray() : null);
		}

		public ISentLot SendLot(IUserCredentials credentials, ILotInfo lot, IFileMessage lotMessage)
		{
			CheckNetworkAvailability();
			return svc.SendLot((UserCredentials)credentials, (LotInfo)lot, (FileMessage)lotMessage);
		}

		public ILotState[] GetLotState(IUserCredentials credentials, int[] idsLot)
		{
			CheckNetworkAvailability();
			LotState[] res = svc.GetLotState((UserCredentials)credentials, idsLot);
			List<ILotState> list = new List<ILotState>();
			foreach (LotState ls in res)
				list.Add(ls);
			return list.ToArray();
		}

		public string GetCadastralCode(string city, out int error)
		{
			CheckNetworkAvailability();
			return svc.GetCadastralCode(city, out error);
		}

		public IEstimateCharge GetEstimateCharge(IUserCredentials credentials, decimal amountCharge)
		{
			CheckNetworkAvailability();
			return svc.GetEstimateCharge((UserCredentials)credentials, amountCharge);
		}

		public LegalInfos GetLegalInfos(string language)
		{
			CheckNetworkAvailability();
			return svc.GetLegalInfos(language);
		}

		//---------------------------------------------------------------------
		//public static void Test()
		//{
		//}
		private static void CheckNetworkAvailability()
		{
			bool isNetWorkAvailable = NetworkInterface.GetIsNetworkAvailable();
			if (false == isNetWorkAvailable)
				throw new ApplicationException(LocalizedStrings.NoNetworkConnectionException);
		}
	}
}
