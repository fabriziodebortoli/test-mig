
#pragma once

#include <TbGeneric\Array.h>

//includere alla fine degli include del .H
#include "beginh.dex"

/////////////////////////////////////////////////////////////////////////////
// Define di uso generale

// Messaggi di l'aggiornamento emmessi sulla UpdateAllView
#define UPDATE_VIEW_SCROLL_TITLEBAR 		1
#define UPDATE_SCROLL_SIZE					2
#define UPDATE_VIEW							3
#define UPDATE_SCROLL_SIZE_FORCE_REPAINT	4
#define UPDATE_FROM_ROW_DOWN				5
#define UPDATE_FROM_ROW_UP					6
#define UPDATE_FROM_PAGE_DOWN				7
#define UPDATE_FROM_PAGE_UP					8
#define UPDATE_OPTIMIZATION					9

// Attributi grafici
#define DEFAULT_COLUMN_OFFSET	4
#define MULTI_LINES_MIN			1
#define MULTI_LINES_MAX			5

#define COLUMN_CHAR_WIDTH_MIN_VALUE 4
#define COLUMN_CHAR_WIDTH_MAX_VALUE 32
//
enum TB_EXPORT ActiveAndFirst {START_FROM_TOP, START_FROM_BOTTOM, FROM_FIND_NEXT, FROM_FIND_PREV, FROM_SHOW_ROW, MULTI_LINES};

// Identifica i cursori e i relativi messaggi nella status bar in base all'attuale
// posizione del mouse
enum TB_EXPORT SelectCursor {ARROW, COL_RESIZE, LB_SEL_ROW, LB_SEL_COL, TIB_SEL_COL, INS_DEL_COL};

//	Definizione di RadarIntObject
//=============================================================================
class TB_EXPORT RadarIntObject : public CObject
{
private:
	int m_nInt;

public:
	RadarIntObject (int nInt)	{ m_nInt = nInt; }

	int GetInt () { return m_nInt; }

// Diagnostics
#ifdef _DEBUG
public:
	void Dump		(CDumpContext&)	const;
	void AssertValid()				const { CObject::AssertValid(); }
#endif // _DEBUG
};

//	Definizione di RadarIntArray
//=============================================================================
class TB_EXPORT RadarIntArray : public Array
{
public:
	int	AddInt	(int nInt)					{ return Array::Add (new RadarIntObject (nInt)); }
	int	Add		(RadarIntObject* pIntObj)	{ return Array::Add (pIntObj); }

	RadarIntObject*	GetAt	(int nIdx) const;
	int				GetInt	(int nIdx) const { return GetAt (nIdx)->GetInt(); }

	void InsertAt	(int nIdx, RadarIntObject *pIntObj)	{ Array::InsertAt (nIdx, pIntObj); }
	void InsertIntAt(int nIdx, int nInt)				{ Array::InsertAt (nIdx, new RadarIntObject (nInt)); }

	RadarIntObject* operator [] (int nIdx)	{ return GetAt (nIdx); }

// Diagnostics
#ifdef _DEBUG
public:
	void Dump		(CDumpContext&)	const;
	void AssertValid()				const { Array::AssertValid(); }
#endif // _DEBUG
};

//	Definizione di RadarColumnIdxObj
//=============================================================================
class TB_EXPORT RadarColumnIdxObj : public CObject
{
protected:
	int		m_nIdx;
	CString	m_szTitle;

public:
	RadarColumnIdxObj (CString& szTitle, int nColIdx);

public:
	LPCTSTR	GetTitle	() { return m_szTitle;	}
	int		GetIdx		() { return m_nIdx;		}

// Diagnostics
#ifdef _DEBUG
public:
	void Dump		(CDumpContext&)	const;
	void AssertValid()				const { CObject::AssertValid(); }
#endif // _DEBUG
};

//	Definizione di RadarColumnsIdxArray
//=============================================================================
class TB_EXPORT RadarColumnsIdxArray : public Array
{
public:
	int Add	(RadarColumnIdxObj* pObj) { return Array::Add(pObj); }
	int Add	(CString szTitle, int nDBCol)
				{ return Array::Add(new RadarColumnIdxObj(szTitle, nDBCol)); }

public:
	RadarColumnIdxObj* GetAt (int nIdx);
	RadarColumnIdxObj* operator [] (int nIdx) { return GetAt(nIdx); }
	
// Diagnostics
#ifdef _DEBUG
public:
	void Dump		(CDumpContext&)	const;
	void AssertValid()				const { Array::AssertValid(); }
#endif // _DEBUG
};

///////////////////////////////////////////////////////////////////////////////
///////////////////////////////////////////////////////////////////////////////
//
//	Font functions
//
///////////////////////////////////////////////////////////////////////////////
///////////////////////////////////////////////////////////////////////////////

TB_EXPORT CFont*	CreateRadarDefaultFont	(int nMult = 8);
TB_EXPORT CFont*	CreateRadarDefaultFont	(LOGFONT&, int nMult = 8);
TB_EXPORT CFont*	CreateRadarFont
			(
				LOGFONT&,
				int			lfHeight,
				CString		strFaceName		= CString(_T("Verdana")),
				int			lfWeight		= FW_REGULAR,
				BYTE		lfItalic		= 0,
				BYTE		lfPitchAndFamily= VARIABLE_PITCH | FF_SWISS
			);
TB_EXPORT CFont* CreateRadarFont		(LOGFONT&);
TB_EXPORT void	 GetRadarTextMetrix		(CFont*, TEXTMETRIC&);

#include "endh.dex"
