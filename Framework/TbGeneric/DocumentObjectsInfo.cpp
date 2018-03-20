#include "stdafx.h"

#include <io.h>
#include <TBNameSolver\ApplicationContext.h>
#include <TBNameSolver\LoginContext.h>
#include <TBNameSolver\Diagnostic.h>
#include <TBNameSolver\FileSystemFunctions.h>

#include "SettingsTable.h"
#include "DocumentObjectsInfo.h"

//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char THIS_FILE[] = __FILE__;
#endif

static const TCHAR szDefaultBaseParents[] = _T("CBaseDocument");
static const TCHAR szHierarchySeparator[] = _T(".");
const TCHAR szDefaultFamilyParents[] = _T("CAbstractDoc.CAbstractFormDoc.");

//------------------------------------------------------------------------------
const CServerDocDescriArray* AFXAPI AfxGetClientDocsTable()
{
	return AfxGetApplicationContext()->GetObject< const CServerDocDescriArray>(&CApplicationContext::GetClientDocsTable);
}

//----------------------------------------------------------------------------------------------
CClientFormDescription* CServerFormDescription::AddClient(const CString& sName, bool bExclude, const CTBNamespace& nsModule)
{
	for (int i = 0; i < m_arClientForms.GetSize(); i++)
	{
		CClientFormDescription* pDescri = m_arClientForms[i];
		if (pDescri->m_sName == sName && pDescri->m_bExclude == bExclude)
		{
			pDescri->m_Module = nsModule;
			return pDescri;
		}
	}
	CClientFormDescription* pDescri = new CClientFormDescription;
	pDescri->m_pServer = this;
	pDescri->m_sName = sName;
	pDescri->m_bExclude = bExclude;
	pDescri->m_Module = nsModule;
	m_arClientForms.Add(pDescri);
	return pDescri;
}


//----------------------------------------------------------------------------------------------
BOOL CServerFormDescription::RemoveClient(const CString& sName, BOOL bPersistOnFileSystem)
{
	for (int i = 0; i < m_arClientForms.GetSize(); i++)
	{
		CClientFormDescription* pDescri = m_arClientForms[i];
		if (pDescri->m_sName == sName)
		{
			if (bPersistOnFileSystem && !pDescri->RemoveFromFileSystem())
				return FALSE;
			m_arClientForms.RemoveAt(i);

			return TRUE;
		}
	}
	return TRUE;
}
//----------------------------------------------------------------------------------------------
BOOL CClientFormDescription::RemoveFromFileSystem()
{
	CString sFile = AfxGetPathFinder()->GetClientDocumentObjectsFullName(m_Module);
	CXMLDocumentObject aDoc;
	if (!ExistFile(sFile))
		return TRUE;
	if (!aDoc.LoadXMLFile(sFile))
		return FALSE;
	CXMLNode* pRoot = aDoc.GetRoot();
	if (!pRoot)
		return TRUE;
	CXMLNode* pNode = pRoot->GetChildByName(XML_CLIENTFORMS_TAG);
	if (!pNode)
		return TRUE;
	CXMLNode* pFormNode = pNode->GetChildByAttributeValue(XML_CLIENTFORM_TAG, XML_NAME_ATTRIBUTE, m_sName);
	if (!pFormNode)
		return TRUE;
	if (!pNode->RemoveChild(pFormNode))
		return FALSE;
	return aDoc.SaveXMLFile(sFile, TRUE);
}
//----------------------------------------------------------------------------------------------
BOOL CClientFormDescription::PersistOnFileSystem()
{
	CString sFile = AfxGetPathFinder()->GetClientDocumentObjectsFullName(m_Module);
	CXMLDocumentObject aDoc;
	if (ExistFile(sFile))
	{
		if (!aDoc.LoadXMLFile(sFile))
			return FALSE;
	}
	else
	{
		if (!aDoc.Initialize())
			return FALSE;
	}
	if (!aDoc.GetRoot())
	{
		CXMLNode* pRoot = aDoc.CreateRoot(XML_CDDOCUMENTOBJECTS_TAG);
		aDoc.CreateElement(XML_CLIENTDOCS_TAG, pRoot);
	}
	CXMLNode* pNode = aDoc.GetRoot()->GetChildByName(XML_CLIENTFORMS_TAG);
	if (!pNode)
	{
		pNode = aDoc.CreateElement(XML_CLIENTFORMS_TAG, aDoc.GetRoot());
	}
	CXMLNode* pFormNode = pNode->GetChildByAttributeValue(XML_CLIENTFORM_TAG, XML_NAME_ATTRIBUTE, m_sName);
	if (!pFormNode)
	{
		pFormNode = aDoc.CreateElement(XML_CLIENTFORM_TAG, pNode);
	}
	CString s;
	if ((pFormNode->GetAttribute(XML_NAME_ATTRIBUTE, s) && s == m_sName) && (pFormNode->GetAttribute(XML_SERVER_ATTRIBUTE, s) && s == m_pServer->m_sName))
		return TRUE;
	pFormNode->SetAttribute(XML_NAME_ATTRIBUTE, m_sName);
	pFormNode->SetAttribute(XML_SERVER_ATTRIBUTE, m_pServer->m_sName);
	return aDoc.SaveXMLFile(sFile, TRUE);
}

