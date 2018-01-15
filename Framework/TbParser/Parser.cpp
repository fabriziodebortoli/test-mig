
#include "stdafx.h"

#include <TBNameSolver\Chars.h>

#include <TBGeneric\GeneralFunctions.h>
#include <TBGeneric\EnumsTable.h>

#include "parser.h"
// tokens
#include "TokensTable.h"

//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char BASED_CODE THIS_FILE[] = __FILE__;
#endif

// Used in parse and unparse dor DataEnum for bad values
static const TCHAR BASED_CODE szUnknown[]	= _T("Unknown");

//============================================================================
//		    Parser Class implementation
//============================================================================

// diagnostic is hanled using Error(parser.getErrStrBox()) just
// after last parse method. Obviously must call Error... if there are same error
//
//---------------------------------------------------------------------------
Parser::Parser(const TToken* userTokenTable)
:
LexDiagnostic	(userTokenTable)
{
}

// see above
//
//---------------------------------------------------------------------------
Parser::Parser(LPCTSTR pszString, const TToken* userTokenTable/*=NULL*/, long nStringLen/*=-1*/, BOOL bAllowOpenWhenEmpty /*= FALSE*/)
:
LexDiagnostic	(pszString, userTokenTable, nStringLen, bAllowOpenWhenEmpty)
{}

//---------------------------------------------------------------------------
Parser::~Parser()
{
}

//------------------------------------------------------------------------------
BOOL Parser::ParseAlias(WORD& id) { return Match(T_ALIAS) && MatchWord(id); }
BOOL Parser::ParseID(CString& id) { return MatchID(id); }
BOOL Parser::ParseTag(Token token) { return Match(token); }
BOOL Parser::ParseUserTag(int token) { return Match((Token)token); }
BOOL Parser::ParseBegin() { return Match(T_BEGIN); }
BOOL Parser::ParseEnd() { return Match(T_END); }
BOOL Parser::ParseComma() { return Match(T_COMMA); }
BOOL Parser::ParseColon() { return Match(T_COLON); }
BOOL Parser::ParseSep() { return Match(T_SEP); }
BOOL Parser::ParseSquareOpen() { return Match(T_SQUAREOPEN); }
BOOL Parser::ParseSquareClose() { return Match(T_SQUARECLOSE); }
BOOL Parser::ParseOpen() { return Match(T_ROUNDOPEN); }
BOOL Parser::ParseClose() { return Match(T_ROUNDCLOSE); }
BOOL Parser::ParseBool(BOOL& value) { return MatchBool(value); }
BOOL Parser::ParseInt(int& value) { return MatchInt(value); }
BOOL Parser::ParseWord(WORD& value) { return MatchWord(value); }
BOOL Parser::ParseLong(long& value) { return MatchLong(value); }
BOOL Parser::ParseDWord(DWORD& value) { return MatchDWord(value); }
BOOL Parser::ParseDouble(double& value) { return MatchDouble(value); }
BOOL Parser::ParseComment(Comment& value) { return MatchComment(value); }

//---------------------------------------------------------------------------
BOOL Parser::ParseString(CString& str, BOOL bEscaped /*=TRUE*/)
{
	if (!MatchString(str))
		return FALSE;

	if (!bEscaped)
		return TRUE;

	int nLen = str.GetLength();
	TCHAR* pszStr = str.GetBuffer(nLen);

	for (;;)
	{

		/* CASO DI ESCAPING basato sui delimitatori della stringa
		CString strDelim(GetCurrentStringToken().GetAt(0), 2);
		TCHAR* pszQuote = _tcsstr(pszStr, strDelim);
		if (pszQuote == NULL) break;
		*/

		// In questo caso si fa l'escaping di tutti gli '' e "" che si
		// trovano nella stringa
		//
		TCHAR* pszQuote = _tcsstr(pszStr, _T("\"\""));
		if (pszQuote == NULL)
		{
			pszQuote = _tcsstr(pszStr, _T("\'\'"));
			if (pszQuote == NULL)
				break;
		}
		else
		{
			TCHAR* pszSingQuote = _tcsstr(pszStr, _T("\'\'"));

			if (pszSingQuote && pszSingQuote < pszQuote)
				pszQuote = pszSingQuote;
		}
		//

		nLen--;
		pszQuote++;

		// si prepara per la prossima iterazione
		pszStr = pszQuote;

		// shifta a sinistra di un carattere tutto il buffer a partire dal secondo 
		// apice (doppioapice) trovato
		while (*pszQuote)
		{
			pszQuote[0] = pszQuote[1];
			pszQuote++;
		}
	}

	str.ReleaseBuffer(nLen);
	return TRUE;
}

