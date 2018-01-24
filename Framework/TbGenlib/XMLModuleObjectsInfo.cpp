
#include "stdafx.h"
#include <io.h>

#include <TbXmlCore\XMLTags.h>
#include <TbXmlCore\XMLDocObj.h>
#include <TbXmlCore\XMLSaxReader.h>

#include <TbNameSolver\FileSystemFunctions.h>
#include <TbNameSolver\PathFinder.h>
#include <TbNameSolver\IFileSystemManager.h>

#include <TbClientCore\ClientObjects.h>
#include <TbWebServicesWrappers\LoginManagerInterface.h>

#include <TBGeneric\EnumsTable.h>
#include <TbGeneric\SettingsTable.h>

#include <TbGeneric\DataObjDescription.h>
#include <TbGeneric\ReferenceObjectsInfo.h>
#include <TbGeneric\FunctionObjectsInfo.h>
#include <TbGeneric\OutdateObjectsInfo.h>
#include <TbGeneric\LocalizableObjs.h>

#include <TbParser\XmlFunctionObjectsParser.h>
#include <TbParser\XmlOutDateObjectsParser.h>
#include <TbParser\XmlSettingsParser.h>

#include "XmlReferenceObjectsParser.h"

#include "OslInfo.h"
#include "ParsObj.h"
#include "Messages.h"
#include "baseapp.h"
#include "funproto.h"

#include "XmlModuleObjectsInfo.h"
//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char THIS_FILE[] = __FILE__;
#endif

/////////////////////////////////////////////////////////////////////////////
// 				class CXMLVariable Implementation
/////////////////////////////////////////////////////////////////////////////
//
//----------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(CXMLVariable, CObject)

//----------------------------------------------------------------------------
CXMLVariable::CXMLVariable(const CString& strName, DataObj* pDataObj, BOOL bOwnsDataObj /*FALSE*/)
:
	m_strName		(strName),
	m_pDataObj		(pDataObj),
	m_nReference	(0),
	m_bOwnsDataObj	(bOwnsDataObj)
{
	if (m_strName.IsEmpty() || !m_pDataObj)
		ASSERT(FALSE);
}

//----------------------------------------------------------------------------
CXMLVariable::CXMLVariable(const CString& strName, DataObj& aDataObj, BOOL bOwnsDataObj /*FALSE*/)
	:
	m_strName		(strName),
	m_pDataObj		(NULL),
	m_nReference	(0),
	m_bOwnsDataObj	(bOwnsDataObj)
{
	m_pDataObj = &aDataObj;
	
	if (m_strName.IsEmpty() || !m_pDataObj)
		ASSERT(FALSE);
}

// serve nel caso stia semplicemente parsando-unparsando il file dei criteri di selezione
// nel caso di editor profili del form manager
//----------------------------------------------------------------------------
CXMLVariable::CXMLVariable(const CString& strName, const CString& strXMLValue)
:
	m_strName		(strName),
	m_strXMLValue	(strXMLValue),	
	m_pDataObj		(NULL),
	m_nReference	(0),
	m_bOwnsDataObj	(FALSE)
{
	ASSERT(m_strName.IsEmpty());
}

//----------------------------------------------------------------------------
CXMLVariable::CXMLVariable(const CXMLVariable& aXMLVariable)
{
	Assign(aXMLVariable);
}

//----------------------------------------------------------------------------
CXMLVariable::~CXMLVariable()
{
	SetDataObj(NULL);
}

//----------------------------------------------------------------------------
void CXMLVariable::SetDataObj(DataObj* pDataObj) 
{
	if (m_pDataObj && m_bOwnsDataObj)
	{
		delete m_pDataObj;
	}
	m_pDataObj = pDataObj; 
}

//----------------------------------------------------------------------------
void CXMLVariable::BindExternalDataObj (DataObj* pDataObj)
{
	if (!pDataObj)
		return;

	if (m_pDataObj && m_bOwnsDataObj && m_nReference <= 1)
		SAFE_DELETE(m_pDataObj);

	// no previously dataobj
	if (!m_pDataObj)
	{
		m_pDataObj = pDataObj;
		m_bOwnsDataObj = FALSE;
		return;
	}

	ASSERT(FALSE);
	TRACE1("CXMLVariable::BindExternalDataObj of variable %s cannot bind pDataObj as the existing one is not owned or already referenced!", m_strName);
}

