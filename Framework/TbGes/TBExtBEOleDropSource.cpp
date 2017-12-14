#include "StdAfx.h"

#include "TBExtBEOleDropSource.h"

//-----------------------------------------------------------------------------
///////////////////////////////////////////////////////////////////////////////
//	CMyOleDropSource
//-----------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(CTBExtBEOleDropSource, CCmdTarget)

//-----------------------------------------------------------------------------
CTBExtBEOleDropSource::CTBExtBEOleDropSource(CBodyEdit*	pSource)
	:
	m_pSource(pSource)
{
	ASSERT(m_pSource);
}

//-----------------------------------------------------------------------------
SCODE CTBExtBEOleDropSource::GiveFeedback( DROPEFFECT dropEffect )
{
	return DRAGDROP_S_USEDEFAULTCURSORS;
}
