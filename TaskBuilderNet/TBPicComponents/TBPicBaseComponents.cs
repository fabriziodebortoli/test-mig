using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GdPicture12;

namespace Microarea.TBPicComponents
{ 
    //--------------------------------------------------------------------------------
    public enum TBPictureStatus 
        {
            // Summary:
            //     Indicates that the method was successful.
            OK = 0,
            //
            // Summary:
            //     Indicates that there was an error on the method call, which is identified
            //     as something other than those defined by the other elements of this enumeration.
            GenericError = 1,
            //
            // Summary:
            //     Indicates that one of the arguments passed to the method was not valid.
            InvalidParameter = 2,
            //
            // Summary:
            //     Indicates that the operating system is out of memory and could not allocate
            //     memory to process the method call.
            OutOfMemory = 3,
            //
            // Summary:
            //     Indicates that one of the arguments specified in the API is already in use
            //     in another thread.
            ObjectBusy = 4,
            //
            // Summary:
            //     Indicates that a buffer specified as an argument in the API is not large
            //     enough to hold the data to be received.
            InsufficientBuffer = 5,
            //
            // Summary:
            //     Indicates that the method is not implemented.
            NotImplemented = 6,
            //
            // Summary:
            //     Indicates that the method generated a Microsoft Win32 error.
            Win32Error = 7,
            //
            // Summary:
            //     Indicates that the object is in an invalid state to satisfy the API call.
            WrongState = 8,
            //
            // Summary:
            //     Indicates that the method was aborted.
            Aborted = 9,
            //
            // Summary:
            //     Indicates that the specified image file or metafile cannot be found.
            FileNotFound = 10,
            //
            // Summary:
            //     Indicates that the method performed an arithmetic operation that produced
            //     a numeric overflow.
            ValueOverflow = 11,
            //
            // Summary:
            //     Indicates that a write operation is not allowed on the specified file.
            AccessDenied = 12,
            //
            // Summary:
            //     Indicates that the specified image file format is not known.
            UnknownImageFormat = 13,
            //
            // Summary:
            //     Indicates that the specified font family cannot be found. Either the font
            //     family name is incorrect or the font family is not installed.
            FontFamilyNotFound = 14,
            //
            // Summary:
            //     Indicates that the specified style is not available for the specified font
            //     family.
            FontStyleNotFound = 15,
            //
            // Summary:
            //     Indicates that the font retrieved from an HDC or LOGFONT is not a TrueType
            //     font and cannot be used with GdPicture.
            NotTrueTypeFont = 16,
            //
            // Summary:
            //     Indicates that the version of GDI+ that is installed on the system is incompatible
            //     with the version needed by GdPicture.
            UnsupportedGdiplusVersion = 17,
            //
            // Summary:
            //     Indicates that the GDI+API is not in an initialized state. Should never appends.
            GdiplusNotInitialized = 18,
            //
            // Summary:
            //     Indicates that the specified property does not exist in the target.
            PropertyNotFound = 19,
            //
            // Summary:
            //     Indicates that the specified property is not supported by the format of the
            //     target, therefore, cannot be set.
            PropertyNotSupported = 20,
            //
            // Summary:
            //     Indicates that the color profile required to save an image in CMYK format
            //     was not found.
            ProfileNotFound = 21,
            //
            // Summary:
            //     Indicates that the format of the image is not supported by the method.
            UnsupportedImageFormat = 22,
            //
            // Summary:
            //     The template was not found by the GdPicture ADR engine.
            TemplateNotFound = 23,
            //
            // Summary:
            //     An exception has been raised by the system during the printing process.
            PrintingException = 24,
            //
            // Summary:
            //     Indicates that an error was raised by the TWAIN plugin.
            TwainError = 30,
            //
            // Summary:
            //     Indicates that the GdPicture.NET.twain.client.dll version are invalid.
            WrongGdTwainVersion = 31,
            //
            // Summary:
            //     Indicates that an error of state was raised by the TWAIN plugin.
            BadTwainState = 32,
            //
            // Summary:
            //     Indicates that a transfer cancellation was raised by the TWAIN plugin.
            TwainTransferCanceled = 33,
            //
            // Summary:
            //     Indicates that an error of transfer was raised by the TWAIN plugin.
            TwainTransferError = 34,
            //
            // Summary:
            //     Indicates that the selected transfer mode is not supported by the current
            //     device.
            TwainInvalidTransferMode = 36,
            //
            // Summary:
            //     Indicates that the file passed as parameter to the method can not be created.
            CanNotCreateFile = 41,
            //
            // Summary:
            //     Indicates that the barcode data passed as parameter to the method is invalid.
            InvalidBarCode = 50,
            //
            // Summary:
            //     Indicates that the method needs an image with an indexed pixel format.
            NotIndexedPixelFormat = 61,
            //
            // Summary:
            //     Indicates that the method doesn't support the pixel format of the image.
            UnsupportedPixelFormat = 62,
            //
            // Summary:
            //     Indicates that the PDF handle provided is invalid or inexistant.
            InvalidPDFHandle = 63,
            //
            // Summary:
            //     Could not access to Internet.
            InternetOpenError = 100,
            //
            // Summary:
            //     An error occurred during the Internet connexion.
            InternetConnectError = 101,
            //
            // Summary:
            //     An error occurred during the HTTP open request.
            InternetHttpOpenRequestError = 102,
            //
            // Summary:
            //     An error occurred during the HTTP query.
            InternetHttpQueryError = 103,
            //
            // Summary:
            //     An error occurred sending an HTTP request.
            InternetHttpSendRequestError = 104,
            //
            // Summary:
            //     The length of the file returned from the HTTP server is invalid or null.
            InternetHttpInvalidFileLength = 105,
            //
            // Summary:
            //     An error occurred during the HTTP transfer.
            InternetHttpTransferError = 106,
            //
            // Summary:
            //     An error occurred during the HTTP writing operation.
            InternetHTTPWriteFileError = 107,
            //
            // Summary:
            //     An error occurred downloading a file from the FTP server.
            InternetFtpGetFileError = 300,
            //
            // Summary:
            //     An error occurred writing a file to the FTP server.
            InternetFtpWriteFileError = 301,
            //
            // Summary:
            //     The PDF must be unencrypted before performing this operation.
            PdfDocumentMustBeUnencrypted = 500,
            //
            // Summary:
            //     The PDF can not be decrypted by GdPicture.
            PdfCanNotBeDecrypted = 501,
            //
            // Summary:
            //     A password was required to open this PDF.
            PdfPasswordNeeded = 502,
            //
            // Summary:
            //     The password supplied to open the PDF was invalid.
            PdfBadPassword = 503,
            //
            // Summary:
            //     The PDF file can not be opened.
            PdfCanNotOpenFile = 504,
            //
            // Summary:
            //     Indicates that GdPicture was not able to process the document.
            PdfRenderingPageError = 505,
            //
            // Summary:
            //     Indicates that there was an error related to PDF manipulation, which is identified
            //     as something other than those defined by the other elements of this enumeration.
            PdfGenericError = 506,
            //
            // Summary:
            //     The image has not been added to the PDF.
            PdfErrorAddingImage = 507,
            //
            // Summary:
            //     The password supplied for the PDF certificate is invalid.
            PdfCertificateWrongPassword = 508,
            //
            // Summary:
            //     The format of the supplied certificate is invalid or unsupported.
            PdfCertificateWrongFormat = 509,
            //
            // Summary:
            //     The supplied certificate do not contains private key.
            PdfCertificateNoPrivateKey = 510,
            //
            // Summary:
            //     The operation is not supported in PDF/A.
            PdfUnsupportedInPdfA = 511,
            //
            // Summary:
            //     An invalid PDF structure or content has been encountered.
            PdfInvalidContent = 512,
            //
            // Summary:
            //     The GdPicture.NET.image.gdimgplug.dll library was needed or was not found
            //     on the computer.
            GdimgplugDllRequired = 700,
            //
            // Summary:
            //     The GdPicture.NET.image.gdimgplug.dll version found on the computer is older.
            WrongGdimgplugVersion = 701,
            //
            // Summary:
            //     The GdPicture.NET.ocr.tesseract.dll library was needed or was not found on
            //     the computer.
            OCRTesseractDllRequired = 800,
            //
            // Summary:
            //     The GdPicture.NET.ocr.tesseract.dll raised an unhandled exception.
            OCRDictionaryNotFound = 801,
            //
            // Summary:
            //     The dictionary files needed by the GdPicture Tesseract plugin was not found
            //     on the specified path.
            OCRUnhandledException = 802,
            //
            // Summary:
            //     The password provided for the certificate in invalid.
            CertificateWrongPassword = 900,
            //
            // Summary:
            //     The provided certificate has wrong or unsupported format.
            CertificateWrongFormat = 901,
            //
            // Summary:
            //     The provided certificate does not includes private key.
            CertificateWrongPrivateKey = 902,
            //
            // Summary:
            //     Data format invalid. (Invalid length).
            BarcodeInvalidLength = 1001,
            //
            // Summary:
            //     Data format invalid. (Invalid START character).
            BarcodeInvalidStart = 1002,
            //
            // Summary:
            //     Data format invalid. (Invalid STOP character).
            BarcodeInvalidStop = 1003,
            //
            // Summary:
            //     Data length invalid. (Length must be 13 or 14).
            BarcodeLengthMustBe13or14 = 1004,
            //
            // Summary:
            //     Numeric data only.
            BarcodeNotNumeric = 1005,
            //
            // Summary:
            //     Could not determine start character.
            BarcodeCanNotDetermineStart = 1006,
            //
            // Summary:
            //     Unknown start type in fixed type encoding.
            BarcodeUnknownStartType = 1007,
            //
            // Summary:
            //     No start character found in CurrentCodeSet.
            BarcodeNoStartInCurrentCodeSet = 1008,
            //
            // Summary:
            //     Could not insert start and code characters.
            BarcodeCouldNotInsertStart = 1009,
            //
            // Summary:
            //     Could not find encoding of a value in the formatted data.
            BarcodeNoEncodingValueFound = 1010,
            //
            // Summary:
            //     Invalid Data.
            BarcodeInvalidData = 1011,
            //
            // Summary:
            //     Destination not large enough to draw the barcode.
            BarcodeInvalidDestinationSize = 1012,
            // Summary:
            //     Country assigning manufacturer code not found.
            BarcodeInvalidManufacturerCode = 1013,
            //
            // Summary:
            //     Invalid data length. (7 or 8 numbers only).
            BarcodeLengthMustBe7or8 = 1014,
            //
            // Summary:
            //     Data length invalid. Must be a multiple of 2.
            BarcodeLengthMustBeMultipleOf2 = 1015,
            //
            // Summary:
            //     Invalid input. Must start with 978 and be length must be 9, 10, 12, 13 characters.
            BarcodeMustStartWith978orBadLength = 1016,
            //
            // Summary:
            //     Invalid Country Code for JAN13 (49 required).
            BarcodeMustStartWith49 = 1017,
            //
            // Summary:
            //     Invalid data length. (5, 6, 9, or 11 digits only).
            BarcodeBarcodeLengthMustBe5or6or9or11 = 1018,
            //
            // Summary:
            //     Data length invalid. (Length must be 12).
            BarcodeBarcodeLengthMustBe12 = 1019,
            //
            // Summary:
            //     Invalid data length. (8 or 12 numbers only).
            BarcodeBarcodeLengthMustBe8or12 = 1020,
            //
            // Summary:
            //     Invalid Number System (only 0 and 1 are valid).
            BarcodeBarcodeMustBinaryChar = 1021,
            //
            // Summary:
            //     Illegal UPC-A entered for conversion. Unable to convert.
            BarcodeIllegalUPCA = 1022,
            //
            // Summary:
            //     Invalid data length. (Length = 2 required).
            BarcodeBarcodeLengthMustBe2 = 1023,
            //
            // Summary:
            //     Invalid data length. (Length = 5 required).
            BarcodeBarcodeLengthMustBe5 = 1024,
            //
            // Summary:
            //     Data length invalid. (Length must be 12 or 13).
            BarcodeLengthMustBe12or13 = 1025,
            //
            // Summary:
            //     Data length invalid. (Length must be 11 or 12).
            BarcodeLengthMustBe11or12 = 1026,
            //
            // Summary:
            //     Destination not large enough to draw the barcode because of set size of a single bar.
            BarcodeInvalidDestinationBarSize = 1027,
            //
            // Summary:
            //     Width of barcode is zero.
            BarcodeSizeZero = 1028,
            //
            // Summary:
            //     Can't load GdPicture.NET.9.barcode.1d.reader.dll.
            Barcode1DReaderPluginNotLoaded = 1100,
            //
            // Summary:
            //     Unknown error reported from the 1D Barcode reader plugin.
            Barcode1DReaderUnknownError = 1101,
            //
            // Summary:
            //     Can't load the DataMatrix reader dll.
            BarcodeDataMatrixReaderPluginNotLoaded = 1200,
            //
            // Summary:
            //     Unknown error reported from the DataMatrix Barcode reader plugin.
            BarcodeDataMatrixReaderUnknownError = 1201,
            //
            // Summary:
            //     Can't load the QR-Code reader dll.
            BarcodeQRCodeReaderPluginNotLoaded = 1300,
            //
            // Summary:
            //     Can't load GdPicture.NET.9.barcode.pdf417.reader.dll.
            BarcodePDF417ReaderPluginNotLoaded = 1400,
            //
            // Summary:
            //     The version provided to the Qr-Code encoder is invalid.
            BarcodeQrEncoderInvalidVersion = 1500,
            //
            // Summary:
            //     The Qr-Code expected only numeric data.
            BarcodeQrEncoderNotNumericData = 1501,
            //
            // Summary:
            //     The Qr-Code expected only alpha-numeric data.
            BarcodeQrEncoderNotAlphanumericData = 1502,
            //
            // Summary:
            //     The Qr-Code expected only byte data.
            BarcodeQrEncoderNot8BitData = 1503,
            //
            // Summary:
            //     The Qr-Code expected only kanji data.
            BarcodeQrEncoderNotKanjiData = 1504,
            //
            // Summary:
            //     The version provided to the DataMatrix encoder is invalid.
            BarcodeDatamatrixEncoderInvalidVersion = 1510,
            //
            // Summary:
            //     The version provided to the PDF417 encoder is invalid.
            BarcodePDF417EncoderInvalidVersion = 1520,
            //
            // Summary:
            //     The PDF417 encoder expected only numeric data.
            BarcodePDF417EncoderNotNumericData = 1521,
            //
            // Summary:
            //     The PDF417 encoder expected only alpha-numeric data.
            BarcodePDF417EncoderNotTextData = 1522,
            //
            // Summary:
            //     The PDF417 encoder expected only byte data.
            BarcodePDF417EncoderNotByteData = 1523,
            //
            // Summary:
            //     The selected encryption scheme is not supported.
            EncryptionSchemeNotSupported = 2000,
            //
            // Summary:
            //     Can't load the GdPicture JBIG2 encoder library.
            JBIG2PluginNotLoaded = 3000,
            //
            // Summary:
            //     Can't load the GdPicture Document analyzer encoder library.
            DocumentAnalyzerDllRequired = 4000,
            //
            // Summary:
            //     The license key provided doesn't include this feature.
            InvalidLicense = 9999,
            //
            // Summary:
            //     WIA error. Use WiaGetLastError() functions for diagnosing the error.
            WIAGenericError = 20000,
        };

