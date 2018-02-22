#pragma once

#include <math.h>

#include <TbXmlCore\XMLSchema.h>
#include <TbNameSolver\ThreadContext.h>
#include <TbNameSolver\Observable.h>
#include <TbNameSolver\JsonSerializer.h>
#include <TbNameSolver\CallbackHandler.h>

#include "DatesFunctions.h"
#include "tbstrings.h"
#include "Array.h"
#include "Globals.h"
#include "CollateCultureFunctions.h"

//includere alla fine degli include del .H
#include "beginh.dex"
//===========================================================================

class EnumItemArray;
class DataType;
class DataObj;
class RDEData;
	class DataStr;
	class DataDate;
	class DataInt;
	class DataLng;
	class DataDbl;
	class DataMon;
	class DataQty;
	class DataPerc;
	class DataBool;
	class DataEnum;
	class DataTypeNamedArray;

class SqlTable;
class ISqlRecord;
class HotKeyLink;
//===========================================================================
typedef UINT	DataSize;

//===========================================================================

//spostato qui poich� viene usato anche in dataobj.cpp. Per non fare l'inclusione di parsobj.h
//in parsobj.cpp invece c'� gi� l'include di dataobj.h
#define UNDEF_FORMAT	-2		// e` usato dal costruttore di CParsedCtrl

// per la conversione da stringa a datatime utilizzata in fase di parsing-unparsing (anche per il 
// formato XML)
#define CONVERT_DATATIME_SUCCEEDED		0
#define CONVERT_DATATIME_FAILED			1
#define CONVERT_DATATIME_SYNTAX_ERROR	2

//serve per Oracle dove i DataStr sono visti come delle stringhe di lunghezza 38 (compreso le parentesi)
#define GUID_LEN 38

//the DATA_BOOL in a database column is stored as char(1). We also consider the '\0' so the real lenght to communicate
// to OLEDB Accessor is 2
#define BOOL_DBLENGHT 2
#define EPSILON_DECIMAL 7

//Ridefinite perche utilizzate anche dalla parte managed (MDataObj)
#define DATA_INT_MINVALUE SHRT_MIN
#define DATA_INT_MAXVALUE SHRT_MAX

#define DATA_LNG_MINVALUE LONG_MIN
#define DATA_LNG_MAXVALUE LONG_MAX

#define DATA_DBL_MINVALUE -DBL_MAX
#define DATA_DBL_MAXVALUE DBL_MAX

//===========================================================================
class TB_EXPORT MemoryLeakTrackNew : public CObject
{
	DECLARE_DYNAMIC(MemoryLeakTrackNew)

public:
	CString m_sFile;
	int m_nLine;

	MemoryLeakTrackNew(LPCSTR pszFile, int nLine)
		:
		m_sFile(pszFile),
		m_nLine (nLine)
		{}

// diagnostics
#ifdef _DEBUG
public:	
	void Dump(CDumpContext&) const;
#endif
};

//////////////////////////////////////////////////////////////////////////////

//----------------------------------------------------------------------------
// return tag decodification of data types.
TB_EXPORT CString		FromDataTypeToDescr			(const DataType& aDataType);	// language depented

//----------------------------------------------------------------------------
// used to convert string to datatime with a fixed format
// used in parsing and xmlparsing
TB_EXPORT int ConvertStringToDateTime (DataType aType, LPCTSTR lpszInput, DWORD* pValue1, DWORD* pValue2 = NULL);

//----------------------------------------------------------------------------
// ritorna il minimo valore positivo gestito per il datatype corrispondente
TB_EXPORT CString FromTBTypeToNetType(const DataType& aDataType);

//----------------------------------------------------------------------------
// return token of data types.
TB_EXPORT CString FromDataTypeToScriptType(const DataType& aDataType);

//----------------------------------------------------------------------------
// ritorna il nome del tipo .Net corrispondente al datatype corrispondente
TB_EXPORT double GetEpsilonForDataType (const DataType& aDataType) ;

//----------------------------------------------------------------------------
#define IsNullGUID(guid) IsEqualGUID(guid,NULL_GUID)

//////////////////////////////////////////////////////////////////////////////
typedef enum { 
	CMP_EQUAL, CMP_NOT_EQUAL, 
	CMP_LESSER_THEN, CMP_LESSER_OR_EQUAL, 
	CMP_GREATER_THEN, CMP_GREATER_OR_EQUAL,
	CMP_BEGIN_WITH, CMP_END_WITH,
	CMP_CONTAINS, CMP_NOT_CONTAINS,
	CMP_MATCH
} ECompareType;

BEGIN_TB_STRING_MAP(CompareTypeStrings)
	TB_LOCALIZED(CONTAINS,			"Contains")
	TB_LOCALIZED(DOESCONTAINS,		"Does not contain")
	TB_LOCALIZED(EQUAL,				"Equals")
	TB_LOCALIZED(NOTEQUAL,			"Does not equal")
	TB_LOCALIZED(BEGINSWITH,		"Begins with")
	TB_LOCALIZED(ENDSWITH,			"Ends with")

	TB_LOCALIZED(LESS,				"Less than")
	TB_LOCALIZED(LESSEQUAL,			"Less than or equal to")
	TB_LOCALIZED(GREATER,			"Greater than")
	TB_LOCALIZED(GREATEREQUAL,		"Greater than or equal to")
END_TB_STRING_MAP()

TB_EXPORT void FillCompareType(CComboBox*, const DataType&);
TB_EXPORT int SelectCompareType(CComboBox*, ECompareType);

//////////////////////////////////////////////////////////////////////////////
//					DataType definition
//////////////////////////////////////////////////////////////////////////////
//
#define DATA_NULL_TYPE  WORD(0)
#define DATA_STR_TYPE	WORD(1)
#define DATA_INT_TYPE	WORD(2)
#define DATA_LNG_TYPE	WORD(3)
#define DATA_DBL_TYPE	WORD(4)
#define DATA_MON_TYPE	WORD(5)
#define DATA_QTA_TYPE	WORD(6)
#define DATA_PERC_TYPE	WORD(7)
#define DATA_DATE_TYPE	WORD(8)
#define DATA_BOOL_TYPE	WORD(9)
#define DATA_ENUM_TYPE	WORD(10)
//#define DATA_SET_TYPE		WORD(11) removed
#define DATA_ARRAY_TYPE	WORD(11)
#define DATA_GUID_TYPE	WORD(12)
#define DATA_TXT_TYPE	WORD(13)
#define DATA_VARIANT_TYPE	WORD(14)
//TODO
#define DATA_RECORD_TYPE	WORD(15)
#define DATA_SQLRECORD_TYPE	WORD(16)	//map to a SqlRecord
#define DATA_BLOB_TYPE		WORD(17)	//NON implementato

//	WARNING! The DataType sequence must not be changed because
//	expression evaluation uses next #define
//allocate expression DATA_TYPE_OP_MAP
#define LAST_MAPPED_DATA_TYPE	DATA_BLOB_TYPE
//used in BOOL CDataObjTypesCombo::OnInitCtrl()
#define LAST_USED_DATA_TYPE		DATA_TXT_TYPE
//----
class TB_EXPORT DataTypeRecordDescr: public CObject
{
public:
	DataTypeNamedArray*		m_pRecordFields = NULL;	//record fields declaration
	CString					m_sName;	

	DataTypeRecordDescr() {}
	~DataTypeRecordDescr();
};
//-----------------------------

class TB_EXPORT DataType
{
public:
	WORD			m_wType = DATA_NULL_TYPE;	// datatype id
	//union {	//per evitare spreco di memoria
		WORD		m_wTag; // Some values from DataObj::DataStatus bitvector (FULLDATE, TIME, TB_HANDLE, TB_VOID) to select subtype (time, datetime, elapsedtime, object), or enum tag value
		//DataType*	m_pBaseType = NULL;	//array base datatype
	//};

public:
	DataType()									{ m_wType = DATA_NULL_TYPE; m_wTag = 0; }
	DataType(WORD wType, WORD wTag = WORD(0))	{ m_wType = wType; m_wTag = wTag; }
	DataType(const DataType& t)					{ *this = t; }
	DataType(const CString& st);	//st is the numeric datatype value as LONG
	~DataType();

public:
   	static  const DataType	Null;
   	static  const DataType	String;
   	static  const DataType	Integer;
   	static  const DataType	Long;
   	static  const DataType	Double;
   	static  const DataType	Money;
   	static  const DataType	Quantity;
   	static  const DataType	Percent;
   	static  const DataType	Date;
   	static  const DataType	DateTime;
   	static  const DataType	Time;
   	static  const DataType	ElapsedTime;
   	static  const DataType	Bool;
   	static  const DataType	Enum;
 	static  const DataType	Guid;
	static  const DataType	Text;
   	static  const DataType	Blob;
   	static  const DataType	Array;
  	static  const DataType	Variant;
   	static  const DataType	Object;
  	static  const DataType	Void;
 	static  const DataType	Record;
	static  const DataType	SqlRecord;

public:
	BOOL 	IsFullDate		()		const;
	BOOL 	IsATime			()		const;

	BOOL 	IsAVoid			()		const;
	BOOL 	IsAHandle		()		const;

	void	SetFullDate		(BOOL bFullDate		= TRUE);
	void	SetAsTime		(BOOL bIsTime		= TRUE);

	void operator=  (const DataType& t)		{ m_wType = t.m_wType; m_wTag = t.m_wTag; }
	void operator=  (WORD wType)			{ m_wType = wType; m_wTag = 0; }

	operator WORD	() const { return m_wType; }
	//verificare che non vada in contrasto con il precedente
	//operator DWORD	() const { return MAKELONG(m_wType, m_wTag); }

	CString	ToString() const;
	CString	FormatDefaultValue() const;

	BOOL IsNumeric() const;
	BOOL IsReal() const;

	friend BOOL operator== (const DataType& t1, const DataType& t2)		{ return t1.m_wType == t2.m_wType && t1.m_wTag == t2.m_wTag; }
	friend BOOL operator!= (const DataType& t1, const DataType& t2)		{ return !(t1 == t2); }
	friend BOOL operator== (const DataType& t1, const WORD& t2)			{ return t1.m_wType == t2; }
	friend BOOL operator!= (const DataType& t1, const WORD& t2)			{ return !(t1 == t2); }

	static BOOL IsCompatible(const DataType& dtFromType, const DataType& dtToType);
	static BOOL IsCompatible(VARTYPE vtFromType, const DataType& dtToType);
};

//////////////////////////////////////////////////////////////////////////////
//					DataObj definition
//////////////////////////////////////////////////////////////////////////////
//
class TB_EXPORT DataObj : public CContextObject
{
	DECLARE_DYNAMIC(DataObj)

	friend class SqlTable;
	friend class SqlRecord;
	friend class DataStr;
	friend class CTaskBuilderApp;
	friend class HotKeyLink;

protected:                                                                         
	DWORD	m_wDataStatus;
	SWORD	m_nSqlDataType;
	CArray<HotKeyLink*> m_arAlignedHKLs;//elenco di hotlink per cui la find � stata effettuata e non necessita di essere rifatta
public:
	BEGIN_TB_STRING_MAP(Strings)
		TB_LOCALIZED(YES, "Yes")
		TB_LOCALIZED(NO, "No")
	END_TB_STRING_MAP()

	enum DataStatus
	{
		DBCASE_COMPLIANT	= 0x0001,
		UPPERCASE			= 0x0002,

		FULLDATE			= 0x0004,	// usato come attributo del DATA_DATE_TYPE 
										// per indicare l'uso del Time (DataType::DateTime)
		TIME				= 0x0008,	// usato come attributo del DATA_DATE_TYPE 
										// per indicare che e` un Ora 
										// usato come attributo del DATA_LONG_TYPE
										// per indicare che e` un Tempo
		ACCOUNTABLE			= 0x0008,   // usato nel DATA_MON_TYPE per definire se l'importo � Accountable o Not Accountable
		TB_HANDLE			= 0x0004,	// usato come attributo del DATA_LONG_TYPE (DataType::Long) 
										// per indicare che il contenuto � un handle (DataType::Object)
		TB_VOID				= 0x0008,	// usato come attributo del DATA_NULL_TYPE (DataType::Null)
										// per indicare un valore di ritorno void (DataType::Void)
		
		READONLY			= 0x00010,	// per gestire il readonly nei controls dipendentemente allo stato del documento
		HIDE				= 0x00020,	// per gestire il hide/show dei controls
		FINDABLE			= 0x00040,	// abilita la ricerca nei documenti
		VALUE_CHANGED		= 0x00080,	// riservato ed utilizzabile dal programmatore
		VALID				= 0x00100,	// usato dal report engine
		MODIFIED			= 0x00200,	// riservato dalla gestione interna del documento
		DIRTY				= 0x00400,	// usato per ottimizzare i/o su database
		UPDATE_VIEW			= 0x00800,	// usato per forzare la rivisualizzazione del dato
		OSL_READONLY		= 0x01000,	// OSL: per gestire il readonly nei controls
		OSL_HIDE			= 0x02000,	// OSL: per gestire il hide/show dei controls
		ALWAYS_READONLY		= 0x04000,	// per gestire il readonly nei controls indipendentemente dallo stato del documento
		VALUE_LOCKED		= 0x08000,	// per impedire l'assegnazione di un nuovo valore al DataObj
		PRIVATE				= 0x10000,   // � un dato privato. Per la visualizzazione viene utilizzato il formattatore CPrivacyFormatter //utilizzato dalla RowSecurity.
										// @@BAUZI: non ho potuto usare OSL_HIHE poich� legato al control e non al dato come invece serve per la RowSecurity
		ALWAYS_EDITABLE		= 0x20000,	// @@PERA: per rendere il campo editabile anche se il documento � in stato di browse
		WEB_BOUND			= 0x40000,	// @@PERA: per segnalare che il campo � utilizzato nell'interfaccia web e quindi deve essere inserito nel model
		BPM_READONLY		= 0x80000	// readonly perch� gestito da processi di BPM
	};
public:
	// constructors & destructor
	DataObj ();
	virtual ~DataObj();

public:
	void	SetStatus	(BOOL bSet, DWORD aStatusFlag)
		{	m_wDataStatus = bSet? m_wDataStatus | aStatusFlag : m_wDataStatus & ~aStatusFlag ; }

	void	SetStatus	(BOOL bSet, DataStatus aStatusFlag)
		{	SetStatus (bSet, (DWORD)aStatusFlag); }
	// pure virtual functions
	virtual BOOL	IsEmpty		()  				const	= 0;
	virtual int		IsEqual     (const DataObj&)  	const	= 0;
	virtual int		IsLessThan	(const DataObj&)  	const	= 0;

	virtual CString		Str         (int = -1, int = -1) const	= 0;
	virtual CString		ToString    () const { return Str(); }
	//virtual CString		ToJson		(BOOL bracket = FALSE, BOOL escape = FALSE, BOOL quoted = TRUE) const ;

	virtual void*		GetRawData  (DataSize* = NULL)	const	= 0;

	virtual DataType	GetDataType	() const				= 0;

	//@@OLEDB
	//We consider bUnicode = TRUE if the database column is UNICODE instead bUnicode = FALSE
	//We use it in DataStr e DataBool
	virtual void		Assign		   (BYTE*, DBLENGTH, BOOL bUnicode = TRUE);
	virtual DBLENGTH	GetOleDBSize   (BOOL bUnicode = TRUE )	const	= 0;
	virtual void*		GetOleDBDataPtr()	const	= 0;
	
	virtual void	Assign      (LPCTSTR pszString)			= 0;
	virtual void	Assign      (const RDEData&)			= 0;
	virtual void	Assign      (const DataObj&)			= 0;
	virtual void	Assign      (LPCTSTR pszString, int /*nIndexFormat*/) { Assign(pszString);};
	virtual void	Assign		(const VARIANT& v) = 0;
	
	virtual void	AssignStatus(const DataObj&);
	virtual void	Clear       (BOOL bValid = TRUE);

	virtual	void	SetLowerValue	(int)			{}
	virtual	void	SetUpperValue	(int)			{}
	virtual	BOOL	IsLowerValue	() const		{ return FALSE; }
	virtual	BOOL	IsUpperValue	() const		{ return FALSE; }

	// utili per preallocare data member locali al singolo tipo di DataObj
	// ad oggi utilizzate solo dal DataStr e DataBlob
	virtual	void	Allocate	()				{};
	virtual	void	Allocate	(int nSize)		{ SetAllocSize(nSize); Allocate(); };
	virtual	void	SetAllocSize(int /*nSize*/)	{};
	virtual	int		GetAllocSize() const		{ return 0; }
	
	virtual	int		GetColumnLen() const		{ return 0; }
	virtual void	SetSqlDataType(SWORD sqlDBType) { m_nSqlDataType = sqlDBType; }
	virtual SWORD	GetSqlDataType() const { return m_nSqlDataType; } 

	virtual VARIANT ToVariant() const { VARIANT v; VariantInit(&v); return v; }

	virtual CContextObject* CloneContextObject() { return DataObjClone(); }
	virtual void SerializeJsonValue(CJsonSerializer& jsonSerializer);
	virtual void AssignJsonValue(CJsonParser& parser);

			void AssignFromJson(CJsonParser& parser);
			void SerializeToJson(CJsonSerializer& jsonSerializer);

			bool AlignHKL(HotKeyLink* pHKL);
	void SignalOnChanged();
/*
DataObjClone e DataObjCreate in debug simulano il comportamento della DEBUG_NEW 
che � abilitata di default in stdafx.h
Per tracciare la sorgente dell'istruzione di allocazione dell'oggetto viene utilizzato un oggetto apposito
di classe MemoryLeakTrackNew.
Il metodo Clone � lasciato per retrocompatibilit�
*/
	virtual DataObj*	Clone				() const;
#ifdef _DEBUG
	#undef DataObjClone
	virtual DataObj*	DataObjClone		(LPCSTR pszFile = NULL, int nLine = 0) const;
	#define DataObjClone() DataObjClone(THIS_FILE, __LINE__)
#else
	virtual DataObj*	DataObjClone		(/*LPCSTR pszFile = NULL, int nLine = 0*/) const;
#endif

#ifdef _DEBUG
	#undef DataObjCreate
	static  DataObj* DataObjCreate(const DataType& aType, LPCSTR pszFile = NULL, int nLine = 0);
	#define DataObjCreate(dt) DataObjCreate(dt, THIS_FILE, __LINE__)
#else
	static  DataObj* DataObjCreate(const DataType& aType/*, LPCSTR pszFile = NULL, int nLine = 0*/);
#endif

	// gestione XML
	// restituisce il tipo XML associato al DataObj
	virtual CString GetXMLType(BOOL bSoapType = TRUE) const = 0;	
	// effettua l'unparsing ed il parsing di un dato xml 
	virtual CString FormatDataForXML(BOOL bSoapType = TRUE) const = 0;
	virtual void	AssignFromXMLString(LPCTSTR) = 0;

	// sono reimplementati nelle classi che gestiscono tipi blob: DataText
	virtual void	AppendFromSStream(BYTE* pBuf, int nLen, BOOL bUnicode, BOOL bSet = FALSE) {}

public:
	// non overridable functions
	BOOL 	IsValid				()		const;
	BOOL 	IsModified			()		const;
	BOOL 	IsUpperCase			()		const;
	BOOL 	IsDBCaseCompliant	()		const;
	BOOL 	IsValueChanged		()		const;
	BOOL 	IsDirty				()		const;
	BOOL 	IsFindable			()		const;
	BOOL 	IsFullDate			()		const;
	BOOL 	IsATime				()		const;
	BOOL 	IsAccountable		()		const;
	BOOL 	IsUpdateView		()		const;

