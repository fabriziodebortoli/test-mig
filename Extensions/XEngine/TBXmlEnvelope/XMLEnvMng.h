#pragma once

#include "XMLEnvInfo.h"

//includere alla fine degli include del .H
#include "beginh.dex"

class CXMLEnvSiteElem;
class CXMLEnvClassElem;
class CAbstractFormDoc;

#define ENVELOPE_TO_IMPORT_EXIST		0
#define ENVELOPE_TO_IMPORT_NO_EXIST		1
#define ENVELOPE_TO_IMPORT_ERROR		2


// classe utilizzata per la generazione automatica dei nomi di file di export
/////////////////////////////////////////////////////////////////////////////
//		CXMLExpFileElem
/////////////////////////////////////////////////////////////////////////////
//
class TB_EXPORT CXMLExpFileElem : public CObject
{
public:
	CString	m_strDocTitle; // mi serve perchè lo stesso nome file può essere utilizzato da doc diversi
	CString m_strXMLBaseFileName;
	CString m_strXMLCurrFileName;
	int		m_nPadding;
	int		m_nMaxRecNumb; 
	int		m_nMaxKByte;
	int		m_nIncrementalKByte;	
	int		m_nCurrentKByte;	
	long	m_lCurrRecCount;
	long	m_lCurrNumbFile;
	long	m_lCurrBookmark;


public:
	CXMLExpFileElem(const CString& strTitle, CString& strXMLFileName, int nPadding, BOOL bConcat = FALSE);
	CXMLExpFileElem(const CXMLExpFileElem&);

private:
	void SetXMLCurrFileName();

public:
	void	SetMaxDim				(int nMaxRecNumb, int nMaxKByte);
	void	SetIncrementalKByte		(int, BOOL bComputeIncr =FALSE);

	void	IncrementExpRecordCount	()  { m_lCurrRecCount++; }
	long	GetNextBookmark			()	{ return ++m_lCurrBookmark;}

	BOOL	GetNextFileName			(CString&);

	BOOL	IsThisFileElem			(const CString& strTitle, const CString& strXMLFileName);

public://operator
	CXMLExpFileElem&		operator =	(const CXMLExpFileElem&);
};

/////////////////////////////////////////////////////////////////////////////
//		CXMLExpFileManager
/////////////////////////////////////////////////////////////////////////////
//
class TB_EXPORT CXMLExpFileManager 
{
public:
	Array m_XMLExpFileNames;

public:
	CXMLExpFileManager() {}
	CXMLExpFileManager(CXMLExpFileManager&);

private:
	BOOL IsUsedFileName(const CString& strTitle, const CString& strXMLFileName);

public:
	CXMLExpFileElem* GetXMLExpFileElem	(const CString& strTitle, const CString& strXMLFileName);
	int				 GetXMLExpFileIndex	(const CString& strTitle, const CString& strXMLFileName);

public:
	CXMLExpFileElem* GetNextFileName(const CString& strTitle, CString& strXMLFileName, int nPadding, BOOL& bNew);
	void	IncrementExpRecordCount	(CXMLExpFileElem*);
	long	GetNextBookmark			(const CString& strTitle, const CString& strXMLFileName);

public://operator
	CXMLExpFileManager&		operator =	(const CXMLExpFileManager&);
};


//---------------------------------------------------------------
// le tre classi successive mappano la seguente struttura
//RX
//	EnvClassName
//		Site
//			EnvName1			
//			EnvName2
//			EnvName3	


//----------------------------------------------------------------
//class CXMLEnvElem 
//----------------------------------------------------------------
//
class TB_EXPORT CXMLEnvElem : public CObject
{
	DECLARE_DYNAMIC(CXMLEnvElem)

public:
	CXMLEnvSiteElem		*m_pSiteAncestor;	//oggetto che contiene un array di questi oggetti
	CXMLEnvClassElem	*m_pClassAncestor;	//oggetto che contiene un array di questi oggetti
	CString				m_strEnvName;	// nome dell'envelope da mostrare all'utente
	CString				m_strEnvFileName; //path del file di envelope

public: 
	CXMLEnvElem(CXMLEnvSiteElem*  pAncestor, const CString& strEnvName, const CString& strEnvFileName);
	CXMLEnvElem(CXMLEnvClassElem* pAncestor, const CString& strEnvName, const CString& strEnvFileName);
	CXMLEnvElem(const CXMLEnvElem&);

public:	//operator
	CXMLEnvElem&		operator =	(const CXMLEnvElem&); 
};

