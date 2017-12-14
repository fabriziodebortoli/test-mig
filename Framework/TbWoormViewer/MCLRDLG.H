
#pragma once

#include <TbGenlib\parsobj.h>
#include <TbGenlib\expr.h>

//per l'editor di expression
#include <TbWoormEngine\edtcmm.h>
#include <TbWoormEngine\RepTable.h>

//includere alla fine degli include del .H
#include "beginh.dex"
//=============================================================================

class CColorCombo;
class ColorQueue;
class CWoormDocMng;
//==============================================================================
class TB_EXPORT ColorDlgObj : public CParsedDialog
{
	DECLARE_DYNAMIC(ColorDlgObj)
	DECLARE_MESSAGE_MAP()
public:
	static void ManSetup (int nID, COLORREF rgbColor);

protected:
	static const int c_nSize = 4;

	ColorDlgObj		(UINT nResId, CWnd* pParent);

	ColorQueue*		m_CurrQueue;
	CColorCombo*	m_CurrCombo;

	ColorQueue*		m_WorkCQ[c_nSize];
	CColorCombo*    m_Combos[c_nSize];

	BOOL    OnInitDialog();
	void    OnOK		();

	void    OnCustomize0	();
	void    OnCustomize1	();
	void    OnCustomize2	();
	void    OnCustomize3	();

	void    OnCancel	();
	
	virtual ColorQueue*	LookupQueue	(int nIndex) = 0;

	virtual	void Setup		() = 0;
	virtual BOOL CopyBack	() = 0;

	void    SetFocus0	()  { m_CurrCombo = m_Combos[0]; m_CurrQueue = m_WorkCQ[0]; }
	void    SetFocus1	()  { m_CurrCombo = m_Combos[1]; m_CurrQueue = m_WorkCQ[1]; }
	void    SetFocus2	()  { m_CurrCombo = m_Combos[2]; m_CurrQueue = m_WorkCQ[2]; }
	void    SetFocus3	()  { m_CurrCombo = m_Combos[3]; m_CurrQueue = m_WorkCQ[3]; }

	void    DoCustomize	();

};

//==============================================================================
class TB_EXPORT CColorsDlg : public ColorDlgObj				
{
	DECLARE_MESSAGE_MAP()
public:
	enum    { LABEL, VALUE, BACKGROUND, BORDER, MAX };

	CColorsDlg  (COLORREF* rgbMCarray, BOOL* pbFlags, Expression*& pTextColorExpr, Expression*& pBkgColorExpr, Expression*& pLabelTextColorExpr, SymTable* pSymTable = NULL, CWnd* pParent=NULL);
	~CColorsDlg ();

protected:
	COLORREF*       m_pMCArray;
	BOOL*           m_pFlags;

	Expression*&		m_pTextColorExpr;
	Expression*&		m_pBkgColorExpr;
	Expression*&		m_pLabelTextColorExpr;

	CExpEdit			m_edtTextColorExpr;
	CExpEdit			m_edtBkgColorExpr;
	CExpEdit			m_edtLabelTextColorExpr;
	SymTable*			m_pSymTable;

	virtual ColorQueue*	LookupQueue	(int nIndex);

	virtual	void Setup		();
	virtual BOOL CopyBack	();
	virtual	void SetupCombo	(int nIdx, UINT nComboID, UINT nLabelID, UINT nCustomID);
};

//============================================================================
class TB_EXPORT CCellColorsDlg : public ColorDlgObj
{
	DECLARE_MESSAGE_MAP()
public:
	enum  		{ VALUE, BACKGROUND, BORDER, MAX };
	enum What	{ CELL, ROW, COLUMN, ALL };

	CCellColorsDlg	(COLORREF* rgbMCArray, What& what, CWnd* pParent=NULL);
	~CCellColorsDlg	();

protected:
	COLORREF*   m_pMCArray;
	What&		m_What;
	CButton		m_Btn[4];

	afx_msg void OnDisableBorder	();
	afx_msg void OnEnableBorder		();

