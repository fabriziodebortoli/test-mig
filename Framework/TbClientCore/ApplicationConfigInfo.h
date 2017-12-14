
#pragma once

#include <TbXmlCore\XmlSaxReader.h>

#include "beginh.dex"

class CApplicationConfigInfo;
//=============================================================================
class TB_EXPORT CApplicationConfigContent: public CXMLSaxContent
{
	DECLARE_DYNAMIC (CApplicationConfigContent)

private:
	CApplicationConfigInfo* m_pConfigInfo;

public:
	CApplicationConfigContent (CApplicationConfigInfo* pConfigInfo);

protected:
	virtual CString	OnGetRootTag			() const;
	virtual void	OnBindParseFunctions	();

private:
	int ParseType			(const CString& sUri, const CString& sTagValue);
	int ParseDbSignature	(const CString& sUri, const CString& sTagValue);
	int ParseVersion		(const CString& sUri, const CString& sTagValue);
};

// Definizione dei parametri dell'applicazione
//=============================================================================
class TB_EXPORT CApplicationConfigInfo: public CObject
{
	friend class CApplicationConfigContent;

private:
	CString				m_sDbSignature;
	CString				m_sName;
	CString				m_sVersion;
	BOOL				m_bTbApplication;

public:
	CApplicationConfigInfo	();
	~CApplicationConfigInfo	();

public:
	void Init ();

	const CString&		GetName			() const { return m_sName; }
	const CString&		GetDbSignature	() const { return m_sDbSignature; }		
	const CString&		GetVersion		() const { return m_sVersion; }		
	const BOOL&			IsTbApplication () const { return m_bTbApplication; }
};

class CLocalizableApplicationConfigInfo;
//=============================================================================
class TB_EXPORT CLocalizableApplicationConfigContent: public CXMLSaxContent
{
	DECLARE_DYNAMIC(CLocalizableApplicationConfigContent)

private:
	CLocalizableApplicationConfigInfo*	m_pConfigInfo;

public:
	CLocalizableApplicationConfigContent (CLocalizableApplicationConfigInfo* pConfigInfo);

protected:
	virtual CString	OnGetRootTag			() const;
	virtual void	OnBindParseFunctions	();

private:
	BOOL ParseTitle	(const CString& sUri, const CXMLSaxContentAttributes& arAttributes);
};

// versione dell'application.config che contiene i tag per la localizzazione e
// che è posizionato nel master module
//=============================================================================
class TB_EXPORT CLocalizableApplicationConfigInfo: public CObject
{
	friend class CLocalizableApplicationConfigContent;

private:
	CString				m_sNotLocalizedTitle;
	CTBNamespace		m_OwnerModule;

public:
	CLocalizableApplicationConfigInfo	();

public:
	const CTBNamespace&	GetOwnerModule		() const	{ return m_OwnerModule; }
	const CString		GetTitle			() const;
	const CString&		GetNotLocalizedTitle() const	{ return m_sNotLocalizedTitle; }

	void				SetNotLocalizedTitle(const CString&	sTitle) { m_sNotLocalizedTitle = sTitle; }
};

#include "endh.dex"
