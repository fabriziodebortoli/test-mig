
#include "stdafx.h"

#include <TbFrameworkImages\GeneralFunctions.h>

#include <TbClientCore\ClientObjects.h>
#include <TbWebServicesWrappers\LoginManagerInterface.h>

#include <TbGenlib\baseapp.h>
#include <TbGenlib\TBCaptionBar.h>
#include <TbFrameworkImages\CommonImages.h>

#include <TbWoormEngine\edtcmm.h>
#include <TbWoormEngine\edtmng.h>
#include <TbWoormEngine\eqnedit.h>
#include <TbWoormEngine\rpsymtbl.h>

#include <TbOledb\sqlcatalog.h>
#include <TbOledb\sqltable.h>	
#include <TbOledb\sqlrec.h>
#include <TbOledb\wclause.h>
#include <TbOleDb\oledbmng.h>

#include <TbGes\extdoc.h>
#include <TbGes\tabber.h>
#include <TbGes\TileManager.h>
#include <TbGes\HeaderStrip.h>
#include <TbGes\formmng.h>
#include <TbGes\ExtDocFrame.h>
#include <TbGes\dbt.h>
#include <TbGes\bodyedit.h>
#include <TbGenlib\TBPropertyGrid.h>
#include <TbGes\hotlink.h>
#include <TbGes\ParsedPanel.h>
#include <TbGes\SlaveViewContainer.h>

#include "OslDlg.hjson" //JSON AUTOMATIC UPDATE
#include "oslsecurityinterface.h"
#include "osldlg.h"

#include <TbGes\FieldInspector.hjson> //JSON AUTOMATIC UPDATE
#include <XEngine\TBXMLTransfer\XMLDataMng.hjson>
#include <EasyBuilder\TbEasyBuilder\TBeasyBuilder.h>
#include <EasyBuilder\TbEasyBuilder\CDEasyBuilder.hjson> //JSON AUTOMATIC UPDATE

#ifdef _DEBUG
#undef THIS_FILE
static const char BASED_CODE THIS_FILE[] = __FILE__;

#define new DEBUG_NEW
#endif


//=============================================================================

//SONO INDICI IN UNA ImageList basata su di una unica bitmap
#define OSLDLG_BMP_DOC				7
#define OSLDLG_BMP_FINDERDOC		14

#define OSLDLG_BMP_MASTERVIEW		0
#define OSLDLG_BMP_BATCHVW			8
#define OSLDLG_BMP_SLAVEVW			5
#define OSLDLG_BMP_ROWSLAVEVW		4
#define OSLDLG_BMP_EMBEDDEDSLAVEVW	9

#define OSLDLG_BMP_TABMNGR			6
#define OSLDLG_BMP_TABDLG			1

#define OSLDLG_BMP_BODYEDIT			2
#define OSLDLG_BMP_BODYEDITCOL		3

#define OSLDLG_BMP_CONTROL			10
#define OSLDLG_BMP_TOOLBAR			11
#define OSLDLG_BMP_TILEMANAGER		12
#define OSLDLG_BMP_TILE				13

#define OSLDLG_BMP_TABDLG_EMPTY		15
#define OSLDLG_BMP_TABDLG_DUPLICATE		16

#define OSLDLG_BMP_TOOLBARBUTTON		17

///////////////////////////////////////////////////////////////////////////////
class TB_EXPORT CObjectOSL : public CObject
{
	DECLARE_DYNAMIC(CObjectOSL);
public:
	CInfoOSL m_InfoOSL;

public:
	CObjectOSL() : m_InfoOSL(OSLType_Tabber) {}
	virtual ~CObjectOSL () {}
};

IMPLEMENT_DYNAMIC (CObjectOSL,CObject);

//===========================================================================

int OslTypeToBmp(OSLTypeObject eType)
{
	int nBmp;

	switch (eType)
	{
	case OSLType_Template:
		nBmp = OSLDLG_BMP_DOC;
		break;
	case OSLType_BatchTemplate:
		nBmp = OSLDLG_BMP_BATCHVW;
		break;
	case OSLType_FinderDoc:
		nBmp = OSLDLG_BMP_FINDERDOC;
		break;
	default :
		ASSERT(FALSE);
		nBmp = OSLDLG_BMP_DOC;
	}
	return nBmp;
}

/////////////////////////////////////////////////////////////////////////////
// serve solo per inserire un oggetto CInfoOSL in un Array per essere deallocato
// alla chiusura del form manager

class CInfoOSLObject: public CObject, public CInfoOSL
{
public:
	CInfoOSLObject() {}
};

//===========================================================================

/////////////////////////////////////////////////////////////////////////////
// COslDlg dialog
/////////////////////////////////////////////////////////////////////////////

COslDlg::COslDlg (CLocalizablePropertySheet* pSheet, UINT idd)
	:	 
	CLocalizablePropertyPage (idd),
	m_hCurrentItem		(NULL),
	m_lParamCurrentItem	(LPARAM(0L)),
	m_bFilling			(TRUE),
	m_pSheet			(pSheet)
{
}
/////////////////////////////////////////////////////////////////////////////

IMPLEMENT_DYNAMIC (COslDlg, CLocalizablePropertyPage)
/////////////////////////////////////////////////////////////////////////////

BEGIN_MESSAGE_MAP (COslDlg, CLocalizablePropertyPage)

	ON_NOTIFY (TVN_SELCHANGED, IDC_DLGOSL_TREE, OnSelchangedTree)
	ON_NOTIFY (TVN_SELCHANGING, IDC_DLGOSL_TREE, OnSelchangingTree)

	ON_BN_CLICKED (IDC_DLGOSL_SAVE, OnClickInsert)
	ON_BN_CLICKED (IDC_DLGOSL_INSERTSUBTREE, OnClickRecursiveInsert)

END_MESSAGE_MAP ()
/////////////////////////////////////////////////////////////////////////////

BOOL COslDlg::OnInitDialog () 
{
	CBitmap bm;

	CLocalizablePropertyPage::OnInitDialog();
	m_ctrlGuid.SubclassDlgItem(IDC_DLGOSL_NAMESPACE,	this);
	m_ctrlTree.SubclassDlgItem	(IDC_DLGOSL_TREE, this);

	// use 32 Color image 
	m_imaSmall.Create(20, 20, ILC_COLOR32, 0, 20);
	
	HICON hIcon;
	// 0 OSLDLG_BMP_MASTERVIEW
	hIcon = TBLoadPng(TBIcon(szIconSecurityMasterView, IconSize::CONTROL));
	m_imaSmall.Add(hIcon);
	// 1 OSLDLG_BMP_TABDLG
	hIcon = TBLoadPng(TBIcon(szIconSecurityTabDlg, IconSize::CONTROL));
	m_imaSmall.Add(hIcon);
	// 2 OSLDLG_BMP_BODYEDIT
	hIcon = TBLoadPng(TBGlyph(szGlyphTable));
	m_imaSmall.Add(hIcon);
	// 3 OSLDLG_BMP_BODYEDITCOL
	hIcon = TBLoadPng(TBGlyph(szGlyphColumn));
	m_imaSmall.Add(hIcon);
	// 4 OSLDLG_BMP_ROWSLAVEVW
	hIcon = TBLoadPng(TBIcon(szIconSecurityRowSlaveView, IconSize::CONTROL));
	m_imaSmall.Add(hIcon);
	// 5 OSLDLG_BMP_SLAVEVW
	hIcon = TBLoadPng(TBIcon(szIconSecuritySlaveView, IconSize::CONTROL));
	m_imaSmall.Add(hIcon);
	// 6 OSLDLG_BMP_TABMNGR
	hIcon = TBLoadPng(TBIcon(szIconSecurityTabMngr, IconSize::CONTROL));
	m_imaSmall.Add(hIcon);
	// 7 OSLDLG_BMP_DOC				
	hIcon = TBLoadPng(TBIcon(szIconDocument, IconSize::CONTROL));
	m_imaSmall.Add(hIcon);
	// 8 OSLDLG_BMP_BATCHVW			
	hIcon = TBLoadPng(TBIcon(szIconBatch, IconSize::CONTROL));
	m_imaSmall.Add(hIcon);
	// 9 OSLDLG_BMP_EMBEDDEDSLAVEVW		
	hIcon = TBLoadPng(TBIcon(szIconSecurityEmbeddedSlaveView, IconSize::CONTROL));
	m_imaSmall.Add(hIcon);
	// 10 OSLDLG_BMP_CONTROL			
	hIcon = TBLoadPng(TBIcon(szIconSecurityControl, IconSize::CONTROL));
	m_imaSmall.Add(hIcon);
	// 11 OSLDLG_BMP_TOOLBAR			
	hIcon = TBLoadPng(TBIcon(szIconSecurityToolbar, IconSize::CONTROL));
	m_imaSmall.Add(hIcon);
	// 12 OSLDLG_BMP_TILEMANAGER							
	hIcon = TBLoadPng(TBIcon(szIconSecurityTileManager, IconSize::CONTROL));
	m_imaSmall.Add(hIcon);
	// 13 OSLDLG_BMP_TILE										
	hIcon = TBLoadPng(TBIcon(szIconSecurityTile, IconSize::CONTROL));
	m_imaSmall.Add(hIcon);
	// 14 OSLDLG_BMP_FINDERDOC					
	hIcon = TBLoadPng(TBIcon(szIconSecurityFinderDoc, IconSize::CONTROL));
	m_imaSmall.Add(hIcon);
	// 15 OSLDLG_BMP_TABDLG_EMPTY						
	hIcon = TBLoadPng(TBIcon(szIconSecurityTabDlgEmpty, IconSize::CONTROL));
	m_imaSmall.Add(hIcon);
	// 16 OSLDLG_BMP_TABDLG_DUPLICATE					
	hIcon = TBLoadPng(TBIcon(szIconSecurityTabDlgDuplicate, IconSize::CONTROL));
	m_imaSmall.Add(hIcon);
	// 17 OSLDLG_BMP_TOOLBARBUTTON							
	hIcon = TBLoadPng(TBIcon(szIconSecurityToolbarButton, IconSize::CONTROL));
	m_imaSmall.Add(hIcon);

	m_ctrlTree.SetImageList(&m_imaSmall, TVSIL_NORMAL);

	return TRUE;
}

