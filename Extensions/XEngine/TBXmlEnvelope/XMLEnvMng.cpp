
#include "stdafx.h"

#include <io.h>

#include <TbGeneric\DocumentObjectsInfo.h>

#include <TbClientCore\ClientObjects.h>
#include <TbWebServicesWrappers\LoginManagerInterface.h>
#include <TbNameSolver\PathFinder.h>
#include <TbNameSolver\IFileSystemManager.h>

#include <TBGes\XmlGesInfo.h>
#include <TBGes\extdoc.h>

#include "XMLEnvelopeTags.h"
#include "GenFunc.h"
#include "XMLEnvInfo.h"
#include "XMLEnvMng.h"
#include "EnvelopeTree.h"
#include "XEngineObject.h"

#ifdef _DEBUG
#undef THIS_FILE
static const char THIS_FILE[] = __FILE__;
#endif

// definizioni delle directory di memorizzazione
static const TCHAR	szDataFolder	[]	= _T("Data");
static const TCHAR	szSchemaFolder	[]	= _T("Schema");
static const TCHAR	szLogFolder		[]	= _T("Logging");

static const TCHAR	szAll[]			= _T("\\*.*");

/////////////////////////////////////////////////////////////////////////////
// CXMLEnvElem implementation 
/////////////////////////////////////////////////////////////////////////////
IMPLEMENT_DYNAMIC(CXMLEnvElem, CObject)

//----------------------------------------------------------------------------
CXMLEnvElem::CXMLEnvElem(CXMLEnvSiteElem* pAncestor, const CString& strEnvName, const CString& strEnvFileName)
:
	m_pSiteAncestor		(pAncestor),
	m_pClassAncestor	(NULL),
	m_strEnvName		(strEnvName),	
	m_strEnvFileName	(strEnvFileName)
{
}

//----------------------------------------------------------------------------
CXMLEnvElem::CXMLEnvElem(CXMLEnvClassElem* pAncestor, const CString& strEnvName, const CString& strEnvFileName)
:
	m_pClassAncestor	(pAncestor),
	m_pSiteAncestor		(NULL),
	m_strEnvName		(strEnvName),	
	m_strEnvFileName	(strEnvFileName)
{
}

//----------------------------------------------------------------------------
CXMLEnvElem::CXMLEnvElem(const CXMLEnvElem& aXMLRXEnvElem)
{
	*this = aXMLRXEnvElem;
}

//----------------------------------------------------------------------------------------------
CXMLEnvElem& CXMLEnvElem::operator =(const CXMLEnvElem& aXMLRXEnvElem)
{
	if (this == &aXMLRXEnvElem)
		return *this;

	m_strEnvName	= aXMLRXEnvElem.m_strEnvName;
	m_strEnvFileName= aXMLRXEnvElem.m_strEnvFileName;
	m_pSiteAncestor	= aXMLRXEnvElem.m_pSiteAncestor;
	m_pClassAncestor= aXMLRXEnvElem.m_pClassAncestor;
	return *this;
}

/////////////////////////////////////////////////////////////////////////////
// CXMLEnvSiteElem implementation
/////////////////////////////////////////////////////////////////////////////
//
IMPLEMENT_DYNAMIC(CXMLEnvSiteElem, CObject)
//-----------------------------------------------------------------------------
CXMLEnvSiteElem::CXMLEnvSiteElem(CXMLEnvClassElem* pAncestor, const CString& strSiteName, const CTBNamespace& aNameSpace /*= NameSpace()*/ )
:
	m_pAncestor			(pAncestor),
	m_strSiteName		(strSiteName),	
	m_NameSpace			(aNameSpace)
{
	m_pEnvElemArray = new CXMLEnvElemArray;
}

//----------------------------------------------------------------------------
CXMLEnvSiteElem::CXMLEnvSiteElem(const CXMLEnvSiteElem& aXMLRXEnvSiteElem)
:
	m_pEnvElemArray	(NULL)	

{
	*this = aXMLRXEnvSiteElem;
}

//-----------------------------------------------------------------------------
CXMLEnvSiteElem::~CXMLEnvSiteElem()
{
	if (m_pEnvElemArray)
		delete m_pEnvElemArray;
}

//-----------------------------------------------------------------------------
BOOL CXMLEnvSiteElem::LoadInfoFromPath(LPCTSTR lpszRXEnvSitePath)
{
	CString strFilePath = lpszRXEnvSitePath;
	CString	strEnvFileName;
	BOOL bFound = FALSE;
	
	CStringArray arSubFolders;
	AfxGetFileSystemManager()->GetSubFolders (strFilePath, &arSubFolders);

	CString sPath (lpszRXEnvSitePath);
	if (sPath.Right(1) != SLASH_CHAR) 
		sPath += SLASH_CHAR;

	for (int i=0; i <= arSubFolders.GetUpperBound(); i++)
	{     
		strFilePath = sPath + arSubFolders.GetAt(i);

		strEnvFileName 	= strFilePath + SLASH_CHAR + ENV_XML_FILE_NAME + szXmlExt;	

		if (
				m_NameSpace.IsValid() &&
				IsValidEnvelope(strEnvFileName)
			)
		{
			bFound = TRUE;
			CXMLEnvElem* pEnvElem = new CXMLEnvElem(this, arSubFolders.GetAt(i), strEnvFileName);
			m_pEnvElemArray->Add(pEnvElem);
		}
	}
	
	return bFound;
}

//-----------------------------------------------------------------------------
BOOL CXMLEnvSiteElem::IsValidEnvelope(const CString& strEnvFileName)
{
	CXMLDocumentObject envObj(FALSE,FALSE,FALSE);
	if (!envObj.LoadXMLFile (strEnvFileName))
	{
		return FALSE;
	}

	CString strNameSpace;
	//cerco "//DocumentInfo/RootDocNamespace"
	CString strPrefix = GET_NAMESPACE_PREFIX((&envObj));
	CString strFilter = CString(URL_SLASH_CHAR) + URL_SLASH_CHAR + strPrefix + ENV_XML_DOC_INFO_TAG + URL_SLASH_CHAR + strPrefix + ENV_XML_ROOT_NS_TAG;
	
	CXMLNode *pNode = envObj.SelectSingleNode (strFilter, strPrefix);
	if (!pNode)
	{
		ASSERT(FALSE);
		return FALSE;
	}

	pNode->GetText (strNameSpace);
	SAFE_DELETE(pNode);
	
	return !strNameSpace.CompareNoCase(m_NameSpace.ToString());
}

//----------------------------------------------------------------------------------------------
CXMLEnvSiteElem& CXMLEnvSiteElem::operator =(const CXMLEnvSiteElem& aXMLRXEnvSiteElem)
{
	if (this == &aXMLRXEnvSiteElem)
		return *this;

	m_strSiteName		= aXMLRXEnvSiteElem.m_strSiteName ;
	m_NameSpace			= aXMLRXEnvSiteElem.m_NameSpace ;

	m_pEnvElemArray	= new CXMLEnvElemArray;
	
	if (aXMLRXEnvSiteElem.m_pEnvElemArray)
		for (int i = 0; i < aXMLRXEnvSiteElem.m_pEnvElemArray->GetSize(); i++)
		{
			CXMLEnvElem* pEnvElem = aXMLRXEnvSiteElem.m_pEnvElemArray->GetAt(i);
			if (pEnvElem)
			{
				CXMLEnvElem* pNewEl= new CXMLEnvElem(*pEnvElem);
				pNewEl->m_pSiteAncestor = this;
				m_pEnvElemArray->Add(pNewEl);
			}
		}		
	return *this;
}

