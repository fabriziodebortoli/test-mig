
#include "stdafx.h"
#include <io.h>

#include <TBXMLCore\XMLSchema.h>
#include <TBNameSolver\FileSystemFunctions.h>
#include <TBNameSolver\IFileSystemManager.h>
#include <TbGeneric\DocumentObjectsInfo.h>
#include <TbClientCore\ClientObjects.h>
#include <TbWebServicesWrappers\LoginManagerInterface.h>
#include <TbGenlib\messages.h>
#include <TbGenlib\basedoc.h>
#include <TbGenlib\const.h>

#include <XEngine\TBXMLEnvelope\XEngineObject.h>
#include <XEngine\TBXMLEnvelope\XMLEnvelopeTags.h>
#include "GenFunc.h"
#include "XMLProfileInfo.h"
#include "XMLTransferTags.h"
#include "ExpCriteriaObj.h"

#ifdef _DEBUG
#undef THIS_FILE
static const char THIS_FILE[] = __FILE__;
#endif

static const TCHAR szCustomFolder[]	= _T("CustomComponents");
static const TCHAR szProfile[]		= _T("ExportProfiles");

//----------------------------------------------------------------------------------------------
//	CXMLProfileInfo
//----------------------------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(CXMLProfileInfo, CXMLDocObjectInfo)

//----------------------------------------------------------------------------------------------
CXMLProfileInfo::CXMLProfileInfo(const CTBNamespace& aDocumentNameSpace, LPCTSTR lpszProfileName /*=NULL*/, const CString& strUserName/*=_T("")*/)
	:
	CXMLDocObjectInfo		(aDocumentNameSpace),
	m_pExportCriteria		(NULL),
	m_pCurrExportCriteria	(NULL), 
	m_pDocument				(NULL),
	m_bInExport				(FALSE),
	m_bNewProfile			(FALSE),
	m_bPreferred			(FALSE),
	m_bModified				(FALSE),
	m_bUseFieldSel			(FALSE)
{
	m_strUserName = (strUserName.IsEmpty()) ? AfxGetLoginInfos()->m_strUserName : strUserName;

	Assign(aDocumentNameSpace, lpszProfileName);
}

// serve quando il profilo viene istanziato in fase di esportazione
//----------------------------------------------------------------------------------------------
CXMLProfileInfo::CXMLProfileInfo(CAbstractFormDoc* pDocument, LPCTSTR lpszProfileName /*=NULL*/,  const CString& strUserName/*=_T("")*/)
	:
	CXMLDocObjectInfo		(pDocument->GetNamespace()),
	m_pExportCriteria		(NULL),
	m_pCurrExportCriteria	(NULL), 
	m_pDocument				(NULL),
	m_bInExport				(TRUE),
	m_bNewProfile			(FALSE),
	m_bPreferred			(FALSE),
	m_bModified				(FALSE),
	m_bUseFieldSel			(FALSE)
{
	ASSERT(pDocument);
	
	m_strUserName = (strUserName.IsEmpty()) ? AfxGetLoginInfos()->m_strUserName : strUserName;
	SetDocument(pDocument);

	Assign(m_pDocument->GetNamespace(), lpszProfileName);
}


//----------------------------------------------------------------------------------------------
CXMLProfileInfo::CXMLProfileInfo(const CXMLProfileInfo& aProfileInfo)
	:
	CXMLDocObjectInfo		(aProfileInfo),
	m_pExportCriteria		(NULL),
	m_pCurrExportCriteria	(NULL),
	m_pDocument				(NULL),
	m_bInExport				(FALSE),
	m_bModified				(FALSE),
	m_bUseFieldSel			(FALSE)
{
	Assign(aProfileInfo);
}

//----------------------------------------------------------------------------
CXMLProfileInfo::~CXMLProfileInfo()
{
	BOOL bSameCriteria = (m_pCurrExportCriteria == m_pExportCriteria);

	if (m_pExportCriteria)
	{
		delete m_pExportCriteria;
		m_pExportCriteria = NULL;
	}

	if (m_pCurrExportCriteria && !bSameCriteria )
		delete m_pCurrExportCriteria;
	m_pCurrExportCriteria = NULL;
}

//----------------------------------------------------------------------------
CString CXMLProfileInfo::GetProfileNameFromPath(const CString& strProfilePath)
{
	if (!strProfilePath.IsEmpty())
	{	 
		//se è una path (ottimizzazione per la gestione della dialog dei profili, per evitare di dover riscorrere nuovamente i profili)
		int pos = strProfilePath.ReverseFind(SLASH_CHAR);
		if (pos < 0) 
			pos = strProfilePath.ReverseFind(URL_SLASH_CHAR);
		if (pos >= 0)
			return strProfilePath.Right(strProfilePath.GetLength() - (pos + 1));
	}
	return strProfilePath;
}
//----------------------------------------------------------------------------------------------
void CXMLProfileInfo::Assign(const CTBNamespace& aDocumentNameSpace, LPCTSTR lpszProfileName)
{
	if (GetNamespaceDoc() != aDocumentNameSpace)
	{
		SetDocInformation(aDocumentNameSpace);
		m_strDocumentPath = ::AfxGetPathFinder()->GetDocumentPath(aDocumentNameSpace, CPathFinder::STANDARD);
		AttachClientDocInfo();
	}

	SetName(lpszProfileName);
	SetAllFilesName();
}

