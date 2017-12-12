#include "stdafx.h"

#include "RIChecker.h"
#include "SqlRec.h"
#include "Sqltable.h"

#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char  THIS_FILE[] = __FILE__;
#endif

static TCHAR szP1[] = _T("p1");

// Tag con cui viene costruito il file xml rappresentante l'albero per la validazione dei dati da mandare ad Infinity
//-----------------------------------------------------------------------------
#define TAG_CHECK_NODE				_T("CheckNode")
#define ATTR_RICHECKER_NAME			_T("name")						// Namespace documento 
#define TAG_CHECKER_CLASS_LIST		_T("RICheckerClassList")		// Array dei checker 
#define TAG_CHECKER_CLASS			_T("RICheckerClass")			// Classe che gestisce il controllo della chiave esterna tra due tabelle
#define ATTR_CHECKER_CLASS_NAME     _T("name")						// Nome della "attuale" della classe RICheckerClass
#define TAG_MASSIVE_QUERY			_T("MassiveQuery")				// Query che verrà utilizzata nella validazione Massiva
#define TAG_SONS					_T("Sons")						// Array dei nodi figli

// Tag con cui viene costruito l'xml rappresentante gli errori di validazione FK e XSD
//-----------------------------------------------------------------------------
#define TAG_VAL_REPORT_ROOT			_T("Validation_Report")
#define TAG_VAL_REPORT_ERRORS		_T("Errors")
#define TAG_VAL_REPORT_ERROR		_T("Error")
#define ATTR_VAL_REPORT_TYPE		_T("Type")

//////////////////////////////////////////////////////////////////////////////////
//				class CheckerClass Implementation					 			//
//////////////////////////////////////////////////////////////////////////////////
//
//-------------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CheckerClass, CObject)

//-------------------------------------------------------------------------------
CheckerClass::CheckerClass()
	:
	m_pRuntimeClass(NULL)
{
}

//-------------------------------------------------------------------------------
CheckerClass::~CheckerClass()
{
}

//////////////////////////////////////////////////////////////////////////////////
//				class ReferentialIntegrityChecker Implementation	 			//
//////////////////////////////////////////////////////////////////////////////////
//
//-------------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(ReferentialIntegrityChecker, CObject)

//-------------------------------------------------------------------------------
ReferentialIntegrityChecker::ReferentialIntegrityChecker()
{
}

//-------------------------------------------------------------------------------
ReferentialIntegrityChecker::~ReferentialIntegrityChecker()
{
}

//////////////////////////////////////////////////////////////////////////////////
//					class RICheckNode Implementation							//
//////////////////////////////////////////////////////////////////////////////////
//
//-------------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(RICheckNode, CObject)

//-------------------------------------------------------------------------------
RICheckNode::RICheckNode()
{
}

//-------------------------------------------------------------------------------
RICheckNode::~RICheckNode()
{
	CheckerClass* pCheckerClass(NULL);

	for (int j = m_RICheckerClassList.GetUpperBound(); j >= 0; j--)
	{
		pCheckerClass = m_RICheckerClassList.GetAt(j);
		SAFE_DELETE(pCheckerClass);
		m_RICheckerClassList.RemoveAt(j);
	}

	RICheckNode* pNode(NULL);

	for (int i = m_Sons.GetUpperBound(); i >= 0; i--)
	{
		pNode = m_Sons.GetAt(i);
		SAFE_DELETE(pNode);

		m_Sons.RemoveAt(i);
	}
}

//-------------------------------------------------------------------------------
BOOL RICheckNode::LookUp(DataStr sName, RICheckNode*& pNode)
{
	if(this->GetName().Str().CompareNoCase(sName.Str()) == 0)
	{
		pNode = this;
		return TRUE;
	}

	int nSonUpperBound = GetSons().GetUpperBound();
	BOOL bFound = FALSE;
	for(int i = 0; i <= nSonUpperBound; i++)
	{
		if(GetSons().GetAt(i)->LookUp(sName, pNode))
			return TRUE;
	}

	return FALSE;
}

