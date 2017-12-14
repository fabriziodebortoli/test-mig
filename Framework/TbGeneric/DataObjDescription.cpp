#include "stdafx.h"

#include <TbXmlCore\XmlGeneric.h>
#include <TbXmlCore\XmlTags.h>

#include <TbNameSolver\FileSystemFunctions.h>
#include <TbNameSolver\TBNamespaces.h>
#include <TbNameSolver\PathFinder.h>
#include <TbNameSolver\Chars.h>
#include <TbNameSolver\Diagnostic.h>

#include <TbGeneric\EnumsTable.h>
#include <TbGeneric\GeneralFunctions.h>
#include <TbGeneric\TBStrings.h>

#include "DataObjDescription.h"

//--------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CApplyAreaDescription, CObject)

//--------------------------------------------------------------------
CApplyAreaDescription::CApplyAreaDescription()
	:
	m_bForAllUsers	(TRUE)
{
}

//----------------------------------------------------------------------------------------------
void CApplyAreaDescription::SetOwner (const CTBNamespace& nsOwner)
{
	m_nsOwner = nsOwner;
}

//----------------------------------------------------------------------------------------------
void CApplyAreaDescription::SetForAllUsers (const BOOL& bForAllUsers)
{
	m_bForAllUsers = bForAllUsers;
	if (m_bForAllUsers)
		m_arUsers.RemoveAll();
}

//----------------------------------------------------------------------------------------------
const BOOL CApplyAreaDescription::IsForUser (const CString& sUser) const
{
	if (m_bForAllUsers)
		return TRUE;
	
	for (int i=0; i <= m_arUsers.GetUpperBound(); i++)
		if (_tcsicmp(m_arUsers.GetAt(i), sUser) == 0)
			return TRUE;
	
	return FALSE;
}

//----------------------------------------------------------------------------------------------
void CApplyAreaDescription::AddUser (const CString& sUser)
{
	for (int i=0; i <= m_arUsers.GetUpperBound(); i++)
		if (_tcsicmp(m_arUsers.GetAt(i), sUser) == 0)
			return;
	m_arUsers.Add(sUser);
}


//----------------------------------------------------------------------------------------------
void CApplyAreaDescription::Assign (const CApplyAreaDescription* pDescri)
{
	m_nsOwner		= pDescri->m_nsOwner;
	m_bForAllUsers	= pDescri->m_bForAllUsers;
	m_arUsers.RemoveAll();

	for (int i=0; i <= pDescri->m_arUsers.GetUpperBound(); i++)
		AddUser(pDescri->m_arUsers.GetAt(i));
}

//--------------------------------------------------------------------
BOOL CApplyAreaDescription::IsEqual	(const CApplyAreaDescription& aDescri)
{
	BOOL bOk = 
			m_nsOwner			== aDescri.m_nsOwner && 
			m_bForAllUsers		== aDescri.m_bForAllUsers && 
			m_arUsers.GetSize()	== aDescri.m_arUsers.GetSize();
	if (!bOk)
		return FALSE;

	for (int i=0; i <= m_arUsers.GetUpperBound(); i++)
		if (_tcsicmp(m_arUsers.GetAt(i), aDescri.m_arUsers.GetAt(i)) != 0)
			return FALSE;

	return TRUE;
}

//--------------------------------------------------------------------
CApplyAreaDescription& CApplyAreaDescription::operator= (const CApplyAreaDescription& ad) 
{ 
	Assign(&ad); 
	return *this; 
}

//--------------------------------------------------------------------
BOOL CApplyAreaDescription::operator== (const CApplyAreaDescription& ad) 
{ 
	return IsEqual (ad);
}

//----------------------------------------------------------------------------------------------
//	class CBaseDescription implementation
//----------------------------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CBaseDescription, CObject)

//----------------------------------------------------------------------------------------------
CBaseDescription::CBaseDescription ()
{
	m_XMLFrom	= CBaseDescription::XML_NULL;
	m_NSType = CTBNamespace::NOT_VALID;
}

//----------------------------------------------------------------------------------------------
CBaseDescription::CBaseDescription (CTBNamespace::NSObjectType aNSType)
{
	m_XMLFrom	= CBaseDescription::XML_NULL;
	m_NSType = aNSType;
}

//----------------------------------------------------------------------------------------------
CBaseDescription::CBaseDescription (const CTBNamespace& aNs, const CString& sTitle)
{
	m_XMLFrom	= CBaseDescription::XML_NULL;
	m_Namespace	= aNs;
	m_sNotLocalizedTitle = sTitle;
	m_sName		= m_Namespace.GetObjectName();
	m_NSType	= aNs.GetType();
}

//----------------------------------------------------------------------------------------------
CBaseDescription::CBaseDescription (const CTBNamespace& aNs)
{
	m_Namespace	= aNs;
	m_sName		= m_Namespace.GetObjectName();
	m_NSType	= aNs.GetType();
	m_XMLFrom	= CBaseDescription::XML_NULL;
}

//----------------------------------------------------------------------------------------------
void CBaseDescription::SetNamespace	(const CTBNamespace& aNamespace)
{ 
	m_Namespace = aNamespace;
	m_sName = m_Namespace.GetObjectName();
}

