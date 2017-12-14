#pragma once

#include <TbXmlCore\XmlSaxReader.h>
#include <TbNameSolver\TBNamespaces.h>
#include <TbGeneric\EnumsTable.h>

//includere alla fine degli include del .H
#include "beginh.dex"

static const int MAX_DEFAULT_DIM = 50;

// classe di parsing XML
//===========================================================================
class TB_EXPORT CXmlEnumsContent : public CXMLSaxContent
{
	DECLARE_DYNAMIC (CXmlEnumsContent)

private:
	CTBNamespace	m_CurrentModule;
	EnumsTable*		m_pEnumsTable;
	CString			m_sTagName;
	WORD			m_wTagValue;
	BOOL			m_bTagHidden;
	int				m_nDefaultsFound;
	WORD			m_arDefaultItemValues[MAX_DEFAULT_DIM];

public:
	CXmlEnumsContent();

public:
	void SetCurrentModule (const CTBNamespace& aCurrentModule);
	void AttachTable	  (EnumsTable* pTable);

public:
	virtual CString OnGetRootTag		() const;
	virtual void	OnBindParseFunctions();

private:
	int	 CheckTagEnd		(const CString& sUri, const CString& aTagValue);
	int	 ParseTag			(const CString& sUri, const CXMLSaxContentAttributes& arAttributes);
	int	 ParseDefaultValue	(const CString& sUri, const CXMLSaxContentAttributes& arAttributes);
	int	 ParseItem			(const CString& sUri, const CXMLSaxContentAttributes& arAttributes);
};

#include "endh.dex"
