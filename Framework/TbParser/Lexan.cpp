
#include "stdafx.h"
#include <string.h>
#include <ctype.h>
#include <limits.h>
#include <malloc.h>

// this .cpp file
#include <TbNameSolver\Chars.h>
#include <TbNameSolver\FileSystemFunctions.h>
#include <TbNameSolver\ThreadContext.h>

#include <TbGeneric\GeneralFunctions.h>
#include <TbGeneric\linefile.h>
#include <TbGeneric\schedule.h>
#include <TbGeneric\stack.h>
#include <TbGeneric\globals.h>


#include <TbParser\TokensTable.h>
#include <TbParser\parser.h>

#include "lexan.h"

//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static const char BASED_CODE THIS_FILE[] = __FILE__;
#endif


//================================================================

typedef enum
		{
			OPERATOR,
			BRACKET,
			ALPHA,
			DIGITS,
			EXPONENT,
			DQUOTE,
			SQUOTE,
			SLASH,
			STAR,
			SEPARATOR,
			SPACE,
			ZERO,
			HEXSYMB,
			HEXALPHA,
			DECSEP
		}
		CharClass;

typedef enum { LAST, SKIP, INCL} Action;

const
	//............. Transition state table for Finite State Machine
	FsmState BASED_CODE transition_table[18][15] =
	{
  /*OPERATOR BRACKET ALPHA    DIGITS   EXPONENT DQUOTE   SQUOTE   SLASH    STAR    SEP     SPACE	ZERO	HEXSYMB HEXALPHA DECSEP */

	{OPER,   BRACK,   ID,     NUM,     ID,      STRING,  SSTRING, CMT_BEG,  OPER,   SEP,    START,	ZERONUM,ID,		ID,     NUM		}, /*START*/
	{OPER,   BRACK,   ID,     NUM,     ID,      STRING,  SSTRING, CMT_BEG,  OPER,   SEP,    START,	ZERONUM,ID,		ID,     NUM		}, /*OPER*/
	{OPER,   BRACK,   ID,     ID,      ID,      STRING,  SSTRING, CMT_BEG,  OPER,   SEP,    START,	ID,     ID,		ID,     ID		}, /*ID*/
	{OPER,   BRACK,   ID,     NUM,     EXP,     STRING,  SSTRING, CMT_BEG,  OPER,   SEP,    START,	NUM,    ID,		ID,     NUM		}, /*NUM*/
	{NUM,    BRACK,   ID,     NUM,     ID,      STRING,  SSTRING, CMT_BEG,  NUM,    SEP,    START,	NUM,    ID,		ID,     NUM		}, /*EXP*/
	{STRING, STRING,  STRING, STRING,  STRING,  ENDSTR,  STRING,  STRING,  STRING, STRING, STRING,	STRING, STRING,	STRING, STRING	}, /*STRING*/
	{OPER,   BRACK,   ID,     NUM,     ID,      STRING,  SSTRING, CMT_BEG,  OPER,   SEP,    START,	ZERONUM,ID,		ID,     NUM		}, /*ENDSTR*/
	{SSTRING,SSTRING, SSTRING,SSTRING, SSTRING, SSTRING, ENDSSTR, SSTRING, SSTRING,SSTRING,SSTRING,	SSTRING,SSTRING,SSTRING,SSTRING	}, /*SSTRING*/
	{OPER,   BRACK,   ID,     NUM,     ID,      STRING,  SSTRING, CMT_BEG,  OPER,   SEP,    START,	ZERONUM,ID,		ID,     NUM		}, /*ENDSSTR*/
	{OPER,   BRACK,   ID,     NUM,     ID,      STRING,  SSTRING, CMT_BEG,  OPER,   SEP,    START,	ZERONUM,ID,		ID,     NUM		}, /*SEP*/
	{OPER,   BRACK,   ID,     NUM,     ID,      STRING,  SSTRING, CMT_BEG,  OPER,   SEP,    START,	ZERONUM,ID,		ID,     NUM		}, /*BRACK*/
	{OPER,   BRACK,   ID,     NUM,     EXP,     STRING,  SSTRING, CMT_BEG,  OPER,   SEP,    START,	ZERONUM,HEXNUM,	ID,     NUM		}, /*ZERONUM*/
	{OPER,   BRACK,   ID,     HEXNUM,  HEXNUM,  STRING,  SSTRING, CMT_BEG,  OPER,   SEP,    START,	HEXNUM, ID,		HEXNUM, ID		}, /*HEXNUM*/

		    /*  analize comments in C++ form */

	{CMNT,   CMNT,    CMNT,   CMNT,    CMNT,    CMNT,    CMNT,    CMNT,    CMT_LST, CMNT,   CMNT,	CMNT,	CMNT,	CMNT,   CMNT	}, /*CMNT*/
	{CMNT,   CMNT,    CMNT,   CMNT,    CMNT,    CMNT,    CMNT,    CMT_END,  CMT_LST, CMNT,   CMNT,	CMNT,	CMNT,	CMNT,   CMNT	}, /*CMT_LST*/
	{OPER,   BRACK,   ID,     NUM,     ID,      STRING,  SSTRING, CMT_BEG,  OPER,   SEP,    START,	NUM,	ID,		ID,     NUM		}, /*CMT_END*/
	{OPER,   BRACK,   ID,     NUM,     ID,      STRING,  SSTRING, CMT_EOL,  CMNT,   SEP,    START,	NUM,	ID,		ID,     NUM		}, /*CMT_BEG*/
	{CMT_EOL, CMT_EOL,  CMT_EOL, CMT_EOL,  CMT_EOL,  CMT_EOL,  CMT_EOL,  CMT_EOL,  CMT_EOL, CMT_EOL, CMT_EOL,	CMT_EOL,	CMT_EOL,	CMT_EOL, CMT_EOL	}, /*CMT_EOL*/
	};

const
	// emit table row entry fsm, column entry
	Action BASED_CODE emit_table[18][15] =
	{
  /*OPERATOR BRACKET ALPHA    DIGITS   EXPONENT DQUOTE   SQUOTE   SLASH    STAR    SEP     SPACE	ZERO	HEXSYMB	 HEXALPHA	DECSEP*/

	{SKIP,   SKIP,   SKIP,    SKIP,    SKIP,    SKIP,    SKIP,    SKIP,    SKIP,   SKIP,   SKIP,    SKIP,   SKIP,    SKIP,		SKIP}, /*START*/
	{INCL,   LAST,   LAST,    LAST,    LAST,    LAST,    LAST,    LAST,    INCL,   LAST,   LAST,    LAST,   LAST,    LAST,		LAST}, /*OP*/
	{LAST,   LAST,   INCL,    INCL,    INCL,    LAST,    LAST,    LAST,    LAST,   LAST,   LAST,    INCL,   INCL,    INCL,		INCL}, /*ID*/
	{LAST,   LAST,   LAST,    INCL,    INCL,    LAST,    LAST,    LAST,    LAST,   LAST,   LAST,    INCL,   LAST,    LAST,		INCL}, /*NUM*/
	{INCL,   LAST,   INCL,    INCL,    LAST,    LAST,    LAST,    LAST,    INCL,   LAST,   LAST,    INCL,   INCL,    INCL,		INCL}, /*EXP*/
	{INCL,   INCL,   INCL,    INCL,    INCL,    INCL,    INCL,    INCL,    INCL,   INCL,   INCL,    INCL,   INCL,    INCL,		INCL}, /*STRING*/
	{LAST,   LAST,   LAST,    LAST,    LAST,    INCL,    LAST,    LAST,    LAST,   LAST,   LAST,    LAST,   LAST,    LAST,		LAST}, /*ENDSTR*/
	{INCL,   INCL,   INCL,    INCL,    INCL,    INCL,    INCL,    INCL,    INCL,   INCL,   INCL,    INCL,   INCL,    INCL,		INCL}, /*SSTRING*/
	{LAST,   LAST,   LAST,    LAST,    LAST,    LAST,    INCL,    LAST,    LAST,   LAST,   LAST,    LAST,   LAST,    LAST, 	 	LAST}, /*ENDSSTR*/
	{LAST,   LAST,   LAST,    LAST,    LAST,    LAST,    LAST,    LAST,    LAST,   LAST,   LAST,    LAST,   LAST,    LAST, 	 	LAST}, /*SEP*/
	{LAST,   LAST,   LAST,    LAST,    LAST,    LAST,    LAST,    LAST,    LAST,   LAST,   LAST,    LAST,   LAST,    LAST, 	 	LAST}, /*BRACK*/
	{LAST,   LAST,   LAST,    INCL,    LAST,    LAST,    LAST,    LAST,    LAST,   LAST,   LAST,    INCL,   INCL,    LAST, 	 	INCL}, /*ZERONUM*/
	{LAST,   LAST,   LAST,    INCL,    INCL,    LAST,    LAST,    LAST,    LAST,   LAST,   LAST,    INCL,   LAST,    INCL,		LAST}, /*HEXNUM*/

		    /*  analize comments in C++ form */

	{INCL,   INCL,   INCL,    INCL,    INCL,    INCL,    INCL,    INCL,    INCL,   INCL,   INCL,    INCL,   INCL,    INCL,      INCL}, /*CMNT*/
	{INCL,   INCL,   INCL,    INCL,    INCL,    INCL,    INCL,    INCL,    INCL,   INCL,   INCL,    INCL,   INCL,    INCL,		INCL}, /*CMT_LST*/
	{LAST,   LAST,   LAST,    LAST,    LAST,    LAST,    LAST,    LAST,    LAST,   LAST,   LAST,    LAST,   LAST,    LAST,		LAST}, /*CMT_END*/
	{LAST,   LAST,   LAST,    LAST,    LAST,    LAST,    LAST,    INCL,    INCL,   LAST,   LAST,    LAST,   LAST,    LAST,		LAST}, /*CMT_BEG*/
	{INCL,   INCL,   INCL,    INCL,    INCL,    INCL,    INCL,    INCL,    INCL,   INCL,   INCL,    INCL,   INCL,    INCL,		INCL}, /*CMT_EOL*/
	};


