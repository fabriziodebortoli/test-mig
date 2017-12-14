#pragma once
#include <TbNameSolver\CallbackHandler.h>
#include <TbGeneric\DataObj.h>
#include <TbGenlib\PARSOBJ.H>
#include <TbGenlib\ParsObjManaged.h>

#include "beginh.dex"

class DBTSlaveBuffered;
class DBTSlave;
class DBTSlaveBuffered;
class DBTMaster;
class DBTObject;
class SqlRecord;
class CSlaveViewContainer;
class CNodeLevelInfo;
class CBodyEdit;
class CTreeBag;

#define CMD_NEW				1
#define CMD_DELETE			2
#define CMD_TOGGLE_EXPAND	3

//===========================================================================
class CNodeLevelInfoArray : public CArray<CNodeLevelInfo*, CNodeLevelInfo*> 
{
	static int Compare(const void *, const void *);

public:
	void Sort() { qsort(GetData(), GetSize(), sizeof(CNodeLevelInfo*), Compare); };
};

//===========================================================================
class TB_EXPORT CDBTTreeEdit : public CTreeViewAdvCtrl
{
	friend class CBodyEdit;

	DECLARE_DYNCREATE(CDBTTreeEdit)
	DECLARE_MESSAGE_MAP()

	CMap<CString, LPCTSTR, CTreeBag*, CTreeBag*> m_ObjectMap;
	CMap<CObject*, CObject*, CString, LPCTSTR> m_KeyMap;
	CMap<UINT, UINT, CSlaveViewContainer*, CSlaveViewContainer*> m_RowViewContainerMap;

	DBTSlaveBuffered*	m_pDBT; 
	CNodeLevelInfoArray m_NodeInfoMap;

	int		m_nWidth;
	BOOL	m_bSelecting;
	BOOL	m_bCreatingStructure;
	BOOL	m_bUpdatingContent;
	BOOL	m_bUpdatingObjectMap;
	CString m_sSelectedNode;
	UINT	m_nTreeStructureHash;
	UINT	m_nTempTreeStructureHash;
	UINT	m_nTreeContentHash;
	UINT	m_nTempTreeContentHash;
	UINT	m_nTreeObjectsHash;
	UINT	m_nTempTreeObjectsHash;
	CWnd*	m_pActiveWnd;
	BOOL	m_bInited;
	int		m_nKeyCounter;
	CString m_sDocumentKey;
	BOOL	m_bExpanded;
	int		m_nInitialSelectionLevel;
	int		m_nInitialExpansionLevel;
	BOOL	m_bCanHide;
	BOOL	m_bCanResize;

protected: 
	virtual void	OnSelectionChanged	();
	virtual void	UpdateCtrlView		();
	virtual	void	UpdateCtrlStatus	();
	virtual	BOOL	ForceUpdateCtrlView	(int = -1);
	virtual	BOOL	IsDataModified		();
	virtual CString	GetDBTImage			(DBTSlaveBuffered* pDBTObject);
	virtual CString	GetRecordImage		(DBTSlaveBuffered* pDBTObject, SqlRecord* pRecord);
	virtual	void	OnLoad				() {}

public:
	CDBTTreeEdit();
	~CDBTTreeEdit(void);

	DBTSlaveBuffered*	GetDBT() const	{ return m_pDBT; }
	
	void	Attach		(DBTSlaveBuffered* pDBT);
	void	RefreshTree	(BOOL bUpdateObjectMap, BOOL bUpdateTreeContent, BOOL bUpdateTreeStructure);
	
	CBodyEdit* AddView	(DBTSlaveBuffered* pDBTObject, UINT nBodyIDC, UINT nViewIDC, CRuntimeClass* pViewClass);
	
	CBodyEdit* AddBody	
		(
		DBTSlaveBuffered*	pDBTObject,
		UINT				nBodyIDC, 
		UINT				nViewIDC,
		CRuntimeClass*		pBodyClass, 
		CRuntimeClass*		pViewClass = NULL, 
		CString				strRowFormViewTitle =_T(""),
		CString				sBodyName = _T(""),
		CString				sRowViewName = _T("")
		);

	void SetNodeImages			(DBTSlaveBuffered* pDBTObject, const CString& sDBTNodeImage, const CString& sRecordNodeImage);
	//per evitare al gestionale di aggiungere un reference a TBGenlibManaged
	void AddImage				(const CString& sImageKey, const CString& sImagePath){ __super::AddImage(sImageKey, sImagePath);}
	void GetSelectedNodeObjects	(DBTObject*& pDBT, SqlRecord*& pRecord);
	void ToggleExpansion		();
	void SelectNode				(DBTSlaveBuffered* pDBT, SqlRecord* pRecord);
	void SetCanHide				(BOOL bSet)	{ m_bCanHide = bSet; }
	void SetCanResize			(BOOL bSet)	{ m_bCanResize = bSet; }
	
	void SetInitialNodeSelectionLevel	(int nLevel)	{ m_nInitialSelectionLevel = nLevel; }
	void SetInitialNodeExpansionLevel	(int nLevel)	{ m_nInitialExpansionLevel = nLevel; }
	
	virtual void OnSizeChanged			();

protected:
	virtual	void	SetDataReadOnly		(BOOL bRO);
	virtual void	OnToolBarCommand	(int cmdId);
	virtual void	OnActivateRecord	(BOOL bActivate, DBTSlaveBuffered* pDBT, SqlRecord* pRecord){}
	virtual void	OnAfterAddRecord	(const CString& sKey, SqlRecord* pRecord) {}

private:
	void			AddToHash			(const CString& str, const CString& sKey, int nAdditionalInfo, void* pObject);
	CString			GetNewKey			(CObject* pObj);
	CString			AddRecord			(const CString& sParentNode, DBTSlaveBuffered* pDBT, SqlRecord* pRec, CNodeLevelInfo* pInfo);
	void			AddDBT				(const CString& sParentNode, DBTSlaveBuffered* pDBT);
	void			ClearObjectMap		();
	void			ActivateDBTView		(DBTSlaveBuffered* pDBT, SqlRecord* pRecord);
	CBodyEdit*		GetBodyEdit			(DBTSlaveBuffered* pBuff);
	CNodeLevelInfo* GetNodeLevelInfo	(DBTSlaveBuffered* pDBT);
	CTreeBag*		GetObjectFromMap	(const CString& sKey);
	CString			GetKeyFromMap		(DBTSlaveBuffered* pDBT, SqlRecord* pRec);

	void			AddToMap			(const CString& sKey, DBTSlaveBuffered* pDBT, SqlRecord* pRec);
	void			Activate			(DBTSlaveBuffered* pDBT);
	void			Activate			(DBTSlaveBuffered* pDBT, SqlRecord* pRecord);
	void			DelayedInit			();
	void			Select				(const CString& sKey);
	
	CSlaveViewContainer* CreateSlaveViewContainer(CWnd* pParent, UINT nIDC);
};

#include "endh.dex"
