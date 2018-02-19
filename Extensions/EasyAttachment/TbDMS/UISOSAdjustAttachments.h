#pragma once

#include <TbGes\TileDialog.h>
#include <TbGes\TileManager.h>
#include <TbGes\BODYEDIT.H>

#include "beginh.dex"

class BDSOSAdjustAttachments;

///////////////////////////////////////////////////////////////////////////////
//			Class CSOSAdjustAttachmentsResultsBodyEdit definition
//			 body edit con elenco dei SOS document da inviare
///////////////////////////////////////////////////////////////////////////////
//============================================================================
class CSOSAdjustAttachmentsResultsBodyEdit : public CJsonBodyEdit
{
	DECLARE_DYNCREATE(CSOSAdjustAttachmentsResultsBodyEdit)

public:
	CSOSAdjustAttachmentsResultsBodyEdit();
};

/////////////////////////////////////////////////////////////////////////////
//			class CSOSAdjustAttachmentsWizardFormView definition
/////////////////////////////////////////////////////////////////////////////
//============================================================================
class TB_EXPORT CSOSAdjustAttachmentsWizardFormView : public CWizardFormView
{
	DECLARE_DYNCREATE(CSOSAdjustAttachmentsWizardFormView)

protected:
	CSOSAdjustAttachmentsWizardFormView();

public:
	BDSOSAdjustAttachments* GetDocument() const { return (BDSOSAdjustAttachments*)m_pDocument; }

public:
	virtual void UpdateTitle();

protected:
	//{{AFX_MSG(CSOSAdjustAttachmentsWizardFormView)
	afx_msg	void OnTimer(UINT nIDEvent);
	//}}AFX_MSG

	DECLARE_MESSAGE_MAP()
};