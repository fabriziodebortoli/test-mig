
#pragma once

#include "radardef.h"

//includere alla fine degli include del .H
#include "beginh.dex"

// Libreria GenLib
class CParsedDialog;
class CIntEdit;
class DataObj;

class RadarColumnsIdxArray;
class RadarIntArray;
class RadarColumn;
class RadarBody;

// Classi definite all'interno del modulo
//=============================================================================
class CRadarInsertColumnsDlg;
class CRadarResizeColumnsDlg;

class CRadarFindDlg;
	class CRadarFindStrDlg;
	class CRadarFindEnumDlg;
	class CRadarFindNumDlg;
		class CRadarFindIntDlg;
		class CRadarFindDblDlg;
		class CRadarFindLngDlg;
		class CRadarFindQtaDlg;
		class CRadarFindMonDlg;
		class CRadarFindPercDlg;
		class CRadarFindDateDlg;

//	Definizione di RadarFindCondition
//=============================================================================
class TB_EXPORT RadarFindCondition : public CObject
{
private:
	RadarIntArray* m_pBufferSelCols;

public:
	BOOL m_bDown;
	ECompareType m_nNumCond;

	DataObj* m_pDataObj;

public:
	RadarFindCondition	();
	~RadarFindCondition	();
                                                                    
public:
	int  GetSelCol		(int nIdx);
	int  GetSizeSelCols	();                             

	void AddSelCols	(int nIdx);
	void RemoveAll	();

// Diagnostics
#ifdef _DEBUG
public:
	void Dump		(CDumpContext&)	const;
	void AssertValid()				const { CObject::AssertValid(); }
#endif // _DEBUG
};

//	Definizione di CRadarInsertColumnsDlg
//=============================================================================
class TB_EXPORT CRadarInsertColumnsDlg : public CParsedDialog
{
	DECLARE_DYNAMIC(CRadarInsertColumnsDlg)
private:
	BOOL m_bOneColumnSel;

	CButton	 m_BtnDesAllFileds;
	CButton	 m_BtnSelAllFileds;
	CButton	 m_CBWithMouse;
	CButton	 m_RBHeadTable;
	CButton	 m_RBNextSelCol;
	CButton	 m_RBPrevSelCol;
	CButton	 m_RBTailTable;

	CTBListBox m_LBDeletedCols;

	RadarColumnsIdxArray* m_pDeletedCols;
	RadarColumnsIdxArray* m_pColsToInsert;

	CRadarDoc* m_pRadarDoc;

public:
	CRadarInsertColumnsDlg (CRadarDoc* pRadarDoc, RadarColumnsIdxArray*, RadarColumnsIdxArray*, BOOL bOneColumnSel, CWnd* = NULL);

protected:
	void EnableRadioButton	(BOOL bEnable = TRUE);
	void EnableTypeAction	(int nSelCount);

protected:
	virtual BOOL OnInitDialog	();
	virtual void OnOK			();

	//{{AFX_MSG(CRadarInsertColumnsDlg)
	afx_msg void OnBtnDesAllColumns	();
	afx_msg void OnBtnSelAllColumns	();
	afx_msg void OnCBWithMouse		();
	afx_msg void OnListBoxSelChange	();
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()

// Diagnostics
#ifdef _DEBUG
public:
	void Dump		(CDumpContext&)	const;
	void AssertValid()				const { CParsedDialog::AssertValid(); }
#endif // _DEBUG
};

//	Definizione di CRadarResizeColumnsDlg
//=============================================================================
class TB_EXPORT CRadarResizeColumnsDlg : public CParsedDialog
{
	DECLARE_DYNAMIC(CRadarResizeColumnsDlg)
private:
	CButton m_RBOnlySelCols;
	CButton m_RBAllCols;
	CButton m_RBMinWidth;
	CButton m_RBDefaultWidth;
	CButton m_RBCustomWidth;

	CIntEdit*		m_pEdtCustomWidth;
	RadarBody*		m_pRadarBody;
	RadarIntArray*	m_pWidthCols;

public:
	CRadarResizeColumnsDlg	(RadarBody*, RadarIntArray* pWidthCols, CWnd* = NULL);
	~CRadarResizeColumnsDlg	();

protected:
	virtual BOOL OnInitDialog	();
	virtual void OnOK			();

