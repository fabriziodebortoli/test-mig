
#include "stdafx.h"

#include <TbFrameworkImages\CommonImages.h>
#include <TbFrameworkImages\GeneralFunctions.h>

#include <TbGeneric\GeneralFunctions.h>

#include <TbGeneric\DataObj.h>
#include <TbGeneric\FormatsTable.h>

#include <TbParser\TokensTable.h>
#include <TbParser\Parser.h>

#include <TbGenlib\messages.h>

#include <TbOleDb\oledbmng.h>
#include <TbOleDb\sqlcatalog.h>
#include <TbOleDb\wclause.h>
#include <TbOleDb\sqlrec.h>

#include "ActionsRepEngin.h"
#include "edtcmm.h"
#include "prgdata.h"
#include "rpsymtbl.h"
#include "ruledata.h"
#include "tbldlg.h"
#include "generic.h"
#include "expdlg.h"

#include "INPUTMNG.hjson" //JSON AUTOMATIC UPDATE
#include "tbldlg.hjson" //JSON AUTOMATIC UPDATE

//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char THIS_FILE[] = __FILE__;
#endif

const UINT TableIndicators[] =
{   
	ID_TBLDLG_INDICATOR_TABLE_NAME,
	ID_TBLDLG_INDICATOR_FIELD_NAME,
	ID_TBLDLG_INDICATOR_MESSAGE
};

#define NO_BMP		CMultiListBox::UNCHECKED
#define SEL_BMP		CMultiListBox::CHECK_ONE
#define HID_BMP		CMultiListBox::CHECK_TWO
#define KEY_BMP		CMultiListBox::CHECK_THREE
#define SELKEY_BMP	CMultiListBox::CHECK_FOUR
#define HIDKEY_BMP	CMultiListBox::CHECK_FIVE

static const TCHAR szAUDIT[]			= _T("AU_");
#define AUDIT_PREFIX_LEN  3

//============================================================================


//============================================================================
// CTblRuleDlg
//============================================================================
IMPLEMENT_DYNAMIC(CTblRuleDlg, CParsedDialog)
BEGIN_MESSAGE_MAP(CTblRuleDlg, CParsedDialog)
	//{{AFX_MSG_MAP(CTblRuleDlg)
    ON_BN_CLICKED       (IDC_BTN_ADD_LINK,				AddVisibleLinkAction)
    ON_BN_CLICKED       (IDC_BTN_ADD_LINK_HIDDEN,		AddHiddenLinkAction)
    ON_BN_CLICKED       (IDC_BTN_UNDO,					UndoAction)
    ON_BN_CLICKED       (IDC_ADD_CALCULATE_COLUMN,		AddVisibleCalculatedColumn)
	ON_BN_CLICKED       (IDC_ADDHIDDEN_CALCUL_COLUMN,   AddHiddenCalculatedColumn)
    ON_BN_CLICKED       (IDC_REMOVE_CALCUL_COLUMN,		RemoveCalculatedColumn)

    ON_BN_CLICKED       (IDC_ADDJOIN,					AddJoin)
	ON_BN_CLICKED		(IDC_UPDATEJOIN,				UpdateJoin)				
	
	ON_BN_CLICKED       (IDC_APPLY,						OnApplySelection)

    ON_BN_CLICKED       (IDC_TBLRULE_SHOW_ALL_FIELD,	ShowFieldList)
    ON_BN_CLICKED       (IDC_TBLRULE_SHOW_SELECT_FIELD,	ShowFieldList)

    ON_CBN_SELCHANGE    (IDC_TABLE_GROUPS,				TableGroupChanged)
    ON_LBN_DBLCLK    	(IDC_TABLE_LIST,				AddNewReplicTable)
    ON_LBN_DBLCLK    	(IDC_FIELD_LIST,				AddVisibleLinkAction)
    ON_LBN_DBLCLK       (IDC_CALCCOLUMNS_LIST,			EditCalculatedColumn)
	ON_LBN_SELCHANGE    (IDC_TABLE_LIST,				TableNameChanged)
    ON_LBN_SELCHANGE    (IDC_FIELD_LIST,				TableFieldChanged)
    ON_LBN_SELCHANGE    (IDC_CALCCOLUMNS_LIST,			TableCalculatedColumnsChanged)
	ON_EN_VALUE_CHANGED	(IDC_TBLFIELD_NAME,				PublicNameChanged)
	ON_EN_VALUE_CHANGED	(IDC_TBLFIELD_DATA_TYPE,		DataTypeChanged)
	ON_EN_CHANGE		(IDC_WCLAUSE,					FilterChanged)
	ON_EN_CHANGE		(IDC_SORTBY,					FilterChanged)
	ON_EN_CHANGE        (IDC_HCLAUSE,					FilterChanged)		
	ON_EN_CHANGE		(IDC_GROUP_BY,					FilterChanged)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//----------------------------------------------------------------------------
CTblRuleDlg::CTblRuleDlg
	(
		RuleDataArray*       pRuleData,
		WoormTable* pSymTable,
		CWordArray&     IdArray,
		BOOL            bOneLink,
		SqlConnection*	pSqlConnection,
		CBaseDocument*	pDocument,
		DispTableInfo*	pDispTableInfo,
		BOOL			bShowAdvancedForm
	)
    :
    CParsedDialog		(bShowAdvancedForm ? IDD_TBLRULE_EX  : IDD_TBLRULE),

    m_pRuleData     	(pRuleData),
    m_pTblRule      	(NULL),
	m_bNewRule			(FALSE),
	m_bIsJoin			(FALSE),
	m_bIsApplyPressed	(FALSE),
	m_pSymTable     	(pSymTable),
    m_IdArray          	(IdArray),
    m_bOnlyOneLink		(bOneLink),
    m_edtWClause		(pSymTable, pRuleData),
    m_edtOrderBy		(NULL, NULL),
    m_edtHClause		(pSymTable, pRuleData),
    m_edtGroupBy		(NULL, NULL),
    m_edtPublicName		(pSymTable),
    m_pSqlTableInfo		(NULL),
    m_mlbTables			(TBIcon(szIconOk, CONTROL)),
	m_mlbFields(IDB_CHECK, IDB_HIDDEN, IDB_KEY, IDB_CHECKKEY, IDB_HIDDENKEY),
	m_mlbCalculatedCols(TBIcon(szIconOk, CONTROL), TBIcon(szIconHidden, CONTROL)),
	m_pSqlConnection	(pSqlConnection),
	m_pDispTableInfo	(pDispTableInfo),
	m_pDocument			(pDocument),
	m_edtWhenClause		(pSymTable, NULL)
{
}

//----------------------------------------------------------------------------
CTblRuleDlg::~CTblRuleDlg()
{
}

//----------------------------------------------------------------------------
BOOL CTblRuleDlg::OnInitDialog()
{
	CParsedDialog::OnInitDialog();
	
	EnableToolTips();
		    
	SendDlgItemMessage(IDC_TBLRULE_SHOW_ALL_FIELD, BM_SETCHECK, TRUE, 0);

	CreateListBox();	
	InitStatusBar();	
	
	m_stcTblOfRule.				SubclassDlgItem	(IDC_TBL_JOIN1,	        	 this);
    m_cbTableGroups.			SubclassDlgItem (IDC_TABLE_GROUPS,  		 this);
    m_mlbTables.      			SubclassDlgItem (IDC_TABLE_LIST,    		 this);
	m_mlbCalculatedCols.		SubclassDlgItem (IDC_CALCCOLUMNS_LIST,  	 this);
	m_edtPublicName.			SubclassEdit	(IDC_TBLFIELD_NAME,			 this);
	m_cbDataObjTypes.			SubclassEdit	(IDC_TBLFIELD_DATA_TYPE,	 this);
    m_cbConstrainMode.			SubclassDlgItem (IDC_CONSTRAIN_MODE,   		 this);
    m_edtWClause.    			SubclassEdit	(IDC_WCLAUSE,       		 this);
    m_edtOrderBy.    			SubclassEdit	(IDC_SORTBY,	       		 this);
    m_edtHClause.    			SubclassEdit	(IDC_HCLAUSE,       		 this);
    m_edtGroupBy.    			SubclassEdit	(IDC_GROUP_BY,	       		 this);
    m_edtWhenClause.    		SubclassEdit	(IDC_WHEN_CLAUSE,			 this);
	m_edtTopRows.				SubclassEdit	(IDC_TOP_ROWS,	       		 this);
	m_edtTopRows.SetRange(0, 0x7fff);

    m_btnAddLink.				SubclassDlgItem (IDC_BTN_ADD_LINK,  		 this);
    m_btnAddLinkHid.			SubclassDlgItem (IDC_BTN_ADD_LINK_HIDDEN,	 this);
    m_btnUndo.					SubclassDlgItem (IDC_BTN_UNDO,				 this);
	m_btnAddCalcColumn.			SubclassDlgItem (IDC_ADD_CALCULATE_COLUMN,	 this);
	m_btnAddHiddenCalcColumn.	SubclassDlgItem (IDC_ADDHIDDEN_CALCUL_COLUMN,this);
	m_btnRemoveCalcColumn.		SubclassDlgItem (IDC_REMOVE_CALCUL_COLUMN,	 this);
	m_btnAddJoin.				SubclassDlgItem (IDC_ADDJOIN,				 this);
	m_btnUpdateJoin.			SubclassDlgItem (IDC_UPDATEJOIN,			 this);
	m_btnApply.					SubclassDlgItem (IDC_APPLY,					 this);
	
	m_edtPublicName.	EnableWindow(FALSE);
	m_cbDataObjTypes.	EnableWindow(FALSE);
    
	m_mlbTables.SetTabStops(16);
	m_mlbTables.SetStyle(CMultiListBox::SORT_ON_FIRST_STRING);
   
	m_mlbCalculatedCols.SetTabStops(16);
	m_mlbCalculatedCols.SetStyle(CMultiListBox::SORT_ON_FIRST_STRING);

	SendDlgItemMessage(IDC_BTN_DISTINCT_SQL, BM_SETCHECK, FALSE);
	GetDlgItem(IDC_BTN_DISTINCT_SQL)->EnableWindow(FALSE);

	// viene messa in editing l'ultima rule utilizzata	
    if (m_pTblRule == NULL)
        m_pTblRule = m_pRuleData->m_pLastTblRule;
 
	// vengono chiamate in cascata tutte le funzioni di inizializzazione
	ShowTableGroups();

	if (m_pDispTableInfo)
	{
		m_pDispTableInfo->OnInitDialog(this, FALSE);
		m_pDispTableInfo->SetDispTableName(_T(""));
	}

	RecalcLayout();

    return TRUE;
}

// Poiche` non e` possibile cambiare lo stile di una listbox dopo che e` stata
// creata dal dialog manager, dato che si ha bisogno di una listbox single-selection
// o multiselection a seconda di come viene chiamata la dialog, si usa il trucco di
// avere una listbox non visibile e crearne una sopra run-time utilizzando unicamente
// le informazioni di dimensione e posizionamento
//------------------------------------------------------------------------------
void CTblRuleDlg::CreateListBox()
{
	DWORD lbStyle = LBS_NOTIFY | LBS_OWNERDRAWFIXED | LBS_HASSTRINGS |
    				LBS_USETABSTOPS | LBS_NOINTEGRALHEIGHT | 
                    WS_CHILD | WS_VISIBLE | ES_AUTOHSCROLL | WS_HSCROLL | WS_VSCROLL | WS_TABSTOP;

	CRect rectWnd(0,0,0,0);
	
	if (!m_bOnlyOneLink)
		lbStyle |= LBS_EXTENDEDSEL;
	
	m_mlbFields.Create(lbStyle, rectWnd, this, IDC_FIELD_LIST);

	// Attenzione: lo stile WS_EX_CLIENTEDGE viene correttamente sentito solo
	// se viene usata la CreateEx oppure, come implementato qui e nell' INPUTMNG.CPP,
	// effettuando la SetWindowPos sucessivamente
	::SetWindowLong(m_mlbFields.m_hWnd, GWL_EXSTYLE, WS_EX_CLIENTEDGE);

	GetDlgItem(IDC_FIELD_LIST_DUMMY)->GetWindowRect(rectWnd);
	GetDlgItem(IDC_FIELD_LIST_DUMMY)->DestroyWindow();

	ScreenToClient(rectWnd);
	m_mlbFields.SetWindowPos(NULL, rectWnd.left, rectWnd.top, rectWnd.Width(), rectWnd.Height(), SWP_NOZORDER);
	m_mlbFields.SetFont(GetFont());
	m_mlbFields.SetStyle(CMultiListBox::SORT_ON_FIRST_STRING);
}

//------------------------------------------------------------------------------
void CTblRuleDlg::InitStatusBar()
{
	UINT	nID, nStyle;
	int		cxWidth = 80;
	
	if	(
			!m_sbStatus.Create(this) || 
			!m_sbStatus.SetIndicators(TableIndicators, sizeof(TableIndicators) / sizeof(UINT))
		)
	{
		TRACE("Failed to create status bar\n");
		return;
	}

	m_sbStatus.GetPaneInfo(0, nID, nStyle, cxWidth);
	m_sbStatus.SetPaneInfo(0, nID, SBPS_NORMAL, cxWidth + 60);
	
	m_sbStatus.GetPaneInfo(1, nID, nStyle, cxWidth);
	m_sbStatus.SetPaneInfo(1, nID, SBPS_NORMAL, cxWidth + 60) ;
	
	m_sbStatus.GetPaneInfo(2, nID, nStyle, cxWidth);
	m_sbStatus.SetPaneInfo(2, nID, SBPS_STRETCH|SBPS_NORMAL, cxWidth);
	
	m_sbStatus.SetPaneText(0, NULL);
	m_sbStatus.SetPaneText(1, NULL);
	m_sbStatus.SetPaneText(2, NULL);
}

//----------------------------------------------------------------------------
void CTblRuleDlg::RecalcLayout()
{
	RepositionBars(0, 0xFFFF, AFX_IDW_PANE_FIRST);
}

//----------------------------------------------------------------------------
void CTblRuleDlg::FilterChanged()
{
	//m_cbTableGroups.	EnableWindow(TRUE);
    m_cbTableGroups.	EnableWindow(FALSE);
	m_mlbTables.		EnableWindow(FALSE);
}

//----------------------------------------------------------------------------
void CTblRuleDlg::ShowTableGroups()
{
	//CString strCurrGroup = Strings::ALL_TABLES();
	//m_cbTableGroups.ResetContent();
	//int nGrpIdx = m_cbTableGroups.AddString(Strings::ALL_TABLES());

	CString strCurrGroup = Strings::OLD_SELECTED();
	m_cbTableGroups.ResetContent();
	int nGrpIdx = m_cbTableGroups.AddString(Strings::ALL_TABLES());
	nGrpIdx = m_cbTableGroups.AddString(Strings::OLD_SELECTED());

	// i gruppi vengono descritti dagli Xml di modulo
	AddOnApplication*	pAddOnApp;
	AddOnModule*		pAddOnMod;

	for (int i=0; i <= AfxGetAddOnAppsTable()->GetUpperBound(); i++)
	{
		pAddOnApp = AfxGetAddOnAppsTable()->GetAt(i);
		if (!pAddOnApp || !pAddOnApp->m_pAddOnModules)
			continue;

		for (int n=0; n <= pAddOnApp->m_pAddOnModules->GetUpperBound(); n++)
		{
			pAddOnMod = pAddOnApp->m_pAddOnModules->GetAt(n);
			
			// se non ci sono oggetti di db presenti non visualizzo il modulo
			if (pAddOnMod && AfxGetDatabaseObjectsTable()->ModuleHasObjects(pAddOnMod->m_Namespace))
				m_cbTableGroups.AddString(pAddOnMod->GetModuleTitle());
		}
	}
		
    m_cbTableGroups.SetCurSel(nGrpIdx);
	TableGroupChanged();
}

