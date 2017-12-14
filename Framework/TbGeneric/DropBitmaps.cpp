#include "StdAfx.h"

#include <TbFrameworkImages\CommonImages.h>
#include <TbFrameworkImages\GeneralFunctions.h>


#include "globals.h"

#include "DropBitmaps.h"

#include "dialogs.hjson" //JSON AUTOMATIC UPDATE

//-----------------------------------------------------------------------------
void CTBExtDropBitmaps::Load(CCISBitmap*& pointer, UINT idb)
{
	if(pointer == NULL)
	{
		pointer = new CCISBitmap();
		VERIFY(pointer->LoadBitmap(idb));
	}
}

//-----------------------------------------------------------------------------
CTBExtDropBitmaps::CTBExtDropBitmaps()
{
	m_pBmpLeft		= NULL;
	m_pBmpRight		= NULL;
	m_pBmpTop		= NULL;
	m_pBmpBottom	= NULL;
}


//-----------------------------------------------------------------------------
CTBExtDropBitmaps::~CTBExtDropBitmaps()
{
	Release();
}

//-----------------------------------------------------------------------------
void CTBExtDropBitmaps::Release()
{
	SAFE_DELETE(m_pBmpLeft);
	SAFE_DELETE(m_pBmpRight);
	SAFE_DELETE(m_pBmpTop);
	SAFE_DELETE(m_pBmpBottom);
}

//-----------------------------------------------------------------------------
void CTBExtDropBitmaps::Create(UINT idbTOP_ARROW, UINT idbBOTTOM_ARROW, UINT idbLEFT_ARROW, UINT idbRIGHT_ARROW)
{
	Load(m_pBmpTop, idbTOP_ARROW);
	Load(m_pBmpBottom, idbBOTTOM_ARROW);
	Load(m_pBmpLeft, idbLEFT_ARROW);
	Load(m_pBmpRight, idbRIGHT_ARROW);

	m_sLR.cx = m_pBmpLeft->Width();
	m_sLR.cy = m_pBmpLeft->Height();
	m_sTD.cx = m_pBmpTop->Width();
	m_sTD.cy = m_pBmpTop->Height();
}

//-----------------------------------------------------------------------------
void CTBExtDropBitmaps::Load(CCISBitmap*& pointer, CString idb)
{
	if (pointer == NULL)
	{
		pointer = new CCISBitmap();
		VERIFY(pointer->Attach(TBLoadImage(idb)));
	}
}


//-----------------------------------------------------------------------------
void CTBExtDropBitmaps::Create(CString idbTOP_ARROW, CString idbBOTTOM_ARROW, CString idbLEFT_ARROW, CString idbRIGHT_ARROW)
{
	Load(m_pBmpTop, idbTOP_ARROW);
	Load(m_pBmpBottom, idbBOTTOM_ARROW);
	Load(m_pBmpLeft, idbLEFT_ARROW);
	Load(m_pBmpRight, idbRIGHT_ARROW);

	m_sLR.cx = m_pBmpLeft->Width();
	m_sLR.cy = m_pBmpLeft->Height();
	m_sTD.cx = m_pBmpTop->Width();
	m_sTD.cy = m_pBmpTop->Height();
}

//-----------------------------------------------------------------------------
void CTBExtDropBitmaps::Create()
{

	Create(	TBIcon(szIconArrowDown, IconSize::CONTROL), TBIcon(szIconArrowUp, IconSize::CONTROL),
			TBIcon(szIconArrowLeft, IconSize::CONTROL), TBIcon(szIconArrowRight, IconSize::CONTROL));
}		

//-----------------------------------------------------------------------------
void	CTBExtDropBitmaps::Draw		(CDC*	pDc, CRect rect, UINT what, BOOL bInside)
{
	BOOL	bLeft	= what & BMP_LEFT;
	BOOL	bRight	= what & BMP_RIGHT;
	BOOL	bTop	= what & BMP_TOP;
	BOOL	bBottom	= what & BMP_BOTTOM;

	if (bLeft)
	{
		int	x = rect.left	- ( bInside ? 0 : m_sLR.cx );
		int	y = rect.top	+ ( rect.Height() - m_sLR.cy ) / 2;		// centra in Y
		DrawLeft(pDc, x, y);
	}

	if (bRight)
	{
		int	x = rect.right	- ( bInside ? m_sLR.cx : 0 );
		int	y = rect.top	+ ( rect.Height() - m_sLR.cy ) / 2;		// centra in Y
		DrawRight(pDc, x, y);
	}
	
	if (bTop)
	{
		int	x = rect.left	+ ( rect.Width () - m_sTD.cx ) / 2;		// centra in X
		int	y = rect.top	- ( bInside ? 0 : m_sTD.cy) ;		
		DrawTop(pDc, x, y);
	}

	if (bBottom)
	{
		int	x = rect.left	+ ( rect.Width () - m_sTD.cx ) / 2;		// centra in X
		int	y = rect.bottom	- ( bInside ? m_sTD.cy : 0) ;		
		DrawBottom(pDc, x, y);
	}
}
