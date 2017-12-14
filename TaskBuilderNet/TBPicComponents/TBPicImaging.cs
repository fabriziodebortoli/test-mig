using System;
using GdPicture12;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;

namespace Microarea.TBPicComponents
{
    //=========================================================================
    public class TBPicImaging
    {
        GdPictureImaging gdPicture;

        //--------------------------------------------------------------------------------
        public TBPicImaging()
        {
            TBPicBaseComponents.Register();
            gdPicture = new GdPictureImaging();
        }

        //--------------------------------------------------------------------------------
        public TBPictureStatus GetStat()
        {
            return TBPicBaseComponents.TranslateTBPictureStatus(gdPicture.GetStat());
        }

        //--------------------------------------------------------------------------------
        public TBPicDocumentFormat GetDocumentFormatFromFile(string filepath)
        {
            return (TBPicDocumentFormat)GdPictureDocumentUtilities.GetDocumentFormatFromFileName(filepath);
        }

        //--------------------------------------------------------------------------------
        public TBPictureStatus ReleaseGdPictureImage(int imageId)
        {
            return TBPicBaseComponents.TranslateTBPictureStatus(gdPicture.ReleaseGdPictureImage(imageId));
        }

        //--------------------------------------------------------------------------------
        public int CreateGdPictureImageFromFile(string filePath)
        {
            return gdPicture.CreateGdPictureImageFromFile(filePath);
        }

        //--------------------------------------------------------------------------------
        public void SetROI(int left, int top, int width, int height)
        {
            gdPicture.SetROI(left, top, width, height);
        }

        //--------------------------------------------------------------------------------
        public TBPictureStatus OCRTesseractReinit()
        {
            return TBPicBaseComponents.TranslateTBPictureStatus(gdPicture.OCRTesseractReinit());
        }

        //--------------------------------------------------------------------------------
        public string OCRTesseractDoOCR(int ImageID, string Dictionary, string DictionaryPath, string CharWhiteList, int Timeout = 0)
        {
            return gdPicture.OCRTesseractDoOCR(ImageID, Dictionary, DictionaryPath, CharWhiteList, Timeout);
        }

        //--------------------------------------------------------------------------------
        public void OCRTesseractClear()
        {
            gdPicture.OCRTesseractClear();
        }

        //--------------------------------------------------------------------------------
        public TBPictureStatus Scale(int imageID, float scalePercent, InterpolationMode interpolationMode)
        {
            return TBPicBaseComponents.TranslateTBPictureStatus(gdPicture.Scale(imageID, scalePercent, interpolationMode));
        }

        //--------------------------------------------------------------------------------
        public TBPictureStatus TiffSelectPage(int ImageID, int Page) { return TBPicBaseComponents.TranslateTBPictureStatus(gdPicture.TiffSelectPage(ImageID, Page)); }

        //--------------------------------------------------------------------------------
        public bool TiffIsMultiPage(int ImageID) { return gdPicture.TiffIsMultiPage(ImageID); }

        //--------------------------------------------------------------------------------
        public int TiffGetPageCount(int ImageID) { return gdPicture.TiffGetPageCount(ImageID); }

        //--------------------------------------------------------------------------------
        public int TiffCreateMultiPageFromFile(string FilePath, bool LoadInMemory = false) { return gdPicture.TiffCreateMultiPageFromFile(FilePath, LoadInMemory); }

        //--------------------------------------------------------------------------------
        public int CreateGdPictureImageFromByteArray(byte[] Data) { return gdPicture.CreateGdPictureImageFromByteArray(Data); }

        //--------------------------------------------------------------------------------
        public TBPicBarcode1DReaderType Barcode1DReaderGetBarcodeType(int BarcodeNo)
        {
            return TBPicBaseComponents.TranslateTBPicBarcode1DReaderType(gdPicture.Barcode1DReaderGetBarcodeType(BarcodeNo));
        }

        //--------------------------------------------------------------------------------
        public string Barcode1DReaderGetBarcodeValue(int barcodeIdx)
        {
            return gdPicture.Barcode1DReaderGetBarcodeValue(barcodeIdx);
        }

        //--------------------------------------------------------------------------------
        public string Barcode2DReaderGetBarcodeValue(int barcodeIdx)
        {
            return gdPicture.BarcodeDataMatrixReaderGetBarcodeValue(barcodeIdx);
        }

        //--------------------------------------------------------------------------------
        public TBPictureStatus Barcode1DReaderDoScan(int ImageID)
        {
            return TBPicBaseComponents.TranslateTBPictureStatus(gdPicture.Barcode1DReaderDoScan(ImageID));
        }

        //--------------------------------------------------------------------------------
        public TBPictureStatus Barcode2DReaderDoScan(int ImageID)
        {
            return TBPicBaseComponents.TranslateTBPictureStatus(gdPicture.BarcodeDataMatrixReaderDoScan(ImageID));
        }

        //--------------------------------------------------------------------------------
        public void Barcode1DReaderClear() { gdPicture.Barcode1DReaderClear(); }

        //--------------------------------------------------------------------------------
        public void Barcode2DReaderClear(TBPicBarcode2DWriterType bType)
        {

            gdPicture.BarcodeDataMatrixReaderClear();

            switch (bType)
            {
                case TBPicBarcode2DWriterType.Barcode2DWriterDataMatrix:
                    {
                        gdPicture.BarcodeDataMatrixReaderClear();
                        break;
                    }


                case TBPicBarcode2DWriterType.Barcode2DWriterMicroQR:
                    {
                        gdPicture.BarcodeMicroQRReaderClear();
                        break;
                    }

                case TBPicBarcode2DWriterType.Barcode2DWriterQR:
                    {
                        gdPicture.BarcodeQRReaderClear();
                        break;
                    }

                case TBPicBarcode2DWriterType.Barcode2DWriterPDF417:
                    {
                        gdPicture.BarcodePDF417ReaderClear();
                        break;
                    }
            }
        }