//----------------------------------------------------------------------------
void CTblRuleDlg::TableGroupChanged()
{
	AfxGetApp()->BeginWaitCursor ();

	CString strCurrGroup = Strings::ALL_TABLES();
	
	int curIdxGroup = m_cbTableGroups.GetCurSel();
	//if (curIdxGroup > 0)
	m_cbTableGroups.GetLBText(curIdxGroup, strCurrGroup);

	m_mlbTables.SetRedraw(FALSE);
	m_mlbTables.ResetContent();
	m_TableNames.RemoveAll();

	POSITION pos;
	CString key;
	SqlCatalogEntry* pCatalogEntry;
	AddOnModule* pAddOnMod = NULL;

	CString strAlready = Strings::OLD_SELECTED();
	int idxAlreadySelected = m_cbTableGroups.FindString(0, strAlready );


	if ( curIdxGroup == idxAlreadySelected )   //old selected
	{// lock scope
		SqlCatalogConstPtr pCatalog = m_pSqlConnection->GetCatalog();
		  
		int position = 0;
		  for (pos = pCatalog->GetStartPosition(); pos != NULL;)
			{
				pCatalog->GetNextAssoc(pos, key, (CObject*&) pCatalogEntry);

				if (pCatalogEntry->m_nType != TABLE_TYPE && pCatalogEntry->m_nType != VIEW_TYPE)
					continue;

				// devo aggiungere solo quelle marcate
				BOOL comp;
				for (int i = 0; i < m_pRuleData->GetSize(); i++)
				{   
					comp = FALSE;
					TblRuleData* pRuleDataObj = dynamic_cast<TblRuleData*>(m_pRuleData->GetAt(i));
					if (pRuleDataObj)
					{ 
						int numTableInfo = pRuleDataObj->m_arSqlTableJoinInfoArray.GetSize();
						for (int j = 0; j < numTableInfo; j++)
						{
							if (pCatalogEntry->m_strTableName.CompareNoCase(((TblRuleData*)pRuleDataObj)->m_arSqlTableJoinInfoArray.GetAt(j)->GetTableName()) == 0)
							{
								comp = TRUE;
								break;
							}
						}		
					    if (comp)
						{ 	 			
							m_TableNames.InsertAt
								(
									m_mlbTables.AddString
										(
											pCatalogEntry->m_strTableName, 
											numTableInfo > 1 ? cwsprintf(_T("[%d]"), i) : _T("")
										),
									pCatalogEntry->m_strTableName
								);
						}
					}
				}
			}
		//mark all table in list
		for (int k = 0; k < m_mlbTables.GetCount() ; k++) 
				   m_mlbTables.SetFlag(k, SEL_BMP);			
		m_mlbTables.SetCurSel(0);
	}
	
	if ((curIdxGroup != idxAlreadySelected/*1*/) || ((m_mlbTables.GetCount()==0) && (curIdxGroup == idxAlreadySelected) )) //1 = already selected
	 {	 
	   if ((m_mlbTables.GetCount()==0) && (curIdxGroup == idxAlreadySelected/*1*/))
	   {
		 strCurrGroup = Strings::ALL_TABLES(); 
		 int nIndex = 0;
		 if ( (nIndex = m_cbTableGroups.FindString(nIndex, strCurrGroup)) != LB_ERR)
			 m_cbTableGroups.SetCurSel( nIndex );
	   }
		
	   if (strCurrGroup.CompareNoCase(Strings::ALL_TABLES()))
			for (int a=0; a <= AfxGetAddOnAppsTable()->GetUpperBound(); a++)
			{
				AddOnApplication* pAddOnApp = AfxGetAddOnAppsTable()->GetAt(a);
				if (!pAddOnApp || !pAddOnApp->m_pAddOnModules)
					continue;

				for (int m=0; m <= pAddOnApp->m_pAddOnModules->GetUpperBound(); m++)
				{
					pAddOnMod =  pAddOnApp->m_pAddOnModules->GetAt(m);
					if	(pAddOnMod && AfxGetCultureInfo()->IsEqual(strCurrGroup, pAddOnMod->GetModuleTitle()))
						break;

					pAddOnMod = NULL;
				}

				if (pAddOnMod)
					break;
			}

	{// lock scope
		SqlCatalogConstPtr pCatalog = m_pSqlConnection->GetCatalog();
			// ora ciclo sulle tabelle per definire se appartengono al modulo selezionato
		for (pos = pCatalog->GetStartPosition(); pos != NULL;)
		{
			pCatalog->GetNextAssoc(pos, key, (CObject*&) pCatalogEntry);

			if (pCatalogEntry->m_nType != TABLE_TYPE && pCatalogEntry->m_nType != VIEW_TYPE)
				continue;

			const CDbObjectDescription* pDbObjectDescr = AfxGetDbObjectDescription(pCatalogEntry->m_strTableName);

			// tutti i gruppi oppure la tabella appartiene al modulo corrente
			CString sApp;
			CString sMod;
			if (pDbObjectDescr)
			{
				sApp = pDbObjectDescr->GetNamespace().GetApplicationName();
				sMod = pDbObjectDescr->GetNamespace().GetModuleName();
			}
			if	(
					strCurrGroup == Strings::ALL_TABLES() 
					||
					(
						pAddOnMod && pDbObjectDescr 
						&&
						sApp.CompareNoCase(pAddOnMod->GetApplicationName()) == 0
						&&
						sMod.CompareNoCase(pAddOnMod->GetModuleName()) == 0
					)
				)
			{
				m_TableNames.InsertAt
					(
						m_mlbTables.AddString(pCatalogEntry->m_strTableName, _T("")),
						pCatalogEntry->m_strTableName
					);
			}
		}
	}

	// ogni tabella attualmente referenziata da una rule deve essere
	// marcata con un check mark
	int nCurSel = -1;
	if (m_mlbTables.GetCount() > 0)
	{
		// MarkSelectedTables() ritorna l'indice della tabella referenziata
		// dalla rule corrente (se esiste)
		nCurSel = MarkSelectedTables();
	}
	
	m_mlbTables.SetCurSel(nCurSel);
	} // end else if( curIdxGroup == 1 )

	// a fronte del SetCurSel precedente viene forzato il cambio di
	// tabella con tutte le inizializzazioni che ne conseguono
	TableNameChanged();
	AfxGetApp()->EndWaitCursor ();

	m_mlbTables.SetRedraw(TRUE);
	m_mlbTables.Invalidate(FALSE);
}

//----------------------------------------------------------------------------
int CTblRuleDlg::MarkSelectedTables()
{
	int nIdx = 0;
	CStringArray arTableNames;
	// mark selected tables
	//
	for (int i = 0; i < m_pRuleData->GetSize(); i++)
	{		
		RuleDataObj* pRuleDataObj = (RuleDataObj*) m_pRuleData->GetAt(i);
		
		if (pRuleDataObj->IsARule() == RULE_DATA_TABLE)
		{
			((TblRuleData*) pRuleDataObj)->GetTableNames(arTableNames);
			int nNumTables = m_TableNames.GetSize();
			for (int z = 0; z < arTableNames.GetSize(); z++)
			{	
				for (int j = 0; j < nNumTables; j++)
		        	{		    
					  if (m_TableNames[j].CompareNoCase(arTableNames.GetAt(z)) == 0)
							{
								int k = MarkTableName(j);

								// se e` quella corrente ne ritorna la posizione
								if (m_pTblRule == pRuleDataObj)
									nIdx = k;
								break;
							}
					}
			}
		}
	}

	return nIdx;
}

// Ritorna la posizione della tabella marcata.
// Se del caso replica la tabella per gestire piu` rule che referenziano la
// stessa tabella
//----------------------------------------------------------------------------
int CTblRuleDlg::MarkTableName(int nIdx)
{
	if (m_mlbTables.GetFlag(nIdx))
	{
		// se la tabella e` gia` marcata significa che si sta aggiungendo
		// un ulteriore referenziamento alla stessa tabella
		//
		CString strName		= m_TableNames[nIdx];
		CString	strDescr	= CString("\t") + m_mlbTables.GetString1(nIdx);

		// si cerca il punto ove inserire (ovviamente in fondo alla lista
		// delle tabelle "replicate"
		while	(
					++nIdx < m_TableNames.GetSize() && 
					strName.CompareNoCase(m_TableNames[nIdx]) == 0
				);

		// "replica" la tabella
		m_TableNames.InsertAt(nIdx, strName);
		m_mlbTables.AddString(nIdx, strDescr,  _T(""), SEL_BMP);
	}
	else
		m_mlbTables.SetFlag(nIdx, SEL_BMP);
		
	return nIdx;
}

//----------------------------------------------------------------------------
void CTblRuleDlg::TableNameChanged()
{                
	m_bIsJoin = FALSE;
	// to avoid menagement of current message from input
	//
	if (!m_mlbTables.IsWindowEnabled())
		return;
			
	BeginWaitCursor();	
	m_mlbTables.EnableWindow(FALSE);
	m_stcTblOfRule.SetWindowText(_T(""));
	m_btnUpdateJoin.EnableWindow(FALSE);
	
	if (m_pTblRule && m_bNewRule && !m_pTblRule->IsTemporaryJoin())
		delete m_pTblRule;
	m_pTblRule = NULL;

	CString strCurrTableName;
	if (m_mlbTables.GetCurSel() != LB_ERR)
	{
		strCurrTableName = GetCurTableName();
		// se la tabella selezionata e` associata ad una rule si seleziona la rule stessa
 		if (m_mlbTables.GetFlag(m_mlbTables.GetCurSel()))
		{	
			m_pTblRule = (TblRuleData*) m_pRuleData->GetRuleData
				(
					strCurrTableName,
					RULE_DATA_TABLE,
					GetCurReplicTable()
				);
			m_arSqlTableInfo.RemoveAll();
			m_arSqlTableInfo.Append(m_pTblRule->m_arSqlTableJoinInfoArray);
	        
			m_btnUpdateJoin.EnableWindow(TRUE);
			//metto nomi tabelle nella join rule
			CString tableNames = CString("");
			if (m_arSqlTableInfo.GetSize() > 1)
			{
				m_bIsJoin = TRUE;
				int nCurrIdx = m_mlbTables.GetCurSel();
				tableNames = m_mlbTables.GetString2(nCurrIdx) + CString(": ");    
			}
			
			for (int i = 0; i < m_arSqlTableInfo.GetSize(); i++)
					tableNames = tableNames +  m_arSqlTableInfo.GetAt(i)->GetTableName() + CString(", "); 
			tableNames = tableNames.Left(tableNames.GetLength() - 2);
	
			m_stcTblOfRule.SetWindowText(tableNames);		
		}
	}

	m_bNewRule = (m_pTblRule == NULL);

	if (m_bNewRule ||(m_pTblRule && m_pTblRule->IsTemporaryJoin()) )
	{
		SendDlgItemMessage(IDC_WCLAUSE_BTN_NATIVE_SQL, BM_SETCHECK, FALSE);

   		m_edtWClause.SetWindowText(_T("")); //filter changed
   		m_edtOrderBy.SetWindowText(_T(""));

 		SendDlgItemMessage(IDC_HCLAUSE_BTN_NATIVE_SQL, BM_SETCHECK, FALSE);

   		m_edtHClause.SetWindowText(_T(""));
   		m_edtGroupBy.SetWindowText(_T(""));
		
   		m_edtWhenClause.SetWindowText(_T(""));
		// 
		m_cbTableGroups.	EnableWindow(TRUE);
		SendDlgItemMessage(IDC_BTN_DISTINCT_SQL, BM_SETCHECK, FALSE);
	}
	
	//devo controllare se è una tabella di tracciatura. In questo caso devo eseguire i seguenti passi:
	// - caricare le CDbObjectDescription della tabella sotto tracciatura
	// - caricare le dll necessarie per la register della tabella sotto tracciatura. la register effettua anche
	// il controllo se la tabella e sotto tracciatura o meno. se è sotto tracciatura viene istanziato il TAuditingRecord
	// di strCurrTableName per cui vengono effettuato in modo corretto il binding dei datatype alle colonne
	CString strTracedTable;
	int nPos = strCurrTableName.Find(szAUDIT);
	if (nPos >= 0)
		strTracedTable = strCurrTableName.Right(strCurrTableName.GetLength() - AUDIT_PREFIX_LEN);

	// attraverso il nome e la sua descrizione Xml scateno il caricamento delle DLL
	const CDbObjectDescription* pXmlDescri = AfxGetDbObjectDescription((strTracedTable.IsEmpty()) ? strCurrTableName : strTracedTable);

	if (pXmlDescri)
		AfxGetTbCmdManager()->LoadNeededLibraries(pXmlDescri->GetNamespace());

	LoadDictionary(strCurrTableName);

	SendDlgItemMessage(IDC_TBLRULE_SHOW_ALL_FIELD, BM_SETCHECK, TRUE, 0);
	
	LoadCalculatedColumns();
	ShowCalculatedColumns();

	ShowFieldList			();
	ShowWclause				();
	ShowConstraintMode		();                     
	
	EnableSelectionControls	(FALSE, TRUE, FALSE, TRUE);
	
	EnableCalcColumnControls (TRUE, TRUE, FALSE);
	
	// resets normal listbox status
	//	
	m_mlbTables.EnableWindow(TRUE);
	EndWaitCursor();	

	m_mlbTables.SetFocus();
}

//----------------------------------------------------------------------------
CString CTblRuleDlg::GetCurTableName()
{
    int nCurrIdx = m_mlbTables.GetCurSel();

    if (nCurrIdx >= 0)
        return m_TableNames[ nCurrIdx ];
        
    return CString("");
}

//	return a number that rappresent the id replic of the current table.
//	This is necessary because can be present many ruleTable associate to
//	same data table.
//
//----------------------------------------------------------------------------
int CTblRuleDlg::GetCurReplicTable()
{
	CString		strCurrTableName;
	int			nReplicId	= 0;
    int 		nCurrIdx	= m_mlbTables.GetCurSel();

    if (nCurrIdx >= 0)
    {
    	strCurrTableName = m_TableNames[ nCurrIdx ];
    	for (int i = nCurrIdx - 1; i >= 0; i--)
    	{
    		if (strCurrTableName.CompareNoCase(m_TableNames[i]) != 0)
    			return nReplicId;

			nReplicId++;
		}
	}    
        
    return nReplicId;
}

//----------------------------------------------------------------------------
BOOL CTblRuleDlg::LoadDictionary(LPCTSTR pszTableName)
{  
	m_sbStatus.SetPaneText(0, AfxLoadDatabaseString(pszTableName, pszTableName));
	m_sbStatus.SetPaneText(1, NULL);
	m_sbStatus.SetPaneText(2, NULL);

	if (pszTableName == NULL || *pszTableName == NULL)
	{
	    m_pSqlTableInfo = NULL;
		return TRUE;
	}

	if (!m_pSqlConnection->ExistTable(pszTableName))    
        return FALSE;

	CMessages mess(_TB("Fields of this type not supported"));

    m_pSqlTableInfo = m_pSqlConnection->GetTableInfo(pszTableName);
     
	ASSERT(m_pSqlTableInfo);

	CString strPublicName;
	CString strPhysicalName;
	PublicFieldObj::SelectionType selType;
	DataType dataType;
	
    m_PublicFieldsLinks.RemoveAll();

	BOOL bTemporaryBinded = FALSE;
	
	// non deve far vedere i campi virtuali definiti dal programmatore
	// ed inseriti nel TableInfo dalla prima istanza di record derivato 
	// che definisce eventuali campi virtuali
	const  Array* pColums = m_pSqlTableInfo->GetPhysicalColumns();
	for (int i = 0; i < pColums->GetSize(); i++)
	{
		SqlColumnInfo* pSqlColumnInfo = (SqlColumnInfo*) pColums->GetAt(i);
		ASSERT (pSqlColumnInfo);
		ASSERT(!pSqlColumnInfo->m_bVirtual);

		if (!bTemporaryBinded && pSqlColumnInfo->m_DataObjType == DATA_NULL_TYPE)
		{		
			// Istanzia (e distrugge) un SqlRecord per memorizzare in ColumnInfo i DataObj type
			// delle collonne delle tabelle registrate (Germano)
			const SqlCatalogEntry *pEntry = m_pSqlConnection->GetCatalogEntry(pszTableName);
			SqlRecord* pRec = pEntry ? pEntry->CreateRecord() : NULL;			
			delete pRec;

			bTemporaryBinded = TRUE;
		}

		strPhysicalName = pSqlColumnInfo->GetColumnName();
   		if (m_pTblRule)
		{
			strPublicName = CString("");
			if (m_pTblRule->m_arSqlTableJoinInfoArray.m_arFieldLinks.GetSize() == 1) //cerco con sintassi non qualificata
			   strPublicName = m_pTblRule->GetPublicNameOf(strPhysicalName);

			// provo a cercarla anche con la sintassi qualificata
			if (strPublicName.IsEmpty())
			{
				CString sQualifiedName (CString(pszTableName) + DOT_CHAR + strPhysicalName);
       			strPublicName = m_pTblRule->GetPublicNameOf(sQualifiedName);
			}
		}
   		else
   			strPublicName.Empty();
       		
   		if (strPublicName.IsEmpty())
   		{
			// colonna non ancora selezionata
   			strPublicName = /*@@TODO CString(pszTableName) + "_" +*/ strPhysicalName;

			CWordArray dataTypes;
   			if (!pSqlColumnInfo->GetDataObjTypes(dataTypes))
   			{
				mess.Add(strPhysicalName);

       			selType = PublicFieldObj::BAD_TYPE;
       			dataType = DataType(DATA_NULL_TYPE);
   			}
			else
			{
   				selType = PublicFieldObj::NONE;
   				dataType = pSqlColumnInfo->m_DataObjType == DATA_NULL_TYPE
					? dataTypes[0]
					: pSqlColumnInfo->m_DataObjType;
   			}
   		}
   		else
   		{
			// colonna gia` selezionata
			WoormField* pField = m_pSymTable->GetField((LPCTSTR)strPublicName);
			ASSERT(pField);
			if (pField)
			{
   				selType = pField->IsHidden() ? PublicFieldObj::OLD_HIDDEN : PublicFieldObj::OLD_SELECTED;
   				dataType = pField->GetDataType();
			}
   		}
		m_PublicFieldsLinks.Add(new PublicFieldObj(strPublicName, selType, dataType, pSqlColumnInfo->m_bSpecial, strPhysicalName));
	}
	mess.Show(TRUE);
    return TRUE;
}

//----------------------------------------------------------------------------
BOOL CTblRuleDlg::LoadCalculatedColumns()
{
	m_PublicCalculatedColumnsLinks.RemoveAll();
	if (m_pTblRule)
	{    //	Adding calculated columns
		CStringArray arCalculatedColumns;
		CString publicName;
		CString physicalName;
		DataType type;
		PublicFieldObj::SelectionType selType;
		
		m_pTblRule->GetCalculatedColumns(arCalculatedColumns);
		
		for (int i = 0; i < arCalculatedColumns.GetSize(); i++)  		  
		 {
			physicalName = arCalculatedColumns.GetAt(i);
			publicName = m_pTblRule->GetPublicNameOf(physicalName);
			WoormField* pField = m_pSymTable->GetField(publicName);
			ASSERT(pField);

			selType = pField->IsHidden() ? PublicFieldObj::OLD_HIDDEN : PublicFieldObj::OLD_SELECTED;
       		type = pField->GetDataType();
            m_PublicCalculatedColumnsLinks.Add(new PublicFieldObj(publicName, selType, type, FALSE, physicalName)); 
		 }
	}
	return TRUE;
}