    //--------------------------------------------------------------------------------
    public enum TBPicBarcode1DReaderType
    {
        Barcode1DReaderNone = 0,
        Barcode1DReaderIndustrial2of5 = 1,
        Barcode1DReaderInverted2of5 = 2,
        Barcode1DReaderInterleaved2of5 = 4,
        Barcode1DReaderIata2of5 = 8,
        Barcode1DReaderMatrix2of5 = 16,
        Barcode1DReaderCode39 = 32,
        Barcode1DReaderCodeabar = 64,
        Barcode1DReaderBcdMatrix = 128,
        Barcode1DReaderDataLogic2of5 = 256,
        Barcode1DReaderCode128 = 4096,
        Barcode1DReaderEAN128 = 8192,
        Barcode1DReaderCODE93 = 16384,
        Barcode1DReaderEAN13 = 32768,
        Barcode1DReaderUPCA = 65536,
        Barcode1DReaderEAN8 = 131072,
        Barcode1DReaderUPCE = 262144,
        Barcode1DReaderADD5 = 524288,
        Barcode1DReaderADD2 = 1048576,
    }

    //--------------------------------------------------------------------------------
    public enum TBPicBarcode1DWriterType
    {
        Barcode1DWriterUPCVersionA = 0,
        Barcode1DWriterUPCVersionE = 1,
        Barcode1DWriterUPCSupplemental2Digit = 2,
        Barcode1DWriterUPCSupplemental5Digit = 3,
        Barcode1DWriterEAN13 = 4,
        Barcode1DWriterEAN8 = 5,
        Barcode1DWriterInterleaved2of5 = 6,
        Barcode1DWriterStandard2of5 = 7,
        Barcode1DWriterIndustrial2of5 = 8,
        Barcode1DWriterCode39 = 9,
        Barcode1DWriterCode39Extended = 10,
        Barcode1DWriterCodabar = 11,
        Barcode1DWriterPostNet = 12,
        Barcode1DWriterBookland = 13,
        Barcode1DWriterISBN = 14,
        Barcode1DWriterJAN13 = 15,
        Barcode1DWriterMSIMod10 = 16,
        Barcode1DWriterMSI2Mod10 = 17,
        Barcode1DWriterMSIMod11 = 18,
        Barcode1DWriterMSIMod11Mod10 = 19,
        Barcode1DWriterModifiedPlessey = 20,
        Barcode1DWriterCode11 = 21,
        Barcode1DWriterUSD8 = 22,
        Barcode1DWriterUCC12 = 23,
        Barcode1DWriterUCC13 = 24,
        Barcode1DWriterLOGMARS = 25,
        Barcode1DWriterCode128 = 26,
        Barcode1DWriterCode128A = 27,
        Barcode1DWriterCode128B = 28,
        Barcode1DWriterCode128C = 29,
        Barcode1DWriterITF14 = 30,
        Barcode1DWriterCode93 = 31,
        Barcode1DWriterTelePen = 32,
    }