////////////////////////////////////////////////////////////////////////////////
//									CDocUIObjectsCache
////////////////////////////////////////////////////////////////////////////////
//
//------------------------------------------------------------------------------
CDocUIObjectsCache::CDocUIObjectsCache()
{
}

//-----------------------------------------------------------------------------
CObject* CDocUIObjectsCache::GetCachedObject(const CTBNamespace& aNamespace)
{
	CObject* pValue = NULL;
	CString sKey = aNamespace.ToString();
	Lookup(sKey.MakeLower(), pValue);

	return pValue;
}

//-----------------------------------------------------------------------------
void CDocUIObjectsCache::AddCachedObject(const CTBNamespace& aNamespace, CObject* pObject)
{
	if (GetCachedObject(aNamespace))
	{
		ASSERT_TRACE1(FALSE, "namespace %s already cached. Substituting object.", aNamespace.ToString());
	}
	CString sKey = aNamespace.ToString();
	SetAt(sKey.MakeLower(), pObject);
}


//=============================================================================        
//			class CViewModeDescription implementation
//=============================================================================        
IMPLEMENT_DYNCREATE(CViewModeDescription, CBaseDescription)

//----------------------------------------------------------------------------------------------
CViewModeDescription::CViewModeDescription()
	:
	CBaseDescription(CTBNamespace::FORM),
	m_Type(VMT_DATAENTRY),
	m_bSchedulable(TRUE),
	m_bNoWeb(FALSE)
{
}

//----------------------------------------------------------------------------------------------
void CViewModeDescription::SetType(const ViewModeType aType)
{
	m_Type = aType;
}

//----------------------------------------------------------------------------------------------
void CViewModeDescription::SetSchedulable(const BOOL& bSchedulable)
{
	m_bSchedulable = bSchedulable;
}

//----------------------------------------------------------------------------------------------
void CViewModeDescription::SetNoWeb(const BOOL bNoWeb)
{
	m_bNoWeb = bNoWeb;
}

//----------------------------------------------------------------------------------------------
void CViewModeDescription::SetFrameID(CString sFrameID)
{
	m_sFrameID = sFrameID;
}

//----------------------------------------------------------------------------------------------
CViewModeDescription* CViewModeDescription::Clone()
{
	CViewModeDescription* pNewDescri = new CViewModeDescription();
	pNewDescri->Assign(*this);
	return pNewDescri;
}

//----------------------------------------------------------------------------------------------
void CViewModeDescription::Assign(const CViewModeDescription& vd)
{
	CBaseDescription::Assign(vd);

	m_Type = vd.m_Type;
	m_bSchedulable = vd.m_bSchedulable;
	m_sFrameID = vd.m_sFrameID;
}

//=============================================================================        
//	class CDocumentPartDescription implementation
//=============================================================================        
IMPLEMENT_DYNAMIC(CDocumentPartDescription, CBaseDescription)

//----------------------------------------------------------------------------------------------
CDocumentPartDescription::CDocumentPartDescription()
{
}

//----------------------------------------------------------------------------------------------
void CDocumentPartDescription::SetClass(const CString& sClass)
{
	m_sClass = sClass;
}

//----------------------------------------------------------------------------------------------
void CDocumentPartDescription::SetLoadingMode(const LoadingMode aLoadingMode)
{
	m_LoadingMode = aLoadingMode;
}

