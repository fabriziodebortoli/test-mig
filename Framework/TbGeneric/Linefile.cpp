
#include "stdafx.h"         

#include <atlenc.h>

#include <TbNameSolver\FileSystemFunctions.h>
#include <TbNameSolver\IFileSystemManager.h>
#include <TbNameSolver\PathFinder.h>

#include <TbNameSolver\Chars.h>
#include <TbGeneric\crypt.h>

#include "CollateCultureFunctions.h"

#include <TbGeneric\GeneralFunctions.h>
#include <TbGeneric\Globals.h>

#include "linefile.h"
//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char THIS_FILE[] = __FILE__;
#endif
                    
// buffer size for read/write operations
#define BUFFER_SIZE		((UINT)16384)

// line size for reading a single line
#define LINE_SIZE		((UINT)1024)

enum { NO_OPE, READ_OPE, WRITE_OPE };

////////////////////////////////////////////////////////////////////////////
// CLineFile implementation
//
//----------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(CLineFile, CObject)

//----------------------------------------------------------------------------
CLineFile::CLineFile(BOOL bUseMemFile /*=FALSE*/)
	:
	m_pFile					(NULL),
	m_pchBuffer 			(NULL),
	m_nCurrBuffPos			(0),
	m_nCurrFilePos			(0),
	m_nCount				(0),
	m_nLastOperation		(NO_OPE),
	m_bUseMemFile			(bUseMemFile),
	m_pchMemBuffer			(NULL),
	m_Format				(ANSI),
	m_nBufferSize			(BUFFER_SIZE),
	m_bAllBufferLoaded		(FALSE)
{
	if (m_bUseMemFile)
	{
		m_pchBuffer 	= new TCHAR[m_nBufferSize + 1];	//un carattere per il terminatore
		m_pchMemBuffer 	= new TCHAR[m_nBufferSize];
		m_pFile			= new CMemFile(); //(BYTE *)m_pchMemBuffer, m_nBufferSize, m_nBufferSize);
		m_Format		= UTF16_LE;
		SetFormat (m_Format);
	}
	else
		m_pFile = new CFile;

}

//----------------------------------------------------------------------------
CLineFile::CLineFile(LPCTSTR pszFileName, UINT nOpenFlags, BOOL bOverwriteIfReadOnly /*FALSE*/)
	:
	m_pchBuffer			(NULL),
	m_bUseMemFile		(FALSE),
	m_Format			(ANSI),
	m_nBufferSize		(BUFFER_SIZE),
	m_bAllBufferLoaded	(FALSE)
{
	ASSERT_TRACE(AfxIsValidString(pszFileName),"Parameter pszFileName is not a valid string");

	m_pFile = new CFile;

	CFileException e;

	// verifico se è già in stato corretto
	DWORD wAttributes = GetTbFileAttributes(pszFileName);
	if (bOverwriteIfReadOnly && wAttributes & FILE_ATTRIBUTE_READONLY)
		SetFileAttributes(pszFileName, wAttributes & ~FILE_ATTRIBUTE_READONLY);

	if (!Open(pszFileName, nOpenFlags, &e))
		AfxThrowFileException(e.m_cause, e.m_lOsError, pszFileName);
}

//----------------------------------------------------------------------------
CLineFile::~CLineFile()
{
	Close();

	ASSERT_VALID(this);

	if (m_pFile && m_pFile->m_hFile != CFile::hFileNull)
	{
		if (m_bUseMemFile) 
			Close();
	}
	if (m_pchBuffer)
	{
		delete [] m_pchBuffer;
		m_pchBuffer = NULL;
	}
	SAFE_DELETE(m_pFile);
}

