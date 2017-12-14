#pragma once



#include "beginh.dex"

/////////////////////////////////////////////////////////////////////////////////
class CWoormDocMng;

class /*TB_EXPORT*/ RenderPdfSharp : public CObject
{
	CWoormDocMng* m_pDocument = NULL;

public:
	RenderPdfSharp(CWoormDocMng* pWDoc) : m_pDocument(pWDoc) {}
	RenderPdfSharp() {}

	void Render();
	
};


#include "endh.dex"