//----------------------------------------------------------------------------
void CTblRuleDlg::EnableCalcColumnControls(BOOL bEnableAdd, BOOL bEnableAddHidden, BOOL bEnableRemove)
{
	m_btnAddCalcColumn.EnableWindow(bEnableAdd);
	m_btnAddHiddenCalcColumn.EnableWindow(bEnableAddHidden);
	m_btnRemoveCalcColumn.EnableWindow(bEnableRemove);
}

//----------------------------------------------------------------------------
void CTblRuleDlg::EnableSelectionControls(BOOL bEnableAdd, BOOL bCheckAlso, BOOL bEnableModify, BOOL bClear)
{
	if (bCheckAlso)
	{
	    SendDlgItemMessage(IDC_TBLRULE_SHOW_ALL_FIELD, BM_SETCHECK, TRUE, 0);
	    SendDlgItemMessage(IDC_TBLRULE_SHOW_SELECT_FIELD, BM_SETCHECK, FALSE, 0);
	}
	
	BOOL bFieldsSelected	= FALSE;
	BOOL bCurrentSelected	= FALSE;

	for (int i = 0; i < m_PublicFieldsLinks.GetSize(); i++)
	{
		if
			(
				m_PublicFieldsLinks[i]->m_Selection != PublicFieldObj::NONE        &&
				m_PublicFieldsLinks[i]->m_Selection != PublicFieldObj::BAD_TYPE
			)
		{             
			bFieldsSelected = TRUE;
			break;
		}
	}
    for (int i = 0; i < m_PublicCalculatedColumnsLinks.GetSize(); i++)
	{
		if
			(
				m_PublicCalculatedColumnsLinks[i]->m_Selection != PublicFieldObj::NONE        &&
				m_PublicCalculatedColumnsLinks[i]->m_Selection != PublicFieldObj::BAD_TYPE
			)
		{             
			bFieldsSelected = TRUE;
			break;
		}
	}
	if (m_mlbFields.GetCount() > 0 && m_mlbFields.GetCurSel() >= 0)
	{
		int nCurrIdx = GetLinkIdxFromFieldListIdx(m_mlbFields.GetCurSel());
		if (nCurrIdx >= 0)
		{
			PublicFieldObj* pPublicFieldObj = m_PublicFieldsLinks[nCurrIdx];
			if
				(
					pPublicFieldObj->m_Selection != PublicFieldObj::NONE           &&
					pPublicFieldObj->m_Selection != PublicFieldObj::BAD_TYPE
				)
					bCurrentSelected = TRUE;
		} 
	}

    m_btnAddLink.	EnableWindow(bEnableAdd /*&& IsDlgButtonChecked(IDC_TBLRULE_SHOW_ALL_FIELD) */&& !bCurrentSelected);
    m_btnAddLinkHid.EnableWindow(bEnableAdd /*&& IsDlgButtonChecked(IDC_TBLRULE_SHOW_ALL_FIELD) */&& !bCurrentSelected);
    m_btnUndo.		EnableWindow(bEnableAdd && bCurrentSelected);
    
	GetDlgItem(IDC_TBLRULE_SHOW_ALL_FIELD)->	EnableWindow(bFieldsSelected);
	GetDlgItem(IDC_TBLRULE_SHOW_SELECT_FIELD)->	EnableWindow(bFieldsSelected);
	
	//
	BOOL isEmptyJoin = TRUE;
	if (m_bIsJoin)
	{   
		ASSERT (m_pTblRule);
		if (
			    m_pTblRule->m_arSqlTableJoinInfoArray.m_arFieldLinks.GetSize() > 0
			||
				m_pTblRule->m_CalcColumnLinks.GetSize() > 0
		    )
		{
			isEmptyJoin = FALSE;
		}
	}

	GetDlgItem(IDOK)->							EnableWindow(bFieldsSelected || !isEmptyJoin);
	m_btnApply.									EnableWindow(bFieldsSelected || !isEmptyJoin);

	m_edtPublicName.	EnableWindow(bEnableModify);
	m_cbDataObjTypes.	EnableWindow(bEnableModify);

	if (bClear)
	{
		m_edtPublicName.	SetValue(_T(""));
		m_cbDataObjTypes.	SetTypeValue(DATA_NULL_TYPE);
	}
}
    
//----------------------------------------------------------------------------
void CTblRuleDlg::ShowCalculatedColumns()
{
	m_mlbCalculatedCols.SetRedraw(FALSE);
	m_mlbCalculatedCols.ResetContent();

	for (int i = 0; i < m_PublicCalculatedColumnsLinks.GetSize(); i++)
	{   
		PublicFieldObj* pPFO = m_PublicCalculatedColumnsLinks[i];
		
		if ((pPFO->m_Selection == PublicFieldObj::SELECTED) || (pPFO->m_Selection == PublicFieldObj::OLD_SELECTED))
		      m_mlbCalculatedCols.AddString(pPFO->m_physicalName, _T(""), SEL_BMP);

		if  ((pPFO->m_Selection == PublicFieldObj::HIDDEN) || (pPFO->m_Selection == PublicFieldObj::OLD_HIDDEN))
			 m_mlbCalculatedCols.AddString(pPFO->m_physicalName, _T(""), HID_BMP);
	}
	m_mlbCalculatedCols.SetRedraw(TRUE);
	m_mlbCalculatedCols.Invalidate(FALSE);
}
//----------------------------------------------------------------------------
void CTblRuleDlg::ShowFieldList()
{
    if (m_pSqlTableInfo == NULL)
    {
        m_mlbFields.ResetContent();
        return;
    }

	CString	strCurrPhysicalName;		

    m_mlbFields.SetRedraw(FALSE);
    m_mlbFields.ResetContent();
	
	// non deve far vedere i campi virtuali definiti dal programmatore
	// ed inseriti nel TableInfo dalla prima istanza di record derivato 
	// che definisce eventuali campi virtuali
	const  Array* pColums = m_pSqlTableInfo->GetPhysicalColumns();
	for (int i = 0; i < pColums->GetSize(); i++)
	{
		SqlColumnInfo* pSqlColumnInfo = (SqlColumnInfo*)pColums->GetAt(i);
		ASSERT (pSqlColumnInfo);
		ASSERT (!pSqlColumnInfo->m_bVirtual);
		
		strCurrPhysicalName = pSqlColumnInfo->GetColumnName();
        
		PublicFieldObj* pPFO = m_PublicFieldsLinks[i];
		if	(
				IsDlgButtonChecked(IDC_TBLRULE_SHOW_ALL_FIELD) ||
				(
    				pPFO->m_Selection != PublicFieldObj::NONE	&&
    				pPFO->m_Selection != PublicFieldObj::BAD_TYPE
    			)
    		)
        
		{
    		switch (pPFO->m_Selection)
    		{
    			case PublicFieldObj::SELECTED		:
    			case PublicFieldObj::OLD_SELECTED	:
	        		m_mlbFields.AddString
						(
							strCurrPhysicalName, _T(""),
							pPFO->m_bSpecial ? SELKEY_BMP : SEL_BMP
						);
    				break;

    			case PublicFieldObj::BAD_TYPE		:
    			case PublicFieldObj::HIDDEN			:
    			case PublicFieldObj::OLD_HIDDEN		:
	        		m_mlbFields.AddString
						(
							strCurrPhysicalName, _T(""),
							pPFO->m_bSpecial ? HIDKEY_BMP : HID_BMP
						);
    				break;
        			
    			default :
	        		m_mlbFields.AddString
						(
							strCurrPhysicalName, _T(""),
							pPFO->m_bSpecial ? KEY_BMP : NO_BMP
						);
    				break;
    		}
	 	}
	}
	//m_mlbFields.AddString
	//	(
	//		_TB("<native sql expression>"), _T(""),
	//		NO_BMP
	//	);

    m_mlbFields.SetRedraw(TRUE);
    m_mlbFields.Invalidate(FALSE);
    if (m_mlbFields.GetCount() > 0)
		m_mlbFields.SelItemRange(FALSE, 0, m_mlbFields.GetCount() - 1);
}

//----------------------------------------------------------------------------
void CTblRuleDlg::ShowWclause()
{
  //Controllo se ci sono campi selezionati nel caso fosse un join 
	BOOL bUnselectTable = TRUE;
	
	if (m_pTblRule != NULL)
	{
			for (int i = 0; i < m_PublicFieldsLinks.GetSize(); i++)
			{
				PublicFieldObj* pPFO = m_PublicFieldsLinks[i];
				if	(
						pPFO->m_Selection != PublicFieldObj::BAD_TYPE	&&
						pPFO->m_Selection != PublicFieldObj::NONE
					)
				{
					bUnselectTable = FALSE;
					break;
				}
			}
			for (int i = 0; i < m_PublicCalculatedColumnsLinks.GetSize(); i++)
			{
				PublicFieldObj* pPFO = m_PublicCalculatedColumnsLinks[i];
				if	(
						pPFO->m_Selection != PublicFieldObj::BAD_TYPE	&&
						pPFO->m_Selection != PublicFieldObj::NONE
					)
				{
					bUnselectTable = FALSE;
					break;
				}
			}
	}
	
    BOOL isTemporaryJoin = FALSE;
	if (m_pTblRule != NULL)
		isTemporaryJoin = m_pTblRule->IsTemporaryJoin();

	BOOL enableCriterion = ( ((m_pTblRule != NULL)|| isTemporaryJoin) && (!bUnselectTable || m_bIsJoin));

	SendDlgItemMessage(IDC_BTN_DISTINCT_SQL, BM_SETCHECK, FALSE);
	GetDlgItem(IDC_BTN_DISTINCT_SQL)->EnableWindow(FALSE);

	if //(m_pTblRule &&  (!m_bNewRule || (!m_pTblRule->IsTemporaryJoin() && m_bIsJoin ) )rivedere
      (
	    (m_pTblRule && !m_bNewRule && !m_bIsJoin) 
	     ||
	    (m_pTblRule && (!m_pTblRule->IsTemporaryJoin() && m_bIsJoin ) )
	  )
	{
		CString strTmp;
    	
		//Where-OrderBy
    	strTmp = m_pTblRule->m_pWhereClause->ToString();
		ConvertCString(strTmp, LF_TO_CRLF);
    	m_edtWClause.SetWindowText(strTmp);  //filter changed

	    SendDlgItemMessage(IDC_WCLAUSE_BTN_NATIVE_SQL, BM_SETCHECK, m_pTblRule->m_pWhereClause->IsNative());
    	
    	strTmp = m_pTblRule->m_strOrderBy;
		ConvertCString(strTmp, LF_TO_CRLF);
    	m_edtOrderBy.SetWindowText(strTmp); //filter changed

		//GroupBy-Having
    	strTmp = m_pTblRule->m_pHavingClause->ToString();
		ConvertCString(strTmp, LF_TO_CRLF);
    	m_edtHClause.SetWindowText(strTmp);

	    SendDlgItemMessage(IDC_HCLAUSE_BTN_NATIVE_SQL, BM_SETCHECK, m_pTblRule->m_pHavingClause->IsNative());
    	
    	strTmp = m_pTblRule->m_strGroupBy;
		ConvertCString(strTmp, LF_TO_CRLF);
    	m_edtGroupBy.SetWindowText(strTmp);

		//When
    	strTmp = m_pTblRule->m_pWhenExpr->ToString();
		ConvertCString(strTmp, LF_TO_CRLF);
    	m_edtWhenClause.SetWindowText(strTmp);

		//----
		m_cbTableGroups.	EnableWindow(TRUE);
		m_mlbTables.		EnableWindow(TRUE);
		SendDlgItemMessage(IDC_BTN_DISTINCT_SQL, BM_SETCHECK, m_pTblRule->IsDistinct());
   }
	GetDlgItem(IDC_BTN_DISTINCT_SQL)->EnableWindow(enableCriterion);
 
	m_cbConstrainMode.						EnableWindow(enableCriterion);
    GetDlgItem(IDC_CONSTRAIN_MODE_TITLE)->	EnableWindow(enableCriterion);
   
	if ((m_pTblRule == NULL) || (m_pTblRule->m_arSqlTableJoinInfoArray.GetSize() == 0))
	{	 
		if (m_pSqlTableInfo)
		{
			m_arSqlTableInfo.RemoveAll();
			m_arSqlTableInfo.Add(m_pSqlTableInfo);
		}
	}
	else
		m_arSqlTableInfo = m_pTblRule->m_arSqlTableJoinInfoArray;

	m_edtWClause.SetTableInfo(m_arSqlTableInfo,  m_pTblRule);  
   	m_edtOrderBy.SetTableInfo(m_arSqlTableInfo );  
   	
    GetDlgItem(IDC_WCLAUSE_BTN_NATIVE_SQL)->EnableWindow(m_pTblRule != NULL);

	m_edtWClause.					EnableWindow(enableCriterion);
   	m_edtOrderBy.					EnableWindow(enableCriterion);
 
   	m_edtHClause.SetTableInfo(m_arSqlTableInfo, m_pTblRule);  
   	m_edtGroupBy.SetTableInfo(m_arSqlTableInfo);  
   	
    GetDlgItem(IDC_HCLAUSE_BTN_NATIVE_SQL)->EnableWindow(enableCriterion);

   	m_edtHClause.					EnableWindow(enableCriterion);
   	m_edtGroupBy.					EnableWindow(enableCriterion);

   	m_edtWhenClause.				EnableWindow(enableCriterion);
}

//----------------------------------------------------------------------------
void CTblRuleDlg::ShowConstraintMode()
{
    m_cbConstrainMode.ResetContent();
    m_cbConstrainMode.AddString(_TB("All data"));
    m_cbConstrainMode.AddString(_TB("Only NOT 'null' data "));
    m_cbConstrainMode.AddString(_TB("Only 'null' data"));

    if (m_pTblRule && !m_bNewRule)
    {
    	switch (m_pTblRule->m_ConstraintMode)
    	{
        	case RuleSelectionMode::SEL_ALL       : m_cbConstrainMode.SetCurSel(0);	break;
        	case RuleSelectionMode::SEL_NOT_NULL  : m_cbConstrainMode.SetCurSel(1);	break;
        	case RuleSelectionMode::SEL_NULL      : m_cbConstrainMode.SetCurSel(2);	break;
    	}
	}
	else
		m_cbConstrainMode.SetCurSel(0);

	m_cbConstrainMode.EnableWindow(m_pTblRule != NULL);
}

//----------------------------------------------------------------------------
void CTblRuleDlg::AddNewReplicTable()
{
	if (!m_mlbTables.IsWindowEnabled()) return;

    int	nIdxInsert;

    if ((nIdxInsert = m_mlbTables.GetCurSel()) < 0)	return;

    if (!m_mlbTables.GetFlag(nIdxInsert))
    {
		// fatto doppio click su una tabella non ancora marcata e quindi
		// senza alcuna rule associata
		//
		TableNameChanged();
		return;
	}

    CString	strCurrTableName(m_TableNames[nIdxInsert]);
    CString	strCurrTableDescr(m_mlbTables.GetString1(nIdxInsert));

	// facendo doppio click su una tabella marcata si indica che si vuole una replicazione
	// della stessa, in tal caso si determina se si era stata gia` fatta una replicazione,
	// senza per altro selezionarne alcun campo e quindi con la possibilita` di riutilizzare
	// tale replicazione
	while	(
				++nIdxInsert < m_TableNames.GetSize() && 
				strCurrTableName.CompareNoCase(m_TableNames[nIdxInsert]) == 0
			)
		if (!m_mlbTables.GetFlag(nIdxInsert))
		{
		    m_mlbTables.SetCurSel(nIdxInsert);
			TableNameChanged();
			return;
		}

	if (strCurrTableDescr[0] != TAB_CHAR)		
		strCurrTableDescr	= _T("\t") + strCurrTableDescr;
		
    m_TableNames.InsertAt(nIdxInsert, strCurrTableName);
    m_mlbTables.AddString(nIdxInsert, strCurrTableDescr, _T(""), NO_BMP);
    m_mlbTables.SetCurSel(nIdxInsert);

	TableNameChanged();
}
                                 
//----------------------------------------------------------------------------
void CTblRuleDlg::TableCalculatedColumnsChanged()
{     	    
	if (m_mlbFields.GetCount() > 0)
	   m_mlbFields.SelItemRange(FALSE, 0, m_mlbFields.GetCount()); //Deselect all selected DB fields 
	CString physicalName = m_mlbCalculatedCols.GetString1(m_mlbCalculatedCols.GetCurSel());
	for (int i = 0; i < m_PublicCalculatedColumnsLinks.GetSize(); i++)
	{
		PublicFieldObj* pPFO = m_PublicCalculatedColumnsLinks[i];
		if	(pPFO->m_physicalName.Compare(physicalName) == 0)
		{ 
			m_edtPublicName.SetValue(pPFO->m_strName);
			m_cbDataObjTypes.SetTypeValue(pPFO->m_DataType);
			if	(
				 (pPFO->m_Selection == PublicFieldObj::OLD_SELECTED)
				 ||(pPFO->m_Selection == PublicFieldObj::OLD_HIDDEN)
				)
				EnableCalcColumnControls (TRUE, TRUE, FALSE);
			else
				EnableCalcColumnControls (TRUE, TRUE, TRUE);
			
			break;
		}
	}
	EnableSelectionControls(FALSE, FALSE, FALSE, FALSE);
}

