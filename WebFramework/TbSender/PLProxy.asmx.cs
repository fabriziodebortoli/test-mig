using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using Microarea.TaskBuilderNet.TbSenderBL;
using Microarea.TaskBuilderNet.TbSenderBL.PostaLite;
using System.Threading.Tasks;
using Microarea.TaskBuilderNet.TbSenderBL.postalite.api;
using System.Globalization;
using Microarea.TaskBuilderNet.TbSenderBL.Exceptions;
using Microarea.TaskBuilderNet.Core.Generic;

namespace Microarea.WebServices.TbSender
{
	/// <summary>
	/// Summary description for PLProxy
	/// </summary>
	[WebService(Namespace = "http://microarea.it/TbSender/")]
	[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
	[System.ComponentModel.ToolboxItem(false)]
	// To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
	// [System.Web.Script.Services.ScriptService]
	public class PLProxy : System.Web.Services.WebService
	{
		// per fare funzionare EF4:
		// http://stackoverflow.com/questions/7698286/login-failed-for-user-iis-apppool-asp-net-v4-0
		//
		// per debuggare su win64:
		// usare IIS come web server
		//-------------------------------------------------------------------------------

		//---------------------------------------------------------------------
		[WebMethod]
		public string Subscribe
			(
			string company,  //è l'azienda su cui sta lavorando l'utente che effettua la sottoscrizione
			string companyName, //è la ragione sociale del sottoscrittore del contratto postalite
			string city,
			string address,
			string zipCode,
			string county,
			string country,
			string prefixTelephoneNumber,
			string telephoneNumber,
			string vatNumber,
			string fiscalCode,
			string eMail,
			string legalCode,
			string activityCode,
			string areaCode, //codice catastale
			bool privateEntity,
			string privateEntityOption,
			string senderCompanyName,
			string senderCity,
			string senderAddress,
			string senderZipCode,
			string senderCounty,
			string senderCountry, // serve???
			string senderVatNumber,
			string senderFiscalCode,
			string senderEMail,
			string senderLegalCode,
			string senderActivityCode,
			out string loginId,
			out string errorMessage
			)
		{
			errorMessage = null;
			loginId = null;
			using (new CultureByCompany(company))
			try
			{
				// Indipendente dalla company
				// Non posso usare tabella d'appoggio, perché i dati di sottoscrizione sono su file di settings

				// purtroppo tb non può usare web methods con parametri complessi, per cui dobbiamo mettere 
				// come parametri tutti i campi esplicitamente.

				// creo qui l'oggetto raccoglitore; sarebbe più pulito farlo fuori dal web method, 
				// ma scriverei il doppio e otterrei codice meno manutenibile.
				ISubscriberInfo subscriberInfo = new SubscriberInfo()
				{
					Subscriber_SurNameCompanyName = companyName,
					Subscriber_City = city,
					Subscriber_Address = address,
					Subscriber_ZipCode = zipCode,
					Subscriber_County = county,
					Subscriber_State = country,
					Subscriber_VatNumber = vatNumber,
					Subscriber_FiscalCode = fiscalCode,
					Subscriber_EMail = eMail,
					Subscriber_PrefixPhone = prefixTelephoneNumber,
					Subscriber_TelephoneNumber = telephoneNumber,
					Subscriber_LegalCode = legalCode,
					Subscriber_ActivityCode = activityCode,
					Subscriber_AreaCode = areaCode,

					Subscriber_PrivateEntity = privateEntity,
					Sender_Option = privateEntityOption,

					Sender_LegalCode = senderLegalCode,
					Sender_ActivityCode = senderActivityCode,
					//Sender_AreaCode

					Sender_CompanyName = senderCompanyName,
					Sender_City = senderCity,
					Sender_Address = senderAddress,
					Sender_ZipCode = senderZipCode,
					Sender_CountyCode = senderCounty,
					Sender_Country = senderCountry,
					Sender_VatNumber = senderVatNumber,
					Sender_FiscalCode = senderFiscalCode,
					Sender_Email = senderEMail,
					//Sender_TelephoneNumber = senderPrefixTelephoneNumber + senderTelephoneNumber, // facoltativo, attento a non concatenare dei null
					//Sender_FaxNumber = senderfaxNumber, // facoltativo
				};
				
				IUserCredentials cred = Global.SubscriptionEngine.Subscribe(subscriberInfo);

				if (cred.Error < 0)
					errorMessage = ErrorMessages.GetErrorMessage(cred.Error);

				loginId = cred.Login;

				return Crypto.Encrypt(cred.TokenAuth);
			}
			catch (Exception ex)
			{
				TreatException(ex);
				errorMessage = ex.Message;
				return null;
			}
		}


        /// <summary>
        /// Metodo richiamato dalla console per riavviare il servizio
        /// </summary>
        /// <returns></returns>
        //---------------------------------------------------------------------
        [WebMethod]
        public bool Init()
        {
			lock (typeof(InitLocker))
			{
				try
				{
					//TimerManager timer = Application["Timer"] as TimerManager;
					//HttpRuntime.UnloadAppDomain();
					//if (timer != null)
					//    timer.Stop(); // se lo facessi prima rischierebbe di non ripartire, se non lo faccio rischierebbe di rimanere vivo in garbage

					// TODO auto wake-ati!!!!!!!!!!
					// sennò se non c'è il login manager, non riparte!

					TimerManager timer = Application["Timer"] as TimerManager;
					timer.Stop();
					Application.Clear();
					Application["Timer"] = timer;
					timer.Start();
					Global.appHolder.Application = Application; // ho dovuto fare così, non so perché ma teneva una copia in cui spariva solo il timer!
					Global.SettingsProvider.Refresh();
				}
				catch (Exception ex)
				{
					System.Diagnostics.Debug.WriteLine(ex.Message); // mi serve solo in debug, in rel preferisco non faccia nulla (al momento)
					return false;
				}
				return true;
			}
        }
		private class InitLocker { }

        //---------------------------------------------------------------------
        [WebMethod]
        public bool IsAlive()
        {
            return true;       
        }

		//-------------------------------------------------------------------------------
		[WebMethod]
		public int Charge
			(
			string company, 
			string loginId,
			string token, 
			string fileContentBase64, 
			double amountCharge, 
			double vat, 
			double amountActivation, 
			double totalAmount,
			out double credit, 
			out DateTime expiryDate, 
			out string errorMessage
			)
		{
			errorMessage = null;
			credit = 0;
			expiryDate = DateTime.MinValue;
			using (new CultureByCompany(company))
			try
			{
				// Indipendente dalla company
				// Non posso usare tabella d'appoggio, perché i dati di sottoscrizione/credito sono su file di settings

				string decryptedToken = Crypto.Decrypt(token);
				ICreditState creditState = Global.SubscriptionEngine.Charge(decryptedToken, fileContentBase64,
					(decimal)amountCharge, vat, (decimal)amountActivation, (decimal)totalAmount);

				if (creditState.Error < 0)
					throw new PostaLiteErrorException(creditState.Error, company: null);

				Global.SubscriptionEngine.NotifyAfterCharge(loginId,
					(decimal)amountCharge, vat, (decimal)amountActivation, (decimal)totalAmount);

				credit = (double)creditState.Credit;
				expiryDate = creditState.ExpiryDate;
				return creditState.CodeState;
			}
			catch (Exception ex)
			{
				TreatException(ex);
				errorMessage = ex.Message;
				return 0;
			}
		}

		//-------------------------------------------------------------------------------
		[WebMethod]
		public string GetCadastralCode(string company, string city, out string errorMessage)
		{
			errorMessage = null;
			using (new CultureByCompany(company))
				try
				{
					int error;
					string code = Global.SubscriptionEngine.GetCadastralCode(city, out error);
					if (error < 0)
						throw new PostaLiteErrorException(error, company: null);
					return code;
				}
				catch (Exception ex)
				{
					TreatException(ex);
					//errorMessage = ex.Message;
					return string.Empty;
				}
		}

        //-------------------------------------------------------------------------------
        [WebMethod]
		public bool GetLegalInfos
			(
			string company,
			string language,
			out string restrictiveClauses,
			out string iban,
			out string bankName,
			out string beneficiary,
			out string privacyPolicy,
			out string priceListUrl,
			out string termsOfUse,
			out string generalConditionsCharge,
			out string transparancyObligations,
			out string cadastralPageUrl,
			out double minimumRecharge,
			out string errorMessage
			)
		{
			errorMessage = null;
			minimumRecharge = 150;
			using (new CultureByCompany(company))
			{
				LegalInfos li = Global.SubscriptionEngine.GetLegalInfos(language);
				restrictiveClauses = li.RestrictiveClauses; //LocalizedStrings.RestrictiveClause;
				iban = li.Iban;
				bankName = li.BankName;
				beneficiary = li.Beneficiary;
				privacyPolicy = li.PrivacyPolicyUrl;
				termsOfUse = li.TermsOfUseUrl;
				priceListUrl = li.PriceListUrl;
				generalConditionsCharge =  li.GeneralConditionsCharge; //LocalizedStrings.AcceptRecharge;
				transparancyObligations = li.TransparencyObligations; //LocalizedStrings.Law136;
				cadastralPageUrl = li.CadastralCodeUrl;
			}
			return true;
		}

		//-------------------------------------------------------------------------------
		[WebMethod]
		public bool GetEstimateCharge
			(
			string company, 
			string login, // serve?
			string token,
			double amountCharge,
			out double vat,
			out double amountActivation,
			out double totalAmount,
			out string iban,
			out string nameBank,
			out string beneficiary,
			out string errorMessage
			)
		{
			amountActivation = 0;
			vat = 0;
			totalAmount = 0;
			iban = null;
			nameBank = null;
			beneficiary = null;
			errorMessage = null;

			using (new CultureByCompany(company))
				try
				{
					string decryptedToken = Crypto.Decrypt(token);
					IEstimateCharge estCharge = Global.SubscriptionEngine.GetEstimateCharge(decryptedToken, (decimal)amountCharge);
					if (estCharge.Error < 0)
						throw new PostaLiteErrorException(estCharge.Error, company: null);
					amountActivation = (double)estCharge.AmountActivation;
					vat = estCharge.Vat;
					totalAmount = (double)estCharge.TotalAmount;
					iban = estCharge.Iban;
					nameBank = estCharge.BankName;
					beneficiary = estCharge.Beneficiary;
					return true;
				}
				catch (Exception ex)
				{
					TreatException(ex);
					errorMessage = ex.Message;
					return false;
				}
		}

		//-------------------------------------------------------------------------------
		[WebMethod]
		public bool UploadSingleLot(string company, int lotID, out string errorMessage)
		{
			errorMessage = null;
			using (new CultureByCompany(company))
			try
			{
				SenderEngine engine = GetEngine(company);
				if (engine == null)
					throw new CompanyNotEnabledException(company: company);
				engine.UploadSingleLot(lotID);
			}
			catch (Exception ex)
			{
				TreatException(ex);
				errorMessage = ex.Message;
				return false;
			}
			return true;
		}

		//-------------------------------------------------------------------------------
		[WebMethod]
		public bool UpdateSentLotsStatus(string company, bool async, out string errorMessage)
		{
			errorMessage = null;
			if (async)
			{
				new Task(() => UpdateSentLotsStatus(company)).Start();
				return true;
			}
			else
				return UpdateSentLotsStatus(company) != null;
		}
		private string UpdateSentLotsStatus(string company) // ritorna il messaggio d'errore, o null
		{
			using (new CultureByCompany(company))
			try
			{
				SenderEngine engine = GetEngine(company);
				if (engine == null)
					throw new CompanyNotEnabledException(company: company);

				engine.UpdateSentLotsStatus();
				return null; // è brutto, ma così è facile da invocare con un task asincrono
			}
			catch (Exception ex)
			{
				TreatException(ex);
				return ex.Message;
			}
		}

		//-------------------------------------------------------------------------------
		[WebMethod]
		public bool GetLotCostEstimate(string company, int lotID, out string errorMessage)
		{
			using (new CultureByCompany(company))
			try
			{
				errorMessage = null;
				SenderEngine engine = GetEngine(company);
				if (engine == null)
					throw new CompanyNotEnabledException(company: company);
				// il lotto non deve essere ancora stato uploadato (stato Allotted)
				engine.GetLotCostEstimate(lotID);
			}
			catch (Exception ex)
			{
				TreatException(ex);
				errorMessage = ex.Message;
				return false;
			}
			return true;
		}

		//-------------------------------------------------------------------------------
		[WebMethod]
		public void DoTick() // TEMP, solo per test
		{
			//using (new CultureByCompany(company)) // commented: non ho una company da cui ricavare una lingua UI
			Global.Tick();
		}

		//-------------------------------------------------------------------------------
		[WebMethod]
		public void WakeUp() // per fare partire la prima volta (visto che non è un service)
		{
			// nop
		}

		//-------------------------------------------------------------------------------
		[WebMethod]
		public void RefreshSettings() //necessario per far rileggere i settings cambiati dalla maschera di tb
		{
			//using (new CultureByCompany(company)) // commented: non ho una company da cui ricavare una lingua UI
			Global.SettingsProvider.Refresh();
		}

		//-------------------------------------------------------------------------------
		/// <summary>
		/// Effettua la login date le credenziali dell'account: ritorna il token di autenticazione 
		/// e in out tutti i dati memorizzati nella sottoscrizione
		/// </summary>
		[WebMethod]
		public string Login
			(
			string company,
			string loginID,
			string password,
			out string surNameCompanyName,
			out string city,
			out string address,
			out string zipCode,
			out string county,
			out string state,
			out string prefixTelephoneNumber,
			out string telephoneNumber,
			out string vatNumber,
			out string fiscalCode,
			out string eMail,
			out string legalCode,
			out string activityCode,
			out string areaCode,
			out bool privateEntity,
			out string privateEntityOption,
			out string senderCompanyName,
			out string senderCity,
			out string senderAddress,
			out string senderZipCode,
			out string senderCounty,

			out string senderCountry,
			out string senderVatNumber,
			out string senderFiscalCode,
			out string senderEMail,
			out string senderLegalCode,
			out string senderActivityCode,
			out string errorMessage
			)
		{
			errorMessage = null;
			surNameCompanyName = null;
			city = null;
			address = null;
			zipCode = null;
			county = null;
			state = null;
			prefixTelephoneNumber = null;
			telephoneNumber = null;
			vatNumber = null;
			fiscalCode = null;
			eMail = null;
			legalCode = null;
			activityCode = null;
			areaCode = null;
			privateEntity = false;
			privateEntityOption = null;
			senderCompanyName = null;
			senderCity = null;
			senderAddress = null;
			senderZipCode = null;
			senderCounty = null;
			senderCountry = null;
			senderVatNumber = null;
			senderFiscalCode = null;
			senderEMail = null;
			senderLegalCode = null;
			senderActivityCode = null;
			errorMessage = null;
			
			using (new CultureByCompany(company)) // commented: non ho una company da cui ricavare una lingua UI
			try
			{
				ISubscriberInfo subscriberInfo;
				IUserCredentials uc = Global.SubscriptionEngine.Login(loginID, password, out subscriberInfo);
				string token =  uc != null ? Crypto.Encrypt(uc.TokenAuth) : null;
				if (uc != null && uc.Error != 0)
				{
					errorMessage = ErrorMessages.GetErrorMessage(uc.Error); // me lo becco nella lingua che capita...
					return null;
				}
				surNameCompanyName = subscriberInfo.Subscriber_SurNameCompanyName;
				city = subscriberInfo.Subscriber_City;
				address = subscriberInfo.Subscriber_Address;
				zipCode = subscriberInfo.Subscriber_ZipCode;
				county = subscriberInfo.Subscriber_County;
				state = subscriberInfo.Subscriber_State;
				prefixTelephoneNumber = subscriberInfo.Subscriber_PrefixPhone;
				telephoneNumber = subscriberInfo.Subscriber_TelephoneNumber;
				vatNumber = subscriberInfo.Subscriber_VatNumber;
				fiscalCode = subscriberInfo.Subscriber_FiscalCode;
				eMail = subscriberInfo.Subscriber_EMail;
				legalCode = subscriberInfo.Subscriber_LegalCode;
				activityCode = subscriberInfo.Subscriber_ActivityCode;
				areaCode = subscriberInfo.Subscriber_AreaCode;
				privateEntity = subscriberInfo.Subscriber_PrivateEntity;
				privateEntityOption = subscriberInfo.Sender_Option;
			
				senderCompanyName =  subscriberInfo.Sender_CompanyName;
				senderCity =  subscriberInfo.Sender_City;
				senderAddress =   subscriberInfo.Sender_Address;
				senderZipCode =   subscriberInfo.Sender_ZipCode;
				senderCounty =   subscriberInfo.Sender_CountyCode;
				senderCountry =   subscriberInfo.Sender_Country;
				senderVatNumber =   subscriberInfo.Sender_VatNumber;
				senderFiscalCode = subscriberInfo.Sender_FiscalCode;
				senderEMail =   subscriberInfo.Sender_Email;
				senderLegalCode =   subscriberInfo.Sender_LegalCode;
				senderActivityCode =   subscriberInfo.Sender_ActivityCode;
				return token;
			}
			catch (Exception ex)
			{
				TreatException(ex);
				errorMessage = ex.Message;
				surNameCompanyName = null;
				city = null;
				address = null;
				zipCode = null;
				county = null;
				state = null;
				telephoneNumber = null;
				eMail = null;
				return null;
			}
		}

		//-------------------------------------------------------------------------------
		private SenderEngine GetEngine(string company)
		{
			Dictionary<string, SenderEngine> enginePool = Global.EnginePool; // invocare fuori da lock!
			lock (typeof(Microarea.WebServices.TbSender.Global.SenderEngineLocker))
			{
				return Global.GetOrAddCompanyEngineUnlocked(company, enginePool, Global.SubscriptionEngine);
			}
		}

		//-------------------------------------------------------------------------------
		[WebMethod]
		public double GetCreditState(string company, string login, string token, out int codeState, out DateTime expiryDate, out string errorMessage)
		{
			errorMessage = null;
			expiryDate = DateTime.MinValue;
			using (new CultureByCompany(company)) 
			try
			{
				IUserCredentials credentials = new UserCredentials();
				credentials.Login = login;
				credentials.TokenAuth = Crypto.Decrypt(token);

				ICreditState creditState = Global.SubscriptionEngine.GetCreditState(credentials);

				if (creditState.Error < 0)
					throw new PostaLiteErrorException(creditState.Error, company: null);

				codeState = creditState.CodeState;
				expiryDate = creditState.ExpiryDate;
				return (double)creditState.Credit;
			}
			catch (Exception ex)
			{
				TreatException(ex);
				errorMessage = ex.Message;
				codeState = 0;
				return 0.0D;
			}
		}

		//-------------------------------------------------------------------------------
		[WebMethod]
		public bool AllotMessages(string company, out string errorMessage)
		{
			errorMessage = null;
			using (new CultureByCompany(company))
			try
			{
				SenderEngine engine = GetEngine(company);
				if (engine == null)
					throw new CompanyNotEnabledException(company: company);
				return engine.AllotMessages();
			}
			catch (Exception ex)
			{
				TreatException(ex);
				errorMessage = ex.Message;
				return false;
			}
		}

		//-------------------------------------------------------------------------------
		[WebMethod]
		public bool ReopenClosedLot(string company, int lotId, out string errorMessage)
		{
			errorMessage = null;
			using (new CultureByCompany(company))
			try
			{
				SenderEngine engine = GetEngine(company);
				if (engine == null)
					throw new CompanyNotEnabledException(company: company);
				return engine.ReopenClosedLot(company, lotId);
			}
			catch (Exception ex)
			{
				TreatException(ex);
				errorMessage = ex.Message;
				return false;
			}
		}

		//-------------------------------------------------------------------------------
		[WebMethod]
		public bool RemoveFromLot(string company, int msgId, out string errorMessage)
		{
			errorMessage = null;
			using (new CultureByCompany(company))
				try
				{
					SenderEngine engine = GetEngine(company);
					if (engine == null)
						throw new CompanyNotEnabledException(company: company);
					return engine.RemoveFromLot(company, msgId);
				}
				catch (Exception ex)
				{
					TreatException(ex);
					errorMessage = ex.Message;
					return false;
				}
		}

		//-------------------------------------------------------------------------------
		[WebMethod]
		public bool CloseLot(string company, int lotId, out string errorMessage)
		{
			errorMessage = null;
			using (new CultureByCompany(company))
				try
				{
					SenderEngine engine = GetEngine(company);
					if (engine == null)
						throw new CompanyNotEnabledException(company: company);
					return engine.CloseLot(company, lotId);
				}
				catch (Exception ex)
				{
					TreatException(ex);
					errorMessage = ex.Message;
					return false;
				}
		}

		//-------------------------------------------------------------------------------
		[WebMethod]
		public bool CreateSingleMessageLot(string company, int msgId, bool sendImmediately, out string errorMessage)
		{
			errorMessage = null;
			using (new CultureByCompany(company))
			try
			{
				SenderEngine engine = GetEngine(company);
				if (engine == null)
					throw new CompanyNotEnabledException(company: company);
				return engine.CreateSingleMessageLot(company, msgId, sendImmediately);
			}
			catch (Exception ex)
			{
				TreatException(ex);
				errorMessage = ex.Message;
				return false;
			}
		}

		//-------------------------------------------------------------------------------
		[WebMethod]
		public bool DeleteMessage(string company, int msgId, out string errorMessage)
		{
			errorMessage = null;
			using (new CultureByCompany(company))
			try
			{
				SenderEngine engine = GetEngine(company);
				if (engine == null)
					throw new CompanyNotEnabledException(company: company);
				return engine.DeleteMessage(msgId);
			}
			catch (Exception ex)
			{
				TreatException(ex);
				errorMessage = ex.Message;
				return false;
			}
		}

		//-------------------------------------------------------------------------------
		[WebMethod]
		public bool ChangeMessageDeliveryType(string company, int msgId, int deliveryType, out string errorMessage)
		{
			errorMessage = null;
			using (new CultureByCompany(company))
			try
			{
				SenderEngine engine = GetEngine(company);
				if (engine == null)
					throw new CompanyNotEnabledException(company: company);
				return engine.ChangeMessageDeliveryType(msgId, deliveryType);
			}
			catch (Exception ex)
			{
				TreatException(ex);
				errorMessage = ex.Message;
				return false;
			}
		}

		//-------------------------------------------------------------------------------
		[WebMethod]
		public bool ChangeMessagePrintType(string company, int msgId, int printType, out string errorMessage)
		{
			errorMessage = null;
			using (new CultureByCompany(company))
			try
			{
				SenderEngine engine = GetEngine(company);
				if (engine == null)
					throw new CompanyNotEnabledException(company: company);
				return engine.ChangeMessagePrintType(msgId, printType);
			}
			catch (Exception ex)
			{
				TreatException(ex);
				errorMessage = ex.Message;
				return false;
			}
		}

		//-------------------------------------------------------------------------------
		private static void TreatException(Exception ex)
		{
			SenderEngine.TreatException(ex, Global.Diagnostic);
		}
	}
}