//----------------------------------------------------------------------------
void CXMLVariable::UnParse(CXMLNode* pNode)
{
	if (!pNode)
		return;

	CXMLNode* pValue;
	if (m_pDataObj->GetDataType() == DataType::Array)
	{
		DataArray* pAr = (DataArray*)m_pDataObj;
		for (int i = 0; i < pAr->GetSize(); i++)
		{
			pValue = pNode->CreateNewChild(XML_VALUE_TAG);
			pValue->SetText((LPCTSTR)(pAr->GetAt(i)->FormatDataForXML()));
		}
	}
	else
		pNode->SetText(GetDataObjValue());
}

//----------------------------------------------------------------------------
void CXMLVariable::Parse(CXMLNode* pNode)
{
	if (!pNode)
		return;

	if (m_pDataObj->GetDataType() == DataType::Array)
	{
		DataArray* pDataArray = (DataArray*)m_pDataObj;
		DataType aBaseType = pDataArray->GetBaseDataType();
		DataObj* pDataValue = NULL;
		CXMLNodeChildsList* pValues = pNode->GetChilds();
		if (!pValues)
			return;

		CString sTemp;
		for (int i = 0; i <= pValues->GetUpperBound(); i++)
		{
			if (
				!pValues->GetAt(i)->GetName(sTemp) ||
				_tcsicmp(sTemp, XML_VALUE_TAG)
				)
				continue;

			pDataValue = DataObj::DataObjCreate(aBaseType);
			pValues->GetAt(i)->GetText(sTemp);
			if (!sTemp.IsEmpty())
			{
				pDataValue->AssignFromXMLString(sTemp);
				pDataArray->Add(pDataValue);
			}
		}
	}
	else
	{
		CString strValue;
		pNode->GetText(strValue);
		SetDataObjValue(strValue);
	}
}

//----------------------------------------------------------------------------
void CXMLVariable::Assign(const CXMLVariable& aXMLVariable)
{
	if (this == &aXMLVariable)
		return;
	
	m_strName		= aXMLVariable.m_strName;	
	m_pDataObj		= aXMLVariable.m_pDataObj;
	m_strXMLValue	= aXMLVariable.m_strXMLValue;
	m_bOwnsDataObj	= aXMLVariable.m_bOwnsDataObj;
	m_nReference	= aXMLVariable.m_nReference;
}

//----------------------------------------------------------------------------
BOOL CXMLVariable::IsEqual(const CXMLVariable& aXMLVariable) const
{
	if (this == &aXMLVariable)
		return TRUE;
	
	return 
		(
			_tcsicmp(m_strName, aXMLVariable.m_strName) == 0 &&
			m_bOwnsDataObj == aXMLVariable.m_bOwnsDataObj &&
			(
				(m_pDataObj && m_pDataObj == aXMLVariable.m_pDataObj) ||
				(!m_pDataObj && _tcsicmp(m_strXMLValue, aXMLVariable.m_strXMLValue) == 0)
			)
		);
}

//----------------------------------------------------------------------------
CXMLVariable&	CXMLVariable::operator = (const CXMLVariable& aXMLVariable)
{
	if (this == &aXMLVariable)
		return *this;

	Assign(aXMLVariable);
	return *this;
}

//////////////////////////////////////////////////////////////////////////////
//					CXMLVariableArray Implementation
//////////////////////////////////////////////////////////////////////////////
//
IMPLEMENT_DYNCREATE (CXMLVariableArray, Array)

//----------------------------------------------------------------------------
CXMLVariableArray::~CXMLVariableArray()
{
	RemoveAll();
	if (m_pBindingData)
		delete m_pBindingData;
}