    //--------------------------------------------------------------------------------
    public enum TBPicBarcode2DReaderType
    {  
        Barcode2DReaderDataMatrix,
        Barcode2DReaderMicroQR,
        Barcode2DReaderQR ,
        Barcode2DReaderPDF417
    }

    //--------------------------------------------------------------------------------
    public enum TBPicBarcode2DWriterType
    {

        Barcode2DWriterDataMatrix,
        Barcode2DWriterMicroQR,
        Barcode2DWriterQR,
        Barcode2DWriterPDF417
    }

    //--------------------------------------------------------------------------------
    public enum TBPicTiffCompression
    {
        // Summary:
        //     No compression
        TiffCompressionNONE = 1,
        //
        // Summary:
        //     CCITT modified Huffman RLE
        TiffCompressionRLE = 2,
        //
        // Summary:
        //     CCITT Group 3 fax encoding
        TiffCompressionCCITT3 = 3,
        //
        // Summary:
        //     CCITT Group 4 fax encoding
        TiffCompressionCCITT4 = 4,
        //
        // Summary:
        //     Lempel-Ziv and Welch
        TiffCompressionLZW = 5,
        //
        // Summary:
        //     !6.0 JPEG
        TiffCompressionOJPEG = 6,
        //
        // Summary:
        //     %JPEG DCT compression
        TiffCompressionJPEG = 7,
        //
        // Summary:
        //     Deflate compression,as recognized by Adobe
        TiffCompressionADOBEDEFLATE = 8,
        //
        // Summary:
        //     NeXT 2-bit RLE
        TiffCompressionNEXT = 32766,
        //
        // Summary:
        //     #1 w/ word alignment
        TiffCompressionCCITTRLEW = 32771,
        //
        // Summary:
        //     Macintosh RLE
        TiffCompressionPACKBITS = 32773,
        //
        // Summary:
        //     ThunderScan RLE
        TiffCompressionTHUNDERSCAN = 32809,
        //
        // Summary:
        //     Deflate compression
        TiffCompressionDEFLATE = 32946,
        //
        // Summary:
        //     Uses CCITT4 compression for bitonal image and LZW for others. TiffCompressionAUTO
        //     allows to mix compression in a multipage tiff document.
        TiffCompressionAUTO = 65536,
    }

