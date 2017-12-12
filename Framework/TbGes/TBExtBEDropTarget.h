#pragma once

#include <afxole.h>

#include "BodyEdit.h"

//includere alla fine degli include del .H
#include "beginh.dex"

/////////////////////////////////////////////////////////////////////////////
// CTBExtBEDropTarget 
//-----------------------------------------------------------------------------
class TB_EXPORT CTBExtBEDropTarget : public COleDropTarget
{
	friend class CBodyEdit;

	DECLARE_DYNCREATE(CTBExtBEDropTarget);

protected:
	CBodyEdit*						m_pTarget ;
	CLIPFORMAT						m_cfBody;
	CLIPFORMAT						m_cfBodySelf;

	// dove e' avvenuto il drop...
	CBodyEdit::DragSelType			m_dstWhere;
	int								m_nDropRecordIdx;

	BOOL							m_bDisableCF_TEXT;
	BOOL							m_bDisableCF_SelfDrop;
	BOOL							m_bDisableCF_CoDec;
	BOOL							m_bDisableCF_Body;

protected:
	CTBExtBEDropTarget();

	void	AttachTarget(CBodyEdit* pBody);

public:
	virtual ~CTBExtBEDropTarget() {};

	virtual DROPEFFECT	OnDragEnter	( CWnd* pWnd, COleDataObject* pDataObject, DWORD dwKeyState, CPoint point );
	virtual DROPEFFECT	OnDragOver	( CWnd* pWnd, COleDataObject* pDataObject, DWORD dwKeyState, CPoint point );
	virtual BOOL		OnDrop		( CWnd* pWnd, COleDataObject* pDataObject, DROPEFFECT dropEffect, CPoint point );
	virtual void		OnDragLeave	( CWnd* pWnd );
	
	void				ActivateMainFrame(CWnd* pWnd);

protected:
	CBodyEdit*			GetSourceBody	(COleDataObject* pDataObject) const;
	CAbstractFormDoc*	GetDocument		() const { return m_pTarget->GetDocument(); }
	BOOL				DocumentIsReadOnly() const;

	///////////////////////////////////////
	// reimplementabili...
	//

	// drop da un altro bodyedit
	virtual	void		OnDropBody	(CBodyEdit*	/*pSourceBody*/){}

	// drop di testo..
	virtual	void		OnDropText	(const CString& /*txt*/){}

	// drop CoDec
	virtual	void		OnDropCoDec	(CTBEDataCoDec*	pClpBrdDataCodec){}
};

//=============================================================================
#include "endh.dex"
