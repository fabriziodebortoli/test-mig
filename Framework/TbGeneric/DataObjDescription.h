#pragma once

#include <TbXmlCore\XmlDocObj.h>
#include <TbNameSolver\TbNamespaces.h>
#include <TbNameSolver\MacroToRedifine.h>

#include <TbGeneric\Array.h>
#include <TbGeneric\DataObj.h>
#include "Beginh.dex"

class CDiagnostic;

//----------------------------------------------------------------
class TB_EXPORT CApplyAreaDescription : public CObject
{
	DECLARE_DYNCREATE(CApplyAreaDescription)

private:
	CTBNamespace	m_nsOwner;
	BOOL			m_bForAllUsers;
	CStringArray	m_arUsers;

public:
	CApplyAreaDescription();

public:
	const CTBNamespace& GetOwner		() const { return m_nsOwner; }
	const CStringArray&	GetUsers		() const { return m_arUsers; }
	const BOOL&			IsForAllUsers	() const { return m_bForAllUsers; }
	const BOOL			IsForUser		(const CString& sUser) const;

	void 	SetOwner		(const CTBNamespace&	nsOwner);
	void 	SetForAllUsers	(const BOOL&			bForAllUsers);
	void	AddUser			(const CString&			sUser);

	void	Assign			(const CApplyAreaDescription* pDescri);
	BOOL	IsEqual			(const CApplyAreaDescription& aDescri);

	CApplyAreaDescription&	operator=	(const CApplyAreaDescription& ad);
	BOOL					operator==	(const CApplyAreaDescription& ad);
};

// Descrizione minimale di un oggetto applicativo
//----------------------------------------------------------------
class TB_EXPORT CBaseDescription : public CObject
{
	DECLARE_DYNCREATE(CBaseDescription)

enum XMLFrom 
		{
			XML_NULL,
			XML_STANDARD, 
			XML_ALLUSERS, 
			XML_USER,
			XML_MODIFIED,
			XML_DELETED,
			XML_ADD
		};

public:
	XMLFrom			m_XMLFrom;

protected:
	CTBNamespace	m_Namespace;
	CString			m_sName;
	CString			m_sNotLocalizedTitle;
	
	// membri di utilità
	CTBNamespace::NSObjectType	m_NSType;
	CApplyAreaDescription		m_ApplyArea;
	
	
public:
	CBaseDescription ();
	CBaseDescription (CTBNamespace::NSObjectType aNSType);
	CBaseDescription (const CTBNamespace& aNs, const CString& sTitle);
	CBaseDescription (const CTBNamespace& aNs);
	virtual ~CBaseDescription () {}

public:
	virtual const CString GetTitle				() const;
	const CTBNamespace&	GetNamespace			() const { return m_Namespace; }
	const CString		GetName					() const { return m_sName; }
	const CString&		GetNotLocalizedTitle	() const { return m_sNotLocalizedTitle; }
	

	const CTBNamespace::NSObjectType	GetType				() const { return m_NSType; }

	const CTBNamespace&		GetOwner		 () const { return m_ApplyArea.GetOwner(); }

	// metodi di settaggio
	void SetName				(const CString&	aName);
	void SetNamespace			(const CTBNamespace& aNamespace);
	void SetNamespace			(const CString& sPartial, const CTBNamespace& aParent);
	void SetNsType				(CTBNamespace::NSObjectType aNSType);
	void SetNotLocalizedTitle	(const CString& sTitle);
	void SetOwner				(const CTBNamespace&	nsOwner);

	TB_OLD_METHOD 	void SetTitle	(const CString& sTitle)	{ SetNotLocalizedTitle(sTitle); }

public:
	// operatori
	CBaseDescription&	operator=	(const CBaseDescription& bd);
	BOOL				operator!=	(const CBaseDescription& bd);
	BOOL				operator==	(const CBaseDescription& bd);

	void				Assign	(const CBaseDescription& bd);
	BOOL				IsEqual	(const CBaseDescription& bd);
	
	virtual CBaseDescription*	Clone	();
// diagnostics
#ifdef _DEBUG
public:	
	virtual void Dump(CDumpContext&) const;
	virtual void AssertValid() const;
#endif
};

// array di descrizioni base
//----------------------------------------------------------------
class TB_EXPORT CBaseDescriptionArray : public NamedDataObjArray
{
	DECLARE_DYNAMIC (CBaseDescriptionArray)

public:
	int	Add	(CBaseDescription* pInfo) { return __super::Add (pInfo); }

	CBaseDescription* GetAt		(int nIdx) const { return (CBaseDescription*)__super::GetAt (nIdx); }
	CBaseDescription* GetInfo	(const CTBNamespace& aNS) const;
	CBaseDescription* GetInfo	(const CString& sName) const;

public:
	// operatori
	CBaseDescriptionArray&	operator=	(const CBaseDescriptionArray& ar);
	BOOL					operator!=	(const CBaseDescriptionArray& ar);
	BOOL					operator==	(const CBaseDescriptionArray& ar);

