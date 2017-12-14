
#include "stdafx.h"

#include <TbGeneric\GeneralFunctions.h>

#include <TbGenlib\Generic.h>
#include <TbGenlib\baseapp.h>

#include <TbGenlibUI\FontsDialog.h>
#include <TbGenlibUI\FormatDialog.h>

#include <TbWoormEngine\RpSymTbl.h>
#include <TbWoormEngine\generic.h>

#include "woormdoc.h"

#include "mclrdlg.h"
#include "mclrdlg.hjson" //JSON AUTOMATIC UPDATE

//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char BASED_CODE THIS_FILE[] = __FILE__;
#endif



//============================================================================
class ColorQueue : public CTBLockable
{
public:
	ColorQueue	();
	ColorQueue	(const ColorQueue& MCQ);

	~ColorQueue	();
	ColorQueue&	operator=	(const ColorQueue& MCQ);

	COLORREF	GetColor	(int nIndex);
	int			Locate		(COLORREF rgbColor);
	int			Insert		(COLORREF rgbColor);
	void		DefSetup	();
	virtual LPCSTR  GetObjectName() const { return "ColorQueue"; }

protected:
	COLORREF* m_pPalette;
};


//============================================================================
class CColorCombo : public CBCGPComboBox
{
public:
	CColorCombo	(ColorQueue* CQ) : CBCGPComboBox(), m_ColorQueue(CQ) { }

	COLORREF	GetColor	() const			{ return m_ColorQueue->GetColor(GetCurSel()); }
	int			SetColor	(COLORREF rgbColor)	{ return m_ColorQueue->Insert(rgbColor); }

	ColorQueue* OwnQueue	() const { return m_ColorQueue; }

protected:
	ColorQueue* m_ColorQueue;

	virtual void MeasureItem(LPMEASUREITEMSTRUCT);
	virtual void DrawItem	(LPDRAWITEMSTRUCT);
};


//////////////////////////////////////////////////////////////////////////////
//	ColorQueue Methods														//
//////////////////////////////////////////////////////////////////////////////


#define	COLOR_QUEUE_SIZE	16

//----------------------------------------------------------------------------
ColorQueue::ColorQueue()
{
	m_pPalette = new COLORREF[COLOR_QUEUE_SIZE];
}


//----------------------------------------------------------------------------
ColorQueue::~ColorQueue()
{
	if (m_pPalette) delete []m_pPalette;
}


//----------------------------------------------------------------------------
ColorQueue::ColorQueue(const ColorQueue& MCQ)
{
	TB_LOCK_FOR_WRITE();
	
	(*this) = MCQ;
}


//----------------------------------------------------------------------------
ColorQueue& ColorQueue::operator=(const ColorQueue& MCQ)
{
	TB_LOCK_FOR_WRITE();
	for (int i = 0; i < COLOR_QUEUE_SIZE; i++)
		m_pPalette[i] = MCQ.m_pPalette[i];

	return *this;
}


//----------------------------------------------------------------------------
COLORREF ColorQueue::GetColor(int nIndex)
{
	TB_LOCK_FOR_READ();
	return (nIndex >= 0 && nIndex < COLOR_QUEUE_SIZE) ? m_pPalette[nIndex] : RGB(0,0,0);
}


//----------------------------------------------------------------------------
int ColorQueue::Locate(COLORREF rgbColor)
{
	TB_LOCK_FOR_READ();
	for (int i = 0; i < COLOR_QUEUE_SIZE; i++)
		if (m_pPalette[i] == rgbColor)
			return i;

	return -1;
}


//----------------------------------------------------------------------------
int ColorQueue::Insert(COLORREF rgbColor)
{
	TB_LOCK_FOR_WRITE();
	int nIndex = Locate(rgbColor);

	if (nIndex != -1) return nIndex;

	for (nIndex = COLOR_QUEUE_SIZE - 2; nIndex >= 0; nIndex--)
		m_pPalette[nIndex + 1] = m_pPalette[nIndex];
	m_pPalette[0] = rgbColor;

	return 0;
}


//----------------------------------------------------------------------------
void ColorQueue::DefSetup()
{
	Insert(RGB(0x00, 0x00, 0x00));
	Insert(RGB(0xFF, 0xFF, 0xFF));
	Insert(RGB(0x00, 0x00, 0xFF));
	Insert(RGB(0x00, 0xFF, 0x00));
	Insert(RGB(0xFF, 0x00, 0x00));
	Insert(RGB(128, 128, 128));//0x80
	Insert(RGB(192, 192, 192));//0xC0

	Insert(RGB(0x80,0x00,0x00));
	Insert(RGB(0x00,0x80,0x00));
	Insert(RGB(0x00,0x00,0x80));

	Insert(RGB(0xFF,0x00,0xFF));
	Insert(RGB(0xFF,0xFF,0x00));
	Insert(RGB(0x00,0xFF,0xFF));

	Insert(RGB(0x80,0x00,0x80));
	Insert(RGB(0x80,0x80,0x00));
	Insert(RGB(0x00,0x80,0x80));
}


/*
	Insert(RGB(0xFF, 0xFF, 0xFF));
	Insert(RGB(0x00, 0x00, 0x00));

	Insert(RGB(0x00, 0x00, 0xFF));
	Insert(RGB(0x00, 0xFF, 0x00));
	Insert(RGB(0xFF, 0x00, 0x00));

	Insert(RGB(0x40, 0x40, 0x40));
	Insert(RGB(0x80, 0x80, 0x80));
	Insert(RGB(0xC0, 0xC0, 0xC0));

	Insert(RGB(0x80,0x00,0x00));
	Insert(RGB(0x00,0x80,0x00));
	Insert(RGB(0x00,0x00,0x80));

	Insert(RGB(0x40,0x00,0x00));
	Insert(RGB(0x00,0x40,0x00));
	Insert(RGB(0x00,0x00,0x40));

	Insert(RGB(0xFF,0x00,0xFF));
	Insert(RGB(0xFF,0xFF,0x00));
	Insert(RGB(0x00,0xFF,0xFF));

	Insert(RGB(0x80,0x00,0x80));
	Insert(RGB(0x80,0x80,0x00));
	Insert(RGB(0x00,0x80,0x80));

	Insert(RGB(0xC0,0x00,0x00));
	Insert(RGB(0x00,0xC0,0x00));
	Insert(RGB(0x00,0x00,0xC0));

	Insert(RGB(0xFF,0x00,0x80));
	Insert(RGB(0x80,0xFF,0x00));
	Insert(RGB(0x00,0x80,0xFF));

	Insert(RGB(0xFF,0x00,0xC0));
	Insert(RGB(0xC0,0xFF,0x00));
	Insert(RGB(0x00,0xC0,0xFF));

	Insert(RGB(0xFF,0x00,0x40));
	Insert(RGB(0x40,0xFF,0x00));
	Insert(RGB(0x00,0x40,0xFF));
}

*/
//////////////////////////////////////////////////////////////////////////////
//	CColorCombo Methods														//
//////////////////////////////////////////////////////////////////////////////


