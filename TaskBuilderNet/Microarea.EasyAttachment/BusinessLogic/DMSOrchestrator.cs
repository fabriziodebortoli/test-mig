using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Reflection;
using System.Runtime.InteropServices;

using Microarea.EasyAttachment.Components;
using Microarea.EasyAttachment.Core;
using Microarea.EasyAttachment.UI.Forms;
using Microarea.Framework.TBApplicationWrapper;
using Microarea.TaskBuilderNet.Core.DiagnosticManager;
using Microarea.TaskBuilderNet.Core.DiagnosticUI;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Core.NotificationManager;
using Microarea.TaskBuilderNet.Core.WebServicesWrapper;
using Microarea.TaskBuilderNet.Interfaces;
using Microarea.TaskBuilderNet.Interfaces.Model;

namespace Microarea.EasyAttachment.BusinessLogic
{
	///<summary>
	/// DMSOrchestrator class
	/// </summary>
	//================================================================================
	public class DMSOrchestrator
	{
		private MassiveAttach massiveAttachForm = null;
        private QuickAttachment quickAttachForm = null;
		private BBTrayletForm bbTrayletForm = null;
		private BBSettingsForm bbSettingsForm = null;

		private bool openMassiveAttach = false;
		private bool openQuickAttach = false;
		private bool openBBTraylet = false;
		private bool openBBSettings = false;

		//collection di archiviazione
		private DMS_Collector repositoryCollector = null;
		private DMS_Collection repositoryCollection = null;

		// CompanyLanguage to use for FullTextSearch	
		private int companySID = 1033;

		protected DMSModelDataContext dc = null;

		// Lock Manager
		public EALockManager LockManager = null;
		public string LockContext { get { return GetHashCode().ToString(); } }

		// Wrapper per EasyAttachmentSync
		public EasyAttachmentSync EASync = null;

		// Events
		//--------------------------------------------------------------------------------------------------		
		// Events used from c++ TBRepositoryManager class in order to check the settings
		//public event EventHandler AdminSettingsSaved;
		public event EventHandler<AttachmentInfoEventArgs> ArchiveCompleted;
        public event EventHandler<AttachmentEventArgs> AttachmentDeleted;
        public event EventHandler<CollectionEventArgs> UpdateCollectionCompleted;

        //--------------------------------------------------------------------------------
        public int WorkerId { get; set; }
		public MWorkersTable WorkersTable { get; set; }
        public bool IsAdmin { get; set; }

        //--------------------------------------------------------------------------------
        public bool OpenForm { get; set; }

		//instance running info
		//--------------------------------------------------------------------------------
		public string CompanyCustomPath { get; set; }

		//--------------------------------------------------------------------------------
		public string DMSConnectionString { get; set; }

		//used to call TbDMS web methods from OCRManager
		//--------------------------------------------------------------------------------
		public string AuthenticationToken { get; set; }

		//--------------------------------------------------------------------------------
		public string ServerName { get; set; }

		//--------------------------------------------------------------------------------
		public int ServicePort { get; set; }

		// per sapere se la form del RepoExplorer e' in fase di caricamento
		// se ci sono molti documenti a video non e' cosi' immediato 
		//--------------------------------------------------------------------------------
		public bool RepositoryFormIsLoading { get; set; }

		//Business Logic Managers
		//--------------------------------------------------------------------------------
		public Diagnostic DMSDiagnostic { get; set; }
		//--------------------------------------------------------------------------------
		public ArchiveManager ArchiveManager { get; set; }
		//--------------------------------------------------------------------------------
		public SearchManager SearchManager { get; set; }
		//--------------------------------------------------------------------------------
		public CategoryManager CategoryManager { get; set; }
		//--------------------------------------------------------------------------------
		public SettingsManager SettingsManager { get; set; }
		//--------------------------------------------------------------------------------
		public BarcodeManager BarcodeManager { get; set; }
		//--------------------------------------------------------------------------------
		public OCRManager OCRManager { get; set; }
		//--------------------------------------------------------------------------------
		public FullTextFilterManager FullTextFilterManager { get; set; }
		//--------------------------------------------------------------------------------
		public SOSManager SosManager { get; set; }

        //--------------------------------------------------------------------------------		
        public Utils UtilsManager { get; set; }

        //--------------------------------------------------------------------------------
        public NotificationManager NotificationManager { get; set; }

		//--------------------------------------------------------------------------------
		public string SOSSubjectCode
		{
			get
			{
				if (dc.DMS_SOSConfigurations != null && dc.DMS_SOSConfigurations.Any())
				{
					try
					{
						var sosConfRec = from rec in dc.DMS_SOSConfigurations where rec.ParamID == 0 select rec;
						// seleziono l'unico record con ParamID = 0 se esiste
						DMS_SOSConfiguration sosConf = (sosConfRec != null) ? sosConfRec.Single() : null;
						return (sosConf != null && !string.IsNullOrWhiteSpace(sosConf.SubjectCode)) ? sosConf.SubjectCode : string.Empty;
					}
					catch
					{
						return string.Empty;
					}
				}
				return
					string.Empty;
			}
		}

		// FullTextSearch language support
		//--------------------------------------------------------------------------------
		public int SID
		{
			set
			{
				companySID = value;
				OCRDictionary = OCRDictionaryHelper.GetOCRDictionaryFromLCID(value);
			}
			get { return companySID; }
		}

		//--------------------------------------------------------------------------------
		public OCRDictionary OCRDictionary { get; private set; }
		//

		//--------------------------------------------------------------------------------
		public DMSModelDataContext DataContext
		{
			set { dc = value; }
			get
			{
				if (dc == null)
					dc = new DMSModelDataContext(DMSConnectionString);
				return dc;
			}
		}

		//Checks Security, MailConnector, Full Text Search, SosConnector
		//--------------------------------------------------------------------------------
		public bool SecurityEnabled { get; set; }

		//--------------------------------------------------------------------------------
		public bool MailConnectorEnabled { get; set; }

		//--------------------------------------------------------------------------------
		public bool FullTextSearchEnabled { get; set; }

		//--------------------------------------------------------------------------------
		public bool BarcodeEnabled { get { return SettingsManager.UsersSettingState.Options.BarcodeDetectionOptionsState.EnableBarcode; } }

		//--------------------------------------------------------------------------------
		public bool SearchInContentEnabled { get { return SettingsManager.UsersSettingState.Options.FTSOptionsState.EnableFTS; } }

		//--------------------------------------------------------------------------------
		public bool SosConnectorActivated { get; set; } // isActivated

		//--------------------------------------------------------------------------------
		public bool SosConnectorEnabled
		{
			get
			{
				return
					SosConnectorActivated &&
					SettingsManager.UsersSettingState.Options.SOSOptionsState.EnableSOS &&
					(SOSConfigurationState == null || !string.IsNullOrEmpty(SOSConfigurationState.SOSConfiguration.MySOSUser));
			}
		}

		//---------------------------------------------------------------------
		public SOSConfigurationState SOSConfigurationState { get { return SosManager.SOSConfigurationState; } set { SosManager.SOSConfigurationState = value; } }

		//--------------------------------------------------------------------------------
		virtual public string CollectorName { get { return CommonStrings.EACollector; } }

		//--------------------------------------------------------------------------------
		virtual public string CollectionName { get { return CommonStrings.EACollection; } }

		//--------------------------------------------------------------------------------
		virtual public int CollectorID { get { return (RepositoryCollector != null) ? RepositoryCollector.CollectorID : -1; } }

		//--------------------------------------------------------------------------------
		virtual public int CollectionID { get { return (RepositoryCollection != null) ? RepositoryCollection.CollectionID : -1; } }

		//--------------------------------------------------------------------------------
		virtual public int ERPDocumentID { get { return -1; } }

		//--------------------------------------------------------------------------------
		virtual public MSqlRecord MasterRecord { get { return null; } }


		protected bool inUnattendedMode = false; //viene posto a true solo nelle chiamate effettuate dai webmethods di TBDMS e nelle procedure che non vogliono visualizzare messaggi

		//--------------------------------------------------------------------------------
		virtual public bool InUnattendedMode
		{
			get { return inUnattendedMode; }
			set { inUnattendedMode = value; }
		}

		// Id of company currently connected
		//--------------------------------------------------------------------------------
		public int CompanyID { get; set; }

		// Id of login currently connected (it is not the WorkerId!!!!)
		//--------------------------------------------------------------------------------
		public int LoginId { get; set; }

		//---------------------------------------------------------------------
		public DMS_Collector RepositoryCollector
		{
			get
			{
				if (repositoryCollector != null)
					return repositoryCollector;

				var collector = from coll in dc.DMS_Collectors
								where coll.Name == CommonStrings.EACollector
								select coll;

				repositoryCollector = (collector != null && collector.Any()) ? (DMS_Collector)collector.Single() : null;
				try
				{
					//non è  ancora presente, lo inserisco
					if (repositoryCollector == null)
					{
						string lockErr = string.Empty;
						if (LockManager.LockRecord("DMS_Collector", CommonStrings.EACollector, LockContext, ref lockErr))
						{
							DMS_Collector newCollector = new DMS_Collector();
							//nessuno lo sta inserendo, allora lo inserisco io
							newCollector.Name = CommonStrings.EACollector;
							newCollector.IsStandard = true;
							dc.DMS_Collectors.InsertOnSubmit(newCollector);
							dc.SubmitChanges();
							LockManager.UnlockRecord("DMS_Collector", CommonStrings.EACollector, LockContext);
							repositoryCollector = newCollector;
						}
						else
						{
							MessageEventArgs arg = new MessageEventArgs();
							arg.Explain = lockErr;
							arg.MessageType = DiagnosticType.Error;
							ShowMessage(this, arg);
							repositoryCollector = null;
						}
					}
				}
				catch (Exception e)
				{
					MessageEventArgs arg = new MessageEventArgs();
					arg.Explain = e.Message;
					arg.MessageType = DiagnosticType.Error;
					ShowMessage(this, arg);
					repositoryCollector = null;
				}
				return repositoryCollector;
			}
		}

