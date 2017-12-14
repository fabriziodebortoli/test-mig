#pragma once

#include <TbNameSolver\TBNamespaces.h>
#include <TbGeneric\Array.h>

//includere alla fine degli include del .H
#include "beginh.dex"

//=============================================================================        
class EnumTag;
class EnumItem;
class EnumTagArray;
class EnumItemArray;
class EnumsTable;

//=============================================================================        
#define TAG_ERROR			((WORD)0xFFFF)
#define ITEM_ERROR			((WORD)0xFFFF)
#define	MAX_ENUM_TAG_VALUE	((WORD)0xFFF0)

#define TAG_REPORT_STATUS					((WORD)0xFFF0)
#define TAG_REPORT_STATUS_SUCCESS			((WORD)0x0000)
#define TAG_REPORT_STATUS_NO_DATA_FOUND		((WORD)0x0001)
#define TAG_REPORT_STATUS_ABNORMAL_END		((WORD)0x0002)
#define TAG_REPORT_STATUS_USER_ABORT		((WORD)0x0003)
#define TAG_REPORT_STATUS_PRINTING_PREVIEW	((WORD)0x0004)
#define TAG_REPORT_STATUS_SCRIPT_ABORT		((WORD)0x0005)
#define TAG_REPORT_STATUS_SCRIPT_QUIT		((WORD)0x0006)
// Helper function for test all bits in a int. Only One can be On.
//-----------------------------------------------------------------------------
TB_EXPORT WORD WhichBitOn (WORD wItemValue);

// item singolo dell'enumerativo
//=============================================================================        
class TB_EXPORT EnumItem : public CObject
{
protected:
	EnumTag*		m_pEnumTag;
	CString			m_strItemName;
	WORD			m_wItemValue;
	CTBNamespace	m_OwnerModule;
	BOOL			m_bHidden;

public:
	EnumItem ();
	EnumItem (const CString& strItemName, WORD wItemValue, EnumTag *pEnumTag, const CTBNamespace& OwnerModule);
	
public:
	// operators
	const	EnumItem& operator	=	(const EnumItem&);
	BOOL	operator			==	(const EnumItem&) const;
	BOOL	operator			!=	(const EnumItem& aEI) const { return !(*this == aEI); }

public:
	const CString&		GetItemName		()	const { return m_strItemName; }
	const CTBNamespace&	GetOwnerModule	()	const { return m_OwnerModule; }
	CString				GetTitle		()	const;
	WORD				GetItemValue	()	const { return m_wItemValue; }
	void				SetItem			(const CString& strItemName, WORD wItemValue, const CTBNamespace& OwnerModule);
	const BOOL&			IsHidden		();
	void				SetHidden		(const BOOL& bHidden);

};        
                      
// array di elementi dell'enumerativo
//===========================================================================
class TB_EXPORT EnumItemArray : public Array
{
public:
	// operators
	const	EnumItemArray& operator	=	(const EnumItemArray&);
	BOOL	operator				==	(const EnumItemArray&) const;
	BOOL	operator				!=	(const EnumItemArray& aEIA) const { return !(*this == aEIA); }

public:
	// overloaded operator helpers
	EnumItem*	GetAt			(int nIndex)		const	{ return (EnumItem*) Array::GetAt(nIndex); }
	EnumItem*	operator[]		(int nIndex)		const	{ return GetAt(nIndex); }
	EnumItem*&	operator[]		(int nIndex)				{ return (EnumItem*&) ElementAt(nIndex); }
	EnumItem*	GetItemByName	(const CString& strItemName)	const;
	EnumItem*	GetItemByValue	(WORD wItemValue)				const;

public:
	CString	GetEnumItemName		(WORD wItemValue)			const;
	CString	GetTitle			(WORD wItemValue)			const;
	WORD	GetEnumItemValue	(const CString& strItemName)const;

	BOOL	ExistItemInModule	(const CTBNamespace& nsModule) const;
};                                        

