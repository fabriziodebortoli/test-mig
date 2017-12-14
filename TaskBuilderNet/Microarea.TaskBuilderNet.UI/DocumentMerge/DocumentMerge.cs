using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Microarea.TBPicComponents;
using Microsoft.Office.Interop.Word;
using Microsoft.CSharp.RuntimeBinder;

namespace Microarea.TaskBuilderNet.UI.DocumentMerge
{

	public enum TemplateType { TPL_EMPTY, TPL_RTF, TPL_PDF, TPL_MSWORD, TPL_MSWORDX, TPL_MSEXCEL, TPL_MSEXCELX, TPL_ODT, TPL_SXW }; 

	//============================================================================
    public class DocumentMerge: IDisposable
    {
		private TBPicImaging gdPictureImaging = new TBPicImaging();
		private TBPicPDF nativePDF = new TBPicPDF();

        private Microsoft.Office.Interop.Word.Application	word = null;
		private Microsoft.Office.Interop.Word.Document		doc = null;

		private object missing = System.Type.Missing;
		private Object oFalse = false;

		private string templateFilePath = string.Empty;
		private string destinationFilePath = string.Empty;

		private Dictionary<string, object> placeholdersForReplace = null;
		private TemplateType fileType;

		private string paleceholderForDOC = "==";
		private string paleceholderForRTF = "$$";

		public string PaleceholderForDOC		{ get { return paleceholderForDOC; } set { paleceholderForDOC = value; } }
		public string PaleceholderForRTF		{ get { return paleceholderForRTF; } set { paleceholderForRTF = value; } }

		public string TemplateFilePath		{ get { return templateFilePath; } set { templateFilePath = value; } }
		public string DestinationFilePath	{ get { return destinationFilePath; } set { destinationFilePath = value; } }

		public Dictionary<string, object> PlaceholdersForReplace	{ get { return placeholdersForReplace; } set { placeholdersForReplace = value; } }
		public TemplateType FileType							{ get { return fileType; } set { fileType = value; } }

		//--------------------------------------------------------------------------------------------------------------------------------
		public DocumentMerge(string templateFilePath, string destinationFilePath, TemplateType fileType, Dictionary<string, object> placeholdersForReplace)
		{
			this.templateFilePath = templateFilePath;
			this.destinationFilePath = destinationFilePath;

			this.placeholdersForReplace = placeholdersForReplace;
			this.fileType = fileType;
		}


		//--------------------------------------------------------------------------------------------------------------------------------
		public DocumentMerge(string templateFilePath, string destinationFilePath, TemplateType fileType)
		{
			this.templateFilePath = templateFilePath;
			this.destinationFilePath = destinationFilePath;
			this.fileType = fileType;
		}
		//---------------------------------------------------------------------
		public void CreateDocx(object[,] aCellsValues, string[] aTitles)
		{
			string[] titles = aTitles;
			object[,] cellsValues = aCellsValues;
			try
			{
				OpenWord();

				doc = word.Documents.Add(Template: templateFilePath);
				doc.Activate();

				foreach (Field f in doc.Fields)
				{ }
			}
			catch (System.Exception)
			{
			}
			finally
			{
				QuitWord();
			}
		}
	    //---------------------------------------------------------------------
        public void ReplacePlaceHolders()
        {
            switch (fileType)
            {
                case TemplateType.TPL_RTF:
                {
                    ReplacePlaceHoldersInRTFFile();
                    break;
                }
                case TemplateType.TPL_PDF:
                {
                    ReplacePlaceHoldersInPDFFile(false);
                    break;
                }
                case TemplateType.TPL_MSWORD:
                {
                    ReplacePlaceHoldersInDOCFile();
                    break;
                }
                case TemplateType.TPL_MSWORDX:
                {
                    ReplacePlaceHoldersInDOCXFile();
                    break;
                }
                case TemplateType.TPL_MSEXCEL:
                {
                    ReplacePlaceHoldersInExcelFile();
                    break;
                }
                 case TemplateType.TPL_ODT:
                {
                    ReplacePlaceHoldersInOpenOfficeFile();
                    break;
                }
                default:
                {
                    break;
                }
            }
        }

	    //---------------------------------------------------------------------
		void ReplacePlaceHoldersInRTFFile()
		{
			try
			{
				string text = null;
				if (File.Exists(templateFilePath))
				{
					using (StreamReader sr = new StreamReader(templateFilePath))
						text = sr.ReadToEnd();

					foreach (string placeholder in placeholdersForReplace.Keys)
						text = text.Replace(paleceholderForRTF + placeholder + paleceholderForRTF, placeholdersForReplace[placeholder].ToString());
				}

				using (StreamWriter sw = new StreamWriter(destinationFilePath))
					sw.Write(text);
			}
			catch (System.Exception)
			{
			}
		}

