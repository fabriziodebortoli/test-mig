
#include "StdAfx.h"

#include <TbXmlCore\XmlGeneric.h>

#include <TbGeneric\CollateCultureFunctions.h>
#include <TbGeneric\JsonFormEngine.h>
#include <TbGeneric\WndObjDescription.h>

#include <TbNameSolver\PathFinder.h>
#include <TbNameSolver\ThreadContext.h>
#include <TbNameSolver\ApplicationContext.h>
#include <TbNameSolver\Chars.h>
#include <TbNameSolver\TBNamespaces.h>
#include <TbNameSolver\FileSystemFunctions.h>
#include <TbNameSolver\IFileSystemManager.h>
#include "tbstrings.h"
#include "GeneralFunctions.h"
#include "globals.h"

static const TCHAR szNamespace[]		= _T("urn:TBStringLoader");
static const TCHAR szTrue[]				= _T("true");
static const TCHAR szLocalizable[]		= _T("localizable");
static const TCHAR szDictionary[]		= _T("dictionary");
static const TCHAR szBaseLocalize[]		= _T("baseLocalize");

static const TCHAR szCNPrefix[]			= _T("zh");
static const TCHAR szENPrefix[]			= _T("en");
static const TCHAR szPrefix[]	= _T("sl");

//****************************************************************
//*****************INLINE GLOBAL FUNCTIONS************************
//****************************************************************
//-----------------------------------------------------------------------------
CString AFXAPI AfxLoadTBString(UINT	IdString, HINSTANCE hinstance/*=NULL*/)
{ 
	CString strToLoad;
	//se non ho la parte alta (ID della DLL ed ID del file) controllo se è una stringa da risorsa
	if (GetNumIDS(IdString) <= 0x7FFF) 
	{
		if (IdString < MinTbCommand)
		{
			//stringhe in risorsa
			if (hinstance)
			{
				if (!strToLoad.LoadString(hinstance, IdString))
				{
					hinstance = NULL;
					strToLoad.LoadString(IdString);
				}
			}
			else
			{
				strToLoad.LoadString(IdString);
			}
		}
		else 
		{
#ifdef _DEBUG
			CJsonResource sId = AfxGetTBResourcesMap()->DecodeID(TbCommands, IdString);
#endif
			//stringhe in json
			CStringEntry entry;// = AfxGetTBResourcesMap()->GetString(IdString);
			if (entry.lpszString[0])//gestione delle stringtable json non più supportata
			{//ho trovato la stringa json, provo a tradurla
				CStringLoader *pStrLoader = AfxGetStringLoader();
				if (!pStrLoader)
					return entry.lpszString;

				CStringArray paths;
				CString sPath;
				if (GetDriver(entry.lpszFile).IsEmpty())
				{
					int nPos = 0;
					CString sFile = entry.lpszFile;
					CString app = sFile.Tokenize(_T("\\"), nPos);
					CString mod = sFile.Tokenize(_T("\\"), nPos);

					CTBNamespace ns(CTBNamespace::MODULE, app + _T(".") + mod);
					sPath = AfxGetPathFinder()->GetDictionaryPathFromNamespace(ns, FALSE);
				}
				else
				{
					sPath = AfxGetPathFinder()->GetDictionaryPathFromFileName(entry.lpszFile);
				}

				if (sPath.IsEmpty())
					return entry.lpszString;
				paths.Add(sPath);
				return pStrLoader->LoadSourceString(entry.lpszString, entry.lpszFile, paths);
			}
			else
			{
				//non l'ho trovata, provo a caricarla da risorsa
				strToLoad.LoadString(IdString);
				hinstance = AfxFindStringResourceHandle(IdString);
			}
			
		}
	}
	else
		ASSERT_TRACE(FALSE,"Old string format in ST files no more supported");	//la vecchia logica con le stringhe in file ST non è più supportata!
	
	if (!strToLoad.IsEmpty())
	{
		CStringLoader *pStrLoader = AfxGetStringLoader(); 
		if (pStrLoader)
		{
			CStringArray paths;
			if (hinstance)
				AfxGetDictionaryPathsFormDllInstance(hinstance, paths);
			else
				AfxGetDictionaryPathsFromID (IdString, RT_STRING, paths);
			
			return pStrLoader->LoadResourceString (strToLoad, IdString, paths);
		}
	}
	return strToLoad;
}              

