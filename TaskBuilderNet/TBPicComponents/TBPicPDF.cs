using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GdPicture12;
using System.Drawing.Imaging;

namespace Microarea.TBPicComponents
{
    //=========================================================================
    public class TBPicPDF
    {
        //--------------------------------------------------------------------------------
        private GdPicturePDF gdPicturePDF;

        //--------------------------------------------------------------------------------
        public TBPicPDF()
        {
            TBPicBaseComponents.Register();
            gdPicturePDF = new GdPicturePDF();
        }

		//--------------------------------------------------------------------------------
		public TBPicPDF(GdPicturePDF aGdPicturePDF)
		{
			TBPicBaseComponents.Register();
			gdPicturePDF = aGdPicturePDF;
		}

		//--------------------------------------------------------------------------------
		public TBPicPDF MergeDocuments(TBPicPDF[] SrcDoc)
		{
			List<GdPicturePDF> array = new List<GdPicturePDF>();

			foreach (TBPicPDF pdf in SrcDoc)
				array.Add(pdf.gdPicturePDF);

			TBPicPDF pic = new TBPicPDF(gdPicturePDF.MergeDocuments(array.ToArray()));
			return pic ;
		}

        //--------------------------------------------------------------------------------
        public TBPictureStatus LoadFromFile(string path, bool loadInMemory)
        {
            return TBPicBaseComponents.TranslateTBPictureStatus(gdPicturePDF.LoadFromFile(path, loadInMemory));
        }

        //--------------------------------------------------------------------------------
        public TBPictureStatus CloseDocument()
        {
            return TBPicBaseComponents.TranslateTBPictureStatus(gdPicturePDF.CloseDocument());
        }

        //--------------------------------------------------------------------------------
        public int GetPageCount()
        {
            return gdPicturePDF.GetPageCount();
        }

        //--------------------------------------------------------------------------------
        public float GetPageWidth()
        {
            return gdPicturePDF.GetPageWidth();
        }

        //--------------------------------------------------------------------------------
        public float GetPageHeight()
        {
            return gdPicturePDF.GetPageHeight();
        }

        //--------------------------------------------------------------------------------
        public int GetCurrentPage()
        {
            return gdPicturePDF.GetCurrentPage();
        }

        //--------------------------------------------------------------------------------
        public TBPictureStatus GetStat()
        {
            return TBPicBaseComponents.TranslateTBPictureStatus(gdPicturePDF.GetStat());
        }

        //--------------------------------------------------------------------------------
        public TBPictureStatus SelectPage(int pageNo)
        {
            return TBPicBaseComponents.TranslateTBPictureStatus(gdPicturePDF.SelectPage(pageNo));
        }

        //--------------------------------------------------------------------------------
        public int ExtractPageImage(int ImageNo)
        {
            return gdPicturePDF.ExtractPageImage(ImageNo);
        }

        //--------------------------------------------------------------------------------
        public int RenderPageToGdPictureImage(float dpi, bool RenderFormFields)
        {
            return gdPicturePDF.RenderPageToGdPictureImage(dpi, RenderFormFields);
        }

        //--------------------------------------------------------------------------------
        public int RenderPageToGdPictureImageEx(float DPI, bool RenderFormFields)
        {
            return gdPicturePDF.RenderPageToGdPictureImageEx(DPI, RenderFormFields);
        }

        //--------------------------------------------------------------------------------
        public int RenderPageToGdPictureImageEx(float DPI, bool RenderFormFields, PixelFormat PixelFormat)
        {
            return gdPicturePDF.RenderPageToGdPictureImageEx(DPI, RenderFormFields, PixelFormat);
        }

        //--------------------------------------------------------------------------------
        public int GetFormFieldID(int FieldIdx)
        { return gdPicturePDF.GetFormFieldId(FieldIdx); }

        //--------------------------------------------------------------------------------
        public int GetFormFieldsCount()
        { return gdPicturePDF.GetFormFieldsCount(); }

        //--------------------------------------------------------------------------------
        public int GetFormFieldChildCount(int FieldID)
        { return gdPicturePDF.GetFormFieldChildCount(FieldID); }

        //--------------------------------------------------------------------------------
        public TBPicPdfFormFieldType GetFormFieldType(int FieldID)
        { return (TBPicPdfFormFieldType)gdPicturePDF.GetFormFieldType(FieldID); }

        //--------------------------------------------------------------------------------
        public string GetFormFieldValue(int FieldID)
        { return gdPicturePDF.GetFormFieldValue(FieldID); }


