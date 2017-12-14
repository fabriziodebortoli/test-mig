#pragma once

#include "dib.h"

#include "TBThemeManager.h"

//includere alla fine degli include del .H
#include "beginh.dex"

//=============================================================================
class TB_EXPORT CDIBitmap : public CGdiObject
{
	DECLARE_DYNAMIC(CDIBitmap)

public:
	// Constructors
	CDIBitmap   ()
		{ 
			m_hObject = NULL;
			m_bShowProportional = FALSE;
		}

	~CDIBitmap  ();

	BOOL			m_bShowProportional;

public:
	BOOL CreateBitmap   (const CDIBitmap& bmp, CSize sz);
	BOOL CreateBitmap   (const CDIBitmap& bmp);
	BOOL CreateBitmap   (const CBitmap& bmp, DWORD biStyle, WORD biBits, const CPalette& pal, WORD wUsage = DIB_RGB_COLORS);
	BOOL CreateBitmap   (LPCTSTR lpBitmapName);
	BOOL SaveBitmap     (LPCTSTR lpBitmapName);

	BOOL IsOK           () const 	{ return GetSafeHandle() != NULL; }

	BOOL DibInfo        (LPBITMAPINFOHEADER lpbi) const 	{ return ::DibInfo(GetSafeHandle(), lpbi); }

	BOOL SetDibUsage    (const CPalette& pal, WORD wUsage = DIB_RGB_COLORS)
		{ return ::SetDibUsage(GetSafeHandle(), (HPALETTE) pal.GetSafeHandle(), wUsage); }

	BOOL DibBitBlt      (CDC&, const CRect& dest, DWORD rop, WORD usage = DIB_RGB_COLORS);
	BOOL DibStretchBlt  (CDC&, const CRect& dest, DWORD rop, WORD usage = DIB_RGB_COLORS);

	BOOL DibBitBlt      (CDC&, const CRect& dest, const CPoint& src, DWORD rop, WORD usage = DIB_RGB_COLORS);
	BOOL DibStretchBlt  (CDC&, const CRect& dest, const CRect& src, DWORD rop, WORD usage = DIB_RGB_COLORS);

	CSize   GetSize       () const;
	int     GetWidth      () const;
	int     GetHeight     () const;
	WORD    GetPaletteSize() const;
	WORD    GetNumColors  () const;

	static CDIBitmap*	FromHandle(HANDLE hBitmap)
		{ return (CDIBitmap*) CGdiObject::FromHandle(hBitmap); }

	static HBITMAP		LoadBitmap(LPCTSTR sPath);
};

//=============================================================================
class TB_EXPORT CCheckBitmap : public CObject
{
	DECLARE_DYNAMIC(CCheckBitmap)

public:
	BITMAP	m_bmpInfo;
	UINT	m_nIDB;
	CString m_sNamespaceImage;

public:
	// Constructors
	CCheckBitmap			(UINT IdBmp);
	CCheckBitmap			(CString namespaceImg);
	virtual ~CCheckBitmap	();

public:
	BOOL	FloodDrawBitmap	(HDC hdc, int x, int y, DWORD rop, COLORREF foreColor, COLORREF bkgrColor, BOOL bChecked = TRUE, BOOL bDrawFarme = FALSE, HBRUSH = NULL);
	int		GetWidth		() { return m_bmpInfo.bmWidth; }
	int		GetHeight		() { return m_bmpInfo.bmHeight; }
};

//---------------------------------------------------------------------------------
class TB_EXPORT CTransBmp : public CBitmap
{
private:
    int			m_iWidth;
    int			m_iHeight;
    HBITMAP		m_hbmMask; // Handle to mask bitmap
	COLORREF	m_clrTransparent;

public:
    CTransBmp	(COLORREF = AfxGetThemeManager()->GetTransBmpTransparentDefaultColor());
    ~CTransBmp	();

public:
	void	Draw			(HDC hDC, int x, int y);
    void	Draw			(CDC* pDC, int x, int y);
    void	DrawTransparent	(HDC hDC, int x, int y);
    void	DrawTransparent	(CDC* pDC, int x, int y);
    int		GetWidth		();
    int		GetHeight		();

    void	GetMetrics		();
    void	CreateMask		(HDC hDC);

	void	SetTransparentColor	(const COLORREF& aColor) { m_clrTransparent = aColor; }
};

/////////////////////////////////////////////////////////////////////////////
// CTranspBmpCtrl window

class TB_EXPORT CTranspBmpCtrl : public CStatic
{
// Construction
public:
	CTranspBmpCtrl(int, BOOL bTransparent = TRUE);
	CTranspBmpCtrl(const CString& sBmpName, BOOL bTransparent = TRUE);

// Attributes
public:
	int			m_nBmpRef;
	CString		m_sBmpRefName;
	BOOL		m_bTransparent;
	CTransBmp	m_TransparentBmp;
// Operations
public:
	void ChangeBitMap(const CString& sBmpName, BOOL bTransparent = TRUE);
	void ChangeBitMap(int, BOOL bTransparent = TRUE);

// Overrides
	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(CTranspBmpCtrl)
	protected:
	virtual void PreSubclassWindow();
	//}}AFX_VIRTUAL

// Implementation
public:
	virtual ~CTranspBmpCtrl();
	
	BOOL InitCtrl();
	// Generated message map functions
protected:
	//{{AFX_MSG(CTranspBmpCtrl)
	afx_msg void OnPaint();
	afx_msg LRESULT OnGetControlDescription(WPARAM wParam, LPARAM lParam);
	//}}AFX_MSG

	DECLARE_MESSAGE_MAP()
};

// Usefull C++ routines
//
//-----------------------------------------------------------------------------
inline BOOL PaletteEq(const CPalette& p1, const CPalette& p2)
	{ return ::PaletteEq((HPALETTE) p1.GetSafeHandle(), (HPALETTE) p2.GetSafeHandle()); }

inline BOOL SetPaletteFlags(const CPalette& pal, int iIndex, int cntEntries, WORD wFlags)
	{ return ::SetPaletteFlags ((HPALETTE) pal.GetSafeHandle(), iIndex, cntEntries, wFlags); }


#include "endh.dex"