//----------------------------------------------------------------------------------------------
void CBaseDescription::SetNamespace (const CString& sPartial, const CTBNamespace& aParent)
{
	if (!m_Namespace.AutoCompleteNamespace(m_NSType, sPartial, aParent)) 
	{
		m_Namespace.SetNamespace(sPartial);
		m_sName = sPartial;
	}
	else
		m_sName = m_Namespace.GetObjectName();
}

//----------------------------------------------------------------------------------------------
void CBaseDescription::SetName (const CString& aName)
{
	m_sName = aName;
}


//----------------------------------------------------------------------------------------------
void CBaseDescription::SetNotLocalizedTitle	(const CString& sTitle)
{
	m_sNotLocalizedTitle = sTitle;
}

//----------------------------------------------------------------------------------------------
const CString CBaseDescription::GetTitle () const
{
	return m_sNotLocalizedTitle;
}

//----------------------------------------------------------------------------------------------
void CBaseDescription::SetNsType(CTBNamespace::NSObjectType aNSType)
{
	m_NSType = aNSType;
}

//----------------------------------------------------------------------------------------------
void CBaseDescription::SetOwner (const CTBNamespace& nsOwner)
{
	m_ApplyArea.SetOwner(nsOwner);
}


//----------------------------------------------------------------------------------------------
void CBaseDescription::Assign (const CBaseDescription& bd)
{
	m_Namespace			= bd.m_Namespace;
	m_sName				= bd.m_sName;
	m_sNotLocalizedTitle= bd.m_sNotLocalizedTitle;
	m_NSType			= bd.m_NSType;
	m_XMLFrom			= bd.m_XMLFrom;
	m_ApplyArea			= bd.m_ApplyArea;
}

//----------------------------------------------------------------------------------------------
CBaseDescription* CBaseDescription::Clone ()
{
	CBaseDescription* pNewBaseDescritpion = new CBaseDescription();

	pNewBaseDescritpion->Assign(*this);

	return pNewBaseDescritpion;
}

//----------------------------------------------------------------------------------------------
BOOL CBaseDescription::IsEqual (const CBaseDescription& bd)
{
	return  m_Namespace			== bd.m_Namespace &&
			m_sName				== bd.m_sName &&
			m_NSType			== bd.m_NSType && 
			m_sNotLocalizedTitle== bd.m_sNotLocalizedTitle && 
			m_ApplyArea			== bd.m_ApplyArea;
}

//--------------------------------------------------------------------
CBaseDescription& CBaseDescription::operator= (const CBaseDescription& bd) 
{ 
	Assign(bd); 
	return *this; 
}

//--------------------------------------------------------------------
BOOL CBaseDescription::operator== (const CBaseDescription& bd) 
{ 
	return IsEqual (bd);
}

//--------------------------------------------------------------------
BOOL CBaseDescription::operator!= (const CBaseDescription& bd) 
{ 
	return !IsEqual (bd);
}

/////////////////////////////////////////////////////////////////////////////
// Diagnostics

#ifdef _DEBUG
void CBaseDescription::Dump(CDumpContext& dc) const
{
	ASSERT_VALID(this);
	dc << _T("\n\tCBaseDescription = ") << this->GetRuntimeClass()->m_lpszClassName << _T(", ") << this->m_Namespace.ToString();
}

void CBaseDescription::AssertValid() const
{
	__super::AssertValid();
}
#endif //_DEBUG


//----------------------------------------------------------------------------------------------
//	class CBaseDescriptionArray implementation
//----------------------------------------------------------------------------------------------
IMPLEMENT_DYNAMIC (CBaseDescriptionArray, NamedDataObjArray)

//----------------------------------------------------------------------------------------------
CBaseDescription* CBaseDescriptionArray::GetInfo (const CTBNamespace& aNS) const
{
	CBaseDescription* pInfo;
	for (int i=0; i <= GetUpperBound(); i++)
	{
		pInfo = GetAt(i);
		if (aNS == pInfo->GetNamespace())
			return pInfo;
	}

	return NULL;
}

//----------------------------------------------------------------------------------------------
CBaseDescription* CBaseDescriptionArray::GetInfo (const CString& sName) const
{
	CBaseDescription* pInfo;
	for (int i=0; i <= GetUpperBound(); i++)
	{
		pInfo = GetAt(i);
		if (_tcsicmp(sName, pInfo->GetName()) == 0)
			return pInfo;
	}

	return NULL;
}

//----------------------------------------------------------------------------------------------
//[TBWebMethod(thiscall_method=true)]
DataObj* CBaseDescriptionArray::GetDataObjFromName (DataStr dsName)
{
	return GetDataObjFromColumnName (dsName.GetString());
}

DataObj* CBaseDescriptionArray::GetDataObjFromColumnName (const CString& sName)
{
	CBaseDescription* pInfo = GetInfo (sName);
	if (!pInfo)
		return NULL;
	if (!pInfo->IsKindOf(RUNTIME_CLASS(CDataObjDescription)))
		return NULL;

	CDataObjDescription * pDI = (CDataObjDescription*) pInfo;
	return pDI->GetValue();
}