// unicode compatible character class translation
class CharClassTable
{
public:
	static CharClass GetCharClass(const TCHAR c) 
	{
		if (_T_ISCNTRL(c) || c == BLANK_CHAR || c == SLASH_CHAR || c == SHARP_CHAR) return SPACE;
		if (c == _T(';')) return SEPARATOR;
		if (c == _T('0')) return ZERO;
		if (_istdigit(c)) return DIGITS;
		if (c == DOT_CHAR) return DECSEP;
		if (c == ASTERISK_CHAR) return STAR;
		if (c == APEX_CHAR) return SQUOTE;
		if (c == _T('"')) return DQUOTE;
		if (c == URL_SLASH_CHAR) return SLASH;
		if (c == _T('!') || c == _T('$') || c == _T('%') || c == _T('&') || c == _T('+') || c == _T('-') || c == _T('<') || c == _T('=') || c == _T('>') || c == _T('?') || c == _T('^') || c == _T('~') || c == _T('`') || c == _T('|')) return OPERATOR;
		if (c == _T('(') || c == _T(')') || c == _T(',') || c == _T(':') || c == _T('[') || c == _T(']') || c == _T('{') || c == _T('}')) return BRACKET;

		TCHAR u = _T_TOUPPER(c);
		if (u == _T('E')) return EXPONENT;
		if (u == _T('X')) return HEXSYMB;
		if (u == _T('A') || u == _T('B') || u == _T('C') || u == _T('D') || u == _T('E') || u == _T('F')) return HEXALPHA;

		return ALPHA;
	}
};
	//Old ASCII conversion table

	//CharClass BASED_CODE class_table[] =
	//{
	///* ^@ */ SPACE,     /* ^A */ SPACE,      /* ^B */ SPACE,    /* ^C */ SPACE,
	///* ^D */ SPACE,     /* ^E */ SPACE,      /* ^F */ SPACE,    /* ^G */ SPACE,
	///* ^H */ SPACE,     /* ^I */ SPACE,      /* ^J */ SPACE,    /* ^K */ SPACE,
	///* ^L */ SPACE,     /* ^M */ SPACE,      /* ^N */ SPACE,    /* ^O */ SPACE,
	///* ^P */ SPACE,     /* ^Q */ SPACE,      /* ^R */ SPACE,    /* ^S */ SPACE,
	///* ^T */ SPACE,     /* ^U */ SPACE,      /* ^V */ SPACE,    /* ^W */ SPACE,
	///* ^X */ SPACE,     /* ^Y */ SPACE,      /* ^Z */ SPACE,    /* ^[ */ SPACE,
	///* ^\ */ SPACE,     /* ^] */ SPACE,      /* ^^ */ SPACE,    /* ^_ */ SPACE,

	///*   */  SPACE,     /* ! */  OPERATOR,   /* " */ DQUOTE,      /* # */ SPACE,
	///* $ */  OPERATOR,  /* % */  OPERATOR,   /* & */ OPERATOR,    /* ' */ SQUOTE,
	///* ( */  BRACKET,   /* ) */  BRACKET,    /* * */ STAR,        /* + */ OPERATOR,
	///* , */  BRACKET,   /* - */  OPERATOR,   /* . */ DECSEP,      /* / */ SLASH,
	///* 0 */  ZERO,	  /* 1 */  DIGITS,     /* 2 */ DIGITS,      /* 3 */ DIGITS,
	///* 4 */  DIGITS,    /* 5 */  DIGITS,     /* 6 */ DIGITS,      /* 7 */ DIGITS,
	///* 8 */  DIGITS,    /* 9 */  DIGITS,     /* : */ BRACKET,     /* ; */ SEPARATOR,
	///* < */  OPERATOR,  /* = */  OPERATOR,   /* > */ OPERATOR,    /* ? */ OPERATOR,
	///* @ */  ALPHA,     /* A */  HEXALPHA,   /* B */ HEXALPHA,    /* C */ HEXALPHA,
	///* D */  HEXALPHA,  /* E */  EXPONENT,   /* F */ HEXALPHA,    /* G */ ALPHA,
	///* H */  ALPHA,     /* I */  ALPHA,      /* J */ ALPHA,       /* K */ ALPHA,
	///* L */  ALPHA,     /* M */  ALPHA,      /* N */ ALPHA,       /* O */ ALPHA,
	///* P */  ALPHA,     /* Q */  ALPHA,      /* R */ ALPHA,       /* S */ ALPHA,
	///* T */  ALPHA,     /* U */  ALPHA,      /* V */ ALPHA,       /* W */ ALPHA,
	///* X */  HEXSYMB,   /* Y */  ALPHA,      /* Z */ ALPHA,       /* [ */ BRACKET,
	///* \ */  SPACE,     /* ] */  BRACKET,    /* ^ */ OPERATOR,    /* _ */ ALPHA,
	///* ` */  OPERATOR,  /* a */  HEXALPHA,   /* b */ HEXALPHA,    /* c */ HEXALPHA,
	///* d */  HEXALPHA,  /* e */  EXPONENT,   /* f */ HEXALPHA,    /* g */ ALPHA,
	///* h */  ALPHA,     /* i */  ALPHA,      /* j */ ALPHA,       /* k */ ALPHA,
	///* l */  ALPHA,     /* m */  ALPHA,      /* n */ ALPHA,       /* o */ ALPHA,
	///* p */  ALPHA,     /* q */  ALPHA,      /* r */ ALPHA,       /* s */ ALPHA,
	///* t */  ALPHA,     /* u */  ALPHA,      /* v */ ALPHA,       /* w */ ALPHA,
	///* x */  HEXSYMB,   /* y */  ALPHA,      /* z */ ALPHA,       /* { */ BRACKET,
	///* - */  OPERATOR,  /* { */  BRACKET,    /* ~ */ OPERATOR,    /* del */ SPACE
	//};

                  
const CString CommentTerminator[] = {_T("\r\n"), _T("\r"), _T("\n"), _T("\0"), _T("")};

#define LEXAN_BUFFER_SIZE 8192

//......................................................... Token definition
//

//=======================================================================
//				class Symbol implementation
//=======================================================================

//----------------------------------------------------------------------------
Symbol::Symbol()
{
	m_State = START;
	m_Lexeme = new TCHAR[1];
	m_Lexeme[0] = NULL_CHAR;
 }

//----------------------------------------------------------------------------
Symbol::Symbol (const Symbol& symbol)
{
	m_State = symbol.m_State;
	m_Lexeme = new TCHAR[_tcslen(symbol.m_Lexeme) + 1];
	TB_TCSCPY(m_Lexeme, symbol.m_Lexeme);
}

