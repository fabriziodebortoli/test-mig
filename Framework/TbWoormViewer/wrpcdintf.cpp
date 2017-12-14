// Machine generated IDispatch wrapper class(es) created with ClassWizard

#include "stdafx.h"
#include "wrpcdintf.h"

#include "begincpp.dex"

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

/////////////////////////////////////////////////////////////////////////////
// ICDIntf properties

BOOL ICDIntf::GetFontEmbedding()
{
	BOOL result;
	GetProperty(0x1, VT_BOOL, (void*)&result);
	return result;
}

void ICDIntf::SetFontEmbedding(BOOL propVal)
{
	SetProperty(0x1, VT_BOOL, propVal);
}

BOOL ICDIntf::GetPageContentCompression()
{
	BOOL result;
	GetProperty(0x2, VT_BOOL, (void*)&result);
	return result;
}

void ICDIntf::SetPageContentCompression(BOOL propVal)
{
	SetProperty(0x2, VT_BOOL, propVal);
}

BOOL ICDIntf::GetJPEGCompression()
{
	BOOL result;
	GetProperty(0x3, VT_BOOL, (void*)&result);
	return result;
}

void ICDIntf::SetJPEGCompression(BOOL propVal)
{
	SetProperty(0x3, VT_BOOL, propVal);
}

short ICDIntf::GetPaperSize()
{
	short result;
	GetProperty(0x4, VT_I2, (void*)&result);
	return result;
}

void ICDIntf::SetPaperSize(short propVal)
{
	SetProperty(0x4, VT_I2, propVal);
}

long ICDIntf::GetPaperWidth()
{
	long result;
	GetProperty(0x5, VT_I4, (void*)&result);
	return result;
}

void ICDIntf::SetPaperWidth(long propVal)
{
	SetProperty(0x5, VT_I4, propVal);
}

long ICDIntf::GetPaperLength()
{
	long result;
	GetProperty(0x6, VT_I4, (void*)&result);
	return result;
}

void ICDIntf::SetPaperLength(long propVal)
{
	SetProperty(0x6, VT_I4, propVal);
}

short ICDIntf::GetOrientation()
{
	short result;
	GetProperty(0x7, VT_I2, (void*)&result);
	return result;
}

void ICDIntf::SetOrientation(short propVal)
{
	SetProperty(0x7, VT_I2, propVal);
}

long ICDIntf::GetResolution()
{
	long result;
	GetProperty(0x8, VT_I4, (void*)&result);
	return result;
}

void ICDIntf::SetResolution(long propVal)
{
	SetProperty(0x8, VT_I4, propVal);
}

CString ICDIntf::GetDefaultDirectory()
{
	CString result;
	GetProperty(0x9, VT_BSTR, (void*)&result);
	return result;
}

void ICDIntf::SetDefaultDirectory(LPCTSTR propVal)
{
	SetProperty(0x9, VT_BSTR, propVal);
}

CString ICDIntf::GetDefaultFileName()
{
	CString result;
	GetProperty(0xa, VT_BSTR, (void*)&result);
	return result;
}

void ICDIntf::SetDefaultFileName(LPCTSTR propVal)
{
	SetProperty(0xa, VT_BSTR, propVal);
}

short ICDIntf::GetFileNameOptions()
{
	short result;
	GetProperty(0xb, VT_I2, (void*)&result);
	return result;
}

void ICDIntf::SetFileNameOptions(short propVal)
{
	SetProperty(0xb, VT_I2, propVal);
}

short ICDIntf::GetHorizontalMargin()
{
	short result;
	GetProperty(0xc, VT_I2, (void*)&result);
	return result;
}

void ICDIntf::SetHorizontalMargin(short propVal)
{
	SetProperty(0xc, VT_I2, propVal);
}

short ICDIntf::GetVerticalMargin()
{
	short result;
	GetProperty(0xd, VT_I2, (void*)&result);
	return result;
}

void ICDIntf::SetVerticalMargin(short propVal)
{
	SetProperty(0xd, VT_I2, propVal);
}

