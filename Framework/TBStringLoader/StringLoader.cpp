
#include "stdafx.h"

#include <shlwapi.h>
#include <sys\stat.h>

#include "stringloader.h"
#include "generic.h"
#include "tbstringloader.h"
#include "const.h"
#include ".\stringloader.h"



static const TCHAR sxStringLoaderUrn[]	= _T("urn:TBStringLoader");
static const TCHAR szXmlExt[]				= _T(".xml");

extern CTBStringLoaderApp theApp;

//===========================================================================
// CModuleStrings
//===========================================================================

IMPLEMENT_SERIAL(CModuleStrings, CObject, SERIALIZATION_VERSION)
		
//------------------------------------------------------------------------------
CModuleStrings::CModuleStrings(const CString &strDictionaryPath)
{
	CSingleLock lock(&m_CriticalSection, TRUE);
	m_strDictionaryPath = strDictionaryPath;
	m_Dictionary.ParseDictionary(m_strDictionaryPath);
}

//------------------------------------------------------------------------------
CModuleStrings::~CModuleStrings()
{
	CSingleLock lock(&m_CriticalSection, TRUE);
	m_Dictionary.RemoveAll();
}

//------------------------------------------------------------------------------
void CModuleStrings::Serialize(CArchive& ar)
{
	CSingleLock lock(&m_CriticalSection, TRUE);
	__super::Serialize(ar);
	if (ar.IsStoring())
	{
		ar << m_strDictionaryPath;
	}
	else
	{
		ar >> m_strDictionaryPath;
	}

	m_Dictionary.Serialize(ar);
}

//------------------------------------------------------------------------------
void CModuleStrings::FreeStrings()
{
	CSingleLock lock(&m_CriticalSection, TRUE);
	m_Dictionary.RemoveAll();
}

//------------------------------------------------------------------------------
CStringBlock* CModuleStrings::GetDialogStrings(CStringLoader *pLoader, UINT nIDD)
{
	CSingleLock lock(&m_CriticalSection, TRUE);
	CString strResName;
	strResName.Format(_T("%d"), nIDD);
	
	return m_Dictionary.FindStringBlock(pLoader, DIALOG_TYPE, _T(""), strResName);
}
//------------------------------------------------------------------------------
CStringBlock* CModuleStrings::GetJsonFormStrings(CStringLoader *pLoader, const CString& sId, const CString& sName)
{
	CSingleLock lock(&m_CriticalSection, TRUE);
	
	return m_Dictionary.FindStringBlock(pLoader, JSON_FORMS_TYPE, sId, sName);
}
//------------------------------------------------------------------------------
CStringBlock* CModuleStrings::GetStringTableStrings(CStringLoader *pLoader, UINT nIDD)
{
	CSingleLock lock(&m_CriticalSection, TRUE);
	CString strResName;
	strResName.Format(_T("%d"), nIDD);
	return m_Dictionary.FindStringBlock(pLoader, STRINGTABLE_TYPE, _T(""), strResName);
}
 
//------------------------------------------------------------------------------
CStringBlock* CModuleStrings::GetMenuStrings(CStringLoader *pLoader, UINT nIDD)
{
	CSingleLock lock(&m_CriticalSection, TRUE);
	CString strResName;
	strResName.Format(_T("%d"), nIDD);
	return m_Dictionary.FindStringBlock(pLoader, MENU_TYPE, _T(""), strResName);
}

//------------------------------------------------------------------------------
CStringBlock* CModuleStrings::GetSourceStrings(CStringLoader *pLoader, const CString &strFileName)
{
	CSingleLock lock(&m_CriticalSection, TRUE);
	return m_Dictionary.FindStringBlock(pLoader, SOURCE_STRINGS_FOLDER, SOURCE_STRING_IDENTIFIER, AdjustName(strFileName));
}

//------------------------------------------------------------------------------
CStringBlock* CModuleStrings::GetDatabaseStrings(CStringLoader *pLoader, const CString &strTableName)
{
	CSingleLock lock(&m_CriticalSection, TRUE);
	return m_Dictionary.FindStringBlock(pLoader, DATABASE_STRINGS_FOLDER, DATABASE_STRINGS_IDENTIFIER, strTableName);
}


