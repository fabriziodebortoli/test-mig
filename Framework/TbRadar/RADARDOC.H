
#pragma once

#include <TBGenlib\const.h> 

#include <TbGes\extdoc.h>
#include <TbGes\TbRadarInterface.h>
#include <TbWoormViewer\expexter.h>
#include <TbWoormViewer\export.h>
#include "radardef.h"

//includere alla fine degli include del .H
#include "beginh.dex"
//=============================================================================

class WClause;
class DataObj;
class SqlSession;
class SqlTable;
class SqlRecord;
class SqlColumnInfo;
class Scheduler;
class SqlRecord;
class HotKeyLink;
class CExportDataGenericNet;
class CExportInfo;
//-----------------------------------------------------------------------------
#define IDT_RADAR	300	// id del timer attivato dal radar

//  E' ammessa solo la creazione di radar attraverso la OnNewDocument
//=============================================================================
#define CREATE_RADAR(name, view, record) \
	APMGR_CALLBACK name(DocInvocationInfo*) \
	{ \
		CSingleExtDocTemplate* pTemplate = AfxGetTemplate(RUNTIME_CLASS(view));\
		ASSERT(pTemplate);\
		if (pTemplate)\
		{\
			CInfoOSL infoOSL(pTemplate->GetNamespace(), pTemplate->GetInfoOSL().m_eType);\
			AfxGetInterfaceOSL()->GetObjectGrant(&infoOSL));\
			if (!OSL_CAN_DO(&(infoOSL), OSL_GRANT_BROWSE))\
			{\
				AfxMessageBox(cwsprintf(OSLErrors::MISSING_GRANT(), GetInfoOSL()->m_Namespace.ToString())););\
				return NULL; \
			}\
		}\
		CRadarDoc* pRadarDoc = (CRadarDoc*) AfxOpenDocumentOnCurrentThread(AfxGetTemplate(RUNTIME_CLASS(CRadarView)), NULL);\
		if (pRadarDoc)\
			pRadarDoc->Attach(RUNTIME_CLASS(view), RUNTIME_CLASS(record));\
		return NULL; \
	}

// Classi definite nel radar
//=============================================================================
class CRadarFrame;
class CRadarView;
class RadarBody;
class RadarColumnsIdxArray;

class RadarFindCondition;

// Classi definite all'interno del modulo
//=============================================================================
class RadarSqlRecCircleList;
class CRadarDoc;

//	Definizione di RadarSqlRecCircleList (usata solo da CRadarDoc)
//=============================================================================
class TB_EXPORT RadarSqlRecCircleList : public Array
{
private:
	// Numero massimo di record presenti nell'array circolare.
	int m_nNrMaxRecords;
	// Indice per la gestione dell'array circolare
	int m_nStartIdx;

	CRadarDoc* m_pDoc;

private:
	SqlRecord*	CloneCallerRec ();

	SqlRecord*	ReadNext (int nNewIdx, BOOL bAdd);
	SqlRecord*	ReadPrev (int nNewIdx, BOOL bAdd);

	BOOL MoveNext (SqlRecord*);
	BOOL MovePrev (SqlRecord*);

public:
	RadarSqlRecCircleList (CRadarDoc*);

public:
	SqlRecord*	GetAt		(int nIdx);
	DataObj*	GetDataObj	(int nIdxRec, int nIdxFld);

	void RemoveAll ()	{ Array::RemoveAll(); m_nStartIdx = 0; }

public:
	int GetMaxSize	();
	int GetStartIdx	();

public:
	void SetMaxSize	(int nNrRec)	{ m_nNrMaxRecords	= nNrRec;	}
	void SetStartIdx(int nStartIdx) { m_nStartIdx		= nStartIdx;}

public:
	int Add (SqlRecord* pRec)	{ return Array::Add ((CObject*)pRec);}
	// Caricano i record nell'array circolare leggendoli dalla tabella
	SqlRecord*	InsertNext ();
	SqlRecord*	InsertPrev ();

	BOOL LoadFromBottom ();
	BOOL LoadFromTop 	();
	void SkipRows		(BOOL bNextDirection);

// Diagnostics
#ifdef _DEBUG
public:
	void Dump		(CDumpContext&)	const;
	void AssertValid()				const { Array::AssertValid(); }
#endif // _DEBUG
};

