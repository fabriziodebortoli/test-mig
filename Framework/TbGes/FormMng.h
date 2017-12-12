
#pragma once

#include <TbXmlCore\XMLTags.h>

#include <TbGeneric\Array.h>
#include <TbGeneric\FunctionObjectsInfo.h>
#include <TbGeneric\ReportObjectsInfo.h>
#include <TbGeneric\DataObjDescription.h>

#include "CustomSaveDialog.h"
#include <TbGenlib\baseapp.h>	

#include <TbNameSolver\TBNamespaces.h>

#include <TbGes\XmlGesInfo.h>

//includere alla fine degli include del .H
#include "beginh.dex"

class FormObjects;
class FormProperties;

class Parser;
class Unparser;
class BodyEditInfo;
class RadarInfo;
class CFormSheet;
class CFormReportDlg;
class CFormBodyDlg;
class CFormNSChangeArray;

#define STATUS_NORMAL				0
#define STATUS_HIDDEN				0x0001
#define STATUS_GRAYED				0x0002
#define STATUS_NOCHANGE_GRAYED		0x0004
#define STATUS_NOCHANGE_HIDDEN		0x0008
#define STATUS_LOCKED				0x0010
#define STATUS_SORTED_ASC			0x0020
#define STATUS_SORTED_DESC			0x0040

const GUID FORMMNG_NULLGUID={0x0, 0x0, 0x0, {0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0}};

//=============================================================================
class TB_EXPORT BodyEditColumn : public CObject
{
	DECLARE_DYNAMIC(BodyEditColumn);
public:
	CTBNamespace	m_Namespace;

	DataType	m_ColumnDataType;

	WORD		m_wStatus;			// current state
	WORD		m_wDefaultStatus;
	CString		m_strColumnTitle;
	CString		m_strDefaultColumnTitle;
	int			m_nColScreenWidth;
	int			m_nDefaultColScreenWidth;
	int			m_nColPos;
	int			m_nDefaultColPos;
	BOOL		m_bModified;
	CString     m_UICulture;
	BOOL		m_bStatusChanged;

public:
	BodyEditColumn();

	BodyEditColumn
		(
			CTBNamespace&	m_Namespace,

			LPCTSTR		pszColumnTitle,
			LPCTSTR		pszDefaultColumnTitle,
			DataType	ColumnDataType,
			WORD		wDefaultStatus = STATUS_NORMAL,
			WORD		wStatus = STATUS_NORMAL,
			int			nColPos = -1,
			int			nDefaultColPos = -1,
			int			nColScreenWidth = 0,
			int			nDefaultColScreenWidth = 0
		);

	// copy contructor e assignement operator
	BodyEditColumn (const BodyEditColumn&);
   	BodyEditColumn& operator	= (const BodyEditColumn&);

// Diagnostics
#ifdef _DEBUG
public:
	void Dump (CDumpContext& dc) const	{ ASSERT_VALID(this); AFX_DUMP0(dc, " BodyEditColumn\n");}
#endif // _DEBUG
};

//=============================================================================
class TB_EXPORT BodyEditInfo : public Array, public IOSLObjectManager
{
	DECLARE_DYNAMIC(BodyEditInfo);

public:
	CString m_strBodyTitle;
	BOOL	m_bModified;
	BOOL	m_bValid;		// determina se visualizzare nel tree della dialog
							// a causa di informazioni incomplete nel caso che il BodyEdit
							// non sia ancora stato istanziato.
	CRuntimeClass* m_pRowFormViewClass;

public:
	// copy contructor e assignement operator
	BodyEditInfo ();
	BodyEditInfo (const BodyEditInfo&);
	BodyEditInfo (const CTBNamespace& ns, LPCTSTR pszTitle = NULL);

	BodyEditInfo& operator	= (const BodyEditInfo&);

public:
	BOOL				IsDefaultBody	() const;
	void				ResetToDefault	();

