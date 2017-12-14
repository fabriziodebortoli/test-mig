
#include "stdafx.h"

#include <TbXmlCore\XmlGeneric.h>
#include <TbXmlCore\XmlTags.h>

#include <TbNameSolver\FileSystemFunctions.h>
#include <TbNameSolver\TBNamespaces.h>
#include <TbNameSolver\Diagnostic.h>

#include "GeneralFunctions.h"
#include "EnumsTable.h"

#include "FunctionCall.h"

static const TCHAR szRetValueParam[]	= _T("@@RetValue@@");
static const TCHAR szResponse[]		= _T("Response");
static const TCHAR szReturnValue[]	= _T("ReturnValue");
static const TCHAR szError[]			= _T("Error");

#define	XML_SERVICE_ATTRIBUTE			_T("service")
#define	XML_SERVICE_NAMESPACE_ATTRIBUTE	_T("serviceNamespace")
#define	XML_SERVER_ATTRIBUTE			_T("server")
#define	XML_PORT_ATTRIBUTE				_T("port")
#define	XML_WCF_ATTRIBUTE				_T("WCF")
#define	XML_THISCALL_ATTRIBUTE			_T("thiscall_method")
#define	XML_CLASSTYPE_ATTRIBUTE			_T("classType")
#define	XML_EXECUTEDEPLOY_POLICY		_T("executeDeployPolicy")


// Oggetto che effettua una chiamata ad una funzione remota attraverso socket in xml
// dopo aver effettuato la chiamata attende che il server risponda
//----------------------------------------------------------------------------
//						CFunctionDescription
//----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CFunctionDescription, CBaseDescription);

//------------------------------------------------------------------------------
CFunctionDescription::CFunctionDescription()
	: 
	CBaseDescription		(CTBNamespace::FUNCTION),
	m_bPostCommand			(TRUE),
	m_bAlwaysCalledIfEvent	(FALSE),
	m_nPort					(0),
	m_bPublished			(TRUE)
{
}

//------------------------------------------------------------------------------
CFunctionDescription::CFunctionDescription(CString strFunctionName)
	: 
	CBaseDescription		(CTBNamespace::FUNCTION),
	m_bPostCommand			(TRUE),
	m_bAlwaysCalledIfEvent	(FALSE),
	m_nPort					(0),
	m_bPublished			(TRUE)
{
	SetName(strFunctionName);
}

//------------------------------------------------------------------------------
CFunctionDescription::CFunctionDescription(CTBNamespace::NSObjectType aNSType)
	:
	CBaseDescription		(aNSType),
	m_bPostCommand			(TRUE),
	m_bAlwaysCalledIfEvent	(FALSE),
	m_nPort					(0),
	m_bPublished			(TRUE)
{
}

//------------------------------------------------------------------------------
CFunctionDescription::CFunctionDescription(const CTBNamespace& ns)
	:
	CBaseDescription		(ns),
	m_bPostCommand			(TRUE),
	m_bAlwaysCalledIfEvent	(FALSE),
	m_nPort					(0),
	m_bPublished			(TRUE)
{
}

//----------------------------------------------------------------------------
const CString CFunctionDescription::GetTitle() const
{
	return AfxLoadXMLString (
								m_sNotLocalizedTitle, 
								szWebMethods, 
								AfxGetDictionaryPathFromNamespace(m_Namespace, TRUE)
							);
}


//aggiunge un parametro di input 
//----------------------------------------------------------------------------
int CFunctionDescription::InternalAddParam(const CString& strParamName, DataObj* pValue, BOOL bDataObjOwner)
{
	if (strParamName.GetLength() == 0)
		return -1;
	RemoveParam(strParamName);
	
	CDataObjDescription* pParam = new CDataObjDescription(strParamName, bDataObjOwner ? pValue->DataObjClone() : pValue, bDataObjOwner);

	return m_arFunctionParams.Add(pParam);
}

//----------------------------------------------------------------------------
int CFunctionDescription::AddParam(const CString& strParamName, DataObj* pValue)
{
	return InternalAddParam(strParamName, pValue, TRUE);
}

//----------------------------------------------------------------------------------------------
int CFunctionDescription::AddParam(CDataObjDescription* pParam)
{
	RemoveParam(pParam->GetName());
	return m_arFunctionParams.Add (pParam);
}

//aggiunge un parametro di input di tipo stringa
//----------------------------------------------------------------------------
int CFunctionDescription::AddStrParam(const CString& strParamName, const CString& strValue)
{
	if(strParamName.GetLength() == 0)
		return -1;
	RemoveParam(strParamName);
	
	CDataObjDescription* pParam = new CDataObjDescription(strParamName, new DataStr(strValue), TRUE);
	
	return m_arFunctionParams.Add(pParam);
}