        //--------------------------------------------------------------------------------
        public int Barcode1DReaderGetBarcodeCount() { return gdPicture.Barcode1DReaderGetBarcodeCount(); }

        //--------------------------------------------------------------------------------
        public int Barcode2DReaderGetBarcodeCount(TBPicBarcode2DWriterType bType)
        {

            int result = -1;
            switch (bType)
            {
                case TBPicBarcode2DWriterType.Barcode2DWriterDataMatrix:
                    {
                        result = gdPicture.BarcodeDataMatrixReaderGetBarcodeCount();
                        break;
                    }


                case TBPicBarcode2DWriterType.Barcode2DWriterMicroQR:
                    {
                        result = gdPicture.BarcodeMicroQRReaderGetBarcodeCount();
                        break;
                    }

                case TBPicBarcode2DWriterType.Barcode2DWriterQR:
                    {
                        result = gdPicture.BarcodeQRReaderGetBarcodeCount();
                        break;
                    }

                case TBPicBarcode2DWriterType.Barcode2DWriterPDF417:
                    {
                        result = gdPicture.BarcodePDF417ReaderGetBarcodeCount();
                        break;
                    }
            }

            return result;

        }

        //--------------------------------------------------------------------------------
        public int CreateClonedGdPictureImageI(int ImageID) { return gdPicture.CreateClonedGdPictureImage(ImageID); }

        //--------------------------------------------------------------------------------
        public TBPictureStatus TiffSaveAsMultiPageFile(int ImageID, string FilePath, TBPicTiffCompression Compression)
        {
            return TBPicBaseComponents.TranslateTBPictureStatus(gdPicture.TiffSaveAsMultiPageFile(ImageID, FilePath, (TiffCompression)(int)(Compression)));
        }

        //--------------------------------------------------------------------------------
        public TBPictureStatus TiffAddToMultiPageFile(int ImageID, int ImageToAddID) { return TBPicBaseComponents.TranslateTBPictureStatus(gdPicture.TiffAddToMultiPageFile(ImageID, ImageToAddID)); }

        //--------------------------------------------------------------------------------
        public void ClearGdPicture() { gdPicture.ClearGdPicture(); }

        //--------------------------------------------------------------------------------
        public TBPictureStatus Barcode1DWrite(int ImageID, TBPicBarcode1DWriterType BarcodeType, int nNarrowBar, int nHeight, string Data, int DstLeft, int DstTop, int DstWidth, int DstHeight, Color FillColor, TBPicBarcodeAlign Alignment, TBPicBarcodeVerticalAlign VAlignment, bool isPrinting)
        {
            int width = gdPicture.Barcode1DWriteGetMinWidth((Barcode1DWriterType)(int)BarcodeType, Data);
            if (width <= 0)
                return TBPictureStatus.BarcodeSizeZero;
            if (width > DstWidth)
                return TBPictureStatus.BarcodeInvalidDestinationSize;
            if ((width * nNarrowBar) > DstWidth)
                return TBPictureStatus.BarcodeInvalidDestinationBarSize;
            if(nHeight > DstHeight)
                return TBPictureStatus.BarcodeInvalidDestinationBarSize;

            width *= nNarrowBar;
            int diff = (DstWidth - width) / 2 - 1;
            if (diff > 0)
            {
                DstWidth -= (2 * diff);
                if (Alignment == TBPicBarcodeAlign.BarcodeAlignCenter)
                {
                    DstLeft += diff;
                }
                else if (Alignment == TBPicBarcodeAlign.BarcodeAlignLeft)
                {
                }
                else if (Alignment == TBPicBarcodeAlign.BarcodeAlignRight)
                {
                    DstLeft += (2 * diff);
                }
            }
            if(nHeight > 0)
            {
                diff = (DstHeight - nHeight) / 2 - 1;
                if (diff > 0)
                {
                    DstHeight -= (2 * diff);
                    if (VAlignment == TBPicBarcodeVerticalAlign.BarcodeAlignCenter)
                    {
                        DstTop += diff;
                    }
                    else if (VAlignment == TBPicBarcodeVerticalAlign.BarcodeAlignTop)
                    {
                    }
                    else if (VAlignment == TBPicBarcodeVerticalAlign.BarcodeAlignBottom)
                    {
                        DstTop += (2 * diff);
                    }
                }
            }

            GdPictureStatus status = gdPicture.Barcode1DWrite(ImageID, (Barcode1DWriterType)(int)BarcodeType, Data, DstLeft, DstTop, DstWidth, DstHeight, FillColor, (BarcodeAlign)(int)Alignment);
            
            return TBPicBaseComponents.TranslateTBPictureStatus(status);
        }

