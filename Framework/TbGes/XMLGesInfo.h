
#pragma once

#include <TbXmlCore\xmlgeneric.h>
#include <TbXmlCore\xmlparser.h>

#include <TbGeneric\Array.h>
#include <TbGeneric\DataObj.h>
#include <TbGeneric\DocumentObjectsInfo.h>

#include <TbGenlib\generic.h>
#include <TbGenlib\parsobj.h>
#include <TbGenlib\AutoExpressionMng.h>
#include <TbGenlib\TbTreeCtrl.h>

#include "extdocview.h"

//includere alla fine degli include del .H
#include "beginh.dex"

#define XML_TYPE_TAG							_T("Type")
#define XML_MAXDOC_TAG							_T("MaxDocuments")
#define XML_MAX_DIMENSION_TAG					_T("MaxDimension")
#define	XML_CHOOSEUPDATE_TAG					_T("ChooseUpdate")
#define XML_POSTABLE_TAG						_T("Postable")
#define XML_POSTBACK_TAG						_T("PostBack")
#define XML_NOEXTREF_POSTBACK_TAG				_T("NoExtRefPostBack")
#define XML_FULLPREPARE_TAG						_T("FullPrepare")
#define XML_VERSION_TAG							_T("Version")
#define XML_APP_VERSION_TAG						_T("AppVersion")
#define XML_DATA_URL_TAG						_T("DataUrl")
#define XML_ONLY_BO_TAG							_T("OnlyBusinessObject")


//transform document
#define	XML_TRANSFORM_XSLT_NAME_TAG				_T("XSLTName")

#define XML_FOREIGN_KEYSEG_TAG					_T("Foreign")
#define XML_PRIMARY_KEYSEG_TAG					_T("Primary")
#define XML_EXPORT_TAG							_T("Export")
#define XML_MUST_EXIST_TAG						_T("MustExist")
#define XML_XREF_SUBJECT_TO_TAG					_T("SubjectTo")
#define XML_XREF_EXPRESSION_TAG					_T("Expression")
#define XML_NULL_ALLOWED_TAG					_T("NullAllowed")
#define XML_LOG_URL_TAG							_T("LogUrl")
#define XML_PROFILE_NAME_TAG					_T("ProfileName")
#define XML_NAMESPACE_DOC_TAG					_T("DocumentNamespace")
#define XML_KEYS_TAG							_T("Keys")


#define XML_NO_DOC_QUERY_ATTRIBUTE				_T("nodocquery")
#define XML_ENVELOPE_CLASS_EXT_ATTRIBUTE		_T("extension")

#define XML_DBT_TYPE_MASTER_TAG					_T("Master")
#define XML_DBT_TYPE_SLAVE_TAG					_T("Slave")
#define XML_DBT_TYPE_SLAVES_TAG					_T("Slaves")
#define XML_DBT_TYPE_BUFFERED_TAG				_T("SlaveBuffered")
#define XML_DBT_TYPE_SLAVABLE_TAG				_T("Slavable")
#define XML_DBT_TYPE_UNDEF						_T("Undefined")

#define XML_DBT_UPDATETYPE_TAG					_T("UpdateType")
#define XML_DBT_UPDATE_REPLACE_TAG				_T("Replace")
#define XML_DBT_UPDATE_INSERTUPDATE_TAG			_T("InsertUpdate")	
#define XML_DBT_UPDATE_ONLYINSERT_TAG			_T("OnlyInsert")
											
#define XML_MAIN_EXTERNAL_REFERENCES_TAG		_T("MainExternalReferences")
#define XML_EXTERNAL_REFERENCES_TAG				_T("ExternalReferences")
#define XML_EXTERNAL_REFERENCE_TAG				_T("ExternalReference")
#define XML_DBTS_TAG							_T("DBTs")
#define XML_DBT_TAG								_T("DBT")
#define XML_KEY_SEGMENT_TAG						_T("KeySegment")
#define XML_SLAVES_TAG							_T("Slaves")
#define XML_SELECTIONS_TAG						_T("Selections")
#define XML_APPENDED_EXTERNAL_REFERENCES_TAG	_T("AppendedExternalReferences")
#define XML_APPENDED_EXTERNAL_REFERENCE_TAG		_T("AppendedExternalReference")
											
#define XML_SELECTIONS_TAG						_T("Selections")
#define XML_SEL_ASK_EXPORT_RULES				_T"AskExportRules")
#define XML_SEL_EXT_REF_QRY						_T("ExtRefQuery")
#define XML_SEL_WHERE							_T("Where")
#define XML_SEL_ORDER_BY						_T("OrderBy")
#define XML_SEL_SCHEMA							_T("x-schema:")
#define XML_SEL_SCHEMA_PATH						_T("XMLSchema\\")
#define XML_SEL_XMLNS							_T("xmlns")
#define XML_SEL_SITE							_T("Site")
													
#define XML_DEFAULT_ROOT						_T("Defaults")
#define XML_DEFAULT_PROFILE_TAG					_T("Profile")
#define XML_PREFERRED_PROFILE_TAG				_T("Preferred")

#define XML_FIELD_ROOT_TAG						_T("DBTS")
#define XML_FIELD_DBT_TAG						_T("DBT")
#define XML_FIELD_FIELDS_TAG					_T("Fields")
#define XML_FIELD_TAG							_T("Field")
#define XML_FIELD_NAME_TAG						_T("name")
#define XML_FIELD_EXPORT_TAG					_T("export")

#define XML_UNIVERSAL_KEYS_TAG					_T("UniversalKeys")
#define XML_UNIVERSAL_KEY_TAG					_T("UniversalKey")
#define XML_UNIVERSAL_KEY_FUNC_NAME_TAG			_T("FunctionName")
#define XML_UNIVERSAL_KEY_SEGMENT_TAG			_T("Segment")
#define XML_UNIVERSAL_KEY_FUNCTIONS_TAG			_T("Functions")

#define XML_UNIVERSAL_KEY_FUNCNAME_ATTRIBUTE	_T("functionname")
#define XML_UNIVERSAL_KEY_TABLENAME_ATTRIBUTE	_T("tablename")
#define XML_UNIVERSAL_KEY_EXPORT_ATTRIBUTE		_T("export")
#define XML_UNIVERSAL_KEY_SEGNAME_ATTRIBUTE		_T("name")
#define XML_UNIVERSAL_KEY_NAME_ATTRIBUTE		_T("ukname")	

#define XML_FIXED_KEYS_TAG						_T("FixedKeys")
#define XML_FIXED_SEG_TAG						_T("Segment")
#define XML_FIXED_SEG_NAME_ATTRIBUTE			_T("name")
#define XML_FIXED_SEG_VALUE_ATTRIBUTE			_T("value")


#define XML_HKL_ROOT_TAG						_T("DBTHKLs")
#define XML_HKL_DBT_TAG							_T("DBT")
#define XML_HKLS_TAG							_T("HotKeyLinks")
#define XML_HKL_FIELD_TAG						_T("Field")
#define XML_HKL_NAME_ATTRIBUTE					_T("name")
#define XML_HKL_NS_REPORT_ATTRIBUTE				_T("report")
#define XML_HKL_FIELDTYPE_ATTRIBUTE				_T("fieldType")
#define XML_HKL_SUBTYPE_ATTRIBUTE				_T("subType")
#define XML_HKL_TEXTBOXFILTER_TAG				_T("TextBoxFilter")
#define XML_HKL_LBDESCRIPTION_TAG				_T("LBDescription")
#define XML_HKL_IMAGE_TAG						_T("Image")
#define XML_HKL_PREVIEWS_TAG					_T("Previews")
#define XML_HKL_PREVIEW_TAG						_T("Preview")
#define XML_HKL_FILTERS_TAG						_T("Filters")
#define XML_HKL_FILTER_TAG						_T("Filter")
#define XML_HKL_RESULTS_TAG						_T("Results")
#define XML_HKL_RESULT_TAG						_T("Result")
#define XML_HKL_REPFIELD_ATTRIBUTE				_T("reportField")
#define XML_HKL_DOCFIELD_ATTRIBUTE				_T("documentField")
#define XML_HKL_FROMXREF_ATTRIBUTE				_T("fromXRef")
#define XML_ROW_TAG								_T("Row")

#define XML_AUTO_EXPR_EXPR_ATTRIBUTE			_T("expression")

//Business constraints
#define XML_SEARCH_BOOKMARKS_TAG					_T("SearchBookmarks")
#define XML_SEARCH_BOOKMARK_BOOK_TAG				_T("Bookmark")
#define XML_SEARCH_BOOKMARK_BOOK_NAME_ATTRIBUTE		_T("name")
#define XML_SEARCH_BOOKMARK_AS_DESCRI_ATTRIBUTE		_T("showasdescription")
#define XML_SEARCH_BOOKMARKS_HKLNAME_ATTRIBUTE		_T("hklname")
#define XML_SEARCH_BOOKMARKS_VERSION_ATTRIBUTE		_T("version")
#define XML_SEARCH_BOOKMARKS_KEYCODE_ATTRIBUTE		_T("keycode")


class CXMLNode;
class CXMLXRefInfo;
class DataObjArray;
class CAbstractFormDoc;
class SqlTable;
class CTabManager;
class AddOnModule;
class CXMLDocInfo;
class CXMLDocModInfo;
class ControlLinks;
class SymTableEx;
class CTabManager;
class CXMLVariableArray;
class CClientDoc;
class DBTSlave;
class DBTObject;
class CXMLDBTInfo;

// macro utilizzata per dichiarare le variabili data member nel metodo OnDeclareVariables
// della classe CXMLBaseAppCriteria

//dimensione in KByte dei file dei doc
#define	HEADER_MAX_DOC_DIMENSION		4096
#define	HEADER_MIN_DOC_DIMENSION		1
#define	HEADER_DEFAULT_DOC_DIMENSION	100

//numero max di documenti da esportare in un file
#define HEADER_MAX_DOCUMENT_NUM			100
#define HEADER_MIN_DOCUMENT_NUM			1
#define HEADER_DEFAULT_DOCUMENT_NUM		10

#define	HEADER_ENV_CLASS_SEPARATOR		_T("_")

#define NS_CMP_IDENTICAL			0
#define NS_CMP_NOT_FOUND			1
#define NS_CMP_DIFFERENT			2

TB_EXPORT UINT CompareNamespaceTagValue(CXMLNode* pNode, LPCTSTR lpszNamespace);
TB_EXPORT UINT CompareNamespaceTagValue(CXMLNode* pNode, const CTBNamespace& ns);
TB_EXPORT BOOL CompareNamespaceAttributeValue(CXMLNode* pNode, LPCTSTR lpszNamespace);
TB_EXPORT BOOL CompareNamespaceAttributeValue(CXMLNode* pNode, const CTBNamespace& ns);