//----------------------------------------------------------------------------
void CTblRuleDlg::TableFieldChanged()
{
	m_mlbCalculatedCols.SetCurSel(-1);
	EnableCalcColumnControls (TRUE, TRUE, FALSE);

	if (m_pSqlTableInfo == NULL || m_mlbFields.GetCurSel() == LB_ERR)
		return;

	int nCurrIdx;
	//int nSel = m_mlbFields.GetCurSel();
	//if (nSel == 0)
	//{
	//	m_cbDataObjTypes.GetDataTypes().Add(DATA_INT_TYPE);
	//	m_cbDataObjTypes.GetDataTypes().Add(DATA_LNG_TYPE);
	//	m_cbDataObjTypes.GetDataTypes().Add(DATA_DBL_TYPE);
	//	m_cbDataObjTypes.GetDataTypes().Add(DATA_QTA_TYPE);
	//	m_cbDataObjTypes.GetDataTypes().Add(DATA_MON_TYPE);
	//	m_cbDataObjTypes.GetDataTypes().Add(DATA_PERC_TYPE);
	//	m_cbDataObjTypes.GetDataTypes().Add(DATA_DATE_TYPE);
	//	m_cbDataObjTypes.GetDataTypes().Add(DATA_ENUM_TYPE);
	//	m_cbDataObjTypes.GetDataTypes().Add(DATA_STR_TYPE);
	//	m_cbDataObjTypes.GetDataTypes().Add(DATA_BOOL_TYPE);
	//	m_cbDataObjTypes.GetDataTypes().Add(DATA_TXT_TYPE);
	//	m_cbDataObjTypes.GetDataTypes().Add(DATA_GUID_TYPE); //per ORACLE

	//	m_cbDataObjTypes.FillListBox();

	//	return;
	//}
	//else
	nCurrIdx = GetLinkIdxFromFieldListIdx(m_mlbFields.GetCurSel());
    
	PublicFieldObj*	pPublicFieldObj = m_PublicFieldsLinks[nCurrIdx];
	SqlColumnInfo*	pSqlColumnInfo	= m_pSqlTableInfo->GetAt(nCurrIdx);

	// nome pubblico corrente
	m_edtPublicName.SetValue(pPublicFieldObj->m_strName);

	// per visualizzare il tipo di dato correntemente selezionato si inizializza
	// il vettore dei tipi compatibili associato alla combobox
	//
	pSqlColumnInfo->GetDataObjTypes(m_cbDataObjTypes.GetDataTypes());
	m_cbDataObjTypes.FillListBox();
	m_cbDataObjTypes.SetTypeValue(pPublicFieldObj->m_DataType);

	// poiche` LBN_SELCHANGE arriva anche muovendo le frecce della statiera e` necessario
	// conoscere lo stato del corrente item, che potrebbe essere stato modificato ora o in
	// precedenza clickando con il mouse (o premendo la barra di spazio) 
	//
	
	BOOL bEnableModify = FALSE;

	// se e` una colonna selezionata in una precedente sessione
	// oppure sono in stato di non selezione vengono protetti
	// l'edit del nome pubblico e la combobox del tipo
	//
	switch (pPublicFieldObj->m_Selection)
	{
		case PublicFieldObj::NONE	:
			bEnableModify = m_mlbFields.GetSel(m_mlbFields.GetCurSel());
			break;
	
		case PublicFieldObj::SELECTED	:
		case PublicFieldObj::HIDDEN	:
		    bEnableModify = TRUE;
			break;
	
		case PublicFieldObj::OLD_SELECTED	:
		case PublicFieldObj::OLD_HIDDEN	:
		case PublicFieldObj::BAD_TYPE	:
		    bEnableModify = FALSE;
			break;
	}

	// si determina se abilitare o meno i bottoni di selezione
	BOOL bEnableAdd = FALSE;

    if (m_bOnlyOneLink)
    {
		if	(
				pPublicFieldObj->m_Selection != PublicFieldObj::OLD_SELECTED	&&
				pPublicFieldObj->m_Selection != PublicFieldObj::OLD_HIDDEN		&&
				pPublicFieldObj->m_Selection != PublicFieldObj::BAD_TYPE
			)
			bEnableAdd = TRUE;
    }
    else
	{
	    int nItems = m_mlbFields.GetSelCount();
	
	    if (nItems > 0)
		{
			PublicFieldObj::SelectionType fieldSelType;
		    int* pItemVector = new int[nItems];
			m_mlbFields.GetSelItems(nItems, pItemVector);
						
			for (int i = 0; i < nItems; i++)
			{
				fieldSelType = GetLinkFromFieldListIdx(*(pItemVector + i))->m_Selection;

				if	(
						fieldSelType != PublicFieldObj::OLD_SELECTED	&&
						fieldSelType != PublicFieldObj::OLD_HIDDEN		&&
						fieldSelType != PublicFieldObj::BAD_TYPE
					)
				{
					bEnableAdd = TRUE;
					break;
				}
			}
			
			delete []pItemVector;
		}
	}
	EnableSelectionControls(bEnableAdd, FALSE, bEnableModify, FALSE);
	
	CString	strBuffer(pSqlColumnInfo->m_strTypeName);
    strBuffer += cwsprintf(_T("( %d"), pSqlColumnInfo->GetColumnLength());

	if (pSqlColumnInfo->GetColumnDecimal() > 0)
	    strBuffer += cwsprintf(_T(", %d"), pSqlColumnInfo->GetColumnDecimal());
	
	strBuffer += _T(" )");
	
	m_sbStatus.SetPaneText(1, pSqlColumnInfo->GetColumnTitle());
	m_sbStatus.SetPaneText(2, strBuffer); 
}

//----------------------------------------------------------------------------
void CTblRuleDlg::PublicNameChanged()
{
	if (m_mlbFields.GetCurSel() == LB_ERR)
		return;

	CString strName; m_edtPublicName.GetValue(strName);
	if (AfxGetTokensTable()->GetKeywordsToken(strName) != T_NOTOKEN)
	{
		//m_edtPublicName.SetFocus();  
		m_edtPublicName.SetError(_TB("The name collides with a reserved word of TaskBuilder"));
		return;
	}
	
	int	nCurrIdx = GetLinkIdxFromFieldListIdx(m_mlbFields.GetCurSel());
	m_PublicFieldsLinks[nCurrIdx]->m_strName = strName;

	if (ExistPublicName(m_PublicFieldsLinks[nCurrIdx]->m_strName, nCurrIdx, FALSE))
	{
		m_edtPublicName.SetFocus();
		m_edtPublicName.SetError(_TB(" {0-%s}: field already defined"));
		return;
	}
    //TODO unique name considering calculated columns name too
}

//----------------------------------------------------------------------------
void CTblRuleDlg::DataTypeChanged()
{
	if (m_mlbFields.GetCurSel() == LB_ERR)
		return;

    PublicFieldObj* pPFO = GetLinkFromFieldListIdx(m_mlbFields.GetCurSel());

    pPFO->m_DataType = m_cbDataObjTypes.GetTypeValue();
}

//----------------------------------------------------------------------------
void CTblRuleDlg::AddVisibleLinkAction()
{
	AddLinks(FALSE);
}

//----------------------------------------------------------------------------
void CTblRuleDlg::AddHiddenLinkAction()
{
	AddLinks(TRUE);
}

//----------------------------------------------------------------------------
void CTblRuleDlg::AddVisibleCalculatedColumn()
{
  if (m_bOnlyOneLink)
    {
	  for (int i = 0; i < m_PublicFieldsLinks.GetSize(); i++)
	    {
	        PublicFieldObj* pPFO = m_PublicFieldsLinks[i];
			if	(
					//pPFO->m_Selection == PublicFieldObj::HIDDEN	||
		 			pPFO->m_Selection == PublicFieldObj::SELECTED
			 	)
			{
				AfxMessageBox(_TB("ONLY ONE link can be added!"), MB_OK | MB_ICONEXCLAMATION);
				return;
			}
	    }
		for (int i = 0; i < m_PublicCalculatedColumnsLinks.GetSize(); i++)
	    {
	        PublicFieldObj* pPFO = m_PublicCalculatedColumnsLinks[i];
			if	(
					//pPFO->m_Selection == PublicFieldObj::HIDDEN	||
		 			pPFO->m_Selection == PublicFieldObj::SELECTED
			 	)
			{
				AfxMessageBox(_TB("ONLY ONE link can be added!"), MB_OK | MB_ICONEXCLAMATION);
				return;
			}
	    }
    }
	AddCalculatedColumn (FALSE);
 }

//----------------------------------------------------------------------------
void CTblRuleDlg::AddHiddenCalculatedColumn()
{
	AddCalculatedColumn (TRUE);
}


//----------------------------------------------------------------------------
void CTblRuleDlg::AddCalculatedColumn(BOOL bHiddenCalcColumn)
{	
	BOOL b_ruleIsNull = FALSE;
	m_mlbFields.SelItemRange(FALSE, 0, m_mlbFields.GetCount());
	CString tableName = GetCurTableName(); 
	
	if (m_pTblRule == NULL)
	{
		b_ruleIsNull = TRUE;	
		m_pTblRule = new TblRuleData(*m_pSymTable, m_pSqlConnection, tableName);
	}
	if (m_pTblRule->m_arSqlTableJoinInfoArray.GetSize() == 0)
	{
		SqlTableInfo* pSqlTableInfo = m_pSqlConnection->GetTableInfo(tableName);
		ASSERT(pSqlTableInfo);
		m_pTblRule->AddJoinTable(pSqlTableInfo);
	}
	
	CAddCalcColumn AddCalcColDlg( m_pTblRule, m_pSymTable, m_PublicCalculatedColumnsLinks, m_PublicFieldsLinks );

	if (AddCalcColDlg.DoModal() == IDOK)
		{	
			if (bHiddenCalcColumn)
				m_PublicCalculatedColumnsLinks.Add(new PublicFieldObj(AddCalcColDlg.m_publicName, PublicFieldObj::HIDDEN, AddCalcColDlg.m_type , FALSE /*special*/, AddCalcColDlg.m_physicalName));
			else
				m_PublicCalculatedColumnsLinks.Add(new PublicFieldObj(AddCalcColDlg.m_publicName, PublicFieldObj::SELECTED, AddCalcColDlg.m_type , FALSE /*special*/, AddCalcColDlg.m_physicalName));
				
			ShowCalculatedColumns();
			m_mlbTables.SetFlag(m_mlbTables.GetCurSel(), SEL_BMP);
			m_mlbTables.		EnableWindow(FALSE);
			
			if (m_pTblRule == NULL)
			{
				m_pTblRule = new TblRuleData(*m_pSymTable, m_pSqlConnection, GetCurTableName());
			}
			ShowWclause();			
			
			m_cbTableGroups.	EnableWindow(FALSE);
			m_mlbTables.		EnableWindow(FALSE);
			
			EnableSelectionControls(TRUE, FALSE, FALSE, FALSE);	
			EnableCalcColumnControls(TRUE, TRUE, FALSE);
			m_btnAddJoin.EnableWindow(FALSE);
			m_btnUpdateJoin.EnableWindow(FALSE);

			if (m_pDispTableInfo)
			{
				if (m_pDispTableInfo->GetEditDispTableName().IsEmpty())
					m_pDispTableInfo->SetDispTableName(GetCurTableName());

				if (!m_pDispTableInfo->AreControlsVisible())
					m_pDispTableInfo->ShowControls(this, TRUE);
			}
		}
	else if (b_ruleIsNull)
		{	
			delete	m_pTblRule;
			m_pTblRule = NULL;
		}
	
}

//----------------------------------------------------------------------------
void CTblRuleDlg::RemoveCalculatedColumn()
{
	int selected = m_mlbCalculatedCols.GetCurSel();
    if (selected == LB_ERR)
	 {	 AfxMessageBox(_TB("Select calculated column to remove before"), MB_OK | MB_ICONEXCLAMATION);
		 return;
	 }
	CString selectedString;
	selectedString = m_mlbCalculatedCols.GetString1(selected);

	for (int i = 0; i < m_PublicCalculatedColumnsLinks.GetSize(); i++)
    {
        PublicFieldObj* pPFO = m_PublicCalculatedColumnsLinks[i];
		if	(pPFO->m_physicalName.Compare(selectedString) == 0)
		{
			if	((pPFO->m_Selection == PublicFieldObj::OLD_SELECTED)||
				(pPFO->m_Selection == PublicFieldObj::OLD_HIDDEN))
		           AfxMessageBox(_TB("Cannot remove calculated columns saved previously"), MB_OK | MB_ICONEXCLAMATION);
	        else
			{
				m_PublicCalculatedColumnsLinks.RemoveAt(i,1);
				ShowCalculatedColumns();
				if (m_mlbCalculatedCols.GetCount() != 0)
				{
				  m_mlbCalculatedCols.SetCurSel(0);
			
			      WPARAM wParam = MAKEWPARAM(IDC_CALCCOLUMNS_LIST, LBN_SELCHANGE);
				  LPARAM lParam = (LPARAM)m_mlbCalculatedCols.m_hWnd;
				  PostMessage(WM_COMMAND, wParam, lParam);
				}
			}
			break;
		}
	}
	if (m_PublicCalculatedColumnsLinks.GetSize() == 0)
	{		
		EnableCalcColumnControls(TRUE, FALSE, FALSE);
		EnableSelectionControls(FALSE, FALSE, FALSE, TRUE);
	}
	CheckUndoAll(); //controlla se si sono deselezionate tutte le selezioni, in tal caso cancella nuova TblRule e riabilita tabelle
}

//----------------------------------------------------------------------------
void CTblRuleDlg::EditCalculatedColumn()
{
 //TODO
 //se non e' selected o hidden  ma old selected o old hidden niente,altrimenti permetto di modificare tutto
  int selected = m_mlbCalculatedCols.GetCurSel();
  CString selectedString;
  selectedString = m_mlbCalculatedCols.GetString1(selected);
  for (int i = 0; i < m_PublicCalculatedColumnsLinks.GetSize(); i++)
    {
        PublicFieldObj* pPFO = m_PublicCalculatedColumnsLinks[i];
		if	(pPFO->m_physicalName.Compare(selectedString) == 0)
		{
			if	((pPFO->m_Selection == PublicFieldObj::SELECTED)||
				(pPFO->m_Selection == PublicFieldObj::HIDDEN))
		    {
				CAddCalcColumn AddCalcColDlg( m_pTblRule, m_pSymTable, m_PublicCalculatedColumnsLinks, m_PublicFieldsLinks, pPFO->m_physicalName, pPFO->m_strName, pPFO->m_DataType, i);
	
                if (AddCalcColDlg.DoModal() == IDOK)
				{	
					pPFO->m_strName = AddCalcColDlg.m_publicName;
					pPFO->m_DataType = AddCalcColDlg.m_type;	 				  
					pPFO->m_physicalName = AddCalcColDlg.m_physicalName;
				}
			}
			ShowCalculatedColumns();
		    break; 
		}
	}
}

//----------------------------------------------------------------------------
void CTblRuleDlg::UpdateJoin()
{
    BOOL oldJoin = FALSE;
	if (m_pTblRule)
	 {
		 //if there was only a table before, it's need qualify selected field by dot notation
		 if (m_pTblRule->m_arSqlTableJoinInfoArray.GetSize() == 1)
		 {
			 CString tableName = m_pTblRule->m_arSqlTableJoinInfoArray.GetAt(0)->GetTableName();
			 // selected field, where clause, order by,  group by, having
			 if (m_pTblRule->m_arSqlTableJoinInfoArray.m_arFieldLinks.GetSize() > 0)
			 {
				 for (int i = 0; i < m_pTblRule->m_arSqlTableJoinInfoArray.m_arFieldLinks.GetAt(0)->GetSize(); i++)
				 {
					 DataFieldLink*  pObjLink = (DataFieldLink*) m_pTblRule->m_arSqlTableJoinInfoArray.m_arFieldLinks.GetAt(0)->GetAt(i);
					 if (pObjLink->m_strPhysicalName.Find(DOT_CHAR) < 0)  //is not qualified
					   pObjLink->m_strPhysicalName = tableName + DOT_CHAR + pObjLink->m_strPhysicalName;
				 }	
			 }
		 }
				
		 int idxRule = m_pRuleData->GetRuleIndex(GetCurTableName(), RULE_DATA_TABLE, GetCurReplicTable() /*nReplic*/);
		
		CAddJoin addJoinDialog (this, TRUE /*oldJoin*/, idxRule);

		if (addJoinDialog.DoModal() == IDOK)
		{
	        ShowTableGroups();
		}
	}
}
//----------------------------------------------------------------------------
void CTblRuleDlg::AddJoin()
{
  CAddJoin addJoinDialog (this, FALSE/*oldJoin*/);
  if (addJoinDialog.DoModal() == IDOK)
  {
		ShowTableGroups();
  }
}

//----------------------------------------------------------------------------
void CTblRuleDlg::AddLinks(BOOL bAddHiddenLinks)
{
    if (!bAddHiddenLinks && m_bOnlyOneLink)
    {
	    for (int i = 0; i < m_PublicFieldsLinks.GetSize(); i++)
	    {
	        PublicFieldObj* pPFO = m_PublicFieldsLinks[i];
			if	(
					//pPFO->m_Selection == PublicFieldObj::HIDDEN	||
		 			pPFO->m_Selection == PublicFieldObj::SELECTED
			 	)
			{
				AfxMessageBox(_TB("ONLY ONE link can be added!"), MB_OK | MB_ICONEXCLAMATION);
					        
				if (m_mlbFields.GetCount() > 0)
				    m_mlbFields.SelItemRange(FALSE, 0, m_mlbFields.GetCount() - 1);

				return;
			}
		}
		for (int i = 0; i < m_PublicCalculatedColumnsLinks.GetSize(); i++)
			{
				PublicFieldObj* pPFO = m_PublicCalculatedColumnsLinks[i];
				if	(
						//pPFO->m_Selection == PublicFieldObj::HIDDEN	||
		 				pPFO->m_Selection == PublicFieldObj::SELECTED
			 		)
				{
					AfxMessageBox(_TB("ONLY ONE link can be added!"), MB_OK | MB_ICONEXCLAMATION);
						        
					if (m_mlbFields.GetCount() > 0)
						m_mlbFields.SelItemRange(FALSE, 0, m_mlbFields.GetCount() - 1);

					return;
				}
			}
	}
    if (m_bOnlyOneLink || m_mlbFields.GetSelCount() <= 1)
        AddOneLink(bAddHiddenLinks);
    else
        AddMultiLink(bAddHiddenLinks);

	if (m_pTblRule == NULL)
	{
		m_pTblRule = new TblRuleData(*m_pSymTable, m_pSqlConnection, GetCurTableName());
		ShowWclause();
	}
	if (m_bIsJoin && m_pTblRule && m_pTblRule->IsTemporaryJoin())
		 ShowWclause(); 

    if (m_mlbFields.GetCount() > 0)
	    m_mlbFields.SelItemRange(FALSE, 0, m_mlbFields.GetCount() - 1);
    	       
	m_cbTableGroups.	EnableWindow(FALSE);
	m_mlbTables.		EnableWindow(FALSE);

	EnableSelectionControls(TRUE, FALSE, FALSE, FALSE);
    m_btnAddJoin.EnableWindow(FALSE);
	m_btnUpdateJoin.EnableWindow(FALSE);
	

	if (m_pDispTableInfo)
	{
		if (m_pDispTableInfo->GetEditDispTableName().IsEmpty())
			m_pDispTableInfo->SetDispTableName(GetCurTableName());

		if (!m_pDispTableInfo->AreControlsVisible())
			m_pDispTableInfo->ShowControls(this, TRUE);
	}

	m_mlbFields.SetFocus();
}