        //--------------------------------------------------------------------------------
        public TBPictureStatus GetFormFieldLocation(int FieldID, ref float Left, ref float Top, ref float Right, ref float Bottom)
        { return (TBPictureStatus)gdPicturePDF.GetFormFieldLocation(FieldID, ref Left, ref Top, ref Right, ref Bottom); }


        //--------------------------------------------------------------------------------
        public string GetFormFieldTitle(int FieldID)
        { return gdPicturePDF.GetFormFieldTitle(FieldID); }


        //--------------------------------------------------------------------------------
        public int GetFormFieldPage(int FieldID)
        { return gdPicturePDF.GetFormFieldPage(FieldID); }


        //--------------------------------------------------------------------------------
        public void SetOrigin(TBPicPdfOrigin Origin)
        { gdPicturePDF.SetOrigin((PdfOrigin)Origin); }

        //--------------------------------------------------------------------------------
        public int GetFormFieldChildID(int FieldID, int FieldIdx)
        { return gdPicturePDF.GetFormFieldChildID(FieldID, FieldIdx); }


        //--------------------------------------------------------------------------------
        public void SetMeasurementUnit(TBPicPdfMeasurementUnit UnitMode)
        { gdPicturePDF.SetMeasurementUnit((PdfMeasurementUnit)UnitMode); }

        //--------------------------------------------------------------------------------
        public string GetPageTextArea(float Left, float Top, float Width, float Height)
        {
            return gdPicturePDF.GetPageTextArea(Left, Top, Width, Height);
        }

        //--------------------------------------------------------------------------------
        public string GetPageTextWithCoords(string FieldSeparator)
        {
            return gdPicturePDF.GetPageTextWithCoords(FieldSeparator);
        }

        //--------------------------------------------------------------------------------
        public int GetPageImageCount()
        {
            return gdPicturePDF.GetPageImageCount();
        }

        //--------------------------------------------------------------------------------
        public string GetPageText()
        {
            return gdPicturePDF.GetPageText();
        }

        //--------------------------------------------------------------------------------
        public int GetPDFAConformance()
        {
            return gdPicturePDF.GetPDFAConformance();
        }

        //--------------------------------------------------------------------------------
        public TBPictureStatus NewPDF(bool PDFA = false)
        {
            return TBPicBaseComponents.TranslateTBPictureStatus(gdPicturePDF.NewPDF(PDFA));
        }

        //--------------------------------------------------------------------------------
        public string AddImageFromGdPictureImage(int ImageID, bool ImageMask, bool DrawImage)
        {
            return gdPicturePDF.AddImageFromGdPictureImage(ImageID, ImageMask, DrawImage);
        }

        //--------------------------------------------------------------------------------
        public int AddTextFormField(Single Left, Single Top, Single Width, Single Height, String FieldName, String Text, Boolean MultiLines, String FontResName, Single FontSize, Byte TextRed, Byte TextGreen, Byte TextBlue)
        {
            return gdPicturePDF.AddTextFormField(Left, Top, Width, Height, FieldName, Text, MultiLines, FontResName, FontSize, TextRed, TextGreen, TextBlue);
        }

        //--------------------------------------------------------------------------------
        public string AddStandardFont(TBPicPdfStandardFont StdFont)
        {
            return gdPicturePDF.AddStandardFont((PdfStandardFont)StdFont);
        }

        //--------------------------------------------------------------------------------
        public TBPictureStatus SaveToFile(string FilePath)
        {
            return TBPicBaseComponents.TranslateTBPictureStatus(gdPicturePDF.SaveToFile(FilePath));
        }
        //--------------------------------------------------------------------------------
        public TBPictureStatus SetFormFieldValue(int FieldID, string Value)
        {
            return TBPicBaseComponents.TranslateTBPictureStatus(gdPicturePDF.SetFormFieldValue(FieldID, Value));
        }

        //--------------------------------------------------------------------------------
        public TBPictureStatus ClonePage(TBPicPDF FromPDF, int PageNo)
        {
            return TBPicBaseComponents.TranslateTBPictureStatus(gdPicturePDF.ClonePage(FromPDF.gdPicturePDF, PageNo));
        }

        //--------------------------------------------------------------------------------
        public TBPictureStatus RemoveFormField(int FieldID)
        {
            return TBPicBaseComponents.TranslateTBPictureStatus(gdPicturePDF.RemoveFormField(FieldID));
        }


        //--------------------------------------------------------------------------------
        public TBPictureStatus SetFormFieldChecked(int FieldID, Boolean Checked)
        {

            return TBPicBaseComponents.TranslateTBPictureStatus(gdPicturePDF.SetFormFieldChecked(FieldID, Checked));
        }


    }
}