//----------------------------------------------------------------
//class CXMLDefaultInfo  
//----------------------------------------------------------------
class TB_EXPORT CXMLDefaultInfo : public CObject
{
	DECLARE_DYNAMIC(CXMLDefaultInfo)

private:
	CTBNamespace	m_nsDoc;
	CString			m_strFileName;
	CString			m_strPrefProfile;
	BOOL			m_bIsLoaded;

public:
	CXMLDefaultInfo(CXMLDocInfo* = NULL);
	CXMLDefaultInfo(const CTBNamespace&);
	CXMLDefaultInfo(const CXMLDefaultInfo&);

private:
	BOOL	IsEqual	(const CXMLDefaultInfo&) const;

public:
	BOOL		Parse		();
	BOOL		Parse		(CPathFinder::PosType ePosType, const CString& strUserRole = _T(""));
	BOOL		UnParse		();
	BOOL		UnParse		(CPathFinder::PosType ePosType, const CString& strUserRole = _T(""));
	void		Clear		();

public:
	void			SetDocumentNamespace (const CTBNamespace&);
	CTBNamespace	GetDocumentNamespace () const { return m_nsDoc; }
	const CString&	GetPreferredProfile	 () const { return m_strPrefProfile; }
	BOOL			IsLoaded			 () const { return m_bIsLoaded; }
	CString			GetFileName			 () const { return m_strFileName; }

	BOOL	SetPreferredProfile	(const CString &);
	void	SetFileName			();

public: //operator
	CXMLDefaultInfo&	operator =	(const CXMLDefaultInfo&);
	BOOL				operator ==	(const CXMLDefaultInfo&)	const;
	BOOL				operator !=	(const CXMLDefaultInfo&)	const;

};

//////////////////////////////////////////////////////////////////////////////
//					CXMLAppCriteriaDlgElem definition
//////////////////////////////////////////////////////////////////////////////
//
class TB_EXPORT CXMLAppCriteriaDlgElem : public CObject
{
	friend class CXMLAppCriteriaDlgArray;

	DECLARE_DYNAMIC(CXMLAppCriteriaDlgElem);

private:
	CRuntimeClass*		m_pAppDlgRuntimeClass;
	UINT				m_nIDD;

public:
	CXMLAppCriteriaDlgElem(CRuntimeClass*, UINT);
	CXMLAppCriteriaDlgElem(const CXMLAppCriteriaDlgElem&);

private:
	BOOL IsEqual(const CXMLAppCriteriaDlgElem& aXMLCriteria) const;

protected:
	void Assign	(const CXMLAppCriteriaDlgElem&);

public:
	CRuntimeClass*	GetRTDialogClass()		const { return m_pAppDlgRuntimeClass;	}
	UINT			GetIDD()				const { return m_nIDD;					} 

public:
	BOOL					operator ==	(const CXMLAppCriteriaDlgElem& aXMLDlg)	const { return IsEqual	(aXMLDlg); }
	BOOL					operator !=	(const CXMLAppCriteriaDlgElem& aXMLDlg)	const { return !IsEqual	(aXMLDlg); }

	CXMLAppCriteriaDlgElem&	operator =	(const CXMLAppCriteriaDlgElem&);
};

//////////////////////////////////////////////////////////////////////////////
//					CXMLAppCriteriaDlgArray definition
//////////////////////////////////////////////////////////////////////////////
//
class TB_EXPORT CXMLAppCriteriaDlgArray : public Array
{
	DECLARE_DYNCREATE (CXMLAppCriteriaDlgArray);

public:
	CXMLAppCriteriaDlgArray () {}
	CXMLAppCriteriaDlgArray (const CXMLAppCriteriaDlgArray& a) { *this = a; }

	CXMLAppCriteriaDlgElem* GetAt		(int nIndex) const					{ return (CXMLAppCriteriaDlgElem*) Array::GetAt(nIndex);		}
	
	int						Add		(CXMLAppCriteriaDlgElem* pVar)			{ return Array::Add(pVar); }
	int						Add		(CRuntimeClass* pRTClass, UINT nIDD)		{ return Array::Add(new CXMLAppCriteriaDlgElem(pRTClass, nIDD)); }

public:
	BOOL	IsEqual			(const CXMLAppCriteriaDlgArray&) const;

public:	
	CXMLAppCriteriaDlgElem* 	operator[]	(int nIndex) const	{ return GetAt(nIndex);		}

	BOOL						operator== (const CXMLAppCriteriaDlgArray& aDlgArray) const { return IsEqual	(aDlgArray); }
	BOOL						operator!= (const CXMLAppCriteriaDlgArray& aDlgArray) const { return !IsEqual	(aDlgArray); }

	CXMLAppCriteriaDlgArray&	operator= (const CXMLAppCriteriaDlgArray& aDlgArray);	
};

// classe base da cui derivare i criteri di selezione cablati programmativamente
//----------------------------------------------------------------
//class CXMLBaseAppCriteria
//----------------------------------------------------------------
//===========================================================================
class TB_EXPORT CXMLBaseAppCriteria : public CObject
{
	DECLARE_DYNCREATE(CXMLBaseAppCriteria)

private:
	CXMLAppCriteriaDlgArray*	m_pXMLAppCriteriaDlgArray;	// array di coppie IDD - CRuntimeClass delle 
															// tabdialog contenenti i criteri di selezione applicativi

public:
	CBaseDocument*			m_pDocument;
	CXMLVariableArray*	m_pVariablesArray;

public:
	DataBool	m_bSelected;
	DataStr		m_FromKey;
	DataStr		m_ToKey;
	
public:
	CXMLBaseAppCriteria				();
	~CXMLBaseAppCriteria				();

private:
	BOOL	IsEqual						(const CXMLBaseAppCriteria& aBaseCriteria) const;

public:
	void	DeclareVariables			();	
	void	SetKeyLength				(int nNewLength);
	void	AttachDocument				(CBaseDocument*);
	void	Customize					();

	UINT	GetFirstDialogIDD			();
	UINT	GetLastDialogIDD			();
	void	AddAppCriteriaTabDlg		(CRuntimeClass*, UINT);
	BOOL	CreateAppExpCriteriaTabDlgs	(CTabManager*, int);
	
public: //inline
	CXMLVariableArray*	GetVariablesArray()	const { return m_pVariablesArray; }

public: //virtual
	virtual void			OnCustomize					()				{}	
	virtual void			OnDeclareVariables			()				{}	
	virtual void 			OnDefineXMLExportQuery		(SqlTable*)		{}
	virtual void			OnPrepareXMLExportQuery		(SqlTable*)		{}

	virtual BOOL 	Parse		(CXMLNode*, CAutoExpressionMng* = NULL);
	virtual BOOL 	Unparse		(CXMLNode*, CAutoExpressionMng* = NULL);	
protected:
	void DeclareVariable(const CString& sName, DataObj* pDataObj);
public:
	CXMLBaseAppCriteria&	operator =	(const CXMLBaseAppCriteria&);
	BOOL					operator ==	(const CXMLBaseAppCriteria& aXMLBaseExp)	const { return IsEqual	(aXMLBaseExp); }
	BOOL					operator !=	(const CXMLBaseAppCriteria& aXMLBaseExp)	const { return !IsEqual	(aXMLBaseExp); }
};

//----------------------------------------------------------------
//class CXMLUniversalKey 
//----------------------------------------------------------------
class TB_EXPORT CXMLUniversalKey : public CObject
{
	DECLARE_DYNAMIC(CXMLUniversalKey)
	
protected:
	CStringArray	m_SegmentArray;
	CString			m_strName;

public:
	CXMLUniversalKey					();
	CXMLUniversalKey					(CXMLUniversalKey&);
	CXMLUniversalKey					(const CStringArray&);

public:
	CString				GetSegmentAt		(int i)							{return m_SegmentArray.GetAt(i);}
	int					GetSegmentNumber	()								{return m_SegmentArray.GetSize();}
	int					AddSegment			(const CString& strVal)			{return m_SegmentArray.Add(strVal);}
	void				SetSegmentAt		(int i, const CString& strVal)	{m_SegmentArray.SetAt(i, strVal);}
	void				RemoveSegmentAt		(int i)							{m_SegmentArray.RemoveAt(i);}
	void				RemoveAllSegments	()								{m_SegmentArray.RemoveAll();}
	void				SetName				(const CString& strName)		{m_strName = strName;}
	CString				GetName				()						const	{return m_strName;} 
	
	//dato il nome di una colonna restituisce TRUE se è un segmento di UniversalKey
	BOOL				IsUniversalKeySegment(const CString& strColumnName);

public:
	BOOL				Parse			(CXMLNode*);
	BOOL				UnParse			(CXMLNode*);

public:
	BOOL				IsEqual			(const CXMLUniversalKey&) const;
	CXMLUniversalKey&	operator =		(const CXMLUniversalKey&);
	BOOL				operator ==		(const CXMLUniversalKey& aUK)	const { return IsEqual	(aUK); }
	BOOL				operator !=		(const CXMLUniversalKey& aUK)	const { return !IsEqual	(aUK); }
};

//----------------------------------------------------------------
//class CXMLUniversalKeyGroup 
//----------------------------------------------------------------
class TB_EXPORT CXMLUniversalKeyGroup : public Array
{
	DECLARE_DYNAMIC(CXMLUniversalKeyGroup)

protected:
	CString			m_strFuncion;
	CString			m_strTableName;
	BOOL			m_bExportData;

public:
	CXMLUniversalKeyGroup();
	CXMLUniversalKeyGroup(const CXMLUniversalKeyGroup&);

public:
	void					SetFunctionName	(const CString& strVal)	{m_strFuncion = strVal;}
	const CString&			GetFunctionName	()			const		{return m_strFuncion;}

	void					SetTableName	(const CString& strVal)	{m_strTableName = strVal;}
	const CString&			GetTableName	()			const		{return m_strTableName;
	}
	void					SetExportData	(BOOL bVal)				{m_bExportData = bVal;}
	BOOL					IsExportData	()		const			{return m_bExportData;}
	
public:
	BOOL					Parse			(CXMLNode*);
	BOOL					UnParse			(CXMLNode*);

public:
	int						Add				(CXMLUniversalKey* pEl)	{ return Array::Add (pEl); }
	CXMLUniversalKey*		GetAt			(int nIdx) const		{ return (CXMLUniversalKey*) Array::GetAt(nIdx);	}
	CXMLUniversalKey*&		ElementAt		(int nIdx)				{ return (CXMLUniversalKey*&) Array::ElementAt(nIdx); }
	BOOL					Remove			(CXMLUniversalKey*);
	BOOL					IsPresent		(const CString&);

	CXMLUniversalKey*		GetUKByName		(const CString& strUK) const;

