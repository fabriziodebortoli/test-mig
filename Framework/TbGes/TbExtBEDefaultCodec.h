#pragma once

#include <tbges\TBExtBEDropTarget.h>
#include <tbges\CTBEDataCoDecASCII.h>

#include "beginh.dex"

class CAbstractFormView;

///////////////////////////////////////////////////////////////////////////////
class TB_EXPORT CTBEDefaultCoDec : public CTBEDataCoDecASCII
{
	DECLARE_DYNAMIC(CTBEDefaultCoDec)

protected:
	CLIPFORMAT			m_cfCoDec;

public:
	CTBEDefaultCoDec(CRuntimeClass* pDBTRrc);
	virtual	~CTBEDefaultCoDec();

	virtual	void		LoadRows			(CAbstractFormDoc* pDocument, DBTSlaveBuffered* pDBT, CBodyEdit* pBody, int nRecIdx = -1);
	virtual	CLIPFORMAT	GetClipFormat		() { return m_cfCoDec; }

protected:
			void		PrepareCodec		(LPCTSTR strCFFormat);
};

///////////////////////////////////////////////////////////////////////////////

class TB_EXPORT CTBEDefaultDropTargetCoDec : public CTBExtBEDropTarget
{
	DECLARE_DYNCREATE(CTBEDefaultDropTargetCoDec);

public:
	CTBEDefaultDropTargetCoDec()
	{
		m_bDisableCF_TEXT		= TRUE;
		m_bDisableCF_SelfDrop	= TRUE;
		m_bDisableCF_Body		= FALSE;		
		m_bDisableCF_CoDec		= FALSE;
	}
	
	virtual	~CTBEDefaultDropTargetCoDec (){}
	
	virtual	void OnDropCoDec	(CTBEDataCoDec*	pClpBrdDataCodec);
};

///////////////////////////////////////////////////////////////////////////////
class CAbstractFormView;

//-----------------------------------------------------------------------------
//			class CTBEDefaultBaseDropTarget
//-----------------------------------------------------------------------------
class TB_EXPORT CTBEDefaultBaseDropTarget : public CTBExtBEDropTarget
{
public:
	virtual DROPEFFECT OnDragEnter	(CWnd* pWnd, COleDataObject* pDataObject, DWORD dwKeyState, CPoint point);
	virtual DROPEFFECT OnDragOver	(CWnd* pWnd, COleDataObject* pDataObject, DWORD dwKeyState, CPoint point);
	virtual void OnDragLeave		(CWnd* pWnd);
};


//-----------------------------------------------------------------------------
//			class CTBEDefaultBaseDropTargetArray
//-----------------------------------------------------------------------------
class TB_EXPORT CTBEDefaultBaseDropTargetArray : public TArray <CTBEDefaultBaseDropTarget>
{
public:
	void	AutoRegister	(CAbstractFormView* pView);

protected:
	void	Register		(CWnd* pWnd);
};


//-----------------------------------------------------------------------------
//	class CTBEDefaultBaseDropTargetPlugIn
//-----------------------------------------------------------------------------
class TB_EXPORT CTBEDefaultBaseDropTargetPlugIn
{	
private:
	BOOL m_bDropTargetEnabled;

protected:
	CTBEDefaultBaseDropTargetArray		m_DropTargetArray;

	CTBEDefaultBaseDropTargetPlugIn	();
	
public:
	void EnableDropTarget		(BOOL bEnable = TRUE) { m_bDropTargetEnabled = bEnable; }
	BOOL IsDropTargetEnabled	() { return m_bDropTargetEnabled; }
};


#include "endh.dex"