//----------------------------------------------------------------------------------------------
void CDocumentPartDescription::AddViewMode(CViewModeDescription* pDescri)
{
	m_arViewModes.Add(pDescri);
}

//----------------------------------------------------------------------------------------------
CViewModeDescription* CDocumentPartDescription::GetViewMode(const CTBNamespace& aNamespace) const
{
	return (CViewModeDescription*)m_arViewModes.GetInfo(aNamespace);
}

//----------------------------------------------------------------------------------------------
CViewModeDescription* CDocumentPartDescription::GetViewMode(const CString& sViewMode) const
{
	return (CViewModeDescription*)m_arViewModes.GetInfo(sViewMode);
}

//----------------------------------------------------------------------------------------------
CBaseDescriptionArray& CDocumentPartDescription::GetViewModes()
{
	return m_arViewModes;
}
//----------------------------------------------------------------------------------------------
CViewModeDescription* CDocumentPartDescription::GetFirstViewMode() const
{
	return m_arViewModes.GetSize() > 0 ? (CViewModeDescription*)m_arViewModes.GetAt(0) : NULL;
}

//=============================================================================        
//	class CDocumentDescription implementation
//=============================================================================        
IMPLEMENT_DYNCREATE(CDocumentDescription, CBaseDescription)

//----------------------------------------------------------------------------------------------
CDocumentDescription::CDocumentDescription()
	:
	m_bExcludeFromFamily(FALSE),
	CBaseDescription(CTBNamespace::DOCUMENT),
	m_bTransferDisabled(FALSE),
	m_bExcludeFromExtRef(FALSE),
	m_bDynamic(FALSE),
	m_bLiveInStandard(FALSE),
	m_bRunnableAlone(TRUE),
	m_pTemplateNamespace(NULL),
	m_bDesignable(TRUE)

{
	m_arDescriptionKeys = new CStringArray();
}

//----------------------------------------------------------------------------------------------
CDocumentDescription::CDocumentDescription(const CTBNamespace& aNamespace, const CString& sTitle)
	:
	m_bExcludeFromFamily(FALSE),
	CBaseDescription(CTBNamespace::DOCUMENT),
	m_bTransferDisabled(FALSE),
	m_bExcludeFromExtRef(FALSE),
	m_bDynamic(FALSE),
	m_bLiveInStandard(FALSE),
	m_bRunnableAlone(TRUE),
	m_pTemplateNamespace(NULL),
	m_bDesignable(TRUE)
{
	m_Namespace = aNamespace;
	m_sNotLocalizedTitle = sTitle;
	m_arDescriptionKeys = new CStringArray();
	m_sNotLocalizedCaption = _T("");
}


//----------------------------------------------------------------------------------------------
CDocumentDescription::~CDocumentDescription()
{
	delete m_pTemplateNamespace;
	delete m_arDescriptionKeys;
}

//----------------------------------------------------------------------------------------------
CString CDocumentDescription::GetClassHierarchy() const
{
	return GetClassHierarchy(m_sClassHierarchy);
}

// la classe di gerarchia di default è quella dei documenti gestionali. E' possibile comunque
// definirne una personalizzata ripartendo da quelle definite come szDefaultBaseParents
//----------------------------------------------------------------------------------------------
CString CDocumentDescription::GetClassHierarchy(const CString& sClassName)
{
	CString sBase(szDefaultBaseParents);

	if (_tcsicmp(sClassName.Left(sBase.GetLength()), sBase) == 0)
		return sClassName;

	return CString(szDefaultBaseParents) + szHierarchySeparator + szDefaultFamilyParents + sClassName;
}

//----------------------------------------------------------------------------------------------
const CString CDocumentDescription::GetTitle() const
{
	return AfxLoadXMLString
	(
		m_sNotLocalizedTitle,
		szDocumentObjects,
		AfxGetDictionaryPathFromNamespace(m_Namespace, TRUE)
	);
}

//----------------------------------------------------------------------------------------------
const CString CDocumentDescription::GetCaption() const
{
	return AfxLoadXMLString
	(
		m_sNotLocalizedCaption,
		szDocumentObjects,
		AfxGetDictionaryPathFromNamespace(m_Namespace, TRUE)
	);
}