		//---------------------------------------------------------------------
		public DMS_Collection RepositoryCollection
		{
			get
			{
				if (repositoryCollection != null)
					return repositoryCollection;

				try
				{
					var collection = from coll in dc.DMS_Collections
									 where
									 coll.DMS_Collector.Name == CommonStrings.EACollector &&
									 coll.Name == CommonStrings.EACollection &&
									 coll.IsStandard == true
									 select coll;

					repositoryCollection = (collection != null && collection.Any()) ? (DMS_Collection)collection.Single() : null;
					//non è  ancora presente, lo inserisco
					if (repositoryCollection == null)
					{
						string lockErr = string.Empty;
						if (LockManager.LockRecord("DMS_Collection", CommonStrings.EACollection, LockContext, ref lockErr))
						{
							DMS_Collection newCollection = new DMS_Collection();
							newCollection.Name = CommonStrings.EACollection;
							newCollection.TemplateName = "Standard";
							newCollection.CollectorID = RepositoryCollector.CollectorID;
							newCollection.IsStandard = true;
							newCollection.SosDocClass = "";
							newCollection.Version = 1;
							dc.DMS_Collections.InsertOnSubmit(newCollection);
							dc.SubmitChanges();
							LockManager.UnlockRecord("DMS_Collection", CommonStrings.EACollection, LockContext);
							repositoryCollection = newCollection;
						}
						else
						{
							MessageEventArgs arg = new MessageEventArgs();
							arg.Explain = lockErr;
							arg.MessageType = DiagnosticType.Error;
							ShowMessage(this, arg);
							repositoryCollection = null;
						}
					}
				}
				catch (Exception e)
				{
					MessageEventArgs arg = new MessageEventArgs();
					arg.Explain = e.Message;
					arg.MessageType = DiagnosticType.Error;
					ShowMessage(this, arg);
					repositoryCollection = null;
				}

				return repositoryCollection;
			}
		}

		//--------------------------------------------------------------------------------------------------
		virtual public TabPage CurrentTabPage { get { return null; } }

		/// restituisce le estensioni text compatible lette dalla tabella DMS_TextExtensions		
		//--------------------------------------------------------------------------------------------------
		public List<string> TextExtensions { get { return dc.TextExtensions; } }

        //gestione directory temporaree
        //--------------------------------------------------------------------------------------------------
        public string EasyAttachmentTempPath { get { return UtilsManager.GetEasyAttachmentTempPath(); } }

        //--------------------------------------------------------------------------------------------------
        public string OldEasyAttachmentTempPath { get { return UtilsManager.GetOldEasyAttachmentTempPath(); } }

        //--------------------------------------------------------------------------------------------------
        public string SosConnectorTempPath { get { return UtilsManager.GetSosConnectorTempPath(); } }
		public string SosConnectorLogFilePath { get { return UtilsManager.GetSosConnectorLogFilePath(); } }

		//--------------------------------------------------------------------------------------------------
		public string DocToArchiveTempPath { get { return UtilsManager.GetDocToArchiveTempPath(); } }


        // contatore globale a livello di documento di ERP, per i pending papery (sono sempre negativi)
        private int paperyDocsCounter = 0;

		///<summary>
		/// Constructors
		///</summary>
		//--------------------------------------------------------------------------------------------------
		public DMSOrchestrator()
		{
			DMSDiagnostic = new Diagnostic("DMSDiagnostic");
		}

		//--------------------------------------------------------------------------------------------------
		public void InitializeManager(string authentication, string userName)
		{
            //deve essere il primo
            UtilsManager = new Utils(CUtility.GetCompany());

            SearchManager = new SearchManager(this);
			SearchManager.ManagerErrorOccurred += new EventHandler<MessageEventArgs>(ShowMessage);

			SosManager = new SOSManager(this);
			SosManager.ManagerErrorOccurred += new EventHandler<MessageEventArgs>(ShowMessage);

			CategoryManager = new CategoryManager(this);
			CategoryManager.ManagerErrorOccurred += new EventHandler<MessageEventArgs>(ShowMessage);

			BarcodeManager = new BarcodeManager(this);
			BarcodeManager.ManagerErrorOccurred += new EventHandler<MessageEventArgs>(ShowMessage);

			ArchiveManager = new ArchiveManager(this);
			ArchiveManager.ManagerErrorOccurred += new EventHandler<MessageEventArgs>(ShowMessage);
			ArchiveManager.ArchiveCompleted += ArchiveManager_ArchiveCompleted;
            ArchiveManager.UpdateCollectionCompleted += ArchiveManager_UpdateCollectionCompleted;
            ArchiveManager.AttachmentDeleted += ArchiveManager_AttachmentDeleted;

            SettingsManager = new SettingsManager(this);
			SettingsManager.ManagerErrorOccurred += new EventHandler<MessageEventArgs>(ShowMessage);

			OCRManager = new OCRManager(this);
			OCRManager.ManagerErrorOccurred += new EventHandler<MessageEventArgs>(ShowMessage);

			FullTextFilterManager = new FullTextFilterManager(this);
			FullTextFilterManager.ManagerErrorOccurred += new EventHandler<MessageEventArgs>(ShowMessage);

			//chiedo all'EasyAttachmentSync se il database di EA è abilitato all'uso del Full Text Search
			EASync = new EasyAttachmentSync(BasePathFinder.BasePathFinderInstance.EasyAttachmentSyncUrl);
			try
			{
				// metto il try perche' se l'applicationpool e' stoppato il clientdoc non riesce a venire su + crash report
				FullTextSearchEnabled = EASync.IsFTSEnabled(CompanyID);
			}
			catch
			{
				FullTextSearchEnabled = false;
			}

			LockManager = new EALockManager(DataContext.Connection.Database, authentication, userName);

			//NotificationManager.ManagerErrorOccurred += new EventHandler<MessageEventArgs>(ShowMessage);
			NotificationManager = new NotificationManager(this.WorkerId, this.CompanyID, false);
			//mi registro alle notifiche real time di XSocket
			NotificationManager.SubscribeToTheNotificationServiceAsync();
		}

       

        // Viene passato il DMSOrchestrator di TbRepositoryManager in cui vi è istanziato il SettingsManager ed il 
        //LockManager comuni a tutt l'applicazione EasyAttachment. E anche il datacontext, questo per evitare da aprire 
        //una connessione per DMSOrchestrator ovvero per ogni documento aperto
        //--------------------------------------------------------------------------------------------------
        virtual public void InitializeManager(DMSOrchestrator repDMSOrchestrator)
		{
			CompanyCustomPath = repDMSOrchestrator.CompanyCustomPath;
			DMSConnectionString = repDMSOrchestrator.DMSConnectionString;
			WorkerId = repDMSOrchestrator.WorkerId;
			WorkersTable = repDMSOrchestrator.WorkersTable;

            UtilsManager = repDMSOrchestrator.UtilsManager;
            LockManager = repDMSOrchestrator.LockManager;
			EASync = repDMSOrchestrator.EASync;

			SecurityEnabled = repDMSOrchestrator.SecurityEnabled;
			MailConnectorEnabled = repDMSOrchestrator.MailConnectorEnabled;
			FullTextSearchEnabled = repDMSOrchestrator.FullTextSearchEnabled;
			SosConnectorActivated = repDMSOrchestrator.SosConnectorActivated;

			AuthenticationToken = repDMSOrchestrator.AuthenticationToken;
			ServerName = repDMSOrchestrator.ServerName;
			ServicePort = repDMSOrchestrator.ServicePort;
			SID = repDMSOrchestrator.SID;
			CompanyID = repDMSOrchestrator.CompanyID;
            IsAdmin = repDMSOrchestrator.IsAdmin;
			LoginId = repDMSOrchestrator.LoginId;

			//by Andrea
			NotificationManager = repDMSOrchestrator.NotificationManager;

			SearchManager = new SearchManager(this);
			SearchManager.ManagerErrorOccurred += new EventHandler<MessageEventArgs>(ShowMessage);
           
			SosManager = new SOSManager(this);
			SosManager.ManagerErrorOccurred += new EventHandler<MessageEventArgs>(ShowMessage);

			// da fare DOPO la new del SOSManager, che a sua volta istanzia il CoreSOSManager
			SOSConfigurationState = repDMSOrchestrator.SOSConfigurationState;

			CategoryManager = new CategoryManager(this);
			CategoryManager.ManagerErrorOccurred += new EventHandler<MessageEventArgs>(ShowMessage);

			BarcodeManager = new BarcodeManager(this);
			BarcodeManager.ManagerErrorOccurred += new EventHandler<MessageEventArgs>(ShowMessage);

			ArchiveManager = new ArchiveManager(this);
			ArchiveManager.ManagerErrorOccurred += new EventHandler<MessageEventArgs>(ShowMessage);
			ArchiveManager.ArchiveCompleted += ArchiveManager_ArchiveCompleted;
            ArchiveManager.UpdateCollectionCompleted += ArchiveManager_UpdateCollectionCompleted;
            ArchiveManager.AttachmentDeleted += ArchiveManager_AttachmentDeleted;

            SettingsManager = repDMSOrchestrator.SettingsManager;
			SettingsManager.ManagerErrorOccurred += new EventHandler<MessageEventArgs>(ShowMessage);

			OCRManager = new OCRManager(this);
			OCRManager.ManagerErrorOccurred += new EventHandler<MessageEventArgs>(ShowMessage);

			FullTextFilterManager = new FullTextFilterManager(this);
			FullTextFilterManager.ManagerErrorOccurred += new EventHandler<MessageEventArgs>(ShowMessage);
		}

        //Chiamato solo dal RepositoryManager
        //--------------------------------------------------------------------------------------------------
        public void DestroyManager()
		{
			//by Andrea
			if (NotificationManager != null)
				NotificationManager.UnSubScribe();
		}

		// evento intercettato dal codice C++ 
		//--------------------------------------------------------------------------------------------------
		private void ArchiveManager_ArchiveCompleted(object sender, AttachmentInfoEventArgs e)
		{
			if (ArchiveCompleted != null)
				ArchiveCompleted(sender, e);
		}

        //--------------------------------------------------------------------------------------------------
        private void ArchiveManager_UpdateCollectionCompleted(object sender, CollectionEventArgs e)
        {
            if (UpdateCollectionCompleted != null)
                UpdateCollectionCompleted(sender, e);
        }

        //--------------------------------------------------------------------------------------------------
        private void ArchiveManager_AttachmentDeleted(object sender, AttachmentEventArgs e)
        {
            if (AttachmentDeleted != null)
                AttachmentDeleted(sender, e);
        }

        //--------------------------------------------------------------------------------------------------
        public void FireMassiveRowProcessed(MassiveEventArgs args)
		{
			CUtility.FireDMS_MassiveRowProcessed
				(
				args.aiod.Attachment.ArchivedDocId,
				(int)args.aiod.ActionToDo,
				(int)args.aiod.Result,
				SearchManager.GetAttachmentsForArchivedDocId(args.aiod.Attachment.ArchivedDocId)
				);
		}


		///// <summary>
		/// dato un attachmentID restituisce i suoi Bookmark: collectionFields, FreeTags 
		/// </summary>
		//---------------------------------------------------------------------
		virtual public void GetAttachmentInfoBookmarks(ref AttachmentInfo attachmentInfo)
		{
			ArchiveManager.GetBookmarksValues(RepositoryCollection.CollectionID, ref attachmentInfo);

		}

