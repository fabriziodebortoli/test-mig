#pragma once


#include <TbGeneric\JsonFormEngine.h>
#include <TbGenlib\PARSOBJ.H>
#include <TbGenlib\TBPropertyGrid.h>
#include "extdocview.h"
#include "Tabber.h"
#include "TileManager.h"
#include "TileDialog.h"
#include "BodyEditTree.h"
#include "UnpinnedTilesPane.h"
#include "HeaderStrip.h"

#include "beginh.dex"

static const TCHAR szNsInstanceSeparator = _T('_');

class HotFilterObj;

typedef CMap<CString, LPCTSTR, CWndObjDescription*, CWndObjDescription*> DescriptionMap;
class CJsonContext : public CJsonContextObj
{
protected:
	CJsonContext(){}
public:
	ControlLinks* m_pLinks = NULL;
	CAbstractFormDoc* m_pDoc = NULL;
	CParsedForm* m_pForm = NULL;
	CAbstractFormView* m_pView = NULL;
	//-----------------------------------------------------------------------------
	static CJsonContext* Create() { return new CJsonContext; }
	//-----------------------------------------------------------------------------
	virtual void Associate(CWnd* pWnd);
	//-----------------------------------------------------------------------------
	virtual void Assign(CJsonContextObj* pOther);
	virtual CRuntimeClass* GetControlClass(CString sControlClass, CWndObjDescription::WndObjType m_Type, DataObj* pDataObj, DWORD& dwNeededStyle, DWORD& dwNeededExStyle);
	bool CanCreateControl(CWndObjDescription* pWndDesc, UINT nID);
	template <class T, class TDataObj>
	bool EvaluateExpression(const CString& sText, CWndObjDescription* pDescri, T& bOut);
	bool Evaluate(CJsonExpressions& expressions, CWndObjDescription* pDescri);
	void BuildDataControlLinks();
	BOOL TranslateAliasDefaults(CString& sAlias, CWndObjDescription::WndObjType controlType);
	void OnParsedControlCreated(CParsedCtrl* pCtrl);
	void OnColumnInfoCreated(ColumnInfo* pColInfo);
	void OnPropertyCreated(CTBProperty* pProperty);
	BOOL OnGetToolTipProperties(CBETooltipProperties* pTooltip);
	void EnableBodyEditButtons(CBodyEdit* pBodyEdit);
	bool CheckActivation(CString singleActivation);
	bool CheckActivationExpression(const CString& activationExpression);
	void GetActivationExpressions(CStringArray& arIds, CArray<bool>& arActivated);
	CString AdjustExpression(const CString& sRawExpression);
	void AttachItemSource(CItemSourceDescription* pItemSource, CParsedCtrl* pParsedCtrl);
	void AttachDataAdapter(CDataAdapterDescription* pDataAdapterDescription, CParsedCtrl* pParsedCtrl);
	void AttachValidator(CValidatorDescription* pValidator, CParsedCtrl* pParsedCtrl);
	void AttachControlBehaviour(CControlBehaviourDescription* pControlBehaviour, CParsedCtrl* pParsedCtrl);
	HotFilterObj* CreateHotFilter(UINT nHFID, CWndObjDescription* pDesc);

	BOOL CreateSplitter(CSplitterDescription* pSplitterDesc, CSplittedForm* pSplitterForm, CBaseDocument* pDoc);
	CAbstractFormView* GetOwnerView();
};
class CHotFilterJsonContext : public CJsonContext {
private:
	CString m_sCommnControlClass;
public:
	CHotFilterJsonContext(const CString& sCommonControlClass) : m_sCommnControlClass(sCommonControlClass) {}
	CRuntimeClass* GetControlClass(CString sControlClass, CWndObjDescription::WndObjType m_Type, DataObj* pDataObj, DWORD& dwNeededStyle, DWORD& dwNeededExStyle);

};
class CTBJsonData
{
protected:
	CWndObjDescription* m_pWndDesc = NULL;
	CJsonContext* m_pContext = NULL;
public:
	void SetDescription(CJsonContext* pContext, CWndObjDescription* pWndDesc)
	{
		m_pWndDesc = pWndDesc;
		m_pContext = pContext;
	}

};
//-----------------------------------------------------------------------------
class TB_EXPORT CJsonTileDialog : public CTileDialog
{
	DECLARE_DYNCREATE(CJsonTileDialog)
public:
	CJsonTileDialog() : CTileDialog(_T(""), 0){}
	CJsonTileDialog(UINT nIDD);
	void AssignContext(CJsonContextObj* pContext);
	TileDialogSize GetSize();
	int GetFlex();
	bool IsInitiallyPinned();
	