//----------------------------------------------------------------------------------------------
BOOL CDocumentDescription::IsMyClass(const CString& sClassName, BOOL bExactMatch /*FALSE*/) const
{
	CString sMyClass = GetClassHierarchy();
	if (_tcsicmp(sClassName, sMyClass) == 0)
		return TRUE;

	CString sQualifiedClass = GetClassHierarchy(sClassName);
	if (bExactMatch)
		return _tcsicmp(sMyClass, sQualifiedClass) == 0;

	return sMyClass.Find(sQualifiedClass) >= 0;
}

//----------------------------------------------------------------------------------------------
CViewModeDescription* CDocumentDescription::GetViewMode(const CTBNamespace& aNamespace) const
{
	return (CViewModeDescription*)m_arViewModes.GetInfo(aNamespace);
}

//----------------------------------------------------------------------------------------------
CViewModeDescription* CDocumentDescription::GetViewMode(const CString& sViewMode) const
{
	return (CViewModeDescription*)m_arViewModes.GetInfo(sViewMode);
}

//----------------------------------------------------------------------------------------------
CBaseDescriptionArray& CDocumentDescription::GetViewModes()
{
	return m_arViewModes;
}

//----------------------------------------------------------------------------------------------
CViewModeDescription* CDocumentDescription::GetFirstViewMode() const
{
	return m_arViewModes.GetSize() > 0 ? (CViewModeDescription*)m_arViewModes.GetAt(0) : NULL;
}

//----------------------------------------------------------------------------------------------
void CDocumentDescription::SetClassHierarchy(const CString& sClass)
{
	m_sClassHierarchy = sClass;
}

//----------------------------------------------------------------------------------------------
void CDocumentDescription::SetInterfaceClass(const CString& sClass)
{
	m_sInterfaceClass = sClass;
}


//----------------------------------------------------------------------------------------------
void CDocumentDescription::AddViewMode(CViewModeDescription* pDescri)
{
	m_arViewModes.Add(pDescri);
}

//----------------------------------------------------------------------------------------------
void CDocumentDescription::Assign(const CDocumentDescription& dd)
{
	__super::Assign(dd);

	m_sClassHierarchy = dd.m_sClassHierarchy;
	m_sInterfaceClass = dd.m_sInterfaceClass;
	m_bExcludeFromFamily = dd.m_bExcludeFromFamily;
	m_arViewModes = dd.m_arViewModes;
	m_bLiveInStandard = dd.m_bLiveInStandard;
	m_bRunnableAlone = dd.m_bRunnableAlone;
	m_bDynamic = dd.m_bDynamic;
	m_bTransferDisabled = dd.m_bTransferDisabled;
	m_bExcludeFromExtRef = dd.m_bExcludeFromExtRef;

	m_arViewModes.RemoveAll();
	for (int i = 0; i <= dd.m_arViewModes.GetUpperBound(); i++)
	{
		CViewModeDescription* pDescri = (CViewModeDescription*)dd.m_arViewModes.GetAt(i);
		m_arViewModes.Add(pDescri->Clone());
	}

}

//----------------------------------------------------------------------------------------------
CDocumentDescription* CDocumentDescription::Clone()
{
	CDocumentDescription* pNewDescri = new CDocumentDescription();

	pNewDescri->Assign(*this);

	return pNewDescri;
}

//=============================================================================        
// class CClientDocDescription implementation
//=============================================================================        
IMPLEMENT_DYNCREATE(CClientDocDescription, CObject)

//----------------------------------------------------------------
CClientDocDescription::CClientDocDescription()
	:
	CBaseDescription(CTBNamespace::DOCUMENT),
	m_Type(CClientDocDescription::NORMAL),
	m_MsgRouting(CClientDocDescription::CD_MSG_BEFORE)
{
}

//----------------------------------------------------------------
CClientDocDescription::CClientDocDescription(const CTBNamespace& aNamespace, const CString& sTitle)
	:
	CBaseDescription(CTBNamespace::DOCUMENT),
	m_Type(CClientDocDescription::NORMAL),
	m_MsgRouting(CClientDocDescription::CD_MSG_BEFORE)
{
	m_Namespace = aNamespace;
	m_sNotLocalizedTitle = sTitle;
}

