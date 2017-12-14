#pragma once

#include "ADMResourcesMng.h"
#include "RMControls.h"

#include "beginh.dex"

//=============================================================================
class TB_EXPORT BDWorkerWindow : public CAuxiliaryFormDoc, public ADMWorkerWindowObj
{ 
	DECLARE_DYNCREATE(BDWorkerWindow)
	friend class CWorkerWindowView; 

public:
	BDWorkerWindow ();

public:	
	virtual	ADMObj*	GetADM() { return this; }

private:
	CAbstractFormDoc*			m_pDoc;
	CResourcesPictureStatic*	m_pCreatedWorkerPicture;
	CResourcesPictureStatic*	m_pModifiedWorkerPicture;

	DataLng				m_Worker;
	DataStr				m_WorkerDes;
	DataLng				m_CreatedWorker;
	DataStr				m_CreatedWorkerDes;
	DataDate			m_CreatedDate;
	DataStr				m_CreatedWorkerOfficePhone;
	DataStr				m_CreatedWorkerEmail;
	DataStr				m_CreatedWorkerPicture;
	DataLng				m_ModifiedWorker;
	DataStr				m_ModifiedWorkerDes;
	DataDate			m_ModifiedDate;
	DataStr				m_ModifiedWorkerOfficePhone;
	DataStr				m_ModifiedWorkerEmail;
	DataStr				m_ModifiedWorkerPicture;

public:
	void SetWorker( DataLng		aCreatedWorker, 
					DataStr		aCreatedWorkerDes, 
					DataDate	aCreatedDate, 
					DataStr		aCreatedWorkerOfficePhone, 
					DataStr		aCreatedWorkerEmail, 
					DataStr		aCreatedWorkerPicture, 
					DataLng		aModifiedWorker, 
					DataStr		aModifiedWorkerDes, 
					DataDate	aModifiedDate,
					DataStr		aModifiedWorkerOfficePhone, 
					DataStr		aModifiedWorkerEmail,
					DataStr		aModifiedWorkerPicture); 

protected: 
	virtual	BOOL OnOpenDocument			(LPCTSTR);
	virtual BOOL OnAttachData 			();
	virtual	void DisableControlsForBatch();
	virtual void OnParsedControlCreated	(CParsedCtrl* pCtrl);
	virtual	void OnFrameCreated();

public:
	//{{AFX_MSG(BDWorkerWindow)
	afx_msg void OnCloseWindowClick	();
	//}}AFX_MSG

	DECLARE_MESSAGE_MAP()
};

#include "endh.dex"