#define	COMBOSIZE	8

//----------------------------------------------------------------------------
void CColorCombo::MeasureItem(LPMEASUREITEMSTRUCT lpMIS)
{
	lpMIS->itemHeight = COMBOSIZE;
}


//----------------------------------------------------------------------------
void CColorCombo::DrawItem(LPDRAWITEMSTRUCT lpDIS)
{
	int nIndex = (int) lpDIS->itemData;

	LOGBRUSH lb;
	lb.lbStyle = BS_SOLID;
	lb.lbColor = m_ColorQueue->GetColor(nIndex);

	HBRUSH currBrush = CreateBrushIndirect(&lb);
	lpDIS->rcItem.bottom++;
	FillRect(lpDIS->hDC,&lpDIS->rcItem,currBrush);
	DeleteObject(currBrush);
}


//////////////////////////////////////////////////////////////////////////////
//	Static variables for this module										//
//////////////////////////////////////////////////////////////////////////////


static ColorQueue _4_label;
static ColorQueue _4_value;
static ColorQueue _4_backgr;
static ColorQueue _4_border;
static ColorQueue cell_value;
static ColorQueue cell_backgr;
static ColorQueue cell_border;
static ColorQueue tabtitle_label;
static ColorQueue tabtitle_backgr;
static ColorQueue tabtitle_border;
static ColorQueue coltitle_label;
static ColorQueue coltitle_backgr;
static ColorQueue coltitle_border;
static ColorQueue total_value;
static ColorQueue total_backgr;
static ColorQueue total_border;
static ColorQueue tab_easyview;

BOOL InitializeColorQueue()
{
	_4_label.		DefSetup();
	_4_value.		DefSetup();
	_4_backgr.		DefSetup();
	_4_border.		DefSetup();
	cell_value.		DefSetup();
	cell_backgr.	DefSetup();
	cell_border.	DefSetup();
	tabtitle_label.	DefSetup();
	tabtitle_backgr.DefSetup();
	tabtitle_border.DefSetup();
	coltitle_label.	DefSetup();
	coltitle_backgr.DefSetup();
	coltitle_border.DefSetup();
	total_value.	DefSetup();
	total_backgr.	DefSetup();
	total_border.	DefSetup();
	tab_easyview.	DefSetup();
	return TRUE;
}
static const BOOL bColorsInitialized = InitializeColorQueue();

//////////////////////////////////////////////////////////////////////////////
//	ColorsDlgObj Methods													//
//////////////////////////////////////////////////////////////////////////////


//----------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(ColorDlgObj, CParsedDialog)
BEGIN_MESSAGE_MAP(ColorDlgObj, CParsedDialog)
	//{{AFX_MSG_MAP(ColorDlgObj)
	ON_COMMAND		(IDC_COLOR_CUSTOM0, OnCustomize0)
	ON_COMMAND		(IDC_COLOR_CUSTOM1, OnCustomize1)
	ON_COMMAND		(IDC_COLOR_CUSTOM2, OnCustomize2)
	ON_COMMAND		(IDC_COLOR_CUSTOM3, OnCustomize3)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP		()

//----------------------------------------------------------------------------
ColorDlgObj::ColorDlgObj(UINT nResId, CWnd* pParent)
	:
	CParsedDialog	(nResId, pParent),
	m_CurrCombo		(NULL),
	m_CurrQueue		(NULL)
{
	for (int i = 0; i < c_nSize; i++)
	{
		m_WorkCQ[i] = NULL;
		m_Combos[i] = NULL;
	}
}

//----------------------------------------------------------------------------
BOOL ColorDlgObj::OnInitDialog()
{
	CParsedDialog::OnInitDialog();
	Setup();

	RECT rect;

	int i = 0;
	for (i = 0; i < c_nSize; i++)
		if (m_Combos[i] && m_Combos[i]->m_hWnd)
		{
			m_Combos[i]->GetClientRect(&rect);
			break;
		}

	if (i == c_nSize)
		return FALSE;

	for (i = 0; i < c_nSize; i++)
		if (m_Combos[i] && m_Combos[i]->m_hWnd)
			AdjustClientRect(*m_Combos[i], rect.right, COLOR_QUEUE_SIZE * COMBOSIZE + rect.bottom + 4);

	return TRUE;
}


//----------------------------------------------------------------------------
void ColorDlgObj::OnOK()
{
	if (CopyBack())
		EndDialog(IDOK);
}


//----------------------------------------------------------------------------
void ColorDlgObj::OnCancel()
{
	EndDialog(IDCANCEL);
}

//----------------------------------------------------------------------------
void ColorDlgObj::OnCustomize1()
{
	SetFocus1 ();
	DoCustomize();
}
void ColorDlgObj::OnCustomize2()
{
	SetFocus2 ();
	DoCustomize();
}
void ColorDlgObj::OnCustomize3()
{
	SetFocus3 ();
	DoCustomize();
}
void ColorDlgObj::OnCustomize0()
{
	SetFocus0 ();
	DoCustomize();
}

//----------------------------------------------------------------------------
void ColorDlgObj::DoCustomize()
{
	if (!m_CurrCombo)
	{
		AfxMessageBox(_TB("You must select one of the elements for customizing"));
		return;
	}

	CHOOSECOLOR cc;
	COLORREF clr[16];

	memset(&cc,0,sizeof(CHOOSECOLOR));
	cc.lStructSize = sizeof(CHOOSECOLOR);
	cc.hwndOwner = m_hWnd;
	cc.lpCustColors = clr;
	cc.Flags = CC_FULLOPEN;

	for (int i = 0; i < 16; i++)
		clr[i] = COLORREF(m_CurrQueue->GetColor(i));

	if (ChooseColor(&cc))
	{
    	int nIndex = m_CurrCombo->SetColor(cc.rgbResult);
		m_CurrCombo->SetCurSel(nIndex);
	}
}

//////////////////////////////////////////////////////////////////////////////
// Gestisce il controllo e il salvataggio di un'espressione editata in un CExpEdit 
BOOL CheckAndSaveExpression (CExpEdit& expEdit, Expression*& pExpr, SymTable* pSymTable/* = NULL*/, WORD wDatatype = DATA_LNG_TYPE)
{
	CString strExpr;
	BOOL isNewExpr = FALSE;

	expEdit.GetWindowText(strExpr);
	ConvertCString(strExpr, CRLF_TO_LF);
	Parser	lex(strExpr);
	
	if (strExpr.IsEmpty())         //espressione vuota
	{
		if (pExpr)					//se prima c'era expr la elimino
			SAFE_DELETE(pExpr);
		return TRUE;
	}
	
	if (!pExpr)                     
	{
		pExpr = new Expression(pSymTable);
		isNewExpr = TRUE;
	}
	if (!expEdit.CheckExp(lex, *pExpr, wDatatype))
	{
		if (isNewExpr)
			SAFE_DELETE(pExpr);
		
		expEdit.SetCtrlFocus(TRUE);
		return FALSE;
	}
	return TRUE;
}

