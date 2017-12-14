#pragma once

#include "XmlBaseDescriptionParser.h"

class CDataFileElement;

//includere alla fine degli include del .H
#include "beginh.dex"
//============================================================================
class CDataFileInfo;

class TB_EXPORT CXMLDataFileParser : public CObject
{
	DECLARE_DYNAMIC(CXMLDataFileParser)

private:
	CDataFileInfo* p_mDFI;

public:
	CXMLDataFileParser() {}
	~CXMLDataFileParser() {}

public:
	BOOL LoadDataFile		(CDataFileInfo*	pDataFileInfo);
	//il salvataggio avviene per i soli elementi aggiunti dall'utente. Il file è salvato nella CUSTOM/ALL_USER
	void SaveDataFile		(CDataFileInfo*	pDataFileInfo); 


private:
	BOOL ParseDataFile		(const CString& strFileName, BOOL bFromCustom);	
	void UnparseDataFile	(const CString& strFileName);

	BOOL LoadHeader			(CLocalizableXMLDocument&	aDocument);
	BOOL ParseTagFieldType	(CXMLNode* pTagNode);
	BOOL LoadElements		(CLocalizableXMLDocument& aDocument, BOOL bFromCustom);
	BOOL ParseTagElement	(CXMLNode* pTagNode, BOOL bFromCustom);
	BOOL ParseTagField		(CXMLNode*	pTagNode, CDataFileElement*	pElement);
	BOOL LoadParameters		(CLocalizableXMLDocument& aDocument);
	BOOL ParseTagFilterLike	(CXMLNode* pTagNode);

	void UnparseElements	(CXMLNode* pnRoot);		
	void UnparseHeader		(CXMLNode* pnRoot);

};


//============================================================================
#include "endh.dex"