		//---------------------------------------------------------------------
		public bool UpdateArchivedDoc(ref AttachmentInfo attachmentInfo, string description, string freeTags, string barcode)
		{
			if (!ArchiveManager.UpdateArchivedDoc(ref attachmentInfo, description, freeTags))
				return false;

			//Gestione barcode
			// richiamo l'aggiornamento del nuovo valore del barcode                    
			if (UpdateBarcode(attachmentInfo, barcode))
				attachmentInfo.TBarcode.Value = barcode;

			return true;
		}


		//--------------------------------------------------------------------------------------------------
		public void FireMassiveProcessTerminated()
		{
			CUtility.FireDMS_MassiveProcessTerminated();
		}


		///<summary>
		/// Metodo per assegnazione di un id univoco per un pending papery
		/// (e' negativo e viene assegnato alla variabile ArchiveDocId)
		///</summary>
		//---------------------------------------------------------------------
		public int GetPaperyDocIdGlobal()
		{
			return --paperyDocsCounter;
		}

		//--------------------------------------------------------------------------------------------------
		public void OpenMassiveAttachForm(IntPtr menuHandle)
		{
			using (SafeThreadCallContext context = new SafeThreadCallContext())
			{
				try
				{
					if (!BarcodeEnabled)
					{
						MessageEventArgs e = new MessageEventArgs();
						e.MessageType = DiagnosticType.Warning;
						e.Explain = Strings.NoOpenAttachPapery;
						ShowMessage(this, e);
						return;
					}

					if (openMassiveAttach && massiveAttachForm != null)
						massiveAttachForm.Activate();
					else
					{
						massiveAttachForm = new MassiveAttach(this);
						openMassiveAttach = true;
						CUtility.AddWindowRef(massiveAttachForm.Handle, false);
						massiveAttachForm.FormClosed += new FormClosedEventHandler(massiveAttachForm_FormClosed);
						massiveAttachForm.Show(menuHandle);
					}
				}
				catch (Exception exc)
				{
					Debug.WriteLine(exc.ToString());
					openMassiveAttach = false;
					CUtility.RemoveWindowRef(massiveAttachForm.Handle, false);
				}
			}
		}

		//--------------------------------------------------------------------------------------------------
		void massiveAttachForm_FormClosed(object sender, FormClosedEventArgs e)
		{
			using (SafeThreadCallContext context = new SafeThreadCallContext())
			{
				openMassiveAttach = false;
				try
				{
					CUtility.RemoveWindowRef(massiveAttachForm.Handle, false);
				}
				catch (ObjectDisposedException)
				{
				}
			}
		}

		//--------------------------------------------------------------------------------------------------
		public void OpenQuickAttachForm(IntPtr menuHandle)
		{
			using (SafeThreadCallContext context = new SafeThreadCallContext())
			{
				try
				{
					if (openQuickAttach && quickAttachForm != null)
						quickAttachForm.Activate();
					else
					{
						quickAttachForm = new QuickAttachment(this);
						openQuickAttach = true;
						CUtility.AddWindowRef(quickAttachForm.Handle, false);
						quickAttachForm.FormClosed += new FormClosedEventHandler(quickAttachForm_FormClosed);
						quickAttachForm.Show(menuHandle);
					}
				}
				catch (Exception exc)
				{
					Debug.WriteLine(exc.ToString());
					openQuickAttach = false;
					CUtility.RemoveWindowRef(massiveAttachForm.Handle, false);
				}
			}
		}

		//--------------------------------------------------------------------------------------------------
		void quickAttachForm_FormClosed(object sender, FormClosedEventArgs e)
		{
			using (SafeThreadCallContext context = new SafeThreadCallContext())
			{
				openQuickAttach = false;
				try
				{
					CUtility.RemoveWindowRef(quickAttachForm.Handle, false);
				}
				catch (ObjectDisposedException)
				{
				}
			}
		}
		///<summary>
		/// Apre e attiva la form del traylet di Brain Business
		///</summary>
		//--------------------------------------------------------------------------------------------------
		public void OpenBBTrayletForm(IntPtr menuHandle)
		{
			using (SafeThreadCallContext context = new SafeThreadCallContext())
			{
				try
				{
					if (openBBTraylet && bbTrayletForm != null)
						bbTrayletForm.Activate();
					else
					{
						bbTrayletForm = new UI.Forms.BBTrayletForm(this);
						openBBTraylet = true;
						CUtility.AddWindowRef(bbTrayletForm.Handle, false);
						bbTrayletForm.FormClosed += new FormClosedEventHandler(BBTrayletForm_FormClosed);
						bbTrayletForm.Show(menuHandle);
					}
				}
				catch (Exception exc)
				{
					Debug.WriteLine(exc.ToString());
					openBBTraylet = false;
					CUtility.RemoveWindowRef(bbTrayletForm.Handle, false);
				}
			}
		}

		//--------------------------------------------------------------------------------------------------
		void BBTrayletForm_FormClosed(object sender, FormClosedEventArgs e)
		{
			using (SafeThreadCallContext context = new SafeThreadCallContext())
			{
				openBBTraylet = false;
				try
				{
					CUtility.RemoveWindowRef(bbTrayletForm.Handle, false);
				}
				catch (ObjectDisposedException)
				{ }
			}
		}

		///<summary>
		/// Apre e attiva la form delle impostazioni del BBConnector
		///</summary>
		//--------------------------------------------------------------------------------------------------
		public void OpenBBSettings(IntPtr menuHandle)
		{
			using (SafeThreadCallContext context = new SafeThreadCallContext())
			{
				try
				{
					if (openBBSettings && bbSettingsForm != null)
						bbSettingsForm.Activate();
					else
					{
						bbSettingsForm = new UI.Forms.BBSettingsForm(this);
						openBBSettings = true;
						CUtility.AddWindowRef(bbSettingsForm.Handle, false);
						bbSettingsForm.FormClosed += new FormClosedEventHandler(BBSettings_FormClosed);
						bbSettingsForm.Show(menuHandle);
					}
				}
				catch (Exception exc)
				{
					Debug.WriteLine(exc.ToString());
					openBBSettings = false;
					CUtility.RemoveWindowRef(bbSettingsForm.Handle, false);
				}
			}
		}

		//--------------------------------------------------------------------------------------------------
		void BBSettings_FormClosed(object sender, FormClosedEventArgs e)
		{
			using (SafeThreadCallContext context = new SafeThreadCallContext())
			{
				openBBSettings = false;
				try
				{
					CUtility.RemoveWindowRef(bbSettingsForm.Handle, false);
				}
				catch (ObjectDisposedException)
				{ }
			}
		}

		//---------------------------------------------------------------------
		public bool UpdateBarcode(AttachmentInfo attachmentInfo, string newBarcodeValue)
		{
			if (string.IsNullOrEmpty(attachmentInfo.TBarcode.Value) && string.IsNullOrEmpty(newBarcodeValue))
				return true;

			// se e' cambiato il valore del barcode effettuo i controlli
			if (string.Compare(attachmentInfo.TBarcode.Value, newBarcodeValue, StringComparison.InvariantCultureIgnoreCase) != 0)
				//  richiamo l'aggiornamento del nuovo valore del barcode
				// e poi il check dei papery esistenti e l'eventuale attach multiplo
				return BarcodeManager.UpdateBarcodeValue(attachmentInfo, newBarcodeValue) && BarcodeManager.CheckAndAttachMultiplePapery(attachmentInfo);

			return true;
		}

		//chiamato dal mondo unmanaged
		//--------------------------------------------------------------------------------------------------
		public TypedBarcode DetectBarcode(AttachmentInfo ai)
		{
			return ArchiveManager.DetectBarcode(ai);
		}

        //chiamato dal mondo unmanaged
        //--------------------------------------------------------------------------------------------------
        public ArchiveResult ArchiveFile(string fileName, string description, bool isWoormReport, bool skipBarcodeDetect, string barcode)
        {
            int id = -1;
            return ArchiveManager.ArchiveFile(fileName, description, out id, isWoormReport, skipBarcodeDetect, barcode);
        }

        //--------------------------------------------------------------------------------------------------
        public ArchiveResult ArchiveBinaryContent(IntPtr binaryPtr, int lenght, string sourceFileName, string description, bool isWoormReport, bool skipBarcodeDetect, string barcode, out int archivedDocId, out string result)
        {
            archivedDocId = -1;
            result = string.Empty;
            byte[] binaryContent = null;
            try
            {
                if (binaryPtr != null && lenght > 0)
                {
                    binaryContent = new byte[lenght];
                    Marshal.Copy(binaryPtr, binaryContent, 0, lenght);
                }
            }
            catch (OutOfMemoryException)
            {
                result = string.Format(Strings.ErrorArchivingTooLargeFile, sourceFileName, "1.2");
                binaryContent = null;
            }

            return (binaryContent != null) ? ArchiveManager.ArchiveBinaryContent(binaryContent, sourceFileName, description, out archivedDocId, isWoormReport, skipBarcodeDetect, barcode) : ArchiveResult.TerminatedWithError;
        }

        //chiamato dal mondo unmanaged
        //--------------------------------------------------------------------------------------------------
        public List<AttachmentInfo> SearchAttachmentsForDocument(string docNamespace, string docPrimaryKey, string searchText, SearchLocation location, string searchFields)
		{
			SearchManager newSearchMng = new BusinessLogic.SearchManager(this);
			SearchFieldsDataTable selectedSearchFields = SearchManager.CreateSearchFieldsDataTable(searchFields);
			return newSearchMng.SearchAttachmentsForDocument(docNamespace, docPrimaryKey, searchText, location, selectedSearchFields);
		}

		//--------------------------------------------------------------------------------------------------
		public List<AttachmentInfo> SearchAttachments(string collectorName, string collectionName, string extType, DateTime startDate, DateTime endDate, string searchText, SearchLocation location, string searchFields)
		{
			SearchManager newSearchMng = new BusinessLogic.SearchManager(this);
			SearchFieldsDataTable selectedSearchFields = SearchManager.CreateSearchFieldsDataTable(searchFields);
			return newSearchMng.SearchAttachments(collectorName, collectionName, extType, startDate, endDate, searchText, location, selectedSearchFields);
		}

		//--------------------------------------------------------------------------------------------------
		public string GetDocumentKeyDescription(int nAttachmentID)
		{
			return SearchManager.GetDocumentKeyDescription(nAttachmentID);
		}        
   
        //chiamato dal mondo unmanaged
        //--------------------------------------------------------------------------------------------------
        public int GetAttachmentIDByFileName(string documentNamespace, string documentKey, string fileName)
        {
            return SearchManager.GetAttachmentIDByFileName(documentNamespace, documentKey, fileName);
        }