//	Definizione di CRadarDoc
//=============================================================================
class TB_EXPORT CRadarDoc : public CAbstractDoc, public ITBRadar
{
	friend class RadarSqlRecCircleList;
	friend class RadarBody;

	DECLARE_DYNCREATE(CRadarDoc)

protected:
	// Flag di abilitazione del collegamento con il Documento o DBT per la visualizzazione
	// automatica del record selezionato
	BOOL m_bLinkRadar;
	
	BOOL m_bDocumentCalled;

	// Tipo di Sqlrecord (usato come buffer di scambio) con relativa tabella sui quali il radar
	// deve operare.
	//SqlRecord*	m_pCallerSqlRecord;
	SqlTable*	m_pTable;
	SqlSession* m_pSqlSession; //session locale per lavorare in modalità disconnessa

	// Documento al quale il radar è collegato (*)
	//CAbstractFormDoc* m_pCallerDoc;

	// HotLink al quale il radar è collegato (*)
	//HotKeyLink*	m_pCallerHotLink;

	// RunTimeClass del documento e SqlRecord a cui il radar è collegato (*)
	CTBNamespace	m_CallerDocNamespace;
	//SqlRecord*		m_pDynamicSqlRecord;
	// NB: (*) mutuamente esclusivi

protected:
	// Membri utilizzati nella ricerca stupida
	UINT m_nTimerID;

	CQueryManager*		m_pQueryManager;	// gestisce le query del documento
	Scheduler*			m_pScheduler;
	RadarFindCondition*	m_pFindCond;

	// Indice dello stile di formattazione dall'interno della tabella degli stili di formattazione
	int			m_nIdxFontStyle;
	CString		m_strFontStyle;
	BOOL		m_bEnableSave;
    RadarBody*	m_pRadarBody;

	CString	m_strRadarFile;
	CString	m_strTempRadarFile;

protected:
	BOOL	m_bNextDirection;
	int		m_nFirstIdxSqlRec;
	BOOL	m_bFirstRun;

	

	RadarSqlRecCircleList*	m_pSqlRecCircleList;
	RadarColumnsIdxArray*	m_pDeletedColumns;
	RadarColumnsIdxArray*	m_pColsToInsert;

public:
	// Membri utilizzati nella ricerca stupida
	LONG m_nFindTic;
	BOOL m_bEditingQuery;
	BOOL m_bIsSaving;
	int	 m_nRowCount;
public:
	CRadarDoc	();
	~CRadarDoc	();

public:
	// Ritornano la view e la frame associate al documento
	CRadarView*		GetRadarView	();
	CRadarFrame*	GetRadarFrame	();

	BOOL			SaveModified	();
	void			CloseRadar	();
	ITBRadar*		GetRadarInterface () { return this; }

public:
	void	AddColumnGrpTable	();
	int		AddColumnGrpTable	(int nIdx, SqlRecord* pRec/* = NULL*/);

	void AddDelColumnGrpTableFromMouse	(const CPoint&);
	void AddDelColumnGrpTable			(int nCol);
	void AddDelColumnTailGrpTable		();

	void InvertCursorDirection			(BOOL bNextDirection = TRUE);

	void GoEnd	();
	void GoHome	(BOOL bUpdate = TRUE);

	void ChangeSelVisibleRow(int nRec, BOOL bRedraw);
	int  GetSizeCircleList	();
	LONG GetSqlRowActive    ();

	void UpdateNrMultiLines (int nVisibleRows);

public:
	RadarBody* GetRadarBody();

	void FindStupidReset	(const DataType&);
	void FindStupidDisable	();

	void SetWidthSelColumn	(int nWidth);
	void SetRunIndicator	();
	void ShowCorrectRows	();
	void ActivateAndShowRadar (BOOL bShow = TRUE);

	CBaseDocument* GetDocument () { return this; }

public:
	BOOL ChangeStayAlive	();
	void ActiveStayAlive	();

public:
	void Attach	(CAbstractFormDoc*);
	void Attach	(CTBNamespace aCallerDocNamespace, CRuntimeClass* pRecordClass);
	void Attach	(HotKeyLink*, SqlTable*, SqlRecord*);
	void Attach (CAbstractFormDoc*, BOOL) {}
	void Detach	();

	void OnRadarRecordSelected	();
	BOOL DblClkRecordSelect		(int nRec);

public:
	LOGFONT LoadDefaultFontStyle();
	void	FontStyleUpdate		(CString&);
	void	OnFontStyleChanged	();