//----------------------------------------------------------------------------------------------
void CXMLProfileInfo::Assign(const CXMLProfileInfo& aProfileInfo)
{
	if (this == &aProfileInfo)
		return;

	m_strProfileName    = aProfileInfo.m_strProfileName;
	m_nsProfile			= aProfileInfo.m_nsProfile;
	m_strUserName		= aProfileInfo.m_strUserName;
	
	m_strDocumentPath	= aProfileInfo.m_strDocumentPath;
	ASSERT(!m_strDocumentPath.IsEmpty());
	
	m_strDocProfilePath = aProfileInfo.m_strDocProfilePath;
	ASSERT(!m_strDocProfilePath.IsEmpty());

	m_pDocument		= aProfileInfo.m_pDocument;
	m_bPreferred	= aProfileInfo.m_bPreferred;
	m_bInExport		= aProfileInfo.m_bInExport;
	m_bUseFieldSel	= aProfileInfo.m_bUseFieldSel;

	m_strExpCriteriaFileName	= aProfileInfo.m_strExpCriteriaFileName;
	m_strFieldInfoFileName		= aProfileInfo.m_strFieldInfoFileName;
	m_strHKLInfoFileName		= aProfileInfo.m_strHKLInfoFileName;
	m_strProfileXRefFileName	= aProfileInfo.m_strProfileXRefFileName;
	m_strSchemaFileName			= aProfileInfo.m_strSchemaFileName;

	SetXMLExportCriteria(aProfileInfo);
}

//----------------------------------------------------------------------------
void CXMLProfileInfo::SetDocument(CAbstractFormDoc* pDocument)
{
	m_pDocument = pDocument;
	if (!m_pDocument)
	{
		SetDocInformation(CTBNamespace());
		m_strDocumentPath.Empty();
		m_bPreferred = FALSE;
		return;
	}	
	
	SetDocInformation(m_pDocument->GetNamespace());
	m_strDocumentPath = ::AfxGetPathFinder()->GetDocumentPath(m_pDocument->GetNamespace(), CPathFinder::STANDARD);
	
	m_bPreferred = (!m_strProfileName.IsEmpty() && m_pDocument->GetXMLDocInfo()) ? (m_strProfileName.CompareNoCase(m_pDocument->GetXMLDocInfo()->GetPreferredProfile()) == 0) : FALSE;
	AttachClientDocInfo();
}

//----------------------------------------------------------------------------------------------
CXMLProfileInfo& CXMLProfileInfo::operator = (const CXMLProfileInfo& aProfileInfo)
{
	if (this == &aProfileInfo)
		return *this;
	
	CXMLDocObjectInfo::Assign(aProfileInfo);

	Assign(aProfileInfo);

	return *this;
}

//------------------------------------------------------------------------------
BOOL CXMLProfileInfo::operator == (const CXMLProfileInfo& aProfileInfo) const
{
	if (this == &aProfileInfo)
		return TRUE;
	
	if (*(CXMLDocObjectInfo*)this != *(CXMLDocObjectInfo*)&aProfileInfo)
		return FALSE;
	
	return
		(
			m_pDocument				== aProfileInfo.m_pDocument					&&
			m_bIsLoaded				== aProfileInfo.m_bIsLoaded					&&
			m_bInExport				== aProfileInfo.m_bInExport					&&
			m_bNewProfile			== aProfileInfo.m_bNewProfile				&&
			m_bPreferred			== aProfileInfo.m_bPreferred				&&
			m_bUseFieldSel			== aProfileInfo.m_bUseFieldSel				&&
			m_strProfileName		== aProfileInfo.m_strProfileName			&&
			m_nsProfile				== aProfileInfo.m_nsProfile					&&
			m_strUserName			== aProfileInfo.m_strUserName				&&
			m_strDocumentPath		== aProfileInfo.m_strDocumentPath			&&
			m_strDocProfilePath		== aProfileInfo.m_strDocProfilePath			&&
			m_strExpCriteriaFileName== aProfileInfo.m_strExpCriteriaFileName	&&
			m_strFieldInfoFileName	== aProfileInfo.m_strFieldInfoFileName		&&
			m_strHKLInfoFileName	== aProfileInfo.m_strHKLInfoFileName		&&
			m_strProfileXRefFileName== aProfileInfo.m_strProfileXRefFileName	&&
			*m_pExportCriteria		== *aProfileInfo.m_pExportCriteria			&&
			*m_pCurrExportCriteria	== *aProfileInfo.m_pCurrExportCriteria      &&
			*m_strSchemaFileName	== *aProfileInfo.m_strSchemaFileName		
		);
}

//------------------------------------------------------------------------------
BOOL CXMLProfileInfo::operator != (const CXMLProfileInfo& aProfileInfo) const
{
	return !(*this == aProfileInfo);
}