//----------------------------------------------------------------------------
void CTblRuleDlg::AddOneLink(BOOL bAddHiddenLinks)
{
	if	(
			m_mlbFields.GetCurSel() == LB_ERR		||
			m_mlbFields.GetSel(m_mlbFields.GetCurSel()) == 0
		)
		return;
		
	int nCurrIdx = GetLinkIdxFromFieldListIdx(m_mlbFields.GetCurSel());
	PublicFieldObj*	pPFO = m_PublicFieldsLinks[nCurrIdx];

	if	(
			pPFO->m_Selection != PublicFieldObj::NONE							&&
			(pPFO->m_Selection != PublicFieldObj::HIDDEN || bAddHiddenLinks)	&&
	 		(pPFO->m_Selection != PublicFieldObj::SELECTED || !bAddHiddenLinks)
	 	)
		return;

	if (ExistPublicName(pPFO->m_strName, nCurrIdx, TRUE))
	{
		CString str = m_pSqlTableInfo->GetAt(nCurrIdx)->GetColumnName();
		str += CString(": ") + pPFO->m_strName;
		AfxMessageBox
			(
				cwsprintf(_TB("As the field name has already been used, the following coupling was done: {0-%s}"),	(LPCTSTR) str),
				MB_OK | MB_ICONERROR
			);
		return;
	}
	if (AfxGetTokensTable()->GetKeywordsToken(pPFO->m_strName) != T_NOTOKEN)
	{
		AfxMessageBox
			(
				_TB("The name collides with a reserved word of TaskBuilder"),
				MB_OK | MB_ICONERROR
			);
		return;
	}

	pPFO->m_Selection = bAddHiddenLinks ? PublicFieldObj::HIDDEN : PublicFieldObj::SELECTED;

	m_mlbTables.SetFlag(m_mlbTables.GetCurSel(), SEL_BMP);
	m_mlbFields.SetFlag
		(
			m_mlbFields.GetCurSel(),
			bAddHiddenLinks
				? (pPFO->m_bSpecial	? HIDKEY_BMP : HID_BMP)
				: (pPFO->m_bSpecial	? SELKEY_BMP : SEL_BMP)
		);
}

//----------------------------------------------------------------------------
void CTblRuleDlg::AddMultiLink(BOOL bAddHiddenLinks)
{
    int     nItems;
    int*    pItemVector;

    if ((nItems = m_mlbFields.GetSelCount()) <= 0)
    	return;

    pItemVector = new int[nItems];
    m_mlbFields.GetSelItems(nItems, pItemVector);

	CMessages mess;
	mess.Add(_TB("As the field name has already been used, the following couplings were done:"), CMessages::MSG_HINT);
	
    for (int i = 0; i < nItems; i++)
    {
    	int nIdx = *(pItemVector + i);
		int nCurrIdx = GetLinkIdxFromFieldListIdx(nIdx);

		PublicFieldObj*	pPFO = m_PublicFieldsLinks[nCurrIdx];

		if	(
				pPFO->m_Selection != PublicFieldObj::NONE							&&
				(pPFO->m_Selection != PublicFieldObj::HIDDEN || bAddHiddenLinks)	&&
		 		(pPFO->m_Selection != PublicFieldObj::SELECTED || !bAddHiddenLinks)
		 	)
			continue;
		
		if (ExistPublicName(pPFO->m_strName, nCurrIdx, TRUE))
		{
			CString strCurrPhysicalName = m_pSqlTableInfo->GetAt(nCurrIdx)->GetColumnName();
			mess.Add(strCurrPhysicalName + _T(": ") + pPFO->m_strName);
			continue;
		}
		if (AfxGetTokensTable()->GetKeywordsToken(pPFO->m_strName) != T_NOTOKEN)
		{
			mess.Add(_TB("The name collides with a reserved word of TaskBuilder")+ _T(": ") + pPFO->m_strName);			
			continue;
		}

		pPFO->m_Selection = bAddHiddenLinks ? PublicFieldObj::HIDDEN : PublicFieldObj::SELECTED;
		
		m_mlbFields.SetFlag
			(
				nIdx,
				bAddHiddenLinks
					? (pPFO->m_bSpecial	? HIDKEY_BMP : HID_BMP)
					: (pPFO->m_bSpecial	? SELKEY_BMP : SEL_BMP)
			);	
    }                

	m_mlbTables.SetFlag(m_mlbTables.GetCurSel(), SEL_BMP);
    delete [] pItemVector;                     

	if (!mess.ErrorFound())
		return;

	if (mess.GetUpperBound() == 1)    
		AfxMessageBox
			(
				cwsprintf(_TB("As the field name has already been used, the following coupling was done: {0-%s}"),	(LPCTSTR) mess.GetMessageLine(1)),
				MB_OK | MB_ICONINFORMATION
			);
	else
		mess.Show(TRUE);
}

//----------------------------------------------------------------------------
void CTblRuleDlg::UndoAction()
{
	int nIdx;

    if (m_bOnlyOneLink || m_mlbFields.GetSelCount() <= 1)
    {
	    PublicFieldObj* pPFO = GetLinkFromFieldListIdx(m_mlbFields.GetCurSel());
	
		if	(
				pPFO->m_Selection == PublicFieldObj::HIDDEN		||
		 		pPFO->m_Selection == PublicFieldObj::SELECTED
		 	)
		{
			pPFO->m_Selection = PublicFieldObj::NONE;
			nIdx = m_mlbFields.GetCurSel();

			if (IsDlgButtonChecked(IDC_TBLRULE_SHOW_ALL_FIELD))
				m_mlbFields.SetFlag(nIdx, pPFO->m_bSpecial ? KEY_BMP : NO_BMP);
	   		else
	   			m_mlbFields.DelString(nIdx);
		}
	}
    else
    {
	    int     nItems = m_mlbFields.GetSelCount();
	    int*    pItemVector;
	
	    if (nItems <= 0) return;
	
	    pItemVector = new int[nItems];
	    m_mlbFields.GetSelItems(nItems, pItemVector);

		// si deve andare all'indietro perche`, se si e` in stato di visualizzazione
		// dei soli campi selezionati, si effettua la contestuale cancellazione
	    for (int i = nItems - 1; i >= 0; i--)
	    {
		    PublicFieldObj* pPFO = GetLinkFromFieldListIdx(*(pItemVector + i));

			if	(
					pPFO->m_Selection != PublicFieldObj::HIDDEN		&&
			 		pPFO->m_Selection != PublicFieldObj::SELECTED
			 	)
			 	continue;
	
			pPFO->m_Selection = PublicFieldObj::NONE;
			nIdx = *(pItemVector + i);
		
			if (IsDlgButtonChecked(IDC_TBLRULE_SHOW_ALL_FIELD))
	   			m_mlbFields.SetFlag(nIdx, pPFO->m_bSpecial ? KEY_BMP : NO_BMP);
	   		else
	   			m_mlbFields.DelString(nIdx);
	    }                
	    
		delete []pItemVector;                     
	}

	if (m_mlbFields.GetCount() > 0)
	{
		EnableSelectionControls(FALSE, FALSE, FALSE, TRUE);
		m_mlbFields.SelItemRange(FALSE, 0, m_mlbFields.GetCount() - 1);
	}
	else
	{
		// la listbox si puo` essere svuotata solo se e` stato fatto l'undo in stato
		// di visualizzazione dei soli campi selezionati, in tal case si ritorna in stato
		// di visualizzazione di tutti i campi e si ririempire la listbox
		EnableSelectionControls(FALSE, TRUE, FALSE, TRUE);
		ShowFieldList();
	}

	CheckUndoAll(); //controlla se si sono deselezionate tutte le selezioni, in tal caso cancella nuova TblRule e riabilita tabelle
	
	m_mlbFields.SetFocus();
}

//----------------------------------------------------------------------------

int CTblRuleDlg::GetLinkIdxFromFieldListIdx(int nIdx)
{
	int i = m_pSqlTableInfo->GetColumnInfoPos(m_mlbFields.GetString1(nIdx));
	ASSERT(i >= 0);
	return i;
	//CString s = m_mlbFields.GetString1(nIdx);
	//int idx = -1;
	//for (int i = 0; i < m_pTblRule->m_arSqlTableInfo.GetSize(); i++)
	//{
	//	idx = m_pTblRule->m_arSqlTableInfo.GetAt(i)->GetColumnInfoPos(s);
	//	if (idx >= 0) 
	//		break;
	//}
	//ASSERT(idx >= 0);
	//return idx;
}

//----------------------------------------------------------------------------
BOOL CTblRuleDlg::ExistPublicName(CString& strName, int nCurrIdx, BOOL bRetry)
{
 	//@@TODO non serve se viene abilitata la "qualifica" predisposta nella LoadDictionary
	CString	strOldName = strName + _T("_") + GetCurTableName();
	
	for (int nRetryCount = 0; nRetryCount < 32767; nRetryCount++)
	{
		BOOL bExist = FALSE;

	    if (m_pSymTable && m_pSymTable->ExistField(strName))
			bExist = TRUE;
		else
	    {
		    for (int i = 0; i < m_PublicFieldsLinks.GetSize() && !bExist; i++)
		    {
		    	if (i == nCurrIdx)
		    		continue;
		    		
		        PublicFieldObj* pPFO = m_PublicFieldsLinks[i];
		        if	(
		        		pPFO->m_Selection != PublicFieldObj::NONE		&&
		        		pPFO->m_Selection != PublicFieldObj::BAD_TYPE	&&
		        		strName.CompareNoCase(pPFO->m_strName) == 0
		        	)
		            bExist = TRUE;
		    }
			for (int i = 0; i < m_PublicCalculatedColumnsLinks.GetSize() && !bExist; i++)
		    {
		    	if (i == nCurrIdx)
		    		continue;
		    		
		        PublicFieldObj* pPFO = m_PublicCalculatedColumnsLinks[i];
		        if	(
		        		pPFO->m_Selection != PublicFieldObj::NONE		&&
		        		pPFO->m_Selection != PublicFieldObj::BAD_TYPE	&&
		        		strName.CompareNoCase(pPFO->m_strName) == 0
		        	)
		            bExist = TRUE;
		    }
	    }

    	if (!bExist)
    	{
			// non e` stato trovato pero` ritorna TRUE se ha fatto piu` tentativi
    		return nRetryCount > 0;
    	}

		if (!bRetry)
			return TRUE;

		if (nRetryCount == 0)
			strName = strOldName;
		else
			strName = strOldName + cwsprintf(_T("_%d"), nRetryCount);
	}

    return TRUE;
}



//----------------------------------------------------------------------------
void CTblRuleDlg::CheckUndoAll()
{		
	BOOL bUnselectTable = TRUE;
	for (int i = 0; i < m_PublicFieldsLinks.GetSize(); i++)
	{
	    PublicFieldObj* pPFO = m_PublicFieldsLinks[i];
		if	(
				pPFO->m_Selection == PublicFieldObj::HIDDEN		||
				pPFO->m_Selection == PublicFieldObj::SELECTED
			)
		{
			bUnselectTable = FALSE;
			break;
		}
	}
//controllo che non ci siano nuove colonne calcolate
	for (int i = 0; i < m_PublicCalculatedColumnsLinks.GetSize(); i++)
	{
	    PublicFieldObj* pPFO = m_PublicCalculatedColumnsLinks[i];
		if	(
				pPFO->m_Selection == PublicFieldObj::HIDDEN		||
				pPFO->m_Selection == PublicFieldObj::SELECTED
			)
		{
			bUnselectTable = FALSE;
			break;
		}
	}

	if (bUnselectTable)
	{
/**/    m_btnAddJoin.EnableWindow(TRUE);
		m_btnUpdateJoin.EnableWindow(TRUE);
		m_cbTableGroups.EnableWindow(TRUE);
    	m_mlbTables.EnableWindow(TRUE);
		if (m_bNewRule)
		{
			if (m_pTblRule)
			{
				delete m_pTblRule;
				m_pTblRule = NULL;
			}

			m_mlbTables.SetFlag(m_mlbTables.GetCurSel(), NO_BMP);
			ShowWclause();

			if (m_pDispTableInfo)
				m_pDispTableInfo->SetDispTableName(_T(""));
		}
		if (m_pTblRule && m_pTblRule->IsTemporaryJoin() && m_bIsJoin)
		{
			ShowWclause();
			if (m_pDispTableInfo)
				m_pDispTableInfo->SetDispTableName(_T(""));
		}

		if (m_pDispTableInfo && m_pDispTableInfo->AreControlsVisible())
			m_pDispTableInfo->ShowControls(this, FALSE);
	}
}

//----------------------------------------------------------------------------
void CTblRuleDlg::SetConstraintMode()
{
	m_pTblRule->m_ConstraintMode = RuleSelectionMode::SEL_ALL;
	switch (m_cbConstrainMode.GetCurSel())
	{
		case 0:	m_pTblRule->m_ConstraintMode   = RuleSelectionMode::SEL_ALL;		break;
		case 1: m_pTblRule->m_ConstraintMode   = RuleSelectionMode::SEL_NOT_NULL;	break;
		case 2: m_pTblRule->m_ConstraintMode   = RuleSelectionMode::SEL_NULL;		break;
	}
}