//---------------------------------------------------------------------------
BOOL Parser::ParseDataType(DataType& aType)
{
	aType = FromTokenToDataType(SkipToken());

	if (aType == DATA_NULL_TYPE)
		return SetError(_TB("Unhandled data type"));

	if (aType != DATA_ENUM_TYPE)
		return TRUE;

	// sintassi per gli enumerativi
	//		ENUM["ENUM QUALCHE COSA"]		variabile_enumarativa;
	//		ENUM[nn]						variabile_enumarativa;
	CString strTagName;

	if (!ParseTag(T_SQUAREOPEN))
		return FALSE;

	if (LookAhead(T_STR))
	{
		if (!ParseString(strTagName))
			return FALSE;

		aType.m_wTag = AfxGetEnumsTable()->GetEnumTagValue(strTagName);
		if (aType.m_wTag == TAG_ERROR)
			return SetError(_TB("Enumeration expected"));
	}	
	else if (LookAhead(T_INT) || LookAhead(T_WORD) || LookAhead(T_LONG))
	{
		WORD wTagValue = TAG_ERROR;
		if (!ParseWord(wTagValue))
			return SetError(_TB("Enumeration expected"));
		if (!AfxGetEnumsTable()->ExistEnumTagValue(wTagValue))
			return SetError(_TB("Enumeration unknown"));

		aType.m_wTag = wTagValue;
		if (aType.m_wTag == TAG_ERROR)
			return SetError(_TB("Enumeration expected"));
	}	

	return ParseTag(T_SQUARECLOSE);
}

//---------------------------------------------------------------------------
BOOL Parser::ParseDataType(DataType& aType, DataType& aBaseType)
{
	CString sRecordName;
	return ParseDataType(aType, aBaseType, sRecordName);
}

//---------------------------------------------------------------------------
BOOL Parser::ParseDataType(DataType& aType, DataType& aBaseType, CString& sRecordName)
{
	if (!ParseDataType(aType))
	{
		return FALSE;
	}

	if (aType == DataType::Array)
	{
		if (!ParseDataType(aBaseType))
			return SetError(_TB("Wrong base datatype"));

		if (aBaseType == DataType::Array)
			return SetError(_TB("Array base datatype does not allowed"));

		if (aBaseType == DataType::Record)
			if (!ParseID(sRecordName))
				return SetError(_TB("Missing record table name"));
	}
	else if (aType == DataType::Record)
	{
		if (!ParseID(sRecordName))
			return SetError(_TB("Missing record table name"));
	}

	return TRUE;
}
// parsa in una unica stringa eventuali piu' righe di stringhe
// BEGIN "first line" "second line" END
//------------------------------------------------------------------------------
BOOL Parser::ParseCEdit (CString& strText)
{
	BOOL bBeginFound = Matched(T_BEGIN);

	if (ParseString(strText))
	{
		//Matched(T_PLUS);	//TDT style
		while ( (bBeginFound ? Matched(T_PLUS), TRUE : Matched(T_PLUS)) && LookAhead(T_STR) && !Bad() && !Eof())
		{                 
			CString strBuffer;
			if (!ParseString(strBuffer))
				return FALSE;
			//Matched(T_PLUS);	//TDT style
			strText += _T("\r\n") + strBuffer;
		}
		if (bBeginFound && !ParseEnd()) 
			return FALSE;
		return TRUE;
	}
	return FALSE;
}

// parsa in una unica stringa eventuali piu' righe di stringhe
// "first line" + "second line"
//------------------------------------------------------------------------------
BOOL Parser::ParseTDTString (CString& strText)
{
	return ParseCEdit(strText);
}

//------------------------------------------------------------------------------
BOOL Parser::ParseGUID (GUID& aGuid)
{
	CString strGUID;
	BOOL bOk = 
		ParseTag	(T_UUID)	&&
		ParseString (strGUID);

	aGuid = StringToGuid(strGUID);
	return bOk;
}


//------------------------------------------------------------------------------
BOOL Parser::ParseColor (Token token, COLORREF& dwColor)
{
	int nRed, nGreen, nBlue;
	BOOL ok1 = token == T_NULL_TOKEN ? TRUE : ParseTag	(token);

	BOOL ok = ok1 &&
		ParseOpen	()		&&
		ParseInt	(nRed)	&&
		ParseComma	()		&&
		ParseInt	(nGreen)&&
		ParseComma	()		&&
		ParseInt	(nBlue)	&&
		ParseClose	();
	if(ok)
		dwColor = RGB(nRed, nGreen, nBlue);
	return ok;
}

//---------------------------------------------------------------------------
BOOL Parser::ParseItem (CString& strItemName)
{
	return
		ParseOpen 	()				&&
		ParseID		(strItemName)	&&
		ParseClose	();
}

//---------------------------------------------------------------------------
BOOL Parser::ParseSubscr (int& nVal)
{
	return
		ParseSquareOpen	()		&&
		ParseInt		(nVal)	&&
		ParseSquareClose();
}

//---------------------------------------------------------------------------
BOOL Parser::ParseSubscr (int& nVal1, int& nVal2)
{
	BOOL ok = 
		ParseSquareOpen	() &&
		ParseInt		(nVal1);

	if (ok && Matched (T_COMMA))
		ok = ParseInt (nVal2);
	else
		nVal2 = 0;

	ok = ok && ParseSquareClose();

	return ok;
}

//-----------------------------------------------------------------------------
BOOL Parser::ParseDateTimeString(Token aType, DWORD* pValue1, DWORD* pValue2 /* = NULL */)
{
	CString strInput;
	if (!ParseString(strInput))	return FALSE;

	switch (ConvertStringToDateTime(FromTokenToDataType(aType), strInput, pValue1, pValue2))
	{
	case CONVERT_DATATIME_SUCCEEDED:
		return TRUE;

	case CONVERT_DATATIME_SYNTAX_ERROR		:
		return SetError(_TB("Syntax error converting from string to date and time"));

	case CONVERT_DATATIME_FAILED	:	
		{
			ASSERT(FALSE);
			return FALSE;
		}
	}

	return TRUE;
}

