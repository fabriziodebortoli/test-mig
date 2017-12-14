using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Serialization;
using Ionic.Zip;

using Microarea.TaskBuilderNet.Core.DiagnosticManager;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Core.WebServicesWrapper;
using Microarea.TBPicComponents;

namespace Microarea.EasyAttachment.Core
{
	// tipo spedizione verso SOS
	//--------------------------------------------------------------------------------
	public enum SendingType
	{
		WebService, // 0: default
		FTP         // 1
	}

	#region SOSConfigurationState
	///<summary>
	/// Classe serializzabile che identifica un record della tabella DMS_SOSConfiguration
	/// Sono serializzate i soli datamember/proprieta' public
	/// <![CDATA[
	/// Esempio di output qui di seguito:
	///	<SOSConfigurationState xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema">
	///	  <DocumentClasses>
	///		<DocClassesList>
	///			<DocClass code="FATTEMESSE" description="Fatture emesse" internaldocclass="FATTEMESSE">
	///				<ERPDocNamespaces>
	///					<ERPSOSDocumentType erpDocNS="Document.ERP.Sales.Documents.Invoice" erpDocType="Fattura Immediata" />
	///					<ERPSOSDocumentType erpDocNS="Document.ERP.Sales.Documents.AccInvoice" erpDocType="Fattura Accompagnatoria" />
	///				</ERPDocNamespaces>
	///			</DocClass>
	///		</DocClassesList>
	///	  </DocumentClasses>
	///	</SOSConfigurationState> 
	///	]]>
	///</summary>
	//================================================================================
	[Serializable]
	public class SOSConfigurationState
	{
		private DMS_SOSConfiguration sosConfiguration = null;

		//--------------------------------------------------------------------------------
		[XmlIgnore]
		public DMS_SOSConfiguration SOSConfiguration { get { return sosConfiguration; } set { sosConfiguration = value; } }

		public DocClasses DocumentClasses = new DocClasses();

		//--------------------------------------------------------------------------------
		public SOSConfigurationState() { }

		/// <summary>
		/// Trasforma la colonna DocClasses dalla sua versione in XML ad un oggetto di tipo DocClassesState in memoria
		/// </summary>
		//--------------------------------------------------------------------
		public static SOSConfigurationState Deserialize(string xmlString)
		{
			XmlSerializer serializer = new XmlSerializer(typeof(SOSConfigurationState));
			using (StringReader sr = new StringReader(xmlString))
			{
				SOSConfigurationState dcs = (SOSConfigurationState)serializer.Deserialize(sr);

				if (dcs == null)
				{
					Debug.Assert(false);
					return null;
				}
				return dcs;
			}
		}

		/// <summary>
		/// Trasforma il DocClassesState caricato in memoria in una stringa da salvare su database
		/// </summary>
		//--------------------------------------------------------------------
		public string Serialize()
		{
			XmlSerializer serializer = new XmlSerializer(typeof(SOSConfigurationState));
			using (StringWriter sw = new StringWriter())
			{
				serializer.Serialize(sw, this);
				return sw.ToString();
			}
		}
	}

	//================================================================================
	[Serializable]
	public class DocClasses
	{
		public List<DocClass> DocClassesList = new List<DocClass>();

		//--------------------------------------------------------------------------------
		public DocClasses() { }

		// Dato il namespace di un documento ERP restituisce l'InternalDocClass
		//--------------------------------------------------------------------------------
		public string GetInternalDocClass(string docNamespace, string documentType = "")
		{
			foreach (DocClass doc in DocClassesList)
			{
				if (doc.GetErpDocument(docNamespace, documentType) != null)
					return doc.InternalDocClass;
			}
			return string.Empty;
		}

		// Dato l'InternalDocClass e il tipo documento restituisce il nome della classe documentale del SOS
		// (utilizzato nell'EASync per individuare la classe documentale specifica, nel caso delle classi
		// documentali accorpate)
		//--------------------------------------------------------------------------------
		public string GetDocClassCodeFromInternalClass(string internalDocClass, string docTypeDescription)
		{
			foreach (DocClass doc in DocClassesList)
			{
				if (string.Compare(doc.InternalDocClass, internalDocClass, StringComparison.InvariantCultureIgnoreCase) == 0)
				{
					foreach (ERPSOSDocumentType docType in doc.ERPDocNamespaces)
					{
						if (string.Compare(docType.DocType, docTypeDescription, StringComparison.InvariantCultureIgnoreCase) == 0)
							return doc.Code;
					}
				}

			}
			return string.Empty;
		}

		// Dato il namespace di un documento ERP restituisce il nome della classe documentale del SOS
		//--------------------------------------------------------------------------------
		public string GetDocClassCodeFromNs(string docNamespace, string documentType = "")
		{
			foreach (DocClass doc in DocClassesList)
			{
				if (doc.GetErpDocument(docNamespace, documentType) != null)
					return doc.Code;
			}
			return string.Empty;
		}

		// Dato il namespace di un documento ERP restituisce il tipo documento
		//--------------------------------------------------------------------------------
		public string GetERPSOSDocumentTypeFromNs(string docNamespace, string documentType = "")
		{
			foreach (DocClass doc in DocClassesList)
			{
				ERPSOSDocumentType erpDoc = doc.GetErpDocument(docNamespace, documentType);
				if (erpDoc != null)
					return erpDoc.DocType;
			}

			return string.Empty;
		}

		// Dato il codice restituisce l'istanza di DocClass corrispondente
		//--------------------------------------------------------------------------------
		public DocClass GetDocClass(string docClassCode)
		{
			foreach (DocClass doc in DocClassesList)
				if (string.Compare(doc.Code, docClassCode, StringComparison.InvariantCultureIgnoreCase) == 0)
					return doc;

			return null;
		}
	}

	//================================================================================
	[Serializable]
	public class ERPSOSDocumentType
	{
		[XmlAttribute(AttributeName = "erpDocNS")]
		public string DocNamespace = string.Empty;
		[XmlAttribute(AttributeName = "erpDocType")]
		public string DocType = string.Empty;

		//--------------------------------------------------------------------------------
		public ERPSOSDocumentType() { }

		//--------------------------------------------------------------------------------
		public ERPSOSDocumentType(string docNamespace, string docType)
		{
			DocNamespace = docNamespace;
			DocType = docType;
		}
	}

	//================================================================================
	[Serializable]
	public class DocClass
	{
		[XmlAttribute(AttributeName = "code")]
		public string Code = string.Empty;
		[XmlAttribute(AttributeName = "description")]
		public string Description = string.Empty;
		[XmlAttribute(AttributeName = "internaldocclass")]
		public string InternalDocClass = string.Empty;

		public List<ERPSOSDocumentType> ERPDocNamespaces = new List<ERPSOSDocumentType>();

		//--------------------------------------------------------------------------------
		public DocClass() { }

		//--------------------------------------------------------------------------------
		public ERPSOSDocumentType GetErpDocument(string docNamespace, string documentType)
		{
			foreach (ERPSOSDocumentType erpPDoc in ERPDocNamespaces)
				if (
						(string.Compare(erpPDoc.DocNamespace, docNamespace, StringComparison.InvariantCultureIgnoreCase) == 0) &&
						(string.IsNullOrEmpty(documentType) || string.Compare(erpPDoc.DocType, documentType, StringComparison.InvariantCultureIgnoreCase) == 0)
					)
					return erpPDoc;

			return null;
		}

		//--------------------------------------------------------------------------------
		public List<string> GetSOSDocumentTypes()
		{
			List<string> sosDocumentTypes = new List<string>();
			foreach (ERPSOSDocumentType erpSosDT in ERPDocNamespaces)
				sosDocumentTypes.Add(erpSosDT.DocType);

			return sosDocumentTypes;
		}
	}
	#endregion

