
#pragma once

#include <TbGeneric\DataObj.h>
#include "lexdiag.h"
#include <TbGeneric\linefile.h>

//includere alla fine degli include del .H
#include "beginh.dex"

//=============================================================================
class TB_EXPORT Parser : public LexDiagnostic
{
public:
	Parser	(const TToken* userTokenTable = NULL);
	Parser	(LPCTSTR pszString, const TToken* userTokenTable = NULL, long nStringLen = -1, BOOL bAllowOpenWhenEmpty = FALSE);
	virtual ~Parser	();

	// useful parse funtion
	BOOL ParseAlias		(WORD& id);
	BOOL ParseID		(CString& id);
	BOOL ParseTag		(Token token);
	BOOL ParseUserTag	(int token);
	BOOL ParseBegin		();
	BOOL ParseEnd		();
	BOOL ParseComma		();
	BOOL ParseColon		();
	BOOL ParseSep		();
	BOOL ParseString	(CString& str, BOOL bEscaped = TRUE);
	BOOL ParseColor		(Token, COLORREF&);
	BOOL ParseColor		(COLORREF& dwColor);
	BOOL ParseItem		(CString& itemName);
	BOOL ParseSubscr	(int& val);
	BOOL ParseSubscr	(int& val1, int& val2);
	BOOL ParseDataType	(DataType&);
	BOOL ParseDataType	(DataType&, DataType&);
	BOOL ParseDataType	(DataType&, DataType&, CString&);
	BOOL ParseCEdit		(CString&);
	BOOL ParseTDTString	(CString&);
	BOOL ParseGUID		(GUID& guid);
	BOOL ParseComment	(Comment&);
	BOOL LookForAFunc	();
	
	BOOL ParseSquareOpen	();
	BOOL ParseSquareClose	();
	BOOL ParseOpen			();
	BOOL ParseClose			();

	// parse only number withou any sign (-3 or +3 not allowed, 3 allowed )
	BOOL ParseByte			(BYTE& value);
	BOOL ParseBool			(BOOL& value);
	BOOL ParseInt			(int& value);
	BOOL ParseWord			(WORD& value);
	BOOL ParseLong			(long& value);
	BOOL ParseDWord			(DWORD& value);
	BOOL ParseDouble		(double& value);
	BOOL ParseDateTimeString(Token aType, DWORD* pValue1, DWORD* pValue2 = NULL);

	DataType	ParseComplexData	(DWORD* pValue1, DWORD* pValue2 = NULL, BOOL parseBraceOpen = TRUE);
	DataObj*	ParseComplexData	(BOOL parseBraceOpen = TRUE);

	// parse also negative number (-3 allowed, +3 not allowed, 3 allowed )
	BOOL ParseSignedInt		(int& value);
	BOOL ParseSignedLong	(long& value);
	BOOL ParseSignedDouble	(double& value);

	BOOL ParseInnerContent	(Token begin, Token end, CString& content);
	BOOL ParseSquaredIdent	(CString& content);	// returns [Orders details], orderID
	BOOL ParseBracedContent (CString& content);	// returns inner text of calculated columns/contentof without braces
	BOOL ParseSquaredCoupleIdent(CString& content);	// returns table.column, [table].[column], [column], column
};

//=============================================================================
class TB_EXPORT Unparser : public CLineFile
{
protected:
	const TokensTable*  m_pTokensTable;
	const TToken*		m_pUserTokenTable;

public:
	// Constructors
	Unparser	(BOOL bUseMemFile = FALSE);
	Unparser	(LPCTSTR pszFileName);
		
	void SetTokensTable (const TokensTable* pTokensTable) { m_pTokensTable = pTokensTable; }
	const TokensTable* GetTokensTable () const { return m_pTokensTable; }

	void Attach(const TToken* pUserTokenTable) { m_pUserTokenTable = pUserTokenTable; }

public:
	static	CString	UnparseEnumItem		(const DWORD& dwValue);
	static	CString	UnparseDateTime		(const DataDate& aDate);
	static	CString	UnparseElapsedTime	(const DataLng& aTime);	//@@ElapsedTime
	static void	UnparseEscapedString(LPCTSTR pszSource, CString& strDest);

public:
	virtual BOOL Open	(LPCTSTR pszFileName, UINT nOpenFlags, CFileException* pError = NULL);
	virtual BOOL Open	(LPCTSTR pszFileName, CFileException* pError = NULL);

