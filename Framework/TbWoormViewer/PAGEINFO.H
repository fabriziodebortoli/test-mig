
#pragma once

#include <TbGenlib\parsctrl.h>
#include <TbGeneric\Array.h>
#include <TbGeneric\dib.h>  //for CopyHandle method

//includere alla fine degli include del .H
#include "beginh.dex"

class Parser;
class Unparser;
class CPageSetupDlg;
class Array;
				
#define SET_CAPABILITY(a,b)	if (lpDevMode->dmFields & nSetFlags & a)\
								lpDevMode->##b = ##b;

#define SET_CAPABILITY2(a,b,c)	if (lpDevMode->dmFields & nSetFlags & a)\
	lpDevMode->##b = ##c.##b;

// indica che non si seleziona un formato particolare ma dimensioni speciali
#define DMPAPER_SPECIAL		0
#define A4_WIDTH			2100
#define A4_HEIGHT			2970
#define MU_DECIMAL			3
#define MU_SCALE			100.
#define PAGE_PRECISION		0
#define MARGIN_PRECISION	2


//===========================================================================
class CLongArraySorted : public CArray<LONG,LONG>
{
private:
	LONG m_nMinValue;
	LONG m_nMaxValue;
	
public:
	CLongArraySorted(){m_nMinValue = 0; m_nMaxValue = MAXLONG;}

//-----------------------------------------------------------------------------
	void SetMaxValue(LONG nMaxValue){ m_nMaxValue = nMaxValue;}

//-----------------------------------------------------------------------------	
	INT_PTR Add(LONG nValue)
	{
		return AddSorted(nValue);
	}

//-----------------------------------------------------------------------------
	INT_PTR AddSorted(LONG nValue)
	{
		if (nValue > m_nMaxValue || nValue < m_nMinValue )
			return -1;

		for (int i = 0; i < GetSize(); i++)
		{	
			if(GetAt(i) >= nValue)
			{
				InsertAt(i, nValue);		
				return i;
			}
		}
		int insertionIndex = GetSize();
		SetAtGrow(insertionIndex, nValue);
		return insertionIndex;
	}

	//-----------------------------------------------------------------------------
	INT_PTR AddSortedUnique(LONG nValue)
	{
		if (nValue > m_nMaxValue || nValue < m_nMinValue)
			return -1;

		for (int i = 0; i < GetSize(); i++)
		{
			if(GetAt(i) == nValue)
				return -1;

			if (GetAt(i) >= nValue)
			{
				InsertAt(i, nValue);
				return i;
			}
		}
		int insertionIndex = GetSize();
		SetAtGrow(insertionIndex, nValue);
		return insertionIndex;
	}

//-----------------------------------------------------------------------------
	BOOL Remove(LONG nValue)
	{
		for (int i = 0; i < GetSize(); i++)
		{	
			if(GetAt(i) == nValue)
			{
				RemoveAt(i);
				return TRUE;
			};
		}
		return FALSE;
	}

//-----------------------------------------------------------------------------
	BOOL operator == (const CLongArraySorted& source)
	{
		if (GetSize() != source.GetSize())
			return FALSE;
		
		for (int i = 0; i < GetSize(); i++)
		{
			if (GetAt(i) != source.GetAt(i))
				return FALSE;
		}
		return TRUE;
	}
};

//===========================================================================
// CSampleIcon window
//===========================================================================

//-----------------------------------------------------------------------------
class TB_EXPORT CSampleIcon : public CButton
{
private:
	BOOL	m_bLandscape;
	CPoint	m_ptUlc;
	CSize	m_sizeRefRectSize;
	
// Operations
public:
			void	SetRefRect		(const CPoint& ptUlc, const CSize& sizeRectSize);
			void	SizeToContent	(double ratio);

// Implementation
protected:
	virtual void	DrawItem		(LPDRAWITEMSTRUCT lpDrawItemStruct);

	//{{AFX_MSG(CSampleIcon)
	afx_msg BOOL OnEraseBkgnd(CDC* pDC);
	//}}AFX_MSG