	BOOL	IsReadOnly			(DWORD flags = READONLY|OSL_READONLY|ALWAYS_READONLY|BPM_READONLY) const
													{ return ((m_wDataStatus & flags) != 0) && ((m_wDataStatus & ALWAYS_EDITABLE) == 0); }
	BOOL	IsStateReadOnly		()		const		{ return IsReadOnly(READONLY); }
	BOOL	IsOSLReadOnly		()		const		{ return IsReadOnly(OSL_READONLY); }
	BOOL	IsBPMReadOnly		()		const		{ return IsReadOnly(BPM_READONLY); }
	BOOL	IsAlwaysReadOnly	()		const		{ return IsReadOnly(ALWAYS_READONLY); }
	
	BOOL	IsHide				(DWORD flags = HIDE|OSL_HIDE/*|PRIVATE*/) const							
													{ return (m_wDataStatus & flags) != 0; }
	BOOL	IsStateHide			()		const		{ return IsHide(HIDE); }
	BOOL	IsOSLHide			()		const		{ return IsHide(OSL_HIDE); }

	BOOL 	IsValueLocked		()	const;
	BOOL 	IsHandle			()	const;

	BOOL	IsPrivate			() const; //@@TBRowSecurity
	BOOL	IsWebBound() const { return (m_wDataStatus & WEB_BOUND) == WEB_BOUND; }

	DWORD	GetStatus			()		const { return m_wDataStatus; }
	
	virtual CString	FormatData			(LPCTSTR pszFormatName = NULL) const;

	void SetValid			(BOOL bValid		= TRUE);
	void SetModified		(BOOL bModified		= TRUE);
	void SetUpperCase		(BOOL bUpperCase	= TRUE);
	void SetDBCaseCompliant	(BOOL bDBCaseComp	= TRUE);
	void SetValueChanged	(BOOL bChanged		= TRUE);
	void SetDirty			(BOOL bDirty		= TRUE);
	void SetFindable		(BOOL bFindable		= TRUE);
	void SetFullDate		(BOOL bFullDate		= TRUE);
	void SetAsTime			(BOOL bIsTime		= TRUE);
	void SetAsHandle		(BOOL bHandle		= TRUE);
	void SetAccountable		(BOOL bValue		= TRUE);
	void SetUpdateView		(BOOL bToBeUpdated	= TRUE);

	void SetReadOnly		(BOOL bReadOnly		= TRUE);
	void SetHide			(BOOL bHide			= TRUE);
	void SetOSLHide			(BOOL bHide			= TRUE);
	void SetOSLReadOnly		(BOOL bReadOnly		= TRUE);
	void SetBPMReadOnly		(BOOL bReadOnly		= TRUE);
	void SetAlwaysReadOnly	(BOOL bReadOnly		= TRUE);
	void SetAlwaysEditable	(BOOL bEditable		= TRUE);
	void SetValueLocked		(BOOL bValueLocked	= TRUE);

	void SetPrivate			(BOOL bPrivate = TRUE);
	void SetWebBound		(BOOL bSet = TRUE) { SetStatus(bSet, WEB_BOUND); }
	virtual void SetCollateCultureSensitive	(bool bSensitive = true) {}

public:
	// non overridable operatori di assegnazione
	virtual bool IsCollateCultureSensitive	()				const	{ return false;	} //di default non sono sensibile alla collate culture, a meno che sia una stringa...
	virtual int	IsLessEqualThan		(const DataObj& aData)  const	{ return IsLessThan(aData) || IsEqual(aData); }
	virtual int	IsGreaterThan		(const DataObj& aData)  const	{ return !IsLessEqualThan(aData); }
	virtual int	IsGreaterEqualThan	(const DataObj& aData)  const	{ return !IsLessThan(aData); }

	virtual BOOL operator==(const DataObj& aData) { return IsEqual(aData); }
	virtual BOOL operator!=(const DataObj& aData) { return ! IsEqual(aData); }

	virtual BOOL operator<	(const DataObj& aData) { return IsLessThan(aData); }
	virtual BOOL operator<=	(const DataObj& aData) { return IsLessEqualThan(aData); }
	virtual BOOL operator>	(const DataObj& aData) { return IsGreaterThan(aData); }
	virtual BOOL operator>=	(const DataObj& aData) { return IsGreaterEqualThan(aData); }

	const DataObj& operator=(const DataObj& o)	{ Assign(o);		return *this; }

	BOOL CompareBy(ECompareType cmp, DataObj*, BOOL bCompareNoCase = TRUE);
	BOOL CompareBy(ECompareType cmp, const CStringArray& sCmpValues, DataObj* pPreAllocatedObj = NULL, BOOL bCompareNoCase = TRUE);

// diagnostics
#ifdef _DEBUG
public:	
	void Dump(CDumpContext&) const;
	void AssertValid() const;
	MemoryLeakTrackNew* m_pNewed;
#endif
};

class Formatter;
//============================================================================
class TB_EXPORT DataStr : public DataObj
{
	DECLARE_DYNCREATE (DataStr)

	friend class SqlTable;
	friend class SqlRecord;

protected:
	CString		m_strText;
	int			m_nAllocSize;
    bool		m_bCollateCultureSensitive;

public:
	// constructors & destructor
	DataStr ();
	DataStr (LPCTSTR);
	DataStr (const DataStr&);
	DataStr (const CString&);
	~DataStr ();
public:
	// overridable
	virtual BOOL		IsEmpty		()  const { return m_strText.IsEmpty(); }
	virtual int         IsEqual     (const DataObj&)	const;
	virtual int         IsLessThan  (const DataObj&)	const;
	
			int         CompareNoCase     (const DataStr&)	const;

	// Il primo parametro indica che si devono tornare un numero di caratteri
	// ben determinato (troncando o "paddando" a destra di spazi)
	virtual CString     Str         (int = -1, int = -1) const;
	virtual CString     ToString    () const;
	
	virtual void*       GetRawData  (DataSize* = NULL)	const;
	virtual DataType    GetDataType	() const { return DataType(DATA_STR_TYPE, 0); }

	//@@OLEDB
	// I need '\0' char to end the string
	// We consider bUnicode = TRUE if the database column is UNICODE instead bUnicode = FALSE
	// so to consider the real string bytes to comunicate to the OLEDB accessor buffer
	virtual DBLENGTH	GetOleDBSize  (BOOL bUnicode = TRUE)	const 
		{ return (bUnicode) ? GetAllocSize() + sizeof(TCHAR) : m_nAllocSize + sizeof(CHAR); }
	
	virtual void*		GetOleDBDataPtr() const { return (void*)((LPCTSTR)m_strText); }

	virtual void		Assign		(BYTE*, DBLENGTH, BOOL bUseUnicode);
	virtual void		Assign      (LPCTSTR);
	virtual void		Assign      (const RDEData&);
	virtual void		Assign     	(const DataObj&);
	virtual void		Assign      (TCHAR);
	virtual void		Assign      (const VARIANT&);

	virtual void    Clear       (BOOL bValid = TRUE);

	virtual	void	SetLowerValue	(int);
	virtual	void	SetUpperValue	(int);
	virtual	BOOL	IsLowerValue	() const;
	virtual	BOOL	IsUpperValue	() const;

	virtual bool IsCollateCultureSensitive() const							{ return m_bCollateCultureSensitive; }
	virtual void SetCollateCultureSensitive	(bool bSensitive = true)	{ m_bCollateCultureSensitive = bSensitive; }

	// Permettono la gestione della riallocazione della stringa alla size voluta per
	// gestire correttamente il rebind alle colonne di database in ODBC
	virtual	void	Allocate	();
	virtual	void	SetAllocSize(int nSize)		{ m_nAllocSize = nSize; }
	virtual	int		GetAllocSize()  const		{ return m_nAllocSize * sizeof(TCHAR); }
	
	virtual	int		GetColumnLen() const		{ return m_nAllocSize; }

	virtual VARIANT ToVariant() const { VARIANT v; v.vt = VT_BSTR; v.bstrVal = m_strText.AllocSysString(); return v; }
	
	virtual CString GetXMLType(BOOL bSoapType = TRUE) const { return SCHEMA_XSD_DATATYPE_STRING_VALUE;} ;	
	virtual CString FormatDataForXML(BOOL bSoapType = TRUE) const;
	virtual void	AssignFromXMLString(LPCTSTR);

	BSTR GetSoapValue()					{ return m_strText.AllocSysString(); }
	void SetSoapValue(BSTR bstrText)	{ Assign(CString(bstrText)); }

protected:
	void	MakeUpper	(BOOL bSignal);
	void	MakeLower	(BOOL bSignal);

public:
	// non overridable
	int             GetLen		() const	{ return m_strText.GetLength();	}
	const CString&	GetString	() const	{ return m_strText;				}

	void	SetAt		(int i, TCHAR c);
	void	StripBlank	();
	void	Append(TCHAR c, BOOL duplicate = FALSE);

	CString	Trim		() const;
	CString	Ltrim		() const;
	CString	Rtrim		() const;

	void	TrimUpperLimit ();

	void	MakeUpper	() { MakeUpper(TRUE); }
	void	MakeLower	() { MakeLower(TRUE); }
	CString	GetUpperCase();
	CString	GetLowerCase();

public:
	// (CString compatible) Access to string implementation buffer as "C" character array
	TCHAR*	GetBuffer			(int nMinBufLength)		{ return m_strText.GetBuffer(nMinBufLength); }
	void	ReleaseBuffer		(int nNewLength = -1)	{ m_strText.ReleaseBuffer(nNewLength); }
	TCHAR*	GetBufferSetLength	(int nNewLength)		{ return m_strText.GetBufferSetLength(nNewLength); }
	int		GetAllocLength		() const				{ return m_strText.GetAllocLength(); }

public:
	// non overridable operatori di assegnazione
	TCHAR	operator	[]		(int i)	const	{ return m_strText[i]; }
	
	virtual operator CString	()		const	{ return m_strText; }
	LPCTSTR	GetPtrString()		const	{ return (LPCTSTR) m_strText; }

public:
	// operatori di assegnazione
	const DataStr& operator=(const DataStr& datastr)	{ Assign (datastr);	return *this; }
	const DataStr& operator=(const DataObj& i)			{ Assign (i);		return *this; } 
	const DataStr& operator=(const CString& string)		{ Assign (string);	return *this; }
	const DataStr& operator=(TCHAR ch)					{ Assign (ch);		return *this; }
	const DataStr& operator=(LPCTSTR psz)				{ Assign (psz);		return *this; }

	// operatori di assegnazione ed incremento
	const DataStr& operator+=(const DataStr& datastr);
	const DataStr& operator+=(const CString& string);
	const DataStr& operator+=(TCHAR ch);
	const DataStr& operator+=(LPCTSTR psz);

TB_EXPORT 	friend DataStr operator+(const DataStr&	s1, const DataStr&	s2);
TB_EXPORT 	friend DataStr operator+(const DataStr&	s1, const CString&	s2);
TB_EXPORT 	friend DataStr operator+(const CString&	s1,	const DataStr&	s2);
TB_EXPORT 	friend DataStr operator+(const DataStr&	s1,	TCHAR			s2);
TB_EXPORT 	friend DataStr operator+(TCHAR 			s1,	const DataStr&	s2);
TB_EXPORT 	friend DataStr operator+(const DataStr&	s1,	LPCTSTR			s2);
TB_EXPORT 	friend DataStr operator+(LPCTSTR			s1,	const DataStr&	s2);

	// operatori di confronto
TB_EXPORT 	friend BOOL operator==(const DataStr&	s1,	const DataStr&	s2);
TB_EXPORT 	friend BOOL operator==(const DataStr&	s1,	const CString&	s2);
TB_EXPORT 	friend BOOL operator==(const CString&	s1,	const DataStr&	s2);
TB_EXPORT 	friend BOOL operator==(const DataStr&	s1,	LPCTSTR			s2);
TB_EXPORT 	friend BOOL operator==(LPCTSTR			s1, const DataStr&	s2);
	
TB_EXPORT	friend BOOL operator<(const DataStr&	s1, const DataStr&	s2);
TB_EXPORT	friend BOOL operator<(const DataStr&	s1, const CString&	s2);
TB_EXPORT	friend BOOL operator<(const CString&	s1, const DataStr&	s2);
TB_EXPORT	friend BOOL operator<(const DataStr&	s1, LPCTSTR			s2);
TB_EXPORT	friend BOOL operator<(LPCTSTR			s1, const DataStr&	s2);

TB_EXPORT 	friend BOOL operator>(const DataStr&	s1, const DataStr&	s2);
TB_EXPORT 	friend BOOL operator>(const DataStr&	s1, const CString&	s2);
TB_EXPORT 	friend BOOL operator>(const CString&	s1, const DataStr&	s2);
TB_EXPORT 	friend BOOL operator>(const DataStr&	s1, LPCTSTR			s2);
TB_EXPORT 	friend BOOL operator>(LPCTSTR			s1, const DataStr&	s2);

TB_EXPORT	friend BOOL operator!=(const DataStr&	s1,	const DataStr&	s2) { return !(s1 == s2); }
TB_EXPORT	friend BOOL operator!=(const DataStr&	s1,	const CString&	s2) { return !(s1 == s2); }
TB_EXPORT	friend BOOL operator!=(const CString&	s1,	const DataStr&	s2) { return !(s1 == s2); }
TB_EXPORT	friend BOOL operator!=(const DataStr&	s1,	LPCTSTR			s2) { return !(s1 == s2); }
TB_EXPORT	friend BOOL operator!=(LPCTSTR			s1,	const DataStr&	s2) { return !(s1 == s2); }

TB_EXPORT	friend BOOL operator<=(const DataStr&	s1, const DataStr&	s2) { return !(s1 > s2); }
TB_EXPORT	friend BOOL operator<=(const DataStr&	s1, const CString&	s2) { return !(s1 > s2); }
TB_EXPORT	friend BOOL operator<=(const CString&	s1, const DataStr&	s2) { return !(s1 > s2); }
TB_EXPORT	friend BOOL operator<=(const DataStr&	s1, LPCTSTR			s2) { return !(s1 > s2); }
TB_EXPORT	friend BOOL operator<=(LPCTSTR			s1, const DataStr&	s2) { return !(s1 > s2); }

TB_EXPORT	friend BOOL operator>=(const DataStr&	s1, const DataStr&	s2) { return !(s1 < s2); }
TB_EXPORT	friend BOOL operator>=(const DataStr&	s1, const CString&	s2) { return !(s1 < s2); }
TB_EXPORT	friend BOOL operator>=(const CString&	s1, const DataStr&	s2) { return !(s1 < s2); }
TB_EXPORT	friend BOOL operator>=(const DataStr&	s1, LPCTSTR			s2) { return !(s1 < s2); }
TB_EXPORT	friend BOOL operator>=(LPCTSTR			s1, const DataStr&	s2) { return !(s1 < s2); }

// diagnostics
#ifdef _DEBUG
public:	
	void Dump(CDumpContext&) const;
	void AssertValid() const;
#endif
};

//============================================================================
class TB_EXPORT DataBool : public DataObj
{
	DECLARE_DYNCREATE (DataBool)

	friend class SqlTable;
	friend class SqlRecord;
	friend class CRSBoolProp;
	friend class CRS_ObjectPropertyView;
	friend class CrsGridStyleProp;

protected:
	BOOL m_bValue;
	
public:
	// constructors & destructor
	DataBool (const BOOL = FALSE);
	DataBool (const DataBool&);

public:
	// overridable
	virtual BOOL		IsEmpty		()  const { return !m_bValue; }
	virtual int         IsEqual     (const DataObj&)	const;
	virtual int         IsLessThan  (const DataObj&)	const;

	// Il primo parametro indica:
	//	se <= 0 che si deve tornare "TRUE" o "FALSE"
	//	se > 0 che si deve tornare "Si" o "No"
	virtual CString     Str         (int = -1, int = -1)const;

	virtual void*       GetRawData  (DataSize* = NULL)	const;
	virtual DataType    GetDataType () const { return DataType(DATA_BOOL_TYPE, 0); }
	
	//@@OLEDB
	// We consider bUnicode = TRUE if the database column is UNICODE instead bUnicode = FALSE
	// so to consider the real string bytes to comunicate to the OLEDB accessor buffer
	virtual DBLENGTH	GetOleDBSize  (BOOL bUnicode = TRUE) const 
		{ return (bUnicode) ?  BOOL_DBLENGHT * sizeof(TCHAR) : BOOL_DBLENGHT;}
	virtual void*	GetOleDBDataPtr() const { return (void*)(&m_bValue); }

	virtual void	Assign	(BYTE*, DBLENGTH = 0, BOOL = TRUE);
	virtual void	Assign	(LPCTSTR);
	virtual void	Assign	(const RDEData&);
	virtual void	Assign	(const DataObj&);
	virtual void	Assign	(const BOOL);
	virtual void	Assign	(const VARIANT& v);

	virtual void	Clear	(BOOL bValid = TRUE);

public:
	// operatori di cast
	virtual operator const BOOL	() const	{ return m_bValue; }
	explicit operator const Bool3() const { return m_bValue ? B_TRUE : B_FALSE; }
	explicit operator const bool() const { return m_bValue == TRUE; }
	virtual VARIANT ToVariant() const;

	virtual CString GetXMLType(BOOL bSoapType = TRUE) const { return SCHEMA_XSD_DATATYPE_STRING_VALUE;} ;	
	virtual CString FormatDataForXML(BOOL bSoapType = TRUE) const;
	virtual void	AssignFromXMLString(LPCTSTR);
	virtual void	SerializeJsonValue(CJsonSerializer& jsonSerializer);
	virtual void	AssignJsonValue(CJsonParser& parser);

#pragma warning(disable:4800)
	bool GetSoapValue()					{ return m_bValue; }
#pragma warning(default:4800)
	void SetSoapValue(BOOL bValue)		{ Assign(bValue); }
public:
	// operatori di assegnazione
	const DataBool& operator=(const DataBool& db)	{ Assign(db);	return *this; }
	//const DataBool& operator=(const DataObj& i)		{ Assign(i);		return *this; }
	const DataBool& operator=(const BOOL& b)		{ Assign(b);	return *this; }
	const DataBool& operator=(LPCTSTR psz)			{ Assign(psz);	return *this; }

	// operatori di confronto
	friend BOOL operator==(const DataBool&	b1,	const DataBool&	b2) { return b1.m_bValue	== b2.m_bValue;	}
	friend BOOL operator==(const DataBool&	b1,	const BOOL&		b2) { return b1.m_bValue	== b2;			}
	friend BOOL operator==(const BOOL&		b1,	const DataBool&	b2) { return b1 			== b2.m_bValue;	}

// diagnostics
#ifdef _DEBUG
public:	
	void Dump(CDumpContext&) const;
	void AssertValid() const;
#endif
};

#pragma warning( disable : 4244 )
//============================================================================
class TB_EXPORT DataInt : public DataObj
{
	DECLARE_DYNCREATE (DataInt)

	friend class DataLng;
	friend class DataDbl;
	friend class SqlTable;
	friend class SqlRecord;
	friend class CRSShortProp;
	friend class CRSIniProp;
	friend class CRS_ObjectPropertyView;

protected:
	short	m_nValue;

public:
	// constructors & destructor
	DataInt (const int nValue = 0);
	DataInt (const DataInt&);

public:
	// overridable
	virtual BOOL		IsEmpty		() const { return m_nValue == 0; }
	virtual int         IsEqual     (const DataObj&)	const;
	virtual int         IsLessThan  (const DataObj&)	const;

	// Il primo parametro indica:
	//	se <= 0 che si deve tornare il numero convertito
	//	se > 0 che indica il numero di cifre da ritornare:
	//  in tal caso se si indica un secondo parametro = 0 significa
	//	che si vuole il numero "paddato" a sinistra di '0'
	virtual CString     Str         (int = -1, int = -1)const;

	virtual void*       GetRawData  (DataSize* = NULL)	const;
	virtual DataType    GetDataType	() const { return DataType(DATA_INT_TYPE, 0); }
	