        //chiamato dal mondo unmanaged
        //--------------------------------------------------------------------------------------------------
        public List<AttachmentInfo> GetAttachments(string docNamespace, string docPrimaryKey, AttachmentFilterType filterType)
		{
			return SearchManager.GetAttachments(docNamespace, docPrimaryKey, filterType);
		}
		
		//restituisce gli allegati del documento che possono essere inviati come allegati di una email
		//---------------------------------------------------------------------
		public List<AttachmentInfo> GetAttachmentsByGuid(string docNamespace, string docGuid, AttachmentFilterType filterType)
		{
			return SearchManager.GetAttachmentsByGuid(docNamespace, docGuid, filterType);
		}
		//chiamato dal mondo unmanaged
		//--------------------------------------------------------------------------------------------------
		public int GetAttachmentsCount(string docNamespace, string docPrimaryKey, AttachmentFilterType filterType)
		{
			return SearchManager.GetAttachmentsCount(docNamespace, docPrimaryKey, filterType);
		}

		// chiamato dal mondo unmanaged
		//--------------------------------------------------------------------------------------------------
		public bool DeleteArchiveDocInCascade(AttachmentInfo attachmentInfo)
		{
			return ArchiveManager.DeleteArchiveDocInCascade(attachmentInfo);
		}

		//chiamato dal mondo unmanaged
		//--------------------------------------------------------------------------------------------------
		public bool DeleteAttachment(int attachmentID, int archivedDocId)
		{
			return ArchiveManager.DeleteAttachment(attachmentID, archivedDocId);
		}
	
		// Remove all erp document info: attachments, papery and record in DMS_ErpDocuments     
		//--------------------------------------------------------------------------------------------------
		public bool DeleteAllErpDocumentInfo(string tbDocNamespace, string tbPrimaryKey, ref string strError)
		{
			try
			{
				ArchiveManager.DeleteErpDocument(SearchManager.GetErpDocumentID(tbDocNamespace, tbPrimaryKey));
			}
			catch (Exception ex)
			{
				strError = Strings.ErrorDeletingAttachment + ex.Message;
				return false;
			}
			return true;
		}

		// Remove only attachments 
		//--------------------------------------------------------------------------------------------------
		public bool DeleteAttachments(string tbDocNamespace, string tbPrimaryKey, ref string strError)
		{
			try
			{
				ArchiveManager.DeleteAttachments(SearchManager.GetErpDocumentID(tbDocNamespace, tbPrimaryKey));
			}
			catch (Exception ex)
			{
				strError = Strings.ErrorDeletingAttachment + ex.Message;
				return false;
			}
			return true;
		}

		//chiamato dal mondo unmanaged
		//--------------------------------------------------------------------------------------------------
		public CollectorsResultDataTable GetAllCollectors()
		{
			return SearchManager.GetAllCollectors();
		}

        //restituisce le sole collection utilizzate
        //--------------------------------------------------------------------------------------------------
        public CollectionResultDataTable GetUsedCollections()
        {
            return SearchManager.GetUsedCollections();
        }

        //chiamato dal mondo unmanaged
        //--------------------------------------------------------------------------------------------------
        public List<String> GetAllExtensions()
		{
			return SearchManager.GetAllExtensions();
		}

		//--------------------------------------------------------------------------------------------------
		public ArchivedDocDataTable GetArchivedDocuments(FilterEventArgs fea)
		{
			// istanzio un search manager locale per evitare il cache in memoria dei dati estratti di LINQ
			SearchManager searchManager = new SearchManager(this);
			searchManager.ManagerErrorOccurred += new EventHandler<MessageEventArgs>(ShowMessage);

			return searchManager.GetArchivedDocuments(fea);
		}

		//--------------------------------------------------------------------------------------------------
		public ArchivedDocDataTable GetArchivedDocuments()
		{
			// istanzio un search manager locale per evitare il cache in memoria dei dati estratti di LINQ
			SearchManager searchManager = new SearchManager(this);
			searchManager.ManagerErrorOccurred += new EventHandler<MessageEventArgs>(ShowMessage);

			return searchManager.GetArchivedDocuments();
		}

		//--------------------------------------------------------------------------------------------------
		public AttachmentInfo GetAttachmentInfoFromAttachmentId(int attachmentId)
		{
			return SearchManager.GetAttachmentInfoFromAttachmentId(attachmentId);
		}

		//--------------------------------------------------------------------------------------------------
		public AttachmentInfo GetAttachmentInfoFromArchivedDocId(int archivedDocId)
		{
			return SearchManager.GetAttachmentInfoFromArchivedDocId(archivedDocId);
		}

		//---------------------------------------------------------------------
		public AttachmentInfo GetCompletedAttachmentInfoFromAttachmentId(int attachmentId)
		{
			AttachmentInfo attInfo = SearchManager.GetAttachmentInfoFromAttachmentId(attachmentId);
			if (attInfo != null)
				ArchiveManager.GetBookmarksValues(attInfo.CollectionID, ref attInfo);
			return attInfo;
		}


		//---------------------------------------------------------------------
		public void ReloadBookmarksValues(ref AttachmentInfo attInfo)
		{
			ArchiveManager.GetBookmarksValues(attInfo.CollectionID, ref attInfo);			
		}

		//---------------------------------------------------------------------
		public AttachmentInfo GetCompletedAttachmentInfoFromArchivedDocId(int archivedDocId)
		{
			AttachmentInfo attInfo = SearchManager.GetAttachmentInfoFromArchivedDocId(archivedDocId);
			if (attInfo != null)
				ArchiveManager.GetBookmarksValues(attInfo.CollectionID, ref attInfo);
			return attInfo;
		}

		//--------------------------------------------------------------------------------------------------
		public SearchResultDataTable GetERPDocumentAttachment(int archivedDocId)
		{
			return SearchManager.GetERPDocumentAttachment(archivedDocId);
		}

        //chiamato dal mondo unmanaged
        //--------------------------------------------------------------------------------------------------
        public byte[] GetBinaryContent(int attachmentID, ref bool veryLargeFile, ref string fileName)
        {
            byte[] content = null;
            veryLargeFile = false;
            fileName = string.Empty;

            var doc = from att in dc.DMS_Attachments
                      where att.AttachmentID == attachmentID
                      select att.DMS_ArchivedDocument;

            if (doc != null && doc.Any())
            {
                AttachmentInfo info = new AttachmentInfo((DMS_ArchivedDocument)doc.Single(), this);
                content = GetBinaryContent(info, ref veryLargeFile);
                fileName = info.Name;
            }

            return content;
        }

          //--------------------------------------------------------------------------------------------------
        public byte[] GetBinaryContent(AttachmentInfo attachInfo, ref bool veryLargeFile)
		{
			return SearchManager.GetBinaryContent(attachInfo, ref veryLargeFile);
		}

		//--------------------------------------------------------------------------------------------------
		public string GetStorageFileName(AttachmentInfo attachInfo)
		{
			return SearchManager.GetStorageFileName(attachInfo);
		}

		//--------------------------------------------------------------------------------------------------
		public string GetArchivedDocTempFile(int archivedDocID)
		{
			var doc = from archDoc in dc.DMS_ArchivedDocuments
					  where archDoc.ArchivedDocID == archivedDocID
					  select archDoc;

			if (doc != null && doc.Any())
			{
				AttachmentInfo info = new AttachmentInfo((DMS_ArchivedDocument)doc.Single(), this);
                UtilsManager.SaveAttachmentFile(info);
				return info.TempPath;
			}
			return string.Empty;
		}

		//--------------------------------------------------------------------------------------------------
		public string GetAttachmentTempFile(int attachmentID)
		{
			var doc = from att in dc.DMS_Attachments
					  where att.AttachmentID == attachmentID
					  select att.DMS_ArchivedDocument;

			if (doc != null && doc.Any())
			{
				AttachmentInfo info = new AttachmentInfo((DMS_ArchivedDocument)doc.Single(), this);

				try
				{
                    UtilsManager.SaveAttachmentFile(info);
				}
				catch (Exception)
				{
					return string.Empty;
				}

				return info.TempPath;
			}
			return string.Empty;
		}

		//--------------------------------------------------------------------------------------------------
		public void SaveAttachmentInTempFile(int attachmentID, string tempFileName)
		{
			var doc = from att in dc.DMS_Attachments
					  where att.AttachmentID == attachmentID
					  select att.DMS_ArchivedDocument;

			if (doc != null && doc.Any())
			{
				AttachmentInfo info = new AttachmentInfo((DMS_ArchivedDocument)doc.Single(), this);
                UtilsManager.SaveAttachmentFile(info, tempFileName);
			}
		}

		//--------------------------------------------------------------------------------------------------
		public string SaveAttachmentFileInFolder(int attachmentID, string folderName)
		{
			var doc = from att in dc.DMS_Attachments
					  where att.AttachmentID == attachmentID
					  select att.DMS_ArchivedDocument;

			if (doc != null && doc.Any())
			{
				AttachmentInfo info = new AttachmentInfo((DMS_ArchivedDocument)doc.Single(), this);
				return UtilsManager.SaveAttachmentFileInFolder(info, folderName);
			}

			return string.Empty;
		}

        //--------------------------------------------------------------------------------------------------
        public bool SaveAttachmentFile(AttachmentInfo attInfo, string tempFileName = null)
        {
            return UtilsManager.SaveAttachmentFile(attInfo, tempFileName);
        }

        //--------------------------------------------------------------------------------------------------
        public string TransformToPdfA(AttachmentInfo attInfo, string tempFileName = "", bool isSOSDocument = false)
        {
            return UtilsManager.TransformToPdfA(attInfo, tempFileName, isSOSDocument);
        }

        //--------------------------------------------------------------------------------------------------
        public string GetArchiveDocTempFileName(string fileName)
        {
            return UtilsManager.GetArchiveDocTempFileName(fileName);
        }

        //--------------------------------------------------------------------------------------------------
        public string GetArchiveDocTempFileName(AttachmentInfo attInfo)
        {
            return UtilsManager.GetArchiveDocTempFileName(attInfo);
        }

        //--------------------------------------------------------------------------------------------------
        public void OpenDocument(AttachmentInfo attInfo, string tempFileName = null)
        {
            if (attInfo != null && UtilsManager.SaveAttachmentFile(attInfo, tempFileName))
                Process.Start(attInfo.TempPath);
        }

