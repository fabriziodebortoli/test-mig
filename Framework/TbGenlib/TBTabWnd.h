#pragma once

#include "beginh.dex"

class CBaseDocument;

// qui ho dovuto derivare uno oggetto  perche' tutti i membri bcg sono 
// protetti e gestiti friend delle classi originali.
//======================================================================
class TB_EXPORT CTaskBuilderTabWndInfo : public CBCGPTabInfo
{
	DECLARE_DYNAMIC(CTaskBuilderTabWndInfo);

public:
	CTaskBuilderTabWndInfo(const CString& strText, UINT nTabID);
	~CTaskBuilderTabWndInfo();

public:
	void SetVisible		(BOOL bVisible);
	void Enable			(BOOL bEnabled);
	void SetForeColor	(COLORREF color);
};

/////////////////////////////////////////////////////////////////////////////
//			CTaskBuilderTabWnd
/////////////////////////////////////////////////////////////////////////////
class TB_EXPORT CTaskBuilderTabWnd : public CBCGPTabWnd
{
	DECLARE_DYNAMIC(CTaskBuilderTabWnd);

	friend class CTabSelector;

public:
	// i primi 9 elementi li considero il CBCGPTabWnd::Style
	enum TBStyle { TB_STRIP = 20 };

private:
	BOOL		m_bDeleteTabInfo;
	TBStyle		m_TbStyle;
	COLORREF	m_HoveringBkgColor;
	COLORREF	m_HoveringForeColor;
	BOOL		m_HasTabTopBorder;

	BOOL		ProcessSysKeyMessage(MSG* pMsg);

protected:
	CBaseDocument*	m_pDocument;

public:
	CTaskBuilderTabWnd();
	~CTaskBuilderTabWnd();

public:
	void AddTabByInfo	(CTaskBuilderTabWndInfo* pInfo, int nPos = -1);
	BOOL Create			(int tbStyle, const RECT& rect, CWnd* pParentWnd, UINT nID, Location location = LOCATION_BOTTOM, BOOL bCloseBtn = FALSE);

	BOOL		HasTopTabBorder		() const;
	void		SetHasTopTabBorder(BOOL bValue);
	void		AdjustTabTopBorder(CRect& aRect) const;

	int			GetTabIndexOf		(CWnd* pWnd);
	void		SetTabImageOf		(CWnd* pWnd, UINT nID);
	
	TBStyle		GetTbStyle			() const;
	void		SetTbStyle			(TBStyle style);
	void		ModifyTabberStyle	(int style);

	COLORREF	GetHoveringBkgColor() const;
	void		SetHoveringBkgColor(COLORREF color);
	COLORREF	GetHoveringForeColor() const;
	void		SetHoveringForeColor(COLORREF color);

	virtual CFont*		GetTabFont();
	virtual COLORREF	GetBestTabBkgColor(int nTab, BOOL isActive = FALSE) const;
	virtual COLORREF	GetBestTabForeColor(int nTab, BOOL isActive = FALSE) const;

	virtual	BOOL		PreTranslateMessage	(MSG* pMsg);
	virtual void		AttachDocument(CBaseDocument* pDoc) = 0;

	virtual int			GetDefaultHeight() const;
	static BOOL			PreProcessSysKeyMessage(MSG* pMsg, CBaseDocument* pDoc, CWnd* pWnd);
	CBCGPTabInfo*		FindTabInfo(int nTab);
	
};

#include "endh.dex"
