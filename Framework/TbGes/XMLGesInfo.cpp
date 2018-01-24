
#include "stdafx.h"
#include <io.h>

#include <TbXmlCore\XMLDocObj.h>
#include <TbXmlCore\XMLParser.h>
#include <TbXmlCore\XMLTags.h>
#include <TbXmlCore\XMLGeneric.h>

#include <TbClientCore\ClientObjects.h>
#include <TbWebServicesWrappers\LoginManagerInterface.h>

#include <TbGeneric\DataObj.h>
#include <TbGeneric\globals.h>

#include <TbGenlib\generic.h>
#include <TbGenlib\extres.hjson> //JSON AUTOMATIC UPDATE
#include <TbGenlib\XMLModuleObjectsInfo.h>
#include <TbGenlib\BaseApp.h>
#include <TbGenlib\AddOnMng.h>

#include <TbOledb\sqltable.h>
#include <TbOledb\sqlcatalog.h>
#include <TbOledb\oledbmng.h>

#include <TbWoormEngine\ActionsRepEngin.h>
#include <TbWoormEngine\ruledata.h>
#include <TbWoormEngine\rpsymtbl.h>

#include <TbFrameworkImages\CommonImages.h>
#include <TbFrameworkImages\GeneralFunctions.h>

#include "hotlink.h"
#include "tabber.h"
#include "extdoc.h"
#include "dbt.h"
#include "XMLControls.hjson"

//includere come ulo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char THIS_FILE[] = __FILE__;
#endif


static const TCHAR szBaseSelected[] = _T("BaseSelected");
static const TCHAR szBaseFromKey[] = _T("BaseFromKey");
static const TCHAR szBaseToKey	[] = _T("BaseToKey");

static const TCHAR szTableName	[]	= _T("[TableName]");

#define ADDON_IMAGE_IDX		0
#define MODULE_IMAGE_IDX	1
#define DOCUMENT_IMAGE_IDX  2



//----------------------------------------------------------------------------------------------
UINT CompareNamespaceTagValue(CXMLNode* pNode, LPCTSTR lpszNamespace)
{
	if (!pNode || !lpszNamespace) return FALSE;
	
	CString strNodeName;
	pNode->GetName(strNodeName);
	
	CXMLNode* pNSNode = strNodeName.CompareNoCase(XML_NAMESPACE_TAG) ? pNode->GetChildByName(XML_NAMESPACE_TAG) : pNode;

	CString	strNamespace;
	if 
		(
			!pNSNode ||
			!pNSNode->GetText(strNamespace) ||
			strNamespace.IsEmpty()
		)
		return NS_CMP_NOT_FOUND;

	if ( !strNamespace.CompareNoCase(lpszNamespace))
		return NS_CMP_IDENTICAL;

	return NS_CMP_DIFFERENT;	
}

//----------------------------------------------------------------------------------------------
UINT CompareNamespaceTagValue(CXMLNode* pNode, const CTBNamespace& ns)
{
	return CompareNamespaceTagValue(pNode, (LPCTSTR)ns.ToUnparsedString());
}

//----------------------------------------------------------------------------------------------
BOOL CompareNamespaceAttributeValue(CXMLNode* pNode, LPCTSTR lpszNamespace)
{
	if (!pNode || !lpszNamespace) return FALSE;

	CString	strNamespace;
	if 
		(
			!pNode->GetAttribute(XML_NAMESPACE_ATTRIBUTE, strNamespace) ||
			strNamespace.IsEmpty()
		)
		return NS_CMP_NOT_FOUND;

	if ( !strNamespace.CompareNoCase(lpszNamespace) )
		return NS_CMP_IDENTICAL;

	return NS_CMP_DIFFERENT;	
}

//----------------------------------------------------------------------------------------------
BOOL CompareNamespaceAttributeValue(CXMLNode* pNode, const CTBNamespace& ns)
{
	return CompareNamespaceAttributeValue (pNode, (LPCTSTR)ns.ToUnparsedString());
}

//----------------------------------------------------------------------------------------------
//	CXMLDefaultInfo implementation
//----------------------------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(CXMLDefaultInfo, CObject)
//----------------------------------------------------------------------------------------------
CXMLDefaultInfo::CXMLDefaultInfo(CXMLDocInfo* pDocInfo /* = NULL */)
	:
	m_bIsLoaded	(FALSE)
{

	SetDocumentNamespace(pDocInfo ? pDocInfo->GetNamespaceDoc() : CTBNamespace());
}

//----------------------------------------------------------------------------------------------
CXMLDefaultInfo::CXMLDefaultInfo(const CTBNamespace& nsDoc)
	:
	m_bIsLoaded	(FALSE)
{
	SetDocumentNamespace(nsDoc);
}

//----------------------------------------------------------------------------------------------
CXMLDefaultInfo::CXMLDefaultInfo(const CXMLDefaultInfo& aDefaultInfo)
{
	*this = aDefaultInfo;
}

//----------------------------------------------------------------------------------------------
void CXMLDefaultInfo::SetDocumentNamespace (const CTBNamespace& nsDoc)
{
	if (!nsDoc.IsValid())
	{
		Clear();
		return;
	}
	// il file Defaults.xml è personalizzabile dall'utente per cui bisogna prima cercare l'esistenza del file
	// nella custom dell'utente, se non esiste nella AllUsers ed infine nella Standard

	// Se cambia il namespace di documento occorre aggiornare il nome del file
	// contenente le impostazioni di default
	m_nsDoc = nsDoc;
	
	SetFileName ();
	m_strPrefProfile.Empty();
}

//----------------------------------------------------------------------------------------------
void CXMLDefaultInfo::SetFileName ()
{

	m_strFileName = AfxGetPathFinder()->GetDocumentDefaultsFile(m_nsDoc, CPathFinder::USERS, AfxGetLoginInfos()->m_strUserName);
	if (ExistFile(m_strFileName))
		return;

	m_strFileName = AfxGetPathFinder()->GetDocumentDefaultsFile(m_nsDoc, CPathFinder::ALL_USERS);
	if (ExistFile(m_strFileName))
		return;

	//const CDocumentDescription* pDocDescri = AfxGetDocumentDescription(m_nsDoc);

	// dynamic document has description in custom directory or in standard
	m_strFileName = AfxGetPathFinder()->GetDocumentDefaultsFile(m_nsDoc, CPathFinder::CUSTOM);
	if (ExistFile(m_strFileName))
		return;

	m_strFileName = AfxGetPathFinder()->GetDocumentDefaultsFile(m_nsDoc, CPathFinder::STANDARD);
}

//----------------------------------------------------------------------------------------------
void CXMLDefaultInfo::Clear() 
{
	m_nsDoc.Clear();
	m_strFileName.Empty();
	m_strPrefProfile.Empty();
}

//-------------------------------------------------------------------------------------------
BOOL CXMLDefaultInfo::Parse(CPathFinder::PosType ePosType, const CString& strUserRole) 
{
	SetFileName ();
	return Parse();
}

//-------------------------------------------------------------------------------------------
BOOL CXMLDefaultInfo::Parse()
{
	m_bIsLoaded = FALSE;

	if (m_strFileName.IsEmpty() || !ExistFile(m_strFileName))
		return TRUE;

	CLocalizableXMLDocument aXMLDefDoc(m_nsDoc, AfxGetPathFinder());
	aXMLDefDoc.EnableMsgMode(FALSE);

	if (!aXMLDefDoc.LoadXMLFile(m_strFileName))
		return FALSE;

	CXMLNode* pRoot = aXMLDefDoc.GetRoot();
	if (!pRoot)
		return FALSE;

	CXMLNode* pProfNode;
	if (pProfNode = pRoot->GetChildByName(XML_DEFAULT_PROFILE_TAG))
		pProfNode->GetAttribute(XML_PREFERRED_PROFILE_TAG, m_strPrefProfile);

	m_bIsLoaded = TRUE;
	return TRUE;
}

//---------------------------------------------------------------------------------
BOOL CXMLDefaultInfo::UnParse(CPathFinder::PosType ePosType, const CString& strUserRole)
{
	m_strFileName = AfxGetPathFinder()->GetDocumentDefaultsFile(m_nsDoc, ePosType, strUserRole, TRUE);
	return UnParse();
}

//---------------------------------------------------------------------------------
BOOL CXMLDefaultInfo::UnParse()
{
	if (m_strFileName.IsEmpty())
		return FALSE;

	CLocalizableXMLDocument aXMLDefDoc(m_nsDoc, AfxGetPathFinder());
				
	CXMLNode* pnRoot = aXMLDefDoc.CreateRoot(XML_DEFAULT_ROOT);

	if(!pnRoot)
		return FALSE;
	
	CXMLNode* pnDocumentNs = pnRoot->CreateNewChild(XML_NAMESPACE_DOC_TAG);
	pnDocumentNs->SetText(m_nsDoc.ToUnparsedString());

	CXMLNode* pnProfile	 = pnRoot->CreateNewChild(XML_DEFAULT_PROFILE_TAG);
	
	pnProfile->SetAttribute(XML_PREFERRED_PROFILE_TAG, m_strPrefProfile);

	aXMLDefDoc.SaveXMLFile(m_strFileName, TRUE);

	return TRUE;
}

//----------------------------------------------------------------------------------------------
BOOL CXMLDefaultInfo::SetPreferredProfile(const CString & strPrefPRofile)
{
	// cerca caratteri invalidi nel nome
	if (strPrefPRofile.FindOneOf(_T("\\/:* ?<>|")) >= 0)
		return FALSE;
	
	m_strPrefProfile = strPrefPRofile;
	return TRUE;
}

//----------------------------------------------------------------------------------------------
CXMLDefaultInfo& CXMLDefaultInfo::operator =(const CXMLDefaultInfo& aDefaultInfo)
{
	if (this == &aDefaultInfo)
		return *this;
	
	m_nsDoc				= aDefaultInfo.m_nsDoc;
	m_strFileName		= aDefaultInfo.m_strFileName;
	m_strPrefProfile	= aDefaultInfo.m_strPrefProfile;
	m_bIsLoaded			= aDefaultInfo.m_bIsLoaded;

	return *this;
}

//------------------------------------------------------------------------------
BOOL CXMLDefaultInfo::operator == (const CXMLDefaultInfo& aDefaultInfo) const
{
	return IsEqual(aDefaultInfo);
}

//------------------------------------------------------------------------------
BOOL CXMLDefaultInfo::operator != (const CXMLDefaultInfo& aDefaultInfo) const
{
	return !(*this == aDefaultInfo);
}

//----------------------------------------------------------------------------------------------
BOOL CXMLDefaultInfo::IsEqual(const CXMLDefaultInfo& aDefaultInfo) const
{
	if (this == &aDefaultInfo)
		return TRUE;
	
	return
		(
			m_nsDoc			 == aDefaultInfo.m_nsDoc			&&
			m_strFileName	 == aDefaultInfo.m_strFileName		&&
			m_strPrefProfile == aDefaultInfo.m_strPrefProfile
		);
}

//////////////////////////////////////////////////////////////////////////////
//					CXMLAppCriteriaDlgElem implementation
//////////////////////////////////////////////////////////////////////////////
//
//
IMPLEMENT_DYNAMIC (CXMLAppCriteriaDlgElem, CObject)

//----------------------------------------------------------------------------
CXMLAppCriteriaDlgElem::CXMLAppCriteriaDlgElem(CRuntimeClass* pRuntimeClass, UINT nIDD)
:
	m_pAppDlgRuntimeClass	(pRuntimeClass),
	m_nIDD					(nIDD)
{
}

//----------------------------------------------------------------------------
CXMLAppCriteriaDlgElem::CXMLAppCriteriaDlgElem(const CXMLAppCriteriaDlgElem& aExpCriteriaDlgElem)
{
	Assign(aExpCriteriaDlgElem);
}

//----------------------------------------------------------------------------
void CXMLAppCriteriaDlgElem::Assign(const CXMLAppCriteriaDlgElem& aExpCriteriaDlgElem)
{
	if (this == &aExpCriteriaDlgElem)
		return;
	
	m_pAppDlgRuntimeClass	= aExpCriteriaDlgElem.m_pAppDlgRuntimeClass;	
	m_nIDD					= aExpCriteriaDlgElem.m_nIDD;
}

//----------------------------------------------------------------------------
BOOL CXMLAppCriteriaDlgElem::IsEqual(const CXMLAppCriteriaDlgElem& aExpCriteriaDlgElem) const
{
	if (this == &aExpCriteriaDlgElem)
		return TRUE;
	
	return 
		(
			m_pAppDlgRuntimeClass == aExpCriteriaDlgElem.m_pAppDlgRuntimeClass &&
			m_nIDD				  == aExpCriteriaDlgElem.m_nIDD
		);
}			

//----------------------------------------------------------------------------
CXMLAppCriteriaDlgElem&	CXMLAppCriteriaDlgElem::operator = (const CXMLAppCriteriaDlgElem& aExpCriteriaDlgElem)
{
	if (this == &aExpCriteriaDlgElem)
		return *this;

	Assign(aExpCriteriaDlgElem);
	return *this;
}

//////////////////////////////////////////////////////////////////////////////
//					CXMLAppCriteriaDlgArray Implementation
//////////////////////////////////////////////////////////////////////////////
//
IMPLEMENT_DYNCREATE (CXMLAppCriteriaDlgArray, Array)

//----------------------------------------------------------------------------------------------
BOOL CXMLAppCriteriaDlgArray::IsEqual(const CXMLAppCriteriaDlgArray& aDlgArray) const
{
	if (this == &aDlgArray)
		return TRUE;
	
	if(aDlgArray.GetSize() != GetSize())
		return FALSE;
	
	for(int i = 0 ; i < GetSize() ; i++)
	{
		if (!GetAt(i) && aDlgArray.GetAt(i))
			return FALSE;
		
		if(!GetAt(i)->IsEqual(*aDlgArray.GetAt(i)))
			return FALSE;
	}

	return TRUE;
}

//----------------------------------------------------------------------------
CXMLAppCriteriaDlgArray& CXMLAppCriteriaDlgArray::operator= (const CXMLAppCriteriaDlgArray& aDlgArray)
{
	if (this != &aDlgArray)
	{
		RemoveAll();
		for (int i=0; i < aDlgArray.GetSize(); i++)
			Add(new CXMLAppCriteriaDlgElem(*aDlgArray.GetAt(i)));
	}
	return *this;
}

/////////////////////////////////////////////////////////////////////////////
// 				class CXMLBaseAppCriteria Implementation
/////////////////////////////////////////////////////////////////////////////

//----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CXMLBaseAppCriteria, CObject)

//----------------------------------------------------------------------------
CXMLBaseAppCriteria::CXMLBaseAppCriteria ()
	:
	m_pVariablesArray			(NULL),
	m_pXMLAppCriteriaDlgArray	(NULL),
	m_pDocument					(NULL),
	m_bSelected					(FALSE)
{
	m_FromKey.SetUpperCase();
	m_ToKey.SetUpperCase();

	m_pVariablesArray = new CXMLVariableArray();
}

//----------------------------------------------------------------------------
CXMLBaseAppCriteria::~CXMLBaseAppCriteria ()
{
	SAFE_DELETE(m_pVariablesArray);
	SAFE_DELETE(m_pXMLAppCriteriaDlgArray);
}

//----------------------------------------------------------------------------
void CXMLBaseAppCriteria::AttachDocument(CBaseDocument* pDocument)
{
	ASSERT(pDocument);
	m_pDocument = pDocument;
}

//----------------------------------------------------------------------------
void CXMLBaseAppCriteria::DeclareVariable(const CString& sName, DataObj* pDataObj)
{
	if (!m_pVariablesArray)
		m_pVariablesArray = new CXMLVariableArray();
	m_pVariablesArray->Add(sName, pDataObj);
}
//----------------------------------------------------------------------------
void CXMLBaseAppCriteria::DeclareVariables()
{
	DECLARE_VAR(szBaseSelected, &m_bSelected);
	DECLARE_VAR(szBaseFromKey,	&m_FromKey	);
	DECLARE_VAR(szBaseToKey,	&m_ToKey	);
	
	OnDeclareVariables(); 	
}

//----------------------------------------------------------------------------
void CXMLBaseAppCriteria::Customize() 
{
	DeclareVariables();
	OnCustomize();
}

//----------------------------------------------------------------------------
UINT CXMLBaseAppCriteria::GetFirstDialogIDD()
{
	if (
			m_pXMLAppCriteriaDlgArray && 
			m_pXMLAppCriteriaDlgArray->GetSize() > 0 &&
			m_pXMLAppCriteriaDlgArray->GetAt(0)
		)
		return m_pXMLAppCriteriaDlgArray->GetAt(0)->GetIDD();
	
	return 0;
}

//----------------------------------------------------------------------------
UINT CXMLBaseAppCriteria::GetLastDialogIDD()
{
	int nLast;
	if (
			m_pXMLAppCriteriaDlgArray && 
			m_pXMLAppCriteriaDlgArray->GetSize() > 0
		)
	{
		nLast = m_pXMLAppCriteriaDlgArray->GetUpperBound();
		if(m_pXMLAppCriteriaDlgArray->GetAt(nLast))
			return m_pXMLAppCriteriaDlgArray->GetAt(nLast)->GetIDD();
	}	

	return 0;
}

//----------------------------------------------------------------------------
void CXMLBaseAppCriteria::AddAppCriteriaTabDlg(CRuntimeClass* pRTClass, UINT nIDD)
{
	if (!m_pXMLAppCriteriaDlgArray)
		m_pXMLAppCriteriaDlgArray = new CXMLAppCriteriaDlgArray;

	m_pXMLAppCriteriaDlgArray->Add(new CXMLAppCriteriaDlgElem(pRTClass, nIDD));
}

//----------------------------------------------------------------------------
BOOL CXMLBaseAppCriteria::CreateAppExpCriteriaTabDlgs(CTabManager* pTabMng, int nPos)
{
	if (!m_pXMLAppCriteriaDlgArray)
		return FALSE;

	// vuol dire che l'ho già creato
	if (
			GetFirstDialogIDD() > 0 && 
			pTabMng->GetTabDialogPos(GetFirstDialogIDD()) > -1
		)
		return TRUE;

	for (int i = 0; i <= m_pXMLAppCriteriaDlgArray->GetUpperBound(); i++)
	{
		if (m_pXMLAppCriteriaDlgArray->GetAt(i))
			pTabMng->AddDialog
							(
								m_pXMLAppCriteriaDlgArray->GetAt(i)->GetRTDialogClass(), 
								m_pXMLAppCriteriaDlgArray->GetAt(i)->GetIDD(), 
								++nPos
							);
	}
	return TRUE;
}

//----------------------------------------------------------------------------
void CXMLBaseAppCriteria::SetKeyLength (int nNewLength)
{
	if (nNewLength <= 0)
		return;

	m_FromKey.	SetAllocSize (nNewLength);
	m_ToKey.	SetAllocSize (nNewLength);
}

//----------------------------------------------------------------------------
BOOL CXMLBaseAppCriteria::IsEqual(const CXMLBaseAppCriteria& aBaseCriteria) const
{	
	if (this == &aBaseCriteria)
		return TRUE;
	
	if (m_pVariablesArray->GetSize() == aBaseCriteria.m_pVariablesArray->GetSize())
		return FALSE;

	if (!m_pVariablesArray->IsEqual(*aBaseCriteria.m_pVariablesArray))
			return FALSE;
	
	return ((bool)m_bSelected != (bool)aBaseCriteria.m_bSelected);
}

//----------------------------------------------------------------------------------------------
BOOL CXMLBaseAppCriteria::Parse(CXMLNode* pNode, CAutoExpressionMng* pAutoExpressionMng /*= NULL*/)
{
	if (!pNode || !m_pVariablesArray)
		return FALSE;
	
	if (pNode->GetChildsNum() <= 0) 
		return TRUE;
	
	CXMLNode*				pVarNode;
	CXMLVariable*	pVariable;
	CString					strTagName;	
	CString					strValue;

	if(pAutoExpressionMng)
		pAutoExpressionMng->RemoveAll();

	if (m_pDocument)
	{
		for (int nIdx = 0; nIdx <= m_pVariablesArray->GetUpperBound(); nIdx++)		
		{
			pVariable = m_pVariablesArray->GetAt(nIdx);
			if (pVariable)
			{
				pVarNode = pNode->GetChildByName(pVariable->GetName());
				if (pVarNode)
				{
					pVarNode->GetText(strValue);
					pVariable->SetDataObjValue(strValue);

					//se devono essere gestite le auto expression riempio l'array con i valori parsati
					if(pAutoExpressionMng)
					{
						CString strExpression;
						pVarNode->GetAttribute(XML_AUTO_EXPR_EXPR_ATTRIBUTE, strExpression);

						if(!strExpression.IsEmpty() && m_pDocument->GetBaseExportCriteria()->GetVariablesArray())
						{
							CString strVarName;
							for(int i = 0 ; i < m_pDocument->GetBaseExportCriteria()->GetVariablesArray()->GetSize(); i++)
							{
								if(m_pDocument->GetBaseExportCriteria()->GetVariablesArray()->GetAt(i)->GetDataObj() == pVariable->GetDataObj())
								{
									strVarName = m_pDocument->GetBaseExportCriteria()->GetVariablesArray()->GetAt(i)->GetName();
									break;
								}
							}

							if(!strVarName.IsEmpty())
							{
								CAutoExpressionData* pAutoExpressionData = new CAutoExpressionData(strExpression, strVarName, pVariable->GetDataObj());
								pAutoExpressionMng->EvaluateExpression(strExpression, pVariable->GetDataObj());								
								pAutoExpressionMng->Add(pAutoExpressionData);
							}
						}
					}
				}
			}		
		}
		return TRUE;
	}
	
	// vuol dire che sono nell'editor dei profili del formmanager e non sono in fase di 
	// esportazione
	m_pVariablesArray->RemoveAll();		
	for (int i =0; i < pNode->GetChildsNum(); i++)
	{
		pVarNode = pNode->GetChildAt(i);
		if (pVarNode)
		{
			pVarNode->GetName(strTagName);
			pVarNode->GetText(strValue);
			if (!strTagName.IsEmpty())
				m_pVariablesArray->Add(new CXMLVariable(strTagName, strValue));
		}
	}
	return TRUE;
}

//----------------------------------------------------------------------------------------------
BOOL CXMLBaseAppCriteria::Unparse(CXMLNode* pNode, CAutoExpressionMng* pAutoExpressionMng /*= NULL*/)
{
	if (!pNode)
		return FALSE;

	CXMLVariable*	pVariable;
	CXMLNode*				pVarNode;

	for ( int nIdx = 0 ; nIdx <= m_pVariablesArray->GetUpperBound() ; nIdx++)
	{	
		pVariable = m_pVariablesArray->GetAt(nIdx);
		
		if (pVariable && !pVariable->GetName().IsEmpty())
		{
			pVarNode = pNode->CreateNewChild(pVariable->GetName());
			if (m_pDocument)
			{
				//se il documento fa uso di autoexpression chiedo al manager l'espressione 
				//in base al nome var, se la trova aggiunge un attributo all'xml con la descrizione
				//della stessa
				if(pAutoExpressionMng)
				{
					CString strExpression = pAutoExpressionMng->GetExpressionByVarName(pVariable->GetName());
					
					if(!strExpression.IsEmpty())
						pVarNode->SetAttribute(XML_AUTO_EXPR_EXPR_ATTRIBUTE, strExpression);
				}

				pVarNode->SetText(pVariable->GetDataObjValue());
			}
			else
				pVarNode->SetText(pVariable->GetXMLValue());
		
		}
	}
	return TRUE;
}

//----------------------------------------------------------------------------------------------
CXMLBaseAppCriteria& CXMLBaseAppCriteria::operator = (const CXMLBaseAppCriteria& aBaseCriteria)
{
	if (this == &aBaseCriteria)
		return *this;

	ASSERT(m_pVariablesArray->GetSize() == aBaseCriteria.m_pVariablesArray->GetSize());
 
	m_bSelected = aBaseCriteria.m_bSelected;
	*m_pVariablesArray = *aBaseCriteria.m_pVariablesArray;

	return *this;	
}
    
//----------------------------------------------------------------------------------------------
// CXMLUniversalKey
//----------------------------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(CXMLUniversalKey, CObject)

//----------------------------------------------------------------------------------------------
CXMLUniversalKey::CXMLUniversalKey()
{

}

//----------------------------------------------------------------------------------------------
CXMLUniversalKey::CXMLUniversalKey(const CStringArray& aSegmentArray)
{
	m_SegmentArray.RemoveAll();
	for(int i = 0 ; i < aSegmentArray.GetSize() ; i++)
		m_SegmentArray.Add(aSegmentArray.GetAt(i));
}

//----------------------------------------------------------------------------------------------
CXMLUniversalKey::CXMLUniversalKey(CXMLUniversalKey& aXMLUniversalKey)
{
	SetName(aXMLUniversalKey.GetName());

	m_SegmentArray.RemoveAll();
	for(int i = 0 ; i < aXMLUniversalKey.m_SegmentArray.GetSize() ; i++)
		m_SegmentArray.Add(aXMLUniversalKey.m_SegmentArray.GetAt(i));
}

//----------------------------------------------------------------------------------------------
BOOL CXMLUniversalKey::IsEqual(const CXMLUniversalKey& aXMLUniversalKey) const
{
	if (this == &aXMLUniversalKey)
		return TRUE;
	
	if(aXMLUniversalKey.m_SegmentArray.GetSize() != m_SegmentArray.GetSize() || aXMLUniversalKey.GetName().CompareNoCase(GetName()) != 0)
		return FALSE;

	for(int i = 0 ; i < aXMLUniversalKey.m_SegmentArray.GetSize() ; i++)
	{
		if(aXMLUniversalKey.m_SegmentArray.GetAt(i).CompareNoCase(m_SegmentArray.GetAt(i)) != 0)
			return FALSE;
	}

	return TRUE;
}



//----------------------------------------------------------------------------------------------
CXMLUniversalKey& CXMLUniversalKey::operator = (const CXMLUniversalKey& aXMLUniversalKey)
{
	if (this == &aXMLUniversalKey)
		return *this;
	
	m_strName = aXMLUniversalKey.GetName();

	m_SegmentArray.RemoveAll();
	for(int i = 0 ; i < aXMLUniversalKey.m_SegmentArray.GetSize() ; i++)
		m_SegmentArray.Add(aXMLUniversalKey.m_SegmentArray.GetAt(i));

	return *this;
}

//---------------------------------------------------------------------------------
BOOL CXMLUniversalKey::IsUniversalKeySegment(const CString& strColumnName)
{
    //scorro i segmenti della universal key
	for(int iSeg = 0 ; iSeg < GetSegmentNumber() ; iSeg++)
	{
		if (strColumnName.CompareNoCase(GetSegmentAt(iSeg)) == 0)
			return TRUE;
	}
	return FALSE;	
}

//----------------------------------------------------------------------------------------------
BOOL CXMLUniversalKey::Parse(CXMLNode* pnUniversalKey)
{
	if (!pnUniversalKey)
		return FALSE;
	
	pnUniversalKey->GetAttribute(XML_UNIVERSAL_KEY_NAME_ATTRIBUTE, m_strName); 

	//prendo i segmenti della universal key
	for(int iSeg = 0 ; iSeg < pnUniversalKey->GetChildsNum() ; iSeg++)
	{
		CXMLNode* pnUKSegment = pnUniversalKey->GetChildAt(iSeg);
		if(pnUKSegment)
		{
			CString strSegName;
			pnUKSegment->GetAttribute(XML_UNIVERSAL_KEY_SEGNAME_ATTRIBUTE, strSegName);
			AddSegment(strSegName);	
		}		
	}

	return TRUE;
}

//----------------------------------------------------------------------------------------------
BOOL CXMLUniversalKey::UnParse(CXMLNode* pnUniversalKey)
{
	if(!pnUniversalKey)
		return FALSE;

	pnUniversalKey->SetAttribute(XML_UNIVERSAL_KEY_NAME_ATTRIBUTE, m_strName);

	//prendo i segmenti della universal key
	for(int iSeg = 0 ; iSeg < GetSegmentNumber() ; iSeg++)
	{
		CXMLNode* pnUKSegment = pnUniversalKey->CreateNewChild(XML_UNIVERSAL_KEY_SEGMENT_TAG);
		if(pnUKSegment)
			pnUKSegment->SetAttribute(XML_UNIVERSAL_KEY_SEGNAME_ATTRIBUTE, GetSegmentAt(iSeg));
	}

	return TRUE;
}

//----------------------------------------------------------------------------------------------
//CXMLUniversalKeyGroup
//----------------------------------------------------------------------------------------------

IMPLEMENT_DYNAMIC(CXMLUniversalKeyGroup, Array)

//----------------------------------------------------------------------------------------------
CXMLUniversalKeyGroup::CXMLUniversalKeyGroup()
	:
	m_bExportData(TRUE)
{

}

//----------------------------------------------------------------------------------------------
CXMLUniversalKeyGroup::CXMLUniversalKeyGroup(const CXMLUniversalKeyGroup& aUniversalKeyArray)
	:
	m_bExportData(TRUE)
{
	RemoveAll();

	for(int i = 0 ; i < aUniversalKeyArray.GetSize() ; i++)
	{
		if(aUniversalKeyArray.GetAt(i))
			Add(new CXMLUniversalKey(*aUniversalKeyArray.GetAt(i)));
	}

	m_strFuncion	= aUniversalKeyArray.m_strFuncion;
	m_strTableName	= aUniversalKeyArray.m_strTableName;
	m_bExportData	= aUniversalKeyArray.m_bExportData;
}

//----------------------------------------------------------------
CXMLUniversalKey* CXMLUniversalKeyGroup::GetUKByName(const CString& strUKName) const
{
	for (int i = 0 ; i < GetSize() ; i++)
	{
		CXMLUniversalKey* pUKTmp = GetAt(i);
		if ( pUKTmp->GetName().CompareNoCase(strUKName) == 0 )
			return pUKTmp;
	}
	
	return NULL;
}

//----------------------------------------------------------------
BOOL CXMLUniversalKeyGroup::Parse(CXMLNode* pnUniversalKeys) 
{
	if (!pnUniversalKeys)
		return FALSE;
	
	RemoveAll();
	pnUniversalKeys->GetAttribute(XML_UNIVERSAL_KEY_FUNCNAME_ATTRIBUTE, m_strFuncion);
	pnUniversalKeys->GetAttribute(XML_UNIVERSAL_KEY_TABLENAME_ATTRIBUTE, m_strTableName);
	
	CString strTmp;
	pnUniversalKeys->GetAttribute(XML_UNIVERSAL_KEY_EXPORT_ATTRIBUTE, strTmp);
	m_bExportData = GetBoolFromXML(strTmp);

	//prendo tutti i nodi universal key
	for(int iUk = 0 ; iUk < pnUniversalKeys->GetChildsNum() ; iUk++)
	{
		CXMLNode* pnUniversalKey = pnUniversalKeys->GetChildAt(iUk);
		if(pnUniversalKey)
		{
			//parsing del nodo UK e segmenti
			CXMLUniversalKey* pXMLUniversalKey = new CXMLUniversalKey;
			pXMLUniversalKey->Parse(pnUniversalKey);
			Add(pXMLUniversalKey);
		}
	}

	return TRUE;
}

//---------------------------------------------------------------------------------
BOOL CXMLUniversalKeyGroup::UnParse(CXMLNode* pnUniversalKeys)
{
	if (!pnUniversalKeys)
		return FALSE;

	pnUniversalKeys->SetAttribute(XML_UNIVERSAL_KEY_FUNCNAME_ATTRIBUTE, m_strFuncion);
	
	if(!m_strTableName.IsEmpty())
	{
		pnUniversalKeys->SetAttribute(XML_UNIVERSAL_KEY_TABLENAME_ATTRIBUTE, m_strTableName);
	
		pnUniversalKeys->SetAttribute(XML_UNIVERSAL_KEY_EXPORT_ATTRIBUTE, FormatBoolForXML(m_bExportData));
	}

	//prendo tutti i nodi universal key
	for(int iUk = 0 ; iUk < GetSize() ; iUk++)
	{
		CXMLUniversalKey* pUniversalKey = GetAt(iUk);
		if(pUniversalKey)
		{
			CXMLNode* pnUniversalKey = pnUniversalKeys->CreateNewChild(XML_UNIVERSAL_KEY_TAG);
			pUniversalKey->UnParse(pnUniversalKey);
		}
	}

	return TRUE;
}

//---------------------------------------------------------------------------------
BOOL CXMLUniversalKeyGroup::Remove(CXMLUniversalKey* pXMLUniversalKey)
{
	if(!pXMLUniversalKey)
		return FALSE;
	
	for (int i = 0 ; i < GetSize() ; i++)
	{
		CXMLUniversalKey* pUKTmp = GetAt(i);
		if (*pXMLUniversalKey != *pUKTmp)
			continue;
		
		RemoveAt(i);
		return TRUE;
	}

	return FALSE;
}

//dato il nome di una colonna itera sulle UK definite nel gruppo
// e restituisce TRUE se è un segmento di almeno una di esse
//---------------------------------------------------------------------------------
BOOL CXMLUniversalKeyGroup::IsUniversalKeySegment(const CString& strColumnName)
{
	for (int i = 0 ; i < GetSize() ; i++)
	{
		CXMLUniversalKey* pUKTmp = GetAt(i);
		if ( pUKTmp->IsUniversalKeySegment(strColumnName))
			return TRUE;
	}

	return FALSE;
}


//---------------------------------------------------------------------------------
BOOL CXMLUniversalKeyGroup::IsPresent(const CString& strUKName)
{
	for (int i = 0 ; i < GetSize() ; i++)
	{
		CXMLUniversalKey* pUKTmp = GetAt(i);
		if ( pUKTmp->GetName().CompareNoCase(strUKName) == 0 )
			return TRUE;
	}
	
	return FALSE;
}

//----------------------------------------------------------------------------------------------
BOOL CXMLUniversalKeyGroup::IsEqual(const CXMLUniversalKeyGroup& aUniversalKeyArray) const
{
	if (this == &aUniversalKeyArray)
		return TRUE;
	
	if	(
		aUniversalKeyArray.GetSize() != GetSize()								|| 
		m_strFuncion.CompareNoCase(aUniversalKeyArray.m_strFuncion) != 0		||
		m_strTableName.CompareNoCase(aUniversalKeyArray.m_strTableName) != 0	||
		m_bExportData != aUniversalKeyArray.m_bExportData
		)
		return FALSE;
	
	for(int i = 0 ; i < GetSize() ; i++)
	{
		CXMLUniversalKey* pUniversalKey = GetAt(i);
		if (!pUniversalKey || !aUniversalKeyArray.GetAt(i))
			return FALSE;
		
		if(pUniversalKey == aUniversalKeyArray.GetAt(i) || *pUniversalKey != *aUniversalKeyArray.GetAt(i))
			return FALSE;
	}

	return TRUE;
}