BOOL ICDIntf::GetHTMLUseLayers()
{
	BOOL result;
	GetProperty(0xe, VT_BOOL, (void*)&result);
	return result;
}

void ICDIntf::SetHTMLUseLayers(BOOL propVal)
{
	SetProperty(0xe, VT_BOOL, propVal);
}

BOOL ICDIntf::GetHTMLMultipleHTMLs()
{
	BOOL result;
	GetProperty(0xf, VT_BOOL, (void*)&result);
	return result;
}

void ICDIntf::SetHTMLMultipleHTMLs(BOOL propVal)
{
	SetProperty(0xf, VT_BOOL, propVal);
}

long ICDIntf::GetAttributes()
{
	long result;
	GetProperty(0x1f, VT_I4, (void*)&result);
	return result;
}

void ICDIntf::SetAttributes(long propVal)
{
	SetProperty(0x1f, VT_I4, propVal);
}

BOOL ICDIntf::GetRTFFullRTF()
{
	BOOL result;
	GetProperty(0x24, VT_BOOL, (void*)&result);
	return result;
}

void ICDIntf::SetRTFFullRTF(BOOL propVal)
{
	SetProperty(0x24, VT_BOOL, propVal);
}

BOOL ICDIntf::GetRTFTextRTF()
{
	BOOL result;
	GetProperty(0x25, VT_BOOL, (void*)&result);
	return result;
}

void ICDIntf::SetRTFTextRTF(BOOL propVal)
{
	SetProperty(0x25, VT_BOOL, propVal);
}

BOOL ICDIntf::GetRTFTextOnly()
{
	BOOL result;
	GetProperty(0x26, VT_BOOL, (void*)&result);
	return result;
}

void ICDIntf::SetRTFTextOnly(BOOL propVal)
{
	SetProperty(0x26, VT_BOOL, propVal);
}

short ICDIntf::GetJPegLevel()
{
	short result;
	GetProperty(0x29, VT_I2, (void*)&result);
	return result;
}

void ICDIntf::SetJPegLevel(short propVal)
{
	SetProperty(0x29, VT_I2, propVal);
}

CString ICDIntf::GetServerAddress()
{
	CString result;
	GetProperty(0x2d, VT_BSTR, (void*)&result);
	return result;
}

void ICDIntf::SetServerAddress(LPCTSTR propVal)
{
	SetProperty(0x2d, VT_BSTR, propVal);
}

long ICDIntf::GetServerPort()
{
	long result;
	GetProperty(0x2e, VT_I4, (void*)&result);
	return result;
}

void ICDIntf::SetServerPort(long propVal)
{
	SetProperty(0x2e, VT_I4, propVal);
}

CString ICDIntf::GetServerUsername()
{
	CString result;
	GetProperty(0x2f, VT_BSTR, (void*)&result);
	return result;
}

void ICDIntf::SetServerUsername(LPCTSTR propVal)
{
	SetProperty(0x2f, VT_BSTR, propVal);
}

CString ICDIntf::GetEmailFieldTo()
{
	CString result;
	GetProperty(0x30, VT_BSTR, (void*)&result);
	return result;
}

void ICDIntf::SetEmailFieldTo(LPCTSTR propVal)
{
	SetProperty(0x30, VT_BSTR, propVal);
}

CString ICDIntf::GetEmailFieldCC()
{
	CString result;
	GetProperty(0x31, VT_BSTR, (void*)&result);
	return result;
}

void ICDIntf::SetEmailFieldCC(LPCTSTR propVal)
{
	SetProperty(0x31, VT_BSTR, propVal);
}

CString ICDIntf::GetEmailFieldBCC()
{
	CString result;
	GetProperty(0x32, VT_BSTR, (void*)&result);
	return result;
}

void ICDIntf::SetEmailFieldBCC(LPCTSTR propVal)
{
	SetProperty(0x32, VT_BSTR, propVal);
}

CString ICDIntf::GetEmailSubject()
{
	CString result;
	GetProperty(0x33, VT_BSTR, (void*)&result);
	return result;
}

