#pragma once

#include "TileManager.h"
#include "TileDialog.h"
#include "beginh.dex"

class CHeaderStripTile;
class CHeaderStripFormat;

//============================================================================= 
class TB_EXPORT CHeaderTileGroup : public CTileGroup
{
	DECLARE_DYNCREATE(CHeaderTileGroup)

public:
	CHeaderTileGroup();

protected:
	virtual void Customize		();

};

//============================================================================= 
class TB_EXPORT CHeaderStrip : public CHeaderTileGroup
{
	DECLARE_DYNCREATE(CHeaderStrip)
	DECLARE_MESSAGE_MAP()
public:
	CHeaderStrip();

public:
	void					SetCaption				(const CString& strCaption);
	void					UpdateCaption			();
	CHeaderStripFormat*		Add						(DataObj*); 	// Add fields to observe
	static CHeaderStrip*	AddHeaderStrip			(CAbstractFormView* pView, UINT nIDC, const CString& strDefaultCaption,BOOL bCallInitialUpdate = TRUE, CRect rectWnd = CRect(0, 0, 0, 0), CRuntimeClass* pClass = NULL); // static methos to create and update a single HeaderStrip per documnet

protected:
	virtual void Customize		();
	// This method reattach fields of the current line of the bodyedit
	virtual void RebuildLinks	(SqlRecord* pRec);
	LRESULT OnGetControlDescription(WPARAM wParam, LPARAM lParam);
private:
	CHeaderStripTile*			m_pHeaderStripTile;
	CObArray					m_HeaderFields;	// Array of fields shown in the caption of header strip

	CAbstractFormView* m_pView;
};

//============================================================================= 
class CHeaderStripFormat : public CDataEventsObj, public CObject
{
private:
	TDisposablePtr<CHeaderStrip>	m_pHeaderStrip;
	DataObj*		m_pDataObj;

public:
	CHeaderStripFormat(DataObj* pObj, CHeaderStrip* pHeaderStrip)
		:
		m_pHeaderStrip(pHeaderStrip)
	{
		ASSERT_VALID(pHeaderStrip);
		ASSERT_VALID(pObj);

		m_pDataObj = pObj->Clone();
		m_bOwned = TRUE;
	}

	~CHeaderStripFormat() { delete m_pDataObj; }

	virtual void Signal(CObservable* pSender, EventType eType)
	{
		//ASSERT_VALID(m_pHeaderStrip);
		ASSERT_VALID(m_pDataObj);

		if (eType == ON_CHANGED)
		{
			if (m_pHeaderStrip)
				m_pHeaderStrip->UpdateCaption();
		}
	}

	virtual void Fire(CObservable* pSender, EventType eType)
	{
	}

	virtual CObserverContext* GetContext() const { return NULL; }
};

#include "endh.dex"