	///<summary>
	/// Classe CORE del SOSManager, in uso sia da EasyAttachment che da EasyAttachmentSync
	///</summary>
	//================================================================================
	public class CoreSOSManager
	{
		private DMSModelDataContext dmsModel = null;
		private string tempPath = string.Empty;
		private string sosSubjectCode = string.Empty;
		private string companyName = string.Empty;
		private string dmsConnectionString = string.Empty;
		private int loginId = -1;
		private bool fromEASync = false;
		private long maxEnvelopeDimensionInBytes = 0;

		private SOSConfigurationState sosConfigurationState = null;

		// Properties
		// info che espongo perche' mi servono anche nell'EASync per effettuare le spedizioni
		//---------------------------------------------------------------------
		public string DocClassName { get; private set; }
		public int CollectionId { get; private set; }
		public List<SOSZipElement> SOSZipList { get; private set; }

		//---------------------------------------------------------------------
		public SOSConfigurationState SOSConfigurationState
		{
			get
			{
				if (sosConfigurationState != null)
					return sosConfigurationState;
				try
				{
					sosConfigurationState = LoadSOSConfiguration(sosSubjectCode);
				}
				catch (Exception e)
				{
					Debug.Fail(e.Message);
				}

				return sosConfigurationState;
			}

			set { sosConfigurationState = value; }
		}

		///<summary>
		/// Dato il subjectCode ritorna il record corrispondente nella tabella DMS_SOSConfiguration
		/// Per SubjectCode s'intende il codice cliente assegnato all'azienda, che viene poi riconosciuto
		/// all'interno del sistema Zucchetti
		/// Solitamente e' uguale alla Partita IVA dell'azienda
		///</summary>
		//--------------------------------------------------------------------------------
		private SOSConfigurationState LoadSOSConfiguration(string subjectCode)
		{
			// c'e' sempre solo un record in questa tabella
			var sosConf = from rec in dmsModel.DMS_SOSConfigurations where rec.ParamID == 0 select rec;

			DMS_SOSConfiguration sosConfiguration = null;
			SOSConfigurationState sosConfigurationState = null;

			try
			{
				sosConfiguration = (sosConf != null && sosConf.Any()) ? (DMS_SOSConfiguration)sosConf.Single() : null;

				// se esiste il record riempio la struttura con le informazioni
				if (sosConfiguration != null)
				{
					sosConfigurationState = (sosConfiguration.DocClasses != null)
											? SOSConfigurationState.Deserialize(sosConfiguration.DocClasses.ToString())
											: new SOSConfigurationState();

					// memorizzo tutto il record.. da fare DOPO la Deserialize, perche' altrimenti sovrascrive tutto il docState!
					sosConfigurationState.SOSConfiguration = sosConfiguration;
				}
				else
				{
					// creo un record con ParamID = 0 e inizializzo gli altri campi
					sosConfigurationState = new SOSConfigurationState();
					sosConfigurationState.SOSConfiguration = new DMS_SOSConfiguration();
					sosConfigurationState.SOSConfiguration.ParamID = 0;
					sosConfigurationState.SOSConfiguration.SubjectCode = subjectCode;
					sosConfigurationState.SOSConfiguration.KeeperCode = CoreStrings.SosKeeperCode;
					sosConfigurationState.SOSConfiguration.MySOSUser = string.Empty;
					sosConfigurationState.SOSConfiguration.MySOSPassword = Crypto.Encrypt(string.Empty);
					sosConfigurationState.SOSConfiguration.SOSWebServiceUrl = CoreStrings.SosWSUrl;
					sosConfigurationState.SOSConfiguration.AncestorCode = string.Empty;
					sosConfigurationState.SOSConfiguration.ChunkDimension = 20;
					sosConfigurationState.SOSConfiguration.EnvelopeDimension = 600;
					sosConfigurationState.SOSConfiguration.FTPSend = false;
					sosConfigurationState.SOSConfiguration.FTPSharedFolder = string.Empty;
					sosConfigurationState.SOSConfiguration.FTPUpdateDayOfWeek = 7;
					dmsModel.DMS_SOSConfigurations.InsertOnSubmit(sosConfigurationState.SOSConfiguration);
					dmsModel.SubmitChanges();
				}
			}
			catch (Exception e)
			{
				Debug.Fail("LoadSOSConfiguration", e.Message);
			}

			return sosConfigurationState;
		}

		///<summary>
		/// Constructor
		///</summary>
		///<param name="dataContext">contesto LINQ per eseguire query</param>
		///<param name="tempPath">i file temporanei e gli zip vengono creati dentro il path, quindi si pilota dall'esterno a seconda delle esigenze</param>
		///<param name="sosSubjectCode">codice soggetto per caricare i parametri dalla DMS_SOSConfiguratio</param>
		///<param name="companyName">per salvare il log nel folder dell'azienda che stiamo utilizzando</param>
		///<param name="dmsConnectionString">stringa di connessione al DMS (la property dc.Connection.ConnectionString non contiene l'eventuale password)</param>
		///<param name="loginId">loginId da salvare nella DMS_SOSEnvelope</param>
		///<param name="fromEASync">se l'oggetto e' istanziato dall'EASync</param>
		//--------------------------------------------------------------------------------
		public CoreSOSManager
			(
			DMSModelDataContext dataContext,
			string tempPath,
			string sosSubjectCode,
			string companyName,
			string dmsConnectionString,
			int loginId,
			bool fromEASync = false
			)
		{
			dmsModel = dataContext;

			this.tempPath = tempPath;
			this.sosSubjectCode = sosSubjectCode;
			this.companyName = companyName;
			this.dmsConnectionString = dmsConnectionString;
			this.loginId = loginId;
			this.fromEASync = fromEASync;

			// il parametro EnvelopeDimension e' espresso in MB, devo convertirlo in bytes
			this.maxEnvelopeDimensionInBytes = (long)(((SOSConfigurationState.SOSConfiguration.EnvelopeDimension == null) ? 600 : SOSConfigurationState.SOSConfiguration.EnvelopeDimension) * 1024 * 1024);
		}

		///<summary>
		/// Dato il file passato come parametro istanzia un GdPicturePDF e controlla
		/// se si tratta di un formato PDF/A o meno
		///</summary>
		//--------------------------------------------------------------------------------
		public bool IsPDFA(string path)
		{
			if (!FileExtensions.IsPdfPath(path))
				return false;

			TBPicPDF gdPicturePDF = new TBPicPDF();
			try
			{
				if (gdPicturePDF.LoadFromFile(path, false) == TBPictureStatus.OK)
				{
					// controllo il formato del PDF
					int pdfAConformance = gdPicturePDF.GetPDFAConformance(); //valori di ritorno: 0: non PDF/A, 1: PDF/A-A, 2: PDF/A-B, 9: Unknown
					gdPicturePDF.CloseDocument();
					return (pdfAConformance == 1 || pdfAConformance == 2);
				}
			}
			catch (Exception)
			{
				return false;
			}

			return false;
		}

		///<summary>
		/// Ritorna il path del file temporaneo dove e' stato salvato il contenuto binario del documento archiviato
		///</summary>
		//---------------------------------------------------------------------
		public string GetTemporaryFileName(DMS_ArchivedDocument archDoc, SqlConnection connection, out bool veryLargeFile)
		{
			veryLargeFile = false;

			if (archDoc.ArchivedDocID < 1)
				return string.Empty;

			try
			{
				return GetBinaryContent(archDoc, connection, ref veryLargeFile);
			}
			catch (Exception exc)
			{
				SOSLogWriter.WriteLogEntry(companyName, string.Format("{0} exception: {1}", exc.Message, "CoreSOSManager:GetTemporaryFileName"), extendedInfo: exc.ToString());
				throw (exc);
			}
		}

