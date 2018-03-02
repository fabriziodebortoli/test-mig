#pragma once
#include <TbGeneric\JsonFormEngine.h>
#include "ExtDocFrame.h"
#include "beginh.dex"

//=============================================================================
template<class T>
class TB_EXPORT CJsonFrameT : public T
{
protected:
	CJsonContext* m_pJsonContext = NULL;

private:
	BOOL HasToolbar();

public:
	CJsonFrameT();
	~CJsonFrameT();
	CWndFrameDescription* GetFrameDescription();
	BOOL PreCreateWindow(CREATESTRUCT& cs);

	virtual	BOOL OnCreateClient(LPCREATESTRUCT, CCreateContext*);
	virtual	BOOL UseSplitters() { return TRUE; }

	void OnCreateStepper();
	virtual BOOL CreateJsonToolbar(UINT nID) { return TRUE; /*NOT SUPPORTED, USE JSON TOOLBAR INSTEAD*/ }
	virtual BOOL CreateAccelerator();
	virtual BOOL OnCustomizeJsonToolBar() { return TRUE; }
	virtual BOOL LoadFrame(UINT nIDResource,
		DWORD dwDefaultStyle = WS_OVERLAPPEDWINDOW | FWS_ADDTOTITLE,
		CWnd* pParentWnd = NULL,
		CCreateContext* pContext = NULL);
	virtual void OnAdjustFrameSize(CSize& size); 
	void OnUpdateFrameTitle(BOOL bAddToTitle);
	virtual BOOL OnPopulatedDropDown(UINT nIdCommand);
	virtual void OnFrameCreated();

	afx_msg LRESULT OnGetComponent(WPARAM, LPARAM);
	afx_msg LRESULT OnGetComponentStrings(WPARAM, LPARAM);
	afx_msg LRESULT OnGetActivationData(WPARAM, LPARAM);
	void CreateHotLinks(CWndObjDescription* pDescri, CAbstractFormDoc* pDoc);
};

class TB_EXPORT CJsonFrame : public CJsonFrameT<CMasterFrame>
{
public:
	void SwitchBatchRunButtonState	();
	bool IsBatchFrame				()	{ return GetFrameDescription()->GetViewCategory() == VIEW_BATCH; }

public:
	DECLARE_DYNCREATE(CJsonFrame)
	DECLARE_MESSAGE_MAP()
};

class TB_EXPORT CJsonSlaveFrame : public CJsonFrameT<CSlaveFrame>
{
public:
	DECLARE_DYNCREATE(CJsonSlaveFrame)
	DECLARE_MESSAGE_MAP()

};

class TB_EXPORT CJsonDialog : public CParsedDialog
{
public:
	CJsonDialog(CAbstractFormDoc* pDoc, UINT nIDD);
	~CJsonDialog();
	CAbstractFormDoc* GetDocument() {
		return (CAbstractFormDoc*)m_pDocument;
	}
	
	virtual BOOL OnInitDialog();
};

#include "endh.dex"