void ICDIntf::SetEmailSubject(LPCTSTR propVal)
{
	SetProperty(0x33, VT_BSTR, propVal);
}

CString ICDIntf::GetEmailMessage()
{
	CString result;
	GetProperty(0x34, VT_BSTR, (void*)&result);
	return result;
}

void ICDIntf::SetEmailMessage(LPCTSTR propVal)
{
	SetProperty(0x34, VT_BSTR, propVal);
}

BOOL ICDIntf::GetEmailPrompt()
{
	BOOL result;
	GetProperty(0x35, VT_BOOL, (void*)&result);
	return result;
}

void ICDIntf::SetEmailPrompt(BOOL propVal)
{
	SetProperty(0x35, VT_BOOL, propVal);
}

long ICDIntf::GetFileNameOptionsEx()
{
	long result;
	GetProperty(0x3c, VT_I4, (void*)&result);
	return result;
}

void ICDIntf::SetFileNameOptionsEx(long propVal)
{
	SetProperty(0x3c, VT_I4, propVal);
}

/////////////////////////////////////////////////////////////////////////////
// ICDIntf operations

long ICDIntf::CreateDC()
{
	long result;
	InvokeHelper(0x10, DISPATCH_METHOD, VT_I4, (void*)&result, NULL);
	return result;
}

BOOL ICDIntf::SetDefaultConfig()
{
	BOOL result;
	InvokeHelper(0x11, DISPATCH_METHOD, VT_BOOL, (void*)&result, NULL);
	return result;
}

BOOL ICDIntf::SetDefaultPrinter()
{
	BOOL result;
	InvokeHelper(0x12, DISPATCH_METHOD, VT_BOOL, (void*)&result, NULL);
	return result;
}

BOOL ICDIntf::StartSpooler()
{
	BOOL result;
	InvokeHelper(0x13, DISPATCH_METHOD, VT_BOOL, (void*)&result, NULL);
	return result;
}

BOOL ICDIntf::StopSpooler()
{
	BOOL result;
	InvokeHelper(0x14, DISPATCH_METHOD, VT_BOOL, (void*)&result, NULL);
	return result;
}

long ICDIntf::PDFDriverInit(LPCTSTR PrinterName)
{
	long result;
	static BYTE parms[] =
		VTS_BSTR;
	InvokeHelper(0x15, DISPATCH_METHOD, VT_I4, (void*)&result, parms,
		PrinterName);
	return result;
}

long ICDIntf::HTMLDriverInit(LPCTSTR PrinterName)
{
	long result;
	static BYTE parms[] =
		VTS_BSTR;
	InvokeHelper(0x16, DISPATCH_METHOD, VT_I4, (void*)&result, parms,
		PrinterName);
	return result;
}

long ICDIntf::EMFDriverInit(LPCTSTR PrinterName)
{
	long result;
	static BYTE parms[] =
		VTS_BSTR;
	InvokeHelper(0x17, DISPATCH_METHOD, VT_I4, (void*)&result, parms,
		PrinterName);
	return result;
}

void ICDIntf::DriverEnd()
{
	InvokeHelper(0x18, DISPATCH_METHOD, VT_EMPTY, NULL, NULL);
}

CString ICDIntf::GetLastErrorMsg()
{
	CString result;
	InvokeHelper(0x19, DISPATCH_METHOD, VT_BSTR, (void*)&result, NULL);
	return result;
}

BOOL ICDIntf::RestoreDefaultPrinter()
{
	BOOL result;
	InvokeHelper(0x1a, DISPATCH_METHOD, VT_BOOL, (void*)&result, NULL);
	return result;
}

long ICDIntf::DriverInit(LPCTSTR PrinterName)
{
	long result;
	static BYTE parms[] =
		VTS_BSTR;
	InvokeHelper(0x1b, DISPATCH_METHOD, VT_I4, (void*)&result, parms,
		PrinterName);
	return result;
}

CString ICDIntf::GetDocumentTitle(long JobID)
{
	CString result;
	static BYTE parms[] =
		VTS_I4;
	InvokeHelper(0x1c, DISPATCH_METHOD, VT_BSTR, (void*)&result, parms,
		JobID);
	return result;
}