		//---------------------------------------------------------------------
		private string GetBinaryContent(DMS_ArchivedDocument archDoc, SqlConnection connection, ref bool veryLargeFile)
		{
			byte[] contentArray = null;
			string tempFileName = String.Format(@"{0}\{1}_({2}){3}", tempPath, Path.GetFileNameWithoutExtension(archDoc.Name), archDoc.ArchivedDocID.ToString(), Path.GetExtension(archDoc.Name));

			try
			{
				using (IDbCommand myCommand = connection.CreateCommand())
				{
					myCommand.CommandText = "SELECT [BinaryContent] FROM [DMS_ArchivedDocContent] WHERE [ArchivedDocID] = @archivedDocID";

					SqlParameter archDocIDParam = new SqlParameter("@archivedDocID", SqlDbType.Int);
					archDocIDParam.Value = Convert.ToInt32(archDoc.ArchivedDocID);
					myCommand.Parameters.Add(archDocIDParam);

					contentArray = (byte[])myCommand.ExecuteScalar();
				}

				if (contentArray == null || contentArray.Length == 0)
					return string.Empty;

				FileInfo f = new FileInfo(tempFileName);
				if (f.Exists)
					f.IsReadOnly = false;

				using (FileStream s = new FileStream(tempFileName, FileMode.OpenOrCreate))
					s.Write(contentArray, 0, contentArray.Length);

				contentArray = null;

				return tempFileName;
			}
			catch (SqlException e)
			{
				contentArray = null;
				SOSLogWriter.WriteLogEntry(companyName, string.Format("{0} exception: {1}", e.Message, "CoreSOSManager:GetBinaryContent"), extendedInfo: e.ToString());
				throw (e);
			}
			catch (OutOfMemoryException e)
			{
				// nel caso di eccezione OutOfMemoryException tento nuovamente di leggere
				// il documento dividendolo prima in piccoli pezzi
				contentArray = null;
				veryLargeFile = true;
				SOSLogWriter.WriteLogEntry(companyName, string.Format("{0} exception: {1}", e.Message, "CoreSOSManager:GetBinaryContent"), extendedInfo: e.ToString());
				if (GetBinaryContentForBigFile(archDoc, connection, tempFileName))
					return tempFileName;
			}
			catch (Exception e)
			{
				contentArray = null;
				SOSLogWriter.WriteLogEntry(companyName, string.Format("{0} exception: {1}", e.Message, "CoreSOSManager:GetBinaryContent"), extendedInfo: e.ToString());
				throw (e);
			}

			return string.Empty;
		}

		//---------------------------------------------------------------------
		private bool GetBinaryContentForBigFile(DMS_ArchivedDocument archDoc, SqlConnection connection, string tempFileName)
		{
			try
			{
				using (IDbCommand command = connection.CreateCommand())
				{
					command.CommandText = @"SELECT SUBSTRING([BinaryContent], @start, @length) 
											FROM [DMS_ArchivedDocContent] 
											WHERE [ArchivedDocID] = @archivedDocID";

					SqlParameter archDocIDParam = new SqlParameter("@archivedDocID", SqlDbType.Int);
					archDocIDParam.Value = Convert.ToInt32(archDoc.ArchivedDocID);
					command.Parameters.Add(archDocIDParam);

					SqlParameter startParam = new SqlParameter("@start", SqlDbType.BigInt);
					command.Parameters.Add(startParam);

					SqlParameter lengthParam = new SqlParameter("@length", SqlDbType.BigInt);
					command.Parameters.Add(lengthParam);

					long bytesRead = 0;

					FileInfo fi = new FileInfo(tempFileName);
					if (fi.Exists)
						fi.IsReadOnly = false;

					using (FileStream fs = new FileStream(tempFileName, FileMode.OpenOrCreate))
					{
						while (bytesRead < archDoc.Size)
						{
							startParam.Value = (bytesRead == 0) ? 1 : bytesRead + 1;
							// leggiamo a blocchi di 30MB
							lengthParam.Value = (archDoc.Size - bytesRead) > 31457280 ? 31457280 : (archDoc.Size - bytesRead);

							byte[] buffer = (byte[])command.ExecuteScalar();
							bytesRead += buffer.Length;

							// si e' optato per salvare il content in un file per problemi di outofmemory in caricamento
							// di file di dimensioni molto grandi utilizzando i buffer di byte
							fs.Write(buffer, 0, buffer.Length);
							buffer = null;
						}
					}
				}
			}
			catch (SqlException e)
			{
				SOSLogWriter.WriteLogEntry(companyName, string.Format("{0} exception: {1}", e.Message, "CoreSOSManager:GetBinaryContentForBigFile"), extendedInfo: e.ToString());
				return false;
			}
			catch (OutOfMemoryException ex)
			{
				SOSLogWriter.WriteLogEntry(companyName, string.Format("{0} exception: {1}", ex.Message, "CoreSOSManager:GetBinaryContentForBigFile"), extendedInfo: ex.ToString());
				return false;
			}
			catch (Exception e)
			{
				SOSLogWriter.WriteLogEntry(companyName, string.Format("{0} exception: {1}", e.Message, "CoreSOSManager:GetBinaryContentForBigFile"), extendedInfo: e.ToString());
				return false;
			}

			return true;
		}

		///<summary>
		/// Dato un Attachment va a creare il file temporaneo in formato PDF/A. 
		/// Il binario viene letto dalla tabella DMS_ArchivedDocContent oppure dalla tabella DMS_SOSDocument
		///</summary>
		//--------------------------------------------------------------------------------
		public string GetPDFATemporaryFile(DMS_Attachment attachment, SqlConnection eaConnection)
		{
			string temporaryFile = string.Empty;
			bool veryLargeFile = false;

			try
			{
				// se e' null nel SOS significa che e' gia' stato archiviato in formato PDF/A, quindi richiamo i metodi standard
				// altrimenti devo prendere in binario dal DMS_SOSDocument
				temporaryFile = //(attachment.DMS_SOSDocument.PdfABinary == null || attachment.DMS_SOSDocument.PdfABinary.Length <= 1)
						//? GetTemporaryFileName(attachment.DMS_ArchivedDocument, eaConnection, out veryLargeFile) :
						GetPDFABinaryContent(attachment.DMS_SOSDocument, eaConnection, out veryLargeFile);
			}
			catch (OutOfMemoryException e)
			{
				SOSLogWriter.WriteLogEntry(companyName, string.Format("{0} exception: {1}", e.Message, "CoreSOSManager:GetPDFATemporaryFile"), extendedInfo: e.ToString());
				temporaryFile = GetTemporaryFileName(attachment.DMS_ArchivedDocument, eaConnection, out veryLargeFile);
			}
			catch (Exception e)
			{
				SOSLogWriter.WriteLogEntry(companyName, string.Format("{0} exception: {1}", e.Message, "CoreSOSManager:GetPDFATemporaryFile"), extendedInfo: e.ToString());
				temporaryFile = string.Empty;
			}

			return temporaryFile;
		}