    //--------------------------------------------------------------------------------
    public enum TBPicBarcodeAlign
    {
        BarcodeAlignLeft = 0,
        BarcodeAlignCenter = 1,
        BarcodeAlignRight = 2,
    }

    //--------------------------------------------------------------------------------
    public enum TBPicBarcodeVerticalAlign
    {
        BarcodeAlignTop = 0,
        BarcodeAlignCenter = 1,
        BarcodeAlignBottom = 2,
    }

    //--------------------------------------------------------------------------------
    public enum TBPicFontStyle
    {
        FontStyleRegular = 0,
        FontStyleBold = 1,
        FontStyleItalic = 2,
        FontStyleBoldItalic = 3,
        FontStyleUnderline = 4,
        FontStyleBoldUnderline = 5,
        FontStyleItalicUnderline = 6,
        FontStyleBoldItalicUnderline = 7,
        FontStyleStrikeout = 8,
        FontStyleBoldStrikeout = 9,
        FontStyleItalicStrikeout = 10,
        FontStyleBoldItalicStrikeout = 11,
        FontStyleUnderlineStrikeout = 12,
        FontStyleBoldUnderlineStrikeout = 13,
        FontStyleItalicUnderlineStrikeout = 14,
        FontStyleBoldItalicUnderlineStrikeout = 15,
    }