	DECLARE_MESSAGE_MAP()
};

//=============================================================================
class TB_EXPORT PrinterInfoItem : public CObject
{
	DECLARE_DYNAMIC (PrinterInfoItem)

public:
	CString	m_strPaperSize;
    WORD	m_wPaperType;
    CSize	m_sPaperSize;

public:
	PrinterInfoItem
	(
		CString	strPaperSize,
		WORD	wPaperType,
		CSize	sPaperSize
	);

	PrinterInfoItem (const PrinterInfoItem&);

public:
	PrinterInfoItem&	operator=(const PrinterInfoItem&);
};
//===========================================================================
class TB_EXPORT PrinterInfo
{
public:
	// informazioni recuperate dalla DeviceCapabilities applicate
	// alla stampante di default
	Array	m_PrinterPaperInfo;
	CString m_strDriverName;
	CString m_strPrinterName;
	CString m_strPortName;

	CString m_strPreferredPrinter;

	WORD	m_wMinWidth;
	WORD	m_wMaxWidth;
	WORD	m_wMinHeight;
	WORD	m_wMaxHeight;

public:
	PrinterInfo();
	PrinterInfo(CString m_strPreferredPrinter);

private:
	BOOL BypassWin32sBug(WORD *pList, DWORD dwTotal);

public:
	BOOL	FillPaperInfo	();
	int		GetPrinterInfoItem	(WORD wPaperSize);
	PrinterInfoItem* GetPrinterInfoItemObject (WORD wPaperSize);
};


//===========================================================================
class TB_EXPORT PrinterPageInfo
{
public:
	BOOL  m_bUseCloningPrint;  //True, z-print by cloning array m_objects, False z-print moving wnd origin

	// information about physical printer page
    short dmPaperSize;
    short dmPaperLength;	// in decimi di millimetro
    short dmPaperWidth;		// in decimi di millimetro

public:         
	PrinterPageInfo();
	//PrinterPageInfo(const PrinterPageInfo&);

public:                  
	BOOL Parse		(Parser& lex);
	void Unparse	(Unparser& ofile);

	PrinterPageInfo&	operator =	(const PrinterPageInfo&);
	BOOL				operator ==	(const PrinterPageInfo&);

	CSize		GetPageSize_LP		() const; // in pixel (Logical Point)
	CSize		GetPageSize_mm		() const; // in millimetri

	void		CalculatePageSize	(PrinterInfoItem* pInfo, short orientation);
};

//===========================================================================
class TB_EXPORT PageInfo :public CObject
{
	friend	CPageSetupDlg;
	friend class CRSPageProp;

public:
	CRect	m_rectMargins; // in pixel
	BOOL	m_bUsePrintableArea;

	// copertina semplificata alla struttura DEVMODE 
    DWORD dmFields;
    short dmOrientation;
    short dmPaperSize;
    short dmPaperLength;	// in decimi di millimetro
    short dmPaperWidth;		// in decimi di millimetro
    short dmScale;
    short dmCollate;
    short dmPrintQuality;

	PrinterPageInfo m_PrinterPageInfo;

	CLongArraySorted m_arHPageSplitter; //horizontal offset for z-print
	//CLongArraySorted m_arVPageSplitter; //vertical offset for z-print

private:
    short dmCopies;

	CString m_strPreferredPrinter;
	//handles to printer
	HANDLE m_hDevMode;
	HANDLE m_hDevNames;

public:         
	PageInfo();
	PageInfo(const PageInfo&);
	~PageInfo();

// operators
public:
	PageInfo&	operator =	(const PageInfo&);
	BOOL		operator ==	(const PageInfo&);
	BOOL		operator !=	(const PageInfo&);
	
public:
	void	FreePrinterDeviceNames();
	void	FreePrinterDeviceMode();

	BOOL	IsDefault		();