	//@@OLEDB 
	virtual DBLENGTH	GetOleDBSize   (BOOL /*bUnicode*/ = TRUE) const { return sizeof(m_nValue);}
	virtual void*		GetOleDBDataPtr() const { return (void*)(&m_nValue); }

	virtual void    Assign	(LPCTSTR);
	virtual void	Assign	(const RDEData&);
	virtual void    Assign	(const DataObj&);
	virtual void    Assign	(const short);
	virtual void    Assign	(const VARIANT&);

	virtual void    Clear	(BOOL bValid = TRUE);
    
	virtual	void		SetLowerValue	(int);
	virtual	void		SetUpperValue	(int);
	virtual	BOOL		IsLowerValue	() const;
	virtual	BOOL		IsUpperValue	() const;

public:
	// operatori di cast
	virtual operator const	double      () const	{ return m_nValue; }
	virtual operator const	long        () const	{ return m_nValue; }
	virtual operator const	int         () const	{ return m_nValue; }
	virtual operator const	short       () const	{ return m_nValue; }
	virtual VARIANT ToVariant() const	{ VARIANT v; v.vt = VT_I2; v.iVal = m_nValue; return v; }

	virtual CString GetXMLType(BOOL bSoapType = TRUE) const { return SCHEMA_XSD_DATATYPE_SHORT_VALUE;} ;	
	virtual CString FormatDataForXML(BOOL bSoapType = TRUE) const;
	virtual void	AssignFromXMLString(LPCTSTR);
	virtual void	SerializeJsonValue(CJsonSerializer& jsonSerializer);
	virtual void	AssignJsonValue(CJsonParser& parser);

	int GetSoapValue()					{ return m_nValue; }
	void SetSoapValue(int nValue)		{ Assign(nValue); }
	
	static   DataInt    MINVALUE;
	static   DataInt    MAXVALUE;

public:
	// operatori di assegnazione
	const DataInt& operator=(const DataInt& di)	{ Assign(di);	return *this; }
	//const DataInt& operator=(const DataObj& i)	{ Assign(i);	return *this; }
	const DataInt& operator=(const int& i)		{ Assign(i);	return *this; }
	const DataInt& operator=(LPCTSTR psz)		{ Assign(psz);	return *this; }
	
	const DataInt& operator+=(const DataInt& di){ Assign(m_nValue + di.m_nValue); return *this; }
	const DataInt& operator+=(const int& i)		{ Assign(m_nValue + i);			 return *this; }

	const DataInt& operator-=(const DataInt& di){ Assign(m_nValue - di.m_nValue); return *this; }
	const DataInt& operator-=(const int& i)		{ Assign(m_nValue - i); return *this; }
	
	const DataInt& operator*=(const DataInt& di){ Assign(m_nValue * di.m_nValue); return *this; }
	const DataInt& operator*=(const int& i)		{ Assign(m_nValue * i); return *this; }

	const DataInt& operator/=(const DataInt& di){ Assign(m_nValue / di.m_nValue); return *this; }
	const DataInt& operator/=(const int& i)		{ Assign(m_nValue / i); return *this; }
	
	const DataInt& operator%=(const DataInt& di){ Assign(m_nValue % di.m_nValue); return *this; }
	const DataInt& operator%=(const int& i)		{ Assign(m_nValue % i); return *this; }

	const DataInt& operator&=(const DataInt& di){ Assign(m_nValue & di.m_nValue); return *this; }
	const DataInt& operator&=(const int& i)		{ Assign(m_nValue & i); return *this; }

	const DataInt& operator|=(const DataInt& di){ Assign(m_nValue | di.m_nValue); return *this; }
	const DataInt& operator|=(const int& i)		{ Assign(m_nValue | i); return *this; }

	const DataInt& operator^=(const DataInt& di){ Assign(m_nValue ^ di.m_nValue); return *this; }
	const DataInt& operator^=(const int& i)		{ Assign(m_nValue ^ i); return *this; }
	
	// operatori di incremento/decremento
	DataInt operator++()	{ Assign(m_nValue+1); return *this;}
	DataInt operator++(int)	{ int i = m_nValue; Assign(m_nValue+1); return i;}
	DataInt operator--()	{ Assign(m_nValue-1); return *this;}
	DataInt operator--(int)	{ int i = m_nValue; Assign(m_nValue-1); return i;}

	// operatori aritmentici
	friend DataInt operator+(const DataInt& i1, const DataInt&	i2)	{ return i1.m_nValue	+ i2.m_nValue;	}
	friend DataInt operator+(const DataInt& i1, const int&		i2)	{ return i1.m_nValue	+ i2;			}
	friend DataInt operator+(const int&		i1, const DataInt&	i2)	{ return i1				+ i2.m_nValue;	}
	
	friend DataInt operator-(const DataInt& i1)						{ return -i1.m_nValue;					}
	friend DataInt operator-(const DataInt& i1, const DataInt&	i2)	{ return i1.m_nValue	- i2.m_nValue;	}
	friend DataInt operator-(const DataInt& i1, const int&		i2)	{ return i1.m_nValue	- i2;			}
	friend DataInt operator-(const int&		i1, const DataInt&	i2)	{ return i1				- i2.m_nValue;	}
	
	friend DataInt operator*(const DataInt& i1, const DataInt&	i2)	{ return i1.m_nValue	* i2.m_nValue;	}
	friend DataInt operator*(const DataInt& i1, const int&		i2)	{ return i1.m_nValue	* i2;			}
	friend DataInt operator*(const int&		i1, const DataInt&	i2)	{ return i1				* i2.m_nValue;	}
	
	friend DataInt operator/(const DataInt& i1, const DataInt&	i2)	{ return i1.m_nValue	/ i2.m_nValue;	}
	friend DataInt operator/(const DataInt& i1, const int&		i2)	{ return i1.m_nValue	/ i2;			}
	friend DataInt operator/(const int&		i1, const DataInt&	i2)	{ return i1				/ i2.m_nValue;	}

	friend DataInt operator%(const DataInt& i1, const DataInt&	i2)	{ return i1.m_nValue	% i2.m_nValue;	}
	friend DataInt operator%(const DataInt& i1, const int&		i2)	{ return i1.m_nValue	% i2;			}
	friend DataInt operator%(const int&		i1, const DataInt&	i2)	{ return i1				% i2.m_nValue;	}

	// operatori bitwise
	friend DataInt operator&(const DataInt& i1, const DataInt&	i2)	{ return i1.m_nValue	& i2.m_nValue;	}
	friend DataInt operator&(const DataInt& i1, const int&		i2)	{ return i1.m_nValue	& i2;			}
	friend DataInt operator&(const int&		i1, const DataInt&	i2)	{ return i1				& i2.m_nValue;	}

	friend DataInt operator|(const DataInt& i1, const DataInt&	i2)	{ return i1.m_nValue	| i2.m_nValue;	}
	friend DataInt operator|(const DataInt& i1, const int&		i2)	{ return i1.m_nValue	| i2;			}
	friend DataInt operator|(const int&		i1, const DataInt&	i2)	{ return i1				| i2.m_nValue;	}

	friend DataInt operator^(const DataInt& i1, const DataInt&	i2)	{ return i1.m_nValue	^ i2.m_nValue;	}
	friend DataInt operator^(const DataInt& i1, const int&		i2)	{ return i1.m_nValue	^ i2;			}
	friend DataInt operator^(const int&		i1, const DataInt&	i2)	{ return i1				^ i2.m_nValue;	}

	// operatori di confronto
	friend BOOL operator==(const DataInt&	i1,	const DataInt&	i2) { return i1.m_nValue	== i2.m_nValue;	}
	friend BOOL operator==(const DataInt&	i1,	const int&		i2) { return i1.m_nValue	== i2;			}
	friend BOOL operator==(const int&		i1,	const DataInt&	i2) { return i1 			== i2.m_nValue;	}
	
	friend BOOL operator< (const DataInt&	i1, const DataInt&	i2) { return i1.m_nValue	< i2.m_nValue;	}
	friend BOOL operator< (const DataInt&	i1, const int&		i2) { return i1.m_nValue	< i2;			}
	friend BOOL operator< (const int&		i1, const DataInt&	i2) { return i1				< i2.m_nValue;	}

	friend BOOL operator> (const DataInt&	i1, const DataInt&	i2) { return i1.m_nValue	> i2.m_nValue;	}
	friend BOOL operator> (const DataInt&	i1, const int&		i2) { return i1.m_nValue	> i2;			}
	friend BOOL operator> (const int&		i1, const DataInt&	i2) { return i1				> i2.m_nValue;	}
                                                                   
	friend BOOL operator!=(const DataInt&	i1,	const DataInt&	i2) { return i1.m_nValue	!= i2.m_nValue; }
	friend BOOL operator!=(const DataInt&	i1,	const int&		i2) { return i1.m_nValue	!= i2;			}
	friend BOOL operator!=(const int&		i1,	const DataInt&	i2) { return i1				!= i2.m_nValue; }

	friend BOOL operator<=(const DataInt&	i1, const DataInt&	i2) { return i1.m_nValue	<= i2.m_nValue;	}
	friend BOOL operator<=(const DataInt&	i1, const int&		i2) { return i1.m_nValue	<= i2;			}
	friend BOOL operator<=(const int&		i1, const DataInt&	i2) { return i1				<= i2.m_nValue;	}

	friend BOOL operator>=(const DataInt&	i1, const DataInt&	i2) { return i1.m_nValue	>= i2.m_nValue;	}
	friend BOOL operator>=(const DataInt&	i1, const int&		i2) { return i1.m_nValue	>= i2;			}
	friend BOOL operator>=(const int&		i1, const DataInt&	i2) { return i1				>= i2.m_nValue; }

// diagnostics
#ifdef _DEBUG
public:	
	void Dump(CDumpContext&) const;
	void AssertValid() const;
#endif
};
#pragma warning( default : 4244 )


//============================================================================
class TB_EXPORT DataLng : public DataObj
{
	DECLARE_DYNCREATE (DataLng)

	friend class DataInt;
	friend class DataDbl;
	friend class SqlTable;
	friend class SqlRecord;
    
protected:
	long	m_nValue;
	
public:
	// constructors & destructor
	DataLng (const long = 0L);
	DataLng (const DataLng&);
	DataLng (void*, BOOL /*isHandle = TRUE*/);

	// costruisce un DataLng come ElapsedTime
	DataLng (long lDays, long nHours, long nMins, long nSecs);	//@@ElapsedTime

public:
	// overridable
	virtual BOOL		IsEmpty		() const { return m_nValue == 0L; }
	virtual int         IsEqual     (const DataObj&)	const;
	virtual int         IsLessThan  (const DataObj&)	const;

	// Il primo parametro puo` assumere 2 significati:
	// 1)	nel caso si tratti di un ElapsedTime puo` essere:
	//		>= 1,	per ottenere una formattazione dipendente da un formattatore
	//				(opzionalmente indicato	nel secondo parametro)
	//		< 0		(default) per ottenere la formattazione standard della sintassi
	//				{et"D:H:M:S"}
	//		= 0		per ottenere la formattazione compatta "etDDDDDHHMMSS"
	//
	// 2)	nel caso si tratti di normale long indica
	//		se <= 0 che si deve tornare il numero convertito
	//		se > 0 che indica il numero di cifre da ritornare:
	//			in tal caso se si indica un secondo parametro = 0 significa
	//			che si vuole il numero "paddato" a sinistra di '0'
	virtual CString     Str         (int = -1, int = -1) const;
	virtual CString		ToString    () const;

	virtual void*       GetRawData  (DataSize* = NULL)	const;
	virtual DataType    GetDataType	() const { return DataType(DATA_LNG_TYPE, unsigned int(m_wDataStatus & (TIME|TB_HANDLE))); }
	
	//@@OLEDB 
	virtual DBLENGTH	GetOleDBSize   (BOOL /*bUnicode*/ = TRUE) const { return sizeof(m_nValue);}
	virtual void*		GetOleDBDataPtr() const { return (void*)(&m_nValue); }

	virtual void	Assign	(LPCTSTR);
	virtual void	Assign	(const RDEData&);
	virtual void	Assign	(const DataObj&);
	virtual void	Assign	(const long);
	virtual void	Assign	(LPCTSTR, int nFormatIdx);
	virtual void	Assign	(const VARIANT&);

	virtual void	Clear	(BOOL bValid = TRUE);

	virtual	void		SetLowerValue	(int);
	virtual	void		SetUpperValue	(int);
	virtual	BOOL		IsLowerValue	() const;
	virtual	BOOL		IsUpperValue	() const;

public:
	// operatori di cast
	virtual operator const double   ()	const { return m_nValue; }
	virtual operator const long		()	const { return m_nValue; }

	virtual VARIANT ToVariant()	const; //{ VARIANT v; v.vt = VT_I4; v.lVal = m_nValue; return v; }

	virtual CString GetXMLType(BOOL bSoapType = TRUE) const { return SCHEMA_XSD_DATATYPE_INT_VALUE;} ;	
	virtual CString FormatDataForXML(BOOL bSoapType = TRUE) const;
	virtual void	AssignFromXMLString(LPCTSTR);
	virtual void	SerializeJsonValue(CJsonSerializer& jsonSerializer);
	virtual void	AssignJsonValue(CJsonParser& parser);

	long GetSoapValue()					{ return m_nValue; }
	void SetSoapValue(long nValue)		{ Assign(nValue); }

	static  const DataLng    MINVALUE;
	static  const DataLng    MAXVALUE;

public:
	//@@ElapsedTime funzioni per DataLng come ElapsedTime
	// numero di secondi totali della sola parte hh:mm:ss ignorando la data
	void	Assign			(long lDays, long nHours, long nMins, double nSecs);
	void	Assign			(COleDateTime&);

	void	SetElapsedTime	(const DataDate&, const DataDate&);
	long	GetDays			()	const;
	long	GetTotalHours	()	const;
	long	GetHours		()	const;
	long	GetTotalMinutes	()	const;
	long	GetMinutes		()	const;
	double	GetTotalSeconds	()	const;
	double	GetSeconds		()	const;
	double	GetDecDays		()	const;
	double	GetDecHours		()	const;
	double	GetDecMinutes	()	const;
	double	GetCentHours	()	const;   //ora centesimale 1 sec = 0.0278 h centesimale

	static	void	SetElapsedTimePrecision(int nPrec);
	static	int		GetElapsedTimePrecision();			

public:
	// operatori di assegnazione
	const DataLng& operator=(const DataLng& i)	{ Assign(i);		return *this; }
	const DataLng& operator=(const DataInt& i)	{ Assign(i);		return *this; }
	const DataLng& operator=(const DataObj& i)	{ Assign(i);		return *this; }
	const DataLng& operator=(const int& i)		{ Assign((long)i);	return *this; }
	const DataLng& operator=(const long& i)		{ Assign(i);		return *this; }
	
	const DataLng& operator+=(const DataLng& di){ Assign(m_nValue + di.m_nValue); return *this; }
	const DataLng& operator+=(const DataInt& di){ Assign(m_nValue + (int) di); return *this; }
	const DataLng& operator+=(const int& i)		{ Assign(m_nValue + i); return *this; }
	const DataLng& operator+=(const long& i)	{ Assign(m_nValue + i); return *this; }

	const DataLng& operator-=(const DataLng& di){ Assign(m_nValue - di.m_nValue); return *this; }
	const DataLng& operator-=(const DataInt& di){ Assign(m_nValue - (int) di); return *this; }
	const DataLng& operator-=(const int& i)		{ Assign(m_nValue - i); return *this; }
	const DataLng& operator-=(const long& i)	{ Assign(m_nValue - i); return *this; }

	const DataLng& operator*=(const DataLng& di){ Assign(m_nValue * di.m_nValue); return *this; }
	const DataLng& operator*=(const DataInt& di){ Assign(m_nValue * (int) di); return *this; }
	const DataLng& operator*=(const int& i)		{ Assign(m_nValue * i); return *this; }
	const DataLng& operator*=(const long& i)	{ Assign(m_nValue * i); return *this; }

	const DataLng& operator/=(const DataLng& di){ Assign(m_nValue / di.m_nValue); return *this; }
	const DataLng& operator/=(const DataInt& di){ Assign(m_nValue / (int) di); return *this; }
	const DataLng& operator/=(const int& i)		{ Assign(m_nValue / i); return *this; }
	const DataLng& operator/=(const long& i)	{ Assign(m_nValue / i); return *this; }

	const DataLng& operator%=(const DataLng& di){ Assign(m_nValue % di.m_nValue); return *this; }
	const DataLng& operator%=(const DataInt& di){ Assign(m_nValue % (int) di); return *this; }
	const DataLng& operator%=(const int& i)		{ Assign(m_nValue % i); return *this; }
	const DataLng& operator%=(const long& i)	{ Assign(m_nValue % i); return *this; }

	const DataLng& operator&=(const DataLng& di){ Assign(m_nValue & di.m_nValue); return *this; }
	const DataLng& operator&=(const DataInt& di){ Assign(m_nValue & (int) di); return *this; }
	const DataLng& operator&=(const int& i)		{ Assign(m_nValue & i); return *this; }
	const DataLng& operator&=(const long& i)	{ Assign(m_nValue & i); return *this; }
	
	const DataLng& operator|=(const DataLng& di){ Assign(m_nValue | di.m_nValue); return *this; }
	const DataLng& operator|=(const DataInt& di){ Assign(m_nValue | (int) di); return *this; }
	const DataLng& operator|=(const int& i)		{ Assign(m_nValue | i); return *this; }
	const DataLng& operator|=(const long& i)	{ Assign(m_nValue | i); return *this; }

	const DataLng& operator^=(const DataLng& di){ Assign(m_nValue ^ di.m_nValue); return *this; }
	const DataLng& operator^=(const DataInt& di){ Assign(m_nValue ^ (int) di); return *this; }
	const DataLng& operator^=(const int& i)		{ Assign(m_nValue ^ i); return *this; }
	const DataLng& operator^=(const long& i)	{ Assign(m_nValue ^ i); return *this; }

	// operatori di incremento/decremento
	DataLng operator++()	{ Assign(m_nValue+1); return *this;}
	DataLng operator++(int)	{  long i = m_nValue; Assign(m_nValue+1); return i;}
	DataLng operator--()	{ Assign(m_nValue-1); return *this;}
	DataLng operator--(int)	{ long i = m_nValue; Assign(m_nValue-1); return i;}

	// operatori aritmentici
	friend DataLng operator+(const DataLng& i1, const DataLng&	i2)	{ return i1.m_nValue	+ i2.m_nValue;	}
	friend DataLng operator+(const DataLng& i1, const DataInt&	i2)	{ return i1.m_nValue	+ long(i2);		}
	friend DataLng operator+(const DataInt& i1, const DataLng&	i2)	{ return long(i1) 	 	+ i2.m_nValue;	}
	friend DataLng operator+(const DataLng& i1, const int&		i2)	{ return i1.m_nValue	+ i2;			}
	friend DataLng operator+(const int&		i1, const DataLng&	i2)	{ return i1				+ i2.m_nValue;	}
	friend DataLng operator+(const DataLng& i1, const long&		i2)	{ return i1.m_nValue	+ i2;			}
	friend DataLng operator+(const long&	i1, const DataLng&	i2)	{ return i1				+ i2.m_nValue;	}