//----------------------------------------------------------------------------
BOOL CLineFile::Open( LPCTSTR pszFileName, UINT nOpenFlags, CFileException* pError, CLineFile::FileFormat format /*=ANSI*/)
{          
	ASSERT_VALID(this);

	if (!m_pFile || (m_pFile && m_bUseMemFile))
	{
		ASSERT_TRACE(m_pFile && !(m_pFile && m_bUseMemFile),"Either m_pFile must be valid or use of mem file must be set");
		return FALSE;
	}

	ASSERT_TRACE(m_pchBuffer == NULL,"Buffer is already allocated");
	
	//maschero access type text gestito da questa classe, ma non dalla superclasse CFile
	ASSERT_TRACE((nOpenFlags & CFile::typeText) != 0,"Text access type is not managed by CFile parent class"); 
	nOpenFlags &= ~(UINT)CFile::typeText;

	CString sFileName (pszFileName);
	sFileName.Trim();

	IFileSystemManager* pFileSystemManager = AfxGetFileSystemManager();
	if (pFileSystemManager && pFileSystemManager->IsManagedByAlternativeDriver((LPCTSTR) sFileName))
	{
		if (OpenOnVirtualDriver ((LPCTSTR) sFileName, nOpenFlags, format))
			return TRUE;
	}

	if (!m_pFile->Open((LPCTSTR) sFileName, nOpenFlags, pError))
		return FALSE;
	
	m_sFileName = sFileName;

	if(format != UNDEFINED && (nOpenFlags & CFile::modeCreate) != 0)
	{
		SetFormat (format);
		m_nBufferSize = BUFFER_SIZE;		
	}
	else
	{
		// controllo il tipo del file
		m_Format = GetFileType(FALSE); //con FALSE "mangia" i byte-order mark
		if (m_Format == UTF8)
			m_nBufferSize = max(BUFFER_SIZE, (UINT)m_pFile->GetLength() -3);	//se il formato è UTF8, devo caricare tutto il file in memoria
		else																	//(perche i caratteri hanno lunghezza variabile e non so
			m_nBufferSize = BUFFER_SIZE;										 //a priori dove termina un carattere; tolgo i tre byte-order mark)
	}

	m_pchBuffer 	= new TCHAR[m_nBufferSize + 1];	//un carattere per il terminatore
	m_nCurrBuffPos	= 0;
	m_nCurrFilePos	= 0;
	m_nCount		= 0;
	m_nLastOperation= NO_OPE;

	return TRUE;
}

//----------------------------------------------------------------------------
CLineFile::FileFormat CLineFile::GetFileType(BOOL bRememberPosition /*=TRUE*/)
{          
	ASSERT_VALID(this);
	if(m_pFile == NULL)
	{
		ASSERT_TRACE(m_pFile != NULL,"m_pFile cannot be null in this context");
		return UNDEFINED;
	}

	FileFormat nFormat = UNDEFINED;
	
	BYTE b1=0, b2=0;
	
	ULONGLONG nPos = m_pFile->GetPosition ();

	m_pFile->Seek(0, CFile::begin);
	m_pFile->Read (&b1, 1);
	m_pFile->Read (&b2, 1);
	
	if(b1==0xFF && b2==0xFE )		//UTF16  little endian
	{
		nFormat = UTF16_LE;
	}
	else if (b1==0xFE && b2==0xFF)	//UTF16 big endian
	{
		nFormat = UTF16_BE;
	}
	else if (b1==0xEF && b2==0xBB)	//UTF8?
	{
		m_pFile->Read (&b1, 1);
		if(b1==0xBF)				//UTF8!
		{
			nFormat = UTF8;
		}
		else
		{
			m_pFile->Seek (0, CFile::begin); //ho consumato dei byte di dati: li ripristino
			nFormat = ANSI;
		}
	}
	else
	{
		m_pFile->Seek (0, CFile::begin); //ho consumato dei byte di dati: li ripristino
		nFormat = ANSI;
	}

	if(bRememberPosition)
		m_pFile->Seek (nPos, CFile::begin);

	return nFormat;

}

//----------------------------------------------------------------------------
void CLineFile::Close()
{
	if (!m_pFile)
	{
		ASSERT_TRACE(m_pFile != NULL,"m_pFile cannot be null in this context");
		return;
	}

	ASSERT_VALID(this);

	if (m_pchBuffer != NULL)
	{
		Flush();
	
		delete [] m_pchBuffer;		m_pchBuffer = NULL;
		
		if (m_bUseMemFile && m_pchMemBuffer != NULL)
		{
			delete [] m_pchMemBuffer;
			m_pchMemBuffer = NULL;
		}			
	}
	if (m_pFile->m_hFile && m_pFile->m_hFile != CFile::hFileNull && m_pFile->m_hFile != INVALID_HANDLE_VALUE)
		m_pFile->Close();

	m_sFileName.Empty ();
}

