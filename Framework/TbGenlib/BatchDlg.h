
#pragma once

#include "parsctrl.h"

#include <TbGeneric\critical.h>
#include <TbGeneric\schedule.h>

//includere alla fine degli include del .H
#include "beginh.dex"

//===========================================================================
class TB_EXPORT CBatchDialog : public CParsedDialog
{
	DECLARE_DYNAMIC(CBatchDialog)
protected:
	CriticalArea		 m_BatchRunningArea;
	Scheduler			 m_BatchScheduler;
	BOOL				 m_bBatchRunning;

public:
	CBatchDialog(UINT nIdd, CWnd* pWndParent = NULL);

public:	
	virtual	BOOL	PreTranslateMessage	(MSG* pMsg);
	virtual void	OnOK				();
	
protected:
	void	SetBatchButtonState	();
	void	BatchStart			();
	void	BatchStop			();

protected:
	virtual BOOL	OnInitDialog		();
	// da reimplementare, contiene il "cuore" del processo batch
	virtual	void	OnBatchExecute		()	{ /* default do nothing */ }
	// chiamata all'uscita della dialog, per disallocare eventuali risorse, ecc.
	virtual	void	OnCloseDialog		()	{ /* default do nothing */ }
	// permette al programmatore di disabilitare gli edit sulla dialog durante il running
	virtual void	EnableControls		();

protected:
	//{{AFX_MSG( CBatchDialog )
	afx_msg	void	OnBatchStartStop	();
	afx_msg	void 	OnBatchPauseResume	();
	afx_msg	void	OnCancel			();
	//}}AFX_MSG

	DECLARE_MESSAGE_MAP()
};

#include "endh.dex"
