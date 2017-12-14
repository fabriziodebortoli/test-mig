#pragma once
#include <TbNameSolver\TbNamespaces.h>
#include <TbGeneric\referenceObjectsInfo.h>

//includere alla fine degli include del .H
#include "beginh.dex"

// useful class
//
class DataObj;
class DataObjArray;
class CBaseDocument;
class CTBNamespace;
class CTBTabbedToolbar;
class CTooltipProperties;
class HotLinkInfo;
// Questa classe virtuale pura e` deginita cosi` per evitare la dipendenza
// incrociata delle dlls genlib e ges. La vera implementazione e` nella
// libreria Ges nel file HotLink.cpp
//
/////////////////////////////////////////////////////////////////////////////
//							class HotKeyLinkObj definition
/////////////////////////////////////////////////////////////////////////////
class TB_EXPORT HotKeyLinkObj : public CObject
{
	DECLARE_DYNAMIC(HotKeyLinkObj)

	friend class CParsedCtrl;
	friend class CParsedCtrlEvents;

protected:
	CBaseDocument*	m_pDocument;
	CArray<CParsedCtrl*>	m_OwnerCtrls;
	WORD			m_wRunningMode;
	BOOL			m_bIsAddOnFlyRunning;

	BOOL			m_bAddOnFlyEnabled;
	BOOL			m_bMustExistData;

	BOOL			m_bEnableFillListBox;
	BOOL			m_bHotLinkEnabled;
	BOOL			m_bSearchOnLinkEnabled;
	BOOL			m_bLikeOnDropDownEnabled;
	BOOL			m_bHyperLinkEnabled;
	BOOL			m_bAutoFindable;//effettua la find in automatico quando necessario
	CStringArray	m_arAdditionalSensitiveFields;//segmenti di chiave aggiuntivi oltre a quello principale (es tipo clifor)
public:
	HotKeyLinkObj();
	virtual ~HotKeyLinkObj();
	
public:
	enum SelectionType	{ NO_SEL, DIRECT_ACCESS, UPPER_BUTTON, LOWER_BUTTON, COMBO_ACCESS, CUSTOM_ACCESS};

	CBaseDocument*	GetAttachedDocument	()	const	{ return m_pDocument; }
	DataObj*		GetAttachedData();
	SqlRecord*		GetMasterRecord();
public:
	virtual bool	FindNeeded(DataObj* pDataObj, SqlRecord* pMasterRec) { return false; }
	virtual	BOOL	ExistData			(DataObj* pData)	= 0;
	virtual	void	StopRunning			()					= 0;

	// ritorna i dati selezionati dalla Combo Query 
	// 0=FAILED 1=OK 2=MAX_REACHED
	virtual int	SearchComboQueryData (const int& nMaxItems, DataObjArray& pKeyData, CStringArray& arDescriptions) = 0;
	virtual	int	DoSearchComboQueryData(const int& nMaxItems, DataObjArray& pKeyData, CStringArray& arDescriptions)
													{ return SearchComboQueryData(nMaxItems, pKeyData, arDescriptions); };

public:
	virtual	void	AttachDocument		(CBaseDocument* pDocument)	{ m_pDocument = pDocument; }
	virtual	BOOL	Customize			(const DataObjArray&)		{ return FALSE; }
	virtual	BOOL	DispatchCustomize	(const DataObjArray& a)		{ return Customize(a); }

protected:
	virtual void	DoCallLink			(BOOL = FALSE)		= 0;
	virtual BOOL	SearchOnLinkUpper	()					= 0;
	virtual BOOL	SearchOnLinkLower	()					= 0;

protected:
	virtual	BOOL	CanDoCallLink		() const	{ return IsEnabledAddOnFly(); }
	virtual BOOL	FindDataRecord		(DataObj* pData) = 0;
	
public:
	virtual	BOOL		CanDoSearchOnLink	() const	{ return m_bSearchOnLinkEnabled; }
	virtual CDocument*	BrowserLink			
							(
									DataObj*, 
									CDocument*		= NULL, 
							const	CRuntimeClass*	= NULL,
									BOOL			= TRUE
							)		{ return NULL; };

	virtual void					InitNamespace		()  = 0;
	virtual const CString			GetAddOnFlyNamespace()	= 0;
	virtual CTBNamespace&			GetNamespace		()	= 0;

	CParsedCtrl*					GetOwnerCtrl() const;
	virtual void 					SetOwnerCtrl			(CParsedCtrl*	pCtrl);
	virtual void 					SetActiveOwnerCtrl		(CParsedCtrl*	pCtrl);
	virtual void 					RemoveOwnerCtrl			(CParsedCtrl*	pCtrl);

