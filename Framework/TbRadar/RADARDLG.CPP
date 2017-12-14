
#include "stdafx.h"

#include <TbGeneric\GeneralFunctions.h>
#include <TbGeneric\DataObj.h>

#include <TbGenlib\parsctrl.h>
#include <TbGenlib\baseapp.h>

#include "radarbdy.h"
#include "radardlg.h"

#include "radar.hrc"    //risorse non ancora gestite nel JSON
#include "radar.hjson" //JSON AUTOMATIC UPDATE


//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char BASED_CODE THIS_FILE[] = __FILE__;
#endif

///////////////////////////////////////////////////////////////////////////////
// Implementazione di RadarFindCondition
///////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
RadarFindCondition::RadarFindCondition()
	:
	m_pDataObj		(NULL),
	m_bDown			(TRUE),
	m_nNumCond		(ECompareType::CMP_EQUAL)
{
	m_pBufferSelCols = new RadarIntArray;
}

//-----------------------------------------------------------------------------
RadarFindCondition::~RadarFindCondition()
{
	if (m_pDataObj)	delete m_pDataObj;

	ASSERT_VALID (m_pBufferSelCols);
	delete m_pBufferSelCols;
}
                                                                    
//-----------------------------------------------------------------------------
void RadarFindCondition::AddSelCols(int nIdx)
{
	m_pBufferSelCols->AddInt (nIdx);
}

//-----------------------------------------------------------------------------
int RadarFindCondition::GetSelCol(int nIdx)
{
	return m_pBufferSelCols->GetInt(nIdx);
}

//-----------------------------------------------------------------------------
int RadarFindCondition::GetSizeSelCols()
{
	return m_pBufferSelCols->GetSize();
}

//-----------------------------------------------------------------------------
void RadarFindCondition::RemoveAll()
{
	m_pBufferSelCols->RemoveAll();
}

///////////////////////////////////////////////////////////////////////////////
// Diagnostics
#ifdef _DEBUG
void RadarFindCondition::Dump (CDumpContext& dc) const
{
	ASSERT_VALID (this);
	AFX_DUMP0(dc, " RadarFindCondition\n");
	CObject::Dump(dc);
}
#endif // _DEBUG

///////////////////////////////////////////////////////////////////////////////
// Implementazione di CRadarInsertColumnsDlg
///////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(CRadarInsertColumnsDlg, CParsedDialog)
BEGIN_MESSAGE_MAP(CRadarInsertColumnsDlg, CParsedDialog)
	//{{AFX_MSG_MAP(CRadarInsertColumnsDlg)
	ON_BN_CLICKED (IDC_RADAR_INS_BT_DES_ALL,	OnBtnDesAllColumns)
	ON_BN_CLICKED (IDC_RADAR_INS_BT_SEL_ALL,	OnBtnSelAllColumns)
	ON_BN_CLICKED (IDC_RADAR_INS_CKB_MOUSE,		OnCBWithMouse)

	ON_LBN_SELCHANGE (IDC_RADAR_INS_LISTBOX,	OnListBoxSelChange)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
CRadarInsertColumnsDlg::CRadarInsertColumnsDlg
	(
		CRadarDoc*				pRadarDoc, 
		RadarColumnsIdxArray*	pDelCols,
		RadarColumnsIdxArray*	pColsToInsert,
		BOOL					bOneColumnSel,
		CWnd*					pParent
	)
	:
	CParsedDialog	(IDD_RADAR_INSERT_DELETED_COLUMNS, pParent),
	m_pRadarDoc		(pRadarDoc),
	m_bOneColumnSel	(bOneColumnSel),
	m_pColsToInsert	(pColsToInsert),
	m_pDeletedCols	(pDelCols)
{
}