//----------------------------------------------------------------------------
void CLineFile::LoadBuffer()
{
	// Storage on database management
	if (m_bAllBufferLoaded)
		return;

	USES_CONVERSION;

	switch(m_Format)
	{
		case ANSI:
			{
				char* pBuff = new char [m_nBufferSize + 1];

				m_nCount = m_pFile->Read(pBuff, m_nBufferSize);
				pBuff[m_nCount] = '\0';
				
				TB_TCSNCPY(m_pchBuffer, A2T(pBuff), m_nCount);
				m_pchBuffer[m_nCount] = _T('\0');

				delete [] pBuff;
				break;
			}
			case UTF16_BE:
			{
				UWideChar *pBuff= new UWideChar [m_nBufferSize + 1];
				m_nCount = m_pFile->Read (pBuff, m_nBufferSize*sizeof(UWideChar))/sizeof(UWideChar);
				pBuff[m_nCount].wChar = L'\0';

				for(UINT i=0; i<m_nCount; i++) 
					pBuff[i].swapByte();
				
				TB_TCSNCPY(m_pchBuffer, W2T((LPWSTR)pBuff), m_nCount);
				m_pchBuffer[m_nCount] = _T('\0');
				
				delete [] pBuff;
				break;				
			}
		case UTF16_LE:
			{
				wchar_t* pBuff= new wchar_t [m_nBufferSize + 1];
				m_nCount = m_pFile->Read (pBuff, m_nBufferSize*sizeof(wchar_t))/sizeof(wchar_t);
				pBuff[m_nCount] = L'\0';
				
				TB_TCSNCPY(m_pchBuffer, W2T(pBuff), m_nCount);
				m_pchBuffer[m_nCount] = _T('\0');
				
				delete [] pBuff;
				break;
			}
		case UTF8:
			{
				char *pBuff = new char [m_nBufferSize + 1]; 
				
				int n = m_pFile->Read (pBuff, m_nBufferSize);				
				wchar_t *pwBuff = new wchar_t[n + 1];
				
				m_nCount = MultiByteToWideChar(CP_UTF8, 0, pBuff, n, pwBuff, n);
				pwBuff[m_nCount] = L'\0';

				TB_TCSNCPY(m_pchBuffer, W2T(pwBuff), m_nCount);
				m_pchBuffer[m_nCount] = _T('\0');
				
				delete [] pBuff;
				delete [] pwBuff;
				break;	
			}
	}
}
 
//----------------------------------------------------------------------------
TCHAR CLineFile::GetChar	(CFileException& e)
{
	if (!m_pFile  || m_pchBuffer == NULL)
		return _T_EOF;

	TCHAR ch;
	
	do
	{
		if ( m_nCount == 0 )
		{
			m_nCurrBuffPos = 0;
			
			TRY
			{
				LoadBuffer();
			}
			CATCH( CFileException, pe )
			{
				e.m_cause = pe->m_cause;
				return _T_EOF;
			}
			END_CATCH
	
			if (m_nCount == 0)
			{
				e.m_cause = CFileException::endOfFile;
				return _T_EOF;                         
			}
		
		}
		
		m_nCount--;
		
		ch = m_pchBuffer[m_nCurrBuffPos++];

		m_nCurrFilePos++;
	}
	while (ch == CR_CHAR && m_pchBuffer[m_nCurrBuffPos] == LF_CHAR);

	return ch;
}   

