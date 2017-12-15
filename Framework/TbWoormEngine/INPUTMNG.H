#pragma once

#include <TbParser\TokensTable.h>

#include <TbGeneric\Array.h>
#include <TbGenlib\basedoc.h>
#include <TbGenlib\TBStrings.h>
#include <TbGenlib\parsctrl.h>

#include <TbGes\TileManager.h>
#include <TbGes\TileDialog.h>
#include "ActionsRepEngin.h"

#include "askdata.h"

//includere alla fine degli include del .H
#include "beginh.dex"

//=============================================================================
class CWnd;
class DataObj;
class SymTable;
class AskEntry;
class AskGroup;
class Expression;
class EngineScheduler;
class CParsedForm;
class CStaticFieldArrayCache;
class InputMng;
class CAutoExpressionMng;
class Block;
class WoormTable;
class WoormField;
class COutParamBag;
class CExportDataNetAskDialog;
class CExportDataNetAskDialogGroup;
class CExportDataNetAskObject;

//============================================================================
class TB_EXPORT AskEntry : public CObject, public CAbstractCtrl
{
public:
	AskFieldData*	m_pAskFieldData;
	BOOL			m_bOwnsAskFieldData;

	AskGroup*		m_pOwnerAskGroup;

	DataObj*		m_pData;
	Expression*		m_pInitExpr;

	CString			m_strLocalizedCaption;

	BOOL			m_bHidden;
	//BOOL			m_bHide;
	int				m_nLen;
	int				m_nPrec;
	int				m_nRows;

	CStatic*		m_pStatic;
	CParsedCtrl*	m_pCtrl;
	int				m_nCtlId;

	int				m_nMinWidth;
	int				m_nMinHeight;
	int				m_nMinAlign;

	int				m_nCaptionWidth;
	int				m_nCaptionHeight;
	int				m_nFieldWidth;
	int				m_nFieldHeight;
	int				m_nAlign;

	CString			m_strAutomaticExpression;

	DataObjArray	m_HLComponentParamsData;

public:
	AskEntry    (AskGroup* pOwner, AskFieldData* pAskFieldData = NULL);
	virtual ~AskEntry   ();

	BOOL	AddHotLinkParam	();

	BOOL    AllocItems      (CWnd* pParent, int& nCtlId, BOOL bGroup, CBaseDocument* pDocument);
	void    SetFont         (CFont* pFont, BOOL bRedraw = TRUE);
	void    SetFocus		();
	void	SetEntrySize	(int len, int prec = 0);

	void    EvalMinSize     ();
	void    Layout          (int Left, int Top, int Width, int Height);

	BOOL    GetEdit					(BOOL bGetAutomaticExpression);
	void    DoValueChanged			(BOOL bSetAutomaticExpression, BOOL bInitData/* = TRUE*/, LPCTSTR /*= NULL*/, CObArray* parCascadeChangedEntries/* = NULL*/, COutParamBag* pBag/* = NULL*/);
	void    SetupQueryComponentsEdit(LPCTSTR = NULL);
	BOOL	SetAutomaticExpression	(const CString& strDataExpr, BOOL bSetValue);

	virtual CString		GetPublicName	()	{ return m_pAskFieldData->m_strPublicName; }
	virtual DataObj*	GetCtrlData		()	{ return m_pData; }
	virtual CString		GetCtrlClass	()	{ return _T("AskEntry"); }
	virtual CString		GetCtrlName		()	{ return m_pAskFieldData->m_strPublicName; }

	virtual BOOL		IsAutomaticExpression () const	{ return m_pCtrl && m_pCtrl->IsAutomaticExpression(); }
	virtual CString		GetAutomaticExpression () const	{ return m_strAutomaticExpression; }
	virtual BOOL		SetAutomaticExpression (const CString& strExpr);

	void QueryHiddenStatus();
	
	virtual	void OnPresetStyle(DWORD &style) {}