//------------------------------------------------------------------------------
CStringBlock* CModuleStrings::GetEnumStrings(CStringLoader *pLoader, const CString &strTagName)
{
	CSingleLock lock(&m_CriticalSection, TRUE);
	return m_Dictionary.FindStringBlock(pLoader, ENUM_STRINGS_FOLDER, ENUM_STRINGS_IDENTIFIER, strTagName);
}

//------------------------------------------------------------------------------
CStringBlock* CModuleStrings::GetFormatterStrings(CStringLoader *pLoader)
{
	CSingleLock lock(&m_CriticalSection, TRUE);
	return m_Dictionary.FindStringBlock(pLoader, FORMATTER_STRINGS_FOLDER, FORMATTER_STRINGS_NAME, FORMATTER_STRINGS_NAME);
}

//------------------------------------------------------------------------------
CStringBlock* CModuleStrings::GetFontStrings(CStringLoader *pLoader)
{
	CSingleLock lock(&m_CriticalSection, TRUE);
	return m_Dictionary.FindStringBlock(pLoader, FORMATTER_STRINGS_FOLDER, FONT_STRINGS_NAME, FONT_STRINGS_NAME);
}

//------------------------------------------------------------------------------
CStringBlock* CModuleStrings::GetXMLStrings(CStringLoader *pLoader, const CString &strFileName)
{
	CSingleLock lock(&m_CriticalSection, TRUE);
	CString strAdjustedFile = AdjustName(strFileName);
	return m_Dictionary.FindStringBlock(pLoader, XML_TYPE, strAdjustedFile, strAdjustedFile);
}

//------------------------------------------------------------------------------
CStringBlock* CModuleStrings::GetReportStrings(CStringLoader *pLoader, const CString &strFileName)
{
	CSingleLock lock(&m_CriticalSection, TRUE);
	return m_Dictionary.FindStringBlock(pLoader, REPORT_TYPE, AdjustName(strFileName), REPORT_TYPE);
}

//===========================================================================
// CStringLoader
//===========================================================================

//------------------------------------------------------------------------------
CStringLoader::CStringLoader(const CString &sCachePath)
{
	LoadCache(sCachePath);
}

//------------------------------------------------------------------------------
CStringLoader::CStringLoader(const CString& strLocalCacheId, const CString &sCachePath)
{
	m_strLocalCacheId = strLocalCacheId;
	LoadCache(sCachePath);
}

//------------------------------------------------------------------------------
CStringLoader::~CStringLoader(void)
{
	//IMPORTANT!!! call this method before SaveCache, because SaveCache delete objects used by m_BinaryFileLRU.RemoveAll()
	m_BinaryFileLRU.RemoveAll(); 
	SaveCache();
}

//------------------------------------------------------------------------------
CStringBlockContainer* CStringLoader::GetDictionary(const CString& strFilePath) 
{
	CSingleLock lock(&m_CriticalSection, TRUE);
	return m_BinaryFileLRU.GetDictionary(strFilePath); 
}
//------------------------------------------------------------------------------
void CStringLoader::ClearCache()
{
	CSingleLock lock(&m_CriticalSection, TRUE);
	if (PathFileExists(m_strCacheFilePath))
		DeleteFile(m_strCacheFilePath);
	FreeCache();
}

//------------------------------------------------------------------------------
void CStringLoader::LoadCache(const CString &sCachePath)
{
	CSingleLock lock(&m_CriticalSection, TRUE);
	InitCacheFilePath(sCachePath);

	if (!PathFileExists(m_strCacheFilePath))
		return;
	
	try
	{
		CFile aFile (m_strCacheFilePath, CFile::modeRead);
		CArchive anArchive (&aFile, CArchive::load);
		CString fileCacheID;
		anArchive >> fileCacheID;
		if (m_strLocalCacheId == fileCacheID)
			m_Modules.Serialize(anArchive);
	}
	catch(...)
	{
		m_Modules.RemoveAll();
	}
}

//------------------------------------------------------------------------------
void CStringLoader::SaveCache()
{
	CSingleLock lock(&m_CriticalSection, TRUE);
	TRY
	{
		CFile aFile (m_strCacheFilePath, CFile::modeWrite | CFile::modeCreate);
		CArchive anArchive(&aFile, CArchive::store);
		anArchive << m_strLocalCacheId;
		m_Modules.Serialize(anArchive);
	}
	CATCH_ALL(e)
	{
		DeleteFile(m_strCacheFilePath);
	}
	END_CATCH_ALL

	for (int i = 0; i < m_Modules.GetSize (); i++) delete m_Modules[i];
	for (int i = 0; i < m_Fonts.GetSize (); i++) delete m_Fonts[i];
}

