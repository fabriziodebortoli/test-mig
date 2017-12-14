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
	int			m_nWebServiceDriverPort;
	CString		m_sWebServiceDriverService;
	CString		m_sWebServiceDriverNamespace;

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


	const int&		GetWebServiceDriverPort		() const;
	const CString&	GetWebServiceDriverService	() const;
	const CString&	GetWebServiceDriverNamespace() const;

private:
	CString GetFileName () const;
};

#include "EndH.dex"
