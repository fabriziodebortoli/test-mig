#pragma once

#include <TbGeneric\DataObj.h>

#include <TbGenlib\PARSEDT.H>

//includere alla fine degli include del .H
#include "beginh.dex"

//=============================================================================
//			Class CWorkerStatic 
//=============================================================================
class TB_EXPORT CWorkerStatic : public CStrStatic
{
	DECLARE_DYNCREATE(CWorkerStatic)

public:
	CWorkerStatic();

private:
	BOOL	m_bMouseInside;
	HCURSOR	m_hOldCursor;
	HCURSOR	m_hHandCursor;

private:
	DataLng*	m_pWorker;

private:
	void OpenWorker();
	virtual	BOOL OnInitCtrl();

public:
	void SetWorker(DataLng* pWorker) { m_pWorker = pWorker; }

protected:
	//{{AFX_MSG(CWorkerStatic)
	afx_msg void OnMouseMove(UINT nFlags, CPoint point);
	afx_msg void OnLButtonUp(UINT nFlags, CPoint point);
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};

//=============================================================================
//			Class CResourcesPictureStatic 
//=============================================================================
class TB_EXPORT CResourcesPictureStatic : public CPictureStatic
{
	DECLARE_DYNCREATE(CResourcesPictureStatic)

public:
	CResourcesPictureStatic();

private:
	BOOL	m_bMouseInside;
	HCURSOR	m_hOldCursor;
	HCURSOR	m_hHandCursor;

private:
	DataStr	m_ResourceType;
	DataStr	m_Resource;
	DataLng	m_Worker;

private:
	void OpenResource();

public:
	void SetRoot	()											{ m_ResourceType.Clear();	m_Resource.Clear();		m_Worker.Clear(); }
	void SetResource(DataStr aResourceType, DataStr aResource)	{ m_ResourceType = aResourceType; m_Resource = aResource;	m_Worker.Clear(); }
	void SetWorker	(DataLng aWorker)							{ m_ResourceType.Clear(); m_Resource.Clear(); m_Worker = aWorker; }

protected:
	//{{AFX_MSG(CResourcesPictureStatic)
	afx_msg void OnMouseMove(UINT nFlags, CPoint point);
	afx_msg void OnLButtonUp(UINT nFlags, CPoint point);
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};

#include "endh.dex"