		//---------------------------------------------------------------------
		private string GetPDFABinaryContent(DMS_SOSDocument sosDoc, SqlConnection connection, out bool veryLargeFile)
		{
			veryLargeFile = false;
			byte[] contentArray = null;

			string tempFileName = String.Format(
								@"{0}\{1}_({2}){3}",
								tempPath,
								Path.GetFileNameWithoutExtension(sosDoc.FileName),
								sosDoc.AttachmentID.ToString(),
								Path.GetExtension(sosDoc.FileName));

			try
			{
				using (IDbCommand myCommand = connection.CreateCommand())
				{
					myCommand.CommandText = "SELECT [PdfABinary] FROM [DMS_SOSDocument] WHERE [AttachmentID] = @attachID";

					SqlParameter attIDParam = new SqlParameter("@attachID", SqlDbType.Int);
					attIDParam.Value = Convert.ToInt32(sosDoc.AttachmentID);
					myCommand.Parameters.Add(attIDParam);

					contentArray = (byte[])myCommand.ExecuteScalar();
				}

				if (contentArray == null || contentArray.Length <= 1)
					return string.Empty;

				FileInfo f = new FileInfo(tempFileName);
				if (f.Exists)
					f.IsReadOnly = false;

				using (FileStream s = new FileStream(tempFileName, FileMode.OpenOrCreate))
					s.Write(contentArray, 0, contentArray.Length);

				contentArray = null;

				return tempFileName;
			}
			catch (SqlException e)
			{
				contentArray = null;
				SOSLogWriter.WriteLogEntry(companyName, string.Format("{0} exception: {1}", e.Message, "CoreSOSManager:GetPDFABinaryContent"), extendedInfo: e.ToString());
				throw (e);
			}
			catch (OutOfMemoryException e)
			{
				// nel caso di eccezione OutOfMemoryException tento nuovamente di leggere
				// il documento dividendolo prima in piccoli pezzi
				contentArray = null;
				veryLargeFile = true;
				SOSLogWriter.WriteLogEntry(companyName, string.Format("{0} exception: {1}", e.Message, "CoreSOSManager:GetPDFABinaryContent"), extendedInfo: e.ToString());
				if (GetPDFABinaryContentForBigFile(sosDoc, connection, tempFileName))
					return tempFileName;
			}
			catch (Exception e)
			{
				contentArray = null;
				SOSLogWriter.WriteLogEntry(companyName, string.Format("{0} exception: {1}", e.Message, "CoreSOSManager:GetPDFABinaryContent"), extendedInfo: e.ToString());
				throw (e);
			}

			return string.Empty;
		}

		//---------------------------------------------------------------------
		private bool GetPDFABinaryContentForBigFile(DMS_SOSDocument sosDoc, SqlConnection connection, string tempFileName)
		{
			try
			{
				using (IDbCommand command = connection.CreateCommand())
				{
					command.CommandText = @"SELECT SUBSTRING([PdfABinary], @start, @length) 
											FROM [DMS_SOSDocument] 
											WHERE [AttachmentID] = @attachID";

					SqlParameter attIDParam = new SqlParameter("@attachID", SqlDbType.Int);
					attIDParam.Value = Convert.ToInt32(sosDoc.AttachmentID);
					command.Parameters.Add(attIDParam);

					SqlParameter startParam = new SqlParameter("@start", SqlDbType.BigInt);
					command.Parameters.Add(startParam);

					SqlParameter lengthParam = new SqlParameter("@length", SqlDbType.BigInt);
					command.Parameters.Add(lengthParam);

					long bytesRead = 0;

					FileInfo fi = new FileInfo(tempFileName);
					if (fi.Exists)
						fi.IsReadOnly = false;

					using (FileStream fs = new FileStream(tempFileName, FileMode.OpenOrCreate))
					{
						while (bytesRead < sosDoc.Size)
						{
							startParam.Value = (bytesRead == 0) ? 1 : bytesRead + 1;
							// leggiamo a blocchi di 30MB
							lengthParam.Value = (sosDoc.Size - bytesRead) > 31457280 ? 31457280 : (sosDoc.Size - bytesRead);

							byte[] buffer = (byte[])command.ExecuteScalar();
							bytesRead += buffer.Length;

							// si e' optato per salvare il content in un file per problemi di outofmemory in caricamento
							// di file di dimensioni molto grandi utilizzando i buffer di byte
							fs.Write(buffer, 0, buffer.Length);
							buffer = null;
						}
					}
				}
			}
			catch (SqlException e)
			{
				SOSLogWriter.WriteLogEntry(companyName, string.Format("{0} exception: {1}", e.Message, "CoreSOSManager:GetPDFABinaryContentForBigFile"), extendedInfo: e.ToString());
				return false;
			}
			catch (OutOfMemoryException e)
			{
				SOSLogWriter.WriteLogEntry(companyName, string.Format("{0} exception: {1}", e.Message, "CoreSOSManager:GetPDFABinaryContentForBigFile"), extendedInfo: e.ToString());
				return false;
			}
			catch (Exception e)
			{
				SOSLogWriter.WriteLogEntry(companyName, string.Format("{0} exception: {1}", e.Message, "CoreSOSManager:GetPDFABinaryContentForBigFile"), extendedInfo: e.ToString());
				return false;
			}

			return true;
		}

		///<summary>
		/// Metodo che elimina tutti i file dentro la cartella temporanea
		///</summary>
		//---------------------------------------------------------------------
		public void DeleteTemporaryFiles()
		{
			DirectoryInfo di = new DirectoryInfo(this.tempPath);
			foreach (FileInfo fi in di.EnumerateFiles())
			{
				try
				{
					fi.IsReadOnly = false;
					fi.Delete();
				}
				catch
				{
				}
			}
		}

		///<summary>
		/// Dato un attachmentId vado ad estrarre il CollectionId, la SosDocClass e il DocumentType
		/// ed ottengo la classe documentale da utilizzare nei nomi dell'envelope
		///</summary>
		//--------------------------------------------------------------------------------
		private void GetDocumentClass(List<int> attachmentIds)
		{
			// faccio una query secca sul primo record (tanto sono gia' accorpati per classe documentale)
			var attach = from a in dmsModel.DMS_Attachments where a.AttachmentID == attachmentIds[0] select a;
			DMS_Attachment attachment = (attach != null && attach.Any()) ? attach.Single() : null;

			string sosDocClass = string.Empty, docTypeDescription = string.Empty;
			CollectionId = -1;
			if (attachment != null)
			{
				sosDocClass = attachment.DMS_Collection.SosDocClass;
				docTypeDescription = attachment.DMS_SOSDocument.DocumentType;
				CollectionId = attachment.CollectionID;
			}

			if (string.IsNullOrWhiteSpace(sosDocClass) || string.IsNullOrWhiteSpace(docTypeDescription) || CollectionId == -1)
			{
				DocClassName = string.Empty;
				return;
			}

			// cerco il codice della classe documentale specifica da utilizzare per la spedizione (per i documenti contabili sono accorpate)
			DocClassName = SOSConfigurationState.DocumentClasses.GetDocClassCodeFromInternalClass(sosDocClass, docTypeDescription);
		}

		///<summary>
		/// Entry-point per la gestione FTP
		/// Comprende la generazione dei documenti in PDF/A, la creazione dello zip e relativo envelope,
		/// spostamento dello zip nel folder sharato per FTP
		///</summary>
		//--------------------------------------------------------------------------------
		public bool PrepareSOSEnvelopeForFTP(List<int> attachmentIds)
		{
			if (!PrepareSOSDocumentsAndEnvelope(attachmentIds))
			{
				SOSLogWriter.WriteLogEntry(companyName, "PrepareSOSDocumentsAndEnvelope (for FTP) ended with errors.");
				return false;
			}

			bool result = true;

			//-----------------------------------------------------------------------------
			// Gestione FTP
			//-----------------------------------------------------------------------------
			if ((bool)SOSConfigurationState.SOSConfiguration.FTPSend)
			{
				if (SOSZipList.Count == 0)
				{
					SOSLogWriter.WriteLogEntry(companyName, "SOSZipList does not contain files zip to move");
					result = false;
				}

				foreach (SOSZipElement sosZip in SOSZipList)
				{
					if (!MoveToFTPSharedFolder(sosZip.Elements, sosZip.ZipFilePath, CollectionId))
						result = result && false; // lo assegno tutte le volte perche' se avessi piu' file da spostare devo provarli cmq tutti
				}
			}

			DeleteTemporaryFiles();

			SOSLogWriter.WriteLogEntry(companyName, result ? "PrepareSOSEnvelopeForFTP successfully completed." : "PrepareSOSEnvelopeForFTP ended with errors.");
			return result;
		}

