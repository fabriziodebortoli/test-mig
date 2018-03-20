#pragma once

#include <TbXmlCore/XmlSaxReader.h>

#include "XmlBaseDescriptionParser.h"

//includere alla fine degli include del .H
#include "beginh.dex"

class CServerDocDescription;
class DocumentObjectsTable;
class CDocumentDescription;
class CViewModeDescription;

// lettura scrittura del file DocumentObjects.xml
//----------------------------------------------------------------
class TB_EXPORT CXMLDocumentObjectsParser : public CXMLBaseDescriptionParser
{
	DECLARE_DYNAMIC(CXMLDocumentObjectsParser)

public:
	CXMLDocumentObjectsParser();

public:
	BOOL LoadDocumentObjects	(const CTBNamespace& aNamespace, CPathFinder::PosType, DocumentObjectsTable* pTable);

private:
	BOOL Parse				(
								CXMLDocumentObject* pDoc, 
								DocumentObjectsTable* pTable, 
								const CTBNamespace& nsParent
							);
	BOOL ParseDocument		(
								CXMLNode*				pNode, 
								CDocumentDescription*	pDescri, 
								const CTBNamespace&		nsParent
							);
	BOOL ParseViewMode		(
								CXMLNode*				pNode, 
								CViewModeDescription*	pDescri, 
								const CTBNamespace&		nsParent
							);
};

// lettura del file ClientDocumentObjects.xml
//----------------------------------------------------------------
class TB_EXPORT CXMLClientDocumentObjectsContent: public CXMLSaxContent
{
	DECLARE_DYNAMIC(CXMLClientDocumentObjectsContent)

private:
	CTBNamespace			m_CurrentModule;
	CString					m_sCurrentClientFormName;
	CServerDocDescription*	m_pCurrentServerDescri = NULL;
	CClientDocDescription*	m_pCurrentClientDocDescri = NULL;
	CServerDocDescriArray*	m_pServerDocArray = NULL;
	CServerFormDescriArray*	m_pServerFormArray = NULL;
public:
	CXMLClientDocumentObjectsContent();

public:
	void SetCurrentModule (const CTBNamespace& aCurrentModule);

public:
	virtual CString OnGetRootTag		() const;
	virtual void	OnBindParseFunctions();

private:
	int	 ParseServerDocument (const CString& sUri, const CXMLSaxContentAttributes& arAttributes);
	int	 ParseClientDocument (const CString& sUri, const CXMLSaxContentAttributes& arAttributes);

	int	 ParseClientForm(const CString& sUri, const CXMLSaxContentAttributes& arAttributes);
	int	 ParseClientFormServer(const CString& sUri, const CXMLSaxContentAttributes& arAttributes);
	int	 ParseClientFormServerExclude(const CString& sUri, const CXMLSaxContentAttributes& arAttributes);
	int	 EndServerTag(const CString& sUri, const CString& aTagValue);

	int  ParseClientDocumentViewMode(const CString& sUri, const CXMLSaxContentAttributes& arAttributes);
	CServerDocDescriArray* GetServerDocDescriArray();
	CServerFormDescriArray* GetServerFormDescriArray();
};
void UpdateClientFormsSection();
#include "endh.dex"