//------------------------------------------------------------------------------
void CStringLoader::FreeCache()
{
	theApp.FreeCache();
}

//------------------------------------------------------------------------------
void CStringLoader::FreeModules()
{
	CSingleLock lock(&m_CriticalSection, TRUE);

	m_BinaryFileLRU.RemoveAll(); 
	for (int i = 0; i < m_Modules.GetSize (); i++) delete m_Modules[i];
	m_Modules.RemoveAll();
}


//------------------------------------------------------------------------------
void CStringLoader::InitCacheFilePath(const CString &sCachePath)
{
	TCHAR path[MAX_PATH];

	m_strCacheFilePath = PathCombine(path, sCachePath, _T("TBStringLoader.bin"));
	// on client workstation file system
	if (PathFileExists(m_strCacheFilePath))
		_tchmod(m_strCacheFilePath, _S_IREAD | _S_IWRITE);
}

//------------------------------------------------------------------------------
CFont* CStringLoader::GetFont(const LOGFONT &lf)
{
	CSingleLock lock(&m_CriticalSection, TRUE);

	CFont* pFont = NULL;
	LOGFONT tmpLf;

	for (int i=0; i<m_Fonts.GetSize (); i++) 
	{
		memset(&tmpLf, 0, sizeof(LOGFONT));        
		pFont = (CFont*) m_Fonts[i];
		pFont->GetLogFont(&tmpLf);

		if (memcmp(&lf, &tmpLf, sizeof(LOGFONT))==0) return pFont; 
	}

	return NULL;
}

//------------------------------------------------------------------------------
BOOL CStringLoader::FindString(LPCTSTR lpcstrBaseString, CStringBlock* pStringBlock, CString &strTarget)
{
	if (!pStringBlock || !lpcstrBaseString) return FALSE;

	BOOL bEscape = FALSE;
	CStringItem* pStringItem = FindStringItem(lpcstrBaseString, pStringBlock, bEscape); 

	if (!pStringItem)
	{
		bEscape = TRUE;
		pStringItem = FindStringItem(lpcstrBaseString, pStringBlock, bEscape); 
	}

	if (pStringItem)
	{
		strTarget = bEscape
			? UnescapeString(pStringItem->m_strTarget)
			: pStringItem->m_strTarget;

		return !strTarget.IsEmpty();
	}
	return FALSE;
}



//------------------------------------------------------------------------------
CStringItem* CStringLoader::FindStringItem(LPCTSTR lpcstrBaseString, CStringBlock* pStringBlock, BOOL bEscape)
{
	if (!pStringBlock || !lpcstrBaseString) return NULL;  

	CString strBaseString = bEscape ? EscapeString(lpcstrBaseString) : lpcstrBaseString;    
	CStringItem* pItem;
	if (pStringBlock->Lookup(strBaseString, pItem) && pItem->IsValid())
		return pItem;
	strBaseString.Trim();
	strBaseString.Replace(_T("\r\n"), _T("\n"));
	if (pStringBlock->Lookup(strBaseString, pItem) && pItem->IsValid())
		return pItem;
	
	/*scommentare per vedere il contenuto del blocco
	
	POSITION pos = pStringBlock->GetStartPosition();
	CString sKey;
	CObject* pVal;
	while(pos)
		pStringBlock->GetNextAssoc(pos, sKey, pVal);*/

	return NULL;
}
 