		///<summary>
		/// Utilizzato per la spedizione via FTP
		/// Entry-point dall'EASync per la spedizione in SOS via webservice
		///</summary>
		//--------------------------------------------------------------------------------
		public bool PrepareSOSDocumentsAndEnvelope(List<int> attachmentIds)
		{
			// leggo il nome della classe documentale da utilizzare
			GetDocumentClass(attachmentIds);

			if (string.IsNullOrWhiteSpace(DocClassName))
				return false;

			List<List<DMS_SOSDocument>> sosDocumentsGlobalList;
			// vado a caricare i SOSDocument e creo delle sottoliste in modo da non avere spedizioni 
			// di dimensione piu' grande di quella impostata nei parametri del web.config
			PrepareSOSDocuments(attachmentIds, out sosDocumentsGlobalList);

			if (sosDocumentsGlobalList == null || sosDocumentsGlobalList.Count == 0)
				return false;

			SOSZipList = new List<SOSZipElement>();

			foreach (List<DMS_SOSDocument> sosDocList in sosDocumentsGlobalList)
			{
				//-----------------------------------------------------------------------------
				// prepara i temporanei dei vari file pdfa e tengo da parte una lista di oggetti di tipo SOSEnvelopeElement
				// che contengono le informazioni che mi occorrono per i successivi passaggi
				//-----------------------------------------------------------------------------
				List<SOSEnvelopeElement> envElements;
				if (!PrepareDocForEnvelope(sosDocList, out envElements))
				{
					SOSLogWriter.WriteLogEntry(companyName, "PrepareDocForEnvelope ended with errors");
					ClearLINQLocalCache();
					continue;
				}

				ClearLINQLocalCache();

				if (envElements != null && envElements.Count == 0)
				{
					SOSLogWriter.WriteLogEntry(companyName, "PrepareDocForEnvelope has NO elements to send.");
					continue;
				}

				SOSLogWriter.WriteLogEntry(companyName, "PrepareDocForEnvelope successfully completed");

				//-----------------------------------------------------------------------------
				// creo la vera e propria envelope: creo il file evidenze.seq e zippo tutto insieme ai file pdf				
				//-----------------------------------------------------------------------------
				string zipFilePath = string.Empty;
				if (!CreateZipEnvelope(envElements, out zipFilePath) || string.IsNullOrWhiteSpace(zipFilePath))
				{
					SOSLogWriter.WriteLogEntry(companyName, "CreateZipEnvelope ended with errors");
					try
					{
						// se e' fallita la creazione dello zip devo mettere lo stato di tutti i documenti contenuti nello zip = TORESEND
						using (SqlConnection eaConnection = new SqlConnection(dmsConnectionString))
						{
							eaConnection.Open();

							using (SqlCommand myCommand = new SqlCommand("UPDATE [DMS_SOSDocument] SET [DocumentStatus] = @docStatus WHERE [AttachmentID] = @attId", eaConnection))
							{
								foreach (SOSEnvelopeElement elem in envElements)
								{
									myCommand.Parameters.Add(new SqlParameter("@docStatus", (int)StatoDocumento.TORESEND));
									myCommand.Parameters.Add(new SqlParameter("@attId", elem.AttachmentId));
									myCommand.ExecuteNonQuery();
									myCommand.Parameters.Clear();
									SOSLogWriter.AppendText(companyName, string.Format("SOSDocument with AttachmentID = {0} has been skipped and set to RESEND!", elem.AttachmentId.ToString()));
								}
							}
						}
					}
					catch (SqlException e)
					{
						SOSLogWriter.WriteLogEntry(companyName, "An error occurred updating SOSDocument after error in zip file creation", "PrepareSOSDocumentsAndEnvelope", e.ToString());
					}
					continue; // vado cmq avanti sulla lista successiva
				}

				SOSLogWriter.WriteLogEntry(companyName, string.Format("Zip envelope file {0} successfully created", zipFilePath));

				SOSZipElement sosZip = new SOSZipElement(zipFilePath);
				sosZip.Elements = envElements;
				SOSZipList.Add(sosZip);

				// serve per evitare che vengano generati nomi di zip omonimi (il timestamp arriva solo ai secondi)!
				System.Threading.Thread.Sleep(1000);
			}

			return true;
		}

		//-----------------------------------------------------------------------
		private void ClearLINQLocalCache()
		{
			const BindingFlags FLAGS = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
			var method = dmsModel.GetType().GetMethod("ClearCache", FLAGS);
			method.Invoke(dmsModel, null);
		}

