#include "stdafx.h"

#include <atlenc.h>
#include <locale.h>

#include <TBNamesolver\LoginContext.h>
#include <TBNamesolver\ThreadContext.h>

#include "SettingsTable.h"
#include "ParametersSections.h"
#include "CollateCultureFunctions.h"
#include "DataObj.h"

//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

//-----------------------------------------------------------------------------
CStringA UnicodeToUTF8(CString strData)
{
	int nBuffSize = AtlUnicodeToUTF8(strData, strData.GetLength(), NULL,0); //calcolo la dimensione richiesta per il buffer
	
	CStringA strResult;
	
	AtlUnicodeToUTF8(strData, strData.GetLength(), strResult.GetBuffer(nBuffSize), nBuffSize);	

	strResult.ReleaseBuffer();

	return strResult;
}

//-----------------------------------------------------------------------------
CString UTF8ToUnicode(CStringA strData)
{
	int nBuffSize = MultiByteToWideChar(CP_UTF8, 0, strData, strData.GetLength(), NULL, 0);//calcolo la dimensione richiesta per il buffer
	
	CString strResult;
	
	MultiByteToWideChar(CP_UTF8, 0, strData, strData.GetLength(), strResult.GetBuffer(nBuffSize), nBuffSize);

	strResult.ReleaseBuffer();

	return strResult;
}
//-----------------------------------------------------------------------------
CString TBUrlUnescape(CStringA strUrl)
{
	strUrl.Replace('+', ' ');
	VERIFY (S_OK == UrlUnescapeA(strUrl.GetBuffer(), NULL, NULL, URL_UNESCAPE_INPLACE));
	strUrl.ReleaseBuffer();
	return UTF8ToUnicode(strUrl);
}
//////////////////////////////////////////////////////////////////////////////
//						CULTURE INFO
//////////////////////////////////////////////////////////////////////////////

//----------------------------------------------------------------------------
inline TB_EXPORT const CCultureInfo* AFXAPI AfxGetCultureInfo()
{
	CLoginContext* pContext = AfxGetLoginContext();
	return pContext 
		? pContext->GetObject<CCultureInfo>(&CLoginContext::GetCultureInfo)
		: NULL;
}

//////////////////////////////////////////////////////////////////////////////
//						CCultureInfo
//////////////////////////////////////////////////////////////////////////////
IMPLEMENT_DYNAMIC(CCultureInfo, CObject)

//-----------------------------------------------------------------------------
CString CCultureInfo::PadUpperLimitString(int nPadLen) const
{
	return CString(m_cCultureUpperLimit/*m_cUpper*/, nPadLen);
}

//-----------------------------------------------------------------------------
CString CCultureInfo::TrimUpperLimitString(CString s) const
{
	int j = 0;
	for (j = s.GetLength() - 1; j >= 0 && s[j] == m_cCultureUpperLimit/*m_cUpper*/; j--);
	return s.Left(j + 1);
}

//-----------------------------------------------------------------------------
CCultureInfo::CCultureInfo ()
	:
	m_nCulture	(LOCALE_INVARIANT)
{
	m_nSizeOfTCHAR = sizeof (TCHAR);

	m_cCultureLowerLimit = _T(' ');
	m_cCultureUpperLimit = _T('z');
	m_sCharSetSample = _T("");
}

//-----------------------------------------------------------------------------
BOOL CCultureInfo::IsManageCompanyDatabaseCultureDisabled()
{
	DataObj* pDataObj = AfxGetSettingValue 
					(
						CTBNamespace(szTbGenlibNamespace),
						szEnvironment,
						szManageCompanyDatabaseCulture,
						DataBool(TRUE)
					);

	// not enabled
	return (!pDataObj || pDataObj->GetDataType() != DataType::Bool || *((DataBool*) pDataObj) == FALSE);
}
//-----------------------------------------------------------------------------
void CCultureInfo::SetCultureLCID (const LCID& aCultureID)
{
	// not enabled
	if (IsManageCompanyDatabaseCultureDisabled())
	{
		m_nCulture = LOCALE_INVARIANT;
		return;
	}

	// existance check
	TCHAR sTmpBuffer[255]; 
	int nBufferSize = GetLocaleInfo(aCultureID, LOCALE_SENGLANGUAGE, (LPTSTR) sTmpBuffer, sizeof(sTmpBuffer));
	if (nBufferSize)
		m_nCulture = aCultureID;
}

