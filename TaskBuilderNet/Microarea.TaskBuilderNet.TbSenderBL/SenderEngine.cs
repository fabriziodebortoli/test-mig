using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using Microarea.TaskBuilderNet.Interfaces;
using Microarea.TaskBuilderNet.TbSenderBL.postalite.api;
using Microarea.TaskBuilderNet.TbSenderBL.PostaLite;
using System.Threading;
using System.Globalization;
using Microarea.TaskBuilderNet.TbSenderBL.Exceptions;
using Microarea.TaskBuilderNet.Core.Generic;
using System.Text;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Microarea.TaskBuilderNet.TbSenderBL
{
	public class SenderEngine
	{
		readonly IPostaLiteService plSvc;
		readonly ICredentialsProvider credentialsProvider;
		readonly IPostaLiteSettingsProvider postaLiteSettingsProvider;
		readonly IDiagnostic diagnostic;
		readonly string company;

		private object tickLocker = new object();
		DateTime lastCreditMsg;

		//-------------------------------------------------------------------------------
		public SenderEngine
			(
			IPostaLiteService plSvc, 
			ICredentialsProvider credentialsProvider, 
			IPostaLiteSettingsProvider postaLiteSettingsProvider, 
			IDiagnostic diagnostic,
			string company)
		{
			this.plSvc = plSvc;
			this.credentialsProvider = credentialsProvider;
			this.postaLiteSettingsProvider = postaLiteSettingsProvider;
			this.diagnostic = diagnostic;
			this.company = company;
		}

		//---------------------------------------------------------------------
		public IInvalidDocumentNotifier InvalidDocumentNotifier { get; set; }

		//---------------------------------------------------------------------
		public void UploadSingleLot(int lotID)
		{
			TB_MsgLots lot = TB_MsgLots.GetLot(lotID, this.company);
			UploadSingleLot(lot);
		}

		public void UploadSingleLot(TB_MsgLots lot, 
			IUserCredentials credentials = null,
			ICreditState creditState = null)
		{
			if (lot.HasAnAlreadyUploadedStatus())
				throw new EnvelopeAlreadySentException(lot, company);
			if (credentials == null)
				credentials = GetCredentials();
			if (credentials == null)
				throw new NoSubscriptionDataException(company);
			if (credentials.Error < 0)
				throw new PostaLiteErrorException(credentials.Error, company);

			if (creditState == null)
				creditState = plSvc.GetCreditState(credentials);
			if (creditState.Error < 0)
				throw new PostaLiteErrorException(creditState.Error, company);
			if (creditState.CodeState != (int)EnumCreditStatus.Abilitato)
			{
				if (creditState.CodeState == (int)EnumCreditStatus.Sospeso)
					throw new UserSuspendedException(company);
				//if (creditState.CodeState == (int)EnumCreditStatus.NonAttivo)
					throw new UserNonActiveException(company);
			}
			if (creditState.Credit == 0)
				throw new CreditException(LocalizedStrings.CreditExceptionCreditZeroNoUploadSingle, this.company);
			if (creditState.Credit < (decimal)lot.TotalAmount)
				throw new CreditException(LocalizedStrings.CreditExceptionCreditInsufficientNoUploadSingle, this.company);

			ILotInfo lotInfo = lot.GetLotInfo(this.company, this.postaLiteSettingsProvider);
			IFileMessage fileMsg = MsgHelper.GetFileMessage(lot, this.company, this.postaLiteSettingsProvider);

			// l'upload può durare un po', meglio marcare come "untouchable"
			lot.Status = (int)LotStatus.Uploading;
			lot.SaveChanges(this.company);

			// invoke Web method
			ISentLot sentLot = plSvc.SendLot(credentials, lotInfo, fileMsg);

			// persist return values
			lot.ErrorExt = sentLot.Error;
			if (lot.ErrorExt >= 0) // sennò mette tutto a zero
			{
				lot.Status = (int)LotStatus.Uploaded; //TODOLUCA
				lot.IdExt = sentLot.IdLot;
				lot.DeliveryType = (int)lotInfo.TypeDelivery;
				lot.PostageAmount = (double)sentLot.AmountPostage;
				lot.PrintAmount = (double)sentLot.AmountPrint;
				lot.TotalAmount = (double)sentLot.Total;
				lot.BoolIncongruous = sentLot.Incongruous;
				lot.TotalPages = lotInfo.TotalPages; // pdf pages, not paper pages
			}
			else
			{
				lot.Status = (int)LotStatus.Invalid;
			}

			lot.SaveChanges(this.company);

			if (sentLot.Error < 0)
			{
				string errorMessage;
				string htmlMessage = null;

				if (sentLot.Error == -56 // Invalid document
					&& sentLot.Details != null && sentLot.Details.Length != 0)
				{
					// TODO cosa? 
					// eccezione x balloon? però se fossi in un ciclo lo interromperei, meglio farlo altrove
					// ma è bello fare un balloon all'utente che non ci può fare nulla?
					// Al momento preferisco scrivere solo sull'EventLog

					string head = string.Format(CultureInfo.InvariantCulture,
						LocalizedStrings.InvalidDocumentMessageHeadMask,
						lot.LotID);
					StringBuilder sbHtml = new StringBuilder();
					StringBuilder sb = new StringBuilder();
					sb.AppendLine(head);
					sbHtml.Append("<p>");
					sbHtml.AppendLine(head);
					sbHtml.Append("</p>");
					sb.AppendLine();
					foreach (string line in sentLot.Details)
					{
						sb.Append("- ");
						sb.AppendLine(line);
						sbHtml.Append("<li>");
						sbHtml.Append(line);
						sbHtml.Append("</li>");
					}
					errorMessage = sb.ToString();
					htmlMessage = sbHtml.ToString();

					try // nel dubbio, non voglio bloccare
					{
						diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Error, errorMessage);
					}
					catch (Exception ex)
					{
						Debug.Write(ex.ToString()); // do nothing, non voglio bloccare un eventuale ciclo (al max ci metto un breakpoint)
					}

					try // nel dubbio, non voglio bloccare
					{
						// TODO spedisci asynch a ws microarea tracker (idea di Germano)
						
						// TODO per sicurezza anche la chiamata a login manager la farei non bloccante
						string loginID = credentials.Login;
						string userID = LoginManagerConnector.GetUserInfoID(); // TODO rimuovi static!!!!
						int lotID = lot.LotID;

						if (this.InvalidDocumentNotifier != null)
						{
							//DateTime chargeTime = dateTimeProvider.Now;
							// chiamata asincrona a WS Microarea
							new Task(
								() => this.InvalidDocumentNotifier
									.LogInvalidDocumentFormat(loginID, userID, lotID, sentLot.Details))
								.Start();
						}
					}
					catch (Exception ex)
					{
						Debug.Write(ex.ToString()); // do nothing, non voglio bloccare un eventuale ciclo (al max ci metto un breakpoint)
					}
				}
				else
				{
					errorMessage = string.Format(CultureInfo.InvariantCulture,
						LocalizedStrings.SentLotErrorButNotInvalidDocumentMask,
						lot.LotID, ErrorMessages.GetErrorMessage(sentLot.Error));
				}

				try // non dovrebbe servire a nulla, perché invoco asincrono, ma ormai non credo più a nulla
				{
					// chiamata asincrona a login manager per balloon
					string bTxt = htmlMessage != null ? htmlMessage : errorMessage;
					new Task(
						() => this.SendBalloonedText(bTxt))
						.Start();
				}
				catch (Exception ex)
				{
					Debug.Write(ex.ToString()); // do nothing, non voglio bloccare un eventuale ciclo (al max ci metto un breakpoint)
				}
			}
		}

		private void SendBalloonedText(string text)
		{
			// balloon
			try
			{
				LoginManagerConnector.SendBalloon(text);
			}
			catch (Exception inEx)
			{
				using (new CultureByCompany(CultureInfo.GetCultureInfo("en")))
					this.diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Error, inEx.ToString()); // rifaccio apposta la ToString() perché per la BCL è localizzata
			}
		}

		//-------------------------------------------------------------------------------
		public void UpdateSentLotsStatus()
		{
			IUserCredentials credentials = GetCredentials();
			if (credentials == null)
				throw new NoSubscriptionDataException(company);
			if (credentials.Error < 0)
				throw new PostaLiteErrorException(credentials.Error, company);

			UpdateAllSentLotsStatus(credentials);
		}

		//-------------------------------------------------------------------------------
		public void GetLotCostEstimate(int lotID)
		{
			// prepare data
			TB_MsgLots lot = TB_MsgLots.GetLot(lotID, this.company);
			if (lot.HasAnAlreadyUploadedStatus())
				throw new EnvelopeAlreadySentException(lot, company);
			IUserCredentials credentials = GetCredentials();
			if (credentials == null)
				throw new NoSubscriptionDataException(company);
			if (credentials.Error < 0)
				throw new PostaLiteErrorException(credentials.Error, company);
			ILotInfo lotInfo = lot.GetLotInfo(this.company, this.postaLiteSettingsProvider);
			ILotInfo[] lotInfos = new ILotInfo[] { lotInfo };

			// invoke Web method
			IEstimate[] estimates = plSvc.GetLotCostEstimate(credentials, lotInfos);

			// persist return values
			IEstimate estimate = estimates[0];
			CopyEstimateIntoLot(estimate, lot);
			lot.SaveChanges(this.company);
		}

		//---------------------------------------------------------------------
		private IUserCredentials GetCredentials()
		{
			IUserCredentials credentials = this.credentialsProvider.GetCredentials(this.company);
			return credentials;
		}

		//---------------------------------------------------------------------
		private void CheckAndUpdateLots(IUserCredentials credentials, ICreditState creditState)
		{
			decimal credit = creditState.Credit;
			List<TB_MsgLots> lots = TB_MsgLots.GetLotsToUpload(this.company);
			foreach (TB_MsgLots current in lots)
			{
				if (credit < (decimal)current.TotalAmount)
				{
					// - log in diagnostics che alcuni messaggi schedulati non sono partiti per mancanza di credito
					// - se siamo in interattivo, messaggio esplicito all'utente
					// lanciando un'ApplicationException, questa sarà gestita nel modo opportuno
					string msg = string.Format(CultureInfo.CurrentCulture, LocalizedStrings.CreditExceptionCreditInsufficientMultiUploadMask, this.company);
					throw new CreditException(msg, this.company);
					// uso eccezione ad hoc da saper riconoscere da programma,
					// così possiamo inviare un balloon
				}
				UploadSingleLot(current, credentials, creditState);
				credit -= (decimal)current.TotalAmount;
			}
		}

		//---------------------------------------------------------------------
		public void Tick() // invocato dal timer, non interattivo: trap eccezioni e log
		{
			using (new CultureByCompany(this.company))
			try
			{
				IUserCredentials credentials = GetCredentials();
				if (credentials == null || string.IsNullOrEmpty(credentials.TokenAuth))
					return;
				// devo fare una chiamata a WS per capire se utente è buono
				ICreditState creditState = plSvc.GetCreditState(credentials);
				//if (creditState == null) // commented, non so che fare (non dovrebbe accadere), almeno mi piglio il log
				if (creditState.Error < 0)
					throw new PostaLiteErrorException(creditState.Error, company);
				if (creditState.CodeState != (int)EnumCreditStatus.Abilitato)
				{
					if (creditState.CodeState == (int)EnumCreditStatus.Sospeso)
						throw new UserSuspendedException(company);
					//if (creditState.CodeState == (int)EnumCreditStatus.NonAttivo)
					throw new UserNonActiveException(company);
				}
				//if (creditState <= 0) { } // niente, al momento non so se c'è roba da uploadare

				if (false == AllotAndUpload(uploadAlso: true, credentials: credentials, creditState: creditState))
				{
					string msg = string.Format(CultureInfo.CurrentCulture, LocalizedStrings.ConcurrentOperationInCourseMask, this.company);
					this.diagnostic.SetWarning(msg);
				}
			}
			catch (Exception ex)
			{
				TreatException(ex, this.diagnostic);
				CreditException cex = ex as CreditException;
				if (cex != null 
					&& this.lastCreditMsg < DateTime.Today) // se oggi gliene ho già mandato uno, evito
				{
					// balloon
					try
					{
						LoginManagerConnector.SendBalloon(cex.Message);
						this.lastCreditMsg = DateTime.Now;
					}
					catch (Exception inEx)
					{
						using (new CultureByCompany(CultureInfo.GetCultureInfo("en")))
							this.diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Error, inEx.ToString()); // rifaccio apposta la ToString() perché per la BCL è localizzata
					}
				}
			}
		}

		public static void TreatException(Exception ex, IDiagnostic diagnostic)
		{
			// TODO pilotare il diagnostic perché mostri il .Message e lo stack nei dettagli (come si fa?)
			// nota: l'eccezione dovrebbe essere solo loggata nell'event viewer

			// qui gli errori applicativi, nella lingua dell'utente perché devono potenzialmente
			// essere anche presentati in interattivo

			CreditException cex = ex as CreditException;
			if (cex != null)
			{
				diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Warning, ex.Message); // solo message, è applicativo
				return;
			}
			PostaLiteException plEx = ex as PostaLiteException;
			if (plEx != null)
			{
				diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Error, ex.Message); // solo message, è applicativo
				return;
			}
			TbSenderException tbsEx = ex as TbSenderException;
			if (tbsEx != null)
			{
				diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Error, ex.Message); // solo message, è applicativo
				return;
			}
			ApplicationException appEx = ex as ApplicationException;
			if (appEx != null)
			{
				diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Information, ex.ToString());
				return;
			}

			// qualsiasi altra eccezione è un errore

			using (new CultureByCompany(CultureInfo.GetCultureInfo("en")))
				diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Error, ex.ToString()); // rifaccio apposta la ToString() perché per la BCL è localizzata
		}

		//---------------------------------------------------------------------
		public bool AllotAndUpload
			(
			bool uploadAlso = true,  // al momento se è false c'è un utente interattivo
			IUserCredentials credentials = null,  // lo valorizzo con chiamata a WS solo quando strettamente necessario!
			ICreditState creditState = null // lo valorizzo con chiamata a WS solo quando strettamente necessario!
			)
		{
			if (false == Monitor.TryEnter(tickLocker))
				return false;
			try
			{
				// nota: potrei non essere ancora sottoscritto ma voler allottare lo stesso
				List<TB_MsgLots> changedLots = AllotMessagesUnsafe();

				// qualsiasi altra azione successiva richiede il dialogo con il WS di Postalite, 
				// pertanto comincio a controllare se posso farlo

				if (credentials == null)
					credentials = GetCredentials();
				if (credentials == null)
					throw new TbSenderException(LocalizedStrings.NoAccessCredentialsSoNoUpload, company);
				if (credentials.Error < 0)
					throw new PostaLiteErrorException(credentials.Error, company);

				// il credit state mi dice anche lo stato di attivazione dell'utente, mi serve comunque

				// uno credit state con un problema (es. utente sospeso, o credito zero)
				// NON deve impedirmi di poter chiedere la stima e lo stato di dei lotti

				// prova ad uploadare i lotti la cui scadenza è passata (anche da allottamenti precedenti)
				if (uploadAlso)
				{
					if (creditState == null)
						creditState = plSvc.GetCreditState(credentials);
					if (creditState != null // in questo caso le credenziali sono valide
						&& creditState.CodeState == (int)EnumCreditStatus.Abilitato
						&& creditState.Error >= 0
						&& creditState.Credit > 0) // non so se basta, ma almeno ci provo
						// aggiorna anche i costi con i valori consuntivi
						CheckAndUpdateLots(credentials, creditState); // lancia eccezione se il credito si esaurisce
				}

				// Chiede lo stato di tutti i lotti uploaded che risultano ancora non consegnati
				// Sono compresi anche i lotti spediti in chiamate precedenti
				UpdateAllSentLotsStatus(credentials);

				// se non ho fatto upload, e alcuni lotti sono cambiati, chiedo al WS di postalite di darmene la stima
				// TODO potrei farlo su tutti quelli con amount zero (un'eccezione potrebbe avere impedito di chiederlo)
				if (//false == uploadAlso && // commento, sennò gli allottati non uploadati non hanno preventivo (alla peggio chiedo preventivo e poi uploado)
					changedLots.Count != 0)
				{
					// prepara elenco lotinfo dei lotti modificati
					List<ILotInfo> lotInfos = new List<ILotInfo>(changedLots.Count);
					foreach (TB_MsgLots lot in changedLots)
					{
						ILotInfo lotInfo = lot.GetLotInfo(company, this.postaLiteSettingsProvider);
						lotInfos.Add(lotInfo);
					}

					// chiede in bulk lo stato dei lotti modificate e aggiorna i lotti corrispondenti
					IEstimate[] estimates = plSvc.GetLotCostEstimate(credentials, lotInfos.ToArray());
					UpdateLotsEstimates(changedLots, estimates);
				}

				// voglio capire se il credito è sufficiente per spedire tutti i lotti pending
				List<TB_MsgLots> unsentLots;
				using (var db = ConnectionHelper.GetPostaLiteIntegrationEntities(company))
				{
					unsentLots = GetUnsentLots(db);
				}
				if (unsentLots.Count != 0)
				{
					if (creditState == null)
						creditState = plSvc.GetCreditState(credentials);

					// non devo sommare solo i nuovi, ma tutti i lotti da spedire!
					decimal totEst = (decimal)unsentLots.Sum(x => x.TotalAmount);
					
					if (creditState != null) // in questo caso le credenziali sono valide
					{
						if (creditState.CodeState != (int)EnumCreditStatus.Abilitato)
							throw new CreditException(LocalizedStrings.CreditExceptionMsgsQueuedButUserNonActivated, company);

						if (creditState.Error < 0)
							throw new PostaLiteErrorException(creditState.Error, company);

						if (creditState.Credit < totEst) // non so se basta, ma almeno ci provo
							throw new CreditException(LocalizedStrings.CreditExceptionCreditInsufficientAfterEstimate, company);

						// se comunque il credito è sotto una certa soglia, lancio un'eccezione per avvertire
						PostaliteSettings param = postaLiteSettingsProvider.GetSettings(company);
						decimal threashold = (decimal)param.CreditLimit; // soglia minima
						if (creditState.Credit < threashold)
						{
							string msg = string.Format(CultureInfo.CurrentCulture,
								LocalizedStrings.CreditExceptionCreditBelowThreshold, creditState.Credit, threashold);
							throw new CreditException(msg, company);
						}
					}
				}
			}
			finally
			{
				Monitor.Exit(tickLocker);
			}
			return true;
		}

		//---------------------------------------------------------------------
		private void UpdateAllSentLotsStatus(IUserCredentials credentials)
		{
			// ora chiedo anche lo stato di tutti i lotti uploaded che risultano ancora non consegnati
			using (var db = ConnectionHelper.GetPostaLiteIntegrationEntities(company))
			// non uso transazione apposta
			{
				List<TB_MsgLots> sentPending = TB_MsgLots.GetUploadedAndNotDeliveredLots(db);
				if (sentPending.Count != 0)
				{
					List<int> lotsID = new List<int>(sentPending.Count);
					foreach (TB_MsgLots sntLot in sentPending)
						lotsID.Add(sntLot.IdExt);

					ILotState[] lotsStatusEx = plSvc.GetLotState(credentials, lotsID.ToArray());
					foreach (ILotState lotState in lotsStatusEx)
					{
						if (lotState == null) // non dovrebbe capitare mai, a meno di errori lato postalite
							continue;
						if (lotState.Id == 0) // da documentazione, in caso di errore (ma chiedo ci sia l'id vero)
							continue;
						TB_MsgLots sntLot = sentPending.Find(x => x.IdExt == lotState.Id);
						if (sntLot == null) // non dovrebbe capitare mai, a meno di errori lato postalite
							continue;
						sntLot.SetExternalState(lotState);
					}
					db.SaveChanges();
				}
			}
		}

		//---------------------------------------------------------------------
		public bool AllotMessages() // da web method: c'è un utente interattivo
		{
			return AllotAndUpload(uploadAlso: false);
		}

		//-------------------------------------------------------------------------------
		public List<TB_MsgLots> AllotMessagesUnsafe()
		{
			Dictionary<int, TB_MsgLots> changedLots = new Dictionary<int, TB_MsgLots>();
			PostaliteSettings param = postaLiteSettingsProvider.GetSettings(company);
			string defCountry = param.DefaultCountry;
			if (string.IsNullOrWhiteSpace(defCountry))
				defCountry = "Italia"; // ultima spiaggia

			using (var db = ConnectionHelper.GetPostaLiteIntegrationEntities(company))
			using (TransactionScope ts = new TransactionScope())
			{
				//TODOLUCA
				List<TB_MsgLots> lots = GetUnsentLots(db);

				// prende tutti i messaggi cui non è ancora stato assegnato un lotto
				var msgs = (from p in db.TB_MsgQueue
							where p.LotId == 0 || p.LotId == null // tb assegna zero
							select p)
							.ToList(); // forzo la chiusura del datareader

				foreach (TB_MsgQueue msg in msgs)
				{
					// trova il lotto più oppurtuno
					TB_MsgLots lot = FindExistingAvailableLot(lots, msg, db, defCountry);
					if (lot == null)
					{
						// e se non lo trova lo crea ex-novo
						lot = TB_MsgLots.CreateLot(msg);
						db.AddToTB_MsgLots(lot);
						lot.SendAfter = param.GetSentAfterTime(msg.EnumDeliveryType); // passo il delivery perché i fax devono partire subito
						int nlPages = msg.DocPages + 1; // cover
						if (lot.IsDoubleSided)
							nlPages += 1;
						lot.TotalPages = nlPages;
						lots.Add(lot);
					}
					msg.LotId = lot.LotID; // prima del save, così l'elenco dei messaggi è aggiornato
					db.SaveChanges(); // forzo l'assegnazione di un id al nuovo lotto, e cmq aggiorno # pagine
					msg.LotId = lot.LotID; // e _anche_ dopo il save(!), che se il lotto è nuovo prima del save non ha id e il msg non sarebbe associato a un lotto
					lot.SetDescription();

					if (false == changedLots.ContainsKey(lot.LotID))
						changedLots[lot.LotID] = lot;
				}
				db.SaveChanges();
				ts.Complete();
			}
			return changedLots.Values
				.OrderBy(x => x.LotID)
				.ToList();
		}

		private static List<TB_MsgLots> GetUnsentLots(PostaLiteIntegrationEntities db)
		{
			return (
				from p in db.TB_MsgLots
				where p.Status == (int)LotStatus.Allotted// TODO prendere quelli non ancora spediti
				select p
				).ToList();
		}

		//-------------------------------------------------------------------------------
		private static TB_MsgLots FindExistingAvailableLot
			(
			List<TB_MsgLots> lots, 
			TB_MsgQueue msg, 
			PostaLiteIntegrationEntities db,
			string defCountry
			)
		{
			//if (string.IsNullOrEmpty(msg.AddresseePrimaryKey))
			//    return null; // caso in cui si vuole che il lotto sia mono-messaggio

			// prima scrematura, tolgo quelli che non hanno parametri compatibili, destinatario a parte
			List<TB_MsgLots> fLots = lots.FindAll(x =>
					x.DeliveryType == msg.DeliveryType &&
					x.PrintType == msg.PrintType &&
					x.EnumStatus == LotStatus.Allotted && // potrei averlo chiuso allottando un messaggio precedente, anche se nella collezione originale era aperto
					x.AvailablePdfSpace >= msg.DocPages);

			// il SendAfter non fa parte dei criteri di imbustamento, è calcolato solo in base all'ora per schedulare (e i fax partono immediati)

			// ora differenzio per fax o non fax, e vado per anagrafica
			TB_MsgLots lot;
			if (msg.EnumDeliveryType == Delivery.Fax)
				lot = fLots.Find(x => 
					false == string.IsNullOrEmpty(x.Fax) && // non si sa mai (in particolare per il null)
					string.Compare(x.Fax, msg.Fax, StringComparison.InvariantCultureIgnoreCase) == 0);
			else
				lot = fLots.Find(x =>
					(
					string.Compare(x.Country, msg.Country, StringComparison.InvariantCultureIgnoreCase) == 0
					// se la nazione non è riportata, vuole dire la nazione di default (di solito Italia)
					|| (string.IsNullOrEmpty(x.Country) && string.Compare(defCountry, msg.Country, StringComparison.InvariantCultureIgnoreCase) == 0)
					|| (string.IsNullOrEmpty(msg.Country) && string.Compare(defCountry, x.Country, StringComparison.InvariantCultureIgnoreCase) == 0)
					) && 
					string.Compare(x.County, msg.County, StringComparison.InvariantCultureIgnoreCase) == 0 &&
					string.Compare(x.City, msg.City, StringComparison.InvariantCultureIgnoreCase) == 0 &&
					string.Compare(x.Zip, msg.Zip, StringComparison.InvariantCultureIgnoreCase) == 0 &&
					string.Compare(x.Address, msg.Address, StringComparison.InvariantCultureIgnoreCase) == 0);

			if (lot == null)
				return null;

			int maxPaperPages = TB_MsgLots.MaxPaperPages;
			int pdfLotPages = MsgHelper.CalculateLotPdfPages(lot, db);

			bool addShims = lot.IsDoubleSided; // Fronte-retro

			int totalPdfPages = MsgHelper.AddMessagePagesToCount(addShims, pdfLotPages, msg);

			int actPaperPages = addShims
				? (int)Math.Ceiling(((double)totalPdfPages) / 2)
				: totalPdfPages;

			if (actPaperPages > maxPaperPages)
				return null; // non ci sta, occorre creare un nuovo lotto

			if (actPaperPages == maxPaperPages)
				lot.EnumStatus = LotStatus.Closed;

			lot.TotalPages = totalPdfPages;

			return lot;
		}

		//---------------------------------------------------------------------
		public bool CreateSingleMessageLot(string company, int msgId, bool sendImmediately)
		{
			if (false == Monitor.TryEnter(tickLocker))
				return false;
			try
			{
				return CreateSingleMessageLot(msgId, sendImmediately, this.postaLiteSettingsProvider);
			}
			finally
			{
				Monitor.Exit(tickLocker);
			}
		}

		//-------------------------------------------------------------------------------
		public bool CreateSingleMessageLot(int msgId, bool sendImmediately, IPostaLiteSettingsProvider postaLiteSettingsProvider)
		{
			bool res = false;
			TB_MsgLots newLot = null;
			using (var db = ConnectionHelper.GetPostaLiteIntegrationEntities(company))
			using (TransactionScope ts = new TransactionScope()) 
			{
				db.Connection.Open(); // forzo l'uso di un'unica connessione perché posso avere più SaveChanges

				TB_MsgQueue msg = db.TB_MsgQueue.SingleOrDefault(p => p.MsgID == msgId);
				if (msg == null)
					return false;

				TB_MsgLots prevLot = null;
				if (msg.LotId != null && msg.LotId != 0)
					prevLot = db.TB_MsgLots.SingleOrDefault(x => x.LotID == msg.LotId);
				if (prevLot != null)
				{
					if (prevLot.HasAnAlreadyUploadedStatus())
						return false;
					int cnt = (from p in db.TB_MsgQueue
							   where p.LotId == msg.LotId
							   select p)
							  .Count();
					if (cnt == 0) // non capita, alla peggio c'è il singolo messaggio. lo lascio in omaggio a murphy
						db.DeleteObject(prevLot);
					else if (cnt == 1) // il messaggio in oggetto
						newLot = prevLot; // così evito di crearne uno nuovo
				}

				if (newLot == null) // se non sono riuscito a riciclarlo
				{
					newLot = TB_MsgLots.CreateLot(msg);
					db.AddToTB_MsgLots(newLot);
					db.SaveChanges(); // forzo assegnazione di un id al lotto
				}

				if (sendImmediately)
					newLot.SendAfter = postaLiteSettingsProvider.TimeProvider.Now;
				else
				{
					PostaliteSettings param = postaLiteSettingsProvider.GetSettings(company);
					newLot.SendAfter = param.GetSentAfterTime(msg.EnumDeliveryType); // passo il delivery perché i fax devono partire subito
				}
				msg.LotId = newLot.LotID;
				newLot.SetDescription();
				newLot.EnumStatus = LotStatus.Closed; // nessun altro messaggio deve essere allottato in esso

				db.SaveChanges();
				ts.Complete();
				res = true;
			}

			if (res && sendImmediately && newLot != null)
				this.UploadSingleLot(newLot);

			return res;
		}

		//-------------------------------------------------------------------------------
		public bool DeleteMessage(int msgId)
		{
			TB_MsgLots prevLot = null;
			using (var db = ConnectionHelper.GetPostaLiteIntegrationEntities(company))
			using (TransactionScope ts = new TransactionScope())
			{
				db.Connection.Open(); // forzo l'uso di un'unica connessione perché posso avere più SaveChanges

				TB_MsgQueue msg = db.TB_MsgQueue.SingleOrDefault(p => p.MsgID == msgId);
				if (msg == null)
					return false;

				// se è già imbustata, devo toglierla dalla busta
				if (msg.LotId != null && msg.LotId != 0)
					prevLot = db.TB_MsgLots.SingleOrDefault(x => x.LotID == msg.LotId);
				if (prevLot != null)
				{
					if (prevLot.HasAnAlreadyUploadedStatus())
						return false;
					int cnt = (from p in db.TB_MsgQueue
							   where p.LotId == msg.LotId
							   select p)
							  .Count();
					if (cnt == 0 // non capita, alla peggio c'è il singolo messaggio. lo lascio in omaggio a murphy
						|| cnt == 1) // il messaggio in oggetto
					{
						db.DeleteObject(prevLot);
						prevLot = null;
					}
				}

				db.DeleteObject(msg);
				db.SaveChanges();
				if (prevLot != null)
				{
					int pdfLotPages = MsgHelper.CalculateLotPdfPages(prevLot, db);
					prevLot.TotalPages = pdfLotPages;
					prevLot.EnumStatus = LotStatus.Allotted;
					db.SaveChanges();
				}
				ts.Complete();
			}
			if (prevLot != null)
				GetLotCostEstimate(prevLot.LotID); // lo trovo per ID perché detached

			return true;
		}

		//-------------------------------------------------------------------------------
		public bool ChangeMessageDeliveryType(int msgId, int deliveryType)
		{
			if (false == Enum.IsDefined(typeof(Delivery), deliveryType))
				return false; // nop

			return ChangeMessageProperty(msgId,
				x => x.DeliveryType == deliveryType,
				x => x.DeliveryType = deliveryType);
		}

		//-------------------------------------------------------------------------------
		public bool ChangeMessagePrintType(int msgId, int printType)
		{
			if (false == Enum.IsDefined(typeof(Printing), printType))
				return false; // nop

			return ChangeMessageProperty(msgId, 
				x => x.PrintType == printType,
				x => x.PrintType = printType);
		}

		//-------------------------------------------------------------------------------
		private delegate void SetPropertyDelegate(TB_MsgQueue msg);
		private bool ChangeMessageProperty(int msgId, Func<TB_MsgQueue, bool> isAlready, SetPropertyDelegate setPropertyDelegate)
		{
			TB_MsgLots prevLot = null;
			using (var db = ConnectionHelper.GetPostaLiteIntegrationEntities(company))
			using (TransactionScope ts = new TransactionScope())
			{
				db.Connection.Open(); // forzo l'uso di un'unica connessione perché posso avere più SaveChanges

				TB_MsgQueue msg = db.TB_MsgQueue.SingleOrDefault(p => p.MsgID == msgId);
				if (msg == null)
					return false;
				if (isAlready(msg))
				//if (msg.DeliveryType == deliveryType)
					return true; // nop

				// se è già imbustata, devo toglierla dalla busta
				if (msg.LotId != null && msg.LotId != 0)
					prevLot = db.TB_MsgLots.SingleOrDefault(x => x.LotID == msg.LotId);
				if (prevLot != null)
				{
					if (prevLot.HasAnAlreadyUploadedStatus())
						return false;
					int cnt = (from p in db.TB_MsgQueue
							   where p.LotId == msg.LotId
							   select p)
							  .Count();
					if (cnt == 0 // non capita, alla peggio c'è il singolo messaggio. lo lascio in omaggio a murphy
						|| cnt == 1) // il messaggio in oggetto
					{
						db.DeleteObject(prevLot);
						prevLot = null;
					}
				}

				//msg.DeliveryType = deliveryType;
				setPropertyDelegate(msg);
				msg.LotId = 0;
				db.SaveChanges(); // forzo assegnazione di un id al lotto
				if (prevLot != null)
				{
					int pdfLotPages = MsgHelper.CalculateLotPdfPages(prevLot, db);
					prevLot.TotalPages = pdfLotPages;
					db.SaveChanges();
				}
				ts.Complete();
			}
			if (prevLot != null) // aggiorna stima di costo lotto
				GetLotCostEstimate(prevLot.LotID); // lo trovo per ID perché detached

			return true;
		}

		//-------------------------------------------------------------------------------
		private void UpdateLotsEstimates(List<TB_MsgLots> changedLots, IEstimate[] estimates)
		{
			if (estimates == null || changedLots.Count != estimates.Length)
				throw new ArgumentException();

			using (var db = ConnectionHelper.GetPostaLiteIntegrationEntities(company))
			using (TransactionScope ts = new TransactionScope())
			{
				db.Connection.Open(); // forzo l'uso di un'unica connessione perché posso avere più SaveChanges

				for (int i = 0; i < changedLots.Count; ++i)
				{
					int lotID = changedLots[i].LotID;
					IEstimate estimate = estimates[i];
					if (estimate == null) // non è sotto mio controllo per cui non posso escluderlo
						continue; // TODO log

					// non posso  usare quello in changedLots perché siamo in una nuova connession (o posso?)
					TB_MsgLots lot = db.TB_MsgLots.SingleOrDefault(x => x.LotID == lotID);
					CopyEstimateIntoLot(estimate, lot);
				}

				db.SaveChanges();
				ts.Complete();
			}
		}

		private static void CopyEstimateIntoLot(IEstimate estimate, TB_MsgLots lot)
		{
			lot.PostageAmount = (double)estimate.AmountPostage;
			lot.PrintAmount = (double)estimate.AmountPrint;
			lot.TotalAmount = (double)estimate.Total;
			lot.ErrorExt = estimate.Error;
			if (estimate.Error < 0)
				lot.EnumStatus = LotStatus.Invalid;
			else if (lot.EnumStatus == LotStatus.Invalid)
				lot.EnumStatus = LotStatus.Closed; // l'alternativa sarebbe allotted, mi tengo sul conservativo
			lot.BoolIncongruous = estimate.Incongruous;
		}

		//-------------------------------------------------------------------------------
		public bool ReopenClosedLot(string company, int lotId)
		{
			using (var db = ConnectionHelper.GetPostaLiteIntegrationEntities(company))
			using (TransactionScope ts = new TransactionScope())
			{
				db.Connection.Open(); // forzo l'uso di un'unica connessione perché posso avere più SaveChanges

				TB_MsgLots msg = db.TB_MsgLots.SingleOrDefault(p => p.LotID == lotId);
				if (msg == null)
					return false;
				msg.EnumStatus = LotStatus.Allotted;

				db.SaveChanges();
				ts.Complete();
			}

			return true;
		}

		//-------------------------------------------------------------------------------
		public bool CloseLot(string company, int lotId)
		{
			using (var db = ConnectionHelper.GetPostaLiteIntegrationEntities(company))
			using (TransactionScope ts = new TransactionScope())
			{
				db.Connection.Open(); // forzo l'uso di un'unica connessione perché posso avere più SaveChanges

				TB_MsgLots lot = db.TB_MsgLots.SingleOrDefault(p => p.LotID == lotId);
				if (lot == null)
					return false;

				if (lot.HasAnAlreadyUploadedStatus())
					return false;

				lot.EnumStatus = LotStatus.Closed;

				db.SaveChanges();
				ts.Complete();
			}

			return true;
		}

		//-------------------------------------------------------------------------------
		public bool RemoveFromLot(string company, int msgId)
		{
			TB_MsgLots prevLot = null;
			using (var db = ConnectionHelper.GetPostaLiteIntegrationEntities(company))
			using (TransactionScope ts = new TransactionScope())
			{
				db.Connection.Open(); // forzo l'uso di un'unica connessione perché posso avere più SaveChanges

				TB_MsgQueue msg = db.TB_MsgQueue.SingleOrDefault(p => p.MsgID == msgId);
				if (msg == null)
					return false;

				if (msg.LotId != null && msg.LotId != 0)
					prevLot = db.TB_MsgLots.SingleOrDefault(x => x.LotID == msg.LotId);

				if (prevLot == null)
					return false; // omaggio a Murphy

				if (prevLot.HasAnAlreadyUploadedStatus())
					return false;
				int cnt = (from p in db.TB_MsgQueue
						   where p.LotId == msg.LotId
						   select p)
						  .Count();
				if (cnt == 0 // non capita, alla peggio c'è il singolo messaggio. lo lascio in omaggio a murphy
					|| cnt == 1) // il messaggio in oggetto
				{
					db.DeleteObject(prevLot);
					prevLot = null;
				}

				msg.LotId = 0;
				db.SaveChanges();
				if (prevLot != null)
				{
					int pdfLotPages = MsgHelper.CalculateLotPdfPages(prevLot, db);
					prevLot.TotalPages = pdfLotPages;
					prevLot.EnumStatus = LotStatus.Allotted;
					db.SaveChanges();
				}
				ts.Complete();
			}
			if (prevLot != null) // aggiorna stima di costo lotto
				GetLotCostEstimate(prevLot.LotID); // lo trovo per ID perché detached

			return true;
		}
	}
}