//-----------------------------------------------------------------------------
DataType Parser::ParseComplexData(DWORD* pValue1, DWORD* pValue2 /* = NULL */, BOOL parseBraceOpen/* = TRUE*/)
{
	if (parseBraceOpen && !ParseTag(T_BRACEOPEN)) 
	{
		SetError(_TB("Unhandled data type"));
		return DATA_NULL_TYPE;
	}

	if (LookAhead(T_ID))
	{
		// Date value syntax		::= '{' d "<YYYYMMDD>" '}' or '{' d"<DD/MM/YYYY>" '}'
		// DateTime value syntax	::= '{' dt"<DD/MM/YYYY HH:MM:SS>" '}'
		// DateTime value syntax	::= '{' dt"<YYYY-MM-DDTHH:MM:SS>" '}'
		// DateTime value syntax	::= '{' ts"<YYYY-MM-DD HH:MM:SS>" '}'
		// Time value syntax		::= '{' t "<HH:MM:SS>" '}'
		// ElapsedTime value syntax	::= '{' et"<DDDDD:HH:MM:SS>" '}'	//@@ElapsedTime
		// Guid ::= '{E58EA4AA-0C98-4A3C-B41B-6D212087177C}'

		CString strTag;

		if (!ParseID(strTag))	
		{
			SetError(_TB("Unhandled data type"));
			return DATA_NULL_TYPE;
		}

		Token aToken;
		if (strTag.CompareNoCase(_T("d")) == 0)
			aToken = T_TDATE;
		else if (strTag.CompareNoCase(_T("dt")) == 0 || strTag.CompareNoCase(_T("ts")) == 0)
			aToken = T_TDATETIME;
		else if (strTag.CompareNoCase(_T("t")) == 0)
			aToken = T_TTIME;
		else if (strTag.CompareNoCase(_T("et")) == 0)
			aToken = T_TELAPSED_TIME;		//@@ElapsedTime
		else
		{
			SetError(_TB("Unhandled data type"));
			return DATA_NULL_TYPE;
		}

		if (!ParseDateTimeString(aToken, pValue1, pValue2))	
		{
			SetError(_TB("Unhandled data type"));
			return DATA_NULL_TYPE;
		}
		if (!ParseTag(T_BRACECLOSE))						
		{
			SetError(_TB("Unhandled data type"));
			return DATA_NULL_TYPE;
		}
		return FromTokenToDataType(aToken);
	}

	if (LookAhead(T_STR))
	{
		// Enum value syntax ::= '{' "<TagEnumName>" : "<ItemEnumName>" '}'
		CString strTagName;
		CString	strItemName;
		WORD	wTagValue;
		WORD	wItemValue;

		if (!ParseString(strTagName)) 
			goto l_err;		

		if (AfxGetEnumsTable()->ExistEnumTagName(strTagName))
		{
			if (!ParseTag(T_COLON) || !ParseString(strItemName)) 
				goto l_err;		

			wTagValue	= AfxGetEnumsTable()->GetEnumTagValue(strTagName);
			wItemValue	= AfxGetEnumsTable()->GetEnumItemValue(strTagName, strItemName);

			if (wTagValue == TAG_ERROR || wItemValue == ITEM_ERROR)
				goto l_err;		

			if (!ParseTag(T_BRACECLOSE))
				goto l_err;		
			*pValue1 = MAKELONG(wItemValue, wTagValue);

			return DataType(DATA_ENUM_TYPE, wTagValue);
		}
	}
	else if (LookAhead(T_INT) || LookAhead(T_WORD) || LookAhead(T_LONG))
	{
		// Enum value syntax ::= '{' <TagEnumValue> : <ItemEnumValue> '}'
		WORD	wTagValue;
		WORD	wItemValue;

		if (!ParseWord(wTagValue)) 
			goto l_err;		
		if (!AfxGetEnumsTable()->ExistEnumTagValue(wTagValue)) 
			goto l_err;		

		if (!ParseTag(T_COLON)) 
			goto l_err;		

		if (!ParseWord(wItemValue)) 
			goto l_err;		
		EnumTag* pTag = AfxGetEnumsTable()->GetEnumTags()->GetTagByValue(wTagValue);

		if (!ParseTag(T_BRACECLOSE)) 
			goto l_err;		
		if (!pTag->ExistItemValue(wItemValue)) 
			goto l_err;		
		*pValue1 = MAKELONG(wItemValue, wTagValue);

		return DataType(DATA_ENUM_TYPE, wTagValue);
	}

l_err:
	SetError(_TB("Enumeration expected"));
	return DATA_NULL_TYPE;
}

//-----------------------------------------------------------------------------
DataObj* Parser::ParseComplexData(BOOL parseBraceOpen/* = TRUE*/)
{
	DWORD dwValue1 = 0;
	DWORD dwValue2 = 0;
	DataType aDataType = ParseComplexData(&dwValue1, &dwValue2, parseBraceOpen);

	switch (aDataType.m_wType)
	{
		case DATA_ENUM_TYPE : 
			return new DataEnum	(dwValue1);	

		case DATA_DATE_TYPE :
		{
			DataDate* pData = new DataDate();

			if (aDataType.IsFullDate())
				if (aDataType.IsATime())
					pData->SetAsTime();
				else
					pData->SetFullDate();

			pData->Assign((const long) dwValue1, (const long) dwValue2);
			return pData;
		}
		case DATA_LNG_TYPE :	
		{
			//@@ElapsedTime
			DataLng* pData = new DataLng((const long) dwValue1);
			pData->SetAsTime();
			return pData;
		}

		case DATA_NULL_TYPE : 
			break;	// errore di parsing
		default : 
			ASSERT(FALSE);	
			// tipo di dato non previsto
	}
	return NULL;
}

