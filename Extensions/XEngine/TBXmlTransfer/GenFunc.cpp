#include "stdafx.h" 
#include <io.h>
#include <Vfw.h>

#include <TBXMLCore\XMLGeneric.h>
#include <TBXMLCore\XMLDocObj.h>
#include <TbClientCore\ClientObjects.h>
#include <TbWebServicesWrappers\LoginManagerInterface.h>
#include <TBGENERIC\dllmod.h>
#include <TBGENLIB\generic.h>
#include <TBGENLIB\baseapp.h>
#include <TBGENLIB\addonmng.h>
#include <TBGES\xmlgesinfo.h>

#include <XEngine\TBXMLEnvelope\XMLEnvelopeTags.h>

#include "XMLProfileInfo.h"
#include "XMLTransferTags.h"


#include "GenFunc.h"

static const TCHAR szDescription[]			= _T("Description");
static const TCHAR szProfile[]				= _T("ExportProfiles");
static const TCHAR szUrlDoc[]					= _T("Document.xml");
static const TCHAR szXRef[]					= _T("ExternalReferences.xml");
static const TCHAR szUrlField[]				= _T("Field.xml");
static const TCHAR szHotKeyLink[]				= _T("HotKeyLink.xml");
static const TCHAR szUrlExpCriteria[]			= _T("ExpCriteriaVars.xml");
static const TCHAR szUrlImpCriteria[]			= _T("ImpCriteriaVars.xml");
static const TCHAR szUrlUsrCriteria[]			= _T("UserExportCriteria.xml");
static const TCHAR szCodingRules[]			= _T("CodingRules.xml");

//----------------------------------------------------------------------------------------------
CString	GetProfilePath(
						const CTBNamespace& nsDocument, 
						const CString& strProfile, 
						CPathFinder::PosType ePosType, 
						const CString& userName, 
						BOOL bCreate /*=FALSE*/, 
						BOOL bUseCustomSearch /*=FALSE*/
					   )
{	
	// se come nome profilo ho Descri+nome del documento vuol dire che ho utilizzato la descrizione del documento
	// allora utilizzo la stringa vuota per caricarmi il profilo
	CString strProfileName = (!strProfile.IsEmpty() && strProfile.CompareNoCase(DESCRI + nsDocument.GetObjectName()) == 0) 
							? _T("") 
							: strProfile;	

	if (ePosType == CPathFinder::STANDARD || strProfileName.IsEmpty())
		return AfxGetPathFinder()->GetExportProfilePath(nsDocument, strProfileName, CPathFinder::STANDARD);
	
	CString strPath;
	if (ePosType == CPathFinder::ALL_USERS)
	{
		strPath = AfxGetPathFinder()->GetExportProfilePath(nsDocument, strProfileName, CPathFinder::ALL_USERS);
		if (!ExistPath(strPath) && bUseCustomSearch)
			strPath = AfxGetPathFinder()->GetExportProfilePath(nsDocument, strProfileName, CPathFinder::STANDARD);
		return strPath;
	}
	
	strPath = AfxGetPathFinder()->GetExportProfilePath(nsDocument, strProfileName, CPathFinder::USERS, userName, FALSE);
	if (!ExistPath(strPath) && bUseCustomSearch)
	{
		strPath = AfxGetPathFinder()->GetExportProfilePath(nsDocument, strProfileName, CPathFinder::ALL_USERS);
		if (!ExistPath(strPath))
			strPath = AfxGetPathFinder()->GetExportProfilePath(nsDocument, strProfileName, CPathFinder::STANDARD);
	}

	return strPath;		
}

