#pragma once

#include <TbGeneric\linefile.h>
#include <TbGenlib\basedoc.h>

//includere alla fine degli include del .H
#include "beginh.dex"

//===========================================================================
class TB_EXPORT CPadDoc : public CBaseDocument
{
private:
	BOOL	m_bSaveAs;
	CLineFile::FileFormat m_FileFormat;
	
	DECLARE_DYNCREATE(CPadDoc)

protected:
	virtual BOOL    OnNewDocument	();
	virtual BOOL    OnOpenDocument	(LPCTSTR pszPathName);
	virtual BOOL	OnSaveDocument	(LPCTSTR pszPathName);
   	virtual void	SetPathName		(LPCTSTR lpszPathName, BOOL = TRUE);

protected:
	BOOL DoSave(LPCTSTR pszPathName, BOOL bReplace = TRUE);
	virtual BOOL SaveModified(); // return TRUE if ok to continue

protected:
	void Serialize(CArchive& ar);
	//{{AFX_MSG(CPadDoc)
	afx_msg void OnUpdateFileSave	(CCmdUI* pCmdUI);
	afx_msg void OnFileSave			();
	afx_msg void OnFileSaveAs		();
	afx_msg void OnTabStop			();
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};
//===========================================================================
class TB_EXPORT CPadFrame : public CLocalizableFrame
{
public:
	virtual BOOL LoadFrame(UINT nIDResource, DWORD dwDefaultStyle = WS_OVERLAPPEDWINDOW | FWS_ADDTOTITLE, CWnd* pParentWnd = NULL, CCreateContext* pContext = NULL);
private:
	void CreateMenu();
};
//===========================================================================
class TB_EXPORT CPadView : public CEditView
{
	DECLARE_DYNCREATE(CPadView)
public:
	CPadView();

public:
	virtual void OnInitialUpdate();
	virtual void OnUpdate (CView* pSender, LPARAM lHint = 0L, CObject* pHint = NULL);
};

#include "endh.dex"

