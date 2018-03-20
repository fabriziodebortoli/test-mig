#include "stdafx.h"

#include <io.h>

#include <TbNameSolver\Diagnostic.h>
#include <TbNameSolver\ApplicationContext.h>

#include <TbXmlCore\XMLTags.h>
#include <TbXmlCore\XMLDocObj.h>
#include <TbXmlCore\XMLParser.h>
#include <TBXMLCore\xmlgeneric.h>

#include <TBGeneric\DocumentObjectsInfo.h>
#include <TBGeneric\GeneralFunctions.h>
#include <TBGeneric\DataObjDescription.h>

#include "XmlDocumentObjectsParser.h"


#ifdef _DEBUG
#undef THIS_FILE
static const char THIS_FILE[] = __FILE__;
#endif

static const TCHAR szXmlServerKey[]		= _T("/ClientDocumentObjects/ClientDocuments/ServerDocument");
static const TCHAR szXmlClientKey[]		= _T("/ClientDocumentObjects/ClientDocuments/ServerDocument/ClientDocument");
static const TCHAR szXmlClientFormKey[]		= _T("/ClientDocumentObjects/ClientForms/ClientForm");
static const TCHAR szXmlClientFormServerKey[] = _T("/ClientDocumentObjects/ClientForms/ClientForm/Server");
static const TCHAR szXmlClientFormExcludeKey[] = _T("/ClientDocumentObjects/ClientForms/ClientForm/Exclude");
static const TCHAR szXmlClientDocumentViewModeKey[]		= _T("/ClientDocumentObjects/ClientDocuments/ServerDocument/ClientDocument/ViewModes/Mode");

//----------------------------------------------------------------------------------------------
//							CXMLDocumentObjectsParser
//----------------------------------------------------------------------------------------------
//
//----------------------------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(CXMLDocumentObjectsParser, CXMLBaseDescriptionParser)

//----------------------------------------------------------------------------------------------
CXMLDocumentObjectsParser::CXMLDocumentObjectsParser()
{
	SetTagName(XML_DOCUMENT_TAG);
}

//-----------------------------------------------------------------------------
BOOL CXMLDocumentObjectsParser::LoadDocumentObjects (const CTBNamespace& aNamespace, CPathFinder::PosType aPosType, DocumentObjectsTable* pTable)
{
	CString sFileName = AfxGetPathFinder()->GetDocumentObjectsFullName(aNamespace, aPosType, aPosType == CPathFinder::CUSTOM ? CPathFinder::EASYSTUDIO : CPathFinder::CURRENT);
	if (!ExistFile(sFileName))
		return TRUE;
	
	CXMLDocumentObject aDoc;
	TRY
	{
		if (aDoc.LoadXMLFile(sFileName) && Parse(&aDoc, pTable, aNamespace))
		return TRUE;
	}
		CATCH(CException, e)
	{
		TCHAR szError[1024];
		e->GetErrorMessage(szError, 1024);
		AfxGetDiagnostic()->Add(cwsprintf(_TB("{0-%s} file has not been loaded due to the following error {1-%s} "), sFileName, szError), CDiagnostic::Warning);
		return FALSE;
	}
	END_CATCH

	return FALSE;
}

//----------------------------------------------------------------------------------------------
BOOL CXMLDocumentObjectsParser::Parse(
										CXMLDocumentObject*		pDoc, 
										DocumentObjectsTable*	pTable, 
										const CTBNamespace&		nsParent
									 )
{
	CString sNodeName;

	// root
	CXMLNode* pRoot = pDoc->GetRoot();
	CString sRootName;
	if (!pRoot || !pRoot->GetName(sRootName) || _tcsicmp(sRootName, XML_DOCUMENTOBJECTS_TAG) != 0) 
	{
		AfxGetDiagnostic()->Add (cwsprintf(_TB("The file DocumentObjects.xml for module {0-%s} has no root element. File not loaded."), (LPCTSTR) nsParent.ToString ()));
		return FALSE;
	}

	// Documents
	CXMLNode* pDocsNode = pRoot->GetChildByName(XML_DOCUMENTS_TAG);

	CXMLNode* pDocNode;
	if (pDocsNode && pDocsNode->GetChilds())
	{
		CDocumentDescription* pNewDescription;
		for (int i=0; i <= pDocsNode->GetChilds()->GetUpperBound(); i++)
		{
			pDocNode = pDocsNode->GetChilds()->GetAt(i);

			if (!pDocNode || !pDocNode->GetName(sNodeName) || sNodeName != XML_DOCUMENT_TAG)
				continue;

			pNewDescription = new CDocumentDescription();

			if  (
					!ParseDocument(pDocNode, pNewDescription, nsParent) ||
					pTable->AddObject(pNewDescription) <= 0
				)
				delete pNewDescription;
		}
	}

	return TRUE;
}