//---------------------------------------------------------------------------
BOOL Parser::ParseSignedInt	(int& nVal)
{
	int nSign = 1;
	if (LookAhead(T_MINUS))
	{
		SkipToken();
		nSign = -1;
	}
	else if (LookAhead(T_PLUS))
		SkipToken();

	BOOL ok = ParseInt (nVal);
	nVal *= nSign;
	return ok;
}

//---------------------------------------------------------------------------
BOOL Parser::ParseSignedLong	(long& nVal)
{
	long nSign = 1L;
	if (LookAhead(T_MINUS))
	{
		SkipToken();
		nSign = -1L;
	}
	else if (LookAhead(T_PLUS))
		SkipToken();

	BOOL ok = ParseLong (nVal);
	nVal *= nSign;
	return ok;
}

//---------------------------------------------------------------------------
BOOL Parser::ParseSignedDouble	(double& nVal)
{
	double nSign = 1.0;
	if (LookAhead(T_MINUS))
	{
		SkipToken();
		nSign = -1.0;
	}
	else if (LookAhead(T_PLUS))
		SkipToken();

	BOOL ok = ParseDouble (nVal);
	nVal *= nSign;
	return ok;
}

//---------------------------------------------------------------------------
BOOL Parser::LookForAFunc()
{
	switch (LookAhead())
	{
	case T_CSUM		: 
	case T_CCAT 	:
	case T_CMIN		:
	case T_CMAX		: 
	case T_CCOUNT	:
	case T_CAVG		:
	case T_CFIRST	:
	case T_CLAST	:
		return TRUE;
	}

	return FALSE;
}			

//--------------------------------------------------------------------------------
BOOL Parser::ParseInnerContent(Token begin, Token end, CString& content)
{
	if (!ParseTag(begin))
		return FALSE;

	CString sOldAudit;
	BOOL bAudit = IsAuditStringOn();
	if (bAudit)
		sOldAudit = GetAuditString();
		
	EnableAuditString();

	if (!SkipToToken2(end, FALSE, TRUE))
		return FALSE;

	content = GetAuditString();
	if (bAudit) sOldAudit.Append(content);

	content.Trim();

	EnableAuditString(bAudit);
	if (bAudit) ConcatAuditString(sOldAudit);

	if (!ParseTag(end))
		return FALSE;

	return !ErrorFound();
}