//////////////////////////////////////////////////////////////////////////////
//	CColorsDlg Methods														//
//////////////////////////////////////////////////////////////////////////////


//----------------------------------------------------------------------------
BEGIN_MESSAGE_MAP	(CColorsDlg, ColorDlgObj)
END_MESSAGE_MAP()

//----------------------------------------------------------------------------
CColorsDlg::CColorsDlg(COLORREF* rgbMCArray, BOOL* pFlags, Expression*& pTextColorExpr, Expression*& pBkgColorExpr, Expression*& pLabelTextColorExpr, SymTable* pSymTable/* = NULL*/, CWnd* pParent)
	:
	ColorDlgObj	(IDD_COLOR_4, pParent),
	m_pFlags	(pFlags),
	m_pMCArray	(rgbMCArray),
	m_pTextColorExpr		(pTextColorExpr),
	m_pBkgColorExpr			(pBkgColorExpr),
	m_pLabelTextColorExpr	(pLabelTextColorExpr),
	m_pSymTable			    (pSymTable)
{
	m_edtTextColorExpr.			SetSymbolTable(m_pSymTable);
	m_edtBkgColorExpr.			SetSymbolTable(m_pSymTable);
	m_edtLabelTextColorExpr.	SetSymbolTable(m_pSymTable);	

	for (int i=0;i<4;i++)
	{
		m_WorkCQ[i]=new ColorQueue;
		m_Combos[i]=new CColorCombo(m_WorkCQ[i]);
	}
}


//----------------------------------------------------------------------------
CColorsDlg::~CColorsDlg()
{
	for (int i = 0; i < 4; i++)
    {
		if (m_WorkCQ[i]) delete m_WorkCQ[i];
		if (m_Combos[i]) delete m_Combos[i];
		m_WorkCQ[i] = NULL;
        m_Combos[i] = NULL;
	}
}


//----------------------------------------------------------------------------
ColorQueue* CColorsDlg::LookupQueue(int nIndex)
{
	switch (nIndex)
	{
		case  0:	return &_4_label;
		case  1:	return &_4_value;
		case  2:	return &_4_backgr;
		case  3:	return &_4_border;
		default:	return NULL;
    }
}


//----------------------------------------------------------------------------
void CColorsDlg::SetupCombo(int nIdx, UINT nComboID, UINT nLabelID, UINT nCustomID)
{
	m_Combos[nIdx]->SubclassDlgItem(nComboID, this);

	if (!m_pFlags[nIdx])
	{
		m_Combos[nIdx]->ShowWindow(SW_HIDE);
		GetDlgItem(nLabelID)->ShowWindow(SW_HIDE);
		GetDlgItem(nCustomID)->ShowWindow(SW_HIDE);
	}
}

//----------------------------------------------------------------------------
void CColorsDlg::Setup()
{
	m_edtTextColorExpr.			SubclassEdit	(IDC_EDIT_VALUE_COLOR_EXPR,       		this);
	m_edtBkgColorExpr.			SubclassEdit	(IDC_EDIT_BKGCOLOR_EXPR,	    		this);
	m_edtLabelTextColorExpr.	SubclassEdit	(IDC_EDIT_FORECOLOR_EXPR,				this);

	//enum    { LABEL, VALUE, BACKGROUND, BORDER, MAX };
	m_edtLabelTextColorExpr.EnableWindow(m_pFlags[LABEL]);
	m_edtBkgColorExpr.EnableWindow(m_pFlags[VALUE] || m_pFlags[LABEL]); //is a SQRrect
	m_edtTextColorExpr.EnableWindow(m_pFlags[VALUE]);

	if (m_pTextColorExpr != NULL && m_edtTextColorExpr.IsWindowEnabled())
	{
		m_edtTextColorExpr.SetWindowText(m_pTextColorExpr->ToString());
	}
	if (m_pBkgColorExpr != NULL && m_edtBkgColorExpr.IsWindowEnabled())
	{
		m_edtBkgColorExpr.SetWindowText(m_pBkgColorExpr->ToString());
	}
	if (m_pLabelTextColorExpr != NULL && m_edtLabelTextColorExpr.IsWindowEnabled())
	{
		m_edtLabelTextColorExpr.SetWindowText(m_pLabelTextColorExpr->ToString());
	}
	
	int i, nIndex;

	SetupCombo(0, IDC_COLOR_COMBO0, IDC_COLOR_LABEL0, IDC_COLOR_CUSTOM0);
	SetupCombo(1, IDC_COLOR_COMBO1, IDC_COLOR_LABEL1, IDC_COLOR_CUSTOM1);
	SetupCombo(2, IDC_COLOR_COMBO2, IDC_COLOR_LABEL2, IDC_COLOR_CUSTOM2);
	SetupCombo(3, IDC_COLOR_COMBO3, IDC_COLOR_LABEL3, IDC_COLOR_CUSTOM3);

	for (i = 0; i < 4; i++)
		if (m_pFlags[i])
		{
			for (int j = 0; j < 16; j++) m_Combos[i]->SendMessage(CB_ADDSTRING,0,j);

			*m_WorkCQ[i] = *LookupQueue(i);
			nIndex=m_WorkCQ[i]->Insert(m_pMCArray[i]);
			m_Combos[i]->SetCurSel(nIndex);
		}
}


//----------------------------------------------------------------------------
BOOL CColorsDlg::CopyBack()
{
	for (int i = 0; i < 4; i++)
	{
		if (m_pFlags[i])
		{
			(*LookupQueue(i)) = *m_WorkCQ[i];
			m_pMCArray[i]=m_Combos[i]->GetColor();
		}
	}
	//Gestione Expressions
	BOOL bOK = TRUE;
	if (m_edtTextColorExpr.IsWindowEnabled())
		bOK = ::CheckAndSaveExpression(m_edtTextColorExpr, m_pTextColorExpr,m_pSymTable);
	if (m_edtBkgColorExpr.IsWindowEnabled())
		bOK = bOK && ::CheckAndSaveExpression(m_edtBkgColorExpr, m_pBkgColorExpr,m_pSymTable);
	if (m_edtLabelTextColorExpr.IsWindowEnabled())
		bOK = bOK && ::CheckAndSaveExpression(m_edtLabelTextColorExpr, m_pLabelTextColorExpr,m_pSymTable);	
	
	return bOK;
}


//////////////////////////////////////////////////////////////////////////////
//	CCellColorsDlg Methods													//
//////////////////////////////////////////////////////////////////////////////


//----------------------------------------------------------------------------
BEGIN_MESSAGE_MAP	(CCellColorsDlg,	ColorDlgObj)
	ON_CONTROL (BN_CLICKED, IDC_COLOR_BTN0,  OnDisableBorder)
	ON_CONTROL (BN_CLICKED, IDC_COLOR_BTN1,  OnDisableBorder)
	ON_CONTROL (BN_CLICKED, IDC_COLOR_BTN2,  OnEnableBorder)
	ON_CONTROL (BN_CLICKED, IDC_COLOR_BTN3,  OnEnableBorder)