	int		GetFormatIdx		(const DataType&);
	void	FormatStyleChanged	();
	void	MakeOrderBy			(BOOL bDesc = FALSE);

public:
	// Il primo controllo è fatto per vedere se è attiva una find stupida il secondo
	BOOL IsFindCondActive	();
	
	BOOL IsActiveFind		() { return m_nTimerID > 0;	}
	BOOL IsActiveLinkRadar	() ;
	BOOL IsFindNotActive	() { return m_nTimerID <= 0;}
	BOOL CanDoEnable		() ;
	BOOL CanDoLinkRecord	() { return IsActiveLinkRadar () && CanDoEditRecord	(); }
	BOOL CanDoFindNextPrev	() { return CanDoFindStart() && IsFindCondActive	(); }
	BOOL CanDoFindStop		() { return m_nTimerID > 0;	}
	BOOL CanDoChangeRowSel	() { return CanDoEnable	(); }
	BOOL CanDoEditRecord	() { return CanDoEnable	(); }
	BOOL CanDoMoveDown		() { return CanDoEnable	();	}
	BOOL CanDoMoveUp		() { return CanDoEnable	();	}
	BOOL CanDoRecordSelect	() { return CanDoEnable	();	}
	BOOL CanDoSaveGrpAttrib	() { return m_bEnableSave;	}
	BOOL CanDoSelOrDesCols	() { return IsFindNotActive	(); }
	BOOL ChangeLinkRadar	() { return (m_bLinkRadar = !m_bLinkRadar); }

	BOOL CanDoFindStart		();
	BOOL CanDoFindEdit		();
	BOOL CanDoDeleteCols	();
	BOOL CanDoInsertCols	();
	BOOL CanDoShiftColLeft	();
	BOOL CanDoShiftColRight	();
	BOOL CanDoExecQuery		();
	BOOL CanDoEditQuery		();
	BOOL CanDoOrderBy		();
	BOOL CanDoExport		();
	CString FormatDataForExport(DataObj* pCol);
	BOOL CanDoNewRecord		();
	BOOL CanDoExit			();

	void StartReady			();

	virtual BOOL OnPopulatedDropDown    (UINT nIdCommand);
	virtual void SetDefaultSelect(const CString& sQualifiedName);

	LRESULT FillAutoComple(WPARAM wp, LPARAM lp);

public:
	CString	GetEmptyTableCaption	();
	LPCTSTR	GetTableName			();
	CString	GetTableTitle			();
	BOOL	OpenTableAndCheckIsEmpty();
	
private:
	void NextRow (int nRec, WORD wCode);
	void PrevRow (int nRec, WORD wCode);
	void GoToEndRow	 ();
	void TopRow();

	BOOL CheckExistFontStyle (CString& strFontStyle, CString& szNewStyle, const CString& strErr);

	BOOL GetRadarProfileString	(LPCTSTR lpszSection, LPCTSTR lpszEntry, LPCTSTR lpszDefault, CString& strString);
	int  GetRadarProfileInt		(LPCTSTR lpszSection, LPCTSTR lpszEntry, int nDefault);
	BOOL WriteRadarProfileString(LPCTSTR lpszSection, LPCTSTR lpszEntry, LPCTSTR lpszString);
	BOOL WriteRadarProfileInt	(LPCTSTR lpszSection, LPCTSTR lpszEntry, int nValue);

	// find operation related members
	BOOL FindDataObjNext		();
	BOOL FindDataObjPrev		();
	BOOL FindNextLoadGrpTable	();
	BOOL FindNextRec			(int nRec);
	BOOL FindPrevLoadGrpTable	();
	BOOL FindPrevRec			(int nRec);
	BOOL FindStrNext			(CString&);
	BOOL FindStrPrev			(CString&);
	void KillFindTimer			(BOOL bDeleteScheduler = TRUE);
	void UserBreakFind			(BOOL bDeleteScheduler = TRUE);
	BOOL IsStringFind			(CString strSrc, CString& strFind);
	void StartFindStupid		();
	BOOL RestartSearch			();

	SqlRecord* TicGetSqlRecord		(int nRec);
	SqlRecord* TicInsertNextSqlRec	();
	SqlRecord* TicInsertPrevSqlRec	();

	void UpdateRadarView(WORD highCode);

private:
	typedef BOOL (*TestData)(DataObj*, DataObj*);
	TestData pfTestData;

	BOOL CreateCallerDoc();