//----------------------------------------------------------------------------
UINT CLineFile::Read(void* lpBuf, UINT nCount)
{
	if (!m_pFile || m_pchBuffer == NULL)
		return 0;

	ASSERT(AfxIsValidAddress(lpBuf, nCount));
	ASSERT_TRACE1(AfxIsValidAddress(lpBuf, nCount),"lpBuf does not contain nCount = %d valid positions",nCount);

	TCHAR chOrEOF; 			// because it could be an EOF which isn't a char

	UINT nRead	= 0;
	TCHAR* lpBufT = (TCHAR*) lpBuf;
	
	CFileException e;
	e.m_cause = CFileException::none;
    
    if (m_nLastOperation == WRITE_OPE)
		AfxThrowFileException(CFileException::invalidFile);
    
	m_nLastOperation= READ_OPE;

	while ((UINT)nRead < nCount)
	{
		if ((chOrEOF = GetChar(e)) == _T_EOF)
		{
			if ( e.m_cause == CFileException::endOfFile)
				break;          
				
			// real error
			AfxThrowFileException(e.m_cause, _doserrno);
		}

		nRead++;
		*lpBufT++ = chOrEOF;
	}

	return nRead;
}
//----------------------------------------------------------------------------
CString CLineFile::ReadToEnd ()
{
	CString sLine, sContent;
	while (ReadString(sLine))
	{
		if (!sContent.IsEmpty())
			sContent.AppendChar(_T('\n'));
		sContent.Append(sLine);
	}
	return sContent;
}
//----------------------------------------------------------------------------
BOOL CLineFile::ReadString (CString &strLine)
{
	strLine.Empty();
	TCHAR szLine [LINE_SIZE];
	while (ReadString(szLine, LINE_SIZE) != NULL)
	{
		int nLen = _tcslen(szLine);
		if (szLine[nLen-1] == LF_CHAR || szLine[nLen-1] == CR_CHAR)
			szLine[nLen-1] = '\0';

		if (szLine[nLen - 2] == LF_CHAR || szLine[nLen - 2] == CR_CHAR)
			szLine[nLen - 2] = '\0';

		strLine.Append(szLine);
		
		// se non ho riempito il buffer oppure l'ultimo carattere è un 'a capo' posso uscire
		if ( (nLen != LINE_SIZE - 1) )
			return TRUE;
	}

	return FALSE;
}   

//----------------------------------------------------------------------------
TCHAR* CLineFile::ReadString	(TCHAR* lpsz, UINT nMax, BOOL bAppendCRLF /* = FALSE */)
{
	if (!m_pFile || m_pchBuffer == NULL)
		return NULL;

	ASSERT(AfxIsValidAddress(lpsz, nMax));

	TCHAR chOrEOF; // because it could be an EOF which isn't a char

	UINT nRead	= 0;
	LPTSTR lpszT = lpsz;
	
	CFileException e;
	e.m_cause = CFileException::none;

    if (m_nLastOperation == WRITE_OPE)
		AfxThrowFileException(CFileException::invalidFile);

	m_nLastOperation= READ_OPE;

	BOOL bEOL = FALSE;

	while ((UINT)nRead < nMax - (bAppendCRLF ? 3 : 1))
	{
		if ((chOrEOF = GetChar(e)) == _T_EOF)
		{
			if (e.m_cause == CFileException::endOfFile)
			{
				bEOL = TRUE;
				break;
			}
				
			// real error
			AfxThrowFileException(e.m_cause, _doserrno);
		}

		nRead++;
		if ((*lpszT++ = chOrEOF) == LF_CHAR)
			break;
	}

	if (bAppendCRLF && nRead)
	{
		if (lpszT[-1] == LF_CHAR)
		{
			lpszT--;
			bEOL = TRUE;
		}

		if (bEOL)
		{
			*lpszT++ = CR_CHAR;
			*lpszT++ = LF_CHAR;
		}
	}

	*lpszT = NULL_CHAR;                      
	return (lpsz == lpszT ? NULL : lpsz);
}                         

//----------------------------------------------------------------------------
BOOL CLineFile::PutChar(TCHAR ch, CFileException& e)
{                  
	if (!m_pFile || m_pchBuffer == NULL)
		return FALSE;

	m_pchBuffer[m_nCurrBuffPos++] = ch;

	m_nCurrFilePos++;
	
	if ( m_nCurrBuffPos == m_nBufferSize && !FlushBuffer(e))
		return FALSE;
		       
	return TRUE;
}   

//----------------------------------------------------------------------------
BOOL CLineFile::WriteChar(TCHAR ch, CFileException& e)
{
	if (ch == CR_CHAR)
		return TRUE;

	if (ch == LF_CHAR && !PutChar(CR_CHAR, e))
		return FALSE;
				       
	return PutChar(ch, e);
}   