//----------------------------------------------------------------
CViewModeDescription* CClientDocDescription::GetViewModeDescription(CString sName)
{
	CViewModeDescription* aCViewModeDescription = NULL;
	for (int i = 0; i < m_ViewModeDescriptions.GetSize(); i++)
	{
		aCViewModeDescription = ((CViewModeDescription*)m_ViewModeDescriptions[i]);
		if (aCViewModeDescription->GetName() == sName)
		{
			return aCViewModeDescription;
		}
	}
	return NULL;
}

//=============================================================================        
// class CServerDocDescription implementation
//=============================================================================        
IMPLEMENT_DYNCREATE(CServerDocDescription, CObject)

//----------------------------------------------------------------
CServerDocDescription::CServerDocDescription(const CString& sClass, const BOOL& bIsFamily, const CTBNamespace& aNamespace)
	:
	m_bIsFamily(bIsFamily),
	m_sClass(sClass),
	m_Namespace(aNamespace)
{
}

//----------------------------------------------------------------
void CServerDocDescription::SetClass(const CString& sClass)
{
	m_sClass = sClass;
}

//----------------------------------------------------------------
void CServerDocDescription::SetIsFamily(const BOOL& bValue)
{
	m_bIsFamily = bValue;
}

//----------------------------------------------------------------
void CServerDocDescription::SetNamespace(const CTBNamespace& aNS)
{
	m_Namespace = aNS;
}

//----------------------------------------------------------------
void CServerDocDescription::AddClientDoc(CClientDocDescription* pDescri)
{
	m_arClientDocs.Add(pDescri);

	// aggiungo all' elenco la libreria solo se non esiste già
	CTBNamespace aLibNs
	(
		CTBNamespace::LIBRARY,
		pDescri->GetNamespace().GetApplicationName()
		+ CTBNamespace::GetSeparator() +
		pDescri->GetNamespace().GetObjectName(CTBNamespace::MODULE)
	);
	aLibNs.SetObjectName(pDescri->GetNamespace().GetObjectName(CTBNamespace::LIBRARY));

	BOOL bFound = FALSE;
	for (int i = 0; i <= m_arLibraries.GetUpperBound(); i++)
		if (*m_arLibraries.GetAt(i) == aLibNs)
		{
			bFound = TRUE;
			break;
		}

	if (!bFound)
		m_arLibraries.Add(new CTBNamespace(aLibNs));
}

//----------------------------------------------------------------
CBaseDescriptionArray& CServerDocDescription::GetClientDocs()
{
	return m_arClientDocs;
}

//----------------------------------------------------------------
CTBNamespaceArray& CServerDocDescription::GetLibraries()
{
	return m_arLibraries;
}

// determina se il When Server rappresentato rientra nella gerarchia 
// di classi indicata preoccupandomi però con non sia una sottoparola
//----------------------------------------------------------------
BOOL CServerDocDescription::IsHierarchyOf(const CString& sHierarchyClass) const
{
	if (sHierarchyClass.IsEmpty() || m_sClass.IsEmpty())
		return FALSE;

	// prima lo cerco intermedio comprensivo dei suoi separatori
	int nPos = sHierarchyClass.Find(szHierarchySeparator + m_sClass + szHierarchySeparator);
	if (nPos > 0)
		return TRUE;

	int lenHierarchy = sHierarchyClass.GetLength();
	int lenClass = m_sClass.GetLength();

	// poi provo a cercarlo all'inizio della gerarchia compreso del separatore finale
	if (_tcsicmp(sHierarchyClass.Left(lenClass + 1), m_sClass + szHierarchySeparator) == 0)
		return TRUE;

	// adesso provo a cercarlo al termine della gerarchia ed adesso tengo
	// conto del fatto che possa essere con o senza separatore (stringa secca)
	nPos = lenHierarchy - lenClass - 1;

	// carattere precedente
	CString sCarPrec = sHierarchyClass.Mid(nPos, 1);

	BOOL bOk = _tcsicmp(sHierarchyClass.Right(lenClass), m_sClass) == 0 &&
		(lenClass == lenHierarchy || _tcsicmp(sCarPrec, szHierarchySeparator) == 0);

	return bOk;
}

//=============================================================================        
//class CServerFormDescriArray implementation
//=============================================================================  