	CSize	GetPageSize_LP			() const; // in pixel
	CSize	GetPrinterPageSize_LP	() const { return m_PrinterPageInfo.GetPageSize_LP(); } // in pixel
	
	CSize	GetPageSize_mm			() const;
	CSize	GetPrinterPageSize_mm	() const { return m_PrinterPageInfo.GetPageSize_mm(); }

	void	CalculateSize	(PrinterInfoItem* pInfo);
	short	GetCopies		() const { return dmCopies; }
	void	SetCopies		(short nCopies);
	void	CalculateMargins(CPrintInfo* pInfo = NULL);
	BOOL	UsePrinterPage();
	void	SetPreferredPrinter	(CString strPreferredPrinter) {m_strPreferredPrinter = strPreferredPrinter;}

	void	SetDevMode	(HANDLE hDevMode);
	void	SetDevNames	(HANDLE hDevNames);
	HANDLE	GetDevMode	();
	HANDLE	GetDevNames	();

public:                  
	BOOL OldParse	(Parser& lex);
	BOOL Parse		(Parser& lex);
	void Unparse	(Unparser& ofile);	

	BOOL GetPrinterCapability	();
	BOOL SetPrinterCapability	(DWORD nSetFlags);
	BOOL SetPrinterCapability	(DWORD nSetFlags, HGLOBAL hDevMode);

	CString GetPreferredPrinter () { return m_strPreferredPrinter; }
};


//===========================================================================
class TB_EXPORT CPageSetupDlg : public CParsedDialog
{
	DECLARE_DYNAMIC(CPageSetupDlg)
// Dialog Data
private:
	CBCGPComboBox	m_PaperSizeCombo;
	CMetricEdit		m_PageWidthEdit;
	CMetricEdit		m_PageHeightEdit;
	CButton			m_PortraitRadio;
	CButton			m_LandscapeRadio;

	//Duplicate info for printer page
	CBCGPComboBox	m_PrinterPaperSizeCombo;
	CMetricEdit		m_PrinterPageWidthEdit;
	CMetricEdit		m_PrinterPageHeightEdit;
	CButton			m_PrinterPortraitRadio;
	CButton			m_PrinterLandscapeRadio;
	
	CSampleIcon		m_SampleIcon;

	CMetricEdit		m_TopMarginEdit;
	CMetricEdit		m_BottomMarginEdit;
	CMetricEdit		m_LeftMarginEdit;
	CMetricEdit		m_RightMarginEdit;
	CIntEdit		m_CopiesEdit;

	PageInfo&		m_PageInfo;
	PrinterInfo		m_PrinterInfo;
	
// Construction
public:
	CPageSetupDlg(PageInfo&, CWnd* pWndParent = NULL);
	CPageSetupDlg(PageInfo&, UINT nIDD, CWnd* pWndParent = NULL);

	// Implementation
protected:                  
    void	ExchangeSize		();
	void	ExchangePrinterSize	();

    void	DrawPageSample		();

    void	EnablePageSize			();
	void	EnablePrinterPageSize	();
    
	void	SetPageSize			();
	void	SetPrinterPageSize	();

	void	SetDefaultMargins	();
            
protected:                  
	virtual BOOL OnInitDialog	();
	virtual void OnOK			();

	// Generated message map functions
	//{{AFX_MSG(CPageSetupDlg)
	afx_msg void		OnSelchangePaperSize	();
	afx_msg void		OnKillfocusPageSize		();
	afx_msg void		OnClickedLandscape		();
	afx_msg void		OnClickedPortrait		();
	afx_msg void		OnSpecialSize			();
	afx_msg void		OnUsePrintableArea		();

	afx_msg void		OnSelchangePrinterPaperSize	();
	afx_msg void		OnClickedPrinterLandscape	();
	afx_msg void		OnClickedPrinterPortrait	();
	afx_msg void		OnPrinterSpecialSize		();
	afx_msg void		OnKillfocusPrinterPageSize	();
	//}}AFX_MSG
	
	DECLARE_MESSAGE_MAP()
};

#include "endh.dex"
