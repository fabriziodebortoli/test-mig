using Microarea.TaskBuilderNet.TbSenderBL.PostaLite;
using Microarea.TaskBuilderNet.TbSenderBL.postalite.api;
using Microarea.TaskBuilderNet.Interfaces;
using System;
using System.Threading.Tasks;

namespace Microarea.TaskBuilderNet.TbSenderBL
{
	public class SubscriptionEngine : ICredentialsProvider
	{
		readonly IPostaLiteService plSvc;
		readonly IPostaLiteSettingsProvider postaLiteSettingsProvider;
		readonly IDiagnostic diagnostic;
		readonly IDateTimeProvider dateTimeProvider;

		//-------------------------------------------------------------------------------
		public SubscriptionEngine
			(
			IPostaLiteService plSvc, 
			IPostaLiteSettingsProvider postaLiteSettingsProvider,
			IDiagnostic diagnostic,
			IDateTimeProvider dateTimeProvider
			)
		{
			this.plSvc = plSvc;
			this.postaLiteSettingsProvider = postaLiteSettingsProvider;
			this.diagnostic = diagnostic;
			this.dateTimeProvider = dateTimeProvider;
		}

		//---------------------------------------------------------------------
		public IChargeTracker ChargeTracker { get; set; }

		//---------------------------------------------------------------------
		public IUserCredentials GetCredentials(string company)
		{
			// 
			PostaliteSettings param = postaLiteSettingsProvider.GetSettings(company);
			if (param == null)
				return null;

			IUserCredentials credentials = new UserCredentials()
			{
				Login = param.LoginId,
				TokenAuth = param.TokenAuth
			};
			return credentials;
		}

		//---------------------------------------------------------------------
		public IUserCredentials Subscribe(ISubscriberInfo subscriberInfo)
		{
			subscriberInfo.Subscriber_CompanyID = "Mago"; // costante richiesta da PostaLite per l'integrazione con Mago.net
			IUserCredentials credentials = plSvc.Subscribe(subscriberInfo);
			return credentials;
		}

		//---------------------------------------------------------------------
		public IUserCredentials Login(string login, string password, out ISubscriberInfo subscriberInfo)
		{
			IUserCredentials uc;
			ISubscriberInfo si = plSvc.GetSubscriptionInfo(login, password, out uc);
			subscriberInfo = si;
			return uc;
		}

		//---------------------------------------------------------------------
		public ICreditState GetCreditState(IUserCredentials credentials)
		{
			return plSvc.GetCreditState(credentials);
		}
		
		//---------------------------------------------------------------------
		public ICreditState Charge(string token, string fileContentBase64,
			decimal amountCharge, double vat, decimal amountActivation, decimal totalAmount)
		{
			IFileMessage paymentNote = new FileMessage()
			{
				FileName = Guid.NewGuid().ToString() + ".pdf",
				FileContentBase64 = fileContentBase64
			};

			IUserCredentials credentials = new UserCredentials();
			credentials.TokenAuth = token;

			ICreditState creditState = plSvc.Charge(credentials, paymentNote, 
				amountCharge, vat, amountActivation, totalAmount);
			return creditState;
		}

		//---------------------------------------------------------------------
		public void NotifyAfterCharge // essendo un metodo public non voglio sia troppo esplicito il nome
			(
			string loginId,
			decimal amountCharge, double vat, decimal amountActivation, decimal totalAmount
			)
		{
			// TODO per sicurezza anche la chiamata a login manager la farei non bloccante
			string userID = LoginManagerConnector.GetUserInfoID(); // TODO rimuovi static!!!!

			if (this.ChargeTracker != null)
			{
				//DateTime chargeTime = dateTimeProvider.Now;
				// chiamata asincrona a WS Microarea
				new Task(
					() => this.ChargeTracker
						.Track(loginId, userID, amountCharge, amountActivation, vat, totalAmount))
					.Start();
			}
		}

		public string GetCadastralCode(string city, out int error)
		{
			return plSvc.GetCadastralCode(city, out error);
		}

		public IEstimateCharge GetEstimateCharge(string token, decimal amountCharge)
		{
			IUserCredentials credentials = new UserCredentials() { TokenAuth = token };
			return plSvc.GetEstimateCharge((UserCredentials)credentials, amountCharge);
		}

		public LegalInfos GetLegalInfos(string language)
		{
			return plSvc.GetLegalInfos(language);
		}
	}
}