	BodyEditColumn* 	GetColumnObject	(const CTBNamespace&) const;
	BodyEditColumn* 	GetColumnObject	(const CString&) const;

public:
	BodyEditColumn* 	GetAt		(int nIdx)const	{ return (BodyEditColumn*) Array::GetAt(nIdx);	}
	BodyEditColumn*&	ElementAt	(int nIdx)		{ return (BodyEditColumn*&) Array::ElementAt(nIdx); }
	
	BodyEditColumn* 	operator[]	(int nIdx)const	{ return GetAt(nIdx);	}
	BodyEditColumn*&	operator[]	(int nIdx)		{ return ElementAt(nIdx);	}

	virtual BOOL		LessThen( CObject*, CObject*) const ;
	virtual int			Compare(CObject* po1, CObject* po2) const;

	void				FixLineFeeds();
// Diagnostics
#ifdef _DEBUG
public:
	void Dump (CDumpContext& dc) const	{ ASSERT_VALID(this); AFX_DUMP0(dc, " BodyEditInfo\n");}
#endif // _DEBUG
};

//=============================================================================
class TB_EXPORT FormBodyEdits : public Array
{
public:
	// copy contructor e assignement operator
	FormBodyEdits() : Array() {}
	FormBodyEdits(const FormBodyEdits&);
	FormBodyEdits& operator	= (const FormBodyEdits&);

	void FixLineFeeds();

// Diagnostics
#ifdef _DEBUG
public:
	void Dump (CDumpContext& dc) const	{ ASSERT_VALID(this); AFX_DUMP0(dc, " FormBodyEdits\n");}
#endif // _DEBUG
};

//=============================================================================
class TB_EXPORT TileInfo : public CObject
{
	DECLARE_DYNAMIC(TileInfo);

public:
	CString	m_sNamespace;
	BOOL	m_bIsPinned;
	BOOL	m_bIsCollapsed;

	TileInfo(const CString& sNs, BOOL bIsPinned, BOOL bIsCollapsed = FALSE)
		: 
		m_sNamespace(sNs), m_bIsPinned(bIsPinned), m_bIsCollapsed(bIsCollapsed)
	{
	}

	TileInfo(const TileInfo& source) 
		: m_sNamespace(source.m_sNamespace), m_bIsPinned(source.m_bIsPinned), m_bIsCollapsed(source.m_bIsCollapsed)
	{
	}
};

//=============================================================================
class TB_EXPORT FormTiles : public Array
{
public:
	// copy contructor e assignement operator
	FormTiles() : Array() {}
	FormTiles(const FormTiles&);
	FormTiles& operator	= (const FormTiles&);

	// Diagnostics
#ifdef _DEBUG
public:
	void Dump(CDumpContext& dc) const	{ ASSERT_VALID(this); AFX_DUMP0(dc, " FormTiles\n"); }
#endif // _DEBUG
};

//=============================================================================
class TB_EXPORT FormProperties : public Array
{
public:
	enum OrderByType { FINDABLE_QUERY, BROWSER_QUERY, DEFAULT_QUERY};

public:
	OrderByType m_OrderByType;	// crea la orderby del radar usando i campi findable
	BOOL		m_bDescending;	// aggiunge la clausola descending nella orderby del radar
	BOOL		m_bFindSlave;	// permette la ricerca anche sui campi slave

public:
	// copy contructor e assignement operator
	FormProperties ();
	FormProperties (const FormProperties&);
   	FormProperties& operator	= (const FormProperties&);

public:
// Diagnostics
#ifdef _DEBUG
public:
	void Dump (CDumpContext& dc) const	{ ASSERT_VALID(this); AFX_DUMP0(dc, " FormProperties\n");}
#endif // _DEBUG
};

//=============================================================================
class TB_EXPORT CReportManager : public CObject
{
	friend class CFormManager;
	friend class CFormReportDlg;
	friend class ReportMngDlg;
	friend class CAdminToolDocReportDlg;