//----------------------------------------------------------------------------
int	CXMLVariableArray::GetVariable(const CString& strName)
{
	for (int i = 0; i <= GetUpperBound(); i++)
	{
		if (GetAt(i) && _tcsicmp(GetAt(i)->m_strName, strName) == 0)
			return i;	
	}

	return -1;
}

//----------------------------------------------------------------------------
CXMLVariable* CXMLVariableArray::GetVariableByName(const CString& strName)
{
	for (int i = 0; i <= GetUpperBound(); i++)
	{
		if (GetAt(i) && _tcsicmp(GetAt(i)->m_strName, strName) == 0)
			return (CXMLVariable*)GetAt(i);
	}

	return NULL;
}

//----------------------------------------------------------------------------
BOOL CXMLVariableArray::ExistVariable(const CString& strName)
{
	return GetVariable(strName) > -1;
}

//----------------------------------------------------------------------------
int	CXMLVariableArray::Add(const CString& strName, DataObj* pDataObj)
{
	if (ExistVariable(strName))
	{
		//TODO Marcolino mettere su ids
		ASSERT(FALSE);
		TRACE1("CXMLVariableArray::Add : %s variable already inserted", (LPCTSTR)strName);
		return -1;
	}
	return Array::Add(new CXMLVariable(strName, pDataObj));
}

//----------------------------------------------------------------------------
int	CXMLVariableArray::Add(const CString& strName, DataObj& aDataObj)
{
	if (ExistVariable(strName))
	{
		//TODO Marcolino mettere su ids
		ASSERT(FALSE);
		TRACE1("CXMLVariableArray::Add %s variable already inserted", (LPCTSTR)strName);
		return -1;
	}

	return Array::Add(new CXMLVariable(strName, aDataObj));
}


//----------------------------------------------------------------------------
int	CXMLVariableArray::Add(const CString& strName, const CString& strXMLValue)
{
	if (ExistVariable(strName))
	{
		ASSERT(FALSE);
		TRACE1("CXMLVariableArray::Add %s variable already inserted", (LPCTSTR)strName);
		return -1;
	}

	return Array::Add(new CXMLVariable(strName, strXMLValue));
}

//aggiunge le variabili di un altra CXMLBaseAppCriteria se non presenti
//----------------------------------------------------------------------------
int	CXMLVariableArray::AddVariables(CXMLVariableArray* pXMLVariableArray)
{
	int nAdd = 0;
	CXMLVariable* pVarToAdd = NULL;

	if(!pXMLVariableArray)
		return nAdd;
	
	for(int n = 0 ; n < pXMLVariableArray->GetSize() ; n++)
	{
		pVarToAdd = pXMLVariableArray->GetAt(n);
		for(int i = 0 ; i < GetSize() ; i++)
		{
			if(pVarToAdd == GetAt(i))
			{
				pVarToAdd = NULL;
				break;
			}
		}

		if(pVarToAdd)
		{
			nAdd++;
			(pVarToAdd->m_nReference)++;
			Add(pVarToAdd);
		}
	}

	return nAdd;
}

//appende la variabili ad un nodo esistente
//----------------------------------------------------------------------------
void CXMLVariableArray::UnParse(CXMLNode* pNode)
{
	CXMLVariable* pVariable = NULL;
	CXMLNode* pVarNode = NULL;
	for (int nIdx = 0; nIdx < GetSize(); nIdx++)
	{
		pVariable = GetAt(nIdx);
		if (pVariable && !pVariable->GetName().IsEmpty())
		{
			pVarNode = pNode->CreateNewChild(pVariable->GetName());
			pVariable->UnParse(pVarNode);
		}
	}
}

//crae un documento xml con l'elenco delle variabili e con il nodo avete attributo name = m_strName
//----------------------------------------------------------------------------
CString CXMLVariableArray::UnParseToXMLString()
{
	CXMLNode* pRoot;
	CXMLDocumentObject aDoc;
	pRoot = aDoc.CreateRoot(_T("XMLVariables"));

	UnParse(pRoot);
	CString strXML;
	aDoc.GetXML(strXML);
	aDoc.Close();
	return strXML;
}