        public TBPictureStatus Barcode2DWrite(TBPicBarcode2DWriterType bType, int ImageID, string Data, int QuietZone, int DstLeft, int DstTop, int dstWidth, int dstHeight, Color FillColor, Color BackColor,
            int nEncodingType = -1, int nVersion = -1, int nModuleSize = -1, int nErrCorrLevel = -1, int nRowHeight = -1,
            int nRows = -1, int nColumns = -1)
        {
            TBPictureStatus status = TBPictureStatus.BarcodeInvalidData;

            switch (bType)
            {
                case TBPicBarcode2DWriterType.Barcode2DWriterDataMatrix:
                    {
                        BarcodeDataMatrixVersion version = nVersion > -1 ? (BarcodeDataMatrixVersion)nVersion : BarcodeDataMatrixVersion.BarcodeDataMatrixVersionAuto;
                        BarcodeDataMatrixEncodingMode encodingMode = nEncodingType > -1 ? (BarcodeDataMatrixEncodingMode)nEncodingType : BarcodeDataMatrixEncodingMode.BarcodeDataMatrixEncodingModeUndefined;
                        status = GetBarcodeMatrixMinimumSize(Data, Math.Min(dstWidth, dstHeight), QuietZone, encodingMode, ref version, ref nModuleSize);
                        if (status == TBPictureStatus.OK)
                        {
                            int effectiveWidth = GetDataMatrixModuleNumFromVersion(version) * nModuleSize;

                            DstLeft = Math.Abs(dstWidth / 2 - effectiveWidth / 2);
                            DstTop = Math.Abs(dstHeight / 2 - effectiveWidth / 2);

                            status = TBPicBaseComponents.TranslateTBPictureStatus(
                                gdPicture.BarcodeDataMatrixWrite
                                                                (
                                                                ImageID, Data,
                                                                encodingMode,
                                                                ref version,
                                                                QuietZone, nModuleSize, DstLeft, DstTop, 0, FillColor, BackColor   // no refactoring of DstLeft
                                                                 ));
                        }
                        break;
                    }


                case TBPicBarcode2DWriterType.Barcode2DWriterMicroQR:
                    {
                        int version = nVersion > -1 ? nVersion : 0 ; // 0 = auto
                        BarcodeQREncodingMode encodingMode = nEncodingType > -1 ? (BarcodeQREncodingMode)nEncodingType : BarcodeQREncodingMode.BarcodeQREncodingModeAlphaNumeric;
                        BarcodeMicroQRErrorCorrectionLevel corrLevel = nErrCorrLevel > -1 ? (BarcodeMicroQRErrorCorrectionLevel)nErrCorrLevel : BarcodeMicroQRErrorCorrectionLevel.BarcodeMicroQRErrorCorrectionLevelM;

                        status = GetBarcodeMicroQRMinimumSize(Data, Math.Min(dstWidth, dstHeight), QuietZone, encodingMode, corrLevel, ref version, ref nModuleSize);

                        if (status == TBPictureStatus.OK)
                        {
                            int effectiveWidth = nModuleSize * ((version - 1) * 2 + 11); ;

                            DstLeft = Math.Abs(dstWidth / 2 - effectiveWidth / 2);
                            DstTop = Math.Abs(dstHeight / 2 - effectiveWidth / 2);

                            status = TBPicBaseComponents.TranslateTBPictureStatus(
                                    gdPicture.BarcodeMicroQRWrite(
                                                                    ImageID, Data, encodingMode,
                                                                    corrLevel, version,
                                                                    QuietZone, nModuleSize, DstLeft, DstTop,  0, FillColor, BackColor       

                                                                    ));
                        }
                        break;
                    }

                case TBPicBarcode2DWriterType.Barcode2DWriterQR:
                    {
                        int version = nVersion > -1 ? nVersion : 0; // 0 = auto
                        BarcodeQREncodingMode encodingMode = nEncodingType > -1 ? (BarcodeQREncodingMode)nEncodingType : BarcodeQREncodingMode.BarcodeQREncodingModeAlphaNumeric;
                        BarcodeQRErrorCorrectionLevel corrLevel = nErrCorrLevel>-1 ? (BarcodeQRErrorCorrectionLevel) nErrCorrLevel : BarcodeQRErrorCorrectionLevel.BarcodeQRErrorCorrectionLevelM;

                        status = GetBarcodeQRMinimumSize(Data, Math.Min(dstWidth, dstHeight), QuietZone, encodingMode, corrLevel, ref version, ref nModuleSize);

                        if (status == TBPictureStatus.OK)
                        {
                            int effectiveWidth = nModuleSize * ((version - 1) * 4 + 21); ;
            
                            DstLeft = Math.Abs(dstWidth / 2 - effectiveWidth / 2);
                            DstTop = Math.Abs(dstHeight / 2 - effectiveWidth / 2);

                            status = TBPicBaseComponents.TranslateTBPictureStatus(
                                       gdPicture.BarcodeQRWrite(
                                                                       ImageID, Data, encodingMode,
                                                                       corrLevel, version,
                                                                       QuietZone, nModuleSize, DstLeft, DstTop, 0, FillColor, BackColor

                                                                       ));
                        }
                        break;
                    }

                case TBPicBarcode2DWriterType.Barcode2DWriterPDF417:
                    {
                        int rows = nRows > -1 ? nRows : 0;  // 0 = auto
                        int cols = nColumns > -1 ? nColumns : 0; // 0 = auto
                        int moduleWidth = nModuleSize;

                        BarcodePDF417EncodingMode encodingMode = nEncodingType > -1 ? (BarcodePDF417EncodingMode)nEncodingType : BarcodePDF417EncodingMode.BarcodePDF417EncodingModeUndefined;
                        BarcodePDF417ErrorCorrectionLevel corrLevel = nErrCorrLevel > -1 ? (BarcodePDF417ErrorCorrectionLevel)nErrCorrLevel : BarcodePDF417ErrorCorrectionLevel.BarcodePDF417ErrorCorrectionLevelAuto;
                        status = GetBarcodePdf417MinimumSize(Data, encodingMode, ref corrLevel, ref rows, ref cols, QuietZone, ref moduleWidth, ref nRowHeight, dstWidth, dstHeight);
                        if (status == TBPictureStatus.OK)
                        {

                            int effectiveWidth = 0;//moduleWidth * (cols * 17 + 69);
                            int effectiveHeight = 0;// rows * rowHeight;

                            gdPicture.BarcodePDF417GetSize(Data, encodingMode, ref corrLevel, ref rows, ref cols, QuietZone, moduleWidth, nRowHeight, ref effectiveWidth, ref effectiveHeight);
                            if (effectiveWidth < dstWidth)
                                DstLeft = dstWidth / 2 - effectiveWidth / 2;


                            if (effectiveHeight < dstHeight)
                                DstTop = dstHeight / 2 - effectiveHeight / 2;

                            status = TBPicBaseComponents.TranslateTBPictureStatus(
                                        gdPicture.BarcodePDF417Write(
                                                                        ImageID, Data, encodingMode,
                                                                        corrLevel, rows, cols,
                                                                        QuietZone, moduleWidth, nRowHeight, DstLeft, DstTop, 0, FillColor, BackColor

                                                                        ));
                        }
                        break;
                    }
            }
            return status;
        }