//aggiunge un parametro di input di tipo intero
//----------------------------------------------------------------------------
int CFunctionDescription::AddIntParam(const CString& strParamName, int nValue)
{
	if(strParamName.GetLength() == 0)
		return -1;
	RemoveParam(strParamName);
	
	CDataObjDescription* pParam = new CDataObjDescription(strParamName, new DataInt(nValue), TRUE);
	
	return m_arFunctionParams.Add(pParam);
}

//aggiunge un parametro di input di tipo datetime
//----------------------------------------------------------------------------
BOOL CFunctionDescription::AddTimeParam(const CString& strParamName, CTime dtValue)
{
	if(strParamName.GetLength() == 0)
		return FALSE;
	RemoveParam(strParamName);
	
	CDataObjDescription* pParam = new CDataObjDescription(strParamName, new DataDate(dtValue), TRUE);
	m_arFunctionParams.Add(pParam);

	return TRUE;
}

//----------------------------------------------------------------------------
int CFunctionDescription::AddOutParam(const CString& strParamName, DataObj* pValue)
{
	if(strParamName.GetLength() == 0 || pValue == NULL)
		return -1;
	RemoveParam(strParamName);
	
	int i = AddParam(new CDataObjDescription(strParamName, pValue->GetDataType(), CDataObjDescription::_OUT));
	CDataObjDescription* pd = GetParamDescription(i);
	pd->SetDataObj(pValue);
	return i;
}

//----------------------------------------------------------------------------
int CFunctionDescription::AddOutParam(CDataObjDescription* pParam)
{
	RemoveParam(pParam->GetName());
	
	pParam->SetPassedMode(CDataObjDescription::_OUT);
	int i = AddParam(pParam);
	return i;
}

//----------------------------------------------------------------------------
int CFunctionDescription::AddInOutParam(CDataObjDescription* pParam)
{
	RemoveParam(pParam->GetName());
	
	pParam->SetPassedMode(CDataObjDescription::_INOUT);
	int i = AddParam(pParam);
	return i;
}

//----------------------------------------------------------------------------
int CFunctionDescription::AddInOutParam(const CString& strParamName, DataObj* pValue)
{
	if(strParamName.GetLength() == 0 || pValue == NULL)
		return -1;
	RemoveParam(strParamName);
	
	int i = AddParam(new CDataObjDescription(strParamName, pValue->GetDataType(), CDataObjDescription::_INOUT));
	CDataObjDescription* pd = GetParamDescription(i);
	pd->SetDataObj(pValue);
	return i;
}

//----------------------------------------------------------------------------
//int CFunctionDescription::AddOutParam(const CString& strParamName, DataType baseType, DataObjArray* pValues)
//{
//	if(strParamName.GetLength() == 0 || pValues == NULL)
//		return -1;
//	RemoveParam(strParamName);
//
//	int i = AddParam(new CDataObjDescription(strParamName, baseType, CDataObjDescription::_OUT));
//	CDataObjDescription* pd = GetParamDescription(i);
//	pd->SetDataObj(pValues);
//	return i;
//}
//
////----------------------------------------------------------------------------
//int CFunctionDescription::AddInOutParam(const CString& strParamName, DataType baseType, DataObjArray* pValues)
//{
//	if(strParamName.GetLength() == 0 || pValues == NULL)
//		return -1;
//	RemoveParam(strParamName);
//
//	int i = AddParam(new CDataObjDescription(strParamName, baseType, CDataObjDescription::_INOUT));
//	CDataObjDescription* pd = GetParamDescription(i);
//	pd->SetDataObj(pValues);
//	return i;
//}

//rinuove un parametro
//----------------------------------------------------------------------------
BOOL CFunctionDescription::RemoveParam (const CString& strParamName)
{
	int nIdx = GetParamIndex(strParamName);
	if (nIdx > -1)
		m_arFunctionParams.RemoveAt(nIdx);

	return nIdx > -1;
}

//----------------------------------------------------------------------------
int	 CFunctionDescription::GetParamIndex (const CString& strParamName)
{
	for(int i = 0 ; i < m_arFunctionParams.GetSize() ; i++)
	{
		CDataObjDescription* pdod = (CDataObjDescription*) m_arFunctionParams.GetAt(i);
		if (_tcsicmp(pdod->GetName(), strParamName) == 0)
		{
			return i;
		}
	}
	return -1;
}

//----------------------------------------------------------------------------
void CFunctionDescription::SetParamValue (const CString& strParamName, const DataObj& aVal)	
{ 
	GetParamDescription(strParamName)->SetValue(aVal);			
}

void CFunctionDescription::SetParamValue (int i, const DataObj& aVal) 
{ 
	GetParamDescription(i)->SetValue(aVal); 
}

//----------------------------------------------------------------------------
DataObj* CFunctionDescription::GetParamValue (const CString& strParamName)	
{ 
	CDataObjDescription* par = GetParamDescription(strParamName);
	return par ? par->GetValue(): NULL; 
}

