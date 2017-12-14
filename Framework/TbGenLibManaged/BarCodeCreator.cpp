#include "stdafx.h" 

#include "BarCodeCreator.h"
#include <TbGeneric\TBStrings.h>
#include <TbGeneric\GeneralFunctions.h>

using namespace System;
using namespace System::Data;
using namespace System::Runtime::InteropServices;
using namespace Microarea::TBPicComponents;

///////////////////////////////////////////////////////////////////////////////
//					BarCodeCreator
///////////////////////////////////////////////////////////////////////////////
IMPLEMENT_DYNCREATE(BarCodeCreator, CObject)

//----------------------------------------------------------------------------
BarCodeCreator::TBPictureStatus BarCodeCreator::WriteBarcodeToHDC
	(
		CDC& dc, CString value, CString humanValue, CString barcodeType, int nNarrowBar, int nHeight,
		CRect clipRegion, COLORREF txtColor, COLORREF bkColor, 
		BOOL showLabel, BOOL bPreview, LOGFONT& lffont, BOOL isVertical, 
		int align/* = 1 = TBPicBarcodeAlign::BarcodeAlignCenter*/,
		int vAlign/* = 1 = TBPicBarcodeVerticalAlign::BarcodeAlignCenter*/,
		int nEncodingType, int nVersion, int nErrCorrLevel, int nRows , int nColumns
	)
{
	TBPicImaging^ tbPicImaging = gcnew TBPicImaging();

	int hFont = GetFontPointHeight(&dc, lffont, bPreview);

	hFont = ScalePix(hFont);

	IntPtr hdcPtr(dc.GetSafeHdc()); 

	System::Drawing::Color tColor = System::Drawing::ColorTranslator::FromWin32(txtColor);
	System::Drawing::Color bColor = System::Drawing::ColorTranslator::FromWin32(bkColor);
	
	BarCodeCreator::TBPictureStatus status = (TBPictureStatus)tbPicImaging->CreateBarcodeOnHDC
		(
			gcnew String(barcodeType), gcnew String(value), humanValue.IsEmpty()? gcnew String(value) : gcnew String(humanValue),
			hdcPtr, 
			clipRegion.TopLeft().x,clipRegion.TopLeft().y, clipRegion.Width(), clipRegion.Height(),
			tColor, bColor, 
			showLabel ? true : false, 
			gcnew String(lffont.lfFaceName), hFont,
			nNarrowBar, nHeight, isVertical ? true : false,
			dc.IsPrinting() ? true : false,
			(TBPicBarcodeAlign)align, (TBPicBarcodeVerticalAlign)vAlign, nEncodingType, nVersion, nErrCorrLevel, nRows, nColumns
		);
	
	return status;
}

//----------------------------------------------------------------------------
BOOL BarCodeCreator::If2DBarCode(CString barcodeType)
{
	TBPicImaging^ tbPicImaging = gcnew TBPicImaging();
	return tbPicImaging->If2DBarcode(gcnew String(barcodeType));
}

