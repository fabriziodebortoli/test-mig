///////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------

#include "StdAfx.h"

#include "BodyEdit.h"
#include "TBExtBEDropTarget.h"
#include "TBExtBEOleDataSource.h"
#include "TBEDataCoDec.h"

/////////////////////////////////////////////////////////////////////////////
// CTBExtBEDropTarget
//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CTBExtBEDropTarget, CCmdTarget)
//-----------------------------------------------------------------------------
CTBExtBEDropTarget::CTBExtBEDropTarget()
{
	m_bDisableCF_TEXT		= FALSE;
	m_bDisableCF_SelfDrop	= FALSE;
	m_bDisableCF_CoDec		= FALSE;
	m_bDisableCF_Body		= FALSE;
	m_cfBody				= CTBExtBEOleDataSource::GetBodyCF();
}

//-----------------------------------------------------------------------------
void	CTBExtBEDropTarget::AttachTarget(CBodyEdit* pBody)
{
	m_pTarget = pBody;

	ASSERT(m_pTarget);
	Register(m_pTarget);
	m_cfBodySelf	= CTBExtBEOleDataSource::GetBodyCF(m_pTarget);
}

//------------------------------------------------------------------------------
DROPEFFECT	CTBExtBEDropTarget::OnDragEnter	( CWnd* pWnd, COleDataObject* pDataObject, DWORD dwKeyState, CPoint point )
{
	if (DocumentIsReadOnly())
		return DROPEFFECT_NONE;

	// i formati sono sempre tutti disponibili...
	// quindi e' importante l'ordine..

	if (!m_bDisableCF_SelfDrop && pDataObject->IsDataAvailable(m_cfBodySelf))
	{
		m_pTarget->StartDragging();
		return DROPEFFECT_MOVE;
	}

	if	(
			(!m_bDisableCF_CoDec	&&	m_pTarget->GetDataCoDec() && m_pTarget->GetDataCoDec()->AcceptDropFormat(pDataObject)) ||
			(!m_bDisableCF_Body		&&	pDataObject->IsDataAvailable(m_cfBody)) ||
			(!m_bDisableCF_TEXT		&&	pDataObject->IsDataAvailable(CF_UNICODETEXT))
		)
	{
		m_pTarget->StartDragging();
	}

	return DROPEFFECT_NONE;
}

//------------------------------------------------------------------------------
BOOL		CTBExtBEDropTarget::OnDrop		( CWnd* pWnd, COleDataObject* pDataObject, DROPEFFECT dropEffect, CPoint point )
{
	// Eventuale codec per gestire il drop del formato nella clpbrd
	CTBEDataCoDec*	pClpBrdDataCodec = NULL;
	ASSERT(!DocumentIsReadOnly());

	// memorizza la posizione di drop...
	m_pTarget->GetDragPosition(point, m_nDropRecordIdx, m_dstWhere);
	m_pTarget->StopDragging();

	if (!m_bDisableCF_SelfDrop && pDataObject->IsDataAvailable(m_cfBodySelf))
	{
		return TRUE;
	}

	CLIPFORMAT	cfCodec = 0;

	// controlla il tipo di drop... (CoDec)
	if (!m_bDisableCF_CoDec	&&	m_pTarget->GetDataCoDec() && m_pTarget->GetDataCoDec()->AcceptDropFormat(pDataObject, &cfCodec, pClpBrdDataCodec))
	{
		HGLOBAL	hMem = pDataObject->GetGlobalData(cfCodec);
		CMemFile	sf((BYTE*) ::GlobalLock(hMem), ::GlobalSize(hMem));
		CString		buffer;

		LPTSTR	str = buffer.GetBufferSetLength(::GlobalSize(hMem));
		sf.Read(str, ::GlobalSize(hMem));
		::GlobalUnlock(hMem);

		pClpBrdDataCodec->Load(str,cfCodec);

		OnDropCoDec(pClpBrdDataCodec);

		return TRUE;
	}


	// controlla il tipo di drop...
	if (!m_bDisableCF_Body && pDataObject->IsDataAvailable(m_cfBody))
	{
		CBodyEdit*		pSource = GetSourceBody(pDataObject);
		ASSERT(pSource);

		OnDropBody(pSource);
		return TRUE;
	}

	if (!m_bDisableCF_TEXT && pDataObject->IsDataAvailable(CF_UNICODETEXT))
	{
		HGLOBAL	hMem = pDataObject->GetGlobalData(CF_UNICODETEXT);
		CMemFile	sf((BYTE*) ::GlobalLock(hMem), ::GlobalSize(hMem));
		CString		buffer;

		LPTSTR	str = buffer.GetBufferSetLength(::GlobalSize(hMem));
		sf.Read(str, ::GlobalSize(hMem));
		::GlobalUnlock(hMem);

		OnDropText(str);
		return TRUE;
	}

	return FALSE;
}