	void StartRadar (SqlTable*, BOOL bUsePredefined = FALSE);

	// caricano i record alla tabella nell'array circolare e nella tabella grafica
	void AddRowGrpTable		(int nRec);
	void AddRowHeadGrpTable	(int nRec);
	void AddRowTailGrpTable	(int nRec);

	BOOL LoadCircleListNext	(int nNrRec, int& nRecEnd, BOOL bWaitCursor = TRUE);
	BOOL LoadCircleListPrev	(int nNrRec, int& nRecEnd);

	int  GetLastIdxSqlRec	();
	BOOL LoadNextRecFormat	(int& nNewActiveRec);
	void UpdateAllViewWithRec(int nNewActiveRec, WORD = 0);

	int	GetColumnIdxFindCond(int nIdx);
	int GetLastIdxGrpRow	();

	void RadarRecordSelected	(BOOL bAtcivateDoc = FALSE, BOOL bForceEditing = FALSE);
	BOOL CheckRadarFullFileName (BOOL bFromSave = FALSE );

	void OnFindStart();
	
protected:
	int GetIdxFirstSelCol					();
	// save and load the graphic			 attributes
	BOOL LoadGrpAttribFromFile				();
	void LoadFontStyle						(CString&);
	void ReadWindowPlacement				(LPCTSTR lpszSection);
	void SaveWindowPlacement				(CString&);
	void SaveGraphicAttrib					(BOOL bAskConfirm = FALSE);
	
	virtual BOOL CanPushToClient			() { return FALSE;}
	
private:
	BOOL LoadFromVirtualDriver	();
	BOOL SaveFromVirtualDriver	();
	BOOL DataObjAssign();

public:
	virtual	int		GetRadarColumnIndex		(const CString& sColumnName);
	virtual	BOOL	MoveRadarColumn			(int nFromIdxCol, int nToIdxCol);
	virtual BOOL	SetColumnTitle			(int nIdxCol, LPCTSTR szTitle);
	virtual BOOL	SetColumnAlign			(int nIdxCol, UINT align);	//DT_LEFT, DT_CENTER, DT_RIGHT
	virtual void	SetColumnScreenSize		(int nIdxCol, int width);
	virtual	BOOL	RemoveRadarColumn		(int nIdxCol);

	virtual void	ExportTableToExcel ();
	virtual void	ExportTableToODS ();

	virtual WebCommandType OnGetWebCommandType(UINT commandID);

protected:
	virtual	void ExportTable(TypeOfExport* pExp); //per evitare di includere <WrmEng\Extern.h>
	void ExecuteEditBoxQuery();

public:
	void GoRow		(int nRow);
	
	void RowDown	();
	void RowUp		();
	void PageDown	();
	void PageUp		();

	// Spostamento della riga attiva
	void PageEnd		();
	void PageHome		();

public:
	afx_msg void OnRowDown	();
	afx_msg void OnRowUp	();
	afx_msg void OnPageDown	();
	afx_msg void OnPageUp	();

	// Spostamento della riga attiva
	afx_msg	void OnEnd		();
	afx_msg	void OnHome		();

// Find Macro Method generate
	FIND_BAR_METHOD()

protected:
	// Reimplementano le chiamate standard della classe CAbstractDoc
	virtual void DeleteContents	() {}						// non deve fare niente
	virtual	BOOL OnNewDocument	();
	virtual	BOOL OnOpenDocument	(LPCTSTR) { return TRUE; }	// non deve fare niente
	virtual BOOL OnSaveDocument	(LPCTSTR) { return TRUE; }	// non deve fare niente 
	
	// Generated message map functions
	//{{AFX_MSG(CRadarDoc)
	afx_msg	void OnOrderByAsc	();
	afx_msg	void OnOrderByDesc	();

	// Modifiche grafiche della tabella
	afx_msg void OnDeleteCols	();
	afx_msg	void OnInsertCols	();
	afx_msg	void OnShiftColLeft	();
	afx_msg	void OnShiftColRight();
	afx_msg	void OnResizeColumns();

	// Eseguono la ricerca stupida	
	afx_msg	void OnFindNext	();
	afx_msg	void OnFindPrev	();
	afx_msg	void OnFindStartDlg();
	afx_msg	void OnFindStop	();

	// Modificano o inseriscono un nuovo record nell'oggetto collegato al radar
	afx_msg void OnEditRecord	();
	afx_msg void OnNewRecord	();