//----------------------------------------------------------------------------------------------
void CBaseDescriptionArray::Assign (const CBaseDescriptionArray& ar)
{
	RemoveAll ();

	CBaseDescription* pNewDescri;
	CBaseDescription* pDescri;
	for (int i=0; i <= ar.GetUpperBound(); i++)
	{
		pDescri = ar.GetAt(i);
		VERIFY(pNewDescri = (CBaseDescription*) pDescri->GetRuntimeClass()->CreateObject());
		*pNewDescri = *pDescri;
		Add (pNewDescri);
	}
}

//----------------------------------------------------------------------------------------------
BOOL CBaseDescriptionArray::IsEqual (const CBaseDescriptionArray& ar)
{
	BOOL bEqual = GetSize () == ar.GetSize();

	if (bEqual)
		for (int i=0; i <= ar.GetUpperBound(); i++)
			if (GetAt(i) != ar.GetAt(i))
				return FALSE;

	return  bEqual;
}

//--------------------------------------------------------------------
CBaseDescriptionArray& CBaseDescriptionArray::operator= (const CBaseDescriptionArray& ar) 
{ 
	Assign(ar); 
	return *this; 
}

//--------------------------------------------------------------------
BOOL CBaseDescriptionArray::operator== (const CBaseDescriptionArray& ar) 
{ 
	return IsEqual (ar);
}

//--------------------------------------------------------------------
BOOL CBaseDescriptionArray::operator!= (const CBaseDescriptionArray& ar) 
{ 
	return !IsEqual (ar);
}

//----------------------------------------------------------------------------------------------
//	class CDataObjDescription implementation
//----------------------------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CDataObjDescription, CBaseDescription)

//----------------------------------------------------------------------------------------------
CDataObjDescription::CDataObjDescription ()
	:
	CBaseDescription				(),
	m_nPassedMode					(_IN),
	m_bDefaultByOut					(FALSE),
	m_pValue						(NULL),
	m_bOwnerValue					(TRUE),
	m_bArray						(FALSE),
	m_bOptional						(FALSE),
	m_bCollateCultureSensitiveValue	(FALSE),
	m_DataType						(DataType::String),
	m_nLength						(0),
	m_bUseVoidAsDefaultType			(FALSE)
{
	SetDataType(DataType::String);
}

//----------------------------------------------------------------------------------------------
CDataObjDescription::CDataObjDescription (const CString& strName, DataObj* pValue, BOOL bOwner)
	:
	CBaseDescription				(),
	m_nPassedMode					(_IN),
	m_bDefaultByOut					(FALSE),
	m_pValue						(pValue),
	m_bArray						(FALSE),
	m_bOwnerValue					(bOwner),
	m_bOptional						(FALSE),
	m_bCollateCultureSensitiveValue	(FALSE),
	m_nLength						(0),
	m_bUseVoidAsDefaultType			(FALSE)
{
	SetName(strName);
	if (pValue)
	{
		m_DataType =  pValue->GetDataType();
		m_bArray = m_DataType == DataType::Array;
		m_bCollateCultureSensitiveValue = pValue->IsCollateCultureSensitive();
	}
	else
		m_DataType = DataType::String;
}

//----------------------------------------------------------------------------------------------
CDataObjDescription::CDataObjDescription (const CString& strName, DataType dt, PassMode pm)
	:
	CBaseDescription				(),
	m_nPassedMode					(pm),
	m_bDefaultByOut					(FALSE),
	m_pValue						(NULL),
	m_bOwnerValue					(TRUE),
	m_bArray						(FALSE),
	m_bOptional						(FALSE),
	m_bCollateCultureSensitiveValue	(FALSE),
	m_DataType						(DataType::String),
	m_nLength						(0),
	m_bUseVoidAsDefaultType			(FALSE)
{
	SetName(strName);
	SetDataType(dt);
}

//----------------------------------------------------------------------------------------------
CDataObjDescription::CDataObjDescription (CString strName, CString strValue, CString sParamType, BOOL bCollateCultureSensitiveValue /*FALSE*/ )
	:
	CBaseDescription				(),
	m_nPassedMode					(_IN),
	m_bDefaultByOut					(FALSE),
	m_pValue						(NULL),
	m_bOwnerValue					(FALSE),
	m_bArray						(FALSE),
	m_bOptional						(FALSE),
	m_bCollateCultureSensitiveValue	(bCollateCultureSensitiveValue),
	m_DataType						(DataType::String),
	m_nLength						(0),
	m_bUseVoidAsDefaultType			(FALSE)
{
	SetName(strName);
	SetDataType(ToDataType(sParamType));
	CreateDataObj(strValue, m_DataType);
}

//----------------------------------------------------------------------------------------------
CDataObjDescription::CDataObjDescription (const CString& strName, const CStringArray& arValue, const CString& sParamType)
	:
	CBaseDescription		(),
	m_DataType				(DATA_STR_TYPE, 0),
	m_nPassedMode			(_IN),
	m_bDefaultByOut 		(FALSE),
	m_pValue				(NULL),
	m_bOwnerValue			(TRUE),
	m_bArray				(sParamType == XML_DATATYPE_ARRAY_VALUE),
	m_bOptional				(FALSE),
	m_nLength				(0),
	m_bUseVoidAsDefaultType	(FALSE)
{
	SetName(strName);
	SetDataType(ToDataType(sParamType));
	CreateDataObjArray(arValue);
}