//----------------------------------------------------------------------------------------------
BOOL CXMLDocumentObjectsParser::ParseDocument(CXMLNode* pNode, CDocumentDescription* pDescri, const CTBNamespace& aModuleNS)
{
	if (!pNode)
		return FALSE;

	if (!CXMLBaseDescriptionParser::Parse (pNode, pDescri, aModuleNS))
		return FALSE;

	CDocumentDescription* pDocDescri = (CDocumentDescription*) pDescri;

	pDescri->SetOwner(aModuleNS);

	CString sValue;
	// country checking
	pNode->GetAttribute(XML_ALLOWISO_ATTRIBUTE, sValue);
	if (!sValue.IsEmpty() && !CXmlAttributeValidator::IsValidCountry(sValue, TRUE))
		pDocDescri->SetActivated(FALSE);

	pNode->GetAttribute(XML_DENYISO_ATTRIBUTE, sValue);
	if (!sValue.IsEmpty() && !CXmlAttributeValidator::IsValidCountry(sValue, FALSE))
		pDocDescri->SetActivated(FALSE);

	pNode->GetAttribute(XML_ACTIVATION_ATTRIBUTE, sValue);
	if (!sValue.IsEmpty() && !CXmlAttributeValidator::IsValidActivation(sValue))
		pDocDescri->SetActivated(FALSE);

	if (pNode->GetAttribute(XML_PUBLISHED_ATTRIBUTE, sValue))
		pDocDescri->SetPublished(GetBoolFromXML(sValue));

	pNode->GetAttribute(XML_CLASSHIERARCHY_ATTRIBUTE, sValue);
	pDocDescri->SetClassHierarchy (sValue);

	// gestiscono i family solo i documenti che definiscono la gerarchia dei family
	int nPos = pDocDescri->GetClassHierarchy().Find(szDefaultFamilyParents);
	pDocDescri->SetExcludedFromFamily (nPos < 0);
	
	sValue.Empty ();
	CXMLNode* pClassNode = pNode->GetChildByName(XML_INTERFACECLASS_TAG);
	if (pClassNode)
		pClassNode->GetText(sValue);

	pDocDescri->SetInterfaceClass(sValue);

	//documenti dinamici di easy builder
	if (pNode->GetAttribute(XML_DYNAMIC_ATTRIBUTE, sValue))
		pDescri->SetDynamic(GetBoolFromXML(sValue));

	//documenti non runnabili da soli
	if (pNode->GetAttribute(XML_RUNNABLEALONE_ATTRIBUTE, sValue))
		pDescri->SetRunnableAlone(GetBoolFromXML(sValue));
	
	//documenti dinamici di easy builder sono apportati dalla custom o dalla standard?
	if (AfxGetPathFinder()->GetDocumentObjectsFullName(aModuleNS, CPathFinder::STANDARD))
		pDescri->SetLiveInStandard(TRUE);

	//to disable transfer operation
	if (pNode->GetAttribute(XML_TRANSFERDISABLED_ATTRIBUTE, sValue))
		pDescri->SetTransferDisabled(GetBoolFromXML(sValue));

	//to disable transfer operation
	if (pNode->GetAttribute(XML_TRANSFERDISABLED_ATTRIBUTE, sValue))
		pDescri->SetTransferDisabled(GetBoolFromXML(sValue));

	//documenti dinamici di easy builder
	if (pNode->GetAttribute(XML_DOC_TEMPLATE_ATTRIBUTE, sValue))
		pDescri->SetTemplateNamespace(new CTBNamespace(sValue));
	
	//documenti non editabili in design
	if (pNode->GetAttribute(XML_DESIGNABLE_ATTRIBUTE, sValue))
		pDescri->m_bDesignable = GetBoolFromXML(sValue);

	// <ViewModes>
	CXMLNode* pViewsNode = pNode->GetChildByName(XML_VIEWMODES_TAG);

	if (!pViewsNode || !pViewsNode->GetChilds())
		return TRUE;

	CString sNodeName;
	CXMLNode* pViewNode;
	CViewModeDescription* pNewDescription;
	for (int i=0; i <= pViewsNode->GetChilds()->GetUpperBound(); i++)
	{
		pViewNode = pViewsNode->GetChilds()->GetAt(i);

		if (!pViewNode || !pViewNode->GetName(sNodeName) || sNodeName != XML_MODE_TAG)
			continue;

		pNewDescription = new CViewModeDescription();

		if (ParseViewMode(pViewNode, pNewDescription, pDocDescri->GetNamespace()))
			pDocDescri->AddViewMode(pNewDescription);
		else
			delete pNewDescription;
	}

	//DescriptionKeys
	CXMLNode* pCaptionNode = pNode->GetChildByName(XML_CAPTION_TAG);
	if (!pCaptionNode || !pCaptionNode->GetChilds())
		return TRUE;

	CString sFormatStringValue;
	pCaptionNode->GetAttribute(XML_LOCALIZE_ATTRIBUTE, sFormatStringValue);
	pDocDescri->SetNotLocalizedCaption(sFormatStringValue);

	CXMLNode* pKeyNode;
	for (int i = 0; i <= pCaptionNode->GetChilds()->GetUpperBound(); i++)
	{
		pKeyNode = pCaptionNode->GetChilds()->GetAt(i);
		if (!pKeyNode)
			continue;
		
		pKeyNode->GetAttribute(XML_CAPTION_VALUE_ATTRIBUTE, sValue);

		pDocDescri->GetDescriptionKeys()->Add(sValue);
	}

	return TRUE;
}

