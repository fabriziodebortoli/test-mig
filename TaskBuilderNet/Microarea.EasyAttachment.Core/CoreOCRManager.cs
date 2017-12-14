using System;
using System.Drawing.Drawing2D;
using System.IO;
using Microarea.TBPicComponents;
using Microarea.TaskBuilderNet.Core.Generic;


namespace Microarea.EasyAttachment.Core
{
	//================================================================================
	public class CoreOCRManager
	{
		private TBPicImaging gdPicture = new TBPicImaging();
		private TBPicPDF gdPDF = new TBPicPDF();

		private OCRDictionary ocrDict = null;
		private DMSModelDataContext dc = null;

		private string tempPath = string.Empty;
		private int imageId = 0;
		private bool isPdfFile = false;

		//-------------------------------------------------------------------------------
		public int ImageId { get { return imageId; } set { imageId = value; } }
		public int PdfPageCount { get { return gdPDF.GetPageCount(); } }
		public int PdfCurrentPage { get { return gdPDF.GetCurrentPage(); } }
		public bool IsPdfFile { get { return isPdfFile; } }

		public TBPictureStatus Stat { get { return (isPdfFile) ? gdPDF.GetStat() : gdPicture.GetStat(); } }

		//-------------------------------------------------------------------------------
		public CoreOCRManager(DMSModelDataContext dataContext, string path, OCRDictionary ocrDict)
		{
			this.ocrDict = ocrDict;
			dc = dataContext;
			tempPath = path;
           
		}				

		///<summary>
		/// Costruttore utilizzato dal FullTextManager
		/// Essendo istanziato in EasyAttachmentSync non conosce il dmsorchestrator
		///</summary>
		//-------------------------------------------------------------------------------
		public CoreOCRManager(DMSModelDataContext dataContext, string path, int lcid)
			: 
			this(dataContext, path, OCRDictionaryHelper.GetOCRDictionaryFromLCID(lcid))
		{
		}				
		
		//---------------------------------------------------------------------
		public void PdfCloseDocument()
		{
			if (isPdfFile)
				gdPDF.CloseDocument();
		}

		///<summary>
		/// Si posiziona nella pagina successiva a quella corrente nel file pdf
		///</summary>
		//---------------------------------------------------------------------
		public bool PdfSelectNextPage()
		{
			return PdfSelectPage(1);
		}

		///<summary>
		/// Si posiziona nella pagina precedente a quella corrente nel file pdf
		///</summary>
		//---------------------------------------------------------------------
		public bool PdfSelectPreviousPage()
		{
			return PdfSelectPage(-1);
		}

		///<summary>
		/// Seleziona la pagina prec. o succ.
		///</summary>
		//---------------------------------------------------------------------
		private bool PdfSelectPage(int deltaPage)
		{
			int page = PdfCurrentPage + deltaPage;
			if (page > PdfPageCount || page < 1)
				return false;

			return PdfRenderPage(page);
		}

		///<summary>
		/// Seleziona la specifica pagina passata come parametro
		///</summary>
		//---------------------------------------------------------------------
		public bool PdfRenderPage(int gotoPage)
		{
			if (!isPdfFile || gotoPage== gdPDF.GetCurrentPage())
				return true;

			// seleziono la pagina corrente e la renderizzo in memoria
            if (gdPDF.SelectPage(gotoPage) == TBPictureStatus.OK)
			{
				try
				{
					if (imageId > 0)
						gdPicture.ReleaseGdPictureImage(imageId);

					imageId = gdPDF.RenderPageToGdPictureImage(300, false);
				}
				catch (Exception e)
				{
					System.Diagnostics.Debug.WriteLine(e.ToString());
				}

				if (imageId > 0)
					return true;
			}

			return false;
		}

		//---------------------------------------------------------------------
		public void LoadFromByteArray(byte[] content)
		{
			try
			{
				isPdfFile = false;
				imageId = gdPicture.CreateGdPictureImageFromByteArray(content);
			}
			catch (OutOfMemoryException e)
			{ 
				throw(e);
			}
		}