CServerFormDescriArray::~CServerFormDescriArray()
{
	POSITION pos = GetStartPosition();
	CString sKey;
	CServerFormDescription* pServerDescri = NULL;
	while (pos)
	{
		GetNextAssoc(pos, sKey, pServerDescri);
		delete pServerDescri;
	}

}
//----------------------------------------------------------------
CServerFormDescription* CServerFormDescriArray::Get(const CString &sName, BOOL bCreate /*= FALSE*/)
{
	CServerFormDescription* pServerDescri = NULL;
	if (!Lookup(sName, pServerDescri) && bCreate)
	{
		pServerDescri = new CServerFormDescription;
		pServerDescri->m_sName = sName;
		(*this)[sName] = pServerDescri;
	}
	return pServerDescri;
}

//=============================================================================        
//class CServerDocDescriArray implementation
//=============================================================================        

//----------------------------------------------------------------
CClientDocDescription* CServerDocDescriArray::GetClientDocInfo(const CTBNamespace& aDocNS) const
{
	CServerDocDescription* pServerInfo;
	CClientDocDescription* pClientDoc;
	for (int nIdx = 0; nIdx < GetSize(); nIdx++)
	{
		pServerInfo = GetAt(nIdx);
		if (!pServerInfo)
			continue;

		pClientDoc = (CClientDocDescription*)pServerInfo->m_arClientDocs.GetInfo(aDocNS);

		if (pClientDoc)
			return pClientDoc;
	}

	return NULL;
}


//------------------------------------------------------------------------------
void CServerDocDescriArray::AddClientDocsOnServer(CServerDocDescription* pServerInfo)
{
	if (!pServerInfo)
		return;

	CServerDocDescription* pServerDescri = NULL;
	// cerco se esiste
	for (int i = 0; i <= AfxGetClientDocsTable()->GetUpperBound(); i++)
	{
		pServerDescri = AfxGetClientDocsTable()->GetAt(i);
		if (
			pServerInfo->GetIsFamily() == pServerDescri->GetIsFamily() &&
			pServerInfo->GetClass() == pServerDescri->GetClass() &&
			pServerInfo->GetNamespace() == pServerDescri->GetNamespace()
			)
			break;

		pServerDescri = NULL;
	}

	// se esiste già, travaso nell'esistente
	if (pServerDescri)
	{
		for (int i = 0; i <= pServerInfo->GetClientDocs().GetUpperBound(); i++)
		{
			CClientDocDescription* pClientDoc = (CClientDocDescription*)pServerInfo->GetClientDocs().GetAt(i);

			// esiste già una dichiarazione sulla stessa library
			if (!pClientDoc)
				continue;

			CClientDocDescription* pNewClientDoc = new CClientDocDescription();
			*pNewClientDoc = *pClientDoc;
			pServerDescri->AddClientDoc(pNewClientDoc);
		}

		delete pServerInfo;
	}
	else
		this->Add(pServerInfo);
}

//=============================================================================        
//			DocumentObjectsTable needed structures
//=============================================================================        
//------------------------------------------------------------------------------
//							General Functions
//-----------------------------------------------------------------------------

//------------------------------------------------------------------------------
DocumentObjectsTablePtr AFXAPI AfxGetWritableDocumentObjectsTable()
{
	CLoginContext* pContext = AfxGetLoginContext();

	// standard context
	if (!pContext)
		return DocumentObjectsTablePtr(AfxGetApplicationContext()->GetObject<DocumentObjectsTable>(&CApplicationContext::GetStandardDocumentsTable), TRUE);

	// loguin context exist but table is not attached
	if (!pContext->GetDocumentObjectsTable())
		pContext->AttachDocumentObjectsTable(new DocumentObjectsTable());

	return DocumentObjectsTablePtr(pContext->GetObject<DocumentObjectsTable>(&CLoginContext::GetDocumentObjectsTable), TRUE);
}

