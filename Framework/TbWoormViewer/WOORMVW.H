
#pragma once

#include "afxpriv.h"

//includere alla fine degli include del .H
#include "beginh.dex"

#include "BASEOBJ.H"
//===========================================================================
class CWoormDocMng;
class CWoormFrame;
class CriticalArea;
class TFXDataTip;
class CWatermark;

enum PrintType { PT_NORMAL, PT_EMF, PT_BMP, PT_VMT };

//#define DRAGDROP_WOORM_CRSTREECTRL			_T("CRSTreeCtrl")
#define DRAGDROP_WOORM_ENGINE_VIEW			_T("WOORM_ENGINE_VIEW")
#define DRAGDROP_WOORM_LAYOUT_VIEW			_T("WOORM_LAYOUT_VIEW")
#define DRAGDROP_WOORM_OBJECTS_VIEW			_T("WOORM_OBJECTS_VIEW")
#define DRAGDROP_WOORM_DB_VIEW				_T("WOORM_DB_VIEW")

/////////////////////////////////////////////////////////////////////////////
// CWatermark
// Si occupa di scrivere nello sfondo i messaggi di warning sull'attivazione
//-----------------------------------------------------------------------------
class TB_EXPORT CWatermark
{
private:
	CString m_sText;
	CFont*	m_pFont;
	int		m_nFontAngle;
	int		m_nFontSize;
	CRect	m_rectPage;
public:
	BOOL	m_bCreated;

public:
	CWatermark () 
		:
		m_pFont					(NULL),
		m_nFontAngle			(450),
		m_nFontSize				(70),
		m_bCreated				(FALSE)
		{ }

	~CWatermark () 
		{ 
			Clear();
		}

	void Clear ();

	CFont* GetFont() 
		{  
			ASSERT(m_pFont);
			return m_pFont;
		}

	void Draw (CDC* pDC, const CRect& rect);

protected:
	BOOL Create (CDC* pDC, const CRect& rect);
	void SetText();
};

//===========================================================================
// Derive the view from CBCGPScrollView to add scrolling functionality.
//===========================================================================
class CNodeTreeArray;
class Table;
class CRSTreeCtrl;
class CNodeTree;
class TblRuleData;

class TB_EXPORT CWoormView : public CBCGPScrollView
{
	DECLARE_DYNCREATE(CWoormView)

	enum ScalingType {ONLY_VERTICAL, ONLY_HORIZONTAL, HORIZONTAL_MORETHAN_VERTICAL, VERTICAL_MORETHAN_HORIZONTAL, NONE};

protected:
	BOOL	m_bSensibility;
	BOOL	m_bSupportCollate;
	BOOL	m_bSupportCopies;
	int		m_nCurrentCopy;
	UINT	m_nFromPage;
	UINT	m_nToPage; 
	int		m_nScaleDenom;
	int		m_nScaleNum;
	BOOL	m_bDoScale;
	BOOL	m_bPDFPrinting;
	BOOL	m_bCalculatedScale;
	CPoint  m_ptOffsetOrgAfterScaling;
	double  m_dLowerScale;
	double  m_dScaleFactor;
	ScalingType	m_scaleType;
	TFXDataTip*				m_pDataTip;

	COleDropTarget			m_DropTarget;
	CPoint					m_PointDrop;

	NewActiveRect*			m_pTransActiveRect		= NULL;	//E' il default, alternativo al precedente CWoormDoc::m_pActiveRect

	Table*					m_pTransStartTable		= NULL;
	int						m_nTransStartColAlias	= -1;
	int 					m_nTransDestColAlias	= -1;

protected:
	CWoormView();

// Attributes
public:
	CWoormDocMng*	GetDocument	();
	BOOL		Disabled (BOOL bCheckEditMode = TRUE);

	CSize		GetPageSizeWithoutMargin_LP	();
	CSize		GetPrinterPageSizeWithoutMargin_LP	();
	CSize		GetPrinterFullPageSize_mm ();

	CSize		GetScrollSize ();

	int			GetCurrentCopy () const { return m_nCurrentCopy; }

	CWoormFrame* GetWoormFrame() const { return (CWoormFrame*) __super::GetParentFrame(); }
// Operations
public:
	CriticalArea*	m_pProcessingMouse;
	CWatermark*	m_pWatermark;
	
// Implementation
public:
	virtual ~CWoormView();
	
	virtual void OnInitialUpdate	();
	virtual void OnUpdate			(CView* pSender, LPARAM lHint = 0L, CObject* pHint = NULL);
	virtual void OnDraw				(CDC* pDC);
	virtual void OnPrepareDC		(CDC* pDC, CPrintInfo* pInfo = NULL);
	virtual	void DoDraw				(CDC* pDC, CPrintInfo* pInfo = NULL);
	
public:
	BOOL DrawBitmap				(CDC* pDC, CPoint ptOrigin, const CString& strBmpName);
	void DrawMargins			(CDC* pDC, CPrintInfo* pInfo);
	BOOL DoPreparePrinting		(CPrintInfo* pInfo, BOOL bPrintDialog = TRUE);
	BOOL MFCCloneDoPreparePrinting(CPrintInfo* pInfo, BOOL bPrintDialog = TRUE); //reimplemented to avoid call of MFC PrintDialog (which change application printer)
	void GetPrinterCapability	(CPrintInfo* pInfo, BOOL bFromPrintDialog = FALSE);