        //--------------------------------------------------------------------------------
        private TBPictureStatus GetBarcodeQRMinimumSize(String data, int width, int QuietZone, BarcodeQREncodingMode encodingMode, BarcodeQRErrorCorrectionLevel corrLevel, ref int version, ref int moduleSize)
        {
            int realModuleSize = moduleSize > 1 ? moduleSize : 1;
            int minSize = gdPicture.BarcodeQRGetSize(data, encodingMode, corrLevel, ref version, QuietZone, realModuleSize);
            if (minSize <= width)
            {
                if (moduleSize < 1)
                    moduleSize = width / ((version - 1) * 4 + 21);
                return TBPictureStatus.OK;
            }

            return TBPictureStatus.BarcodeInvalidDestinationSize;
        }

        //--------------------------------------------------------------------------------
        private TBPictureStatus GetBarcodeMicroQRMinimumSize(String data, int width, int QuietZone, BarcodeQREncodingMode encodingMode, BarcodeMicroQRErrorCorrectionLevel corrLevel, ref int version, ref int moduleSize)
        {
            int realModuleSize = moduleSize > 1 ? moduleSize : 1;
            int minSize = gdPicture.BarcodeMicroQRGetSize(data, encodingMode, corrLevel, ref version, QuietZone, realModuleSize);
            if (minSize <= width)
            {
                if (moduleSize < 1)
                    moduleSize = width / ((version - 1) * 2 + 11);
                return TBPictureStatus.OK;
            }

            return TBPictureStatus.BarcodeInvalidDestinationSize;
        }

        //--------------------------------------------------------------------------------
        private TBPictureStatus GetBarcodeMatrixMinimumSize(String data, int width, int QuietZone, BarcodeDataMatrixEncodingMode encodingMode, ref BarcodeDataMatrixVersion version, ref int moduleSize)
        {

            int realModuleSize = moduleSize > 1 ? moduleSize : 1;
            TBPictureStatus status = TBPictureStatus.OK;

            int minWidth = 0, minHeight = 0;
            status = TBPicBaseComponents.TranslateTBPictureStatus(gdPicture.BarcodeDataMatrixGetSize(data, encodingMode, ref version, QuietZone, realModuleSize, ref minWidth, ref minHeight));
            if (status != TBPictureStatus.OK)
                return status;

            int dataMatrixModuleNum = GetDataMatrixModuleNumFromVersion(version);

            if (minWidth <= width)
            {
                if (moduleSize < 1)
                    moduleSize = width / dataMatrixModuleNum;
                return status;
            }

            return TBPictureStatus.BarcodeInvalidDestinationSize;
        }

        //--------------------------------------------------------------------------------

        private int GetDataMatrixModuleNumFromVersion(BarcodeDataMatrixVersion version)
        {
            switch (version)
            {
                case BarcodeDataMatrixVersion.BarcodeDataMatrixVersion010x010:
                    return 10;
                case BarcodeDataMatrixVersion.BarcodeDataMatrixVersion012x012:
                    return 12;
                case BarcodeDataMatrixVersion.BarcodeDataMatrixVersion014x014:
                    return 14;
                case BarcodeDataMatrixVersion.BarcodeDataMatrixVersion016x016:
                    return 16;
                case BarcodeDataMatrixVersion.BarcodeDataMatrixVersion008x018:
                case BarcodeDataMatrixVersion.BarcodeDataMatrixVersion018x018:
                    return 18;
                case BarcodeDataMatrixVersion.BarcodeDataMatrixVersion020x020:
                    return 20;
                case BarcodeDataMatrixVersion.BarcodeDataMatrixVersion022x022:
                    return 22;
                case BarcodeDataMatrixVersion.BarcodeDataMatrixVersion024x024:
                    return 24;
                case BarcodeDataMatrixVersion.BarcodeDataMatrixVersion012x026:
                case BarcodeDataMatrixVersion.BarcodeDataMatrixVersion026x026:
                    return 26;
                case BarcodeDataMatrixVersion.BarcodeDataMatrixVersion008x032:
                case BarcodeDataMatrixVersion.BarcodeDataMatrixVersion032x032:
                    return 32;
                case BarcodeDataMatrixVersion.BarcodeDataMatrixVersion016x036:
                case BarcodeDataMatrixVersion.BarcodeDataMatrixVersion012x036:
                case BarcodeDataMatrixVersion.BarcodeDataMatrixVersion036x036:
                    return 36;
                case BarcodeDataMatrixVersion.BarcodeDataMatrixVersion040x040:
                    return 40;
                case BarcodeDataMatrixVersion.BarcodeDataMatrixVersion044x044:
                    return 44;
                case BarcodeDataMatrixVersion.BarcodeDataMatrixVersion016x048:
                case BarcodeDataMatrixVersion.BarcodeDataMatrixVersion048x048:
                    return 48;
                case BarcodeDataMatrixVersion.BarcodeDataMatrixVersion052x052:
                    return 52;
                case BarcodeDataMatrixVersion.BarcodeDataMatrixVersion064x064:
                    return 64;
                case BarcodeDataMatrixVersion.BarcodeDataMatrixVersion072x072:
                    return 72;
                case BarcodeDataMatrixVersion.BarcodeDataMatrixVersion080x080:
                    return 80;
                case BarcodeDataMatrixVersion.BarcodeDataMatrixVersion088x088:
                    return 88;
                case BarcodeDataMatrixVersion.BarcodeDataMatrixVersion096x096:
                    return 96;
                case BarcodeDataMatrixVersion.BarcodeDataMatrixVersion104x104:
                    return 104;
                case BarcodeDataMatrixVersion.BarcodeDataMatrixVersion120x120:
                    return 120;
                case BarcodeDataMatrixVersion.BarcodeDataMatrixVersion132x132:
                    return 132;
                case BarcodeDataMatrixVersion.BarcodeDataMatrixVersion144x144:
                    return 144;
                default:
                    return 0;
            }
        }

