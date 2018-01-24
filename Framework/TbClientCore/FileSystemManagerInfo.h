#pragma once

#include <TbXmlCore\XmlSaxReader.h>

#include "BeginH.dex"

class CFileSystemManagerInfo;
//=============================================================================
class TB_EXPORT CFileSystemManagerContent : public CXMLSaxContent
{
	DECLARE_DYNAMIC (CFileSystemManagerContent)

private:
	CFileSystemManagerInfo* m_pConfigInfo;

public:
	CFileSystemManagerContent (CFileSystemManagerInfo* pConfigInfo);

protected:
	virtual CString	OnGetRootTag			() const;
	virtual void	OnBindParseFunctions	();

private:
	int ParseDriver					(const CString& sUri, const CXMLSaxContentAttributes& arAttributes);
	int ParseCaching				(const CString& sUri, const CXMLSaxContentAttributes& arAttributes);
	int ParsePerformanceCheck		(const CString& sUri, const CXMLSaxContentAttributes& arAttributes);
	int ParseWebServiceDriver		(const CString& sUri, const CXMLSaxContentAttributes& arAttributes);
	int	ParseFileSystemDriver		(const CString& sUri, const CXMLSaxContentAttributes& arAttributes);
	int ParserDatabaseDriverKey		(const CString& sUri, const CXMLSaxContentAttributes& arAttributes);
};

//=============================================================================
class TB_EXPORT CFileSystemManagerInfo : public CObject
{
	friend class CFileSystemManagerContent;

public:
	enum DriverType { FileSystem, WebService, Database };

private:
	DriverType	m_Driver;
	BOOL		m_bAutoDetectDriver;
	BOOL		m_bEnableCaching;
	BOOL		m_bEnablePerformanceCheck;
	//FileSystemDriver
	CString		m_sFSServerName;
	CString		m_sFSInstanceName;
	CString		m_sFSStandardPath;
	CString		m_sFSCustomPath;

	//WebServiceDriver
	int			m_nWebServiceDriverPort;
	CString		m_sWebServiceDriverService;
	CString		m_sWebServiceDriverNamespace;

	//DatabaseDriver
	CString		m_strStandardConnectionString;

public:
	CFileSystemManagerInfo();

public:
	const BOOL	LoadFile					();
	const BOOL	SaveFile					();

	DriverType	GetDriver					() const;
	void		SetDriver					(DriverType aDriverType); 

	BOOL		IsAutoDetectDriver			() const;
	BOOL		IsCachingEnabled			() const;
	BOOL		IsPerformanceCheckEnabled	() const;

	const CString&	GetFSServerName() const { return m_sFSServerName; }
	const CString&	GetFSInstanceName() const { return m_sFSInstanceName; } 
	const CString&	GetFSStandardPath() const { return m_sFSStandardPath; }
	const CString&	GetFSCustomPath() const { return m_sFSCustomPath; }

	const int&		GetWebServiceDriverPort() const;
	const CString&	GetWebServiceDriverService() const;
	const CString&	GetWebServiceDriverNamespace() const;

	const CString&	GetStandardConnectionString() const { return m_strStandardConnectionString; }
	
	

private:
	CString GetFileName () const;
};

#include "EndH.dex"