long ICDIntf::SetBookmark(long hDC, long lParent, LPCTSTR Title)
{
	long result;
	static BYTE parms[] =
		VTS_I4 VTS_I4 VTS_BSTR;
	InvokeHelper(0x1d, DISPATCH_METHOD, VT_I4, (void*)&result, parms,
		hDC, lParent, Title);
	return result;
}

long ICDIntf::CaptureEvents(long bCapture)
{
	long result;
	static BYTE parms[] =
		VTS_I4;
	InvokeHelper(0x1e, DISPATCH_METHOD, VT_I4, (void*)&result, parms,
		bCapture);
	return result;
}

long ICDIntf::SetWatermark(LPCTSTR Watermark, LPCTSTR FontName, short FontSize, short Orientation, long Color, long HorzPos, long VertPos, long Foreground)
{
	long result;
	static BYTE parms[] =
		VTS_BSTR VTS_BSTR VTS_I2 VTS_I2 VTS_I4 VTS_I4 VTS_I4 VTS_I4;
	InvokeHelper(0x20, DISPATCH_METHOD, VT_I4, (void*)&result, parms,
		Watermark, FontName, FontSize, Orientation, Color, HorzPos, VertPos, Foreground);
	return result;
}

long ICDIntf::SetHyperLink(long hDC, LPCTSTR URL)
{
	long result;
	static BYTE parms[] =
		VTS_I4 VTS_BSTR;
	InvokeHelper(0x21, DISPATCH_METHOD, VT_I4, (void*)&result, parms,
		hDC, URL);
	return result;
}

BOOL ICDIntf::SetDefaultConfigEx()
{
	BOOL result;
	InvokeHelper(0x22, DISPATCH_METHOD, VT_BOOL, (void*)&result, NULL);
	return result;
}

long ICDIntf::RTFDriverInit(LPCTSTR PrinterName)
{
	long result;
	static BYTE parms[] =
		VTS_BSTR;
	InvokeHelper(0x23, DISPATCH_METHOD, VT_I4, (void*)&result, parms,
		PrinterName);
	return result;
}

long ICDIntf::SendMessagesTo(LPCTSTR WndClass)
{
	long result;
	static BYTE parms[] =
		VTS_BSTR;
	InvokeHelper(0x27, DISPATCH_METHOD, VT_I4, (void*)&result, parms,
		WndClass);
	return result;
}

long ICDIntf::BatchConvert(LPCTSTR FileName)
{
	long result;
	static BYTE parms[] =
		VTS_BSTR;
	InvokeHelper(0x28, DISPATCH_METHOD, VT_I4, (void*)&result, parms,
		FileName);
	return result;
}

long ICDIntf::Lock(LPCTSTR szLockName)
{
	long result;
	static BYTE parms[] =
		VTS_BSTR;
	InvokeHelper(0x2a, DISPATCH_METHOD, VT_I4, (void*)&result, parms,
		szLockName);
	return result;
}

long ICDIntf::Unlock(LPCTSTR szLockName, long dwTimeout)
{
	long result;
	static BYTE parms[] =
		VTS_BSTR VTS_I4;
	InvokeHelper(0x2b, DISPATCH_METHOD, VT_I4, (void*)&result, parms,
		szLockName, dwTimeout);
	return result;
}

long ICDIntf::SendMail(LPCTSTR szTo, LPCTSTR szCC, LPCTSTR szBCC, LPCTSTR szSubject, LPCTSTR szMessage, LPCTSTR szFilenames, long lOptions)
{
	long result;
	static BYTE parms[] =
		VTS_BSTR VTS_BSTR VTS_BSTR VTS_BSTR VTS_BSTR VTS_BSTR VTS_I4;
	InvokeHelper(0x2c, DISPATCH_METHOD, VT_I4, (void*)&result, parms,
		szTo, szCC, szBCC, szSubject, szMessage, szFilenames, lOptions);
	return result;
}

