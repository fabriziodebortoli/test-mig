#pragma once

#include <afxole.h>

//includere alla fine degli include del .H
#include "beginh.dex"

///////////////////////////////////////////////////////////////////////////////
//	CMyOleDropSource
//-----------------------------------------------------------------------------
class CBodyEdit;

class  TB_EXPORT CTBExtBEOleDropSource : public COleDropSource
{
	DECLARE_DYNAMIC(CTBExtBEOleDropSource)
protected:
	CBodyEdit*	 const	m_pSource;
public:
	CTBExtBEOleDropSource (CBodyEdit*	pSource);

	virtual SCODE				GiveFeedback	( DROPEFFECT dropEffect );

			CBodyEdit*		GetSourceBody() const {return m_pSource; }
};

//=============================================================================
#include "endh.dex"