//----------------------------------------------------------------------------
BOOL CLineFile::FlushBuffer(CFileException& e)
{
	if (!m_pFile || m_pchBuffer == NULL)
		return FALSE;
	
	IFileSystemManager* pFileSystemManager = AfxGetFileSystemManager();
	if (pFileSystemManager && pFileSystemManager->IsManagedByAlternativeDriver(m_sFileName))
		return WriteOnVirtualDriver(m_sFileName);

	if (m_nCurrBuffPos)
	{
		USES_CONVERSION;

		TRY
		{
			m_pchBuffer[m_nCurrBuffPos] = _T('\0');
			switch(m_Format)
			{
				case ANSI:
					{
						m_pFile->Write(T2A(m_pchBuffer), m_nCurrBuffPos);
						break;
					}
				case UTF16_BE:
					{						
						UWideChar *pBuff= new UWideChar [m_nCurrBuffPos];
						TB_WCSNCPY((LPWSTR)pBuff, T2W(m_pchBuffer), m_nCurrBuffPos);
						for(UINT i=0; i<m_nCurrBuffPos; i++) 
							pBuff[i].swapByte();
						m_pFile->Write(pBuff, m_nCurrBuffPos*sizeof(wchar_t));
						delete [] pBuff;
						break;
					}
				case UTF16_LE:
					{	
						m_pFile->Write(T2W(m_pchBuffer), m_nCurrBuffPos*sizeof(wchar_t));
						break;
					}
				case UTF8:
					{
						CStringA aBuff = UnicodeToUTF8(m_pchBuffer);
						m_pFile->Write(aBuff, aBuff.GetLength());							
						break;	
					}
			}				
		}
		CATCH( CFileException, pe )
		{
			e.m_cause = pe->m_cause;
			return FALSE;
		}
		END_CATCH
		
		m_nCurrBuffPos = 0;
	}
		
	return TRUE;
}

//----------------------------------------------------------------------------
void CLineFile::Write(const void* lpBuf, UINT nCount)
{
	if (!m_pFile || m_pchBuffer == NULL)
		return;

	ASSERT_TRACE1(AfxIsValidAddress(lpBuf, nCount),"lpBuf does not contain nMax = %d valid positions",nCount);

	TCHAR* lpBufT = (TCHAR*) lpBuf;

	CFileException e;
	e.m_cause = CFileException::none;

    if (m_nLastOperation == READ_OPE)
		AfxThrowFileException(CFileException::invalidFile);
    
	m_nLastOperation = WRITE_OPE;

	while (nCount--)
		if (!WriteChar(*lpBufT++, e))
			AfxThrowFileException(e.m_cause, _doserrno);
}


//----------------------------------------------------------------------------
void CLineFile::WriteString(LPCTSTR lpsz)
{
	if (!m_pFile || m_pchBuffer == NULL)
		return;

	ASSERT_TRACE(lpsz != NULL,"parameter lpsz must be not null");

	CFileException e;
	e.m_cause = CFileException::none;

    if (m_nLastOperation == READ_OPE)
		AfxThrowFileException(CFileException::invalidFile);
    
	m_nLastOperation= WRITE_OPE;

	TCHAR ch;

	while ((ch = *lpsz++) != NULL_CHAR)
		if (!WriteChar(ch, e))
			AfxThrowFileException(e.m_cause, _doserrno);
}

//----------------------------------------------------------------------------
DWORD CLineFile::GetPosition() const
{
	if (!m_pFile || m_pchBuffer == NULL)
		return 0;

	return m_nCurrFilePos;
}

//----------------------------------------------------------------------------
DWORD CLineFile::GetLine() const
{
	ASSERT(m_pchBuffer);
	if (m_pchBuffer == NULL)
		return 0;
	
	return _tcsccnt(m_pchBuffer, L'\n');
}

//----------------------------------------------------------------------------
LONG CLineFile::Seek(LONG lOff, UINT nFrom)
{
	if (!m_pFile || m_pchBuffer == NULL)
		return 0;

    Flush();
	int nScale, nOffset;
	switch(m_Format)
	{
		case ANSI:
			{
				nScale = 1; nOffset=0;
				break;
			}
		case UTF16_BE:
			{
				nScale = 2; nOffset=2;
				break;				
			}
		case UTF16_LE:
			{
				nScale = 2; nOffset=2;
				break;
			}
		case UTF8: 
			{
				ASSERT_TRACE(FALSE,"Seek in UTF8 format is not guarantee to work as character size is not fixed"); //WARNING: non è detto che funzioni perché la dimensione di un carattere è variabile
				nScale = 1; nOffset=2;
				break;	
			}
	}

	m_nCurrFilePos = (DWORD)m_pFile->Seek(lOff*nScale, nFrom*nScale + nOffset) /nScale;

	return m_nCurrFilePos;
}