//----------------------------------------------------------------------------------------------
CString GetSchemaProfileFile(CXMLProfileInfo* pProfileInfo)
{
	if (!pProfileInfo)
		return _T("");

	CString strActualProfileName, strDocPath;

	CPathFinder::PosType ePosType = AfxGetPathFinder()->GetPosTypeFromPath(pProfileInfo->m_strDocProfilePath);
	CString strUser = AfxGetPathFinder()->GetUserNameFromPath(pProfileInfo->m_strDocProfilePath);

	//se è un profilo standard oppure la descrizione inserisco lo schema nella ALL_USER
	if (ePosType == CPathFinder::STANDARD)
		ePosType = CPathFinder::ALL_USERS;

	// se il profilo è vuoto, significa che sto utilizzando la descrizione del documento
	// come profilo
	strActualProfileName =  pProfileInfo->GetNamespaceDoc().GetObjectName(CTBNamespace::DOCUMENT);
	if (pProfileInfo->m_strProfileName.IsEmpty())
		strDocPath = AfxGetPathFinder()->GetDocumentDescriptionPath(pProfileInfo->GetNamespaceDoc(), ePosType, strUser);
	else
	{	
		strDocPath = ::GetProfilePath(pProfileInfo->GetNamespaceDoc(), pProfileInfo->m_strProfileName, ePosType, strUser);
		strActualProfileName += pProfileInfo->m_strProfileName;
	}
	
	if (strDocPath.IsEmpty())
		return _T("");

	if (!IsDirSeparator(strDocPath.Right(1))) 
		strDocPath += SLASH_CHAR;

	return  strDocPath + strActualProfileName + szXsdExt;
}

//----------------------------------------------------------------------------------------------
BOOL GetProfileNamespace(const CTBNamespace& nsDocument, const CString& strProfileName, CTBNamespace& nsProfile)
{
	nsProfile.AutoCompleteNamespace(CTBNamespace::PROFILE, strProfileName, nsDocument);
	return nsProfile.IsValid();
}

//----------------------------------------------------------------------------------------------
BOOL ExistProfile(const CTBNamespace& nsDocument, const CString& strProfileName)
{	
	if (!strProfileName.IsEmpty() && ::IsPathName(strProfileName))
		return ::ExistPath(strProfileName);

	//compongo il namespace del profilo
	CTBNamespace aNsProfile;
	if (!strProfileName.IsEmpty() && !GetProfileNamespace(nsDocument, strProfileName, aNsProfile))
		return FALSE;

	CString strProfilePath = (strProfileName.IsEmpty())
							? AfxGetPathFinder()->GetDocumentDescriptionPath(nsDocument, CPathFinder::STANDARD)
							: AfxGetPathFinder()->GetFileNameFromNamespace(aNsProfile, AfxGetLoginInfos()->m_strUserName);
	CString strDocFileName = strProfilePath + SLASH_CHAR + szUrlDoc;
	
	return !strDocFileName.IsEmpty() && ExistFile(strDocFileName);
}

//----------------------------------------------------------------------------------------------
BOOL ExistProfile(const CString& strProfilePath)
{
	if (strProfilePath.IsEmpty())
		return FALSE;

	CString strDocFileName = strProfilePath + SLASH_CHAR + szUrlDoc;
	
	return !strDocFileName.IsEmpty() && ExistFile(strDocFileName);
}

// la path che mi viene passata come parametro è:
// <app>\BusinessObjects\<module>\<document>\ExportProfiles\<profilename>\
//----------------------------------------------------------------------------------------------
CString GetDocFileFromDocProfilePath(const CString& strProfilePath)
{
	if (strProfilePath.IsEmpty())
	{
		ASSERT(FALSE);
		return _T("");
	}
	CString strFilePath = strProfilePath;
	if (strFilePath.Right(1) != SLASH_CHAR) 
		strFilePath += SLASH_CHAR;
	return strFilePath +  szUrlDoc;
}

// la path che mi viene passata come parametro è:
// <app>\BusinessObjects\<module>\<document>\ExportProfiles\<profilename>\
//----------------------------------------------------------------------------------------------
CString GetXRefFileFromDocProfilePath(const CString& strProfilePath)
{
	if (strProfilePath.IsEmpty())
	{
		ASSERT(FALSE);
		return _T("");
	}
	CString strFilePath = strProfilePath;
	if (strFilePath.Right(1) != SLASH_CHAR) 
		strFilePath += SLASH_CHAR;

	return strFilePath + szXRef;
}


//----------------------------------------------------------------------------------------------
CString GetUsrCriteriaFileFromDocProfilePath(const CString& strProfilePath)
{
	if (strProfilePath.IsEmpty())
	{
		ASSERT(FALSE);
		return _T("");
	}
	CString strFilePath = strProfilePath;
	if (strFilePath.Right(1) != SLASH_CHAR) 
		strFilePath += SLASH_CHAR;

	return strFilePath + szUrlUsrCriteria;
}