//-----------------------------------------------------------------------------
BOOL Parser::ParseBracedContent(CString& content)
{
	if (!ParseInnerContent(T_BRACEOPEN, T_BRACECLOSE, content))
		return FALSE;
	StripBlankNearSquareBrackets(content);
	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL Parser::ParseSquaredIdent(CString& ident)
{
	if (LookAhead(T_SQUAREOPEN))
	{
		if (!ParseInnerContent(T_SQUAREOPEN, T_SQUARECLOSE, ident))
			return FALSE;
		ident = '[' + ident + ']';
	}
	else if (!ParseID(ident))
			return FALSE;

	return TRUE;
}

BOOL Parser::ParseSquaredCoupleIdent(CString& strName)
{
	if (LookAhead(T_SQUAREOPEN))
	{
		if (!ParseSquaredIdent(strName))
			return FALSE;

		//if (LookAhead() && GetCurrentStringToken() == L"." && SkipToken())
		if (Matched(T_DOUBLE))
		{
			CString sCol;
			if (LookAhead(T_SQUAREOPEN))
			{
				if (!ParseSquaredIdent(sCol))
					return FALSE;
			}
			else if (!ParseID(sCol))
			{
				return FALSE;
			}
			strName.Append(L".");
			strName.Append(sCol);
		}
	}
	else 
	{
		if (!ParseID(strName))
			return FALSE;

		if (strName[strName.GetLength() -1] == '.')
		{ 
			CString sCol;
			if (!ParseSquaredIdent(sCol))
				return FALSE;
			strName.Append(sCol);
		}
	}
	return TRUE;
}

//============================================================================
//		    Unparser Class implementation
//============================================================================

const UINT UNPARSER_MODE = 
CFile::modeCreate | CFile::modeWrite | 
CFile::shareDenyRead | CFile::typeText;

//------------------------------------------------------------------------------
Unparser::Unparser(BOOL bUseMemFile /*= FALSE*/)
	:
	CLineFile			(bUseMemFile),
	m_nTabCounter		(0),
	m_bMustTab			(TRUE),
	m_pUserTokenTable	(NULL),
	m_pTokensTable		(AfxGetTokensTable())
{}

//------------------------------------------------------------------------------
Unparser::Unparser(LPCTSTR pszFileName)
	:
	CLineFile			(pszFileName, UNPARSER_MODE, FALSE),
	m_nTabCounter		(0),
	m_bMustTab			(TRUE),
	m_pUserTokenTable	(NULL),
	m_pTokensTable		(AfxGetTokensTable())
{
}

//------------------------------------------------------------------------------
BOOL Unparser::Open(LPCTSTR pszFileName, CFileException* pError)
{
	return CLineFile::Open(pszFileName, UNPARSER_MODE, pError);
}

//------------------------------------------------------------------------------
BOOL Unparser::Open(LPCTSTR pszFileName, UINT nOpenFlags, CFileException* pError)
{
	return CLineFile::Open(pszFileName, nOpenFlags, pError);
}

//------------------------------------------------------------------------------
CString Unparser::UnparseEnumItem (const DWORD& dwValue)
{
	WORD wTagValue = HIWORD(dwValue);
	WORD wItemValue = LOWORD(dwValue);

	const EnumItemArray* pItems = AfxGetEnumsTable()->GetEnumItems(wTagValue);

	if (pItems == NULL)
	{
		TRACE1("Unparse::UnparseEnumItem : unknown enum '%d'\n", wTagValue);
		ASSERT(FALSE);

		return _T("{\"ENUM UNKNOWN\"}");
	}

	CString str;
	str.Format(_T("{%d:%d}"), wTagValue, wItemValue);
	return  str;
}

//------------------------------------------------------------------------------
CString Unparser::UnparseDateTime (const DataDate& aDate)
{
	CString str;
	switch (FromDataTypeToToken(aDate.GetDataType()))
	{
	case T_TDATE		:	str = "{d\""; break;
	case T_TDATETIME	:	str = "{dt\""; break;
	case T_TTIME		:	str = "{t\""; break;
	}

	// per la data usa il formato GG/MM/AAAA
	return	str + aDate.Str(-1, 0) + _T("\"}");
}

//------------------------------------------------------------------------------
CString Unparser::UnparseElapsedTime (const DataLng& aTime)	//@@ElapsedTime
{
	ASSERT(aTime.IsATime());

	return CString(_T("{et\"")) + aTime.Str() + _T("\"}");
}

//------------------------------------------------------------------------------
void Unparser::UnparseColor (Token nTag, const COLORREF& dwColor, BOOL bNewLine)
{
	UnparseTag		(nTag,					FALSE);
	UnparseOpen		();
	UnparseInt		(GetRValue(dwColor),	FALSE);
	UnparseComma	();
	UnparseInt		(GetGValue(dwColor),	FALSE);
	UnparseComma	();
	UnparseInt		(GetBValue(dwColor),	FALSE);
	UnparseClose	(bNewLine);
}

//------------------------------------------------------------------------------
void Unparser::UnparseAlias (const WORD& id, BOOL bNewLine)
{
	UnparseTag		(T_ALIAS,	FALSE);
	UnparseWord		(id,		FALSE);
	UnparseBlank	(bNewLine);
}

//------------------------------------------------------------------------------
void Unparser::UnparseTag (Token nTag, BOOL bNewLine)
{
	CString s = GetTokensTable()->ToString(nTag);
	if (s.IsEmpty() && m_pUserTokenTable)
		s = TToken::ToString(m_pUserTokenTable, nTag);
	Write(s, FALSE);
	UnparseBlank (bNewLine);
}

//------------------------------------------------------------------------------
void Unparser::UnparseID (LPCTSTR pszTag, BOOL bNewLine)
{
	Write(pszTag, FALSE);
	UnparseBlank (bNewLine);
}

//------------------------------------------------------------------------------
void Unparser::UnparseExpr (LPCTSTR pszExpr, BOOL bNewLine)
{
	CString s(pszExpr); s.Trim(L" \t\r\n");
	Write(s, bNewLine);

	if (bNewLine)
		m_bMustTab = TRUE;
	else
		UnparseBlank (FALSE);
}

//------------------------------------------------------------------------------
void Unparser::UnparseBool (const BOOL& bVal, BOOL bNewLine)
{
	if (bVal)
		UnparseTag	(T_TRUE,	bNewLine);
	else
		UnparseTag	(T_FALSE,	bNewLine);
}

//------------------------------------------------------------------------------
void Unparser::UnparseInt (const int& nVal, BOOL bNewLine)
{
	const rsize_t nLen = 32;
	TCHAR unparserBuffer[nLen]; 
	_sntprintf_s(unparserBuffer, nLen, sizeof unparserBuffer, _T("%d"), nVal);
	Write(unparserBuffer, bNewLine);
}

//------------------------------------------------------------------------------
void Unparser::UnparseWord (const WORD& wVal, BOOL bNewLine)
{
	const rsize_t nLen = 32;
	TCHAR unparserBuffer[nLen]; 
	_sntprintf_s(unparserBuffer, nLen, sizeof unparserBuffer, _T("%u"), wVal);
	Write(unparserBuffer, bNewLine);
}

//------------------------------------------------------------------------------
void Unparser::UnparseByte (const BYTE& wVal, BOOL bNewLine)
{
	UnparseWord (wVal, bNewLine);
}

//------------------------------------------------------------------------------
void Unparser::UnparseLong (const long& nVal, BOOL bNewLine)
{
	const rsize_t nLen = 32;
	TCHAR unparserBuffer[nLen]; 
	_sntprintf_s(unparserBuffer, nLen, sizeof unparserBuffer, _T("%ld"), nVal);
	Write(unparserBuffer, bNewLine);
}

//------------------------------------------------------------------------------
void Unparser::UnparseDWord (const DWORD& dwVal, BOOL bNewLine /*= TRUE*/, BOOL bHex /*= FALSE*/)
{
	const rsize_t nLen = 32;
	TCHAR unparserBuffer[nLen]; 
	if (bHex)
		_sntprintf_s(unparserBuffer, nLen, sizeof unparserBuffer, _T("0x%.8X"), dwVal);
	else
		_sntprintf_s(unparserBuffer, nLen, sizeof unparserBuffer, _T("%lu"), dwVal);

	Write(unparserBuffer, bNewLine);

	UnparseBlank (bNewLine);
}

//------------------------------------------------------------------------------
void Unparser::UnparseDouble (const double& nVal, BOOL bNewLine, LPCTSTR sFormat)
{
	const rsize_t nLen = 256;
	TCHAR unparserBuffer[nLen]; 
	_stprintf_s(unparserBuffer, nLen, sFormat /*_T("%.15f")*/, nVal);
	Write(unparserBuffer, bNewLine);
}

//------------------------------------------------------------------------------
void Unparser::UnparseEnum(const DWORD& dwValue,BOOL bNewLine)
{
	Write(UnparseEnumItem(dwValue), bNewLine);
}

//------------------------------------------------------------------------------
void Unparser::UnparseComment(const Comment& comments,	BOOL bNewLine)
{
	if (comments.GetSize() == 0) return;

	CString strTab(BLANK_CHAR, comments.GetTabSize());

	for (int i =0 ; i < comments.GetSize(); i++)
	{
		CString s = comments.GetAt(i);
		s.Remove('\r');		s.Remove('\n');		s.TrimRight();
		if (s.IsEmpty())
			continue;

		Write(strTab, FALSE);
		Write(s, (i < comments.GetSize() - 1) ? TRUE : bNewLine);
	}
}

//------------------------------------------------------------------------------
void Unparser::UnparseComment(const CStringArray& comments,	BOOL bNewLine)
{
	if (comments.GetSize() == 0) return;

	for (int i = 0; i < comments.GetSize(); i++)
	{
		CString s = comments.GetAt(i);
		s.Remove('\r');		s.Remove('\n');		s.TrimRight();
		if (s.IsEmpty())
			continue;

		Write(s, (i < comments.GetSize() - 1) ? TRUE : bNewLine);
	}
}

// Abilita l'utilizzo del carattere di SINGLE_QUOTE come ateso dalla sintassi SQL
//------------------------------------------------------------------------------
void Unparser::UnparseSqlString
(
 LPCTSTR	pszString, 
 BOOL	bNewLine	/*=TRUE*/, 
 BOOL	bEscaped	/*=TRUE*/
 )
{
	UnparseString (pszString, bNewLine, bEscaped, TRUE);
}

//------------------------------------------------------------------------------
void Unparser::UnparseEscapedString(LPCTSTR pszSource, CString& strDest)
{
	// Esegue l'escaping sia degli ' o dei "
	//
	CString	strSource(pszSource);
	int		nPos = 0;
	TCHAR	ch;

	while (nPos >= 0)
	{
		ch = _T('"');
		nPos = strSource.Find(ch);
		if (nPos < 0)
		{
			ch = APEX_CHAR;
			nPos = strSource.Find(ch);
			if (nPos < 0)
			{
				strDest += strSource;
				break;
			}
		}
		else
		{
			TCHAR ch1 = APEX_CHAR;
			int nPos1 = strSource.Find(ch1);
			if (nPos1 >= 0 && nPos1 < nPos)
			{
				nPos = nPos1;
				ch = ch1;
			}
		}

		strDest += strSource.Left(nPos + 1) + CString(ch);
		strSource = strSource.Right(strSource.GetLength() - nPos - 1);
	}
}

//------------------------------------------------------------------------------
void Unparser::UnparseString
(
 LPCTSTR	pszString, 
 BOOL	bNewLine	/*=TRUE*/,
 BOOL	bEscaped	/*=TRUE*/,
 BOOL	bSqlType	/*=FALSE*/
 )
{
	const rsize_t nLen = 16256;
	TCHAR unparserBuffer[nLen]; 
	
	CString	strDest;

	if (bEscaped)
		Unparser::UnparseEscapedString(pszString, strDest);
	else
		strDest = pszString;

	// Supporto delle string SQL.
	if (bSqlType)
		_sntprintf_s(unparserBuffer, nLen, sizeof unparserBuffer, _T("'%s' "), (LPCTSTR)strDest);
	else
		_sntprintf_s(unparserBuffer, nLen, sizeof unparserBuffer, _T("\"%s\" "), (LPCTSTR)strDest);

	Write(unparserBuffer, bNewLine);
}

//------------------------------------------------------------------------------
void Unparser::UnparseClose (BOOL bNewLine)
{
	Write(T_ROUNDCLOSE, FALSE);
	Write(T_BLANK,	bNewLine);
}

//------------------------------------------------------------------------------
void Unparser::UnparseSquareClose (BOOL bNewLine)
{
	Write(T_SQUARECLOSE, FALSE);
	Write(T_BLANK,	bNewLine);
}


//------------------------------------------------------------------------------
void Unparser::UnparseBlank (BOOL bNewLine)
{
	if (bNewLine)
		UnparseCrLf();
	else
		Write(T_BLANK, bNewLine);
}

//------------------------------------------------------------------------------
void Unparser::UnparseBegin	(BOOL bNewLine)
{
	IncTab();
	UnparseTag (T_BEGIN, bNewLine);
	IncTab();
}

//------------------------------------------------------------------------------
void Unparser::UnparseEnd (BOOL bNewLine)
{
	DecTab();
	UnparseTag (T_END, bNewLine);
	DecTab();
}


//---------------------------------------------------------------------------
void Unparser::UnparseItem (const CString& strItemName, BOOL bNewLine)
{
	UnparseOpen		();		
	Write			(strItemName, FALSE);
	UnparseClose	(bNewLine);
}

//---------------------------------------------------------------------------
void Unparser::UnparseSubscr (const int& nVal, BOOL bNewLine)
{
	UnparseSquareOpen	();	
	UnparseInt			(nVal, FALSE);
	UnparseSquareClose	(bNewLine);
}

//---------------------------------------------------------------------------
void Unparser::UnparseSubscr (const int& nVal1, const int& nVal2, BOOL bNewLine)
{
	UnparseSquareOpen	();	
	UnparseInt			(nVal1, FALSE);
	if (nVal2)
	{
		UnparseComma	();
		UnparseInt		(nVal2, FALSE);
	}
	UnparseSquareClose	(bNewLine);
}


//---------------------------------------------------------------------------
void Unparser::UnparseGUID(const GUID& aGuid, BOOL bNewLine)
{
	CString strGUID = GuidToString(aGuid);

	UnparseTag		(T_UUID, FALSE);
	UnparseString	(strGUID, bNewLine);
}


//------------------------------------------------------------------------------
void Unparser::Write(LPCTSTR pszString, BOOL bNewLine)
{
	CString aStr;
	// first column tabulation
	if (m_bMustTab && pszString && pszString[0])
	{
		aStr = GetTokensTable()->ToString(T_TAB);
		for (int i = 0; i < m_nTabCounter; i++)
			WriteString(aStr);

		// inibisce la tabulazione fino al newline
		m_bMustTab = FALSE;
	}

	TCHAR* str = NULL;
	try {
		str = (TCHAR*)_tcschr(pszString, LF_CHAR);
	}
	catch (...)
	{
		return;
		//pszString = L"";
	}
	TCHAR ch = NULL_CHAR;

	if (str++)
	{
		// trovato LF allora si mette il tappo dopo
		// salvando il carattere presente
		ch = *str;
		*str = NULL_CHAR;
	}

	// Write string
	WriteString (pszString);

	// testando il ch salvato si evita di fare la chiamata se l'ultimo carattere
	// della stringa e` proprio il LF
	if (ch)
	{
		// si ripristina il carattere salvato per eseguire la scrittura
		// della riga successiva
		//
		*str = ch;
		m_bMustTab = TRUE;	// si ripristina la tabulazione

		// viene chiamata ricorsivamente la Write per scrivere le righe
		// tabbando opportunamente
		//
		Write(str, bNewLine);
	}

	// si ripristina la tabulazione
	if (bNewLine)
		UnparseCrLf();
}

//---------------------------------------------------------------------------
void Unparser::UnparseCrLf ()
{
	CString aStr;
	aStr = GetTokensTable()->ToString(T_NEWLINE);
	WriteString(aStr);
	m_bMustTab = TRUE;
}

// Ignora il Tag per enum
//---------------------------------------------------------------------------
CString Unparser::DataTypeToString(const DataType& dataType, const TokensTable*  pTokensTable/* = AfxGetTokensTable()*/)
{
	CString strTmp = pTokensTable->ToString(FromDataTypeToToken(dataType));

	if (dataType == DATA_ENUM_TYPE)
	{
		CString str;
		str.Format(_T("[%d]"), dataType.m_wTag);
		strTmp += str;
	}
	return strTmp;
}

void Unparser::UnparseDataType(const DataType& dataType, BOOL bNewLine, BOOL bIndent, const CString& recName)
{
	CString strTmp = DataTypeToString(dataType, GetTokensTable());

	if (bIndent)
		UnparseID(cwsprintf(_T("%-*s"), max(strTmp.GetLength(), 12), (LPCTSTR)strTmp), FALSE);
	else
		UnparseID(strTmp, FALSE); 

	if (!recName.IsEmpty())
		UnparseID(recName, FALSE); 

	if (bNewLine)
		UnparseCrLf();
}

void Unparser::UnparseDataType(const DataType& dataType, const DataType& baseDataType, BOOL bNewLine, BOOL bIndent, const CString& recName)
{
	if (baseDataType != DataType::Null && dataType == DataType::Array)
	{
		UnparseDataType(dataType,		false,		bIndent);
		UnparseDataType(baseDataType,	bNewLine,	bIndent, recName);
	}
	else
		UnparseDataType(dataType,		bNewLine,	bIndent, recName);
}

//------------------------------------------------------------------------------
void Unparser::UnparseCEdit (const CString& strText, BOOL bNewLine, BOOL bTDTStyle/*=FALSE*/)
{   
	if (strText.IsEmpty())
	{
		UnparseString(strText, bNewLine);
		return;
	}

	CString strNewText (strText);
	strNewText.Remove('\r');

	int nStart = 0;
	int nEnd = 0;

	BOOL bMultiLine = FALSE;

	for (int i = 0; i < strNewText.GetLength(); i++)
	{
		if (strNewText[i] == LF_CHAR || (nEnd - nStart) >= 8128)
		{
			if (!bMultiLine)
			{
				UnparseCrLf();
				if (!bTDTStyle) UnparseBegin();
				bMultiLine = TRUE;
			}

			UnparseString(strNewText.Mid(nStart, nEnd - nStart), FALSE);
			if (i < strNewText.GetLength() - 1)
			{
				if (bTDTStyle) UnparseTag(T_PLUS);
				UnparseCrLf();
			}

			nStart = nEnd = i + 1;
			continue;
		}
		nEnd++;
	}

	// write remaining string in source string don't terminate with CR-LF pairs
	if (nEnd > nStart)
	{
		UnparseString(strNewText.Mid(nStart, nEnd - nStart), bNewLine);
	}

	if (bMultiLine)
	{
		if (!bNewLine) UnparseCrLf();
		if (!bTDTStyle) UnparseEnd();
	}
}

// simile a quella precedente solo che scrive tutti in una linea separando
// le diverse righe della tringa da unparsare con il simbolo + (es: "aa"+"bb")
//------------------------------------------------------------------------------
void Unparser::UnparseTDTString (const CString& strText, BOOL bNewLine)
{   
	UnparseCEdit (strText, bNewLine, TRUE);
}

//------------------------------------------------------------------------------
BOOL Parser::ParseByte(BYTE& value)
{
	WORD aWord = 0;
	BOOL bOk = MatchWord (aWord); 
	if (bOk) value = (BYTE)aWord;

	return bOk;
}

//------------------------------------------------------------------------------
void Unparser::Write(Token token,BOOL bNewline)
{ 
	CString aStr = GetTokensTable()->ToString(token);

	Write(aStr, bNewline); 
}

//------------------------------------------------------------------------------
void Unparser::IncTab	()	{ m_nTabCounter++; }
void Unparser::DecTab	()	{ if (m_nTabCounter > 0) m_nTabCounter--; }

void Unparser::UnparseComma		(BOOL newline)	{ Write(T_COMMA,		newline); }
void Unparser::UnparseColon		(BOOL newline)	{ Write(T_COLON,		newline); }
void Unparser::UnparseOpen		(BOOL newline)	{ Write(T_ROUNDOPEN,	newline); }
void Unparser::UnparseSquareOpen(BOOL newline)	{ Write(T_SQUAREOPEN,	newline); }
void Unparser::UnparseSep		(BOOL newline)	{ Write(T_SEP,			newline); }

//-----------------------------------------------------------------------------
DataType FromTokenToDataType(const Token& aToken)
{
	switch (aToken)
	{
	case T_TSTR			:	return DataType::String;
	case T_TINTEGER		:	return DataType::Integer;
	case T_TLONG		:	return DataType::Long;
	case T_TELAPSED_TIME:	return DataType::ElapsedTime;	//@@ElapsedTime
	case T_TVAR:			return DataType::Object;	//@@Handle
	case T_TDOUBLE		:	return DataType::Double;
	case T_TMONEY		:	return DataType::Money;
	case T_TQUANTITY	:	return DataType::Quantity;
	case T_TPERCENT		:	return DataType::Percent;
	case T_TDATE		:	return DataType::Date;
	case T_TDATETIME	:	return DataType::DateTime;
	case T_TTIME		:	return DataType::Time;
	case T_TBOOL		:	return DataType::Bool;
	case T_TENUM		:	return DataType::Enum;
	case T_UUID			:	return DataType::Guid;
	case T_TTEXT		:	return DataType::Text;		
	case T_TBLOB		:	return DataType::Blob;
	case T_ARRAY		:	return DataType::Array;
	case T_RECORD		:	return DataType::Record;
	default				:	return DataType::Null;
	}
}

//-----------------------------------------------------------------------------
Token FromDataTypeToToken(const DataType& aDataType)
{
	switch (aDataType.m_wType)
	{
	case DATA_STR_TYPE	:	return T_TSTR;
	case DATA_INT_TYPE	:	return T_TINTEGER;
	case DATA_LNG_TYPE	:	return aDataType.IsATime() ? 
										T_TELAPSED_TIME : //@@ElapsedTime
										(aDataType.IsAHandle() ? T_TVAR : T_TLONG);		
	case DATA_DBL_TYPE	:	return T_TDOUBLE;
	case DATA_MON_TYPE	:	return T_TMONEY;
	case DATA_QTA_TYPE	:	return T_TQUANTITY;
	case DATA_PERC_TYPE	:	return T_TPERCENT;
	case DATA_DATE_TYPE	:	return !aDataType.IsFullDate() ? T_TDATE : aDataType.IsATime() ? T_TTIME : T_TDATETIME;
	case DATA_BOOL_TYPE	:	return T_TBOOL;
	case DATA_ENUM_TYPE	:	return T_TENUM;
	case DATA_GUID_TYPE	:	return T_UUID;
	case DATA_TXT_TYPE	:	return T_TTEXT;			
	case DATA_BLOB_TYPE	:	return T_TBLOB;
	case DATA_ARRAY_TYPE	:	return T_ARRAY;
	case DATA_RECORD_TYPE	:	return T_RECORD;
	case DATA_TRECORD_TYPE	:	return T_RECORD;
	default				:	return T_NOTOKEN;
	}
}

//-----------------------------------------------------------------------------
CString FromDataTypeToTokenString (const DataType& aDataType, const TokensTable*  pTokensTable/* = AfxGetTokensTable()*/)
{
	if (aDataType.m_wType == DATA_NULL_TYPE)
		return _T("NullType") ;
	if (aDataType.m_wType == DATA_VARIANT_TYPE)
		return _T("VariantType") ;

	CString aStr = pTokensTable->ToString(FromDataTypeToToken(aDataType));
	return aStr;
}