//-----------------------------------------------------------------------

CString ShowParent(CInfoOSL* pInfo)
{
	CString s;
	for (;pInfo; pInfo = pInfo->m_pParent)
	{
		s += '[' + pInfo->FormatType() + L"]: " + pInfo->m_Namespace.ToString() + L"; ";
	}
	return s;
}

//-----------------------------------------------------------------------
void COslDlg::OnSelchangedTree(NMHDR* pNMHDR, LRESULT* pResult) 
{
	m_ctrlGuid.SetWindowText(L""); GetDlgItem(IDC_DLGOSL_NSPARENT)->SetWindowText(L"");

	*pResult = 0;
	if (m_bFilling) return;

	NMTREEVIEW* pNMTreeView = (NMTREEVIEW*)pNMHDR;
	
	// occorre mantenere info dell'elemento del tree correntemente attivo
	m_hCurrentItem = pNMTreeView->itemNew.hItem;
	m_lParamCurrentItem = pNMTreeView->itemNew.lParam; //data item

	if (m_lParamCurrentItem != 0)
	{
		COslTreeItem* pItem = (COslTreeItem*) (m_lParamCurrentItem);
		ASSERT_KINDOF(COslTreeItem, pItem);

		if (pItem->m_pInfo)
		{
			CString str = pItem->m_pInfo->m_Namespace.ToString();
#ifdef _DEBUG
			str = '['+ pItem->m_pInfo->FormatType() + L"]: " + str ;

			GetDlgItem(IDC_DLGOSL_NSPARENT)->SetWindowText(ShowParent(pItem->m_pInfo->m_pParent));
#endif
			m_ctrlGuid.SetWindowText(str);
		}
	}
}

/// il killfocus arriva dopo la selezione del tree
//-----------------------------------------------------------------------
void COslDlg::OnSelchangingTree(NMHDR* pNMHDR, LRESULT* pResult) 
{
	// autorizzo la prosecuzione della selezione
	*pResult = 0;
	if(m_bFilling)return;

	// all'inizio non ho nessun corrente selezionato
	if (m_hCurrentItem == NULL)
		return;
}

//---------------------------------------------------------------------------
void COslDlg::DoClickInsert(BOOL bRecursive)
{
	int nInserted = 0;

	if(m_hCurrentItem != NULL)
	{
		HTREEITEM pItem = (HTREEITEM)m_hCurrentItem;
		DoInsert(pItem, bRecursive, nInserted, TRUE);
	}
}

//---------------------------------------------------------------------------
void COslDlg::OnClickInsert()
{
	DoClickInsert(FALSE);
}

//---------------------------------------------------------------------------
void COslDlg::OnClickRecursiveInsert ()
{
	DoClickInsert(TRUE);
}

//---------------------------------------------------------------------------
BOOL COslDlg::DoInsert (CInfoOSL* pInfoOSL, int& nInserted, BOOL bCheckParent, CString sNickName)
{
		if 
			(
					pInfoOSL				
				&&
					pInfoOSL->GetType() != OSLType_AddOnApp
				&&
					pInfoOSL->GetType() != OSLType_AddOnMod
			)
		{
			long nFlags = 0;	//OLD OSL_FLAG_INSERT;		nFlags = 1;		//OSL_FLAG_PROTECT;

			if (bCheckParent && !DoInsert(pInfoOSL->m_pParent, nInserted, true, _T("")))
				return FALSE;

			if (pInfoOSL->GetType() != OSLType_Skip)
			{
				if (((CSecurityInterface*)AfxGetSecurityInterface())->InsertObjectIntoOSL (pInfoOSL, &nFlags, sNickName) )
				{
					nInserted++;

				}
				else	//TODO CMessage 
				{
					//fallisce se esiste già non è errore
					//	return FALSE;
				}
			}
		}
		return TRUE;
}

//---------------------------------------------------------------------------
BOOL COslDlg::DoInsert (COslTreeItem* pItemInfo, BOOL bRecursive, int& nInserted, BOOL bCheckParent)
{
	if (pItemInfo)
	{
		ASSERT_KINDOF(COslTreeItem, pItemInfo);
		CInfoOSL* pInfoOSL = pItemInfo->m_pInfo;

		if (bCheckParent && pInfoOSL->m_pParent && pItemInfo->m_pParent)
		{
			if (!DoInsert(pItemInfo->m_pParent, FALSE, nInserted, bCheckParent))
				return FALSE;
			bCheckParent = FALSE;
		}

		if (pInfoOSL && !DoInsert(pInfoOSL, nInserted, bCheckParent, pItemInfo->m_sNickName))
			return FALSE;
	}

	return TRUE;
}

//---------------------------------------------------------------------------
BOOL COslDlg::DoInsert (HTREEITEM hSubRootItem, BOOL bRecursive, int& nInserted, BOOL bCheckParent)
{
	if (hSubRootItem == NULL) 
		return TRUE;
	COslTreeItem* pItemInfo = (COslTreeItem*) m_ctrlTree.GetItemData(hSubRootItem); 

	if (!DoInsert (pItemInfo, bRecursive, nInserted, bCheckParent))
	{
		return FALSE;
	}

	//recursive on children...
	if (bRecursive && m_ctrlTree.ItemHasChildren(hSubRootItem))
	{
		for 
			(
				HTREEITEM hChildItem = m_ctrlTree.GetChildItem(hSubRootItem);
				hChildItem != NULL;
				hChildItem = m_ctrlTree.GetNextItem(hChildItem, TVGN_NEXT)
			)
	   {
			BOOL bRet = DoInsert(hChildItem, bRecursive, nInserted, FALSE);
			//if (!bRet) return FALSE;
	   }
	}
	return TRUE;
}

/////////////////////////////////////////////////////////////////////////////
// COslDlgDoc dialog
/////////////////////////////////////////////////////////////////////////////
COslDlgDoc::COslDlgDoc ( CLocalizablePropertySheet* pSheet, CAbstractFormDoc* pDoc)
	:	 
	COslDlg		(pSheet, IDD_DLGOSL_FORMOBJECTS),
	m_bFirstMaster (FALSE)
{
	ASSERT_VALID(pDoc);
	m_pDoc = pDoc;
}

/////////////////////////////////////////////////////////////////////////////

IMPLEMENT_DYNAMIC (COslDlgDoc, COslDlg)

//----------------------------------------------------------------------------
BOOL COslDlgDoc::OnInitDialog () 
{
	COslDlg::OnInitDialog();
	
	FillAllTree();

	return FALSE;
}

