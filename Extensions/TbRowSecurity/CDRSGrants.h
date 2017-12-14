#pragma once 

#include <TbGeneric\DataObj.h>
#include <TbGes\DBT.h>
#include <TbGes\ExtDocClientDoc.h>
#include <TbGes\Tabber.h>


//includere alla fine degli include del .H
#include "beginh.dex"

class RSEntityInfo;
class CEntityGrant;
class DBTEntitySubjectsGrants;
class TRS_SubjectsGrants;

/////////////////////////////////////////////////////////////////////////////
class TB_EXPORT CRSGrantsClientDoc : public CClientDoc
{
	DECLARE_DYNAMIC(CRSGrantsClientDoc)

public:
	RSEntityInfo* m_pEntityInfo;

private:
	CNumbererRequest* m_pNumRequest;
	DBTEntitySubjectsGrants*	m_pDBTEntitySubjectsGrants;

public:
	CRSGrantsClientDoc();
	~CRSGrantsClientDoc();

public:
	BOOL m_bValidDocument;
	BOOL m_bGrantToLoad;
	DataBool m_bTreeView;
	DataBool m_bFilterWriteGrant;

	BOOL	 m_bShowDeny;
	BOOL	 m_bShowRead;
	BOOL	 m_bShowFull;
public:
	//gestione singolo grant
	TRS_SubjectsGrants* m_pCurrSubjectsGrantsRec;
	DataEnum			m_CurrSubjectGrantType;
	DataStr			    m_strGrantDescription;
	DataStr		        m_strGrantInherited;
	DataStr			    m_strGrantPicture;

	

private:
	CAbstractFormDoc*	GetServerDoc();
	BOOL				CurrentWorkerHasFullGrant();
	BOOL				SaveExplicitGrants();
	void				SetImageAndDescription();

	void				RefreshGrantsTree();
	void				RefreshCurrentNode();

public:
	void				ModifyImplicitGrant(int nOldWorkerID, int nNewWorkerID);	
	TRS_SubjectsGrants*	GetGrantRecordForSubject(int nSubjectID);
	Array* 				ModifyExplicitGrant(TRS_SubjectsGrants* pSubjectsGrantsRec, DataEnum grantType);
	
	void SetCurrSubjectsGrantsRec(TRS_SubjectsGrants* pCurrSubjectsGrantsRec);
	void DoCurrentSubjectGrantsChanged();
	void  DoGrantTypeChanged(); //restituisce il numero di subjectgrants cambiati

	BOOL IsShowDeny() const { return m_bShowDeny; }
	BOOL IsShowRead() const { return m_bShowRead; }
	BOOL IsShowFull() const { return m_bShowFull; }

protected:
	virtual BOOL OnAttachData		();
    virtual void Customize	(); 
	virtual void OnAfterCreateAndInitDBT (DBTObject*);	

	virtual	BOOL CanDoDeleteRecord		 () { return CurrentWorkerHasFullGrant(); }
	virtual BOOL CanDoEditRecord		 () { return CurrentWorkerHasFullGrant(); }

	virtual	BOOL OnExtraNewTransaction	 ();
	virtual	BOOL OnExtraEditTransaction	 ();
	virtual	BOOL OnExtraDeleteTransaction();

	virtual BOOL OnPrepareAuxData();

public:
	//{{AFX_MSG(CRSGrantClientDoc)
	afx_msg void OnOpenFormGrant();
	afx_msg void OnUpdateOpenFormGrant	(CCmdUI*);
	afx_msg void OnProtectedCheckChanged();

	afx_msg void OnNoGrantShow();
	afx_msg void OnReadGrantShow();
	afx_msg void OnFullGrantShow();

	afx_msg void OnUpdateGrantShow(CCmdUI* pCmdUI);
	afx_msg void OnUpdateReadGrantShow(CCmdUI* pCmdUI);
	afx_msg void OnUpdateFullGrantShow(CCmdUI* pCmdUI);
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};

#include "endh.dex"