	friend DataLng operator-(const DataLng& i1)						{ return -(i1.m_nValue);				}
	friend DataLng operator-(const DataLng& i1, const DataLng&	i2)	{ return i1.m_nValue	- i2.m_nValue;	}
	friend DataLng operator-(const DataLng& i1, const DataInt&	i2)	{ return i1.m_nValue	- long(i2);		}
	friend DataLng operator-(const DataInt& i1, const DataLng&	i2)	{ return long(i1) 	 	- i2.m_nValue;	}
	friend DataLng operator-(const DataLng& i1, const int&		i2)	{ return i1.m_nValue	- i2;			}
	friend DataLng operator-(const int&		i1, const DataLng&	i2)	{ return i1				- i2.m_nValue;	}
	friend DataLng operator-(const DataLng& i1, const long&		i2)	{ return i1.m_nValue	- i2;			}
	friend DataLng operator-(const long&	i1, const DataLng&	i2)	{ return i1				- i2.m_nValue;	}

	friend DataLng operator*(const DataLng& i1, const DataLng&	i2)	{ return i1.m_nValue	* i2.m_nValue;	}
	friend DataLng operator*(const DataLng& i1, const DataInt&	i2)	{ return i1.m_nValue	* long(i2);		}
	friend DataLng operator*(const DataInt& i1, const DataLng&	i2)	{ return long(i1)		* i2.m_nValue;	}
	friend DataLng operator*(const DataLng& i1, const int&		i2)	{ return i1.m_nValue	* i2;			}
	friend DataLng operator*(const int&		i1, const DataLng&	i2)	{ return i1				* i2.m_nValue;	}
	friend DataLng operator*(const DataLng& i1, const long&		i2)	{ return i1.m_nValue	* i2;			}
	friend DataLng operator*(const long&	i1, const DataLng&	i2)	{ return i1				* i2.m_nValue;	}

	friend DataLng operator/(const DataLng& i1, const DataLng&	i2)	{ return i1.m_nValue	/ i2.m_nValue;	}
	friend DataLng operator/(const DataLng& i1, const DataInt&	i2)	{ return i1.m_nValue	/ long(i2);		}
	friend DataLng operator/(const DataInt& i1, const DataLng&	i2)	{ return long(i1) 	 	/ i2.m_nValue;	}
	friend DataLng operator/(const DataLng& i1, const int&		i2)	{ return i1.m_nValue	/ i2;			}
	friend DataLng operator/(const int&		i1, const DataLng&	i2)	{ return i1				/ i2.m_nValue;	}
	friend DataLng operator/(const DataLng& i1, const long&		i2)	{ return i1.m_nValue	/ i2;			}
	friend DataLng operator/(const long&	i1, const DataLng&	i2)	{ return i1				/ i2.m_nValue;	}

	friend DataLng operator%(const DataLng& i1, const DataLng&	i2)	{ return i1.m_nValue	% i2.m_nValue;	}
	friend DataLng operator%(const DataLng& i1, const DataInt&	i2)	{ return i1.m_nValue	% long(i2);		}
	friend DataLng operator%(const DataInt& i1, const DataLng&	i2)	{ return long(i1) 		% i2.m_nValue;	}
	friend DataLng operator%(const DataLng& i1, const int&		i2)	{ return i1.m_nValue	% i2;			}
	friend DataLng operator%(const int&		i1, const DataLng&	i2)	{ return i1				% i2.m_nValue;	}
	friend DataLng operator%(const DataLng& i1, const long&		i2)	{ return i1.m_nValue	% i2;			}
	friend DataLng operator%(const long&	i1, const DataLng&	i2)	{ return i1				% i2.m_nValue;	}

	// operatori bitwise
	// operatori bitwise
	friend DataLng operator&(const DataLng& i1, const DataLng&	i2)	{ return i1.m_nValue	& i2.m_nValue;	}
	friend DataLng operator&(const DataLng& i1, const DataInt&	i2)	{ return i1.m_nValue	& long(i2);		}
	friend DataLng operator&(const DataInt& i1, const DataLng&	i2)	{ return long(i1) 		& i2.m_nValue;	}
	friend DataLng operator&(const DataLng& i1, const int&		i2)	{ return i1.m_nValue	& i2;			}
	friend DataLng operator&(const int&		i1, const DataLng&	i2)	{ return i1				& i2.m_nValue;	}
	friend DataLng operator&(const DataLng& i1, const long&		i2)	{ return i1.m_nValue	& i2;			}
	friend DataLng operator&(const long&	i1, const DataLng&	i2)	{ return i1				& i2.m_nValue;	}

	friend DataLng operator|(const DataLng& i1, const DataLng&	i2)	{ return i1.m_nValue	| i2.m_nValue;	}
	friend DataLng operator|(const DataLng& i1, const DataInt&	i2)	{ return i1.m_nValue	| long(i2);		}
	friend DataLng operator|(const DataInt& i1, const DataLng&	i2)	{ return long(i1) 		| i2.m_nValue;	}
	friend DataLng operator|(const DataLng& i1, const int&		i2)	{ return i1.m_nValue	| i2;			}
	friend DataLng operator|(const int&		i1, const DataLng&	i2)	{ return i1				| i2.m_nValue;	}
	friend DataLng operator|(const DataLng& i1, const long&		i2)	{ return i1.m_nValue	| i2;			}
	friend DataLng operator|(const long&	i1, const DataLng&	i2)	{ return i1				| i2.m_nValue;	}

	friend DataLng operator^(const DataLng& i1, const DataLng&	i2)	{ return i1.m_nValue	^ i2.m_nValue;	}
	friend DataLng operator^(const DataLng& i1, const DataInt&	i2)	{ return i1.m_nValue	^ long(i2);		}
	friend DataLng operator^(const DataInt& i1, const DataLng&	i2)	{ return long(i1) 		^ i2.m_nValue;	}
	friend DataLng operator^(const DataLng& i1, const int&		i2)	{ return i1.m_nValue	^ i2;			}
	friend DataLng operator^(const int&		i1, const DataLng&	i2)	{ return i1				^ i2.m_nValue;	}
	friend DataLng operator^(const DataLng& i1, const long&		i2)	{ return i1.m_nValue	^ i2;			}
	friend DataLng operator^(const long&	i1, const DataLng&	i2)	{ return i1				^ i2.m_nValue;	}

	// operatori di confronto
	friend BOOL operator==(const DataLng& i1, const DataLng&	i2)	{ return i1.m_nValue	== i2.m_nValue;	}
	friend BOOL operator==(const DataLng& i1, const DataInt&	i2)	{ return i1.m_nValue	== long(i2);	}
	friend BOOL operator==(const DataInt& i1, const DataLng&	i2)	{ return long(i1) 		== i2.m_nValue;	}
	friend BOOL operator==(const DataLng& i1, const int&		i2)	{ return i1.m_nValue	== i2;			}
	friend BOOL operator==(const int&	  i1, const DataLng&	i2)	{ return i1				== i2.m_nValue;	}
	friend BOOL operator==(const DataLng& i1, const long&		i2)	{ return i1.m_nValue	== i2;			}
	friend BOOL operator==(const long&	  i1, const DataLng&	i2)	{ return i1				== i2.m_nValue; }

	friend BOOL operator!=(const DataLng& i1, const DataLng&	i2)	{ return i1.m_nValue	!= i2.m_nValue; }
	friend BOOL operator!=(const DataLng& i1, const DataInt&	i2)	{ return i1.m_nValue	!= long(i2);	}
	friend BOOL operator!=(const DataInt& i1, const DataLng&	i2)	{ return long(i1) 		!= i2.m_nValue; }
	friend BOOL operator!=(const DataLng& i1, const int&		i2)	{ return i1.m_nValue	!= i2;			}
	friend BOOL operator!=(const int&	  i1, const DataLng&	i2)	{ return i1				!= i2.m_nValue; }
	friend BOOL operator!=(const DataLng& i1, const long&		i2)	{ return i1.m_nValue	!= i2;			}
	friend BOOL operator!=(const long&	  i1, const DataLng&	i2)	{ return i1				!= i2.m_nValue;	}

	friend BOOL operator> (const DataLng& i1, const DataLng&	i2)	{ return i1.m_nValue	> i2.m_nValue;	}
	friend BOOL operator> (const DataLng& i1, const DataInt&	i2)	{ return i1.m_nValue	> long(i2);		}
	friend BOOL operator> (const DataInt& i1, const DataLng&	i2)	{ return long(i1) 		> i2.m_nValue;	}
	friend BOOL operator> (const DataLng& i1, const int&		i2)	{ return i1.m_nValue	> i2;			}
	friend BOOL operator> (const int&	  i1, const DataLng&	i2)	{ return i1				> i2.m_nValue;	}
	friend BOOL operator> (const DataLng& i1, const long&		i2)	{ return i1.m_nValue	> i2;			}
	friend BOOL operator> (const long&	  i1, const DataLng&	i2)	{ return i1				> i2.m_nValue;	}

	friend BOOL operator>=(const DataLng& i1, const DataLng&	i2)	{ return i1.m_nValue	>= i2.m_nValue; }
	friend BOOL operator>=(const DataLng& i1, const DataInt&	i2)	{ return i1.m_nValue	>= long(i2);	}
	friend BOOL operator>=(const DataInt& i1, const DataLng&	i2)	{ return long(i1) 		>= i2.m_nValue;	}
	friend BOOL operator>=(const DataLng& i1, const int&		i2)	{ return i1.m_nValue	>= i2;			}
	friend BOOL operator>=(const int&	  i1, const DataLng&	i2)	{ return i1				>= i2.m_nValue;	}
	friend BOOL operator>=(const DataLng& i1, const long&		i2)	{ return i1.m_nValue	>= i2;			}
	friend BOOL operator>=(const long&	  i1, const DataLng&	i2)	{ return i1				>= i2.m_nValue;	}

	friend BOOL operator< (const DataLng& i1, const DataLng&	i2)	{ return i1.m_nValue	< i2.m_nValue;	}
	friend BOOL operator< (const DataLng& i1, const DataInt&	i2)	{ return i1.m_nValue	< long(i2);		}
	friend BOOL operator< (const DataInt& i1, const DataLng&	i2)	{ return long(i1) 		< i2.m_nValue;	}
	friend BOOL operator< (const DataLng& i1, const int&		i2)	{ return i1.m_nValue	< i2;			}
	friend BOOL operator< (const int&	  i1, const DataLng&	i2)	{ return i1				< i2.m_nValue;	}
	friend BOOL operator< (const DataLng& i1, const long&		i2)	{ return i1.m_nValue	< i2;			}
	friend BOOL operator< (const long&	  i1, const DataLng&	i2)	{ return i1				< i2.m_nValue;	}

	friend BOOL operator<=(const DataLng& i1, const DataLng&	i2)	{ return i1.m_nValue	<= i2.m_nValue; }
	friend BOOL operator<=(const DataLng& i1, const DataInt&	i2)	{ return i1.m_nValue	<= long(i2);	}
	friend BOOL operator<=(const DataInt& i1, const DataLng&	i2)	{ return long(i1) 		<= i2.m_nValue;	}
	friend BOOL operator<=(const DataLng& i1, const int&		i2)	{ return i1.m_nValue	<= i2;			}
	friend BOOL operator<=(const int&	  i1, const DataLng&	i2)	{ return i1				<= i2.m_nValue;	}
	friend BOOL operator<=(const DataLng& i1, const long&		i2)	{ return i1.m_nValue	<= i2;			}
	friend BOOL operator<=(const long&	  i1, const DataLng&	i2)	{ return i1				<= i2.m_nValue; }
	
// diagnostics
#ifdef _DEBUG
public:	
	void Dump(CDumpContext&) const;
	void AssertValid() const;
#endif
};

// gestione della macro di definizione degli operatori friend                                                                            
//++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
#define DECLARE_OPERATORS(DataXXX)\
\
	friend DataXXX operator+(const DataXXX& i1, const DataXXX&	i2)	{ return i1.m_nValue	+ i2.m_nValue;	}\
	friend DataXXX operator+(const DataXXX& i1, const DataLng&	i2)	{ return i1.m_nValue	+ double(i2);	}\
	friend DataXXX operator+(const DataLng& i1, const DataXXX&	i2)	{ return double(i1) 	+ i2.m_nValue;	}\
	friend DataXXX operator+(const DataXXX& i1, const DataInt&	i2)	{ return i1.m_nValue	+ double(i2);	}\
	friend DataXXX operator+(const DataInt& i1, const DataXXX&	i2)	{ return double(i1) 	+ i2.m_nValue;	}\
	friend DataXXX operator+(const DataXXX& i1, const int&		i2)	{ return i1.m_nValue	+ i2;			}\
	friend DataXXX operator+(const int&		i1, const DataXXX&	i2)	{ return i1				+ i2.m_nValue;	}\
	friend DataXXX operator+(const DataXXX& i1, const long&		i2)	{ return i1.m_nValue	+ i2;			}\
	friend DataXXX operator+(const long&	i1, const DataXXX&	i2)	{ return i1				+ i2.m_nValue;	}\
	friend DataXXX operator+(const DataXXX& i1, const double&	i2)	{ return i1.m_nValue	+ i2;			}\
	friend DataXXX operator+(const double&	i1, const DataXXX&	i2)	{ return i1				+ i2.m_nValue;	}\
\
	friend DataXXX operator-(const DataXXX& i1)						{ return -i1.m_nValue;					}\
	friend DataXXX operator-(const DataXXX& i1, const DataXXX&	i2)	{ return i1.m_nValue	- i2.m_nValue;	}\
	friend DataXXX operator-(const DataXXX& i1, const DataLng&	i2)	{ return i1.m_nValue	- double(i2);	}\
	friend DataXXX operator-(const DataLng& i1, const DataXXX&	i2)	{ return double(i1) 	- i2.m_nValue;	}\
	friend DataXXX operator-(const DataXXX& i1, const DataInt&	i2)	{ return i1.m_nValue	- double(i2);	}\
	friend DataXXX operator-(const DataInt& i1, const DataXXX&	i2)	{ return double(i1) 	- i2.m_nValue;	}\
	friend DataXXX operator-(const DataXXX& i1, const int&		i2)	{ return i1.m_nValue	- i2;			}\
	friend DataXXX operator-(const int&		i1, const DataXXX&	i2)	{ return i1				- i2.m_nValue;	}\
	friend DataXXX operator-(const DataXXX& i1, const long&		i2)	{ return i1.m_nValue	- i2;			}\
	friend DataXXX operator-(const long&	i1, const DataXXX&	i2)	{ return i1				- i2.m_nValue;	}\
	friend DataXXX operator-(const DataXXX& i1, const double&	i2)	{ return i1.m_nValue	- i2;			}\
	friend DataXXX operator-(const double&	i1, const DataXXX&	i2)	{ return i1				- i2.m_nValue;	}\
\
	friend DataXXX operator*(const DataXXX& i1, const DataXXX&	i2)	{ return i1.m_nValue	* i2.m_nValue;	}\
	friend DataXXX operator*(const DataXXX& i1, const DataLng&	i2)	{ return i1.m_nValue	* double(i2);	}\
	friend DataXXX operator*(const DataLng& i1, const DataXXX&	i2)	{ return double(i1) 	* i2.m_nValue;	}\
	friend DataXXX operator*(const DataXXX& i1, const DataInt&	i2)	{ return i1.m_nValue	* double(i2);	}\
	friend DataXXX operator*(const DataInt& i1, const DataXXX&	i2)	{ return double(i1) 	* i2.m_nValue;	}\
	friend DataXXX operator*(const DataXXX& i1, const int&		i2)	{ return i1.m_nValue	* i2;			}\
	friend DataXXX operator*(const int&		i1, const DataXXX&	i2)	{ return i1				* i2.m_nValue;	}\
	friend DataXXX operator*(const DataXXX& i1, const long&		i2)	{ return i1.m_nValue	* i2;			}\
	friend DataXXX operator*(const long&	i1, const DataXXX&	i2)	{ return i1				* i2.m_nValue;	}\
	friend DataXXX operator*(const DataXXX& i1, const double&	i2)	{ return i1.m_nValue	* i2;			}\
	friend DataXXX operator*(const double&	i1, const DataXXX&	i2)	{ return i1				* i2.m_nValue;	}\
\
	friend DataXXX operator/(const DataXXX& i1, const DataXXX&	i2)	{ return i1.m_nValue	/ i2.m_nValue;	}\
	friend DataXXX operator/(const DataXXX& i1, const DataLng&	i2)	{ return i1.m_nValue	/ double(i2);	}\
	friend DataXXX operator/(const DataLng& i1, const DataXXX&	i2)	{ return double(i1) 	/ i2.m_nValue;	}\
	friend DataXXX operator/(const DataXXX& i1, const DataInt&	i2)	{ return i1.m_nValue	/ double(i2);	}\
	friend DataXXX operator/(const DataInt& i1, const DataXXX&	i2)	{ return double(i1) 	/ i2.m_nValue;	}\
	friend DataXXX operator/(const DataXXX& i1, const int&		i2)	{ return i1.m_nValue	/ i2;			}\
	friend DataXXX operator/(const int&		i1, const DataXXX&	i2)	{ return i1				/ i2.m_nValue;	}\
	friend DataXXX operator/(const DataXXX& i1, const long&		i2)	{ return i1.m_nValue	/ i2;			}\
	friend DataXXX operator/(const long&	i1, const DataXXX&	i2)	{ return i1				/ i2.m_nValue;	}\
	friend DataXXX operator/(const DataXXX& i1, const double&	i2)	{ return i1.m_nValue	/ i2;			}\
	friend DataXXX operator/(const double&	i1, const DataXXX&	i2)	{ return i1				/ i2.m_nValue;	}\
\
	friend BOOL operator==(const DataXXX& i1, const DataXXX&	i2)	{ return fabs(i1.m_nValue - i2.m_nValue) < i1.GetEpsilon();	}\
	friend BOOL operator==(const DataXXX& i1, const DataLng&	i2)	{ return fabs(i1.m_nValue - double(i2)) < i1.GetEpsilon();	}\
	friend BOOL operator==(const DataLng& i1, const DataXXX&	i2)	{ return fabs(double(i1) - i2.m_nValue) < i2.GetEpsilon();	}\
	friend BOOL operator==(const DataXXX& i1, const DataInt&	i2)	{ return fabs(i1.m_nValue - double(i2)) < i1.GetEpsilon();	}\
	friend BOOL operator==(const DataInt& i1, const DataXXX&	i2)	{ return fabs(double(i1) - i2.m_nValue) < i2.GetEpsilon();	}\
	friend BOOL operator==(const DataXXX& i1, const int&		i2)	{ return fabs(i1.m_nValue - i2) < i1.GetEpsilon();	}\
	friend BOOL operator==(const int&	  i1, const DataXXX&	i2)	{ return fabs(i1 - i2.m_nValue) < i2.GetEpsilon();	}\
	friend BOOL operator==(const DataXXX& i1, const long&		i2)	{ return fabs(i1.m_nValue - i2) < i1.GetEpsilon();	}\
	friend BOOL operator==(const long&	  i1, const DataXXX&	i2)	{ return fabs(i1 - i2.m_nValue) < i2.GetEpsilon();	}\
	friend BOOL operator==(const DataXXX& i1, const double&		i2)	{ return fabs(i1.m_nValue - i2) < i1.GetEpsilon();	}\
	friend BOOL operator==(const double&  i1, const DataXXX&	i2)	{ return fabs(i1 - i2.m_nValue) < i2.GetEpsilon();	}\