//----------------------------------------------------------------------------
void CLineFile::Flush()
{
	if (!m_pFile || m_pchBuffer == NULL)
		return;

	if(m_nLastOperation == WRITE_OPE)
	{   
		CFileException e;
		e.m_cause = CFileException::none;

		if (!FlushBuffer(e))
			AfxThrowFileException(e.m_cause, _doserrno);
		else
		{
			if (m_pFile->m_hFile != CFile::hFileNull)
				m_pFile->Flush();
		}
	}
}

//----------------------------------------------------------------------------
void CLineFile::SetFormat(FileFormat format /*= ANSI*/) 
{
	m_Format = format;

	IFileSystemManager* pFileSystemManager = AfxGetFileSystemManager ();
	if (pFileSystemManager && pFileSystemManager->IsManagedByAlternativeDriver (m_sFileName))
		return;
	
	ASSERT_TRACE(m_pFile,"Datamember m_pFile must be not null in this context");

	BYTE b = 0;
	m_pFile->Seek(0, CFile::begin);
	switch(format)
	{
		case UTF16_BE:
			{
				b=0xFE;
				m_pFile->Write (&b, 1);
				b=0xFF;
				m_pFile->Write (&b, 1);
				break;
			}
		case UTF16_LE:
			{
				b=0xFF;
				m_pFile->Write (&b, 1);
				b=0xFE;
				m_pFile->Write (&b, 1);
				break;
			}
		case UTF8:
			{
				b=0xEF;
				m_pFile->Write (&b, 1);
				b=0xBB;
				m_pFile->Write (&b, 1);
				b=0xBF;
				m_pFile->Write (&b, 1);
				break;
			}

	}
}

//----------------------------------------------------------------------------
void CLineFile::Abort()
{
	Close();
}

//----------------------------------------------------------------------------
CFile* CLineFile::Duplicate() const
{
	AfxThrowNotSupportedException();
	return NULL;
}

//----------------------------------------------------------------------------
void CLineFile::LockRange(DWORD, DWORD)
{
	AfxThrowNotSupportedException();
}


//----------------------------------------------------------------------------
void CLineFile::UnlockRange(DWORD, DWORD)
{
	AfxThrowNotSupportedException();
}

//----------------------------------------------------------------------------
void CLineFile::SeekToBegin()
{
	IFileSystemManager* pFileSystemManager = AfxGetFileSystemManager ();
	if (pFileSystemManager && pFileSystemManager->IsManagedByAlternativeDriver (m_sFileName))
		return;

	if (!m_pFile)
		return;

	Seek(0, CFile::begin);
}

//----------------------------------------------------------------------------
DWORD CLineFile::SeekToEnd()
{
	IFileSystemManager* pFileSystemManager = AfxGetFileSystemManager ();
	if (pFileSystemManager && pFileSystemManager->IsManagedByAlternativeDriver (m_sFileName))
		return 0;

	if (!m_pFile)
		return 0;

	return Seek(0, CFile::end);
}

//----------------------------------------------------------------------------
CString CLineFile::GetBufferString()
{
	if (!m_bUseMemFile)
	{
		ASSERT_TRACE(m_bUseMemFile,"This function is supported for mem file only");
		return NULL;
	}

	ASSERT_TRACE(m_pFile->IsKindOf(RUNTIME_CLASS(CMemFile)),"Datamember m_pFile is not of type CMemFile");
	
	Flush();

	DWORD dwLenght = (DWORD)m_pFile->GetLength();
	if (dwLenght == 0)
		return L"";

	BYTE* lpBuffToDelete;
	BYTE* lpBuff ;
	lpBuffToDelete = lpBuff = ((CMemFile*)m_pFile)->Detach();
	CString str;

	switch(m_Format)
	{
		case ANSI:
			{
				lpBuff[dwLenght-1] = '\0';
				str = CString (lpBuff);
				break;
			}
		case UTF16_BE:
			{
				if (dwLenght == 2)
					return L"";

				lpBuff += 2;	//skippo i byte order mark
				
				dwLenght = (dwLenght-2) / sizeof(wchar_t);
				
				UWideChar *pUnicodeBuff = (UWideChar*) lpBuff;
				for(UINT i=0; i<dwLenght; i++) 
					pUnicodeBuff[i].swapByte();
				
				pUnicodeBuff[dwLenght-1].wChar = L'\0';
				
				str = CString ((wchar_t*)pUnicodeBuff);
				break;
			}
		case UTF16_LE:
			{
				if (dwLenght == 2)
					return L"";

				lpBuff += 2;	//skippo i byte order mark
				
				dwLenght = (dwLenght-2) / sizeof(wchar_t);
				
				wchar_t* pUnicodeBuff= (wchar_t*) lpBuff;
				
				pUnicodeBuff[dwLenght-1] = L'\0';	

				str = CString (pUnicodeBuff);
				break;
			}
		case UTF8:
			{
				if (dwLenght == 3)
					return L"";

				lpBuff += 3;	//skippo i byte order mark
				
				dwLenght = dwLenght-3;

				char * pANSIBuff = (char*) lpBuff;
				wchar_t * pUnicodeBuff = new wchar_t[dwLenght+1];
				
				dwLenght = MultiByteToWideChar(CP_UTF8, 0, pANSIBuff, dwLenght, pUnicodeBuff, dwLenght);
				
				pUnicodeBuff[dwLenght] = _T('\0');	

				str = CString (pUnicodeBuff);
				if (pUnicodeBuff) 
					delete [] pUnicodeBuff;
				break;
			}
	}
	if (lpBuffToDelete)
		delete [] lpBuffToDelete;
	return str;
}