//----------------------------------------------------------------------------
void CXMLVariableArray::ParseFromXMLString(const CString& xmlString)
{
	if (xmlString.IsEmpty())
		return;

	CXMLDocumentObject aDoc;
	aDoc.LoadXML(xmlString);

	CXMLNode*	pRoot = aDoc.GetRoot();
	Parse(pRoot);
	aDoc.Close();
}

//----------------------------------------------------------------------------
void CXMLVariableArray::Parse(CXMLNode* pNode)
{
	CXMLVariable* pVariable = NULL;
	CXMLNode* pVarNode = NULL;

	for (int nIdx = 0; nIdx < GetSize(); nIdx++)
	{
		pVariable = GetAt(nIdx);
		if (pVariable)
		{
			pVarNode = pNode->GetChildByName(pVariable->GetName());
			if (pVarNode)
				pVariable->Parse(pVarNode);
		}
	}
}


//----------------------------------------------------------------------------------------------
BOOL CXMLVariableArray::IsEqual(const CXMLVariableArray& aVarArray) const
{
	if (this == &aVarArray)
		return TRUE;
	
	if(aVarArray.GetSize() != GetSize())
		return FALSE;
	
	for(int i = 0 ; i < GetSize() ; i++)
	{
		if (!GetAt(i) && aVarArray.GetAt(i))
			return FALSE;
		
		if(!GetAt(i)->IsEqual(*aVarArray.GetAt(i)))
			return FALSE;
	}

	return TRUE;
}

//----------------------------------------------------------------------------
CXMLVariableArray& CXMLVariableArray::operator= (const CXMLVariableArray& aVarArray)
{
	if (this != &aVarArray)
	{
		RemoveAll();
		for (int i=0; i < aVarArray.GetSize(); i++)
			Add(new CXMLVariable(*aVarArray.GetAt(i)));
	}
	return *this;
}

//----------------------------------------------------------------------------
void CXMLVariableArray::RemoveAll()
{
	if (m_bOwnsElements)
	{
		int n = GetSize();
		CXMLVariable* pO;
		for (int i = 0; i < n; i++) 
			if (pO = GetAt(i)) 
			{
				ASSERT_VALID(pO);
				if(pO->m_nReference == 0)
					delete pO;
				else
					(pO->m_nReference)--;
			}
	}

	CObArray::RemoveAll();
}


//----------------------------------------------------------------------------------------------
//							CEnvelopeObjectsDescription
//----------------------------------------------------------------------------------------------
//
//-----------------------------
CEnvelopeObjectsDescription::CEnvelopeObjectsDescription ()
	:
	m_bLoaded (false)
{
}

//----------------------------------------------------------------------------------------------
BOOL CEnvelopeObjectsDescription::ReadFile (const CTBNamespace& aModuleNamespace, CPathFinder* pPathFinder)
{
	if (!pPathFinder || !aModuleNamespace.IsValid())
		return FALSE;

	CString sFileName = pPathFinder->GetEnvelopeObjectsFullName(aModuleNamespace);

	if (!ExistFile(sFileName))
		return FALSE;
	
	CString strTagVal;

	CLocalizableXMLDocument aXMLModDoc(aModuleNamespace, pPathFinder);
	aXMLModDoc.EnableMsgMode(FALSE);
	
	if(!aXMLModDoc.LoadXMLFile(sFileName))
	{
		ASSERT(FALSE);
		TRACE(_T("ModuleObjects: the file XML grammar is not correct. The declarations contained into the file cannot be loaded."));
		return FALSE;
	}
	
	CXMLNode* pRoot = aXMLModDoc.GetRoot();
	if (!pRoot) 
	{
		ASSERT(FALSE);
		TRACE(_T("ModuleObjects: The file root tag is missing. The declarations contained into the file cannot be loaded."));
		return FALSE;
	}
	
	CXMLNode* pNode = NULL;
	CXMLNode* pElemNode = NULL;
	
	m_arEnvClasses.RemoveAll();

	for (int i = 0; i < pRoot->GetChildsNum(); i++)
	{
		pElemNode = pRoot->GetChildAt(i);
		if (pElemNode)
		{
			pElemNode->GetText(strTagVal);	
			if (!strTagVal.IsEmpty())
				m_arEnvClasses.Add(strTagVal);
		}		
	}
	SetLoaded(true);

	return TRUE;
}