//----------------------------------------------------------------------------------------------
CString CXMLProfileInfo::GetXSLTFileName() const
{
	//Transformation xslt
	 CString strXSLTFileName;
	if (m_pHeaderInfo->m_bTransform  && !m_pHeaderInfo->m_strTransformXSLT.IsEmpty())
	{
		//first I search the xsl transform file in profile folder
		strXSLTFileName = ::MakeFilePath(m_strDocProfilePath, m_pHeaderInfo->m_strTransformXSLT, FileExtension::XSL_EXT());
		if (AfxGetFileSystemManager()->ExistFile(strXSLTFileName))
			return strXSLTFileName;
		else
		{
			//then in the standard document description folder
			strXSLTFileName = ::MakeFilePath(
										AfxGetPathFinder()->GetDocumentDescriptionPath(GetNamespaceDoc(), CPathFinder::STANDARD),
										m_pHeaderInfo->m_strTransformXSLT, 
										FileExtension::XSL_EXT()
										 );

			if (AfxGetFileSystemManager()->ExistFile(strXSLTFileName))
				return strXSLTFileName;
		}
	}
	return strXSLTFileName;
}

//----------------------------------------------------------------------------------------------
void CXMLProfileInfo::SetAllFilesName()
{
	//const CDocumentDescription* pDocDescri =  AfxGetDocumentDescription(m_nsDoc);
	CPathFinder::PosType posType = CPathFinder::STANDARD;

	m_strDBTFileName			= AfxGetPathFinder()->GetDocumentDbtsFullName	(m_nsDoc);
	m_strXRefFileName			= AfxGetPathFinder()->GetDocumentXRefFullName	(m_nsDoc);
	m_strDocumentPath			= ::AfxGetPathFinder()->GetDocumentPath	(m_nsDoc, posType);
		
	m_strExpCriteriaFileName	= GetExpCriteriaVarFile(m_nsDoc, m_strProfileName);
	
	// se non ho il nome del profilo allora prendo le info necessarie dai file di descrizione
	// del documento
	if (m_strProfileName.IsEmpty() || m_strDocProfilePath.IsEmpty())
	{
		m_strDocFileName		= m_nsDoc.GetObjectName(CTBNamespace::DOCUMENT);
		m_strUsrCriteriaFileName.Empty();
		m_strFieldInfoFileName.Empty();
		m_strHKLInfoFileName.Empty();
	}
	else
	{	
		m_ePosType = AfxGetPathFinder()->GetPosTypeFromPath(m_strDocProfilePath);
		if (m_ePosType == CPathFinder::USERS)
			m_strUserName = AfxGetPathFinder()->GetUserNameFromPath(m_strDocProfilePath);

		m_strDocFileName			= ::GetDocFileFromDocProfilePath			(m_strDocProfilePath);
		m_strUsrCriteriaFileName	= ::GetUsrCriteriaFileFromDocProfilePath	(m_strDocProfilePath);
		m_strFieldInfoFileName		= ::GetFieldFileFromDocProfilePath			(m_strDocProfilePath);
		m_strProfileXRefFileName	= ::GetXRefFileFromDocProfilePath			(m_strDocProfilePath);
		m_strHKLInfoFileName		= ::GetHKLFileFromDocProfilePath			(m_strDocProfilePath);
		m_strSchemaFileName			= ::GetSmartSchemaProfileFile				(m_nsDoc, m_strDocProfilePath);
		if (m_pClientDocsInfo)
		{
			//set the exportprofile's path for the attached clientdocuments
			// Example clientdoc DocVenditaRiferimenti
			// standard: C:\MicroareaServer\Development\Running\Standard\Applications\TestApplication\TaDataEntry\ModuleObjects\DocVenditaRiferimenti\ExportProfiles\DDT\ProfileName
			// alluser:C:\MicroareaServer\Development\Running\Custom\Companies\MagoNet\Applications\TestApplication\TADataEntry\ModuleObjects\DocVenditaRiferimenti\ExportProfiles\AllUsers\DDT\ProfileName
			// user: C:\MicroareaServer\Development\Running\Custom\Companies\MagoNet\Applications\TestApplication\TADataEntry\ModuleObjects\DocVenditaRiferimenti\ExportProfiles\Users\sa\DDT\ProfileName
			// to optimize  the profile computes partial path for all the clientdocument only one time 
				m_pClientDocsInfo->SetFilesFromPartialPath(AfxGetPathFinder()->GetPartialProfilePathForClientDoc(m_nsProfile, m_ePosType, m_strUserName), m_ePosType);
		}
	}
}


//----------------------------------------------------------------------------------------------
BOOL CXMLProfileInfo::CanRunOnlyBusinessObject()
{
	if (m_bIsLoaded)
		return m_pHeaderInfo->m_bOnlyBusinessObject;

	SetAllFilesName();
	return (LoadHeaderFile()) ? m_pHeaderInfo->m_bOnlyBusinessObject : FALSE;
}

//----------------------------------------------------------------------------------------------
void CXMLProfileInfo::SetProfileNamespace(const CString& strProfileName)
{
	m_nsProfile.AutoCompleteNamespace(CTBNamespace::PROFILE, strProfileName, m_nsDoc);
}