//----------------------------------------------------------------------------
CString CLineFile::GetFilePath()
{
	if (!m_sFileName.IsEmpty())
		return m_sFileName;

	if (!m_pFile) 
		return _T("");

	return m_pFile->GetFilePath();
}

//----------------------------------------------------------------------------
DWORD CLineFile::GetLength() const
{
	if (m_bAllBufferLoaded)
		return m_nBufferSize;

	if (!m_pFile)
		return 0;
	
	return (DWORD)m_pFile->GetLength();	
}

//----------------------------------------------------------------------------
HANDLE CLineFile::GetHandleFile() const
{
	if (!m_pFile)
		return CFile::hFileNull;
	
	return m_pFile->m_hFile;
}	

//----------------------------------------------------------------------------
BOOL CLineFile::OpenOnVirtualDriver (LPCTSTR pszFileName, UINT nOpenFlags, FileFormat format /*ANSI*/)
{
	IFileSystemManager* pFileSystemManager = AfxGetFileSystemManager ();

	// not managed
	if (!pFileSystemManager || !pFileSystemManager->IsManagedByAlternativeDriver(pszFileName))
		return FALSE;

	m_nCurrBuffPos		= 0;
	m_nCurrFilePos		= 0;
	m_nLastOperation	= NO_OPE;
	m_sFileName			= pszFileName;

	// get the content and is prepared to write
	CString sFileContent = pFileSystemManager->GetTextFile (pszFileName);
	if (sFileContent.IsEmpty())
	{
		if ((nOpenFlags & CFile::modeCreate) == 0)
			return FALSE;
	
		// create buffer preparing operations
		m_nBufferSize		= BUFFER_SIZE;
		m_pchBuffer 		= new TCHAR[m_nBufferSize + 1];	//un carattere per il terminatore
		m_nCount			= m_nBufferSize + 1;
		m_bAllBufferLoaded	= TRUE;
		return TRUE;
	}

	m_nBufferSize		= sFileContent.GetLength();
	m_pchBuffer 		= new TCHAR[m_nBufferSize + 1];	//un carattere per il terminatore
	m_nCount			= m_nBufferSize + 1;

	int i=0;
	for (i=0; i < sFileContent.GetLength(); i++)
		m_pchBuffer[i] = sFileContent.GetAt (i);

	m_pchBuffer[i]		= _T('\0');
	m_bAllBufferLoaded	= TRUE;

	return TRUE;
}

//----------------------------------------------------------------------------
BOOL CLineFile::WriteOnVirtualDriver (LPCTSTR pszFileName)
{
	// not managed
	IFileSystemManager* pFileSystemManager = AfxGetFileSystemManager ();
	if (!pFileSystemManager || !pFileSystemManager->IsManagedByAlternativeDriver(pszFileName))
		return FALSE;

	if (m_nCurrBuffPos)
		m_pchBuffer[m_nCurrBuffPos] = _T('\0');

	CString sContent (m_pchBuffer);
	return pFileSystemManager->SaveTextFile (m_sFileName, sContent);
}