//----------------------------------------------------------------------------
Symbol& Symbol::operator= (const Symbol& symbol)
{
	if (this != &symbol)
	{
		m_State = symbol.m_State;
		if (m_Lexeme) delete [] m_Lexeme;
		m_Lexeme = new TCHAR[_tcslen(symbol.m_Lexeme) + 1];
		TB_TCSCPY(m_Lexeme, symbol.m_Lexeme);
	}
	return *this;
}

//----------------------------------------------------------------------------
Symbol::Symbol (FsmState aState, LPCTSTR pszSource, int nSize)
{
	m_Lexeme = new TCHAR[nSize + 1];
	TB_TCSNCPY(m_Lexeme, pszSource, nSize);
	m_Lexeme[nSize] = NULL_CHAR;
	m_State = aState;
}

//----------------------------------------------------------------------------
CString Symbol::GetStateDescription	()	const	
{ 
	switch(m_State)
	{
		case START          :return _TB("START");
		case OPER	        :return _TB("OPERATOR");
		case ID             :return _TB("ID");
		case NUM            :return _TB("NUM");
		case EXP            :return _TB("EXP");
		case STRING         :return _TB("STRING");
		case ENDSTR		    :return _TB("ENDSTRING");
		case SSTRING        :return _TB("START_STRING");
		case ENDSSTR        :return _TB("END_STRING");
		case SEP            :return _TB("SEP");
		case BRACK		    :return _TB("BRACKET");
		case ZERONUM		:return _TB("ZERONUM");
		case HEXNUM			:return _TB("HEXNUM");
		case CMNT		    :return _TB("COMMENT");
		case CMT_LST			:return _TB("LAST_CHAR_COMMENT");
		case CMT_END			:return _TB("END_COMMENT");
		case CMT_BEG			:return _TB("BEGIN_COMMENT");
		case CMT_EOL			:return _TB("COMMENT_TO_EOL");
		case ENDF			:return _TB("END_OF_FILE");
	}

	ASSERT(FALSE); 
	return _T("");
}

//=======================================================================
//				class Preprocessor implementation
//=======================================================================
//---------------------------------------------------------------------------
Preprocessor::Preprocessor(Lexan* pParent)
	:
	m_pLexan	(pParent)
{
}

//----------------------------------------------------------------------------
Preprocessor::~Preprocessor()
{
}

//----------------------------------------------------------------------------
BOOL Preprocessor::ParseDefine(LPCTSTR pszBuffer)
{
	Parser* pLex = new Parser(pszBuffer);

	CString strDefine;
	DWORD	dwDefine = 1;	// default value for define
	int		nCurrPos = 0;	
	
	pLex->SkipToken();
	pLex->SetExpandDefine(FALSE);
	pLex->ParseID(strDefine);
	pLex->SetExpandDefine();
	

	// don't allow redefine of already declared define
	if (m_pLexan->GetDefine(strDefine, dwDefine))
	{
		nCurrPos = pLex->GetCurrentPos();
		delete pLex;
		return m_pLexan->SetError
			(
				_TB("DEFINE already defined"), _T(""),
				nCurrPos, 
				m_pLexan->GetCurrentLine()
			);
	}
				
	switch (pLex->LookAhead())
	{
		case T_ID 	: 
		{
			CString strIdDefine;

			pLex->SetExpandDefine(FALSE);
			pLex->ParseID(strIdDefine);
			pLex->SetExpandDefine();
			if (m_pLexan->GetDefine(strIdDefine, dwDefine))
			{
				m_pLexan->AddDefine(strDefine, dwDefine);
				break;
			}

			nCurrPos = pLex->GetCurrentPos();
			delete pLex;
			// second search for tag value in define table failed
			// define non already defined for tag value
			return m_pLexan->SetError
					(
						_TB("Reexpansion on DEFINE not yet declared:"), strIdDefine,
						nCurrPos,
						m_pLexan->GetCurrentLine()
					);
		}
			
    	case T_INT	:
    	case T_WORD	:
    	case T_LONG	:
    	case T_DWORD: 
    		pLex->ParseDWord(dwDefine); 
			m_pLexan->AddDefine(strDefine, dwDefine);
    		break;

    	case T_EOF	:
    		// empty define use 1 as default value
			m_pLexan->AddDefine(strDefine, 1);
    		break;
    		
    	default:
			// define value not allowed
			if (!ParseMacroDefine(pLex, strDefine))
			{
				nCurrPos = pLex->GetCurrentPos();
				delete pLex;
				return m_pLexan->SetError
						(
							_TB("Invalid value for the DEFINE directive"), _T(""),
							nCurrPos, 
							m_pLexan->GetCurrentLine()
						);
			}
	}

	delete pLex;
	return TRUE;
}

//----------------------------------------------------------------------------
BOOL Preprocessor::ParseUndef(LPCTSTR pszBuffer)
{
	Parser* pLex = new Parser(pszBuffer);

	// skip UNDEF token
	pLex->SkipToken();

	// Parse define id ignorin remaining tokens on same line
	CString strDefine;
	if (!pLex->ParseID(strDefine))
		return FALSE;

	// remove DEFINE from table	(give error on undefined define)
	if (!m_pLexan->RemoveDefine(strDefine))
	{
		delete pLex;
		return m_pLexan->SetError
			(
				_TB("DEFINE not yet declared"), _T(""),
				pLex->GetCurrentPos(), 
				m_pLexan->GetCurrentLine()
			);
	}
	
	delete pLex;
	return TRUE;
}


//----------------------------------------------------------------------------
BOOL Preprocessor::ParseInclude(LPCTSTR pszBuffer)
{
	//devo parsare il file dell'include
	//il nome del file deve essere incluso come "path\filename.ext" in modo da poterlo ricercare
	//nella directory path oppure (se non presente) nella directory dove è presente il file che lo 
	//include

	CString strFileInclude;
	m_pLexan->SkipToken();
	if (!m_pLexan->MatchString(strFileInclude) || strFileInclude.IsEmpty()) 
	{
		m_pLexan->SetError
					(
						_TB("Invalid INCLUDE file"), _T(""),
						m_pLexan->GetCurrentPos(), 
						m_pLexan->GetCurrentLine()
					);
		return FALSE;
	}

	if (IsRelativePath(strFileInclude))
		strFileInclude = MakeFilePath(m_pLexan->m_pLexanState->m_strWorkingFolder, strFileInclude);

	//vado in ricorsione per parsare il file incluso
	//per prima cosa salvo lo stato attuale del lexan 
	m_pLexan->SaveLexState();
	
	//apro il file e lo parso
	if (!m_pLexan->Open(strFileInclude, FALSE))
	{
		//se non sono riuscita ad aprirlo faccio il reset del vecchio stato
		CString	e = m_pLexan->m_pLexanState->m_pLexBuf->GetException();
		SAFE_DELETE(m_pLexan->m_pLexanState->m_pLexBuf);
		m_pLexan->ResetLexState();
		m_pLexan->SetError
				(
					_TB("Unable to open the INCLUDE file:"), 
					strFileInclude + _T(" ") + e,
					m_pLexan->GetCurrentPos(), 
					m_pLexan->GetCurrentLine()
				);
		return FALSE;
	}
	
	return TRUE;
}

//----------------------------------------------------------------------------
BOOL Preprocessor::ParseBuffer(LPCTSTR pszBuffer)
{
	//se sono alla fine del file (o stringa) non faccio niente
	if (m_pLexan->m_pLexanState->m_pLexBuf->Eof()) return TRUE;

    // Empty Line, so normal parse action
    if (pszBuffer && (_tcslen(pszBuffer) < 1 || pszBuffer[0] != SHARP_CHAR))
    	return TRUE;

	BOOL ok = TRUE;
	++pszBuffer;
	switch (m_pLexan->LookAhead())
	{
		case T_DEFINE:	ok = ParseDefine(pszBuffer);break;
		case T_UNDEF:	ok = ParseUndef(pszBuffer);	break;
		case T_INCLUDE: ok = ParseInclude(pszBuffer); break;
		default :		
			ok = m_pLexan->SetError
					(
						_TB("Invalid preprocessor directive"), _T(""),
						m_pLexan->GetCurrentPos(), 
						m_pLexan->GetCurrentLine()
					);
			break;
	}
		
	// preprocessor directive go here for all line contents
	// load next line to try normal parse or preprocessor parse again
	return ok && m_pLexan->m_pLexanState->m_pLexBuf->LoadBuffer() && m_pLexan->PreprocessLine();
}