		///<summary>
		/// Dato un oggetto di tipo SOSDocElement e gli AttachmentId in esso contenuti,
		/// per ognuno di essi trovo il relativo DMS_SOSDocument (con stato documento == TOSEND).
		///	Per la riga con colonna FileName non ancora valorizzata ed estensione del documento archiviato:
		/// - salvo in un file temporaneo il BinaryContent letto dalla tabella DMS_ArchivedDocContent
		/// - lo converto in PDFA se necessario e salvo il nuovo binario sulla tabella DMS_SOSDocument
		/// - calcolo il nome del file e HashCode e aggiorno le chiavi di descrizione (come prefisso e suffisso) e memorizzo tutto
		/// E modifico lo stato del documento in WAITING
		///</summary>
		//-----------------------------------------------------------------------
		private void PrepareSOSDocuments(List<int> attachmentIds, out List<List<DMS_SOSDocument>> sosDocumentsGlobalList)
		{
			long totalFileSize = 0;
			sosDocumentsGlobalList = new List<List<DMS_SOSDocument>>();
			List<DMS_SOSDocument> sosDocumentList = new List<DMS_SOSDocument>();

			try
			{
				using (SqlConnection eaConnection = new SqlConnection(dmsConnectionString))
				{
					eaConnection.Open();

					// scorro tutti gli attachmentId (ri-controllo anche lo Stato)
					for (int i = 0; i < attachmentIds.Count; i++)
					{
						int attachId = attachmentIds[i];

						string sosFileName = string.Empty;
						long size = -1;

						using (SqlCommand myCommand = new SqlCommand())
						{
							myCommand.Connection = eaConnection;
							myCommand.CommandText = "SELECT [FileName], [Size] FROM [DMS_SOSDocument] WHERE [AttachmentID] = @attId AND [DocumentStatus] = @docStatus";
							myCommand.Parameters.Add(new SqlParameter("@attId", attachId));
							myCommand.Parameters.Add(new SqlParameter("@docStatus", (int)StatoDocumento.TOSEND));

							using (SqlDataReader reader = myCommand.ExecuteReader())
							{
								if (reader != null && reader.Read())
								{
									sosFileName = reader["FileName"].ToString();
									size = (long)reader["Size"];
								}
							}
						}

						var att = from a in dmsModel.DMS_Attachments where a.AttachmentID == attachId select a;

						DMS_Attachment attachment = (att != null && att.Any()) ? att.Single() : null;
						if (attachment == null)
							continue;

						string pdfAFile = Path.Combine(this.tempPath, sosFileName);

						//se il file non esiste nella dir temporanea
						if (!File.Exists(pdfAFile))
						{
							// se non provengo dall'EASync (in caso di FTP) metto lo stato TORESEND e procedo al file successivo
							if (!fromEASync)
							{
								using (SqlCommand myCommand = new SqlCommand())
								{
									myCommand.Connection = eaConnection;
									myCommand.CommandText = @"UPDATE [DMS_SOSDocument] SET [DocumentStatus] = @docStatus WHERE [AttachmentID] = @attId";
									myCommand.Parameters.Add(new SqlParameter("@docStatus", (int)StatoDocumento.TORESEND));
									myCommand.Parameters.Add(new SqlParameter("@attId", attachId));
									myCommand.ExecuteNonQuery();
								}
								SOSLogWriter.AppendText(companyName, string.Format("Unable to find temporary file for AttachmentID = {0} (DocumentStatus will be set {1})", attachId.ToString(), StatoDocumento.TORESEND.ToString()));
								continue;
							}

							// mi ha chiamato l'EASync, allora salvo in un file temporaneo il contenuto binario del SOS document, letto dalla DMS_SOSDocument
							// stavolta nella temp di Windows!
							string temporaryPdfAFile = GetPDFATemporaryFile(attachment, eaConnection);
							if (string.IsNullOrWhiteSpace(temporaryPdfAFile))
							{
								using (SqlCommand myCommand = new SqlCommand())
								{
									myCommand.Connection = eaConnection;
									myCommand.CommandText = @"UPDATE [DMS_SOSDocument] SET [DocumentStatus] = @docStatus WHERE [AttachmentID] = @attId";
									myCommand.Parameters.Add(new SqlParameter("@docStatus", (int)StatoDocumento.TORESEND));
									myCommand.Parameters.Add(new SqlParameter("@attId", attachId));
									myCommand.ExecuteNonQuery();
								}
								SOSLogWriter.AppendText(companyName, string.Format("Unable to save the temporary file for AttachmentID = {0} (DocumentStatus is set {1})", attachId.ToString(), StatoDocumento.TORESEND.ToString()));

								continue; // se non sono riuscito a salvare il file temporaneo non procedo
							}

							// calcolo il path del file pdfa definitivo ed eseguo il rename (previa cancellazione del vecchio)
							pdfAFile = Path.Combine(Path.GetDirectoryName(temporaryPdfAFile), sosFileName);

							try
							{
								if (File.Exists(pdfAFile))
									File.Delete(pdfAFile);
								File.Move(temporaryPdfAFile, pdfAFile);
								// a questo punto posso eliminare anche il file temporaneo
								File.Delete(temporaryPdfAFile);
							}
							catch
							{
								using (SqlCommand myCommand = new SqlCommand())
								{
									myCommand.Connection = eaConnection;
									myCommand.CommandText = @"UPDATE [DMS_SOSDocument] SET [DocumentStatus] = @docStatus WHERE [AttachmentID] = @attId";
									myCommand.Parameters.Add(new SqlParameter("@docStatus", (int)StatoDocumento.TORESEND));
									myCommand.Parameters.Add(new SqlParameter("@attId", attachId));
									myCommand.ExecuteNonQuery();
								}
								SOSLogWriter.AppendText(companyName, string.Format("Unable to rename the temporary file for AttachmentID = {0} (DocumentStatus is set {1})", attachId.ToString(), StatoDocumento.TORESEND.ToString()));

								continue;
							}

							SOSLogWriter.AppendText(companyName, "File converted in PDF/A available in: " + pdfAFile);
						}

						// calcolo la Size del nuovo file PDF/A e la ri-assegno
						FileInfo fi = new FileInfo(pdfAFile);
						if (size != fi.Length)
							size = fi.Length;

						// aggiorno lo stato
						using (SqlCommand myCommand = new SqlCommand())
						{
							myCommand.Connection = eaConnection;
							myCommand.CommandText = @"UPDATE [DMS_SOSDocument] SET [FileName] = @fileName, [Size] = @size, [DocumentStatus] = @docStatus WHERE [AttachmentID] = @attId";
							myCommand.Parameters.Add(new SqlParameter("@fileName", sosFileName));
							myCommand.Parameters.Add(new SqlParameter("@size", size));
							myCommand.Parameters.Add(new SqlParameter("@docStatus", (int)StatoDocumento.WAITING));
							myCommand.Parameters.Add(new SqlParameter("@attId", attachId));
							myCommand.ExecuteNonQuery();
						}

						SOSLogWriter.AppendText(companyName, string.Format("Updating DocumentStatus = {0} for SOSDocument with AttachmentID = {1}", StatoDocumento.WAITING.ToString(), attachId));

						// estraggo il SosDoc
						var sosDocs = from doc in dmsModel.DMS_SOSDocuments
									  where doc.AttachmentID == attachId && doc.DocumentStatus == (int)StatoDocumento.WAITING
									  select doc;

						DMS_SOSDocument sDoc = (sosDocs != null && sosDocs.Any()) ? sosDocs.Single() : null;
						if (sDoc == null)
							continue;

						// inserisco il documento analizzato nella lista corrente
						sosDocumentList.Add(sDoc);

						// calcolo la dimensione dei singoli file PDF/A che sto analizzando
						totalFileSize += fi.Length;

						// se la dimensione totale dei file analizzati (in bytes) eccede la dimensione massima scelta nei parametri
						// mi tengo da parte la lista, e re-inizializzo la lista locale e il contatore della size
						if (totalFileSize > maxEnvelopeDimensionInBytes)
						{
							sosDocumentsGlobalList.Add(new List<DMS_SOSDocument>(sosDocumentList)); // devo fare la new cosi ne faccio una copia prima della Clear!
							sosDocumentList.Clear();
							totalFileSize = 0;
						}
						else
							if (i == attachmentIds.Count - 1) // se sono l'ultimo elemento della lista
							sosDocumentsGlobalList.Add(new List<DMS_SOSDocument>(sosDocumentList));

						ClearLINQLocalCache();
					}
				}
			}
			catch (Exception e)
			{
				SOSLogWriter.WriteLogEntry(companyName, string.Format("{0} exception: {1}", e.Message, "CoreSOSManager:PrepareSOSDocuments"), extendedInfo: e.ToString());
			}
		}