//----------------------------------------------------------------
//class CXMLEnvElemArray 
//----------------------------------------------------------------
//
class TB_EXPORT CXMLEnvElemArray : public Array
{
public:
	CXMLEnvElemArray(){}
	CXMLEnvElemArray(const CXMLEnvElemArray&);

public:
	CXMLEnvElem*		GetAt		(int nIndex)	const	{ return (CXMLEnvElem*) Array::GetAt(nIndex);}
	CXMLEnvElem*&		ElementAt	(int nIndex)			{ return (CXMLEnvElem*&) Array::ElementAt(nIndex); }
	int					Add			(CXMLEnvElem* pEnvElem) { return Array::Add(pEnvElem); }

public:	//operator
	CXMLEnvElem*		operator[]	(int nIndex) const	{ return GetAt(nIndex);		}
	CXMLEnvElem*& 	operator[]	(int nIndex)		{ return ElementAt(nIndex);	}

	CXMLEnvElemArray&		operator =	(const CXMLEnvElemArray&); 
};

//----------------------------------------------------------------
//class CXMLEnvSiteElem 
//----------------------------------------------------------------
//
class TB_EXPORT CXMLEnvSiteElem : public CObject
{
	DECLARE_DYNAMIC(CXMLEnvSiteElem)

public:
	CXMLEnvClassElem*	m_pAncestor;
	CString				m_strSiteName;	
	CTBNamespace		m_NameSpace;	// per l'eventuale ricerca dei soli envelope relativi al documento 
										// su cui si sta effettuando l'importazione
	
	CXMLEnvElemArray*	m_pEnvElemArray;

public: 
	CXMLEnvSiteElem(CXMLEnvClassElem* pAncestor, const CString& strSiteName, const CTBNamespace& aNameSpace = CTBNamespace());
	CXMLEnvSiteElem(const CXMLEnvSiteElem&);
	~CXMLEnvSiteElem();

public:
//	int		AddEnvName	(LPCTSTR lpszEnvName, LPCTSTR lpszEnvFileName)	{ return m_pEnvElemArray->Add(new CXMLEnvElem(NULL, lpszEnvName, lpszEnvFileName)); }
	CString	GetEnvNameAt(int nIndex)			const { return m_pEnvElemArray->GetAt(nIndex)->m_strEnvName; }

	BOOL	LoadInfoFromPath	(LPCTSTR lpszRXEnvSitePath);
	BOOL	IsValidEnvelope		(const CString& strFilePath);

public: //operator
	CXMLEnvSiteElem&		operator =	(const CXMLEnvSiteElem&);

};


//----------------------------------------------------------------
//class CXMLEnvSiteArray 
//----------------------------------------------------------------
//
class TB_EXPORT CXMLEnvSiteArray : public Array
{
public:
	CXMLEnvSiteArray() {}

public:
	CXMLEnvSiteElem*		GetAt		(int nIndex)	const	{ return (CXMLEnvSiteElem*) Array::GetAt(nIndex);}
	CXMLEnvSiteElem*&		ElementAt	(int nIndex)			{ return (CXMLEnvSiteElem*&) Array::ElementAt(nIndex); }

	int						Add			(CXMLEnvSiteElem* pEnvSiteElem) { return Array::Add(pEnvSiteElem); }
	
	CXMLEnvSiteElem*		GetSiteByName(const CString& strSiteName) const;
	int						GetIndexByName(const CString& strSiteName) const;

public:	

public:	  //operator
	CXMLEnvSiteElem*		operator[]	(int nIndex) const	{ return GetAt(nIndex);		}
	CXMLEnvSiteElem*& 	operator[]	(int nIndex)		{ return ElementAt(nIndex);	}

};

