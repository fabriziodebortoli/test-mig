
#pragma once

//includere alla fine degli include del .H
#include "beginh.dex"

#define VtPenStyleNull 							0
#define VtPenStyleSolid 						1
#define VtPenStyleDashed 						2
#define VtPenStyleDotted						3
#define VtPenStyleDashDot						4
#define VtPenStyleDashDotDot					5
#define VtPenStyleDitted						6
#define VtPenStyleDashDit						7
#define VtPenStyleDashDitDit					8
#define VtPenStyleNative						9

class CWalkBitmap;

//=============================================================================
//			Class CGDIObjCombo
//=============================================================================
class TB_EXPORT CGDIObjCombo : public CBCGPComboBox
{
	DECLARE_DYNAMIC (CGDIObjCombo)

public:
	// Construction
	CGDIObjCombo();
public:
	
	// dynamic subclassing (don't use in conjunction with create)
	BOOL 	SubclassGDIObjCombo	(UINT nID, CWnd* pParent);

protected:
	// user overridable	
	virtual BOOL	InitCtrl			();

protected:
	//{{AFX_MSG(CGDIObjCombo)
	afx_msg void	OnKeyDown	(UINT nChar, UINT nRepCnt, UINT nFlags);
	//}}AFX_MSG

	DECLARE_MESSAGE_MAP()
};

//=============================================================================
//			Class CFillColorCombo
//=============================================================================
class TB_EXPORT CFillColorCombo : public CGDIObjCombo
{
	DECLARE_DYNAMIC (CFillColorCombo)


public:
	void		SetValue		(COLORREF	lColor);
	COLORREF	GetValue		();

protected:
	virtual void	DrawItem		(LPDRAWITEMSTRUCT lpDrawItem);

protected:
	// user overridable	
	virtual BOOL	InitCtrl			();
	void			FillColors			();

protected:
	//{{AFX_MSG(CFillColorCombo)
	//}}AFX_MSG

	DECLARE_MESSAGE_MAP()
};

//=============================================================================
//			Class CPenStyleCombo
//=============================================================================
class TB_EXPORT CPenStyleCombo : public CGDIObjCombo
{
	DECLARE_DYNAMIC (CPenStyleCombo)
protected:
	int m_nPenWidth;

public:
	void			SetValue	(int 	nPenStyle);
	int				GetValue	();
	void			SetPenWidth (int	nWidth){m_nPenWidth = nWidth; UpdateWindow();}

protected:
	virtual void	DrawItem	(LPDRAWITEMSTRUCT lpDrawItem);

protected:
	// user overridable	
	virtual BOOL	InitCtrl			();
	void			FillPenStyles		();

protected:
	//{{AFX_MSG(CPenStyleCombo)
	//}}AFX_MSG

	DECLARE_MESSAGE_MAP()
};


//=============================================================================
//			Class CChartPenStyleCombo
//=============================================================================
class TB_EXPORT CChartPenStyleCombo : public CGDIObjCombo
{
	DECLARE_DYNAMIC (CChartPenStyleCombo)

public:
	// Contruction
	CChartPenStyleCombo		();
	// Destruction
	~CChartPenStyleCombo	();

protected:
	CWalkBitmap* m_pbmpPenStylesList;

public:
	void			SetValue	(short);
	short			GetValue	();

protected:
	virtual void	DrawItem	(LPDRAWITEMSTRUCT lpDrawItem);
 	virtual void	MeasureItem	(LPMEASUREITEMSTRUCT lpMeasureItem);

protected:
	// user overridable	
	virtual BOOL	InitCtrl			();
	void			FillPenStyles		();

protected:
	//{{AFX_MSG(CChartPenStyleCombo)
	//}}AFX_MSG

	DECLARE_MESSAGE_MAP()
};

#include "endh.dex"