// attenzione: sia la CompareNoCase, sia la CollateNoCase non tengono conto
// dell'ID della cultura corretto per il turco. Devo per forza utilizzare
// la MakeUpper/MakeLower per convertire il testo prima di compararlo.
//-----------------------------------------------------------------------------
int	CCultureInfo::CompareStrings (const CString& str1, const CString& str2, BOOL bCaseSensitive /*FALSE*/) const
{
	// codice pre-esistente privo di locale-sensitive
	if (bCaseSensitive || IsInvariantCulture())
		return bCaseSensitive ? str1.Compare(str2) : str1.CompareNoCase(str2);
	
	CString tempStr1, tempStr2;

	// poichè le funzioni di compare non lavorano
	// correttamente, trasformo tutto in maiuscolo
	tempStr1.SetString(str1, str1.GetLength());
	tempStr2.SetString(str2, str2.GetLength());

	LCMapString (m_nCulture, LCMAP_LINGUISTIC_CASING | LCMAP_UPPERCASE, str1, -1, tempStr1.GetBuffer(0), m_nSizeOfTCHAR * tempStr1.GetLength ());
	tempStr1.ReleaseBuffer();	
	
	LCMapString	(m_nCulture, LCMAP_LINGUISTIC_CASING | LCMAP_UPPERCASE, str2, -1, tempStr2.GetBuffer(0), m_nSizeOfTCHAR * tempStr2.GetLength ());
	tempStr2.ReleaseBuffer();

	return tempStr1.Compare(tempStr2);
}

//-----------------------------------------------------------------------------
void CCultureInfo::MakeUpper (CString& str) const
{
	if (IsInvariantCulture())
	{
		str.MakeUpper ();
		return;
	}

	LCMapString
		(
			m_nCulture, 
			LCMAP_LINGUISTIC_CASING | LCMAP_UPPERCASE, 
			str, 
			str.GetLength(), 
			str.GetBuffer(0), 
			m_nSizeOfTCHAR * str.GetLength ()
		);
	str.ReleaseBuffer();	
}

//-----------------------------------------------------------------------------
void CCultureInfo::MakeLower (CString& str) const
{
	if (IsInvariantCulture ())
	{
		str.MakeLower ();
		return;
	}

	LCMapString
		(
			m_nCulture, 
			LCMAP_LINGUISTIC_CASING | LCMAP_LOWERCASE, 
			str, 
			str.GetLength(), 
			str.GetBuffer(0), 
			m_nSizeOfTCHAR * str.GetLength ()
		);
	str.ReleaseBuffer();	
}

//-----------------------------------------------------------------------------
CString	CCultureInfo::GetUpperCase (const CString& str) const
{
	CString tempStr1;

	tempStr1.SetString (str, str.GetLength());
	
	MakeUpper (tempStr1);
	return tempStr1;
}

//-----------------------------------------------------------------------------
CString	CCultureInfo::GetLowerCase (const CString& str) const
{
	CString tempStr1;

	tempStr1.SetString (str, str.GetLength());
	
	MakeLower (tempStr1);
	return tempStr1;
}


//--------------------------------------------------------------------------------
void InitThreadContext()
{
	CThreadContext* pThreadContext = AfxGetThreadContext();
	CLoginContext* pLoginContext = AfxGetLoginContext();
	if (pLoginContext)
	{
		pThreadContext->SetOperationsDate
			(
			pLoginContext->GetOperationsDay(),
			pLoginContext->GetOperationsMonth(),
			pLoginContext->GetOperationsYear());
	
		const CCultureInfo* pInfo = AfxGetCultureInfo();
		if (pInfo)
		{
			pThreadContext->SetUICulture(pInfo->GetUICulture());
			if (!pInfo->IsInvariantCulture())
				pThreadContext->SetCollateCultureSensitive();
		}
	}
	else
	{
		DataDate aDate (TodayDate());
		pThreadContext->SetOperationsDate
			(
			aDate.Day(),
			aDate.Month(),
			aDate.Year()
			);
	}
}