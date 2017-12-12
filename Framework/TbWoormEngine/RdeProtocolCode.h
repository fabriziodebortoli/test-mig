#pragma once

#include <TbGeneric\DataObj.h>
#include <TbGeneric\RDEProtocol.h>

//includere alla fine degli include del .H
#include "beginh.dex"

//============================================================================
class RDEFile;
class RDEFileReader;
class RDEFileWriter;
class ReportEngine;
//=============================================================================
//RDE NULL ID 
//#define SpecialReportField::NO_INTERNAL_ID		((WORD) 0xFFFF)

//============================================================================
// must be unique in all projects                                 
#define ID_RDE_NEWPAGE		20001

//=============================================================================
struct HeaderRecordStruct
{     
	short		m_nTag;	//object ID (Alias)
	RDEcmd		m_Cmd;
	DataSize	m_Size;
};				


//=============================================================================
struct TailerFileStruct
{
	DWORD	m_dwPageInfoOffs;
	DWORD	m_dwWrmStartOffs;
	DWORD	m_dwWrmEndOffs;
};
		
//=============================================================================
class TB_EXPORT RDEManager : public IRDEManager
{
// friends area
	friend RDEFile;
	friend RDEFileWriter;
	friend RDEFileReader;

public:
	// constructos and distructor
	RDEManager		(ReportEngine*);
	~RDEManager		();

	// comunication attaching function
	void		AttachFrame		(CWnd* pWnd);
   	
	// generic fuction
	int			MessageError	(const CString& str, CFileException* pFE = NULL, LPCTSTR pszMsg = NULL, UINT nIdp = 0, UINT = MB_ICONSTOP);
	BOOL		Create			();
	BOOL		Open			(
									LPCTSTR	pszRDEFileName,
									UINT	nMode			= CFile::modeRead,
									BOOL	bTemporary		= FALSE
								);
	BOOL		Open			( LPCTSTR pszRDEFileName, CString& strPlaybackName );
	BOOL		Remove			( LPCTSTR pszRDEFileName );
	
	void		Close			();
	BOOL		SaveRDEFile		( LPCTSTR pszRDEFileName );
	RDEStatus	GetRDEStatus	();

	RDEDataType	LookAhead		();					
	WORD		GetId			();					
	RDEcmd		GetCommand		();				
	DataSize	GetParam		();	
			
	BOOL		GetDataValue			(RDEData&);
	BOOL		GetDataValueUnlimited	(RDEData&);

	BOOL		SkipData		();

	BOOL		IsColTotal		() const;
	BOOL		IsSubTotal		() const;

	int			Search			( WORD wId, LPCTSTR pszWhat, Direction = FORWARD,
								  Origin = ENTIRE_SCOPE, Case = CASE_SENSITIVE,
								  Match = EXACTLY );
	int			SearchNext		();
	CString		GetWhichString	() const;
	BOOL		SearchForward	() const;
	BOOL		SearchCaseSens	() const;
	BOOL		SearchExactly	() const;

	BOOL		SeekPage		( int, CFile::SeekPosition = CFile::begin );

	BOOL		SeekFirstPage	();					
	BOOL		SeekLastPage	();					
	BOOL		SeekCurrPage	();					
	BOOL		SeekNextPage	( int = 1 );	
	BOOL		SeekPrevPage	( int = 1 );	

	int			LastPage 		()	const;
	int			CurrPageRead	()	const;

	BOOL		IsLastPage		()	const;
	BOOL		IsFirstPage		()	const;
	BOOL		IsClosed		()	const;

	BOOL		Eop				(); 
	BOOL		Eod				(); 
	BOOL		Eof				();

	BOOL		Write			( short nID, RDEData&, BOOL bLimited =TRUE);
	BOOL		Write			( short nTag, RDEcmd, DataSize );
	BOOL		WriteNewPage	( const CString& sLayout );

	CString		GetDataFileName () { return m_strDataFileName; }