END_MESSAGE_MAP()


//----------------------------------------------------------------------------
CCellColorsDlg::CCellColorsDlg(COLORREF* rgbMCArray, What& what, CWnd* pParent)
	:
	ColorDlgObj	(IDD_COLOR_CELL, pParent),
	m_pMCArray	(rgbMCArray),
	m_What		(what)
{
	for (int i = 0; i < 3; i++)
	{
		m_WorkCQ[i] = new ColorQueue;
		m_Combos[i] = new CColorCombo(m_WorkCQ[i]);
	}
}


//----------------------------------------------------------------------------
CCellColorsDlg::~CCellColorsDlg()
{
	for (int i = 0; i < 3; i++)
    {
		if (m_WorkCQ[i]) delete m_WorkCQ[i];
		if (m_Combos[i]) delete m_Combos[i];
		m_WorkCQ[i] = NULL;
        m_Combos[i] = NULL;
	}
}


//----------------------------------------------------------------------------
ColorQueue* CCellColorsDlg::LookupQueue(int nIndex)
{
	switch (nIndex)
	{
		case  0:	return &cell_value;
		case  1:	return &cell_backgr;
		case  2:	return &cell_border;
		default:	return NULL;
    }
}


//----------------------------------------------------------------------------
void CCellColorsDlg::OnDisableBorder()
{
	m_Combos[2]->EnableWindow(FALSE);
	GetDlgItem(IDC_COLOR_LABEL2)->EnableWindow(FALSE);
	GetDlgItem(IDC_COLOR_CUSTOM2)->EnableWindow(FALSE);
}

//----------------------------------------------------------------------------
void CCellColorsDlg::OnEnableBorder()
{
	m_Combos[2]->EnableWindow();
	GetDlgItem(IDC_COLOR_LABEL2)->EnableWindow();
	GetDlgItem(IDC_COLOR_CUSTOM2)->EnableWindow();
}

//----------------------------------------------------------------------------
void CCellColorsDlg::Setup()
{
	m_Combos[0]->SubclassDlgItem(IDC_COLOR_COMBO0, this);
	m_Combos[1]->SubclassDlgItem(IDC_COLOR_COMBO1, this);
	m_Combos[2]->SubclassDlgItem(IDC_COLOR_COMBO2, this);

	m_Btn[0].SubclassDlgItem(IDC_COLOR_BTN0 ,this);
	m_Btn[1].SubclassDlgItem(IDC_COLOR_BTN1 ,this);
	m_Btn[2].SubclassDlgItem(IDC_COLOR_BTN2 ,this);
	m_Btn[3].SubclassDlgItem(IDC_COLOR_BTN3 ,this);

	for (int i = 0; i < 3; i++)
	{
		for (int j = 0; j < 16; j++) m_Combos[i]->SendMessage(CB_ADDSTRING,0,j);
		*m_WorkCQ[i] = *LookupQueue(i);
		int nIndex=m_WorkCQ[i]->Insert(m_pMCArray[i]);
		m_Combos[i]->SetCurSel(nIndex);
	}

	switch (m_What)
	{
		case CELL	:	m_Btn[0].SetCheck(1);	OnDisableBorder();	break;
		case ROW	:	m_Btn[1].SetCheck(1);	OnDisableBorder();	break;
		case COLUMN	:	m_Btn[2].SetCheck(1);	OnEnableBorder();	break;
		case ALL	:	m_Btn[3].SetCheck(1);	OnEnableBorder();	break;
	}

	m_CurrCombo=m_Combos[0];
	m_CurrQueue=m_WorkCQ[0];
}


//----------------------------------------------------------------------------
BOOL CCellColorsDlg::CopyBack()
{
	for (int i=0;i<3;i++)
	{
		(*LookupQueue(i)) = *m_WorkCQ[i];
		m_pMCArray[i] = m_Combos[i]->GetColor();
	}

			if (m_Btn[0].GetCheck())	m_What = CELL; 
	else	if (m_Btn[1].GetCheck())	m_What = ROW;
	else	if (m_Btn[2].GetCheck())	m_What = COLUMN;
	else	if (m_Btn[3].GetCheck())	m_What = ALL;

	return TRUE;
}



//////////////////////////////////////////////////////////////////////////////
//	CColumnColorsDlg Methods													//
//////////////////////////////////////////////////////////////////////////////


//----------------------------------------------------------------------------
BEGIN_MESSAGE_MAP	(CColumnColorsDlg,	ColorDlgObj)
END_MESSAGE_MAP()

//----------------------------------------------------------------------------
CColumnColorsDlg::CColumnColorsDlg(COLORREF* rgbMCArray, BOOL& bAll, Expression*& p_TextColorExpr, Expression*& p_BkgColorExpr, SymTable* pSymTable/*= NULL*/, CWnd* pParent)
	:
	ColorDlgObj	(IDD_COLOR_COLUMN, pParent),
	m_pMCArray	(rgbMCArray),
	m_bAll		(bAll),
	m_pTextColorExpr	(p_TextColorExpr),
	m_pBkgColorExpr		(p_BkgColorExpr),
	m_pSymTable			(pSymTable)
{
	m_edtTextColorExpr.	SetSymbolTable(pSymTable);
	m_edtBkgColorExpr.	SetSymbolTable(pSymTable);

	for (int i = 0; i < 3; i++)
	{
		m_WorkCQ[i]=new ColorQueue;
		m_Combos[i]=new CColorCombo(m_WorkCQ[i]);
	}
}


//----------------------------------------------------------------------------
CColumnColorsDlg::~CColumnColorsDlg()
{
	for (int i = 0 ;i < 3 ; i++)
    {
		if (m_WorkCQ[i]) delete m_WorkCQ[i];
		if (m_Combos[i]) delete m_Combos[i];
		m_WorkCQ[i]=NULL;
        m_Combos[i]=NULL;
	}
}

//----------------------------------------------------------------------------
ColorQueue* CColumnColorsDlg::LookupQueue(int nIndex)
{
	switch (nIndex)
	{
		case  0:	return &cell_value;
		case  1:	return &cell_backgr;
		case  2:	return &cell_border;
		default:	return NULL;
    }
}


//----------------------------------------------------------------------------
void CColumnColorsDlg::Setup()
{
	m_edtTextColorExpr.			SubclassEdit	(IDC_EDIT_FORECOLOR_EXPR,       		this);
	m_edtBkgColorExpr.			SubclassEdit	(IDC_EDIT_BKGCOLOR_EXPR,	    		this);

	if (m_pTextColorExpr != NULL && m_edtTextColorExpr.IsWindowEnabled() )
	{
		m_edtTextColorExpr.SetWindowText(m_pTextColorExpr->ToString());
	}
	if (m_pBkgColorExpr != NULL && m_edtBkgColorExpr.IsWindowEnabled())
	{
		m_edtBkgColorExpr.SetWindowText(m_pBkgColorExpr->ToString());
	}
	
	m_Combos[0]->SubclassDlgItem(IDC_COLOR_COMBO0, this);
	m_Combos[1]->SubclassDlgItem(IDC_COLOR_COMBO1, this);
	m_Combos[2]->SubclassDlgItem(IDC_COLOR_COMBO2, this);

	for (int i = 0; i < 3; i++)
	{
		for (int j = 0; j < 16; j++) m_Combos[i]->SendMessage(CB_ADDSTRING,0,j);
		*m_WorkCQ[i] = *LookupQueue(i);
		int nIndex = m_WorkCQ[i]->Insert(m_pMCArray[i]);
		m_Combos[i]->SetCurSel(nIndex);
	}

	m_Btn.SubclassDlgItem(IDC_COLOR_BTN0, this);
	m_Btn.SetCheck(m_bAll);

	m_CurrCombo=m_Combos[0];
	m_CurrQueue=m_WorkCQ[0];
}