//=======================================================================
//				class Lexbuffer implementation
//=======================================================================

//----------------------------------------------------------------------------
LexBuffer::LexBuffer (Lexan* pLexan)
	:
	m_pBufferInputText	(NULL),
	m_bFromFile		(TRUE),
	m_pLexan		(pLexan)
{
	InitLexBuffer();
}

//----------------------------------------------------------------------------
LexBuffer::LexBuffer (LPCTSTR pszParseString, Lexan* pLexan, long nTotalByte, BOOL bAllowOpenWhenEmpty/* = FALSE*/)
	:
	m_pBufferInputText (NULL),
	m_bFromFile	(FALSE),
	m_pLexan	(pLexan)
{
	ASSERT(pszParseString);

	InitLexBuffer();

	// per la gestione dello stato di compilazione
	m_nTotalBytes = nTotalByte < 0
					? _tcslen(pszParseString)
					: nTotalByte;

	if (m_nTotalBytes == 0 && bAllowOpenWhenEmpty)
	{
		m_bFromFile	= TRUE;	//Riccardo: in certi contesti tento il parsing da stringa e se è vuota apro un file chiamando la Open
		return;
	}

	m_pBufferInputText = new CLineText(pszParseString, m_nTotalBytes);

	// alloca i buffers e legge la prima linea
	AllocLexBuffer();
}

//----------------------------------------------------------------------------
void LexBuffer::InitLexBuffer ()
{
	ASSERT(m_pLexan);

	m_pInputFile			= new CLineFile();

	m_pszBuffer				= NULL;
	m_nBufferLen			= -2;
	m_nStart				= -1;
	m_nFinish				= -1;
	m_nLineNumber			= 0;
	m_bError				= FALSE;
	m_nTotalBytes			= 0L;
	m_nParsedBytes			= 0L;
	m_bAuditStringOn		= FALSE;
	m_Scheduler				= NULL;
	m_bExternalScheduler	= FALSE;
	m_bNoScheduler			= TRUE;
	m_bTicEnabled		    = FALSE;
}

//----------------------------------------------------------------------------
BOOL LexBuffer::Open(LPCTSTR pszFilename, BOOL bTicEnabled)
{                    
	// open is valid only for parse from file not for string
	if (!m_bFromFile) 
		return TRUE;

	// on open can choose if monitoring parse progress status using tic
	m_bTicEnabled = FALSE;//bTicEnabled && !AfxIsInUnattendedMode();
	
	// start local or attached external scheduler for multitasking emulation
	SchedulerStart();

	// allocate buffers and read first line
	AllocLexBuffer();

    // must use static member GetStatus because NT version as bug
    // test file exit
	UINT nFlags = CFile::modeRead | CFile::shareDenyWrite | CFile::typeText;
	CFileStatus	status;
	if (!CLineFile::GetStatus(pszFilename, status))
	{
		SetHardwareError();
		m_FileException.m_cause = CFileException::fileNotFound;
    	return FALSE;
	}

    // test open error like access denied
	if (!m_pInputFile->Open(pszFilename, nFlags, &m_FileException))
	{
		SetHardwareError();
    	return FALSE;
	}

	// set values for calculate compile progress
	m_nTotalBytes	= (long) status.m_size;
	m_nParsedBytes	= 0;

	return TRUE;
}

//----------------------------------------------------------------------------
void LexBuffer::DeleteContent()
{
	//ASSERT(m_pInputFile);

	// It must deletes m_pszBuffer only when parsing from file because from string we use
	// TextString::GetLine return
	if (m_bFromFile)
	{
		if (m_pszBuffer)
		{
			delete[] m_pszBuffer;
			m_pszBuffer = NULL;
		}

		// Close only for successfully open
		if (m_pInputFile && !HardwareError() && m_pInputFile->GetHandleFile() != CFile::hFileNull)
			m_pInputFile->Close();
	}

	SAFE_DELETE(m_pBufferInputText);
	SAFE_DELETE(m_pInputFile);

	// return control to pump message main loop
	SchedulerStop();
}

//----------------------------------------------------------------------------
LexBuffer::~LexBuffer ()
{
	DeleteContent();
}

//----------------------------------------------------------------------------
LPCTSTR LexBuffer::GetException	()
{
	return PCause(&(m_FileException));
}

//----------------------------------------------------------------------------
void LexBuffer::StartProgress(Scheduler* aScheduler)
{
	m_bTicEnabled = FALSE;//!AfxIsInUnattendedMode();

	Attach(aScheduler);
	SchedulerStart();
}
	
// signal to user front-end begin of parse from file.
// UM_PARSE_BEGIN must be a unique User message
//
//----------------------------------------------------------------------------
void LexBuffer::SchedulerStart ()
{   
	// test disabled tic
	if (!m_bTicEnabled) return;
	
	if (m_bNoScheduler)
	{
		// no scheduler attached (inhibit OnIdle), message go to MainWindow directly
		if (AfxGetApp()->m_pMainWnd)
			AfxGetApp()->m_pMainWnd->SendMessage(UM_PARSE_BEGIN);
		return;
	}
	
	// attach local scheduler, otherwise use one passed to lexan
	if (!m_Scheduler)
		m_Scheduler = new Scheduler;
	
	if (!m_Scheduler->IsAttached())		
		m_Scheduler->Attach(AfxGetApp()->m_pMainWnd->m_hWnd);

	m_Scheduler->SendMessage(UM_PARSE_BEGIN);
}
        
        
// signal to user front-end end of parse from file.
// UM_PARSE_END must be a unique User message
//
//----------------------------------------------------------------------------
void LexBuffer::SchedulerStop ()
{
	// test disabled tic
	if (!m_bTicEnabled) return;

	if (m_bNoScheduler)
	{
		// no scheduler attached (inhibit OnIdle), message go to MainWindow directly
		if (AfxGetApp()->m_pMainWnd)
			AfxGetApp()->m_pMainWnd->SendMessage(UM_PARSE_END);
		return;
	}
	        
	// use attached scheduler to signal end of parse (remove it if local)	        
	if (m_Scheduler)
	{
		m_Scheduler->SendMessage(UM_PARSE_END);

		if (!m_bExternalScheduler)
	    {
			delete m_Scheduler;
			m_Scheduler = NULL;
		}
	}
}
        
// signal to user front-end end of parse from file.
// signal compile from file progress pumping message and executing OnIdle
// UM_PARSE_TIC must be a unique User message
//
//----------------------------------------------------------------------------
void LexBuffer::SchedulerTic ()
{
	// test disabled tic
	if (!m_bTicEnabled) return;

	WORD wPerc = 0;
	if (m_nTotalBytes)	// non si puo' usare la MulDiv perche' accetta solo int
		wPerc = (WORD) ((double) m_nParsedBytes / (double) m_nTotalBytes * 100.0);

	if (m_bNoScheduler)
	{
		// no scheduler attached (inhibit OnIdle), message go to MainWindow directly
		if (AfxGetApp()->m_pMainWnd)
			AfxGetApp()->m_pMainWnd->SendMessage(UM_PARSE_TIC, (WPARAM) wPerc);
		return;
	}

	if (m_Scheduler)
	{
		if (m_Scheduler->IsAborted())
		{
			m_nBufferLen = LEX_END_OF_FILE;
			return;
		}

		m_Scheduler->SendMessage(UM_PARSE_TIC, (WPARAM) wPerc);
	}
}

//----------------------------------------------------------------------------
void LexBuffer::Attach (Scheduler* aScheduler)
{                         
	// if already attached go ahead. (attach used after open);
	if (m_Scheduler) return;

	// no scheduler attached (inhibit OnIdle), message go to MainWindow directly
	if (!aScheduler)
	{
		m_bNoScheduler = TRUE;
		return;
	}
	
	m_Scheduler = aScheduler;
	m_bExternalScheduler = TRUE;
}

//----------------------------------------------------------------------------
void LexBuffer::AllocLexBuffer ()
{
	// need buffer if parsing from file
	if (m_bFromFile)
    {
		m_pszBuffer		= new TCHAR[LEXAN_BUFFER_SIZE];
		m_pszBuffer[0]	= NULL_CHAR;
    }
}

//----------------------------------------------------------------------------
void LexBuffer::ConcatAuditString(const CString& strLexeme)
{
	if (m_bAuditStringOn)
		m_strAuditString += strLexeme + _T(" ");
}