        //chiamato dal mondo unmanaged per il salvataggio di piu' documenti archiviati in un folder su filesystem
        //--------------------------------------------------------------------------------------------------
        public void SaveMultipleArchiveDocFileInFolder(List<int> archiveDocIds)
		{
			FolderBrowserDialog dialog = new FolderBrowserDialog();

			dialog.SelectedPath = 
				string.IsNullOrWhiteSpace(Properties.Settings.Default.CopyInFolderPath) 
				? Environment.GetFolderPath(Environment.SpecialFolder.MyComputer) 
				: Properties.Settings.Default.CopyInFolderPath;
            dialog.ShowNewFolderButton = true;

			try
			{
				if (dialog.ShowDialog() == DialogResult.OK)
				{
					AttachmentInfo ai = null;

					foreach (int id in archiveDocIds)
					{
						var doc = from archDoc in dc.DMS_ArchivedDocuments where archDoc.ArchivedDocID == id select archDoc;

						if (doc != null && doc.Any())
						{
							ai = new AttachmentInfo((DMS_ArchivedDocument)doc.Single(), this);
							if (ai == null || (ai.DocContent == null && !ai.VeryLargeFile))
								continue;

							if (ai.VeryLargeFile)
							{
								File.Copy(ai.TempPath, Path.Combine(dialog.SelectedPath, ai.Name));
								continue;
							}

							using (Stream myStream = File.OpenWrite(Path.Combine(dialog.SelectedPath, ai.Name)))
							using (StreamWriter s = new StreamWriter(myStream))
								myStream.Write(ai.DocContent, 0, ai.DocContent.Length);
						}

						Cursor.Current = Cursors.Default;
						ai.DisposeDocContent();
					}

					Properties.Settings.Default.CopyInFolderPath = dialog.SelectedPath;
					Properties.Settings.Default.Save();
                }
			}
			catch { }
		}

		//chiamato dal mondo unmanaged per il salvataggio di un documento archiviato su file system
		//--------------------------------------------------------------------------------------------------
		public void SaveArchiveDocFileInFolder(int archiveDocID)
		{
			var doc = from archDoc in dc.DMS_ArchivedDocuments
					  where archDoc.ArchivedDocID == archiveDocID
					  select archDoc;

			if (doc != null && doc.Any())
			{
				AttachmentInfo ai = new AttachmentInfo((DMS_ArchivedDocument)doc.Single(), this);

				SaveFileDialog saveFileDialog = new SaveFileDialog();
				saveFileDialog.DefaultExt = ai.ExtensionType;
				saveFileDialog.DefaultExt.Replace(".", "");
				saveFileDialog.Filter = String.Format("{0} (*.*)|*.*", Strings.AllFiles);
				saveFileDialog.AddExtension = true;
				saveFileDialog.RestoreDirectory = true;
				saveFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyComputer, Environment.SpecialFolderOption.Create);
				saveFileDialog.FileName = ai.Name;

				if (saveFileDialog.ShowDialog() == DialogResult.OK)
				{
					Cursor.Current = Cursors.WaitCursor;

					if (ai.VeryLargeFile)
					{
						File.Copy(ai.TempPath, saveFileDialog.FileName);
						return;
					}

					using (Stream myStream = saveFileDialog.OpenFile())
					using (StreamWriter s = new StreamWriter(myStream))
						myStream.Write(ai.DocContent, 0, ai.DocContent.Length);

					Cursor.Current = Cursors.Default;
				}

				ai.DisposeDocContent();
			}
		}

		///<summary> 
		/// Funzione che ritorna se un valore di barcode e' valido per EA (dai settings)
		///</summary>
		//--------------------------------------------------------------------------------
		public bool IsValidEABarcodeValue(string value)
		{
			return
				value != null &&
				value.StartsWith(SettingsManager.UsersSettingState.Options.BarcodeDetectionOptionsState.BarcodePrefix, StringComparison.InvariantCulture) &&
				value.Length < 18;
		}

		//---------------------------------------------------------------------
		public MassiveAttachInfo MassiveAttachUnattendedMode(string folder, bool splitFile)
		{
			//MassiveAttachInfo massive = new MassiveAttachInfo();
			//massive.AddAttachmentInfo(new AttachmentInfoOtherData());
			//string s = massive.Serialize();
			//return massive;
			return BarcodeManager.MassiveAttachUnattendedMode(folder, splitFile);
		}

		/// <summary>
		/// restituisce tutte i raggruppamenti presenti nella tabella MDM_Collectors. Sono i raggruppamenti dei documenti. 
		/// Esempio Documenti di vendita
		/// </summary>
		//---------------------------------------------------------------------
		public List<string> GetCollectors()
		{
			List<string> collList = new List<string>();

			var collectors = from coll in dc.DMS_Collectors select coll.Name;
			collList = collectors.ToList();

			return collList;
		}

		/// <summary>
		/// Data una collector restituisce le proprie collezioni. Sono i documenti presenti in un dato raggruppamento
		/// </summary>
		//---------------------------------------------------------------------
		public List<string> GetCollections(string collector)
		{
			List<string> collList = new List<string>();

			var collections = from coll in dc.DMS_Collections
							  where coll.DMS_Collector.Name == collector
							  select coll.Name;
			collList = collections.ToList();

			return collList;
		}

		//Categories management        
		//---------------------------------------------------------------------
		public List<string> GetCategoriesName()
		{
			return CategoryManager.GetCategoriesName();
		}

		//---------------------------------------------------------------------
		public DTCategories GetCategories()
		{
			return CategoryManager.GetCategories();
		}

		//---------------------------------------------------------------------
		public bool SaveCategoryInfo(CategoryInfo categoryInfo)
		{
			return CategoryManager.SaveCategory(categoryInfo);
		}

		//---------------------------------------------------------------------
		public bool DeleteCategory(string categoryName)
		{
			return CategoryManager.DeleteCategory(categoryName);
		}

		//---------------------------------------------------------------------
		public CategoryInfo GetCategory(string categoryName)
		{
			CategoryInfo catInfo = new CategoryInfo();
			DMS_Field field = CategoryManager.GetCategory(categoryName);
			if (field == null) return null;

			catInfo.CategoriesValuesDataTable = new DTCategoriesValues();
			catInfo.Name = field.FieldName;
			catInfo.Description = field.FieldDescription;
			catInfo.TheColor = Color.FromArgb(field.DMS_FieldProperty != null ? (int)field.DMS_FieldProperty.FieldColor : 0);
			catInfo.Disabled = (field.DMS_FieldProperty != null) ? (bool)field.DMS_FieldProperty.Disabled : false;
			catInfo.InUse = CategoryManager.IsCategoryUsed(catInfo.Name);
			string defValue = string.Empty;

			if (field.DMS_FieldProperty != null && field.DMS_FieldProperty.XMLValues != null)
			{
				List<FieldValueState> catValues = FieldPropertiesState.Deserialize(field.DMS_FieldProperty.XMLValues.ToString());
				foreach (FieldValueState vField in catValues)
				{
					DataRow fRow = catInfo.CategoriesValuesDataTable.NewRow();
					fRow[CommonStrings.InUse] = CategoryManager.IsASearchField(field.FieldName, vField.FieldValue);
					fRow[CommonStrings.Value] = vField.FieldValue;
					fRow[CommonStrings.IsDefault] = vField.IsDefault;
					if (vField.IsDefault)
						defValue = vField.FieldValue;
					catInfo.CategoriesValuesDataTable.Rows.Add(fRow);
				}
			}

			return catInfo;
		}

		//---------------------------------------------------------------------
		public TypedBarcode CreateRandomBarcodeValue()
		{
			return BarcodeManager.CreateRandomBarcodeValue();
		}

		//torna il tipo di default dei settings 
		//---------------------------------------------------------------------
		public bool GetDefaultBarcodeType(out string type, out string prefix)
		{
			type = SettingsManager.UsersSettingState.Options.BarcodeDetectionOptionsState.BarcodeTypeValue;
			prefix = SettingsManager.UsersSettingState.Options.BarcodeDetectionOptionsState.BarcodePrefix;
			return true;
		}

        ///cancella la cartella C:\Users\Bauzone\AppData\Local\Temp\TBTemp\usr-bauzoneann1\DEVELOPMENT_ERP\TbAppManager\DMSTemp 
        /// dove ci file temporanei generati dal DMS
        //---------------------------------------------------------------------
        public void RemoveTemporaryFiles()
		{
            UtilsManager.RemoveTemporaryFiles();
        }

		//---------------------------------------------------------------------
		public TypedBarcode GetBarcodeForReport(string fileName, bool isArchiving)
		{
			return BarcodeManager.GetBarcodeForReport(fileName, isArchiving);
		}

		///<summary>
		/// Evento intercettato dai vari manager che rimpallano i vari errori riscontrati 
		/// Con le informazioni popolo il Diagnostic e lo visualizzo con il DiagnosticViewer.
		///</summary>
		//--------------------------------------------------------------------------------------------------
		virtual public void ShowMessage(object sender, MessageEventArgs e)
		{
			if (sender is Diagnostic)
				DMSDiagnostic.Set((Diagnostic)sender);
			else
			{
				// se non ho un messaggio non procedo
				if (string.IsNullOrWhiteSpace(e.Explain))
					return;

				ExtendedInfo ei = null;
				// se il message contiene dei valori assegno le variabili dell'eccezione, altrimenti
				// assegno solo le info base
				if (!string.IsNullOrWhiteSpace(e.Message))
				{
					ei = new ExtendedInfo();
					ei.Add(Strings.Description, e.Message);
					ei.Add(Strings.Source, e.Source);
					ei.Add(Strings.StackTrace, e.StackTrace);
					ei.Add(Strings.Function, e.Function);
					ei.Add(Strings.Library, e.Library);
				}

				DMSDiagnostic.Set(e.MessageType, e.Explain, ei);
			}

			if (!InUnattendedMode)
			{
				// imposto questa proprietà in modo da evitare l'errore di Cross-Thread execution
				// N.B. noi siamo sullo stesso thread, è a basso livello che il framework fa casino quando
				// tenta di portare in secondo piano la form di Mago, che invece è su un altro thread
				using (SafeThreadCallContext context = new SafeThreadCallContext())
					DiagnosticViewer.ShowDiagnosticAndClear(DMSDiagnostic);
			}
		}

		//---------------------------------------------------------------------
		internal void EnqueueAttachmentsToSend(List<int> attachmentIds)
		{
			EASync.EnqueueAttachmentsToSend(CompanyID, attachmentIds, this.LoginId);
		}

		//---------------------------------------------------------------------
		public bool CheckIn(AttachmentInfo ai)
		{
			return ArchiveManager.CheckIn(ai);
		}

		//---------------------------------------------------------------------
		public bool CheckOut(AttachmentInfo ai)
		{
			return ArchiveManager.CheckOut(ai, WorkerId);
		}
		//---------------------------------------------------------------------
		public bool Undo(AttachmentInfo ai)
		{
			return ArchiveManager.Undo(ai, WorkerId);
		}