//----------------------------------------------------------------------------
BOOL CColumnColorsDlg::CopyBack()
{
	for (int i = 0; i < 3; i++)
	{
		(*LookupQueue(i)) = *m_WorkCQ[i];
		m_pMCArray[i] = m_Combos[i]->GetColor();
	}

	m_bAll = m_Btn.GetCheck();

	//Gestione Expressions
	BOOL bOK = TRUE;

	bOK = ::CheckAndSaveExpression(m_edtTextColorExpr, m_pTextColorExpr,m_pSymTable);
	bOK = bOK && ::CheckAndSaveExpression(m_edtBkgColorExpr, m_pBkgColorExpr,m_pSymTable);
	
	return bOK; 
}

//////////////////////////////////////////////////////////////////////////////
//	CSubTotalColorsDlg Methods													//
//////////////////////////////////////////////////////////////////////////////


//----------------------------------------------------------------------------
BEGIN_MESSAGE_MAP	(CSubTotalColorsDlg,	ColorDlgObj)
END_MESSAGE_MAP()

//----------------------------------------------------------------------------
CSubTotalColorsDlg::CSubTotalColorsDlg(COLORREF* rgbMCArray, BOOL& pFlags, Expression*& p_SubTotalTextColorExpr, Expression*& p_SubTotalBkgColorExpr, SymTable* pSymTable/*= NULL*/, CWnd* pParent)
	:
	ColorDlgObj	(IDD_COLOR_SUBTOTAL, pParent),
	m_pMCArray	(rgbMCArray),
	m_bAll		(pFlags),
	m_pSubTotalTextColorExpr(p_SubTotalTextColorExpr),
	m_pSubTotalBkgColorExpr	(p_SubTotalBkgColorExpr),
	m_pSymTable				(pSymTable)
{
	m_edtSubTotalTextColorExpr. SetSymbolTable	(pSymTable);
	m_edtSubTotalBkgColorExpr.	SetSymbolTable(pSymTable);
	
	for (int i = 0; i < 2; i++)
	{
		m_WorkCQ[i] = new ColorQueue;
		m_Combos[i] = new CColorCombo(m_WorkCQ[i]);
	}
}


//----------------------------------------------------------------------------
CSubTotalColorsDlg::~CSubTotalColorsDlg()
{
	for (int i = 0; i < 2 ; i++)
    {
		if (m_WorkCQ[i]) delete m_WorkCQ[i];
		if (m_Combos[i]) delete m_Combos[i];
		m_WorkCQ[i] = NULL;
        m_Combos[i] = NULL;
	}
}


//----------------------------------------------------------------------------
ColorQueue* CSubTotalColorsDlg::LookupQueue(int nIndex)
{
	switch (nIndex)
	{
		case  0:	return &cell_value;
		case  1:	return &cell_backgr;
		default:	return NULL;
    }
}


//----------------------------------------------------------------------------
void CSubTotalColorsDlg::Setup()
{
	m_edtSubTotalTextColorExpr.		SubclassEdit	(IDC_EDIT_FORECOLOR_EXPR,       		this);
	m_edtSubTotalBkgColorExpr.		SubclassEdit	(IDC_EDIT_BKGCOLOR_EXPR,	    		this);

	if (m_pSubTotalTextColorExpr != NULL && m_edtSubTotalTextColorExpr.IsWindowEnabled())
	{
		m_edtSubTotalTextColorExpr.SetWindowText(m_pSubTotalTextColorExpr->ToString());
	}
	if (m_pSubTotalBkgColorExpr != NULL && m_edtSubTotalBkgColorExpr.IsWindowEnabled())
	{
		m_edtSubTotalBkgColorExpr.SetWindowText(m_pSubTotalBkgColorExpr->ToString());
	}

	m_Combos[0]->SubclassDlgItem(IDC_COLOR_COMBO0, this);
	m_Combos[1]->SubclassDlgItem(IDC_COLOR_COMBO1, this);

	for (int i = 0; i < 2; i++)
	{
		for (int j = 0; j < 16; j++) m_Combos[i]->SendMessage(CB_ADDSTRING,0,j);
		*m_WorkCQ[i] = *LookupQueue(i);
		int nIndex = m_WorkCQ[i]->Insert(m_pMCArray[i]);
		m_Combos[i]->SetCurSel(nIndex);
	}

	m_Btn.SubclassDlgItem(IDC_COLOR_BTN0, this);
	m_Btn.SetCheck(m_bAll);

	m_CurrCombo=m_Combos[0];
	m_CurrQueue=m_WorkCQ[0];
}

//----------------------------------------------------------------------------
BOOL CSubTotalColorsDlg::CopyBack()
{
	for (int i = 0; i < 2; i++)
	{
		(*LookupQueue(i)) = *m_WorkCQ[i];
		m_pMCArray[i] = m_Combos[i]->GetColor();
	}

	m_bAll = m_Btn.GetCheck();

	//Gestione Expressions
	BOOL bOK = TRUE;
	
	bOK = ::CheckAndSaveExpression(m_edtSubTotalTextColorExpr, m_pSubTotalTextColorExpr, m_pSymTable);
	bOK = bOK && ::CheckAndSaveExpression(m_edtSubTotalBkgColorExpr, m_pSubTotalBkgColorExpr, m_pSymTable);

	return bOK;
}


//////////////////////////////////////////////////////////////////////////////
//	CTableTitleColorsDlg Methods											//
//////////////////////////////////////////////////////////////////////////////


//----------------------------------------------------------------------------
BEGIN_MESSAGE_MAP	(CTableTitleColorsDlg,	ColorDlgObj)
END_MESSAGE_MAP()


//----------------------------------------------------------------------------
CTableTitleColorsDlg::CTableTitleColorsDlg(COLORREF* pMCArray, CWnd* pParent)
	:
	ColorDlgObj	(IDD_COLOR_TABLE_TITLE, pParent),
	m_pMCArray	(pMCArray)
{
	for (int i = 0; i < c_nSize; i++)
	{
		m_WorkCQ[i] = new ColorQueue;
		m_Combos[i] = new CColorCombo(m_WorkCQ[i]);
	}
}