//----------------------------------------------------------------
//class CXMLEnvClassElem 
//----------------------------------------------------------------
//
class TB_EXPORT CXMLEnvClassElem : public CObject
{
	DECLARE_DYNAMIC(CXMLEnvClassElem)

public:
	CString				m_strEnvClass;
	CXMLEnvSiteArray*	m_pEnvSiteArray;
	CXMLEnvElemArray*	m_pEnvElemArray;
	BOOL				m_bIsPending;
	BOOL				m_bContainsEnvelopes;

public:
	CXMLEnvClassElem(const CString& strEnvClass, BOOL bIsPending, BOOL bContainsEnvelopes);
	CXMLEnvClassElem(const CXMLEnvClassElem&);
	~CXMLEnvClassElem();

public:
	BOOL	LoadInfoFromPath	(LPCTSTR lpszRXEnvClassPath, const CTBNamespace& aNameSpace);

public: //operator
	CXMLEnvClassElem&	operator =	(const CXMLEnvClassElem&);

};


//----------------------------------------------------------------
//class CXMLEnvClassArray 
//----------------------------------------------------------------
//
class TB_EXPORT CXMLEnvClassArray : public Array
{
public:
	CString			m_strEnvClass;  // servono se voglio scaricare solo envelope di una determinata classe
	CTBNamespace	m_NameSpace;		// ed eventualmente relativi ad un determinato documento

public:
	CXMLEnvClassArray(const CString& strEnvClass = _T(""), const CTBNamespace& aNameSpace = CTBNamespace());
	CXMLEnvClassArray(const CXMLEnvClassArray&);

public:
	CXMLEnvClassElem*		GetAt		(int nIndex)	const	{ return (CXMLEnvClassElem*) Array::GetAt(nIndex);}
	CXMLEnvClassElem*&		ElementAt	(int nIndex)			{ return (CXMLEnvClassElem*&) Array::ElementAt(nIndex); }

	int						Add			(CXMLEnvClassElem* pEnvClassElem) { return Array::Add(pEnvClassElem); }
	
	CXMLEnvClassElem*		GetClassByName	(const CString& strClassName) const;
	int						GetIndexByName	(const CString& strClassName) const;

public:	
	void	Clear();
	BOOL	LoadInfoFromPath		(LPCTSTR lpszPath, BOOL bIsPending =FALSE, BOOL bContainsEnvelopes=FALSE);
	UINT	GetEnvelopeToImport		(CXMLEnvElemArray*, LPCTSTR pstrEnvFileName =NULL); // chiamato in fase di scheduler quando l'utente 
																							// non sceglie gli envelope da importare

public:	//operator
	CXMLEnvClassElem* 	operator[]	(int nIndex) const	{ return GetAt(nIndex);		}
	CXMLEnvClassElem*& 	operator[]	(int nIndex)		{ return ElementAt(nIndex);	}

	CXMLEnvClassArray&	operator =	(const CXMLEnvClassArray&);
};

//----------------------------------------------------------------
//class CXMLEnvelopeManager 
//----------------------------------------------------------------
class TB_EXPORT CXMLEnvelopeManager : public CCmdTarget
{
	DECLARE_DYNAMIC(CXMLEnvelopeManager)

private:

	//export
	CString				m_strTXEnvFolder;
	DWORD				m_dwTread; //numero del tread associato 
	//import
	CString				m_strRXEnvFolder;
	
	CString				m_strSenderSite;
	CString				m_strEnvClass;
	CString				m_strEnvFileName;
	CString				m_strEnvName;
	CString				m_strSite;

	DWORD				m_dwGetEnvThreadID;

public:
	CXMLEnvelopeInfo	m_aXMLEnvInfo;	
	CXMLExpFileManager*	m_pXMLExpFileManager;
	CXMLEnvClassArray*	m_pXMLRXEnvClasses;
	CAbstractFormDoc*	m_pDocument;
	
public:
	CXMLEnvelopeManager	(CAbstractFormDoc*);	
	CXMLEnvelopeManager	(const CXMLEnvelopeManager&);	
	~CXMLEnvelopeManager();

private:
	CString	CreateEnvName();
	BOOL GetInfoFromEnvelope(const CString& strEnvFileName, CString& m_strEnvClass, CString& m_strSenderSite, CTBNamespace& aNameSpace);

public:

	BOOL	ReadEnvelope			(CXMLDocumentObject* = NULL);
	BOOL	CreateEnvelope			(BOOL bDisplayMsgBox = TRUE, const CString& strPath = _T(""), BOOL bCreateSchema=FALSE);
	BOOL	DropEnvelope			(const CString& strPath = _T(""));
	UINT	GetEnvelopeToImport		(CXMLEnvElemArray*, LPCTSTR pstrEnvFileName = NULL);

	BOOL	LoadRXEnvClassArray		(LPCTSTR lpszEnvClass, const CTBNamespace& aNameSpace);
	BOOL	LoadPendingEnvClassArray(LPCTSTR lpszEnvClass, const CTBNamespace& aNameSpace);
	BOOL	LoadBothEnvClassArray	(LPCTSTR lpszEnvClass, const CTBNamespace& aNameSpace);
	BOOL	FillSelection			(CXMLEnvElemArray* pRXSelectedElems, LPCTSTR strEnvFolder, CAbstractFormDoc* pDoc);
	void	MoveEnvelopesToRXPath	();
	void	MoveEnvelopesToRXPath	(const CString &strFilePath);

public:
	CXMLExpFileElem* GetXMLExpFileName (const CString& strTitle, CString& strXMLFileName, int nPadding, BOOL& bNew);
	
	void	IncrementExpRecordCount	(CXMLExpFileElem*, const CString&  strXMLFileName, int nDataInstancesNumb);
	long	GetNextBookmark			(const CString& strTitle, const CString& strXMLFileName);

	void	SetRootDocNameSpace		(const CTBNamespace& aNameSpace)	{m_aXMLEnvInfo.SetRootDocNameSpace(aNameSpace);} 
	void	SetDescription			(const CString& strDescri)			{m_aXMLEnvInfo.SetDescription(strDescri);} 

	BOOL	SetTXEnvFolderPath		(LPCTSTR lpszEnvClass, LPCTSTR lpszAlternativePath,const CTBNamespace& nsDocument);
	void	SetRXEnvFolderPath		(LPCTSTR lpszPath);
	
	void	SetEnvName				(const CString& strEnvName) {m_strEnvName = strEnvName;}
	void	SetEnvClass				(const CString& strEnvClass) {m_strEnvClass = strEnvClass;}
	void	SetSenderSite			(const CString& strSenderSite) {m_strSenderSite = strSenderSite;}
	void	SetSite					(const CString& strSite) {m_strSite = strSite;}
	
	CString CreateFullEnvFileName	(const CString& strEnvFolder);

	CString	GetEnvName				()	const {return m_strEnvName;}
	CString	GetEnvFileName			()	const {return m_strEnvFileName;}
	CString	GetEnvClass				()	const {return m_strEnvClass;}
	CString	GetSenderSite			()	const {return m_strSenderSite;}
	CString	GetSite					()	const {return m_strSite;}
	
	CString	GetTXEnvFolderPath			(BOOL bCreate = TRUE)	const;
	CString	GetTXEnvFolderDataPath		(BOOL bCreate = TRUE)	const;
	CString	GetTXEnvFolderSchemaPath	(BOOL bCreate = TRUE)	const;
	CString	GetTXEnvFolderLoggingPath	(BOOL bCreate = TRUE)	const;

	CString	GetRXEnvFolderPath			(BOOL bCreate = TRUE)	const;
	CString	GetRXEnvFolderDataPath		(BOOL bCreate = TRUE)	const;
	CString	GetRXEnvFolderSchemaPath	(BOOL bCreate = TRUE)	const;
	CString	GetRXEnvFolderLoggingPath	(BOOL bCreate = TRUE)	const;
	CString	GetRXEnvClassPath			(BOOL bCreate = TRUE)	const;
	CString	GetRXSenderSitePath			(BOOL bCreate = TRUE)	const;
	
	CString	GetFailureEnvClassPath			(BOOL bIsForImport = TRUE, BOOL bCreate = TRUE)	const;
	CString	GetFailureSenderSitePath		(BOOL bIsForImport = TRUE, BOOL bCreate = TRUE)	const;
	CString	GetFailureEnvFolderPath			(BOOL bIsForImport = TRUE, BOOL bCreate = TRUE)	const;
	CString	GetFailureEnvFolderDataPath		(BOOL bIsForImport = TRUE, BOOL bCreate = TRUE)	const;
	CString	GetFailureEnvFolderSchemaPath	(BOOL bIsForImport = TRUE, BOOL bCreate = TRUE)	const;
	CString	GetFailureEnvFolderLoggingPath	(BOOL bIsForImport = TRUE, BOOL bCreate = TRUE)												const;

