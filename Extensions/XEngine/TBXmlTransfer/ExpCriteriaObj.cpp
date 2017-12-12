#include "stdafx.h" 
#include  <io.h>

#include <TBXMLCore\xmlgeneric.h>
#include <TBNameSolver\LoginContext.h>
#include <TbClientCore\ClientObjects.h>
#include <TbWebServicesWrappers\LoginManagerInterface.h>
#include <TBgeneric\GeneralFunctions.h>
#include <TBGES\barquery.h>
#include <TBGES\dbt.h>
#include <TBGES\XMLGesInfo.h>

#include <TBWoormEngine\ActionsRepEngin.h>
#include <TBWoormEngine\askdata.h>
#include <TBWoormEngine\inputmng.h>
#include <TBWoormEngine\prgdata.h>
#include <TBWoormEngine\askdlg.h>
#include <TBWoormEngine\reptable.h>

#include <TBOleDB\sqltable.h>

#include <XEngine\TBXMLEnvelope\XEngineObject.h>
#include <XEngine\TBXMLEnvelope\GenFunc.h>

#include "GenFunc.h"
#include "XMLTransferTags.h"
#include "XMLProfileInfo.h"
#include "ExpCriteriaObj.h"
#include "ExpCriteriaDlg.h"

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static const char THIS_FILE[] = __FILE__;
#endif

#define _QRY(s) _T(s)

static const TCHAR szNomeQuery			[] = _QRY("User custom query");
static const TCHAR szSchemaUsrCritFile	[] = _T("SchemaUsrExpCriteria.xml");

//----------------------------------------------------------------------------
//	Class CXMLExportDocSelection implementation
//----------------------------------------------------------------------------
//
CXMLExportDocSelection::CXMLExportDocSelection(CAbstractFormDoc* pDocument)
	:
	m_nProfileSelType		(USE_PREDEFINED_PROFILE),	
	m_pCurrentProfile		(NULL),
	m_bSendEnvelopeNow		(FALSE),	
	m_bExistPredefined		(FALSE),
	m_bCompressFile			(FALSE),
	m_bUseAlternativePath	(FALSE),
	m_nSchemaSelType		(EXPORT_ONLY_DOC),
	m_pDocument				(pDocument)
{	
	if (pDocument && pDocument->ValidCurrentRecord())
		m_nDocSelType = EXPORT_ONLY_CURR_DOC;
	else
		m_nDocSelType = EXPORT_DOC_SET;

	m_strAlternativePath = (AfxGetParameters()->f_ExportPath.IsEmpty())
							? GetXMLTXTargetSitePath(AfxGetSiteName(), TRUE, FALSE) 
							: AfxGetParameters()->f_ExportPath;

	//se sto effettuando il running sul server e se sto utilizzando come path la DynamicPath utilizzo la path
	//come locale e non come remota //vedi bugfix #14071
	if (	
			AfxGetPathFinder()->IsStandAlone() && 
			m_strAlternativePath.IsEqual(DataStr(AfxGetPathFinder()->TransformInRemotePath(AfxGetDynamicInstancePath())))
		)
		m_strAlternativePath = AfxGetDynamicInstancePath();

	LoadAllProfile();
}

//----------------------------------------------------------------------------
//
CXMLExportDocSelection::~CXMLExportDocSelection()
{	
	if (m_pCurrentProfile)
		delete m_pCurrentProfile;
}

//----------------------------------------------------------------------------
CXMLExportDocSelection::CXMLExportDocSelection(const CXMLExportDocSelection& aXMLExpDocSelection)
{
	Assign(&aXMLExpDocSelection);
}

//----------------------------------------------------------------------------
void CXMLExportDocSelection::LoadAllProfile()
{
	if (!m_pDocument && !m_pDocument->GetNamespace().IsValid())
		return;

	m_aProfNamesArray.RemoveAll();
	m_bExistPredefined = FALSE;
	
	m_strPreferredProfile = m_pDocument->GetXMLDocInfo()
							? m_pDocument->GetXMLDocInfo()->GetPreferredProfile()
							: _T("");
	if (!ExistProfile(m_pDocument->GetNamespace(), m_strPreferredProfile))
		m_strPreferredProfile = _T("");


	CStringArray aProfileList;
	::GetAllExportProfiles(m_pDocument->GetNamespace(), &aProfileList, CPathFinder::USERS, AfxGetLoginInfos()->m_strUserName);

	CString strProfileName;
	CPathFinder::PosType ePosType;
	for (int i = 0 ; i < aProfileList.GetSize() ; i++ )
	{
		if (IsSmartSchemaToExport() && !AfxGetBaseApp()->IsDevelopment())
		{
			//non permetto il profilo standard per la generazone del file XSD
			ePosType = AfxGetPathFinder()->GetPosTypeFromPath(aProfileList.GetAt(i));
			if (ePosType == CPathFinder::STANDARD) 
			{
				if (!m_strPreferredProfile.IsEmpty() &&	strProfileName.CompareNoCase(m_strPreferredProfile) == 0)
					m_strPreferredProfile.Empty();
				continue;  
			}
		}
		strProfileName = ::GetName(aProfileList.GetAt(i));

		// skippo il profilo preferenziale e quello predefinito
		if (
				!m_strPreferredProfile.IsEmpty() &&
				strProfileName.CompareNoCase(m_strPreferredProfile) == 0
			)
			continue;

		if (strProfileName.CompareNoCase(szPredefined) == 0)
		{
			m_bExistPredefined = TRUE;
			continue;
		}

		m_aProfNamesArray.Add(aProfileList.GetAt(i));
	}

	if (!m_strPreferredProfile.IsEmpty())
	{
		m_nProfileSelType = USE_PREFERRED_PROFILE;
		m_strProfileName = m_strPreferredProfile;
	}
	else
	{
		m_strProfileName.Clear();
		m_strPreferredProfile.Empty();
		if (m_bExistPredefined && !IsSmartSchemaToExport())
			m_nProfileSelType = USE_PREDEFINED_PROFILE;
		else if (m_aProfNamesArray.GetSize() > 0)
			m_nProfileSelType = USE_SELECTED_PROFILE;
		else
			m_nProfileSelType = USE_PREDEFINED_PROFILE;
	}	
}

//----------------------------------------------------------------------------
void CXMLExportDocSelection::Assign(const CXMLExportDocSelection* pXMLExpDocSelection)
{
	m_pDocument				= pXMLExpDocSelection ? pXMLExpDocSelection->m_pDocument : NULL;
	m_nDocSelType			= pXMLExpDocSelection ? pXMLExpDocSelection->m_nDocSelType : EXPORT_DOC_SET;
	m_nProfileSelType		= pXMLExpDocSelection ? pXMLExpDocSelection->m_nProfileSelType : USE_PREDEFINED_PROFILE;
	m_bSendEnvelopeNow		= pXMLExpDocSelection ? pXMLExpDocSelection->m_bSendEnvelopeNow : FALSE;	
	m_strProfileName		= pXMLExpDocSelection ? pXMLExpDocSelection->m_strProfileName : _T("");
	m_bExistPredefined		= pXMLExpDocSelection ? pXMLExpDocSelection->m_bExistPredefined : FALSE;
	m_strPreferredProfile	= pXMLExpDocSelection ? pXMLExpDocSelection->m_strPreferredProfile : _T("");
	m_bCompressFile 		= pXMLExpDocSelection ? pXMLExpDocSelection->m_bCompressFile : FALSE;
	m_bUseAlternativePath	= pXMLExpDocSelection ? pXMLExpDocSelection->m_bUseAlternativePath : !AfxGetParameters()->f_ExportPath.IsEmpty();
	m_strAlternativePath	= pXMLExpDocSelection ? pXMLExpDocSelection->m_strAlternativePath : AfxGetParameters()->f_ExportPath;

	m_aProfNamesArray.RemoveAll();

	if (m_pCurrentProfile)
		delete m_pCurrentProfile;
	m_pCurrentProfile = NULL;

	if (pXMLExpDocSelection)
	{
		if (pXMLExpDocSelection->m_pCurrentProfile)
			m_pCurrentProfile = new CXMLProfileInfo(*pXMLExpDocSelection->m_pCurrentProfile);
	
		for (int i = 0; i < pXMLExpDocSelection->m_aProfNamesArray.GetSize(); i++)
			m_aProfNamesArray.Add(pXMLExpDocSelection->m_aProfNamesArray.GetAt(i));		
	}
}

//----------------------------------------------------------------------------------------------
BOOL CXMLExportDocSelection::SetCurrentProfileInfo	(LPCTSTR lpszProfileName, CAutoExpressionMng* pAutoExpressionMng	/* = NULL*/)
{
	m_strProfileName = lpszProfileName;

	if (
			!m_pCurrentProfile ||
			m_pCurrentProfile->GetName().CompareNoCase(lpszProfileName) != 0
		)
	{
		if (m_pCurrentProfile)
			delete m_pCurrentProfile;

		m_pCurrentProfile = new CXMLProfileInfo(m_pDocument, lpszProfileName);		
		
	}
	return m_pCurrentProfile ? m_pCurrentProfile->LoadAllFiles(pAutoExpressionMng) : TRUE;
}

//----------------------------------------------------------------------------------------------
CXMLExportDocSelection& CXMLExportDocSelection::operator =(const CXMLExportDocSelection& aXMLExpDocSelection)
{
	if (this == &aXMLExpDocSelection)
		return *this;

	Assign(&aXMLExpDocSelection);

	return *this;
}