	//dato il nome di una colonna restituisce TRUE se è un segmento di una delle UniversalKey definite nel gruppo
	BOOL	IsUniversalKeySegment(const CString& strColumnName);


public: //operator
	BOOL					IsEqual			(const CXMLUniversalKeyGroup&) const;
	CXMLUniversalKey* 		operator[]		(int nIdx) const	{ return GetAt(nIdx);	}
	CXMLUniversalKey*&		operator[]		(int nIdx)			{ return ElementAt(nIdx);	}
	CXMLUniversalKeyGroup&	operator =		(const CXMLUniversalKeyGroup&);
	BOOL					operator ==		(const CXMLUniversalKeyGroup& aSeg)	const { return IsEqual	(aSeg); }
	BOOL					operator !=		(const CXMLUniversalKeyGroup& aSeg)	const { return !IsEqual	(aSeg); }
};

//----------------------------------------------------------------
//class CXMLFieldInfo 
//----------------------------------------------------------------
class TB_EXPORT CXMLFieldInfo : public CObject
{
	DECLARE_DYNAMIC(CXMLFieldInfo)

protected:
	CString	m_strFieldName;
	BOOL	m_bExport;

public:
	CXMLFieldInfo(LPCTSTR = NULL, BOOL = TRUE);
	CXMLFieldInfo(CXMLFieldInfo&);

public:
	BOOL	IsEqual			(const CXMLFieldInfo&) const;

public:
	const CString&	GetFieldName()	const { return m_strFieldName; }
	BOOL			IsToExport	()	const { return m_bExport; }
	
	void			SetFieldName(const CString&);
	void			SetExport	(BOOL = TRUE);

public: //operator
	CXMLFieldInfo&	operator =	(const CXMLFieldInfo&);
	BOOL			operator ==	(const CXMLFieldInfo& aField)	const { return IsEqual	(aField); }
	BOOL			operator !=	(const CXMLFieldInfo& aField)	const { return !IsEqual	(aField); }
};

//----------------------------------------------------------------
//class CXMLFieldsInfoArray 
//----------------------------------------------------------------
class TB_EXPORT CXMLFieldInfoArray : public Array
{
public:
	CXMLFieldInfoArray			();
	CXMLFieldInfoArray			(const CXMLFieldInfoArray&);

private:
	BOOL			IsEqual		(const CXMLFieldInfoArray&) const;

public:
	BOOL			Parse		(CXMLNode*);
	BOOL			UnParse		(CXMLNode*);

public:
	int				Add					(CXMLFieldInfo* pEl){ return Array::Add (pEl); }
	CXMLFieldInfo* 	GetAt				(int nIdx) const	{ return (CXMLFieldInfo*) Array::GetAt(nIdx);	}
	CXMLFieldInfo*&	ElementAt			(int nIdx)			{ return (CXMLFieldInfo*&) Array::ElementAt(nIdx); }
	CXMLFieldInfo* 	GetFieldByName		(const CString& strFieldName);
	BOOL			IsToExport			(const CString& strFieldName);
	BOOL			HasFieldsToExport	() const;

public: //operator
	CXMLFieldInfo* 		operator[]	(int nIdx) const	{ return GetAt(nIdx);	}
	CXMLFieldInfo*&		operator[]	(int nIdx)			{ return ElementAt(nIdx);	}
	CXMLFieldInfoArray&	operator =	(const CXMLFieldInfoArray&);
	BOOL				operator ==	(const CXMLFieldInfoArray& aSeg)	const { return IsEqual	(aSeg); }
	BOOL				operator !=	(const CXMLFieldInfoArray& aSeg)	const { return !IsEqual	(aSeg); }
};


//----------------------------------------------------------------
//class CXMLSegmentInfo 
//----------------------------------------------------------------
class TB_EXPORT CXMLSegmentInfo : public CObject
{
	DECLARE_DYNAMIC(CXMLSegmentInfo)

protected:
	CXMLXRefInfo*	m_pXRefInfo;
	BOOL			m_bOwnXRef;
	CString			m_strFKSegment;
	CString			m_strReferencedSegment;	
	CString			m_strFKFixedValue;

public:
	CXMLSegmentInfo(CXMLXRefInfo* = NULL, LPCTSTR = NULL, LPCTSTR = NULL, LPCTSTR = NULL, BOOL = FALSE);
	CXMLSegmentInfo(CXMLSegmentInfo&, BOOL = FALSE);

public:
	void	SetXRef			(CXMLXRefInfo*);
	BOOL	SetKeySegments	(LPCTSTR, LPCTSTR, LPCTSTR, BOOL = FALSE);
	BOOL	IsEqual			(const CXMLSegmentInfo&) const;
	BOOL	Parse			(CXMLNode*);
	BOOL	UnParse			(CXMLNode*);

public:
	CXMLXRefInfo*	GetXRef()				const { return m_pXRefInfo; } 
	const CString&	GetFKSegment()			const { return m_strFKSegment; } 
	const CString&	GetReferencedSegment()	const { return m_strReferencedSegment; }
	const CString&	GetFKStrFixedValue()	const { return m_strFKFixedValue; }

public: //operator
	CXMLSegmentInfo&	operator =	(const CXMLSegmentInfo&);
	BOOL				operator ==	(const CXMLSegmentInfo& aSeg)	const { return IsEqual	(aSeg); }
	BOOL				operator !=	(const CXMLSegmentInfo& aSeg)	const { return !IsEqual	(aSeg); }
};

//----------------------------------------------------------------
//class CXMLSegmentInfoArray 
//----------------------------------------------------------------
class TB_EXPORT CXMLSegmentInfoArray : public Array
{
public:
	CXMLSegmentInfoArray();
	CXMLSegmentInfoArray(CXMLSegmentInfoArray&);

private:
	BOOL				IsEqual	(const CXMLSegmentInfoArray&) const;

public:
	int					Add				(CXMLSegmentInfo* pEl){ return Array::Add (pEl); }
	CXMLSegmentInfo* 	GetAt			(int nIdx) const	{ return (CXMLSegmentInfo*) Array::GetAt(nIdx);	}
	CXMLSegmentInfo*&	ElementAt		(int nIdx)			{ return (CXMLSegmentInfo*&) Array::ElementAt(nIdx); }

	BOOL				IsFkPresent		(const CString&);

public: //operator
	CXMLSegmentInfo* 		operator[]	(int nIdx) const	{ return GetAt(nIdx);	}
	CXMLSegmentInfo*&		operator[]	(int nIdx)			{ return ElementAt(nIdx);	}
	CXMLSegmentInfoArray&	operator =	(const CXMLSegmentInfoArray&);
	BOOL					operator ==	(const CXMLSegmentInfoArray& aSeg)	const { return IsEqual	(aSeg); }
	BOOL					operator !=	(const CXMLSegmentInfoArray& aSeg)	const { return !IsEqual	(aSeg); }

	void SetSegmentsXRef(CXMLXRefInfo*);
};

//----------------------------------------------------------------
//class CXMLXRefInfo 
//----------------------------------------------------------------
class TB_EXPORT CXMLXRefInfo : public CObject
{
	DECLARE_DYNAMIC(CXMLXRefInfo)

protected:
	CTBNamespace			m_nsDoc;

public:
	CString					m_strName;
	CString					m_strUrlDati;
	CString					m_strProfile;
	//property
	BOOL					m_bMustExist;
	BOOL					m_bCanbeNull;
	BOOL					m_bNoDocQuery;
	BOOL					m_bSubjectTo; //se è un ext-ref condizionato

	//ext-ref condizionato
	DataStr					m_strExpression;	// espressione condizionale sull'utilizzo o meno dell'externalRef utlizzata solo se m_bSubjectTo = TRUE
	SymTable*				m_pSymTable;		// tabella dei simboli per l'espressione
	
	CXMLSegmentInfoArray	m_SegmentsArray;

	CString					m_strExportedInFile; // utilizzato in fase di esportazione. Serve per sapere il nome 
												 // del file in cui è stato esportato il record individuato dall'extref	
	long					m_lBookmark;		 // individua il punto nel file di esportazione in cui è presente il
												 // record individuato dall'extref
	BOOL					m_bOldUse	;		// used to save the original value in XMLProfileInfo
	CXMLUniversalKeyGroup*	m_pXMLUniversalKeyGroup;

	BOOL					m_bModified;

private:
	CString					m_strTableName;
	CTBNamespace			m_nsReferencedDBT;
	CString					m_strReferencedTableName;
	CTBNamespace			m_nsReferencedTable; //namespace della tabella referenziata
	BOOL					m_bUse;				 //usato dai profili per indicare quali xref usare
	BOOL					m_bOwnedByDoc;		 //usato dai profili per indicare se l'xref e' di documento o se e' stato aggiunto dal profilo.
	BOOL					m_bIsAppended;		 //usato per indicare se il profile proviene o meno da un file AppendExtReferences. Improvement #


public:
	CXMLXRefInfo(LPCTSTR = NULL);
	CXMLXRefInfo(CXMLXRefInfo&);
	~CXMLXRefInfo();

private:
	BOOL IsEqual	(const CXMLXRefInfo&) const;

public:
	BOOL Parse	(CXMLNode*, BOOL bDescription = TRUE);
	BOOL UnParse(CXMLNode*, BOOL bDescription = TRUE);

public:
	void					Set						(CXMLXRefInfo*);
	int						GetSegmentIdx			(const CXMLSegmentInfo&) const; 
	CXMLSegmentInfo*		GetSegmentAt			(int nIdx)	const;
	void					SetSegmentAt			(int, CXMLSegmentInfo*);
	BOOL					AddSegment				(CXMLSegmentInfo*);
	void					RemoveSegmentAt			(int);
	void					RemoveAllSegments		();	
	BOOL					SetDocumentNamespace	(LPCTSTR);
	BOOL					SetDocumentNamespace	(const CTBNamespace&);
	CString					GetReferencedTableName	();
	void					SetReferencedTableNs	(const CString& strNamespace);
	void					GetReferencedDBTList	(CObArray& arDBTs);
	CTBNamespace			GetReferencedDBTNs		();
	void					SetReferencedDBTNs		(const CString& strNamespace);

	CXMLUniversalKeyGroup*	GetXMLUniversalKeyGroup	() {return m_pXMLUniversalKeyGroup;}
	void					SetXMLUniversalKeyGroup	(CXMLUniversalKeyGroup*);

public: //inline
	int						GetSegmentsNum			() const { return m_SegmentsArray.GetSize();}