	CString	GetPendingEnvClassPath			(BOOL bIsForImport = TRUE, BOOL bCreate = TRUE)	const;
	CString	GetPendingSenderSitePath		(BOOL bIsForImport = TRUE, BOOL bCreate = TRUE)	const;
	CString	GetPendingEnvFolderPath			(BOOL bIsForImport = TRUE, BOOL bCreate = TRUE)	const;
	CString	GetPendingEnvFolderDataPath		(BOOL bIsForImport = TRUE, BOOL bCreate = TRUE)	const;
	CString	GetPendingEnvFolderSchemaPath	(BOOL bIsForImport = TRUE, BOOL bCreate = TRUE)	const;
	CString	GetPendingEnvFolderLoggingPath	(BOOL bIsForImport = TRUE, BOOL bCreate = TRUE)												const;

	CString	GetSuccessEnvClassPath			(BOOL bIsForImport = TRUE, BOOL bCreate = TRUE)	const;
	CString	GetSuccessSenderSitePath		(BOOL bIsForImport = TRUE, BOOL bCreate = TRUE)	const;
	CString	GetSuccessEnvFolderPath			(BOOL bIsForImport = TRUE, BOOL bCreate = TRUE)	const;
	CString	GetSuccessEnvFolderDataPath		(BOOL bIsForImport = TRUE, BOOL bCreate = TRUE)	const;
	CString	GetSuccessEnvFolderSchemaPath	(BOOL bIsForImport = TRUE, BOOL bCreate = TRUE)	const;
	CString	GetSuccessEnvFolderLoggingPath	(BOOL bIsForImport = TRUE, BOOL bCreate = TRUE)												const;

	CString	GetPartialSuccessEnvClassPath			(BOOL bIsForImport = TRUE, BOOL bCreate = TRUE)	const;
	CString	GetPartialSuccessSenderSitePath			(BOOL bIsForImport = TRUE, BOOL bCreate = TRUE)	const;
	CString	GetPartialSuccessEnvFolderPath			(BOOL bIsForImport = TRUE, BOOL bCreate = TRUE)	const;
	CString	GetPartialSuccessEnvFolderDataPath		(BOOL bIsForImport = TRUE, BOOL bCreate = TRUE)	const;
	CString	GetPartialSuccessEnvFolderSchemaPath	(BOOL bIsForImport = TRUE, BOOL bCreate = TRUE)	const;
	CString	GetPartialSuccessEnvFolderLoggingPath	(BOOL bIsForImport = TRUE, BOOL bCreate = TRUE)												const;

public:
	CXMLEnvContentsArray*	GetEnvContensArray()			{ return &m_aXMLEnvInfo.m_aEnvContents; }
	CXMLEnvFile*			GetEnvFileAt	(int i)	const 	{ return m_aXMLEnvInfo.GetEnvFileAt(i); }
	
	int		AddEnvFile 
				(
					CXMLEnvFile::ContentFileType	eFileType, 
					LPCTSTR							lpszUrlDati, 
					LPCTSTR							lpszProfile = NULL, 
					LPCTSTR							lpszEnvClass= NULL, 
					LPCTSTR							lpszDocName	= NULL, 
					int								nDocNum		= 0
				)
				{ return m_aXMLEnvInfo.AddEnvFile(eFileType, lpszUrlDati, lpszProfile, lpszEnvClass, lpszDocName, nDocNum); }
	

	BOOL	IsRootFilePresent() const { return m_aXMLEnvInfo.IsRootFilePresent(); }
			
public:
	CXMLEnvelopeManager& operator =(const CXMLEnvelopeManager&);

	//{{AFX_MSG(CXMLEnvelopeManager)
	//}}AFX_MSG	
	DECLARE_MESSAGE_MAP()

	// Generated OLE dispatch map functions
	//{{AFX_DISPATCH(CXMLEnvelopeManager)
	//}}AFX_DISPATCH
};

#include "endh.dex"