//----------------------------------------------------------------------------
//	Class CPreferencesCriteria implementation
//----------------------------------------------------------------------------
//
CPreferencesCriteria::CPreferencesCriteria()
:
	m_bSelModeOSL		(AfxGetLoginInfos()->m_bAuditing && AfxIsActivated(TBEXT_APP, TBAUDITING_ACT)),
	m_bSelModeApp		(TRUE), 	
	m_bSelModeUsr		(TRUE)
{	
}

//----------------------------------------------------------------------------
CPreferencesCriteria::CPreferencesCriteria(const CPreferencesCriteria& aPreferencesCriteria)
{
	Assign(&aPreferencesCriteria);
}

//----------------------------------------------------------------------------
void CPreferencesCriteria::Assign(const CPreferencesCriteria* pPreferencesCriteria)
{
	m_bSelModeOSL		= pPreferencesCriteria->m_bSelModeOSL;
	m_bSelModeApp		= pPreferencesCriteria->m_bSelModeApp;
	m_bSelModeUsr		= pPreferencesCriteria->m_bSelModeUsr;

	m_strEnvFileName	= pPreferencesCriteria->m_strEnvFileName;
}

//----------------------------------------------------------------------------------------------
CPreferencesCriteria& CPreferencesCriteria::operator =(const CPreferencesCriteria& aPreferencesCriteria)
{
	if (this == &aPreferencesCriteria)
		return *this;

	Assign(&aPreferencesCriteria);

	return *this;
}

//------------------------------------------------------------------------------
BOOL CPreferencesCriteria::operator == (const CPreferencesCriteria& aPreferencesCriteria) const
{
	if (this == &aPreferencesCriteria)
		return TRUE;
	
	return
		(
			m_bSelModeOSL			== aPreferencesCriteria.m_bSelModeOSL			&&
			m_bSelModeApp			== aPreferencesCriteria.m_bSelModeApp			&&
			m_bSelModeUsr			== aPreferencesCriteria.m_bSelModeUsr			&&
			m_strEnvFileName		== aPreferencesCriteria.m_strEnvFileName		
		);
}

//------------------------------------------------------------------------------
BOOL CPreferencesCriteria::operator != (const CPreferencesCriteria& aPreferencesCriteria) const
{
	return !(*this == aPreferencesCriteria);
}


//----------------------------------------------------------------
BOOL CPreferencesCriteria::Parse(CXMLNode* pNode) 
{
	if (!pNode)
	{
		ASSERT(FALSE);
		return FALSE;
	}

	CXMLNode* pInfoNode = NULL;
	CXMLNode* pCrNode = NULL;

	CString strTagValue;

	//sezione dei criteri da utilizzare
	if (pCrNode = pNode->GetChildByName(CRITERIA_XML_PREFERENCES_USE))
	{
		if (pCrNode)
		{
			if (pInfoNode =  pCrNode->GetChildByName(CRITERIA_XML_PREFERENCES_USE_APP))
			{
				pInfoNode->GetText(strTagValue);
				m_bSelModeApp.AssignFromXMLString(strTagValue);
			}

			if (pInfoNode =  pCrNode->GetChildByName(CRITERIA_XML_PREFERENCES_USE_OSL))
			{
				pInfoNode->GetText(strTagValue);
				m_bSelModeOSL.AssignFromXMLString(strTagValue);
			}
			
			if (pInfoNode =  pCrNode->GetChildByName(CRITERIA_XML_PREFERENCES_USE_USER))
			{
				pInfoNode->GetText(strTagValue);
				m_bSelModeUsr.AssignFromXMLString(strTagValue);
			}
		}
	}
	
	
	// EnvelopeClass
	if (pInfoNode = pNode->GetChildByName(CRITERIA_XML_ENVFILE))
	{
		pInfoNode->GetAttribute(CRITERIA_XML_ENVFILE_NAME_ATTRIBUTE, strTagValue);
		m_strEnvFileName = strTagValue;
	}

	return TRUE;
}

//----------------------------------------------------------------------------
BOOL CPreferencesCriteria::Unparse(CXMLNode* pNode)
{
	if (!pNode)
	{
		ASSERT(FALSE);
		return FALSE;
	}

	CXMLNode* pnChild = NULL;
	CXMLNode* pInfoNode = NULL;

	// creo il nodo relativo ai criteri di selezione da utilizzare
	pnChild = pNode->CreateNewChild(CRITERIA_XML_PREFERENCES_USE);
	pInfoNode = pnChild->CreateNewChild(CRITERIA_XML_PREFERENCES_USE_OSL);
	pInfoNode->SetText(FormatBoolForXML(m_bSelModeOSL));
	pInfoNode = pnChild->CreateNewChild(CRITERIA_XML_PREFERENCES_USE_APP);
	pInfoNode->SetText(FormatBoolForXML(m_bSelModeApp));
	pInfoNode = pnChild->CreateNewChild(CRITERIA_XML_PREFERENCES_USE_USER);
	pInfoNode->SetText(FormatBoolForXML(m_bSelModeUsr));

	// creo nodo relativo all'EnvelopeClass
	if (!m_strEnvFileName.IsEmpty())
	{
		pnChild = pNode->CreateNewChild(CRITERIA_XML_ENVFILE);
		pnChild->SetAttribute(CRITERIA_XML_ENVFILE_NAME_ATTRIBUTE, (LPCTSTR)m_strEnvFileName.Str());
	}
	
	return TRUE;
}

//----------------------------------------------------------------------------
//	Class CUserExportCriteria implementation
//----------------------------------------------------------------------------
//
//----------------------------------------------------------------------------
CUserExportCriteria::CUserExportCriteria(CXMLExportCriteria* pExportCriteria)
	:
	m_pQueryInfo			(NULL),
	m_pRecord				(NULL),
	m_pSqlTableInfo			(NULL),
	m_bCriteriaInit			(TRUE),
	m_bOverrideDefaultQuery	(FALSE)
{
	ASSERT(pExportCriteria);

	m_pRecord = pExportCriteria->m_pRecord;
	m_strTableName = (m_pRecord) ? m_pRecord->GetTableName() : _T("");
	m_pSqlTableInfo = m_pRecord ? m_pRecord->GetTableInfo() : NULL;

	m_pQueryInfo = new QueryInfo(AfxGetDefaultSqlConnection(), FALSE);	
}

//---------------------------------------------------------------------------------------------
CUserExportCriteria::CUserExportCriteria(const CUserExportCriteria& aUsrExpCriteria)		
:
	m_pQueryInfo	(NULL),
	m_bCriteriaInit	(TRUE)
{
	Assign(&aUsrExpCriteria);
}


//----------------------------------------------------------------------------
CUserExportCriteria::~CUserExportCriteria()
{
	SAFE_DELETE(m_pQueryInfo);
}

//----------------------------------------------------------------------------------------------
void CUserExportCriteria::Assign(const CUserExportCriteria* pUsrCriteriaSrc)
{	
	m_pRecord		= pUsrCriteriaSrc->m_pRecord;
	m_strTableName	= pUsrCriteriaSrc->m_strTableName; 
	m_pSqlTableInfo	= pUsrCriteriaSrc->m_pSqlTableInfo;
	m_OldQueryString= pUsrCriteriaSrc->m_OldQueryString;
	m_bCriteriaInit	= pUsrCriteriaSrc->m_bCriteriaInit;
	m_bOverrideDefaultQuery = pUsrCriteriaSrc->m_bOverrideDefaultQuery;

	if (m_pQueryInfo)
	{
		delete m_pQueryInfo;
		m_pQueryInfo = NULL;
	}

	SqlTableInfoArray ar; ar.Add(m_pSqlTableInfo);
	m_pQueryInfo = new QueryInfo(AfxGetDefaultSqlConnection(), ar, pUsrCriteriaSrc->GetCurrentQueryString(), m_strTableName, szNomeQuery);

	if (!pUsrCriteriaSrc->m_pQueryInfo->m_pPrgData)
		return;

	//metto i valori che andranno nei controlli.
	AskRuleData* pAskRuleDataDest = m_pQueryInfo->m_pPrgData->GetAskRuleData();
	if (!pAskRuleDataDest)
	{
		ASSERT(FALSE);
		return;
	}

	AskRuleData* pAskRuleDataSrc = pUsrCriteriaSrc->m_pQueryInfo->m_pPrgData->GetAskRuleData();
	if (!pAskRuleDataSrc)
	{
		ASSERT(FALSE);
		return;
	}

	AskDialogData* pAskDialogDataSrc = pAskRuleDataSrc->GetAskDialog(0);
	if (!pAskDialogDataSrc)
		return;

	AskDialogData* pAskDialogDataDest = pAskRuleDataDest->GetAskDialog(0);
	if (!pAskDialogDataDest)
	{
		ASSERT(FALSE);
		return;
	}

	for(int idG = 0 ; idG < pAskDialogDataSrc->GetAskGroupSize() ; idG++)
	{
		AskGroupData* pAskGroupData = pAskDialogDataSrc->GetAskGroup(idG);	
		if (!pAskGroupData)
		{
			ASSERT(FALSE);
			continue;
		}
		
		for(int idF = 0 ; idF < pAskGroupData->GetAskFieldSize() ; idF++)
		{
			AskFieldData* pAskFieldData = pAskGroupData->GetAskField(idF);
			if (!pAskFieldData)
			{
				ASSERT(FALSE);
				continue;
			}

			CString strVarName = pAskFieldData->GetPublicName();
			WoormTable* pReportSymTable = (WoormTable*)pAskDialogDataSrc->GetSymTable();
			if (!pReportSymTable)
			{
				ASSERT(FALSE);
				continue;
			}

			WoormField* pRepFieldDataSrc = (WoormField*) pReportSymTable->GetField(strVarName);
			if (!pRepFieldDataSrc)
			{
				ASSERT(FALSE);
				continue;
			}

			DataObj* pDataObjSrc =	pRepFieldDataSrc->GetData();
			if (!pDataObjSrc)
			{
				ASSERT(FALSE);
				continue;
			}

			WoormTable* pReportSymTableDest = (WoormTable*)pAskDialogDataDest->GetSymTable();
			if (!pReportSymTableDest)
			{
				ASSERT(FALSE);
				continue;
			}

			WoormField* pRepFieldDataDest = (WoormField*) pReportSymTableDest->GetField(strVarName);
			if (!pRepFieldDataDest)
			{
				ASSERT(FALSE);
				continue;
			}

			DataObj* pDataObjDest =	pRepFieldDataDest->GetData();
			if (!pDataObjDest)
			{
				ASSERT(FALSE);
				continue;
			}

			pDataObjDest->Assign(*pDataObjSrc);
		}
	}
}