//-----------------------------------------------------------------------------
BOOL CRadarInsertColumnsDlg::OnInitDialog ()
{
	CParsedDialog::OnInitDialog();

	// Subclass dei control della dialog
	VERIFY (m_BtnDesAllFileds.	SubclassDlgItem (IDC_RADAR_INS_BT_DES_ALL,	this));
	VERIFY (m_BtnSelAllFileds.	SubclassDlgItem (IDC_RADAR_INS_BT_SEL_ALL,	this));
	VERIFY (m_CBWithMouse.		SubclassDlgItem (IDC_RADAR_INS_CKB_MOUSE,	this));
	VERIFY (m_LBDeletedCols.	SubclassDlgItem (IDC_RADAR_INS_LISTBOX,		this));
	VERIFY (m_RBHeadTable.		SubclassDlgItem (IDC_RADAR_INS_RB_HEAD,		this));
	VERIFY (m_RBPrevSelCol.		SubclassDlgItem (IDC_RADAR_INS_RB_PREV_COL,	this));
	VERIFY (m_RBNextSelCol.		SubclassDlgItem (IDC_RADAR_INS_RB_NEXT_COL,	this));
	VERIFY (m_RBTailTable.		SubclassDlgItem (IDC_RADAR_INS_RB_TAIL,		this));

	// Inizializzazione di valori di default per i control che lo prevedono
	m_RBTailTable. SetCheck(1);
	for (int nCol = 0; nCol < m_pDeletedCols->GetSize(); nCol++)
	{
		RadarColumnIdxObj* pRiCol = (*m_pDeletedCols)[nCol];

		CString s = pRiCol->GetTitle();
		int nIdx = m_LBDeletedCols.AddString(s);

		m_LBDeletedCols.SetItemData(nIdx, nCol);
	}

	CenterWindow();
	return TRUE;
}

//-----------------------------------------------------------------------------
void CRadarInsertColumnsDlg::OnBtnDesAllColumns ()
{
	m_LBDeletedCols.SetSel (-1, FALSE);
	EnableTypeAction (0);
}

//-----------------------------------------------------------------------------
void CRadarInsertColumnsDlg::OnBtnSelAllColumns ()
{
	m_LBDeletedCols.SetSel (-1);
	EnableTypeAction (m_pDeletedCols->GetSize());
}

//-----------------------------------------------------------------------------
void CRadarInsertColumnsDlg::OnCBWithMouse()
{
	EnableRadioButton (!m_CBWithMouse.GetCheck());
}

//-----------------------------------------------------------------------------
void CRadarInsertColumnsDlg::OnListBoxSelChange()
{
	EnableTypeAction (m_LBDeletedCols.GetSelCount());
}

//-----------------------------------------------------------------------------
void CRadarInsertColumnsDlg::OnOK ()
{
	int nNrSel = m_LBDeletedCols.GetSelCount();

	// Non ci sono colonna d selezionate
	if (nNrSel == 0)
	{
		CParsedDialog::OnOK();
		return;
	}

	ASSERT_VALID (m_pColsToInsert);
	ASSERT_VALID (m_pDeletedCols);

	// Aggiorno il contenuto dell'aray delle colonne da inserire
	// Inizializza il flag di selezione per le colonne da inserire nuovamente nel radar
	int*	pSelItems	= (int *) malloc(sizeof(int) * nNrSel);
	int		nNrSelItems	= m_LBDeletedCols.GetSelItems (nNrSel, pSelItems);
	int nIdx = 0;
	for (nIdx = 0; nIdx < nNrSelItems; nIdx++)
	{
		int nIdxArray = (int) m_LBDeletedCols.GetItemData (pSelItems[nIdx]);
		m_pColsToInsert->Add	(m_pDeletedCols->GetAt (nIdxArray));
		m_pDeletedCols->SetAt	(nIdxArray, NULL);
	}

    // Rimuovo gli elementi non più validi pechè trasferiti nell'array per l'inserimento
	for (nIdx = 0; nIdx < m_pDeletedCols->GetSize (); )
	{
		if (m_pDeletedCols->GetAt (nIdx))
		{
			nIdx++;
			continue;
		}
		m_pDeletedCols->RemoveAt (nIdx);
	}

	// Deallocazione della memoria precendentemente allocata dall'array di interi
	free (pSelItems);

	// Ritorna l'ID del control che l'utente a selezionato per le modalità di inserimento
	// delle colonna cancellate
	if		(m_CBWithMouse.	GetCheck()) EndDialog (IDC_RADAR_INS_CKB_MOUSE);
	else if (m_RBPrevSelCol.GetCheck())	EndDialog (IDC_RADAR_INS_RB_PREV_COL);
	else if (m_RBNextSelCol.GetCheck())	EndDialog (IDC_RADAR_INS_RB_NEXT_COL);
	else if (m_RBHeadTable.	GetCheck())	EndDialog (IDC_RADAR_INS_RB_HEAD);
	else EndDialog (IDC_RADAR_INS_RB_TAIL);
}

