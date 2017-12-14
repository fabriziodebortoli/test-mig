
#pragma once

#include <TBGENLIB\basedoc.h>

#include <TBOLEDB\sqlrec.h>
#include <TBOLEDB\TbExtensionsInterface.h>

#include <TBGES\extdoc.h>
#include <TBGES\tabber.h>
#include <TBGES\xmlgesinfo.h>

#include <TBGenlib\OslInfo.h>
#include <TbGenlib\OslBaseInterface.h>

//includere alla fine degli include del .H
#include "beginh.dex"

class CXMLDataManager;
class CXMLExportCriteria;
class SqlRecord;
class SqlTable;
class QueryInfo;
class ProgramData;
class COSLExportCriteria;
class CTabManager;
class CXMLProfileInfo;
class CXMLExportCriteriaDoc;
class CXMLExportSelDocPage;
class CXMLExportSelProfilePage;
class CXMLVariableArray;
class CExportWizardPage;
class CXMLExportCriteriaPage;
class CXMLExportOSLCriteriaPage;
class CXMLExportSummaryPage;
class InputMng;
class CTabManager;
class CExpCriteriaWizardDoc;
class DBTMaster;


#define EXTRACT_RECORD_SUCCEEDED		0
#define EXTRACT_RECORD_NO_DATA			1
#define EXTRACT_RECORD_ERROR			2

#define XML_CRITERIA_DEFAULT		XML_CRITERIA_OSL | XML_CRITERIA_APP | XML_CRITERIA_USR

#define EXPORT_ONLY_CURR_DOC 0x0001
#define EXPORT_DOC_SET		 0x0002
#define EXPORT_ALL_DOCS		 0x0003

#define USE_PREDEFINED_PROFILE	 0x0001
#define USE_PREFERRED_PROFILE	 0x0002
#define USE_SELECTED_PROFILE	 0x0003

#define EXPORT_ONLY_DOC			 0x0001
#define EXPORT_ONLY_SCHEMA		 0x0002
#define EXPORT_DOC_AND_SCHEMA	 0x0003
#define EXPORT_SMART_SCHEMA		 0x0004


// classe contenente le informazioni impostate nella dialog CXMLExportDocDlg
/////////////////////////////////////////////////////////////////////////////
class CXMLExportDocSelection
{
public:
	CAbstractFormDoc*	m_pDocument;	
	int					m_nDocSelType;
	int					m_nProfileSelType;
	int					m_nSchemaSelType;
	DataBool			m_bSendEnvelopeNow;	
	DataBool			m_bCompressFile;
	DataBool			m_bUseAlternativePath;
	DataStr				m_strAlternativePath;
	DataStr				m_strProfileName;
	CXMLProfileInfo*	m_pCurrentProfile;
	CString				m_strExpCriteriaFileName; // serve per lo Scheduler
	CStringArray		m_aProfNamesArray;
	CString				m_strPreferredProfile;
	BOOL				m_bExistPredefined;

public:
	CXMLExportDocSelection	(CAbstractFormDoc*);
	CXMLExportDocSelection	(const CXMLExportDocSelection&);
	~CXMLExportDocSelection	();

private:
	void	Assign	(const CXMLExportDocSelection*);

public:
	BOOL				IsPredefinedProfileToUse	()	const	{ return m_nProfileSelType == USE_PREDEFINED_PROFILE; }
	BOOL				IsPreferredProfileToUse		()	const	{ return m_nProfileSelType == USE_PREFERRED_PROFILE; }
	BOOL				IsSelectedProfileToUse		()	const	{ return m_nProfileSelType == USE_SELECTED_PROFILE; }

	BOOL				IsOnlyCurrentDocToExport()	const	{ return m_nDocSelType == EXPORT_ONLY_CURR_DOC; }
	BOOL				AreAllDocumentsToExport	()	const	{ return m_nDocSelType == EXPORT_ALL_DOCS;	}
	BOOL				MustUseCriteria			()	const	{ return m_nDocSelType == EXPORT_DOC_SET; }

	BOOL				IsOnlyDocToExport		()	const	{ return m_nSchemaSelType == EXPORT_ONLY_DOC; }
	BOOL				IsOnlySchemaToExport	()	const	{ return m_nSchemaSelType == EXPORT_ONLY_SCHEMA;	}
	BOOL				IsDocAndSchemaToExport	()	const	{ return m_nSchemaSelType == EXPORT_DOC_AND_SCHEMA; }
	BOOL				IsSmartSchemaToExport	()	const	{ return m_nSchemaSelType == EXPORT_SMART_SCHEMA; }

