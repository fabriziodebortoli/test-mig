#pragma once

#include <TbGes\BODYEDIT.H>

#include "beginh.dex"

class BDSOSDocSender;

///////////////////////////////////////////////////////////////////////////////
//				Class CSOSDocSenderResultsBodyEdit definition
//		 body edit con elenco dei SOS document da inviare
///////////////////////////////////////////////////////////////////////////////
//
class CSOSDocSenderResultsBodyEdit : public CJsonBodyEdit
{
	DECLARE_DYNCREATE(CSOSDocSenderResultsBodyEdit)

public:
	CSOSDocSenderResultsBodyEdit();
};

/////////////////////////////////////////////////////////////////////////////
//			class CSOSDocSenderWizardFormView definition
/////////////////////////////////////////////////////////////////////////////
//
class TB_EXPORT CSOSDocSenderWizardFormView : public CWizardFormView
{
	DECLARE_DYNCREATE(CSOSDocSenderWizardFormView)

protected:
	CSOSDocSenderWizardFormView();

public:
	BDSOSDocSender* GetDocument() const { return (BDSOSDocSender*)m_pDocument; }

public:
	virtual void UpdateTitle();

protected:
	//{{AFX_MSG(CSOSDocSenderWizardFormView)
	afx_msg	void OnTimer(UINT nIDEvent);
	//}}AFX_MSG

	DECLARE_MESSAGE_MAP()
};