// definizione dell'enumerativo contenente tag ed array di elementi 
//=============================================================================        
class TB_EXPORT EnumTag : public CObject
{
protected:
	CString			m_strTagName;
	WORD			m_wTagValue;
	WORD			m_wDefaultItemValue;
	EnumItemArray*	m_pEnumItems;
	BOOL			m_bHidden;

public:
	EnumTag	();
	EnumTag	(const	CString& strTagName, WORD wTagValue, const CString& sTitle, const CTBNamespace& OwnerModule,
			 WORD wDefaultItemValue);
	virtual ~EnumTag ();

public:
	// operators
	const	EnumTag& operator	=	(const EnumTag&);
	BOOL	operator			==	(const EnumTag&) const;
	BOOL	operator			!=	(const EnumTag& aET) const { return !(*this == aET); }

public:
	// useful to managing items
	EnumItem*	AddItem 		(const CString& strItemName, WORD wItemValue, const CTBNamespace& OwnerModule);
	void		DeleteItem		(const CString& strItemName);
	BOOL		ExistItemName	(const CString& strItemName);
	BOOL		ExistItemValue	(WORD wItemValue);
	BOOL		ExistItem		(const CString& strItemName, WORD wItemValue);

public:
//	const CTBNamespace&	GetOwnerModule	()	const					{ return m_OwnerModule;	}
//	void				SetOwnerModule	(const CTBNamespace& aNs)	{ m_OwnerModule = aNs;	}

	void			SetTag				(const CString& strTagName, WORD wTagValue);
	const CString&	GetTagName			()	const	{ return m_strTagName;	}
	CString			GetTagTitle			()	const;
	//void			SetTagTitle			(const CString& sTagTitle) { m_sTagTitle = sTagTitle; }
	WORD			GetTagValue			()	const	{ return m_wTagValue;	}

	WORD			GetDefaultItemValue	()	const	{ return m_wDefaultItemValue; }
	void			SetDefaultItemValue	(WORD wDefaultItemValue)	{ m_wDefaultItemValue = wDefaultItemValue; }
	EnumItemArray*	GetEnumItems		()	const	{ return m_pEnumItems;	}
	WORD			GetLongerItemValue	()	const;
	int				GetLongerItemIdx	()	const;

	const BOOL&		IsHidden			();
	void			SetHidden			(const BOOL& bHidden);
};
        
// array degli enumerativi definiti
//===========================================================================
class TB_EXPORT EnumTagArray : public Array
{
public:
	EnumTagArray();
	
public:
	// operators
	const	EnumTagArray& operator	=	(const EnumTagArray&);
	BOOL	operator				==	(const EnumTagArray&) const;
	BOOL	operator				!=	(const EnumTagArray& aETA) const { return !(*this == aETA); }

public:
	// overloaded operator helpers
	EnumTag*	GetAt			(int nIndex)	const	{ return (EnumTag*) Array::GetAt(nIndex); }
	EnumTag*	operator[]		(int nIndex)	const	{ return GetAt(nIndex); }
	EnumTag*&	operator[]		(int nIndex)			{ return (EnumTag*&) ElementAt(nIndex); }
	EnumTag*	GetTagByValue	(WORD wTagValue) const;
	EnumTag*	GetTagByName	(const CString& strTagName)	const;

public:
	// useful to managing enum type
	EnumTag*	AddTag			(const CString& strTagName, WORD wTagValue, const CString& sTitle, const CTBNamespace& OwnerModule, WORD wDefaultItemValue = 0);
	void		DeleteTag		(const CString& strTagName);
	BOOL		ExistTagName	(const CString& strTagName)	const { return GetTagByName	(strTagName) != NULL; }
	BOOL		ExistTag		(const CString& strTagName, WORD wTagValue);
	BOOL		ExistTagValue	(WORD wTagValue) const { return GetTagByValue(wTagValue) != NULL; }
	BOOL		ExistTagInModule (const CTBNamespace& nsModule) const;
	BOOL		ExistEnumValue	(WORD wTagValue, WORD wItemValue) const;