//----------------------------------------------------------------------------
DataObj* CFunctionDescription::GetParamValue (int i)	
{ 
	CDataObjDescription* par = GetParamDescription(i);
	return par ? par->GetValue(): NULL; 
}

//dopo che la funzione è stata processata si possono chiedere i valori di ritorno
//----------------------------------------------------------------------------
BOOL CFunctionDescription::GetParamValue(CString strParamName, CString& strRetVal)
{
	strRetVal = _T("");
	CDataObjDescription* par = GetParamDescription(strParamName);

	if (
			strParamName == par->GetName() &&
			par->GetDataType() == DataType::String
		)
	{
		strRetVal = par->GetValue()->Str();
		return TRUE;
	}
	return FALSE;
}

//----------------------------------------------------------------------------
BOOL CFunctionDescription::GetParamValue(CString strParamName, int& nRetVal)
{
//pulire
	for(int i = 0 ; i < m_arFunctionParams.GetSize() ; i++)
	{
		CDataObjDescription* pFunctionParam = ((CDataObjDescription*)m_arFunctionParams.GetAt(i)); 
		if(strParamName == pFunctionParam->GetName() &&	pFunctionParam->GetDataType() == DataType::Integer)
		{
			nRetVal = *((DataInt*)(pFunctionParam->GetValue()));
			return TRUE;
		}
	}
	return FALSE;
}

//----------------------------------------------------------------------------
BOOL CFunctionDescription::GetBoolParamValue(CString strParamName, BOOL& bRetVal)
{
	//pulire
	for(int i = 0 ; i < m_arFunctionParams.GetSize() ; i++)
	{
		CDataObjDescription* pFunctionParam = ((CDataObjDescription*)m_arFunctionParams.GetAt(i)); 
		if(strParamName == pFunctionParam->GetName() && pFunctionParam->GetDataType() == DataType::Bool)
		{
			bRetVal = *((DataBool*)(pFunctionParam->GetValue()));
			return TRUE;
		}
	}
	return FALSE;
}

//----------------------------------------------------------------------------
BOOL CFunctionDescription::GetParamValue(CString strParamName, CTime& dtRetVal)
{
	//pulire
	for(int i = 0 ; i < m_arFunctionParams.GetSize() ; i++)
	{
		CDataObjDescription* pFunctionParam = ((CDataObjDescription*)m_arFunctionParams.GetAt(i)); 
		if(strParamName == pFunctionParam->GetName() && pFunctionParam->GetDataType() == DataType::DateTime)
		{
			dtRetVal = CStringToCTime(pFunctionParam->GetValue()->FormatDataForXML());
			return TRUE;
		}
	}
	return FALSE;
}

//----------------------------------------------------------------------------
CDataObjDescription* CFunctionDescription::GetParamDescription (const int& i) 
{ 
	return i < GetParameters().GetSize() ? (CDataObjDescription*)(GetParameters().GetAt(i)) : NULL;
}

//----------------------------------------------------------------------------
CDataObjDescription* CFunctionDescription::GetParamDescription	 (const CString& strParamName) 
{ 
	for(int i = 0 ; i < m_arFunctionParams.GetSize() ; i++)
	{
		CDataObjDescription*  p = GetParamDescription(i);
		if (_tcsicmp(strParamName, p->GetName()) == 0)
			return p;
	}
	return NULL;
}

//----------------------------------------------------------------------------
BOOL CFunctionDescription::SetParametersValue	(CBaseDescriptionArray& ar)
{
	if (m_arFunctionParams.GetUpperBound() != ar.GetUpperBound())
		return FALSE;

	for (int i = 0; i <= ar.GetUpperBound(); i++)
	{
		CDataObjDescription* pDst = (CDataObjDescription*) m_arFunctionParams.GetAt(i);
		CDataObjDescription* pSrc = (CDataObjDescription*) ar.GetAt(i);

		if (!DataType::IsCompatible(pSrc->GetDataType(), pDst->GetDataType()))
			return FALSE;

		pDst->GetValue()->Assign(*pSrc->GetValue());
	}
	return TRUE;
}

//----------------------------------------------------------------------------
void CFunctionDescription::RemoveParamsStartingWith(const CString& strParamStart)
{
	for (int i = m_arFunctionParams.GetUpperBound(); i >=0; i--)
	{
		CDataObjDescription*  p = GetParamDescription(i);
		if (p->GetName().Find(strParamStart) == 0)
			m_arFunctionParams.RemoveAt(i);
	}
}

//----------------------------------------------------------------------------
int CFunctionDescription::GetContextHandle()
{
	int paramCount = GetParameters().GetCount();
	int contextHandle = 0;
	if (paramCount > 0)
	{
		CDataObjDescription* pParam = GetParamDescription(0);
		if (pParam->GetName() == _T("handle")
			&& pParam->IsPassedModeIn()
			&& pParam->GetDataType() == DataType::Object) 
		{
			contextHandle = *((DataLng*) pParam->GetValue()); 
		}
	}

	return contextHandle;
}