//-------------------------------------------------------------------------------
BOOL RICheckNode::IsValid(const SqlRecord* pSqlRec, CStringArray& arMsgTot)
{
	DataStr		 guidDocFind(_T(""));
	CStringArray arMsg;
			
	ReferentialIntegrityChecker* pRIChecker(NULL);
	BOOL bFound(TRUE);
	
	for (int j = 0; j <= GetRICheckerClassList().GetUpperBound(); j++)
	{		
		arMsg.RemoveAll();

		CRuntimeClass* pRTClass = GetRICheckerClassList().GetAt(j)->m_pRuntimeClass;
		pRIChecker = dynamic_cast<ReferentialIntegrityChecker*>(pRTClass->CreateObject());

		if(!pRIChecker)
			continue;

		guidDocFind.Clear();

		bFound &= pRIChecker->FindForValidation(pSqlRec, guidDocFind, arMsg); // per ogni checker posso avere più di un messaggio di errore (es. Currecy di testa e currecy di un Branch)

		for (int i = 0; i < arMsg.GetCount(); i++)
			arMsgTot.Add(arMsg.GetAt(i));
		
		SAFE_DELETE(pRIChecker);
	}

	return bFound;
}

//-------------------------------------------------------------------------------
void RICheckNode::SetXMLNode(CXMLNode* pXMLNode, RICheckNode* pCheckNode, eFilterSetType setType /*SET_EMPTY*/)
{
	if (pXMLNode == NULL || pCheckNode == NULL)	
		return;

	if (pCheckNode->m_RICheckerClassList.GetUpperBound() < 0 && pCheckNode->m_Sons.GetUpperBound() < 0)
		return;
	
	CXMLNode*		pListNode		= pXMLNode->CreateNewChild(TAG_CHECKER_CLASS_LIST);
	CRuntimeClass*	pRuntimeClass	= NULL;
	CString			sQuery			= _T(""); 
	CString			sMasterTable	= _T(""); 

	CXMLNode* pNode = NULL;

	for (int i = 0; i <= pCheckNode->m_RICheckerClassList.GetUpperBound(); i++)
	{
		pRuntimeClass = pCheckNode->m_RICheckerClassList.GetAt(i)->m_pRuntimeClass;
		sQuery	      = pCheckNode->m_RICheckerClassList.GetAt(i)->m_sQuery;
		sMasterTable  = pCheckNode->m_RICheckerClassList.GetAt(i)->m_sMasterTable;

		pNode = pListNode->CreateNewChild(TAG_CHECKER_CLASS);
		pNode->SetAttribute(ATTR_CHECKER_CLASS_NAME, (CString)(pRuntimeClass->m_lpszClassName));
		pNode = pNode->CreateNewChild(TAG_MASSIVE_QUERY);

		if (setType == SET_IN)
			sQuery = sQuery + _T(" ") + cwsprintf(_T("\nAND {0-%s}.TBGuid IN (SELECT DocTBGuid FROM DS_ValidationInfo WHERE UsedForFilter = '1')\n"), sMasterTable);
		else if (setType == SET_NOTIN)
			sQuery = sQuery + _T(" ") + cwsprintf(_T("\nAND {0-%s}.TBGuid NOT IN (SELECT DocTBGuid FROM DS_ValidationInfo WHERE UsedForFilter = '1')\n"), sMasterTable);
		
		pNode->SetText(cwsprintf(_T("\n{0-%s}\n"), sQuery));
	}

	pListNode = pXMLNode->CreateNewChild(TAG_SONS);	
	CXMLNode* pSonNode = NULL;
	DataStr sName = _T("");

	for (int i = 0; i <= pCheckNode->m_Sons.GetUpperBound(); i++)
	{
		sName = pCheckNode->m_Sons.GetAt(i)->GetName();

		if (IsCheckNodeToSkip(pCheckNode->m_Name, sName, m_NamespaceToSkip))
			continue;

		pSonNode = pListNode->CreateNewChild(TAG_CHECK_NODE);
		pSonNode->SetAttribute(ATTR_RICHECKER_NAME, sName.Str());

		if (IsCheckNodeFilter_IN(pCheckNode->m_Name, sName, m_NamespaceFilter_IN))
			setType = SET_IN;
		else if (IsCheckNodeFilter_NOTIN(pCheckNode->m_Name, sName, m_NamespaceFilter_NOTIN))
			setType = SET_NOTIN;
		else
			setType = SET_EMPTY;

		SetXMLNode(pSonNode, pCheckNode->m_Sons.GetAt(i), setType);
	}
}

