using System;
using System.Collections.Generic;
using System.Configuration;

using Microarea.TaskBuilderNet.Core.DiagnosticManager;
using Microarea.TaskBuilderNet.Core.WebServicesWrapper;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.TaskBuilderNet.TbSenderBL.SOS
{
	///<summary>
	/// Interfacciamento con il web service per il servizio di conservazione sostitutiva Zucchetti
	///</summary>
	//================================================================================
	public class SOSEngine
	{
		private it.sostitutivazucchetti.infinity.ws_as_servizioService sosService = new it.sostitutivazucchetti.infinity.ws_as_servizioService();
		private const string sosResponses = "SOSResponses";

		//--------------------------------------------------------------------------------
		public string Url
		{
			get { return sosService.Url; }
			set { sosService.Url = value; }
		}

		//--------------------------------------------------------------------------------
		public SOSEngine()
		{
			// leggo dal web.config del TbSender il timeout
			int msTimeout = 20; // default 20 minuti
			if (!int.TryParse(ConfigurationManager.AppSettings["SOSWSTimeout"], out msTimeout))
				msTimeout = 20;

			sosService.Timeout = ((msTimeout == 0) ? 20 : msTimeout) * 60000;
		}

		///<summary>
		/// Il Web Service di generazione ticket riceve in ingresso il Codice Conservatore, 
		/// il Codice Cliente (coppia assegnata univocamente dal Sistema durante il processo di 
		/// configurazione iniziale) e la password associata al Cliente, ed esegue l’autenticazione e, 
		/// in caso positivo, comunica il ticket.
		///</summary>
		//--------------------------------------------------------------------------------
		public bool Singolo(string codiceConservatore, string codiceCliente, string password, out Ticket ticket)
		{
			ticket = null;

			SOSLogWriter.WriteLogEntry(NameSolverStrings.AllCompanies, string.Format("Inizio chiamata web-method singolo(codiceconservatore: {0}, codicecliente: {1}, password: {2})",
			codiceConservatore, codiceCliente, password), "Singolo", logName: sosResponses);

			string ticketResponse = sosService.singolo(codiceConservatore, codiceCliente, password);
			SOSLogWriter.AppendText(NameSolverStrings.AllCompanies, string.Format("Fine chiamata web-method singolo (Risposta: {0})", ticketResponse), sosResponses);
			if (string.IsNullOrWhiteSpace(ticketResponse))
				return false;

			bool result = SOSParser.ParseTicket(ticketResponse, out ticket);
			if (!result)
				SOSLogWriter.AppendText(NameSolverStrings.AllCompanies, string.Format("Parse della risposta concluso con {0}", result ? "successo" : "errori"), sosResponses);
			return result;
		}

		///<summary>
		/// Il Web Service di generazione sequenze di ticket accetta in ingresso il Codice Conservatore, 
		/// il Codice Cliente e la Password associata al Cliente come per il ticket singolo, 
		/// richiede inoltre la numerosità della sequenza attraverso il parametro NumeroChunk. 
		/// Se questa numerosità non è nota (è valorizzata a 0 [zero]), permette al cliente di trasmettere 
		/// le dimensioni dell'archivio zip in byte in modo che il Web Service stesso calcoli la numerosità dei TicketChunk. 
		/// Il parametro NumeroChunk è prioritario rispetto alla Dimensione pertanto qualora entrambi siano valorizzati e 
		/// maggiori di 0 (zero) sarà considerato solo il NumeroChunk.
		/// 
		/// QUESTA RICHIESTA E' UTILIZZATA ESCLUSIVAMENTE PER LA FUNZIONALITA' SPEDIZIONE FRAMMENTATA
		///</summary>
		//--------------------------------------------------------------------------------
		public bool Sequenza(string codiceConservatore, string codiceCliente, string password, int numeroChunk, int dimensione, out Ticket ticket)
		{
			ticket = null;

			SOSLogWriter.WriteLogEntry(NameSolverStrings.AllCompanies, string.Format("Inizio chiamata web-method sequenza(codiceconservatore: {0}, codicecliente: {1}, password: {2}, numeroChunk: {3}, dimensione: {4})",
			codiceConservatore, codiceCliente, password, numeroChunk.ToString(), dimensione.ToString()), "Sequenza", logName: sosResponses);

			string sequenzaResponse = sosService.sequenza(codiceConservatore, codiceCliente, password, numeroChunk, dimensione);
			SOSLogWriter.AppendText(NameSolverStrings.AllCompanies, string.Format("Fine chiamata web-method sequenza (Risposta: {0})", sequenzaResponse), sosResponses);
			if (string.IsNullOrWhiteSpace(sequenzaResponse))
				return false;

			bool result = SOSParser.ParseTicket(sequenzaResponse, out ticket);
			if (!result)
				SOSLogWriter.AppendText(NameSolverStrings.AllCompanies, string.Format("Parse della risposta concluso con {0}", result ? "successo" : "errori"), sosResponses);
			return result;
		}

		///<summary>
		/// Il Web Service di Spedizione Semplice è consigliato per spedire archivi zip di dimensioni non superiori a 1 Mb.
		///</summary>
		//--------------------------------------------------------------------------------
		public bool Semplice(string ticketGUID, string nomeFileZip, Byte[] contenutoFileZip, out RispostaSpedizione spedizioneSemplice) 
		{
			spedizioneSemplice = null;
			SOSLogWriter.WriteLogEntry(NameSolverStrings.AllCompanies, string.Format("Inizio chiamata web-method semplice(ticketGUID: {0}, nomeFileZip: {1})", ticketGUID, nomeFileZip),
			"Semplice", logName: sosResponses);

			string spedizioneResponse = string.Empty;
			try
			{
				spedizioneResponse = sosService.semplice(ticketGUID, nomeFileZip, contenutoFileZip);
            }
			catch (Exception e)
			{
				SOSLogWriter.AppendText(NameSolverStrings.AllCompanies, string.Format("Eccezione nella chiamata al web-method semplice (Eccezione: {0})", e.ToString()), sosResponses);
			}

			SOSLogWriter.AppendText(NameSolverStrings.AllCompanies, string.Format("Fine chiamata web-method semplice (Risposta: {0})", spedizioneResponse), sosResponses);
			if (string.IsNullOrWhiteSpace(spedizioneResponse))
				return false;

			bool result = SOSParser.ParseSpedizioneSemplice(spedizioneResponse, out spedizioneSemplice);
			if (!result)
				SOSLogWriter.AppendText(NameSolverStrings.AllCompanies, string.Format("Parse della risposta concluso con {0}", result ? "successo" : "errori"), sosResponses);
			return result;
		}

		///<summary>
		/// Il Web Service di Spedizione Frammentata permette di spedire archivi zip di dimensioni elevate.
		///</summary>
		//--------------------------------------------------------------------------------
		public bool Frammentata
			(
			string ticketGUID, 
			string ticketChunkGUID, 
			string nomeFileZip, 
			Byte[] contenutoBlocco,
			out RispostaSpedizione spedizioneFrammentata
			) 
		{
			spedizioneFrammentata = null;

			SOSLogWriter.WriteLogEntry(NameSolverStrings.AllCompanies,
			string.Format("Inizio chiamata web-method frammentata(ticketGUID: {0}, ticketChunkGUID: {1}, nomeFileZip: {2})", ticketGUID, ticketChunkGUID, nomeFileZip),
			"Frammentata", logName: sosResponses);

			string spedizione = string.Empty;

			try
			{
				spedizione = sosService.frammentata(ticketGUID, ticketChunkGUID, nomeFileZip, contenutoBlocco);
			}
			catch (Exception e)
			{
				SOSLogWriter.AppendText(NameSolverStrings.AllCompanies, string.Format("Eccezione nella chiamata al web-method frammentata (Eccezione: {0})", e.ToString()), sosResponses);
			}

			SOSLogWriter.AppendText(NameSolverStrings.AllCompanies, string.Format("Fine chiamata web-method frammentata (Risposta: {0})", spedizione), sosResponses);
			if (string.IsNullOrWhiteSpace(spedizione))
				return false;

			bool result = SOSParser.ParseSpedizioneFrammentata(spedizione, out spedizioneFrammentata);
			if (!result)
				SOSLogWriter.AppendText(NameSolverStrings.AllCompanies, string.Format("Parse della risposta concluso con {0}", result ? "successo" : "errori"), sosResponses);
			return result;
		}

		///<summary>
		/// E' un metodo che permette di accedere alle funzionalità di richiesta dello stato di una spedizione.
		/// Utilizzato per conoscere lo stato attuale di una Spedizione.
		/// La risposta del Web Service è composta dall'eventuale data di presa in carico, dall'eventuale
		/// data di archiviazione documentale e un codice che indica lo stato del trattamento della spedizione.
		///</summary>
		//--------------------------------------------------------------------------------
		public bool StatoSpedizione
			(
			string conservatore, 
			string soggetto, 
			string ticketGUID, 
			string nomeFileZip,
			out StatoSpedizione statoSpedizione
			)
		{
			statoSpedizione = null;

			SOSLogWriter.WriteLogEntry(NameSolverStrings.AllCompanies,
			string.Format("Inizio chiamata web-method statospedizione(conservatore: {0}, soggetto: {1}, ticketGUID: {2}, nomeFileZip: {3})", conservatore, soggetto, ticketGUID, nomeFileZip),
			"StatoSpedizione", logName: sosResponses);

			string stato = sosService.statospedizione(conservatore, soggetto, ticketGUID, nomeFileZip);
			SOSLogWriter.AppendText(NameSolverStrings.AllCompanies, string.Format("Fine chiamata web-method statospedizione (Risposta: {0})", stato), sosResponses);
			if (string.IsNullOrWhiteSpace(stato))
				return false;

			bool result = SOSParser.ParseStatoSpedizione(stato, out statoSpedizione);
			if (!result)
				SOSLogWriter.AppendText(NameSolverStrings.AllCompanies, string.Format("Parse della risposta concluso con {0}", result ? "successo" : "errori"), sosResponses);
			return result;
		}

		///<summary>
		/// E' un metodo che permette di accedere alle funzionalità di richiesta dei dati dei documenti di una spedizione ricevuti dal sistema.
		/// Utilizzato per eseguire una verifica dei documenti appartenenti ad una data spedizione.
		/// Il Web Service risponde con l'eventuale data di presa in carico e con l’elenco di ogni documento che appartiene alla spedizione. 
		/// Comunica inoltre tutte le informazioni dei documenti interessati veicolate attraverso le Chiavi di Descrizione.
		///</summary>
		//--------------------------------------------------------------------------------
		public bool EsitoSpedizione
			(
			string conservatore, 
			string soggetto, 
			string ticketGUID, 
			string nomeFileZip, 
			string codiceClasseDocumentale, 
			out EsitoSpedizione esitoSpedizione
			)
		{
			esitoSpedizione = null;

			SOSLogWriter.WriteLogEntry(NameSolverStrings.AllCompanies,
			string.Format("Inizio chiamata web-method esitospedizione(conservatore: {0}, soggetto: {1}, ticketGUID: {2}, nomeFileZip: {3}, codiceClasseDocumentale: {4})", conservatore, soggetto, ticketGUID, nomeFileZip, codiceClasseDocumentale),
			"EsitoSpedizione", logName: sosResponses);

			string esito = sosService.esitospedizione(conservatore, soggetto, ticketGUID, nomeFileZip, codiceClasseDocumentale);
			SOSLogWriter.AppendText(NameSolverStrings.AllCompanies, string.Format("Fine chiamata web-method esitospedizione (Risposta: {0})", esito), sosResponses);
			if (string.IsNullOrWhiteSpace(esito))
				return false;

			bool result = SOSParser.ParseEsitoSpedizione(esito, out esitoSpedizione);
			if (!result)
				SOSLogWriter.AppendText(NameSolverStrings.AllCompanies, string.Format("Parse della risposta concluso con {0}", result ? "successo" : "errori"), sosResponses);
			return result;
		}

		/// <summary>
		/// E' un metodo che permette di chiedere l'elenco delle Classi Documentali definite nel sistema e di competenza del Cliente.
		///</summary>
		//--------------------------------------------------------------------------------
		public bool ElencoClassiDocumentali(string conservatore, string soggetto, string ticketGUID, out List<ClasseDocumentale> classeDocList)
		{
			classeDocList = null;

			string elenco = sosService.elencoclassidocumentali(conservatore, soggetto, ticketGUID);
			if (string.IsNullOrWhiteSpace(elenco))
				return false;

			bool result = SOSParser.ParseElencoClassiDocumentali(elenco, out classeDocList);
			if (!result)
				SOSLogWriter.WriteLogEntry(NameSolverStrings.AllCompanies, string.Format("Parse risposta web-method elencoclassidocumentali ('{0}') concluso con {1}", elenco, result ? "successo" : "errori"), sosResponses);
			return result;
		}

		///<summary>
		/// E' un metodo che permette di accedere all'elenco delle Chiavi di Descrizione delle Classi Documentali.
		///</summary>
		//--------------------------------------------------------------------------------
		public bool ElencoChiaviClasseDocumentale
			(
			string conservatore, 
			string soggetto, 
			string ticketGUID, 
			string codiceClasseDocumentale, 
			out ClasseDocumentale docClass
			)
		{
			docClass = null;

			string chiavi = sosService.elencochiaviclassedocumentale(conservatore, soggetto, ticketGUID, codiceClasseDocumentale);
			if (string.IsNullOrWhiteSpace(chiavi))
				return false;

			bool result = SOSParser.ParseElencoChiaviClasseDocumentale(chiavi, out docClass);
			if (!result)
				SOSLogWriter.WriteLogEntry(NameSolverStrings.AllCompanies, string.Format("Parse risposta web-method elencochiaviclassedocumentale ('{0}') concluso con {1}", chiavi, result ? "successo" : "errori"), sosResponses);
			return result;
		}
	} 
}
