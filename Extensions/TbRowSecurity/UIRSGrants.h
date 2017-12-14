#pragma once 


#include <TBGes\TileManager.h>
#include <TBGes\TileDialog.h>
#include <TbGes\ExtDocView.h>
#include <TbGes\ExtDocFrame.h>
#include <TbGeneric\DataObj.h>

//includere alla fine degli include del .H
#include "beginh.dex"

class RSEntityInfo;
class CEntityGrant;
class DBTEntitySubjectsGrants;
class CSubjectCache;
class TRS_SubjectsGrants;
class CRSGrantsClientDoc;


//=============================================================================
class CEntityGrantsTreeNode : public CObject
{
	DECLARE_DYNCREATE(CEntityGrantsTreeNode)

public:
	CString m_strKey;
	CSubjectCache* m_pSubject;
	TRS_SubjectsGrants* m_pSubjectsGrantsRec;
	CString m_strParentKey;


public:
	CEntityGrantsTreeNode();
	CEntityGrantsTreeNode(CSubjectCache*pSubjectcache, TRS_SubjectsGrants* pSubjectGrants, const CString& strParentKey);
};


//=============================================================================
class CEntityGrantsTreeViewAdv : public CTreeViewAdvCtrl
{
	DECLARE_DYNCREATE(CEntityGrantsTreeViewAdv)

private:
	Array*	m_pAllNodes;
	BOOL	m_bTreeViewLoaded;
	BOOL	m_bIsProtected;
	CString m_strSelectedNodeKey;
	

public:
	CEntityGrantsTreeViewAdv();
	~CEntityGrantsTreeViewAdv();


public:
	void SetAllNodes(Array* pAllNode, BOOL isProtected);
	
	void Load();
	CString GetSubjectTreeImage(CEntityGrantsTreeNode* pNode);
	CEntityGrantsTreeNode* GetSelectedTreeNode();
	CEntityGrantsTreeNode* GetNodeByKey(const CString& strKey);
	void InsertNodeInTree(CEntityGrantsTreeNode* pNode);
	void ChangeNodeImage(CEntityGrantsTreeNode* pNode);
	
	virtual void Enable	(const BOOL bValue = TRUE) { CTreeViewAdvCtrl::Enable(bValue); }

protected: 	
	virtual void OnInitControl();
	virtual void OnToolBarCommand(int cmdId);
	virtual void OnSelectionChanged();
};


//=============================================================================
class TB_EXPORT CGrantsTileGroup : public CTileGroup
{
	DECLARE_DYNCREATE(CGrantsTileGroup)

protected:
	virtual void Customize();
};

//=============================================================================
class CTileGrantsTree : public CTileDialog
{
	DECLARE_DYNCREATE(CTileGrantsTree)
	
private:
	CEntityGrantsTreeViewAdv*	m_pTreeView;
	BOOL						m_bTreeViewLoaded;
	
public:
	// Construction
	CTileGrantsTree();


private:
	CRSGrantsClientDoc* GetRSGrantsClientDoc() const; 
	void AddTreeNode(Array* pAllNode, CSubjectCache* pSubjectCache, const CString& strParentKey);

protected:
	virtual void BuildDataControlLinks();
	virtual	BOOL OnPrepareAuxData();

protected:		
//{{AFX_MSG(CTileGrantsTree)	
	afx_msg void OnSelectionNodeChanged	();
	afx_msg void OnLoadGrantsTree		();
	afx_msg void OnRefreshCurrentNode	();
//}}AFX_MSG
DECLARE_MESSAGE_MAP()
};

//=============================================================================
class CTileSingleGrant : public CTileDialog
{
	DECLARE_DYNCREATE(CTileSingleGrant)

public:
	// Construction
	CTileSingleGrant();


private:
	CRSGrantsClientDoc* GetRSGrantsClientDoc() const;

protected:
	virtual void BuildDataControlLinks();
	
protected:
	//{{AFX_MSG(CTileSingleGrant)	
	afx_msg void OnGrantTypeChanged();
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};


//=============================================================================
class CRSEntityGrantsView : public CSlaveFormView
{
	DECLARE_DYNCREATE(CRSEntityGrantsView)

protected:	
	// Construction
	CRSEntityGrantsView();

public:		
	virtual	void BuildDataControlLinks();

};


//=============================================================================
// CRSGrantsFrame
//=============================================================================
class CRSEntityGrantsFrame : public CSlaveFrame
{
	DECLARE_DYNCREATE(CRSEntityGrantsFrame)

protected:
	CRSEntityGrantsFrame();

public:
	BOOL OnCustomizeTabbedToolBar(CTBTabbedToolbar* pTabbedBar);
};


#include "endh.dex"