//----------------------------------------------------------------------------
CString LexBuffer::GetAuditString(BOOL bReset)
{
	CString Temp(m_strAuditString);

	if (bReset)
		m_strAuditString = "";

	return Temp;
}


//----------------------------------------------------------------------------
BOOL LexBuffer::LoadBuffer()
{
	// control end of file exception or bad file exception
	if (Eof()  || HardwareError())
		return FALSE;

    // reset lexeme pointers
	m_nFinish		= -1;
	m_nStart		= -1;

    // count processed lines.
	m_nLineNumber++;

    // signal compile from file progress pumping message and executing OnIdle
    SchedulerTic();

    // read from string
	if (!m_bFromFile)
	{
		m_nBufferLen = m_pBufferInputText->GetLine(m_pszBuffer);
		m_pszLineStart = m_pBufferInputText->GetLineStart();

		if (m_nBufferLen < 0)
		{
			m_nBufferLen = LEX_END_OF_FILE;
			return TRUE;
		}
		
		// add carriage return to audit string (skipped by lexan)
		if (m_bAuditStringOn)
			m_strAuditString += "\r\n";

		return TRUE;
	}
    
	if (Eof())
		return TRUE;

	int nBufIdx = 0;
	size_t nNewBufSize = LEXAN_BUFFER_SIZE;
	
	TRY	// try to read from file
	{
		while (TRUE)
		{
			// read from bufferd file set eof condition.
			if (m_pInputFile->ReadString(&m_pszBuffer[nBufIdx], LEXAN_BUFFER_SIZE))
			{
				m_nBufferLen = _tcslen(m_pszBuffer);
				if	(
						(m_nBufferLen == LEXAN_BUFFER_SIZE - 1)	&&
						m_pszBuffer[m_nBufferLen - 1] != LF_CHAR
					)
				{
					nBufIdx += LEXAN_BUFFER_SIZE - 1;
					nNewBufSize += LEXAN_BUFFER_SIZE;

					if (_msize(m_pszBuffer) < nNewBufSize)
					{
						TCHAR* pB = new TCHAR[nNewBufSize];
						memcpy(pB, m_pszBuffer, _msize(m_pszBuffer));
						delete  [] m_pszBuffer;
						m_pszBuffer = pB;
					}
					continue;
				}
			
				// add carriage return to audit string (skipped by lexan)
				if (m_bAuditStringOn)
					m_strAuditString += "\r\n";
			}
			else
				// eof condition
				m_nBufferLen = LEX_END_OF_FILE;

			break;
		}
	}
	CATCH (CFileException, e)
	{
		m_FileException.m_lOsError = e->m_lOsError;
		m_FileException.m_cause = e->m_cause;
		SetHardwareError();
		return FALSE;
	}
	END_CATCH

	return TRUE;
}

//----------------------------------------------------------------------------
CString LexBuffer::GetDoubleSlashComment() const 
{
	// abbiamo solo un cr e non un cr-lf alla fine del buffer
	int nLenght = m_nBufferLen - m_nStart;
	if (m_pszBuffer && m_nStart >= 0 && nLenght > 0)
	{
		TCHAR* pszTemp = new TCHAR[nLenght + 1];
		TB_TCSNCPY(pszTemp, &(m_pszBuffer[m_nStart]), nLenght);
		pszTemp[nLenght] = NULL_CHAR;

		CString strTemp (pszTemp);
		delete [] pszTemp;
		return strTemp;
	}
	return _T("");
}

BOOL LexBuffer::GetSlashStarComment(CString& sComment)  
{
	// abbiamo solo un cr e non un cr-lf alla fine del buffer
	BOOL openComment = FALSE;
	BOOL closeComment = FALSE;

	if (m_nStart == -1) m_nStart = 0;
	int nLenght = m_nBufferLen - m_nStart;
	if (m_pszBuffer && m_nStart >= 0 && nLenght > 0)
	{
		TCHAR* p = &(m_pszBuffer[m_nStart]);

		int i;
		for (i = 0 ; i < (nLenght - 1); i++) 
		{
			if ( p[i] == '/' && p[i + 1] == '*') 
			{
				openComment = TRUE;
				break;
			}
		}
		if (!openComment)
			i = 0;
		int j = openComment ? i + 2 : 0;
		for (; j < (nLenght - 1); j++) 
		{
			if ( p[j] == '*' &&  p[j + 1] == '/') 
			{	
				closeComment = TRUE;
				break; 
			}
		}
		if (!closeComment)
			j = nLenght - 1;

		int l = min(j - i + 2, nLenght);
		TCHAR* pszTemp = new TCHAR[l + 1];
		TB_TCSNCPY(pszTemp, &(p[i]), l);
		pszTemp[l] = NULL_CHAR;

		sComment = pszTemp;
		delete [] pszTemp;

		if (openComment) ASSERT(sComment.Left(2) == L"/*");

		if (closeComment) 
		{
			ASSERT(sComment.Right(2) == L"*/");

			m_nStart += j + 1;
			m_nFinish = m_nStart;
			return TRUE;
		}
	}
	return FALSE;
}

//=======================================================================
//				class LexanState implementation
//=======================================================================

//----------------------------------------------------------------------------
LexanState::LexanState()	
{
	InitLexState();
}


//----------------------------------------------------------------------------
LexanState::LexanState(const LexanState& OldState)
{
	m_NewState			= OldState.m_NewState;			
	m_pLexBuf			= OldState.m_pLexBuf;
	m_strFileName		= OldState.m_strFileName;
	m_CurrentSymbol		= OldState.m_CurrentSymbol;
	m_bNoCurrentSymbol	= OldState.m_bNoCurrentSymbol;
	m_nErrNo			= OldState.m_nErrNo;
	m_TokenExpected		= OldState.m_TokenExpected;
	m_TokenFound		= OldState.m_TokenFound;
	m_nLastAtof			= OldState.m_nLastAtof;	
	m_bExpandDefine		= OldState.m_bExpandDefine;
	m_strWorkingFolder	= OldState.m_strWorkingFolder;
}

//----------------------------------------------------------------------------
LexanState::~LexanState()	
{
	SAFE_DELETE(m_pLexBuf);
}

//----------------------------------------------------------------------------
void LexanState ::InitLexState()	
{
	m_pLexBuf = NULL;
	m_strFileName = "";	
	m_NewState = START;		
	m_bNoCurrentSymbol = TRUE;
	m_nErrNo =0;		
	m_nLastAtof = 0.0;	
	m_bExpandDefine = TRUE;
	m_strWorkingFolder = "";
}

//=======================================================================
//				class Comment implementation
//=======================================================================
//
//----------------------------------------------------------------------------
Comment::Comment()
:
	m_nTabSize(1)
{
}

//----------------------------------------------------------------------------
Comment::Comment(const Comment& aComment)
{
	*this = aComment;
}

//----------------------------------------------------------------------------
CString Comment::All(BOOL noLeadingSlash, LineTerminator terminator) const 
{
	CString strAll;
	CString strTemp;
	for (int i = 0; i < GetSize(); i++)
	{
		strTemp = GetAt(i);
		strAll += ((noLeadingSlash && (strTemp.GetLength() > 2)) ? strTemp.Right(2) : strTemp);
		strAll += CommentTerminator[(int)terminator];
	}
	return strAll;
}

//----------------------------------------------------------------------------
void Comment::AddDoubleSlashComment(const CString& strComment)
{
	CString strDelimiter = _T("//");
	int idx = strComment.Find(strDelimiter);
	if (idx == 0) 
	{
		__super::Add(strComment);
	}
	else if (idx > 0)
	{
		ASSERT(FALSE);
		__super::Add(strComment.Mid(idx));
	}
	else
		__super::Add(strDelimiter + strComment);
}

int Comment::Add(CString strComment)
{
	strComment.Trim(); 
	if (strComment == L"\r\n") 
		strComment.Empty();
	if (!strComment.IsEmpty()) 
		return __super::Add(strComment);
	return -1;
}