		//---------------------------------------------------------------------
		public bool LoadFromFile(string path)
		{
			isPdfFile = false;

			if (string.IsNullOrEmpty(path))
				return false;

			string ext = Path.GetExtension(path);

			if (!CoreUtils.IsOCRCompatible(dc.TextExtensions, ext))
				return false;

			try
			{
				switch (ext.ToLowerInvariant())
				{
					case FileExtensions.DotPdf:
						{
							isPdfFile = true;
							if (gdPDF.LoadFromFile(path, false) == TBPictureStatus.OK)
								imageId = gdPDF.RenderPageToGdPictureImage(300, false); 
							break;
						}

					case FileExtensions.TxtCompatible:
						{
							string pdfFileName = CoreUtils.TransformToPdfA(path, tempPath);
							if (File.Exists(pdfFileName))
							{
								isPdfFile = true;
								if (gdPDF.LoadFromFile(pdfFileName, false) == TBPictureStatus.OK)
									imageId = gdPDF.RenderPageToGdPictureImage(300, false);								
							}
							break;
						}
					default:
						{
							imageId = gdPicture.CreateGdPictureImageFromFile(path);
							if (gdPicture.GetStat() != TBPictureStatus.OK && CoreUtils.IsTextCompatible(dc.TextExtensions, ext))
								goto case FileExtensions.TxtCompatible;
							break;
						}
				}
			}
			catch (Exception e)
			{
				throw (e);
			}

			return Stat == TBPictureStatus.OK;
		}

		//---------------------------------------------------------------------
		internal bool OCRCurrentPage(ref string ocrResult)
		{
			ocrResult = string.Empty;

			if (imageId == 0)
				return false;

			try
			{
				gdPicture.Scale(imageId, 300, InterpolationMode.HighQualityBicubic);
				gdPicture.OCRTesseractReinit();
				ocrResult = gdPicture.OCRTesseractDoOCR(imageId, ocrDict.Name, ocrDict.Path, string.Empty);
			}
			catch (Exception e)
			{
				System.Diagnostics.Debug.WriteLine("OCRTesseractDoOCR: " + e.ToString());
			}
			finally
			{
				gdPicture.OCRTesseractClear();
			}

			return (gdPicture.GetStat() == TBPictureStatus.OK);
		}

