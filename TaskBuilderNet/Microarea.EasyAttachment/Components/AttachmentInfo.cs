using System;
using System.Data.SqlTypes;
using System.IO;
using System.Xml.Serialization;
using Microarea.EasyAttachment.BusinessLogic;
using Microarea.EasyAttachment.Core;
using Microarea.TaskBuilderNet.Core.WebServicesWrapper;
using Microarea.TaskBuilderNet.Core.NameSolver;

namespace Microarea.EasyAttachment.Components
{
    public enum StorageTypeEnum { Database, FileSystem };

    ///<summary>
    /// Classe di appoggio con le informazioni relative ad un documento da archiviare
    /// un documento può essere archiviato mediante la sua path oppure tramite il passaggio direttamente del binario
    ///</summary>
    //================================================================================
    public class DocumentToArchive : IDisposable
    {
        private byte[] binaryContent = null;
        public bool IsAFile = true; //true only if the document is a file

        public bool IsABigFile = false;

        public string DocumentPath = string.Empty;
        public string Description = string.Empty;
        public long CRC = -1;

        public bool IsWoormReport = false;
        public string Barcode = string.Empty;
        public bool SkipDetectBarcode = false;

        public byte[] BinaryContent { get { return binaryContent; } set { binaryContent = value; IsAFile = false; } }

        public long Size
        {
            get
            {
                long size = 0;
                if (IsAFile)
                {
                    FileInfo fileInfo = new FileInfo(DocumentPath);
                    if (fileInfo.Exists)
                        size = fileInfo.Length;
                }
                else
                    size = (BinaryContent != null) ? BinaryContent.Length : 0;

                return size;
            }
        }


        //--------------------------------------------------------------
        public DateTime CreationTimeUtc
        {
            get
            {
                DateTime date = DateTime.MinValue;
                if (IsAFile)
                {
                    FileInfo fileInfo = new FileInfo(DocumentPath);
                    if (fileInfo.Exists)
                        date = fileInfo.CreationTimeUtc;
                }
                else
                    date = DateTime.UtcNow;

                return date;
            }
        }

        //--------------------------------------------------------------
        public DateTime LastWriteTimeUtc
        {
            get
            {
                DateTime date = DateTime.MinValue;
                if (IsAFile)
                {
                    FileInfo fileInfo = new FileInfo(DocumentPath);
                    if (fileInfo.Exists)
                        date = fileInfo.LastWriteTimeUtc;
                }
                else
                    date = DateTime.UtcNow;

                return date;
            }
        }

        //----------------------------------------------------------------
        public void Dispose()
        {
            binaryContent = null;
        }


    }
    ///<summary>
    /// Classe di appoggio con le info minime per un attachment
    ///</summary>
    //================================================================================
    [Serializable]
	public class AttachmentInfo : IDisposable
    {
		//questo serve per il caricamento OnDemand del contenuto binario del documento archiviato. Così si evita di avero in memoria inutilmente
		private byte[] docContent = null;
		private DMSOrchestrator dmsOrchestrator = null;
        private string storageFile = string.Empty;
		
		[XmlIgnore]
		public long KBSize = 0;
		[XmlIgnore]
		public long OriginalSize = 0;
		[XmlIgnore]
		public bool VeryLargeFile = false;
		[XmlIgnore]
		public string TempPath = null;
		[XmlIgnore]		
		public string Tags = string.Empty;
        [XmlIgnore]
        public string ModifiedBy = string.Empty;
		[XmlIgnore]
		public int ModifierID = -1;
		[XmlIgnore]
		public string CreatedBy = string.Empty;
		[XmlIgnore]
		public bool IsAFile = false; //true only if the AttachmentInfo is a File 
		[XmlIgnore]
		public bool IsAPapery = false; // solo per i pending papery


        [XmlIgnore]
        public StorageTypeEnum StorageType = StorageTypeEnum.Database;