//-----------------------------------------------------------------------------
BOOL CLineFile::GetStatus (const CString& sFileName, CFileStatus& fs)
{
	return ::GetStatus (sFileName, fs);
}

/////////////////////////////////////////////////////////////////////////////
// CLineText implentation
/////////////////////////////////////////////////////////////////////////////
IMPLEMENT_DYNAMIC(CLineText, CObject);

//-----------------------------------------------------------------------------
CLineText::CLineText(LPCTSTR pszInput, long nStringLen)
	:
	m_nLineNumber			(0),
	m_pszOriginalInputString(pszInput),
	m_pszLineStart			(NULL)
{
	
	int nLen = (nStringLen < 0)	? _tcslen(pszInput): nStringLen;
	
	// Il buffer viene copiato poiche` la GetLine lo modifica.
	//
	// Nel caso in cui l'ultimo carattere della stringa
	// non sia un LF ('\n') viene allocato un byte in piu` della sua
	// lunghezza (oltre a quello per il '\0' di normale terminazione)
	// per aggiungere un ulteriore '\0' per interrompere correttamente
	// l'algoritmo della LoadBuffer (vedi piu` sotto)
	//
	if (!nLen || pszInput[nLen - 1] != LF_CHAR)
	{
		m_pszInputString = new TCHAR [nLen + 2];
		m_pszInputString[nLen + 1] = NULL_CHAR;
	}
	else
		m_pszInputString = new TCHAR [nLen + 1];
	
	// copia la stringa nel nuovo buffer, la copio solo di nLen caratteri a causa di Win95-Win98 che
	// nel caso di file mappato in memoria restituisce una stringa non terminata con \0
	TB_TCSNCPY(m_pszInputString, pszInput, nLen);
	m_pszInputString[nLen] = NULL_CHAR;
}

//-----------------------------------------------------------------------------
CLineText::~CLineText()
{
	if (m_pszInputString) 
		delete [] m_pszInputString;
}

//-----------------------------------------------------------------------------
int CLineText::GetLine(LPTSTR& pszBuffer)
{
	m_nLineNumber++;
	
	// La prima volta viene usato il puntatore all'inizio stringa, per le righe
	// seguenti viene usato il puntatore al primo byte successivo al '\n' trovato
	// in precedenza
	//
	// NB. non viene usata la strtok, che ha una modalita` di funzionamento simile,
	// per il fatto che essa elimina in botto piu` '\n' consecutivi (vedi documentazione)
	// facendo perdere conoscenza del numero di righe parsate
	//
	if (m_nLineNumber == 1)
		m_pszCurrLine =  m_pszInputString;		

	if (m_pszCurrLine[0])
	{
		TCHAR* pszNextLine = _tcschr(m_pszCurrLine, LF_CHAR);
		if (pszNextLine)
			*pszNextLine = NULL_CHAR;		// break input string to get substring

		// ritorna e memorizza il puntatore all'inizio della nuova linea
		pszBuffer =  m_pszCurrLine;
		m_pszLineStart = m_pszOriginalInputString;

		int nLineLen = _tcslen(m_pszCurrLine);

		// Si prepara per la successiva iterazione spostando il puntatore alla
		// successiva linea.
		// Questa istruzione e` resa consistente dalla corretta allocazione
		// di m_pszInputString effettuata dal costruttore
		//
		m_pszCurrLine = &m_pszCurrLine[nLineLen + 1];
		m_pszOriginalInputString = &m_pszOriginalInputString[nLineLen + 1];
		
		return nLineLen;
	}

	pszBuffer = NULL;
	return -1;
}

//=============================================================================
BOOL LoadLineTextFile (const CString& strFile, CString& strText)
{
	CFileException  exception;  	// signal file exception
	CLineFile       ifile;      	// file to be parsed
	UINT			flags = CFile::modeRead | CFile::typeText | CFile::shareDenyWrite ;

	if (!ifile.Open(strFile, flags, &exception))
	{
		strText = PCause(&exception);
		return FALSE;
	}

	TRY // try to read from file
	{
		CString buffer;
		while (ifile.ReadString(buffer.GetBuffer(1024), 1023, TRUE))
		{
			buffer.ReleaseBuffer();
			
			strText += buffer;
		}
		ifile.Close();
	}
	CATCH (CFileException, e)
	{
		strText = PCause(e);
		return FALSE;
	}
	END_CATCH

	return TRUE;
}