	CReportObjectsDescription m_arAllReports;

public:
	CReportObjectsDescription m_arShowReports;

	BOOL			m_bModUser;
	BOOL			m_bModAllUsers;
	CTBNamespace	m_NsUsr;
	CTBNamespace	m_NsAllUsrs;
	CTBNamespace	m_NsStd;
	CTBNamespace	m_NsCurrDefault;

	BOOL			m_bIsRadars;
	BOOL			m_bIsBarcode;

	CTBNamespace	m_NsSpecificReportForPrint;
	CTBNamespace	m_NsSpecificReportForEmail;

protected:
	CReportObjectsDescription m_arStandardReports;
	CReportObjectsDescription m_arAllUsersReports;
	CReportObjectsDescription m_arUserReports;

protected:
	BOOL Parse (const CTBNamespace& nsDoc, CString sFileName, CReportObjectsDescription& arReports, CTBNamespace& nsDefault, const CString& sUsr);

public:
	// copy contructor e assignement operator
	CReportManager(BOOL bIsRadars = FALSE,BOOL bIsBarcode = FALSE);
	CReportManager(const CReportManager&);
	CReportManager& operator=(const CReportManager& rm);

public:
	void	Clear			() { m_arAllReports.Clear(); }
	BOOL	Parse			(const CTBNamespace&, BOOL bAdmin = FALSE);
	BOOL	ParseUser		(const CTBNamespace&, const CString&);
	BOOL	AddDefaultReport();

	void	SetFrom			(CReportObjectsDescription& arReports, CBaseDescription::XMLFrom From);

	void    SetUserModified		(BOOL bMod = TRUE){m_bModUser = bMod;}
	void    SetAllUsersModified	(BOOL bMod = TRUE){m_bModAllUsers = bMod;}

	BOOL	IsUserModified		()	{return m_bModUser;}
	BOOL	IsAllUsersModified	()	{return m_bModAllUsers;}	
    
	CDocumentReportDescription* GetDefaultReportDescription();

	void						SetDefaultReportDescriptionInShowRep();
	CDocumentReportDescription*	GetDefaultReportDescriptionInShowRep();

	CDocumentReportDescription* GetReportDescription (int nIdx);
	CDocumentReportDescription* GetReportDescription (const CString& sReport);

	void						SetCurrentDefaultReport(const CString& sReport);
	CDocumentReportDescription* GetCurrentDefaultReport();

	void MakeGeneralArrayReport			();
	void SetDetaultInGeneralArray		();

	void RedistribuitionReportArray		();
	void MakeShowReportArray			();
	BOOL ExistElementInShowArray		(CDocumentReportDescription* pReportInfo);
	void CleanShowArray					(CDocumentReportDescription* pReportInfo);
	void CleanFromStd					();
	void CleanFromAllUsrs				();
	void RemoveFromGeneralReportArray	(CDocumentReportDescription* pReport);
};

//=============================================================================
class TB_EXPORT CFormManager : public CObject
{
	friend class CFormBodyDlg;
	friend class CFormRadarsDlg;
	friend class CFormReportDlg;
	friend class CFormSheet;
	friend class CAbstractFormDoc;

public:
	BOOL				m_bTBFModified;
	CReportManager		m_Barcode;
protected:
	CAbstractFormDoc*	m_pDocument;

	FormBodyEdits		m_BodyEditInfos;
	FormTiles			m_TileInfos;

	FormProperties		m_Properties;

	CFormNSChangeArray* m_pFormNSChangeArray;

	int			m_nRelease;		// release con cui unparsero la form corrente
	int			m_nFormRelease;	// release number della form appena letta (se non nuova)
	int			m_nNsRelease;	// release di namespace

	CReportManager				m_Reports;
	CReportManager				m_Radars;
	