//-------------------------------------------------------------------------------
BOOL RICheckNode::DoTestCheckNode(DataStr sParentName, DataStr sName, const CStringArray& arNamespace)
{
	if (sParentName != _T("CRMInfinity"))
		return FALSE;

	for (int i = 0; i < arNamespace.GetSize(); i++)
	{
		if (arNamespace.GetAt(i) == sName)
			return TRUE;
	}

	return FALSE;
}

//-------------------------------------------------------------------------------
BOOL RICheckNode::IsCheckNodeToSkip(DataStr sParentName, DataStr sName, const CStringArray& namespaceToSkip)
{
	return DoTestCheckNode(sParentName, sName, namespaceToSkip);
}

//-------------------------------------------------------------------------------
BOOL RICheckNode::IsCheckNodeFilter_IN(DataStr sParentName, DataStr sName, const CStringArray& namespaceFilter_IN)
{
	return DoTestCheckNode(sParentName, sName, namespaceFilter_IN);
}

//-------------------------------------------------------------------------------
BOOL RICheckNode::IsCheckNodeFilter_NOTIN(DataStr sParentName, DataStr sName, const CStringArray& namespaceFilter_NOTIN)
{
	return DoTestCheckNode(sParentName, sName, namespaceFilter_NOTIN);
}

//-------------------------------------------------------------------------------
CString RICheckNode::Serialize()
{
	CString strXML = _T("");

	CXMLDocumentObject* pXMLDoc = new CXMLDocumentObject(TRUE, FALSE);

	m_NamespaceToSkip.		RemoveAll();
	m_NamespaceFilter_IN.	RemoveAll();
	m_NamespaceFilter_NOTIN.RemoveAll();

	DeserializeValidationFilters(m_NamespaceToSkip, m_NamespaceFilter_IN, m_NamespaceFilter_NOTIN);
	
	CXMLNode* pXMLNode = pXMLDoc->CreateRoot(TAG_CHECK_NODE);
	pXMLNode->SetAttribute(ATTR_RICHECKER_NAME, GetName().Str());

	SetXMLNode(pXMLNode, this);

    pXMLDoc->GetXML(strXML);
	
	SAFE_DELETE(pXMLDoc);

	return strXML;
}

//-------------------------------------------------------------------------------
CString RICheckNode::SerializeErrors(const CStringArray& arMsg)
{
	CString strXML = _T("");
	
	CXMLDocumentObject* pDocObj = new CXMLDocumentObject(TRUE, FALSE);
	pDocObj->CreateRoot(TAG_VAL_REPORT_ROOT);

	CXMLNode* pErrors = pDocObj->CreateRootChild(TAG_VAL_REPORT_ERRORS);
	CXMLNode* pError = NULL;

	for (int i = 0; i <= arMsg.GetUpperBound(); i++)
	{
		pError = pErrors->CreateNewChild(TAG_VAL_REPORT_ERROR);
		pError->SetAttribute(ATTR_VAL_REPORT_TYPE, _T("FK"));
		pError->SetText(arMsg.GetAt(i));
	}
	
    pDocObj->GetXML(strXML);
	
	SAFE_DELETE(pDocObj);

	return strXML;
}