//----------------------------------------------------------------------------
void COslDlgDoc::EnumControls (COslTreeItem* pParent, HTREEITEM htmLevParent, ControlLinks* pControlLinks) 
{
	//if (!AfxGetBaseApp()->IsDevelopment()) return;
	if (pControlLinks == NULL) 
		return;
	int nBmp = OSLDLG_BMP_CONTROL;
	for (int i = 0; i <= pControlLinks->GetUpperBound(); i++)
	{
		CWnd* pWnd = pControlLinks->GetAt(i);
		if (pWnd->IsKindOf(RUNTIME_CLASS(CTreeViewAdvCtrl)))
			continue;
		if (pWnd->IsKindOf(RUNTIME_CLASS(CGanttCtrl)))
			continue;
		if (pWnd->IsKindOf(RUNTIME_CLASS(CLabelStatic)))
			continue;
		if (pWnd->IsKindOf(RUNTIME_CLASS(CBodyEdit)))
		{
			EnumBodyEdit(pParent, htmLevParent, (CBodyEdit*)pWnd);
			continue;
		}

		if (pWnd->IsKindOf(RUNTIME_CLASS(CTBPropertyGrid)))
		{
			CTBPropertyGrid* prop = (CTBPropertyGrid*)pWnd;

			IOSLObjectManager* pOSLManager = dynamic_cast<IOSLObjectManager*>(prop);
			CInfoOSL* pInfoOSL = pOSLManager ? pOSLManager->GetInfoOSL() : NULL;

			// il titolo può derivare da fonti diverse
			CString sProp = prop->GetName();
			if (sProp.IsEmpty())
				sProp = pInfoOSL->m_Namespace.GetObjectName();

			HTREEITEM htmLevBody = m_ctrlTree.InsertItem(sProp, OSLDLG_BMP_BODYEDIT, OSLDLG_BMP_BODYEDIT, htmLevParent);
			COslTreeItem *pBodyItemInfo = new COslTreeItem(pParent, pInfoOSL, sProp);
			m_arInfoTreeItems.Add(pBodyItemInfo);
			m_ctrlTree.SetItemData(htmLevBody, (DWORD)pBodyItemInfo);
			continue;
		}

		if (pWnd->IsKindOf(RUNTIME_CLASS(CParsedPanel)))
		{
			EnumControls( pParent, htmLevParent, ((CParsedPanel*) pWnd)->GetControlLinks());
		}

		CParsedCtrl* pControl = GetParsedCtrl(pWnd);
		if (pControl == NULL ||!pControl->GetCtrlCWnd()->IsWindowVisible())
			continue;
		//--------------------
		CInfoOSL* osl = pControl->GetInfoOSL();
		CString strTitle = pControl->GetInfoOSL()->m_Namespace.GetObjectName();
		if (pControl->GetCtrlCWnd()->IsKindOf(RUNTIME_CLASS(CBoolButton)))
		{
			pControl->GetCtrlCWnd()->GetWindowText(strTitle);
			if (strTitle.IsEmpty())
			{
				strTitle = (pControl->GetNamespace()).GetObjectName(); 
			}
		}
		else
		{
			int idx = strTitle.Find('_');
			if (idx >=0)
			{
				idx = strTitle.Find('_', idx + 1);
				if (idx >=0)
				{
					CString sTable = strTitle.Left(idx);
					SqlRecord* pRec = AfxCreateRecord(sTable);
					if (pRec)
					{
						CString sColumn = strTitle.Mid(idx + 1);
						if (pRec->GetIndexFromColumnName(sColumn) >= 0)
						{
							CString sTableLoc = AfxLoadDatabaseString(sTable, sTable);
							if (!sTableLoc.IsEmpty())
							{
								CString sColumnLoc = AfxLoadDatabaseString(sColumn, sTable);
								if (!sColumnLoc.IsEmpty() && sTableLoc.CompareNoCase(sTable) && sColumnLoc.CompareNoCase(sColumn))
								{
									strTitle += cwsprintf(_T(" (%s.%s)"), sTableLoc, sColumnLoc);
								}
							}
						}

						delete pRec;
					}
				}
			}
		}

		if (strTitle.IsEmpty())
		{
			int k = 0;
		}

		HTREEITEM htmLevControl = m_ctrlTree.InsertItem (strTitle, nBmp, nBmp, htmLevParent);
			COslTreeItem* pItemInfo = new COslTreeItem (pParent, pControl->GetInfoOSL(), strTitle);
			m_arInfoTreeItems.Add(pItemInfo);
			m_ctrlTree.SetItemData (htmLevControl, (DWORD)pItemInfo); 
	}
}

//----------------------------------------------------------------------------
void COslDlgDoc::EnumCaptionBarObjects(COslTreeItem* pParent, HTREEITEM htmLevDoc)
{
	CTaskBuilderCaptionBar* pBar = m_pDoc->GetMasterFrame()->GetCaptionBar();
	
	if (!pBar) 
		return;

	HTREEITEM htmLevParent = NULL;
	for (int i = 0; i < pBar->GetOSLInfos().GetSize(); i++)
	{
		CInfoOSL* pInfo = pBar->GetOSLInfos().GetAt(i);
		if (!pInfo->m_Namespace.IsValid()) 
			continue;

		COslTreeItem* pTreeItemInfo = new COslTreeItem (pParent, pInfo, pInfo->m_Namespace.GetObjectName());
		m_arInfoTreeItems.Add(pTreeItemInfo);

		if (i == 0)
		{
			htmLevParent = m_ctrlTree.InsertItem (_TB("Caption Bar"), OSLDLG_BMP_TOOLBAR, OSLDLG_BMP_TOOLBAR, htmLevDoc );
			pParent = pTreeItemInfo;
			m_ctrlTree.SetItemData(htmLevParent, (DWORD) pTreeItemInfo); 
		}
		else
		{
			HTREEITEM htmLevItem = m_ctrlTree.InsertItem (pInfo->m_Namespace.GetObjectName(), OSLDLG_BMP_CONTROL, OSLDLG_BMP_CONTROL, htmLevParent );
			m_ctrlTree.SetItemData(htmLevItem, (DWORD) pTreeItemInfo); 
		}
	}
}

//----------------------------------------------------------------------------
BOOL IsCommandFramework(UINT nID)
{
	return 
		nID == ID_EXTDOC_NEW ||
		nID == ID_EXTDOC_EDIT ||
		nID == ID_EXTDOC_SAVE ||
		nID == ID_EXTDOC_REPORT ||
		nID == ID_EXTDOC_RADAR ||
		nID == ID_EXTDOC_CUSTOMIZE ||

		nID == ID_EXTDOC_FIND ||
		nID == ID_EXTDOC_QUERY ||
		nID == ID_EXTDOC_FIRST ||
		nID == ID_EXTDOC_PREV ||
		nID == ID_EXTDOC_NEXT ||
		nID == ID_EXTDOC_LAST ||
		
		nID == ID_EXTDOC_REFRESH_ROWSET ||
		nID == ID_EXTDOC_EXEC_QUERY ||
		nID == ID_EXTDOC_DELETE ||
		nID == ID_EXTDOC_ESCAPE ||
		nID == ID_EXTDOC_EXIT ||
		nID == ID_EXTDOC_HELP ||

		nID == ID_EXTDOC_BATCH_STARTSTOP_TOOLBTN ||
		nID == ID_EXTDOC_BATCH_PAUSERESUME_TOOLBTN ||
		
		nID == ID_EXTDOC_GOTO_MASTER ||

		nID == ID_EXTDOC_INSERT_ROW ||
		nID == ID_EXTDOC_FIRST_ROW ||
		nID == ID_EXTDOC_PREV_ROW ||
		nID == ID_EXTDOC_NEXT_ROW ||
		nID == ID_EXTDOC_LAST_ROW ||
		nID == ID_EXTDOC_DELETE_ROW ||

		nID == IDC_WIZARD_NEXT ||
		nID == IDC_WIZARD_BACK ||
		nID == ID_WIZARD_RESTART ||

		nID == ID_EXTDOC_IMPORT_XML_DATA ||
		nID == ID_EXTDOC_EXPORT_XML_DATA ||
		nID == ID_INSPECT ||

		nID == ID_EXDOC_TAB_SWITCH ||
		nID == ID_STATUS_BAR_SWITCH ||
		nID == ID_EXTDOC_SWITCHTO ||
		(nID >= ID_FORM_EDITOR_EDIT && nID < ID_FORM_EDITOR_EDIT + MAX_EB_COMMANDS)
		;
}

//----------------------------------------------------------------------------
void COslDlgDoc::EnumTabbedToolbarElements(COslTreeItem* pParent, HTREEITEM htmLevDoc)
{
	CString  strTitle;
	
	CTBTabbedToolbar*	pTabbedToolBar = m_pDoc->GetMasterFrame()->GetTabbedToolBar();
	if (!pTabbedToolBar)
		return;

	HTREEITEM htreeRibbon = m_ctrlTree.InsertItem (_TB("Tabbed toolbar"), OSLDLG_BMP_TOOLBAR, OSLDLG_BMP_TOOLBAR, htmLevDoc );
	COslTreeItem* pRibbonBarItemInfo = new COslTreeItem (pParent,  pTabbedToolBar->GetInfoOSL(), _T("TabbedToolbar"));
	m_arInfoTreeItems.Add(pRibbonBarItemInfo);
	m_ctrlTree.SetItemData(htreeRibbon, (DWORD)pRibbonBarItemInfo); 

	CBCGPTabWnd*	m_pTabsWnd = pTabbedToolBar->GetUnderlinedWindow();
	for (int i = 0; i < m_pTabsWnd->GetTabsNum (); i ++)
	{
		CTBToolBar* m_pToolBar = pTabbedToolBar->GetToolBar(i);
		EnumToolbarElements(pRibbonBarItemInfo, htreeRibbon, m_pToolBar);
	}
}

//----------------------------------------------------------------------------
void COslDlgDoc::EnumToolbarElements(COslTreeItem* pParent, HTREEITEM htmLevDoc, CTBToolBar* m_pToolBar)
{
	CString  strTitle;

	if (!m_pToolBar)
		return;

	strTitle = _T("");
	m_pToolBar->GetWindowTextW(strTitle); 

	// Append ToolBar node to tree Control
	HTREEITEM htreeToolBar = m_ctrlTree.InsertItem(strTitle, OSLDLG_BMP_TOOLBAR, OSLDLG_BMP_TOOLBAR, htmLevDoc);
	COslTreeItem* pToolBarItemInfo = new COslTreeItem(pParent, m_pToolBar->GetInfoOSL(), strTitle);
	m_arInfoTreeItems.Add(pToolBarItemInfo);
	m_ctrlTree.SetItemData(htreeToolBar, (DWORD)pToolBarItemInfo);
	for (int i = 0; i < m_pToolBar->m_arInfoOSL.GetSize(); i++)
	{
		CInfoOSLButton* pInfo = m_pToolBar->m_arInfoOSL.GetAt(i);

		if (!pInfo->m_Namespace.IsValid())
			continue;

		if (IsCommandFramework(pInfo->m_nID))
			continue;

		CString  strTitle = pInfo->m_Namespace.GetObjectName();
		HTREEITEM htreeNew = m_ctrlTree.InsertItem(strTitle, OSLDLG_BMP_TOOLBARBUTTON, OSLDLG_BMP_TOOLBARBUTTON, htreeToolBar);
		COslTreeItem* pTBtnItemInfo = new COslTreeItem(pToolBarItemInfo, pInfo, strTitle);
		m_arInfoTreeItems.Add(pTBtnItemInfo);
		m_ctrlTree.SetItemData(htreeNew, (DWORD)pTBtnItemInfo);

		

	}
}