/////////////////////////////////////////////////////////////////////////////
// CXMLEnvSiteElem implementation
/////////////////////////////////////////////////////////////////////////////
//

//-----------------------------------------------------------------------------
int CXMLEnvSiteArray::GetIndexByName(const CString& strSiteName) const
{
	if (strSiteName.IsEmpty()) return -1;

	for (int i =0 ; i <= GetUpperBound(); i++)
		if (GetAt(i)->m_strSiteName.CompareNoCase(strSiteName) == 0)
			return i;
	
	return -1;
}

//-----------------------------------------------------------------------------
CXMLEnvSiteElem* CXMLEnvSiteArray::GetSiteByName(const CString& strSiteName) const
{
	if (strSiteName.IsEmpty()) return NULL;

	int nIndex = GetIndexByName(strSiteName);
	
	return (nIndex > -1)
			? GetAt(nIndex)
			: NULL;
}

/////////////////////////////////////////////////////////////////////////////
// CXMLEnvClassElem implementation
/////////////////////////////////////////////////////////////////////////////
//
IMPLEMENT_DYNAMIC(CXMLEnvClassElem, CObject)
//-----------------------------------------------------------------------------
CXMLEnvClassElem::CXMLEnvClassElem(const CString& strEnvClass, BOOL bIsPending, BOOL bContainsEnvelopes)
:
	m_strEnvClass	(strEnvClass),
	m_bIsPending	(bIsPending),
	m_bContainsEnvelopes (bContainsEnvelopes)
{
	if (bContainsEnvelopes)
	{
		m_pEnvSiteArray = NULL;
		m_pEnvElemArray = new CXMLEnvElemArray;
	}
	else
	{
		 m_pEnvSiteArray = new CXMLEnvSiteArray;
		 m_pEnvElemArray = NULL;
	}
}

//----------------------------------------------------------------------------
CXMLEnvClassElem::CXMLEnvClassElem(const CXMLEnvClassElem& aXMLRXEnvClassElem)
{
	*this = aXMLRXEnvClassElem;
}

//-----------------------------------------------------------------------------
CXMLEnvClassElem::~CXMLEnvClassElem()
{
	if (m_pEnvSiteArray)
		delete m_pEnvSiteArray;
	if (m_pEnvElemArray)
		delete m_pEnvElemArray;
}

//-----------------------------------------------------------------------------
BOOL CXMLEnvClassElem::LoadInfoFromPath(LPCTSTR lpszRXEnvClassPath, const CTBNamespace& aNameSpace)
{
	CString strFilePath = lpszRXEnvClassPath;
	
	CStringArray arFolders;
	AfxGetFileSystemManager ()->GetSubFolders(strFilePath, &arFolders);

	if (!arFolders.GetSize())
		return FALSE;

	BOOL bOk = FALSE;

	CXMLEnvSiteElem* pEnvSite = new CXMLEnvSiteElem(this, _T("LOCALSITE"), aNameSpace);
	if (pEnvSite->LoadInfoFromPath(lpszRXEnvClassPath))
	{
		m_pEnvSiteArray->Add(pEnvSite);
		bOk=TRUE;
	}
	else
		delete pEnvSite;

	CStringArray arFiles;
	
	CString sPath (lpszRXEnvClassPath);
	if (sPath.Right(1) != SLASH_CHAR) 
		sPath += SLASH_CHAR;
	for (int i=0; i <= arFolders.GetUpperBound(); i++)
	{     
		strFilePath = sPath + arFolders.GetAt(i);

		if (m_bContainsEnvelopes)
		{
			arFiles.RemoveAll ();
			AfxGetFileSystemManager ()->GetFiles(strFilePath, CString(ENV_XML_FILE_NAME) + szXmlExt, &arFiles);

			if (arFiles.GetSize() <= 1) 
			{
				CXMLEnvElem* pEnvElem = new CXMLEnvElem(this, arFolders.GetAt(i), arFiles.GetAt(i));
				m_pEnvElemArray->Add(pEnvElem);
				bOk=TRUE;
			}
		}
		else
		{
			CXMLEnvSiteElem* pEnvSite = new CXMLEnvSiteElem(this, arFolders.GetAt(i), aNameSpace);
			if (pEnvSite->LoadInfoFromPath(strFilePath))
			{
				m_pEnvSiteArray->Add(pEnvSite);
				bOk=TRUE;
			}
			else
				delete pEnvSite;
		}
	}
	
	return bOk;
	
}

//----------------------------------------------------------------------------------------------
CXMLEnvClassElem& CXMLEnvClassElem::operator =(const CXMLEnvClassElem& aXMLRXEnvClassElem)
{
	if (this == &aXMLRXEnvClassElem)
		return *this;

	m_strEnvClass	= aXMLRXEnvClassElem.m_strEnvClass;
	m_bIsPending	= aXMLRXEnvClassElem.m_bIsPending;
	m_bContainsEnvelopes = aXMLRXEnvClassElem.m_bContainsEnvelopes;
	
	if (m_bContainsEnvelopes)
	{
		m_pEnvElemArray = new CXMLEnvElemArray(); 
		m_pEnvSiteArray = NULL;

		CXMLEnvElemArray* pOriginArray = aXMLRXEnvClassElem.m_pEnvElemArray;
		ASSERT(pOriginArray);

		for (int i = 0; i < pOriginArray->GetSize(); i++)
		{
			CXMLEnvElem* pEnvElem = pOriginArray->GetAt(i);
			if (pEnvElem)
			{
				CXMLEnvElem* pNewEnvElem = new CXMLEnvElem(*pEnvElem);
				pNewEnvElem->m_pClassAncestor = this;
				m_pEnvElemArray->Add(pNewEnvElem);
			}
		}
	}
	else
	{
		m_pEnvElemArray = NULL;
		m_pEnvSiteArray = new CXMLEnvSiteArray();

		CXMLEnvSiteArray* pOriginArray = aXMLRXEnvClassElem.m_pEnvSiteArray;
		ASSERT(pOriginArray);

		for (int i = 0; i < pOriginArray->GetSize(); i++)
		{
			CXMLEnvSiteElem* pEnvSite = pOriginArray->GetAt(i);
			if (pEnvSite)
			{
				CXMLEnvSiteElem* pNewEnvSite = new CXMLEnvSiteElem(*pEnvSite);
				pNewEnvSite->m_pAncestor = this;
				m_pEnvSiteArray->Add(pNewEnvSite);
			}
		}	
	}
	
	return *this;
}

/////////////////////////////////////////////////////////////////////////////
// CXMLEnvClassArray implementation
/////////////////////////////////////////////////////////////////////////////
//
//-----------------------------------------------------------------------------
CXMLEnvClassArray::CXMLEnvClassArray(const CString& strEnvClass /*= ""*/, const CTBNamespace& aNameSpace /*= NameSpace()*/)
:
	m_strEnvClass	(strEnvClass),
	m_NameSpace		(aNameSpace)
{
}

//----------------------------------------------------------------------------
CXMLEnvClassArray::CXMLEnvClassArray(const CXMLEnvClassArray& aXMLRXEnvClassArray)
{
	*this = aXMLRXEnvClassArray;
}

//-----------------------------------------------------------------------------
void CXMLEnvClassArray::Clear()
{
	m_strEnvClass.Empty();
	RemoveAll ();
}