//per gli exportcriteria considero sempre la path custom dell'utente loggato
//----------------------------------------------------------------------------------------------
CString GetExpCriteriaVarPath(const CTBNamespace& nsDocument, const CString& strProfileName)
{
	return GetProfilePath(nsDocument, strProfileName, CPathFinder::USERS, AfxGetLoginInfos()->m_strUserName);
}

//----------------------------------------------------------------------------------------------
CString GetExpCriteriaVarFile(const CTBNamespace& nsDocument, const CString& strProfileName)
{
	return GetExpCriteriaVarFile(nsDocument, strProfileName, szUrlExpCriteria);
}
	
//----------------------------------------------------------------------------------------------
CString GetExpCriteriaVarFile(const CTBNamespace& nsDocument, const CString& strProfileName, const CString& strExpCriteriaFileName)
{	
	CString strFileFullPath;
	
	//provo prima in custom user
	strFileFullPath = GetProfilePath(nsDocument, strProfileName, CPathFinder::USERS, AfxGetLoginInfos()->m_strUserName) + SLASH_CHAR + strExpCriteriaFileName;
	if (ExistFile(strFileFullPath))
		return strFileFullPath;

	//non l'ho trovato allora vado in custom alluser
	return GetProfilePath(nsDocument, strProfileName, CPathFinder::ALL_USERS) + SLASH_CHAR + strExpCriteriaFileName;
}

//----------------------------------------------------------------------------------------------
CString MakeExpCriteriaVarFile(const CTBNamespace& nsDocument, const CString& strProfileName, const CString& strFileName, CPathFinder::PosType ePosType, const CString& strUserRole /*=_T("")*/)
{
	CString strExpCriteriaFileName = (strFileName.IsEmpty()) ? szUrlExpCriteria : strFileName;
	return GetProfilePath(nsDocument, strProfileName, ePosType, strUserRole) + SLASH_CHAR + strExpCriteriaFileName;
}

//----------------------------------------------------------------------------------------------
CString MakeImpCriteriaVarFile(const CTBNamespace& nsDocument, const CString& strFileName, CPathFinder::PosType ePosType, const CString& strUserRole /*=_T("")*/)
{
	CString strImpCriteriaFileName = (strFileName.IsEmpty()) ? szUrlImpCriteria : strFileName;
	return AfxGetPathFinder()->GetDocumentDescriptionPath(nsDocument, ePosType, strUserRole, TRUE) + SLASH_CHAR + strImpCriteriaFileName;
}

//----------------------------------------------------------------------------------------------
CString GetImpCriteriaVarFile(const CTBNamespace& nsDocument, const CString& strImpCriteriaFileName)
{	
	return AfxGetPathFinder()->GetDocumentDescriptionPath(nsDocument, CPathFinder::USERS, AfxGetLoginInfos()->m_strUserName) + SLASH_CHAR + strImpCriteriaFileName;;
}

//----------------------------------------------------------------------------------------------
CString GetFieldFileFromDocProfilePath(const CString& strProfilePath)
{
	if (strProfilePath.IsEmpty())
	{
		ASSERT(FALSE);
		return _T("");
	}
	CString strFilePath = strProfilePath;
	if (strFilePath.Right(1) != SLASH_CHAR) 
		strFilePath += SLASH_CHAR;

	return strFilePath + szUrlField;
}

//----------------------------------------------------------------------------------------------
CString GetHKLFileFromDocProfilePath(const CString& strProfilePath)
{
	if (strProfilePath.IsEmpty())
	{
		ASSERT(FALSE);
		return _T("");
	}
	CString strFilePath = strProfilePath;
	if (strFilePath.Right(1) != SLASH_CHAR) 
		strFilePath += SLASH_CHAR;

	return strFilePath + szHotKeyLink;
}