//----------------------------------------------------------------------------
BOOL CTblRuleDlg::Save()
{
	if	(
			m_pDispTableInfo &&
			m_pDispTableInfo->AreControlsVisible() &&
			!m_pDispTableInfo->CheckControls()
		)
		return FALSE;

	if (m_pTblRule->m_arSqlTableJoinInfoArray.GetSize() == 0)
	{	
		m_pTblRule->m_arSqlTableJoinInfoArray.Add(m_pSqlConnection, m_pSymTable, m_pSqlTableInfo);
	}
	SqlTableInfoArray arTI; 
	arTI.Append(m_pTblRule->m_arSqlTableJoinInfoArray);
      
	WClause wclauseTmp(m_pSqlConnection, m_pSymTable, arTI);
    wclauseTmp.SetNative(IsDlgButtonChecked(IDC_WCLAUSE_BTN_NATIVE_SQL));

	m_edtWClause.SetTableInfo(arTI, m_pTblRule);
    if (!m_edtWClause.CheckWC(wclauseTmp))
    {
    	m_edtWClause.SetCtrlFocus(TRUE);
        return FALSE;
	}

    SendDlgItemMessage(IDC_WCLAUSE_BTN_NATIVE_SQL, BM_SETCHECK, wclauseTmp.IsNative());

    m_pTblRule->AssignWhereClause(wclauseTmp);
    
   //----
	WClause hclauseTmp(m_pSqlConnection, m_pSymTable, arTI);
    hclauseTmp.SetNative(IsDlgButtonChecked(IDC_HCLAUSE_BTN_NATIVE_SQL));

	m_edtHClause.SetTableInfo(arTI, m_pTblRule);
    if (!m_edtHClause.CheckWC(hclauseTmp))
    {
    	m_edtHClause.SetCtrlFocus(TRUE);
        return FALSE;
	}

    SendDlgItemMessage(IDC_HCLAUSE_BTN_NATIVE_SQL, BM_SETCHECK, hclauseTmp.IsNative());

    m_pTblRule->AssignHavingClause(hclauseTmp);

	//---- WHEN clause
	//TODO save prev expr
 	if (!m_edtWhenClause.CheckExp(*m_pTblRule->m_pWhenExpr, DATA_BOOL_TYPE, TRUE))
	{   
		// reselect edit field and focus it                 
		m_edtWhenClause.SetCtrlFocus(TRUE);
		return FALSE;
	}
	//----

	if (!m_edtOrderBy.IsEmpty())
	{
		CString	strTmp;
	
		m_edtOrderBy.GetWindowText(strTmp);

		Parser lex(strTmp);
		
		if (!ParseOrderBy(lex, m_pSymTable, &arTI, m_pTblRule->m_strOrderBy))
		{
			m_edtOrderBy.SetCtrlFocus(TRUE);
		    return FALSE;
		}
	}
	else
		m_pTblRule->m_strOrderBy.Empty();
	
	//----
	if (!m_edtGroupBy.IsEmpty())
	{	
		CStringArray arColumnsGroupBy;
		CString columnSelected;
		
		CString	strTmp;
	
		m_edtGroupBy.GetWindowText(strTmp);

		Parser lex(strTmp);
		//bQualified False because  in PublicFieldObj physical name isn't qualified
		if (!ParseOrderBy(lex, m_pSymTable, &arTI, m_pTblRule->m_strGroupBy, TRUE/*bQualified*/, &arColumnsGroupBy))
		{
			m_edtGroupBy.SetCtrlFocus(TRUE);
		    return FALSE;
		}

		// check list columns(not calculated) selected  in select statement are present in group by clause
   
		for (int i = 0; i < m_PublicFieldsLinks.GetSize(); i++)
		{
			PublicFieldObj* pPFO = m_PublicFieldsLinks[i];
			if	(
					pPFO->m_Selection != PublicFieldObj::NONE		&&
					pPFO->m_Selection != PublicFieldObj::BAD_TYPE
		 	)
			{
				columnSelected = m_pSqlTableInfo->GetTableName() + DOT_CHAR + pPFO->m_physicalName;
				
				if ( CStringArray_Find(arColumnsGroupBy, columnSelected) == -1 ) 
				{	
					AfxMessageBox(
									_TB("Selected columns must be in Group By clause or in an aggregate function "),
									MB_OK | MB_ICONEXCLAMATION
								 );
					return FALSE;
				}
			}
		}
	}
	else
	{
		m_pTblRule->m_strGroupBy.Empty();
	}
	//----
	m_pTblRule->SetDistinct(IsDlgButtonChecked(IDC_BTN_DISTINCT_SQL));
	m_pTblRule->SetTemporaryJoin (FALSE);
	m_pTblRule->m_nTop = m_edtTopRows.GetValue();

    if (m_bNewRule)
        m_pRuleData->Add(m_pTblRule);

	// last Table Rule managed
    m_pRuleData->m_pLastTblRule = m_pTblRule;

    SetConstraintMode ();

	CTBNamespace* pNs = NULL;
	if (m_pDocument)
		pNs = &m_pDocument->GetNamespace();

    for (int i = 0; i < m_PublicFieldsLinks.GetSize(); i++)
    {
        PublicFieldObj* pPFO = m_PublicFieldsLinks[i];
		if	(
				pPFO->m_Selection != PublicFieldObj::HIDDEN	&&
		 		pPFO->m_Selection != PublicFieldObj::SELECTED
		 	)
		 	continue;
		 	
		SqlColumnInfo* pSqlColumnInfo = m_pSqlTableInfo->GetAt(i);
		ASSERT(pSqlColumnInfo);
//qui devo passargli il num di tabella nel join..
		CString strCurrTableName;
		if (m_mlbTables.GetCurSel() != LB_ERR)
	    {
			strCurrTableName = GetCurTableName();
		  //questo for potrebbe diventare un metodo di tblRule
			int k;
			for (k = 0; k < m_pTblRule->m_arSqlTableJoinInfoArray.GetSize(); k++)
			{
				if (strCurrTableName.Compare(m_pTblRule->m_arSqlTableJoinInfoArray.GetAt(k)->GetTableName()) == 0)
					break;
			}
			//pSqlColumnInfo->GetColumnName() se e' un join devo aggiungerlo con sintassi qualificata
	        CString physicalName;
			
			if (m_pTblRule->m_arSqlTableJoinInfoArray.GetSize() > 1) //e' un join
				physicalName = pSqlColumnInfo->GetTableName() + DOT_CHAR + pSqlColumnInfo->GetColumnName();
			else
			{
				physicalName = pSqlColumnInfo->GetColumnName();
				if (AfxGetTokensTable()->GetKeywordsToken(physicalName) != T_NOTOKEN)
					physicalName = pSqlColumnInfo->GetTableName() + DOT_CHAR + physicalName;
			}
			
			m_pTblRule->AddLink(physicalName, pPFO->m_strName, FALSE, pPFO->m_DataType, k);
		}		
        WoormField* pRepFieldData = new WoormField(pPFO->m_strName);			
		pRepFieldData->SetDataType		 (pPFO->m_DataType);

		pRepFieldData->SetLen            (AfxGetFormatStyleTable()->GetInputCharLen(pPFO->m_DataType, pNs));
        pRepFieldData->SetHidden         (pPFO->m_Selection == PublicFieldObj::HIDDEN);
        pRepFieldData->SetTableRuleField (TRUE);

		pRepFieldData->SetNumDec         (pSqlColumnInfo->GetColumnDecimal());

        m_pSymTable->Add (pRepFieldData);

        if (pPFO->m_Selection == PublicFieldObj::SELECTED)
		{
			SymField* pF = m_pSymTable->GetField(pPFO->m_strName);
			ASSERT(pF);
			m_IdArray.Add(pF->GetId());
		}
	}

	//ciclare analogamente su calculated columns...
	for (int i = 0; i < m_PublicCalculatedColumnsLinks.GetSize(); i++)
    {
        PublicFieldObj* pPFO = m_PublicCalculatedColumnsLinks[i];
		if	(
				pPFO->m_Selection != PublicFieldObj::HIDDEN	&&
		 		pPFO->m_Selection != PublicFieldObj::SELECTED
		 	)
		 	continue;
		
		m_pTblRule->AddLink(pPFO->m_physicalName, pPFO->m_strName, TRUE, pPFO->m_DataType);

		WoormField* pRepFieldData = new WoormField(pPFO->m_strName);			
		pRepFieldData->SetDataType		 (pPFO->m_DataType);

		pRepFieldData->SetLen            (AfxGetFormatStyleTable()->GetInputCharLen(pPFO->m_DataType, pNs));
		pRepFieldData->SetHidden         (pPFO->m_Selection == PublicFieldObj::HIDDEN);
        pRepFieldData->SetTableRuleField (TRUE);

        m_pSymTable->Add (pRepFieldData);

        if (pPFO->m_Selection == PublicFieldObj::SELECTED)
		{
			SymField* pF = m_pSymTable->GetField(pPFO->m_strName);
			ASSERT(pF);
			m_IdArray.Add(pF->GetId());
		}
	}
	return TRUE;
}

//----------------------------------------------------------------------------
void CTblRuleDlg::OnApplySelection()
{
	if (m_pTblRule == NULL)	
		return;
	
	if (!Save())
		return;

	m_bIsApplyPressed = TRUE; //e' stato fatto un apply, se esco poi con tasto cancel devo comunque disegnare

	m_bNewRule = FALSE;
	m_bIsJoin = FALSE;

	m_edtPublicName.			EnableWindow(TRUE);
	m_edtWClause.    			EnableWindow(TRUE);
    m_edtOrderBy.    			EnableWindow(TRUE);
    m_edtHClause.    			EnableWindow(TRUE);
    m_edtGroupBy.    			EnableWindow(TRUE);
    m_edtWhenClause.    		EnableWindow(TRUE);
    m_btnAddLink.				EnableWindow(TRUE);
    m_btnAddLinkHid.			EnableWindow(TRUE);
    m_btnUndo.					EnableWindow(TRUE);
	m_btnAddCalcColumn.			EnableWindow(TRUE);
	m_btnAddHiddenCalcColumn.	EnableWindow(TRUE);
	m_btnRemoveCalcColumn.		EnableWindow(TRUE);
	m_btnAddJoin.				EnableWindow(TRUE);
	m_btnUpdateJoin.			EnableWindow(TRUE);
	m_btnApply.					EnableWindow(TRUE);

	m_cbTableGroups.			EnableWindow(TRUE);
	m_mlbTables.				EnableWindow(TRUE);
    m_edtPublicName.			EnableWindow(FALSE);
	m_cbDataObjTypes.			EnableWindow(FALSE);
	
	SendDlgItemMessage(IDC_BTN_DISTINCT_SQL, BM_SETCHECK, FALSE);
	GetDlgItem(IDC_BTN_DISTINCT_SQL)->EnableWindow(FALSE);

// viene messa in editing l'ultima rule utilizzata	
    if (m_pTblRule == NULL)
        m_pTblRule = m_pRuleData->m_pLastTblRule;

	ShowTableGroups();
}

//----------------------------------------------------------------------------
void CTblRuleDlg::OnOK()
{                                          
	if (m_pTblRule == NULL)
	{
		EndDialog(IDCANCEL);
		return;
	}
	if (Save())
		EndDialog(IDOK);
	else
		return;
}

//----------------------------------------------------------------------------
void CTblRuleDlg::OnCancel()
{
	if (m_pTblRule && m_bNewRule )
		delete m_pTblRule;
	
	if (m_pRuleData)
	{
		for (int i = 0; i < m_pRuleData->GetSize(); i++)   
		{  
			RuleDataObj* pRuleDataObj = (RuleDataObj*) m_pRuleData->GetAt(i);
			if 
				(
				pRuleDataObj->IsARule() == RULE_DATA_TABLE
				&& 
					(((TblRuleData*)pRuleDataObj)->m_arSqlTableJoinInfoArray.m_arFieldLinks.GetSize() == 0)
				&&
					(((TblRuleData*)pRuleDataObj)->m_CalcColumnLinks.GetSize() == 0)	
				)
			{
				m_pRuleData->RemoveAt(i);
				i--;
			}
		}
	}
	EndDialog(IDCANCEL);
}

//----------------------------------------------------------------------------
INT_PTR CTblRuleDlg::OnToolHitTest(CPoint point, TOOLINFO* pTI) const
{
	if (::OnToolHitTest
			(
				this,
				m_mlbTables, 
				point, 
				pTI, 
				_T("")
			)
		)
	{
		return IDC_TABLE_LIST;
	}

	if (::OnToolHitTest
			(
				this,
				m_mlbFields, 
				point, 
				pTI, 
				m_arSqlTableInfo.GetSize() ?  m_arSqlTableInfo.GetAt(0)->GetTableName() :  _T("")
			)
		)	
	{
		return IDC_FIELD_LIST;
	}

	if (::OnToolHitTestTableName
			(
				this,
				m_stcTblOfRule,
				point, 
				pTI
			)
		)
	{
		return IDC_TBL_JOIN1;
	}

	if (::OnToolHitTestCriterion
			(
				this,
				m_edtWClause,
				point, 
				pTI,
				m_pSqlConnection
			)
		)
	{
		return IDC_WCLAUSE;
	}

    if (::OnToolHitTestCriterion
			(
				this,
				m_edtGroupBy,
				point, 
				pTI,
				m_pSqlConnection
			)
		)
	{
		return IDC_GROUP_BY;
	}

	if (::OnToolHitTestCriterion
			(
				this,
				m_edtHClause,
				point, 
				pTI,
				m_pSqlConnection
			)
		)
	{
		return IDC_HCLAUSE;
	}

	if (::OnToolHitTestCriterion
			(
				this,
				m_edtOrderBy,
				point, 
				pTI,
				m_pSqlConnection
			)
		)
	{
		return IDC_SORTBY;
	}

	if (::OnToolHitTestCriterion
			(
				this,
				m_edtWhenClause,
				point, 
				pTI,
				m_pSqlConnection
			)
		)
	{
		return IDC_WHEN_CLAUSE;
	}

	return __super::OnToolHitTest(point, pTI);
}

//============================================================================
//	CModTblRuleDlg
//============================================================================
IMPLEMENT_DYNAMIC(CModTblRuleDlg, CParsedDialog)
BEGIN_MESSAGE_MAP(CModTblRuleDlg, CParsedDialog)
	//{{AFX_MSG_MAP(CModTblRuleDlg)
	ON_EN_VALUE_CHANGED	(IDC_TBLFIELD_DATA_TYPE,		DataTypeChanged)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//----------------------------------------------------------------------------
CModTblRuleDlg::CModTblRuleDlg
	(
	    LPCTSTR			pszNameField,
	    RuleDataArray*       pRuleData,
	    TblRuleData*    pTblRule,
	    WoormTable*		pSymTable,
		BOOL			bAlsoModifyType,
		CString			strDocumentFixedQuery,
		SqlConnection*  pSqlConnection,
		BOOL			bShowAdvancedForm
	)
    :
    CParsedDialog			(bShowAdvancedForm ? IDD_MOD_TBLRULE_EX : IDD_MOD_TBLRULE),

	m_strPublicNameField	(pszNameField),
    m_pRuleData     		(pRuleData),
    m_pTblRule				(pTblRule),
    m_pSymTable				(pSymTable),
    m_edtWClause			(pSymTable, pRuleData),
    m_edtOrderBy			(NULL, NULL),
    m_edtHClause			(pSymTable, pRuleData),
    m_edtGroupBy			(NULL, NULL),
	m_bAlsoModifyType		(bAlsoModifyType),
	m_strDocumentFixedQuery	(strDocumentFixedQuery),
	m_pSqlConnection		(pSqlConnection),
	m_edtWhenClause			(pSymTable, NULL)
{
}

//----------------------------------------------------------------------------
CModTblRuleDlg::~CModTblRuleDlg ()
{
}

//----------------------------------------------------------------------------
BOOL CModTblRuleDlg::OnInitDialog()
{
	CParsedDialog::OnInitDialog();
	EnableToolTips();
	
	m_stcTableName.		SubclassDlgItem	(IDC_TBLNAME1,				this);
	m_stcPublicName.	SubclassEdit	(IDC_TBLFIELD_NAME,			this);
	m_cbDataObjTypes.	SubclassEdit	(IDC_TBLFIELD_DATA_TYPE,	this);
    m_cbConstrainMode.	SubclassDlgItem	(IDC_CONSTRAIN_MODE,   		this);
    m_edtWClause.		SubclassEdit	(IDC_WCLAUSE,       		this);
    m_edtOrderBy.		SubclassEdit	(IDC_SORTBY,	       		this);
    m_edtHClause.		SubclassEdit	(IDC_HCLAUSE,       		this);
    m_edtGroupBy.		SubclassEdit	(IDC_GROUP_BY,	       		this);
    m_edtWhenClause.	SubclassEdit	(IDC_WHEN_CLAUSE,	       	this);
	m_edtTopRows.		SubclassEdit	(IDC_TOP_ROWS,	       		this);
	m_edtTopRows.SetRange(0, 0x7fff);

	m_edtWClause.SetCheckMethod(this, (CHECK_EXPRESSION)&CModTblRuleDlg::OnCheckEdtWhereClause);
	m_edtOrderBy.SetCheckMethod(this, (CHECK_EXPRESSION)&CModTblRuleDlg::OnCheckEdtOrderClause);
	m_edtHClause.SetCheckMethod(this, (CHECK_EXPRESSION)&CModTblRuleDlg::OnCheckEdtHavingClause);
	m_edtGroupBy.SetCheckMethod(this, (CHECK_EXPRESSION)&CModTblRuleDlg::OnCheckEdtOrderClause);

	if (m_pTblRule && m_pTblRule->m_pWhenExpr)
	{
		CString s = m_pTblRule->m_pWhenExpr->ToString();
		m_edtWClause.SetWindowText(s);
	}

	SendDlgItemMessage(IDC_BTN_DISTINCT_SQL, BM_SETCHECK, FALSE);

	GetDlgItem(IDC_BTN_DISTINCT_SQL)->EnableWindow(FALSE);

	if (!m_strDocumentFixedQuery.IsEmpty())
	{
		GetDlgItem(IDC_STATIC_DOCUMENT_FILTER)->ShowWindow(SW_SHOW);
		GetDlgItem(IDC_DOCUMENT_FILTER)->ShowWindow(SW_SHOW);
		GetDlgItem(IDC_DOCUMENT_FILTER)->SetWindowText(m_strDocumentFixedQuery);
	}
	
	if (m_pTblRule == NULL || m_pTblRule->m_arSqlTableJoinInfoArray.GetSize() == 0/* || !LoadDictionary(m_pTblRule->GetTableName())*/)
    {
		EndDialog(IDCANCEL);
        return FALSE;
	}
	CString tableNames = CString("");
	m_arSqlTableInfo.Append(m_pTblRule->m_arSqlTableJoinInfoArray);
	
	for (int i = 0; i < m_arSqlTableInfo.GetSize(); i++)
					tableNames = tableNames +  m_arSqlTableInfo.GetAt(i)->GetTableName() + CString(", "); 
	tableNames = tableNames.Left(tableNames.GetLength() - 2);
	m_stcTableName.SetWindowText(tableNames);

	m_stcPublicName.SetValue(m_strPublicNameField);

	SymField* pField = m_pSymTable->GetField(m_strPublicNameField);
	DataObj* pDataObj = pField ? pField->GetData() : NULL;
	ASSERT(pDataObj);

	CString strPhysicalName = m_pTblRule->GetPhysicalNameOf(m_strPublicNameField);
	if (!m_pTblRule->IsNativeExpr(m_strPublicNameField))
	{
		const SqlColumnInfo* pSqlColumnInfo = GetColumnInfo(strPhysicalName);
		ASSERT(pSqlColumnInfo);

		// per visualizzare il tipo di dato correntemente selezionato si inizializza
		// il vettore dei tipi compatibili associato alla combobox
		if (pSqlColumnInfo)
			pSqlColumnInfo->GetDataObjTypes(m_cbDataObjTypes.GetDataTypes());
	}
	else
	{
		m_cbDataObjTypes.FillListBox();
	}

	m_cbDataObjTypes.FillListBox();
	m_cbDataObjTypes.SetTypeValue(pDataObj->GetDataType());

	// solo se chiamata dal wrmeng e` reso modificabile il tipo
	if (!m_bAlsoModifyType)
		m_cbDataObjTypes.EnableWindow(FALSE);

    ShowWclause         ();
    ShowConstraintMode  ();
    
	m_edtWClause.SetFocus();
	
	InitStatusBar		();	
	RecalcLayout		();
	ShowStatusBarInfo	();
	return TRUE;
}