	BOOL OnInitDialog();
	
	virtual	BOOL	UseSplitters();
	virtual void	OnCreateSplitters(CCreateContext* pContext);

protected:
	BOOL OnCmdMsg(UINT nID, int nCode, void* pExtra, AFX_CMDHANDLERINFO* pHandlerInfo);
private:
	void InitializeFromContext();
};

template<class T>
class TB_EXPORT TBJsonTileGroupWrapper : public T, public CTBJsonData
{
	bool m_bOwnContext = false;

public:
	TBJsonTileGroupWrapper() {}
	~TBJsonTileGroupWrapper();

public:
	virtual	void				Customize();
	virtual BOOL				OnPrepareAuxData();
private:
	void Customize(CWndObjDescriptionContainer& children, CLayoutContainer *pContainer, CTilePanel* pContainerPanel);

};

//---------------------------------------------------------------------------------
class TB_EXPORT CJsonTileGroup : public TBJsonTileGroupWrapper<CTileGroup>
{
private:
	DECLARE_DYNCREATE(CJsonTileGroup);
};

//---------------------------------------------------------------------------------
class TB_EXPORT CJsonPinnedTilesTileGroup : public TBJsonTileGroupWrapper<CPinnedTilesTileGroup>
{
private:
	DECLARE_DYNCREATE(CJsonPinnedTilesTileGroup)
};


//---------------------------------------------------------------------------------
class TB_EXPORT CJsonHeaderStrip : public CHeaderStrip, public CTBJsonData
{
	DECLARE_DYNCREATE(CJsonHeaderStrip);

	virtual	void		Customize();

private:
	void Customize(CWndObjDescriptionContainer& children, CLayoutContainer *pContainer, CTilePanel* pContainerPanel);
	~CJsonHeaderStrip();

};
//-----------------------------------------------------------------------------
class TB_EXPORT CJsonPropertyGrid : public CTBPropertyGrid, public CTBJsonData
{
private:
	DECLARE_DYNCREATE(CJsonPropertyGrid);
	void Customize(CWndObjDescriptionContainer& children, CTBProperty* pGroup);
protected:
	void OnCustomize();
};
//-----------------------------------------------------------------------------
class TB_EXPORT CJsonTileManager : public CTileManager, public CTBJsonData
{
private:
	DECLARE_DYNCREATE(CJsonTileManager);
public:
	virtual BOOL CreateEx(DWORD dwExStyle, DWORD dwStyle, const RECT& rect, CWnd* pParentWnd, UINT nID);
	virtual	void Customize();
};

#pragma warning(push)
#pragma warning (disable : 4661)	// C++ Exception Specification ignored
//-----------------------------------------------------------------------------
template<class T>
class TJsonTabManager : public T, public CTBJsonData
{
public:
	virtual BOOL CreateEx(DWORD dwExStyle, DWORD dwStyle, const RECT& rect, CWnd* pParentWnd, UINT nID);
	virtual	void Customize();
	virtual DlgInfoItem* AddDialog(UINT nIDD) = 0;
};
//-----------------------------------------------------------------------------
template<class T>
class TJsonTabDialog : public T
{
public:
	TJsonTabDialog() : T(_T(""), IDD_EMPTY_TAB){}
	virtual BOOL Create(CBaseTabManager* pParentWnd);
};
#pragma warning(pop)
//-----------------------------------------------------------------------------
class TB_EXPORT CJsonTabManager : public TJsonTabManager<CTabManager>
{
	DECLARE_DYNCREATE(CJsonTabManager);
	virtual DlgInfoItem* AddDialog(UINT nIDD);
};

//-----------------------------------------------------------------------------
class TB_EXPORT CJsonTabDialog : public TJsonTabDialog<CTabDialog>
{
	DECLARE_DYNCREATE(CJsonTabDialog);
};

