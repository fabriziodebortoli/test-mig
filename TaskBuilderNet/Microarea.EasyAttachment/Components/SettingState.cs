using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Xml.Serialization;

namespace Microarea.EasyAttachment.Components
{
	// enumerativo per identificare la tipologia di setting (ovvero per chi e' stato salvato)
	//-------------------------------------------------------------------------
	public enum SettingType
	{
		Standard = 1,
		AllUsers = 2
	}

	///<summary>
	/// Classe serializzabile che identifica un record della tabella DMS_Settings
	/// Sono serializzate le sole proprieta' public
	///</summary>
	//-------------------------------------------------------------------------
	[Serializable]
	public class SettingState
	{
		[XmlIgnore]
		public int WorkerID = -1;
		[XmlIgnore]
		public SettingType Type = SettingType.Standard;

		private SettingStateOptions options = new SettingStateOptions();
		
		public SettingStateOptions Options { get { return options; } set { options = value; } }

		//--------------------------------------------------------------------
		public SettingState() { }

		/// <summary>
		/// Trasforma un Setting dalla sua versione in XML ad un oggetto di tipo Setting in memoria
		/// </summary>
		//--------------------------------------------------------------------
		public static SettingState Deserialize(string xmlString)
		{
			XmlSerializer serializer = new XmlSerializer(typeof(SettingState));
			using (StringReader sr = new StringReader(xmlString))
			{
				SettingState ss = (SettingState)serializer.Deserialize(sr);

				if (ss == null)
				{
					Debug.Assert(false);
					return null;
				}
				return ss;
			}
		}

		/// <summary>
		/// Trasforma il Setting caricato in memoria in una stringa da salvare su database
		/// </summary>
		//--------------------------------------------------------------------
		public string Serialize()
		{
			XmlSerializer serializer = new XmlSerializer(typeof(SettingState));
			using (StringWriter sw = new StringWriter())
			{
				serializer.Serialize(sw, this);
				return sw.ToString();
			}
		}
	}

	///<summary>
	/// Classe di opzioni serializzabile
	///</summary>
	//-------------------------------------------------------------------------
	[Serializable]
	public class SettingStateOptions
	{
		private AttachmentDeleteOptions deleteOptionsState			= new AttachmentDeleteOptions();
		private DuplicateDocOptions		duplicateOptionsState		= new DuplicateDocOptions();
		private BookmarksOptions		bookmarksOptionsState		= new BookmarksOptions();
		private RepositoryOptions		repositoryOptionsState		= new RepositoryOptions();
		private BarcodeDetectionOptions barcodeDetectionOptionsState= new BarcodeDetectionOptions();
		private FTSOptions				ftsOptionsState				= new FTSOptions();
		private SOSOptions				sosOptionsState				= new SOSOptions();
		private StorageOptions			storageOptionsState			= new StorageOptions();
		private AttachmentPanelOptions	attachmentPanelOptionsState = new AttachmentPanelOptions();
		private AttachmentPanelOptions	attachmentPanelTempOptionsState = new AttachmentPanelOptions();

		[XmlElement(ElementName = "AttachmentDeleteOptions")]
		public AttachmentDeleteOptions DeleteOptionsState { get { return deleteOptionsState; } set { deleteOptionsState = value; } }
		[XmlElement(ElementName = "DuplicateDocOptions")]
		public DuplicateDocOptions DuplicateOptionsState { get { return duplicateOptionsState; } set { duplicateOptionsState = value; } }
		[XmlElement(ElementName = "BookmarksOptions")]
		public BookmarksOptions BookmarksOptionsState { get { return bookmarksOptionsState; } set { bookmarksOptionsState = value; } }
		[XmlElement(ElementName = "RepositoryOptions")]
		public RepositoryOptions RepositoryOptionsState { get { return repositoryOptionsState; } set { repositoryOptionsState = value; } }
		[XmlElement(ElementName = "BarcodeDetectionOptions")]
		public BarcodeDetectionOptions BarcodeDetectionOptionsState { get { return barcodeDetectionOptionsState; } set { barcodeDetectionOptionsState = value; } }
		[XmlElement(ElementName = "FTSOptions")]
		public FTSOptions FTSOptionsState { get { return ftsOptionsState; } set { ftsOptionsState = value; } }
		[XmlElement(ElementName = "SOSOptions")]
		public SOSOptions SOSOptionsState { get { return sosOptionsState; } set { sosOptionsState = value; } }
		[XmlElement(ElementName = "StorageOptions")]
		public StorageOptions StorageOptionsState { get { return storageOptionsState; } set { storageOptionsState = value; } }
		[XmlElement(ElementName = "AttachmentPanelOptions")]
		public AttachmentPanelOptions AttachmentPanelOptionsState { get { return attachmentPanelOptionsState; } set { attachmentPanelOptionsState = value; } }
		[XmlIgnore]
		public AttachmentPanelOptions AttachmentPanelTempOptionsState { get { return attachmentPanelTempOptionsState; } set { attachmentPanelTempOptionsState = value; } }