//----------------------------------------------------------------------------
BOOL CFunctionDescription::GetParamValue(CString strParamName, CStringArray& arrayRetVal)
{
	arrayRetVal.RemoveAll();

	for(int i = 0 ; i < m_arFunctionParams.GetSize() ; i++)
	{
		CDataObjDescription* pFunctionParam = (CDataObjDescription*) m_arFunctionParams.GetAt(i);
		if(!pFunctionParam)
			continue;

		if(strParamName == pFunctionParam->GetName() && pFunctionParam->IsArray())
		{
			DataArray* pAr = (DataArray*) pFunctionParam->GetValue();
			for (int n = 0 ; n < pAr->GetSize() ; n++)
				arrayRetVal.Add(pAr->GetAt(n)->Str());
			
			return TRUE;
		}
	}
	return FALSE;
}

//----------------------------------------------------------------------------------------------
const BOOL CFunctionDescription::IsFullExecutePolicy () const
{
	return _tcsicmp(m_sExecutePolicy, XML_DEPLOYPOLICY_FULL_VALUE) == 0;
}

//----------------------------------------------------------------------------------------------
const BOOL CFunctionDescription::IsAddOnExecutePolicy () const
{
	return _tcsicmp(m_sExecutePolicy, XML_DEPLOYPOLICY_ADDON_VALUE) == 0;
}

//----------------------------------------------------------------------------------------------
const BOOL CFunctionDescription::IsBaseExecutePolicy () const
{
	return _tcsicmp(m_sExecutePolicy, XML_DEPLOYPOLICY_BASE_VALUE) == 0;
}

//----------------------------------------------------------------------------------------------
void CFunctionDescription::SetAlwaysCalledIfEvent (const BOOL bValue)
{
	m_bAlwaysCalledIfEvent = bValue;
}

//----------------------------------------------------------------------------------------------
void CFunctionDescription::SetReturnValueDescription (const CDataObjDescription& aRetValue)
{
	m_ReturnValue = aRetValue;
}

//----------------------------------------------------------------------------
BOOL CFunctionDescription::ParseArguments (const DataStr& strXml)
{
	CString sArgs = strXml.GetString();
	sArgs.Trim();
	if (sArgs.IsEmpty())
		return TRUE;

	if (sArgs[0] != '<')
	{
		return ParseTbLinkArguments(sArgs);
	}

	CXMLDocumentObject	aDoc(FALSE, FALSE);
	CXMLNode*			pRoot = NULL;
	
	if (!aDoc.LoadXML(sArgs))
		return FALSE;
	
	pRoot = aDoc.GetRoot();

	if (!pRoot)
		return FALSE;
	
	return ParseArguments(pRoot);
}

//----------------------------------------------------------------------------
BOOL CFunctionDescription::ParseArguments (CXMLNode* pNode, CTBNamespace* pParent /*= NULL*/)
{
	if (!pNode)
		return FALSE;
	if (!pNode->GetChilds())
		return TRUE;

	// ora processo i singoli parameters
	CString sNodeName;
	for (int i=0; i <= pNode->GetChilds()->GetUpperBound(); i++)
	{
		CXMLNode* pParamNode = pNode->GetChilds()->GetAt(i);

		if (!pParamNode || !pParamNode->GetName(sNodeName))
			continue;

		if (sNodeName == XML_TBSCRIPT_TAG)
		{
			ParseTbScript (pParamNode);
			continue;
		}
		
		if (sNodeName == XML_PARAMETER_TAG)	//potrebbero esserci nodi con commenti
		{
			CDataObjDescription* pNewDescription = new CDataObjDescription ();
			if (pParent)
				pNewDescription->SetNamespace(*pParent);

			pNewDescription->SetDefaultPassedByOut(FALSE);

			if (pNewDescription->Parse(pParamNode))
				AddParam(pNewDescription);
			else
				delete pNewDescription;
		}
		//else
		//{
		//	ASSERT_TRACE(sNodeName.Compare(L"#comment") == 0, (pParent ? pParent->ToString() : L"") + L" - " + L"Invalid node into parameters list: " + sNodeName);
		//}
	}
	return TRUE;
}

//----------------------------------------------------------------------------
BOOL CFunctionDescription::ParseTbLinkArguments (const CString& sArgs)
{
	CStringArray ar;
	int n = ::CStringArray_Split(ar, sArgs);

	for (int i = 0; i < n; i++)
	{
		CString p = ar[i];
		int j = //p.Find('=');
				_tcscspn((LPCTSTR)p, L":=");
		//if (j < 0) 
		if (j == p.GetLength())
			continue;
		
		CString sName	= p.Left(j);
		CString sVal	= p.Mid(j + 1);

		CDataObjDescription* pNewDescription = new CDataObjDescription (sName, sVal, L"String");

		AddParam(pNewDescription);
	}

	return TRUE;
}