//----------------------------------------------------------------------------
void COslDlgDoc::EnumRibbonBarElements(COslTreeItem* pParent, HTREEITEM htmLevDoc)
{
/*	CString  strTitle;
	CTBRibbonBar* pRibbonBar = m_pDoc->GetMasterFrame()->GetRibbonBar();
	if (!pRibbonBar) return;

	CObjectOSL* pO = new CObjectOSL();
	pO->m_InfoOSL.m_pParent = pParent->m_pInfo;
	pO->m_InfoOSL.SetType(OSLType_Control);
	pO->m_InfoOSL.m_Namespace =  pRibbonBar->GetInfoOSL()->m_Namespace;
	m_arInfoTreeItems.Add(pO);

	HTREEITEM htreeRibbon = m_ctrlTree.InsertItem (_TB("Ribbon toolbar"), OSLDLG_BMP_TOOLBAR, OSLDLG_BMP_TOOLBAR, htmLevDoc );
	COslTreeItem* pRibbonBarItemInfo = new COslTreeItem (pParent,  &(pO->m_InfoOSL), _T("RibbonToolbar"));
	m_arInfoTreeItems.Add(pRibbonBarItemInfo);
	m_ctrlTree.SetItemData(htreeRibbon, (DWORD)pRibbonBarItemInfo); 

	// score all ribbon category
	for (int iCategory=0; iCategory < pRibbonBar->GetCategoryCount(); iCategory++)
	{
		CTBRibbonCategory* pCategory = pRibbonBar->GetCategory(iCategory);
		if (!pCategory) continue;
		if (!pCategory->GetInfoOSL()->m_Namespace.IsValid()) 
				continue;

		// append node to tree Control
		strTitle = pCategory->GetInfoOSL()->m_Namespace.GetObjectName();
		HTREEITEM htreeCategory = m_ctrlTree.InsertItem (strTitle, OSLDLG_BMP_CONTROL, OSLDLG_BMP_CONTROL, htreeRibbon );
		// Append Ribbon to tree
		COslTreeItem* pRibbonItemInfo = new COslTreeItem (pRibbonBarItemInfo, pCategory->GetInfoOSL(), strTitle);
		m_arInfoTreeItems.Add(pRibbonItemInfo);
		m_ctrlTree.SetItemData (htreeCategory, (DWORD) pRibbonItemInfo); 

		// Score the Panel
		for (int iPanel=0; iPanel < pCategory->GetPanelCount(); iPanel++)
		{
			CTBRibbonPanel* pPanel = pCategory->GetPanel(iPanel);
			if (!pPanel) continue;
			if (!pPanel->GetInfoOSL()->m_Namespace.IsValid()) 
				continue;
			strTitle = pPanel->GetInfoOSL()->m_Namespace.GetObjectName();
			HTREEITEM htreePanel = m_ctrlTree.InsertItem (strTitle, OSLDLG_BMP_CONTROL, OSLDLG_BMP_CONTROL, htreeCategory);
			// Append Panel to tree
			COslTreeItem* pPanelItemInfo = new COslTreeItem (pRibbonItemInfo, pPanel->GetInfoOSL(), strTitle);
			m_arInfoTreeItems.Add(pPanelItemInfo);
			m_ctrlTree.SetItemData (htreePanel, (DWORD) pPanelItemInfo); 

			// Append Buttons to Tree
			RibbonBarButtonsElements(&pPanel->m_arButtonOSLInfo, pPanelItemInfo, htreePanel);

			// Append ButtonsGroup
			for (int iGroup = 0; iGroup < pPanel->m_arButonsGroup.GetSize(); iGroup++)
			{
				CTBRibbonButtonsGroup* pGroup = pPanel->m_arButonsGroup.GetAt(iGroup);
				if (!pGroup) continue;
				if (!pGroup->GetInfoOSL()->m_Namespace.IsValid())
				continue;
				strTitle = pGroup->GetInfoOSL()->m_Namespace.GetObjectName();
				HTREEITEM htreeGroup = m_ctrlTree.InsertItem (strTitle, OSLDLG_BMP_CONTROL, OSLDLG_BMP_CONTROL, htreePanel);
				// Append ButtonsGroup to tree
				COslTreeItem*  pGroupItemInfo = new COslTreeItem (pPanelItemInfo, pGroup->GetInfoOSL(), strTitle);
				m_arInfoTreeItems.Add(pGroupItemInfo);
				m_ctrlTree.SetItemData (htreeGroup, (DWORD) pGroupItemInfo); 

				// Append Buttons to Tree
				RibbonBarButtonsElements(&pGroup->m_arButtonOSLInfo, pGroupItemInfo, htreeGroup);
			}
		}
	}
*/
}

//----------------------------------------------------------------------------
void COslDlgDoc::RibbonBarButtonsElements(CArray<CInfoOSLButton*,CInfoOSLButton*>* pArButtonOSLInfo, COslTreeItem* pParent, HTREEITEM htree)
{
	// Append Buttons
	for (int i = 0; i < pArButtonOSLInfo->GetSize(); i++)
	{
		CInfoOSLButton* pInfo = pArButtonOSLInfo->GetAt(i);
		if (!pInfo->m_Namespace.IsValid()) 
		continue;
		CString  strTitle = pInfo->m_Namespace.GetObjectName();
		HTREEITEM htreeNew = m_ctrlTree.InsertItem (strTitle, OSLDLG_BMP_CONTROL, OSLDLG_BMP_CONTROL, htree);
		COslTreeItem* pTBtnItemInfo = new COslTreeItem (pParent, pInfo, strTitle);
		m_arInfoTreeItems.Add(pTBtnItemInfo);
		m_ctrlTree.SetItemData (htreeNew, (DWORD) pTBtnItemInfo); 
	}
}

//----------------------------------------------------------------------------
void COslDlgDoc::EnumTab (COslTreeItem* pParent, HTREEITEM htmLevParent, DlgInfoItem* pTabDlgInf, CTabDialog* pTab, DlgInfoArray* parDlgInf, int nTab) 
{
	IOSLObjectManager* pOSLManager = dynamic_cast<IOSLObjectManager*>(pTabDlgInf);
	CInfoOSL* pInfoOSL = pOSLManager ? pOSLManager->GetInfoOSL() : NULL;
	CString sName = pInfoOSL->m_Namespace.GetObjectName();

	CString strTitle = pTabDlgInf->m_strTitle;
	strTitle.Remove('&');
	if (strTitle.IsEmpty())
		strTitle = sName;

	ASSERT_TRACE( 
		sName.IsEmpty() || nTab == parDlgInf->Find(sName),
		cwsprintf(_T("Tab with duplicate namespace: %s, %s, %d\n"),
		sName, pTabDlgInf->GetDialogClass()->m_lpszClassName, pTabDlgInf->GetDialogID())
		);

	HTREEITEM htmLevTabDlg = NULL;
	if (sName.IsEmpty())
	{
		htmLevTabDlg = m_ctrlTree.InsertItem (strTitle, OSLDLG_BMP_TABDLG_EMPTY, OSLDLG_BMP_TABDLG_EMPTY, htmLevParent );
		m_ctrlTree.SetItemData (htmLevTabDlg, (DWORD) NULL); 
		return;
	}
	if (nTab != parDlgInf->Find(sName))
	{
		htmLevTabDlg = m_ctrlTree.InsertItem (strTitle, OSLDLG_BMP_TABDLG_DUPLICATE, OSLDLG_BMP_TABDLG_DUPLICATE, htmLevParent );
		m_ctrlTree.SetItemData (htmLevTabDlg, (DWORD) NULL); 
		return;
	}

	htmLevTabDlg = m_ctrlTree.InsertItem (strTitle, OSLDLG_BMP_TABDLG, OSLDLG_BMP_TABDLG, htmLevParent );
	COslTreeItem* pTabItemInfo = new COslTreeItem(pParent, pInfoOSL, strTitle);
		m_arInfoTreeItems.Add(pTabItemInfo);
		m_ctrlTree.SetItemData (htmLevTabDlg, (DWORD) pTabItemInfo); 

	// active dialog does not use form manager
	if (pTab && pTab->m_hWnd && pTab->GetDlgInfoItem() == pTabDlgInf)
	{
		EnumTabbers		(pTabItemInfo, htmLevTabDlg, pTab->GetChildTabManagers());
		EnumTileGroup	(pTabItemInfo, htmLevTabDlg, pTab->GetChildTileGroup());
		EnumBodyEdits	(pTabItemInfo, htmLevTabDlg, pTab);
		EnumControls	(pTabItemInfo, htmLevTabDlg, pTab->GetControlLinks());
		EnumPropertiesGrid(pTabItemInfo, htmLevTabDlg, pTab);

		return;
	}
	
	//---- enum BE in TabDialog
	if (pTabDlgInf->m_strlistChildren.GetCount())
	{
		for (int j = 0; j < pTabDlgInf->m_strlistChildren.GetCount(); j++)
		{
			CString strChildBE = pTabDlgInf->m_strlistChildren.GetAt(pTabDlgInf->m_strlistChildren.FindIndex(j));
					
			BodyEditInfo* pBodyEditInfo = NULL;
			for (int k = 0; k < m_pDoc->m_pFormManager->GetBodyEditInfos()->GetSize(); k++)
			{
				pBodyEditInfo = (BodyEditInfo*)(m_pDoc->m_pFormManager->GetBodyEditInfos()->GetAt(k));
				ASSERT(pBodyEditInfo);

				if (strChildBE.CompareNoCase(pInfoOSL->m_Namespace.ToString()) == 0)
					goto l_Enumbe;
			}
			continue;
		l_Enumbe:
			if (pBodyEditInfo)
			{
				//modifica alla struttura per adeguamento alla effettiva gerarchia
				pBodyEditInfo->GetInfoOSL()->m_pParent = pTabDlgInf->GetInfoOSL();

				CBodyEdit* pBody = NULL;	
				if (pTab && pTab->m_hWnd && pTab->GetDlgInfoItem() == pTabDlgInf)
				{
					int nIdx = 0;
					do
					{
						pBody = pTab->GetBodyEdits(&nIdx);
						nIdx++;
						if (pBody && pInfoOSL->m_Namespace == pBodyEditInfo->GetInfoOSL()->m_Namespace)
						{
							break;
						}
					} while (pBody);
				}
				if (pBody)
					EnumBodyEdit (pTabItemInfo, htmLevTabDlg, pBody);
				else
					EnumBodyEdit (pTabItemInfo, htmLevTabDlg, pBodyEditInfo);
			}
		}
	}
}