//----------------------------------------------------------------------------
CString CUserExportCriteria::GetCurrentQueryString() const
{
	CString strQueryString;
	m_pQueryInfo->UnparseInString(strQueryString, m_strTableName, szNomeQuery);
	return strQueryString;
}

//----------------------------------------------------------------------------------------------
void CUserExportCriteria::AttachVariables(CXMLVariableArray* pVariablesArray)
{

	if (!pVariablesArray || !m_pQueryInfo || !m_pQueryInfo->m_pPrgData)
		return;
	
	AskRuleData* pAskRuleData = m_pQueryInfo->m_pPrgData->GetAskRuleData();
	if (!pAskRuleData)
	{
		ASSERT(FALSE);
		return;
	}

	AskDialogData* pAskDialogData = pAskRuleData->GetAskDialog(0);
	if (!pAskDialogData)
	{
		ASSERT(FALSE);
		return;
	}

	CString strVarName;
	SymTable* pSymTable = pAskDialogData->GetSymTable();
	if (!pSymTable)
		return;

	for (int nSym = 0; nSym < pSymTable->GetSize(); nSym++)
	{	
		SymField* pField = pSymTable->GetAt(nSym);

		strVarName = pField->GetName();
		if (pField->GetData())
		{
			int nIdx = pVariablesArray->GetVariable(strVarName);
			if (nIdx > -1)
				pVariablesArray->GetAt(nIdx)->SetDataObj(pField->GetData());
			else 
				pVariablesArray->Add(strVarName, pField->GetData());
		}
	}
}

//----------------------------------------------------------------------------
BOOL CUserExportCriteria::ParseExp(CXMLNode* pUserNode, CAutoExpressionMng* pAutoExpressionMng /*= NULL*/)
{
	if (!m_pQueryInfo || !pUserNode) 
		return FALSE;
	
	if (!m_pQueryInfo->m_pPrgData)
	{
		ProgramData* pPrgData = new ProgramData(NULL);
		m_pQueryInfo->SetProgramData(pPrgData);
	}

	CXMLNode* pnVariables	= NULL;
	CXMLNode* pnGroup		= NULL;
	CXMLNode* pnVariable	= NULL;

	// non ci sono variabili con valorizzazione utente
	pnVariables = pUserNode->GetChildByName(CRITERIA_XML_USER_VARIABLES);
	if (!pnVariables)
		return TRUE;
	
	AskRuleData* pAskRuleData = m_pQueryInfo->m_pPrgData->GetAskRuleData();
	if (!pAskRuleData)
	{
		ASSERT(FALSE);
		return FALSE;
	}

	AskDialogData* pAskDialogData = pAskRuleData->GetAskDialog(0);
	if (!pAskDialogData)
	{
		ASSERT(FALSE);
		return FALSE;
	}

	for(int idGroup = 0 ; idGroup < pnVariables->GetChildsNum() ; idGroup++)
	{
		AskGroupData* pAskGroupData = pAskDialogData->GetAskGroup(idGroup);
		pnGroup = pnVariables->GetChildAt(idGroup);
		if (!pnGroup || !pAskGroupData)
		{
			ASSERT(FALSE);
			continue;
		}

		for(int idVar = 0 ; idVar < pnGroup->GetChildsNum() ; idVar++)
		{
			pnVariable = pnGroup->GetChildAt(idVar);
			if (!pnVariable)
			{
				ASSERT(FALSE);
				continue;
			}

			WoormTable* pReportSymTable = (WoormTable*)pAskDialogData->GetSymTable();
			if (!pReportSymTable)
			{
				ASSERT(FALSE);
				continue;
			}

			CString strVarName;
			pnVariable->GetAttribute(CRITERIA_XML_USER_VAR_NAME_ATTRIBUTE, strVarName);

			WoormField* pRepFieldData = (WoormField*) pReportSymTable->GetField(strVarName);
			if (!pRepFieldData)
			{
				ASSERT(FALSE);
				continue;
			}

			DataObj* pDataObj =	pRepFieldData->GetData();
			if (!pDataObj)
			{
				ASSERT(FALSE);
				continue;
			}

			CString strVal;
			pnVariable->GetAttribute(XML_AUTO_EXPR_EXPR_ATTRIBUTE, strVal);
			if (pAutoExpressionMng && !strVal.IsEmpty())
			{
				CAutoExpressionData* pAutoExpressionData = new CAutoExpressionData(strVal, strVarName, pDataObj);
				if (pAutoExpressionMng->EvaluateExpression(strVal, pDataObj))
				{
					pAutoExpressionMng->Add(pAutoExpressionData);
					m_bCriteriaInit = FALSE;
				}	
			}
			else
			{	
				pnVariable->GetAttribute(CRITERIA_XML_USER_VAR_VALUE_ATTRIBUTE, strVal);
				pDataObj->AssignFromXMLString(strVal);
				if (!strVal.IsEmpty())
					m_bCriteriaInit = FALSE;
			}	
		}
	}

	return TRUE;
}



//----------------------------------------------------------------------------
BOOL CUserExportCriteria::UnparseExp(CXMLNode* pUserNode, CAutoExpressionMng* pAutoExpressionMng /*= NULL*/)
{
	if (!m_pQueryInfo || !pUserNode) 
		return FALSE;

	CXMLNode*	pnChild = NULL;
	CXMLNode*	pnVar = NULL;
	
	//se non ho modificato niente allora salvo anche le eventuali valorizzazioni effettuate 
	// in precedenza dall'utente
	if (m_pQueryInfo->m_pPrgData)
	{
		pnChild = pUserNode->CreateNewChild(CRITERIA_XML_USER_VARIABLES);
		if (!pnChild)
		{
			ASSERT(FALSE);
			return FALSE;
		}

		AskRuleData* pAskRuleData = m_pQueryInfo->m_pPrgData->GetAskRuleData();
		if (!pAskRuleData)
		{
			ASSERT(FALSE);
			return FALSE;
		}

		AskDialogData* pAskDialogData = pAskRuleData->GetAskDialog(0);
		if (!pAskDialogData)
		{
			ASSERT(FALSE);
			return FALSE;
		}

		for(int idG = 0 ; idG < pAskDialogData->GetAskGroupSize() ; idG++)
		{
			AskGroupData* pAskGroupData = pAskDialogData->GetAskGroup(idG);
			
			CXMLNode* pnGroup = pnChild->CreateNewChild(CRITERIA_XML_USER_GROUP);

			if (!pAskGroupData || !pnGroup)
			{
				ASSERT(FALSE);
				continue;
			}
			
			pnGroup->SetAttribute(CRITERIA_XML_USER_GROUP_NAME_ATTRIBUTE, pAskGroupData->GetTitle());

			for(int idF = 0 ; idF < pAskGroupData->GetAskFieldSize() ; idF++)
			{
				AskFieldData* pAskFieldData = pAskGroupData->GetAskField(idF);
				if (!pAskFieldData)
				{
					ASSERT(FALSE);
					continue;
				}

				CString strVarName = pAskFieldData->GetPublicName();

				pnVar = pnGroup->CreateNewChild(CRITERIA_XML_USER_VARIABLE);
				if (!pnVar)
				{
					ASSERT(FALSE);
					continue;
				}
				
				pnVar->SetAttribute(CRITERIA_XML_USER_VAR_NAME_ATTRIBUTE, strVarName);

				WoormTable* pReportSymTable = (WoormTable*)pAskDialogData->GetSymTable();
				if (!pReportSymTable)
				{
					ASSERT(FALSE);
					continue;
				}

				WoormField* pRepFieldData = (WoormField*) pReportSymTable->GetField(strVarName);
				if (!pRepFieldData)
				{
					ASSERT(FALSE);
					continue;
				}

				DataObj* pDataObj =	pRepFieldData->GetData();
				if (!pDataObj)
				{
					ASSERT(FALSE);
					continue;
				}

				if (pAutoExpressionMng)
				{
					CString strExpression = pAutoExpressionMng->GetExpressionByVarName(strVarName);
					
					if (!strExpression.IsEmpty())
						pnVar->SetAttribute(XML_AUTO_EXPR_EXPR_ATTRIBUTE, strExpression);
					else
						pnVar->SetAttribute(CRITERIA_XML_USER_VAR_VALUE_ATTRIBUTE, pDataObj->FormatDataForXML());
				}
				else
					pnVar->SetAttribute(CRITERIA_XML_USER_VAR_VALUE_ATTRIBUTE, pDataObj->FormatDataForXML());				
			}	
		}
	}

	return TRUE;
}