//-----------------------------------------------------------------------------
int	CXMLEnvClassArray::GetIndexByName(const CString& strClassName) const
{
	if (strClassName.IsEmpty()) return -1;

	for (int i =0 ; i <= GetUpperBound(); i++)
		if (GetAt(i)->m_strEnvClass.CompareNoCase(strClassName) == 0)
			return i;
	
	return -1;
}

//-----------------------------------------------------------------------------
CXMLEnvClassElem*	CXMLEnvClassArray::GetClassByName(const CString& strClassName) const
{
	if (strClassName.IsEmpty()) return NULL;

	int nIndex = GetIndexByName(strClassName);
	
	return (nIndex > -1)
			? GetAt(nIndex)
			: NULL;
}

// la struttura da leggere è fissa:
// RX
//	EnvClass1
//		Site1
//			Env1
//			Env2
//		Site2
//			Env21
//			Env22
//	EnvClass2
//		Site1
//			Env21
//			Env22
//		Site2
//			Env221
//			Env222

//-----------------------------------------------------------------------------
BOOL CXMLEnvClassArray::LoadInfoFromPath(LPCTSTR lpszRXPath, BOOL bIsPending /*=FALSE*/, BOOL bContainsEnvelopes /*=FALSE*/)
{
	CString strFilePath = lpszRXPath;
	BOOL bOk = FALSE;

	CStringArray arFolders;
	AfxGetFileSystemManager ()->GetSubFolders(strFilePath, &arFolders);

	CString sPath (lpszRXPath);
	if (sPath.Right(1) != SLASH_CHAR) 
		sPath += SLASH_CHAR;

	for (int i=0; i <= arFolders.GetUpperBound(); i++)
	{     
		CString folderName = arFolders.GetAt (i);
		strFilePath = sPath + folderName;

		// se devo scegliere solo una specifica envelope class
		if (!m_strEnvClass.IsEmpty () &&
			folderName.CompareNoCase(m_strEnvClass) != 0 &&
			folderName.Find (HEADER_ENV_CLASS_SEPARATOR) == -1)
			continue;

		CXMLEnvClassElem* pEnvClass = new CXMLEnvClassElem(folderName, bIsPending, bContainsEnvelopes);
		if (pEnvClass->LoadInfoFromPath(strFilePath, m_NameSpace))
		{
			bOk = TRUE;
			Add(pEnvClass);
		}
		else
			delete pEnvClass;
	}
	
	return bOk;
}

// scarico in pRXSelectedElems tutti gli envelope disponibili
// (oppure solo quella indicata dal parametro pstrEnvFileName
//-----------------------------------------------------------------------------
UINT CXMLEnvClassArray::GetEnvelopeToImport(CXMLEnvElemArray* pRXSelectedElems, LPCTSTR pstrEnvFileName/*=NULL*/)
{
	CXMLEnvClassElem* pEnvClass = NULL;
	CXMLEnvSiteElem*	pEnvSite = NULL;
	CXMLEnvElem*		pEnvElem = NULL;

	int nResult = ENVELOPE_TO_IMPORT_NO_EXIST;
	
	// riempio l'array passato come argomento con tutti gli envelope disponibili
	for (int nClass = 0; nClass < GetSize(); nClass++)
	{
		pEnvClass = GetAt(nClass);
		if (pEnvClass && pEnvClass->m_pEnvSiteArray && pEnvClass->m_pEnvSiteArray->GetSize() > 0)
		{
			for (int nSite = 0; nSite < pEnvClass->m_pEnvSiteArray->GetSize(); nSite++)
			{
				pEnvSite = pEnvClass->m_pEnvSiteArray->GetAt(nSite);
				if (pEnvSite && pEnvSite->m_pEnvElemArray && pEnvSite->m_pEnvElemArray->GetSize() > 0)
				{
					for (int nEnv = 0; nEnv < pEnvSite->m_pEnvElemArray->GetSize(); nEnv++)
					{
						pEnvElem = pEnvSite->m_pEnvElemArray->GetAt(nEnv);
						if (pEnvElem && 
							(!pstrEnvFileName || !pEnvElem->m_strEnvFileName.CompareNoCase (pstrEnvFileName)))
						{
							pRXSelectedElems->Add(new CXMLEnvElem(pEnvSite, pEnvElem->m_strEnvName, pEnvElem->m_strEnvFileName));
							nResult = ENVELOPE_TO_IMPORT_EXIST;
						}
					}
				}
			}
		}
	}

	return nResult;
}

//----------------------------------------------------------------------------------------------
CXMLEnvClassArray& CXMLEnvClassArray::operator =(const CXMLEnvClassArray& aXMLRXEnvClassArray)
{
	if (this == &aXMLRXEnvClassArray)
		return *this;

	m_strEnvClass	= aXMLRXEnvClassArray.m_strEnvClass;
	m_NameSpace		= aXMLRXEnvClassArray.m_NameSpace;
	
	for (int i = 0; i < aXMLRXEnvClassArray.GetSize(); i++)
	{
		CXMLEnvClassElem* pEnvClass = aXMLRXEnvClassArray.GetAt(i);
		if (pEnvClass)
			Add(new CXMLEnvClassElem(*pEnvClass));
	}

	return *this;
}

	
/////////////////////////////////////////////////////////////////////////////
//		CXMLExpFileElem implementation
/////////////////////////////////////////////////////////////////////////////
//
//
//-----------------------------------------------------------------------------
CXMLExpFileElem::CXMLExpFileElem(const CString& strTitle, CString& strXMLFileName, int nPadding,  BOOL bConcat /*=FALSE*/)
	:
	m_nMaxRecNumb		(HEADER_MAX_DOCUMENT_NUM),		
	m_nMaxKByte			(HEADER_MAX_DOC_DIMENSION),
	m_strDocTitle		(strTitle),
	m_nPadding			(nPadding),
	m_nIncrementalKByte	(0),
	m_nCurrentKByte		(0),
	m_lCurrRecCount		(0),
	m_lCurrNumbFile		(1),
	m_lCurrBookmark		(0)	
{
	m_strXMLBaseFileName =  GetName(strXMLFileName);
	if (bConcat) 
	{
		CString strNewTitle = strTitle;
		strNewTitle.Replace(_T("/"), _T("-"));
		strNewTitle.Replace(_T("\\"), _T("-"));
		strNewTitle.Replace(_T("&"), _T("-"));
		strNewTitle.Replace(_T("."), _T("-"));
		m_strXMLBaseFileName += strNewTitle;
	}
	
	SetXMLCurrFileName();
}


//-----------------------------------------------------------------------------
CXMLExpFileElem::CXMLExpFileElem(const CXMLExpFileElem& aXMLExpFileElem)
	:
	m_nMaxRecNumb		(HEADER_MAX_DOCUMENT_NUM),		
	m_nMaxKByte			(HEADER_MAX_DOC_DIMENSION),
	m_nPadding			(0), 
	m_nIncrementalKByte	(0),
	m_nCurrentKByte		(0),	
	m_lCurrRecCount		(0),
	m_lCurrNumbFile		(1),
	m_lCurrBookmark		(0)
{
	*this = aXMLExpFileElem;
}


//-----------------------------------------------------------------------------
void CXMLExpFileElem::SetMaxDim(int nMaxRecNumb, int nMaxKByte)
{
	m_nMaxRecNumb = nMaxRecNumb;
	m_nMaxKByte	  = nMaxKByte;	
}