	CTBNamespace				m_Ns;	//per la ChangeReport();

private:
	BOOL	m_bParseError;
	BOOL	m_bInternalModified;
	BOOL	m_ForCurrentUser;
	BOOL	m_InAllCompanies;
	BOOL	m_bUpdateTBFFiles;
	int		m_nDialogsModified;
	BOOL	m_bDialogsSaveStateEnabled;

public:
	CFormManager(CAbstractFormDoc* pDocument);
	CFormManager (const CFormManager&);
	virtual ~CFormManager();

   	CFormManager& operator	= (const CFormManager&);
	void	CopyTBFDataTo		(CFormManager* pTo);
	void	CopyRadarsDataTo	(CFormManager* pTo);
	void	CopyReportsDataTo	(CFormManager* pTo);
	void	CopyBarcodeDataTo	(CFormManager* pTo);
	void	FixLineFeeds		() { m_BodyEditInfos.FixLineFeeds(); }
public:
	CAbstractFormDoc*	GetDocument() const { return m_pDocument; }

	BOOL	SaveModified			();
	BOOL	IsModified				(BOOL& bXMLRadarModified, BOOL& bXMLReportModified, BOOL& bTBFModified, BOOL& bXMLBarcodeModified);
	BOOL	SaveSheetModified		(const CString& sUserForSave = _T(""), CFormManager* pFormMngToUpdate = NULL, BOOL bFromDoc = FALSE);
	int		EditForm				(BOOL bExecEnabled = TRUE, LPCTSTR pszNewFile = NULL);
	BOOL	ActivableForm			();
	
	// gestione delle dialogs
	BOOL	HasPinUnPinModified		() const;
	void	SetDialogsPinState		(CTBNamespace aNs, BOOL bPinned);
	void	SetDialogsCollapsedState(CTBNamespace aNs, BOOL bCollapsed);
	BOOL	HasDialogCustomized		(CTBNamespace aNs, BOOL& bPinned, BOOL& bCollapsed);
	void	EnableDialogStateSave	(BOOL bEnable = TRUE);
	BOOL	IsTBFInAutoSave			() const;


	CString GetRadarName			();
	CString	GetRadarName			(int nIdx);
	void	EnumRadarAlias			(CStringArray& arReportAlias);
	int		GetIndexRadarDefault	();
	int		GetIndexRadarPredefinito ();

	CString					GetReportName			();
	CString					GetReportName			(int nIdx);
	CFunctionDescription*	GetReportDescription	();
	CFunctionDescription*	GetReportDescription	(int nIdx);

	void	EnumReportAlias			(CReportMenuNode* pReportRootNode);
	int		GetIndexReportDefault	();

	CString					GetBarcodeName			();
	CString					GetBarcodeName			(int nIdx);
	CFunctionDescription*	GetBarcodeDescription	();
	CFunctionDescription*	GetBarcodeDescription	(int nIdx);
					void	EnumBarcodeAlias		(CStringArray& arReportAlias);
					int		GetIndexBarcodeDefault	();

	CReportManager*			GetReports	() { return &m_Reports; }

	// Usati dal BodyEdit
	BodyEditInfo*			GetBodyEditInfo(const CTBNamespace&) const;
	BodyEditInfo*			GetBodyEditInfo(const CString& sNs) const;
	int						AddBodyEditInfo(BodyEditInfo* aObj) { return m_BodyEditInfos.Add(aObj); }
	const FormBodyEdits*	GetBodyEditInfos() const { return &m_BodyEditInfos; }