    //--------------------------------------------------------------------------------
    public enum TBPicTwainStatus
    {
        // Summary:
        //     Unknown Error.
        TWAIN_ERROR = -1,
        //
        // Summary:
        //     Presession, Twain environment is enable.
        TWAIN_PRESESSION = 1,
        //
        // Summary:
        //     Source Manager was loaded.
        TWAIN_SM_LOADED = 2,
        //
        // Summary:
        //     Source Manager is open.
        TWAIN_SM_OPEN = 3,
        //
        // Summary:
        //     A source is open.
        TWAIN_SOURCE_OPEN = 4,
        //
        // Summary:
        //     A source is enabled.
        TWAIN_SOURCE_ENABLED = 5,
        //
        // Summary:
        //     A source is ready to transfer data.
        TWAIN_TRANSFER_READY = 6,
        //
        // Summary:
        //     A source is transfering data.
        TWAIN_TRANSFERRING = 7,
    }

    //--------------------------------------------------------------------------------
    public enum TbPicTwainResultCode
    {
        // Summary:
        //     Operation was successful.
        TWRC_SUCCESS = 0,
        //
        // Summary:
        //     Operation failed - get the Condition Code for more information.
        TWRC_FAILURE = 1,
        //
        // Summary:
        //     Partially successful operation; request further information.
        TWRC_CHECKSTATUS = 2,
        //
        // Summary:
        //     Abort transfer or the Cancel button was pressed.
        TWRC_CANCEL = 3,
        //
        // Summary:
        //     Event or Windows message beIntegers to this Source.
        TWRC_DSEVENT = 4,
        //
        // Summary:
        //     Event or Windows message does not beInteger to this source.
        TWRC_NOTDSEVENT = 5,
        //
        // Summary:
        //     All data has been transfered.
        TWRC_XFERDONE = 6,
        //
        // Summary:
        //     No more sources found after MSG_GETNEXT.
        TWRC_ENDOFLIST = 7,
        //
        // Summary:
        //     The type of information requested is not supported by the data source.
        TWRC_INFONOTSUPPORTED = 8,
        //
        // Summary:
        //     Data for the requested information is not available.
        TWRC_DATANOTAVAILABLE = 9,
    }