//----------------------------------------------------------------------------------------------
BOOL CEnvelopeObjectsDescription::SaveFile(const CTBNamespace& aModuleNamespace, CPathFinder* pPathFinder)
{
	CXMLDocumentObject aXMLModDoc(TRUE);
			
	CXMLNode* pRoot = aXMLModDoc.CreateRoot(XML_MODULEOBJECTS_TAG);
	if(!pRoot)
	{
		ASSERT(FALSE);
		return FALSE;
	}

	CXMLNode* pNode	= NULL;
	CXMLNode* pLeafNode = NULL;

	
	if ((pNode = pRoot->CreateNewChild(XML_ENVELOPE_CLASSES_TAG)) != NULL)
	{
		for (int i = 0; i < m_arEnvClasses.GetSize(); i++)
		{
			CString strEnvClass = m_arEnvClasses.GetAt(i);
			if (!strEnvClass.IsEmpty())
			{
				pLeafNode = pNode->CreateNewChild(XML_ENVELOPE_CLASS_TAG);
				if (pLeafNode)
					pLeafNode->SetText((LPCTSTR)strEnvClass);					
			}
		}
	}

	CString sFilename = pPathFinder->GetEnvelopeObjectsFullName(aModuleNamespace);
	
	return aXMLModDoc.SaveXMLFile(sFilename, TRUE);
}

///////////////////////////////////////////////////////////////////////////////
// 						CModuleDescription:
///////////////////////////////////////////////////////////////////////////////
//
//-----------------------------------------------------------------------------
CModuleDescription::CModuleDescription ()
{
}

//-----------------------------------------------------------------------------
const COutDateObjectsDescription& CModuleDescription::GetOutDateObjectsInfo()
{
	return m_OutDatesInfo;
}

//-----------------------------------------------------------------------------
const CFunctionObjectsDescription& CModuleDescription::GetFunctionsInfo ()
{
	// non è caricato
	if (!m_FunctionsInfo.IsLoaded())
	{
		TB_LOCK_FOR_WRITE();	
		if (!m_FunctionsInfo.IsLoaded()) //make a new check, meanwhile it may have been loaded
			LoadFunctionsObjects();
	}
	
	return m_FunctionsInfo;
}

//-----------------------------------------------------------------------------
const CFunctionObjectsDescription& CModuleDescription::GetEventHandlersInfo ()
{
	// non è caricato
	if (!m_EventHandlersInfo.IsLoaded())
	{
		TB_LOCK_FOR_WRITE();	
		if (!m_EventHandlersInfo.IsLoaded()) //make a new check, meanwhile it may have been loaded
			LoadEventHandlerObjects();
	}
	
	return m_EventHandlersInfo;
}

//-----------------------------------------------------------------------------
const CReferenceObjectsDescription& CModuleDescription::GetReferencesInfo ()
{
	// non è caricato
	if (!m_ReferencesInfo.IsLoaded())
	{
		TB_LOCK_FOR_WRITE();	
		if (!m_ReferencesInfo.IsLoaded()) //make a new check, meanwhile it may have been loaded
			LoadReferenceObjects();
	}

	return m_ReferencesInfo;
}

//-----------------------------------------------------------------------------
const CEnvelopeObjectsDescription& CModuleDescription::GetEnvelopeInfo ()
{
	// non è caricato
	if (!m_EnvelopeInfo.IsLoaded())
	{
		TB_LOCK_FOR_WRITE();	
		if (!m_EnvelopeInfo.IsLoaded()) //make a new check, meanwhile it may have been loaded
			m_EnvelopeInfo.ReadFile(m_Info.GetNamespace(), AfxGetPathFinder());
	}

	return m_EnvelopeInfo;
}

