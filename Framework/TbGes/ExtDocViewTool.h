
#pragma once
#include <atlimage.h>
#include <TbGenlib\oslinfo.h>
//-------------------------------------------------------------------------
//includere alla fine degli include del .H
#include "beginh.dex"
//============================================================================
class HotKeyLink;
class SqlRecord;
class CAbstractFormView;
class CTabDialog;
class CLabelStatic;
class CGroupBoxBtn;

//=============================================================================
class TB_EXPORT CGroupControls 
{
private:
	UINT				m_nPlaceHolderIDC;
	UINT				m_nTemplateFormIDD;
	CLocalizableDialog*	m_pDlgTemplate;
	BOOL				m_bBuilt;
protected:
	CTabDialog*			m_pTab;
	CAbstractFormView*	m_pView;

public:

	class TB_EXPORT LinkInfo : public CObject
	{
	public:
		UINT			m_nBornIDC;
		UINT			m_nIDC;
		CString			m_sName;
		CRuntimeClass*	m_pRTC;
		DataObj*		m_pData;
		SqlRecord*		m_pRec;
		HotKeyLink*		m_pHKL;
		UINT			m_nBtnID;

		LinkInfo (UINT nBornIDC, CRuntimeClass* pRTC = NULL, UINT nBtnID = BTN_DEFAULT)
			:
			m_nBornIDC		(nBornIDC),
			m_sName			(),
			m_nIDC			(0),
			m_pRTC			(pRTC),	
			m_pData			(NULL),			
			m_pRec			(NULL),			
			m_pHKL			(NULL),			
			m_nBtnID		(nBtnID)		
		{}

		operator UINT () const { return m_nIDC; }
	};

	LinkInfo* FindLink(UINT nBornIDC) const
	{
		for (int i = 0; i < m_arLinks.GetSize(); i++)
		{
			LinkInfo* pL = (LinkInfo*) m_arLinks.GetAt(i);
			if (pL->m_nBornIDC == nBornIDC)
				return pL;
		}
		return NULL;
	}

	LinkInfo* RebindLink(UINT nBornIDC, UINT nNewIDC, const CString& sName, SqlRecord* pRec = NULL,  DataObj* pData = NULL,HotKeyLink* pHKL = NULL)
	{
		LinkInfo* pL = FindLink(nBornIDC);
		if (!pL)
		{
			ASSERT(FALSE);
			return NULL;
		}
		pL->m_nIDC = nNewIDC;
		pL->m_sName = sName;
		pL->m_pData = pData;
		pL->m_pRec = pRec;
		pL->m_pHKL = pHKL;

		return pL;
	}

protected:
	LinkInfo* InitLink(UINT nBornIDC, CRuntimeClass* pRTC = NULL, UINT nBtnID = BTN_DEFAULT)
	{
		LinkInfo* pL = new LinkInfo(nBornIDC, pRTC, nBtnID);
		m_arLinks.Add(pL);
		return pL;
	}

protected:
	Array	m_arLinks;

public:
					CGroupControls			(CWnd* pMaster, UINT nTemplateFormIDD);
	virtual			~CGroupControls			();

			BOOL	AddGroupControlsLink	(UINT nPlaceHolderIDC, BOOL bBuild = FALSE);
			BOOL	Build					();

private:
	BOOL			CreateControls		();

protected:
	virtual void	BuildDataControlLinks	() {}

	CWnd*			GetMasterWnd		() const { return (m_pTab ? (CWnd*) m_pTab : (CWnd*) m_pView); }
	UINT			LookupIDC			(UINT nBornIDC) const ;

	CWnd*			GetDlgItem			(UINT nBornIDC) const { return GetMasterWnd()->GetDlgItem (LookupIDC(nBornIDC)); }
	void			HideControlGroup	(UINT nBornIDC, BOOL bHide = TRUE);	
	BOOL			HideControl			(UINT nBornIDC, BOOL bHide = TRUE);	

	CParsedCtrl*	AddLink				(UINT nBornIDC);
	CExtButton*		AddExtButtonLink	(UINT nBornIDC);
	CLabelStatic*	AddLabelLink		(UINT nBornIDC);
	CGroupBoxBtn*	AddGroupBoxLink		(UINT nBornIDC);

	virtual void	InitLinks	() {}
	virtual void	RebindLinks () {}
};

//==========================================================================================
#include "endh.dex"