//----------------------------------------------------------------------------
const CDocumentDescription* AFXAPI AfxGetDocumentDescription(const CTBNamespace& aNamespace)
{
	DocumentObjectsTable* pCustomTable = NULL;

	CLoginContext* pContext = AfxGetLoginContext();

	if (pContext)
		pCustomTable = pContext->GetObject<DocumentObjectsTable>(&CLoginContext::GetDocumentObjectsTable);

	CDocumentDescription* pDescri = NULL;

	//nelle mappe i namespace sono nel formato document.Erp..... per cui autocompleto 
	//i namespace per assicurarmi che vengano cercati in maniera corretta
	CTBNamespace aNs(aNamespace);
	aNs.AutoCompleteNamespace(CTBNamespace::DOCUMENT, aNamespace.ToString(), CTBNamespace());

	if (pCustomTable)
		pDescri = pCustomTable->GetDescription(aNs);

	// custom description
	if (pDescri)
		return pDescri;

	DocumentObjectsTable* pStandardTable = (DocumentObjectsTable*)AfxGetApplicationContext()->GetStandardDocumentsTable();
	return pStandardTable->GetDescription(aNs);
}
//----------------------------------------------------------------------------
void AFXAPI AfxAddDocumentDescription(const CDocumentDescription* pDescription)
{
	DocumentObjectsTable* pStandardTable = (DocumentObjectsTable*)AfxGetApplicationContext()->GetStandardDocumentsTable();
	pStandardTable->AddObject((CDocumentDescription*)pDescription);
}

//----------------------------------------------------------------------------
CBaseDescriptionArray* AFXAPI AfxGetDocumentDescriptionsOf(const CTBNamespace& aNsModule)
{
	CBaseDescriptionArray* pDescriptions = new CBaseDescriptionArray();
	pDescriptions->SetOwns(FALSE);

	DocumentObjectsTable* pStandardTable = (DocumentObjectsTable*)AfxGetApplicationContext()->GetStandardDocumentsTable();

	// standard descriptions
	if (pStandardTable)
		pStandardTable->GetDescriptionsOf(aNsModule, *pDescriptions);

	DocumentObjectsTable* pCustomTable = NULL;

	CLoginContext* pContext = AfxGetLoginContext();
	if (pContext)
		pCustomTable = pContext->GetObject<DocumentObjectsTable>(&CLoginContext::GetDocumentObjectsTable);

	if (!pCustomTable)
		return pDescriptions;

	// customization declaration have to substitute standard document
	CObArray arCustomDescriptions;
	pCustomTable->GetDescriptionsOf(aNsModule, arCustomDescriptions);
	if (!arCustomDescriptions.GetSize())
		return pDescriptions;

	CDocumentDescription* pCstDescri;
	CDocumentDescription* pStdDescri;
	for (int i = 0; i <= arCustomDescriptions.GetUpperBound(); i++)
	{
		pCstDescri = (CDocumentDescription*)arCustomDescriptions.GetAt(i);

		for (int n = 0; n <= pDescriptions->GetUpperBound(); n++)
		{
			pStdDescri = (CDocumentDescription*)pDescriptions->GetAt(n);
			if (pStdDescri && pCstDescri && pStdDescri->GetNamespace() == pCstDescri->GetNamespace())
				pDescriptions->SetAt(n, pCstDescri);
		}
	}
	return pDescriptions;
}

//-------------------------------------------------------------------------------------
CBaseDescriptionArray* AFXAPI AfxGetDocumentsDescriptions()
{
	CBaseDescriptionArray* pDescriptions = new CBaseDescriptionArray();
	pDescriptions->SetOwns(FALSE);

	DocumentObjectsTable* pStandardTable = (DocumentObjectsTable*)AfxGetApplicationContext()->GetStandardDocumentsTable();

	// standard descriptions
	if (pStandardTable)
		pStandardTable->GetDescriptions(*pDescriptions);

	DocumentObjectsTable* pCustomTable = NULL;

	CLoginContext* pContext = AfxGetLoginContext();
	if (pContext)
		pCustomTable = pContext->GetObject<DocumentObjectsTable>(&CLoginContext::GetDocumentObjectsTable);

	if (!pCustomTable)
		return pDescriptions;

	// customization declaration have to substitute standard document
	CObArray arCustomDescriptions;
	pCustomTable->GetDescriptions(arCustomDescriptions);
	if (!arCustomDescriptions.GetSize())
		return pDescriptions;

	CDocumentDescription* pCstDescri;
	CDocumentDescription* pStdDescri;
	for (int i = 0; i <= arCustomDescriptions.GetUpperBound(); i++)
	{
		pCstDescri = (CDocumentDescription*)arCustomDescriptions.GetAt(i);

		for (int n = 0; n <= pDescriptions->GetUpperBound(); n++)
		{
			pStdDescri = (CDocumentDescription*)pDescriptions->GetAt(n);
			if (pStdDescri && pCstDescri && pStdDescri->GetNamespace() == pCstDescri->GetNamespace())
				pDescriptions->SetAt(n, pCstDescri);
		}
	}
	return pDescriptions;
}