    //--------------------------------------------------------------------------------
    public enum TBPicViewerZoomMode
    {
        ZoomMode100 = 1,
        ZoomModeFitToViewer = 2,
        ZoomModeWidthViewer = 3,
        ZoomModeCustom = 4,
        ZoomModeHeightViewer = 5,
        ZoomModeToViewer = 6,
    }

    //--------------------------------------------------------------------------------
    public enum TBPicViewerMouseWheelMode
    {
        MouseWheelModeZoom = 0,
        MouseWheelModeVerticalScroll = 1,
        MouseWheelModePageChange = 2,
    }

    //--------------------------------------------------------------------------------
    public enum TBPicMouseButton
    {
        MouseButtonNone = 0,
        MouseButtonLeft = 1,
        MouseButtonRight = 2,
        MouseButtonMiddle = 4,
        MouseButtonXButton1 = 8,
        MouseButtonXButton2 = 16,
    }

    //--------------------------------------------------------------------------------
    public enum TBPicViewerMouseMode
    {
        // Summary:
        //     Do nothing
        MouseModeDefault = 0,
        //
        // Summary:
        //     Allows user to select an area of the displayed document
        MouseModeAreaSelection = 1,
        //
        // Summary:
        //     Allows user to pan the displayed document
        MouseModePan = 2,
        //
        // Summary:
        //     Allows user to select for zooming an area of the displayed document
        MouseModeAreaZooming = 3,
        //
        // Summary:
        //     Displays a magnifier tool.
        MouseModeMagnifier = 4,
    }