//-----------------------------------------------------------------------------
CDataObjDescription::CDataObjDescription(const CDataObjDescription& dd) 
	:
	m_bDefaultByOut 		(FALSE),
	m_pValue				(NULL),
	m_bOwnerValue			(TRUE),
	m_bArray				(FALSE),
	m_bOptional				(FALSE),
	m_bUseVoidAsDefaultType	(FALSE)
{ 
	Assign(dd); 
}

//----------------------------------------------------------------------------------------------
CDataObjDescription::~CDataObjDescription ()
{
	if (m_pValue && m_bOwnerValue)
		delete m_pValue;

	m_pValue = NULL;
}

//----------------------------------------------------------------------------------------------
DataObj* CDataObjDescription::GetValue ()
{ 
	if (m_pValue == NULL && m_DataType != DataType::Null && m_DataType != DataType::Void && m_DataType != DataType::Variant)
	{
		m_pValue = m_bArray ? new DataArray(m_DataType) : DataObj::DataObjCreate(m_DataType);
		ASSERT_TRACE(m_pValue,"Datamember m_pValue cannot be null in this context");
		m_bOwnerValue = TRUE;
		m_pValue->SetValid(FALSE);
	}
	return m_pValue; 
}

//----------------------------------------------------------------------------------------------
DataType CDataObjDescription::ToDataType (const CString& sType)
{
		 if (_tcsicmp(sType,XML_DATATYPE_STRING_VALUE) == 0)		return DataType::String;
	else if (_tcsicmp(sType,XML_DATATYPE_CI_STRING_VALUE) == 0)		return DataType::String;

	else if (_tcsicmp(sType,XML_DATATYPE_INT_VALUE) == 0)			return DataType::Integer;
	else if (_tcsicmp(sType, L"Int16") == 0)						return DataType::Integer;
	else if (_tcsicmp(sType,XML_DATATYPE_INT2_VALUE) == 0)			return DataType::Integer;

	else if (_tcsicmp(sType,XML_DATATYPE_LONG_VALUE) == 0)			return DataType::Long;
	else if (_tcsicmp(sType, L"Int32") == 0)						return DataType::Long;
	else if (_tcsicmp(sType, L"Int64") == 0)						return DataType::Long;

	else if (_tcsicmp(sType,XML_DATATYPE_DOUBLE_VALUE) == 0)		return DataType::Double;
	else if (_tcsicmp(sType,XML_DATATYPE_PERC_VALUE) == 0)			return DataType::Percent;
	else if (_tcsicmp(sType,XML_DATATYPE_QUANTITY_VALUE) == 0)		return DataType::Quantity;
	else if (_tcsicmp(sType,XML_DATATYPE_MONEY_VALUE) == 0)			return DataType::Money;
	else if (_tcsicmp(sType,XML_DATATYPE_UUID_VALUE) == 0)			return DataType::Guid;

	else if (_tcsicmp(sType,XML_DATATYPE_BOOLEAN_VALUE) == 0)		return DataType::Bool;
	else if (_tcsicmp(sType,XML_DATATYPE_BOOLEAN2_VALUE) == 0)		return DataType::Bool;

	else if (_tcsicmp(sType,XML_DATATYPE_DATETIME_VALUE) == 0)		return DataType::DateTime;
	else if (_tcsicmp(sType,XML_DATATYPE_TIME_VALUE) == 0)			return DataType::Time;
	else if (_tcsicmp(sType,XML_DATATYPE_DATE_VALUE) == 0)			return DataType::Date;
	else if (_tcsicmp(sType,XML_DATATYPE_ELAPSEDTIME_VALUE) == 0)	return DataType::ElapsedTime;
	else if (_tcsicmp(sType,XML_DATATYPE_ENUM_VALUE) == 0)			return DataType::Enum;
	else if (_tcsicmp(sType,XML_DATATYPE_VOID_VALUE) == 0)			return DataType::Void;
	else if (_tcsicmp(sType,XML_DATATYPE_ARRAY_VALUE) == 0)			return DataType::Array;
	else if (_tcsicmp(sType,XML_DATATYPE_BLOB_VALUE) == 0)			return DataType::Blob;
	else if (_tcsicmp(sType,XML_DATATYPE_TEXT_VALUE) == 0)			return DataType::Text;
	else if (_tcsicmp(sType,XML_DATATYPE_IDENTITY_VALUE) == 0)		return DataType::Long;
	else if (_tcsicmp(sType,XML_DATATYPE_DATAARRAY_VALUE) == 0)		return DataType::Array;
	else if (_tcsicmp(sType,XML_DATATYPE_DATAENUM_VALUE) == 0)		return DataType::Enum;
	else if (_tcsicmp(sType,XML_DATATYPE_VARIANT_VALUE) == 0)		return DataType::Variant;
	else if (_tcsicmp(sType, XML_DATATYPE_OBJECT_VALUE) == 0)		return DataType::Object;

	ASSERT_TRACE1(FALSE,"Unknown type: %s",sType);
	return DataType::Null;
}