	virtual ColorQueue*	LookupQueue	(int nIndex);

	virtual	void Setup		();
	virtual BOOL CopyBack	();
};

//============================================================================
class TB_EXPORT CColumnColorsDlg : public ColorDlgObj
{
	DECLARE_MESSAGE_MAP()
public:
	enum    { VALUE, BACKGROUND, BORDER, MAX };

	CColumnColorsDlg	(COLORREF* rgbMCArray, BOOL& bAll, Expression*& p_TitleTextColorExpr, Expression*& p_TitleBkgColorExpr, SymTable* pSymTable = NULL, CWnd* pParent=NULL);
	~CColumnColorsDlg	();

protected:
	COLORREF*	m_pMCArray;
	BOOL&		m_bAll;
	CButton		m_Btn;

	Expression*&		m_pTextColorExpr;
	Expression*&		m_pBkgColorExpr;
	CExpEdit			m_edtTextColorExpr;
	CExpEdit			m_edtBkgColorExpr;
	SymTable*			m_pSymTable;

	virtual ColorQueue*	LookupQueue	(int nIndex);

	virtual	void Setup		();
	virtual BOOL CopyBack	();
};

//============================================================================
class TB_EXPORT CSubTotalColorsDlg : public ColorDlgObj
{
	DECLARE_MESSAGE_MAP()
public:
	enum    { VALUE, BACKGROUND, MAX };

	CSubTotalColorsDlg	(COLORREF* rgbMCArray, BOOL& bAll,  Expression*& p_SubTotalTextColorExpr, Expression*& p_SubTotalBkgColorExpr, SymTable* pSymTable = NULL, CWnd* pParent=NULL);
	~CSubTotalColorsDlg	();

protected:
	COLORREF*	m_pMCArray;
	BOOL&		m_bAll;
	CButton		m_Btn;

	Expression*&		m_pSubTotalTextColorExpr;
	Expression*&		m_pSubTotalBkgColorExpr;
	CExpEdit			m_edtSubTotalTextColorExpr;
	CExpEdit			m_edtSubTotalBkgColorExpr;
	SymTable*			m_pSymTable;

	virtual ColorQueue*	LookupQueue	(int nIndex);

	virtual	void Setup		();
	virtual BOOL CopyBack	();
};

//============================================================================
class TB_EXPORT CTableTitleColorsDlg : public ColorDlgObj		
{
	DECLARE_MESSAGE_MAP()
public:
	enum    { LABEL, BACKGROUND, BORDER, EASYVIEW, MAX };

	CTableTitleColorsDlg	(COLORREF* rgbColor, CWnd* pParent=NULL);
	~CTableTitleColorsDlg   ();

protected:
	COLORREF* m_pMCArray;

	virtual ColorQueue*	LookupQueue	(int nIndex);

	virtual	void Setup		();
	virtual BOOL CopyBack	();
};

//============================================================================
class TB_EXPORT CColumnTitleColorsDlg : public ColorDlgObj	
{
	DECLARE_MESSAGE_MAP()
public:
	enum    { LABEL, BACKGROUND, BORDER, MAX };

	CColumnTitleColorsDlg	(COLORREF* rgbColor, BOOL& bAll, Expression*& p_TitleTextColorExpr, Expression*& p_TitleBkgColorExpr, SymTable* pSymTable = NULL, CWnd* pParent=NULL);
	~CColumnTitleColorsDlg  ();

protected:
	COLORREF*	m_pMCArray;
	BOOL&		m_bAll;
	CButton		m_Btn;

	Expression*&		m_pTitleTextColorExpr;
	Expression*&		m_pTitleBkgColorExpr;
	CExpEdit			m_edtTitleTextColorExpr;
	CExpEdit			m_edtTitleBkgColorExpr;
	SymTable*			m_pSymTable;

	virtual ColorQueue*	LookupQueue	(int nIndex);

	virtual	void Setup		();
	virtual BOOL CopyBack	();
};