	const CString&			GetName					()	const 	{ return m_strName;		}
	BOOL					MustExist				()	const 	{ return m_bMustExist;	}
	BOOL					CanBeNull				()	const 	{ return m_bCanbeNull;	}
	BOOL					IsNotDocQueryToUse		()	const 	{ return m_bNoDocQuery; }
	const CString&			GetUrlDati				()	const 	{ return m_strUrlDati;	}
	const CString&			GetProfile				()	const	{ return m_strProfile;	}
	const CString&			GetTableName			()	const	{ return m_strTableName;}
	CTBNamespace			GetDocumentNamespace	()	const	{ return m_nsDoc;	}
	CString					GetExportedFile			()	const	{ return m_strExportedInFile; }
	long					GetBookmark				()  const	{ return m_lBookmark; }
	void					SetUse					(BOOL bUse)	{m_bUse = bUse;}
	BOOL					IsToUse					()	const	{ return m_bUse;}
	void					SetOwnedByDoc			(BOOL bOwnedByDoc){m_bOwnedByDoc = bOwnedByDoc;}
	BOOL					IsOwnedByDoc			()	const	{ return m_bOwnedByDoc; }

	void					SetAppended			(BOOL bIsAppended) {m_bIsAppended = bIsAppended;}
	BOOL					IsAppended			()	const	{ return m_bIsAppended; }	
	
	BOOL					HasValidRefDoc			();

public: //gestione ext-ref 
	BOOL LoadSymTable			();
	void RemoveSymTable			();

	BOOL AddFieldInSymTable		(const CString& strName, DataObj* pDataObj);
	BOOL CheckExpressionSintax	(const DataStr& strExpression, CString& strMessage);
	BOOL LoadSymTable			(SqlRecord* pRecord);
	BOOL EvalExpression			(SqlRecord* pRecord, CString& strMessage);

public: //operator
	CXMLXRefInfo&	operator =	(const CXMLXRefInfo&);
	BOOL			operator ==	(const CXMLXRefInfo& aXRef)	const { return IsEqual	(aXRef); }
	BOOL			operator !=	(const CXMLXRefInfo& aXRef)	const { return !IsEqual	(aXRef); }
};

//----------------------------------------------------------------
class CXMLDBTData : public CObject
{
public:
	CString m_strNs;
	CString m_strTitle;
	CString m_strTableNs;

public:
	CXMLDBTData (const CString& strNs, const CString& strTitle,const CString& strTableNs);
};

//----------------------------------------------------------------
//class CXMLXRefInfoArray 
//----------------------------------------------------------------
class TB_EXPORT CXMLXRefInfoArray : public Array
{
public:
	CXMLXRefInfoArray();
	CXMLXRefInfoArray(const CXMLXRefInfoArray&);

private:	
	BOOL			IsEqual	(const CXMLXRefInfoArray&) const;

public:
	int				Add					(CXMLXRefInfo* pEl)	{ return Array::Add (pEl); }
	CXMLXRefInfo* 	GetAt				(int nIdx)const	{ return (CXMLXRefInfo*) Array::GetAt(nIdx);	}
	CXMLXRefInfo*&	ElementAt			(int nIdx)		{ return (CXMLXRefInfo*&) Array::ElementAt(nIdx); }
	CXMLXRefInfo*	Lookup				(LPCTSTR aSegment, ...);
	BOOL			GetXRefArrayByFK	(const CString&, CXMLXRefInfoArray*, BOOL bUsedOnly = TRUE);
	BOOL			IsFKInUsedExtRef	(const CString&) const;

public:
	BOOL Parse		(CXMLNode*, LPCTSTR);
	BOOL UnParse	(CXMLNode*, CXMLDBTInfo*, BOOL bDescription = TRUE);

public: //operator
	CXMLXRefInfo* 		operator[]	(int nIdx) const		{ return GetAt(nIdx);	 }
	CXMLXRefInfo*&		operator[]	(int nIdx)				{ return ElementAt(nIdx);}
	CXMLXRefInfoArray&	operator =	(const CXMLXRefInfoArray&);
	BOOL				operator ==	(const CXMLXRefInfoArray& aXRefAr)	const { return IsEqual	(aXRefAr); }
	BOOL				operator !=	(const CXMLXRefInfoArray& aXRefAr)	const { return !IsEqual	(aXRefAr); }
};


//----------------------------------------------------------------
//class CXMLXReferencesToAppend 
//----------------------------------------------------------------
class CXMLXReferencesToAppend : public CObject
{
	DECLARE_DYNAMIC(CXMLXReferencesToAppend)	

public:
	CString				m_strFileName;
	CString				m_strDocNamespace;
	CXMLXRefInfoArray*	m_pXRefsToAppendArray;

private:
	BOOL IsEqual(const CXMLXReferencesToAppend& aXRefArray) const;

public:
	CXMLXReferencesToAppend	();
	CXMLXReferencesToAppend	(CXMLXReferencesToAppend&);
	~CXMLXReferencesToAppend();

public: //operator
	BOOL	operator ==	(const CXMLXReferencesToAppend& aXRefAr)	const { return IsEqual	(aXRefAr); }
	BOOL	operator !=	(const CXMLXReferencesToAppend& aXRefAr)	const { return !IsEqual	(aXRefAr); }

public:	
	void SubstituteTableName(CXMLXRefInfo* pXRefInfo, const CString& strOldTableName, const CString& strNewTableName);
	void SetInfoFromDocNamespace(const CString& strDocNamespace);

	BOOL Parse(const CString& strFileName, const CString& strTableName);
	void Unparse(const CString& tableName);
};
 
//----------------------------------------------------------------
//class CXMLXReferencesToAppendArray 
//----------------------------------------------------------------
class TB_EXPORT CXMLXReferencesToAppendArray : public Array
{
public:
	CXMLXReferencesToAppendArray() {};
	CXMLXReferencesToAppendArray(const CXMLXReferencesToAppendArray&);

private:
	BOOL IsEqual(const CXMLXReferencesToAppendArray& aXRefArray) const;

public:
	int					Add				(CXMLXReferencesToAppend* pEl){ return Array::Add (pEl); }
	CXMLXReferencesToAppend* 	GetAt	(int nIdx) const	{ return (CXMLXReferencesToAppend*) CObArray::GetAt(nIdx);	}

public: //operator
	CXMLXReferencesToAppend* 		operator[]	(int nIdx) const	{ return GetAt(nIdx);	}
	CXMLXReferencesToAppendArray&   operator =(const CXMLXReferencesToAppendArray& aXRefAppendArray);
	BOOL				operator ==	(const CXMLXReferencesToAppendArray& aXRefAr)	const { return IsEqual	(aXRefAr); }
	BOOL				operator !=	(const CXMLXReferencesToAppendArray& aXRefAr)	const { return !IsEqual	(aXRefAr); }


public:
	void Unparse(CXMLNode*pDBTNode, const CString& tableName);
	BOOL Parse(CXMLNode*pDBTNode, const CString& tableName);
};

//----------------------------------------------------------------
//class CXMLFixedKey 
//----------------------------------------------------------------
class CXMLFixedKey : public CObject
{
	DECLARE_DYNAMIC(CXMLFixedKey)
	
protected:
	CString		m_strName;
	CString		m_strValue;

public:
	CXMLFixedKey	();
	CXMLFixedKey	(CXMLFixedKey&);

public:
	void				SetName		(const CString& strName)		{m_strName = strName;}
	CString				GetName		()						const	{return m_strName;} 
	void				SetValue	(const CString& strValue)		{m_strValue = strValue;}
	CString				GetValue	()						const	{return m_strValue;} 

public:
	BOOL				Parse			(CXMLNode*);
	BOOL				UnParse			(CXMLNode*);

public:
	BOOL				IsEqual			(const CXMLFixedKey&) const;
	CXMLFixedKey&		operator =		(const CXMLFixedKey&);
	BOOL				operator ==		(const CXMLFixedKey& aFixedK)	const { return IsEqual	(aFixedK); }
	BOOL				operator !=		(const CXMLFixedKey& aFixedK)	const { return !IsEqual	(aFixedK); }
};


//----------------------------------------------------------------
//class CXMLFixedKeyArray 
//----------------------------------------------------------------
class CXMLFixedKeyArray : public Array
{
	DECLARE_DYNAMIC(CXMLFixedKeyArray)

public:
	CXMLFixedKeyArray();
	CXMLFixedKeyArray(const CXMLFixedKeyArray&);

public:
	BOOL					Parse			(CXMLNode*);
	BOOL					UnParse			(CXMLNode*);

public:
	int						Add				(CXMLFixedKey* pEl)	{ return Array::Add (pEl); }
	CXMLFixedKey*			GetAt			(int nIdx) const		{ return (CXMLFixedKey*) Array::GetAt(nIdx);	}
	CXMLFixedKey*&			ElementAt		(int nIdx)				{ return (CXMLFixedKey*&) Array::ElementAt(nIdx); }

	CXMLFixedKey*			GetFixedKeyByName	(const CString& strKeyName) const;
	BOOL					Remove				(CXMLFixedKey* pXMLFixedKey);

public: //operator
	BOOL				IsEqual			(const CXMLFixedKeyArray&) const;
	CXMLFixedKey* 		operator[]		(int nIdx) const	{ return GetAt(nIdx);	}
	CXMLFixedKey*&		operator[]		(int nIdx)			{ return ElementAt(nIdx);	}
	CXMLFixedKeyArray&	operator =		(const CXMLFixedKeyArray&);
	BOOL				operator ==		(const CXMLFixedKeyArray& aFixedArray)	const { return IsEqual	(aFixedArray); }
	BOOL				operator !=		(const CXMLFixedKeyArray& aFixedArray)	const { return !IsEqual	(aFixedArray); }
};

//----------------------------------------------------------------
//class CXMLHKLField
//----------------------------------------------------------------
class CXMLHKLField : public CObject
{
	DECLARE_DYNAMIC(CXMLHKLField)
	
protected:
	CString		m_strReportField;
	CString		m_strDocumentField;
	BOOL		m_bIsXRefField;

public:
	CXMLHKLField	();
	CXMLHKLField	(CXMLHKLField&);

public:
	void				SetReportField		(const CString& strReportField)		{ m_strReportField = strReportField; }
	CString				GetReportField		()						const		{ return m_strReportField; } 
	void				SetDocumentField	(const CString& strDocumentField)	{ m_strDocumentField = strDocumentField; }
	CString				GetDocumentField	()						const		{ return m_strDocumentField; } 
	void				SetXRefField		(BOOL bSet)							{ m_bIsXRefField = bSet; }
	BOOL				IsXRefField			()						const		{ return m_bIsXRefField; } 
	
public:
	void				Parse			(CXMLNode*);
	void				UnParse			(CXMLNode*);

public:
	BOOL				IsEqual			(const CXMLHKLField&) const;
	CXMLHKLField&		operator =		(const CXMLHKLField&);
	BOOL				operator ==		(const CXMLHKLField& aField)	const { return IsEqual	(aField); }
	BOOL				operator !=		(const CXMLHKLField& aField)	const { return !IsEqual	(aField); }
};


