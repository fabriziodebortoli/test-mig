using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Microarea.EasyAttachment.BusinessLogic;
using Microarea.EasyAttachment.Core;
using Microarea.TBPicComponents;

namespace Microarea.EasyAttachment.Components
{
    /// <summary>
    /// to scan multiple documents at once, putting giro barcode on each document (on first page if multipage)
    /// </summary>
    //--------------------------------------------------------------------------------
    public static class MultipleScanWithBarcodeAsSeparator
    {
        /// <summary>
        /// ogni documento, anche multipagina, ha sulla sua prima facciata un barcode, 
		/// qualunque esso sia viene intepretato come separatore in modo che non sia necessario inserire separatori tra i doc
        /// </summary>
        //--------------------------------------------------------------------------------
        public static List<string> ScanPdf(string filename, BarcodeManager bm)
        {
            List<string> acquiredFiles = new List<string>();
            int ImageID;
            TBPicImaging Imaging1 = new TBPicImaging();
            TBPicPDF ImagingPdf = new TBPicPDF();
		   
            int pdfCount = 0;
            int ptr = DateTime.Now.Millisecond;//?me.Handle?

            Imaging1.TwainOpenDefaultSource(new IntPtr(ptr));
            Imaging1.TwainSetHideUI(false);
            Imaging1.TwainSetAutoFeed(true); //Set AutoFeed Enabled
            Imaging1.TwainSetAutoScan(true);//To  achieve the maximum scanning rate
            string acquiredFilePath = Path.Combine(bm.DMSOrchestrator.EasyAttachmentTempPath, filename);
            ImagingPdf.NewPDF(true); // creo il file PDF (formato PDF/A 1-b compatibile)
            
            do
            {
                ImageID = Imaging1.TwainAcquireToGdPictureImage(new IntPtr(ptr));
                if (ImageID == 0)
                    continue;

                List<Barcode> barcodes = bm.GetBarcodesFromImage(ImageID);
                if (barcodes == null || barcodes.Count == 0  )  
       
                    //nessun barcode trovato, addo pagina
                    ImagingPdf.AddImageFromGdPictureImage(ImageID, false, true);
                else
                {
                    bool found = false;
                    foreach (Barcode b in barcodes)
                    {
                        found |= (!(b.Status == BarcodeStatus.TypeNotValid ||
                            !bm.IsValidBarcodeType(b.Type) ||
                             !bm.IsValidEABarcodeValue(b.Value)));
                    }
                    if (!found)
                        //nessun barcode trovato, addo pagina
                        ImagingPdf.AddImageFromGdPictureImage(ImageID, false, true);
                    else
                    {
                        if (ImagingPdf.GetPageCount() > 0)
                        {
                            //A separator has been detected, we start saves & start giro PDF
                            pdfCount += 1;
                            string completepath = acquiredFilePath + "_" + pdfCount.ToString() + FileExtensions.DotPdf;
                            ImagingPdf.SaveToFile(completepath);
                            acquiredFiles.Add(completepath);
                            ImagingPdf.CloseDocument();
                            // creo il file PDF (formato PDF/A 1-b compatibile)
                            ImagingPdf.NewPDF(true); //nuovo giro cui aggiungo subito la pagina corrente col barcode trovato
                            ImagingPdf.AddImageFromGdPictureImage(ImageID, false, true);
                        }
                        else
                            ImagingPdf.AddImageFromGdPictureImage(ImageID, false, true);// caso primissima pagina con bc
                    }
                }
                Imaging1.ReleaseGdPictureImage(ImageID);

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
            return acquiredFiles;
        }
        
        //--------------------------------------------------------------------------------
        public static List<string> ScanTif(string filename, BarcodeManager bm)
        {
            List<string> acquiredFiles = new List<string>();
            int ImageID;
      
            int ptr = DateTime.Now.Millisecond;//?me.Handle?
          
            int fileCount = 0;
            int tiffID = -1;
            int nTIFFImageCount = 0;
            string completepath = null;
            string acquiredFilePath = Path.Combine(bm.DMSOrchestrator.EasyAttachmentTempPath, filename);

            TBPicImaging Imaging1 = new TBPicImaging();
            Imaging1.TwainOpenDefaultSource(new IntPtr(ptr));
            Imaging1.TwainSetHideUI(false);
            Imaging1.TwainSetAutoFeed(true); //Set AutoFeed Enabled
            Imaging1.TwainSetAutoScan(true);//To  achieve the maximum scanning rate

            Debug.WriteLine("******************** Start multiple scan");
            
            do
            {
                Debug.WriteLine("Leggo...");
                ImageID = Imaging1.TwainAcquireToGdPictureImage(new IntPtr(ptr));
                if (ImageID == 0)
                    continue;
                if (nTIFFImageCount == 0) 
                    tiffID = ImageID;

                List<Barcode> barcodes = bm.GetBarcodesFromImage(ImageID);
                if (barcodes == null || barcodes.Count == 0) //nessun barcode trovato, addo pagina
                {
                    // creo il file TIFF e aggiungo la prima pagina
                    Debug.WriteLine("nessun barcode trovato=" + ImageID);

                    if (nTIFFImageCount == 0)
                    {
                        fileCount++;
						completepath = acquiredFilePath + "_" + fileCount.ToString() + FileExtensions.DotTif;
                        Debug.WriteLine("creo doc=" + completepath);
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
                else
                {
                    Debug.WriteLine("barcode trovato=" + ImageID);
                    if (nTIFFImageCount>0)
                    { 
						//A separator has been detected, we start saves & start giro PDF
                        Debug.WriteLine("chiudo doc con pagine=" + nTIFFImageCount);
                        Imaging1.TiffCloseMultiPageFile(tiffID);
                        acquiredFiles.Add(completepath); 
                        nTIFFImageCount = 0;
                        tiffID = ImageID;//preparo per doc successivo
                    }
					// else// caso primissima pagina con bc
                    fileCount++;

					completepath = acquiredFilePath + "_" + fileCount.ToString() + FileExtensions.DotTif;
                    Debug.WriteLine("creo doc=" + completepath);
                    Imaging1.TiffSaveAsMultiPageFile(tiffID, completepath, TBPicTiffCompression.TiffCompressionAUTO);
                    Debug.WriteLine("Aggiungo a doc=" + fileCount);
					// Imaging1.TiffAddToMultiPageFile(tiffID, ImageID);
                    nTIFFImageCount ++;
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
            return acquiredFiles;
        }
    }
}