	afx_msg	void OnExecQuery	();
	afx_msg	void OnEditQuery	();
	afx_msg void OnEditBoxQuery ();
		
	// Funzioni di utilizzo generale
	afx_msg	void OnEscape		();
	afx_msg	void OnSaveGrpAttrib() { SaveGraphicAttrib(); }

	// Switch button
	afx_msg void OnTabSwitch();
	afx_msg void OnTabSwitchRange(UINT nID);

	// update commands ui
	afx_msg void OnUpdateNext			(CCmdUI* pCmd) { pCmd->Enable (CanDoFindStart   ());}
	afx_msg void OnUpdatePrev			(CCmdUI* pCmd) { pCmd->Enable (CanDoFindNextPrev());}
	afx_msg void OnUpdateFindStop		(CCmdUI* pCmd) { pCmd->Enable (CanDoFindStop	());}
	afx_msg void OnUpdateFindStart		(CCmdUI* pCmd) { pCmd->Enable (CanDoFindStart	());}
	afx_msg void OnUpdateFindEdit		(CCmdUI* pCmd) { pCmd->Enable (CanDoFindEdit	());}
	afx_msg void OnUpdateEditRecord		(CCmdUI* pCmd) { pCmd->Enable (CanDoEditRecord());}
	afx_msg void OnUpdateNewRecord		(CCmdUI* pCmd) { pCmd->Enable (CanDoNewRecord	());}
	afx_msg void OnUpdateHome			(CCmdUI* pCmd) { pCmd->Enable (CanDoMoveUp		());}
	afx_msg void OnUpdatePageUp			(CCmdUI* pCmd) { pCmd->Enable (CanDoMoveUp		());}
	afx_msg void OnUpdateRowUp			(CCmdUI* pCmd) { pCmd->Enable (CanDoMoveUp		());}
	afx_msg void OnUpdateRowDown		(CCmdUI* pCmd) { pCmd->Enable (CanDoMoveDown	());}
	afx_msg void OnUpdatePageDown		(CCmdUI* pCmd) { pCmd->Enable (CanDoMoveDown	());}
	afx_msg void OnUpdateEnd			(CCmdUI* pCmd) { pCmd->Enable (CanDoMoveDown	());}
	afx_msg void OnUpdateDeleteCols		(CCmdUI* pCmd) { pCmd->Enable (CanDoDeleteCols	());}
	afx_msg void OnUpdateInsertCols		(CCmdUI* pCmd) { pCmd->Enable (CanDoInsertCols	());}
	afx_msg void OnUpdateShiftColLeft	(CCmdUI* pCmd) { pCmd->Enable (CanDoShiftColLeft());}
	afx_msg void OnUpdateShiftColRight	(CCmdUI* pCmd) { pCmd->Enable (CanDoShiftColRight());}
	afx_msg void OnUpdateSaveGrpAttrib	(CCmdUI* pCmd) { pCmd->Enable (CanDoSaveGrpAttrib());}
	afx_msg void OnUpdateExecQuery		(CCmdUI* pCmd) { pCmd->Enable (CanDoExecQuery	());}
	afx_msg void OnUpdateEditQuery		(CCmdUI* pCmd) { pCmd->Enable (CanDoEditQuery	());}
	afx_msg void OnUpdateOrderBy		(CCmdUI* pCmd) { pCmd->Enable (CanDoOrderBy		());}
	afx_msg void OnUpdateTabSwitch		(CCmdUI* pCmd) { pCmd->Enable (TRUE); }

	afx_msg void OnUpdateExport			(CCmdUI* pCmdUI);
	

	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()

// Diagnostics
#ifdef _DEBUG
public:
	void Dump		(CDumpContext&)	const;
	void AssertValid()				const { CAbstractDoc::AssertValid(); }
#endif // _DEBUG
};

///////////////////////////////////////////////////////////////////////////////
///////////////////////////////////////////////////////////////////////////////
//
//	inline functions
//
///////////////////////////////////////////////////////////////////////////////
///////////////////////////////////////////////////////////////////////////////
inline int RadarSqlRecCircleList::GetMaxSize	() { return m_nNrMaxRecords;}
inline int RadarSqlRecCircleList::GetStartIdx	() { return m_nStartIdx;	}

//=============================================================================
inline void CRadarDoc::UpdateRadarView (WORD highCode) { UpdateAllViews (NULL, MAKELPARAM(0, highCode));}

#include "endh.dex"