        //--------------------------------------------------------------------------------
        private TBPictureStatus GetBarcodePdf417MinimumSize(String data, BarcodePDF417EncodingMode encodingMode, ref BarcodePDF417ErrorCorrectionLevel errorCorrectionLevel, ref int rows, ref int cols, int QuietZone, ref int moduleWidth, ref int rowHeight, int width, int height)
        {
            int realModuleWidth = moduleWidth > 1 ? moduleWidth : 1;
            int realRowHeight = rowHeight > 1 ? rowHeight : 1;

            TBPictureStatus status = TBPictureStatus.OK;

            int minWidth = 0, minHeight = 0;
            status = TBPicBaseComponents.TranslateTBPictureStatus(gdPicture.BarcodePDF417GetSize(data, encodingMode, ref errorCorrectionLevel, ref rows, ref cols, QuietZone, realModuleWidth, realRowHeight, ref minWidth, ref minHeight));

            if (status != TBPictureStatus.OK)
                return status;

            if (minWidth <= width && minHeight <= height)
            {
                // formula found on PDF417 tutorials
                //  symbolwidth = (numCols*17+69)*moduleWidth
               // symbolHeight = numRows * yHeight
               if(moduleWidth < 1)
                    moduleWidth =Math.Max(width / (cols*17+69), realModuleWidth);
                if (rowHeight < 1)
                    rowHeight = Math.Max(height / rows, realRowHeight);
                
                return status;
            }

            return TBPictureStatus.BarcodeInvalidDestinationSize;
        }
        //--------------------------------------------------------------------------------
        public int CreateNewGdPictureImage(int Width, int Height, PixelFormat PixelFormat, Color BackColor)
        {
            return gdPicture.CreateNewGdPictureImage(Width, Height, PixelFormat, BackColor);
        }

        //--------------------------------------------------------------------------------
        public TBPictureStatus DrawText(int ImageID, string Text, int DstLeft, int DstTop, float FontSize, TBPicFontStyle FontStyle, Color TextColor, string FontName, bool AntiAlias)
        {
            return TBPicBaseComponents.TranslateTBPictureStatus(gdPicture.DrawText(ImageID, Text, DstLeft, DstTop, FontSize, (GdPicture12.FontStyle)(int)FontStyle, TextColor, FontName, AntiAlias));
        }

        //--------------------------------------------------------------------------------
        public bool TwainOpenDefaultSource(IntPtr HANDLE)
        {
            return gdPicture.TwainOpenDefaultSource(HANDLE);
        }

        //--------------------------------------------------------------------------------
        public void TwainSetHideUI(bool Hide)
        {
            gdPicture.TwainSetHideUI(Hide);
        }

        //--------------------------------------------------------------------------------
        public bool TwainSetAutoFeed(bool AutoFeed)
        {
            return gdPicture.TwainSetAutoFeed(AutoFeed);
        }

        //--------------------------------------------------------------------------------
        public bool TwainSetAutoScan(bool AutoScan)
        {
            return gdPicture.TwainSetAutoScan(AutoScan);
        }

        //--------------------------------------------------------------------------------
        public int TwainAcquireToGdPictureImage(IntPtr HANDLE)
        {
            return gdPicture.TwainAcquireToGdPictureImage(HANDLE);
        }

        //--------------------------------------------------------------------------------
        public TBPicTwainStatus TwainGetState()
        {
            return (TBPicTwainStatus)(int)gdPicture.TwainGetState();
        }

        //--------------------------------------------------------------------------------
        public bool TwainCloseSource()
        {
            return gdPicture.TwainCloseSource();
        }

        //--------------------------------------------------------------------------------
        public TBPictureStatus TiffCloseMultiPageFile(int ImageID)
        {
            return (TBPictureStatus)(int)gdPicture.TiffCloseMultiPageFile(ImageID);
        }

        //--------------------------------------------------------------------------------
        public int ADRCreateTemplateFromGdPictureImage(int ImageID)
        {
            return gdPicture.ADRCreateTemplateFromGdPictureImage(ImageID);
        }

        //--------------------------------------------------------------------------------
        public double ADRGetLastConfidence()
        {
            return gdPicture.ADRGetLastConfidence();
        }

        //--------------------------------------------------------------------------------
        public bool TwainSelectSource(IntPtr HANDLE)
        {
            return gdPicture.TwainSelectSource(HANDLE);
        }

