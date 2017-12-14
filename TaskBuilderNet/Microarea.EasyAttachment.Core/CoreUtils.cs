using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using Apitron.PDF.Kit;

using Microarea.TaskBuilderNet.Core.CoreTypes;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TBPicComponents;
using Microsoft.Office.Interop.Word;

namespace Microarea.EasyAttachment.Core
{
	//================================================================================
	public class CoreStrings
	{
		public const string SosWSUrl		= "https://infinity.sostitutivazucchetti.it/SOS/services/ws_as_servizio?wsdl";
		public const string SosKeeperCode	= "zucc";
	}

	//================================================================================
	public static class CoreUtils
	{
		///<summary>
		/// Dato il contenuto di un file controlla se si tratta di un file di tipo immagine o meno
		///</summary>
		//---------------------------------------------------------------------
		public static bool HasImageFormat(byte[] bytes)
		{
			var bmp = Encoding.ASCII.GetBytes("BM");		// BMP
			var gif = Encoding.ASCII.GetBytes("GIF");		// GIF
			var png = new byte[] { 137, 80, 78, 71 };		// PNG
			var tiff = new byte[] { 73, 73, 42 };			// TIFF
			var tiff2 = new byte[] { 77, 77, 42 };			// TIFF
			var jpeg = new byte[] { 255, 216, 255, 224 };	// jpeg
			var jpeg2 = new byte[] { 255, 216, 255, 225 };	// jpeg canon

			if (
				bmp.SequenceEqual(bytes.Take(bmp.Length)) ||
				gif.SequenceEqual(bytes.Take(gif.Length)) ||
				png.SequenceEqual(bytes.Take(png.Length)) ||
				tiff.SequenceEqual(bytes.Take(tiff.Length)) ||
				tiff2.SequenceEqual(bytes.Take(tiff2.Length)) ||
				jpeg.SequenceEqual(bytes.Take(jpeg.Length)) ||
				jpeg2.SequenceEqual(bytes.Take(jpeg2.Length))
				)
				return true;

			return false;
		}

		//---------------------------------------------------------------------
		public static bool IsTextCompatible(List<string> textExtensions, string extension)
		{
			if (textExtensions == null || string.IsNullOrWhiteSpace(extension) || !textExtensions.ContainsNoCase(extension))
				return false;

			return true;
		}

		//---------------------------------------------------------------------
		public static bool IsOCRCompatible(List<string> textExtensions, string extension)
		{
			if (textExtensions == null || string.IsNullOrWhiteSpace(extension))
				return false;

			if (IsTextCompatible(textExtensions, extension))
				return true;

			switch (extension.ToLowerInvariant())
			{
				case FileExtensions.DotBmp:
				case FileExtensions.DotGif:
				case FileExtensions.DotJpg:
				case FileExtensions.DotJpeg:
				case FileExtensions.DotPdf:
				case FileExtensions.DotPng:
				case FileExtensions.DotTif:
				case FileExtensions.DotTiff:
					return true;
			}

			return false;
		}

		///<summary>
		/// Calcola l'Hash di un file secondo l'algoritmo SHA256
		///</summary>
		//---------------------------------------------------------------------
		public static string CreateDocumentHash256(string docPath)
		{
			string hashCode = string.Empty;
			if (string.IsNullOrWhiteSpace(docPath))
				return hashCode;

			try
			{
				using (FileStream fs = File.OpenRead(docPath))
				{
					SHA256 sha256 = SHA256.Create();
					if (sha256 == null)
						return hashCode;

					byte[] mHash = sha256.ComputeHash(fs);

					for (int i = 0; i < mHash.Length; i++)
						hashCode += String.Format("{0:X2}", mHash[i]);
					return hashCode;
				}
			}
			catch (Exception)
			{
				return string.Empty;
			}
		}

