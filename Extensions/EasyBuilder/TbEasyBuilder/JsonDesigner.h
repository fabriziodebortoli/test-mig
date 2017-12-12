#pragma once
#include <TbGes\EXTDOC.H>
class CEasyStudioDesignerDialog;

//=============================================================================
class CEasyStudioDesignerDialog : public CAbstractFormView
{
	CStringArray m_arFiles;
	
	friend class CEasyStudioDesignerView;
	CString m_strJsonFile;
	CDummyDescription   m_DummyParent;
	void Init();
public:
	CEasyStudioDesignerDialog(const CString& sJsonFile);
	CEasyStudioDesignerDialog(const CString& sJsonFile, CJsonContextObj* pContext);

	~CEasyStudioDesignerDialog();
	void AppendDefine(CString& sBuffer, CWndObjDescription* pDesc);
	bool UpdateDescription(HWND hwnd);
	bool AddDescription(HWND hwnd, HWND hwndParent);
	bool DeleteDescription(HWND hwnd);
	bool UpdateTabOrder(HWND hwnd);
	void ShowError(LPCTSTR szError);
	CString GetCode();
	bool SaveFile();
	BOOL Create(CWnd* pParent, CBaseDocument* pDoc);
	void BuildDataControlLinks(void){}
	/*CBCGP*/afx_msg void OnShowWindow(BOOL bShow, UINT nStatus);

	DECLARE_DYNAMIC(CEasyStudioDesignerDialog)
	DECLARE_MESSAGE_MAP()
};

//=============================================================================
class CEasyStudioDesignerDoc : public CDynamicFormDoc
{
	DECLARE_DYNCREATE(CEasyStudioDesignerDoc)
public:
	CString m_sInitialFile;
	CEasyStudioDesignerDoc();
	virtual ~CEasyStudioDesignerDoc();
	virtual	void OnFrameCreated();
	BOOL InitDocument();
	DECLARE_MESSAGE_MAP()
	virtual BOOL OnOpenDocument(LPCTSTR lpszPathName);
};
//=============================================================================
class CEasyStudioDesignerView : public CDynamicFormView
{
	friend ref class JsonConnector;
	CStringArray m_arCodeStack;
	int m_nCodePointer = -1;
	gcroot<JsonConnector^> m_Connector;
	gcroot<Microarea::EasyBuilder::UI::JsonCodeControl^> m_CodeControl;
public:
	CEasyStudioDesignerDialog* m_pDialog = NULL;

	DECLARE_DYNCREATE(CEasyStudioDesignerView)

	CEasyStudioDesignerView();
	~CEasyStudioDesignerView();
	virtual	void BuildDataControlLinks();
	BOOL OpenDialog(const CString& sFile);
	BOOL CloseDialog(const CString& sFile);
	void SetDirty(bool bDirty);
	DECLARE_MESSAGE_MAP()
	afx_msg BOOL OnEraseBkgnd(CDC* pDC);
	virtual BOOL OnWndMsg(UINT message, WPARAM wParam, LPARAM lParam, LRESULT* pResult);
	void UpdateSourceCode();
	void UpdateSourceCode(CString sCode);
	System::String^ SetCodeAndUpdate(CString sCode);
	BOOL UpdateFromSourceCode(const CString& sCode);
	void Select(const CString& sId);
	CString GetCodeControlCode(){ return m_CodeControl ? m_CodeControl->Code : _T(""); }
	CString GetSourceCode(){ return m_pDialog ? m_pDialog->GetCode() : _T(""); }
	System::String^ Undo();
	System::String^ Redo();
	afx_msg void OnDestroy();
};

//=============================================================================
class CEasyStudioDesignerFrame : public CMasterFrame
{
	DECLARE_DYNCREATE(CEasyStudioDesignerFrame)
public:
	CEasyStudioDesignerFrame();

};