	void				LoadAllProfile			();
	BOOL				AreSelProfilePresent	()	const	{ return m_aProfNamesArray.GetSize() > 0; }
	DataStr				GetProfileName			()	const	{ return m_strProfileName;		}
	CXMLProfileInfo*	GetProfileInfo			()	const	{ return m_pCurrentProfile;		}
	
	BOOL				SetCurrentProfileInfo	(LPCTSTR, CAutoExpressionMng* = NULL);

public: //operator
	CXMLExportDocSelection&	operator =	(const CXMLExportDocSelection&);

};										


//Classe contenente le informazioni per la gestione dei flag impostati dall'utente
/////////////////////////////////////////////////////////////////////////////
// Dichiarazione di CPreferencesCriteria
/////////////////////////////////////////////////////////////////////////////
// 
class CPreferencesCriteria
{
	friend CXMLExportCriteriaDoc;
	friend CXMLExportCriteriaPage;

public:
	enum PriorityMode	{ USE_OSL_QUERY, USE_APPLICATION_QUERY, USE_AUTOMATIC_QUERY };

private:
	// mi servono esclusivamente per la CBaseTabDialog CSetPreferencesCriteriaPage per
	// ricevere i valori impostati dall'utente

	//questi 3 flag indicano come comporre i criteri di selezione, non sono esclusivi
	DataBool	m_bSelModeOSL;	// criteri di selezione OSL
	DataBool	m_bSelModeApp;	// criteri di selezione predefiniti
	DataBool	m_bSelModeUsr;	// criteri di selezione personalizzati


public:
	DataStr				m_strEnvFileName;
	PriorityMode		m_nPriorityQuery;	

public:
	CPreferencesCriteria	(); 
	CPreferencesCriteria	(const CPreferencesCriteria&);

private:
	void Assign				(const CPreferencesCriteria*);

public:
	BOOL Parse				(CXMLNode*); 
	BOOL Unparse			(CXMLNode*);

public:
	DataStr	GetEnvFileName()			{ return m_strEnvFileName; }

	void SetCriteriaModeOSL(BOOL bSet)	{ m_bSelModeOSL = bSet; } 
	void SetCriteriaModeApp(BOOL bSet)	{ m_bSelModeApp = bSet; } 
	void SetCriteriaModeUsr(BOOL bSet)	{ m_bSelModeUsr = bSet; } 

	PriorityMode	GetPriorityMode			()	const	{ return m_nPriorityQuery;  }
	BOOL			IsPriorityModeOSL		()	const	{ return m_nPriorityQuery == USE_OSL_QUERY;			}
	BOOL			IsPriorityModeApp		()	const	{ return m_nPriorityQuery == USE_APPLICATION_QUERY;  }
	BOOL			IsPriorityModeAutomatic	()	const	{ return m_nPriorityQuery == USE_AUTOMATIC_QUERY;	}

	BOOL			IsCriteriaModeOSL		()	const	{ return m_bSelModeOSL; }
	BOOL			IsCriteriaModeApp		()	const	{ return m_bSelModeApp; }
	BOOL			IsCriteriaModeUser		()	const	{ return m_bSelModeUsr; }
	BOOL			IsCriteriaModeAppOrUser	()	const	{ return m_bSelModeApp || m_bSelModeUsr;}

public: //operator
	CPreferencesCriteria&	operator =	(const CPreferencesCriteria&);
	BOOL					operator ==	(const CPreferencesCriteria&)	const;
	BOOL					operator !=	(const CPreferencesCriteria&)	const;
};


// Classe contenente le informazioni dei criteri di selezione personalizzati dall'utente
/////////////////////////////////////////////////////////////////////////////
// Dichiarazione di CUserExportCriteria
//
/////////////////////////////////////////////////////////////////////////////
//
class CUserExportCriteria
{
public:
	QueryInfo*			m_pQueryInfo;
	SqlRecord*			m_pRecord;
	CString				m_strTableName;
	const SqlTableInfo*	m_pSqlTableInfo;
	CString				m_OldQueryString;
	BOOL				m_bCriteriaInit;
	BOOL				m_bOverrideDefaultQuery;

public:
	CUserExportCriteria	(CXMLExportCriteria*);
	CUserExportCriteria	(const CUserExportCriteria&);
	~CUserExportCriteria();

private:
	void	Assign					(const CUserExportCriteria*);

public:
	CString GetCurrentQueryString	() const;
	CSize	ExecAskRules			(CExportWizardPage*, InputMng*);
	BOOL	PrepareQuery			(SqlTable*);

