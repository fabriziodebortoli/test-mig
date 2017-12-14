#pragma once

#include <TbGenlib\ParsObj.h>
#include <TbGenlib\ParsObjManaged.h>

#include "ExtDocView.h"

#include "beginh.dex"
//=============================================================================

class TB_EXPORT CSlaveViewContainer : public CAbstractFormView, public ResizableCtrl
{
	DECLARE_DYNAMIC(CSlaveViewContainer)

	DECLARE_MESSAGE_MAP()
public:
	CSlaveViewContainer (UINT nIDC, CWnd*pParent, CBaseDocument* pDocument);
	
	virtual ~CSlaveViewContainer ();

	void			CalcSlaveViewSize	();
	void			HideSlaveView		();
	void			DeleteSlaveView		(CWnd* pCurrent);

	void			SetFocusOnActiveSlaveView ();
	CView*			GetFirstSlaveView		();

	LRESULT			OnRecalcCtrlSize	(WPARAM, LPARAM);

	virtual	void	DrawItem			(LPDRAWITEMSTRUCT) {}
	
	//CAbstractFormDoc*	GetDocument	() const { return (CAbstractFormDoc*) CView::m_pDocument; }
	void BuildDataControlLinks(void){}

protected:
	//{{AFX_MSG(CSlaveViewContainer)
	afx_msg LRESULT OnGetControlDescription(WPARAM, LPARAM);
	////}}AFX_MSG
public:
	afx_msg BOOL OnEraseBkgnd(CDC* pDC);

	virtual BOOL PreCreateWindow(CREATESTRUCT& cs);
};

///////////////////////////////////////////////////////////////////////////////
//					CSlaveViewTree
///////////////////////////////////////////////////////////////////////////////

//-----------------------------------------------------------------------------
class TB_EXPORT CSlaveViewTree : public CTreeViewAdvCtrl
{
	DECLARE_DYNCREATE(CSlaveViewTree)

protected:
	int						m_nWidth;
	BOOL					m_bCanHide;
	BOOL					m_bLoadingNodes;
	BOOL					m_bSelectFirstVisibleNode;
	BOOL					m_bDeleteSlaveView;

	CSlaveViewContainer*	m_pSlaveContainer;

	CString					m_sKeyFirstNode;
	CString					m_sKeyCurNode;

public:
	Array					m_AllNodes;
	CObArray				m_Nodes;

public:
	class TB_EXPORT CNodeInfo: public CObject 
	{
	public:
		BOOL			m_bVisible;
		CString			m_sParentKey;
		CString			m_sTitle;
		CString			m_sKey;

		CRuntimeClass*	m_pClassView;

		CSlaveFormView* m_pView;

		CString			m_sImage;	// immagine associata al nodo
		int				m_nMenuType;// tipo di menu contestuale (0 = nessun menu contestuale)

		BOOL			m_bShowChilds;
		BOOL			m_bIsBookmarkToParent;
		BOOL			m_nBookmark;

		CNodeInfo
			(
				BOOL			bVisible,
				LPCTSTR			sParentKey,
				LPCTSTR			sTitle,
				LPCTSTR			sKey,
				CRuntimeClass*	pClassView = NULL,
				CString			sImage = _T(""),
				int				nMenuType = 0
			)
			:
			m_bVisible		(bVisible),
			m_sParentKey	(sParentKey),    
			m_sTitle		(sTitle), 
			m_sKey			(sKey), 
			m_pClassView	(pClassView), 
			m_pView			(NULL), 
			m_sImage		(sImage),
			m_nMenuType		(nMenuType),
			m_bShowChilds	(FALSE),
			m_bIsBookmarkToParent	(FALSE),
			m_nBookmark				(0)
		{}
	};

public:
	CSlaveViewTree()
		:
		m_pSlaveContainer			(NULL),
		m_bLoadingNodes				(FALSE),
		m_bSelectFirstVisibleNode	(FALSE),
		m_bDeleteSlaveView			(FALSE),
		m_bCanHide(TRUE),
		m_nWidth(0)
	{
	}

public:
	virtual void	LoadAllNodes() {}

protected:
	virtual void	AddImages	() {}
	virtual CString GetNodeImage(CNodeInfo* pNode);
		
	virtual BOOL OnBeforeSelectionChanged	(const CString& sKeyCurNode) { return TRUE; }
	virtual BOOL CanDoSelectionChanged		(const CString& sKey)  { return TRUE; }
	virtual void SelectionChangedFailed		(const CString& sKey) {}
	virtual void OnPrepareSelectionChanged	(const CString& sKey) {}

public:
	void LoadNodes();
	void OnInitControl ();
	CSlaveViewContainer* AttachContainer (UINT nIDCPlaceHolder, CWnd* pParentWnd = NULL);
	CSlaveViewContainer* GetContainer() { ASSERT(m_pSlaveContainer); return m_pSlaveContainer; }
	
	BOOL CreateSlaveDialog(CNodeInfo*);
	BOOL CallDialog(CNodeInfo*);
	BOOL CallChildDialog(CNodeInfo*);

	void SelectFirstNode();
	
	int	 GetNodesUpperBound()										{ return m_Nodes.GetUpperBound(); }
	CSlaveViewTree::CNodeInfo*	GetNodeElem(const CString& sKey);
	CSlaveViewTree::CNodeInfo*	GetNodeElem(int i)					{ return (CNodeInfo*)m_Nodes.GetAt(i); }
	int	 GetAllNodesUpperBound()									{ return m_AllNodes.GetUpperBound(); }
	CSlaveViewTree::CNodeInfo*	GetAllNodeElem(const CString& sKey);
	CSlaveViewTree::CNodeInfo*	GetAllNodeElem(int i)				{ return (CNodeInfo*)m_AllNodes.GetAt(i); }

	void SetFirstNode(LPCTSTR sKey) { m_sKeyFirstNode = sKey; }

	virtual void OnSelectionChanged();

	void RemoveContextMenu();
	void SetCanHide(BOOL bSet){ m_bCanHide = bSet; }
	virtual void OnSizeChanged();

	void SetNodeImage (CString& sKey, CString& sIcon);
};

//=============================================================================
#include "endh.dex"