//-----------------------------------------------------------------------------
CString CModuleDescription::GetWebMethodsModifyDate()
{
	CString sFile = AfxGetPathFinder()->GetWebMethodsFullName(m_Info.GetNamespace());
	if (!ExistFile(sFile))
		return _T("");
	CFileStatus	fileStatus;
	if (::GetStatus(sFile, fileStatus))
		return fileStatus.m_mtime.Format(_T("%Y-%m-%d%H-%M-%S"));

	return _T("");

}

//-----------------------------------------------------------------------------
/*static*/BOOL CModuleDescription::LoadFunctionsObjects 
							(
								const CString& sPath,
								CFunctionObjectsDescription& arDescription,
								const CTBNamespace& ns,
								BOOL bSkipDuplicate /*= FALSE*/
							)
{
	CXMLDocumentObject aXMLModDoc;
	aXMLModDoc.EnableMsgMode(FALSE);

	if (IsGoodFile(sPath, XML_FUNCTIONOBJ_TAG, &aXMLModDoc))
	{
		CXMLFunctionObjectsParser aFunParser;
		aFunParser.Parse (&aXMLModDoc, &arDescription, ns, bSkipDuplicate);

		CMapFunctionDescription* pMap = &(const_cast<AddOnAppsArray*>(AfxGetAddOnAppsTable())->GetMapWebClass());
		for (int i = 0 ; i < arDescription.GetFunctions().GetSize(); i++)
		{
			CFunctionDescription* pF = (CFunctionDescription*)(arDescription.GetFunctions().GetAt(i));
			if (!pF->IsThisCallMethods())
				continue;
			CString sClassType = pF->GetClassType();
			if (sClassType.IsEmpty())
				continue;
			int idx = sClassType.Find(',');
			if (idx > 0)
				sClassType = sClassType.Left(idx);
			sClassType.Trim();
			CFunctionDescriptionArray* par = NULL;
			if (!pMap->Lookup(sClassType, (CObject*&)par))
			{
				par = new CFunctionDescriptionArray();
				par->m_sClassName = sClassType;
				pMap->SetAt(sClassType, par);
			}
			par->Add(pF);
		}
	}
	return TRUE;
}

//-----------------------------------------------------------------------------
void CModuleDescription::LoadFunctionsObjects ()
{
	//TODO aggiungere load incrementali con override da custom ...
	CString sPath = AfxGetPathFinder()->GetWebMethodsFullName(m_Info.GetNamespace());
	LoadFunctionsObjects(sPath, m_FunctionsInfo, m_Info.GetNamespace());
	m_FunctionsInfo.SetLoaded(true);
}



//-----------------------------------------------------------------------------
void CModuleDescription::LoadEventHandlerObjects ()
{
	CXMLDocumentObject aXMLModDoc;
	aXMLModDoc.EnableMsgMode(FALSE);

	if (IsGoodFile(AfxGetPathFinder()->GetEventHandlerObjectsFullName(m_Info.GetNamespace()), XML_FUNCTIONOBJ_TAG, &aXMLModDoc))
	{
		CXMLFunctionObjectsParser aParser;
		aParser.SetFunctionType(CTBNamespace::EVENTHANDLER);
		aParser.Parse (&aXMLModDoc, &m_EventHandlersInfo, m_Info.GetNamespace(), FALSE);
	}

	m_EventHandlersInfo.SetLoaded(true);
}

//-----------------------------------------------------------------------------
/*static */CXMLReferenceObjectsParserBase* CModuleDescription::s_pReferenceObjectsParser = NULL;

//-----------------------------------------------------------------------------
void CModuleDescription::LoadReferenceObjects()
{
	LoadReferenceObjects(CPathFinder::STANDARD);
	LoadReferenceObjects(CPathFinder::CUSTOM);
	m_ReferencesInfo.SetLoaded(true);
}