//----------------------------------------------------------------------------
CTableTitleColorsDlg::~CTableTitleColorsDlg()
{
	for (int i = 0; i < c_nSize; i++)
    {
		if (m_WorkCQ[i]) delete m_WorkCQ[i];
		if (m_Combos[i]) delete m_Combos[i];
		m_WorkCQ[i]=NULL;
		m_Combos[i]=NULL;
	}
}


//----------------------------------------------------------------------------
ColorQueue* CTableTitleColorsDlg::LookupQueue(int nIndex)
{
	switch (nIndex)
	{
		case  CTableTitleColorsDlg::LABEL:		return &tabtitle_label;
		case  CTableTitleColorsDlg::BACKGROUND:	return &tabtitle_backgr;
		case  CTableTitleColorsDlg::BORDER:		return &tabtitle_border;
		case  CTableTitleColorsDlg::EASYVIEW:	return &tab_easyview;
		default:	return NULL;
    }
}


//----------------------------------------------------------------------------
void CTableTitleColorsDlg::Setup()
{
	m_Combos[0]->SubclassDlgItem(IDC_COLOR_COMBO0, this);
	m_Combos[1]->SubclassDlgItem(IDC_COLOR_COMBO1, this);
	m_Combos[2]->SubclassDlgItem(IDC_COLOR_COMBO2, this);
	m_Combos[3]->SubclassDlgItem(IDC_COLOR_COMBO3, this);

	for (int i = 0; i < c_nSize; i++)
    {
		for (int j = 0; j < 16; j++) m_Combos[i]->SendMessage(CB_ADDSTRING,0,j);
		*m_WorkCQ[i] = *LookupQueue(i);
		int nIndex = m_WorkCQ[i]->Insert(m_pMCArray[i]);
		m_Combos[i]->SetCurSel(nIndex);
	}
}


//----------------------------------------------------------------------------
BOOL CTableTitleColorsDlg::CopyBack()
{
	for (int i = 0; i < c_nSize; i++)
	{
		(*LookupQueue(i)) = *m_WorkCQ[i];
		m_pMCArray[i] = m_Combos[i]->GetColor();
	}

	return TRUE;
}


//////////////////////////////////////////////////////////////////////////////
//	CColumnTitleColorsDlg Methods											//
//////////////////////////////////////////////////////////////////////////////


//----------------------------------------------------------------------------
BEGIN_MESSAGE_MAP	(CColumnTitleColorsDlg,	ColorDlgObj)
END_MESSAGE_MAP()

//----------------------------------------------------------------------------
CColumnTitleColorsDlg::CColumnTitleColorsDlg(COLORREF* pMCArray, BOOL& bAll, Expression*& p_TitleTextColorExpr, Expression*& p_TitleBkgColorExpr, SymTable* pSymTable/*= NULL*/, CWnd* pParent)
	:
	ColorDlgObj	(IDD_COLOR_COLUMN_TITLE, pParent),
	m_bAll		(bAll),
	m_pMCArray	(pMCArray),
	m_pTitleTextColorExpr		(p_TitleTextColorExpr),
	m_pTitleBkgColorExpr		(p_TitleBkgColorExpr),
	m_pSymTable					(pSymTable)
{
	m_edtTitleTextColorExpr. SetSymbolTable	(pSymTable);
	m_edtTitleBkgColorExpr.  SetSymbolTable	(pSymTable);

	for (int i = 0; i < 3; i++)
	{
		m_WorkCQ[i] = new ColorQueue;
		m_Combos[i] = new CColorCombo(m_WorkCQ[i]);
	}
}


//----------------------------------------------------------------------------
CColumnTitleColorsDlg::~CColumnTitleColorsDlg()
{
	for (int i = 0; i < 3; i++)
    {
		if (m_WorkCQ[i]) delete m_WorkCQ[i];
		if (m_Combos[i]) delete m_Combos[i];
		m_WorkCQ[i] = NULL;
        m_Combos[i] = NULL;
	}
}


//----------------------------------------------------------------------------
ColorQueue* CColumnTitleColorsDlg::LookupQueue(int nIndex)
{
	switch (nIndex)
	{
		case  0:	return &coltitle_label;
		case  1:	return &coltitle_backgr;
		case  2:	return &coltitle_border;
		default:	return NULL;
    }
}


//----------------------------------------------------------------------------
void CColumnTitleColorsDlg::Setup()
{
	m_edtTitleTextColorExpr.		SubclassEdit	(IDC_EDIT_FORECOLOR_EXPR,       		this);
	m_edtTitleBkgColorExpr.			SubclassEdit	(IDC_EDIT_BKGCOLOR_EXPR,	    		this);

	if (m_pTitleTextColorExpr != NULL && m_edtTitleTextColorExpr.IsWindowEnabled())
	{
		m_edtTitleTextColorExpr.SetWindowText(m_pTitleTextColorExpr->ToString());
	}
	if (m_pTitleBkgColorExpr != NULL && m_edtTitleBkgColorExpr.IsWindowEnabled())
	{
		m_edtTitleBkgColorExpr.SetWindowText(m_pTitleBkgColorExpr->ToString());
	}

	m_Combos[0]->SubclassDlgItem(IDC_COLOR_COMBO0, this);
	m_Combos[1]->SubclassDlgItem(IDC_COLOR_COMBO1, this);
	m_Combos[2]->SubclassDlgItem(IDC_COLOR_COMBO2, this);

	m_Btn.SubclassDlgItem(IDC_COLOR_BTN0,this);

	for (int i = 0; i < 3; i++)
    {
		for (int j = 0; j < 16; j++) m_Combos[i]->SendMessage(CB_ADDSTRING,0,j);
		*m_WorkCQ[i] = *LookupQueue(i);
		int nIndex = m_WorkCQ[i]->Insert(m_pMCArray[i]);
		m_Combos[i]->SetCurSel(nIndex);
	}
}


//----------------------------------------------------------------------------
BOOL CColumnTitleColorsDlg::CopyBack()
{
	for (int i = 0; i < 3; i++)
	{
		(*LookupQueue(i)) = *m_WorkCQ[i];
		m_pMCArray[i] = m_Combos[i]->GetColor();
	}

	m_bAll = m_Btn.GetCheck();

//Gestione Expressions
	BOOL bOK = TRUE;
	
	bOK = ::CheckAndSaveExpression(m_edtTitleTextColorExpr, m_pTitleTextColorExpr, m_pSymTable);
	
	bOK = bOK && ::CheckAndSaveExpression(m_edtTitleBkgColorExpr, m_pTitleBkgColorExpr, m_pSymTable);

	return bOK;
}

//////////////////////////////////////////////////////////////////////////////
//	CTotalColorsDlg Methods											//
//////////////////////////////////////////////////////////////////////////////

//----------------------------------------------------------------------------
BEGIN_MESSAGE_MAP	(CTotalColorsDlg,	ColorDlgObj)
END_MESSAGE_MAP()