		///<summary>
		/// Per tutti gli attachment con stato == WAITING
		/// - creo il temporaneo del formato PDF/A (leggendo dalla tabella opportuna)
		/// - mi tengo da parte le info che mi servono 
		/// - ritorno una lista di elementi di tipo Envelope
		///</summary>
		//--------------------------------------------------------------------------------
		private bool PrepareDocForEnvelope(List<DMS_SOSDocument> sosDocumentsList, out List<SOSEnvelopeElement> envElements)
		{
			envElements = new List<SOSEnvelopeElement>();

			try
			{
				using (SqlConnection eaConnection = new SqlConnection(dmsConnectionString))
				{
					eaConnection.Open();

					foreach (DMS_SOSDocument sosDoc in sosDocumentsList)
					{
						string tempPdfaFile = string.Empty;

						// leggo le informazioni dell'Attachment, SOLO se ha lo stato WAITING
						var attach = from a in dmsModel.DMS_Attachments
									 where a.AttachmentID == sosDoc.AttachmentID && a.DMS_SOSDocument.DocumentStatus == (int)StatoDocumento.WAITING
									 select a;

						DMS_Attachment attachment;

						try
						{
							attachment = (attach != null && attach.Any()) ? attach.Single() : null;
							if (attachment == null)
								continue;

							// potrei avere ancora il temporaneo in pdfa salvato nella directory temporanea, cosi' evito di creare un altro file
							tempPdfaFile = Path.Combine(this.tempPath, attachment.DMS_SOSDocument.FileName);
						}
						catch (Exception e)
						{
							SOSLogWriter.WriteLogEntry(companyName, string.Format("{0} exception: {1}", e.Message, "CoreSOSManager:PrepareDocForEnvelope(1)"), extendedInfo: e.ToString());
							continue;
						}

						string pdfaTempPath = string.Empty;

						try
						{
							// se il file esiste e la sua dimensione e' maggiore di 1KB allora lo considero
							if (File.Exists(tempPdfaFile) && new FileInfo(tempPdfaFile).Length > 1 && CoreUtils.IsPDFA(tempPdfaFile))
								pdfaTempPath = tempPdfaFile;
							else
							{
								// se non esiste gia', salva il file temporaneo estraendo il binario in formato PDF/A
								string newPdfaTempPath = GetPDFATemporaryFile(attachment, eaConnection);

								// calcolo il path del file pdfa definitivo ed eseguo il rename (previa cancellazione del vecchio)
								string pdfAFile = Path.Combine(Path.GetDirectoryName(newPdfaTempPath), sosDoc.FileName);

								try
								{
									if (File.Exists(pdfAFile))
										File.Delete(pdfAFile);
									File.Move(newPdfaTempPath, pdfAFile);

									// eseguo un ulteriore controllo sul fatto che il file sia effettivamente in formato PDF/A
									// se cosi non fosse NON posso provare a convertirlo per problemi di licenza 
									if (!CoreUtils.IsPDFA(pdfAFile))
									{
										sosDoc.DocumentStatus = (int)StatoDocumento.TORESEND;
										dmsModel.SubmitChanges();
										continue;
									}
								}
								catch (Exception e)
								{
									sosDoc.DocumentStatus = (int)StatoDocumento.TORESEND;
									dmsModel.SubmitChanges();
									SOSLogWriter.WriteLogEntry(companyName, string.Format("{0} exception: {1}", e.Message, "CoreSOSManager:PrepareDocForEnvelope(2)"), extendedInfo: e.ToString());
									continue;
								}

								pdfaTempPath = pdfAFile;
							}

							// in ogni caso ricalcolo l'HashCode, perche' sul db potrebbe essere stato memorizzato quello sbagliato
							// (caso del file da 1 byte corrotto)
							string newHashCode = CoreUtils.CreateDocumentHash256(pdfaTempPath);
							// se e' vuoto annullo tutto
							if (string.IsNullOrWhiteSpace(newHashCode))
								pdfaTempPath = string.Empty;
							else
								// se e' diverso da quello salvato sul database lo ri-assegno
								if (string.Compare(sosDoc.HashCode, newHashCode, StringComparison.InvariantCultureIgnoreCase) != 0)
							{
								// mi tengo da parte il vecchio
								string oldHashCode = sosDoc.HashCode;
								// se le chiavi di descrizione finiscono col vecchio
								if (sosDoc.DescriptionKeys.EndsWith(oldHashCode, StringComparison.InvariantCultureIgnoreCase))
								{
									// calcolo la Size del nuovo file PDF/A
									FileInfo fi = new FileInfo(pdfaTempPath);
									sosDoc.Size = fi.Length;

									// assegno il nuovo sia al codice che alle chiavi
									sosDoc.HashCode = newHashCode;
									sosDoc.DescriptionKeys = sosDoc.DescriptionKeys.Replace(oldHashCode, newHashCode);
									dmsModel.SubmitChanges();
								}
								else // altrimenti annullo tutto
									pdfaTempPath = string.Empty;
							}
						}
						catch (Exception e)
						{
							SOSLogWriter.WriteLogEntry(companyName, string.Format("{0} exception: {1}", e.Message, "CoreSOSManager:PrepareDocForEnvelope(3)"), extendedInfo: e.ToString());
							pdfaTempPath = string.Empty;
						}

						if (string.IsNullOrWhiteSpace(pdfaTempPath))
						{
							SOSLogWriter.WriteLogEntry(companyName, string.Format("Unable to convert in PDF-A: file {0} has been skipped and set to RESEND!", tempPdfaFile), "PrepareDocForEnvelope");
							sosDoc.DocumentStatus = (int)StatoDocumento.TORESEND;
							dmsModel.SubmitChanges();
							continue;
						}

						try
						{
							// mi tengo da parte le informazioni del singolo elemento dell'envelope
							SOSEnvelopeElement sosEnv = new SOSEnvelopeElement(attachment.AttachmentID, pdfaTempPath, attachment.DMS_SOSDocument.DescriptionKeys);
							envElements.Add(sosEnv);

							ClearLINQLocalCache();
						}
						catch (Exception e)
						{
							SOSLogWriter.WriteLogEntry(companyName, string.Format("{0} exception: {1}", e.Message, "CoreSOSManager:PrepareDocForEnvelope(4)"), extendedInfo: e.ToString());
						}
					}
				}
			}
			catch (Exception e)
			{
				SOSLogWriter.WriteLogEntry(companyName, string.Format("{0} exception: {1}", e.Message, "CoreSOSManager:PrepareDocForEnvelope(5)"), extendedInfo: e.ToString());
				return false;
			}

			return true;
		}

		///<summary>
		/// Crea il file con le chiavi di descrizione evidenze.seq e scrive al suo interno l'elenco dei file con tutte le 
		/// informazioni necessarie a corollario
		/// Compone il nome dello zip ed esegue lo zip con i vari file .pdf e il .seq
		///</summary>
		//--------------------------------------------------------------------------------
		private bool CreateZipEnvelope(List<SOSEnvelopeElement> envElements, out string zipFilePath)
		{
			zipFilePath = GetZipEnvelopePath(DocClassName);
			if (string.IsNullOrWhiteSpace(zipFilePath))
				return false;

			string evidenzePath = string.Empty;

			try
			{
				// costruisco la stringa con le chiavi da inserire nel file evidenze.seq
				StringBuilder evidenzeStr = new StringBuilder();
				foreach (SOSEnvelopeElement elem in envElements)
				{
					evidenzeStr.Append(elem.Keys);
					evidenzeStr.Append(Environment.NewLine);
				}

				evidenzePath = Path.Combine(this.tempPath, "evidenze.seq");

				// scrivo il file evidenze.seq (se ne esiste uno con lo stesso nome lo sovrascrivo)
				using (StreamWriter sw = new StreamWriter(evidenzePath, false))
					sw.Write(evidenzeStr.ToString());
			}
			catch (Exception e)
			{
				SOSLogWriter.WriteLogEntry(companyName, string.Format("Error writing file {0})", evidenzePath), "CreateZipEnvelope", e.ToString());
				return false;
			}

			try
			{
				// se il file esiste lo elimino
				if (File.Exists(zipFilePath))
				{
					FileInfo fi = new FileInfo(zipFilePath);
					fi.IsReadOnly = false;
					fi.Delete();
				}

				SOSLogWriter.WriteLogEntry(companyName, "CREATING ENVELOPE ZIP " + zipFilePath);

				// crea il file zip (attenzione: il file non deve esistere!)
				using (ZipFile zip = new ZipFile(zipFilePath))
				{
					// aggiungo tutti i pdf
					foreach (SOSEnvelopeElement elem in envElements)
					{
						zip.AddFile(elem.PdfaTempPath, string.Empty); // Il 2o param è string.Empty per evitare che venga replicato il path del file nello zip
						SOSLogWriter.AppendText(companyName, "Adding file: " + Path.GetFileName(elem.PdfaTempPath));
					}

					zip.AddFile(evidenzePath, string.Empty); // alla fine aggiungo il file .seq
					SOSLogWriter.AppendText(companyName, "Adding file: " + Path.GetFileName(evidenzePath));
					SOSLogWriter.AppendText(companyName, string.Format("Start save zip file ({0})", DateTime.Now));
					zip.Save(); // salvo il file zip
					SOSLogWriter.AppendText(companyName, string.Format("End save zip file ({0})", DateTime.Now));
				}
			}
			catch (ZipException ze)
			{
				zipFilePath = string.Empty;
				SOSLogWriter.WriteLogEntry(companyName, string.Format("Error creating zip file {0})", zipFilePath), "CreateZipEnvelope", ze.ToString());
				return false;
			}
			catch (Exception e)
			{
				zipFilePath = string.Empty;
				SOSLogWriter.WriteLogEntry(companyName, string.Format("Error creating zip file {0})", zipFilePath), "CreateZipEnvelope", e.ToString());
				return false;
			}

			return true;
		}