//----------------------------------------------------------------------------------------------
BOOL CXMLDocumentObjectsParser::ParseViewMode (CXMLNode* pNode, CViewModeDescription* pDescri, const CTBNamespace& aParent)
{
	CString sValue;
	pNode->GetAttribute(XML_NAME_ATTRIBUTE, sValue);
	if (sValue.IsEmpty())
		return FALSE;

	CTBNamespace aObjectNs;
	aObjectNs.AutoCompleteNamespace(CTBNamespace::FORM, sValue, aParent);
	pDescri->SetNamespace(aObjectNs);

	pNode->GetAttribute(XML_TYPE_ATTRIBUTE, sValue);

	if (_tcsicmp(sValue, XML_VIWMODETYPE_BATCH_VALUE) == 0)
		pDescri->SetType(VMT_BATCH);
	else if (_tcsicmp(sValue, XML_VIWMODETYPE_FINDER_VALUE) == 0)
		pDescri->SetType(VMT_FINDER);
	// default sempre
	else
		pDescri->SetType(VMT_DATAENTRY);

	if (pNode->GetAttribute(XML_FRAME_ID_ATTRIBUTE, sValue))
		pDescri->SetFrameID(sValue);

	pNode->GetAttribute(XML_SCHEDULABLE_ATTRIBUTE, sValue);
	pDescri->SetSchedulable (sValue.IsEmpty () || _tcsicmp(sValue, XML_TRUE_VALUE) == 0);

	pNode->GetAttribute(XML_NOWEB_ATTRIBUTE, sValue);
	pDescri->SetNoWeb(GetBoolFromXML(sValue));

	return TRUE;
}

//----------------------------------------------------------------------------------------------
//							CXMLClientDocumentObjectsContent
//----------------------------------------------------------------------------------------------

//----------------------------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(CXMLClientDocumentObjectsContent, CXMLSaxContent)

//----------------------------------------------------------------------------------------------
CXMLClientDocumentObjectsContent::CXMLClientDocumentObjectsContent()
{
}

//-----------------------------------------------------------------------------
void CXMLClientDocumentObjectsContent::SetCurrentModule(const CTBNamespace& aCurrentModule)
{
	m_CurrentModule = aCurrentModule;
}

//----------------------------------------------------------------------------------------------
CString CXMLClientDocumentObjectsContent::OnGetRootTag() const
{
	return XML_CDDOCUMENTOBJECTS_TAG;
}

//----------------------------------------------------------------------------------------------
void CXMLClientDocumentObjectsContent::OnBindParseFunctions()
{
	BIND_PARSE_ATTRIBUTES(szXmlServerKey, &CXMLClientDocumentObjectsContent::ParseServerDocument)
		BIND_PARSE_ATTRIBUTES(szXmlClientKey, &CXMLClientDocumentObjectsContent::ParseClientDocument)
		BIND_PARSE_ATTRIBUTES(szXmlClientFormKey, &CXMLClientDocumentObjectsContent::ParseClientForm)
		BIND_PARSE_ATTRIBUTES(szXmlClientFormServerKey, &CXMLClientDocumentObjectsContent::ParseClientFormServer)
		BIND_PARSE_ATTRIBUTES(szXmlClientFormExcludeKey, &CXMLClientDocumentObjectsContent::ParseClientFormServerExclude)
		BIND_PARSE_TAG(szXmlServerKey, &CXMLClientDocumentObjectsContent::EndServerTag)

		BIND_PARSE_ATTRIBUTES(szXmlClientDocumentViewModeKey, &CXMLClientDocumentObjectsContent::ParseClientDocumentViewMode)
}