        //--------------------------------------------------------------------------------
        public TbPicTwainResultCode TwainGetLastResultCode()
        {
            return (TbPicTwainResultCode)/*(int)*/gdPicture.TwainGetLastResultCode();
        }

        //--------------------------------------------------------------------------------
        public string TwainGetDefaultSourceName(IntPtr HANDLE)
        {
            return gdPicture.TwainGetDefaultSourceName(HANDLE);
        }

        //--------------------------------------------------------------------------------
        public bool TwainOpenSource(IntPtr HANDLE, string SourceName)
        {
            return gdPicture.TwainOpenSource(HANDLE, SourceName);
        }

        //--------------------------------------------------------------------------------
        public bool TwainCloseSourceManager(IntPtr HANDLE)
        {
            return gdPicture.TwainCloseSourceManager(HANDLE);
        }

        //--------------------------------------------------------------------------------
        public bool TwainUnloadSourceManager(IntPtr HANDLE)
        {
            return gdPicture.TwainUnloadSourceManager(HANDLE);
        }

        //--------------------------------------------------------------------------------
        public bool TwainSetIndicators(bool ShowIndicator)
        {
            return gdPicture.TwainSetIndicators(ShowIndicator);
        }

        //--------------------------------------------------------------------------------
        public TBPictureStatus SaveAsJPEG(int ImageID, string FilePath)
        {
            return (TBPictureStatus)gdPicture.SaveAsJPEG(ImageID, FilePath);
        }

        //--------------------------------------------------------------------------------
        public TBPictureStatus SaveAsPNG(int ImageID, string FilePath)
        {
            return (TBPictureStatus)gdPicture.SaveAsPNG(ImageID, FilePath);
        }

        //--------------------------------------------------------------------------------
        public TBPictureStatus SaveAsStream(int ImageID, Stream Stream, TBPicDocumentFormat ImageFormat, int EncoderParameter)
        {
            return (TBPictureStatus)gdPicture.SaveAsStream(ImageID, Stream, (GdPicture12.DocumentFormat)ImageFormat, EncoderParameter);
        }

        //--------------------------------------------------------------------------------
        public TBPictureStatus SaveAsBMP(int ImageID, string FilePath)
        {
            return (TBPictureStatus)gdPicture.SaveAsBMP(ImageID, FilePath);
        }

        //--------------------------------------------------------------------------------
        public TBPictureStatus SaveAsGIF(int ImageID, string FilePath)
        {
            return (TBPictureStatus)gdPicture.SaveAsGIF(ImageID, FilePath);
        }

        //--------------------------------------------------------------------------------
        public TBPictureStatus ConvertTo1Bpp(int ImageID) { return (TBPictureStatus)gdPicture.ConvertTo1Bpp(ImageID); }
        //--------------------------------------------------------------------------------
        public TBPictureStatus ConvertTo1Bpp(int ImageID, byte Threshold) { return (TBPictureStatus)gdPicture.ConvertTo1Bpp(ImageID, Threshold); }

        //--------------------------------------------------------------------------------
        public bool TwainIsAvailable()
        {
            return gdPicture.TwainIsAvailable();
        }

        //--------------------------------------------------------------------------------
        public int ADRGetCloserTemplateForGdPictureImage(int ImageID)
        {
            return gdPicture.ADRGetCloserTemplateForGdPictureImage(ImageID);
        }

        //--------------------------------------------------------------------------------
        public int CreateThumbnail(int ImageID, int Width, int Height)
        {
            return gdPicture.CreateThumbnail(ImageID, Width, Height);
        }


        //--------------------------------------------------------------------------------
        private int GetImageIdFromTypedBarcode
                                    (
                                        TBPicBarcode1DWriterType type, int nNarrowBar, int nHeight,
                                        string value, string humanValue,
                                        int dstLeft, int dstRight,int dstWidth, int dstHeight,
                                        ref TBPictureStatus gStatus, 
                                        Color txtColor, Color bkColor, 
                                        bool showLabel, bool isPrinting,
                                        TBPicBarcodeAlign align = TBPicBarcodeAlign.BarcodeAlignCenter,
                                        TBPicBarcodeVerticalAlign vAlign = TBPicBarcodeVerticalAlign.BarcodeAlignCenter,
                                        String fontName ="Arial", int fontSize=8
                                   )
        {
            int imageId = -1;
            // se il value e' empty non procedo
            if (string.IsNullOrWhiteSpace(value))
            {
                ClearGdPicture();
                return imageId;
            }

            gStatus = TBPictureStatus.InvalidBarCode;

            try
            {
                //margine di 3px sui lati
                int margin = 0; //bkColor = Color.Aqua;

                // Crea una GdPicture Image vuota
                imageId = gdPicture.CreateNewGdPictureImage(dstWidth/*width*/ - 2*margin, dstHeight/*height*/, PixelFormat.Format24bppRgb, bkColor);
                if (imageId > 0)
                {
                    int hFont = 0;
                    fontSize = System.Math.Abs(fontSize);

                    if (/*gStatus == TBPictureStatus.OK &&*/ showLabel)
					{
                        // calculates the optimal font size
                        int fntWidth = (int) gdPicture.GetTextWidth(imageId, humanValue, fontName, fontSize, GdPicture12.FontStyle.FontStyleRegular);
                        while (fntWidth > dstWidth && fontSize > 5)
                        {
                            fontSize--;
                            fntWidth = (int)gdPicture.GetTextWidth(imageId, humanValue, fontName, fontSize, GdPicture12.FontStyle.FontStyleRegular);
                        } 

                        int fntLeft = (((dstWidth - fntWidth) > 0) ? ((dstWidth - fntWidth) / 2) : 0);

                        hFont = (int)gdPicture.GetTextHeight(imageId, humanValue, fontName, fontSize, GdPicture12.FontStyle.FontStyleRegular);

                        //float compHeight = gdPicture.GetTextHeight(imageId, humanValue, fontName, 8, GdPicture12.FontStyle.FontStyleRegular);
                        //int optimalFontSize = Convert.ToInt32(fontSize * Math.Sqrt((dstHeight / 4) / compHeight));
                        //compHeight = gdPicture.GetTextHeight(imageId, humanValue, fontName, optimalFontSize, GdPicture12.FontStyle.FontStyleRegular);

                        int height = dstHeight;

                        int diff = 0;
                        if(nHeight > 0)
                            diff = (dstHeight - nHeight) / 2 - 1;
                        if (diff > 0)
                        {
                            //dstHeight -= (2 * diff);
                            if (vAlign == TBPicBarcodeVerticalAlign.BarcodeAlignCenter)
                            {
                                height = nHeight + diff + hFont;
                            }
                           /* else if (vAlign == TBPicBarcodeVerticalAlign.BarcodeAlignBottom)
                            {
                                height = dstHeight - hFont;
                            }*/
                        }
                        gStatus = DrawText
                                    (
                                        imageId,
                                        humanValue,
                                        fntLeft,
                                        height - hFont + 2, 
                                        fontSize,
                                        TBPicFontStyle.FontStyleRegular,
                                        txtColor,
                                        fontName,
                                        true /*apply the Antialiasing algorithm*/
                                    );
                    }
                    // disegno il barcode nell'immagine appena creata (il valore alfanumerico NON compare, uso la DrawText!)
                    gStatus = Barcode1DWrite
                                    (
                                        imageId,
                                        type,
                                        nNarrowBar,
                                        nHeight,
                                        value,
                                        margin, /*DstLeft + margine*/
                                        2, /*DstTop*/
                                        dstWidth - 2*margin, /*DstWidth - 2 margini*/
                                        showLabel ? (dstHeight - hFont - 4) : dstHeight - 2, /*DstHeight*/
                                        txtColor,
                                        align,
                                        vAlign,
                                        isPrinting
                                    );
                }
            }
            catch
            {
                ClearGdPicture();
                imageId = -1;
            }

            return imageId;
        }

