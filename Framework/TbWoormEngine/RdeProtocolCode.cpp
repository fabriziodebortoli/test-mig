//---------------------------------------------------------------------------
// Protocol structure:
// ===================
//
// The record file contain the follow data format
//
// For data exchanging;
// RDEManager::INTERNAL_ID
//
// HEADER{short tag,	RDEcmd	Cmd,	DataSize	size		},		<stream size-data>
// {			ID,				STATUS,				LEN(nByte)	},		DATA
//
// with ID > 0.
//
//
// For object related command exchanging:
// RDEManager::ID_COMMAND
//
// short,	RDEcmd,	DataSize
// ID,		CMD,	LPARAM    (M4 2.0 - CUSTOM_TITEL_LINE) , <stream size-data>
//
// with ID < 0.
//
//
// For general command exchanging
// RDEManager::GEN_COMMAND
//
// short,	RDEcmd,	DataSize,	nByte
// 0,		CMD,	LEN(nByte), DATA
//
//---------------------------------------------------------------------------
//	Read sample:
//	===========
//
//	void ReadFromRdeFile(LPCTSTR fileName)
//	{
//		RDEManager channel(this);
//		channel.Open (fileName);
//
//		RDEData	data;
//		BOOL	loop = TRUE;
//
//		while ( loop && !channel.Eof() )
//			switch ( channel.LookAhead() )
//			{
//				case RDEManager::INTERNAL_ID :
//				{
//					loop =	(channel.GetId() != 0) && channel.GetDataValue(data);
//					break;
//				}
//
//				case RDEManager::ID_COMMAND :
//				{
//					DataSize	lpar	= channel.GetParam();
//					RDEcmd		cmd		= channel.GetCommand();
//
//					loop = (cmd != RDEManager::NONE);
//
//					if (loop)
//						channel.SkipData();
//
//					break;
//				}
//
//				case RDEManager::GEN_COMMAND :
//				{
//					DataSize	lpar	= channel.GetParam();
//					RDEcmd		cmd		= channel.GetCommand();
//
//					loop =	(cmd != RDEManager::NONE);
//
//					if (loop)
//					{
//						channel.SkipData(); 
//						if (cmd == RDEManager::NEW_PAGE)
//							loop = DoPause();
//					}
//
//					break;
//				}
//
//				default :
//				{
//					//Some error was occurred ...
//					return;
//				}
//			}
//	}
//
//=============================================================================

#include "stdafx.h"

#include <io.h>
#include <errno.h>

#include <TbNameSolver\FileSystemFunctions.h>
#include <TbNameSolver\TBNamespaces.h>
#include <TbNameSolver\PathFinder.h>

#include <TbGeneric\GeneralFunctions.h>
#include <TbGeneric\DatesFunctions.h>
#include <TbGeneric\TbStrings.h>
#include <TbGeneric\schedule.h>
#include <TbGeneric\ParametersSections.h>
#include <TbGeneric\SettingsTable.h>
#include <TbGeneric\LineFile.h>

#include "repfield.h"
#include "reptable.h"
#include "repengin.h"
#include "MultiLayout.h"

#include "RdeProtocolCode.h"
 
//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char BASED_CODE THIS_FILE[] = __FILE__;
#endif

#define RDE_BUFFER_SIZE		32767

static const TCHAR szRdeExt [] = _T("rde");

//==============================================================================
//          Class RDEFile implementation
//==============================================================================

//-----------------------------------------------------------------------------
RDEFile::RDEFile(RDEManager& Owner)
	:
	CFile		(),
	m_TheOwner	(Owner)
{
	m_Header.m_nTag	= 0;
	m_Header.m_Cmd	= 0;
	m_Header.m_Size	= 0;
}

//-----------------------------------------------------------------------------
BOOL RDEFile::RdeOpenFile(const CString& strFileName, UINT nOpenFlags)
{
	CFileException e;

	if (!CFile::Open(strFileName, nOpenFlags, &e))
	{
		m_TheOwner.MessageError(RDEManager::ErrorMessages::CANT_OPEN_FILE(), &e, strFileName);
		m_TheOwner.m_MngrStatus = RDEManager::FILE_CLOSED;
		return FALSE;
	}

	// to check if SHARE.EXE (or VSHARE for Windows for Workgroup) is installed
	// we must attempt to lock the file (see Interrupt 21h function 5Ch, MS dixit)
	TRY
	{
		CFile::LockRange(0, 1);
	}
	CATCH(CFileException, e)
	{
		switch (e->m_cause)
		{
			case CFileException::genericException :
			case CFileException::sharingViolation :
			case CFileException::lockViolation :
			{
				m_TheOwner.MessageError(RDEManager::ErrorMessages::SHARE_NOT_EXISTS(), NULL, strFileName);
				m_TheOwner.m_MngrStatus = RDEManager::SHARING_VIOLATION;
				return FALSE;
			}	
			default :
			{
				m_TheOwner.MessageError(RDEManager::ErrorMessages::CANT_OPEN_FILE(), e, strFileName);
				m_TheOwner.m_MngrStatus = RDEManager::FATAL_ERROR;
				return FALSE;
			}
		}
	}
	END_CATCH

	CFile::UnlockRange(0, 1);

	m_strRDEFileName		= strFileName;
	m_TheOwner.m_MngrStatus	= RDEManager::NO_ERROR_;
	
	return TRUE;
}


//==============================================================================
//          Class RDEFileWriter implementation
//==============================================================================

//-----------------------------------------------------------------------------
RDEFileWriter::RDEFileWriter(RDEManager& Owner)
	:
	RDEFile					(Owner),
	m_dwBeginPagePosition 	(0),
	m_bPostedNewPage		(FALSE)
	//,
	//m_sLayoutName			(_T("default"))
{
}

