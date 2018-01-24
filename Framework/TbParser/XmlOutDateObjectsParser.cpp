#include "stdafx.h"

#include <io.h>

#include <TbXmlCore\XMLTags.h>
#include <TbXmlCore\XMLDocObj.h>
#include <TbXmlCore\XMLParser.h>

#include <TbNameSolver\FileSystemFunctions.h>
#include <TbNameSolver\Diagnostic.h>

#include <TbGeneric\OutDateObjectsInfo.h>
#include <TBGeneric\GeneralFunctions.h>

#include "XmlOutDateObjectsParser.h"

//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char THIS_FILE[] = __FILE__;
#endif

//----------------------------------------------------------------------------------------------
//							CXMLOutDateObjectsParser
//----------------------------------------------------------------------------------------------
//
//----------------------------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(CXMLOutDateObjectsParser, CObject)

//----------------------------------------------------------------------------------------------
CXMLOutDateObjectsParser::CXMLOutDateObjectsParser()
{
}

//----------------------------------------------------------------------------------------------
CXMLOutDateObjectsParser::~CXMLOutDateObjectsParser()
{
}

//----------------------------------------------------------------------------------------------
BOOL CXMLOutDateObjectsParser::Load (COutDateObjectsDescription* pDescri, const CTBNamespace& aMoudleNs, CPathFinder* pPathFinder)
{
	CString sFileName = pPathFinder->GetOutDateObjectsFullName(aMoudleNs);
	if (!ExistFile(sFileName))
		return TRUE;

	// da fare una LoadOutDateObjects!!
	CLocalizableXMLDocument aXMLModDoc(aMoudleNs, pPathFinder);
	aXMLModDoc.EnableMsgMode(FALSE);

	if (!aXMLModDoc.LoadXMLFile(sFileName))
		return FALSE;

	return Parse(&aXMLModDoc, pDescri, aMoudleNs);
}

//----------------------------------------------------------------------------------------------
BOOL CXMLOutDateObjectsParser::Parse(
										CXMLDocumentObject*			pDoc, 
										COutDateObjectsDescription* pDescri, 
										const CTBNamespace&			aParent
									)
{
	CString sNodeName;

	CXMLNode* pRoot = pDoc->GetRoot();
	if (!pRoot || !pRoot->GetName(sNodeName) || _tcsicmp(sNodeName, XML_OUTDATEOBJECTS_TAG) != 0) 
	{
		AfxGetDiagnostic()->Add(cwsprintf(_TB("The file OutDateObjects.xml for module {0-%s} has no root element. File not loaded."), 
							(LPCTSTR) aParent.ToString()));
		return FALSE;
	}
	
	// Reports
	CXMLNode* pObjsNode = pRoot->GetChildByName(XML_REPORTS_TAG);
	if (!pObjsNode)
		return TRUE;

	CXMLNode* pObjNode;
	COutDateObjectDescription* pNewDescription;

	if (pObjsNode->GetChilds())
		for (int i=0; i <= pObjsNode->GetChilds()->GetUpperBound(); i++)
		{
			pObjNode = pObjsNode->GetChilds()->GetAt(i);

			if (!pObjNode || !pObjNode->GetName(sNodeName) || sNodeName != XML_REPORT_TAG)
				continue;

			pNewDescription = new COutDateObjectDescription(CTBNamespace::REPORT);

			if (ParseObject(pObjNode, CTBNamespace::REPORT, pNewDescription, aParent))
				pDescri->m_arReportsInfo.Add(pNewDescription);
			else
				delete pNewDescription;
		}

	// ParametersSettings
	pObjsNode = pRoot->GetChildByName(XML_SETTINGS_TAG);
	if (!pObjsNode)
		return TRUE;

	COutDateSettingsSectionDescription* pParamDescri;
	for (int i=0; i <= pObjsNode->GetChilds()->GetUpperBound(); i++)
	{
		pObjNode = pObjsNode->GetChilds()->GetAt(i);

		if (!pObjNode || !pObjNode->GetName(sNodeName) || sNodeName != XML_SECTION_TAG)
			continue;

		pParamDescri = new COutDateSettingsSectionDescription();

		if (ParseParamSection(pObjNode, pParamDescri, aParent))
			pDescri->m_arSettingsInfo.Add(pParamDescri);
		else
			delete pParamDescri;
	}

	return TRUE;
}