	void	GetXmlParameters(CExportDataNetAskObject & entry);
};

//============================================================================
class TB_EXPORT AskGroup : public CObject 
{
public:
	BOOL			m_bHiddenTitleGroupBoxAlways;
	AskGroupData*	m_pAskGroupData;
	BOOL			m_bOWnsAskGroupData;
	TileDialogSize  m_TileDialogSize;

	Array   	m_Entries;
	InputMng*	m_pOwnerInputMng;

	CString		m_strLocalizedCaption;

	CWnd*   	m_pGroupBox;

	int     	m_nMinWidth;
	int			m_nMinHeight;
	int			m_nMinAlign;
	int     	m_nAlign;
	int			m_nCaptionHeight;

	BOOL		m_bHidden; //contiene a runtime la valutazione dell' HideExpr effettuata in fase di creazione della dialog

public:
	AskGroup    (InputMng* pOwner, AskGroupData* pAskGroupData);
	virtual ~AskGroup   ();

	AskEntry*	AddEntry	(AskFieldData* pAskFieldData, WoormField* pRepField);

	BOOL    AllocItems      (CWnd* pParent, int&, CBaseDocument*);
	void    SetFont         (CFont* pFont, BOOL bRedraw = TRUE);
	BOOL	SetFocus		();
	void    ShowWindows     ();

	BOOL    GetEdits					(BOOL bGetAutomaticExpression);
	void    DoValueChanged				(BOOL bSetAutomaticExpression, BOOL bInitData/* = TRUE*/, COutParamBag* pBag/* = NULL*/);
	void    SetupQueryComponentsEdits	();
	BOOL	IsOneBoolCltrAligned		() const;

	void    EvalMinSize     ();
	void    Layout          (int Left, int Top, int Width, int Height);

	void	QueryHiddenStatus();

	void	GetXmlParameters(CExportDataNetAskDialogGroup& dataGroup, int nGroup);
};

//==============================================================================
class TB_EXPORT CAskDialogDataAndGroups : public CObject
{
public:	//members
		AskDialogData*				m_pAskDialogData;
		Array						m_Groups;
		int							m_idxArray;
		int							m_idxTab;	//serve per sapere il numero di tab (tutto cio' per gestire il FirstFocus in manieta light in navigazione sui tab)
		CArray<CTileDialog*>		m_Tiles;	//serve per fare la QueryHiddenStatus (cambio di tab)
		
		CAskDialogDataAndGroups(AskDialogData* pAskDialogData = NULL)
			:
			m_pAskDialogData		(pAskDialogData),
			m_idxTab				(-1)
		{
		}

		~CAskDialogDataAndGroups() { }

public: //methods
		BOOL			IsBeforeBlockEecuted()
		{
			return !m_pAskDialogData->m_pBeforeBlock || m_pAskDialogData->m_pBeforeBlock->Exec();
		}

		BOOL			IsWhenExprEvalued(DataBool& bWhenResult)
		{
			return !m_pAskDialogData->m_pWhenExpr || m_pAskDialogData->m_pWhenExpr->Eval(bWhenResult); 
		}

		BOOL			IsAfterBlockExecuted()
		{
			return !m_pAskDialogData->m_pAfterBlock || m_pAskDialogData->m_pAfterBlock->Exec();
		}
		
		BOOL			IsOnExprEvalued(DataBool& bAbortRun)
		{
			return !m_pAskDialogData->m_pOnExpr || m_pAskDialogData->m_pOnExpr->Eval(bAbortRun);
		}

		BOOL			IsAbortExprEvalued(DataStr& sAbortMsg)
		{
			return! m_pAskDialogData->m_pAbortExpr || m_pAskDialogData->m_pAbortExpr->Eval(sAbortMsg);
		}

};

typedef CArray<CAskDialogDataAndGroups*> CAskDialogDataAndGroupsArray;