//-----------------------------------------------------------------------------
BOOL AFXAPI AfxLoadTBString(CString& strToLoad, UINT IdString, HINSTANCE hinstance/*=NULL*/) 
{ 
	strToLoad = AfxLoadTBString(IdString, hinstance);
		
	return !strToLoad.IsEmpty();
}  



//-----------------------------------------------------------------------------
void AFXAPI AfxLoadWindowStrings(CWnd* pWnd, LPCTSTR lpcszTemplate)
{
	ASSERT_TRACE(pWnd,"Parameter pWnd cannot be null");
	UINT nIDD = REVERSEMAKEINTRESOURCE(lpcszTemplate);
	if (IS_INTRESOURCE(nIDD))
	{
		CStringLoader *pStrLoader = AfxGetStringLoader(); 
		if (pStrLoader)
		{
			CStringArray paths;
			AfxGetDictionaryPathsFromID (nIDD, RT_DIALOG, paths);
			pStrLoader->LoadWindowStrings (pWnd, paths, nIDD); 
		}
	}
	else
		ASSERT_TRACE1(IS_INTRESOURCE(nIDD),"Resource associated to template %s not found or bad type",lpcszTemplate);
}
//-----------------------------------------------------------------------------
CString	AFXAPI AfxLoadJsonString(LPCTSTR lpcszBaseString, CWndObjDescription* pDescription)
{
	if (!lpcszBaseString || !lpcszBaseString[0])
		return _T("");
	CString sCulture = AfxGetCulture();
	if (sCulture.IsEmpty())
		return lpcszBaseString;
	CStringLoader *pStrLoader = AfxGetStringLoader(); 
	if (pStrLoader)
	{
		CArray<CJsonResource*> arFiles;
		pDescription->GetResources(arFiles);
		for (int i = 0; i < arFiles.GetCount(); i++)
		{
			CJsonResource* res = arFiles[i];
			CString path = AfxGetDictionaryPathFromNamespace(res->GetOwnerNamespace(), FALSE);
			if (path.IsEmpty())
				return lpcszBaseString;
			CString sFile = res->GetFile();
			CString sFileName = GetName(sFile);
			CString sPath = GetPath(sFile);
			CString sFolder = res->m_strJsonContext;
			if (sFolder.IsEmpty())
			{
				sFolder = GetName(sPath);
				if (sFolder == szJsonForms)
					sFolder = GetName(GetPath(sPath));
			}
			//se è cambiata la culture, devo buttare via le traduzioni in cache
			if (res->m_sPrivateDataCulture != sCulture)
			{
				res->m_sPrivateDataCulture = sCulture;
				res->m_pTranslationPrivateData = NULL;
			}
			CString sTarget;
			if (pStrLoader->LoadJsonFormString(lpcszBaseString, sFolder, sFileName, path, res->m_pTranslationPrivateData, sTarget))
				return sTarget;
		}
		
	}
	return lpcszBaseString;
}
//-----------------------------------------------------------------------------
void AFXAPI AfxLoadMenuStrings(CMenu* pMenu, UINT nIDD)
{
	ASSERT_TRACE(pMenu,"Parameter pMenu cannot be null");
	if (IS_INTRESOURCE(nIDD))
	{
		CStringLoader *pStrLoader = AfxGetStringLoader(); 
		if (pStrLoader)
		{
			CStringArray paths;
			AfxGetDictionaryPathsFromID(nIDD, RT_MENU, paths);
			pStrLoader->LoadMenuStrings (pMenu, paths, nIDD); 
		}
	}
	else
		ASSERT_TRACE1(IS_INTRESOURCE(nIDD),"Resource ID = %d not found",nIDD);
}