//------------------------------------------------------------------------------
BOOL CStringLoader::ReplaceString(CWnd* pwndChild, CStringBlock* pStringBlock)
{
	ASSERT(pwndChild);
	ASSERT(pStringBlock);

	CString strText, strTarget; 

	CStringItem * pItem;
	pwndChild->GetWindowText(strText);
	if (!strText.IsEmpty ())
	{
		CString strX, strY, strH, strW, strFontName, strFontSize, strFontBold, strFontItalic;
		BOOL bOffsetExists = FALSE, bFontExists = FALSE;

		pItem = FindStringItem(strText, pStringBlock, FALSE); 
		if (!pItem)
			pItem = FindStringItem(strText, pStringBlock, TRUE);
		if (!pItem) 
			return FALSE;

		bOffsetExists |= pItem->GetAttribute(X_ATTRIBUTE, strX);
		bOffsetExists |= pItem->GetAttribute(Y_ATTRIBUTE, strY);
		bOffsetExists |= pItem->GetAttribute(H_ATTRIBUTE, strH);
		bOffsetExists |= pItem->GetAttribute(W_ATTRIBUTE, strW);

		if (bOffsetExists)
		{
			CRect r;
			pwndChild->GetWindowRect(r); 
			if (pwndChild->GetParent()) pwndChild->GetParent()->ScreenToClient (r);

			pwndChild->SetWindowPos
				(
					NULL, 
					r.left + _ttoi(strX), 
					r.top + _ttoi(strY), 
					r.right - r.left + _ttoi(strW),
					r.bottom - r.top + _ttoi(strH),
					SWP_NOZORDER 
				) ;
		}

		bFontExists |= pItem->GetAttribute(FONTNAME_ATTRIBUTE, strFontName);
		bFontExists |= pItem->GetAttribute(FONTSIZE_ATTRIBUTE, strFontSize);
		bFontExists |= pItem->GetAttribute(FONTBOLD_ATTRIBUTE, strFontBold);
		bFontExists |= pItem->GetAttribute(FONTITALIC_ATTRIBUTE, strFontItalic);

		if (bFontExists)
		{
			LOGFONT lf;
			memset(&lf, 0, sizeof(LOGFONT));        
			pwndChild->GetFont()->GetLogFont(&lf);  

			if (!strFontName.IsEmpty()) _tcsncpy_s(lf.lfFaceName, strFontName, 32);

			if (!strFontSize.IsEmpty()) lf.lfHeight = _ttoi(strFontSize);

			if (strFontBold == XML_TRUE) lf.lfWeight = FW_BOLD; 
			else lf.lfWeight = FW_REGULAR;

			lf.lfItalic = (strFontItalic == XML_TRUE); 					

			CFont *pFont = GetFont(lf);
			if (!pFont)
			{
				CSingleLock lock(&m_CriticalSection, TRUE);

				pFont = new CFont();
				pFont->CreateFontIndirect(&lf);
				m_Fonts.Add(pFont);
			}

			pwndChild->SetFont(pFont);
		} 

		
		pwndChild->SetWindowText(pItem->m_strTarget); 

	}
	return TRUE;
}

//------------------------------------------------------------------------------
BOOL CStringLoader::LoadWindowStrings(CWnd* pWnd, CStringBlock *pBlock, BOOL bDemo)
{
	if (!pBlock || !pWnd || pBlock->IsEmpty()) return FALSE;

	CDemoDialog *pDemoDialog = NULL;
	if (bDemo)
	{
		pDemoDialog = (CDemoDialog*) pWnd;
		pDemoDialog->m_NonLocalizedWindows.RemoveAll();
	}

	if (!ReplaceString(pWnd, pBlock) && pDemoDialog)
		pDemoDialog->AddNotFoundString(pWnd);

	CWnd* pwndChild = pWnd->GetWindow(GW_CHILD);
	while (pwndChild)
	{
		// se sono in demo, devo visualizzare anche le finestre nascoste
		if (bDemo) pwndChild->ShowWindow(SW_SHOW);

		if (!ReplaceString(pwndChild, pBlock) && pDemoDialog)
			pDemoDialog->AddNotFoundString(pwndChild);

		pwndChild = pwndChild->GetNextWindow();
	}

	return TRUE;
}
	
//------------------------------------------------------------------------------
BOOL CStringLoader::LoadMenuStrings(CMenu* pMenu, CStringBlock *pStringBlock)
{
	if (!pMenu || !pStringBlock) return FALSE;
	
	//se pNode è nullo, ripristino la lingua nativa del menu (che potrebbe essere stato tradotto)
	
	CString strText, strTarget;
	CMenu* pSubMenu = NULL;

	BOOL bResult = TRUE; 

	for (int i = 0; i < (int)pMenu->GetMenuItemCount(); i++)
	{
		pMenu->GetMenuString(i, strText, MF_BYPOSITION);
		if (!strText.IsEmpty())
		{
			if (!FindString (strText, pStringBlock, strTarget))
				strTarget = strText;
			
			pMenu->ModifyMenu(i, MF_BYPOSITION | MF_STRING, pMenu->GetMenuItemID(i), strTarget);
		}

		pSubMenu = pMenu->GetSubMenu(i);
		if (pSubMenu && ::IsMenu (pMenu->m_hMenu))
			bResult = LoadMenuStrings(pSubMenu, pStringBlock) && bResult;
	}

	return bResult;
}