\
	friend BOOL operator!=(const DataXXX& i1, const DataXXX&	i2)	{ return fabs(i1.m_nValue - i2.m_nValue) >= i1.GetEpsilon();}\
	friend BOOL operator!=(const DataXXX& i1, const DataLng&	i2)	{ return fabs(i1.m_nValue - double(i2)) >= i1.GetEpsilon();	}\
	friend BOOL operator!=(const DataLng& i1, const DataXXX&	i2)	{ return fabs(double(i1) - i2.m_nValue) >= i2.GetEpsilon();	}\
	friend BOOL operator!=(const DataXXX& i1, const DataInt&	i2)	{ return fabs(i1.m_nValue - double(i2)) >= i1.GetEpsilon();	}\
	friend BOOL operator!=(const DataInt& i1, const DataXXX&	i2)	{ return fabs(double(i1) - i2.m_nValue) >= i2.GetEpsilon();	}\
	friend BOOL operator!=(const DataXXX& i1, const int&		i2)	{ return fabs(i1.m_nValue - i2) >= i1.GetEpsilon();	}\
	friend BOOL operator!=(const int&	  i1, const DataXXX&	i2)	{ return fabs(i1 - i2.m_nValue) >= i2.GetEpsilon();	}\
	friend BOOL operator!=(const DataXXX& i1, const long&		i2)	{ return fabs(i1.m_nValue - i2) >= i1.GetEpsilon();	}\
	friend BOOL operator!=(const long&	  i1, const DataXXX&	i2)	{ return fabs(i1 - i2.m_nValue) >= i2.GetEpsilon();	}\
	friend BOOL operator!=(const DataXXX& i1, const double&		i2)	{ return fabs(i1.m_nValue - i2) >= i1.GetEpsilon();	}\
	friend BOOL operator!=(const double&  i1, const DataXXX&	i2)	{ return fabs(i1 - i2.m_nValue) >= i2.GetEpsilon();	}\
\
    friend BOOL operator> (const DataXXX& i1, const DataXXX&	i2)	{ return i1.m_nValue > i2.m_nValue  && fabs(i1.m_nValue - i2.m_nValue)>= i1.GetEpsilon() ;}\
	friend BOOL operator> (const DataXXX& i1, const DataLng&	i2)	{ return i1.m_nValue > double(i2) && fabs(i1.m_nValue - double(i2))>= i1.GetEpsilon() ;	}\
	friend BOOL operator> (const DataLng& i1, const DataXXX&	i2)	{ return double(i1) > i2.m_nValue  && fabs(double(i1) - i2.m_nValue)>= i2.GetEpsilon() ;	}\
	friend BOOL operator> (const DataXXX& i1, const DataInt&	i2)	{ return i1.m_nValue > double(i2) && fabs(i1.m_nValue - double(i2))>= i1.GetEpsilon() ;	}\
	friend BOOL operator> (const DataInt& i1, const DataXXX&	i2)	{ return double(i1) > i2.m_nValue  && fabs(double(i1) - i2.m_nValue)>= i2.GetEpsilon() ;	}\
	friend BOOL operator> (const DataXXX& i1, const int&		i2)	{ return i1.m_nValue > i2 && fabs(i1.m_nValue - i2)>= i1.GetEpsilon() ;	}\
	friend BOOL operator> (const int&	  i1, const DataXXX&	i2)	{ return i1 > i2.m_nValue && fabs(i1 - i2.m_nValue)>= i2.GetEpsilon() ;	}\
	friend BOOL operator> (const DataXXX& i1, const long&		i2)	{ return i1.m_nValue > i2 && fabs(i1.m_nValue - i2)>= i1.GetEpsilon() ;	}\
	friend BOOL operator> (const long&	  i1, const DataXXX&	i2)	{ return i1 > i2.m_nValue && fabs(i1 - i2.m_nValue)>= i2.GetEpsilon() ;	}\
	friend BOOL operator> (const DataXXX& i1, const double&		i2)	{ return i1.m_nValue > i2 && fabs(i1.m_nValue - i2)>= i1.GetEpsilon() ;	}\
	friend BOOL operator> (const double&  i1, const DataXXX&	i2)	{ return i1 > i2.m_nValue && fabs(i1 - i2.m_nValue)>= i2.GetEpsilon() ;	}\
\
	friend BOOL operator>=(const DataXXX& i1, const DataXXX&	i2)	{ return i1.m_nValue>i2.m_nValue || fabs(i1.m_nValue - i2.m_nValue) < i1.GetEpsilon() ; }\
	friend BOOL operator>=(const DataXXX& i1, const DataLng&	i2)	{ return i1.m_nValue>double(i2) || fabs(i1.m_nValue - double(i2)) < i1.GetEpsilon() ; }\
	friend BOOL operator>=(const DataLng& i1, const DataXXX&	i2)	{ return double(i1)>i2.m_nValue || fabs(double(i1) - i2.m_nValue) < i2.GetEpsilon() ; }\
	friend BOOL operator>=(const DataXXX& i1, const DataInt&	i2)	{ return i1.m_nValue>double(i2) || fabs(i1.m_nValue - double(i2)) < i1.GetEpsilon() ; }\
	friend BOOL operator>=(const DataInt& i1, const DataXXX&	i2)	{ return double(i1)>i2.m_nValue || fabs(double(i1) - i2.m_nValue) < i2.GetEpsilon() ; }\
	friend BOOL operator>=(const DataXXX& i1, const int&		i2)	{ return i1.m_nValue>i2 || fabs(i1.m_nValue - i2) < i1.GetEpsilon() ; }\
	friend BOOL operator>=(const int&	  i1, const DataXXX&	i2)	{ return i1>i2.m_nValue || fabs(i1 - i2.m_nValue) < i2.GetEpsilon() ; }\
	friend BOOL operator>=(const DataXXX& i1, const long&		i2)	{ return i1.m_nValue>i2 || fabs(i1.m_nValue - i2) < i1.GetEpsilon() ; }\
	friend BOOL operator>=(const long&	  i1, const DataXXX&	i2)	{ return i1>i2.m_nValue || fabs(i1 - i2.m_nValue) < i2.GetEpsilon() ; }\
	friend BOOL operator>=(const DataXXX& i1, const double&		i2)	{ return i1.m_nValue>i2 || fabs(i1.m_nValue - i2) < i1.GetEpsilon() ; }\
	friend BOOL operator>=(const double&  i1, const DataXXX&	i2)	{ return i1>i2.m_nValue || fabs(i1 - i2.m_nValue) < i2.GetEpsilon() ; }\
\
	friend BOOL operator< (const DataXXX& i1, const DataXXX&	i2)	{ return i1.m_nValue < i2.m_nValue && fabs(i1.m_nValue - i2.m_nValue)>= i1.GetEpsilon() ;	}\
	friend BOOL operator< (const DataXXX& i1, const DataLng&	i2)	{ return i1.m_nValue < double(i2) && fabs(i1.m_nValue - double(i2))>= i1.GetEpsilon() ;	}\
	friend BOOL operator< (const DataLng& i1, const DataXXX&	i2)	{ return double(i1) < i2.m_nValue  && fabs(double(i1) - i2.m_nValue)>= i2.GetEpsilon() ;}\
	friend BOOL operator< (const DataXXX& i1, const DataInt&	i2)	{ return i1.m_nValue < double(i2) && fabs(i1.m_nValue - double(i2))>= i1.GetEpsilon() ;	}\
	friend BOOL operator< (const DataInt& i1, const DataXXX&	i2)	{ return double(i1) < i2.m_nValue  && fabs(double(i1) - i2.m_nValue)>= i2.GetEpsilon() ;}\
	friend BOOL operator< (const DataXXX& i1, const int&		i2)	{ return i1.m_nValue < i2 && fabs(i1.m_nValue - i2)>= i1.GetEpsilon() ;	}\
	friend BOOL operator< (const int&	  i1, const DataXXX&	i2)	{ return i1 < i2.m_nValue && fabs(i1 - i2.m_nValue)>= i2.GetEpsilon() ;	}\
	friend BOOL operator< (const DataXXX& i1, const long&		i2)	{ return i1.m_nValue < i2 && fabs(i1.m_nValue - i2)>= i1.GetEpsilon() ;	}\
	friend BOOL operator< (const long&	  i1, const DataXXX&	i2)	{ return i1 < i2.m_nValue && fabs(i1 - i2.m_nValue)>= i2.GetEpsilon() ;	}\
	friend BOOL operator< (const DataXXX& i1, const double&		i2)	{ return i1.m_nValue < i2 && fabs(i1.m_nValue - i2)>= i1.GetEpsilon() ;	}\
	friend BOOL operator< (const double&  i1, const DataXXX&	i2)	{ return i1 < i2.m_nValue && fabs(i1 - i2.m_nValue)>= i2.GetEpsilon() ;	}\
\
	friend BOOL operator<=(const DataXXX& i1, const DataXXX&	i2)	{ return i1.m_nValue < i2.m_nValue || fabs(i1.m_nValue - i2.m_nValue) < i1.GetEpsilon() ; }\
	friend BOOL operator<=(const DataXXX& i1, const DataLng&	i2)	{ return i1.m_nValue < double(i2) || fabs(i1.m_nValue - double(i2)) < i1.GetEpsilon() ; }\
	friend BOOL operator<=(const DataLng& i1, const DataXXX&	i2)	{ return double(i1) < i2.m_nValue || fabs(double(i1) - i2.m_nValue) < i2.GetEpsilon() ; }\
	friend BOOL operator<=(const DataXXX& i1, const DataInt&	i2)	{ return i1.m_nValue < double(i2) || fabs(i1.m_nValue - double(i2)) < i1.GetEpsilon() ; }\
	friend BOOL operator<=(const DataInt& i1, const DataXXX&	i2)	{ return double(i1) < i2.m_nValue || fabs(double(i1) - i2.m_nValue) < i2.GetEpsilon() ; }\
	friend BOOL operator<=(const DataXXX& i1, const int&		i2)	{ return i1.m_nValue < i2 || fabs(i1.m_nValue - i2) < i1.GetEpsilon() ; }\
	friend BOOL operator<=(const int&	  i1, const DataXXX&	i2)	{ return i1 < i2.m_nValue || fabs(i1 - i2.m_nValue) < i2.GetEpsilon() ; }\
	friend BOOL operator<=(const DataXXX& i1, const long&		i2)	{ return i1.m_nValue < i2 || fabs(i1.m_nValue - i2) < i1.GetEpsilon() ; }\
	friend BOOL operator<=(const long&	  i1, const DataXXX&	i2)	{ return i1 < i2.m_nValue || fabs(i1 - i2.m_nValue) < i2.GetEpsilon() ; }\
	friend BOOL operator<=(const DataXXX& i1, const double&		i2)	{ return i1.m_nValue < i2 || fabs(i1.m_nValue - i2) < i1.GetEpsilon() ; }\
	friend BOOL operator<=(const double&  i1, const DataXXX&	i2)	{ return i1 < i2.m_nValue || fabs(i1 - i2.m_nValue) < i2.GetEpsilon() ; }\

//++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
/*
\
*/

//============================================================================
class TB_EXPORT DataDbl : public DataObj
{

	friend class CRS_ObjectPropertyView;
	friend class CRSIniProp;

	DECLARE_DYNCREATE (DataDbl)

	friend class DataInt;
	friend class DataLng;
	friend class SqlTable;
	friend class SqlRecord;

protected:
	double  m_nValue;
	
public:
	// constructors & destructor
	DataDbl (const double nDouble = 0.0);
	DataDbl (const DataDbl&);

public:
	// overridable
	virtual BOOL		IsEmpty		()  const;
	virtual int         IsEqual     (const DataObj&)	const;
	virtual int         IsLessThan  (const DataObj&)	const;

	// Il primo parametro indica la lunghezza totale, il secondo
	// il numero di decimali (cfr. printf)
	virtual CString     Str         (int = -1, int = -1)const;

	virtual void*       GetRawData  (DataSize* = NULL)	const;
	virtual DataType    GetDataType	() const { return DataType(DATA_DBL_TYPE, 0); }

	//@@OLEDB 
	virtual DBLENGTH	GetOleDBSize   (BOOL /*bUnicode*/ = TRUE) const { return sizeof(m_nValue);}
	virtual void*		GetOleDBDataPtr() const { return (void*)(&m_nValue); }

	virtual void	Assign	(LPCTSTR);
	virtual void	Assign	(const RDEData&);
	virtual void	Assign	(const DataObj&);
	virtual void	Assign	(const double);
	virtual void	Assign	(const VARIANT&);

	virtual void	Clear	(BOOL bValid = TRUE);

	virtual	void	SetLowerValue	(int);
	virtual	void	SetUpperValue	(int);
	virtual	BOOL	IsLowerValue	() const;
	virtual	BOOL	IsUpperValue	() const;

public:
	// operatori di cast
	virtual operator const double   () const { return m_nValue; }
	virtual VARIANT ToVariant() const { VARIANT v; v.vt = VT_R8; v.dblVal = m_nValue; return v; }

	virtual CString GetXMLType(BOOL bSoapType = TRUE) const { return SCHEMA_XSD_DATATYPE_DOUBLE_VALUE;} ;
	virtual CString FormatDataForXML(BOOL bSoapType = TRUE) const;
	virtual void	AssignFromXMLString(LPCTSTR);
	virtual void	SerializeJsonValue(CJsonSerializer& jsonSerializer);
	virtual void	AssignJsonValue(CJsonParser& parser);
	double GetSoapValue()					{ return m_nValue; }
	void SetSoapValue(double nValue)		{ Assign(nValue); }

public:
	void	Round	(int nDec);

	static  const DataDbl    MINVALUE;
	static  const DataDbl    MAXVALUE;

public:
	static double	GetEpsilon  ();

	// operatori di assegnazione
	const DataDbl& operator=(const DataDbl& i)	{ Assign(i);			return *this; }
	const DataDbl& operator=(const DataLng& i)	{ Assign(i);			return *this; }
	const DataDbl& operator=(const DataInt& i)	{ Assign(i);			return *this; }
	const DataDbl& operator=(const double& i)	{ Assign(i);			return *this; }
	const DataDbl& operator=(const long& i)		{ Assign(double(i));	return *this; }
	const DataDbl& operator=(const int& i)		{ Assign(double(i));	return *this; }

	const DataDbl& operator+=(const DataDbl& i)	{ Assign(m_nValue + i.m_nValue); return *this; }
	const DataDbl& operator+=(const DataLng& i)	{ Assign(m_nValue + (long) i); return *this; }
	const DataDbl& operator+=(const DataInt& i)	{ Assign(m_nValue + (int) i); return *this; }
	const DataDbl& operator+=(const double& i)	{ Assign(m_nValue + i); return *this; }
	const DataDbl& operator+=(const long& i)	{ Assign(m_nValue + i); return *this; }
	const DataDbl& operator+=(const int& i)		{ Assign(m_nValue + i); return *this; }

	const DataDbl& operator-=(const DataDbl& i)	{ Assign(m_nValue - i.m_nValue); return *this; }
	const DataDbl& operator-=(const DataLng& i)	{ Assign(m_nValue - (long) i); return *this; }
	const DataDbl& operator-=(const DataInt& i)	{ Assign(m_nValue - (int) i); return *this; }
	const DataDbl& operator-=(const double& i)	{ Assign(m_nValue - i); return *this; }
	const DataDbl& operator-=(const long& i)	{ Assign(m_nValue - i); return *this; }
	const DataDbl& operator-=(const int& i)		{ Assign(m_nValue - i); return *this; }

	const DataDbl& operator*=(const DataDbl& i)	{ Assign(m_nValue * i.m_nValue); return *this; }
	const DataDbl& operator*=(const DataLng& i)	{ Assign(m_nValue * (long) i); return *this; }
	const DataDbl& operator*=(const DataInt& i)	{ Assign(m_nValue * (int) i); return *this; }
	const DataDbl& operator*=(const double& i)	{ Assign(m_nValue * i); return *this; }
	const DataDbl& operator*=(const long& i)	{ Assign(m_nValue * i); return *this; }
	const DataDbl& operator*=(const int& i)		{ Assign(m_nValue * i); return *this; }

	const DataDbl& operator/=(const DataDbl& i)	{ Assign(m_nValue / i.m_nValue); return *this; }
	const DataDbl& operator/=(const DataLng& i)	{ Assign(m_nValue / (long) i); return *this; }
	const DataDbl& operator/=(const DataInt& i)	{ Assign(m_nValue / (int) i); return *this; }
	const DataDbl& operator/=(const double& i)	{ Assign(m_nValue / i); return *this; }
	const DataDbl& operator/=(const long& i)	{ Assign(m_nValue / i); return *this; }
	const DataDbl& operator/=(const int& i)		{ Assign(m_nValue / i); return *this; }
                                                                            
	DECLARE_OPERATORS(DataDbl)
	
// diagnostics
#ifdef _DEBUG
public:	
	void Dump(CDumpContext&) const;
	void AssertValid() const;
#endif
};

//============================================================================
class TB_EXPORT DataMon : public DataDbl
{
	DECLARE_DYNCREATE (DataMon)

	friend class SqlTable;
	friend class SqlRecord;

public:
	// constructors & destructor
	DataMon (const double nDouble = 0.0);
	DataMon (const DataDbl& nDouble);
	DataMon (const DataMon& nMoney);

public:
	static double	GetEpsilon  ();

	// overridable
	virtual DataType    GetDataType	() const { return DataType(DATA_MON_TYPE, 0); }
	virtual CString		FormatData		(LPCTSTR pszFormatName = NULL) const;
public:

	// operatori di assegnazione
	const DataMon& operator=(const DataMon& i)	{ Assign(i);			return *this; }
	const DataMon& operator=(const DataDbl& i)	{ Assign(i);			return *this; }
	const DataMon& operator=(const DataLng& i)	{ Assign(i);			return *this; }
	const DataMon& operator=(const DataInt& i)	{ Assign(i);			return *this; }
	const DataMon& operator=(const double& i)	{ Assign(i);			return *this; }
	const DataMon& operator=(const long& i)		{ Assign(double(i));	return *this; }
	const DataMon& operator=(const int& i)		{ Assign(double(i));	return *this; }

	//operator ridefiniti
	DECLARE_OPERATORS(DataMon)

	// diagnostics
#ifdef _DEBUG
public:	
	void Dump(CDumpContext&) const;
	void AssertValid() const;
#endif
};

//============================================================================
class TB_EXPORT DataQty : public DataDbl
{
	DECLARE_DYNCREATE (DataQty)

	friend class SqlTable;
	friend class SqlRecord;
	
public:
	// constructors & destructor
	DataQty (const double nDouble = 0.0)	: DataDbl(nDouble)	{}
	DataQty (const DataDbl& nDouble)		: DataDbl(nDouble)	{}
	DataQty (const DataQty& nQta)			: DataDbl(nQta)		{}

public:
	// overridable
	virtual DataType    GetDataType	() const { return DataType(DATA_QTA_TYPE, 0); }

public:
	static double	GetEpsilon  ();

	// operatori di assegnazione
	const DataQty& operator=(const DataQty& i)	{ Assign(i);			return *this; }
	const DataQty& operator=(const DataDbl& i)	{ Assign(i);			return *this; }
	const DataQty& operator=(const DataLng& i)	{ Assign(i);			return *this; }
	const DataQty& operator=(const DataInt& i)	{ Assign(i);			return *this; }
	const DataQty& operator=(const double& i)	{ Assign(i);			return *this; }
	const DataQty& operator=(const long& i)		{ Assign(double(i));	return *this; }
	const DataQty& operator=(const int& i)		{ Assign(double(i));	return *this; }

	DECLARE_OPERATORS(DataQty)

	friend DataMon operator/(const DataMon& i1, const DataQty&	i2)	{ return (double)i1	/ i2.m_nValue;	}
	friend DataMon operator*(const DataMon& i1, const DataQty&	i2)	{ return (double)i1	* i2.m_nValue;	}
	friend DataMon operator*(const DataQty& i1, const DataMon&	i2)	{ return i1.m_nValue* (double)i2;	}

	friend DataDbl operator/(const DataDbl& i1, const DataQty&	i2)	{ return (double)i1	/ i2.m_nValue;	}
	friend DataDbl operator*(const DataDbl& i1, const DataQty&	i2)	{ return (double)i1	* i2.m_nValue;	}
	friend DataDbl operator*(const DataQty& i1, const DataDbl&	i2)	{ return i1.m_nValue* (double)i2;	}
	
// diagnostics
#ifdef _DEBUG
public:	
	void Dump(CDumpContext&) const;
	void AssertValid() const;
#endif
};

