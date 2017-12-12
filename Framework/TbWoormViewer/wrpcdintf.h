#pragma once

//includere alla fine degli include del .H
#include "beginh.dex"

// Machine generated IDispatch wrapper class(es) created with ClassWizard
/////////////////////////////////////////////////////////////////////////////
// ICDIntf wrapper class

class TB_EXPORT ICDIntf : public COleDispatchDriver
{
public:
	enum EPaperSize { Letter=1, Legal=5, A3=8, A4=9, Custom=256 };
	enum EOrientation { Portrait=1, Landscape=2 };
	enum EFileNameOptions { NoPrompt=1, UseDefaultFileName=2, Concatenate=4, DisablePageContentCompression=8, EmbedFonts=16, BroadcastMessages=32 };

public:
	ICDIntf() {}		// Calls COleDispatchDriver default constructor
	ICDIntf(LPDISPATCH pDispatch) : COleDispatchDriver(pDispatch) {}
	ICDIntf(const ICDIntf& dispatchSrc) : COleDispatchDriver(dispatchSrc) {}

// Attributes
public:
	BOOL GetFontEmbedding();
	void SetFontEmbedding(BOOL);

	BOOL GetPageContentCompression();
	void SetPageContentCompression(BOOL);

	BOOL GetJPEGCompression();
	void SetJPEGCompression(BOOL);

	short GetPaperSize();
	void SetPaperSize(short);	// EPaperSize { Letter=1, Legal=5, A3=8, A4=9, Custom=256 };

	long GetPaperWidth();
	void SetPaperWidth(long);	// mm/10, =0 if PaperSize <> Custom
	long GetPaperLength();		
	void SetPaperLength(long);	// mm/10, =0 if PaperSize <> Custom

	short GetOrientation();
	void SetOrientation(short);	// EOrientation { Portrait=1, Landscape=2 }

	long GetResolution();
	void SetResolution(long);	// 75, 150, 300, 600, 1200

	CString GetDefaultDirectory();
	void SetDefaultDirectory(LPCTSTR);
	CString GetDefaultFileName();
	void SetDefaultFileName(LPCTSTR);

	short GetFileNameOptions();
	void SetFileNameOptions(short);		//EFileNameOptions { NoPrompt=1, UseDefaultFileName=2, Concatenate=4, DisablePageContentCompression=8, EmbedFonts=16, BroadcastMessages=32 }

	short GetHorizontalMargin();
	void SetHorizontalMargin(short);	// mm/10, default=60
	short GetVerticalMargin();
	void SetVerticalMargin(short);		// mm/10, default=60

	BOOL GetHTMLUseLayers();
	void SetHTMLUseLayers(BOOL);
	BOOL GetHTMLMultipleHTMLs();
	void SetHTMLMultipleHTMLs(BOOL);

	long GetAttributes();
	void SetAttributes(long);

	BOOL GetRTFFullRTF();
	void SetRTFFullRTF(BOOL);
	BOOL GetRTFTextRTF();
	void SetRTFTextRTF(BOOL);
	BOOL GetRTFTextOnly();
	void SetRTFTextOnly(BOOL);

	short GetJPegLevel();
	void SetJPegLevel(short);

	CString GetServerAddress();
	void SetServerAddress(LPCTSTR);
	long GetServerPort();
	void SetServerPort(long);
	CString GetServerUsername();
	void SetServerUsername(LPCTSTR);

	CString GetEmailFieldTo();
	void SetEmailFieldTo(LPCTSTR);
	CString GetEmailFieldCC();
	void SetEmailFieldCC(LPCTSTR);
	CString GetEmailFieldBCC();
	void SetEmailFieldBCC(LPCTSTR);
	CString GetEmailSubject();
	void SetEmailSubject(LPCTSTR);
	CString GetEmailMessage();
	void SetEmailMessage(LPCTSTR);

	BOOL GetEmailPrompt();
	void SetEmailPrompt(BOOL);

