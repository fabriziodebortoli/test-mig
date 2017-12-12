using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net;
using System.Web.Services.Protocols;

using Microarea.TaskBuilderNet.Core.DiagnosticManager;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.TaskBuilderNet.Core.WebServicesWrapper
{
	# region Enumerativi
	///<summary>
	/// Enumerativi
	///</summary>
	//================================================================================
	public enum NaturaSpedizione
	{
		EMPTY,	// uso interno Microarea: valore generico nel caso in cui la risposta da SOS tornasse vuota
		SPDSMP, // spedizione semplice
		SPDFRM,	// spedizione frammentata
		SPDFTP	// spedizione ftp
	}

	///<summary>
	/// Enumerativo generico utilizzato per le risposte ritornate sia dalla spedizione
	/// semplice che da quella frammentata
	///</summary>
	//================================================================================
	public enum RispostaSpedizioneEnum
	{
		OK,		// spedizione avvenuta con successo
		KO		// errore di spedizione
	}

	//================================================================================
	public enum StatoSpedizioneEnum
	{
		EMPTY,			//  0: uso interno Microarea: valore generico nel caso in cui la risposta da SOS tornasse vuota
		SPDOP,			//  1: preso in carico (significa che la richiesta e' arrivata al servizio SOS ed e' in coda, in attesa di essere lavorata)
		SPDNOP,			//  2: non preso in carico, l'archivio dovrà essere rispedito
		SPDEXIST,		//  3: il file è una copia di un archivio già spedito
		SPDNEX,			//  4: la spedizione indicata non esiste
		SPDALLOK,		//  5: il contenuto della spedizione è stato acquisito
		SPDALLKO,		//  6: il contenuto della spedizione non è stato acquisito, deve essere rivisto e spedito nuovamente (dettaglio con ws_as_servizio.esitospedizione)
		SPDOKWERR,		//  7: il contenuto della spedizione è stato acquisito parzialmente, alcuni doc presentano errori, è necessario correggere i doc errati e rispedirli (dettaglio con ws_as_servizio.esitospedizione)
		SPDSUBUNAUTH,	//  8: il Soggetto in questione non è autorizzato a recuperare i dati
		SPDCONUNEX,		//  9: il Conservatore non è definito a Sistema
		SPDCUSTUNEX,    // 10: il Cliente del Conservatore non è definito a Sistema
        SPDOK           // 11: stato intermedio interno della SOS che intercorre tra la fine della spedizione e l'inserimento effettivo del pacchetto dentro il db della SOS
	}

	//================================================================================
	public enum StatoDocumento
	{
		EMPTY,			//  0: uso interno Microarea: valore generico nel caso in cui la risposta da SOS tornasse vuota
		IDLE,			//  1: uso interno Microarea per il SOSDocument appena inserito in fase di allegato
		TOSEND,			//  2: uso interno Microarea per il SOSDocument pronto per essere spedito (impostato dal SOS Send)
		WAITING,		//  3: uso interno Microarea per il SOSDocument in elaborazione per la spedizione (impostato dall'EASync)
		SENT,			//  4: uso interno Microarea per il SOSDocument spedito e in attesa di stato/esito (impostato dall'EASync)
		TORESEND,		//  5: uso interno Microarea per il SOSDocument non spedito per stato SPDNOP dell'envelope (ovvero e' fallita l'envelope per qualche motivo)

		DOCTEMP,		//  6: stato provvisorio: il documento e' stato importato nel db della SOS ma non è ancora entrato nel giro della SOS. 
						//     N.B. se il doc non fosse stato importato (per qualche errore) non sarebbe neppure presente dentro il db e arriverebbe solo l'email al cliente
		DOCSTD,			//  7: questo stato si ottiene DOPO che il cliente dal sito appone lo firma
		DOCRDY,			//  8: il documento è stato spostato in un lotto. Quest’azione viene eseguita dal cliente oppure da Zucchetti, 
						//	   dipende dal contratto sottoscritto, cioè da chi risulta responsabile della conservazione. 
						//     Nel 99% dei casi viene demandata tale operazione a Zucchetti. In questo caso la chiamata dell’esito spedizione ritorna anche il lottoid.
		DOCSIGN,		//  9: il lotto è stato chiuso e il doc inviato in conservazione (non e' piu' modificabile)
		DOCREP,			// 10: documento sostituito perche' ho inviato in SOS un altro documento con le stesse chiavi di descrizione obbligatorie

		// gli stati seguenti difficilmente saranno replicabili nel nostro ambiente
		DOCKO,			// 11: non acquisito
		DOCREPSIGN,		// 12: documento acquisito che sostituisce un documento già conservato (non e' gestito)
		DOCADDTOEVAL	// 13: documento integrativo acquisito. Da confermare dal cliente se deve essere conservato o meno.

		/* addendum sullo StatoDocumento. 
		 * Fintanto che il documento ha questi stati: DOCTEMP / DOCSTD / DOCRDY
		 * il documento puo' essere sostituito, tramite una spedizione successiva.
		 * Se il documento ha lo stato DOCSIGN non si puo' piu' modificare, essendo gia' inserito in sostitutiva (di fatto lo stato DOCREPSIGN non si dovrebbe mai verificare)
		*/
	}
	
	//================================================================================
	public enum CodiceErrore
	{
		EMPTY,				// uso interno Microarea: valore generico nel caso in cui la risposta da SOS tornasse vuota
		
		// errori segnalati nella spedizione semplice/frammentata
		SPEDIZIONEGIAPRESENTE,
		IMPOSSIBILESALVARE,
		
		// inizio errori bloccanti
		DOCERRNORECORD,		// corrispondenza del documento nel file delle Chiavi di Descrizione mancante
		DOCERRKEYMISSING,	// una o più Chiavi di Descrizione mancanti
		DOCERRKEYWRONG,		// una o più Chiavi di Descrizione errate
		DOCERRNODOC,		// file documento mancante a fronte della presenza dello stesso nel file delle Chiavi di Descrizione
		DOCERRHASHWRONG,	// hash del file documento errato
		
		// inizio errori non bloccanti (warning)
		DOCWRNEXP,			// documento scaduto
		DOCWRNNUM			// numerazione del documento non congruente
	}
	# endregion

	# region Classi oggetti SOS
	//================================================================================
	public class Ticket
	{
		public string Guid = string.Empty;
		public List<TicketChunk> Chunks = new List<TicketChunk>();

		//--------------------------------------------------------------------------------
		public Ticket() { }
	}

	//================================================================================
	public class TicketChunk
	{
		public int Posizione = 0;
		public string Guid = string.Empty;

		//--------------------------------------------------------------------------------
		public TicketChunk() { }

		//--------------------------------------------------------------------------------
		public TicketChunk(int posizione, string guid)
		{
			this.Posizione = posizione;
			this.Guid = guid;
		}
	}

	///<summary>
	/// Tipo di ritorno della spedizione semplice o frammentata
	/// Il field Sequenza e' valorizzato con un numero positivo solo se si riferisce alla frammentata
	///</summary>
	//================================================================================
	public class RispostaSpedizione
	{
		public RispostaSpedizioneEnum Risposta = RispostaSpedizioneEnum.OK;
		public string CodiceSpedizione = string.Empty;
		public int Sequenza = -1; // posizione del ticketchunk
		
		public CodiceErrore Errore = CodiceErrore.EMPTY; // per la spedizione semplice/frammentata 
		// i valori possibili sono: SPEDIZIONEGIAPRESENTE, IMPOSSIBILESALVARE

		//--------------------------------------------------------------------------------
		public RispostaSpedizione() { }
	}

	//================================================================================
	public class ClasseDocumentale
	{
		public string CodiceClasseDocumentale = string.Empty;
		public string Descrizione = string.Empty;

		public List<Chiave> Chiavi = new List<Chiave>();

		//--------------------------------------------------------------------------------
		public ClasseDocumentale() { }

		//--------------------------------------------------------------------------------
		public ClasseDocumentale(string codice, string descrizione)
		{
			this.CodiceClasseDocumentale = codice;
			this.Descrizione = descrizione;
		}
	}

	//================================================================================
	public class Spedizione
	{
		public string CodiceSpedizione = string.Empty;
		public NaturaSpedizione NaturaSpedizione = NaturaSpedizione.SPDSMP;
		public DateTime DataPresaInCarico = DateTime.Now;

		public string CodiceClasseDocumentale = string.Empty;
		// una spedizione nuova nasce come archivio NON preso in carico, cosi se si verifica un errore di invio riesco
		// a re-impostare i documenti in essa presenti con uno stato di RESEND
		public StatoSpedizioneEnum StatoSpedizione = StatoSpedizioneEnum.SPDNOP; 

		//--------------------------------------------------------------------------------
		public Spedizione() { }
	}

	//================================================================================
	public class StatoSpedizione : Spedizione
	{
		public Contenuto Contenuto = new Contenuto(); //sottonodo

		//--------------------------------------------------------------------------------
		public StatoSpedizione() { }
	}

	//================================================================================
	public class Contenuto
	{
		public DateTime DataArchiviazioneDocumentale = DateTime.Now;

		//--------------------------------------------------------------------------------
		public Contenuto() { }
	}

	//================================================================================
	public class EsitoSpedizione : Spedizione
	{
		public List<Documento> Documenti = new List<Documento>(); //sottonodi

		//--------------------------------------------------------------------------------
		public EsitoSpedizione() { }

		//--------------------------------------------------------------------------------
		public Documento GetDocumentByAttachmentId(int attachmentId)
		{
			foreach (Documento doc in Documenti)
				foreach (Chiave key in doc.Chiavi)
				{
					if (
						string.Compare(key.CodiceChiave, "AttachmentId", StringComparison.InvariantCultureIgnoreCase) == 0 && 
						string.Compare(key.Valore, attachmentId.ToString()) == 0
						)
						return doc;
				}

			return null;
		}
	}

	//================================================================================
	public class Documento
	{
		public DateTime DataArchiviazioneDocumentale = DateTime.Now;
		public DateTime DataConservazioneSostitutiva = DateTime.Now;
		public string CodiceClasseDocumentale = string.Empty;

		public string NomeFileOriginale = string.Empty;
		public string CodiceAssoluto = string.Empty;
		public string LottoId = string.Empty;
		public StatoDocumento StatoDocumento = StatoDocumento.EMPTY;

		public List<Chiave> Chiavi = new List<Chiave>();
		public List<Errore> Errori = new List<Errore>();

		//--------------------------------------------------------------------------------
		public Documento() { }
	}

	//================================================================================
	public class Chiave
	{
		public string CodiceChiave = string.Empty;

		public string Tipo = string.Empty;
		public string Descrizione = string.Empty;
		public bool Fiscale = false; // 0/1
		public int Lunghezza = 0;
		public int Ordine = 0;
		public bool DataScad = false; // S/N
		public int CPRowNum = 0;
		public int ValCombo = 0;
		public bool CdObblig = false; // S/N 

		public string Valore = string.Empty; // utilizzato in esitospedizione

		//--------------------------------------------------------------------------------
		public Chiave() { }
	}

	//================================================================================
	public class Errore
	{
		public CodiceErrore Codice = CodiceErrore.EMPTY;
		public string Descrizione = string.Empty; // decidere se decodificare la descrizione

		private bool isWarning = false;
		//--------------------------------------------------------------------------------
		public bool IsWarning // se false si tratta di un errore bloccante, se true e' un warning
		{
			get
			{
				switch (Codice)
				{
					case CodiceErrore.DOCERRNORECORD:
					case CodiceErrore.DOCERRKEYMISSING:
					case CodiceErrore.DOCERRKEYWRONG:
					case CodiceErrore.DOCERRNODOC:
					case CodiceErrore.DOCERRHASHWRONG:
					
					case CodiceErrore.SPEDIZIONEGIAPRESENTE:
					case CodiceErrore.IMPOSSIBILESALVARE:
						return false;

					case CodiceErrore.DOCWRNEXP:
					case CodiceErrore.DOCWRNNUM:
					case CodiceErrore.EMPTY: 
					return true;
				}

				return isWarning;
			}
		}

		//--------------------------------------------------------------------------------
		public Errore() { }
	}
	# endregion

	/// <summary>
	/// Wrapper per l'utilizzo del SOSProxy 
	/// (pagina SOSProxy.asmx in WebFramework\TbSender)
	/// </summary>
	//============================================================================
	public class SOSProxyWrapper
	{
		private Diagnostic sosDiagnostic = new Diagnostic("SOSProxyWrapper");
		private sosProxy.SOSProxy sosProxy = new sosProxy.SOSProxy();

		// ha la conoscenza del conservatore, cliente o soggetto, password
		private string codiceConservatore = string.Empty;
		private string codiceCliente = string.Empty;
		private string codiceSoggetto = string.Empty;
		private string password = string.Empty;

		private const string library = "Microarea.TaskBuilderNet.Core.WebServicesWrapper";

		// properties
		//---------------------------------------------------------------------------
		public Diagnostic SosDiagnostic { get { return sosDiagnostic; } }

		/// <summary>
		/// Costruttore
		/// </summary>
		//---------------------------------------------------------------------------
		public SOSProxyWrapper(string sosUrl)
		{
			sosProxy.Url = sosUrl;

			// leggo dal web.config del TbSender il timeout
			int msTimeout = 20; // default 20 minuti
			if (!int.TryParse(ConfigurationManager.AppSettings["SOSWSTimeout"], out msTimeout))
				msTimeout = 20;

			sosProxy.Timeout = ((msTimeout == 0) ? 20 : msTimeout) * 60000;
		}

		//---------------------------------------------------------------------------
		public void Init(string conservatore, string soggetto, string cliente, string pwd, string url)
		{
			codiceConservatore = conservatore;
			codiceCliente = cliente;
			codiceSoggetto = soggetto;
			password = pwd;

			// devo richiamare un metodo InitUrl per inizializzare l'url
			sosProxy.Init(url);
		}

		//--------------------------------------------------------------------------------
		public bool Singolo(out Ticket ticket)
		{
			bool result = false;
			ticket = null;

			try
			{
				result = sosProxy.Singolo(codiceConservatore, codiceCliente, password, out ticket);			
				
				if (!result)
					SosDiagnostic.Set(DiagnosticType.Error, WebServicesWrapperStrings.UnableToRetrieveSOSTkt);
			}
			catch (SoapException se)
			{
				ExtendedInfo ei = new ExtendedInfo();
				ei.Add(WebServicesWrapperStrings.Description, se.Message);
				ei.Add(WebServicesWrapperStrings.Source, se.Source);
				ei.Add(WebServicesWrapperStrings.Function, "Singolo");
				ei.Add(WebServicesWrapperStrings.Library, library);
				SosDiagnostic.Set(DiagnosticType.Error, string.Format(WebServicesWrapperStrings.InvokeWebMethodFailed, "Singolo"), ei);
			}
			catch (WebException we)
			{
				ExtendedInfo ei = new ExtendedInfo();
				ei.Add(WebServicesWrapperStrings.Description, we.Message);
				ei.Add(WebServicesWrapperStrings.Source, we.Source);
				ei.Add(WebServicesWrapperStrings.Function, "Singolo");
				ei.Add(WebServicesWrapperStrings.Library, library);
				SosDiagnostic.Set(DiagnosticType.Error, string.Format(WebServicesWrapperStrings.InvokeWebMethodFailed, "Singolo"), ei);
			}
			catch (OutOfMemoryException ome)
			{
				ExtendedInfo ei = new ExtendedInfo();
				ei.Add(WebServicesWrapperStrings.Description, ome.Message);
				ei.Add(WebServicesWrapperStrings.Source, ome.Source);
				ei.Add(WebServicesWrapperStrings.Function, "Singolo");
				ei.Add(WebServicesWrapperStrings.Library, library);
				SosDiagnostic.Set(DiagnosticType.Error, string.Format(WebServicesWrapperStrings.InvokeWebMethodFailed, "Singolo"), ei);
			}
			catch (Exception e)
			{
				ExtendedInfo ei = new ExtendedInfo();
				ei.Add(WebServicesWrapperStrings.Description, e.Message);
				ei.Add(WebServicesWrapperStrings.Source, e.Source);
				ei.Add(WebServicesWrapperStrings.Function, "Singolo");
				ei.Add(WebServicesWrapperStrings.Library, library);
				SosDiagnostic.Set(DiagnosticType.Error, string.Format(WebServicesWrapperStrings.InvokeWebMethodFailed, "Singolo"), ei);
			}

			return result;
		}

		//--------------------------------------------------------------------------------
		public bool Sequenza(int numeroChunk, int dimensione, out Ticket ticket)
		{
			ticket = null;

			try
			{
				return sosProxy.Sequenza(codiceConservatore, codiceCliente, password, numeroChunk, dimensione, out ticket);
			}
			catch (SoapException se)
			{
				ExtendedInfo ei = new ExtendedInfo();
				ei.Add(WebServicesWrapperStrings.Description, se.Message);
				ei.Add(WebServicesWrapperStrings.Source, se.Source);
				ei.Add(WebServicesWrapperStrings.Function, "Sequenza");
				ei.Add(WebServicesWrapperStrings.Library, library);
				SosDiagnostic.Set(DiagnosticType.Error, string.Format(WebServicesWrapperStrings.InvokeWebMethodFailed, "Sequenza"), ei);
			}
			catch (WebException we)
			{
				ExtendedInfo ei = new ExtendedInfo();
				ei.Add(WebServicesWrapperStrings.Description, we.Message);
				ei.Add(WebServicesWrapperStrings.Source, we.Source);
				ei.Add(WebServicesWrapperStrings.Function, "Sequenza");
				ei.Add(WebServicesWrapperStrings.Library, library);
				SosDiagnostic.Set(DiagnosticType.Error, string.Format(WebServicesWrapperStrings.InvokeWebMethodFailed, "Sequenza"), ei);
			}
			catch (OutOfMemoryException ome)
			{
				ExtendedInfo ei = new ExtendedInfo();
				ei.Add(WebServicesWrapperStrings.Description, ome.Message);
				ei.Add(WebServicesWrapperStrings.Source, ome.Source);
				ei.Add(WebServicesWrapperStrings.Function, "Sequenza");
				ei.Add(WebServicesWrapperStrings.Library, library);
				SosDiagnostic.Set(DiagnosticType.Error, string.Format(WebServicesWrapperStrings.InvokeWebMethodFailed, "Sequenza"), ei);
			}
			catch (Exception e)
			{
				ExtendedInfo ei = new ExtendedInfo();
				ei.Add(WebServicesWrapperStrings.Description, e.Message);
				ei.Add(WebServicesWrapperStrings.Source, e.Source);
				ei.Add(WebServicesWrapperStrings.Function, "Sequenza");
				ei.Add(WebServicesWrapperStrings.Library, library);
				SosDiagnostic.Set(DiagnosticType.Error, string.Format(WebServicesWrapperStrings.InvokeWebMethodFailed, "Sequenza"), ei);
			}

			return false;
		}

		///<summary>
		/// Spedizione semplice (al suo interno viene generato il ticket)
		/// <param name="nomeFileZip">nome del file di archivio che deve essere trasferito (compresa l'estensione .zip)</param>
		/// <param name="contenutoFileZip">contenuto del file di archivio (Byte[])</param>
		/// <param name="spedizioneSemplice">ritorno della RispostaSpedizione</param>
		///</summary>
		//--------------------------------------------------------------------------------
		public bool Semplice(string nomeFileZip, Byte[] contenutoFileZip, out RispostaSpedizione spedizioneSemplice)
		{
			spedizioneSemplice = null;
			try
			{
				Ticket tkt;
				if (!sosProxy.Singolo(codiceConservatore, codiceCliente, password, out tkt))
				{
					SosDiagnostic.Set(DiagnosticType.Error, WebServicesWrapperStrings.UnableToRetrieveSOSTkt);
					return false;
				}

				return sosProxy.Semplice(tkt.Guid, nomeFileZip, contenutoFileZip, out spedizioneSemplice);
			}
			catch (SoapException se)
			{
				ExtendedInfo ei = new ExtendedInfo();
				ei.Add(WebServicesWrapperStrings.Description, se.Message);
				ei.Add(WebServicesWrapperStrings.Source, se.Source);
				ei.Add(WebServicesWrapperStrings.Function, "Semplice");
				ei.Add(WebServicesWrapperStrings.Library, library);
				SosDiagnostic.Set(DiagnosticType.Error, string.Format(WebServicesWrapperStrings.InvokeWebMethodFailed, "Semplice"), ei);
			}
			catch (WebException we)
			{
				ExtendedInfo ei = new ExtendedInfo();
				ei.Add(WebServicesWrapperStrings.Description, we.Message);
				ei.Add(WebServicesWrapperStrings.Source, we.Source);
				ei.Add(WebServicesWrapperStrings.Function, "Semplice");
				ei.Add(WebServicesWrapperStrings.Library, library);
				SosDiagnostic.Set(DiagnosticType.Error, string.Format(WebServicesWrapperStrings.InvokeWebMethodFailed, "Semplice"), ei);
			}
			catch (OutOfMemoryException ome)
			{
				ExtendedInfo ei = new ExtendedInfo();
				ei.Add(WebServicesWrapperStrings.Description, ome.Message);
				ei.Add(WebServicesWrapperStrings.Source, ome.Source);
				ei.Add(WebServicesWrapperStrings.Function, "Semplice");
				ei.Add(WebServicesWrapperStrings.Library, library);
				SosDiagnostic.Set(DiagnosticType.Error, string.Format(WebServicesWrapperStrings.InvokeWebMethodFailed, "Semplice"), ei);
			}
			catch (Exception e)
			{
				ExtendedInfo ei = new ExtendedInfo();
				ei.Add(WebServicesWrapperStrings.Description, e.Message);
				ei.Add(WebServicesWrapperStrings.Source, e.Source);
				ei.Add(WebServicesWrapperStrings.Function, "Semplice");
				ei.Add(WebServicesWrapperStrings.Library, library);
				SosDiagnostic.Set(DiagnosticType.Error, string.Format(WebServicesWrapperStrings.InvokeWebMethodFailed, "Semplice"), ei);
			}

			return false;
		}

		///<summary>
		/// Spedizione frammentata (Il ticket e i ticket chunk vanno generati esternamente)
		///</summary>
		///<param name="ticketGUID">ticket guid</param>
		///<param name="ticketChunkGUID">ticket guid chunk</param>
		///<param name="nomeFileZip">nome del file di archivio che deve essere trasferito (compresa l'estensione .zip)</param>
		///<param name="contenutoBlocco">contenuto del frammento (chunk) del file di archivio (Byte[])</param>
		///<param name="spedizioneFrammentata">ritorno della RispostaSpedizione</param>
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
			try
			{
				// i ticketchunk devono essere calcolati prima di richiamare questo metodo!
				return sosProxy.Frammentata(ticketGUID, ticketChunkGUID, nomeFileZip, contenutoBlocco, out spedizioneFrammentata);
			}
			catch (SoapException se)
			{
				ExtendedInfo ei = new ExtendedInfo();
				ei.Add(WebServicesWrapperStrings.Description, se.Message);
				ei.Add(WebServicesWrapperStrings.Source, se.Source);
				ei.Add(WebServicesWrapperStrings.Function, "Frammentata");
				ei.Add(WebServicesWrapperStrings.Library, library);
				SosDiagnostic.Set(DiagnosticType.Error, string.Format(WebServicesWrapperStrings.InvokeWebMethodFailed, "Frammentata"), ei);
			}
			catch (WebException we)
			{
				ExtendedInfo ei = new ExtendedInfo();
				ei.Add(WebServicesWrapperStrings.Description, we.Message);
				ei.Add(WebServicesWrapperStrings.Source, we.Source);
				ei.Add(WebServicesWrapperStrings.Function, "Frammentata");
				ei.Add(WebServicesWrapperStrings.Library, library);
				SosDiagnostic.Set(DiagnosticType.Error, string.Format(WebServicesWrapperStrings.InvokeWebMethodFailed, "Frammentata"), ei);
			}
			catch (OutOfMemoryException ome)
			{
				ExtendedInfo ei = new ExtendedInfo();
				ei.Add(WebServicesWrapperStrings.Description, ome.Message);
				ei.Add(WebServicesWrapperStrings.Source, ome.Source);
				ei.Add(WebServicesWrapperStrings.Function, "Frammentata");
				ei.Add(WebServicesWrapperStrings.Library, library);
				SosDiagnostic.Set(DiagnosticType.Error, string.Format(WebServicesWrapperStrings.InvokeWebMethodFailed, "Frammentata"), ei);
			}
			catch (Exception e)
			{
				ExtendedInfo ei = new ExtendedInfo();
				ei.Add(WebServicesWrapperStrings.Description, e.Message);
				ei.Add(WebServicesWrapperStrings.Source, e.Source);
				ei.Add(WebServicesWrapperStrings.Function, "Frammentata");
				ei.Add(WebServicesWrapperStrings.Library, library);
				SosDiagnostic.Set(DiagnosticType.Error, string.Format(WebServicesWrapperStrings.InvokeWebMethodFailed, "Frammentata"), ei);
			}

			return false;
		}

		///<summary>
		/// StatoSpedizione
		///</summary>
		///<param name="nomeFileZip">nome del file di archivio trasferito (SENZA l'estensione .zip)</param>
		///<param name="statospedizione">risposta StatoSpedizione</param>
		//--------------------------------------------------------------------------------
		public bool StatoSpedizione(string nomeFileZip, out StatoSpedizione statospedizione)
		{
			statospedizione = null;
			try
			{
				Ticket tkt;
				if (!sosProxy.Singolo(codiceConservatore, codiceCliente, password, out tkt))
				{
					SosDiagnostic.Set(DiagnosticType.Error, WebServicesWrapperStrings.UnableToRetrieveSOSTkt);
					return false;
				}

				return sosProxy.StatoSpedizione(codiceConservatore, codiceSoggetto, tkt.Guid, nomeFileZip, out statospedizione);
			}
			catch (SoapException se)
			{
				ExtendedInfo ei = new ExtendedInfo();
				ei.Add(WebServicesWrapperStrings.Description, se.Message);
				ei.Add(WebServicesWrapperStrings.Source, se.Source);
				ei.Add(WebServicesWrapperStrings.Function, "StatoSpedizione");
				ei.Add(WebServicesWrapperStrings.Library, library);
				SosDiagnostic.Set(DiagnosticType.Error, string.Format(WebServicesWrapperStrings.InvokeWebMethodFailed, "StatoSpedizione"), ei);
			}
			catch (WebException we)
			{
				ExtendedInfo ei = new ExtendedInfo();
				ei.Add(WebServicesWrapperStrings.Description, we.Message);
				ei.Add(WebServicesWrapperStrings.Source, we.Source);
				ei.Add(WebServicesWrapperStrings.Function, "StatoSpedizione");
				ei.Add(WebServicesWrapperStrings.Library, library);
				SosDiagnostic.Set(DiagnosticType.Error, string.Format(WebServicesWrapperStrings.InvokeWebMethodFailed, "StatoSpedizione"), ei);
			}
			catch (OutOfMemoryException ome)
			{
				ExtendedInfo ei = new ExtendedInfo();
				ei.Add(WebServicesWrapperStrings.Description, ome.Message);
				ei.Add(WebServicesWrapperStrings.Source, ome.Source);
				ei.Add(WebServicesWrapperStrings.Function, "StatoSpedizione");
				ei.Add(WebServicesWrapperStrings.Library, library);
				SosDiagnostic.Set(DiagnosticType.Error, string.Format(WebServicesWrapperStrings.InvokeWebMethodFailed, "StatoSpedizione"), ei);
			}
			catch (Exception e)
			{
				ExtendedInfo ei = new ExtendedInfo();
				ei.Add(WebServicesWrapperStrings.Description, e.Message);
				ei.Add(WebServicesWrapperStrings.Source, e.Source);
				ei.Add(WebServicesWrapperStrings.Function, "StatoSpedizione");
				ei.Add(WebServicesWrapperStrings.Library, library);
				SosDiagnostic.Set(DiagnosticType.Error, string.Format(WebServicesWrapperStrings.InvokeWebMethodFailed, "StatoSpedizione"), ei);
			}

			return false;
		}

		///<summary>
		/// EsitoSpedizione
		///</summary>
		///<param name="nomeFileZip">nome del file di archivio trasferito (SENZA l'estensione .zip)</param>
		///<param name="codiceClasseDocumentale">codice della classe documentale</param>
		///<param name="esitoSpedizione">risposta EsitoSpedizione</param>
		//--------------------------------------------------------------------------------
		public bool EsitoSpedizione(string nomeFileZip, string codiceClasseDocumentale, out EsitoSpedizione esitoSpedizione)
		{
			esitoSpedizione = null;
			try
			{
				Ticket tkt;
				if (!sosProxy.Singolo(codiceConservatore, codiceCliente, password, out tkt))
				{
					SosDiagnostic.Set(DiagnosticType.Error, WebServicesWrapperStrings.UnableToRetrieveSOSTkt);
					return false;
				}

				return sosProxy.EsitoSpedizione(codiceConservatore, codiceSoggetto, tkt.Guid, nomeFileZip, codiceClasseDocumentale, out esitoSpedizione);
			}
			catch (SoapException se)
			{
				ExtendedInfo ei = new ExtendedInfo();
				ei.Add(WebServicesWrapperStrings.Description, se.Message);
				ei.Add(WebServicesWrapperStrings.Source, se.Source);
				ei.Add(WebServicesWrapperStrings.Function, "EsitoSpedizione");
				ei.Add(WebServicesWrapperStrings.Library, library);
				SosDiagnostic.Set(DiagnosticType.Error, string.Format(WebServicesWrapperStrings.InvokeWebMethodFailed, "EsitoSpedizione"), ei);
			}
			catch (WebException we)
			{
				ExtendedInfo ei = new ExtendedInfo();
				ei.Add(WebServicesWrapperStrings.Description, we.Message);
				ei.Add(WebServicesWrapperStrings.Source, we.Source);
				ei.Add(WebServicesWrapperStrings.Function, "EsitoSpedizione");
				ei.Add(WebServicesWrapperStrings.Library, library);
				SosDiagnostic.Set(DiagnosticType.Error, string.Format(WebServicesWrapperStrings.InvokeWebMethodFailed, "EsitoSpedizione"), ei);
			}
			catch (OutOfMemoryException ome)
			{
				ExtendedInfo ei = new ExtendedInfo();
				ei.Add(WebServicesWrapperStrings.Description, ome.Message);
				ei.Add(WebServicesWrapperStrings.Source, ome.Source);
				ei.Add(WebServicesWrapperStrings.Function, "EsitoSpedizione");
				ei.Add(WebServicesWrapperStrings.Library, library);
				SosDiagnostic.Set(DiagnosticType.Error, string.Format(WebServicesWrapperStrings.InvokeWebMethodFailed, "EsitoSpedizione"), ei);
			}
			catch (Exception e)
			{
				ExtendedInfo ei = new ExtendedInfo();
				ei.Add(WebServicesWrapperStrings.Description, e.Message);
				ei.Add(WebServicesWrapperStrings.Source, e.Source);
				ei.Add(WebServicesWrapperStrings.Function, "EsitoSpedizione");
				ei.Add(WebServicesWrapperStrings.Library, library);
				SosDiagnostic.Set(DiagnosticType.Error, string.Format(WebServicesWrapperStrings.InvokeWebMethodFailed, "EsitoSpedizione"), ei);
			}

			return false;
		}

		//--------------------------------------------------------------------------------
		public bool ElencoClassiDocumentali(out ClasseDocumentale[] classeDocList)
		{
			classeDocList = new ClasseDocumentale[]{};

			try
			{
				Ticket tkt;
				if (!sosProxy.Singolo(codiceConservatore, codiceCliente, password, out tkt))
				{
					SosDiagnostic.Set(DiagnosticType.Error, WebServicesWrapperStrings.UnableToRetrieveSOSTkt);
					return false;
				}

				return sosProxy.ElencoClassiDocumentali(codiceConservatore, codiceSoggetto, tkt.Guid, out classeDocList);
			}
			catch (SoapException se)
			{
				ExtendedInfo ei = new ExtendedInfo();
				ei.Add(WebServicesWrapperStrings.Description, se.Message);
				ei.Add(WebServicesWrapperStrings.Source, se.Source);
				ei.Add(WebServicesWrapperStrings.Function, "ElencoClassiDocumentali");
				ei.Add(WebServicesWrapperStrings.Library, library);
				SosDiagnostic.Set(DiagnosticType.Error, string.Format(WebServicesWrapperStrings.InvokeWebMethodFailed, "ElencoClassiDocumentali"), ei);
			}
			catch (WebException we)
			{
				ExtendedInfo ei = new ExtendedInfo();
				ei.Add(WebServicesWrapperStrings.Description, we.Message);
				ei.Add(WebServicesWrapperStrings.Source, we.Source);
				ei.Add(WebServicesWrapperStrings.Function, "ElencoClassiDocumentali");
				ei.Add(WebServicesWrapperStrings.Library, library);
				SosDiagnostic.Set(DiagnosticType.Error, string.Format(WebServicesWrapperStrings.InvokeWebMethodFailed, "ElencoClassiDocumentali"), ei);
			}
			catch (OutOfMemoryException ome)
			{
				ExtendedInfo ei = new ExtendedInfo();
				ei.Add(WebServicesWrapperStrings.Description, ome.Message);
				ei.Add(WebServicesWrapperStrings.Source, ome.Source);
				ei.Add(WebServicesWrapperStrings.Function, "ElencoClassiDocumentali");
				ei.Add(WebServicesWrapperStrings.Library, library);
				SosDiagnostic.Set(DiagnosticType.Error, string.Format(WebServicesWrapperStrings.InvokeWebMethodFailed, "ElencoClassiDocumentali"), ei);
			}
			catch (Exception e)
			{
				ExtendedInfo ei = new ExtendedInfo();
				ei.Add(WebServicesWrapperStrings.Description, e.Message);
				ei.Add(WebServicesWrapperStrings.Source, e.Source);
				ei.Add(WebServicesWrapperStrings.Function, "ElencoClassiDocumentali");
				ei.Add(WebServicesWrapperStrings.Library, library);
				SosDiagnostic.Set(DiagnosticType.Error, string.Format(WebServicesWrapperStrings.InvokeWebMethodFailed, "ElencoClassiDocumentali"), ei);
			}

			return false;
		}

		//--------------------------------------------------------------------------------
		public bool ElencoClassiDocumentali(string conservatore, string soggetto, string cliente, string pwd, out ClasseDocumentale[] classeDocList)
		{
			Ticket tkt;
			classeDocList = new ClasseDocumentale[] { };

			try
			{
				if (!sosProxy.Singolo(conservatore, cliente, pwd, out tkt))
				{
					SosDiagnostic.Set(DiagnosticType.Error, WebServicesWrapperStrings.UnableToRetrieveSOSTkt);
					return false;
				}

				return sosProxy.ElencoClassiDocumentali(conservatore, soggetto, tkt.Guid, out classeDocList);
			}
			catch (SoapException se)
			{
				ExtendedInfo ei = new ExtendedInfo();
				ei.Add(WebServicesWrapperStrings.Description, se.Message);
				ei.Add(WebServicesWrapperStrings.Source, se.Source);
				ei.Add(WebServicesWrapperStrings.Function, "ElencoClassiDocumentali");
				ei.Add(WebServicesWrapperStrings.Library, library);
				SosDiagnostic.Set(DiagnosticType.Error, string.Format(WebServicesWrapperStrings.InvokeWebMethodFailed, "ElencoClassiDocumentali"), ei);
			}
			catch (WebException we)
			{
				ExtendedInfo ei = new ExtendedInfo();
				ei.Add(WebServicesWrapperStrings.Description, we.Message);
				ei.Add(WebServicesWrapperStrings.Source, we.Source);
				ei.Add(WebServicesWrapperStrings.Function, "ElencoClassiDocumentali");
				ei.Add(WebServicesWrapperStrings.Library, library);
				SosDiagnostic.Set(DiagnosticType.Error, string.Format(WebServicesWrapperStrings.InvokeWebMethodFailed, "ElencoClassiDocumentali"), ei);
			}
			catch (OutOfMemoryException ome)
			{
				ExtendedInfo ei = new ExtendedInfo();
				ei.Add(WebServicesWrapperStrings.Description, ome.Message);
				ei.Add(WebServicesWrapperStrings.Source, ome.Source);
				ei.Add(WebServicesWrapperStrings.Function, "ElencoClassiDocumentali");
				ei.Add(WebServicesWrapperStrings.Library, library);
				SosDiagnostic.Set(DiagnosticType.Error, string.Format(WebServicesWrapperStrings.InvokeWebMethodFailed, "ElencoClassiDocumentali"), ei);
			}
			catch (Exception e)
			{
				ExtendedInfo ei = new ExtendedInfo();
				ei.Add(WebServicesWrapperStrings.Description, e.Message);
				ei.Add(WebServicesWrapperStrings.Source, e.Source);
				ei.Add(WebServicesWrapperStrings.Function, "ElencoClassiDocumentali");
				ei.Add(WebServicesWrapperStrings.Library, library);
				SosDiagnostic.Set(DiagnosticType.Error, string.Format(WebServicesWrapperStrings.InvokeWebMethodFailed, "ElencoClassiDocumentali"), ei);
			}

			return false;
		}

		//--------------------------------------------------------------------------------
		public bool ElencoChiaviClasseDocumentale(string codiceClasseDocumentale, out ClasseDocumentale docClass)
		{
			docClass = null;

			try
			{
				Ticket tkt;
				if (!sosProxy.Singolo(codiceConservatore, codiceCliente, password, out tkt))
				{
					SosDiagnostic.Set(DiagnosticType.Error, WebServicesWrapperStrings.UnableToRetrieveSOSTkt);
					return false;
				}

				return sosProxy.ElencoChiaviClasseDocumentale(codiceConservatore, codiceSoggetto, tkt.Guid, codiceClasseDocumentale, out docClass);
			}
			catch (SoapException se)
			{
				ExtendedInfo ei = new ExtendedInfo();
				ei.Add(WebServicesWrapperStrings.Description, se.Message);
				ei.Add(WebServicesWrapperStrings.Source, se.Source);
				ei.Add(WebServicesWrapperStrings.Function, "ElencoChiaviClasseDocumentale");
				ei.Add(WebServicesWrapperStrings.Library, library);
				SosDiagnostic.Set(DiagnosticType.Error, string.Format(WebServicesWrapperStrings.InvokeWebMethodFailed, "ElencoChiaviClasseDocumentale"), ei);
			}
			catch (WebException we)
			{
				ExtendedInfo ei = new ExtendedInfo();
				ei.Add(WebServicesWrapperStrings.Description, we.Message);
				ei.Add(WebServicesWrapperStrings.Source, we.Source);
				ei.Add(WebServicesWrapperStrings.Function, "ElencoChiaviClasseDocumentale");
				ei.Add(WebServicesWrapperStrings.Library, library);
				SosDiagnostic.Set(DiagnosticType.Error, string.Format(WebServicesWrapperStrings.InvokeWebMethodFailed, "ElencoChiaviClasseDocumentale"), ei);
			}
			catch (OutOfMemoryException ome)
			{
				ExtendedInfo ei = new ExtendedInfo();
				ei.Add(WebServicesWrapperStrings.Description, ome.Message);
				ei.Add(WebServicesWrapperStrings.Source, ome.Source);
				ei.Add(WebServicesWrapperStrings.Function, "ElencoChiaviClasseDocumentale");
				ei.Add(WebServicesWrapperStrings.Library, library);
				SosDiagnostic.Set(DiagnosticType.Error, string.Format(WebServicesWrapperStrings.InvokeWebMethodFailed, "ElencoChiaviClasseDocumentale"), ei);
			}
			catch (Exception e)
			{
				ExtendedInfo ei = new ExtendedInfo();
				ei.Add(WebServicesWrapperStrings.Description, e.Message);
				ei.Add(WebServicesWrapperStrings.Source, e.Source);
				ei.Add(WebServicesWrapperStrings.Function, "ElencoChiaviClasseDocumentale");
				ei.Add(WebServicesWrapperStrings.Library, library);
				SosDiagnostic.Set(DiagnosticType.Error, string.Format(WebServicesWrapperStrings.InvokeWebMethodFailed, "ElencoChiaviClasseDocumentale"), ei);
			}

			return false;
		}
	}
}