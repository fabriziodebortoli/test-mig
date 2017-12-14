#pragma once

#include <TBXMLCore\XMLParser.h>
#include <TBGeneric\array.h>
#include <TBGeneric\dataobj.h>
#include <TBNameSolver\TbNameSpaces.h>

//includere alla fine degli include del .H
#include "beginh.dex"


//----------------------------------------------------------------
//class CXMLEnvFile
//----------------------------------------------------------------
class TB_EXPORT CXMLEnvFile : public CObject
{
public:
	enum ContentFileType	{ SCHEMA_FILE, ROOT_FILE, NEXT_ROOT_FILE, XREF_FILE, ENV_FILE, UNDEF_FILE }; 

public:
	ContentFileType		m_eFileType;
	CString				m_strUrlData;
	CString				m_strEnvClass;		//envelope class di appartenenza del doc esportato nel file
	CString				m_strDocumentName;	//nome del documento esportato
	int					m_nDocumentNumb;	//numero di documenti esportati contenuti nel file


public:
	CXMLEnvFile();
	CXMLEnvFile(ContentFileType, LPCTSTR, LPCTSTR = NULL, LPCTSTR = NULL, LPCTSTR = NULL, int = 0);
	CXMLEnvFile(const CXMLEnvFile& aXMLEnvFile);

public:
	BOOL				Parse(CXMLNode*);
	BOOL				Unparse(CXMLNode*);

public:	
	CString				GetFormatDocNumb() const;
	void				SetDocNumb		(const CString&);	

	CString				GetStrFileType	() const;
	void				SetType			(const CString&);

	BOOL				IsRootFile		()	const	{ return (m_eFileType == ROOT_FILE); }
	BOOL				IsNextRootFile	()	const	{ return (m_eFileType == NEXT_ROOT_FILE); }
	BOOL				IsExtRefFile	()	const	{ return (m_eFileType == XREF_FILE); }
	BOOL				IsEnvFile		()	const	{ return (m_eFileType == ENV_FILE);  }
	BOOL				IsDataFile		()	const	{ return IsRootFile() || IsNextRootFile() || IsExtRefFile(); }

public: //operator
	CXMLEnvFile&	operator =	(const CXMLEnvFile&);

};

//----------------------------------------------------------------
//class CXMLEnvContentsArray
//----------------------------------------------------------------
class TB_EXPORT CXMLEnvContentsArray : public Array
{
public:
	BOOL	m_bRootFilePresent; //se è già stato inserito il rootfile

public:
	CXMLEnvContentsArray() : m_bRootFilePresent(FALSE) {}

public:
	BOOL				Parse(CXMLNode*);
	BOOL				Unparse(CXMLNode*);

public:	
	CXMLEnvFile::ContentFileType	GetFileTypeAt		(int nIdx)	const;
	CString							GetUrlDataAt		(int nIdx)	const;
	CString							GetDocumentNameAt	(int nIdx)	const;

public:
	int					Add					(CXMLEnvFile* pEnvFile);

	CXMLEnvFile*		GetAt				(int nIdx)		const	{ return (CXMLEnvFile*)Array::GetAt (nIdx); }		
	
	CXMLEnvFile*		GetEnvFileByName		(LPCTSTR lpszFileName) const;
	void				IncrementExpRecordCount	(LPCTSTR lpszFileName,  int nDataInstancesNumb);

	CXMLEnvFile*		operator[]	(int nIndex)	const	{ return GetAt(nIndex);}
	CXMLEnvFile*&		operator[]	(int nIndex)			{ return (CXMLEnvFile*&) ElementAt(nIndex);}

};


//----------------------------------------------------------------
//class CEnvDocumentInfo
//----------------------------------------------------------------
class TB_EXPORT CXMLEnvDocumentInfo
{
public:
	CString				m_strDomainName;
	CString				m_strSiteName;
	CString				m_strSiteCode;
	CString				m_strUserName;
	DataDate			m_DataTime;
	CString				m_strEnvClass;
	CTBNamespace		m_nsRootDoc;	

public:
	CXMLEnvDocumentInfo();
	CXMLEnvDocumentInfo	(const CXMLEnvDocumentInfo&);

public:
	BOOL	Parse(CXMLNode*);
	BOOL	Unparse(CXMLNode*);

public:
	void	SetCurrentDataTime	();

public:
	CXMLEnvDocumentInfo& operator =(const CXMLEnvDocumentInfo&);
};


