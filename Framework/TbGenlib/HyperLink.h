
#pragma once

#include <TbGenlib\BaseDoc.h>
//includere alla fine degli include del .H
#include "beginh.dex"

class TB_EXPORT CHyperLink : public CBCGPURLLinkButton
{
	DECLARE_DYNAMIC (CHyperLink)

protected:
	// control cui fa riferimento. Il suo parent in realtà è la dialog, altrimenti
	// come child non potrebbe restare attivo quando il suo parent è disabilitato
	CParsedCtrl*	m_pOwner;
	// normalmente il font è quello standard, sul singolo control può essere impostato diverso
	CFont*			m_pFont;
	BOOL			m_bMouseInside;
	HCURSOR			m_hOldCursor;
	HCURSOR			m_hHandCursor;
	// l'hyperlink si rende visibile o meno se il control di riferimento è disabilitato o abilitato,
	// e comunque solo se questo contiene dati. Quindi deve mantenere uno stato di "visibile" quando
	// gli venga detto esplicitamente di nascondersi.
	BOOL			m_bVisible;
	// il documento aperto per seguire l'hyperlink viene mantenuto, per riattivare lo stesso
	// anzichè aprirne uno nuovo sui clic successivi. Inoltre così in fase di distruzione il 
	// documento aperto può essere chiuso
	BaseDocumentPtr	m_pLinkedDocument;
	// l'hyperlink potrebbe dovere essere sempre non visibile, es. se è associato al control
	// di una colonna di bodyedit
	BOOL			m_bAlwaysHidden;
public:
	CHyperLink	(CParsedCtrl*);
	~CHyperLink	(); 

	BOOL Create				(CWnd* pParentWnd);
	void Init				();
	void OverlapToControl	();
	void OverlapToControl	(CRect& rectEdit, UINT nFlags);
	void DoEnable			(BOOL bEnable);
	void UpdateCtrlView		();
	void SetHLinkFont		(CFont* pFont);
	CFont* GetHLinkFont		() { return m_pFont; }
	void ShowCtrl			(int nCmdShow);
	void DoFollowHyperlink	(DataObj* pData = NULL, BOOL bActivate = TRUE);
	void RefreshHyperlink	(DataObj* pData = NULL);
	void SetAlwaysHidden	(BOOL bAlwaysHidden = TRUE)	{ m_bAlwaysHidden = bAlwaysHidden; }
	void NotifyMessage		(UINT message);

	CParsedCtrl* GetOwnerCtrl() { return m_pOwner; }

protected:
	virtual void DrawItem(LPDRAWITEMSTRUCT lpDIS);

private:
	BOOL	ReleaseCapture();

protected:
	//{{AFX_MSG(CHyperLink)
	afx_msg void OnLButtonUp 	(UINT nFlags, CPoint ptMousePos);
	afx_msg void OnRButtonUp	(UINT nFlag, CPoint ptMousePos);
	afx_msg int OnCreate(LPCREATESTRUCT lpCreateStruct);
	afx_msg void OnMouseMove( UINT nFlags, CPoint point );
	afx_msg	void OnFollowHyperlink();
	afx_msg	LRESULT OnGetControlDescription(WPARAM wParam, LPARAM lParam);
	afx_msg void OnShowWindow(BOOL bShow, UINT nStatus);
	//}}AFX_MSG

	DECLARE_MESSAGE_MAP()
};

#include "endh.dex"