// questo metodo viene chiamato due volte
//	dopo la prima esportazione con bComputeIncr == FALSE
//  dopo la secondo esportazione con bComputeIncr == TRUE per calcolare di quanto
//  cresce il file ad ogni esportazione
//-----------------------------------------------------------------------------
void CXMLExpFileElem::SetIncrementalKByte(int nTotKByte, BOOL bComputeIncr /*=FALSE*/ )
{
	if (bComputeIncr)
		m_nIncrementalKByte = (nTotKByte > m_nCurrentKByte) ? nTotKByte - m_nCurrentKByte : 0;

	m_nCurrentKByte = nTotKByte;
}

//-----------------------------------------------------------------------------
void CXMLExpFileElem::SetXMLCurrFileName()
{
	CString strNumb, strPadding;
		
	if (m_lCurrNumbFile > 0)
	{
		strPadding.Format(_T("%%.%dd"), m_nPadding);
		strNumb.Format (strPadding, m_lCurrNumbFile);
	}

	m_strXMLCurrFileName = m_strXMLBaseFileName + strNumb;
}

//-----------------------------------------------------------------------------
BOOL CXMLExpFileElem::GetNextFileName(CString& strFileName)
{
	BOOL bNewFile = m_lCurrRecCount == 0;
	if (
			// è prioritaria la dimensione massima del file in termini di Kbyte
			(m_nCurrentKByte + m_nIncrementalKByte) > m_nMaxKByte ||
			(m_lCurrRecCount + 1) > m_nMaxRecNumb)
	{
		// se ho raggiunto il massimo numero di kbyte o quello di record consentiti 
		// allora devo generare un nuovo file
		m_lCurrNumbFile++;
		m_lCurrRecCount = 0;
		m_nCurrentKByte = 0;
		m_nIncrementalKByte = 0;
		SetXMLCurrFileName();
		
		bNewFile = TRUE;
	}
	strFileName = m_strXMLCurrFileName;
	return bNewFile;
}

//----------------------------------------------------------------------------------------------
BOOL CXMLExpFileElem::IsThisFileElem(const CString& strTitle, const CString& strXMLFileName)
{
	return (
				m_strDocTitle.CompareNoCase(strTitle) == 0 &&
				m_strXMLBaseFileName.Find(strXMLFileName) > 0
			);
}

//----------------------------------------------------------------------------------------------
CXMLExpFileElem& CXMLExpFileElem::operator =(const CXMLExpFileElem& aXMLExpFileElem)
{
	if (this == &aXMLExpFileElem)
		return *this;

	m_strDocTitle		 = aXMLExpFileElem.m_strDocTitle;
	m_strXMLBaseFileName = aXMLExpFileElem.m_strXMLBaseFileName;
	m_strXMLCurrFileName = aXMLExpFileElem.m_strXMLCurrFileName;
	m_nPadding			 = aXMLExpFileElem.m_nPadding;
	m_nMaxRecNumb		 = aXMLExpFileElem.m_nMaxRecNumb;
	m_nMaxKByte			 = aXMLExpFileElem.m_nMaxRecNumb;
	
	m_nIncrementalKByte	 = aXMLExpFileElem.m_nIncrementalKByte;
	m_nCurrentKByte		 = aXMLExpFileElem.m_nCurrentKByte;

	m_lCurrRecCount		 = aXMLExpFileElem.m_lCurrRecCount;
	m_lCurrNumbFile		 = aXMLExpFileElem.m_lCurrNumbFile;
	m_lCurrBookmark		 = aXMLExpFileElem.m_lCurrBookmark;

	return *this;
}


/////////////////////////////////////////////////////////////////////////////
//		CXMLExpFileManager implementation
/////////////////////////////////////////////////////////////////////////////
//
//-----------------------------------------------------------------------------
CXMLExpFileManager::CXMLExpFileManager(CXMLExpFileManager& aXMLExpFileManager)
{
	*this = aXMLExpFileManager;
}

//-----------------------------------------------------------------------------
int CXMLExpFileManager:: GetXMLExpFileIndex(const CString& strTitle, const CString& strXMLFileName)
{
	CString strName = GetName(strXMLFileName);
	for (int i = 0; i <= m_XMLExpFileNames.GetUpperBound(); i++)
	{
		CXMLExpFileElem* pElem =  (CXMLExpFileElem*)m_XMLExpFileNames.GetAt(i);
		if (	
				pElem && 
				pElem->m_strDocTitle.CompareNoCase(strTitle) == 0 &&
				(
					strName.CompareNoCase(pElem->m_strXMLBaseFileName) == 0 ||
					strName.Left(strName.GetLength() - pElem->m_nPadding).CompareNoCase(pElem->m_strXMLBaseFileName) == 0
				)
			)
			return i;
	}
	return -1;
}

//-----------------------------------------------------------------------------
CXMLExpFileElem* CXMLExpFileManager::GetXMLExpFileElem(const CString& strTitle, const CString& strXMLFileName)
{
	int nIndex = GetXMLExpFileIndex(strTitle, strXMLFileName);

	return (nIndex > -1)
			? (CXMLExpFileElem*)m_XMLExpFileNames.GetAt(nIndex)
			: NULL;
}
//-----------------------------------------------------------------------------
BOOL CXMLExpFileManager::IsUsedFileName(const CString& strTitle, const CString& strXMLFileName)
{
	CString strName = GetName(strXMLFileName);
	for (int i = 0; i <= m_XMLExpFileNames.GetUpperBound(); i++)
	{
		CXMLExpFileElem* pElem =  (CXMLExpFileElem*)m_XMLExpFileNames.GetAt(i);
		if (	
				pElem && 
				pElem->m_strDocTitle.CompareNoCase(strTitle) != 0 &&
				strName.CompareNoCase(pElem->m_strXMLBaseFileName) == 0
			)
			return TRUE;
	}
	return FALSE;
}


//-----------------------------------------------------------------------------
CXMLExpFileElem* CXMLExpFileManager::GetNextFileName(const CString& strTitle, CString& strXMLFileName, int nPadding, BOOL& bNew)
{
	if (strTitle.IsEmpty())
		return NULL;

	CXMLExpFileElem* pElem = GetXMLExpFileElem(strTitle, strXMLFileName);
	if (!pElem)		
	{
		// controllo se lo stesso nome file è utilizzato da altri documenti
		pElem = new CXMLExpFileElem(strTitle, strXMLFileName, nPadding, IsUsedFileName(strTitle, strXMLFileName));
		m_XMLExpFileNames.Add(pElem);
	}

	if (pElem)
		bNew = pElem->GetNextFileName(strXMLFileName);
	
	return pElem;
}

//-----------------------------------------------------------------------------
void CXMLExpFileManager::IncrementExpRecordCount(CXMLExpFileElem* pElem)
{
	if (pElem)		
		pElem->IncrementExpRecordCount();
}

//-----------------------------------------------------------------------------
long CXMLExpFileManager::GetNextBookmark(const CString& strTitle, const CString& strXMLFileName)
{
	CXMLExpFileElem* pElem = GetXMLExpFileElem(strTitle, strXMLFileName);
	return	(pElem)		
			? pElem->GetNextBookmark()
			: 0;
}

