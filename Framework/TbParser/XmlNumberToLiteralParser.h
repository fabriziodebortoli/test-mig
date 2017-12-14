#pragma once

#include "XmlBaseDescriptionParser.h"

//includere alla fine degli include del .H
#include "beginh.dex"
//============================================================================
class CNumberToLiteralLookUpTableManager;

class TB_EXPORT CXmlNumberToLiteralParser : public CObject
{
	DECLARE_DYNAMIC(CXmlNumberToLiteralParser)

private:
	CNumberToLiteralLookUpTableManager* m_pLUM;

public:
	CXmlNumberToLiteralParser() {}
	~CXmlNumberToLiteralParser() {}

public:
	BOOL LoadLookUpFile		();
protected:
	BOOL CheckLookUpFile	(CString					strFile,
							 CLocalizableXMLDocument&	aDocument
							 );

	BOOL NameEntryes		(CLocalizableXMLDocument&	aDocument);

	BOOL ParseTagNameEntry	(CXMLNode*					pTagNode);

	BOOL ParseTagForThousands(CXMLNode*						pTagNode,
							  CNumberToLiteralLookUpTable*	p_LU);

	BOOL ParseTagForMillions(CXMLNode*						pTagNode,
							 CNumberToLiteralLookUpTable*	p_LU);

	BOOL LoadGroups			(CLocalizableXMLDocument&	aDocument);

	BOOL LoadParameters		(CLocalizableXMLDocument&	aDocument);

	BOOL LoadSeparatorExceptions	(CXMLNode*		aNode);

	BOOL LoadDeclinations	(CNumberToLiteralLookUpTableManager::DeclinationType	eDecType,
							 CXMLNode*												aNode
							 );

	BOOL LoadDeclinationExceptions
							(CNumberToLiteralLookUpTableManager::DeclinationType	eDecType,
							 int													nDecValue,
							 CXMLNode*												aNode
							 );

};


//============================================================================
#include "endh.dex"