		/// <summary>
		/// Transform file in PDF/A format (only for pdf, tiff/tif, doc/docx, xls/xlsx)
		/// (To preview a doc or docx file I need to export it to a PDF/A file creating a temp file)
		/// </summary>
		//---------------------------------------------------------------------
		public static string TransformToPdfA(string path, string tempPath)
		{
			bool sourceWasAlreadyPDFA = false;
			return TransformToPdfA(path, tempPath, out sourceWasAlreadyPDFA);
		}

		/// <summary>
		/// Transform file in PDF/A format (only for pdf, tiff/tif, doc/docx, xls/xlsx)
		/// (To preview a doc or docx file I need to export it to a PDF/A file creating a temp file)
		/// </summary>
		//---------------------------------------------------------------------
		public static string TransformToPdfA(string path, string tempPath, out bool sourceWasAlreadyPDFA)
		{
			string pdfAFile = string.Empty;
			sourceWasAlreadyPDFA = false;

			switch (Path.GetExtension(path).ToLowerInvariant())
			{
				case FileExtensions.DotPdf:
					pdfAFile = ConvertPDF(path, tempPath, out sourceWasAlreadyPDFA);
					break;

				case FileExtensions.DotTif:
				case FileExtensions.DotTiff:
					pdfAFile = ConvertTIFF(path, tempPath);
					break;

				case FileExtensions.DotDoc:
				case FileExtensions.DotDocx:
					pdfAFile = ConvertWordDoc(path, tempPath);
					break;

				case FileExtensions.DotXls:
				case FileExtensions.DotXlsx:
					pdfAFile = ConvertExcelDoc(path, tempPath);
					break;

				default:
					break;
			}

			return pdfAFile;
		}

		//--------------------------------------------------------------------------------
		public static bool IsPDFA(string path)
		{
			return ApitronIsPDFA(path) || GdPictureIsPDFA(path);
		}

		///<summary>
		/// Dato il file passato come parametro utilizza l'oggetto FixedDocument di Apitron
		/// e controlla se si tratta di un formato PDF/A o meno
		///</summary>
		//--------------------------------------------------------------------------------
		private static bool ApitronIsPDFA(string path)
		{
			try
			{
				using (Stream fileStream = File.OpenRead(path))
				{
					using (FixedDocument doc = new FixedDocument(fileStream))
					{
						if (Enum.GetName(typeof(PdfStandard), doc.PdfStandard) != PdfStandard.PDFA.ToString())
							return false;
					}
				}
			}
			catch (NullReferenceException nre)
			{
				Debug.WriteLine("IsPDFA() - NullReferenceException: " + nre.Message);
				return false;
			}
			catch (UnauthorizedAccessException uae)
			{
				Debug.WriteLine("IsPDFA() - UnauthorizedAccessException: " + uae.Message);
				return false;
			}
			catch (IOException ioe)
			{
				Debug.WriteLine("IsPDFA() - IOException: " + ioe.Message);
				return false;
			}
			catch (Exception e)
			{
				Debug.WriteLine("IsPDFA() - Exception: " + e.Message);
				return false;
			}

			return true;
		}