//-----------------------------------------------------------------------------
BOOL RDEFileWriter::TryWrite (void* pBuff, UINT nCount)
{
	if (m_TheOwner.m_MngrStatus != RDEManager::NO_ERROR_) return FALSE;

	BOOL 	bRetry;
	DWORD   dwCurPos = (DWORD)GetPosition();
									
	do
	{         
		bRetry = FALSE;
		
		TRY
		{
			CFile::Write(pBuff, nCount);
		}
		CATCH(CFileException, e)
		{
			switch (e->m_cause)
			{
				case CFileException::sharingViolation :
				{
					m_TheOwner.MessageError(RDEManager::ErrorMessages::SHARE_NOT_EXISTS(), e, m_strRDEFileName);
					m_TheOwner.m_MngrStatus = RDEManager::SHARING_VIOLATION;
					return FALSE;
				}
				case CFileException::diskFull :
				{
					bRetry = m_TheOwner.MessageError(
													RDEManager::ErrorMessages::DISK_FULL(),
													e,
													m_strRDEFileName,
													0,
													MB_RETRYCANCEL
													) == IDRETRY;
													
					m_TheOwner.m_MngrStatus = RDEManager::DISK_FULL;
					if (bRetry)
					{
						Seek(dwCurPos, CFile::begin);
						break;                      
					}
					else
						return FALSE;
				}
				case CFileException::lockViolation :
				{
					m_TheOwner.MessageError(RDEManager::ErrorMessages::CANT_WRITE_FILE(), e, m_strRDEFileName);
					m_TheOwner.m_MngrStatus = RDEManager::LOCKED;
					return FALSE;
				}
				default :
				{
					m_TheOwner.MessageError(RDEManager::ErrorMessages::CANT_WRITE_FILE(), e, m_strRDEFileName);
					m_TheOwner.m_MngrStatus = RDEManager::FATAL_ERROR;
					return FALSE;
				}
			}
		}
		END_CATCH
	}
	while (bRetry);
	
	m_TheOwner.m_MngrStatus = RDEManager::NO_ERROR_;

	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL RDEFileWriter::RdeOpenOutput(const CString& strFileName)
{
	m_dwBeginPagePosition   = 0;

	if (! RDEFile::RdeOpenFile(strFileName, 
			CFile::modeCreate | CFile::modeWrite | CFile::shareDenyNone | CFile::typeBinary))
		return FALSE;

	return TRUE;
}

//-----------------------------------------------------------------------------
void RDEFileWriter::MenageDiskFull()
{
	if (m_TheOwner.m_MngrStatus != RDEManager::DISK_FULL) return;
	
	m_Header.m_nTag	= 0;
	m_Header.m_Cmd	= RDEManager::END_OF_REPORT;
	m_Header.m_Size	= 0;

	Seek(m_TheOwner.m_dwLockPosition, CFile::begin);
	SetLength(m_TheOwner.m_dwLockPosition);

	if (TryWrite(&m_Header, sizeof(m_Header)))
		m_TheOwner.m_PageSeekPoints.Add(m_dwBeginPagePosition);
	else
	{
		Seek(m_dwBeginPagePosition, CFile::begin);
		SetLength(m_dwBeginPagePosition);
		TryWrite(&m_Header, sizeof(m_Header));
	}

	m_TheOwner.m_MngrStatus		= RDEManager::NO_ERROR_;
	m_TheOwner.m_bCanSaveRde	= TRUE;
}           
						
//-----------------------------------------------------------------------------
BOOL RDEFileWriter::RdeWrite (short nId, RDEData& RdeData, BOOL bLimited/* = TRUE*/)
{
	if (m_TheOwner.m_MngrStatus != RDEManager::NO_ERROR_) 
		return FALSE;
					 
	// if it's holding a NEW_PAGE command then emits it
	if (m_bPostedNewPage)
	{
		if (!RdeWriteNewPageCommand())
			return( FALSE );
	}

	m_Header.m_nTag  = nId;
	m_Header.m_Cmd   = RdeData.GetStatus();
	m_Header.m_Size  = RdeData.GetLen();

	// The actual file position is locked then we begin an atomic write
	ASSERT(!bLimited || m_Header.m_Size <= 0x8FFF);

	if (
		!TryWrite(&m_Header, sizeof(m_Header)) ||
		((RdeData.GetLen() != 0) && !TryWrite(RdeData.GetData(), (bLimited ? (m_Header.m_Size & ~0x8000) : m_Header.m_Size)))
		)
	{
		MenageDiskFull();
		return FALSE;
	}

	m_TheOwner.m_dwLockPosition = (DWORD)GetPosition();

	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL RDEFileWriter::RdeWrite (short nId, RDEcmd Cmd, DataSize Size)
{
	if (m_TheOwner.m_MngrStatus != RDEManager::NO_ERROR_) 
		return FALSE;

	if (m_bPostedNewPage)
	{
		// if it is holding a NewPage command
		
		if (nId == 0 && Cmd == RDEManager::NEW_PAGE)
		{
			// ignore other NewPage commands
			return TRUE;
		}

		if (nId == 0 && Cmd == RDEManager::END_OF_REPORT)
		{
			// ignore holded NewPage and emit only EndOfReport command
			return RdeWriteCommand(nId, Cmd, Size);
		}

		// musts emit current holded NewPage commnad and continue if write ok
		if (!RdeWriteNewPageCommand())
			return FALSE;
	}
	else if (nId == 0 && Cmd == RDEManager::NEW_PAGE)
	{
		// hold the actual NEW_PAGE command
		m_bPostedNewPage = TRUE;
		return TRUE;
	}
	
	// emits current command
	return RdeWriteCommand(nId, Cmd, Size);
}

//-----------------------------------------------------------------------------
BOOL RDEFileWriter::RdeWriteNewPageCommand (const CString& sLayout)
{
	if (m_TheOwner.m_MngrStatus != RDEManager::NO_ERROR_) 
		return FALSE;

	//TODO m_sLayoutName = sLayout;

   if (m_bPostedNewPage)
		return TRUE;

	// hold the actual NEW_PAGE command
	m_bPostedNewPage = TRUE;

	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL RDEFileWriter::RdeWriteNewPageCommand	()
{
	BOOL bOk = RdeWriteCommand(0, RDEManager::NEW_PAGE, 0);

	return bOk;
}

//-----------------------------------------------------------------------------
BOOL RDEFileWriter::RdeWriteCommand (short nId, RDEcmd Cmd, DataSize Size)
{
	m_bPostedNewPage = FALSE;

	//if (Cmd != RDEManager::NEW_PAGE)
	{
		m_Header.m_nTag	= -(nId);          // to indicate command tag
		m_Header.m_Cmd	= Cmd;
		m_Header.m_Size	= Size;

		// The actual file position is locked then we begin an atomic write
 
		if (!TryWrite(&m_Header, sizeof(m_Header)))
		{
			MenageDiskFull();
			return FALSE;
		}
	}

	//----
	m_TheOwner.m_dwLockPosition = (DWORD)GetPosition();

	if ((nId == 0) && ((Cmd == RDEManager::NEW_PAGE) || (Cmd == RDEManager::END_OF_REPORT)))
	{
		m_TheOwner.m_PageSeekPoints.Add(m_dwBeginPagePosition); // data start of new page

		if (Cmd != RDEManager::END_OF_REPORT)
			m_dwBeginPagePosition = (DWORD)GetPosition();
		else
			m_TheOwner.m_bCanSaveRde = TRUE;

		if (m_TheOwner.m_pEngine && m_TheOwner.m_pEngine->GetEngine())
		{
			ASSERT_VALID(m_TheOwner.m_pEngine);
			ASSERT_VALID(m_TheOwner.m_pEngine->GetEngine());
			SymField* pF = m_TheOwner.m_pEngine->GetEngine()->GetSymTable().GetFieldByID(SpecialReportField::ID.LAYOUT);
			if (pF)
			{
				ASSERT_KINDOF(WoormField, pF);
				//ASSERT(m_sLayoutName == m_TheOwner.m_pEngine->GetCallerDoc()->m_dsCurrentLayoutEngine);
				pF->AssignData (m_TheOwner.m_pEngine->GetCallerDoc()->m_dsCurrentLayoutEngine);
				((WoormField*)pF)->Write(*(m_TheOwner.m_pEngine->GetEngine()), TRUE);
			}
			pF = m_TheOwner.m_pEngine->GetEngine()->GetSymTable().GetFieldByID(SpecialReportField::ID.PAGE);
			if (pF)
			{
				ASSERT(pF->IsKindOf(RUNTIME_CLASS(WoormField)));
				//ritorna sempre -1, � solo per la lettura //int np = aRepEngine.GetOutChannel()->CurrPage();
				DataLng* pNP = (DataLng*) pF->GetData();
				pF->AssignData (*pNP + 1);
				((WoormField*)pF)->Write(*(m_TheOwner.m_pEngine->GetEngine()), TRUE);
			}
		}
		// signal to user front-end begin of new page
		// ID_RDE_NEWPAGE must be a unique User message
		if (m_TheOwner.m_hWndComunication)
		{
			::SendMessage(m_TheOwner.m_hWndComunication, WM_COMMAND, ID_RDE_NEWPAGE, 0);
		}
	}
	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL RDEFileWriter::RdeWriteInitialSpecialField()
{
	if (!m_TheOwner.m_pEngine || !m_TheOwner.m_pEngine->GetEngine())
	{
		ASSERT(FALSE);
		return FALSE;
	}
	ASSERT_VALID(m_TheOwner.m_pEngine);
	ASSERT_VALID(m_TheOwner.m_pEngine->GetEngine());
	WoormField* pF = m_TheOwner.m_pEngine->GetEngine()->GetSymTable().GetFieldByID(SpecialReportField::ID.LAYOUT);
	if (pF)
	{
		ASSERT_KINDOF(WoormField, pF);
		ASSERT(*(pF->GetData()) == m_TheOwner.m_pEngine->GetCallerDoc()->m_dsCurrentLayoutEngine);

		pF->Write(*(m_TheOwner.m_pEngine->GetEngine()), TRUE);
	}
	pF = m_TheOwner.m_pEngine->GetEngine()->GetSymTable().GetFieldByID(SpecialReportField::ID.PAGE);
	if (pF)
	{
		ASSERT(pF->IsKindOf(RUNTIME_CLASS(WoormField)));

		pF->AssignData (DataLng(1));
		pF->Write(*(m_TheOwner.m_pEngine->GetEngine()), TRUE);
	}
	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL RDEFileWriter::ConcatWRMFile (LPCTSTR pszWrmFileName, DWORD& dwWrmEnd)
{
	CFileException	e;
	CFile			WrmFile;	// closed at return
	
	UINT nOpenFlags = CFile::modeRead | CFile::typeBinary;
															
	if (!WrmFile.Open(pszWrmFileName, nOpenFlags, &e))
	{
		m_TheOwner.MessageError(RDEManager::ErrorMessages::CANT_OPEN_FILE(), &e, pszWrmFileName);
		return FALSE;
	}

	CFileStatus	WrmStat;        

	if (!CLineFile::GetStatus(pszWrmFileName, WrmStat))
	{
		m_TheOwner.MessageError(RDEManager::ErrorMessages::CANT_OPEN_FILE(), NULL, pszWrmFileName);
		return FALSE;
	}
		
	LONG nWrmSize = (LONG)WrmStat.m_size;
	if (nWrmSize == 0) return TRUE;
	
	UINT nBufSize = (UINT) (nWrmSize < RDE_BUFFER_SIZE ? nWrmSize : RDE_BUFFER_SIZE);
	unsigned char* pchBuf = new unsigned char[nBufSize];
				 
	BOOL ok = TRUE;
		
	while (ok && nWrmSize)
	{
		TRY
		{
			WrmFile.Read(pchBuf, nBufSize);
		}
		CATCH(CFileException, re)
		{
			m_TheOwner.MessageError(RDEManager::ErrorMessages::CANT_READ_FILE(), re, pszWrmFileName);
			ok = FALSE;
		}
		END_CATCH

		if (ok)
		{
			ok = TryWrite(pchBuf, nBufSize);
					
			nWrmSize -= nBufSize;
	
			if (nWrmSize < (LONG)nBufSize)
				nBufSize = (UINT)nWrmSize;
		}
	}
		
	delete [] pchBuf;
	
	if (ok)
		dwWrmEnd = (DWORD)GetPosition();	// return the end of embeded WRM
		
	return ok;
}

//-----------------------------------------------------------------------------
BOOL RDEFileWriter::SetInfo(LPCTSTR pszWrmFileName)
{
	// after the end of report begin the page informations		
	m_TheOwner.m_Tail.m_dwPageInfoOffs = (DWORD)GetPosition();
	
	UINT nPage = m_TheOwner.m_PageSeekPoints.GetSize();
	if (!TryWrite(&nPage, sizeof(nPage))) return FALSE;

	DWORD dwOffs;
	
	for (UINT i = 0; i < nPage; i++)
	{
		dwOffs = m_TheOwner.m_PageSeekPoints[i];
		if (!TryWrite(&dwOffs, sizeof(dwOffs))) return FALSE;
	}                                   
															
	// after page informations can begin the embeded WRM
	m_TheOwner.m_Tail.m_dwWrmStartOffs = m_TheOwner.m_Tail.m_dwWrmEndOffs = (DWORD)GetPosition();
	
	if (pszWrmFileName && *pszWrmFileName && 
		!ConcatWRMFile(pszWrmFileName, m_TheOwner.m_Tail.m_dwWrmEndOffs))
		return FALSE;
		
	return TryWrite(&(m_TheOwner.m_Tail), sizeof(m_TheOwner.m_Tail));
}

//==============================================================================
//          Class RDEFileReader implementation
//==============================================================================

//-----------------------------------------------------------------------------
DataSize RDEFileReader::GetParam ()
{
	return
		(m_TheOwner.m_MngrStatus != RDEManager::NO_ERROR_)
			? 0
			: m_Header.m_Size;
}

//-----------------------------------------------------------------------------
WORD RDEFileReader::GetId ()
{
	return (WORD)
		(
			(m_TheOwner.m_MngrStatus != RDEManager::NO_ERROR_)
				? 0 
				: (m_Header.m_nTag < 0 ? -(m_Header.m_nTag) : m_Header.m_nTag)
		);
}

//-----------------------------------------------------------------------------
RDEcmd RDEFileReader::GetCommand ()
{
	return
		(m_TheOwner.m_MngrStatus != RDEManager::NO_ERROR_)
			? RDEManager::NONE
			: m_Header.m_Cmd;
}

//-----------------------------------------------------------------------------
RDEFileReader::RDEFileReader(RDEManager& Owner)
	:
	RDEFile			(Owner),
	m_bGoAhead		(TRUE),
	m_bBuffered		(TRUE),
	m_nBufferSize	(RDE_BUFFER_SIZE)
{
	m_bBuffered		= *((DataBool*) AfxGetSettingValue (snsTbGeneric, szRdeProtocol, szRdeBuffered, DataBool(FALSE)));
	//DataInt di		= *((DataInt*) AfxGetSettingValue(snsTbGeneric, szRdeProtocol, szRdeBufferSize, DataInt(RDE_BUFFER_SIZE)));
	//m_nBufferSize	= (UINT)(int) di;

	m_pchBuffer			= (m_bBuffered) ? new unsigned char[m_nBufferSize] : NULL;
	m_pchCurrentByte	= m_pchBuffer;
	m_nBytesNumber		= 0;
	m_dwReadStart		= 0;
}

//-----------------------------------------------------------------------------
RDEFileReader::~RDEFileReader()
{
	if (m_pchBuffer)
	{
		delete m_pchBuffer;
		m_pchBuffer = NULL;
	}
}

//-----------------------------------------------------------------------------
BOOL RDEFileReader::TryRead (void* pBuff, UINT nCount)
{
	if (m_TheOwner.m_MngrStatus != RDEManager::NO_ERROR_) return FALSE;

	UINT nByte = 0;

	TRY
	{
		nByte = CFile::Read(pBuff, nCount);
	}
	CATCH(CFileException, e)
	{
		switch (e->m_cause)
		{
			case CFileException::sharingViolation :
			{
				m_TheOwner.MessageError(RDEManager::ErrorMessages::SHARE_NOT_EXISTS(), e, m_strRDEFileName);
				m_TheOwner.m_MngrStatus = RDEManager::SHARING_VIOLATION;
				return FALSE;
			}
			case CFileException::lockViolation :
			{
				m_TheOwner.MessageError(RDEManager::ErrorMessages::CANT_READ_FILE(), e, m_strRDEFileName, 0, MB_ICONINFORMATION);
				m_TheOwner.m_MngrStatus = RDEManager::LOCKED;
				return FALSE;
			}
			case CFileException::endOfFile :
			{
				m_TheOwner.MessageError(RDEManager::ErrorMessages::CANT_READ_FILE(), e, m_strRDEFileName, 0, MB_ICONINFORMATION);
				m_TheOwner.m_MngrStatus = RDEManager::END_OF_FILE;
				return FALSE;
			}
			default :
			{
				m_TheOwner.MessageError(RDEManager::ErrorMessages::CANT_READ_FILE(), e, m_strRDEFileName);
				m_TheOwner.m_MngrStatus = RDEManager::FATAL_ERROR;
				return FALSE;
			}
		}
	}
	END_CATCH

	if (nByte != nCount)
	{
		if (nByte == 0)
			m_TheOwner.m_MngrStatus = RDEManager::LOCKED;
		else
			m_TheOwner.m_MngrStatus = RDEManager::FATAL_ERROR;

		m_TheOwner.MessageError(RDEManager::ErrorMessages::CANT_READ_FILE(), NULL, m_strRDEFileName);
		return FALSE;
	}
	else
	{
		m_TheOwner.m_MngrStatus = RDEManager::NO_ERROR_;
		return TRUE;
	}
}

//-----------------------------------------------------------------------------
BOOL RDEFileReader::TrySeek (LONG lOffs, CFile::SeekPosition From)
{
	if (m_TheOwner.m_MngrStatus != RDEManager::NO_ERROR_) return FALSE;

	TRY
	{
		CFile::Seek(lOffs, From);
	}
	CATCH(CFileException, e)
	{
		switch (e->m_cause)
		{
			case CFileException::accessDenied :
			{
				m_TheOwner.MessageError(RDEManager::ErrorMessages::CANT_SEEK_FILE(), e, NULL);
				m_TheOwner.m_MngrStatus = RDEManager::LOCKED;
				return FALSE;
			}
			case CFileException::endOfFile :
			{
				m_TheOwner.MessageError(RDEManager::ErrorMessages::CANT_SEEK_FILE(), e, NULL);
				m_TheOwner.m_MngrStatus = RDEManager::END_OF_FILE;
				return FALSE;
			}
			case CFileException::badSeek :
			{
				m_TheOwner.MessageError(RDEManager::ErrorMessages::CANT_SEEK_FILE(), e, NULL);
				m_TheOwner.m_MngrStatus = RDEManager::BAD_SEEK;
				return FALSE;
			}
			default :
			{
				m_TheOwner.MessageError(RDEManager::ErrorMessages::CANT_SEEK_FILE(), e, NULL);
				m_TheOwner.m_MngrStatus = RDEManager::FATAL_ERROR;
				return FALSE;
			}
		}
	}
	END_CATCH

	m_TheOwner.m_MngrStatus = RDEManager::NO_ERROR_;
	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL RDEFileReader::RdeOpenInput(const CString& strFileName)
{
	m_bGoAhead = TRUE;

	return RDEFile::RdeOpenFile
		(strFileName, CFile::modeRead | CFile::shareDenyNone | CFile::typeBinary);
}


//-----------------------------------------------------------------------------
BOOL RDEFileReader::RdeRead (void* pBuff, DataSize Size)
{
	if (m_bBuffered)
	{                   
		//@@@ da gestire il caso in cui sizeof(DataSize) > sizeof(UINT)
		//@@@ e quindi la possibilita` che il numero di byte da leggere sia
		//@@@ superiore a quello gestibile dalla TryRead; 
		DataSize		RemBytes	= Size;
		unsigned char*	pchRemBuff	= (unsigned char*)pBuff;

		if (m_nBytesNumber < Size)
		{
			if (m_nBytesNumber)
			{                                        
				//@@@ da gestire il caso in cui sizeof(DataSize) > sizeof(UINT)
				memcpy(pchRemBuff, m_pchCurrentByte, (size_t)m_nBytesNumber);
				RemBytes	-= m_nBytesNumber;
				pchRemBuff	+= m_nBytesNumber;
			}

			// Must be used only for linked client/server processes because the m_dwLockPosition
			// is set only by writer metods
			DWORD dwDistance = m_TheOwner.m_dwLockPosition - (DWORD)GetPosition();

			if (dwDistance < RemBytes)
			{
				m_TheOwner.m_MngrStatus = RDEManager::LOCKED;
				m_TheOwner.MessageError(RDEManager::ErrorMessages::CANT_READ_FILE(), NULL, NULL);
				return FALSE;
			}

			m_pchCurrentByte = m_pchBuffer;
			m_nBytesNumber = (UINT) (m_nBufferSize > dwDistance ? dwDistance : m_nBufferSize);

			m_dwReadStart = (DWORD)GetPosition();

			if (!TryRead(m_pchBuffer, m_nBytesNumber))
				return FALSE;
		}

		//@@@ da gestire il caso in cui sizeof(DataSize) > sizeof(UINT)
		memcpy(pchRemBuff, m_pchCurrentByte, (size_t)RemBytes);
		m_nBytesNumber		-= RemBytes;
		m_pchCurrentByte	+= RemBytes;

		return TRUE;
	}

	// unbuffered read
	// Must be used only for linked client/server processes because the m_dwLockPosition
	// is set only by writer metods
	if (GetPosition() >= m_TheOwner.m_dwLockPosition)
	{
		m_TheOwner.m_MngrStatus = RDEManager::LOCKED;
		m_TheOwner.MessageError(RDEManager::ErrorMessages::CANT_READ_FILE(), NULL, NULL);
		return FALSE;
	}

	return TryRead(pBuff, Size);
}

//-----------------------------------------------------------------------------
BOOL RDEFileReader::RdeSeek (DWORD dwOffs)
{
	if (m_bBuffered)
	{
		// after a bed seek it can retry a new seek
		if (m_TheOwner.m_MngrStatus == RDEManager::BAD_SEEK)
			m_TheOwner.m_MngrStatus = RDEManager::NO_ERROR_;

		// Must be used only for linked client/server processes because the m_dwLockPosition
		// is set only by writer metods
		if (dwOffs > m_TheOwner.m_dwLockPosition)
		{
			m_TheOwner.m_MngrStatus = RDEManager::LOCKED;
			m_TheOwner.MessageError(RDEManager::ErrorMessages::CANT_SEEK_FILE(), NULL, NULL);
			return FALSE;
		}

		DWORD dwCurPos = (DWORD)GetPosition();

		if (dwOffs >= m_dwReadStart && dwOffs < dwCurPos)
		{
			m_pchCurrentByte = m_pchBuffer + dwOffs - m_dwReadStart;
			//@@@ da gestire il caso in cui sizeof(DataSize) > sizeof(UINT)
			m_nBytesNumber = (UINT)(dwCurPos - dwOffs);
		}
		else
		{
			m_pchCurrentByte = m_pchBuffer;
			m_nBytesNumber = 0;

			if (!TrySeek(dwOffs)) return FALSE;
		}

		m_bGoAhead = TRUE;
		return TRUE;
	}

	// UNbuffered seek
	// after a bed seek it can retry a new seek
	if (m_TheOwner.m_MngrStatus == RDEManager::BAD_SEEK)
		m_TheOwner.m_MngrStatus = RDEManager::NO_ERROR_;

	// Must be used only for linked client/server processes because the m_dwLockPosition
	// is set only by writer metods
	if (dwOffs > m_TheOwner.m_dwLockPosition)
	{
		m_TheOwner.m_MngrStatus = RDEManager::LOCKED;
		m_TheOwner.MessageError(RDEManager::ErrorMessages::CANT_SEEK_FILE(), NULL, NULL);
		return FALSE;
	}

	if (!TrySeek(dwOffs)) return FALSE;

	m_bGoAhead = TRUE;
	return TRUE;
}

//-----------------------------------------------------------------------------
RDEManager::RDEDataType RDEFileReader::LookAhead ()
{
	if (m_bGoAhead && !RdeRead(&m_Header, sizeof(m_Header)))
		return RDEManager::UNKNOWN;

	m_bGoAhead = FALSE;

	if (m_Header.m_nTag < 0)
		 return RDEManager::ID_COMMAND;

	if (m_Header.m_nTag > 0)
		return RDEManager::INTERNAL_ID;

	return RDEManager::GEN_COMMAND;
}

//-----------------------------------------------------------------------------
BOOL RDEFileReader::GetDataValue (RDEData& RdeData)
{
	if (m_TheOwner.m_MngrStatus != RDEManager::NO_ERROR_) 
		return FALSE;
	
	if (m_bGoAhead /*|| (m_Header.m_nTag < 0)*/)
		m_TheOwner.m_MngrStatus = RDEManager::BAD_READ;
	else
	{
		DataSize Len;
		if (m_Header.m_Size & 0x8000)
		{
			Len = m_Header.m_Size & ~0x8000;
			RdeData.ThisIsTailMultiLineString ( TRUE );
		}
		else
		{
			Len = m_Header.m_Size;
			RdeData.ThisIsTailMultiLineString ( FALSE );
		}

		void* pNewData = RdeData.NewData(Len, m_Header.m_Cmd);
		if (Len == 0 || RdeRead(pNewData, Len))
		{
			m_bGoAhead = TRUE;
			return TRUE;
		}
	}
	
	return FALSE;
}
//-----------------------------------------------------------------------------
BOOL RDEFileReader::GetDataValueUnlimited(RDEData& RdeData)
{
	if (m_TheOwner.m_MngrStatus != RDEManager::NO_ERROR_)
		return FALSE;

	if (m_bGoAhead /*|| (m_Header.m_nTag < 0)*/)
		m_TheOwner.m_MngrStatus = RDEManager::BAD_READ;
	else
	{
		DataSize Len;
			
		Len = m_Header.m_Size;
			
		RdeData.ThisIsTailMultiLineString(FALSE);
		
		void* pNewData = RdeData.NewData(Len, m_Header.m_Cmd);
		if (Len == 0 || RdeRead(pNewData, Len))
		{
			m_bGoAhead = TRUE;
			return TRUE;
		}
	}

	return FALSE;
}

//-----------------------------------------------------------------------------
BOOL RDEFileReader::SkipData ()
{
	if (m_TheOwner.m_MngrStatus != RDEManager::NO_ERROR_) return FALSE;

	if (m_Header.m_nTag < 0)
	{
		// if ID_COMMAND there's no more data after the header
		m_bGoAhead = TRUE;
		return TRUE;
	}
	else
	{
		// otherwise should be some data after the header
		RDEData DummyRDEData;
		return GetDataValue(DummyRDEData);
	}
}
 
//-----------------------------------------------------------------------------
BOOL RDEFileReader::SplitWRMFile (DWORD dwWrmStart, DWORD dwWrmEnd)
{
	if (!TrySeek(dwWrmStart))
		return FALSE;
											   
	LONG nWrmSize = (LONG) (dwWrmEnd - dwWrmStart);
	
	if (nWrmSize == 0) return TRUE;
	
	CFileException	e;
	CFile			WrmFile;
	
	UINT nOpenFlags = CFile::modeCreate | CFile::modeWrite | CFile::typeBinary;
															
	// Create a temporary file
	m_TheOwner.m_strPlaybackName = GetTempName(_T("WRM"));
	if (!WrmFile.Open(m_TheOwner.m_strPlaybackName, nOpenFlags, &e))
	{
		m_TheOwner.MessageError(RDEManager::ErrorMessages::CANT_OPEN_FILE(), &e, m_TheOwner.m_strPlaybackName);
		return FALSE;
	}

	UINT nBufSize = (UINT) (nWrmSize < (LONG)m_nBufferSize ? nWrmSize : m_nBufferSize);
	unsigned char* pchBuf = new unsigned char[nBufSize];
				 
	BOOL ok = TRUE;
		
	while (ok && nWrmSize)
	{
		ok = TryRead(pchBuf, nBufSize);
				
		if (ok)
		{
			TRY
			{
				WrmFile.Write(pchBuf, nBufSize);
			}
			CATCH(CFileException, we)
			{
				m_TheOwner.MessageError(RDEManager::ErrorMessages::CANT_WRITE_FILE(), we, m_TheOwner.m_strPlaybackName);
				ok = FALSE;
			}
			END_CATCH
		}
			
		nWrmSize -= nBufSize;

		if (nWrmSize < (LONG)nBufSize)
			nBufSize = (UINT)nWrmSize;
	}
		
	delete [] pchBuf;
	
	return ok;
}
		
// At the end of RDE file there are the starting offeset of page info and the starting and 
// ending offset of embeded WRM
//-----------------------------------------------------------------------------
BOOL RDEFileReader::GetInfo(BOOL bDoSplit)
{       
	if (!TrySeek(-((LONG) sizeof(m_TheOwner.m_Tail)), CFile::end)	||
		!TryRead(&(m_TheOwner.m_Tail), sizeof(m_TheOwner.m_Tail)))
		return FALSE;
	   
	// if the channel is opened in read only mode the lock position is at begin
	// of page info
	m_TheOwner.m_dwLockPosition = m_TheOwner.m_Tail.m_dwPageInfoOffs;

	UINT nPage;
	
	if (!TrySeek((LONG) m_TheOwner.m_Tail.m_dwPageInfoOffs)	||
		!TryRead(&nPage, sizeof(nPage)))
		return FALSE;
		
	DWORD dwPageIdx;

	for (UINT i = 0; i < nPage; i++)
	{
		if (!TryRead(&dwPageIdx, sizeof(dwPageIdx)))
			return FALSE;

		m_TheOwner.m_PageSeekPoints.Add(dwPageIdx);
	}
	
	if (bDoSplit && (m_TheOwner.m_Tail.m_dwWrmStartOffs != m_TheOwner.m_Tail.m_dwWrmEndOffs) &&
		!SplitWRMFile(m_TheOwner.m_Tail.m_dwWrmStartOffs, m_TheOwner.m_Tail.m_dwWrmEndOffs))
		return FALSE;
	
	m_TheOwner.m_bCanSaveRde	= TRUE;
	
	return TrySeek(0L);
}

//==============================================================================
//          Class RDEManager implementation
//==============================================================================

RDEManager::RDEStatus	RDEManager::GetRDEStatus()		{ return m_MngrStatus; }

void		RDEManager::AttachFrame		(CWnd* pWnd)	{ m_hWndComunication = pWnd ? pWnd->m_hWnd : NULL; }
int			RDEManager::LastPage 		() const { return m_PageSeekPoints.GetUpperBound(); }
int			RDEManager::CurrPageRead	() const { return m_nCurrentPageRead; }
BOOL		RDEManager::IsLastPage		() const { return m_nCurrentPageRead == LastPage(); }
BOOL		RDEManager::IsFirstPage		() const { return m_nCurrentPageRead <= 0; }
BOOL		RDEManager::IsClosed		() const { return m_MngrStatus == FILE_CLOSED; }
CString		RDEManager::GetWhichString	() const { return m_strStringToFind; }
BOOL		RDEManager::SearchForward	() const { return m_SearchDirection == FORWARD; }
BOOL		RDEManager::SearchCaseSens	() const { return m_CaseSensitivity == CASE_SENSITIVE; }
BOOL		RDEManager::SearchExactly	() const { return m_MatchMode == EXACTLY; } 

//-----------------------------------------------------------------------------
RDEManager::RDEDataType RDEManager::LookAhead	()
{
	return m_pInpFile ? m_pInpFile->LookAhead() : RDEManager::UNKNOWN;
}

//-----------------------------------------------------------------------------
BOOL RDEManager::GetDataValue ( RDEData& data )
{
	if (m_pInpFile == NULL) return FALSE;

	if (!m_pInpFile->GetDataValue(data)) return FALSE;
	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL RDEManager::GetDataValueUnlimited(RDEData& data)
{
	if (m_pInpFile == NULL) return FALSE;

	if (!m_pInpFile->GetDataValueUnlimited(data)) return FALSE;
	return TRUE;
}

//-----------------------------------------------------------------------------
WORD RDEManager::GetId ()
{
	return (WORD)(m_pInpFile ? m_pInpFile->GetId() : 0);
}

//-----------------------------------------------------------------------------
RDEcmd RDEManager::GetCommand	()
{
	return m_pInpFile ? m_pInpFile->GetCommand() : RDEManager::NONE;
}

//-----------------------------------------------------------------------------
DataSize RDEManager::GetParam ()
{
	return m_pInpFile ? m_pInpFile->GetParam() : 0;
}

//-----------------------------------------------------------------------------
BOOL RDEManager::SkipData ()
{
	return m_pInpFile ? m_pInpFile->SkipData() : FALSE;
}

//-----------------------------------------------------------------------------
BOOL RDEManager::IsColTotal () const
{
	return m_pInpFile ? RDEData::IsColTotalKind(m_pInpFile->GetCommand()) : FALSE;
}

//-----------------------------------------------------------------------------
BOOL RDEManager::IsSubTotal () const
{
	return m_pInpFile ? RDEData::IsSubTotalKind(m_pInpFile->GetCommand()) : FALSE;
}

//-----------------------------------------------------------------------------
BOOL RDEManager::SeekFirstPage ()
{
	return SeekPage(0);
}

//-----------------------------------------------------------------------------
BOOL RDEManager::SeekLastPage ()
{
	return SeekPage(0, CFile::end);
}

//-----------------------------------------------------------------------------
BOOL RDEManager::SeekCurrPage ()
{
	return SeekPage(0, CFile::current);
}

//-----------------------------------------------------------------------------
BOOL RDEManager::SeekNextPage ( int nPage )
{
	return SeekPage(nPage, CFile::current);
}

//-----------------------------------------------------------------------------
BOOL RDEManager::SeekPrevPage	( int nPage )
{
	return SeekPage(-(nPage), CFile::current);
}

//-----------------------------------------------------------------------------
BOOL RDEManager::Eop ()
{
	return (LookAhead() == GEN_COMMAND) && (GetCommand() == NEW_PAGE);
}

//-----------------------------------------------------------------------------
BOOL RDEManager::Eod ()
{
	return LookAhead() == UNKNOWN;
}

//-----------------------------------------------------------------------------
BOOL RDEManager::Eof ()
{
	return (LookAhead() == GEN_COMMAND) && (GetCommand() == END_OF_REPORT);
}

//-----------------------------------------------------------------------------
BOOL RDEManager::Write ( short id, RDEData& data, BOOL bLimited /*=TRUE*/ )
{
	return m_pOutFile ? m_pOutFile->RdeWrite(id, data, bLimited) : FALSE;
}

//-----------------------------------------------------------------------------
BOOL RDEManager::Write ( short tag, RDEcmd cmd, DataSize size )
{
	return m_pOutFile ? m_pOutFile->RdeWrite(tag, cmd, size) : FALSE;
}

//-----------------------------------------------------------------------------
BOOL RDEManager::WriteNewPage (const CString& sLayout)
{
	return m_pOutFile ? m_pOutFile->RdeWriteNewPageCommand(sLayout) : FALSE;
}

//-----------------------------------------------------------------------------
BOOL RDEManager::WriteInitialSpecialField()
{
	return m_pOutFile ? m_pOutFile->RdeWriteInitialSpecialField() : FALSE;
}

//-----------------------------------------------------------------------------
RDEManager::RDEManager (ReportEngine* pEngine)
	:
	m_hWndComunication	(NULL),
	m_strDataFileName	(),
	m_bTemporary		(FALSE),
	m_pOutFile			(NULL),
	m_pInpFile			(NULL),
	m_nCurrentPageRead	(-1),
	m_PageSeekPoints	(),
	m_MngrStatus		(FILE_CLOSED),
	m_strPlaybackName	(),
	m_bCanSaveRde		(FALSE),
	m_nTheIdToFind		(SpecialReportField::NO_INTERNAL_ID),
	m_strStringToFind	(),
	m_SearchDirection	(FORWARD),
	m_CaseSensitivity	(IGNORE_CASE),
	m_MatchMode			(EXACTLY),
	m_pEngine			(pEngine)
{
	ASSERT(pEngine);
}

//-----------------------------------------------------------------------------
RDEManager::~RDEManager ()
{
	Close();
	if (!m_strPlaybackName.IsEmpty())
		Remove(m_strPlaybackName);
}

//-----------------------------------------------------------------------------
BOOL RDEManager::Create()
{
	if (m_MngrStatus != RDEManager::FILE_CLOSED)
		Close();

	return Open(GetTempName(szRdeExt), CFile::modeCreate, TRUE);
}


//-----------------------------------------------------------------------------
BOOL RDEManager::Open (LPCTSTR pszRdeFileName, UINT nMode, BOOL bTemporary)
{
	if (m_MngrStatus != RDEManager::FILE_CLOSED)  Close();

	//  Mandatory in this order
	//
	m_bCanSaveRde = FALSE;
	m_PageSeekPoints.RemoveAll();

	m_strDataFileName = pszRdeFileName;
	if (m_strDataFileName.IsEmpty())
	{
		MessageError(RDEManager::ErrorMessages::FILENAME_NOT_VALID(), NULL, NULL);
		return FALSE;
	}
	
	if (nMode != CFile::modeRead)
	{
		m_pOutFile = new RDEFileWriter(*this);
		if (!m_pOutFile->RdeOpenOutput(m_strDataFileName))
		{
			delete m_pOutFile;
			m_pOutFile = NULL;
			return FALSE;
		}
		// if exists a writer process the lock is at first byte of file
		m_dwLockPosition = 0;
	}

	if (nMode != CFile::modeWrite)
	{
		m_pInpFile = new RDEFileReader(*this); 
		if (!m_pInpFile->RdeOpenInput(m_strDataFileName) ||
			((nMode == CFile::modeRead) && !m_pInpFile->GetInfo()))
		{
			m_PageSeekPoints.RemoveAll();
			delete m_pInpFile;
			m_pInpFile = NULL;
			return FALSE;
		}
	}

	// Must be used for not linked client/server processes
	// if (m_pOutFile) m_pOutFile->LockRange(m_dwLockPosition, 1);

	m_nCurrentPageRead = -1;          
	
	// it musts be here
	m_bTemporary   = bTemporary;

	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL RDEManager::Open (LPCTSTR pszRdeFileName, CString& strPlaybackName)
{                                      
	if (!Open(pszRdeFileName)) return FALSE;
										   
	// return the splitted file containing the WRM
	strPlaybackName = m_strPlaybackName;
	
	return TRUE;
}

//-----------------------------------------------------------------------------
void RDEManager::Close ()
{
	if (m_MngrStatus != RDEManager::FILE_CLOSED)
	{
		if (m_pOutFile)
		{
			if (!m_bTemporary)
				m_pOutFile->SetInfo(NULL);
				
			m_pOutFile->Close();
			delete m_pOutFile;
			m_pOutFile = NULL;
		}

		if (m_pInpFile)
		{              
			m_pInpFile->Close();
			delete m_pInpFile;
			m_pInpFile = NULL;
		}

		m_MngrStatus = RDEManager::FILE_CLOSED;

		// Next line musts be uncommented for not linked client/server processes
		//  if (m_pOutFile) m_pOutFile->UnlockRange(m_dwLockPosition, 1);

		// remove RDE file if it's a temporary file
		if (m_bTemporary)
			Remove(m_strDataFileName);
	}
}

//-----------------------------------------------------------------------------
BOOL RDEManager::SaveRDEFile (LPCTSTR pszAsName)
{                             
	if (m_MngrStatus == RDEManager::FILE_CLOSED) return FALSE;

	if (!m_strPlaybackName.IsEmpty()) return FALSE;	// we are in playback mode
	
	if (m_strDataFileName.CompareNoCase(pszAsName) == 0)	return FALSE;

	if (!m_bCanSaveRde)
	{
		MessageError(RDEManager::ErrorMessages::FILE_NOT_VALID(), NULL, NULL);
		return FALSE;
	}
						 
	if (!pszAsName || !*pszAsName) return FALSE;
					
	CString strTmpWrmName(MakeName(pszAsName, _T("$$$")));
	
	if (ExistFile(strTmpWrmName) && !Remove(strTmpWrmName)) return FALSE;

	if (_trename(pszAsName, strTmpWrmName) != 0)
	{
		MessageError(RDEManager::ErrorMessages::CANT_RENAME_FILE(), NULL, pszAsName);
		return FALSE;
	}

	RDEFileWriter RdeFile(*this);
	
	if (!RdeFile.RdeOpenOutput(pszAsName)) return FALSE;
		
	BOOL	bInpFileTemp	= (m_pInpFile == NULL);
	DWORD	dwCurPos		= 0;
	
	if (bInpFileTemp)
	{                        
		// RDE file opened in write only mode
		//
		m_pInpFile = new RDEFileReader(*this); 
		if (!m_pInpFile->RdeOpenInput(m_strDataFileName))
		{
			delete m_pInpFile;
			m_pInpFile = NULL;
			return FALSE;
		}
	}
	else
	{
		dwCurPos = (DWORD) m_pInpFile->GetPosition();
		if (!m_pInpFile->TrySeek(0L))
			return FALSE;
	}
	
	AfxGetApp()->m_pMainWnd->BeginWaitCursor ();
	AfxGetApp()->m_pMainWnd->SendMessage(UM_PARSE_BEGIN);
	
	LONG nSize;

	// if RDE file was opened in read only mode the size is the page information offset
	//
	if (!m_pOutFile)
		nSize = m_Tail.m_dwPageInfoOffs;
	else                           
		nSize = (DWORD)m_pOutFile->GetLength();
	
	UINT nBufSize = (UINT) (nSize < RDE_BUFFER_SIZE ? nSize : RDE_BUFFER_SIZE);
	unsigned char* pchBuf = new unsigned char[nBufSize];
						 
	LONG totalBytes	= nSize;
	BOOL ok			= TRUE;
				
	while (ok && nSize)
	{

		ok = m_pInpFile->TryRead(pchBuf, nBufSize) && RdeFile.TryWrite(pchBuf, nBufSize);
						
		if (ok)
		{
			nSize -= nBufSize;
			
			if (nSize < (LONG)nBufSize)
				nBufSize = (UINT)nSize;
		}

		WORD wPerc = (WORD) ((RdeFile.GetLength() * 100L) / totalBytes);

		AfxGetApp()->m_pMainWnd->SendMessage(UM_PARSE_TIC, (WPARAM) wPerc);
			
	}
				
	delete [] pchBuf;     

	ok = ok && RdeFile.SetInfo(strTmpWrmName) && Remove(strTmpWrmName);

	RdeFile.Close();
		
	if (bInpFileTemp)                                                                 
	{
		m_pInpFile->Close();
		delete m_pInpFile;
		m_pInpFile = NULL;
	}
	else
		ok = m_pInpFile->TrySeek(dwCurPos);

	AfxGetApp()->m_pMainWnd->SendMessage(UM_PARSE_END);
	AfxGetApp()->m_pMainWnd->EndWaitCursor ();
	
	return ok;
}
			
//-----------------------------------------------------------------------------
BOOL RDEManager::SeekPage (int nCount, CFile::SeekPosition From)
{
	// after a bed seek it can retry a new seek
	if (m_MngrStatus == RDEManager::BAD_SEEK)
		m_MngrStatus = RDEManager::NO_ERROR_;
	else
		if (m_MngrStatus != RDEManager::NO_ERROR_) 
			return FALSE;

	int nNewPage;

	switch (From)
	{
		case CFile::begin   : nNewPage = nCount; break;
		case CFile::current : nNewPage = m_nCurrentPageRead + nCount; break;
		case CFile::end     : nNewPage = m_PageSeekPoints.GetUpperBound() - nCount; break;
		default             : nNewPage = -1; break;
	}
	 
	if ((nNewPage < 0) || (nNewPage > m_PageSeekPoints.GetUpperBound()))
	{
		m_MngrStatus = RDEManager::BAD_SEEK;
		MessageError(RDEManager::ErrorMessages::CANT_SEEK_FILE(), NULL, NULL);
		return FALSE;
	}

	if (m_pInpFile && m_pInpFile->RdeSeek(m_PageSeekPoints[nNewPage]))
	{
		m_nCurrentPageRead = nNewPage;
		return TRUE;
	}

	return FALSE;
}

//-----------------------------------------------------------------------------
int RDEManager::Search
				(
					WORD                    wId,
					LPCTSTR            		pszString,
					RDEManager::Direction   aDirection,
					RDEManager::Origin      aOrigin,
					RDEManager::Case        aCaseSens,
					RDEManager::Match       aMatchMode
			   )
{
	if (m_pInpFile && (m_MngrStatus != RDEManager::NO_ERROR_)) return -1;

	switch (aMatchMode)
	{
		case RDEManager::EXACTLY :
		case RDEManager::CONTAINED : m_MatchMode = aMatchMode; break;
		default : return -1;
	}

	m_nTheIdToFind		= wId;
	m_strStringToFind	= pszString;

	switch (aCaseSens)
	{
		case RDEManager::IGNORE_CASE	:
		case RDEManager::CASE_SENSITIVE	: m_CaseSensitivity = aCaseSens; break;
		
		default : return -1;
	}

	BOOL    ok				= FALSE;
	long    nActualPosition	= (DWORD)m_pInpFile->GetPosition();
	int     nActualPage		= m_nCurrentPageRead;

	switch (aDirection)
	{
		case RDEManager::FORWARD :
		{
			switch (aOrigin)
			{
				case RDEManager::FROM_CURRENT : ok = SeekNextPage(); break;
				case RDEManager::ENTIRE_SCOPE : ok = SeekFirstPage(); break;
				default : return -1;
			}

			m_SearchDirection = FORWARD;
			break;
		}
		case RDEManager::BACKWARD :
		{
			switch (aOrigin)
			{
				case RDEManager::FROM_CURRENT : ok = SeekPrevPage(); break;
				case RDEManager::ENTIRE_SCOPE : ok = SeekLastPage(); break;
				default : return -1;
			}

			m_SearchDirection = BACKWARD;
			break;
		}
		default : return -1;
	}

	if (ok)
		return Search(nActualPosition, nActualPage);
	else
	{
		// if first seek is failed reset position to actual position
		m_pInpFile->RdeSeek(nActualPosition);
		m_nCurrentPageRead = nActualPage;
		return -1;
	}
}

//-----------------------------------------------------------------------------
int RDEManager::SearchNext()
{
	if (!m_pInpFile || m_MngrStatus != RDEManager::NO_ERROR_)
		return -1;

	return Search((DWORD)m_pInpFile->GetPosition(), m_nCurrentPageRead);
}

//-----------------------------------------------------------------------------
int RDEManager::Search(long nActualPosition, int nActualPage)
{
	int     nNewPage	= -1;
	BOOL    ok			= TRUE;
	RDEData DummyRDEData;

	CString strSearchedPattern(m_strStringToFind);

	if (m_CaseSensitivity == RDEManager::IGNORE_CASE) strSearchedPattern.MakeUpper();
	
	while (ok && !Eof())
		switch (LookAhead())
		{
			case RDEManager::INTERNAL_ID :
			{
				if (!GetDataValue(DummyRDEData))
					ok = FALSE;
				else
					if (m_nTheIdToFind == GetId())
					{
						// if this data is not a string (null terminated) the result is
						// inpredicible
						TCHAR* pszDataValue = (TCHAR*) DummyRDEData.GetData();
						if (m_CaseSensitivity == RDEManager::IGNORE_CASE) 
						{
							_tcsupr_s(pszDataValue, _tcslen(pszDataValue) + 1);
						}

						if (((m_MatchMode == RDEManager::EXACTLY)     &&
							 (_tcscmp(pszDataValue, strSearchedPattern) == 0)) ||
							((m_MatchMode == RDEManager::CONTAINED)   &&
							 (_tcsstr(pszDataValue, strSearchedPattern) != NULL)))
							{
								nNewPage = m_nCurrentPageRead;
								ok       = FALSE;
							}
					}

				break;
			}
			case RDEManager::ID_COMMAND :
			{
				ok = SkipData();        // go ahead

				break;
			}
			case RDEManager::GEN_COMMAND :
			{
				if (GetCommand() == RDEManager::NEW_PAGE)
					ok = SeekPage(m_SearchDirection == FORWARD ? 1 : -1, CFile::current);
				else
					ok = SkipData();

				break;
			}
			default :
			{
				ok = FALSE;

				break;
			}
		}

	if ((nNewPage >= 0) && !SeekPage(nNewPage)) nNewPage = -1;

	if (nNewPage == -1)
	{
		m_pInpFile->RdeSeek(nActualPosition);
		m_nCurrentPageRead = nActualPage;
	}

	return nNewPage;
}

//-----------------------------------------------------------------------------
int RDEManager::MessageError(const CString& str, CFileException* e, LPCTSTR pszFileName, UINT nIdp, UINT nIcon)
{           
	LPCTSTR pszMess;
	if (pszFileName && *pszFileName)
		pszMess = pszFileName;
	else
		pszMess = m_strDataFileName;
	
	LPCTSTR pszPCause = _T("");
	if (e)	pszPCause = PCause(e);
	
	return AfxMessageBox(cwsprintf(str, pszPCause, pszMess), MB_OK | nIcon, nIdp);
}                                                                                

//-----------------------------------------------------------------------------
BOOL RDEManager::Remove(LPCTSTR pszFileName)
{
	TRY
	{
		CFile::Remove(pszFileName);
	}
	CATCH(CFileException, e)
	{
		MessageError(RDEManager::ErrorMessages::CANT_DELETE_FILE(), e, pszFileName, 0, MB_ICONINFORMATION);
		return FALSE;
	}
	END_CATCH                               
	
	return TRUE;
}