//----------------------------------------------------------------------------
BOOL CFunctionDescription::UnparseTbLinkArguments (DataStr& sArgs)
{
	CStringArray ar;
	CString str, s, n;

	for (int i = 0; i <= m_arFunctionParams.GetUpperBound(); i++)
	{
		CDataObjDescription* pDes = (CDataObjDescription*) m_arFunctionParams.GetAt(i);

		CString v = pDes->GetValue()->Str(0,0);

		if (v.Find(';') > -1)
		{
			ASSERT(FALSE);
			continue;
		}

		s.Format(L"%s=%s", pDes->GetName(), v);
		ar.Add(s);
	}

	::CStringArray_Concat (ar, str);
	return TRUE;
}

//----------------------------------------------------------------------------
BOOL CFunctionDescription::UnparseArguments (DataStr& strXml, LPCTSTR szRoot/* = XML_FUNCTION_TAG*/)	//XML_ARGUMENTS_TAG
{
	CXMLDocumentObject	aDoc(TRUE, FALSE);
	CXMLNode* pRoot = aDoc.CreateRoot(szRoot);
	if (!pRoot)
		return FALSE;
	
	CString strValue; 
	if (UnparseArguments(pRoot) && pRoot->GetXML(strValue))
	{
		strXml = strValue;
		return TRUE;
	}

	return FALSE;
}

//----------------------------------------------------------------------------
BOOL CFunctionDescription::UnparseArguments(CXMLNode* pNode)
{
	if (!pNode)
		return FALSE;
	
	for (int i = 0; i <= m_arFunctionParams.GetUpperBound(); i++)
	{
		CXMLNode* pParamNode = pNode->CreateNewChild(XML_PARAMETER_TAG); 
		CDataObjDescription* pDescription = (CDataObjDescription*) m_arFunctionParams.GetAt(i);
		pDescription->Unparse(pParamNode);
	}
	
	return TRUE;
}

//--------------------------------------------------------------------
CFunctionDescription& CFunctionDescription::operator= (const CFunctionDescription& fd) 
{ 
	Assign(fd); 
	return *this; 
}

//--------------------------------------------------------------------
BOOL CFunctionDescription::operator== (const CFunctionDescription& fd) 
{ 
	return IsEqual (fd);
}

//--------------------------------------------------------------------
BOOL CFunctionDescription::operator!= (const CFunctionDescription& fd) 
{ 
	return !IsEqual (fd);
}

//--------------------------------------------------------------------
CFunctionDescription* CFunctionDescription::Clone() 
{ 
	return new CFunctionDescription(*this); 
}

//--------------------------------------------------------------------
CString CFunctionDescription::ToString()
{
	CString ret;
	if (GetReturnValue())
		ret.Append(cwsprintf(_TB("Return value: {0-%s}"), GetReturnValue()->Str()));
	
	for (int i = 0; i < GetParameters().GetCount(); i++)
		ret.Append(cwsprintf(_TB("\r\nParam name: {0-%s}\t\tParam value: {1-%s}"), GetParamDescription(i)->GetName(), GetParamValue(i)->Str()));
	return ret;
}

//--------------------------------------------------------------------
BOOL CFunctionDescription::IsEqual (const CFunctionDescription& fd)
{
	BOOL bOk =
			m_Namespace					== fd.m_Namespace &&
			m_sName						== fd.m_sName &&
			m_NSType					== fd.m_NSType &&

			m_ReturnValue				== fd.m_ReturnValue &&
			m_arFunctionParams.GetSize()== fd.m_arFunctionParams.GetSize() &&

			m_strError					== fd.m_strError &&

			m_strService				== fd.m_strService &&			
			m_strServiceNamespace		== fd.m_strServiceNamespace &&	
			m_strServer					== fd.m_strServer &&			
			m_nPort						== fd.m_nPort &&				

			m_bAlwaysCalledIfEvent		== fd.m_bAlwaysCalledIfEvent &&
			m_bPostCommand				== fd.m_bPostCommand && 
			m_bPublished				== fd.m_bPublished && 

			m_sTBScript					== fd.m_sTBScript &&
			m_sClassType				== fd.m_sClassType
			;

			//TODO: occorre confrontare i datatype dei parametri oltre al numero
			//m_arFunctionParams == fd.m_arFunctionParams

	if (!bOk)
		return FALSE;

	// ora comparo il contenuto dei parametri
	for (int i=0; i <= m_arFunctionParams.GetUpperBound(); i++)
		if (*((CDataObjDescription*) m_arFunctionParams.GetAt(i)) != *((CDataObjDescription*) fd.m_arFunctionParams.GetAt(i)))
		{
			bOk = FALSE;
			break;
		}

	return bOk;
}