		//---------------------------------------------------------------------
		void ReplacePlaceHoldersInOpenOfficeFile()
		{
			Odt odt = new Odt(templateFilePath);
			OdtDocFields inputs = odt.Inputs;

			foreach (string placeholder in placeholdersForReplace.Keys)
			{
				if (inputs[placeholder] != null)
					inputs[placeholder] = placeholdersForReplace[placeholder].ToString();
			}

			odt.Save(destinationFilePath);
		}

        //sostituzione di placeholder (==) in un word
        //---------------------------------------------------------------------
		void ReplacePlaceHoldersInDOCFile()
        {
            try
            {
                OpenWord();

				object fileName = templateFilePath;

                doc = word.Documents.Open(ref fileName, ref missing, ref missing, ref missing
                    , ref missing, ref missing, ref missing, ref missing, ref missing, ref missing
                    , ref missing, ref missing, ref missing, ref missing, ref missing, ref missing);

                doc.Activate();

				foreach (string placeholder in placeholdersForReplace.Keys)
				{
					object findStr = paleceholderForDOC + placeholder + paleceholderForDOC;
					word.Selection.Find.Execute(ref findStr);
					word.Selection.Text = placeholdersForReplace[placeholder].ToString();					
				}

				object saveFileName = destinationFilePath;

                doc.SaveAs(ref saveFileName, ref missing, ref missing, ref missing, ref missing
                    , ref missing, ref missing, ref missing, ref missing, ref missing, ref missing
                    , ref missing, ref missing, ref missing, ref missing, ref missing);

            }
            catch (System.Exception)
            {
            }
            finally
            {
                QuitWord();
            }
        }

		//---------------------------------------------------------------------
		void ReplacePlaceHoldersInExcelFile()
		{
			Microsoft.Office.Interop.Excel.Application xlApp;
			Microsoft.Office.Interop.Excel.Workbook xlWorkBook;
			Microsoft.Office.Interop.Excel.Worksheet xlWorkSheet = new Microsoft.Office.Interop.Excel.Worksheet();
			Microsoft.Office.Interop.Excel.Range range;

			string str;
			int rCnt = 0;
			int cCnt = 0;

			xlApp = new Microsoft.Office.Interop.Excel.Application();
			xlWorkBook = xlApp.Workbooks.Open(templateFilePath, 0, true, 5, "", "", true, Microsoft.Office.Interop.Excel.XlPlatform.xlWindows, "\t", false, false, 0, true, 1, 0);
			xlWorkSheet = (Microsoft.Office.Interop.Excel.Worksheet)xlWorkBook.Worksheets.get_Item(1);

			range = xlWorkSheet.UsedRange;

			for (rCnt = 1; rCnt <= range.Rows.Count; rCnt++)
			{
				for (cCnt = 1; cCnt <= range.Columns.Count; cCnt++)
				{
                    str = (string)(range.Cells[rCnt, cCnt] as Microsoft.Office.Interop.Excel.Range).Value2;
					foreach (string placeholder in placeholdersForReplace.Keys)
					{
						if (str == placeholder)
							(range.Cells[rCnt, cCnt] as Microsoft.Office.Interop.Excel.Range).Value = placeholdersForReplace[placeholder].ToString();

					}
				}
			}

			object misValue = System.Reflection.Missing.Value;
			xlWorkBook.SaveAs(destinationFilePath, Microsoft.Office.Interop.Excel.XlFileFormat.xlWorkbookNormal, misValue, misValue, misValue,
								misValue, Microsoft.Office.Interop.Excel.XlSaveAsAccessMode.xlExclusive, misValue, misValue, misValue, misValue, misValue);
			xlWorkBook.Close(true, null, null);
			xlApp.Quit();

			releaseObject(xlWorkSheet);
			releaseObject(xlWorkBook);
			releaseObject(xlApp);


		}

		//---------------------------------------------------------------------
		private void releaseObject(object obj)
		{
			try
			{
				System.Runtime.InteropServices.Marshal.ReleaseComObject(obj);
				obj = null;
			}
			catch (Exception ex)
			{
				obj = null;
				MessageBox.Show("Unable to release the Object " + ex.ToString());
			}
			finally
			{
				GC.Collect();
			}
		} 