	EnumItemArray*	GetEnumItems	(const CString&	strTagName)	const;
	EnumItemArray*	GetEnumItems	(WORD wTagValue)			const;
	
	WORD	GetEnumTagValue			(const CString&	strTagName)	const;
	CString	GetEnumTagName			(WORD wTagValue)			const;
	CString	GetEnumTagTitle			(WORD wTagValue)			const;
	EnumTag* GetEnumTag				(WORD wTagValue)			const;

	WORD	GetEnumItemValue		(const CString&	strTagName, const CString& strItemName)	const;
	WORD	GetEnumDefaultItemValue	(WORD wTagValue)										const;
	WORD	GetEnumLongerItemValue	(WORD wTagValue)										const;

	CString GetEnumItemTitle		(WORD wTagValue, WORD wItemValue) const;
};                                        
                     
// tabella di gestione di enumerativi
//===========================================================================
class TB_EXPORT EnumsTable : public CObject , public CTBLockable
{
	friend class CXmlEnumsContent;
	friend class CEBEnumsManager;

protected:
	EnumTagArray*	m_pEnumTags;

public:
	EnumsTable();
	EnumsTable(const EnumsTable&);
	virtual ~EnumsTable();

public:
	void	Init	();

	// operators
	void	AssignEnums	(const EnumsTable&);
	BOOL	IsEqualEnums(const EnumsTable&) const;

	
	
public:
	const EnumTagArray*	GetEnumTags	()							const { return m_pEnumTags; }
	const EnumItemArray*	GetEnumItems	(const CString& strTagName)	const;
	const EnumItemArray*	GetEnumItems	(WORD wTagValue)			const;

	// ENUM specific management function
	EnumTag*	AddEnumTag 			(const CString& strTagName, WORD wTagValue, const CString& sTitle, const CTBNamespace& OwnerModule, WORD wDefaultItemValue = 0);
	BOOL		AddEnumValue 		(EnumTag* pItems, const CString& strItemName, WORD wItemValue, const CTBNamespace& OwnerModule);

	BOOL		ExistEnumTagName	(const CString& strTagName)	const;
	BOOL		ExistEnumTagValue	(WORD wTagValue)			const;
	BOOL		ExistEnumTag		(const CString& strTagName, WORD wTagValue)	const;
	BOOL		ExistEnumValue		(DWORD dwValue) const;

	EnumTag*	GetEnumTag			(WORD wTagValue) const;
	WORD		GetEnumTagValue		(const CString& strTagName)	const;
	CString		GetEnumTagName		(WORD wTagValue)			const;
	CString		GetEnumTagTitle		(WORD wTagValue)			const;

	WORD		GetEnumItemValue		(const CString& strTagName, const CString& strItemName)	const;
	WORD		GetEnumDefaultItemValue	(WORD wTagValue) const;
	WORD		GetEnumLongerItemValue	(WORD wTagValue) const;

	CString		GetEnumItemTitle		(DWORD dwValue) const;

	//for lock tracer
	virtual LPCSTR			GetObjectName() const { return "EnumsTable"; }

	CString ToJson() const;

private:
	void	AddEnumTag	(
									const	CString& strTagName, 
									WORD				wTagValue, 
									const CString&		sTitle, 
									const CTBNamespace& OwnerModule,
									WORD				wDefaultItemValue
								 )	const 
	 { m_pEnumTags->AddTag(strTagName, wTagValue, sTitle, OwnerModule); }
};

DECLARE_SMART_LOCK_PTR(EnumsTable)
DECLARE_CONST_SMART_LOCK_PTR(EnumsTable)

//-----------------------------------------------------------------------------
// General Functions
TB_EXPORT const EnumsTable*		AFXAPI AfxGetStandardEnumsTable();
TB_EXPORT EnumsTableConstPtr	AFXAPI AfxGetEnumsTable();
TB_EXPORT EnumsTablePtr			AFXAPI AfxGetWritableEnumsTable();

TB_EXPORT int CompareEnumsByTitle (CObject* pObj1, CObject* pObj2);

//-----------------------------------------------------------------------------
#include "endh.dex"