// questa funzione non si trova nel path finder per "solidarietà" con la precedente
//-----------------------------------------------------------------------------
void AFXAPI AfxGetDictionaryPathsFromSourceString(LPCTSTR lpcszString, CStringArray& paths)	
{
	if (AfxGetCulture().IsEmpty())
		return;

	AfxGetPathFinder()->GetDictionaryPathsFromString(lpcszString, paths);
}

//-----------------------------------------------------------------------------
// questa funzione non si trova nel path finder perché ha bisogno del concetto di
// AddOn Module, sconosciuto al livello del name solver
void AFXAPI AfxGetDictionaryPathsFromID(UINT nIDD, LPCTSTR lpszType, CStringArray& paths)	
{
	if (AfxGetCulture().IsEmpty()) return;

	AfxGetPathFinder()->GetDictionaryPathsFromID(nIDD, lpszType, paths);
}
//-----------------------------------------------------------------------------
void AFXAPI AfxGetDictionaryPathsFormDllInstance(HINSTANCE hDllInstance, CStringArray &paths)
{
	if (AfxGetCulture().IsEmpty()) return;
	AfxGetPathFinder()->GetDictionaryPathsFormDllInstance(hDllInstance, paths);
}

//-----------------------------------------------------------------------------
CString AFXAPI AfxGetDictionaryPathFromNamespace(const CTBNamespace& aNamespace, BOOL bStandard)
{
	if (AfxGetCulture().IsEmpty()) return _T("");	

	return AfxGetPathFinder()->GetDictionaryPathFromNamespace(aNamespace, bStandard);

}

//-----------------------------------------------------------------------------
CString AFXAPI AfxGetDictionaryPathFromTableName(const CString& strTableName)
{
	if (AfxGetCulture().IsEmpty()) return _T("");	

	return AfxGetPathFinder()->GetDictionaryPathFromTableName(strTableName);
}

//-----------------------------------------------------------------------------
CStringLoader* AFXAPI AfxGetStringLoader()
{ 
	return AfxGetApplicationContext()->GetObject<CStringLoader>(&CApplicationContext::GetStringLoader);
}

//-----------------------------------------------------------------------------
CString	AFXAPI AfxGetCulture()	
{
	return AfxGetThreadContext()->GetUICulture();
}

// indica che siamo in cultura cinese
//-----------------------------------------------------------------------------
BOOL AFXAPI AfxIsChineseCulture()
{
	return !AfxGetCulture().IsEmpty() && AfxGetCulture().Left(2).CompareNoCase (szCNPrefix) == 0;
}

// indica che siamo in cultura inglese
//-----------------------------------------------------------------------------
BOOL AFXAPI AfxIsEnglishCulture()
{
	return AfxIsEnglishCulture(AfxGetCulture());
}

// indica che è cultura inglese
//-----------------------------------------------------------------------------
BOOL AFXAPI AfxIsEnglishCulture(const CString& strCulture)
{
	return !strCulture.IsEmpty() && strCulture.Left(2).CompareNoCase (szENPrefix) == 0;
}

//-----------------------------------------------------------------------------
CString AFXAPI AfxLoadSourceString(LPCTSTR lpcszBaseString, LPCSTR lpszFile)
{
	CStringLoader *pStrLoader = AfxGetStringLoader();   
	if (pStrLoader)
	{
		CString strFileName(lpszFile);
		CStringArray paths;
		AfxGetDictionaryPathsFromSourceString(lpcszBaseString, paths);
		return FixBareLF(pStrLoader->LoadSourceString (lpcszBaseString, ::GetName (strFileName), paths));
	}
	
	return FixBareLF(lpcszBaseString);
}

//-----------------------------------------------------------------------------
CString	AFXAPI AfxLoadEnumString(LPCTSTR lpcszBaseString, LPCTSTR lpszName, const CTBNamespace &aNamespace)
{
	CStringLoader *pStrLoader = AfxGetStringLoader();  
	if (pStrLoader)
		return pStrLoader->LoadEnumString(lpcszBaseString, lpszName, AfxGetDictionaryPathFromNamespace(aNamespace, TRUE));
	
	return lpcszBaseString;
}