//------------------------------------------------------------------------------
CModuleStrings* CStringLoader::GetModule(const CString &strDictionaryPath)
{
	if (strDictionaryPath.IsEmpty())
		return NULL;
	
	CSingleLock lock(&m_CriticalSection, TRUE);

	CModuleStrings* pModule = NULL;
	for (int i=0; i<m_Modules.GetSize (); i++) 
	{
		pModule = (CModuleStrings*) m_Modules[i];
		if (pModule && pModule->m_strDictionaryPath.CompareNoCase(strDictionaryPath) == 0 )
			return pModule;
	}

	pModule = new CModuleStrings(strDictionaryPath);
	m_Modules.Add(pModule);
	return pModule;
}

//------------------------------------------------------------------------------
CStringBlock* CStringLoader::GetStringTableStrings(const CStringArray &strRootPaths, UINT nIDD)
{
	CStringBlock* pBlock = NULL;
	for (int i = 0; i < strRootPaths.GetCount(); i++)
	{
		CModuleStrings *pModule = GetModule(strRootPaths[i]);
		if (!pModule) continue;

		pBlock = pModule->GetStringTableStrings(this, nIDD);		
		if (pBlock && !pBlock->IsEmpty())
			break;
	}
	return pBlock;
}
//------------------------------------------------------------------------------
CStringBlock* CStringLoader::GetMenuStrings(const CStringArray &strRootPaths, UINT nIDD)
{
	CStringBlock* pBlock = NULL;
	for (int i = 0; i < strRootPaths.GetCount(); i++)
	{
		CModuleStrings *pModule = GetModule(strRootPaths[i]);
		if (!pModule) continue;

		pBlock = pModule->GetMenuStrings(this, nIDD);		
		if (pBlock && !pBlock->IsEmpty())
			break;
	}
	return pBlock;
}
//------------------------------------------------------------------------------
CStringBlock* CStringLoader::GetSourceStrings(const CStringArray &strRootPaths, const CString &strFileName)
{
	CStringBlock* pBlock = NULL;
	for (int i = 0; i < strRootPaths.GetCount(); i++)
	{
		CModuleStrings *pModule = GetModule(strRootPaths[i]);
		if (!pModule) continue;

		pBlock = pModule->GetSourceStrings(this, strFileName);		
		if (pBlock && !pBlock->IsEmpty())
			break;
	}
	return pBlock;
}
//------------------------------------------------------------------------------
CStringBlock* CStringLoader::GetJsonFormStrings(const CString &strRootPath, const CString& sId, const CString& sName)
{
	CModuleStrings *pModule = GetModule(strRootPath);
	if (!pModule)
		return NULL;
	CStringBlock* pBlock = pModule->GetJsonFormStrings(this, sId, sName);
	if (!pBlock || pBlock->IsEmpty())
		return NULL;
	return pBlock;
}
//------------------------------------------------------------------------------
CStringBlock* CStringLoader::GetDialogStrings(const CStringArray &strRootPaths, UINT nIDD)
{
	CStringBlock* pBlock = NULL;
	for (int i = 0; i < strRootPaths.GetCount(); i++)
	{
		CModuleStrings *pModule = GetModule(strRootPaths[i]);
		if (!pModule) continue;

		pBlock = pModule->GetDialogStrings(this, nIDD);		
		if (pBlock && !pBlock->IsEmpty())
			break;
	}
	return pBlock;
}
//------------------------------------------------------------------------------
CStringBlock* CStringLoader::GetEnumStrings(const CString &strRootPath, const CString &strTagName)
{
	CModuleStrings *pModule = GetModule(strRootPath);
	if (!pModule) return NULL;

	return pModule->GetEnumStrings(this, strTagName);		

}

//------------------------------------------------------------------------------
CStringBlock* CStringLoader::GetFontStrings(const CString &strRootPath)
{
	CModuleStrings *pModule = GetModule(strRootPath);
	if (!pModule) return NULL;

	return pModule->GetFontStrings(this);		

}

