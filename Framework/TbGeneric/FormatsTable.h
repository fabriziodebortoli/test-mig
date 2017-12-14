#pragma once

#include <TbNameSolver\TBNamespaces.h>
#include <TbGeneric\Array.h>
#include <TbGeneric\DataObj.h>
#include <TbGeneric\JsonTags.h>

//includere alla fine degli include del .H
#include "beginh.dex"

class CTBNamespace;
class CJsonSerializer;
typedef int	FormatIdx;


// caratteristiche di un singolo formattatore
//============================================================================
class TB_EXPORT Formatter : public CObject
{
	friend class CFormatDlg;
	friend class CFormatDlgRoot;
	
	DECLARE_DYNAMIC(Formatter)

public:
	enum AlignType { NONE=0x0000, LEFT=0x0001, RIGHT=0x0002 };
	enum FormatStyleSource { FROM_STANDARD, FROM_CUSTOM, FROM_WOORM };

protected:
	CString				m_strName;
	DataType			m_OwnType;
	CTBNamespace		m_OwnerModule;
	FormatStyleSource	m_FromAndTo;

	BOOL				m_bChanged;
	BOOL				m_bDeleted;
	BOOL				m_bEditable;
						
	int					m_nPaddedLen;
	int					m_nOutputCharLen;
	int					m_nInputCharLen;
    AlignType			m_Align;
	CString				m_strHead;
	CString				m_strTail;
	CStringArray		m_LimitedContextArea;
	BOOL				m_bZeroPadded;

private:
	Formatter*			m_pStandardFormatter;

public:
	Formatter (const CString& sName, const FormatStyleSource aSource, const CTBNamespace& aOwner);

public:
	// read methods
	FormatStyleSource	GetSource		() const { return m_FromAndTo; }
	const CTBNamespace&	GetOwner		() const { return m_OwnerModule; }
	const CString&		GetName			() const { return m_strName; }
	const CString		GetTitle		() const;
	const DataType&		GetDataType		() const { return m_OwnType; }
	const int&			GetPaddedLen	() const { return m_nPaddedLen; }
	const int&			GetOutputCharLen() const { return m_nOutputCharLen; }
	const int&			GetInputCharLen	() const { return m_nInputCharLen; }
	const int			GetAlign		() const { return m_Align; }
	const CString&		GetHead			() const { return m_strHead; }
	const int			GetHeadLength	() const { return m_strHead.GetLength(); }
	const int			GetTailLength	() const { return m_strTail.GetLength(); }
	const CString&		GetTail			() const { return m_strTail;}
	const CString		GetLimitedArea	() const;
	const CStringArray&	GetLimitedAreas	() const;
	BOOL				IsZeroPadded	() const	{ return m_bZeroPadded;		}
	// set methods
	void	SetAlign		(const Formatter::AlignType& nAlign){ m_Align	= nAlign;	}
	void	SetHead			(const CString& sHead)				{ m_strHead = sHead;	}
	void	SetTail			(const CString& sTail)				{ m_strTail = sTail;	}
	void	SetPaddedLen	(const int& nLen)					{ m_nPaddedLen = nLen;	}
	void	SetEditable		(const BOOL& bValue)				{ m_bEditable	= bValue; }
	void	SetOwner		(const CTBNamespace& aOwner)		{ m_OwnerModule = aOwner; }
	void	SetLimitedArea	(const CString& sArea);
	void	SetLimitedAreas	(const CStringArray& aAreas);
	void	SetStandardFormatter (Formatter* pFormatter);
	void	SetZeroPadded	(const BOOL& bValue) { m_bZeroPadded = bValue; }
	void	SetChanged		(const BOOL& bValue) { m_bChanged = bValue; }
	void	SetDeleted		(const BOOL& bValue) { m_bDeleted = bValue; }
	
	BOOL	IsChanged		()	{ return m_bChanged;}
	BOOL	IsDeleted		()	{ return m_bDeleted;}

	void	FormatDataObj	(const DataObj& aDataObj, CString& Str, BOOL bPaddingEnabled = TRUE) const;
	void	Padder			(CString& strToPadd, BOOL padRight) const;
	
	Formatter*	Clone		()	const;

	// static function
	static	void		NumberToWord (long aValue, CString& result);
	static  CString		TextOverflow (int textLen);