//----------------------------------------------------------------------------------------------
CXMLExpFileManager& CXMLExpFileManager::operator =(const CXMLExpFileManager& aXMLExpFileManager)
{
	if (this == &aXMLExpFileManager)
		return *this;

	for (int i =0; i < aXMLExpFileManager.m_XMLExpFileNames.GetSize(); i++)
	{
		CXMLExpFileElem* pElem = (CXMLExpFileElem*)aXMLExpFileManager.m_XMLExpFileNames.GetAt(i);
		if (pElem)
			m_XMLExpFileNames.Add(new CXMLExpFileElem(*pElem));
	}

	return *this;
}		

/////////////////////////////////////////////////////////////////////////////
// CXMLEnvelopeManager
/////////////////////////////////////////////////////////////////////////////
//

//-----------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(CXMLEnvelopeManager, CCmdTarget)

BEGIN_MESSAGE_MAP(CXMLEnvelopeManager, CCmdTarget)
	//{{AFX_MSG_MAP(CXMLEnvelopeManager)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()


//-----------------------------------------------------------------------------
CXMLEnvelopeManager::CXMLEnvelopeManager(CAbstractFormDoc *pDocument) 
	:
	m_pDocument				(pDocument),
	m_pXMLExpFileManager	(NULL),
	m_pXMLRXEnvClasses		(NULL),
	m_dwGetEnvThreadID		(0)
{	
	//preparo le info relative a DocumentInfo
	m_aXMLEnvInfo.SetDomainName(AfxGetDomainName());
	m_aXMLEnvInfo.SetSiteName(AfxGetSiteName());
	m_aXMLEnvInfo.SetSiteCode(AfxGetSiteCode());
	m_aXMLEnvInfo.SetUserName(AfxGetPathFinder()->ToUserDirectory(AfxGetLoginInfos()->m_strUserName));
	
	m_aXMLEnvInfo.SetCurrentDataTime();

	// creao l'ID da associare all'envelope (nome del folder di envelope)
	m_aXMLEnvInfo.SetExportID(CreateEnvName());

	m_strEnvFileName =	ENV_XML_FILE_NAME;
	
	if (GetExtension(m_strEnvFileName).IsEmpty())
		m_strEnvFileName += szXmlExt;
}

//-----------------------------------------------------------------------------
CXMLEnvelopeManager::CXMLEnvelopeManager(const CXMLEnvelopeManager& aEnvMng)
:
	m_pDocument			(NULL),
	m_pXMLExpFileManager(NULL),
	m_pXMLRXEnvClasses	(NULL)
{
	*this = aEnvMng;
}	

//-----------------------------------------------------------------------------
CXMLEnvelopeManager::~CXMLEnvelopeManager() 
{
	if (m_pXMLExpFileManager)
		delete m_pXMLExpFileManager;

	if (m_pXMLRXEnvClasses)
		delete m_pXMLRXEnvClasses;
}



//in fase di esportazione genero solo un envelope per volta
//-----------------------------------------------------------------------------
BOOL CXMLEnvelopeManager::SetTXEnvFolderPath(LPCTSTR lpszEnvClass, LPCTSTR lpszAlternativePath, const CTBNamespace& nsDocument)
{

	m_strEnvClass = lpszEnvClass;
	m_aXMLEnvInfo.SetEnvClass(m_strEnvClass);
	CString strSitePath = GetXMLTXTargetSitePath(AfxGetSiteName(), TRUE, FALSE);
	if (!m_strEnvClass.IsEmpty())
		strSitePath += m_strEnvClass;
	
	if (!lpszAlternativePath || strSitePath.IsEmpty())
		return FALSE;

	m_strTXEnvFolder =	(lpszAlternativePath && lpszAlternativePath[0]) ? lpszAlternativePath : strSitePath;

	if (m_strTXEnvFolder.Right(1) != SLASH_CHAR) 
		m_strTXEnvFolder += SLASH_CHAR;

	m_strTXEnvFolder +=  cwsprintf(_T("%s-%s"), nsDocument.GetObjectName(), m_strEnvName);

	::CompletePath(m_strTXEnvFolder, FALSE, TRUE);

	m_strEnvFileName =	ENV_XML_FILE_NAME;
	
	if (GetExtension(m_strEnvFileName).IsEmpty())
		m_strEnvFileName += szXmlExt;

	return TRUE;
}


//-----------------------------------------------------------------------------
CString	CXMLEnvelopeManager::GetTXEnvFolderPath(BOOL bCreate /*= TRUE*/) const
{
	return CompletePath(m_strTXEnvFolder, TRUE, bCreate);
}

//-----------------------------------------------------------------------------
CString	CXMLEnvelopeManager::GetTXEnvFolderDataPath(BOOL bCreate /*= TRUE*/) const
{ 
	return CompletePath(GetTXEnvFolderPath() + szDataFolder, TRUE, bCreate); 
}	

//-----------------------------------------------------------------------------
CString	CXMLEnvelopeManager::GetTXEnvFolderSchemaPath(BOOL bCreate /*= TRUE*/)	const
{ 
	return CompletePath(GetTXEnvFolderPath() + szSchemaFolder, TRUE, bCreate); 

}	

//-----------------------------------------------------------------------------
CString	CXMLEnvelopeManager::GetTXEnvFolderLoggingPath(BOOL bCreate /*= TRUE*/) const
{ 
	return CompletePath(GetTXEnvFolderPath(bCreate) + szLogFolder, TRUE, bCreate); 
}	

// per importazione

//-----------------------------------------------------------------------------
void CXMLEnvelopeManager::SetRXEnvFolderPath(LPCTSTR lpszPath)
{
	m_strRXEnvFolder = lpszPath;
}

//-----------------------------------------------------------------------------
CString	CXMLEnvelopeManager::GetRXEnvFolderPath(BOOL bCreate /*= TRUE*/) const
{
	return CompletePath(m_strRXEnvFolder, TRUE, bCreate); 
}

//-----------------------------------------------------------------------------
CString CXMLEnvelopeManager::GetRXEnvClassPath(BOOL bCreate /*= TRUE*/) const
{
	return CompletePath(GetXMLRXSourceSitePath(AfxGetSiteName(), TRUE, TRUE) + m_strEnvClass , TRUE, bCreate);
}

//-----------------------------------------------------------------------------
CString	CXMLEnvelopeManager::GetRXSenderSitePath(BOOL bCreate /*= TRUE*/) const
{
	return CompletePath(GetRXEnvClassPath(bCreate) + m_strSenderSite , TRUE, bCreate);
}

//-----------------------------------------------------------------------------
CString	CXMLEnvelopeManager::GetRXEnvFolderDataPath(BOOL bCreate /*= TRUE*/) const
{ 
	return CompletePath(GetRXEnvFolderPath() + szDataFolder , TRUE, bCreate); 
}	

//-----------------------------------------------------------------------------
CString	CXMLEnvelopeManager::GetRXEnvFolderSchemaPath(BOOL bCreate /*= TRUE*/)	const 
{
	return CompletePath(GetRXEnvFolderPath()  + szSchemaFolder, TRUE, bCreate); 
}	

//-----------------------------------------------------------------------------
CString	CXMLEnvelopeManager::GetRXEnvFolderLoggingPath(BOOL bCreate /*= TRUE*/) const
{ 
	return CompletePath(GetRXEnvFolderPath(bCreate) + szLogFolder, TRUE, bCreate); 
}	