	//{{AFX_MSG(CRadarResizeColumnsDlg)
	afx_msg void OnRBCustomSizeDisable	();
	afx_msg void OnRBCustom				();
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()

// Diagnostics
#ifdef _DEBUG
public:
	void Dump		(CDumpContext&)	const;
	void AssertValid()				const { CParsedDialog::AssertValid(); }
#endif // _DEBUG
};

//	Definizione di CRadarFindDlg
//=============================================================================
class TB_EXPORT CRadarFindDlg : public CParsedDialog
{
	DECLARE_DYNAMIC(CRadarFindDlg)
private:
	UINT m_nIDCSub;
	CString m_strTitle;

	CButton	m_FindDown;
	CButton	m_FindUp;

public:
	CParsedCtrl* m_pParsedCtrl;

	RadarFindCondition* m_pCond;
	DataObj*			m_pTmpDataObj;

public:
	CRadarFindDlg (UINT nIdd, const CString& strTitle, UINT nIdcSub, RadarFindCondition*, CWnd* = NULL);
	~CRadarFindDlg();

public:
	virtual void ControlChange	();
	virtual void InitDialog		() {};

protected:
	virtual void OnCancel		();
	virtual BOOL OnInitDialog	();
	virtual void OnOK			();

	//{{AFX_MSG(CRadarFindDlg)
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()

// Diagnostics
#ifdef _DEBUG
public:
	void Dump		(CDumpContext&)	const;
	void AssertValid()				const { CParsedDialog::AssertValid(); }
#endif // _DEBUG
};

//	Definizione di CRadarFindStrDlg
//=============================================================================
class TB_EXPORT CRadarFindStrDlg : public CRadarFindDlg
{
private:
	CButton m_MatchCase;
	CButton m_MatchWord;

public:
	CRadarFindStrDlg	(RadarFindCondition*, CWnd* = NULL);

protected:
	virtual void InitDialog	();
	virtual void OnOK		();

	//{{AFX_MSG(CRadarFindStrDlg)
	afx_msg void OnControlUpdate();
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()

// Diagnostics
#ifdef _DEBUG
public:
	void Dump		(CDumpContext&)	const;
	void AssertValid()				const { CRadarFindDlg::AssertValid(); }
#endif // _DEBUG
};

//	Definizione di CRadarFindStrDlg
//=============================================================================
class TB_EXPORT CRadarFindEnumDlg : public CRadarFindDlg
{
public:
	CRadarFindEnumDlg (RadarFindCondition*, CWnd* = NULL);

protected:
	//{{AFX_MSG(CRadarFindEnumDlg)
	afx_msg void OnControlChange() { ControlChange (); }
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()

// Diagnostics
#ifdef _DEBUG
public:
	void Dump		(CDumpContext&)	const;
	void AssertValid()				const { CRadarFindDlg::AssertValid(); }
#endif // _DEBUG
};

//	Definizione di CRadarFindBoolDlg
//=============================================================================
class TB_EXPORT CRadarFindBoolDlg : public CRadarFindDlg
{
public:
	CRadarFindBoolDlg (RadarFindCondition*, CWnd* = NULL);

protected:
	virtual void InitDialog	();

	//{{AFX_MSG(CRadarFindBoolDlg)
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()

// Diagnostics
#ifdef _DEBUG
public:
	void Dump		(CDumpContext&)	const;
	void AssertValid()				const { CRadarFindDlg::AssertValid(); }
#endif // _DEBUG
};

//	Definizione di CRadarFindNumDlg
//=============================================================================
class TB_EXPORT CRadarFindNumDlg : public CRadarFindDlg
{
private:
	CBCGPComboBox m_cbCondition;

public:
	CRadarFindNumDlg	(RadarFindCondition*, const CString& strTitle, CWnd* = NULL);

protected:
	virtual void InitDialog	();
	virtual void OnOK		();