	void	AttachVariables			(CXMLVariableArray* pVariablesArray);
	BOOL	ParseExp				(CXMLNode* pNode, CAutoExpressionMng* pAutoExpressionMng = NULL);
	BOOL	UnparseExp				(CXMLNode* pNode, CAutoExpressionMng* pAutoExpressionMng = NULL);

	BOOL	ParseUsr				(CXMLNode* pNode);
	BOOL	UnparseUsr				(CXMLNode* pNode);
	
	const SqlTableInfo* GetTableInfo() { return m_pSqlTableInfo; }

public: //operator
	CUserExportCriteria&	operator =	(const CUserExportCriteria&);
	BOOL					operator ==	(const CUserExportCriteria&)	const;
	BOOL					operator !=	(const CUserExportCriteria&)	const;
};


//
// Classe contenente le informazioni per estrarre un insieme di record da 
// esportare attraverso i criteri di selezione cablati dal programmatore

/////////////////////////////////////////////////////////////////////////////
//		Dichiarazione di CAppExportCriteria
/////////////////////////////////////////////////////////////////////////////
//
class CAppExportCriteria
{
	friend CXMLExportCriteria;

public:
	CAbstractFormDoc*		m_pDocument;
	SqlRecord*				m_pRecord;
	CXMLBaseAppCriteria*	m_pBaseExportCriteria;		

	BOOL					m_bOwnUsrCriteria;

public:
	CAppExportCriteria	(CXMLExportCriteria* =NULL);
	CAppExportCriteria	(const CAppExportCriteria&);
	~CAppExportCriteria	();

	
private:
	void Assign			(const CAppExportCriteria*);
	void DefineQuery	(SqlTable*);
	void PrepareQuery	(SqlTable*);
	
public:
	CXMLVariableArray*	GetVariablesArray			()			const	{ return m_pBaseExportCriteria ? m_pBaseExportCriteria->GetVariablesArray()			: NULL;}
	int					GetVariableArraySize		()			const	{ return GetVariablesArray() ? GetVariablesArray()->GetSize()	: 0;	}
	CXMLVariable*		GetVariable					(int nIdx)	const	{ return GetVariablesArray() ? GetVariablesArray()->GetAt(nIdx) : NULL;	}

	BOOL			CreateAppExpCriteriaTabDlgs	(CTabManager* pTabMng, int nPos) const { return m_pBaseExportCriteria ? m_pBaseExportCriteria->CreateAppExpCriteriaTabDlgs(pTabMng, nPos)	: NULL;	}
	UINT			GetFirstAppTabIDD			() const { return m_pBaseExportCriteria ? m_pBaseExportCriteria->GetFirstDialogIDD()			: 0;	}
	UINT			GetLastAppTabIDD			() const { return m_pBaseExportCriteria ? m_pBaseExportCriteria->GetLastDialogIDD()				: 0;	}

	
public:
	BOOL	Parse	(CXMLNode*, CAutoExpressionMng* = NULL);
	BOOL	Unparse	(CXMLNode*, CAutoExpressionMng* = NULL);

public: //operator
	CAppExportCriteria&	operator =	(const CAppExportCriteria&);
	BOOL				operator ==	(const CAppExportCriteria&)	const;
	BOOL				operator !=	(const CAppExportCriteria&)	const;
};


/////////////////////////////////////////////////////////////////////////////
// Dichiarazione di COSLExportCriteria
/////////////////////////////////////////////////////////////////////////////
// classe contente i settaggi dei flag per le scelte dei criteri da utilizzato
class COSLExportCriteria
{
	friend CExpCriteriaWizardDoc;
	friend CXMLExportOSLCriteriaPage;

private:
	DataDate	m_FromDate;
	DataDate	m_ToDate;
	DataBool	m_bInserted;
	DataBool	m_bUpdated;
	DataBool	m_bDeleted;
	
	CStringArray m_aColNameUKValue; //per i segmenti di UK
	const SqlCatalogEntry* m_pCatalogEntry; //é il catalog entry che ha il puntatore al gestore
									  // della tracciatura per la tabella legata al dbtmaster

public:
	COSLExportCriteria	(); 
	COSLExportCriteria	(const COSLExportCriteria&);
	~COSLExportCriteria	();

private:
	void Assign			(const COSLExportCriteria*);

public:
	BOOL	IsDeletedRecordsRequest() const { return m_bDeleted;}