//----------------------------------------------------------------------------------------------
CString CDataObjDescription::ToString (const DataType& aType)
{
	if (aType == DataType::String)							return XML_DATATYPE_STRING_VALUE;
	else if (aType == DataType::Integer)					return XML_DATATYPE_INT_VALUE;
	else if (aType == DataType::Long)						return XML_DATATYPE_LONG_VALUE;
	else if (aType == DataType::Object)						return XML_DATATYPE_LONG_VALUE;
	else if (aType == DataType::Double)						return XML_DATATYPE_DOUBLE_VALUE;
	else if (aType == DataType::Money)						return XML_DATATYPE_MONEY_VALUE;
	else if (aType == DataType::Quantity)					return XML_DATATYPE_QUANTITY_VALUE;
	else if (aType == DataType::Percent)					return XML_DATATYPE_PERC_VALUE;
	else if (aType == DataType::Bool)						return XML_DATATYPE_BOOLEAN_VALUE;
	else if (aType == DataType::Guid)						return XML_DATATYPE_UUID_VALUE;
	else if (aType == DataType::Date)						return XML_DATATYPE_DATE_VALUE;
	else if (aType == DataType::Time)						return XML_DATATYPE_TIME_VALUE;
	else if (aType == DataType::DateTime)					return XML_DATATYPE_DATETIME_VALUE;
	else if (aType.m_wType == DataType::Enum.m_wType)		return XML_DATATYPE_ENUM_VALUE;
	else if (aType == DataType::ElapsedTime)				return XML_DATATYPE_ELAPSEDTIME_VALUE;
	else if (aType == DataType::Array)						return XML_DATATYPE_ARRAY_VALUE;
	else if (aType == DataType::Text)						return XML_DATATYPE_TEXT_VALUE;
	else if (aType == DataType::Blob)						return XML_DATATYPE_BLOB_VALUE;
	else if (aType == DataType::Void)						return XML_DATATYPE_VOID_VALUE;
	else if (aType == DataType::Variant)					return XML_DATATYPE_VARIANT_VALUE;
	else if (aType == DataType::Null)						return _T("");

	ASSERT (FALSE);
	return _T("");
}

//----------------------------------------------------------------------------------------------
DataObj* CDataObjDescription::InternalCreateDataObj (const CString& sValue, const DataType& aType)
{
	DataObj* pDataObj = DataObj::DataObjCreate(aType);
	if (!sValue.IsEmpty())
		pDataObj->AssignFromXMLString(sValue);
	if (m_bCollateCultureSensitiveValue)
		pDataObj->SetCollateCultureSensitive (TRUE);
	return pDataObj;
}
//----------------------------------------------------------------------------------------------
void CDataObjDescription::CreateDataObj (const CString& sValue, const DataType& aType)
{
	if (m_pValue && m_bOwnerValue)
		delete m_pValue;

	m_pValue = InternalCreateDataObj(sValue,aType);
	m_bOwnerValue = TRUE;
}

//----------------------------------------------------------------------------------------------
void CDataObjDescription::CreateDataObjArray (const CStringArray& aValues, const DataType aType)
{
	if (m_pValue && m_bOwnerValue)
	{
		SAFE_DELETE(m_pValue);
	}

	//viene costruito un DataArray con il baseType corretto
	m_pValue = new DataArray(aType);
	m_bOwnerValue = TRUE;

	DataObj* pDataObj;
	for (int i=0; i <= aValues.GetUpperBound(); i++)
	{
		pDataObj = InternalCreateDataObj(aValues.GetAt(i), aType);
		((DataArray*)m_pValue)->Add(pDataObj);
	}
}

//----------------------------------------------------------------------------------------------
void CDataObjDescription::SetDataType(const DataType& aType)
{
	m_DataType = aType;
	m_bArray = aType == DataType::Array;

	if (m_pValue && m_bOwnerValue)
	{
		SAFE_DELETE(m_pValue);
	}
	else
		m_pValue = NULL;
}

//----------------------------------------------------------------------------------------------
void CDataObjDescription::SetLength (const int& nLength)
{
	m_nLength = nLength;
	if (m_pValue && m_nLength != m_pValue->GetColumnLen())
		m_pValue->SetAllocSize(nLength);
}

//----------------------------------------------------------------------------------------------
void CDataObjDescription::SetArrayType (const DataType& aType)
{
	m_DataType = aType;
	m_bArray = TRUE;

	if (m_pValue && m_bOwnerValue)
	{ 
		SAFE_DELETE(m_pValue);
	}
	else
		m_pValue = NULL;
}

//----------------------------------------------------------------------------------------------
void CDataObjDescription::SetDataTypeTag (const WORD& wTag)
{
	ASSERT (m_DataType == DataType::Enum);

	m_DataType.m_wTag = wTag;
}

//----------------------------------------------------------------------------------------------
void CDataObjDescription::SetDefaultPassedByOut (const BOOL& bValue)
{
	m_bDefaultByOut = bValue;
}

//----------------------------------------------------------------------------------------------
void CDataObjDescription::SetValue(const CString & strValue)
{
	CreateDataObj(strValue, m_DataType);
}