	BOOL		WriteInitialSpecialField ();

private:
	int			Search			( long, int );

public:
	HWND			m_hWndComunication;

private:
	CString			m_strDataFileName;
	BOOL			m_bTemporary;
	RDEFileWriter*	m_pOutFile;		// they're the same file as inpfile
	RDEFileReader*	m_pInpFile;
	int				m_nCurrentPageRead;
	CDWordArray		m_PageSeekPoints;
	RDEStatus		m_MngrStatus;
	DWORD			m_dwLockPosition;
	
	// for playback and save RDE management
	CString				m_strPlaybackName;
	TailerFileStruct	m_Tail;
	BOOL				m_bCanSaveRde;            
	
	// for search utilities
	WORD			m_nTheIdToFind;
	CString			m_strStringToFind;
	Direction		m_SearchDirection;
	Case			m_CaseSensitivity;
	Match          	m_MatchMode;
public:
	ReportEngine*		m_pEngine;
};

//==============================================================================
//			Class RDEFile definition
//==============================================================================
class TB_EXPORT RDEFile : public CFile
{
public:
	RDEFile (RDEManager&);

	BOOL RdeOpenFile	( const CString& strFileName, UINT nOpenFlags );

protected:
	CString				m_strRDEFileName;
	HeaderRecordStruct	m_Header;
	RDEManager&			m_TheOwner;
};


//==============================================================================
//			Class RDEFileWriter definition
//==============================================================================
class TB_EXPORT RDEFileWriter : public RDEFile
{
friend RDEManager;

public:
	RDEFileWriter ( RDEManager& );

	BOOL	RdeOpenOutput	( const CString& strFileName );

	BOOL	RdeWrite		( short, RDEData&, BOOL bLimited = TRUE);
	BOOL	RdeWrite		( short, RDEcmd, DataSize );
	BOOL	RdeWriteNewPageCommand	(const CString& sLayout);
	BOOL	RdeWriteInitialSpecialField ();

	BOOL	SetInfo			( LPCTSTR pszWrmFileName );

private:
	BOOL	ConcatWRMFile	( LPCTSTR pszWrmFileName, DWORD& dwWrmEndOffs );
	BOOL	TryWrite		( void*, UINT );
	BOOL	RdeWriteCommand	( short, RDEcmd, DataSize );
	void	MenageDiskFull	();

	BOOL	RdeWriteNewPageCommand	();

private:
	DWORD	m_dwBeginPagePosition;
	BOOL	m_bPostedNewPage;
	//CString	m_sLayoutName;
};

//==============================================================================
//			Class RDEFileReader definition
//==============================================================================
class TB_EXPORT RDEFileReader : public RDEFile
{
friend RDEManager;

public:
	RDEFileReader	( RDEManager& );
	~RDEFileReader	();

	BOOL	RdeOpenInput( const CString& strFileName );
	BOOL	RdeRead		( void*, DataSize );
	BOOL	RdeSeek		( DWORD );


	RDEManager::RDEDataType	LookAhead();

	WORD		GetId			();
	RDEcmd		GetCommand		();
	DataSize	GetParam		();

	BOOL		GetDataValue			(RDEData&);
	BOOL		GetDataValueUnlimited	(RDEData&);
	BOOL		SkipData		();

	BOOL		GetInfo			(BOOL split = TRUE);

private:
	BOOL	SplitWRMFile	( DWORD start, DWORD end );
	BOOL	TryRead			( void*, UINT );
	BOOL	TrySeek			( LONG, CFile::SeekPosition = CFile::begin );

private:
	BOOL	m_bGoAhead;

	// for buffered read management
	unsigned char*	m_pchBuffer;
	unsigned char*	m_pchCurrentByte;
	DWORD			m_dwReadStart;
	BOOL			m_bBuffered;
	UINT			m_nBytesNumber;
	UINT			m_nBufferSize;
};

#include "endh.dex"
