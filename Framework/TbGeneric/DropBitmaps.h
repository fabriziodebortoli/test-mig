#pragma once

#include <afxtempl.h>

#include "CISBitmap.h"
#include "TBThemeManager.h"

//=============================================================================

//includere come ultimo include all'inizio del .h
#include "beginh.dex"


// mask 
#define	BMP_TOP				0x0001
#define	BMP_BOTTOM			0x0002
#define	BMP_LEFT			0x0004
#define	BMP_RIGHT			0x0008

#define BMP_ALL				0xFFFF

class  TB_EXPORT CTBExtDropBitmaps : public CObject
{
protected:
	CCISBitmap*			m_pBmpLeft;
	CCISBitmap*			m_pBmpRight;
	CCISBitmap*			m_pBmpTop;
	CCISBitmap*			m_pBmpBottom;

	CSize				m_sLR;		// dim indicatori left / right
	CSize				m_sTD;		// dim indicatori top / bottom
public:
	CTBExtDropBitmaps();

	virtual ~CTBExtDropBitmaps();

	// ciclo di vita delle bitmap...
	void	Release();
	void	Create();
	void	Create(CString idbTOP_ARROW, CString idbBOTTOM_ARROW, CString idbLEFT_ARROW, CString idbRIGHT_ARROW);
	void	Create(UINT idbTOP_ARROW, UINT idbBOTTOM_ARROW, UINT idbLEFT_ARROW, UINT idbRIGHT_ARROW);

	
	// disegno...
	void	DrawTop(CDC*	pDC, int x, int y)
	{
		if (m_pBmpTop)
			m_pBmpTop->DrawTransparent(pDC, x, y, AfxGetThemeManager()->GetTBExtDropBitmapsTransparentColor());
	}
	void	DrawBottom(CDC*	pDC, int x, int y)
	{
		if (m_pBmpBottom)
			m_pBmpBottom->DrawTransparent(pDC, x, y, AfxGetThemeManager()->GetTBExtDropBitmapsTransparentColor());
	}
	void	DrawLeft(CDC*	pDC, int x, int y)
	{
		if (m_pBmpLeft)
			m_pBmpLeft->DrawTransparent(pDC, x, y, AfxGetThemeManager()->GetTBExtDropBitmapsTransparentColor());
	}
	void	DrawRight(CDC*	pDC, int x, int y)
	{
		if (m_pBmpRight)
			m_pBmpRight->DrawTransparent(pDC, x, y, AfxGetThemeManager()->GetTBExtDropBitmapsTransparentColor());
	}

	void	Draw(CDC*	pDc, CRect rect, UINT what, BOOL bInside);
	
protected:
	static void Load(CCISBitmap*& pointer, UINT idb);
	static void Load(CCISBitmap*& pointer, CString idb);
};

#include "endh.dex"
//=============================================================================