//----------------------------------------------------------------------------------------------
void CDataObjDescription::SetValue (const DataObj& aVal)
{ 
	if (m_pValue)
	{ 
		ASSERT_VALID(m_pValue);
	}

	if (m_pValue == NULL) 
	{
		m_pValue = aVal.DataObjClone(); 
		m_bOwnerValue = TRUE;
	}
	else if (m_pValue->GetDataType() == aVal.GetDataType())
	{
		m_pValue->Assign(aVal); 
		m_pValue->SetCollateCultureSensitive(aVal.IsCollateCultureSensitive () || m_bCollateCultureSensitiveValue);
	}
	else
	{
		if (m_bOwnerValue)
			SAFE_DELETE(m_pValue);

		m_pValue = aVal.DataObjClone(); 

		m_bOwnerValue = TRUE;

		m_pValue->SetCollateCultureSensitive(aVal.IsCollateCultureSensitive () || m_bCollateCultureSensitiveValue);
	}

	if (aVal.GetDataType() == DataType::Array)
	{
		DataArray* pAr = (DataArray*) &aVal;
		if (pAr->GetBaseDataType() != DataType::Null) 
			m_DataType = pAr->GetBaseDataType();
		
		m_bArray = TRUE;
	}
	else
	{
		m_DataType = aVal.GetDataType();

		m_bArray = FALSE;
	}
}

//-----------------------------------------------------------------------------
void CDataObjDescription::SetDataObj (DataObj* pObj)
{
	if (pObj)
		ASSERT_VALID(pObj);

	if (m_pValue && m_bOwnerValue)
	{
		ASSERT_TRACE(pObj != m_pValue,"Parameter pObj must be different than m_pValue\n");	
		SAFE_DELETE(m_pValue); 
	}

	if (pObj->GetDataType() == DataType::Array)
	{
		m_DataType = ((DataArray*)pObj)->GetBaseDataType();
		m_bArray = TRUE;
	}
	else
	{
		m_DataType = pObj->GetDataType();
		m_bArray = FALSE;
	}

	m_pValue = pObj; 
	m_bOwnerValue = FALSE;
}

//-----------------------------------------------------------------------------
void CDataObjDescription::SetDataObj (DataObj* pObj, BOOL bOwnerValue)
{
	SetDataObj (pObj);
	m_bOwnerValue = bOwnerValue;
}

//----------------------------------------------------------------------------------------------
void CDataObjDescription::SetCollateCultureSensitiveValue (BOOL bCollateCultureSensitiveValue)
{
	m_bCollateCultureSensitiveValue = bCollateCultureSensitiveValue;
}

//----------------------------------------------------------------------------------------------
void CDataObjDescription::SetVoidAsDefaultType (const BOOL& bValue)
{
	m_bUseVoidAsDefaultType = bValue;
}

