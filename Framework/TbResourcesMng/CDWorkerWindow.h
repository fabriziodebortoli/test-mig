#pragma once

#include <TbGes\ExtDocClientDoc.h>

#include "TWorkers.h"

#include "beginh.dex"

class ADMWorkerWindowObj;

//==============================================================================
//					Class CDWorkerWindow
//==============================================================================
class TB_EXPORT CDWorkerWindow : public CClientDoc
{
protected:
	DECLARE_DYNCREATE(CDWorkerWindow)

public:
	CDWorkerWindow();
	~CDWorkerWindow();

	CAbstractFormDoc* GetServerDoc() const { return m_pServerDocument; }
	
private:
	TDisposablePtr<ADMWorkerWindowObj> 	m_pDoc;
	TRWorkers*			m_pTRWorkers;

	DataLng				m_Worker;
	DataStr				m_WorkerDes;
	DataLng				m_CreatedWorker;
	DataStr				m_CreatedWorkerDes;
	DataStr				m_CreatedWorkerLastName;
	DataDate			m_CreatedDate;
	DataStr				m_CreatedWorkerOfficePhone;
	DataStr				m_CreatedWorkerEmail;
	DataStr				m_CreatedWorkerPicture;
	DataLng				m_ModifiedWorker;
	DataStr				m_ModifiedWorkerDes;
	DataStr				m_ModifiedWorkerLastName;
	DataDate			m_ModifiedDate;
	DataStr				m_ModifiedWorkerOfficePhone;
	DataStr				m_ModifiedWorkerEmail;
	DataStr				m_ModifiedWorkerPicture;

private:
	void LoadWorker();

protected:
	virtual BOOL OnAttachData		();
	virtual BOOL OnPrepareAuxData	();
	virtual void OnGoInBrowseMode	();
	virtual void Customize			();
			BOOL OnShowStatusBarMsg	(CString& sMsg);

protected:
	//{{AFX_MSG(CDWorkerWindow)
	afx_msg void OnWorkerWindow			();
	afx_msg void OnEnableWorkerWindow	(CCmdUI* pCmdUI);
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};

#include "endh.dex"
