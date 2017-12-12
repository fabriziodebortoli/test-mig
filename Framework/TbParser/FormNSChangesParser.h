#pragma once

#include "XmlBaseDescriptionParser.h"

//includere alla fine degli include del .H
#include "beginh.dex"
//============================================================================
class CFormNSChangeArray;
class CFormNSChange;

class TB_EXPORT CFormNSChangeParser : public CObject
{
	DECLARE_DYNAMIC(CFormNSChangeParser)

private:
	CFormNSChangeArray* p_mFNC;

public:
	CFormNSChangeParser() {}
	~CFormNSChangeParser() {}

public:
	BOOL LoadFormNSChange		(CFormNSChangeArray* pFormNSChangeArray,
								 int						dNsRelease,
								 const CTBNamespace&		nsDoc
								 );

	BOOL CheckFormNSChange		(CString					strFile,
								 CXMLDocumentObject&		aDocument
								 );

	BOOL LoadFormNsChanges		(CXMLDocumentObject& aDocument,
								 int dNsRelease
								 );

	BOOL ParseTagFormNsChange	(CXMLNode* pTagNode);
};


//============================================================================
#include "endh.dex"