//----------------------------------------------------------------------------------------------
BOOL CDataObjDescription::Parse (CXMLNode* pNode, BOOL bWithValues /*TRUE*/)
{
	if (!pNode)
		return FALSE;

	CXMLDocumentObject* pDoc = pNode->GetXMLDocument();
	CString sDomFileName = pDoc ? pDoc->GetFileName() : _T("");

	// se non ha name lo gestisco lo stesso (ad. es. il returnvalue!)
	CString sTemp;
	pNode->GetAttribute(XML_NAME_ATTRIBUTE, sTemp);
	SetName(sTemp);

	// verifico se il titolo esiste come attributo ed eventualmente penso io a tradurre
	if (pNode->GetAttribute(XML_LOCALIZE_ATTRIBUTE, sTemp))
		SetNotLocalizedTitle(sTemp);

	// modalità di passaggio del parametro
	pNode->GetAttribute(XML_PASSINGMODE_ATTRIBUTE, sTemp);

	if (sTemp.IsEmpty () && m_bDefaultByOut)
		SetPassedMode(m_bDefaultByOut ? _OUT : _IN);
	else if (_tcsicmp(sTemp, XML_PASS_IN_VALUE) == 0)
		SetPassedMode(_IN);
	else if (_tcsicmp(sTemp, XML_PASS_OUT_VALUE) == 0)
		SetPassedMode(_OUT);
	else if (_tcsicmp(sTemp, XML_PASS_INOUT_VALUE) == 0)
		SetPassedMode(_INOUT);

	// se non ha type lo intendo stringa
	pNode->GetAttribute(XML_TYPE_ATTRIBUTE, sTemp);
	if (sTemp.IsEmpty())
		pNode->GetAttribute(XML_DATATYPE_ATTRIBUTE, sTemp);
		
	if (sTemp.IsEmpty())
	{
		if (m_bUseVoidAsDefaultType)
			sTemp = XML_DATATYPE_VOID_VALUE;
		else
		{
			AfxGetDiagnostic()->Add (cwsprintf(_TB("<type> attribute empty in parameter called {0-%s} of object {1-%s} in the file {2-%s}. Parameter ignored."), (LPCTSTR) GetName(), (LPCTSTR) GetTitle(),  (LPCTSTR) sDomFileName));
			return FALSE;
		}
	} 
	
	m_bArray = _tcsicmp(sTemp, XML_DATATYPE_ARRAY_VALUE) == 0;
	
	if (!m_bArray)
	{
		DataType dt = ToDataType(sTemp);
		if (dt == DataType::Null)
			return FALSE;
		SetDataType(dt);
	}

	// TODOBRUNA (AUTOINCREMENT/IDENTITY vd sintassi Ilaria)

	// se si tratta di un enumerativo o di un array cerco il basetype
	if (GetDataType() == DataType::Enum || m_bArray) 
	{
		pNode->GetAttribute(XML_BASETYPE_ATTRIBUTE, sTemp);

		if (sTemp.IsEmpty())
		{
			AfxGetDiagnostic()->Add (cwsprintf(_TB("<basetype> attribute empty (when is required for <type> specified) in parameter {0-%s} of object {1-%s} in the file {2-%s}. Parameter ignored."), (LPCTSTR) GetName(), (LPCTSTR) GetTitle(),  (LPCTSTR) sDomFileName));
			return FALSE;
		}
		else if (m_bArray)	// tipo dell'array
		{
			DataType dt = ToDataType(sTemp);
			if (dt == DataType::Null)
				return FALSE;
			SetArrayType(dt);
		}
	}

	// Tag per l'enumerativo
	if (GetDataType() == DataType::Enum)
	{
		if (_wcsspnp(sTemp, _T("0123456789")) != NULL)
		{
			WORD wTag = AfxGetEnumsTable()->GetEnumTagValue (sTemp);
			if (wTag == TAG_ERROR)
			{
				AfxGetDiagnostic()->Add (cwsprintf(_TB("Unknown enumeration in the called parameter {0-%s} of object {1-%s} in the file {2-%s}. Parameter ignored."), (LPCTSTR) GetName(), (LPCTSTR) GetTitle(), (LPCTSTR) sDomFileName));
				return FALSE;
			}
			SetDataTypeTag(wTag);
		}
		else
		{
			WORD wTag = _tstoi ((LPCTSTR) sTemp);
			if (!AfxGetEnumsTable()->ExistEnumTagValue(wTag))
			{
				AfxGetDiagnostic()->Add (cwsprintf(_TB("Unknown enumeration in the called parameter {0-%s} of object {1-%s} in the file {2-%s}. Parameter ignored."), (LPCTSTR) GetName(), (LPCTSTR) GetTitle(), (LPCTSTR) sDomFileName));
				return FALSE;
			}				
			SetDataTypeTag(wTag);
		}
	}

	if (GetDataType() == DataType::Long) 
	{
		pNode->GetAttribute(XML_BASETYPE_ATTRIBUTE, sTemp);
		if (!sTemp.IsEmpty())
		{
			m_sClassType = sTemp;
			SetDataType(DataType::Object);
		}
	}

	if (pNode->GetAttribute(XML_OPTIONAL_ATTRIBUTE, sTemp))
	{
		if (_tcsicmp(sTemp, XML_TRUE_VALUE) == 0)
		{
			ASSERT_TRACE(GetPassedMode() == _IN,"GetPassedMode must be _IN in this context");
			m_bOptional = TRUE;
		}
	}

	sTemp.Empty();
	pNode->GetAttribute(XML_COLLATESENSITIVE_ATTRIBUTE, sTemp);
	SetCollateCultureSensitiveValue(_tcsicmp(sTemp, XML_TRUE_VALUE) == 0);

	if (GetDataType() == DataType::String)
	{
		pNode->GetAttribute(XML_LENGTH_ATTRIBUTE, sTemp);
		if (sTemp.IsEmpty())
			pNode->GetAttribute(XML_DATALENGTH_ATTRIBUTE, sTemp);

		if (!sTemp.IsEmpty())
		{
			TRY
			{
				int nLength = _ttoi(sTemp);
				if (nLength > 0)
					SetLength (nLength);
			}
			CATCH (CException, e)
			{
				// I ignore no legth
			}
			END_CATCH
		}
	}

	// i valori solo se possibile e richiesti
	if (!bWithValues)
		return TRUE;

	if (pNode->GetAttribute(_T("value"), sTemp))
	{
		CreateDataObj(sTemp, GetDataType());

		m_bOptional = TRUE;
		return TRUE;
	}

	CXMLNodeChildsList* pValues = pNode->GetChilds();
	if (!pValues)
		return TRUE;

	CStringArray aArray;
	for (int i=0; i <= pValues->GetUpperBound(); i++)
	{
		if	(
				!pValues->GetAt(i)->GetName(sTemp) || 
				_tcsicmp(sTemp, XML_VALUE_TAG)
			)
			continue;
			
		pValues->GetAt(i)->GetText(sTemp);

		if (!sTemp.IsEmpty())
			aArray.Add(sTemp);
	}

	if (m_bArray)
		CreateDataObjArray(aArray, GetBaseDataType());
	else if (GetDataType() != DataType::Null)
	{
		CString sValue = aArray.GetSize() == 1 ? aArray.GetAt(0) : _T("");
		CreateDataObj(sValue, GetDataType());
	}
	
	return TRUE;
}

//----------------------------------------------------------------------------
void CDataObjDescription::Unparse(DataStr& strXml)	
{
	CXMLDocumentObject	aDoc(TRUE, FALSE);
	CXMLNode* pRoot = aDoc.CreateRoot(XML_VALUE_TAG);
	if (!pRoot)
		return;

	CString strValue;
	Unparse(pRoot);
	pRoot->GetXML(strValue);

	strXml = strValue;
}

