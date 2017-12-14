#pragma once

#include <TBNameSolver\TBResourceLocker.h>
#include <TbNameSolver\MacroToRedifine.h>

#include "FormatsHelpers.h"

#	define _T_TOUPPER		towupper	
#	define _T_TOLOWER		towlower	
#	define _T_MEMSET		wmemset
#	define _T_ISCNTRL(ch)	iswcntrl(ch)
#	define _T_EOF			WEOF

//includere alla fine degli include del .H
#include "beginh.dex"

//=============================================================================
class TB_EXPORT CCultureInfo : public CObject, public CTBLockable
{
friend class CLoginThread;
friend class FontStyleTable;
friend class CSysAdminProviderParams;

	DECLARE_DYNAMIC(CCultureInfo)
private:                                                                         
	LCID				m_nCulture;
	int					m_nSizeOfTCHAR;
	CString 			m_sUICulture;
	FormatStyleLocale	m_FormatStyleLocale;
	CString				m_sCharSetSample;
	TCHAR 				m_cCultureUpperLimit;
	TCHAR 				m_cCultureLowerLimit;

public:
	CCultureInfo ();
	
	CString PadUpperLimitString(int len) const;
	CString TrimUpperLimitString(CString s) const;
	void	InitLocale(const CString& sCulture) { m_FormatStyleLocale.ReadSettings(sCulture); }

private:
	void	SetCultureLCID	(const LCID& aCultureID);
	void	SetUICulture	(const CString& strCulture)	{ m_sUICulture = strCulture; }
	void	SetCharSetSample(const CString& strCharSetSample)	{ m_sCharSetSample = strCharSetSample; }

public:
	const LCID	GetCultureLCID		() const					{ return m_nCulture; }
	const CString&	GetUICulture	()	const					{ return m_sUICulture; } 
	const FormatStyleLocale& GetFormatStyleLocale()	const		{ return m_FormatStyleLocale; }
	const CString&	GetCharSetSample()	const					{ return m_sCharSetSample; } 
	const TCHAR		GetCultureUpperChar	()	const				{ return m_cCultureUpperLimit; }
	const TCHAR		GetCultureLowerChar	()	const				{ return m_cCultureLowerLimit; }

	BOOL	IsInvariantCulture		()	const					{ return m_nCulture == LOCALE_INVARIANT; }
	BOOL	IsEqual					(const CString& str1, const CString& str2, BOOL bCaseSensitive = FALSE) const { return CompareStrings (str1, str2, bCaseSensitive) == 0; }
	BOOL	IsLessThan				(const CString& str1, const CString& str2, BOOL bCaseSensitive = FALSE) const { return CompareStrings (str1, str2, bCaseSensitive) < 0;  }
	BOOL	IsGreaterThan			(const CString& str1, const CString& str2, BOOL bCaseSensitive = FALSE) const { return CompareStrings (str1, str2, bCaseSensitive) > 0;  }
	void	MakeUpper				(CString& str) const;
	void	MakeLower				(CString& str) const;
	CString	GetUpperCase			(const CString& str) const;
	CString	GetLowerCase			(const CString& str) const;
	int		CompareStrings			(const CString& str1, const CString& str2, BOOL bCaseSensitive = FALSE) const;

	virtual LPCSTR  GetObjectName() const { return "CCultureInfo"; }

	static	BOOL	IsManageCompanyDatabaseCultureDisabled();
};

// Culture Info Functions

inline TB_EXPORT const CCultureInfo*					AFXAPI	AfxGetCultureInfo	();
TB_OLD_METHOD inline TB_EXPORT const CCultureInfo*		AFXAPI	AfxGetCollateCulture		() { return AfxGetCultureInfo(); } 

TB_EXPORT CStringA UnicodeToUTF8(CString strData);
TB_EXPORT CString UTF8ToUnicode(CStringA strData);
TB_EXPORT CString TBUrlUnescape(CStringA strUrl);
TB_EXPORT void InitThreadContext();

#include "endh.dex"