		//--------------------------------------------------------------------
		public SettingStateOptions() { }
	}

	///<summary>
	/// Classe serializzabile con le opzioni di Delete di un attachment
	///</summary>
	//-------------------------------------------------------------------------
	[Serializable]
	public class AttachmentDeleteOptions
	{
		private bool enableDeleteOptions = true;
		private DeletingAttachmentAction deletingAttachmentAction = DeletingAttachmentAction.KeepArchivedDoc;

		[XmlAttribute(AttributeName = "enable")]
		public bool EnableDeleteOptions { get { return enableDeleteOptions; } set { enableDeleteOptions = value; } }
		public DeletingAttachmentAction DeletingAttachmentAction { get { return deletingAttachmentAction; } set { deletingAttachmentAction = value; } }

		//--------------------------------------------------------------------
		public AttachmentDeleteOptions() { }
	}

	///<summary>
	/// Classe serializzabile con le opzioni per il salvataggio del documento archiviato
	/// e modalita' di sostituzione/duplicazione (sia per il documento in interattivo che in modalita' batch)
	///</summary>
	//-------------------------------------------------------------------------
	[Serializable]
	public class DuplicateDocOptions
	{
		private bool enableDuplicateOptions = true;
		private DuplicateDocumentAction duplicateActionForDoc = DuplicateDocumentAction.AskMeBeforeAttachDoc;
		private DuplicateDocumentAction duplicateActionForBatch = DuplicateDocumentAction.ArchiveAndKeepBothDocs;

		[XmlAttribute(AttributeName = "enable")]
		public bool EnableDuplicateOptions { get { return enableDuplicateOptions; } set { enableDuplicateOptions = value; } }
		public DuplicateDocumentAction ActionForDocument { get { return duplicateActionForDoc; } set { duplicateActionForDoc = value; } }
		public DuplicateDocumentAction ActionForBatch { get { return duplicateActionForBatch; } set { duplicateActionForBatch = value; } }

		//--------------------------------------------------------------------
		public DuplicateDocOptions() { }
	}

	///<summary>
	/// Classe serializzabile con le opzioni per i bookmarks e le categorie
	///</summary>
	//-------------------------------------------------------------------------
	[Serializable]
	public class BookmarksOptions
	{
		private bool enableBookmarksOptions = true;
		private bool enableEmptyValues = true;

		[XmlAttribute(AttributeName = "enable")]
		public bool EnableBookmarksOptions { get { return enableBookmarksOptions; } set { enableBookmarksOptions = value; } }
		public bool EnableEmptyValues { get { return enableEmptyValues; } set { enableEmptyValues = value; } }

		//--------------------------------------------------------------------
		public BookmarksOptions() { }
	}

	///<summary>
	/// Classe serializzabile con le opzioni per il repository
	///</summary>
	//-------------------------------------------------------------------------
	[Serializable]
	public class RepositoryOptions
	{
		private bool enableRepositoryOptions = true;
		private bool enableToAttachFromRepository = true;
		private bool showOnlyMyArchivedDocs = false;
		private int dpiQualityImage = 300;
		private string excludedExtensions = string.Empty;
		private bool disableAttachFromReport = false;
		private List<ExtensionSize> extensions = new List<ExtensionSize>();