//-----------------------------------------------------------------------------
class TB_EXPORT CJsonWizardTabManager : public TJsonTabManager<CTabWizard>
{
	DECLARE_DYNCREATE(CJsonWizardTabManager);
	virtual DlgInfoItem* AddDialog(UINT nIDD);
};

//-----------------------------------------------------------------------------
class TB_EXPORT CJsonWizardTabDialog : public TJsonTabDialog<CWizardTabDialog>
{
	DECLARE_DYNCREATE(CJsonWizardTabDialog);
};
//-----------------------------------------------------------------------------
class TB_EXPORT IJsonBodyEditWrapper
{
public:
	virtual void SetAllBodyEditStyles() = 0;
	virtual CRuntimeClass*		GetRowViewClass() = 0;
};


//-----------------------------------------------------------------------------
template<class T>
class TB_EXPORT TBJsonBodyEditWrapper : public T, public CTBJsonData, public IJsonBodyEditWrapper
{

protected:
	virtual void				FindHotLink();
public:
	virtual void				OnBeforeCustomize();
	virtual	void				Customize();
	virtual BOOL				OnCreateClient();
	virtual CRuntimeClass*		GetRowViewClass();
	virtual UINT				GetRowFormViewId();
	void						SetAllBodyEditStyles();
	void						FillMissingColumnProps(CWndObjDescription* pRowViewDesc, CWndBodyColumnDescription* pColDesc);
	virtual BOOL				OnGetToolTipProperties(CBETooltipProperties* pTp);
	virtual void				EnableButtons();
	virtual BOOL				CanDoDeleteRow();
};

//-----------------------------------------------------------------------------
class TB_EXPORT CJsonBodyEdit : public TBJsonBodyEditWrapper<CBodyEdit>
{
private:
	DECLARE_DYNCREATE(CJsonBodyEdit);
};

//-----------------------------------------------------------------------------
class TB_EXPORT CJsonTreeBodyEdit : public TBJsonBodyEditWrapper<CTreeBodyEdit>
{
private:
	DECLARE_DYNCREATE(CJsonTreeBodyEdit);
};

//=============================================================================
class TB_EXPORT CJsonRowView : public CRowFormView
{
	DECLARE_DYNCREATE(CJsonRowView)

public:
	CJsonRowView(UINT nResourceId = 0);
public:
	virtual	void BuildDataControlLinks();
};
//-----------------------------------------------------------------------------
class CJsonFormEngine : public CJsonFormEngineObj
{
private:
	DECLARE_LOCKABLE(DescriptionMap, m_Descriptions);//cache delle descrizioni
	CString m_sCachePath;
	friend class CJsonContext;

	void ClearDescriptions();
	void InitCachePath();
protected:
	virtual void ClearCache();
	virtual BOOL ProcessWndDescription(CWndObjDescription* pWndDesc, CWnd* pParentWnd, CJsonContextObj* pContext);
	virtual CJsonContextObj* CreateContext(const CJsonResource& sJsonResource, bool bCacheDescriptions = true, bool bIsJsonEditor = false);
	virtual CJsonContextObj* CreateContext(bool bIsJsonEditor = false);
	virtual void MergeContext(const CJsonResource& sJsonResource, CJsonContextObj* pContext);
	virtual void GetDeltaJsonFormInfos(const CString& sJsonId, CArray<CJsonResource>& sources);
	virtual BOOL IsValid(CLocalizableDialog* pDialog);
	BOOL AddLink(CWndObjDescription* pWndDesc, CWnd* pParentWnd, CJsonContextObj* pContext, CAbstractFormView* pParentView, DBTObject*&pDBT);
	virtual void BuildWebControlLinks(CParsedForm* pParsedForm, CJsonContextObj* pContext);
	virtual int GetLeftMargin(CWndObjDescription* pWndDesc, bool col1, bool bInStaticArea);
	void BuildWebControlLinks(CParsedForm* pParsedForm, CJsonContext* pContext, CWndObjDescription* pChild);
public:
	CJsonFormEngine();
	~CJsonFormEngine();
	static CJsonFormEngine* GetInstance();
};



#include "endh.dex"