//----------------------------------------------------------------------------------------------
BOOL CXMLOutDateObjectsParser::ParseObject 
								(
									CXMLNode* pNode, 
									const CTBNamespace::NSObjectType& aType, 
									COutDateObjectDescription*		pDescri, 
									const CTBNamespace&				aParent
								)
{
	if (!pNode)
		return FALSE;

	CString sTemp;
	if (aType == CTBNamespace::NOT_VALID)
		pNode->GetAttribute(XML_NAME_ATTRIBUTE, sTemp);
	else
		pNode->GetAttribute(XML_NAMESPACE_ATTRIBUTE, sTemp);

	if (sTemp.IsEmpty())
	{
		AfxGetDiagnostic()->Add(cwsprintf(_TB("OutDateObjects.xml for module {0-%s}: attribute <namespace> or <name> not found. Registration ignored."), 
							(LPCTSTR) aParent.ToString()));
		return FALSE;
	}

	if (aType == CTBNamespace::NOT_VALID)
		pDescri->SetName (sTemp);
	else
	{
		CTBNamespace aNamespace (aParent);
		aNamespace.AutoCompleteNamespace (aType, sTemp, aParent);
		pDescri->SetNamespace (aNamespace);
	}

	pNode->GetAttribute(XML_RELEASE_ATTRIBUTE, sTemp);

	if (!sTemp.IsEmpty())
		pDescri->SetRelease(_ttoi((LPCTSTR) sTemp));

	pNode->GetAttribute(XML_OPERATOR_ATTRIBUTE, sTemp);
	
	if (!sTemp.IsEmpty())
		pDescri->SetOperator(StringToOperator(sTemp));

	return TRUE;
}

//----------------------------------------------------------------------------------------------
BOOL CXMLOutDateObjectsParser::ParseParamSection(
													CXMLNode* pNode, 
													COutDateSettingsSectionDescription* pDescri, 
													const CTBNamespace& aParent
												)
{
	// prima la sezione
	if (!pNode || !ParseObject(pNode, CTBNamespace::NOT_VALID, pDescri, aParent))
		return FALSE;

	CString sTemp;
	pNode->GetAttribute(XML_OWNER_ATTRIBUTE, sTemp);
	if (!sTemp.IsEmpty())
		pDescri->SetOwner(sTemp);

	// ora lavoro sui singoli settings
	CXMLNodeChildsList* pChilds = pNode->GetChilds();
	
	// se non ci sono settings si invalida l'intera sezione
	if (!pChilds)
		return TRUE;

	CXMLNode* pSetNode;
	COutDateObjectDescription* pSetDescri;
	for (int i=0; i <= pChilds->GetUpperBound(); i++)
	{
		pSetNode = pChilds->GetAt(i);
		if (!pSetNode || !pSetNode->GetName(sTemp) || sTemp != XML_SETTING_TAG)
			continue;
		
		pSetDescri = new COutDateObjectDescription();
		if (ParseObject(pSetNode, CTBNamespace::NOT_VALID, pSetDescri, aParent))
			pDescri->GetSettings().Add(pSetDescri);
		else
			delete pSetDescri;
	}
	
	return TRUE;
}

//----------------------------------------------------------------------------------------------
CString	CXMLOutDateObjectsParser::OperatorToString (const COutDateObjectDescription::OutDateOperator& ope)
{
	switch (ope)
	{
		case COutDateObjectDescription::LT:	return _T("lt");
		case COutDateObjectDescription::LE:	return _T("le");
		case COutDateObjectDescription::EQ:	return _T("eq");
		case COutDateObjectDescription::GT:	return _T("gt");
		case COutDateObjectDescription::GE:	return _T("ge");
	}
	
	return _T("");
}

//----------------------------------------------------------------------------------------------
COutDateObjectDescription::OutDateOperator CXMLOutDateObjectsParser::StringToOperator(const CString& aStr)
{
	CString s(aStr);
	s.MakeLower ();
	if (_tcsicmp(s, _T("lt")) == 0)	return COutDateObjectDescription::LT;
	if (_tcsicmp(s,_T("le")) == 0)	return COutDateObjectDescription::LE;
	if (_tcsicmp(s,_T("eq")) == 0)	return COutDateObjectDescription::EQ;
	if (_tcsicmp(s,_T("gt")) == 0)	return COutDateObjectDescription::GT;
	if (_tcsicmp(s,_T("ge")) == 0)	return COutDateObjectDescription::GE;

	return COutDateObjectDescription::ND;
}