	//Tiles pinned
	TileInfo*				GetTileInfo(const CTBNamespace&) const;
	TileInfo*				GetTileInfo(const CString& sNs) const;
	int						AddTileInfo(TileInfo* aObj);
	const FormTiles*		GetTileInfos() const { return &m_TileInfos; }

public:
	// parsing/unparsing
	BOOL Parse				();
	BOOL ParseXML			();
	BOOL ParseTBF			();
	BOOL ParseForm			(Parser& lex);
		BOOL ParseObjects		(Parser& lex);
			BOOL ParseBodyInfo		(Parser& lex);
				BOOL ParseAllColumns	(Parser& lex, BodyEditInfo*);
					BOOL ParseColumnInfo	(Parser& lex, BodyEditInfo*);
			BOOL ParseTileInfo		(Parser& lex);
		BOOL ParseProperties(Parser& lex);

	BOOL UnParse			(const CString& sUserForSave = _T(""));
	void UnparseReportXML	(const CString& sUserForSave = _T(""));
	void UnparseRadarXML	(const CString& sUserForSave = _T(""));
	void UnparseBarcodeXML  (const CString& sUserForSave = _T(""));
	BOOL UnparseTBF			();
	void UnparseForm		(Unparser& ofile);
		void UnparseObjects		(Unparser& ofile);
			void UnparseBodyEditInfos	(Unparser& ofile);
				void UnparseBodyEditInfo	(Unparser& ofile, BodyEditInfo*);
			void UnparseTileInfos	(Unparser& ofile);
		void UnparseProperties	(Unparser& ofile);	

	BOOL UnparseStandardReportsXML	();
	BOOL UnparseAllUsersReportsXML	(BOOL bIsRadars = FALSE);
	BOOL UnparseUserReportsXML(const CString& sUsr/*CCustomSaveInterface*	pSaveInterface*/, BOOL bIsRadars = FALSE, BOOL bIsBarcode = FALSE,  BOOL bIsAllUser = FALSE);

private:
	BOOL UnparseTBFSilent	();
	void ParseFormNSChanges	();

// Diagnostics
#ifdef _DEBUG
public:
	void Dump (CDumpContext&) const;
	void AssertValid() const;
#endif // _DEBUG
};

//===========================================================================

/////////////////////////////////////////////////////////////////////////////
// 				class ReportMngDlg 
/////////////////////////////////////////////////////////////////////////////
//
class TB_EXPORT ReportMngDlg : public CLocalizablePropertySheet
{
	friend class CFormReportDlg;

	DECLARE_DYNAMIC(ReportMngDlg);

public:
	CFormReportDlg*	m_pFormReportDlg;
	CReportManager	m_RepManager;
	CReportManager&	m_RepManagerSource;
	CString&		m_sReportName;
	CString			m_sTitleName;
	CTBNamespace	m_Namespace;
	BOOL			m_bUpdated;

public:
	ReportMngDlg	(CString& strReportName, const CString& sDocumentNamespace);
	ReportMngDlg	(CString& strReportName, CAbstractFormDoc*);

	~ReportMngDlg	();

public:
	CString GetSelectedRepTitle	() {return m_sTitleName;}
	
protected:
	void	SetUpdateable		(BOOL bUpd = TRUE) { m_bUpdated = ! bUpd; }
	BOOL	SaveSheet			(const CString& sUserForSave, CReportManager* pRepMng);

protected:
	// Generated message map functions
	//{{AFX_MSG(ReportMngDlg)
	virtual BOOL OnInitDialog	();
	//}}AFX_MSG
DECLARE_MESSAGE_MAP()

// Diagnostics
#ifdef _DEBUG
	void Dump(CDumpContext& dc) const
		{ ASSERT_VALID(this); AFX_DUMP0(dc, " ReportMngDlg\n"); CLocalizablePropertySheet::Dump(dc); }
#endif // _DEBUG
};

//-----------------------------------------------------------------------------
TB_EXPORT CString GetPredefinedRadar();
TB_EXPORT CString GetAliasRadar();
TB_EXPORT CTBNamespace AfxGetDocumentDefaultReport(const CString& sDocumentNamespace);
TB_EXPORT CString AfxGetDocumentDefaultTitleReport(const CString& sDocumentNamespace);

//=============================================================================
#include "endh.dex"
