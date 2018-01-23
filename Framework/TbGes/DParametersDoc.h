#pragma once

#include <TbGes\TileDialog.h>
#include <TbGes\TileManager.h>
#include <TbGes\ExtDoc.h>

#include "beginh.dex"

/////////////////////////////////////////////////////////////////////////////
//					class DParametersDoc definition
/////////////////////////////////////////////////////////////////////////////
class TB_EXPORT DParametersDoc : public CAbstractFormDoc
{
	friend class CDParametersDoc;
	DECLARE_DYNAMIC(DParametersDoc)

public:
	DParametersDoc();
	virtual ~DParametersDoc();

protected:
	virtual BOOL CanSaveParameters	();

protected:
	//{{AFX_MSG(DParametersDoc)
	afx_msg void OnEditRecord();
	afx_msg void OnSaveRecord();
	afx_msg void OnUpdateEditRecord(CCmdUI*);
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};

/////////////////////////////////////////////////////////////////////////////
//					class CDParametersDoc definition
/////////////////////////////////////////////////////////////////////////////
class TB_EXPORT CDParametersDoc : public CClientDoc 
{
	DECLARE_DYNCREATE(CDParametersDoc)

public:
	CDParametersDoc ();
	~CDParametersDoc();
	
public:
	DParametersDoc* GetServerDoc() const 
		{
			ASSERT(m_pServerDocument && m_pServerDocument->IsKindOf(RUNTIME_CLASS(DParametersDoc))); 
			return (DParametersDoc*) m_pServerDocument;
		}

protected:
	virtual	BOOL OnOkTransaction	();
	virtual BOOL OnPrepareAuxData	();
};

/////////////////////////////////////////////////////////////////////////////
//					class CSingleRecordParametersFrame definition
/////////////////////////////////////////////////////////////////////////////
class TB_EXPORT CSingleRecordParametersFrame : public CMasterFrame
{
	DECLARE_DYNCREATE(CSingleRecordParametersFrame)

public:
	CSingleRecordParametersFrame();

protected:
	virtual BOOL OnCustomizeTabbedToolBar	(CTBTabbedToolbar* pTabbedBar);

};

/////////////////////////////////////////////////////////////////////////////
//					class CMultiRecordParametersFrame definition
/////////////////////////////////////////////////////////////////////////////
class TB_EXPORT CMultiRecordParametersFrame : public CMasterFrame
{
	DECLARE_DYNCREATE(CMultiRecordParametersFrame)

public:
	CMultiRecordParametersFrame();

protected:
	virtual BOOL OnCustomizeTabbedToolBar(CTBTabbedToolbar* pTabbedBar);

};

//////////////////////////////////////////////////////////////////////////////
//							CParametersTileDialog
//////////////////////////////////////////////////////////////////////////////
class TB_EXPORT CParametersTileDialog : public CTileDialog
{
	DECLARE_DYNCREATE(CParametersTileDialog)

public:
	CParametersTileDialog();
	CParametersTileDialog(const CString& sName, int nIDD, CWnd* pParent = NULL);

public:
	virtual BOOL	Create(UINT nIDC, const CString& strTitle, CWnd* pParentWnd, TileDialogSize tileSize);

};

/////////////////////////////////////////////////////////////////////////////
//			class DCompanyUserSettingsDoc definition
/////////////////////////////////////////////////////////////////////////////
#define GET_SAVE_SETTING(a)		if (bSave) ini.Set##a(m_##a,FALSE);	else m_##a = ini.Get##a();

#define DECLARE_SETTING(a, b)	Data##a		m_##b;

//------------------------------------------------------------------------------------------
class TB_EXPORT DCompanyUserSettingsDoc : public CAbstractFormDoc
{
	friend class CCompanyUserSettingsTileDlg;
	friend class CCompanyUserSettingsTileGrp;

	DECLARE_DYNAMIC(DCompanyUserSettingsDoc)

public:
	DCompanyUserSettingsDoc();
	~DCompanyUserSettingsDoc();

public:
	CTBPropertyGrid* m_pPropertyGrid;

protected:
	virtual BOOL CanDoNewRecord		()	{ return TRUE; }
	virtual BOOL CanDoEditRecord	()	{ return TRUE; }
	virtual BOOL CanDoSaveRecord	()	{ return (GetFormMode() == NEW || GetFormMode() == EDIT); }
	virtual BOOL OnAttachData		();
	virtual BOOL OnPrepareAuxData	();
	virtual BOOL OnOkTransaction	();
	virtual BOOL OnEditTransaction	();
	virtual void OnGoInBrowseMode	();
	virtual void OnOpenCompleted	();

protected:
	// I due metodi sono alternativi. 
	// Se GetPropertyGridIDD ritorna un valore != 0 ha la priorità
	virtual UINT			GetPropertyGridIDD()			{ return 0; }
	virtual CRuntimeClass*  GetPropertyGridRuntimeClass()	{ return NULL; }

	virtual BOOL GetSaveSettings	(BOOL bSave = FALSE) = 0;

protected:
	//{{AFX_MSG(DCompanyUserSettingsDoc)
		afx_msg void OnControlChanged	(UINT nIDC);
		afx_msg void OnUpdateEditRecord	(CCmdUI*);
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};

//////////////////////////////////////////////////////////////////////////////
//						 CCompanyUserSettingsView
//////////////////////////////////////////////////////////////////////////////
class TB_EXPORT CCompanyUserSettingsView : public CMasterFormView
{
	DECLARE_DYNCREATE(CCompanyUserSettingsView)
	
protected:	
	CCompanyUserSettingsView();

public:		
	DCompanyUserSettingsDoc*	GetDocument  () const { return (DCompanyUserSettingsDoc*) m_pDocument;}

public:		
	virtual	void BuildDataControlLinks();
};

/////////////////////////////////////////////////////////////////////////////
//				CCompanyUserSettingsTileGrp
/////////////////////////////////////////////////////////////////////////////
class TB_EXPORT CCompanyUserSettingsTileGrp : public CTileGroup
{
	DECLARE_DYNCREATE(CCompanyUserSettingsTileGrp)

protected:
	virtual	void Customize();

public:
	DCompanyUserSettingsDoc* GetDocument() { return (DCompanyUserSettingsDoc*)__super::GetDocument(); }
};

/////////////////////////////////////////////////////////////////////////////
//						CCompanyUserSettingsTileDlg
/////////////////////////////////////////////////////////////////////////////
class TB_EXPORT CCompanyUserSettingsTileDlg : public CParametersTileDialog
{
	DECLARE_DYNCREATE(CCompanyUserSettingsTileDlg)

public:
	CCompanyUserSettingsTileDlg();

	virtual	void BuildDataControlLinks();

public:
	DCompanyUserSettingsDoc*	GetDocument() const { return (DCompanyUserSettingsDoc*)m_pDocument; }

};

#include "endh.dex"