//returns the following namespaceUri:
//http://www.microarea.it/Schema/2004/Smart/AppName/ModName/DocumentName/PosType/UserName/ProfileName.xsd
//---------------------------------------------------------------------------------------------
CString CXMLProfileInfo::GetSmartNamespaceURI() const
{
	CString strProfileName = (m_strDocProfilePath.IsEmpty())
						 ? DESCRI + m_nsDoc.GetObjectName() 
						 : GetName();	
	strProfileName.Replace(BLANK_CHAR, _T('_'));
	

	CString strPos = szStandard;
	CPathFinder::PosType ePosType = CPathFinder::STANDARD;
	CString strUser = AfxGetPathFinder()->GetUserNameFromPath(m_strDocProfilePath);

	if (!m_strDocProfilePath.IsEmpty())
	{
		ePosType = AfxGetPathFinder()->GetPosTypeFromPath(m_strDocProfilePath);
		
		if (ePosType != CPathFinder::STANDARD)
		{
			if (ePosType == CPathFinder::USERS)
			{
				strPos = _T("Users");
				strPos += URL_SLASH_CHAR + strUser;
			}
			else
				strPos = szAllUserDirName;
		}
	}

	return  SMART_NAMESPACE + 
			m_nsDoc.GetApplicationName() + URL_SLASH_CHAR +
			m_nsDoc.GetObjectName(CTBNamespace::MODULE) + URL_SLASH_CHAR +
			m_nsDoc.GetObjectName() +  URL_SLASH_CHAR  +
			strPos + URL_SLASH_CHAR +					
			strProfileName + szXsdExt;
}

// I can't directly modify the source file because the XMLDoc doesn't allow me to change the xmlns attribute that is 
// used from the document
//----------------------------------------------------------------------------------------------
void CXMLProfileInfo::ModifySmartDocumentSchema(const CString& strOldSchema)
{
	if (!ExistFile(strOldSchema))
		return;
				
	CString strProfileName = (m_strProfileName.IsEmpty()) ? DESCRI + m_nsDoc.GetObjectName() : m_strProfileName;
	CString strTargetNamespace = GetSmartNamespaceURI();
	CString strId = m_nsDoc.GetObjectName() + strProfileName;
	strId.Replace(BLANK_CHAR, _T('_'));

	CXMLDocumentObject aSourceXMLDocument;
	CXMLDocumentObject aTargetXMLDocument(TRUE);

	//I need to modify the attribute xTechProfile of the only one node element  
	aSourceXMLDocument.LoadXMLFile(strOldSchema);
	CXMLNode* pSourceRoot = aSourceXMLDocument.GetRoot();
	CXMLNode* pNode = pSourceRoot->GetChildByName(SCHEMA_XSD_ELEMENT_TAG);
	if (pNode)
	{
		pNode = pNode->GetChildByName(SCHEMA_XSD_COMPLEX_TYPE_TAG);
		if (pNode)
		{
			pNode = pNode->GetChildByAttributeValue(SCHEMA_XSD_ATTRIBUTE_TAG, SCHEMA_XSD_NAME_ATTRIBUTE, DOC_XML_XTECHPROFILE_ATTRIBUTE);
			if (pNode)
				pNode->SetAttribute(SCHEMA_XSD_FIXED_ATTRIBUTE, strProfileName);
		}
	}

	//Creating the renamed schema
	aTargetXMLDocument.SetNameSpaceURI(SCHEMA_XSD_NAMESPACEURI_VALUE, SCHEMA_XSD_NAMESPACEURI_PREFIX);
	CXMLNode* pTargetRoot = aTargetXMLDocument.CreateRoot(SCHEMA_XSD_SCHEMA_TAG);
	if (pTargetRoot)
	{
		pTargetRoot->SetAttribute(SCHEMA_XSD_ELEMENT_FORM_DEFAULT_ATTRIBUTE, SCHEMA_XSD_QUALIFIED_VALUE);
		pTargetRoot->SetAttribute(SCHEMA_XSD_XMLNS, strTargetNamespace);
		pTargetRoot->SetAttribute(SCHEMA_XSD_TARGET_NAMESPACE_ATTRIBUTE, strTargetNamespace);
		pTargetRoot->SetAttribute(SCHEMA_XSD_ID, strId);		

		//get all the root's childs of the source document and copy them to the target document	
		CXMLNodeChildsList* pChildList = pSourceRoot->GetChilds();
		int nChildCount = pChildList->GetCount();
		for (int nIdx = 0; nIdx < nChildCount; nIdx++)
			pTargetRoot->AppendChild(pChildList->GetAt(0)); //use alway the first child because the AppendChild remove the child from the source document
	}
	
	aTargetXMLDocument.SaveXMLFile(m_strSchemaFileName);	
}

