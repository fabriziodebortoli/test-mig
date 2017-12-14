#pragma once

#include <TbGes\BODYEDIT.H>

#include "beginh.dex"

class BDMassiveArchive;

/////////////////////////////////////////////////////////////////////////////
//			class CMassiveArchiveWizardFormView definition
/////////////////////////////////////////////////////////////////////////////
//----------------------------------------------------------------------------
class TB_EXPORT CMassiveArchiveWizardFormView : public CWizardFormView
{
	DECLARE_DYNCREATE(CMassiveArchiveWizardFormView)

protected:
	CMassiveArchiveWizardFormView();

public:
	BDMassiveArchive* GetDocument() const { return (BDMassiveArchive*)m_pDocument; }

public:
	virtual void UpdateTitle();

protected:
	//{{AFX_MSG(CMassiveArchiveWizardFormView)
	afx_msg	void OnTimer(UINT nIDEvent);
	//}}AFX_MSG

	DECLARE_MESSAGE_MAP()
};

///////////////////////////////////////////////////////////////////////////////
//				Class CFilesToArchiveBodyEdit definition
//			body edit con elenco dei file da archiviare
///////////////////////////////////////////////////////////////////////////////
//----------------------------------------------------------------------------
class CFilesToArchiveBodyEdit : public CJsonBodyEdit
{
	DECLARE_DYNCREATE(CFilesToArchiveBodyEdit)

public:
	CFilesToArchiveBodyEdit();

public:
	BDMassiveArchive* GetDocument() { return (BDMassiveArchive*)CBodyEdit::GetDocument(); }

public:
	virtual BOOL OnPostCreateClient	();
	virtual BOOL OnSubFolderFound	();
	virtual void OnDropFiles		(const CStringArray& arDroppedFiles);
	virtual BOOL OnDblClick			(UINT nFlags, CBodyEditRowSelected* pCurrentRow);
};