//----------------------------------------------------------------
//class CXMLHKLFieldArray 
//----------------------------------------------------------------
class CXMLHKLFieldArray : public Array
{
	DECLARE_DYNAMIC(CXMLHKLFieldArray)

public:
	enum HKLListType { PREVIEW_TYPE, FILTER_TYPE, RESULT_TYPE };

private:
	HKLListType m_eListType;
	CString		 m_strChildTagName;

public: 
	CXMLHKLFieldArray(HKLListType);
	CXMLHKLFieldArray(const CXMLHKLFieldArray&);

public:
	void				Parse			(CXMLNode*);
	void				UnParse			(CXMLNode*);

public:
	int					Add				(CXMLHKLField* pEl)	{ return Array::Add (pEl); }
	CXMLHKLField*		GetAt			(int nIdx) const	{ return (CXMLHKLField*) Array::GetAt(nIdx);	}
	CXMLHKLField*&		ElementAt		(int nIdx)			{ return (CXMLHKLField*&) Array::ElementAt(nIdx); }
	BOOL				Remove			(CXMLHKLField* pXMLFixedKey);

public: //operator
	BOOL				IsEqual			(const CXMLHKLFieldArray&) const;
	CXMLHKLField* 		operator[]		(int nIdx) const	{ return GetAt(nIdx);	}
	CXMLHKLField*&		operator[]		(int nIdx)			{ return ElementAt(nIdx);	}
	CXMLHKLFieldArray&	operator =		(const CXMLHKLFieldArray&);
	BOOL				operator ==		(const CXMLHKLFieldArray& aFieldArray)	const { return IsEqual	(aFieldArray); }
	BOOL				operator !=		(const CXMLHKLFieldArray& aFieldArray)	const { return !IsEqual	(aFieldArray); }
};

//----------------------------------------------------------------
//class CXMLHotKeyLink 
//----------------------------------------------------------------
class TB_EXPORT CXMLHotKeyLink : public CObject
{
	friend class CHotKeyLinkDlg;

	DECLARE_DYNAMIC(CXMLHotKeyLink)

public:
	enum HKLFieldType {DBT, XREF, FIELD};

protected:
	CString				m_strFieldName;
	HKLFieldType		m_eFieldType;
	HKLFieldType		m_eSubType;
	CTBNamespace		m_nsReport;

public:
	CString				m_strTextBoxField;
	CString				m_strDescriptionField;
	CString				m_strImageField;

	CXMLHKLFieldArray	m_arPreviewFields;
	CXMLHKLFieldArray	m_arFilterFields;
	CXMLHKLFieldArray	m_arResultFields;

public:
	CXMLHotKeyLink	();
	CXMLHotKeyLink	(CXMLHotKeyLink&);

private:
	CString	GetStrHKLFieldType(HKLFieldType) const;
	void SetHKLFieldType(HKLFieldType&, const CString& strType);

public:
	void				SetFieldName		(const CString& strName)		{m_strFieldName = strName;}
	CString				GetFieldName		()	const	{return m_strFieldName;} 

	void				SetHKLFieldType		(const HKLFieldType& eType)		{m_eFieldType = eType;}
	HKLFieldType		GetHKLFieldType		()	const	{return m_eFieldType;}

	void				SetHKLSubType		(const HKLFieldType& eType)		{m_eSubType = eType;}
	HKLFieldType		GetHKLSubType		()	const	{return m_eSubType;} 

	void				SetReportNamespace	(const CTBNamespace& nsReport)		{m_nsReport = nsReport;}
	void				SetReportNamespace	(const CString& strNamespace);
	const CTBNamespace&	GetReportNamespace	()	const	{return m_nsReport;} 

public:
	BOOL				Parse			(CXMLNode*);
	BOOL				UnParse			(CXMLNode*);

public:
	BOOL				IsEqual			(const CXMLHotKeyLink&) const;
	CXMLHotKeyLink&		operator =		(const CXMLHotKeyLink&);
	BOOL				operator ==		(const CXMLHotKeyLink& aHotKey)	const { return IsEqual	(aHotKey); }
	BOOL				operator !=		(const CXMLHotKeyLink& aHotKey)	const { return !IsEqual	(aHotKey); }
};


//----------------------------------------------------------------
//class CXMLHotKeyLinkArray 
//----------------------------------------------------------------
class TB_EXPORT CXMLHotKeyLinkArray : public Array
{
	DECLARE_DYNAMIC(CXMLHotKeyLinkArray)

public:
	CXMLHotKeyLinkArray();
	CXMLHotKeyLinkArray(const CXMLHotKeyLinkArray&);

public:
	BOOL	Parse			(CXMLNode*);
	BOOL	UnParse			(CXMLNode*);

public:
	static BOOL IsHKLOfThisExtRef(const CString& strHKLFieldName, const CString& strXRefName);

public:
	int						Add				(CXMLHotKeyLink* pEl)	{ return Array::Add (pEl); }
	CXMLHotKeyLink*			GetAt			(int nIdx) const	{ return (CXMLHotKeyLink*) Array::GetAt(nIdx);	}
	CXMLHotKeyLink*&		ElementAt		(int nIdx)			{ return (CXMLHotKeyLink*&) Array::ElementAt(nIdx); }

	CXMLHotKeyLink*			GetHKLByFieldName  (
													const CString& strFieldName, 
													CXMLHotKeyLink::HKLFieldType eType, 
													CXMLHotKeyLink::HKLFieldType eSubType = CXMLHotKeyLink::FIELD
												) const;

	void					GetAllHKLForType	(CXMLHotKeyLink::HKLFieldType eType, CXMLHotKeyLinkArray* pHKLArray, const CString& strXRefName = _T("")) const;

	BOOL					Remove				(CXMLHotKeyLink* pXMLFixedKey);
	void					RemoveOnlyForXRefHKL(const CString& strXRefName);
	void					RemoveOnlyForDBTHKL ();

public: //operator
	BOOL					IsEqual			(const CXMLHotKeyLinkArray&) const;
	CXMLHotKeyLink* 		operator[]		(int nIdx) const	{ return GetAt(nIdx);		}
	CXMLHotKeyLink*&		operator[]		(int nIdx)			{ return ElementAt(nIdx);	}
	CXMLHotKeyLinkArray&	operator =		(const CXMLHotKeyLinkArray&);
	BOOL					operator ==		(const CXMLHotKeyLinkArray& aHKLArray)	const { return IsEqual	(aHKLArray); }
	BOOL					operator !=		(const CXMLHotKeyLinkArray& aHKLArray)	const { return !IsEqual	(aHKLArray); }
};


//----------------------------------------------------------------
//class CXMLSearchBookmark 
//----------------------------------------------------------------
class TB_EXPORT CXMLSearchBookmark : public CObject
{
	DECLARE_DYNAMIC(CXMLSearchBookmark)
	

protected:
	CString			m_strName;
	BOOL			m_bShowAsDescription;
	CString			m_strHKLName; //server per valorizzare i searchbookmark i cui valori sono contenuti in altre tabelle 
								//esempio CompanyName, Address, FiscalCode.. dei Clienti nei documenti di vendita
								//serve per il SOSConnector. Viene utilizzato l'hotlink del documento identificato univocamenteda m_strHKL
	CString			m_strKeyCode; //impr. #5185
public:
	CXMLSearchBookmark	();
	CXMLSearchBookmark	(CXMLSearchBookmark&);

public:
	CString		GetName			 ()	const	{ return m_strName; } 
	BOOL		ShowAsDescription()	const   { return m_bShowAsDescription; }
	CString		GetHKLName		 ()	const   { return m_strHKLName; }
	CString		GetKeyCode		 ()	const	{ return m_strKeyCode; } 	
		
public:
	BOOL Parse		(CXMLNode*);
	BOOL UnParse	(CXMLNode*);

public:
	BOOL							IsEqual		(const CXMLSearchBookmark&) const;
	CXMLSearchBookmark&	operator =	(const CXMLSearchBookmark&);
	BOOL							operator ==	(const CXMLSearchBookmark& aBusinessConst)	const { return IsEqual	(aBusinessConst); }
	BOOL							operator !=	(const CXMLSearchBookmark& aBusinessConst)	const { return !IsEqual	(aBusinessConst); }
};


//----------------------------------------------------------------
//class CXMLSearchBookmarkArray 
//----------------------------------------------------------------
class TB_EXPORT CXMLSearchBookmarkArray : public Array
{
	DECLARE_DYNAMIC(CXMLSearchBookmarkArray)

private:
	int m_nVersion;

public:
	CXMLSearchBookmarkArray() : m_nVersion(1) {};
	CXMLSearchBookmarkArray(const CXMLSearchBookmarkArray&);

public:
	BOOL	Parse	(CXMLNode*);
	BOOL	UnParse	(CXMLNode*);

public:
	int						Add			(CXMLSearchBookmark* aBusinessConst)	{ return Array::Add (aBusinessConst); }
	CXMLSearchBookmark*		GetAt		(int nIdx) const					{ return (CXMLSearchBookmark*) Array::GetAt(nIdx);	}
	CXMLSearchBookmark*&	ElementAt	(int nIdx)							{ return (CXMLSearchBookmark*&) Array::ElementAt(nIdx); }

	CXMLSearchBookmark*		GetSearchBookmarkByName(const CString& strKeyName) const;
	BOOL					Remove					(CXMLSearchBookmark* aBusinessConst);

	CStringArray*			GetShowAsDescriptionFields() const;
	int						GetVersion() const { return m_nVersion; }

public: //operator
	BOOL					IsEqual	(const CXMLSearchBookmarkArray&) const;
	CXMLSearchBookmark* 	operator[]		(int nIdx) const	{ return GetAt(nIdx);	}
	CXMLSearchBookmark*&	operator[]		(int nIdx)			{ return ElementAt(nIdx);	}
	CXMLSearchBookmarkArray&	operator =		(const CXMLSearchBookmarkArray&);
	BOOL						operator ==		(const CXMLSearchBookmarkArray& aBusinessConstArray)	const { return IsEqual	(aBusinessConstArray); }
	BOOL						operator !=		(const CXMLSearchBookmarkArray& aBusinessConstArray)	const { return !IsEqual	(aBusinessConstArray); }
};



//----------------------------------------------------------------
//class CXMLDBTInfo 
//----------------------------------------------------------------
class TB_EXPORT CXMLDBTInfo : public CObject
{
	friend class CXMLDBTInfoArray;
	friend class CXMLDocInfo;
	friend class CXMLClientDocInfo;

	DECLARE_DYNAMIC(CXMLDBTInfo)

public:
	enum	DBTType			{ MASTER_TYPE, SLAVE_TYPE, BUFFERED_TYPE, SLAVABLE_TYPE, UNDEF_TYPE};
	enum	UpdateType		{ REPLACE, INSERT_UPDATE, ONLY_INSERT};
	enum	OriginType		{ STANDARD, CLIENT_DOC, CUSTOM };

public:
	CTBNamespace				m_nsDBT;
	CString						m_strTitle;
	CString						m_strOriginalTitle;
	CString						m_strTableName;
	CTBNamespace				m_nsTable;
	DBTType						m_eType;
	UpdateType					m_eUpdateType;
	BOOL						m_bChooseUpdate;
	BOOL						m_bExport;
	OriginType					m_bIsFrom;
	