//----------------------------------------------------------------------------------------------
BOOL CXMLProfileInfo::RenameProfile(const CString& strNewName)
{
	if (strNewName.IsEmpty())
		return FALSE;

	CString strOldPath = m_strDocProfilePath;
	CString strOldName = m_strProfileName;
	CString strNewPath;

	if	(!strOldPath.IsEmpty())
	{
		int nNameLengh = strOldName.GetLength();
		if (nNameLengh > 0)
		{
			strNewPath = strOldPath.Left(strOldPath.GetLength() - nNameLengh);
			strNewPath += strNewName;
			if (!strNewPath.IsEmpty())
			{
				if (!RenameFilePath(strOldPath, strNewPath) || (m_pClientDocsInfo && !m_pClientDocsInfo->RenameProfilePath(strNewName)))
					//AfxMessageBox(IDS_PROFILE_RENAME_CLIENDDOC_ERROR); 
					return FALSE;

				CString strOldSchema = m_strSchemaFileName;	
				//devo modificare anche il namespace
				SetProfileNamespace(strNewName);
				m_strProfileName = strNewName;
				m_strDocProfilePath	= strNewPath;
                SetAllFilesName();	
				ModifySmartDocumentSchema(strOldSchema);
				DeleteFile(strOldSchema);
				return TRUE;				
			}
		}
	}
	
	return FALSE;
}

//----------------------------------------------------------------------------------------------
BOOL CXMLProfileInfo::LoadAllFiles(CAutoExpressionMng* pAutoExpressionMng	/* = NULL*/)
{
	return (m_bIsLoaded = 
				CXMLDocObjectInfo::LoadHeaderFile	()						&&
				CXMLDocObjectInfo::LoadDBTFile		()						&& 
				LoadProfileXRefFiles				()						&&
				LoadUsrCriteriaFile					()						&&
				LoadExportCriteriaFile				(pAutoExpressionMng)	&&
				LoadFieldInfoFile					()						&&
				LoadHotKeyLinkInfoFile				()
			);
}

// carico quello dei documento e poi quello dei clientdoc 
//----------------------------------------------------------------------------------------------
BOOL CXMLProfileInfo::LoadProfileXRefFiles()
{
	// se sto caricando la descrizione non faccio niente
	if (m_strProfileName.IsEmpty())
		return TRUE;

	//metto i flag di esportazione a false, verranno messi a true dai rispettivi xref parsati

	SetXRefExportFlag(FALSE);
	if (
			m_pDBTArray &&
			!ParseXRefFile(m_strProfileXRefFileName)
		)
		return FALSE;

	if (m_pClientDocsInfo)
	{
		for (int i = 0; i < m_pClientDocsInfo->GetSize(); i++)
		{
			CXMLClientDocInfo* pClientDoc = m_pClientDocsInfo->GetAt(i);
			if (
				  pClientDoc->m_pDBTArray && 
				  !ParseXRefFile(pClientDoc->m_strXRefFileName)
				)
				return FALSE;
		}
	}

	return TRUE;
}

//carica le informazioni di xref relative a profilo:
//	se il file esiste mette tutti gli export degli xref a false
//	le personalizzazione degli xref apportate dal profilo
//	gli xref definiti da utente
//  setto export a true per ogni xref
//----------------------------------------------------------------------------------------------
BOOL CXMLProfileInfo::ParseXRefFile(const CString& strExtRefFileName)
{
	if (strExtRefFileName.IsEmpty() || !ExistFile(strExtRefFileName))
		return TRUE;

	CXMLDocumentObject aXMLProfileInfoDoc;
	if (!aXMLProfileInfoDoc.LoadXMLFile(strExtRefFileName))
		return FALSE;

	CXMLNode* pnRooth = aXMLProfileInfoDoc.GetRoot();
	if (!pnRooth)
		return FALSE;
	
	for(int i = 0 ; i < pnRooth->GetChildsNum() ; i++)
	{
		CXMLNode* pnDBTNode = pnRooth->GetChildAt(i);
		if (!pnDBTNode )
			continue;

		CString strTagValue;
		pnDBTNode->GetName(strTagValue);
		if (strTagValue != XML_DBT_TAG)
			continue;

		//cerco il nodo xml che contiene il NameSpace del dbt
		CString strDBTNamespace;
		pnDBTNode->GetAttribute(XML_NAMESPACE_ATTRIBUTE, strDBTNamespace);
		CTBNamespace aNs;
		aNs.AutoCompleteNamespace(CTBNamespace::DBT, strDBTNamespace, aNs);
		CXMLDBTInfo* pXMLDBTInfo = m_pDBTArray->GetDBTByNamespace(aNs);

		if (!pXMLDBTInfo)
			continue;
	
		//cerco il nodo export del dbt
		CXMLNode* pnNode = pnDBTNode->GetChildByName(XML_EXPORT_TAG);
		if (!pnNode)
			continue;

		CString strName, strDataUrl, strProfName, strVal;

		pnNode->GetText(strVal);
		pXMLDBTInfo->m_bExport = GetBoolFromXML(strVal);

		pnNode = pnDBTNode->GetChildByName(XML_DBT_UPDATETYPE_TAG);
		if (pnNode)
		{
			pnNode->GetText(strVal);
			pXMLDBTInfo->SetUpdateType(strVal);
		}		

		//cerco il nodo xref
		CXMLNode* pnXRefsNode = pnDBTNode->GetChildByName(XML_EXTERNAL_REFERENCES_TAG);
		if (!pnXRefsNode)
			continue;

		if (pnXRefsNode->GetChildsNum() == 0)
			pXMLDBTInfo->SetXRefUseFlag(FALSE);

		//parso la parte relativa agli xref
		for(int n = 0 ; n < pnXRefsNode->GetChildsNum() ; n++)
		{
			CXMLNode* pnXRefNode = pnXRefsNode->GetChildAt(n);
			if (!pnXRefNode)
				continue;

			CString strTagValue;
			pnXRefNode->GetName(strTagValue);
			if (strTagValue != XML_EXTERNAL_REFERENCE_TAG)
				continue;

			//e' un nodo xref
						
			//nome xref
			CXMLNode* pnNode = pnXRefNode->GetChildByName(XML_NAME_TAG);
			if (!pnNode)
			{
				ASSERT(FALSE);
				continue;
			}

			pnNode->GetText(strName);

			//prendo l'xref caricato dal doc
			CXMLXRefInfo* pXRefInfo = pXMLDBTInfo->GetXRefByName(strName);
			
			//se il doc non lo contiene vuol dire che e' uno user
			if (!pXRefInfo)
			{
				pXRefInfo = new CXMLXRefInfo(pXMLDBTInfo->GetTableName());
				pXRefInfo->SetOwnedByDoc(FALSE);
				pXMLDBTInfo->AddXRef(pXRefInfo);
			}
			else
				pXRefInfo->SetOwnedByDoc(TRUE);
			
			pXRefInfo->Parse(pnXRefNode, FALSE);			
			pXRefInfo->SetUse(TRUE);
			pXRefInfo->m_bOldUse = pXRefInfo->IsToUse();

		}
	}

	return TRUE;
}