	//{{AFX_MSG(CRadarFindNumDlg)
	afx_msg void OnControlChange() { ControlChange (); }
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()

// Diagnostics
#ifdef _DEBUG
public:
	void Dump		(CDumpContext&)	const;
	void AssertValid()				const { CRadarFindDlg::AssertValid(); }
#endif // _DEBUG
};

//	Definizione di CRadarFindDateDlg
//=============================================================================
class TB_EXPORT CRadarFindDateDlg : public CRadarFindNumDlg
{
public:
	CRadarFindDateDlg (RadarFindCondition*, CWnd* = NULL);

protected:
	//{{AFX_MSG(CRadarFindDateDlg)
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()

// Diagnostics
#ifdef _DEBUG
public:
	void Dump		(CDumpContext&)	const;
	void AssertValid()				const { CRadarFindNumDlg::AssertValid(); }
#endif // _DEBUG
};

//	Definizione di CRadarFindDblDlg
//=============================================================================
class TB_EXPORT CRadarFindDblDlg : public CRadarFindNumDlg
{
public:
	CRadarFindDblDlg	(RadarFindCondition*, CWnd* = NULL);

protected:
	//{{AFX_MSG(CRadarFindDblDlg)
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()

// Diagnostics
#ifdef _DEBUG
public:
	void Dump		(CDumpContext&)	const;
	void AssertValid()				const { CRadarFindNumDlg::AssertValid(); }
#endif // _DEBUG
};

//	Definizione di CRadarFindIntDlg
//=============================================================================
class TB_EXPORT CRadarFindIntDlg : public CRadarFindNumDlg
{
public:
	CRadarFindIntDlg	(RadarFindCondition*, CWnd* = NULL);

protected:
	//{{AFX_MSG(CRadarFindIntDlg)
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()

// Diagnostics
#ifdef _DEBUG
public:
	void Dump		(CDumpContext&)	const;
	void AssertValid()				const { CRadarFindNumDlg::AssertValid(); }
#endif // _DEBUG
};

//	Definizione di CRadarFindLngDlg
//=============================================================================
class TB_EXPORT CRadarFindLngDlg : public CRadarFindNumDlg
{
public:
	CRadarFindLngDlg	(RadarFindCondition*, CWnd* = NULL);

protected:
	//{{AFX_MSG(CRadarFindLngDlg)
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()

// Diagnostics
#ifdef _DEBUG
public:
	void Dump		(CDumpContext&)	const;
	void AssertValid()				const { CRadarFindNumDlg::AssertValid(); }
#endif // _DEBUG
};

//	Definizione di CRadarFindNumDlg
//=============================================================================
class TB_EXPORT CRadarFindQtaDlg : public CRadarFindNumDlg
{
public:
	CRadarFindQtaDlg	(RadarFindCondition*, CWnd* = NULL);

protected:
	//{{AFX_MSG(CRadarFindQtaDlg)
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()

// Diagnostics
#ifdef _DEBUG
public:
	void Dump		(CDumpContext&)	const;
	void AssertValid()				const { CRadarFindNumDlg::AssertValid(); }
#endif // _DEBUG
};

//	Definizione di CRadarFindMonDlg
//=============================================================================
class TB_EXPORT CRadarFindMonDlg : public CRadarFindNumDlg
{
public:
	CRadarFindMonDlg	(RadarFindCondition*, CWnd* = NULL);

protected:
	//{{AFX_MSG(CRadarFindMonDlg)
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()

// Diagnostics
#ifdef _DEBUG
public:
	void Dump		(CDumpContext&)	const;
	void AssertValid()				const { CRadarFindNumDlg::AssertValid(); }
#endif // _DEBUG
};

//	Definizione di CRadarFindPercDlg
//=============================================================================
class TB_EXPORT CRadarFindPercDlg : public CRadarFindNumDlg
{
public:
	CRadarFindPercDlg (RadarFindCondition*, CWnd* = NULL);

protected:
	//{{AFX_MSG(CRadarFindPercDlg)
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()

// Diagnostics
#ifdef _DEBUG
public:
	void Dump		(CDumpContext&)	const;
	void AssertValid()				const { CRadarFindNumDlg::AssertValid(); }
#endif // _DEBUG
};

#include "endh.dex"