        //--------------------------------------------------------------------------------
        private int GetImageIdFrom2DBarcode(TBPicBarcode2DWriterType bType, string value, int dstLeft , int dstTop, int dstWidth, int dstHeight,
            ref TBPictureStatus gStatus, Color txtColor, Color bkColor,
            int nEncodingType = -1, int nVersion = 0, int nModuleSize = -1, int nErrCorrLevel = -1,
            int nRowHeight = -1, int nRows  = -1, int nColumns = -1)
        {
            int imageId = -1;
            // se il value e' empty non procedo
            if (string.IsNullOrWhiteSpace(value))
            {
                ClearGdPicture();
                return imageId;
            }

            gStatus = TBPictureStatus.GenericError;

            try
            {
                imageId = gdPicture.CreateNewGdPictureImage(dstWidth/*width*/, dstHeight/*height*/, PixelFormat.Format24bppRgb, bkColor);

                if (imageId > 0)
                {
                    // disegno il barcode nell'immagine appena creata (il valore alfanumerico NON compare, uso dopo la DrawText!)

                    gStatus = Barcode2DWrite(bType, imageId, value, 0, dstLeft/*dstLeft*/, dstTop/*dstTop*/, dstWidth, dstHeight, txtColor, bkColor, nEncodingType, nVersion, nModuleSize, nErrCorrLevel, nRowHeight, nRows, nColumns);

                    if (gStatus != TBPictureStatus.OK)
                        return -1;
                }
            }
            catch
            {
                ClearGdPicture();
                imageId = -1;
            }

            return imageId;
        }

        //--------------------------------------------------------------------------------
        public int GetBarcodeImageId(string barcodeType, int nNarrowBar, int nHeight, string value, bool isPrinting)
        {
            TBPictureStatus status = TBPictureStatus.GenericError;
            int imageId = 0;
			try
			{
				if (If2DBarcode(barcodeType))
				{
					TBPicBarcode2DWriterType type = GetTBPicBarcode2DWriterType(barcodeType);
					if (type == TBPicBarcode2DWriterType.Barcode2DWriterPDF417)
						imageId = GetImageIdFrom2DBarcode(type, value, 0, 0, 330, 80, ref status, Color.Black, Color.White);
					else
						imageId = GetImageIdFrom2DBarcode(type, value, 0, 0, 100, 100, ref status, Color.Black, Color.White);
				}
				else
				{
					TBPicBarcode1DWriterType type = GetTBPicBarcode1DWriterType(barcodeType);
					imageId = GetImageIdFromTypedBarcode(type, nNarrowBar, nHeight, value, value, 0, 0, 250, 60, ref status, Color.Black, Color.White, true, isPrinting);
				}
			}
			catch (Exception)
			{
			}
            return imageId;
        }