//legge il file usercriteria che contiene le informazioni della dialog di inputazione
//criteri definita nel profilo, informazione di applicazione
//----------------------------------------------------------------------------
BOOL CUserExportCriteria::ParseUsr(CXMLNode* pRoot)
{
	if (!m_pQueryInfo || !pRoot)
	{
		ASSERT(FALSE);
		return FALSE;
	}
	CXMLNode* pnString;
	CString strTagValue;
	
	pRoot->GetAttribute(CRITERIA_XML_USER_OVERRIDE_ATTRIBUTE, strTagValue);
	m_bOverrideDefaultQuery = GetBoolFromXML(strTagValue);
	if (pnString = pRoot->GetChildByName(CRITERIA_XML_USER_CRITERIA_STRING))
	{
		CString strQueryString;
		pnString->GetText(strQueryString);
		if (strQueryString.IsEmpty())
			return TRUE;

		m_OldQueryString = strQueryString;

		if (!m_pQueryInfo->ParseFromQueryString(strQueryString, m_strTableName, szNomeQuery, m_pSqlTableInfo))
			return FALSE;
	}

	return TRUE;
}

//----------------------------------------------------------------------------
BOOL CUserExportCriteria::UnparseUsr(CXMLNode* pUserNode)
{
	if (!m_pQueryInfo || !pUserNode) 
		return FALSE;

	CString		strQueryString;	
	CXMLNode*	pnChild = NULL;

	pUserNode->SetAttribute(CRITERIA_XML_USER_OVERRIDE_ATTRIBUTE, FormatBoolForXML(m_bOverrideDefaultQuery));

	pnChild = pUserNode->CreateNewChild(CRITERIA_XML_USER_CRITERIA_STRING);
	if (
		pnChild																		&&
		m_pQueryInfo->UnparseInString(strQueryString, m_strTableName, szNomeQuery)  &&
		!strQueryString.IsEmpty()
		)
		pnChild->SetText(strQueryString);
	
	return TRUE;
}

//-----------------------------------------------------------------------------
CSize CUserExportCriteria::ExecAskRules(CExportWizardPage* pExpWizPage, InputMng* pImpMng)
{
	if	(
			!pExpWizPage ||
			!pExpWizPage->GetDocument() 
		)
	{
		ASSERT(FALSE);
		return CSize(0,0);
	}

	CSize aSize;

	if	(
			m_pQueryInfo								&& 
			m_pQueryInfo->m_pPrgData					&& 
			m_pQueryInfo->m_pPrgData->GetAskRuleData()
		)
	{
		AskDialogInputMng aAskDialogInputMng
			(
				m_pQueryInfo->m_pPrgData->GetAskRuleData(),
				m_pQueryInfo->m_pPrgData->GetSymTable(),
				pExpWizPage->GetDocument()
			);
		if (!aAskDialogInputMng.LoadInputMng(pImpMng))
		{
			ASSERT(FALSE);
			return CSize(0, 0);
		}

		aSize = pImpMng->CreateAskDialog(pExpWizPage, m_bCriteriaInit);
		pImpMng->SetAskDlgEntryAutomaticExpression(pExpWizPage->GetDocument()->m_pAutoExpressionMng);
	}

	return aSize;
}	


//-----------------------------------------------------------------------------
BOOL CUserExportCriteria::PrepareQuery(SqlTable* pTable)
{
	if (!m_pQueryInfo) return TRUE;
	
	ASSERT (pTable);

	SqlTableInfoArray aTableInfoArray(m_pRecord->GetTableInfo());
	WClause aWC (AfxGetDefaultSqlConnection(), m_pQueryInfo->m_pPrgData ? m_pQueryInfo->m_pPrgData->GetSymTable() : NULL, aTableInfoArray, pTable);

	aWC.SetNative(m_pQueryInfo->m_bNativeExpr);

	Parser lex(m_pQueryInfo->m_TableInfo.m_strFilter);

	// Impostazione della Where Clause
	if (!m_pQueryInfo->m_TableInfo.m_strFilter.IsEmpty() && !aWC.Parse(lex))
	{
		TRACE0 ("CUserExportCriteria:MakeUserQuery: WClause::Parse failed\n");
		return FALSE;
	}

	if (!aWC.PrepareQuery(TRUE))
	{
		if (aWC.GetErrId())
		{
			//pTable->m_pContext->AddMessage(aWC.GetErrId());
			return FALSE;
		}				
	}

	// Impostazione delle condizioni di sort e di Select
	if (!m_pQueryInfo->m_TableInfo.m_strSort.IsEmpty())
	{
		if (!pTable->m_strSort.IsEmpty())
			pTable->m_strSort += ", ";
		pTable->m_strSort += m_pQueryInfo->m_TableInfo.m_strSort;
	}
		
	return TRUE;
}

//----------------------------------------------------------------------------------------------
CUserExportCriteria& CUserExportCriteria::operator =(const CUserExportCriteria& aUsrCriteria)
{
	if (this == &aUsrCriteria)
		return *this;
	
	Assign(&aUsrCriteria);
	
	return *this;
}

//------------------------------------------------------------------------------
BOOL CUserExportCriteria::operator == (const CUserExportCriteria& aUsrCriteria) const
{
	if (this == &aUsrCriteria)
		return TRUE;
	
	return
		(
			*m_pQueryInfo	== *m_pQueryInfo				&&
			m_pRecord		== aUsrCriteria.m_pRecord		&&
			m_strTableName	== aUsrCriteria.m_strTableName	&&
			m_pSqlTableInfo	== aUsrCriteria.m_pSqlTableInfo &&
			m_bOverrideDefaultQuery	== aUsrCriteria. m_bOverrideDefaultQuery
		);
}

//------------------------------------------------------------------------------
BOOL CUserExportCriteria::operator != (const CUserExportCriteria& aUsrCriteria) const
{
	return !(*this == aUsrCriteria);
}

//----------------------------------------------------------------------------
//	Class CAppExportCriteria implementation
//----------------------------------------------------------------------------
//
//----------------------------------------------------------------------------
CAppExportCriteria::CAppExportCriteria(CXMLExportCriteria* pExpCriteria)
	:
	m_pDocument				(NULL),
	m_pRecord				(NULL),
	m_pBaseExportCriteria	(NULL)
{
	ASSERT(pExpCriteria);
	
	m_pDocument = pExpCriteria->GetDocument();
	m_pRecord	= pExpCriteria->m_pRecord;	
	
	// se sono nel caso di solo editing del profilo e non sono in fase di esportazione
	// ne istanzio uno pulito per ricevere e salvare le info relative alle variabili eventualmente
	// memorizzate nel file xml
	if (!m_pDocument)
		m_pBaseExportCriteria = new CXMLBaseAppCriteria;
	else
		m_pBaseExportCriteria = m_pDocument->GetBaseExportCriteria();
}

//----------------------------------------------------------------------------
CAppExportCriteria::CAppExportCriteria(const CAppExportCriteria& AppCriteria)
:
	m_pDocument				(NULL),
	m_pRecord				(NULL),
	m_pBaseExportCriteria	(NULL)
{
	Assign(&AppCriteria);
}

//----------------------------------------------------------------------------
CAppExportCriteria::~CAppExportCriteria()
{
	if (!m_pDocument && m_pBaseExportCriteria)
		delete m_pBaseExportCriteria;
}

//----------------------------------------------------------------------------
void CAppExportCriteria::Assign(const CAppExportCriteria* pAppCriteria)
{
	m_pRecord	= pAppCriteria->m_pRecord;			
	
	if (!m_pDocument && m_pBaseExportCriteria)
		delete m_pBaseExportCriteria;
	m_pBaseExportCriteria = NULL;

	m_pDocument	= pAppCriteria->m_pDocument;			

	if (!m_pDocument)
	{
		m_pBaseExportCriteria = new CXMLBaseAppCriteria;
		*m_pBaseExportCriteria = *pAppCriteria->m_pBaseExportCriteria;
	}
	else
		m_pBaseExportCriteria = m_pDocument->GetBaseExportCriteria();
}

//----------------------------------------------------------------------------
void CAppExportCriteria::DefineQuery(SqlTable* pTable)
{
	// define query cablata programmativamente			
	if (m_pBaseExportCriteria)
		m_pBaseExportCriteria->OnDefineXMLExportQuery(pTable);	
}

//----------------------------------------------------------------------------
void CAppExportCriteria::PrepareQuery(SqlTable* pTable)
{
	// prepare query cablata programmativamente
	if (m_pBaseExportCriteria)
		m_pBaseExportCriteria->OnPrepareXMLExportQuery(pTable);	
}
//----------------------------------------------------------------------------------------------
BOOL CAppExportCriteria::Parse(CXMLNode* pXMLNode, CAutoExpressionMng* pAutoExpressionMng /*= NULL*/) 
{ 
	return (m_pBaseExportCriteria) ? m_pBaseExportCriteria->Parse(pXMLNode, pAutoExpressionMng) : TRUE;	
}

//----------------------------------------------------------------------------------------------
BOOL CAppExportCriteria::Unparse(CXMLNode* pXMLNode, CAutoExpressionMng* pAutoExpressionMng /*= NULL*/) 
{
	return (m_pBaseExportCriteria) ? m_pBaseExportCriteria->Unparse(pXMLNode, pAutoExpressionMng) : TRUE;	
}

//----------------------------------------------------------------------------------------------
CAppExportCriteria& CAppExportCriteria::operator =(const CAppExportCriteria& aAppCriteria)
{
	if (this == &aAppCriteria)
		return *this;
	
	Assign(&aAppCriteria);
	
	return *this;
}

