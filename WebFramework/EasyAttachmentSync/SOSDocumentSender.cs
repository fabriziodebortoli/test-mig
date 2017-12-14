using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Threading;

using Microarea.EasyAttachment.Core;
using Microarea.TaskBuilderNet.Core.DiagnosticManager;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Core.WebServicesWrapper;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.WebServices.EasyAttachmentSync
{
	///<summary>
	/// Classe che si occupa di inviare in archiviazione sostitutiva una lista di allegati
	/// Al suo interno definisce una coda di SOSDocElem.
	/// Su un thread separato scorre la coda e, dopo avere preparato i documenti, li invia
	/// per l'archiviazione al servizio Zucchetti SOS
	///</summary>
	//================================================================================
	public class SOSDocumentSender
	{
		private SOSDocQueue<SOSDocElement> sosQueue = new SOSDocQueue<SOSDocElement>();

		internal bool SendToSOSThreadIsStarted = false;

		// dimensione chunk
		private long chunkDimensionInBytes = 20971520; // default is 20MB (dimensione di un blocco di file)

		//--------------------------------------------------------------------------------
		private string sosConnectorTempPath
		{
			get
			{
				// se non esiste creo il folder SOSConnectorTemp
				string sosConnectorTempFolder = Path.Combine(EASyncEngine.GetTempFolderForEasyAttachmentSync(), "SOSConnectorTemp");
				if (!Directory.Exists(sosConnectorTempFolder))
					Directory.CreateDirectory(sosConnectorTempFolder);
				return sosConnectorTempFolder;
			}
		}

		///<summary>
		/// Costruttore
		///</summary>
		//--------------------------------------------------------------------------------
		public SOSDocumentSender()
		{
			// registro l'evento di change all'interno della queue (intercetta SOLO l'evento di Enqueue)
			sosQueue.Changed += new EventHandler(SosQueue_Changed);
		}

		///<summary>
		/// Aggiunge alla queue un elemento
		///</summary>
		//--------------------------------------------------------------------------------
		public void EnqueueElement(DmsDatabaseInfo dmsInfo, List<int> attachmentIds, int loginId)
		{
			SOSDocElement elem = new SOSDocElement(dmsInfo, attachmentIds, loginId);
			sosQueue.Enqueue(elem);
		}

		///<summary>
		/// Ritorna il primo elemento disponibile nella queue
		///</summary>
		//--------------------------------------------------------------------------------
		public SOSDocElement DequeueElement()
		{
			return sosQueue.Dequeue();
		}

		///<summary>
		/// Intercetta l'evento di Enqueue sulla coda e avvia il thread di esecuzione
		/// (se non gia' attivo)
		///</summary>
		//--------------------------------------------------------------------------------
		private void SosQueue_Changed(object sender, EventArgs e)
		{
			// se il thread e' gia' partito non vado avanti
			if (SendToSOSThreadIsStarted)
				return;

			//lock (typeof(SOSDocumentSender))
				SendToSOSThreadIsStarted = true;

			Thread senderThread = new Thread(() =>
			{
				SOSInternalSend(); // richiamo il thread separato per l'invio dei file
				
				//lock (typeof(SOSDocumentSender))
					SendToSOSThreadIsStarted = false;
			});

			senderThread.SetApartmentState(ApartmentState.STA);
			// quando si istanzia un nuovo Thread bisogna assegnargli le CurrentCulture, altrimenti le
			// traduzioni in lingue differenti da quelle del sistema operativo non funzionano!!!
			senderThread.CurrentUICulture = Thread.CurrentThread.CurrentUICulture;
			senderThread.CurrentCulture = Thread.CurrentThread.CurrentCulture;

			senderThread.Start();
		}

		///<summary>
		/// Thread separato che esegue il Send dei documenti per ogni elemento della Queue
		/// Fintanto che ci sono elementi nella coda continua la sua esecuzione (vive, quindi, di vita propria)
		///</summary>
		//-----------------------------------------------------------------------------------------------
		private void SOSInternalSend()
		{
			if (sosQueue.Count == 0)
				return;

			CoreSOSManager coreSosMng = null;

			// finche' ci sono elementi nella coda eseguo il loop
			while (sosQueue.Count > 0)
			{
				// leggo il primo elemento della coda
				SOSDocElement sde = sosQueue.Dequeue();
				// se non ci sono attachment non procedo
				if (sde == null || sde.AttachmentIds.Count == 0)
					continue;

				EASyncEngine.SetCulture();

				// istanzio il model con la stringa di connessione
				DMSModelDataContext dmsModel = new DMSModelDataContext(sde.DmsInfo.DMSConnectionString);
				
				try
				{
					DMS_SOSConfiguration sosConf = (from sc in dmsModel.DMS_SOSConfigurations select sc).Single();
					if (sosConf == null || string.IsNullOrWhiteSpace(sosConf.KeeperCode) || string.IsNullOrWhiteSpace(sosConf.SubjectCode))
						break;

					// il parametro ChunkDimension e' espresso in MB, devo convertirlo in bytes
					chunkDimensionInBytes = (long)(((sosConf.ChunkDimension == null) ? 20 : sosConf.ChunkDimension) * 1024 * 1024);

					// istanzio un CoreSOSManager
					coreSosMng = new CoreSOSManager(dmsModel, sosConnectorTempPath, sosConf.SubjectCode, sde.DmsInfo.Company, sde.DmsInfo.DMSConnectionString, sde.LoginId, true);
					if (!coreSosMng.PrepareSOSDocumentsAndEnvelope(sde.AttachmentIds))
						return;

					foreach (SOSZipElement zip in coreSosMng.SOSZipList)
					{
						SOSLogWriter.WriteLogEntry(sde.DmsInfo.Company, "Starting SendEnvelope");
						SendEnvelope(zip.Elements, sde, dmsModel, coreSosMng, coreSosMng.DocClassName, coreSosMng.CollectionId, zip.ZipFilePath, sde.LoginId);
						SOSLogWriter.WriteLogEntry(sde.DmsInfo.Company, "Ending SendEnvelope");
					}
				}
				catch(Exception e)
				{
					EASyncEngine.WriteMessageInEventLog(string.Format(EASyncStrings.MethodExceptionWithDetail, "EasyAttachmentSync:InternalSend", e.ToString()));
				}
				finally
				{
					if (dmsModel != null)
						dmsModel.Dispose();

					// alla fine elimino i file temporanei
					if (coreSosMng != null)
						coreSosMng.DeleteTemporaryFiles();
				}
			}
		}

		///<summary>
		/// Esegue l'invio dell'envelope
		/// Spedisce il file zip tramite una chiamata al webservice SOSZucchetti
		///</summary>
		//--------------------------------------------------------------------------------
		private bool SendEnvelope
			(
			List<SOSEnvelopeElement> envElements,
			SOSDocElement sde,
			DMSModelDataContext	dmsModel,
			CoreSOSManager coreSosMng,
			string	docClassName,
			int		collectionId,
			string	zipFilePath,
			int		loginId
			)
		{
			try
			{
				// Utilizzo un proxy temporaneo
				SOSProxyWrapper sosProxy = new SOSProxyWrapper(BasePathFinder.BasePathFinderInstance.SOSProxyUrl);
				sosProxy.Init
					(
					coreSosMng.SOSConfigurationState.SOSConfiguration.KeeperCode,
					coreSosMng.SOSConfigurationState.SOSConfiguration.SubjectCode,
					coreSosMng.SOSConfigurationState.SOSConfiguration.MySOSUser,
					Crypto.Decrypt(coreSosMng.SOSConfigurationState.SOSConfiguration.MySOSPassword),
					coreSosMng.SOSConfigurationState.SOSConfiguration.SOSWebServiceUrl
					);

				// calcolo le dimensioni del file zip
				FileInfo fi = new FileInfo(zipFilePath);
				bool isSmallFile = (fi.Length <= chunkDimensionInBytes);

				SOSLogWriter.WriteLogEntry(sde.DmsInfo.Company, string.Format("Start sending {0} file with '{1}' web-method", zipFilePath, (isSmallFile ? "Semplice" : "Frammentata"), "SendEnvelope"));
				SOSLogWriter.AppendText(sde.DmsInfo.Company, string.Format("Zip file length '{0}' bytes", fi.Length.ToString()));

				// se la dimensione del file e' maggiore della dimensione prevista da chunk eseguo la spedizione frammentata, altrimenti quella semplice
				bool sendResult = isSmallFile ? SendSemplice(sosProxy, zipFilePath) : SendFrammentata(sosProxy, zipFilePath, fi.Length, sde.DmsInfo.Company);

				// inizializzo uno StatoSpedizione generico, anche nel caso di spedizione con errori
				StatoSpedizione statoSped = new StatoSpedizione();
				// se la spedizione torna true procedo con la richiesta dello StatoSpedizione
				if (sendResult)
				{
					SOSLogWriter.AppendText(sde.DmsInfo.Company, string.Format("'{0}' successfully completed", (isSmallFile ? "SendSemplice" : "SendFrammentata")));

					// metto uno sleep di 5 secondi per evitare l'errore di interpretazione dello stato intermedio in uso dalla SOS (SPDOK)
					Thread.Sleep(5000);

					if (!sosProxy.StatoSpedizione(Path.GetFileNameWithoutExtension(zipFilePath), out statoSped))
					{
						SOSLogWriter.WriteLogEntry(sde.DmsInfo.Company, string.Format("Error during '{0}' web-method. Please check MA Server in EventViewer", "StatoSpedizione"), "SendEnvelope");

						EASyncEngine.WriteMessageInEventLog(string.Format(EASyncStrings.SOSErrorStatoSpedizione, zipFilePath));
						if (statoSped == null) // nel caso fosse null inizializzo lo statospedizione
						{
							statoSped = new StatoSpedizione();
							statoSped.CodiceSpedizione = Path.GetFileNameWithoutExtension(zipFilePath);
						}
					}
				}
				else
					SOSLogWriter.WriteLogEntry(sde.DmsInfo.Company, string.Format("Error during '{0}'. Please check MA Server in EventViewer", (isSmallFile ? "SendSemplice" : "SendFrammentata")), "SendEnvelope");

				// creo in ogni caso il record nella tabella DMS_SOSEnvelope
				DMS_SOSEnvelope newEnvelope = new DMS_SOSEnvelope();
				newEnvelope.CollectionID = collectionId;
				newEnvelope.DispatchCode = (sendResult) ? statoSped.CodiceSpedizione : Path.GetFileNameWithoutExtension(zipFilePath);
				newEnvelope.DispatchStatus = (int)statoSped.StatoSpedizione;
				newEnvelope.DispatchDate = statoSped.DataPresaInCarico; // sara' quasi sempre vuota quindi impostata con la SqlMinDate
				newEnvelope.SynchronizedDate = DateTime.Now;
				newEnvelope.LoginId = loginId;
				newEnvelope.SendingType = (int)SendingType.WebService;
				dmsModel.DMS_SOSEnvelopes.InsertOnSubmit(newEnvelope);
				dmsModel.SubmitChanges();

				SOSLogWriter.WriteLogEntry(sde.DmsInfo.Company, string.Format("Created new SOSEnvelope with ID = {0}, DispatchCode = {1}, DispatchStatus = {2}",
											newEnvelope.EnvelopeID.ToString(), newEnvelope.DispatchCode, statoSped.StatoSpedizione.ToString()), "SendEnvelope");

				bool checkWithEsitoSped = false;

				StatoDocumento statusForSOSDoc = StatoDocumento.EMPTY;
				// calcolo lo stato da mettere al SOSDocument in base allo statospedizione dell'envelope
				switch ((StatoSpedizioneEnum)newEnvelope.DispatchStatus)
				{
					case StatoSpedizioneEnum.SPDOK: // stato intermedio di Zucchetti: significa che lato SOS stanno ancora elaborando la spedizione, pertanto non va interpretato come un errore
					case StatoSpedizioneEnum.SPDOP:
						statusForSOSDoc = StatoDocumento.SENT;
						break;
					case StatoSpedizioneEnum.SPDALLOK:
						statusForSOSDoc = StatoDocumento.DOCTEMP;
						break;
					case StatoSpedizioneEnum.SPDALLKO:
						statusForSOSDoc = StatoDocumento.DOCKO;
						break;
					case StatoSpedizioneEnum.SPDOKWERR:
						// in questo caso devo richiamare necessariamente l'esitospedizione, per capire quali sono i documenti andati a buon fine e quali no.
						// faccio break e richiamo l'esito spedizione dopo (perche' devo confrontare i singoli sosdoc)
						checkWithEsitoSped = true;
						// nel dubbio metto lo stato SENT, cosi' se l'esitospedizione non ritorna l'elenco dei file sono cmq in uno stato coerente
						statusForSOSDoc = StatoDocumento.SENT;
						break;
					default:
						// in tutti gli altri casi si tratta di errori bloccanti e imposto d'ufficio lo stato 
						// del documento come da inviare nuovamente
						statusForSOSDoc = StatoDocumento.TORESEND;
						break;
				}

				// nuova gestione Update usando ADO
				using (SqlConnection eaConnection = new SqlConnection(sde.DmsInfo.DMSConnectionString))
				{
					eaConnection.Open();

					try
					{
						using (SqlCommand myCommand = eaConnection.CreateCommand())
						{
							// vado ad aggiornare i vari SOSDocument in modo da agganciarli all'envelope: imposto il DocumentStatus e l'EnvelopeID
							foreach (SOSEnvelopeElement elem in envElements)
							{
								// se lo stato documento e' maggiore o uguale a DOCTEMP elimino il binario del PDF/A
								if (statusForSOSDoc >= StatoDocumento.DOCTEMP)
								{
									myCommand.CommandText = string.Format
										(
											@"UPDATE [DMS_SOSDocument] SET [PdfABinary] = NULL WHERE [AttachmentID] = {0}",
											elem.AttachmentId.ToString()
										);
									myCommand.ExecuteNonQuery();
									SOSLogWriter.AppendText(sde.DmsInfo.Company, string.Format("Updating PdfABinary content in SOSDocument with attachmentId = {0}", elem.AttachmentId.ToString()));
								}

								// poi vado ad aggiornare l'EnvelopeId e il DocumentStatus
								myCommand.CommandText = string.Format
									(
										@"UPDATE [DMS_SOSDocument] SET [EnvelopeID] = @envelopeId, [DocumentStatus] = @docStatus WHERE [AttachmentID] = {0}",
										elem.AttachmentId.ToString()
									);
								myCommand.Parameters.AddWithValue("@envelopeId", newEnvelope.EnvelopeID);
								myCommand.Parameters.AddWithValue("@docStatus", (int)statusForSOSDoc);
								myCommand.ExecuteNonQuery();
								myCommand.Parameters.Clear();
								SOSLogWriter.AppendText(sde.DmsInfo.Company, string.Format("Updating SOSDocument with attachmentId = {0} set DocumentStatus = {1}", elem.AttachmentId.ToString(), statusForSOSDoc.ToString()));
							}
						}

						// se devo anche controllare l'esito spedizione
						if (checkWithEsitoSped)
						{
							SOSLogWriter.WriteLogEntry(sde.DmsInfo.Company, string.Format("Calling {0} for envelope {1} and DocClass {2}", "EsitoSpedizione", newEnvelope.DispatchCode, docClassName), "SendEnvelope");

							EsitoSpedizione esitoSped;
							if (sosProxy.EsitoSpedizione(newEnvelope.DispatchCode, docClassName, out esitoSped) && esitoSped != null)
							{
								// aggiorno lo stato della spedizione
								newEnvelope.DispatchStatus = (int)esitoSped.StatoSpedizione;
								newEnvelope.DispatchDate = esitoSped.DataPresaInCarico;
								dmsModel.SubmitChanges();

								SOSLogWriter.AppendText(sde.DmsInfo.Company, string.Format("Updating SOSEnvelope with EnvelopeId = {0} set DispatchStatus = {1}", newEnvelope.EnvelopeID.ToString(), esitoSped.StatoSpedizione.ToString()));

								SOSLogWriter.AppendText(sde.DmsInfo.Company, string.Format("Start check SOSDocuments belonging SOSEnvelope with EnvelopeId = {0}", newEnvelope.EnvelopeID.ToString()));

								foreach (DMS_SOSDocument sosDoc in newEnvelope.DMS_SOSDocuments)
								{
									// Attenzione che l'esito torna i soli documenti che non sono stati scartati!
									Documento doc = esitoSped.GetDocumentByAttachmentId(sosDoc.AttachmentID);

									if (doc == null)
									{
										SOSLogWriter.AppendText(sde.DmsInfo.Company, string.Format("SOSDocument with AttachmentID = {0} does not exist in SOS. Its DocumentStatus is set to RESEND.", sosDoc.AttachmentID));
										// ai documenti scartati (ovvero non ritornati) assegno lo stato TORESEND
										sosDoc.DocumentStatus = (int)StatoDocumento.TORESEND;
									}
									else
									{
										using (SqlCommand sqlCommand = eaConnection.CreateCommand())
										{
											// se lo stato documento e' maggiore o uguale a DOCTEMP elimino il binario del PDF/A
											if (doc.StatoDocumento >= StatoDocumento.DOCTEMP)
											{
												sqlCommand.CommandText = string.Format
													(
														@"UPDATE [DMS_SOSDocument] SET [PdfABinary] = NULL WHERE [AttachmentID] = {0}",
														sosDoc.AttachmentID.ToString()
													);
												sqlCommand.ExecuteNonQuery();
												SOSLogWriter.AppendText(sde.DmsInfo.Company, string.Format("Updating PdfABinary content in SOSDocument with attachmentId = {0}", sosDoc.AttachmentID.ToString()));
											}
										}

										sosDoc.AbsoluteCode = doc.CodiceAssoluto;
										sosDoc.ArchivedDate = doc.DataArchiviazioneDocumentale;
										sosDoc.DocumentStatus = (int)doc.StatoDocumento;
										sosDoc.LotID = doc.LottoId;
										sosDoc.RegistrationDate = doc.DataConservazioneSostitutiva;

										SOSLogWriter.AppendText(sde.DmsInfo.Company, string.Format("SOSDocument with AttachmentID = {0} exists in SOS. Its DocumentStatus is set to {1}. (AbsoluteCode = {2} - LotID = {3})",
											sosDoc.AttachmentID, doc.StatoDocumento.ToString(), doc.CodiceAssoluto, doc.LottoId));
									}
									dmsModel.SubmitChanges();
								}
							}
							else
								SOSLogWriter.WriteLogEntry(sde.DmsInfo.Company, string.Format("Error during '{0}'. Please check MA Server in EventViewer", "EsitoSpedizione"), "SendEnvelope");

							// aggiorno la data di sincronizzazione
							newEnvelope.SynchronizedDate = DateTime.Now;
							dmsModel.SubmitChanges();
						}
					}
					catch (Exception ex)
					{
						SOSLogWriter.WriteLogEntry(sde.DmsInfo.Company, "Error during 'Nuova gestione Update'", "SendEnvelope", ex.ToString());
					}
				}
			}
			catch (SqlException e)
			{
				EASyncEngine.WriteMessageInEventLog(string.Format(EASyncStrings.MethodExceptionWithDetail, "EasyAttachmentSync:SendEnvelope", e.ToString()));
				SOSLogWriter.WriteLogEntry(sde.DmsInfo.Company, string.Format(EASyncStrings.MethodExceptionWithDetail, "EasyAttachmentSync:SendEnvelope", e.ToString()));
				return false;
			}
			catch (InvalidOperationException ioEx)
			{
				EASyncEngine.WriteMessageInEventLog(string.Format(EASyncStrings.MethodExceptionWithDetail, "EasyAttachmentSync:SendEnvelope", ioEx.ToString()));
				SOSLogWriter.WriteLogEntry(sde.DmsInfo.Company, string.Format(EASyncStrings.MethodExceptionWithDetail, "EasyAttachmentSync:SendEnvelope", ioEx.ToString()));
				return false;
			}
			catch (Exception ex)
			{
				EASyncEngine.WriteMessageInEventLog(string.Format(EASyncStrings.MethodExceptionWithDetail, "EasyAttachmentSync:SendEnvelope", ex.ToString()));
				SOSLogWriter.WriteLogEntry(sde.DmsInfo.Company, string.Format(EASyncStrings.MethodExceptionWithDetail, "EasyAttachmentSync:SendEnvelope", ex.ToString()));
				return false;
			}

			return true;
		}

		///<summary>
		/// Scrive nell'eventlog l'eventuale ultimo errore registrato nel SOSDiagnostic
		///</summary>
		//--------------------------------------------------------------------------------
		private void WriteLastMessageInDiagnostic(SOSProxyWrapper sosProxy)
		{
			IDiagnosticItems dItems = sosProxy.SosDiagnostic.AllMessages(DiagnosticType.Error);
			if (dItems.Count == 0)
				return;

			IDiagnosticItem item = dItems[dItems.Count - 1];
			if (item == null || string.IsNullOrWhiteSpace(item.FullExplain))
				return;
			
			string msg = item.FullExplain;
			if (item.ExtendedInfo != null && item.ExtendedInfo.Count > 0)
				msg += item.ExtendedInfo[0].Info;

			EASyncEngine.WriteMessageInEventLog(msg);
		}

		///<summary>
		/// Esegue la spedizione semplice del file zip
		///</summary>
		//--------------------------------------------------------------------------------
		private bool SendSemplice(SOSProxyWrapper sosProxy, string zipFilePath)
		{
			RispostaSpedizione rispostaSped;
			// eseguo la spedizione semplice in SOS (non metto il try/catch perche' gia' gestito a monte)
			if (!sosProxy.Semplice(Path.GetFileName(zipFilePath), ReadFile(zipFilePath), out rispostaSped))
			{
				EASyncEngine.WriteMessageInEventLog(string.Format(EASyncStrings.SOSErrorSemplice, zipFilePath));
				WriteLastMessageInDiagnostic(sosProxy);
				return false;
			}

			// se la spedizione semplice tornasse gli errori IMPOSSIBILESALVARE/SPEDIZIONEGIAPRESENTE cmq
			// tornerebbe il valore RispostaSpedizioneEnum.KO, quindi basta tornare un bool
			return (rispostaSped != null) ? (rispostaSped.Risposta == RispostaSpedizioneEnum.OK) : false;
		}

		///<summary>
		/// Esegue la spedizione frammentata del file zip
		///</summary>
		//--------------------------------------------------------------------------------
		private bool SendFrammentata(SOSProxyWrapper sosProxy, string zipFilePath, long fileLength, string companyName)
		{
			// calcolo il numero di chunk in cui suddividere il file, arrotondando per eccesso (devo convertire in decimal, altrimenti non arrotonda correttamente)
			decimal nr = fileLength / Convert.ToDecimal(chunkDimensionInBytes);
			int nrChunks = Convert.ToInt32(Decimal.Ceiling(nr));

			SOSLogWriter.AppendText(companyName, string.Format("Frammentata needs {0} chunk", nrChunks.ToString()));

			// Il numeroChunk è prioritario rispetto alla dimensione. Qualora entrambi siano valorizzati e > 0 sarà considerato solo il numeroChunk.
			Ticket tkt;
			if (!sosProxy.Sequenza(nrChunks, Convert.ToInt32(chunkDimensionInBytes), out tkt))
			{
				EASyncEngine.WriteMessageInEventLog(EASyncStrings.SOSErrorSequenza);
				WriteLastMessageInDiagnostic(sosProxy);
				SOSLogWriter.WriteLogEntry(companyName, string.Format("Error during '{0}' web-method. Please check MA Server in EventViewer", "Sequenza"), "SendFrammentata");
				return false;
			}

			// suddivido il file in blocchi da inviare
			List<Byte[]> blocks = SplitFileInBlocks(zipFilePath);
			// se il numero dei blocchi e' diverso da quello atteso non procedo
			if (blocks.Count != nrChunks)
				return false;

			// result globale spedizione
			bool sendResult = false;
			RispostaSpedizione rispostaSped;

			// scorro i chunk ed invio il singolo frammento corrispondente
			for (int i = 0; i < tkt.Chunks.Count; i++)
			{
				TicketChunk tChunk = tkt.Chunks[i];

				SOSLogWriter.AppendText(companyName, string.Format("Starting Frammentata - chunk nr. {0}", (i + 1).ToString()));

				if (!sosProxy.Frammentata(tkt.Guid, tChunk.Guid, Path.GetFileName(zipFilePath), blocks.ElementAt(tChunk.Posizione - 1), out rispostaSped))
				{
					SOSLogWriter.AppendText(companyName, string.Format("Frammentata - chunk nr. {0} completed with errors", (i + 1).ToString()));
					EASyncEngine.WriteMessageInEventLog(string.Format(EASyncStrings.SOSErrorFrammentata, zipFilePath, tChunk.Posizione.ToString()));
					WriteLastMessageInDiagnostic(sosProxy);
					return false;
				}

				SOSLogWriter.AppendText(companyName, string.Format("Frammentata - chunk nr. {0} successfully completed", (i + 1).ToString()));

				// se la spedizione del frammento tornasse gli errori IMPOSSIBILESALVARE/SPEDIZIONEGIAPRESENTE cmq
				// tornerebbe il valore RispostaSpedizioneEnum.KO, quindi basta tornare un bool
				if (rispostaSped.Risposta == RispostaSpedizioneEnum.OK)
				{
					// se la spedizione del frammento e' andata a buon fine continuo
					sendResult = true;
					continue;
				}

				bool attemptResult = false;
				RispostaSpedizione attemptRisposta;

				// se la spedizione ha tornato un errore, parto con i tentativi (9 + 1 gia' eseguito = 10)
				if (rispostaSped.Risposta == RispostaSpedizioneEnum.KO)
				{
					for (int k = 0; k < 9; k++)
					{
						if (!sosProxy.Frammentata(tkt.Guid, tChunk.Guid, Path.GetFileName(zipFilePath), blocks[tChunk.Posizione - 1], out attemptRisposta))
						{
							EASyncEngine.WriteMessageInEventLog(string.Format(EASyncStrings.SOSErrorFrammentataAttempt, zipFilePath, tChunk.Posizione.ToString(), k.ToString()));
							WriteLastMessageInDiagnostic(sosProxy);
							attemptResult = false;
							break;
						}

						// se la spedizione del frammento tornasse gli errori IMPOSSIBILESALVARE/SPEDIZIONEGIAPRESENTE cmq
						// tornerebbe il valore RispostaSpedizioneEnum.KO, quindi basta tornare un bool
						if (attemptRisposta.Risposta == RispostaSpedizioneEnum.OK)
						{
							attemptResult = true;
							break; // sono riuscito a spedirlo con l'ennesimo tentativo e faccio break
						}
					}
				}

				if (attemptResult)
					sendResult = true;
				else
				{
					sendResult = false;
					break; // non sono riuscita ad inviarlo neppure con i successivi tentativi: blocco qui l'intero loop
				}
			}

			return sendResult;
		}

		///<summary>
		/// ReadFile
		/// Legge TUTTO il file e ritorna il contenuto in un byte[]
		///</summary>
		//--------------------------------------------------------------------------------
		private byte[] ReadFile(string filePath)
		{
			byte[] buffer;

			using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
			{
				int length = (int)fs.Length;  // get file length
				buffer = new byte[length];    // create buffer
				int count;                    // actual number of bytes read
				int sum = 0;                  // total number of bytes read

				// read until Read method returns 0 (end of the stream has been reached)
				while ((count = fs.Read(buffer, sum, length - sum)) > 0)
					sum += count;  // sum is a buffer offset for next reading
			}

			return buffer;
		}

		///<summary>
		/// SplitFileInBlocks
		/// Esegue lo split del file in blocchi di 1MB e ritorna una lista di Byte[]
		///</summary>
		//--------------------------------------------------------------------------------
		private List<Byte[]> SplitFileInBlocks(string filePath)
		{
			List<Byte[]> blocks = new List<Byte[]>();

			byte[] buffer;
			int div = Convert.ToInt32(chunkDimensionInBytes);

			using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
			{
				int length = (int)fs.Length;	// get file length
				int count;						// actual number of bytes read
				int sum = 0;					// total number of bytes read

				do
				{
					int idx = ((length - sum) > div) ? div : (length - sum);
					buffer = new Byte[idx];

					count = fs.Read(buffer, 0, idx);
					sum += count;  // sum is a buffer offset for next reading
					if (count > 0)
						blocks.Add(buffer);
				}
				while (count > 0);
			}

			return blocks;
		}
	}

	# region SOSDocElement
	///<summary>
	/// Classe che identifica un generico elemento presente nella Queue dei documenti da spedire
	///</summary>
	//================================================================================
	public class SOSDocElement
	{
		private DmsDatabaseInfo dmsInfo;
		private List<int> attachmentIds;
		private int loginId;

		//--------------------------------------------------------------------------------
		public DmsDatabaseInfo DmsInfo { get { return dmsInfo; } }
		//--------------------------------------------------------------------------------
		public List<int> AttachmentIds { get { return attachmentIds; } }
		//--------------------------------------------------------------------------------
		public int LoginId { get { return loginId; } }

		//--------------------------------------------------------------------------------
		public SOSDocElement(DmsDatabaseInfo dmsInfo, List<int> attachmentIds, int loginId)
		{
			this.dmsInfo = dmsInfo;
			this.attachmentIds = attachmentIds;
			this.loginId = loginId;
		}
	}
	# endregion

	# region SOSDocQueue
	///<summary>
	/// Classe generica che implementa una queue e intercetta i changed della struttura
	///</summary>
	//================================================================================
	public class SOSDocQueue<T>
	{
		private readonly Queue<T> queue = new Queue<T>();
		public event EventHandler Changed;

		//--------------------------------------------------------------------------------
		public int Count { get { return queue.Count; } }

		//--------------------------------------------------------------------------------
		protected virtual void OnChanged()
		{
			if (Changed != null)
				Changed(this, EventArgs.Empty);
		}

		//--------------------------------------------------------------------------------
		public virtual void Enqueue(T item)
		{
			queue.Enqueue(item);
			OnChanged();
		}

		//--------------------------------------------------------------------------------
		public virtual T Dequeue()
		{
			T item = queue.Dequeue();
			//OnChanged(); // non scateno l'evento quando tolgo dalla coda!!!
			return item;
		}
	}
	# endregion
}