//----------------------------------------------------------------------------
/// <summary>
///  Ricarica un oggetto Comment a partire da una stringa aggiungendo il simbolo 
///  di commento a inizio riga se non lo trova. Genera tante righe quanti sono i
///  separatori di riga nella stringa di origine, ignorando l'ultimo
/// </summary>
void Comment::Reload(const CString& strComment, LineTerminator terminator)
{
	RemoveAll();

	int nStartIndex = 0;
	int nFoundPos = 0;
	CString strSearch = CommentTerminator[(int) terminator];

	while (nFoundPos >= 0 && nStartIndex < strComment.GetLength())
	{
		nFoundPos = strComment.Find(strSearch, nStartIndex);
		if (nFoundPos >= 0)
		{
			AddDoubleSlashComment(strComment.Mid(nStartIndex, nFoundPos - nStartIndex));
			nStartIndex = nFoundPos + strSearch.GetLength();
		}
	}
	if (nStartIndex < strComment.GetLength())
		AddDoubleSlashComment(strComment.Mid(nStartIndex, strComment.GetLength() - nStartIndex));
}
//----------------------------------------------------------------------------
Comment& Comment::operator =(const Comment& aComment)
{
	m_nTabSize = aComment.m_nTabSize;
	return *this;
}

//=======================================================================
//				class Lexan implementation
//=======================================================================

#pragma warning(disable:4355) // disabilita la warning sull'uso del this del parent
//----------------------------------------------------------------------------
Lexan::Lexan (const TToken* userTokenTable)
	:
	m_pLexanState			(NULL),
    m_pUserTokenTable		(userTokenTable),
	m_pPreprocessor			(NULL),
	m_pDefines				(NULL),
	m_pStateStack			(NULL),
	m_bExternalPreprocessor	(FALSE),
	m_pTokensTable			(AfxGetTokensTable())
{
	m_pPreprocessor	= new Preprocessor(this);
	m_pDefines		= new CMapStringToPtr;
	m_pStateStack	= new Stack();
	m_pLexanState	= new LexanState();
	m_pLexanState->m_pLexBuf	= new LexBuffer(this);
}


//----------------------------------------------------------------------------
Lexan::Lexan 
	(
		LPCTSTR				pszInputString, 
		const TToken*		userTokenTable,
		long				nStringLen, 
		BOOL				bAllowOpenWhenEmpty/* = FALSE*/
	)
	:
	m_pLexanState			(NULL),
	m_pUserTokenTable		(userTokenTable),
	m_pPreprocessor			(NULL),
	m_pDefines				(NULL),
	m_pStateStack			(NULL),
	m_bExternalPreprocessor	(FALSE),
	m_pTokensTable			(AfxGetTokensTable())
{
	m_pPreprocessor	= new Preprocessor(this);
	m_pDefines		= new CMapStringToPtr();
	m_pStateStack	= new Stack();
	m_pLexanState	= new LexanState();
	
	m_pLexanState->m_pLexBuf = new LexBuffer(pszInputString, this, nStringLen, bAllowOpenWhenEmpty);	
}

#pragma warning(default:4355)
                                                        
//----------------------------------------------------------------------------
Lexan::~Lexan ()
{               
	ASSERT(m_pPreprocessor);
	ASSERT(m_pDefines);
	ASSERT(m_pLexanState);
	ASSERT(m_pStateStack);
	
	if (!m_bExternalPreprocessor)
	{
		delete m_pPreprocessor;
		m_pPreprocessor	= NULL;
	}

	//cancello gli eventuali stati precedenti 
	//(non recuperati a causa di un errore in un include)
	m_pStateStack->ClearStack();

	delete m_pDefines;
	delete m_pStateStack;	
	delete m_pLexanState;
	
	m_pDefines		= NULL;
	m_pStateStack	= NULL;
	m_pLexanState	= NULL;

}

//----------------------------------------------------------------------------
void Lexan::Abort()
{
	if (m_pLexanState && m_pLexanState->m_pLexBuf)
	{
		m_pLexanState->m_pLexBuf->DeleteContent();
	}
}

//----------------------------------------------------------------------------
void Lexan::ResetLexState()	
{
	LexanState* pOldState = (LexanState*) m_pStateStack->Pop();

	ASSERT(pOldState);
	SAFE_DELETE(m_pLexanState);
	m_pLexanState = pOldState;
	SetErrFileName (m_pLexanState->m_strFileName);	
}

//----------------------------------------------------------------------------
void Lexan::SaveLexState()
{
	LexanState* pOldState = new LexanState(*m_pLexanState);

	m_pStateStack->Push(pOldState);
	m_pLexanState->InitLexState();

	m_pLexanState->m_pLexBuf = new LexBuffer(this);
	m_pLexanState->m_pLexBuf->Attach(NULL);
	
	SetErrFileName (m_pLexanState->m_strFileName);		
}


//----------------------------------------------------------------------------
BOOL Lexan::Open (LPCTSTR pszFilename, BOOL bTicEnabled /*TRUE*/)
{
	m_pLexanState->m_strFileName = pszFilename;
	m_pLexanState->m_strFileName.MakeLower();

	//gli include file devono stare nella stessa dir del file
	m_pLexanState->m_strWorkingFolder = GetPath(m_pLexanState->m_strFileName);

	// initialize file where can occur parse error
	SetErrFileName (m_pLexanState->m_strFileName);

	return m_pLexanState->m_pLexBuf->Open (pszFilename, bTicEnabled);
}

//----------------------------------------------------------------------------
void Lexan::SetNewPreprocessor	(Preprocessor*	pNewPrep) 
{ 
	if (pNewPrep && !m_bExternalPreprocessor) 
	{
		//delete old preprocessor
		SAFE_DELETE(m_pPreprocessor);
		m_pPreprocessor = pNewPrep; 
		m_pPreprocessor->AttachParent(this);
		m_bExternalPreprocessor = TRUE;
	}
}

//----------------------------------------------------------------------------
BOOL Lexan::DefinePresent (CString strKey) const
{
 	LPVOID pValue;
	strKey.MakeLower();

	return m_pDefines->Lookup(strKey, pValue);
}

//----------------------------------------------------------------------------
BOOL Lexan::AddDefine (CString strKey, DWORD dwValue)
{
 	LPVOID	pValue;
	strKey.MakeLower();

//@@ TODO bisogna controllare che non collida
	BOOL bFound = m_pDefines->Lookup(strKey, pValue);
	
	if (!bFound)
		m_pDefines->SetAt (strKey, (LPVOID) dwValue);

	return bFound;
}

//----------------------------------------------------------------------------
BOOL Lexan::RemoveDefine (CString strKey)
{
	strKey.MakeLower();
	return m_pDefines->RemoveKey(strKey);
}

//----------------------------------------------------------------------------
BOOL Lexan::GetDefine (CString strKey, DWORD& dwValue) const
{
 	LPVOID	pValue;
	strKey.MakeLower();

	BOOL ok = m_pDefines->Lookup(strKey, pValue);
	dwValue = DWORD(pValue);

	return ok;
}
//-----------------------------------------------------------------------
Token Lexan::GetIdToken ()
{   
	// Normal ID processing (no DEFINE)		
	Token token = GetTokensTable()->GetKeywordsToken(m_pLexanState->m_CurrentSymbol.GetLexeme());
	if (token != T_NOTOKEN)
		return token;

	// user defined keyword (if any)
	if (m_pUserTokenTable)
	{
		token =  TToken::GetToken(m_pUserTokenTable, m_pLexanState->m_CurrentSymbol.GetLexeme());
		if (token != T_NOTOKEN)
			return token;
	}

	// Enable DEFINES espansion only on ID token
	if (m_pLexanState->m_bExpandDefine)
	{
		DWORD dwDefine;
		if (GetDefine(m_pLexanState->m_CurrentSymbol.GetLexeme(), dwDefine))
		{
			m_pLexanState->m_nLastAtof = dwDefine;
			return ConvertNumericToken(m_pLexanState->m_nLastAtof);
	    }
	}
	return T_ID;
}

//----------------------------------------------------------------------------
Token Lexan::GetBracketsToken ()
{
	Token token = GetTokensTable()->GetBracketsToken(m_pLexanState->m_CurrentSymbol.GetLexeme());
	if (token == T_NOTOKEN)
		m_pLexanState->m_nErrNo++;
		
	return token;
}

//----------------------------------------------------------------------------
Token Lexan::GetOperatorsToken ()                        
{
	Token token = GetTokensTable()->GetOperatorsToken(m_pLexanState->m_CurrentSymbol.GetLexeme());
	if (token == T_NOTOKEN)
		m_pLexanState->m_nErrNo++;
		
	return token;
}


//----------------------------------------------------------------------------
BOOL Lexan::Eof() 
{
	if (m_pLexanState->m_pLexBuf->Eof())
	{
		//non è un file incluso
		if (m_pStateStack->IsEmpty()) return TRUE;
		//è un file incluso, devo ripristinare il vecchio stato
		ResetLexState();
	}

	return FALSE;
}