//----------------------------------------------------------------------------------------------
int CXMLClientDocumentObjectsContent::ParseClientForm(const CString& sUri, const CXMLSaxContentAttributes& arAttributes)
{
	m_sCurrentClientFormName = arAttributes.GetAttributeByName(XML_NAME_ATTRIBUTE);
	CString sServer = arAttributes.GetAttributeByName(XML_SERVER_ATTRIBUTE);
	if (!sServer.IsEmpty())
	{
		CServerFormDescription* pServerDescri = GetServerFormDescriArray()->Get(sServer, TRUE);
		if (!pServerDescri->AddClient(m_sCurrentClientFormName, false, m_CurrentModule))
			return CXMLSaxContent::ABORT;
	}
	return CXMLSaxContent::OK;

}
//----------------------------------------------------------------------------------------------
int CXMLClientDocumentObjectsContent::ParseClientFormServer(const CString& sUri, const CXMLSaxContentAttributes& arAttributes)
{
	CString sServer = arAttributes.GetAttributeByName(XML_NAME_ATTRIBUTE);
	if (!sServer.IsEmpty())
	{
		CServerFormDescription* pServerDescri = GetServerFormDescriArray()->Get(sServer, TRUE);
		if (!pServerDescri->AddClient(m_sCurrentClientFormName, false, m_CurrentModule))
			return CXMLSaxContent::ABORT;
	}
	return CXMLSaxContent::OK;
}
//----------------------------------------------------------------------------------------------
int CXMLClientDocumentObjectsContent::ParseClientFormServerExclude(const CString& sUri, const CXMLSaxContentAttributes& arAttributes)
{
	CString sServer = arAttributes.GetAttributeByName(XML_NAME_ATTRIBUTE);
	if (!sServer.IsEmpty())
	{
		CServerFormDescription* pServerDescri = GetServerFormDescriArray()->Get(sServer, TRUE);
		if (!pServerDescri->AddClient(m_sCurrentClientFormName, true, m_CurrentModule))
			return CXMLSaxContent::ABORT;
	}
	return CXMLSaxContent::OK;

}
//----------------------------------------------------------------------------------------------
int CXMLClientDocumentObjectsContent::ParseServerDocument(const CString& sUri, const CXMLSaxContentAttributes& arAttributes)
{
	CString sValue = arAttributes.GetAttributeByName(XML_CLASS_ATTRIBUTE);
	CString sType = arAttributes.GetAttributeByName(XML_TYPE_ATTRIBUTE);
	CString sName = arAttributes.GetAttributeByName(XML_NAMESPACE_ATTRIBUTE);

	m_pCurrentServerDescri = new CServerDocDescription();

	m_pCurrentServerDescri->SetClass (sValue);
	m_pCurrentServerDescri->SetIsFamily(_tcsicmp(sType, XML_TYPE_FAMILY_VALUE) == 0);
		
	if ((m_pCurrentServerDescri->GetIsFamily() && m_pCurrentServerDescri->GetClass().IsEmpty()) || (!m_pCurrentServerDescri->GetIsFamily() && sName.IsEmpty()))
	{
		AddError (cwsprintf(_TB("DocumentObjects.xml for module {0-%s}: empty family class or <ServerDocument> name. ClientDocuments contents registration ignored."), (LPCTSTR) m_CurrentModule.ToString ()));
		delete m_pCurrentServerDescri;
		m_pCurrentServerDescri = NULL;
		return CXMLSaxContent::SKIP_THE_CHILDS;
	}
	if (sName.IsEmpty())
		sName = m_pCurrentServerDescri->GetClass();
	
	CTBNamespace aNs;
	aNs.AutoCompleteNamespace(CTBNamespace::DOCUMENT, sName, m_CurrentModule);
	m_pCurrentServerDescri->SetNamespace(aNs);

	return CXMLSaxContent::OK;
}