#define DataQta DataQty

//============================================================================
class TB_EXPORT DataPerc : public DataDbl
{
	DECLARE_DYNCREATE (DataPerc)

	friend class SqlTable;
	friend class SqlRecord;
	
public:
	// constructors & destructor
	DataPerc (const double nDouble = 0.0)	: DataDbl(nDouble)	{}
	DataPerc (const DataDbl& nDouble)		: DataDbl(nDouble)	{}
	DataPerc (const DataPerc& nPerc)		: DataDbl(nPerc)	{}

public:
	// overridable
	virtual DataType    GetDataType	() const { return DataType(DATA_PERC_TYPE, 0); }

public:
	static double	GetEpsilon  ();

	// operatori di assegnazione
	const DataPerc& operator=(const DataPerc& i)	{ Assign(i);			return *this; }
	const DataPerc& operator=(const DataDbl& i)		{ Assign(i);			return *this; }
	const DataPerc& operator=(const DataLng& i)		{ Assign(i);			return *this; }
	const DataPerc& operator=(const DataInt& i)		{ Assign(i);			return *this; }
	const DataPerc& operator=(const double& i)		{ Assign(i);			return *this; }
	const DataPerc& operator=(const long& i)		{ Assign(double(i));	return *this; }
	const DataPerc& operator=(const int& i)			{ Assign(double(i));	return *this; }
	
	DECLARE_OPERATORS(DataPerc)

	// friends mixed operator	
	friend DataMon operator/(const DataMon& i1, const DataPerc&	i2)	{ return (double)i1	/ i2.m_nValue;	}
	friend DataMon operator*(const DataMon& i1, const DataPerc&	i2)	{ return (double)i1	* i2.m_nValue;	}
	friend DataMon operator*(const DataPerc& i1, const DataMon&	i2)	{ return i1.m_nValue* (double)i2;	}

	friend DataQty operator/(const DataQty& i1, const DataPerc&	i2)	{ return (double)i1	/ i2.m_nValue;	}
	friend DataQty operator*(const DataQty& i1, const DataPerc&	i2)	{ return (double)i1	* i2.m_nValue;	}
	friend DataQty operator*(const DataPerc& i1, const DataQty&	i2)	{ return i1.m_nValue* (double)i2;	}

// diagnostics
#ifdef _DEBUG
public:	
	void Dump(CDumpContext&) const;
	void AssertValid() const;
#endif
};

/////////////////////////////////////////////////////////////////////////////
//                         DataDate definition
/////////////////////////////////////////////////////////////////////////////
//
//============================================================================
class TB_EXPORT DataDate : public DataObj
{
	DECLARE_DYNCREATE (DataDate)

	friend class SqlTable;
	friend class SqlRecord;

protected:
	DBTIMESTAMP m_DateStruct;
	
public:
	// constructors & destructor
	DataDate ();
	DataDate
	(
			const UWORD nDay, const UWORD nMonth, const SWORD nYear, 
			const UWORD nHour = MIN_HOUR, const UWORD nMinute = MIN_MINUTE, const UWORD nSecond = MIN_SECOND
	);
	DataDate (LPCTSTR pszDateStr, BOOL bFixFormat = FALSE);
	DataDate (const long nLongDate, const long nLongTime = 0);
	DataDate (const DataDate&);
	DataDate (const DBTIMESTAMP&);
	DataDate (const CTime& aTime);
	DataDate (const COleDateTime& dt)	{ Assign(dt); }
	DataDate (const VARIANT& v)			{ Assign(v); }
	DataDate (const SYSTEMTIME& st)		{ Assign(st); }

public:
	// overridable
	virtual BOOL		IsEmpty		()  const;
	virtual int         IsEqual     (const DataObj&)	const;
	virtual int         IsLessThan  (const DataObj&)	const;

	virtual CString     Str         (int = -1, int = -1) const;
	virtual CString		ToString	() const;

	virtual void*       GetRawData  (DataSize* = NULL)	const;
	virtual DataType    GetDataType	() const { return DataType(DATA_DATE_TYPE, unsigned int(m_wDataStatus & (FULLDATE | TIME))); }

	//@@OLEDB 
	virtual DBLENGTH	GetOleDBSize   (BOOL /*bUnicode*/ = TRUE) const { return sizeof(m_DateStruct);}
	virtual void*		GetOleDBDataPtr() const { return (void*)(&m_DateStruct); }

	virtual void		Assign		(BYTE*, DBLENGTH);
	virtual void        Assign      (LPCTSTR);
	virtual void		Assign		(const RDEData&);
	virtual void        Assign      (const DataObj&);
	virtual void        Assign      (const long);
	virtual void        Assign      (const long, const long);
	virtual void        Assign      (const DBTIMESTAMP& ts);
	virtual void        Assign      (const CTime& aTime);
	virtual void        Assign      (const VARIANT& v);
	virtual void        Assign      (const COleDateTime& dt);
	virtual void        Assign      (LPCTSTR, int nFormatIdx);
	virtual void        Assign      (const SYSTEMTIME& st) { Assign(COleDateTime(st)); }
	
	// in base al tipo di datatime visto come Token mi effettua un assegnazione della stringa
	// indipendente dai formattatori (vedi ParseDateTimeString di Parser e il parse di nodi XML)
	virtual	void		AssignFromOwnToken(LPCTSTR);

	virtual void        Clear       (BOOL bValid = TRUE);

	virtual	void		SetLowerValue	(int);
	virtual	void		SetUpperValue	(int);
	virtual	BOOL		IsLowerValue	() const;
	virtual	BOOL		IsUpperValue	() const;

	virtual int			MonthDays		() const	{ return ::MonthDays (m_DateStruct.month, m_DateStruct.year); }
	virtual const long	GiulianDate		() const	{ return ::GetGiulianDate(m_DateStruct); }

	// valorizzazione di data e ora separatamente
	virtual BOOL		SetDate			(const UWORD nDay, const UWORD nMonth, const SWORD nYear);
	virtual BOOL		SetDate			(const long);
	virtual BOOL		SetTime			(const UWORD wHour, const UWORD hMin, const UWORD hSec);
	virtual BOOL		SetTime			(const long);

public:	// not overridable
	const DBTIMESTAMP&	GetDateTime		() const { return m_DateStruct; }
	const tm				GetTMDateTime	() const;

	UWORD	Day			() const { return m_DateStruct.day;}
	UWORD	Month		() const { return m_DateStruct.month;}
	SWORD	Year		() const { return m_DateStruct.year;}

	UWORD	Hour		() const { return m_DateStruct.hour;}
	UWORD	Minute		() const { return m_DateStruct.minute;}
	UWORD	Second		() const { return m_DateStruct.second;}

	CString WeekDayName 	() const	{ return ::WeekDayName ((long) *this); }
	CString MonthName   	() const	{ return ::MonthName (m_DateStruct.month); }
	CString ShortMonthName  () const	{ return ::ShortMonthName (m_DateStruct.month); }

	//BOOL Is "Bisestile" ?? 		()	{ return ::Intercalary (m_DateStruct.year); }

	int WeekOfMonth  		(int alg = 0) const;// { return ::WeekOfMonth(m_DateStruct); }
	int WeekOfYear  		() const { return ::WeekOfYear(m_DateStruct); }
	int DayOfYear  			() const { return ::DayOfYear(m_DateStruct); }
	int DayOfWeek   		() const { return ::DayOfWeek (m_DateStruct); }

	// numero di secondi totali della sola parte hh:mm:ss ignorando la data
	const long	TotalSeconds	() const { return ::GetTotalSeconds(m_DateStruct); }

	// Autoset dal tempo di sistema
	void SetTodayDate		();
	void SetTodayTime		();
	void SetTodayDateTime	();

	void SetWeekStartDate	();
	void SetWeekStartDate(int year, int week);

	DataDate AddYears	(const long nYear) const;
	DataDate AddMonths	(const long nMonth) const;
	DataDate AddDays	(const long nDay) const;
	DataDate AddHours	(const long wHour) const;
	DataDate AddMinutes	(const long wMin) const;
	DataDate AddSeconds	(const long wSec) const;

	DataDate AddTime		(const long wDays, const long wMonths, const long wYears,  const long wHours, const long wMinutes, const long wSeconds) const;

	static DataDate EasterSunday(SWORD nYear);	//Julian calendar
    
public:
	// operatori di cast
	virtual operator const long () const 
							{ return GiulianDate(); }

	virtual operator const COleDateTime () const 
							{ 
								return COleDateTime
											(
												m_DateStruct.year,
												m_DateStruct.month,
												m_DateStruct.day,
												m_DateStruct.hour,
												m_DateStruct.minute,
												m_DateStruct.second
											); 
							}

	virtual operator const COleDateTimeSpan() const
							{
								ASSERT(IsATime());

								return COleDateTimeSpan
									(
										0,
										m_DateStruct.hour,
										m_DateStruct.minute,
										m_DateStruct.second
									);
							}


	virtual CString GetXMLType(BOOL bSoapType = TRUE) const;	
	virtual CString FormatDataForXML(BOOL bSoapType = TRUE) const;
	virtual void	AssignFromXMLString(LPCTSTR);
 
	BSTR GetSoapValue()					{ return FormatDataForXML().AllocSysString(); }
	void SetSoapValue(BSTR bstrValue);

	// formato data MxW
	static  const DataDate   NULLDATE;
	static  const DataDate   MINVALUE;
	static  const DataDate   MAXVALUE;
	
public:	
	// operatori di assegnazione
	const DataDate& operator=(const DataDate& dateSrc)	{ Assign(dateSrc);	return *this; }
	const DataDate& operator=(LPCTSTR psz)				{ Assign(psz); 		return *this; }
	const DataDate& operator=(const WORD& w)			{ Assign(w); 		return *this; }
	const DataDate& operator=(const long& l)			{ Assign(l); 		return *this; }
	const DataDate& operator=(const CTime& aTime)		{ Assign(aTime);	return *this; }
	const DataDate& operator=(const COleDateTime& dt)	{ Assign(dt);		return *this; }

	// aggiungono o levano solamente giorni
	const DataDate& operator+=(const int& i)			{ Assign(long(GiulianDate() + i)); return *this; }
	const DataDate& operator-=(const int& i)			{ Assign(long(GiulianDate() - i)); return *this; }

	friend DataDate operator+(const DataDate& i1, const long& i2)	{ return long(i1.GiulianDate() + i2); }
	friend DataDate operator-(const DataDate& i1, const long& i2)	{ return long(i1.GiulianDate() - i2); }
	
	// ritorna solo il numero di giorni tra le due date ignorando hh:mm:ss
	friend long operator-(const DataDate& d1, const DataDate& d2)	{ return d1.GiulianDate() - d2.GiulianDate();  }

	// operatori di confronto
TB_EXPORT 	friend BOOL operator==(const DataDate& d1, const DataDate&	d2);
TB_EXPORT 	friend BOOL operator!=(const DataDate& d1, const DataDate&	d2);
TB_EXPORT	friend BOOL operator< (const DataDate& d1, const DataDate&	d2);
TB_EXPORT 	friend BOOL operator> (const DataDate& d1, const DataDate&	d2);
TB_EXPORT	friend BOOL operator<=(const DataDate& d1, const DataDate&	d2) { return (d1 < d2) || (d1 == d2); }
TB_EXPORT	friend BOOL operator>=(const DataDate& d1, const DataDate&	d2) { return (d1 > d2) || (d1 == d2); }
	
//	static void GetEasterDate(SWORD nYear, DataDate& Easter, int CalendarType = 1); //typeCalendar: Gregorian = 0, Julian = 1  

// diagnostics
#ifdef _DEBUG
public:	
	void Dump(CDumpContext&) const;
	void AssertValid() const;
#endif
};

// Helper function per mascherare la struttura interna segli Enumerativi
//============================================================================
#define GET_TAG_VALUE(a)  HIWORD(a)
#define GET_ITEM_VALUE(a) LOWORD(a)
#define GET_TI_VALUE(a,b) MAKELONG(a,b)

//============================================================================
class TB_EXPORT DataEnum: public DataObj
{
	DECLARE_DYNCREATE (DataEnum)

	friend class SqlTable;
	friend class SqlRecord;

private:
	WORD	m_wBirthTag;
protected:
	DWORD	m_dwValue;
	
public:
	// constructors & destructor
	DataEnum (WORD wTagValue, WORD wItemValue);
	DataEnum (DWORD = 0);
	DataEnum (const DataEnum&);
	
public:
	// overridable
	virtual BOOL		IsEmpty		()  				const;
	virtual int         IsEqual     (const DataObj&)  	const;
	virtual int         IsLessThan  (const DataObj&)  	const;

	virtual CString     Str         (int = -1, int = -1) const;
	virtual CString		ToString	() const;

	virtual void*       GetRawData  (DataSize* = NULL)	const;
	virtual DataType    GetDataType	() const { return DataType(DATA_ENUM_TYPE, GetTagValue()); }

	//@@OLEDB 
	virtual DBLENGTH	GetOleDBSize   (BOOL /*bUnicode*/ = TRUE) const { return sizeof(m_dwValue);}
	virtual void*		GetOleDBDataPtr() const { return (void*)(&m_dwValue); }

	virtual void        Assign      (LPCTSTR);
	virtual void		Assign		(const RDEData&);
	virtual void        Assign      (const DataObj&);
	virtual void		Assign 		(WORD wTagValue, WORD wItemValue = 0);
	virtual void		Assign 		(DWORD dwValue);
	virtual void		Assign 		(const VARIANT&);
	
	virtual void        Clear       (BOOL bValid = TRUE);

	// se SoapType = TRUE l'enumerativo viene trattato come numero altrimenti come stringa tag:item
	virtual CString GetXMLType(BOOL bSoapType = TRUE) const;	
	virtual CString FormatDataForXML(BOOL bSoapType = TRUE) const;
	virtual void	AssignFromXMLString(LPCTSTR);

	int GetSoapValue()					{ return m_dwValue; }			
	void SetSoapValue(int nValue)		{ Assign ( (DWORD) nValue); };

	virtual VARIANT ToVariant() const { VARIANT v; v.vt = VT_UI4; v.ulVal = m_dwValue; return v; }
	virtual void	SerializeJsonValue(CJsonSerializer& jsonSerializer);
	virtual void	AssignJsonValue(CJsonParser& parser);

	void AssignByTitle (CString sEnumStr);
public:	
	CString			GetTagName		()	const;
	DWORD			GetValue		()	const	{ return m_dwValue; }
	WORD			GetTagValue		()	const	{ return GET_TAG_VALUE(m_dwValue); }
	WORD			GetBirthTagValue()	const	{ return m_wBirthTag; }
	WORD			GetItemValue	()	const	{ return GET_ITEM_VALUE(m_dwValue); }
	const EnumItemArray*	GetEnumItems	()	const;
	
	BOOL			SameTag			(const DataEnum& aDataEnum) const	{ return GetTagValue() == aDataEnum.GetTagValue(); }

	static	WORD	GetTagValue(CString sValue);

public:
	// operatori di assegnazione
	const DataEnum& operator=(const DataEnum& e)	{ Assign(e); return *this; }
	const DataEnum& operator=(const DWORD& e)		{ Assign(e); return *this; }

	// operatori di confronto
	friend BOOL operator==(const DataEnum&	e1, const DataEnum&	e2) { return e1.GetValue() == e2.GetValue(); }
	friend BOOL operator==(const DataEnum&	e1, const DWORD&	e2) { return e1.GetValue() == e2; }
	friend BOOL operator==(const DWORD& 	e1, const DataEnum&	e2) { return e2.GetValue() == e1; }

	friend BOOL operator!=(const DataEnum&	e1, const DataEnum&	e2) { return e1.GetValue() != e2.GetValue(); }
	friend BOOL operator!=(const DataEnum&	e1, const DWORD&	e2) { return e1.GetValue() != e2; }
	friend BOOL operator!=(const DWORD& 	e1, const DataEnum&	e2) { return e2.GetValue() != e1; }

// diagnostics
#ifdef _DEBUG
public:	
	void Dump(CDumpContext&) const;
	void AssertValid() const;
#endif
};

//==============================================================================
class TB_EXPORT DataGuid : public DataObj
{
	DECLARE_DYNCREATE (DataGuid)

	friend class SqlTable;
	friend class SqlRecord;

protected:
	GUID    m_guid;
   
public:
	// constructors & destructor
	DataGuid ();
	DataGuid (LPCTSTR);
	DataGuid (const DataGuid&);
	DataGuid (const GUID&);

public:
	// overridable
	virtual BOOL		IsEmpty		()  const;

	virtual int			IsEqual     (const DataObj&)			const;
	virtual int			IsEqual     (const GUID&)				const;
	virtual int			IsEqual     (LPCTSTR)					const;
	virtual	int			IsLessThan	(const DataObj& aDataObj)	const;

	//Il primo parametro indica se si vogliono le parentesi grafe (di default) oppure no
	virtual CString     Str         (int bWithoutCurlyBraces= -1, int = -1) const;
	virtual CString     ToString    () const;

	virtual void*       GetRawData  (DataSize* = NULL)	const;

	virtual void    Assign      (LPCTSTR);
	virtual void	Assign      (const RDEData&);
	virtual void    Assign     	(const DataObj&);
	virtual void    Assign      (const GUID&);
	virtual void    Assign      (const VARIANT&) { ASSERT(FALSE); }

	virtual void    Clear       (BOOL bValid = TRUE);

	virtual CString GetXMLType(BOOL bSoapType = TRUE) const { return SCHEMA_XSD_DATATYPE_STRING_VALUE;};	
	virtual CString FormatDataForXML(BOOL bSoapType = TRUE) const;
	virtual void	AssignFromXMLString(LPCTSTR);

	BSTR GetSoapValue()					{ return Str().AllocSysString(); }			
	void SetSoapValue(BSTR bstrValue)	{ Assign (CString(bstrValue)); };

public:
	virtual DataType    GetDataType	() const { return DataType(DATA_GUID_TYPE, 0); }

	// per Oracle
	virtual	int			GetAllocSize() const { return GUID_LEN ; }

	//@@OLEDB 
	virtual DBLENGTH	GetOleDBSize   (BOOL /*bUnicode*/ = TRUE) const { return sizeof(m_guid);}
	virtual void*		GetOleDBDataPtr() const { return (void*)(&m_guid); }

public:
	// al suo interno chiama l'API ::CoCreateGUID e assegna il nuovo GUID creato al
	// datamember m_guid
	void	AssignNewGuid();

	const GUID&	GetGUID	() const { return m_guid; }

public:
	// operatori di assegnazione
	const DataGuid& operator=(const DataGuid& g)	{ Assign(g); return *this; }
	const DataGuid& operator=(const GUID& g)		{ Assign(g); return *this; }
	const DataGuid& operator=(LPCTSTR g)				{ Assign(g); return *this; }

	// operatori di confronto
	friend BOOL operator==(const DataGuid&	g1, const DataGuid&	g2) { return g1.IsEqual(g2); }
	friend BOOL operator==(const DataGuid&	g1, const GUID&	g2)		{ return g1.IsEqual(g2); }
	friend BOOL operator==(const GUID& 	g1, const DataGuid&	g2)		{ return g2.IsEqual(g1); }
	friend BOOL operator==(const DataGuid& 	g1, LPCTSTR	g2)			{ return g1.IsEqual(g2); }
	friend BOOL operator==(LPCTSTR 	g1, const DataGuid&	g2)			{ return g2.IsEqual(g1); }

	// operatori di confronto
	friend BOOL operator!=(const DataGuid&	g1, const DataGuid&	g2) { return !g1.IsEqual(g2); }
	friend BOOL operator!=(const DataGuid&	g1, const GUID&	g2)		{ return !g1.IsEqual(g2); }
	friend BOOL operator!=(const GUID& 	g1, const DataGuid&	g2)		{ return !g2.IsEqual(g1); }
	friend BOOL operator!=(const DataGuid& 	g1, LPCTSTR	g2)			{ return !g1.IsEqual(g2); }
	friend BOOL operator!=(LPCTSTR 	g1, const DataGuid&	g2)			{ return !g2.IsEqual(g1); }

