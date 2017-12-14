#pragma once
#include "HOTLINK.H"
#include "beginh.dex"

class DataObjAssociation
{
public:
	DataObj* m_pCompositeHKL = NULL;
	DataObj* m_pHKL = NULL;

	DataObjAssociation(DataObj* pCompositeHKL, DataObj* pHKL)
		: m_pCompositeHKL(pCompositeHKL), m_pHKL(pHKL)
	{
	}
};
typedef TArray<DataObjAssociation> ASSOCIATIONS;

class TB_EXPORT ComposedHotLink : public HotKeyLink, public CDataEventsObj
{
	DECLARE_DYNCREATE(ComposedHotLink)

	CMap<CString, LPCTSTR, HotKeyLink*, HotKeyLink*> m_HotLinks;
	CMap<CString, LPCTSTR, ASSOCIATIONS*, ASSOCIATIONS*> m_Mappings;
	DataObj* m_pSelector = NULL;
	CString m_sSelectorVar;
	HotKeyLink* m_pCurrentHKL = NULL;
	int m_nButtonId = BTN_DEFAULT;
	ComposedHotLink();
	~ComposedHotLink();
public:
	const HotKeyLink* GetCurrentHotLink() { return m_pCurrentHKL; }
	virtual void SetOwnerCtrl(CParsedCtrl* pCtrl);
	virtual void SetActiveOwnerCtrl(CParsedCtrl* pCtrl);
	virtual void RemoveOwnerCtrl(CParsedCtrl* pCtrl);
	virtual void Signal(CObservable* pSender, EventType eType);
		
	virtual void Fire(CObservable* pSender, EventType eType) {}
	virtual CObserverContext* GetContext() const { return NULL; }

	virtual void		OnDefineQuery(SelectionType nQuerySelection = DIRECT_ACCESS);
	virtual void		OnPrepareQuery(DataObj*, SelectionType nQuerySelection = DIRECT_ACCESS);

	virtual BOOL		IsValid();
	virtual BOOL		OnValidateRadarSelection(SqlRecord*);
	virtual void		OnRecordAvailable();
	virtual void		OnCallLink();

	virtual FindResult	FindRecord(DataObj*, BOOL bCallLink = FALSE, BOOL bFromControl = FALSE, BOOL bAllowRunningModeForInternalUse = FALSE);
	virtual BOOL 		SearchOnLink(DataObj* pData = NULL, SelectionType nQuerySelection = NO_SEL);
	virtual int			SearchComboQueryData(const int& nMaxItems, DataObjArray& pKeyData, CStringArray& arDescriptions);
	virtual SqlParamArray* GetQuery(SelectionType nQuerySelection, CString& sQuery, CString sFilter /*= _T("")*/);
	virtual const CString GetAddOnFlyNamespace();
	virtual void		GetAuxInfoForHklBrowse(LPAUXINFO& pInfo);
	virtual CDocument*	BrowserLink (DataObj*,CDocument* = NULL,const	CRuntimeClass* = NULL,BOOL = TRUE);
	virtual	void		DoCallLink(BOOL bAskForCallLink = FALSE);
	virtual void		CloseTable();
	virtual void		OnRadarRecordAvailable();
	virtual	BOOL		ExistData(DataObj* pData);
	virtual void		OnPrepareAuxData();
	virtual void		OnPrepareForFind(SqlRecord* pRec);
	virtual	BOOL	 	SearchOnLinkUpper();
	virtual	BOOL 		SearchOnLinkLower();
	virtual FindResult	OnRecordNotFound();
	virtual bool		FindNeeded(DataObj* pDataObj, SqlRecord* pMasterRec);
	void	EnableFillListBox(BOOL bEnable = TRUE);
	void	EnableAddOnFly(BOOL bEnable = TRUE);
	void	EnableHotLink(BOOL bEnable = TRUE);
	void	MustExistData(BOOL bExist = TRUE);
	void	EnableSearchOnLink(BOOL bEnable = TRUE);
	void	EnableLikeOnDropDown(BOOL bEnable = TRUE);
	void	EnableHyperLink(BOOL bEnable = TRUE);
	void	EnableAutoFind(BOOL bEnable = TRUE);
	void	SetRunningMode(WORD runningMode);
	WORD	GetRunningMode();

	
	BOOL	IsFillListBoxEnabled()	const;
	BOOL	IsMustExistData()	const;
	BOOL	IsHotLinkEnabled()	const;
	BOOL	IsHotLinkRunning()	const;
	BOOL	IsSearchOnLinkEnabled()	const;
	BOOL	IsLikeOnDropDownEnabled()	const;
	BOOL	IsHyperLinkEnabled()	const;
	BOOL	IsAutoFindable()	const;
	BOOL	IsEnabledAddOnFly()	const;

	BOOL	IsAddOnFlyRunning()	const;
	void	SetAddOnFlyRunning(const BOOL& bDisable);

	void SelectHotLink();
	void AddHotLink(const CString& sName, HotKeyLink* pHotLink);
	void MapField(const CString& sHKLName, const CString& sName, DataObj& sourceDataObj);
	virtual void Parameterize(HotLinkInfo* pInfo, int buttonId) { m_sSelectorVar = pInfo->m_strSelector; m_nButtonId = buttonId; }
};
class TB_EXPORT ComposedJsonHotLink : public ComposedHotLink
{
	bool m_bParameterized = false;

	DECLARE_DYNCREATE(ComposedJsonHotLink)

	virtual void Parameterize(HotLinkInfo* pInfo, int buttonId);
};
#include "endh.dex"