		//---------------------------------------------------------------------
		void ReplacePlaceHoldersInDOCXFile()
		{
			try
			{
				OpenWord();

				doc = word.Documents.Add(Template: templateFilePath);
				doc.Activate();

				foreach (Field f in doc.Fields)
				{
					foreach (string placeholder in placeholdersForReplace.Keys)
					{
						object findStr = placeholder;

						if (f.Code.Text.Contains(findStr.ToString()))
						{
							f.Select();
							word.Selection.TypeText(placeholdersForReplace[placeholder].ToString());
							break;
						}

					}
				}
				doc.SaveAs(destinationFilePath);
			}
			catch (System.Exception)
			{
			}
			finally
			{
				QuitWord();
			}
		}

        //apre word
        //---------------------------------------------------------------------
        public void OpenWord()
        {
           word = new Microsoft.Office.Interop.Word.Application();
           doc = new Microsoft.Office.Interop.Word.Document();
        }

        //esce da word
        //---------------------------------------------------------------------
        public void QuitWord()
        {
            if (doc != null)
            {
                ((Microsoft.Office.Interop.Word._Document)doc).Close();
                doc = null;
            }

            if (word != null && word.Application != null)
            {
                ((Microsoft.Office.Interop.Word._Application)word.Application).Quit(ref missing, ref missing, ref missing);
                word = null;
            }
        }

        //Verifica se fare così o in altro modo
        //---------------------------------------------------------------------
        public void Dispose()
        {
            QuitWord();
        }

		//---------------------------------------------------------------------
		private List<string> GetAllObjectToReplaceRTF(string rtfDocString)
		{
			List<string> placeholders = new List<string>();

			MatchCollection match = Regex.Matches(rtfDocString, @"\$$\w+\$$");

			foreach (Match matchgroup in match)
				placeholders.Add(matchgroup.ToString());

			return placeholders;
		}

		//---------------------------------------------------------------------
		void ReplacePlaceHoldersInPDFFile(bool saveAFormat)
		{
			float Left = 0.0f;
			float Top = 0.0f;
			float Right = 0.0f;
			float Bottom = 0.0f;
			int nFSize = 6;

			TBPicDocumentFormat docFormat = gdPictureImaging.GetDocumentFormatFromFile(templateFilePath);

			if (docFormat == TBPicDocumentFormat.DocumentFormatPDF)
			{
				if (nativePDF.LoadFromFile(templateFilePath, false) == TBPictureStatus.OK)
				{
					for (int i = 0; i < nativePDF.GetPageCount(); i++)
					{
						nativePDF.SelectPage(i);

						int fCount = nativePDF.GetFormFieldsCount();
						for (int j = 0; j < fCount; j++)
						{
							int fieldID = nativePDF.GetFormFieldID(j);
							Debug.Assert(fieldID > -1, "Error in Field ID");
							string fTitle = nativePDF.GetFormFieldTitle(fieldID);

							if (placeholdersForReplace.Keys.Contains(fTitle))
							{
								string newfieldName = placeholdersForReplace[fTitle].ToString();
								if (nativePDF.GetFormFieldLocation(fieldID, ref Left, ref Top, ref Right, ref Bottom) != TBPictureStatus.OK)
									continue;

								if (nativePDF.RemoveFormField(fieldID) != TBPictureStatus.OK)
									continue;

								string fontResName = nativePDF.AddStandardFont(TBPicPdfStandardFont.PdfStandardFontCourier);
								nativePDF.AddTextFormField(Left, Top, Right - Left, Bottom - Top, newfieldName, newfieldName, false, fontResName, nFSize, 0, 0, 0);
							}
						}
					}

					if (saveAFormat)
						SaveAFormat();
					else
						nativePDF.SaveToFile(destinationFilePath);

					nativePDF.CloseDocument();
				}
			}
		}

		//---------------------------------------------------------------------------
		private void SaveAFormat()
		{
			float DPI_RENDERING = 200.0F;
			bool RENDER_FORMS = true;

			TBPicPDF dstPDF = new TBPicPDF();
			dstPDF.NewPDF(true);

			for (int i = 0; i < nativePDF.GetPageCount(); i++)
			{
				nativePDF.SelectPage(i);
				int imageID = nativePDF.RenderPageToGdPictureImageEx(DPI_RENDERING, RENDER_FORMS);
				if (imageID != 0)
				{
					dstPDF.AddImageFromGdPictureImage(imageID, false, true);
					gdPictureImaging.ReleaseGdPictureImage(imageID);
				}
			}

			dstPDF.SaveToFile(destinationFilePath);
			dstPDF.CloseDocument();
		}
	}
}