    // operators
	int		operator==	(const Formatter& Fmt) const	{ return !Compare(Fmt); }
	int		operator!=	(const Formatter& Fmt) const	{ return Compare(Fmt); }

public:
	// pure virtual function
	virtual void 		Format	(const void*, CString&, BOOL bPaddingEnabled/* = TRUE*/, BOOL bCollateCultureSensitive/* = FALSE*/) const = 0;
	virtual void 		Format	(const void* ptr, CString& str, BOOL bPaddingEnabled = TRUE) const { Format(ptr, str, bPaddingEnabled, FALSE); } 

	virtual CString	GetDefaultInputString(DataObj* pDataObj = NULL) = 0;

	// virtual function
	virtual CString		UnFormat			(const CString& s)	const { return s; }
	virtual BOOL		IsSameAsDefault		()					const { return TRUE; }
	virtual	int			Compare				(const Formatter& Fmt)	const;
	virtual	void		Assign				(const Formatter& Fmt);
	virtual void		RecalcWidths		();
	virtual AlignType	GetDefaultAlign		() const;
	virtual void		SetToLocale			() {};
	virtual const CSize	GetInputWidth		(CDC* pDC, int nCols = -1, CFont* = NULL);

	virtual BOOL		CanAttachToData	() const { return FALSE; }
	virtual BOOL		ZeroPaddedHasUI	() const { return CanAttachToData(); }
	virtual void		SerializeJson(CJsonSerializer& strJson) const;
};

// rappresenta un gruppo di formattatori con lo stesso nome provenienti da 
// fonti differenti (vd. Override dello stesso formattatore da più moduli)
//============================================================================
class TB_EXPORT FormatterGroup : public CObject
{
	friend class CFormatDlg;
	friend class FormatStyleTable;
	
	DECLARE_DYNCREATE(FormatterGroup)

protected:
	CString		m_strName;
	DataType	m_OwnType;
	Array		m_Formatters;			
	
public:
	FormatterGroup	();
	FormatterGroup	(const CString& name);
	~FormatterGroup	();

public:
	Formatter*		GetFormatter	(const CTBNamespace* pContext) const;
	// restituisce il puntatore a format specificato (quello giusto non il BestFormatter)
	Formatter*		GetFormatter	(const FormatIdx idxFormatter); 
	// restituisce idx del array del gruppo del formatter
	FormatIdx		GetFormatIdx	(Formatter* FormatterFrom, const Formatter::FormatStyleSource aFormatSource);
	const Array&	GetFormatters	() const;

	const CString&	GetName			() const { return m_strName;	}
	const CString	GetTitle		();
	DataType		GetDataType		() const { return m_OwnType;	}

	void			SetName			(const CString& aStr) { m_strName = aStr;	}
	void			SetOwnType		(const DataType& aType){ m_OwnType = aType;	}

	void			DeleteFormatter	(Formatter* FormatterToDel);

	FormatterGroup*	Clone	() const;

public:
	virtual	void	Assign	(const FormatterGroup& Fmt);

private:
	int			AddFormatter			(Formatter* pFormatter);
	Formatter*	GetFormatter			(const Formatter::FormatStyleSource aSource);
	Formatter*	BestFormatterForContext (const CTBNamespace* pContext) const;
	BOOL		HasPriority				(const Formatter* pOld, const Formatter* pNew, const CTBNamespace* pContext) const;
};

// mantiene le informazioni base del file caricato
//=============================================================================
class TB_EXPORT FormatterFile : public CObject
{ 
private:
	CTBNamespace					m_Owner;
	Formatter::FormatStyleSource	m_Source;
	SYSTEMTIME						m_dLastWrite;

public:
	FormatterFile	(
						const CTBNamespace& aOwner,
						const Formatter::FormatStyleSource& aSource,
						const SYSTEMTIME& aFileDate
					);
	FormatterFile	(const FormatterFile&);

public:
	const CTBNamespace&					GetOwner	() { return m_Owner; }
	const Formatter::FormatStyleSource&	GetSource	() { return m_Source; }
	const SYSTEMTIME&					GetFileDate	() { return m_dLastWrite; }

	// metodi di scrittura
	void SetFileDate (const SYSTEMTIME& aDate);

public:
	// operators
   	const FormatterFile& operator = (const FormatterFile&);
};