//----------------------------------------------------------------------------
void CFunctionDescription::Assign (const CFunctionDescription& fd)
{
	// base description
	CBaseDescription::Assign(fd);

	// locali
	m_ReturnValue			= fd.m_ReturnValue;

	m_strError				= fd.m_strError;

	m_strServer				= fd.m_strServer;
	m_strService			= fd.m_strService;
	m_strServiceNamespace	= fd.m_strServiceNamespace;
	m_nPort					= fd.m_nPort;

	m_bPublished			= fd.m_bPublished;
	m_bPostCommand			= fd.m_bPostCommand;
	m_bAlwaysCalledIfEvent	= fd.m_bAlwaysCalledIfEvent;

	m_sTBScript				= fd.m_sTBScript;
	m_sClassType			= fd.m_sClassType;
	m_sManagedType			= fd.m_sManagedType;

	m_arFunctionParams.RemoveAll();

	for (int i = 0; i <= fd.m_arFunctionParams.GetUpperBound(); i++)
	{
		CDataObjDescription* pDescri = new CDataObjDescription(*(CDataObjDescription*)(fd.m_arFunctionParams.GetAt(i)));
		m_arFunctionParams.Add(pDescri);
	}
}
//-----------------------------------------------------------------------------
// Parsing degli elementi di prototipo
//----------------------------------------------------------------------------
BOOL CFunctionDescription::ParsePrototype (CXMLNode* pNode, const CTBNamespace& aParent)
{
	if (!ParseRequest(pNode, aParent))
		return FALSE;

	// verifico se il titolo esiste come attributo o come sottotag
	CString sValue;
	CXMLNode* pDocNode;
	if (!pNode->GetAttribute(XML_LOCALIZE_ATTRIBUTE, sValue))
		if (pDocNode = pNode->GetChildByName(XML_TITLE_TAG))
			pDocNode->GetText(sValue);

	if (sValue.IsEmpty())
		sValue = GetName();
	SetNotLocalizedTitle(sValue);

	// verifico la pubblicazione dell'oggetto
	if (pNode->GetAttribute(XML_PUBLISHED_ATTRIBUTE, sValue))
		SetPublished (_tcsicmp(sValue, XML_FALSE_VALUE));
	else if (pNode->GetAttribute(XML_REPORT_ATTRIBUTE, sValue))
		SetPublished (_tcsicmp(sValue, XML_FALSE_VALUE));

	pNode->GetAttribute(XML_MANAGED_TYPE_ATTRIBUTE, m_sManagedType);
	
	CString sClassType;
	if (
		pNode->GetAttribute(XML_CLASSTYPE_ATTRIBUTE, sClassType) &&
		!sClassType.IsEmpty()
		)
		m_sClassType = sClassType;

	// attributes as Event Handler 
	if (pNode->GetAttribute(XML_ALWAYSRECEIVEEVENT_ATTRIBUTE, sValue))
		SetAlwaysCalledIfEvent(_tcsicmp(sValue, XML_TRUE_VALUE) == 0);

	pNode->GetAttribute(XML_EXECUTEDEPLOY_POLICY, m_sExecutePolicy);
	m_sExecutePolicy = m_sExecutePolicy.Trim ();
	if (m_sExecutePolicy.IsEmpty())
		m_sExecutePolicy = XML_DEPLOYPOLICY_BASE_VALUE;

	if (
			_tcsicmp(m_sExecutePolicy, XML_DEPLOYPOLICY_BASE_VALUE) && 
			_tcsicmp(m_sExecutePolicy, XML_DEPLOYPOLICY_FULL_VALUE) &&
			_tcsicmp(m_sExecutePolicy, XML_DEPLOYPOLICY_ADDON_VALUE)
		)
	{
		AfxGetDiagnostic()->Add(cwsprintf(_T("Attribute executeDeployPolicy= must be 'base' or 'full' or 'addon' for EventHandler {0-%s}\r\n"), GetNamespace().ToUnparsedString()));
		return FALSE;
	}

	return TRUE;
}
//----------------------------------------------------------------------------
void CFunctionDescription::UnparsePrototype	(CXMLNode* pNode, const CString& sContainerTag /*_T("")*/)
{
	UnparseRequest(pNode, sContainerTag);
	
	CXMLNode* pNewNode = pNode->GetChildByAttributeValue(XML_FUNCTION_TAG, XML_NAMESPACE_ATTRIBUTE, GetNamespace().ToString());

	if (!GetTitle().IsEmpty())
		pNewNode->SetAttribute(XML_LOCALIZE_ATTRIBUTE, GetTitle());

	// come evento riceve sempre i messaggi
	if (AlwaysCalledIfEvent())
		pNewNode->SetAttribute(XML_ALWAYSRECEIVEEVENT_ATTRIBUTE, XML_TRUE_VALUE);

}

//----------------------------------------------------------------------------
BOOL CFunctionDescription::ParseReturnValue	(CXMLNode* pNode, const CTBNamespace& aParent)
{
	// nel Return Value di default il parametro é ByOut
	CDataObjDescription  aParam;
	aParam.SetNamespace(aParent);
	aParam.SetDefaultPassedByOut(TRUE);
	aParam.SetVoidAsDefaultType(m_ReturnValue.UseVoidAsDefaultType());
	if (aParam.Parse (pNode, FALSE))
	{
		SetReturnValueDescription (aParam);
		return TRUE;
	}
	return FALSE;
}