//FAILURE PATH FUNCTIONS-------------------------------------------------------
//-----------------------------------------------------------------------------
CString CXMLEnvelopeManager::GetFailureEnvClassPath(BOOL bIsForImport /*= TRUE*/, BOOL bCreate /*= TRUE*/) const
{
	return CompletePath(GetXMLFailurePath (bIsForImport, TRUE, bCreate) + AfxGetSiteName() + SLASH_CHAR + m_strEnvClass , TRUE, bCreate);
}

//-----------------------------------------------------------------------------
CString	CXMLEnvelopeManager::GetFailureSenderSitePath(BOOL bIsForImport /*= TRUE*/, BOOL bCreate /*= TRUE*/) const
{
	return CompletePath(GetFailureEnvClassPath(bIsForImport, bCreate) + m_strSenderSite , TRUE, bCreate);
}


//-----------------------------------------------------------------------------
CString	CXMLEnvelopeManager::GetFailureEnvFolderPath(BOOL bIsForImport /*= TRUE*/, BOOL bCreate /*= TRUE*/) const
{
	return bIsForImport ?
		CompletePath(GetFailureSenderSitePath(bIsForImport, bCreate) + m_strEnvName , TRUE, bCreate):
		CompletePath(GetFailureEnvClassPath(bIsForImport, bCreate) + m_strEnvName , TRUE, bCreate);
}

//-----------------------------------------------------------------------------
CString	CXMLEnvelopeManager::GetFailureEnvFolderDataPath(BOOL bIsForImport /*= TRUE*/, BOOL bCreate /*= TRUE*/) const
{ 
	return CompletePath(GetFailureEnvFolderPath(bIsForImport, bCreate) + szDataFolder , TRUE, bCreate); 
}	

//-----------------------------------------------------------------------------
CString	CXMLEnvelopeManager::GetFailureEnvFolderSchemaPath(BOOL bIsForImport /*= TRUE*/, BOOL bCreate /*= TRUE*/)	const 
{
	return CompletePath(GetFailureEnvFolderPath(bIsForImport, bCreate)  + szSchemaFolder , TRUE, bCreate); 
}	

//-----------------------------------------------------------------------------
CString	CXMLEnvelopeManager::GetFailureEnvFolderLoggingPath(BOOL bIsForImport /*= TRUE*/, BOOL bCreate /*= TRUE*/)	const
{ 
	return CompletePath(GetFailureEnvFolderPath(bIsForImport, bCreate)  + szLogFolder, TRUE, bCreate); 
}

//PENDING PATH FUNCTIONS-------------------------------------------------------
//-----------------------------------------------------------------------------
CString CXMLEnvelopeManager::GetPendingEnvClassPath(BOOL bIsForImport /*= TRUE*/, BOOL bCreate /*= TRUE*/) const
{
	return CompletePath(GetXMLPendingPath (bIsForImport, TRUE, bCreate) + AfxGetSiteName() + SLASH_CHAR + m_strEnvClass , TRUE, bCreate);
}

//-----------------------------------------------------------------------------
CString	CXMLEnvelopeManager::GetPendingSenderSitePath(BOOL bIsForImport /*= TRUE*/, BOOL bCreate /*= TRUE*/) const
{
	return CompletePath(GetPendingEnvClassPath(bIsForImport, bCreate)  + m_strSenderSite, TRUE, bCreate);
}

//-----------------------------------------------------------------------------
CString	CXMLEnvelopeManager::GetPendingEnvFolderPath(BOOL bIsForImport /*= TRUE*/, BOOL bCreate /*= TRUE*/) const
{
	return bIsForImport ?
		CompletePath(GetPendingSenderSitePath(bIsForImport, bCreate)  + m_strEnvName , TRUE, bCreate) : 
		CompletePath(GetPendingEnvClassPath(bIsForImport, bCreate)  + m_strEnvName , TRUE, bCreate);
}

//-----------------------------------------------------------------------------
CString	CXMLEnvelopeManager::GetPendingEnvFolderDataPath(BOOL bIsForImport /*= TRUE*/, BOOL bCreate /*= TRUE*/) const
{ 
	return CompletePath(GetPendingEnvFolderPath(bIsForImport, bCreate) + szDataFolder , TRUE, bCreate); 
}	

//-----------------------------------------------------------------------------
CString	CXMLEnvelopeManager::GetPendingEnvFolderSchemaPath(BOOL bIsForImport /*= TRUE*/, BOOL bCreate /*= TRUE*/)	const 
{
	return CompletePath(GetPendingEnvFolderPath(bIsForImport, bCreate)  + szSchemaFolder , TRUE, bCreate); 
}	

//-----------------------------------------------------------------------------
CString	CXMLEnvelopeManager::GetPendingEnvFolderLoggingPath(BOOL bIsForImport /*= TRUE*/, BOOL bCreate /*= TRUE*/)	const
{ 
	return CompletePath(GetPendingEnvFolderPath(bIsForImport, bCreate)  + szLogFolder, TRUE, bCreate); 
}

//SUCCESS PATH FUNCTIONS-------------------------------------------------------
//-----------------------------------------------------------------------------
CString CXMLEnvelopeManager::GetSuccessEnvClassPath(BOOL bIsForImport /*= TRUE*/, BOOL bCreate /*= TRUE*/) const
{
	return CompletePath(GetXMLSuccessPath (bIsForImport, TRUE, bCreate) + AfxGetSiteName() + SLASH_CHAR + m_strEnvClass , TRUE, bCreate);
}

//-----------------------------------------------------------------------------
CString	CXMLEnvelopeManager::GetSuccessSenderSitePath(BOOL bIsForImport /*= TRUE*/, BOOL bCreate /*= TRUE*/) const
{
	return CompletePath(GetSuccessEnvClassPath(bIsForImport, bCreate)  + m_strSenderSite, TRUE, bCreate);
}

//-----------------------------------------------------------------------------
CString	CXMLEnvelopeManager::GetSuccessEnvFolderPath(BOOL bIsForImport /*= TRUE*/, BOOL bCreate /*= TRUE*/) const
{
	return bIsForImport ?
		CompletePath(GetSuccessSenderSitePath(bIsForImport, bCreate) + m_strEnvName , TRUE, bCreate) :
		CompletePath(GetSuccessEnvClassPath(bIsForImport, bCreate) + m_strEnvName , TRUE, bCreate);
}

//-----------------------------------------------------------------------------
CString	CXMLEnvelopeManager::GetSuccessEnvFolderDataPath(BOOL bIsForImport /*= TRUE*/, BOOL bCreate /*= TRUE*/) const
{ 
	return CompletePath(GetSuccessEnvFolderPath(bIsForImport, bCreate) + szDataFolder , TRUE, bCreate); 
}	

//-----------------------------------------------------------------------------
CString	CXMLEnvelopeManager::GetSuccessEnvFolderSchemaPath(BOOL bIsForImport /*= TRUE*/, BOOL bCreate /*= TRUE*/)	const 
{
	return CompletePath(GetSuccessEnvFolderPath(bIsForImport, bCreate)  + szSchemaFolder , TRUE, bCreate); 
}	

//-----------------------------------------------------------------------------
CString	CXMLEnvelopeManager::GetSuccessEnvFolderLoggingPath(BOOL bIsForImport /*= TRUE*/, BOOL bCreate /*= TRUE*/)	const
{ 
	return CompletePath(GetSuccessEnvFolderPath(bIsForImport, bCreate)  + szLogFolder, TRUE, bCreate); 
}

