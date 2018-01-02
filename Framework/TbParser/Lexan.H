
#pragma once
#include <TbGeneric\linefile.h>

// tokens
#include <TbParser\TokensTable.h>

//includere alla fine degli include del .H
#include "beginh.dex"

//===========================================================================
class Lexan;
class Scheduler;
class Parser;
class LexBuffer;
class CLineText;
class CLineFile;
class Stack;

//============================================================= public define
#define LEX_END_OF_FILE		-1

//=============================================================== public enum
enum TB_EXPORT FsmState
	{
		START, OPER, ID, NUM, EXP, STRING, ENDSTR,
		SSTRING, ENDSSTR, SEP, BRACK, ZERONUM, HEXNUM,
		CMNT, CMT_LST, CMT_END, CMT_BEG, CMT_EOL,
		ENDF
	};

//=============================================================================
class TB_EXPORT Symbol
{
protected:
	FsmState	m_State;
	TCHAR*		m_Lexeme;

public:
			Symbol	();
			Symbol	(FsmState, LPCTSTR, int);
			Symbol	(const Symbol&);
  virtual	~Symbol	()				{ delete [] m_Lexeme; }

	Symbol&		operator =	(const Symbol&);
	LPCTSTR		GetLexeme	()	const	{ return m_Lexeme; }
	FsmState	GetState	()	const	{ return m_State; }
	CString		GetStateDescription	()	const;	
};

//=============================================================================
class TB_EXPORT Preprocessor
{
protected:
	Lexan*		m_pLexan;
	
public:
	Preprocessor(Lexan* pParent = NULL);
	virtual ~Preprocessor();

public:
	BOOL 	ParseBuffer(LPCTSTR pszBuffer);
	void	AttachParent(Lexan* pParent)	{ m_pLexan = pParent; }

protected:
	BOOL	ParseInclude	(LPCTSTR pszBuffer);
	BOOL	ParseDefine		(LPCTSTR pszBuffer);
	BOOL 	ParseUndef		(LPCTSTR pszBuffer);


//virtual preprocessor function in order to build particular preprocessor (ex: for MACRO). 
protected:	
	virtual BOOL	ParseMacroDefine(Parser*, LPCTSTR) { return FALSE; }
};

//=============================================================================
class TB_EXPORT LexBuffer
{
protected:
	Lexan*				m_pLexan;			// parent lex connected to this buffer
	TCHAR*				m_pszBuffer;		// the string containg data to be scanned
	int					m_nBufferLen;		// the length of the current buffer read from file or string
	int					m_nStart;			// point to lexeme first character
	int					m_nFinish;			// point to lexem end character
	long				m_nLineNumber;		// current line of the file (or string) scanned
	LPCTSTR				m_pszLineStart;		// mi serve per sapere dove inizia la riga correntemente
											// parsata della stringa
	CLineFile*			m_pInputFile;		// file to be parsed
	CLineText*			m_pBufferInputText;		// string to be parsed "xxx\nyyy\n\0"
	BOOL				m_bFromFile;		// indicate the sorce to be parsed
    BOOL				m_bError;			// same kind of error is occurred
    CFileException		m_FileException;	// signal file exception
	CString				m_strAuditString;	// audit string data member
	BOOL				m_bAuditStringOn;

    Scheduler*			m_Scheduler;		
	BOOL				m_bNoScheduler;
    BOOL				m_bExternalScheduler;

    BOOL				m_bTicEnabled;

public:
	long	m_nTotalBytes;
	long	m_nParsedBytes;

public:
			LexBuffer	(Lexan* pParentLex);
			LexBuffer	(LPCTSTR aString, Lexan* pParentLex, long nTotalByte = -1, BOOL bAllowOpenWhenEmpty = FALSE);
	virtual	~LexBuffer	();
	void	DeleteContent ();

public:
	void	Attach			(Scheduler*);
	void	StartProgress	(Scheduler*);

	BOOL	Open			(LPCTSTR pszFilename, BOOL bTicEnabled);
	BOOL	LoadBuffer		();
	void	Rewind			() { m_nStart = m_nFinish; }
	TCHAR	GetNextChar		() { m_nFinish++; return m_pszBuffer[m_nFinish]; }
	void 	IncrementTic	() { m_nParsedBytes++; }
	void	ConcatAuditString(const CString& strLexeme);

	BOOL	Eob				() const	{ return m_nFinish >= m_nBufferLen; }
	BOOL	Eof				() const	{ return m_nBufferLen == LEX_END_OF_FILE; }
	BOOL	FromFile		() const	{ return m_bFromFile; }

	void	SetHardwareError()			{ m_bError = TRUE; m_nBufferLen = LEX_END_OF_FILE; }

	// audit trailing strings function
	CString	GetAuditString		(BOOL bReset = TRUE);
	void	EnableAuditString	(BOOL bOn = TRUE)	{ m_bAuditStringOn = bOn;	}
	BOOL	IsAuditStringOn	()	const	{ return m_bAuditStringOn; }
	
