using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Microarea.EasyAttachment.Core;
using Microarea.TBPicComponents;
using Microarea.EasyAttachment.BusinessLogic;

namespace Microarea.EasyAttachment.Components
{
    /// <summary>
    /// to scan multiple documents at once, putting a separator sheet with a barcode between each documents.
    /// </summary>
    //--------------------------------------------------------------------------------
    public class MultipleScanWithBarcodeSeparatorSheet
    {
        /// <summary>
        /// This code assumes that the first page in the feeder is a separator sheet.
        ///This code creates multipage PDF files. You can modify it easily in order to acquire into multipage TIFF files.
        /// </summary>
        //--------------------------------------------------------------------------------
        public static List<string> ScanPdf(string filename, BarcodeManager bm)
        {
            List<string> acquiredFiles = new List<string>();
            int ImageID;
            int TemplateSeparatorID = -1;
            TBPicImaging Imaging1 = new TBPicImaging();
            TBPicPDF ImagingPdf = new TBPicPDF();
		   
			const int MIN_CONFIDENCE = 92;
            int pdfCount = 0;
            int ptr = DateTime.Now.Millisecond;//?me.Handle?

            Imaging1.TwainOpenDefaultSource(new IntPtr(ptr));
            Imaging1.TwainSetHideUI(false);
            Imaging1.TwainSetAutoFeed(true); //Set AutoFeed Enabled
            Imaging1.TwainSetAutoScan(true);//To  achieve the maximum scanning rate

            string acquiredFilePath = Path.Combine(bm.DMSOrchestrator.EasyAttachmentTempPath, filename);
            //First STEP: We create a new document identifier template from a scan of the paper separator
            ImageID = Imaging1.TwainAcquireToGdPictureImage(new IntPtr(ptr));
            Debug.WriteLine("******************** Start multiple scan"); 
            int a = 0;
            //Step 2, Now we will scan all the document feeder into several PDF files (a new PDF is created if a paper separator is detected)
			ImagingPdf.NewPDF(true); // creo il file PDF (formato PDF/A 1-b compatibile)
           
			do
            {
                Debug.WriteLine("Leggo...");
                if (a == 0)//primo giro: la prima pagina è il separatore
                {   
					Debug.WriteLine("SEPARATORE ="+ImageID); 
                    TemplateSeparatorID = Imaging1.ADRCreateTemplateFromGdPictureImage(ImageID);
                    Imaging1.ReleaseGdPictureImage(ImageID);
                    a++;
                    continue;
                }

                //ogni pagina viene confrontata col separatore  e se viene ritenuta uguale si fa break e si salva il documento se no si aggiunge la pagina al documento
                ImageID = Imaging1.TwainAcquireToGdPictureImage(new IntPtr(ptr));
                if (ImageID != 0)
                {
                    int CloserTemplateID = Imaging1.ADRGetCloserTemplateForGdPictureImage(ImageID);
                    double confidence = Imaging1.ADRGetLastConfidence();
                    Debug.WriteLine("Confidence =" + confidence);
					if (CloserTemplateID == TemplateSeparatorID && confidence >= MIN_CONFIDENCE)
					{
						//A paper separator has been detected, we start saves & start a PDF
						if (ImagingPdf.GetPageCount() > 0)
						{
							Debug.WriteLine("trovato separatore=" + ImageID);
							pdfCount += 1;
							string completepath = acquiredFilePath + "_" + pdfCount.ToString() + FileExtensions.DotPdf;
							ImagingPdf.SaveToFile(completepath);
							acquiredFiles.Add(completepath);
							ImagingPdf.CloseDocument();
							ImagingPdf.NewPDF(true); // creo il file PDF (formato PDF/A 1-b compatibile)
						}
					}
					else
					{
						Debug.WriteLine("aggiungo page=" + ImageID);
						ImagingPdf.AddImageFromGdPictureImage(ImageID, false, true);
					}
                    Imaging1.ReleaseGdPictureImage(ImageID);
                }
            }
            while (Imaging1.TwainGetState() > TBPicTwainStatus.TWAIN_SOURCE_ENABLED);

            if (ImagingPdf.GetPageCount() > 0) //Saves the latest PDF, if any
            {
                pdfCount += 1;
				string completepath = acquiredFilePath + "_" + pdfCount.ToString() + FileExtensions.DotPdf;
                ImagingPdf.SaveToFile(completepath);
                acquiredFiles.Add(completepath);
                ImagingPdf.CloseDocument();
            }

            Imaging1.TwainCloseSource();
            Debug.WriteLine("******************** End multiple scan"); 
            return acquiredFiles;
        }