	// must used instead of WriteString because mantain tabbing
	virtual void Write	(LPCTSTR pszString,	BOOL bNewline = TRUE);
	virtual void Write	(Token token,		BOOL bNewline = TRUE);

	// useful parse funtion
	void UnparseColor		(Token tag, const COLORREF&,	BOOL newline = TRUE);
	void UnparseAlias		(const WORD&,					BOOL newline = TRUE);
	void UnparseID			(LPCTSTR,						BOOL newline = TRUE);

	virtual void UnparseExpr		(LPCTSTR,						BOOL newline = TRUE);
                                                        	
	void UnparseSqlString	(LPCTSTR	pszString, BOOL bNewLine = TRUE, BOOL bEscaped = TRUE);
	void UnparseString		(LPCTSTR	pszString, BOOL bNewLine = TRUE, BOOL bEscaped = TRUE, BOOL bSqlType	= FALSE);

	void UnparseTag			(Token tag,					BOOL newline = TRUE);
	void UnparseUserTag		(int tag,					BOOL newline = TRUE) { UnparseTag((Token)tag, newline); } 
	void UnparseItem		(const CString&, 			BOOL newline = TRUE);
	void UnparseDataType	(const DataType&,	 		BOOL newline = TRUE, BOOL bIndent = FALSE, const CString& = L"");
	void UnparseDataType	(const DataType&,	 		const DataType& baseDataType, BOOL newline = TRUE, BOOL bIndent = FALSE, const CString& = L"");
	void UnparseCEdit		(const CString&, 			BOOL newline = TRUE, BOOL bTDTStyle = FALSE);
	void UnparseTDTString	(const CString&, 			BOOL newline = TRUE);
	void UnparseSubscr		(const int&,		 		BOOL newline = TRUE);
	void UnparseSubscr		(const int&, const int&,	BOOL newline = TRUE);
	void UnparseGUID		(const GUID&,				BOOL newline = TRUE);

	void UnparseByte		(const BYTE& value,			BOOL newline = TRUE);
	void UnparseBool		(const BOOL& value,			BOOL newline = TRUE);
	void UnparseInt			(const int& value,			BOOL newline = TRUE);
	void UnparseWord		(const WORD& value,			BOOL newline = TRUE);
	void UnparseLong		(const long& value,			BOOL newline = TRUE);
	void UnparseDWord		(const DWORD& value,		BOOL newline = TRUE, BOOL bHex = FALSE);
	void UnparseDouble		(const double& value,		BOOL newline = TRUE, LPCTSTR sFormat = _T("%.15f"));
	void UnparseEnum		(const DWORD& value,		BOOL newline = TRUE);
	void UnparseComment		(const Comment& value,		BOOL newline = TRUE);
	void UnparseComment		(const CStringArray& comments,	BOOL bNewLine = TRUE);

	void UnparseClose		(BOOL newline = TRUE);
	void UnparseSquareClose	(BOOL newline = TRUE);
	void UnparseBegin		(BOOL newline = TRUE);
	void UnparseEnd			(BOOL newline = TRUE);
	void UnparseBlank		(BOOL newline = FALSE);

	void UnparseCrLf		();
	void UnparseComma		(BOOL newline = FALSE);
	void UnparseColon		(BOOL newline = FALSE);
	void UnparseOpen		(BOOL newline = FALSE);
	void UnparseSquareOpen	(BOOL newline = FALSE);
	void UnparseSep			(BOOL newline = FALSE);


	// tab character counter inc/dec function
	void IncTab	();
	void DecTab	();
	int  GetTabCounter() const { return m_nTabCounter; }

	virtual BOOL IsLocalizableTextInCurrentLanguage() { return FALSE; }
	virtual CString LoadReportString(const CString& sText) { return sText; };

	static CString DataTypeToString (const DataType& dataType, const TokensTable*  pTokensTable = AfxGetTokensTable());

protected:
	int		m_nTabCounter;
    BOOL	m_bMustTab;
};

// Funzioni generali per la conversione dei token nei tipi base
//=============================================================================
TB_EXPORT DataType	FromTokenToDataType			(const Token& aToken);
TB_EXPORT CString	FromDataTypeToTokenString	(const DataType& aType, const TokensTable*  pTokensTable = AfxGetTokensTable());
TB_EXPORT Token		FromDataTypeToToken			(const DataType& aDataType);

#include "endh.dex"
