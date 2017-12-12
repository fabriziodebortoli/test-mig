using System;
using System.Collections.Generic;
using System.Data;

using Microarea.EasyAttachment.BusinessLogic;
using Microarea.Framework.TBApplicationWrapper;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Data.DatabaseLayer;
using Microarea.TaskBuilderNet.Interfaces.Model;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.EasyAttachment.Components
{
	///<summary>
	/// Possibili valori di ritorno
	/// -1 : errore + msg trappato
	///  0 : non ci sono record da elaborare
	///  1 : ok
	///</summary>

	/// <summary>
	/// Classe che si occupa dell'aggiornamento del valore della colonna TBGuid 
	/// nella tabella DMS_ErpDocument (miglioria 6185)
	/// </summary>
	//================================================================================
	public class GuidUpdater
	{
		private string internalString = string.Empty;
		private DMSOrchestrator dmsOrchestrator;

		private List<ErpDocumentNs> documentsToUpdate = new List<ErpDocumentNs>();

		// lista di documenti orfani da eliminare
		// (la cui chiave non e' presente nel db di ERP perche' il documento e' stato eliminato, magari via batch)
		private List<int> orphanErpDocIdsList = new List<int>();

		public string ElaborationMessage { get; private set; }
		public int NResult { get; private set; }
		public int NrOrphanErpDocuments { get { return orphanErpDocIdsList.Count; } }
		public int NrRecordsUpdated { get; private set; }

		//----------------------------------------------------------------------------
		public GuidUpdater(DMSOrchestrator dmsOrchestrator)
		{
			this.dmsOrchestrator = dmsOrchestrator;
		}

		/// <summary>
		/// Entry-point per richiamare l'aggiornamento
		/// </summary>
		/// <remarks>
		/// Valori di ritorno
		/// -1 : errore + msg trappato
		///  0 : non ci sono record da elaborare
		///  1 : ok
		/// </remarks>
		//----------------------------------------------------------------------------
		public void Execute()
		{
			NrRecordsUpdated = 0;
			ElaborationMessage = string.Empty;

			string lockErr = string.Empty;

			if (!dmsOrchestrator.LockManager.LockRecord("DMS_ErpDocument", "GuidUpdater", dmsOrchestrator.LockContext, ref lockErr))
			{
				ElaborationMessage = lockErr;
				NResult = -1;
				return;
			}

			// prima carico le righe che hanno il TBGuid da aggiornare
			if (!LoadErpDocuments())
			{
				dmsOrchestrator.LockManager.UnlockRecord("DMS_ErpDocument", "GuidUpdater", dmsOrchestrator.LockContext);
				NResult = -1;
				return;
			}

			// se non ho trovato record da aggiornare torno subito
			if (documentsToUpdate.Count == 0)
			{
				ElaborationMessage = Strings.NoRecordsToUpdate;
				dmsOrchestrator.LockManager.UnlockRecord("DMS_ErpDocument", "GuidUpdater", dmsOrchestrator.LockContext);
				NResult = 0;
				return;
			}

			// per ogni documento estratto vado ad arricchire le sue informazioni aggiuntive
			// leggendo sia dal database di EA che da quello di ERP
			bool result = LoadAdditionalInfoForErpDocument() && LoadDataFromERP();
			if (result)
			{
				result = UpdateTBGuidColumn(); // aggiorno la colonna TbGuid della DMS_ErpDocument con il valore letto dal db di ERP

				// se ci sono dei documenti orfani che non esistono in ERP vado ad eliminare tutti i loro riferimenti
				if (orphanErpDocIdsList.Count > 0)
					DeleteOrphansErpDocuments();
			}

			dmsOrchestrator.LockManager.UnlockRecord("DMS_ErpDocument", "GuidUpdater", dmsOrchestrator.LockContext);

			ElaborationMessage = internalString;
			NResult = result ? 1 : -1;
		}

		/// <summary>
		/// Caricamento delle righe della tabella DMS_ErpDocument che necessitano di 
		/// un aggiornamento, ovvero dove la colonna TBGuid ha valori nulli o vuoti (0x00)
		/// </summary>
		//----------------------------------------------------------------------------
		private bool LoadErpDocuments()
		{
			string docNs = string.Empty;
			string pkValue = string.Empty;
			int erpDocId = -1;

			try
			{
				using (TBConnection myConnection = new TBConnection(dmsOrchestrator.DMSConnectionString, DBMSType.SQLSERVER))
				{
					myConnection.Open();

					using (IDbCommand myCommand = myConnection.CreateCommand())
					{
						myCommand.CommandText = @"SELECT [ErpDocumentID], [DocNamespace], [PrimaryKeyValue] FROM [DMS_ErpDocument] 
												WHERE [TBGuid] IS NULL OR [TBGuid] = 0x00 OR [TBGuid] = '00000000-0000-0000-0008-000000000000'";

						using (IDataReader dr = myCommand.ExecuteReader())
						{
							while (dr.Read())
							{
								docNs = dr["DocNamespace"].ToString();
								pkValue = dr["PrimaryKeyValue"].ToString();
								erpDocId = (int)dr["ErpDocumentID"];

								ErpDocumentNs erpDocNs = documentsToUpdate.Find(d => string.Compare(d.DocNamespace, docNs, StringComparison.InvariantCultureIgnoreCase) == 0);
								if (erpDocNs == null)
								{
									// se non ho gia' trovato un doc con quel ns lo creo e lo aggiungo alla lista
									erpDocNs = new ErpDocumentNs();
									erpDocNs.DocNamespace = docNs;
									documentsToUpdate.Add(erpDocNs);
								}

								// aggiunto i dettagli del singolo erpdocid per quel ns
								SingleErpDocument singleDoc = new SingleErpDocument();
								singleDoc.ErpDocumentID = erpDocId;
								singleDoc.PrimaryKeyValue = pkValue;
								erpDocNs.Documents.Add(singleDoc);
							}
						}
					}
				}
			}
			catch (Exception e)
			{
				internalString = string.Format(Strings.ErrorInMethod, "LoadErpDocuments", e.Message);
				return false;
			}

			return true;
		}

		/// <summary>
		/// Per ogni ErpDocument estratto vado ad individuare le sue informazioni aggiuntive,
		/// come il CollectionId, il CollectorId ed il nome della tabella di ERP (dalla quale
		/// andare poi a recuperare il TBGuid da aggiornare)
		/// </summary>
		//----------------------------------------------------------------------------
		private bool LoadAdditionalInfoForErpDocument()
		{
			try
			{
				string collectorName = string.Empty;
				string collectionName = string.Empty;

				string selectQuery = @"SELECT TOP 1 [PhysicalName] FROM [DMS_CollectionsFields] 
										INNER JOIN [DMS_Collection] ON [DMS_Collection].[CollectionID] = [DMS_CollectionsFields].[CollectionID] 
										INNER JOIN [DMS_Collector] ON [DMS_Collection].[CollectorID] = [DMS_Collector].[CollectorID]
										WHERE [DMS_CollectionsFields].[FieldGroup] = 0 AND [DMS_Collector].[Name] = '{0}' AND [DMS_Collection].[Name] = '{1}'";

				using (TBConnection myConnection = new TBConnection(dmsOrchestrator.DMSConnectionString, DBMSType.SQLSERVER))
				{
					myConnection.Open();

					using (IDbCommand myCommand = myConnection.CreateCommand())
					{
						foreach (ErpDocumentNs doc in documentsToUpdate)
						{
							NameSpace ns = new NameSpace(doc.DocNamespace); // Document.ERP.CustomersSuppliers.Documents.Customers
							collectorName = ns.Module; // token 3 
							collectionName = ns.Document; // token 5 

							myCommand.CommandText = string.Format(selectQuery, collectorName, collectionName);
							using (IDataReader dr = myCommand.ExecuteReader())
								if (dr.Read())
								{
									string physicalName = dr["PhysicalName"].ToString();
									doc.TableName = physicalName.Substring(0, physicalName.IndexOf("."));
								}
						}
					}
				}
			}
			catch (Exception e)
			{
				internalString = string.Format(Strings.ErrorInMethod, "LoadAdditionalInfoForErpDocument", e.Message);
				return false;
			}
			return true;
		}

		//----------------------------------------------------------------------------
		private bool LoadDataFromERP()
		{
			MSqlRecord sqlRecord = null;
			MSqlTable sqlTable = null;

			try
			{
				foreach (ErpDocumentNs erpDoc in documentsToUpdate)
				{
					if (string.IsNullOrWhiteSpace(erpDoc.TableName))
						continue;

					try
					{
						sqlRecord = new MSqlRecord(erpDoc.TableName);
					}
					catch (Exception)
					{
						continue;
					}
					
					sqlTable = new MSqlTable(sqlRecord);

					sqlTable.Open();
					sqlTable.Select.AddColumn("TBGuid", (MDataObj)sqlRecord.GetField("TBGuid").DataObj);

					for (int i = 0; i < sqlRecord.PrimaryKeyFields.Count; i++)
					{
						IRecordField pkField = (IRecordField)sqlRecord.PrimaryKeyFields[i];
						sqlTable.Where.AddParameter(string.Format("P{0}", i), (MDataObj)pkField.DataObj); // AddParam
						sqlTable.Where.AddColumn((MDataObj)pkField.DataObj); // AddFilterColumn
					}

					foreach (SingleErpDocument doc in erpDoc.Documents)
					{
						// imposto tutti segmenti della PK al sqlrecord (la stringa viene formattata in automatico)
						sqlRecord.SetPrimaryKeyNameValue(doc.PrimaryKeyValue);

						for (int i = 0; i < sqlRecord.PrimaryKeyFields.Count; i++)
						{
							IRecordField pkField = (IRecordField)sqlRecord.PrimaryKeyFields[i];
							sqlTable.Where.Parameter(string.Format("P{0}", i), (MDataObj)pkField.DataObj); // SetParamValue
						}

						sqlTable.ExecuteQuery();

						if (sqlTable.IsEOF())
						{
							orphanErpDocIdsList.Add(doc.ErpDocumentID);
							continue;
						}

						while (!sqlTable.IsEOF())
						{
							doc.ERPTBGuid = (Guid)sqlRecord.GetValue("TBGuid");
							sqlTable.NextResult();
						}
					}

					sqlTable.Close();
					sqlTable.Dispose();
					sqlRecord.Dispose();
				}
			}
			catch (Exception e)
			{
				internalString = string.Format(Strings.ErrorInMethod, "LoadDataFromERP", e.Message);
				return false;
			}

			return true;
		}

		/// <summary>
		/// Aggiorno il valore della colonna TBGuid nella DMS_ErpDocument con il valore letto
		/// dalla tabella di ERP
		/// </summary>
		//----------------------------------------------------------------------------
		private bool UpdateTBGuidColumn()
		{
			try
			{
				using (TBConnection myConnection = new TBConnection(dmsOrchestrator.DMSConnectionString, DBMSType.SQLSERVER))
				{
					myConnection.Open();

					using (IDbCommand myCommand = myConnection.CreateCommand())
					{
						foreach (ErpDocumentNs erpDoc in documentsToUpdate)
						{
							foreach (SingleErpDocument doc in erpDoc.Documents)
							{
								myCommand.CommandText = string.Format("UPDATE [DMS_ErpDocument] SET [TBGuid] = '{0}' WHERE [ErpDocumentID] = {1}", doc.ERPTBGuid, doc.ErpDocumentID);
								myCommand.ExecuteNonQuery();
								NrRecordsUpdated++;
							}
						}
					}
				}
			}
			catch (Exception e)
			{
				internalString = string.Format(Strings.ErrorInMethod, "UpdateTBGuidColumn", e.Message);
				return false;
			}

			return true;
		}

		/// <summary>
		/// Elimino tutti i riferimenti agli ErpDocumentId che non hanno una corrispondente
		/// chiave nel database di ERP (nel caso di documenti eliminati via batch)
		/// </summary>
		//----------------------------------------------------------------------------
		private void DeleteOrphansErpDocuments()
		{
			try
			{
				foreach (int erpDocId in orphanErpDocIdsList)
					dmsOrchestrator.ArchiveManager.DeleteErpDocument(erpDocId);
			}
			catch (Exception e)
			{
				System.Diagnostics.Debug.WriteLine(string.Format(Strings.ErrorInMethod, "DeleteOrphansErpDocuments", e.Message));
			}
		}
	}

	#region Classi di appoggio per memorizzare le strutture dati da aggiornare
	/// <summary>
	/// Oggetto per memorizzare le informazioni generiche di ogni documento di ERP da aggiornare
	/// </summary>
	//================================================================================
	public class ErpDocumentNs
	{
		public string DocNamespace { get; set; }
		public string PrimaryKeyValue { get; set; }
		public int CollectionID { get; set; } // letto dalla DMS_Collection
		public int CollectorID { get; set; } // letto dalla DMS_Collector
		public string TableName { get; set; } // letto dalla DMS_CollectionFields (token 1 della colonna PhysicalName)

		public List<SingleErpDocument> Documents = new List<SingleErpDocument>(); // lista di singoli ERPDocId da aggiornare
	}

	/// <summary>
	/// Oggetto per memorizzare le informazioni di ogni documento di ERP da aggiornare
	/// </summary>
	//================================================================================
	public class SingleErpDocument
	{
		public int ErpDocumentID { get; set; }
		public string PrimaryKeyValue { get; set; }
		public Guid ERPTBGuid { get; set; } // letto dal db di ERP	
	}
	#endregion
}