//-------------------------------------------------------------------------------
CString RICheckNode::DisplayErrors(const CString& sSerializedErrors)
{
	CString		sErrors  = _T("");
	CXMLNode*	pNode	 = NULL;
	CXMLNode*	pErrNode = NULL;

	CXMLDocumentObject* pDocObj = new CXMLDocumentObject(TRUE,FALSE);
	
	if (!pDocObj->LoadXML(sSerializedErrors))
	{
		SAFE_DELETE(pDocObj);
		return sErrors;
	}

	pNode = pDocObj->GetFirstRootChild();

	if (!pNode)
	{
		SAFE_DELETE(pDocObj);
		return sErrors;
	}

	int		nErrorsNo		= pNode->GetChilds()->GetCount();
	CString sNodeAttribute  = _T("");
	CString sNodeText		= _T("");

	for (int i = 0; i < nErrorsNo; i++)
	{
		pErrNode = pNode->GetChilds()->GetAt(i);
		pErrNode->GetText(sNodeText);
		pErrNode->GetAttribute(ATTR_VAL_REPORT_TYPE, sNodeAttribute);

		sErrors += cwsprintf(_TB("%s: %s\n"), sNodeAttribute, sNodeText);
	}

	SAFE_DELETE(pDocObj);
	return sErrors;
}

// Es. di FiltersExcluded xml
// <?xml version="1.0"?>
// <ValidationFilters>
//		<Query Set ="NOTIN" Namespace="Document.ERP.CustomersSuppliers.Documents.Suppliers">
//			SELECT MA_CustSupp.TBGuid FROM MA_CustSupp LEFT OUTER JOIN MA_CustSuppSupplierOptions ON MA_CustSupp.CustSupp = MA_CustSuppSupplierOptions.Supplier WHERE MA_CustSupp.CustSuppType = 3211265 AND (MA_CustSupp.CustSupp < '0001' OR MA_CustSupp.CustSupp > '0001')
//		</Query>
//		<Query Namespace="Document.ERP.Contacts.Documents.Contacts"></Query>
//		<Query Namespace="Document.ERP.Contacts.Documents.ProspectiveSuppliers"></Query>
//		<Query Namespace="Document.ERP.Items.Documents.Items"></Query>
// </ValidationFilters>
//-------------------------------------------------------------------------------
void RICheckNode::DeserializeValidationFilters(CStringArray& namespaceToSkip, CStringArray& namespaceFilter_IN, CStringArray& namespaceFilter_NOTIN)
{
	namespaceToSkip.RemoveAll();

	CXMLDocumentObject* pDocObj = new CXMLDocumentObject(TRUE,FALSE);
	
	if (!pDocObj->LoadXML(m_xmlValidationFilters))
	{
		SAFE_DELETE(pDocObj);
		return;
	}
		
	int			count = pDocObj->GetRootChilds()->GetCount();
	CXMLNode*	pNode = NULL;

	CString sNodeAttribute			= _T(""); // Namespace
	CString sNodeAttribute_SetType  = _T(""); // Set Type che può avere valore IN / NOTIN
	CString sNodeText				= _T(""); // Query

	for (int i = 0; i < count; i++)
	{
		pNode = pDocObj->GetRootChilds()->GetAt(i);
		pNode->GetAttribute(_T("Namespace"), sNodeAttribute);
		pNode->GetAttribute(_T("Set"), sNodeAttribute_SetType);
		pNode->GetText(sNodeText);
		
		// Il fatto che la query sia vuota significa che devono essere esclusi dalla validazione 
		// tutti i records relativi al namespace indicato (-> il CheckNode corrispondete non deve essere serializzato)
		if (sNodeText.IsEmpty())
			namespaceToSkip.Add(sNodeAttribute);
		else
		{
			if (sNodeAttribute_SetType == _T("NOTIN"))
				namespaceFilter_NOTIN.Add(sNodeAttribute);
			else if (sNodeAttribute_SetType == _T("IN"))
				namespaceFilter_IN.Add(sNodeAttribute);
		}
	}
}

//////////////////////////////////////////////////////////////////////////////////
//					class RICheckNodeFactory Implementation						//
//////////////////////////////////////////////////////////////////////////////////
//
//-------------------------------------------------------------------------------
RICheckNode* RICheckNodeFactory::CreateRoot()
{
	RICheckNode* pRoot = CreateNode();
	pRoot->SetIsRoot();
	return pRoot;
}