//------------------------------------------------------------------------------
BOOL CAppExportCriteria::operator == (const CAppExportCriteria& aAppCriteria) const
{
	if (this == &aAppCriteria)
		return TRUE;
	
	return
		(
			m_pDocument				== aAppCriteria.m_pDocument	&&
			m_pRecord				== aAppCriteria.m_pRecord	&&
			m_pBaseExportCriteria	== aAppCriteria.m_pBaseExportCriteria
		);
}

//------------------------------------------------------------------------------
BOOL CAppExportCriteria::operator != (const CAppExportCriteria& aAppCriteria) const
{
	return !(*this == aAppCriteria);
}

//----------------------------------------------------------------------------
//	Class COSLExportCriteria implementation
//----------------------------------------------------------------------------
//
//----------------------------------------------------------------------------
COSLExportCriteria::COSLExportCriteria()
:
	m_FromDate		(AfxGetApplicationDate()),
	m_ToDate		(AfxGetApplicationDate()),
	m_bInserted		(TRUE),
	m_bUpdated		(TRUE),
	m_bDeleted		(FALSE),
	m_pCatalogEntry	(NULL)
{	
	m_FromDate.SetFullDate();
	m_ToDate.SetFullDate();

	m_FromDate.SetTime(0,0,0);
	m_ToDate.SetTime(23,59,59);
}	

//----------------------------------------------------------------------------
COSLExportCriteria::~COSLExportCriteria()
{
}

//----------------------------------------------------------------------------
COSLExportCriteria::COSLExportCriteria(const COSLExportCriteria& aOSLCriteria)
{
	m_FromDate.SetFullDate();
	m_ToDate.SetFullDate();
	Assign(&aOSLCriteria);
}

//----------------------------------------------------------------------------
void COSLExportCriteria::Assign(const COSLExportCriteria* pOSLCriteria)
{
	m_FromDate  = pOSLCriteria->m_FromDate;
	m_ToDate	= pOSLCriteria->m_ToDate;	
	m_bInserted	= pOSLCriteria->m_bInserted;	
	m_bUpdated	= pOSLCriteria->m_bUpdated;	
	m_bDeleted	= pOSLCriteria->m_bDeleted;	
	m_pCatalogEntry = pOSLCriteria->m_pCatalogEntry;	
}


//----------------------------------------------------------------------------------------------
BOOL COSLExportCriteria::IsUpdateInsertRecordsRequest() const
{
	return m_bUpdated || m_bInserted;
}
//----------------------------------------------------------------------------------------------
void COSLExportCriteria::PrepareQuery(SqlTable* pTable)
{
	int eOperationType = 0;
	if (m_bUpdated)
		eOperationType = AUDIT_UPDATE_OP;
	if (m_bInserted)
		eOperationType |= AUDIT_INSERT_OP;

	m_pCatalogEntry->PrepareQuery(pTable, m_FromDate, m_ToDate, eOperationType);
}

//----------------------------------------------------------------------------------------------
void COSLExportCriteria::PrepareDeletedQuery(SqlTable* pTable, DBTMaster* pDBTMaster)
{
	if (!pTable || !pDBTMaster)
		return;

	m_pCatalogEntry->PrepareDeletedQuery(pTable, pDBTMaster, m_FromDate, m_ToDate);
}

//----------------------------------------------------------------------------------------------
void COSLExportCriteria::AttachVariables(CXMLVariableArray* pVariablesArray)
{
	if (!pVariablesArray)
	{
		ASSERT(FALSE);
		return;
	}

	for(int i = 0 ; i < pVariablesArray->GetSize() ; i++)
	{
		if (pVariablesArray->GetAt(i)->GetName().CompareNoCase(CRITERIA_XML_OSLTRACE_STARTDATE) == 0)
		{
			pVariablesArray->GetAt(i)->SetDataObj(&m_FromDate);
			return;
		}

		if (pVariablesArray->GetAt(i)->GetName().CompareNoCase(CRITERIA_XML_OSLTRACE_ENDDATE) == 0)
		{
			pVariablesArray->GetAt(i)->SetDataObj(&m_ToDate);
			return;
		}
	}

	//se arrivo qui vuol dire che non era stata ancora aggiunta la variabile
	CXMLVariable* pVar = new CXMLVariable(CRITERIA_XML_OSLTRACE_STARTDATE, &m_FromDate);
	pVariablesArray->Add(pVar);
	pVar = new CXMLVariable(CRITERIA_XML_OSLTRACE_ENDDATE, &m_ToDate);
	pVariablesArray->Add(pVar);
}


// se previsto devo fare un filtraggio sui campi chiave
// vedi espoertazione dei clienti/fornitori cancellati
// se non imposto il filtro sul tipo cliente/fornitore mi vengono esportati entrambi
// il valore del filtraggio mi viene data dal metodo OnPrepareForXImportExport

//----------------------------------------------------------------------------------------------
BOOL COSLExportCriteria::SetCriteria(CAbstractFormDoc* pDoc, BOOL bForDeleted)
{
	if (bForDeleted && !m_bDeleted)
		return FALSE;

	if (pDoc && pDoc->m_pDBTMaster)
	{
		m_FromDate.SetTime(0,0,0);
		m_ToDate.SetTime(23,59,59);
		m_pCatalogEntry = pDoc->GetSqlConnection()->GetCatalogEntry(pDoc->m_pDBTMaster->GetTable()->GetTableName());				
	}
	else
	{
		TRACE("COSLExportCriteria::SetCriteria: it's impossible to use the Auditing manager, document or DBTMaster NULL");
		return FALSE;
	}

	return TRUE;
}

//----------------------------------------------------------------
BOOL COSLExportCriteria::Parse(CXMLNode* pNode, CAutoExpressionMng* pAutoExpressionMng /*= NULL*/) 
{
	if (!pNode)
	{
		ASSERT(FALSE);
		return FALSE;
	}

	if (pAutoExpressionMng)
		pAutoExpressionMng->RemoveAll();


	CXMLNode* pInfoNode = NULL;

	CString strTagValue;

	if (pInfoNode = pNode->GetChildByName(CRITERIA_XML_OSLTRACE_STARTDATE))
	{
		pInfoNode->GetText(strTagValue);
		m_FromDate.AssignFromXMLString(strTagValue);

		if (pAutoExpressionMng)
		{
			CString strExpression;
			pInfoNode->GetAttribute(XML_AUTO_EXPR_EXPR_ATTRIBUTE, strExpression);
			if (!strExpression.IsEmpty())
			{
				CAutoExpressionData* pAutoExpressionData = new CAutoExpressionData(strExpression, CRITERIA_XML_OSLTRACE_STARTDATE, &m_FromDate);
				if (!pAutoExpressionMng->EvaluateExpression(strExpression, &m_FromDate))
				{
					ASSERT(FALSE);
				}
			
				pAutoExpressionMng->Add(pAutoExpressionData);
			}
		}

	}

	if (pInfoNode = pNode->GetChildByName(CRITERIA_XML_OSLTRACE_ENDDATE))
	{
		pInfoNode->GetText(strTagValue);
		m_ToDate.AssignFromXMLString(strTagValue);

		if (pAutoExpressionMng)
		{
			CString strExpression;
			pInfoNode->GetAttribute(XML_AUTO_EXPR_EXPR_ATTRIBUTE, strExpression);
			if (!strExpression.IsEmpty())
			{
				CAutoExpressionData* pAutoExpressionData = new CAutoExpressionData(strExpression, CRITERIA_XML_OSLTRACE_ENDDATE, &m_ToDate);
				if (!pAutoExpressionMng->EvaluateExpression(strExpression, &m_ToDate))
				{
					ASSERT(FALSE);
				}

				pAutoExpressionMng->Add(pAutoExpressionData);
			}
		}
	}

	if (pInfoNode = pNode->GetChildByName(CRITERIA_XML_OSLTRACE_UPDATED))
	{
		pInfoNode->GetText(strTagValue);
		m_bUpdated.AssignFromXMLString(strTagValue);
	}

	if (pInfoNode = pNode->GetChildByName(CRITERIA_XML_OSLTRACE_UPDATED))
	{
		pInfoNode->GetText(strTagValue);
		m_bUpdated.AssignFromXMLString(strTagValue);
	}

	if (pInfoNode = pNode->GetChildByName(CRITERIA_XML_OSLTRACE_INSERED))
	{
		pInfoNode->GetText(strTagValue);
		m_bInserted.AssignFromXMLString(strTagValue);
	}
	
	if (pInfoNode = pNode->GetChildByName(CRITERIA_XML_OSLTRACE_DELETED))
	{
		pInfoNode->GetText(strTagValue);
		m_bDeleted.AssignFromXMLString(strTagValue);
	}
	
	return TRUE;
}