long ICDIntf::TestLock(LPCTSTR szLockName)
{
	long result;
	static BYTE parms[] =
		VTS_BSTR;
	InvokeHelper(0x36, DISPATCH_METHOD, VT_I4, (void*)&result, parms,
		szLockName);
	return result;
}

long ICDIntf::GLock()
{
	long result;
	InvokeHelper(0x37, DISPATCH_METHOD, VT_I4, (void*)&result, NULL);
	return result;
}

long ICDIntf::GUnlock()
{
	long result;
	InvokeHelper(0x38, DISPATCH_METHOD, VT_I4, (void*)&result, NULL);
	return result;
}

long ICDIntf::SetDocEmailProps(LPCTSTR szDocTitle, LPCTSTR szTo, LPCTSTR szCC, LPCTSTR szBCC, LPCTSTR szSubject, LPCTSTR szMessage, long lPrompt)
{
	long result;
	static BYTE parms[] =
		VTS_BSTR VTS_BSTR VTS_BSTR VTS_BSTR VTS_BSTR VTS_BSTR VTS_I4;
	InvokeHelper(0x39, DISPATCH_METHOD, VT_I4, (void*)&result, parms,
		szDocTitle, szTo, szCC, szBCC, szSubject, szMessage, lPrompt);
	return result;
}

long ICDIntf::SetDocServerProps(LPCTSTR szDocTitle, LPCTSTR szHostname, LPCTSTR szUsername)
{
	long result;
	static BYTE parms[] =
		VTS_BSTR VTS_BSTR VTS_BSTR;
	InvokeHelper(0x3a, DISPATCH_METHOD, VT_I4, (void*)&result, parms,
		szDocTitle, szHostname, szUsername);
	return result;
}

long ICDIntf::SetDocFileProps(LPCTSTR szDocTitle, long lOptions, LPCTSTR szFileDir, LPCTSTR szFileName)
{
	long result;
	static BYTE parms[] =
		VTS_BSTR VTS_I4 VTS_BSTR VTS_BSTR;
	InvokeHelper(0x3b, DISPATCH_METHOD, VT_I4, (void*)&result, parms,
		szDocTitle, lOptions, szFileDir, szFileName);
	return result;
}


/////////////////////////////////////////////////////////////////////////////
// IDIDocument properties

CString IDIDocument::GetTitle()
{
	CString result;
	GetProperty(0x1, VT_BSTR, (void*)&result);
	return result;
}

void IDIDocument::SetTitle(LPCTSTR propVal)
{
	SetProperty(0x1, VT_BSTR, propVal);
}

CString IDIDocument::GetSubject()
{
	CString result;
	GetProperty(0x2, VT_BSTR, (void*)&result);
	return result;
}

void IDIDocument::SetSubject(LPCTSTR propVal)
{
	SetProperty(0x2, VT_BSTR, propVal);
}

CString IDIDocument::GetCreator()
{
	CString result;
	GetProperty(0x3, VT_BSTR, (void*)&result);
	return result;
}

void IDIDocument::SetCreator(LPCTSTR propVal)
{
	SetProperty(0x3, VT_BSTR, propVal);
}

CString IDIDocument::GetAuthor()
{
	CString result;
	GetProperty(0x4, VT_BSTR, (void*)&result);
	return result;
}

void IDIDocument::SetAuthor(LPCTSTR propVal)
{
	SetProperty(0x4, VT_BSTR, propVal);
}

long IDIDocument::GetLinearized()
{
	long result;
	GetProperty(0x13, VT_I4, (void*)&result);
	return result;
}

void IDIDocument::SetLinearized(long propVal)
{
	SetProperty(0x13, VT_I4, propVal);
}

CString IDIDocument::GetPageMode()
{
	CString result;
	GetProperty(0x14, VT_BSTR, (void*)&result);
	return result;
}

void IDIDocument::SetPageMode(LPCTSTR propVal)
{
	SetProperty(0x14, VT_BSTR, propVal);
}

/////////////////////////////////////////////////////////////////////////////
// IDIDocument operations