//-----------------------------------------------------------------------------
void CModuleDescription::LoadReferenceObjects(CPathFinder::PosType pos, CPathFinder::Company company /*CPathFinder::CURRENT*/)
{
	CLocalizableXMLDocument aXMLModDoc(m_Info.GetNamespace(), AfxGetPathFinder());
	aXMLModDoc.EnableMsgMode(FALSE);

	CString sPath = AfxGetPathFinder()->GetModuleReferenceObjectsPath (m_Info.GetNamespace(), pos, _T(""), FALSE, company); 
	
	CStringArray aFiles;
	// leggo la directory
	AfxGetFileSystemManager()->GetFiles (sPath, AfxGetPathFinder()->GetModuleReferenceObjectsSearch(), &aFiles);

	// per tutti i files letti carico i parametri, non ho motivo
	// particolare di invalidare il loading dei moduli
	CXMLReferenceObjectsParserBase* pParser = CModuleDescription::s_pReferenceObjectsParser;
	ASSERT(pParser);
	for (int i=0; i <= aFiles.GetUpperBound(); i++)
	{
		if	(!IsGoodFile(aFiles.GetAt(i), XML_HOTKEYLINK_TAG, &aXMLModDoc))
			continue;

		pParser->Parse (&aXMLModDoc, &m_ReferencesInfo, m_Info.GetNamespace());
	}
}

//-----------------------------------------------------------------------------
const CStringArray* CModuleDescription::GetAvailableDocEnvClasses (const CTBNamespace& aDocNS)
{
	if (!AfxGetDocumentDescription(aDocNS)) 
		return NULL;

	const CStringArray *parClasses = &GetEnvelopeInfo().m_arEnvClasses;
	CString defEnvClass = aDocNS.GetObjectName(CTBNamespace::MODULE);

	if (defEnvClass.IsEmpty())
		return parClasses;

	for (int i = 0; i < parClasses->GetSize(); i++)
		if (parClasses->GetAt(i) == defEnvClass)
			return parClasses;

	TB_LOCK_FOR_WRITE();
	(const_cast<CStringArray *>(parClasses))->Add(defEnvClass);

	return parClasses;
}

//-----------------------------------------------------------------------------
CFunctionDescription* CModuleDescription::GetParamObjectInfo (const CTBNamespace& aNamespace)
{
	if (!aNamespace.IsValid())
		return NULL;

	if (aNamespace.GetType() == CTBNamespace::FUNCTION)
		return GetFunctionsInfo().GetFunctionInfo(aNamespace);
	else if (aNamespace.GetType() == CTBNamespace::EVENTHANDLER)
		return GetEventHandlersInfo().GetFunctionInfo(aNamespace);
	else if (aNamespace.GetType() == CTBNamespace::HOTLINK)
		return GetReferencesInfo().GetHotlinkInfo (aNamespace);

	return NULL;
}

//-----------------------------------------------------------------------------
BOOL CModuleDescription::IsOutDated (const CTBNamespace& aNamespace, const int& nRelease)
{
	ASSERT (aNamespace.GetType() == CTBNamespace::REPORT);

	COutDateObjectDescription* pInfo = m_OutDatesInfo.GetOutDateObjectInfo(aNamespace);
	if (!pInfo)
		return FALSE;

	return pInfo->IsOutDate(nRelease);
}

//----------------------------------------------------------------------------------------------
/*static*/BOOL CModuleDescription::IsGoodFile		(
											const CString& sFileName, 
											const CString sTagName, 
											CXMLDocumentObject* pDoc
										)
{
	if (!pDoc || sTagName.IsEmpty() || sFileName.IsEmpty() || !ExistFile(sFileName))
		return FALSE;
	
	CString strTagVal;

	if (!pDoc->LoadXMLFile(sFileName))
	{		
		AfxGetDiagnostic()->Add (cwsprintf(_TB(" XML description {0-%s} cannot be read. File not loaded."), (LPCTSTR) sFileName), CDiagnostic::Warning);		
		return FALSE;
	}
	
	CXMLNode* pRoot = pDoc->GetRoot();
	CString sRootName;
	if (!pRoot || !pRoot->GetName(sRootName) || (sRootName != sTagName)) 
	{
		AfxGetDiagnostic()->Add (cwsprintf(_TB(" XML description {0-%s}: the file has no root element. File not loaded."), (LPCTSTR) sFileName), CDiagnostic::Warning);
		return FALSE;
	}
	return TRUE;
}