	CXMLXRefInfoArray*			m_pXRefsArray;
	CXMLFieldInfoArray*			m_pXMLFieldInfoArray;
	CXMLUniversalKeyGroup*		m_pXMLUniversalKeyGroup;

	// serve per OSL e l'estrazione dei record cancellati
	// il programmatore dichiara queali sono gli eventuali 
	// campi di chiave primaria che non variano. Vedi clienti/fornitori e 
	// documenti in cui abbiamo il tipo documento
	CXMLFixedKeyArray*			m_pXMLFixedKeyArray;
	// per MagicDocuments: serve per memorizzare i report da poter utilizzare per la gestione degli hotkeylink
	// è possibile associare il report, ad un singolo campo oppure all'intero DBT
	CXMLHotKeyLinkArray*		m_pXMLHotKeyLinkArray;

	//serve per descrivere i constraint gestionali sui campi. Es. se il campo è obbligatorio in una "transazione" di XGate
	//se deve essere presente come Bookmark in EasyAttachment
	CXMLSearchBookmarkArray* m_pXMLSearchBookmarkArray;
	
	
	//gestione ExtReferencesToAppend.xml creata per i CrossReference di ERP dove la definizione degli externalreference non è per DBT ma è comune a più documenti
	// vedi Improvement #
	CXMLXReferencesToAppendArray*		m_pXRefsToAppendArray;

public:
	CXMLDBTInfo						();
	CXMLDBTInfo						(CXMLDBTInfo&);
	~CXMLDBTInfo					();

private:
	BOOL IsEqual					(const CXMLDBTInfo&) const;
	BOOL CheckUUID					(CXMLNode*, LPCTSTR);
	BOOL SetDBTInfo					(CXMLNode*);
	BOOL LoadExternalReference		(CLocalizableXMLDocument*);
	BOOL LoadAppendedExternalReferences(CXMLNode*);
	BOOL LoadUniversalKeysInfo		(CXMLNode*);
	BOOL SaveUniversalKeysInfo		(CXMLNode*);
	BOOL LoadFixedKeysInfo			(CXMLNode*);
	BOOL SaveFixedKeysInfo			(CXMLNode*);
	BOOL LoadFields					(CXMLNode*);
	BOOL SaveFields					(CXMLNode*);
	BOOL LoadHotKeyLinks			(CXMLNode*);
	BOOL SaveHotKeyLinks			(CXMLNode*);
	BOOL HasFieldsToExport			();

	BOOL LoadBusinessConstraints	(CXMLNode*);
	BOOL SaveBusinessConstraints	(CXMLNode*);
	

public:
	CXMLDBTInfo::UpdateType GetUpdateTypeFromString (const CString& strType);

public:
	// per caricare le info del master da parte di DBTMaster
	BOOL LoadMasterDBTInfo			(CLocalizableXMLDocument*, CLocalizableXMLDocument*, LPCTSTR);
	// per caricare le info di uno slave da parte di DBTSlave/DBTSlaveBuffered
	BOOL LoadDBTInfo				(CLocalizableXMLDocument*, CLocalizableXMLDocument*, LPCTSTR, LPCTSTR);
	BOOL SetDBTInfo					(DBTObject*);
	BOOL UnParse					(CXMLNode*, CXMLNode*);

public:
	void					Set				(CXMLDBTInfo*);
	CString					GetStrType		()	const;
	void					SetType			(const CString&);
	BOOL					IsXRefPresent	(const CString&);
	BOOL					IsXRefPresent	(CXMLXRefInfo*);
	
	//funzioni tipo array per gli external reference
	BOOL					AddXRef				(CXMLXRefInfo*);
	void					SetXRefAt			(int, CXMLXRefInfo*);
	CXMLXRefInfo*			GetXRefAt			(int)const;
	CXMLXRefInfo*			GetXRefByFK			(LPCTSTR)const;
	CXMLXRefInfo*			GetXRefByName		(LPCTSTR)const;
	void					RemoveXRefAt		(int);
	void					RemoveXRef			(const CXMLXRefInfo&);
	void					SetXMLXRefInfoArray	(const CXMLXRefInfoArray*);
	void					SetXMLXRefInfoArray	(const CXMLDBTInfo&);
	CXMLXRefInfoArray*		GetXMLXRefInfoArray	()	{ return m_pXRefsArray; }
	
	void					SetXMLXRefInfoToAppendArray	(const CXMLXReferencesToAppendArray*);
	void					SetXMLXRefInfoToAppendArray (const CXMLDBTInfo&);
	
	void					SetXRefUseFlag	(BOOL /*= TRUE*/);

	//funzioni tipo array per i field
	BOOL					AddField			(CXMLFieldInfo*);
	void					SetFieldAt			(int, CXMLFieldInfo*);
	CXMLFieldInfo*			GetFieldAt			(int)const;
	void					RemoveFieldAt		(int);
	void					RemoveField			(const CXMLFieldInfo&);
	BOOL					IsFieldToExport		(const CString& strFieldName);
	void					SetXMLFieldInfoArray(const CXMLFieldInfoArray*);
	void					SetXMLFieldInfoArray(const CXMLDBTInfo&);
	CXMLFieldInfoArray*		GetXMLFieldInfoArray()	{ return m_pXMLFieldInfoArray; }

	//funzioni tipo array per le universal key
	BOOL					AddXMLUniversalKey			(CXMLUniversalKey*);
	void					SetXMLUniversalKeyAt		(int, CXMLUniversalKey*);
	CXMLUniversalKey*		GetXMLUniversalKeyAt		(int)const;
	void					RemoveXMLUniversalKeyAt	(int);
	void					RemoveXMLUniversalKey		(const CXMLUniversalKey&);
	void					SetXMLUniversalKeyGroup	(const CXMLUniversalKeyGroup*);
	void					SetXMLUniversalKeyGroup	(const CXMLDBTInfo&);
	CXMLUniversalKeyGroup*	GetXMLUniversalKeyGroup	()	{ return m_pXMLUniversalKeyGroup; }
    
	void					SetXMLFixedKeyArray	(const CXMLFixedKeyArray*);
	void					SetXMLFixedKeyArray	(const CXMLDBTInfo&);
	CXMLFixedKeyArray*		GetXMLFixedKeyArray	()	{ return m_pXMLFixedKeyArray; }

	void					SetXMLHotKeyLinkArray	(const CXMLDBTInfo&);
	void					SetXMLHotKeyLinkArray	(const CXMLHotKeyLinkArray*);
	CXMLHotKeyLinkArray*	GetXMLHotKeyLinkArray	()	{ return m_pXMLHotKeyLinkArray; }
	CXMLHotKeyLink*			GetHKLByFieldName		(const CString& strFieldName, CXMLHotKeyLink::HKLFieldType eType, CXMLHotKeyLink::HKLFieldType eSubType = CXMLHotKeyLink::FIELD)	
									{ return (m_pXMLHotKeyLinkArray) ? m_pXMLHotKeyLinkArray->GetHKLByFieldName(strFieldName, eType, eSubType) : NULL; }
	CXMLHotKeyLink*			GetDBTXMLHotKeyLinkInfo();

	
	void					SetXMLSearchBookmarkArray	(const CXMLDBTInfo&);
	void					SetXMLSearchBookmarkArray	(const CXMLSearchBookmarkArray*);
	CXMLSearchBookmarkArray* GetXMLSearchBookmarkArray	()	{ return m_pXMLSearchBookmarkArray; }

	//updateType
	CString					GetStrUpdateType () const;
	void					SetUpdateType	(const CString&);



	void AddXReferencesToAppend(CXMLXReferencesToAppend* );


public: // inline
	CTBNamespace			GetNamespace		()	const { return m_nsDBT; }
	const CString&			GetTitle			()	const { return m_strTitle; }
	const CString&			GetTableName		()	const { return m_strTableName; }
	CTBNamespace			GetTableNameSpace	()	const { return m_nsTable; }
	DBTType					GetType				()	const { return m_eType; }
	BOOL					IsToExport			()	const { return m_bExport; }
	UpdateType				GetUpdateType		()	const { return m_eUpdateType; }
	BOOL					GetChooseUpdate		()  const { return m_bChooseUpdate; }
	
	void					SetFromClientDoc	()	{ m_bIsFrom = CLIENT_DOC; }
	BOOL					IsFromClientDoc		()			const	{ return m_bIsFrom == CLIENT_DOC; }
	OriginType				IsFrom				() 			const	{ return m_bIsFrom; }
	void					SetFrom				(OriginType eOrigin){ m_bIsFrom = eOrigin; }

	BOOL					IsUniversalKeySegment	(const CString strColumnName)		 { return (m_pXMLUniversalKeyGroup && m_pXMLUniversalKeyGroup->IsUniversalKeySegment(strColumnName)); }
	BOOL					IsFieldInUsedExtRef		(const CString& strColumnName) const { return m_pXRefsArray && m_pXRefsArray->IsFKInUsedExtRef(strColumnName); }
	
public: //operator
	CXMLDBTInfo&	operator =	(const CXMLDBTInfo&);
	BOOL			operator ==	(const CXMLDBTInfo& aDbt)	const { return IsEqual	(aDbt); }
	BOOL			operator !=	(const CXMLDBTInfo& aDbt)	const { return !IsEqual	(aDbt); }
};

//----------------------------------------------------------------
//class CXMLDBTInfoArray 
//----------------------------------------------------------------
class TB_EXPORT CXMLDBTInfoArray : public Array
{
public:
	BOOL	m_bExistChooseUpdate;

public:
	CXMLDBTInfoArray();
	CXMLDBTInfoArray(const CXMLDBTInfoArray&);

public:
	int				Add			(CXMLDBTInfo* pEl)	{ return Array::Add (pEl); }
	CXMLDBTInfo*	GetAt		(int nIdx) const	{ return (CXMLDBTInfo*)Array::GetAt (nIdx); }
	CXMLDBTInfo*&	ElementAt	(int nIdx)			{ return (CXMLDBTInfo*&) Array::ElementAt(nIdx); }
	
	CXMLDBTInfo*	GetDBTByName(const CString&);
	CXMLDBTInfo*	GetDBTByNamespace(const CString&);
	CXMLDBTInfo*	GetDBTByNamespace(const CTBNamespace&);
	CXMLDBTInfo*	GetDBTMaster();
	CXMLDBTInfo*	GetDBTByXRef(CXMLXRefInfo*);
	CXMLSearchBookmarkArray* GetXMLSearchBookmark(const CTBNamespace&);
private:
	BOOL IsEqual			(const CXMLDBTInfoArray&) const;

public:
	BOOL ParseMaster		(CXMLNode*, CLocalizableXMLDocument* pXMLXRefDoc = NULL);
	BOOL ParseSlaves		(CXMLNode*, CLocalizableXMLDocument* pXMLXRefDoc = NULL); 