//----------------------------------------------------------------------------
const SqlColumnInfo* CModTblRuleDlg::GetColumnInfo (CString strPhysicalName)
{
	const SqlColumnInfo* pSqlColumnInfo = NULL;
	for (int i = 0; i < m_pTblRule->m_arSqlTableJoinInfoArray.GetSize(); i++)
	{
		pSqlColumnInfo = m_pTblRule->m_arSqlTableJoinInfoArray.GetAt(i)->GetColumnInfo(strPhysicalName);
		if (pSqlColumnInfo) 
			break;
	}
	return pSqlColumnInfo;
}
//----------------------------------------------------------------------------
void CModTblRuleDlg::ShowConstraintMode()
{
    m_cbConstrainMode.ResetContent();
    m_cbConstrainMode.AddString(_TB("All data"));
    m_cbConstrainMode.AddString(_TB("Only NOT 'null' data "));
    m_cbConstrainMode.AddString(_TB("Only 'null' data"));

    if (m_pTblRule)
    	switch (m_pTblRule->m_ConstraintMode)
    	{
        	case RuleSelectionMode::SEL_ALL       : m_cbConstrainMode.SetCurSel(0);	break;
        	case RuleSelectionMode::SEL_NOT_NULL  : m_cbConstrainMode.SetCurSel(1);	break;
        	case RuleSelectionMode::SEL_NULL      : m_cbConstrainMode.SetCurSel(2);	break;
    	}
	else
		m_cbConstrainMode.SetCurSel(0);
}

//----------------------------------------------------------------------------
void CModTblRuleDlg::ShowWclause()
{
    if (m_pTblRule)
    {
    	CString strTmp;
    	
    	strTmp = m_pTblRule->m_pWhereClause->ToString();
		ConvertCString(strTmp, LF_TO_CRLF);
    	m_edtWClause.SetWindowText(strTmp); //filter changed
	   	m_edtWClause.SetTableInfo(m_arSqlTableInfo, m_pTblRule);

	    SendDlgItemMessage(IDC_WCLAUSE_BTN_NATIVE_SQL, BM_SETCHECK, m_pTblRule->m_pWhereClause->IsNative());
    	
    	strTmp = m_pTblRule->m_strOrderBy;
		ConvertCString(strTmp, LF_TO_CRLF);
    	m_edtOrderBy.SetWindowText(strTmp);
	   	m_edtOrderBy.SetTableInfo(m_arSqlTableInfo);  
 
		//----
    	strTmp = m_pTblRule->m_pHavingClause->ToString();
		ConvertCString(strTmp, LF_TO_CRLF);
    	m_edtHClause.SetWindowText(strTmp);
	   	m_edtHClause.SetTableInfo(m_arSqlTableInfo, m_pTblRule);  
	    
		SendDlgItemMessage(IDC_HCLAUSE_BTN_NATIVE_SQL, BM_SETCHECK, m_pTblRule->m_pHavingClause->IsNative());
    	
    	strTmp = m_pTblRule->m_strGroupBy;
		ConvertCString(strTmp, LF_TO_CRLF);
    	m_edtGroupBy.SetWindowText(strTmp);
	   	m_edtGroupBy.SetTableInfo(m_arSqlTableInfo);  

		//----WHEN
    	strTmp = m_pTblRule->m_pWhenExpr->ToString();
		ConvertCString(strTmp, LF_TO_CRLF);
    	m_edtWhenClause.SetWindowText(strTmp);

		//DISTINCT
		SendDlgItemMessage(IDC_BTN_DISTINCT_SQL, BM_SETCHECK, m_pTblRule->IsDistinct());
		GetDlgItem(IDC_BTN_DISTINCT_SQL)->EnableWindow(TRUE);

		m_edtTopRows.SetValue(m_pTblRule->m_nTop);
	}
    else
    {
   		m_edtWClause.SetWindowText(_T("")); //filter changed
   		m_edtWClause.SetTableInfo(NULL, NULL);

	    SendDlgItemMessage(IDC_WCLAUSE_BTN_NATIVE_SQL, BM_SETCHECK, FALSE);

   		m_edtOrderBy.SetWindowText(_T("")); //filter changed
	   	m_edtOrderBy.SetTableInfo(NULL);  
 
		m_edtHClause.SetWindowText(_T(""));
   		m_edtHClause.SetTableInfo(NULL, NULL);

	    SendDlgItemMessage(IDC_HCLAUSE_BTN_NATIVE_SQL, BM_SETCHECK, FALSE);

   		m_edtGroupBy.SetWindowText(_T(""));
	   	m_edtGroupBy.SetTableInfo(NULL); 

		m_edtWhenClause.SetWindowText(_T(""));

		SendDlgItemMessage(IDC_BTN_DISTINCT_SQL, BM_SETCHECK, FALSE);
	}
}

//------------------------------------------------------------------------------
void CModTblRuleDlg::InitStatusBar()
{
	UINT	nID, nStyle;
	int		cxWidth = 80;
	
	if (
		!m_sbStatus.Create(this) || 
		!m_sbStatus.SetIndicators(TableIndicators, sizeof(TableIndicators) / sizeof(UINT))
	  )
	{
		TRACE("Failed to create status bar\n");
		return;
	}

	m_sbStatus.GetPaneInfo(0, nID, nStyle, cxWidth);
	m_sbStatus.SetPaneInfo(0, nID, SBPS_NORMAL, cxWidth + 60);
	
	m_sbStatus.GetPaneInfo(1, nID, nStyle, cxWidth);
	m_sbStatus.SetPaneInfo(1, nID, SBPS_NORMAL, cxWidth + 60);
	
	m_sbStatus.GetPaneInfo(2, nID, nStyle, cxWidth);
	m_sbStatus.SetPaneInfo(2, nID, SBPS_STRETCH|SBPS_NORMAL, cxWidth);
	
	
	CString strPhysicalName = m_pTblRule->GetPhysicalNameOf(m_strPublicNameField);
	CString tableName;
	int start = 0;

	if ( !strPhysicalName.IsEmpty() && strPhysicalName.Find(DOT_CHAR) > 0)
	{	
		tableName = strPhysicalName.Tokenize(_T("."), start); 		
		m_sbStatus.SetPaneText(0, AfxLoadDatabaseString(tableName, tableName));
	}
	else
		m_sbStatus.SetPaneText(0, AfxLoadDatabaseString(m_pTblRule->m_arSqlTableJoinInfoArray[0]->GetTableName(), m_pTblRule->m_arSqlTableJoinInfoArray[0]->GetTableName()));
	
	m_sbStatus.SetPaneText(1, NULL);
	m_sbStatus.SetPaneText(2, NULL);
}

//----------------------------------------------------------------------------
void CModTblRuleDlg::ShowStatusBarInfo()
{
    if (m_arSqlTableInfo.GetSize() == 0)  
    	return;
     
	CString strPhysicalName = m_pTblRule->GetPhysicalNameOf(m_strPublicNameField);
	
	const SqlColumnInfo* pSqlColumnInfo = GetColumnInfo(strPhysicalName);
	if (pSqlColumnInfo == NULL)
	{
		if (!m_pTblRule->IsNativeExpr(m_strPublicNameField))
			ASSERT(FALSE);
		return;
	}
	CString	strBuffer(pSqlColumnInfo->m_strTypeName);
    strBuffer += cwsprintf(_T("( %d"), pSqlColumnInfo->GetColumnLength());

	if (pSqlColumnInfo->GetColumnDecimal() > 0)
	    strBuffer += cwsprintf(_T(", %d"), pSqlColumnInfo->GetColumnDecimal());
	
	strBuffer += _T(" )");
	
	m_sbStatus.SetPaneText(1, pSqlColumnInfo->GetColumnTitle());
	m_sbStatus.SetPaneText(2, strBuffer);
}

//----------------------------------------------------------------------------
void CModTblRuleDlg::RecalcLayout()
{
	RepositionBars(0, 0xFFFF, AFX_IDW_PANE_FIRST);
}

//----------------------------------------------------------------------------
BOOL CModTblRuleDlg::LoadDictionary(LPCTSTR pszTableName)
{
	if (!m_pSqlConnection->ExistTable(pszTableName))
	{
        //m_pSqlTableInfo = NULL; TODO
        return FALSE;
    }
	m_arSqlTableInfo.Add(m_pSqlConnection->GetTableInfo(pszTableName));
    return TRUE;
}

//----------------------------------------------------------------------------
void CModTblRuleDlg::SetConstraintMode()
{
	m_pTblRule->m_ConstraintMode = RuleSelectionMode::SEL_ALL;
	switch (m_cbConstrainMode.GetCurSel())
	{
		case 0:	m_pTblRule->m_ConstraintMode   = RuleSelectionMode::SEL_ALL;		break;
		case 1: m_pTblRule->m_ConstraintMode   = RuleSelectionMode::SEL_NOT_NULL;	break;
		case 2: m_pTblRule->m_ConstraintMode   = RuleSelectionMode::SEL_NULL;		break;
	}
}

//----------------------------------------------------------------------------
bool CModTblRuleDlg::OnCheckEdtWhereClause (const CString& strExpression, CString& strError)
{
	WClause wclauseTmp(m_pSqlConnection, m_pSymTable, m_arSqlTableInfo);
    wclauseTmp.SetNative(IsDlgButtonChecked(IDC_WCLAUSE_BTN_NATIVE_SQL));
	CString strOld;
	m_edtWClause.GetWindowText(strOld);
	m_edtWClause.SetWindowText(strExpression);
    bool valid = m_edtWClause.CheckWC(wclauseTmp) == TRUE;
	m_edtWClause.SetWindowText(strOld);
    return valid;
}


//----------------------------------------------------------------------------
bool CModTblRuleDlg::OnCheckEdtHavingClause (const CString& strExpression, CString& strError)
{
	WClause hclauseTmp(m_pSqlConnection, m_pSymTable, m_arSqlTableInfo);
    hclauseTmp.SetNative(IsDlgButtonChecked(IDC_HCLAUSE_BTN_NATIVE_SQL));

	CString strOld;
	m_edtHClause.GetWindowText(strOld);
	m_edtHClause.SetWindowText(strExpression);
    bool valid = m_edtHClause.CheckWC(hclauseTmp) == TRUE;
	m_edtHClause.SetWindowText(strOld);
    return valid;
}
//----------------------------------------------------------------------------
bool CModTblRuleDlg::OnCheckEdtOrderClause (const CString& strExpression, CString& strError)
{
	if (strExpression.IsEmpty())
		return true;
	CString strTemp;
	Parser lex(strExpression);
	return ParseOrderBy(lex, m_pSymTable, &m_arSqlTableInfo, strTemp) == TRUE;
}
//----------------------------------------------------------------------------
void CModTblRuleDlg::DataTypeChanged()
{
    SymField* pRepFieldData = m_pSymTable->GetField(m_strPublicNameField);
	pRepFieldData->SetDataType(m_cbDataObjTypes.GetTypeValue());
}

//----------------------------------------------------------------------------
void CModTblRuleDlg::OnOK()
{
    WClause wclauseTmp(m_pSqlConnection, m_pSymTable, m_arSqlTableInfo);
    wclauseTmp.SetNative(IsDlgButtonChecked(IDC_WCLAUSE_BTN_NATIVE_SQL));

    if (!m_edtWClause.CheckWC(wclauseTmp))
    {
    	m_edtWClause.SetCtrlFocus(TRUE);
        return;
	}

    SendDlgItemMessage(IDC_WCLAUSE_BTN_NATIVE_SQL, BM_SETCHECK, wclauseTmp.IsNative());

    m_pTblRule->AssignWhereClause(wclauseTmp);
	
	//----
    WClause hclauseTmp(m_pSqlConnection, m_pSymTable, m_arSqlTableInfo);
    hclauseTmp.SetNative(IsDlgButtonChecked(IDC_HCLAUSE_BTN_NATIVE_SQL));

    if (!m_edtHClause.CheckWC(hclauseTmp))
    {
    	m_edtHClause.SetCtrlFocus(TRUE);
        return;
	}

    SendDlgItemMessage(IDC_HCLAUSE_BTN_NATIVE_SQL, BM_SETCHECK, hclauseTmp.IsNative());

    m_pTblRule->AssignHavingClause(hclauseTmp);

	//----
	if (!m_edtOrderBy.IsEmpty())
	{
		CString	strTmp;
		m_edtOrderBy.GetWindowText(strTmp);

		Parser lex(strTmp);
		if (!ParseOrderBy(lex, m_pSymTable, &m_arSqlTableInfo, m_pTblRule->m_strOrderBy))
		{
			m_edtOrderBy.SetCtrlFocus(TRUE);
		    return;
		}
	}
	else
		m_pTblRule->m_strOrderBy.Empty();
	
	//----
	if (!m_edtGroupBy.IsEmpty())
	{
		CString	strTmp;
		m_edtGroupBy.GetWindowText(strTmp);

		Parser lex(strTmp);
		if (!ParseOrderBy(lex, m_pSymTable, &m_arSqlTableInfo, m_pTblRule->m_strGroupBy))
		{
			m_edtGroupBy.SetCtrlFocus(TRUE);
			return;
		}
	}
	else
		m_pTblRule->m_strGroupBy.Empty();

	//----
	//TODO save prev expr
 	if (!m_edtWhenClause.CheckExp(*m_pTblRule->m_pWhenExpr, DATA_BOOL_TYPE, TRUE))
	{   
		// reselect edit field and focus it                 
		m_edtWhenClause.SetCtrlFocus(TRUE);
		return;
	}

	//----
	m_pTblRule->SetDistinct(IsDlgButtonChecked(IDC_BTN_DISTINCT_SQL));
	m_pTblRule->m_nTop = m_edtTopRows.GetValue();

    SetConstraintMode   ();

    EndDialog(IDOK);
}

//----------------------------------------------------------------------------
void CModTblRuleDlg::OnCancel()
{
    EndDialog(IDCANCEL);
}

//----------------------------------------------------------------------------
INT_PTR CModTblRuleDlg::OnToolHitTest(CPoint point, TOOLINFO* pTI) const
{
	if (::OnToolHitTestTableName
			(
				this,
				m_stcTableName,
				point, 
				pTI
			)
		)
	{
		return IDC_TBLNAME1;
	}

	if (::OnToolHitTestCriterion
			(
				this,
				m_edtWClause,
				point, 
				pTI,
				m_pSqlConnection
			)
		)
	{
		return IDC_WCLAUSE;
	}

    if (::OnToolHitTestCriterion
			(
				this,
				m_edtGroupBy,
				point, 
				pTI,
				m_pSqlConnection
			)
		)
	{
		return IDC_GROUP_BY;
	}

	if (::OnToolHitTestCriterion
			(
				this,
				m_edtHClause,
				point, 
				pTI,
				m_pSqlConnection
			)
		)
	{
		return IDC_HCLAUSE;
	}

	if (::OnToolHitTestCriterion
			(
				this,
				m_edtOrderBy,
				point, 
				pTI,
				m_pSqlConnection
			)
		)
	{
		return IDC_SORTBY;
	}

	if (::OnToolHitTestCriterion
			(
				this,
				m_edtWhenClause,
				point, 
				pTI,
				m_pSqlConnection
			)
		)
	{
		return IDC_WHEN_CLAUSE;
	}

	return __super::OnToolHitTest(point, pTI);
}




//============================================================================
// CAddCalcColumn
//============================================================================
IMPLEMENT_DYNAMIC(CAddCalcColumn, CParsedDialog)
BEGIN_MESSAGE_MAP(CAddCalcColumn, CParsedDialog)
	//{{AFX_MSG_MAP(CAddCalcColumn)
    
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//----------------------------------------------------------------------------
CAddCalcColumn::CAddCalcColumn(
							   TblRuleData*    pTblRule, 
							   WoormTable*	pSymTable,
							   const PublicFieldsLinks& calculatedColumnsArray, 
							   const PublicFieldsLinks& publicFieldsArray,
	                           CString old_physicalName,
							   CString old_publicName,
			                   DataType old_type,
							   int old_position
							   ) :
    CParsedDialog				(IDD_ADD_CALCULATE_COLUMN),
    m_pTblRule					(pTblRule),
    m_pSymTable     			(pSymTable),
	m_publicFieldsArray			(publicFieldsArray),
	m_calculatedColumnsArray	(calculatedColumnsArray),
	m_old_publicName			(old_publicName),
	m_old_physicalName			(old_physicalName ? old_physicalName : _T("")), 
	m_old_type					(old_type),
	m_old_position				(old_position)
{
}

//----------------------------------------------------------------------------
CAddCalcColumn::~CAddCalcColumn()
{
}

//----------------------------------------------------------------------------
BOOL CAddCalcColumn::OnInitDialog()
{
	CParsedDialog::OnInitDialog();

	m_edtName.			SubclassEdit	(IDC_CALCULATE_COLUMN_NAME,			this);
	m_cbType.			SubclassEdit	(IDC_CALCULATE_COLUMN_TYPE,			this);
	m_edtExpr.			SubclassEdit	(IDC_CALCULATE_COLUMN_EXPR,			this);

	m_cbType.FillListBox();

	if (m_old_publicName.IsEmpty() )
	{ 
		//build field name to advise
		int i = 0;
		CString advisedName = cwsprintf(WoormDialogStrings::FIELD(),i++);
		while (CheckExistsNameWithoutMessageBox(advisedName))
			advisedName = cwsprintf(WoormDialogStrings::FIELD(),i++);
		m_old_publicName = advisedName;
	}
	m_edtName.SetValue (m_old_publicName);
	m_cbType.SetTypeValue (m_old_type);

	if (m_old_physicalName != _T("") )
	   m_edtExpr.SetWindowText (m_old_physicalName);
	else
		m_edtExpr.SetWindowText(_T(""));
	
	if ( m_pTblRule ) 
		m_edtExpr.SetTableInfo(m_pTblRule->m_arSqlTableJoinInfoArray);
	
	return TRUE;  // return TRUE unless you set the focus to a control	
}


void CAddCalcColumn::OnCancel()
{
	EndDialog(IDCANCEL);
}