        //--------------------------------------------------------------------------------
        public static List<string> ScanTif(string filename, BarcodeManager bm)
        {
            List<string> acquiredFiles = new List<string>();
            int ImageID;
            int TemplateSeparatorID = -1; 
            const int MIN_CONFIDENCE = 92;
            int ptr = DateTime.Now.Millisecond;//?me.Handle?
            int giro = 0;
            int fileCount = 0;
            int tiffID = -1;
            int nTIFFImageCount = 0;
            string completepath = null;
            string acquiredFilePath = Path.Combine(bm.DMSOrchestrator.EasyAttachmentTempPath, filename);

            TBPicImaging Imaging1 = new TBPicImaging();
          
			//Imaging1.SetLicenseNumberUpgrade("4118208038890486058421763", "9121015862970845451491642");
            //Imaging1.SetLicenseNumberOCRTesseract("9121015862970845451491642");
            Imaging1.TwainOpenDefaultSource(new IntPtr(ptr));
            Imaging1.TwainSetHideUI(false);
            Imaging1.TwainSetAutoFeed(true); //Set AutoFeed Enabled
            Imaging1.TwainSetAutoScan(true);//To  achieve the maximum scanning rate
  
            Debug.WriteLine("******************** Start multiple scan");  

            ImageID = Imaging1.TwainAcquireToGdPictureImage(new IntPtr(ptr));

            do
            {
                Debug.WriteLine("Leggo...");
                if (giro == 0)//primo giro: la prima pagina è il separatore
                {
                    Debug.WriteLine("SEPARATORE =" + ImageID);
                    TemplateSeparatorID = Imaging1.ADRCreateTemplateFromGdPictureImage(ImageID);
                    Imaging1.ReleaseGdPictureImage(ImageID);
                    giro++;
                    continue;
                }
                
				// altri giri
                // ogni pagina viene confrontata col separatore e se viene ritenuta uguale si fa break e 
				// si salva il documento se no si aggiunge la pagina al documento
                ImageID = Imaging1.TwainAcquireToGdPictureImage(new IntPtr(ptr));
                if (ImageID == 0) continue;

                if (nTIFFImageCount == 0)
                    tiffID = ImageID;

                int CloserTemplateID = Imaging1.ADRGetCloserTemplateForGdPictureImage(ImageID);
                double confidence = Imaging1.ADRGetLastConfidence();
                Debug.WriteLine("Confidence =" + confidence);

                if (CloserTemplateID == TemplateSeparatorID && confidence >= MIN_CONFIDENCE)
                {
                    Debug.WriteLine("trovato separatore=" + ImageID);
                    //A paper separator has been detected, we start saves & start a PDF
                    if (nTIFFImageCount > 0)
                    { // creo il file TIFF e aggiungo la prima pagina

                        Debug.WriteLine("chiudo doc con pagine=" + nTIFFImageCount);
                        Imaging1.TiffCloseMultiPageFile(tiffID);
                        acquiredFiles.Add(completepath);
                    }
                        tiffID = -1;
                        nTIFFImageCount = 0;
                }
                else
                {// creo il file TIFF e aggiungo la prima pagina
                    Debug.WriteLine("pagina valida=" + ImageID);

                    if (nTIFFImageCount == 0)
                    {
                        fileCount++;
                        Debug.WriteLine("creo doc=" + fileCount);
                        completepath = acquiredFilePath + "_" + fileCount.ToString() + FileExtensions.DotTif;
                        Imaging1.TiffSaveAsMultiPageFile(tiffID, completepath, TBPicTiffCompression.TiffCompressionAUTO);
                        nTIFFImageCount++;
                    }
                    else
                    {
                        Debug.WriteLine("Aggiungo a doc=" + fileCount);
                        Imaging1.TiffAddToMultiPageFile(tiffID, ImageID);
                        nTIFFImageCount++;
                    }
                }

                Imaging1.ReleaseGdPictureImage(ImageID);
            }
            while (Imaging1.TwainGetState() > TBPicTwainStatus.TWAIN_SOURCE_ENABLED);

            if (nTIFFImageCount > 0) //Saves the latest PDF, if any
            {
                Debug.WriteLine("chiudo ULTIMO doc con pagine=" + nTIFFImageCount);
                Imaging1.TiffCloseMultiPageFile(tiffID);
                acquiredFiles.Add(completepath);
            }

            Imaging1.TwainCloseSource();
            Debug.WriteLine("******************** End multiple scan");
            return acquiredFiles;
        }
    }
}