//----------------------------------------------------------------------------
BOOL COSLExportCriteria::Unparse(CXMLNode* pNode, CAutoExpressionMng* pAutoExpressionMng /*= NULL*/)
{
	if (!pNode)
	{
		ASSERT(FALSE);
		return FALSE;
	}

	CXMLNode* pInfoNode = NULL;
	
	pInfoNode = pNode->CreateNewChild(CRITERIA_XML_OSLTRACE_STARTDATE);
	pInfoNode->SetText(m_FromDate.FormatDataForXML());
	if (pAutoExpressionMng)
	{
		CString strExpression = pAutoExpressionMng->GetExpressionByVarName(CRITERIA_XML_OSLTRACE_STARTDATE);
		
		if (!strExpression.IsEmpty())
			pInfoNode->SetAttribute(XML_AUTO_EXPR_EXPR_ATTRIBUTE, strExpression);
	}


	pInfoNode = pNode->CreateNewChild(CRITERIA_XML_OSLTRACE_ENDDATE);
	pInfoNode->SetText(m_ToDate.FormatDataForXML());
	if (pAutoExpressionMng)
	{
		CString strExpression = pAutoExpressionMng->GetExpressionByVarName(CRITERIA_XML_OSLTRACE_ENDDATE);
		
		if (!strExpression.IsEmpty())
			pInfoNode->SetAttribute(XML_AUTO_EXPR_EXPR_ATTRIBUTE, strExpression);
	}

	
	pInfoNode = pNode->CreateNewChild(CRITERIA_XML_OSLTRACE_UPDATED);
	pInfoNode->SetText(m_bUpdated.FormatDataForXML());
	pInfoNode = pNode->CreateNewChild(CRITERIA_XML_OSLTRACE_INSERED);
	pInfoNode->SetText(m_bInserted.FormatDataForXML());
	pInfoNode = pNode->CreateNewChild(CRITERIA_XML_OSLTRACE_DELETED);
	pInfoNode->SetText(m_bDeleted.FormatDataForXML());

	return TRUE;
}

//----------------------------------------------------------------------------------------------
COSLExportCriteria& COSLExportCriteria::operator =(const COSLExportCriteria& aOSLCriteria)
{
	if (this == &aOSLCriteria)
		return *this;
	
	Assign(&aOSLCriteria);

	return *this;
}

//------------------------------------------------------------------------------
BOOL COSLExportCriteria::operator == (const COSLExportCriteria& aOSLCriteria) const
{
	if (this == &aOSLCriteria)
		return TRUE;
	
	return
		(
			m_FromDate		== aOSLCriteria.m_FromDate		&&
			m_ToDate		== aOSLCriteria.m_ToDate		&&
			m_bInserted		== aOSLCriteria.m_bInserted		&&
			m_bUpdated		== aOSLCriteria.m_bUpdated		&&
			m_bDeleted		== aOSLCriteria.m_bDeleted		&&
			m_pCatalogEntry	== aOSLCriteria.m_pCatalogEntry	
		);
}

//------------------------------------------------------------------------------
BOOL COSLExportCriteria::operator != (const COSLExportCriteria& aOSLCriteria) const
{
	return !(*this == aOSLCriteria);
}

//----------------------------------------------------------------------------
//	Class CXMLExportCriteria definition
//----------------------------------------------------------------------------
//
// il secondo parametro viene utilizzato solo nell'effettiva fase di esportazione
//----------------------------------------------------------------------------
CXMLExportCriteria::CXMLExportCriteria(CXMLProfileInfo* pProfile, CAbstractFormDoc* pDoc /*=NULL*/)
	:
	m_pDoc					(pDoc),
	m_pProfile				(pProfile),
	m_pPreferencesCriteria	(NULL),
	m_pAppExportCriteria	(NULL),
	m_pOSLExportCriteria	(NULL),
	m_pUserExportCriteria	(NULL),
	m_pTable				(NULL),
	m_pRecord				(NULL)
{
	ASSERT(m_pProfile);

	if (m_pProfile)
		m_strProfileName = m_pProfile->GetName();

	// sono nel caso di esportazione
if (m_pDoc)
	{
		if ( m_pDoc->m_pDBTMaster)
		{
			m_pRecord	= m_pDoc->m_pDBTMaster->GetRecord();
			m_pTable	= new SqlTable(m_pRecord, m_pDoc->GetReadOnlySqlSession());
			m_bOwnSqlRec = FALSE;
			m_pPreferencesCriteria	= new CPreferencesCriteria();
			m_pAppExportCriteria	= new CAppExportCriteria(this);
			m_pOSLExportCriteria	= new COSLExportCriteria;
			m_pUserExportCriteria	= new CUserExportCriteria(this);
		}
	}
	else
	{
		// sto modificando i criteri personalizzati dall'editor dei profili
		CXMLDBTInfo* pDBTInfo = NULL;
		if (m_pProfile && (pDBTInfo = m_pProfile->GetDBTMaster()))
		{			
			ASSERT(!pDBTInfo->GetTableName().IsEmpty());
			m_pRecord = AfxCreateRecord(pDBTInfo->GetTableName());
			if (m_pRecord)
				m_pTable = new SqlTable(m_pRecord, AfxGetDefaultSqlSession());
		}
		m_bOwnSqlRec = TRUE;
	}
}

//----------------------------------------------------------------------------
CXMLExportCriteria::CXMLExportCriteria(const CXMLExportCriteria& aXMLExpCriteria)
	:
	m_pPreferencesCriteria	(NULL),
	m_pAppExportCriteria	(NULL),
	m_pOSLExportCriteria	(NULL),
	m_pUserExportCriteria	(NULL),
	m_pTable				(NULL),
	m_pRecord				(NULL)
{
	Assign (&aXMLExpCriteria);
}

//----------------------------------------------------------------------------
CXMLExportCriteria::~CXMLExportCriteria()
{
	SAFE_DELETE(m_pPreferencesCriteria);
	SAFE_DELETE(m_pAppExportCriteria);
	SAFE_DELETE(m_pOSLExportCriteria);	
	SAFE_DELETE(m_pUserExportCriteria);

	if (m_pTable)
	{
		if (m_pTable->IsOpen()) m_pTable->Close();
		delete m_pTable;
	}

	if (m_bOwnSqlRec && m_pRecord)
		delete m_pRecord;
}


//----------------------------------------------------------------------------------------------
void CXMLExportCriteria::Assign(const CXMLExportCriteria* pXMLExpCriteria)
{	
	if (this == pXMLExpCriteria)
		return;
	
	m_pDoc				= pXMLExpCriteria->m_pDoc;
	m_pProfile			= pXMLExpCriteria->m_pProfile;
	m_strProfileName	= pXMLExpCriteria->m_strProfileName ;

	if (m_bOwnSqlRec && m_pRecord)
		delete m_pRecord;
	
	m_pTable	 = NULL;
	m_bOwnSqlRec = pXMLExpCriteria->m_bOwnSqlRec;
	if (m_bOwnSqlRec && pXMLExpCriteria->m_pRecord)
		m_pRecord = pXMLExpCriteria->m_pRecord->Create();
	else 
		m_pRecord = pXMLExpCriteria->m_pRecord;


	if (m_pRecord)
	{
		m_pTable = new SqlTable(m_pRecord, AfxGetDefaultSqlSession());
		m_pTable->Open();
	}
	
	m_pAppExportCriteria	= NULL;
	m_pOSLExportCriteria	= NULL;
	m_pUserExportCriteria	= NULL;
	m_pPreferencesCriteria	= NULL;
	
	if (pXMLExpCriteria->m_pPreferencesCriteria)
		m_pPreferencesCriteria = new CPreferencesCriteria(*pXMLExpCriteria->m_pPreferencesCriteria); 	
	if (pXMLExpCriteria->m_pAppExportCriteria)
		m_pAppExportCriteria   = new CAppExportCriteria(*pXMLExpCriteria->m_pAppExportCriteria); 
	if (pXMLExpCriteria->m_pOSLExportCriteria)
		m_pOSLExportCriteria   = new COSLExportCriteria(*pXMLExpCriteria->m_pOSLExportCriteria); 
	if (pXMLExpCriteria->m_pUserExportCriteria)
		m_pUserExportCriteria  = new CUserExportCriteria(*pXMLExpCriteria->m_pUserExportCriteria); 
}

//----------------------------------------------------------------------------------------------
void CXMLExportCriteria::SetExternalRecord(SqlRecord* pRec)
{
	if (m_pTable)
	{
		if (m_pTable->IsOpen())
			m_pTable->Close();
		delete m_pTable;
	}

	m_pRecord = pRec;
	m_pTable = new SqlTable(m_pRecord, AfxGetDefaultSqlSession());
	m_bOwnSqlRec = FALSE;
}

//----------------------------------------------------------------------------
int CXMLExportCriteria::ExportQuery(CString& strErrorMsg)
{
	if (!m_pTable || !m_pDoc)
		return EXTRACT_RECORD_ERROR;

	if (m_pTable->IsOpen()) 
		m_pTable->Close();
		
	m_pTable->Open();
	if (m_pRecord)
		m_pRecord->Init();	

	if (
			!GetPreferencesCriteria()->IsCriteriaModeAppOrUser() &&
			GetPreferencesCriteria()->IsCriteriaModeOSL() &&	
			!m_pOSLExportCriteria->IsUpdateInsertRecordsRequest()
		)
		return EXTRACT_RECORD_NO_DATA;
		

	if (GetPreferencesCriteria()->IsCriteriaModeOSL() && m_pOSLExportCriteria->IsUpdateInsertRecordsRequest())	
	{
		if (! (AfxIsActivated(TBEXT_APP, TBAUDITING_ACT) && m_pOSLExportCriteria->SetCriteria(m_pDoc)))
			return EXTRACT_RECORD_ERROR;
	}


	MakeExportQuery();
	TRY
	{
		m_pTable->Query();	
		return m_pTable->IsEmpty() ? EXTRACT_RECORD_NO_DATA : EXTRACT_RECORD_SUCCEEDED;
	}
	
	CATCH(SqlException, e)
	{
		strErrorMsg = cwsprintf(_T("{0-%s}. Export criteria query: {1-%s}"), e->m_strError, m_pTable->m_strSQL);
		return EXTRACT_RECORD_ERROR;
	}
	END_CATCH

}