	BOOL Parse				(CLocalizableXMLDocument*,  CLocalizableXMLDocument* = NULL); 
	BOOL UnParse			(const CString&, const CString&);

	//informazioni relative ai field da esportare e agli hotkeylink usati da SmartDocuments, solo per i profili
	BOOL LoadFieldInfoFile		(const CString&);
	BOOL SaveFieldInfoFile		(const CString&);
	BOOL SaveHotKeyLinkInfoFile	(const CString&);
	BOOL LoadHotKeyLinkInfoFile	(const CString&);

public:
	BOOL GetXRefArrayByFK	(const CString&, CXMLXRefInfoArray*, BOOL bUsedOnly = TRUE);
	BOOL RemoveXRef			(CXMLXRefInfo*);

	

public: //operator
	CXMLDBTInfo* 		operator[]	(int nIdx)	const	{ return GetAt(nIdx);	}
	CXMLDBTInfo*&		operator[]	(int nIdx)			{ return ElementAt(nIdx);	}
	CXMLDBTInfoArray&	operator =	(const CXMLDBTInfoArray&);
	BOOL				operator ==	(const CXMLDBTInfoArray& aDbtAr)	const { return IsEqual	(aDbtAr); }
	BOOL				operator !=	(const CXMLDBTInfoArray& aDbtAr)	const { return !IsEqual	(aDbtAr); }
};

//----------------------------------------------------------------
//class CXMLHeaderInfo  
//----------------------------------------------------------------
class TB_EXPORT CXMLHeaderInfo : public CObject
{
	DECLARE_DYNAMIC(CXMLHeaderInfo)

public:
	CString			m_strVersion;
	int				m_nMaxDocument;
	int				m_nMaxDimension;
	CString			m_strUrlData;
	
	CString			m_strEnvClassTitle; //localized name
	CString			m_strEnvClass; 
	CString			m_strEnvClassExt;
	
	//for magic documents and web service
	BOOL			m_bPostable; 
	BOOL			m_bPostBack;
	BOOL			m_bNoExtRefPostBack;
	BOOL			m_bOnlyBusinessObject; //Impr. 6393 Istanziazione documento senza view

	// for TabDialog optimization: during the import procedure if m_bFullPrepare = TRUE all the tabdialog are activated 
	// in order to call the virtual method OnPrepareAuxData; otherwise are not activated. (see Bug 14386)
	BOOL			m_bFullPrepare;

	//document transformation
	BOOL				m_bTransform;
	CString				m_strTransformXSLT;

private:
	CTBNamespace	m_nsDoc;

public:
	CXMLHeaderInfo				();
	CXMLHeaderInfo				(const CTBNamespace&);
	CXMLHeaderInfo				(const CXMLHeaderInfo&);

public:
	BOOL		IsEqual			(const CXMLHeaderInfo&) const;
	void		Clear			();
	BOOL		Parse			(CLocalizableXMLDocument*);
	BOOL		UnParse			(const CString&);

public: //inline
	CString	GetStrMaxDoc		() const;
	void	SetMaxDoc			(const CString& strNumb) { m_nMaxDocument = _ttoi((LPCTSTR)strNumb); }
	
	int		GetMaxDimension		() const{ return m_nMaxDimension;}
	void	SetMaxDimension		(int);
	void	SetMaxDimension		(const CString&);
	
	void	SetEnvClass			(const CString& strEnvClass)	{ m_strEnvClass = strEnvClass; }
	void	SetEnvClassExt		(const CString& strEnvClassExt)	{ m_strEnvClassExt = strEnvClassExt; }
	CString	GetEnvClass			() { return m_strEnvClass; }
	CString	GetEnvClassExt		() { return m_strEnvClassExt; }
	CString	GetEnvClassWithExt	();

	// il default per l'envelope class è il nome del modulo di appartenenza del documento
	CString	GetDefaultEnvClass	() { return m_nsDoc.GetObjectName(CTBNamespace::MODULE); } 

	//for magic documents
	//BOOL	IsPostable()	const { return	m_bPostable; }
	//BOOL	IsPostBack()	const { return	m_bPostBack; }		
	//BOOL	IsNoExtRefPostBack()	const { return m_bNoExtRefPostBack;}
	//void	SetPostable(BOOL bSet = TRUE)  { m_bPostable = bSet; }
	//void	SetPostBack(BOOL bSet = TRUE)  { m_bPostBack = bSet; }
	//void	SetNoExtRefPostBackk(BOOL bSet = FALSE)	{ m_bNoExtRefPostBack = bSet;}


	// for TabDialog optimization
	BOOL	IsFullPrepare()	const { return	m_bFullPrepare; }		
	void	SetFullPrepare(BOOL bSet = TRUE)  { m_bFullPrepare = bSet; }	


public: //operator
	CXMLHeaderInfo&	operator =	(const CXMLHeaderInfo&);
	BOOL			operator ==	(const CXMLHeaderInfo& aHesd)	const { return IsEqual	(aHesd); }
	BOOL			operator !=	(const CXMLHeaderInfo& aHesd)	const { return !IsEqual	(aHesd); }

};

//----------------------------------------------------------------
//class CXMLClientDocInfo  
//----------------------------------------------------------------
class TB_EXPORT CXMLClientDocInfo : public CObject
{
	DECLARE_DYNAMIC(CXMLClientDocInfo)

public:
	CTBNamespace			m_nsClientDoc;
	CString					m_strClientDocName;
	CXMLDBTInfoArray*		m_pDBTArray;	 // dbt aggiunti dal ClientDoc. 
											// Non devo distruggere gli elementi, ci pensa il documento

	CString					m_strDBTFileName;
	CString					m_strXRefFileName;
	CLocalizableXMLDocument *m_pXMLClientDBTDoc;
	CLocalizableXMLDocument *m_pXMLClientXRefDoc;

private:
	CString					m_strXRefDescriName;

public:
	CXMLClientDocInfo						(const CTBNamespace&, const CString&);
	CXMLClientDocInfo						(const CXMLClientDocInfo&);
	~CXMLClientDocInfo						();

private:
	void			Assign					(const CXMLClientDocInfo&);
	
public:
	void			SetAllFilesName			();		
	void			SetFilesFromPartialPath	(const CString&, CPathFinder::PosType);
	BOOL			RenameProfilePath		(const CString&);
	BOOL			RemoveProfilePath		();


	BOOL			SaveDBTFile				();
	BOOL			ParseDBTFile			(CXMLDBTInfoArray*); //aggancia le info lette dai file dei dbt e degli extref 
												//all'array dei dbt del documento
	BOOL			LoadDBTFile				();	// serve solo per caricare in memoria i file com xmldom document
												// e leggere le informazioni successivamente				
	BOOL			SaveXRefFile			(BOOL bDescription = TRUE); //serve per i profili

	void			AddDBTInfo				(CXMLDBTInfo*);
	BOOL			ParseDBTInfoByNamespace	(const CTBNamespace&, CXMLDBTInfo*);
	CXMLDBTInfo*	AddDBTInfo				(DBTSlave*);


public: //operator
	CXMLClientDocInfo&	operator =	(const CXMLClientDocInfo&);
	BOOL				operator == (const CXMLClientDocInfo& aClientDocInfo) const;
	BOOL				operator != (const CXMLClientDocInfo& aClientDocInfo) const;

};

//----------------------------------------------------------------
//class CXMLClientDocInfo  
//----------------------------------------------------------------
class TB_EXPORT CXMLClientDocInfoArray : public Array
{
public:
	CXMLClientDocInfoArray();
	CXMLClientDocInfoArray(const CXMLClientDocInfoArray&);

private:
	void Assign(const CXMLClientDocInfoArray&);

public:
	int						Add						(CXMLClientDocInfo* pClientDoc)	{ return Array::Add (pClientDoc); }
	CXMLClientDocInfo*		GetAt					(int nIdx)					const	{ return (CXMLClientDocInfo*)Array::GetAt (nIdx); }
	CXMLClientDocInfo*		GetClientFromNamespace	(const CTBNamespace&)		const;
	
	void				AddDBTInfoToClient			(const CTBNamespace&, CXMLDBTInfo*);
	BOOL				ParseDBTInfoByNamespace		(const CTBNamespace&, const CTBNamespace&, CXMLDBTInfo*);
	CXMLDBTInfo*		UpdateClientDocDBTInfo		(const CTBNamespace&, DBTSlave*);
	
public:
	BOOL	SaveXRefFile	(BOOL bDescription = TRUE); //serve per i profili
	BOOL	SaveDBTFile		();
	BOOL	ParseDBTFile	(CXMLDBTInfoArray*);
	BOOL	LoadDBTFile		();

	void	SetAllFilesName ();
	void	SetFilesFromPartialPath	(const CString&, CPathFinder::PosType);
	BOOL	RenameProfilePath		(const CString&);
	BOOL	RemoveProfilePath		();

public: //operator
	CXMLClientDocInfoArray&	operator =	(const CXMLClientDocInfoArray&);
	BOOL					operator == (const CXMLClientDocInfoArray&) const;
	BOOL					operator != (const CXMLClientDocInfoArray&) const;
};


//----------------------------------------------------------------
//class CXMLDocObjectInfo  
//----------------------------------------------------------------
class TB_EXPORT CXMLDocObjectInfo : public CObject
{
	DECLARE_DYNAMIC(CXMLDocObjectInfo)

protected:
	CTBNamespace 				m_nsDoc;

public:
	CString						m_strDocumentName;
	CString						m_strDocumentTitle;

	CXMLHeaderInfo*				m_pHeaderInfo;
	CXMLDBTInfoArray*			m_pDBTArray;
	
	const CDocumentDescription*	m_pDocDescription;	
	CXMLClientDocInfoArray*		m_pClientDocsInfo;	

	CString						m_strDocFileName;
	CString						m_strDBTFileName;
	CString						m_strXRefFileName;

	BOOL						m_bIsLoaded;	
	BOOL						m_bReadOnly;
	