		[XmlAttribute(AttributeName = "enable")]
		public bool EnableRepositoryOptions { get { return enableRepositoryOptions; } set { enableRepositoryOptions = value; } }
		public bool EnableToAttachFromRepository { get { return enableToAttachFromRepository; } set { enableToAttachFromRepository = value; } }
		public bool ShowOnlyMyArchivedDocs { get { return showOnlyMyArchivedDocs; } set { showOnlyMyArchivedDocs = value; } }
		public int DpiQualityImage { get { return dpiQualityImage; } set { dpiQualityImage = value; } }
		public string ExcludedExtensions { get { return excludedExtensions; } set { excludedExtensions = value; } }
		public bool DisableAttachFromReport { get { return disableAttachFromReport; } set { disableAttachFromReport = value; } }

		// serializzo in una lista le estensioni e relative size
		[XmlArray("MaxSizeForExtensionsInMB")]
		[XmlArrayItem("Extension")]
		public List<ExtensionSize> Extensions { get { return extensions; } set { extensions = value; } }

		//--------------------------------------------------------------------
		public RepositoryOptions() { }

		// data un'estensione ritorna l'oggetto ExtensionSize memorizzato nella lista
		//--------------------------------------------------------------------
		public ExtensionSize GetExtensionSize(string extension)
		{
			return extensions.Find(ext => string.Compare(ext.Name, extension, StringComparison.InvariantCultureIgnoreCase) == 0);
		}
	}

	///<summary>
	/// Classe serializzabile con l'elenco delle dimensioni per le estensioni ammesse
	///</summary>
	//-------------------------------------------------------------------------
	[Serializable]
	public class ExtensionSize
	{
		private string name = string.Empty;
		private int size = 0;

		[XmlAttribute(AttributeName = "name")]
		public string Name { get { return name; } set { name = value; } }
		[XmlAttribute(AttributeName = "size")]
		public int Size { get { return size; } set { size = value; } }

		[XmlIgnore]
		public long SizeInBytes { get { return (size > 0) ? (size * 1024 * 1024) : 0; } } // property che converte la Size da MB a Bytes
	}

	///<summary>
	/// Classe serializzabile con le opzioni per il riconoscimento del barcode all'interno dei documenti
	///</summary>
	//-------------------------------------------------------------------------
	[Serializable]
	public class BarcodeDetectionOptions
	{
		private bool enableBarcodeDetectionOptions = false; // di default la gestione e' disabilitata
		private bool enableBarcode = false;
		private bool automaticBarcodeDetection = true;
		private BarcodeDetectionAction barcodeDetectionAction = BarcodeDetectionAction.DetectOnlyInFirstPage;
		// gestione duplicazione barcode
		private DuplicateDocumentAction duplicateBarcodeForDoc = DuplicateDocumentAction.AskMeBeforeAttachDoc;
		private DuplicateDocumentAction duplicateBarcodeForBatch = DuplicateDocumentAction.ArchiveAndKeepBothDocs;

        private BarCodeType barcodeType = BarCodeType.BC_CODE39;
        private string barcodePrefix = "EA";
		
        private bool printBarcodeInReport = false;

		[XmlAttribute(AttributeName = "enable")]
		public bool EnableBarcodeDetectionOptions { get { return enableBarcodeDetectionOptions; } set { enableBarcodeDetectionOptions = value; } }
		public bool EnableBarcode { get { return enableBarcode; } set { enableBarcode = value; } }
		public bool AutomaticBarcodeDetection { get { return automaticBarcodeDetection; } set { automaticBarcodeDetection = value; } }
		public BarcodeDetectionAction BarcodeDetectionAction { get { return barcodeDetectionAction; } set { barcodeDetectionAction = value; } }
		public DuplicateDocumentAction BCActionForDocument { get { return duplicateBarcodeForDoc; } set { duplicateBarcodeForDoc = value; } }
		public DuplicateDocumentAction BCActionForBatch { get { return duplicateBarcodeForBatch; } set { duplicateBarcodeForBatch = value; } }
        public BarCodeType BarcodeType { get { return barcodeType; } set { barcodeType = value; } }
        public string BarcodePrefix { get { return barcodePrefix; } set { barcodePrefix = value; Utils.BarcodePrefix = value; } }
		public bool PrintBarcodeInReport { get { return printBarcodeInReport; } set { printBarcodeInReport = value; } }