//-----------------------------------------------------------------------------
void CRadarInsertColumnsDlg::EnableRadioButton (BOOL bEnable)
{
	// Abilita / Disabilita i diversi control in accordo le colonne selezionate
	// da inserire
	m_RBHeadTable.EnableWindow (bEnable);
	m_RBTailTable.EnableWindow (bEnable);

	// Se è stata selezionata un sola colonna nel corpo del radar allora abilito
	// / disabilito i control che permettono l'inserimento delle colonne cancellate
	// primo a dopo l'attuale colonna.
	if (m_bOneColumnSel)
	{
		m_RBNextSelCol.EnableWindow (bEnable);
		m_RBPrevSelCol.EnableWindow (bEnable);
	}
}

// Abilita / Disabilita i diversi control in base al numero di colonne selezionate
// nella combo box
//-----------------------------------------------------------------------------
void CRadarInsertColumnsDlg::EnableTypeAction (int nSelCount)
{
	m_BtnSelAllFileds.EnableWindow (nSelCount == m_pDeletedCols->GetSize() ? FALSE : TRUE);
	m_BtnDesAllFileds.EnableWindow (nSelCount);
	m_CBWithMouse.EnableWindow (nSelCount);
	EnableRadioButton (!m_CBWithMouse.GetCheck());
}

///////////////////////////////////////////////////////////////////////////////
// Diagnostics
#ifdef _DEBUG
void CRadarInsertColumnsDlg::Dump (CDumpContext& dc) const
{
	ASSERT_VALID (this);
	AFX_DUMP0(dc, " CRadarInsertColumnsDlg\n");
	CParsedDialog::Dump(dc);
}
#endif // _DEBUG

///////////////////////////////////////////////////////////////////////////////
// CRadarResizeColumnsDlg
//-----------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(CRadarResizeColumnsDlg, CParsedDialog)
BEGIN_MESSAGE_MAP(CRadarResizeColumnsDlg, CParsedDialog)
	//{{AFX_MSG_MAP(CRadarResizeColumnsDlg)
	ON_BN_CLICKED (IDC_RADAR_RESIZE_CUSTOM,		OnRBCustom)
	ON_BN_CLICKED (IDC_RADAR_RESIZE_MIN,		OnRBCustomSizeDisable)
	ON_BN_CLICKED (IDC_RADAR_RESIZE_DEFAULT,	OnRBCustomSizeDisable)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
CRadarResizeColumnsDlg::CRadarResizeColumnsDlg
	(
		RadarBody*		pRadarBody,
		RadarIntArray*	pWidthCols,
		CWnd*			pParent
	)
	:
	CParsedDialog	(IDD_RADAR_RESIZE_COLUMNS, pParent),
	m_pWidthCols	(pWidthCols),
	m_pRadarBody 	(pRadarBody)
{
	ASSERT_VALID (m_pRadarBody);
	ASSERT_VALID (m_pWidthCols);
	m_pEdtCustomWidth = new CIntEdit (BTN_SPIN_ID);
}

//-----------------------------------------------------------------------------
CRadarResizeColumnsDlg::~CRadarResizeColumnsDlg ()
{
	ASSERT (m_pEdtCustomWidth);
	delete m_pEdtCustomWidth;
}