//----------------------------------------------------------------------------------------------
CXMLUniversalKeyGroup& CXMLUniversalKeyGroup::operator =(const CXMLUniversalKeyGroup& aUniversalKeyArray)
{
	if (this == &aUniversalKeyArray)
		return *this;
	
	m_strFuncion	= aUniversalKeyArray.m_strFuncion;
	m_strTableName	= aUniversalKeyArray.m_strTableName;
	m_bExportData	= aUniversalKeyArray.m_bExportData;

	RemoveAll();

	for(int i = 0 ; i < aUniversalKeyArray.GetSize() ; i++)
	{
		if(aUniversalKeyArray.GetAt(i))
			Add(new CXMLUniversalKey(*aUniversalKeyArray.GetAt(i)));
	}
	return *this;
}


//----------------------------------------------------------------------------------------------
// CXMLSegmentInfo
//----------------------------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(CXMLFieldInfo, CObject)
//----------------------------------------------------------------------------------------------
CXMLFieldInfo::CXMLFieldInfo(LPCTSTR lpszFieldName /*= NULL*/, BOOL bExport /*= TRUE*/)
:
	m_bExport	(bExport)
{
	if(lpszFieldName)
		m_strFieldName = lpszFieldName;
}

//----------------------------------------------------------------------------------------------
CXMLFieldInfo::CXMLFieldInfo(CXMLFieldInfo& aField)
{
	m_strFieldName = aField.GetFieldName();
	m_bExport = aField.IsToExport();
}

//----------------------------------------------------------------------------------------------
BOOL CXMLFieldInfo::IsEqual(const CXMLFieldInfo& aFieldInfo) const
{
	if (this == &aFieldInfo)
		return TRUE;
	
	return(!m_strFieldName.CompareNoCase(aFieldInfo.GetFieldName()) && m_bExport == aFieldInfo.IsToExport());
}

//----------------------------------------------------------------------------------------------
CXMLFieldInfo& CXMLFieldInfo::operator = (const CXMLFieldInfo& aFieldInfo)
{
	if (this == &aFieldInfo)
		return *this;
	
	m_strFieldName = aFieldInfo.GetFieldName();
	m_bExport = aFieldInfo.IsToExport();

	return *this;
}

//----------------------------------------------------------------------------------------------
void CXMLFieldInfo::SetFieldName(const CString& strFieldName)
{
	m_strFieldName = strFieldName;
}

//----------------------------------------------------------------------------------------------
void CXMLFieldInfo::SetExport(BOOL bExport /*= TRUE*/)
{
	m_bExport = bExport;
}


//----------------------------------------------------------------------------------------------
//CXMLFieldInfoArray
//----------------------------------------------------------------------------------------------
CXMLFieldInfoArray::CXMLFieldInfoArray()
{

}

//----------------------------------------------------------------------------------------------
CXMLFieldInfoArray::CXMLFieldInfoArray(const CXMLFieldInfoArray& aSegArray)
{
	RemoveAll();

	for(int i = 0 ; i < aSegArray.GetSize() ; i++)
	{
		if(aSegArray.GetAt(i))
			Add(new CXMLFieldInfo(*aSegArray.GetAt(i)));
	}
}

//----------------------------------------------------------------------------------------------
BOOL CXMLFieldInfoArray::IsEqual(const CXMLFieldInfoArray& aSegArray) const
{
	if (this == &aSegArray)
		return TRUE;
	
	if(aSegArray.GetSize() != GetSize())
		return FALSE;
	
	for(int i = 0 ; i < GetSize() ; i++)
	{
		CXMLFieldInfo* pSegInfo = GetAt(i);
		if (!pSegInfo || !aSegArray.GetAt(i))
			return FALSE;
		
		if(pSegInfo == aSegArray.GetAt(i) || *pSegInfo != *aSegArray.GetAt(i))
			return FALSE;
	}

	return TRUE;
}

//----------------------------------------------------------------------------------------------
CXMLFieldInfoArray& CXMLFieldInfoArray::operator =(const CXMLFieldInfoArray& aSegArray)
{
	if (this == &aSegArray)
		return *this;
	
	RemoveAll();

	for(int i = 0 ; i < aSegArray.GetSize() ; i++)
	{
		if(aSegArray.GetAt(i))
			Add(new CXMLFieldInfo(*aSegArray.GetAt(i)));
	}
	return *this;
}

//----------------------------------------------------------------
CXMLFieldInfo* 	CXMLFieldInfoArray::GetFieldByName(const CString& strFieldName)
{
	CXMLFieldInfo* pXMLFieldInfo = NULL;
	for(int i = 0 ; i < GetSize() ; i++)
	{
		pXMLFieldInfo = GetAt(i);
		if(pXMLFieldInfo && pXMLFieldInfo->GetFieldName().CompareNoCase(strFieldName) == 0)
			return pXMLFieldInfo;
	}

	return NULL;
}


//----------------------------------------------------------------
BOOL CXMLFieldInfoArray::HasFieldsToExport() const
{
	CXMLFieldInfo* pXMLField = NULL;
	for (int i = 0; i < GetSize(); i++)
	{
		pXMLField = GetAt(i);
		if (pXMLField && pXMLField->IsToExport())
			return TRUE;
	}
	
	return FALSE;
}

//----------------------------------------------------------------
BOOL CXMLFieldInfoArray::IsToExport(const CString& strFieldName)
{
	CXMLFieldInfo* pXMLField = GetFieldByName(strFieldName);
	return (pXMLField && pXMLField->IsToExport());
}

//----------------------------------------------------------------
BOOL CXMLFieldInfoArray::Parse(CXMLNode* pnFields) 
{
	if (!pnFields)
		return FALSE;

	RemoveAll();

	CXMLNode* pnField = NULL;
	for(int i = 0 ; i < pnFields->GetChildsNum() ; i++)
	{
		pnField = pnFields->GetChildAt(i);
		if(!pnField)
			continue;
		
		CString strFieldName;
		CString strFieldExp;
		BOOL bExp = TRUE;
		pnField->GetAttribute(XML_FIELD_NAME_TAG, strFieldName);
		pnField->GetAttribute(XML_FIELD_EXPORT_TAG, strFieldExp);
		
		CXMLFieldInfo* pXMLFieldInfo = new CXMLFieldInfo(strFieldName, GetBoolFromXML(strFieldExp));

		Add(pXMLFieldInfo);
	}

	return TRUE;
}

//---------------------------------------------------------------------------------
BOOL CXMLFieldInfoArray::UnParse(CXMLNode* pnFields)
{
	CXMLNode* pnField = NULL;
	for(int i = 0 ; i < GetSize() ; i++)
	{
		CXMLFieldInfo* pFieldInfo = GetAt(i);

		// the file contains only exported fields
		if(!pFieldInfo || !pFieldInfo->IsToExport())
			continue;

		pnField = pnFields->CreateNewChild(XML_FIELD_TAG);

		if(!pnField)
			continue;

		pnField->SetAttribute(XML_FIELD_NAME_TAG, pFieldInfo->GetFieldName());
		pnField->SetAttribute(XML_FIELD_EXPORT_TAG, FormatBoolForXML(pFieldInfo->IsToExport()));
	}

	return TRUE;
}

//----------------------------------------------------------------------------------------------
// CXMLSegmentInfo
//----------------------------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(CXMLSegmentInfo, CObject)
//----------------------------------------------------------------------------------------------
CXMLSegmentInfo::CXMLSegmentInfo
					(
						CXMLXRefInfo*	pXRefInfo				/*= NULL*/,
						LPCTSTR			lpszFKSegment			/*= NULL*/,
						LPCTSTR			lpszReferencedSegment	/*= NULL*/,
						LPCTSTR			lpszFKFixedValue		/*= NULL*/,
						BOOL			bOwnXRef				/*= FALSE*/ 
					)
	:
	m_bOwnXRef (bOwnXRef)
{
	m_bOwnXRef = bOwnXRef;
	SetXRef(pXRefInfo);
	SetKeySegments (lpszFKSegment, lpszReferencedSegment, lpszFKFixedValue);
}

//----------------------------------------------------------------------------------------------
CXMLSegmentInfo::CXMLSegmentInfo(CXMLSegmentInfo& aSeg, BOOL bOwnXRef /*= FALSE*/)
:
	m_bOwnXRef (bOwnXRef)
{
	SetXRef(aSeg.m_pXRefInfo);
	SetKeySegments ((LPCTSTR)aSeg.m_strFKSegment, (LPCTSTR)aSeg.m_strReferencedSegment, (LPCTSTR)aSeg.m_strFKFixedValue);
}

//----------------------------------------------------------------------------------------------
void CXMLSegmentInfo::SetXRef(CXMLXRefInfo* pXRefInfo)
{
	if (m_bOwnXRef && pXRefInfo)
	{
		if (!m_pXRefInfo)
			m_pXRefInfo = new CXMLXRefInfo(*pXRefInfo);
		else
			*m_pXRefInfo = *pXRefInfo;
	}
	else
		m_pXRefInfo	= pXRefInfo;
}

//----------------------------------------------------------------------------------------------
BOOL CXMLSegmentInfo::SetKeySegments
						(
							LPCTSTR	lpszFKSegment,
							LPCTSTR	lpszReferencedSegment,
							LPCTSTR	lpszFKFixedValue,
							BOOL	bCheckColumns	/*= FALSE*/
						)
{
	if (bCheckColumns)
	{
		if 
			(
				!m_pXRefInfo ||
				m_pXRefInfo->GetTableName().IsEmpty() ||
				!lpszFKSegment || !lpszFKSegment[0] ||
				m_pXRefInfo->GetReferencedTableName().IsEmpty() ||
				!lpszReferencedSegment || !lpszReferencedSegment[0]
			)
			return FALSE;

		SqlTableInfo* pTableInfo = AfxGetDefaultSqlConnection()->GetTableInfo(m_pXRefInfo->GetTableName());

		if (!pTableInfo)
			return FALSE;
		const SqlColumnInfo*	pFKInfo = pTableInfo->GetColumnInfo(lpszFKSegment);

		SqlTableInfo* pReferencedTableInfo = AfxGetDefaultSqlConnection()->GetTableInfo(m_pXRefInfo->GetReferencedTableName());		
		if (!pReferencedTableInfo)
			return FALSE;
		const SqlColumnInfo*	pPKInfo = pReferencedTableInfo->GetColumnInfo(lpszReferencedSegment);

		if
		(
			!pFKInfo ||
			!pPKInfo ||
			pFKInfo->GetDataObjType() != pPKInfo->GetDataObjType() ||
			(pFKInfo->GetDataObjType().m_wType == DATA_STR_TYPE && pFKInfo->m_lPrecision != pPKInfo->m_lPrecision)
		)
			return FALSE;
	}

	m_strFKSegment	= lpszFKSegment ? lpszFKSegment : _T("");

	m_strReferencedSegment = lpszReferencedSegment ? lpszReferencedSegment : _T("");

	m_strFKFixedValue = lpszFKFixedValue ? lpszFKFixedValue : _T("");

	return TRUE;
}

//----------------------------------------------------------------------------------------------
BOOL CXMLSegmentInfo::IsEqual(const CXMLSegmentInfo& aSegInfo) const
{
	if (this == &aSegInfo)
		return TRUE;
	
	return
		(
			m_strFKSegment == aSegInfo.m_strFKSegment &&
			m_strReferencedSegment == aSegInfo.m_strReferencedSegment
		);
}

//----------------------------------------------------------------------------------------------
CXMLSegmentInfo& CXMLSegmentInfo::operator = (const CXMLSegmentInfo& aSegInfo)
{
	if (this == &aSegInfo)
		return *this;
	
	
	m_strFKSegment = aSegInfo.m_strFKSegment;
	m_strReferencedSegment = aSegInfo.m_strReferencedSegment;
	m_strFKFixedValue = aSegInfo.m_strFKFixedValue;

	return *this;
}

#define XML_FIXED_FOREIGN_KEY_VALUE_ATTRIBUTE _T("fixedValue")
//----------------------------------------------------------------------------------------------
BOOL CXMLSegmentInfo::Parse(CXMLNode* pNode)
{
	if (!pNode)
		return FALSE;
	
	CXMLNode* pInfoNode;
	pInfoNode = pNode->GetChildByName(XML_FOREIGN_KEYSEG_TAG);
	if (pInfoNode)
	{
		pInfoNode->GetText(m_strFKSegment);
		pInfoNode->GetAttribute(XML_FIXED_FOREIGN_KEY_VALUE_ATTRIBUTE, m_strFKFixedValue);
	}

	pInfoNode = pNode->GetChildByName(XML_PRIMARY_KEYSEG_TAG);
	if (pInfoNode)
		pInfoNode->GetText(m_strReferencedSegment);

	return TRUE;
};

//----------------------------------------------------------------------------------------------
BOOL CXMLSegmentInfo::UnParse(CXMLNode* pNode)
{
	if(!pNode)
		return FALSE;

	CXMLNode* pnReference = pNode->CreateNewChild(XML_FOREIGN_KEYSEG_TAG);
	CXMLNode* pnReferenced = pNode->CreateNewChild(XML_PRIMARY_KEYSEG_TAG);

	pnReference->SetText(GetFKSegment());
	if (!GetFKStrFixedValue().IsEmpty())
		pnReference->SetAttribute(XML_FIXED_FOREIGN_KEY_VALUE_ATTRIBUTE, GetFKStrFixedValue());
	pnReferenced->SetText(GetReferencedSegment());	

	return TRUE;
}

//----------------------------------------------------------------------------------------------
//CXMLSegmentInfoArray
//----------------------------------------------------------------------------------------------
CXMLSegmentInfoArray::CXMLSegmentInfoArray()
{

}

//----------------------------------------------------------------------------------------------
CXMLSegmentInfoArray::CXMLSegmentInfoArray(CXMLSegmentInfoArray& aSegArray)
{
	RemoveAll();

	for(int i = 0 ; i < aSegArray.GetSize() ; i++)
	{
		if(aSegArray.GetAt(i))
			Add(new CXMLSegmentInfo(*aSegArray.GetAt(i)));
	}
}

//----------------------------------------------------------------------------------------------
BOOL CXMLSegmentInfoArray::IsEqual(const CXMLSegmentInfoArray& aSegArray) const
{
	if (this == &aSegArray)
		return TRUE;
	
	if(aSegArray.GetSize() != GetSize())
		return FALSE;
	
	for(int i = 0 ; i < GetSize() ; i++)
	{
		CXMLSegmentInfo* pSegInfo = GetAt(i);
		if (!pSegInfo && aSegArray.GetAt(i))
			return FALSE;
		
		if(pSegInfo == aSegArray.GetAt(i) || *pSegInfo != *aSegArray.GetAt(i))
			return FALSE;
	}

	return TRUE;
}

//----------------------------------------------------------------------------------------------
CXMLSegmentInfoArray& CXMLSegmentInfoArray::operator =(const CXMLSegmentInfoArray& aSegArray)
{
	if (this == &aSegArray)
		return *this;
	
	RemoveAll();

	for(int i = 0 ; i < aSegArray.GetSize() ; i++)
	{
		if(aSegArray.GetAt(i))
			Add(new CXMLSegmentInfo(*aSegArray.GetAt(i)));
	}
	return *this;
}

//----------------------------------------------------------------------------------------------
void CXMLSegmentInfoArray::SetSegmentsXRef(CXMLXRefInfo* pXRefInfo)
{
	for(int i = 0 ; i < GetSize() ; i++)
	{
		CXMLSegmentInfo* pSegInfo = GetAt(i);
		if (!pSegInfo)
			continue;

		pSegInfo->SetXRef(pXRefInfo);
	}
	return;
}

//----------------------------------------------------------------------------------------------
BOOL CXMLSegmentInfoArray::IsFkPresent(const CString& strSegmentFk)
{
	if(strSegmentFk.IsEmpty())
		return FALSE;

	for(int i = 0 ; i < GetSize() ; i++)
	{
		CXMLSegmentInfo* pSegInfo = GetAt(i);
		if (!pSegInfo)
			continue;

		if(pSegInfo->GetFKSegment().CompareNoCase(strSegmentFk) == 0)
			return TRUE;
	}

	return FALSE;
}

//----------------------------------------------------------------------------------------------
//CXMLDBTData
//----------------------------------------------------------------------------------------------
CXMLDBTData::CXMLDBTData (const CString& strNs, const CString& strTitle,const CString& strTableNs)
{
	CTBNamespace aDbtNs (CTBNamespace::DBT, strNs);
	CTBNamespace aTableNs (CTBNamespace::TABLE, strTableNs);

	m_strNs			= aDbtNs.ToString();
	m_strTitle		= strTitle;
	m_strTableNs	= aTableNs.ToString();
}

//----------------------------------------------------------------------------------------------
//CXMLXRefInfo
//----------------------------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(CXMLXRefInfo, CObject)
//----------------------------------------------------------------------------------------------
CXMLXRefInfo::CXMLXRefInfo(LPCTSTR lpszTableName)
	:
	m_pSymTable				(NULL),
	m_bMustExist			(TRUE),
	m_bCanbeNull			(TRUE),
	m_bNoDocQuery			(FALSE),
	m_bSubjectTo			(FALSE),
	m_pXMLUniversalKeyGroup	(NULL),
	m_bUse					(TRUE),
	m_bOldUse				(FALSE),
	m_bOwnedByDoc			(TRUE),
	m_bIsAppended			(FALSE),
	m_bModified				(FALSE),
	m_lBookmark				(0)
{
	m_strTableName = lpszTableName ? lpszTableName : _T("");
}
	
//----------------------------------------------------------------------------------------------
CXMLXRefInfo::CXMLXRefInfo(CXMLXRefInfo& aXRef)
{
	m_strName				= aXRef.m_strName;
	m_bNoDocQuery			= aXRef.m_bNoDocQuery;
	m_bMustExist			= aXRef.m_bMustExist;
	m_bCanbeNull			= aXRef.m_bCanbeNull;
	m_strUrlDati			= aXRef.m_strUrlDati;
	m_strProfile			= aXRef.m_strProfile;
	m_nsDoc					= aXRef.m_nsDoc;
	m_SegmentsArray			= aXRef.m_SegmentsArray;
	m_strTableName			= aXRef.m_strTableName;
	m_strReferencedTableName= aXRef.m_strReferencedTableName;
	m_nsReferencedDBT		= aXRef.m_nsReferencedDBT;
	m_nsReferencedTable		= aXRef.m_nsReferencedTable;
	m_bSubjectTo			= aXRef.m_bSubjectTo;
	m_strExpression			= aXRef.m_strExpression;
	m_bUse					= aXRef.m_bUse;
	m_bOldUse				= aXRef.m_bOldUse;
	m_bOwnedByDoc			= aXRef.m_bOwnedByDoc;	
	m_bIsAppended			= aXRef.m_bIsAppended;
	m_bModified				= aXRef.m_bModified;
	m_pSymTable				= NULL;
	m_pXMLUniversalKeyGroup	= NULL;
	
	if (aXRef.m_pXMLUniversalKeyGroup)
		m_pXMLUniversalKeyGroup = new CXMLUniversalKeyGroup(*aXRef.m_pXMLUniversalKeyGroup);
}

//----------------------------------------------------------------------------------------------
CXMLXRefInfo::~CXMLXRefInfo()
{
	RemoveSymTable();

	if(m_pXMLUniversalKeyGroup)
	{
		delete m_pXMLUniversalKeyGroup;
		m_pXMLUniversalKeyGroup = NULL;
	}
}

//----------------------------------------------------------------------------------------------
BOOL CXMLXRefInfo::IsEqual(const CXMLXRefInfo& aXRef) const
{
	if (this == &aXRef)
		return TRUE;
	
	if(
		(m_pXMLUniversalKeyGroup && !aXRef.m_pXMLUniversalKeyGroup) ||
		(!m_pXMLUniversalKeyGroup && aXRef.m_pXMLUniversalKeyGroup)
	  )
		return FALSE;

	return
		(m_strName					== aXRef.m_strName)					&&
		(m_bNoDocQuery				== aXRef.m_bNoDocQuery)				&&
		(m_bMustExist				== aXRef.m_bMustExist)				&&
		(m_bCanbeNull				== aXRef.m_bCanbeNull)				&&
		(m_strUrlDati				== aXRef.m_strUrlDati)				&&
		(m_strProfile				== aXRef.m_strProfile)				&&
		(m_nsDoc					== aXRef.m_nsDoc)					&&
		(m_SegmentsArray			== aXRef.m_SegmentsArray)			&&
		(m_strTableName				== aXRef.m_strTableName)			&&
		(m_strReferencedTableName	== aXRef.m_strReferencedTableName)	&&
		(m_bSubjectTo				== aXRef.m_bSubjectTo)				&&
		(m_bUse						== aXRef.m_bUse)					&&
		(m_bOwnedByDoc				== aXRef.m_bOwnedByDoc)				&&
		(m_bIsAppended				== aXRef.m_bIsAppended)				&&		
		(m_strExpression			== aXRef.m_strExpression)			&&
		(m_nsReferencedDBT			== aXRef.m_nsReferencedDBT)			&&
		(m_bModified				== aXRef.m_bModified)				&&
		(
			(!m_pXMLUniversalKeyGroup && !aXRef.m_pXMLUniversalKeyGroup) ||
			(*m_pXMLUniversalKeyGroup	== *aXRef.m_pXMLUniversalKeyGroup)
		);
}

//----------------------------------------------------------------------------------------------
CXMLXRefInfo& CXMLXRefInfo::operator =(const CXMLXRefInfo& aXRef)
{
	if (this == &aXRef)
		return *this;

	if (*this == aXRef && m_bIsAppended) //onde evitare spiacevoli sorprese questo controllo lo faccio solo per gli ExtReference appended. Modified == FALSE serve per non andare a sovrascrivere il file di ToAppendExternalReference proprietario dell'ExtRef
	{
		m_bModified = FALSE;
		return *this;
	}

	m_bModified = TRUE;

	m_strName		= aXRef.m_strName;
	m_bNoDocQuery	= aXRef.m_bNoDocQuery;
	m_bMustExist	= aXRef.m_bMustExist;
	m_bCanbeNull	= aXRef.m_bCanbeNull;
	m_strUrlDati	= aXRef.m_strUrlDati;
	m_strProfile	= aXRef.m_strProfile;
	m_nsDoc			= aXRef.m_nsDoc;
	m_bSubjectTo	= aXRef.m_bSubjectTo;
	m_strExpression	= aXRef.m_strExpression;

	m_bUse			= aXRef.m_bUse;	
	m_bOldUse		= aXRef.m_bOldUse;
	m_bOwnedByDoc	= aXRef.m_bOwnedByDoc;
	m_bIsAppended	= aXRef.m_bIsAppended;	

	m_SegmentsArray	= aXRef.m_SegmentsArray;
	m_SegmentsArray.SetSegmentsXRef(this);

	m_strTableName			= aXRef.m_strTableName;
	m_strReferencedTableName= aXRef.m_strReferencedTableName;
	m_nsReferencedDBT		= aXRef.m_nsReferencedDBT;
	m_nsReferencedTable		= aXRef.m_nsReferencedTable;

	if(m_pXMLUniversalKeyGroup)
	{
		delete m_pXMLUniversalKeyGroup;
		m_pXMLUniversalKeyGroup = NULL;
	}

	if(aXRef.m_pXMLUniversalKeyGroup)
		m_pXMLUniversalKeyGroup = new CXMLUniversalKeyGroup(*aXRef.m_pXMLUniversalKeyGroup);

	return *this;
}

//----------------------------------------------------------------------------------------------
void CXMLXRefInfo::SetXMLUniversalKeyGroup(CXMLUniversalKeyGroup* pXMLUniversalKeyGroup)
{
	if(m_pXMLUniversalKeyGroup)
	{
		delete m_pXMLUniversalKeyGroup;
		m_pXMLUniversalKeyGroup = NULL;
	}

	if(!pXMLUniversalKeyGroup)
		return;

	m_pXMLUniversalKeyGroup = new CXMLUniversalKeyGroup(*pXMLUniversalKeyGroup);
}


//----------------------------------------------------------------------------------------------
void CXMLXRefInfo::SetSegmentAt(int nIdx, CXMLSegmentInfo* pSeg)
{
	if(m_SegmentsArray.GetSize() < nIdx || nIdx <0)
		return;

	pSeg->SetXRef(this);
	m_SegmentsArray.SetAt(nIdx, pSeg);
}

//----------------------------------------------------------------------------------------------
int CXMLXRefInfo::AddSegment(CXMLSegmentInfo* pSegment)
{
	if(pSegment)
		return m_SegmentsArray.Add(pSegment);
	
	return -1;
}

//----------------------------------------------------------------------------------------------
int CXMLXRefInfo::GetSegmentIdx(const CXMLSegmentInfo& aSegInfo) const
{
	for ( int nSegIdx = 0 ; nSegIdx < m_SegmentsArray.GetSize() ; nSegIdx++)
	{	
		CXMLSegmentInfo* pSeg = m_SegmentsArray.GetAt(nSegIdx);
		if (pSeg && *pSeg == aSegInfo)
			return nSegIdx;
	}
	return -1;
}

//----------------------------------------------------------------------------------------------
CXMLSegmentInfo* CXMLXRefInfo::GetSegmentAt(int nIdx) const
{
	if(m_SegmentsArray.GetSize() < nIdx || nIdx <0)
		return NULL;

	return m_SegmentsArray.GetAt(nIdx);
}

//----------------------------------------------------------------------------------------------
void CXMLXRefInfo::RemoveSegmentAt(int nIdx)
{
	if(m_SegmentsArray.GetSize() < nIdx || nIdx < 0)
		return;
	
	m_SegmentsArray.RemoveAt(nIdx);
}

//----------------------------------------------------------------------------------------------
void CXMLXRefInfo::RemoveAllSegments()
{
	m_SegmentsArray.RemoveAll();
}


//--------------------------------------------------------------------
BOOL CXMLXRefInfo::LoadSymTable()
{
	if (!m_pSymTable)
		m_pSymTable = new SymTable();
	else 
		m_pSymTable->RemoveAll();

	SqlTableInfo* pTableInfo = AfxGetDefaultSqlConnection()->GetTableInfo(m_strTableName);
	if (!pTableInfo)
		return FALSE;

	const  Array* pColums = pTableInfo->GetPhysicalColumns();
	for (int nIdx = 0 ; nIdx < pColums->GetSize() ; nIdx++)
	{
		SqlColumnInfo* pColInfo = (SqlColumnInfo*) pColums->GetAt(nIdx);
		if (!pColInfo)
			continue;

		SymField* pField = new SymField(pColInfo->GetColumnName(), pColInfo->GetDataObjType());
		//pField->AssignData(*pRecord->GetDataObjAt(i));
		m_pSymTable->Add(pField);
	}
	return TRUE;
}

//--------------------------------------------------------------------
BOOL CXMLXRefInfo::LoadSymTable(SqlRecord* pRecord)
{
	if (!pRecord)
		return FALSE;

	if (!m_pSymTable)
		m_pSymTable = new SymTable();
	else 
		m_pSymTable->RemoveAll();

	pRecord->SetQualifier();
	SymField* pField = NULL;
	for (int i = 0; i < pRecord->GetSize(); i++)
	{
		if (!pRecord->GetDataObjAt(i))
			continue;

		pField = new SymField(
				pRecord->GetColumnName(pRecord->GetDataObjAt(i)),
				pRecord->GetDataObjAt(i)->GetDataType(),
				SpecialReportField::NO_INTERNAL_ID,
				pRecord->GetDataObjAt(i)
			);
		m_pSymTable->Add(pField);

		//inserisco anche il nome del campo con la qualifica
		pField = new SymField(
			pRecord->GetQualifiedColumnName(pRecord->GetDataObjAt(i)),
			pRecord->GetDataObjAt(i)->GetDataType(),
			SpecialReportField::NO_INTERNAL_ID,
			pRecord->GetDataObjAt(i)
			);
		m_pSymTable->Add(pField);
	}

	return TRUE;
}

//-----------------------------------------------------------------------------
void CXMLXRefInfo::RemoveSymTable()
{
	if (m_pSymTable)
	{
		delete m_pSymTable;
		m_pSymTable = NULL;
	}
}