	int		GetLexemeSize	()	const	{ return (m_nFinish - m_nStart); }
	LPCTSTR	GetLexemeStart	()	const	{ return (&(m_pszBuffer[m_nStart])); }
	LPCTSTR	GetBuffer		()	const	{ return m_pszBuffer; }
	LPCTSTR GetLineStart	()	const	{ return m_pszLineStart; }

	long	GetCurrentLine	()	const	{ return m_nLineNumber; }
	int		GetCurrentPos	()	const	{ return m_nStart; }
	long	GetScannedLines	()	const	{ return m_nLineNumber - 1; }

	CString GetDoubleSlashComment		()  const;
	BOOL	GetSlashStarComment			(CString&);

	LPCTSTR GetException	();

	BOOL	HardwareError	()	const 	{ return m_bError; }

	CLineFile::FileFormat GetFileFormat() const { return m_pInputFile ? m_pInputFile->GetFormat() : CLineFile::UNDEFINED ;}

protected:
	void	InitLexBuffer	();
	void	AllocLexBuffer	();
	void	SchedulerStart	();
	void	SchedulerStop	();
	void	SchedulerTic	();

	
};

//=============================================================================
class TB_EXPORT LexanState : public CObject
{	
	friend Lexan;

public:	
	FsmState		m_NewState;		// Finite State Machine current state
	LexBuffer*		m_pLexBuf;
	CString			m_strFileName;
	Symbol			m_CurrentSymbol;
	BOOL			m_bNoCurrentSymbol;
	int				m_nErrNo;
	Token			m_TokenExpected;
	Token			m_TokenFound;
	double			m_nLastAtof;		// avoid double atof in number conversion
	BOOL			m_bExpandDefine;
	CString			m_strWorkingFolder; //used to find the include file to parse
										//this field must be set by user if parsing string or mapped file
public:
	LexanState();
	LexanState(const LexanState& OldState);
	~LexanState();

private:	
	void InitLexState();
};

//=============================================================================
class TB_EXPORT Comment : public CStringArray
{	
	friend class Lexan;

public:
	enum LineTerminator {CrLf, Cr, Lf, Zero, None};

private:
	int m_nTabSize;		

public:
	Comment();
	Comment(const Comment&);
	
public:
	CString All		(BOOL, LineTerminator) const;
	void	Reload	(const CString&, LineTerminator);

	int GetTabSize() const { return m_nTabSize; }

	void AddDoubleSlashComment(const CString& strComment);
	int Add(CString strComment);

	Comment& operator =	(const Comment&);
};

//=============================================================================
class TB_EXPORT Lexan
{     
	friend Preprocessor;
	
private:
	LexanState*			m_pLexanState;
	const TToken*		m_pUserTokenTable;		// user define symbol table
	Stack*				m_pStateStack;
	BOOL				m_bExternalPreprocessor; // when user defines preprocessor
	const TokensTable*  m_pTokensTable;

protected:
	// preprocessor related data member
	CMapStringToPtr*	m_pDefines;
    Preprocessor*		m_pPreprocessor;

	Comment				m_Comment;
	CStringArray		m_CommentTrace;
	BOOL				m_bAuditComment = FALSE;

protected:
	Token 	GetBracketsToken	();
	Token 	GetOperatorsToken	();
	Token 	GetNumericToken		();
	Token 	GetHexNumericToken	();
	Token 	GetIdToken			();
	Token	ConvertNumericToken	(double);
	void	ProcessCommentEOL	();
	void	ProcessCommentBeginEnd();

	Symbol	GetNewLexeme 		();
	void	ConcatAuditString	();

	//used for include file management
	void	InitLexState();	
	void	ResetLexState();	
	void	SaveLexState();

public:
	Lexan	(const TToken* userTokenTable = NULL);
	Lexan	(LPCTSTR pszString, const TToken* userTokenTable = NULL, long	nStringLen = -1, BOOL bAllowOpenWhenEmpty = FALSE);
	virtual ~Lexan();

	void Abort();

	void SetTokensTable (const TokensTable* pTokensTable) { m_pTokensTable = pTokensTable; }
	const TokensTable* GetTokensTable () const { return m_pTokensTable; }

	// inizial setting doing after costructor call
	void    SetWorkingFolder	(LPCTSTR szWorkingFolder) { m_pLexanState->m_strWorkingFolder = szWorkingFolder; }
	void	SetNewPreprocessor	(Preprocessor*	pNewPrep); 
	void	StartProgress		(Scheduler* aScheduler)	{ m_pLexanState->m_pLexBuf->StartProgress(aScheduler); }

	const TToken* 	GetSymbolTable	()	const { return m_pUserTokenTable; }

	CString	GetCurrentStringToken	() 		const;
	CString	GetTokenString			(Token) const;
	BOOL	Open					(LPCTSTR pszFilename, BOOL bTicEnabled = TRUE);
	void	Attach					(Scheduler* aScheduler)	{ m_pLexanState->m_pLexBuf->Attach(aScheduler); }

	BOOL	Eof				(); 

	BOOL	ClearErrorNumber ()	const	{ return m_pLexanState->m_nErrNo = 0;	}