	BOOL	IsUpdateInsertRecordsRequest() const;
	BOOL	SetCriteria			(CAbstractFormDoc*, BOOL = FALSE);
	void 	PrepareQuery		(SqlTable*);
	void 	PrepareDeletedQuery	(SqlTable*, DBTMaster*);



	void	AttachVariables	(CXMLVariableArray*);
public:
	BOOL	Parse				(CXMLNode*, CAutoExpressionMng* = NULL);
	BOOL	Unparse				(CXMLNode*, CAutoExpressionMng* = NULL);

public: //operator
	COSLExportCriteria&	operator =	(const COSLExportCriteria&);
	BOOL				operator ==	(const COSLExportCriteria&)	const;
	BOOL				operator !=	(const COSLExportCriteria&)	const;

};

/////////////////////////////////////////////////////////////////////////////
// Dichiarazione di CXMLExportCriteria
//
// Classe contenente le informazioni per estrarre un insieme di record da 
// esportare: si tratta di un'interrogazione complessa, che da un lato fa 
// capo alla tabella gestita dal DBT Master e dall'altro ai dati di Trace
// gestiti da OSL
class CXMLExportCriteria
{
private:
	BOOL					m_bOwnSqlRec;
	CString					m_strProfileName;
	CXMLProfileInfo*		m_pProfile;

public:
	CAbstractFormDoc*		m_pDoc;
	CString					m_strExpCriteriaFileName;		// nome del file .xml su cui rendere persistenti le info per sito
	CString					m_strXMLExpCriteriaSchemaFile;	// nome dello schema 
	CString					m_strUsrCriteriaFileName;		// nome del file .xml su cui rendere persistenti le info
	CPreferencesCriteria*	m_pPreferencesCriteria;
	CAppExportCriteria*		m_pAppExportCriteria;
	COSLExportCriteria*		m_pOSLExportCriteria;
	CUserExportCriteria*	m_pUserExportCriteria;

public:
	SqlTable*	m_pTable;
	SqlRecord*	m_pRecord;

public:
	CXMLExportCriteria(CXMLProfileInfo*, CAbstractFormDoc* = NULL);
	CXMLExportCriteria(const CXMLExportCriteria&);
	
	~CXMLExportCriteria();

private:
	void	Assign					(const CXMLExportCriteria*);
	void	Select					();
	void	MakeExportQuery			();

	void	RemoveVariablesInfo		(const CString& strExpFileName);
			
public:
	void	SetUserCriteria			(CUserExportCriteria*);
	void	SetExternalRecord		(SqlRecord*);
	int		ExportQuery				(CString& strErrorMsg);
	int		DeletedQuery			();
	int		GetNextRecord			();
	int		MakeFindableExportQuery	(CString& strErrorMsg);

	
public:
	//file dei dati relativi a sito
	BOOL	CreateXMLSchemaExpCriteriaFile	(LPCTSTR, BOOL = FALSE); 
	BOOL	ParseExpCriteriaFile			(const CString&, CString = _T(""), CAutoExpressionMng* = NULL);
	BOOL	UnparseExpCriteriaFile			(const CString&, CString = _T(""), CAutoExpressionMng* = NULL);

	//file dei dati di applicazione
	BOOL	ParseUsrCriteriaFile			(const CString&);
	BOOL	UnparseUsrCriteriaFile			(const CString& strUserFileName, const CString& strExpFileName);

public:
	CAbstractFormDoc*		GetDocument				()	{ return m_pDoc;}
	
	CPreferencesCriteria*	GetPreferencesCriteria	()	{ return m_pPreferencesCriteria;}	
	CAppExportCriteria*		GetAppExportCriteria	()	{ return m_pAppExportCriteria;	}
	COSLExportCriteria*		GetOSLExportCriteria	()	{ return m_pOSLExportCriteria;	}
	CUserExportCriteria*	GetUserExportCriteria	()	{ return m_pUserExportCriteria;	}
	CXMLProfileInfo*		GetProfile				()	{ return m_pProfile;}	
	CString					GetProfileName			()	{ return m_strProfileName; }


public: //operator
	CXMLExportCriteria&	operator =	(const CXMLExportCriteria&);
	BOOL				operator ==	(const CXMLExportCriteria&)	const;
	BOOL				operator !=	(const CXMLExportCriteria&)	const;
};


// per la creazione dello schema
//----------------------------------------------------------------

//----------------------------------------------------------------

#include "endh.dex"