//-----------------------------------------------------------------------------
BOOL CRadarResizeColumnsDlg::OnInitDialog ()
{
	CParsedDialog::OnInitDialog();

	// SubClass dei control presenti nella dialog
	VERIFY (m_RBAllCols.		SubclassDlgItem (IDC_RADAR_RESIZE_ALL,		this));
	VERIFY (m_RBOnlySelCols.	SubclassDlgItem (IDC_RADAR_RESIZE_ONLYSEL,	this));
	VERIFY (m_RBMinWidth.		SubclassDlgItem (IDC_RADAR_RESIZE_MIN,		this));
	VERIFY (m_RBDefaultWidth.	SubclassDlgItem (IDC_RADAR_RESIZE_DEFAULT,	this));
	VERIFY (m_RBCustomWidth.	SubclassDlgItem (IDC_RADAR_RESIZE_CUSTOM,	this));

	VERIFY (m_pEdtCustomWidth->	SubclassEdit	(IDC_RADAR_RESIZE_SIZE,		this));

	// Inizializzazione e abilitazione dei control con i valori di default
	if (m_pRadarBody->GetNrSelColumns())
		m_RBOnlySelCols.SetCheck(1);
	else
	{
		m_RBAllCols.SetCheck(1);
		m_RBOnlySelCols.EnableWindow(FALSE);
	}
	m_pEdtCustomWidth->SetRange		(COLUMN_CHAR_WIDTH_MIN_VALUE, COLUMN_CHAR_WIDTH_MAX_VALUE);
	m_pEdtCustomWidth->EnableWindow	(FALSE);
	m_RBMinWidth.SetCheck(1);

	CenterWindow();
	return TRUE;
}

//-----------------------------------------------------------------------------
void CRadarResizeColumnsDlg::OnRBCustomSizeDisable()
{
	m_pEdtCustomWidth->EnableWindow	(FALSE);
	m_pEdtCustomWidth->SetValue (0);
	GetDlgItem(IDC_RADAR_RESIZE_STSIZE)->EnableWindow(FALSE);
}

//-----------------------------------------------------------------------------
void CRadarResizeColumnsDlg::OnRBCustom()
{
	m_pEdtCustomWidth->EnableWindow	();
	m_pEdtCustomWidth->SetValue(3);
	GetDlgItem(IDC_RADAR_RESIZE_STSIZE)->EnableWindow();
}

//-----------------------------------------------------------------------------
void CRadarResizeColumnsDlg::OnOK ()
{
	ASSERT_VALID (m_pRadarBody);
	ASSERT_VALID (m_pWidthCols);

	// Aggiornamento effettivo della dimensione della colonna in base alle scelte dell'utente
	if (m_RBMinWidth.GetCheck())
		m_pRadarBody->UpdateColumnsWidth (m_RBAllCols.GetCheck(), COLUMN_CHAR_WIDTH_MIN_VALUE);
	else if (m_RBCustomWidth.GetCheck())
		m_pRadarBody->UpdateColumnsWidth (m_RBAllCols.GetCheck(), m_pEdtCustomWidth->GetValue ());
	else
		m_pRadarBody->UpdateColumnsWidth (m_RBAllCols.GetCheck(), m_pWidthCols);

	CParsedDialog::OnOK();
}

///////////////////////////////////////////////////////////////////////////////
// Diagnostics
#ifdef _DEBUG
void CRadarResizeColumnsDlg::Dump (CDumpContext& dc) const
{
	ASSERT_VALID (this);
	AFX_DUMP0(dc, " CRadarResizeColumnsDlg\n");
	CParsedDialog::Dump(dc);
}
#endif // _DEBUG

///////////////////////////////////////////////////////////////////////////////
// CRadarFindDlg
//-----------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(CRadarFindDlg, CParsedDialog)
BEGIN_MESSAGE_MAP(CRadarFindDlg, CParsedDialog)
	//{{AFX_MSG_MAP(CRadarFindDlg)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
CRadarFindDlg::CRadarFindDlg
	(
		UINT				nIDD,
		const CString&		strTitle,
		UINT				nIDCSub,
		RadarFindCondition* pCond,
		CWnd*				pParent
	)
	:
	CParsedDialog	(nIDD, pParent),
	m_nIDCSub		(nIDCSub),
	m_pParsedCtrl	(NULL),
	m_pCond			(pCond)
{
	ASSERT (!strTitle.IsEmpty());
	m_strTitle = strTitle;

	ASSERT (nIDCSub		> 0);
	ASSERT_VALID (pCond);
	VERIFY (m_pTmpDataObj = pCond->m_pDataObj->DataObjClone());
}