//----------------------------------------------------------------------------
CString BarCodeCreator::GetErrorMessage(TBPictureStatus status)
{
	switch (status)
	{
	case GenericError:
	{
		return _TB("Generic error occured in the function call");
	}

	case InvalidParameter:
	{
		return _TB("One of the arguments passed to the method was not valid");
	}

	case OutOfMemory:
	{
		return _TB("The operating system is out of memory and could not allocate memory to process the method call");
	}

	case ObjectBusy:
	{
		return _TB("One of the arguments specified in the API is already in use in another thread");
	}

	case InsufficientBuffer:
	{
		return _TB("Buffer specified as an argument in the API is not large  enough to hold the data to be received");
	}

	case NotImplemented:
	{
		return _TB("Method is not implemented");
	}

	case Win32Error:
	{
		return _TB("Method generated a Microsoft Win32 error");
	}

	case WrongState:
	{
		return _TB("Object is in an invalid state to satisfy the API call");
	}

	case Aborted:
	{
		return _TB("Method was aborted");
	}

	case FileNotFound:
	{
		return _TB("Image file or metafile cannot be found");
	}

	case ValueOverflow:
	{
		return _TB("Method performed an arithmetic operation that produced a numeric overflow");
	}

	case AccessDenied:
	{
		return _TB("Write operation is not allowed on the specified file");
	}

	case UnknownImageFormat:
	{
		return _TB("The specified image file format is not known");
	}

	case FontFamilyNotFound:
	{
		return _TB("The specified font family cannot be found. Either the font family name is incorrect or the font family is not installed");
	}

	case FontStyleNotFound:
	{
		return _TB("The specified style is not available for the specified font family");
	}

	case NotTrueTypeFont:
	{
		return _TB("The font retrieved from an HDC or LOGFONT is not a TrueType font and cannot be used");
	}

	case UnsupportedGdiplusVersion:
	{
		return _TB("The version of GDI+ that is installed on the system is incompatible with the version needed");
	}

	case GdiplusNotInitialized:
	{
		return _TB("The GDI+API is not in an initialized state. Should never appends");
	}

	case PropertyNotFound:
	{
		return _TB("The specified property does not exist in the target");
	}

	case PropertyNotSupported:
	{
		return _TB("The specified property is not supported by the format of the target, therefore, cannot be set");
	}

	case ProfileNotFound:
	{
		return _TB("The color profile required to save an image in CMYK format was not found");
	}

	case UnsupportedImageFormat:
	{
		return _TB("The format of the image is not supported by the method");
	}

	case TemplateNotFound:
	{
		return _TB("The template was not found");
	}

	case PrintingException:
	{
		return _TB("An exception has been raised by the system during the printing process");
	}


	case TwainError:
	{
		return _TB("An error was raised by the TWAIN plugin");
	}

	case WrongGdTwainVersion:
	{
		return _TB("The GdPicture.NET.twain.client.dll version are invalid");
	}

	case BadTwainState:
	{
		return _TB("An error of state was raised by the TWAIN plugin");
	}

	case TwainTransferCanceled:
	{
		return _TB("A transfer cancellation was raised by the TWAIN plugin");
	}

	case TwainTransferError:
	{
		return _TB("An error of transfer was raised by the TWAIN plugin");
	}

	case TwainInvalidTransferMode:
	{
		return _TB("The selected transfer mode is not supported by the current device");
	}

	case CanNotCreateFile:
	{
		return _TB("The file passed as parameter to the method can not be created");
	}

	case InvalidBarCode:
	{
		return _TB("The barcode string is invalid");
	}

	case NotIndexedPixelFormat:
	{
		return _TB("The method needs an image with an indexed pixel format");
	}

	case UnsupportedPixelFormat:
	{
		return _TB("The method doesn't support the pixel format of the image");
	}

	case InvalidPDFHandle:
	{
		return _TB("The PDF handle provided is invalid or inexistant");
	}

	case InternetOpenError:
	{
		return _TB("Could not access to Internet");
	}

	case InternetConnectError:
	{
		return _TB("An error occurred during the Internet connection");
	}

	case InternetHttpOpenRequestError:
	{
		return _TB("An error occurred during the HTTP open request");
	}

	case InternetHttpQueryError:
	{
		return _TB("An error occurred during the HTTP query");
	}

	case InternetHttpSendRequestError:
	{
		return _TB("An error occurred sending an HTTP request");
	}

	case InternetHttpInvalidFileLength:
	{
		return _TB("The length of the file returned from the HTTP server is invalid or null");
	}

	case InternetHttpTransferError:
	{
		return _TB("An error occurred during the HTTP transfer");
	}

	case InternetHTTPWriteFileError:
	{
		return _TB("An error occurred during the HTTP writing operation");
	}

	case InternetFtpGetFileError:
	{
		return _TB("An error occurred downloading a file from the FTP server");
	}

	case InternetFtpWriteFileError:
	{
		return _TB("An error occurred writing a file to the FTP server");
	}

	case PdfDocumentMustBeUnencrypted:
	{
		return _TB("The PDF must be unencrypted before performing this operation");
	}

	case PdfCanNotBeDecrypted:
	{
		return _TB("The PDF can not be decrypted");
	}

	case PdfPasswordNeeded:
	{
		return _TB("A password was required to open this PDF");
	}

	case PdfBadPassword:
	{
		return _TB("The password supplied to open the PDF was invalid");
	}

	case PdfCanNotOpenFile:
	{
		return _TB("The PDF file can not be opened");
	}

	case PdfRenderingPageError:
	{
		return _TB("GdPicture was not able to process the document");
	}

	case PdfGenericError:
	{
		return _TB("There was a generic error related to PDF manipulation");
	}

	case PdfErrorAddingImage:
	{
		return _TB("The image has not been added to the PDF");
	}

	case PdfCertificateWrongPassword:
	{
		return _TB("The password supplied for the PDF certificate is invalid");
	}

	case PdfCertificateWrongFormat:
	{
		return _TB("The format of the supplied certificate is invalid or unsupported");
	}

	case PdfCertificateNoPrivateKey:
	{
		return _TB("The supplied certificate do not contains private key");
	}

	case PdfUnsupportedInPdfA:
	{
		return _TB("The operation is not supported in PDF/A");
	}

	case PdfInvalidContent:
	{
		return _TB("An invalid PDF structure or content has been encountered");
	}

	case GdimgplugDllRequired:
	{
		return _TB("The GdPicture.NET.image.gdimgplug.dll library was needed or was not found on the computer");
	}

	case WrongGdimgplugVersion:
	{
		return _TB("The GdPicture.NET.image.gdimgplug.dll version found on the computer is deprecated");
	}

	case OCRTesseractDllRequired:
	{
		return _TB("The GdPicture.NET.ocr.tesseract.dll library was needed or was not found on the computer");
	}

	case OCRDictionaryNotFound:
	{
		return _TB("The GdPicture.NET.ocr.tesseract.dll raised an unhandled exception");
	}

	case OCRUnhandledException:
	{
		return _TB("The dictionary files needed by the GdPicture Tesseract plugin was not found on the specified path");
	}

	case CertificateWrongPassword:
	{
		return _TB("The password provided for the certificate in invalid");
	}

	case CertificateWrongFormat:
	{
		return _TB("The provided certificate has wrong or unsupported format");
	}

	case  CertificateWrongPrivateKey:
	{
		return _TB("The provided certificate does not includes private key");
	}

	case BarcodeInvalidLength:
	{
		return _TB("Data format invalid. (Invalid length)");
	}

	case BarcodeInvalidStart:
	{
		return _TB("Data format invalid. (Invalid START character)");
	}

	case BarcodeInvalidStop:
	{
		return _TB("Data format invalid. (Invalid STOP character)");
	}

	case BarcodeLengthMustBe13or14:
	{
		return _TB("Data length invalid. (Length must be 13 or 14)");
	}

	case BarcodeNotNumeric:
	{
		return _TB("Numeric data only");
	}

	case BarcodeCanNotDetermineStart:
	{
		return _TB("Could not determine start character");
	}

	case BarcodeUnknownStartType:
	{
		return _TB("Unknown start type in fixed type encoding");
	}

	case BarcodeNoStartInCurrentCodeSet:
	{
		return _TB("No start character found in CurrentCodeSet");
	}

	case BarcodeCouldNotInsertStart:
	{
		return _TB("Could not insert start and code characters");
	}

	case BarcodeNoEncodingValueFound:
	{
		return _TB("Could not find encoding of a value in the formatted data");
	}

	case BarcodeInvalidData:
	{
		return _TB("Invalid Data");
	}

	case BarcodeInvalidDestinationSize:
	{
		return _TB("Destination is not large enough to draw the barcode");
	}

	case BarcodeInvalidManufacturerCode:
	{
		return _TB("Country assigning manufacturer code not found");
	}

	case BarcodeLengthMustBe7or8:
	{
		return _TB("Invalid data length. (7 or 8 numbers only)");
	}

	case BarcodeLengthMustBeMultipleOf2:
	{
		return _TB("Data length invalid. Must be a multiple of 2");
	}

	case BarcodeMustStartWith978orBadLength:
	{
		return _TB("Invalid input. Must start with 978 and be length must be 9, 10, 12, 13 characters");
	}

	case BarcodeMustStartWith49:
	{
		return _TB("Invalid Country Code for JAN13 (49 required)");
	}

	case BarcodeBarcodeLengthMustBe5or6or9or11:
	{
		return _TB("Invalid data length. (5, 6, 9, or 11 digits only)");
	}

	case BarcodeBarcodeLengthMustBe12:
	{
		return _TB("Data length invalid. (Length must be 12)");
	}

	case BarcodeBarcodeLengthMustBe8or12:
	{
		return _TB("Invalid data length. (8 or 12 numbers only)");
	}

	case BarcodeBarcodeMustBinaryChar:
	{
		return _TB("Invalid Number System (only 0 and 1 are valid)");
	}

	case BarcodeIllegalUPCA:
	{
		return _TB("Illegal UPC-A entered for conversion. Unable to convert");
	}

	case BarcodeBarcodeLengthMustBe2:
	{
		return _TB("Invalid data length. (Length = 2 required)");
	}

	case BarcodeBarcodeLengthMustBe5:
	{
		return _TB("Invalid data length. (Length = 5 required)");
	}

	case BarcodeLengthMustBe12or13:
	{
		return _TB("Data length invalid. (Length must be 12 or 13)");
	}

	case BarcodeLengthMustBe11or12:
	{
		return _TB("Data length invalid. (Length must be 11 or 12)");
	}

	case Barcode1DReaderPluginNotLoaded:
	{
		return _TB("Can't load GdPicture.NET.9.barcode.1d.reader.dll");
	}

	case Barcode1DReaderUnknownError:
	{
		return _TB("Unknown error reported from the 1D Barcode reader plugin");
	}

	case BarcodeInvalidDestinationBarSize:
	{
		return _TB("Destination not large enough to draw the barcode because of set size of a single bar");
	}

	case BarcodeSizeZero:
	{
		return _TB("Width of barcode is zero");
	}

	case BarcodeDataMatrixReaderPluginNotLoaded:
	{
		return _TB("Can't load the DataMatrix reader dll");
	}

	case BarcodeDataMatrixReaderUnknownError:
	{
		return _TB("Unknown error reported from the DataMatrix Barcode reader plugin");
	}

	case BarcodeQRCodeReaderPluginNotLoaded:
	{
		return _TB("Can't load the QR-Code reader dll");
	}

	case BarcodePDF417ReaderPluginNotLoaded:
	{
		return _TB("Can't load GdPicture.NET.9.barcode.pdf417.reader.dll");
	}

	case BarcodeQrEncoderInvalidVersion:
	{
		return _TB("The version provided to the Qr-Code encoder is invalid");
	}

	case BarcodeQrEncoderNotNumericData:
	{
		return _TB("The Qr-Code expected only numeric data");
	}

	case BarcodeQrEncoderNotAlphanumericData:
	{
		return _TB("The Qr-Code expected only alpha-numeric data");
	}

	case BarcodeQrEncoderNot8BitData:
	{
		return _TB("The Qr-Code expected only byte data");
	}

	case BarcodeQrEncoderNotKanjiData:
	{
		return _TB("The Qr-Code expected only kanji data");
	}

	case BarcodeDatamatrixEncoderInvalidVersion:
	{
		return _TB("The version provided to the DataMatrix encoder is invalid");
	}

	case BarcodePDF417EncoderInvalidVersion:
	{
		return _TB("The version provided to the PDF417 encoder is invalid");
	}

	case BarcodePDF417EncoderNotNumericData:
	{
		return _TB("The PDF417 encoder expected only numeric data");
	}

	case BarcodePDF417EncoderNotTextData:
	{
		return _TB("The PDF417 encoder expected only alpha-numeric data");
	}

	case BarcodePDF417EncoderNotByteData:
	{
		return _TB("The PDF417 encoder expected only byte data");
	}

	case EncryptionSchemeNotSupported:
	{
		return _TB("The selected encryption scheme is not supported");
	}

	case JBIG2PluginNotLoaded:
	{
		return _TB("Can't load the GdPicture JBIG2 encoder library");
	}

	case DocumentAnalyzerDllRequired:
	{
		return _TB("Can't load the GdPicture Document analyzer encoder library");
	}

	case InvalidLicense:
	{
		return _TB("The license key provided doesn't include this feature");
	}

	case WIAGenericError:
	{
		return _TB("WIA error. Use WiaGetLastError() functions for diagnosing the error");
	}
	default:
		return _TB("Unknown error occured");
	}
}