		//--------------------------------------------------------------------
		public BarcodeDetectionOptions() { }

        //--------------------------------------------------------------------
        public string BarcodeTypeValue { get { return BarcodeMapping.GetBarCodeDescription(barcodeType); } }
    }

	///<summary>
	/// Classe serializzabile con le opzioni per la Full-Text Search
	///</summary>
	//-------------------------------------------------------------------------
	[Serializable]
	public class FTSOptions
	{
		private bool enableFTSOptions = false; // di default la gestione e' disabilitata
		private bool enableFTS = false;
        private bool ftsNotConsiderPdF = false;

        [XmlAttribute(AttributeName = "enable")]
		public bool EnableFTSOptions { get { return enableFTSOptions; } set { enableFTSOptions = value; } }
		public bool EnableFTS { get { return enableFTS; } set { enableFTS = value; } }
        public bool FTSNotConsiderPdF { get { return ftsNotConsiderPdF; } set { ftsNotConsiderPdF = value; } }

        //--------------------------------------------------------------------
        public FTSOptions() { }
	}

	///<summary>
	/// Classe serializzabile con le opzioni per il SOSConnector
	///</summary>
	//-------------------------------------------------------------------------
	[Serializable]
	public class SOSOptions
	{
		private bool enableSOSOptions = false; // di default la gestione e' disabilitata
		private bool enableSOS = false;
		private int maxElementsInEnvelope = 200;

		[XmlAttribute(AttributeName = "enable")]
		public bool EnableSOSOptions { get { return enableSOSOptions; } set { enableSOSOptions = value; } }
		public bool EnableSOS { get { return enableSOS; } set { enableSOS = value; } }
		public int MaxElementsInEnvelope { get { return maxElementsInEnvelope; } set { maxElementsInEnvelope = value; }	}

		//--------------------------------------------------------------------
		public SOSOptions() { }
	}

	///<summary>
	/// Classe serializzabile con le opzioni per la scelta dello storage
	///</summary>
	//-------------------------------------------------------------------------
	[Serializable]
	public class StorageOptions
	{
		private bool enableStorageOptions = false; // di default la gestione e' disabilitata
		private bool storageToFileSystem = false;
		private string storageFolderPath = string.Empty;

		[XmlAttribute(AttributeName = "enable")]
		public bool EnableStorageOptions { get { return enableStorageOptions; } set { enableStorageOptions = value; } }
		public bool StorageToFileSystem { get { return storageToFileSystem; } set { storageToFileSystem = value; } }
		public string StorageFolderPath { get { return storageFolderPath; } set { storageFolderPath = value; } }

		//--------------------------------------------------------------------
		public StorageOptions() { }
	}

	///<summary>
	/// Classe serializzabile con le opzioni di visualizzazione del nr degli allegati nel pannello laterale
	///</summary>
	//-------------------------------------------------------------------------
	[Serializable]
	public class AttachmentPanelOptions
	{
		private bool enableAttachmentPanelOptions = true;
		private DateRange searchDateRange = new DateRange(true);
		private int maxDocNumber = 10;

		[XmlAttribute(AttributeName = "enable")]
		public bool EnableAttachmentPanelOptions { get { return enableAttachmentPanelOptions; } set { enableAttachmentPanelOptions = value; } }
		public DateRange SearchDateRange { get { return searchDateRange; } set { searchDateRange = value; } }
		public int MaxDocNumber { get { return maxDocNumber; } set { maxDocNumber = value; } }

		//--------------------------------------------------------------------
		public AttachmentPanelOptions() { }
	}
}