//-----------------------------------------------------------------------------
CRadarFindDlg::~CRadarFindDlg()
{
	// NON CANCELLARE ASSOLUTAMENTE IL MEMBRO DI CLASSE m_pTmpDataObj
	ASSERT (m_pParsedCtrl);
	delete m_pParsedCtrl;
}

//-----------------------------------------------------------------------------
void CRadarFindDlg::OnCancel()
{
	ASSERT (m_pTmpDataObj);

	EndDialog(IDCANCEL);

	delete m_pTmpDataObj;
	m_pTmpDataObj = NULL;
}

//-----------------------------------------------------------------------------
void CRadarFindDlg::ControlChange()
{
	GetDlgItem(IDOK)->EnableWindow(TRUE);
}

//-----------------------------------------------------------------------------
void CRadarFindDlg::OnOK()
{
	ASSERT (m_pCond->m_pDataObj);
    delete m_pCond->m_pDataObj;

	m_pCond->m_pDataObj	= m_pTmpDataObj;
	m_pCond->m_bDown	= m_FindDown.GetCheck();

	CParsedDialog::OnOK();
}

//-----------------------------------------------------------------------------
BOOL CRadarFindDlg::OnInitDialog ()
{
	CParsedDialog::OnInitDialog();

	ASSERT (m_pParsedCtrl);
	VERIFY (m_pParsedCtrl->SubclassEdit (m_nIDCSub, this));

	// Visualizzazione del titolo corretto nella Caption della dialog
	SetWindowText (m_strTitle);

	// Subclass dei control presenti in tutti le dialog di ricerca stupida
	VERIFY (m_FindUp.	SubclassDlgItem (IDC_RADAR_FIND_UP,		this));
	VERIFY (m_FindDown.	SubclassDlgItem (IDC_RADAR_FIND_DOWN,	this));

	if (m_pCond->m_bDown)
		m_FindDown.SetCheck(TRUE);
	else
		m_FindUp.SetCheck(TRUE);

	InitDialog();

	m_pParsedCtrl->UpdateCtrlView ();
	CenterWindow();
	return TRUE;
}

///////////////////////////////////////////////////////////////////////////////
// Diagnostics
#ifdef _DEBUG
void CRadarFindDlg::Dump (CDumpContext& dc) const
{
	ASSERT_VALID(this);
	AFX_DUMP0	(dc, " CRadarFindDlg\n");
	CParsedDialog::Dump(dc);
}
#endif // _DEBUG

///////////////////////////////////////////////////////////////////////////////
// CRadarFindStrDlg
//-----------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CRadarFindStrDlg, CRadarFindDlg)
	//{{AFX_MSG_MAP(CRadarFindStrDlg)
	ON_EN_UPDATE (IDC_RADAR_FIND_EDIT, OnControlUpdate)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
CRadarFindStrDlg::CRadarFindStrDlg (RadarFindCondition* pCond, CWnd* pParent)
	:
	CRadarFindDlg(
						IDD_RADAR_FIND_STR,
						_TB("Search for data of type String"),
						IDC_RADAR_FIND_EDIT,
						pCond,
						pParent
					)
{
	m_pParsedCtrl = new CStrEdit (NO_BUTTON, (DataStr*)	m_pTmpDataObj);
}

//-----------------------------------------------------------------------------
void CRadarFindStrDlg::InitDialog ()
{
	// Subclass dei control che particano alla ricerca per il tipo di dato
	// stringa
	VERIFY (m_MatchCase.SubclassDlgItem (IDC_RADAR_FIND_MATCH_CASE,	this));
	VERIFY (m_MatchWord.SubclassDlgItem (IDC_RADAR_FIND_MATCH_WORD,	this));

	//// Inizializzazione del loro stato
	//m_MatchWord.SetCheck(m_pCond->m_bMatchWord);
	//m_MatchCase.SetCheck(m_pCond->m_bMatchCase);
}

//-----------------------------------------------------------------------------
void CRadarFindStrDlg::OnOK()
{
	/*m_pCond->m_bMatchWord = m_MatchWord.GetCheck();
	m_pCond->m_bMatchCase =	m_MatchCase.GetCheck();*/

	CRadarFindDlg::OnOK();
}