	friend BOOL operator<(const DataGuid&	g1, const DataGuid&	g2) { return g1.IsLessThan(g2); }
	friend BOOL operator<(const DataGuid&	g1, const GUID&	g2)		{ return g1.IsLessThan(DataGuid(g2)); }
	friend BOOL operator<(const GUID& 	g1, const DataGuid&	g2)		{ return (DataGuid(g1)).IsLessThan(g2); }
	friend BOOL operator<(const DataGuid& 	g1, LPCTSTR	g2)			{ return g1.IsLessThan(DataGuid(g2)); }
	friend BOOL operator<(LPCTSTR 	g1, const DataGuid&	g2)			{ return (DataGuid(g1)).IsLessThan(g2); }

	friend BOOL operator<=(const DataGuid&	g1, const DataGuid&	g2) { return g1 == g2 || g1 < g2; }
	friend BOOL operator<=(const DataGuid&	g1, const GUID&	g2)		{ return g1 == g2 || g1 < g2; }
	friend BOOL operator<=(const GUID& 	g1, const DataGuid&	g2)		{ return g1 == g2 || g1 < g2; }
	friend BOOL operator<=(const DataGuid& 	g1, LPCTSTR	g2)			{ return g1 == g2 || g1 < g2; }
	friend BOOL operator<=(LPCTSTR 	g1, const DataGuid&	g2)			{ return g1 == g2 || g1 < g2; }

	friend BOOL operator>(const DataGuid&	g1, const DataGuid&	g2) { return !(g1 <= g2); }
	friend BOOL operator>(const DataGuid&	g1, const GUID&	g2)		{ return !(g1 <= g2); }
	friend BOOL operator>(const GUID& 	g1, const DataGuid&	g2)		{ return !(g1 <= g2); }
	friend BOOL operator>(const DataGuid& 	g1, LPCTSTR	g2)			{ return !(g1 <= g2); }
	friend BOOL operator>(LPCTSTR 	g1, const DataGuid&	g2)			{ return !(g1 <= g2); }
	
	friend BOOL operator>=(const DataGuid&	g1, const DataGuid&	g2) { return !(g1 < g2); }
	friend BOOL operator>=(const DataGuid&	g1, const GUID&	g2)		{ return !(g1 < g2); }
	friend BOOL operator>=(const GUID& 	g1, const DataGuid&	g2)		{ return !(g1 < g2); }
	friend BOOL operator>=(const DataGuid& 	g1, LPCTSTR	g2)			{ return !(g1 < g2); }
	friend BOOL operator>=(LPCTSTR 	g1, const DataGuid&	g2)			{ return !(g1 < g2); }

// diagnostics
#ifdef _DEBUG
public:	
	void Dump(CDumpContext&) const;
	void AssertValid() const;
#endif
};

//lo faccio derivare da DataStr per avere la gestione completa del datamember m_strText
//==============================================================================
class TB_EXPORT DataText : public DataStr
{
	DECLARE_DYNCREATE (DataText)

	friend class SqlTable;
	friend class SqlRecord;

 
public:
	// constructors & destructor
	DataText ();
	DataText (LPCTSTR);
	DataText (const DataText&);

public:
	virtual DataType    GetDataType	() const { return DataType(DATA_TXT_TYPE, 0); }

	//@@OLEDB 
	virtual DBLENGTH	GetOleDBSize (BOOL /*bUnicode*/ = TRUE) const { return sizeof(IUnknown*);		}

	virtual void    Assign      (const DataObj& aDataObj);
	virtual void    Assign		(LPCTSTR pszString);
	virtual void	Assign      (TCHAR ch);

	virtual void	AppendFromSStream(BYTE* pBuf, int nLen, BOOL bUnicode, BOOL bSet = FALSE);

	// Permettono la gestione della riallocazione della stringa alla size voluta per
	// gestire correttamente il rebind alle colonne di database in OLEDB
	virtual	void	Allocate	()				{}
	virtual	void	SetAllocSize(int nSize)		{}
	virtual	int		GetAllocSize()  const		{ return 0; }
	virtual	int		GetColumnLen() const		{ return 0; }

public:
	// operatori di assegnazione
	const DataText& operator=(const DataText& datatxt)	{ Assign (datatxt);	return *this; }
	const DataText& operator=(const DataStr& datastr)	{ Assign (datastr);	return *this; }
	const DataText& operator=(const DataObj& i)			{ Assign (i);		return *this; } 
	const DataText& operator=(const CString& string)	{ Assign (string);	return *this; }
	const DataText& operator=(TCHAR ch)					{ Assign (ch);		return *this; }
	const DataText& operator=(LPCTSTR psz)				{ Assign (psz);		return *this; }

	// operatori di assegnazione ed incremento
	const DataText& operator+=(const DataText& datastr);
	const DataText& operator+=(const DataStr& datastr);
	const DataText& operator+=(const CString& string);
	const DataText& operator+=(TCHAR ch);
	const DataText& operator+=(LPCTSTR psz);

TB_EXPORT 	friend DataText operator+(const DataText&	s1, const DataText&	s2);
TB_EXPORT 	friend DataText operator+(const DataStr&	s1, const DataText&	s2);
TB_EXPORT 	friend DataText operator+(const DataText&	s1, const DataStr&	s2);
TB_EXPORT 	friend DataText operator+(const DataText&	s1, const CString&	s2);
TB_EXPORT 	friend DataText operator+(const CString&	s1,	const DataText&	s2);
TB_EXPORT 	friend DataText operator+(const DataText&	s1,	TCHAR			s2);
TB_EXPORT 	friend DataText operator+(TCHAR 			s1,	const DataText&	s2);
TB_EXPORT 	friend DataText operator+(const DataText&	s1,	LPCTSTR			s2);
TB_EXPORT 	friend DataText operator+(LPCTSTR			s1,	const DataText&	s2);

	// operatori di confronto
TB_EXPORT 	friend BOOL operator==(const DataText&	s1,	const DataText&	s2);
TB_EXPORT 	friend BOOL operator==(const DataText&	s1,	const DataStr&	s2);
TB_EXPORT 	friend BOOL operator==(const DataStr&	s1,	const DataText&	s2);
TB_EXPORT 	friend BOOL operator==(const CString&	s1,	const DataText&	s2);
TB_EXPORT 	friend BOOL operator==(const DataText&	s1,	const CString&	s2);
TB_EXPORT 	friend BOOL operator==(const DataText&	s1,	LPCTSTR			s2);
TB_EXPORT 	friend BOOL operator==(LPCTSTR			s1, const DataText&	s2);
	
TB_EXPORT	friend BOOL operator<(const DataText&	s1, const DataText&	s2);
TB_EXPORT	friend BOOL operator<(const DataText&	s1, const DataStr&	s2);
TB_EXPORT	friend BOOL operator<(const DataStr&	s1, const DataText&	s2);
TB_EXPORT	friend BOOL operator<(const DataText&	s1, const CString&	s2);
TB_EXPORT	friend BOOL operator<(const CString&	s1, const DataText&	s2);
TB_EXPORT	friend BOOL operator<(const DataText&	s1, LPCTSTR			s2);
TB_EXPORT	friend BOOL operator<(LPCTSTR			s1, const DataText&	s2);

TB_EXPORT 	friend BOOL operator>(const DataText&	s1, const DataText&	s2);
TB_EXPORT 	friend BOOL operator>(const DataText&	s1, const DataStr&	s2);
TB_EXPORT 	friend BOOL operator>(const DataStr&	s1, const DataText&	s2);
TB_EXPORT 	friend BOOL operator>(const DataText&	s1, const CString&	s2);
TB_EXPORT 	friend BOOL operator>(const CString&	s1, const DataText&	s2);
TB_EXPORT 	friend BOOL operator>(const DataText&	s1, LPCTSTR			s2);
TB_EXPORT 	friend BOOL operator>(LPCTSTR			s1, const DataText&	s2);

TB_EXPORT	friend BOOL operator!=(const DataText&	s1,	const DataText&	s2) { return !(s1 == s2); }
TB_EXPORT	friend BOOL operator!=(const DataText&	s1,	const DataStr&	s2) { return !(s1 == s2); }
TB_EXPORT	friend BOOL operator!=(const DataStr&	s1,	const DataText&	s2) { return !(s1 == s2); }
TB_EXPORT	friend BOOL operator!=(const DataText&	s1,	const CString&	s2) { return !(s1 == s2); }
TB_EXPORT	friend BOOL operator!=(const CString&	s1,	const DataText&	s2) { return !(s1 == s2); }
TB_EXPORT	friend BOOL operator!=(const DataText&	s1,	LPCTSTR			s2) { return !(s1 == s2); }
TB_EXPORT	friend BOOL operator!=(LPCTSTR			s1,	const DataText&	s2) { return !(s1 == s2); }

TB_EXPORT	friend BOOL operator<=(const DataText&	s1, const DataText&	s2) { return !(s1 > s2); }
TB_EXPORT	friend BOOL operator<=(const DataText&	s1, const DataStr&	s2) { return !(s1 > s2); }
TB_EXPORT	friend BOOL operator<=(const DataStr&	s1, const DataText&	s2) { return !(s1 > s2); }
TB_EXPORT	friend BOOL operator<=(const DataText&	s1, const CString&	s2) { return !(s1 > s2); }
TB_EXPORT	friend BOOL operator<=(const CString&	s1, const DataText&	s2) { return !(s1 > s2); }
TB_EXPORT	friend BOOL operator<=(const DataText&	s1, LPCTSTR			s2) { return !(s1 > s2); }
TB_EXPORT	friend BOOL operator<=(LPCTSTR			s1, const DataText&	s2) { return !(s1 > s2); }

TB_EXPORT	friend BOOL operator>=(const DataText&	s1, const DataText&	s2) { return !(s1 < s2); }
TB_EXPORT	friend BOOL operator>=(const DataText&	s1, const DataStr&	s2) { return !(s1 < s2); }
TB_EXPORT	friend BOOL operator>=(const DataStr&	s1, const DataText&	s2) { return !(s1 < s2); }
TB_EXPORT	friend BOOL operator>=(const DataText&	s1, const CString&	s2) { return !(s1 < s2); }
TB_EXPORT	friend BOOL operator>=(const CString&	s1, const DataText&	s2) { return !(s1 < s2); }
TB_EXPORT	friend BOOL operator>=(const DataText&	s1, LPCTSTR			s2) { return !(s1 < s2); }
TB_EXPORT	friend BOOL operator>=(LPCTSTR			s1, const DataText&	s2) { return !(s1 < s2); }

#ifdef _DEBUG
public:	
	void Dump(CDumpContext& dc) const {	ASSERT_VALID(this); AFX_DUMP0(dc, " DataText\n"); }
	void AssertValid() const{ DataStr::AssertValid(); }
#endif //_DEBUG
};

//@@TODO da rifare per OLEDB
//==============================================================================
class TB_EXPORT DataBlob : public DataObj
{
	DECLARE_DYNCREATE (DataBlob)

	friend class SqlTable;
	friend class SqlRecord;

protected:
	BYTE*   m_pBuffer;
	int		m_nAllocSize;
	int		m_nUsedLen;
    
public:
	// constructors & destructor
	DataBlob ();
	DataBlob (const DataBlob&);
	DataBlob (void* pBuf, int nSize);

	virtual ~DataBlob ();

public:
	const DataBlob& operator=(const DataBlob& o)		{ Assign (o);	return *this; }
	const DataBlob& operator=(const DataObj& o)			{ Assign (o);		return *this; } 
	const DataBlob& operator=(const TCHAR* o)			{ Assign (o);	return *this; }
	const DataBlob& operator=(const RDEData& o)			{ Assign (o);		return *this; } 
	const DataBlob& operator=(const VARIANT& o)			{ Assign (o);		return *this; } 

	// overridable
	virtual BOOL		IsEmpty		()  const { return m_nAllocSize == 0 || m_nUsedLen == 0 || m_pBuffer == NULL; }
	virtual int         IsEqual     (const DataObj&)	const;
	virtual int         IsLessThan  (const DataObj&)  	const { return FALSE; }

	virtual CString     Str         (int = -1, int = -1)const;

	virtual void*       GetRawData  (DataSize* = NULL)	const;

	virtual DataType	GetDataType	() const{ return DataType(DATA_BLOB_TYPE, 0); }

	//@@OLEDB
	virtual DBLENGTH	GetOleDBSize   (BOOL /*bUnicode*/ = TRUE) const { return m_nAllocSize;}
	virtual void*		GetOleDBDataPtr() const { return (void*)(&m_pBuffer); }

    virtual void    Assign      (const TCHAR*);
	virtual void    Assign      (const VARIANT&);
	virtual void	Assign      (const RDEData&);
	virtual void    Assign     	(const DataObj&);
	virtual void    Assign      (void* pBuf, int nSize);

	virtual void    Clear       (BOOL bValid = TRUE);

	// Permettono la gestione della riallocazione della stringa alla size voluta per
	// gestire correttamente il rebind alle colonne di database in ODBC
	virtual	void	Allocate	();
	virtual	void	SetAllocSize(int nSize);
	virtual	int		GetAllocSize() const		{ return m_nAllocSize; }

	virtual CString GetXMLType(BOOL bSoapType = TRUE) const { return SCHEMA_XSD_DATATYPE_STRING_VALUE;};	
	virtual CString FormatDataForXML(BOOL bSoapType = TRUE) const;
	virtual void	AssignFromXMLString(LPCTSTR);

public:
	BYTE* GetBuffer() const { return m_pBuffer; }
	void  SetBuffer(BYTE* pBuffer, int nUsedLen);
	void  Null(); //mette a null il puntatore pBuffer, senza cancellare la memoria. Sar� chi consuma l'area di memoria di m_pBuffer ad occuparsi della memoria

	SAFEARRAY* GetSoapValue() const;			
	void SetSoapValue(SAFEARRAY*);	
	void SetUsedLen(int nLen) { m_nUsedLen = nLen; }

public:
	// non overridable
	int             GetLen		() const	{ return m_nUsedLen+1;	}

// diagnostics
#ifdef _DEBUG
public:	
	void Dump(CDumpContext&) const;
	void AssertValid() const;
#endif
};

///////////////////////////////////////////////////////////////////////////////
//				DataObj inlines
////////////////////////////////////////////////////////////////////////////////
//

inline BOOL DataObj::IsValid			()		const		{ return (m_wDataStatus & VALID)			== VALID;			}
inline BOOL DataObj::IsModified			()		const		{ return (m_wDataStatus & MODIFIED)			== MODIFIED;		}
inline BOOL DataObj::IsUpperCase		()		const		{ return (m_wDataStatus & UPPERCASE)		== UPPERCASE ;		}
inline BOOL DataObj::IsDBCaseCompliant	()		const		{ return (m_wDataStatus & DBCASE_COMPLIANT) == DBCASE_COMPLIANT;}
inline BOOL DataObj::IsValueChanged		()		const		{ return (m_wDataStatus & VALUE_CHANGED)	== VALUE_CHANGED;	}
inline BOOL DataObj::IsDirty			()		const		{ return (m_wDataStatus & DIRTY)			== DIRTY;			}
inline BOOL DataObj::IsFindable			()		const		{ return (m_wDataStatus & FINDABLE)			== FINDABLE;		}
inline BOOL DataObj::IsFullDate			()		const		{ return (m_wDataStatus & FULLDATE)			== FULLDATE ;		}
inline BOOL DataObj::IsATime			()		const		{ return (m_wDataStatus & TIME)				== TIME ;			}
inline BOOL DataObj::IsUpdateView	 	()		const		{ return (m_wDataStatus & UPDATE_VIEW)		== UPDATE_VIEW;		}

inline BOOL DataObj::IsValueLocked		()		const		{ return (m_wDataStatus & VALUE_LOCKED)		== VALUE_LOCKED;	}
inline BOOL DataObj::IsHandle			()		const		{ return (m_wDataStatus & TB_HANDLE)		== TB_HANDLE;		}
inline BOOL DataObj::IsAccountable		()		const		{ return (m_wDataStatus & ACCOUNTABLE)		== ACCOUNTABLE;		}

inline BOOL DataObj::IsPrivate			()		const		{ return (m_wDataStatus & PRIVATE)			== PRIVATE;			}

inline void DataObj::SetValid			(BOOL bValid)		{ SetStatus(bValid,		VALID);				}
inline void DataObj::SetModified		(BOOL bModified)	{ SetStatus(bModified,	MODIFIED);			}
inline void DataObj::SetUpperCase		(BOOL bUpperCase)	{ SetStatus(bUpperCase, UPPERCASE);			ASSERT(GetDataType() == DATA_STR_TYPE); }
inline void DataObj::SetDBCaseCompliant	(BOOL bDBCaseComp)	{ SetStatus(bDBCaseComp,DBCASE_COMPLIANT);	ASSERT(GetDataType() == DATA_STR_TYPE); }
inline void DataObj::SetValueChanged	(BOOL bChanged)		{ SetStatus(bChanged,	VALUE_CHANGED);		}
inline void DataObj::SetDirty			(BOOL bDirty)		{ SetStatus(bDirty,		DIRTY);				}
inline void DataObj::SetFullDate		(BOOL bFullDate)	{ SetStatus(bFullDate,  FULLDATE);			ASSERT(GetDataType() == DATA_DATE_TYPE); }
inline void DataObj::SetAccountable		(BOOL value)		{ SetStatus(value,		ACCOUNTABLE);		ASSERT(GetDataType() == DATA_MON_TYPE); }

inline void DataObj::SetUpdateView		(BOOL bChanged)		{ SetStatus(bChanged,	UPDATE_VIEW);		}

inline void DataObj::SetHide			(BOOL bHide)		{ SetStatus(bHide,		HIDE); SetModified(); }
inline void DataObj::SetOSLHide			(BOOL bHide)		{ SetStatus(bHide,		OSL_HIDE); }

inline void DataObj::SetPrivate			(BOOL bPrivate)		{ SetStatus(bPrivate,	PRIVATE); }


////////////////////////////////////////////////////////////////////////////////
//@@ElapsedTime			inlines for DataLng as ElapsedTime
////////////////////////////////////////////////////////////////////////////////
//
inline double	DataLng::GetTotalSeconds()	const	{ ASSERT(IsATime()); return (double) m_nValue / GetElapsedTimePrecision(); }
inline long		DataLng::GetDays		()	const	{ ASSERT(IsATime()); return (long) GetTotalSeconds() / (24L * 3600L); }
inline long		DataLng::GetTotalHours	()	const	{ ASSERT(IsATime()); return (long) GetTotalSeconds() / 3600L; }
inline long		DataLng::GetHours		()	const	{ ASSERT(IsATime()); return GetTotalHours() - GetDays() * 24L; }
inline long		DataLng::GetTotalMinutes()	const	{ ASSERT(IsATime()); return (long) GetTotalSeconds() / 60L; }
inline long		DataLng::GetMinutes		()	const	{ ASSERT(IsATime()); return GetTotalMinutes() - GetTotalHours() * 60L; }
inline double	DataLng::GetSeconds		()	const	{ ASSERT(IsATime()); return GetTotalSeconds() - GetTotalMinutes() * 60L; }
inline double	DataLng::GetDecDays		()	const	{ ASSERT(IsATime()); return GetTotalSeconds() / (24.* 3600.); }
inline double	DataLng::GetDecHours	()	const	{ ASSERT(IsATime()); return GetTotalSeconds() / 3600.; }
inline double	DataLng::GetDecMinutes	()	const	{ ASSERT(IsATime()); return GetTotalSeconds() / 60.; }