//----------------------------------------------------------------------------
void CFunctionDescription::ParseTbScript (CXMLNode* pNode)
{
	CXMLNodeChildsList* pCDATANodes = pNode->GetChildsByType(NODE_CDATA_SECTION);
	if (pCDATANodes && pCDATANodes->GetSize() > 0)
	{
		CString sTemp;
		CXMLNode* pCDATANode = pCDATANodes->GetAt(0);
		if (pCDATANode && pCDATANode->GetNodeValue(sTemp) && !sTemp.IsEmpty())
			  SetTBScript(sTemp);
	}
	SAFE_DELETE(pCDATANodes);
}

//----------------------------------------------------------------------------
BOOL CFunctionDescription::ParseRequest (CXMLNode* pNode, const CTBNamespace& aParent)
{
	if (!pNode)
		return FALSE;

	// provo a vedere se per caso la function fosse sotto al tag contenitore
	CXMLNode* pParamsNode = pNode->GetChildByName(XML_FUNCTION_TAG);
	if (!pParamsNode)
	{
		pParamsNode = pNode->GetChildByName(XML_EVENT_TAG);
		if (!pParamsNode)
			pParamsNode = pNode;
	}

	CString sName;
	pParamsNode->GetAttribute(XML_NAMESPACE_ATTRIBUTE, sName);
	if (sName.IsEmpty())
		pParamsNode->GetAttribute(XML_NAME_ATTRIBUTE, sName);
	if (sName.IsEmpty())
	{
		// in extremis provo anche sul contenitore
		pNode->GetAttribute(XML_NAMESPACE_ATTRIBUTE, sName);
		if (sName.IsEmpty())
		{
			AfxGetDiagnostic()->Add(cwsprintf(_TB("<Function> object of the {0-%s} module without <name> or <namespace> attribute. Declaration ignored."), 
							(LPCTSTR) aParent.ToString()));
			return FALSE;
		}
	}

	CTBNamespace aFunctionNs(m_NSType);
	aFunctionNs.AutoCompleteNamespace(m_NSType, sName, aParent);
	if (aFunctionNs.IsValid())
		SetNamespace (aFunctionNs);
	else
		SetName(sName);

	CString sTemp;

	// eventuale WEB service esterno
	pParamsNode->GetAttribute (XML_SERVICE_ATTRIBUTE, m_strService);

	// eventuale namespace del WEB service esterno
	pParamsNode->GetAttribute (XML_SERVICE_NAMESPACE_ATTRIBUTE, m_strServiceNamespace);

	// eventuale server per WEB service esterno
	pParamsNode->GetAttribute (XML_SERVER_ATTRIBUTE, m_strServer);

	// eventuale porta per WEB service esterno
	pParamsNode->GetAttribute (XML_PORT_ATTRIBUTE, sTemp);
	m_nPort = _ttoi(sTemp);
	if (m_nPort == 0) m_nPort =80;

	BOOL bOk = ParseReturnValue(pParamsNode, aParent);
	if (!bOk)
	{
		AfxGetDiagnostic()->Add(
			cwsprintf(_TB("<Function> {0-%s} object of the {1-%s} module has wrong return type. Declaration ignored."), 
			sName, (LPCTSTR) aParent.ToString())
			);
		return FALSE;
	}

	bOk = ParseArguments(pParamsNode, const_cast<CTBNamespace*>(&aParent));
	if (!bOk)
	{
		AfxGetDiagnostic()->Add(
			cwsprintf(_TB("<Function> {0-%s} object of the {1-%s} module has wrong parameters. Declaration ignored."), 
			sName, (LPCTSTR) aParent.ToString())
			);
		return FALSE;
	}
	
	return TRUE;
}
//----------------------------------------------------------------------------
void CFunctionDescription::UnparseRequest (CXMLNode* pNode, const CString& sContainerTag /*_T("")*/)
{
	// aggiungo il nuovo child
	pNode->SetAttribute (XML_NAMESPACE_ATTRIBUTE, GetNamespace().ToString());
	pNode->SetAttribute (XML_LOCALIZE_ATTRIBUTE, GetNotLocalizedTitle());

	CXMLNode* pParamsNode;
	
	if (!sContainerTag.IsEmpty())
		pParamsNode = pNode->CreateNewChild(sContainerTag);
	else
		pParamsNode = pNode;

	// return value
	GetReturnValueDescription().SetDefaultPassedByOut(TRUE);
	GetReturnValueDescription().Unparse (pParamsNode, FALSE);

	// parametri
	CDataObjDescription* pParamValue;
	CXMLNode* pParNode;
	for (int i=0; i <= GetParameters().GetUpperBound(); i++)
	{
		pParNode = pParamsNode->CreateNewChild(XML_PARAMETER_TAG);
		pParamValue = GetParamDescription(i);	
		pParamValue->SetDefaultPassedByOut(FALSE);
		pParamValue->Unparse (pParNode);
	}
}