//-----------------------------------------------------------------------------
void CRadarFindStrDlg::OnControlUpdate()
{
	CString str;
	GetDlgItem(IDC_RADAR_FIND_EDIT)->GetWindowText(str);
	GetDlgItem(IDOK)->EnableWindow(!str.IsEmpty());
}

///////////////////////////////////////////////////////////////////////////////
// Diagnostics
#ifdef _DEBUG
void CRadarFindStrDlg::Dump (CDumpContext& dc) const
{
	ASSERT_VALID (this);
	AFX_DUMP0(dc, " CRadarFindStrDlg\n");
	CRadarFindDlg::Dump(dc);
}
#endif // _DEBUG

///////////////////////////////////////////////////////////////////////////////
// CRadarFindEnumDlg
//-----------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CRadarFindEnumDlg, CRadarFindDlg)
	//{{AFX_MSG_MAP(CRadarFindEnumDlg)
	ON_CBN_SELCHANGE (IDC_RADAR_FIND_COMBO, OnControlChange)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
CRadarFindEnumDlg::CRadarFindEnumDlg (RadarFindCondition* pCond, CWnd* pParent)
	:
	CRadarFindDlg(
						IDD_RADAR_FIND_ENUM,
						_TB("Search for data of type Enum"),
						IDC_RADAR_FIND_COMBO,
						pCond,
						pParent
					)
{
	m_pParsedCtrl = new CEnumCombo (NO_BUTTON, (DataEnum*)	m_pTmpDataObj);
}

///////////////////////////////////////////////////////////////////////////////
// Diagnostics
#ifdef _DEBUG
void CRadarFindEnumDlg::Dump (CDumpContext& dc) const
{
	ASSERT_VALID (this);
	AFX_DUMP0(dc, " CRadarFindEnumDlg\n");
	CRadarFindDlg::Dump(dc);
}
#endif // _DEBUG

///////////////////////////////////////////////////////////////////////////////
// CRadarFindBoolDlg
//-----------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CRadarFindBoolDlg, CRadarFindDlg)
	//{{AFX_MSG_MAP(CRadarFindDateDlg)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
CRadarFindBoolDlg::CRadarFindBoolDlg (RadarFindCondition* pCond, CWnd* pParent)
	:
	CRadarFindDlg (IDD_RADAR_FIND_NUMERIC, _TB("Search for data of type Logic"), IDC_RADAR_FIND_EDIT, pCond, pParent)
{
	m_pParsedCtrl = new CBoolEdit (NO_BUTTON, (DataBool*) m_pTmpDataObj);
}

//-----------------------------------------------------------------------------
void CRadarFindBoolDlg::InitDialog ()
{
	GetDlgItem(IDC_RADAR_FIND_COMBO_NUM_TYPE)->ShowWindow(SW_HIDE);
}

///////////////////////////////////////////////////////////////////////////////
// Diagnostics
#ifdef _DEBUG
void CRadarFindBoolDlg::Dump (CDumpContext& dc) const
{
	ASSERT_VALID (this);
	AFX_DUMP0(dc, " CRadarFindDlg\n");
	CRadarFindDlg::Dump(dc);
}
#endif // _DEBUG

///////////////////////////////////////////////////////////////////////////////
// CRadarFindNumDlg
//-----------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CRadarFindNumDlg, CRadarFindDlg)
	//{{AFX_MSG_MAP(CRadarFindNumDlg)
	ON_EN_CHANGE (IDC_RADAR_FIND_EDIT, OnControlChange)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
CRadarFindNumDlg::CRadarFindNumDlg
							(
								RadarFindCondition* pCond,
								const CString&		strTitle,
								CWnd*				pParent		/* NULL */
							)
	:
	CRadarFindDlg(IDD_RADAR_FIND_NUMERIC, strTitle, IDC_RADAR_FIND_EDIT, pCond, pParent)
{
}