     	[XmlIgnore]
		public DateTime CreationTimeUtc = DateTime.MinValue;
		[XmlIgnore]
		public DateTime LastWriteTimeUtc = DateTime.MinValue;
		[XmlIgnore]
		public DateTime ArchivedDate = DateTime.MinValue;
		[XmlIgnore]
		public DateTime AttachedDate = DateTime.MinValue;
		[XmlIgnore]
		public DateTime ModifiedDate = DateTime.MinValue;
		[XmlIgnore]
		public BookmarksDataTable BookmarksDT = null;
		[XmlIgnore]
		public bool OnDemandInfoLoaded = false;
		[XmlIgnore]
		public bool Changed = false;
		[XmlIgnore]
		public bool IsWoormReport = false;
        [XmlIgnore]
        public bool IsMainDoc = false;
		[XmlIgnore]
		public bool IsForMail = false;
		[XmlIgnore]
		public int CollectionID = -1;
        [XmlIgnore]
        public int ErpDocumentID = -1;

		// Datamember per il SOSConnector
		[XmlIgnore]
		public string SOSAbsoluteCode = string.Empty;
		[XmlIgnore]
		public string SOSLotID = string.Empty;
		[XmlIgnore]
		public DateTime SOSRegistrationDate = (DateTime)SqlDateTime.MinValue;
		[XmlIgnore]
		public StatoDocumento SOSDocumentStatus = StatoDocumento.EMPTY;
        [XmlIgnore]
        public bool CreateSOSBookmark = false;

		public int AttachmentId { get; set; }
		public int ArchivedDocId { get; set; }
		public TypedBarcode TBarcode { get; set; }
		public string ExtensionType;
		public string Name { get; set; }
		public string OriginalPath { get; set; }
		public string Description { get; set; }

		// Per Brain Business
		public int WFApprovalStatus { get; set; } //private enum Status { NotApproved=0, Approved, Rejected, UnDefined }; -->in BBConnector
		public int WFApproverId { get; set; }
		public DateTime WFRequestDate { get; set; }
		public DateTime WFApprovalDate { get; set; }
		public string WFRequestComment { get; set; }
		public string WFApprovalComment { get; set; }

		[XmlIgnore]
		public string ERPDocNamespace { get; set; }
		[XmlIgnore]
		public string ERPPrimaryKeyValue { get; set; }

        [XmlIgnore]
        public DMSOrchestrator DMSOrchestrator { get { return dmsOrchestrator; } }

        [XmlIgnore]
		//---------------------------------------------------------------------
		public byte[] DocContent 
		{ 
			get 
			{
                if (docContent == null)
                        docContent = dmsOrchestrator.GetBinaryContent(this, ref VeryLargeFile);
                return docContent;
			}
		}


        [XmlIgnore]
        //---------------------------------------------------------------------
        public string StorageFile
        {
            get
            {
                if (string.IsNullOrEmpty(storageFile))
                    storageFile = dmsOrchestrator.GetStorageFileName(this);
                return storageFile;
            }
        }


		//serve per la serializzazione
		//---------------------------------------------------------------------
		public AttachmentInfo()
		{ 
		}

		//---------------------------------------------------------------------
		public AttachmentInfo(DMS_ArchivedDocument archivedDoc, DMSOrchestrator dmsOrch)
		{
			dmsOrchestrator = dmsOrch;
			ArchivedDocId = archivedDoc.ArchivedDocID;
			OriginalPath = archivedDoc.Path;
			OriginalSize = archivedDoc.Size;
			KBSize = (archivedDoc.Size > 1024) ? archivedDoc.Size / 1024 : 1;
			ExtensionType = archivedDoc.ExtensionType;
			AttachmentId = -1;
			Name = archivedDoc.Name;
			Description = archivedDoc.Description;
			IsWoormReport =  (archivedDoc.IsWoormReport != null) ? (bool)archivedDoc.IsWoormReport : false;
            CreationTimeUtc = (DateTime)archivedDoc.CreationTimeUtc;
			LastWriteTimeUtc = (DateTime)archivedDoc.LastWriteTimeUtc;
            
			ArchivedDate = (DateTime) ((archivedDoc.TBCreated==null) ? DateTime.Now : archivedDoc.TBCreated);
			ModifiedDate = (DateTime)((archivedDoc.TBModified == null) ? DateTime.Now : archivedDoc.TBModified);
            TempPath = dmsOrch.GetArchiveDocTempFileName(this);

            TBarcode = new TypedBarcode(archivedDoc.Barcode, BarcodeMapping.GetBarCodeType(archivedDoc.BarcodeType));
			CollectionID = (int)archivedDoc.CollectionID;

			if (archivedDoc.TBCreatedID != null)
                CreatedBy = dmsOrchestrator.GetWorkerName(archivedDoc.TBCreatedID);
			
			if (archivedDoc.TBModifiedID != null)
				ModifiedBy = dmsOrchestrator.GetWorkerName(archivedDoc.TBModifiedID);

            ModifierID = (archivedDoc.ModifierID != null) ? (int)archivedDoc.ModifierID : -1;

			ERPDocNamespace = string.Empty;
			ERPPrimaryKeyValue = string.Empty;
            ErpDocumentID = -1;

            StorageType = (archivedDoc.StorageType == null || archivedDoc.StorageType == 0) ? StorageTypeEnum.Database : StorageTypeEnum.FileSystem;
        }

