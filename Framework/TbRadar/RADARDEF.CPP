
#include "stdafx.h"

#include "radardef.h"

//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char BASED_CODE THIS_FILE[] = __FILE__;
#endif

///////////////////////////////////////////////////////////////////////////////
// Diagnostics RadarIntObject
///////////////////////////////////////////////////////////////////////////////
#ifdef _DEBUG
void RadarIntObject::Dump (CDumpContext& dc) const
{
	ASSERT_VALID (this);
	AFX_DUMP0(dc, " RadarIntObject\n");
	CObject::Dump(dc);
}
#endif // _DEBUG

///////////////////////////////////////////////////////////////////////////////
// Implementazione di RadarIntArray
///////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
RadarIntObject *RadarIntArray::GetAt (int nIdx) const
{
	ASSERT (!(nIdx < 0 || nIdx >= GetSize()));
	RadarIntObject *pIntObj = (RadarIntObject*) Array::GetAt(nIdx);
	ASSERT (pIntObj);
	return pIntObj;
}

///////////////////////////////////////////////////////////////////////////////
// Diagnostics
#ifdef _DEBUG
void RadarIntArray::Dump (CDumpContext& dc) const
{
	ASSERT_VALID (this);
	AFX_DUMP0(dc, " RadarIntArray\n");
	Array::Dump(dc);
}
#endif // _DEBUG

///////////////////////////////////////////////////////////////////////////////
// Implementazione di RadarColumnIdxObj
///////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
RadarColumnIdxObj::RadarColumnIdxObj (CString& szTitle, int nColIdx)
	:
	m_nIdx		(nColIdx),
	m_szTitle	(szTitle)
{
}

///////////////////////////////////////////////////////////////////////////////
// Diagnostics
#ifdef _DEBUG
void RadarColumnIdxObj::Dump (CDumpContext& dc) const
{
	ASSERT_VALID (this);
	dc << " RadarColumnIdxObj\n\tTitolo Colonna: " << m_szTitle << "\n";
	CObject::Dump(dc);
}
#endif // _DEBUG

///////////////////////////////////////////////////////////////////////////////
// Implementazione di RadarColumnsIdxArray
///////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
RadarColumnIdxObj* RadarColumnsIdxArray::GetAt (int nIdx)
{
	ASSERT (nIdx >= 0 && nIdx < GetSize());
	RadarColumnIdxObj* pCol = (RadarColumnIdxObj*)(Array::GetAt(nIdx));
	return pCol;
}

///////////////////////////////////////////////////////////////////////////////
// Diagnostics
#ifdef _DEBUG
void RadarColumnsIdxArray::Dump (CDumpContext& dc) const
{
	ASSERT_VALID (this);
	AFX_DUMP0(dc, " RadarColumnsIdxArray\n");
	Array::Dump(dc);
}
#endif // _DEBUG

///////////////////////////////////////////////////////////////////////////////
// Implementazione delle funzioni di uso generale
///////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
CFont* CreateRadarDefaultFont(int nMult)
{
	LOGFONT logFont;
	return CreateRadarDefaultFont (logFont, nMult);
}

//-----------------------------------------------------------------------------
CFont* CreateRadarDefaultFont(LOGFONT& logFont, int nMult)
{
	CClientDC dcScreen(NULL);
	return CreateRadarFont (logFont, -MulDiv(nMult, dcScreen.GetDeviceCaps(LOGPIXELSY), 72));
}

//-----------------------------------------------------------------------------
CFont* CreateRadarFont
			(
				LOGFONT&	logFont,
				int			lfHeight,
				CString		strFaceName,
				int			lfWeight,
				BYTE		lfItalic,
				BYTE		lfPitchAndFamily
			)
{
	CFont* pFont = new CFont;
	memset(&logFont, 0, sizeof(logFont));
	// Since design guide says toolbars are fixed height so is the font.
	CClientDC dcScreen(NULL);
	logFont.lfHeight = lfHeight;
	logFont.lfWeight = lfWeight;
	logFont.lfItalic = lfItalic;
	logFont.lfPitchAndFamily = lfPitchAndFamily;
	TB_TCSCPY(logFont.lfFaceName, strFaceName);
	if (!pFont->CreateFontIndirect(&logFont))
	{
		TRACE("Could Not create Font. Use system font.");
		pFont->FromHandle((HFONT)::GetStockObject(SYSTEM_FONT));
	}
	ASSERT (pFont);
	return pFont;
}

//-----------------------------------------------------------------------------
CFont* CreateRadarFont (LOGFONT& logFont)
{
	CFont* pFont = new CFont;
	if (!pFont->CreateFontIndirect(&logFont))
	{
		TRACE("Could Not create Font. Use system font.");
		pFont->FromHandle((HFONT)::GetStockObject(SYSTEM_FONT));
	}
	ASSERT (pFont);
	return pFont;
}

//-----------------------------------------------------------------------------
void GetRadarTextMetrix (CFont* pFont,TEXTMETRIC& tm)
{
	// get text metrics of font
	ASSERT (pFont);
	CClientDC dcScreen(NULL);
	CFont* pOldFont = dcScreen.SelectObject(pFont);
	VERIFY (dcScreen.GetTextMetrics(&tm));
	dcScreen.SelectObject(pOldFont);
}
