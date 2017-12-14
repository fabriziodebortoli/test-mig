#pragma once

//includere alla fine degli include del .H
#include "beginh.dex"

class CLineFile;

//==============================================================================
//			Class CLineFile definition
//==============================================================================
class TB_EXPORT CLineFile : public CObject
{
	DECLARE_DYNAMIC(CLineFile)
	
	// gestisce lo swapping dei byte per il formato UNICODE big endian
	union UWideChar
	{
		BYTE b[2];
		wchar_t wChar;

		void swapByte()
		{
			BYTE t = b[0];
			b[0] = b[1];
			b[1] = t;
		}

		UWideChar(){b[0]=0; b[1]=0;}
		operator wchar_t&() {return wChar;}

	};

public:
	enum FileFormat {ANSI, UTF8, UTF16_BE, UTF16_LE, UNDEFINED};	//UTF16_BE: Big Endian (swap sui byte); UTF16_LE: Little Endian 

private:
	TCHAR*		m_pchBuffer;
	UINT		m_nBufferSize;
	UINT		m_nCurrBuffPos;
	UINT		m_nCount;
	UINT		m_nLastOperation;
	DWORD		m_nCurrFilePos;	
	CFile*		m_pFile;
	BOOL		m_bUseMemFile;
	TCHAR*		m_pchMemBuffer;
	FileFormat	m_Format;
	BOOL		m_bAllBufferLoaded;
	CString		m_sFileName;

public:
// Constructors
	CLineFile(BOOL bMemFile = FALSE);
	CLineFile
			(
				LPCTSTR pszFileName, 
				UINT nOpenFlags, 
				BOOL  bOverwriteIfReadOnly = FALSE
			);
	virtual ~CLineFile();

// Operations
	virtual void	WriteString	(LPCTSTR lpsz);				// write a string, like "C" fputs
	virtual LPTSTR	ReadString	(LPTSTR lpsz, UINT nMax, BOOL bAppendCRLF = FALSE);	// like "C" fgets
	virtual BOOL	ReadString	(CString &strLine);
	
	CString	ReadToEnd	();
	BOOL	IsMemoryFile() const { return m_bUseMemFile; }

// Implementation
	virtual BOOL	Open			(LPCTSTR pszFileName, UINT nOpenFlags, CFileException* pError = NULL, FileFormat = ANSI);
	virtual UINT	Read			(void* lpBuf, UINT nCount);
	virtual void	Write			(const void* lpBuf, UINT nCount);
	virtual DWORD	GetPosition		() const;
	virtual DWORD	GetLine			() const;
	virtual LONG	Seek			(LONG lOff, UINT nFrom);
	virtual void	Abort			();
	virtual void	Flush			();
	virtual void	Close			();
	
	// Unsupported APIs
	virtual CFile*	Duplicate		() const;
	virtual void	LockRange		(DWORD, DWORD);
	virtual void	UnlockRange		(DWORD, DWORD);

public:
	void	SeekToBegin		();
	DWORD	SeekToEnd		();

public:
	FileFormat	GetFormat()						{return m_Format;}
	void		SetFormat(FileFormat format = ANSI); //da richiamare solo in fase di open

	CString	GetBufferString			();
	CString	GetFilePath				();
	DWORD	GetLength				() const;
	HANDLE	GetHandleFile			() const;

	static BOOL	GetStatus			(const CString& sFileName, CFileStatus& fs);

private:      
	TCHAR		GetChar		(CFileException& e);
	BOOL		PutChar		(TCHAR ch, CFileException& e);
	BOOL		WriteChar	(TCHAR ch, CFileException& e);
	BOOL		FlushBuffer	(CFileException& e);
	FileFormat	GetFileType(BOOL bRememberPosition = TRUE);
	void		LoadBuffer	();			


	BOOL	OpenOnVirtualDriver  (LPCTSTR pszFileName, UINT nOpenFlags, FileFormat = ANSI);
	BOOL	WriteOnVirtualDriver (LPCTSTR pszFileName);
// diagnostics
#ifdef _DEBUG
public:	
	void Dump(CDumpContext& dc) const {	ASSERT_VALID(this); AFX_DUMP0(dc, " CLineFile "); CObject::Dump(dc);}
#endif //_DEBUG
};

//=============================================================================
// Lettura di righe da un testo in memoria
//=============================================================================
class TB_EXPORT CLineText: public CObject
{
	DECLARE_DYNAMIC(CLineText);

private:
	TCHAR*	m_pszInputString;
	TCHAR*	m_pszCurrLine;
	int		m_nLineNumber;
	//mi servono per individuare un offset nella stringa originale (vedi FileMapping)
	LPCTSTR	m_pszLineStart;
	LPCTSTR	m_pszOriginalInputString;

public:	
	CLineText(LPCTSTR pszInput, long nStringLen);
	~CLineText();
	
	int GetLine(LPTSTR& pszCurrLine);
	LPCTSTR GetLineStart()	{ return m_pszLineStart; }
// diagnostics
#ifdef _DEBUG
public:	
	void Dump(CDumpContext& dc) const {	ASSERT_VALID(this); AFX_DUMP0(dc, " CLineText "); CObject::Dump(dc);}
#endif //_DEBUG
};

//=============================================================================

TB_EXPORT BOOL LoadLineTextFile (const CString& strFile, CString& strText);

#include "endh.dex"