//-----------------------------------------------------------------------------
void GetAllExportProfiles(const CTBNamespace& nsDocument, CStringArray* pProfilesList, CPathFinder::PosType ePosType, const CString& strUserName)
{
	if (!nsDocument.IsValid() || !pProfilesList)
	{
		ASSERT(FALSE);
		return;
	}
	switch(ePosType)
	{
		case CPathFinder::STANDARD:
			AfxGetPathFinder()->GetProfilesFromPath(AfxGetPathFinder()->GetDocumentExportProfilesPath(nsDocument, CPathFinder::STANDARD), *pProfilesList, FALSE);
			break;
		case CPathFinder::ALL_USERS:
			AfxGetPathFinder()->GetProfilesFromPath(AfxGetPathFinder()->GetDocumentExportProfilesPath(nsDocument, CPathFinder::ALL_USERS), *pProfilesList, FALSE);
			AfxGetPathFinder()->GetProfilesFromPath(AfxGetPathFinder()->GetDocumentExportProfilesPath(nsDocument, CPathFinder::STANDARD), *pProfilesList, FALSE);
			break;
		case CPathFinder::USERS:	
			AfxGetPathFinder()->GetProfilesFromPath(AfxGetPathFinder()->GetDocumentExportProfilesPath(nsDocument, CPathFinder::USERS, (strUserName.IsEmpty()) ? AfxGetLoginInfos()->m_strUserName : strUserName), *pProfilesList, FALSE);
			AfxGetPathFinder()->GetProfilesFromPath(AfxGetPathFinder()->GetDocumentExportProfilesPath(nsDocument, CPathFinder::ALL_USERS), *pProfilesList, FALSE);
			AfxGetPathFinder()->GetProfilesFromPath(AfxGetPathFinder()->GetDocumentExportProfilesPath(nsDocument, CPathFinder::STANDARD), *pProfilesList, FALSE);
			break;
	}
}


//compara due stringhe senza tenere conto dei formattatori di stringa tab o cr
//se stringa 1 < stringa 2 ritorna -1 
//al contrario 1 
//se sono uguali 0
//----------------------------------------------------------------------------
int	CompareNoFormat(const CString& strString1, const CString& strString2)
{
	TCHAR seps[]   = _T(" 	\r\t\n");

	if	(strString1.IsEmpty() && !strString2.IsEmpty())
		return -1;

	if	(strString2.IsEmpty() && !strString1.IsEmpty())
		return 1;

	if (strString1.IsEmpty() && strString2.IsEmpty())
		return 0;

	CStringArray arTocken1, arTocken2;
	int nStrLen1, nStrLen2;
	TCHAR *nextToken;
	TCHAR *token = NULL;

	//memorizzo i tocken in due array di stringhe.
	token = _tcstok_s( (TCHAR*)(LPCTSTR)strString1, seps, &nextToken );
	while( token != NULL)
	{
		arTocken1.Add(token);
		
		/* Get next token: */
		token = _tcstok_s( NULL, seps, &nextToken);
	}
	nStrLen1 = arTocken1.GetSize();

	token = _tcstok_s( (TCHAR*)(LPCTSTR)strString2, seps, &nextToken);
	while( token != NULL)
	{
		arTocken2.Add(token);
		
		/* Get next token: */
		token = _tcstok_s( NULL, seps, &nextToken);
	}
	nStrLen2 = arTocken2.GetSize();

	//confronto gli array
	if (nStrLen1 < nStrLen2)
		return -1;

	if (nStrLen2 < nStrLen1)
		return 1;

	int nRes;
	for(int i = 0 ; i < nStrLen1 ; i++)
	{
		nRes = arTocken1.GetAt(i).CompareNoCase(arTocken2.GetAt(i));
		if (nRes)
			return nRes;
	}
	
	return 0;
}

//----------------------------------------------------------------------------------------------
CString	GetClientDocProfilesPath(const CTBNamespace& nsClientDoc)
{
	CString strDocumentPath = AfxGetPathFinder()->GetDocumentPath(nsClientDoc, CPathFinder::STANDARD);

	if (strDocumentPath.IsEmpty())
		return _T("");
	
	return strDocumentPath + SLASH_CHAR + szProfile + SLASH_CHAR;
}

//----------------------------------------------------------------------------------------------
CString	GetClientDocProfilesCustomPath(const CTBNamespace& nsClientDoc)
{
	CString strDocumentPath = AfxGetPathFinder()->GetDocumentPath(nsClientDoc, CPathFinder::CUSTOM);

	if (strDocumentPath.IsEmpty())
		return _T("");
	
	return strDocumentPath + SLASH_CHAR + szProfile + SLASH_CHAR;
}