//-----------------------------------------------------------------------------
CString	AFXAPI AfxLoadFontString(LPCTSTR lpcszBaseString, const CTBNamespace &aNamespace)
{
	CStringLoader *pStrLoader = AfxGetStringLoader();  
	if (pStrLoader)
		return pStrLoader->LoadFontString(lpcszBaseString, AfxGetDictionaryPathFromNamespace(aNamespace, TRUE));
	
	return lpcszBaseString;
}

//-----------------------------------------------------------------------------
CString	AFXAPI AfxLoadFormatterString(LPCTSTR lpcszBaseString, const CTBNamespace &aNamespace)
{
	CStringLoader *pStrLoader = AfxGetStringLoader();  
	if (pStrLoader)
		return pStrLoader->LoadFormatterString(lpcszBaseString, AfxGetDictionaryPathFromNamespace(aNamespace, TRUE));
	
	return lpcszBaseString;
}

//-----------------------------------------------------------------------------
CString	AFXAPI AfxLoadXMLString(LPCTSTR lpcszBaseString, LPCTSTR lpszFileName, LPCTSTR lpszDictionaryPath)
{
	CStringLoader *pStrLoader = AfxGetStringLoader();  
	if (pStrLoader)
		return pStrLoader->LoadXMLString(lpcszBaseString, lpszFileName, lpszDictionaryPath );
	
	return lpcszBaseString;
}

//-----------------------------------------------------------------------------
static LOADDATABASESTRINGS g_pLoadDatabaseStrings = NULL;
//-----------------------------------------------------------------------------
BOOL AfxInitLoadDatabaseStringFunction(LOADDATABASESTRINGS pFunction)
{
	g_pLoadDatabaseStrings = pFunction;
	return TRUE;
}

//-----------------------------------------------------------------------------
CString AfxBaseLoadDatabaseString(LPCTSTR lpcszBaseString, LPCTSTR lpszTableName)
{
	return g_pLoadDatabaseStrings 
		? g_pLoadDatabaseStrings(lpcszBaseString, lpszTableName)
		: lpcszBaseString;
}


//*****************************************************************************
// CLocalizableXMLDocument 
//*****************************************************************************
//-----------------------------------------------------------------------------
CLocalizableXMLDocument::CLocalizableXMLDocument (const CTBNamespace& aNamespace, CPathFinder* pPathFinder)
{
	ASSERT_TRACE1(aNamespace.IsValid(),"Namespace %s not valid",((LPCTSTR)aNamespace.ToString()));
	ASSERT_TRACE(pPathFinder,"Parameter pPathFinder cannot be null");

	m_Namespace = aNamespace;
	m_pPathFinder = pPathFinder;
	
} 

//-----------------------------------------------------------------------------
BOOL CLocalizableXMLDocument::LoadXMLFile (const CString& strFileName)
{
	m_sFileName		= strFileName;
	if (!__super::LoadXMLFile(strFileName))
		return FALSE;
	CStringLoader *pStrLoader = AfxGetStringLoader();  
	if (!pStrLoader)
		return TRUE;
		
	CString strFile = GetName(m_sFileName);
	CString strDictionaryPath = GetDictionaryPath();
	CXMLNodeChildsList *pList = SelectNodes(_T("//node()[@localizable='true']/text()"));
	for (int i = 0; i < pList->GetSize(); i++)
	{
		CXMLNode* pTextNode = pList->GetAt(i);
		CString strNodeText;
		pTextNode->GetNodeValue(strNodeText);
		CString strLocalizedText = AfxLoadXMLString(strNodeText, strFile, strDictionaryPath);
		
		CXMLNode *pParentNode = pTextNode->GetParentNode();
		CString strNodeName;
		pParentNode->GetName(strNodeName);
		pParentNode->SetAttribute(_T("sl:") + strNodeName, strNodeText);
		pTextNode->SetNodeValue(strLocalizedText);
				
	}
	delete pList;
			

	pList = SelectNodes(_T("//node()[@localize]"));
	for (int i = 0; i < pList->GetSize(); i++)
	{
		CXMLNode* pNode = pList->GetAt(i);
		CString strNodeText;
		pNode->GetAttribute(_T("localize"), strNodeText);
		CString strLocalizedText = AfxLoadXMLString(strNodeText, strFile, strDictionaryPath);
		
		pNode->SetAttribute(_T("sl:baseLocalize"), strNodeText);
		pNode->SetAttribute(_T("localize"), strLocalizedText);
	}
	delete pList;

	return TRUE;


} 

