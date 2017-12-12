#include "stdafx.h"

#include <io.h>

#include <TbNameSolver\Diagnostic.h>
#include <TbNameSolver\Chars.h>

#include <TbXmlCore\XMLTags.h>
#include <TbXmlCore\XMLDocObj.h>
#include <TbXmlCore\XMLParser.h>

#include <TBGeneric\FunctionObjectsInfo.h>
#include <TBGeneric\GeneralFunctions.h>

#include "TokensTable.h"
#include "XmlFunctionObjectsParser.h"

//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char THIS_FILE[] = __FILE__;
#endif

//--------------------------------------------------------------------------
int CompareFunDescr(CObject* pObj1, CObject* pObj2)
{
	CFunctionDescription* pd1 = (CFunctionDescription*)pObj1;
	CFunctionDescription* pd2 = (CFunctionDescription*)pObj2;

	return pd1->GetName().CompareNoCase(pd2->GetName());
}

//----------------------------------------------------------------------------------------------
//							CXMLFunctionObjectsParser
//----------------------------------------------------------------------------------------------
//
//----------------------------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(CXMLFunctionObjectsParser, CObject)

//----------------------------------------------------------------------------------------------
CXMLFunctionObjectsParser::CXMLFunctionObjectsParser()
	:
	m_NSType (CTBNamespace::FUNCTION)
{
}
//----------------------------------------------------------------------------------------------
BOOL CXMLFunctionObjectsParser::Parse	(
											CXMLDocumentObject*				pDoc, 
											CFunctionObjectsDescription*	parDescri, 
											const CTBNamespace&				aParent,
											BOOL							bSkipDuplicate
										)
{
	CString sNodeName;

	// root
	CXMLNode* pRoot = pDoc->GetRoot();

	// Groups
	CXMLNode* pFunsNode = pRoot->GetChildByName(XML_FUNCTIONS_TAG);
	if (!pFunsNode)
	{
		pFunsNode = pRoot->GetChildByName(XML_EVENTS_TAG);
		if (!pFunsNode)
			return TRUE;
	}

	return Parse1(pFunsNode, parDescri, aParent, bSkipDuplicate);
}

//----------------------------------------------------------------------------------------------
BOOL CXMLFunctionObjectsParser::Parse1	(
											CXMLNode*						pGroupNode, 
											CFunctionObjectsDescription*	parDescri, 
											const CTBNamespace&				aParent,
											BOOL							bSkipDuplicate
										)
{
	CXMLNode* pFunNode;
	CString sNodeName;

	if (!pGroupNode->GetChilds())
		return TRUE;

	for (int i=0; i <= pGroupNode->GetChilds()->GetUpperBound(); i++)
	{
		pFunNode = pGroupNode->GetChilds()->GetAt(i);

		if (!pFunNode || !pFunNode->GetName(sNodeName) || (sNodeName != XML_FUNCTION_TAG && sNodeName != XML_EVENT_TAG))
			continue;

		Parse2(pFunNode, pGroupNode,parDescri,aParent,bSkipDuplicate);
	}

	return TRUE;
}

//----------------------------------------------------------------------------------------------
CFunctionDescription* CXMLFunctionObjectsParser::NewFuncDescr(CTBNamespace::NSObjectType aNSType)
{
	return new CFunctionDescription(aNSType);
}

//----------------------------------------------------------------------------------------------
BOOL CXMLFunctionObjectsParser::Parse2(
	CXMLNode*						pFunNode,
	CXMLNode*						pGroupNode,
	CFunctionObjectsDescription*	parDescri,
	const CTBNamespace&				aParent,
	BOOL							bSkipDuplicate
	)
{
	CFunctionDescription* pNewDescription;
	CString sNodeName; 
	pFunNode->GetName(sNodeName);

		BOOL bIsEvent = FALSE;

		if (sNodeName == XML_FUNCTION_TAG)
		{
			CString sPublished;
			BOOL bPublished = TRUE;
			if (pFunNode->GetAttribute(XML_PUBLISHED_ATTRIBUTE, sPublished))
			{
				if (_tcsicmp(sPublished, XML_FALSE_VALUE) == 0)
					bPublished = FALSE;
			}
			else if (pFunNode->GetAttribute(XML_REPORT_ATTRIBUTE, sPublished))
			{
				if (_tcsicmp(sPublished, XML_FALSE_VALUE) == 0)
					bPublished = FALSE;
			}

			CString sServerAttribute;
			pFunNode->GetAttribute(XML_SERVER_ATTRIBUTE, sServerAttribute);
			CString sServiceAttribute;
			pFunNode->GetAttribute(XML_SERVICE_ATTRIBUTE, sServiceAttribute);

			int nPort = 80;
			CString sServicePortAttribute;
			if (pFunNode->GetAttribute(XML_SERVICEPORT_ATTRIBUTE, sServicePortAttribute) && !sServicePortAttribute.IsEmpty())
			{
				nPort = _wtoi(sServicePortAttribute);
				if (nPort == 0)
				{
					ASSERT(FALSE);
					nPort = 80;
				}
			}

			pNewDescription = NewFuncDescr(m_NSType);

			pNewDescription->SetPublished(bPublished);
			pNewDescription->SetServer(sServerAttribute);
			pNewDescription->SetService(sServiceAttribute);
			pNewDescription->SetPort(nPort);

			pFunNode->GetAttribute(XML_GROUP_ATTRIBUTE, pNewDescription->m_strGroup);
		}
		else
		{
			bIsEvent = TRUE;
			pNewDescription = NewFuncDescr(m_NSType);
		}

		if (pNewDescription->ParsePrototype(pFunNode, aParent))
		{
			CFunctionDescription* pExists = NULL;
			if (bIsEvent || !pNewDescription->GetNamespace().IsValid())
				pExists = parDescri->GetFunctionInfo(pNewDescription->GetName());
			else
				pExists = parDescri->GetFunctionInfo(pNewDescription->GetNamespace());

			if (pExists)
			{
				if (!bSkipDuplicate)
				{
					TRACE(_T("\nDuplicate webmethods %s from %s\n"), pNewDescription->GetNamespace().ToString(), aParent.ToString());
					ASSERT(FALSE);	//TODO emit error
				}
				delete pNewDescription;
			}
			else
				parDescri->AddFunction(pNewDescription);
		}
		else
			delete pNewDescription;

	return TRUE;
}