BOOL IDIDocument::Open(LPCTSTR FileName)
{
	BOOL result;
	static BYTE parms[] =
		VTS_BSTR;
	InvokeHelper(0x6, DISPATCH_METHOD, VT_BOOL, (void*)&result, parms,
		FileName);
	return result;
}

BOOL IDIDocument::Save(LPCTSTR FileName)
{
	BOOL result;
	static BYTE parms[] =
		VTS_BSTR;
	InvokeHelper(0x7, DISPATCH_METHOD, VT_BOOL, (void*)&result, parms,
		FileName);
	return result;
}

BOOL IDIDocument::Append(LPCTSTR FileName)
{
	BOOL result;
	static BYTE parms[] =
		VTS_BSTR;
	InvokeHelper(0x8, DISPATCH_METHOD, VT_BOOL, (void*)&result, parms,
		FileName);
	return result;
}

BOOL IDIDocument::AppendEx(LPDISPATCH Document)
{
	BOOL result;
	static BYTE parms[] =
		VTS_DISPATCH;
	InvokeHelper(0x9, DISPATCH_METHOD, VT_BOOL, (void*)&result, parms,
		Document);
	return result;
}

BOOL IDIDocument::SetBookmark(long Page, LPCTSTR Text, long Level)
{
	BOOL result;
	static BYTE parms[] =
		VTS_I4 VTS_BSTR VTS_I4;
	InvokeHelper(0xa, DISPATCH_METHOD, VT_BOOL, (void*)&result, parms,
		Page, Text, Level);
	return result;
}

void IDIDocument::ClearBookmarks()
{
	InvokeHelper(0xb, DISPATCH_METHOD, VT_EMPTY, NULL, NULL);
}

BOOL IDIDocument::SearchText(short Start, LPCTSTR Text, long* Page, double* xPos, double* yPos)
{
	BOOL result;
	static BYTE parms[] =
		VTS_I2 VTS_BSTR VTS_PI4 VTS_PR8 VTS_PR8;
	InvokeHelper(0xc, DISPATCH_METHOD, VT_BOOL, (void*)&result, parms,
		Start, Text, Page, xPos, yPos);
	return result;
}

BOOL IDIDocument::Merge(LPCTSTR FileName, long Options)
{
	BOOL result;
	static BYTE parms[] =
		VTS_BSTR VTS_I4;
	InvokeHelper(0xd, DISPATCH_METHOD, VT_BOOL, (void*)&result, parms,
		FileName, Options);
	return result;
}

BOOL IDIDocument::MergeEx(LPDISPATCH Document, long Options)
{
	BOOL result;
	static BYTE parms[] =
		VTS_DISPATCH VTS_I4;
	InvokeHelper(0xe, DISPATCH_METHOD, VT_BOOL, (void*)&result, parms,
		Document, Options);
	return result;
}

BOOL IDIDocument::Encrypt(LPCTSTR OwnerPassword, LPCTSTR UserPassword, long Permissions)
{
	BOOL result;
	static BYTE parms[] =
		VTS_BSTR VTS_BSTR VTS_I4;
	InvokeHelper(0xf, DISPATCH_METHOD, VT_BOOL, (void*)&result, parms,
		OwnerPassword, UserPassword, Permissions);
	return result;
}

long IDIDocument::PageCount()
{
	long result;
	InvokeHelper(0x10, DISPATCH_METHOD, VT_I4, (void*)&result, NULL);
	return result;
}

BOOL IDIDocument::OpenEx(LPCTSTR FileName, LPCTSTR Password)
{
	BOOL result;
	static BYTE parms[] =
		VTS_BSTR VTS_BSTR;
	InvokeHelper(0x11, DISPATCH_METHOD, VT_BOOL, (void*)&result, parms,
		FileName, Password);
	return result;
}

void IDIDocument::SetFlateCompression(short Ratio)
{
	static BYTE parms[] =
		VTS_I2;
	InvokeHelper(0x12, DISPATCH_METHOD, VT_EMPTY, NULL, parms,
		 Ratio);
}