		/// <summary>
		/// Torna il nome del worker partendo da id in formato stringa
		/// </summary>
		//--------------------------------------------------------------------------------------------------
		public string GetWorkerName(string val)
		{
			int i = -1;
			if (Int32.TryParse(val, out i))
				return GetWorkerName(i);
			return val;
		}

		/// <summary>
		/// Torna il nome del worker partendo da id in formato intero
		/// </summary>
		//--------------------------------------------------------------------------------------------------
		public string GetWorkerName(int? val)
		{
			if (val == null)
				return null;

			MWorker worker = WorkersTable.GetWorker((int)val);
			return (worker != null) ? worker.Name + " " + worker.LastName : val.ToString();
		}

		//--------------------------------------------------------------------------------------------------
		internal string GetCurrentWorkerName()
		{
			return GetWorkerName(WorkerId);
		}

		//--------------------------------------------------------------------------------
		public bool IsDocumentNamespaceInSos(string documentNamespace)
		{
			return (!SosConnectorEnabled || string.IsNullOrEmpty(documentNamespace)) ? false : !string.IsNullOrEmpty(SOSConfigurationState.DocumentClasses.GetInternalDocClass(documentNamespace));
		}

		///<summary>
		/// Metodo "ponte" utilizzato nella parte C++, richiamato per acquisire un documento tramite la scansione da device
		///</summary>
		//--------------------------------------------------------------------------------------------------
		public List<string> MultipleScanWithBarcode(string strFileName, string strExtension)
		{
			List<string> multipleScanList = new List<string>();

			if (string.Compare(strExtension, FileExtensions.Pdf, StringComparison.InvariantCultureIgnoreCase) == 0)
				multipleScanList = MultipleScanWithBarcodeAsSeparator.ScanPdf(strFileName, this.BarcodeManager);
			if (string.Compare(strExtension, FileExtensions.Tiff, StringComparison.InvariantCultureIgnoreCase) == 0)
				multipleScanList = MultipleScanWithBarcodeAsSeparator.ScanTif(strFileName, this.BarcodeManager);

			return multipleScanList;
		}

		///<summary>
		/// Metodo richiamato dalla parte C++, per aggiornare la colonna TBGuid della tabella DMS_ErpDocument
		/// con il valore vero del documento gestionale (vedi miglioria 6185)
		///</summary>
		//--------------------------------------------------------------------------------------------------
		public bool UpdateTBGuidInERPDocument()
		{
			string message = string.Empty;

			GuidUpdater gUpdater = null;

			try
			{
				gUpdater = new GuidUpdater(this);
				gUpdater.Execute(); // l'esecuzione va su un thread separato
				message = gUpdater.ElaborationMessage;

				/// <remarks>
				/// Valori di ritorno
				/// -1 : errore + msg trappato
				///  0 : non ci sono record da elaborare
				///  1 : ok
				/// </remarks>
				if (gUpdater.NResult >= 0)
				{
					if (string.IsNullOrWhiteSpace(message))
						message = string.Format(Strings.MsgUpdatingEmptyTBGuid, gUpdater.NrRecordsUpdated.ToString(), gUpdater.NrOrphanErpDocuments.ToString());

					return true;
				}
			}
			catch (Exception e)
			{
				message = string.Format(Strings.ErrorUpdatingEmptyTBGuid, e.Message);
			}
			finally
			{
				DMSDiagnostic.Set(DiagnosticType.LogInfo | ((gUpdater != null && gUpdater.NResult >= 0) ? DiagnosticType.Information : DiagnosticType.Error), message);
			}

			return false;
		}

		/// <summary>
		/// Chiamato dal C++ per avere elenco dei SOSDocument da inviare in SOStitutiva
		/// (utilizzato nel wizard SOSDocSender)
		/// </summary>
		//--------------------------------------------------------------------------------------------------
		public SearchResultDataTable GetSOSDocuments(SosSearchRules searchRules)
		{
			// istanzio un SOSManager locale per evitare il cache in memoria dei dati estratti di LINQ
			SOSManager sosManager = new SOSManager(this);
			sosManager.ManagerErrorOccurred += new EventHandler<MessageEventArgs>(ShowMessage);

			return sosManager.GetSOSDocuments(searchRules);
		}