//----------------------------------------------------------------------------------------------
void CXMLFunctionObjectsParser::Unparse (CXMLDocumentObject* pDoc, CFunctionObjectsDescription* pDescri)
{
	// prima creo la root
	CXMLNode* pRoot = pDoc->CreateRoot(XML_FUNCTIONOBJ_TAG);
	if (!pRoot)
	{
		ASSERT(FALSE);
		return;
	}

	CXMLNode* pNewNode = pRoot->CreateNewChild(XML_FUNCTIONS_TAG);

	// funzioni
	CFunctionDescription* pDescription;
	CXMLNode* pFunNode;
	for (int i=0; i <= pDescri->GetFunctions().GetUpperBound(); i++)
	{
		pFunNode = pNewNode->CreateNewChild(XML_FUNCTION_TAG);
		pDescription = (CFunctionDescription*) pDescri->GetFunctions().GetAt(i);
		pDescription->UnparsePrototype(pFunNode);
	}
}

//----------------------------------------------------------------------------------------------
void CXMLFunctionObjectsParser::SetFunctionType	(const CTBNamespace::NSObjectType aType)
{	
	m_NSType = aType;
}

//----------------------------------------------------------------------------------------------
//							CInternalFunctionObjectsParser
//----------------------------------------------------------------------------------------------
//
//----------------------------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(CInternalFunctionObjectsParser, CXMLFunctionObjectsParser)

//----------------------------------------------------------------------------------------------
CInternalFunctionObjectsParser::CInternalFunctionObjectsParser()
	:
	CXMLFunctionObjectsParser()
{
}

//----------------------------------------------------------------------------------------------
CInternalFunctionObjectsParser::~CInternalFunctionObjectsParser()
{
	for (int i = 0; i < m_arFunctionGroups.GetSize(); i++)
	{
		CGroupFunctions* pGroup = m_arFunctionGroups.GetAt(i);
		SAFE_DELETE(pGroup);
	}
}

//----------------------------------------------------------------------------------------------
BOOL ParseCData
(
	CXMLNode* pFunNode,
	CString attrib,
	CString& str
)
{
	CXMLNode* pTmpNode = pFunNode->GetChildByName(attrib);
	if (pTmpNode)
	{
		pTmpNode->GetText(str);

		if (str.IsEmpty())
		{
			CXMLNodeChildsList* pCDATANodes = pTmpNode->GetChildsByType(NODE_CDATA_SECTION);
			if (pCDATANodes && pCDATANodes->GetSize() > 0)
			{
				CXMLNode* pCDATANode = pCDATANodes->GetAt(0);
				if (!pCDATANode || !pCDATANode->GetNodeValue(str) || str.IsEmpty())
				{
					//ASSERT_TRACE2(FALSE, "Wrong dynamic hotlink mode script named %s (%s)", sName, sType);
					ASSERT(FALSE);
					return FALSE;
				}
			}
			SAFE_DELETE(pCDATANodes);
		}
		return TRUE;
	}
	return FALSE;
}