//-----------------------------------------------------------------------------
void CRadarFindNumDlg::InitDialog ()
{
	VERIFY (m_cbCondition.SubclassDlgItem (IDC_RADAR_FIND_COMBO_NUM_TYPE, this));

	// Carica le stringhe da visualizzare nella combo box sul tipo di ricerca da
	// eseguire
	m_cbCondition.AddString(_TB("Equal"));
	m_cbCondition.AddString(_TB("Not equal to"));
	m_cbCondition.AddString(_TB("Less than"));
	m_cbCondition.AddString(_TB("Less than or equal to"));
	m_cbCondition.AddString(_TB("Greater than"));
	m_cbCondition.AddString(_TB("Greater than or equal to"));

	// Inizializza le condizioni di ricerca standard
	ASSERT (m_pCond->m_nNumCond < m_cbCondition.GetCount());
	m_cbCondition.SetCurSel((int)m_pCond->m_nNumCond);
}

//-----------------------------------------------------------------------------
void CRadarFindNumDlg::OnOK()
{
	int nNewCond = m_cbCondition.GetCurSel();
	if (nNewCond >= 0)
		m_pCond->m_nNumCond = (ECompareType) nNewCond;

	CRadarFindDlg::OnOK();
}

///////////////////////////////////////////////////////////////////////////////
// Diagnostics
#ifdef _DEBUG
void CRadarFindNumDlg::Dump (CDumpContext& dc) const
{
	ASSERT_VALID (this);
	AFX_DUMP0(dc, " CRadarFindNumDlg\n");
	CRadarFindDlg::Dump(dc);
}
#endif // _DEBUG

///////////////////////////////////////////////////////////////////////////////
// CRadarFindIntDlg
//-----------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CRadarFindIntDlg, CRadarFindNumDlg)
	//{{AFX_MSG_MAP(CRadarFindIntDlg)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
CRadarFindIntDlg::CRadarFindIntDlg (RadarFindCondition* pCond, CWnd* pParent)
	:
	CRadarFindNumDlg (pCond, _TB("Search for data of type Integer"), pParent)
{
	m_pParsedCtrl = new CIntEdit (NO_BUTTON, (DataInt*) m_pTmpDataObj);
}

///////////////////////////////////////////////////////////////////////////////
// Diagnostics
#ifdef _DEBUG
void CRadarFindIntDlg::Dump (CDumpContext& dc) const
{
	ASSERT_VALID (this);
	AFX_DUMP0(dc, " CRadarFindIntDlg\n");
	CRadarFindNumDlg::Dump(dc);
}
#endif // _DEBUG

///////////////////////////////////////////////////////////////////////////////
// CRadarFindLngDlg
//-----------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CRadarFindLngDlg, CRadarFindNumDlg)
	//{{AFX_MSG_MAP(CRadarFindLngDlg)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
CRadarFindLngDlg::CRadarFindLngDlg (RadarFindCondition* pCond, CWnd* pParent)
	:
	CRadarFindNumDlg (pCond, _TB("Search for data of type Extended"), pParent)
{
	m_pParsedCtrl = new CLongEdit (NO_BUTTON, (DataLng*) m_pTmpDataObj);
}

///////////////////////////////////////////////////////////////////////////////
// Diagnostics
#ifdef _DEBUG
void CRadarFindLngDlg::Dump (CDumpContext& dc) const
{
	ASSERT_VALID (this);
	AFX_DUMP0(dc, " CRadarFindLngDlg\n");
	CRadarFindNumDlg::Dump(dc);
}
#endif // _DEBUG

///////////////////////////////////////////////////////////////////////////////
// CRadarFindDblDlg
//-----------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CRadarFindDblDlg, CRadarFindNumDlg)
	//{{AFX_MSG_MAP(CRadarFindDblDlg)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
CRadarFindDblDlg::CRadarFindDblDlg (RadarFindCondition* pCond, CWnd* pParent)
	:
	CRadarFindNumDlg (pCond, _TB("Search for data of type Real"), pParent)
{
	m_pParsedCtrl = new CDoubleEdit (NO_BUTTON, (DataDbl*) m_pTmpDataObj);
}