//=============================================================================
class TB_EXPORT InputMng : public CObject
{
	friend class CAskDoc;
	//friend class CAskFrame;
	//friend class CAskView;
	friend class CDynamicDlg;
	friend class AskGroup;
	friend class AskEntry;
	friend class AskDialogInputMng;
	friend class CXMLExportUserCriteriaPage;
	friend class CDynamicContainerDlg;
	friend class CDynamicContainerTileGrp;

public:
	enum ErrorLevel { NONE, BEFORE_BLOCK, WHEN_EXPR, AFTER_BLOCK, ON_EXPR, ABORT_EXPR };
	enum RunningMode { RUNNING, DESIGN };

private:
	CDynamicContainerDlg*		 m_pCDynamicContainerDlg;
	CAskDialogDataAndGroupsArray m_AllAskDialogDataAndGroups;
	CStringArray				 m_AllActions;
	int							 m_idxLastDialog;
	int							 m_nCtlId;
	int							 m_MinDialogHeight;

	CString			m_strCustomErr;
	CString			m_strErrorDetail;
	ErrorLevel		m_ErrorLevel;
	RunningMode		m_DesignMode;
	CMessages		m_ErrorMessages;

	BOOL			m_bOwnsAskDialogData;

	int				m_nRetVal;
	COutParamBag*	m_pBag;

	CBaseDocument*		m_pDocument;
	WoormTable*			m_pSymTable;
	EngineScheduler*	m_pEngineScheduler;
	BOOL				m_bRunningFromExternalController;

	volatile short	m_nPressedBtn;

public:
	InputMng(CBaseDocument* pDocument, WoormTable* pSymTable = NULL, BOOL bOwnsAskDialogData = FALSE);
	virtual	~InputMng();
	
//	Execution API's
public:
	void	SetAllActions	(const CStringArray& allActions) { m_AllActions.RemoveAll(); m_AllActions.Append(allActions); }
	//BOOL	AskModal		(CWnd* pParent);
	BOOL	AskModalTiles	(CWnd* pParent);
	CSize	CreateAskDialog	(CParsedForm*, BOOL bInitData = TRUE);
	void	CreateAskDialog (CTilePanel* pPanel, CAskDialogDataAndGroups* pAskDialogDataAndGroups,  BOOL bInitData = TRUE);
	void	SetFocusFirst	(int);
	BOOL	CanDoNextAskDialog(int nDialog);
	BOOL	SomethingVisible	(CAskDialogDataAndGroups* pAskDialogDataAndGroups, BOOL& bFoundErrors);
	void	RemoveTilePanelTab	(CTilePanel*, int);

	void	CleanUpAskDialogDataAndGroups	();
	BOOL	SyncAllAskSavedFromExternalController();

	RunningMode	GetDesignMode	() { return m_DesignMode; }
	void		SetDesignMode	(RunningMode DesignMode) { m_DesignMode = DesignMode; }

	void		SetErrorLevel	(ErrorLevel eErrorLevel) { m_ErrorLevel = eErrorLevel; }
	ErrorLevel	GetErrorLevel	() { return m_ErrorLevel; }
	void		ResetErrorLevel	();
	void		ManageErrorLevel();
	void		Message			(LPCTSTR lpszText);
	BOOL		ShowMessages	();

	int							GetIdxAskDialog		(int nByIdxTab);
	CAskDialogDataAndGroups*	GetNextAskDialog	(BOOL bFromNavigator, BOOL& bFoundErrors);
	CAskDialogDataAndGroups*	GetAskDialog		(int nByIdxTab);

protected:
	BOOL	ReadAllEdits		(CParsedForm*);
	void	DoDynValueChanged	(UINT nID, BOOL bSetValueChanged = TRUE);
	void	DoDynValueChanged	(const CString& sName, BOOL bSetValueChanged = TRUE);
	void	DoDynValueChanged	(AskEntry* pAEChanged, BOOL bSetValueChanged);
	void	UpdateDynCtrlView	();
	void	UpdateDynCtrlGrp	(CAskDialogDataAndGroups*);

public:
	void	SetAskDlgEntryAutomaticExpression	(CAutoExpressionMng*); 
	BOOL	SyncExternalControllerInfo			(BOOL bSave, CAskDialogDataAndGroups*);
	void	SetParameterDescription				(COutParamBag* pBag) { m_pBag = pBag; }
	void	InitFieldsOfHiddenDialog			(int nDialog);
	void	GetXmlParameters					(CExportDataNetAskDialog& dialog);
};