BOOL CInternalFunctionObjectsParser::Parse1
			(
				CXMLNode*						pGroupNode,
				CFunctionObjectsDescription*	parDescri,
				const CTBNamespace&				aParent,
				BOOL							bSkipDuplicate
			)
{
	CXMLNode* pFunNode;
	CXMLNode* pGroupBlockNode;
	CString sNodeName;
	CString sFunctionNodeName;
	CString sGroupNodeName;
	CString sGroupNodeTitle;

	CString sRemarksAttribute;
	CString sResultAttribute;
	CString sPrototypeAttribute;
	CString sExampleAttribute;

	if (!pGroupNode->GetChilds())
		return TRUE;

	for (int i = 0; i <= pGroupNode->GetChilds()->GetUpperBound(); i++)
	{
		pGroupBlockNode = pGroupNode->GetChilds()->GetAt(i);
		
		if (!pGroupBlockNode || !pGroupBlockNode->GetName(sNodeName) || sNodeName != _T("Group"))
			return FALSE;	
		
		pGroupBlockNode->GetAttribute(_T("name"), sGroupNodeName);
		pGroupBlockNode->GetAttribute(_T("localize"), sGroupNodeTitle);

		// Se il gruppo non ha il nome allora lo salto
		if (sGroupNodeName.IsEmpty())
			continue;

		for (int j = 0; j <= pGroupBlockNode->GetChilds()->GetUpperBound(); j++)
		{
			pFunNode = pGroupBlockNode->GetChilds()->GetAt(j);

			pFunNode->GetAttribute(_T("name"), sFunctionNodeName);

			// Se la funzione non ha il nome allora la salto
			if (sFunctionNodeName.IsEmpty())
				continue;

			if (Parse2(pFunNode, pGroupNode, parDescri, aParent, bSkipDuplicate))
			{
				CDecoratedFunctionDescription * pFunDesc = dynamic_cast<CDecoratedFunctionDescription*>(parDescri->GetFunctionInfo(sFunctionNodeName));
				if (pFunDesc)
				{
					ASSERT(AfxGetTokensTable()->GetKeywordsToken(pFunDesc->GetName()) != Token::T_NOTOKEN);
					pFunDesc->SetName(sFunctionNodeName);

					sRemarksAttribute.Empty();
					sResultAttribute.Empty();
					sPrototypeAttribute.Empty();
					sExampleAttribute.Empty();

					ParseCData(pFunNode, _T("Remarks"), sRemarksAttribute);
					ParseCData(pFunNode, _T("Example"), sExampleAttribute);
					ParseCData(pFunNode, _T("Prototype"), sPrototypeAttribute);
					ParseCData(pFunNode, _T("Result"), sResultAttribute);

					pFunDesc->SetRemarks(sRemarksAttribute);
					pFunDesc->SetResult(sResultAttribute);
					pFunDesc->SetExample(sExampleAttribute);
					pFunDesc->SetPrototype(sPrototypeAttribute);
				}
				else return FALSE;
			}
		}

		parDescri->m_arFunctions.SetCompareFunction(CompareFunDescr);
		parDescri->m_arFunctions.QuickSort();

		m_arFunctionGroups.Add(new CGroupFunctions(sGroupNodeName, sGroupNodeName, parDescri));
		if (i != pGroupNode->GetChilds()->GetUpperBound())
			parDescri = new CFunctionObjectsDescription();
	}

	return TRUE;
}

//----------------------------------------------------------------------------------------------
CFunctionDescription* CInternalFunctionObjectsParser::NewFuncDescr(CTBNamespace::NSObjectType aNSType)
{
	return new CDecoratedFunctionDescription(aNSType);
}

//----------------------------------------------------------------------------------------------
BOOL CInternalFunctionObjectsParser::Load()
{
	BOOL bOk;
	CString strMsg;
	const CTBNamespace snsTbGenlib = _T("Module.Framework.TbGenlib");

	CFunctionObjectsDescription * parDescri = new CFunctionObjectsDescription();
	CString strRemoteXMLFile = AfxGetPathFinder()->GetModuleXmlPath(snsTbGenlib, CPathFinder::STANDARD) + SLASH_CHAR +
		_T("Functions.xml");

	if (!ExistFile(strRemoteXMLFile))
	{
		strMsg = cwsprintf(_TB("Unable to open XML file {0-%s}"), strRemoteXMLFile);
		AfxGetDiagnostic()->Add(strMsg, CDiagnostic::Warning);
		return FALSE;
	}
	CLocalizableXMLDocument aDocument(snsTbGenlib, AfxGetPathFinder());
	if (!aDocument.LoadXMLFile(strRemoteXMLFile))
		return FALSE;

	const CTBNamespace snsTbNull = _T("");
	try
	{
		bOk = Parse(&aDocument, parDescri, snsTbNull, TRUE);
	}
	catch (CException* e) // Catch all MFC exceptions, including COleExceptions.
	{
		TCHAR szError[1024];
		strMsg = cwsprintf(_TB("Unable to parse XML file {0-%s}"), strRemoteXMLFile);
		e->GetErrorMessage(szError, 1024);
		TRACE(szError);
		AfxGetDiagnostic()->Add(strMsg + _T("\r\n") + szError, CDiagnostic::Warning);
		e->Delete();

		bOk = FALSE;
	}
	return bOk;
}