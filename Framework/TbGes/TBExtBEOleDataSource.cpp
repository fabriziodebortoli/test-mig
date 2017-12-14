#include "StdAfx.h"

#include <AfxAdv.h>

#include <tbges\dbt.h>
#include <tboledb\sqlrec.h>

#include "TBEDataCoDec.h"
#include "TBExtBEOleDataSource.h"

#include "BodyEdit.h"

#ifdef _DEBUG
#define new DEBUG_NEW
#endif

//-----------------------------------------------------------------------------
/*AFX_COMDAT	AFX_DATADEF	*/
extern TCHAR szCFSampleCoDec[];
//CTime CTBExtBEOleDataSource::s_Time = CTime::GetCurrentTime();
const CString CTBExtBEOleDataSource::s_KeyFormat = _T("{5E018C85-019A-48e0-86D0-93ADB7D457BA}");


//-----------------------------------------------------------------------------
CTBExtBEOleDataSource::CTBExtBEOleDataSource(CBodyEdit* pSource)
	: 
	m_pSource(pSource)
{
	ASSERT(m_pSource && m_pSource->IsKindOf(RUNTIME_CLASS(CBodyEdit)));
	TRACE1("CTBExtBEOleDataSource::CTBExtBEOleDataSource: start Drag operation from %p\n", m_pSource);

	m_nFormatBE		= GetBodyCF();
	m_nFormatBESelf	= GetBodyCF(m_pSource);

	//m_Time = CTime::GetCurrentTime();
}

//-----------------------------------------------------------------------------
// Nel caso di drag & drop (vedi Advanced Copy/Paste & Drag&Drop)
// Ordine di richieste CF per applicazioni:
// Excel: 1) CF_UNICODETEXT 2) CSV
// Word:  1) CF_RTF  2) CF_UNICODETEXT
// PPoint:1) CF_RTF  2) CF_UNICODETEXT

void	CTBExtBEOleDataSource::RenderText(COleDataSource*	pOleDataSource, HGLOBAL*	phGlobal )
{
	CSharedFile		sf(GHND);

	CString	text;
	TCHAR	separator = '\t';	// tab

	// intestazione
	for (int nColhead = 0; nColhead<= m_pSource->GetVisibleColumnsInfoUpperBound();/*TBEGetColInfo().GetUpperBound()*/ nColhead++)
	{
		ColumnInfo* pColumnInfo	= m_pSource->GetVisibleColumnFromIdx(nColhead);//>TBEGetColInfo().GetAt(nColhead);
		if (nColhead > 0) text += separator;
		text += pColumnInfo->GetTitle();

		ASSERT(pColumnInfo->GetParsedCtrl());	// viene usata x formattare l'output
	}

	for (int c = 0; c <= m_pSource->GetSelRowsUpperBound(); c++)
	{
		if (m_pSource->GetSelRowsStatus(c) == SelStatus::SELECTED)
		{
			if (!text.IsEmpty())
				text += _T("\r\n");

			SqlRecord*	pRec = m_pSource->GetDBT()->GetRow(c);
			CString		record_data;

			for (int nCol = 0; nCol <= m_pSource->GetVisibleColumnsInfoUpperBound()/*TBEGetColInfo().GetUpperBound()*/; nCol++)
			{
				ColumnInfo* pColumnInfo	= m_pSource->GetVisibleColumnFromIdx(nCol);//TBEGetColInfo().GetAt(nCol);
				if (!record_data.IsEmpty())
				{
					record_data += separator;
				}

				DataObj*	pDataObj = pRec->GetDataObjAt(pColumnInfo->GetDataInfoIdx());
				CString		strCell(pColumnInfo->GetParsedCtrl()->FormatData(pDataObj));

			    strCell.Remove('\r');
				strCell.Replace('\n',' ');

				record_data += strCell;
			}

			text += record_data;
		}
	}

	// carico il buffer per il trasferimento dati
	sf.Write(text, text.GetLength() * sizeof(TCHAR));

	HGLOBAL hMem = sf.Detach();
	if (!hMem)
		return	;

	BOOL	bDone = FALSE;
	if (!pOleDataSource && phGlobal)
	{
		*phGlobal = hMem;
		bDone = TRUE;
	}

	if (pOleDataSource && !phGlobal)
	{
		pOleDataSource->CacheGlobalData(CF_UNICODETEXT, hMem);
		bDone = TRUE;
	}

	ASSERT(bDone);
}

