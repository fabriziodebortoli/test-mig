using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

using Microarea.EasyAttachment.Core;
using Microarea.EasyAttachment.Properties;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Core.WebServicesWrapper;
using Microarea.Framework.TBApplicationWrapper;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.EasyAttachment.Components
{
	//================================================================================
	public class Utils
	{
		public string CompanyName { get; private set; }

		//--------------------------------------------------------------------------------
		public Utils(string compName)
		{
			this.CompanyName = compName;
		}

		//--------------------------------------------------------------------------------
		public static void SetHandlers(Control control, MouseEventHandler mouseDownEventHandler, MouseEventHandler mouseUpEventHandler)
		{
			control.MouseDown -= mouseDownEventHandler;
			control.MouseUp -= mouseUpEventHandler;

			control.MouseDown += mouseDownEventHandler;
			control.MouseUp += mouseUpEventHandler;

			foreach (Control childControl in control.Controls)
				SetHandlers(childControl, mouseDownEventHandler, mouseUpEventHandler);
		}

		//--------------------------------------------------------------------------------
		public static void OpenERPDocument(string tbDocNamespace, string documentKey)
		{
			MDocument tbDoc = MDocument.Create<MDocument>(tbDocNamespace);
			//if (tbDoc != null)
			//{
			//    tbDoc.BrowseRecord(documentKey);
			//    tbDoc.PostMessageUM(ExternalAPI.WM_COMMAND, (IntPtr)TaskBuilderCommands.OpenDms, IntPtr.Zero);
			//}
		}
		//--------------------------------------------------------------------------------
		public string GetArchiveDocTempFileName(AttachmentInfo attach)
		{
			return GetArchiveDocTempFileName(GetTempFileName(attach));
		}

		//--------------------------------------------------------------------------------
		public string GetArchiveDocTempFileName(DMS_ArchivedDocument doc)
		{
			return GetArchiveDocTempFileName(GetTempFileName(doc));
		}

		//--------------------------------------------------------------------------------
		/// <summary>
		/// restituisce il path del file dal nome dato dentro la temp di easy attachment, se il path è troppo lungo accocia il nome del file
		/// //NON è previsto che il nome della cartella temp sia più lunga del dovuto perchè tale nome è sotto il nostro controllo
		/// </summary>
		public string GetArchiveDocTempFileName(string fileName)
		{
			// creo (se non esiste) un'apposita cartella nella directory Temp
			string temp = GetEasyAttachmentTempPath();

			//se in questo punto i file venisse ad avere una lunghezza di path eccessiva, bisogna correggere, altrimenti siamo proni ad errori anche bloccanti.
			return Path.Combine(temp, ShrinkPath(temp, fileName));
		}

		//--------------------------------------------------------------------------------
		/// <summary>
		/// Accorcia il path temporaneo dell'attachment perchè se troppo lungo non si riesce a generare e ciò è causa di errori
		/// </summary>
		public static string ShrinkPath(string path, string fileName)
		{
			if ((fileName.Length + path.Length + 1) < 260)
				return fileName;

			string filenameWoE = fileName.Substring(0, fileName.LastIndexOf("."));
			string extension = fileName.Substring(fileName.LastIndexOf("."));
			int dirl = path.Length + 1;
			string postfix = string.Empty;//_(Id blabla
			string suffix = filenameWoE;
			int ind = filenameWoE.LastIndexOf(Utils.Postfix);
			if (ind > -1)
			{
				suffix = filenameWoE.Substring(0, ind);
				postfix = filenameWoE.Substring(ind);
			}
			int maxFileNameLen = 259 - dirl - extension.Length - postfix.Length;
			return suffix.Substring(0, maxFileNameLen) + postfix + extension;
		}

		//--------------------------------------------------------------------------------
		public static void RemoveHandlers(Control control, MouseEventHandler mouseDownEventHandler, MouseEventHandler mouseUpEventHandler)
		{
			control.MouseDown -= mouseDownEventHandler;
			control.MouseUp -= mouseUpEventHandler;

			foreach (Control childControl in control.Controls)
				RemoveHandlers(childControl, mouseDownEventHandler, mouseUpEventHandler);
		}

		//--------------------------------------------------------------------------------
		public static Point GetAbsolute(Point point, Control sourceControl, Control rootControl)
		{
			Point tempPoint = new Point();
			for (Control iterator = sourceControl; iterator != rootControl; iterator = iterator.Parent)
				tempPoint.Offset(iterator.Left, iterator.Top);

			tempPoint.Offset(point.X, point.Y);
			return tempPoint;
		}

		///<summary>
		/// Salva un temporaneo e lo trasforma in PDFA (solo per file pdf, tif/tiff, doc/docx, xls/xlsx)
		///</summary>
		//---------------------------------------------------------------------
		public string TransformToPdfA(AttachmentInfo currentAttach, string tempFileName = "", bool isSOSDocument = false)
		{
			if (currentAttach != null && SaveAttachmentFile(currentAttach, tempFileName))
			{
				if (isSOSDocument)
					return CoreUtils.TransformToPdfA(tempFileName, GetSosConnectorTempPath());
				else
					return CoreUtils.TransformToPdfA(currentAttach.TempPath, GetEasyAttachmentTempPath());
			}
			else
				return string.Empty;
		}

		///<summary>
		/// Restituisce la stringa con la spiegazione estesa dello stato della spedizione (localizzabile)
		///</summary>
		//----------------------------------------------------------------------------
		public static string GetDispatchStatusDescription(StatoSpedizioneEnum stato)
		{
			switch (stato)
			{
				case StatoSpedizioneEnum.SPDNOP:
					return Strings.SpdNopDispatchStatus; // non preso in carico, l'archivio dovrà essere rispedito
				case StatoSpedizioneEnum.SPDEXIST:
					return Strings.SpdExistsDispatchStatus; // il file è una copia di un archivio già spedito
				case StatoSpedizioneEnum.SPDNEX:
					return Strings.SpdNexDispatchStatus; // la spedizione indicata non esiste
				case StatoSpedizioneEnum.SPDALLOK:
					return Strings.SpdAllOkDispatchStatus; // il contenuto della spedizione è stato acquisito
				case StatoSpedizioneEnum.SPDALLKO:
					return Strings.SpdAllKoDispatchStatus; // il contenuto della spedizione non è stato acquisito, deve essere rivisto e spedito nuovamente
				case StatoSpedizioneEnum.SPDOKWERR:
					return Strings.SpdOkWErrDispatchStatus; // il contenuto della spedizione è stato acquisito parzialmente, alcuni doc presentano errori, è necessario correggere i doc errati e rispedirli
				case StatoSpedizioneEnum.SPDSUBUNAUTH:
					return Strings.SpdSubUnauthDispatchStatus; // il Soggetto in questione non è autorizzato a recuperare i dati
				case StatoSpedizioneEnum.SPDCONUNEX:
					return Strings.SpdConUnexDispatchStatus;  // il Conservatore non è definito a Sistema
				case StatoSpedizioneEnum.SPDCUSTUNEX:
					return Strings.SpdCustUnexDispatchStatus; // il Cliente del Conservatore non è definito a Sistema
				case StatoSpedizioneEnum.SPDOP:
				default:
					return Strings.SpdOpDispatchStatus;  // preso in carico, in attesa di essere lavorato
			}
		}

		///<summary>
		/// Restituisce la stringa con la spiegazione estesa dello stato del documento (localizzabile)
		///</summary>
		//----------------------------------------------------------------------------
		public static string GetDocumentStatusDescription(StatoDocumento stato)
		{
			switch (stato)
			{
				case StatoDocumento.SENT:
					return Strings.DocSentDocumentStatus;
				case StatoDocumento.TORESEND:
					return Strings.DocToResendDocumentStatus;
				case StatoDocumento.DOCTEMP:
					return Strings.DocTempDocumentStatus; // il documento e' stato importato nel db della SOS ma non è ancora entrato nel giro della SOS
				case StatoDocumento.DOCSTD:
					return Strings.DocStdDocumentStatus;// il cliente dal sito ha apposto lo firma
				case StatoDocumento.DOCRDY:
					return Strings.DocRdyDocumentStatus;// il documento è stato spostato in un lotto
				case StatoDocumento.DOCSIGN:
					return Strings.DocSignDocumentStatus;// il lotto è stato chiuso e il doc inviato in conservazione 
				case StatoDocumento.DOCKO:
					return Strings.DocKODocumentStatus;// non acquisito
				case StatoDocumento.DOCREP:
					return Strings.DocRepDocumentStatus; ;// sostituito da un altro documento
				case StatoDocumento.IDLE:
					return Strings.DocIdleDocumentStatus; // documento appena allegato
				case StatoDocumento.TOSEND:
					return Strings.DocToSendDocumentStatus; // impostato dal SOSMonitor
				case StatoDocumento.WAITING:
				default:
					return Strings.DocWaitingDocumentStatus;
			}
		}

		///<summary>
		/// Restituisce l'immagine relativa allo stato del documento
		///</summary>
		//----------------------------------------------------------------------------
		public static Bitmap GetDocumentStatusImage(StatoDocumento stato)
		{
			switch (stato)
			{
				case StatoDocumento.SENT:
					return Microarea.EasyAttachment.Properties.Resources.NEW_DocSent16x16;
				case StatoDocumento.TORESEND:
					return Microarea.EasyAttachment.Properties.Resources.NEW_DocToResend16x16;
				case StatoDocumento.DOCTEMP:
					return Microarea.EasyAttachment.Properties.Resources.NEW_DocTemp16x16; // stato provvisorio: il documento e' stato importato nel db della SOS ma non è ancora entrato nel giro della SOS. 
				case StatoDocumento.DOCSTD:
					return Microarea.EasyAttachment.Properties.Resources.NEW_DocStd16x16; // questo stato si ottiene DOPO che il cliente dal sito appone lo firma
				case StatoDocumento.DOCRDY:
					return Microarea.EasyAttachment.Properties.Resources.New_DocRdy16x16; // il documento è stato spostato in un lotto. 
				case StatoDocumento.DOCSIGN:
					return Microarea.EasyAttachment.Properties.Resources.NEW_DocSign16x16; // il lotto è stato chiuso e il doc inviato in conservazione (non e' piu' modificabile)
				case StatoDocumento.DOCKO:
					return Microarea.EasyAttachment.Properties.Resources.NEW_DocKo16x16; // non acquisito
				case StatoDocumento.TOSEND:
					return Microarea.EasyAttachment.Properties.Resources.NEW_DocToSend16x16;
				case StatoDocumento.IDLE:
				case StatoDocumento.WAITING:
				default:
					return Microarea.EasyAttachment.Properties.Resources.NEW_DocIdle16x16;
			}
		}

		#region Funzioni per la gestione dei temporanei (occhio non sono statiche)


		//---------------------------------------------------------------------
		public string GetOldEasyAttachmentTempPath()
		{
			// old folder: C:\Users\USERNAME\AppData\Local\Temp\TBTemp\COMPUTERNAME\INSTALLATIONNAME\TbAppManager\DMSTemp
			return Path.Combine(BasePathFinder.BasePathFinderInstance.GetAppDataPath(true), "DMSTemp");
		}

		/// <summary>
		/// Return the user's temporary DMS path 
		/// </summary>
		/// <returns> the user's temporary DMS path as C:\Users\USERNAME\AppData\Local\Temp\TBTemp\COMPUTERNAME\INSTALLATIONNAME\Mago.Net\DMS\COMPANYNAME</returns>
		//---------------------------------------------------------------------
		public string GetEasyAttachmentTempPath()
		{
			//Impr. #3210
			string oldTemp = GetOldEasyAttachmentTempPath();
			string newTemp = Path.Combine(BasePathFinder.BasePathFinderInstance.GetAppDataPath(true), "DMS", CompanyName);
			if (!Directory.Exists(newTemp))
			{
				Directory.CreateDirectory(newTemp);

				//Se esiste ancora la vecchia directory temporanea la cancello spostando prima i file con readonly = FALSE (sono quelli in out dall'utente)
				DirectoryInfo oldTempPath = new DirectoryInfo(oldTemp);
				if (oldTempPath.Exists)
				{
					foreach (FileInfo file in oldTempPath.EnumerateFiles())
					{
						if (file.IsReadOnly)
							file.IsReadOnly = false;
						else
							file.MoveTo(Path.Combine(newTemp, file.Name));
					}
					oldTempPath.Delete(true);
				}
			}
			return newTemp;
		}

		//---------------------------------------------------------------------
		public string GetSosConnectorTempPath()
		{
			//Impr. #3210
			string oldTemp = Path.Combine(GetOldEasyAttachmentTempPath(), "SOSConnector");
			string newTemp = Path.Combine(BasePathFinder.BasePathFinderInstance.GetAppDataPath(true), "SOSConnector", CompanyName);
			if (!Directory.Exists(newTemp))
			{
				Directory.CreateDirectory(newTemp);

				//Se esiste ancora la vecchia directory temporanea la cancello spostando prima i file nella nuova
				DirectoryInfo oldTempPath = new DirectoryInfo(oldTemp);
				if (oldTempPath.Exists)
				{
					foreach (FileInfo file in oldTempPath.EnumerateFiles())
						file.MoveTo(Path.Combine(newTemp, file.Name));
					oldTempPath.Delete(true);
				}
			}

			return newTemp;
		}

		///<summary>
		/// Ritorna il path del file di log odierno con le info delle spedizioni se esiste
		/// salvato nella Custom (C:\nome_istanza\Custom\Companies\nome_company\Log\EasyAttachmentSync). 
		/// Altrimenti torna il path del folder
		///</summary>
		//---------------------------------------------------------------------
		public string GetSosConnectorLogFilePath()
		{
			string sosLogDirectoryPath = Path.Combine(BasePathFinder.BasePathFinderInstance.GetCustomCompanyLogPath(CompanyName), NameSolverStrings.EasyAttachmentSync);
			string dailySosLogFilePath = Path.Combine(sosLogDirectoryPath, string.Format("SOSConnector_{0}.txt", DateTime.Now.ToString("yyyy-MM-dd")));

			if (File.Exists(dailySosLogFilePath))
				return dailySosLogFilePath;

			return sosLogDirectoryPath;
		}

		//---------------------------------------------------------------------
		public string GetDocToArchiveTempPath()
		{
			string temp = Path.Combine(GetEasyAttachmentTempPath(), "DocToArchive");
			if (!Directory.Exists(temp))
				Directory.CreateDirectory(temp);
			return temp;
		}

		//---------------------------------------------------------------------
		public string DeleteEasyAttachmentTempPath()
		{
			string temp = GetEasyAttachmentTempPath();
			if (Directory.Exists(temp))
				Directory.Delete(temp, true);
			return temp;
		}

		//---------------------------------------------------------------------
		private static string GetTempFileName(AttachmentInfo att)
		{
			return GetTempFileName(att.Name, att.ArchivedDocId);
		}

		//---------------------------------------------------------------------
		public static string GetTempFileName(DMS_ArchivedDocument doc)
		{
			return GetTempFileName(Path.Combine(doc.Path, doc.Name), doc.ArchivedDocID);
		}

		public static string Postfix = "_(Id-";
		//---------------------------------------------------------------------
		private static string GetTempFileName(string completeOriginalPath, int id)
		{
			return String.Format
					("{0}{1}{2}){3}",
					Path.GetFileNameWithoutExtension(completeOriginalPath),
					Postfix,
					id,
					Path.GetExtension(completeOriginalPath));
		}

		///in fase di chiusura dell'applicaizone vengono cancellati dalla cartella C:\Users\Bauzone\AppData\Local\Temp\Microarea\COMPUTERNAME\ISTALLATIONNAME\Mago.Net\DMS 
		/// i file temporanei generati dal DMS
		/// Non cancello quelli con stato ReadOnly = false perchè in checkout
		//---------------------------------------------------------------------
		internal void RemoveTemporaryFiles()
		{
			string temp = GetEasyAttachmentTempPath();
			DirectoryInfo tempPath = new DirectoryInfo(temp);
			if (tempPath.Exists)
				foreach (FileInfo file in tempPath.EnumerateFiles("*.*").Where(f => f.IsReadOnly == true))
					DeleteFile(file);
		}

		//---------------------------------------------------------------------
		public static void DeleteFile(FileInfo fileInfo)
		{
			try
			{
				//  elimino il file togliendo l'eventuale flag readonly
				fileInfo.IsReadOnly = false;
				fileInfo.Delete();
			}
			catch 
			{
			}
		}

		//---------------------------------------------------------------------
		public string SaveAttachmentFileInFolder(AttachmentInfo att, string targetFolder)
		{
			if (att == null || string.IsNullOrEmpty(targetFolder) || !Directory.Exists(targetFolder))
				return string.Empty;

			try
			{
				string fileName = Path.Combine(targetFolder, ShrinkPath(targetFolder, GetTempFileName(att)));
				return (SaveAttachmentFile(att, fileName)) ? fileName : string.Empty;
			}

			catch (Exception)
			{
				return string.Empty;
			}

		}

		//salvo il file temporaneo relativo all'attachment, con l'indicazione dell'archivedocID 
		//e verifico però le date in modo da salvare il più recente
		//---------------------------------------------------------------------
		public bool SaveAttachmentFile(AttachmentInfo att, string tempFileName = null)
		{
			if (att == null)
				return false;

			try
			{
				string temp = !string.IsNullOrEmpty(tempFileName) ? tempFileName : string.IsNullOrEmpty(att.TempPath) ? GetArchiveDocTempFileName(att) : att.TempPath;
				FileInfo f = new FileInfo(temp);

				// se il file non esiste lo creo
				// poniamo il caso che io abbia fatto add di un documento già esistente ma in una sua versione piè recente, 
				// adesso att è rimasto quello vecchio perchè nessuno gli ha detto che io ho deciso di sovrascrivere il documento 
				// già archiviato con la sua versione più recente, quindi io continuo a vedere quello vecchio perchè lastwritetimeutc è rimasta vecchia.
				// !!!
				if (!f.Exists || (att.LastWriteTimeUtc > f.LastWriteTimeUtc))//verifica date per salvare il più recente (STESSO ARCHIVEDDOCID)
				{
					if (att.DocContent == null && att.VeryLargeFile)
					{
						if (string.Compare(att.TempPath, temp, true) != 0)
							File.Copy(att.TempPath, temp);
						return true;
					}

					if (att.DocContent == null || att.DocContent.Length == 0)
						return false;

					if (f.Exists)
						f.IsReadOnly = false;

					using (FileStream s = new FileStream(temp, FileMode.Create))
						s.Write(att.DocContent, 0, att.DocContent.Length);

					att.DisposeDocContent(); //alleggerisco la memoria dei byte del content

					f.LastWriteTimeUtc = att.LastWriteTimeUtc;

					if (string.IsNullOrEmpty(att.TempPath) && string.IsNullOrEmpty(tempFileName))
						att.TempPath = temp;
				}

				//imposto readonly, è un file temp e non deve essere modificato è solo per visualizzazione, a meno che non sia checkout
				f.IsReadOnly = (att.ModifierID == -1);

				f.Attributes = f.Attributes & ~FileAttributes.Hidden;
				f.Attributes = f.Attributes & ~FileAttributes.System;
				f.Refresh();

				return true;
			}
			catch (Exception exc)
			{
				throw (exc);
			}
		}



		#endregion

		#region Funzioni per le bitmap e estensioni file
		/// <summary>
		/// return the bitmap (16x16) of the file extensions type 
		/// </summary>
		/// <param name="extType">accetta estensione con o senza punto</param>
		//---------------------------------------------------------------------
		public static Bitmap GetSmallImage(string extType)
		{
			return GetImage(extType, true);
		}

		/// <summary>
		/// return the bitmap (64x64) of the file extensions type 
		/// </summary>
		/// <param name="extType">accetta estensione con o senza punto</param>
		//---------------------------------------------------------------------
		public static Bitmap GetMediumImage(string extType)
		{
			return GetImage(extType, false);
		}

		/// <summary>
		/// return the bitmap of the file extensions type 
		/// </summary>
		/// <param name="extType">accetta estensione con o senza punto</param>
		/// <param name="small">ritorna immagine 16x16 oppure 64x64</param>
		//---------------------------------------------------------------------
		public static Bitmap GetImage(string extType, bool small)
		{
			string extension = extType.Trim(new char[] { '.' });

			switch (extension.ToLowerInvariant())
			{
				case FileExtensions.Pdf:
					return small ? Resources.Ext_PDF16x16 : Resources.Ext_PDF64x64;
				case FileExtensions.Bmp:
					return small ? Resources.Ext_BMP16x16 : Resources.Ext_BMP64x64;
				case FileExtensions.Doc:
				case FileExtensions.Docx:
					return small ? Resources.Ext_DOC16x16 : Resources.Ext_DOC64x64;
				case FileExtensions.Ppt:
				case FileExtensions.Pptx:
					return small ? Resources.Ext_PPT16x16 : Resources.Ext_PPT64x64;
				case FileExtensions.Gif:
					return small ? Resources.Ext_GIF16x16 : Resources.Ext_GIF64x64;
				case FileExtensions.Gzip:
					return small ? Resources.Ext_GZIP16x16 : Resources.Ext_GZIP64x64;
				case FileExtensions.Html:
				case FileExtensions.Htm:
					return small ? Resources.Ext_HTML16x16 : Resources.Ext_HTML64x64;
				case FileExtensions.Jpg:
				case FileExtensions.Jpeg:
					return small ? Resources.Ext_JPG16x16 : Resources.Ext_JPG64x64;
				case FileExtensions.Tif:
				case FileExtensions.Tiff:
					return small ? Resources.Ext_TIFF16x16 : Resources.Ext_TIFF64x64;
				case FileExtensions.Txt:
				case FileExtensions.Config:
					return small ? Resources.Ext_TXT16x16 : Resources.Ext_TXT64x64;
				case FileExtensions.Png:
					return small ? Resources.Ext_PNG16x16 : Resources.Ext_PNG64x64;
				case FileExtensions.Rar:
					return small ? Resources.Ext_RAR16x16 : Resources.Ext_RAR64x64;
				case FileExtensions.Xml:
					return small ? Resources.EXT_XML16x16 : Resources.EXT_XML64x64;
				case FileExtensions.Xls:
				case FileExtensions.Xlsx:
					return small ? Resources.Ext_XLS16x16 : Resources.Ext_XLS64x64;
				case FileExtensions.Zip:
				case FileExtensions.Zip7z:
					return small ? Resources.Ext_ZIP16x16 : Resources.Ext_ZIP64x64;
				case FileExtensions.Wmv:
					return small ? Resources.Ext_WMV16x16 : Resources.Ext_WMV64x64;
				case FileExtensions.Mpeg:
					return small ? Resources.Ext_MPEG16x16 : Resources.Ext_MPEG64x64;
				case FileExtensions.Avi:
					return small ? Resources.Ext_AVI16x16 : Resources.Ext_AVI64x64;
				case FileExtensions.Wav:
					return small ? Resources.Ext_WAV16x16 : Resources.Ext_WAV64x64;
				case FileExtensions.Mp3:
					return small ? Resources.Ext_MP316x16 : Resources.Ext_MP364x64;
				case FileExtensions.Msg:
					return small ? Resources.Ext_MAIL16x16 : Resources.Ext_MAIL64x64;
				case FileExtensions.Rtf:
					return small ? Resources.Ext_RTF16x16 : Resources.Ext_RTF64x64;
				case ("all"):
					return small ? Resources.Category16x16 : Resources.Category64x64;
				case FileExtensions.Papery: // si tratta dei documenti cartacei
					return small ? Resources.PaperAirplane16x16 : Resources.PaperAirplane64x64;
				default:
					return small ? Resources.Ext_Default16x16 : Resources.Ext_Default64x64;
			}
		}

		//---------------------------------------------------------------------
		public static IList<string> GetExtensionsForScan()
		{
			IList<string> extensions = new List<string>();
			extensions.Add(FileExtensions.Bmp.ToUpperInvariant());
			extensions.Add(FileExtensions.Gif.ToUpperInvariant());
			extensions.Add(FileExtensions.Jpeg.ToUpperInvariant());
			extensions.Add(FileExtensions.Pdf.ToUpperInvariant());
			extensions.Add(FileExtensions.Png.ToUpperInvariant());
			extensions.Add(FileExtensions.Tiff.ToUpperInvariant());
			return extensions;
		}

		///<summary>
		/// Generazione nome file temporaneo
		/// esempio: EAScan_20110509_4857
		///</summary>
		//--------------------------------------------------------------------------------
		public static string GetFileNameToScan()
		{
			Random r = new Random();
			return CommonStrings.EAScanPrefix + DateTime.Now.ToString("yyyyMMdd") + "_" + r.Next().ToString().Substring(0, 4);
		}
		#endregion

		#region Funzioni per individuare il range di date per la ricerca avanzata
		//--------------------------------------------------------------------------------
		public enum DateRangeType
		{
			ALL_DATES = 1,
			YESTERDAY = 2,
			CURRENT_WEEK = 3,
			LAST_WEEK = 4,
			CURRENT_MONTH = 5,
			LAST_MONTH = 6,
			CURRENT_QUARTER = 7,
			LAST_QUARTER = 8,
			CURRENT_SEMESTER = 9,
			LAST_SEMESTER = 10,
			CURRENT_YEAR = 11,
			LAST_YEAR = 12
		}

		///<summary>
		/// Ritorna la data di ieri
		///</summary>
		//--------------------------------------------------------------------------------
		public static DateTime GetYesterday()
		{
			return DateTime.Today.AddDays(-1);
		}

		#region Weeks
		///<summary>
		/// Ritorna la data del giorno di inizio della settimana corrente
		/// Deve essere richiamata specificando il giorno di inizio della settimana
		/// DateTime dt = DateTime.Now.StartOfWeek(DayOfWeek.Monday);
		///</summary>
		//--------------------------------------------------------------------------------
		public static DateTime GetFirstDayOfCurrentWeek(DayOfWeek startOfWeek)
		{
			int diff = DateTime.Now.DayOfWeek - startOfWeek;
			if (diff < 0)
				diff += 7;

			return DateTime.Now.AddDays(-1 * diff).Date;
		}

		///<summary>
		/// Ritorna la data del giorno di inizio e di fine della settimana precedente alla corrente
		/// Finding Start and End Dates of Previous Week
		///</summary>
		//--------------------------------------------------------------------------------
		public static void GetDatesOfLastWeek(ref DateTime startDate, ref DateTime endDate)
		{
			double offset = 0;
			switch (DateTime.Today.DayOfWeek)
			{
				case DayOfWeek.Monday:
					offset = 0;
					break;
				case DayOfWeek.Tuesday:
					offset = -1;
					break;
				case DayOfWeek.Wednesday:
					offset = -2;
					break;
				case DayOfWeek.Thursday:
					offset = -3;
					break;
				case DayOfWeek.Friday:
					offset = -4;
					break;
				case DayOfWeek.Saturday:
					offset = -5;
					break;
				case DayOfWeek.Sunday:
					offset = -6;
					break;
			}

			endDate = System.DateTime.Today.AddDays(offset);
			startDate = System.DateTime.Today.AddDays(-7 + offset);
		}
		#endregion

		#region Months
		///<summary>
		/// Ritorna la data del primo giorno del mese corrente
		///</summary>
		//--------------------------------------------------------------------------------
		public static DateTime GetFirstDayOfCurrentMonth()
		{
			return new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1, 0, 0, 0, 0);
		}

		///<summary>
		/// Ritorna la data del primo giorno del mese precedente al corrente
		///</summary>
		//--------------------------------------------------------------------------------
		public static DateTime GetFirstDayOfLastMonth()
		{
			return new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1, 0, 0, 0, 0).AddMonths(-1);
		}

		/// <summary>
		/// Ritorna l'ultimo giorno dell'anno precedente al corrente
		/// </summary>
		//--------------------------------------------------------------------------------
		public static DateTime GetLastDayOfLastMonth()
		{
			return new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1, 23, 59, 59, 999).AddDays(-1);
		}
		#endregion

		#region Years
		/// <summary>
		/// Ritorna il primo giorno dell'anno corrente 
		/// </summary>
		//--------------------------------------------------------------------------------
		public static DateTime GetFirstDayOfCurrentYear()
		{
			return new DateTime(DateTime.Today.Year, 1, 1, 0, 0, 0, 0);
		}

		/// <summary>
		/// Ritorna il primo giorno dell'anno precedente al corrente 
		/// </summary>
		//--------------------------------------------------------------------------------
		public static DateTime GetFirstDayOfLastYear()
		{
			return new DateTime(DateTime.Today.AddYears(-1).Year, 1, 1, 0, 0, 0, 0);
		}

		/// <summary>
		/// Ritorna l'ultimo giorno dell'anno precedente al corrente
		/// </summary>
		//--------------------------------------------------------------------------------
		public static DateTime GetLastDayOfLastYear()
		{
			return new DateTime(DateTime.Today.AddYears(-1).Year, 12, 31, 23, 59, 59, 999);
		}
		#endregion

		#region Quarters
		//--------------------------------------------------------------------------------
		public enum Quarter
		{
			First = 1,
			Second = 2,
			Third = 3,
			Fourth = 4
		}

		//--------------------------------------------------------------------------------
		public enum Month
		{
			January = 1,
			February = 2,
			March = 3,
			April = 4,
			May = 5,
			June = 6,
			July = 7,
			August = 8,
			September = 9,
			October = 10,
			November = 11,
			December = 12
		}

		//--------------------------------------------------------------------------------
		public static DateTime GetStartOfQuarter(int Year, Quarter Qtr)
		{
			if (Qtr == Quarter.First)    // 1st Quarter = January 1 to March 31
				return new DateTime(Year, 1, 1, 0, 0, 0, 0);
			else if (Qtr == Quarter.Second) // 2nd Quarter = April 1 to June 30
				return new DateTime(Year, 4, 1, 0, 0, 0, 0);
			else if (Qtr == Quarter.Third) // 3rd Quarter = July 1 to September 30
				return new DateTime(Year, 7, 1, 0, 0, 0, 0);
			else // 4th Quarter = October 1 to December 31
				return new DateTime(Year, 10, 1, 0, 0, 0, 0);
		}

		//--------------------------------------------------------------------------------
		public static DateTime GetEndOfQuarter(int Year, Quarter Qtr)
		{
			if (Qtr == Quarter.First)    // 1st Quarter = January 1 to March 31
				return new DateTime(Year, 3, DateTime.DaysInMonth(Year, 3), 23, 59, 59, 999);
			else if (Qtr == Quarter.Second) // 2nd Quarter = April 1 to June 30
				return new DateTime(Year, 6, DateTime.DaysInMonth(Year, 6), 23, 59, 59, 999);
			else if (Qtr == Quarter.Third) // 3rd Quarter = July 1 to September 30
				return new DateTime(Year, 9, DateTime.DaysInMonth(Year, 9), 23, 59, 59, 999);
			else // 4th Quarter = October 1 to December 31
				return new DateTime(Year, 12, DateTime.DaysInMonth(Year, 12), 23, 59, 59, 999);
		}

		//--------------------------------------------------------------------------------
		public static Quarter GetQuarter(Month Month)
		{
			if (Month <= Month.March)
				return Quarter.First; // 1st Quarter = January 1 to March 31
			else if ((Month >= Month.April) && (Month <= Month.June))
				return Quarter.Second; // 2nd Quarter = April 1 to June 30
			else if ((Month >= Month.July) && (Month <= Month.September))
				return Quarter.Third; // 3rd Quarter = July 1 to September 30
			else
				return Quarter.Fourth; // 4th Quarter = October 1 to December 31
		}

		//--------------------------------------------------------------------------------
		public static DateTime GetEndOfLastQuarter()
		{
			return ((Month)DateTime.Now.Month <= Month.March)
				? GetEndOfQuarter(DateTime.Now.Year - 1, Quarter.Fourth) //go to last quarter of previous year
				: GetEndOfQuarter(DateTime.Now.Year, GetQuarter((Month)DateTime.Now.Month)); //return last quarter of current year
		}

		//--------------------------------------------------------------------------------
		public static DateTime GetStartOfLastQuarter()
		{
			return ((Month)DateTime.Now.Month <= Month.March)
				? GetStartOfQuarter(DateTime.Now.Year - 1, Quarter.Fourth) //go to last quarter of previous year
				: GetStartOfQuarter(DateTime.Now.Year, GetQuarter((Month)DateTime.Now.Month)); //return last quarter of current year
		}

		//--------------------------------------------------------------------------------
		public static DateTime GetStartOfCurrentQuarter()
		{
			return GetStartOfQuarter(DateTime.Now.Year, GetQuarter((Month)DateTime.Now.Month));
		}

		//--------------------------------------------------------------------------------
		public static DateTime GetEndOfCurrentQuarter()
		{
			return GetEndOfQuarter(DateTime.Now.Year, GetQuarter((Month)DateTime.Now.Month));
		}
		#endregion

		#region Semester
		//--------------------------------------------------------------------------------
		public enum Semester
		{
			First = 1,
			Second = 2
		}

		//--------------------------------------------------------------------------------
		public static Semester GetSemester(int Month)
		{
			return (Month <= 6) ? Semester.First : Semester.Second;
		}

		//--------------------------------------------------------------------------------
		public static DateTime GetStartOfSemester(int Year, Semester Smstr)
		{
			return (Smstr == Semester.First)
				? new DateTime(Year, 1, 1, 0, 0, 0, 0) // 1st Semester = January 1 to June 30
				: new DateTime(Year, 7, 1, 0, 0, 0, 0); // 2nd Semester = July 1 to December 31
		}

		//--------------------------------------------------------------------------------
		public static DateTime GetEndOfSemester(int Year, Semester Smstr)
		{
			return (Smstr == Semester.First)
				? new DateTime(Year, 6, 30, 23, 59, 59, 999) // 1st Semester = January 1 to June 30
				: new DateTime(Year, 12, 31, 23, 59, 59, 999); // 2nd Semester = July 1 to December 31
		}

		//--------------------------------------------------------------------------------
		public static DateTime GetStartOfCurrentSemester()
		{
			return GetStartOfSemester(DateTime.Now.Year, GetSemester(DateTime.Now.Month));
		}

		//--------------------------------------------------------------------------------
		public static DateTime GetEndOfCurrentSemester()
		{
			return GetEndOfSemester(DateTime.Now.Year, GetSemester(DateTime.Now.Month));
		}

		//--------------------------------------------------------------------------------
		public static DateTime GetStartOfLastSemester()
		{
			return (DateTime.Now.Month <= 6)
				? GetStartOfSemester(DateTime.Now.Year - 1, Semester.Second) // go to last semester of previous year
				: GetStartOfSemester(DateTime.Now.Year, GetSemester(DateTime.Now.Month - 6)); //return first semester of current year
		}

		//--------------------------------------------------------------------------------
		public static DateTime GetEndOfLastSemester()
		{
			return (DateTime.Now.Month <= 6)
				? GetEndOfSemester(DateTime.Now.Year - 1, Semester.Second) // go to last semester of previous year
				: GetEndOfSemester(DateTime.Now.Year, GetSemester(DateTime.Now.Month - 6)); //return last semester of current year
		}
		#endregion

		#endregion

		///<summary>
		/// Metodo per controllare se una directory e' accedibile
		/// Prova a creare un file temporaneo e se va in eccezione ritorna false
		///</summary>
		//--------------------------------------------------------------------------------
		public static bool CheckDirectoryAccess(string directory, out string errorMsg)
		{
			errorMsg = string.Empty;

			bool success = false;
			string fullPath = Path.Combine(directory, "tempFile.tmp");

			if (Directory.Exists(directory))
			{
				try
				{
					using (FileStream fs = new FileStream(fullPath, FileMode.CreateNew, FileAccess.Write))
						fs.WriteByte(0xff);

					if (File.Exists(fullPath))
					{
						File.Delete(fullPath);
						success = true;
					}
				}
				catch (Exception ex)
				{
					success = false;
					errorMsg = ex.Message;
				}
			}
			else
				errorMsg = string.Format(string.Format(Strings.PathNotExists, directory));

			return success;
		}

		public static string BarcodePrefix = "EA"; //mezzo tapullo: copio qui il prefisso di default perchè mi serve accedervi da finestre che ignorano l'orchestrator.
	}
}