//-------------------------------------------------------------------------------
RICheckNode* RICheckNodeFactory::CreateNode()
{
	RICheckNode* pNode = new RICheckNode();
	return pNode;
}

//--------------------------------------------------------------------------------
void RICheckNodeFactory::FillRICheckerStructure(RICheckNode& pParent, RICheckNode*& pCurrNode, const CString& documentNamespace, const CArray<CheckerClass*, CheckerClass*>& rIcheckerList)
{	
	BOOL		 bNamespaceAlreayHandled = FALSE;
	RICheckNode* pSonNode				 = NULL;

	int nSonsUpperBound = pParent.GetSons().GetUpperBound();
	
	for (int i = 0; i <= nSonsUpperBound; i++)
	{
		// Caso in cui il namespace passato (relativo all'anagrafica) è già presente nel tree.
		// In questo caso devo scorrere la lista delle RuntimeClass ad esso associata (lista delle possibili FK da controllare)
		// e se ce ne sono alcune che non sono presenti aggiungerle, altrimeni no.
		pSonNode = pParent.GetSons().GetAt(i);

		if(pSonNode->GetName().Str().CompareNoCase(documentNamespace) == 0)
		{
			bNamespaceAlreayHandled = TRUE;

			for (int k = 0; k <= rIcheckerList.GetUpperBound(); k++) // considero ogni namespace pevedere se già presente o se va inserito
			{
				BOOL bAddNewRuntimeClass = TRUE;

				for (int j = 0; j <= pSonNode->GetRICheckerClassList().GetUpperBound(); j++) // ciclo sulle runtimeclass già presenti nella lista associata al namespace considerato
				{
					if (rIcheckerList.GetAt(k) == pSonNode->GetRICheckerClassList().GetAt(j))
					{
						bAddNewRuntimeClass = FALSE;
						break;
					}
				}

				if (bAddNewRuntimeClass)
					pSonNode->GetRICheckerClassList().Add(rIcheckerList.GetAt(k));
			}
		}
	}

	if(bNamespaceAlreayHandled)
		return;

	RICheckNode* pNode = RICheckNodeFactory::CreateNode();
	pNode->SetName(documentNamespace);

	for (int i = 0; i <= rIcheckerList.GetUpperBound(); i++)
		pNode->GetRICheckerClassList().Add(rIcheckerList.GetAt(i));
	
	pParent.GetSons().Add(pNode);

	pCurrNode = pNode;
}

//--------------------------------------------------------------------------------
void RICheckNodeFactory::FillRICheckerStructure(RICheckNode& pParent)
{	
	RICheckNode*					pNode	(NULL);
	ReferentialIntegrityChecker*	pChecker(NULL);
	DataStr							sName	(_T(""));

	TRY {
		for (int k = 0; k <= pParent.GetRICheckerClassList().GetUpperBound(); k++)
		{
			pChecker = dynamic_cast<ReferentialIntegrityChecker*>(pParent.GetRICheckerClassList().GetAt(k)->m_pRuntimeClass->CreateObject());
			sName = pChecker->GetDocumentNamespace();

			pNode = RICheckNodeFactory::CreateNode();
			pNode->SetName(sName);

			pParent.GetSons().Add(pNode);

			SAFE_DELETE(pChecker);
		}
	}
	CATCH (CException, e)
	{ 
		return; 
	}
	END_CATCH

}

//===============================================================================
RICheckNode* AfxGetRICheckNodeManager(CString pProviderName)
{
	RICheckNode* pRoot = AfxGetApplicationContext()->GetObject<RICheckNode>();

	if(pRoot && !pRoot->IsRoot())
		pRoot->SetIsRoot();

	RICheckNode* pProviderNode(NULL);

	if(!pRoot->LookUp(pProviderName, pProviderNode))
	{
		pProviderNode = RICheckNodeFactory::CreateNode();
		pProviderNode->SetName(pProviderName);
		pRoot->GetSons().Add(pProviderNode);
	}

	return pProviderNode;
}