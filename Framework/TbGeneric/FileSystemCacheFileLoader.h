#pragma once

#include <TbNameSolver\FileSystemCache.h>
#include <TbXmlCore\XmlSaxReader.h>
#include <TbGeneric\Schedule.h>

//includere alla fine degli include del .H
#include "beginh.dex"

class CXMLNode;
//==============================================================================
class  CFileSystemCacheContent : public CXMLSaxContent
{
	friend class CFileSystemCacheFileLoader;
	
	DECLARE_DYNAMIC (CFileSystemCacheContent)

	CString 			m_sContainer;
	CString 			m_sCurrentPath;
	CString 			m_sNameAttribute;
	CFileSystemCacher*	m_pCacher;
	BOOL				m_bFromSkipChilds;

public:
	CFileSystemCacheContent(CFileSystemCacher*	pCacher);
	~CFileSystemCacheContent();

private:
	virtual CString OnGetRootTag		() const;
	virtual int		OnStartDocument		();
	virtual int		OnStartElement		(
											const CString& sKey, 
											const CString& sUri, 
											const CXMLSaxContentAttributes& arAttributes
										);
	virtual int		OnEndElement		(
											const CString& sKey, 
											const CString& sUri, 
											const CString& sTagValue
										);
};

//==============================================================================
class CFileSystemCacheFileDialog : public CDialog
{
	friend class CFileSystemCacheFileLoader;

	DECLARE_DYNAMIC(CFileSystemCacheFileDialog)

public:
	CFileSystemCacheFileDialog ();

private:
	void	SetOperation (const CString& aOperation);

private:
	virtual void PostNcDestroy	();

protected:
	virtual BOOL OnInitDialog	();

	DECLARE_MESSAGE_MAP ();
};

//==============================================================================
class TB_EXPORT CFileSystemCacheFileLoader 
{
	friend class CFileSystemManagerWebService;
	friend class CFileSystemDriver;

private:
	CString						m_sLocalFileName;
	CString						m_sServerFileName;
	BOOL						m_bEnabled;
	CFileSystemCacheFileDialog* m_pWorkingDialog;

public:
	CFileSystemCacheFileLoader ();

	static const BOOL IsExcludedPath	(const CString& sRelativePath, const CString& sFolderName);
	
	const BOOL		RemoveFiles			(BOOL bLocalFile, BOOL bServerFile);

private:
	const BOOL		IsFileUpdated		() const;

	const CString&	GetLocalFileName	() const;
	const CString&	GetServerFileName	() const;

	const BOOL		SyncLocalFile		();
	const BOOL		SyncServerFile		();
	const BOOL		GenerateFile		();
	const BOOL		GenerateFile		(CXMLNode* pNode, const CString& sPath);
	const BOOL		Load				(CFileSystemCacher*	pCacher);
	const BOOL		Enable				(const BOOL& bEnable);
};

#include "endh.dex"