	BOOL	Bad				()	const	{ return m_pLexanState->m_nErrNo || m_pLexanState->m_pLexBuf->HardwareError();	}
	BOOL	HardwareError	()	const	{ return m_pLexanState->m_pLexBuf->HardwareError();		}
	void	SetHardwareError()			{ m_pLexanState->m_pLexBuf->SetHardwareError(); }

	long	GetScannedLines	()	const	{ return m_pLexanState->m_pLexBuf->GetScannedLines();	}
	int		GetTotalErrors	()	const	{ return m_pLexanState->m_nErrNo;						}

	// error handler support routines
	LPCTSTR GetException		()			{ return m_pLexanState->m_pLexBuf->GetException(); }
	Token	GetTokenExpected	()	const	{ return m_pLexanState->m_TokenExpected; }
	Token	GetTokenFound		()	const	{ return m_pLexanState->m_TokenFound; }

	// audit trailing strings function
	void	EnableAuditString	(BOOL bOn = TRUE, BOOL bAuditComment = FALSE)		
		{ m_pLexanState->m_pLexBuf->EnableAuditString(bOn); m_bAuditComment = bAuditComment; }
	BOOL	IsAuditStringOn		() { return m_pLexanState->m_pLexBuf->IsAuditStringOn(); }
	CString	GetAuditString		(BOOL bReset = TRUE);
	void	ConcatAuditString	(const CString& strLexeme);

	// define expansion support
	BOOL	GetExpandDefine () const			 	{ return m_pLexanState->m_bExpandDefine; }
	void	SetExpandDefine (BOOL bExpand = TRUE)	{ m_pLexanState->m_bExpandDefine = bExpand; }

	Token	LookAhead	();
	BOOL	LookAhead	(Token aToken)	{ return LookAhead() == aToken; }
	Token	SkipToken	();

	BOOL	SkipToToken	(Token t, BOOL bConsumeIt = FALSE, BOOL skipInnerBlock = FALSE, BOOL skipInnerRound = FALSE);
	BOOL	SkipToToken	(Token* t, int numStopTokens, BOOL bConsumeIt = FALSE, BOOL skipInnerBlock = FALSE, BOOL skipInnerRound = FALSE);
	BOOL	SkipBlock(Token startToken, Token endToken);
	//inner couple are: begin-end, () round begin-end, [] square begin-end, {} brace begin-end
	BOOL	SkipToToken2(Token t, BOOL bConsumeIt = FALSE, BOOL skipInnerCouple = FALSE);
	BOOL	SkipToToken2(Token* t, int numStopTokens, BOOL bConsumeIt = FALSE, BOOL skipInnerCouple = FALSE);

	BOOL	Match		(Token);
	BOOL	Matched		(Token);

	CString					GetFileName	() const	{ return m_pLexanState->m_strFileName; }
	CLineFile::FileFormat	GetFileFormat() const	{ return m_pLexanState->m_pLexBuf->GetFileFormat(); }
	Preprocessor*			GetPreprocessor() const	{ return m_pPreprocessor; }

	Comment			GetComment		()	const	{ return m_Comment; }
	void			GetCommentTrace	(CStringArray& comments, BOOL bErase = TRUE);
	void			RemoveCommentTrace	();

public:
	// preprocessor public related function member
	BOOL	DefinePresent		(CString) const;
	BOOL	AddDefine			(CString, DWORD dwValue);
	BOOL	RemoveDefine		(CString);
	BOOL	GetDefine			(CString, DWORD& dwValue) const;
	BOOL	PreprocessLine	() { return m_pPreprocessor->ParseBuffer(m_pLexanState->m_pLexBuf->GetBuffer()); }

public:
	// virtual diagnostic function
	virtual long	GetCurrentLine	()	const	{ return m_pLexanState->m_pLexBuf->GetCurrentLine();	}
	virtual int		GetCurrentPos	()	const	{ return m_pLexanState->m_pLexBuf->GetCurrentPos();	}
	virtual int		GetLexemeSize	()	const	{ return m_pLexanState->m_pLexBuf->GetLexemeSize();	}
	virtual LPCTSTR 	GetLineStart	()	const	{ return m_pLexanState->m_pLexBuf->GetLineStart();		}

	virtual BOOL	ErrorFound		()	const	{ return HardwareError() || Bad();	}
	virtual CString	GetError		()	const = 0;
	virtual BOOL	SetError		(const CString& strError, LPCTSTR = _T(""), int = -1, long = -1) = 0;
	virtual void	SetErrFileName	(const CString&) = 0;
	virtual void	ShowErrors		(BOOL bClear = TRUE) = 0;
	
protected:
	
	// must use similar routines present in Parser
	//
	BOOL	MatchID		(CString&);
	BOOL	MatchString	(CString&);
	BOOL	MatchBool	(BOOL&);
	BOOL	MatchInt	(int&);
	BOOL	MatchWord	(WORD&);
	BOOL	MatchLong	(long&);
	BOOL	MatchDWord	(DWORD&);
	BOOL	MatchDouble	(double&);
	BOOL	MatchComment(Comment&);
};

#include "endh.dex"