//-----------------------------------------------------------------------------
BOOL  CXMLXRefInfo::AddFieldInSymTable(const CString& strName, DataObj* pDataObj)
{
	if (strName.IsEmpty() || !pDataObj)
		return FALSE;

	if (!m_pSymTable)
		m_pSymTable = new SymTable();

	SymField* pField = new SymField(
		strName,
		pDataObj->GetDataType(),
		SpecialReportField::NO_INTERNAL_ID,
		pDataObj
		);
	m_pSymTable->Add(pField);
	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CXMLXRefInfo::CheckExpressionSintax(const DataStr& strExpression, CString& strMessErr)
{
	if (!strExpression.IsEmpty())
	{
		WClause tmpExpr(AfxGetDefaultSqlConnection(), m_pSymTable);
		tmpExpr.SetTableInfo(AfxGetDefaultSqlConnection()->GetTableInfo(m_strTableName));		
		tmpExpr.SetNative(FALSE);

		Parser lex(strExpression.GetString());
		if (!tmpExpr.Parse(lex))
		{
			strMessErr = cwsprintf(_TB("Error evaluating expression {0-%s}."), strExpression.Str());
			if (!lex.GetError().IsEmpty ())
				strMessErr += lex.GetError();
		
			lex.ClearError();
			return FALSE;
		}
	}
	return TRUE;
}

//----------------------------------------------------------------------------
BOOL CXMLXRefInfo::EvalExpression(SqlRecord* pRecord, CString& strMessErr)
{    
	DataBool bValue;

	if (m_strExpression.IsEmpty())
		return TRUE;

	LoadSymTable(pRecord);
	Expression tmpExpr(m_pSymTable);
	Parser	lex(m_strExpression.GetString());
	
	if (!tmpExpr.Parse(lex, DataType::Bool, FALSE))
	{
		strMessErr = cwsprintf(_TB("Error evaluating expression {0-%s}."), m_strExpression);
		if (!lex.GetError().IsEmpty())
			strMessErr += lex.GetError();
		
		return FALSE;
	}

	//valuto l'espressione
	tmpExpr.Eval(bValue);

	return bValue;
}


//----------------------------------------------------------------------------------------------
BOOL CXMLXRefInfo::Parse(CXMLNode* pNode, BOOL bDescription)
{
	if (!pNode) return FALSE;
	CXMLNode* pInfoNode;
	CXMLSegmentInfo* pXMLSegInfo;

	CString strTagValue;

	pInfoNode = pNode->GetChildByName(XML_NAME_TAG);
	if (pInfoNode) pInfoNode->GetText(m_strName);
	

	CString strAttrValue;
	pNode->GetAttribute(XML_NO_DOC_QUERY_ATTRIBUTE, strAttrValue);
	m_bNoDocQuery = GetBoolFromXML(strAttrValue);
				
	
	pInfoNode = pNode->GetChildByName(XML_MUST_EXIST_TAG);
	if (pInfoNode)
	{
		pInfoNode->GetText(strTagValue);
		m_bMustExist = GetBoolFromXML(strTagValue);
	}

	pInfoNode = pNode->GetChildByName(XML_XREF_SUBJECT_TO_TAG);
	if (pInfoNode)
	{
		pInfoNode->GetText(strTagValue);
		m_bSubjectTo = GetBoolFromXML(strTagValue);

		pInfoNode = pNode->GetChildByName(XML_XREF_EXPRESSION_TAG);
		if (pInfoNode)
		{
			pInfoNode->GetText(strTagValue);		
			m_strExpression = strTagValue;
		}
	}

	pInfoNode = pNode->GetChildByName(XML_NULL_ALLOWED_TAG);
	if (pInfoNode)
	{
		pInfoNode->GetText(strTagValue);
		m_bCanbeNull = GetBoolFromXML(strTagValue);
	}
	
	pInfoNode = pNode->GetChildByName(XML_DATA_URL_TAG);
	if (pInfoNode) pInfoNode->GetText(m_strUrlDati);

	pInfoNode = pNode->GetChildByName(XML_PROFILE_NAME_TAG);
	if (pInfoNode) pInfoNode->GetText(m_strProfile);

	pInfoNode = pNode->GetChildByName(XML_DBT_TAG);
	if (pInfoNode) 
	{
		CString ns;
		pInfoNode->GetAttribute(XML_NAMESPACE_ATTRIBUTE, ns);
		if (!ns.IsEmpty())
		{
			m_nsReferencedDBT.AutoCompleteNamespace (CTBNamespace::DBT, ns, m_nsReferencedDBT);
			pInfoNode->GetAttribute(XML_TABLE_ATTRIBUTE, ns);
		}
		if (!ns.IsEmpty())
		{
			m_nsReferencedTable.AutoCompleteNamespace (CTBNamespace::TABLE, ns, m_nsReferencedTable);
			m_strReferencedTableName = m_nsReferencedTable.GetObjectName();
		}
	}

	if(bDescription || !IsOwnedByDoc())
	{
		CString strNamespace;
		if (pNode->GetAttribute(XML_NAMESPACE_ATTRIBUTE, strNamespace))
		{
			m_nsDoc.AutoCompleteNamespace(CTBNamespace::DOCUMENT, strNamespace, m_nsDoc);
			GetReferencedTableName();
		}

		pInfoNode = pNode->GetChildByName(XML_KEYS_TAG);
		if (pInfoNode)
		{
			//modifica per il controllo di esistenza delle colonne riferite dai segmenti di ExtRef
//@@BAUZI
			for (int i = 0; i < pInfoNode->GetChildsNum(); i++)
			{
				CXMLNode* pSegNode = pInfoNode->GetChildAt(i);
				if (pSegNode)
				{
					pXMLSegInfo = new CXMLSegmentInfo(this);
					if (pXMLSegInfo->Parse(pSegNode))
						AddSegment(pXMLSegInfo);
					else
					{
						delete pXMLSegInfo;
						pXMLSegInfo = NULL;
					}
				}
			}
		}
	}

	//Universal Key
	pInfoNode = pNode->GetChildByName(XML_UNIVERSAL_KEYS_TAG);
	if(pInfoNode)
	{
		if(!m_pXMLUniversalKeyGroup) m_pXMLUniversalKeyGroup = new CXMLUniversalKeyGroup();

		CString strIsToExpUK;
		pInfoNode->GetAttribute(XML_UNIVERSAL_KEY_EXPORT_ATTRIBUTE, strIsToExpUK);
			
		m_pXMLUniversalKeyGroup->SetExportData(GetBoolFromXML(strIsToExpUK));
	}

	return TRUE;
}

//----------------------------------------------------------------------------------------------
BOOL CXMLXRefInfo::UnParse(CXMLNode* pExtRefNode, BOOL bDescription /*=TRUE*/)
{
	if(!pExtRefNode)
		return FALSE;
	
	CXMLNode* pChild;

	pChild = pExtRefNode->CreateNewChild(XML_NAME_TAG);
	pChild->SetText(m_strName);
	pExtRefNode->SetAttribute(XML_NO_DOC_QUERY_ATTRIBUTE, FormatBoolForXML(m_bNoDocQuery));

	pChild = pExtRefNode->CreateNewChild(XML_MUST_EXIST_TAG);
	pChild->SetText(FormatBoolForXML(m_bMustExist));

	pChild = pExtRefNode->CreateNewChild(XML_NULL_ALLOWED_TAG);
	pChild->SetText(FormatBoolForXML(m_bCanbeNull));	

	pChild = pExtRefNode->CreateNewChild(XML_XREF_SUBJECT_TO_TAG);
	pChild->SetText(FormatBoolForXML(m_bSubjectTo));
	if (m_bSubjectTo)
	{	
		pChild = pExtRefNode->CreateNewChild(XML_XREF_EXPRESSION_TAG);
		pChild->SetText(m_strExpression.FormatDataForXML());
	}	

	pChild = pExtRefNode->CreateNewChild(XML_DATA_URL_TAG);
	pChild->SetText(m_strUrlDati);
	
	pChild = pExtRefNode->CreateNewChild(XML_PROFILE_NAME_TAG);
	pChild->SetText(m_strProfile);
	
	if (!m_nsReferencedDBT.IsEmpty())
	{
		pChild = pExtRefNode->CreateNewChild(XML_DBT_TAG);

		if (pChild) 
		{
			pChild->SetAttribute(XML_NAMESPACE_ATTRIBUTE, m_nsReferencedDBT.ToUnparsedString());
			pChild->SetAttribute(XML_TABLE_ATTRIBUTE, m_nsReferencedTable.ToUnparsedString());
		}
	}

	if (bDescription || (!bDescription && !IsOwnedByDoc()))
	{
		pExtRefNode->SetAttribute(XML_NAMESPACE_ATTRIBUTE, GetDocumentNamespace().ToUnparsedString());
		
		pChild = pExtRefNode->CreateNewChild(XML_KEYS_TAG);
		CXMLSegmentInfo* pSeg;
		CXMLNode* pnKeySegment;
		
		for ( int nSegIdx = 0 ; nSegIdx < m_SegmentsArray.GetSize() ; nSegIdx++)
		{	
			pSeg = m_SegmentsArray.GetAt(nSegIdx);

			pnKeySegment = pChild->CreateNewChild(XML_KEY_SEGMENT_TAG);

			if(!pSeg || !pnKeySegment)
				return FALSE;
			pSeg->UnParse(pnKeySegment);
		}
	}

	if (m_pXMLUniversalKeyGroup)
	{
		pChild = pExtRefNode->CreateNewChild(XML_UNIVERSAL_KEYS_TAG);
		pChild->SetAttribute(XML_UNIVERSAL_KEY_EXPORT_ATTRIBUTE, FormatBoolForXML(m_pXMLUniversalKeyGroup->IsExportData()));
	}

	return TRUE;
}

//----------------------------------------------------------------------------------------------
void CXMLXRefInfo::SetReferencedTableNs (const CString& strNamespace)
{
	if (!strNamespace.IsEmpty())
	{
		m_nsReferencedTable = strNamespace;
		m_strReferencedTableName = m_nsReferencedTable.GetObjectName();
	}
}

//----------------------------------------------------------------------------------------------
CString CXMLXRefInfo::GetReferencedTableName()
{
	// Per conoscere il nome della tabella referenziata dalla relazione di external 
	// reference occorre necessariamente caricare il file XML di descrizione dei
	// DBT di documento e ricavarlo dal suo contenuto. Per non ripetere più volte 
	// questa operazione salvo tale informazione in un data member.
	
	if (!m_strReferencedTableName.IsEmpty())
		return m_strReferencedTableName;

	//const CDocumentDescription* pDocDescri = AfxGetDocumentDescription(GetDocumentNamespace());
	
	CString strFileDBT = AfxGetPathFinder()->GetDocumentDbtsFullName(GetDocumentNamespace());
	if (strFileDBT.IsEmpty()) 
		return _T("");

	
	CLocalizableXMLDocument	aXMLDBTDoc(m_nsDoc, AfxGetPathFinder());
	CXMLDBTInfoArray	aXMLDBTInfoArray;
		
	if (!aXMLDBTDoc.LoadXMLFile(strFileDBT))
		return _T("");

	CXMLNode* pDBTMasterNode = aXMLDBTDoc.GetRootChildByName(XML_DBT_TYPE_MASTER_TAG);
	if (!pDBTMasterNode)
		return _T("");

	CXMLNode* pDBTMasterTableNode = pDBTMasterNode->GetChildByName(XML_TABLE_TAG);

	CString strTableName;
	if (pDBTMasterTableNode  && pDBTMasterTableNode->GetText(strTableName))
	{
		m_strReferencedTableName = strTableName;
		CString strNameSpace;
		pDBTMasterTableNode->GetAttribute(XML_NAMESPACE_ATTRIBUTE, strNameSpace);
		if (!strNameSpace.IsEmpty())
			m_nsReferencedTable.AutoCompleteNamespace(CTBNamespace::TABLE, strNameSpace, m_nsReferencedTable);
		return m_strReferencedTableName;
	}
	return _T("");
}

//----------------------------------------------------------------------------------------------
void CXMLXRefInfo::SetReferencedDBTNs (const CString& strNamespace)
{
	m_nsReferencedDBT = strNamespace;
}

//----------------------------------------------------------------------------------------------
CTBNamespace CXMLXRefInfo::GetReferencedDBTNs ()
{
	return m_nsReferencedDBT;
}

//----------------------------------------------------------------------------------------------
void CXMLXRefInfo::GetReferencedDBTList(CObArray& arDBTs)
{
	arDBTs.RemoveAll();

	//const CDocumentDescription* pDocInfo = AfxGetDocumentDescription(GetDocumentNamespace());

	CString strFileDBT = AfxGetPathFinder()->GetDocumentDbtsFullName(GetDocumentNamespace());
	if (strFileDBT.IsEmpty()) 
		return;
	
	CLocalizableXMLDocument	aXMLDBTDoc(m_nsDoc, AfxGetPathFinder());
	if (aXMLDBTDoc.LoadXMLFile(strFileDBT))
	{
		CXMLNode* pDBTMasterNode = aXMLDBTDoc.GetRootChildByName(XML_DBT_TYPE_MASTER_TAG);
		if (pDBTMasterNode)
		{

			CString strTitle;
			CString strTableNamespace;
			CString strNamespace;

			pDBTMasterNode->GetAttribute(XML_NAMESPACE_ATTRIBUTE, strNamespace);

			// master data
			CXMLNode* pTitleNode = pDBTMasterNode->GetChildByName(XML_TITLE_TAG);
			if (pTitleNode)
				pTitleNode->GetText(strTitle);

			CXMLNode* pTableNode = pDBTMasterNode->GetChildByName(XML_TABLE_TAG);
			if (pTableNode)
				pTableNode->GetAttribute(XML_NAMESPACE_ATTRIBUTE, strTableNamespace);

			arDBTs.Add(new CXMLDBTData(strNamespace, strTitle, strTableNamespace));

			// slaves data
			CXMLNode* pSlavesNode = pDBTMasterNode->GetChildByName(XML_SLAVES_TAG);
			if (!pSlavesNode || !pSlavesNode->GetChilds())
				return;

			CXMLNode* pSlaveNode;
			CString sNodeName;
			for (int i = 0; i <= pSlavesNode->GetChilds()->GetUpperBound(); i++)
			{
				pSlaveNode = pSlavesNode->GetChilds()->GetAt(i);
				if (
					!pSlaveNode || !pSlaveNode->GetName(sNodeName) ||
					(
						sNodeName.CompareNoCase(XML_DBT_TYPE_SLAVE_TAG) &&
						sNodeName.CompareNoCase(XML_DBT_TYPE_BUFFERED_TAG) &&
						sNodeName.CompareNoCase(XML_DBT_TYPE_SLAVABLE_TAG)
						)
					)
					continue;

				pSlaveNode->GetAttribute(XML_NAMESPACE_ATTRIBUTE, strNamespace);

				pTitleNode = pSlaveNode->GetChildByName(XML_TITLE_TAG);
				if (pTitleNode)
					pTitleNode->GetText(strTitle);

				pTableNode = pSlaveNode->GetChildByName(XML_TABLE_TAG);
				if (pTableNode)
					pTableNode->GetAttribute(XML_NAMESPACE_ATTRIBUTE, strTableNamespace);

				arDBTs.Add(new CXMLDBTData(strNamespace, strTitle, strTableNamespace));
			}
		}
	}
}

//----------------------------------------------------------------------------------------------
BOOL CXMLXRefInfo::SetDocumentNamespace (LPCTSTR lpszNsDoc)		
{
	CTBNamespace aNamespace;
	aNamespace.AutoCompleteNamespace(CTBNamespace::DOCUMENT, lpszNsDoc, aNamespace);
	if (!aNamespace.IsValid()) return FALSE;

	return SetDocumentNamespace(aNamespace);
}

//----------------------------------------------------------------------------------------------
BOOL CXMLXRefInfo::SetDocumentNamespace(const CTBNamespace& nsDoc)
{
	if (m_nsDoc == nsDoc)
		return TRUE;

	m_nsDoc = nsDoc;

	// Essendo cambiato il documento "di appartenenza" devo 
	// aggiornare il nome della tabella referenziata:
	m_strReferencedTableName.Empty();
	return !GetReferencedTableName().IsEmpty();
}

//----------------------------------------------------------------------------------------------
BOOL CXMLXRefInfo::HasValidRefDoc()
{
	//assumo che se non riesce a darmi il nome del doc vuol dire che o non è 
	//stato caricato il modulo o il modobjlist relativo è cannato
	return GetDocumentNamespace().IsValid();
}


//----------------------------------------------------------------------------------------------
//	CXMLXRefInfoArray implementation
//----------------------------------------------------------------------------------------------
CXMLXRefInfoArray::CXMLXRefInfoArray()
{

}

//----------------------------------------------------------------------------------------------
CXMLXRefInfoArray::CXMLXRefInfoArray(const CXMLXRefInfoArray& aXRefArray)
{
	RemoveAll();

	for(int i = 0 ; i < aXRefArray.GetSize() ; i++)
	{
		if(aXRefArray.GetAt(i))
			Add(new CXMLXRefInfo(*aXRefArray.GetAt(i)));
	}
}

//----------------------------------------------------------------------------------------------
BOOL CXMLXRefInfoArray::IsEqual(const CXMLXRefInfoArray& aXRefArray) const
{
	if (this == &aXRefArray)
		return TRUE;
	
	if(aXRefArray.GetSize() != GetSize())
		return FALSE;
	
	for(int i = 0 ; i < GetSize() ; i++)
	{
		if(*GetAt(i) != *aXRefArray.GetAt(i))
			return FALSE;
	}

	return TRUE;
}

//----------------------------------------------------------------------------------------------
CXMLXRefInfoArray& CXMLXRefInfoArray::operator =(const CXMLXRefInfoArray& aXRefArray)
{
	if (this == &aXRefArray)
		return *this;
	
	RemoveAll();

	for(int i = 0 ; i < aXRefArray.GetSize() ; i++)
	{
		if(aXRefArray.GetAt(i))
			Add(new CXMLXRefInfo(*aXRefArray.GetAt(i)));
	}
	return *this;
}

//----------------------------------------------------------------------------------------------
BOOL CXMLXRefInfoArray::Parse(CXMLNode* pExtRefsNode, LPCTSTR lpszTableName)
{
	if (!pExtRefsNode)	
		return FALSE;
	
	RemoveAll();

	CXMLNode* pDocInfoNode;
	CXMLXRefInfo* pXRefInfo;
	
	// cerco l'external reference con il namespace passato
	for (int i = 0; i < pExtRefsNode->GetChildsNum(); i++)
	{
		pDocInfoNode = pExtRefsNode->GetChildAt(i);
		if (pDocInfoNode) 
		{
			pXRefInfo = new CXMLXRefInfo(lpszTableName);
			if (pXRefInfo->Parse(pDocInfoNode, TRUE))
			{
				Add(pXRefInfo);
				if (pXRefInfo->m_bSubjectTo)
				{
					CString strExpr = pXRefInfo->m_strExpression;
					if (strExpr.Replace(szTableName, lpszTableName) > 0)
						pXRefInfo->m_strExpression = strExpr;
				}
			}
			else
			{
				delete pXRefInfo;
				pXRefInfo = NULL;
			}
		}
	}
	return TRUE;
}

//----------------------------------------------------------------------------------------------
BOOL CXMLXRefInfoArray::UnParse(CXMLNode* pnXRefDbtNode, CXMLDBTInfo* pDbt, BOOL bDescription /*=TRUE*/)
{
	pnXRefDbtNode->SetAttribute(XML_NAMESPACE_ATTRIBUTE, pDbt->GetNamespace().ToUnparsedString());

	CXMLNode* pChildNode = pnXRefDbtNode->CreateNewChild(XML_EXPORT_TAG);
	pChildNode->SetText(FormatBoolForXML(pDbt->IsToExport()));

	pChildNode = pnXRefDbtNode->CreateNewChild(XML_DBT_UPDATETYPE_TAG);
	pChildNode->SetText(pDbt->GetStrUpdateType());

	CXMLNode* pExtRefNodes	= pnXRefDbtNode->CreateNewChild(XML_EXTERNAL_REFERENCES_TAG);
	if(!pDbt->m_pXRefsArray || !pDbt->IsToExport()) return TRUE;

	int nXRef = pDbt->m_pXRefsArray->GetSize();

	for ( int nXRefIdx = 0 ; nXRefIdx < nXRef ; nXRefIdx++)
	{	
		CXMLXRefInfo* pXRef = pDbt->m_pXRefsArray->GetAt(nXRefIdx);

		if ((bDescription && pXRef->IsAppended()) || !pXRef->IsToUse())
			continue;

		CXMLNode* pExtRefNode = pExtRefNodes->CreateNewChild(XML_EXTERNAL_REFERENCE_TAG);

		if(!pXRef || !pExtRefNode)
			return FALSE;
		
		pXRef->UnParse(pExtRefNode, bDescription);
	}

	return TRUE;
}

// si occupa di cercare all'interno dell'array di informazioni un external reference per
// la dichiarazione dei campi che costituiscono i segmenti di foreing key.
//----------------------------------------------------------------------------------------------
CXMLXRefInfo* CXMLXRefInfoArray::Lookup	(LPCTSTR aSegment, ...)
{
	// aggiungo il primo parametro
	CStringArray aSegments;
	
	CString strSegment(aSegment);

	if (!strSegment.IsEmpty())
		aSegments.Add(aSegment);

	// prima estraggo i segmenti ricercati
	va_list vaSeg;
	va_start (vaSeg, aSegment); 

	LPCTSTR strp;
	while (strp = va_arg (vaSeg, LPCTSTR))
	{ 
		CString strTmp (strp);
		
		if (strTmp.IsEmpty())
			break;
		
		aSegments.Add((LPCTSTR) strTmp);
	} 

	va_end (vaSeg); 
	
	// poi cerco il reference giusto per uguaglianza
	// dei segmenti relativi alla foreing key
	for (int i=0; i <= GetUpperBound(); i++)
	{
		CXMLXRefInfo* pItem = GetAt(i);
		
		if (!pItem || pItem->GetSegmentsNum() != aSegments.GetSize())
			continue;
		
		BOOL bSameSegments = TRUE;

		for (int n=0; i <= aSegments.GetSize(); i++)
		{
			if (aSegments.GetAt(n) != pItem->GetSegmentAt(n)->GetFKSegment())
			{
				bSameSegments = FALSE;
				break;
			}
		}

		if (bSameSegments)
			return pItem;
	}

	return NULL;
}

//----------------------------------------------------------------------------------------------
BOOL CXMLXRefInfoArray::GetXRefArrayByFK(const CString& strSegmentFk, CXMLXRefInfoArray* pXMLXRefInfoArray, BOOL bUsedOnly /*= TRUE*/)
{
	if(strSegmentFk.IsEmpty() || !pXMLXRefInfoArray)
		return FALSE;

	if(pXMLXRefInfoArray->IsOwnsElements())
		pXMLXRefInfoArray->SetOwns(FALSE);

	//ogni xref che ha una fk come quella passata va aggiunta all'array
	for (int i = 0; i < GetSize() ; i++)
	{
		CXMLXRefInfo* pXMLXRefInfo = GetAt(i);
		
		if (!pXMLXRefInfo || !pXMLXRefInfo->m_SegmentsArray.GetSize() || (bUsedOnly && !pXMLXRefInfo->IsToUse()))
			continue;

		if(pXMLXRefInfo->m_SegmentsArray.IsFkPresent(strSegmentFk))
			pXMLXRefInfoArray->Add(pXMLXRefInfo);
	}

	return TRUE;
}

//----------------------------------------------------------------------------------------------
BOOL CXMLXRefInfoArray::IsFKInUsedExtRef(const CString& strSegmentFk) const
{
	for (int i = GetSize() - 1 ; i >= 0 ; i--)
	{
		CXMLXRefInfo* pXMLXRefInfo = GetAt(i);
		
		if (!pXMLXRefInfo || !pXMLXRefInfo->IsToUse())
			continue;
		
		if(pXMLXRefInfo->m_SegmentsArray.IsFkPresent(strSegmentFk))
			return TRUE;
	}

	return FALSE;
}


//----------------------------------------------------------------
//class CXMLXReferencesToAppend 
//----------------------------------------------------------------
IMPLEMENT_DYNAMIC(CXMLXReferencesToAppend, CObject)	

//----------------------------------------------------------------------------------------------
CXMLXReferencesToAppend::CXMLXReferencesToAppend()
:
	m_pXRefsToAppendArray(NULL)
{
}

//----------------------------------------------------------------------------------------------
CXMLXReferencesToAppend::~CXMLXReferencesToAppend()
{
	if (m_pXRefsToAppendArray)
		delete m_pXRefsToAppendArray;
}

//----------------------------------------------------------------------------------------------
CXMLXReferencesToAppend::CXMLXReferencesToAppend(CXMLXReferencesToAppend& aXMLXReferencesToAppend)
{
	m_pXRefsToAppendArray = new CXMLXRefInfoArray();
	m_pXRefsToAppendArray->SetOwns(FALSE);
	
	*m_pXRefsToAppendArray = *aXMLXReferencesToAppend.m_pXRefsToAppendArray;
	m_strFileName = aXMLXReferencesToAppend.m_strFileName;
	m_strDocNamespace = aXMLXReferencesToAppend.m_strDocNamespace;

}

//----------------------------------------------------------------------------------------------
void CXMLXReferencesToAppend::SubstituteTableName(CXMLXRefInfo* pXRefInfo, const CString& strOldTableName, const CString& strNewTableName)
{
	CString strExpr = pXRefInfo->m_strExpression;
	strExpr.Replace(strOldTableName, strNewTableName);
	pXRefInfo->m_strExpression = strExpr;
}
//----------------------------------------------------------------------------------------------
BOOL CXMLXReferencesToAppend::IsEqual(const CXMLXReferencesToAppend& aXRefArray) const
{
	return 
		(
			(this == &aXRefArray) ||
			(m_strFileName == aXRefArray.m_strFileName &&
			m_strDocNamespace == aXRefArray.m_strDocNamespace &&
			*m_pXRefsToAppendArray == *aXRefArray.m_pXRefsToAppendArray)
		);
}

//----------------------------------------------------------------------------------------------
void CXMLXReferencesToAppend::SetInfoFromDocNamespace(const CString& strDocNamespace)
{
	CString docNamespace = strDocNamespace;
	m_strDocNamespace = strDocNamespace;
	docNamespace.Replace(_T("."), _T("\\"));
	int nLastSlash = docNamespace.ReverseFind (SLASH_CHAR);	
	CString strFileName = AfxGetPathFinder()->GetContainerPath(CPathFinder::TB_APPLICATION)  + SLASH_CHAR + docNamespace.Left(nLastSlash) + _T("\\ModuleObjects\\") + docNamespace.Right(docNamespace.GetLength()-nLastSlash- 1)  + "\\Description\\ExtReferencesToAppend.xml";
	m_strFileName = strFileName;		
}
//----------------------------------------------------------------------------------------------
BOOL CXMLXReferencesToAppend::Parse(const CString& strDocNamespace, const CString& strTableName)
{
	SetInfoFromDocNamespace(strDocNamespace);
	CXMLDocumentObject aXMLXRefDoc;
	aXMLXRefDoc.EnableMsgMode(FALSE);
	CXMLNode* pExtRefsNode = NULL;
	BOOL bResult = FALSE;
	if (aXMLXRefDoc.LoadXMLFile(m_strFileName))
	{	
		pExtRefsNode = aXMLXRefDoc.GetRootChildByName(XML_EXTERNAL_REFERENCES_TAG);
		if (pExtRefsNode)
		{
			m_pXRefsToAppendArray = new CXMLXRefInfoArray();
			m_pXRefsToAppendArray->SetOwns(FALSE);//sono di proprietà dell'array degli xref del dbt
			bResult = m_pXRefsToAppendArray->Parse(pExtRefsNode, strTableName);
		}
	}

	return bResult;	
}

//----------------------------------------------------------------------------------------------
void CXMLXReferencesToAppend::Unparse(const CString& strTableName)
{
	if (!m_pXRefsToAppendArray || m_strFileName.IsEmpty())
		return;

	//faccio l'unparse delle informazioni solo se vi è qualche external reference modificato altrimenti non salvo nulla

	BOOL bModified = FALSE;
	for ( int i = 0 ; i < m_pXRefsToAppendArray->GetSize() ; i++)
	{	
		CXMLXRefInfo* pXRef = m_pXRefsToAppendArray->GetAt(i);
		bModified |= pXRef->m_bModified;
		if (bModified) break; // mi basta anche solo uno modificato per salvare il file
	}

	if (!bModified) //se non ho alcun XRef modificato allora non salvo il file
		return;

	CXMLDocumentObject aXMLXRefDoc(TRUE, FALSE);
	aXMLXRefDoc.CreateInitialProcessingInstruction();
	CXMLNode* pnRootXRefs = aXMLXRefDoc.CreateRoot(XML_MAIN_EXTERNAL_REFERENCES_TAG);
	
	CXMLNode* pExtRefsNode = pnRootXRefs->CreateNewChild(XML_EXTERNAL_REFERENCES_TAG);
	
	for ( int i = 0 ; i < m_pXRefsToAppendArray->GetSize() ; i++)
	{	
		CXMLXRefInfo* pXRef = m_pXRefsToAppendArray->GetAt(i);

		if(!pXRef || !pXRef->IsToUse()) continue;

		CXMLNode* pExtRefNode = pExtRefsNode->CreateNewChild(XML_EXTERNAL_REFERENCE_TAG);

		if (pXRef->m_bSubjectTo)
			SubstituteTableName(pXRef, strTableName, szTableName);				
		
		if(pExtRefNode)
			pXRef->UnParse(pExtRefNode);

		if (pXRef->m_bSubjectTo)
			SubstituteTableName(pXRef, szTableName, strTableName);
	}

	aXMLXRefDoc.SaveXMLFile(m_strFileName);
}

//----------------------------------------------------------------------------------------------
// CXMLXReferencesToAppendArray
//----------------------------------------------------------------------------------------------
//
//----------------------------------------------------------------------------------------------
CXMLXReferencesToAppendArray::CXMLXReferencesToAppendArray(const CXMLXReferencesToAppendArray& aXRefArray)
{	
	RemoveAll();

	for(int i = 0 ; i < aXRefArray.GetSize() ; i++)
	{
		if(aXRefArray.GetAt(i))
			Add(new CXMLXReferencesToAppend(*aXRefArray.GetAt(i)));
	}
}

//----------------------------------------------------------------------------------------------
void CXMLXReferencesToAppendArray::Unparse(CXMLNode*pDBTNode, const CString& tableName)
{
	for(int i = 0 ; i < GetSize() ; i++)
	{
		CXMLXReferencesToAppend* xRef = GetAt(i);
		if (xRef)
		{
			//faccio l'unparse sul file di append
			xRef->Unparse(tableName);
			//inserisco il bookmark nel nodo del dbt con il rimando al file
			CXMLNode* pExtRefsNode = pDBTNode->CreateNewChild(XML_APPENDED_EXTERNAL_REFERENCES_TAG);
			CXMLNode* pExtRefNode = pExtRefsNode->CreateNewChild(XML_APPENDED_EXTERNAL_REFERENCE_TAG);
			pExtRefNode->SetText((LPCTSTR) xRef->m_strDocNamespace);
		}
	}
}

//----------------------------------------------------------------------------------------------
BOOL CXMLXReferencesToAppendArray::Parse(CXMLNode*pDBTNode, const CString& tableName)
{
	CXMLNode*pExtRefNode = NULL;
	CString strNamespaceDoc;
	CXMLNode*pExtRefsNode = pDBTNode->GetChildByName(XML_APPENDED_EXTERNAL_REFERENCES_TAG);
	if (pExtRefsNode)
		// cerco l'external reference con il namespace passato
		for (int i = 0; i < pExtRefsNode->GetChildsNum(); i++)
		{
			pExtRefNode = pExtRefsNode->GetChildAt(i);
			if (pExtRefNode) 
			{
				pExtRefNode->GetText(strNamespaceDoc);
				if (!strNamespaceDoc.IsEmpty())
				{
					CXMLXReferencesToAppend* XRefsToAppend = new CXMLXReferencesToAppend();
					if (XRefsToAppend->Parse(strNamespaceDoc, tableName))
						Add(XRefsToAppend);
					else
						delete XRefsToAppend;
				}
			}
		}
	
	return TRUE;
}

//----------------------------------------------------------------------------------------------
CXMLXReferencesToAppendArray& CXMLXReferencesToAppendArray::operator =(const CXMLXReferencesToAppendArray& aXRefAppendArray)
{
	if (this == &aXRefAppendArray)
		return *this;
	
	RemoveAll();

	for(int i = 0 ; i < aXRefAppendArray.GetSize() ; i++)
	{
		if(aXRefAppendArray.GetAt(i))
			Add(new CXMLXReferencesToAppend(*aXRefAppendArray.GetAt(i)));
	}
	return *this;
}


//----------------------------------------------------------------------------------------------
BOOL CXMLXReferencesToAppendArray::IsEqual(const CXMLXReferencesToAppendArray& aXRefArray) const
{
	if (this == &aXRefArray)
		return TRUE;
	
	if(aXRefArray.GetSize() != GetSize())
		return FALSE;
	
	for(int i = 0 ; i < GetSize() ; i++)
	{
		if(*GetAt(i) != *aXRefArray.GetAt(i))
			return FALSE;
	}

	return TRUE;
}

//----------------------------------------------------------------------------------------------
// CXMLFixedKey
//----------------------------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(CXMLFixedKey, CObject)

//----------------------------------------------------------------------------------------------
CXMLFixedKey::CXMLFixedKey()
{
}

//----------------------------------------------------------------------------------------------
CXMLFixedKey::CXMLFixedKey(CXMLFixedKey& aXMLFixedKey)
{
	SetName(aXMLFixedKey.GetName());
	SetValue(aXMLFixedKey.GetValue());
}

//----------------------------------------------------------------------------------------------
BOOL CXMLFixedKey::IsEqual(const CXMLFixedKey& aXMLFixedKey) const
{
	if (this == &aXMLFixedKey)
		return TRUE;
	

	return	(
				aXMLFixedKey.GetName().CompareNoCase(GetName()) == 0 &&
				aXMLFixedKey.GetValue().CompareNoCase(GetValue()) == 0
			);
}

//----------------------------------------------------------------------------------------------
CXMLFixedKey& CXMLFixedKey::operator = (const CXMLFixedKey& aXMLFixedKey)
{
	if (this == &aXMLFixedKey)
		return *this;
	
	m_strName = aXMLFixedKey.GetName();
	m_strValue = aXMLFixedKey.GetValue();
	return *this;
}

//----------------------------------------------------------------------------------------------
BOOL CXMLFixedKey::Parse(CXMLNode* pnFixedlKey)
{
	if (!pnFixedlKey)
		return FALSE;
	
	pnFixedlKey->GetAttribute(XML_FIXED_SEG_NAME_ATTRIBUTE, m_strName);
	pnFixedlKey->GetAttribute(XML_FIXED_SEG_VALUE_ATTRIBUTE, m_strValue);

	return TRUE;
}

//----------------------------------------------------------------------------------------------
BOOL CXMLFixedKey::UnParse(CXMLNode* pnFixedlKey)
{
	if(!pnFixedlKey)
		return FALSE;

	pnFixedlKey->SetAttribute(XML_FIXED_SEG_NAME_ATTRIBUTE, m_strName);
	pnFixedlKey->SetAttribute(XML_FIXED_SEG_VALUE_ATTRIBUTE, m_strValue);

	return TRUE;
}


//----------------------------------------------------------------------------------------------
//	CXMLFixedKeyArray
//----------------------------------------------------------------------------------------------

IMPLEMENT_DYNAMIC(CXMLFixedKeyArray, Array)

//----------------------------------------------------------------------------------------------
CXMLFixedKeyArray::CXMLFixedKeyArray()
{
}

//----------------------------------------------------------------------------------------------
CXMLFixedKeyArray::CXMLFixedKeyArray(const CXMLFixedKeyArray& aFixedKeyArray)
{
	RemoveAll();

	for(int i = 0 ; i < aFixedKeyArray.GetSize() ; i++)
	{
		if(aFixedKeyArray.GetAt(i))
			Add(new CXMLFixedKey(*aFixedKeyArray.GetAt(i)));
	}
}

//----------------------------------------------------------------
CXMLFixedKey* CXMLFixedKeyArray::GetFixedKeyByName(const CString& strKeyName) const
{
	for (int i = 0 ; i < GetSize() ; i++)
	{
		CXMLFixedKey* pFKTmp = GetAt(i);
		if ( pFKTmp->GetName().CompareNoCase(strKeyName) == 0 )
			return pFKTmp;
	}
	
	return NULL;
}

//----------------------------------------------------------------
BOOL CXMLFixedKeyArray::Parse(CXMLNode* pnFixedKeys) 
{
	if (!pnFixedKeys)
		return FALSE;
	
	RemoveAll();

	//prendo tutti i nodi universal key
	for(int i = 0 ; i < pnFixedKeys->GetChildsNum() ; i++)
	{
		CXMLNode* pnFixedKey = pnFixedKeys->GetChildAt(i);
		if(pnFixedKey)
		{
			//parsing del nodo UK e segmenti
			CXMLFixedKey* pXMLFixedKey = new CXMLFixedKey;
			pXMLFixedKey->Parse(pnFixedKey);
			Add(pXMLFixedKey);
		}
	}

	return TRUE;
}

//---------------------------------------------------------------------------------
BOOL CXMLFixedKeyArray::UnParse(CXMLNode* pnFixedKeys)
{
	if (!pnFixedKeys)
		return FALSE;
	
	CXMLNode* pnFixedKey = NULL;
	BOOL bOk = TRUE;

	//prendo tutti i nodi universal key
	for(int i = 0 ; i < GetSize() ; i++)
	{
		CXMLFixedKey* pFixedKey = GetAt(i);
		if(pFixedKey)
		{
			pnFixedKey = pnFixedKeys->CreateNewChild(XML_FIXED_SEG_TAG);
			bOk = pFixedKey->UnParse(pnFixedKey) && bOk;
		}		
	}

	return bOk;
}

//---------------------------------------------------------------------------------
BOOL CXMLFixedKeyArray::Remove(CXMLFixedKey* pXMLFixedKey)
{
	if(!pXMLFixedKey)
		return FALSE;
	
	for (int i = 0 ; i < GetSize() ; i++)
	{
		CXMLFixedKey* pFKTmp = GetAt(i);
		if (*pXMLFixedKey != *pFKTmp)
			continue;
		
		RemoveAt(i);
		return TRUE;
	}

	return FALSE;
}


//----------------------------------------------------------------------------------------------
BOOL CXMLFixedKeyArray::IsEqual(const CXMLFixedKeyArray& aXMLFixedKeyArray) const
{
	if (this == &aXMLFixedKeyArray)
		return TRUE;
	
	if	(aXMLFixedKeyArray.GetSize() != GetSize())
		return FALSE;
	
	CXMLFixedKey* pFixedKey = NULL;
	for(int i = 0 ; i < GetSize() ; i++)
	{
		pFixedKey = GetAt(i);
		if (
				(!pFixedKey || !aXMLFixedKeyArray.GetAt(i)) ||				
				(pFixedKey == aXMLFixedKeyArray.GetAt(i) || *pFixedKey != *aXMLFixedKeyArray.GetAt(i))
			)
			return FALSE;
	}

	return TRUE;
}

//----------------------------------------------------------------------------------------------
CXMLFixedKeyArray& CXMLFixedKeyArray::operator =(const CXMLFixedKeyArray& aXMLFixedKeyArray)
{
	if (this == &aXMLFixedKeyArray)
		return *this;

	RemoveAll();

	for(int i = 0 ; i < aXMLFixedKeyArray.GetSize() ; i++)
	{
		if(aXMLFixedKeyArray.GetAt(i))
			Add(new CXMLFixedKey(*aXMLFixedKeyArray.GetAt(i)));
	}
	return *this;
} 

//----------------------------------------------------------------------------------------------
// CXMLSearchBookmark
//----------------------------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(CXMLSearchBookmark, CObject)

//----------------------------------------------------------------------------------------------
CXMLSearchBookmark::CXMLSearchBookmark() 
:
	m_bShowAsDescription (FALSE)
{ 
}

//----------------------------------------------------------------------------------------------
CXMLSearchBookmark::CXMLSearchBookmark(CXMLSearchBookmark& aSearchBookmark)
{
	m_strName = aSearchBookmark.GetName();
	m_bShowAsDescription = aSearchBookmark.ShowAsDescription();
	m_strHKLName = aSearchBookmark.m_strHKLName;
}

//----------------------------------------------------------------------------------------------
BOOL CXMLSearchBookmark::IsEqual(const CXMLSearchBookmark& aSearchBookmark) const
{
	if (this == &aSearchBookmark)
		return TRUE;	

	return	(
				m_strName.CompareNoCase(aSearchBookmark.m_strName) == 0 &&
				m_bShowAsDescription == aSearchBookmark.m_bShowAsDescription && 
				m_strHKLName == aSearchBookmark.m_strHKLName 
			);
}

//----------------------------------------------------------------------------------------------
CXMLSearchBookmark& CXMLSearchBookmark::operator = (const CXMLSearchBookmark& aSearchBookmark)
{
	if (this == &aSearchBookmark)
		return *this;
	
	m_strName = aSearchBookmark.m_strName;
	m_bShowAsDescription = aSearchBookmark.m_bShowAsDescription;
	m_strHKLName = aSearchBookmark.m_strHKLName;
	return *this;
}


//----------------------------------------------------------------------------------------------
BOOL CXMLSearchBookmark::Parse(CXMLNode* pnSearchBookmark)
{
	if (!pnSearchBookmark)
		return FALSE;
	
	CString strValue;
	m_bShowAsDescription = FALSE;

	pnSearchBookmark->GetAttribute(XML_SEARCH_BOOKMARK_BOOK_NAME_ATTRIBUTE, m_strName);
	if (pnSearchBookmark->GetAttribute(XML_SEARCH_BOOKMARK_AS_DESCRI_ATTRIBUTE, strValue))
		m_bShowAsDescription = GetBoolFromXML(strValue);	

	if (pnSearchBookmark->GetAttribute(XML_SEARCH_BOOKMARKS_HKLNAME_ATTRIBUTE, strValue))
		m_strHKLName = strValue;

	if (pnSearchBookmark->GetAttribute(XML_SEARCH_BOOKMARKS_KEYCODE_ATTRIBUTE, strValue))
		m_strKeyCode = strValue;

	return TRUE;
}

//----------------------------------------------------------------------------------------------
BOOL CXMLSearchBookmark::UnParse(CXMLNode* pnSearchBookmark)
{
	if(!pnSearchBookmark)
		return FALSE;

	pnSearchBookmark->SetAttribute(XML_SEARCH_BOOKMARK_BOOK_NAME_ATTRIBUTE, m_strName);
	
	if (ShowAsDescription())
		pnSearchBookmark->SetAttribute(XML_SEARCH_BOOKMARK_AS_DESCRI_ATTRIBUTE, FormatBoolForXML(TRUE));

	if (!m_strHKLName.IsEmpty())
		pnSearchBookmark->SetAttribute(XML_SEARCH_BOOKMARKS_HKLNAME_ATTRIBUTE, m_strHKLName);

	return TRUE;
}


//----------------------------------------------------------------------------------------------
//	CXMLSearchBookmarkArray
//----------------------------------------------------------------------------------------------

IMPLEMENT_DYNAMIC(CXMLSearchBookmarkArray, Array)


//----------------------------------------------------------------------------------------------
CXMLSearchBookmarkArray::CXMLSearchBookmarkArray(const CXMLSearchBookmarkArray& aSearchBookmarkArray)
{
	RemoveAll();

	m_nVersion = aSearchBookmarkArray.m_nVersion;

	for(int i = 0 ; i < aSearchBookmarkArray.GetSize() ; i++)
	{
		if(aSearchBookmarkArray.GetAt(i))
			Add(new CXMLSearchBookmark(*aSearchBookmarkArray.GetAt(i)));
	}
}

//----------------------------------------------------------------
CXMLSearchBookmark* CXMLSearchBookmarkArray::GetSearchBookmarkByName(const CString& strName) const
{
	for (int i = 0 ; i < GetSize() ; i++)
	{
		CXMLSearchBookmark* pTmp = GetAt(i);
		if ( pTmp->GetName().CompareNoCase(strName) == 0 )
			return pTmp;
	}	
	return NULL;
}

/* 
	<SearchBookmarks>
		<Bookmark name ="CompanyName" showasdescription = "true"></Bookmark>
		<Bookmark name="TaxIdNumber"></Bookmark>
		<Bookmark name="FiscalCode"></Bookmark>
		<Bookmark name="ExternalCode"></Bookmark>
		<Bookmark name="County"></Bookmark>
	</SearchBookmarks>	>
*/
//----------------------------------------------------------------
BOOL CXMLSearchBookmarkArray::Parse(CXMLNode* pnBookmarks) 
{
	if (!pnBookmarks)
		return FALSE;
	
	RemoveAll();
	CString strVersion;
	m_nVersion = (pnBookmarks->GetAttribute(XML_SEARCH_BOOKMARKS_VERSION_ATTRIBUTE, strVersion)) ? _tstoi((LPCTSTR)strVersion) : 1;

	for(int i = 0 ; i < pnBookmarks->GetChildsNum() ; i++)
	{
		CXMLNode* pnBookmark = pnBookmarks->GetChildAt(i);
		if(pnBookmark)
		{
			CXMLSearchBookmark* pXMLBookmark = new CXMLSearchBookmark();
			pXMLBookmark->Parse(pnBookmark);
			Add(pXMLBookmark);
		}
	}

	return TRUE;
}

//---------------------------------------------------------------------------------
BOOL CXMLSearchBookmarkArray::UnParse(CXMLNode* pnBookmarks)
{
	if (!pnBookmarks)
		return FALSE;
	
	CXMLNode* pnBookmark = NULL;
	BOOL bOk = TRUE;

	CString strVersion;
	strVersion.Format(_T("%d"), m_nVersion);
	pnBookmarks->SetAttribute(XML_SEARCH_BOOKMARKS_VERSION_ATTRIBUTE, strVersion);

	for(int i = 0 ; i < GetSize() ; i++)
	{
		CXMLSearchBookmark* pBookmark = GetAt(i);
		if(pBookmark)
		{
			pnBookmark = pnBookmarks->CreateNewChild(XML_SEARCH_BOOKMARK_BOOK_TAG);
			bOk = pBookmark->UnParse(pnBookmark) && bOk;
		}		
	}

	return bOk;
}

//---------------------------------------------------------------------------------
CStringArray* CXMLSearchBookmarkArray::GetShowAsDescriptionFields() const
{
	CStringArray* pDescriptionFields = new CStringArray();
	
	for(int i = 0 ; i < GetSize() ; i++)
	{
		CXMLSearchBookmark* pBookmark = GetAt(i);
		if (pBookmark && pBookmark->ShowAsDescription())
			pDescriptionFields->Add(pBookmark->GetName());
	}
	return pDescriptionFields;
}

//---------------------------------------------------------------------------------
BOOL CXMLSearchBookmarkArray::Remove(CXMLSearchBookmark* pBookmark)
{
	if(!pBookmark)
		return FALSE;
	
	for (int i = 0 ; i < GetSize() ; i++)
	{
		CXMLSearchBookmark* pTmp = GetAt(i);
		if (*pBookmark != *pTmp)
			continue;
		
		RemoveAt(i);
		return TRUE;
	}

	return FALSE;
}


//----------------------------------------------------------------------------------------------
BOOL CXMLSearchBookmarkArray::IsEqual(const CXMLSearchBookmarkArray& aSearchBookmarkArray) const
{
	if (this == &aSearchBookmarkArray)
		return TRUE;
	
	if	(aSearchBookmarkArray.GetSize() != GetSize())
		return FALSE;
	
	CXMLSearchBookmark* pBookmark = NULL;
	for(int i = 0 ; i < GetSize() ; i++)
	{
		pBookmark = GetAt(i);
		if (
				(!pBookmark || !aSearchBookmarkArray.GetAt(i)) ||				
				(pBookmark == aSearchBookmarkArray.GetAt(i) || *pBookmark != *aSearchBookmarkArray.GetAt(i))
			)
			return FALSE;
	}

	return TRUE;
}

//----------------------------------------------------------------------------------------------
CXMLSearchBookmarkArray& CXMLSearchBookmarkArray::operator =(const CXMLSearchBookmarkArray& aSearchBookmarkArray)
{
	if (this == &aSearchBookmarkArray)
		return *this;

	RemoveAll();

	for(int i = 0 ; i < aSearchBookmarkArray.GetSize() ; i++)
	{
		if(aSearchBookmarkArray.GetAt(i))
			Add(new CXMLSearchBookmark(*aSearchBookmarkArray.GetAt(i)));
	}
	return *this;
} 

//----------------------------------------------------------------------------------------------
// CXMLHKLField
//----------------------------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(CXMLHKLField, CObject)

//----------------------------------------------------------------------------------------------
CXMLHKLField::CXMLHKLField()
:
	m_bIsXRefField(FALSE)
{
}

//----------------------------------------------------------------------------------------------
CXMLHKLField::CXMLHKLField(CXMLHKLField& aHKLField)
{
	m_strReportField = aHKLField.GetReportField();
	m_strDocumentField = aHKLField.GetDocumentField();
	m_bIsXRefField = aHKLField.IsXRefField();
}

//----------------------------------------------------------------------------------------------
BOOL CXMLHKLField::IsEqual(const CXMLHKLField& aHKLField) const
{
	if (this == &aHKLField)
		return TRUE;
	

	return	(
				aHKLField.GetReportField().CompareNoCase(m_strReportField) == 0 &&
				aHKLField.GetDocumentField().CompareNoCase(m_strDocumentField) == 0 &&
				aHKLField.IsXRefField() == m_bIsXRefField
			);
}

//----------------------------------------------------------------------------------------------
CXMLHKLField& CXMLHKLField::operator = (const CXMLHKLField& aHKLField)
{
	if (this == &aHKLField)
		return *this;
	
	m_strReportField = aHKLField.GetReportField();
	m_strDocumentField = aHKLField.GetDocumentField();
	m_bIsXRefField = aHKLField.IsXRefField();

	return *this;
}

//----------------------------------------------------------------------------------------------
void CXMLHKLField::Parse(CXMLNode* pnHKLField)
{
	if (!pnHKLField)
		return;
	
	pnHKLField->GetAttribute(XML_HKL_REPFIELD_ATTRIBUTE, m_strReportField);
	pnHKLField->GetAttribute(XML_HKL_DOCFIELD_ATTRIBUTE, m_strDocumentField);
	CString strIsXRef;
	pnHKLField->GetAttribute(XML_HKL_FROMXREF_ATTRIBUTE, strIsXRef);
	m_bIsXRefField = GetBoolFromXML(strIsXRef);		
}

//----------------------------------------------------------------------------------------------
void CXMLHKLField::UnParse(CXMLNode* pnHKLField)
{
	if(!pnHKLField)
		return;

	pnHKLField->SetAttribute(XML_HKL_REPFIELD_ATTRIBUTE, m_strReportField);
	
	if (!m_strDocumentField.IsEmpty())
		pnHKLField->SetAttribute(XML_HKL_DOCFIELD_ATTRIBUTE, m_strDocumentField);

	if (m_bIsXRefField)	
		pnHKLField->SetAttribute(XML_HKL_FROMXREF_ATTRIBUTE, FormatBoolForXML(m_bIsXRefField));	
}


//----------------------------------------------------------------------------------------------
//	CXMLHKLFieldArray
//----------------------------------------------------------------------------------------------

IMPLEMENT_DYNAMIC(CXMLHKLFieldArray, Array)

//----------------------------------------------------------------------------------------------
CXMLHKLFieldArray::CXMLHKLFieldArray(HKLListType eListType)
:
	m_eListType(eListType)
{  
	switch (m_eListType)
	{
		case PREVIEW_TYPE:
			m_strChildTagName = XML_HKL_PREVIEW_TAG; break;
		case FILTER_TYPE:
			m_strChildTagName = XML_HKL_FILTER_TAG; break;
		case RESULT_TYPE:
			m_strChildTagName = XML_HKL_RESULT_TAG; break;
	}
}

//----------------------------------------------------------------------------------------------
CXMLHKLFieldArray::CXMLHKLFieldArray(const CXMLHKLFieldArray& aHKLFieldArray)
{
    RemoveAll();
	m_eListType = aHKLFieldArray.m_eListType;
	for(int i = 0 ; i < aHKLFieldArray.GetSize() ; i++)
	{
		if(aHKLFieldArray.GetAt(i))
			Add(new CXMLHKLField(*aHKLFieldArray.GetAt(i)));
	}
}


//----------------------------------------------------------------
void CXMLHKLFieldArray::Parse(CXMLNode* pnHKLFields) 
{
	if (!pnHKLFields)
		return;
	
	RemoveAll();

	for(int i = 0 ; i < pnHKLFields->GetChildsNum() ; i++)
	{
		CXMLNode* pChild = pnHKLFields->GetChildAt(i);
		if(pChild)
		{
			//parsing del nodo UK e segmenti
			CXMLHKLField* pHKLField = new CXMLHKLField;
			pHKLField->Parse(pChild);
			Add(pHKLField);
		}
	}
}

//---------------------------------------------------------------------------------
void CXMLHKLFieldArray::UnParse(CXMLNode* pnHKLFields)
{
	if (!pnHKLFields)
		return;
	
	CXMLNode* pChild = NULL;
	BOOL bOk = TRUE;

	//prendo tutti i nodi universal key
	for(int i = 0 ; i < GetSize() ; i++)
	{
		CXMLHKLField* pHKLField = GetAt(i);
		if (pHKLField)
		{
			pChild = pnHKLFields->CreateNewChild(m_strChildTagName);
			pHKLField->UnParse(pChild);
		}		
	}
}

//---------------------------------------------------------------------------------
BOOL CXMLHKLFieldArray::Remove(CXMLHKLField* pHKLField)
{
	if(!pHKLField)
		return FALSE;
	
	for (int i = 0 ; i < GetSize() ; i++)
	{
		CXMLHKLField* pHKLTmp = GetAt(i);
		if (*pHKLField != *pHKLTmp)
			continue;
		
		RemoveAt(i);
		return TRUE;
	}

	return FALSE;
}


//----------------------------------------------------------------------------------------------
BOOL CXMLHKLFieldArray::IsEqual(const CXMLHKLFieldArray& aHKLFieldArray) const
{
	if (this == &aHKLFieldArray)
		return TRUE;
	
	if	(m_eListType != aHKLFieldArray.m_eListType ||	aHKLFieldArray.GetSize() != GetSize())
		return FALSE;
	
	CXMLHKLField* pHKLField = NULL;
	for(int i = 0 ; i < GetSize() ; i++)
	{
		pHKLField = GetAt(i);
		if (
				(!pHKLField || !aHKLFieldArray.GetAt(i)) ||				
				(pHKLField == aHKLFieldArray.GetAt(i) || *pHKLField != *aHKLFieldArray.GetAt(i))
			)
			return FALSE;
	}

	return TRUE;
}

//----------------------------------------------------------------------------------------------
CXMLHKLFieldArray& CXMLHKLFieldArray::operator =(const CXMLHKLFieldArray& aHKLFieldArray)
{
	if (this == &aHKLFieldArray)
		return *this;

	RemoveAll();

	m_eListType = aHKLFieldArray.m_eListType;
	for(int i = 0 ; i < aHKLFieldArray.GetSize() ; i++)
	{
		if(aHKLFieldArray.GetAt(i))
			Add(new CXMLHKLField(*aHKLFieldArray.GetAt(i)));
	}
	return *this;
} 


//----------------------------------------------------------------------------------------------
// CXMLHotKeyLink
//----------------------------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(CXMLHotKeyLink, CObject)

//----------------------------------------------------------------------------------------------
CXMLHotKeyLink::CXMLHotKeyLink()
:
	m_arPreviewFields(CXMLHKLFieldArray::PREVIEW_TYPE),
	m_arFilterFields (CXMLHKLFieldArray::FILTER_TYPE),
	m_arResultFields (CXMLHKLFieldArray::RESULT_TYPE),
	m_eFieldType	 (FIELD),
	m_eSubType		 (FIELD)
{
}

//----------------------------------------------------------------------------------------------
CXMLHotKeyLink::CXMLHotKeyLink(CXMLHotKeyLink& aXMLHKL)
:
	m_arPreviewFields(CXMLHKLFieldArray::PREVIEW_TYPE),
	m_arFilterFields (CXMLHKLFieldArray::FILTER_TYPE),
	m_arResultFields (CXMLHKLFieldArray::RESULT_TYPE)
{
	m_strFieldName	 = aXMLHKL.GetFieldName();
	m_nsReport		 = aXMLHKL.GetReportNamespace();
	m_eFieldType	 = aXMLHKL.m_eFieldType;
	m_eSubType		 = aXMLHKL.m_eSubType;
	m_strTextBoxField		= aXMLHKL.m_strTextBoxField;
	m_strDescriptionField	= aXMLHKL.m_strDescriptionField;
	m_strImageField			= aXMLHKL.m_strImageField;

	m_arPreviewFields= aXMLHKL.m_arPreviewFields;
	m_arFilterFields = aXMLHKL.m_arFilterFields;
	m_arResultFields = aXMLHKL.m_arResultFields;
}

//----------------------------------------------------------------------------------------------
BOOL CXMLHotKeyLink::IsEqual(const CXMLHotKeyLink& aXMLHKL) const
{
	if (this == &aXMLHKL)
		return TRUE;	

	return	(
				m_strFieldName.CompareNoCase(aXMLHKL.GetFieldName()) == 0 &&
				m_nsReport				== aXMLHKL.GetReportNamespace() &&
				m_eFieldType			== aXMLHKL.m_eFieldType &&				
				m_eSubType				== aXMLHKL.m_eSubType &&
				m_strTextBoxField		== aXMLHKL.m_strTextBoxField &&
				m_strDescriptionField	== aXMLHKL.m_strDescriptionField &&
				m_strImageField			== aXMLHKL.m_strImageField &&	
				m_arPreviewFields		== aXMLHKL.m_arPreviewFields &&
				m_arFilterFields		== aXMLHKL.m_arFilterFields &&
				m_arResultFields		== aXMLHKL.m_arResultFields
			);
}

//----------------------------------------------------------------------------------------------
CXMLHotKeyLink& CXMLHotKeyLink::operator = (const CXMLHotKeyLink& aXMLHKL)
{
	if (this == &aXMLHKL)
		return *this;
	
	m_strFieldName			= aXMLHKL.GetFieldName();
	m_nsReport				= aXMLHKL.GetReportNamespace();
	m_eFieldType			= aXMLHKL.m_eFieldType;
	m_eSubType				= aXMLHKL.m_eSubType;
	m_strTextBoxField		= aXMLHKL.m_strTextBoxField;
	m_strDescriptionField	= aXMLHKL.m_strDescriptionField;
	m_strImageField			= aXMLHKL.m_strImageField;

	m_arPreviewFields= aXMLHKL.m_arPreviewFields;
	m_arFilterFields = aXMLHKL.m_arFilterFields;
	m_arResultFields = aXMLHKL.m_arResultFields;

	return *this;
}

//----------------------------------------------------------------------------------------------
void CXMLHotKeyLink::SetReportNamespace(const CString& strNamespace)
{
	CTBNamespace nsReport(strNamespace);
	if (nsReport.IsValid())
		m_nsReport = nsReport;
}


//<HotKeyLinks>
//-	<Field name=”Description” report="Report.erp.contacts.paymentterms”>
//		<TextBoxFilter reportField=”MMM” />
//		<LBDescription reportField=”AAA” />
//		<Image reportField=”BBB” />
//		<Previews>
//			<Preview reportField=”AAA” />
//			<Preview reportField=”CCC” />
//		</ Previews>
//		<Filters>
//			<Filter reportField=”AAA” documentField=”BBB”/>
//			<Filter reportField=”III” documentField=”LLL”/>
//		</Filters>
//		<Results>
//			<Result reportField=”AAA” documentField=”BBB”/>
//			<Result reportField=”III” documentField=”LLL”/>
//		</Results>
//	</Field>
//</HotKeyLinks>

//----------------------------------------------------------------------------------------------
CString	CXMLHotKeyLink::GetStrHKLFieldType(HKLFieldType eType) const
{
	switch (eType)
	{
		case DBT:	return	_T("DBT");
		case XREF:	return	_T("XREF")	;
		case FIELD: return	_T("FIELD");
		
		default: return _T("FIELD");
	}	
}

//----------------------------------------------------------------------------------------------
void CXMLHotKeyLink::SetHKLFieldType(HKLFieldType& eType, const CString& strType)
{
	if (!strType.CompareNoCase(_T("DBT")))		
	{
		eType = DBT;
		return;
	}

	if (!strType.CompareNoCase(_T("XREF")))
	{
		eType = XREF;
		return;
	}

	if (!strType.CompareNoCase(_T("FIELD")))
	{
		eType = FIELD;
		return;
	}
			
	eType =  FIELD;
}

//----------------------------------------------------------------------------------------------
BOOL CXMLHotKeyLink::Parse(CXMLNode* pnHKL)
{
	if (!pnHKL)
		return FALSE;
	
	pnHKL->GetAttribute(XML_HKL_NAME_ATTRIBUTE, m_strFieldName);
	CString strNamespace;
	pnHKL->GetAttribute(XML_HKL_NS_REPORT_ATTRIBUTE, strNamespace);
	m_nsReport.SetNamespace(strNamespace);
	CString strFieldType;
	pnHKL->GetAttribute(XML_HKL_FIELDTYPE_ATTRIBUTE, strFieldType);
	SetHKLFieldType(m_eFieldType, strFieldType);
	if (m_eFieldType == XREF)
	{
		pnHKL->GetAttribute(XML_HKL_SUBTYPE_ATTRIBUTE, strFieldType);
		SetHKLFieldType(m_eSubType, strFieldType);
	}

	CXMLNode* pNode = pnHKL->GetChildByName(XML_HKL_TEXTBOXFILTER_TAG);
	if (pNode)
		pNode->GetText(m_strTextBoxField);

	pNode = pnHKL->GetChildByName(XML_HKL_LBDESCRIPTION_TAG);
	if (pNode)
		pNode->GetText(m_strDescriptionField);

	pNode = pnHKL->GetChildByName(XML_HKL_IMAGE_TAG);
	if (pNode)
		pNode->GetText(m_strImageField);

	pNode = pnHKL->GetChildByName(XML_HKL_PREVIEWS_TAG);
	if (pNode)
		m_arPreviewFields.Parse(pNode);
	
	pNode = pnHKL->GetChildByName(XML_HKL_FILTERS_TAG);
	if (pNode)
		m_arFilterFields.Parse(pNode);

	pNode = pnHKL->GetChildByName(XML_HKL_RESULTS_TAG);
	if (pNode)
		m_arResultFields.Parse(pNode);

	return TRUE;
}

//----------------------------------------------------------------------------------------------
BOOL CXMLHotKeyLink::UnParse(CXMLNode* pnHKL)
{
	if(!pnHKL)
		return FALSE;

	pnHKL->SetAttribute(XML_HKL_NAME_ATTRIBUTE, (LPCTSTR)m_strFieldName);
	pnHKL->SetAttribute(XML_HKL_NS_REPORT_ATTRIBUTE, (LPCTSTR)m_nsReport.ToString());
	pnHKL->SetAttribute(XML_HKL_FIELDTYPE_ATTRIBUTE, GetStrHKLFieldType(m_eFieldType));
	if (m_eFieldType == XREF)
		pnHKL->SetAttribute(XML_HKL_SUBTYPE_ATTRIBUTE, GetStrHKLFieldType(m_eSubType));

	CXMLNode* pChild = pnHKL->CreateNewChild(XML_HKL_TEXTBOXFILTER_TAG);
	if (pChild)
		pChild->SetText(m_strTextBoxField);
	
	pChild = pnHKL->CreateNewChild(XML_HKL_LBDESCRIPTION_TAG);
	if (pChild)
		pChild->SetText(m_strDescriptionField);

	pChild = pnHKL->CreateNewChild(XML_HKL_IMAGE_TAG);
	if (pChild)
		pChild->SetText(m_strImageField);

	if (m_arPreviewFields.GetSize() > 0)
	{
		pChild = pnHKL->CreateNewChild(XML_HKL_PREVIEWS_TAG);
		m_arPreviewFields.UnParse(pChild);
	}

	if (m_arFilterFields.GetSize() > 0)
	{
		pChild = pnHKL->CreateNewChild(XML_HKL_FILTERS_TAG);
		m_arFilterFields.UnParse(pChild);
	}

	if (m_arResultFields.GetSize() > 0)
	{
		pChild = pnHKL->CreateNewChild(XML_HKL_RESULTS_TAG);
		m_arResultFields.UnParse(pChild);
	}

	return TRUE;
}


//----------------------------------------------------------------------------------------------
//	CXMLHotKeyLinkArray
//----------------------------------------------------------------------------------------------
//
IMPLEMENT_DYNAMIC(CXMLHotKeyLinkArray, Array)

//----------------------------------------------------------------------------------------------
CXMLHotKeyLinkArray::CXMLHotKeyLinkArray()
{}

//----------------------------------------------------------------------------------------------
CXMLHotKeyLinkArray::CXMLHotKeyLinkArray(const CXMLHotKeyLinkArray& aHKLArray)
{
	RemoveAll();
	for(int i = 0 ; i < aHKLArray.GetSize() ; i++)
	{
		if(aHKLArray.GetAt(i))
			Add(new CXMLHotKeyLink(*aHKLArray.GetAt(i)));
	}
}

//----------------------------------------------------------------
CXMLHotKeyLink* CXMLHotKeyLinkArray::GetHKLByFieldName(
															const CString& strFieldName, 
															CXMLHotKeyLink::HKLFieldType eType, 
															CXMLHotKeyLink::HKLFieldType eSubType /*= CXMLHotKeyLink::FIELD*/
														) const
{
	for (int i = 0 ; i < GetSize() ; i++)
	{
		CXMLHotKeyLink* pXMLHKL = GetAt(i);
		CString strField = pXMLHKL->GetFieldName();
		if (
				pXMLHKL->GetFieldName().CompareNoCase(strFieldName) == 0 &&	
				pXMLHKL->GetHKLFieldType()== eType && 
				pXMLHKL->GetHKLSubType() == eSubType
			)
			return pXMLHKL;
	}
	
	return NULL;
}
//----------------------------------------------------------------
void CXMLHotKeyLinkArray::GetAllHKLForType(CXMLHotKeyLink::HKLFieldType eType, CXMLHotKeyLinkArray* pHKLArray, const CString& strXRefName /*= _T("")*/) const
{
	if (!pHKLArray)
		pHKLArray = new CXMLHotKeyLinkArray;

	for (int i = 0 ; i < GetSize() ; i++)
	{
		CXMLHotKeyLink* pXMLHKL = GetAt(i);
		if (!GetAt(i) || GetAt(i)->GetHKLFieldType() != eType)
			continue;

		if (GetAt(i)->GetHKLFieldType() != CXMLHotKeyLink::XREF || IsHKLOfThisExtRef(pXMLHKL->GetFieldName(), strXRefName))
			pHKLArray->Add(new CXMLHotKeyLink(*pXMLHKL));
	}
}


//----------------------------------------------------------------
BOOL CXMLHotKeyLinkArray::Parse(CXMLNode* pnHKLs) 
{
	if (!pnHKLs)
		return FALSE;

	CString strFieldName;	
	//prendo tutti i nodi HotKeyLinks
	for(int i = 0 ; i < pnHKLs->GetChildsNum() ; i++)
	{
		CXMLNode* pnHKL = pnHKLs->GetChildAt(i);
		if (pnHKL)
		{
			//parsing del nodo UK e segmenti
			CXMLHotKeyLink* pXMLHKL = new CXMLHotKeyLink;
			pXMLHKL->Parse(pnHKL);
			Add(pXMLHKL);
		}
	}

	return TRUE;
}

//---------------------------------------------------------------------------------
BOOL CXMLHotKeyLinkArray::UnParse(CXMLNode* pnHKLs)
{
	if (!pnHKLs)
		return FALSE;
	
	CXMLNode* pnHKL = NULL;
	BOOL bOk = TRUE;

	//save all the hlks linked to dbt's fields
	for(int i = 0 ; i < GetSize() ; i++)
	{
		CXMLHotKeyLink* pXMLHKL = GetAt(i);
		if(pXMLHKL)
		{
			pnHKL = pnHKLs->CreateNewChild(XML_HKL_FIELD_TAG);
			bOk = pXMLHKL->UnParse(pnHKL) && bOk;
		}		
	}

	return bOk;
}

//---------------------------------------------------------------------------------
BOOL CXMLHotKeyLinkArray::Remove(CXMLHotKeyLink* pXMLHKL)
{
	if(!pXMLHKL)
		return FALSE;
	
	for (int i = 0 ; i < GetSize() ; i++)
	{
		CXMLHotKeyLink* pHKLTmp = GetAt(i);
		if (*pXMLHKL != *pHKLTmp)
			continue;
		
		RemoveAt(i);
		return TRUE;
	}

	return FALSE;
}

//---------------------------------------------------------------------------------------------
void CXMLHotKeyLinkArray::RemoveOnlyForDBTHKL()
{
	for (int i = GetUpperBound() ; i >= 0 ; i--)
	{
		CXMLHotKeyLink* pHKLTmp = GetAt(i);
		if (pHKLTmp->GetHKLFieldType() != CXMLHotKeyLink::XREF)
			RemoveAt(i);
	}
}

//---------------------------------------------------------------------------------------------
BOOL CXMLHotKeyLinkArray::IsHKLOfThisExtRef(const CString& strHKLFieldName, const CString& strXRefName)
{
	CString strToken (URL_SLASH_CHAR);
	int	nPos = 0;
	if (strHKLFieldName.IsEmpty())
	return FALSE;

	CString strElem = strHKLFieldName.Tokenize(strToken, nPos);
	return !strElem.CompareNoCase(strXRefName);
}

//---------------------------------------------------------------------------------------------
void CXMLHotKeyLinkArray::RemoveOnlyForXRefHKL(const CString& strXRefName)
{
	if (strXRefName.IsEmpty())
		return;

	for (int i = GetUpperBound() ; i >= 0 ; i--)
	{
		CXMLHotKeyLink* pHKLTmp = GetAt(i);
		if (pHKLTmp->GetHKLFieldType() != CXMLHotKeyLink::XREF)
			continue;
		if (IsHKLOfThisExtRef(pHKLTmp->GetFieldName(),strXRefName))
			RemoveAt(i);
	}
}

//----------------------------------------------------------------------------------------------
BOOL CXMLHotKeyLinkArray::IsEqual(const CXMLHotKeyLinkArray& aHKLArray) const
{
	if (this == &aHKLArray)
		return TRUE;
	
	if	(aHKLArray.GetSize() != GetSize())
		return FALSE;
	
	CXMLHotKeyLink* pXMLHKL = NULL;
	for(int i = 0 ; i < GetSize() ; i++)
	{
		pXMLHKL = GetAt(i);
		if (
				(!pXMLHKL && aHKLArray.GetAt(i)) ||				
				(pXMLHKL && !aHKLArray.GetAt(i)) || 
				*pXMLHKL != *aHKLArray.GetAt(i)
			)
			return FALSE;
	}

	return TRUE;
}

//----------------------------------------------------------------------------------------------
CXMLHotKeyLinkArray& CXMLHotKeyLinkArray::operator =(const CXMLHotKeyLinkArray& aHKLArray)
{
	if (this == &aHKLArray)
		return *this;
	
	RemoveAll();

	for(int i = 0 ; i < aHKLArray.GetSize() ; i++)
	{
		if(aHKLArray.GetAt(i))
			Add(new CXMLHotKeyLink(*aHKLArray.GetAt(i)));
	}
	return *this;
} 

//----------------------------------------------------------------------------------------------
//	CXMLDBTInfo implementation
//----------------------------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(CXMLDBTInfo, CObject)
//----------------------------------------------------------------------------------------------
CXMLDBTInfo::CXMLDBTInfo()
	:
	m_pXRefsArray				(NULL),	
	m_pXMLFieldInfoArray		(NULL),
	m_pXMLUniversalKeyGroup		(NULL),
	m_pXMLFixedKeyArray			(NULL),
	m_pXMLHotKeyLinkArray		(NULL),
	m_pXMLSearchBookmarkArray	(NULL),
	m_pXRefsToAppendArray		(NULL),
	m_bExport					(TRUE),
	m_bChooseUpdate				(FALSE),
	m_bIsFrom					(STANDARD),
	m_eType						(MASTER_TYPE),
	m_eUpdateType				(REPLACE)
{
}

//----------------------------------------------------------------------------------------------
CXMLDBTInfo::CXMLDBTInfo(CXMLDBTInfo& aDBTInfo)
{
	m_nsDBT					= aDBTInfo.m_nsDBT;
	m_strTitle				= aDBTInfo.m_strTitle;
	m_strOriginalTitle		= aDBTInfo.m_strOriginalTitle;
	m_strTableName			= aDBTInfo.m_strTableName;
	m_nsTable				= aDBTInfo.m_nsTable;
	m_eType					= aDBTInfo.m_eType;
	m_eUpdateType			= aDBTInfo.m_eUpdateType;
	m_bExport				= aDBTInfo.m_bExport;
	m_bChooseUpdate			= aDBTInfo.m_bChooseUpdate;
	m_bIsFrom				= aDBTInfo.m_bIsFrom;
	m_pXRefsArray			= NULL;
	m_pXMLFieldInfoArray	= NULL;
	m_pXMLUniversalKeyGroup	= NULL;
	m_pXMLFixedKeyArray		= NULL;
	m_pXMLHotKeyLinkArray	= NULL;
	m_pXMLSearchBookmarkArray= NULL;
	m_pXRefsToAppendArray	 = NULL;
	
	SetXMLXRefInfoArray			(aDBTInfo);
	SetXMLFieldInfoArray		(aDBTInfo);
	SetXMLUniversalKeyGroup		(aDBTInfo);
	SetXMLFixedKeyArray			(aDBTInfo);
	SetXMLHotKeyLinkArray		(aDBTInfo);
	SetXMLSearchBookmarkArray	(aDBTInfo);
	SetXMLXRefInfoToAppendArray (aDBTInfo);
}


//----------------------------------------------------------------------------------------------
CXMLDBTInfo::~CXMLDBTInfo()
{
	SAFE_DELETE(m_pXRefsArray);
	SAFE_DELETE(m_pXMLFieldInfoArray);
	SAFE_DELETE(m_pXMLUniversalKeyGroup);
	SAFE_DELETE(m_pXMLFixedKeyArray);	
	SAFE_DELETE(m_pXMLHotKeyLinkArray);	
	SAFE_DELETE(m_pXMLSearchBookmarkArray);
	SAFE_DELETE(m_pXRefsToAppendArray);
}

//----------------------------------------------------------------------------------------------
CXMLDBTInfo::UpdateType CXMLDBTInfo::GetUpdateTypeFromString(const CString& strType)
{
	if (!strType.CompareNoCase(XML_DBT_UPDATE_REPLACE_TAG))		
		return REPLACE;

	if (!strType.CompareNoCase(XML_DBT_UPDATE_INSERTUPDATE_TAG))
		return INSERT_UPDATE;

	if (!strType.CompareNoCase(XML_DBT_UPDATE_ONLYINSERT_TAG))
		return ONLY_INSERT;
				
	return (GetType() == CXMLDBTInfo::SLAVE_TYPE) 
			? INSERT_UPDATE
			: REPLACE;
}

//----------------------------------------------------------------------------------------------
BOOL CXMLDBTInfo::IsEqual(const CXMLDBTInfo& aDbt) const
{
	if (this == &aDbt)
		return TRUE;
	
	if(
		(!m_pXMLUniversalKeyGroup	&& aDbt.m_pXMLUniversalKeyGroup)	||
		(m_pXMLUniversalKeyGroup	&& !aDbt.m_pXMLUniversalKeyGroup)	||
		(!m_pXMLFieldInfoArray		&& aDbt.m_pXMLFieldInfoArray)		||
		(m_pXMLFieldInfoArray		&& !aDbt.m_pXMLFieldInfoArray)		||		
		(!m_pXMLFixedKeyArray		&& aDbt.m_pXMLFixedKeyArray)		||
		(m_pXMLHotKeyLinkArray		&& !aDbt.m_pXMLHotKeyLinkArray)	    ||
		(m_pXMLSearchBookmarkArray && !aDbt.m_pXMLSearchBookmarkArray)	||
		(m_pXRefsToAppendArray && !aDbt.m_pXRefsToAppendArray)
	  )
		return FALSE;

	return
		(m_nsDBT			== aDbt.m_nsDBT			) &&
		(m_strTitle			== aDbt.m_strTitle		) &&
		(m_strOriginalTitle == aDbt.m_strOriginalTitle ) &&
		(m_strTableName		== aDbt.m_strTableName	) &&
		(m_eType			== aDbt.m_eType			) &&
		(m_eUpdateType		== aDbt.m_eUpdateType	) &&
		(m_bExport			== aDbt.m_bExport		) &&
		(m_bChooseUpdate	== aDbt.m_bChooseUpdate ) &&
		(m_nsTable			== aDbt.m_nsTable		) &&
		(m_bIsFrom			== aDbt.m_bIsFrom) &&
		((!m_pXMLFieldInfoArray && !aDbt.m_pXMLFieldInfoArray) ||
			(m_pXMLFieldInfoArray && aDbt.m_pXMLFieldInfoArray && *m_pXMLFieldInfoArray == *aDbt.m_pXMLFieldInfoArray)) &&
		((!m_pXMLUniversalKeyGroup && !aDbt.m_pXMLUniversalKeyGroup) ||
			(m_pXMLUniversalKeyGroup && aDbt.m_pXMLUniversalKeyGroup && *m_pXMLUniversalKeyGroup == *aDbt.m_pXMLUniversalKeyGroup)) &&
		((!m_pXRefsArray && !aDbt.m_pXRefsArray) || (m_pXRefsArray && aDbt.m_pXRefsArray && *m_pXRefsArray == *aDbt.m_pXRefsArray)) &&
			((!m_pXMLFixedKeyArray && !aDbt.m_pXMLFixedKeyArray) ||
		(m_pXMLFixedKeyArray && aDbt.m_pXMLFixedKeyArray && *m_pXMLFixedKeyArray == *aDbt.m_pXMLFixedKeyArray)) &&
			((!m_pXMLHotKeyLinkArray && !aDbt.m_pXMLHotKeyLinkArray) ||
		(m_pXMLHotKeyLinkArray && aDbt.m_pXMLHotKeyLinkArray && *m_pXMLHotKeyLinkArray == *aDbt.m_pXMLHotKeyLinkArray)) &&
			((!m_pXMLSearchBookmarkArray && !aDbt.m_pXMLSearchBookmarkArray) ||
		(m_pXMLSearchBookmarkArray && aDbt.m_pXMLSearchBookmarkArray && *m_pXMLSearchBookmarkArray == *aDbt.m_pXMLSearchBookmarkArray)) &&
		((!m_pXRefsToAppendArray && !aDbt.m_pXRefsToAppendArray) || (m_pXRefsToAppendArray && aDbt.m_pXRefsToAppendArray && *m_pXRefsToAppendArray == *aDbt.m_pXRefsToAppendArray));
}

//----------------------------------------------------------------------------------------------
CXMLDBTInfo& CXMLDBTInfo::operator =(const CXMLDBTInfo& aDbt)
{
	if (this == &aDbt)
		return *this;
	
	m_nsDBT				= aDbt.m_nsDBT			;
	m_strTitle			= aDbt.m_strTitle		;
	m_strOriginalTitle  = aDbt.m_strOriginalTitle;
	m_strTableName		= aDbt.m_strTableName	;
	m_eType				= aDbt.m_eType			;
	m_eUpdateType		= aDbt.m_eUpdateType	;
	m_bExport			= aDbt.m_bExport		;
	m_bChooseUpdate		= aDbt.m_bChooseUpdate	;
	m_nsTable			= aDbt.m_nsTable		;
	m_bIsFrom			= aDbt.m_bIsFrom;

	if (m_pXRefsArray)
		delete m_pXRefsArray;

	m_pXRefsArray = new CXMLXRefInfoArray(*aDbt.m_pXRefsArray);

	if (m_pXMLFieldInfoArray)
		delete m_pXMLFieldInfoArray;
	m_pXMLFieldInfoArray = new CXMLFieldInfoArray(*aDbt.m_pXMLFieldInfoArray);

	if (m_pXMLUniversalKeyGroup)
		delete m_pXMLUniversalKeyGroup;	
	m_pXMLUniversalKeyGroup = new CXMLUniversalKeyGroup(*aDbt.m_pXMLUniversalKeyGroup);
	
	if (m_pXMLFixedKeyArray)
		delete m_pXMLFixedKeyArray;	
	m_pXMLFixedKeyArray = new CXMLFixedKeyArray(*aDbt.m_pXMLFixedKeyArray);

	if (m_pXMLHotKeyLinkArray)
		delete m_pXMLHotKeyLinkArray;	
	m_pXMLHotKeyLinkArray = new CXMLHotKeyLinkArray(*aDbt.m_pXMLHotKeyLinkArray);

	if (m_pXMLSearchBookmarkArray)
		delete m_pXMLSearchBookmarkArray;	
	m_pXMLSearchBookmarkArray = new CXMLSearchBookmarkArray(*aDbt.m_pXMLSearchBookmarkArray);

	if (m_pXRefsToAppendArray)
		delete m_pXRefsToAppendArray;
	m_pXRefsToAppendArray = new CXMLXReferencesToAppendArray(*aDbt.m_pXRefsToAppendArray);	


	return *this;
}

//----------------------------------------------------------------------------------------------
int CXMLDBTInfo::AddXRef(CXMLXRefInfo* pXRef)
{
	if(pXRef)
	{
		if(!m_pXRefsArray)
			m_pXRefsArray = new CXMLXRefInfoArray;

		return m_pXRefsArray->Add(pXRef);
	}

	return -1;	
}

//----------------------------------------------------------------------------------------------
void CXMLDBTInfo::SetXRefAt(int nIdx, CXMLXRefInfo* pXRef)
{
	if(!m_pXRefsArray || m_pXRefsArray->GetSize() < nIdx || nIdx < 0)
	{
		ASSERT_VALID(m_pXRefsArray);
		return;
	}
	m_pXRefsArray->SetAt(nIdx, pXRef);
}

//----------------------------------------------------------------------------------------------
CXMLXRefInfo* CXMLDBTInfo::GetXRefAt(int i)const
{
	if(!m_pXRefsArray || m_pXRefsArray->GetSize() < i || i < 0)
		return NULL;

	return m_pXRefsArray->GetAt(i);
}

//----------------------------------------------------------------------------------------------
CXMLXRefInfo* CXMLDBTInfo::GetXRefByFK(LPCTSTR lpszColName)const
{
	if (!m_pXRefsArray)
		return NULL;

	for (int i = 0 ; i < m_pXRefsArray->GetSize() ; i++)
	{
		CXMLXRefInfo* pXRef = m_pXRefsArray->GetAt(i);
		
		if(!pXRef)
			continue;

		for ( int nSegIdx = 0 ; nSegIdx < pXRef->GetSegmentsNum() ; nSegIdx++)
		{
			if(!(pXRef->GetSegmentAt(nSegIdx)->GetFKSegment().CompareNoCase(lpszColName)))
				return pXRef;
		}
	}
	return NULL;
}

//----------------------------------------------------------------------------------------------
CXMLXRefInfo* CXMLDBTInfo::GetXRefByName(LPCTSTR lpszName)const
{
	if (!m_pXRefsArray)
		return NULL;

	for (int i = 0 ; i < m_pXRefsArray->GetSize() ; i++)
	{
		CXMLXRefInfo* pXRef = m_pXRefsArray->GetAt(i);
		
		if(!pXRef)
			continue;

		if(pXRef->GetName().CompareNoCase(lpszName) == 0)
			return pXRef;
	}
	return NULL;
}

//----------------------------------------------------------------------------------------------
void CXMLDBTInfo::RemoveXRefAt(int i)
{
	if(!m_pXRefsArray || m_pXRefsArray->GetSize() < i || i < 0)
		return;

	m_pXRefsArray->RemoveAt(i);
}

//----------------------------------------------------------------------------------------------
void CXMLDBTInfo::RemoveXRef(const CXMLXRefInfo& aXRefInfo)
{
	if (!m_pXRefsArray)
		return;

	for (int i = 0 ; i < m_pXRefsArray->GetSize() ; i++)
	{
		CXMLXRefInfo* pXRef = m_pXRefsArray->GetAt(i);
		if (aXRefInfo != *pXRef)
			continue;
		
		RemoveXRefAt(i);
		return;
	}
}

//----------------------------------------------------------------------------
void CXMLDBTInfo::SetXRefUseFlag(BOOL bUse /*= TRUE*/)
{
	if(!GetXMLXRefInfoArray())
		return;

	for(int n = 0 ; n < GetXMLXRefInfoArray()->GetSize() ; n++)
	{
		CXMLXRefInfo* pXRefInfo = GetXMLXRefInfoArray()->GetAt(n);
		if(pXRefInfo)
			pXRefInfo->SetUse(bUse);
	}
}

//----------------------------------------------------------------------------------------------
int CXMLDBTInfo::AddField(CXMLFieldInfo* pField)
{
	if(pField)
	{
		if(!m_pXMLFieldInfoArray)
			m_pXMLFieldInfoArray = new CXMLFieldInfoArray;

		return m_pXMLFieldInfoArray->Add(pField);
	}

	return -1;	
}

//----------------------------------------------------------------------------------------------
void CXMLDBTInfo::SetFieldAt(int nIdx, CXMLFieldInfo* pField)
{
	if(!m_pXMLFieldInfoArray || m_pXMLFieldInfoArray->GetSize() < nIdx || nIdx < 0)
	{
		ASSERT_VALID(m_pXMLFieldInfoArray);
		return;
	}
	m_pXMLFieldInfoArray->SetAt(nIdx, pField);
}

//----------------------------------------------------------------------------------------------
CXMLFieldInfo* CXMLDBTInfo::GetFieldAt(int i)const
{
	if(!m_pXMLFieldInfoArray || m_pXMLFieldInfoArray->GetSize() < i || i < 0)
		return NULL;

	return m_pXMLFieldInfoArray->GetAt(i);
}

//----------------------------------------------------------------------------------------------
void CXMLDBTInfo::RemoveFieldAt(int i)
{
	if(!m_pXMLFieldInfoArray || m_pXMLFieldInfoArray->GetSize() < i || i < 0)
		return;

	m_pXMLFieldInfoArray->RemoveAt(i);
}

//----------------------------------------------------------------------------------------------
void CXMLDBTInfo::RemoveField(const CXMLFieldInfo& aFieldInfo)
{
	if (!m_pXMLFieldInfoArray)
		return;

	for (int i = 0 ; i < m_pXMLFieldInfoArray->GetSize() ; i++)
	{
		CXMLFieldInfo* pField = m_pXMLFieldInfoArray->GetAt(i);
		if (aFieldInfo != *pField)
			continue;
		
		RemoveFieldAt(i);
		return;
	}
}
//----------------------------------------------------------------------------------------------
BOOL CXMLDBTInfo::IsFieldToExport(const CString& strFieldName)
{
	return (!m_pXMLFieldInfoArray || m_pXMLFieldInfoArray->IsToExport(strFieldName));
}

//----------------------------------------------------------------------------------------------
int CXMLDBTInfo::AddXMLUniversalKey(CXMLUniversalKey* pUniversalKey)
{
	if(pUniversalKey)
	{
		if(!m_pXMLUniversalKeyGroup)
			m_pXMLUniversalKeyGroup = new CXMLUniversalKeyGroup;

		return m_pXMLUniversalKeyGroup->Add(pUniversalKey);
	}

	return -1;	
}

//----------------------------------------------------------------------------------------------
void CXMLDBTInfo::SetXMLUniversalKeyAt(int nIdx, CXMLUniversalKey* pUniversalKey)
{
	if(!m_pXMLUniversalKeyGroup || m_pXMLUniversalKeyGroup->GetSize() < nIdx || nIdx < 0)
	{
		ASSERT_VALID(m_pXMLUniversalKeyGroup);
		return;
	}
	m_pXMLUniversalKeyGroup->SetAt(nIdx, pUniversalKey);
}

//----------------------------------------------------------------------------------------------
CXMLUniversalKey* CXMLDBTInfo::GetXMLUniversalKeyAt(int i)const
{
	if(!m_pXMLUniversalKeyGroup || m_pXMLUniversalKeyGroup->GetSize() < i || i < 0)
		return NULL;

	return m_pXMLUniversalKeyGroup->GetAt(i);
}

//----------------------------------------------------------------------------------------------
void CXMLDBTInfo::RemoveXMLUniversalKeyAt(int i)
{
	if(!m_pXMLUniversalKeyGroup || m_pXMLUniversalKeyGroup->GetSize() < i || i < 0)
		return;

	m_pXMLUniversalKeyGroup->RemoveAt(i);
}

//----------------------------------------------------------------------------------------------
void CXMLDBTInfo::RemoveXMLUniversalKey(const CXMLUniversalKey& aUniversalKey)
{
	if (!m_pXMLUniversalKeyGroup)
		return;

	for (int i = 0 ; i < m_pXMLUniversalKeyGroup->GetSize() ; i++)
	{
		CXMLUniversalKey* pUniversalKey = m_pXMLUniversalKeyGroup->GetAt(i);
		if (aUniversalKey != *pUniversalKey)
			continue;
		
		RemoveFieldAt(i);
		return;
	}
}

//----------------------------------------------------------------------------------------------
CString	CXMLDBTInfo::GetStrUpdateType()	const
{
	switch (m_eUpdateType)
	{
		case REPLACE:		return	XML_DBT_UPDATE_REPLACE_TAG;
		case INSERT_UPDATE:	return	XML_DBT_UPDATE_INSERTUPDATE_TAG;
		case ONLY_INSERT:	return	XML_DBT_UPDATE_ONLYINSERT_TAG;
		
		default: return (GetType() == CXMLDBTInfo::SLAVE_TYPE) 
									? XML_DBT_UPDATE_INSERTUPDATE_TAG
									: XML_DBT_UPDATE_REPLACE_TAG;
	}	
}

//----------------------------------------------------------------------------------------------
void CXMLDBTInfo::SetUpdateType(const CString& strType)
{	
	m_eUpdateType = CXMLDBTInfo::GetUpdateTypeFromString(strType);
}

//----------------------------------------------------------------------------------------------
CString	CXMLDBTInfo::GetStrType()	const
{
	switch (m_eType)
	{
		case MASTER_TYPE:	return	XML_DBT_TYPE_MASTER_TAG;
		case SLAVE_TYPE:	return	XML_DBT_TYPE_SLAVE_TAG	;
		case BUFFERED_TYPE: return	XML_DBT_TYPE_BUFFERED_TAG;
		case SLAVABLE_TYPE: return	XML_DBT_TYPE_SLAVABLE_TAG;
		
		default: return XML_DBT_TYPE_UNDEF;
	}	
}

//----------------------------------------------------------------------------------------------
void CXMLDBTInfo::SetType(const CString& strType)
{
	if (!strType.CompareNoCase(XML_DBT_TYPE_MASTER_TAG))		
	{
		m_eType = MASTER_TYPE;
		return;
	}

	if (!strType.CompareNoCase(XML_DBT_TYPE_SLAVE_TAG))
	{
		m_eType = SLAVE_TYPE;
		return;
	}

	if (!strType.CompareNoCase(XML_DBT_TYPE_BUFFERED_TAG))
	{
		m_eType = BUFFERED_TYPE;
		return;
	}

	if (!strType.CompareNoCase(XML_DBT_TYPE_SLAVABLE_TAG))
	{
		m_eType = SLAVABLE_TYPE;
		return;
	}
				
	m_eType =  UNDEF_TYPE;
}


//----------------------------------------------------------------------------------------------
BOOL CXMLDBTInfo::SetDBTInfo(CXMLNode* pXMLNode)
{	
	if (!pXMLNode) 
		return FALSE;

	BOOL bIsStandard = AfxGetPathFinder()->IsStandardPath(pXMLNode->GetXMLDocument()->GetFileName());
	SetFrom(bIsStandard ? CXMLDBTInfo::STANDARD : CXMLDBTInfo::CUSTOM);

	CString strText;	
	
	pXMLNode->GetName(strText);
	SetType(strText);

	if (pXMLNode->GetAttribute(XML_NAMESPACE_ATTRIBUTE, strText))
	{
		CTBNamespace aNs;
		aNs.AutoCompleteNamespace(CTBNamespace::DBT, strText, aNs);
		m_nsDBT = aNs.ToString();
	}

	CXMLNode* pChildNode = NULL;
	
	pChildNode = pXMLNode->GetChildByName(XML_TITLE_TAG);
	if (pChildNode)
	{
		pChildNode->GetText(m_strTitle); 
		((CLocalizableXMLNode*)pChildNode)->GetLocalizableText(m_strOriginalTitle);	
		if (m_strOriginalTitle.IsEmpty())
			m_strOriginalTitle = m_strTitle;
	}
			
	pChildNode =  pXMLNode->GetChildByName(XML_TABLE_TAG);
	if (pChildNode)
	{
		pChildNode->GetText(m_strTableName);
		pChildNode->GetAttribute(XML_NAMESPACE_ATTRIBUTE, strText);
		if(strText.IsEmpty())
			return TRUE;
		m_nsTable.AutoCompleteNamespace(CTBNamespace::TABLE, strText, m_nsTable);
	}

	pChildNode = pXMLNode->GetChildByName(XML_CHOOSEUPDATE_TAG);
	if (pChildNode)
	{
		pChildNode->GetText(strText);
		m_bChooseUpdate = GetBoolFromXML(strText);
	}
	
	return TRUE;	
}

//---------------------------------------------------------------------------------------------
BOOL CXMLDBTInfo::SetDBTInfo(DBTObject* pDBT)
{
	if(!pDBT || !pDBT->GetRuntimeClass())
		return FALSE;

	m_eType = CXMLDBTInfo::UNDEF_TYPE;
	m_nsDBT = pDBT->GetNamespace();
		
	m_strTitle = m_nsDBT.GetObjectName(CTBNamespace::DBT);
	m_strOriginalTitle = m_nsDBT.GetObjectName(CTBNamespace::DBT);

	SqlTable* pTable = pDBT->GetTable();

	if(!pTable)
		return FALSE;

	m_strTableName = pTable->GetTableName();

	if(!AfxGetDefaultSqlConnection()->GetCatalogEntry(m_strTableName))
		return FALSE;

	m_nsTable = AfxGetDefaultSqlConnection()->GetCatalogEntry(m_strTableName)->GetNamespace();
	
	if(pDBT->IsKindOf(RUNTIME_CLASS(DBTMaster)))
		m_eType = CXMLDBTInfo::MASTER_TYPE;
	else
	{
		if(pDBT->IsKindOf(RUNTIME_CLASS(DBTSlaveBuffered)))
				m_eType = CXMLDBTInfo::BUFFERED_TYPE;
		else
		{
			if(pDBT->IsKindOf(RUNTIME_CLASS(DBTSlave)))
			m_eType = CXMLDBTInfo::SLAVE_TYPE;
		}
	}//todo SLAVABLE
				
	m_bExport = TRUE;
	m_bChooseUpdate = FALSE;

	if(m_pXRefsArray)
		delete m_pXRefsArray;

	m_pXRefsArray = new CXMLXRefInfoArray;
	
	return TRUE;
}

//---------------------------------------------------------------------------------------------
BOOL CXMLDBTInfo::LoadExternalReference (CLocalizableXMLDocument* pXMLXRefDoc)					
{	
	if (!pXMLXRefDoc)
		return TRUE;
	
	CXMLNode* pRoot = pXMLXRefDoc->GetRoot();

	CXMLNode* pDocInfoNode;
	CXMLNode* pDBTInfoNode;

	CString strText;

	pDBTInfoNode = pRoot->GetChildByAttributeValue(XML_DBT_TAG, XML_NAMESPACE_ATTRIBUTE, m_nsDBT.ToUnparsedString(), FALSE); 
	if ( pDBTInfoNode )
	{
		pDocInfoNode = pDBTInfoNode->GetChildByName(XML_EXPORT_TAG);
		if (pDocInfoNode)
		{
			pDocInfoNode->GetText(strText);
			m_bExport = GetBoolFromXML(strText);
		}

		pDocInfoNode = pDBTInfoNode->GetChildByName(XML_DBT_UPDATETYPE_TAG);
		if (pDocInfoNode)
		{
			pDocInfoNode->GetText(strText);
			SetUpdateType(strText);
		}


		pDocInfoNode = pDBTInfoNode->GetChildByName(XML_EXTERNAL_REFERENCES_TAG);
		if (pDocInfoNode)
		{
			if(m_pXRefsArray)
				delete m_pXRefsArray;
		
			m_pXRefsArray = new CXMLXRefInfoArray;
			if (!m_pXRefsArray->Parse(pDocInfoNode, (LPCTSTR)m_strTableName))
			{
				delete m_pXRefsArray;
				m_pXRefsArray = NULL;
				return FALSE;
			}		
		}

		LoadAppendedExternalReferences(pDBTInfoNode);
	}

	return TRUE;
}


//---------------------------------------------------------------------------------------------
BOOL CXMLDBTInfo::LoadAppendedExternalReferences(CXMLNode* pnDbtNode)
{
	if(!pnDbtNode)
		return FALSE;
	
	//nodo dell'array delle uk
	CXMLNode* pnExtRef = pnDbtNode->GetChildByName(XML_APPENDED_EXTERNAL_REFERENCES_TAG);
	if (pnExtRef)
	{
		if(m_pXRefsToAppendArray)
			delete m_pXRefsToAppendArray;		
		m_pXRefsToAppendArray = new CXMLXReferencesToAppendArray();		
		if (m_pXRefsToAppendArray->Parse(pnDbtNode, m_strTableName))
		{
			for(int i = 0; i < m_pXRefsToAppendArray->GetSize(); i++)
			{
				CXMLXReferencesToAppend* xRefs = m_pXRefsToAppendArray->GetAt(i);
				for (int j = 0; j < xRefs->m_pXRefsToAppendArray->GetSize(); j++)
				{
					CXMLXRefInfo* xRefInfo= xRefs->m_pXRefsToAppendArray->GetAt(j);
					xRefInfo->SetAppended(TRUE);
					m_pXRefsArray->Add(xRefInfo);
				}
			}
		}
	}

	return TRUE;
}

//----------------------------------------------------------------------------------------------
BOOL CXMLDBTInfo::LoadUniversalKeysInfo(CXMLNode* pnDbtMaster)
{
	if(!pnDbtMaster)
		return FALSE;
	
	//nodo dell'array delle uk
	CXMLNode* pnUKs = pnDbtMaster->GetChildByName(XML_UNIVERSAL_KEYS_TAG);
	if(!pnUKs)
		return TRUE;
	
	if(m_pXMLUniversalKeyGroup)
		delete m_pXMLUniversalKeyGroup;

	m_pXMLUniversalKeyGroup = new CXMLUniversalKeyGroup();

	//demando il parsing all'array di uk
	return m_pXMLUniversalKeyGroup->Parse(pnUKs);
}

//----------------------------------------------------------------------------------------------
BOOL CXMLDBTInfo::SaveUniversalKeysInfo(CXMLNode* pnDbtMaster)
{
	if(!pnDbtMaster)
		return FALSE;
	
	if(!m_pXMLUniversalKeyGroup)
		return TRUE;

	CXMLNode* pnUniversalKeys = pnDbtMaster->CreateNewChild(XML_UNIVERSAL_KEYS_TAG);
	
	//demando il parsing all'array di uk
	return m_pXMLUniversalKeyGroup->UnParse(pnUniversalKeys);
}

//----------------------------------------------------------------------------------------------
BOOL CXMLDBTInfo::LoadFixedKeysInfo(CXMLNode* pnDbtMaster)
{
	if(!pnDbtMaster)
		return FALSE;
	
	//nodo dell'array delle fk
	CXMLNode* pnFKs = pnDbtMaster->GetChildByName(XML_FIXED_KEYS_TAG);
	if(!pnFKs)
		return TRUE;
	
	if(m_pXMLFixedKeyArray)
		delete m_pXMLFixedKeyArray;

	m_pXMLFixedKeyArray = new CXMLFixedKeyArray();

	//demando il parsing all'array delle fixed keys
	return m_pXMLFixedKeyArray->Parse(pnFKs);
}

//----------------------------------------------------------------------------------------------
BOOL CXMLDBTInfo::SaveFixedKeysInfo(CXMLNode* pnDbtMaster)
{
	if(!pnDbtMaster)
		return FALSE;
	
	if(!m_pXMLFixedKeyArray)
		return TRUE;

	CXMLNode* pnFixedKeys = pnDbtMaster->CreateNewChild(XML_FIXED_KEYS_TAG);
	
	//demando il parsing all'array di fk
	return m_pXMLFixedKeyArray->UnParse(pnFixedKeys);
}

//----------------------------------------------------------------------------------------------
CXMLHotKeyLink* CXMLDBTInfo::GetDBTXMLHotKeyLinkInfo()
{
	if (!m_pXMLHotKeyLinkArray)
		return NULL;
	return m_pXMLHotKeyLinkArray->GetHKLByFieldName(GetNamespace().GetObjectNameForTag(), CXMLHotKeyLink::DBT);
}

//----------------------------------------------------------------------------------------------
BOOL CXMLDBTInfo::LoadDBTInfo
						(
							CLocalizableXMLDocument* pXMLDBTDoc,
							CLocalizableXMLDocument* pXMLXRefDoc,
							LPCTSTR				lpszNamespace,
							LPCTSTR				lpszTagName
						)
{	
	if (!pXMLDBTDoc || lpszNamespace) return FALSE;

	CXMLNode* pRoot = pXMLDBTDoc->GetRoot();	
	// controllo se è effettivamente il master
	CXMLNode* pDBTTagNode = pRoot->GetChildByName(XML_DBT_TYPE_MASTER_TAG);

	CXMLNode* pDBTNode = pDBTTagNode->GetChildByTagValue(lpszTagName, lpszNamespace, FALSE);
	
	return pDBTNode && SetDBTInfo(pDBTNode) && LoadExternalReference(pXMLXRefDoc);
}


//----------------------------------------------------------------------------------------------
BOOL CXMLDBTInfo::LoadMasterDBTInfo
					(
						CLocalizableXMLDocument* pXMLDBTDoc, 
						CLocalizableXMLDocument* pXMLXRefDoc,
						LPCTSTR				lpszNamespace

					)
{	
	if (!pXMLDBTDoc || !lpszNamespace) return FALSE;

	CXMLNode* pRoot = pXMLDBTDoc->GetRoot();	

	// controllo se è effettivamente il master
	CXMLNode* pMasterNode = pRoot->GetChildByTagValue(XML_DBT_TYPE_MASTER_TAG, lpszNamespace, FALSE);

	return 
		(
			pMasterNode &&
			SetDBTInfo(pMasterNode) &&
			LoadExternalReference(pXMLXRefDoc)
		);	
}

//----------------------------------------------------------------------------------------------
BOOL CXMLDBTInfo::LoadFields(CXMLNode* pnDbtNode)
{
	if(!pnDbtNode)
		return FALSE;
	
	CXMLNode* pnFields = pnDbtNode->GetChildByName(XML_FIELD_FIELDS_TAG);
	if(!pnFields)
		return FALSE;
	
	if(!m_pXMLFieldInfoArray)
		m_pXMLFieldInfoArray = new CXMLFieldInfoArray;

	return m_pXMLFieldInfoArray->Parse(pnFields);
}

//----------------------------------------------------------------------------------------------
BOOL CXMLDBTInfo::SaveFields(CXMLNode* pnDBTNode)
{
	if(!pnDBTNode || pnDBTNode->GetChildsNum() > 0)
		return FALSE;
	
	CXMLNode* pnFields = pnDBTNode->CreateNewChild(XML_FIELD_FIELDS_TAG);
	if(!pnFields)
		return FALSE;
	
	CXMLFieldInfoArray* pXMLFieldInfoArray = GetXMLFieldInfoArray();
	if(!pXMLFieldInfoArray)
		return TRUE;

	return pXMLFieldInfoArray->UnParse(pnFields);
}

//----------------------------------------------------------------------------------------------
BOOL CXMLDBTInfo::HasFieldsToExport ()
{
	CXMLFieldInfoArray* pXMLFieldInfoArray = GetXMLFieldInfoArray();

	return pXMLFieldInfoArray && pXMLFieldInfoArray->HasFieldsToExport ();
}

//----------------------------------------------------------------------------------------------
BOOL CXMLDBTInfo::LoadHotKeyLinks(CXMLNode* pnDbt)
{
	if(!pnDbt)
		return FALSE;
	
	//nodo dell'array delle fk
	CXMLNode* pnHKLs = pnDbt->GetChildByName(XML_HKLS_TAG);
	if(!pnHKLs)
		return TRUE;
	
	if(m_pXMLHotKeyLinkArray)
		delete m_pXMLHotKeyLinkArray;

	m_pXMLHotKeyLinkArray = new CXMLHotKeyLinkArray();

	//demando il parsing all'array degli hkl
	return m_pXMLHotKeyLinkArray->Parse(pnHKLs);
}

//----------------------------------------------------------------------------------------------
BOOL CXMLDBTInfo::SaveHotKeyLinks(CXMLNode* pnDbt)
{
	if(!pnDbt)
		return FALSE;
	
	if(!m_pXMLHotKeyLinkArray)
		return TRUE;

	CXMLNode* pnHKLs = pnDbt->CreateNewChild(XML_HKLS_TAG);
	
	//demando il parsing all'array di htkl
	return m_pXMLHotKeyLinkArray->UnParse(pnHKLs);
}

//----------------------------------------------------------------------------------------------
BOOL CXMLDBTInfo::LoadBusinessConstraints(CXMLNode* pnDbt)
{
	if(!pnDbt)
		return FALSE;
	
	//nodo dell'array dei constraints
	CXMLNode* pnBookmarks = pnDbt->GetChildByName(XML_SEARCH_BOOKMARKS_TAG);
	if(!pnBookmarks)
		return TRUE;
	
	if(m_pXMLSearchBookmarkArray)
		delete m_pXMLSearchBookmarkArray;

	m_pXMLSearchBookmarkArray = new CXMLSearchBookmarkArray();

	//demando il parsing all'array dei constraints
	return m_pXMLSearchBookmarkArray->Parse(pnBookmarks);
}

//----------------------------------------------------------------------------------------------
BOOL CXMLDBTInfo::SaveBusinessConstraints(CXMLNode* pnDbt)
{
	if(!pnDbt)
		return FALSE;
	
	if(!m_pXMLSearchBookmarkArray)
		return TRUE;

	CXMLNode* pnBookmarks = pnDbt->CreateNewChild(XML_SEARCH_BOOKMARKS_TAG);
	
	//demando il parsing all'array dei constraints
	return m_pXMLSearchBookmarkArray->UnParse(pnBookmarks);
}


//----------------------------------------------------------------------------------------------
BOOL CXMLDBTInfo::UnParse(CXMLNode* pnDbtNode, CXMLNode* pnXRefDbtNode)
{
	if (!pnDbtNode) return FALSE;

	pnDbtNode->SetAttribute(XML_NAMESPACE_ATTRIBUTE, m_nsDBT.ToUnparsedString());
	
	CXMLNode* pChildNode = pnDbtNode->CreateNewChild(XML_TITLE_TAG);


	pChildNode->SetText(m_strTitle);
	((CLocalizableXMLNode*)pChildNode)->SetLocalizableText(m_strOriginalTitle);	
	
	pChildNode = pnDbtNode->CreateNewChild(XML_TABLE_TAG);
	pChildNode->SetText(m_strTableName);
	pChildNode->SetAttribute(XML_NAMESPACE_ATTRIBUTE, m_nsTable.ToUnparsedString());
	
	pChildNode = pnDbtNode->CreateNewChild(XML_CHOOSEUPDATE_TAG);
	pChildNode->SetText(FormatBoolForXML(m_bChooseUpdate));	
	
	if (m_pXRefsArray)
		m_pXRefsArray->UnParse(pnXRefDbtNode, this);

	if (m_pXRefsToAppendArray)
		m_pXRefsToAppendArray->Unparse(pnXRefDbtNode, m_strTableName);
	
	return TRUE;
}

//----------------------------------------------------------------------------------------------
BOOL CXMLDBTInfo::IsXRefPresent	(const CString& strNewXRefName)
{
	if (!m_pXRefsArray)
		return FALSE;

	for (int nIdx = 0 ; nIdx < m_pXRefsArray->GetSize() ; nIdx++)
	{
		if (m_pXRefsArray->GetAt(nIdx)->m_strName == strNewXRefName)
			return TRUE;
	}

	return FALSE;
}

//----------------------------------------------------------------------------------------------
BOOL CXMLDBTInfo::IsXRefPresent	(CXMLXRefInfo* pXMLXRefInfo)
{
	if(!pXMLXRefInfo)
		return FALSE;

	if (!m_pXRefsArray)
		return FALSE;

	for (int nIdx = 0 ; nIdx < m_pXRefsArray->GetSize() ; nIdx++)
	{
		if (m_pXRefsArray->GetAt(nIdx) == pXMLXRefInfo)
			return TRUE;
	}

	return FALSE;
}

//----------------------------------------------------------------------------------------------
void CXMLDBTInfo::SetXMLXRefInfoArray(const	CXMLXRefInfoArray* pXRefsArray)
{
	if (pXRefsArray)
	{
		if (m_pXRefsArray)
			*m_pXRefsArray = *pXRefsArray;
		else
			m_pXRefsArray = new CXMLXRefInfoArray(*pXRefsArray);
	}
	else
	{
		if (m_pXRefsArray)
			delete m_pXRefsArray;
		m_pXRefsArray = NULL;
	}
}


//----------------------------------------------------------------------------------------------
void CXMLDBTInfo::SetXMLXRefInfoToAppendArray(const	CXMLDBTInfo& aDBTInfo)
{
	if (this == &aDBTInfo)
		return;
	
	SetXMLXRefInfoToAppendArray(aDBTInfo.m_pXRefsToAppendArray);
}

//----------------------------------------------------------------------------------------------
void CXMLDBTInfo::SetXMLXRefInfoToAppendArray(const	CXMLXReferencesToAppendArray* pXRefsToAppendArray)
{
	if (pXRefsToAppendArray)
	{
		if (m_pXRefsToAppendArray)
			*m_pXRefsToAppendArray = *pXRefsToAppendArray;
		else
			m_pXRefsToAppendArray = new CXMLXReferencesToAppendArray(*pXRefsToAppendArray);
	}
	else
	{
		if (m_pXRefsToAppendArray)
			delete m_pXRefsToAppendArray;
		m_pXRefsToAppendArray = NULL;
	}
}

//----------------------------------------------------------------------------------------------
void CXMLDBTInfo::AddXReferencesToAppend(CXMLXReferencesToAppend* pXRefsToAppend)
{
	if (!m_pXRefsToAppendArray)
		m_pXRefsToAppendArray = new CXMLXReferencesToAppendArray();

	//prima verifico che non sia già presente 
	m_pXRefsToAppendArray->Add(pXRefsToAppend);
}

//----------------------------------------------------------------------------------------------
void CXMLDBTInfo::SetXMLXRefInfoArray(const	CXMLDBTInfo& aDBTInfo)
{
	if (this == &aDBTInfo)
		return;
	
	SetXMLXRefInfoArray(aDBTInfo.m_pXRefsArray);
}

//----------------------------------------------------------------------------------------------
void CXMLDBTInfo::SetXMLFieldInfoArray(const CXMLFieldInfoArray* pXMLFieldInfoArray)
{
	if (pXMLFieldInfoArray)
	{
		if (m_pXMLFieldInfoArray)
			*m_pXMLFieldInfoArray = *pXMLFieldInfoArray;
		else
			m_pXMLFieldInfoArray = new CXMLFieldInfoArray(*pXMLFieldInfoArray);
	}
	else
	{
		if (m_pXMLFieldInfoArray)
			delete m_pXMLFieldInfoArray;
		m_pXMLFieldInfoArray = NULL;
	}
}


//----------------------------------------------------------------------------------------------
void CXMLDBTInfo::SetXMLFieldInfoArray(const	CXMLDBTInfo& aDBTInfo)
{
	if (this == &aDBTInfo)
		return;
	
	SetXMLFieldInfoArray(aDBTInfo.m_pXMLFieldInfoArray);
}

//----------------------------------------------------------------------------------------------
void CXMLDBTInfo::SetXMLUniversalKeyGroup(const CXMLUniversalKeyGroup* pXMLUniversalKeyGroup)
{
	if (pXMLUniversalKeyGroup)
	{
		if (m_pXMLUniversalKeyGroup)
			*m_pXMLUniversalKeyGroup = *pXMLUniversalKeyGroup;
		else
			m_pXMLUniversalKeyGroup = new CXMLUniversalKeyGroup(*pXMLUniversalKeyGroup);
	}
	else
	{
		if (m_pXMLUniversalKeyGroup)
			delete m_pXMLUniversalKeyGroup;
		m_pXMLUniversalKeyGroup = NULL;
	}
}

//----------------------------------------------------------------------------------------------
void CXMLDBTInfo::SetXMLUniversalKeyGroup(const	CXMLDBTInfo& aDBTInfo)
{
	if (this == &aDBTInfo)
		return;
	
	SetXMLUniversalKeyGroup(aDBTInfo.m_pXMLUniversalKeyGroup);
}
//----------------------------------------------------------------------------------------------
void CXMLDBTInfo::SetXMLFixedKeyArray(const	CXMLDBTInfo& aDBTInfo)
{
	if (this == &aDBTInfo)
		return;
	
	SetXMLFixedKeyArray(aDBTInfo.m_pXMLFixedKeyArray);
}


//----------------------------------------------------------------------------------------------
void CXMLDBTInfo::SetXMLFixedKeyArray(const CXMLFixedKeyArray* pXMLFixedKeyArray)
{
	if (pXMLFixedKeyArray)
	{
		if (m_pXMLFixedKeyArray)
			*m_pXMLFixedKeyArray = *pXMLFixedKeyArray;
		else
			m_pXMLFixedKeyArray = new CXMLFixedKeyArray(*pXMLFixedKeyArray);
	}
	else
	{
		if (m_pXMLFixedKeyArray)
			delete m_pXMLFixedKeyArray;
		m_pXMLFixedKeyArray = NULL;
	}
}

//----------------------------------------------------------------------------------------------
void CXMLDBTInfo::SetXMLHotKeyLinkArray(const CXMLDBTInfo& aDBTInfo)
{
	if (this == &aDBTInfo)
		return;
	
	SetXMLHotKeyLinkArray(aDBTInfo.m_pXMLHotKeyLinkArray);
}


//----------------------------------------------------------------------------------------------
void CXMLDBTInfo::SetXMLHotKeyLinkArray(const CXMLHotKeyLinkArray* pXMLHKLArray)
{
	if (m_pXMLHotKeyLinkArray)
	{
		if (m_pXMLHotKeyLinkArray)
			*m_pXMLHotKeyLinkArray = *pXMLHKLArray;
		else
			m_pXMLHotKeyLinkArray = new CXMLHotKeyLinkArray(*pXMLHKLArray);
	}
	else
	{
		if (m_pXMLHotKeyLinkArray)
			delete m_pXMLHotKeyLinkArray;
		m_pXMLHotKeyLinkArray = NULL;
	}
}

//----------------------------------------------------------------------------------------------
void CXMLDBTInfo::SetXMLSearchBookmarkArray(const CXMLDBTInfo& aDBTInfo)
{
	if (this == &aDBTInfo)
		return;
	
	SetXMLSearchBookmarkArray(aDBTInfo.m_pXMLSearchBookmarkArray);
}


//----------------------------------------------------------------------------------------------
void CXMLDBTInfo::SetXMLSearchBookmarkArray(const CXMLSearchBookmarkArray* pSearchBookmarkArray)
{
	if (m_pXMLSearchBookmarkArray)
	{
		if (m_pXMLSearchBookmarkArray)
			*m_pXMLSearchBookmarkArray = *pSearchBookmarkArray;
		else
			m_pXMLSearchBookmarkArray = new CXMLSearchBookmarkArray(*pSearchBookmarkArray);
	}
	else
	{
		if (m_pXMLSearchBookmarkArray)
			delete m_pXMLSearchBookmarkArray;
		m_pXMLSearchBookmarkArray = NULL;
	}
}



//----------------------------------------------------------------------------------------------
//CXMLDBTInfoArray
//----------------------------------------------------------------------------------------------
CXMLDBTInfoArray::CXMLDBTInfoArray()
:
	m_bExistChooseUpdate(FALSE)
{

}

//----------------------------------------------------------------------------------------------
CXMLDBTInfoArray::CXMLDBTInfoArray(const CXMLDBTInfoArray& aDbtArray)
{
	RemoveAll();

	for(int i = 0 ; i < aDbtArray.GetSize() ; i++)
	{
		if(aDbtArray.GetAt(i) && !aDbtArray.GetAt(i)->IsFromClientDoc())
			Add(new CXMLDBTInfo(*aDbtArray.GetAt(i)));
	}
}

//----------------------------------------------------------------------------------------------
CXMLDBTInfo* CXMLDBTInfoArray::GetDBTByXRef(CXMLXRefInfo* pXMLXRefInfo)
{
	if(!pXMLXRefInfo)
		return NULL;
	
	for(int i = 0 ; i < GetSize() ; i++)
	{
		CXMLDBTInfo* pXMLDBTInfo = GetAt(i);
		if(pXMLDBTInfo && pXMLDBTInfo->IsXRefPresent(pXMLXRefInfo))
			return pXMLDBTInfo;
	}

	return NULL;
}

//----------------------------------------------------------------------------------------------
CXMLSearchBookmarkArray* CXMLDBTInfoArray::GetXMLSearchBookmark(const CTBNamespace& dbtNamespace)
{ 
	CXMLDBTInfo* pXMLDBTInfo = GetDBTByNamespace(dbtNamespace);
	return (pXMLDBTInfo) ? pXMLDBTInfo->GetXMLSearchBookmarkArray() : NULL; 
}


//----------------------------------------------------------------------------------------------
BOOL CXMLDBTInfoArray::IsEqual(const CXMLDBTInfoArray& aDbtArray) const
{
	if (this == &aDbtArray)
		return TRUE;
	
	if(aDbtArray.GetSize() != GetSize())
		return FALSE;
	
	for(int i = 0 ; i < GetSize() ; i++)
	{
		if(*GetAt(i) != *aDbtArray.GetAt(i))
			return FALSE;
	}

	return TRUE;
}

//----------------------------------------------------------------------------------------------
CXMLDBTInfoArray& CXMLDBTInfoArray::operator =(const CXMLDBTInfoArray& aDbtArray)
{
	if (this == &aDbtArray)
		return *this;
	
	RemoveAll();

	for(int i = 0 ; i < aDbtArray.GetSize() ; i++)
	{
		if(aDbtArray.GetAt(i) && !aDbtArray.GetAt(i)->IsFromClientDoc())
			Add(new CXMLDBTInfo(*aDbtArray.GetAt(i)));
	}
	return *this;
}

//----------------------------------------------------------------------------------------------
CXMLDBTInfo* CXMLDBTInfoArray::GetDBTByName(const CString& strName)
{
	CXMLDBTInfo* pXMLDBTInfo = NULL; 
	for(int i = 0 ; i < GetSize() ; i++)
	{
		pXMLDBTInfo = GetAt(i);
		if(!pXMLDBTInfo)
			continue;

		if(pXMLDBTInfo->GetNamespace().GetObjectName() == strName)
			return pXMLDBTInfo;
	}

	return NULL;
}

//----------------------------------------------------------------------------------------------
CXMLDBTInfo* CXMLDBTInfoArray::GetDBTByNamespace(const CString& strNamespace)
{
	CXMLDBTInfo* pXMLDBTInfo = NULL; 
	for(int i = 0 ; i < GetSize() ; i++)
	{
		pXMLDBTInfo = GetAt(i);
		if(!pXMLDBTInfo)
			continue;

		if(pXMLDBTInfo->GetNamespace().ToString() == strNamespace)
			return pXMLDBTInfo;
	}

	return NULL;
}

//----------------------------------------------------------------------------------------------
CXMLDBTInfo* CXMLDBTInfoArray::GetDBTByNamespace(const CTBNamespace& aNamespace)
{
	CXMLDBTInfo* pXMLDBTInfo = NULL; 
	for(int i = 0 ; i < GetSize() ; i++)
	{
		pXMLDBTInfo = GetAt(i);
		if(!pXMLDBTInfo)
			continue;

		if(pXMLDBTInfo->GetNamespace() == aNamespace)
			return pXMLDBTInfo;
	}

	return NULL;
}

//----------------------------------------------------------------------------------------------
CXMLDBTInfo* CXMLDBTInfoArray::GetDBTMaster()
{
	CXMLDBTInfo* pXMLDBTInfo = NULL; 
	for(int i = 0 ; i < GetSize() ; i++)
	{
		pXMLDBTInfo = GetAt(i);
		if(!pXMLDBTInfo)
			continue;

		if(pXMLDBTInfo->GetType() == CXMLDBTInfo::MASTER_TYPE)
			return pXMLDBTInfo;
	}

	return NULL;
}

//----------------------------------------------------------------------------------------------
BOOL CXMLDBTInfoArray::Parse(CLocalizableXMLDocument* pXMLDBTDoc,  CLocalizableXMLDocument* pXMLXRefDoc /*=NULL*/) 
{
	CXMLNode* pDBTMasterNode = pXMLDBTDoc->GetRootChildByName(XML_DBT_TYPE_MASTER_TAG);

	if (!ParseMaster(pDBTMasterNode, pXMLXRefDoc))
		return FALSE;
	
	CXMLNode* pDBTSlavesNode = pDBTMasterNode->GetChildByName(XML_SLAVES_TAG);

	return pDBTSlavesNode ? ParseSlaves(pDBTSlavesNode, pXMLXRefDoc) : TRUE;
}

//----------------------------------------------------------------------------------------------
BOOL CXMLDBTInfoArray::ParseMaster(CXMLNode* pDBTMasterNode, CLocalizableXMLDocument* pXMLXRefDoc /*=NULL*/) 
{
	if (!pDBTMasterNode)
		return FALSE;
	
	CXMLDBTInfo* pDBTMasterInfo = new CXMLDBTInfo;

	CXMLDocumentObject* pXmlDoc = pDBTMasterNode->GetXMLDocument();
	if (pXmlDoc)
	{
		BOOL bIsStandard = AfxGetPathFinder()->IsStandardPath(pXmlDoc->GetFileName());
		pDBTMasterInfo->SetFrom(bIsStandard ? CXMLDBTInfo::STANDARD : CXMLDBTInfo::CUSTOM);
	}

	CString strTagValue;

	if (
			pDBTMasterInfo->SetDBTInfo(pDBTMasterNode) && 
			pDBTMasterInfo->LoadExternalReference(pXMLXRefDoc) && 
			pDBTMasterInfo->LoadUniversalKeysInfo(pDBTMasterNode) &&
			pDBTMasterInfo->LoadFixedKeysInfo(pDBTMasterNode) &&
			pDBTMasterInfo->LoadBusinessConstraints(pDBTMasterNode)
		)
		Add(pDBTMasterInfo);
	else
	{
		ASSERT(FALSE);
		if (pDBTMasterInfo)
			delete pDBTMasterInfo;
		return FALSE;
	}
	return TRUE;
}

//----------------------------------------------------------------------------------------------
BOOL CXMLDBTInfoArray::ParseSlaves(CXMLNode* pDBTSlavesNode, CLocalizableXMLDocument* pXMLXRefDoc /*=NULL*/) 
{
	if (!pDBTSlavesNode)
		return FALSE;

	if(!pDBTSlavesNode->GetChildsNum())
		return TRUE;
	
	BOOL bOk = TRUE;
	CString strTagValue;
	//carico le info di ciascun slave
	CXMLNode* pSlaveNode = pDBTSlavesNode->GetFirstChild();
	if(pSlaveNode)
	{
		pSlaveNode->GetName(strTagValue);
		if(strTagValue == "#comment")
			pSlaveNode = pDBTSlavesNode->GetNextChild();
	}

	CXMLDocumentObject* pXmlDoc = pDBTSlavesNode->GetXMLDocument();
	BOOL bIsStandard = FALSE;
	if (pXmlDoc)
		bIsStandard = AfxGetPathFinder()->IsStandardPath(pXmlDoc->GetFileName());

	while (pSlaveNode)
	{
		CXMLDBTInfo* pSlaveDBTInfo = new CXMLDBTInfo;
		pSlaveDBTInfo->SetFrom(bIsStandard ? CXMLDBTInfo::STANDARD : CXMLDBTInfo::CUSTOM);

		if (pSlaveDBTInfo->SetDBTInfo(pSlaveNode) && pSlaveDBTInfo->LoadExternalReference(pXMLXRefDoc))
		{
			Add(pSlaveDBTInfo);
			m_bExistChooseUpdate = pSlaveDBTInfo->m_bChooseUpdate || m_bExistChooseUpdate;
			
		}
		else
		{
			if (pSlaveDBTInfo)
				delete pSlaveDBTInfo;
			bOk = FALSE;
		}
		pSlaveNode = pDBTSlavesNode->GetNextChild();
		if(pSlaveNode)
		{
			pSlaveNode->GetName(strTagValue);
			if(strTagValue == "#comment")
				pSlaveNode = pDBTSlavesNode->GetNextChild();
		}
	}

	return bOk;
}


//----------------------------------------------------------------------------------------------
BOOL CXMLDBTInfoArray::UnParse(const CString& strDbtFileName, const CString& strXRefFileName)
{
	ASSERT(GetSize());

	//il primo elemento è il master
	CXMLDBTInfo* pDbt = GetAt(0);
	
	CLocalizableXMLDocument aXMLDBTDoc(pDbt->GetNamespace(), AfxGetPathFinder());
	CLocalizableXMLDocument aXMLXRefDoc(pDbt->GetNamespace(), AfxGetPathFinder());

	CXMLNode* pnRootXRefs = aXMLXRefDoc.CreateRoot(XML_MAIN_EXTERNAL_REFERENCES_TAG);
	if(!pnRootXRefs)
		return FALSE;
	
	//scorre la struttura e crea i nodi dom mano a mano che trova nodi in struttura
	CXMLNode* pnRoot = aXMLDBTDoc.CreateRoot(XML_DBTS_TAG);
	if(!pnRoot)
		return FALSE;
	
	CXMLNode* pnMaster = pnRoot->CreateNewChild(XML_DBT_TYPE_MASTER_TAG);

	CXMLNode* pnXRefDBTNode	 = pnRootXRefs->CreateNewChild(XML_DBT_TAG);

	pDbt->UnParse(pnMaster, pnXRefDBTNode);
	pDbt->SaveUniversalKeysInfo(pnMaster);
	pDbt->SaveFixedKeysInfo(pnMaster);
	pDbt->SaveBusinessConstraints(pnMaster);

	BOOL bIsSavingInCustom = AfxGetPathFinder()->IsCustomPath(strDbtFileName);
	CXMLNode* pnSlaves = pnMaster->CreateNewChild(XML_DBT_TYPE_SLAVES_TAG);
	int nDBT = GetSize();
	CXMLNode* pnSlave = NULL;
	for ( int i = 1 ; i < nDBT ; i++)
	{
		pDbt = GetAt(i);	
		if	(
				pDbt && 
				(
					(bIsSavingInCustom && pDbt->IsFrom() == CXMLDBTInfo::CUSTOM) ||
					(!bIsSavingInCustom && pDbt->IsFrom() == CXMLDBTInfo::STANDARD)
				)
			)
		{
			pnSlave = pnSlaves->CreateNewChild(pDbt->GetStrType());
		
			pnXRefDBTNode	 = pnRootXRefs->CreateNewChild(XML_DBT_TAG);
			pDbt->UnParse(pnSlave, pnXRefDBTNode);	
		}
	}

	return aXMLDBTDoc.SaveXMLFile(strDbtFileName, TRUE) && aXMLXRefDoc.SaveXMLFile(strXRefFileName, TRUE);
}

//----------------------------------------------------------------------------------------------
BOOL CXMLDBTInfoArray::SaveFieldInfoFile(const CString& strFieldFileName)
{
	ASSERT(GetSize());
	CXMLDBTInfo* pDbt = GetAt(0);

	if (ExistFile(strFieldFileName))
		DeleteFile(strFieldFileName);

	CLocalizableXMLDocument aXMLFieldDoc(pDbt->m_nsDBT, AfxGetPathFinder());
	
	CXMLNode* pnRoot = aXMLFieldDoc.CreateRoot(XML_FIELD_ROOT_TAG);
	if(!pnRoot)
		return FALSE;

	for ( int i = 0 ; i < GetSize() ; i++)
	{
		pDbt = GetAt(i);
		if (pDbt && pDbt->HasFieldsToExport())
		{
			CXMLNode* pnDBT = pnRoot->CreateNewChild(XML_FIELD_DBT_TAG);

			pnDBT->SetAttribute(XML_NAMESPACE_ATTRIBUTE, pDbt->GetNamespace().ToUnparsedString());
			pDbt->SaveFields(pnDBT);	
		}
	}

	 return !pnRoot->HasChildNodes()|| aXMLFieldDoc.SaveXMLFile(strFieldFileName, TRUE);
}

//----------------------------------------------------------------------------------------------
BOOL CXMLDBTInfoArray::LoadFieldInfoFile(const CString& strFieldFileName)
{
	ASSERT(GetSize());
	CXMLDBTInfo* pDbt = GetAt(0);
	CLocalizableXMLDocument aXMLFieldDoc(pDbt->m_nsDBT, AfxGetPathFinder());
	if (!aXMLFieldDoc.LoadXMLFile(strFieldFileName))
		return FALSE;

	CXMLNode* pnRooth = aXMLFieldDoc.GetRoot();
	if (!pnRooth)
		return FALSE;
	
	//carico le info di ciascun slave
	for(int i = 0 ; i < pnRooth->GetChildsNum() ; i++)
	{
		CXMLNode* pnDBTNode = pnRooth->GetChildAt(i);
		if(!pnDBTNode )
			continue;

		CString strTagValue;
		pnDBTNode->GetName(strTagValue);
		if(strTagValue == "#comment") continue;

		CString strDBTNamespace;
		pnDBTNode->GetAttribute(XML_NAMESPACE_ATTRIBUTE, strDBTNamespace);
		CTBNamespace aNs;
		aNs.AutoCompleteNamespace(CTBNamespace::DBT, strDBTNamespace, aNs);
		CXMLDBTInfo* pXMLDBTInfo = GetDBTByNamespace(aNs);

		if(pXMLDBTInfo)
			pXMLDBTInfo->LoadFields(pnDBTNode);
	}

	return TRUE;
}

//----------------------------------------------------------------------------------------------
BOOL CXMLDBTInfoArray::SaveHotKeyLinkInfoFile(const CString& strFieldFileName)
{
	ASSERT(GetSize());
	CXMLDBTInfo* pDbt = GetAt(0);

	CLocalizableXMLDocument aXMLHKLDoc(pDbt->m_nsDBT, AfxGetPathFinder());
	
	CXMLNode* pnRootHKLs = aXMLHKLDoc.CreateRoot(XML_HKL_ROOT_TAG);
	if(!pnRootHKLs)
		return FALSE;

	BOOL bSaveFile = FALSE;
	for ( int i = 0 ; i < GetSize() ; i++)
	{
		pDbt = GetAt(i);
		if(!pDbt)
			return FALSE;

		if(pDbt->GetXMLHotKeyLinkArray())
		{
			bSaveFile = TRUE;
			CXMLNode* pnDBT = pnRootHKLs->CreateNewChild(XML_HKL_DBT_TAG);
			pnDBT->SetAttribute(XML_NAMESPACE_ATTRIBUTE, pDbt->GetNamespace().ToUnparsedString());
			pDbt->SaveHotKeyLinks(pnDBT);	
		}
	}

	if(bSaveFile)
		aXMLHKLDoc.SaveXMLFile(strFieldFileName, TRUE);
	
	return TRUE;
}

//----------------------------------------------------------------------------------------------
BOOL CXMLDBTInfoArray::LoadHotKeyLinkInfoFile(const CString& strFieldFileName)
{
	ASSERT(GetSize());
	CXMLDBTInfo* pDbt = GetAt(0);
	CLocalizableXMLDocument aXMLHKLDoc(pDbt->m_nsDBT, AfxGetPathFinder());
	if (!aXMLHKLDoc.LoadXMLFile(strFieldFileName))
		return FALSE;

	CXMLNode* pnRooth = aXMLHKLDoc.GetRoot();
	if (!pnRooth)
		return FALSE;
	
	//carico le info di ciascun slave
	for(int i = 0 ; i < pnRooth->GetChildsNum() ; i++)
	{
		CXMLNode* pnDBTNode = pnRooth->GetChildAt(i);
		if(!pnDBTNode )
			continue;

		CString strTagValue;
		pnDBTNode->GetName(strTagValue);
		if(strTagValue == "#comment") continue;

		CString strDBTNamespace;
		pnDBTNode->GetAttribute(XML_NAMESPACE_ATTRIBUTE, strDBTNamespace);
		CTBNamespace aNs;
		aNs.AutoCompleteNamespace(CTBNamespace::DBT, strDBTNamespace, aNs);
		CXMLDBTInfo* pXMLDBTInfo = GetDBTByNamespace(aNs);

		if(pXMLDBTInfo)
			pXMLDBTInfo->LoadHotKeyLinks(pnDBTNode);
	}

	return TRUE;
}

//----------------------------------------------------------------------------------------------
BOOL CXMLDBTInfoArray::RemoveXRef(CXMLXRefInfo* pXMLXRefInfo)
{
	if(!pXMLXRefInfo)
		return FALSE;

	for ( int i = 0 ; i < GetSize() ; i++)
	{
		CXMLDBTInfo* pDbt = GetAt(i);
		if(!pDbt)
			return FALSE;
		pDbt->RemoveXRef(*pXMLXRefInfo);
		return TRUE;
	}

	return TRUE;
}

//----------------------------------------------------------------------------------------------
BOOL CXMLDBTInfoArray::GetXRefArrayByFK(const CString& strSegmentFk, CXMLXRefInfoArray* pXMLXRefInfoArray, BOOL bUsedOnly /*= TRUE*/)
{
	if(strSegmentFk.IsEmpty() || !pXMLXRefInfoArray)
		return FALSE;
	
	if(pXMLXRefInfoArray->IsOwnsElements())
		pXMLXRefInfoArray->SetOwns(FALSE);

	//mi faccio dare tutti gli xref con la fk passata e li aggiungo all'array di ritorno
	for ( int i = 0 ; i < GetSize() ; i++)
	{
		CXMLDBTInfo* pDbt = GetAt(i);
		if(!pDbt || !pDbt->GetXMLXRefInfoArray())
			continue;
		
		pDbt->GetXMLXRefInfoArray()->GetXRefArrayByFK(strSegmentFk, pXMLXRefInfoArray, bUsedOnly);
	}

	return TRUE;
}


//----------------------------------------------------------------------------------------------
//	CXMLHeaderInfo implementation
//----------------------------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(CXMLHeaderInfo, CObject)
//----------------------------------------------------------------------------------------------
CXMLHeaderInfo::CXMLHeaderInfo()
{
	Clear();
}

//----------------------------------------------------------------------------------------------
CXMLHeaderInfo::CXMLHeaderInfo(const CTBNamespace& aNS)
{
	m_nsDoc = aNS;
	Clear();
	
	m_strEnvClass = GetDefaultEnvClass();;
}

//----------------------------------------------------------------------------------------------
CXMLHeaderInfo::CXMLHeaderInfo(const CXMLHeaderInfo& aHeaderInfo)
{
	*this = aHeaderInfo;
}

//----------------------------------------------------------------------------------------------
void CXMLHeaderInfo::Clear() 
{
	m_strVersion.Empty();
	m_nMaxDocument	= 0;	
	m_nMaxDimension	= HEADER_DEFAULT_DOC_DIMENSION;
	m_strUrlData.Empty();
	m_strEnvClass.Empty();
	m_strEnvClassExt.Empty();
	m_strEnvClassTitle.Empty();
	m_bPostable = TRUE;
	m_bPostBack = TRUE;
	m_bNoExtRefPostBack = FALSE;	
	m_bFullPrepare = TRUE; 
	m_bTransform = FALSE;
	m_strTransformXSLT.Empty();
	m_bOnlyBusinessObject = FALSE;
}

//----------------------------------------------------------------------------------------------
BOOL CXMLHeaderInfo::IsEqual(const CXMLHeaderInfo& aHead) const
{
	if (this == &aHead)
		return TRUE;
	
	return
		m_strVersion		== aHead.m_strVersion		&&
		m_nMaxDocument		== aHead.m_nMaxDocument		&&
		m_strUrlData		== aHead.m_strUrlData		&&
		m_strEnvClass		== aHead.m_strEnvClass		&&
		m_strEnvClassExt	== aHead.m_strEnvClassExt	&&
		m_bPostable			== aHead.m_bPostable		&&
		m_bPostBack			== aHead.m_bPostBack		&&
		m_bNoExtRefPostBack == aHead.m_bNoExtRefPostBack &&
		m_bFullPrepare		== aHead.m_bFullPrepare		&&
		m_nsDoc 			== aHead.m_nsDoc			&&
		m_bTransform		== aHead.m_bTransform		&&
		m_strTransformXSLT	== aHead.m_strTransformXSLT	&&
		m_bOnlyBusinessObject == aHead.m_bOnlyBusinessObject;
}


//----------------------------------------------------------------------------------------------
CString	CXMLHeaderInfo::GetStrMaxDoc()	const
{ 
	CString strMd;
	strMd.Format(_T("%d"), m_nMaxDocument);
	
	return strMd;	
}

//----------------------------------------------------------------------------
void CXMLHeaderInfo::SetMaxDimension(int nMaxDimension)
{
	if(nMaxDimension <= HEADER_MAX_DOC_DIMENSION || nMaxDimension >= HEADER_MIN_DOC_DIMENSION)
		m_nMaxDimension = nMaxDimension;
}

//----------------------------------------------------------------------------
void CXMLHeaderInfo::SetMaxDimension(const CString& strMaxDimension)
{
	
	int nMaxDimension = _tstoi((LPCTSTR)strMaxDimension);
	if(nMaxDimension <= HEADER_MAX_DOC_DIMENSION || nMaxDimension >= HEADER_MIN_DOC_DIMENSION)
		m_nMaxDimension = nMaxDimension;
}

//----------------------------------------------------------------------------
CString CXMLHeaderInfo::GetEnvClassWithExt()
{ 
	if (m_strEnvClass.IsEmpty())
		return _T("");

	return (m_strEnvClassExt.IsEmpty())
			? m_strEnvClass
			: m_strEnvClass + HEADER_ENV_CLASS_SEPARATOR + m_strEnvClassExt;
}

//----------------------------------------------------------------
BOOL CXMLHeaderInfo::Parse(CLocalizableXMLDocument* pXMLHdDoc) 
{
	if (!pXMLHdDoc) return FALSE;

	CXMLNode* pRoot = pXMLHdDoc->GetRoot();
	if (!pRoot) return FALSE;
	
	CXMLNode* pInfoNode;
	CString strTagValue;

	if (pInfoNode = pRoot->GetChildByName(XML_MAXDOC_TAG))
	{
		pInfoNode->GetText(strTagValue);
		SetMaxDoc(strTagValue);
	}

	if (pInfoNode = pRoot->GetChildByName(XML_MAX_DIMENSION_TAG))
	{
		pInfoNode->GetText(strTagValue);
		SetMaxDimension(strTagValue);
	}

	if (pInfoNode = pRoot->GetChildByName(XML_VERSION_TAG))
		pInfoNode->GetText(m_strVersion);

	if (pInfoNode = pRoot->GetChildByName(XML_DATA_URL_TAG))
		pInfoNode->GetText(m_strUrlData);

	CLocalizableXMLNode* pEnvNode = NULL;
	
	if (pEnvNode = (CLocalizableXMLNode*)pRoot->GetChildByName(XML_ENVELOPE_CLASS_TAG))
	{
		pEnvNode->GetText(m_strEnvClassTitle);		
		pEnvNode->GetLocalizableText(m_strEnvClass);	
		pEnvNode->GetAttribute(XML_ENVELOPE_CLASS_EXT_ATTRIBUTE, m_strEnvClassExt);
	}

	if (m_strEnvClass.IsEmpty())
		m_strEnvClassTitle = m_strEnvClass = GetDefaultEnvClass();

	CString strBool;
	if (pInfoNode = pRoot->GetChildByName(XML_POSTABLE_TAG))
	{
		pInfoNode->GetText(strBool);
		m_bPostable = GetBoolFromXML(strBool);
	}
	if (pInfoNode = pRoot->GetChildByName(XML_POSTBACK_TAG))
	{
		pInfoNode->GetText(strBool);
		m_bPostBack = GetBoolFromXML(strBool);
	}
	if (pInfoNode = pRoot->GetChildByName(XML_NOEXTREF_POSTBACK_TAG))
	{
		pInfoNode->GetText(strBool);
		m_bNoExtRefPostBack = GetBoolFromXML(strBool);
	}
	
	if (pInfoNode = pRoot->GetChildByName(XML_FULLPREPARE_TAG))
	{
		pInfoNode->GetText(strBool);
		SetFullPrepare(GetBoolFromXML(strBool));
	}

	if (pInfoNode = pRoot->GetChildByName(XML_TRANSFORM_XSLT_NAME_TAG))
	{
		m_bTransform = TRUE;
		pInfoNode->GetText(m_strTransformXSLT);	
	}

	
	if (pInfoNode = pRoot->GetChildByName(XML_ONLY_BO_TAG))
	{
		pInfoNode->GetText(strBool);
		m_bOnlyBusinessObject = GetBoolFromXML(strBool);
	}
	
	return TRUE;
}

//---------------------------------------------------------------------------------
BOOL CXMLHeaderInfo::UnParse(const CString& strHeaderFileName)
{
	CLocalizableXMLDocument aXMLHdrDoc(m_nsDoc, AfxGetPathFinder());

	CXMLNode* pnRoot = aXMLHdrDoc.CreateRoot(XML_DOCUMENT_TAG);

	if(!pnRoot)
		return FALSE;
	
	CXMLNode* pnChild = pnRoot->CreateNewChild(XML_VERSION_TAG);
	pnChild->SetText(m_strVersion);
		
	CString sMd;
	sMd.Format(_T("%d"), m_nMaxDocument);
	pnChild = pnRoot->CreateNewChild(XML_MAXDOC_TAG);
	pnChild->SetText(sMd);

	sMd.Format(_T("%d"), m_nMaxDimension);
	pnChild = pnRoot->CreateNewChild(XML_MAX_DIMENSION_TAG);
	pnChild->SetText(sMd);

	pnChild = pnRoot->CreateNewChild(XML_DATA_URL_TAG);
	pnChild->SetText(m_strUrlData);

	pnChild = pnRoot->CreateNewChild(XML_ENVELOPE_CLASS_TAG);
	((CLocalizableXMLNode*)pnChild)->SetLocalizableText(m_strEnvClass);	

	pnChild->SetAttribute(XML_ENVELOPE_CLASS_EXT_ATTRIBUTE, m_strEnvClassExt);

	pnChild = pnRoot->CreateNewChild(XML_POSTABLE_TAG);
	pnChild->SetText(FormatBoolForXML(m_bPostable));
	pnChild = pnRoot->CreateNewChild(XML_POSTBACK_TAG);
	pnChild->SetText(FormatBoolForXML(m_bPostBack));
	pnChild = pnRoot->CreateNewChild(XML_NOEXTREF_POSTBACK_TAG);
	pnChild->SetText(FormatBoolForXML(m_bNoExtRefPostBack));
	
	pnChild = pnRoot->CreateNewChild(XML_FULLPREPARE_TAG);
	pnChild->SetText(FormatBoolForXML(m_bFullPrepare));

	if (m_bTransform)
	{
		pnChild = pnRoot->CreateNewChild(XML_TRANSFORM_XSLT_NAME_TAG);
		pnChild->SetText(m_strTransformXSLT);
	}

	if (m_bOnlyBusinessObject)
	{
		pnChild = pnRoot->CreateNewChild(XML_ONLY_BO_TAG);
		pnChild->SetText(FormatBoolForXML(m_bOnlyBusinessObject));
	}


	return aXMLHdrDoc.SaveXMLFile(strHeaderFileName, TRUE);
}

//----------------------------------------------------------------------------------------------
CXMLHeaderInfo& CXMLHeaderInfo::operator =(const CXMLHeaderInfo& aHeader)
{
	if (this == &aHeader)
		return *this;
	
	m_strVersion			= aHeader.m_strVersion;
	m_nMaxDocument			= aHeader.m_nMaxDocument;
	m_nMaxDimension			= aHeader.m_nMaxDimension;
	m_strUrlData			= aHeader.m_strUrlData;
	m_strEnvClassTitle		= aHeader.m_strEnvClassTitle;
	m_strEnvClass			= aHeader.m_strEnvClass;
	m_nsDoc					= aHeader.m_nsDoc;
	m_bPostable				= aHeader.m_bPostable;
	m_bPostBack				= aHeader.m_bPostBack;
	m_bFullPrepare			= aHeader.m_bFullPrepare;
	m_bNoExtRefPostBack		= aHeader.m_bNoExtRefPostBack;
	m_bOnlyBusinessObject	= aHeader.m_bOnlyBusinessObject;

	//document transformation
	m_bTransform			= aHeader.m_bTransform;
	m_strTransformXSLT		= aHeader.m_strTransformXSLT;



	return *this;
}


//----------------------------------------------------------------------------------------------
//class CXMLClientDocInfo  implementatio
//----------------------------------------------------------------------------------------------
//
//----------------------------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(CXMLClientDocInfo, CObject)

//----------------------------------------------------------------------------------------------
CXMLClientDocInfo::CXMLClientDocInfo(const CTBNamespace& aClientNS, const CString& strName)
:
	m_nsClientDoc		(aClientNS),
	m_strClientDocName	(strName),
	m_pDBTArray			(NULL),
	m_pXMLClientDBTDoc	(NULL),
	m_pXMLClientXRefDoc	(NULL)
{
	SetAllFilesName();
}

//----------------------------------------------------------------------------------------------
CXMLClientDocInfo::CXMLClientDocInfo(const CXMLClientDocInfo& aClientDocInfo)
:
	m_pDBTArray			(NULL),
	m_pXMLClientDBTDoc	(NULL),
	m_pXMLClientXRefDoc	(NULL)
{
	Assign(aClientDocInfo);
}

//----------------------------------------------------------------------------------------------
CXMLClientDocInfo::~CXMLClientDocInfo()
{
	SAFE_DELETE(m_pDBTArray);
	SAFE_DELETE(m_pXMLClientDBTDoc);
	SAFE_DELETE(m_pXMLClientXRefDoc);
}

//----------------------------------------------------------------------------------------------
void CXMLClientDocInfo::Assign(const CXMLClientDocInfo& aClientDocInfo)
{
	m_nsClientDoc			= aClientDocInfo.m_nsClientDoc;
	m_strClientDocName		= aClientDocInfo.m_strClientDocName;
	m_strDBTFileName		= aClientDocInfo.m_strDBTFileName;
	m_strXRefFileName		= aClientDocInfo.m_strXRefFileName;
	m_strXRefDescriName		= aClientDocInfo.m_strXRefDescriName;


	if(aClientDocInfo.m_pDBTArray)
	{
		if (m_pDBTArray)
			m_pDBTArray->RemoveAll();
		else
		{
			m_pDBTArray = new CXMLDBTInfoArray;
			m_pDBTArray->SetOwns(FALSE);
		}

		for (int i = 0; i < aClientDocInfo.m_pDBTArray->GetSize(); i++)
			m_pDBTArray->Add(new CXMLDBTInfo(*(aClientDocInfo.m_pDBTArray->GetAt(i))));
	}
	else 
		SAFE_DELETE(m_pDBTArray);
}

//----------------------------------------------------------------------------------------------
void CXMLClientDocInfo::SetAllFilesName ()
{
	CString strClienDocPath = AfxGetPathFinder()->GetDocumentPath(m_nsClientDoc, CPathFinder::STANDARD);
	
	// se non ho la path del documento non devo fare niente
	// il clientdoc non è descritto 
	if (strClienDocPath.IsEmpty())
		return;

	//nome dei file
	m_strDBTFileName			= AfxGetPathFinder()->GetDocumentDbtsFullName(m_nsClientDoc, CPathFinder::STANDARD);
	m_strXRefFileName			= AfxGetPathFinder()->GetDocumentXRefFullName(m_nsClientDoc, CPathFinder::STANDARD);
	
	m_strXRefDescriName		= m_strXRefFileName;	
}

// è utilizzata dalla gestione dei profili
// attraverso la path di memorizzazione del profilo so se devo caricare le informazioni dalla custom (user\alluser)
// oppure dalla standard. Esempio di path con il clientdoc DocVenditaRiferimenti
// standard: C:\MicroareaServer\Development\Running\Standard\Applications\TestApplication\TaDataEntry\ModuleObjects\DocVenditaRiferimenti\ExportProfiles\DDT\ProfileName
// alluser:C:\MicroareaServer\Development\Running\Custom\Companies\MagoNet\Applications\TestApplication\TADataEntry\ModuleObjects\DocVenditaRiferimenti\ExportProfiles\AllUsers\DDT\ProfileName
// user: C:\MicroareaServer\Development\Running\Custom\Companies\MagoNet\Applications\TestApplication\TADataEntry\ModuleObjects\DocVenditaRiferimenti\ExportProfiles\Users\sa\DDT\ProfileName

//----------------------------------------------------------------------------------------------
void CXMLClientDocInfo::SetFilesFromPartialPath(const CString& strProfilePath, CPathFinder::PosType ePosType)
{
	if (strProfilePath.IsEmpty())
		return;

	//path of this clientdoc:standard|custom\module\moduleobjects\clientdocumentname 
	CString strClientProfilePath = (ePosType == CPathFinder::STANDARD) 
								? AfxGetPathFinder()->GetDocumentPath(m_nsClientDoc, CPathFinder::STANDARD)
								: AfxGetPathFinder()->GetDocumentPath(m_nsClientDoc, CPathFinder::CUSTOM);

	//add the specified partial path
	strClientProfilePath += SLASH_CHAR + strProfilePath;
	m_strXRefFileName = AfxGetPathFinder()->GetXRefFileFromThisPath(strClientProfilePath);
}

//----------------------------------------------------------------------------------------------
BOOL CXMLClientDocInfo::RenameProfilePath(const CString& strNewProfileName)
{
	if (m_strXRefFileName.IsEmpty() || strNewProfileName.IsEmpty())
		return TRUE;

	//a partire dal path del file degli external reference del clientdoc, rinomino la path che lo contiene
	CString strProfileOldPath = ::GetPath(m_strXRefFileName);
	if (!ExistPath(strProfileOldPath))
		return TRUE;

	CString strProfileNewPath;

	int pos = strProfileOldPath.ReverseFind(SLASH_CHAR);
	if (pos < 0) 
		pos = strProfileOldPath.ReverseFind(URL_SLASH_CHAR);
	if (pos >= 0)
	{
		strProfileNewPath = strProfileOldPath.Left(pos);
		strProfileNewPath += SLASH_CHAR;
		strProfileNewPath += strNewProfileName;
		if (::RenameFilePath(strProfileOldPath, strProfileNewPath))
		{
			m_strXRefFileName = AfxGetPathFinder()->GetXRefFileFromThisPath(strProfileNewPath);
			return TRUE;
		}
	}	
	return FALSE;		
}

//----------------------------------------------------------------------------------------------
BOOL CXMLClientDocInfo::RemoveProfilePath()
{
	if (m_strXRefFileName.IsEmpty())
		return TRUE;

	CString strProfilePath = ::GetPath(m_strXRefFileName);
	return !ExistPath(strProfilePath) || RemoveFolderTree(strProfilePath);
}

//----------------------------------------------------------------------------------------------
void CXMLClientDocInfo::AddDBTInfo(CXMLDBTInfo* pDBTInfo)
{
	if (!m_pDBTArray)
	{
		m_pDBTArray = new CXMLDBTInfoArray;
		m_pDBTArray->SetOwns(FALSE);
	}

	m_pDBTArray->Add(pDBTInfo);
}


//----------------------------------------------------------------------------------------------
BOOL CXMLClientDocInfo::ParseDBTInfoByNamespace(const CTBNamespace& nsDBT, CXMLDBTInfo* pDBTInfo) 
{
	if (!m_pXMLClientDBTDoc || !pDBTInfo || !nsDBT.IsValid())
		return FALSE;

	// non ho il master. Da un clientdoc vengono gestiti solo gli slaves agganciati al master
	// del documento server
	CXMLNode* pDBTSlavesNode = m_pXMLClientDBTDoc->GetRootChildByName(XML_SLAVES_TAG);
	if (!pDBTSlavesNode || pDBTSlavesNode->GetChildsNum() <= 0)
		return FALSE;

	//carico le info di ciascun slave
	CXMLNode* pSlaveNode = pDBTSlavesNode->GetFirstChild();
	while (pSlaveNode)
	{
		CString strDbtSlaveNameSpace;
		pSlaveNode->GetAttribute(XML_NAMESPACE_ATTRIBUTE, strDbtSlaveNameSpace);
		CTBNamespace aSlaveNs;
		aSlaveNs.AutoCompleteNamespace(CTBNamespace::DBT, strDbtSlaveNameSpace, aSlaveNs);
		if (aSlaveNs == nsDBT)
		{
			if (!pDBTInfo->SetDBTInfo(pSlaveNode))
				return FALSE;

			if (m_pXMLClientXRefDoc)
				pDBTInfo->LoadExternalReference(m_pXMLClientXRefDoc);
			
			pDBTInfo->SetFromClientDoc();
			//aggiungo il dbt all'array dei dbts gestiti dal clientdoc
			AddDBTInfo(pDBTInfo);
			return TRUE;
		}
		pSlaveNode = pDBTSlavesNode->GetNextChild();
	}
	return FALSE;
}


//----------------------------------------------------------------------------------------------
BOOL CXMLClientDocInfo::ParseDBTFile(CXMLDBTInfoArray* pDBTDoc)
{
	// posso avere clientdoc senza dbtslave
	if (m_strDBTFileName.IsEmpty() || !ExistFile(m_strDBTFileName))
		return TRUE; 

	if (!pDBTDoc)
		return FALSE;

	if (m_pDBTArray)
		m_pDBTArray->RemoveAll();
	else
	{
		m_pDBTArray = new CXMLDBTInfoArray;
		m_pDBTArray->SetOwns(FALSE);
	}

	int nOldDBTDoc = pDBTDoc->GetSize(); //numero di dbt già presenti nel documento
	CLocalizableXMLDocument aXMLDBTDoc(m_nsClientDoc, AfxGetPathFinder());
	if (aXMLDBTDoc.LoadXMLFile(m_strDBTFileName))
	{
		CLocalizableXMLDocument aXMLXRefDoc(m_nsClientDoc, AfxGetPathFinder());
		aXMLDBTDoc.EnableMsgMode(FALSE);
		aXMLXRefDoc.EnableMsgMode(FALSE);

		CXMLNode* pDBTSlavesNode = NULL;

		pDBTSlavesNode = aXMLDBTDoc.GetRootChildByName(XML_SLAVES_TAG);
		if (!pDBTSlavesNode)
			return TRUE;

		if (!m_strXRefDescriName.IsEmpty() && ExistFile(m_strXRefDescriName))
		{
			if (!aXMLXRefDoc.LoadXMLFile(m_strXRefDescriName))
				return FALSE;
		}
		if (pDBTDoc->ParseSlaves(pDBTSlavesNode, (!m_strXRefDescriName.IsEmpty()) ? &aXMLXRefDoc : NULL))
		{
			int nNewDBTDoc = pDBTDoc->GetSize();

			if (nOldDBTDoc >= nNewDBTDoc)
				return TRUE;

			int nDiff = nNewDBTDoc - nOldDBTDoc;

			for (int i = nNewDBTDoc - nDiff; i < pDBTDoc->GetSize(); i++)
			{
				CXMLDBTInfo* pDBTInfo = pDBTDoc->GetAt(i);
				if (pDBTInfo)
				{
					pDBTInfo->SetFromClientDoc();
					m_pDBTArray->Add(pDBTInfo);
				}
			}
			return TRUE;
		}		
	}
	return FALSE;	
}

//----------------------------------------------------------------------------------------------
BOOL CXMLClientDocInfo::SaveDBTFile()
{
	if (!m_pDBTArray || m_pDBTArray->GetSize() <= 0)
		return TRUE;

	CLocalizableXMLDocument aXMLDBTDoc(m_nsClientDoc, AfxGetPathFinder());
	CLocalizableXMLDocument aXMLXRefDoc(m_nsClientDoc, AfxGetPathFinder());
	
	CXMLNode* pnRootXRefs = aXMLXRefDoc.CreateRoot(XML_MAIN_EXTERNAL_REFERENCES_TAG);
	if(!pnRootXRefs)
		return FALSE;
	
	//scorre la struttura e crea i nodi dom mano a mano che trova nodi in struttura
	CXMLNode* pnRoot = aXMLDBTDoc.CreateRoot(XML_DBTS_TAG);
	if(!pnRoot)
		return FALSE;
	
	CXMLNode* pnSlaves = pnRoot->CreateNewChild(XML_DBT_TYPE_SLAVES_TAG);
	CXMLDBTInfo* pDbt = NULL;
	CXMLNode* pnSlave = NULL;
	CXMLNode* pnXRefDBTNode	 = NULL;

	for ( int i = 0 ; i < m_pDBTArray->GetSize() ; i++)
	{
		pDbt = (CXMLDBTInfo*)m_pDBTArray->GetAt(i);
		if (pDbt)
		{
			pnSlave = pnSlaves->CreateNewChild(pDbt->GetStrType());
		
			pnXRefDBTNode = pnRootXRefs->CreateNewChild(XML_DBT_TAG);
			pDbt->UnParse(pnSlave, pnXRefDBTNode);	
		}
	}


	aXMLDBTDoc.SaveXMLFile(m_strDBTFileName, TRUE);
	aXMLXRefDoc.SaveXMLFile(m_strXRefFileName, TRUE);
	
	return TRUE;
}


//----------------------------------------------------------------------------------------------
BOOL CXMLClientDocInfo::SaveXRefFile(BOOL bDescription /*=TRUE*/) 
{	
	if(!m_pDBTArray || m_pDBTArray->GetSize() <= 0)
		return TRUE;

	CLocalizableXMLDocument aXMLXRefDoc(m_nsClientDoc, AfxGetPathFinder());
		
	CXMLNode* pnRootXRefs = aXMLXRefDoc.CreateRoot(XML_MAIN_EXTERNAL_REFERENCES_TAG);
	if(!pnRootXRefs)
		return FALSE;
	

	for ( int i = 0 ; i < m_pDBTArray->GetSize() ; i++)
	{
		CXMLDBTInfo* pDbt = m_pDBTArray->GetAt(i);
	
		if(!pDbt)
			continue;

		if(!pDbt->m_pXRefsArray)
			pDbt->m_pXRefsArray = new CXMLXRefInfoArray;

		CXMLNode* pnXRefDBTNode = pnRootXRefs->CreateNewChild(XML_DBT_TAG);

		if (!pnXRefDBTNode)
			continue;

		pDbt->m_pXRefsArray->UnParse(pnXRefDBTNode, pDbt, bDescription);
	}

	aXMLXRefDoc.SaveXMLFile(m_strXRefFileName, TRUE);

	return TRUE;
}

//----------------------------------------------------------------------------------------------
BOOL CXMLClientDocInfo::LoadDBTFile()
{
	// posso avere clientdoc senza dbtslave
	if (m_strDBTFileName.IsEmpty())		
		return TRUE;

	if (m_strDBTFileName.IsEmpty() || !ExistFile(m_strDBTFileName))
		return TRUE;

	if (!m_pXMLClientDBTDoc)
		m_pXMLClientDBTDoc	= new CLocalizableXMLDocument(m_nsClientDoc, AfxGetPathFinder());
	if (!m_pXMLClientXRefDoc)
		m_pXMLClientXRefDoc	= new CLocalizableXMLDocument(m_nsClientDoc, AfxGetPathFinder());

	m_pXMLClientDBTDoc->EnableMsgMode(FALSE);	
	m_pXMLClientXRefDoc->EnableMsgMode(FALSE);


	if (m_pXMLClientDBTDoc->LoadXMLFile(m_strDBTFileName))
	{
		if (!m_strXRefFileName.IsEmpty() && ExistFile(m_strXRefFileName))
		{
			if (!m_pXMLClientXRefDoc->LoadXMLFile(m_strXRefFileName))
			{
				SAFE_DELETE(m_pXMLClientXRefDoc);
				return FALSE;
			}
		}
		else
			SAFE_DELETE(m_pXMLClientXRefDoc);

		return TRUE;
	}
	SAFE_DELETE(m_pDBTArray);
	SAFE_DELETE(m_pXMLClientDBTDoc);
	SAFE_DELETE(m_pXMLClientXRefDoc);
	return FALSE;
}

//----------------------------------------------------------------------------------------------
CXMLDBTInfo* CXMLClientDocInfo::AddDBTInfo(DBTSlave* pSlave)
{
	if (!pSlave) return NULL;

	CTBNamespace aDBTNS = pSlave->GetNamespace();
	if (!aDBTNS.IsValid()) return NULL;

	CXMLDBTInfo* pDBTInfo = NULL;
	if (m_pDBTArray)
	{
		pDBTInfo = m_pDBTArray->GetDBTByNamespace(aDBTNS);
		if (pDBTInfo) return pDBTInfo;
	}
		
	pDBTInfo = new  CXMLDBTInfo;
	if (!pDBTInfo->SetDBTInfo(pSlave))
	{
		delete pDBTInfo;
		pDBTInfo = NULL;
	}
	else
		AddDBTInfo(pDBTInfo);
	
	return pDBTInfo;
}

//----------------------------------------------------------------------------------------------
CXMLClientDocInfo& CXMLClientDocInfo::operator =(const CXMLClientDocInfo& aClientDocInfo)
{
	if (this != &aClientDocInfo)
		Assign(aClientDocInfo);

	return *this;
}


//----------------------------------------------------------------------------------------------
BOOL CXMLClientDocInfo::operator == (const CXMLClientDocInfo& aClientDocInfo) const
{
	if (this == &aClientDocInfo)
		return TRUE;

	return
		(
			m_nsClientDoc		== aClientDocInfo.m_nsClientDoc			  &&
			m_strClientDocName	== aClientDocInfo.m_strClientDocName	  &&
			m_strDBTFileName	== aClientDocInfo.m_strDBTFileName		  &&
			m_strXRefFileName	== aClientDocInfo.m_strXRefFileName		  &&
			m_strXRefDescriName	== aClientDocInfo.m_strXRefDescriName	  &&
			((!(m_pDBTArray && aClientDocInfo.m_pDBTArray)) || (*m_pDBTArray == *aClientDocInfo.m_pDBTArray))		
		);
}

//----------------------------------------------------------------------------------------------
BOOL CXMLClientDocInfo::operator != (const CXMLClientDocInfo& aClientDocInfo) const
{
	return !(*this == aClientDocInfo);
}

//----------------------------------------------------------------------------------------------
//class CXMLClientDocInfoArray  implementation
//----------------------------------------------------------------------------------------------
//
//----------------------------------------------------------------------------------------------
CXMLClientDocInfoArray::CXMLClientDocInfoArray()
{
}

//----------------------------------------------------------------------------------------------
CXMLClientDocInfoArray::CXMLClientDocInfoArray(const CXMLClientDocInfoArray& aClientsArray)
{
	Assign(aClientsArray);
}

//----------------------------------------------------------------------------------------------
void CXMLClientDocInfoArray::Assign(const CXMLClientDocInfoArray& aClientsArray)
{
	RemoveAll();
	for (int i = 0; i < aClientsArray.GetSize(); i++)
		Add(new CXMLClientDocInfo(*aClientsArray.GetAt(i)));

}

//----------------------------------------------------------------------------------------------
void CXMLClientDocInfoArray::SetFilesFromPartialPath(const CString& strProfilePath, CPathFinder::PosType ePosType)
{
	for (int i = 0; i < GetSize(); i++)
		if (GetAt(i))
			GetAt(i)->SetFilesFromPartialPath(strProfilePath, ePosType);
}

//----------------------------------------------------------------------------------------------
BOOL CXMLClientDocInfoArray::RenameProfilePath(const CString& strNewProfileName)
{
	BOOL bOk = TRUE;
	for (int i = 0; i < GetSize(); i++)
		if (GetAt(i))
			bOk =  GetAt(i)->RenameProfilePath(strNewProfileName) && bOk;
	return bOk;
}

//----------------------------------------------------------------------------------------------
BOOL CXMLClientDocInfoArray::RemoveProfilePath()
{
	BOOL bOk = TRUE;
	for (int i = 0; i < GetSize(); i++)
		if (GetAt(i))
			bOk =  GetAt(i)->RemoveProfilePath() && bOk;
	return bOk;
}	

//----------------------------------------------------------------------------------------------
CXMLClientDocInfo*	CXMLClientDocInfoArray::GetClientFromNamespace(const CTBNamespace& aNamespace) const
{
	if (!aNamespace.IsValid()) return NULL;

	CXMLClientDocInfo* pClient = NULL;
	for (int i = 0; i < GetSize(); i++)
	{
		pClient = GetAt(i);
		if (pClient && pClient->m_nsClientDoc ==  aNamespace)
			return pClient;
	}

	return NULL;
}


//----------------------------------------------------------------------------------------------
void CXMLClientDocInfoArray::AddDBTInfoToClient	(const CTBNamespace& aNS,  CXMLDBTInfo* pDBTInfo)
{
	if (!aNS.IsValid()) return;

	CXMLClientDocInfo* pClient = GetClientFromNamespace(aNS);
	if (pClient) pClient->AddDBTInfo(pDBTInfo);
}

//----------------------------------------------------------------------------------------------
BOOL CXMLClientDocInfoArray::ParseDBTInfoByNamespace(const CTBNamespace& nsClientDoc, const CTBNamespace& nsDBT, CXMLDBTInfo* pDBTInfo)
{
	if ( !nsClientDoc.IsValid() || !nsDBT.IsValid() )
		return FALSE;

	CXMLClientDocInfo* pClient = GetClientFromNamespace(nsClientDoc);

	return (pClient) 
			? pClient->ParseDBTInfoByNamespace(nsDBT, pDBTInfo) 
			: FALSE;
}

//----------------------------------------------------------------------------------------------
BOOL CXMLClientDocInfoArray::SaveXRefFile(BOOL bDescription /*= TRUE*/) 
{	
	BOOL bOk = TRUE;
	for (int i = 0; i < GetSize(); i++)
		if (GetAt(i))
			bOk = GetAt(i)->SaveXRefFile(bDescription) && bOk;

	return bOk;
}

//----------------------------------------------------------------------------------------------
BOOL CXMLClientDocInfoArray::SaveDBTFile()
{
	BOOL bOk = TRUE;
	for (int i = 0; i < GetSize(); i++)
		if (GetAt(i))
			bOk = GetAt(i)->SaveDBTFile() && bOk;

	return bOk;
}

//----------------------------------------------------------------------------------------------
BOOL CXMLClientDocInfoArray::ParseDBTFile(CXMLDBTInfoArray* pDBTDoc)
{
	BOOL bOk = TRUE;
	for (int i = 0; i < GetSize(); i++)
		if (GetAt(i))
			bOk = GetAt(i)->ParseDBTFile(pDBTDoc) && bOk;

	return bOk;
}


//----------------------------------------------------------------------------------------------
BOOL CXMLClientDocInfoArray::LoadDBTFile()
{
	BOOL bOk = TRUE;
	for (int i = 0; i < GetSize(); i++)
		if (GetAt(i))
			bOk = GetAt(i)->LoadDBTFile() && bOk;

	return bOk;
}

//----------------------------------------------------------------------------------------------
void CXMLClientDocInfoArray::SetAllFilesName ()
{
	for (int i = 0; i < GetSize(); i++)
		if (GetAt(i))
			GetAt(i)->SetAllFilesName();
}

//----------------------------------------------------------------------------------------------
CXMLDBTInfo* CXMLClientDocInfoArray::UpdateClientDocDBTInfo(const CTBNamespace& aClientNamespace, DBTSlave* pSlave)
{
	CXMLClientDocInfo* pClient = GetClientFromNamespace(aClientNamespace);
	return (pClient)
			? pClient->AddDBTInfo(pSlave)
			: NULL;
}

//----------------------------------------------------------------------------------------------
CXMLClientDocInfoArray&	CXMLClientDocInfoArray::operator =	(const CXMLClientDocInfoArray& aClientsArray)
{
	if (this != &aClientsArray)
		Assign(aClientsArray);

	return *this;
}

//----------------------------------------------------------------------------------------------
BOOL CXMLClientDocInfoArray::operator == (const CXMLClientDocInfoArray& aClientsArray) const
{
	if (aClientsArray.GetSize() != GetSize())
		return FALSE;

	for (int i = 0; i < aClientsArray.GetSize(); i++)
		if (*GetAt(i) != *aClientsArray.GetAt(i))
			return FALSE;

	return TRUE;
}


//----------------------------------------------------------------------------------------------
BOOL CXMLClientDocInfoArray::operator != (const CXMLClientDocInfoArray& aClientsArray) const
{
	return !(*this == aClientsArray);
}

//----------------------------------------------------------------------------------------------
//	CXMLDocObjectInfo implementation
//----------------------------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(CXMLDocObjectInfo, CObject)
//----------------------------------------------------------------------------------------------
CXMLDocObjectInfo::CXMLDocObjectInfo(const CTBNamespace& nsDoc)
	:	
	m_pHeaderInfo		(NULL),
	m_pDocDescription	(NULL),
	m_pDBTArray			(NULL),
	m_pClientDocsInfo	(NULL),
	m_ePosType			(CPathFinder::STANDARD),
	m_bIsLoaded			(FALSE),
	m_bReadOnly			(FALSE)
{
	if (nsDoc.IsValid())
	{
		SetDocInformation(nsDoc);	
		SetAllFilesName();
		AttachClientDocInfo();
	}
	else
		ASSERT(FALSE);
}

//----------------------------------------------------------------------------------------------
CXMLDocObjectInfo::CXMLDocObjectInfo(const CXMLDocObjectInfo& aDocObjInfo)
	:	
	m_pHeaderInfo		(NULL),
	m_pDocDescription	(NULL),
	m_pDBTArray			(NULL),
	m_pClientDocsInfo	(NULL),
	m_bIsLoaded			(FALSE),
	m_bReadOnly			(FALSE)
{
	Assign(aDocObjInfo);
}

//----------------------------------------------------------------------------------------------
CXMLDocObjectInfo::~CXMLDocObjectInfo()
{	
	SAFE_DELETE(m_pHeaderInfo);
	SAFE_DELETE(m_pDBTArray);	
	SAFE_DELETE(m_pClientDocsInfo);
}

//----------------------------------------------------------------------------------------------
void CXMLDocObjectInfo::Assign	(const CXMLDocObjectInfo& aDocObjInfo)
{
	if (this == &aDocObjInfo)
		return;
	
	m_nsDoc = aDocObjInfo.m_nsDoc;
	
	m_strDocumentName = aDocObjInfo.m_strDocumentName;
	m_strDocumentTitle = aDocObjInfo.m_strDocumentTitle;
	m_pDocDescription = aDocObjInfo.m_pDocDescription;

	m_strDocFileName		= aDocObjInfo.m_strDocFileName;
	m_strDBTFileName		= aDocObjInfo.m_strDBTFileName;
	m_strXRefFileName		= aDocObjInfo.m_strXRefFileName;

	m_bIsLoaded	= aDocObjInfo.m_bIsLoaded;
	m_ePosType = aDocObjInfo.m_ePosType;
	m_strUserName = aDocObjInfo.m_strUserName;
	m_bReadOnly	  = aDocObjInfo.m_bReadOnly;
	
	SetHeaderInfo(aDocObjInfo.GetHeaderInfo());
	SetDBTArray(aDocObjInfo.GetDBTInfoArray());
	SetClientDocs(aDocObjInfo.GetClientDocInfoArray());
}

//----------------------------------------------------------------------------------------------
void CXMLDocObjectInfo::SetDocInformation(const CTBNamespace& nsDoc)
{
	if (nsDoc.IsEmpty()|| !nsDoc.IsValid())
	{
		SetNamespaceDoc(CTBNamespace());
		m_strDocumentName = _T("");
		m_strDocumentTitle = _T("");
		return;
	}
	
	SetNamespaceDoc(nsDoc);
	m_strDocumentName = nsDoc.GetObjectName(CTBNamespace::DOCUMENT);
	m_pDocDescription = AfxGetDocumentDescription(nsDoc);
	
	if (!m_pDocDescription)
	{
		ASSERT(FALSE);
		TRACE1("CXMLDocObjectInfo::CXMLDocObjectInfo: the document %s isn't registered in the module", nsDoc.ToString());
		return;
	}

	m_strDocumentTitle = m_pDocDescription->GetTitle();
}

//----------------------------------------------------------------------------------------------
void CXMLDocObjectInfo::SetAllFilesName()
{
	// doc file is substituted
	m_strDocFileName = AfxGetPathFinder()->GetDocumentDocumentFullName(m_nsDoc);
	
	// dbts and external references have to integrate custom and standard definitions
	m_strDBTFileName = AfxGetPathFinder()->GetDocumentDbtsFullName(m_nsDoc);
	m_strXRefFileName = AfxGetPathFinder()->GetDocumentXRefFullName(m_nsDoc);
}

//----------------------------------------------------------------------------------------------
void CXMLDocObjectInfo::AttachClientDocInfo()
{
	CServerDocDescription* pServerInfo = NULL;
	CBaseDescriptionArray aClientsArray; 
	
	// i family potrebbero essere rappresentati da più di un when server
	for (int i=0; i <= AfxGetClientDocsTable()->GetUpperBound(); i++)
	{
		pServerInfo = AfxGetClientDocsTable()->GetAt(i); 
		if (!pServerInfo)
			continue;
		 
		if	(	// clientdoc non family
				pServerInfo->GetNamespace() == GetNamespaceDoc() ||
				// clientdoc family
				(!m_pDocDescription->IsExcludedFromFamily() && pServerInfo->IsHierarchyOf(m_pDocDescription->GetClassHierarchy()))
			)
			for (int n = 0; n < pServerInfo->GetClientDocs().GetSize(); n++)
			{
				CClientDocDescription* pClient = (CClientDocDescription*) pServerInfo->GetClientDocs().GetAt(n);
				if (pClient)
					aClientsArray.Add(new CClientDocDescription(pClient->GetNamespace(), pClient->GetTitle()));
			}
	}

	if (aClientsArray.GetSize() <= 0)
	{
		SAFE_DELETE(m_pClientDocsInfo);
		return;
	}

	if (m_pClientDocsInfo)
		SAFE_DELETE(m_pClientDocsInfo);

	m_pClientDocsInfo = new CXMLClientDocInfoArray;
	
	CClientDocDescription* pXMLModCliDoc = NULL;
	for (int nCli = 0; nCli < aClientsArray.GetSize(); nCli++)
	{
		pXMLModCliDoc = (CClientDocDescription*)aClientsArray.GetAt(nCli);
		if (pXMLModCliDoc && pXMLModCliDoc->GetNamespace().IsValid())
			m_pClientDocsInfo->Add(new CXMLClientDocInfo
											(
												pXMLModCliDoc->GetNamespace(), 
												pXMLModCliDoc->GetName()
											)
								   );
	}
}

//----------------------------------------------------------------------------------------------
BOOL CXMLDocObjectInfo::IsValid()
{
	return !m_strDocFileName.IsEmpty() && m_pHeaderInfo;
}

//----------------------------------------------------------------------------------------------
int	CXMLDocObjectInfo::GetDBTIndexFromNamespace (const CTBNamespace& ns) const
{
	if (!m_pDBTArray)
		return -1;
	
	for(int i = 0 ; i < m_pDBTArray->GetSize() ; i++)
	{
		if (
				m_pDBTArray->GetAt(i) && 
				m_pDBTArray->GetAt(i)->m_nsDBT == ns
			)
			return i;
	}

	return -1;
}

//----------------------------------------------------------------------------------------------
CXMLDBTInfo* CXMLDocObjectInfo::GetDBTFromNamespace (const CTBNamespace& nsDBT) const
{
	if (!m_pDBTArray)
		return NULL;

	for(int i = 0 ; i < m_pDBTArray->GetSize() ; i++)
	{
		if (
				m_pDBTArray->GetAt(i) && 	
				m_pDBTArray->GetAt(i)->m_nsDBT == nsDBT
			)			
			return m_pDBTArray->GetAt(i);
	}

	return NULL;
}

//----------------------------------------------------------------------------------------------
CXMLDBTInfo* CXMLDocObjectInfo::GetDBTByXRef(CXMLXRefInfo* pXMLXRefInfo)
{
	if (!m_pDBTArray)
		return NULL;
	
	return m_pDBTArray->GetDBTByXRef(pXMLXRefInfo);
}


//----------------------------------------------------------------------------------------------
CXMLDBTInfo* CXMLDocObjectInfo::GetDBTMaster() const
{
	if (!m_pDBTArray)
		return NULL;

	for(int i = 0 ; i < m_pDBTArray->GetSize() ; i++)
	{
		if (
				m_pDBTArray->GetAt(i) && 	
				m_pDBTArray->GetAt(i)->GetType() == CXMLDBTInfo::MASTER_TYPE
			)			
			return m_pDBTArray->GetAt(i);
	}

	return NULL;
}

//----------------------------------------------------------------------------------------------
void CXMLDocObjectInfo::GetDBTXRefList(CXMLXRefInfoArray* pXMLXRefInfoArray, CTBNamespace aDBTNS)
{
	if(!pXMLXRefInfoArray || !m_pDBTArray || !aDBTNS.IsValid())
		return;
	
	CXMLDBTInfo* pXMLDBTInfo = NULL;
	for(int i = 0 ; i < m_pDBTArray->GetSize() ; i++)
	{
		pXMLDBTInfo = m_pDBTArray->GetAt(i);
		if (!pXMLDBTInfo)
			continue;
		
		//il dbt e' quello che sto cercando
		if(pXMLDBTInfo->GetNamespace() == aDBTNS)
		{
			if(!pXMLDBTInfo->GetXMLXRefInfoArray())
				return;

			for(int n = 0 ; n < pXMLDBTInfo->GetXMLXRefInfoArray()->GetSize() ; n++)
			{
				CXMLXRefInfo* pXRefInfo = pXMLDBTInfo->GetXMLXRefInfoArray()->GetAt(n);
				if(pXRefInfo)
					pXMLXRefInfoArray->Add(pXRefInfo);
			}

			return;
		}
	}
}

//----------------------------------------------------------------------------------------------
int CXMLDocObjectInfo::AddDBT(CXMLDBTInfo* pDBT)
{
	int n = -1;
	if (pDBT)
	{
		if (!m_pDBTArray)
		{
			m_pDBTArray = new CXMLDBTInfoArray;
			m_pDBTArray->SetOwns(FALSE);
		}

		return m_pDBTArray->Add(pDBT);
	}

	return n;	
}

//----------------------------------------------------------------------------------------------
void CXMLDocObjectInfo::RemoveDBTAt(int nIdx)
{
	if (!m_pDBTArray || m_pDBTArray->GetSize() < nIdx || nIdx < 0)
	{
		ASSERT_VALID(m_pDBTArray);
		return;
	}

	if (m_pDBTArray->GetAt(nIdx))
		m_pDBTArray->RemoveAt(nIdx);
			
}

//----------------------------------------------------------------------------------------------
void CXMLDocObjectInfo::SetDBTAt(int nIdx, CXMLDBTInfo* pDBT)
{
	if (!m_pDBTArray || m_pDBTArray->GetSize() < nIdx || nIdx < 0)
	{
		ASSERT_VALID(m_pDBTArray);
		return;
	}

	m_pDBTArray->SetAt(nIdx, pDBT);
}

//----------------------------------------------------------------------------------------------
CXMLDBTInfo* CXMLDocObjectInfo::GetDBTAt(int nIdx)const
{
	if (!m_pDBTArray || m_pDBTArray->GetSize() < nIdx || nIdx < 0)
		return NULL;

	if(m_pDBTArray->GetAt(nIdx))
		return m_pDBTArray->GetAt(nIdx);
	
	return NULL;
}

//----------------------------------------------------------------------------------------------
BOOL CXMLDocObjectInfo::SetNamespaceDoc (LPCTSTR lpszNSDoc)		
{
	CTBNamespace nsDoc(lpszNSDoc);
	if (!nsDoc.IsValid()) return FALSE;

	return SetNamespaceDoc(nsDoc);
}

//----------------------------------------------------------------------------------------------
BOOL CXMLDocObjectInfo::SetNamespaceDoc(const CTBNamespace& nsDoc)
{
	m_nsDoc = nsDoc;
	return OnNamespaceDocChanged();
}

//----------------------------------------------------------------------------------------------
BOOL CXMLDocObjectInfo::LoadHeaderFile()
{
	CString strDocFileToLoad = m_strDocFileName; 
	
	if (strDocFileToLoad.IsEmpty() || !ExistFile(strDocFileToLoad))
		strDocFileToLoad = AfxGetPathFinder()->GetDocumentDocumentFullName(m_nsDoc);
	
	if(strDocFileToLoad.IsEmpty())
		return FALSE;

	if (m_pHeaderInfo)
		m_pHeaderInfo->Clear();
	else
		m_pHeaderInfo = new CXMLHeaderInfo(m_nsDoc);	

	ASSERT(m_pHeaderInfo);

	CLocalizableXMLDocument aXMLHdrDoc(m_nsDoc, AfxGetPathFinder());
	aXMLHdrDoc.EnableMsgMode(FALSE);
	if (
			aXMLHdrDoc.LoadXMLFile(strDocFileToLoad) &&
			m_pHeaderInfo->Parse(&aXMLHdrDoc)
		)
		return TRUE;
	
	SAFE_DELETE(m_pHeaderInfo);
	return FALSE;
}

//----------------------------------------------------------------------------------------------
BOOL CXMLDocObjectInfo::LoadDBTFiles
		(
			CLocalizableXMLDocument*&	pDBTDoc,
			CLocalizableXMLDocument*&	pXRefDoc,
			BOOL						bParse /*FALSE*/
		)
{
	if (!IsValid()) 
		return FALSE;

	if (m_pDBTArray)
		m_pDBTArray->RemoveAll();
	else
	{
		m_pDBTArray = new CXMLDBTInfoArray;
		m_pDBTArray->SetOwns(TRUE);
	}

	ASSERT_VALID(m_pDBTArray);

	if (!pDBTDoc)  pDBTDoc  = new CLocalizableXMLDocument(m_nsDoc, AfxGetPathFinder());

	pDBTDoc->EnableMsgMode(FALSE);	
	if (!pDBTDoc->LoadXMLFile(m_strDBTFileName))
	{
		SAFE_DELETE(pDBTDoc);
		return FALSE;
	}

	if (!m_strXRefFileName.IsEmpty() && ExistFile(m_strXRefFileName))
	{
		// related external refereces
		if (!pXRefDoc) 
			pXRefDoc = new CLocalizableXMLDocument(m_nsDoc, AfxGetPathFinder());
		pXRefDoc->EnableMsgMode(FALSE);
		if (!pXRefDoc->LoadXMLFile(m_strXRefFileName))
		{
			SAFE_DELETE(pXRefDoc);
			return FALSE;
		}		
	}
	else
		SAFE_DELETE(pXRefDoc);

	if (bParse)
		m_pDBTArray->Parse (pDBTDoc, pXRefDoc);
		
	// client documents 
	if (m_pClientDocsInfo && !m_pClientDocsInfo->LoadDBTFile())
		return FALSE;
		
	if (bParse)
		m_pClientDocsInfo->ParseDBTFile (m_pDBTArray);

	return TRUE;
}

//----------------------------------------------------------------------------------------------
BOOL CXMLDocObjectInfo::LoadDBTFile()
{
	CLocalizableXMLDocument* pXMLDBTDoc = NULL;
	CLocalizableXMLDocument* pXMLXRefDoc = NULL;
	
	BOOL bOk = LoadDBTFiles(pXMLDBTDoc, pXMLXRefDoc, TRUE);

	SAFE_DELETE(pXMLDBTDoc);
	SAFE_DELETE(pXMLXRefDoc);
	
	if (m_pDBTArray->GetSize() == 0)
		SAFE_DELETE(m_pDBTArray);

	return bOk;
}
	

//----------------------------------------------------------------------------------------------
BOOL CXMLDocObjectInfo::LoadAllFiles()
{
	return (m_bIsLoaded = 
				LoadHeaderFile		() &&
				LoadDBTFile		() 
			);
}

//----------------------------------------------------------------------------------------------
BOOL CXMLDocObjectInfo::SaveHeaderFile() 
{
	return m_pHeaderInfo && m_pHeaderInfo->UnParse(m_strDocFileName);
}


//----------------------------------------------------------------------------------------------
BOOL CXMLDocObjectInfo::SaveDBTFile() 
{
	return 
			m_pDBTArray && 
			m_pDBTArray->UnParse(m_strDBTFileName, m_strXRefFileName) &&
			(!m_pClientDocsInfo || m_pClientDocsInfo->SaveDBTFile());
}

//----------------------------------------------------------------------------------------------
BOOL CXMLDocObjectInfo::SaveAllFiles() 
{
	return (
				SaveHeaderFile		() &&
				SaveDBTFile			() 
			);
}

//----------------------------------------------------------------------------------------------
void CXMLDocObjectInfo::SetHeaderInfo(const CXMLHeaderInfo*	pHeaderInfo) 
{
	if (pHeaderInfo)
	{
		if (m_pHeaderInfo)
			*m_pHeaderInfo = *pHeaderInfo;
		else
			m_pHeaderInfo = new CXMLHeaderInfo(*pHeaderInfo);
	}
	else
	{
		if (m_pHeaderInfo)
			delete m_pHeaderInfo;
		m_pHeaderInfo = NULL;
	}
}

//----------------------------------------------------------------------------------------------
void CXMLDocObjectInfo::SetHeaderInfo(const CXMLDocObjectInfo& aDocInfo) 
{
	if (this == &aDocInfo)
		return;
	
	SetHeaderInfo(aDocInfo.GetHeaderInfo());
}

//----------------------------------------------------------------------------------------------
void CXMLDocObjectInfo::SetDBTArray(const CXMLDBTInfoArray*	pDBTArray) 
{
	if (pDBTArray)
	{
		if (m_pDBTArray)
			*m_pDBTArray = *pDBTArray;
		else
			m_pDBTArray = new CXMLDBTInfoArray(*pDBTArray);
	}
	else
	{
		if (m_pDBTArray)
			delete m_pDBTArray;
		m_pDBTArray = NULL;
	}
}

//----------------------------------------------------------------------------------------------
void CXMLDocObjectInfo::SetDBTArray(const CXMLDocObjectInfo& aDocInfo) 
{
	if (this == &aDocInfo)
		return;
	
	SetDBTArray(aDocInfo.GetDBTInfoArray());
}

//----------------------------------------------------------------------------------------------
void CXMLDocObjectInfo::SetClientDocs(const CXMLClientDocInfoArray* pClientDocArray)
{
	if (pClientDocArray)
	{
		if (m_pClientDocsInfo)
			SAFE_DELETE(m_pClientDocsInfo);
		
		m_pClientDocsInfo = new CXMLClientDocInfoArray(*pClientDocArray);
		
		// devo assegnare i dbt dei clientdoc all'array dei dbt del documento
		// questo perchè devono avere la stessa istanza dell'elemento dbt del clientdoc
		CXMLClientDocInfo* pXMLClientDoc = NULL;
		for (int i = 0; i < m_pClientDocsInfo->GetCount(); i++)
		{
			pXMLClientDoc = m_pClientDocsInfo->GetAt(i);
			
			if (pXMLClientDoc->m_pDBTArray)
			{
				if (!m_pDBTArray)
					m_pDBTArray = new CXMLDBTInfoArray();

				for (int j = 0; j < pXMLClientDoc->m_pDBTArray->GetCount(); j++)
					if (pXMLClientDoc->m_pDBTArray->GetAt(j))
					{
						ASSERT_VALID(m_pDBTArray); 
						if (m_pDBTArray)
							m_pDBTArray->Add(pXMLClientDoc->m_pDBTArray->GetAt(j));
					}
			}
		}
	}
	else
		SAFE_DELETE(m_pClientDocsInfo);		
}


//----------------------------------------------------------------------------------------------
void CXMLDocObjectInfo::SetClientDocs(const CXMLDocObjectInfo& aDocInfo) 
{
	if (this == &aDocInfo)
		return;
	
	SetClientDocs(aDocInfo.GetClientDocInfoArray());
}

//----------------------------------------------------------------------------------------------
CXMLDBTInfo* CXMLDocObjectInfo::UpdateDBTInfo(DBTSlave* pDBTSlave)
{
	CXMLDBTInfo* pDBTSlaveInfo = new CXMLDBTInfo;
	pDBTSlaveInfo->SetDBTInfo(pDBTSlave);
	
	if(!pDBTSlave->GetXMLDBTInfo())
		pDBTSlave->SetXMLDBTInfo(pDBTSlaveInfo);

	if(!m_pDBTArray)
	{
		m_pDBTArray = new CXMLDBTInfoArray;
		m_pDBTArray->SetOwns(FALSE);
	}

	m_pDBTArray->Add(pDBTSlaveInfo);

	return pDBTSlaveInfo;
}

//----------------------------------------------------------------------------------------------
CXMLDBTInfo* CXMLDocObjectInfo::UpdateClientDocDBTInfo(const CTBNamespace& aClientNS, DBTSlave* pDBTSlave)
{
	return (m_pClientDocsInfo)
			? m_pClientDocsInfo->UpdateClientDocDBTInfo(aClientNS, pDBTSlave)
			: NULL;
}			

//----------------------------------------------------------------------------------------------
CXMLDocObjectInfo& CXMLDocObjectInfo::operator = (const CXMLDocObjectInfo& aDocObjInfo)
{
	if (this == &aDocObjInfo)
		return *this;
	
	Assign(aDocObjInfo);
	
	return *this;
}

//------------------------------------------------------------------------------
BOOL CXMLDocObjectInfo::operator == (const CXMLDocObjectInfo& aDocObjInfo) const
{
	if (this == &aDocObjInfo) return TRUE;

	return
		(
			m_nsDoc					== aDocObjInfo.m_nsDoc				&&
			m_bIsLoaded				== aDocObjInfo.m_bIsLoaded			&&
			m_ePosType				== aDocObjInfo.m_ePosType			&&
			m_strUserName			== aDocObjInfo.m_strUserName		&&
			m_bReadOnly				== aDocObjInfo.m_bReadOnly			&&
			m_strDocumentName		== aDocObjInfo.m_strDocumentName	&&
			m_strDocumentTitle		== aDocObjInfo.m_strDocumentTitle	&&
			m_strDocFileName		== aDocObjInfo.m_strDocFileName		&&
			m_strDBTFileName		== aDocObjInfo.m_strDBTFileName		&&
			m_strXRefFileName		== aDocObjInfo.m_strXRefFileName	&&

			(!(m_pHeaderInfo && aDocObjInfo.m_pHeaderInfo) ||
			*m_pHeaderInfo == *aDocObjInfo.m_pHeaderInfo)				&&

			(!(m_pDBTArray && aDocObjInfo.m_pDBTArray) ||
			*m_pDBTArray == *aDocObjInfo.m_pDBTArray)					&&			

			(!(m_pClientDocsInfo && aDocObjInfo.m_pClientDocsInfo) || 
			  *m_pClientDocsInfo	== *aDocObjInfo.m_pClientDocsInfo)	
		);
}

//------------------------------------------------------------------------------
BOOL CXMLDocObjectInfo::operator != (const CXMLDocObjectInfo& aDocObjInfo) const
{
	return !(*this == aDocObjInfo);
}

//----------------------------------------------------------------------------------------------
//	CXMLDocInfo
//----------------------------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(CXMLDocInfo, CXMLDocObjectInfo)
//----------------------------------------------------------------------------------------------
CXMLDocInfo::CXMLDocInfo(const CTBNamespace& nsDoc)
	:
	CXMLDocObjectInfo	(nsDoc),
	m_pXMLDBTDoc		(NULL),
	m_pXMLXRefDoc		(NULL),
	m_pDefaultInfo		(NULL)
{
}

//----------------------------------------------------------------------------
CXMLDocInfo::CXMLDocInfo(const CXMLDocInfo& aXMLDocInfo)
	:
	CXMLDocObjectInfo	(*(CXMLDocObjectInfo*)&aXMLDocInfo),
	m_pXMLDBTDoc		(NULL),
	m_pXMLXRefDoc		(NULL),
	m_pDefaultInfo		(NULL)
{
	Assign(aXMLDocInfo);
}

//----------------------------------------------------------------------------------------------
CXMLDocInfo::~CXMLDocInfo()
{
	SAFE_DELETE(m_pXMLDBTDoc);
	SAFE_DELETE(m_pXMLXRefDoc);
	SAFE_DELETE(m_pDefaultInfo);
}

//----------------------------------------------------------------------------------------------
void CXMLDocInfo::SetAllFilesName()
{
	CXMLDocObjectInfo::SetAllFilesName();
}

//----------------------------------------------------------------------------
BOOL CXMLDocInfo::IsDataEqual(const CXMLDocInfo& aXMLDocInfo) const
{
	if (this == &aXMLDocInfo)
		return TRUE;
	
	return *((CXMLDocObjectInfo*)this)	==	*((CXMLDocObjectInfo*)(&aXMLDocInfo));
}

//----------------------------------------------------------------------------
BOOL CXMLDocInfo::IsEqual(const CXMLDocInfo& aXMLDocInfo) const
{
	if (this == &aXMLDocInfo)
		return TRUE;
	
	return 
		(
			*((CXMLDocObjectInfo*)this)	==	*((CXMLDocObjectInfo*)(&aXMLDocInfo))&&
			*m_pDefaultInfo				==	*aXMLDocInfo.m_pDefaultInfo			&&
			m_strDefaultsFileName		==	aXMLDocInfo.m_strDefaultsFileName	
		);
}

//----------------------------------------------------------------------------------------------
CXMLDocInfo& CXMLDocInfo::operator = (const CXMLDocInfo& aDocInfo)
{
	Assign(aDocInfo);

	return *this;
}

//----------------------------------------------------------------------------------------------
void CXMLDocInfo::Assign(const CXMLDocInfo& aXMLDocInfo)
{
	CXMLDocObjectInfo::Assign(*(CXMLDocObjectInfo*)&aXMLDocInfo);

	if(m_pDefaultInfo && aXMLDocInfo.m_pDefaultInfo)
	{
		delete m_pDefaultInfo;
		m_pDefaultInfo = new CXMLDefaultInfo(*aXMLDocInfo.m_pDefaultInfo);
	}
	
	if(!m_pDefaultInfo && aXMLDocInfo.m_pDefaultInfo)
		m_pDefaultInfo = new CXMLDefaultInfo(*aXMLDocInfo.m_pDefaultInfo);

	if(m_pDefaultInfo && !aXMLDocInfo.m_pDefaultInfo)
	{
		delete m_pDefaultInfo;
		m_pDefaultInfo = NULL;
	}

	m_strDefaultsFileName =	aXMLDocInfo.m_strDefaultsFileName;
}

//----------------------------------------------------------------------------------------------
BOOL CXMLDocInfo::LoadAllFiles()
{
	return (
				CXMLDocObjectInfo::LoadAllFiles() &&
				LoadDefaultsFile() 
			);
}

//----------------------------------------------------------------------------------------------
BOOL CXMLDocInfo::LoadDBTFile()
{
	// loads only files and does not parse them
	BOOL bOk = LoadDBTFiles(m_pXMLDBTDoc, m_pXMLXRefDoc, FALSE);

	if (m_pDBTArray->GetSize() == 0)
		SAFE_DELETE(m_pDBTArray);

	return bOk;
}

//----------------------------------------------------------------------------------------------
BOOL CXMLDocInfo::LoadDefaultsFile()
{
	if (m_pDefaultInfo)
		m_pDefaultInfo->Clear();
	else
		m_pDefaultInfo = new CXMLDefaultInfo(this);	

	return m_pDefaultInfo->Parse();
}

//----------------------------------------------------------------------------------------------
BOOL CXMLDocInfo::SetPreferredProfile(const CString & strPrefPRofile)
{
	if(!m_pDefaultInfo)
		m_pDefaultInfo = new CXMLDefaultInfo(this);
	
	return m_pDefaultInfo->SetPreferredProfile(strPrefPRofile);
}

//----------------------------------------------------------------------------------------------
BOOL CXMLDocInfo::ParseDBTInfoByNamespace (const CTBNamespace& nsDBT, CXMLDBTInfo* pDBTInfo) 
{
	if ((!m_pXMLDBTDoc) || !pDBTInfo || !nsDBT.IsValid() )
		return FALSE;
	
	CXMLNode* pDBTMasterNode = NULL;
	CXMLNode* pDBTSlavesNode = NULL;
	if (m_pXMLDBTDoc)
	{
		pDBTMasterNode = m_pXMLDBTDoc->GetRootChildByName(XML_DBT_TYPE_MASTER_TAG);

		if (pDBTMasterNode) 
		{
			CString nsMaster;
			pDBTMasterNode->GetAttribute(XML_NAMESPACE_ATTRIBUTE, nsMaster);
			CTBNamespace aNsMaster;
			aNsMaster.AutoCompleteNamespace(CTBNamespace::DBT, nsMaster, aNsMaster);
			if (aNsMaster == nsDBT)
			{
				// carico le info del master
				if (!pDBTInfo->SetDBTInfo(pDBTMasterNode))
					return FALSE;

				if (m_pXMLXRefDoc)
					pDBTInfo->LoadExternalReference(m_pXMLXRefDoc);

				pDBTInfo->LoadUniversalKeysInfo(pDBTMasterNode);
				pDBTInfo->LoadFixedKeysInfo(pDBTMasterNode);
				pDBTInfo->LoadBusinessConstraints(pDBTMasterNode);

				return TRUE;
			}

			// scendo nel livello degli slaves
			pDBTSlavesNode = pDBTMasterNode->GetChildByName(XML_SLAVES_TAG);
			if (pDBTSlavesNode)
			{
				//carico le info di ciascun slave
				CXMLNode* pSlaveNode = pDBTSlavesNode->GetFirstChild();
				while (pSlaveNode)
				{
					CString nsSlave;
					pSlaveNode->GetAttribute(XML_NAMESPACE_ATTRIBUTE, nsSlave);
					CTBNamespace aNsSlave;
					aNsSlave.AutoCompleteNamespace(CTBNamespace::DBT, nsSlave, aNsSlave);

					if (aNsSlave == nsDBT)
					{
						if (!pDBTInfo->SetDBTInfo(pSlaveNode))
							return FALSE;

						if (m_pXMLXRefDoc)
							pDBTInfo->LoadExternalReference(m_pXMLXRefDoc);

						return TRUE;
					}
					pSlaveNode = pDBTSlavesNode->GetNextChild();
				}
			}
		}
	}

	return FALSE;
}

// il terzo parametro mi serve per capire se si tratta di un dbt agganciato da un clientdoc
//----------------------------------------------------------------------------------------------
BOOL CXMLDocInfo::LoadXMLDBTInfo(const CTBNamespace& aDBTNS, CXMLDBTInfo* pDBTInfo, const CTBNamespace& aClientNS)
{	
	if (!pDBTInfo || !aDBTNS.IsValid()) return FALSE;

	BOOL bOk = FALSE;
	// se e' un dbt agganciato da un clientdoc prima controllo l'esistenza della sua descrizione nel file dbts.xml del clientdoc
	// e se no presente nel file dbts.xml del documento. Server per il metodo CXMLDocGenerator::CorrectInfo che permette di aggiornare
	// la descrizione del documento
	if ((aClientNS.IsValid() && m_pClientDocsInfo))
	{
		bOk = m_pClientDocsInfo->ParseDBTInfoByNamespace(aClientNS, aDBTNS, pDBTInfo) || ParseDBTInfoByNamespace(aDBTNS, pDBTInfo);
		if (bOk)
			pDBTInfo->SetFromClientDoc();
	}
	else
		bOk = ParseDBTInfoByNamespace(aDBTNS, pDBTInfo);
	
	if (bOk)
	{
		pDBTInfo->m_nsDBT = aDBTNS;

		if(!m_pDBTArray)
		{
			m_pDBTArray = new CXMLDBTInfoArray;
			m_pDBTArray->SetOwns(FALSE);
		}
		
		m_pDBTArray->Add(pDBTInfo);
	}
	//@@TODO BAUZI: parlare con Rinaldi
	//else
	//	ASSERT(FALSE);
	
	return bOk;
}

//----------------------------------------------------------------------------------------------
BOOL CXMLDocInfo::SaveDefaultFile() 
{
	if(!m_pDefaultInfo)
		m_pDefaultInfo = new CXMLDefaultInfo(this);

	return m_pDefaultInfo && m_pDefaultInfo->UnParse();
}

//----------------------------------------------------------------------------------------------
BOOL CXMLDocInfo::SaveAllFiles() 
{
	return(
			CXMLDocObjectInfo::SaveAllFiles() &&
			SaveDefaultFile()	
		  );
}

//----------------------------------------------------------------------------------------------
CString CXMLDocInfo::GetMasterTableNamespace(const CTBNamespace& aDocNamespace)
{
	CString strFileDBT = AfxGetPathFinder()->GetDocumentDbtsFullName(aDocNamespace);
	if (strFileDBT.IsEmpty())
		return _T("");

	CLocalizableXMLDocument	aXMLDBTDoc(aDocNamespace, AfxGetPathFinder());
	if (aXMLDBTDoc.LoadXMLFile(strFileDBT))
	{
		CXMLDBTInfoArray	aXMLDBTInfoArray;
		CXMLNode* pDBTMasterNode = aXMLDBTDoc.GetRootChildByName(XML_DBT_TYPE_MASTER_TAG);
		if (!pDBTMasterNode)
			return _T("");

		CXMLNode* pDBTMasterTableNode = pDBTMasterNode->GetChildByName(XML_TABLE_TAG);

		CString strTableName;
		CTBNamespace aTableNamespace;
		if (pDBTMasterTableNode  && pDBTMasterTableNode->GetText(strTableName))
		{
			CString strNameSpace;
			pDBTMasterTableNode->GetAttribute(XML_NAMESPACE_ATTRIBUTE, strNameSpace);
			if (!strNameSpace.IsEmpty())
				aTableNamespace.AutoCompleteNamespace(CTBNamespace::TABLE, strNameSpace, aTableNamespace);
			return aTableNamespace.ToString();
		}		
	}
	return _T("");
}


/////////////////////////////////////////////////////////////////////////////
// CAppDocumentsTreeCtrl
/////////////////////////////////////////////////////////////////////////////
IMPLEMENT_DYNCREATE(CAppDocumentsTreeCtrl, CTBTreeCtrl)

BEGIN_MESSAGE_MAP(CAppDocumentsTreeCtrl, CTBTreeCtrl)
	//{{AFX_MSG_MAP(CAppDocumentsTreeCtrl)
	ON_WM_LBUTTONDOWN		()
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
CAppDocumentsTreeCtrl::CAppDocumentsTreeCtrl(BOOL bReadOnly /*=FALSE*/)
{
	m_bReadOnly = bReadOnly;

	InitializeImageList();
}

//-----------------------------------------------------------------------------
CAppDocumentsTreeCtrl::~CAppDocumentsTreeCtrl()
{
	m_ImageList.DeleteImageList();
}

//-------------------------------------------------------------------------------
void CAppDocumentsTreeCtrl::InitializeImageList()
{
	CString asPaths[3];
	asPaths[0] = TBGlyph(szIconAddOn);
	asPaths[1] = TBGlyph(szIconModule);
	asPaths[2] = TBGlyph(szIconDocumentIcon);

	for (size_t i = 0; i < 3; i++)
	{
		HICON hIcon = TBLoadImage(asPaths[i]);
		if (i == 0)
		{
			CSize iconSize = GetHiconSize(hIcon);
			m_ImageList.Create(iconSize.cx, iconSize.cy, ILC_COLOR32, 16, 16);
			m_ImageList.SetBkColor(CLR_WHITE);
		}

		m_ImageList.Add(hIcon);
		::DeleteObject(hIcon);
	}
}

//-------------------------------------------------------------------------------
void CAppDocumentsTreeCtrl::OnLButtonDown(UINT nFlags, CPoint DPMousePos)
{
	if(!m_bReadOnly)
		__super::OnLButtonDown(nFlags, DPMousePos);
}

//-------------------------------------------------------------------------------
void CAppDocumentsTreeCtrl::FillTree(const CTBNamespace& aDocNSToSel)
{
	if (!GetImageList(TVSIL_NORMAL))
		SetImageList(&m_ImageList, TVSIL_NORMAL);

	DeleteAllItems();
	
	for( int nAppIdx = 0 ; nAppIdx < AfxGetAddOnAppsTable()->GetSize() ; nAppIdx++)
	{
		AddOnApplication* pAddOnApp = AfxGetAddOnAppsTable()->GetAt(nAppIdx);
		if (!pAddOnApp || !pAddOnApp->m_pAddOnModules || !pAddOnApp->m_pAddOnModules->GetSize())
			continue;		

		HTREEITEM hAddOnItem = InsertItem(pAddOnApp->GetTitle(),ADDON_IMAGE_IDX,ADDON_IMAGE_IDX);
		if (hAddOnItem)
			SetItemData(hAddOnItem, (DWORD)pAddOnApp);

		if(!hAddOnItem || !pAddOnApp->m_pAddOnModules)
			continue;

		for( int nModIdx = 0 ; nModIdx < pAddOnApp->m_pAddOnModules->GetSize() ; nModIdx++)
		{
			AddOnModule* pAddOnMod = pAddOnApp->m_pAddOnModules->GetAt(nModIdx);

			CBaseDescriptionArray* pDocArray = AfxGetDocumentDescriptionsOf(pAddOnMod->m_Namespace);

			if (!pAddOnMod || !pDocArray || !pDocArray->GetSize())
			{
				if (pDocArray)	SAFE_DELETE(pDocArray);
				continue;
			}
				

			HTREEITEM hModuleItem = InsertItem((LPCTSTR)pAddOnMod->GetModuleTitle(), MODULE_IMAGE_IDX, MODULE_IMAGE_IDX, hAddOnItem);
			if (hModuleItem)
				SetItemData(hModuleItem, (DWORD)pAddOnMod);

			if(!hModuleItem)
			{
				SAFE_DELETE(pDocArray);
				continue;
			}

			for(int nDocIdx = 0 ; nDocIdx < pDocArray->GetSize() ; nDocIdx++)
			{
				CDocumentDescription* pDocModInfo = (CDocumentDescription*) pDocArray->GetAt(nDocIdx);
				if (!pDocModInfo || pDocModInfo->IsTransferDisabled() || pDocModInfo->IsExcludeFromExtRef())
					continue;

				CViewModeDescription* pViewModeInfo = pDocModInfo->GetViewMode(szDefaultViewMode);
				if (pViewModeInfo && pViewModeInfo->GetType() == VMT_BATCH)
					continue;

				//se il documento non ha la descrizione in xml è inutile che venga mostrato
				CString strDescriptionPath = AfxGetPathFinder()->GetDocumentDescriptionPath(pDocModInfo->GetNamespace(), CPathFinder::STANDARD);
				if (strDescriptionPath.IsEmpty() || !::ExistPath(strDescriptionPath))
					continue;

				HTREEITEM hDocumentItem = InsertItem((LPCTSTR)pDocModInfo->GetTitle(), DOCUMENT_IMAGE_IDX, DOCUMENT_IMAGE_IDX, hModuleItem);

				if (hDocumentItem)
					SetItemData(hDocumentItem, (DWORD)pDocModInfo);

				if(!hDocumentItem)
					continue;

				if(aDocNSToSel.IsValid() && pDocModInfo->GetNamespace() == aDocNSToSel)
					SelectItem(hDocumentItem);
			}

			delete pDocArray;
		}
	}
}

//----------------------------------------------------------------------------
CAppDocumentsTreeCtrl::ItemType	CAppDocumentsTreeCtrl::GetItemType(HTREEITEM hItem) const
{
	CObject* pItemObject = NULL;

	if (!hItem || !(pItemObject = (CObject*)GetItemData(hItem)))
		return APP_DOC_TREE_ITEM_TYPE_UNDEFINED;

	ASSERT_VALID(pItemObject);

	if (pItemObject->IsKindOf(RUNTIME_CLASS(AddOnApplication)))
		return APP_DOC_TREE_ITEM_TYPE_ADDONAPP;
	if (pItemObject->IsKindOf(RUNTIME_CLASS(AddOnModule)))
		return APP_DOC_TREE_ITEM_TYPE_ADDONMODULE;
	if (pItemObject->IsKindOf(RUNTIME_CLASS(CDocumentDescription)))
		return APP_DOC_TREE_ITEM_TYPE_DOCUMENT;

	return APP_DOC_TREE_ITEM_TYPE_UNDEFINED;
}

//----------------------------------------------------------------------------
AddOnApplication* CAppDocumentsTreeCtrl::GetCurrentAddOnApp(HTREEITEM* lphDBTItem /* = NULL*/) const
{
	if (lphDBTItem)
		*lphDBTItem = NULL;

	HTREEITEM hSelItem = GetSelectedItem();
	if (hSelItem && GetItemType(hSelItem) == APP_DOC_TREE_ITEM_TYPE_DOCUMENT)
		hSelItem = GetParentItem(hSelItem);
	if (hSelItem && GetItemType(hSelItem) == APP_DOC_TREE_ITEM_TYPE_ADDONMODULE)
		hSelItem = GetParentItem(hSelItem);
	if (!hSelItem || GetItemType(hSelItem) != APP_DOC_TREE_ITEM_TYPE_ADDONAPP)
		return NULL;

	if (lphDBTItem)
		*lphDBTItem = hSelItem;
	
	return (AddOnApplication*)GetItemData(hSelItem);
}

//----------------------------------------------------------------------------
AddOnModule* CAppDocumentsTreeCtrl::GetCurrentAddOnModule(HTREEITEM* lphDBTItem /* = NULL*/) const
{
	if (lphDBTItem)
		*lphDBTItem = NULL;

	HTREEITEM hSelItem = GetSelectedItem();
	if (hSelItem && GetItemType(hSelItem) == APP_DOC_TREE_ITEM_TYPE_DOCUMENT)
		hSelItem = GetParentItem(hSelItem);
	if (!hSelItem || GetItemType(hSelItem) != APP_DOC_TREE_ITEM_TYPE_ADDONMODULE)
		return NULL;

	if (lphDBTItem)
		*lphDBTItem = hSelItem;
	
	return (AddOnModule*)GetItemData(hSelItem);
}

//----------------------------------------------------------------------------
CDocumentDescription* CAppDocumentsTreeCtrl::GetCurrentDocInfo(HTREEITEM* lphDBTItem /* = NULL*/) const
{
	if (lphDBTItem)
		*lphDBTItem = NULL;

	HTREEITEM hSelItem = GetSelectedItem();

	if (!hSelItem || GetItemType(hSelItem) != APP_DOC_TREE_ITEM_TYPE_DOCUMENT)
		return NULL;

	if (lphDBTItem)
		*lphDBTItem = hSelItem;
	
	return (CDocumentDescription*)GetItemData(hSelItem);
}

//----------------------------------------------------------------------------
CTBNamespace CAppDocumentsTreeCtrl::GetCurrentDocNamespace (HTREEITEM* lphDBTItem /* = NULL*/) const
{
	CDocumentDescription* pDocModInfo = GetCurrentDocInfo(lphDBTItem);
	
	return pDocModInfo ? pDocModInfo->GetNamespace() : CTBNamespace();
}

//----------------------------------------------------------------------------
CString CAppDocumentsTreeCtrl::GetCurrentDocName(HTREEITEM* lphDBTItem /* = NULL*/) const
{
	CDocumentDescription* pDocModInfo = GetCurrentDocInfo(lphDBTItem);
	
	return pDocModInfo ? pDocModInfo->GetName() : _T("");
}

//-------------------------------------------------------------------------------
BOOL CAppDocumentsTreeCtrl::SelItemFromNamespace(const CTBNamespace& aDocNSToSel)
{
	if (!aDocNSToSel.IsValid()) return FALSE;

	HTREEITEM hAppRoot = GetRootItem();
	while (hAppRoot != NULL)
	{
		if (ItemHasChildren(hAppRoot))
		{
			HTREEITEM hModuleItem = GetChildItem(hAppRoot);

			while (hModuleItem != NULL)
			{
				if (ItemHasChildren(hModuleItem))
				{
					HTREEITEM hDocItem = GetChildItem(hModuleItem);

					while (hDocItem != NULL)
					{
						if (hDocItem && GetItemType(hDocItem) == APP_DOC_TREE_ITEM_TYPE_DOCUMENT)
						{
							CDocumentDescription* pDocInfo = (CDocumentDescription*)GetItemData(hDocItem);
							if (pDocInfo && pDocInfo->GetNamespace() == aDocNSToSel)
								return	(SelectItem(hDocItem) != 0);
						}
						hDocItem = GetNextSiblingItem(hDocItem);
					}
				}
				hModuleItem = GetNextSiblingItem(hModuleItem);
			}
		}
		hAppRoot = GetNextSiblingItem(hAppRoot);
	}
	
	return FALSE;
}