//----------------------------------------------------------------
//class CXMLEnvelopeInfo
//----------------------------------------------------------------
class TB_EXPORT CXMLEnvelopeInfo
{

public:
	CString					m_strExportID;
	CString					m_strDescription;
	CXMLEnvDocumentInfo		m_aEnvDocInfo;
	CXMLEnvContentsArray	m_aEnvContents;

public:
		
	BOOL Parse					(const CString&, CXMLDocumentObject* pXMLDoc /*= NULL*/);
	BOOL Unparse				(const CString&, BOOL bDisplayMsgBox = TRUE, BOOL bCreateSchema = FALSE);
	BOOL CreateEnvelopeSchema	(const CString&, BOOL bDisplayMsgBox = TRUE);
public:
	int		AddEnvFile
				(
					CXMLEnvFile::ContentFileType	eFileType, 
					LPCTSTR							lpszUrlDati, 
					LPCTSTR							lpszProfile  = NULL, 
					LPCTSTR							lpszEnvClass = NULL, 
					LPCTSTR							lpszDocName	 = NULL, 
					int								nDocNum		 = 1					
				); 
public:
	void	SetExportID		(const CString& strExpID)			{ m_strExportID = strExpID; }
	void	SetDescription	(const CString& strDescri)			{ m_strDescription = strDescri; }

	// per la sezione relativa a DocumentInfo
	void	SetDomainName		(const CString& strDomain)			{ m_aEnvDocInfo.m_strDomainName = strDomain; }
	void	SetSiteName			(const CString& strSite)			{ m_aEnvDocInfo.m_strSiteName = strSite; }
	void	SetSiteCode			(const CString& strSiteCode)		{ m_aEnvDocInfo.m_strSiteCode = strSiteCode; }
	void	SetUserName			(const CString& strUsr)				{ m_aEnvDocInfo.m_strUserName = strUsr; }
	void	SetCurrentDataTime	()									{ m_aEnvDocInfo.SetCurrentDataTime();}
	void	SetEnvClass			(const CString& strEnvClass)		{ m_aEnvDocInfo.m_strEnvClass = strEnvClass; }
	void	SetRootDocNameSpace	(const CTBNamespace& aNameSpace)	{ m_aEnvDocInfo.m_nsRootDoc = aNameSpace; }

	
		
public:
	void	GetExportID		(const CString& strExpID)			{ m_strExportID = strExpID; }
	void	GetDescription	(const CString& strDescri)			{ m_strDescription = strDescri; }


	const CString&	GetExportID		()	const 	{ return m_strExportID;}
	const CString&	GetDescription	()	const 	{ return m_strDescription; }	

	const CString&		GetDomainName	()	const 	{ return m_aEnvDocInfo.m_strDomainName;	}
	const CString&		GetSiteName		()	const 	{ return m_aEnvDocInfo.m_strSiteName;	}
	const CString&		GetUserName		()	const 	{ return m_aEnvDocInfo.m_strUserName;	}	
	const CString&		GetEnvClass		()	const   { return m_aEnvDocInfo.m_strEnvClass;	}
	const CTBNamespace&	GetRootNameSpace()	const   { return m_aEnvDocInfo.m_nsRootDoc;	}

	BOOL	IsRootFilePresent() const	{ return m_aEnvContents.m_bRootFilePresent; }

	CXMLEnvFile*					GetEnvFileAt		(int nIdx)	const	{ return m_aEnvContents.GetAt(nIdx); }
	CXMLEnvFile*					GetEnvFileByName	(LPCTSTR lpszFileName)	const	{ return m_aEnvContents.GetEnvFileByName(lpszFileName); }

	CXMLEnvFile::ContentFileType	GetFileTypeAt		(int nIdx)	const	{ return m_aEnvContents.GetFileTypeAt(nIdx); }
	CString							GetUrlDataAt		(int nIdx)	const	{ return m_aEnvContents.GetUrlDataAt(nIdx); }
	CString							GetDocumentNameAt	(int nIdx)	const	{ return m_aEnvContents.GetDocumentNameAt(nIdx); }

	void	IncrementExpRecordCount(LPCTSTR lpszFileName,  int nDataInstancesNumb) { m_aEnvContents.IncrementExpRecordCount(lpszFileName,  nDataInstancesNumb); }

public:
	CXMLEnvelopeInfo& operator =(const CXMLEnvelopeInfo&);

};



#include "endh.dex"
