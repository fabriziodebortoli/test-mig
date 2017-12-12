using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;

using Microarea.TaskBuilderNet.Core.DiagnosticManager;
using Microarea.TaskBuilderNet.Core.WebServicesWrapper;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.TaskBuilderNet.TbSenderBL.SOS
{
	//================================================================================
	public class SOSParser
	{
		///<summary>
		/// Esegue un check generico delle risposte ritornate dall'it.sostitutivazucchetti.infinity.ws_as_servizioService
		/// Una valida risposta deve rispettare queste regole:
		/// - il nodo principale deve chiamarsi risposta
		/// - il nodo deve avere degli attributi
		/// - esiste almeno un attributo che si chiama servizio
		/// - il nodo deve avere dei sottoelementi
		///</summary>
		//--------------------------------------------------------------------------------
		private static bool CheckResponse(XElement xElemResponse)
		{
			if (
				string.Compare(xElemResponse.Name.LocalName, "risposta", StringComparison.InvariantCultureIgnoreCase) != 0 ||
				!xElemResponse.HasAttributes ||
				xElemResponse.Attributes("servizio").Count() == 0 ||
				!xElemResponse.HasElements
				)
			{
				SOSLogWriter.WriteLogEntry(NameSolverStrings.AllCompanies,
					@"La risposta da analizzare non è corretta (il nodo principale deve chiamarsi risposta, deve avere degli attributi, 
					deve esistere almeno un attributo che si chiama servizio, il nodo deve avere dei sottoelementi", "CheckResponse", logName: "SOSResponses");
				return false;
			}

			return true;
		}

		///<summary>
		/// Esegue il parse della risposta ritornata dai webmethod singolo o sequenza
		///</summary>
		//--------------------------------------------------------------------------------
		public static bool ParseTicket(string ticketResponse, out Ticket ticket)
		{
			ticket = null;

			try
			{
				XDocument xDoc = XDocument.Load(new StringReader(ticketResponse));
				XElement xElemResponse = (XElement)xDoc.FirstNode;

				if (!CheckResponse(xElemResponse))
					return false;

				// decido se si tratta di una chiamata singola o sequenza
				bool? isSingolo = null;
				if (xElemResponse.Attribute("servizio").Value == "ws_as_ticket.singolo")
					isSingolo = true;
				else
					if (xElemResponse.Attribute("servizio").Value == "ws_as_ticket.sequenza")
						isSingolo = false;

				if (isSingolo == null)
					return false;

				ticket = new Ticket();
				if (isSingolo == true)
				{
					ticket.Guid = xElemResponse.Value;
					return true;
				}

				IEnumerable<XElement> t = from el in xElemResponse.Descendants() where el.Name.LocalName == "ticket" select el;
				IEnumerable<XElement> chunks = from el in xElemResponse.Descendants("ticketchunk").Descendants("guid") select el;
				ticket.Guid = ((XElement)(t.ElementAt<XElement>(0).FirstNode)).Value;

				foreach (XElement ck in chunks)
					ticket.Chunks.Add(new TicketChunk(Int16.Parse(ck.FirstAttribute.Value), ck.Value));
			}
			catch (Exception e)
			{
				Debug.Fail(e.ToString());
				SOSLogWriter.WriteLogEntry(NameSolverStrings.AllCompanies, string.Format("Eccezione rilevata nel metodo ParseTicket. Message: {0}", e.Message), "ParseTicket", e.ToString(), "SOSResponses");
				return false;
			}

			return true;
		}

		///<summary>
		/// Esegue il parse della risposta ritornata dal webmethod semplice
		///</summary>
		//--------------------------------------------------------------------------------
		public static bool ParseSpedizioneSemplice(string spedizione, out RispostaSpedizione spedizioneSemplice)
		{
			spedizioneSemplice = null;

			try
			{
				XDocument xDoc = XDocument.Load(new StringReader(spedizione));
				XElement xElemResponse = (XElement)xDoc.FirstNode;

				if (!CheckResponse(xElemResponse) || xElemResponse.Attribute("servizio").Value != "ws_as_servizio.semplice")
					return false;

				spedizioneSemplice = new RispostaSpedizione();

				IEnumerable<XAttribute> attributes = ((XElement)(xElemResponse.FirstNode)).Attributes();

				foreach (XAttribute att in attributes)
				{
					if (att.Name.LocalName == "statospedizionesemplice")
						spedizioneSemplice.Risposta = string.Compare(att.Value, "SPDSMPOK", StringComparison.InvariantCultureIgnoreCase) == 0 
														? RispostaSpedizioneEnum.OK : RispostaSpedizioneEnum.KO;
					else if (att.Name.LocalName == "codicespedizione")
						spedizioneSemplice.CodiceSpedizione = att.Value;
				}

				if (((XElement)(xElemResponse.FirstNode)).HasElements)
				{
					XAttribute a = ((XElement)(((XElement)(xElemResponse.FirstNode)).FirstNode)).FirstAttribute;

					if (a != null && a.Name.LocalName == "codice")
					{
						if (string.IsNullOrWhiteSpace(a.Value))
							spedizioneSemplice.Errore = CodiceErrore.EMPTY;
						else
						{
							CodiceErrore ce = CodiceErrore.EMPTY;
							if (Enum.TryParse(a.Value, out ce))
								spedizioneSemplice.Errore = ce;
							else
								SOSLogWriter.WriteLogEntry(NameSolverStrings.AllCompanies,
									string.Format("Unexpected value for 'codice' attribute ({0}) for codicespedizione = {1}", a.Value, spedizioneSemplice.CodiceSpedizione),
									"ParseSpedizioneSemplice", logName: "SOSResponses");
						}
					}
				}
			}
			catch (Exception e)
			{
				Debug.Fail(e.ToString());
				SOSLogWriter.WriteLogEntry(NameSolverStrings.AllCompanies, string.Format("Eccezione rilevata nel metodo ParseSpedizioneSemplice. Message: {0}", e.Message),
				"ParseSpedizioneSemplice", e.ToString(), "SOSResponses");
				return false;
			}

			return true;
		}

		///<summary>
		/// Esegue il parse della risposta ritornata dal webmethod frammentata
		///</summary>
		//--------------------------------------------------------------------------------
		public static bool ParseSpedizioneFrammentata(string spedizione, out RispostaSpedizione spedizioneFrammentata)
		{
			spedizioneFrammentata = null;

			try
			{
				XDocument xDoc = XDocument.Load(new StringReader(spedizione));
				XElement xElemResponse = (XElement)xDoc.FirstNode;

				if (!CheckResponse(xElemResponse) || xElemResponse.Attribute("servizio").Value != "ws_as_servizio.frammentata")
					return false;

				spedizioneFrammentata = new RispostaSpedizione();

				IEnumerable<XAttribute> attributes = ((XElement)(xElemResponse.FirstNode)).Attributes();

				foreach (XAttribute att in attributes)
				{
					if (att.Name.LocalName == "statospedizioneframmentata")
						spedizioneFrammentata.Risposta = string.Compare(att.Value, "SPDFRMOK", StringComparison.InvariantCultureIgnoreCase) == 0 
														? RispostaSpedizioneEnum.OK : RispostaSpedizioneEnum.KO;
					else if (att.Name.LocalName == "codicespedizione")
						spedizioneFrammentata.CodiceSpedizione = att.Value;
					else if (att.Name.LocalName == "sequenza")
						spedizioneFrammentata.Sequenza = Int16.Parse(att.Value);
				}

				if (((XElement)(xElemResponse.FirstNode)).HasElements)
				{
					XAttribute a = ((XElement)(((XElement)(xElemResponse.FirstNode)).FirstNode)).FirstAttribute;

					if (a != null && a.Name.LocalName == "codice")
					{
						if (string.IsNullOrWhiteSpace(a.Value))
							spedizioneFrammentata.Errore = CodiceErrore.EMPTY;
						else
						{
							CodiceErrore ce = CodiceErrore.EMPTY;
							if (Enum.TryParse(a.Value, out ce))
								spedizioneFrammentata.Errore = ce;
							else
								SOSLogWriter.WriteLogEntry(NameSolverStrings.AllCompanies,
									string.Format("Unexpected value for 'codice' attribute ({0}) for codicespedizione = {1}", a.Value, spedizioneFrammentata.CodiceSpedizione),
									"ParseSpedizioneFrammentata", logName: "SOSResponses");
						}
					}
				}
			}
			catch (Exception e)
			{
				Debug.Fail(e.ToString());
				SOSLogWriter.WriteLogEntry(NameSolverStrings.AllCompanies, string.Format("Eccezione rilevata nel metodo ParseSpedizioneFrammentata. Message: {0}", e.Message),
				"ParseSpedizioneFrammentata", e.ToString(), "SOSResponses");
				return false;
			}

			return true;
		}

		///<summary>
		/// Esegue il parse della risposta ritornata dal webmethod statospedizione/esitospedizione
		///</summary>
		//--------------------------------------------------------------------------------
		public static bool ParseStatoSpedizione(string stato, out StatoSpedizione statoSpedizione)
		{
			statoSpedizione = new StatoSpedizione();

			try
			{
				XDocument xDoc = XDocument.Load(new StringReader(stato));
				XElement xElemResponse = (XElement)xDoc.FirstNode;

				if (!CheckResponse(xElemResponse) || xElemResponse.Attribute("servizio").Value != "ws_as_servizio.statospedizione")
					return false;

				IEnumerable<XAttribute> attributes = ((XElement)(xElemResponse.FirstNode)).Attributes();
				foreach (XAttribute att in attributes)
				{
					if (att.Name.LocalName == "codicespedizione")
						statoSpedizione.CodiceSpedizione = att.Value;
					else if (att.Name.LocalName == "naturaspedizione")
					{
						if (string.IsNullOrWhiteSpace(att.Value))
							statoSpedizione.NaturaSpedizione = NaturaSpedizione.EMPTY;
						else
						{
							NaturaSpedizione ns = NaturaSpedizione.EMPTY;
							if (Enum.TryParse(att.Value, out ns))
								statoSpedizione.NaturaSpedizione = ns;
							else
								SOSLogWriter.WriteLogEntry(NameSolverStrings.AllCompanies,
									string.Format("Unexpected value for 'naturaspedizione' attribute ({0}) for codicespedizione = {1}", att.Value, statoSpedizione.CodiceSpedizione),
									"ParseStatoSpedizione", logName: "SOSResponses");
						}
					}
					else if (att.Name.LocalName == "datapresaincarico")
						statoSpedizione.DataPresaInCarico = string.IsNullOrWhiteSpace(att.Value) // SOS ritorna il formato data MM/GG/AAAA
															? (DateTime)SqlDateTime.MinValue
															: Convert.ToDateTime(att.Value, new CultureInfo("en-US"));
					else if (att.Name.LocalName == "statospedizione")
					{
						if (string.IsNullOrWhiteSpace(att.Value))
							statoSpedizione.StatoSpedizione = StatoSpedizioneEnum.EMPTY;
						else
						{
							StatoSpedizioneEnum ss = StatoSpedizioneEnum.EMPTY;
							if (Enum.TryParse(att.Value, out ss))
								statoSpedizione.StatoSpedizione = ss;
							else
								SOSLogWriter.WriteLogEntry(NameSolverStrings.AllCompanies,
									string.Format("Unexpected value for 'statospedizione' attribute ({0}) for codicespedizione = {1}", att.Value, statoSpedizione.CodiceSpedizione),
									"ParseStatoSpedizione", logName: "SOSResponses");
						}
					}
					else if (att.Name.LocalName == "codiceclassedocumentale")
						statoSpedizione.CodiceClasseDocumentale = att.Value;
				}

				XAttribute a = ((XElement)(((XElement)(xElemResponse.FirstNode)).FirstNode)).FirstAttribute;
				if (a.Name.LocalName == "dataarchiviazionedocumentale")
					statoSpedizione.Contenuto.DataArchiviazioneDocumentale = string.IsNullOrWhiteSpace(a.Value) // SOS ritorna il formato data MM/GG/AAAA
																			? (DateTime)SqlDateTime.MinValue
																			: Convert.ToDateTime(a.Value, new CultureInfo("en-US")); 
			}
			catch (Exception e)
			{
				Debug.Fail(e.ToString());
				SOSLogWriter.WriteLogEntry(NameSolverStrings.AllCompanies, string.Format("Eccezione rilevata nel metodo ParseStatoSpedizione. Message: {0}", e.Message),
				"ParseStatoSpedizione", e.ToString(), "SOSResponses");
				return false;
			}

			return true;
		}

		///<summary>
		/// Esegue il parse della risposta ritornata dal webmethod esitospedizione
		///</summary>
		//--------------------------------------------------------------------------------
		public static bool ParseEsitoSpedizione(string esito, out EsitoSpedizione esitoSpedizione)
		{
			esitoSpedizione = new EsitoSpedizione();

			try
			{
				XDocument xDoc = XDocument.Load(new StringReader(esito));
				XElement xElemResponse = (XElement)xDoc.FirstNode;

				if (
					!CheckResponse(xElemResponse) || 
					!(xElemResponse.Attribute("servizio").Value == "ws_as_servizio.statospedizione" ||
					xElemResponse.Attribute("servizio").Value == "ws_as_servizio.esitospedizione")
					)
					return false;

				IEnumerable<XAttribute> attributes = ((XElement)(xElemResponse.FirstNode)).Attributes();
				foreach (XAttribute att in attributes)
				{
					if (att.Name.LocalName == "codicespedizione")
						esitoSpedizione.CodiceSpedizione = att.Value;
					else if (att.Name.LocalName == "naturaspedizione")
					{
						if (string.IsNullOrWhiteSpace(att.Value))
							esitoSpedizione.NaturaSpedizione = NaturaSpedizione.EMPTY;
						else
						{
							NaturaSpedizione ns = NaturaSpedizione.EMPTY;
							if (Enum.TryParse(att.Value, out ns))
								esitoSpedizione.NaturaSpedizione = ns;
							else
								SOSLogWriter.WriteLogEntry(NameSolverStrings.AllCompanies,
									string.Format("Unexpected value for 'naturaspedizione' attribute ({0}) for codicespedizione = {1}", att.Value, esitoSpedizione.CodiceSpedizione),
									"ParseEsitoSpedizione", logName: "SOSResponses");
						}
					}
					else if (att.Name.LocalName == "datapresaincarico")
						esitoSpedizione.DataPresaInCarico = string.IsNullOrWhiteSpace(att.Value) // SOS ritorna il formato data MM/GG/AAAA
															? (DateTime)SqlDateTime.MinValue
															: Convert.ToDateTime(att.Value, new CultureInfo("en-US")); 
					else if (att.Name.LocalName == "statospedizione")
					{
						if (string.IsNullOrWhiteSpace(att.Value))
							esitoSpedizione.StatoSpedizione = StatoSpedizioneEnum.EMPTY;
						else
						{
							StatoSpedizioneEnum ss = StatoSpedizioneEnum.EMPTY;
							if (Enum.TryParse(att.Value, out ss))
								esitoSpedizione.StatoSpedizione = ss;
							else
								SOSLogWriter.WriteLogEntry(NameSolverStrings.AllCompanies,
									string.Format("Unexpected value for 'statospedizione' attribute ({0}) for codicespedizione = {1}", att.Value, esitoSpedizione.CodiceSpedizione),
									"ParseEsitoSpedizione", logName: "SOSResponses");
						}
					}
					else if (att.Name.LocalName == "codiceclassedocumentale")
						esitoSpedizione.CodiceClasseDocumentale = att.Value;
				}

				IEnumerable<XElement> documents = xElemResponse.Descendants("documento");
				// per ogni nodo di tipo document
				foreach (XElement docElem in documents)
				{
					Documento doc = new Documento();
					// leggo gli attributi
					foreach (XAttribute att in docElem.Attributes())
					{
						if (att.Name.LocalName == "dataarchiviazionedocumentale")
							doc.DataArchiviazioneDocumentale = string.IsNullOrWhiteSpace(att.Value) // SOS ritorna il formato data MM/GG/AAAA
																? (DateTime)SqlDateTime.MinValue 
																: Convert.ToDateTime(att.Value, new CultureInfo("en-US")); 
						else if (att.Name.LocalName == "dataconservazionesostitutiva")
							doc.DataConservazioneSostitutiva = string.IsNullOrWhiteSpace(att.Value) // SOS ritorna il formato data MM/GG/AAAA
																? (DateTime)SqlDateTime.MinValue 
																: Convert.ToDateTime(att.Value, new CultureInfo("en-US")); 
						else if (att.Name.LocalName == "codiceclassedocumentale")
							doc.CodiceClasseDocumentale = att.Value;
					}

					// scorro i sottonodi
					foreach (XElement child in docElem.Descendants())
					{
						if (child.Name == "nomefileoriginale")
							doc.NomeFileOriginale = child.Value;
						else if (child.Name == "codiceassoluto")
							doc.CodiceAssoluto = child.Value;
						else if (child.Name == "lottoid")
							doc.LottoId = child.Value;
						else if (child.Name == "stato")
						{
							if (string.IsNullOrWhiteSpace(child.Value))
								doc.StatoDocumento = StatoDocumento.EMPTY;
							else
							{
								StatoDocumento sd = StatoDocumento.EMPTY;
								if (Enum.TryParse(child.Value, out sd))
									doc.StatoDocumento = sd;
								else
									SOSLogWriter.WriteLogEntry(NameSolverStrings.AllCompanies,
										string.Format("Unexpected value for 'stato' attribute ({0}) for nomefileoriginale = {1} codicespedizione = {2}", child.Value, doc.NomeFileOriginale, esitoSpedizione.CodiceSpedizione),
										"ParseEsitoSpedizione", logName: "SOSResponses");
							}
						}
						else if (child.Name == "chiave")
						{
							Chiave key = new Chiave();
							XAttribute chiaveAttr = ((XAttribute)(child.FirstAttribute));
							if (chiaveAttr.Name.LocalName == "codicechiave")
								key.CodiceChiave = chiaveAttr.Value;
							if (child.HasElements && ((XElement)(child.FirstNode)).Name == "valore")
								key.Valore = ((XElement)(child.FirstNode)).Value;
							doc.Chiavi.Add(key);
						}
						else if (child.Name == "valore")
							continue; // skippo il nodo <valore> perche' l'ho gia' analizzato nel nodo parent "chiave" qui sopra
						else if (child.Name == "errore")
						{
							Errore err = new Errore();
							foreach (XElement subChild in child.Descendants())
							{
								if (subChild.Name == "codice")
								{
									if (string.IsNullOrWhiteSpace(subChild.Value))
										err.Codice = CodiceErrore.EMPTY;
									else
									{
										CodiceErrore ce = CodiceErrore.EMPTY;
										if (Enum.TryParse(child.Value, out ce))
											err.Codice = ce;
										else
											SOSLogWriter.WriteLogEntry(NameSolverStrings.AllCompanies,
											string.Format("Unexpected value for 'codice' attribute ({0}) for nomefileoriginale = {1} codicespedizione = {2}", child.Value, doc.NomeFileOriginale, esitoSpedizione.CodiceSpedizione),
											"ParseEsitoSpedizione", logName: "SOSResponses");
									}
								}
								else if (subChild.Name == "descrizione")
									err.Descrizione = subChild.Value;
							}
							doc.Errori.Add(err);
						}
					}

					esitoSpedizione.Documenti.Add(doc);
				}
			}
			catch (Exception e)
			{
				Debug.Fail(e.ToString());
				SOSLogWriter.WriteLogEntry(NameSolverStrings.AllCompanies, string.Format("Eccezione rilevata nel metodo ParseEsitoSpedizione. Message: {0}", e.Message),
				"ParseEsitoSpedizione", e.ToString(), "SOSResponses");
				return false;
			}

			return true;
		}

		///<summary>
		/// Esegue il parse della risposta ritornata dal webmethod elencoclassidocumentali
		///</summary>
		//--------------------------------------------------------------------------------
		public static bool ParseElencoClassiDocumentali(string elenco, out List<ClasseDocumentale> classeDocList)
		{
			classeDocList = null;

			try
			{
				XDocument xDoc = XDocument.Load(new StringReader(elenco));
				XElement xElemResponse = (XElement)xDoc.FirstNode;

				if (!CheckResponse(xElemResponse) || xElemResponse.Attribute("servizio").Value != "ws_as_servizio.elencoclassidocumentali")
					return false;

				classeDocList = new List<ClasseDocumentale>();

				IEnumerable<XElement> classes = from c in xElemResponse.Descendants()
												where c.Name.LocalName == "classedocumentale"
												select c;

				foreach (XElement cl in classes)
					classeDocList.Add(new ClasseDocumentale(cl.FirstAttribute.Value, ((XElement)(cl.FirstNode)).Value));
			}
			catch (Exception e)
			{
				Debug.Fail(e.ToString());
				SOSLogWriter.WriteLogEntry(NameSolverStrings.AllCompanies, string.Format("Eccezione rilevata nel metodo ParseElencoClassiDocumentali. Message: {0}", e.Message),
				"ParseElencoClassiDocumentali", e.ToString(), "SOSResponses");
				return false;
			}

			return true;
		}

		///<summary>
		/// Esegue il parse della risposta ritornata dal webmethod elencochiaviclassedocumentale
		///</summary>
		//--------------------------------------------------------------------------------
		public static bool ParseElencoChiaviClasseDocumentale(string chiavi, out ClasseDocumentale docClass)
		{
			docClass = null;

			try
			{
				XDocument xDoc = XDocument.Load(new StringReader(chiavi));
				XElement xElemResponse = (XElement)xDoc.FirstNode;

				if (!CheckResponse(xElemResponse) || xElemResponse.Attribute("servizio").Value != "ws_as_servizio.elencochiaviclassedocumentale")
					return false;

				docClass = new ClasseDocumentale();
				docClass.CodiceClasseDocumentale = ((XElement)(xElemResponse.FirstNode)).Attribute("codiceclassedocumentale").Value;

				IEnumerable<XElement> chiaviElements = from c in xElemResponse.Descendants() where c.Name.LocalName == "chiave" select c;

				foreach (XElement k in chiaviElements)
				{
					Chiave key = new Chiave();
					IEnumerable<XAttribute> kAtt = k.Attributes();

					foreach (XAttribute att in kAtt)
					{
						if (att.Name.LocalName == "codicechiave")
							key.CodiceChiave = att.Value;
						else if (att.Name.LocalName == "tipo")
							key.Tipo = att.Value;
						else if (att.Name.LocalName == "descrizione")
							key.Descrizione = att.Value;
						else if (att.Name.LocalName == "fiscale")
							key.Fiscale = (att.Value == "1");
						else if (att.Name.LocalName == "lunghezza")
							key.Lunghezza = Int16.Parse(att.Value);
						else if (att.Name.LocalName == "ordine")
							key.Ordine = Int16.Parse(att.Value);
						else if (att.Name.LocalName == "datascad")
							key.DataScad = (att.Value == "S");
						else if (att.Name.LocalName == "cprownum")
							key.CPRowNum = Int16.Parse(att.Value);
						else if (att.Name.LocalName == "valcombo")
							key.ValCombo = Int16.Parse(att.Value);
						else if (att.Name.LocalName == "cdobblig")
							key.CdObblig = (att.Value == "S");
					}

					docClass.Chiavi.Add(key);
				}
			}
			catch (Exception e)
			{
				Debug.Fail(e.ToString());
				SOSLogWriter.WriteLogEntry(NameSolverStrings.AllCompanies, string.Format("Eccezione rilevata nel metodo ParseElencoChiaviClasseDocumentale. Message: {0}", e.Message),
				"ParseElencoChiaviClasseDocumentale", e.ToString(), "SOSResponses");
				return false;
			}

			return true;
		}
	}
}