		//---------------------------------------------------------------------
		public AttachmentInfo(int archivedDocID, FileInfo file, DMSOrchestrator dmsOrch)
		{
			dmsOrchestrator = dmsOrch;
			ArchivedDocId = archivedDocID;
			AttachmentId = archivedDocID;
			OriginalPath = file.DirectoryName;
			OriginalSize = file.Length;
			KBSize = (OriginalSize > 1024) ? OriginalSize / 1024 : 1;
			ExtensionType = file.Extension;
			Name = file.Name;
			Description = Name;
			IsWoormReport = false;
			CreationTimeUtc = file.CreationTimeUtc;
			LastWriteTimeUtc = file.LastWriteTimeUtc;
			TempPath = file.FullName;
            TBarcode = new TypedBarcode("", dmsOrch.SettingsManager.UsersSettingState.Options.BarcodeDetectionOptionsState.BarcodeType);
			IsAFile = true;
			ERPDocNamespace = string.Empty;
			ERPPrimaryKeyValue = string.Empty;
		}

        //---------------------------------------------------------------------
        public AttachmentInfo(DocumentToArchive docToArchive, DMSOrchestrator dmsOrch)
        {
            FileInfo file = new FileInfo(docToArchive.DocumentPath);

            dmsOrchestrator = dmsOrch;
            ArchivedDocId = -1;
            AttachmentId = -1;
            OriginalPath = docToArchive.DocumentPath;
            OriginalSize = docToArchive.Size;
            KBSize = (OriginalSize > 1024) ? OriginalSize / 1024 : 1;
            ExtensionType = file.Extension;
            Name = file.Name;
            Description = Name;
            IsWoormReport = docToArchive.IsWoormReport;
            CreationTimeUtc = docToArchive.CreationTimeUtc;
            LastWriteTimeUtc = docToArchive.LastWriteTimeUtc;
            if (docToArchive.IsAFile)
                TempPath = file.FullName;
            else
            {
                if (dmsOrch.InUnattendedMode)
                {
                    string directoryPath = BasePathFinder.BasePathFinderInstance.GetWebProxyFilesPath();
                    if (!Directory.Exists(directoryPath))
                        Directory.CreateDirectory(directoryPath);
                    TempPath = string.Format("{0}\\{1}", directoryPath, Name);
                }
                else
                    TempPath = file.FullName;
            }

            TBarcode = new TypedBarcode("", dmsOrch.SettingsManager.UsersSettingState.Options.BarcodeDetectionOptionsState.BarcodeType);
            IsAFile = (docToArchive.IsAFile); //in ogni caso ho un file (o nella directory temporanea oppure come FullName
            ERPDocNamespace = string.Empty;
            ERPPrimaryKeyValue = string.Empty;
            docContent = (docToArchive.IsAFile) ? null : docToArchive.BinaryContent;
        }

        //---------------------------------------------------------------------
        public AttachmentInfo(DMS_Attachment attachment, DMSOrchestrator dmsOrch)
		{
			dmsOrchestrator = dmsOrch;
			Valorize(attachment);
		}
		