//----------------------------------------------------------------------------------------------
BOOL CXMLProfileInfo::LoadExportCriteriaFile(CAutoExpressionMng* pAutoExpressionMng	/* = NULL*/)
{
	// non è detto che ci sia
	if (m_strExpCriteriaFileName.IsEmpty() || !ExistFile(m_strExpCriteriaFileName))
		return TRUE;
	
	if (!m_pExportCriteria)
	{
		m_pExportCriteria = new CXMLExportCriteria(this, m_pDocument);
		m_pCurrExportCriteria = m_pExportCriteria;
	}

	return m_pExportCriteria->ParseExpCriteriaFile(m_strExpCriteriaFileName, _T(""), pAutoExpressionMng);
}

//----------------------------------------------------------------------------------------------
BOOL CXMLProfileInfo::LoadUsrCriteriaFile()
{
	// non è detto che ci sia
	if (m_strUsrCriteriaFileName.IsEmpty() || !ExistFile(m_strUsrCriteriaFileName))
		return TRUE;
	
	if (!m_pExportCriteria)
	{
		m_pExportCriteria = new CXMLExportCriteria(this, m_pDocument);
		m_pCurrExportCriteria = m_pExportCriteria;
	}

	return m_pExportCriteria->ParseUsrCriteriaFile(m_strUsrCriteriaFileName);
}

//----------------------------------------------------------------------------------------------
BOOL CXMLProfileInfo::LoadFieldInfoFile()
{
	// non è detto che ci sia
	if (m_strFieldInfoFileName.IsEmpty() || !ExistFile(m_strFieldInfoFileName) || !m_pDBTArray)
		return TRUE;
	
	m_bUseFieldSel = m_pDBTArray->LoadFieldInfoFile(m_strFieldInfoFileName);

	return TRUE;
}

//----------------------------------------------------------------------------------------------
BOOL CXMLProfileInfo::LoadHotKeyLinkInfoFile()
{
	// non è detto che ci sia
	if (m_strHKLInfoFileName.IsEmpty() || !ExistFile(m_strHKLInfoFileName) || !m_pDBTArray)
		return TRUE;
	
	return m_pDBTArray->LoadHotKeyLinkInfoFile(m_strHKLInfoFileName);
}

//----------------------------------------------------------------------------------------------
BOOL CXMLProfileInfo::SaveProfile(const CString& strPath, const CString& strNewName)
{
	if (!m_bModified)
		return TRUE;

	SetProfileNamespace(strNewName);
	m_strProfileName = strNewName;
	m_strDocProfilePath	= strPath;
	SetAllFilesName();	

	// se esiste il file di schema associato al profilo, lo rimuovo
	// verrà poi generato all'occorrenza
	CString strProfileSchema = ::GetSchemaProfileFile(this);
	if (ExistFile(strProfileSchema))
		DeleteFile(strProfileSchema);

	return (
				SaveHeaderFile		()	&&
				SaveXRefFile		()	&&
				SaveUsrCriteriaFile	()	&&
				SaveFieldInfoFile	()  &&
				SaveHotKeyLinkInfoFile()
			);
}