//----------------------------------------------------------------------------
int CXMLExportCriteria::DeletedQuery()
{
	if (!m_pTable || !m_pDoc)
		return EXTRACT_RECORD_ERROR;

	if (m_pTable->IsOpen()) 
		m_pTable->Close();
		
	m_pTable->Open();
	if (m_pRecord)
		m_pRecord->Init();

    
	if (!m_pOSLExportCriteria->IsDeletedRecordsRequest())
		return EXTRACT_RECORD_NO_DATA;

	if (
			!( AfxIsActivated(TBEXT_APP, TBAUDITING_ACT) &&
				m_pOSLExportCriteria->SetCriteria(m_pDoc, TRUE)
			 )
		)
		return EXTRACT_RECORD_ERROR;

	m_pOSLExportCriteria->PrepareDeletedQuery(m_pTable, m_pDoc->m_pDBTMaster);
	TRY
	{
		m_pTable->Query();	
		return m_pTable->IsEmpty() ? EXTRACT_RECORD_NO_DATA : EXTRACT_RECORD_SUCCEEDED;
	}
	
	CATCH(SqlException, e)
	{
		return EXTRACT_RECORD_ERROR;
	}
	END_CATCH	
}

//----------------------------------------------------------------------------
void CXMLExportCriteria::Select()
{
	m_pTable->ClearColumns();

	for (int i = 0; i <= m_pRecord->GetUpperBound(); i++)
		if (m_pRecord->IsSpecial(i))
			m_pTable->Select(m_pRecord->GetDataObjAt(i));
}

//utilizza la query di findable del documento
//----------------------------------------------------------------------------
int CXMLExportCriteria::MakeFindableExportQuery(CString& strErrorMsg)
{
	if (m_pTable->IsOpen()) 
		m_pTable->Close();
		
	m_pTable->Open();
	if (m_pRecord)
		m_pRecord->Init();

	TRY
	{
		if (m_pDoc)
			m_pDoc->PrepareFindQuery(m_pTable, TRUE);

		m_pTable->ClearColumns();		
		CString strParam;
		CString strColName;
		for (int i = 0; i <= m_pRecord->GetUpperBound(); i++)
		{
			SqlRecordItem* pRecItem = m_pRecord->GetAt(i);
			if (pRecItem->IsSpecial())
			{
				DataObj* pDataObj = pRecItem->GetDataObj();
				m_pTable->Select(pDataObj);

				strColName = m_pTable->GetQualifiedColumnName(pDataObj);			
				if (m_pTable->m_strFilter.Find(strColName) == -1 && pDataObj->IsValueLocked())
				{
					strParam = cwsprintf(_T("bauzi%d"), i);
	
					// Sulle stringe commuta automaticamente in like     	
	    			if (pDataObj->GetDataType() == DATA_STR_TYPE && !pDataObj->IsEmpty())
	    			{
						m_pTable->AddFilterLike	(*pDataObj);
						m_pTable->AddParam		(strParam, *pDataObj);
						m_pTable->SetParamLike	(strParam, *pDataObj);
					}
					else
					{
						m_pTable->AddFilterColumn	(*pDataObj);
						m_pTable->AddParam		(strParam, *pDataObj);
						m_pTable->SetParamValue	(strParam, *pDataObj);
					}
				}
			}
		}

		//effettuo l'unlock dei campi lockati.
		// il lock è necessario per costruire la where clause nella query di find (VEDI METODO CAbstractFormDoc::PrepareFindQuery)
		// devo fare l'unlock altrimenti il primo record estratto non viene bindato per i campi lockati
		for (int i = 0; i < m_pRecord->GetSize (); i++)
			m_pRecord->GetDataObjAt (i)->SetValueLocked (FALSE);

		m_pTable->Query();	
		return m_pTable->IsEmpty() ? EXTRACT_RECORD_NO_DATA : EXTRACT_RECORD_SUCCEEDED;
	}
	
	CATCH(SqlException, e)
	{
		strErrorMsg = cwsprintf(_T("{0-%s}. Export criteria query: {1-%s}"), e->m_strError, m_pTable->m_strSQL);
		return EXTRACT_RECORD_ERROR;
	}
	END_CATCH
}

//----------------------------------------------------------------------------
void CXMLExportCriteria::MakeExportQuery()
{
	if (GetPreferencesCriteria()->IsCriteriaModeOSL())
	{
		m_pRecord->SetQualifier(_T("APP"));	
		m_pTable->AddSelectKeyword(SqlTable::DISTINCT);
		m_pTable->FromTable(m_pDoc->m_pDBTMaster->GetRecord()->GetTableName(), _T("APP"));
	}		
	if (
			m_pDoc && m_pDoc->m_pDBTMaster &&
			(!m_pUserExportCriteria || !m_pUserExportCriteria->m_bOverrideDefaultQuery)
		)
		m_pDoc->m_pDBTMaster->OnPrepareForXImportExport(m_pTable);

	m_pTable->ClearColumns();
	
	for (int i = 0; i <= m_pRecord->GetUpperBound(); i++)
		if (m_pRecord->IsSpecial(i))
			m_pTable->Select(m_pRecord->GetDataObjAt(i));
	
	if (GetPreferencesCriteria()->IsCriteriaModeApp())
	{
		m_pAppExportCriteria->DefineQuery(m_pTable);
		m_pAppExportCriteria->PrepareQuery(m_pTable);
	}
		
	if (GetPreferencesCriteria()->IsCriteriaModeUser())
		m_pUserExportCriteria->PrepareQuery(m_pTable);

	if (GetPreferencesCriteria()->IsCriteriaModeOSL())
	{
		m_pOSLExportCriteria->PrepareQuery(m_pTable); 
		m_pRecord->SetQualifier(_T(""));
	}
}

//----------------------------------------------------------------------------------------------
int CXMLExportCriteria::GetNextRecord()
{
	if (m_pTable->IsEmpty() || m_pTable->IsEOF()) 
		return EXTRACT_RECORD_NO_DATA;

	m_pTable->MoveNext();
	
	return m_pTable->IsEOF() ? EXTRACT_RECORD_NO_DATA : EXTRACT_RECORD_SUCCEEDED;
}


//----------------------------------------------------------------------------
void CXMLExportCriteria::SetUserCriteria(CUserExportCriteria* pUsrCriteria)
{
	if (!pUsrCriteria) 
	{
		SAFE_DELETE(m_pUserExportCriteria);
		return;
	}

	if (m_pUserExportCriteria)
		*m_pUserExportCriteria = *pUsrCriteria;
	else
		m_pUserExportCriteria = new CUserExportCriteria(*pUsrCriteria);	
}

//----------------------------------------------------------------------------
BOOL CXMLExportCriteria::ParseExpCriteriaFile(const CString& strExpFileName, CString strSiteName /*=""*/, CAutoExpressionMng* pAutoExpressionMng	/* = NULL*/)
{
	if (strExpFileName.IsEmpty())
		return FALSE;

	if (strSiteName.IsEmpty())
		strSiteName = AfxGetSiteName();

	CXMLDocumentObject aXMLCriteriaDoc;
	aXMLCriteriaDoc.EnableMsgMode(FALSE);// da togliere un domani quando ci dovranno
											// essere tutte le descizioni dei documenti
	
	if (!aXMLCriteriaDoc.LoadXMLFile(strExpFileName))  
		return FALSE;

	CXMLNode* pRoot = aXMLCriteriaDoc.GetRoot();
	if (!pRoot) 
		return FALSE;
	
	CXMLNode* pInfoNode = NULL;
	CXMLNode* pSiteNode = NULL;
	CString strTagValue;

	//cerco il nodo relativo al site corrente
	for(int i = 0 ; i < pRoot->GetChildsNum() ; i++)
	{
		pSiteNode = pRoot->GetChildAt(i);
		if (!pSiteNode)
			continue;
		
		CString strTmp;
		pSiteNode->GetAttribute(DOC_XML_NAME_ATTRIBUTE, strTmp);

		if (strTmp.CompareNoCase(strSiteName) == 0)
			break;

		pSiteNode = NULL;
	}

	if (!pSiteNode)
		return TRUE;

	if (pInfoNode = pSiteNode->GetChildByName(CRITERIA_XML_PREFERENCES_CRITERIA))
	{
		if (!m_pPreferencesCriteria	)
			m_pPreferencesCriteria	= new CPreferencesCriteria();
		if (!m_pPreferencesCriteria->Parse(pInfoNode))
			return FALSE;
	}

	if (pInfoNode = pSiteNode->GetChildByName(CRITERIA_XML_PREDEFINED_CRITERIA))
	{
		if (!m_pAppExportCriteria)		
			m_pAppExportCriteria	= new CAppExportCriteria(this);
		if (!m_pAppExportCriteria->Parse(pInfoNode, pAutoExpressionMng))
			return FALSE;
	}

	if (pInfoNode = pSiteNode->GetChildByName(CRITERIA_XML_OSLTRACE_CRITERIA))
	{

		if (!m_pOSLExportCriteria)		
			m_pOSLExportCriteria = new COSLExportCriteria;
		if (!m_pOSLExportCriteria->Parse(pInfoNode, pAutoExpressionMng))
			return FALSE;
	}

	if (pInfoNode = pSiteNode->GetChildByName(CRITERIA_XML_USER_CRITERIA_RULES))
	{
		if (!m_pUserExportCriteria)		
			m_pUserExportCriteria = new CUserExportCriteria(this);
		if (!m_pUserExportCriteria->ParseExp(pInfoNode, pAutoExpressionMng))
			return FALSE;
	}
	
	return TRUE;
}