//----------------------------------------------------------------------------
CTotalColorsDlg::CTotalColorsDlg(COLORREF* pMCArray, BOOL& bAll, Expression*& p_TotalTextColorExpr, Expression*& p_TotalBkgColorExpr, SymTable* pSymTable/*= NULL*/, CWnd* pParent)
	:
	ColorDlgObj				(IDD_COLOR_TOTAL, pParent),
	m_bAll					(bAll),
	m_pMCArray				(pMCArray),
	m_pTotalTextColorExpr	(p_TotalTextColorExpr),
	m_pTotalBkgColorExpr	(p_TotalBkgColorExpr),
	m_pSymTable				(pSymTable)
{
	m_edtTotalTextColorExpr. SetSymbolTable	(pSymTable);
	m_edtTotalBkgColorExpr.	SetSymbolTable(pSymTable);

	for (int i = 0; i < 3; i++)
	{
		m_WorkCQ[i] = new ColorQueue;
		m_Combos[i] = new CColorCombo(m_WorkCQ[i]);
	}
}


//----------------------------------------------------------------------------
CTotalColorsDlg::~CTotalColorsDlg()
{
	for (int i = 0; i < 3; i++)
    {
		if (m_WorkCQ[i]) delete m_WorkCQ[i];
		if (m_Combos[i]) delete m_Combos[i];
		m_WorkCQ[i] = NULL;
        m_Combos[i] = NULL;
	}
}

//----------------------------------------------------------------------------
ColorQueue* CTotalColorsDlg::LookupQueue(int nIndex)
{
	switch (nIndex)
	{
		case  0:	return &total_value;
		case  1:	return &total_backgr;
		case  2:	return &total_border;
		default:	return NULL;
    }
}


//----------------------------------------------------------------------------
void CTotalColorsDlg::Setup()
{
	m_edtTotalTextColorExpr.		SubclassEdit	(IDC_EDIT_FORECOLOR_EXPR,       		this);
	m_edtTotalBkgColorExpr.			SubclassEdit	(IDC_EDIT_BKGCOLOR_EXPR,	    		this);

	if (m_pTotalTextColorExpr != NULL && m_edtTotalTextColorExpr.IsWindowEnabled())
	{
		m_edtTotalTextColorExpr.SetWindowText(m_pTotalTextColorExpr->ToString());
	}
	if (m_pTotalBkgColorExpr != NULL && m_edtTotalBkgColorExpr.IsWindowEnabled())
	{
		m_edtTotalBkgColorExpr.SetWindowText(m_pTotalBkgColorExpr->ToString());
	}

	m_Combos[0]->SubclassDlgItem(IDC_COLOR_COMBO0, this);
	m_Combos[1]->SubclassDlgItem(IDC_COLOR_COMBO1, this);
	m_Combos[2]->SubclassDlgItem(IDC_COLOR_COMBO2, this);

	m_Btn.SubclassDlgItem(IDC_COLOR_BTN0,this);

	for (int i = 0; i < 3; i++)
    {
		for (int j = 0; j < 16; j++) m_Combos[i]->SendMessage(CB_ADDSTRING,0,j);
		*m_WorkCQ[i] = *LookupQueue(i);
		int nIndex = m_WorkCQ[i]->Insert(m_pMCArray[i]);
		m_Combos[i]->SetCurSel(nIndex);
	}
}


//----------------------------------------------------------------------------
BOOL CTotalColorsDlg::CopyBack()
{
	for (int i = 0; i < 3; i++)
	{
		(*LookupQueue(i)) = *m_WorkCQ[i];
		m_pMCArray[i] = m_Combos[i]->GetColor();
	}

	m_bAll = m_Btn.GetCheck();
	
	//Gestione Expressions
	BOOL bOK = TRUE;

	bOK = ::CheckAndSaveExpression(m_edtTotalTextColorExpr, m_pTotalTextColorExpr, m_pSymTable);

	bOK = bOK && ::CheckAndSaveExpression(m_edtTotalBkgColorExpr, m_pTotalBkgColorExpr, m_pSymTable);

	return bOK;
}

//////////////////////////////////////////////////////////////////////////////
//	CDynamicFontStyleColumnDlg Methods
//////////////////////////////////////////////////////////////////////////////
//
IMPLEMENT_DYNAMIC(CDynamicFontStyleColumnDlg, CParsedDialog)
BEGIN_MESSAGE_MAP (CDynamicFontStyleColumnDlg,	CParsedDialog)
	ON_COMMAND (IDC_COLOR_CUSTOM1, OnCustomize)
END_MESSAGE_MAP()

//----------------------------------------------------------------------------
CDynamicFontStyleColumnDlg::CDynamicFontStyleColumnDlg(Expression*& pExpr, SymTable* pSymTable, BOOL& bAll, CWoormDocMng* pDoc)
	:
	CParsedDialog		(IDD_DYNAMIC_FONTSTYLE_COLUMN),
	m_bAll				(bAll),
	m_pExpr				(pExpr),
	m_pSymTable			(pSymTable),
	m_pDocument			(pDoc)
{
	m_edtExpr.SetSymbolTable(pSymTable);
}

//----------------------------------------------------------------------------
CDynamicFontStyleColumnDlg::~CDynamicFontStyleColumnDlg()
{
}

//----------------------------------------------------------------------------
BOOL CDynamicFontStyleColumnDlg::OnInitDialog()
{
	CParsedDialog::OnInitDialog();

	m_edtExpr.SubclassEdit	(IDC_EDIT_FORECOLOR_EXPR, this);

	if (m_pExpr)
	{
		m_edtExpr.SetWindowText(m_pExpr->ToString());
	}
	return TRUE;
}

//----------------------------------------------------------------------------
void CDynamicFontStyleColumnDlg::OnOK()
{
	if (!::CheckAndSaveExpression(m_edtExpr, m_pExpr, m_pSymTable, DATA_STR_TYPE))
		return;

	m_bAll = ((CButton*)GetDlgItem(IDC_COLOR_BTN0))->GetCheck() == 1;
	EndDialog(IDOK);
}


//----------------------------------------------------------------------------
void CDynamicFontStyleColumnDlg::OnCancel()
{
	EndDialog(IDCANCEL);
}

//----------------------------------------------------------------------------
void CDynamicFontStyleColumnDlg::OnCustomize()
{
	if (!m_pDocument) return;

	FontIdx     	nFontIdx = FNT_ERROR;
	CFontStylesDlg	dialog (*(m_pDocument->m_pFontStyles), nFontIdx, FALSE, this, m_pDocument->GetNamespace(), FALSE, m_pDocument->m_Template.m_bIsTemplate);
	
	if (dialog.DoModal() != IDOK)
	{
		if (!m_pDocument->IsModified())
			m_pDocument->SetModifiedFlag(m_pDocument->m_pFormatStyles->IsModified());
		return;
	}

	if (!m_pDocument->IsModified())
		m_pDocument->SetModifiedFlag(m_pDocument->m_pFormatStyles->IsModified());

	if (nFontIdx != FNT_ERROR)
	{
		CString sName = m_pDocument->m_pFontStyles->GetStyleName(nFontIdx);
		if (!sName.IsEmpty())
		{
			::CEditReplaceSelection(m_edtExpr, '"'+sName+'"');
		}		
	}

	m_pDocument->Invalidate();
}

