#pragma once

#include "beginh.dex"
#include <TbGeneric\DataObj.h>
#include <TbGenlib\parsobj.h>
#include <TbGenlib\parsedt.h>
#include <TbGenlib\parscbx.h>
#include <TbGes\extdoc.h>
#include <TbGes\ExtDocView.h>
#include <TbGes\dbt.h>
#include <TbGes\BODYEDIT.H>
#include <TbOleDb\Sqltable.h>

#include "PostaLiteTables.h"

class CPostaliteDocumentsGraphicNavigationSplitter;
class CPostaliteDocumentsGraphicNavigationFrame;
class CPostaliteDocumentsGraphicNavigationEdit;
class UIPostaliteDocumentsGraphicNavigation;
class BDPostaliteDocumentsGraphicNavigation;
class CPostaliteDocumentsGraphicNavigationBodyEditView;
//=============================================================================
class TB_EXPORT CPostaliteDocumentsGraphicNavigationSplitter : public CSplitterWnd
{
	DECLARE_DYNAMIC(CPostaliteDocumentsGraphicNavigationSplitter)

// Attributes
public:
    BOOL	m_bPanesSwapped;
    float	m_fSplitRatio;
    int		m_nSplitResolution;

// Implementation
public:
	CPostaliteDocumentsGraphicNavigationSplitter		();
	~CPostaliteDocumentsGraphicNavigationSplitter	();
	CWnd* GetActivePane			(int* pRow = NULL, int* pCol = NULL);

public:
	void SetSplitRatio			(float fRatio );
	BOOL IsSplitHorizontally	() const;
	BOOL IsSplitVertically		() const { return !IsSplitHorizontally(); }
	BOOL ArePanesSwapped		() const { return m_bPanesSwapped; }

protected:
    void UpdateSplitRatio		();
    void UpdatePanes			(int cx, int cy);
    void UpdatePanes			();

public: 
    void SplitVertically		();
    void SplitHorizontally		();

protected: 
	//{{AFX_MSG(CPostaliteDocumentsGraphicNavigationSplitter)
	afx_msg void OnSize		(UINT nType, int cx, int cy);
	afx_msg void OnLButtonUp(UINT uFlags, CPoint point);
	//}}AFX_MSG
    DECLARE_MESSAGE_MAP()
};

/////////////////////////////////////////////////////////////////////////////
//			class CPostaliteDocumentsGraphicNavigationFrame  definition
/////////////////////////////////////////////////////////////////////////////
//
//=============================================================================
class TB_EXPORT CPostaliteDocumentsGraphicNavigationFrame : public CBatchFrame
{
	DECLARE_DYNCREATE(CPostaliteDocumentsGraphicNavigationFrame)
	
public:
	CPostaliteDocumentsGraphicNavigationSplitter m_wndSplitter;

public:
	virtual BOOL OnCustomizeTabbedToolBar(CTBTabbedToolbar* pTabbedBar);
	virtual BOOL CreateStatusBar	();
	virtual BOOL CreateAuxObjects(CCreateContext* pCreateContext);

protected:
	virtual BOOL UseSplitters() { return TRUE; }

};


//=============================================================================
class TB_EXPORT CPostaliteDocumentsGraphicNavigationEdit : public CBodyEdit
{ 
	friend class PostaliteDocumentsGraphicNavigationDetailsStrings;
	
	DECLARE_DYNCREATE(CPostaliteDocumentsGraphicNavigationEdit)
	
public:
	CPostaliteDocumentsGraphicNavigationEdit() : CBodyEdit(_NS_BE("PostaliteDocumentsGraphicNavigationEdit")) {}

public:
	virtual void Customize				();
	virtual BOOL OnGetCustomColor		(CBodyEditRowSelected* pCurrentRow);
	virtual BOOL OnDrawCell				(CBodyEditRowSelected* pCurrentRow, CDC* pDC, CRect& aRect);
	virtual BOOL OnDblClick				(UINT nFlags, CBodyEditRowSelected* pCurrentRow);
	
	DECLARE_MESSAGE_MAP()
};


//=============================================================================
class TB_EXPORT  UIPostaliteDocumentsGraphicNavigation : public CMasterFormView
{
	friend class BDPostaliteDocumentsGraphicNavigation;
	DECLARE_DYNCREATE(UIPostaliteDocumentsGraphicNavigation)

public:
	UIPostaliteDocumentsGraphicNavigation();
	UIPostaliteDocumentsGraphicNavigation(UINT);

public:
	BDPostaliteDocumentsGraphicNavigation*	GetDocument	() const;

public:
	virtual	void BuildDataControlLinks();
};

//=============================================================================
class TB_EXPORT CPostaliteDocumentsGraphicNavigationBodyEditView : public CMasterFormView
{
	DECLARE_DYNCREATE(CPostaliteDocumentsGraphicNavigationBodyEditView)
	
public:	
	CPostaliteDocumentsGraphicNavigationBodyEditView();

public:
	BDPostaliteDocumentsGraphicNavigation* GetDocument() const;

public:		
	virtual	void BuildDataControlLinks();

protected:
	void OnInitialUpdate();
    DECLARE_MESSAGE_MAP()
};