    //--------------------------------------------------------------------------------
    public enum TBPicDisplayQuality
    {
        DisplayQualityLow = 0,
        DisplayQualityBilinear = 1,
        DisplayQualityBicubic = 2,
        DisplayQualityBilinearHQ = 3,
        DisplayQualityBicubicHQ = 4,
    }

    //--------------------------------------------------------------------------------
    public enum TBPicViewerDocumentAlignment
    {
        DocumentAlignmentMiddleLeft = 0,
        DocumentAlignmentMiddleRight = 1,
        DocumentAlignmentMiddleCenter = 2,
        DocumentAlignmentTopLeft = 4,
        DocumentAlignmentTopRight = 5,
        DocumentAlignmentTopCenter = 6,
        DocumentAlignmentBottomLeft = 7,
        DocumentAlignmentBottomRight = 8,
        DocumentAlignmentBottomCenter = 9,
    }

    //--------------------------------------------------------------------------------
    public enum TBPicViewerDocumentPosition
    {
        DocumentPositionMiddleLeft = 0,
        DocumentPositionMiddleRight = 1,
        DocumentPositionMiddleCenter = 2,
        DocumentPositionTopLeft = 4,
        DocumentPositionTopRight = 5,
        DocumentPositionTopCenter = 6,
        DocumentPositionBottomLeft = 7,
        DocumentPositionBottomRight = 8,
        DocumentPositionBottomCenter = 9,
    }


    //--------------------------------------------------------------------------------
    public enum TBPicPdfFormFieldType
    {
        // Summary:
        //     Undefined type. Check for error.
        PdfFormFieldTypeUnknown = 0,
        //
        // Summary:
        //     A pushbutton is a purely interactive control that responds immediately to
        //     user input without retaining a permanent value.
        PdfFormFieldTypePushButton = 1,
        //
        // Summary:
        //     A pushbutton is a purely interactive control that responds immediately to
        //     user input without retaining a permanent value.
        PdfFormFieldTypeCheckBoxButton = 2,
        //
        // Summary:
        //     A check box toggles between two states, on and off.
        PdfFormFieldTypeRadioButton = 3,
        //
        // Summary:
        //     A Radio button fields contain a set of related buttons that can each be on
        //     or off.  Typically, at most one radio button in a set may be on at any given
        //     time, and selecting any one of the buttons automatically deselects all the
        //     others..
        PdfFormFieldTypeText = 4,
        //
        // Summary:
        //     Choice Field.
        PdfFormFieldTypeChoice = 5,
        //
        // Summary:
        //     Signature Field.
        PdfFormFieldTypeSignature = 6,
    }

    public enum TBPicPdfMeasurementUnit
    {
        // Summary:
        //     Point. 1 point = 1/72 inch.
        PdfMeasurementUnitPoint = 0,
        //
        // Summary:
        //     Millimeters.
        PdfMeasurementUnitMillimeter = 1,
        //
        // Summary:
        //     Centimeters. 1 centimeter = 1/2.54 inch.
        PdfMeasurementUnitCentimeter = 2,
        //
        // Summary:
        //     Inch. 1 inch = 72 points.
        PdfMeasurementUnitInch = 3,
    }

