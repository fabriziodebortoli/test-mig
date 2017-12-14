#pragma once

#include <afxole.h>

//includere alla fine degli include del .H
#include "beginh.dex"

///////////////////////////////////////////////////////////////////////////////
//	CTBExtBEOleDataSource
//-----------------------------------------------------------------------------
// Non ancora gestito correttamente...
//#define USA_RTF

class  TB_EXPORT CTBExtBEOleDataSource : public COleDataSource
{
protected:
	CBodyEdit*	 const	m_pSource;

	CLIPFORMAT	m_nFormatBE;
	CLIPFORMAT	m_nFormatBESelf;

public:
	CTBExtBEOleDataSource(CBodyEdit* pSource);

	virtual	BOOL		OnRenderGlobalData	(LPFORMATETC	lpFormatEtc, HGLOBAL*	phGlobal);
	virtual	DROPEFFECT	DoDragDrop			(DWORD dwEffects = DROPEFFECT_COPY|DROPEFFECT_MOVE|DROPEFFECT_LINK, LPCRECT lpRectStartDrag = NULL, COleDropSource* pDropSource = NULL );
	virtual	void		CopyToClipboard		();

protected:
	void	RenderText		(COleDataSource*	pOleDataSource, HGLOBAL*	phGlobal = NULL);
	void	RenderCoDec		(COleDataSource*	pOleDataSource, HGLOBAL*	phGlobal = NULL);
	void	RenderBE		(COleDataSource*	pOleDataSource, HGLOBAL*	phGlobal = NULL);

protected:
	//static CTime	m_Time;
	static const CString s_KeyFormat;
public:
	static CString		GetAppKey			();
	static CLIPFORMAT	GetBodyCF			(CBodyEdit* pSource = NULL);
};

///////////////////////////////////////////////////////////////////////////////
//	::GetTBExtBodyCF
//-----------------------------------------------------------------------------
//inline CLIPFORMAT	GetTBExtBodyCF(CBodyEdit* pSource = NULL)
//{
//	return GetBodyCF(pSource);
//}


//=============================================================================
#include "endh.dex"