//PARTIAL SUCCESS PATH FUNCTIONS-------------------------------------------------------
//-----------------------------------------------------------------------------
CString CXMLEnvelopeManager::GetPartialSuccessEnvClassPath(BOOL bIsForImport /*= TRUE*/, BOOL bCreate /*= TRUE*/) const
{
	return CompletePath(GetXMLPartialSuccessPath (bIsForImport, TRUE, bCreate) + AfxGetSiteName() + SLASH_CHAR + m_strEnvClass , TRUE, bCreate);
}

//-----------------------------------------------------------------------------
CString	CXMLEnvelopeManager::GetPartialSuccessSenderSitePath(BOOL bIsForImport /*= TRUE*/, BOOL bCreate /*= TRUE*/) const
{
	return CompletePath(GetPartialSuccessEnvClassPath(bIsForImport, bCreate)  + m_strSenderSite, TRUE, bCreate);
}

//-----------------------------------------------------------------------------
CString	CXMLEnvelopeManager::GetPartialSuccessEnvFolderPath(BOOL bIsForImport /*= TRUE*/, BOOL bCreate /*= TRUE*/) const
{
	return bIsForImport ?
		CompletePath(GetPartialSuccessSenderSitePath(bIsForImport, bCreate) + m_strEnvName , TRUE, bCreate):
		CompletePath(GetPartialSuccessEnvClassPath(bIsForImport, bCreate) + m_strEnvName , TRUE, bCreate);
}

//-----------------------------------------------------------------------------
CString	CXMLEnvelopeManager::GetPartialSuccessEnvFolderDataPath(BOOL bIsForImport /*= TRUE*/, BOOL bCreate /*= TRUE*/) const
{ 
	return CompletePath(GetPartialSuccessEnvFolderPath(bIsForImport, bCreate) + szDataFolder , TRUE, bCreate); 
}	

//-----------------------------------------------------------------------------
CString	CXMLEnvelopeManager::GetPartialSuccessEnvFolderSchemaPath(BOOL bIsForImport /*= TRUE*/, BOOL bCreate /*= TRUE*/)	const 
{
	return CompletePath(GetPartialSuccessEnvFolderPath(bIsForImport, bCreate)  + szSchemaFolder , TRUE, bCreate); 
}	

//-----------------------------------------------------------------------------
CString	CXMLEnvelopeManager::GetPartialSuccessEnvFolderLoggingPath(BOOL bIsForImport /*= TRUE*/, BOOL bCreate /*= TRUE*/)	const
{ 
	return CompletePath(GetPartialSuccessEnvFolderPath(bIsForImport, bCreate)  + szLogFolder, TRUE, bCreate); 
}

//-----------------------------------------------------------------------------
CString	CXMLEnvelopeManager::CreateEnvName()
{
	if (m_strEnvName.IsEmpty())
	{
		m_strEnvName = cwsprintf 
					(
						_T("%s-%02d-%02d-%02d-%02d-%02d-%02d"),
						m_aXMLEnvInfo.m_aEnvDocInfo.m_strUserName,
						m_aXMLEnvInfo.m_aEnvDocInfo.m_DataTime.Year(),
						m_aXMLEnvInfo.m_aEnvDocInfo.m_DataTime.Month(),
						m_aXMLEnvInfo.m_aEnvDocInfo.m_DataTime.Day(),		
						m_aXMLEnvInfo.m_aEnvDocInfo.m_DataTime.Hour(),	
						m_aXMLEnvInfo.m_aEnvDocInfo.m_DataTime.Minute(),
						m_aXMLEnvInfo.m_aEnvDocInfo.m_DataTime.Second()

					);
	}

	return m_strEnvName;
}

//-----------------------------------------------------------------------------
CString	CXMLEnvelopeManager::CreateFullEnvFileName	(const CString& strEnvFolder)
{
	int nLastSlash = strEnvFolder.ReverseFind(SLASH_CHAR);
	
	CString retVal = strEnvFolder;
	if (nLastSlash != strEnvFolder.GetLength()) 
		retVal += SLASH_CHAR;
	
	retVal += ENV_XML_FILE_NAME;
	retVal += szXmlExt;
	return retVal;
}

// genera il nome del file di export
//-----------------------------------------------------------------------------
CXMLExpFileElem* CXMLEnvelopeManager::GetXMLExpFileName(const CString& strTitle, CString& strXMLFile, int nPadding, BOOL& bNew) 
{
	if (!m_pXMLExpFileManager)
		m_pXMLExpFileManager = new CXMLExpFileManager;


	CXMLExpFileElem* pElem =  m_pXMLExpFileManager->GetNextFileName(strTitle, strXMLFile, nPadding, bNew);

	strXMLFile = MakeFilePath(GetTXEnvFolderDataPath(), strXMLFile, szXmlExt);
	return pElem;
}


//-----------------------------------------------------------------------------
void CXMLEnvelopeManager::IncrementExpRecordCount(CXMLExpFileElem* pElem, const CString& strXMLFileName, int nDataInstancesNumb)
{
	if (!m_pXMLExpFileManager || !pElem) 
		return;

	m_pXMLExpFileManager->IncrementExpRecordCount(pElem);
	m_aXMLEnvInfo.IncrementExpRecordCount(strXMLFileName, nDataInstancesNumb);
}

//-----------------------------------------------------------------------------
long CXMLEnvelopeManager::GetNextBookmark(const CString& strTitle, const CString& strXMLFileName)
{
	return  (m_pXMLExpFileManager)
			? m_pXMLExpFileManager->GetNextBookmark(strTitle, strXMLFileName)
			: 0;
}

//-----------------------------------------------------------------------------
BOOL CXMLEnvelopeManager::ReadEnvelope(CXMLDocumentObject* pXMLDoc /*= NULL*/)
{
	CString m_strFile = GetPendingEnvFolderPath() + m_strEnvFileName;
	return m_aXMLEnvInfo.Parse(m_strFile, pXMLDoc);
}

//-----------------------------------------------------------------------------
BOOL CXMLEnvelopeManager::CreateEnvelope(BOOL bDisplayMsgBox /* = TRUE*/, const CString& strPath /*=""*/, BOOL bCreateSchema /*=FALSE*/)
{
	if (m_strEnvName.IsEmpty() || m_strEnvClass.IsEmpty())
	{
		ASSERT(FALSE);
		return FALSE;
	}
	CString strEnvPath = strPath.IsEmpty () ? GetTXEnvFolderPath() : strPath;
	return m_aXMLEnvInfo.Unparse(strEnvPath + m_strEnvFileName, bDisplayMsgBox, bCreateSchema);
}

//-----------------------------------------------------------------------------
BOOL CXMLEnvelopeManager::DropEnvelope(const CString& strPath /*=""*/)
{
	if (m_strEnvName.IsEmpty() || m_strEnvClass.IsEmpty())
	{
		ASSERT(FALSE);
		return FALSE;
	}

	CString strEnvPath = strPath.IsEmpty () ? GetTXEnvFolderPath() : strPath;
	return RemoveFolderTree(strEnvPath);
}




//-----------------------------------------------------------------------------
BOOL CXMLEnvelopeManager::LoadRXEnvClassArray(LPCTSTR lpszEnvClass, const CTBNamespace& aNameSpace)
{
	if (m_pXMLRXEnvClasses)
		delete m_pXMLRXEnvClasses;
	
	m_pXMLRXEnvClasses = new CXMLEnvClassArray(lpszEnvClass, aNameSpace);

	return m_pXMLRXEnvClasses->LoadInfoFromPath(GetXMLRXSourceSitePath(AfxGetSiteName()));
}