		//--------------------------------------------------------------------------------
		public TBPictureStatus CreateBarcodeOnHDC (
                    string barcodeType, string value, string humanValue, IntPtr hdc, 
                    int dstLeft, int dstTop, int dstWidth, int dstHeight, 
                    Color txtColor, Color bkColor, bool showLabel, 
                    String fontName, int fontSize, 
                    int nNarrowBar, int nHeight, bool isVertical, bool isPrinting, 
                    TBPicBarcodeAlign align = TBPicBarcodeAlign.BarcodeAlignCenter,
                    TBPicBarcodeVerticalAlign vAlign = TBPicBarcodeVerticalAlign.BarcodeAlignCenter,
                    int nEncodingType = -1, int nVersion = 0, int nErrCorrLevel = -1, int nRows = -1, int nColumns = -1)
		{
			int imageId = 0;
			TBPictureStatus status = TBPictureStatus.GenericError;

			try
			{
				if (If2DBarcode(barcodeType))
				{
					TBPicBarcode2DWriterType type = GetTBPicBarcode2DWriterType(barcodeType);

					imageId = GetImageIdFrom2DBarcode(type, value, 0, 0, 
                                isVertical ? dstHeight : dstWidth, isVertical ? dstWidth : dstHeight, ref status, txtColor, bkColor, nEncodingType, nVersion, nNarrowBar, nErrCorrLevel, nHeight, nRows, nColumns);
				}
				else
				{
					TBPicBarcode1DWriterType type = GetTBPicBarcode1DWriterType(barcodeType);
                    
					imageId = GetImageIdFromTypedBarcode(type, nNarrowBar > 0 ? nNarrowBar : 1, nHeight, value, humanValue, 0, 0, 
                                isVertical ? dstHeight : dstWidth, isVertical ? dstWidth : dstHeight, ref status, txtColor, bkColor,
                                showLabel, isPrinting, align, vAlign, fontName, fontSize);
				}
			}
			catch (Exception)
			{
				return TBPictureStatus.InvalidBarCode;
			}

            if (status != TBPictureStatus.OK)
            {
                if (imageId != -1)
                    gdPicture.ReleaseGdPictureImage(imageId);
                return status;
            }

            if (imageId < 0)
                return TBPictureStatus.InvalidBarCode;

			if (isVertical)
				status = TBPicBaseComponents.TranslateTBPictureStatus(gdPicture.RotateAngle(imageId, 270));
            
			status = TBPicBaseComponents.TranslateTBPictureStatus(gdPicture.DrawGdPictureImageOnHDC(imageId, hdc, dstLeft, dstTop, dstWidth, dstHeight, InterpolationMode.High));

            gdPicture.ReleaseGdPictureImage(imageId);

            return status;
        }

        ///<summary>
        /// Ritorna il primo codice a barre individuato nell'imageId specificato
        /// Questo metodo e' utilizzato in fase di disegno di un codice a barre partendo da un valore
        ///</summary>
        //--------------------------------------------------------------------------------
        private string CreateBarcodeForImage(int imageId)
        {
            if (imageId <= 0)
                return null;

            try
            {
                // effettua una scansione dell'immagine per identificare gli eventuali codici a barre
                if (Barcode1DReaderDoScan(imageId) == TBPictureStatus.OK && Barcode1DReaderGetBarcodeCount() >= 1)
                    return Barcode1DReaderGetBarcodeValue(1); // 1 sta per indice del barcode
            }
            catch
            {
            }
            finally
            {
                Barcode1DReaderClear();
            }

            return null;
        }

        //--------------------------------------------------------------------------------
        public TBPicBarcode1DWriterType GetTBPicBarcode1DWriterType(string name)
        {
            switch (name)
            {
                case "MSIPLESSEY": return TBPicBarcode1DWriterType.Barcode1DWriterModifiedPlessey;
                case "ZIP": return TBPicBarcode1DWriterType.Barcode1DWriterPostNet;
                case "EXT39": return TBPicBarcode1DWriterType.Barcode1DWriterCode39Extended;
                case "EAN8": return TBPicBarcode1DWriterType.Barcode1DWriterEAN8;
                case "EAN13": return TBPicBarcode1DWriterType.Barcode1DWriterEAN13;
                case "UPCA": return TBPicBarcode1DWriterType.Barcode1DWriterUPCVersionA;
                case "UPCE0":
                case "UPCE1":
                case "UPCE": return TBPicBarcode1DWriterType.Barcode1DWriterUPCVersionE;
                case ""://def
                case "Default":
                case "CODE39": return TBPicBarcode1DWriterType.Barcode1DWriterCode39;
                case "INT25": return TBPicBarcode1DWriterType.Barcode1DWriterInterleaved2of5;
                case "CODE128":
                case "EAN128": return TBPicBarcode1DWriterType.Barcode1DWriterCode128;
                case "CODE128A": return TBPicBarcode1DWriterType.Barcode1DWriterCode128A;
                case "CODE128B": return TBPicBarcode1DWriterType.Barcode1DWriterCode128B;
                case "CODE128C": return TBPicBarcode1DWriterType.Barcode1DWriterCode128C;
                case "CODABAR": return TBPicBarcode1DWriterType.Barcode1DWriterCodabar;
                case "CODE93": return TBPicBarcode1DWriterType.Barcode1DWriterCode93;
            }

            throw new Exception(String.Format("This barcode type ({0}) can not be displayed", name));
        }

        //--------------------------------------------------------------------------------
        public TBPicBarcode2DWriterType GetTBPicBarcode2DWriterType(string name)
        {
            switch (name)
            {
                case ""://def
                case "Default":
                case "DataMatrix": return TBPicBarcode2DWriterType.Barcode2DWriterDataMatrix;
                case "MicroQR": return TBPicBarcode2DWriterType.Barcode2DWriterMicroQR;
                case "QR": return TBPicBarcode2DWriterType.Barcode2DWriterQR;
                case "PDF417": return TBPicBarcode2DWriterType.Barcode2DWriterPDF417;
            }

            throw new Exception(String.Format("This barcode type ({0}) can not be displayed", name));
        }

        //--------------------------------------------------------------------------------
        public bool If2DBarcode(string name)
        {
            switch (name)
            {
                case "DataMatrix":
                case "MicroQR":
                case "QR":
                case "PDF417":
                    return true;
                default: return false;
            }
        }
    }
}