		//---------------------------------------------------------------------
		public void Valorize(DMS_Attachment attachment)
		{
			ExtensionType = attachment.DMS_ArchivedDocument.ExtensionType;
			AttachmentId = attachment.AttachmentID;
			ArchivedDocId = attachment.ArchivedDocID;
			KBSize = (attachment.DMS_ArchivedDocument.Size > 1024) ? attachment.DMS_ArchivedDocument.Size / 1024 : 1;
			OriginalSize = attachment.DMS_ArchivedDocument.Size;
			OriginalPath = attachment.DMS_ArchivedDocument.Path;
			Name = (!string.IsNullOrWhiteSpace(attachment.DMS_ArchivedDocument.Name)) ? attachment.DMS_ArchivedDocument.Name : string.Empty;
			Description = attachment.Description;
			IsWoormReport = (attachment.DMS_ArchivedDocument.IsWoormReport != null) ? (bool)attachment.DMS_ArchivedDocument.IsWoormReport : false;
            IsMainDoc = (attachment.IsMainDoc != null) ? (bool)attachment.IsMainDoc : false;
			IsForMail = (attachment.IsForMail != null) ? (bool)attachment.IsForMail : false;
			CreationTimeUtc = (DateTime)attachment.DMS_ArchivedDocument.CreationTimeUtc;
			LastWriteTimeUtc = (DateTime)attachment.DMS_ArchivedDocument.LastWriteTimeUtc;
			ArchivedDate = (DateTime)attachment.DMS_ArchivedDocument.TBCreated;
			AttachedDate = (DateTime)attachment.TBCreated;
			ModifiedDate = (DateTime)attachment.TBModified;

            TempPath = dmsOrchestrator.GetArchiveDocTempFileName(this);
            TBarcode = new TypedBarcode(attachment.DMS_ArchivedDocument.Barcode, BarcodeMapping.GetBarCodeType(attachment.DMS_ArchivedDocument.BarcodeType));
			CollectionID = attachment.CollectionID;

            if (attachment.TBCreatedID != null)
                CreatedBy = dmsOrchestrator.GetWorkerName(attachment.TBCreatedID);

            if (attachment.TBModifiedID != null)
				ModifiedBy = dmsOrchestrator.GetWorkerName(attachment.TBModifiedID);

            ModifierID = (attachment.DMS_ArchivedDocument.ModifierID != null) ? (int)attachment.DMS_ArchivedDocument.ModifierID : -1;
			
			ERPDocNamespace = attachment.DMS_ErpDocument.DocNamespace;
			ERPPrimaryKeyValue = attachment.DMS_ErpDocument.PrimaryKeyValue;
            ErpDocumentID = attachment.ErpDocumentID;
            
            // se la tipologia del documento gestionale e' in SOS procedo a valorizzare i dati del SOS
            if (dmsOrchestrator.IsDocumentNamespaceInSos(ERPDocNamespace))
				ValorizeSOSValues(attachment);


            StorageType = (attachment.DMS_ArchivedDocument.StorageType == null) ? StorageTypeEnum.Database : (StorageTypeEnum)attachment.DMS_ArchivedDocument.StorageType;
		}

		///<summary>
		/// Costruttore da utilizzare per creare un AttachmentInfo dalle informazioni di un pending papery
		///</summary>
		//---------------------------------------------------------------------
		public AttachmentInfo(DMS_ErpDocBarcode erpDoc, DMSOrchestrator dmsOrch)
		{
			dmsOrchestrator = dmsOrch;

			IsAPapery = true;

			TBarcode = new TypedBarcode(erpDoc.Barcode, BarCodeType.PAPERY);
			Name = erpDoc.Barcode;

			ExtensionType = FileExtensions.Papery; 
			Description = erpDoc.Notes;

			ArchivedDocId = dmsOrch.GetPaperyDocIdGlobal();
            AttachmentId = -1;
			ERPDocNamespace = string.Empty;
			ERPPrimaryKeyValue = string.Empty;
		}

		///<summary>
		/// Costruttore da utilizzare per creare un AttachmentInfo dalle informazioni di un pending papery
		///</summary>
		//---------------------------------------------------------------------
		public AttachmentInfo(string barcode, string notes, DMSOrchestrator dmsOrch)
		{
			dmsOrchestrator = dmsOrch;

			IsAPapery = true;

            TBarcode = new TypedBarcode(barcode, BarCodeType.PAPERY);
			Name = barcode;

			ExtensionType = FileExtensions.Papery;
			Description = notes;

			ArchivedDocId = dmsOrch.GetPaperyDocIdGlobal();
            AttachmentId = -1;
			
			ERPDocNamespace = string.Empty;
			ERPPrimaryKeyValue = string.Empty;
		}