//-----------------------------------------------------------------------------
DROPEFFECT	CTBExtBEOleDataSource::DoDragDrop
						(
							DWORD			dwEffects		,
							LPCRECT			lpRectStartDrag ,
							COleDropSource* pDropSource
						)
{
//	RenderCoDec	(this);
//	RenderText	(this);

	RenderBE	(this);

	// render delayed x altri formati
	DelayRenderData(CF_UNICODETEXT);

	CTBEDataCoDec*		pDataCodec = m_pSource->GetDataCoDec();
	if (pDataCodec)
	{
		DelayRenderData(pDataCodec->GetClipFormat());
	}

	DelayRenderData(m_nFormatBESelf	);

	return COleDataSource::DoDragDrop(dwEffects, lpRectStartDrag, pDropSource);
}


//-----------------------------------------------------------------------------
void		CTBExtBEOleDataSource::CopyToClipboard		()
{
	COleDataSource*		pOleDataSource = new COleDataSource();

	pOleDataSource->FlushClipboard();

	RenderCoDec	(pOleDataSource);
	RenderText	(pOleDataSource);

	pOleDataSource->SetClipboard();
}

//-----------------------------------------------------------------------------
void		CTBExtBEOleDataSource::RenderCoDec(COleDataSource*	pOleDataSource, HGLOBAL*	phGlobal )
{
	CTBEDataCoDec*		pDataCodec = m_pSource->GetDataCoDec();

	if (!pDataCodec)
	{
		return	;
	}

	// render immediato per CoDec
	CSharedFile		sf(GHND);
	CArchive		ar(&sf, CArchive::store);

	pDataCodec->Encode(m_pSource);
	pDataCodec->Save(ar);

	ar.Close();

	HGLOBAL		hMem = sf.Detach();

	CLIPFORMAT	cf = pDataCodec->GetClipFormat();

	if (!hMem)
		return	;

	BOOL	bDone = FALSE;
	if (!pOleDataSource && phGlobal)
	{
		*phGlobal = hMem;
		bDone = TRUE;
	}

	if (pOleDataSource && !phGlobal)
	{
		pOleDataSource->CacheGlobalData(cf, hMem);
		bDone = TRUE;
	}

	ASSERT(bDone);
//	pOleDataSource->CacheGlobalData(cf, hMem);
}

//-----------------------------------------------------------------------------
void		CTBExtBEOleDataSource::RenderBE(COleDataSource*	pOleDataSource, HGLOBAL*	phGlobal)
{
	CSharedFile	sf(GHND);
	CString		address;
	address.Format(_T("%p"), m_pSource);
	sf.Write(address, address.GetLength() * sizeof(TCHAR));
	HGLOBAL		hMem = sf.Detach();

	if (!hMem)	return;

	pOleDataSource->CacheGlobalData(m_nFormatBE, hMem);
}

//-----------------------------------------------------------------------------
BOOL	CTBExtBEOleDataSource::OnRenderGlobalData	(
														LPFORMATETC	lpFormatEtc,
														HGLOBAL*	phGlobal
													)
{
	if (lpFormatEtc->cfFormat == m_nFormatBESelf)
	{
		return TRUE;
	}

	if (lpFormatEtc->cfFormat == CF_UNICODETEXT)
	{
		RenderText(NULL, phGlobal);
		return TRUE;
	}

	CTBEDataCoDec*		pDataCodec = m_pSource->GetDataCoDec();
	if (pDataCodec && lpFormatEtc->cfFormat == pDataCodec->GetClipFormat())
	{
		RenderCoDec(NULL, phGlobal);
		return TRUE;
	}
	return FALSE;
}

//-----------------------------------------------------------------------------
CString CTBExtBEOleDataSource::GetAppKey()
{
	//CString strKey;

	//strKey = cwsprintf(_T("%u::%s::%s"), GetCurrentProcessId(), m_Time.Format(_T("%Y%m%d")), m_Time.Format(_T("%H%M%S")));

	return s_KeyFormat;
}
//-----------------------------------------------------------------------------
CLIPFORMAT CTBExtBEOleDataSource::GetBodyCF(CBodyEdit* pSource /*= NULL*/)
{
	CString	name;
	if (pSource)
		name.Format(_T("TBECF::%s::%p"), GetAppKey(), pSource);
	else
		name.Format(_T("TBECF::%s"), GetAppKey());

	CLIPFORMAT cf = ::RegisterClipboardFormat(name/*szCFSampleCoDec*/);

	TRACE("CTBExtBEOleDataSource::GetBodyCF: name = %s %d\n", name, cf);

	return cf;
}



