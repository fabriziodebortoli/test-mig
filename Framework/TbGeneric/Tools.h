#pragma once

// evita l'errore di compilazione in comdef.h
// bug di Microsoft: IXMLElementNotificationSink non piu' definita in MSXML3.DLL
#define _COM_NO_STANDARD_GUIDS_
#include <comdef.h>
#undef _COM_NO_STANDARD_GUIDS_

//includere alla fine degli include del .H
#include "beginh.dex"

// -----------------------------------------------------------------------
// Apre un file temporaneo
TB_EXPORT BOOL AfxOpenTmpFile (CFile&, CString&, LPCTSTR, LPCTSTR);

// carica dalle risorse un cursore animato (.ANI)
TB_EXPORT HCURSOR AfxLoadAnimCursor (UINT nCursorResId);

//-------------------------------------------------------------------------------
// converte l'indice passato (n) nella corrispondente label di colonna di Excel
TB_EXPORT CString IndexToExcelLabel (int n);

//-------------------------------------------------------------------------------
// Verifica esistenza in esecuzione di una finestra di livello top con il title specificato
TB_EXPORT HWND ThereIsTopWindowWithTitle (LPCTSTR pszTitle, LPCTSTR pszSkipFalseParent = NULL);

//-------------------------------------------------------------------------------
// Verifica esistenza servizi DCOM 
TB_EXPORT BOOL IsDCOMInstalled();

//==============================================================================
//  Dichiarazione Classe CEnhMetaFile
//					 
//==============================================================================

class TB_EXPORT CEnhMetaFile: public CObject
{
	DECLARE_DYNAMIC(CEnhMetaFile)

protected:
	CString			m_strFileName;
	HENHMETAFILE	m_hemfMetaFile;

public:
	CEnhMetaFile	();
	CEnhMetaFile	( const CString& );
	~CEnhMetaFile	();

	HENHMETAFILE	LoadFromFile	( const CString& );
	BOOL			Play			( CDC&, const CRect& rectInside );

};

/////////////////////////////////////////////////////////////////////////////
#include "endh.dex"
