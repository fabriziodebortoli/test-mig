#include "stdafx.h"

#include <io.h>

#include <TbNameSolver\Diagnostic.h>

#include <TbXmlCore\XMLTags.h>
#include <TbXmlCore\XMLDocObj.h>
#include <TbXmlCore\XMLParser.h>

#include <TBGeneric\ReportObjectsInfo.h>
#include <TBGeneric\GeneralFunctions.h>
#include <TBGeneric\EnumsTable.h>

#include <TBGeneric\LocalizableObjs.h>

#include <TbWebServicesWrappers\LoginManagerInterface.h>
#include <TbClientCore\ClientObjects.h>

#include "XmlReportObjectsParser.h"

//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char THIS_FILE[] = __FILE__;
#endif

//----------------------------------------------------------------------------------------------
//	class CXMLDocumentReportParser implementation
//----------------------------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CXMLDocumentReportParser, CObject)

//----------------------------------------------------------------------------------------------
CXMLDocumentReportParser::CXMLDocumentReportParser ()
	:
	m_BaseParser (XML_REPORT_TAG, TRUE)
{
}

//----------------------------------------------------------------------------------------------
BOOL CXMLDocumentReportParser::Parse (CXMLNode* pNode, CBaseDescription* pDescri, const CTBNamespace& aModuleNS, Array* pGroupArray /*= NULL*/)
{
	if (!pNode)
		return FALSE;

	CDocumentReportDescription* pRepDescri = (CDocumentReportDescription*) pDescri;

	CString strTemp;
	if (pNode->GetAttribute (XML_ALLOWISO_ATTRIBUTE, strTemp))
		if (!strTemp.IsEmpty() && !CXmlAttributeValidator::IsValidCountry (strTemp, TRUE))
			return FALSE;

	if (pNode->GetAttribute (XML_DENYISO_ATTRIBUTE, strTemp))
		if (!strTemp.IsEmpty() && !CXmlAttributeValidator::IsValidCountry (strTemp, FALSE))
			return FALSE;

	// activation checking
	if (pNode->GetAttribute (XML_ACTIVATION_ATTRIBUTE, strTemp))
		if (!strTemp.IsEmpty() && !CXmlAttributeValidator::IsValidActivation (strTemp))
			return FALSE;

	CDocumentReportGroupDescription* pReportGroupDescr(NULL);
	if (pNode->GetAttribute(XML_GROUPID_ATTRIBUTE, strTemp))
	{
		Array groupIdArray;
		if (pGroupArray)
		{
			CDocumentReportGroupDescription* pCurrGroupDescr(NULL);
			for (int i = 0; i <= pGroupArray->GetUpperBound(); i++)
			{
				pCurrGroupDescr = dynamic_cast<CDocumentReportGroupDescription*>(pGroupArray->GetAt(i));
				if (!pCurrGroupDescr)
					return FALSE;

				if (pCurrGroupDescr->GetId() == _wtoi(strTemp))
					pRepDescri->SetGroupDescription(*pCurrGroupDescr);

				groupIdArray.Add(new DataInt(pCurrGroupDescr->GetId()));
			}
		}

		if (!strTemp.IsEmpty() && !CXmlAttributeValidator::IsValidGroup(_wtoi(strTemp), &groupIdArray))
			return FALSE;
	}

	// function prototype or simple description
	if (pNode->GetChildByName(XML_FUNCTION_TAG) != NULL)
	{
		if (!pRepDescri->ParsePrototype (pNode, aModuleNS))
		{
			AfxGetDiagnostic()->Add (cwsprintf(_TB("The entry prototype {0-%s} element {1-%s} contains errors. Declaration ignored."), 
					(LPCTSTR) aModuleNS.ToString (), (LPCTSTR) pRepDescri->GetNamespace().ToString ()));
			return FALSE;
		}
	}
	else
	{
		m_BaseParser.Parse(pNode, pRepDescri, aModuleNS);
		if (pRepDescri)
			pRepDescri->ParseArguments(pNode->GetChildByName(XML_ARGUMENTS_TAG));
	}

	return TRUE;
}

//----------------------------------------------------------------------------------------------
void CXMLDocumentReportParser::Unparse(CXMLNode* pNode, CBaseDescription* pDescri)
{
	if (!pNode || pDescri->m_XMLFrom == CBaseDescription::XML_MODIFIED)
		return;

	pNode->SetAttribute(XML_NAMESPACE_ATTRIBUTE, pDescri->GetNamespace().ToUnparsedString());
	CString strTitle = pDescri->GetNotLocalizedTitle();
	if (strTitle.IsEmpty())
		strTitle = GetName(pDescri->GetName());

	pNode->SetAttribute(XML_LOCALIZE_ATTRIBUTE, strTitle);
}