//----------------------------------------------------------------------------
void COslDlgDoc::EnumTabbers (COslTreeItem* pParent, HTREEITEM htmLevParent, TabManagers* pTabbers) //TODO LARA
{
	if (pTabbers == NULL) 
		return;

	for(int i = 0; i <= pTabbers->GetUpperBound(); i++)
	{
		CTabManager* pTM = (CTabManager*) (pTabbers->GetAt(i));
		ASSERT(pTM);
		if (pTM == NULL) break;

		IOSLObjectManager* pOSLManager = dynamic_cast<IOSLObjectManager*>(pTM);
		CInfoOSL* pInfoOSL = pOSLManager ? pOSLManager->GetInfoOSL() : NULL;

		CString strTitle = pInfoOSL->m_Namespace.GetObjectName();

		//BOOL existItem = FALSE;
		//for (int i = 0; i <= m_arInfoTreeItems.GetUpperBound(); i++)
		//{
		//	if (((COslTreeItem*)m_arInfoTreeItems[i])->m_pInfo->m_Namespace.ToString() == pInfoOSL->m_Namespace.ToString())
		//	{
		//		existItem = TRUE;
		//		break;
		//	}
		//}

		//if (existItem)
		//	return;

		HTREEITEM htmLevTabMngr = m_ctrlTree.InsertItem(strTitle, OSLDLG_BMP_TABMNGR, OSLDLG_BMP_TABMNGR, htmLevParent);
		COslTreeItem* pTabberItemInfo = new COslTreeItem(pParent, pInfoOSL, _T(""));
		m_arInfoTreeItems.Add(pTabberItemInfo);
		m_ctrlTree.SetItemData(htmLevTabMngr, (DWORD)pTabberItemInfo); 

		DlgInfoArray* parDlgInf = pTM->GetDlgInfoArray();
		ASSERT(parDlgInf);

		if (parDlgInf == NULL) break;

		for (int nTab = 0; nTab <= parDlgInf->GetUpperBound(); nTab++)
		{
			DlgInfoItem* pTabDlgInf	= parDlgInf->GetAt(nTab);
			EnumTab (pTabberItemInfo, htmLevTabMngr, pTabDlgInf, pTM->GetActiveDlg(), parDlgInf, nTab);
		}
	}
}

//-----------------------------------------------------------------------------
void COslDlgDoc::EnumTileGroup (COslTreeItem* pParent, HTREEITEM htmLevParent, CBaseTileGroup* pTileGroup)
{
	if (!pTileGroup)
		return;
	
	ASSERT_VALID(pTileGroup);

	IOSLObjectManager* pOSLManager = dynamic_cast<IOSLObjectManager*>(pTileGroup);
	CInfoOSL* pInfoOSL = pOSLManager ? pOSLManager->GetInfoOSL() : NULL;

	CString strTitle = pInfoOSL->m_Namespace.GetObjectName();

	//BOOL existItem = FALSE;
	//for (int i = 0; i <= m_arInfoTreeItems.GetUpperBound(); i++)
	//{
	//	if (((COslTreeItem*)m_arInfoTreeItems[i])->m_pInfo->m_Namespace.ToString() == pInfoOSL->m_Namespace.ToString())
	//	{
	//		existItem = TRUE;
	//		break;
	//	}
	//}

	//if (existItem)
	//	return;

	//ADD NODO
	HTREEITEM htmLevTab = m_ctrlTree.InsertItem(strTitle, OSLDLG_BMP_TILE, OSLDLG_BMP_TILE, htmLevParent);
	COslTreeItem* pTabItemInfo = new COslTreeItem(pParent, pInfoOSL, _T(""));
	m_arInfoTreeItems.Add(pTabItemInfo);
	m_ctrlTree.SetItemData(htmLevTab, (DWORD)pTabItemInfo);

	const	LayoutElementArray* tgElements = pTileGroup->GetContainedElements();

	for (int nElement = 0; nElement <= tgElements->GetUpperBound(); nElement++)
	{
		LayoutElement* pEment = tgElements->GetAt(nElement);

		CTilePanel* tl = dynamic_cast<CTilePanel*>(pEment);
		if (tl != NULL)
		{
			EnumTilePanel(pTabItemInfo, htmLevTab, tl);
			continue;
		}

		CBaseTileDialog* tile = dynamic_cast<CBaseTileDialog*>(pEment);
		if (tile != NULL)
		{
			EnumTile(pTabItemInfo, htmLevTab, tile);
			continue;
		}

		CLayoutContainer* container = dynamic_cast<CLayoutContainer*>(pEment);
		if (container == NULL)
			continue;

		const	LayoutElementArray* containerElemnet = container->GetContainedElements();
		for (int nElement = 0; nElement <= containerElemnet->GetUpperBound(); nElement++)
		{
			LayoutElement* pEment = containerElemnet->GetAt(nElement);

			CTilePanel* tl = dynamic_cast<CTilePanel*>(pEment);
			if (tl != NULL)
			{
				EnumTilePanel(pTabItemInfo, htmLevTab, tl);
				continue;
			}

			CBaseTileDialog* tile = dynamic_cast<CBaseTileDialog*>(pEment);
			if (tile != NULL)
			{
				EnumTile(pTabItemInfo, htmLevTab, tile);
				continue;
			}
		}	
	}
}

//-----------------------------------------------------------------------------
void COslDlgDoc::EnumTilePanel(COslTreeItem* pParent, HTREEITEM htmLevParent, CTilePanel* pTilep)
{
	if (!pTilep)
		return;
	
	IOSLObjectManager* pOSLManager = dynamic_cast<IOSLObjectManager*>(pTilep);
	CInfoOSL* pInfoOSL = pOSLManager ? pOSLManager->GetInfoOSL() : NULL;

	CString strTitle = pInfoOSL->m_Namespace.GetObjectName();

	HTREEITEM htmLevTab = m_ctrlTree.InsertItem(strTitle, OSLDLG_BMP_TILE, OSLDLG_BMP_TILE, htmLevParent);
	COslTreeItem* pTabItemInfo = new COslTreeItem(pParent, pInfoOSL, _T(""));
	m_arInfoTreeItems.Add(pTabItemInfo);
	m_ctrlTree.SetItemData(htmLevTab, (DWORD)pTabItemInfo);

	CTilePanelTab* tilepaneltab = pTilep->GetActiveTab();
	const	LayoutElementArray* tgElements;
	if (tilepaneltab == NULL)
		return;

	tgElements = tilepaneltab->GetContainedElements();//TODO LARA

	for (int nElement = 0; nElement <= tgElements->GetUpperBound(); nElement++)
	{
		LayoutElement* pEment = tgElements->GetAt(nElement);
		CLayoutContainer* container = dynamic_cast<CLayoutContainer*>(pEment);
		if (container != NULL)
		{

			const	LayoutElementArray* containerElemnet = container->GetContainedElements();
			for (int nElement = 0; nElement <= containerElemnet->GetUpperBound(); nElement++)
			{
				LayoutElement* pEment = containerElemnet->GetAt(nElement);

				CTilePanel* tl = dynamic_cast<CTilePanel*>(pEment);
				if (tl != NULL)
				{
					EnumTilePanel(pTabItemInfo, htmLevTab, tl);
					continue;
				}

				CBaseTileDialog* tile = dynamic_cast<CBaseTileDialog*>(pEment);
				if (tile != NULL)
				{
					EnumTile(pTabItemInfo, htmLevTab, tile);
					continue;

				}
			}
		}

		CBaseTileDialog* tile = dynamic_cast<CBaseTileDialog*>(pEment);
		if (tile != NULL)
			EnumTile(pTabItemInfo, htmLevTab, tile);			
	}
}

