#pragma once

//includere alla fine degli include del .H
#include "beginh.dex"

extern TCHAR szReportExtension[];
extern TCHAR szWordDocumentExtension[];
extern TCHAR szWordTemplateExtension[];
extern TCHAR szExcelDocumentExtension[];
extern TCHAR szExcelTemplateExtension[];
extern TB_EXPORT TCHAR szTBJsonFileExt[];
extern TB_EXPORT TCHAR szBinFileExt[];

// Class for namespace type management
//=============================================================================
class CTBNamespaceType : public CObject
{ 
	DECLARE_DYNAMIC (CTBNamespaceType)

	friend class NSTypesTable;
	friend class CTBNamespace;

private:
	int		m_nType; 
	CString	m_sPublicName;
	BOOL	m_bHasLibrary;
	int		m_nFixedTokens;
	BOOL	m_bHasExtension;
	CString	m_sDefaultExtension;
	BOOL	m_bHasPathInside;
	CString	m_sFakeLibraryName;

public:
	CTBNamespaceType(
						const int&		nType, 
						const CString&	sPublicName, 
						const int		nFixedTokens		= -1,	  
						const BOOL		bHasLibrary			= TRUE,
						const BOOL		bHasExtension		= FALSE,
						const CString	sDefaultExtens		= _T(""),
						const BOOL		bHasPathInside		= FALSE,
						const CString	sFakeLibraryName	=  _T("DynamicDocuments")
					);

	//CTBNamespaceType(); 
};

// Class that defines all namespace types defined
//=============================================================================
class NSTypesTable : public CObArray
{
	friend class CTBNamespace;

public:
	NSTypesTable();
	virtual ~NSTypesTable();

private:
	const CTBNamespaceType* GetAt		(const CString& sName)	const;
	const CTBNamespaceType* GetTypeAt	(const int&	nType)		const;
};

// Class for namespace management
//=============================================================================
class TB_EXPORT CTBNamespace : public CObject
{     
	DECLARE_DYNAMIC (CTBNamespace)

	friend class CPathFinder;
	friend class CPadDoc;

public:
	enum NSObjectType 
		{ 
			NOT_VALID, 
			MODULE, 
			LIBRARY, 
			DOCUMENT, 
			REPORT, 
			DBT, 
			HOTLINK, 
			TABLE, 
			FUNCTION, 
			IMAGE, 
			TEXT, 
			FILE, 
			VIEW,
			FORM,
			TABBER,
			TABDLG,
			GRID,
			GRIDCOLUMN,
			CONTROL,
			EVENTHANDLER,
			PROCEDURE,
			DATAFILE,
			ALIAS,
			PROFILE,
			EXCELDOCUMENT,		// magic documents 
			EXCELTEMPLATE,	
			WORDDOCUMENT,
			WORDTEMPLATE,
			TOOLBARBUTTON,		// toolbar
			REPORTSCHEMA,		// only in c# platform
			DOCUMENTSCHEMA,
			VIRTUAL_TABLE,
			TOOLBAR,
			ENTITY,
			BEHAVIOUR,
			PDF,
			RTF,
			ODS,
			ODT,
			BARPANEL,
			TILEPANEL,
			TILEPANELTAB,
			JSON,
			ITEMSOURCE,
			HOTFILTER,
			VALIDATOR,
			DATA_ADAPTER,
			CONTEXT_MENU,
			CONTROL_BEHAVIOUR
		};

private:
	CString				m_sNamespace;

	// utility members
	static NSTypesTable		m_NSTypeTable;
	CStringArray			m_aTokens;
	const CTBNamespaceType*	m_pCurrentType;

public:
	CTBNamespace (const NSObjectType& aType = CTBNamespace::NOT_VALID);
	CTBNamespace (const NSObjectType& aType, const CString& strNamespace);
	CTBNamespace (const CTBNamespace& aNamespace);
	CTBNamespace (const CString& strNamespace);

public:
	// static members
	static CString		GetNotSupportedChars();
	static CString		GetSeparator		();

public:
	// not static members
	const CString& 		ToString			() const;
	const CString 		ToUnparsedString	() const;
	const NSObjectType	GetType				() const;
	const CString		GetTypeString		() const;
	const CString		GetApplicationName	() const;
	const CString		GetModuleName		() const;
	const CString		GetObjectName		() const;
	const CString		GetObjectName		(const NSObjectType& aType) const;
	CStringArray*		GetTokenArray		()  { return  &m_aTokens; }

	const CString		Left			(const NSObjectType& aType, const BOOL bIncludeType = FALSE) const;
	const CString		Right			(const NSObjectType& aType) const;
	const CString		GetRightTokens	(const int& nrOfTokens) const;

	const BOOL 			HasAFakeLibrary	() const;
		  BOOL 			IsSameModule	(const CTBNamespace&) const;

	// gets the tag name used in X-Tech xml documents. It substitute blanks with '-' char
	const CString		GetObjectNameForTag	() const;

	void SetType			(const NSObjectType& aType);
	void SetApplicationName	(const CString& sAppName);
	void SetModuleName		(const CString& sModuleName);
	void SetObjectName		(const NSObjectType& aType, const CString& sObjectName, BOOL bChangeType = FALSE, BOOL bSubstituteLastInVariableNamespace = FALSE);
	void SetObjectName		(const CString& sObjectName, BOOL bSubstituteLastInVariableNamespace = FALSE);
	void SetNamespace		(const CTBNamespace& aNamespace);
	void SetNamespace		(const CString& sNamespace);

	const BOOL IsValid			() const;
	const BOOL IsEmpty			() const;
	const BOOL IsFromTaskBuilder() const;