//----------------------------------------------------------------------------
BOOL CXMLExportCriteria::UnparseExpCriteriaFile(const CString& strExpFileName, CString strSiteName /*=""*/, CAutoExpressionMng* pAutoExpressionMng	/* = NULL*/)
{
	if (strExpFileName.IsEmpty())
		return FALSE;


	if (strSiteName.IsEmpty())
		strSiteName = AfxGetSiteName();

	
	CXMLDocumentObject aXMLCriteriaDoc(FALSE);
	
	CXMLNode* pnRoot = NULL;
	CXMLNode* pnSiteNode = NULL;

	//controllo se esiste il nodo rooth e quello relativo al sito se non c'e'
	//la rooth creo tutto se c'e' e manca il nodo site lo creo
	//se c'e' la rooth e il nodo site lo distruggo e lo ricreo
	if (ExistFile(strExpFileName))
	{
		if (!aXMLCriteriaDoc.LoadXMLFile(strExpFileName))  
		return FALSE;

		pnRoot = aXMLCriteriaDoc.GetRoot();
		if (pnRoot) 
		{
			//cerco il nodo relativo al site corrente
			for(int i = 0 ; i < pnRoot->GetChildsNum() ; i++)
			{
				pnSiteNode = pnRoot->GetChildAt(i);
				if (!pnSiteNode)
					continue;
				
				CString strTmp;
				pnSiteNode->GetAttribute(DOC_XML_NAME_ATTRIBUTE, strTmp);

				if (strTmp.CompareNoCase(strSiteName) == 0)
				{
					pnRoot->RemoveChildAt(i);
					pnSiteNode = NULL;
					break;
				}

				pnSiteNode = NULL;
			}
		}
	}

	if (!pnRoot)
	{
		AfxInitWithXEngineEncoding(aXMLCriteriaDoc);
		pnRoot = aXMLCriteriaDoc.CreateRoot(CRITERIA_XML_SELECTIONS);
	}

	if (!pnSiteNode)
	{
		pnSiteNode = pnRoot->CreateNewChild(DOC_XML_SITE_TAG);
		pnSiteNode->SetAttribute(DOC_XML_NAME_ATTRIBUTE, strSiteName);
	}

	CXMLNode* pnChild = NULL;
	// creo il nodo relativo alle preferences
	if (m_pPreferencesCriteria)
	{
		pnChild = pnSiteNode->CreateNewChild(CRITERIA_XML_PREFERENCES_CRITERIA);
		if (!m_pPreferencesCriteria->Unparse(pnChild))
			return FALSE;
	}

	// creo il nodo relativo alle selezioni inserite dal programmatore
	if (m_pPreferencesCriteria && m_pPreferencesCriteria->IsCriteriaModeApp() && m_pAppExportCriteria)
	{
		pnChild  = pnSiteNode->CreateNewChild(CRITERIA_XML_PREDEFINED_CRITERIA);
		if (!m_pAppExportCriteria->Unparse(pnChild, pAutoExpressionMng))
			return FALSE;
	}

	// creo il nodo relativo alle selezioni per l'OSL tracer
	if (m_pPreferencesCriteria && m_pPreferencesCriteria->IsCriteriaModeOSL() && m_pOSLExportCriteria)
	{
		pnChild  = pnSiteNode->CreateNewChild(CRITERIA_XML_OSLTRACE_CRITERIA);
		if (!m_pOSLExportCriteria->Unparse(pnChild, pAutoExpressionMng))
			return FALSE;
	}
	
	// creo il nodo relativo alle selezioni personalizzate
	if (m_pPreferencesCriteria && m_pPreferencesCriteria->IsCriteriaModeUser() && m_pUserExportCriteria)
	{
		pnChild  = pnSiteNode->CreateNewChild(CRITERIA_XML_USER_CRITERIA_RULES);

		// se m_pDoc è NULL vuol dire che sto editando gli usercriteria dai profili			
		if (!m_pUserExportCriteria->UnparseExp(pnChild, pAutoExpressionMng))
			return FALSE;
	}
	
	aXMLCriteriaDoc.SaveXMLFile(strExpFileName, TRUE);
	return TRUE;
}

//----------------------------------------------------------------------------
BOOL CXMLExportCriteria::ParseUsrCriteriaFile(const CString& strUsrFileName)
{
	if (strUsrFileName.IsEmpty())
		return FALSE;

	CXMLDocumentObject aXMLUsrCriteriaDoc;
	aXMLUsrCriteriaDoc.EnableMsgMode(FALSE);// da togliere un domani quando ci dovranno
											// essere tutte le descizioni dei documenti
	
	if (!aXMLUsrCriteriaDoc.LoadXMLFile(strUsrFileName))  
		return FALSE;

	CXMLNode* pRoot = aXMLUsrCriteriaDoc.GetRoot();
	if (!pRoot) 
		return FALSE;
		
	if (!m_pUserExportCriteria)		
		m_pUserExportCriteria = new CUserExportCriteria(this);

	return m_pUserExportCriteria->ParseUsr(pRoot);
}

//----------------------------------------------------------------------------
BOOL CXMLExportCriteria::UnparseUsrCriteriaFile(const CString& strUsrFileName, const CString& strExpFileName)
{
	if (!m_pUserExportCriteria)
	{
		if (!strUsrFileName.IsEmpty() && ::ExistFile(strUsrFileName))
			DeleteFile(strUsrFileName);
		
		return TRUE;
	}

	if (strUsrFileName.IsEmpty())
		return FALSE;

	CXMLDocumentObject aXMLUsrCriteriaDoc;
	aXMLUsrCriteriaDoc.SetValidateOnParse(TRUE);
	AfxInitWithXEngineEncoding(aXMLUsrCriteriaDoc);

	CXMLNode* pnRoot = aXMLUsrCriteriaDoc.CreateRoot(CRITERIA_XML_USER_CRITERIA_RULES);
	if (!pnRoot)
	{
		ASSERT(FALSE);
		return FALSE;
	}
	
	// creo il nodo relativo alle selezioni personalizzate
	// se m_pDoc è NULL vuol dire che sto editando gli usercriteria dai profili			
	if (!m_pUserExportCriteria->UnparseUsr(pnRoot))
		return FALSE;

	//se e' stato modificato il program data devo rimuovere le informazioni
	//relative alle variabili nel file expcriteria	
	if	(
			CompareNoFormat(
								m_pUserExportCriteria->m_OldQueryString, 
								m_pUserExportCriteria->GetCurrentQueryString()
							) != 0 &&
			!strExpFileName.IsEmpty() &&
			ExistFile(strExpFileName) 			
		)
		RemoveVariablesInfo(strExpFileName);
	
	aXMLUsrCriteriaDoc.SaveXMLFile(strUsrFileName, TRUE);
	return TRUE;
}

//----------------------------------------------------------------------------------------------
void CXMLExportCriteria::RemoveVariablesInfo(const CString& strExpFileName)
{
	CXMLDocumentObject aXMLCriteriaDoc;
	aXMLCriteriaDoc.EnableMsgMode(FALSE);// da togliere un domani quando ci dovranno
											// essere tutte le descizioni dei documenti
	
	if (!aXMLCriteriaDoc.LoadXMLFile(strExpFileName))  
		return;

	CXMLNode* pRoot = aXMLCriteriaDoc.GetRoot();
	if (!pRoot) 
		return;
	
	CXMLNode* pSiteNode = NULL;
	CXMLNode* pnUsrCrit = NULL;

	//cerco il nodo relativo al site corrente
	for(int i = 0 ; i < pRoot->GetChildsNum() ; i++)
	{
		pSiteNode = pRoot->GetChildAt(i);
		if (!pSiteNode)
			continue;

		pnUsrCrit = pSiteNode->GetChildByName(CRITERIA_XML_USER_CRITERIA_RULES);
		if (!pnUsrCrit)
			continue;

		pSiteNode->RemoveChild(pnUsrCrit);
		pnUsrCrit = NULL;
	}

	aXMLCriteriaDoc.SaveXMLFile(strExpFileName);
}

//----------------------------------------------------------------------------------------------
CXMLExportCriteria& CXMLExportCriteria::operator =(const CXMLExportCriteria& aXMLExpCriteria)
{
	if (this == &aXMLExpCriteria)
		return *this;

	Assign(&aXMLExpCriteria);

	return *this;
}

//------------------------------------------------------------------------------
BOOL CXMLExportCriteria::operator == (const CXMLExportCriteria& aExpCriteria) const
{
	if (this == &aExpCriteria)
		return TRUE;
	
	return
		(
			m_pDoc						== aExpCriteria.m_pDoc						&&
			m_pProfile					== aExpCriteria.m_pProfile					&&
			m_pRecord					== aExpCriteria.m_pRecord					&&
			m_bOwnSqlRec				== aExpCriteria.m_bOwnSqlRec				&&
			*m_pAppExportCriteria		== *aExpCriteria.m_pAppExportCriteria		&&
			*m_pOSLExportCriteria		== *aExpCriteria.m_pOSLExportCriteria		&&
			*m_pUserExportCriteria		== *aExpCriteria.m_pUserExportCriteria		&&
			*m_pPreferencesCriteria		== *aExpCriteria.m_pPreferencesCriteria
		);
}

//------------------------------------------------------------------------------
BOOL CXMLExportCriteria::operator != (const CXMLExportCriteria& aExpCriteria) const
{
	return !(*this == aExpCriteria);
}