//----------------------------------------------------------------------------------------------
CString GetSmartSchemaProfileFile(const CTBNamespace& nsDocument, const CString& strProfilePath)
{
	// se il profilo è vuoto, significa che sto utilizzando la descrizione del documento
	// come profilo
	CString strProfileName = CXMLProfileInfo::GetProfileNameFromPath(strProfilePath);
	CString strSchemaName = (strProfileName.IsEmpty()) 
							? nsDocument.GetObjectName(CTBNamespace::DOCUMENT)
							: strProfileName;

	strSchemaName.Replace(BLANK_CHAR, _T('_'));
	strSchemaName += szXsdExt;

	//controllo dove è posizionato il profilo
	CPathFinder::PosType ePosType = AfxGetPathFinder()->GetPosTypeFromPath(strProfilePath);
	CString strUser = AfxGetPathFinder()->GetUserNameFromPath(strProfilePath);
	//restituisco la path corrispondente del documento + nome dello schema
	return AfxGetPathFinder()->GetDocumentSchemaPath(nsDocument, ePosType, strUser) + SLASH_CHAR + strSchemaName;
}

//----------------------------------------------------------------------------------------------
CString MakeSmartSchemaProfileFile(const CTBNamespace& nsDocument, const CString& strProfileName, CPathFinder::PosType ePosType, const CString& strUserRole /*=_T("")*/)
{
	// se il profilo è vuoto, significa che sto utilizzando la descrizione del documento
	// come profilo
	CString strActualProfileName = (strProfileName.IsEmpty()) 
									? nsDocument.GetObjectName(CTBNamespace::DOCUMENT)
									: strProfileName;
	
	CString strDocPath = AfxGetPathFinder()->GetDocumentSchemaPath(nsDocument, ePosType, strUserRole);			
	return ::MakeFilePath(strDocPath, strActualProfileName, szXsdExt);
}

//--------------------------------------------------------------------
void GetXSLTInformation(const CString& strXSLTFileName, CString& strXSLTDescri, CTBNamespace& nsTransDoc)
{
	CString strNsTransDoc;
	strXSLTDescri = _T("");
	nsTransDoc.Clear();

	if (!ExistFile(strXSLTFileName))
		return;

	CXMLDocumentObject aXMLDoc;
	if (aXMLDoc.LoadXMLFile(strXSLTFileName))
	{
		aXMLDoc.AddSelectionNamespace(_T("xsl"),_T("http://www.w3.org/1999/XSL/Transform"));
		aXMLDoc.AddSelectionNamespace(_T("ns"), XTECH_NAMESPACE);
		//se esistel eggo la descrizione del foglio xslt dal nodo <XSLTDescription>  
		CXMLNode* pNsDocNode = aXMLDoc.SelectSingleNode(_T("//xsl:stylesheet/ns:XSLTDescription"));
			if (pNsDocNode)
			pNsDocNode->GetText(strXSLTDescri);
	
		//se esiste leggo il namespace del documento di destinazione del foglio xslt dal nodo <Namespace>
		//se esiste e non è presente il nodo <XSLTDescription> considero questo come descrizione del foglio XSLT
		pNsDocNode = aXMLDoc.SelectSingleNode(_T("//xsl:stylesheet/xsl:template/ns:DocumentInfo/xsl:for-each/xsl:choose/xsl:when/ns:Namespace"));
		if (pNsDocNode)
		{
				pNsDocNode->GetText(strNsTransDoc);
				nsTransDoc.SetNamespace(strNsTransDoc);
				if (strXSLTDescri.IsEmpty() && nsTransDoc.IsValid())
				{
					const CDocumentDescription* pDocDescri = AfxGetDocumentDescription(nsTransDoc);
					if (pDocDescri)
						strXSLTDescri = pDocDescri->GetTitle();
				}
		}
		

		if (pNsDocNode)
			delete pNsDocNode;
		aXMLDoc.Close();
	}	
	if (strXSLTDescri.IsEmpty())
		strXSLTDescri = ::GetName(strXSLTFileName);	
}