//----------------------------------------------------------------------------
void Lexan::ProcessCommentEOL()
{
	m_Comment.m_nTabSize = m_pLexanState->m_pLexBuf->GetCurrentPos() + 1;
	m_Comment.Add(m_pLexanState->m_pLexBuf->GetDoubleSlashComment());

	m_pLexanState->m_NewState = START;

	if (m_pLexanState->m_pLexBuf->LoadBuffer()) 
		PreprocessLine();
}

//----------------------------------------------------------------------------
void Lexan::ProcessCommentBeginEnd()
{
	m_Comment.m_nTabSize = m_pLexanState->m_pLexBuf->GetCurrentPos() + 1;
	CString sComm ;
	while (!m_pLexanState->m_pLexBuf->GetSlashStarComment(sComm))
	{
		m_Comment.Add(sComm);

		if (m_pLexanState->m_pLexBuf->LoadBuffer())
			PreprocessLine();
		else break;
	}
	m_Comment.Add(sComm);

	m_pLexanState->m_NewState = START;
}

//----------------------------------------------------------------------------
void Lexan::GetCommentTrace	(CStringArray& comments, BOOL bErase/* = TRUE*/)
{ 
	LookAhead();	//serve per posizionarsi sul primo token valido, consumando tutti gli eventuali commenti che lo precedono

	m_CommentTrace.Append(m_Comment); m_Comment.RemoveAll(); 

	comments.Append(m_CommentTrace); 
	if (bErase) 
		m_CommentTrace.RemoveAll();
}

//----------------------------------------------------------------------------
void Lexan::RemoveCommentTrace	()
{ 
	m_CommentTrace.RemoveAll();
}

//----------------------------------------------------------------------------
Symbol Lexan::GetNewLexeme()
{
	CharClass     character;
	FsmState      state = START;
	Action        action;
	TCHAR         i;

	do
	{
		m_CommentTrace.Append(m_Comment); m_Comment.RemoveAll();

		m_pLexanState->m_pLexBuf->Rewind();
		do
		{               
			if (m_pLexanState->m_pLexBuf->Eob())
			{
				// try to preprocess current buffer line		
				if (m_pLexanState->m_pLexBuf->LoadBuffer()) 
					PreprocessLine();				
				if (Eof()) 
					break;
			}

			state = m_pLexanState->m_NewState;				// starting state
			i = m_pLexanState->m_pLexBuf->GetNextChar();		// advance the lexeme end character in the buffer
			m_pLexanState->m_pLexBuf->IncrementTic();		// monitor compile progress

			character = CharClassTable::GetCharClass(i);					// determinate class character
			m_pLexanState->m_NewState = transition_table[state][character];	// determinate the new state of FSM
			action = emit_table[state][character];							// action corresponding to new state

			if (m_pLexanState->m_NewState == CMT_EOL)
			{
				// ignore the remaining characters in the line reloading e rewinding buffer
				ProcessCommentEOL();
			}
			else if (state == CMT_BEG && m_pLexanState->m_NewState == CMNT)
			{
				// ignore the remaining characters in the next lines reloading e rewinding buffer more until CMT_END
				ProcessCommentBeginEnd();
				//state = CMT_END;
			}

			// skip the lexeme advancing the starting pointer
			if (action == SKIP)
			{
				m_pLexanState->m_pLexBuf->Rewind();
			}
		}
		while ((action != LAST) && (!Eof()));
	}
	while (((state == CMNT) || (state == CMT_END) || (state == CMT_LST)) && (!Eof()));

	if (Eof())
	{
		Symbol theSymbol (ENDF,_T(""),0);
		return theSymbol;
	}
	else
	{
		Symbol theSymbol (state,m_pLexanState->m_pLexBuf->GetLexemeStart(),m_pLexanState->m_pLexBuf->GetLexemeSize());
		return theSymbol;
	}
}

//----------------------------------------------------------------------------
CString Lexan::GetCurrentStringToken () const
{
    CString strCurrentToken(m_pLexanState->m_CurrentSymbol.GetLexeme());
    
    return  strCurrentToken;
}

//----------------------------------------------------------------------------
CString Lexan::GetTokenString (Token aToken) const
{
	switch (aToken)       // predefined generic tokens
	{
		case T_ID		: return _TB("ID");
		case T_STR		: return _TB("STRING");
		case T_BOOL		: return _TB(" BOOLEAN");
		case T_INT		: return _TB(" INTEGER");
		case T_WORD		: return _TB(" WORD");
		case T_LONG		: return _TB(" LONG");
		case T_DWORD	: return _TB(" DWORD");
		case T_DOUBLE	: return _TB(" DOUBLE");
		case T_COMMA	: return _TB(" SEPARATOR");
		case T_SEP		: return _TB(" SEPARATOR");
		case T_CMT		: return _TB("COMMENT");
		case T_EOF		: if (m_pLexanState->m_pLexBuf->FromFile())
							return _TB("END FILE");
						  else
							return _TB("END ROW");
	}
	return _TB("data type, or comment/separatory in incorrect syntax.");
}

//----------------------------------------------------------------------------
Token Lexan::ConvertNumericToken (double dbl)
{
		 if (dbl <= SHRT_MAX)	return T_INT;
	else if (dbl <= USHRT_MAX)	return T_WORD;
	else if (dbl <= LONG_MAX) 	return T_LONG;
	else if (dbl <= ULONG_MAX)	return T_DWORD;

	return T_DOUBLE;
}

// lexan return only positive numbers (negative number must recognized by parser)
// find RIGHT token using number range limits
//
//----------------------------------------------------------------------------
Token Lexan::GetNumericToken ()
{
	m_pLexanState->m_nLastAtof = _tstof(m_pLexanState->m_CurrentSymbol.GetLexeme());

	for (int i = 0; m_pLexanState->m_CurrentSymbol.GetLexeme()[i]; i++ )
		if (!_istdigit(m_pLexanState->m_CurrentSymbol.GetLexeme()[i]))
			return T_DOUBLE;
	
	return ConvertNumericToken (m_pLexanState->m_nLastAtof);
}

//----------------------------------------------------------------------------
Token Lexan::GetHexNumericToken ()
{
	CString strHex(m_pLexanState->m_CurrentSymbol.GetLexeme());
	DWORD	dwHex = 0;

	if (strHex.FindOneOf(_T("xX")) >= 0) 
		_stscanf_s((LPCTSTR)strHex, _T("%x"), &dwHex);
	else 
		dwHex = (DWORD) _tstol(strHex);

	m_pLexanState->m_nLastAtof = (double)dwHex;

	return ConvertNumericToken (m_pLexanState->m_nLastAtof);
}

//----------------------------------------------------------------------------
Token Lexan::LookAhead()
{
	if (m_pLexanState->m_bNoCurrentSymbol)
	{
		m_pLexanState->m_CurrentSymbol = GetNewLexeme();
		m_pLexanState->m_bNoCurrentSymbol = FALSE;
	}

	/* CMNT, EXP, STRING, SSTRING: no terminal state */
	switch (m_pLexanState->m_CurrentSymbol.GetState())
	{
		case START  : break;
		case ID     : return GetIdToken();
		case ZERONUM:
		case NUM    : return GetNumericToken();
		case HEXNUM : return GetHexNumericToken();
		case OPER   : return GetOperatorsToken();
		case ENDSTR : return T_STR;
		case ENDSSTR: return T_STR;
		case SEP    : return T_SEP;
		case CMT_END : return T_CMT;
		case BRACK  : return GetBracketsToken();
		case CMT_BEG : return GetOperatorsToken(); // the / operator can be also a comment
		case ENDF   : return T_EOF;
		default     :
			{
				CString badstate = m_pLexanState->m_CurrentSymbol.GetStateDescription();
				AfxMessageBox 
				(	
					_TB("Lexan: FSM error. Bad State! ") + badstate,
					MB_OK | MB_ICONSTOP
				);
				break;
			}
	} // switch

	return T_NOTOKEN;
}

//----------------------------------------------------------------------------
Token Lexan::SkipToken ()
{
	Token token	= LookAhead();
	m_pLexanState->m_bNoCurrentSymbol = TRUE;

	ConcatAuditString();
	return token;
}