//////////////////////////////////////////////////////////////////////////////////
//						CDynamicContainerTileGrp
////////////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
class TB_EXPORT CDynamicContainerTileGrp : public CTileGroup
{
	DECLARE_DYNCREATE(CDynamicContainerTileGrp)

public:
	//memorize tile dialog widths
	int				m_WideWidth;
	int				m_LargeWidth;
	int				m_StandardWidth;
	int				m_MiniWidth;

protected:
	virtual	void Customize();
};

//////////////////////////////////////////////////////////////////////////////
//					    CDynamicContainerTileDlg
//////////////////////////////////////////////////////////////////////////////
//=============================================================================
class TB_EXPORT CDynamicContainerTileDlg : public CTileDialog
{
	DECLARE_DYNCREATE(CDynamicContainerTileDlg)

public:
	CDynamicContainerTileDlg();
	~CDynamicContainerTileDlg() {}

	void SetTileDlgName(CString sName);

	//{{AFX_MSG(CDynamicContainerTileDlg)
	afx_msg	BOOL	OnEraseBkgnd(CDC* pDC);
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};

////////////////////////////////////////////////////////////////////////////////
//							CDynamicContainerDlg
///////////////////////////////////////////////////////////////////////////////
class TB_EXPORT CDynamicContainerDlg : public CParsedDialogWithTiles
{
	DECLARE_DYNAMIC(CDynamicContainerDlg);

	friend class CDynamicContainerTileGrp;

private:
	int				m_bFromNavigator;
	int				m_nFirstActiveTab;
	BOOL			m_bCreated;
		
public:
	InputMng&		m_OwnInputMng;
	CTilePanel*		m_pTilePanel;
	
public:
	CDynamicContainerDlg(InputMng&, CWnd* pParent = NULL);

private:
	int				GetDialogMinHeight		(BOOL bWithoutTabber = FALSE);
	int				DecideOptimalWidth		(CDynamicContainerTileGrp* pGroup);
	void			RemoveAllTabsFrom		(int nTab);
	void			ManageAfterValueChanged	();
	void			SetDefaultActionButton	();
	BOOL			DialogsCleanup			(int idTab);
	BOOL			DoGoNextClick			(int idTab = -1);
	void			CenterMe				(CWnd* pWndParent);

	CAskDialogDataAndGroups* GetNextAskDialog(BOOL& bFoundErrors);

public:
	void			AskModal();
	
protected:
	virtual BOOL	OnInitDialog		();
	virtual BOOL	OnCommand			(WPARAM, LPARAM);
	virtual void	EndDialog			(int nResult);
	virtual void	OnCustomizeToolbar	();
	virtual	BOOL	PreProcessMessage	(MSG* pMsg);
	
protected:
	//{{AFX_MSG( CDynamicContainerDlg )   
	afx_msg LRESULT OnGetReportNamespace	(WPARAM, LPARAM);
	afx_msg	LRESULT OnGetControlDescription	(WPARAM wParam, LPARAM lParam);
	afx_msg	LRESULT OnGetLocalizerInfo		(WPARAM wParam, LPARAM lParam);
	afx_msg	void	OnWindowPosChanging		(WINDOWPOS FAR* wndPos);
	afx_msg void	OnMoving				(UINT, LPRECT);
	afx_msg LRESULT OnChangeActiveTab		(WPARAM, LPARAM);
	afx_msg LRESULT OnChangingActiveTab		(WPARAM, LPARAM);