//-----------------------------------------------------------------------------
BOOL CXMLEnvelopeManager::LoadPendingEnvClassArray(LPCTSTR lpszEnvClass, const CTBNamespace& aNameSpace)
{
	if (m_pXMLRXEnvClasses)
		delete m_pXMLRXEnvClasses;
	
	m_pXMLRXEnvClasses = new CXMLEnvClassArray(lpszEnvClass, aNameSpace);

	return m_pXMLRXEnvClasses->LoadInfoFromPath(GetXMLPendingSitePath(TRUE, AfxGetSiteName()), TRUE);
}

//-----------------------------------------------------------------------------
BOOL CXMLEnvelopeManager::LoadBothEnvClassArray(LPCTSTR lpszEnvClass, const CTBNamespace& aNameSpace)
{
	if (m_pXMLRXEnvClasses)
		delete m_pXMLRXEnvClasses;
	
	m_pXMLRXEnvClasses = new CXMLEnvClassArray(lpszEnvClass, aNameSpace);

	BOOL bResult = m_pXMLRXEnvClasses->LoadInfoFromPath(GetXMLPendingSitePath(TRUE, AfxGetSiteName()), TRUE);
	
	return m_pXMLRXEnvClasses->LoadInfoFromPath(GetXMLRXSourceSitePath(AfxGetSiteName())) || bResult;
}
//-----------------------------------------------------------------------------
BOOL CXMLEnvelopeManager::FillSelection(CXMLEnvElemArray* pRXSelectedElems, LPCTSTR strEnvFolder, CAbstractFormDoc* pDoc)
{
	if (!pRXSelectedElems || !pDoc)
	{
		ASSERT(FALSE);
		return FALSE;
	}

	pRXSelectedElems->RemoveAll ();

	CString strEnvClass, strSenderSite;
	CString strEnvFile = CreateFullEnvFileName(strEnvFolder);
	CTBNamespace aNameSpace;
	if (!GetInfoFromEnvelope(strEnvFile, strEnvClass, strSenderSite, aNameSpace))
		return FALSE;

	if (aNameSpace != pDoc->GetNamespace())
		return FALSE;
	
	LoadBothEnvClassArray((LPCTSTR)strEnvClass, aNameSpace);

	if (GetEnvelopeToImport(pRXSelectedElems, (LPCTSTR)strEnvFile) == ENVELOPE_TO_IMPORT_NO_EXIST)
	{
		CXMLEnvClassElem* pEnvClass = new CXMLEnvClassElem(strEnvClass, FALSE, FALSE);
		m_pXMLRXEnvClasses->Add(pEnvClass);

		CXMLEnvSiteElem*	pEnvSite = new CXMLEnvSiteElem(pEnvClass, strSenderSite, aNameSpace );
		pEnvClass->m_pEnvSiteArray->Add(pEnvSite);

		pRXSelectedElems->Add(new CXMLEnvElem(pEnvSite, GetName(strEnvFolder), strEnvFile));				
	}
	return TRUE;
}

//----------------------------------------------------------------------------------------------
void CXMLEnvelopeManager::MoveEnvelopesToRXPath	()
{
	TRXEParameters aTR(m_pDocument);
	if (aTR.FindRecord () != TableReader::FOUND)
		return;	
	
	CString strFilePath = aTR.GetRecord()->f_ImportPath.GetString ();
	
	MoveEnvelopesToRXPath(strFilePath);
}

//----------------------------------------------------------------------------------------------
void CXMLEnvelopeManager::MoveEnvelopesToRXPath	(const CString &strPath)
{
	if (strPath.IsEmpty () || !ExistPath(strPath))
		return;

	CStringArray arSubFolders;
	AfxGetFileSystemManager()->GetSubFolders (strPath, &arSubFolders);

	CString	strEnvFileName, strDestinationPath, strFilePath;
	CTBNamespace aNameSpace;
	
	for (int i=0; i <= arSubFolders.GetUpperBound(); i++)
	{     
		strFilePath = strPath + SLASH_CHAR + arSubFolders.GetAt(i);

		strEnvFileName 	= strFilePath + SLASH_CHAR + ENV_XML_FILE_NAME + szXmlExt;	
		if (GetInfoFromEnvelope(strEnvFileName, m_strEnvClass, m_strSenderSite, aNameSpace))
		{
			strDestinationPath = GetRXSenderSitePath() + arSubFolders.GetAt(i);
			if (CopyFolderTree(strFilePath, strDestinationPath, TRUE, FALSE))
				RemoveFolderTree(strFilePath, TRUE);
		}
	}	
}

//----------------------------------------------------------------------------------------------
BOOL CXMLEnvelopeManager::GetInfoFromEnvelope(const CString& strEnvFileName, CString& m_strEnvClass, CString& m_strSenderSite, CTBNamespace& aNameSpace)
{	
	if (!m_aXMLEnvInfo.Parse(strEnvFileName, NULL))
		return FALSE;
	
	m_strEnvClass = m_aXMLEnvInfo.GetEnvClass();
	m_strSenderSite = m_aXMLEnvInfo.GetSiteName();
	aNameSpace = m_aXMLEnvInfo.GetRootNameSpace();

	//controllo che l'envelopeclass e il namespace siano quelle del documento di cui sto effettuando l'importazione
	return m_pDocument->GetNamespace() == aNameSpace;
}

//----------------------------------------------------------------------------------------------
UINT CXMLEnvelopeManager::GetEnvelopeToImport(CXMLEnvElemArray* pRXSelectedElems, LPCTSTR pstrEnvFileName/*=NULL*/)
{
	if (!m_pXMLRXEnvClasses)
		return ENVELOPE_TO_IMPORT_ERROR;
	
	if (m_pXMLRXEnvClasses->GetSize() < 1)
		return ENVELOPE_TO_IMPORT_NO_EXIST;

	return m_pXMLRXEnvClasses->GetEnvelopeToImport(pRXSelectedElems, pstrEnvFileName);
}

//----------------------------------------------------------------------------------------------
CXMLEnvelopeManager& CXMLEnvelopeManager::operator =(const CXMLEnvelopeManager& aXMLEnvMng)
{
	if (m_pXMLExpFileManager)
		delete m_pXMLExpFileManager;
	m_pXMLExpFileManager = NULL;

	if (m_pXMLRXEnvClasses)
		delete m_pXMLRXEnvClasses;
	m_pXMLRXEnvClasses	 = NULL;
	
	m_aXMLEnvInfo		= aXMLEnvMng.m_aXMLEnvInfo;

	m_pDocument			= aXMLEnvMng.m_pDocument;
	m_strEnvClass		= aXMLEnvMng.m_strEnvClass;	
	m_strEnvFileName	= aXMLEnvMng.m_strEnvFileName;

	
	m_strTXEnvFolder	= aXMLEnvMng.m_strTXEnvFolder;
	m_strRXEnvFolder	= aXMLEnvMng.m_strRXEnvFolder;


	if (aXMLEnvMng.m_pXMLExpFileManager)
		m_pXMLExpFileManager = new CXMLExpFileManager(*aXMLEnvMng.m_pXMLExpFileManager);

	if (aXMLEnvMng.m_pXMLRXEnvClasses)
		m_pXMLRXEnvClasses = new CXMLEnvClassArray(*aXMLEnvMng.m_pXMLRXEnvClasses);

	return *this;
}