		///<summary>
		/// Dato il file passato come parametro istanzia un GdPicturePDF e controlla
		/// se si tratta di un formato PDF/A o meno
		///</summary>
		//--------------------------------------------------------------------------------
		private static bool GdPictureIsPDFA(string path)
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
		/// Esegue la conversione di un PDF in un altro file di tipo PDF/A utilizzando libreria APITRON
		///</summary>
		//--------------------------------------------------------------------------------
		private static string ConvertPDF(string path, string tempPath, out bool sourceWasAlreadyPDFA)
		{
			sourceWasAlreadyPDFA = false;

			string pdfFile = string.Format(@"{0}\{1}_A.{2}", tempPath, Path.GetFileNameWithoutExtension(path), FileExtensions.Pdf);

			if (ApitronIsPDFA(path))
			{
				sourceWasAlreadyPDFA = true;
				return path;
			}

			try
			{
				// se il file esiste lo elimino, intanto andrei a sovrascriverlo (ma se e' impostato come readonly avrei errori)
				if (File.Exists(pdfFile))
				{
					FileInfo f = new FileInfo(pdfFile);
					f.IsReadOnly = false;
					File.Delete(pdfFile);
				}

				using (Stream inputStream = File.OpenRead(path), outputStream = File.Create(pdfFile))
				{
					if (inputStream.Length == 0)
					{
						Debug.WriteLine("ConvertPDF() - inputStream.Length == 0");
						return string.Empty;
					}

					try
					{
						using (FixedDocument pdfDocument = new FixedDocument(inputStream, PdfStandard.PDFA))
						{
							Debug.WriteLine(string.Format("Start PDF/A file creation {0} - {1}", Path.GetFileName(pdfFile), DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss")));

							pdfDocument.IsCompressedStructure = false;

							try
							{
								pdfDocument.Save(outputStream);
							}
							catch (Apitron.PDF.Kit.ErrorHandling.DocumentSaveException dse)
							{
								Debug.WriteLine("ConvertPDF() - FixedDocument Save - Apitron.PDF.Kit.ErrorHandling.DocumentSaveException: " + dse.Message);
								pdfFile = string.Empty;
							}
							catch (AccessViolationException ave)
							{
								Debug.WriteLine("ConvertPDF() - FixedDocument Save - AccessViolationException: " + ave.Message);
								pdfFile = string.Empty;
							}
							catch (TypeInitializationException tie)
							{
								Debug.WriteLine("ConvertPDF() - FixedDocument Save - TypeInitializationException: " + tie.Message);
								pdfFile = string.Empty;
							}
							catch (Exception e)
							{
								Debug.WriteLine("ConvertPDF() - FixedDocument Save - Exception: " + e.Message);
								pdfFile = string.Empty;
							}

							Debug.WriteLine("End PDF/A file creation - " + DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss"));
						}
					}
					catch (Exception e)
					{
						Debug.WriteLine("ConvertPDF() - new FixedDocument - Exception: " + e.Message);
					}
				}

				if (!string.IsNullOrWhiteSpace(pdfFile))
				{
					FileInfo fi = new FileInfo(pdfFile);
					Debug.WriteLine(string.Format("ConvertPDF() pdfFile: {0}", pdfFile));

					if (!fi.Exists)
					{
						Debug.WriteLine("ConvertPDF() the file does not exists!");
						pdfFile = string.Empty;
					}

					Debug.WriteLine("ConvertPDF() length of file: " + fi.Length.ToString());
					if (fi.Length == 0)
						pdfFile = string.Empty;
				}
			}
			catch (NullReferenceException nre)
			{
				Debug.WriteLine("ConvertPDF() - NullReferenceException: " + nre.Message);
				pdfFile = string.Empty;
			}
			catch (Exception ex)
			{
				Debug.WriteLine("ConvertPDF() - Exception: " + ex.Message);
				pdfFile = string.Empty;
			}

			return pdfFile;
		}

		///<summary>
		/// Esegue la conversione di un PDF in un altro file di tipo PDF/A utilizzando libreria GdPicture
		///</summary>
		//--------------------------------------------------------------------------------
		private static string GdPictureConvertPDF(string path, string tempPath, out bool sourceWasAlreadyPDFA)
		{
			sourceWasAlreadyPDFA = false;

			// lavoro sempre nella temp che mi è stata passata (rinomino il file perche' non riesco a sovrascriverlo)
			string pdfFile = string.Format(@"{0}\{1}_A.{2}", tempPath, System.IO.Path.GetFileNameWithoutExtension(path), FileExtensions.Pdf);

			TBPicImaging gdPicture = new TBPicImaging();
			TBPicPDF srcPDF = new TBPicPDF();
			TBPicPDF dstPDF = new TBPicPDF();

			try
			{
				if (srcPDF.LoadFromFile(path, false) != TBPictureStatus.OK)
					return path;

				// se il file e' gia' in formato PDF/A lo ritorno subito e non procedo
				int pdfAConformance = srcPDF.GetPDFAConformance();
				if (pdfAConformance == 1 || pdfAConformance == 2) //valori di ritorno: 0: non PDF/A, 1: PDF/A-A, 2: PDF/A-B, 9: Unknown
				{
					sourceWasAlreadyPDFA = true;
					pdfFile = path;
					srcPDF.CloseDocument();
					return pdfFile;
				}

				// creo il file PDF (formato PDF/A 1-b compatibile)
				if (dstPDF.NewPDF(true) != TBPictureStatus.OK)
					return path;

				System.Diagnostics.Debug.WriteLine("Start PDF/A file creation- " + DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss"));

				for (int i = 1; i <= srcPDF.GetPageCount(); i++)
				{
					srcPDF.SelectPage(i);

					int imageId = srcPDF.RenderPageToGdPictureImage(100.0F, false);
					if (imageId > 0)
					{
						dstPDF.AddImageFromGdPictureImage(imageId, false, true);
						gdPicture.ReleaseGdPictureImage(imageId);
					}
				}

				dstPDF.SaveToFile(pdfFile);
				dstPDF.CloseDocument();
				srcPDF.CloseDocument();

				System.Diagnostics.Debug.WriteLine("End PDF/A file creation - " + DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss"));
			}
			catch (Exception ex)
			{
				throw (ex);
			}

			return pdfFile;
		}

		///<summary>
		/// Esegue la conversione di un TIFF in un file di tipo PDF/A
		///</summary>
		//--------------------------------------------------------------------------------
		private static string ConvertTIFF(string path, string tempPath)
		{
			// lavoro sempre nella temp che mi è stata passata
			string pdfFile = string.Format(@"{0}\{1}_A.{2}", tempPath, Path.GetFileNameWithoutExtension(path), FileExtensions.Pdf);
			
			TBPicImaging gdPicture = new TBPicImaging();
			TBPicPDF gdPDF = new TBPicPDF();
			int imageId = 0;

			try
			{
				imageId = gdPicture.CreateGdPictureImageFromFile(path);

				if (imageId <= 0)
					return path;

				// creo il file PDF (formato PDF/A 1-b compatibile)
				if (gdPDF.NewPDF(true) != TBPictureStatus.OK)
					return path;

				// Adds the first page
				gdPDF.AddImageFromGdPictureImage(imageId, false, true);

				int nrPages = gdPicture.TiffGetPageCount(imageId);
				if (nrPages > 1)
				{
					// parto da due perche' skippo la prima pagina 
					for (int i = 2; i <= nrPages; i++)
					{
						gdPicture.TiffSelectPage(imageId, i);
						// Adds other pages, if any
						gdPDF.AddImageFromGdPictureImage(imageId, false, true);
					}
				}

				gdPicture.ReleaseGdPictureImage(imageId);
				gdPDF.SaveToFile(pdfFile);
				gdPDF.CloseDocument();
			}
			catch (Exception ex)
			{
				throw (ex);
			}

			return pdfFile;
		}

		///<summary>
		/// Esegue la conversione di un file di Word in un file di tipo PDF/A
		///</summary>
		//--------------------------------------------------------------------------------
		private static string ConvertWordDoc(string path, string tempPath)
		{
            if (!RegisterKeyChecker.IsOfficeInstalled())
				return string.Empty;

			object nullobj = System.Reflection.Missing.Value;
			object file = path;

			FileInfo fileInfo = new FileInfo(path);

			// lavoro sempre nella temp che mi è stata passata
			string pdfFile = string.Format(@"{0}\{1}_A.{2}", tempPath, Path.GetFileNameWithoutExtension(path), FileExtensions.Pdf);

			Microsoft.Office.Interop.Word._Application myWinWord = null;
			_Document mydoc = null;
			object doNotSaveChanges = WdSaveOptions.wdDoNotSaveChanges;

			try
			{
				if (!File.Exists(pdfFile))
				{
					WdExportFormat paramExportFormat = WdExportFormat.wdExportFormatPDF;
					bool paramOpenAfterExport = false;

					WdExportOptimizeFor paramExportOptimizeFor = WdExportOptimizeFor.wdExportOptimizeForPrint;
					WdExportRange paramExportRange = WdExportRange.wdExportAllDocument;
					int paramStartPage = 0;
					int paramEndPage = 0;

					WdExportItem paramExportItem = WdExportItem.wdExportDocumentContent;
					bool paramIncludeDocProps = true;
					bool paramKeepIRM = true;

					WdExportCreateBookmarks paramCreateBookmarks = WdExportCreateBookmarks.wdExportCreateWordBookmarks;
					bool paramDocStructureTags = true;
					bool paramBitmapMissingFonts = true;
					bool paramUseISO19005_1 = true; //formato PDF/A
					object confirmConversions = false;
					object noEncodingDialog = true;

					myWinWord = new Microsoft.Office.Interop.Word.Application();

					// disabilito tutti gli AddIn di Word (compreso MagicDocuments) (e' pericoloso)
					/*for (int i = 0; i < myWinWord.AddIns.Count; i++)
					{
						AddIn ai = myWinWord.AddIns[i];
						ai.Installed = false;
					}
					myWinWord.AddIns.Unload(false); */

					mydoc = myWinWord.Documents.Open(ref file, ref confirmConversions, ref nullobj, ref nullobj, ref nullobj,
						ref nullobj, ref nullobj, ref nullobj, ref nullobj,
						ref nullobj, ref nullobj, ref nullobj, ref nullobj,
						ref nullobj, ref noEncodingDialog, ref nullobj);

					mydoc.ExportAsFixedFormat
						(
						pdfFile,
						paramExportFormat,
						paramOpenAfterExport,
						paramExportOptimizeFor,
						paramExportRange,
						paramStartPage,
						paramEndPage,
						paramExportItem,
						paramIncludeDocProps,
						paramKeepIRM,
						paramCreateBookmarks,
						paramDocStructureTags,
						paramBitmapMissingFonts,
						paramUseISO19005_1,
						ref nullobj
						);
				}
			}
			catch (COMException e)
			{
				throw (e);
			}
			catch (Exception ex)
			{
				throw (ex);
			}
			finally
			{
				try
				{
					if (mydoc != null)
						mydoc.Close(ref doNotSaveChanges, ref nullobj, ref nullobj);
					if (myWinWord != null)
						myWinWord.Quit(ref doNotSaveChanges, ref nullobj, ref nullobj);
				}
				catch (Exception e)
				{
					throw (e);
				}
			}

			return pdfFile;
		}

		///<summary>
		/// Esegue la conversione di un file di Excel in un file di tipo PDF/A
		///</summary>
		//--------------------------------------------------------------------------------
		private static string ConvertExcelDoc(string path, string tempPath)
		{
            if (!RegisterKeyChecker.IsOfficeInstalled())
				return string.Empty;

			FileInfo fileInfo = new FileInfo(path);
			// lavoro sempre nella temp che mi è stata passata
			string pdfFile = string.Format(@"{0}\{1}_A.{2}", tempPath, Path.GetFileNameWithoutExtension(path), FileExtensions.Pdf);

			// Create new instance of Excel
			Microsoft.Office.Interop.Excel.Application excelApplication = new Microsoft.Office.Interop.Excel.Application();
			excelApplication.ScreenUpdating = false; // Make the process invisible to the user
			excelApplication.DisplayAlerts = false; // Make the process silent

			// Open the workbook that you wish to export to PDF
			Microsoft.Office.Interop.Excel.Workbook excelWorkbook = excelApplication.Workbooks.Open(path);

			// If the workbook failed to open, stop, clean up, and bail out
			if (excelWorkbook == null)
			{
				excelApplication.Quit();
				excelApplication = null;
				excelWorkbook = null;
				return string.Empty;
			}

			try
			{
				// Call Excel's native export function (valid in Office 2007 and Office 2010, AFAIK)
				excelWorkbook.ExportAsFixedFormat(Microsoft.Office.Interop.Excel.XlFixedFormatType.xlTypePDF, pdfFile);
			}
			catch (COMException e)
			{
				throw (e);
			}
			catch (Exception ex)
			{
				throw (ex);
			}
			finally
			{
				// Close the workbook, quit the Excel, and clean up regardless of the results...
				if (excelWorkbook != null)
					excelWorkbook.Close();
				if (excelApplication != null)
					excelApplication.Quit();
				excelApplication = null;
				excelWorkbook = null;
			}

			return pdfFile;
		}

		///<summary>
		/// Utilizzato per comporre la stringa delle chiavi di descrizione per i documenti da inviare al SOS
		/// - Sostituisce i caratteri che invalidano la spedizione con uno spazio (i caratteri sostituiti sono i seguenti: \r\n \r \n ;)
		/// - tronca la riga a 100 chr
		/// - aggiunge il separatore ; in fondo alla stringa
		///</summary>
		//---------------------------------------------------------------------
		public static string ReplaceForbiddenCharsInSOSDescriptionKey(string strToEscape)
		{
			strToEscape = strToEscape.Replace("\r\n", " ").Replace(';', ' ').Replace("\r", " ").Replace("\n", " ");

			if (strToEscape.Length > 100)
				strToEscape = strToEscape.Substring(0, 100);

			return strToEscape + ';';
		}

		///<summary>
		/// Come da email di Tommaso Galli del 25/10/17
		/// un nome file non deve superare i 100 chr e non deve contenere i seguenti caratteri
		/// " * : < > ? / \ | ~ # % & { } à è ì ò ù á é í ó ú « » ' § ä Ä ß ü Ü ö Ö , ; °
		///</summary>
		//---------------------------------------------------------------------
		static string[] forbiddenCharsForSOSFileName = new string[] { "\"", "*", ":", "<", ">", "?", "/", "\\", "|", "~", "#", "%", "&", "{", "}",
													"à", "è", "ì", "ò", "ù", "á", "é", "í", "ó", "ú", "«", "»", "'", "§", "ä",
													"Ä", "ß", "ü", "Ü", "ö", "Ö", ",", ";", "°" };

		///<summary>
		/// Utilizzato per escapare il nome del file da mettere come prefisso nella stringa 
		/// delle chiavi di descrizione per i documenti da inviare al SOS: 
		/// sostituisce tutti i caratteri non ammessi da Windows e da Zucchetti nel nome di un file
		/// mentre il separatore ; in fondo alla stringa viene aggiunto dal chiamante
		///</summary>
		//---------------------------------------------------------------------
		public static string ReplaceForbiddenCharsInFileName(string fileName)
		{
			// elimino eventuali caratteri non validi nel nome file
			foreach (char invalidChar in Path.GetInvalidFileNameChars())
				fileName = fileName.Replace(invalidChar, '_');

			// elimino i caratteri non validi per il DMS Infinity
			foreach (string fc in forbiddenCharsForSOSFileName)
				fileName = fileName.Replace(fc, "_");

			return fileName;
		}

		//---------------------------------------------------------------------
		public static string GetEAStringValue(object erpValue, string valueType)
        {
            string stringValue = string.Empty;

            if (string.Compare(valueType, DataType.DataTypeStrings.String, true) == 0) return SoapTypes.ToSoapString((string)erpValue);
            else if (string.Compare(valueType, DataType.DataTypeStrings.Integer, true) == 0) return SoapTypes.ToSoapShort((short)erpValue);
            else if (string.Compare(valueType, DataType.DataTypeStrings.Long, true) == 0) return SoapTypes.ToSoapInt((int)erpValue);
            else if (string.Compare(valueType, DataType.DataTypeStrings.Double, true) == 0) return SoapTypes.ToSoapDouble((double)erpValue);
            else if (string.Compare(valueType, DataType.DataTypeStrings.Money, true) == 0) return SoapTypes.ToSoapDouble((double)erpValue);
            else if (string.Compare(valueType, DataType.DataTypeStrings.Quantity, true) == 0) return SoapTypes.ToSoapDouble((double)erpValue);
            else if (string.Compare(valueType, DataType.DataTypeStrings.Percent, true) == 0) return SoapTypes.ToSoapDouble((double)erpValue);
            else if (string.Compare(valueType, DataType.DataTypeStrings.Bool, true) == 0) return SoapTypes.ToSoapBoolean((bool)erpValue);
            else if (string.Compare(valueType, DataType.DataTypeStrings.Uuid, true) == 0) return SoapTypes.ToSoapGuid((Guid)erpValue);
            else if (string.Compare(valueType, DataType.DataTypeStrings.Date, true) == 0)
            {
                DateTime erpDate = (DateTime)erpValue;
                return (erpDate.Year == 1799 && erpDate.Month == 12 && erpDate.Day == 31) ? string.Empty : SoapTypes.ToSoapDate(erpDate);
            }

            else
                if (
                    (string.Compare(valueType, DataType.DataTypeStrings.Time, true) == 0) ||
                    (string.Compare(valueType, DataType.DataTypeStrings.DateTime, true) == 0)
                    )
                {
                    DateTime erpDateTime = (DateTime)erpValue;
                    return (erpDateTime.Year == 1799 && erpDateTime.Month == 12 && erpDateTime.Day == 31) ? string.Empty : SoapTypes.ToSoapDateTime(erpDateTime);
                }

                else if (string.Compare(valueType, DataType.DataTypeStrings.ElapsedTime, true) == 0) return SoapTypes.ToSoapInt((int)erpValue);
                else if (string.Compare(valueType, DataType.DataTypeStrings.Enum, true) == 0) return SoapTypes.ToSoapDataEnum(new DataEnum(Convert.ToUInt32(erpValue)));
                else if (string.Compare(valueType, DataType.DataTypeStrings.Text, true) == 0) return SoapTypes.ToSoapString((string)erpValue);
                else
                    if (valueType.StartsWith(DataType.DataTypeStrings.EnumType))
                        return SoapTypes.ToSoapDataEnum(new DataEnum(Convert.ToUInt32(erpValue)));

            return stringValue;
        }
	}

	//================================================================================
	public static class FileExtensions
	{
		// possibili estensioni di file gestiti
		public const string Avi = "avi";
		public const string Bmp = "bmp";
		public const string Config = "config";
		public const string Doc = "doc";
		public const string Docx = "docx";
        public const string Ppt = "ppt";
        public const string Pptx = "pptx";
		public const string Gif = "gif";
		public const string Gzip = "gzip";
		public const string Html = "html";
		public const string Htm = "htm";
		public const string Jpg = "jpg";
		public const string Jpeg = "jpeg";
		public const string Mp3 = "mp3";
		public const string Mpeg = "mpeg";
		public const string Msg = "msg";
		public const string Tif = "tif";
		public const string Tiff = "tiff";
		public const string Txt = "txt";
		public const string Papery = "papery";
		public const string Pdf = "pdf";
		public const string Png = "png";
		public const string Rar = "rar";
		public const string Rtf = "rtf";
		public const string Xml = "xml";
		public const string Xls = "xls";
		public const string Xlsx = "xlsx";
		public const string Zip = "zip";
		public const string Zip7z = "7z";
		public const string Wmv = "wmv";
		public const string Wav = "wav";

		// 
		public const string TxtCompatible = "Txt compatible";
		public const string All = "all";

		// estensioni con il punto
		public const string DotBmp = ".bmp";
		public const string DotDoc = ".doc";
		public const string DotDocx = ".docx";
		public const string DotGif = ".gif";
		public const string DotJpg = ".jpg";
		public const string DotJpeg = ".jpeg";
		public const string DotPdf = ".pdf";
		public const string DotPng = ".png";
		public const string DotTif = ".tif";
		public const string DotTiff = ".tiff";
		public const string DotXls = ".xls";
		public const string DotXlsx = ".xlsx";

		/// <summary>
		/// dice se la stringa termina con '.pdf'
		/// </summary>
		/// <param name="path">nome del file o percorso completo, ovviamente comprensivo di estensione</param>
		/// <returns></returns>
		//---------------------------------------------------------------------
		public static bool IsPdfPath(string path)
		{
			if (string.IsNullOrWhiteSpace(path))
				return false;

			return path.EndsWith(DotPdf, StringComparison.InvariantCultureIgnoreCase);
		}

		/// <summary>
		/// dice se la stringa e' uguale a 'pdf'
		/// </summary>
		//---------------------------------------------------------------------
		public static bool IsPdfString(string val)
		{
			if (string.IsNullOrWhiteSpace(val))
				return false;

			return String.Compare(val, Pdf, StringComparison.InvariantCultureIgnoreCase) == 0;
		}

		/// <summary>
		/// dice se la stringa termina con '.tif' o '.tiff'
		/// </summary>
		/// <param name="path">nome del file o percorso completo, ovviamente comprensivo di estensione</param>
		/// <returns></returns>
		//---------------------------------------------------------------------
		public static bool IsTifPath(string path)
		{
			if (string.IsNullOrWhiteSpace(path))
				return false;

			return
				path.EndsWith(DotTif, StringComparison.InvariantCultureIgnoreCase) ||
				path.EndsWith(DotTiff, StringComparison.InvariantCultureIgnoreCase);
		}

		/// <summary>
		/// dice se la stringa e' uguale a 'tif' o 'tiff'
		/// </summary>
		//---------------------------------------------------------------------
		public static bool IsTifString(string val)
		{
			if (string.IsNullOrWhiteSpace(val))
				return false;

			return
				String.Compare(val, Tif, StringComparison.InvariantCultureIgnoreCase) == 0 ||
				String.Compare(val, Tiff, StringComparison.InvariantCultureIgnoreCase) == 0;
		}

		///<summary>
		/// Dice se l'estensione (con il punto iniziale) e' del tipo di un documento di Office 
		/// (nel nostro caso Word (doc/docx) o Excel (xls/xlsx))
		///</summary>
		//---------------------------------------------------------------------
		public static bool IsOfficeDocExtension(string extensionWithDot)
		{
			if (string.IsNullOrWhiteSpace(extensionWithDot))
				return false;

			return
				String.Compare(extensionWithDot, DotDoc, StringComparison.InvariantCultureIgnoreCase) == 0 ||
				String.Compare(extensionWithDot, DotDocx, StringComparison.InvariantCultureIgnoreCase) == 0	||
				String.Compare(extensionWithDot, DotXls, StringComparison.InvariantCultureIgnoreCase) == 0 ||
				String.Compare(extensionWithDot, DotXlsx, StringComparison.InvariantCultureIgnoreCase) == 0;
		}

        //---------------------------------------------------------------------
        public static StringCollection ExtensionsConvertibleToPDFA = new StringCollection{
                 FileExtensions.DotPdf,
				 FileExtensions.DotTif,
				 FileExtensions.DotTiff,
				 FileExtensions.DotDoc,
				 FileExtensions.DotDocx,
				 FileExtensions.DotXls,
				 FileExtensions.DotXlsx};

		///<summary>
		/// Data l'estensione passata come parametro stabilisco se il file e' convertibile in un formato PDF/A
		/// (sono convertibili i soli file pdf, tiff/tif, doc/docx, xls/xlsx)
		///</summary>
		//---------------------------------------------------------------------
		public static bool CanBeConvertedToPDFA(string extensionWithDot)
		{
            return ExtensionsConvertibleToPDFA.ContainsNoCase(extensionWithDot);
		}
	}
} 