//----------------------------------------------------------------------------------------------
//							CXMLReportObjectsParser
//----------------------------------------------------------------------------------------------
//
//----------------------------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(CXMLReportObjectsParser, CObject)

//----------------------------------------------------------------------------------------------
CXMLReportObjectsParser::CXMLReportObjectsParser()
{
}

//----------------------------------------------------------------------------------------------
BOOL CXMLReportObjectsParser::Parse (CXMLDocumentObject* pDoc, CReportObjectsDescription* pDescri, const CTBNamespace& aParent, CTBNamespace& NsDefault)
{
	CString sNodeName;

	// root
	CXMLNode* pRoot = pDoc->GetRoot();
	CString sRootName;
	if (!pRoot || !pRoot->GetName(sRootName) || _tcsicmp(sRootName, XML_REPORTOBJ_TAG) != 0) 
	{
		AfxGetDiagnostic()->Add (cwsprintf(_TB("The file Reports.xml for document {0-%s} has no root element. File not loaded."), (LPCTSTR) aParent.ToString ()));
		return FALSE;
	}

	// Groups
	BOOL bGroupsMode(FALSE);
	CXMLNode* pGroupsNode = pRoot->GetChildByName(XML_GROUPS_TAG);
	Array aGroupsArray;
	
	if (pGroupsNode)
	{
		aGroupsArray.Add(new CDocumentReportGroupDescription(0, _T("Default"), TRUE)); // creo il gruppo di default per eventuali aggiunte tramite interfaccia

		bGroupsMode = TRUE;
		CXMLNode* pGroupNode;
		CString groupNodeName;
		CString sAuxIdAttr;
		CString sAuxLocalAttr;
		CString sAuxSubmenuAttr;
		CDocumentReportGroupDescription* pNewReportGroupDescr(NULL);
		for (int i = 0; i <= pGroupsNode->GetChilds()->GetUpperBound(); i++)
		{
			pGroupNode = pGroupsNode->GetChilds()->GetAt(i);
			
			if (!pGroupNode || !pGroupNode->GetName(groupNodeName))
				continue;

			if (groupNodeName != XML_GROUP_TAG)
				continue;

			pGroupNode->GetAttribute(XML_GROUPID_ATTRIBUTE, sAuxIdAttr);
			if (sAuxIdAttr.IsEmpty())
			{
				ASSERT(FALSE);
				continue;
			}

			pGroupNode->GetAttribute(XML_LOCALIZE_ATTRIBUTE, sAuxLocalAttr);
			if (sAuxLocalAttr.IsEmpty())
			{
				ASSERT(FALSE);
				continue;
			}

			pGroupNode->GetAttribute(XML_SUBMENU_ATTRIBUTE, sAuxSubmenuAttr);
			if (sAuxLocalAttr.IsEmpty())
			{
				ASSERT(FALSE);
				continue;
			}
			
			pNewReportGroupDescr = new CDocumentReportGroupDescription(_wtoi(sAuxIdAttr), sAuxLocalAttr, sAuxSubmenuAttr.MakeUpper().Trim() == _T("TRUE") ? TRUE : FALSE);
			aGroupsArray.Add(pNewReportGroupDescr);
		}
	}

	// Reports
	CXMLNode* pRepsNode = pRoot->GetChildByName(XML_REPORTS_TAG);
	if (!pRepsNode)
		return TRUE;


	CString sTemp;
	pRepsNode->GetAttribute (XML_DEFAULT_ATTRIBUTE, sTemp);
	if (!sTemp.Trim().IsEmpty())
		NsDefault.AutoCompleteNamespace(CTBNamespace::REPORT, sTemp.Trim(), NsDefault);		

	if (!pRepsNode->GetChilds())
	{
		m_NsDefault = NsDefault;
		return TRUE;
	}

	CXMLNode* pRepNode;
	CDocumentReportDescription* pNewDescription;

	for (int i=0; i <= pRepsNode->GetChilds()->GetUpperBound(); i++)
	{
		pRepNode = pRepsNode->GetChilds()->GetAt(i);

		if (!pRepNode || !pRepNode->GetName(sNodeName))
			continue;

		if (sNodeName != XML_REPORT_TAG)
			continue;

		pNewDescription = new CDocumentReportDescription();
		//pNewDescription->m_sTitle
		// si deve considerare il caso dei clientdoc: nn devo inserire nell'array i report doppi
		if (m_ObjectParser.Parse(pRepNode, pNewDescription, aParent, &aGroupsArray))
		{
			CDocumentReportDescription* pExistedRep = pDescri->GetReportInfo(pNewDescription->GetNamespace());
			if (pExistedRep == NULL  || pExistedRep->IsDefault())
			{
				if (!pNewDescription->GetNamespace().IsValid())
				{
					delete pNewDescription;
					continue;
				}

				// Il Report appartiene alla configurazione licenziata
				if (!AfxIsActivated(pNewDescription->GetNamespace().GetApplicationName(), pNewDescription->GetNamespace().GetModuleName()))
				{
					delete pNewDescription;
					continue;
				}

				// DEVO CONTROLLARE CHE IL REPORT ESISTA NEL FILE SYSTEM
				if (!ExistFile(AfxGetPathFinder()->GetReportFullName(pNewDescription->GetNamespace(), AfxGetLoginInfos()->m_strUserName)))
				{
					delete pNewDescription;
					continue;
				}
				
				pNewDescription->SetDefault(FALSE);

				if (pNewDescription->GetNotLocalizedTitle().IsEmpty())
					pNewDescription->SetNotLocalizedTitle(GetName(pNewDescription->GetName()));
				
				pDescri->AddReport(pNewDescription);
			}
			else
				delete pNewDescription;
		}
		else
			delete pNewDescription;
	}

	CString sSelectQuery = XML_REPORTOBJ_TAG;
	sSelectQuery += _T("/");
	sSelectQuery += XML_REPORTS_TAG;
	sSelectQuery += _T("/");
	sSelectQuery += XML_DEFAULT_TAG;

	CXMLNodeChildsList* pDefNodes = pDoc->SelectNodes (sSelectQuery);
	CXMLNode* pDefNode;
	if (pDefNodes)
	{
		for (int i=0; i < pDefNodes->GetSize(); i++)
		{
			pDefNode = pDefNodes->GetAt(i);
			if (pDefNode)
				ParseDefaultReport (pDefNode, pDescri);
		}
		delete pDefNodes;
	}

	// I set the defaultValue
	CDocumentReportDescription* pReportDescri = NULL;

	if (!m_CountryNsDefault.IsEmpty ())
		pReportDescri = pDescri->GetReportInfo (m_CountryNsDefault);

	if (pReportDescri)
	{
		pReportDescri->SetDefault(TRUE);
		NsDefault = m_CountryNsDefault;
		return TRUE;
	}

	if (!NsDefault.IsEmpty ())
		pReportDescri = pDescri->GetReportInfo (NsDefault);

	if (pReportDescri)
		pReportDescri->SetDefault(TRUE);

	return TRUE;
}

