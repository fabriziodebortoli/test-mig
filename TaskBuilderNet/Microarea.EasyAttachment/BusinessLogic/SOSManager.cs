using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

using Microarea.EasyAttachment.Components;
using Microarea.EasyAttachment.Core;
using Microarea.Framework.TBApplicationWrapper; // it is not unnecessary! se si leva non compila piu' niente!
using Microarea.TaskBuilderNet.Core.CoreTypes;
using Microarea.TaskBuilderNet.Core.DiagnosticManager;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Core.WebServicesWrapper;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.EasyAttachment.BusinessLogic
{
	/// <summary>
	/// Filtri utilizzati nel SosMonitor relativi al singolo documento
	/// </summary>
	//---------------------------------------------------------------------------------------------------
	[Flags]
	public enum DocFilterType
	{
		ToSend = 1,
		Sent = 2,
		DocTemp = 4,
		DocStd = 8,
		DocRdy = 16,
		DocSign = 32,
		DocKO = 64,
		DocToResend = 128
	}

	/// <summary>
	/// Filtri utilizzati nel SosMonitor relativi all'envelope
	/// </summary>
	//---------------------------------------------------------------------------------------------------
	[Flags]
	public enum EnvFilterType
	{
		ToSend = 1,
		Sent = 2,
		Error = 4
	}

	//---------------------------------------------------------------------------------------------------
	public enum CanBeSentToSOSType
	{
		BeSent,
		NoPDFA,
		EmptySosKeyValue
	}

    /// <summary>
    /// Gestione filtri gestionali
    /// </summary>
    //---------------------------------------------------------------------------------------------------
    public class ERPFieldRule
    { 
        private string fieldName = string.Empty;

		public string FieldName { get { return fieldName; } }
		public string FromValue { get; set; }
		public string ToValue { get; set; }

        //---------------------------------------------------------------------------------------------------	
        public ERPFieldRule(string fieldName)
        {
            this.fieldName = fieldName;
        }
    }

	//---------------------------------------------------------------------------------------------------
	public class ERPFieldRuleList
	{
		public List<ERPFieldRule> ERPFieldsRules = new List<ERPFieldRule>();

		//-----------------------------------------------------------------------------------------       
		public void AddBetweenERPFilter(string fieldName, MDataObj fromDataObj, MDataObj toDataObj)
		{
			FieldData fromData = new FieldData(fromDataObj.DataType);
			fromData.DataValue = fromDataObj.Value;

			FieldData toData = new FieldData(toDataObj.DataType);
			toData.DataValue = toDataObj.Value;

			ERPFieldRule fieldRule = ERPFieldsRules.Find(a => a.FieldName.CompareNoCase(fieldName));
			if (fieldRule == null)
				fieldRule = new ERPFieldRule(fieldName);

			fieldRule.FromValue = fromData.StringValue; //è il valore nativo di un indice del DMS
			fieldRule.ToValue = toData.StringValue; //è il valore nativo di un indice del DMS

			ERPFieldsRules.Add(fieldRule);
		}

		//-----------------------------------------------------------------------------------------       
		public void AddFromERPFilter(string fieldName, MDataObj dataObj)
		{
			FieldData filedData = new FieldData(dataObj.DataType);
			filedData.DataValue = dataObj.Value;

			ERPFieldRule fieldRule = ERPFieldsRules.Find(a => a.FieldName.CompareNoCase(fieldName));
			if (fieldRule == null)
				fieldRule = new ERPFieldRule(fieldName);

			fieldRule.FromValue = filedData.StringValue; //è il valore nativo di un indice del DMS
			fieldRule.ToValue = string.Empty; //è il valore nativo di un indice del DMS

			ERPFieldsRules.Add(fieldRule);
		}

		//-----------------------------------------------------------------------------------------       
		public void AddToERPFilter(string fieldName, MDataObj dataObj)
		{
			FieldData filedData = new FieldData(dataObj.DataType);
			filedData.DataValue = dataObj.Value;

			ERPFieldRule fieldRule = ERPFieldsRules.Find(a => a.FieldName.CompareNoCase(fieldName));
			if (fieldRule == null)
				fieldRule = new ERPFieldRule(fieldName);

			fieldRule.FromValue = string.Empty;
			fieldRule.ToValue = filedData.StringValue;

			ERPFieldsRules.Add(fieldRule);
		}

		//-----------------------------------------------------------------------------------------       
		public void AddERPFilter(string fieldName, string fromValue, string toValue)
		{
			ERPFieldRule fieldRule = null;
			foreach (ERPFieldRule fr in ERPFieldsRules)
			{
				if (string.Compare(fr.FieldName, fieldName) == 0)
					fieldRule = fr;
			}
			if (fieldRule == null)
				fieldRule = new ERPFieldRule(fieldName);

			fieldRule.FromValue = fromValue;
			fieldRule.ToValue = toValue;

			ERPFieldsRules.Add(fieldRule);
		}

		//-----------------------------------------------------------------------------------------
		public bool UseERPFilter()
		{
			return ERPFieldsRules.Count > 0;
		}

		//-----------------------------------------------------------------------------------------
		public void Clear()
		{
			ERPFieldsRules.Clear();
		}
	}

    /// <summary>
    /// Filtri utilizzati nel SosMonitor relativi ai documenti da inviare
    /// </summary>
    //---------------------------------------------------------------------------------------------------
	public class SosSearchRules
	{
		//public List<int> CollectionIDs = null;	//data un classe documentale possiamo avere più collection
		public string SosTaxJournal { get; set; }
		public List<string> SosDocumentTypes { get; set; }
		public ERPSOSDocumentType ERPSosDocumentType { get; set; }
		public List<StatoDocumento> SosDocumentStatus { get; set; }
        public string FiscalYear { get; set; }
		public bool OnlyMainDoc = false;

        public ERPFieldRuleList ERPFieldsRuleList { get; set; }

        //-----------------------------------------------------------------------------------------       
        public void AddBetweenERPFilter(string fieldName, MDataObj fromDataObj, MDataObj toDataObj)
        {
            if (ERPFieldsRuleList == null)
                ERPFieldsRuleList = new ERPFieldRuleList();

            ERPFieldsRuleList.AddBetweenERPFilter(fieldName, fromDataObj, toDataObj);
        }

        //-----------------------------------------------------------------------------------------       
        public void AddERPFilter(string fieldName, string fromValue, string toValue)
        {
            if (ERPFieldsRuleList == null)
                ERPFieldsRuleList = new ERPFieldRuleList();

            ERPFieldsRuleList.AddERPFilter(fieldName, fromValue, toValue);
        }

        //-----------------------------------------------------------------------------------------
        public bool UseERPFilter() 
        {
            if (ERPFieldsRuleList == null)
                ERPFieldsRuleList = new ERPFieldRuleList();

            return ERPFieldsRuleList.UseERPFilter();
        }

        //-----------------------------------------------------------------------------------------
        public void Clear()
        {
            if (ERPFieldsRuleList == null)
                ERPFieldsRuleList = new ERPFieldRuleList();

			ERPFieldsRuleList.Clear();
        }
    }

	///<summary>
	/// Classe manager per il SOSConnector
	/// Mette a disposizione i metodi che implementano funzionalita' per l'interfacciamento
	/// e la gestione della conservazione sostitutiva
	///</summary>
	//================================================================================
	public class SOSManager : BaseManager
	{
		private DMSModelDataContext dc = null;
		private SOSProxyWrapper sosProxy = null; // puntatore al SOSProxyWrapper
		private CoreSOSManager coreSosManager = null;

		private bool startSearchSOSDocuments = false;

        public DMSDocOrchestrator DMSDocOrchestrator = null;

		//event for SOSDocuments searching
		public event EventHandler SearchResultCleared;
		public event EventHandler SearchFinished;
		public event EventHandler<AddRowInResultEventArgs> OnAddRowToResult;
		public event EventHandler AdjustAttachmentsFinished;
		// events for SOSSender
		public event EventHandler<SOSEventArgs> SOSOperationCompleted;
		public event EventHandler SOSSendFinished;

		public DMSModelDataContext DMSDataContext { get { return dc; } }

		// Properties
		//--------------------------------------------------------------------------------
		public SOSProxyWrapper SOSProxyW
		{
			get
			{
				if (sosProxy == null)
				{
					sosProxy = new SOSProxyWrapper(BasePathFinder.BasePathFinderInstance.SOSProxyUrl);
					sosProxy.Init
						(
						SOSConfigurationState.SOSConfiguration.KeeperCode,
						SOSConfigurationState.SOSConfiguration.SubjectCode,
						SOSConfigurationState.SOSConfiguration.MySOSUser,
						Crypto.Decrypt(SOSConfigurationState.SOSConfiguration.MySOSPassword),
						DMSOrchestrator.SOSConfigurationState.SOSConfiguration.SOSWebServiceUrl
						);
				}

				return sosProxy;
			}
		}

		//-------------------------------------------------------------------------------
		public SOSConfigurationState SOSConfigurationState { get { return coreSosManager.SOSConfigurationState; } set { coreSosManager.SOSConfigurationState = value; } }

		///<summary>
        /// Constructor called by DMSOrchestrator
		///</summary>
		//-------------------------------------------------------------------------------
		public SOSManager(DMSOrchestrator dmsOrch)
		{
			DMSOrchestrator = dmsOrch;
			ManagerName = "SOSManager";

			//creo una nuova connessione per avere i dati sempre aggiornati, altrimenti dovre fare ogni volta un refresh con perdita di tempo
			dc = new DMSModelDataContext(DMSOrchestrator.DMSConnectionString);

			// istanzio il CoreSOSManager passando tutte le informazioni che servono per creare gli zip
			coreSosManager = new CoreSOSManager
				(
				dc,
				DMSOrchestrator.SosConnectorTempPath,
				DMSOrchestrator.SOSSubjectCode,
				DMSOrchestrator.UtilsManager.CompanyName,
				DMSOrchestrator.DMSConnectionString,
				DMSOrchestrator.LoginId
				);
		}
	
		/// <summary>
		/// Ritorna l'oggetto DMS_SOSDocument con PK = AttachmentId
		/// (la lascio privata ma okkio che ritorna anche il contenuto
		/// della colonna PdfABinary ed e' pesante)
		/// </summary>
		//---------------------------------------------------------------------
		private DMS_SOSDocument GetSOSDocument(int attachmentId)
		{
			var sosDoc = (from sd in dc.DMS_SOSDocuments
						  where sd.AttachmentID == attachmentId
						  select sd);

			return (sosDoc != null && sosDoc.Any()) ? sosDoc.Single() : null;
		}

		///<summary>
		/// Fa una query nella tabella DMS_SOSDocument ed estrae tutte le colonne (tranne la PDFABinary)
		/// corrispondenti a quell'AttachmentId e ritorna una struttura in memoria
		///</summary>
		//-------------------------------------------------------------------------------
		public SOSDocumentInfo GetSosDocumentInfo(int attachmentId)
		{
			SOSDocumentInfo sosDocInfo = null;

			try
			{
				using (SqlConnection myConnection = new SqlConnection(DMSOrchestrator.DMSConnectionString))
				{
					myConnection.Open();

					using (IDbCommand myCommand = myConnection.CreateCommand())
					{
						myCommand.CommandText = @"SELECT [FileName], [Size], [DescriptionKeys], [HashCode], [DocumentStatus], [AbsoluteCode],
							[LotID], [ArchivedDate], [RegistrationDate], [EnvelopeID], [TaxJournal], [DocumentType], [FiscalYear], [SendingType]
							FROM DMS_SOSDocument WHERE AttachmentId = " + attachmentId;

						using (IDataReader dr = myCommand.ExecuteReader())
						{
							if (dr.Read())
							{
								sosDocInfo = new SOSDocumentInfo(attachmentId, this.DMSOrchestrator);

								sosDocInfo.FileName = (dr["FileName"] == DBNull.Value) ? string.Empty : dr["FileName"].ToString();
								sosDocInfo.Size = (dr["Size"] == DBNull.Value) ? 0 : (long)dr["Size"];

								sosDocInfo.DescriptionKeys = (dr["DescriptionKeys"] == DBNull.Value) ? string.Empty : dr["DescriptionKeys"].ToString();
								sosDocInfo.HashCode = (dr["HashCode"] == DBNull.Value) ? string.Empty : dr["HashCode"].ToString();

								StatoDocumento docStatus = StatoDocumento.IDLE;
								if (Enum.TryParse(dr["DocumentStatus"].ToString(), out docStatus))
									sosDocInfo.DocumentStatus = docStatus;

								sosDocInfo.AbsoluteCode = (dr["AbsoluteCode"] == DBNull.Value) ? string.Empty : dr["AbsoluteCode"].ToString();
								sosDocInfo.LotID = (dr["LotID"] == DBNull.Value) ? string.Empty : dr["LotID"].ToString();

								sosDocInfo.ArchivedDate = (dr["ArchivedDate"] == DBNull.Value) ? (DateTime)SqlDateTime.MinValue : Convert.ToDateTime(dr["ArchivedDate"]);
								sosDocInfo.RegistrationDate = (dr["RegistrationDate"] == DBNull.Value) ? (DateTime)SqlDateTime.MinValue : Convert.ToDateTime(dr["RegistrationDate"]);

								sosDocInfo.EnvelopeID = (dr["EnvelopeID"] == DBNull.Value) ? 0 : (int)dr["EnvelopeID"];

								sosDocInfo.TaxJournal = (dr["TaxJournal"] == DBNull.Value) ? string.Empty : dr["TaxJournal"].ToString();
								sosDocInfo.DocumentType = (dr["DocumentType"] == DBNull.Value) ? string.Empty : dr["DocumentType"].ToString();
								sosDocInfo.FiscalYear = (dr["FiscalYear"] == DBNull.Value) ? string.Empty : dr["FiscalYear"].ToString();

								SendingType sendingType = SendingType.WebService;
								if (Enum.TryParse(dr["SendingType"].ToString(), out sendingType))
									sosDocInfo.SendingType = sendingType;
							}
						}
					}
				}
			}
			catch (Exception)
			{
			}

			return sosDocInfo;
		}

		///<summary>
		/// Salva un record nella tabella DMS_SOSConfiguration, sulla base delle informazioni
		/// presenti nella classe locale DocClassesState
		///</summary>
		//--------------------------------------------------------------------------------
		public bool SaveSOSConfiguration(SOSConfigurationState sosConfigurationState)
		{
			bool result = false;

			if (sosConfigurationState == null)
				return result;

			try
			{
				DMS_SOSConfiguration insertRecord = sosConfigurationState.SOSConfiguration;
				if (insertRecord == null)
					return result;

				string lockMsg = string.Empty;

				// seleziono l'unico record con ParamID = 0
				var recID0 = (from rec in dc.DMS_SOSConfigurations where rec.ParamID == 0 select rec);
				DMS_SOSConfiguration sosConf = (recID0 != null && recID0.Any()) ? (DMS_SOSConfiguration)recID0.Single() : null;

				if (sosConf != null)
				{
					if (DMSOrchestrator.LockManager.LockRecord(sosConf, DMSOrchestrator.LockContext, ref lockMsg))
					{
						sosConf.SubjectCode = insertRecord.SubjectCode;
						sosConf.KeeperCode = insertRecord.KeeperCode;
						sosConf.MySOSUser = insertRecord.MySOSUser;
						sosConf.MySOSPassword = insertRecord.MySOSPassword;
						sosConf.SOSWebServiceUrl = insertRecord.SOSWebServiceUrl;
						sosConf.AncestorCode = insertRecord.AncestorCode;
						sosConf.ChunkDimension = insertRecord.ChunkDimension;
						sosConf.EnvelopeDimension = insertRecord.EnvelopeDimension;
						sosConf.FTPSend = insertRecord.FTPSend;
						sosConf.FTPSharedFolder = insertRecord.FTPSharedFolder;
						sosConf.FTPUpdateDayOfWeek = insertRecord.FTPUpdateDayOfWeek;

                        // arricchisco la lista delle classi con le informazioni aggiuntive che carico dai file SosConfiguration.xml del modulo SOSConnector
                        LoadErpDocumentInfo(ref sosConfigurationState.DocumentClasses.DocClassesList);
                        
                        using (StringReader sr = new StringReader(sosConfigurationState.Serialize()))
							sosConf.DocClasses = XElement.Load(sr); // NON USARE XElement.Parse(string)!!!!
						dc.SubmitChanges();
					}

					DMSOrchestrator.LockManager.UnlockRecord(sosConf, DMSOrchestrator.LockContext);

					SOSProxyW.Init
						(
						SOSConfigurationState.SOSConfiguration.KeeperCode,
						SOSConfigurationState.SOSConfiguration.SubjectCode,
						SOSConfigurationState.SOSConfiguration.MySOSUser,
						Crypto.Decrypt(SOSConfigurationState.SOSConfiguration.MySOSPassword),
						DMSOrchestrator.SOSConfigurationState.SOSConfiguration.SOSWebServiceUrl
						);
					result = true;
				}
				else
					SetMessage(string.Format(Strings.ErrorSavingSOSConfigurationForSubject, sosConfigurationState.SOSConfiguration.SubjectCode), new Exception(lockMsg), "SaveSOSConfiguration");
			}
			catch (Exception e)
			{
				SetMessage(string.Format(Strings.ErrorSavingSOSConfigurationForSubject, sosConfigurationState.SOSConfiguration.SubjectCode), e, "SaveSOSConfiguration");
			}

			return result;
		}


        /// <summary>
        /// Restituisce la lista dei file di configurazione della SOS, eventualmente uno per ogni applicazione
        /// Ogni file conteniene l'elenco dei documenti dell'applicazione abilitati alla sostitutiva
        /// </summary>
         //-----------------------------------------------------------------------------
        public List<string> GetSosConfigurationFiles()
        {
            List<string> sosConfigurationFiles = new List<string>();

            // carico in una lista di appoggio tutte le applicazione dichiarati nell'installazione
            // sia nella Standard che nella Custom (ad es. EasyBuilder)
            StringCollection supportList = new StringCollection();
            StringCollection applicationsList = new StringCollection();

            // carico le applications dentro la Standard
            BasePathFinder.BasePathFinderInstance.GetApplicationsList(ApplicationType.TaskBuilderApplication, out supportList);
            for (int i = 0; i < supportList.Count; i++)
                applicationsList.Add(supportList[i]);

            // infine guardo le customizzazioni realizzate con EasyStudio
            BasePathFinder.BasePathFinderInstance.GetApplicationsList(ApplicationType.Customization, out supportList);
            for (int i = 0; i < supportList.Count; i++)
                applicationsList.Add(supportList[i]);

            foreach (string applicationName in applicationsList)
            {
                //prima verifico se esiste il file SOSConfiguration.xml nella custom 
                string configurationFile = Path.Combine(BasePathFinder.BasePathFinderInstance.GetCustomModuleObjectsPath(NameSolverStrings.AllCompanies, applicationName, NameSolverStrings.SOSConnectorModule), NameSolverStrings.SOSConfigurationXmlFile);
                if (File.Exists(configurationFile))
                    sosConfigurationFiles.Add(configurationFile);
                else
                {
                    configurationFile = Path.Combine(BasePathFinder.BasePathFinderInstance.GetApplicationModuleObjectsPath(applicationName, NameSolverStrings.SOSConnectorModule), NameSolverStrings.SOSConfigurationXmlFile);
                    if (File.Exists(configurationFile))
                        sosConfigurationFiles.Add(configurationFile);
                }
            }
            return sosConfigurationFiles;
        }

        //--------------------------------------------------------------------------------
        internal void LoadErpDocumentInfo(ref List<DocClass> docClasses)
        {
            List<string> sosConfigurationFiles = GetSosConfigurationFiles();
            foreach (string filePath in sosConfigurationFiles)
            {
                XmlReader xmlReader = XmlReader.Create(filePath);
                XmlSerializer serializer = new XmlSerializer(typeof(SOSConfigurationState));
                SOSConfigurationState sosConfState = serializer.Deserialize(xmlReader) as SOSConfigurationState;

                foreach (DocClass docClass in sosConfState.DocumentClasses.DocClassesList)
                {
                    // le DocClass mi vengono fornite dalla Sostitutiva Zucchetti
                    // sono quelle per cui ha firmato il cliente
                    DocClass existClass = docClasses.Find(a => a.Code == docClass.Code);
                    if (existClass == null)
                        continue;
                    existClass.InternalDocClass = docClass.InternalDocClass;
                    foreach (ERPSOSDocumentType erpSOSDocType in docClass.ERPDocNamespaces)                   
                        existClass.ERPDocNamespaces.Add(erpSOSDocType);
                }
                xmlReader.Close();
            }

        }

        ///<summary>
        /// Questo metodo viene chiamato in fase di configurazione del SOSConnector, 
        /// quando ancora l'utente sta inserendo i dati necessari al collegamento (quindi deve passare anche i parametri dell'utente MySOS)
        ///</summary>
        //--------------------------------------------------------------------------------
        public bool ElencoClassiDocumentali(string keeper, string subject, string user, string pwd, string url, out List<ClasseDocumentale> docClassesList)
		{
			docClassesList = null;

			// Qui utilizzo un proxy temporaneo perchè non ho memorizzato ancora i giusti valori
			SOSProxyWrapper tempProxy = new SOSProxyWrapper(BasePathFinder.BasePathFinderInstance.SOSProxyUrl);
			tempProxy.Init(keeper, subject, user, pwd, url);

			ClasseDocumentale[] list;
			if (!tempProxy.ElencoClassiDocumentali(keeper, subject, user, pwd, out list))
			{
				SetDiagnostic(tempProxy.SosDiagnostic);
				return false;
			}

			docClassesList = list.OfType<ClasseDocumentale>().ToList();
			// se il result e' false oppure non sono state ritornate classi documentali 
			if (docClassesList == null || docClassesList.Count == 0)
			{
				SetMessage(Strings.ErrorLoadingDocClasses, null, "ElencoClassiDocumentali");
				return false;
			}

			return true;
		}

		//--------------------------------------------------------------------------------
		public bool ElencoChiaviClasseDocumentale(string classeDocumentale, out ClasseDocumentale docClass)
		{
			docClass = null;

			if (!SOSProxyW.ElencoChiaviClasseDocumentale(classeDocumentale, out docClass))
			{
				SetDiagnostic(sosProxy.SosDiagnostic);
				return false;
			}

			return docClass != null && !string.IsNullOrEmpty(docClass.CodiceClasseDocumentale);
		}

		//-------------------------------------------------------------------------------
		private DataRow AddSOSSpecialField(Chiave key, ref BookmarksDataTable dataTable)
		{
            if (DMSDocOrchestrator == null)
                return null;

			DataRow newRow = null;
			if (string.Compare(key.CodiceChiave, CommonStrings.FiscalYear, true) == 0)
				newRow = dataTable.AddSOSSpecialField(CommonStrings.FiscalYear, DMSDocOrchestrator);
			else
			{
				if (string.Compare(key.CodiceChiave, CommonStrings.Suffix, true) == 0)
                    newRow = dataTable.AddSOSSpecialField(CommonStrings.Suffix, DMSDocOrchestrator);
				else
					if (string.Compare(key.CodiceChiave, CommonStrings.SOSDocumentType, true) == 0)
                        newRow = dataTable.AddSOSSpecialField(CommonStrings.SOSDocumentType, DMSDocOrchestrator);
			}
			if (newRow != null)
			{
				newRow[CommonStrings.SosPosition] = key.Ordine;
				newRow[CommonStrings.SosMandatory] = key.CdObblig;
			}
			return newRow;
		}

		//-------------------------------------------------------------------------------
		public bool AdjustTemplateForSosConnector(ref BookmarksDataTable dataTable)
		{
            if (DMSDocOrchestrator == null)
                return true;
			try
			{
				//string internalDocClassName = DMSOrchestrator.SOSConfigurationState.DocumentClasses.GetInternalDocClass(DMSOrchestrator.DocumentNamespace);

                string docClassName = SOSConfigurationState.DocumentClasses.GetDocClassCodeFromNs(DMSDocOrchestrator.DocumentNamespace, DMSDocOrchestrator.Document.GetSosDocumentType());
				if (string.IsNullOrEmpty(docClassName))
					return false;

				//il documentType lo creo sempre 
                dataTable.AddSOSSpecialField(CommonStrings.SOSDocumentType, DMSDocOrchestrator);

				ClasseDocumentale docClass;
				if (ElencoChiaviClasseDocumentale(docClassName, out docClass))
				{
					foreach (Chiave key in docClass.Chiavi)
					{
						DataRow foundRow = dataTable.FindBySosKeyCode(key.CodiceChiave);
						if (foundRow != null)
						{
							foundRow[CommonStrings.SosPosition] = key.Ordine;
							foundRow[CommonStrings.SosMandatory] = key.CdObblig;
						}
						else
						{
							//se è uno dei campi speciali allora lo creo
							foundRow = AddSOSSpecialField(key, ref dataTable);
							//non è uno dei campi speciali. Allora vado a controllare se appartiene al variabili del documento batch o al master del dataentry                           
							if (foundRow == null)
							{
								//se il documento è di tipo batch (vedi improvement #5373)
								//vado a considerare le variabili di documento (quelle presenti nella CVariableArray)
                                if (DMSDocOrchestrator.Document.Batch)
								{
                                    MXMLVariableArray variables = DMSDocOrchestrator.Document.BookmarkXMLVariables;
									MXMLVariable variable = (variables != null) ? variables.GetVariable(key.CodiceChiave) : null;
									if (variable != null)
									{
										foundRow = dataTable.AddBindingField(variable);
										foundRow[CommonStrings.SosPosition] = key.Ordine;
										foundRow[CommonStrings.SosMandatory] = key.CdObblig;
									}
								}
								else
								{
									//se il documento è un dataentry
                                    MSqlRecordItem recItem = (MSqlRecordItem)DMSDocOrchestrator.MasterRecord.GetField(key.CodiceChiave);
									if (recItem != null)
									{
										foundRow = dataTable.AddBindingField(recItem, false);
										foundRow[CommonStrings.SosPosition] = key.Ordine;
										foundRow[CommonStrings.SosMandatory] = key.CdObblig;
									}
								}
							}
						}
						if (foundRow != null)
						{
							foundRow[CommonStrings.Disable] = false; //se è per il SOS non deve essere mai disabilitato. Deve essere sempre presente
							if (foundRow.RowState == DataRowState.Unchanged)
								foundRow.SetModified();
						}
					}
					return true;
				}
			}
			catch (Exception)
			{
				return false;
			}

			return false;
		}

		//-------------------------------------------------------------------------------
		private bool CheckEmptyValues(AttachmentInfo attachment, ref string msg)
		{
            if (DMSDocOrchestrator == null)
                return true;

			bool sosRowsFound = false;

			string emptyMsg = string.Empty;

			//se mi viene passato il template di default allora devo verificare che i campi obbligatori per il SOS siano presenti tra i bookmark dell'attachment 
            BookmarksDataTable defaultDT = (DMSDocOrchestrator.AttachManager.DefaultBookmarksDT == null)
                ? DMSDocOrchestrator.AttachManager.GetDefaultCollectionFields(DMSDocOrchestrator.DocumentCollection)
                : DMSDocOrchestrator.AttachManager.DefaultBookmarksDT;

			if (defaultDT != null)
			{
				foreach (DataRow row in defaultDT.Rows)
				{
					// considero le sole righe con position > 0 					
					if (Convert.ToInt32(row[CommonStrings.SosPosition]) <= 0)
						continue;
					sosRowsFound = true;

					//verifico che il campo x il SOS sia presente nel BookmarkDT dell'attachment
					DataRow foundRow = attachment.BookmarksDT.Rows.Find(row[CommonStrings.Name].ToString());
					//DataRow foundRow = (foundRows != null && foundRows.Count() > 0) ? foundRows.Single() : null;

					//se esiste almeno un bookmark con attributo SosMandatory a true e con valore empty allora non posso mettere il documento in sostitutiva
					//non creo il record in DMS_SOSDocument e fornisco un messaggio
					if (foundRow == null || ((bool)foundRow[CommonStrings.SosMandatory] && string.IsNullOrEmpty(((FieldData)foundRow[CommonStrings.FieldData]).StringValue)))
					{
						if (!string.IsNullOrEmpty(emptyMsg))
							emptyMsg += "; ";
						emptyMsg += row[CommonStrings.FieldDescription].ToString();
					}
				}
			}
			else
			{
				foreach (DataRow row in attachment.BookmarksDT.Rows)
				{
					// considero le sole righe con position > 0 					
					if (Convert.ToInt32(row[CommonStrings.SosPosition]) <= 0)
						continue;

					sosRowsFound = true;

					//se esiste almeno un bookmark con attributo SosMandatory a true e con valore empty allora non posso mettere il documento in sostitutiva
					//non creo il record in DMS_SOSDocument e fornisco un messaggio
					if ((bool)row[CommonStrings.SosMandatory] && string.IsNullOrEmpty(((FieldData)row[CommonStrings.FieldData]).StringValue))
					{
						if (!string.IsNullOrEmpty(emptyMsg))
							emptyMsg += "; ";
						emptyMsg += row[CommonStrings.FieldDescription].ToString();
					}
				}
			}

			// se non ho trovato nessuna riga nei bookmark relativa al SOS si tratta di un allegato
			// inserito prima di avviare il SOS, quindi non puo' essere aggiunto in SOS essendo privo delle chiavi di descrizione obbligatorie
			if (!sosRowsFound)
				msg = string.Format(Strings.DocCreatedBeforeSos, attachment.Name);
			else
				if (!string.IsNullOrEmpty(emptyMsg))
					msg = string.Format(Strings.NoSosDocumentCreatedFieldsEmpty, emptyMsg);

			return string.IsNullOrEmpty(msg);
		}

		//cancella i vecchi bookmark per poter utilizzare il nuovo template compatibile con SOS
		//-------------------------------------------------------------------------------
		private void CreateSOSBookmark(AttachmentInfo attachment)
		{
            if (DMSDocOrchestrator == null)
                return;

			//se mi viene passato il template di default allora devo verificare che i campi obbligatori per il SOS siano presenti tra i bookmark dell'attachment 
            BookmarksDataTable defaultDT = (DMSDocOrchestrator.AttachManager.DefaultBookmarksDT == null)
                ? DMSDocOrchestrator.AttachManager.GetDefaultCollectionFields(DMSDocOrchestrator.DocumentCollection)
                : DMSDocOrchestrator.AttachManager.DefaultBookmarksDT;

			if (defaultDT != null)
			{
				foreach (DataRow row in defaultDT.Rows)
				{
					// considero le sole righe con position > 0 					
					if (Convert.ToInt32(row[CommonStrings.SosPosition]) <= 0)
						continue;

					//verifico che il campo x il SOS sia presente nel BookmarkDT dell'attachment
					DataRow foundRow = attachment.BookmarksDT.Rows.Find(row[CommonStrings.Name].ToString());

					//se esiste almeno un bookmark con attributo SosMandatory a true e con valore empty allora non posso mettere il documento in sostitutiva
					//non creo il record in DMS_SOSDocument e fornisco un messaggio
					if (foundRow != null)
					{
						if (string.IsNullOrEmpty(((FieldData)foundRow[CommonStrings.FieldData]).StringValue))
                            DMSDocOrchestrator.AttachManager.SyncronizeFieldValue(ref foundRow);

						//valorizzo i campi propri del SOSConnector
						foundRow[CommonStrings.SosMandatory] = row[CommonStrings.SosMandatory];
						foundRow[CommonStrings.SosPosition] = row[CommonStrings.SosPosition];
						foundRow[CommonStrings.SosKeyCode] = row[CommonStrings.SosKeyCode];
					}
					else
					{
						DataRow newRow = attachment.BookmarksDT.NewRow();
						newRow[CommonStrings.Name] = row[CommonStrings.Name];
						newRow[CommonStrings.FieldDescription] = row[CommonStrings.FieldDescription];
                        newRow[CommonStrings.FieldData] = row[CommonStrings.FieldData];
						newRow[CommonStrings.ValueSet] = row[CommonStrings.ValueSet];
						newRow[CommonStrings.ControlName] = row[CommonStrings.ControlName];
						newRow[CommonStrings.PhysicalName] = row[CommonStrings.PhysicalName];
						newRow[CommonStrings.OCRPosition] = row[CommonStrings.OCRPosition];
						newRow[CommonStrings.ValueType] = row[CommonStrings.ValueType];
						newRow[CommonStrings.FieldGroup] = row[CommonStrings.FieldGroup];
						newRow[CommonStrings.ShowAsDescription] = row[CommonStrings.ShowAsDescription];
						newRow[CommonStrings.HotKeyLink] = row[CommonStrings.HotKeyLink];
						newRow[CommonStrings.SosMandatory] = row[CommonStrings.SosMandatory];
						newRow[CommonStrings.SosPosition] = row[CommonStrings.SosPosition];
						newRow[CommonStrings.SosKeyCode] = row[CommonStrings.SosKeyCode];
                        DMSDocOrchestrator.AttachManager.SyncronizeFieldValue(ref newRow);
                        attachment.BookmarksDT.Rows.Add(newRow);
					}
				}
                DMSDocOrchestrator.ArchiveManager.UpdateSearchIndexes(attachment, attachment.Tags, attachment.Description);
            }
        }

		///<summary>        
		/// true se l'attachmnewRow[eCommonStrings.Disable, typeof(Snt ha tutti i requisiti per andare in SOS ovvero:
		/// - il file è convertibile in PDFA
		/// - le chiavi di descrizione obbligatori hanno un valore
		///</summary>
		//-------------------------------------------------------------------------------
		public CanBeSentToSOSType CanBeSentToSOS(AttachmentInfo attachment, out string msg)
		{
			msg = string.Empty;
			string emptyMsg = string.Empty;
			if (!FileExtensions.CanBeConvertedToPDFA(attachment.ExtensionType))
			{
				msg = string.Format(Strings.UnconvertibleToPDFA, attachment.Name);
				return CanBeSentToSOSType.NoPDFA;
			}

			return CheckEmptyValues(attachment, ref msg) ? CanBeSentToSOSType.BeSent : CanBeSentToSOSType.EmptySosKeyValue;
		}

		//-------------------------------------------------------------------------------
		public string GetSosFormattingData(DataRow row)
		{
            if (DMSDocOrchestrator == null)
                return string.Empty;

			string sosStringValue = string.Empty;
			string fieldName = row[CommonStrings.Name].ToString();

			if (
					!string.IsNullOrEmpty(row[CommonStrings.ValueType].ToString()) &&
					(string.Compare(row[CommonStrings.ValueType].ToString(), DataType.Date.ToString(), true) == 0 ||
					string.Compare(row[CommonStrings.ValueType].ToString(), DataType.DateTime.ToString(), true) == 0)
				)
			{
				switch ((FieldGroup)row[CommonStrings.FieldGroup])
				{
					case FieldGroup.Key:
					case FieldGroup.Binding:
					case FieldGroup.External:
						{
							MSqlRecordItem recField = null;

							if ((FieldGroup)row[CommonStrings.FieldGroup] == FieldGroup.External)
								recField = (row[CommonStrings.HotKeyLink] == DBNull.Value)
											? null
											: (MSqlRecordItem)(((MHotLink)row[CommonStrings.HotKeyLink]).Record.GetField(fieldName));
							else
                                recField = (MSqlRecordItem)DMSDocOrchestrator.MasterRecord.GetField(fieldName);

							if (recField != null)
							{
								DateTime sosDate = ((MDataDate)recField.DataObj).Value;
								sosStringValue = CoreUtils.ReplaceForbiddenCharsInSOSDescriptionKey(sosDate.ToString("d", new System.Globalization.CultureInfo("it-IT", true)));
							}
						}
						break;
					case FieldGroup.Variable:
                        MXMLVariableArray variables = DMSDocOrchestrator.Document.BookmarkXMLVariables;
						MXMLVariable variable = (variables != null) ? variables.GetVariable(fieldName) : null;
						if (variables != null)
						{
                            DateTime sosDate = ((MDataDate)variable.DataObj).Value;
							sosStringValue = CoreUtils.ReplaceForbiddenCharsInSOSDescriptionKey(sosDate.ToString("d", new System.Globalization.CultureInfo("it-IT", true)));
						}
						break;

					default:
						break;
				}
			}
			else
				sosStringValue = CoreUtils.ReplaceForbiddenCharsInSOSDescriptionKey(((FieldData)row[CommonStrings.FieldData]).StringValue);

			return sosStringValue;
		}

		///<summary>
		/// Inserisce un record nella tabella DMS_SosDocument, in fase di creazione allegato dal documento di ERP
		///</summary>
		//-------------------------------------------------------------------------------
		public bool CreateNewSosDocument(ref AttachmentInfo attachmentInfo)
		{
			if (attachmentInfo == null)
				return false;

			if (!FileExtensions.CanBeConvertedToPDFA(attachmentInfo.ExtensionType))
			{
				// non visualizzo piu' il messaggio ma ritorno subito
				//SetMessage(string.Format(Strings.CanBeConvertedToPDFA, attachmentInfo), null, "CreateNewSosDocument", DiagnosticType.Warning);
				return false;
			}

			// se il SosDocument esiste gia' sul DB mi tengo da parte solo il suo stato
			SOSDocumentInfo sosDocInfo = GetSosDocumentInfo(attachmentInfo.AttachmentId);
			if (sosDocInfo != null)
			{
				attachmentInfo.SOSDocumentStatus = sosDocInfo.DocumentStatus;
				return true;
			}

			string msg = string.Empty;

			try
			{
				//se è necessario creo i bookmark per la SOS
				if (attachmentInfo.CreateSOSBookmark)
					CreateSOSBookmark(attachmentInfo);

				//verifico che il documento sia inviabile al SOS ovvero che tutti i bookmark per la SOS siano valorizzati correttamente
				if (!CheckEmptyValues(attachmentInfo, ref msg))
				{
					SetMessage(string.Format(Strings.ErrorCreatingNewSosDocument, attachmentInfo.AttachmentId) + msg, null, "CreateNewSosDocument", TaskBuilderNet.Interfaces.DiagnosticType.Warning);
					return false;
				}

				// eseguo il sort sulla colonna SOSPosition
				DataView dv = attachmentInfo.BookmarksDT.DefaultView;
				dv.Sort = CommonStrings.SosPosition;
				DataTable dtBookmarksSorted = dv.ToTable();

				string keys = string.Empty;
				string emptyMsg = string.Empty;
				string taxJournal = string.Empty;
				string documentType = string.Empty;
				string fiscalYear = string.Empty;

				// scorro tutte le righe del DataTable 
				//intanto che scorro le righe considero, se esiste il campo TaxJournal contenente il codice del registro iva
				foreach (DataRow row in dtBookmarksSorted.Rows)
				{
					if (string.Compare(row[CommonStrings.Name].ToString(), CommonStrings.SOSDocumentType, true) == 0)
						documentType = ((FieldData)row[CommonStrings.FieldData]).StringValue;

					// considero le sole righe con position > 0 					
					if (Convert.ToInt32(row[CommonStrings.SosPosition]) <= 0)
						continue;

					if (string.Compare(row[CommonStrings.Name].ToString(), CommonStrings.TaxJournal, true) == 0)
						taxJournal = ((FieldData)row[CommonStrings.FieldData]).StringValue;

					if (string.Compare(row[CommonStrings.Name].ToString(), CommonStrings.FiscalYear, true) == 0)
						fiscalYear = ((FieldData)row[CommonStrings.FieldData]).StringValue;

					keys += GetSosFormattingData(row);
				}

				if (!string.IsNullOrEmpty(keys))
				{
					keys += attachmentInfo.AttachmentId + ";"; // AttachmentId
					keys += "00000;"; // codice soggetto padre
					keys += DMSOrchestrator.SOSSubjectCode + ";"; // codice soggetto figlio
				}

				// creo il nuovo SOSDocument
				DMS_SOSDocument sosDoc = new DMS_SOSDocument();
				sosDoc.AttachmentID = attachmentInfo.AttachmentId;
				sosDoc.FileName = string.Empty;
				sosDoc.HashCode = string.Empty;
				sosDoc.LotID = string.Empty;
				sosDoc.AbsoluteCode = string.Empty;
				sosDoc.DocumentStatus = (int)StatoDocumento.IDLE;
				sosDoc.TaxJournal = taxJournal;
				sosDoc.DocumentType = documentType;
				sosDoc.FiscalYear = fiscalYear;
				sosDoc.SendingType = (bool)DMSOrchestrator.SOSConfigurationState.SOSConfiguration.FTPSend ? (int)SendingType.FTP : (int)SendingType.WebService;

				// in questo caso salvo SOLO i valori delle chiavi di descrizione (il nome documento e l'hashcode sono calcolati successivamente)
				sosDoc.DescriptionKeys = keys;

				dc.DMS_SOSDocuments.InsertOnSubmit(sosDoc);
				dc.SubmitChanges();

				// aggiorno i dati dell'AttachmentInfo passato by ref
				attachmentInfo.SOSDocumentStatus = StatoDocumento.IDLE;
			}
			catch (Exception e)
			{
				SetMessage(string.Format(Strings.ErrorCreatingNewSosDocument, attachmentInfo.AttachmentId), e, "CreateNewSosDocument");
				return false;
			}

			return true;
		}

		///<summary>
		/// Posso cancellare il sosDocument relativo all'attachment con attachmentId solo 
		/// se il documento non è stato ancora inviato oppure se è già in stato di conservazione
		/// o se è in resend
		///</summary>
		//--------------------------------------------------------------------------------
		public bool DeleteSosDocument(int attachmentId)
		{
			SOSDocumentInfo sosDocInfo = GetSosDocumentInfo(attachmentId);
			if (sosDocInfo == null)
				return true;

			if (
				sosDocInfo.DocumentStatus != StatoDocumento.IDLE &&
				sosDocInfo.DocumentStatus != StatoDocumento.TORESEND &&
				sosDocInfo.DocumentStatus != StatoDocumento.DOCREPSIGN
				)
				return false;

			try
			{
				using (SqlConnection myConnection = new SqlConnection(DMSOrchestrator.DMSConnectionString))
				{
					myConnection.Open();

					using (IDbCommand myCommand = myConnection.CreateCommand())
					{
						myCommand.CommandText = string.Format(@"DELETE FROM [DMS_SOSDocument] WHERE [AttachmentID] = {0}", attachmentId.ToString());
						myCommand.ExecuteNonQuery();
					}
				}
			}
			catch (Exception e)
			{
				SetMessage(string.Format(Strings.ErrorDeletingSosDocument, attachmentId), e, "DeleteSosDocument");
				return false;
			}

			return true;
		}

		//-------------------------------------------------------------------------------
		public bool UpdateSosDocument(AttachmentInfo attachmentInfo)
		{
			if (attachmentInfo == null)
				return false;

			try
			{
				// carico tutte le colonne tranne il binario, che in questo contesto lo ricalcolo e lo ri-assegno sempre
				SOSDocumentInfo currentSosDoc = GetSosDocumentInfo(attachmentInfo.AttachmentId);
				if (currentSosDoc == null)
					return false;

				string pdfAFile = string.Empty;

				// prima devo convertire in PDF/A il documento
				if (!TransformToPdfAAndUpdateBinary(attachmentInfo, out pdfAFile))
					return false;

				// calcolo la Size del nuovo file PDF/A
				FileInfo fi = new FileInfo(pdfAFile);
				long size = fi.Length;

				// levo l'estensione al nome originale ed eseguo l'escape dei caratteri non validi (fix anomalia 25640)
				string escapedFileName = Path.GetFileNameWithoutExtension(CoreUtils.ReplaceForbiddenCharsInFileName(attachmentInfo.Name));
				// calcolo il suffisso univoco
				string attachmentSuffix = "_" + attachmentInfo.AttachmentId.ToString();
				
				// mi tengo da parte le lunghezze di entrambi i segmenti
				int fileLen = escapedFileName.Length;
				int attSuffixLen = attachmentSuffix.Length;

				// controllo che il nome completo del file non ecceda i 92 chr ed eventualmente tronco 
				// (sono esclusi i 4 chr dell'estensione .pdf che aggiungo dopo e i 4 chr dell'estensione .p7m che sono aggiunti in SOS dopo la firma)
				if ((fileLen + attSuffixLen) > 92)
					escapedFileName = escapedFileName.Substring(0, 92 - attSuffixLen);

				// aggiungo al nome _attachmentId e l'estensione .pdf
				string sosFileName = escapedFileName + attachmentSuffix + FileExtensions.DotPdf;
				
				// cambio il nome al file originale
				string fullSosFilePath = Path.Combine(Path.GetDirectoryName(pdfAFile), sosFileName);
				try
				{
					// elimino se esiste un file con lo stesso nome e lo rinomino
					if (File.Exists(fullSosFilePath))
					{
						FileInfo f = new FileInfo(fullSosFilePath);
						f.IsReadOnly = false;
						File.Delete(fullSosFilePath);
					}
					File.Move(pdfAFile, fullSosFilePath);
				}
				catch
				{
					return false;
				}

				// memorizzo l'hashcode calcolato
				string hashCode = CoreUtils.CreateDocumentHash256(fullSosFilePath);

				string newKeys = string.Empty;
				// se il doc e' da spedire per la prima volta
				if (currentSosDoc.DocumentStatus == StatoDocumento.IDLE)
				{
					// ricalcolo le chiavi di descrizione, mettendo prefisso (nomefile) e suffisso (hash)
					newKeys = sosFileName + ";";
					newKeys += currentSosDoc.DescriptionKeys;
					newKeys += hashCode;
				}
				else
				{
					// il documento e' da rispedire, quindi devo eliminare il primo token (con il nome del file) e l'ultimo (con l'hashcode)
					// ma prima controllo che le chiavi inizino con il nome del file, altrimenti potrebbe essere un falso resend (magari aggiornato a mano)
					if (currentSosDoc.DescriptionKeys.StartsWith(Path.GetFileNameWithoutExtension(sosFileName), StringComparison.InvariantCultureIgnoreCase))
					{
						try
						{
							currentSosDoc.DescriptionKeys = currentSosDoc.DescriptionKeys.Remove(0, currentSosDoc.DescriptionKeys.IndexOf(";") + 1);
							currentSosDoc.DescriptionKeys = currentSosDoc.DescriptionKeys.Remove(currentSosDoc.DescriptionKeys.LastIndexOf(";") + 1);
						}
						catch (ArgumentOutOfRangeException)
						{
							// per pararci
							return false;
						}
					}

					// ricalcolo le chiavi di descrizione, mettendo prefisso (nomefile) e suffisso (hash)
					newKeys = sosFileName + ";";
					newKeys += currentSosDoc.DescriptionKeys;
					newKeys += hashCode;
				}

				using (SqlConnection myConnection = new SqlConnection(DMSOrchestrator.DMSConnectionString))
				{
					myConnection.Open();

					using (IDbCommand myCommand = myConnection.CreateCommand())
					{
						myCommand.CommandText = @"UPDATE [DMS_SOSDocument] 
													SET [FileName] = @fileName, [Size] = @size, [DescriptionKeys] = @keys, [HashCode] = @hash, [DocumentStatus] = @docStatus, [SendingType] =  @sendingType
													WHERE [AttachmentID] = @attId";

						myCommand.Parameters.Add(new SqlParameter("@fileName", sosFileName));
						myCommand.Parameters.Add(new SqlParameter("@size", size));
						myCommand.Parameters.Add(new SqlParameter("@keys", newKeys));
						myCommand.Parameters.Add(new SqlParameter("@hash", hashCode));
						// aggiorno lo stato per qualsiasi tipo di documento e metto TOSEND (cosi l'EASync inizia a lavorarli)
						myCommand.Parameters.Add(new SqlParameter("@docStatus", (int)StatoDocumento.TOSEND));
						myCommand.Parameters.Add(new SqlParameter("@sendingType", (bool)DMSOrchestrator.SOSConfigurationState.SOSConfiguration.FTPSend ? (int)SendingType.FTP : (int)SendingType.WebService));
						myCommand.Parameters.Add(new SqlParameter("@attId", attachmentInfo.AttachmentId));

						myCommand.ExecuteNonQuery();
					}
				}
			}
			catch (Exception e)
			{
				SetMessage(string.Format(Strings.ErrorUpdatingSOSDoc, attachmentInfo.AttachmentId), e, "UpdateSosDocument");
				return false;
			}

			return true;
		}

		///<summary>
		/// Trasformo il file in Pdf/A e scrivo il binario nella colonna PdfABinary con query nativa SQL
		///</summary>
		//--------------------------------------------------------------------------------
		private bool TransformToPdfAAndUpdateBinary(AttachmentInfo ai, out string pdfAFile)
		{
			string pdfATempFile = string.Empty;
			pdfAFile = string.Empty;

			try
			{
				// trasformo subito in PDF/A il DocContent e lo salvo nella dir temporanea 
				pdfATempFile = DMSOrchestrator.TransformToPdfA(ai, Path.Combine(DMSOrchestrator.SosConnectorTempPath, ai.Name), true);
			}
			catch
			{
				return false;
			}

			// se e' fallita la conversione torno false
			if (string.IsNullOrWhiteSpace(pdfATempFile))
				return false;

			if (UpdatePDFABinaryContent(ai, pdfATempFile))
			{
				pdfAFile = pdfATempFile;
				return true;
			}

			return false;
		}

		///<summary>
		/// Aggiorno la colonna PDFABinary con il content estratto dal file PDFA
		/// utilizzando la query nativa di SQL
		///</summary>
		//--------------------------------------------------------------------------------
		private bool UpdatePDFABinaryContent(AttachmentInfo ai, string pdfAFile)
		{
			bool result = false;

			try
			{
				using (SqlConnection myConnection = new SqlConnection(DMSOrchestrator.DMSConnectionString))
				{
					myConnection.Open();

					using (IDbCommand myCommand = myConnection.CreateCommand())
					{
						myCommand.CommandText = string.Format(@"UPDATE [DMS_SOSDocument] SET [PdfABinary] = @binaryContent WHERE [AttachmentID] = {0}", ai.AttachmentId.ToString());

						myCommand.Parameters.Add(new SqlParameter("@binaryContent", File.ReadAllBytes(pdfAFile)));
						myCommand.ExecuteNonQuery();
					}
				}

				result = true;
			}
			catch (OutOfMemoryException)
			{
				return UpdatePDFABinaryContentForBigFile(GetSosDocumentInfo(ai.AttachmentId), pdfAFile);
			}
			catch (UnauthorizedAccessException uae)
			{
				SetMessage(Strings.ErrorSavingBinaryContent, uae, "UpdatePDFABinaryContent");
			}
			catch (SqlException e)
			{
				SetMessage(Strings.ErrorSavingBinaryContent, e, "UpdatePDFABinaryContent");
			}

			return result;
		}

		///<summary>
		/// Aggiorno la colonna PDFABinary con il content estratto dal file PDFA e dividendolo a pezzi
		/// utilizzando la query nativa di SQL
		///</summary>
		//--------------------------------------------------------------------------------
		private bool UpdatePDFABinaryContentForBigFile(SOSDocumentInfo sosDoc, string pdfAFile)
		{
			try
			{
				using (SqlConnection myConnection = new SqlConnection(DMSOrchestrator.DMSConnectionString))
				{
					myConnection.Open();

					using (IDbCommand myCommand = myConnection.CreateCommand())
					{
						// prima aggiorno la riga con il Content vuoto
						myCommand.CommandText = string.Format
							(
								@"UPDATE [DMS_SOSDocument] SET [PdfABinary] = NULL WHERE [AttachmentID] = {0}",
								sosDoc.AttachmentId.ToString()
							);
						myCommand.ExecuteNonQuery();

						// poi aggiorno il PdfABinary scrivendo un pezzetto alla volta
						myCommand.CommandText = @"UPDATE [DMS_SOSDocument] SET [PdfABinary].WRITE(@content,  @offset, @len) WHERE [AttachmentID] = @attachID";

						SqlParameter contentParam = new SqlParameter("@content", SqlDbType.VarBinary);
						myCommand.Parameters.Add(contentParam);
						SqlParameter offsetParam = new SqlParameter("@offset", SqlDbType.BigInt);
						myCommand.Parameters.Add(offsetParam);
						SqlParameter lengthParam = new SqlParameter("@len", SqlDbType.BigInt);
						myCommand.Parameters.Add(lengthParam);
						SqlParameter attachDocIDParam = new SqlParameter("@attachID", SqlDbType.Int);
						attachDocIDParam.Value = Convert.ToInt32(sosDoc.AttachmentId);
						myCommand.Parameters.Add(attachDocIDParam);

						using (FileStream fs = new FileStream(pdfAFile, FileMode.Open, FileAccess.Read, FileShare.Read))
						{
							// leggiamo a blocchi di 30MB
							int bufferSize = (sosDoc.Size > 31457280) ? 31457280 : (int)sosDoc.Size;

							byte[] buffer = new byte[bufferSize]; // chunk sizes 50MB
							int read = 0;
							int offset = 0;

							while ((read = fs.Read(buffer, 0, buffer.Length)) > 0)
							{
								contentParam.Value = buffer;
								offsetParam.Value = offset;
								lengthParam.Value = read;
								myCommand.ExecuteNonQuery();
								offset += read;
							}
							buffer = null;
						}
					}
				}
			}
			catch (SqlException e)
			{
				SetMessage(Strings.ErrorSavingBinaryContent, e, "UpdatePDFABinaryContentForBigFile");
				return false;
			}

			return true;
		}

		//--------------------------------------------------------------------------------
		public string GetPDFATemporaryFile(DMS_Attachment attachment)
		{
			SqlConnection connect = null;
			string tempFile = string.Empty;
			try
			{
				connect = new SqlConnection(DMSOrchestrator.DMSConnectionString);
				connect.Open();
				tempFile = coreSosManager.GetPDFATemporaryFile(attachment, connect);
				connect.Close();
			}
			catch (SqlException e)
			{
				SetMessage(string.Format(Strings.ErrorGettingPDFATempFile, attachment.AttachmentID.ToString()), e, "GetPDFATemporaryFile");
			}
			finally
			{
				if (connect.State == ConnectionState.Open)
					connect.Close();
			}
			return tempFile;
		}

		//--------------------------------------------------------------------------------
		public List<DMS_SOSEnvelope> GetEnvelopesForClass(string theClass)
		{
			var docs = from att in dc.DMS_SOSEnvelopes
					   where att.DMS_Collection.SosDocClass == theClass
					   select att;
			try
			{
				if (docs == null || !docs.Any())
					return null;
			}
			catch (Exception e)
			{
				SetMessage(string.Format(Strings.ErrorGettingSosEnvelope, theClass), e, "GetEnvelopesForClass");
			}
			return docs.ToList();
		}

		/// <summary>
		/// Introdotta da Anna per la ricerca degli envelope per data nel SOSMonitor della 3.x
		/// Vedere poi se serve nel nuovo monitor
		/// </summary>
		/// <param name="theClass"></param>
		/// <param name="startDate"></param>
		/// <param name="endDate"></param>
		/// <returns></returns>
		//--------------------------------------------------------------------------------
		public List<DMS_SOSEnvelope> GetEnvelopesForClass(string theClass, DateTime startDate, DateTime endDate)
		{
			var docs = from att in dc.DMS_SOSEnvelopes
					   where att.DMS_Collection.SosDocClass == theClass &&
					   (att.DispatchDate >= startDate && att.DispatchDate <= endDate)
					   select att;
			try
			{
				if (docs == null || !docs.Any())
					return null;
			}
			catch (Exception e)
			{
				SetMessage(string.Format(Strings.ErrorGettingSosEnvelope, theClass), e, "GetEnvelopesForClass");
			}
			return docs.ToList();
		}

		//--------------------------------------------------------------------------------
		public List<DMS_SOSEnvelope> GetAllEnvelopesOrderByClass()
		{
			var docs = from att in dc.DMS_SOSEnvelopes
					   orderby att.CollectionID
					   select att;
			try
			{
				if (docs == null || !docs.Any())
					return null;
			}
			catch (Exception e)
			{
				SetMessage(string.Format(Strings.ErrorGettingSosEnvelope, "All classes"), e, "GetAllEnvelopesOrderByClass");
			}
			return docs.ToList();
		}

		//--------------------------------------------------------------------------------
		public List<DMS_SOSDocument> GetDocumentForEnvelope(DMS_SOSEnvelope envelope)
		{
			var docs = from att in dc.DMS_SOSDocuments
					   where att.EnvelopeID == envelope.EnvelopeID
					   orderby att.FileName
					   select att;
			try
			{
				if (docs == null || !docs.Any())
					return null;
			}
			catch (Exception e)
			{
				SetMessage(string.Format(Strings.ErrorGettingEnvSosDocument, envelope.DispatchCode), e, "GetAllEnvelopesOrderByClass");
			}
			return docs.ToList();
		}

		//--------------------------------------------------------------------------------
		public List<int> GetCollectionsForDocClass(string theClass)
		{
			try
			{
				var docs = from att in dc.DMS_Collections where att.SosDocClass == theClass select att.CollectionID;

				if (docs == null || !docs.Any())
					return null;

				return docs.ToList();
			}
			catch (Exception e)
			{
				SetMessage(Strings.ErrorReadingClassInfo, e, "GetCollectionsForDocClass");
			}

			return null;
		}

		//--------------------------------------------------------------------------------
		public List<string> GetAllTaxJournals(string docType, List<StatoDocumento> sosDocumentStatus)
		{
			List<string> collection = new List<string>();
            try
            {
                using (SqlConnection myConnection = new SqlConnection(DMSOrchestrator.DMSConnectionString))
                {
                    myConnection.Open();

                    using (SqlCommand myCommand = myConnection.CreateCommand())
                    {

                        string docState = string.Empty;
                        foreach (StatoDocumento state in sosDocumentStatus)
                        {
                            if (!string.IsNullOrEmpty(docState))
                                docState += " OR ";
                            docState += string.Format("DocumentStatus = {0}", ((int)state).ToString());
                        }

                      if (string.IsNullOrWhiteSpace(docState) || string.IsNullOrWhiteSpace(docType))
							return collection;

						myCommand.CommandText = string.Format("SELECT DISTINCT [TaxJournal] FROM [DMS_SOSDocument] WHERE ({0}) AND (DocumentType = '{1}') ORDER BY [TaxJournal]", docState, docType);
                        
                        SqlDataReader sdr = myCommand.ExecuteReader();
                        while (sdr.Read())
                            collection.Add(sdr["TaxJournal"].ToString());
                        sdr.Close();
                    }
                }
            }
            catch (SqlException e)
            {
                SetMessage(string.Format(Strings.Error, e.Message), e, "GetAllTaxJournals");
            }

            return collection;
        }
         
		//--------------------------------------------------------------------------------
		public List<string> GetAllDocumentType(List<string> sosDocumentTypes)
		{
			List<string> documentTypes = new List<string>();
			try
			{
				using (SqlConnection myConnection = new SqlConnection(DMSOrchestrator.DMSConnectionString))
				{
					myConnection.Open();
					string docType = string.Empty;
					foreach (string docTemp in sosDocumentTypes)
					{
						if (!string.IsNullOrEmpty(docType))
							docType += " OR ";
						docType += string.Format("DocumentType = '{0}'", docTemp);
					}

					using (SqlCommand myCommand = myConnection.CreateCommand())
					{
						// prima aggiorno la riga con il Content vuoto
						myCommand.CommandText = string.Format("SELECT DISTINCT DocumentType FROM DMS_SOSDocument WHERE {0} ORDER BY DocumentType ", docType);
						SqlDataReader sdr = myCommand.ExecuteReader();
						while (sdr.Read())
							documentTypes.Add(sdr["DocumentType"].ToString());
						sdr.Close();
					}
				}
			}

			catch (SqlException e)
			{
				SetMessage(string.Format(Strings.Error, e.Message), e, "GetAllDocumentType");
			}

			return documentTypes;
		}

        //--------------------------------------------------------------------------------
        public List<string> GetAllFiscalYears(string docType, string taxJournal, List<StatoDocumento> sosDocumentStatus)
        {
            List<string> collection = new List<string>();
            try
            {
                using (SqlConnection myConnection = new SqlConnection(DMSOrchestrator.DMSConnectionString))
                {
                    myConnection.Open();

                    using (SqlCommand myCommand = myConnection.CreateCommand())
                    {

                        string docState = string.Empty;
                        foreach (StatoDocumento state in sosDocumentStatus)
                        {
                            if (!string.IsNullOrEmpty(docState))
                                docState += " OR ";
                            docState += string.Format("DocumentStatus = {0}", ((int)state).ToString());
                        }

                      
						if (string.IsNullOrWhiteSpace(docState) || string.IsNullOrWhiteSpace(docType))
							return collection;

						myCommand.CommandText = string.Format
                        (
                            "SELECT DISTINCT [FiscalYear] FROM [DMS_SOSDocument] WHERE ({0}) AND (DocumentType = '{1}'){2} ORDER BY [FiscalYear] DESC",
                            docState, docType, (string.IsNullOrEmpty(taxJournal)) ? "" : string.Format(" AND [TaxJournal] = '{0}'", taxJournal)
                        );

                        SqlDataReader sdr = myCommand.ExecuteReader();
                        while (sdr.Read())
                            collection.Add(sdr["FiscalYear"].ToString());
                        sdr.Close();
                    }
                }
            }
            catch (SqlException e)
            {
                SetMessage(string.Format(Strings.Error, e.Message), e, "GetAllFiscalYears");
            }

            return collection;
        }
        
		//--------------------------------------------------------------------------------
        public bool Searching()
        {
            return startSearchSOSDocuments;
        }

		//effettua la ricerca tra gli allegati che devono essere ancora resi disponibili per la spedizione
		//essere resi disponibili per la spedizione significa
		//--------------------------------------------------------------------------------
		public SearchResultDataTable SearchAttachmentsForAdjust(SosSearchRules searchRules)
		{
			SearchResultDataTable searchResultDT = new SearchResultDataTable();
			try
			{
				using (SqlConnection myConnection = new SqlConnection(DMSOrchestrator.DMSConnectionString))
				{
					myConnection.Open();

					using (SqlCommand myCommand = myConnection.CreateCommand())
					{
						myCommand.CommandText = GetSearchAttachmentsForAdjustCmdString(searchRules);

						using (SqlDataReader sdr = myCommand.ExecuteReader())
						{
							while (sdr.Read())
							{
								string extensionType = sdr["ExtensionType"].ToString();
								// skippo gli allegati con estensione non convertibile in pdf/a
								if (!FileExtensions.CanBeConvertedToPDFA(extensionType))
									continue;

								string docKeyDescri = DMSOrchestrator.SearchManager.GetDocumentKeyDescription((int)sdr["AttachmentID"]);

								DataRow newRow = searchResultDT.NewRow();
								newRow[CommonStrings.ArchivedDocID]		= sdr["ArchivedDocID"].ToString();
								newRow[CommonStrings.AttachmentID]		= (int)sdr["AttachmentID"];
								newRow[CommonStrings.Collector]			= -1;
								newRow[CommonStrings.Collection]		= (int)sdr["CollectionID"];
								newRow[CommonStrings.DocNamespace]		= searchRules.ERPSosDocumentType.DocType;
								newRow[CommonStrings.TBPrimaryKey]		= docKeyDescri;
								newRow[CommonStrings.Name]				= sdr["Name"].ToString();
								newRow[CommonStrings.AttachmentDate]	= (sdr["TBCreated"] is DBNull) ? DateTime.MinValue : (DateTime)sdr["TBCreated"];
								newRow[CommonStrings.ExtensionType]		= extensionType;
								newRow[CommonStrings.DocKeyDescription] = docKeyDescri;
								newRow[CommonStrings.AttachmentInfo]	= DMSOrchestrator.SearchManager.GetAttachmentInfoFromAttachmentId((int)newRow[CommonStrings.AttachmentID]);
								searchResultDT.Rows.Add(newRow);

								OnAddRowToResult?.Invoke(this, new AddRowInResultEventArgs(newRow));
							}
						}
					}
				}
			}
			catch (SqlException e)
			{
				SetMessage(Strings.ErrorSearchingAttachment, e, "SearchSosDocumentsThread");
			}
			finally
			{
				SearchFinished?.Invoke(this, EventArgs.Empty);
			}

			return searchResultDT;
		}

		//--------------------------------------------------------------------------------
		private string GetSearchAttachmentsForAdjustCmdString(SosSearchRules searchRules)
		{
			string strCompleteCmd = string.Empty;

			if (!searchRules.UseERPFilter())
			{
				strCompleteCmd = string.Format
					(
						@"SELECT a.ArchivedDocID, a.AttachmentID, a.CollectionID, e.DocNamespace, e.ErpDocumentID, d.Name, d.ExtensionType, a.TBCreated 
						FROM DMS_Attachment a, DMS_ArchivedDocument d,  DMS_ErpDocument e, DMS_CollectionsFields c, DMS_SearchFieldIndexes f, DMS_AttachmentSearchIndexes i 
						WHERE e.DocNamespace = '{0}' AND a.ErpDocumentID = e.ErpDocumentID AND d.ArchivedDocID = a.ArchivedDocID  
						AND NOT EXISTS (SELECT s.AttachmentID FROM DMS_SOSDocument s WHERE s.AttachmentID = a.AttachmentID) AND 
						f.SearchIndexID = i.SearchIndexID AND i.AttachmentID = a.AttachmentID AND c.CollectionID = a.CollectionID AND 
						(c.SosKeyCode ='FiscalYear' AND f.FieldName = c.FieldName AND f.FieldValue = '{1}') ORDER BY e.ErpDocumentID, a.TbCreated",
						searchRules.ERPSosDocumentType.DocNamespace,
						searchRules.FiscalYear
					);
			}
			else //devo fare tante query da intersecare tra loro quanti sono i criteri gestionali impostati
			{
				string strSingleQuery = string.Empty;

				string strBaseCmd = string.Format
					(
						@"SELECT a.ArchivedDocID, a.AttachmentID, a.CollectionID, e.DocNamespace, e.ErpDocumentID, d.Name, d.ExtensionType, a.TBCreated 
                        FROM DMS_Attachment a, DMS_ArchivedDocument d,  DMS_ErpDocument e, DMS_CollectionsFields c, DMS_SearchFieldIndexes f, DMS_AttachmentSearchIndexes i 
                        WHERE e.DocNamespace = '{0}' AND a.ErpDocumentID = e.ErpDocumentID AND d.ArchivedDocID = a.ArchivedDocID  
						AND NOT EXISTS (Select s.AttachmentID from DMS_SOSDocument s where s.AttachmentID = a.AttachmentID) AND 
                        f.SearchIndexID = i.SearchIndexID AND i.AttachmentID = a.AttachmentID AND c.CollectionID = a.CollectionID",
						searchRules.ERPSosDocumentType.DocNamespace
					);

				foreach (ERPFieldRule fieldRule in searchRules.ERPFieldsRuleList.ERPFieldsRules)
				{
					strSingleQuery = GetSingleCmdString(strBaseCmd, fieldRule);
					strCompleteCmd = (string.IsNullOrEmpty(strCompleteCmd)) ? strSingleQuery : string.Format("({0}) INTERSECT ({1})", strCompleteCmd, strSingleQuery);
				}

				if (!string.IsNullOrEmpty(strCompleteCmd))
					strCompleteCmd += " ORDER BY e.ErpDocumentID, a.TbCreated";
			}
			
			return strCompleteCmd;
		}

		/// <summary>
		/// Metodo richiamato dal C++ per inviare generare delle righe nella DMS_SOSDocument 
		/// dagli allegati generati in tempi antecedenti l'attivazione della SOS
		/// </summary>
		/// <param name="attList">lista di AttachmentInfo da considerare</param>
		/// <returns></returns>
		//--------------------------------------------------------------------------------
		public bool AdjustAttachmentsForSOS(List<AttachmentInfo> attList, ERPSOSDocumentType sosDocType)
		{
			// considero il namespace del documento legato al document type ed istanzio il documento
			// (lo faccio qui per non avere problemi dopo sul thread separato!)
			MDocument tbDoc = MDocument.CreateUnattended<MDocument>(sosDocType.DocNamespace, null);

			Thread myThread = new Thread(() => InternalAdjustAttachmentsForSOS(attList, tbDoc));
			myThread.CurrentUICulture = Thread.CurrentThread.CurrentUICulture;
			myThread.CurrentCulture = Thread.CurrentThread.CurrentCulture;
			myThread.Start();

			return true;
		}

		/// <summary>
		/// Metodo che, data l'istanza di un documento, e' in grado di generare i bookmarks obbligatori per la SOS
		/// ed inserire un record nella DMS_SOSDocument
		/// </summary>
		/// <param name="attList">lista allegati</param>
		/// <param name="tbDoc">istanza MDocument</param>
		//--------------------------------------------------------------------------------
		public void InternalAdjustAttachmentsForSOS(List<AttachmentInfo> attList, MDocument tbDoc)
		{
			PostSOSEventArgsMessage(string.Format(Strings.StartingDocumentsElaboration, attList.Count.ToString()));

			// contatore allegati generati con successo
			int nrAttWithSuccess = 0;

			if (attList.Count > 0)
			{
				if (tbDoc != null)
				{
					SearchResultDataTable searchResultDT = new SearchResultDataTable();
					int currErpDocID = -1;
					string result = string.Empty;

					for (int i = 0; i < attList.Count; i++)
					{
						AttachmentInfo attInfo = attList[i];

						string message = string.Format(Strings.FileElaborationInProgress, attInfo.Name, (i + 1).ToString(), attList.Count.ToString());
						PostSOSEventArgsMessage(message, DiagnosticType.None, i);

						if (attInfo.ErpDocumentID != currErpDocID)
						{
							tbDoc.BrowseRecord(attInfo.ERPPrimaryKeyValue);
							currErpDocID = attInfo.ErpDocumentID;
						}

						if (!tbDoc.ValidCurrentRecord()) continue;

						result = string.Empty;

						bool bOk = CUtility.CreateNewSosDocument(tbDoc.TbHandle, attInfo.AttachmentId, ref result);
						if (!bOk)
						{
							DataRow newRow = searchResultDT.NewRow();
							newRow[CommonStrings.ArchivedDocID]		= attInfo.ArchivedDocId.ToString();
							newRow[CommonStrings.AttachmentID]		= attInfo.AttachmentId;
							newRow[CommonStrings.Collector]			= -1;
							string docKeyDescri = DMSOrchestrator.SearchManager.GetDocumentKeyDescription((int)newRow["AttachmentID"]);
							newRow[CommonStrings.Collection]		= attInfo.CollectionID;
							newRow[CommonStrings.DocNamespace]		= result;
							newRow[CommonStrings.TBPrimaryKey]		= attInfo.ERPPrimaryKeyValue;
							newRow[CommonStrings.Name]				= attInfo.Name;
							newRow[CommonStrings.AttachmentDate]	= attInfo.AttachedDate;
							newRow[CommonStrings.ExtensionType]		= attInfo.ExtensionType;
							newRow[CommonStrings.DocKeyDescription] = docKeyDescri;
							newRow[CommonStrings.AttachmentInfo]	= attInfo;
							searchResultDT.Rows.Add(newRow);

							OnAddRowToResult?.Invoke(this, new AddRowInResultEventArgs(newRow));
							PostSOSEventArgsMessage(string.Format(Strings.FileElaborationEndedWithErrors, attInfo.Name) + result, DiagnosticType.Error, i);
						}
						else
						{
							nrAttWithSuccess++;
							PostSOSEventArgsMessage(string.Format(Strings.FileElaborationSuccessfullyCompleted, attInfo.Name), DiagnosticType.Information, i);
						}
					}
					tbDoc.Close();

					PostSOSEventArgsMessage(string.Format(Strings.ElaborationCompleted, nrAttWithSuccess.ToString(), attList.Count.ToString()));
				}
			}

			AdjustAttachmentsFinished?.Invoke(this, EventArgs.Empty);
		}

		//--------------------------------------------------------------------------------
		public void SearchSosDocuments(SosSearchRules searchRules)
		{
			while (startSearchSOSDocuments)
				Thread.Sleep(1500);

			startSearchSOSDocuments = true;

			//Thread thread = new Thread(() =>
			//{
			SearchSosDocumentsThread(searchRules);
			startSearchSOSDocuments = false;
			//});

			//thread.SetApartmentState(ApartmentState.STA);
			//// quando si istanzia un nuovo Thread bisogna assegnargli le CurrentCulture, altrimenti le
			//// traduzioni in lingue differenti da quelle del sistema operativo non funzionano!!!
			//thread.CurrentUICulture = Thread.CurrentThread.CurrentUICulture;
			//thread.CurrentCulture = Thread.CurrentThread.CurrentCulture;

			//thread.Start();

			SearchFinished?.Invoke(this, EventArgs.Empty);
		}

		//--------------------------------------------------------------------------------
		private void SearchSosDocumentsThread(SosSearchRules searchRules)
		{
			SearchResultCleared?.Invoke(this, EventArgs.Empty);

			SearchResultDataTable searchResultDT = new SearchResultDataTable();
			try
			{
				using (SqlConnection myConnection = new SqlConnection(DMSOrchestrator.DMSConnectionString))
				{
					myConnection.Open();

					using (SqlCommand myCommand = myConnection.CreateCommand())
					{
                        myCommand.CommandText = GetSearchSosDocumentCmdString(searchRules);

						using (SqlDataReader sdr = myCommand.ExecuteReader())
						{
							while (sdr.Read())
							{
								string docKeyDescri = DMSOrchestrator.SearchManager.GetDocumentKeyDescription((int)sdr["AttachmentID"]);

								DataRow newRow = searchResultDT.NewRow();
								newRow[CommonStrings.ArchivedDocID]		= sdr["ArchivedDocID"].ToString();
								newRow[CommonStrings.AttachmentID]		= (int)sdr["AttachmentID"];
								newRow[CommonStrings.Collector]			= -1;
								newRow[CommonStrings.Collection]		= (int)sdr["CollectionID"];
								newRow[CommonStrings.DocNamespace]		= sdr["DocumentType"].ToString();
								newRow[CommonStrings.TBPrimaryKey]		= docKeyDescri;
								newRow[CommonStrings.Name]				= sdr["Name"].ToString();
								newRow[CommonStrings.AttachmentDate]	= (sdr["TBCreated"] is DBNull) ? DateTime.MinValue : (DateTime)sdr["TBCreated"];
								newRow[CommonStrings.ExtensionType]		= sdr["ExtensionType"].ToString();
								newRow[CommonStrings.DocKeyDescription] = docKeyDescri;
								newRow[CommonStrings.DocStatus]			= (int)sdr["DocumentStatus"];
								newRow[CommonStrings.AttachmentInfo]	= DMSOrchestrator.SearchManager.GetAttachmentInfoFromAttachmentId((int)newRow[CommonStrings.AttachmentID]);
								searchResultDT.Rows.Add(newRow);

								OnAddRowToResult?.Invoke(this, new AddRowInResultEventArgs(newRow));
							}
						}
					}
				}
			}
			catch (SqlException e)
			{
				SetMessage(Strings.ErrorSearchingAttachment, e, "SearchSosDocumentsThread");
			}			
		}

		/// <summary>
		/// Metodo richiamato dal C++ nel SOSDocSender wizard
		/// </summary>
		//--------------------------------------------------------------------------------
		public SearchResultDataTable GetSOSDocuments(SosSearchRules searchRules)
		{
			SearchResultDataTable searchResultDT = new SearchResultDataTable();

			try
			{
				using (SqlConnection myConnection = new SqlConnection(DMSOrchestrator.DMSConnectionString))
				{
					myConnection.Open();

					using (SqlCommand myCommand = myConnection.CreateCommand())
					{
						myCommand.CommandText = GetSearchSosDocumentCmdString(searchRules);

						using (SqlDataReader sdr = myCommand.ExecuteReader())
						{
							while (sdr.Read())
							{
								string docKeyDescri = DMSOrchestrator.SearchManager.GetDocumentKeyDescription((int)sdr["AttachmentID"]);

								DataRow newRow = searchResultDT.NewRow();
								newRow[CommonStrings.ArchivedDocID]		= sdr["ArchivedDocID"].ToString();
								newRow[CommonStrings.AttachmentID]		= (int)sdr["AttachmentID"];
								newRow[CommonStrings.Collector]			= -1;
								newRow[CommonStrings.Collection]		= (int)sdr["CollectionID"];
								newRow[CommonStrings.DocNamespace]		= sdr["DocumentType"].ToString();
								newRow[CommonStrings.TBPrimaryKey]		= docKeyDescri;
								newRow[CommonStrings.Name]				= sdr["Name"].ToString();
								newRow[CommonStrings.AttachmentDate]	= (sdr["TBCreated"] is DBNull) ? DateTime.MinValue : (DateTime)sdr["TBCreated"];
								newRow[CommonStrings.ExtensionType]		= sdr["ExtensionType"].ToString();
								newRow[CommonStrings.DocKeyDescription] = docKeyDescri;
								newRow[CommonStrings.DocStatus]			= (int)sdr["DocumentStatus"];
								newRow[CommonStrings.AttachmentInfo]	= DMSOrchestrator.SearchManager.GetAttachmentInfoFromAttachmentId((int)newRow[CommonStrings.AttachmentID]);
								searchResultDT.Rows.Add(newRow);

								OnAddRowToResult?.Invoke(this, new AddRowInResultEventArgs(newRow));
							}
						}
					}
				}
			}
			catch (SqlException e)
			{
				SetMessage(Strings.ErrorSearchingAttachment, e, "SearchSosDocumentsThread");
			}

			return searchResultDT;
		}

		//--------------------------------------------------------------------------------
		private IEnumerable<DMS_SOSDocument> GetSearchResult(SosSearchRules searchRules)
		{
			if (searchRules.SosDocumentTypes == null || searchRules.SosDocumentTypes.Count <= 0)
				return null;

            //non filtro sulla classe documentale poichè mi arrivano i tipi documento già filtrati
            IQueryable<DMS_SOSDocument> sosDocs = dc.DMS_SOSDocuments;

            var dTypeValues = PredicateBuilder.False<DMS_SOSDocument>();
            foreach (string sosType in searchRules.SosDocumentTypes)
            {
                string temp = sosType;
                dTypeValues = dTypeValues.Or(p => (p.DocumentType == temp));
            }
            //filtro su documentType
            sosDocs = sosDocs.Where(dTypeValues);

            //filtro su documentStatus
            var dStatusValues = PredicateBuilder.False<DMS_SOSDocument>();

            foreach (StatoDocumento sosStat in searchRules.SosDocumentStatus)
            {
                int temp = (int)sosStat;
                dStatusValues = dStatusValues.Or(p => (p.DocumentStatus == temp));
            }
            sosDocs = sosDocs.Where(dStatusValues);

            //filtro sul TaxJournal		
            sosDocs = sosDocs.Where(s => s.TaxJournal == searchRules.SosTaxJournal);

            //order by document type, document primary key and modified data
            sosDocs.OrderBy(s => s.DocumentType).ThenBy(s => s.DMS_Attachment.DMS_ErpDocument.PrimaryKeyValue).ThenBy(s => s.DMS_Attachment.TBModified);

            return sosDocs.AsEnumerable();
		}	 

        //--------------------------------------------------------------------------------
        public AutoCompleteStringCollection GetFieldValues(string sosKeyCode, List<string> docTypes, string taxJournal, List<StatoDocumento> sosDocumentStatus, string fiscalYear)
		{
            AutoCompleteStringCollection collection = new AutoCompleteStringCollection();
			try
			{
				using (SqlConnection myConnection = new SqlConnection(DMSOrchestrator.DMSConnectionString))
				{
					myConnection.Open();

					using (SqlCommand myCommand = myConnection.CreateCommand())
					{

                        string docState = string.Empty;
                        foreach (StatoDocumento state in sosDocumentStatus)
                        {
                            if (!string.IsNullOrEmpty(docState))
                                docState += " OR ";
                            docState += string.Format("s.DocumentStatus = {0}", ((int)state).ToString());
                        }
                        
                        string docType = string.Empty;
                        foreach (string docTemp in docTypes)
                        { 
                            if (!string.IsNullOrEmpty(docType))
                                docType += " OR ";
                            docType += string.Format("s.DocumentType = '{0}'", docTemp);
                        }
    
                        // prima aggiorno la riga con il Content vuoto
						myCommand.CommandText = string.Format
							(
                                @"SELECT DISTINCT FieldValue FROM DMS_SearchFieldIndexes f, DMS_AttachmentSearchIndexes i, DMS_SOSDocument s, DMS_CollectionsFields c, DMS_Attachment a
                                WHERE ({0}) AND ({1}) AND FiscalYear = '{2}' AND {3}a.AttachmentID = s.AttachmentID AND 
                                f.SearchIndexID = i.SearchIndexID AND i.AttachmentID = a.AttachmentID AND c.CollectionID = a.CollectionID AND 
                                (c.SosKeyCode ='{4}' AND f.FieldName = c.FieldName) ORDER BY FieldValue",
                                docState, docType, fiscalYear, (string.IsNullOrEmpty(taxJournal)) ? "" : string.Format("s.TaxJournal = '{0}' AND ", taxJournal), sosKeyCode
							);
						SqlDataReader sdr = myCommand.ExecuteReader();
                        while (sdr.Read())
                            collection.Add(sdr["FieldValue"].ToString());
                        sdr.Close();
					}
				}
			}
			catch (SqlException e)
			{
                SetMessage(string.Format(Strings.Error, e.Message), e, "GetFieldValues");
			}

            return collection;
		}

		/// <summary>
		/// da richiamare nella batch di adjustsosdocument
		/// </summary>
		/// <param name="sosKeyCode"></param>
		/// <param name="docType"></param>
		/// <returns></returns>
		//--------------------------------------------------------------------------------
		public AutoCompleteStringCollection GetAttachmentsFieldValues(string sosKeyCode, ERPSOSDocumentType docType)
		{
			AutoCompleteStringCollection collection = new AutoCompleteStringCollection();
			try
			{
				using (SqlConnection myConnection = new SqlConnection(DMSOrchestrator.DMSConnectionString))
				{
					myConnection.Open();

					using (SqlCommand myCommand = myConnection.CreateCommand())
					{

						// prima aggiorno la riga con il Content vuoto
						myCommand.CommandText = string.Format
							(
								@"SELECT DISTINCT FieldValue FROM DMS_SearchFieldIndexes f, DMS_AttachmentSearchIndexes i, DMS_CollectionsFields c, DMS_Attachment a, DMS_ErpDocument e
                                WHERE e.DocNamespace = '{0}' AND a.ErpDocumentID = e.ErpDocumentID AND 
                                f.SearchIndexID = i.SearchIndexID AND i.AttachmentID = a.AttachmentID AND c.CollectionID = a.CollectionID AND 
                                (c.SosKeyCode ='{1}' AND f.FieldName = c.FieldName) ORDER BY FieldValue",
								docType.DocNamespace, sosKeyCode
							);
						SqlDataReader sdr = myCommand.ExecuteReader();
						while (sdr.Read())
							collection.Add(sdr["FieldValue"].ToString());
						sdr.Close();
					}
				}
			}
			catch (SqlException e)
			{
				SetMessage(string.Format(Strings.Error, e.Message), e, "GetAttachmentsFieldValues");
			}

			return collection;
		}

		/*
        Select a.ArchivedDocID, a.AttachmentID, a.CollectionID, d.DocumentType, d.[FileName], a.TBCreated from  DMS_SOSDocument d, DMS_Attachment a
                                where (d.DocumentStatus = 5 OR d.DocumentStatus = 1) AND (d.DocumentType = 'Nota di credito' OR d.DocumentType = 'Fattura Immediata' OR d.DocumentType = 'Fattura Accompagnatoria') 
                                AND d.TaxJournal = 'VEN' AND a.AttachmentID = d.AttachmentID 
         */
		//--------------------------------------------------------------------------------
		private string GetSimpleSearchString(SosSearchRules searchRules)
        {
            string strSimpleCmd;

            string docState = string.Empty;
            foreach (StatoDocumento state in searchRules.SosDocumentStatus)
            {
                if (!string.IsNullOrEmpty(docState))
                    docState += " OR ";
                docState += string.Format("s.DocumentStatus = {0}", ((int)state).ToString());
            }

            string docType = string.Empty;
            foreach (string docTemp in searchRules.SosDocumentTypes)
            {
                if (!string.IsNullOrEmpty(docType))
                    docType += " OR ";
                docType += string.Format("s.DocumentType = '{0}'", docTemp);
            }

            // prima aggiorno la riga con il Content vuoto
            strSimpleCmd = string.Format
                (
                    @"SELECT a.ArchivedDocID, a.AttachmentID, a.CollectionID, s.DocumentType, d.Name, d.ExtensionType, a.TBCreated FROM DMS_SOSDocument s, DMS_Attachment a, DMS_ArchivedDocument d
                     WHERE ({0}) AND ({1}) AND FiscalYear = '{2}' AND {3}d.ArchivedDocID =a.ArchivedDocID AND s.AttachmentID = a.AttachmentID ORDER BY a.TBCreated, s.DocumentType",
                    docState, docType, searchRules.FiscalYear, (string.IsNullOrEmpty(searchRules.SosTaxJournal)) ? "" : string.Format("s.TaxJournal = '{0}' AND ", searchRules.SosTaxJournal)
                );

            return strSimpleCmd;
        }


		/*
		La query di estrazione di un documenti dal DMS_Attachment per la preparazione o  dalla DMS_SOSDocument per l'invio deve verificare le condizioni imposte dall'utente che possiamo dividere in
		Condizioni di base: sono quelle relative alla stato del documento, al tipo di documento ed eventualmente se presente al registro IVA
		Condizione gestionali: possono essere presenti uno o più condizioni di gestionali. Queste si differenziano a seconda del Classe documentale scelta
			DOCUMENTI EMESSI
			- Data documento
			- Data emissione documento
			- Numero documento
			- Cliente 
			DOCUMENTI RICEVUTI
			- Data documento
			- Data emissione documento
			- Numero documento
			- Numero documento fornitore
			- Fornitore
			REGISTRI IVA, LIBRO GIORNALE, LIBRO CESPITI, DICHIARAZIONE INTENTI
			- Anno fiscale
			- Data inizio transazione 
			- Data fine transazione
		 * 
		//Nel caso in cui l'utente vada a specificare più condizioni gestionali è necessario intersecare i risultati 
		 * Esempio nel caso di preparazione di allegati non ancora inclusi in DMS_SOSDocument
		   (Select a.AttachmentID from DMS_Attachment a, DMS_ErpDocument e, DMS_CollectionsFields c, DMS_SearchFieldIndexes f, DMS_AttachmentSearchIndexes s where e.DocNamespace = 'Document.ERP.Sales.Documents.Invoice' AND a.ErpDocumentID = e.ErpDocumentID 
			AND NOT EXISTS (Select s.AttachmentID from DMS_SOSDocument s where s.AttachmentID = a.AttachmentID) AND c.CollectionID = a.CollectionID  AND 
		 * (c.SosKeyCode ='CustSupp' AND f.FieldName = c.FieldName and f.FieldValue between '01041271' and '01042768') AND s.SearchIndexID = f.FieldName AND .AttachmentID = a.AttachmentID  ) 
		 * 
		 * 
		 *      // Esempio nel caso di invio:
			(Select a.ArchivedDocID, a.AttachmentID, a.CollectionID, d.DocumentType, d.[FileName], a.TBCreated from DMS_SearchFieldIndexes f, DMS_AttachmentSearchIndexes s, DMS_SOSDocument d, DMS_CollectionsFields c, DMS_Attachment a
			  where (d.DocumentStatus = 5 OR d.DocumentStatus = 1) AND (d.DocumentType = 'Nota di credito' OR d.DocumentType = 'Fattura Immediata' OR d.DocumentType = 'Fattura Accompagnatoria') AND d.TaxJournal = 'VEN' AND a.AttachmentID = d.AttachmentID AND 
			  f.SearchIndexID = s.SearchIndexID AND s.AttachmentID = a.AttachmentID AND c.CollectionID = a.CollectionID AND                                   
			 (c.SosKeyCode ='CustSupp' AND f.FieldName = c.FieldName and f.FieldValue between 'TEC000001' and 'TEC00021'))                                    
			INTERSECT
			(Select d.* from DMS_SearchFieldIndexes f, DMS_AttachmentSearchIndexes s, DMS_SOSDocument d, DMS_CollectionsFields c, DMS_Attachment a
			where (d.DocumentStatus = 5 OR d.DocumentStatus = 1) AND (d.DocumentType = 'Nota di credito' OR d.DocumentType = 'Fattura Immediata' OR d.DocumentType = 'Fattura Accompagnatoria') AND d.TaxJournal = 'VEN' AND a.AttachmentID = d.AttachmentID AND 
			f.SearchIndexID = s.SearchIndexID AND s.AttachmentID = a.AttachmentID AND c.CollectionID = a.CollectionID AND 
			(c.SosKeyCode ='DocNo' AND f.FieldName = c.FieldName and f.FieldValue between '000027/14' and '001769/14'))
         
		 Per ottimizzare possiamo pensare ad una prima parte di stringa comprendente i criteri di base. Tale stringa deve essere utilizzata poi da tutte le query delle INTERESECT
		 */
		//--------------------------------------------------------------------------------
		private string GetSingleCmdString(string strBaseCmd, ERPFieldRule fieldRule)
		{
			string strValue = string.Empty;
			string strSingleCmd = string.Empty;

			if (string.IsNullOrEmpty(fieldRule.FieldName) || (string.IsNullOrEmpty(fieldRule.FromValue) && string.IsNullOrEmpty(fieldRule.ToValue)))
				return string.Empty;

			//c.Fieldvalue = "value1'
			if (string.Compare(fieldRule.FromValue, fieldRule.ToValue, true) == 0)
				strValue = string.Format("f.FieldValue = '{0}'", fieldRule.FromValue);
			else
			{
				if (!string.IsNullOrEmpty(fieldRule.FromValue) && string.IsNullOrEmpty(fieldRule.ToValue))
					strValue = string.Format("f.FieldValue >= '{0}'", fieldRule.FromValue);
				else
				{
					if (string.IsNullOrEmpty(fieldRule.FromValue) && !string.IsNullOrEmpty(fieldRule.ToValue))
						strValue = string.Format("f.FieldValue <= '{0}'", fieldRule.ToValue);
					else
						strValue = string.Format("f.FieldValue BETWEEN '{0}' AND '{1}'", fieldRule.FromValue, fieldRule.ToValue);
				}
			}

			strSingleCmd = string.Format("{0} AND (c.SosKeyCode ='{1}' AND f.FieldName = c.FieldName AND {2})", strBaseCmd, fieldRule.FieldName, strValue);

			return strSingleCmd;
		}

		//--------------------------------------------------------------------------------
		private string GetSearchSosDocumentCmdString(SosSearchRules searchRules)
		{
			string strValue = string.Empty;
			string strCompleteCmd = string.Empty;

			//non uso il filtraggio gestionale
			if (!searchRules.UseERPFilter())
			{
				string docState = string.Empty;
				foreach (StatoDocumento state in searchRules.SosDocumentStatus)
				{
					if (!string.IsNullOrEmpty(docState))
						docState += " OR ";
					docState += string.Format("s.DocumentStatus = {0}", ((int)state).ToString());
				}

				string docType = string.Empty;
				foreach (string docTemp in searchRules.SosDocumentTypes)
				{
					if (!string.IsNullOrEmpty(docType))
						docType += " OR ";
					docType += string.Format("s.DocumentType = '{0}'", docTemp);
				}

				// prima aggiorno la riga con il Content vuoto
				strCompleteCmd = string.Format
					(
						@"SELECT a.ArchivedDocID, a.AttachmentID, a.CollectionID, s.DocumentType, s.DocumentStatus, d.Name, d.ExtensionType, a.TBCreated 
						FROM DMS_SOSDocument s, DMS_Attachment a, DMS_ArchivedDocument d
                        WHERE ({0}) AND ({1}) AND FiscalYear = '{2}' AND {3}{4}d.ArchivedDocID = a.ArchivedDocID AND s.AttachmentID = a.AttachmentID order by a.TBCreated, s.DocumentType",
						docState, docType, searchRules.FiscalYear,
						(string.IsNullOrEmpty(searchRules.SosTaxJournal)) ? "" : string.Format("s.TaxJournal = '{0}' AND ", searchRules.SosTaxJournal),
						searchRules.OnlyMainDoc ? "a.IsMainDoc = 1 AND " : ""
					);
			}
			else //devo fare tante query da intersecare tra loro quanti sono i criteri gestionali impostati
			{
				string strSingleQuery = string.Empty;
				string strBaseCmd;

				string docState = string.Empty;
				foreach (StatoDocumento state in searchRules.SosDocumentStatus)
				{
					if (!string.IsNullOrEmpty(docState))
						docState += " OR ";
					docState += string.Format("s.DocumentStatus = {0}", ((int)state).ToString());
				}

				string docType = string.Empty;
				foreach (string docTemp in searchRules.SosDocumentTypes)
				{
					if (!string.IsNullOrEmpty(docType))
						docType += " OR ";
					docType += string.Format("s.DocumentType = '{0}'", docTemp);
				}

				// prima aggiorno la riga con il Content vuoto
				strBaseCmd = string.Format
					(
						@"SELECT a.ArchivedDocID, a.AttachmentID, a.CollectionID, s.DocumentType, s.DocumentStatus, d.Name, d.ExtensionType, a.TBCreated 
						FROM DMS_SearchFieldIndexes f, DMS_AttachmentSearchIndexes i, DMS_SOSDocument s, DMS_CollectionsFields c, DMS_Attachment a, DMS_ArchivedDocument d
                        WHERE ({0}) AND ({1}) AND FiscalYear = '{2}' AND {3}{4}d.ArchivedDocID = a.ArchivedDocID AND a.AttachmentID = i.AttachmentID AND 
                        f.SearchIndexID = i.SearchIndexID AND s.AttachmentID = a.AttachmentID AND c.CollectionID = a.CollectionID",
						docState, docType, searchRules.FiscalYear,
						(string.IsNullOrEmpty(searchRules.SosTaxJournal)) ? "" : string.Format("s.TaxJournal = '{0}' AND ", searchRules.SosTaxJournal),
						searchRules.OnlyMainDoc ? "a.IsMainDoc = 1 AND " : ""
					);

				foreach (ERPFieldRule fieldRule in searchRules.ERPFieldsRuleList.ERPFieldsRules)
				{
					strSingleQuery = GetSingleCmdString(strBaseCmd, fieldRule);
					strCompleteCmd = (string.IsNullOrEmpty(strCompleteCmd)) ? strSingleQuery : string.Format("({0}) INTERSECT ({1})", strCompleteCmd, strSingleQuery);
				}

				if (!string.IsNullOrEmpty(strCompleteCmd))
					strCompleteCmd += " ORDER BY a.TbCreated, s.DocumentType";
			}

			return strCompleteCmd;
		}

		///<summary>
		/// Per preparare i documenti e lo zip con l'envelope lato client (in caso di FTP)
		///</summary>
		//--------------------------------------------------------------------------------
		public bool PrepareSOSEnvelopeForFTP(List<int> attachmentIds)
		{
			return coreSosManager.PrepareSOSEnvelopeForFTP(attachmentIds);
		}

		///<summary>
		/// Per eliminare i documenti dalla directory temporanea
		///</summary>
		//--------------------------------------------------------------------------------
		public void DeleteTemporaryFiles()
		{
			coreSosManager.DeleteTemporaryFiles();
		}

				//dato lo stato del documento in sostitutiva restituisce il messaggio in chiaro da fornire all'utente
		//--------------------------------------------------------------------------------
		internal string GetDocumentSOSStatusMessage(StatoDocumento sosStatus)
		{
			switch (sosStatus)
			{
				case StatoDocumento.EMPTY:
					return string.Empty;
				case StatoDocumento.IDLE:
					return Strings.DocIdleDocumentStatus;
				case StatoDocumento.WAITING:
					return Strings.DocWaitingDocumentStatus;
				case StatoDocumento.TOSEND:
					return Strings.DocToSendDocumentStatus;
				case StatoDocumento.SENT:
					return Strings.DocSentDocumentStatus;
				case StatoDocumento.TORESEND:
					return Strings.DocToResendDocumentStatus;
				case StatoDocumento.DOCTEMP:
					return Strings.DocTempDocumentStatus;
				case StatoDocumento.DOCSTD:
					return Strings.DocStdDocumentStatus;
				case StatoDocumento.DOCRDY:
					return Strings.DocRdyDocumentStatus;
				case StatoDocumento.DOCSIGN:
					return Strings.DocSignDocumentStatus;
				case StatoDocumento.DOCREP:
					return Strings.DocRepDocumentStatus;
				case StatoDocumento.DOCKO:
					return Strings.DocKODocumentStatus;
				default:
					return string.Empty;
			}
		}

		//restituisce lo stato del documento in sostitutiva e in msgStatus il messaggio in chiaro da fornire all'utente
		//--------------------------------------------------------------------------------
		internal void GetDocumentSOSInfo(int erpDocumentID, out StatoDocumento sosStatus, out int attachmentID, out string msgStatus)
		{
			sosStatus = StatoDocumento.EMPTY;
			msgStatus = string.Empty;
			attachmentID = -1;
			try
			{
				if (erpDocumentID == -1)
					return;

				var var = from att in dc.DMS_Attachments
						  where att.ErpDocumentID == erpDocumentID && att.IsMainDoc == true && att.LotID != string.Empty
						  orderby att.TBModified descending
						  select att.AttachmentID;

				if (var != null && var.Any())
				{
					sosStatus = StatoDocumento.DOCSIGN;
					attachmentID = var.First();
					msgStatus = Strings.DocSignDocumentStatus;
					return;
				}

				var docStatus = from att in dc.DMS_Attachments
								join s in dc.DMS_SOSDocuments on att.AttachmentID equals s.AttachmentID
								where att.ErpDocumentID == erpDocumentID && att.IsMainDoc == true
								orderby att.TBModified descending
								select new { sosStatus = att.DMS_SOSDocument.DocumentStatus, attachmentID = att.AttachmentID };

				if (docStatus != null && docStatus.Any())
				{
					var first = docStatus.First();
					if (first != null)
					{
						sosStatus = (StatoDocumento)first.sosStatus;
						attachmentID = first.attachmentID;
						msgStatus = GetDocumentSOSStatusMessage(sosStatus);
						return;
					}
				}
			}
			catch (Exception e)
			{
				SetMessage(Strings.ErrorSearchingAttachment, e, "GetDocumentSOSStatus");
			}
        }
		/// <summary>
		/// Metodo richiamato dal C++ per escludere gli allegati dall'invio della SOS
		/// Viene eliminata la riga nella tabella DMS_SOSDocument corrispondente all'AttachmentID
		/// </summary>
		/// <param name="attList">lista di AttachmentInfo da escludere</param>
		/// <returns></returns>
		//--------------------------------------------------------------------------------
		public bool RemoveAttachmentsFromSOS(List<AttachmentInfo> attList)
		{
			Thread myThread = new Thread(() => InternalRemoveAttachmentsFromSOS(attList));
			myThread.CurrentUICulture = Thread.CurrentThread.CurrentUICulture;
			myThread.CurrentCulture = Thread.CurrentThread.CurrentCulture;
			myThread.Start();

			return true;
		}

		/// <summary>
		/// Metodo richiamato dal C++ per inviare gli allegati dal SOSDocSender
		/// </summary>
		/// <param name="attList">lista di AttachmentInfo da inviare</param>
		/// <returns></returns>
		//--------------------------------------------------------------------------------
		public bool SendAttachmentsToSos(List<AttachmentInfo> attList)
		{
			Thread myThread = new Thread(() => InternalPrepareAttachmentsToSend(attList));
			myThread.CurrentUICulture = Thread.CurrentThread.CurrentUICulture;
			myThread.CurrentCulture = Thread.CurrentThread.CurrentCulture;
			myThread.Start();

			return true;
		}

		///<summary>
		/// Metodo che si occupa di convertire in PDF/A i documenti selezionati nel grid
		///</summary>
		//-------------------------------------------------------------------------------------
		private void InternalPrepareAttachmentsToSend(List<AttachmentInfo> attList)
		{
			Cursor.Current = Cursors.WaitCursor;

			bool sosSenderIsRunning = true;

			Debug.WriteLine(string.Format("---- Inizio Elaborazione nr. {0} Allegati da inviare in SOS", attList.Count.ToString()));

			List<int> attachmentIds = PrepareAttachmentsToSend(attList);

			Debug.WriteLine(string.Format("---- Fine Elaborazione Allegati da inviare in SOS (validi {0} di {1} documenti)", attachmentIds.Count.ToString(), attList.Count.ToString()));

			bool result = false;

			if (attachmentIds.Count > 0)
			{
				// se nei parametri ho scelto di inviare i documenti via FTP genero l'envelope e lo zip nel folder specificato
				if ((bool)SOSConfigurationState.SOSConfiguration.FTPSend)
					result = PrepareSOSEnvelopeForFTP(attachmentIds);
				else
				{
					// elimino i file dalla directory temporanea del client
					DMSOrchestrator.SosManager.DeleteTemporaryFiles();

					// altrimenti mando un evento all'EASync tramite il DMSOrchestrator in modo da avviare l'invio alla SOS via WS
					DMSOrchestrator.EnqueueAttachmentsToSend(attachmentIds);
					result = true; // metto d'ufficio a true perche' e' demandato tutto al Sync
				}
			}
			else
				PostSOSEventArgsMessage(Strings.SOSNoDocumentsToSend);

			Cursor.Current = Cursors.Default;
			sosSenderIsRunning = false;
			if (!sosSenderIsRunning)
			{
				if (attachmentIds.Count > 0)
				{
					if (result)
					{
						if ((bool)SOSConfigurationState.SOSConfiguration.FTPSend)
						{
							PostSOSEventArgsMessage(string.Format(Strings.FTPElaborationCompleted, attachmentIds.Count.ToString(), attList.Count.ToString()));

							// imposto il link nel folder della cartella condivisa dell'FTP
							// decidere se mettere un link per l'apertura del folder FTP
							//string fileToOpenWithLink = SOSConfigurationState.SOSConfiguration.FTPSharedFolder;
							//SafeGui.ControlText(LinkLblOpenFile, Strings.OpenFTPFolderLink);
						}
						else
							PostSOSEventArgsMessage(string.Format(Strings.SOSElaborationCompleted, attachmentIds.Count.ToString(), attList.Count.ToString()));
					}
					else
					{
						PostSOSEventArgsMessage(Strings.ElaborationCompletedWithErrors);
						// imposto il link nel folder nella cartella/file di log
						//string fileToOpenWithLink = DMSOrchestrator.SosConnectorLogFilePath;
						//SafeGui.ControlText(LinkLblOpenFile, Strings.OpenSOSLogFileLink);
					}
				}
			}

			if (SOSSendFinished != null)
				SOSSendFinished(this, EventArgs.Empty);
		}

		///<summary>
		/// Preparazione degli allegati per l'invio in SOS
		/// Per tutti i documenti selezionati nel bodyedit (passati come parametro)
		/// tramite l'attachmentId viene caricato il binario e serializzato in un file temporaneo, 
		/// poi effettuata la conversione in PDF/A, la scrittura delle chiavi, hashcode e l'aggiornamento dello stato
		///</summary>
		//--------------------------------------------------------------------------------
		private List<int> PrepareAttachmentsToSend(List<AttachmentInfo> attList)
		{
			List<int> idsList = new List<int>();

			if (attList == null || attList.Count == 0)
				return idsList;

			PostSOSEventArgsMessage(string.Format(Strings.StartingDocumentsElaboration, attList.Count.ToString()));
			SOSLogWriter.WriteLogEntry(DMSOrchestrator.UtilsManager.CompanyName, "Start preparing attachments to send", "SOSMonitor::PrepareAttachmentsToSend");

			for (int i = 0; i < attList.Count; i++)
			{
				AttachmentInfo ai = attList[i];

				string message = string.Format(Strings.FileElaborationInProgress, ai.Name, (i + 1).ToString(), attList.Count.ToString());
				PostSOSEventArgsMessage(message, DiagnosticType.None, i);

				if (!UpdateSosDocument(ai))
				{
					PostSOSEventArgsMessage(string.Format(Strings.FileElaborationEndedWithErrors, ai.Name), DiagnosticType.Error, i);
					// tengo traccia nel log dei documenti che non sono riuscita a convertire
					SOSLogWriter.AppendText(DMSOrchestrator.UtilsManager.CompanyName, string.Format(Strings.ErrorUpdatingSOSDoc, ai.AttachmentId) + ". The document will be skipped.");
					continue;
				}
				else
					PostSOSEventArgsMessage(string.Format(Strings.FileElaborationSuccessfullyCompleted, ai.Name), DiagnosticType.Information, i);

				int x = ai.AttachmentId;
				if (!idsList.Contains(x))
					idsList.Add(x);
			}

			SOSLogWriter.WriteLogEntry(DMSOrchestrator.UtilsManager.CompanyName, string.Format("End preparing attachments to send (nr. {0} documents ready to sent)", idsList.Count.ToString()), "SOSMonitor::PrepareAttachmentsToSend");

			return idsList;
		}

		//--------------------------------------------------------------------------------
		private void InternalRemoveAttachmentsFromSOS(List<AttachmentInfo> attList)
		{
			PostSOSEventArgsMessage(string.Format(Strings.StartingDocumentsElaboration, attList.Count.ToString()));

			for (int i = 0; i < attList.Count; i++)
			{
				AttachmentInfo ai = attList[i];

				string message = string.Format(Strings.FileElaborationInProgress, ai.Name, (i + 1).ToString(), attList.Count.ToString());
				PostSOSEventArgsMessage(message, DiagnosticType.None, i);

				if (!DeleteSosDocument(ai.AttachmentId))
				{
					PostSOSEventArgsMessage(string.Format(Strings.FileElaborationEndedWithErrors, ai.Name), DiagnosticType.Error, i);
					continue;
				}
				else
					PostSOSEventArgsMessage(string.Format(Strings.FileElaborationSuccessfullyCompleted, ai.Name), DiagnosticType.Information, i);
			}

			if (SOSSendFinished != null)
				SOSSendFinished(this, EventArgs.Empty);
		}

		/// <summary>
		/// Evento intercettato dal C++ per aggiornare lo stato dell'elaborazione nel SOSDocSender
		/// </summary>
		/// <param name="message">testo da visualizzare</param>
		/// <param name="type">None, Error, Information</param>
		/// <param name="idx">indice della riga da aggiornare</param>
		//--------------------------------------------------------------------------------
		private void PostSOSEventArgsMessage(string message, DiagnosticType type = DiagnosticType.None, int idx = -1)
		{
			// se idx == -1 viene aggiornato il testo dello static sopra la progress bar, altrimenti le righe nel dbt
			if (SOSOperationCompleted != null)
			{
				SOSEventArgs args = new SOSEventArgs();
				args.Message = message;
				args.MessageType = type;
				args.Idx = idx;
				SOSOperationCompleted(this, args);
			}
		}
	}
}