//----------------------------------------------------------------------------------------------
void CDataObjDescription::Unparse (CXMLNode* pNode, BOOL bWithValues /*TRUE*/)
{
	if (!pNode)
		return;

	CString sTemp;
	sTemp = GetName();
	if (!sTemp.IsEmpty())
		pNode->SetAttribute(XML_NAME_ATTRIBUTE, sTemp);
	
	sTemp = GetNotLocalizedTitle();
	if (!sTemp.IsEmpty())
		pNode->SetAttribute(XML_LOCALIZE_ATTRIBUTE, sTemp);

	sTemp = ToString(GetDataType());
	if (IsArray())
	{
		pNode->SetAttribute(XML_TYPE_ATTRIBUTE, (LPCTSTR) XML_DATATYPE_ARRAY_VALUE);
		pNode->SetAttribute(XML_BASETYPE_ATTRIBUTE, ToString(GetBaseDataType()));
	}
	else
		pNode->SetAttribute(XML_TYPE_ATTRIBUTE, sTemp);
	
	if (GetDataType().m_wType == DataType::Enum.m_wType)
		pNode->SetAttribute
			(
				XML_BASETYPE_ATTRIBUTE, 
				cwsprintf(_T("%d"), GetDataType().m_wTag)
			);
	
	// TODOBRUNA (AUTOINCREMENT/IDENTITY vd sintassi Ilaria)

	// passing mode
	switch(GetPassedMode())
	{
	case _IN:
		if (m_bDefaultByOut)
			pNode->SetAttribute
				(
					XML_PASSINGMODE_ATTRIBUTE, 
					XML_PASS_IN_VALUE
				);
		break;
	case _OUT:
			if (!m_bDefaultByOut)
				pNode->SetAttribute
				(
					XML_PASSINGMODE_ATTRIBUTE, 
					XML_PASS_OUT_VALUE
				);
		break;
	case _INOUT:
		pNode->SetAttribute
			(
				XML_PASSINGMODE_ATTRIBUTE, 
				XML_PASS_INOUT_VALUE
			);
		break;
	}
	
	// i valori solo se possibile e richiesti
	if (!bWithValues || GetValue() == NULL)
		return;

	CXMLNode* pValue;
	if (IsArray())
	{
		DataArray* pAr = (DataArray*)GetValue();
		for (int i=0; i < pAr->GetSize(); i++)
		{
			pValue = pNode->CreateNewChild(XML_VALUE_TAG);
			pValue->SetText((LPCTSTR)(pAr->GetAt(i)->FormatDataForXML()));
		}
	}
	else
		pNode->SetAttribute(_T("value"), GetValue()->FormatDataForXML());
}

//--------------------------------------------------------------------
BOOL CDataObjDescription::IsEqual (const CDataObjDescription& dd)
{
	return 	CBaseDescription::IsEqual(dd) && 
			m_DataType		== dd.m_DataType &&
			m_pValue		== dd.m_pValue &&
			m_nPassedMode	== dd.m_nPassedMode &&
			m_bDefaultByOut	== dd.m_bDefaultByOut &&
			m_bOwnerValue	== dd.m_bOwnerValue &&
			m_bArray		== dd.m_bArray &&
			m_nLength		== dd.m_nLength && 
			m_bCollateCultureSensitiveValue == dd.m_bCollateCultureSensitiveValue &&
			m_sClassType					== dd.m_sClassType &&
			m_bUseVoidAsDefaultType			== dd.m_bUseVoidAsDefaultType;
}

//----------------------------------------------------------------------------
CDataObjDescription& CDataObjDescription::Assign (const CDataObjDescription& dd)
{
	if (m_bOwnerValue && m_pValue)
	{
		delete m_pValue;
		m_pValue = NULL;
	}

	CBaseDescription::Assign(dd);

	m_DataType		= dd.m_DataType;
	m_bOwnerValue	= dd.m_bOwnerValue;

	if (dd.m_pValue)
	{
		if (m_bOwnerValue)
			SetValue(*dd.m_pValue);
		else
			m_pValue = dd.m_pValue;
	}

	m_nPassedMode	= dd.m_nPassedMode;
	m_bDefaultByOut	= dd.m_bDefaultByOut;
	m_bArray		= dd.m_bArray;
	
	m_bOptional		= dd.m_bOptional;
	m_nLength		= dd.m_nLength;
	m_bCollateCultureSensitiveValue = dd.m_bCollateCultureSensitiveValue;

	m_bUseVoidAsDefaultType = dd.m_bUseVoidAsDefaultType;

	m_sClassType			= dd.m_sClassType;

	return *this;
}

//----------------------------------------------------------------------------------------------
CBaseDescription* CDataObjDescription::Clone ()
{
	return new CDataObjDescription(*this);
}


//--------------------------------------------------------------------
BOOL CDataObjDescription::operator== (const CDataObjDescription& dd) 
{ 
	return IsEqual (dd);
}

//--------------------------------------------------------------------
BOOL CDataObjDescription::operator!= (const CDataObjDescription& dd) 
{ 
	return !IsEqual (dd);
}
