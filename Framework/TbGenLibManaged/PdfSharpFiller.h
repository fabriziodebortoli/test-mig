#pragma once
//#include "stdafx.h"

#include "beginh.dex"

class CInternalPdfSharpDocumentWrapper;

//=============================================================================
class TB_EXPORT CPdfSharpFiller
{
	CMapStringToString					m_mapValues;
	CInternalPdfSharpDocumentWrapper*	m_pDocumentWrapper;

public:
	CPdfSharpFiller(void):  
		m_pDocumentWrapper(NULL)
	{}
	virtual ~CPdfSharpFiller(void) {}

	bool LoadTemplateFile	(CString sFilePath);		
	void SetValue			(CString sKey, CString sValue);
	bool SaveOutputFile		(CString sFilePath);
};

//=============================================================================
#include "endh.dex"