//----------------------------------------------------------------------------
const BOOL CFunctionDescription::IsContextFunction()
{
	if (GetParameters().GetCount() == 0)
		return FALSE;
	CDataObjDescription* pFirstParam = GetParamDescription(0);
	return (pFirstParam->GetDataType() == DataType::Object && pFirstParam->GetName() == _T("handle"));
}

//---------------------------------------------------------------------------------------
CString CFunctionDescription::GetUrl()
{ 
	// cotruisco la Url di chiamata sintassi (server:port/service)
	CString sUrl;

	if (m_strServer.Find(_T("http://")) != 0 && m_strServer.Find(_T("https://")) != 0)
		sUrl = _T("http://");
	
	sUrl += m_strServer;
	
	sUrl.TrimRight(_T('/'));
	sUrl.TrimRight(_T('\\'));
	
	if (m_nPort)
	{
		CString sPort;
		sPort.Format(_T(":%d"), m_nPort);
		sUrl += sPort;
	}
	
	sUrl += _T("/") + m_strService;
		
	return sUrl;
}

//---------------------------------------------------------------------------------------
CString	 CFunctionDescription::GetHelpSignature()
{
	CString strHelp;
	strHelp = GetReturnValueDescription().ToString(GetReturnValueDescription().GetDataType());
	strHelp += _T(" ");
	strHelp += GetName();
	strHelp += _T(" (\r\n\t");
	for (int i = 0; i < GetParameters().GetSize(); i++)
	{
		CDataObjDescription* pPar = GetParamDescription(i);
		if (pPar)
		{
			/*if (pPar->IsPassedModeIn())
				strHelp += _T("in ");
			else*/ if (pPar->IsPassedModeOut())
				strHelp += _T("[out] ");
			else if (pPar->IsPassedModeInOut())
				strHelp += _T("[in out] ");

			strHelp += pPar->ToString(pPar->GetDataType());

			strHelp += _T(" ") + pPar->GetName();

			if (pPar->IsOptional())
			{
				strHelp += _T(" [optional");
				if (pPar->GetValue() && !pPar->GetValue()->IsEmpty())
				{
					strHelp += L" ="  + pPar->GetValue()->ToString();
				}
				strHelp += _T("]");
			}

			if (!pPar->GetTitle().IsEmpty())
				strHelp +=  _T(" /*") + pPar->GetTitle() + _T("*/");

			if (i < (GetParameters().GetSize() - 1))
				strHelp += _T(",\r\n\t");
		}
	}
	strHelp += _T(")\r\n");

	CString sT = GetTitle() + _T("\r\n") + strHelp;
	return (sT);
}

//----------------------------------------------------------------------------------------------
//	class CDecoratedFunctionDescription implementation
//----------------------------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CDecoratedFunctionDescription, CFunctionDescription)

//----------------------------------------------------------------------------------------------
CDecoratedFunctionDescription::CDecoratedFunctionDescription()
	:
	CFunctionDescription()
{}

//-----------------------------------------------------------------------------
CDecoratedFunctionDescription::CDecoratedFunctionDescription(const CDecoratedFunctionDescription& dd)
{
	Assign(dd);
}

//--------------------------------------------------------------------
BOOL CDecoratedFunctionDescription::operator== (const CDecoratedFunctionDescription& dd)
{
	return IsEqual(dd);
}

//--------------------------------------------------------------------
BOOL CDecoratedFunctionDescription::operator!= (const CDecoratedFunctionDescription& dd)
{
	return !IsEqual(dd);
}

//--------------------------------------------------------------------
BOOL CDecoratedFunctionDescription::IsEqual(const CDecoratedFunctionDescription& dd)
{
	return 	CBaseDescription::IsEqual(dd) 	&&
		m_sRemarks == dd.m_sRemarks			&&
		m_sExample == dd.m_sExample			&&
		m_sResult == dd.m_sResult			&&
		m_sPrototype == dd.m_sPrototype;
}

//----------------------------------------------------------------------------
CDecoratedFunctionDescription& CDecoratedFunctionDescription::Assign(const CDecoratedFunctionDescription& dd)
{
	CBaseDescription::Assign(dd);

	m_sRemarks 		= dd.m_sRemarks;
	m_sExample 		= dd.m_sExample;
	m_sResult 		= dd.m_sResult;
	m_sPrototype 	= dd.m_sPrototype;

	return *this;
}

//------------------------------------------------------------------------------
CDecoratedFunctionDescription::CDecoratedFunctionDescription(CTBNamespace::NSObjectType aNSType)
	:
	CFunctionDescription(aNSType)
{
}