	virtual void 					OnCreatedOwnerCtrl		()					{}
	virtual void 					DoOnCreatedOwnerCtrl	();
	virtual void 					PreCreateOwnerCtrl		(CParsedCtrl*, DWORD& /*style*/) {}
	virtual void					OnCustomizeRadarToolbar	(CTBTabbedToolbar* pTabbedToolbar);
	virtual BOOL					GetToolTipProperties	(CTooltipProperties* pTp);
	virtual void					OnPrepareAuxData()					{}
	virtual void					OnPrepareForFind(SqlRecord* pRec);	//chiamato prima della Find automatica, per impostare alcune proprietà dell'hotlink
	virtual void					Parameterize(HotLinkInfo* pInfo, int buttonId)	{} //default does nothing
	virtual void	EnableFillListBox	(BOOL bEnable = TRUE)	{ m_bEnableFillListBox = bEnable; }
	virtual void	EnableAddOnFly		(BOOL bEnable = TRUE)	{ m_bAddOnFlyEnabled = bEnable;	}
	virtual void	EnableHotLink		(BOOL bEnable = TRUE)	{ m_bHotLinkEnabled = bEnable; }
	virtual void	MustExistData		(BOOL bExist = TRUE)	{ m_bMustExistData = bExist; }
	virtual void	EnableSearchOnLink	(BOOL bEnable = TRUE)	{ m_bSearchOnLinkEnabled = bEnable; }
	virtual void	EnableLikeOnDropDown(BOOL bEnable = TRUE)	{ m_bLikeOnDropDownEnabled = bEnable; }
	virtual void	EnableHyperLink		(BOOL bEnable = TRUE)	{ m_bHyperLinkEnabled = bEnable; }
	virtual void	EnableAutoFind		(BOOL bEnable = TRUE)	{ m_bAutoFindable = bEnable; }
	
	virtual BOOL	IsFillListBoxEnabled	()	const			{ return m_bEnableFillListBox; }
	virtual BOOL	IsMustExistData			()	const			{ return m_bMustExistData; }
	virtual BOOL	IsHotLinkEnabled		()	const			{ return m_bHotLinkEnabled; }
	virtual BOOL	IsHotLinkRunning		()	const			{ return m_wRunningMode != 0; }
	virtual BOOL	IsSearchOnLinkEnabled	()	const			{ return m_bSearchOnLinkEnabled; }
	virtual BOOL	IsLikeOnDropDownEnabled	()	const			{ return m_bLikeOnDropDownEnabled; }
	virtual BOOL	IsHyperLinkEnabled		()	const			{ return m_bHyperLinkEnabled; }
	virtual BOOL	IsAutoFindable			()	const			{ return m_bAutoFindable; }
	virtual BOOL	IsEnabledAddOnFly		()	const			{ return m_bAddOnFlyEnabled; }

	virtual BOOL	IsAddOnFlyRunning		()	const				{ return m_bIsAddOnFlyRunning; }
	virtual void	SetAddOnFlyRunning		(const BOOL& bDisable) 	{ m_bIsAddOnFlyRunning = bDisable; }

	virtual void		SetRunningMode(WORD runningMode)			{ m_wRunningMode = runningMode; }
	virtual WORD		GetRunningMode()							{ return m_wRunningMode; }
	virtual void		SetParamValue		(DataStr sName, DataObj* value) {}
	virtual DataObj*	GetField			(LPCTSTR sName) const { return NULL; }
	virtual CString		GetDescriptionField () const { return _T(""); }

	virtual void		OnExtendContextMenu	(CMenu& menu) {}
	virtual void		DoContextMenuAction	(UINT nCode)  {}

	virtual	void		BindParam			(DataObj* , int =-1) {}

	virtual BOOL		DoFindRecord				(DataObj* pKey)					{ return FALSE; }
	virtual CString		GetHKLDescription			()						const	{ return L""; }
	virtual DataObj*	GetDescriptionDataObj		()								{ return NULL; }
	virtual BOOL		IsHKLSimulated				()						const	{ return FALSE; }
	virtual BOOL		NeedToDestroyLinkedDocument	(const CBaseDocument*)	const	{ return FALSE; }
	virtual BOOL		SearchComboKeyDescription (const DataObj* /*pKey*/, CString& /*sDescription*/)
							{ ASSERT(FALSE); return FALSE; }	//da reimplementare negli HKLSimulated con ShowDescription
};

#include "endh.dex"