//-----------------------------------------------------------------------------
void COslDlgDoc::EnumTile (COslTreeItem* pParent, HTREEITEM htmLevParent, CBaseTileDialog* pTileDlg)
{
	if (!pTileDlg)
		return;
	
	IOSLObjectManager* pOSLManager = dynamic_cast<IOSLObjectManager*>(pTileDlg);
	CInfoOSL* pInfoOSL = pOSLManager ? pOSLManager->GetInfoOSL() : NULL;

	CString strTitle = pInfoOSL->m_Namespace.GetObjectName();

	//BOOL existItem = FALSE;
	//for (int i = 0; i <= m_arInfoTreeItems.GetUpperBound(); i++)
	//{
	//	if (((COslTreeItem*)m_arInfoTreeItems[i])->m_pInfo->m_Namespace.ToString() == pInfoOSL->m_Namespace.ToString())
	//	{
	//		existItem = TRUE;
	//		break;
	//	}
	//}

	//if (existItem)
	//	return;

	HTREEITEM htmLevTab = m_ctrlTree.InsertItem(strTitle, OSLDLG_BMP_TILE, OSLDLG_BMP_TILE, htmLevParent);
		COslTreeItem* pTabItemInfo = new COslTreeItem (pParent, pTileDlg->GetInfoOSL(), _T(""));
		m_arInfoTreeItems.Add(pTabItemInfo);
		m_ctrlTree.SetItemData(htmLevTab, (DWORD)pTabItemInfo); 

	if (pTileDlg->IsSecurityChildHidden())
		return;

	EnumControls  (pTabItemInfo, htmLevTab, pTileDlg->GetControlLinks());
//	EnumPropertiesGrid(pTabItemInfo, htmLevTab, pTileDlg); 
}

//--------------------------------------------------------------------------
void COslDlgDoc::EnumBodyEdit (COslTreeItem* pParent, HTREEITEM htmLevParent, Array* pBEInfo) //TODO CON GERMANO
{
	ASSERT_VALID(pBEInfo);
	ASSERT_KINDOF(BodyEditInfo, pBEInfo);
	BodyEditInfo* pBodyEditInfo = (BodyEditInfo*) pBEInfo;

	HTREEITEM htmLevBody = m_ctrlTree.InsertItem (pBodyEditInfo->m_strBodyTitle, OSLDLG_BMP_BODYEDIT, OSLDLG_BMP_BODYEDIT, htmLevParent);
	COslTreeItem *pBodyItemInfo = new COslTreeItem (pParent, pBodyEditInfo->GetInfoOSL(), pBodyEditInfo->m_strBodyTitle);
	m_arInfoTreeItems.Add(pBodyItemInfo);
	m_ctrlTree.SetItemData(htmLevBody, (DWORD)pBodyItemInfo); 

	for (int j = 0; j <= pBodyEditInfo->GetUpperBound(); j++)
	{
		BodyEditColumn* pBodyEditColumn = pBodyEditInfo->GetAt(j);
		ASSERT(pBodyEditColumn);
		HTREEITEM htmLevBodyCol = m_ctrlTree.InsertItem(pBodyEditColumn->m_strColumnTitle, OSLDLG_BMP_BODYEDITCOL, OSLDLG_BMP_BODYEDITCOL, htmLevBody );

		//la classe BodyEditColumn NON ha il data member m_InfoOSL: ne alloco uno temporaneo
		CObjectOSL* pO = new CObjectOSL();
		pO->m_InfoOSL.m_Namespace = pBodyEditColumn->m_Namespace;
		pO->m_InfoOSL.SetType(OSLType_BodyEditColumn);
		pO->m_InfoOSL.m_pParent = pBodyEditInfo->GetInfoOSL();
		COslTreeItem* pItemInfo = new COslTreeItem (pBodyItemInfo, &(pO->m_InfoOSL), pBodyEditColumn->m_strColumnTitle);

		//saranno deletati alla chiusura della finestra
		m_arInfoTreeItems.Add(pItemInfo);
		m_arInfoTreeItems.Add(pO);

		m_ctrlTree.SetItemData(htmLevBodyCol, (DWORD)pItemInfo); 
	}
}


//-----------------------------------------------------------------------------------
void COslDlgDoc::EnumPropertiesGrid(COslTreeItem* pParent, HTREEITEM htmLevParent, CParsedForm* pParsedForm)
{
	if (!pParsedForm || !pParsedForm->GetControlLinks())
		return;

	CTBPropertyGrid* prop = NULL;

	for (int i = 0; i <= pParsedForm->GetControlLinks()->GetUpperBound(); i++)
	{
		CWnd* pWnd = pParsedForm->GetControlLinks()->GetAt(i);
		if (!pWnd || !pWnd->IsKindOf(RUNTIME_CLASS(CTBPropertyGrid)))
			continue;

		prop = (CTBPropertyGrid*)pWnd;
		break;
	}

	if (prop == NULL)
		return;

	IOSLObjectManager* pOSLManager = dynamic_cast<IOSLObjectManager*>(prop);
	CInfoOSL* pInfoOSL = pOSLManager ? pOSLManager->GetInfoOSL() : NULL;

	// il titolo può derivare da fonti diverse
	CString sProp = prop->GetName();
	if (sProp.IsEmpty())
		sProp = pInfoOSL->m_Namespace.GetObjectName();

	HTREEITEM htmLevBody = m_ctrlTree.InsertItem(sProp, OSLDLG_BMP_BODYEDIT, OSLDLG_BMP_BODYEDIT, htmLevParent);
	COslTreeItem *pBodyItemInfo = new COslTreeItem(pParent, pInfoOSL, sProp);
	m_arInfoTreeItems.Add(pBodyItemInfo);
	m_ctrlTree.SetItemData(htmLevBody, (DWORD)pBodyItemInfo);
}

//-----------------------------------------------------------------------------------
void COslDlgDoc::EnumBodyEdits (COslTreeItem* pParent, HTREEITEM htmLevParent, CParsedForm* pParsedForm)
{
	if (!pParsedForm || !pParsedForm->GetControlLinks())
		return;

	for (int i=0; i <= pParsedForm->GetControlLinks()->GetUpperBound(); i++)
	{
		CWnd* pWnd = pParsedForm->GetControlLinks()->GetAt(i);
		if (!pWnd || !pWnd->IsKindOf(RUNTIME_CLASS(CBodyEdit)))
			continue;

		EnumBodyEdit (pParent, htmLevParent, (CBodyEdit*) pWnd);
	} 
}