    public enum TBPicPdfStandardFont
    {
        PdfStandardFontCourier = 1,
        PdfStandardFontCourierOblique = 2,
        PdfStandardFontCourierBold = 3,
        PdfStandardFontCourierBoldOblique = 4,
        PdfStandardFontHelvetica = 5,
        PdfStandardFontHelveticaOblique = 6,
        PdfStandardFontHelveticaBold = 7,
        PdfStandardFontHelveticaBoldOblique = 8,
        PdfStandardFontTimesRoman = 9,
        PdfStandardFontTimesItalic = 10,
        PdfStandardFontTimesBold = 11,
        PdfStandardFontTimesBoldItalic = 12,
        PdfStandardFontSymbol = 13,
        PdfStandardFontZapfDingbats = 14,
    }

    public enum TBPicPdfOrigin
    {
        PdfOriginBottomLeft = 0,
        PdfOriginTopLeft = 1,
        PdfOriginTopRight = 2,
        PdfOriginBottomRight = 3
    }

    public enum TBPicDocumentFormat
    {
        DocumentFormatUNKNOWN = 0,
        DocumentFormatICO = 1,
        DocumentFormatBMP = 2,
        DocumentFormatWBMP = 3,
        DocumentFormatJPEG = 4,
        DocumentFormatGIF = 5,
        DocumentFormatPNG = 6,
        DocumentFormatTIFF = 7,
        DocumentFormatFAXG3 = 8,
        DocumentFormatEXIF = 9,
        DocumentFormatEMF = 10,
        DocumentFormatWMF = 11,
        DocumentFormatJNG = 12,
        DocumentFormatKOALA = 13,
        DocumentFormatIFF = 14,
        DocumentFormatMNG = 15,
        DocumentFormatPCD = 16,
        DocumentFormatPCX = 17,
        DocumentFormatPBM = 18,
        DocumentFormatPBMRAW = 19,
        DocumentFormatPFM = 20,
        DocumentFormatPGM = 21,
        DocumentFormatPGMRAW = 22,
        DocumentFormatPPM = 23,
        DocumentFormatPPMRAW = 24,
        DocumentFormatRAS = 25,
        DocumentFormatTARGA = 26,
        DocumentFormatPSD = 27,
        DocumentFormatCUT = 28,
        DocumentFormatXBM = 29,
        DocumentFormatXPM = 30,
        DocumentFormatDDS = 31,
        DocumentFormatHDR = 32,
        DocumentFormatSGI = 33,
        DocumentFormatEXR = 34,
        DocumentFormatJ2K = 35,
        DocumentFormatJP2 = 36,
        DocumentFormatJBIG = 37,
        DocumentFormatPICT = 38,
        DocumentFormatRAW = 39,
        DocumentFormatJBIG2 = 100,
        DocumentFormatMemoryBMP = 500,
        DocumentFormatPDF = 1000,
    }


    //=========================================================================
    public static class TBPicBaseComponents
    {
        //--------------------------------------------------------------------------------
        public static void Register()
        {
            LicenseManager LicMgr = new LicenseManager();
            //LicMgr.RegisterKEY("4118208038890486058421763");
            //LicMgr.RegisterKEY("9121015862970845451491642");
            //LicMgr.RegisterKEY("912171885523027521116121526563211");
            LicMgr.RegisterKEY("21189874691622973121413129334161071597");
        }

        //--------------------------------------------------------------------------------
        public static TBPictureStatus TranslateTBPictureStatus(GdPictureStatus status)
        {
            int val = (int)status;
            return (TBPictureStatus)val;
        }

        //--------------------------------------------------------------------------------
        public static TBPicBarcode1DReaderType TranslateTBPicBarcode1DReaderType(Barcode1DReaderType status)
        {
            int val = (int)status;
            return (TBPicBarcode1DReaderType)val;
        }

        //--------------------------------------------------------------------------------
        public static TBPicBarcode1DWriterType TranslateTBPicBarcode1DWriterType(Barcode1DWriterType status)
        {
            int val = (int)status;
            return (TBPicBarcode1DWriterType)val;
        }

        //--------------------------------------------------------------------------------
        public static TBPicTiffCompression TranslateTBPicTiffCompression(TiffCompression status)
        {
            int val = (int)status;
            return (TBPicTiffCompression)val;
        }

    }
}