// 1 frazione di minuto centesimale = (10.000/3.600) * frazione di minuto sessantesimale
// 1 secondo centesimale = (10.000/3.600) / 100 * seconto sessantesimale = 1/36
inline double	DataLng::GetCentHours	()	const	{ ASSERT(IsATime()); return GetTotalSeconds() / 36.; }


//////////////////////////////////////////////////////////////////////////////
//					DataObjArray definition
//////////////////////////////////////////////////////////////////////////////
//
class TB_EXPORT DataObjArray : public Array
{
	DECLARE_DYNCREATE (DataObjArray);

public:
	DataObjArray () {}
	DataObjArray (const DataObjArray& a) { *this = a; }
	~DataObjArray () {}
public:
	BOOL	IsEqual			(const DataObjArray&) const;
	BOOL	IsLessThan		(const DataObjArray&) const;

public:
	DataObj* 	GetAt		(int nIndex) const	{ return (DataObj*) Array::GetAt(nIndex);	}
	DataObj*&	ElementAt	(int nIndex)		{ return (DataObj*&) Array::ElementAt(nIndex); }
	int			Add			(DataObj* pObj)		{ return Array::Add((CObject*)pObj); }
	
	DataObj* 	operator[]	(int nIndex) const	{ return GetAt(nIndex);	}
	DataObj*& 	operator[]	(int nIndex)		{ return ElementAt(nIndex);	}

	DataObjArray&	operator=	(const DataObjArray& a);
	BOOL			operator==	(const DataObjArray& a) const { return IsEqual	(a); }
	BOOL			operator!=	(const DataObjArray& a) const { return !IsEqual	(a); }

	void		Assign			(CString str, DataType dt);

	void		ToStringArray	(CStringArray& ars) const;	//uses DataObj::Str()
	CString		ToString		() const;
	CString		ToSqlString		() const;

	INT_PTR Append	(const DataObjArray& src);
	void	Copy	(const DataObjArray& src);

	virtual DataObjArray*	Clone		(LPCSTR pszFile = NULL, int nLine = 0) const;

	virtual BOOL	LessThen	(CObject* po1, CObject* po2) const;
	virtual int		Compare		(CObject* po1, CObject* po2) const;
	virtual BOOL	IsElementEqual (CObject* po1, CObject* po2) const;

	virtual int		Find (const DataObj* pObj, int nStartPos = 0, BOOL noCase = FALSE) const;

	template <class T>	void	CalcSum (DataObj& aSum) const;
						void	CalcSum (DataObj& aSum) const;
						
	template <class T>	void	CalcPercentages(DataObjArray& arPercentages) const;

	DataObj* 	GetMinElem		() const;
	DataObj* 	GetMaxElem		() const;

#ifdef _DEBUG	
	virtual void	Dump(CDumpContext& dc) const;
	virtual void	AssertValid() const;
#endif //_DEBUG
};

///////////////////////////////////////////////////////////////////////////////////

class TB_EXPORT DataObjArrayOfArray : public Array
{
	DECLARE_DYNCREATE(DataObjArrayOfArray);

public:
	DataObjArrayOfArray () {}
	DataObjArrayOfArray (const DataObjArrayOfArray& a) { *this = a; }
	~DataObjArrayOfArray() {}
public:
	BOOL	IsEqual	(const DataObjArrayOfArray&) const;

public:
	DataObjArray*	GetAt		(int nIndex) const	{ return (DataObjArray*) __super::GetAt(nIndex); }
	DataObj*		GetAt		(int nIndex, int nIndex2) const	{ return (DataObj*) ((DataObjArray*)__super::GetAt(nIndex))->GetAt(nIndex2); }
	DataObjArray*&	ElementAt	(int nIndex)		{ return (DataObjArray*&) __super::ElementAt(nIndex); }
	int				Add			(DataObjArray* pObj) { return __super::Add(pObj); }

	DataObjArray* 	operator[]	(int nIndex) const	{ return GetAt(nIndex);	}
	DataObjArray*& 	operator[]	(int nIndex)		{ return ElementAt(nIndex);	}

	DataObjArrayOfArray&	operator=	(const DataObjArrayOfArray& a);
	BOOL					operator==	(const DataObjArrayOfArray& a) const { return IsEqual	(a); }
	BOOL					operator!=	(const DataObjArrayOfArray& a) const { return !IsEqual	(a); }
};

//////////////////////////////////////////////////////////////////////////////
//             Class DataStrArray definition
//////////////////////////////////////////////////////////////////////////////
//                                                                          
//=============================================================================
class TB_EXPORT DataStrArray : public DataObjArray
{
	DECLARE_DYNCREATE (DataStrArray)

public:
	DataStr* 	GetAt		(int nIndex)const	{ return (DataStr*) DataObjArray::GetAt(nIndex);	}
	DataStr*&	ElementAt	(int nIndex)		{ return (DataStr*&) DataObjArray::ElementAt(nIndex); }
	
	DataStr* 	operator[]	(int nIndex)const	{ return GetAt(nIndex);	}
	DataStr*& 	operator[]	(int nIndex)		{ return ElementAt(nIndex);	}

public:
	void operator=  (const DataStrArray&);	
	void operator=  (const CStringArray&);	
};

//////////////////////////////////////////////////////////////////////////////
//             Class DataTypeNamedArray definition
//////////////////////////////////////////////////////////////////////////////
//                                                                          
//=============================================================================
class TB_EXPORT DataTypeNamedArray : public Array
{
	DECLARE_DYNAMIC (DataTypeNamedArray)

public:
	class DataTypeNamed : public CObject
	{
	public:
		CString		m_strName;
		DataType	m_DataType;

		DataTypeNamed () 
			: m_DataType(DataType::Null) {}
		DataTypeNamed (CString strName, DataType dt) 
			: m_DataType(dt), m_strName(strName) {}
	};

	DataTypeNamedArray::DataTypeNamed* 	GetAt		(int nIndex)const	{ return (DataTypeNamedArray::DataTypeNamed*) Array::GetAt(nIndex);	}
	DataTypeNamedArray::DataTypeNamed*&	ElementAt	(int nIndex)		{ return (DataTypeNamedArray::DataTypeNamed*&) Array::ElementAt(nIndex); }
	
	DataTypeNamedArray::DataTypeNamed* 	operator[]	(int nIndex)const	{ return GetAt(nIndex);	}
	DataTypeNamedArray::DataTypeNamed*& operator[]	(int nIndex)		{ return ElementAt(nIndex);	}

	int	Add	(DataTypeNamedArray::DataTypeNamed* dbm)		{ return Array::Add(dbm); }
	int	Add	(CString strName, DataType dt)					{ return Add(new DataTypeNamedArray::DataTypeNamed(strName, dt)); }
};

//============================================================================
class TB_EXPORT DataArray : public DataObj
{
	DECLARE_DYNCREATE (DataArray)

protected:
	DataObjArray	m_arData;
	DataType		m_BaseDataType;
	//CString			m_sRecordName;
	
public:
	// constructors & destructor
	DataArray ();
	DataArray (const DataArray& ar) { Assign(ar); }
	DataArray (DataType baseType) { m_BaseDataType = baseType; }
	virtual ~DataArray () {}

	DataArray&	operator=	(const DataArray& ar)			{ 	Assign(ar); return *this; }

	DataObjArray&	GetData			()				{ return m_arData; }

	DataObj* 	operator[]	(int nIndex) const	{ return m_arData.GetAt(nIndex); }
	DataObj*& 	operator[]	(int nIndex)		{ return m_arData.ElementAt(nIndex); }

	DataObj* 	GetAt		(int nIndex) const				{ return m_arData.GetAt(nIndex); }
	void		SetAt		(int nIndex, DataObj* pObj);
	void		SetAtGrow	(int nIndex, DataObj* pObj);

	void		InsertAt	(INT_PTR nIndex, CObject* newElement, INT_PTR nCount = 1) 
															{ m_arData.InsertAt(nIndex, newElement, nCount); }
	void		RemoveAt	(INT_PTR nIndex, INT_PTR nCount = 1) { m_arData.RemoveAt(nIndex, nCount); }

	int			Add			(DataObj* pObj)					{ return m_arData.Add(pObj); }
	BOOL		Append		(const CStringArray&);

	int			GetUpperBound() const						{ return m_arData.GetUpperBound(); }
	int			GetSize		() const						{ return m_arData.GetSize(); }
	void		SetSize		(INT_PTR nNewSize) 				{ m_arData.SetSize(nNewSize); }
	void		RemoveAll	()								{ m_arData.RemoveAll(); }

	virtual void    Assign	(const DataObj& ar);

	DataType		GetBaseDataType	() const		{ return m_BaseDataType; }
	void			SetBaseDataType	(DataType t)	{ m_BaseDataType = t; }

	BOOL			Attach			(DataArray*);
	BOOL			Attach			(CObArray*);
	void			Detach			();

	// overridable
	virtual BOOL	IsEmpty		() const { return m_arData.GetSize() == 0; }

	virtual BOOL    IsEqual     (const DataObj& ar)	const;
	virtual BOOL    IsLessThan  (const DataObj& ar)	const;
	BOOL			operator==	(const DataArray& ar) const { return IsEqual (ar); }
	BOOL			operator!=	(const DataArray& ar) const { return !IsEqual (ar); }

	virtual DataType    GetDataType () const { return DataType(DATA_ARRAY_TYPE, 0); }

	virtual void		Clear	(BOOL bValid = TRUE);

	virtual CString     Str				(int = -1, int = -1) const;
	virtual CString     ToString		() const;
			void		ToStringArray	(CStringArray& ars) const { m_arData.ToStringArray(ars); }

			int			Find	(DataObj* pVal, int nStartPos = 0, BOOL bNoCase = FALSE) const	{ return m_arData.Find(pVal, nStartPos, bNoCase); }
			BOOL		Sort	(BOOL bDescending = FALSE, int start = 0, int end = -1)			{ return m_arData.Sort(bDescending, start, end); }

			void 		CalcSum			(DataObj&) const;
			DataObj* 	GetMinElem		() const;
			DataObj* 	GetMaxElem		() const;
			void		CalcPercentages	(DataObjArray& arPercentages) const;

	//TODO i metodi seguneti non sono implementati
	virtual void    Assign      (const TCHAR*);
	virtual void    Assign      (const VARIANT&);
	virtual void	Assign      (const RDEData&);

	virtual void*   GetRawData  (DataSize* = NULL)	const { ASSERT(FALSE); return NULL; }

	virtual CString GetXMLType(BOOL bSoapType = TRUE) const { ASSERT(FALSE); return _T(""); }
	virtual CString FormatDataForXML(BOOL bSoapType = TRUE) const;
	virtual void	AssignFromXMLString(LPCTSTR);

	virtual DBLENGTH	GetOleDBSize   (BOOL /*bUnicode*/ = TRUE) const { ASSERT(FALSE); return 0;}
	virtual void*		GetOleDBDataPtr() const { ASSERT(FALSE); return NULL; }
	virtual void		SerializeJsonValue(CJsonSerializer& jsonSerializer);

	BOOL	FixDataType(DataType newType);
// diagnostics
#ifdef _DEBUG
public:	
	void Dump(CDumpContext&) const;
	void AssertValid() const;
#endif
};

//============================================================================
class TB_EXPORT DataRecord : public DataArray
{
	DECLARE_DYNCREATE(DataRecord)

public:
	// constructors & destructor
	DataRecord() {}
	DataRecord(const DataRecord& ar) { Assign(ar); }
	virtual ~DataRecord() {}

	virtual DataType    GetDataType() const { return DataType(DATA_RECORD_TYPE, 0); }

};

//============================================================================
class TB_EXPORT DataSqlRecord : public DataObj
{
	DECLARE_DYNCREATE (DataSqlRecord)

protected:
	ISqlRecord*	m_pRecord;
	BOOL		m_bOwnRecord;

public:
	// constructors & destructor
	DataSqlRecord ();
	DataSqlRecord (const DataSqlRecord& ar);
	DataSqlRecord (ISqlRecord* pRec, BOOL bOwnRecord);
	~DataSqlRecord ();

	DataSqlRecord&	operator=	(const DataSqlRecord& ar) {  Assign(ar); return *this; }

	virtual void    Assign	(const DataObj& ar);

	ISqlRecord*		GetIRecord	() { return m_pRecord; }
	void			SetIRecord	(ISqlRecord* rec, BOOL bOwnRecord);

	// overridable
	virtual BOOL	IsEmpty		() const { return m_pRecord == NULL; }

	virtual BOOL    IsEqual     (const DataObj& ar)	const;
	virtual BOOL    IsLessThan  (const DataObj& ar)	const;
	BOOL			operator==	(const DataArray& ar) const { return IsEqual (ar); }
	BOOL			operator!=	(const DataArray& ar) const { return !IsEqual (ar); }

	virtual DataType    GetDataType () const { return DataType(DATA_SQLRECORD_TYPE, 0); }

	virtual void		Clear	(BOOL bValid = TRUE);

	virtual CString     Str				(int = -1, int = -1) const;
	virtual CString     ToString		() const;

	//TODO i metodi seguneti non sono implementati
	virtual void    Assign      (const TCHAR*);
	virtual void    Assign      (const VARIANT&);
	virtual void	Assign      (const RDEData&);

	virtual void*   GetRawData  (DataSize* = NULL)	const { ASSERT(FALSE); return NULL; }

	virtual CString GetXMLType(BOOL bSoapType = TRUE) const { ASSERT(FALSE); return _T(""); }
	virtual CString FormatDataForXML(BOOL bSoapType = TRUE) const;
	virtual void	AssignFromXMLString(LPCTSTR);

	virtual DBLENGTH	GetOleDBSize   (BOOL /*bUnicode*/ = TRUE) const { ASSERT(FALSE); return 0;}
	virtual void*		GetOleDBDataPtr() const { ASSERT(FALSE); return NULL; }

// diagnostics
#ifdef _DEBUG
public:	
	void Dump(CDumpContext&) const;
	void AssertValid() const;
#endif
};
//==============================================================================
// CGuid: incapsulates GUID structure
//////////////////////////////////////////////////////////////////////

class TB_EXPORT CGuid : public CObject  
{
	DECLARE_DYNCREATE(CGuid)

// Construction
public:
	CGuid();					// standard constructor
	CGuid(const CGuid& guid);	// copy constructor
	CGuid(const GUID& guid);	// from a GUID to a CGuid
	CGuid(LPCTSTR lpszGuid);	// from a string ({1234ABCD-1111-2222-...}) 
								// to a CGuid - throws AfxOleException
	virtual ~CGuid() {}			// standard distructor

// Attributes
public:
	GUID GetValue() const;
	void SetValue(const GUID& newValue);
	
// Operations
public:
	void GenerateGuid();		// generates a new GUID
	
	CGuid& operator=(const CGuid& guid);
	CGuid& operator=(const GUID& guid);
	CGuid& operator=(LPCTSTR lpszGuid);	// throws AfxOleException
	BOOL operator==(const CGuid& guid) const;
	BOOL operator!=(const CGuid& guid) const;
	BOOL operator<(const CGuid& guid) const;
	BOOL operator>(const CGuid& guid) const;

	operator LPGUID();	// converts to a pointer to a GUID
	operator CString() const;	// converts to a CString ({1234ABCD-1111-...})

protected:
	GUID m_guid;

protected:
	void GuidFromString(LPCTSTR lpsz, GUID* pGuid);

// CDiagnostic support
#ifdef _DEBUG
public:
	virtual void AssertValid() const;
	virtual void Dump(CDumpContext& dc) const;
#endif
};

typedef WORD	RDEcmd;		// IRDEManager::Command (not less than WORD)

// max valid id for total fields
#define MAX_ID_FOR_TOTAL	WORD(0x7FFF)
//============================================================================
class TB_EXPORT RDEData
{
public:
	RDEData		();
	RDEData		( const RDEData& );
	RDEData		( RDEcmd Status, DataSize Size, void* pData, BOOL bKeepPropr );
	RDEData		( RDEcmd Status, const CString& str );
	~RDEData	();

	const RDEData&	operator =	(const RDEData&);
	const RDEData&	CloneData	(const RDEData&);

	// Used by Writer engine
	void	SetData		( RDEcmd status, const DataObj*, BOOL bIsTailMultiLineString = FALSE );
	// Used by Reader engine
	void*	NewData		( DataSize, RDEcmd, BOOL bValid = TRUE, BOOL bIsOwner = TRUE );

	RDEcmd		GetStatus	() const;
	DataSize	GetLen		() const;
	void*		GetData		() const;

	BOOL	IsEnabled	() const;

	BOOL	IsTailMultiLineString () const { return m_bIsTailMultiLineString; };
	void	ThisIsTailMultiLineString (BOOL bIs) { m_bIsTailMultiLineString = bIs; };

	void	DisableData	();

    BOOL	IsColTotal		() const;
	BOOL	IsSubTotal		() const;
	WORD	GetColTotalId   () const;
	WORD	GetSubTotalId	() const;

	BOOL	IsValid			() const { return m_bValid; }
	void	ResetValid		()  { m_bValid = FALSE; }

	static	RDEcmd	GetSubTotalKind ( WORD wID );
	static	RDEcmd	GetColTotalKind ( WORD wID );
	static	BOOL	IsColTotalKind	( RDEcmd );
	static	BOOL	IsSubTotalKind	( RDEcmd );

private:
	RDEcmd		m_Status;
	DataSize	m_Len;	// data len
	DataSize	m_Size;	// allocated size (different from len if data is string)
	void*		m_pData;
	BOOL		m_bValid;
    BOOL		m_bIsDataOwner;
	BOOL		m_bIsTailMultiLineString;

	DataStr		m_dsOverflowError;
};

//============================================================================
class TB_EXPORT BaseField : public CObject, public IDisposingSourceImpl
{
	friend class SymTable;
	DECLARE_DYNAMIC(BaseField)
protected:
	CString		m_strName;
	DataType    m_DataType;
	DataObj*	m_pData = NULL;
	BOOL		m_bOwnData = TRUE;

public:
	BaseField(const CString& strName, DataType dt = DataType::Null, DataObj* pValue = NULL, BOOL bCloneValue = TRUE);
	BaseField(const BaseField&);
	virtual ~BaseField();

	const CString&		GetName		() const { return m_strName; }	
	DataType 			GetDataType	() const { return m_DataType; }

	BOOL		IsArray() const { return GetDataType().m_wType == DATA_ARRAY_TYPE; }

			void	SetDataPtr(DataObj* pData, BOOL bOwnData = TRUE);
	virtual void	SetDataType(const DataType& newDataType, BOOL bArray = FALSE);

	virtual DataObj*	GetData(int /*nDataLevel = -1*/) const;
	virtual void		AssignData(const DataObj& aData);

protected:
	BOOL AllocData();

#ifdef _DEBUG
public:
	virtual void Dump(CDumpContext& dc) const;
	virtual void AssertValid() const;
#endif //_DEBUG
};

//============================================================================
class TB_EXPORT IDataProvider
{
public:
	virtual int			GetCurrentRowIdx	() const = 0;
	virtual void		SetCurrentRow		(int /*nRow*/) = 0;
	virtual int			GetRowCount			() const = 0;

	virtual DataObj*	GetData				(const CString& /*sColumnName*/, int /*nRow*/ = -1) = 0;
	virtual	int		    FindRecordIndex		(const CString& /*sColumnName*/, const DataObj* /*value*/, int /*nStartPos*/ = 0) const = 0;

	virtual BOOL		CalcSum				(const CString& /*sColumnName*/, DataObj& /*aSum*/) const = 0;
	virtual DataObj*	GetMinElem			(const CString& /*sColumnName*/) = 0;
	virtual DataObj*	GetMaxElem			(const CString& /*sColumnName*/) = 0;
};

//============================================================================
#include "endh.dex"