		///<summary>
		/// Ritorna il path del file di envelope
		/// "CodiceClasseDocumentale@Data@Ora@CodiceConservatore@CodiceCliente.zip"
		///</summary>
		//--------------------------------------------------------------------------------
		private string GetZipEnvelopePath(string docClassName)
		{
			string fileName = string.Empty;
			try
			{
				string file = String.Format
					(
					"{0}@{1}@{2}@{3}@{4}.zip",
					docClassName,
					DateTime.Now.ToString("yyyyMMdd"),
					DateTime.Now.ToString("HHmmss"), // HH: The hour, using a 24-hour clock from 00 to 23.
					SOSConfigurationState.SOSConfiguration.KeeperCode,
					SOSConfigurationState.SOSConfiguration.MySOSUser
					);

				fileName = Path.Combine(this.tempPath, file);
			}
			catch (Exception e)
			{
				SOSLogWriter.WriteLogEntry(companyName, string.Format("Error GetZipEnvelopePath({0}) for file {1})", docClassName, fileName), "GetZipEnvelopePath", e.ToString());
				return string.Empty;
			}

			return fileName;
		}

		///<summary>
		/// Completamento del flusso per la gestione FTP
		/// Devo spostare lo zip nel folder condiviso
		/// Inserire la riga nella DMS_SOSEnvelope con apposito stato
		/// Vado ad aggiornare tutti i SOSDocument correlati con relativo stato a seconda dell'avvenuta copia del file
		///</summary>
		//--------------------------------------------------------------------------------
		private bool MoveToFTPSharedFolder(List<SOSEnvelopeElement> envElements, string zipFilePath, int collectionId)
		{
			bool moveResult = true;

			try
			{
				File.Move(zipFilePath, Path.Combine(SOSConfigurationState.SOSConfiguration.FTPSharedFolder, Path.GetFileName(zipFilePath)));
				SOSLogWriter.WriteLogEntry(companyName, string.Format("Moved zip file {0} to FTP shared folder", Path.GetFileName(zipFilePath)));
			}
			catch (Exception ex)
			{
				// se non riesco a spostare lo zip devo inserire l'envelope con uno stato di errore ed i relativi documenti correlati da reinviare
				SOSLogWriter.WriteLogEntry(companyName, string.Format("Error moving zip file {0} to FTP shared folder", Path.GetFileName(zipFilePath)), "MoveToFTPSharedFolder", ex.ToString());
				moveResult = false;
			}

			// creo in ogni caso il record nella tabella DMS_SOSEnvelope
			DMS_SOSEnvelope newEnvelope = new DMS_SOSEnvelope();
			try
			{
				newEnvelope.CollectionID = collectionId;
				newEnvelope.DispatchCode = Path.GetFileNameWithoutExtension(zipFilePath);
				newEnvelope.DispatchStatus = moveResult ? (int)StatoSpedizioneEnum.SPDOP : (int)StatoSpedizioneEnum.SPDALLKO; // se NON riesco a spostare lo zip metto lo stato di errore
				newEnvelope.DispatchDate = (DateTime)SqlDateTime.MinValue;
				newEnvelope.SynchronizedDate = DateTime.Now;
				newEnvelope.LoginId = loginId;
				newEnvelope.SendingType = (int)SendingType.FTP;
				dmsModel.DMS_SOSEnvelopes.InsertOnSubmit(newEnvelope);
				dmsModel.SubmitChanges();

				SOSLogWriter.WriteLogEntry(companyName, string.Format("Created new SOSEnvelope for FTP, with ID = {0}, DispatchCode = {1}, DispatchStatus = {2}",
						newEnvelope.EnvelopeID.ToString(), newEnvelope.DispatchCode, newEnvelope.DispatchStatus.ToString()));
			}
			catch (Exception ex)
			{
				SOSLogWriter.WriteLogEntry(companyName, "Error occurred creating new SOSEnvelope", "MoveToFTPSharedFolder", ex.ToString());
				moveResult = false;
			}

			using (SqlConnection eaConnection = new SqlConnection(dmsConnectionString))
			{
				eaConnection.Open();

				try
				{
					using (SqlCommand myCommand = eaConnection.CreateCommand())
					{
						StatoDocumento sosStatus = (moveResult) ? StatoDocumento.SENT : StatoDocumento.TORESEND;

						// vado ad aggiornare i vari SOSDocument in modo da agganciarli all'envelope: imposto il DocumentStatus e l'EnvelopeID
						foreach (SOSEnvelopeElement elem in envElements)
						{
							// poi vado ad aggiornare l'EnvelopeId e il DocumentStatus
							myCommand.CommandText = string.Format
								(
									@"UPDATE [DMS_SOSDocument] SET [EnvelopeID] = @envelopeId, [DocumentStatus] = @docStatus WHERE [AttachmentID] = {0}",
									elem.AttachmentId.ToString()
								);
							myCommand.Parameters.AddWithValue("@envelopeId", newEnvelope.EnvelopeID);
							myCommand.Parameters.AddWithValue("@docStatus", sosStatus); // se NON riesco a spostare lo zip metto il doc da reinviare
							myCommand.ExecuteNonQuery();
							myCommand.Parameters.Clear();
							SOSLogWriter.AppendText(companyName, string.Format("Updating SOSDocument for FTP with attachmentId = {0} set DocumentStatus = {1}", elem.AttachmentId.ToString(), sosStatus.ToString()));
						}
					}
				}
				catch (Exception ex)
				{
					SOSLogWriter.WriteLogEntry(companyName, "Error during update SOSDocument for FTP", "MoveToFTPSharedFolder", ex.ToString());
					moveResult = false;
				}
			}

			return moveResult;
		}
	}

	#region Classi di appoggio 
	///<summary>
	/// Elenco informazioni per andare poi a scrivere nel file evidenze.seq i dati delle
	/// chiavi e il nome del file
	///</summary>
	//================================================================================
	public class SOSEnvelopeElement
	{
		private int attachmentId = -1;
		private string pdfaTempPath = string.Empty;
		private string keys = string.Empty;

		//--------------------------------------------------------------------------------
		public int AttachmentId { get { return attachmentId; } }
		//--------------------------------------------------------------------------------
		public string PdfaTempPath { get { return pdfaTempPath; } }
		//--------------------------------------------------------------------------------
		public string Keys { get { return keys; } }

		//--------------------------------------------------------------------------------
		public SOSEnvelopeElement(int attachmentId, string pdfaTempPath, string keys)
		{
			this.attachmentId = attachmentId;
			this.pdfaTempPath = pdfaTempPath;
			this.keys = keys;
		}
	}

	///<summary>
	/// Struttura per raggruppare per ogni zip i suoi elementi
	///</summary>
	//================================================================================
	public class SOSZipElement
	{
		public string ZipFilePath { get; private set; }
		public List<SOSEnvelopeElement> Elements = new List<SOSEnvelopeElement>();

		//--------------------------------------------------------------------------------
		public SOSZipElement(string zipFilePath)
		{
			ZipFilePath = zipFilePath;
		}
	}
	#endregion
}