//-----------------------------------------------------------------------------
BOOL CLocalizableXMLDocument::SaveXMLFile (const CString& strFileName, BOOL bCreatePath /*= FALSE*/)
{	
	m_sFileName	= strFileName;
	
	CStringLoader *pStrLoader = AfxGetStringLoader();  
	if (!pStrLoader || m_pPathFinder->IsCustomPath (m_sFileName))
	{
		CXMLDocumentObject doc(*this);
		CXMLNodeChildsList *pList = doc.SelectNodes(_T("//node()[@localizable='true']"));
		for (int i = 0; i < pList->GetSize(); i++)
		{
			CXMLNode* pParentNode = pList->GetAt(i);
			CString strNodeName;
			pParentNode->GetName(strNodeName);
			pParentNode->RemoveAttribute(_T("sl:") + strNodeName);		
		}
		delete pList;
			

		pList = doc.SelectNodes(_T("//node()[@localize]"));
		for (int i = 0; i < pList->GetSize(); i++)
		{
			CXMLNode* pNode = pList->GetAt(i);
			CString strNodeText;
			pNode->RemoveAttribute(_T("sl:baseLocalize"));	
		}
		delete pList;
		return doc.SaveXMLFile(strFileName, bCreatePath);
	}
	
	CXMLDocumentObject doc(*this);
	if (!doc.GetIXMLDOMDocumentPtr())
		return FALSE;
	CXMLNodeChildsList *pList = doc.SelectNodes(_T("//node()[@localizable='true']"));
	for (int i = 0; i < pList->GetSize(); i++)
	{
		CXMLNode* pParentNode = pList->GetAt(i);
		CString strNodeName;
		pParentNode->GetName(strNodeName);
		
		CString strNodeText;
		if (pParentNode->GetAttribute(_T("sl:") + strNodeName, strNodeText))
		{
			pParentNode->SetText(strNodeText);
			pParentNode->RemoveAttribute(_T("sl:") + strNodeName);		
		}
	}
	delete pList;
			

	pList = doc.SelectNodes(_T("//node()[@localize]"));
	for (int i = 0; i < pList->GetSize(); i++)
	{
		CXMLNode* pNode = pList->GetAt(i);
		CString strNodeText;
		pNode->GetAttribute(_T("sl:baseLocalize"), strNodeText);
		pNode->SetAttribute(_T("localize"), strNodeText);
		pNode->RemoveAttribute(_T("sl:baseLocalize"));	


	}
	delete pList;

	return doc.SaveXMLFile(strFileName, bCreatePath);
}

//-----------------------------------------------------------------------------
CString CLocalizableXMLDocument::GetDictionaryPath() 
{
	ASSERT_TRACE(m_pPathFinder,"Datamember m_pPathFinder cannot be null in this context");
	return AfxGetDictionaryPathFromNamespace(m_Namespace, TRUE);
}

//-----------------------------------------------------------------------------
BOOL CLocalizableXMLDocument::GetLocalizableText(CXMLNode *pNode, CString &strText)
{
	ASSERT_TRACE(pNode,"Parameter pNode cannot be null");
	if (pNode)
	{
		CString strName;
		return pNode->GetName(strName) && pNode->GetAttribute(szPrefix + CString(_T(":")) + strName, strText);
	}
	return FALSE;
}