//----------------------------------------------------------------------------------------------
BOOL CXMLProfileInfo::SaveXRefFile() 
{	
	CXMLDocumentObject aXMLXRefDoc;
	AfxInitWithXEngineEncoding(aXMLXRefDoc);
	
	CXMLNode* pnRootXRefs = aXMLXRefDoc.CreateRoot(XML_MAIN_EXTERNAL_REFERENCES_TAG);
	if (!pnRootXRefs)
	{
		ASSERT(FALSE);
		return FALSE;
	}

	if (!m_pDBTArray)
		return FALSE;

	for ( int i = 0 ; i < m_pDBTArray->GetSize() ; i++)
	{
		CXMLDBTInfo* pDbt = m_pDBTArray->GetAt(i);
	
		if (!pDbt || pDbt->IsFromClientDoc())
			continue;

		if (!pDbt->m_pXRefsArray)
			pDbt->m_pXRefsArray = new CXMLXRefInfoArray;

		CXMLNode* pnXRefDBTNode = pnRootXRefs->CreateNewChild(XML_DBT_TAG);

		if (!pnXRefDBTNode)
			continue;

		pDbt->m_pXRefsArray->UnParse(pnXRefDBTNode, pDbt, FALSE);		
	}

	aXMLXRefDoc.SaveXMLFile(m_strProfileXRefFileName, TRUE);

	if (m_pClientDocsInfo)
		m_pClientDocsInfo->SaveXRefFile(FALSE);

	return TRUE;
}

//----------------------------------------------------------------------------------------------
BOOL CXMLProfileInfo::SaveUsrCriteriaFile()
{
	if (!m_pExportCriteria || m_strUsrCriteriaFileName.IsEmpty()) return TRUE;
		
	return 	m_pExportCriteria->UnparseUsrCriteriaFile(m_strUsrCriteriaFileName, m_strExpCriteriaFileName);
}


//----------------------------------------------------------------------------------------------
BOOL CXMLProfileInfo::SaveFieldInfoFile()
{
	if (!m_pDBTArray)
	{
		ASSERT(FALSE);
		return FALSE;
	}

	return 	m_pDBTArray->SaveFieldInfoFile(m_strFieldInfoFileName);
}

//----------------------------------------------------------------------------------------------
BOOL CXMLProfileInfo::SaveHotKeyLinkInfoFile()
{
	if (!m_pDBTArray)
	{
		ASSERT(FALSE);
		return FALSE;
	}

	return 	m_pDBTArray->SaveHotKeyLinkInfoFile(m_strHKLInfoFileName);
}


//----------------------------------------------------------------------------------------------
void CXMLProfileInfo::SetXMLExportCriteria(const CXMLExportCriteria* pExportCriteria)
{
	if (pExportCriteria)
	{
		if (m_pExportCriteria != pExportCriteria)
		{
			if (m_pExportCriteria)
				delete m_pExportCriteria;
			m_pExportCriteria = new CXMLExportCriteria(*pExportCriteria);
		}
		m_pCurrExportCriteria = m_pExportCriteria;
	}
	else
	{
		BOOL bSameCriteria = (m_pExportCriteria == m_pCurrExportCriteria);
		if (m_pExportCriteria)
			delete m_pExportCriteria;
		m_pExportCriteria = NULL;
		
		if (m_pCurrExportCriteria && !bSameCriteria)
			delete m_pCurrExportCriteria;
		m_pCurrExportCriteria = NULL;
	}
}

//----------------------------------------------------------------------------------------------
void CXMLProfileInfo::SetCurrentXMLExportCriteria(CXMLExportCriteria* pExportCriteria)
{
	if (	
			m_pCurrExportCriteria &&
			m_pCurrExportCriteria != pExportCriteria &&
			m_pCurrExportCriteria != m_pExportCriteria
		)
		
		delete m_pCurrExportCriteria;
		
	m_pCurrExportCriteria = pExportCriteria;
	
	if (!m_pExportCriteria)
		m_pExportCriteria = m_pCurrExportCriteria;			
}


//----------------------------------------------------------------------------------------------
void CXMLProfileInfo::SetXMLExportCriteria(const CXMLProfileInfo& aProfileInfo)
{
	if (this == &aProfileInfo)
		return;

	SetXMLExportCriteria(aProfileInfo.GetXMLExportCriteria());
}


//----------------------------------------------------------------------------
void CXMLProfileInfo::SetName(const CString& strProfileName)
{
	if (!strProfileName.IsEmpty() && ::IsPathName(strProfileName))
	{
		m_strProfileName = GetProfileNameFromPath(strProfileName);
		m_strDocProfilePath = strProfileName;
		SetProfileNamespace(m_strProfileName);
	}
	else
	{
		m_strProfileName = strProfileName;
		SetProfileNamespace(m_strProfileName);
		m_strDocProfilePath = AfxGetPathFinder()->GetFileNameFromNamespace(m_nsProfile, m_strUserName);
	}	
}

