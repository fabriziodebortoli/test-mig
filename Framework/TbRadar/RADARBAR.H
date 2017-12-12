
#pragma once

//includere alla fine degli include del .H
#include "beginh.dex"

class CriticalArea;

class RadarBody;
class RadarColumn;

class CRadarFrame;
class CRadarView;

// Classi definite all'interno del modulo
//=============================================================================
class CRadarButtonBar;
	class CRadarTitleBar;

//	Definizione di CRadarTitleBar
//=============================================================================
class TB_EXPORT CRadarTitleBar : public CBCGPControlBar
{
	DECLARE_DYNAMIC(CRadarTitleBar)

private:
	// Font di default
	CRadarFrame*	m_pRadFrame;
	CFont*			m_pRBFont;

	int m_nHeight;
	int m_nXNewOrigin;

	CriticalArea*	m_pProcessingMouse;
	RadarBody*		m_pRadarBody;

private:
	void DPToLP (CPoint&);

public:
	CRadarTitleBar (CRadarFrame*);
	virtual ~CRadarTitleBar ();

	BOOL Create (CWnd*);

public:
	// Richiesta di informazioni
	int				GetRowHeight	();
	CRadarFrame*	GetRadarFrame	();
	CRadarView* 	GetRadarView	();

	void DrawLine			(int nXPos, int nDrawMode);
	int	 GetHeight			() { return m_nHeight; }

	void ScrollTitle		(int nDPOffset, BOOL bRedraw = TRUE);
	void UpdateAndSetNewOrigin (int nDPOffset);
	void ShowGrpCondMenu	(CPoint& LPMousePos);
	void SelectAllColumns	(BOOL bSelAll);
	void InvalidateUpdateRect(int nXStart, int nXEnd);

	void DrawSepLine	(CDC*, int iXS, int iYS, int iXE, int iYE, COLORREF cr);
	void FontChanged	(LOGFONT logFont);

public:
	virtual void	AttachRadarBody		(RadarBody*);
	virtual CSize	CalcFixedLayout		(BOOL bStretch, BOOL bHorz);
	virtual BOOL	CursorMustChange 	(CPoint&);
	virtual void	LeftButtonDown		(UINT, CPoint&);
	
protected:
	virtual void OnUpdateCmdUI	(CFrameWnd*, BOOL) {};
	virtual void DoPaint	(CDC*);

	//{{AFX_MSG(CRadarTitleBar)
		afx_msg void OnLButtonUp	(UINT, CPoint);
		afx_msg void OnMouseMove	(UINT, CPoint);
		afx_msg void OnRButtonDown	(UINT, CPoint);
		afx_msg BOOL OnEraseBkgnd	(CDC* pDC);

		afx_msg LRESULT	OnGetFont		(WPARAM, LPARAM)	{ return (LRESULT)(UINT)(HFONT)(m_pRBFont->m_hObject); }
		afx_msg void	OnLButtonDown	(UINT, CPoint);
		afx_msg BOOL	OnSetCursor		(CWnd*, UINT, UINT);
		afx_msg void	OnSize			(UINT, int, int)	{ Invalidate(); }

	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()

// Diagnostics
#ifdef _DEBUG
public:
	void Dump		(CDumpContext&)	const;
	void AssertValid()				const 
	{ 
		__super::AssertValid();
	}
#endif // _DEBUG
};

#include "endh.dex"