//------------------------------------------------------------------------------
CStringBlock* CStringLoader::GetFormatterStrings(const CString &strRootPath)
{
	CModuleStrings *pModule = GetModule(strRootPath);
	if (!pModule) return NULL;

	return pModule->GetFormatterStrings(this);		

}

//------------------------------------------------------------------------------
CStringBlock* CStringLoader::GetXMLStrings(const CString &strRootPath, const CString &strFileName)
{
	CModuleStrings *pModule = GetModule(strRootPath);
	if (!pModule) return NULL;

	return pModule->GetXMLStrings(this, strFileName);		
}

//------------------------------------------------------------------------------
CStringBlock* CStringLoader::GetDatabaseStrings(const CString &strRootPath, const CString &strTableName)
{
	CModuleStrings *pModule = GetModule(strRootPath);
	if (!pModule) return NULL;

	return pModule->GetDatabaseStrings(this, strTableName);		
}

//------------------------------------------------------------------------------
CStringBlock* CStringLoader::GetReportStrings(const CString &strRootPath, const CString &strFileName)
{
	CModuleStrings *pModule = GetModule(strRootPath);
	if (!pModule) return NULL;

	return pModule->GetReportStrings(this, strFileName);		
}

//------------------------------------------------------------------------------
BOOL CStringLoader::LoadWindowStrings(CWnd* pWnd, const CStringArray& strDictionaryPaths, UINT IDD)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	if (!pWnd) return FALSE;

	return LoadWindowStrings(pWnd, GetDialogStrings(strDictionaryPaths, IDD), FALSE);
}

//------------------------------------------------------------------------------
BOOL CStringLoader::LoadJsonFormString(LPCTSTR lpcstrBaseString, LPCTSTR lpcstrId, LPCTSTR lpcstrName, LPCTSTR lpcstrDictionaryPath, void*& lpPrivateData, CString& sTarget)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	CStringBlock* pBlock;
	if (lpPrivateData)
	{
		pBlock = (CStringBlock*) lpPrivateData;
	}
	else
	{
		pBlock = GetJsonFormStrings(lpcstrDictionaryPath, lpcstrId, lpcstrName);
		lpPrivateData = pBlock;
	}
	
	if (!pBlock || pBlock->IsEmpty()) return FALSE;

	return FindString(lpcstrBaseString, pBlock, sTarget);
}
//------------------------------------------------------------------------------
CString CStringLoader::LoadResourceString(LPCTSTR lpcstrBaseString, UINT IDD, const CStringArray& strDictionaryPaths)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	CStringBlock *pBlock = GetStringTableStrings(strDictionaryPaths, IDD);
	if (!pBlock || pBlock->IsEmpty()) return lpcstrBaseString;

	CString strTarget;
	return FindString(lpcstrBaseString, pBlock, strTarget) ? strTarget : lpcstrBaseString;
}

//------------------------------------------------------------------------------
BOOL CStringLoader::LoadMenuStrings(CMenu* pMenu, const CStringArray& strDictionaryPaths, UINT nIDD)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState()); 

	if (!pMenu || !::IsMenu(pMenu->m_hMenu) ) return FALSE; 

	CStringBlock *pBlock = GetMenuStrings(strDictionaryPaths, nIDD);	
	if (!pBlock || pBlock->IsEmpty()) return FALSE;

	return LoadMenuStrings(pMenu, pBlock);
}
//------------------------------------------------------------------------------
CString CStringLoader::LoadSourceString(LPCTSTR lpcstrBaseString, LPCTSTR lpcstrFileName, const CStringArray& strDictionaryPaths)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	if (!lpcstrBaseString || !lpcstrBaseString[0]) return _T("");

	CStringBlock *pBlock = GetSourceStrings(strDictionaryPaths, lpcstrFileName); 
	if (!pBlock || pBlock->IsEmpty()) 
		return lpcstrBaseString;

	CString strTarget;
	return FindString(lpcstrBaseString, pBlock, strTarget) ? strTarget : lpcstrBaseString;
}

//------------------------------------------------------------------------------
CString CStringLoader::LoadDatabaseString(LPCTSTR lpcstrBaseString, LPCTSTR lpcstrTableName, LPCTSTR lpcstrDictionaryPath)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	CStringBlock *pBlock = GetDatabaseStrings(lpcstrDictionaryPath, lpcstrTableName);
	if (!pBlock || pBlock->IsEmpty()) return lpcstrBaseString;

	CString strTarget;
	return FindString(lpcstrBaseString, pBlock, strTarget) ? strTarget : lpcstrBaseString;
}