		/// <summary>
		/// Chiamato dal C++ per avere elenco degli allegati da "sistemare" e per poi inviare in SOStitutiva
		/// (utilizzato nel wizard SOSAdjustAttachments)
		/// </summary>
		//--------------------------------------------------------------------------------------------------
		public SearchResultDataTable GetAttachmentsToAdjustForSOS(SosSearchRules searchRules)
		{
			// istanzio un SOSManager locale per evitare il cache in memoria dei dati estratti di LINQ
			SOSManager sosManager = new SOSManager(this);
			sosManager.ManagerErrorOccurred += new EventHandler<MessageEventArgs>(ShowMessage);

			return sosManager.SearchAttachmentsForAdjust(searchRules);
		}
	}

	///<summary>
	/// DMSDocOrchestrator: used to manager attachements. It is linked to any dataentry 
	/// </summary>
	//================================================================================
	public class DMSDocOrchestrator : DMSOrchestrator, IDisposable
	{
		private MDocument mDocument = null;
		List<BookmarkToObserve> bookmarksToObserve = new List<BookmarkToObserve>();

		//collection di attachment
		private DMS_Collector documentCollector = null;
		private DMS_Collection documentCollection = null;
		private DMS_ErpDocument erpDocument = null;
		public int ErpDocumentID = -1;

		public AttachManager AttachManager { get; set; }

		// Properties
		//--------------------------------------------------------------------------------------------------
		public string DocumentPrimaryKey
		{
			get
			{
				string primaryKey = (CurrentRecordValid && MasterRecord != null) ? MasterRecord.GetPrimaryKeyNameValue() : string.Empty;
				return (string.IsNullOrEmpty(primaryKey)) ? ((mDocument.Batch) ? DocumentNamespace : string.Empty) : primaryKey;
			}
		}

		//--------------------------------------------------------------------------------------------------
		public Guid DocumentTBGuid
		{
			get
			{
				if (CurrentRecordValid && MasterRecord != null)
				{
					IRecordField tbGuid = MasterRecord.GetField("TBGuid");
					if (tbGuid != null)
						return (Guid)tbGuid.Value;
				}
				
				return Guid.Empty;
			}
		}

		//--------------------------------------------------------------------------------
		public bool CurrentRecordValid
		{
			get
			{
				return (mDocument.ValidCurrentRecord() && (mDocument.FormMode != FormModeType.Find || mDocument.FormMode != FormModeType.New)) || mDocument.Batch;
			}
		}

		//--------------------------------------------------------------------------------
		public bool CanUseAttachmentPanel
		{
			get
			{
				return (mDocument.ValidCurrentRecord() && mDocument.FormMode != FormModeType.Find && mDocument.FormMode != FormModeType.New) || mDocument.Batch;
			}
		}

		//--------------------------------------------------------------------------------
		public MDocument Document { get { return mDocument; } }

		//--------------------------------------------------------------------------------
		override public bool InUnattendedMode
		{
			get { return (mDocument.InUnattendedMode || CUtility.IsRemoteInterface()) || inUnattendedMode; }
		}

		//--------------------------------------------------------------------------------
		public List<BookmarkToObserve> BookmarksToObserve { get { return bookmarksToObserve; } }

		//--------------------------------------------------------------------------------
		private List<MXMLSearchBookmark> bookmarks = null;

		//--------------------------------------------------------------------------------
		override public string CollectorName { get { return mDocument.Namespace.Module; } }

		//--------------------------------------------------------------------------------
		override public string CollectionName { get { return mDocument.Namespace.Document; } }

		//--------------------------------------------------------------------------------
		override public int CollectorID { get { return (DocumentCollector != null) ? DocumentCollector.CollectorID : -1; } }

		//--------------------------------------------------------------------------------
		override public int CollectionID { get { return (DocumentCollection != null) ? DocumentCollection.CollectionID : -1; } }

		//--------------------------------------------------------------------------------
		override public int ERPDocumentID { get { return ErpDocumentID; } }

		//--------------------------------------------------------------------------------------------------
		override public MSqlRecord MasterRecord { get { return (mDocument.Master != null)  ? (MSqlRecord)mDocument.Master.Record : null ; } }

		//--------------------------------------------------------------------------------
		public string DocumentNamespace { get { return (mDocument != null) ? mDocument.Namespace.ToString() : string.Empty; } }


		private int currentCollectionVersion = 1;
		//--------------------------------------------------------------------------------
		public List<MXMLSearchBookmark> Bookmarks
		{
			get
			{
				if (bookmarks == null && Document.Master != null)
					bookmarks = Document.GetXMLSearchBookmark(Document.Master.Namespace, out currentCollectionVersion);

				return bookmarks;
			}
		}

		//--------------------------------------------------------------------------------
		public int CurrentCollectionVersion
		{
			get
			{
				if (bookmarks == null && Document.Master != null)
					bookmarks = Document.GetXMLSearchBookmark(Document.Master.Namespace, out currentCollectionVersion);

				return currentCollectionVersion;
			}
		}

		//--------------------------------------------------------------------------------
		private bool documentNamespaceInSos = false;
		public bool DocumentNamespaceInSos { get { return SosConnectorEnabled && documentNamespaceInSos; } }

		//---------------------------------------------------------------------
		public DMS_Collector DocumentCollector
		{
			get
			{
				if (documentCollector != null)
					return documentCollector;
				try
				{
					var collector = from coll in dc.DMS_Collectors
									where coll.Name == CollectorName
									select coll;

					documentCollector = (collector != null && collector.Any()) ? (DMS_Collector)collector.Single() : null;
					//non è  ancora presente, lo inserisco
					if (documentCollector == null)
					{
						string lockErr = string.Empty;
						if (LockManager.LockRecord("DMS_Collector", CollectorName, LockContext, ref lockErr))
						{
							DMS_Collector newCollector = new DMS_Collector();
							newCollector.Name = CollectorName;
							newCollector.IsStandard = true;
							dc.DMS_Collectors.InsertOnSubmit(newCollector);
							dc.SubmitChanges();
							LockManager.UnlockRecord("DMS_Collector", CollectorName, LockContext);
							documentCollector = newCollector;
						}
						else
						{
							MessageEventArgs arg = new MessageEventArgs();
							arg.Explain = lockErr;
							arg.MessageType = DiagnosticType.Error;
							ShowMessage(this, arg);
							documentCollector = null;
						}
					}
				}
				catch (Exception e)
				{
					MessageEventArgs arg = new MessageEventArgs();
					arg.Explain = e.Message;
					arg.MessageType = DiagnosticType.Error;
					ShowMessage(this, arg);
					documentCollector = null;
				}

				return documentCollector;
			}
		}

		//---------------------------------------------------------------------
		public DMS_Collection DocumentCollection
		{
			get
			{
				if (documentCollection != null)
					return documentCollection;

				try
				{  
                    //string templateName = DocumentInSos ? CommonStrings.SosTemplate : CommonStrings.DefaultTemplate; 
                    documentCollection = SearchManager.GetCollection(CollectorName, CollectionName, CommonStrings.DefaultTemplate);
                  	//se non è  ancora presente, lo inserisco
					if (documentCollection == null)
					{
						string lockErr = string.Empty;
						if (LockManager.LockRecord("DMS_Collection", CollectionName, LockContext, ref lockErr))
						{
							DMS_Collection newCollection = new DMS_Collection();
							newCollection.Name = CollectionName;
							newCollection.TemplateName = CommonStrings.DefaultTemplate;
							newCollection.CollectorID = DocumentCollector.CollectorID;
							newCollection.IsStandard = true;
							newCollection.SosDocClass = (DocumentNamespaceInSos) ? SOSConfigurationState.DocumentClasses.GetInternalDocClass(DocumentNamespace) : "";
							newCollection.Version = CurrentCollectionVersion;
							dc.DMS_Collections.InsertOnSubmit(newCollection);
							dc.SubmitChanges();
							LockManager.UnlockRecord("DMS_Collection", CollectionName, LockContext);
							documentCollection = newCollection;
						}
						else
						{
							MessageEventArgs arg = new MessageEventArgs();
							arg.Explain = lockErr;
							arg.MessageType = DiagnosticType.Error;
							ShowMessage(this, arg);
							documentCollection = null;
						}
					}
				}
				catch (Exception e)
				{
					MessageEventArgs arg = new MessageEventArgs();
					arg.Explain = e.Message;
					arg.MessageType = DiagnosticType.Error;
					ShowMessage(this, arg);
					documentCollection = null;
				}

				return documentCollection;
			}
		}

		// mette a disposizione il record di ERP un colpo indietro allo stato attuale (prima del cambio del FormMode)
		//---------------------------------------------------------------------
		public DMS_ErpDocument OldErpDocument { get { return erpDocument; } }

		//---------------------------------------------------------------------
		public DMS_ErpDocument ErpDocument
		{
			get
			{
				try
				{
					if (
						erpDocument != null &&
						string.Compare(erpDocument.PrimaryKeyValue, DocumentPrimaryKey, StringComparison.InvariantCultureIgnoreCase) == 0
						)
						return erpDocument;

					var document = (from doc in dc.DMS_ErpDocuments
									where doc.DocNamespace == DocumentNamespace &&
									doc.PrimaryKeyValue == DocumentPrimaryKey
									select doc);

					DMS_ErpDocument erpDoc = null;

                    if (document != null && document.Any() && document.Count() == 1)
                        erpDocument = (DMS_ErpDocument)document.Single();
                    else if (document.Count() > 1)//segnalazione 240417+ risoluzione
                    {

                        MessageEventArgs arg = new MessageEventArgs();
                        arg.Explain = Strings.DuplicatedErpDocID;
                        arg.Message = String.Format("DocumentNamespace: {0} - DocumentPrimaryKey: {1}", DocumentNamespace, DocumentPrimaryKey);
                        arg.Function = "DmsOrchestrator.ErpDocument";
                        arg.MessageType = DiagnosticType.Warning;
                        ShowMessage(this, arg);
                        ArchiveManager.ResetDuplicatedErpDocId();

                        //rieseguo query tale e quale:
                        {
                            var document2 = (from doc in dc.DMS_ErpDocuments
                                             where doc.DocNamespace == DocumentNamespace &&
                                             doc.PrimaryKeyValue == DocumentPrimaryKey
                                             select doc);


                            if (document2 != null && document2.Any() && document2.Count() == 1)
                                erpDocument = (DMS_ErpDocument)document2.Single();
                            else if (document.Count() > 1)//segnalazione 240417+ risoluzione
                            {

                                arg = new MessageEventArgs();
                                arg.Explain = Strings.DuplicatedErpDocID2;
                                arg.Message = String.Format("DocumentNamespace: {0} - DocumentPrimaryKey: {1}", DocumentNamespace, DocumentPrimaryKey);
                                arg.Function = "DmsOrchestrator.ErpDocument";
                                arg.MessageType = DiagnosticType.Warning;
                                ShowMessage(this, arg);
                                erpDocument = (DMS_ErpDocument)document.First();
                            }
                            else
                            {
                                //non lo salvo subito..
                                //posticipo la suo creazione a quando viene effettuato il primo attachment
                                erpDoc = new DMS_ErpDocument();
                                erpDoc.ErpDocumentID = -1;
                                erpDoc.DocNamespace = DocumentNamespace;
                                erpDoc.PrimaryKeyValue = DocumentPrimaryKey;
								erpDoc.TBGuid = DocumentTBGuid;
								erpDocument = erpDoc;
                            }

                        }
                    }
                    else
                    {
                        //non lo salvo subito..
                        //posticipo la suo creazione a quando viene effettuato il primo attachment
                        erpDoc = new DMS_ErpDocument();
                        erpDoc.ErpDocumentID = -1;
                        erpDoc.DocNamespace = DocumentNamespace;
                        erpDoc.PrimaryKeyValue = DocumentPrimaryKey;
						erpDoc.TBGuid = DocumentTBGuid;
						erpDocument = erpDoc;
                    }

					return erpDocument;
				}
				catch (Exception e)
				{
					throw (e);
				}
			}

			set { erpDocument = value; }
		}

		// Events
		//--------------------------------------------------------------------------------------------------
		public event EventHandler DeleteErpDocument;
		public event EventHandler<FormModeEventArgs> FormModeChanged;

		public event EventHandler<AttachmentInfoEventArgs> AttachCompleted;
        public event EventHandler<EventArgs> SyncronizationIndexesFinished;


        //--------------------------------------------------------------------------------------------------
        public DMSDocOrchestrator(MDocument document)
			: base()
		{
			DMSDiagnostic = new Diagnostic("DMSDiagnostic");
			mDocument = document;
		}

		//--------------------------------------------------------------------------------------------------
		public void Dispose()
		{
			if (mDocument != null)
				mDocument.Dispose();
		}

		// Viene passato il DMSOrchestrator di TbRepositoryManager in cui vi è istanziato il SettingsManager ed il 
		//LockManager comuni a tutt l'applicazione EasyAttachment. E anche il datacontext, questo per evitare da aprire 
		//una connessione per DMSOrchestrator ovvero per ogni documento aperto
		//--------------------------------------------------------------------------------------------------
		override public void InitializeManager(DMSOrchestrator repDMSOrchestrator)
		{
			base.InitializeManager(repDMSOrchestrator);

			SosManager.DMSDocOrchestrator = this;
			SosManager.ManagerErrorOccurred += new EventHandler<MessageEventArgs>(ShowMessage);


			AttachManager = new AttachManager(this);
			AttachManager.ManagerErrorOccurred += new EventHandler<MessageEventArgs>(ShowMessage);
			AttachManager.AttachCompleted += AttachManager_AttachCompleted;

            SearchManager.SyncronizationIndexesFinished += SearchManager_SyncronizationIndexesFinished;

            documentNamespaceInSos = IsDocumentNamespaceInSos(DocumentNamespace);
		}
        
        //--------------------------------------------------------------------------------------------------
        void AttachManager_AttachCompleted(object sender, AttachmentInfoEventArgs e)
		{
			if (AttachCompleted != null)
				AttachCompleted(sender, e);
		}

        //the template has been changed
        //I have to re-load the fields to observe
        //--------------------------------------------------------------------------------------------------
        //void AttachManager_UpdateCollectionCompleted(object sender, EventArgs e)
        //{
        //    //if (Document.FormMode != FormModeType.Edit)
        //    //    return;

        //    if (UpdateCollectionCompleted != null)
        //        UpdateCollectionCompleted(sender, e);
        //}

        //--------------------------------------------------------------------------------------------------
        private void SearchManager_SyncronizationIndexesFinished(object sender, EventArgs e)
        {
            if (SyncronizationIndexesFinished != null)
                SyncronizationIndexesFinished(sender, e);
        }

        //--------------------------------------------------------------------------------------------------
        public void AfterSetFormMode(FormModeType oldFormMode)
		{
			if (FormModeChanged != null)
			{
				FormModeEventArgs arg = new FormModeEventArgs();
				arg.NewFormMode = mDocument.FormMode;
				arg.OldFormMode = oldFormMode;
				FormModeChanged(this, arg);
			}
		}

		//--------------------------------------------------------------------------------------------------
		public void AfterNewErpDocument()
		{
			ErpDocumentID = -1;
			if (bookmarksToObserve.Count > 0)
				bookmarksToObserve.RemoveRange(0, bookmarksToObserve.Count);
		}

	
		//--------------------------------------------------------------------------------------------------
		public void AfterBrowseErpDocument()
		{
			ErpDocumentID = (CurrentRecordValid) ? ErpDocument.ErpDocumentID : -1;
		}

		//--------------------------------------------------------------------------------------------------
		public void AfterDeleteErpDocument()
		{
			if (DeleteErpDocument != null)
				DeleteErpDocument(this, new EventArgs());
		}

        //--------------------------------------------------------------------------------------------------
        public void StartBookmarkObserving()
        {
            if (bookmarksToObserve.Count > 0)
                bookmarksToObserve.RemoveRange(0, bookmarksToObserve.Count);

            if (SearchManager.GetCollection(CollectorName, CollectionName) == null)
                return;

            List<string> bookmarkFieldsToObserve = AttachManager.GetBookmarkFieldsToObserve(DocumentCollection.CollectionID);

            foreach (string field in bookmarkFieldsToObserve)
            {
                MSqlRecordItem recField = (MSqlRecordItem)MasterRecord.GetField(field);
                if (recField != null)
                    bookmarksToObserve.Add(new BookmarkToObserve(field, (MDataObj)recField.DataObj));
            }
        }

        //--------------------------------------------------------------------------------------------------
        public void EndBookmarkObserving()
		{
			if (bookmarksToObserve.Count > 0)
				SearchManager.SyncronizationIndexes(ErpDocument.ErpDocumentID, bookmarksToObserve);
		}

		//---------------------------------------------------------------------
		public int GetAllAttachmentsCount()
		{
			return SearchManager.GetAttachmentsCount(ErpDocumentID, AttachmentFilterType.Both);
		}

        //chiamato dal mondo unmanaged
        //--------------------------------------------------------------------------------------------------
        public ArchiveResult AttachFile(ref int attachmentId, string fileName, string strDescription)
        {
            ArchiveResult result = AttachManager.AttachFile(fileName, strDescription, out attachmentId, false, false, string.Empty);
            const BindingFlags FLAGS = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            var method = dc.GetType().GetMethod("ClearCache", FLAGS);
            method.Invoke(dc, null);
            return result;
        }
        
        //chiamato dal mondo unmanaged
        //--------------------------------------------------------------------------------------------------
        public ArchiveResult AttachReport(ref int attachmentId, string fileName, string strDescription, string barcode)
        {
            ArchiveResult result = AttachManager.AttachFile(fileName, strDescription, out attachmentId, true, true, barcode);
            const BindingFlags FLAGS = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            var method = dc.GetType().GetMethod("ClearCache", FLAGS);
            method.Invoke(dc, null);
            return result;
        }

        //--------------------------------------------------------------------------------------------------
        public ArchiveResult AttachBinaryContent(IntPtr binaryPtr, int lenght, string sourceFileName, string description, out int attachmentId)
        {
            attachmentId = -1;
            byte[] binaryContent = null;
            try
            {
                if (binaryPtr != null && lenght > 0)
                {
                    binaryContent = new byte[lenght];
                    Marshal.Copy(binaryPtr, binaryContent, 0, lenght);
                }
            }
            catch (OutOfMemoryException)
            {
                MessageEventArgs arg = new MessageEventArgs();
                arg.Explain = string.Format(Strings.ErrorArchivingTooLargeFile, sourceFileName, "1.2");
                arg.MessageType = DiagnosticType.Error;
                ShowMessage(this, arg);
                binaryContent = null;
            }

            return (binaryContent != null) ? AttachManager.AttachBinaryContent(binaryContent, sourceFileName, description, out attachmentId) : ArchiveResult.TerminatedWithError;
        }

        //chiamato dal mondo unmanaged
        //--------------------------------------------------------------------------------------------------
        public ArchiveResult AttachPapery(string barcode, string description, string fileName)
        {
            return IsValidEABarcodeValue(barcode) ? AttachManager.AttachPapery(barcode, description, fileName) : ArchiveResult.TerminatedWithError;
        }

        //chiamato dal mondo unmanaged
        //--------------------------------------------------------------------------------------------------
        public ArchiveResult AttachArchivedDocument(int archivedDocId, ref int attachmentId)
		{
			DMS_ArchivedDocument ad = SearchManager.GetArchivedDocument(archivedDocId);
			attachmentId = AttachManager.AttachPendingPapery(ad);
			return (attachmentId > -1) ? ArchiveResult.TerminatedSuccess : ArchiveResult.TerminatedWithError;
		}

		//chiamato dal mondo unmanaged
		//--------------------------------------------------------------------------------------------------
		public List<AttachmentInfo> GetAttachments(AttachmentFilterType filterType)
		{
			return SearchManager.GetAttachments(DocumentNamespace, DocumentPrimaryKey, filterType);
		}

		//chiamato dal mondo unmanaged
		//--------------------------------------------------------------------------------------------------
		public int GetAttachmentsCount(AttachmentFilterType filterType)
		{
			return SearchManager.GetAttachmentsCount(DocumentNamespace, DocumentPrimaryKey, filterType);
		}

		//--------------------------------------------------------------------------------------------------
		public bool DeletePapery(string barcodeValue)
		{
			return BarcodeManager.DeleteBarcodeForErpDocument(barcodeValue, ErpDocumentID);
		}

		///// <summary>
		/// dato un attachmentID restituisce i suoi Bookmark: collectionFields, FreeTags 
		/// </summary>
		//---------------------------------------------------------------------
		override public void GetAttachmentInfoBookmarks(ref AttachmentInfo attachmentInfo)
		{
			AttachManager.GetBookmarksValues(DocumentCollection.CollectionID, ref attachmentInfo);
		}

		//---------------------------------------------------------------------
		public bool UpdateAttachment(ref AttachmentInfo attachmentInfo, string description, string freeTags, string barcode, bool mainDoc, bool forMail)
		{
            if (!AttachManager.UpdateAttachment(ref attachmentInfo, description, freeTags, mainDoc, forMail))
				return false;

			// gestione ad-hoc per l'update del valore del barcode (solo se si tratta dello stesso valore)
			if (
				attachmentInfo.IsAPapery &&
				string.Compare(attachmentInfo.TBarcode.Value, barcode, StringComparison.InvariantCultureIgnoreCase) == 0
				)
			{
				// aggiorno le note solo se sono cambiate
				if (string.Compare(attachmentInfo.Description, description, StringComparison.InvariantCultureIgnoreCase) != 0)
				{
					if (BarcodeManager.UpdatePapery(barcode, description, string.Empty, this))
						attachmentInfo.Description = description;
				}
			}
			else
			{
				// richiamo l'aggiornamento del nuovo valore del barcode                    
				if (UpdateBarcode(attachmentInfo, barcode))
					attachmentInfo.TBarcode.Value = barcode;
			}

			return true;
		}

		///<summary>
		/// Apre e attiva la form contenente la lista degli allegati del documento. Restituisce gli allegati scelti dall'utente
		///</summary>
		//--------------------------------------------------------------------------------------------------
		public void OpenAttachmentsListForm(ref List<int> selectedAttachments, bool onlyForMail)
		{
			using (SafeThreadCallContext context = new SafeThreadCallContext())
			{
				try
				{
					List<AttachmentInfo> attachments = this.GetAttachments((onlyForMail) ? AttachmentFilterType.OnlyForMail : AttachmentFilterType.OnlyAttachment);
					AttachmentsListForm attachmentListForm = new AttachmentsListForm(attachments);
					//se o è null la form verrà posizionata in location 0,0 senza owner
					//attachmentListForm.Owner = (attachmentListForm.Parent != null) ? attachmentListForm.Parent.FindForm() : null;
					if (attachmentListForm.Owner == null)
						attachmentListForm.TopMost = true;

					if (attachmentListForm.ShowDialog() == DialogResult.OK)
					{
						foreach (AttachmentInfo att in attachmentListForm.SelectedDocuments)
							selectedAttachments.Add(att.AttachmentId);
						attachmentListForm.Dispose();
					}
				}
				catch (Exception exc)
				{
					Debug.WriteLine(exc.ToString());
				}
			}
		}

		//allow a web application to upload the file to attach to ERP documents used by PAI
		//--------------------------------------------------------------------------------------------------
		public ArchiveResult AttachFromTable()
		{
			inUnattendedMode = true;
			ArchiveResult result = AttachManager.AttachFromTable();
			inUnattendedMode = false;
			return result;
		}

        //chiamato dalla procedura AdjustAttachmentsForSOS mediante il CDMSAttachmentManagerObj
        //--------------------------------------------------------------------------------------------------
        public bool CreateNewSosDocument(int attachmentID)
        {

            /*var doc = from att in dc.DMS_Attachments
                      where att.AttachmentID == attachmentID
                      select att;

            if (doc != null && doc.Any())
            {
                AttachmentInfo attInfo = new AttachmentInfo((DMS_Attachment)doc.Single(), this);
                ArchiveManager.GetBookmarksValues(DocumentCollection.CollectionID, ref attInfo);
                attInfo.CreateSOSBookmark = true;
                return SosManager.CreateNewSosDocument(ref attInfo);
            }
            return false;*/

			var doc = from att in dc.DMS_Attachments where att.AttachmentID == attachmentID select att;

			bool result = true;

			if (doc != null && doc.Any())
			{
				AttachmentInfo attInfo = new AttachmentInfo((DMS_Attachment)doc.Single(), this);
				ArchiveManager.GetBookmarksValues(DocumentCollection.CollectionID, ref attInfo);
				attInfo.CreateSOSBookmark = true;
				result = SosManager.CreateNewSosDocument(ref attInfo);

				//per evitare che la memoria cresca a dismisura faccio la pulizia della cache
				const BindingFlags FLAGS = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
				var method = SosManager.DMSDataContext.GetType().GetMethod("ClearCache", FLAGS);
				method.Invoke(SosManager.DMSDataContext, null);
			}

			return result;
		}

        //chiamato dal mondo unmanaged
        //--------------------------------------------------------------------------------------------------        
        public void GetDocumentSOSInfo(out int sosStatus, out int attachmentID, out string statusMsg)
        {
            StatoDocumento stato = StatoDocumento.EMPTY;
            attachmentID = -1;
            statusMsg = string.Empty;
            if (DocumentNamespaceInSos)
                SosManager.GetDocumentSOSInfo(ErpDocumentID, out stato, out attachmentID, out statusMsg);
            sosStatus = (int)stato;
        }


		//--------------------------------------------------------------------------------
		internal string GetSOSDocumentType()
		{
			string sosDocType = string.Empty;
			if (Document != null)
			{
				sosDocType = Document.GetSosDocumentType();
				if (string.IsNullOrEmpty(sosDocType) && SOSConfigurationState != null)
					sosDocType = SOSConfigurationState.DocumentClasses.GetERPSOSDocumentTypeFromNs(DocumentNamespace);
				if (string.IsNullOrEmpty(sosDocType) && Document != null)
					sosDocType = Document.Title;
			}

			return sosDocType;
		}

		///<summary>
		/// Evento intercettato dai vari manager che rimpallano i vari errori riscontrati 
		/// Con le informazioni popolo il Diagnostic e lo visualizzo con il DiagnosticViewer.
		///</summary>
		//--------------------------------------------------------------------------------------------------
		override public void ShowMessage(object sender, MessageEventArgs e)
		{
			if (sender is Diagnostic)
				DMSDiagnostic.Set((Diagnostic)sender);
			else
			{
				// se non ho un messaggio non procedo
				if (string.IsNullOrWhiteSpace(e.Explain))
					return;

				ExtendedInfo ei = null;
				// se il message contiene dei valori assegno le variabili dell'eccezione, altrimenti
				// assegno solo le info base
				if (!string.IsNullOrWhiteSpace(e.Message))
				{
					ei = new ExtendedInfo();
					ei.Add(Strings.Description, e.Message);
					ei.Add(Strings.Source, e.Source);
					ei.Add(Strings.StackTrace, e.StackTrace);
					ei.Add(Strings.Function, e.Function);
					ei.Add(Strings.Library, e.Library);
				}

				DMSDiagnostic.Set(e.MessageType, e.Explain, ei);
			}

			if (InUnattendedMode)
			{
				if (Document != null)
					Document.AddMessage(DMSDiagnostic.GetErrorsStrings(), DiagnosticType.Error);
			}
			else
				// imposto questa proprietà in modo da evitare l'errore di Cross-Thread execution
				// N.B. noi siamo sullo stesso thread, è a basso livello che il framework fa casino quando
				// tenta di portare in secondo piano la form di Mago, che invece è su un altro thread
				using (SafeThreadCallContext context = new SafeThreadCallContext())
					DiagnosticViewer.ShowDiagnosticAndClear(DMSDiagnostic);
		}
	}
}