///////////////////////////////////////////////////////////////////////////////
// Diagnostics
#ifdef _DEBUG
void CRadarFindDblDlg::Dump (CDumpContext& dc) const
{
	ASSERT_VALID (this);
	AFX_DUMP0(dc, " CRadarFindDblDlg\n");
	CRadarFindNumDlg::Dump(dc);
}
#endif // _DEBUG

///////////////////////////////////////////////////////////////////////////////
// CRadarFindDateDlg
//-----------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CRadarFindDateDlg, CRadarFindNumDlg)
	//{{AFX_MSG_MAP(CRadarFindDateDlg)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
CRadarFindDateDlg::CRadarFindDateDlg (RadarFindCondition* pCond, CWnd* pParent)
	:
	CRadarFindNumDlg(pCond, _TB("Search for data of type Data"), pParent)
{
	m_pParsedCtrl = new CDateEdit (NO_BUTTON, (DataDate*) m_pTmpDataObj);
}

///////////////////////////////////////////////////////////////////////////////
// Diagnostics
#ifdef _DEBUG
void CRadarFindDateDlg::Dump (CDumpContext& dc) const
{
	ASSERT_VALID (this);
	AFX_DUMP0(dc, " CRadarFindDateDlg\n");
	CRadarFindNumDlg::Dump(dc);
}
#endif // _DEBUG

///////////////////////////////////////////////////////////////////////////////
// CRadarFindMonDlg
//-----------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CRadarFindMonDlg, CRadarFindNumDlg)
	//{{AFX_MSG_MAP(CRadarFindMonDlg)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
CRadarFindMonDlg::CRadarFindMonDlg (RadarFindCondition* pCond, CWnd* pParent)
	:
	CRadarFindNumDlg (pCond, _TB("Search for data of type Real"), pParent)
{
	m_pParsedCtrl = new CMoneyEdit (NO_BUTTON, (DataMon*) m_pTmpDataObj);
}

///////////////////////////////////////////////////////////////////////////////
// Diagnostics
#ifdef _DEBUG
void CRadarFindMonDlg::Dump (CDumpContext& dc) const
{
	ASSERT_VALID (this);
	AFX_DUMP0(dc, " CRadarFindMonDlg\n");
	CRadarFindNumDlg::Dump(dc);
}
#endif // _DEBUG

///////////////////////////////////////////////////////////////////////////////
// CRadarFindQtaDlg
//-----------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CRadarFindQtaDlg, CRadarFindNumDlg)
	//{{AFX_MSG_MAP(CRadarFindQtaDlg)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
CRadarFindQtaDlg::CRadarFindQtaDlg (RadarFindCondition* pCond, CWnd* pParent)
	:
	CRadarFindNumDlg (pCond, _TB("Search for data of type Real"), pParent)
{
	m_pParsedCtrl = new CQuantityEdit (NO_BUTTON, (DataQty*) m_pTmpDataObj);
}

///////////////////////////////////////////////////////////////////////////////
// Diagnostics
#ifdef _DEBUG
void CRadarFindQtaDlg::Dump (CDumpContext& dc) const
{
	ASSERT_VALID (this);
	AFX_DUMP0(dc, " CRadarFindQtaDlg\n");
	CRadarFindNumDlg::Dump(dc);
}
#endif // _DEBUG

///////////////////////////////////////////////////////////////////////////////
// CRadarFindPercDlg
//-----------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CRadarFindPercDlg, CRadarFindNumDlg)
	//{{AFX_MSG_MAP(CRadarFindPercDlg)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
CRadarFindPercDlg::CRadarFindPercDlg (RadarFindCondition* pCond, CWnd* pParent)
	:
	CRadarFindNumDlg (pCond, _TB("Search for data of type Real"), pParent)
{
	m_pParsedCtrl = new CPercEdit (NO_BUTTON, (DataPerc*) m_pTmpDataObj);
}

///////////////////////////////////////////////////////////////////////////////
// Diagnostics
#ifdef _DEBUG
void CRadarFindPercDlg::Dump (CDumpContext& dc) const
{
	ASSERT_VALID (this);
	AFX_DUMP0(dc, " CRadarFindPercDlg\n");
	CRadarFindNumDlg::Dump(dc);
}
#endif // _DEBUG