//-----------------------------------------------------------------------------------
void COslDlgDoc::EnumBodyEdit (COslTreeItem* pParent, HTREEITEM htmLevParent, CBodyEdit* pBodyEdit)
{
	ASSERT_VALID(pBodyEdit);
	if (!pBodyEdit)
		return;

	IOSLObjectManager* pOSLManager = dynamic_cast<IOSLObjectManager*>(pBodyEdit);
	CInfoOSL* pInfoOSL = pOSLManager ? pOSLManager->GetInfoOSL() : NULL;

	// il titolo può derivare da fonti diverse
	CString sTile = pBodyEdit->GetTitle().IsEmpty() ?  pBodyEdit->GetRowFormViewTitle() : pBodyEdit->GetTitle();
	if (sTile.IsEmpty())
		sTile = pInfoOSL->m_Namespace.GetObjectName();

	HTREEITEM htmLevBody = m_ctrlTree.InsertItem (sTile, OSLDLG_BMP_BODYEDIT, OSLDLG_BMP_BODYEDIT, htmLevParent);
	COslTreeItem *pBodyItemInfo = new COslTreeItem(pParent, pInfoOSL, sTile);
	m_arInfoTreeItems.Add(pBodyItemInfo);
	m_ctrlTree.SetItemData(htmLevBody, (DWORD)pBodyItemInfo); 
	
	ColumnInfo* pColInfo;
	for (int j = 0; j <= pBodyEdit->GetAllColumnsInfoUpperBound(); j++)
	{
		pColInfo = pBodyEdit->GetColumnFromIdx (j);
		ASSERT(pColInfo);

		pOSLManager = dynamic_cast<IOSLObjectManager*>(pColInfo);
		pInfoOSL = pOSLManager ? pOSLManager->GetInfoOSL() : NULL;

		HTREEITEM htmLevBodyCol = m_ctrlTree.InsertItem(pColInfo->GetTitle(), OSLDLG_BMP_BODYEDITCOL, OSLDLG_BMP_BODYEDITCOL, htmLevBody );

		COslTreeItem* pItemInfo = new COslTreeItem(pBodyItemInfo, pInfoOSL, pColInfo->GetTitle());

		//saranno deletati alla chiusura della finestra
		m_arInfoTreeItems.Add(pItemInfo);

		m_ctrlTree.SetItemData(htmLevBodyCol, (DWORD)pItemInfo); 
	}

	// user buttons
	HTREEITEM htmLevToolbar = NULL; CObjectOSL* pO = NULL; COslTreeItem* pToolbarItemInfo = NULL;
/*
	for (int k = 0; k < pBodyEdit->m_UserButtons.GetSize(); k++)
	{
		CBodyBitmapButton*	pBtn = (CBodyBitmapButton*)(pBodyEdit->m_UserButtons.GetAt(k));
		if (!pBtn->m_InfoOSL.m_Namespace.IsValid()) 
			continue;

		if (htmLevToolbar == NULL)
		{
			pO = new CObjectOSL();
			pO->m_InfoOSL.m_pParent = &(pBodyEdit->m_InfoOSL);
			pO->m_InfoOSL.SetType(OSLType_Skip);	//NO OSLType_Toolbar
			m_arInfoTreeItems.Add(pO);

			htmLevToolbar = m_ctrlTree.InsertItem (_TB("Mini toolbar"), OSLDLG_BMP_TOOLBAR, OSLDLG_BMP_TOOLBAR, htmLevBody);
			pToolbarItemInfo = new COslTreeItem (pBodyItemInfo,  &(pO->m_InfoOSL), _T("MiniToolbar"));
			m_arInfoTreeItems.Add(pToolbarItemInfo);
			m_ctrlTree.SetItemData(htmLevToolbar, (DWORD)pToolbarItemInfo); 
		}

		HTREEITEM htmBodyBtn = m_ctrlTree.InsertItem (pBtn->m_strToolTip, OSLDLG_BMP_TOOLBARBUTTON, OSLDLG_BMP_TOOLBARBUTTON, htmLevToolbar);
		COslTreeItem* pBtnInfo = new COslTreeItem (pToolbarItemInfo, &(pBtn->m_InfoOSL), pBtn->m_strToolTip);
		m_arInfoTreeItems.Add(pBtnInfo);
		m_ctrlTree.SetItemData (htmBodyBtn, (DWORD) pBtnInfo); 
	}
*/
	for (int k = 0; k < pBodyEdit->m_HeaderToolBar.GetSize(); k++)
	{
		CBEButton*	pBtn = pBodyEdit->m_HeaderToolBar.GetAt(k);
		if (CBodyEdit::IsStandardButton(pBtn->m_nID))
			continue;

		if (htmLevToolbar == NULL)
		{
			htmLevToolbar = m_ctrlTree.InsertItem (_TB("Header toolbar"), OSLDLG_BMP_TOOLBAR, OSLDLG_BMP_TOOLBAR, htmLevBody);
			pToolbarItemInfo = new COslTreeItem (pBodyItemInfo, pBodyEdit->m_HeaderToolBar.GetInfoOSL(), _T("HeaderToolbar"));
			m_arInfoTreeItems.Add(pToolbarItemInfo);
			m_ctrlTree.SetItemData(htmLevToolbar, (DWORD)pToolbarItemInfo); 
		}

		HTREEITEM htmBodyBtn = m_ctrlTree.InsertItem (pBtn->GetTooltip(), OSLDLG_BMP_TOOLBARBUTTON, OSLDLG_BMP_TOOLBARBUTTON, htmLevToolbar);
		COslTreeItem* pBtnInfo = new COslTreeItem (pToolbarItemInfo, pBtn->GetInfoOSL(), pBtn->GetText() + L" - " + pBtn->GetTooltip());
		m_arInfoTreeItems.Add(pBtnInfo);
		m_ctrlTree.SetItemData (htmBodyBtn, (DWORD) pBtnInfo); 

		for (int y = 0; y < pBtn->m_CInfoOSLs.GetSize(); y++)
		{
			CInfoOSLButton* info = pBtn->m_CInfoOSLs.GetAt(y);

			HTREEITEM htmBodyBtn2 = m_ctrlTree.InsertItem(info->m_strName, OSLDLG_BMP_TOOLBARBUTTON, OSLDLG_BMP_TOOLBARBUTTON, htmBodyBtn);
			COslTreeItem* pBtnInfo2 = new COslTreeItem(pBtnInfo, info, info->m_strName);
			m_arInfoTreeItems.Add(pBtnInfo2);
			m_ctrlTree.SetItemData(htmBodyBtn2, (DWORD)pBtnInfo2);
		}
	}
	for (int k = 0; k < pBodyEdit->m_FooterToolBar.GetSize(); k++)
	{
		CBEButton*	pBtn = pBodyEdit->m_FooterToolBar.GetAt(k);
		if (CBodyEdit::IsStandardButton(pBtn->m_nID))
			continue;

		if (htmLevToolbar == NULL)
		{
			htmLevToolbar = m_ctrlTree.InsertItem (_TB("Footer toolbar"), OSLDLG_BMP_TOOLBAR, OSLDLG_BMP_TOOLBAR, htmLevBody);
			pToolbarItemInfo = new COslTreeItem (pBodyItemInfo, pBodyEdit->m_FooterToolBar.GetInfoOSL(), _T("FooterToolbar"));
			m_arInfoTreeItems.Add(pToolbarItemInfo);
			m_ctrlTree.SetItemData(htmLevToolbar, (DWORD)pToolbarItemInfo); 
		}

		HTREEITEM htmBodyBtn = m_ctrlTree.InsertItem(pBtn->GetTooltip(), OSLDLG_BMP_TOOLBARBUTTON, OSLDLG_BMP_TOOLBARBUTTON, htmLevToolbar);
		COslTreeItem* pBtnInfo = new COslTreeItem(pToolbarItemInfo, pBtn->GetInfoOSL(), pBtn->GetText() + L" - " + pBtn->GetTooltip());
		m_arInfoTreeItems.Add(pBtnInfo);
		m_ctrlTree.SetItemData (htmBodyBtn, (DWORD) pBtnInfo); 

		for (int y = 0; y < pBtn->m_CInfoOSLs.GetSize(); y++)
		{
			CInfoOSLButton* info = pBtn->m_CInfoOSLs.GetAt(y);

			HTREEITEM htmBodyBtn2 = m_ctrlTree.InsertItem(info->m_strName, OSLDLG_BMP_TOOLBARBUTTON, OSLDLG_BMP_TOOLBARBUTTON, htmBodyBtn);
			COslTreeItem* pBtnInfo2 = new COslTreeItem(pBtnInfo, info, info->m_strName);
			m_arInfoTreeItems.Add(pBtnInfo2);
			m_ctrlTree.SetItemData(htmBodyBtn2, (DWORD)pBtnInfo2);
		}
	}
}

//-----------------------------------------------------------------------------
void COslDlgDoc::EnumView(COslTreeItem* pParent, HTREEITEM htmLevParent, CAbstractFormView* pView, BOOL isMaster)
{
	if (!pView)
		return;
	ASSERT_VALID(pView);

	if (pView->IsKindOf(RUNTIME_CLASS(CSlaveViewContainer)))
	{
		return;
	}

	CString strTitle;
	pView->GetParent()->GetWindowText(strTitle);
	if (strTitle.IsEmpty())
		strTitle = pView->GetNamespace().GetObjectName();

	int bmp = OSLDLG_BMP_SLAVEVW;

	CMasterFormView* masterView = dynamic_cast<CMasterFormView*>(pView);
	if (masterView == NULL && isMaster)
		return;

	

	COslTreeItem*pViewItemInfo = new COslTreeItem(pParent, pView->GetInfoOSL(), pView->GetNamespace().GetObjectName());
	HTREEITEM htmLevView = m_ctrlTree.InsertItem(strTitle, bmp, bmp, htmLevParent);
	m_ctrlTree.SetItemData(htmLevView, (DWORD)pViewItemInfo);
	m_arInfoTreeItems.Add(pViewItemInfo);

	if (pView->m_pTabManagers != NULL)
		EnumTabbers(pViewItemInfo, htmLevView, pView->m_pTabManagers);
	EnumControls(pViewItemInfo, htmLevView, pView->GetControlLinks());
	
	const	LayoutElementArray* tgElements = pView->GetContainedElements();
	if (tgElements == NULL)
	{
		EnumBodyEdits(pViewItemInfo, htmLevView, pView);
		return;
	}

	for (int nElement = 0; nElement <= tgElements->GetUpperBound(); nElement++)
	{
		LayoutElement* pEment = tgElements->GetAt(nElement);

		IOSLObjectManager* pOSLManager = dynamic_cast<IOSLObjectManager*>(pEment);
		CInfoOSL* pInfoOSL = pOSLManager ? pOSLManager->GetInfoOSL() : NULL;
		strTitle = pInfoOSL->m_Namespace.GetObjectName();

		CHeaderStrip* pHeader = dynamic_cast<CHeaderStrip*>(pEment);
		if (pHeader != NULL)
			continue;

		BOOL existItem = FALSE;
		for (int i = 0; i <= m_arInfoTreeItems.GetUpperBound(); i++)
		{
			if (((COslTreeItem*)m_arInfoTreeItems[i])->m_pInfo->m_Namespace.ToString() == pInfoOSL->m_Namespace.ToString())
			{
				existItem = TRUE;
				break;
			}
		}

		if (existItem)
			continue;


		//HTREEITEM htmLevTab = m_ctrlTree.InsertItem(strTitle, OSLDLG_BMP_TILE, OSLDLG_BMP_TILE, htmLevView);
		COslTreeItem* pTabItemInfo = new COslTreeItem(pViewItemInfo, pInfoOSL, strTitle);

		//m_arInfoTreeItems.Add(pTabItemInfo);
		//m_ctrlTree.SetItemData(htmLevTab, (DWORD)pTabItemInfo);
		//EnumElements(pTabItemInfo, htmLevTab, pEment);

		CTileManager* pTManager = dynamic_cast<CTileManager*>(pEment);
		if (pTManager != NULL)
		{
			CTileGroup* pTG = pTManager->GetActiveTileGroup();
			EnumTileGroup(pViewItemInfo, htmLevView, pTG);
		}

		TabManagers* pTabManagers = dynamic_cast<TabManagers*>(pEment);
		if (pTabManagers != NULL)
			EnumTabbers(pParent, htmLevParent, pTabManagers);

		CBaseTileDialog* tile = dynamic_cast<CBaseTileDialog*>(pEment);
		if (tile != NULL)
			EnumTile(pParent, htmLevParent, tile);

		CTileGroup* pTG = dynamic_cast<CTileGroup*>(pEment);
		if (pTG != NULL)
			EnumTileGroup(pViewItemInfo, htmLevView, pTG);
	}

}

