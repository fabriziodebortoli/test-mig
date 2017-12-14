#pragma once

#include "beginh.dex"

class TB_EXPORT BarCodeCreator : public CObject
{	
public:
	enum TBPictureStatus {
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
				//
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
				BarcodeSizeZero = 1028,//
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

	DECLARE_DYNCREATE(BarCodeCreator)

	BarCodeCreator() {};
	TBPictureStatus WriteBarcodeToHDC(	CDC& dc, CString value, CString humanValue, CString barcodeType, int nNarrowBar, int nHeight, CRect clipRegion,
										COLORREF txtColor, COLORREF bkColor, BOOL showLabel, 
										BOOL bPreview, LOGFONT& font, BOOL isVertical,
										/*TBPicBarcodeAlign*/int align = 1/* = TBPicBarcodeAlign::BarcodeAlignCenter*/,
										/*TBPicBarcodeVerticalAlign*/int vAlign = 1/* = TBPicBarcodeVerticalAlign::BarcodeAlignCenter*/,
										int nEncodingType = -1, int nVersion = 0, int nErrCorrLevel = -1, int nRows = -1, int nColumns = -1);
	BOOL If2DBarCode(CString barcodeType);
	CString GetErrorMessage(TBPictureStatus status);
};

#include "endh.dex"