//-----------------------------------------------------------------------------
BOOL CLocalizableXMLDocument::SetLocalizableText(CXMLNode *pNode, LPCTSTR lpszText, LPCTSTR lpszDictionary /*= NULL*/)
{
	CXMLNode *pRoot = GetRoot();
	if (pRoot && pNode && lpszText)
	{
		// non testare il valore di ritorno: potrebbe fallire
		// ma non significa che per questo debba fallire il metodo
		pRoot->SetAttribute(CString(XML_NAMESPACEURI_TAG) + szPrefix, szNamespace);
		
		BOOL bResult = TRUE;
		CString strName;
		bResult = bResult && pNode->GetName (strName);
		bResult = bResult && pNode->SetAttribute(szPrefix + CString(_T(":")) + strName, lpszText);
		bResult = bResult && pNode->SetAttribute(szLocalizable, szTrue);
		if (lpszDictionary)
			bResult = bResult && pNode->SetAttribute(szDictionary, lpszDictionary);

		return bResult;
	}

	ASSERT_TRACE(pRoot && pNode && lpszText,"Either parameters pRoot, pNode or lpszText are null: not allowed");
	return FALSE;
}

//-----------------------------------------------------------------------------
BOOL CLocalizableXMLDocument::GetLocalizableAttribute(CXMLNode *pNode, CString &strText)
{
	ASSERT_TRACE(pNode,"Parameter pNode cannot be null");
	if (pNode)
	{
		return pNode->GetAttribute(szPrefix + CString(_T(":")) + szBaseLocalize, strText);
	}
	return FALSE;
}

//-----------------------------------------------------------------------------
BOOL CLocalizableXMLDocument::SetLocalizableAttribute(CXMLNode *pNode, LPCTSTR lpszText)
{
	CXMLNode *pRoot = GetRoot();
	if (pRoot && pNode && lpszText)
	{
		// non testare il valore di ritorno: potrebbe fallire
		// ma non significa che per questo debba fallire il metodo
		pRoot->SetAttribute(CString(XML_NAMESPACEURI_TAG) + szPrefix, szNamespace);
		
		return pNode->SetAttribute(szPrefix + CString(_T(":")) + szBaseLocalize, lpszText);
	}

	ASSERT_TRACE(pRoot && pNode && lpszText,"Either parameters pRoot, pNode or lpszText are null: not allowed");
	return FALSE;
}

//*****************************************************************************
// CLocalizableXMLNode 
//*****************************************************************************
//-----------------------------------------------------------------------------
BOOL CLocalizableXMLNode::GetLocalizableText(CString &strText)
{
	ASSERT_TRACE(m_pXMLDoc,"Datamember m_pXMLDoc cannot be null in this context");
	ASSERT_KINDOF(CLocalizableXMLDocument, m_pXMLDoc);
	return m_pXMLDoc ? ((CLocalizableXMLDocument*)m_pXMLDoc)->GetLocalizableText(this, strText) : FALSE;
}

//-----------------------------------------------------------------------------
BOOL CLocalizableXMLNode::SetLocalizableText(LPCTSTR lpszText, LPCTSTR lpszDictionary /*= NULL*/)
{
	ASSERT_TRACE(m_pXMLDoc && lpszText, "Both m_pXMLDoc and lpszText must be not null in this context");
	ASSERT_KINDOF(CLocalizableXMLDocument, m_pXMLDoc);
	return m_pXMLDoc 
		? ((CLocalizableXMLDocument*)m_pXMLDoc)->SetLocalizableText(this, lpszText, lpszDictionary)
		: FALSE;
}

//-----------------------------------------------------------------------------
BOOL CLocalizableXMLNode::GetLocalizableAttribute(CString &strText)
{
	ASSERT_TRACE(m_pXMLDoc,"Datamember m_pXMLDoc cannot be null in this context");
	ASSERT_KINDOF(CLocalizableXMLDocument, m_pXMLDoc);
	return m_pXMLDoc ? ((CLocalizableXMLDocument*)m_pXMLDoc)->GetLocalizableAttribute(this, strText) : FALSE;
}

//-----------------------------------------------------------------------------
BOOL CLocalizableXMLNode::SetLocalizableAttribute(LPCTSTR lpszText)
{
	ASSERT_TRACE(m_pXMLDoc && lpszText, "Both m_pXMLDoc and lpszText must be not null in this context");
	ASSERT_KINDOF(CLocalizableXMLDocument, m_pXMLDoc);
	return m_pXMLDoc 
		? ((CLocalizableXMLDocument*)m_pXMLDoc)->SetLocalizableAttribute(this, lpszText)
		: FALSE;
}