//------------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(DocumentObjectsTable, CObject)

//------------------------------------------------------------------------------
DocumentObjectsTable::DocumentObjectsTable()
{
}

//------------------------------------------------------------------------------
DocumentObjectsTable::~DocumentObjectsTable()
{
	CDataObjDescription* pItem;
	CString strKey;
	POSITION			pos;

	for (pos = m_Documents.GetStartPosition(); pos != NULL;)
	{
		m_Documents.GetNextAssoc(pos, strKey, (CObject*&)pItem);
		if (pItem)
		{
			m_Documents.RemoveKey(strKey);
			delete pItem;
		}
	}
}

//------------------------------------------------------------------------------
int DocumentObjectsTable::AddObject(CDocumentDescription* pDescri)
{
	TB_LOCK_FOR_WRITE();

	CString sKey = pDescri->GetNamespace().ToString();
	sKey = sKey.MakeLower();

	// first of all I lookup into the table if I have to merge with an existing 
	CDocumentDescription* pExistingDescri = NULL;
	if (m_Documents.Lookup(sKey, (CObject*&)pExistingDescri) && pExistingDescri)
		return Merge(pExistingDescri, pDescri);

	// If an existing description does not exist I have to considerate
	// if I'm writing standard or custom table. Custom table contains only
	// customizations but if the customization is related to a standard 
	// document, the customization is inserted and merged with its standard
	// description, in order to manage a complete document description. See AfxGetDocumentDescription()
	if (this == AfxGetApplicationContext()->GetStandardDocumentsTable())
	{
		// in standard table descriptions are normally listed
		m_Documents.SetAt(sKey, pDescri);
		return 1;
	}

	// I' in the customizations table, first of all I search standard description
	CDocumentDescription* pStandardDescri = ((DocumentObjectsTable*)AfxGetApplicationContext()->GetStandardDocumentsTable())->GetDescription(pDescri->GetNamespace());

	if (pStandardDescri)
	{
		// if exist first of all a clone the standard one
		pExistingDescri = pStandardDescri->Clone();
		m_Documents.SetAt(sKey, pExistingDescri);

		//  the I merge the customization
		return Merge(pExistingDescri, pDescri);
	}

	// if standard does not exists, I insert only the custom declaration
	m_Documents.SetAt(sKey, pDescri);
	return 1;
}

//------------------------------------------------------------------------------
CDocumentDescription* DocumentObjectsTable::GetDescription(const CTBNamespace& nsDoc) const
{
	TB_LOCK_FOR_READ();

	CString sKey = nsDoc.ToString();
	CDocumentDescription* pDescri = NULL;
	m_Documents.Lookup(sKey.MakeLower(), (CObject*&)pDescri);

	return pDescri;
}

//------------------------------------------------------------------------------
void DocumentObjectsTable::GetDescriptionsOf(const CTBNamespace& nsModule, CObArray& arDescri) const
{
	CDocumentDescription* pItem;
	CString strKey;
	POSITION pos;

	for (pos = m_Documents.GetStartPosition(); pos != NULL;)
	{
		m_Documents.GetNextAssoc(pos, strKey, (CObject*&)pItem);
		if (
			pItem
			&&
			_tcsicmp(pItem->GetOwner().GetApplicationName(), nsModule.GetApplicationName()) == 0
			&&
			_tcsicmp(pItem->GetOwner().GetModuleName(), nsModule.GetModuleName()) == 0
			)
			arDescri.Add(pItem);
	}
}

//-----------------------------------------------------------------------------------------
void DocumentObjectsTable::GetDescriptions(CObArray& arDescri) const
{
	CDocumentDescription* pItem;
	CString strKey;
	POSITION pos;

	for (pos = m_Documents.GetStartPosition(); pos != NULL;)
	{
		m_Documents.GetNextAssoc(pos, strKey, (CObject*&)pItem);
		if (pItem && !AfxGetPathFinder()->IsASystemApplication(pItem->GetOwner().GetApplicationName()))
			arDescri.Add(pItem);
	}
}

//------------------------------------------------------------------------------
int DocumentObjectsTable::Merge(CDocumentDescription* pExistingDescri, CDocumentDescription* pNewDescri)
{
	return -1;
}