//============================================================================
class TB_EXPORT CTotalColorsDlg : public ColorDlgObj
{
	DECLARE_MESSAGE_MAP()
public:
	enum    { VALUE, BACKGROUND, BORDER, MAX };

	CTotalColorsDlg	(COLORREF* rgbColor, BOOL& bAll, Expression*& p_TotalTextColorExpr, Expression*& p_TotalBkgColorExpr, SymTable* pSymTable = NULL, CWnd* pParent=NULL);
	~CTotalColorsDlg  ();

protected:
	COLORREF*	m_pMCArray;
	BOOL&		m_bAll;
	CButton		m_Btn;

	Expression*&		m_pTotalTextColorExpr;
	Expression*&		m_pTotalBkgColorExpr;
	CExpEdit			m_edtTotalTextColorExpr;
	CExpEdit			m_edtTotalBkgColorExpr;
	SymTable*			m_pSymTable;

	virtual ColorQueue*	LookupQueue	(int nIndex);

	virtual	void Setup		();
	virtual BOOL CopyBack	();
};

//============================================================================
class TB_EXPORT CDropShadowDlg : public CParsedDialog
{
	DECLARE_DYNAMIC(CDropShadowDlg)
	DECLARE_MESSAGE_MAP()
public:
	DataLng			m_Color;
	DataInt			m_Height;

	CDropShadowDlg	(COLORREF crDropShadowColor, int nDropShadowHeight);
	~CDropShadowDlg	();

protected:

	CIntEdit		m_edtHeight;
	CColorEdit		m_edtColor;

	BOOL    OnInitDialog();
	void    OnOK		();
	void    OnCancel	();

	void    OnCustomize	();
};

//==============================================================================
class TB_EXPORT CDynamicFormatStyleColumnDlg : public CParsedDialog			
{
	DECLARE_MESSAGE_MAP()
public:
	CDynamicFormatStyleColumnDlg  (Expression*& pExpr, SymTable* pSymTable, CWoormDocMng* pDoc, DataType dt);
	~CDynamicFormatStyleColumnDlg ();

protected:
	SymTable*			m_pSymTable;
	Expression*&		m_pExpr;
	CExpEdit			m_edtExpr;
	CWoormDocMng*			m_pDocument;
	DataType			m_dt;

	BOOL    OnInitDialog();
	void    OnOK		();
	void    OnCustomize	();
	void    OnCancel	();
};

//==============================================================================
class TB_EXPORT CDynamicFontStyleColumnDlg : public CParsedDialog			
{
	DECLARE_DYNAMIC(CDynamicFontStyleColumnDlg)
	DECLARE_MESSAGE_MAP()
public:
	CDynamicFontStyleColumnDlg  (Expression*& pExpr, SymTable* pSymTable, BOOL& bAll, CWoormDocMng* pDoc);
	~CDynamicFontStyleColumnDlg ();

protected:
	SymTable*			m_pSymTable;
	Expression*&		m_pExpr;
	CExpEdit			m_edtExpr;
	BOOL&				m_bAll;
	CWoormDocMng*			m_pDocument;

	BOOL    OnInitDialog();
	void    OnOK		();
	void    OnCustomize	();
	void    OnCancel	();
};

//==============================================================================
class TB_EXPORT CDynamicCellBordersDlg : public CParsedDialog			
{
	DECLARE_MESSAGE_MAP()
public:
	CDynamicCellBordersDlg  (Expression*& pExpr, SymTable* pSymTable, BOOL& bAll, CWoormDocMng* pDoc = NULL);
	~CDynamicCellBordersDlg ();

protected:
	SymTable*			m_pSymTable;
	Expression*&		m_pExpr;
	CExpEdit			m_edtExpr;
	BOOL&				m_bAll;
	CWoormDocMng*			m_pDocument;

	BOOL    OnInitDialog();
	void    OnOK		();
	void    OnCancel	();
};

//==============================================================================
#include "endh.dex"

