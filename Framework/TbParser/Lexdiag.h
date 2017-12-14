
#pragma once

#include <TbParser\lexan.h>
                         
class CShowErrorsDlg;
class CLineText;

//includere alla fine degli include del .H
#include "beginh.dex"

//---------------------------------------------------------------------------
class TB_EXPORT LexDiagnostic : public Lexan
{
	friend CShowErrorsDlg;

private:
	BOOL		m_SkipError;
	CString		m_strErrFileName;
	CString		m_strError;
	int			m_nErrPos;
	long		m_nErrLine;
	CLineText*	m_pInputText;	// used in parse from string to save original string
								// string to be parsed "xxx\nyyy\n\0"
public:
	LexDiagnostic	( const TToken* userTokenTable = NULL);
	LexDiagnostic	( LPCTSTR pszString, const TToken* userTokenTable = NULL, long nStringLen = -1, BOOL bAllowOpenWhenEmpty = FALSE);
	virtual	~LexDiagnostic	();

public:
	virtual int		GetCurrentPos	() const;
	virtual long	GetCurrentLine	() const;
	virtual BOOL	ErrorFound		() const { return Lexan::ErrorFound() || !m_strError.IsEmpty(); }
	virtual void	ClearError		();
	
	virtual CString	GetError		() const { return m_strError; }
	virtual BOOL	SetError		(const CString& strErr, LPCTSTR = _T(""), int nCol = -1, long nLine = -1);
	
	virtual void	SetErrFileName	(const CString& strErrFileName) { m_strErrFileName = strErrFileName; }
	virtual void	ShowErrors		(BOOL bClear = TRUE);
			CString FormatErrors	();

public:
	CString	BuildErrMsg (BOOL bClear = FALSE);
};

#include "endh.dex"