//------------------------------------------------------------------------------
void CTBExtBEDropTarget::OnDragLeave(CWnd* pWnd)
{
	m_pTarget->StopDragging();
}

//-----------------------------------------------------------------------------
CBodyEdit* CTBExtBEDropTarget::GetSourceBody(COleDataObject* pDataObject) const
{
	ASSERT(pDataObject->IsDataAvailable(m_cfBody));
	CBodyEdit*	pSource = NULL;

	HGLOBAL	hMem = pDataObject->GetGlobalData(m_cfBody);
	CString		buffer;
	LPTSTR	str = buffer.GetBufferSetLength(::GlobalSize(hMem));

	CMemFile	sf((BYTE*) ::GlobalLock(hMem), ::GlobalSize(hMem));

	sf.Read(str, ::GlobalSize(hMem));
	::GlobalUnlock(hMem);

	if (!buffer.IsEmpty())
	{
		VERIFY(_stscanf_s(buffer, _T("%p"), &pSource) == 1);
		ASSERT(pSource->IsKindOf(RUNTIME_CLASS(CBodyEdit)));
		TRACE1("CTBExtBEDropTarget::GetSourceBody: the source CBodyEdit is %p\n", pSource);
	}

	return pSource;
}

//------------------------------------------------------------------------------
DROPEFFECT CTBExtBEDropTarget::OnDragOver(CWnd* pWnd, COleDataObject* pDataObject, DWORD dwKeyState, CPoint point )
{
	if (DocumentIsReadOnly())
		return DROPEFFECT_NONE;

	// attiva la maschera
	//TRACE2("CTBExtBEDropTarget::OnDragOver: BodyEdit (%d, %d)\n", point.x, point.y);
	ActivateMainFrame(pWnd);

	DROPEFFECT	de = DROPEFFECT_NONE;

	// evito il drop su se stesso...
	if (!m_bDisableCF_SelfDrop && pDataObject->IsDataAvailable(m_cfBodySelf))
	{
		de = DROPEFFECT_MOVE;
	}
	else if (!m_bDisableCF_CoDec && m_pTarget->GetDataCoDec() && m_pTarget->GetDataCoDec()->AcceptDropFormat(pDataObject))
	{
		de = DROPEFFECT_COPY;
	}
	else if (!m_bDisableCF_Body && pDataObject->IsDataAvailable(m_cfBody))
	{
		de = DROPEFFECT_COPY;
	}
	else if (!m_bDisableCF_TEXT && pDataObject->IsDataAvailable(CF_UNICODETEXT))
	{
		de = DROPEFFECT_COPY;
	}

	if (de != DROPEFFECT_NONE )
	{
		// notifico al bodyedit di visualizzare l'operazione...
		m_pTarget->ShowDragging(point);
	}

	return de;
}


//-----------------------------------------------------------------------------
BOOL	CTBExtBEDropTarget::DocumentIsReadOnly() const
{
	return	(
				!GetDocument()->m_bBatch &&
				(
					(GetDocument()->GetFormMode() != CBaseDocument::EDIT) &&
					(GetDocument()->GetFormMode() != CBaseDocument::NEW)
				)
			);
}

//-----------------------------------------------------------------------------
void CTBExtBEDropTarget::ActivateMainFrame(CWnd* pWnd)
{
	CFrameWnd* pFrameWnd = DYNAMIC_DOWNCAST(CFrameWnd, AfxGetMainWnd());
	
	if (pFrameWnd)
		pFrameWnd->SetForegroundWindow();

	CFrameWnd	*pFrame = pWnd->GetParentFrame();
	
	if (pFrame)
	{
		// finestra corrente
		CWnd*	pCurrentWnd = pWnd->GetFocus();
		if (pCurrentWnd)
		{
			if (pCurrentWnd->GetParentFrame() == pFrame)
			{
				// la finestra su cui sta passando il cursore del mouse è già attiva
				return;
			}
		}
		
		// la finestra su cui sta passando il cursore del mouse viene attivata
		pFrame->ActivateFrame	(SW_SHOW);
		pFrame->SetFocus		();
	}
}
//-----------------------------------------------------------------------------