//----------------------------------------------------------------------------------------------
BOOL CXMLReportObjectsParser::ParseDefaultReport (CXMLNode* pNode, CReportObjectsDescription* pReportObjects)
{
	CString strTemp;

	if (pNode->GetAttribute (XML_ALLOWISO_ATTRIBUTE, strTemp))
		if (!strTemp.IsEmpty() && !CXmlAttributeValidator::IsValidCountry (strTemp, TRUE))
			return FALSE;

	if (pNode->GetAttribute (XML_DENYISO_ATTRIBUTE, strTemp))
		if (!strTemp.IsEmpty() && !CXmlAttributeValidator::IsValidCountry (strTemp, FALSE))
			return FALSE;

	if (pNode->GetAttribute (XML_NAMESPACE_ATTRIBUTE, strTemp) && !strTemp.Trim().IsEmpty())
	{
		CTBNamespace aNs; 
		aNs.AutoCompleteNamespace(CTBNamespace::REPORT, strTemp.Trim(), aNs);
	
		CDocumentReportDescription* pRepDescri = pReportObjects->GetReportInfo (aNs);
		if (pRepDescri)
			m_CountryNsDefault = aNs;
	}

	return TRUE;
}

//----------------------------------------------------------------------------------------------
void CXMLReportObjectsParser::Unparse (CXMLDocumentObject* pDoc, CReportObjectsDescription* pDescri, CDocumentReportDescription* pDefault /*= NULL*/)
{
	// prima creo la root
	CXMLNode* pRoot = pDoc->CreateRoot(XML_REPORTOBJ_TAG);
	if (!pRoot)
	{
		ASSERT(FALSE);
		return;
	}

	CXMLNode* pNewNode = pRoot->CreateNewChild(XML_REPORTS_TAG);
	
	if (pDefault)
		pNewNode->SetAttribute(XML_DEFAULT_ATTRIBUTE, pDefault->GetNamespace().ToUnparsedString());
	// reports
	for (int i = 0; i <= pDescri->GetReports().GetUpperBound(); i++)
	{
		CDocumentReportDescription* pRepDescr  = (CDocumentReportDescription*) pDescri->GetReports().GetAt(i);
		if (pRepDescr->m_XMLFrom != CDocumentReportDescription::XML_MODIFIED)
		{
			CXMLNode* pNewNodeRep = pNewNode->CreateNewChild(XML_REPORT_TAG);
			m_ObjectParser.Unparse(pNewNodeRep, pDescri->GetReports().GetAt(i));		
		}
	}
}