//----------------------------------------------------------------------------
BOOL CXMLProfileInfo::RemoveProfilePath() const
{
	if (m_strDocProfilePath.IsEmpty())
		return FALSE;

	if (RemoveFolderTree(m_strDocProfilePath))
	{
		if (m_pClientDocsInfo) 
		{
			BOOL bOk = m_pClientDocsInfo->RemoveProfilePath(); 
			if (!bOk)
				return FALSE;
		}

		//devo cancellare dalla path Schema (per OfficeIntegration) lo schema del profilo (se esiste)
		if (ExistFile(m_strSchemaFileName))
			return DeleteFile(m_strSchemaFileName);

		return TRUE;
	}

	return FALSE;

}

//----------------------------------------------------------------------------
BOOL CXMLProfileInfo::RemoveOptProfileFiles() const
{
	if (m_strDocProfilePath.IsEmpty())
		return FALSE;

	return 
		DeleteFile((LPCTSTR)::GetUsrCriteriaFileFromDocProfilePath(m_strDocProfilePath)) &&
		DeleteFile((LPCTSTR)::GetFieldFileFromDocProfilePath(m_strDocProfilePath));
}

//----------------------------------------------------------------------------
BOOL CXMLProfileInfo::IsPredefined() const
{
	return (m_strProfileName.CompareNoCase(szPredefined) == 0);
}

//----------------------------------------------------------------------------
void CXMLProfileInfo::SetPreferred(BOOL bPreferred /*= TRUE*/)
{
	m_bPreferred = bPreferred;

	if (!m_pDocument || !m_pDocument->GetXMLDocInfo())
		return;
	
	if (!m_bPreferred && (m_strProfileName.CompareNoCase(m_pDocument->GetXMLDocInfo()->GetPreferredProfile()) == 0))
	{
		m_pDocument->GetXMLDocInfo()->SetPreferredProfile(_T(""));
		return;
	}
	if (m_bPreferred)
		m_pDocument->GetXMLDocInfo()->SetPreferredProfile(m_strProfileName);
}

//----------------------------------------------------------------------------
void CXMLProfileInfo::SetModified(BOOL bModified /* = TRUE*/)
{
	m_bModified = bModified;
}

//----------------------------------------------------------------------------
void CXMLProfileInfo::SetXRefExportFlag(BOOL bExport /*= TRUE*/)
{
	if (!m_pDBTArray)
	{
		ASSERT(FALSE);
		return;
	}

	CXMLDBTInfo* pXMLDBTInfo = NULL;
	for(int i = 0 ; i < m_pDBTArray->GetSize() ; i++)
	{
		pXMLDBTInfo = m_pDBTArray->GetAt(i);
		if (!pXMLDBTInfo)
			continue;
	
		pXMLDBTInfo->m_bExport = (pXMLDBTInfo->GetType() == CXMLDBTInfo::MASTER_TYPE);
	
		if (!pXMLDBTInfo->GetXMLXRefInfoArray())
			continue;

		for(int n = 0 ; n < pXMLDBTInfo->GetXMLXRefInfoArray()->GetSize() ; n++)
		{
			CXMLXRefInfo* pXRefInfo = pXMLDBTInfo->GetXMLXRefInfoArray()->GetAt(n);
			if (!pXRefInfo)
			{
				ASSERT(FALSE);
				continue;
			}

			pXRefInfo->SetUse(bExport);
		}
	}
}

//Verifico se l'external reference passato ha tutti i campi esportati
//----------------------------------------------------------------------------
BOOL CXMLProfileInfo::IsValidXRef(CXMLXRefInfo* pXMLXRefInfo)
{
	if (!pXMLXRefInfo || !m_pDBTArray)
	{
		ASSERT(FALSE);
		return FALSE;
	}

	for(int nSeg = 0 ; nSeg < pXMLXRefInfo->GetSegmentsNum() ; nSeg++)
	{
		CString strFK = pXMLXRefInfo->GetSegmentAt(nSeg)->GetFKSegment();

		CXMLDBTInfo* pXMLDBTInfo = m_pDBTArray->GetDBTByXRef(pXMLXRefInfo);
		if (!pXMLDBTInfo)
		{
			ASSERT(FALSE);
			return FALSE;
		}

		if (!pXMLDBTInfo->m_pXMLFieldInfoArray)
			return TRUE;

		//scorro i campi del dbt
		for(int nField = 0 ; nField < pXMLDBTInfo->m_pXMLFieldInfoArray->GetSize() ; nField++)
		{
			CXMLFieldInfo* pFieldInfo = pXMLDBTInfo->m_pXMLFieldInfoArray->GetAt(nField);
			//è il campo che mi serve
			if (pFieldInfo->GetFieldName().CompareNoCase(strFK) == 0 && pFieldInfo->IsToExport())
				return TRUE;
		}
	}

	return FALSE;
}


/////////////////////////////////////////////////////////////////////////////
// Diagnostics

#ifdef _DEBUG
void CXMLProfileInfo::Dump(CDumpContext& dc) const
{
	ASSERT_VALID (this);
	AFX_DUMP0(dc, "===> CXMLProfileInfo\n");
	dc << "\tProfile Name = " << m_strProfileName << "\n";
	CObject::Dump(dc);
}

//----------------------------------------------------------------------------
void CXMLProfileInfo::AssertValid() const
{
	CObject::AssertValid();
}

#endif //_DEBUG