//----------------------------------------------------------------------------------------------
CServerDocDescriArray*CXMLClientDocumentObjectsContent::GetServerDocDescriArray()
{
	if (m_pServerDocArray == NULL)
		m_pServerDocArray = AfxGetApplicationContext()->GetObject<CServerDocDescriArray>(&CApplicationContext::GetClientDocsTable);
	return m_pServerDocArray;
}
//----------------------------------------------------------------------------------------------
CServerFormDescriArray* CXMLClientDocumentObjectsContent::GetServerFormDescriArray()
{
	if (m_pServerFormArray == NULL)
		m_pServerFormArray = AfxGetApplicationContext()->GetObject<CServerFormDescriArray>(&CApplicationContext::GetClientFormsTable);
	return m_pServerFormArray;
}
//----------------------------------------------------------------------------------------------
int CXMLClientDocumentObjectsContent::EndServerTag(const CString& sUri, const CString& aTagValue)
{
	if (m_pCurrentServerDescri->GetClientDocs().GetSize() > 0)
	{
		GetServerDocDescriArray()->AddClientDocsOnServer(m_pCurrentServerDescri);
	}
	else
	{
		delete m_pCurrentServerDescri;
		m_pCurrentServerDescri = NULL;
	}

	return CXMLSaxContent::OK;
}

//----------------------------------------------------------------------------------------------
int CXMLClientDocumentObjectsContent::ParseClientDocument(const CString& sUri, const CXMLSaxContentAttributes& arAttributes)
{
	CString sNamespace	= arAttributes.GetAttributeByName (XML_NAMESPACE_ATTRIBUTE);
	CString sTitle		= arAttributes.GetAttributeByName (XML_LOCALIZE_ATTRIBUTE);

	if (sNamespace.IsEmpty ())
	{
		AddWarning (cwsprintf(_TB("ClientDocumentObjects.xml for module {0-%s}: empty family class or <ServerDocument> name. ClientDocuments contents registration ignored."), (LPCTSTR) m_CurrentModule.ToString ()));
		return CXMLSaxContent::OK;
	}

	CTBNamespace aNs;
	aNs.AutoCompleteNamespace(CTBNamespace::DOCUMENT, sNamespace, m_CurrentModule);

	m_pCurrentClientDocDescri = new CClientDocDescription();

	m_pCurrentClientDocDescri->SetNamespace			(aNs);
	m_pCurrentClientDocDescri->SetNotLocalizedTitle	(sTitle);
	
	CString sType = arAttributes.GetAttributeByName (XML_TYPE_ATTRIBUTE);
		if (_tcsicmp(sType, _T("family")) == 0)
			m_pCurrentClientDocDescri->m_Type = CClientDocDescription::FAMILY;
		else if (_tcsicmp(sType, _T("Script")) == 0)
			m_pCurrentClientDocDescri->m_Type = CClientDocDescription::SCRIPT;
		else if (_tcsicmp(sType, _T("familyScript")) == 0)
			m_pCurrentClientDocDescri->m_Type = CClientDocDescription::FAMILYSCRIPT;
		else //if (sType.CompareNoCase(_T("normal")) == 0)
			m_pCurrentClientDocDescri->m_Type = CClientDocDescription::NORMAL;

	CString sRoutingMode = arAttributes.GetAttributeByName (_T("messageRoutingMode"));
		if (_tcsicmp(sRoutingMode, _T("Both")) == 0)
			m_pCurrentClientDocDescri->m_MsgRouting = CClientDocDescription::CD_MSG_BOTH;
		else if (_tcsicmp(sRoutingMode, _T("After")) == 0)
			m_pCurrentClientDocDescri->m_MsgRouting = CClientDocDescription::CD_MSG_AFTER;
		else //if (sRoutingMode.CompareNoCase(_T("Before")) == 0)
			m_pCurrentClientDocDescri->m_MsgRouting = CClientDocDescription::CD_MSG_BEFORE;

	m_pCurrentServerDescri->AddClientDoc(m_pCurrentClientDocDescri);

	return CXMLSaxContent::OK;
}

//----------------------------------------------------------------------------------------------
int CXMLClientDocumentObjectsContent::ParseClientDocumentViewMode(const CString& sUri, const CXMLSaxContentAttributes& arAttributes)
{
	CString sName			= arAttributes.GetAttributeByName (XML_NAME_ATTRIBUTE);

	if (sName.IsEmpty ())
	{
		AddWarning (cwsprintf(_TB("ClientDocumentObjects.xml for module {0-%s}: empty ViewMode name. ClientDocuments contents registration ignored."), (LPCTSTR) m_CurrentModule.ToString ()));
		return CXMLSaxContent::OK;
	}

	CViewModeDescription* pDescri = new CViewModeDescription();
	pDescri->SetName			(sName);

	m_pCurrentClientDocDescri->AddViewModeDescription(pDescri);

	return CXMLSaxContent::OK;
}