	//la descrizione è sempre in STANDARD è la posizione del profilo che varia
	CPathFinder::PosType		m_ePosType;
	CString						m_strUserName; //è presente nella path del profilo a seconda della personalizzazione effettuata

	
public:
	CXMLDocObjectInfo			(const CTBNamespace& = CTBNamespace());
	CXMLDocObjectInfo			(const CXMLDocObjectInfo&);
	~CXMLDocObjectInfo			();

protected:
	void SetDocInformation(const CTBNamespace& nsDoc);
	void Assign	(const CXMLDocObjectInfo&);

public:
	int				AddDBT						(CXMLDBTInfo*);
	void			RemoveDBTAt					(int);
	void			SetDBTAt					(int, CXMLDBTInfo*);
	CXMLDBTInfo*	GetDBTAt					(int)const;
	int				GetDBTIndexFromNamespace	(const CTBNamespace&) const;
	CXMLDBTInfo*	GetDBTFromNamespace			(const CTBNamespace&) const;
	CXMLDBTInfo*	GetDBTMaster				() const;
	CXMLDBTInfo*	GetDBTByXRef				(CXMLXRefInfo*);
	void			GetDBTXRefList				(CXMLXRefInfoArray*, CTBNamespace);
	CXMLSearchBookmarkArray* GetXMLSearchBookmark(const CTBNamespace& dbtNamespace) { return (m_pDBTArray) ? m_pDBTArray->GetXMLSearchBookmark(dbtNamespace) : NULL; }

	// setta il namespace e restituisce TRUE se la stringa è un namespace valido
	BOOL			SetNamespaceDoc			(LPCTSTR);
	BOOL			SetNamespaceDoc			(const CTBNamespace&);
	BOOL			IsValid				();

	
public:
	BOOL			SaveHeaderFile		();
	BOOL			SaveDBTFile			();

public: // inline
	BOOL			IsLoaded		()			{ return m_bIsLoaded;}
	const CString&	GetDocumentName	()	const	{ return m_strDocumentName;}
	const CString&	GetDocumentTitle()	const	{ return m_strDocumentTitle;}
	CTBNamespace	GetNamespaceDoc	()	const	{ return m_nsDoc;}

	CString	GetVersion			()	const	{ return (m_pHeaderInfo) ? m_pHeaderInfo->m_strVersion : _T("");	}
	int		GetMaxDocument		()	const	{ return (m_pHeaderInfo) ? m_pHeaderInfo->m_nMaxDocument : 0;		}
	int		GetMaxDimension		()	const	{ return (m_pHeaderInfo) ? m_pHeaderInfo->m_nMaxDimension : 0;		}
	CString	GetUrlData			()	const	{ return (m_pHeaderInfo) ? m_pHeaderInfo->m_strUrlData : _T("");	}
	CString	GetEnvClassTitle	()	const	{ return (m_pHeaderInfo) ? m_pHeaderInfo->m_strEnvClassTitle : _T("");	}
	CString	GetEnvClass			()	const	{ return (m_pHeaderInfo) ? m_pHeaderInfo->m_strEnvClass : _T("");	}
	CString	GetEnvClassWithExt	()  const	{ return (m_pHeaderInfo) ? m_pHeaderInfo->GetEnvClassWithExt() : _T("");}
	
	//for magic documents
	BOOL IsPostable		()	const { return	(m_pHeaderInfo) ? m_pHeaderInfo->m_bPostable : TRUE;; }
	BOOL IsPostBack		()	const { return	(m_pHeaderInfo) ? m_pHeaderInfo->m_bPostBack : FALSE; }		
	BOOL IsNoExtRefPostBack()	const { return	(m_pHeaderInfo) ? m_pHeaderInfo->m_bNoExtRefPostBack : FALSE; }		
	

	void SetVersion		 (const CString& strVers)			{ if (m_pHeaderInfo) m_pHeaderInfo->m_strVersion	= strVers;	   }
	void SetMaxDocument	 (int nMaxDoc)						{ if (m_pHeaderInfo) m_pHeaderInfo->m_nMaxDocument	= nMaxDoc;	   }
	void SetMaxDimension (int nMaxDimension)				{ if (m_pHeaderInfo) m_pHeaderInfo->m_nMaxDimension	= nMaxDimension;   }
	void SetUrlData		 (const CString & strUrlData)		{ if (m_pHeaderInfo) m_pHeaderInfo->m_strUrlData	= strUrlData;  }
	void SetEnvClass	 (const CString & strEnvClass)		{ if (m_pHeaderInfo) m_pHeaderInfo->m_strEnvClass	= strEnvClass; }
	void SetEnvClassWithExt(const CString & strEnvClassExt)	{ if (m_pHeaderInfo) m_pHeaderInfo->m_strEnvClassExt= strEnvClassExt; }

	//for magic documents
	void SetPostable		(BOOL bSet = TRUE)  { if (m_pHeaderInfo) m_pHeaderInfo->m_bPostable = bSet; }
	void SetPostBack		(BOOL bSet = TRUE)  { if (m_pHeaderInfo) m_pHeaderInfo->m_bPostBack = bSet; }		
	void SetNoExtRefPostBack(BOOL bSet = FALSE) { if (m_pHeaderInfo) m_pHeaderInfo->m_bNoExtRefPostBack = bSet; }		

	CXMLHeaderInfo*		GetHeaderInfo()	const { return m_pHeaderInfo;}
	
	void SetHeaderInfo (const CXMLHeaderInfo*); 
	void SetHeaderInfo (const CXMLDocObjectInfo&); 
	
	CXMLDBTInfoArray*	GetDBTInfoArray	()	const { return m_pDBTArray;}		
	
	void SetDBTArray (const CXMLDBTInfoArray*); 
	void SetDBTArray (const CXMLDocObjectInfo&); 

	//per i clientdoc
	CXMLClientDocInfoArray*	GetClientDocInfoArray()  const { return m_pClientDocsInfo;	}
	
	void			AttachClientDocInfo		();
	CXMLDBTInfo*	UpdateDBTInfo			(DBTSlave*);
	CXMLDBTInfo*	UpdateClientDocDBTInfo	(const CTBNamespace&, DBTSlave*);
	void			SetClientDocs			(const CXMLClientDocInfoArray*);
	void			SetClientDocs			(const CXMLDocObjectInfo&); 

	void			SetReadOnly			(BOOL bSet) { m_bReadOnly = bSet; }
	BOOL			IsReadOnly			() const { return m_bReadOnly; }

protected:
	BOOL LoadDBTFiles		(
								CLocalizableXMLDocument*&	pDBTDoc,
								CLocalizableXMLDocument*&	pXRefDoc,
								BOOL						bParse = FALSE
							);
public: // virtual
	virtual BOOL	LoadHeaderFile		();
	virtual BOOL	LoadDBTFile			();
	
	virtual BOOL	SaveAllFiles	();
	virtual	BOOL	LoadAllFiles	();

	// i file sono in path differenti a seconda delle classi figlie istanziate
	virtual void	SetAllFilesName ();

	virtual BOOL	OnNamespaceDocChanged() { return TRUE;}

public: //operator
	CXMLDocObjectInfo&	operator =	(const CXMLDocObjectInfo&);
	BOOL				operator ==	(const CXMLDocObjectInfo&)	const;
	BOOL				operator !=	(const CXMLDocObjectInfo&)	const;
};



// per la descrizione di un documento
//----------------------------------------------------------------
//class CXMLDocInfo
//----------------------------------------------------------------
class TB_EXPORT CXMLDocInfo : public CXMLDocObjectInfo
{
	DECLARE_DYNAMIC(CXMLDocInfo)

public:
	CLocalizableXMLDocument* m_pXMLDBTDoc;
	CLocalizableXMLDocument* m_pXMLXRefDoc;
	CXMLDefaultInfo*		m_pDefaultInfo;

private:
	CString				m_strDefaultsFileName;
	
public:
	CXMLDocInfo			(const CTBNamespace& = CTBNamespace());
	CXMLDocInfo			(const CXMLDocInfo&);
	~CXMLDocInfo		();

private:
	BOOL	IsEqual					(const CXMLDocInfo& aXMLDocInfo) const;
	BOOL	ParseDBTInfoByNamespace	(const CTBNamespace&, CXMLDBTInfo*) ;

public:
	BOOL	LoadXMLDBTInfo			(const CTBNamespace&, CXMLDBTInfo*, const CTBNamespace&);
	BOOL	SaveDefaultFile			();
	CString	GetPreferredProfile		()	const	{ return m_pDefaultInfo ? m_pDefaultInfo->GetPreferredProfile() : _T("");	}
	BOOL	SetPreferredProfile		(const CString &);
	
	CXMLDefaultInfo*	GetDefaultInfo	()			{ return m_pDefaultInfo;}

public: //static
	static CString  GetMasterTableNamespace(const CTBNamespace&); //passato il namespace del documento restituisce il namespace della master table

public: // virtual
	virtual BOOL SaveAllFiles		();
	virtual	BOOL LoadAllFiles		();
	virtual void SetAllFilesName	();
	virtual BOOL LoadDefaultsFile	();
	virtual BOOL LoadDBTFile		();

protected:
	void Assign						(const CXMLDocInfo&);

public:
	BOOL IsDataEqual				(const CXMLDocInfo& aXMLDocInfo) const;

public: //operator
	CXMLDocInfo&	operator =	(const CXMLDocInfo&);
	BOOL			operator ==	(const CXMLDocInfo& aXMLDocInfo) const { return IsEqual	(aXMLDocInfo); }
	BOOL			operator !=	(const CXMLDocInfo& aXMLDocInfo) const { return !IsEqual(aXMLDocInfo); }

};

//----------------------------------------------------------------------------
//	CAppDocumentsTreeCtrl
//----------------------------------------------------------------------------
class TB_EXPORT CAppDocumentsTreeCtrl : public CTBTreeCtrl
{
	DECLARE_DYNCREATE(CAppDocumentsTreeCtrl)

public:
	enum ItemType { APP_DOC_TREE_ITEM_TYPE_UNDEFINED, APP_DOC_TREE_ITEM_TYPE_ADDONAPP, APP_DOC_TREE_ITEM_TYPE_ADDONMODULE, APP_DOC_TREE_ITEM_TYPE_DOCUMENT };

private:
	CImageList	m_ImageList;
	BOOL		m_bReadOnly;

public:
	CAppDocumentsTreeCtrl	(BOOL bReadOnly = FALSE);
	~CAppDocumentsTreeCtrl	();

protected:
	void		InitializeImageList	();
	ItemType	GetItemType			(HTREEITEM) const;

public:
	void					FillTree				(const CTBNamespace& = CTBNamespace());
	AddOnApplication*		GetCurrentAddOnApp		(HTREEITEM* = NULL) const;
	AddOnModule*			GetCurrentAddOnModule	(HTREEITEM* = NULL) const;
	CDocumentDescription*	GetCurrentDocInfo		(HTREEITEM* = NULL) const;
	CTBNamespace			GetCurrentDocNamespace	(HTREEITEM* = NULL) const;
	CString					GetCurrentDocName		(HTREEITEM* = NULL) const;

	BOOL				SelItemFromNamespace		(const CTBNamespace&);

	void				SetReadOnly				(BOOL bReadOnly){m_bReadOnly = bReadOnly;};

public:
	//{{AFX_MSG(CAppDocumentsTreeCtrl)
	afx_msg void	OnLButtonDown	(UINT, CPoint);
	//}}AFX_MSG

	DECLARE_MESSAGE_MAP()
};



#include "endh.dex"