//============================================================================
class TB_EXPORT FormatStyleTable : public Array, public CTBLockable
{
protected:
	BOOL	m_bModified;			// indica che qualcosa e` cambiato nella tabella
	Array	m_arLoadedFiles;

public:
	FormatStyleTable ();
	FormatStyleTable (const FormatStyleTable&);

public:
	// read methods
	CString		GetStyleName	(FormatIdx Index) const;
	CString		GetStyleTitle	(FormatIdx nIndex) const;

	Formatter*	GetFormatter	(const CString& stylename,	const CTBNamespace* pContext) const;
	Formatter*	GetFormatter	(const int& nIdx,			const CTBNamespace* pContext) const;
	Formatter*	GetFormatter	(const DataType& type,		const CTBNamespace* pContext) const;
	Formatter*	GetFormatter	(Formatter* FormatterFrom,	const Formatter::FormatStyleSource aFormatSource);													// restituisce il puntatore a format specificato (quello giusto non il BestFormatter)
	
	FormatIdx	GetFormatIdx	(const DataType& type)		const;
	FormatIdx	GetFormatIdx	(const CString& stylename)	const;
   	FormatIdx   GetFormatIdx	(Formatter* FormatterFrom, const Formatter::FormatStyleSource aFormatSource = Formatter::FROM_CUSTOM); // restituisce idx del array del gruppo del formatter
	
	DataType	GetDataType		(FormatIdx Index) const;

	int			GetPaddedLen	(const FormatIdx Index,	const CTBNamespace* pContext) const;
	
	int			GetOutputCharLen(const FormatIdx Index,	const CTBNamespace* pContext) const;
	int			GetOutputCharLen(const DataType& type,	const CTBNamespace* pContext) const;

	int			GetInputCharLen	(const FormatIdx Index,	const CTBNamespace* pContext) const;
	int			GetInputCharLen	(const DataType& type,	const CTBNamespace* pContext) const;

	SYSTEMTIME	GetFileDate		(const CTBNamespace& aOwner, const Formatter::FormatStyleSource& aSource);

	// set methods
	BOOL		IsModified		()			const	{ return m_bModified; }
	void		SetModified		(BOOL bMod)			{ m_bModified = bMod; }

	int			AddFileLoaded	(const CTBNamespace& aOwner, const Formatter::FormatStyleSource& aSource, const SYSTEMTIME& aDate);
	void		RemoveFileLoaded(const CTBNamespace& aOwner, const Formatter::FormatStyleSource& aSource);
	void		CopyFileLoaded	(const FormatStyleTable& fromFiles);
	int			AddFormatter	(Formatter* pFormatter);
	void		DeleteFormatter	(Formatter* FormatterToDel);

	BOOL		CheckFormatTable(CTBNamespaceArray& aNsToSave, const CTBNamespace aNsReport = CTBNamespace());
	void		ClearFormatsOf	(const CTBNamespace& aOwner, const Formatter::FormatStyleSource& aSource);

	FormatterGroup*	GetFormatterGroup	(FormatIdx Index) { return GetAt(Index);}
	FormatterGroup*	GetAt				(FormatIdx Index) const;

	// operators
   	const FormatStyleTable& operator = (const FormatStyleTable&);

	//for lock tracer
	virtual LPCSTR			GetObjectName() const { return "FormatStyleTable"; }

	void	GetCompatibleFormatterNames (const DataType& dataType, CStringArray& arNames) const;
 
private:
	FormatterGroup*	operator[]	(FormatIdx Index)	const;
};


DECLARE_SMART_LOCK_PTR(FormatStyleTable)
DECLARE_CONST_SMART_LOCK_PTR(FormatStyleTable)
// General Functions
//-----------------------------------------------------------------------------
TB_EXPORT const FormatStyleTable* AFXAPI AfxGetStandardFormatStyleTable();
TB_EXPORT FormatStyleTableConstPtr AFXAPI AfxGetFormatStyleTable();
TB_EXPORT FormatStyleTablePtr AFXAPI AfxGetWritableFormatStyleTable();

//-----------------------------------------------------------------------------
TB_EXPORT CString FromDataTypeToFormatName	(const DataType& aType);
TB_EXPORT CString	 AfxGetMoneyFormatterName	(BOOL bIsAccountable = TRUE);
TB_EXPORT Formatter* AfxGetMoneyFormatter		(BOOL bIsAccountable = TRUE, CTBNamespace* pContext = NULL);
//=============================================================================        
#include "endh.dex"