void CAddCalcColumn::OnOK()
{	
    m_publicName = m_edtName.GetValue();
	m_physicalName = m_edtExpr.GetValue();
    m_type = m_cbType.GetTypeValue();

    if (AfxGetTokensTable()->GetKeywordsToken(m_publicName) != T_NOTOKEN)
	{ 
		m_edtName.SetError(_TB("The name collides with a reserved word of TaskBuilder"));
		return;
	}
 
	//Check no empty expression
	if (m_physicalName.IsEmpty()) 
	{
         AfxMessageBox(_TB("Empty expression not allowed"), MB_OK | MB_ICONINFORMATION);
		 return; 
	}
	if (CheckExistsName(m_publicFieldsArray)) return;           
	if (CheckExistsName(m_calculatedColumnsArray)) return;

   EndDialog(IDOK);
}


BOOL CAddCalcColumn::CheckExistsName(const PublicFieldsLinks& fieldsArray)
{
	if (m_pSymTable && m_pSymTable->ExistField(m_publicName))
	{
		AfxMessageBox(_TB("Public name already defined"), MB_OK | MB_ICONINFORMATION);
		m_edtName.SetError(_TB(" {0-%s}: field already defined"));
		return TRUE;
	}

	for (int i = 0; i < fieldsArray.GetSize(); i++)
		{
			PublicFieldObj* pPFO = fieldsArray[i];
			//check unique public name
			if	(
					pPFO->m_Selection != PublicFieldObj::NONE		&&
					pPFO->m_Selection != PublicFieldObj::BAD_TYPE	&&
					m_publicName.CompareNoCase(pPFO->m_strName) == 0 &&
					i != m_old_position                                 //in edit is possible (re)save the same public name 
				)
			{   
				AfxMessageBox(_TB("Public name already defined"), MB_OK | MB_ICONINFORMATION);
				m_edtName.SetError(_TB(" {0-%s}: field already defined"));
				return TRUE;
			}
           //check unique physical name
			if	(
					pPFO->m_Selection != PublicFieldObj::NONE		&&
					pPFO->m_Selection != PublicFieldObj::BAD_TYPE	&&
					this->m_physicalName.CompareNoCase(pPFO->m_physicalName) == 0 &&
					i != m_old_position                                 //in edit is possible (re)save the same pyhsical name 
				)
			{   
				AfxMessageBox(_TB("Physical name already defined"), MB_OK | MB_ICONINFORMATION);
				m_edtName.SetError(_TB(" {0-%s}: field already defined"));
				return TRUE;
			}
		}
	return FALSE;
}


BOOL CAddCalcColumn::CheckExistsNameWithoutMessageBox(CString advisedName)
{
	if (m_pSymTable && m_pSymTable->ExistField(advisedName))
		return TRUE;


	for (int i = 0; i < m_publicFieldsArray.GetSize(); i++)
		{
			PublicFieldObj* pPFO = m_publicFieldsArray[i];
			if	(
					pPFO->m_Selection != PublicFieldObj::NONE		&&
					pPFO->m_Selection != PublicFieldObj::BAD_TYPE	&&
					advisedName.CompareNoCase(pPFO->m_strName) == 0
				)
			{   
				return TRUE;
			}
		}

   for (int i = 0; i < m_calculatedColumnsArray.GetSize(); i++)
		{
			PublicFieldObj* pPFO = m_calculatedColumnsArray[i];
			if	(
					pPFO->m_Selection != PublicFieldObj::NONE		&&
					pPFO->m_Selection != PublicFieldObj::BAD_TYPE	&&
					advisedName.CompareNoCase(pPFO->m_strName) == 0
				)
			{   
				return TRUE;
			}
		}
	return FALSE;
}


//============================================================================
// CAddJoin
//============================================================================
IMPLEMENT_DYNAMIC(CAddJoin, CParsedDialog)
BEGIN_MESSAGE_MAP(CAddJoin, CParsedDialog)
	//{{AFX_MSG_MAP(CAddJoin)
     ON_CBN_SELCHANGE   (IDC_CBX_GROUPTABLES,				TableGroupChanged)
	 ON_BN_CLICKED		(IDC_BTN_JOIN_ADD,					AddSelection)
	 ON_BN_CLICKED		(IDC_BTN_JOIN_REM,					RemoveSelection)
	 ON_LBN_DBLCLK		(IDC_LIST_TABLESJOIN,				AddSelection)
	 ON_LBN_DBLCLK	    (IDC_LIST_TABLES_JOINED,			RemoveSelection)
  //}}AFX_MSG_MAP
END_MESSAGE_MAP()

//----------------------------------------------------------------------------
CAddJoin::CAddJoin(
					CTblRuleDlg*			pCTblRuleDlg,
					BOOL					oldJoin,
					int						idxRule /*= -1*/
				   ) :
	CParsedDialog			(IDD_ADD_JOIN),
    m_pCTblRuleDlg			(pCTblRuleDlg),
	m_oldJoin				(oldJoin),
	m_pTblRule				(pCTblRuleDlg->m_pTblRule),
	m_idxRule				(idxRule)
{
}

//----------------------------------------------------------------------------
CAddJoin::~CAddJoin() 
{
}

//----------------------------------------------------------------------------
BOOL CAddJoin::OnInitDialog()
{
	CParsedDialog::OnInitDialog();
    
	m_cbTableGroups.		SubclassDlgItem		(IDC_CBX_GROUPTABLES, this);
	m_lbTables.				SubclassDlgItem		(IDC_LIST_TABLESJOIN, this);
	m_lbSelectedTables.		SubclassDlgItem		(IDC_LIST_TABLES_JOINED, this);
	m_addTbl.				SubclassDlgItem		(IDC_BTN_JOIN_ADD, this);
	m_removeTbl.			SubclassDlgItem		(IDC_BTN_JOIN_REM, this);

	ShowTableGroups();

	m_lbSelectedTables.ResetContent();
	
	if (m_pTblRule && m_oldJoin)
	{
		for (int j = 0; j < m_pTblRule->m_arSqlTableJoinInfoArray.GetSize(); j++)
			m_lbSelectedTables.AddString(m_pTblRule->m_arSqlTableJoinInfoArray.GetAt(j)->GetTableName());
	}

	return TRUE;
}

//----------------------------------------------------------------------------
void CAddJoin::OnCancel()
{
	EndDialog(IDCANCEL);
}

//----------------------------------------------------------------------------
void CAddJoin::OnOK()
{	
	CString s;
	m_tablesNewJoin.RemoveAll();
	for (int i = 0; i < m_lbSelectedTables.GetCount(); i++)
	{
		m_lbSelectedTables.GetText(i, s);
		m_tablesNewJoin.Add(s);
	}
	if (!m_oldJoin && m_tablesNewJoin.GetCount() <= 1)
		AfxMessageBox(_TB("Select 2 or more tables"), MB_OK | MB_ICONEXCLAMATION);
				
	else
	{
		if (m_pTblRule && m_oldJoin)
		{
			for (int i = 0; i < m_tablesNewJoin.GetSize(); i++)
			{	
				BOOL oldSelected = FALSE;		
				for (int j = 0; j < m_pTblRule->m_arSqlTableJoinInfoArray.GetSize(); j++)
					if (m_tablesNewJoin.GetAt(i).Compare(m_pTblRule->m_arSqlTableJoinInfoArray.GetAt(j)->GetTableName()) == 0)
					{
						oldSelected = TRUE;
						break;
					}
				
				if (!oldSelected) //adding tableinfo to the table rule
				{
					//load tableInfo
					m_pTblRule->AddTableInfo(m_pCTblRuleDlg->m_pSqlConnection->GetTableInfo(m_tablesNewJoin.GetAt(i)));
				}
			}

			for (int i = 0; i < m_pTblRule->m_arSqlTableJoinInfoArray.GetSize(); i++)
			{	
				BOOL found = FALSE;		
				for (int j = 0; j < m_tablesNewJoin.GetSize(); j++)	
					if (m_tablesNewJoin.GetAt(j).Compare(m_pTblRule->m_arSqlTableJoinInfoArray.GetAt(i)->GetTableName()) == 0)
					{
						found = TRUE;
						break;
					}
				
				if (!found) //removing tableinfo from table rule
				{
					m_pTblRule->m_arSqlTableJoinInfoArray.RemoveAt(i);
					i--;

					if (m_pTblRule->m_arSqlTableJoinInfoArray.GetSize() == 0)
					{   //Se non ci sono piu' tabelle, il join (la rule) deve essere cancellato
						ASSERT (m_idxRule >=0);
						if (m_idxRule >= 0)
							m_pCTblRuleDlg->m_pRuleData->RemoveAt(m_idxRule);
					}
				}
			}
		}
		else
		{
			m_pTblRule = new TblRuleData(*(m_pCTblRuleDlg->m_pSymTable), m_pCTblRuleDlg->m_pSqlConnection, CString("")); //metto stringa vuota e non nome tabella, perche ce ne sono piu di una nel join
			for (int i = 0; i < m_tablesNewJoin.GetSize(); i++)
			{	
				m_pTblRule->AddTableInfo(m_pCTblRuleDlg->m_pSqlConnection->GetTableInfo(m_tablesNewJoin.GetAt(i)));
			}	
			m_pTblRule->SetTemporaryJoin(TRUE);
			m_pCTblRuleDlg->m_pRuleData->Add(m_pTblRule);
			m_pCTblRuleDlg->m_pRuleData->m_pLastTblRule = m_pTblRule;
		}
		EndDialog(IDOK);
	}
}

//----------------------------------------------------------------------------
void CAddJoin::ShowTableGroups()
{
	CString strCurrGroup = Strings::ALL_TABLES();
	m_cbTableGroups.ResetContent();
	int nGrpIdx = m_cbTableGroups.AddString(Strings::ALL_TABLES());
    // i gruppi vengono descritti dagli Xml di modulo
	AddOnApplication*	pAddOnApp;
	AddOnModule*		pAddOnMod;

	for (int i=0; i <= AfxGetAddOnAppsTable()->GetUpperBound(); i++)
	{
		pAddOnApp = AfxGetAddOnAppsTable()->GetAt(i);
		if (!pAddOnApp || !pAddOnApp->m_pAddOnModules)
			continue;

		for (int n=0; n <= pAddOnApp->m_pAddOnModules->GetUpperBound(); n++)
		{
			pAddOnMod = pAddOnApp->m_pAddOnModules->GetAt(n);
			
			// se non ci sono oggetti di db presenti non visualizzo il modulo
			if (AfxGetDatabaseObjectsTable()->ModuleHasObjects(pAddOnMod->m_Namespace))
				m_cbTableGroups.AddString(pAddOnMod->GetModuleTitle());
		}
	}	
    m_cbTableGroups.SetCurSel(nGrpIdx);
	TableGroupChanged();
}

//----------------------------------------------------------------------------
void CAddJoin::TableGroupChanged()
{
	AfxGetApp()->BeginWaitCursor ();

	CString strCurrGroup = Strings::ALL_TABLES();
	
	int curIdxGroup = m_cbTableGroups.GetCurSel();
	if (curIdxGroup > 0)
		 m_cbTableGroups.GetLBText(curIdxGroup, strCurrGroup);

	m_lbTables.SetRedraw(FALSE);
	m_lbTables.ResetContent();
	m_TableNames.RemoveAll();

	POSITION pos;
	CString key;
	SqlCatalogEntry* pCatalogEntry;
	AddOnModule* pAddOnMod = NULL;
	
	// cerco quale modulo è stato selezionato
	if (!AfxGetCultureInfo()->IsEqual(strCurrGroup, Strings::ALL_TABLES()))
		for (int a=0; a <= AfxGetAddOnAppsTable()->GetUpperBound(); a++)
		{
			AddOnApplication* pAddOnApp = AfxGetAddOnAppsTable()->GetAt(a);
			if (!pAddOnApp || !pAddOnApp->m_pAddOnModules)
				continue;

			for (int m=0; m <= pAddOnApp->m_pAddOnModules->GetUpperBound(); m++)
			{
				pAddOnMod =  pAddOnApp->m_pAddOnModules->GetAt(m);
				if	(pAddOnMod && AfxGetCultureInfo()->IsEqual(strCurrGroup, pAddOnMod->GetModuleTitle()))
					break;

				pAddOnMod = NULL;
			}
			if (pAddOnMod)
				break;
		}
	{//lock scope
		SqlCatalogConstPtr pCatalog = m_pCTblRuleDlg->m_pSqlConnection->GetCatalog();
		// ora ciclo sulle tabelle per definire se appartengono al modulo selezionato
		for (pos = pCatalog->GetStartPosition(); pos != NULL;)
		{
			pCatalog->GetNextAssoc(pos, key, (CObject*&) pCatalogEntry);

			if (pCatalogEntry->m_nType != TABLE_TYPE && pCatalogEntry->m_nType != VIEW_TYPE)
				continue;

			// tutti i gruppi oppure la tabella appartiene al modulo corrente
			BOOL bInsert = strCurrGroup == Strings::ALL_TABLES();
			if (!bInsert && pAddOnMod)
			{
				const CDbObjectDescription* pDbDescr = AfxGetDbObjectDescription(pCatalogEntry->m_strTableName);
				if (pDbDescr)
				{
					CTBNamespace ns(CTBNamespace::MODULE, 
						pDbDescr->GetNamespace().GetApplicationName()
						+ CTBNamespace::GetSeparator () +
						pDbDescr->GetNamespace().GetModuleName()
						);

					bInsert = ns == pAddOnMod->m_Namespace;
				}
			}
			if	(bInsert)
				m_TableNames.InsertAt
					(
						m_lbTables.AddString(pCatalogEntry->m_strTableName),
						pCatalogEntry->m_strTableName
					);
		}
	}
	m_lbTables.SetCurSel(0);
	AfxGetApp()->EndWaitCursor ();

	m_lbTables.SetRedraw(TRUE);
	m_lbTables.Invalidate(FALSE);
}

//----------------------------------------------------------------------------
void CAddJoin::AddSelection()
{
	int nCount = m_lbTables.GetSelCount();

	CArray<int,int> arSelections;
	arSelections.SetSize(nCount);
	m_lbTables.GetSelItems(nCount, arSelections.GetData());

	CStringArray arSelected;
	CString s;
	for (int i=0; i <= arSelections.GetUpperBound(); i++)
	{
		m_lbTables.GetText(arSelections.GetAt(i), s);
		arSelected.Add(s);
	}

	CheckSelectionsToAdd (arSelected); //delete table already selected

	for (int i=0; i <= arSelected.GetUpperBound(); i++)
		m_lbSelectedTables.AddString(arSelected.GetAt(i));
}

//----------------------------------------------------------------------------
void CAddJoin::CheckSelectionsToAdd (CStringArray& arSelections)
{
	// elimino le tabelle che sono già presenti nelle selezionate
	CString sSel;
	for (int i = arSelections.GetUpperBound(); i >= 0; i--)
	{
		sSel = arSelections.GetAt(i);
		if (m_lbSelectedTables.FindStringExact(0, sSel) >= 0)
			arSelections.RemoveAt(i);
	}
}

//----------------------------------------------------------------------------
void CAddJoin::RemoveSelection()
{
	BOOL deletableTable = TRUE;
	int nCount = m_lbSelectedTables.GetSelCount();
	CArray<int,int> arSelections;
	arSelections.SetSize(nCount);
	m_lbSelectedTables.GetSelItems(nCount, arSelections.GetData());
	int idxDeletingTable = 0;

	for (int i = arSelections.GetUpperBound(); i >= 0; i--)
	{	
        CString tableToDelete;
		m_lbSelectedTables.GetText(arSelections.GetAt(i),tableToDelete);
		
		if (m_pTblRule)
		{
			for (int j = 0; j < m_pTblRule->m_arSqlTableJoinInfoArray.GetSize(); j++)
			{
				if (m_pTblRule->m_arSqlTableJoinInfoArray.GetAt(j)->GetTableName().CompareNoCase(tableToDelete) == 0)
				{
					if (!IsDeletableTable(m_pTblRule->m_arSqlTableJoinInfoArray.GetAt(j)))
					{
						deletableTable = FALSE;
						AfxMessageBox(_TB("Cannot Delete a table with selected field or calculated columns: " + tableToDelete), MB_OK | MB_ICONEXCLAMATION);
						break;
					}
				}
			}
		}
		if (deletableTable)
		{	
			m_lbSelectedTables.DeleteString(arSelections.GetAt(i));
		}
	}
}

//----------------------------------------------------------------------------
BOOL CAddJoin::IsDeletableTable (const SqlTableInfo* pSqlTableInfo)
{
	if (m_pTblRule->m_CalcColumnLinks.GetSize() > 0)
		return FALSE;   //se c'e' funzione di gruppo non permetto cancellazione, e' condivisa la colonna calcolata
	
	// non deve far vedere i campi virtuali definiti dal programmatore
	// ed inseriti nel TableInfo dalla prima istanza di record derivato 
	// che definisce eventuali campi virtuali
	const  Array* pColums = pSqlTableInfo->GetPhysicalColumns();
	for (int i = 0; i < pColums->GetSize(); i++)
	{
		SqlColumnInfo* pSqlColumnInfo = (SqlColumnInfo*) pColums->GetAt(i);
		ASSERT (pSqlColumnInfo);
		ASSERT(!pSqlColumnInfo->m_bVirtual);

		CString strPhysicalName = pSqlColumnInfo->GetColumnName();
     	if (m_pTblRule)
		{
			CString strPublicName = CString("");
			if (m_pTblRule->m_arSqlTableJoinInfoArray.m_arFieldLinks.GetSize() == 1) //cerco con sintassi non qualificata
				strPublicName = m_pTblRule->GetPublicNameOf(strPhysicalName);

			// provo a cercarla anche con la sintassi qualificata
			if (strPublicName.IsEmpty())
			{
				CString sQualifiedName (CString(pSqlTableInfo->GetTableName()) + DOT_CHAR + strPhysicalName);
				strPublicName = m_pTblRule->GetPublicNameOf(sQualifiedName);
			}
			if (!strPublicName.IsEmpty())
				return FALSE;			//e' stato trovato un campo selezionato di questa tabella
		}
	}
  return TRUE;
}