//------------------------------------------------------------------------------
CString CStringLoader::LoadXMLString(LPCTSTR lpcstrBaseString, LPCTSTR lpcstrFileName, LPCTSTR lpcstrDictionaryPath)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	if (!lpcstrBaseString || !lpcstrBaseString[0]) return _T("");

	CStringBlock *pBlock = GetXMLStrings(lpcstrDictionaryPath, lpcstrFileName);

	if (!pBlock || pBlock->IsEmpty()) return lpcstrBaseString;
	CString strTarget;
	if (FindString (lpcstrBaseString, pBlock, strTarget)) return strTarget;

	// se non la trovo in prima istanza, provo a trimmarla
	// (il processore XSLT potrebbe aggiungere dei blank inutili)
	CString strBaseString(lpcstrBaseString);
	strBaseString.Trim();
	if (FindString (strBaseString, pBlock, strTarget)) return strTarget;

	return lpcstrBaseString;
}

//------------------------------------------------------------------------------
CString CStringLoader::LoadEnumString(LPCTSTR lpcstrBaseString, LPCTSTR lpcstrTagName, LPCTSTR lpcstrDictionaryPath)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	if (!lpcstrBaseString || !lpcstrBaseString[0]) 
		return _T("");

	CStringBlock *pBlock = GetEnumStrings(lpcstrDictionaryPath, lpcstrTagName); 
	if (!pBlock || pBlock->IsEmpty()) 
		return lpcstrBaseString;

	CString strTarget;
	return FindString(lpcstrBaseString, pBlock, strTarget) ? strTarget : lpcstrBaseString;
}

//------------------------------------------------------------------------------
CString CStringLoader::LoadFormatterString(LPCTSTR lpcstrBaseString, LPCTSTR lpcstrDictionaryPath)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	if (!lpcstrBaseString || !lpcstrBaseString[0]) 
		return _T("");

	CStringBlock *pBlock = GetFormatterStrings(lpcstrDictionaryPath);
	if (!pBlock || pBlock->IsEmpty()) 
		return lpcstrBaseString;

	CString strTarget;
	return FindString(lpcstrBaseString, pBlock, strTarget) ? strTarget : lpcstrBaseString;
}

//------------------------------------------------------------------------------
CString CStringLoader::LoadFontString(LPCTSTR lpcstrBaseString, LPCTSTR lpcstrDictionaryPath)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	if (!lpcstrBaseString || !lpcstrBaseString[0]) 
		return _T("");

	CStringBlock *pBlock = GetFontStrings(lpcstrDictionaryPath);
	if (!pBlock || pBlock->IsEmpty()) 
		return lpcstrBaseString;

	CString strTarget;
	return FindString(lpcstrBaseString, pBlock, strTarget) ? strTarget : lpcstrBaseString;
}

//------------------------------------------------------------------------------
CString CStringLoader::LoadReportString(LPCTSTR lpcstrBaseString, LPCTSTR lpcstrFileName, LPCTSTR lpcstrDictionaryPath)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState()); 

	if (!lpcstrBaseString || !lpcstrBaseString[0]) 
		return _T("");

	CStringBlock *pBlock = GetReportStrings(lpcstrDictionaryPath, lpcstrFileName);
	if (!pBlock || pBlock->IsEmpty()) 
		return lpcstrBaseString;

	CString strTarget;
	return FindString(lpcstrBaseString, pBlock, strTarget) ? strTarget : lpcstrBaseString;
}

//------------------------------------------------------------------------------
void CStringLoader::FreeStrings(const CString &strRootPath, const CString& strType, const CString& strFileName)
{
	CModuleStrings *pModule = GetModule(strRootPath);
	if (!pModule) return;

	pModule->FreeStrings();
}

//------------------------------------------------------------------------------
CString CModuleStrings::AdjustName(const CString& strFileName)
{
	CString strBareFile(strFileName);
	strBareFile.MakeLower();
	strBareFile = PathFindFileName(strBareFile);
	LPTSTR lpszBuff = strBareFile.GetBuffer();
	PathRemoveExtension(lpszBuff);
	strBareFile.ReleaseBuffer();
	return strBareFile;
}