	void Assign	(const CBaseDescriptionArray& ar);
	BOOL IsEqual(const CBaseDescriptionArray& ar);

					virtual DataObj* GetDataObjFromColumnName (const CString&);	
/*TBWebMethod*/		virtual DataObj* GetDataObjFromName	(DataStr);	
};

// rappresentazione dei DataObjs. Manca il DataBlob che in
// Xml non lo vedo rappresentabile se non con una href ad un file
// Il DataDate lo gestisco sempre come un DataTime
//----------------------------------------------------------------
class TB_EXPORT CDataObjDescription : public CBaseDescription
{
	DECLARE_DYNCREATE(CDataObjDescription)

public:
	enum PassMode { _IN, _OUT, _INOUT };

protected:
	DataType		m_DataType;
	DataObj*		m_pValue;
	PassMode		m_nPassedMode;
	BOOL			m_bDefaultByOut;
	BOOL			m_bOwnerValue;
	BOOL			m_bArray;
	BOOL			m_bOptional;
	BOOL			m_bCollateCultureSensitiveValue;
	int				m_nLength;
	BOOL			m_bUseVoidAsDefaultType;
	CString			m_sClassType;

public:
	CDataObjDescription		();
	CDataObjDescription		(const CDataObjDescription& dd);
	CDataObjDescription		(CString strName, CString strValue, CString sParamType, BOOL bLocalizedValue = FALSE);
	CDataObjDescription		(const CString& strName, const CStringArray& arValue, const CString& sParamType);
	CDataObjDescription		(const CString& strName, DataObj* pValue, BOOL bOwner);
	CDataObjDescription		(const CString& strName, DataType, PassMode);
	~CDataObjDescription	();

public:
	const DataType&		GetDataType		() const { return m_bArray ? DataType::Array : m_DataType; }
	const int&			GetLength		() const { return m_nLength; }
	const DataType&		GetBaseDataType	() const { return m_DataType; }
	const BOOL			IsArray			() const { return m_bArray; }
	void				SetDataType		(const DataType& aType);
	void				SetArrayType	(const DataType& aType);

	const PassMode&		GetPassedMode	() const { return m_nPassedMode; }
	const WORD&			GetEnumTag		() const { return m_DataType.m_wTag; }
	const BOOL			IsOptional		() const { return m_bOptional; }
	const BOOL			IsCollateCultureSensitiveValueValue() const { return m_bCollateCultureSensitiveValue; }
	const BOOL&			UseVoidAsDefaultType				() const { return m_bUseVoidAsDefaultType; }

	DataObj*	GetValue				();
	void		SetValue				(const DataObj& aValue);
	void		SetValue				(const CString& strValue);
	void		SetDataObj				(DataObj* pObj); // sostituisce il puntatore al dataobj
	void		SetDataObj				(DataObj* pObj, BOOL bOwnerValue); // sostituisce il puntatore al dataobj

	const BOOL	IsPassedModeIn		() const { return m_nPassedMode == _IN; }
	const BOOL	IsPassedModeOut		() const { return m_nPassedMode == _OUT ; }
	const BOOL	IsPassedModeInOut	() const { return m_nPassedMode == _INOUT; }

	void SetDataTypeTag					(const WORD& wTag);
	void SetPassedMode					(PassMode pm) { m_nPassedMode = pm; }
	void SetDefaultPassedByOut			(const BOOL& bValue);
	void SetCollateCultureSensitiveValue(BOOL bSensitive);
	void SetLength						(const int& nLength);
	void SetVoidAsDefaultType			(const BOOL& bValue);

	CString GetClassType() const	{ return m_sClassType; }

public:
	// funzioni statiche
	static DataType ToDataType	(const CString& sType);
	static CString	ToString	(const DataType& aType);

	BOOL Parse		(CXMLNode*, BOOL bWithValues = TRUE);
	void Unparse	(CXMLNode*, BOOL bWithValues = TRUE);
	void Unparse(DataStr& strXml);

public:
	// operatori
	CDataObjDescription&	operator=	(const CDataObjDescription& dd)  { return Assign(dd); }
	BOOL					operator!=	(const CDataObjDescription& dd);
	BOOL					operator==	(const CDataObjDescription& dd);
	
	CDataObjDescription&	Assign	(const CDataObjDescription& dd);
	BOOL					IsEqual	(const CDataObjDescription& dd);
	
	virtual CBaseDescription*	Clone	();

private:
	void	 CreateDataObj			(const CString& sValue, const DataType& aType);
	void	 CreateDataObjArray		(const CStringArray& aValues, const DataType aType = DataType::String);
	DataObj* InternalCreateDataObj	(const CString& sValue, const DataType& aType);
};

#include "Endh.dex"