	void  Clear					() ;
	BOOL  AutoCompleteNamespace (const NSObjectType& aType, const CString& sPartialNS, const CTBNamespace& aSourceNS);
	BOOL  SetChildNamespace		(const NSObjectType& aType, const CString& sName, const CTBNamespace& aParentNamespace);

public:
	// operators
	BOOL			operator ==	(const CTBNamespace& aNS) const { return IsEqual(aNS); }
	BOOL			operator !=	(const CTBNamespace& aNS) const { return !IsEqual(aNS); }
	CTBNamespace&	operator =	(const CTBNamespace& aNS)		{ SetNamespace(aNS); return *this; }

private:
	void	ToStringArray	(const CString& sNamespace, CStringArray& aTokens) const;

	// utility private members
	const CString	GetToken		(const int& nToken) const;
	const  void		SetToken		(const int& nToken, const CString& sName);
	BOOL			IsEqual			(const CTBNamespace& aNS) const;

	// managed only by CPathFinder and 
	const CString	GetPathInside	() const;
	void			SetPathInside	(const CString& sPath);

// diagnostics
#ifdef _DEBUG
public:	
	void Dump(CDumpContext&) const;
#endif
};

// Class for namespace array
//=============================================================================
class TB_EXPORT CTBNamespaceArray : public CObArray
{
public:
	CTBNamespaceArray();
	virtual ~CTBNamespaceArray();

public:
	void	RemoveAt	(int nIndex, int nCount = 1);
	void	RemoveAll	();

	void	AddIfNotExists		(const CTBNamespace& aToAdd);

	CTBNamespace* 	GetAt		(int nIndex)const	{ return (CTBNamespace*) CObArray::GetAt(nIndex);	}
	CTBNamespace*&	ElementAt	(int nIndex)		{ return (CTBNamespace*&) CObArray::ElementAt(nIndex); }
	int				GetIndex	(const CTBNamespace&);
	
	CTBNamespace* 	operator[]	(int nIndex)const	{ return GetAt(nIndex);	}
	CTBNamespace*&	operator[]	(int nIndex)		{ return ElementAt(nIndex);	}
	
	CTBNamespaceArray& 	operator=	(const CTBNamespaceArray& aArray);

// diagnostics
#ifdef _DEBUG
public:	
	void Dump(CDumpContext&) const;
#endif
};

// macro to be used into DBT's derived class constructors (DBTMaster, DBTSlave,DBTSlaveBuffered, ...)
//-----------------------------------------------------------------------------
#define _NS_DBT(ns) _T(ns) 			// Dbts
#define _NS_LIB(ns) _T(ns) 			// Libraries
#define _NS_MOD(ns) _T(ns) 			// Modules
#define _NS_APP(ns) _T(ns) 			// Application
#define _NS_DOC(ns) _T(ns) 			// Documents
#define _NS_HKL(ns) _T(ns) 			// HotKeyLinks
#define _NS_IS(ns) _T(ns) 			// ItemSources
#define _NS_WEB(ns) _T(ns) 			// WebMethods
#define _NS_CD(ns) _T(ns)			// ClientDocuments
#define _NS_CD_BSP(ns) _T(ns)		// ClientDocuments dei BSP
#define _NS_ACT(ns) _T(ns)			// Activations
#define _NS_WRM(ns) _T(ns)			// Woorm Reports Variables
#define _NID(name) _T(name)			// Identifiers for Scripting syntax (addon to _NS_WRM)
#define _NS_DF(ns) _T(ns)			// DataFiles
#define _NS_DFEL(ns) _T(ns)			// DataFiles Element
#define _NS_FONT(ns) _T(ns)			// Fonts
#define _NS_FMT(ns) _T(ns) 			// Formatters
#define _NS_EH(ns) _T(ns)  			// Event Handler
#define _NS_TBL(ns) _T(ns) 			// Tables
#define _NS_FLD(ns) _T(ns) 			// Fields
#define _NS_VIEW(ns) _T(ns)			// FormView
#define _NS_BE(ns) _T(ns)			// BodyEdit
#define _NS_TABMNG(ns) _T(ns)		// TabManager
#define _NS_TABDLG(ns) _T(ns)		// TabDialog
#define _NS_TILEMNG(ns) _T(ns)		// TileManager
#define _NS_TILEGRP(ns) _T(ns)		// TileGroup
#define _NS_TILEDLG(ns) _T(ns)		// TileDialog
#define _NS_CLN(ns) _T(ns)			// AddColumn Names
#define _NS_LNK(ns) _T(ns)			// AddLink Names
#define _NS_WRMVAR(ns) _T(ns)		// Woorm Variables 
#define _NS_DLG(ns) _T(ns)			// Dialog
#define _NS_CTRL(ns) _T(ns)			// Dialog control
#define _NS_HF(ns) _T(ns)			// Hotfilter
#define _NS_VL(ns) _T(ns)			// Validator
#define _NS_DA(ns) _T(ns)			// DataAdapter
#define _NS_CB(ns) _T(ns)			// ControlBehaviour


//#define _NS_LFLD(ns) cwsprintf(_T("l_%s"), _T(ns))	// Local Fields
#define _NS_LFLD(ns) (CString(L"l_") + _T(ns))	// Local Fields

#define _NS_TOOLBARBTN(ns) _T(ns)	// Toolbar button namespace segment Name, use in AddButton
#define _NS_BEUSRBTN(ns) _T(ns)		// Bodyedit user button namespace segment Name, use in AddUserButton
#define _NS_IMG(ns) _T(ns)			// Image

//-----------------------------------------------------------------------------
#include "endh.dex"