	// Printing support
protected:
			void CalculateScaleFactor (CPrintInfo* pInfo);
	virtual BOOL OnPreparePrinting	(CPrintInfo* pInfo);
	virtual void OnBeginPrinting	(CDC* pDC, CPrintInfo* pInfo);
	virtual void OnEndPrinting		(CDC* pDC, CPrintInfo* pInfo);
	virtual void OnPrint			(CDC* pDC, CPrintInfo* pInfo);
	virtual void OnLinkSelected		();
			void GetObjDescription		();

private:
	enum PrintJobStatus { NO_PRINT_ERROR, PRINT_ABORT, FAILED_TO_START_PRINT };
	PrintJobStatus m_PrintStatus;
	void OnPDFPrint			(UINT nStartPage, UINT nEndPage, UINT nCopies, LPCTSTR pszFileName, LPCTSTR pszPassword, CString sPrinterTemplate);

	//----
	void GetDropText(CString& str, COleDataObject* pDataObject);
	BOOL IsDropText(COleDataObject* pDataObject);
	BOOL ReadDropData(COleDataObject* pDataObject);

	BOOL		UpdateNodeSelection	(CRSTreeCtrl* sourceTree);
	Table*		GetTableAtDropPos	(CClientDC& dc);
	BaseObj*	GetTargetDropObject	(CClientDC& dc);
	Table*		GetTableAtPoint		(CPoint pt, BOOL bSetActive = FALSE);

	BOOL AddHiddenVariableFromDrop(CNodeTreeArray*& pAr, BOOL isNewTable, TblRuleData* pTblRule);
	BOOL AddDBColumnsFromDrop(BOOL bFromTree = FALSE); 
	BOOL AddDBTableFromDrop();

public:
	void OnSilentFilePrint	(LPCTSTR pstrFileName = NULL, PrintType type = PT_NORMAL);
	BOOL OnPDFPrint			(UINT nStartPage, UINT nEndPage, UINT nCopies, LPCTSTR pszFileName);
	BOOL OnPDFPrint			(LPCTSTR pszFileName);

	BOOL OnObjectsSelChanged (CObject*);	

	// Drag & Drop
	virtual DROPEFFECT	OnDragEnter(COleDataObject* pDataObject, DWORD dwKeyState, CPoint point);
	virtual DROPEFFECT	OnDragOver(COleDataObject* pDataObject, DWORD dwKeyState, CPoint point);
	virtual BOOL		OnDrop(COleDataObject* pDataObject, DROPEFFECT dropEffect, CPoint point);
	virtual void		OnDragLeave();
	void				ManageDragToTransform(CPoint point);
	void				CancelDragToTransform();//reset
	void				TransformFieldRectInColumn();


	
protected:
	//{{AFX_MSG(CWoormView)
	afx_msg	void OnTimer (UINT nIDEvent);
	afx_msg void OnLButtonDown(UINT nFlags, CPoint point);
	afx_msg void OnLButtonUp(UINT nFlags, CPoint point);
	afx_msg void OnMouseMove(UINT nFlags, CPoint point);
	afx_msg void OnRButtonDown(UINT nFlags, CPoint point);
	afx_msg BOOL OnSetCursor(CWnd* pWnd, UINT nHitTest, UINT message);
	afx_msg void OnAccelerateRMouseDown ();
	afx_msg void OnUpdateAccelerateRMouseDown (CCmdUI*);
	afx_msg	void OnUpdateFilePrint(CCmdUI* pCmdUI);
	afx_msg BOOL OnEraseBkgnd(CDC* pDC);
	afx_msg void OnFilePrint();
	afx_msg LRESULT OnGetReportNamespace(WPARAM, LPARAM);
	LRESULT OnGetLocalizerInfo(WPARAM wParam, LPARAM lParam);
	
	afx_msg void OnScrollUp		();
	afx_msg void OnScrollDown	();
	afx_msg void OnScrollLeft	();
	afx_msg void OnScrollRight	();
	afx_msg void OnScrollTop	();
	afx_msg void OnScrollBotton	();
	afx_msg void OnNcDestroy	();

	afx_msg void OnComboObjectsSelChanged	();

	afx_msg void OnUpdateCut(CCmdUI* pCmdUI);
	afx_msg	void OnObjectCut();

	afx_msg	void OnVKLeft();
	afx_msg	void OnVKRight();
	afx_msg	void OnVKUp();
	afx_msg	void OnVKDown();
	afx_msg	void OnVKTab();
	//}}AFX_MSG

	BOOL OnToolTipText(UINT id, NMHDR* pNMHDR, LRESULT* pResult);

public:
	afx_msg void OnUpdateVKMove(CCmdUI* pCmdUI);
	BOOL IsInTransfertMode() { return m_pTransStartTable != NULL; }

	DECLARE_MESSAGE_MAP()

#ifdef _DEBUG
	virtual void AssertValid() const;
	virtual void Dump(CDumpContext& dc) const;
#endif
};

/////////////////////////////////////////////////////////////////////////////
// CWoormPreviewView

class TB_EXPORT CWoormPreviewView : public CBCGPPrintPreviewView
{
	DECLARE_DYNCREATE(CWoormPreviewView)

public:
	CWoormPreviewView ();
	~CWoormPreviewView ();

protected:
	CWoormDocMng* GetDocument() { return (CWoormDocMng*) m_pDocument; }

protected:

	afx_msg int OnCreate(LPCREATESTRUCT lpCreateStruct);
	afx_msg void OnPreviewClose();
	afx_msg void OnNextPage();
	afx_msg void OnPrevPage();
	afx_msg void OnUpdateNextPage(CCmdUI* pCmdUI);
	afx_msg void OnUpdatePrevPage(CCmdUI* pCmdUI);
	afx_msg void OnUpdateNumPageChange(CCmdUI* pCmdUI);

	DECLARE_MESSAGE_MAP()
};

//===========================================================================
#ifndef _DEBUG	// debug version in woormvw.cpp
inline CWoormDocMng* CWoormView::GetDocument()
   { return (CWoormDocMng*) m_pDocument; }
#endif



#include "endh.dex"