	afx_msg void	OnGoPrevClick			();
	afx_msg void	OnGoNextClick			();
	afx_msg void	OnUpdateCommandGoPrev	(CCmdUI*);
	afx_msg void	OnUpdateCommandGoNext	(CCmdUI*);
	afx_msg void	OnUpdateCommandOK		(CCmdUI*);
	//}}AFX_MSG

protected:
	DECLARE_MESSAGE_MAP()
};

///////////////////////////////////////////////////////////////////////////////
//							CDynamicDlg
///////////////////////////////////////////////////////////////////////////////
//
///////////////////////////////////////////////////////////////////////////////
/*class TB_EXPORT CDynamicDlg : public CParsedDialog
{
	DECLARE_DYNAMIC(CDynamicDlg);

protected:
	InputMng&			m_OwnInputMng;

public:
	CDynamicDlg  (InputMng&, CWnd* pParent = NULL);

public: 
	void			AskModal		();

protected:
	virtual BOOL	OnInitDialog    ();
	virtual BOOL	OnCommand       (WPARAM, LPARAM);
	virtual void	EndDialog		(int nResult);

	//virtual void	PostNcDestroy	();

protected:
	//{{AFX_MSG( CDynamicDlg )   
	afx_msg LRESULT OnGetReportNamespace(WPARAM, LPARAM);
	afx_msg	LRESULT OnGetControlDescription(WPARAM wParam, LPARAM lParam);
	afx_msg	LRESULT OnGetLocalizerInfo(WPARAM wParam, LPARAM lParam);
	//}}AFX_MSG
	
protected:
	DECLARE_MESSAGE_MAP()
};
*/
///////////////////////////////////////////////////////////////////////////////
//							AskDialogInputMng
///////////////////////////////////////////////////////////////////////////////
//
///////////////////////////////////////////////////////////////////////////////
class TB_EXPORT AskDialogInputMng : public CObject
{
protected:
	AskRuleData*	m_pAskRuleData;
	WoormTable*		m_pSymTable;
	CBaseDocument*	m_pDocument;

private:
	CStringArray	m_AllActions;
public:
	AskDialogInputMng(AskRuleData* pAskRuleData, WoormTable* pSymTable, CBaseDocument*	pDocument)
	{
		m_pAskRuleData = pAskRuleData; 
		m_pSymTable = pSymTable;
		m_pDocument = pDocument;
	}

public:
	void			SetAllActions(const CStringArray& allActions) { m_AllActions.RemoveAll(); m_AllActions.Append(allActions); }
	BOOL			ExecAskRules	(CWnd* pParent, BOOL bInitOnly);
	BOOL			ExecAskRules	(CWnd* pParent, ReportEngine* pReport = NULL, BOOL bDesignMode = FALSE, COutParamBag* pBag = NULL);
	BOOL			ExecAsk			(CWnd* pParent, AskDialogData*, ReportEngine* pReport = NULL, BOOL bDesignMode = FALSE, COutParamBag* pBag = NULL);
	BOOL			ExecAllActions	(CWnd* pParent, ReportEngine* pReport = NULL, BOOL bDesignMode = FALSE, COutParamBag* pBag = NULL);
	BOOL			ExecAllAsk		(CWnd* pParent, ReportEngine* pReport = NULL, BOOL bDesignMode = FALSE, COutParamBag* pBag = NULL, BOOL bIsOnAsk = FALSE);
	BOOL			LoadInputMng	(InputMng*, AskDialogData* = NULL);		
	BOOL			LoadAllInputMng	(InputMng*, BOOL bIsOnAsk);

private:
	BOOL			IsExistingAction(CString aAction);
	BOOL			ReturnExec		(InputMng& inpMng, BOOL bReturn);

protected:
	//{{AFX_MSG( AskDialogInputMng )   
	//}}AFX_MSG
};

#include "endh.dex"