//-----------------------------------------------------------------------------
void COslDlgDoc::EnumElements(COslTreeItem* pParent, HTREEITEM htmLevParent, LayoutElement* pElement) //TODO LARA
{
	const	LayoutElementArray* tgElements = pElement->GetContainedElements();
	if (tgElements != NULL)
	{
		for (int nElement = 0; nElement <= tgElements->GetUpperBound(); nElement++)
		{
			LayoutElement* pChildElement = tgElements->GetAt(nElement);

			CLayoutContainer* container = dynamic_cast<CLayoutContainer*>(pChildElement);

			if (container != NULL)
			{
				EnumElements(pParent, htmLevParent, pChildElement);
				continue;
			}

			CTilePanel* tl = dynamic_cast<CTilePanel*>(pChildElement);
			if (tl != NULL)
			{
				EnumTilePanel(pParent, htmLevParent, tl);
				continue;
			}
			IOSLObjectManager* pOSLManager = dynamic_cast<IOSLObjectManager*>(pChildElement);
			CInfoOSL* pInfoOSL = pOSLManager ? pOSLManager->GetInfoOSL() : NULL;

			CString strTitle = pInfoOSL->m_Namespace.GetObjectName();

			HTREEITEM htmLevTab = m_ctrlTree.InsertItem(strTitle, OSLDLG_BMP_TILE, OSLDLG_BMP_TILE, htmLevParent);
			COslTreeItem* pTabItemInfo = new COslTreeItem(pParent, pInfoOSL, _T(""));
			m_arInfoTreeItems.Add(pTabItemInfo);
			m_ctrlTree.SetItemData(htmLevTab, (DWORD)pTabItemInfo);

			const	LayoutElementArray* tgElementChilds = pChildElement->GetContainedElements();
			if (tgElementChilds != NULL && tgElementChilds->GetUpperBound() > 0)
				EnumElements(pTabItemInfo, htmLevTab, pChildElement);

			CParsedForm* parsedForm = dynamic_cast<CParsedForm*>(pChildElement);
			if (parsedForm != NULL)
				EnumControls(pTabItemInfo, htmLevTab, parsedForm->GetControlLinks());
		}
	}

	CAbstractFormView* pView = dynamic_cast<CAbstractFormView*>(pElement);
	if (pView != NULL)
		EnumTabbers(pParent, htmLevParent, pView->m_pTabManagers);
}

//-----------------------------------------------------------------------------
void COslDlgDoc::EnumDialog(COslTreeItem* pParent, HTREEITEM htmLevParent, CParsedDialog* pDialog)
{
	if (!pDialog)
		return;

	ASSERT_VALID(pDialog);

	CString strTitle;
	pDialog->GetParent()->GetWindowText(strTitle);
	if (strTitle.IsEmpty())
		strTitle = pDialog->GetNamespace().GetObjectName();

	int bmp = OSLDLG_BMP_SLAVEVW;

	COslTreeItem* pViewItemInfo = pParent;
	HTREEITEM htmLevView = htmLevParent;

	if (!pDialog->IsKindOf(RUNTIME_CLASS(CMasterFormView)))
	{
		pViewItemInfo = new COslTreeItem(pParent, pDialog->GetInfoOSL(), _T(""));
		htmLevView = m_ctrlTree.InsertItem(strTitle, bmp, bmp, htmLevParent);
		m_ctrlTree.SetItemData(htmLevView, (DWORD)pViewItemInfo);
		m_arInfoTreeItems.Add(pViewItemInfo);
	}


	CLayoutContainer* layoutContainer = pDialog->GetLayoutContainer();
	
	if (layoutContainer == NULL)
		return;
	
	const LayoutElementArray* tgElements = layoutContainer->GetContainedElements();
	
	for (int nElement = 0; nElement <= tgElements->GetUpperBound(); nElement++)
	{
		LayoutElement* pEment = tgElements->GetAt(nElement);
		CTileGroup* pTG = dynamic_cast<CTileGroup*>(pEment);
		if (pTG != NULL)
			EnumTileGroup(pViewItemInfo, htmLevView, pTG);
	}
	
	EnumToolbarElements(pViewItemInfo, htmLevView, pDialog->GetToolbar());
	EnumControls(pViewItemInfo, htmLevView, pDialog->GetControlLinks());
	EnumBodyEdits(pViewItemInfo, htmLevView, pDialog);
	EnumPropertiesGrid(pViewItemInfo, htmLevView, pDialog);

}
//-----------------------------------------------------------------------------
void COslDlgDoc::FillAllTree() 
{
	m_arInfoTreeItems.RemoveAll();
	m_bFilling = TRUE;
	
	ASSERT(m_pDoc->GetDocTemplate());
	ASSERT(m_pDoc->GetDocTemplate()->IsKindOf(RUNTIME_CLASS(CSingleExtDocTemplate)));
	CSingleExtDocTemplate* pMasterTplDoc = (CSingleExtDocTemplate*)(m_pDoc->GetDocTemplate());
	if (pMasterTplDoc == NULL)
		return;

	int nBmp = OslTypeToBmp(m_pDoc->GetInfoOSL()->GetType()); 
	CString strTitle = m_pDoc->GetTitle();

	HTREEITEM htmLevDoc = m_ctrlTree.InsertItem(strTitle, nBmp, nBmp, TVI_ROOT, TVI_LAST );
	COslTreeItem * pDocItemInfo = new COslTreeItem (NULL, m_pDoc->GetInfoOSL(), strTitle);
	m_arInfoTreeItems.Add(pDocItemInfo);
	m_ctrlTree.SetItemData (htmLevDoc, (DWORD) pDocItemInfo); 

	EnumTabbedToolbarElements(pDocItemInfo, htmLevDoc);
	EnumRibbonBarElements(pDocItemInfo, htmLevDoc);
	EnumCaptionBarObjects(pDocItemInfo, htmLevDoc);

	CAbstractFormView* pView2 = NULL;
	POSITION pos = m_pDoc->GetFirstViewPosition();
	while(pos)
	{
		CView* pVw = m_pDoc->GetNextView(pos);
		ASSERT(pVw);

		if (!pVw->IsKindOf(RUNTIME_CLASS(CAbstractFormView))) 
			continue;
		CAbstractFormView* pView = (CAbstractFormView*) pVw;
		if (pView2 != NULL && pView->GetNamespace().GetObjectName() == pView2->GetNamespace().GetObjectName())
			continue;

		pView2 = pView;

		if (pView->IsKindOf(RUNTIME_CLASS(CRowFormView)) || pView->IsKindOf(RUNTIME_CLASS(CSlaveFormView)))
			EnumView(pDocItemInfo, htmLevDoc, pView, FALSE);
		else
		EnumView(pDocItemInfo, htmLevDoc, pView);
	}
	// scorre tutte le child window della dialog
	CDockingPanes* arrDock = m_pDoc->GetMasterFrame()->GetDockPane();

	//for (int i = 0; i < arrDock->GetSize(); i++)
	//{

	//	CTaskBuilderDockPane* pCtrlBar = dynamic_cast<CTaskBuilderDockPane*>(arrDock->GetAt(i));
	//	for (int j = 0; j < pCtrlBar->m_Forms.GetCount(); j++)
	//	{
	//		CTaskBuilderDockPaneForm* pDlgPanel = (CTaskBuilderDockPaneForm*)pCtrlBar->m_Forms.GetAt(j);

	//		//DIALOG (ES EASYATTACHMENT)
	//		CParsedDialog* pDlg = dynamic_cast<CParsedDialog*>(pDlgPanel->GetWnd());
	//		if (pDlg)
	//			EnumDialog(pDocItemInfo, htmLevDoc, pDlg);

	//		//VIEW (ES LINK)
	//		CAbstractFormView* pView = dynamic_cast<CAbstractFormView*>(pDlgPanel->GetWnd());
	//		if (pView != NULL)
	//			EnumView(pDocItemInfo, htmLevDoc, pView, FALSE);
	//	}
	//}
	
	m_pDoc->DispatchBuildingSecurityTree(&m_ctrlTree, &m_arInfoTreeItems);
	m_bFilling = FALSE;
}