		// questo metodo viene chiamato solo da EasyAttachmentSync per la crezione del testo per FullTextSearch in caso
		// di file di tipo image e pdf. Tutto il resto viene fatto utilizzando i Filter di SqlServer
		//---------------------------------------------------------------------
		internal string OCRDocumentContent(string fileName)
		{
			int pageCount = 1;
			string ocrResult = string.Empty;
			bool success = true;
		
			if (string.IsNullOrEmpty(fileName))
				return ocrResult;

			string ext = Path.GetExtension(fileName);
			if (ext.CompareNoCase(FileExtensions.Txt))
				return ocrResult;

			// lavoro sempre nel directory temporanea o del webservice EasyAttachmentSync nel caso di creazione testo per FullFextSearch			
			// oppure nella directory temp dell'utente
			string txtFile = string.Format(@"{0}\{1}.{2}", tempPath, Path.GetFileNameWithoutExtension(fileName), FileExtensions.Txt);

			isPdfFile = false;

			StreamWriter writer = File.CreateText(txtFile);

			try
			{
				switch (ext.ToLowerInvariant())
				{
					case FileExtensions.DotPdf:
						{
							isPdfFile = true;
							bool emptyResult = true;

							if (gdPDF.LoadFromFile(fileName, false)	== TBPictureStatus.OK)
							{
								for (int i = 1; i <= gdPDF.GetPageCount(); i++)
								{
									if (i > 1)
										gdPDF.SelectPage(i);

									ocrResult = gdPDF.GetPageText();
									if (gdPDF.GetStat() != TBPictureStatus.OK)
									{
										success = false;
										break;
									}

									emptyResult = string.IsNullOrEmpty(ocrResult);
									writer.WriteLine(ocrResult);
								}
								
								// se non ho estratto niente è probabile che il PDF sia un immagine e non un PDFA.
								// provo a fare l'OCR trattando il file come immagine
								if (emptyResult)
								{
									// seleziono la prima pagina, 
									// perche' richiamo il goto default e devo posizionarmi di nuovo nella prima pagina
									gdPDF.SelectPage(1);
									goto default;
								}
							}
							break;
						}
					case FileExtensions.DotTif:
					case FileExtensions.DotTiff:
						{
							imageId = gdPicture.TiffCreateMultiPageFromFile(fileName);

							if (gdPicture.GetStat() != TBPictureStatus.OK)
							{
								success = false;
								break;
							}

							// se si tratta di un Tiff multipagina scorro le pagine
							if (gdPicture.TiffIsMultiPage(imageId))
							{
								pageCount = gdPicture.TiffGetPageCount(imageId);
								for (int i = 1; i <= pageCount; i++)
								{
									if (i > 1)
										gdPicture.TiffSelectPage(imageId, i);

									if (!OCRCurrentPage(ref ocrResult))
									{
										success = false;
										break;
									}
									writer.WriteLine(ocrResult);
								}
							}
							else // altrimenti vado sull'unica pagina
								if (!OCRCurrentPage(ref ocrResult))
									success = false;
								else
									writer.WriteLine(ocrResult);
							break;
						}
					default:
						{
							if (isPdfFile) // in questo caso il pdf è di tipo immagine e non PDFA
							{
								imageId = gdPDF.RenderPageToGdPictureImage(300, false);

								for (int i = 1; i <= gdPDF.GetPageCount(); i++)
								{
									if (i > 1)
										PdfRenderPage(i);

									if (!OCRCurrentPage(ref ocrResult))
									{
										success = false;
										break;
									}
									writer.WriteLine(ocrResult);
								}
							}
							else
							{
								imageId = gdPicture.CreateGdPictureImageFromFile(fileName);
								
								if (!OCRCurrentPage(ref ocrResult))
									success = false;
								else
									writer.WriteLine(ocrResult);
							}
							break;
						}
				}
			}
			catch (Exception ex)
			{
				throw (ex);
			}
			finally
			{
				writer.Close();
				if (!success)
					File.Delete(txtFile);
				gdPicture.OCRTesseractClear();
				if (imageId > 0)
					gdPicture.ReleaseGdPictureImage(imageId);
				if (isPdfFile)
					gdPDF.CloseDocument();
			}

			return (success) ? txtFile : string.Empty;
		}

		///<summary>
		/// Data la pagina e le quattro coordinate identifica il testo contenuto nel rettangolo via OCR
		/// e ritorna il testo
		///</summary>		
		//---------------------------------------------------------------------
		public string GetOCRTextArea(int leftArea, int topArea, int widthArea, int heightArea)
		{
			string ocrText = string.Empty;
			
			try
			{
				gdPicture.SetROI(leftArea, topArea, widthArea, heightArea);
				gdPicture.OCRTesseractReinit();

				// leggo il testo contenuto nel rect
				ocrText = gdPicture.OCRTesseractDoOCR(imageId, ocrDict.Name, ocrDict.Path, string.Empty);

				if (gdPicture.GetStat() != TBPictureStatus.OK)
					ocrText = string.Empty;
			}
			catch (Exception e)
			{
				System.Diagnostics.Debug.WriteLine("OCRTesseractDoOCR: " + e.ToString());
				ocrText = string.Empty;
			}
			finally
			{
				gdPicture.OCRTesseractClear();
			}

			return ocrText;
		}

		///<summary>
		/// Data la pagina PDF e le quattro coordinate identifica il testo contenuto nel rettangolo via OCR
		/// e ritorna il testo
		///</summary>
		//---------------------------------------------------------------------
		public string GetPdfOCRTextArea(int page, float leftArea, float topArea, float widthArea, float heightArea)
		{
			string ocrText = string.Empty;		
			
			try
			{
				if (isPdfFile && PdfRenderPage(page) && imageId > 0)
					ocrText = gdPDF.GetPageTextArea(leftArea, topArea, widthArea, heightArea);
			}
			catch (Exception)
			{
			}

			if (gdPicture.GetStat() != TBPictureStatus.OK)
				ocrText = string.Empty;

			return ocrText;
		}
	}
}