		//---------------------------------------------------------------------
		public void DisposeDocContent()
		{
			docContent = null;
			//chiamare qualcosa per far liberare subito la memoria
		}

        //----------------------------------------------------------------
        public void Dispose()
        {
            DisposeDocContent();
        }

        //---------------------------------------------------------------------
        public override string ToString()
        {
            return Name;
        }

        //---------------------------------------------------------------------
        public bool SaveAttachmentFile()
        {
            return dmsOrchestrator != null && dmsOrchestrator.SaveAttachmentFile(this);
        }

        //---------------------------------------------------------------------
        public void OpenDocument()
        {
            if (dmsOrchestrator != null)
                dmsOrchestrator.OpenDocument(this);
        }
        
        //---------------------------------------------------------------------
        public string TransformToPdfA()
        {
            return (dmsOrchestrator != null) ? dmsOrchestrator.TransformToPdfA(this) : string.Empty;
        }
        //---------------------------------------------------------------------
        internal void SetModifiedBy()
        {
            ModifiedBy = dmsOrchestrator.GetCurrentWorkerName();
        }

        //---------------------------------------------------------------------
        internal void SetModifier(int workerId)
        {
            ModifierID = workerId;
        }

		///<summary>
		/// Metodo che mi consente di valorizzare i dati del SOSDocument agganciato all'Attachment
		///</summary>
		//---------------------------------------------------------------------
		internal void ValorizeSOSValues(DMS_Attachment attachment)
		{
			SOSDocumentInfo sosDoc = dmsOrchestrator.SosManager.GetSosDocumentInfo(attachment.AttachmentID);

			// SOSConnector (i dati li leggo dal record del SOSDocument (se esistente) oppure direttamente dall'Attachment)
			SOSAbsoluteCode = (sosDoc != null) ? sosDoc.AbsoluteCode : attachment.AbsoluteCode;
			SOSLotID = (sosDoc != null) ? sosDoc.LotID : attachment.LotID;
			SOSRegistrationDate = (sosDoc != null)
				? sosDoc.RegistrationDate
				: (attachment.RegistrationDate == null) ? (DateTime)SqlDateTime.MinValue : (DateTime)attachment.RegistrationDate;

			// se le colonne sulla tabella DMS_Attachment sono tutte valorizzate significa che il doc e' inserito in sostitutiva e deduco quindi il suo stato 
			if (
				!string.IsNullOrWhiteSpace(attachment.AbsoluteCode)
				&& attachment.AbsoluteCode != "0.0"
				&& !string.IsNullOrWhiteSpace(attachment.LotID)
				&& attachment.LotID != "0.0"
				&& attachment.RegistrationDate != null
				&& (DateTime)attachment.RegistrationDate != (DateTime)SqlDateTime.MinValue
				)
				SOSDocumentStatus = StatoDocumento.DOCSIGN;
			else // altrimenti leggo lo stato effettivo dal record della SOSDocument
				SOSDocumentStatus = (sosDoc != null) ? (StatoDocumento)sosDoc.DocumentStatus : StatoDocumento.EMPTY;
		}

        ///<summary>
		/// Permette di aggiungere una categoria ai bookmark dell'attachment
		///</summary>
		//---------------------------------------------------------------------		
        public void AddCategoryField(string name, string value)
        {
            if (BookmarksDT != null)
                BookmarksDT.AddCategory(name, value);
        }

        ///<summary>
        /// Permette di aggiungere una campo del dbtmaster del documento ai bookmark dell'attachment
        ///</summary>
        //---------------------------------------------------------------------		
        public void AddBindingField(string name)
        {
            if (BookmarksDT != null)
            {        
                //mi faccio prima dare il MSqlRecordItem del SqlRecord del DBTMaster avente al nome passato come argomento
                BookmarksDT.AddBindingField(name, dmsOrchestrator);
            }
        }
    }
}
