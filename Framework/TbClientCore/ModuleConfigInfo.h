#pragma once

#include <TbXmlCore\XmlSaxReader.h>

#include "beginh.dex"

class CTBNamespaceArray;
class CXMLNode;
//=============================================================================
class TB_EXPORT CModuleConfigLibraryInfo: public CObject
{
	friend class CModuleConfigContent;
	friend class CModuleConfigInfo;

private:
	CString		m_sName;
	CString		m_sDeployPolicy;
	CString		m_sSourceFolder;
	BOOL		m_bAssembly;

public:
	CModuleConfigLibraryInfo	();

public:
	void	Init			();

	const CString&	GetLibraryName	() const { return m_sName; }
	const CString&	GetDeployPolicy	() const { return m_sDeployPolicy; }
	const CString&	GetSourceFolder	() const { return m_sSourceFolder; }
	const CString	GetAlias	() const;
	const BOOL&		IsAssembly	() const;
};

// Classe per la gestione di un' array di LibraryInfo
//=============================================================================
class TB_EXPORT CModuleConfigLibrariesInfo : public CObArray
{
public:
	virtual ~CModuleConfigLibrariesInfo();

public:
	void	RemoveAll();

	CModuleConfigLibraryInfo* 	GetAt		(int nIndex)const	{ return (CModuleConfigLibraryInfo*) CObArray::GetAt(nIndex);	}
	CModuleConfigLibraryInfo*&	ElementAt	(int nIndex)		{ return (CModuleConfigLibraryInfo*&) CObArray::ElementAt(nIndex); }
	
	CModuleConfigLibraryInfo* 	operator[]	(int nIndex)const	{ return GetAt(nIndex);	}
	CModuleConfigLibraryInfo*&  operator[]	(int nIndex)		{ return ElementAt(nIndex);	}
};

class CModuleConfigInfo;
//=============================================================================
class TB_EXPORT CModuleConfigContent: public CXMLSaxContent
{
	DECLARE_DYNAMIC (CModuleConfigContent)

private:
	CModuleConfigInfo* m_pConfigInfo;

public:
	CModuleConfigContent (CModuleConfigInfo* pConfigInfo);

protected:
	virtual CString	OnGetRootTag		() const;
	virtual void	OnBindParseFunctions();

private:
	int		ParseModuleInfo		(const CString& sUri, const CXMLSaxContentAttributes& arAttributes);
	int		ParseDLL			(const CString& sUri, const CXMLSaxContentAttributes& arAttributes);
	int		ParseAssembly		(const CString& sUri, const CXMLSaxContentAttributes& arAttributes);
	BOOL	IsOkLibraryInfo		(CModuleConfigLibraryInfo* pInfo);

	int		ParseLibrary		(BOOL bIsAssembly, const CXMLSaxContentAttributes& arAttributes);
};

//=============================================================================
class TB_EXPORT CModuleConfigInfo: public CObject
{
	friend class CModuleConfigContent;

private:
	CTBNamespace				m_Namespace;
	CString						m_sName;
	CString						m_sNotLocalizedTitle;
	CModuleConfigLibrariesInfo	m_Libraries;	
	CString						m_sLibrarieNames;

public:
	CModuleConfigInfo	();
	~CModuleConfigInfo	();

public:
	static const TCHAR	GetNamesSeparator	();

public:
	void Init ();

	const CTBNamespace&	GetNamespace		() const { return m_Namespace; }
	const CString&		GetName				() const { return m_sName; }
	const CString		GetTitle			() const;
	const CString&		GetNotLocalizedTitle() const { return m_sNotLocalizedTitle; }
	void				SetNotLocalizedTitle(const CString&	sTitle) { m_sNotLocalizedTitle = sTitle; }
	const BOOL			HasLibraries		() const;

	const CString						GetLibrariesNames	() const;
	CStringArray*						GetLibraries		() const;
	CStringArray*						GetLibrariesAliases	() const;
	const CModuleConfigLibrariesInfo&	GetLibrariesInfo	() const { return m_Libraries; }

	// policies
	CString			GetDeployPolicyOf	(const CString& sLibraryAlias)   const;
	BOOL			IsFullDeployPolicy	(const CString& sLibraryAlias)   const;
	BOOL			IsAddOnDeployPolicy	(const CString& sLibraryAlias)   const;
	BOOL			IsBaseDeployPolicy	(const CString& sLibraryAlias)   const;
	BOOL			HasLibrary			(const CString& sLibraryAlias)	const;
	CString			ResolveLibrary		(const CTBNamespace& nsLibrary) const;
};

#include "endh.dex"