//////////////////////////////////////////////////////////////////////////////
//	CDynamicFormatStyleColumnDlg Methods
//////////////////////////////////////////////////////////////////////////////
//
BEGIN_MESSAGE_MAP (CDynamicFormatStyleColumnDlg,	CParsedDialog)
	ON_COMMAND (IDC_COLOR_CUSTOM0, OnCustomize)
END_MESSAGE_MAP()

//----------------------------------------------------------------------------
CDynamicFormatStyleColumnDlg::CDynamicFormatStyleColumnDlg(Expression*& pExpr, SymTable* pSymTable, CWoormDocMng* pDoc, DataType dt)
	:
	CParsedDialog		(IDD_DYNAMIC_FONTSTYLE_COLUMN),
	m_pExpr				(pExpr),
	m_pSymTable			(pSymTable),
	m_pDocument			(pDoc),
	m_dt				(dt)
{
	m_edtExpr.SetSymbolTable(pSymTable);
}

//----------------------------------------------------------------------------
CDynamicFormatStyleColumnDlg::~CDynamicFormatStyleColumnDlg()
{
}

//----------------------------------------------------------------------------
BOOL CDynamicFormatStyleColumnDlg::OnInitDialog()
{
	GetDlgItem(IDC_COLOR_BTN0)->ShowWindow(SW_HIDE);
	CParsedDialog::OnInitDialog();

	m_edtExpr.SubclassEdit	(IDC_EDIT_FORECOLOR_EXPR, this);

	if (m_pExpr)
	{
		m_edtExpr.SetWindowText(m_pExpr->ToString());
	}
	return TRUE;
}

//----------------------------------------------------------------------------
void CDynamicFormatStyleColumnDlg::OnOK()
{
	if (!::CheckAndSaveExpression(m_edtExpr, m_pExpr, m_pSymTable, DATA_STR_TYPE))
		return;

	EndDialog(IDOK);
}

//----------------------------------------------------------------------------
void CDynamicFormatStyleColumnDlg::OnCancel()
{
	EndDialog(IDCANCEL);
}

//----------------------------------------------------------------------------
void CDynamicFormatStyleColumnDlg::OnCustomize()
{
	if (!m_pDocument) return;

	FormatIdx     	nIdx = m_pDocument->m_pFormatStyles->GetFormatIdx(m_dt);
	CFormatDlg	dialog (*(m_pDocument->m_pFormatStyles), nIdx, FALSE, this, m_pDocument->GetNamespace(), FALSE);
	
	if (dialog.DoModal() != IDOK)
	{
		if (!m_pDocument->IsModified())
			m_pDocument->SetModifiedFlag(m_pDocument->m_pFormatStyles->IsModified());
		return;
	}

	if (!m_pDocument->IsModified())
		m_pDocument->SetModifiedFlag(m_pDocument->m_pFormatStyles->IsModified());

	if (nIdx != FNT_ERROR)
	{
		CString sName = m_pDocument->m_pFormatStyles->GetStyleName(nIdx);
		if (!sName.IsEmpty())
		{
			::CEditReplaceSelection(m_edtExpr, '"' + sName + '"');
		}		
	}

	m_pDocument->Invalidate();
}

//////////////////////////////////////////////////////////////////////////////
//	CDynamicCellBordersDlg Methods
//////////////////////////////////////////////////////////////////////////////
//
BEGIN_MESSAGE_MAP (CDynamicCellBordersDlg,	CParsedDialog)
END_MESSAGE_MAP()

//----------------------------------------------------------------------------
CDynamicCellBordersDlg::CDynamicCellBordersDlg(Expression*& pExpr, SymTable* pSymTable, BOOL& bAll, CWoormDocMng* pDoc/*=NULL*/)
	:
	CParsedDialog		(IDD_DYNAMIC_CELL_BORDERS),
	m_bAll				(bAll),
	m_pExpr				(pExpr),
	m_pSymTable			(pSymTable),
	m_pDocument			(pDoc)
{
	m_edtExpr.SetSymbolTable(pSymTable);
}

//----------------------------------------------------------------------------
CDynamicCellBordersDlg::~CDynamicCellBordersDlg()
{
}

//----------------------------------------------------------------------------
BOOL CDynamicCellBordersDlg::OnInitDialog()
{
	CParsedDialog::OnInitDialog();

	m_edtExpr.SubclassEdit	(IDC_EDIT_FORECOLOR_EXPR, this);

	if (m_pExpr)
	{
		m_edtExpr.SetWindowText(m_pExpr->ToString());
	}
	return TRUE;
}

//----------------------------------------------------------------------------
void CDynamicCellBordersDlg::OnOK()
{
	if (!::CheckAndSaveExpression(m_edtExpr, m_pExpr, m_pSymTable, DATA_STR_TYPE))
		return;

	m_bAll = ((CButton*)GetDlgItem(IDC_COLOR_BTN0))->GetCheck() == 1;
	EndDialog(IDOK);
}


//----------------------------------------------------------------------------
void CDynamicCellBordersDlg::OnCancel()
{
	EndDialog(IDCANCEL);
}

//////////////////////////////////////////////////////////////////////////////
//	CDropShadowDlg Methods
//////////////////////////////////////////////////////////////////////////////
//
IMPLEMENT_DYNAMIC(CDropShadowDlg, CParsedDialog)
BEGIN_MESSAGE_MAP (CDropShadowDlg,	CParsedDialog)
END_MESSAGE_MAP()

CDropShadowDlg::CDropShadowDlg  (COLORREF crDropShadowColor, int nDropShadowHeight)
	:
	CParsedDialog		(IDD_DROP_SHADOW),
	m_Color	(crDropShadowColor),
	m_Height (nDropShadowHeight)
{
}

CDropShadowDlg::~CDropShadowDlg ()
{
}

BOOL CDropShadowDlg::OnInitDialog()
{
	CParsedDialog::OnInitDialog();

	EnableToolTips(TRUE);

	CParsedCtrl* p = ::GetParsedCtrl(&m_edtColor);
	p->Attach(&m_Color);
	m_edtColor.Attach(BTN_COLOR_ID);
	m_edtColor.SubclassEdit(IDC_DROP_SHADOW_COLOR, this);

	p = ::GetParsedCtrl(&m_edtHeight);
	p->Attach(&m_Height);
	m_edtHeight.SubclassEdit(IDC_DROP_SHADOW_HEIGHT, this);

	m_edtColor.UpdateCtrlStatus();
	m_edtColor.UpdateCtrlView();

	m_edtHeight.UpdateCtrlStatus();
	m_edtHeight.UpdateCtrlView();

	return TRUE;
}

void    CDropShadowDlg::OnOK		()
{
	if (!CheckForm())
		return;
	EndDialog(IDOK);
}

void    CDropShadowDlg::OnCancel	()
{
	EndDialog(IDCANCEL);
}