//----------------------------------------------------------------------------
void Lexan::ConcatAuditString()
{
    CString strLexeme(m_pLexanState->m_CurrentSymbol.GetLexeme());
	m_pLexanState->m_pLexBuf->ConcatAuditString(strLexeme);
}

//----------------------------------------------------------------------------
void Lexan::ConcatAuditString(const CString& strLexeme)
{
 	m_pLexanState->m_pLexBuf->ConcatAuditString(strLexeme);
}

//----------------------------------------------------------------------------
BOOL Lexan::Match (Token aToken)
{
	Token token;

	token = LookAhead();
	m_pLexanState->m_bNoCurrentSymbol = TRUE;

	ConcatAuditString();

	if (token != aToken)
	{
		m_pLexanState->m_TokenExpected	= aToken;
		m_pLexanState->m_TokenFound	= token;
		m_pLexanState->m_nErrNo++;
		return FALSE;
	}
	
	return TRUE;
}

//----------------------------------------------------------------------------
BOOL Lexan::MatchID (CString& aString)
{
	if (!Match (T_ID)) return FALSE;

	aString = m_pLexanState->m_CurrentSymbol.GetLexeme();
	return TRUE;
}


//----------------------------------------------------------------------------
BOOL Lexan::MatchBool (BOOL& aBool)
{   
	if (Matched (T_TRUE))
	{
		aBool = TRUE;
		return TRUE;
	}
	
	if (Matched (T_FALSE))
	{
		aBool = FALSE;
		return TRUE;
	}

	// cosi` da il messaggio "Aspettato Boolean incontrato ...."
	return Match(T_BOOL);
}

//----------------------------------------------------------------------------
BOOL Lexan::MatchInt (int& aInt)
{   
	if (!Match (T_INT)) return FALSE;
	aInt = (int) m_pLexanState->m_nLastAtof;
	return TRUE;
}


//----------------------------------------------------------------------------
BOOL Lexan::MatchWord (WORD& aWord)
{                         
	Token token = LookAhead();

	// accept cast between number
    if (token != T_INT && token != T_WORD)
	{
		m_pLexanState->m_bNoCurrentSymbol = TRUE;
		m_pLexanState->m_TokenExpected	= T_WORD;
		m_pLexanState->m_TokenFound	= token;
		m_pLexanState->m_nErrNo++;
		return FALSE;
	}
    	
	SkipToken();
	aWord = (WORD) m_pLexanState->m_nLastAtof;
	return TRUE;
}


//----------------------------------------------------------------------------
BOOL Lexan::MatchLong (long& aLong)
{
	Token token = LookAhead();

	// accept cast between number
    if	(
    	token != T_INT && token != T_WORD && 
    	token != T_LONG
    	) 
	{
		m_pLexanState->m_bNoCurrentSymbol = TRUE;
		m_pLexanState->m_TokenExpected	= T_LONG;
		m_pLexanState->m_TokenFound	= token;
		m_pLexanState->m_nErrNo++;
		return FALSE;
	}

	SkipToken();
	aLong = (long) m_pLexanState->m_nLastAtof;
	return TRUE;
}

//----------------------------------------------------------------------------
BOOL Lexan::MatchDWord (DWORD& aDWord)
{
	Token token = LookAhead();

	// accept cast between number
    if	(
    	token != T_INT && token != T_WORD && 
    	token != T_LONG && token != T_DWORD
    	) 
	{
		m_pLexanState->m_bNoCurrentSymbol = TRUE;
		m_pLexanState->m_TokenExpected	= T_DWORD;
		m_pLexanState->m_TokenFound	= token;
		m_pLexanState->m_nErrNo++;
		return FALSE;
	}

	SkipToken();
	aDWord = (DWORD) m_pLexanState->m_nLastAtof;
	return TRUE;
}


//----------------------------------------------------------------------------
BOOL Lexan::MatchDouble (double& aDouble)
{
	Token token = LookAhead();

	// accept cast between number
    if	(
    	token != T_INT && token != T_WORD && 
    	token != T_LONG && token != T_DWORD && 
    	token != T_DOUBLE 
    	) 
	{
		m_pLexanState->m_bNoCurrentSymbol = TRUE;
		m_pLexanState->m_TokenExpected	= T_DOUBLE;
		m_pLexanState->m_TokenFound	= token;
		m_pLexanState->m_nErrNo++;
		return FALSE;
	}
    
    SkipToken();
	aDouble = m_pLexanState->m_nLastAtof;
	return TRUE;
}


//----------------------------------------------------------------------------
BOOL Lexan::Matched	(Token tk)
{
	if (LookAhead() != tk)
		return FALSE;

	m_pLexanState->m_bNoCurrentSymbol = TRUE;
	ConcatAuditString();
	
	return TRUE;
}

//----------------------------------------------------------------------------
BOOL Lexan::MatchString (CString& aString)
{
	if (!Match (T_STR)) return FALSE;
	aString = m_pLexanState->m_CurrentSymbol.GetLexeme();
	aString = aString.Mid(1,aString.GetLength() - 2);
	
	return TRUE;
}

//----------------------------------------------------------------------------
BOOL Lexan::MatchComment(Comment& aComment)
{
	aComment = m_Comment;
	return TRUE;
}

//----------------------------------------------------------------------------

BOOL Lexan::SkipToToken(Token t, BOOL bConsumeIt, BOOL skipInnerBlock, BOOL skipInnerRound)
{
	Token arTk[1];
	arTk[0] = t;
    
    return SkipToToken(arTk, 1, bConsumeIt, skipInnerBlock, skipInnerRound);
}

BOOL Lexan::SkipToToken2(Token t, BOOL bConsumeIt, BOOL skipInnerCouple)
{
	Token arTk[1];
	arTk[0] = t;

	return SkipToToken2(arTk, 1, bConsumeIt, skipInnerCouple);
}

//----------------------------------------------------------------------------

BOOL Lexan::SkipToToken(Token* art, int numStopTokens, BOOL bConsumeIt, BOOL skipInnerBlock, BOOL skipInnerRound)
{
	int i = 0;
	Token dummy;
    while (!LookAhead(T_EOF) && !ErrorFound())
	{
		if (skipInnerBlock && LookAhead(T_BEGIN))
		{
			if (!SkipBlock(T_BEGIN, T_END)) 
				return FALSE;
		}
		else if (skipInnerRound && LookAhead(T_ROUNDOPEN))
		{
			if (!SkipBlock(T_ROUNDOPEN, T_ROUNDCLOSE)) 
				return FALSE;
		}
		else 
		{		
			dummy = LookAhead();
			for (i = 0; i < numStopTokens; i++)
			{
				if (dummy == art[i])
				{
					if (bConsumeIt) 
						SkipToken();
					return TRUE;			
				}
			}

			SkipToken();
		}
	}
	return !ErrorFound();
}


BOOL Lexan::SkipToToken2(Token* art, int numStopTokens, BOOL bConsumeIt, BOOL skipInnerCouple)
{
	int i = 0;
	while (!LookAhead(T_EOF) && !ErrorFound())
	{
		Token tk = LookAhead();	

		if (skipInnerCouple)
		{
			switch (tk)
			{
				case T_BEGIN:
					if (!SkipBlock(T_BEGIN, T_END))
						return FALSE;
					continue;
					break;
				case T_ROUNDOPEN:
					if (!SkipBlock(T_ROUNDOPEN, T_ROUNDCLOSE))
						return FALSE;
					continue;
					break;
				case T_SQUAREOPEN:
					if (!SkipBlock(T_SQUAREOPEN, T_SQUARECLOSE))
						return FALSE;
					continue;
					break;
				case T_BRACEOPEN:
					if (!SkipBlock(T_BRACEOPEN, T_BRACECLOSE))
						return FALSE;
					continue;
					break;
			}
		}
		
		for (i = 0; i < numStopTokens; i++)
		{
			if (tk == art[i])
			{
				if (bConsumeIt)
					SkipToken();
				return TRUE;
			}
		}

		SkipToken();
	}
	return !ErrorFound();
}

//--------------------------------------------------------------------------------
BOOL Lexan::SkipBlock (Token startToken, Token endToken)
{
	if (!Match(startToken)) 
		return FALSE;
	int openBlock = 1;
	while (openBlock > 0)
	{
		if (LookAhead() == endToken) 
			openBlock--;
		else if (LookAhead() == startToken) 
			openBlock++;

		if (LookAhead() == T_EOF) 
			return FALSE;
		SkipToken();
	}
	return !ErrorFound();
}