	long GetFileNameOptionsEx();
	void SetFileNameOptionsEx(long);

// Operations
public:
	long CreateDC();
	BOOL SetDefaultConfig();
	BOOL SetDefaultPrinter();
	BOOL StartSpooler();
	BOOL StopSpooler();
	long PDFDriverInit(LPCTSTR PrinterName);
	long HTMLDriverInit(LPCTSTR PrinterName);
	long EMFDriverInit(LPCTSTR PrinterName);
	void DriverEnd();
	CString GetLastErrorMsg();
	BOOL RestoreDefaultPrinter();
	long DriverInit(LPCTSTR PrinterName);
	CString GetDocumentTitle(long JobID);
	long SetBookmark(long hDC, long lParent, LPCTSTR Title);
	long CaptureEvents(long bCapture);
	long SetWatermark(LPCTSTR Watermark, LPCTSTR FontName, short FontSize, short Orientation, long Color, long HorzPos, long VertPos, long Foreground);
	long SetHyperLink(long hDC, LPCTSTR URL);
	BOOL SetDefaultConfigEx();
	long RTFDriverInit(LPCTSTR PrinterName);
	long SendMessagesTo(LPCTSTR WndClass);
	long BatchConvert(LPCTSTR FileName);
	long Lock(LPCTSTR szLockName);
	long Unlock(LPCTSTR szLockName, long dwTimeout);
	long SendMail(LPCTSTR szTo, LPCTSTR szCC, LPCTSTR szBCC, LPCTSTR szSubject, LPCTSTR szMessage, LPCTSTR szFilenames, long lOptions);
	long TestLock(LPCTSTR szLockName);
	long GLock();
	long GUnlock();
	long SetDocEmailProps(LPCTSTR szDocTitle, LPCTSTR szTo, LPCTSTR szCC, LPCTSTR szBCC, LPCTSTR szSubject, LPCTSTR szMessage, long lPrompt);
	long SetDocServerProps(LPCTSTR szDocTitle, LPCTSTR szHostname, LPCTSTR szUsername);
	long SetDocFileProps(LPCTSTR szDocTitle, long lOptions, LPCTSTR szFileDir, LPCTSTR szFileName);
};

/////////////////////////////////////////////////////////////////////////////
// IDIDocument wrapper class
class TB_EXPORT IDIDocument : public COleDispatchDriver
{
public:
	IDIDocument() {}		// Calls COleDispatchDriver default constructor
	IDIDocument(LPDISPATCH pDispatch) : COleDispatchDriver(pDispatch) {}
	IDIDocument(const IDIDocument& dispatchSrc) : COleDispatchDriver(dispatchSrc) {}

	enum EPermissions 
			{ 
				eEnablePrinting					= 4, 
				eEnableChanging					= 8, 
				eEnableCopyngTextAndGraphics	= 16,
				eEnableAddingAndChangingNotes	= 32,
				eEncrypted						= 0xFFFFFFC0
			};

// Attributes
public:
	CString GetTitle();
	void SetTitle(LPCTSTR);
	CString GetSubject();
	void SetSubject(LPCTSTR);
	CString GetCreator();
	void SetCreator(LPCTSTR);
	CString GetAuthor();
	void SetAuthor(LPCTSTR);
	long GetLinearized();
	void SetLinearized(long);
	CString GetPageMode();
	void SetPageMode(LPCTSTR);

// Operations
public:
	BOOL Open(LPCTSTR FileName);
	BOOL Save(LPCTSTR FileName);
	BOOL Append(LPCTSTR FileName);
	BOOL AppendEx(LPDISPATCH Document);
	BOOL SetBookmark(long Page, LPCTSTR Text, long Level);
	void ClearBookmarks();
	BOOL SearchText(short Start, LPCTSTR Text, long* Page, double* xPos, double* yPos);
	BOOL Merge(LPCTSTR FileName, long Options);
	BOOL MergeEx(LPDISPATCH Document, long Options);
	BOOL Encrypt(LPCTSTR OwnerPassword, LPCTSTR UserPassword, long Permissions);
	long PageCount();
	BOOL OpenEx(LPCTSTR FileName, LPCTSTR Password);
	void SetFlateCompression(short Ratio);
};

#include "endh.dex"
