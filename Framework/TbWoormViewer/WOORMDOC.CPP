#include "stdafx.h"

#include <io.h>
#include <sys\stat.h>
#include <mmsystem.h>
#include <stdlib.h>
#include <locale.h>
#include <winspool.h>

#include <TbFrameworkImages\CommonImages.h>
#include <TbFrameworkImages\GeneralFunctions.h>

#include <TBXmlCore\XMLSchema.h>
#include <TBXmlCore\XMLTags.h>
																																   
#include <TbNameSolver\ThreadContext.h>
#include <TbNameSolver\LoginContext.h>
#include <TbNameSolver\Chars.h>
#include <TbNameSolver\PathFinder.h>
#include <TbNameSolver\FileSystemFunctions.h>
#include <TbNameSolver\Templates.h>

#include <TbGeneric\stack.h>
#include <TbGeneric\globals.h>
#include <TbGeneric\FontsTable.h>
#include <TbGeneric\dib.h>
#include <TbGeneric\dibitmap.h>
#include <TbGeneric\RdeProtocol.h>
#include <TbGeneric\FormatsTable.h>
#include <TbGeneric\ParametersSections.h>
#include <TbGeneric\WebServiceStateObjects.h>
#include <TbGeneric\EnumsTable.h>

#include <TbWebServicesWrappers\LoginManagerInterface.h>

#include <TbParser\FontsParser.h>
#include <TbParser\FormatsParser.h>

#include <TbGenlib\baseapp.h>
#include <TbGenlib\funproto.h>
#include <TbGenlib\reswalk.h>
#include <TbGenlib\addonmng.h>
#include <TbGenlib\baseapp.h>
#include <TbGenlib\baseapp.h>
#include <TbGenlib\oslbaseinterface.h>
#include <TbGenlib\const.h>
#include <TbGenlib\TbTreeCtrl.h>

#include <TbGenlibManaged\EmfToPdf.h>
#include <TbGenlibManaged\ZCompress.h>
#include <TbGenLibManaged\Main.h>
#include <TbGenLibManaged\MenuFunctions.h>

#include <TbGenlibUI\FormatDialog.h>
#include <TbGenlibUI\FontsDialog.h>
#include <TbGenlibUI\TBExplorer.h>
#include <TbGenlibUI\TBExplorerUtility.h>
#include <TbGenlibUI\SettingsTableManager.h>

#include <TbOledb\oledbmng.h>

#include <TbWoormEngine\edtmng.h>
#include <TbWoormEngine\qrywrkid.h>
#include <TbWoormEngine\repfield.h>
#include <TbWoormEngine\reptable.h>
#include <TbWoormEngine\report.h>
#include <TbWoormEngine\rulengin.h>
#include <TbWoormEngine\ReportLink.h>
#include <TbWoormEngine\MultiLayout.h>
#include <TbWoormEngine\ExportSymbols.h>
#include <TbWoormEngine\disptbl.h>
#include <TbWoormEngine\ActionsRepEngin.h>
#include <TbWoormEngine\PRGDATA.H>
#include <TbWoormEngine\InputMng.H>


#include "pageinfo.h"
#include "woormfrm.h"
#include "woormvw.h"
#include "baseobj.h"
#include "singleob.h"
#include "mulselob.h"
#include "rectobj.h"
#include "column.h"
#include "table.h"
#include "repeater.h"
#include "viewpars.h"
#include "listdlg.h"
#include "woormini.h"
#include "woormdoc.h"
#include "export.h"           
#include "docproperties.h"           
#include "woorminfoviewer.h"
#include "soapfunctions.h"
#include "TBPrintDialog.h"
#include "woormfrm.h"
#include "woormdoc.h"
#include "RSEditorUI.h"
#include "RSEditView.h"
#include "ExpExter.h"

//................................. resources

#include "commands.hrc"	//NECESSARIO sino a che non sara disponibile l'editor degli hjson

#include "woormdoc.hjson" //JSON AUTOMATIC UPDATE
#include "woormfrm.hjson" //JSON AUTOMATIC UPDATE
#include "listdlg.hjson" //JSON AUTOMATIC UPDATE
#include "export.hjson" //JSON AUTOMATIC UPDATE
#include "rectobj.hjson" //JSON AUTOMATIC UPDATE
#include "TbGenlib\BASEFRM.hjson" //JSON AUTOMATIC UPDATE
#include <TbGes\extdoc.hjson>

//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char  THIS_FILE[] = __FILE__;
#endif


static const UINT TIMER_TIME_OUT = 500; // half second

TCHAR* DEFAULT_CRYPT_PDF_PASSWORD	=_T("M0i0c5r7o2a2r2e0a1 0S1.p.A.");

#define NUM_CURSORS 10
//------------------------------------------------------------------------------
static const HCURSOR* LoadCursors()
{   
	// initilization of static cursor array for speed optimization
	// Note: all track cursors must live in same module
	HINSTANCE hInst = AfxFindResourceHandle(
		MAKEINTRESOURCE(IDC_SQRRECT), RT_GROUP_CURSOR);
	static HCURSOR NEAR rghCursors[NUM_CURSORS];

	// initialize the cursor array
	rghCursors[0] = ::LoadCursor(hInst, MAKEINTRESOURCE(IDC_SQRRECT));
	rghCursors[1] = ::LoadCursor(hInst, MAKEINTRESOURCE(IDC_TEXTRECT));
	rghCursors[2] = ::LoadCursor(hInst, MAKEINTRESOURCE(IDC_GRAPHRECT));
	rghCursors[3] = ::LoadCursor(hInst, MAKEINTRESOURCE(IDC_FILERECT));
	rghCursors[4] = ::LoadCursor(hInst, MAKEINTRESOURCE(IDC_FIELDRECT));
	rghCursors[5] = ::LoadCursor(hInst, MAKEINTRESOURCE(IDC_TABLE));
	rghCursors[6] = ::LoadCursor(hInst, MAKEINTRESOURCE(IDC_GRAPHRECT));
	rghCursors[7] = ::LoadCursor(NULL,  MAKEINTRESOURCE(IDC_ARROW));
	rghCursors[8] = ::LoadCursor(NULL,  MAKEINTRESOURCE(IDC_HAND));
	rghCursors[9] = ::LoadCursor(hInst, MAKEINTRESOURCE(IDC_TEXTRECT));
	return rghCursors;
}

#define WOORM_RELEASE	7
static const TCHAR szDefaultPageLayout[] = _T("Default");

/////////////////////////////////////////////////////////////////////////////
BOOL LoadTextFile(CString sFileName, CString& sContent)
{
	if (!IsFileName(sFileName))
		sFileName = AfxGetPathFinder()->FromNs2Path(sFileName, CTBNamespace::TEXT, CTBNamespace::FILE);

	if (!IsFileName(sFileName))
	{
		return FALSE;
	}
	if (!::LoadLineTextFile(sFileName, sContent)) 
	{
		return FALSE;
	}
	return TRUE;
}


/////////////////////////////////////////////////////////////////////////////
// 							InvalidArea
/////////////////////////////////////////////////////////////////////////////
//
//-----------------------------------------------------------------------------                      
IMPLEMENT_DYNAMIC (InvalidArea, CObject)                   

//-----------------------------------------------------------------------------                      
InvalidArea::InvalidArea (const CRect& aRect, BOOL bRepaint)
{
	m_bRepaint = bRepaint;
	m_invalid  = aRect;
}


/////////////////////////////////////////////////////////////////////////////
// 							CObArrayLockable
/////////////////////////////////////////////////////////////////////////////
//
//-----------------------------------------------------------------------------                      
INT_PTR CObArrayLockable::Add (CObject* po)
{
	TB_LOCK_FOR_WRITE();

	int i = 0;
	for (i = 0; i < GetSize(); i++)
	{
		if (GetAt(i) == po)
			break;
	}
	if (i == GetSize())
		return __super::Add(po);
	else 
		return -1;
}

//-----------------------------------------------------------------------------                      
void CObArrayLockable::RemoveAll ()
{
	TB_LOCK_FOR_WRITE();

	int n = GetSize();
	CObject* pO;
	for (int i = 0; i < n; i++) 
		if (pO = GetAt(i)) 
		{
			ASSERT_VALID(pO);
			delete pO;
		}

	__super::RemoveAll();
}


/////////////////////////////////////////////////////////////////////////////
// CPrintDialog1

DECLARE_AND_INIT_THREAD_VARIABLE(CWnd*, pCurrPrintDlg, NULL);

//-----------------------------------------------------------------------------
class CPrintDialog1 : public CTBPrintDialog
{
	
public:
	CPrintDialog1 (CWnd* parent) 
		: 
			CTBPrintDialog (FALSE, PD_ALLPAGES | PD_NOPAGENUMS | PD_NOSELECTION | PD_USEDEVMODECOPIESANDCOLLATE | PD_ENABLEPRINTHOOK, parent)
		{
			GET_THREAD_VARIABLE(CWnd*, pCurrPrintDlg);
			m_pd.lpfnPrintHook = PrintHookProc;
			
			pCurrPrintDlg = this;
		}

	static	UINT_PTR CALLBACK PrintHookProc
		(
			HWND hdlg,
			UINT uiMsg,
			WPARAM wParam,
			LPARAM lParam
		)
	{
		GET_THREAD_VARIABLE(CWnd*, pCurrPrintDlg);
		if (uiMsg == WM_INITDIALOG)
		{
			PRINTDLG* pds = (PRINTDLG*) lParam;

			CWnd* pCtrl = pCurrPrintDlg->GetDlgItem(0x482);//edit copies
			if (pCtrl)
			{
				pCtrl->EnableWindow(FALSE);
				pCtrl->ShowWindow(SW_HIDE);
			}
			pCtrl = pCurrPrintDlg->GetDlgItem(0x270F); //up-down copies
			if (pCtrl)
			{
				pCtrl->EnableWindow(FALSE);
				pCtrl->ShowWindow(SW_HIDE);
			}
			pCtrl = pCurrPrintDlg->GetDlgItem(0x444);//static "Number of copies:"
			if (pCtrl)
			{
				pCtrl->ShowWindow(SW_HIDE);
			}
			//pCtrl = pCurrPrintDlg->GetDlgItem(0x43E);//bitmap 11 22 33
			//if (pCtrl)
			//{
			//	pCtrl->ShowWindow(SW_HIDE);	
			//}
			//pCtrl = pCurrPrintDlg->GetDlgItem(0x411); //chekbox "Collate"
			//if (pCtrl)
			//{
			//	pCtrl->ShowWindow(SW_HIDE);
			//}
			//pCtrl = pCurrPrintDlg->GetDlgItem(0x431); //groupbox "Copies"
			//if (pCtrl)
			//{
			//	pCtrl->ShowWindow(SW_HIDE);
			//}
			return 1;
		} 
		return 0; //return != 0 eats msg 
	}
};


/////////////////////////////////////////////////////////////////////////////
// CWoormTemplate

//-------------------------------------------------------------------------
CWoormTemplate::~CWoormTemplate() 
{ 
	Unload();
}

void CWoormTemplate::Unload()
{
	m_sNsTemplate.Empty();
	m_bLoaded = FALSE;

	if (m_pWoormTpl)
	{
		//AfxGetTbCmdManager()->CloseWoormReport(m_pWoormTpl);
		delete m_pWoormTpl;
		m_pWoormTpl = NULL;
	}

	ASSERT(_CrtCheckMemory());
}

/////////////////////////////////////////////////////////////////////////////
// CMultiLayouts

//-------------------------------------------------------------------------
CLayout::CLayout (const CString& strLayoutName, CWoormDocMng* pWDoc, BOOL bAddAutoSpecialFields/* = TRUE*/)
	:
	m_strLayoutName (strLayoutName),
	m_pWoorm		(pWDoc),
	m_nCountAutoObjects (0)
{
	if (pWDoc && bAddAutoSpecialFields)
	{
		FieldRect* f = NULL;

		f = new FieldRect(CPoint(0,0), pWDoc, SpecialReportField::ID.PAGE);
			//f->RightSizeObject (0);
			//f->BottomSizeObject (0);
			f->SetLabelText(SpecialReportField::NAME.PAGE);
			f->SetHidden(); f->SetBaseRect(CRect(0,0,0,0));
			f->m_bPersistent = FALSE;
		Add(f);

		f = new FieldRect(CPoint(0,0), pWDoc, SpecialReportField::ID.LAYOUT);
			//f->RightSizeObject (0);
			//f->BottomSizeObject (0);
			f->SetLabelText(SpecialReportField::NAME.LAYOUT);
			f->SetHidden(); f->SetBaseRect(CRect(0,0,0,0));
			f->m_bPersistent = FALSE;
		Add(f);

		//----
		m_nCountAutoObjects = GetSize();
	}
	
	// predispone il sort trasparent/ordinate/ascisse per minimizzare la rasterizzazione e rendere più leggibile il layout
	SetCompareFunction(BaseObj_CompareZYX);
}

//-------------------------------------------------------------------------
CLayout::CLayout (const CLayout& source)
{
	m_strLayoutName		= source.m_strLayoutName;
	m_pWoorm			= source.m_pWoorm;
	m_nCountAutoObjects = source.m_nCountAutoObjects;

	for (int i = 0; i < source.GetSize(); i ++)
	{
		BaseObj* pObj = source[i];
		BaseObj* pCloneObj = pObj->Clone();

		if (!pCloneObj)
		{
			ASSERT(FALSE);
			continue;
		}
		Add (pCloneObj);
	}
	// predispone il sort trasparent/ordinate/ascisse per minimizzare la rasterizzazione e rendere più leggibile il layout
	SetCompareFunction(BaseObj_CompareZYX);
}

::BaseObj*	CLayout::operator[](int nIndex) const { return (::BaseObj*) GetAt(nIndex); }
::BaseObj*&	CLayout::operator[](int nIndex) { return (::BaseObj*&) ElementAt(nIndex); }

//-------------------------------------------------------------------------
int  CLayout::FindIndexByID(WORD id)
{
	for (int i = 0; i < GetSize(); i++)
	{
		if (id == (*this)[i]->GetInternalID())
		{
			return i;
		}
	}
	return -1;
}

//-------------------------------------------------------------------------
BaseObj*  CLayout::FindByID(WORD id)
{
	for (int i = 0; i < GetSize(); i++)
	{
		if (id == (*this)[i]->GetInternalID())
		{
			return (*this)[i];
		}
	}
	return NULL;
}

//-------------------------------------------------------------------------
CObject* CLayout::FindObjectByID(WORD id)
{
	for (int i = 0; i < GetSize(); i++)
	{
		BaseObj* pObj = (*this)[i];

		if (pObj->GetInternalID() == id)
			return pObj;

		if (pObj->IsKindOf(RUNTIME_CLASS(Table)))
		{
			Table* pTable = (Table*)pObj;
			int idx = pTable->GetIdxColFromAlias(id);
			if (idx != -1)
				return const_cast<TableColumn*>(pTable->GetColumn(idx));
		}
	}
	return NULL;
}

//-------------------------------------------------------------------------
const TableColumn* CLayout::FindColumnByID (WORD id)
{
	for (int i = 0; i < GetSize(); i++)
	{
		BaseObj* pObj = (*this)[i];
		if (pObj->IsKindOf(RUNTIME_CLASS(Table)))
		{
			if (pObj->GetInternalID() == id)
				break;

			Table* pTable = (Table*) pObj;
			int idx = pTable->GetIdxColFromAlias(id);
			if (idx != -1)
				return pTable->GetColumn(idx);
		}
	}
	return NULL;
}

//-------------------------------------------------------------------------
const BOOL CLayout::ExistsColumnID (WORD id)
{
	for (int i = 0; i < GetSize(); i++)
	{
		BaseObj* pObj = (*this)[i];
		if (pObj->IsKindOf(RUNTIME_CLASS(Table)))
		{
			if (pObj->GetInternalID() == id)
				break;

			Table* pTable = (Table*) pObj;
			int idx = pTable->GetIdxColFromAlias(id);
			if (idx != -1)
				return TRUE;
		}
		else if (pObj->IsKindOf(RUNTIME_CLASS(Repeater)))
		{
			if (pObj->GetInternalID() == id)
				break;

			Repeater* pRep = (Repeater*) pObj;
			int idx = pRep->ExistChildID(id);
			if (idx != -1)
				return TRUE;
		}
	}
	return FALSE;
}

//------------------------------------------------------------------------------
Table* CLayout::FindTableByID(WORD nID/*=0*/)
{
	Table* pTable = NULL;
	for (int i = 0; i <= GetUpperBound(); i++)
	{
		if (
			(nID ? (*this)[i]->GetInternalID() == nID : TRUE)	&&
			(*this)[i]->IsKindOf(RUNTIME_CLASS(Table))
			)
		{
			pTable = (Table*) (*this)[i];
		}
	}
	return pTable;
}

//-------------------------------------------------------------------------
BOOL CLayout::CanDeleteField (LPCTSTR pszFieldName, CString& sLog) const
{
	for (int i = 0; i < GetSize(); i++)
	{
		if (!(*this)[i]->CanDeleteField(pszFieldName, sLog))
		{
			return FALSE;
		}
	}
	return TRUE;
}

//-------------------------------------------------------------------------
void CLayout::ClearDynamicAttributes () 
{
	for (int i = 0; i < GetSize(); i++)
	{
		(*this)[i]->ClearDynamicAttributes();
	}
}

//-------------------------------------------------------------------------
void CLayout::DisableData () 
{
	for (int i = 0; i < GetSize(); i++)
	{
		(*this)[i]->DisableData();
	}
}

//-------------------------------------------------------------------------
void CLayout::ApplyRepeater () 
{
	for (int i = 0; i < GetSize(); i++)
	{
		BaseObj* pO = (*this)[i];
		if (pO->IsKindOf(RUNTIME_CLASS(Repeater)))
			((Repeater*)pO)->Rebuild(this);
	}
}

//-------------------------------------------------------------------------
void CLayout::MoveBaseRect (int xOffset, int yOffset, BOOL bIgnoreBorder/* = FALSE*/)
{
	for (int i = 0; i < GetSize(); i++)
	{
		BaseObj* pObj = (*this)[i];

		if (pObj->IsKindOf(RUNTIME_CLASS(BaseRect)))
		{
			BaseRect* pR = (BaseRect*) pObj;

			pR->MoveBaseRect(xOffset, yOffset, bIgnoreBorder);
		}
		else if (pObj->IsKindOf(RUNTIME_CLASS(Table)))
		{
			Table* pT = (Table*)  pObj;

			pT->MoveTable (CSize(xOffset, yOffset));
		}
	}
}

//-------------------------------------------------------------------------
void CLayout::Paint (CDC& DC, CPrintInfo* pi)
{
	for (int i = 0; i < GetSize(); i++)
	{
		BaseObj* pObj = (*this)[i];

		pObj->Paint (DC, pi);
	}
}

//-------------------------------------------------------------------------
void CLayout::Draw (CDC& DC, BOOL bPreview)
{
	for (int i = 0; i < GetSize(); i++)
	{
		BaseObj* pObj = (*this)[i];

		pObj->Draw (DC, bPreview);
	}
}

//-------------------------------------------------------------------------
void CLayout::GetObjDescription ()
{
	for (int i = 0; i < GetSize(); i++)
	{
		BaseObj* pObj = (*this)[i];

		pObj->GetObjDescription();
	}
}

//-------------------------------------------------------------------------
void CLayout::Unparse (ViewUnparser& ofile, BOOL bIsSavingTemplate, BOOL& bThereIsTemplate)
{
	// write all elements CLayout* pObjects
	int ub = GetUpperBound();
	for (int i = 0; i <= ub; i++)
	{
		BaseObj* pObj = (*this)[i];

		if (pObj->m_bInheritByTemplate)
			continue;
		if (!pObj->m_bPersistent)
			continue;

		if (bIsSavingTemplate)
		{
			if (pObj->IsKindOf(RUNTIME_CLASS(Table)))
			{
				if (!((Table*)pObj)->IsTemplate())
					continue;
				bThereIsTemplate = TRUE;

				Table* pTable = new Table (*(Table*)pObj);

				pTable->MarkTemplateOverridden();

				pTable->MergeTemplateColumns();

				pTable->PurgeTemplateColumns();

				pObj = pTable;
			} 
			else if (pObj->IsKindOf(RUNTIME_CLASS(BaseRect)))
			{
				if (!((BaseRect*)pObj)->IsTemplate())
					continue;
				bThereIsTemplate = TRUE;

				((BaseRect*)pObj)->MarkTemplateOverridden();
			}
		}

		pObj->Unparse(ofile);
	}
}

//-------------------------------------------------------------------------
//cerca alternado la direzione, le coordinate di un rettangolo libero delle dimensioni indicate
//cerca priima in alto, poi in basso. 
//Si ferma quando la ricerca si incontra al centro della pagina
//NB: numericamente TOP < BOTTOM, TOPa video in alto nela pagina
BOOL CLayout::FindFreeRect (int height, int width, CRect& rectTarget)
{
	ASSERT(m_pWoorm);

	//pWoormView->GetPageSizeWithoutMargin_LP()
	CSize sizePage = m_pWoorm->m_PageInfo.GetPageSize_LP();
		sizePage.cx -= m_pWoorm->m_PageInfo.m_rectMargins.right;
		sizePage.cy -= m_pWoorm->m_PageInfo.m_rectMargins.bottom;
		sizePage.cx -= m_pWoorm->m_PageInfo.m_rectMargins.left;
		sizePage.cy -= m_pWoorm->m_PageInfo.m_rectMargins.top;
	//----

	BOOL	bTopDown	= TRUE;
	int		nTopDownY	= 0;
	int		nBottomUpY	= sizePage.cy;

	for (; nTopDownY < nBottomUpY; bTopDown = !bTopDown)
	{
		rectTarget.left = sizePage.cx - width;
		rectTarget.right = sizePage.cx;

		if (bTopDown)	
		{
			rectTarget.top = nTopDownY;
			rectTarget.bottom = rectTarget.top + height;
			if (rectTarget.bottom > sizePage.cy) 
				break;

			if (FindFreeRect (width, rectTarget))
				return TRUE;

			nTopDownY++;
		}
		else
		{
			rectTarget.bottom  = nBottomUpY;
			rectTarget.top = rectTarget.bottom - height;
			if (rectTarget.top < 0) 
				break;

			if (FindFreeRect (width, rectTarget))
				return TRUE;
			
			nBottomUpY--;
		}
	}
	return FALSE;
}

BOOL CLayout::FindFreeRect (int width, CRect& rectTarget)
{
	//check one row (right to left)
	CRect rectInt(0,0,0,0);
	for(;;)
	{
		int i = 0;
		for (; i < GetSize(); i ++)
		{
			BaseObj* pObj = (*this)[i];

			if (pObj->IsAlwaysHidden())
				continue;

			Table* pTable = pObj->IsKindOf(RUNTIME_CLASS(Table)) ? (Table*) pObj : NULL;

			const CRect& ro = pTable ? pTable->NormalizedBaseRect(TRUE) : pObj->GetBaseRect();

			if (!rectInt.IntersectRect(&rectTarget, &ro))
				continue;
			if (rectInt.Height() == 0 || rectInt.Height() == 0 )
			{
				ASSERT(FALSE);
				continue;
			}

			break;
		}
		
		if (i == GetSize())	
			return TRUE;

		rectTarget.right--;	
		rectTarget.left--;

		if (rectTarget.left < 0)
			return FALSE;	
	}
	return FALSE;
}
/*
//-------------------------------------------------------------------------
//cerca alternado la direzione, le coordinate di un rettangolo libero delle dimensioni indicate
//cerca priima in alto, poi in basso. si sposta nella striscia identificata dal limite più vicino al bordo 
//degli oggetti che hanno colliso al passo precedente (ci sono 2 set distini di variabili)
//si ferma quando la ricerca si incontra al centro della pagina
//NB: numericamente TOP < BOTTOM, TOPa video in alto nela pagina
BOOL CLayout::FindFreeRect (int height, int width, CRect& rectTarget)
{
	ASSERT(m_pWoorm);

	CSize sizePage = m_pWoorm->m_PageInfo.GetPageSize();
	sizePage.cx -= m_pWoorm->m_PageInfo.m_rectMargins.right;
	sizePage.cy -= m_pWoorm->m_PageInfo.m_rectMargins.bottom;
	sizePage.cx -= m_pWoorm->m_PageInfo.m_rectMargins.left;
	sizePage.cy -= m_pWoorm->m_PageInfo.m_rectMargins.top;

	BOOL TopDown = TRUE;

	int nTopDownY = 0;
	int nMinBottom = sizePage.cy;

	int nBottomUpY = sizePage.cy;
	int nMaxTop = 0;
	
	int dummy = 0;
	BOOL bFirst = TRUE;

	for (; nMinBottom > nMaxTop || bFirst; TopDown = !TopDown)
	{
		bFirst = FALSE;
		
		rectTarget.left = sizePage.cx - width;
		rectTarget.right = sizePage.cx;

		if (1) //if (TopDown)	// 
		{

			rectTarget.top = nTopDownY;
			rectTarget.bottom = rectTarget.top + height;
			if (rectTarget.bottom > sizePage.cy) 
				break;

			if (FindFreeRect (width, rectTarget, dummy, nMinBottom))
				return TRUE;

			nTopDownY++; // = nMinBottom + 1;	//++
			nMinBottom = sizePage.cy;
		}
		else
		{
			rectTarget.bottom  = nBottomUpY;
			rectTarget.top = rectTarget.bottom - height;
			if (rectTarget.top < 0) 
				break;

			if (FindFreeRect (width, rectTarget, nMaxTop, dummy))
				return TRUE;
			
			nBottomUpY--;// = nMaxTop - 1;	//--
			nMaxTop = 0;
		}
	}
	return FALSE;
}

BOOL CLayout::FindFreeRect (int width, CRect& rectTarget, int& nMaxTop, int& nMinBottom)
{
	//check one row (right to left)
	CRect rectInt;
	int maxLeft = 0;
	for(;;)
	{
		BOOL bInt = FALSE;
		for (int i = 0; i < GetSize(); i ++)
		{
			BaseObj* pObj = (*this)[i];

			if (pObj->IsAlwaysHidden())
				continue;

			Table* pTable = pObj->IsKindOf(RUNTIME_CLASS(Table)) ? (Table*) pObj : NULL;

			const CRect& ro = pTable ? pTable->NormalizedBaseRect(TRUE) : pObj->GetBaseRect();

			if (!rectInt.IntersectRect(&rectTarget, &ro))
				continue;
			if (rectInt.Height() == 0 || rectInt.Height() == 0 )
			{
				ASSERT(FALSE);
				continue;
			}
			bInt = TRUE;

			//ricerca Top-Down: MinBottom - cerco coordinate bordo inferiore piu' alto nella pagina (numericamente più basso)
			if (nMinBottom > ro.bottom)
				nMinBottom = ro.bottom;

			//ricerca Bottom-Up: MaxTop - cerco coordinate bordo superiore piu' basso nella pagina (numericamente più alto)
			if (nMaxTop < ro.top)
				nMaxTop = ro.top;
			
			//ricerca left-right
			if (maxLeft < ro.left)
				maxLeft = ro.left;
		}
		if (!bInt)
			return TRUE;

		//evita loop infinito
		if (rectTarget.right == (maxLeft - 1))
			return FALSE;	//maxLeft--;	

		rectTarget.right = maxLeft - 1;
		rectTarget.left = rectTarget.right - width;

		if (rectTarget.left < 0)
			return FALSE;	
	}
	return FALSE;
}
*/
///////////////////////////////////////////////////////////////////////////////

int BaseObj_CompareYX(CObject*arg1, CObject* arg2)
{
	BaseObj* o1 = (BaseObj*)arg1; CRect& r1 = o1->m_BaseRect;
	BaseObj* o2 = (BaseObj*)arg2; CRect& r2 = o2->m_BaseRect;

	if (r1.top == r2.top)
	{
		int x = r1.left - r2.left;
		if (x)
			return x < 0 ? -1 : 1;

		x = r1.right - r2.right;
		return x < 0 ? 1 : (x > 0 ? -1 : 0);
	}
	int y = r1.top - r2.top;
	if (y)
		return y < 0 ? -1 : 1;

	y = r1.bottom - r2.bottom;
	return y < 0 ? 1 : (y > 0 ? -1 : 0);
}

int BaseObj_CompareZYX(CObject*arg1, CObject* arg2)
{
	BaseObj* o1 = (BaseObj*)arg1; CRect& r1 = o1->m_BaseRect;
	BaseObj* o2 = (BaseObj*)arg2; CRect& r2 = o2->m_BaseRect;

	int cmp = 0;
	for (;;)
	{
		if (r1.top == r2.top)
		{
			int x = r1.left - r2.left;
			if (x)
			{
				cmp = x < 0 ? -1 : 1;
				break;
			}

			x = r1.right - r2.right;
			cmp = x < 0 ? 1 : (x > 0 ? -1 : 0);
			break;
		}
		int y = r1.top - r2.top;
		if (y)
		{
			cmp = y < 0 ? -1 : 1;
			break;
		}

		y = r1.bottom - r2.bottom;
		cmp = y < 0 ? 1 : (y > 0 ? -1 : 0);
		break;
	}

	if (o1->m_bTransparent == o2->m_bTransparent)
		return cmp;

	CRect rectIntersect;
	if (!rectIntersect.IntersectRect(r1, r2))
		return cmp;

	if (o1->m_wInternalID && (o1->m_wInternalID == o2->m_AnchorRepeaterID))
	{
		return -1;
	}
	if (o2->m_wInternalID && (o2->m_wInternalID == o1->m_AnchorRepeaterID))
	{
		return 1;
	}

	//i campi trasparenti che iniziano prima di un campo non trasparente vasso spostati in avanti
	//es. titoli fittizi di una tabella (Germano)
	if (cmp < 0 && o1->m_bTransparent)	//implicita: && !o2->m_bTransparent
	{
		cmp = 1;
	}
	//ANOMALIA 18672: non vale l'inverso!
	//altrimenti i campi trasparenti posti sopra ad un'immagine di sfondo vengono nascosti
	//else if (cmp > 0)	//implicite: && !o1->m_bTransparent && o2->m_bTransparent
	//{
	//	cmp = -1;
	//}
	else if (cmp == 0)
	{
		cmp = o1->m_bTransparent ? 1 : -1;
	}

	return cmp;
}

/////////////////////////////////////////////////////////////////////////////
// CMultiLayouts

//-----------------------------------------------------------------------------
CMultiLayouts::CMultiLayouts (CWoormDocMng* pDoc)
	: 	
	m_pDocument		(pDoc)
{
}

//-----------------------------------------------------------------------------
BOOL CMultiLayouts::Lookup (CString sLayoutName, CLayout*& pObject) const
{
	sLayoutName.MakeLower();
	return __super::Lookup(sLayoutName, (CObject*&)pObject);
}

//-----------------------------------------------------------------------------
BOOL CMultiLayouts::Lookup (CString sLayoutName) const
{
	CLayout* pObj;
	return Lookup(sLayoutName, pObj);
}

//-----------------------------------------------------------------------------
//non aggiunge doppioni
CLayout* CMultiLayouts::Add (CString sLayoutName)
{
	CLayout* pBaseObjAr = new CLayout(sLayoutName, m_pDocument);
	sLayoutName.MakeLower();

	CLayout* pObj;
	if (Lookup(sLayoutName, pObj))
	{
		delete pBaseObjAr;
		return pObj;
	}

	SetAt (sLayoutName, pBaseObjAr);
	return pBaseObjAr;
}

//-----------------------------------------------------------------------------
BOOL CMultiLayouts::Rename (CString sPrevName, const CString& sNewName)
{
	CString sNewKey(sNewName);	sNewKey.MakeLower();
	sPrevName.MakeLower();

	CLayout* pObj;
	if (Lookup(sNewKey, pObj))
	{
		return FALSE;
	}
	if (!Lookup(sPrevName, pObj))
	{
		return FALSE;
	}

	if (!__super::RemoveKey(sPrevName))
		return FALSE;

	SetAt (sNewKey, pObj);

	pObj->m_strLayoutName = sNewName;
	return TRUE;
}

//-----------------------------------------------------------------------------
void CMultiLayouts::RemoveAll ()
{
	CString strName; CObject* pObj = NULL;
	for (POSITION pos = GetStartPosition(); pos != NULL; pObj = NULL, strName.Empty())
	{
		GetNextAssoc (pos, strName, pObj );
		CLayout* pObjects = (CLayout*) pObj;
		SAFE_DELETE (pObjects);
	}
	__super::RemoveAll();
}

//-----------------------------------------------------------------------------
BOOL CMultiLayouts::ExistsFieldID	(WORD id, BOOL bSkipCurrent) const
{
	CString strName; CObject* pObj = NULL;
	for (POSITION pos = GetStartPosition(); pos != NULL; pObj = NULL, strName.Empty())
	{
		GetNextAssoc (pos, strName, pObj );
		CLayout* pObjects = (CLayout*) pObj;
		
		if (bSkipCurrent && m_pDocument->m_Objects == pObjects)
			continue;

		if (pObjects->FindByID(id))
			return TRUE;
		if (pObjects->FindColumnByID(id))
			return TRUE;
	}
	return FALSE;
}

//-----------------------------------------------------------------------------
BOOL CMultiLayouts::ExistsID	(WORD id, CString sLayoutName) const
{
	return FindByID(id, sLayoutName) != NULL;
}

//-----------------------------------------------------------------------------
BOOL CMultiLayouts::ExistsColumnID	(WORD id, CString sLayoutName) const
{
	CLayout* pObjects = NULL;
	sLayoutName.MakeLower();
	if (! __super::Lookup(sLayoutName, (CObject*&)pObjects))
		return FALSE;
	return pObjects->ExistsColumnID(id);
}

//-----------------------------------------------------------------------------
BaseObj* CMultiLayouts::FindByID (WORD id, CString sLayoutName) const
{
	CLayout* pObjects = NULL;
	sLayoutName.MakeLower();
	if (! __super::Lookup(sLayoutName, (CObject*&)pObjects))
		return FALSE;
	return pObjects->FindByID(id);
}

//-----------------------------------------------------------------------------
const TableColumn* CMultiLayouts::FindColumnByID (WORD id, CString sLayoutName) const
{
	CLayout* pObjects = NULL;
	sLayoutName.MakeLower();
	if (! __super::Lookup(sLayoutName, (CObject*&)pObjects))
		return FALSE;
	return pObjects->FindColumnByID(id);
}

//-----------------------------------------------------------------------------
void CMultiLayouts::EnumObjects (TFEnumObjects pfAction)
{
	CString strName; CObject* pObj = NULL;
	for (POSITION pos = GetStartPosition(); pos != NULL; pObj = NULL, strName.Empty())
	{
		GetNextAssoc (pos, strName, pObj );
		CLayout* pObjects = (CLayout*) pObj;

		(m_pDocument->*(pfAction))(pObjects);
	}
}

//-----------------------------------------------------------------------------
BOOL CMultiLayouts::SetCurrent (CString sName)
{
	if (GetCount() <= 1)
		return TRUE;
	CLayout* pObj;
	if (Lookup(sName, pObj))
	{
		m_pDocument->m_dsCurrentLayoutView = sName;
		m_pDocument->m_Objects = pObj;
		return TRUE;
	}
	return FALSE;
}

//-----------------------------------------------------------------------------
BOOL CMultiLayouts::CanDeleteField	(LPCTSTR pszFieldName, CString& sLog) const
{
	CString strName; CObject* pObj = NULL;
	for (POSITION pos = GetStartPosition(); pos != NULL; pObj = NULL, strName.Empty())
	{
		GetNextAssoc (pos, strName, pObj );
		CLayout* pObjects = (CLayout*) pObj;
		
		if (!pObjects->CanDeleteField (pszFieldName, sLog))
			return FALSE;
	}
	return TRUE;
}

/////////////////////////////////////////////////////////////////////////////
// CWoormDocMng

//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CWoormDocMng, CWoormDoc)

//-----------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CWoormDocMng, CWoormDoc)

	ON_COMMAND(ID_APP_NEW_REPORT,  				OnFileNew)
	ON_COMMAND(ID_APP_OPEN_REPORT,				OnFileOpen)
	
	ON_COMMAND(ID_FILE_SAVE,  				OnFileSave)
	ON_COMMAND(ID_FILE_SAVE_2,				OnFileSave)

	ON_COMMAND(ID_FILE_SAVE_AS,				OnFileSaveAs)
	ON_COMMAND(ID_FILE_SAVE_RDE,   			OnFileSaveRDE)
	ON_COMMAND(ID_FILE_SAVE_PDF,   			OnFileSavePDF)

	ON_COMMAND(ID_FILE_PAGE_SETUP,			OnPageSetup)
	ON_COMMAND(ID_FILE_PROPERTIES,			OnFileProperties)

	ON_COMMAND(ID_SAVE_ONLY_GRAPH_INFO,		ToggleSaveOnlyGraphInfo)

	ON_COMMAND(ID_RS_REMOVE_LAYOUT_OBJECT,	OnObjectCut) 

	ON_UPDATE_COMMAND_UI(ID_OBJECT_CUT,					OnUpdateCut)
	ON_UPDATE_COMMAND_UI(ID_RS_REMOVE_LAYOUT_OBJECT,	OnUpdateCut)

	ON_COMMAND(ID_OBJECT_COPY,				OnObjectCopy)
	ON_COMMAND(ID_OBJECT_PASTE,				OnObjectPaste)
	ON_COMMAND(ID_OBJECT_UNDO,				OnObjectUndo)

	ON_COMMAND(ID_CLEAR_ALL,				OnClearAll)
	ON_COMMAND(ID_SELECT_ALL,      			OnSelectAll)
	ON_COMMAND(ID_UNSELECT_ALL,    			OnDeselectAll)

	ON_COMMAND(ID_ALLOW_EDITING,  			OnAllowEditing)

	ON_COMMAND(ID_SHOW_EDIT_TOOLBARS,		OnShowEditToolbars)

	ON_COMMAND(ID_ESCAPE,          OnStop)
	ON_COMMAND(ID_RUN,             OnRun)
	ON_COMMAND(ID_COMPILE,         OnCompile)
	ON_COMMAND(ID_STOP,            OnStop)
	ON_COMMAND(ID_PAUSE,           OnPause)
	ON_COMMAND(ID_RESUME,          OnResume)
	ON_COMMAND(ID_RUN_STOP,        OnRunStop)
	ON_COMMAND(ID_RUN_STOP_2,	   OnRunStop)
	ON_COMMAND(ID_PAUSE_RESUME,    OnPauseResume)
	ON_COMMAND(ID_RS_RUN_DEBUGGER, OnRunDebugger)

	ON_COMMAND(ID_RUN_WITHOUT_CHILDS,	OnRunWithOutChild)
	ON_COMMAND(ID_RUN_AND_ATTACH_PDF_CHILDS, OnRunAndAttachChild)

	ON_COMMAND(ID_PRINT_WITH_CHILDS,	OnPrintWithChild)
	ON_COMMAND(ID_PRINT_MULTICOPIES,	OnPrintMultiCopies)
	ON_COMMAND(ID_PRINT_WITH_DIALOG,	OnPrintWithDialog)
	ON_COMMAND(ID_PRINT_ON_LETTERHEAD, OnTogglePrintOnLetterhead)

	ON_COMMAND(ID_ARCHIVE_PDF_FORMAT, OnArchivePdfFormat)
	ON_COMMAND(ID_ARCHIVE_WITH_CHILDS, OnArchiveWithChilds)
	ON_COMMAND(ID_GENERATE_PAPERY,		OnGeneratePapery)

	ON_COMMAND(ID_GOTO_PAGE,       OnGotoPage)

	ON_COMMAND(ID_TOOLBAR_FIND_EDITBOX, OnFindWord)
	ON_COMMAND(ID_TOOLBAR_FIND_PREV, OnFindPrev)
	ON_COMMAND(ID_TOOLBAR_FIND_NEXT, OnFindNext)
	ON_COMMAND(ID_FILE_PRINT_PREVIEW, OnFilePrintPreview)
		
	FIND_BAR_DROPDOWN_MENU_ON_UPDATE_COMMAND_UI(OnUpdateFindWord, OnUpdateFindWord, OnUpdateFindWord, OnUpdateFindWord)
	FIND_BAR_DROPDOWN_MENU_ON_UPDATE_COMMAND()

	ON_COMMAND(ID_ADD_SQR_RECT,    OnAddSqrRect)
	ON_COMMAND(ID_ADD_TEXT_RECT,   OnAddTextRect)
	ON_COMMAND(ID_ADD_GRAPH_RECT,  OnAddGraphRect)
	ON_COMMAND(ID_ADD_FILE_RECT,   OnAddFileRect)
	ON_COMMAND(ID_ADD_REPEATER,    OnAddRepeater)
	ON_COMMAND(ID_ADD_CHART,		OnAddChart)

	ON_COMMAND(ID_RS_ADD_TABLE_FROM_DB,				OnAddTableFromDB)
	ON_COMMAND(ID_RS_ADD_TABLE_NEW_FUNCEXPR,		OnAddTableFromFuncExpr)
	ON_COMMAND(ID_RS_ADD_TABLE_FROM_HIDDEN_FIELDS,  OnAddTableFromHiddens)

	ON_COMMAND(ID_RS_ADD_COLUMNS_FROM_DB,			OnCreateDbObj)
	ON_COMMAND(ID_RS_ADD_COLUMN_NEW_FUNCEXPR,		OnCreateFuncExprObj)
	ON_COMMAND(ID_RS_ADD_COLUMNS_FROM_HIDDENS,		OnCreateObjFromHiddenField)

	ON_COMMAND(ID_RS_ADD_FIELDS_FROM_DB,			OnCreateDbObj)
	ON_COMMAND(ID_RS_ADD_FIELD_NEW_FUNCEXPR,		OnCreateFuncExprObj)
	ON_COMMAND(ID_RS_ADD_FIELDS_FROM_HIDDENS,		OnCreateObjFromHiddenField)

	ON_UPDATE_COMMAND_UI(ID_RS_ADD_TABLE_FROM_DB,	OnUpdateAddTableOrFields)
	ON_UPDATE_COMMAND_UI(ID_RS_ADD_FIELDS_FROM_DB,	OnUpdateAddTableOrFields)
	ON_UPDATE_COMMAND_UI(ID_RS_ADD_COLUMNS_FROM_DB, OnUpdateAddColumns)

	ON_COMMAND(ID_COL_LMOVE,       OnColLeftMove)
	ON_COMMAND(ID_COL_RMOVE,       OnColRightMove)

	ON_COMMAND(ID_MULSEL_ALIGN,    OnMulSelAlign)
	ON_COMMAND(ID_MULSEL_SIZE,     OnMulSelSize)

	ON_COMMAND(ID_OPTIONS,         OnOptions)
	ON_COMMAND(ID_WOORM_INI,       OnWoormIni)

	ON_COMMAND(ID_OPTION_CREATE_SCHEMA1,			OnToggleCreateSchema1)
	ON_UPDATE_COMMAND_UI(ID_OPTION_CREATE_SCHEMA1,	OnUpdateCreateSchema1)
	ON_COMMAND(ID_OPTION_CREATE_SCHEMA2,			OnToggleCreateSchema2)
	ON_UPDATE_COMMAND_UI(ID_OPTION_CREATE_SCHEMA2,	OnUpdateCreateSchema2)

	ON_COMMAND(ID_FONT_STYLES,     OnFontStyles)
	ON_COMMAND(ID_FORMAT_STYLES,   OnFormatStyles)

	ON_COMMAND(ID_ENUMS_VIEWER,	   OnEnumsViewer)
	ON_UPDATE_COMMAND_UI(ID_ENUMS_VIEWER, OnUpdateEnumsViewer)

	ON_UPDATE_COMMAND_UI(ID_TOOLS, OnUpdateTools)
	ON_UPDATE_COMMAND_UI(ID_TOOLS_2, OnUpdateTools)

	ON_COMMAND			(ID_GENERAL_SETTINGS,	OnGeneralSettings)
	ON_UPDATE_COMMAND_UI(ID_GENERAL_SETTINGS,	OnUpdateGeneralSettings)

	ON_COMMAND			(ID_PROPERTIES,			OnProperties )
	ON_UPDATE_COMMAND_UI(ID_PROPERTIES,			OnUpdateProperties)

	ON_COMMAND			(ID_PAGE_INFO,			OnPageInfo)
	ON_UPDATE_COMMAND_UI(ID_PAGE_INFO,			OnUpdatePageInfo)

	ON_COMMAND(ID_REPORT_PARAMETER, OnShowReportParameters) 

	ON_COMMAND(ID_RDE_NEWPAGE,     	OnNewPage)
	ON_COMMAND(ID_PG_UP,   			OnPrevPage)
	ON_COMMAND(ID_PG_DN,   			OnNextPage)
	ON_COMMAND(ID_PG_HOME, 			OnFirstPage)
	ON_COMMAND(ID_PG_END,  			OnLastPage)

	ON_COMMAND(ID_REPORT_BEGIN,		OnReportBegin)
	ON_COMMAND(ID_REPORT_END,		OnReportEnd)
	ON_COMMAND(ID_BEGIN_SORT,		OnBeginSort)
	ON_COMMAND(ID_END_SORT,			OnEndSort)
	ON_COMMAND(ID_BEGIN_GROUP,		OnBeginGroup)
	ON_COMMAND(ID_END_GROUP,		OnEndGroup)
	ON_COMMAND(ID_BEGIN_MEM_GROUP,	OnBeginMemGroup)
	ON_COMMAND(ID_END_MEM_GROUP,	OnEndMemGroup)
	ON_COMMAND(ID_SEARCH_TIC,		OnSearchTic)
	ON_COMMAND(ID_RETRIEVE_TIC,		OnRetrieveTic)

	ON_COMMAND(ID_VK_SHIFT_LEFT,	OnVKShiftLeft)
	ON_COMMAND(ID_VK_SHIFT_RIGHT,	OnVKShiftRight)
	ON_COMMAND(ID_VK_SHIFT_UP,		OnVKShiftUp)
	ON_COMMAND(ID_VK_SHIFT_DOWN,	OnVKShiftDown)

	ON_COMMAND(ID_VK_CTRL_LEFT,		OnVKCtrlLeft)
	ON_COMMAND(ID_VK_CTRL_RIGHT,	OnVKCtrlRight)
	ON_COMMAND(ID_VK_CTRL_UP,		OnVKCtrlUp)
	ON_COMMAND(ID_VK_CTRL_DOWN,		OnVKCtrlDown)

	//ON_COMMAND(ID_VK_TAB,			OnVKTab)
	ON_COMMAND(ID_VK_SHIFT_TAB,		OnVKShiftTab)
	//ON_COMMAND(ID_VK_RETURN,		OnVKReturn)

	ON_COMMAND(ID_SNAP_TO_GRID,		OnSnapToGrid)

	ON_COMMAND_EX(ID_TOGGLE_BORDER_ALL,		OnToggleBorder)
	ON_COMMAND_EX(ID_TOGGLE_BORDER_UP,		OnToggleBorder)
	ON_COMMAND_EX(ID_TOGGLE_BORDER_DOWN,	OnToggleBorder)
	ON_COMMAND_EX(ID_TOGGLE_BORDER_LEFT,	OnToggleBorder)
	ON_COMMAND_EX(ID_TOGGLE_BORDER_RIGHT,	OnToggleBorder)

	ON_COMMAND(ID_TOGGLE_TRANSPARENT,		OnToggleTransparent)

	ON_COMMAND_EX(ID_ALIGN_HLEFT,			OnAlignmentBar)
	ON_COMMAND_EX(ID_ALIGN_HRIGHT,			OnAlignmentBar)
	ON_COMMAND_EX(ID_ALIGN_HSPACE_EQUAL,	OnAlignmentBar)
	ON_COMMAND_EX(ID_ALIGN_VTOP,			OnAlignmentBar)
	ON_COMMAND_EX(ID_ALIGN_VBOTTOM,			OnAlignmentBar)
	ON_COMMAND_EX(ID_ALIGN_VSPACE_EQUAL,	OnAlignmentBar)
	ON_COMMAND_EX(ID_ALIGN_STACK_LEFT,		OnAlignmentBar)
	ON_COMMAND_EX(ID_ALIGN_STACK_RIGHT,		OnAlignmentBar)
	ON_COMMAND_EX(ID_ALIGN_STACK_TOP,		OnAlignmentBar)
	ON_COMMAND_EX(ID_ALIGN_STACK_BOTTOM,	OnAlignmentBar)
	ON_COMMAND_EX(ID_ALIGN_CUT_H_LEFT,		OnAlignmentBar)
	ON_COMMAND_EX(ID_ALIGN_CUT_H_RIGHT,		OnAlignmentBar)
	ON_COMMAND_EX(ID_ALIGN_CUT_V_TOP,		OnAlignmentBar)
	ON_COMMAND_EX(ID_ALIGN_CUT_V_BOTTOM,	OnAlignmentBar)
	ON_COMMAND_EX(ID_LAST_LARGE,			OnAlignmentBar)
	ON_COMMAND_EX(ID_LAST_HIGH,				OnAlignmentBar)

	ON_COMMAND(ID_WOORM_TAB_SWITCH, OnTabSwitch)
	ON_COMMAND_RANGE(ID_WOORM_TAB_SWITCH_MENU_START, (UINT)(ID_WOORM_TAB_SWITCH_MENU_END), OnTabSwitchRange)

	ON_UPDATE_COMMAND_UI(ID_WOORM_TAB_SWITCH, OnUpdateTabSwitch)
	
	ON_UPDATE_COMMAND_UI(ID_ALLOW_EDITING,	OnUpdateAllowEditingButton)
	ON_UPDATE_COMMAND_UI(ID_SHOW_EDIT_TOOLBARS,	OnUpdateShowEditToolbars)
	ON_UPDATE_COMMAND_UI(ID_APP_NEW_REPORT,  		OnUpdateFileNew)
	ON_UPDATE_COMMAND_UI(ID_APP_OPEN_REPORT,		OnUpdateFileOpen)

	ON_UPDATE_COMMAND_UI(ID_FILE_SAVE,  	 OnUpdateFileSave)
	ON_UPDATE_COMMAND_UI(ID_FILE_SAVE_2,	 OnUpdateFileSave)
	ON_UPDATE_COMMAND_UI(ID_FILE_SAVE_AS,	 OnUpdateFileSave)
	ON_UPDATE_COMMAND_UI(ID_FILE_SAVE_RDE,   OnUpdateFileSaveRDE)
	ON_UPDATE_COMMAND_UI(ID_FILE_SAVE_PDF,	 OnUpdateFileSavePDF)
	ON_UPDATE_COMMAND_UI(ID_RUN_AND_ATTACH_PDF_CHILDS,	 OnUpdateSendMail)

	ON_UPDATE_COMMAND_UI(ID_FILE_PAGE_SETUP,		OnUpdatePageSetup)
	ON_UPDATE_COMMAND_UI(ID_FILE_PROPERTIES,		OnUpdateFileProperties)	

	ON_UPDATE_COMMAND_UI(ID_SAVE_ONLY_GRAPH_INFO,   OnUpdateSaveOnlyGraphInfo)

	ON_UPDATE_COMMAND_UI(ID_OBJECT_COPY,			OnUpdateCopy)
	ON_UPDATE_COMMAND_UI(ID_OBJECT_PASTE,			OnUpdatePaste)
	ON_UPDATE_COMMAND_UI(ID_OBJECT_UNDO,			OnUpdateUndo)
	ON_UPDATE_COMMAND_UI(ID_CLEAR_ALL,				OnUpdateClearAll)

	ON_UPDATE_COMMAND_UI(ID_RUN,             OnUpdateRun)
	ON_UPDATE_COMMAND_UI(ID_STOP,            OnUpdateStop)
	ON_UPDATE_COMMAND_UI(ID_COMPILE,         OnUpdateRun)
	ON_UPDATE_COMMAND_UI(ID_PAUSE,           OnUpdatePause)
	ON_UPDATE_COMMAND_UI(ID_RESUME,          OnUpdateResume)
	ON_UPDATE_COMMAND_UI(ID_RUN_STOP,        OnUpdateRunStop)
	ON_UPDATE_COMMAND_UI(ID_RUN_STOP_2,		 OnUpdateRunStop)
	ON_UPDATE_COMMAND_UI(ID_RS_RUN_DEBUGGER, OnUpdateRunStop)
	ON_UPDATE_COMMAND_UI(ID_PAUSE_RESUME,    OnUpdatePauseResume)

	ON_UPDATE_COMMAND_UI(ID_GOTO_PAGE,       OnUpdateGotoPage)

	ON_UPDATE_COMMAND_UI(ID_PG_UP,   		 OnUpdateMovePage)
	ON_UPDATE_COMMAND_UI(ID_PG_DN,   		 OnUpdateMovePage)
	ON_UPDATE_COMMAND_UI(ID_PG_HOME, 		 OnUpdateMovePage)
	ON_UPDATE_COMMAND_UI(ID_PG_END,  		 OnUpdateMovePage)
	ON_UPDATE_COMMAND_UI(ID_FILE_PRINT_PREVIEW, OnUpdateFilePreviewPrint)
	ON_UPDATE_COMMAND_UI(ID_ADD_SQR_RECT,    OnUpdateAddRect)
	ON_UPDATE_COMMAND_UI(ID_ADD_TEXT_RECT,   OnUpdateAddRect)
	ON_UPDATE_COMMAND_UI(ID_ADD_GRAPH_RECT,  OnUpdateAddRect)
	ON_UPDATE_COMMAND_UI(ID_ADD_FILE_RECT,   OnUpdateAddRect)
	ON_UPDATE_COMMAND_UI(ID_ADD_REPEATER,    OnUpdateAddRect)

	ON_UPDATE_COMMAND_UI(ID_RS_ADD_LINK,	 OnUpdateAddLink)

	ON_UPDATE_COMMAND_UI(ID_COL_LMOVE,       OnUpdateColMove)
	ON_UPDATE_COMMAND_UI(ID_COL_RMOVE,       OnUpdateColMove)

	ON_UPDATE_COMMAND_UI(ID_SELECT_ALL,      OnUpdateSelectAll)
	ON_UPDATE_COMMAND_UI(ID_UNSELECT_ALL,    OnUpdateSelection)
	ON_UPDATE_COMMAND_UI(ID_MULSEL_ALIGN,    OnUpdateSelection)
	ON_UPDATE_COMMAND_UI(ID_MULSEL_SIZE,     OnUpdateSelection)

	ON_UPDATE_COMMAND_UI(ID_OPTIONS,         OnUpdateOptions)
	ON_UPDATE_COMMAND_UI(ID_WOORM_INI,       OnUpdateWoormOptions)

	ON_UPDATE_COMMAND_UI(ID_FONT_STYLES,     OnUpdateEditMode)
	ON_UPDATE_COMMAND_UI(ID_FORMAT_STYLES,   OnUpdateEditMode)

	ON_UPDATE_COMMAND_UI(ID_REPORT_PARAMETER,		OnUpdateShowReportParameters)	
	ON_UPDATE_COMMAND_UI(ID_INDICATOR_TRACK_RECT,	OnUpdateTrackRectIndicator)

	ON_UPDATE_COMMAND_UI(ID_VK_SHIFT_LEFT,	OnUpdateVKSize)
	ON_UPDATE_COMMAND_UI(ID_VK_SHIFT_RIGHT,	OnUpdateVKSize)
	ON_UPDATE_COMMAND_UI(ID_VK_SHIFT_UP,	OnUpdateVKSize)
	ON_UPDATE_COMMAND_UI(ID_VK_SHIFT_DOWN,	OnUpdateVKSize)

	ON_UPDATE_COMMAND_UI(ID_VK_CTRL_LEFT,	OnUpdateVKSize)
	ON_UPDATE_COMMAND_UI(ID_VK_CTRL_RIGHT,	OnUpdateVKSize)
	ON_UPDATE_COMMAND_UI(ID_VK_CTRL_UP,		OnUpdateVKSize)
	ON_UPDATE_COMMAND_UI(ID_VK_CTRL_DOWN,	OnUpdateVKSize)

	ON_UPDATE_COMMAND_UI(ID_TOGGLE_BORDER_ALL,		OnUpdateToggleBorder)
	ON_UPDATE_COMMAND_UI(ID_TOGGLE_BORDER_UP,		OnUpdateToggleBorder)
	ON_UPDATE_COMMAND_UI(ID_TOGGLE_BORDER_UP,		OnUpdateToggleBorder)
	ON_UPDATE_COMMAND_UI(ID_TOGGLE_BORDER_DOWN,		OnUpdateToggleBorder)
	ON_UPDATE_COMMAND_UI(ID_TOGGLE_BORDER_LEFT,		OnUpdateToggleBorder)
	ON_UPDATE_COMMAND_UI(ID_TOGGLE_BORDER_RIGHT,	OnUpdateToggleBorder)

	ON_UPDATE_COMMAND_UI(ID_SNAP_TO_GRID,			OnUpdateSnapToGrid)
	ON_UPDATE_COMMAND_UI(ID_TOGGLE_TRANSPARENT,		OnUpdateToggleTransparent)

	//ON_UPDATE_COMMAND_UI(ID_ALIGN_HLEFT,		OnUpdateAlignmentBar)
	ON_UPDATE_COMMAND_UI(ID_ALIGN_HRIGHT, OnUpdateAlignmentBar)
	ON_UPDATE_COMMAND_UI(ID_ALIGN_HSPACE_EQUAL, OnUpdateAlignmentBar)
	ON_UPDATE_COMMAND_UI(ID_ALIGN_VTOP, OnUpdateAlignmentBar)
	ON_UPDATE_COMMAND_UI(ID_ALIGN_VBOTTOM, OnUpdateAlignmentBar)
	ON_UPDATE_COMMAND_UI(ID_ALIGN_VSPACE_EQUAL, OnUpdateAlignmentBar)
	ON_UPDATE_COMMAND_UI(ID_ALIGN_STACK_LEFT, OnUpdateAlignmentBar)
	ON_UPDATE_COMMAND_UI(ID_ALIGN_STACK_RIGHT, OnUpdateAlignmentBar)
	ON_UPDATE_COMMAND_UI(ID_ALIGN_STACK_TOP, OnUpdateAlignmentBar)
	ON_UPDATE_COMMAND_UI(ID_ALIGN_STACK_BOTTOM, OnUpdateAlignmentBar)
	ON_UPDATE_COMMAND_UI(ID_ALIGN_CUT_H_LEFT, OnUpdateAlignmentBar)
	ON_UPDATE_COMMAND_UI(ID_ALIGN_CUT_H_RIGHT, OnUpdateAlignmentBar)
	ON_UPDATE_COMMAND_UI(ID_ALIGN_CUT_V_TOP, OnUpdateAlignmentBar)
	ON_UPDATE_COMMAND_UI(ID_ALIGN_CUT_V_BOTTOM, OnUpdateAlignmentBar)
	ON_UPDATE_COMMAND_UI(ID_LAST_LARGE, OnUpdateAlignmentBar)
	ON_UPDATE_COMMAND_UI(ID_LAST_HIGH, OnUpdateAlignmentBar)

	ON_COMMAND_RANGE((UINT)(ID_EXPORT_DATA_BEGIN), (UINT)(ID_EXPORT_DATA_END), OnExportData)
	ON_UPDATE_COMMAND_UI_RANGE((UINT)(ID_EXPORT_DATA_BEGIN), (UINT)(ID_EXPORT_DATA_END), OnUpdateExportData)

	ON_UPDATE_COMMAND_UI(ID_EXPORT_DATA, OnUpdateExportDropDown)

	ON_COMMAND(ID_USE_PREV_ASK_INPUT, OnUsePrevAskInput)
	ON_UPDATE_COMMAND_UI(ID_USE_PREV_ASK_INPUT, OnUpdateUsePrevAskInput)

	ON_COMMAND(ID_WMR_RERUN_STARTSTOP, OnReRunStartStop)
	ON_UPDATE_COMMAND_UI(ID_WMR_RERUN_STARTSTOP, OnUpdateReRunStartStop)
	
	ON_UPDATE_COMMAND_UI(ID_WMR_COMBO_TIME_RERUN, OnUpdateComboReRun)

	ON_COMMAND(ID_SEND_MAIL, OnSendMail)
	ON_COMMAND(ID_SEND_POSTALITE, OnSendPostaLite)
	ON_UPDATE_COMMAND_UI(ID_SEND_MAIL, OnUpdateSendMail)
	ON_UPDATE_COMMAND_UI(ID_SEND_POSTALITE, OnUpdateSendMail)

	ON_COMMAND(ID_WRM_EASY_READING_BYCOLOR, OnEasyReadingByColor)
	ON_COMMAND(ID_WRM_EASY_READING_BYCOLOR_DYNAMIC, OnEasyReadingByColorDynamic)
	ON_COMMAND(ID_WRM_EASY_READING_BYLINE, OnEasyReadingByLine)
	ON_COMMAND(ID_WRM_EASY_READING_BYLINE_DYNAMIC, OnEasyReadingByLineDynamic)

	ON_UPDATE_COMMAND_UI(ID_WRM_EASY_READING,					OnUpdateEasyReading)//pulsante
	ON_UPDATE_COMMAND_UI(ID_WRM_EASY_READING_BYCOLOR,			OnUpdateEasyReading)//voci di menu della dropdown
	ON_UPDATE_COMMAND_UI(ID_WRM_EASY_READING_BYCOLOR_DYNAMIC,	OnUpdateEasyReading)//			""
	ON_UPDATE_COMMAND_UI(ID_WRM_EASY_READING_BYLINE,			OnUpdateEasyReading)//			""
	ON_UPDATE_COMMAND_UI(ID_WRM_EASY_READING_BYLINE_DYNAMIC,	OnUpdateEasyReading)//			""

	ON_COMMAND(ID_REC_VALIDATION,					OnRecordValidation) //used only with MacroRecorder extensions
	ON_UPDATE_COMMAND_UI(ID_REC_VALIDATION,			OnUpdateRecordValidation)

	ON_COMMAND(ID_COPY_TEXT, OnCopyText)
	ON_COMMAND(ID_TOGGLE_CONCAT_PDF, OnToggleConcatPdf)

	ON_COMMAND(ID_OPEN_AS_TEXT, OnOpenAsText)
	ON_UPDATE_COMMAND_UI(ID_OPEN_AS_TEXT, OnUpdateOpenAsText)

	ON_COMMAND(ID_ADD_EA_BARCODE, OnAddEABarcode)
	ON_UPDATE_COMMAND_UI(ID_ADD_EA_BARCODE, OnUpdateAddEABarcode)
	
	//----	TEMPLATE
	ON_COMMAND(ID_FILE_SAVE_AS_TEMPLATE, OnFileSaveAsTemplate)
	ON_UPDATE_COMMAND_UI(ID_FILE_SAVE_AS_TEMPLATE, OnUpdateFileSave)

	ON_COMMAND(ID_OPEN_TEMPLATE, LoadTemplate)	//utilizzato in fase di aperture di un report con template

	ON_COMMAND(ID_CLEAR_ALL_CUSTOM_STYLE, OnClearCustomStyles)
	ON_UPDATE_COMMAND_UI(ID_CLEAR_ALL_CUSTOM_STYLE, OnUpdateClearCustomStyles)
	
	ON_COMMAND(ID_LOAD_TEMPLATE, OnLoadTemplate)
	ON_COMMAND(ID_UNLOAD_TEMPLATE, OnUnloadTemplate)

	ON_UPDATE_COMMAND_UI(ID_LOAD_TEMPLATE, OnUpdateLoadTemplate)
	ON_UPDATE_COMMAND_UI(ID_UNLOAD_TEMPLATE, OnUpdateUnloadTemplate)

	ON_CBN_SELCHANGE(IDC_DROPDOWN_TEMPLATES, OnComboTemplatesSelChanged)

	//----	LAYOUTS
		//ON_COMMAND(ID_LAYOUT_RENAME,					OnRenameLayout)
		//ON_UPDATE_COMMAND_UI(ID_LAYOUT_RENAME,			OnUpdateRenameLayout)

	ON_COMMAND(ID_LAYOUT_CLONE_OBJECT, OnLayoutCopyObject)
	ON_COMMAND(ID_LAYOUT_MOVE_OBJECT, OnLayoutMoveObject)

	ON_UPDATE_COMMAND_UI(ID_LAYOUT_MOVE_OBJECT,		OnUpdateLayoutCopyMoveObject)
	ON_UPDATE_COMMAND_UI(ID_LAYOUT_CLONE_OBJECT,	OnUpdateLayoutCopyMoveObject)

	ON_COMMAND(ID_RS_ADD_LINK, OnAddLink)

	ON_UPDATE_COMMAND_UI(ID_EXTDOC_SWITCHTO, OnUpdateSwitchTo)
	ON_UPDATE_COMMAND_UI(ID_EXTDOC_BACKTOMENU, OnUpdateBackToMenu)

	ON_COMMAND(ID_EXTDOC_SWITCHTO, OnSwitchTo)
	ON_COMMAND(ID_EXTDOC_BACKTOMENU, OnBackToMenu)

	ON_UPDATE_COMMAND_UI(ID_ACTIONS_COPY, OnUpdateActionsCopy)
	ON_COMMAND(ID_ACTIONS_COPY, OnActionsCopy)
	ON_COMMAND(ID_ACTIONS_REP_COPY, OnActionsCopy)
	ON_UPDATE_COMMAND_UI(ID_ACTIONS_REP_COPY, OnUpdateActionsCopy)
	ON_COMMAND(ID_ACTIONS_REP_XML_COPY, OnXmlActionsCopy)
	ON_UPDATE_COMMAND_UI(ID_ACTIONS_REP_XML_COPY, OnUpdateActionsCopy)

END_MESSAGE_MAP()

/////////////////////////////////////////////////////////////////////////////
// CWoormDocMng construction/destruction

//-----------------------------------------------------------------------------
CWoormDocMng::CWoormDocMng()
	:
	m_nRelease				(WOORM_RELEASE),
	m_nReportRelease		(WOORM_RELEASE),
	m_nModifyRelease		(0),
	m_pDataDefaults			(NULL),
	m_bReportChanged		(FALSE),
	m_bViewChanged			(FALSE),
	m_bPlayback				(FALSE),
	m_bOnlyGraphInfo		(FALSE),
	m_bPrintingOnDefault	(FALSE),
	m_pCurrentObj			(NULL),
	m_pMultipleSelObj		(NULL),
	m_pMultiColumns			(NULL),
	m_pNodesSelection		(NULL),
	m_pDragRectObj			(NULL),
	m_Creating				(NONE),
	m_wCreatingId			(0),
	m_pOldWoormInfo			(NULL),
	m_pChildReport			(NULL),
	m_pExportData			(NULL),
	m_pCurrentFont			(NULL),
	m_pOldFont				(NULL),
	m_pDocProperties		(NULL),
	m_bAscendingOrderByColumn (TRUE),
	m_pWaitingSubReport		(NULL),
	m_bUsePrevAskInput		(TRUE),
	m_bReRun				(FALSE),
	m_bRunWithOutChild		(FALSE),
	m_bMustReRun			(FALSE),
	m_bMultiCopies			(FALSE),
	m_bCloseChildOnEndPrint (FALSE),
	m_bAttachReportChild	(FALSE),
	m_bDirectAttachRDE		(FALSE),
	m_bDirectAttachPDF		(FALSE),
	m_bDirectCompressAttach	(FALSE),
	m_bDirectSendMail		(FALSE),
	m_bDirectConcatPDF		(FALSE),
	m_bReportComputing		(FALSE),
	m_bIsNew				(FALSE),
	m_bIsInParse			(FALSE),
	m_bNoWeb				(FALSE),
	m_bSaveAsWithCurrentLanguage (FALSE),
	m_bNeedFinalInvalidate	(FALSE),
	m_bReplaceHiddenWhenExpr(FALSE),
	m_FindDlg				(NULL),
	m_nTimerID				(0),
	m_pFullLayoutSplitted	(NULL),
	m_nCurrentSplittedPage	(-1),
	m_nSplittedPageMaxWidth	(0),
	m_pUndoStack			(new Stack ()),
	m_Objects				(NULL),
	m_Layouts				(this),
	m_idcCounter			(29000),
	m_pHiddenIcon			(NULL),
	m_pHiddenWhenIcon		(NULL),
	m_pAnchorIcon			(NULL),
	m_pAnchorLeftIcon		(NULL),
	m_pMailIcon				(NULL),
	m_pLockedIcon			(NULL),
	m_pRepeaterIcon			(NULL),
	m_pRepeaterFieldIcon	(NULL),
	m_pSplitterIcon			(NULL),
	m_pPinnedIcon			(NULL),
	m_bCultureVariable		(FALSE),
	m_iFindCondition		(ECompareType::CMP_CONTAINS),
	m_bLoadingTemplate		(FALSE),
	m_nTimerAutoSave		(0),
	m_nReRunTimer			(0),
	m_nReRun				(0),
	m_pFormatStyles			(NULL)
{   
	ASSERT(AfxGetBaseApp()->GetTaskBuilderAddOnApp()->m_pAddOnModules);
	MailConnectorParams* params = AfxGetIMailConnector()->GetParams();

	m_bDirectCompressAttach = params->GetMailCompress();

	// statusbar indicators toggle and data
	m_bRunIndicatorActive  = FALSE;
	pchSortStatusString[0] = BLANK_CHAR;
	pchSortStatusString[1] = BLANK_CHAR;
	pchSortStatusString[2] = BLANK_CHAR;
	pchSortStatusString[3] = NULL_CHAR;

	m_bTrackRectIndicatorActive= FALSE;
	m_bTrackRowsIndicatorActive= FALSE;
	m_TrackRect.SetRectEmpty();
	m_wTrackRows = 0;

	// reset indicators counter;
	m_nRetrieveTic = 0;
	m_nSearchTic   = 0;
	
	// create dynamic object
	m_pDataDefaults		= new DataDefaults		();
	m_pExportData		= new CExportData		(this);
	m_pOptions			= new Options   		(this);
	m_pWoormIni			= new WoormIni  		();
	
	//Set Culture
	m_pFormatStyles = new FormatStyleTable(*AfxGetFormatStyleTable());
	//SetCulture(_T("it-IT")); // x Riccardo
	
	m_pFontStyles		= new FontStyleTable    (*AfxGetFontStyleTable());
	if(m_pWoormIni->m_bEnableNewObjectSelection)
		m_pActiveRect = new NewActiveRect(m_pWoormIni,TRUE);
	else
		m_pActiveRect = new OldActiveRect();

	m_pEngine			= new ReportEngine		(this);
	m_pEditorManager	= new EditorManager     (m_pEngine, m_pWoormIni->m_bShowReportTree);
	m_pRDEmanager		= new RDEManager		(m_pEngine);
	m_pInvalidAreas		= new CInvalidArray		();
	m_pDocProperties	= new CDocProperties	(this);	

	GetInfoOSL()->SetType(OSLType_Report);

	m_sStartUICulture = __super::GetUICulture();
	SetCultureStrings (m_sStartUICulture);	
	
	//---------------
	m_Objects = m_Layouts.Add(REPORT_DEFAULT_LAYOUT_NAME);
	//---------------
	m_bIsUserReportsDeveloper = IsUserReportsDeveloper();

	DataObj* pSetting = AfxGetSettingValue(snsTbWoormViewer, szWoormRunningOptions, L"ShowRSBetaFeatures", DataBool(FALSE), szTbWoormViewerSettingFileName);
	m_bBetaFeatures = pSetting ? *((DataBool*)pSetting) : FALSE;
}

//-----------------------------------------------------------------------------
void CWoormDocMng::SetCulture(CString strLanguage)
{
	CCultureInfo *pInfo = const_cast<CCultureInfo*>(AfxGetCultureInfo());
	pInfo->InitLocale(strLanguage);
	ASSERT(pInfo);

	SAFE_DELETE(m_pFormatStyles);
	m_pFormatStyles = new FormatStyleTable(*AfxGetFormatStyleTable());
}

//-----------------------------------------------------------------------------
void CWoormDocMng::RestoreCulture()
{
	const CLoginInfos *pInfos = AfxGetLoginInfos();
	CCultureInfo *pInfo = const_cast<CCultureInfo*>(AfxGetCultureInfo());
	ASSERT(pInfos);
	ASSERT(pInfo);
	pInfo->InitLocale(pInfos->m_strApplicationLanguage);
}

//-----------------------------------------------------------------------------
CString CWoormDocMng::GetReportNamespace() const
{
	CString s = const_cast<CWoormDocMng*>(this)->GetInfoOSL()->m_Namespace.ToUnparsedString();
	int i = s.ReverseFind('.');
	return i > 0 ? s.Left(i) : L"";
}

//-----------------------------------------------------------------------------
BOOL IsEqualFont(const LOGFONT& pLogFont1, const LOGFONT& pLogFont2)
{
	return (
		pLogFont1.lfHeight			== pLogFont2.lfHeight			&&
		pLogFont1.lfWidth			== pLogFont2.lfWidth			&&
		pLogFont1.lfWeight			== pLogFont2.lfWeight			&&
		pLogFont1.lfItalic			== pLogFont2.lfItalic			&&
		_tcscmp(pLogFont1.lfFaceName, pLogFont2.lfFaceName) == 0	&&
		pLogFont1.lfUnderline		== pLogFont2.lfUnderline		&&
		pLogFont1.lfEscapement		== pLogFont2.lfEscapement		&&
		pLogFont1.lfOrientation		== pLogFont2.lfOrientation		&&
		pLogFont1.lfStrikeOut		== pLogFont2.lfStrikeOut		&&
		pLogFont1.lfCharSet			== pLogFont2.lfCharSet			&&
		pLogFont1.lfOutPrecision	== pLogFont2.lfOutPrecision		&&
		pLogFont1.lfClipPrecision	== pLogFont2.lfClipPrecision	&&
		pLogFont1.lfQuality			== pLogFont2.lfQuality			&&
		pLogFont1.lfPitchAndFamily	== pLogFont2.lfPitchAndFamily
			);
}

//-----------------------------------------------------------------------------
CFont* CWoormDocMng::SetCurrentFont(LOGFONT* pLogFont, CDC& DC)
{
	ASSERT((m_pOldFont == NULL && m_pCurrentFont == NULL ) || m_pOldFont != m_pCurrentFont);

	LOGFONT lf = *pLogFont;
	CString sFaceName =  AfxGetFontAliasTable()->LookupFaceName(pLogFont->lfFaceName);
	if (!sFaceName.IsEmpty())
		TB_TCSCPY(lf.lfFaceName, (LPCTSTR)sFaceName);

	if	(
			m_pCurrentFont &&																																					
			IsEqualFont(m_LogFont, lf)
		)
		return m_pCurrentFont;

	CFont* pFont = new CFont();
	if (pFont->CreateFontIndirect(&lf))
	{
		CFont* pF = DC.SelectObject(pFont);
		if (pF)
		{
			LOGFONT lfold; 
			pF->GetLogFont(&lfold);

			CFont* pOldFont = new CFont();
			if (pOldFont->CreateFontIndirect(&lfold))
			{
				if (m_pOldFont)
				{
					VERIFY(m_pOldFont->DeleteObject());
					delete m_pOldFont;
				}
				m_pOldFont = pOldFont;
			}
			else
			{
				ASSERT(FALSE);
				delete pOldFont;
			}
		}
		if (m_pCurrentFont)
		{
			VERIFY(m_pCurrentFont->DeleteObject());
			delete m_pCurrentFont;
		}
		m_pCurrentFont = pFont;
		m_LogFont = lf;
	}
	else
	{
		ASSERT(FALSE);
		delete pFont;
	}
	return m_pCurrentFont;
}

// ripristino il font evetualmente modificato da qualche oggetto
// contenente del testo e cancella quello usato per creare il font voluto
//-----------------------------------------------------------------------------
void CWoormDocMng::ResetCurrentFont(CDC& DC)
{
	ASSERT((m_pOldFont == NULL && m_pCurrentFont == NULL ) || m_pOldFont != m_pCurrentFont);
	if (m_pOldFont)
	{
		ASSERT (m_pOldFont->GetSafeHandle());
		DC.SelectObject(m_pOldFont);

		if (m_pCurrentFont)
		{
			VERIFY(m_pCurrentFont->DeleteObject());
			delete m_pCurrentFont;
			m_pCurrentFont = NULL;
		}
		//m_pCurrentFont = m_pOldFont; 
		//m_pCurrentFont->GetLogFont(&m_LogFont);
		//m_pOldFont = NULL;
		if (m_pOldFont)
		{
			VERIFY(m_pOldFont->DeleteObject());
			delete m_pOldFont;
			m_pOldFont = NULL;
		}
	}
}

//-----------------------------------------------------------------------------
CWoormDocMng::~CWoormDocMng()
{
	ASSERT_VALID(this);

	SAFE_DELETE(m_pDataDefaults);
	SAFE_DELETE(m_pExportData);
	SAFE_DELETE(m_pOptions);	
	SAFE_DELETE(m_pWoormIni);
	SAFE_DELETE(m_pFontStyles);
	SAFE_DELETE(m_pFormatStyles);
	SAFE_DELETE(m_pActiveRect);
	SAFE_DELETE(m_pUndoStack);
	SAFE_DELETE(m_pEngine);
	SAFE_DELETE(m_pRDEmanager);
	SAFE_DELETE(m_pInvalidAreas);
	SAFE_DELETE(m_pEditorManager);
	SAFE_DELETE(m_pDocProperties);

	m_RemovedObj.RemoveAll();

	m_Layouts.RemoveAll();

	if (m_FindDlg)
	{
		m_FindDlg->DestroyWindow();
		m_FindDlg = NULL;
	}
	// nel caso venga chiuso senza che il run sia andato a buon fine
	// ed il WoormInfo appartenga al report
	if (m_pWaitingSubReport)
	{
		AfxGetTbCmdManager()->CloseWoormReport(m_pWaitingSubReport);
		m_pWaitingSubReport = NULL;
	}
	if (m_pWoormInfo && m_pWoormInfo->m_bOwnedByReport)
	{
		if (m_pWoormInfo->m_ReportNames.GetSize() == 1)
		{
			SAFE_DELETE(m_pWoormInfo);
		}
		m_pWoormInfo = NULL;
	}
	//E' sicuramente di altri
	//if (m_pOldWoormInfo && m_pOldWoormInfo->m_bOwnedByReport)
	//	SAFE_DELETE(m_pOldWoormInfo);

	//@@TODO Solo se é un proprio contesto
	SAFE_DELETE(m_pTbContext);
	SAFE_DELETE(m_pMessages);
	SAFE_DELETE(m_pOldFont);
	SAFE_DELETE(m_pCurrentFont);
	
	//----
	SAFE_DELETE(m_pHiddenIcon)
	SAFE_DELETE(m_pHiddenWhenIcon)
	SAFE_DELETE(m_pAnchorIcon)
	SAFE_DELETE(m_pAnchorLeftIcon)
	SAFE_DELETE(m_pMailIcon)
	SAFE_DELETE(m_pLockedIcon)
	SAFE_DELETE(m_pRepeaterIcon)
	SAFE_DELETE(m_pRepeaterFieldIcon)
	SAFE_DELETE(m_pSplitterIcon)
	SAFE_DELETE(m_pPinnedIcon)
	//----
	ASSERT(_CrtCheckMemory());

	//RestoreCulture();
}

// cleanup degli strumenti del frame
//------------------------------------------------------------------------------
void CWoormDocMng::PreCloseFrame(CFrameWnd* pFrameArg)
{
	CWoormFrame* pWF = dynamic_cast<CWoormFrame*>(pFrameArg);
	if (pWF)
		pWF->SaveSettingsAndCleanUp(FALSE);

	// standard actions
	__super::PreCloseFrame(pFrameArg);
} 

//---------------------------------------------------------------------------
CTBPicture* CWoormDocMng::LoadBmp	(int idb, CDC& DC)
{
	CTBPicture* pIcon = new CTBPicture();
	HDC hDC = DC.GetSafeHdc();
	pIcon->LoadBitmapFromResource(idb, hDC);
	return pIcon;
}
//--------------

CTBPicture*		CWoormDocMng::GetHiddenIcon(CDC& DC)
{
	if (!m_pHiddenIcon)
		m_pHiddenIcon = LoadBmp (IDB_HIDDEN_FIELD, DC);
	return m_pHiddenIcon;
}

CTBPicture*		CWoormDocMng::GetHiddenWhenIcon(CDC& DC)
{
	if (!m_pHiddenWhenIcon)
		m_pHiddenWhenIcon = LoadBmp (IDB_HIDDEN_FIELD_EXPR, DC);
	return m_pHiddenWhenIcon;
}

CTBPicture*		CWoormDocMng::GetAnchorIcon(CDC& DC)
{
	if (!m_pAnchorIcon)
		m_pAnchorIcon = LoadBmp (IDB_ANCHOR, DC);
	return m_pAnchorIcon;
}

CTBPicture*		CWoormDocMng::GetAnchorLeftIcon(CDC& DC)
{
	if (!m_pAnchorLeftIcon)
		m_pAnchorLeftIcon = LoadBmp (IDB_ANCHOR_LEFT, DC);
	return m_pAnchorLeftIcon;
}

CTBPicture*		CWoormDocMng::GetMailIcon(CDC& DC)
{
	if (!m_pMailIcon)
		m_pMailIcon = LoadBmp (IDB_MAIL_FIELD, DC);
	return m_pMailIcon;
}

CTBPicture*		CWoormDocMng::GetLockedIcon(CDC& DC)
{
	if (!m_pLockedIcon)
		m_pLockedIcon = LoadBmp (IDB_LOCKED, DC);
	return m_pLockedIcon;
}

CTBPicture*		CWoormDocMng::GetRepeaterIcon(CDC& DC)
{
	if (!m_pRepeaterIcon)
		m_pRepeaterIcon = LoadBmp (IDB_REPEATER, DC);
	return m_pRepeaterIcon;
}

CTBPicture*		CWoormDocMng::GetRepeaterFieldIcon(CDC& DC)
{
	if (!m_pRepeaterFieldIcon)
		m_pRepeaterFieldIcon = LoadBmp (IDB_REPEATER_FIELD, DC);
	return m_pRepeaterFieldIcon;
}

CTBPicture*		CWoormDocMng::GetSplitterIcon(CDC& DC)
{
	if (!m_pSplitterIcon)
		m_pSplitterIcon = LoadBmp (IDB_COLUMN_SPLITTER, DC);
	return m_pSplitterIcon;
}

CTBPicture*		CWoormDocMng::GetPinnedIcon(CDC& DC)
{
	if (!m_pPinnedIcon)
		m_pPinnedIcon = LoadBmp (IDB_COLUMN_PINNED, DC);
	return m_pPinnedIcon;
}

//---------------------------------------------------------------------------
// ordina gli oggetti in ordine di posizione; prima le righe e poi le 
// colonne per poter minimizzare i problemi sulle stampanti ad aghi
// e regola di sovrapposizione deterministica

void CWoormDocMng::SortLayoutObjectsOnPosition()
{
	if (m_pWoormIni->m_bSortObjects)
		m_Layouts.EnumObjects(&CWoormDocMng::SortObjectsOnPosition);
}

void CWoormDocMng::SortObjectsOnPosition()
{
	SortObjectsOnPosition(&GetObjects()); 
}

void CWoormDocMng::SortObjectsOnPosition(CLayout* pObjects)
{
	// evita di sortare se non ci sono almeno due elementi
	if (pObjects->GetSize() <= pObjects->m_nCountAutoObjects)
		return;
	
	//volutamente uso il BubbleSort a causa delle logiche nella funzione di ordinamento
	//(Z-Order relativo dovuto al Trasparent)
	VERIFY(pObjects->BubbleSort());	
	//VERIFY(pObjects->QuickSort());
}

void CWoormDocMng::ApplyRepeater(CLayout* pObjects)
{
	if (pObjects->GetSize() <= pObjects->m_nCountAutoObjects)
		return;
	
	pObjects->ApplyRepeater();
}

//------------------------------------------------------------------------------
//	Attach frame window handle for comunication purpose with m_pEngine and
//	RDE manager (tic send by parser and report)
//
BOOL CWoormDocMng::InitDocument()
{                
	CLocalizableFrame* pFrame = GetFrame();

	m_pEngine		->AttachFrame		(pFrame);
	m_pRDEmanager	->AttachFrame		(pFrame);
	m_pEngine		->AttachCallerDoc	(this);
	m_pActiveRect	->Attach			(this);
	m_pEditorManager->Attach			(this);
	
	return TRUE;
}

// metodo principale che si occupa anche di caricare le DLL
//------------------------------------------------------------------------------
Formatter* CWoormDocMng::GetFormatter(const CString& strStyle)	const
{
	const CTBNamespace& ns = (const_cast < CWoormDocMng* > (this))->GetNamespace();
	Formatter* pFormatter = m_pFormatStyles->GetFormatter(strStyle, &ns);
	
	// se il formattatore si trova in una libreria, devo sovrascrivere 
	// quello dichiarato con quello implementato nella libreria
	if (pFormatter && pFormatter->GetOwner().GetType() == CTBNamespace::LIBRARY)
	{
		AfxGetTbCmdManager()->LoadNeededLibraries(pFormatter->GetOwner());
		pFormatter = m_pFormatStyles->GetFormatter(strStyle, &ns);
	}

	if (pFormatter)
		return pFormatter;
		
	pFormatter = AfxGetFormatStyleTable()->GetFormatter(strStyle, &ns);

	// se il formattatore si trova in una libreria, devo sovrascrivere 
	// quello dichiarato con quello implementato nella libreria
	if (pFormatter && pFormatter->GetOwner().GetType() == CTBNamespace::LIBRARY)
	{
		AfxGetTbCmdManager()->LoadNeededLibraries(pFormatter->GetOwner());
		pFormatter = AfxGetFormatStyleTable()->GetFormatter(strStyle, &ns);
	}
	
	return pFormatter;
}

// usa quella con nome 
//------------------------------------------------------------------------------
Formatter* CWoormDocMng::GetFormatter (const FormatIdx& nIdx) const
{
	Formatter* pFormatter = GetFormatter (m_pFormatStyles->GetStyleName(nIdx));

	if (pFormatter)
		return pFormatter;

	return GetFormatter (AfxGetFormatStyleTable()->GetStyleName(nIdx));
}

// usa quella con nome 
//------------------------------------------------------------------------------
Formatter* CWoormDocMng::GetFormatter(const DataType& aType) const
{
	return GetFormatter (FromDataTypeToFormatName(aType));
}

//------------------------------------------------------------------------------
void CWoormDocMng::UpdateStatusBar()
{                
	SetPageIndicator();
	SetRunIndicator();
}

//------------------------------------------------------------------------------
void CWoormDocMng::DeleteContents()
{                
	__super::DeleteContents();

	m_arWoormLinks.RemoveAll();
}

//------------------------------------------------------------------------------
void CWoormDocMng::InvalidateRect (const CRect& aRect, BOOL bRepaint)
{                                         
	// add Invalid rectange to all
	m_pInvalidAreas->Add(new InvalidArea(ScaleRect(aRect), bRepaint));
}

//------------------------------------------------------------------------------
void CWoormDocMng::Invalidate (BOOL bRepaint)
{
	// pHints = NULL means update all client area,  
	// lHint = TRUE means erase background
	UpdateAllViews (NULL, bRepaint);
	m_pInvalidAreas->RemoveAll();
}

//------------------------------------------------------------------------------
void CWoormDocMng::UpdateWindow ()
{
	// ignore WM_PAINT with no Invalid area
	if (m_pInvalidAreas->GetSize() == 0) 
		return;

	UpdateAllViews (NULL, FALSE, m_pInvalidAreas);
	m_pInvalidAreas->RemoveAll();
}

//------------------------------------------------------------------------------
void CWoormDocMng::CreateListObj(CDC& aDC, CBCGPScrollView* aView, BOOL bSkipTrackPoint/* = FALSE*/)
{
	m_pCurrentObj = NULL;

	// posiziona al punto della griglia minore di quello attuale
	if (m_pWoormIni->m_bSnapToGrid)
	{
		int x = m_ptCurrPos.x % m_pWoormIni->m_nGridX; 
		int y = m_ptCurrPos.y % m_pWoormIni->m_nGridY;
		
		m_ptCurrPos.Offset(-x, -y);
	}

	switch (m_Creating)
	{
	case SQRRECT    :
		m_pCurrentObj = new SqrRect(m_ptCurrPos, this);
		((SqrRect*)m_pCurrentObj)->m_BaseRect.right = ((SqrRect*)m_pCurrentObj)->m_BaseRect.right + 80;
		((SqrRect*)m_pCurrentObj)->m_BaseRect.bottom = ((SqrRect*)m_pCurrentObj)->m_BaseRect.bottom + 25;
		break;

	case TEXTRECT   :
		{
			m_pCurrentObj = new TextRect (m_ptCurrPos, this);
			TextRect* pObj = (TextRect*) m_pCurrentObj;

			/*pObj->m_BaseRect.right = pObj->m_BaseRect.right + 30;
			pObj->m_BaseRect.bottom = pObj->m_BaseRect.bottom + 5;*/

			pObj->SetText(m_strTxtName);

			m_strTextField = ""; // clean global buffer
		
			break;
		}

	case GRAPHRECT  :
		{
			m_pCurrentObj = new GraphRect  (m_ptCurrPos, m_strBmpName, this);
			m_strBmpName = "";  // clean global buffer
			break;
		}

	case METAFILERECT  :
		{
			m_pCurrentObj = new MetafileRect  (m_ptCurrPos, m_strBmpName, this);
			m_strBmpName = "";  // clean global buffer
			break;
		}
	case URLRECT  :
		{
			m_pCurrentObj = new UrlRect  (m_ptCurrPos, m_strTxtName, this);
			m_strTxtName = "";  // clean global buffer
			break;
		}

	case FILERECT   :
		{
			m_pCurrentObj = new FileRect   (m_ptCurrPos, m_strTxtName, this);
			m_strTxtName = "";  // clean global buffer
			break;
		}

	case FIELDRECT  : 
		{
			FieldRect* pFR = NULL;
			if (m_wCreatingColumnIds.GetSize() > 0 /*&& m_wCreatingId == m_wCreatingColumnIds[0]*/)
			{
				CPoint pt (m_ptCurrPos);
				for (int i = 0; i < m_wCreatingColumnIds.GetSize(); i++)
				{
					pFR = new FieldRect(pt, this, m_wCreatingColumnIds[i]);

					pFR->m_bTransparent = m_pWoormIni->m_bTransparentCreate;
					if (m_pWoormIni->m_bNoBorderCreate) 
						pFR->ToggleBorder(ID_TOGGLE_BORDER_ALL);

					ApplyTemplateStyles (pFR);
					
					GetObjects().Add (pFR);

					InvalidateRect	(pFR->GetBaseRect());

					//m_pActiveRect->SetActive(pFR->GetActiveRect());
					m_pCurrentObj = pFR;
					
					m_pCurrentObj->AddCreateUndoInfo();

					pt.y += 30;
				}
			
				InvalidateRect(m_pCurrentObj->GetBaseRect());
				//m_pActiveRect->SetActive(m_pCurrentObj->GetActiveRect());

				UpdateWindow();
				ExitCreatingMode();

				m_wCreatingColumnIds.RemoveAll();

				// allow tracking only for non table object
				if (m_pCurrentObj && !m_pCurrentObj->IsKindOf(RUNTIME_CLASS(Table)) && !bSkipTrackPoint)
				{
					// get BottomRight corner for tracking and positionate cursor	
					CPoint ptTrack = m_pCurrentObj->StartTrackPoint();

					SetCursorTrackPoint(ptTrack);
					m_pCurrentObj->LeftButtonDown(aDC, ptTrack, aView);
				}
				//ASSERT(pFR == m_pCurrentObj); m_pCurrentObj puo' diventare NULL dopo i refresh

				//if (bSkipTrackPoint)
				{
					int nAlias = pFR->GetInternalID();
					WoormField* pF = GetEditorSymTable()->GetFieldByID(nAlias);
					ASSERT_VALID(pF);
					SyncronizeViewSymbolTable(pF);
				}

				RefreshRSTree(ERefreshEditor::Variables);
				RefreshRSTree(ERefreshEditor::Layouts);

				/*if (m_pCurrentObj)
					GetWoormFrame()->SelectLayoutObject(m_pCurrentObj, !bSkipTrackPoint);
				else if (pFR)
					GetWoormFrame()->SelectLayoutObject(pFR, !bSkipTrackPoint);*/

				return;
			}
			else if (m_wCreatingId)
			{
 				m_pCurrentObj = new FieldRect(m_ptCurrPos, this, m_wCreatingId);
			}
			else ASSERT(FALSE);
		}
		break;

	case TABLE      :
	{
		int nRows = m_pEditorManager->GetTableRows(m_wCreatingId);
		if (nRows < 1)
		{
			nRows = 1;
			Message (_TB("Number of lines not valid. Use default."));
		}

		// read from ini default value
		m_pCurrentObj = new Table
			(
				m_ptCurrPos, this,
				nRows, m_wCreatingColumnIds.GetSize(),
				m_wCreatingId, m_wCreatingColumnIds
			);

		((Table*)m_pCurrentObj)->m_bHideTableTitle = TRUE;

		CPoint point;
		((Table*)m_pCurrentObj)->SetActiveColumn(m_wCreatingColumnIds.ElementAt(m_wCreatingColumnIds.GetCount()-1), point);
		break;
	}

	case REPEATER    :							
		m_pCurrentObj = new Repeater(m_ptCurrPos, this, m_wCreatingId);
		m_pCurrentObj->m_BaseRect.right += 80;
		m_pCurrentObj->m_BaseRect.bottom += 25;
		break;
	case CHART:
		m_pCurrentObj = new Chart(m_ptCurrPos, this, m_wCreatingId);
		m_pCurrentObj->m_BaseRect.right +=  80;
		m_pCurrentObj->m_BaseRect.bottom += 25;
		break;

	case BARCODE:
		m_pCurrentObj = GetObjects().FindByID(SpecialReportField::ID.EA_BARCODE);
		if (!m_pCurrentObj)
		{
			m_pCurrentObj = CreateBarcodeAtPosition(m_ptCurrPos.x, m_ptCurrPos.y, FALSE);
		}
		else if (m_pCurrentObj->IsKindOf(RUNTIME_CLASS(TextRect)))
		{
			TextRect* pT = (TextRect*)m_pCurrentObj;
			pT->SetBarCode(CBarCodeTypes::BarCodeEnum(_T("CODE39")), FALSE);
			pT->SetText(L"EA-12345678901234");
			pT->SetHideExpression(_T("Not (ReportIsArchiving && ReportCurrentPageNumber == 1)"));
			SelectRSTreeItemData(ERefreshEditor::Layouts, pT);
			return;
		}

		break;

	default:
		m_wCreatingColumnIds.RemoveAll();
		return; // error condition
	}

	// se richiesto dal .ini si creano gli oggetti trasparenti e senza bordi
	if (!m_pCurrentObj->IsKindOf(RUNTIME_CLASS(Repeater)))
		m_pCurrentObj->m_bTransparent = m_pWoormIni->m_bTransparentCreate;

	if (m_pWoormIni->m_bNoBorderCreate) 
		m_pCurrentObj->ToggleBorder(ID_TOGGLE_BORDER_ALL);

	ApplyTemplateStyles (m_pCurrentObj);

	// add object and initialize it in view area
	if (m_pCurrentObj->IsKindOf(RUNTIME_CLASS(Repeater)))
	{
		GetObjects().InsertAt (GetObjects().m_nCountAutoObjects, m_pCurrentObj);
		((Repeater*)m_pCurrentObj)->Rebuild(&GetObjects());
	}
	else
		GetObjects().Add(m_pCurrentObj);

	RefreshRSTree(ERefreshEditor::Variables);
	RefreshRSTree(ERefreshEditor::Layouts, m_pCurrentObj);

	ASSERT_VALID(m_pCurrentObj);

	//GetWoormFrame()->GetLayoutTreeView()->SelectLayoutObject(m_pCurrentObj, FALSE);

	/*InvalidateRect	(m_pCurrentObj->GetBaseRect());
	m_pActiveRect->SetActive(m_pCurrentObj->GetActiveRect());*/
	
	m_pCurrentObj->AddCreateUndoInfo();

	UpdateWindow    ();
	ExitCreatingMode();
	
	// allow tracking only for non table object
	if (!m_pCurrentObj->IsKindOf(RUNTIME_CLASS(Table)))
	{
		// get BottomRight corner for tracking and positionate cursor	
		/*CPoint ptTrack = m_pCurrentObj->StartTrackPoint();         
				
		SetCursorTrackPoint(ptTrack);
		m_pCurrentObj->LeftButtonDown(aDC, ptTrack, aView);

		GetWoormFrame()->SelectLayoutObject(m_pCurrentObj);*/
	}
	m_wCreatingColumnIds.RemoveAll();

	//seleziono la colonna della tabella appena creata "simulando il click sul suo titolo"
	Table* pTable = dynamic_cast<Table*>(m_pCurrentObj);
	if (pTable)
	{
		TableColumn* pCol = pTable->GetActiveColumn();
		if (pCol)
			SetCurrentObj(pCol->m_ColumnTitleRect.CenterPoint());
	}
}

//------------------------------------------------------------------------------
void CWoormDocMng::CreateDragRect(CDC& aDC, CBCGPScrollView* aView)
{
	DeleteDragRect();

	CPoint ptCur = m_ptCurrPos;
	ScalePoint(ptCur, aDC);

	m_pDragRectObj = new DragRect(ptCur, this);

	m_pDragRectObj->InitDraggingRect(aDC, m_pDragRectObj->m_BaseRect.BottomRight(), aView);
}

//------------------------------------------------------------------------------
void CWoormDocMng::DeleteDragRect()
{
	SAFE_DELETE (m_pDragRectObj)
}

//------------------------------------------------------------------------------
void CWoormDocMng::SetCursorTrackPoint(CPoint point)
{
	CWoormView* pView = GetWoormView();
	CClientDC dc (pView);

	pView->OnPrepareDC(&dc);
	dc.LPtoDP(&point);
	
	pView->ClientToScreen(&point);
	SetCursorPos(point.x, point.y);
}
					   
// caused by limited activation area of table object, object below table
// must Invalidate areas for right repaint.
//                                       
//------------------------------------------------------------------------------
void CWoormDocMng::InvalidateStackedObjects()
{
	if (!CurrentIsTable()) return;

	// Invalidate all areas below table	
	CRect rect;
	int ub = GetObjects().GetUpperBound();
	for (int i =0; i < ub; i++)
	{
		BaseObj* pObj = GetObjects()[i];
		rect.IntersectRect(m_pCurrentObj->GetBaseRect(), pObj->GetBaseRect());
		if (!rect.IsRectEmpty())
			InvalidateRect(rect, FALSE);
	}
}

//------------------------------------------------------------------------------
void CWoormDocMng::FindWordClear()
{
	m_Layouts.EnumObjects(&CWoormDocMng::ClearFindCellArray);
	m_findWordInfo.Clear();

	m_findWordInfo.m_eCmp = m_iFindCondition;
	UpdateWindow	();
}

//------------------------------------------------------------------------------
void CWoormDocMng::SelectForExportData(CPoint point, BOOL bSelForTitles /*= FALSE*/, BOOL bClickAsShift /*= FALSE*/)
{
	for (int i = 0; i <= GetObjects().GetUpperBound(); i++)
	{
		BaseObj* pObj = GetObjects()[i];
		if (!pObj->InMe(point))
			continue;
	
		// se l'oggetto e' validamente aggiunto rinfresco il video		
		if (m_pExportData->AddRemoveElement(pObj, point, bSelForTitles, bClickAsShift))
		{
			InvalidateRect(pObj->GetBaseRect(), FALSE);

			FindWordClear();
		}
		return;
	}

	// non ho selezionato nulla ma ho cliccato in area vuota. Allora
	// deseleziono gli oggetti selezionati se ce ne sono e rinfresco il video
	if (m_pExportData->m_pItem)
	{
		SAFE_DELETE(m_pExportData->m_pItem);

		Invalidate (FALSE);

		FindWordClear();
	}
}

//------------------------------------------------------------------------------
void CWoormDocMng::AdjustRowsExportSelection(WORD wTableID, int nRowsNumber)
{
	if (!m_pExportData->IsTableItem()||!m_pExportData->IncludeTable(wTableID))
		return;
	
	m_pExportData->AdjustRowsNumber(nRowsNumber);
}

//------------------------------------------------------------------------------
CObject* CWoormDocMng::GetSelectedTableObject(Table* pTable)
{
	//modifiche per il caricamento della property grid
	if (pTable->m_nActiveColumn == Table::NO_ACTIVE_COLUMN)
	{
		return pTable;
	}
	else if (pTable->m_nActiveRow == Table::ACTIVE_IS_TITLE)
	{
		//colonna
		return  pTable->GetActiveColumn();
	}
	else if (pTable->m_nActiveRow == Table::ACTIVE_IS_TOTAL)
	{
		//totale
		return pTable->GetTotalCell(pTable->m_nActiveColumn);
	}
	else
	{
		//cella
		return pTable->GetTableCell(pTable->m_nActiveRow, pTable->m_nActiveColumn);
	}
}

//------------------------------------------------------------------------------
BaseObj* CWoormDocMng::SetCurrentObj (CPoint point, BOOL Pop)
{
	if (!Pop && !m_pMultipleSelObj)
		GetWoormFrame()->SelectLayoutObject(NULL);

	// give to current object priority over other object 
	// Must click left mouse button olding down Ctrl key for Pop it
	if (m_pCurrentObj && m_pCurrentObj->InMe(point) && !Pop)
	{
		//----
		CObject* pCurr = m_pCurrentObj;

		if (m_pCurrentObj->IsKindOf(RUNTIME_CLASS(Table)))
			pCurr = GetSelectedTableObject(((Table*)m_pCurrentObj));

		else if (m_pCurrentObj->IsKindOf(RUNTIME_CLASS(Repeater)))
		{
			BaseObj* pObj = ((Repeater*)m_pCurrentObj)->GetMasterFieldByPosition (point);
			if (pObj) 
				pCurr = pObj;
		}

		if (pCurr && pCurr->IsKindOf(RUNTIME_CLASS(BaseObj)))
			m_pCurrentObj = (BaseObj*)pCurr;

		GetWoormFrame()->SelectLayoutObject(pCurr); //TODO LAYOUT : aggiungere layout corrente come parametro

		// set active status only if not in multiple selection
		if (!m_pMultipleSelObj && m_pActiveRect)
			if (m_pActiveRect->SetActive(m_pCurrentObj->GetActiveRect()))
			{
				InvalidateStackedObjects();
				UpdateWindow ();
			}

		return m_pCurrentObj;
	}
	
	// save previous active object
	BaseObj* pPrevObj = m_pCurrentObj;

	// search for new current object base on mouse down position
	m_pCurrentObj = NULL;
	int ub = GetObjects().GetUpperBound();
	for (int i = ub; i >= 0; i--)
	{
		BaseObj* pObj = GetObjects()[i];

		if (pObj->InMe(point))
		{
			m_pCurrentObj = pObj;
			//if (pObj != pPrevObj) 
			//	RemoveFindDialog ();

			// pack GetObjects() after current and make current last object in array
			// to make it on Top during window repaint
			/*for (int j = i + 1; j <= ub; j++)
				GetObjects()[j - 1] = GetObjects()[j];
			GetObjects()[ub] = pObj;*/
			
			// set active status only if not in multiple selection
			if (!m_pMultipleSelObj)
				if (m_pActiveRect->SetActive(m_pCurrentObj->GetActiveRect()))
				{
					InvalidateStackedObjects();
					UpdateWindow ();
				}

			//----
			CObject* pCurr = m_pCurrentObj;

			if (m_pCurrentObj->IsKindOf(RUNTIME_CLASS(Table)))
				pCurr = GetSelectedTableObject(((Table*)m_pCurrentObj));
			if(!Pop)
				GetWoormFrame()->SelectLayoutObject(pCurr); //TODO LAYOUT : aggiungere layout corrente come parametro
			
			return pPrevObj;
		}
	}

	if(!m_pCurrentObj)
		GetWoormFrame()->SelectLayoutObject(NULL);

	m_pActiveRect->Clear();
	return pPrevObj;
}

//------------------------------------------------------------------------------
BaseObj* CWoormDocMng::SetCurrentObj (int nID, CPoint& pt)
{
	BaseObj* pPrevObj = m_pCurrentObj;

	m_pCurrentObj = NULL;
	int ub = GetObjects().GetUpperBound();
	for (int i = 0; i <= ub; i++)
	{
		BaseObj* pObj = GetObjects()[i];
		if (
			pObj->GetInternalID() == nID 
			||
			(pObj->IsKindOf(RUNTIME_CLASS(Table)) && ((Table*)pObj)->SetActiveColumn(nID, pt))
			)
		{
			if (!pObj->IsKindOf(RUNTIME_CLASS(Table)))
				pt = CPoint(pObj->Left() + 1, pObj->Top() + 1);

			m_pCurrentObj = pObj;

			// pack GetObjects() after current and make current last object in array
			// to make it on Top during window repaint
			//
			for (int j = i + 1; j <= ub; j++)
				GetObjects()[j - 1] = GetObjects()[j];
			GetObjects()[ub] = pObj;
			
			// set active status only if not in multiple selection
			if (!m_pMultipleSelObj)
				if (m_pActiveRect->SetActive(m_pCurrentObj->GetActiveRect()))
				{
					InvalidateStackedObjects();
					UpdateWindow ();
				}

			return pPrevObj;
		}
	}
	//potrebbe essere un totale
	if (SetTotalRectActiveById(nID))
		return pPrevObj;

	m_pActiveRect->Clear();
	return pPrevObj;
}

//------------------------------------------------------------------------------
BOOL CWoormDocMng::SetTotalRectActiveById(int nID)
{
	int ub = GetObjects().GetUpperBound();
	for (int i = 0; i <= ub; i++)
	{
		Table* pTable = dynamic_cast<Table*>(GetObjects()[i]);
		if (pTable)
			if (pTable->SetTotalRectActiveById(nID))
				return TRUE;
	}
	return FALSE;
}

//------------------------------------------------------------------------------
TotalCell* CWoormDocMng::GetTotalCellById(int nID)
{
	int ub = GetObjects().GetUpperBound();
	for (int i = 0; i <= ub; i++)
	{
		Table* pTable = dynamic_cast<Table*>(GetObjects()[i]);
		if (pTable)
		{
			int nCol;
			TotalCell* pTotalCell = pTable->GetTotalCellById(nID, nCol);
			if (pTotalCell)
				return pTotalCell;
		}
	}
	return NULL;
}

//------------------------------------------------------------------------------
BaseObj* CWoormDocMng::SetCurrentObj (BaseObj* pO, CPoint& pt)
{
	BaseObj* pPrevObj = m_pCurrentObj;

	m_pCurrentObj = NULL;
	int ub = GetObjects().GetUpperBound();
	if (pO) for (int i = 0; i <= ub; i++)
	{
		BaseObj* pObj = GetObjects()[i];
		if (pObj == pO)
		{
			if (!pObj->IsKindOf(RUNTIME_CLASS(Table)))
				pt = CPoint(pObj->Left() + 1, pObj->Top() + 1);

			m_pCurrentObj = pObj;

			// pack GetObjects() after current and make current last object in array
			// to make it on Top during window repaint
			for (int j = i + 1; j <= ub; j++)
				GetObjects()[j - 1] = GetObjects()[j];
			GetObjects()[ub] = pObj;
			
			// set active status only if not in multiple selection
			if (!m_pMultipleSelObj)
			{
				//fix anomalia: mancata selezione titolo tabella se prima era già selezionata una colonna
				if (m_pCurrentObj->IsKindOf(RUNTIME_CLASS(Table)))
					((Table*)m_pCurrentObj)->m_nActiveColumn = Table::NO_ACTIVE_COLUMN;

				if (m_pActiveRect->SetActive(m_pCurrentObj->GetActiveRect()))
				{
					InvalidateStackedObjects();
					UpdateWindow();
				}
			}
			GetWoormFrame()->SelectLayoutObject(pObj); //TODO LAYOUT : aggiungere layout corrente come parametro
			return pPrevObj;
		}
	}

	m_pActiveRect->Clear();
	return pPrevObj;
}

//  MoveObjects
//
//------------------------------------------------------------------------------
void CWoormDocMng::MoveObjects()
{
	// move multiple selected GetObjects()
	if (!m_pMultipleSelObj->IsEmpty() && m_pMultipleSelObj->MovingObject())
	{
		m_pMultipleSelObj->MoveMultipleSelObjects();
		UpdateWindow();
	}
}

//------------------------------------------------------------------------------
void CWoormDocMng::OnObjectCopy()
{
	// toggle acctive enhaecement for copy only object when saved as bitmap (default)
	if (m_pCurrentObj && !m_pMultipleSelObj)
	{
		
		// force WM_PAINT to allow copy in clipboard
		InvalidateRect(m_pCurrentObj->GetBaseRect());
		UpdateWindow();
	}
}      

//------------------------------------------------------------------------------
void CWoormDocMng::OnObjectPaste()
{
}

//------------------------------------------------------------------------------
void CWoormDocMng::OnObjectCut()
{
	if(!GetWoormView()->IsInTransfertMode())
		if (AfxMessageBox(_TB("Do you confirm the deletion of the selected objects? "), MB_YESNO) != IDYES)
			return;

	CRSTreeCtrl* m_pTree = GetRSTree(ERefreshEditor::Layouts);

	BOOL bDeleted = FALSE;
	
	// se esiste, svuoto la property grid altrimenti, cancellando l'oggetto,
	// rimarrebbe la property che punta a qualcosa non più valido dando svariati problemi
	if (GetWoormFrame()->m_pObjectPropertyView)
		GetWoormFrame()->m_pObjectPropertyView->ClearPropertyGrid();

	int ub = GetObjects().GetUpperBound();
	for (int i = ub; i >= 0; i--)
	{
		BaseObj* pObj = GetObjects()[i];
		if (
			pObj &&
				(
					(!m_pMultipleSelObj && m_pCurrentObj && m_pCurrentObj == pObj)
					||
					(m_pMultipleSelObj && m_pMultipleSelObj->ObjectInSelection(pObj))
				)
			)
		
		{
			SAFE_DELETE(m_pMultiColumns);
			if (pObj->DeleteEditorEntry())
			{
				// if deleting currentobj must deselect it
				if (pObj == m_pCurrentObj)
				{
					m_pActiveRect->Clear();
					m_pCurrentObj = NULL;
				}

				InvalidateRect(pObj->GetRectToInvalidate(), TRUE);

				BaseObj * pNew = GetObjects()[i];
				ASSERT(pNew == pObj);
				m_RemovedObj.Add(pNew);
				GetObjects().SetAt(i, NULL);
				GetObjects().RemoveAt(i);

				bDeleted = TRUE;
			}
		}
	}

	if (m_pMultiColumns && m_pMultiColumns->m_columns.GetSize() > 0)
	{
		bDeleted = TRUE;
		ActiveColumn* aCol = (ActiveColumn*)m_pMultiColumns->m_columns.GetAt(0);
		ASSERT_VALID(aCol);
		TableColumn* tCol = aCol->m_pColumn;
		ASSERT_VALID(tCol);
		Table* pTable = tCol->GetTable();
		ASSERT_VALID(pTable);

		if (pTable->GetColumns().GetCount() == m_pMultiColumns->m_columns.GetSize())
		{
			pTable->m_nActiveColumn = -1;
			SAFE_DELETE(m_pMultiColumns);

			if (pTable->DeleteEditorEntry())
			{
				// if deleting currentobj must deselect it
				if (pTable == m_pCurrentObj)
				{
					m_pActiveRect->Clear();
					m_pCurrentObj = NULL;
				}

				InvalidateRect(pTable->GetRectToInvalidate(), TRUE);

				int idx = GetObjects().FindIndexByID(pTable->GetInternalID());
				if (idx > -1)
				{
					BaseObj * pNew = GetObjects()[idx];
					ASSERT(pNew == pTable);
					m_RemovedObj.Add(pNew);
					GetObjects().SetAt(idx, NULL);
					GetObjects().RemoveAt(idx);
				}
			}
		}
		else
			m_pMultiColumns->DeleteAllSelectedColumns();

		SAFE_DELETE(m_pMultiColumns);
		bDeleted = TRUE;
	}

	if (m_pNodesSelection)
	{
		for (int i = 0; i < m_pNodesSelection->GetSize(); i++)
		{
			CNodeTree* node = (CNodeTree*)m_pNodesSelection->GetAt(i);
			WoormField* pField = dynamic_cast<WoormField*>(node->m_pItemData);

			if (!pField)
				break;
			if (node->m_NodeType != CNodeTree::NT_VARIABLE)
				break;
			if (!pField->IsHidden())
				break;
			if (!GetEditorSymTable()->CanDeleteField(pField->GetName()))
				continue;
			
			CString sName = pField->GetName();
			WORD id = pField->GetId();

			if (GetEditorSymTable()->GetFieldByID(id))
				GetEditorSymTable()->DelField(sName);

			if (GetSymTable()->GetFieldByID(id))
				GetSymTable()->DelField(sName);

			if (m_pEditorManager->GetRuleData())
				m_pEditorManager->GetRuleData()->DeleteField(sName);

			bDeleted = TRUE;
		}
	}

	// remove multiple selection because all object in it was bDeleted                                    
	if (m_pMultipleSelObj) OnDeselectAll();

	if (bDeleted)
	{
		SetModifiedFlag();
		UpdateWindow ();

		RefreshRSTree(ERefreshEditor::Layouts);
		RefreshRSTree(ERefreshEditor::Variables, NULL, TRUE);
	}
}

//------------------------------------------------------------------------------
void CWoormDocMng::ObjectUndo(BaseObj * pObject)
{
	if (pObject->IsKindOf(RUNTIME_CLASS(Table)))
	{
		return;		//TODO problema per campi TOTALI E SUBTOTALI 
		//VEDI BOOL Table::DeleteEditorEntry ()
		Table * pTbl = (Table*)pObject;
		
		WORD nFirstID;

		WoormTable*	pSymTable = GetEditorSymTable();

		DisplayTables*	pDispTable = GetEditorSymTable()->GetDisplayTables();

		CString sTableName = cwsprintf(L"Table_%d", pTbl->m_wInternalID);
		
		CString layout = m_dsCurrentLayoutView.GetString();

		pDispTable->AddTable
			(
				sTableName,
				10,
				pTbl->m_wInternalID,
				layout
				);
		TableColumn * pCol;
		for (int i = 0; i < pTbl->m_Columns.GetSize(); i++)
		{
			pCol = pTbl->m_Columns[i];
			if (i == 0 )
				nFirstID = pCol->m_wInternalID;
			WoormField* pF = (WoormField*)GetEditorSymTable()->GetFieldByID(pCol->m_wInternalID);

			if (!pF) continue;

			ASSERT_VALID(pF);
			pF->SetHidden(FALSE);
			pF->SetFieldType(WoormField::RepFieldType::FIELD_COLUMN);

			SyncronizeViewSymbolTable(pF);
		}

		CPoint point;
		pTbl->SetActiveColumn(nFirstID, point);
		
		m_pCurrentObj = pTbl;
		GetObjects().Add(m_pCurrentObj);
		InvalidateRect(m_pCurrentObj->GetBaseRect());
		m_pActiveRect->SetActive(m_pCurrentObj->GetActiveRect());
		m_pCurrentObj->AddCreateUndoInfo();

		RefreshRSTree(ERefreshEditor::Layouts);
		RefreshRSTree(ERefreshEditor::Variables);

		SetCurrentObj(pTbl->m_wInternalID, point);

		GetWoormFrame()->GetLayoutTreeView()->SelectLayoutObject(pTbl->m_Columns[0]);
		UpdateWindow();
		return;
	}
	else if (pObject->IsKindOf(RUNTIME_CLASS(FieldRect)))
	{
		FieldRect * pRect = (FieldRect*)pObject;
		WoormField* pF = (WoormField*)GetEditorSymTable()->GetFieldByID(pRect->m_wInternalID);

		if (!pF) return;

		ASSERT_VALID(pF);
		pF->SetHidden(FALSE);
		SyncronizeViewSymbolTable(pF);
	}

	m_pCurrentObj = pObject;
	// add object and initialize it in view area
	if (m_pCurrentObj->IsKindOf(RUNTIME_CLASS(Repeater)))
	{
		GetObjects().InsertAt(GetObjects().m_nCountAutoObjects, m_pCurrentObj);
		((Repeater*)m_pCurrentObj)->Rebuild(&GetObjects());
	}
	else
		GetObjects().Add(m_pCurrentObj);

	if (m_pCurrentObj->IsKindOf(RUNTIME_CLASS(FieldRect)))
		RefreshRSTree(ERefreshEditor::Variables);
	
	RefreshRSTree(ERefreshEditor::Layouts);

	InvalidateRect(m_pCurrentObj->GetBaseRect());

	m_pActiveRect->SetActive(m_pCurrentObj->GetActiveRect());

	m_pCurrentObj->AddCreateUndoInfo();

	UpdateWindow();
}


//------------------------------------------------------------------------------
void CWoormDocMng::OnObjectUndo()
{          
	// multiple selection as local undo stack
	if (m_pMultipleSelObj) 
	{
		m_pMultipleSelObj->Undo();

		UpdateWindow();
		return;
	}

	// make undo only if something is changed
	if (m_pUndoStack->GetSize() == 0)
	{
		if (GetObjects().GetSize() == 0)
			m_pCurrentObj = NULL;
		return;
	}

	// Pop info from storic last action stack info
	UndoInfo* pUndoInfo = (UndoInfo*) m_pUndoStack->Pop();

	if (pUndoInfo->m_pObj && IsWindow(pUndoInfo->m_pObj->m_hWnd))
	{

		// if object was removed from user continue iteration
		if (!ExistObject(pUndoInfo->m_pObj))
		{
			ObjectUndo(pUndoInfo->m_pObj);
			int pos = m_RemovedObj.FindPtr(pUndoInfo->m_pObj);
			if (pos > -1)
				m_RemovedObj.SetAt(pos, NULL); m_RemovedObj.RemoveAt(pos);
			return;
		}

		// rect with null data (all 0s) means creation on object events
		if (pUndoInfo->m_rect.IsRectNull())
		{
			m_pCurrentObj = pUndoInfo->m_pObj;
			OnObjectCut();
			if (m_pUndoStack->GetSize() == 0)
				m_pCurrentObj = NULL;
			else
			{
				UndoInfo* pUndoInfoH = (UndoInfo*)m_pUndoStack->Top();
				if (pUndoInfoH->m_pObj && IsWindow(pUndoInfoH->m_pObj->m_hWnd))
					m_pCurrentObj = ((UndoInfo*)m_pUndoStack->Top())->m_pObj;
				else
					m_pCurrentObj = NULL;
			}
		}
		else
		{
			pUndoInfo->m_pObj->UndoMoveAndSize(pUndoInfo->m_rect);
			m_pCurrentObj = pUndoInfo->m_pObj;
			// Pop info from storic last action stack info
			UndoInfo* pUndoInfoP = (UndoInfo*)m_pUndoStack->Top();
			if (pUndoInfoP && pUndoInfoP->m_pObj && m_pCurrentObj == pUndoInfoP->m_pObj && pUndoInfoP->m_rect.IsRectNull())
				OnObjectUndo();
		}

	}
	else 
		m_pCurrentObj = NULL;

	if (m_pCurrentObj && IsWindow(m_pCurrentObj->m_hWnd))
		m_pActiveRect->SetActive(m_pCurrentObj->GetActiveRect());

	// delete undo info and force repaint;
	delete pUndoInfo;

	UpdateWindow();
}

//------------------------------------------------------------------------------
BOOL CWoormDocMng::ExistObject(BaseObj* aObj)
{
	int ub = GetObjects().GetUpperBound();
	for (int i = ub; i >= 0; i--)
	{
		BaseObj* pObj = GetObjects()[i];
		if (pObj == aObj) return TRUE;
	}      
	return FALSE;
} 
 
//------------------------------------------------------------------------------
void CWoormDocMng::PasteDIBitmap()
{
	if (m_Creating != NONE) return;
	CString strName (m_strBmpName);

	CTBNamespace aNamespace(CTBNamespace::IMAGE);
	aNamespace.SetNamespace(L"Image." + GetNamespace().GetApplicationName() + '.' + GetNamespace().GetModuleName());

	CTBExplorer aExplorer (CTBExplorer::SAVE, aNamespace, TRUE);
	BOOL bOk = aExplorer.Open ();
	CStringArray arPath;
	
	aExplorer.GetSelPathElements(&arPath);
	
	if (arPath.GetSize())
		m_strBmpName = arPath.GetAt(0);

	// copy DIB from clipboard in CDIBitmap structure
	HANDLE		hDIBitmap;
	CWoormView*	pView = GetWoormView();
		
	if (!pView->OpenClipboard())
		return;
	
	hDIBitmap = CopyHandle(GetClipboardData(CF_DIB));
	CloseClipboard ();
	
	if (!hDIBitmap)
		return;
		
	CDIBitmap* pDIBitmap = new CDIBitmap;
	if(!(pDIBitmap->Attach(hDIBitmap)))
	{
		//delete pDIBitmap;
		::DeleteObject(pDIBitmap);
		return;
	}
		
	// save bitmap to choosed strFile
	pDIBitmap->SaveBitmap(m_strBmpName);
	//delete pDIBitmap;
	::DeleteObject(pDIBitmap);
								
	// post creation on text object		                    
	m_Creating = GRAPHRECT;
	ClipCursorToActiveView();
}
 
//------------------------------------------------------------------------------
//void CWoormDocMng::PasteText(BOOL bIsOemText)
//{
//	if (m_Creating != NONE) return;
//
//	//text object from clipboard
//	CWoormView*	pView = GetWoormView();
//	if (!(pView->OpenClipboard()))
//		return;
//		
//	// load text data
//	pView->BeginWaitCursor ();
//	HANDLE hData = CopyHandle(GetClipboardData(bIsOemText ? CF_OEMTEXT : CF_TEXT));
//	CloseClipboard ();
//	pView->EndWaitCursor ();
//	
//	// same strange error seem to appear in clipboard	
//	if (!hData)	return;
//
//	m_strTextField = (LPTSTR) ::GlobalLock ((HGLOBAL) hData);
//	GlobalUnlock((HGLOBAL) hData);
//
//#ifndef _UNICODE
//	if (bIsOemText) m_strTextField.OemToAnsi();
//#endif
//		                    
//	// post creation on text object		                    
//	m_Creating = TEXTRECT;
//	ClipCursorToActiveView();
//}

//------------------------------------------------------------------------------
BOOL CWoormDocMng::CurrentIsTable () const
{
	if (!m_pCurrentObj) return FALSE;
	if (!m_pCurrentObj->IsKindOf(RUNTIME_CLASS(Table))) return FALSE;

	return TRUE;
}


//------------------------------------------------------------------------------
BOOL CWoormDocMng::NoActiveColumn () const
{
	return
		(!CurrentIsTable()) ||
		(CurrentIsTable() && ((Table*) m_pCurrentObj)->NoActiveColumn());
}


//------------------------------------------------------------------------------
BOOL CWoormDocMng::MoreThanOneColumn () const
{
	return !NoActiveColumn () && ((Table*) m_pCurrentObj)->MoreThanOneColumn();
}


//------------------------------------------------------------------------------
BOOL CWoormDocMng::ActiveColumnHasTotal() const
{
	return !NoActiveColumn () && ((Table*) m_pCurrentObj)->ActiveColumnHasTotal();
}


//------------------------------------------------------------------------------
BOOL CWoormDocMng::ActiveColumnCanHaveTotal() const
{
	return !NoActiveColumn () && ((Table*) m_pCurrentObj)->ActiveColumnCanHaveTotal();
}

//------------------------------------------------------------------------------
void CWoormDocMng::AnchorFieldToColumn(CLayout* pObjects)
{
	AnchorFieldToColumn(pObjects, 0);
}

//------------------------------------------------------------------------------
void CWoormDocMng::AnchorFieldToColumn(CLayout* pObjects, int nStartIndex)
{
	//Guardo se ho dei field ancorati a colonne e li vado a posizionare sotto la colonna
	//non posso farlo prima, perche il field ancorato potrebbe essere scritto prima nel report rispetto la colonna cui e' ancorato
	for (int i = nStartIndex; i < pObjects->GetSize(); i++)
	{
		BaseObj* pObj = (*pObjects)[i];
		if (pObj->IsKindOf(RUNTIME_CLASS(BaseRect)))
		{
			BaseRect* pBaseRect = (BaseRect*) pObj;
			if (pBaseRect->m_AnchorLeftColumnID)
			{
				pBaseRect->AnchorFieldToColumn(pObjects, pBaseRect->m_AnchorLeftColumnID, pBaseRect->m_AnchorRightColumnID);
			}
		}
	}
}

//------------------------------------------------------------------------------
void CWoormDocMng::OnUpdateAddEABarcode (CCmdUI* pCmdUI)
{
	pCmdUI->Enable
		(
			m_bAllowEditing  &&
			!m_bPlayback  && 
			m_Creating == NONE && 
			!IsReportRunning() &&
			AfxGetOleDbMng()->EasyAttachmentEnable()
		);
}

//------------------------------------------------------------------------------
void CWoormDocMng::OnAddEABarcode ()
{
	m_dsEABarCode.m_strBarcodeValue = L"EA-12345678901234";
	m_dsEABarCode.m_strBarcodeType =  _T("CODE39"); //E_BARCODE_TYPE_ALFA39;

	AddEABarCodeObject(&GetObjects());
	
	BaseObj* pObj = GetObjects().FindByID(SpecialReportField::ID.EA_BARCODE);
	if (!pObj)
	{
		Message(_TB("There is not an empty area to add the barcode: please try to move some fields"));
		return;
	}
	
	m_pCurrentObj = pObj;

	RefreshRSTree(ERefreshEditor::Layouts, pObj);

	InvalidateRect	(m_pCurrentObj->GetBaseRect());
	m_pActiveRect->SetActive(m_pCurrentObj->GetActiveRect());
	UpdateWindow  ();
}



//------------------------------------------------------------------------------
void CWoormDocMng::AddEABarCodeObject(CLayout* pObjects)
{
	BaseObj* pObj = pObjects->FindByID(SpecialReportField::ID.EA_BARCODE);
	if (!pObj)
	{
		BeginWaitCursor(); //Wait cursor
		BOOL bVert = FALSE;

		CRect rect(0,0,0,0) ;

		// for different barcode type i need differents rects
		int height = 60;
		int width = 250;

		BOOL bOk = pObjects->FindFreeRect (height, width, rect);
		if (bOk)
		{
			ASSERT_TRACE(rect.Width() == width && rect.Height() == height, _T("AddEABarCodeObject - Bad H. free rect"));
		}
		else
		{
			bOk = pObjects->FindFreeRect (width, height, rect);
			if (bOk)
			{
				ASSERT_TRACE(rect.Width() == height && rect.Height() == width, _T("AddEABarCodeObject - Bad V. free rect"));
			}
			bVert = TRUE;
		}
		if (bOk)
		{
			TextRect* pField = new TextRect(CPoint(rect.left, rect.top), this);
			pField->m_wInternalID = SpecialReportField::ID.EA_BARCODE;
			pField->SetBaseRect(rect);
			pField->SetBarCode(CBarCodeTypes::BarCodeEnum(m_dsEABarCode.m_strBarcodeType), bVert);//todo ilaria barcode ?? gestione tipi ... 
			pField->SetHideExpression(_T("Not (ReportIsArchiving && ReportCurrentPageNumber == 1)"));
			pField->m_sStyleClass = CWoormTemplate::s_sEA_BarCode_StyleName;
			pField->m_bPersistent = FALSE;

			pField->SetText(m_dsEABarCode.m_strBarcodeValue);
			pObjects->Add(pField);
			DataBool b = pField->m_bHidden;
			if (!pField->m_pHideExpr->Eval(b))
			{
				if (pField->m_pHideExpr->GetErrId() != Expression::NULL_OPR)
					TRACE(_T("Evalutation error in show/hide field expression: %s\n"), pField->GetDescription());
			}
			else
				pField->m_bHidden = b;

			pField->UpdateDocument();
		}	
		EndWaitCursor(); //Wait cursor
	}
	else if (pObj->IsKindOf(RUNTIME_CLASS(TextRect)))
	{
		TextRect* pT = (TextRect*) pObj;
		pT->SetText(m_dsEABarCode.m_strBarcodeValue);
		pT->SetBarCode(CBarCodeTypes::BarCodeEnum(m_dsEABarCode.m_strBarcodeType), pT->IsVerticalBarCode ());
		pT->UpdateDocument();
	}
}

BaseRect* CWoormDocMng::CreateBarcodeAtPosition(LONG x, LONG y, BOOL bVert)
{
	m_dsEABarCode.m_strBarcodeValue = L"EA-12345678901234";
	m_dsEABarCode.m_strBarcodeType = _T("CODE39"); //E_BARCODE_TYPE_ALFA39;

	int height = 60;
	int width = 250;

	TextRect* pText = new TextRect(CPoint(x, y), this);
	pText->m_wInternalID = SpecialReportField::ID.EA_BARCODE;
	CRect rect;
	if (!bVert)
	{
		rect.left = x;
		rect.top = y;
		rect.right = x + width;
		rect.bottom = y + height;
	}
	else
	{
		rect.left = x;
		rect.top = y;
		rect.right = x + height;
		rect.bottom = y + width;
	}

	pText->SetBaseRect(rect);

	pText->SetBarCode(CBarCodeTypes::BarCodeEnum(m_dsEABarCode.m_strBarcodeType), bVert);
	
	pText->SetText(m_dsEABarCode.m_strBarcodeValue);

	pText->SetHideExpression(_T("Not (ReportIsArchiving && ReportCurrentPageNumber == 1)"));
	
	pText->m_sStyleClass = CWoormTemplate::s_sEA_BarCode_StyleName;

	pText->m_bPersistent = FALSE;

	DataBool b = pText->m_bHidden;
	if (!pText->m_pHideExpr->Eval(b))
	{
		if (pText->m_pHideExpr->GetErrId() != Expression::NULL_OPR)
			TRACE(_T("Evalutation error in show/hide field expression: %s\n"), pText->GetDescription());
	}
	else
		pText->m_bHidden = b;

	return pText;
}

//------------------------------------------------------------------------------
CString CWoormDocMng::GetAlternativeNameForEA() const
{
	if ((m_pWoormInfo && m_pWoormInfo->m_bUseOutputFileName && m_pWoormInfo->m_arstrOutputFileNames.GetSize() > 0))
		return m_pWoormInfo->m_arstrOutputFileNames[0];

	CString strAlternativeName;
	if (m_pWoormInfo)
	{
		if (m_pWoormInfo->m_bConcatPDF && m_pWoormInfo->m_arstrOutputFileNames.GetSize() > 0)
			strAlternativeName = ::GetName(m_pWoormInfo->m_arstrOutputFileNames[0]);
		else
			strAlternativeName = (!m_pWoormInfo->m_Email.m_sAttachmentReportName.IsEmpty()) ? m_pWoormInfo->m_Email.m_sAttachmentReportName : m_Email.m_sAttachmentReportName;
	}
	else
		strAlternativeName = m_Email.m_sAttachmentReportName;

	return strAlternativeName;
}

//------------------------------------------------------------------------------
void CWoormDocMng::ValorizeEABarCode(const CString strAlternativeName, BOOL bGenerateNewBarcode)
{
	if (m_Template.m_bIsTemplate)
		return;
	//----
	if (!m_dsEABarCode.m_strBarcodeValue.IsEmpty() && m_dsEABarCode.m_strBarcodeValue != L"EA-12345678901234")	//todo ilaria barcode, chiedi questo codice come viene usato? dubbio perchè introducendo gestione di più tipi barcode non sono tutti alfanumerici
		return;

	m_dsEABarCode.m_strBarcodeValue = L"EA-12345678901234";
	m_dsEABarCode.m_strBarcodeType =  _T("CODE39"); //E_BARCODE_TYPE_ALFA39;

	if (!AfxGetOleDbMng()->EasyAttachmentEnable())
		return;
			
	m_dsEABarCode = AfxGetIDMSRepositoryManager()->GetBarcodeValue(GetNamespace().ToString(), m_pCallerDocument, strAlternativeName, bGenerateNewBarcode == TRUE);		

	if (CanShowEABarCode())
	{
		m_bIsArchiving = TRUE;
		m_Layouts.EnumObjects(&CWoormDocMng::AddEABarCodeObject);
		BaseObj* pObj = GetObjects().FindByID(SpecialReportField::ID.EA_BARCODE);
		if (!pObj && (!m_pWoormInfo || (m_pWoormInfo && !m_pWoormInfo->m_bHideFrame && !m_pWoormInfo->m_bIconized)))
			Message(_TB("The procedure AddEABarCodeObject fails to find a free rect for EA barcode.\n\rPlease modify the report adding the EasyAttachment barcode from Objects menu."), 0, 0, 0, CMessages::MSG_WARNING);
	}
}

//------------------------------------------------------------------------------
void CWoormDocMng::ValorizeEABarCode()
{
	ValorizeEABarCode(GetAlternativeNameForEA(), (m_pWoormInfo && m_pWoormInfo->m_bArchivePdfFormat));
}

//------------------------------------------------------------------------------
BOOL CWoormDocMng::CanShowEABarCode()
{
	return !m_dsEABarCode.m_strBarcodeValue.IsEmpty() && m_dsEABarCode.m_strBarcodeValue.CompareNoCase(L"EA-12345678901234");
}

//=============================================================================

BOOL CWoormDocMng::ParseLayouts (ViewParser& lex)
{
	BOOL bInvertOrientation = FALSE;
	if (!lex.LookAhead(T_PAGE_LAYOUT))
	{
		VERIFY(m_Layouts.Lookup(REPORT_DEFAULT_LAYOUT_NAME, m_Objects));

		return ParseObjects(lex, FALSE);
	}

	while (lex.LookAhead(T_PAGE_LAYOUT))
	{
		CString sLayoutName;
		if (lex.ParseTag(T_PAGE_LAYOUT) && lex.ParseString(sLayoutName))
		{
			bInvertOrientation = lex.Matched(T_INVERT_ORIENTATION);
			lex.ParseBegin();
		}
		else
			return lex.SetError (_TB("Incorrect PageLayout syntax: missing layout name "));

		m_Objects = m_Layouts.Add(sLayoutName);
		m_Objects->m_bInvertOrientation = bInvertOrientation;

		if (!ParseObjects(lex, TRUE))
			return FALSE;
	}

	m_Layouts.Lookup(REPORT_DEFAULT_LAYOUT_NAME, m_Objects);	//se non c'e rimane selezionato l'ultimo parsato
	m_dsCurrentLayoutEngine = m_dsCurrentLayoutView = m_Objects->m_strLayoutName;

	m_Layouts.EnumObjects(&CWoormDocMng::AnchorFieldToColumn);

	// ordina gli oggetti prima di stamparli per ottimizzare su dispositivi
	// non raster (tipo stampanti ad aghi) se richiesto dall'utente (default si)
	SortLayoutObjectsOnPosition();
	
	return TRUE;
}

//------------------------------------------------------------------------------
BOOL CWoormDocMng::ParseObjects (ViewParser& lex, BOOL bWithPageLayout)
{
	while (TRUE)
	{
		switch (lex.LookAhead())	//riordinate "case entry" in base alla frequenza di utilizzo
		{
			case T_FIELD    : m_pCurrentObj = new FieldRect    (m_ptCurrPos, this, 0);     break;
			case T_TEXT     : m_pCurrentObj = new TextRect     (m_ptCurrPos, this);        break;
			case T_TABLE    : // parse only dimensions
			{
				int nRow, nCol = 1;
				if  (lex.ParseTable (nRow, nCol))
				{
					m_pCurrentObj = new Table (m_ptCurrPos, this, nRow, nCol);
					break;
				}
				return FALSE;
			}
			case T_END		: return bWithPageLayout && lex.ParseEnd(); 
			case T_RNDRECT  : // mantiene la compatibilita' con il passato
			case T_SQRRECT  : m_pCurrentObj = new SqrRect      (m_ptCurrPos, this);				break;
			case T_BITMAP    : m_pCurrentObj = new GraphRect   (m_ptCurrPos, _T(""), this);		break;
			case T_FILE     : m_pCurrentObj = new FileRect     (m_ptCurrPos, _T(""), this);		break;
			case T_REPEATER : m_pCurrentObj = new Repeater     (m_ptCurrPos, this, 0);			break;
			case T_CHART	: m_pCurrentObj = new Chart		   (m_ptCurrPos, this, 0);			break;
			case T_METAFILE : m_pCurrentObj = new MetafileRect (m_ptCurrPos, _T(""), this);     break;
			case T_LINKURL	: m_pCurrentObj = new UrlRect	   (m_ptCurrPos, _T(""), this);     break;
			case T_LINKS    : return TRUE;
			case T_SELECT   : return TRUE;
			case T_REPORT   : return TRUE;
			case T_EOF      : return TRUE;
			case T_PROPERTIES: return TRUE; //???
			default         : return lex.SetError (_TB("Object not found."));
		}

		// found new element and try to parse completly
		if (!m_pCurrentObj->Parse(lex))
		{
			SAFE_DELETE(m_pCurrentObj);			
			return FALSE;
		}
		
		if (m_pCurrentObj->GetInternalID())
		{
			if (GetObjects().FindByID(m_pCurrentObj->GetInternalID()))
			{
				lex.SetError (_TB("Duplicate Object Alias found.") + cwsprintf(_T(" (ID: %d)"), m_pCurrentObj->GetInternalID()), NULL, lex.GetCurrentPos(), lex.GetCurrentLine());
				SAFE_DELETE(m_pCurrentObj);
				continue;
			}
			if (m_pCurrentObj->IsKindOf(RUNTIME_CLASS(FieldRect)) && !m_Template.m_bIsTemplate)
			{
				WoormField* pF = m_ViewSymbolTable.GetFieldByID(m_pCurrentObj->GetInternalID());
				if (!pF)
				{
					lex.SetError(_TB("Variable was not found.") + cwsprintf(_T(" (ID: %d)"), m_pCurrentObj->GetInternalID()), NULL, lex.GetCurrentPos(), lex.GetCurrentLine());
					SAFE_DELETE(m_pCurrentObj);
					continue;
				}
			}
		}

		// parse bOk, so add to element list
		GetObjects().Add (m_pCurrentObj);
	}

	return FALSE;
}

// parsa stili font e oggetti utilizzando la release letta in testa al report
//------------------------------------------------------------------------------
BOOL CWoormDocMng::ParseWoormLinks (ViewParser& lex)
{
	m_arWoormLinks.m_pViewSymbolTable = &m_ViewSymbolTable;
	m_arWoormLinks.m_bPlayBack = m_bPlayback;
	
	return m_arWoormLinks.Parse(lex);
}

// parsa stili font e oggetti utilizzando la release letta in testa al report
//------------------------------------------------------------------------------
BOOL CWoormDocMng::ParseViewInfo (ViewParser& lex)
{
	FontsParser aFontsParser;
	FormatsParser aFormatsParser;
	
	CTBNamespace& Ns = IsReportString() && m_pCallerDocument
		?
		m_pCallerDocument->GetNamespace()
		:
		GetNamespace()
		;

	BOOL bOk =	aFontsParser.Parse(Ns, FontStyle::FROM_WOORM, m_pFontStyles, lex);

	// smart pointer is used to temporary lock the table
	FormatStyleTablePtr pFormatTable (m_pFormatStyles, TRUE);
	bOk = bOk && aFormatsParser.Parse(Ns, Formatter::FROM_WOORM, pFormatTable.GetPointer(), lex);

	if (AfxGetExportSymbols()->IsActivated())
	{
		int i = 0;
		for (; i < m_pFontStyles->GetSize(); i++)
		{
			FontStylesGroup* pG = m_pFontStyles->GetAt(i);
			for (int j=0; j < pG->GetFontStyles().GetSize(); j++)
			{
				FontStyle* pF = (FontStyle*) pG->GetFontStyles().GetAt(j);
				if (pF->GetOwner().GetType() == CTBNamespace::REPORT)
					AfxGetExportSymbols()->AddItemOnLastFile(pF->GetStyleName(), EXP_SYMB_FONT);
			}
		}
		for (i = 0; i < m_pFormatStyles->GetSize(); i++)
		{
			FormatterGroup* pG = m_pFormatStyles->GetAt(i);
			int j = 0;
			for (; j < pG->GetFormatters().GetSize(); j++)
			{
				Formatter* pF = (Formatter*) pG->GetFormatters().GetAt(j);
				if (pF->GetOwner().GetType() == CTBNamespace::REPORT)
					AfxGetExportSymbols()->AddItemOnLastFile(pF->GetName(), EXP_SYMB_FORMATTER);
			}
		}
	}

	//---- build & update m_ViewSymbolTable
	// search T_VAR. If it's a RDE file parsing allow equals alias (for column/columnTotal)
	CString sError; int nLine = -1;
	bOk = bOk &&
		IsReportString()
		?
		m_ViewSymbolTable.ParseVariablesFromString(GetReportString(), sError, nLine, m_bPlayback)
		:
		m_ViewSymbolTable.ParseVariables(lex.GetFileName(), sError, nLine, m_bPlayback);
	if (!bOk)
	{
		lex.SetError(cwsprintf(L"%s\r\nline: %d", sError, nLine), L"", nLine);
	}
	
	if (bOk && m_pWoormInfo && m_pWoormInfo->GetParameters().GetSize() && m_ViewSymbolTable.GetSize())
	{
		//rivalorizzazione a partire dal WoormInfo
		int i;
		for (i = 0; i < m_pWoormInfo->GetParameters().GetSize(); i++)
		{
			CDataObjDescription* par = m_pWoormInfo->GetParamDescription(i);
			SymField* pF = m_ViewSymbolTable.GetField(par->GetName());
			if (pF)
			{
				if (DataType::IsCompatible(par->GetValue()->GetDataType(), pF->GetDataType()))
					pF->AssignData(*par->GetValue());
				else
				{
					TRACE(_T("Field %s unmatch datatype between variable declaration and woorminfo parameter\n"), par->GetName());
				}
			}
		}
	}
	//----

	bOk = bOk && ParseLayouts(lex);

	if (bOk)
	{
		//LoadTemplate(BOOL bEditing, BOOL bShowDialog = TRUE, BOOL bImportStaticObjects=TRUE, BOOL bUpdateDoc = TRUE);
		if (!LoadTemplate(FALSE, TRUE, TRUE, FALSE))
			m_Layouts.EnumObjects(&CWoormDocMng::ApplyRepeater);
	}

	return bOk;
}

//------------------------------------------------------------------------------
CWoormFrame* CWoormDocMng::GetWoormFrame(BOOL bCheck /*=TRUE*/) const
{
	CWoormFrame* pFrame = dynamic_cast<CWoormFrame*>(GetFrame());
	if (bCheck)
	{
		ASSERT_VALID(pFrame);
	}
	return pFrame;
}

//------------------------------------------------------------------------------
CWoormView*	CWoormDocMng::GetWoormView(BOOL bCheck /*=TRUE*/) const
{
	CWoormView* pView = dynamic_cast<CWoormView*> (__super::ViewAlreadyPresent(RUNTIME_CLASS(CWoormView), FALSE));
	if (bCheck)
	{
		ASSERT_VALID(pView);
	}	
	return pView;
}

//------------------------------------------------------------------------------
WoormTable* CWoormDocMng::GetEngineSymTable() 
{ 
	if (m_pEngine)
		return m_pEngine->GetSymTable();
	return NULL;
}

//------------------------------------------------------------------------------
WoormTable* CWoormDocMng::GetEditorSymTable()
{		
	if (m_pEditorManager && m_pEditorManager->GetPrgData())
		return m_pEditorManager->GetPrgData()->GetSymTable();
	return NULL;
}

// Una volta si usava solo la PostMessage. Oggi inoltre se si usa la CWoormInfo 
// per gestire anche multipli report e stampe automatiche potrebbe 
// essere disallocato m_pWoormInfo da fuori senza che sia ancora partita
// la stampa vera e propria (vedi simulazione di differita in TIM)
//------------------------------------------------------------------------------
void CWoormDocMng::RunReport(BOOL bActivate /*= FALSE*/, BOOL bPost/* = TRUE*/)
{
	SetDecorateTitle();

	if (bActivate)
		Activate(NULL, bPost);

	if (bPost)
		PostMessage(WM_COMMAND, ID_RUN, NULL);
	else
		SendMessage(WM_COMMAND, ID_RUN, NULL);
}

//------------------------------------------------------------------------------
void CWoormDocMng::SetDecorateTitle()
{
	ASSERT_VALID(this);
	CWoormFrame* pFrame = GetWoormFrame();
	ASSERT_VALID(pFrame);
	if (!::IsWindow(pFrame->m_hWnd))
		return;

	UINT idi = IDI_REPORT;

	CString str;

	if (m_pWoormInfo)
	{
		ASSERT_VALID(m_pWoormInfo);
		if (!m_pWoormInfo->m_arReportTitles.IsEmpty() &&  m_pWoormInfo->m_nNextReport >= 0 && m_pWoormInfo->m_nNextReport <= m_pWoormInfo->m_arReportTitles.GetSize())
		{
			str = m_pWoormInfo->m_arReportTitles[max(0, m_pWoormInfo->m_nNextReport - 1)];
		}
	}

	if (str.IsEmpty() && m_pDocProperties)
	{
		ASSERT_VALID(m_pDocProperties);
		if (!m_pDocProperties->m_strTitle.IsEmpty())
		str = AfxLoadReportString(m_pDocProperties->m_strTitle, this);
	}

	if (str.IsEmpty())
		str = ::GetName(m_strPathName);

	//----------------
	if (!AfxGetPathFinder()->IsCustomPath(m_strPathName))
	{
		if (str.IsEmpty())
		{
			if (m_pCallerDocument)
			{
				ASSERT_VALID(m_pCallerDocument);
				str = cwsprintf(_TB("Radar of %s"), m_pCallerDocument->GetTitle());
			}
		}
		else
			str += _T(" - ") + _TB("Standard");
	}
	else
	{
		if (AfxGetPathFinder()->GetPosTypeFromPath(m_strPathName) == CPathFinder::ALL_USERS)
			str += _T(" - ") + _TB("All Users");
		else
			str += _T(" - ") + AfxGetPathFinder()->GetUserNameFromPath(m_strPathName);
	}
	
	SetTitle(str);
	//----------------

	// Set Tabbed toolBar title
	if (AfxGetThemeManager()->IsToolbarInfinity())
	{
		CTBTabbedToolbar* pTabbed = GetWoormFrame()->GetTabbedToolBar();
		if (pTabbed)
		{
			pTabbed->AddDocumentTitle(str);
		}
	}

	if (GetFirstView() && ::IsWindow(GetFirstView()->m_hWnd))
	{
		CFrameWnd* pFr = GetFirstView()->GetParentFrame();
		ASSERT_VALID(pFr);
		if (pFr && ::IsWindow(pFr->m_hWnd))
		{
			HICON hi = ::LoadWalkIcon(idi);
			SET_ICON(pFr->m_hWnd, hi);
			::DeleteObject(hi);
		}
	}
}

//------------------------------------------------------------------------------
void CWoormDocMng::UnparseWoorm (const CString& strFile, BOOL bOnlyGraphInfo)
{   
	ViewUnparser ofile(strFile);
		ofile.m_bSaveAsWithCurrentLanguage	= m_bSaveAsWithCurrentLanguage;
		ofile.m_strOriginFileName			= m_strOriginFileName;
		ofile.m_strOriginDictionaryPath		= m_strOriginDictionaryPath;

	UnparseHeadData		(ofile, bOnlyGraphInfo);

	if (!m_Template.m_bIsSavingTemplate)
		UnparseProperties	(ofile);

	UnparsePageInfo		(ofile);
				
	if (!m_Template.m_bIsSavingTemplate)
	{
		m_pOptions->Unparse(ofile);
	}

	// scrive gli stili dei font se sono stati modificati (ignorando la release)
	FontsParser aFontsParser;
	if (m_pFontStyles->CheckFontTable(CTBNamespaceArray(), GetNamespace()))
		aFontsParser.Unparse(GetNamespace(), FontStyle::FROM_WOORM, FontStyleTablePtr(m_pFontStyles, FALSE), ofile);
	
	if (!m_Template.m_bIsSavingTemplate)
	{
		// scrive gli stili dei formattatori se sono stati modificati (ignorando la release)
		FormatsParser aFormatsParser;
		if (m_pFormatStyles->CheckFormatTable(CTBNamespaceArray(), GetNamespace()))
			aFormatsParser.Unparse (GetNamespace(), Formatter::FROM_WOORM, FormatStyleTablePtr(m_pFormatStyles, TRUE), ofile);
	}

	// write all elements
	UnparseLayouts(ofile);

	if (!m_Template.m_bIsSavingTemplate)
		m_arWoormLinks.Unparse(ofile);
	
	// unparse only graphic information if needed
	if (!bOnlyGraphInfo)
	{
		m_pEditorManager->Unparse(ofile);
	}
	else if (!m_Template.m_bIsSavingTemplate)
	{
		ofile.UnparseTag (T_REPORT);
		ofile.UnparseBegin(FALSE);
		CString strRepName = _T("NoName");
		ofile.UnparseID		(strRepName,	TRUE);
	
		m_ViewSymbolTable.UnparseVariables(ofile);

		ofile.UnparseEnd();
	}
}

///////////////////////////////////////////////////////////////////////////////

//------------------------------------------------------------------------------
void CWoormDocMng::OnToggleCreateSchema1 ()
{

	if (
			m_pOptions &&
			m_bAllowEditing  &&
			!m_bPlayback  &&
			m_Creating == NONE && 
			!IsReportRunning()  &&
			AfxIsActivated(CLIENTNET_APP, MAGICPANE_MOD)
		)
	{
		m_pOptions->m_bCreateXsd1 = ! m_pOptions->m_bCreateXsd1;
		if (m_pOptions->m_bCreateXsd1)
			m_pOptions->m_bCreateXsd2 = FALSE;
		SetModifiedFlag	();
	}
}

//------------------------------------------------------------------------------
void CWoormDocMng::OnToggleCreateSchema2 ()
{

	if (
			m_pOptions &&
			m_bAllowEditing  &&
			!m_bPlayback  &&
			m_Creating == NONE && 
			!IsReportRunning()  &&
			AfxIsActivated(CLIENTNET_APP, MAGICPANE_MOD)
		)
	{
		m_pOptions->m_bCreateXsd2 = ! m_pOptions->m_bCreateXsd2;
		if (m_pOptions->m_bCreateXsd2)
			m_pOptions->m_bCreateXsd1 = FALSE;
		SetModifiedFlag	();
	}
}

//------------------------------------------------------------------------------
void CWoormDocMng::OnUpdateCreateSchema1 (CCmdUI* pCmdUI)
{	
	BOOL bOk = 
		(
			m_pOptions &&
			m_bAllowEditing  &&
			!m_bPlayback  &&
			m_Creating == NONE && 
			!IsReportRunning() &&
			AfxIsActivated(CLIENTNET_APP, MAGICPANE_MOD)
		);

	pCmdUI->SetCheck(m_pOptions && m_pOptions->m_bCreateXsd1);
	pCmdUI->Enable(bOk);
}

//------------------------------------------------------------------------------
void CWoormDocMng::OnUpdateCreateSchema2 (CCmdUI* pCmdUI)
{	
	BOOL bOk = 
		(
			m_pOptions &&
			m_bAllowEditing  &&
			!m_bPlayback  &&
			m_Creating == NONE && 
			!IsReportRunning() &&
			AfxIsActivated(CLIENTNET_APP, MAGICPANE_MOD)
		);

	pCmdUI->SetCheck(m_pOptions && m_pOptions->m_bCreateXsd2);
	pCmdUI->Enable(bOk);
}
//------------------------------------------------------------------------------
void CWoormDocMng::GetSchema(CPathFinder::PosType	ePosType, const CString& strUser, const CTBNamespace& ns, CXSDGenerator*& XMLSchema)
{
	XMLSchema = NULL;
	if (!PrepareRunEngine (this->m_strPathName))
	{
		ASSERT_TRACE(FALSE, _T("Failure to parse the report\n"));
		return;
	}

	CString strName = ns.GetObjectName(); 
	strName = strName.Left(strName.ReverseFind('.')) ;
	CString strTargetNamespace;
	switch(ePosType)
	{
		case CPathFinder::USERS:
		{
			strTargetNamespace.Format
			(
				_T("http://www.microarea.it/Schema/2004/Smart/%s/%s/Users/%s/%s.xsd"), 
				ns.GetApplicationName(), ns.GetModuleName(), 
				strUser,
				strName
			);
			break;
		}
		case CPathFinder::ALL_USERS:
		{
			strTargetNamespace.Format
			(
				_T("http://www.microarea.it/Schema/2004/Smart/%s/%s/AllUsers/%s.xsd"), 
				ns.GetApplicationName(), ns.GetModuleName(), 
				strName
			);
			break;
		}
		case CPathFinder::STANDARD:
		{
			strTargetNamespace.Format
			(
				_T("http://www.microarea.it/Schema/2004/Smart/%s/%s/%s.xsd"), 
				ns.GetApplicationName(), ns.GetModuleName(), 
				strName
			);
			break;
		}
		default:
			ASSERT_TRACE(FALSE, cwsprintf(_T("Wrong namespace: %s\n"), ns.ToString()));
			return;
	}

	XMLSchema = new CXSDGenerator (strTargetNamespace, TRUE, strName);

	CString strNs = ns.ToString();
	strNs = strNs.Left(strNs.ReverseFind('.'));

	XMLSchema->BeginComplexElement(strName);	//Report Name
		XMLSchema->InsertAttribute(_T("tbNamespace"), SCHEMA_XSD_DATATYPE_STRING_VALUE, _T(""), (LPCTSTR)strNs); 

		GetSchemaObjects(XMLSchema, m_pEditorManager->GetSymTable());

		m_pEditorManager->GetSchema(XMLSchema); 

		XMLSchema->BeginComplexElement(_T("CDiagnostic"), _T("0"), _T("1"));
			XMLSchema->BeginComplexElement(_T("Errors"), _T("0"), _T("1"));
				XMLSchema->BeginComplexElement(_T("Error"), _T("0"), SCHEMA_XSD_UNBOUNDED_VALUE);
					XMLSchema->InsertElement(_T("Code"), 		SCHEMA_XSD_DATATYPE_INT_VALUE,		_T("0"), _T("1")); 
					XMLSchema->InsertElement(_T("Source"),		SCHEMA_XSD_DATATYPE_STRING_VALUE,	_T("0"), _T("1")); 
					XMLSchema->InsertElement(_T("Message"),		SCHEMA_XSD_DATATYPE_STRING_VALUE,	_T("0"), _T("1")); 
				XMLSchema->EndComplexElement();  
			XMLSchema->EndComplexElement();  

			XMLSchema->BeginComplexElement(_T("Warnings"), _T("0"), _T("1"));
				XMLSchema->BeginComplexElement(_T("Warning"), _T("0"), SCHEMA_XSD_UNBOUNDED_VALUE);
					XMLSchema->InsertElement(_T("Code"), 		SCHEMA_XSD_DATATYPE_INT_VALUE,		_T("0"), _T("1")); 
					XMLSchema->InsertElement(_T("Source"),		SCHEMA_XSD_DATATYPE_STRING_VALUE,	_T("0"), _T("1")); 
					XMLSchema->InsertElement(_T("Message"),		SCHEMA_XSD_DATATYPE_STRING_VALUE,	_T("0"), _T("1")); 
				XMLSchema->EndComplexElement();  
			XMLSchema->EndComplexElement();  
		XMLSchema->EndComplexElement();  

	XMLSchema->EndComplexElement();	//Report Name
}

//------------------------------------------------------------------------------
CString CWoormDocMng::GetReportSchemaString(const CString& forUser)
{
	ForceEngineParse();

	CString result = GetSchemaString(forUser);

	return result;

}
//------------------------------------------------------------------------------
void CWoormDocMng::GetSchema(CString strFile)
{
	CTBNamespace ns = AfxGetPathFinder()->GetNamespaceFromPath(strFile);
	if (!ns.IsValid())
	{
		TRACE(_T("Path %s without namespace\n"), strFile);
		ASSERT(FALSE);
		return;
	}
	
	CPathFinder::PosType ePosType = AfxGetPathFinder()->GetPosTypeFromPath(strFile);
	CString strUser; 
	if (ePosType == CPathFinder::USERS)
		strUser = AfxGetPathFinder()->GetUserNameFromPath(strFile);

	CXSDGenerator* XMLSchema = NULL;
	GetSchema(ePosType, strUser, ns, XMLSchema);
	if (XMLSchema)
	{
		XMLSchema->SaveXMLFile((LPCTSTR)(strFile.Left(strFile.ReverseFind('.')) + _T(".xsd")), TRUE);
		delete XMLSchema;
	}
}

//------------------------------------------------------------------------------
CString CWoormDocMng::GetSchemaString(LPCTSTR pszTargetUser/*=NULL*/)
{
	CTBNamespace ns = GetNamespace();
	CString strUser (pszTargetUser ? pszTargetUser : AfxGetThreadContext()->GetSessionLoginName());

	CPathFinder::PosType ePosType;
	if (strUser.CompareNoCase(szAllUserDirName) == 0)
		ePosType = CPathFinder::ALL_USERS;
	else
	{
		if (strUser.CompareNoCase(szStandard) == 0)
			ePosType = CPathFinder::STANDARD;
		else
			ePosType = CPathFinder::USERS;
	}

	CString result;
	CXSDGenerator* XMLSchema = NULL;
	GetSchema(ePosType, strUser, ns, XMLSchema);
	if (XMLSchema)
	{
		XMLSchema->GetSchema()->GetXML(result);
		delete XMLSchema;
	}

	return result;
}

//------------------------------------------------------------------------------
void CWoormDocMng::GetSchemaObjects(CXSDGenerator* pSchema, WoormTable* pRst)
{
	pSchema->BeginComplexElement(_T("ReportData"), _T("0"), _T("1"));	//Report

		int ub = GetObjects().GetUpperBound();
		for (int i =0; i <= ub; i++)
		{
			BaseObj* pObj = GetObjects()[i];
			if (pObj)
				pObj->GetSchema(pSchema, pRst);
		}

	pSchema->EndComplexElement();	//ReportData
}

//------------------------------------------------------------------------------
BOOL CWoormDocMng::GetAskDialogsParameters(CArray<CExportDataNetAskDialog*>& m_arAskDialogs)
{
	if (!m_pEngine || !m_pEngine->m_pMainEngine)
		return FALSE;

	RepEngine* pRepEngine = m_pEngine->m_pMainEngine;
	AskDialogInputMng aAskDialogInputMng(&pRepEngine->GetAskingRules(), &pRepEngine->GetSymTable(), NULL);
	InputMng aAskDlg(pRepEngine->GetCallerDoc());

	//ciclo sulle askdialog dell'engine
	for (int i = 0; i < pRepEngine->GetAskingRules().GetCount(); i++)
	{
		if (pRepEngine->GetAskingRules().GetAskDialog(i)->IsOnAsk())
			continue;

		if (!aAskDialogInputMng.LoadInputMng(&aAskDlg, pRepEngine->GetAskingRules().GetAskDialog(i)))
			return FALSE;

		CExportDataNetAskDialog* dialog = new CExportDataNetAskDialog();
		aAskDlg.GetXmlParameters(*dialog);
		m_arAskDialogs.Add(dialog);

	}

	return TRUE;
}

///////////////////////////////////////////////////////////////////////////////

//------------------------------------------------------------------------------
void CWoormDocMng::UnparseHeadData(ViewUnparser& ofile, BOOL bOnlyGraphInfo)
{
	// calculate  frame dimension for saving them
	CWoormFrame* pParentFrame = GetWoormFrame ();
	CRect rect;
	pParentFrame->GetWindowRect(rect);

	// write header and m_nRelease number
	ofile.UnparseHeader ();
	if (m_bNoWeb) 
		ofile.UnparseTag    (T_NO_WEB, FALSE);
	ofile.UnparseTag    (T_RELEASE,	FALSE);
	ofile.UnparseInt    (m_nRelease, FALSE);
	if (m_nModifyRelease > 0) 
	{
		ofile.UnparseTag    (T_COMMA,	FALSE);
		ofile.UnparseInt    (m_nModifyRelease, FALSE);
	}
	ofile.UnparseBlank  ();
									
	// write window dimension
	if (pParentFrame->IsIconic())		ofile.UnparseTag    (T_MINIMIZED, FALSE);
	else if (pParentFrame->IsZoomed())	ofile.UnparseTag    (T_MAXIMIZED, FALSE);
	else								ofile.UnparseRect   (T_RECT, rect, FALSE);

	// signal that only graph info will be saved
	if (bOnlyGraphInfo)
		ofile.UnparseTag (T_ONLY_GRAPH, FALSE);

	if (!m_Template.m_bIsSavingTemplate && !m_Template.m_sNsTemplate.IsEmpty())
	{
		ofile.UnparseTag	(T_TEMPLATE, FALSE);
		ofile.UnparseString (m_Template.m_sNsTemplate, FALSE);
	}
	else if (m_Template.m_bIsTemplate)
		ofile.UnparseTag	(T_TEMPLATE, FALSE);

	ofile.UnparseCrLf();
}

//------------------------------------------------------------------------------
void CWoormDocMng::UnparseProperties(ViewUnparser& ofile)
{
	if (m_pDocProperties && !m_pDocProperties->IsEmpty())
	{
		m_pDocProperties->Unparse(ofile);
		ofile.UnparseCrLf();
	}
}	

//------------------------------------------------------------------------------
void CWoormDocMng::UnparseLayouts (ViewUnparser& ofile)
{
	m_Layouts.EnumObjects(&CWoormDocMng::RemoveInheritObjects);
	
	SortLayoutObjectsOnPosition();

	BOOL bThereIsTemplate = FALSE;
	CString strName; CObject* pObj = NULL;
	for (POSITION pos = m_Layouts.GetStartPosition(); pos != NULL; pObj = NULL, strName.Empty())
	{
		m_Layouts.GetNextAssoc (pos, strName, pObj);
		CLayout* pObjects = (CLayout*) pObj;

		if (
			pObjects->GetCount() <= pObjects->m_nCountAutoObjects &&
			pObjects->m_strLayoutName.CompareNoCase(REPORT_DEFAULT_LAYOUT_NAME)
			)
			continue;
		//----

		ofile.UnparseTag	(T_PAGE_LAYOUT, FALSE);
		ofile.UnparseString	(pObjects->m_strLayoutName, !pObjects->m_bInvertOrientation);
		if (pObjects->m_bInvertOrientation)
			ofile.UnparseTag(T_INVERT_ORIENTATION);

		ofile.UnparseBegin	();

		if (!m_Template.m_bIsSavingTemplate)
			RemoveTemplateStyles();

		// write all elements CLayout* pObjects
		pObjects->Unparse(ofile, m_Template.m_bIsSavingTemplate, bThereIsTemplate);

		if (!m_Template.m_bIsSavingTemplate)
			ApplyTemplateStyles();

		ofile.UnparseEnd();
	}
	if (bThereIsTemplate && m_Template.m_bIsSavingTemplate && m_Template.m_pWoormTpl)
		UnparseInheritTemplateObjects(ofile);

	m_Layouts.EnumObjects(&CWoormDocMng::AddInheritObjects);
}

//------------------------------------------------------------------------------
void CWoormDocMng::UnparseInheritTemplateObjects (ViewUnparser& ofile)
{
	ASSERT(m_Template.m_bIsSavingTemplate && m_Template.m_pWoormTpl && m_pEditorManager);

	int offset = m_pEditorManager->GetNextId() + 100;

	CString strName; CObject* pObj = NULL;

	for (POSITION pos = m_Template.m_pWoormTpl->m_Layouts.GetStartPosition(); pos != NULL; pObj = NULL, strName.Empty())
	{
		m_Template.m_pWoormTpl->m_Layouts.GetNextAssoc (pos, strName, pObj );
		CLayout* pObjects = (CLayout*) pObj;
		//----

		ofile.UnparseTag	(T_PAGE_LAYOUT, FALSE);
		ofile.UnparseString	(_T("_TPL_") + strName);
		ofile.UnparseBegin	();

		// write all elements
		int ub = pObjects->GetUpperBound();
		for (int i = 0; i <= ub; i++)
		{
			BaseObj* pObj = (*pObjects)[i];

			if (pObj->IsKindOf(RUNTIME_CLASS(Table)))
			{
				if (((Table*)pObj)->IsTemplateOverridden())
					continue;

				Table* pTable = new Table (*(Table*)pObj);
				pTable->PurgeTemplateColumns();

				pTable->RenameAlias(offset);

				pObj = pTable;
			} 
			else if (pObj->IsKindOf(RUNTIME_CLASS(BaseRect)))
			{
				if (!((BaseRect*)pObj)->IsTemplate())
					continue;
				if (((BaseRect*)pObj)->IsTemplateOverridden())
					continue;

				((BaseRect*)pObj)->RenameAlias(offset);
			}

			pObj->Unparse(ofile);
		}
		ofile.UnparseEnd();
	}
}

// write document dimension
//------------------------------------------------------------------------------
void CWoormDocMng::UnparsePageInfo (ViewUnparser& ofile)
{
	if (!m_PageInfo.IsDefault())
	{
		m_PageInfo.	Unparse(ofile);
		ofile.		UnparseCrLf();
	}
}

//------------------------------------------------------------------------------
void CWoormDocMng::ShowValue()
{
	// restore value show status in ini strFile
	if (m_pWoormIni->m_Show != WoormIni::VALUE)
	{
		m_pWoormIni->m_Show = WoormIni::VALUE;
		m_pWoormIni->WriteWoormSettings();

		FormatAll ();
	}
}

//------------------------------------------------------------------------------
void CWoormDocMng::FormatAll()
{
	int ub = GetObjects().GetUpperBound();
	for (int i =0; i <= ub; i++)
		(GetObjects()[i])->Format();
		
	Invalidate		();
	UpdateWindow	();
}

//------------------------------------------------------------------------------
void CWoormDocMng::OnClearAll()
{                                                                       
	// ask for Clear confirm (do non check for unattended status: this can be only in interactive mode!)
	if (AfxMessageBox(_TB("Confirm delete ALL objects"), MB_YESNO) != IDYES) return;

	// already empty; do nothing
	if (!GetObjects().GetSize()) return;
					 
	// deselect all eventual multiple selected GetObjects()                            
	if (m_pMultipleSelObj) OnDeselectAll();

	// iterate for delete all GetObjects()
	int ub = GetObjects().GetUpperBound();
	for (int i = ub; i >= 0; i--)
	{
		BaseObj* pObj = GetObjects()[i];
		if (pObj && pObj->DeleteEditorEntry())
			GetObjects().RemoveAt(i);
	}

	m_pCurrentObj = NULL;
	Invalidate ();
	m_pActiveRect->Clear();
	UpdateWindow ();
}

//------------------------------------------------------------------------------
void CWoormDocMng::MultipleSelect(CRect includeRect, BOOL bAll)
{
	includeRect = CRect(UnScalePix(includeRect.left), UnScalePix(includeRect.top), UnScalePix(includeRect.right), UnScalePix(includeRect.bottom));

	// no GetObjects() so do nothing
	if (GetObjects().GetSize() == 0) return;

	// create m_pMultipleSelObj if they don't already exist and show 
	// align PaletteBar
	if (!m_pMultipleSelObj) 
		m_pMultipleSelObj = new SelectionRect (this);
	
	// disable current object active enhancement during multiple selection status
	m_pActiveRect->Clear();
	
	// Clear for recalculate included GetObjects()
	m_pMultipleSelObj->ClearMultipleSelObjects();

	// add all object included in multiple selection
	CRect rect (includeRect);
	for (int i = 0; i <= GetObjects().GetUpperBound(); i++)
	{
		BaseObj* pObj = GetObjects()[i];
		if (pObj->m_nLayer != m_nCurrentLayer)
			continue;

		CRect rectObj (pObj->GetBaseRect());

		// l'oggetto deve essere interamente incluso nel rettangolo di drag
		rect.IntersectRect(&includeRect, rectObj);

		if (bAll || rect == rectObj)
		{
			if (pObj->GetInternalID() < SpecialReportField::REPORT_LOWER_SPECIAL_ID)
				m_pMultipleSelObj->AddToSelectedObjects(pObj, pObj->GetBaseRect());
		}
	}

	// se la selezione fallische allora rimuove anche il vettore
	if (!m_pMultipleSelObj->GetSize() || m_pMultipleSelObj->GetSize() == 0)
	{
		delete m_pMultipleSelObj;
		m_pMultipleSelObj = NULL;
	}
	else if (GetWoormFrame()->m_pObjectPropertyView)
		//altrimenti se non è fallita la multiselezione carico la property grid per gli oggetti selezionati
		GetWoormFrame()->m_pObjectPropertyView->LoadMultipleSelectionProperties(m_pMultipleSelObj);
	// refresh video status
	UpdateWindow();
}

//------------------------------------------------------------------------------
void CWoormDocMng::OnSelectAll()
{
	MultipleSelect(CRect(0,0,0,0), TRUE);
}

//------------------------------------------------------------------------------
void CWoormDocMng::OnDeselectAll()
{
	if (m_pMultipleSelObj)
	{
		// destroy multiple selection
		m_pActiveRect->Clear();
		UpdateWindow();

		delete m_pMultipleSelObj;
		m_pMultipleSelObj = NULL;
	}
}

//------------------------------------------------------------------------------
void CWoormDocMng::ReadPrevPage (BOOL goto_first_page)
{
	// cannot process page moving if RDE is closed.
	if (m_pRDEmanager->IsClosed() || m_pRDEmanager->IsFirstPage()) return;

	if (!m_ReadingPage.IsLocked())
	{
		if (goto_first_page)
			m_pRDEmanager->SeekFirstPage();
		else
			m_pRDEmanager->SeekPrevPage();

		// read real data
		ReadPage();

		m_ReadingPage.Unlock();
	}
}

//------------------------------------------------------------------------------
void CWoormDocMng::ReadNextPage (BOOL bGotoLastPage)
{
	// cannot process page moving if RDE is closed.
	if (m_pRDEmanager->IsClosed() || m_pRDEmanager->IsLastPage()) return;

	if (!m_ReadingPage.IsLocked())
	{
		if (bGotoLastPage)
			m_pRDEmanager->SeekLastPage();
		else
			m_pRDEmanager->SeekNextPage();

		// read real data
		ReadPage();

		m_ReadingPage.Unlock();
	}
}

//------------------------------------------------------------------------------
void CWoormDocMng::ReadSelectedPage (int page, BOOL bUpdateWnd /* = TRUE*/)
{
	// cannot process page moving if RDE is closed.
	if (m_pRDEmanager->IsClosed()) return;

	if (!m_ReadingPage.IsLocked())
	{
		m_pRDEmanager->SeekPage(page);
		ReadPage(bUpdateWnd);

		m_ReadingPage.Unlock();
	}
}

//------------------------------------------------------------------------------
void CWoormDocMng::ReadPage(BOOL bUpdateWnd /* = TRUE*/)
{
	if (m_pRDEmanager->IsClosed()) return;
	BOOL bPrepared = FALSE;

	// read page
	BOOL bLoop = TRUE;
	while (bLoop)
	{
		switch (m_pRDEmanager->LookAhead())
		{
			case RDEManager::INTERNAL_ID:
			{
				bLoop = ReadDataFromId(bPrepared);
				break;
			}

			case RDEManager::ID_COMMAND:
			{
				// manage also eof condition
				bLoop = ReadCommandFromId();
				break;
			}

			case RDEManager::GEN_COMMAND:
			{
				bLoop = ReadGeneralCommand(bUpdateWnd); // FALSE if end page
				break;
			}

			default :
			{
				// probably there was same strange error (note : sharing error
				// already notified by rde manager
				if (m_pRDEmanager->GetRDEStatus() != RDEManager::SHARING_VIOLATION)
					Message(_TB("Error during the ID reading or wrong command from engine."));

				bLoop = FALSE;
				break;
			}

		}   // end switch LookAhead
	}   // end while !eof

	UpdateViewSymbolTable();

	//for (int i = 0; i < GetObjects().GetSize(); i++)
	//{
	//	BaseObj* pB = (BaseObj*) GetObjects().GetAt(i);
	//	if (pB->IsKindOf(RUNTIME_CLASS(Table)))
	//	{
	//		Table* pT = (Table*) pB;
	//		if (pT->m_pChart && pT->m_nChartType)
	//		{
	//			pT->SyncChart();
	//		}
	//	}
	//}

				   
	// repaint all areas invalidated from RDE readolumn
	if (bUpdateWnd)
	{
		// se c'e' almeno un campo trasparente deve invalidare tutto
		// per evitare che il cambio dei dati scriva uno sopra all'altro
		// senza cancellare il dato precedente
		if (AlmostOneTrasparent())
			Invalidate();
		else
			UpdateWindow();
	}
}

//------------------------------------------------------------------------------
int	CWoormDocMng::GetCurrentPage ()
{
	if (m_pRDEmanager->IsClosed())
		return 0;
	
	return m_pRDEmanager->CurrPageRead();
}

//------------------------------------------------------------------------------
BOOL CWoormDocMng::AlmostOneTrasparent()
{
	return TRUE;
	//for (int i = 0; i <= GetObjects().GetUpperBound(); i++)
	//{
	//	BaseObj* pObj = GetObjects()[i];
	//	if (pObj->m_bTransparent)
	//		return TRUE;
	//}

	//return FALSE;
}

// onGotoPage:
//
//      page range is fro 0 to max value so be carefully on count it
//
//------------------------------------------------------------------------------
void CWoormDocMng::OnGotoPage()
{
	// cannot process page moving if RDE is closed.
	if (m_pRDEmanager->IsClosed()) return;

	int page = m_pRDEmanager->CurrPageRead() + 1;
	CGotoPageDlg dialog(page, m_pRDEmanager->LastPage() + 1);

	if (dialog.DoModal() == IDOK)
		ReadSelectedPage(page - 1);
}

//------------------------------------------------------------------------------
void CWoormDocMng::InitializeDisplayStatus()
{
	int ub = GetObjects().GetUpperBound();
	for (int i = ub; i >= 0; i--)
	{
		BaseObj* pObj = GetObjects()[i];

		if (pObj->IsKindOf(RUNTIME_CLASS(Table)) || pObj->IsKindOf(RUNTIME_CLASS(Repeater)))
			pObj->ResetCounters();

		pObj->DisableData();
	}
}

//------------------------------------------------------------------------------
int CWoormDocMng::GetPosition(int nID)
{
	for (int i = 0; i <= GetObjects().GetUpperBound(); i++)
		if (GetObjects()[i]->GetInternalID() == nID)
			return i + 1;

	return -1;
}

//------------------------------------------------------------------------------
void CWoormDocMng::CheckEmailParameter(FieldRect* p)
{
	if (p->IsEmailParameter())
	{
		CString str = p->GetText();
		if (!str.IsEmpty())
		{
			switch (p->GetEmailParameter())
			{
				case FieldRect::EP_From:
					m_Email.SetFrom(str);
					break;
				case FieldRect::EP_To:
					m_Email.SetTo(str, p->GetAppendMailPart());
					break;
				case FieldRect::EP_To_by_Certified:
					m_Email.SetTo(str, p->GetAppendMailPart(), CMapiMessage::TAG_CERTIFIED);
					break;
				case FieldRect::EP_Cc:
					m_Email.SetCc(str, p->GetAppendMailPart());
					break;
				case FieldRect::EP_Bcc:
					m_Email.SetBcc(str, p->GetAppendMailPart());
					break;
				case FieldRect::EP_Subject:
					m_Email.SetSubject(str, p->GetAppendMailPart(), TRUE);
					break;
				case FieldRect::EP_Body:
					m_Email.SetBody(str, p->GetAppendMailPart(), TRUE);
					break;
				case FieldRect::EP_Attachment:
					AddEmailAttachments(str, _T(""), p->GetAppendMailPart());
					break;
				case FieldRect::EP_RecipientIdentity:
					m_Email.SetIdentity(str);
					break;
				case FieldRect::EP_AttachmentReportName:
					m_Email.SetAttachmentReportName(str);
					break;
				case FieldRect::EP_TemplateFileName:
					m_Email.SetTemplateFileName(str);
					break;

				case FieldRect::EP_Fax:
				{
					if (m_Email.m_PostaLiteMsg.GetCount() == 0)
						m_Email.m_PostaLiteMsg.Add(str, L"");
					else
					{
						CPostaLiteAddress* pAddr = (CPostaLiteAddress*)m_Email.m_PostaLiteMsg.GetAt(m_Email.m_PostaLiteMsg.GetUpperBound());
						pAddr->m_sFax = str;
					}
					break;
				}
				case FieldRect::EP_Addressee:
				{
					if (m_Email.m_PostaLiteMsg.GetCount() == 0)
						m_Email.m_PostaLiteMsg.Add(L"", L"");

					CPostaLiteAddress* pAddr = (CPostaLiteAddress*)m_Email.m_PostaLiteMsg.GetAt(m_Email.m_PostaLiteMsg.GetUpperBound());
					pAddr->m_sAddressee = str;
					break;
				}
				case FieldRect::EP_Address:
				{
					if (m_Email.m_PostaLiteMsg.GetCount() == 0)
						m_Email.m_PostaLiteMsg.Add(L"", L"");

					CPostaLiteAddress* pAddr = (CPostaLiteAddress*)m_Email.m_PostaLiteMsg.GetAt(m_Email.m_PostaLiteMsg.GetUpperBound());
					pAddr->m_sAddress = str;
					break;
				}
				case FieldRect::EP_City:
				{
					if (m_Email.m_PostaLiteMsg.GetCount() == 0)
						m_Email.m_PostaLiteMsg.Add(L"", L"");

					CPostaLiteAddress* pAddr = (CPostaLiteAddress*)m_Email.m_PostaLiteMsg.GetAt(m_Email.m_PostaLiteMsg.GetUpperBound());
					pAddr->m_sCity = str;
					break;
				}
				case FieldRect::EP_County:
				{
					if (m_Email.m_PostaLiteMsg.GetCount() == 0)
						m_Email.m_PostaLiteMsg.Add(L"", L"");

					CPostaLiteAddress* pAddr = (CPostaLiteAddress*)m_Email.m_PostaLiteMsg.GetAt(m_Email.m_PostaLiteMsg.GetUpperBound());
					pAddr->m_sCounty = str;
					break;
				}
				case FieldRect::EP_Country:
				{
					if (m_Email.m_PostaLiteMsg.GetCount() == 0)
						m_Email.m_PostaLiteMsg.Add(L"", L"");

					CPostaLiteAddress* pAddr = (CPostaLiteAddress*)m_Email.m_PostaLiteMsg.GetAt(m_Email.m_PostaLiteMsg.GetUpperBound());
					pAddr->m_sCountry = str;
					break;
				}
				case FieldRect::EP_ZipCode:
				{
					if (m_Email.m_PostaLiteMsg.GetCount() == 0)
						m_Email.m_PostaLiteMsg.Add(L"", L"");

					CPostaLiteAddress* pAddr = (CPostaLiteAddress*)m_Email.m_PostaLiteMsg.GetAt(m_Email.m_PostaLiteMsg.GetUpperBound());
					pAddr->m_sZipCode = str;
					break;
				}
				case FieldRect::EP_ISOCode:
				{
					if (m_Email.m_PostaLiteMsg.GetCount() == 0)
						m_Email.m_PostaLiteMsg.Add(L"", L"");

					CPostaLiteAddress* pAddr = (CPostaLiteAddress*)m_Email.m_PostaLiteMsg.GetAt(m_Email.m_PostaLiteMsg.GetUpperBound());
					pAddr->m_sISOCode = str;
					break;
				}

				case FieldRect::EP_DeliveryType:
				{
					if (m_Email.m_PostaLiteMsg.GetCount() == 0)
						m_Email.m_PostaLiteMsg.Add(L"", L"");

					CPostaLiteAddress* pAddr = (CPostaLiteAddress*)m_Email.m_PostaLiteMsg.GetAt(m_Email.m_PostaLiteMsg.GetUpperBound());
					pAddr->m_deDeliveryType = _wtoi(str);
					break;
				}
				case FieldRect::EP_PrintType:
				{
					if (m_Email.m_PostaLiteMsg.GetCount() == 0)
						m_Email.m_PostaLiteMsg.Add(L"", L"");

					CPostaLiteAddress* pAddr = (CPostaLiteAddress*)m_Email.m_PostaLiteMsg.GetAt(m_Email.m_PostaLiteMsg.GetUpperBound());
					pAddr->m_dePrintType = _wtoi(str);
					break;
				}
			}
		}
	}
}

//------------------------------------------------------------------------------
BOOL CWoormDocMng::ReadArrayFields(WORD wId)
{
	SymField* pAr = m_ViewSymbolTable.GetFieldByID(wId);
	if (!pAr) 
		return FALSE;
	RDEData tmpRDEData;

	if (!m_pRDEmanager->GetDataValueUnlimited(tmpRDEData))
		return FALSE;

	DataStr ds; ds.Assign(tmpRDEData);
	pAr->GetRepData()->Assign(ds.GetString());

	return TRUE;
}

//------------------------------------------------------------------------------
BOOL CWoormDocMng::ReadDataFromId(BOOL& bPrepared)
{
	WORD wId = m_pRDEmanager->GetId();

	if (wId != SpecialReportField::ID.LAYOUT && !bPrepared)
	{
		//MANCA il valore del Layout name: assumo che l'RDE è precedente alla 3.5
		PrepareLayout();

		bPrepared = TRUE;
	}

	int ub = GetObjects().GetUpperBound();
	for (int i = 0; i <= ub; i++)
	{
		BaseObj* pObj = GetObjects()[i];
		
		if (pObj->AssignData(wId, m_pRDEmanager))
		{
			if (pObj->IsKindOf(RUNTIME_CLASS(FieldRect)))
			{
				FieldRect* p = (FieldRect*) pObj;

				if (wId == SpecialReportField::ID.LAYOUT)
				{
					//Imposto il nome del Layout per la pagina (è sicuramente il primo campo)
					m_dsCurrentLayoutView.Assign(*(p->GetRDEValue()));
			
					if (!bPrepared)
					{
						m_dsCurrentLayoutView.Assign(*(p->GetRDEValue()));

						PrepareLayout();
						//evito che se il campo viene displayato automaticamente più volte mi perdo parte della pagina
						bPrepared = TRUE;
					}
					else
					{
						DataStr ds; ds.Assign(*(p->GetRDEValue()));
						ASSERT(m_dsCurrentLayoutView == ds);
					}
					return TRUE;
				}

				CheckEmailParameter(p);
			}
			return TRUE;
		}
	}

	// error! no object found but read data for sincronize m_pRDEmanager
	m_pRDEmanager->SkipData();

	//TODO LAYOUT - SPECIAL FIELDS
	if (m_Layouts.GetCount() > 1 || wId >= SpecialReportField::REPORT_LOWER_SPECIAL_ID)
		return TRUE;

	Message(cwsprintf(_TB("No graphic object corresponding to value for alias field {0-%d} "), wId));
	return FALSE;
}

//------------------------------------------------------------------------------
//es: NEXTLINE, INPUT_LOWER_DATA, INPUT_UPPER_DATA, ARRAY_DATA
BOOL CWoormDocMng::ReadCommandFromId()
{
	WORD wId = m_pRDEmanager->GetId();

	int ub = GetObjects().GetUpperBound();
	for (int i = ub; i >= 0; i--)
	{
		BaseObj* pObj = GetObjects()[i];

		if (pObj->ExecCommand(wId, m_pRDEmanager))
			return TRUE;
	}

	if (ReadArrayFields(wId))
		return TRUE;

	// error! no object found but read command for sincronize m_pRDEmanager
	m_pRDEmanager->SkipData();

	//TODO LAYOUT - SPECIAL FIELDS
	if (m_Layouts.GetCount() > 1 || wId >= SpecialReportField::REPORT_LOWER_SPECIAL_ID)
		return TRUE;

	Message(cwsprintf(_TB("No graphic object corresponding to alias field (or table) {0-%d}"), wId));
	return FALSE;
}

//------------------------------------------------------------------------------
BOOL CWoormDocMng::ReadGeneralCommand(BOOL bUpdateWnd /*= FALSE*/)
{
	switch (m_pRDEmanager->GetCommand())
	{
		case RDEManager::END_OF_REPORT:
			return FALSE;

		case RDEManager::NEW_PAGE:
		{
			m_pRDEmanager->SkipData();
			return FALSE;

//TODO LAYOUT - SPECIAL FIELDS
		//case RDEManager::NEW_PAGE_CHANGE_LAYOUT:

			//RDEData rdeData;
			//if (m_pRDEmanager->GetDataValue(rdeData))
			//{
			//	m_dsCurrentLayout.Assign(rdeData);
			//	
			//	PrepareLayout();
			//}
			//return FALSE;
		}

		case RDEManager::MESSAGE_BOX:
		{                     
		if (bUpdateWnd)
		{
			UpdateViewSymbolTable();

			// repaint all areas invalidated from RDE readolumn
			// se c'e' almeno un campo trasparente deve invalidare tutto
			// per evitare che il cambio dei dati scriva uno sopra all'altro
			// senza cancellare il dato precedente
			if (AlmostOneTrasparent())
				Invalidate();
			UpdateWindow();
		}
			RDEData RDEdata;
			m_pRDEmanager->GetDataValue(RDEdata);

		int nIcon = -1; 
		CString message = (LPCTSTR)RDEdata.GetData();
		if (message)
		{
			int iconIndex = message.ReverseFind('|');
			if (iconIndex >= 0)
			{
				CString sIcon = message.Right(message.GetLength() - iconIndex);
				if (sIcon == L"|error")
					nIcon = MB_ICONERROR;
				else if (sIcon == L"|warning")
					nIcon = MB_ICONWARNING;
				else if (sIcon == L"|info")
					nIcon = MB_ICONINFORMATION;
				else if (sIcon == L"|question")
					nIcon = MB_ICONQUESTION;
			}
			if (nIcon >= 0)
				message = message.Left(iconIndex);
			else
				nIcon = MB_ICONINFORMATION;

			Message(message, nIcon);
		}
			return TRUE;
		}
	}
	return TRUE;
}

//------------------------------------------------------------------------------
BOOL CWoormDocMng::PrepareRunEngine (const CString& strFile)
{
	// Clear selection from active object
	OnDeselectAll();
	if (m_pCurrentObj)
	{
		m_pActiveRect->Clear();
		m_pCurrentObj = NULL;
	}

	// reset show of data value (not id or public name)
	ShowValue();

	// activate strFile channel for comunicate data (close if already opened)
	if (!m_pRDEmanager->Create())
		return FALSE;

	// load m_pEngine report and refresh page indicator
	// compile it (don't free internal engine resource)
	//BeginWaitCursor();

	CString error; long line;
	BOOL bOk = m_pEngine->Load(strFile);
	if (!bOk)
		error = _TB("Fails to load report") + ' ' + strFile;
	if (bOk)
		bOk = m_pEngine->Compile(error, line, FALSE);

	//EndWaitCursor();
	   
	// Do not run report when compilation fails
	if (!bOk) 
	{
		if (IsInUnattendedMode())
		{
			if (m_pWoormInfo && m_pWoormInfo->m_bHideFrame)
			{
				m_pWoormInfo->m_arErrors.Add(error);
				m_pWoormInfo->m_nLine = line;
			}
		}
		return FALSE;
	}

	return TRUE;
}

//------------------------------------------------------------------------------
BOOL CWoormDocMng::RunEngine (const CString& strFile)
{
	if (!PrepareRunEngine(strFile))
		return FALSE;

	// l'utente e` riuscito a stoppare il running durante
	// le fasi di compilazione
	if (!m_bEngineRunning)
	{
		m_pEngine->FreeMemory();
		return FALSE;
	}
	// validate run for enable eventual RDE filesave action and page moving
	//
	m_pEditorManager->SetValidRun();

	//-----
	GetObjects().ClearDynamicAttributes();

	CString strOrderBy;
	if (m_pExportData && m_pExportData->IsTableItem() )
	{
		CExportTableItem* pTItem = m_pExportData->GetExportTableItem(); 
		if (pTItem && pTItem->GetSizeSelectedColumns() >0)
		{
			Array* parOrderOnThisColumns = new Array();
			for (int i = 0; i < pTItem->GetSizeSelectedColumns(); i++)
			{
				parOrderOnThisColumns->Add(new COrderColumn(pTItem->m_SelectedCol[i].m_ColID, !pTItem->IsAlternate(i)));
			}

			BOOL bRet = m_pEngine->Run(m_pRDEmanager, parOrderOnThisColumns, &m_strDocumentFilter, m_bUsePrevAskInput, m_pWoormInfo);

			delete parOrderOnThisColumns;
			return bRet;
		}
	}

	//---- OnRun
#ifdef _DEBUG
if (m_pWoormInfo)
{
	CString s = m_pWoormInfo->ToString();
		int i = s.GetLength();
}
#endif

	BOOL bOk = m_pEngine->Run(m_pRDEmanager, NULL, &m_strDocumentFilter, m_bUsePrevAskInput, m_pWoormInfo, &m_ViewSymbolTable);
	
	if (bOk && m_deReportStatus == DataEnum(TAG_REPORT_STATUS, TAG_REPORT_STATUS_SCRIPT_QUIT))
	{
		m_pWoormInfo->m_bCloseOnEndPrint = TRUE;
	}
	return bOk;
}

//------------------------------------------------------------------------------
void CWoormDocMng::CompileEngine (const CString& strFile)
{
	CString error; long line;
	BeginWaitCursor();
	BOOL bOk = m_pEngine->Load(strFile) && m_pEngine->Compile(error,line);
	EndWaitCursor();
	
	if (bOk) 
		Message(_TB("Compiling Ok."), 0,0,0, CMessages::MessageType::MSG_HINT);
	else 
		Message(_TB("Compiling Errors at line:") + cwsprintf(L" %d", line), 0, 0, error, CMessages::MessageType::MSG_ERROR);
}

//------------------------------------------------------------------------------
void CWoormDocMng::OnNewPage()
{
	// only first page must throw by m_pEngine (the other must be throw by user action)
	if  (m_pRDEmanager->CurrPageRead() == -1)
		if (!m_ReadingPage.IsLocked())
		{
			// seek at first page for initializate current page
			m_pRDEmanager->SeekFirstPage();
			ReadPage();

			m_ReadingPage.Unlock();
		}
			
	// show pages on statusbar indicator without use CmdUI interface
	// for optimizing refresh velocity on background processing
	SetPageIndicator ();

	// aggiorna eventuali campi statici (special field)
	UpdateSpecialField();
}

//------------------------------------------------------------------------------
void CWoormDocMng::ExitCreatingMode()
{
	if (m_Creating != NONE)
	{
		m_Creating = NONE;
		ClipCursor(NULL);
		SetModifiedFlag();
	}
}

//------------------------------------------------------------------------------
void CWoormDocMng::ClipCursorToActiveView()
{                                                 
	// free current active object to manage right cursor in m_Creating mode
	m_pCurrentObj = NULL;
	m_pActiveRect->Clear();

	// constrain mouse movement to drawArea rectangle
	CView* pView = GetFirstView();
									
	ASSERT_KINDOF(CWoormView, pView);
	CRect rect;
	pView->GetClientRect(rect);
	pView->ClientToScreen(rect);
	::ClipCursor (rect);

	// set cursor in active view Top left corner 
	SetCursorPos (rect.left, rect.top);
}

//------------------------------------------------------------------------------
void CWoormDocMng::OnAddSqrRect()
{
	if (m_Creating == NONE)
	{
		m_Creating = SQRRECT;
		ClipCursorToActiveView();
	}
}

//------------------------------------------------------------------------------
void CWoormDocMng::OnAddRepeater()
{
	if (m_Creating == NONE)
	{
		m_Creating = REPEATER;
		if (m_pEditorManager->AddRepeater (m_wCreatingId))
			ClipCursorToActiveView();
	}
}

//------------------------------------------------------------------------------
void CWoormDocMng::OnAddChart()
{
	if (m_Creating == NONE)
	{
		m_Creating = CHART;
		if (m_pEditorManager->AddChart(m_wCreatingId))
			ClipCursorToActiveView();
	}
}

//------------------------------------------------------------------------------
void CWoormDocMng::OnAddTextRect()
{
	if (m_Creating == NONE)
	{
//----------------------------
		{
			CRSEditView* pEditView = dynamic_cast<CRSEditView*>(CreateSlaveView(RUNTIME_CLASS(CRSEditView)));
			ASSERT_VALID(pEditView);
			
			BOOL bSaved = FALSE;
			pEditView->LoadElement(&m_strTextField, &bSaved);
			pEditView->DoEvent();
			if (bSaved)
			{
				m_Creating = TEXTRECT;
				ClipCursorToActiveView();
			}
		}
	}
//-----------------------------
}

//-------------------------------------------------------------------  -----------
void CWoormDocMng::OnAddGraphRect()
{
	if (m_Creating != NONE) return;

	CTBNamespace aNamespace (GetNamespace());
	aNamespace.SetObjectName(_T(""));
	aNamespace.SetType(CTBNamespace::IMAGE);

	CTBExplorer aExplorer (CTBExplorer::OPEN, aNamespace);
	aExplorer.SetCanLink();
	if (!aExplorer.Open ())
		return;
	
	CString strPath;	
	aExplorer.GetSelPathElement(strPath);
	
	if (!strPath.IsEmpty())
		m_strBmpName = strPath;
	
	if (!m_strBmpName.IsEmpty())
	{
		if ( _tcsicmp( m_strBmpName.Right(3), _T("EMF") ) == 0 )
			m_Creating = METAFILERECT;
		else
			m_Creating = GRAPHRECT;
	}
	ClipCursorToActiveView();
}

//------------------------------------------------------------------------------
void CWoormDocMng::OnAddFileRect()
{
	if (m_Creating != NONE) return;

	CTBNamespace aNamespace (GetNamespace());
	aNamespace.SetObjectName(_T(""));
	aNamespace.SetType(CTBNamespace::TEXT);

	CTBExplorer aExplorer (CTBExplorer::OPEN, aNamespace);
	aExplorer.SetCanLink();
	if (!aExplorer.Open ())
		return;

	CString strPath;	
	aExplorer.GetSelPathElement(strPath);
	
	if (!strPath.IsEmpty())
		m_strTxtName = strPath;

	// all bOk so proceed to create object
	if (!m_strTxtName.IsEmpty())
		m_Creating = FILERECT;
	ClipCursorToActiveView();
}

//------------------------------------------------------------------------------
BOOL CWoormDocMng::EditorManagerCreateField(int anID, TblRuleData* pTblRule /*= NULL*/)
{
	if (anID == ID_ADD_DATA_FIELD )
		return m_pEditorManager->AddDataField   (m_wCreatingId, m_wCreatingColumnIds, pTblRule);
	if (anID == ID_ADD_EXP_FIELD)
		return m_pEditorManager->AddExpField    (m_wCreatingId);
	if (anID == ID_ADD_HIDDEN_FIELD)
		return m_pEditorManager->AddHiddenField (m_wCreatingId);
	if (anID == ID_ADD_VAR_FIELD)
		return m_pEditorManager->AddVarField    (m_wCreatingId);
	return FALSE;
}

//------------------------------------------------------------------------------
void CWoormDocMng::CreateField(int anID, TblRuleData* pTblRule /*= NULL*/)
{
	if (m_Creating != NONE) return;

	// add object in table column
	if (CurrentIsTable())
	{
		((Table*) m_pCurrentObj)->AddColumn(anID, pTblRule);
		return;
	}

	// add free (not in table) object
	if  (EditorManagerCreateField(anID, pTblRule))
	{
		m_Creating = CWoormDocMng::ObjectType::FIELDRECT;
		ClipCursorToActiveView();
	}
}

//------------------------------------------------------------------------------
void CWoormDocMng::OnAddTable()
{
	if (/*!GetWoormFrame()->m_pObjectPropertyView->ClearPropertyGrid() ||*/ m_wCreatingColumnIds.GetCount()==0)
		return;

	if (m_Creating == NONE)
	{
		m_wCreatingId = 0;
		//m_wCreatingColumnIds.RemoveAll();

		if (m_pEditorManager->AddTableDataField(m_wCreatingId, m_wCreatingColumnIds, TRUE))
		{
			m_Creating = TABLE;
			ClipCursorToActiveView();
		
		}
	}
}

void CWoormDocMng::OnAddTableFromDB()
{
	ASSERT_VALID(GetWoormFrame()->m_pObjectPropertyView);
	GetWoormFrame()->m_pObjectPropertyView->NewDBElement(TRUE);
}

void CWoormDocMng::OnAddTableFromFuncExpr()
{
	ASSERT_VALID(GetWoormFrame()->m_pObjectPropertyView);
	GetWoormFrame()->m_pObjectPropertyView->NewElement(TRUE,FALSE);
}

void CWoormDocMng::OnAddTableFromHiddens()
{
	int hiddenFieldsCount = 0;
	for (int i = 0; i < GetEditorSymTable()->GetSize(); i++)
	{
		WoormField* pF = (WoormField*)GetEditorSymTable()->GetAt(i);
		ASSERT_VALID(pF);

		if (pF->IsColTotal() || pF->IsSubTotal() || pF->IsInput() || pF->GetId() >= SpecialReportField::REPORT_LOWER_SPECIAL_ID)
			continue;
		if (pF->IsHidden())
			hiddenFieldsCount++;
	}
	if (hiddenFieldsCount == 0)
		return;

	ASSERT_VALID(GetWoormFrame()->m_pObjectPropertyView);
	GetWoormFrame()->m_pObjectPropertyView->NewElement(TRUE, TRUE);
}

//-----------------------------------------------------------------------------
void CWoormDocMng::OnCreateDbObj()
{
	ASSERT_VALID(GetWoormFrame()->m_pObjectPropertyView);
	GetWoormFrame()->m_pObjectPropertyView->NewDBElement(FALSE);
}

//-----------------------------------------------------------------------------
void CWoormDocMng::OnCreateFuncExprObj()
{
	ASSERT_VALID(GetWoormFrame()->m_pObjectPropertyView);
	GetWoormFrame()->m_pObjectPropertyView->NewElement(FALSE, FALSE);
}

void CWoormDocMng::OnCreateObjFromHiddenField()
{
	int hiddenFieldsCount = 0;
	for (int i = 0; i < GetEditorSymTable()->GetSize(); i++)
	{
		WoormField* pF = (WoormField*)GetEditorSymTable()->GetAt(i);
		ASSERT_VALID(pF);

		if (pF->IsColTotal() || pF->IsSubTotal() || pF->IsInput() || pF->GetId() >= SpecialReportField::REPORT_LOWER_SPECIAL_ID)
			continue;
		if (pF->IsHidden())
			hiddenFieldsCount++;
	}
	if (hiddenFieldsCount == 0)
		return;

	ASSERT_VALID(GetWoormFrame()->m_pObjectPropertyView);
	GetWoormFrame()->m_pObjectPropertyView->NewElement(FALSE, TRUE);
}

//------------------------------------------------------------------------------
void CWoormDocMng::OnMulSelAlign ()
{
	int     hID = IDC_H_NO_CHANGE;
	int     vID = IDC_V_NO_CHANGE;

	CMulAlignDlg dialog (hID, vID);
	if (dialog.DoModal() != IDOK) return;

	// move multiple selected GetObjects() if any
	if (!m_pMultipleSelObj->IsEmpty())
	{
		m_pMultipleSelObj->AlignMultipleSelObjects(hID, vID);
		UpdateWindow();
	}
	RemoveFocusFromBtn();
}

//------------------------------------------------------------------------------
void CWoormDocMng::OnMulSelSize ()
{
	CSize   size(m_pMultipleSelObj->GetBaseRect().Size());
	int     hID = IDC_H_NO_CHANGE;
	int     vID = IDC_V_NO_CHANGE;

	CMulSizeDlg dialog (hID, vID, size);
	if (dialog.DoModal() != IDOK) return;

	// move multiple selected GetObjects() in any
	if (!m_pMultipleSelObj->IsEmpty())
	{
		m_pMultipleSelObj->SizeMultipleSelObjects(hID, vID, size);
		UpdateWindow();
	}
	RemoveFocusFromBtn();
}

//------------------------------------------------------------------------------
BOOL CWoormDocMng::CheckTypeConsistence(CString& sError)
{
	// check type consistence for all columns
	int ub = GetObjects().GetUpperBound();
	for (int col = 0; col <= ub; col++)
		if (!GetObjects()[col]->CheckTypeConsistence(sError))
			return FALSE;

	return TRUE;
}

//------------------------------------------------------------------------------
void CWoormDocMng::OnFontStyles()
{
	FontIdx  fontIdx = m_pFontStyles->GetFontIdx(FNT_DEFAULT);
	CFontStylesDlg	dialog(*m_pFontStyles, fontIdx, FALSE, NULL, GetNamespace(), TRUE);

	if (dialog.DoModal() == IDOK)
	{
		Invalidate		();
		UpdateWindow	();
	}

	if (!IsModified())
		SetModifiedFlag(m_pFontStyles->IsModified());
}

//------------------------------------------------------------------------------
void CWoormDocMng::OnFormatStyles()
{
	FormatIdx       formatIdx = 0;
	CFormatDlg      dialog(*m_pFormatStyles, formatIdx, FALSE, NULL, GetNamespace(), TRUE);

	if (dialog.DoModal() == IDOK)
		FormatAll();

	if (!IsModified())
		SetModifiedFlag(m_pFormatStyles->IsModified());
}

//------------------------------------------------------------------------------
void CWoormDocMng::OnEnumsViewer()
{
	AfxRunEnumsViewer ();
}

//------------------------------------------------------------------------------
void CWoormDocMng::OnUpdateEnumsViewer(CCmdUI* pCmdUI)
{
	pCmdUI->Enable(CanSaveReport());
}

//------------------------------------------------------------------------------
void CWoormDocMng::OnUpdateTools(CCmdUI* pCmdUI)
{
	pCmdUI->Enable(TRUE);
}

//------------------------------------------------------------------------------
void CWoormDocMng::OnUpdateGeneralSettings(CCmdUI* pCmdUI)
{
	pCmdUI->Enable(m_bAllowEditing);
}

//------------------------------------------------------------------------------
void CWoormDocMng::OnGeneralSettings()
{
	//carico la property grid per i general settings passandogli un cnode fittizio
	CNodeTree* pNode = new CNodeTree(NULL, CRSTreeCtrlImgIdx::NoGLyph, CNodeTree::ENodeType::NT_SETTINGS, m_pWoormIni, NULL);

	if(GetWoormFrame() && GetWoormFrame()->m_pObjectPropertyView)
		GetWoormFrame()->m_pObjectPropertyView->LoadPropertyGrid(pNode);
}

//------------------------------------------------------------------------------
void CWoormDocMng::OnUpdateProperties(CCmdUI* pCmdUI)
{
	pCmdUI->Enable(TRUE);
}

//------------------------------------------------------------------------------
void CWoormDocMng::OnProperties()
{
	GetWoormFrame()->CreateAndShowPropertyPanel();

	//carico la property grid per i general settings passandogli un cnode fittizio
	CNodeTree* pNode = new CNodeTree(NULL, CRSTreeCtrlImgIdx::NoGLyph, CNodeTree::ENodeType::NT_PROPERTIES, m_pDocProperties, NULL);

	if (GetWoormFrame() && GetWoormFrame()->m_pObjectPropertyView)
		GetWoormFrame()->m_pObjectPropertyView->LoadPropertyGrid(pNode);

}

//------------------------------------------------------------------------------
void CWoormDocMng::OnUpdatePageInfo(CCmdUI* pCmdUI)
{
	pCmdUI->Enable(TRUE);
}

//------------------------------------------------------------------------------
void CWoormDocMng::OnPageInfo()
{
	GetWoormFrame()->CreateAndShowPropertyPanel();

	//carico la property grid per i general settings passandogli un cnode fittizio
	CNodeTree* pNode = new CNodeTree(NULL, CRSTreeCtrlImgIdx::NoGLyph, CNodeTree::ENodeType::NT_PAGE, &m_PageInfo, NULL);

	if (GetWoormFrame() && GetWoormFrame()->m_pObjectPropertyView)
		GetWoormFrame()->m_pObjectPropertyView->LoadPropertyGrid(pNode);

}

//------------------------------------------------------------------------------
void CWoormDocMng::MoveColumn (BOOL left)
{
	if (CurrentIsTable())
		((Table*) m_pCurrentObj)->MoveColumn(left);
}

//------------------------------------------------------------------------------
void CWoormDocMng::OnColAddTotal()
{
	if (CurrentIsTable())
		((Table*) m_pCurrentObj)->AddColumnTotal();
}

//------------------------------------------------------------------------------
void CWoormDocMng::OnColDelete()
{
	if (CurrentIsTable())
		((Table*)m_pCurrentObj)->DeleteColumnComplete(((Table*)m_pCurrentObj)->m_nActiveColumn);
}

//------------------------------------------------------------------------------
void CWoormDocMng::SetAppropriateCursor()
{
	ASSERT(m_Creating <= ObjectType::NONE);
	SetCursor(m_Creating);
}
  
//------------------------------------------------------------------------------
void CWoormDocMng::SetCursor(int nCursor)
{
	ASSERT(nCursor < NUM_CURSORS);
	// various GDI objects we need to draw
	static const HCURSOR* NEAR rghCursors = LoadCursors();	
	::SetCursor(rghCursors[nCursor]);
}
//-----------------------------------------------------------------------------
BOOL CWoormDocMng::OnNewDocument()
{
	// Suppone che tutto vada a buon fine.
	m_bAborted = FALSE;
	m_FileFormat = CLineFile::UTF8;

	////OSL: L'utente può creare nuovi report ?
	//if 
	//	(
	//		AfxGetSecurityInterface()->IsSecurityEnabled() 
	//		&&
	//		m_pAllowEditing
	//		&&
	//		OSL_IS_PROTECTED(m_pAllowEditing->GetInfoOSL())
	//		&&
	//		! OSL_CAN_DO( m_pAllowEditing->GetInfoOSL(), OSL_GRANT_Report_Create) 
	//	)
	//{
	//	Message(OSLErrors::MISSING_GRANT());

	//	m_bAborted = TRUE;
	//	return !m_bAborted;
	//}

	m_pMessages = new CMessages();

	if 
		(
			m_pCallerDocument &&
			AfxGetDefaultSqlConnection()->GetDBMSType() == DBMS_ORACLE &&
			m_pCallerDocument->m_pTbContext->GetUpdatableSqlSession()->IsTxnInProgress()
		)
	{
		m_pTbContext = new CTBContext(*(m_pCallerDocument->m_pTbContext));
	}
	else
		m_pTbContext = new CTBContext(AfxGetDefaultSqlConnection(), this);	// da mettere come connection di documento	

	// create a new document if valid catalog was loaded
	if (
			!m_pTbContext ||
			!m_pTbContext->IsValid() ||         
			!ResourceAvailable() ||
			!__super::OnNewDocument()
		)
	{
		m_bAborted = TRUE;
		return !m_bAborted;
	}

	m_pSqlConnection = m_pTbContext->GetSqlConnection();
	
	m_bAllowEditing = TRUE;
	m_bIsNew = TRUE;

	SetModifiedFlag(FALSE);     // start off with unmodified
	// implementation dependent initialization
	InitDocument();

	GetWoormFrame()->CreateTools(this);
	
	return !m_bAborted;
}

//-----------------------------------------------------------------------------
BOOL CWoormDocMng::ParseHeadData (ViewParser& lex)
{
	m_bNoWeb = lex.Matched (T_NO_WEB);

	BOOL bInvalid = lex.Matched (T_INVALID);
	if (bInvalid)
	{
		lex.SetError(_TB("The report is invalid because its migration was completed with errors. We suggest to open report with editore and find comments which explain the problems met during the migration."));
		return FALSE;
	}

	if	(
			!lex.ParseTag (T_RELEASE) ||
			!lex.ParseInt (m_nReportRelease)
		)
	{
		lex.SetError(_TB("Report release missing!"));
		return FALSE;
	}

	// break if wrong m_nRelease definition strFile
	if (m_nReportRelease > m_nRelease)
	{
		lex.SetError(_TB("Wrong release!"));
		return FALSE;
	}

	// verifico che sia compatibile come release di modifiche al report
	if (lex.LookAhead(T_COMMA))
		if (!lex.ParseComma() || !lex.ParseInt(m_nModifyRelease))
		{
			lex.SetError(_TB("Outdate syntax is wrong. Release declaration of changes  to the report not correct."));
			return FALSE;
		}

	AddOnModule* pAddOnMod = AfxGetAddOnModule(GetNamespace());
	
	// se il namespace non mi arriva di tipo report vuol dire che è nuovo e non
	// ha senso verificare gli OutDates che non ha ancora un modulo assegnato.
	if (GetNamespace().GetType() == CTBNamespace::REPORT && pAddOnMod && pAddOnMod->m_XmlDescription.IsOutDated(GetNamespace(), m_nModifyRelease))
	{
		lex.SetError(_TB("The producer declared the report incompatible. The customizing must be repeated from that standard!"));
		return FALSE;
	}

	m_bOnlyGraphInfo = FALSE;

	int		nStyle = 0L;
	CRect	rect(0,0,0,0);
	CWoormFrame* pFrame = dynamic_cast<CWoormFrame*>(GetFrame());
	if (pFrame)
		rect = pFrame->rectDefault;
	
	// exit from loop for any keyword different from
	// RECT, MAXIMIZED, MINIMIZED, READ_ONLY, OnlyGraphInfo
	for (;;)
	{
		switch (lex.LookAhead())
		{
			case T_RECT:
			{
				if (!lex.ParseRect(T_RECT, rect))
					return FALSE;

				if ((m_pWoormInfo == NULL || !m_pWoormInfo->m_bHideFrame) && pFrame)
				{
					pFrame->SetWindowPos
						(NULL, rect.left, rect.top, rect.Width(), rect.Height(), SWP_NOZORDER);
				}
				break;
			}
	
			case T_MAXIMIZED:
			{   
				if ((m_pWoormInfo == NULL || !m_pWoormInfo->m_bHideFrame) && pFrame && pFrame->IsWindowVisible())
					pFrame->ShowWindow(SW_SHOWMAXIMIZED);
				lex.SkipToken();
				break;
			}

			case T_MINIMIZED:
			{
				//@@ TODO non funziona il ripristino dello stato minimizzato
				if ((m_pWoormInfo == NULL || !m_pWoormInfo->m_bHideFrame) && pFrame && pFrame->IsWindowVisible())
					pFrame->ShowWindow(SW_SHOWMINIMIZED);
				lex.SkipToken();
				break;
			}

			case T_ONLY_GRAPH:
			{
				m_bOnlyGraphInfo = TRUE;
				lex.SkipToken();
				break;
			}
			
			case T_TEMPLATE:
			{
				lex.SkipToken();
				if (lex.LookAhead(T_STR))
				{
					lex.ParseString(m_Template.m_sNsTemplate);
				}
				else
					m_Template.m_bIsTemplate = TRUE;
				break;
			}

			default:
			{
				return TRUE;
			}
		}
	}
}           

//-----------------------------------------------------------------------------
BOOL CWoormDocMng::ParseProperties (ViewParser& lex)
{
	return m_pDocProperties->Parse(lex);
}

//-----------------------------------------------------------------------------
BOOL CWoormDocMng::ParsePageInfo (ViewParser& lex)
{
	BOOL bOk = m_PageInfo.Parse(lex);

	if (bOk && m_pWoormInfo && !m_pWoormInfo->AreCopiesLocked() )
		m_pWoormInfo->m_nCopies = m_PageInfo.GetCopies();

	return bOk;
}


// deve esser reimplementata perche' noi utilizziamo la OpenDocument con
// parametro <> NULL per fare lo stesso una OpenDocument(NULL) cio' OnFileNew
// ma passiamo come parametro il puntatore del documento chiamante per collegare
// tra loro piu' documenti. MFC non controlla il puntatore e usa questa routine
// che setta la path (a noi non interessa). Userebbe il puntatore ad un documento 
// come se fosse CString ??? (sic.)
//------------------------------------------------------------------------------
void CWoormDocMng::SetPathName(LPCTSTR lpszPathName, BOOL/* = TRUE*/)
{ 
	if (lpszPathName == NULL)
		return;
	if (IsReportString())
		return;

	DocInvocationInfo* pInfo = (DocInvocationInfo*) lpszPathName;

	__super::SetPathName(pInfo->m_lpszFileName);
}

//equivalente alla versione originale MFC di SetPathName
//------------------------------------------------------------------------------
void CWoormDocMng::SetReportPathName(LPCTSTR lpszPathName, BOOL/* = TRUE*/)
{ 
	if (lpszPathName == NULL)
		return;
	if (IsReportString())
		return;

	GetInfoOSL()->m_Namespace = AfxGetPathFinder()->GetNamespaceFromPath(lpszPathName);

	__super::SetPathName(lpszPathName);
}

//------------------------------------------------------------------------------
BOOL CWoormDocMng::IsReportString() const
{
	return m_pWoormInfo && m_pWoormInfo->m_bIsReportString;
}

//------------------------------------------------------------------------------
CString CWoormDocMng::GetReportString() const
{
	return m_pWoormInfo ? m_pWoormInfo->m_ReportNames.GetAt(0) : CString();
}

//-----------------------------------------------------------------------------
void CWoormDocMng::AttachCallerDocument(CBaseDocument* pCallerDoc)
{ 
	if (!pCallerDoc)
		return;

	__super::AttachCallerDocument(pCallerDoc);

	m_ViewSymbolTable.SetOwnerId(pCallerDoc);

	if (m_pEditorManager && m_pEditorManager->GetSymTable())
		m_pEditorManager->GetSymTable()->SetOwnerId(pCallerDoc);
}

//-----------------------------------------------------------------------------
BOOL CWoormDocMng::ParseReport(CWoormInfo* pWoormInfo, CString strReport, BOOL bIsReportString/*=TRUE*/)
{
	ASSERT_VALID(pWoormInfo);
	pWoormInfo->AddReport(strReport);
	pWoormInfo->m_bOwnedByReport = FALSE;
	pWoormInfo->m_bHideFrame = TRUE;
	pWoormInfo->m_bIsReportString = bIsReportString;

	// apro il report senza farne la run
	UserInteractionMode oldUM = ::AfxGetThreadContext()->GetUserInteractionMode();
	::AfxGetThreadContext()->SetUserInteractionMode(UNATTENDED);

	CWoormDocMng* pDoc = (CWoormDocMng*)AfxGetTbCmdManager()->RunWoormReport(pWoormInfo, this, NULL, FALSE, FALSE);
	if (pDoc)
	{
		// costringe il parsing della parte relativa al motore dati che di default
		// non viene fatta in apertura del documento
		if (pWoormInfo->m_arErrors.GetCount() == 0) 
		{
			if (!pDoc->ForceEngineParse())
				pWoormInfo->m_arErrors.Add(_TB("Parse report engine failed"));
		}

		pDoc->OnCloseDocument();
		//----
		if (pWoormInfo->m_arErrors.GetCount() == 1 && pWoormInfo->m_arErrors.GetAt(0).IsEmpty())
			pWoormInfo->m_arErrors.RemoveAll();
			
		if (pWoormInfo->m_arErrors.GetCount() == 0)
		{
			::AfxGetThreadContext()->SetUserInteractionMode(oldUM);
			return TRUE;
		}
	}
	else pWoormInfo->m_arErrors.Add(_TB("Parse report layout failed"));

	::AfxGetThreadContext()->SetUserInteractionMode(oldUM);
	return pWoormInfo->m_arErrors.GetCount() == 0;
}

//-----------------------------------------------------------------------------
void CWoormDocMng::OpenEditViewFullText(const CString& sErr, int line, int pos/* = -1*/)
{
	if (m_pWoormInfo && m_pWoormInfo->m_bHideFrame)
		return;

	if (!CanDoEditReport())
	{
		AfxMessageBox(sErr, MB_OK|MB_ICONERROR);

		if (m_pWoormInfo && m_pWoormInfo->m_bHideFrame)
			m_pWoormInfo->m_arErrors.Add(sErr);

		SetModifiedFlag(FALSE);
		GetFrame()->PostMessageW(WM_CLOSE);
		return;
	}

	int idmsg;
	if (!sErr.IsEmpty() && (idmsg = AfxMessageBox(sErr, MB_ICONERROR| MB_CANCELTRYCONTINUE) != IDTRYAGAIN))
	{
		if (idmsg == IDCANCEL)
		{
			if (m_pWoormInfo && m_pWoormInfo->m_bHideFrame)
				m_pWoormInfo->m_arErrors.Add(sErr);

			SetModifiedFlag(FALSE);
			GetFrame()->PostMessageW(WM_CLOSE);
		}
		return;
	}

	if (m_bAllowEditing && IsModified())
	{
		if (AfxMessageBox(_TB("Report is modified, if you will save the report script by EditView form you will lose current update"), MB_OKCANCEL) != IDOK)
		{
			return;
		}
		SetModifiedFlag(FALSE);
	}

	if (m_strPathName.IsEmpty())
	{
		AfxMessageBox(_TB("It is Impossible to show full script. You must save your changes before"));
		return;
	}

	CTBNamespace ns (CTBNamespace::REPORT);
	ns = AfxGetPathFinder()->GetNamespaceFromPath(m_strPathName);
	if (!ns.IsValid())
	{
		AfxMessageBox(_TB("It is Impossible to show full script. You must save your changes before"));
		return;
	}

	//-------------------------------
	CRSEditViewFullText* pEdtView = dynamic_cast<CRSEditViewFullText*>(CreateSlaveView(RUNTIME_CLASS(CRSEditViewFullText)));
	ASSERT_VALID(pEdtView);

	BOOL bSaved = FALSE;
	pEdtView->LoadFullReport(::ExistFile(m_sRunnedFilePath) ? m_sRunnedFilePath : m_strPathName, &bSaved, sErr, line, pos);
	pEdtView->DoEvent();

	if (bSaved)
	{
		if (m_pWoormInfo && sErr.IsEmpty())
		{
			m_pWoormInfo->m_nNextReport = 0;
			CWoormInfo* pWI = m_pWoormInfo;
			m_pWoormInfo = NULL;
			//riapro il report da file system poichè potenzialmente è stato modificato o corretto
			/*CWoormDoc* pNewReport = */AfxGetTbCmdManager()->RunWoormReport(pWI, GetCallerDocument(), NULL, FALSE, FALSE);
		}
		//chiudo la copia del report in memoria perchè è obsoleta
		SetModifiedFlag(FALSE);
		GetFrame()->PostMessageW(WM_CLOSE);
	}
}

// Open document enable m_bPlayback if filename end for .rde
// in this case RDEManager return a strTmp strFile that contain
// only graphic information for link report with saved info
//
//-----------------------------------------------------------------------------
BOOL CWoormDocMng::OnOpenDocument(LPCTSTR pObject)	
{
	// Suppone che tutto vada a buon fine.
	m_bAborted = FALSE;

	//BOOL bMayBeCrypted = AfxGetSecurityInterface()->IsSecurityEnabled();

	// potrei non partire con NULL e ricevere le informazioni dal mondo
	// esterno. per compatibilita con MFC devo passare attraverso un LPCTSTR
	m_pDocInvocationInfo = (DocInvocationInfo*) pObject;

	m_pExternalControllerInfo = m_pDocInvocationInfo ? m_pDocInvocationInfo->m_pControllerInfo : NULL;
	
	AttachWoormInfoFromDocInvocationInfo();

	//Da scheduler il report in fase di impostazioni parametri viene impostato con il booleano hideFrame  = true, ma non deve essere
	//messo in unattended mode (perche l'ask dialog deve essere mostrata all'operatore umano che sta impostando i parametri)
	if (m_pWoormInfo && m_pWoormInfo->m_bHideFrame && !IsEditingParamsFromExternalController())
		SetInUnattendedMode(TRUE);
	
	// associa il documento chiamante per l'eventuale risoluzione di funzioni di documento
	AttachCallerDocument(m_pDocInvocationInfo->m_pAncestorDoc);
		
	// inizializza il namespace del documento
	CTBNamespace aNs(CTBNamespace::REPORT);

	CString strPathName = m_pDocInvocationInfo->m_lpszFileName;
	if (!strPathName.IsEmpty() && !IsReportString())
		aNs.SetNamespace(AfxGetPathFinder()->GetNamespaceFromPath(strPathName));

	if (aNs.IsValid())
		GetNamespace() = aNs;
		
	// creo il contesto di lavoro
	m_pMessages = new CMessages();

	if 
	(
		m_pCallerDocument &&
		AfxGetDefaultSqlConnection()->GetDBMSType() == DBMS_ORACLE &&
		m_pCallerDocument->m_pTbContext->GetUpdatableSqlSession()->IsTxnInProgress()
	)
	{
		m_pTbContext = new CTBContext(*(m_pCallerDocument->m_pTbContext));
	}
	else
		m_pTbContext = new CTBContext(AfxGetDefaultSqlConnection(), this);	// da mettere come connection di documento
	
	if (m_pTbContext->GetSqlConnection()->GetUsePerformanceMng())
	{
		SqlPerformanceDlg* pPerformanceDlg = new SqlPerformanceDlg(m_pTbContext);
	}

	CString strDefDir;
	CString strPath; 	
	int i = 0;

	// establish if must use m_bPlayback name or normal wrm strFile document
	CString strDocName;
	CString strExt = _T(".") + FileExtension::RDE_EXT();
	ViewParser lex(IsReportString() ? GetReportString() : _T(""), TRUE);		

	// time consuming funtion. Use waiting cursor
	BeginWaitCursor();

	// Open an existing document if valid catalog was loaded oppure
	// se ci sono abbastanza risorse
	// create a new document only if datasource is valid and tables exists
	// e ci sono abbastanza risorse GDI,Heap,System disponibili
	if (
			!m_pTbContext ||
			!m_pTbContext->IsValid() ||         
			!ResourceAvailable()
		)
	{
		m_bAborted = TRUE;
		goto endopen;
	}
	m_pSqlConnection = m_pTbContext->GetSqlConnection();

	// implementation dependent initialization
	InitDocument();

	if (!IsReportString() && strExt.CompareNoCase(::GetExtension(strPathName)) == 0)    
	{
		if (!m_pRDEmanager->Open(strPathName, strDocName))
		{
			m_bAborted = TRUE;
			goto endopen;
		}
		m_bPlayback = TRUE;
		SetReportPathName(strPathName); // contain also RDEData info
	}
	else
	{
		m_bPlayback = FALSE;
		if (!IsReportString())
		{
			CString sT = ::GetExtension(strPathName);
			strDocName = MakeName(strPathName, 
				sT.CompareNoCase('.' + FileExtension::WRMT_EXT()) == 0  ? FileExtension::WRMT_EXT() : FileExtension::WRM_EXT());
			SetReportPathName(strDocName);
		}
	}

	if (AfxGetExportSymbols()->IsActivated())
		AfxGetExportSymbols()->AddFile(GetNamespace().ToUnparsedString(), EXP_SYMB_REPORT);

	if (!IsReportString())
		AfxGetSecurityInterface()->GetObjectGrant (GetInfoOSL());

	// Open diagnostic already maded by lexan
	if	(
			!IsReportString() &&
			!lex.Open (strDocName, TRUE)
		)
	{
		m_bAborted = TRUE;
		goto endopen;
	}
	if	(
			!ParseHeadData	(lex)			||
			!ParseProperties(lex)			||
			!ParsePageInfo	(lex)			
		)
	{
		m_bAborted = TRUE;
		goto endopen;
	}

	m_FileFormat = IsReportString() ? CLineFile::UTF8 : lex.GetFileFormat();

	// NON ELIMINARE: inserito per compatibilita` con le versioni precedenti
	// alla 3.5 di TaskBuilder: se non l'ha parsato prima (vedi ParseHeadData)
	// ci prova ora. (Germano)
	if (!m_bOnlyGraphInfo) 
		m_bOnlyGraphInfo = lex.Matched(T_ONLY_GRAPH);

	if (!m_pOptions->Parse (lex))
	{
		m_bAborted = TRUE;
		goto endopen;
	}

	// carica le eventuali info della stampante preferenziale
	if (!IsReportString())
		m_pOptions->LoadDefault(GetNamespace());

	if (!m_pOptions->m_strDefaultPrinter.IsEmpty())
		m_PageInfo.SetPreferredPrinter	(m_pOptions->m_strDefaultPrinter);
	
	m_bAborted = !ParseViewInfo (lex);

	if (!m_bOnlyGraphInfo && m_bAllowEditing && !m_bAborted)
	{
		m_bAborted = !m_pEditorManager->Parse (lex);
		if (!m_bAborted)
		{
			CString sErr;
			m_bAborted = !CheckTypeConsistence (sErr);
			if (m_bAborted)
				lex.SetError(sErr);
		}
	}

	if (!m_bAborted && !ParseWoormLinks(lex))
	{
		//TODO SkipToToken T_REPORT, se riesce non abortire l'intero report ma segnalare errore
		m_bAborted = TRUE;
		goto endopen;
	}

	// si mette in condizione di oggetto corrente non selezionato
	m_pCurrentObj = NULL;
	m_pActiveRect->Clear(FALSE);

	if (!m_bAborted)
	{
		Invalidate();
		//UpdateWindow();
	}

	SetModifiedFlag(FALSE);     // start off with unmodified
	m_pEditorManager->SetModifiedFlag(FALSE);

endopen:	

	EndWaitCursor();

	if (lex.ErrorFound())
		m_bAborted = TRUE;

	if (m_bAborted)
	{
		if (IsInUnattendedMode())
		{
			Message(lex.BuildErrMsg());
			if (m_pWoormInfo && m_pWoormInfo->m_bHideFrame)
			{
				m_pMessages->ToStringArray(m_pWoormInfo->m_arErrors);
				m_pWoormInfo->m_nLine = lex.GetCurrentLine();
				m_pWoormInfo->m_nCol = lex.GetCurrentPos();
			}
			lex.ClearError();
		}
		else if (CanDoEditReport())
		{
			if (m_pEngine && m_pEngine->IsRunning())
				m_pEngine->Kill();

			CString sErr = lex.BuildErrMsg();
			int nLine = lex.GetCurrentLine();
			int nPos = lex.GetCurrentPos();
			lex.ClearError();
			lex.Abort();	//chiude il file
			
			OpenEditViewFullText(sErr, nLine, nPos);

			return FALSE;
		}

		m_pExternalControllerInfo = NULL;
	}

	if (!m_bAborted)
		SetDecorateTitle();
	
	//m_bEngineRunning = TRUE;

	return !m_bAborted;
}

//-----------------------------------------------------------------------------
BOOL CWoormDocMng::OpenReportTemplate(const CString& sNsTemplate)
{
	m_bAborted = FALSE;

	GetNamespace() = CTBNamespace(CTBNamespace::REPORT, sNsTemplate);

	CString strPathName = AfxGetPathFinder()->GetReportFullName(GetNamespace(), AfxGetLoginInfos()->m_strUserName);
	if (! ::ExistFile(strPathName))
	{
		m_bAborted = TRUE;
		return FALSE;
	}
	return LoadReportTemplate(strPathName);
}

//-----------------------------------------------------------------------------
BOOL CWoormDocMng::LoadReportTemplate(const CString& strPathName)
{
	m_bAborted = FALSE;

	// creo il contesto di lavoro
	m_pMessages = new CMessages();

	ViewParser lex(_T(""), TRUE);		

	// time consuming funtion. Use waiting cursor
	BeginWaitCursor();

	// implementation dependent initialization
	InitDocument();

	// Open diagnostic already maded by lexan
	if	(!lex.Open (strPathName, TRUE))
	{
		m_bAborted = TRUE;
		goto endopen;
	}

	if	(
			!ParseHeadData	(lex)			||
			!ParseProperties(lex)			||
			!ParsePageInfo	(lex)			
		)
	{
		m_bAborted = TRUE;
		goto endopen;
	}

	m_FileFormat = lex.GetFileFormat();

	if (!m_bOnlyGraphInfo) 
		m_bOnlyGraphInfo = lex.Matched(T_ONLY_GRAPH);

	if (!m_pOptions->Parse (lex))
	{
		m_bAborted = TRUE;
		goto endopen;
	}

	// carica le eventuali info della stampante preferenziale
	m_pOptions->LoadDefault(GetNamespace());

	if (!m_pOptions->m_strDefaultPrinter.IsEmpty())
		m_PageInfo.SetPreferredPrinter	(m_pOptions->m_strDefaultPrinter);
	
	m_bAborted = !ParseViewInfo (lex);

	// si mette in condizione di oggetto corrente non selezionato
	m_pCurrentObj = NULL;
	m_pActiveRect->Clear();

	SetModifiedFlag(FALSE);     // start off with unmodified

endopen:	
	EndWaitCursor();
	
	if (m_bAborted)
	{
		if (IsInUnattendedMode())
		{
			Message(lex.BuildErrMsg());
			lex.ClearError();
		}

		m_pExternalControllerInfo = NULL;
	}	
	
	return !m_bAborted;
}

//-----------------------------------------------------------------------------
void CWoormDocMng::OnFrameCreated ()
{
	if (m_pWoormInfo)
	{
		if (m_pWoormInfo->m_bOwnedByReport)
			PostMessage(WM_COMMAND, MAKEWPARAM(ID_POST_OPEN_WOORM_DOC, m_pWoormInfo->m_bRunReport), (LPARAM)GetFrameHandle());
		else
			SendMessage(WM_COMMAND, MAKEWPARAM(ID_POST_OPEN_WOORM_DOC, m_pWoormInfo->m_bRunReport), (LPARAM)GetFrameHandle());
	}
}

//-----------------------------------------------------------------------------
void CWoormDocMng::SetModifiedFlag(BOOL bModified /*= TRUE*/)
{
	ASSERT_VALID(this);
	__super::SetModifiedFlag(bModified);

	if (m_pEditorManager && m_bAllowEditing)
	{
		ASSERT_VALID(m_pEditorManager);
		m_pEditorManager->SetModifiedFlag(bModified, FALSE);
	}
}

//-----------------------------------------------------------------------------
::SqlSession*		CWoormDocMng::GetReadOnlySqlSession		() const { return m_pTbContext->GetReadOnlySqlSession(); }
::SqlSession*		CWoormDocMng::GetUpdatableSqlSession	() const { return m_pTbContext->GetUpdatableSqlSession(); }
::SqlConnection*	CWoormDocMng::GetSqlConnection			() const { return m_pSqlConnection; }

//-----------------------------------------------------------------------------
BOOL CWoormDocMng::OnCmdMsg(UINT nID, int nCode, void* pExtra, AFX_CMDHANDLERINFO* pHandlerInfo)
{
	ASSERT_VALID(this);
	if (nID == ID_POST_OPEN_WOORM_DOC)
	{
		OnPostOpenWoormDoc((BOOL)nCode);
		return TRUE;
	}

	return __super::OnCmdMsg(nID, nCode, pExtra, pHandlerInfo);
}
	
//-----------------------------------------------------------------------------
void CWoormDocMng::OnPostOpenWoormDoc(BOOL bRunReport)
{
	ASSERT_VALID(this);
	CWoormFrame* pFrame = GetWoormFrame();
	ASSERT_VALID(pFrame);
	ASSERT(pFrame->m_hWnd);
	pFrame->CreateTools(this);

	if (m_pWoormInfo->m_bHideFrame )
		pFrame->ShowWindow(SW_HIDE);
	else if (m_pWoormInfo->m_bIconized )
		pFrame->ShowWindow(SW_SHOWMINIMIZED);

	// SendMessage e non Post per permettere di cancellare il pWormInfo
	// da chi sta chiamando e non dal report stesso nel caso sia di proprieta
	// del documento chiamante.
	if (bRunReport)
	{
		RunReport(FALSE, FALSE);
	}
	else if (m_bPlayback && m_pRDEmanager)
	{                    
		// force reading first page without checking current page that it is undefined
		if (!m_pRDEmanager->IsClosed() || !m_ReadingPage.IsLocked())
		{
			m_pRDEmanager->SeekFirstPage();

			// read real data
			ReadPage();
	
			m_ReadingPage.Unlock();
		}
	}
	
	SetDecorateTitle();
}

//-----------------------------------------------------------------------------
BOOL CWoormDocMng::CanCloseDocument()
{
	ASSERT_VALID(this);
	if (!m_bAllowEditing && AfxIsRemoteInterface())
		return TRUE;

	return __super::CanCloseDocument();
}

//-----------------------------------------------------------------------------
void CWoormDocMng::OnCloseDocument()
{
	ASSERT_VALID(this);
	if (IsReportRunning())
		return;

	CFunctionDescription fd;
	if (	
			AfxGetApplicationContext()->m_MacroRecorderStatus == CApplicationContext::RECORDING &&
			AfxGetTbCmdManager()->GetFunctionDescription(_NS_WEB("TestManager.TBMacroRecorder.TBMacroRecorder.RecordCloseReport"), fd)
		)
	{
		fd.SetParamValue(_T("woormDocHandle"), DataLng((long)this));
		AfxGetTbCmdManager()->RunFunction(&fd, 0);		
	}
	
	if (/*m_bAllowEditing &&*/ GetWoormFrame(FALSE))
	{
		ASSERT_VALID(GetWoormFrame());

		GetWoormFrame()->ClosePanels();

		if (m_pUndoStack)
			m_pUndoStack->RemoveAll();
		if (m_RemovedObj.GetCount() > 0)
			m_RemovedObj.RemoveAll();

		//if (AfxMessageBox(L"esci?", MB_YESNO) == IDNO)
		//	return;
	}
	__super::OnCloseDocument();
}

//-----------------------------------------------------------------------------
BOOL CWoormDocMng::EngineStopped()
{
	// control m_pEngine status and accept exit only if all background working
	// process have terminated all activities
	if (m_pEngine->IsRunning())
	{
		CString strTemp = GetPathName();
		if (strTemp.IsEmpty()) 
			strTemp.LoadString(AFX_IDS_UNTITLED);
			
		if 	(
				IsInUnattendedMode() ||
				IsEditingParamsFromExternalController() ||
				AfxMessageBox
				(
					cwsprintf(_TB("The report {0-%s} is still running. \r\nDo you want to abort it in order to exit?\r\n"), (LPCTSTR) strTemp), 
					MB_YESNO | MB_ICONHAND
				) == IDYES
			)
			m_pEngine->Kill();
		
		// must return wihtout save to give time for m_pEngine real stop
		return FALSE;
	}      

	// prova a chiudere eventuali report in cascata
	return !m_bCloseChildReport || AfxGetTbCmdManager()->CloseWoormReport(m_pChildReport);
}

//-----------------------------------------------------------------------------
BOOL BackupFile(CString sFilePath)
{
	if (!ExistFile(sFilePath))
		return TRUE;

	CString sExt;
	int idx = sFilePath.ReverseFind('.');
	if (idx > -1)
	{
		sExt = sFilePath.Mid(idx);
		sFilePath = sFilePath.Left(idx);
	}

	CString p; int n = 1;
	do
	{
		p = sFilePath + cwsprintf(L"_%d.", n++) + FileExtension::BAK_EXT();
	} while (ExistFile(p));

	return RenameFilePath(sFilePath + sExt, p);
}

CString UpdateName(CString sFilePath)
{
	if (!ExistFile(sFilePath))
		return sFilePath;

	CString sExt;
	int idx = sFilePath.ReverseFind('.');
	if (idx > -1)
	{
		sExt = sFilePath.Mid(idx);
		sFilePath = sFilePath.Left(idx);
	}

	CString p; int n = 1;
	do
	{
		p = sFilePath + cwsprintf(L"_%d", n++) + sExt;
	} while (ExistFile(p));

	return p;
}

BOOL CWoormDocMng::PrepareFileNameForSave(CString& filepath)
{
	BOOL bOk = TRUE;
	if (ExistFile(filepath))
	{
		DWORD wAttributes = GetTbFileAttributes(filepath);
		if ((wAttributes & FILE_ATTRIBUTE_READONLY) && AfxTBMessageBox(cwsprintf(_TB("Report {0-%s} is readonly, you want to overwrite it the same ?"), (LPCTSTR)filepath), MB_OKCANCEL) != IDOK)
		{
			return FALSE;
		}
		///*bOk = */SetFileAttributes(filepath, wAttributes & ~FILE_ATTRIBUTE_READONLY);
		
		bOk = BackupFile(filepath);
	}

	if (!bOk)
	{
		CString sNewFilePath = UpdateName(filepath);

		if (AfxTBMessageBox(cwsprintf(
			_TB("Error on saving report {0-%s} It fails to backup previous version. Do you want save it with an updated name {1-%s} ?"),
			(LPCTSTR)filepath, (LPCTSTR)sNewFilePath), MB_OKCANCEL) != IDOK)
		{
			return FALSE;
		}

		filepath = sNewFilePath;
		bOk = TRUE;
	}
	ASSERT(!ExistFile(filepath));
	//----
	return TRUE;
}

BOOL CWoormDocMng::OnSaveDocument(LPCTSTR pszPathName)
{                 
	// cannot save info if current strFile is a m_bPlayback of already saved info
	if (m_bPlayback) return TRUE;
	
	CString strWrmFile = pszPathName;

	if (!PrepareFileNameForSave(strWrmFile))
		return FALSE;

	// make correct title and  start wait cursor
	BeginWaitCursor();
 
	// save strFile name for unparse GraphRect and FileRect            
	SetReportPathName(strWrmFile);

	// save on wrm strFile without bakacup
	UnparseWoorm (strWrmFile, m_bOnlyGraphInfo);	//OnSaveDocument

	if (!m_bOnlyGraphInfo && m_pOptions->MustCreateXsd())
		GetSchema(strWrmFile);

	SetModifiedFlag(FALSE);     // back to unmodified
	m_pEditorManager->SetModifiedFlag(FALSE);

	//m_dsCurrentLayoutView = REPORT_DEFAULT_LAYOUT_NAME;
	//PrepareLayout();

	Invalidate(TRUE);
	UpdateWindow ();

	EndWaitCursor();

	m_bIsNew = FALSE;

	return TRUE;
}

//------------------------------------------------------------------------------
void CWoormDocMng::OnStop()
{
	// terminate running m_pEngine only if not running 
	if (m_bEngineRunning)
	{
		if (m_pEngine->m_pScheduler->m_bExecuteCommandWorking)
		{
			m_pEngine->m_pScheduler->PostMessage(WM_COMMAND, ID_STOP);
			return;
		}

		if (m_pEngine->IsPaused()) GetWoormFrame()->ShowPauseButton();

		if (m_pEngine->IsRunning())
			m_pEngine->ForceDetach ();

		// stop background process
		m_pEngine->Terminate();
		m_bEngineRunning = FALSE; ResetCustomUICulture();

		if (m_pWaitingSubReport)
		{
			AfxGetTbCmdManager()->CloseWoormReport(m_pWaitingSubReport);
			m_pWaitingSubReport = NULL;
		}

		GetWoormFrame()->ShowRunButton();
	}
}

//	must save on strTmp strFile for eventual modified object to run
//	background process and unparse only database information 
//	(graphics info not needed)
//------------------------------------------------------------------------------
void CWoormDocMng::UnparseReport(const CString& strTmp)
{            
	ViewUnparser ofile(strTmp);

	m_pEditorManager->Unparse(ofile);

	ofile.Close();
}

//-----------------------------------------------------------------------------
BOOL CWoormDocMng::ReplaceEmailParameters()
{
	CString sMailBody = m_Email.m_sBody;

	if (
			m_Email.m_sTemplateFileName.IsEmpty() &&
			m_pWoormInfo &&
			!m_pWoormInfo->m_Email.m_sTemplateFileName.IsEmpty()
		)
			m_Email.m_sTemplateFileName = m_pWoormInfo->m_Email.m_sTemplateFileName;

	if (!m_Email.m_sTemplateFileName.IsEmpty())
	{
		CString sTplBody;
		if (::LoadTextFile(m_Email.m_sTemplateFileName, sTplBody)) 
		{
			m_Email.m_sBody = sTplBody;	
			m_Email.m_sHtml.Empty();
		}
	}

	ASSERT(m_Email.m_arstrBodyParameters.GetSize() == m_Email.m_arstrBodyParameterNames.GetSize());

	CString sNewTag(_T("##"));
	{
		CString sStartTag(_T("<tag>"));	CString sEndTag(_T("</tag>"));
		m_Email.m_sBody.Replace(sStartTag, sNewTag);
		m_Email.m_sBody.Replace(sEndTag, sNewTag);
	}

	BOOL bMailBodyParam = FALSE;
	for (;;)
	{
		int nStartPos = m_Email.m_sBody.Find(sNewTag);
		if (nStartPos < 0) break;
		int nEndPos = m_Email.m_sBody.Find(sNewTag, nStartPos + sNewTag.GetLength());
		if (nEndPos < 0) break;
		
		CString sParamName = m_Email.m_sBody.Mid(nStartPos + sNewTag.GetLength(), nEndPos - nStartPos - sNewTag.GetLength());
		int nParamNameLenght = sParamName.GetLength();
		sParamName.Trim();

		CString sParamValue;
		BOOL bFinded = m_Email.GetBodyParameter(sParamName, sParamValue);
		if (!bFinded)
		{
			if (m_pEngine != NULL && m_pEngine->m_pSymTable != NULL)
			{
				SymField* pF = m_pEngine->m_pSymTable->GetField(sParamName);
				DataObj* pV = pF ? pF->GetData() : NULL;
				if (bFinded = (pV != NULL))
					sParamValue = pV->FormatData();
			}
		}
		if (!bFinded)
		{
			if (sParamName.CompareNoCase(_T("MailBody")) == 0 && !bMailBodyParam)
			{
				sParamValue = sMailBody;
				bMailBodyParam = TRUE;	//impedisce ricorsione infinita
			}
			else
			{
				Message(cwsprintf(_TB("Unknown parameter in the e-mail template: {0-%s}"), sParamName));
				//return FALSE;
			}
		}
		m_Email.m_sBody = m_Email.m_sBody.Left(nStartPos) + sParamValue + m_Email.m_sBody.Mid(nEndPos + sNewTag.GetLength());
	}
	return TRUE;
}

//------------------------------------------------------------------------------
BOOL CWoormDocMng::OnSendMailForIMago()
{
	if (!CanDoSendMail()) 
		return FALSE;
	if (!AfxIsActivated(TBEXT_APP, MAILCONNECTOR_ACT))
	{
		AfxMessageBox(LICENZE_TERMINATE);
		return FALSE;
	}
	if (!AfxIsActivated(MAGONET_APP, IMAGO_ACT))
	{
		AfxMessageBox(LICENZE_TERMINATE);
		return FALSE;
	}

	//------------------------------------------
	BOOL bAuto = FALSE;
	BOOL bAttachOther = FALSE;
	BOOL bExpandMailTemplate = FALSE;

	MailConnectorParams* params = AfxGetIMailConnector()->GetParams();
	BOOL bAttachRDE = FALSE;
	BOOL bAttachPDF = TRUE;
	BOOL bCompressAttach = params->GetMailCompress();
	BOOL bRequestDeliveryNotification = params->GetRequestDeliveryNotifications();
	BOOL bRequestReadNotification = params->GetRequestReadNotifications();

	if (m_pWoormInfo)
	{
		m_pWoormInfo->m_bErrorOnSendEmail = FALSE;
		if (m_pOldWoormInfo)
			m_pOldWoormInfo->m_bErrorOnSendEmail = FALSE;
	}

	if (!this->DoAttachReport(bAuto, bAttachRDE, bAttachPDF, bCompressAttach, bAttachOther))
	{
		if (m_pWoormInfo)
			m_pWoormInfo->m_bErrorOnSendEmail = TRUE;
		if (m_pOldWoormInfo)
			m_pOldWoormInfo->m_bErrorOnSendEmail = TRUE;
		return FALSE;
	}

	BOOL bOk = TRUE;
	// effettuo l'invio se:
	// 1) la mail non deve essere unica (una mail per report)
	// 2) la mail deve essere unica e sto elaborando l'ultimo report (ho appena 
	//    inserito l'ultimo allegato
	if (!m_pWoormInfo || !m_pWoormInfo->m_bUniqueMail || m_pWoormInfo->IsLastReport())
	{
		if (bExpandMailTemplate && !this->ReplaceEmailParameters())
		{
			if (m_pWoormInfo)
				m_pWoormInfo->m_bErrorOnSendEmail = TRUE;
			if (m_pOldWoormInfo)
				m_pOldWoormInfo->m_bErrorOnSendEmail = TRUE;
			return FALSE;
		}

		BOOL bOk =  AfxGetIMailConnector()->ShowEmailDlg
												(
													m_pCallerDocument,
													m_Email,
													&bAttachRDE, &bAttachPDF,
													&bCompressAttach,
													NULL, NULL,
													&m_bDirectConcatPDF,
													&bRequestDeliveryNotification, &bRequestReadNotification
												);
		if (!bOk && m_pWoormInfo)
		{
			m_pWoormInfo->m_bErrorOnSendEmail = TRUE;
			if (m_pOldWoormInfo)
				m_pOldWoormInfo->m_bErrorOnSendEmail = TRUE;
		}
	}

	m_Email.m_sHtml.Empty();
	return bOk;
}

//------------------------------------------------------------------------------
void CWoormDocMng::OnSendMail()
{
	if (!CanDoSendMail())
		return;
	if (!AfxIsActivated(TBEXT_APP,  MAILCONNECTOR_ACT))
	{
		AfxMessageBox(LICENZE_TERMINATE);
		return;
	}

	if (m_Email.UsePostaLite())
	{
		if (AfxGetIMailConnector()->ShowSendPostaLiteDlg(m_pCallerDocument, m_Email))
		{
			DoSendMail (FALSE, FALSE, TRUE, FALSE, FALSE, FALSE, FALSE, FALSE);
		}
		return;
	}

	if (AfxIsActivated(MAGONET_APP, IMAGO_ACT) && AfxGetLoginContext()->GetLoginInfos()->m_bDataSynchro)
	{
		CTBNamespace ns(_NS_MOD("Module.Extensions.TbMailer"));
		DataObj* pSetting = AfxGetSettingValue(ns, _T("MailServer"), _T("UseMago"), DataBool(TRUE));
		BOOL useMago = pSetting ? *((DataBool*)pSetting) : TRUE;
		if (!useMago)
		{
			OnSendMailForIMago();
			return;
		}
	}

	ASSERT(AfxGetBaseApp()->GetTaskBuilderAddOnApp()->m_pAddOnModules);
	MailConnectorParams* params = AfxGetIMailConnector()->GetParams();

	BOOL bAttachRDE = FALSE;
	BOOL bAttachPDF = TRUE;
	BOOL bCompressAttach= params->GetMailCompress();
	BOOL bRequestDeliveryNotification = params->GetRequestDeliveryNotifications();
	BOOL bRequestReadNotification = params->GetRequestReadNotifications();

	CString strLocal = ::_tsetlocale (LC_ALL, NULL);

	ReplaceEmailParameters();

	if 
		(
			AfxGetIMailConnector()->ShowEmailDlg
				(
					m_pCallerDocument,	
					m_Email, 
					&bAttachRDE, &bAttachPDF, 
					&bCompressAttach, 
					NULL, NULL,
					&m_bDirectConcatPDF,
					&bRequestDeliveryNotification, &bRequestReadNotification
				)
		)
	{
		DoSendMail (FALSE, bAttachRDE, bAttachPDF, bCompressAttach, bRequestDeliveryNotification, bRequestReadNotification, FALSE, FALSE);
	}

	CString strLocal2 = ::_tsetlocale (LC_ALL, NULL);
	if (strLocal != strLocal2)
	{
		::_tsetlocale (LC_ALL, strLocal);
	}
}

//------------------------------------------------------------------------------
void CWoormDocMng::OnSendPostaLite()
{
	if (!CanDoSendMail()) return ;
	if (!AfxIsActivated(TBEXT_APP,  MAILCONNECTOR_ACT))
	{
		AfxMessageBox(LICENZE_TERMINATE);
		return;
	}
	if (!AfxGetIMailConnector()->IsPostaLiteEnabled()) return ;

	m_Email.SetUsePostaLite(TRUE);

	if 
		(
			AfxGetIMailConnector()->ShowSendPostaLiteDlg
				(
					m_pCallerDocument,	
					m_Email
				)
		)
	{
		DoSendMail (FALSE, FALSE, TRUE, FALSE, FALSE, FALSE, FALSE, FALSE);
	}
	m_Email.SetUsePostaLite(FALSE);
}

//------------------------------------------------------------------------------
BOOL CWoormDocMng::DoSendMail (BOOL bAuto, BOOL bAttachRDE, BOOL bAttachPDF, BOOL bCompressAttach, BOOL bRequestDeliveryNotification, BOOL bRequestReadNotification, BOOL bAttachOther, BOOL bExpandMailTemplate/* = TRUE*/)
{
	if (!CanDoSendMail()) return FALSE;
	if (!AfxIsActivated(TBEXT_APP,  MAILCONNECTOR_ACT))
	{
		AfxMessageBox(LICENZE_TERMINATE);

		if (m_pWoormInfo) 
			m_pWoormInfo->m_bErrorOnSendEmail = TRUE;
		if (m_pOldWoormInfo) 
			m_pOldWoormInfo->m_bErrorOnSendEmail = TRUE;
		return FALSE;
	}

	if (m_pWoormInfo) 
	{
		m_pWoormInfo->m_bErrorOnSendEmail = FALSE;
		if (m_pOldWoormInfo) 
			m_pOldWoormInfo->m_bErrorOnSendEmail = FALSE;
	}
	if (m_Email.m_To.GetSize() == 0 && m_Email.m_CC.GetSize() == 0 && m_Email.m_BCC.GetSize() == 0 && m_Email.m_PostaLiteMsg.GetCount() == 0)
	{
		if (m_pWoormInfo) 
			m_pWoormInfo->m_bErrorOnSendEmail = TRUE;
		if (m_pOldWoormInfo) 
			m_pOldWoormInfo->m_bErrorOnSendEmail = TRUE;
		return FALSE;
	}

	if (!this->DoAttachReport (bAuto, bAttachRDE, bAttachPDF, bCompressAttach, bAttachOther))
	{
		if (m_pWoormInfo) 
			m_pWoormInfo->m_bErrorOnSendEmail = TRUE;
		if (m_pOldWoormInfo) 
			m_pOldWoormInfo->m_bErrorOnSendEmail = TRUE;
		return FALSE;
	}

	// effettuo l'invio se:
	// 1) la mail non deve essere unica (una mail per report)
	// 2) la mail deve essere unica e sto elaborando l'ultimo report (ho appena 
	//    inserito l'ultimo allegato
	if (!m_pWoormInfo || !m_pWoormInfo->m_bUniqueMail || m_pWoormInfo->IsLastReport())
	{
		if (bExpandMailTemplate && !ReplaceEmailParameters())
		{
			if (m_pWoormInfo) 
				m_pWoormInfo->m_bErrorOnSendEmail = TRUE;
			if (m_pOldWoormInfo) 
				m_pOldWoormInfo->m_bErrorOnSendEmail = TRUE;
			return FALSE;
		}

		if (m_Email.UsePostaLite())
		{
			if (!DoPostaLiteOutput(&m_Email))
			{
				if (m_pWoormInfo)
				{
					m_pWoormInfo->m_bErrorOnSendEmail = TRUE;
					if (m_pOldWoormInfo) 
						m_pOldWoormInfo->m_bErrorOnSendEmail = TRUE;
				}
				if (m_pMessages)
					m_pMessages->Show();
				return FALSE;
			}
		}
		else 
		{
			if (!AfxGetIMailConnector()->SendMail (m_Email, &bRequestDeliveryNotification, &bRequestReadNotification, m_pMessages))
			{
				if (m_pWoormInfo)
				{
					m_pWoormInfo->m_bErrorOnSendEmail = TRUE;
					if (m_pOldWoormInfo) 
						m_pOldWoormInfo->m_bErrorOnSendEmail = TRUE;
				}
				if (m_pMessages)
					m_pMessages->Show();
				m_Email.m_sHtml.Empty();
				return FALSE;
			}
		}
		m_Email.m_sHtml.Empty();
	}
	return TRUE;
}

//------------------------------------------------------------------------------
BOOL CWoormDocMng::DoAttachReport (BOOL bAuto, BOOL bAttachRDE, BOOL bAttachPDF, BOOL bCompressAttach, BOOL bAttachOther/*= FALSE*/)
{
	if (bAttachPDF && !AfxIsActivated(TBEXT_APP,  MAILCONNECTOR_ACT))
	{
		AfxMessageBox(LICENZE_TERMINATE);
		if (m_pWoormInfo) 
		{
			m_pWoormInfo->m_bErrorOnSendEmail = TRUE;
			if (m_pOldWoormInfo) 
				m_pOldWoormInfo->m_bErrorOnSendEmail = TRUE;
		}
		return FALSE;
	}

	CWoormView* pView = GetWoormView();
	ASSERT_VALID(pView);
	if (!pView) 
	{
		if (m_pWoormInfo) 
			m_pWoormInfo->m_bErrorOnSendEmail = TRUE;
		if (m_pOldWoormInfo) 
			m_pOldWoormInfo->m_bErrorOnSendEmail = TRUE;
		return FALSE;
	}

	CString strFilePDF;
	CString strFile(GetTempName(FileExtension::WRM_EXT()));
	strFile.MakeLower();

	if (bAttachPDF)
	{
		CString strTitle; 
		if
			(
				m_pWoormInfo && 
				m_pWoormInfo->m_arstrOutputFileNames.GetSize() >= m_pWoormInfo->m_nNextReport &&
				m_pWoormInfo->m_nNextReport > 0
			)
		{
			strFilePDF = m_pWoormInfo->m_arstrOutputFileNames[m_pWoormInfo->m_nNextReport-1];
			strTitle = 	GetNameWithExtension(strFilePDF);
		}
		else
		{
			strFilePDF = strFile;
			strFilePDF.Replace( _T(".tmp"), _T(".") + FileExtension::PDF_EXT());
			strTitle = m_Email.m_sAttachmentReportName.IsEmpty() 
					?
						::MakeName(::GetName(GetPathName()), FileExtension::PDF_EXT())	//an. 8130
					:
						::MakeName(m_Email.m_sAttachmentReportName, FileExtension::PDF_EXT())	//m. 3519
					; 
		}

		CMapiMessage mmSave;
		if (!bAuto)
		{
			mmSave = m_Email;
		}
		
		if (!pView->OnPDFPrint(strFilePDF)) 
			return FALSE;
		
		if (!bAuto)
		{
			m_Email = mmSave;
		}

		// se devo concatenare tutti i report in un unico PDF,
		// prendo il pdf stampato e lo accodo nel file individuato
		// dal primo elemento di m_arstrOutputFileNames
		if ((m_pWoormInfo && m_pWoormInfo->m_bConcatPDF) || m_bDirectConcatPDF)
		{
			CString strUniqueFile;
			if (m_pWoormInfo->m_arstrOutputFileNames.GetSize())
			{
				strUniqueFile = m_pWoormInfo->m_arstrOutputFileNames[0];

				ConcatPDF(strFilePDF, strUniqueFile);

				strFilePDF = strUniqueFile;

				strTitle = m_Email.m_sAttachmentReportName.IsEmpty() 
					?
						GetNameWithExtension(strUniqueFile)
					:
						::MakeName(m_Email.m_sAttachmentReportName, FileExtension::PDF_EXT())	
					; 
			}
			else
			{
				strUniqueFile = strFilePDF;

				strTitle = m_Email.m_sAttachmentReportName.IsEmpty() 
					?
						::MakeName(GetNamespace().GetObjectName(), FileExtension::PDF_EXT())
					:
						::MakeName(m_Email.m_sAttachmentReportName, FileExtension::PDF_EXT())	
					; 
			}

			ConcatPDFAttachments (strUniqueFile);
		}

		// allego il file se:
		// 1) non devo concatenare i PDF
		// 2) questo è l'ultimo file in elaborazione 
		//	  (per cui l'eventuale concatenazione è completa)
		if (
			!m_pWoormInfo 
			||
			!(m_pWoormInfo->m_bConcatPDF || m_bDirectConcatPDF)
			||
			m_pWoormInfo->IsLastReport()
			)
		{
			
			strTitle = 
				m_Email.m_sAttachmentReportName.IsEmpty() 
				?
				(
					m_pWoormInfo->m_arstrOutputFileNames.GetCount() == 0
					?
						::MakeName(GetNamespace().GetObjectName(), FileExtension::PDF_EXT())
					:
						::MakeName(m_pWoormInfo->m_arstrOutputFileNames[0], FileExtension::PDF_EXT())				
				)
				:
				::MakeName(m_Email.m_sAttachmentReportName, FileExtension::PDF_EXT())	
			; 

			if (bCompressAttach)
			{
				::ZCompress(strFilePDF, strFilePDF + _T(".zip"), strTitle);
				strFilePDF += _T(".zip");
				strTitle += _T(".zip");
			}

			//if (m_pWoormInfo->m_ReportNames.GetSize() == 1)
			//	m_Email.InsertTopAttachment(strFilePDF, strTitle); //si inserisce prima di eventuali allegati/subreport
			//else
				m_Email.SetAttachment(strFilePDF, strTitle, TRUE);
		}
	}
	else if ((m_pWoormInfo && m_pWoormInfo->m_bConcatPDF) || m_bDirectConcatPDF)
	{
		CString strUniqueFile, strTitle;

		if (m_pWoormInfo && m_pWoormInfo->m_arstrOutputFileNames.GetSize())
		{
			strUniqueFile = m_pWoormInfo->m_arstrOutputFileNames[0];

			strTitle = m_Email.m_sAttachmentReportName.IsEmpty() 
				?
					GetNameWithExtension(strUniqueFile)
				:
					::MakeName(m_Email.m_sAttachmentReportName, FileExtension::PDF_EXT())	
				; 
		}
		else
		{
			strUniqueFile = GetTempName(FileExtension::PDF_EXT()) + _T(".pdf");
			strUniqueFile.Replace(_T(".tmp."), _T("."));

			strTitle = m_Email.m_sAttachmentReportName.IsEmpty() 
				?
					::MakeName(GetNamespace().GetObjectName(), FileExtension::PDF_EXT())
				:
					::MakeName(m_Email.m_sAttachmentReportName, FileExtension::PDF_EXT())	
				; 
		}

		BOOL bOkConcat = ConcatPDFAttachments(strUniqueFile);

		if (bOkConcat)
		{
			m_Email.SetAttachment(strUniqueFile, strTitle, TRUE);
		}
	}
	
	if (bAttachOther)
	{
		CString strFileOther = strFile;

		CExportInfo* pExp = (CExportInfo*) (m_pWoormInfo->m_arExportInfo.GetAt(m_pWoormInfo->m_nNextReport - 1));

		strFileOther.Replace(_T(".tmp"), _T(".") + pExp->GetExportFileExtension());	

		CString strTitle = m_Email.m_sAttachmentReportName.IsEmpty() 
				?
					MakeName(GetNamespace().GetObjectName(), pExp->GetExportFileExtension())
				:
					::MakeName(m_Email.m_sAttachmentReportName, pExp->GetExportFileExtension())	
				; 

		int idx = m_pWoormInfo->m_nNextReport - 1;
		if (idx > m_pWoormInfo->m_arstrOutputFileNames.GetUpperBound())
			m_pWoormInfo->m_arstrOutputFileNames.SetAtGrow(idx, strFileOther);

		if (DoExportOutput())
		{
			if (bCompressAttach)
			{
				ZCompress(strFileOther, strFileOther + _T(".zip"), strTitle);
				strFileOther += _T(".zip");
				strTitle += _T(".zip");
			}

			m_Email.SetAttachment(strFileOther, strTitle, TRUE);
		}
		else
			ASSERT(FALSE);
	}

	if (bAttachRDE)
	{
		CString strFileRDE = strFile;
		strFileRDE.Replace(_T(".tmp"), _T(".") + FileExtension::RDE_EXT());	

		UnparseWoorm (strFileRDE, TRUE);	//Attachment RDE
		m_pRDEmanager->SaveRDEFile (strFileRDE);

		CString strTitle = m_Email.m_sAttachmentReportName.IsEmpty() 
				?
					MakeName(GetNamespace().GetObjectName(), FileExtension::RDE_EXT()) 
				:
					::MakeName(m_Email.m_sAttachmentReportName, FileExtension::RDE_EXT())	
				; 

		if (bCompressAttach)
		{
			ZCompress(strFileRDE, strFileRDE + _T(".zip"), strTitle);
			strFileRDE += _T(".zip");
			strTitle += _T(".zip");
		}

		m_Email.SetAttachment(strFileRDE, strTitle, TRUE);
	}
		
	return TRUE;
}

//------------------------------------------------------------------------------
BOOL CWoormDocMng::DoPDFOutput(CString* psFileName/* = NULL*/)
{
	if (!AfxIsActivated(TBEXT_APP,  MAILCONNECTOR_ACT))
	{
		AfxMessageBox(LICENZE_TERMINATE);
		return FALSE;
	}
	BOOL bResult = TRUE;

	// se devo concatenare i PDF in un unico file, utilizzo m_arstrPDFFiles
	// come elenco dei file generati (se è vuoto, inserisco dei nomi di file temporanei)
	if (m_pWoormInfo->m_bConcatPDF || m_bDirectConcatPDF)
	{
		for (int i = m_pWoormInfo->m_arstrOutputFileNames.GetSize(); i < m_pWoormInfo->m_ReportNames.GetSize(); i++)
		{
			CString strFile(GetTempName(cwsprintf(FileExtension::PDF_EXT())));
			m_pWoormInfo->m_arstrOutputFileNames.Add (strFile);
		}
	}

	//----
	CWoormView* pView = GetWoormView();	
	ASSERT_VALID(pView);
	if (!pView) 
	{
		return FALSE;
	}
	//----

	CString strFileName;

	if (m_pWoormInfo->m_ReportNames.GetSize() != m_pWoormInfo->m_arstrOutputFileNames.GetSize()) 
	{
		if (psFileName)
		{
			strFileName = GetTempName(FileExtension::PDF_EXT());
			int nIdx = strFileName.ReverseFind('\\');
			ASSERT(nIdx > 0);
			CString sReport = this->GetNamespace().GetObjectName();
			sReport = sReport.Left(sReport.ReverseFind('.'));
			strFileName = strFileName.Left(nIdx + 1) + sReport + _T("##") + strFileName.Mid(nIdx + 1);
		}
		else
		{
			ASSERT(FALSE);
			return FALSE;
		}
	}
	else
	{
		strFileName = m_pWoormInfo->m_arstrOutputFileNames[m_pWoormInfo->m_nNextReport - 1];
	}

	strFileName = AppendExtensionIfNeeded(strFileName, _T("pdf"));
		
	bResult = pView->OnPDFPrint (strFileName);
		
	if (bResult) 
	{
		if (psFileName) *psFileName = strFileName;

		ConcatPDFAndShowWhenRequired(psFileName);	
	}
	
	return bResult;
}

//------------------------------------------------------------------------------
BOOL CWoormDocMng::PostaLiteCheckFile(CMapiMessage* pEmail)
{
	if (pEmail->m_Attachments.GetSize() == 0)
		return FALSE;
	CString sFile ( pEmail->m_Attachments[pEmail->m_Attachments.GetUpperBound()] );
	if (sFile.Right(4).CompareNoCase(_T(".pdf")))
		return FALSE;
	//----

	pEmail->m_PostaLiteMsg.m_sFileName = sFile;
	pEmail->m_PostaLiteMsg.m_nFileSize = ::GetFileSize(sFile);
	pEmail->m_PostaLiteMsg.m_nFilePages = ::GetPdfPageNumber(sFile);
	ASSERT(pEmail->m_PostaLiteMsg.m_nFilePages);
	return TRUE;
}

//------------------------------------------------------------------------------
BOOL CWoormDocMng::DoPostaLiteOutput(CMapiMessage* pEmail)
{
	ASSERT(pEmail);

	if (pEmail->m_PostaLiteMsg.GetSize() == 0)
		return TRUE;

	if (!PostaLiteCheckFile(pEmail))
		return FALSE;

	//----	//AfxGetDefaultSqlSession ()->GetSqlConnection()->GetNewSqlSession()
	ASSERT(this->m_pTbContext);
	SqlSession* pSql = this->m_pTbContext->GetUpdatableSqlSession();

	BOOL bOk = AfxGetIMailConnector()->PostaLiteEnqueueMsg(pEmail, pSql /*TODO , pDiagnostic*/);
	if (!bOk)
	{
		ASSERT(FALSE);
		return FALSE;
	}
	return TRUE;
}

//------------------------------------------------------------------------------
BOOL CWoormDocMng::DoRDEOutput()
{
	CString strFileRDE = AppendExtensionIfNeeded (m_pWoormInfo->m_arstrOutputFileNames.GetAt(m_pWoormInfo->m_nNextReport - 1), _T("rde"));
	
	// save with only graphics info but leave current wrm as is
	UnparseWoorm (strFileRDE, TRUE);	//RDE
	// add data info to parsed woorm report (error recovered by RDEManager)
	return m_pRDEmanager->SaveRDEFile(strFileRDE);
}

//------------------------------------------------------------------------------
BOOL CWoormDocMng::DoExportOutput()
{	
	CExportInfo* pExp = (CExportInfo*) ( m_pWoormInfo->m_arExportInfo.GetAt (m_pWoormInfo->m_nNextReport - 1) );
	if (!pExp) 
		return FALSE;

	CArray<CExportTableItem*> m_parTables;
	m_pExportData->AutoAddTable(m_parTables);
	if (m_parTables.GetSize() == 0)		//seleziono una tabella
		m_pExportData->AutoAddAllFields();
	else
		m_pExportData->m_pItem = (CExportTableItem*)m_parTables[0];

	m_pExportData->m_ExportInfo = *pExp; //copio l'oggetto
	//sempre tutte le pagine
	m_pExportData->m_ExportInfo.SetPagesNum ((USHORT)(m_pRDEmanager->LastPage() + 1));
	m_pExportData->m_ExportInfo.SetCurrentPage (1);
	m_pExportData->m_ExportInfo.SetPageRange (1, m_pRDEmanager->LastPage() + 1 );
	
	CString strFilename;
	if (m_pWoormInfo->m_arstrOutputFileNames.GetUpperBound() >= m_pWoormInfo->m_nNextReport - 1)
	{
		CString str = m_pWoormInfo->m_arstrOutputFileNames.GetAt(m_pWoormInfo->m_nNextReport - 1);
		if (str.Find('.') < 0)
			strFilename = AppendExtensionIfNeeded(str, pExp->GetExportFileExtension());
		else 
			strFilename = str;
	}
	else
		strFilename = MakeName(_T("Att"), pExp->GetExportFileExtension()) ;

	return m_pExportData->RunExport (strFilename, TRUE);
}

//------------------------------------------------------------------------------
void CWoormDocMng::ConcatPDFAndShowWhenRequired(CString* psFileName/* = NULL*/)
{
	ASSERT(m_pWoormInfo);

	CString strPreviewFile;
	if (m_pWoormInfo->m_bConcatPDF || m_bDirectConcatPDF)
	{
		CString strUniqueFile = m_pWoormInfo->m_arstrOutputFileNames[0];

		ConcatPDFAttachments (strUniqueFile);

		// se devo concatenare i PDF in un unico file, ogni file PDF generato viene "fuso"
		// con quello in testa alla lista m_arstrPDFFiles
		CString strCurrentFile = m_pWoormInfo->m_arstrOutputFileNames[m_pWoormInfo->m_nNextReport - 1];
		if (strCurrentFile.CompareNoCase(strUniqueFile))
		{
			ConcatPDF (strCurrentFile, strUniqueFile);
		}

		if (m_pWoormInfo->IsLastReport()) 
			strPreviewFile = strUniqueFile;
	}
	else 
	{
		strPreviewFile = psFileName ? *psFileName : m_pWoormInfo->m_arstrOutputFileNames[m_pWoormInfo->m_nNextReport - 1];
	}

	if (
			m_pWoormInfo->m_bPDFOutputPreview && 
			!strPreviewFile.IsEmpty() &&
			ExistFile(strPreviewFile)
		)
	{
		HINSTANCE hInst = ::TBShellExecute(strPreviewFile);
		if (hInst <= (HINSTANCE)32)
			Message (ShellExecuteErrMsg((int)hInst));
	}
	
	if (psFileName)	*psFileName = strPreviewFile;
}

//------------------------------------------------------------------------------
BOOL CWoormDocMng::ConcatPDF(const CString strOrigin, const CString strDestination)
{
	if (strOrigin.CompareNoCase(strDestination) == 0)
		return TRUE;

	if (!ExistFile(strOrigin))
	{
		ASSERT(FALSE);
		return FALSE;
	}

	if (!ExistFile(strDestination))
	{
		RenameFilePath (strOrigin, strDestination);
		return TRUE;
	}

	CStringArray ar;
	ar.Add(strDestination);
	ar.Add(strOrigin);

	if (m_pWoormInfo && (m_pWoormInfo->m_Email.m_PostaLiteMsg.m_bUsePostaLite || m_pWoormInfo->m_bArchivePdfFormat))
	{
		::ConcatPdf(ar, strDestination);
	}
	else
	{
		MailConnectorParams* params = AfxGetIMailConnector()->GetParams();

		CString pwd;
		if (params->GetCryptFile())
		{
			pwd = params->GetPassword();
			if (pwd.IsEmpty()) 
				pwd = DEFAULT_CRYPT_PDF_PASSWORD;
		}
		::ConcatPdf(ar, strDestination, pwd);
	}
	//DeleteFile(strOrigin);
	return TRUE;
}

//------------------------------------------------------------------------------
BOOL CWoormDocMng::ConcatPDFAttachments (const CString strDestination)
{
	for (int i=0; i < m_Email.m_Attachments.GetSize(); i++)
	{
		CString sAtt = m_Email.m_Attachments.GetAt(i);

		if (sAtt.Right(4).CompareNoCase(_T(".pdf")) == 0)
		{
			if (!::CheckPdf(sAtt))
				continue;

			if (ConcatPDF(sAtt, strDestination))
			{ 
				m_Email.m_Attachments.RemoveAt(i);
				if (i < m_Email.m_AttachmentTitles.GetSize())
					m_Email.m_AttachmentTitles.RemoveAt(i);
				i--;
			}
		}
	}
	return TRUE;
}

//------------------------------------------------------------------------------
void CWoormDocMng::OnFileSavePDF()
{
	if (!CanDoFileSavePDF()) return;
	if (!AfxIsActivated(TBEXT_APP,  MAILCONNECTOR_ACT))
	{
		AfxMessageBox(LICENZE_TERMINATE);
		return;
	}

	CString strExt		= FileExtension::PDF_EXT();
	CString	strAny		= FileExtension::ANY_PDF();
	CString	strFilter	= FileExtension::PDF_FILTER();

	CWoormFileDialogPDF dialog(FALSE, strExt, strAny, WRITEFLAGS, strFilter);
	if ((dialog.DoModal() == IDOK))
	{
		CWoormView* pView = GetWoormView();
		if (!pView) 
			return;

		// force PDF extention for security
		CString strFile = MakeName(dialog.GetPathName(), FileExtension::PDF_EXT());

		if (!pView->OnPDFPrint(strFile))
		{
			Message(cwsprintf(_TB("Saving to file {0-%s} failed."), strFile));
			return;
		}

		if (m_bDirectConcatPDF)
		{
			ConcatPDFAttachments (strFile);
		}

		if (dialog.m_bPreview)
		{
			HINSTANCE hInst = ::TBShellExecute(strFile);
			if (hInst <= (HINSTANCE)32)
				Message (ShellExecuteErrMsg((int)hInst));
		}
		if (dialog.m_bCompress)
		{
			ZCompress(strFile, strFile + _T(".zip"), CString());
		}
	}
}
	
//------------------------------------------------------------------------------
BOOL CWoormDocMng::ArchivePdfFormat(BOOL bShowMsg /*=TRUE*/)
{
	if (!AfxGetOleDbMng()->EasyAttachmentEnable())
		return FALSE;
	
	CWoormView* pView = GetWoormView();
	if (!pView) 
		return TRUE;

	pView->BeginWaitCursor();
	
	CString strConcatFile;	
	CString strAlternativeName;	
	CString fileName;
	// m_pWoormInfo->m_bUseOutputFileName == TRUE se è stata utilizzata la funzione di WOORM ReportSaveAsAndAttach
	// in questo caso il report è già stato salvato in pdf
	//naturalmente dopo aver allegato non devo cancellare il pdf poichè non si tratta di un temporaneo di EA
	if (!m_pWoormInfo || (m_pWoormInfo && !m_pWoormInfo->m_bUseOutputFileName))
	{
		if (m_pWoormInfo)
		{
			if (m_pWoormInfo->m_bConcatPDF && m_pWoormInfo->m_arstrOutputFileNames.GetSize() > 0)
				strConcatFile = ::GetName(m_pWoormInfo->m_arstrOutputFileNames[0]);
			else
				strAlternativeName = (!m_pWoormInfo->m_Email.m_sAttachmentReportName.IsEmpty()) ? m_pWoormInfo->m_Email.m_sAttachmentReportName : m_Email.m_sAttachmentReportName;
		}
		else
			strAlternativeName = m_Email.m_sIdentity;

		fileName = AfxGetIDMSRepositoryManager()->GetPdfFileName(GetNamespace().ToString(), m_pCallerDocument, strAlternativeName);

		if (fileName.IsEmpty())
		{
			pView->EndWaitCursor();
			if (bShowMsg)
				Message(cwsprintf(_TB("Archiving report {0-%s} failed. Impossible to create the temporary file."), m_strTitle));
			return FALSE;
		}

		if (!pView->OnPDFPrint(fileName))
		{
			pView->EndWaitCursor();
			if (bShowMsg)
				Message(cwsprintf(_TB("Archiving file {0-%s} failed."), fileName));
			return FALSE;
		}
		ConcatPDFAttachments(fileName);	//concatena solo gli allegati

		if (m_pWoormInfo && m_pWoormInfo->m_bConcatPDF && !strConcatFile.IsEmpty())
		{
			strConcatFile = ::MakeFilePath(::GetPath(fileName), strConcatFile, L".pdf");
			ConcatPDF(fileName, strConcatFile);
			fileName = strConcatFile;
		}
	}

	BOOL bResult = TRUE;
	::ArchiveResult archRes = ::ArchiveResult::Cancel;
	if (!m_pWoormInfo || m_pWoormInfo->IsLastReport())
	{
		CString archivefileName = (m_pWoormInfo && m_pWoormInfo->m_bUseOutputFileName && m_pWoormInfo->m_arstrOutputFileNames.GetSize() > 0) ? m_pWoormInfo->m_arstrOutputFileNames[0] : fileName;
		CString strResult;
		if (m_pWoormInfo && m_pWoormInfo->m_pDocumentAttachTo)
			archRes = AfxGetIDMSRepositoryManager()->ArchiveReport(archivefileName, ::GetName(archivefileName), m_pWoormInfo->m_pDocumentAttachTo, m_dsEABarCode.m_strBarcodeValue, false, strResult);
		else
			archRes = AfxGetIDMSRepositoryManager()->ArchiveReport(archivefileName, (m_Email.m_sIdentity.IsEmpty()) ? m_strTitle : m_Email.m_sIdentity, m_pCallerDocument, m_dsEABarCode.m_strBarcodeValue, true, strResult);

		pView->EndWaitCursor();

		if (!strResult.IsEmpty() && bShowMsg && (!m_pCallerDocument || !m_pCallerDocument->IsInUnattendedMode()))
			Message(strResult, MB_OK || (archRes == ::ArchiveResult::TerminatedWithError) ? MB_ICONERROR : MB_ICONINFORMATION, 0U, (LPCTSTR)0, (archRes == ::ArchiveResult::TerminatedWithError) ? CMessages::MSG_ERROR : CMessages::MSG_HINT);

	}
	return archRes != ::ArchiveResult::TerminatedWithError;;
};

//------------------------------------------------------------------------------
BOOL CWoormDocMng::DoArchivePdfFormat()
{
	if (!AfxGetOleDbMng()->EasyAttachmentEnable())
		return FALSE;

	m_pWoormInfo->m_bArchived = ArchivePdfFormat(FALSE);

	return m_pWoormInfo->m_bArchived;
}
//------------------------------------------------------------------------------
void CWoormDocMng::OnArchivePdfFormat()
{
	if (!AfxGetOleDbMng()->EasyAttachmentEnable())
		return;
	
	CWoormView* pView = GetWoormView();
	if (!pView) 
		return;

	BOOL oldDirectConcatPDF = m_bDirectConcatPDF;
	BOOL oldConcatPDF =  (m_pWoormInfo) ? m_pWoormInfo->m_bConcatPDF : FALSE;
	if (m_pWoormInfo)
		m_pWoormInfo->m_bConcatPDF = TRUE;	
	m_bDirectConcatPDF = TRUE;		
	
	BOOL oldArchivePdfFormat = m_pWoormInfo ? m_pWoormInfo->m_bArchivePdfFormat : FALSE;
	if (m_pWoormInfo && !m_pWoormInfo->m_bArchivePdfFormat)
	{
		m_pWoormInfo->m_bArchivePdfFormat = TRUE;

		ValorizeEABarCode(GetAlternativeNameForEA(), TRUE);
	}

	if (m_bMustReRun)
		OnRun();
	else
		ArchivePdfFormat();
	
	if (m_pWoormInfo)
	{
		m_pWoormInfo->m_bArchivePdfFormat = oldArchivePdfFormat;
	}

	m_bDirectConcatPDF = oldDirectConcatPDF;
	if (m_pWoormInfo)
		m_pWoormInfo->m_bConcatPDF = oldConcatPDF;
}

//---------------------------------------------------------
void CWoormDocMng::OnArchiveWithChilds ()
{
	m_bMustReRun = TRUE;

	OnArchivePdfFormat();

	m_bMustReRun = FALSE;
}

//------------------------------------------------------------------------------
void CWoormDocMng::OnGeneratePapery()
{
	GeneratePapery();
}


//------------------------------------------------------------------------------
BOOL CWoormDocMng::GeneratePapery(BOOL bShowMsg /*=TRUE*/)
{
	if (!AfxGetOleDbMng()->EasyAttachmentEnable())
		return FALSE;

	CWoormView* pView = GetWoormView();
	if (!pView)
		return TRUE;

	pView->BeginWaitCursor();

	CString strAlternativeName = GetAlternativeNameForEA();
	CString fileName = AfxGetIDMSRepositoryManager()->GetPdfFileName(GetNamespace().ToString(), m_pCallerDocument, strAlternativeName);

	ValorizeEABarCode(strAlternativeName, TRUE);

	if (fileName.IsEmpty())
	{
		pView->EndWaitCursor();
		if (bShowMsg)
			Message(cwsprintf(_TB("Generate papery for {0-%s} failed. Report name is empty."), m_strTitle));
		return FALSE;
	}


	CString strResult;
	::ArchiveResult archRes = AfxGetIDMSRepositoryManager()->GeneratePapery(fileName, m_dsEABarCode.m_strBarcodeValue, m_pCallerDocument, strResult);
	pView->EndWaitCursor();
	if (!strResult.IsEmpty() && bShowMsg && !m_pCallerDocument->IsInUnattendedMode())
		Message(strResult, MB_OK || (archRes == ::ArchiveResult::TerminatedWithError) ? MB_ICONERROR : MB_ICONINFORMATION, 0U, (LPCTSTR)0, (archRes == ::ArchiveResult::TerminatedWithError) ? CMessages::MSG_ERROR  : CMessages::MSG_HINT);

	return archRes!= ::ArchiveResult::TerminatedWithError;
};

//------------------------------------------------------------------------------
void CWoormDocMng::OnExportData (UINT nID)
{               
	if (nID == ID_EXPORT_OPENOFFICE_ODS)
		m_pExportData->m_ExportInfo.SetExportType(TypeOfExport::EXPORT_OPENOFFICE_ODS_TYPE);
	else if (nID == ID_EXPORT_EXCELNET)
		m_pExportData->m_ExportInfo.SetExportType(TypeOfExport::EXPORT_EXCELNET_TYPE);
	else if (nID == ID_EXPORT_WORD_NET)
		m_pExportData->m_ExportInfo.SetExportType(TypeOfExport::EXPORT_WORDNET_TYPE);
	else if (nID == ID_EXPORT_OPENOFFICE_ODT)
		m_pExportData->m_ExportInfo.SetExportType(TypeOfExport::EXPORT_OPENOFFICE_ODT_TYPE);
	else if (nID == ID_EXPORT_TEXT)
		m_pExportData->m_ExportInfo.SetExportType(TypeOfExport::EXPORT_CSV_TYPE);
	else if (nID == ID_EXPORT_HTML)
		m_pExportData->m_ExportInfo.SetExportType(TypeOfExport::EXPORT_HTML_TYPE);
	else if (nID == ID_EXPORT_XML)
		m_pExportData->m_ExportInfo.SetExportType(TypeOfExport::EXPORT_XML_TYPE);
	else if (nID == ID_EXPORT_XML_FULL)
		m_pExportData->m_ExportInfo.SetExportType(TypeOfExport::EXPORT_XML_FULL_TYPE);
	else if (nID == ID_EXPORT_JSON)
		m_pExportData->m_ExportInfo.SetExportType(TypeOfExport::EXPORT_JSON_TYPE);
	else if (nID == ID_EXPORT_OPENXML_EXCEL)
		m_pExportData->m_ExportInfo.SetExportType(TypeOfExport::EXPORT_OPENXML_EXCEL_TYPE);
	else
	{
		ASSERT(FALSE);
		Message(_TB("Internal error. No object report is currectly selected: try to selct again objects that you want export."));
		return;
	}

	int np = m_pRDEmanager->CurrPageRead() + 1;

	m_pExportData->m_ExportInfo.SetCurrentPage(np);
	m_pExportData->m_ExportInfo.SetPagesNum((USHORT)(m_pRDEmanager->LastPage() + 1));
	m_pExportData->m_ExportInfo.SetPageRange(np, np);
	m_pExportData->m_ExportInfo.m_strReportNameSpace = this->GetNamespace().ToUnparsedString();
	m_pExportData->m_ExportInfo.m_strReportName = this->GetNamespace().GetObjectName();

	if (m_pExportData->m_ExportInfo.m_nExportType == TypeOfExport::EXPORT_FIELDS_TO_TEMPLATE)
	{
		BeginWaitCursor();
		m_pExportData->RunExport();
		EndWaitCursor();
		return;
	}

	if (!m_pExportData->m_pItem)
	{
		CArray<CExportTableItem*> m_parTables;
		m_pExportData->AutoAddTable(m_parTables);
		if (m_parTables.GetSize() == 0)		//seleziono una tabella
		{
			Message(_TB("Non risulta correttamente selezionato alcun oggetto del report: provare a ri-selezionare gli oggetti che si vuole esportare"));
			return;
		}
	}

	if (m_pExportData->m_ExportInfo.m_nExportType == TypeOfExport::EXPORT_XML_FULL_TYPE)
	{
		m_pExportData->RunExport();
		return;
	}

	CExportInfoDlg aDlg ( m_pExportData->m_ExportInfo, ! (m_pExportData->IsTableItem()),NULL);

	if (aDlg.DoModal () == IDOK)
	{
		m_pExportData->RunExport ();
	}
}

//------------------------------------------------------------------------------
void CWoormDocMng::OnFilePrintPreview()
{
	CWoormView* pView = GetWoormView();
	if (!pView) return;
	CPrintPreviewState *pState = new CPrintPreviewState;

	if (!pView->DoPrintPreview(AFX_IDD_PREVIEW_TOOLBAR, pView,
		RUNTIME_CLASS(CWoormPreviewView), pState))
	{
		TRACE0("Error: OnFilePrintPreview failed.\n");
		AfxMessageBox(AFX_IDP_COMMAND_FAILURE);
		delete pState;      // preview failed to initialize, delete State now
	}
}

//------------------------------------------------------------------------------
void CWoormDocMng::OnFindNext()
{
	m_findWordInfo.m_bIsDown = TRUE;
	OnFindWord();
}

//------------------------------------------------------------------------------
void CWoormDocMng::OnFindPrev()
{
	m_findWordInfo.m_bIsDown = FALSE;
	OnFindWord();
}

//------------------------------------------------------------------------------
void CWoormDocMng::OnFindWord()
{
	CTBTabbedToolbar* pTabbedBar = GetWoormFrame()->GetTabbedToolBar();
	if (!pTabbedBar) return;
	CTBToolBar* pToolBar = pTabbedBar->FindToolBar(ID_TOOLBAR_FIND_EDITBOX);
	if (!pToolBar) return;
	CString strFindWord = pToolBar->GetTextContent(ID_TOOLBAR_FIND_EDITBOX);
	if (strFindWord.IsEmpty())	
	{
		FindWordClear();
		return;
	}
	FindWord (strFindWord, m_findWordInfo.m_bIsDown);
}

//------------------------------------------------------------------------------
BOOL CWoormDocMng::IsFindWordOnSingleColumn ()
{
	CExportTableItem* pItem = m_pExportData->GetExportTableItem();
	if (!pItem)
		return FALSE;

	return pItem->m_SelectedCol.GetSize() == 1;
}

//------------------------------------------------------------------------------
BOOL CWoormDocMng::FindWord (CString strFindWord, BOOL bIsDown)
{
	ASSERT (m_pRDEmanager);
	if (!m_pRDEmanager)
		return FALSE;
	if (m_pRDEmanager->IsClosed())
	{
		FindWordClear();
		return FALSE;
	}

	ASSERT_VALID (m_pExportData);
	if (!m_pExportData)
		return FALSE;
	
	Table* pTable = NULL;

	CExportTableItem* pItem = m_pExportData->GetExportTableItem();
	if (pItem)
	{
		ASSERT_VALID (pItem);
		if (pItem->m_SelectedCol.GetUpperBound() < 0)
		{
			ASSERT(FALSE);
			return FALSE;
		}

		pTable = GetObjects().FindTableByID(pItem->GetTableID());
		ASSERT_VALID(pTable);
	}
	else 
	{
		for (int i = 0; i <= GetObjects().GetUpperBound (); i++)
		{
			BaseObj* pObj = GetObjects()[i];
			if ( pObj->IsKindOf (RUNTIME_CLASS(Table)) ) 
			{
				pTable = (Table*) pObj;
				break;
			}
		}
	}
	if (!pTable)
	{
		FindWordClear();
		return FALSE;
	}
	
	if (strFindWord.IsEmpty())
	{
		FindWordClear();
		return FALSE;
	}

	int nValidRows = 0;
	int nPageIdx = 0;
	int nColIdx  = 0;
	int nRowIdx  = 0;
	int	nPagesToExport = 0;    
	CString strLabel;
	CString strValue;
	bool bOutputTitle = false;
	int nRowMax  = -1;
	int nColMax  = -1;
	
	strFindWord.MakeLower();

	m_findWordInfo.m_eCmp = m_iFindCondition;

	if (m_findWordInfo.m_strFindWord.Compare(strFindWord) == 0)
	{
		nPageIdx	= m_findWordInfo.m_nCurrPage;
		nColIdx		= bIsDown ? (m_findWordInfo.m_nCurrCol + 1) : (m_findWordInfo.m_nCurrCol - 1);
		nRowIdx		= m_findWordInfo.m_nCurrRow;
	}
	else
	{
		FindWordClear();

		m_findWordInfo.m_strFindWord = strFindWord;
		if (IsFindWordOnSingleColumn ())
		{
			int nCol = m_pExportData->GetExportTableItem()->m_SelectedCol[nColIdx].m_ColIdx;
			const TableColumn* pCol = pTable->GetColumn(nCol);
			ASSERT_VALID(pCol);

			m_findWordInfo.m_Type = pCol->GetDataType();

			// set default search mode in select the collum
			if (
				m_findWordInfo.m_Type == DATA_STR_TYPE ||
				m_findWordInfo.m_Type == DATA_TXT_TYPE
				)
			{
				m_iFindCondition = ECompareType::CMP_CONTAINS;
			}
			else
			{
				m_iFindCondition = ECompareType::CMP_EQUAL;
			}

			m_findWordInfo.m_eCmp = m_iFindCondition;
		}
	}

	try 
	{	
		Table* pFirstTable = pTable;
		int nAliasTable = pTable->GetInternalID();
		BOOL bMultiLayout = (m_Layouts.GetCount() > 1);

		if (bIsDown)
		{
			nRowMax			= pTable->LastRow();
			nColMax			= pTable->GetColumns().GetCount();
			nPagesToExport  = (USHORT)(m_pRDEmanager->LastPage());
		}
		
		if (bIsDown)
			nPagesToExport += 1;
		else
			nPagesToExport -= 1;

		for (; nPageIdx != nPagesToExport; bIsDown ? nPageIdx++ : nPageIdx--)
		{
			if (nPageIdx <= -1) break;

			//rileggo la pagina RDE necessaria
			if (m_pRDEmanager->CurrPageRead() != nPageIdx)
			{
				ReadSelectedPage (nPageIdx, TRUE);
			}
			
			if (bMultiLayout)
			{
				//la tabella potrebbe non esserci od avere una struttura differente (numero righe e/o colonne)
				pTable = (Table*) (GetObjects().FindByID(nAliasTable));
				if (pTable == NULL || !pTable->IsKindOf(RUNTIME_CLASS(Table)))
					continue;
			}

			for (;nRowIdx != nRowMax; bIsDown ? nRowIdx++ : nRowIdx--)
			{
				if (nRowIdx <= -1) break;

				int nRow = ((pTable == pFirstTable) && pItem) ? pItem->m_Rows.GetAt(nRowIdx) : nRowIdx;

				UpdateViewSymbolTable(pTable, nRow);
				//----

				for (; nColIdx != nColMax;  bIsDown ? nColIdx++ : nColIdx-- )
				{
					if (pItem)
					{
						if (nColIdx < 0) 
						{
							//ASSERT(FALSE);
							break;
						}
						if (nColIdx >= pItem->m_SelectedCol.GetSize())
						{
							//ASSERT(FALSE);
							break;
						}
					}

					int nCol = pItem ? pItem->m_SelectedCol[nColIdx].m_ColIdx : nColIdx;
					if (nCol < 0 || nCol >= pTable->GetColumns().GetCount())
					{
						ASSERT(FALSE);
						break;
					}

					const TableColumn* pCol = pTable->GetColumn(nCol);
					ASSERT_VALID(pCol);
					if (!pCol || pCol->IsHidden())
						continue;
					//----

					strValue = "";
					DataType oDataType;
					BOOL bIsTailMultiLine = FALSE;
					void* pDataValue = NULL;

					if (m_pExportData->ConvertCellDataToString( this, strValue,	pTable, nRow, nCol,	oDataType, pDataValue, bIsTailMultiLine))
					{
						BOOL bMatched = FALSE;

						DataObj* pObjCell = DataObj::DataObjCreate(m_findWordInfo.m_Type);
						if (pObjCell) pObjCell->Assign(strValue);

						DataObj* pObjWord = DataObj::DataObjCreate(m_findWordInfo.m_Type);
						if (pObjWord) pObjWord->Assign(strFindWord);

						if (
								m_findWordInfo.m_Type != DataType::Enum &&
								pObjCell && pObjWord && 
								pObjCell->IsValid() && pObjWord->IsValid()
							)
								bMatched = pObjCell->CompareBy(m_findWordInfo.m_eCmp, pObjWord);
						else
						{
							//strValue.MakeLower();
							//bMatched = strValue.Find(strFindWord) >= 0;

							DataStr dsCell(strValue);
							DataStr dsWord(strFindWord);

							bMatched = dsCell.CompareBy(m_findWordInfo.m_eCmp, &dsWord);
						}
						SAFE_DELETE(pObjCell);
						SAFE_DELETE(pObjWord);

						if (bMatched) 
						{
							if (m_findWordInfo.m_nCurrPage == nPageIdx && m_findWordInfo.m_nCurrRow >= 0)
							{
								int nc = m_findWordInfo.m_nCurrCol;
								if (pItem && nc < pItem->m_SelectedCol.GetSize())
								{
									nc = pItem->m_SelectedCol[nc].m_ColIdx;
								}

								int nr = m_findWordInfo.m_nCurrRow;
								if ((pTable == pFirstTable) && pItem)
								{
									nr = pItem->m_Rows.GetAt(nr);
								}

								TableCell*	pPrevCell = pTable->GetTableCell(nr, nc);
								if (pPrevCell)
									InvalidateRect(pPrevCell->GetCellRect(), FALSE);
							}

							m_findWordInfo.m_nCurrPage	= nPageIdx;
							m_findWordInfo.m_nCurrRow	= nRowIdx;
							m_findWordInfo.m_nCurrCol	= nColIdx;

							TableCell*	pCell = pTable->GetTableCell(nRow, nCol);
							if (pCell)
							{
								pCell->m_arSelectedOnPage.Add(nPageIdx);

								InvalidateRect(pCell->GetCellRect(), TRUE);
								UpdateWindow ();
							}

							return TRUE;
						}
					}					
				} //end for colonne
				
				nColIdx = bIsDown ? 0 : (pItem ? pItem->GetSizeSelectedColumns() : pTable->GetColumns().GetCount()) - 1;

			} //end for righe di una pagina

			nRowIdx = bIsDown ? 0 : min (pTable->LastRow(), (pItem ? pItem->GetRows().GetUpperBound() : pTable->LastRow()));
		}//end for pagine	
	
		return TRUE;
	} 
	catch (...) 
	{
		;
	}
	return FALSE;
}

//------------------------------------------------------------------------------
void CWoormDocMng::ClearFindCellArray(CLayout* ar)
{
	for (int i = 0; i <= ar->GetUpperBound(); i++)
	{
		if ((*ar)[i]->IsKindOf(RUNTIME_CLASS(Table)))
		{
			Table* pTable = (Table*) (*ar)[i];
			pTable->ClearCellDynamicAttributes(FALSE, TRUE, FALSE);
		}
	}
}

//------------------------------------------------------------------------------
void CWoormDocMng::ClearDynamicAttributes(CLayout* ar)
{
	for (int i = 0; i <= ar->GetUpperBound(); i++)
	{
		if ((*ar)[i]->IsKindOf(RUNTIME_CLASS(Table)))
		{
			Table* pTable = (Table*) (*ar)[i];
			pTable->ClearCellDynamicAttributes(TRUE, TRUE, TRUE);
		}
	}
}

//------------------------------------------------------------------------------
void CWoormDocMng::OnUsePrevAskInput ()
{
	m_bUsePrevAskInput = !m_bUsePrevAskInput;

	if (!m_bUsePrevAskInput)
	{
		this->m_bHideAllAskDialogs = FALSE;

		OnReInitHiddenInput();
	}
}

//------------------------------------------------------------------------------
void CWoormDocMng::OnUpdateUsePrevAskInput (CCmdUI* pCmdUI)
{
	pCmdUI->SetCheck(m_bUsePrevAskInput);
}

//------------------------------------------------------------------------------
void CWoormDocMng::OnReRunStartStop()
{
	StopReRunTimer();

	BOOL b = !m_bAllowEditing && !IsReportRunning();

	m_bReRun = !m_bReRun && b;
	m_bHideAllAskDialogs = m_bReRun;

 	CWoormFrame* pFrame =  GetWoormFrame();
	if (pFrame)
	{
		if (m_bReRun) 
			pFrame->ShowReStopButton();
		else pFrame->ShowReRunButton();
	}

	if (m_bReRun)
		StartReRunTimer();
}

//------------------------------------------------------------------------------
void CWoormDocMng::OnUpdateReRunStartStop(CCmdUI* pCmdUI)
{
	BOOL b = !m_bAllowEditing && !IsReportRunning() && m_bAllowReRun;

	pCmdUI->SetCheck(b && m_bReRun);
	pCmdUI->Enable(b);
}

//------------------------------------------------------------------------------
void CWoormDocMng::OnUpdateComboReRun(CCmdUI* pCmdUI)
{
	pCmdUI->Enable(!m_bAllowEditing && !IsReportRunning() && m_bAllowReRun);
}

//---------------------------------------------------------
///<summary>
///Close the Report
///</summary>
//[TBWebMethod(securityhidden=true, thiscall_method=true)]
DataBool CWoormDocMng::Close()
{
	WoormDocPtr ptr = this;
	if (ptr->IsReportRunning())
	{
		CPushMessageLoopDepthMng __pushLoopDepth(MODAL_STATE);
		AfxGetThreadContext()->RaiseCallBreakEvent();
		while (
			ptr &&
			ptr->IsReportRunning() &&
			CTBWinThread::PumpThreadMessages())
			::Sleep(1);
	}
	if (ptr)
		ptr->CloseDocument();

	return DataBool(TRUE);
}

///<summary>
///Print the Report
///</summary>
//[TBWebMethod(securityhidden=true, thiscall_method=true)]
DataBool CWoormDocMng::Print(DataBool bClose)
{
	WoormDocPtr ptrWDoc = this;

	if (ptrWDoc->IsReportRunning())
	{
		CPushMessageLoopDepthMng __pushLoopDepth(MODAL_STATE);
		AfxGetThreadContext()->RaiseCallBreakEvent();
		while (
			ptrWDoc &&
			ptrWDoc->IsReportRunning() &&
			CTBWinThread::PumpThreadMessages())
			::Sleep(1);
	}
	if (!ptrWDoc)
		return DataBool(FALSE);
	
	CWoormView* pView = ptrWDoc->GetWoormView();
	if (!pView)
		return DataBool(FALSE);

	pView->SendMessage(WM_COMMAND, ID_FILE_PRINT);

	if (ptrWDoc->IsReportRunning())
	{
		CPushMessageLoopDepthMng __pushLoopDepth(MODAL_STATE);
		AfxGetThreadContext()->RaiseCallBreakEvent();
		while (
			ptrWDoc &&
			ptrWDoc->IsReportRunning() &&
			CTBWinThread::PumpThreadMessages())
			::Sleep(1);
	}
	if (!ptrWDoc)
		return DataBool(FALSE);

	BOOL bRet = DataBool(!ptrWDoc->IsAborted() && ptrWDoc->m_deReportStatus == DataEnum (TAG_REPORT_STATUS, TAG_REPORT_STATUS_SUCCESS));
	
	if (bClose)
		ptrWDoc->Close();
	
	return bRet;
}

//---------------------------------------------------------
void CWoormDocMng::OnPrintWithDialog()
{
	CWoormView* pView = GetWoormView();
	if (!pView) 
		return;

	BOOL bSaveNoPrintDialog = TRUE;
	if (m_pWoormInfo)
	{
		bSaveNoPrintDialog = m_pWoormInfo->m_bNoPrintDialog;
		m_pWoormInfo->m_bNoPrintDialog = FALSE;
	}

	pView->SendMessage(WM_COMMAND, ID_FILE_PRINT);

	if (m_pWoormInfo)
	{
		m_pWoormInfo->m_bNoPrintDialog = bSaveNoPrintDialog;
	}
}

//------------------------------------------------------------------------------
void CWoormDocMng::OnTogglePrintOnLetterhead()
{
	m_bPrintOnLetterhead = !m_bPrintOnLetterhead;
}

//------------------------------------------------------------------------------
void CWoormDocMng::OnRunWithOutChild()
{
	m_bRunWithOutChild = TRUE;

	OnRun();

	m_bRunWithOutChild = FALSE;
}

//---------------------------------------------------------
void CWoormDocMng::OnPrintWithChild ()
{
	m_bMustReRun = TRUE;

	CWoormView* pView = GetWoormView();
	if (pView) 
	{
		pView->SendMessage(WM_COMMAND, ID_FILE_PRINT);
	}

	m_bMustReRun = FALSE;
}

//---------------------------------------------------------
void CWoormDocMng::OnPrintMultiCopies ()
{
	m_bMultiCopies = TRUE;

	int nCopies = 1;
	BOOL bLockedCopies = FALSE;
	BOOL bMultiCopies = FALSE;

	if (m_pWoormInfo)
	{
		m_pWoormInfo->m_bCloseOnEndPrint = TRUE;

		bLockedCopies = m_pWoormInfo->m_bLockedCopies;
		m_pWoormInfo->m_bLockedCopies = TRUE;
		
		nCopies = m_pWoormInfo->m_nCopies;
		m_pWoormInfo->m_nCopies = 1;

		bMultiCopies = m_pWoormInfo->m_bMultiCopies;
		m_pWoormInfo->m_bMultiCopies = TRUE;
	}

	OnPrintWithChild ();

	m_bMultiCopies = FALSE;

	if (m_pWoormInfo)
	{
		m_pWoormInfo->m_bLockedCopies = bLockedCopies;
		m_pWoormInfo->m_nCopies = nCopies;
		m_pWoormInfo->m_bMultiCopies = bMultiCopies;
	}
}

//---------------------------------------------------------
void CWoormDocMng::OnRunAndAttachChild()
{
	if (!CanDoSendMail()) return ;

	BOOL bAlloc = FALSE;

	ASSERT(AfxGetBaseApp()->GetTaskBuilderAddOnApp()->m_pAddOnModules);
	MailConnectorParams* params = AfxGetIMailConnector()->GetParams();

	BOOL bAttachRDE = FALSE;
	BOOL bAttachOther = FALSE;
	BOOL bAttachPDF = TRUE;
	BOOL bCompressAttach= params->GetMailCompress();

	BOOL bOldAttachRDE = FALSE;
	BOOL bOldAttachOther = FALSE;
	BOOL bOldAttachPDF = FALSE;
	BOOL bOldCompressAttach = TRUE;

	if (!AfxGetIMailConnector()->ShowEmailWithChildDlg(&bAttachRDE, &bAttachPDF, &bCompressAttach, &m_bDirectConcatPDF))
	{
		return;
	}

	if (m_pWoormInfo == NULL)
	{
		m_pWoormInfo = new CWoormInfo();
		m_pWoormInfo->m_bOwnedByReport = TRUE;
		bAlloc = TRUE;
	}
	else
	{
		bOldAttachRDE = m_pWoormInfo->m_bAttachRDE;
		bOldAttachOther = m_pWoormInfo->m_bAttachOther;
		bOldAttachPDF = m_pWoormInfo->m_bAttachPDF;
		bOldCompressAttach = m_pWoormInfo->m_bCompressAttach;
	}

	m_pWoormInfo->m_bAttachRDE = bAttachRDE;  
	m_pWoormInfo->m_bAttachPDF = bAttachPDF;
	m_pWoormInfo->m_bAttachOther = bAttachOther;
	m_pWoormInfo->m_bCompressAttach = bCompressAttach;

	m_pWoormInfo->m_Email.m_Attachments.RemoveAll();
	m_pWoormInfo->m_Email.m_AttachmentTitles.RemoveAll();

	m_bAttachReportChild = TRUE;

	OnRun();

	//if (!AfxGetTbCmdManager()->ExistDocument(this))
	//	return;
	ASSERT_VALID(this);

	m_bAttachReportChild = FALSE;

	if (bAlloc)
	{
		delete m_pWoormInfo;
		m_pWoormInfo = NULL;
	}
	else
	{
		m_pWoormInfo->m_bAttachRDE = bOldAttachRDE;  
		m_pWoormInfo->m_bAttachOther = bOldAttachOther;  
		m_pWoormInfo->m_bAttachPDF = bOldAttachPDF;
		m_pWoormInfo->m_bCompressAttach = bOldCompressAttach;
	}

	CString strLocal = ::_tsetlocale (LC_ALL, NULL);
	if (AfxGetIMailConnector()->ShowEmailDlg(m_pCallerDocument, m_Email, NULL,NULL,NULL,NULL,NULL,&m_bDirectConcatPDF))
	{
		BOOL bRet = DoSendMail (FALSE, FALSE, FALSE, FALSE, params->GetRequestDeliveryNotifications() , params->GetRequestReadNotifications(), FALSE);
	}
	CString strLocal2 = ::_tsetlocale (LC_ALL, NULL);
	if (strLocal != strLocal2)
	{
		::_tsetlocale (LC_ALL, strLocal);
	}
}

//------------------------------------------------------------------------------
void  CWoormDocMng::SetReRunTimer(UINT nTime)
{ 
	m_nReRunTimer = nTime;
}

//------------------------------------------------------------------------------
BOOL  CWoormDocMng::StartReRunTimer()
{
	StopReRunTimer();

	if (m_nReRunTimer <= 0)
		return FALSE;

	if (GetFirstView() == NULL)
		return FALSE;

	// Minuto to elapse
	UINT nWoormElapse = m_nReRunTimer * (1000 * 60);
	m_nReRun = GetFirstView()->SetTimer(IDT_WOORM_RERUN, nWoormElapse, NULL);
	return (m_nReRun != 0);
}

//------------------------------------------------------------------------------
BOOL  CWoormDocMng::StopReRunTimer()
{
	if (m_nReRun != 0)
	{
		GetFirstView()->KillTimer(m_nReRun);
		return TRUE;
	}
	return FALSE;
}

//------------------------------------------------------------------------------
void CWoormDocMng::ReRun()
{
	if (!PrepareParamsForReRun())
	{
		m_bHideAllAskDialogs = m_bReRun = FALSE;
		return;
	}

	m_bHideAllAskDialogs = m_bReRun;
	
	OnRun();
}

//------------------------------------------------------------------------------
void CWoormDocMng::OnRun()
{        
	if (m_bAborted)
		return;

	StopReRunTimer();

	ASSERT_VALID(this);
	CString strTmp;
	BOOL bOk = TRUE;

	//nel caso di macrorecorder attivo disabilita il recording per il tempo in cui è su il report 
	CApplicationContext::MacroRecorderStatus localStatus = AfxGetApplicationContext()->m_MacroRecorderStatus;
	AfxGetApplicationContext()->m_MacroRecorderStatus = CApplicationContext::IDLE;

	// skip if already running (can be called also from DDE) or in localizing mode 
	if (m_bEngineRunning) 
		goto l_skiprun;

	// se c'è l'OSL verifico se il report può essere lanciato
	if (!OSL_CAN_DO(GetInfoOSL(), OSL_GRANT_EXECUTE)) 
	{
		Message(cwsprintf(OSLErrors::MISSING_GRANT(), GetInfoOSL()->m_Namespace.ToString()));
		goto l_skiprun;
	}

	// in m_bPlayback cannot execute report but only m_bPlayback it
	if (m_bPlayback)
	{                    
		// force reading first page without checking current page that it is undefined
		if (!m_pRDEmanager->IsClosed() || !m_ReadingPage.IsLocked())
		{
			m_pRDEmanager->SeekFirstPage();

			// read real data
			ReadPage();
	
			m_ReadingPage.Unlock();
		}
		goto l_skiprun;
	}
	
	// run report only if there are database information
	if (m_bOnlyGraphInfo)	
		goto l_skiprun;

	m_Layouts.EnumObjects(&CWoormDocMng::ClearDynamicAttributes);

	if 
		(
			m_pWoormInfo && 
			m_pWoormInfo->m_nNextReport == 1 &&
			m_pWoormInfo->m_bOwnedByReport &&
			!m_pWoormInfo->m_bMultiCopies
		)
		m_pWoormInfo->LockCopies (FALSE);

	if (m_pWoormInfo && m_pWoormInfo->m_bAutoPrint && m_pWoormInfo->m_bShowPrintDialogBeforeRunning && !IsRunningFromExternalController())
	{
		CPrintDialog1 dlg(GetFirstView());
		
		if (m_pOptions && !m_pOptions->m_strDefaultPrinter.IsEmpty())
		{
			//inserire qui la chiamata alla print 
			HGLOBAL hDevMode;
			HGLOBAL hDevNames;

			if (GetPrinterDevice(m_pOptions->m_strDefaultPrinter.GetString(), &hDevNames, &hDevMode))
			{
				dlg.m_pd.hDevNames = hDevNames;
				dlg.m_pd.hDevMode = hDevMode;
			}	
			else
			{
				ASSERT(FALSE);
			}
		}
		int res = dlg.DoModal();
		if (res == IDCANCEL || dlg.GetCopies() != 1)
		{
			if (res != IDCANCEL)
			{
				::AfxMessageBox(_TB("The copies number depends on settings of multicopies report"));
			}

			m_bMustReRun = TRUE;
			m_pChildReport = NULL;
			m_pWoormInfo->m_bPrintAborted = TRUE;
			m_pWoormInfo->m_bPrinted = FALSE;
			if (m_pOldWoormInfo)
			{
				m_pOldWoormInfo->m_bPrintAborted = TRUE;
				m_pOldWoormInfo->m_bPrinted = FALSE;
			}
			////////////////////////
			CloseDocument();

			// set timer for ReRun
			StartReRunTimer();
			return;
		}

		// il caso del multicopia setta le copie a 1 e poi le gestisce lui
		if (!m_pWoormInfo->AreCopiesLocked())
		{
			m_pWoormInfo->m_nCopies = 1;
			m_pWoormInfo->LockCopies (TRUE);
		}
		m_pWoormInfo->m_strPrinterName = dlg.GetDeviceName();
		m_pWoormInfo->m_bShowPrintDialogBeforeRunning = FALSE;
		m_pWoormInfo->m_bNoPrintDialog = TRUE;
		DeleteDC(dlg.m_pd.hDC);
	}

	// build a strTmp strFile name
	strTmp = m_strPathName;

	// set flags before starting real backgroud work
	GetWoormFrame()->ShowStopButton();

	m_bEngineRunning = TRUE; 
	
	SetCustomUICulture();

	// ordina gli oggetti prima di stamparli per ottimizzare su dispositivi
	// non raster (tipo stampanti ad aghi) se richiesto dall'utente (default si)
	if (m_bAllowEditing)
		SortLayoutObjectsOnPosition();

	if (m_bAllowEditing &&  m_pEditorManager && m_pEditorManager->GetModifiedFlag())
	{	
		strTmp = GetTempName(FileExtension::WRM_EXT());
		UnparseReport(strTmp);	// unparse on a temporary file
	}

	//---- bug 16326, 16547
	m_Email.m_To.RemoveAll();
	m_Email.m_CC.RemoveAll();
	m_Email.m_BCC.RemoveAll();
	m_Email.m_Attachments.RemoveAll();
	m_Email.m_AttachmentTitles.RemoveAll();
	// Anomalia n. 23644 Lara
	m_Email.m_sSubject.Empty();
	m_Email.m_sBody.Empty();

	CExportInfo* pExp = NULL;
	BOOL bAttachOther = FALSE;

	if (m_pWoormInfo)
	{
		if (m_pWoormInfo->m_nNextReport > 0 && (m_pWoormInfo->m_nNextReport - 1) <  m_pWoormInfo->m_arExportInfo.GetSize())
			pExp = (CExportInfo*)(m_pWoormInfo->m_arExportInfo.GetAt(m_pWoormInfo->m_nNextReport - 1));
		bAttachOther = pExp != NULL;

		if (m_pWoormInfo->m_bSendEmail || m_pWoormInfo->m_bAttachPDF || m_pWoormInfo->m_bAttachRDE || bAttachOther || m_pWoormInfo->m_bArchivePdfFormat)
		{
			m_Email = m_pWoormInfo->m_Email;

			// se aggancio un file PDF unico, tutti gli allegati vengono messi in una mail unica
			if (m_pWoormInfo->m_bAttachPDF && (m_pWoormInfo->m_bConcatPDF || m_bDirectConcatPDF))
				m_pWoormInfo->m_bUniqueMail = TRUE;

			//vedi duale in LABEL 1001 
			if (!m_pWoormInfo->m_bSendEmail && (m_pWoormInfo->m_bAttachPDF || m_pWoormInfo->m_bAttachRDE || bAttachOther))
			{
				m_Email.m_Attachments.RemoveAll();
				m_Email.m_AttachmentTitles.RemoveAll();
			}
		}
	}

	///////////////////////////////////////////////////////////////////////////
	this->m_deReportStatus.Clear();

	m_sRunnedFilePath = strTmp;

	bOk = RunEngine(strTmp);

	m_sRunnedFilePath.Empty();

	if (
			!bOk ||
			(
				this->m_deReportStatus != DataEnum (TAG_REPORT_STATUS, TAG_REPORT_STATUS_SUCCESS) &&
				this->m_deReportStatus != DataEnum (TAG_REPORT_STATUS, TAG_REPORT_STATUS_NO_DATA_FOUND)
			)
		)
		goto l_endrun;

	//TODOLUCA  eliminata fino a quando non decidiamo il da farsi
	//BOOL bAllowTrack 
	//escludere wrmradar woorminfo hide frame, archive, sub runreport, mail/postalite, export, callerdocument
	/*if (!IsInUnattendedMode() && AfxIsActivated(TBNET_APP, _T("MenuHistory")))
	{
		const CLoginInfos* pInfos =  AfxGetLoginContext()->GetLoginInfos();
		if (pInfos != NULL)
		{
			CString ns = GetNamespace().ToString();
			ns.Replace(_T(".wrm"), _T(""));
			AddToHistory(AfxGetLoginContext(), ns, GetNamespace().GetTypeString(), GetReportParametersKey(), this->GetTitle(), this->GetDefaultMenuDescription());
		}
	}*/
	
	///////////////////////////////////////////////////////////////////////////
	if (!IsReportString())
		ValorizeEABarCode(); //valorizzazione barcode per EA //BugFix 22991

	if (m_pWoormInfo)	
	{
		//può essere stato cambiato dall'engine
		if (m_pWoormInfo->m_nNextReport > 0 && (m_pWoormInfo->m_nNextReport - 1) < m_pWoormInfo->m_arExportInfo.GetSize())
			pExp = (CExportInfo*)(m_pWoormInfo->m_arExportInfo.GetAt(m_pWoormInfo->m_nNextReport - 1));
		bAttachOther = pExp != NULL;
	}

	///////////////////////////////////////////////////////////////////////////
	m_bIsExporting = TRUE;
	//NON SI PUO' anticipare: m_bEngineRunning = FALSE;

	MailConnectorParams* params = AfxGetIMailConnector()->GetParams();

	//In stato di New manca il WoormInfo!!!! ASSERT(m_pWoormInfo);
	if (m_pWoormInfo)
	{		
		if (m_pWoormInfo->m_bShowUI2SendMail)
		{
			OnSendMail();	
		}
		else if (m_pWoormInfo->m_bPDFOutput)
		{
			bOk = DoPDFOutput();	
		}
		else if (m_pWoormInfo->m_bSendEmail)
		{
			BOOL bRet = DoSendMail
							(
								TRUE, 
								m_pWoormInfo->m_bAttachRDE, 
								m_pWoormInfo->m_bAttachPDF, 
								m_pWoormInfo->m_bCompressAttach,
								params->GetRequestDeliveryNotifications() , params->GetRequestReadNotifications(),
								bAttachOther
							);

			m_pWoormInfo->m_Email = m_Email;
		}
		else if (m_pWoormInfo->m_bAttachPDF || m_pWoormInfo->m_bAttachRDE || bAttachOther)
		{
			BOOL bRet = DoAttachReport
							(
								TRUE, 
								m_pWoormInfo->m_bAttachRDE, 
								m_pWoormInfo->m_bAttachPDF, 
								m_pWoormInfo->m_bCompressAttach,
								bAttachOther
							);
			ASSERT(m_Email.m_Attachments.GetSize());

			//m_pWoormInfo->m_Email = m_Email; //vedi duale in LABEL 1001 - NO!
			m_pWoormInfo->m_Email.m_Attachments.Append(m_Email.m_Attachments);
			m_pWoormInfo->m_Email.m_AttachmentTitles.Append(m_Email.m_AttachmentTitles);
		}	
		else if (m_bDirectSendMail)
		{
				BOOL bRet = DoSendMail
								(
									TRUE, 
									m_bDirectAttachRDE, 
									m_bDirectAttachPDF, 
									m_bDirectCompressAttach,
									params->GetRequestDeliveryNotifications() , params->GetRequestReadNotifications(),
									bAttachOther
								);
				m_bDirectSendMail = FALSE; m_Email.Clear();
		}
		else if 
				(
					m_pWoormInfo->m_bRDEOutput &&
					m_pWoormInfo->m_arstrOutputFileNames.GetSize() &&
					m_pWoormInfo->m_nNextReport <= m_pWoormInfo->m_arstrOutputFileNames.GetSize()
				)
		{
			bOk = DoRDEOutput() && bOk;
		}
		else if 
				( 
					m_pWoormInfo->m_arExportInfo.GetSize () > 0 &&
					m_pWoormInfo->m_nNextReport <= m_pWoormInfo->m_arExportInfo.GetSize()
				) 
		{
			bOk = DoExportOutput() && bOk;
		}
	}
	else if (m_bDirectSendMail)
	{
			BOOL bRet = DoSendMail
							(
								TRUE, 
								m_bDirectAttachRDE, 
								m_bDirectAttachPDF, 
								m_bDirectCompressAttach,
								params->GetRequestDeliveryNotifications() , params->GetRequestReadNotifications(),
								bAttachOther
							);
			m_bDirectSendMail = FALSE; m_Email.Clear();
	}

	//-------------------------------------------------------
	if (m_pWoormInfo && m_pWoormInfo->m_bArchivePdfFormat)
	{
		bOk = DoArchivePdfFormat();

		m_pWoormInfo->m_Email.RemoveAllPdfAttachments();
	}

	///////////////////////////////////////////////////////////////////////////
l_endrun:

	if (m_pEditorManager->GetModifiedFlag())
		DeleteFile(strTmp);		// delete temporary file
				 
	m_bIsExporting = FALSE; 
	m_bEngineRunning = FALSE; 

	ResetCustomUICulture();

   GetWoormFrame()->ShowRunButton();

   if (m_bNeedFinalInvalidate) 
	   GetWoormFrame()->Invalidate();
	
	// ---- esegue eventuali report in cascata
	if (bOk) 
	{
		if (
				m_pWoormInfo && 
				(
					m_pWoormInfo->m_ReportNames.GetSize() > 1 ||
					!m_pWoormInfo->m_bOwnedByReport
				)
			)
			m_pWoormInfo->LockCopies (TRUE);

		ExecuteNextReport();
	}
	else // anche in caso di errore se richiesto devo chiudere il report	
		if  (m_pWoormInfo && (m_pWoormInfo->m_bCloseOnEndPrint || m_pWoormInfo->m_bHideFrame))
		{
			m_pChildReport = NULL;
			CloseDocument();
		}

	///////////////////////////////////////////////////////////////////////////////
l_skiprun:

	if	(IsEditingParamsFromExternalController() || IsRunningFromExternalController())
	{
		if (IsRunningFromExternalController())
		{
			SetRunningTaskStatus(bOk ? CExternalControllerInfo::TASK_SUCCESS : CExternalControllerInfo::TASK_FAILED);
			m_pExternalControllerInfo->m_Finished.Set();

			m_pExternalControllerInfo = NULL;

			if (m_pWoormInfo && !m_pWoormInfo->m_bCloseOnEndPrint)
			{
				m_bIsUserReportsDeveloper = FALSE;
				m_bAllowReRun = FALSE;
				AfxGetApplicationContext()->SetInUnattendedMode(FALSE);	

				ASSERT_VALID(m_pWoormInfo);
				if (!m_pWoormInfo->m_bOwnedByReport)
				{
					CWoormInfo* pNewWI = new CWoormInfo(*m_pWoormInfo);
					pNewWI->m_bOwnedByReport = TRUE;
					pNewWI->m_nNextReport = 1;
					m_pWoormInfo = pNewWI;
				}
			}
		}
		else
		{
			SetRunningTaskStatus(bOk ? CExternalControllerInfo::TASK_SAVE_PARAMS : CExternalControllerInfo::TASK_CANCEL_PARAMS);
			m_pExternalControllerInfo->m_Finished.Set();
		}
	}
	//ripristina lo stato del macrorecorder
	AfxGetApplicationContext()->m_MacroRecorderStatus = localStatus;

	m_bEngineRunning = FALSE;
	m_bIsExporting = FALSE;

	// set timer for ReRun
	StartReRunTimer();
}

//------------------------------------------------------------------------------
CString CWoormDocMng::GetReportParametersKey()
{
	CString strKey;
	for (int i = 0; i < m_ViewSymbolTable.GetSize(); i++)
	{
		WoormField* pF = m_ViewSymbolTable.GetAt(i);
		ASSERT_VALID(pF);
		if (!pF->GetData()) 
			continue;

		if (m_pCallerDocument)
		{
			if (!pF->IsInput()) 
				continue;
		}
		else if (!pF->IsAsk())
			continue; 

		if (!pF->GetData())
			continue;

		DataType t = pF->GetData()->GetDataType();
		if (t == DataType::Array || t == DataType::Void)	//TODO per altri tipi complessi (es Record)
			continue;

		CString v = pF->GetData()->Str(0,0);

		if (v.Find(';') > -1)
		{
			ASSERT(FALSE);
			continue;
		}

		strKey += pF->GetName() + '=' + v + ';';
	}
	strKey.TrimRight(';');
	return strKey;
}

//------------------------------------------------------------------------------
void CWoormDocMng::OnRunStop()
{              
	if (m_bEngineRunning) 
	{
		m_bDebugging = FALSE;

		OnStop(); 
	}
	else 
		OnRun();
}

//------------------------------------------------------------------------------
void CWoormDocMng::OnPauseResume()
{
	if (!m_bEngineRunning) return;
	
	if (m_pEngine->IsPaused()) 
		OnResume(); 
	else 
		OnPause();
}

//------------------------------------------------------------------------------
void CWoormDocMng::OnRunDebugger()
{
	if (m_bEngineRunning) 
	{
		OnStop(); 
	}

	m_bDebugging = TRUE;

	OnRun();
}

//------------------------------------------------------------------------------
void CWoormDocMng::OnReInitHiddenInput()
{
	if (m_bEngineRunning || m_pEngine == NULL) 
		return;
	
	m_pEngine->ClearStaticField();
}

//------------------------------------------------------------------------------
void CWoormDocMng::OnFileSaveRDE()
{
	CTBNamespace aNs (GetNamespace().GetType(), GetNamespace().GetApplicationName() + CTBNamespace::GetSeparator () +  GetNamespace().GetModuleName());
	CString s = GetNamespace().GetObjectName();
	s.Replace(FileExtension::WRM_EXT(), FileExtension::RDE_EXT());
	aNs.SetObjectName(s);

	CTBExplorer aExplorer (CTBExplorer::SAVE, aNs, TRUE);
	if (!aExplorer.Open ())
		return;
	
	CStringArray arPath;
	aExplorer.GetSelPathElements(&arPath);

	CString strFileNameRDE = arPath.GetSize() ? arPath.GetAt(0) : _T("");

	if (strFileNameRDE.IsEmpty())
		return;

	// force RDE extention for security
	strFileNameRDE = MakeName(strFileNameRDE, FileExtension::RDE_EXT());

	//----
	m_bReplaceHiddenWhenExpr = TRUE;
	if (aExplorer.GetIsSavedInCurrentLanguage())
	{
		m_bSaveAsWithCurrentLanguage = TRUE;
		m_strOriginFileName = ::GetName (GetPathName());
		m_strOriginDictionaryPath = GetDictionaryPath(TRUE);
	}

	// save with only localized graphics info but leave current wrm as is
	UnparseWoorm (strFileNameRDE, TRUE);	//RDE

	m_bSaveAsWithCurrentLanguage = FALSE;
	m_bReplaceHiddenWhenExpr = FALSE;
	//----

	// add data info to parsed woorm report (error recovered by RDEManager)
	m_pRDEmanager->SaveRDEFile (strFileNameRDE);
}

//------------------------------------------------------------------------------
void CWoormDocMng::OnCompile ()
{
	// skip if there are only graphic information to save on strFile
	if (m_bOnlyGraphInfo) return;

	// build a strTmp strFile name
	// must save on strTmp strFile for eventual modified object
	CString strTmp = GetTempName(FileExtension::WRM_EXT());

	// toggle running status
	m_bEngineRunning = TRUE; SetCustomUICulture();
	UnparseReport	(strTmp);
	CompileEngine	(strTmp);
	DeleteFile		(strTmp);
	m_bEngineRunning = FALSE; ResetCustomUICulture();
}


//------------------------------------------------------------------------------
void CWoormDocMng::OnWoormIni ()
{
	// read automatically from ini strFile
	WoormIni    old;

	// make modification directly on task.ini strFile
	CWoormIniDlg dialog;
	if (dialog.DoModal() == IDOK)
	{
		// read changed data
		m_pWoormIni->ReadWoormSettings ();
															 
		// modify and format all object with value
		if (old.m_Show != m_pWoormIni->m_Show)
			FormatAll();              
			
		// set new stack undo level for current report and for other new report
		m_pUndoStack->SetSize(30);

		// refresh di eventuali modifiche a video (tipo griglie etc..)
		Invalidate ();
		UpdateWindow();
	}
}

//------------------------------------------------------------------------------
void CWoormDocMng::OnOptions ()
{
	Options     buffer = *m_pOptions;
	COptionsDlg dialog (buffer, this);

	if (dialog.DoModal() == IDOK)
	{
		// set modified flag for force saving strFile
		if (!(buffer == *m_pOptions)) 
		{
			*m_pOptions = buffer;

			Invalidate ();
			UpdateWindow();

			SetModifiedFlag	();
		}
	}
}

//------------------------------------------------------------------------------
void CWoormDocMng::OnPageSetup()
{	
	PageInfo aPageInfo(m_PageInfo);

	if (m_pWoormInfo && m_pWoormInfo->AreCopiesLocked() )
		aPageInfo.SetCopies(m_pWoormInfo->m_nCopies);

	CPageSetupDlg dialog(aPageInfo);

	WORD  nFlags =	DM_ORIENTATION |
					DM_PAPERSIZE |
					DM_PAPERLENGTH |
					DM_PAPERWIDTH |
					DM_SCALE |
					DM_COPIES |
					DM_COLLATE;

	if (dialog.DoModal() == IDOK && aPageInfo != m_PageInfo)
	{
		m_PageInfo = aPageInfo;
		
		if (m_pWoormInfo)
			m_pWoormInfo->m_nCopies = m_PageInfo.GetCopies();

		CView* pView = GetFirstView();
		if (pView->IsKindOf(RUNTIME_CLASS(CWoormView))) 
		{
			((CWoormView*)pView)->m_pWatermark->Clear();
		}

		Invalidate ();
		UpdateWindow();
		SetModifiedFlag();
	}
}

//------------------------------------------------------------------------------
void CWoormDocMng::OnFileProperties()
{
	DoPropertiesSheet(this);
	SetModifiedFlag(m_pDocProperties && m_pDocProperties->IsModified());
}

//------------------------------------------------------------------------------
void CWoormDocMng::OnRecordValidation()
{
	CFunctionDescription fdDump;
	if (
		AfxIsActivated(_T("RanorexTestSupport"), _T("RanorexTestSupport")) &&
		AfxGetTbCmdManager()->GetFunctionDescription(_NS_WEB("RanorexTestSupport.RanorexTestSupport.AddOnsM4GO.DumpReport"), fdDump)
		)
	{
		fdDump.SetParamValue(_T("docHandle"), DataLng((long)this));
		BOOL bOk = AfxGetTbCmdManager()->RunFunction(&fdDump, 0);
	}


	CFunctionDescription fd;		
	if (
			AfxGetApplicationContext()->m_MacroRecorderStatus == CApplicationContext::RECORDING &&
			AfxGetTbCmdManager()->GetFunctionDescription(_NS_WEB("TestManager.TBMacroRecorder.TBMacroRecorder.RecordReportValidation"), fd)
		)
	{
		//fd.SetParamValue(_T("woormDocHandle"),	DataLng((long)this));
		BOOL bOk = AfxGetTbCmdManager()->RunFunction(&fd, 0);
		AfxGetApplicationContext()->m_MacroRecorderStatus = CApplicationContext::IDLE;
		CString strMsg = (bOk) ? _TB("Record report data validation succeded") : _TB("Record report data validation failed");
		AfxMessageBox(strMsg);
		AfxGetApplicationContext()->m_MacroRecorderStatus = CApplicationContext::RECORDING;
	}
}

//------------------------------------------------------------------------------
void CWoormDocMng::OnReportBegin()
{
	m_bRunIndicatorActive = TRUE;

	pchSortStatusString[0] = BLANK_CHAR;
	pchSortStatusString[1] = BLANK_CHAR;
	pchSortStatusString[2] = BLANK_CHAR;
	pchSortStatusString[3] = NULL_CHAR;

	// reset counter; and update status bar
	m_nRetrieveTic = 0;
	m_nSearchTic   = 0;
	UpdateStatusBar();

	// timer for update status bar (speed purpose)
	m_nTimerID = GetFirstView()->SetTimer(IDT_WOORM, TIMER_TIME_OUT, NULL);
}


// update status bar at any tic if there are no timer available
//------------------------------------------------------------------------------
void CWoormDocMng::OnSearchTic	()	
{   
	m_nSearchTic++;					
	if (!m_nTimerID)
		SetRunIndicator ();
}

// update status bar at any tic if there are no timer available
//------------------------------------------------------------------------------
void CWoormDocMng::OnRetrieveTic ()
{
	m_nRetrieveTic++;
	if (!m_nTimerID)
		SetRunIndicator ();
}

//------------------------------------------------------------------------------
void CWoormDocMng::ExecuteNextReport()	
{
	// Gestione degli automatismi su fine report
	if (m_pWoormInfo)
	{
		// Lancia in automatico la stampa e aspetta che la stessa termini.
		if (
				m_pWoormInfo->m_bAutoPrint &&
				(
					this->m_deReportStatus == DataEnum (TAG_REPORT_STATUS, TAG_REPORT_STATUS_SUCCESS) 
					||
					this->m_deReportStatus == DataEnum (TAG_REPORT_STATUS, TAG_REPORT_STATUS_NO_DATA_FOUND) 
				)
			)
			GetWoormFrame()->SendMessage(WM_COMMAND, ID_FILE_PRINT);

		BOOL bAborted = FALSE;

		if (m_pWoormInfo->m_bOnePrintDialog)
		{
			//setto no print dialog per i report successivi
			m_pWoormInfo->m_bNoPrintDialog = TRUE;
			//controllo se è stato fatto Cancel 
			if (m_pWoormInfo->m_bPrintAborted)
				bAborted = TRUE;
		}

		//chiusura automatica report corrente 
		BOOL bClose = m_pWoormInfo->m_bCloseOnEndPrint || m_pWoormInfo->m_bHideFrame;

		if (!bAborted)
		{
			// Lancia il prossimo report della catena. Non occorre controllare se esiste
			// perche lo fa la RunWoormReport che torna NULL se non ce ne sono piu'
			m_pChildReport = AfxGetTbCmdManager()->RunWoormReport(m_pWoormInfo, GetCallerDocument());
			if (m_pChildReport && GetCallerDocument())
			{
				ASSERT_VALID(m_pChildReport);
				ASSERT_VALID(GetCallerDocument());

				//se sono io l'owner e si tratta di report in cascata, devo togliermi e lasciare l'ownership al caller document,
				//altrimenti il report figlio si chiude quando io muoio
				if
					(
						m_pChildReport->GetWoormFrame() &&
						m_pChildReport->GetWoormFrame()->GetOwner() == GetWoormFrame() &&
						GetCallerDocument()->GetFrame()
						)
					m_pChildReport->GetWoormFrame()->SetOwner(GetCallerDocument()->GetFrame());
			}

			// se questo e' l'ultimo report della catena e quindi cancella le info			
			// allocate esternamente e passate a tutti i documenti della catena
			int nNext = m_pWoormInfo->m_nNextReport;
			if (!m_pChildReport && !m_pWoormInfo->m_bOwnedByReport)
			{
				// permette di riutilizzare la catena dei report se sono 
				// stato chiamata in un loop e non sono proprietario del WoormInfo
				m_pWoormInfo->m_nNextReport = 0;
			}

			//Devo salvare gli eventuali parametri passati dal documento
			CWoormInfo* pWI = NULL;
			if (!bClose && !m_pWoormInfo->m_bOwnedByReport)
			{
				pWI = new CWoormInfo(*m_pWoormInfo);
				pWI->m_bOwnedByReport = TRUE;

				// devo inibire l'eventuale riutilizzo se il report non e' stato chiuso
				// in automatico (vedi sotto) senza perdere gli eventuali parametri iniziali
				if (m_pOldWoormInfo == NULL)
					m_pOldWoormInfo = m_pWoormInfo;

				pWI->m_nNextReport = nNext;
				m_pWoormInfo = pWI;
			}
		}

		// chiude il report corrente come ultima operazione da fare		
		if  (bClose)
		{
			m_pChildReport = NULL;
			CloseDocument();
		}
	}
}

//------------------------------------------------------------------------------
BOOL CWoormDocMng::IsReportRunning() 
{
	BOOL b = __super::IsReportRunning();
	if (!b) 
	{
		if (m_pChildReport)
		{
			b = m_pChildReport->IsReportRunning();
			//b = TRUE;
		}
	}
	return b;
}

//------------------------------------------------------------------------------
void CWoormDocMng::OnReportEnd	()	
{ 
	SetRunIndicator ();
	m_bRunIndicatorActive = FALSE;	

	if (m_nTimerID)
	{
		GetFirstView()->KillTimer(m_nTimerID);
		m_nTimerID = 0;
	}
}

//------------------------------------------------------------------------------
void CWoormDocMng::OnBeginSort		()	{ pchSortStatusString[0] = _T('S');	}
void CWoormDocMng::OnEndSort		()	{ pchSortStatusString[0] = BLANK_CHAR;	}
void CWoormDocMng::OnBeginGroup	()	{ pchSortStatusString[1] = _T('G');	}
void CWoormDocMng::OnEndGroup		()	{ pchSortStatusString[1] = BLANK_CHAR;	}
void CWoormDocMng::OnBeginMemGroup	()	{ pchSortStatusString[2] = _T('M');	}
void CWoormDocMng::OnEndMemGroup	()	{ pchSortStatusString[2] = BLANK_CHAR;	}


//------------------------------------------------------------------------------
void CWoormDocMng::VKMoveObject(CSize offset)	
{              
	// skip VK command 
	if	(!m_bAllowEditing || m_Creating != NONE || m_bEngineRunning)
		return;

	SetModifiedFlag();
	if (m_pMultipleSelObj)
	{
		m_pMultipleSelObj->MoveMultipleSelObjects(offset);
		UpdateWindow();
		return;
	}
	
	if (m_pCurrentObj) 
	{
		m_pCurrentObj->MoveObject(offset);
		m_pActiveRect->SetActive(m_pCurrentObj->GetActiveRect());
		m_pCurrentObj->UpdateRepeater();
		UpdateWindow();
	}
}


#define DIRECTION_LEFT	1
#define DIRECTION_RIGHT	2
#define DIRECTION_UP	3
#define DIRECTION_DOWN	4

//------------------------------------------------------------------------------
void CWoormDocMng::VKSizeObject(int increment, int direction)	
{              
	// skip VK command 
	if	(!m_bAllowEditing || m_Creating != NONE || m_bEngineRunning)
		return;

	SetModifiedFlag();
	if (m_pCurrentObj) 
	{
		int h = m_pCurrentObj->GetActiveRect().Height() + increment;
		int w = m_pCurrentObj->GetActiveRect().Width() + increment;

		switch (direction)
		{
			case DIRECTION_LEFT:	m_pCurrentObj->LeftSizeObject(w);	break;
			case DIRECTION_RIGHT:	m_pCurrentObj->RightSizeObject(w);	break;
			case DIRECTION_UP:		m_pCurrentObj->TopSizeObject(h);	break;
			case DIRECTION_DOWN:	m_pCurrentObj->BottomSizeObject(h);	break;
		}

		m_pActiveRect->SetActive(m_pCurrentObj->GetActiveRect());
		UpdateWindow();
	}
}

// abilitato dallo opportuno flag in woormini
//------------------------------------------------------------------------------
#define STEP_X (m_pWoormIni->m_bSnapToGrid ? m_pWoormIni->m_nGridX : 1)
#define STEP_Y (m_pWoormIni->m_bSnapToGrid ? m_pWoormIni->m_nGridY : 1)

//------------------------------------------------------------------------------
void CWoormDocMng::OnVKLeft			()	{ VKMoveObject(CSize(-STEP_X, 0));			RemoveFocusFromBtn();}
void CWoormDocMng::OnVKRight		()	{ VKMoveObject(CSize(+STEP_X, 0));			RemoveFocusFromBtn();}
void CWoormDocMng::OnVKUp			()	{ VKMoveObject(CSize(0, -STEP_Y));			RemoveFocusFromBtn();}
void CWoormDocMng::OnVKDown			()	{ VKMoveObject(CSize(0, +STEP_Y));			RemoveFocusFromBtn();}

//------------------------------------------------------------------------------
void CWoormDocMng::OnVKShiftLeft	()	{ VKSizeObject(-STEP_X, DIRECTION_RIGHT);	RemoveFocusFromBtn();}
void CWoormDocMng::OnVKShiftRight	()	{ VKSizeObject(+STEP_X, DIRECTION_RIGHT);	RemoveFocusFromBtn();}
void CWoormDocMng::OnVKShiftUp		()	{ VKSizeObject(+STEP_Y, DIRECTION_UP);		RemoveFocusFromBtn();}
void CWoormDocMng::OnVKShiftDown	()	{ VKSizeObject(-STEP_Y, DIRECTION_UP);		RemoveFocusFromBtn();}

//------------------------------------------------------------------------------
void CWoormDocMng::OnVKCtrlLeft		()	{ VKSizeObject(+STEP_X, DIRECTION_LEFT);	RemoveFocusFromBtn();}
void CWoormDocMng::OnVKCtrlRight	()	{ VKSizeObject(-STEP_X, DIRECTION_LEFT);	RemoveFocusFromBtn();}
void CWoormDocMng::OnVKCtrlUp		()	{ VKSizeObject(-STEP_Y, DIRECTION_DOWN);	RemoveFocusFromBtn();}
void CWoormDocMng::OnVKCtrlDown		()	{ VKSizeObject(+STEP_Y, DIRECTION_DOWN);	RemoveFocusFromBtn();}

//------------------------------------------------------------------------------
//metodo da chiamare per rimuovere il focus dai pulsanti della toolbox delle actions
//da cui non si toglio il fuoco dopo averci cliccato
void CWoormDocMng::RemoveFocusFromBtn()
{
	HWND hWnd = ::GetFocus();
	if (hWnd == NULL)
	{
		ASSERT(FALSE);
		return;
	}
	CWnd* cWnd = CWnd::FromHandle(hWnd);
	if (cWnd == NULL)
	{
		ASSERT(FALSE);
		return;
	}
	if(cWnd->IsKindOf(RUNTIME_CLASS(CBCGPButton)))
		SetFocus(GetWoormFrame()->m_pToolBarView->m_hWnd);
}

//------------------------------------------------------------------------------
void CWoormDocMng::OnToggleTransparent()	
{ 
	if	(!CanDoToggleTransparent())
		return;

	if (m_pMultipleSelObj)	
		m_pMultipleSelObj->ToggleTransparent();

	else if (m_pCurrentObj) 
		m_pCurrentObj->ToggleTransparent();

	UpdateWindow();
	RemoveFocusFromBtn();
}
//------------------------------------------------------------------------------
void CWoormDocMng::DoSnapToGrid ()	
{ 
	if	(!CanDoSnapObjects())
		return;

	if (m_pMultipleSelObj)	
		m_pMultipleSelObj->SnapToGrid();
	
	else if (m_pCurrentObj) 
	{
		m_pCurrentObj->SnapToGrid();
		m_pActiveRect->SetActive(m_pCurrentObj->GetActiveRect());
	}

	UpdateWindow();
	RemoveFocusFromBtn();
}

//------------------------------------------------------------------------------
void CWoormDocMng::OnSnapToGrid ()	{ DoSnapToGrid(); }

//------------------------------------------------------------------------------
void CWoormDocMng::OnVKReturn ()	
{ 
	if (!m_pMultipleSelObj && m_pCurrentObj) 
		m_pCurrentObj->VKReturn();
}

//------------------------------------------------------------------------------
void CWoormDocMng::OnVKTab()
{
	// don't tab object if multiple selection is active or there are no GetObjects()
	if (m_pMultipleSelObj || !m_pCurrentObj || GetObjects().GetSize() <= 1) return;
	
	int ub = GetObjects().GetUpperBound();
	BaseObj* pObj = GetObjects()[0];
	
	for (int j = 1; j <= ub; j++)
		GetObjects()[j - 1] = GetObjects()[j];
		
	GetObjects()[ub] = pObj;
	
	// m_pCurrentObj is always last object
	m_pCurrentObj = GetObjects()[ub];
	m_pActiveRect->SetActive(m_pCurrentObj->GetActiveRect()); 
	UpdateWindow();
}

//------------------------------------------------------------------------------
void CWoormDocMng::OnVKShiftTab()
{
	// don't tab object if multiple selection is active or there are no GetObjects()
	if (m_pMultipleSelObj || !m_pCurrentObj || GetObjects().GetSize() <= 1) return;
	
	int ub = GetObjects().GetUpperBound();
	BaseObj* pObj = GetObjects()[ub];
	
	for (int j = ub - 1; j >= 0; j--)
		GetObjects()[j + 1] = GetObjects()[j];
		
	GetObjects()[0] = pObj;
	m_pCurrentObj = GetObjects()[ub];
	m_pActiveRect->SetActive(m_pCurrentObj->GetActiveRect());
	UpdateWindow();
}

/////////////////////////////////////////////////////////////////////////////
// CWoormDocMng commands
//------------------------------------------------------------------------------
void CWoormDocMng::ToggleSaveOnlyGraphInfo ()
{
	// gestione della versione demo che impedisce il salvataggio
	m_bOnlyGraphInfo = !m_bOnlyGraphInfo;
}

//void CWoormDocMng::OnAddDataField		()	{ CreateField	(ID_ADD_DATA_FIELD);	}
//void CWoormDocMng::OnAddExpField		()	{ CreateField	(ID_ADD_EXP_FIELD);		}
//void CWoormDocMng::OnAddHiddenField		()	{ CreateField	(ID_ADD_HIDDEN_FIELD);	}
//void CWoormDocMng::OnAddVarField		()	{ CreateField	(ID_ADD_VAR_FIELD);		}

void CWoormDocMng::OnPause			()	{ m_pEngine->Pause(); GetWoormFrame()->ShowResumeButton();}
void CWoormDocMng::OnResume		()	{ m_pEngine->Resume(); GetWoormFrame()->ShowPauseButton();}

void CWoormDocMng::OnColLeftMove	() { MoveColumn(TRUE);		RemoveFocusFromBtn(); }
void CWoormDocMng::OnColRightMove	() { MoveColumn (FALSE);	RemoveFocusFromBtn(); }



void CWoormDocMng::OnShowReportParameters ()
{/*
	WoormInfoViewer WoormInfoDlg(m_pWoormInfo, (this->m_pEngine && m_pEngine->m_pSymTable) ? m_pEngine->m_pSymTable : NULL);
	WoormInfoDlg.DoModal();*/

	GetWoormFrame()->CreateAndShowPropertyPanel();

	//carico la property grid per i general settings passandogli un cnode fittizio
	CNodeTree* pNode = new CNodeTree(NULL, CRSTreeCtrlImgIdx::NoGLyph, CNodeTree::ENodeType::NT_PARAMETERS, m_pWoormInfo, NULL);

	if (GetWoormFrame() && GetWoormFrame()->m_pObjectPropertyView)
		GetWoormFrame()->m_pObjectPropertyView->LoadPropertyGrid(pNode);
}

//------------------------------------------------------------------------------
BOOL CWoormDocMng::CanDoToggleTransparent()
{ 
	return CanDoMoveObjects();
}

//------------------------------------------------------------------------------
BOOL CWoormDocMng::CanDoCutObjects()
{
	return CanDoMoveObjects();
}

//------------------------------------------------------------------------------
BOOL CWoormDocMng::CanDoMoveObjects()
{ 
	return
		m_bAllowEditing  &&
		!m_bPlayback  && m_Creating == NONE && 
		GetObjects().GetSize() && !m_bEngineRunning &&
		(m_pCurrentObj || (m_pMultiColumns && m_pMultiColumns->GetSize()>0) || (m_pMultipleSelObj && !m_pMultipleSelObj->IsEmpty()));
}

//------------------------------------------------------------------------------
BOOL CWoormDocMng::CanDoSizeObjects()
{ 
	return
		m_bAllowEditing  &&
		!m_bPlayback  && m_Creating == NONE && 
		GetObjects().GetSize() && !m_bEngineRunning &&
		!m_pMultipleSelObj&&
		m_pCurrentObj && 
		m_pCurrentObj->IsKindOf(RUNTIME_CLASS(SingleItemObj));
}

//------------------------------------------------------------------------------
void CWoormDocMng::OnUpdatePageSetup(CCmdUI* pCmdUI)
{	
	pCmdUI->Enable(m_Creating == NONE && !m_bEngineRunning);
}

//------------------------------------------------------------------------------
void CWoormDocMng::OnUpdateFileProperties(CCmdUI* pCmdUI)
{
	pCmdUI->Enable(!m_bEngineRunning);
}

//------------------------------------------------------------------------------
void CWoormDocMng::OnUpdateRecordValidation(CCmdUI* pCmdUI)
{
	pCmdUI->Enable(!m_bEngineRunning);
}

//------------------------------------------------------------------------------
void CWoormDocMng::OnUpdateVKSize(CCmdUI* pCmdUI)			{ pCmdUI->Enable(CanDoSizeObjects()); }
void CWoormDocMng::OnUpdateVKMove(CCmdUI* pCmdUI)			{ pCmdUI->Enable(CanDoMoveObjects()); }
void CWoormDocMng::OnUpdateToggleBorder(CCmdUI* pCmdUI)	{ pCmdUI->Enable(CanDoMoveObjects()); }
void CWoormDocMng::OnUpdateCut (CCmdUI* pCmdUI)			{ pCmdUI->Enable(CanDoCutObjects()); }
void CWoormDocMng::OnUpdateSnapToGrid (CCmdUI* pCmdUI)		{ pCmdUI->Enable(CanDoSnapObjects()); }
void CWoormDocMng::OnUpdateToggleTransparent (CCmdUI* pCmdUI)
{
	pCmdUI->Enable(CanDoToggleTransparent());
}
void CWoormDocMng::OnUpdateSendMail (CCmdUI* pCmdUI)		{ pCmdUI->Enable(CanDoSendMail()); }
void CWoormDocMng::OnUpdateFileSavePDF (CCmdUI* pCmdUI)	{ pCmdUI->Enable(CanDoFileSavePDF()); }

//------------------------------------------------------------------------------
BOOL CWoormDocMng::CanDoSnapObjects ()		
{ 
	return /*m_pWoormIni->m_bSnapToGrid &&*/ CanDoMoveObjects();
}

//------------------------------------------------------------------------------
void CWoormDocMng::OnUpdateClearAll (CCmdUI* pCmdUI)
{	
	BOOL bOk = 
		m_bAllowEditing &&
		!m_bPlayback && m_Creating == NONE && 
		GetObjects().GetSize() && !m_bEngineRunning;
		
	pCmdUI->Enable(bOk);
}
	
//------------------------------------------------------------------------------
void CWoormDocMng::OnUpdateCopy (CCmdUI* pCmdUI)
{	
	BOOL bOk = 
			m_bAllowEditing &&
			!m_bPlayback  && m_Creating == NONE && !m_bEngineRunning &&
			m_pCurrentObj && !m_pMultipleSelObj;
			
	pCmdUI->Enable(bOk);
}
	
//------------------------------------------------------------------------------
void CWoormDocMng::OnUpdatePaste (CCmdUI* pCmdUI)
{	                       
	BOOL bOk =              
			m_bAllowEditing &&
			!m_bPlayback  && m_Creating == NONE && !m_bEngineRunning &&
			(
				IsClipboardFormatAvailable(CF_DIB) || 
				IsClipboardFormatAvailable(CF_UNICODETEXT) ||
				IsClipboardFormatAvailable(CF_TEXT) ||
				IsClipboardFormatAvailable(CF_OEMTEXT)
			);
			
	pCmdUI->Enable(bOk);
}
	
//------------------------------------------------------------------------------
void CWoormDocMng::OnUpdateUndo (CCmdUI* pCmdUI)
{	
	BOOL bOk = 
			m_bAllowEditing &&
			!m_bPlayback  && m_Creating == NONE && !m_bEngineRunning &&
			(m_pUndoStack->GetSize() > 0);
			
	pCmdUI->Enable(bOk);
}
	
//------------------------------------------------------------------------------
void CWoormDocMng::OnUpdateRun (CCmdUI* pCmdUI)
{	      
	pCmdUI->Enable(
					m_Creating == NONE && 
					!m_bEngineRunning  &&
					(m_pWoormInfo ? ! m_pWoormInfo->m_bBlockReRun : TRUE) &&
					OSL_CAN_DO(GetInfoOSL(), OSL_GRANT_EXECUTE)					
				   );
}

//------------------------------------------------------------------------------
void CWoormDocMng::OnUpdateStop (CCmdUI* pCmdUI)
{	
	pCmdUI->Enable(!m_bPlayback && m_Creating == NONE && m_bEngineRunning);
}
	
//------------------------------------------------------------------------------
void CWoormDocMng::OnUpdateRunStop (CCmdUI* pCmdUI)
{	      
	pCmdUI->Enable	(
						!m_bPlayback &&
						m_Creating == NONE  && 
						(m_pWoormInfo ? !m_pWoormInfo->m_bBlockReRun : TRUE) &&
						OSL_CAN_DO(GetInfoOSL(), OSL_GRANT_EXECUTE)
					);
}

//------------------------------------------------------------------------------
void CWoormDocMng::OnUpdatePause (CCmdUI* pCmdUI)
{	
	BOOL bOk = 	!m_bPlayback && 
				m_Creating == NONE && 
				m_bEngineRunning && !m_pEngine->IsPaused();

	pCmdUI->Enable(bOk);
}

//------------------------------------------------------------------------------
void CWoormDocMng::OnUpdateResume (CCmdUI* pCmdUI)
{	
	BOOL bOk = 	m_Creating == NONE && !m_bPlayback && 
				m_bEngineRunning && m_pEngine->IsPaused();

	pCmdUI->Enable(bOk);
}
	
//------------------------------------------------------------------------------
void CWoormDocMng::OnUpdatePauseResume (CCmdUI* pCmdUI)
{	      
	pCmdUI->Enable(!m_bPlayback && m_Creating == NONE && m_bEngineRunning);
}

//------------------------------------------------------------------------------
void CWoormDocMng::OnUpdateGotoPage (CCmdUI* pCmdUI)
{	
	BOOL bOk = IsFilledWithData()
				// in this case all work fine because this function is always
				// called by menu manager (see below for buttons)
				 && m_pRDEmanager->LastPage() > 0;

	pCmdUI->Enable(bOk);
}

//------------------------------------------------------------------------------
void CWoormDocMng::OnUpdateMovePage (CCmdUI* pCmdUI)
{	
	BOOL bOk = IsFilledWithData();
				// can't use because OnIdle is called only once by scheduler
				// && m_pRDEmanager->lastPage() > 0;

	pCmdUI->Enable(bOk);
}

//------------------------------------------------------------------------------
void CWoormDocMng::OnUpdateFilePreviewPrint(CCmdUI* pCmdUI)
{
	BOOL bEnable = !m_bEngineRunning;
	pCmdUI->Enable(bEnable);
}

//------------------------------------------------------------------------------
void CWoormDocMng::OnUpdateFindWord (CCmdUI* pCmdUI)
{
	pCmdUI->Enable(CanDoExportData (pCmdUI->m_nID));
}

//------------------------------------------------------------------------------
BOOL CWoormDocMng::IsDataLoaded()
{
	return !m_pRDEmanager->IsClosed() && m_pEditorManager->IsCurrRunValid();
}

//------------------------------------------------------------------------------
BOOL CWoormDocMng::IsFilledWithData() 
{ 
	return m_Creating == NONE && GetObjects().GetSize() && IsDataLoaded();
}

// Se sono limitato allora posso salvare perche' le opzioni abilitate alla modifica 
// sono le solo che abilitano lo stato di "modified" e necessitano il salvataggio 
// del report (tipo "Opzioni di report")
//------------------------------------------------------------------------------
BOOL CWoormDocMng::CanSaveReport()
{
	return 
		m_bAllowEditing &&
		!m_bPlayback  && 
		m_Creating == NONE &&
		!m_bEngineRunning;
}

//------------------------------------------------------------------------------
BOOL CWoormDocMng::CanSaveReportData()
{
	BOOL bOk = !m_bPlayback && 
		(!m_bEngineRunning || (m_bEngineRunning && m_bIsExporting))
		&& 
		IsFilledWithData();

	if ( bOk && OSL_IS_PROTECTED(GetInfoOSL()) )
	{
		bOk = OSL_CAN_DO(GetInfoOSL(), OSL_GRANT_EXPORT); 
	}
	return bOk;
}
		
//------------------------------------------------------------------------------
BOOL CWoormDocMng::CanDoSendMail ()
{	
	BOOL bOk = AfxIsActivated(TBEXT_APP,  MAILCONNECTOR_ACT);

	return  bOk && CanSaveReportData();
}

//------------------------------------------------------------------------------
BOOL CWoormDocMng::CanDoFileSavePDF ()
{	
	return CanDoSendMail();
}

//------------------------------------------------------------------------------
void CWoormDocMng::OnUpdateFileNew(CCmdUI* pCmdUI)
{ 
	pCmdUI->Enable(m_pSqlConnection->IsValid() && m_bIsUserReportsDeveloper);
}

//------------------------------------------------------------------------------
void CWoormDocMng::OnUpdateFileOpen(CCmdUI* pCmdUI)
{
	CTBNamespace ns1(CTBNamespace::FUNCTION, _NS_WEB("Framework.TbWoormViewer.TbWoormViewer.ExecOpenReport"));
	CInfoOSL infoOSL1(ns1, OSLType_Function);
	AfxGetSecurityInterface()->GetObjectGrant (&infoOSL1);
	BOOL bEnable = OSL_CAN_DO(&infoOSL1, OSL_GRANT_EXECUTE);

	pCmdUI->Enable(m_pSqlConnection->IsValid() && bEnable);
}

//------------------------------------------------------------------------------
BOOL CWoormDocMng::CanDoExportData (UINT)
{
	BOOL bOk =	
		IsDataLoaded()		&&
		! m_bEngineRunning;
			
	if ( bOk && OSL_IS_PROTECTED(GetInfoOSL()) )
	{
		bOk = OSL_CAN_DO(GetInfoOSL(), OSL_GRANT_EXPORT); 
	}

	return bOk;
}

//------------------------------------------------------------------------------
void CWoormDocMng::OnUpdateExportData (CCmdUI* pCmdUI)
{
	pCmdUI->Enable (CanDoExportData (pCmdUI->m_nID));
}

//------------------------------------------------------------------------------
void CWoormDocMng::OnUpdateExportDropDown (CCmdUI* pCmdUI)
{
	pCmdUI->Enable (TRUE);
}

//------------------------------------------------------------------------------
void CWoormDocMng::OnUpdateFileSave(CCmdUI* pCmdUI)
{
	pCmdUI->Enable(CanSaveReport());
}

//------------------------------------------------------------------------------
void CWoormDocMng::OnUpdateFileSaveRDE (CCmdUI* pCmdUI)
{	
	pCmdUI->Enable(CanSaveReportData());
}

//------------------------------------------------------------------------------
void CWoormDocMng::OnUpdateAddRect (CCmdUI* pCmdUI)
{	
	BOOL bOk = 	
		m_bAllowEditing &&
		!m_bPlayback && 
		m_Creating == NONE && 
		!CurrentIsTable() &&
		!m_bEngineRunning;
				
	pCmdUI->Enable(bOk);
}

//------------------------------------------------------------------------------
void CWoormDocMng::OnUpdateAddTableOrFields (CCmdUI* pCmdUI)
{	
	BOOL bOk =	
		m_bAllowEditing &&
		!m_bPlayback && 
		m_Creating == NONE && 
		!CurrentIsTable() &&
		!m_bEngineRunning &&
		!m_pMultipleSelObj;

	pCmdUI->Enable(bOk);
}

//------------------------------------------------------------------------------
void CWoormDocMng::OnUpdateAddColumns(CCmdUI* pCmdUI)
{
	BOOL bOk =
		m_bAllowEditing &&
		!m_bPlayback &&
		m_Creating == NONE &&
		CurrentIsTable() &&
		!m_bEngineRunning &&
		!m_pMultipleSelObj;

	pCmdUI->Enable(bOk);
}

//------------------------------------------------------------------------------
void CWoormDocMng::OnUpdateAddLink(CCmdUI* pCmdUI)
{
	BOOL bOk =
		m_bAllowEditing &&
		!m_bPlayback &&
		m_Creating == NONE &&
		(
			(CurrentIsTable() && !NoActiveColumn())
			||
			(m_pCurrentObj && dynamic_cast<FieldRect*>(m_pCurrentObj))
		) &&
		!m_bEngineRunning;

	pCmdUI->Enable(bOk);
}

//------------------------------------------------------------------------------
void CWoormDocMng::OnUpdateColMove (CCmdUI* pCmdUI)
{	
	BOOL bOk =	
		m_bAllowEditing &&
		!m_bPlayback &&
		m_Creating == NONE &&
		GetObjects().GetSize() && 
		!m_bEngineRunning &&
		CurrentIsTable() && 
		!NoActiveColumn() &&
		MoreThanOneColumn();

	pCmdUI->Enable(bOk);
}

//------------------------------------------------------------------------------
void CWoormDocMng::OnUpdateSelectAll (CCmdUI* pCmdUI)
{	
	BOOL bOk =	
		m_bAllowEditing &&
		!m_bPlayback && m_Creating == NONE && GetObjects().GetSize() && 
		!m_bEngineRunning && !m_pMultipleSelObj;

	pCmdUI->Enable(bOk);
}

//------------------------------------------------------------------------------
void CWoormDocMng::OnUpdateSelection (CCmdUI* pCmdUI)
{	
	BOOL bOk = 	
		m_bAllowEditing &&
		!m_bPlayback && m_Creating == NONE && GetObjects().GetSize() && 
		!m_bEngineRunning && m_pMultipleSelObj;
	
	pCmdUI->Enable(bOk);
}

//------------------------------------------------------------------------------
void CWoormDocMng::OnUpdateOptions(CCmdUI* pCmdUI)
{	
	// le opzioni di report possono essere salvate solo quando
	// il report ha un namespace, perchè dipendono dal namespace
	// del documento
	BOOL bOk = 	
		!m_bPlayback && m_Creating == NONE && !m_bEngineRunning &&
		GetNamespace().GetType() == CTBNamespace::REPORT;

	pCmdUI->Enable(bOk);
}

//------------------------------------------------------------------------------
void CWoormDocMng::OnUpdateWoormOptions (CCmdUI* pCmdUI)
{	
	// disabilito le opzioni anche in nuovo documento in caso di non Admin
	BOOL bOk = 	m_bAllowEditing &&
				!m_bPlayback && m_Creating == NONE && !m_bEngineRunning && 
				m_bIsUserReportsDeveloper;

	pCmdUI->Enable(bOk);
}

//------------------------------------------------------------------------------
void CWoormDocMng::OnUpdateEditMode (CCmdUI* pCmdUI)
{	
	BOOL bOk = 	
		m_bAllowEditing &&
		!m_bPlayback && m_Creating == NONE && !m_bEngineRunning;
	
	pCmdUI->Enable(bOk);
}

//------------------------------------------------------------------------------
void CWoormDocMng::OnUpdateShowReportParameters (CCmdUI* pCmdUI)
{	
	BOOL bOk =	
		m_pWoormInfo != NULL &&
		m_pWoormInfo->GetParameters().GetSize() != 0;
			
	pCmdUI->Enable(bOk);
}

//------------------------------------------------------------------------------
void CWoormDocMng::OnUpdateShowReportObjects (CCmdUI* pCmdUI)
{	
	BOOL bOk = GetObjects().GetCount() > 2;
			
	pCmdUI->Enable(bOk);
}

//------------------------------------------------------------------------------
void CWoormDocMng::OnUpdateMakerMode (CCmdUI* pCmdUI)
{	
	BOOL bOk =	
		m_bAllowEditing &&
		!m_bPlayback && m_Creating == NONE && !m_bEngineRunning;
				
	pCmdUI->Enable(bOk);
}

//------------------------------------------------------------------------------
void CWoormDocMng::OnUpdateSaveOnlyGraphInfo (CCmdUI* pCmdUI)
{
	BOOL bOk = FALSE;
	
	bOk = 
		m_bAllowEditing &&
		!m_bPlayback && m_Creating == NONE;
											
	pCmdUI->Enable(bOk);
	pCmdUI->SetCheck(m_bOnlyGraphInfo);
}

//------------------------------------------------------------------------------
BOOL CWoormDocMng::CanDoEditReport()
{
	BOOL bAllow = this ? m_bIsUserReportsDeveloper: IsUserReportsDeveloper();

	if (!bAllow && AfxGetSecurityInterface()->IsSecurityEnabled())
	{
		//current report has specific grants ?
		if (OSL_IS_PROTECTED(GetInfoOSL()))
			bAllow =  OSL_CAN_DO(GetInfoOSL(), OSL_GRANT_EDIT);

		if (bAllow && ! AfxIsCalAvailable(TBNET_APP, REPORTEDITOR_ACT))
			return FALSE;
	}

	return bAllow;
}

BOOL CWoormDocMng::CanDoAllowEditingButton(BOOL bPrepare/* = FALSE*/)
{
	BOOL bAllow = bPrepare ?
					TRUE :
					(
						!m_bPlayback && 
						m_Creating == NONE && 
			!m_bEngineRunning &&
			! IsExternalControlled()
					);

	if (!bAllow)
		return FALSE;
	
	return CanDoEditReport();
}

//-----------------------------------------------------------------------------
void CWoormDocMng::OnTabSwitchRange(UINT nID)
{
	int nPos = nID - ID_WOORM_TAB_SWITCH_MENU_START;
	CTBTabbedToolbar* pTabbed = GetWoormFrame()->GetTabbedToolBar();
	if (pTabbed)
	{
		pTabbed->SetActiveTabByPos(nPos);
	}
}

//-----------------------------------------------------------------------------
void CWoormDocMng::OnTabSwitch()
{
	CTBTabbedToolbar* pTabbed = GetWoormFrame()->GetTabbedToolBar();
	if (pTabbed)
	{
		pTabbed->MoveNextTab();
	}	
}

//------------------------------------------------------------------------------
void CWoormDocMng::OnUpdateTabSwitch(CCmdUI* pCmdUI)
{
	pCmdUI->Enable(TRUE);
}

//------------------------------------------------------------------------------
void CWoormDocMng::OnUpdateAllowEditingButton (CCmdUI* pCmdUI)
{
	pCmdUI->Enable(!m_bIsNew && CanDoAllowEditingButton());
	pCmdUI->SetCheck(m_bAllowEditing);
}

//------------------------------------------------------------------------------
void CWoormDocMng::OnUpdateShowEditToolbars(CCmdUI* pCmdUI)
{
	pCmdUI->Enable(m_bAllowEditing);
}

//------------------------------------------------------------------------------
void CWoormDocMng::UpdateSpecialField()
{                         
	for (int i = 0; i <= GetObjects().GetUpperBound(); i++)
	{
		BaseObj* pObj = GetObjects()[i];
		if (!pObj->IsKindOf(RUNTIME_CLASS(TextRect)))
			continue;

		TextRect* pTextRect = (TextRect*) pObj;
		if (!pTextRect->GetSpecialField())
			continue;

		InvalidateRect(pTextRect->GetBaseRect(), pTextRect->IsTransparent());
	}
}

//------------------------------------------------------------------------------
void CWoormDocMng::SetPageIndicator ()
{                         
	if (m_pRDEmanager->IsClosed()) return;
	
	int		curr_page = m_pRDEmanager->CurrPageRead() + 1;
	int 	last_page = m_pRDEmanager->LastPage() + 1;
	
	CWoormFrame* pFrame = GetWoormFrame();
	if (pFrame == NULL || !::IsWindow(pFrame->m_StatusBar.m_hWnd))
		return;
	CClientDC	dc(&(pFrame->m_StatusBar));
	
	const rsize_t nBuff = 40;
	static TCHAR indicator[40];

	// format page status
	_sntprintf_s(indicator, nBuff, sizeof indicator, _T("%04d:%04d"), curr_page, last_page);

	pFrame->m_StatusBar.SetPaneText(0, indicator);
}


//------------------------------------------------------------------------------
void CWoormDocMng::SetRunIndicator ()
{
	// don't display record count on playback (no record extracted from database)                      
	if (m_bPlayback || m_pRDEmanager->IsClosed()) return; 

	CWoormFrame* pFrame = GetWoormFrame();
	if (pFrame == NULL || !::IsWindow(pFrame->m_StatusBar.m_hWnd))
		return;
	CClientDC	dc(&(pFrame->m_StatusBar));
	
	// format run status
	const rsize_t nBuff = 40;
	static TCHAR indicator[40];
	_sntprintf_s(indicator, nBuff, sizeof indicator, _T("%-3s %06ld : %06ld"), pchSortStatusString, m_nSearchTic, m_nRetrieveTic);
						
	pFrame->m_StatusBar.SetPaneText(1, indicator);
	
}
//------------------------------------------------------------------------------
void CWoormDocMng::SetMessageInStatusBar(CString message, MSGSBType eMsgType)
{
	CWoormFrame* pFrame = GetWoormFrame();
	if (pFrame == NULL || !::IsWindow(pFrame->m_StatusBar.m_hWnd))
		return;
	CClientDC	dc(&(pFrame->m_StatusBar));

	switch (eMsgType)
	{
	case MSG_SB_NORMAL:
		pFrame->m_StatusBar.SetPaneTextColor(3, AfxGetThemeManager()->GetStatusbarTextColor());
		break;
	case MSG_SB_SUCCESS:
		pFrame->m_StatusBar.SetPaneTextColor(3, RGB(0, 255, 0));
		break;
	case MSG_SB_INFO:
		pFrame->m_StatusBar.SetPaneTextColor(3, RGB(0, 148, 255));
		break;
	case MSG_SB_WARNING:
		pFrame->m_StatusBar.SetPaneTextColor(3, RGB(255, 216, 0));
		break;
	case MSG_SB_ERROR:
		pFrame->m_StatusBar.SetPaneTextColor(3, RGB(255, 0, 0));
		break;
	}

	pFrame->m_StatusBar.SetPaneText(3, message);
	
	CFont* pFont = pFrame->m_StatusBar.GetFont();
	
	CDC* pDC = pFrame->m_StatusBar.GetDC();
	CSize sz = ::GetTextSize(pDC, message, pFont);
	
	pFrame->m_StatusBar.ReleaseDC(pDC);

	pFrame->m_StatusBar.SetPaneWidth(3, sz.cx);
}

//------------------------------------------------------------------------------
void CWoormDocMng::ClearMessageInStatusBar()
{
	CWoormFrame* pFrame = GetWoormFrame();
	if (pFrame == NULL || !::IsWindow(pFrame->m_StatusBar.m_hWnd))
		return;
	CClientDC	dc(&(pFrame->m_StatusBar));

	pFrame->m_StatusBar.SetPaneText(3, _T(""));
}

//------------------------------------------------------------------------------
void CWoormDocMng::OnUpdateTrackRectIndicator (CCmdUI* pCmdUI)
{
	BOOL bEnable = m_pCurrentObj || m_bTrackRectIndicatorActive;
	pCmdUI->Enable(bEnable);
	if (!bEnable) return;

	CRect rect(0,0,0,0);
	if (m_bTrackRectIndicatorActive)
	{
		static CRect prev_rect(0,0,0,0);
		if (prev_rect == m_TrackRect && prev_rect.IsRectEmpty())
			return;
				   
		prev_rect = m_TrackRect;
		rect = m_TrackRect;
	}
	else if (m_pCurrentObj && IsWindow(m_pCurrentObj->m_hWnd))
		rect = m_pCurrentObj->GetActiveRect();

	// presenza le size sulla base del grid se richiesto
	CString msg;
	if (m_pWoormIni->m_bSizeInGridUnits)
	{
		rect.top	= MulDiv(rect.top,1,m_pWoormIni->m_nGridY) + 1;
		rect.left	= MulDiv(rect.left,1,m_pWoormIni->m_nGridX) + 1;
		rect.right	= MulDiv(rect.right,1,m_pWoormIni->m_nGridX) + 1;
		rect.bottom	= MulDiv(rect.bottom,1,m_pWoormIni->m_nGridY) + 1;

		msg = cwsprintf
		(
			//_TB("Line:{0-%d} Col:{1-%d} Height:{2-%d} Width.:{3-%d} (grid)"),
			_T("Line:%d Column:%d Height:%d Width:%d (grid)"),
			rect.top, rect.left, rect.Height(), rect.Width()
		);
	}
	else
		msg = cwsprintf
		(
			//_TB("Y:{0-%2.2f} X:{1-%2.2f} Alt.:{2-%2.2f} Width:{3-%2.2f} (mm)"),
			_T("Y:%2.2f X:%2.2f Height:%2.2f Width:%2.2f (mm)"),
			LPtoMU(rect.top),      
			LPtoMU(rect.left),     
			LPtoMU(rect.Height()),
			LPtoMU(rect.Width())
		);
	pCmdUI->SetText(msg);
}

//------------------------------------------------------------------------------
void CWoormDocMng::OnUpdateAlignmentBar(CCmdUI* pCmdUI)
{
	pCmdUI->Enable(m_pMultipleSelObj && m_pMultipleSelObj->GetSize());
}

//------------------------------------------------------------------------------
void CWoormDocMng::SetAutoSaveTimer()
{
	if (m_nTimerAutoSave != 0)
		GetFirstView()->KillTimer(m_nTimerAutoSave);

	if (m_pWoormIni->m_iTimeAutoSave > 0)
	{
		// m_iTimeAutoSave is Minuto to elapse
		UINT nWoormElapseAutoSave = m_pWoormIni->m_iTimeAutoSave * (1000*60);
		
		m_nTimerAutoSave = GetFirstView()->SetTimer(IDT_WOORM_AUTOSAVE, nWoormElapseAutoSave, NULL);
	}
}

//------------------------------------------------------------------------------
void CWoormDocMng::OnAllowEditing()	
{
	if (!CanDoAllowEditingButton())
		return;
	
	BOOL bOk = TRUE;
	if (m_bAllowEditing)
	{
		if (SaveModified())
		{
			if (m_pWoormInfo)
			{
				ASSERT_VALID(m_pWoormInfo);
				m_pWoormInfo->m_nNextReport = 0;
				CWoormInfo* pWI = m_pWoormInfo;
				m_pWoormInfo = NULL;
				AfxGetTbCmdManager()->RunWoormReport(pWI, GetCallerDocument(), NULL, FALSE, FALSE);
			}
			OnCloseDocument(); // chiudo il documento corrente che oramai è obsoleto
			return;
		}
		else
		{
			// Clear selection from active object
			OnDeselectAll();
			if (m_pCurrentObj)
			{
				m_pActiveRect->Clear();
				m_pCurrentObj = NULL;
			}
			return;
		}
	}
	else
	{
		if (!m_bIsInParse)
		{
			DataStr sCurrLayout = m_dsCurrentLayoutView;

			m_bIsInParse = TRUE;
			if (!ForceEngineParse())
			{
				m_bIsInParse = TRUE;
				m_dsCurrentLayoutView = sCurrLayout;
				return;
			}
			m_bIsInParse = FALSE;
					
			m_dsCurrentLayoutView = sCurrLayout;
		}
	}

	m_bAllowEditing = !m_bAllowEditing && CanDoAllowEditingButton();
	if (m_bAllowEditing)
	{
		SetFormMode(FormMode::EDIT);
		SetAutoSaveTimer();
	}
	else
	{
		SetFormMode(FormMode::BROWSE);
		if (m_nTimerAutoSave != 0)
			GetFirstView()->KillTimer(m_nTimerAutoSave);
	}

	GetWoormFrame()->SetAllowEditButtonInfo(m_bAllowEditing);
	
	BOOL res=GetWoormFrame()->GetEditorToolBar()->HideButton(IDC_DROPDOWN_TEMPLATES, FALSE);

	// se devo mostrare La griglia allora devo rinfrescare al cambio di stato per
	// farla comparire in editing e scomparire quando torno indietro in run mode
	if (m_pWoormIni->m_bShowGrid)
	{
		Invalidate();
		UpdateWindow();
	}
}

//------------------------------------------------------------------------------
void CWoormDocMng::OnShowEditToolbars()	
{
}

// forza il parse della parte di motore dati. La parte grafica viene sempre
// parsata in fase di apertura del report
//------------------------------------------------------------------------------
BOOL CWoormDocMng::ForceEngineParse()
{
	m_bReportComputing = TRUE;

	BOOL bOk = TRUE;
	if	(!m_bOnlyGraphInfo && !m_pEditorManager->ExistProgram())
	{
		CStringArray arErrors; int nLine = -1; int nCol = -1;
		bOk = m_pEditorManager->Parse (m_strPathName, arErrors, nLine, nCol);

		if (bOk)
		{
			CString sErr;
			bOk = CheckTypeConsistence(sErr);
			if (!bOk)
				arErrors.Add(sErr);
		}

		if (!bOk)
		{
			CString sErrors;
			CStringArray_Concat(arErrors, sErrors, L"\r\n");

			OpenEditViewFullText(sErrors, nLine, nCol);

			return bOk;
		}
	}
	m_bReportComputing = FALSE;

	return bOk;
}

/////////////////////////////////////////////////////////////////////////////
// CWoormDocMng inlinable function

void CWoormDocMng::OnFirstPage		()	{ ReadPrevPage(TRUE); }
void CWoormDocMng::OnLastPage		()	{ ReadNextPage(TRUE); }
void CWoormDocMng::OnNextPage		()	{ ReadNextPage(); }
void CWoormDocMng::OnPrevPage		()	{ ReadPrevPage(); }
void CWoormDocMng::ReadLastPage	()	{ ReadNextPage (TRUE); }
void CWoormDocMng::ReadFirstPage	()	{ ReadPrevPage (TRUE); }

/////////////////////////////////////////////////////////////////////////////
// CWoormDocMng diagnostics

#ifdef _DEBUG
void CWoormDocMng::AssertValid() const
{
	__super::AssertValid();
}

void CWoormDocMng::Dump(CDumpContext& dc) const
{
	__super::Dump(dc);
}

#endif //_DEBUG

//-----------------------------------------------------------------------------
void CWoormDocMng::OnActionsCopy ()	
{
	DataBool db(TRUE);
	CString strKey;
	strKey.Format(L"%s=%s;", SpecialReportField::NAME.HIDE_ALL_ASK_DIALOGS, db.Str(0,0));

	WoormTable* pSymTable = this->GetEngineSymTable() ? this->GetEngineSymTable() : &m_ViewSymbolTable;

	for (int i = 0; i < pSymTable->GetSize(); i++)
	{
		WoormField* pF = pSymTable->GetAt(i);
		ASSERT_VALID(pF);
		if (!pF->GetData()) 
			continue;

		if (m_pCallerDocument)
		{
			if (!pF->IsInput()) 
				continue;
		}
		else if (!pF->IsAsk())
			continue; 

		if (!pF->GetData())
			continue;

		DataType t = pF->GetData()->GetDataType();
		if (t == DataType::Array || t == DataType::Void || t == DataType::Variant)	//TODO per altri tipi complessi (es Record)
			continue;

		CString v = pF->GetData()->Str(0,0);

		if (v.Find(';') > -1)
		{
			ASSERT(FALSE);
			continue;
		}

		strKey += pF->GetName() + '=' + v + ';';
	}
	strKey.TrimRight(';');
 	
    //TODO verificare 
//CString strKey = GetReportParametersKey();

	CString strLink = GetTBNavigateUrl(GetNamespace().ToString(), strKey);

	if (CopyTBLinkToClipboard(strLink))
		AfxSetStatusBarText (_TB("The report link has been succesfully copied to clipboard."));
	else
		AfxSetStatusBarText (_TB("There was an error copying the report link to clipboard."));
}

//-----------------------------------------------------------------------------
void CWoormDocMng::OnXmlActionsCopy ()	
{
	DataBool db(TRUE);
	CFunctionDescription fd;
	fd.InternalAddParam(SpecialReportField::NAME.HIDE_ALL_ASK_DIALOGS, &db, FALSE);

	WoormTable* pSymTable = this->GetEngineSymTable() ? this->GetEngineSymTable() : &m_ViewSymbolTable;

	for (int i = 0; i < pSymTable->GetSize(); i++)
	{
		WoormField* pF = pSymTable->GetAt(i);
		ASSERT_VALID(pF);
		if (!pF->GetData()) 
			continue;

		if (m_pCallerDocument)
		{
			if (!pF->IsInput()) 
				continue;
		}
		else if (!pF->IsAsk())
			continue; 

		if (!pF->GetData())
			continue;

		DataType t = pF->GetData()->GetDataType();
		if (t == DataType::Array|| t == DataType::Variant || t == DataType::Void || t == DataType::Null)	//TODO per altri tipi complessi (es Record)
			continue;

		fd.InternalAddParam(pF->GetName(), pF->GetData(), FALSE);
	}

	DataStr dsArguments;
	if (!fd.UnparseArguments(dsArguments, XML_ARGUMENTS_TAG))
	{
		AfxSetStatusBarText (_TB("There was an error copying the report link to clipboard."));
		return;
	}

	if (CopyTBLinkToClipboard(dsArguments))
		AfxSetStatusBarText (_TB("The report link has been succesfully copied to clipboard."));
	else
		AfxSetStatusBarText (_TB("There was an error copying the report link to clipboard."));
}

//-----------------------------------------------------------------------------
BOOL CWoormDocMng::PrepareParamsForReRun()
{
	ASSERT_VALID(m_pWoormInfo);
	if (!m_pWoormInfo)
		return FALSE;

	WoormTable* pSymTable = this->GetEngineSymTable() ? this->GetEngineSymTable() : &m_ViewSymbolTable;
	TRACE("Parametri pe rerun\n");
	for (int i = 0; i < pSymTable->GetSize(); i++)
	{
		WoormField* pF = pSymTable->GetAt(i);
		ASSERT_VALID(pF);

		if (!pF->IsAsk())
			continue;

		if (!pF->GetData())
			continue;

		CDataObjDescription* pDescription = m_pWoormInfo->GetParamDescription(pF->GetName());
		if (!pDescription)
		{
			pDescription = new CDataObjDescription(pF->GetName(), pF->GetDataType(), CDataObjDescription::PassMode::_IN);
			m_pWoormInfo->AddParam(pDescription);
		}

		if (pF->IsUpperLimit())
		{
			pF->GetData()->SetUpperValue(pF->GetLen());
		}

		pDescription->GetValue()->Assign(*pF->GetData());
		TRACE(pF->GetName() + '=' + pF->GetData()->FormatData() + '\n');
	}

	return TRUE;
}


//-----------------------------------------------------------------------------
void CWoormDocMng::OnUpdateActionsCopy (CCmdUI* pCmdUI)	
{
	pCmdUI->Enable(!m_bAllowEditing && !m_bEngineRunning);
}

//------------------------------------------------------------------------------
BOOL CWoormDocMng::OnToggleBorder(UINT nID)
{
	// Sleep for draw of button toolbar in click color.! 
	Sleep(150);

	if	(!m_bAllowEditing || m_Creating != NONE || m_bEngineRunning)
		return TRUE;

	if (m_pMultipleSelObj)
		m_pMultipleSelObj->ToggleBorder(nID);

	else if (m_pCurrentObj)
		m_pCurrentObj->ToggleBorder(nID);

	UpdateWindow();
	RemoveFocusFromBtn();
	return TRUE;
}

//------------------------------------------------------------------------------
BOOL CWoormDocMng::OnAlignmentBar(UINT nID)
{
	if (!m_pMultipleSelObj) return TRUE;

	if (nID == ID_ALIGN_HLEFT)
		m_pMultipleSelObj->AlignHLeft();
	else if (nID == ID_ALIGN_HRIGHT)
		m_pMultipleSelObj->AlignHRight();
	else if (nID == ID_ALIGN_HSPACE_EQUAL)
		m_pMultipleSelObj->AlignHSpaceEqual();
	else if (nID == ID_ALIGN_VTOP)
		m_pMultipleSelObj->AlignVTop();
	else if (nID == ID_ALIGN_VBOTTOM)
		m_pMultipleSelObj->AlignVBottom();
	else if (nID == ID_ALIGN_VSPACE_EQUAL)
		m_pMultipleSelObj->AlignVSpaceEqual();
	else if (nID == ID_ALIGN_STACK_LEFT)
		m_pMultipleSelObj->StackLeft();
	else if (nID == ID_ALIGN_STACK_RIGHT)
		m_pMultipleSelObj->StackRight();
	else if (nID == ID_ALIGN_STACK_TOP)
		m_pMultipleSelObj->StackTop();
	else if (nID == ID_ALIGN_STACK_BOTTOM)
		m_pMultipleSelObj->StackBottom();
	else if (nID == ID_ALIGN_CUT_H_LEFT)
		m_pMultipleSelObj->CutHLeft();
	else if (nID == ID_ALIGN_CUT_H_RIGHT)
		m_pMultipleSelObj->CutHRight();
	else if (nID == ID_ALIGN_CUT_V_TOP)
		m_pMultipleSelObj->CutVTop();
	else if (nID == ID_ALIGN_CUT_V_BOTTOM)
		m_pMultipleSelObj->CutVBottom();
	else if (nID == ID_LAST_LARGE)
		m_pMultipleSelObj->SizeLargeAsLast();
	else if (nID == ID_LAST_HIGH)
		m_pMultipleSelObj->SizeHighAsLast();
	// reconstruct baseRect for multiple selected object and active it                                    
	m_pMultipleSelObj->BuildBaseRect();
	RemoveFocusFromBtn();
	UpdateWindow();
	return TRUE;
}

//-----------------------------------------------------------------------------------
RDEData* CWoormDocMng::GetRDEDataFromAlias (int nA, int nRow/*=-1*/) 
{
	for (int nObj = 0; nObj <= GetObjects().GetUpperBound(); nObj++)
	{
		BaseObj* pObj = GetObjects()[nObj];

		if (pObj->GetInternalID() == nA && pObj->IsKindOf(RUNTIME_CLASS(FieldRect)))
		{
			if (pObj->m_AnchorRepeaterID)
				continue;

			FieldRect* pField = STATIC_DOWNCAST(FieldRect, pObj);
			if (!pField->GetRDEValue() || !pField->GetRDEValue()->IsValid() || !pField->GetRDEValue()->GetData())
			{
				return NULL;
			}
			return pField->GetRDEValue();
		}
		else if (pObj->IsKindOf(RUNTIME_CLASS(Table)))
		{
			Table* pTable = STATIC_DOWNCAST(Table, pObj);
			if (!pTable->ExistChildID(nA)) 
				continue;

			TableCell* pCell = pTable->GetCellFromID(nRow, nA);
			if (pCell == NULL || !pCell->GetRDEValue()->IsValid() || !pCell->GetRDEValue()->GetData())
				continue;

			return pCell->GetRDEValue();
		}
		else if (pObj->IsKindOf(RUNTIME_CLASS(Repeater)))
		{
			Repeater* pRep = STATIC_DOWNCAST(Repeater, pObj);
			if (!pRep->ExistChildID(nA)) 
				continue;

			FieldRect* pField = pRep->GetCellFromID(nRow, nA);
			//ASSERT(pField);
			if (!pField || !pField->GetRDEValue() || !pField->GetRDEValue()->IsValid() || !pField->GetRDEValue()->GetData())
			{
				return NULL;
			}
			return pField->GetRDEValue();
		}
	}
	return NULL;
}

//-----------------------------------------------------------------------------------
BOOL CWoormDocMng::GetObjectFromAlias (int nA, int nRow/*=-1*/, FieldRect*& pField, TableCell*& pCell) 
{
	pField = NULL;
	pCell = NULL;

	for (int nObj = 0; nObj <= GetObjects().GetUpperBound(); nObj++)
	{
		BaseObj* pObj = GetObjects()[nObj];

		if (pObj->GetInternalID() == nA && pObj->IsKindOf(RUNTIME_CLASS(FieldRect)))
		{
			if (pObj->m_AnchorRepeaterID)
				continue;	//sarà gestito dal Repeater

			pField = STATIC_DOWNCAST(FieldRect, pObj);
			return TRUE;
		}
		else if (pObj->IsKindOf(RUNTIME_CLASS(Table)))
		{
			Table* pTable = STATIC_DOWNCAST(Table, pObj);
			if (!pTable->ExistChildID(nA)) 
				continue;

			pCell = pTable->GetCellFromID(nRow, nA);
			if (pCell != NULL)
				return TRUE;
		}
		else if (pObj->IsKindOf(RUNTIME_CLASS(Repeater)))
		{
			Repeater* pRep = STATIC_DOWNCAST(Repeater, pObj);
			if (!pRep->ExistChildID(nA)) 
				continue;

			pField = pRep->GetCellFromID(nRow, nA);
			if (pField != NULL)
				return TRUE;
		}
	}
	return FALSE;
}

//-----------------------------------------------------------------------------------
void CWoormDocMng::UpdateViewSymbolTable () 
{
	UpdateViewSymbolTable(NULL, -1);
}

//-----------------------------------------------------------------------------------
void CWoormDocMng::UpdateViewSymbolTable (BaseObj* pTableOrRepeater, int nRow, BOOL updateOnlyTailCell/* = FALSE*/) 
{
	if (pTableOrRepeater)
	{
		if (pTableOrRepeater->IsKindOf(RUNTIME_CLASS(Table)))
			((Table*)pTableOrRepeater)->m_nViewCurrentRow = nRow;
		else if (pTableOrRepeater->IsKindOf(RUNTIME_CLASS(Repeater)))
			((Repeater*)pTableOrRepeater)->m_nViewCurrentRow = nRow;
	}

	if (pTableOrRepeater && (nRow == -1))	//verificare, rimangono le colonne valide
		return;

	//TODO x otimizzazione: quando non si è in editing, si potrebbe rimuovere dalla sym table tutti i field non referenziati
	for (int i = 0; i < m_ViewSymbolTable.GetCount(); i++)
	{
		WoormField* pF = m_ViewSymbolTable.GetAt(i);
		ASSERT_VALID(pF);
		if (!pF || !pF->GetData())
		{
			//ASSERT(FALSE);
			continue;
		}

		//TODO ottimizzazione (SAFE) if (!pF->GetRefCount()) continue;

		if (pTableOrRepeater)
		{
			if (pTableOrRepeater->IsKindOf(RUNTIME_CLASS(Table)))
			{
				if (!((Table*)pTableOrRepeater)->ExistChildID(pF->GetId()))
					continue;
			}
			else if (pTableOrRepeater->IsKindOf(RUNTIME_CLASS(Repeater)))
			{
				if (nRow < 0)
					continue;	//non ha totali

				if (!((Repeater*)pTableOrRepeater)->ExistChildID(pF->GetId()))
					continue;
			}
		}

		RDEData *pRDEData = GetRDEDataFromAlias(pF->GetId(), nRow);

		if (updateOnlyTailCell && (!pRDEData || !pRDEData->IsTailMultiLineString()))
			continue;

		if (pRDEData && pRDEData->IsValid() && pRDEData->GetData())
			pF->GetData()->Assign(*pRDEData);
		else if (nRow > -1 && pF->IsColumn())
			pF->GetData()->SetValid(FALSE);
	}
}

//-----------------------------------------------------------------------------
BOOL CWoormDocMng::GetAliasObjectFromPoint (int& nAlias, int& nRow, BaseObj*& pTable, BaseRect*& pField, CPoint* pt /*= NULL*/)
{
	pField = NULL; 
	pTable = NULL;
	if (pt == NULL) 
		pt = &m_ptCurrPos;

	for (int nObj = GetObjects().GetUpperBound() ; nObj >= 0 ; nObj--)
	{
		BaseObj* pObj = GetObjects()[nObj];

		nRow = -1;
		nAlias = 0;

		if (pObj->m_AnchorRepeaterID)
			continue;

		if (pObj->IsKindOf(RUNTIME_CLASS(Repeater)))
		{
			CRect rect = pObj->GetRectToInvalidate();
			if (rect.PtInRect(*pt))
			{
				Repeater* pRep = STATIC_DOWNCAST(Repeater, pObj);
				BaseObj* pO = pRep->GetFieldByPosition(*pt, nRow);
				if (pO)
				{
					pTable = pRep;
					nAlias = pO->GetInternalID();
					pField = (BaseRect*) pO;

					return TRUE;
				}

				if (!pObj->InMe(*pt))
					continue;

				nAlias = pObj->GetInternalID();
				pField = (BaseRect*) pObj;

				return TRUE;
			}
			continue;
		}
		else if (!pObj->InMe(*pt))
			continue;

		if (pObj->IsKindOf(RUNTIME_CLASS(Table)))
		{
			Table* pLinkTable = STATIC_DOWNCAST(Table, pObj);
			Table::SelPosition tsp = pLinkTable->GetPosition(*pt, nRow, &nAlias);
			if (tsp == Table::POS_ROW)
			{
				pTable = pLinkTable;

				return TRUE;
			}
			else if (tsp == Table::POS_COLUMN)
			{
				pTable = pLinkTable;
			}
			break;
		}
		else if (pObj->IsKindOf(RUNTIME_CLASS(FieldRect)))
		{
			nAlias = pObj->GetInternalID();
			pField = (BaseRect*) pObj;

			return TRUE;
		}
		else if (pObj->IsKindOf(RUNTIME_CLASS(Chart)))
		{
			nAlias = pObj->GetInternalID();
			pField = (Chart*)pObj;

			return TRUE;
		}
		else if (pObj->IsKindOf(RUNTIME_CLASS(BaseRect)))
		{
			nAlias = 0;
			pField = (BaseRect*) pObj;

			return TRUE;
		}
	}
	return FALSE;
}

//-----------------------------------------------------------------------------
WoormLink* CWoormDocMng::GetEnableLink(BaseObj* pTableOrRepeater, int nAlias, int nRow, BOOL bUpdateSymTable/* = TRUE*/)
{
	BOOL bFoundLinkOnObj = FALSE;
	WoormLink* pConn = NULL;
	for (int nCl = 0; nCl < m_arWoormLinks.GetSize(); nCl++) 
	{
		pConn = (WoormLink*) m_arWoormLinks.GetAt(nCl);
		if (pConn->m_LinkType == WoormLink::ConnectionRadar)
			continue;

		if (nAlias == pConn->m_nAlias)
		{
			RDEData* pRDE = GetRDEDataFromAlias(pConn->m_nAlias, nRow);
			if (pRDE == NULL)
				break;
			if(!pRDE->IsValid() || pRDE->GetData() == NULL || pRDE->IsTailMultiLineString())
				break;

			if (bUpdateSymTable && !pRDE->IsTailMultiLineString())
				UpdateViewSymbolTable(pTableOrRepeater, nRow);
	
			DataBool b(FALSE);
		
			if (pConn->m_pEnableLinkWhenExpr)   //when clause present
			{
				if (!pConn->m_pEnableLinkWhenExpr->Eval(b))
					continue;	//break;

				bFoundLinkOnObj = b; 
			}
			else   //when clause not present
			{
				bFoundLinkOnObj = TRUE;
			}

			if (bFoundLinkOnObj)
				break;
		}
	}
	return bFoundLinkOnObj ? pConn : NULL;
}

//-----------------------------------------------------------------------------
BOOL CWoormDocMng::EvalLinkParameters (CFunctionDescription* pFD, WoormLink* pConn, WoormTable* pSymTable, int nRow)
{
	if (!pSymTable)
		return FALSE;
	ASSERT_VALID(pSymTable);

    for (int i = 0; i <= pSymTable->GetUpperBound(); i++)
	{
		WoormField* pItem = pSymTable->GetAt(i);
		ASSERT_VALID(pItem);
		if (pItem->GetId() == SpecialReportField::ID.LINKED_DOC || pItem->GetId() == SpecialReportField::ID.FUNCTION_RETURN_VALUE)
			continue;

		if (pItem->GetInitExpression() == NULL)
		{
			TRACE("Missing value for link parameter: Identifier %s - row %d \n", pItem->GetName(), nRow);
			//ASSERT(FALSE);		
			return FALSE;
		}
		else
		{
			DataObj* pObj = pItem->GetRuleData();
			ASSERT_VALID(pObj);
			/*eval param-name = expr;*/
			if (!pItem->GetInitExpression()->Eval(*pObj))
			{
				//ASSERT(FALSE);		
				return FALSE;
			}

			CDataObjDescription* pDO = pFD->GetParamDescription(pItem->GetName());
			if (pDO)
			{
				ASSERT_VALID(pDO);
				ASSERT_TRACE(pObj->GetDataType() == pDO->GetDataType(), _T("Link: Before Events evaluate parameters ") + pItem->GetName());
				
				if (pDO->GetPassedMode() == CDataObjDescription::_IN)
					pDO->SetValue(*pObj);
				else
				{
					pDO->SetDataObj(pObj, FALSE);
				}
			}
			else
			{
				pFD->InternalAddParam(pItem->GetName(), pObj, TRUE);
			}
		}
	}
	return TRUE;
}

//------------------------------------------------------------------------------
//BOOL CWoormDocMng::ThereIsWoormLink(int nAlias)
//{
//	return m_arWoormLinks.GetFromID(nAlias) != NULL;
//}

//-----------------------------------------------------------------------------
//occorre cambiare nome: fa due cose differenti, detect mouse hover su hyperlink e get tooltip
BOOL CWoormDocMng::IsMouseHoverLink(CString* psTip /*= NULL*/)
{
	WoormLink* pConn = NULL;
	int nRow = -1;
	int nAlias = 0;
	BaseObj* pTableOrRepeater = NULL;
	BaseRect* pField = NULL;
	
	if (!GetAliasObjectFromPoint(nAlias, nRow, pTableOrRepeater, pField))
	{
		if (psTip && pTableOrRepeater && pTableOrRepeater->IsKindOf(RUNTIME_CLASS(Table)))
		{
			int nCol = ((Table*)pTableOrRepeater)->GetIdxColFromAlias(nAlias);
			if (nCol >= 0)
			{
				const TableColumn* pCol = ((Table*)pTableOrRepeater)->GetColumn(nCol);
				*psTip = pCol->GetTitleTooltip();
			}
		}
		return FALSE;
	}

	if (nAlias)
		pConn = GetEnableLink (pTableOrRepeater, nAlias, nRow); 

	if (psTip)
	{
		//if (!nAlias)
			UpdateViewSymbolTable(pTableOrRepeater, nRow);

		if (pField)
		{
			*psTip = pField->GetTooltip(m_pRDEmanager->CurrPageRead(), m_ptCurrPos);
		}
		else if (pTableOrRepeater && pTableOrRepeater->IsKindOf(RUNTIME_CLASS(Table)))
		{
			TableCell* pCell = ((Table*)pTableOrRepeater)->GetCellFromID(nRow, nAlias);
			ASSERT_VALID(pCell);
			if (pCell)
				*psTip = pCell->GetTooltip(m_pRDEmanager->CurrPageRead());
		}
		//else if (pTableOrRepeater && pTableOrRepeater->IsKindOf(RUNTIME_CLASS(Repeater)))
		//{
		//	FieldRect* fr = ((Repeater*)pTableOrRepeater)->GetCellFromID(nRow, nAlias);
		//	ASSERT_VALID(fr);
		//	if (fr)
		//		*psTip = fr->GetTooltip(m_pRDEmanager->CurrPageRead());
		//}
	}

	return pConn ? TRUE : FALSE;
}

//-----------------------------------------------------------------------------
CString CWoormDocMng::OnMouseHoverLink()
{
	CString sTip;
	if (IsMouseHoverLink(&sTip))
		SetCursor(8);

	return sTip;
}

//-----------------------------------------------------------------------------
BOOL CWoormDocMng::OnLinkSelectedFromWeb(int nAlias, int nRow)
{
	if (m_arWoormLinks.GetSize() == 0)
		return FALSE;
	
	Table* pTable = NULL;
	FieldRect* pField = NULL;
	TableCell* pCell = NULL;

	if (!GetObjectFromAlias(nAlias, nRow, pField, pCell))
		return FALSE;

	if (pCell)
		pTable = pCell->m_pColumn->m_pTable;
	
	return DoBrowseLink(pTable, pField, nAlias, nRow);
}
//-----------------------------------------------------------------------------
BOOL CWoormDocMng::OnLinkSelected ()
{
	if (m_arWoormLinks.GetSize() == 0)
		return FALSE;
	
	int nRow = -1;
	int nAlias = 0;
	BaseObj* pTableOrRepeater = NULL;
	BaseRect* pField = NULL;

	if (!GetAliasObjectFromPoint(nAlias, nRow, pTableOrRepeater, pField))
		return FALSE;

	if (pField && !pField->IsKindOf(RUNTIME_CLASS(FieldRect)))
		return FALSE;

	return DoBrowseLink(pTableOrRepeater, (FieldRect*)pField, nAlias, nRow);
}

//-----------------------------------------------------------------------------
BOOL CWoormDocMng::DoBrowseLink(BaseObj* pTableOrRepeater, FieldRect* pField, int nAlias, int nRow)
{
	CString strLinkShellEx = _T("");
	WoormLink* pConn = GetEnableLink (pTableOrRepeater, nAlias, nRow);
	if(pConn == NULL)
		return FALSE;

	if (m_pRDEmanager && !m_pRDEmanager->IsClosed())
	{
		if (pTableOrRepeater && pTableOrRepeater->IsKindOf(RUNTIME_CLASS(Table)))
		{
			(((Table*)pTableOrRepeater)->GetCellFromID(nRow, nAlias))->m_arBrowsedOnPage.Add(m_pRDEmanager->CurrPageRead());
			GetWoormFrame()->Invalidate();
		}
		else if (pField)
		{
			pField->m_arBrowsedOnPage.Add(m_pRDEmanager->CurrPageRead());
			GetWoormFrame()->Invalidate();
		}
	}

	CWoormInfo* pWrmInfo = NULL; //va bene per tutte le chiamate 
	CBaseDocument* pDoc = NULL;
	DataStr nsDocument;

	SymField* pF = NULL;
	if (pConn->m_bLinkTargetByField)
	{
		pF = m_ViewSymbolTable.GetField(pConn->m_strTarget);
		if (!pF || !pF->GetData())
		{
			return FALSE;
		}
		ASSERT_VALID(pF);
		ASSERT_VALID(pF->GetData());
	}

	switch(pConn->m_LinkType)
	{
		case WoormLink::ConnectionForm:
		{
			pWrmInfo = new CWoormInfo();

			if (pConn->m_bLinkTargetByField)
			{
				nsDocument = *(DataStr*) pF->GetData(); 
			}
			else
				nsDocument = pConn->m_strTarget;

			break;
		}
		case WoormLink::ConnectionReport:
		{
			pWrmInfo = new CWoormInfo();

			if (pConn->m_bLinkTargetByField)
			{
				DataStr* nsReport = (DataStr*) pF->GetData();

				pWrmInfo->m_ReportNames.Add(nsReport->GetString());
			}
			else
				pWrmInfo->m_ReportNames.Add(pConn->m_strTarget);
			break;
		}
		case WoormLink::ConnectionFunction:
		{
			pWrmInfo = new CWoormInfo();

			if (pConn->m_bLinkTargetByField)
			{
				DataStr* nsFunction = (DataStr*) pF->GetData();

				if (!AfxGetTbCmdManager()->GetFunctionDescription((LPCTSTR)(nsFunction), *pWrmInfo))
					return FALSE;
			}
			else
				if (!AfxGetTbCmdManager()->GetFunctionDescription((LPCTSTR)(pConn->m_strTarget), *pWrmInfo))
					return FALSE;

			break;
		}
		case WoormLink::ConnectionURL:
		{
			if (pConn->m_bLinkTargetByField)
			{
				strLinkShellEx = ((DataStr*)pF->GetData())->GetString();
				CTBNamespace nsLinkedFile(strLinkShellEx);

				if (nsLinkedFile.IsValid())
					strLinkShellEx = AfxGetPathFinder()->GetFileNameFromNamespace(nsLinkedFile, AfxGetLoginInfos()->m_strUserName);
			}
			else
			{
				CTBNamespace nsLinkedFile(pConn->m_strTarget);

				if (nsLinkedFile.IsValid())
					strLinkShellEx = AfxGetPathFinder()->GetFileNameFromNamespace(nsLinkedFile, AfxGetLoginInfos()->m_strUserName);
				else
					strLinkShellEx = pConn->m_strTarget;
			}
			break;
		}
		default :
			return FALSE;
	}

	UpdateViewSymbolTable(pTableOrRepeater, nRow);

	CFunctionDescription fdContext;

	if (pConn->m_LinkType != WoormLink::ConnectionURL)
	{
		if (!EvalLinkParameters (pWrmInfo, pConn, pConn->m_pLocalSymbolTable, nRow))
		{
			//TODO show error
			SAFE_DELETE(pWrmInfo);
			return FALSE;
		}

		if (pConn->m_LinkType == WoormLink::ConnectionForm)
		{
			if (!EvalLinkParameters (&fdContext, pConn, pConn->m_pDocumentContextSymbolTable, nRow))
			{
				//TODO show error
				SAFE_DELETE(pWrmInfo);
				return FALSE;
			}
		}
	}

	if (pConn->m_pBeforeLink && !pConn->m_pBeforeLink->IsEmpty())
	{
		if (!pConn->m_pBeforeLink->Exec())
		{
			//TODO show error
			SAFE_DELETE(pWrmInfo);
			return FALSE;
		}
	}
	//---- ----

	if (pConn->m_LinkType == WoormLink::ConnectionForm)
	{
		CContextBag* pContextBag = NULL;
		if (fdContext.GetParameters().GetCount())
		{
			pContextBag = new CContextBag;
			for (int i = 0; i < fdContext.GetParameters().GetCount(); i++)
			{
				CDataObjDescription* pDObj = (CDataObjDescription*) fdContext.GetParameters().GetAt(i);
				ASSERT_VALID(pDObj); 
				ASSERT_KINDOF(CDataObjDescription, pDObj);

				DataObj* pO = (DataObj*) pContextBag->AddContextObject(pDObj->GetName(), pDObj->GetValue()->GetRuntimeClass());
				ASSERT_VALID(pO); 
				ASSERT(pO->IsKindOf(pDObj->GetValue()->GetRuntimeClass()));

				pO->Assign(*pDObj->GetValue());
			}
		}

		pDoc = AfxGetTbCmdManager()->RunDocument(nsDocument.GetString(),szDefaultViewMode, FALSE, NULL, NULL, NULL, NULL, NULL, FALSE, NULL, NULL, pContextBag);
		if (!pDoc)
		{
			delete pWrmInfo;
			return FALSE;
		}

		pDoc->GoInBrowserMode(pWrmInfo);

		SAFE_DELETE(pWrmInfo);
	}
	else if (pConn->m_LinkType == WoormLink::ConnectionReport)
	{
		//pWrmInfo sarà deletato dal report chiamato
		pDoc = AfxGetTbCmdManager()->RunWoormReport (pWrmInfo);
	}
	else if (pConn->m_LinkType == WoormLink::ConnectionFunction)
	{
		AfxGetTbCmdManager()->RunFunction (pWrmInfo);

		//no lo uso dopo SAFE_DELETE(pWrmInfo);
	}
	else if (pConn->m_LinkType == WoormLink::ConnectionURL)
	{
		strLinkShellEx = pConn->EncodeURLString(strLinkShellEx);
		
		//TODO posso controllare se indirizzo web o percorso di rete file esiste
		if (!strLinkShellEx.IsEmpty())
		{
			HINSTANCE hInst = ::TBShellExecute(strLinkShellEx);
			if (hInst <= (HINSTANCE)32)
				Message (ShellExecuteErrMsg((int)hInst));
		}
	}

	//---- ----
	if (pConn->m_pAfterLink && !pConn->m_pAfterLink->IsEmpty())
	{
		if (pDoc)
		{
			ASSERT_VALID(pDoc);
			WoormField* pField = pConn->m_pLocalSymbolTable->GetFieldByID(SpecialReportField::ID.LINKED_DOC);
			ASSERT(pField);
			if (pField)
			{
				// imposto il valore dell'oggetto più interno della gerarchia, la SetReadOnly fa poi sì che
				// venga ereditato anche dagli oggetti figli
				DataLng* pData = (DataLng*) pField->GetRuleData();
				ASSERT(pData);
				ASSERT_KINDOF(DataLng, pData);
				pData->Assign((long)pDoc);
				pData->SetAsHandle();

				pField->ClearMethods ();
				pField->AddMethodList(RUNTIME_CLASS(CBaseDocument));
			}
		}
		else if (pConn->m_LinkType == WoormLink::ConnectionFunction)
		{
			WoormField* pField = pConn->m_pLocalSymbolTable->GetFieldByID(SpecialReportField::ID.FUNCTION_RETURN_VALUE);
			if (pField && pWrmInfo && pWrmInfo->GetReturnValueDataType() != DataType::Void && pWrmInfo->GetReturnValue())
			{
				ASSERT_VALID(pField);
				ASSERT_VALID(pWrmInfo->GetReturnValue());
				
				DataObj* pData = pField->GetRuleData();
				ASSERT_VALID(pData);
				if (pData)
					pData->Assign(*pWrmInfo->GetReturnValue());
			}
		}

		if (!pConn->m_pAfterLink->Exec())
		{
			if (pConn->m_LinkType == WoormLink::ConnectionFunction)
				SAFE_DELETE(pWrmInfo);
			//TODO show cmd error
			return FALSE;
		}
	}

	if (pConn->m_LinkType == WoormLink::ConnectionFunction)
		SAFE_DELETE(pWrmInfo);
	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL  CWoormDocMng::PrepareLayout()
{
	BOOL bOk = m_Layouts.SetCurrent(m_dsCurrentLayoutView.GetString());

	// prepare GetObjects() for cleaning all data.
	// also control reset of line counter and subtotal interline
	InitializeDisplayStatus();
			
	// show pages on statusbar indicator without use CmdUI interface
	// for optimizing refresh velocity on background processing
	SetPageIndicator ();

	// aggiorna eventuali campi statici (special field)
	UpdateSpecialField();

	return bOk;
}

//------------------------------------------------------------------------------
void CWoormDocMng::OnFileNew()
{
	if (CanDoAllowEditingButton())
		AfxOpenDocumentOnCurrentThread(AfxGetTemplate(RUNTIME_CLASS(CWoormView), 0), NULL);	
	else
		AfxMessageBox(_TB("Current User cannot develop reports"));
}

//------------------------------------------------------------------------------
void CWoormDocMng::OnFileOpen()
{
	FailedInvokeCode aFailedCode; 
	AfxGetTbCmdManager()->RunFunction(szOpenReport, &aFailedCode);
}

//------------------------------------------------------------------------------
void CWoormDocMng::OnFileSave()
{		
	//TODO evita il rischio di salvataggio in lingua corrente 
	if (!DoSave(m_strPathName))
	{
		TRACE0("Warning: File save failed\n");
	}

#ifdef DEBUG
	else
		AfxMessageBox(::cwsprintf(_TB("Report saved in path: {0-%s}"), m_strPathName));
#endif
}

//------------------------------------------------------------------------------
void CWoormDocMng::OnFileSaveAs()
{		
	if (!DoSave(NULL))
		TRACE0("Warning: File save-as failed\n");
}

//------------------------------------------------------------------------------
void CWoormDocMng::OnFileSaveAsTemplate()
{		
	m_bCanClose = FALSE;//evito schiantamenti se la DoSave tentasse di chiudermi il documento sollo il sedere
	if (IsModified())
	{
		if (AfxMessageBox(_TB("Current report it is modified! Do you want save it before save the template ?"), MB_YESNO|MB_ICONHAND| MB_DEFBUTTON1) == IDYES)
		{
			if (!DoSave(m_strPathName))
			{
				TRACE0("Warning: File save-as template failed\n");
				return;
			}
		}
	}
	
	BOOL bFlag = m_bOnlyGraphInfo;
	m_bOnlyGraphInfo = TRUE;
	m_Template.m_bIsSavingTemplate = TRUE;

	if (!DoSave(NULL))
		TRACE0("Warning: File save-as template failed\n");

	m_bOnlyGraphInfo = bFlag;
	m_Template.m_bIsSavingTemplate = FALSE;
	m_bCanClose = TRUE;
}

//------------------------------------------------------------------------------
void CWoormDocMng::FileAutoSave()
{
	// Stop Timer event
	ASSERT(m_nTimerAutoSave != 0);
	if (m_nTimerAutoSave != 0)
		GetFirstView()->KillTimer(m_nTimerAutoSave);

	// Make & save file
	CString strCustomName = AfxGetPathFinder()->GetModuleReportPath(GetNamespace(), CPathFinder::USERS, AfxGetLoginInfos()->m_strUserName, TRUE);
	strCustomName += SLASH_CHAR;
	CString sUserPath = strCustomName + GetNameWithExtension(m_strPathName);
	sUserPath.Replace(_T(".wrm"), _T("_AUTOSAVE.wrm"));

	if (CanSaveReport())
	{
		if (ExistFile(sUserPath))
		{
			RemoveFile(sUserPath);
		}

		// make correct title and  start wait cursor
		BeginWaitCursor();

		// save on wrm strFile without bakacup
		UnparseWoorm(sUserPath, m_bOnlyGraphInfo);	//OnSaveDocument

		if (!m_bOnlyGraphInfo && m_pOptions->MustCreateXsd())
			GetSchema(sUserPath);

		EndWaitCursor();
	}

	// restart time
	SetAutoSaveTimer();
}

// Save the document data to a strFile
// pszPathName = path name where to save document strFile
// if pszPathName is NULL then the user will be prompted (SaveAs)
// note: pszPathName can be different than 'm_strPathName'
// if 'bReplace' is TRUE will change strFile name if successful (SaveAs)
// if 'bReplace' is FALSE will not change path name (SaveCopyAs)
//------------------------------------------------------------------------------
BOOL CWoormDocMng::DoSave(LPCTSTR pszPathName, BOOL bReplace /*=TRUE*/)
{		
	// avoid save to strFile if non object are present (@@ Itri enanchement)
	if (!CanSaveReport()) return TRUE;

	CTBNamespace aNs;// ();
	aNs.SetType(CTBNamespace::REPORT);
	if (GetNamespace().IsValid())
	{
		aNs.SetApplicationName(GetNamespace().GetApplicationName());
		aNs.SetObjectName(CTBNamespace::MODULE, GetNamespace().GetObjectName(CTBNamespace::MODULE));
		if (m_pDataDefaults && m_pDataDefaults->m_bUseAsRadar && m_pCallerDocument)
		{
			if 
				(
					aNs == CTBNamespace(CTBNamespace::MODULE, _NS_MOD("Framework.TbRadar"))
					||
					::ExistPath(aNs.ToUnparsedString())
				)
			{
				aNs.SetApplicationName(m_pCallerDocument->GetNamespace().GetApplicationName());
				aNs.SetObjectName(CTBNamespace::MODULE, m_pCallerDocument->GetNamespace().GetObjectName(CTBNamespace::MODULE));
			}
		}
	}
	else if (m_pCallerDocument)
	{
		aNs.SetApplicationName(m_pCallerDocument->GetNamespace().GetApplicationName());
		aNs.SetObjectName(CTBNamespace::MODULE, m_pCallerDocument->GetNamespace().GetObjectName(CTBNamespace::MODULE));
	}

	//----------------------------------------------------
	//Nuovo report o utente sviluppatore
	CString newName = pszPathName;
	if (newName.IsEmpty() || IsUserReportsDeveloper())
	{
		CDocTemplate* pTemplate = GetDocTemplate();
		ASSERT(pTemplate != NULL);

		newName = m_strPathName;
		if (newName.IsEmpty())
		{
			newName = m_strTitle;
			//if (newName.GetLength() > 8)
			//	newName.ReleaseBuffer(8);
			newName.Replace(' ', '_');
			newName.Replace('.', '_');
			int iBad = newName.FindOneOf(_T(" #%;/\\"));    // dubious filename
			if (iBad != -1)
				newName.ReleaseBuffer(iBad);

			// append the default suffix if there is one
			CString strExt;
			if (pTemplate->GetDocString(strExt, CDocTemplate::filterExt) &&
			  !strExt.IsEmpty())
			{
				ASSERT(strExt[0] == DOT_CHAR);
				newName += strExt;
			}
		}

		// mi arriva un namespace valido ma che non mi serve
		if (newName.IsEmpty())
			aNs.Clear();
		newName = ::GetNameWithExtension(newName);

		AddOnApplication* pAddOnApp = AfxGetBaseApp()->GetMasterAddOnApp();
		if (
				pAddOnApp && 
				pAddOnApp->m_pAddOnModules && 
				pAddOnApp->m_pAddOnModules->GetSize() > 1 &&
				!aNs.IsValid()
			)
		{
			aNs.SetNamespace(pAddOnApp->m_pAddOnModules->GetAt(0)->m_Namespace);
			aNs.SetObjectName(CTBNamespace::REPORT, newName, TRUE);
		}
		else
			aNs.SetObjectName(CTBNamespace::REPORT, newName, TRUE);

		BOOL bSaveAs = FALSE;
		if (GetNamespace().GetType() == CTBNamespace::REPORT)
			bSaveAs = TRUE;

		BOOL bSaveForDlgExplorer = TRUE;

		if (!bSaveAs)
			bSaveForDlgExplorer = FALSE;
		else
			if (!pszPathName)
				bSaveForDlgExplorer = FALSE;

		if (m_Template.m_bIsSavingTemplate)
		{
			CString s = aNs.GetObjectName();
			int idx = s.ReverseFind('.');
			if (idx < 0)
				s += _T(".wrmt");
			else if (s.GetLength() == (idx + 4) && s.Mid(idx).CompareNoCase(_T(".wrm")) == 0)
				s += 't';
			aNs.SetObjectName(s);
		}

		CStringArray aNewName;
		//----------------------------
#ifdef __DEBUG		
		if (this->m_bBetaFeatures)
		{
			CFullSaveAdminDialog tbExplorer(aNs, this->GetFrame());
			if (tbExplorer.DoModal() != IDOK)
				return FALSE;

			if (!tbExplorer.GetSavePath(aNewName))
				return FALSE;

			if (tbExplorer.GetIsSavedInCurrentLanguage())
				m_bSaveAsWithCurrentLanguage = TRUE;
		}
		else
#endif
		{
			CTBExplorer tbExplorer(CTBExplorer::SAVE, aNs, TRUE, 0, (IsUserReportsDeveloper() ? CTBExplorer::FORCE_ADMIN : CTBExplorer::DEFAULT), bSaveForDlgExplorer);
			if (!tbExplorer.Open())
				return FALSE;

			if (!tbExplorer.GetSavePath(aNewName))
				return FALSE;

			if (tbExplorer.GetIsSavedInCurrentLanguage())
				m_bSaveAsWithCurrentLanguage = TRUE;
		}
		//----------------------------

		int n = 0;
		BOOL bSendMessageToMenu = FALSE;
		for (n = 0 ; n < aNewName.GetSize() ; n++)
		{
			newName = aNewName.GetAt(n);
			if (newName.IsEmpty())
				return FALSE;

			if (n == 0 && m_bSaveAsWithCurrentLanguage)
			{
				m_strOriginFileName = ::GetNameWithExtension (GetPathName());
				m_strOriginDictionaryPath = GetDictionaryPath(TRUE);
			}
			
			GetNamespace().SetNamespace(AfxGetPathFinder()->GetNamespaceFromPath(newName));
		
			if (!DoSave2(pszPathName, bReplace, newName, FALSE))
				return FALSE;

			m_strPathName = newName;

			//Aggiunge il file alla customizzazione corrente
			AfxAddFileToCustomizationContext(newName);
			
			if (AfxGetPathFinder()->IsCustomPath(newName))
				bSendMessageToMenu = TRUE;
		}
	
		//comunico al menu di rinfrescare la vista sui report custom
		if (bSendMessageToMenu)
			::PostMessage(AfxGetMenuWindowHandle(), UM_REFRESH_USER_OBJECTS, REFRESH_USER_REPORT, NULL);
			
		if (m_bSaveAsWithCurrentLanguage && n > 0)
		{
			m_bSaveAsWithCurrentLanguage = FALSE;
			
			//AfxTBMessageBox(_TB("Now Woorm will reload the localized report"), MB_ICONINFORMATION | MB_OK);  //@@BAUZI: CHIEDERE A RICHI
			if (!m_pDataDefaults->m_bUseAsRadar)
			{
				CWoormInfo* pWInfo = m_pWoormInfo;
				m_pWoormInfo = NULL;
				if (pWInfo)
				{
					pWInfo->m_ReportNames.RemoveAll();
					pWInfo->m_ReportNames.Add(newName);

					AfxGetTbCmdManager()->RunWoormReport(pWInfo, m_pCallerDocument, NULL, FALSE, FALSE);
				}
				else
					AfxGetTbCmdManager()->RunWoormReport(newName, m_pCallerDocument, FALSE, FALSE);
				
				GetFirstView()->PostMessage(WM_COMMAND, ID_FILE_CLOSE);
			}
		}
		return TRUE;
	}

	//----------------------------------------
	//report esistente ed utente NON sviluppatore? : POSSIBILE CODICE MORTO
	CString strCustomName = AfxGetPathFinder()->GetModuleReportPath(GetNamespace(), CPathFinder::USERS, AfxGetLoginInfos()->m_strUserName, TRUE);

	if (AfxGetPathFinder()->GetPosTypeFromPath(strCustomName) == CPathFinder::ALL_USERS)
		strCustomName = AfxGetPathFinder()->GetModuleReportPath(GetNamespace(), CPathFinder::ALL_USERS, _T(""), TRUE);
	
	strCustomName += SLASH_CHAR;
	newName = strCustomName + GetNameWithExtension(m_strPathName);
	
	BOOL bRet = DoSave2(pszPathName, bReplace, newName, FALSE);
	//comunico al menu di rinfrescare la vista sui report custom
	if (bRet && AfxGetPathFinder()->IsCustomPath(newName))
	{
		::PostMessage(AfxGetMenuWindowHandle(), UM_REFRESH_USER_OBJECTS, REFRESH_USER_REPORT, NULL);
		
		//Aggiunge il file alla customizzazione corrente
		AfxAddFileToCustomizationContext(newName);
	}
	return bRet;
}

//------------------------------------------------------------------------------
BOOL CWoormDocMng::DoSave2(LPCTSTR pszPathName, BOOL bReplace, const CString& newName, BOOL /*bSetNs*/)
{		
	if (newName.IsEmpty())
		return FALSE;

	if (!OnSaveDocument(newName))
	{
		m_bSaveAsWithCurrentLanguage = FALSE;
		if (pszPathName != NULL)
			return FALSE;

		return FALSE;
	}

	if (bReplace)
	{
		// Reset the title and change the document name
		if (IsReportString())
		{
			m_pWoormInfo->m_bIsReportString = FALSE;
			m_pWoormInfo->m_ReportNames.SetAt(0, newName);
		}
		SetReportPathName(newName);
		//ASSERT(m_strPathName == newName);       // must be set
	}
	return TRUE;
}

//------------------------------------------------------------------------------
BOOL CWoormDocMng::SaveModified()
{
	if (!m_bCanClose)
		return FALSE;

	// if running cannot exit (must stop it before)
	if (!EngineStopped())
		return FALSE;
				
	// check if bOk to continue
	if (!IsModified())
		return TRUE;

	// avoid save to strFile if no object are present (@@ Itri enanchement)
	if (!CanSaveReport()) 
		return TRUE;

	CString name = m_strPathName;
	if (name.IsEmpty())
		name = m_strTitle;

	CString prompt;
	prompt = cwsprintf(_TB("Do you want save changes to {0-%s} ?"), name);
	int nID = IDYES;
	if (!IsInUnattendedMode())
		nID = AfxMessageBox(prompt, MB_YESNOCANCEL);
	switch (nID)
	{
	case IDCANCEL:
		return FALSE;       // don't continue

	case IDYES:
		// If so, either Save or Update, as appropriate
		if (!DoSave(NULL/*m_strPathName*/))
			return FALSE;       // don't continue
		break;

	case IDNO:
		// If not saving changes, revert the document
		break;

	default:
		ASSERT(FALSE);
		break;
	}

	return TRUE;    // keep going
}

//------------------------------------------------------------------------------
void CWoormDocMng::OnUpdateEasyReading(CCmdUI* pCmdUI)
{
	BOOL bEnable = FALSE;
	for (int i = 0; i <= GetObjects().GetUpperBound(); i++)
	{
		BaseObj* pObj = GetObjects()[i];
		if (pObj->IsKindOf(RUNTIME_CLASS(Table)))
		{
			bEnable = TRUE;
			break;
		}
	}

	pCmdUI->Enable(bEnable);
}

//-----------------------------------------------------------------------------
void CWoormDocMng::OnEasyReadingByColor()
{	//TODO gestire solo tabella/e selezionate
	for (int i = 0; i <= GetObjects().GetUpperBound(); i++)
	{
		BaseObj* pObj = GetObjects()[i];
		if (!pObj->IsKindOf(RUNTIME_CLASS(Table)))
			continue;

		((Table*)pObj)->OnEasyReadingByColor();
	}
}

//-----------------------------------------------------------------------------
void CWoormDocMng::OnEasyReadingByColorDynamic()
{	//TODO gestire solo tabella/e selezionate
	for (int i = 0; i <= GetObjects().GetUpperBound(); i++)
	{
		BaseObj* pObj = GetObjects()[i];
		if (!pObj->IsKindOf(RUNTIME_CLASS(Table)))
			continue;

		((Table*)pObj)->OnEasyReadingByColorDynamic();
	}
}

//-----------------------------------------------------------------------------
void CWoormDocMng::OnEasyReadingByLine()
{   //TODO gestire solo tabella/e selezionate
	for (int i = 0; i <= GetObjects().GetUpperBound(); i++)
	{
		BaseObj* pObj = GetObjects()[i];
		if (!pObj->IsKindOf(RUNTIME_CLASS(Table)))
			continue;

		((Table*)pObj)->OnEasyReadingByLine();
	}
}

//-----------------------------------------------------------------------------
void CWoormDocMng::OnEasyReadingByLineDynamic()
{   //TODO gestire solo tabella/e selezionate
	for (int i = 0; i <= GetObjects().GetUpperBound(); i++)
	{
		BaseObj* pObj = GetObjects()[i];
		if (!pObj->IsKindOf(RUNTIME_CLASS(Table)))
			continue;

		((Table*)pObj)->OnEasyReadingByLineDynamic();
	}
}

//-----------------------------------------------------------------------------
int CWoormDocMng::CalculateFieldWidth(int objectId, const CString& strText)
{	
	const TableColumn* pCol = m_Layouts.FindColumnByID (objectId, this->m_dsCurrentLayoutEngine);
	if (pCol)
		return pCol->CalculateFieldWidth(strText);

	ASSERT(FALSE);
	TRACE("Function 'CalculateFieldWidth' does not support the kind of object associated with alias %d", objectId);
	return 0;
}

//-----------------------------------------------------------------------------
BOOL CWoormDocMng::GetFieldWidthFactors(int objectId, FieldWidthFactors& fieldWidthFactors, BOOL bSubTotal /*= FALSE*/)
{
	const TableColumn* pCol = m_Layouts.FindColumnByID (objectId, this->m_dsCurrentLayoutEngine);
	if (pCol)
		return pCol->CalculateFieldWidthFactors(fieldWidthFactors, bSubTotal);

	ASSERT(FALSE);
	TRACE("Function 'GetFieldWidthFactors' does not support the kind of object associated with alias %d", objectId);
	return FALSE;
}

//-----------------------------------------------------------------------------
BOOL CWoormDocMng::UseOptimizedLineBreak()
{
	if (m_pWoormIni)
		return m_pWoormIni->m_bOptimizedLineBreak;
	return __super::UseOptimizedLineBreak();
}

//-----------------------------------------------------------------------------
HMENU CWoormDocMng::GetDefaultMenu()
{
	//@@ TODO interessante per gestire la differenza tra editor e runner
	return NULL;    // just use original default
}

//-----------------------------------------------------------------------------
BOOL CWoormDocMng::ExistsFieldID(WORD id, BOOL bSkipCurrentLayout) const
{
	return m_Layouts.ExistsFieldID(id, bSkipCurrentLayout);
}
//-----------------------------------------------------------------------------
BOOL CWoormDocMng::ExistsID (WORD id, CString sLayoutName) const
{ 
	return m_Layouts.ExistsID	(id, sLayoutName);
}
//-----------------------------------------------------------------------------
IBaseObj* CWoormDocMng::FindByID (WORD id, CString sLayoutName) const
{ 
	return m_Layouts.FindByID	(id, sLayoutName);
}

//-----------------------------------------------------------------------------
BOOL CWoormDocMng::ExistsColumnID (WORD id, CString sLayoutName) const
{ 
	return m_Layouts.ExistsColumnID	(id, sLayoutName);
}

BOOL CWoormDocMng::ColumnShowedAsImage(WORD id, CString sLayoutName) const
{
	const TableColumn* pCol = m_Layouts.FindColumnByID(id, sLayoutName);
	return pCol ? pCol->m_pBitmap != NULL : FALSE;
}

CObject* CWoormDocMng::GetColumnPtr(WORD id, CString sLayoutName) const
{
	const TableColumn* pCol = m_Layouts.FindColumnByID(id, sLayoutName);
	return (CObject*) pCol;
}

//-----------------------------------------------------------------------------
BOOL CWoormDocMng::OnGetToolTipText (CPoint* pt, CString& strMessage)
{
	if (!m_pRDEmanager || IsAborted() || !IsFilledWithData())
		return FALSE;

	int nAlias = 0;
	int nRow = -1;
	BaseObj* pTable = NULL;
	BaseRect* pField = NULL;

	if (GetAliasObjectFromPoint (nAlias, nRow, pTable, pField, pt))
	{
		if (pTable && pTable->IsKindOf(RUNTIME_CLASS(Table)))
		{
			TableCell* 	pCell = ((Table*)pTable)->GetCellFromID(nRow, nAlias);
			strMessage = pCell->GetTooltip(m_pRDEmanager->CurrPageRead());
			return !strMessage.IsEmpty();
		}
		else if (pField)
		{
			strMessage = pField->GetTooltip(m_pRDEmanager->CurrPageRead());
			return !strMessage.IsEmpty();
		}
	}
	return FALSE;
}

///////////////////////////////////////////////////////////////////////////////

//-----------------------------------------------------------------------------
BOOL CWoormDocMng::SplitCurrentLayout()
{
	RestoreCurrentLayout();

	if (m_PageInfo.m_arHPageSplitter.GetSize() == 0)
		return FALSE;

	m_pFullLayoutSplitted = m_Objects;

	for (int i = 0; i <= m_PageInfo.m_arHPageSplitter.GetSize(); i++)
	{
		CLayout* pSplitter = new CLayout(*m_Objects);
		AnchorFieldToColumn(pSplitter);
		m_arSplittedObjects.Add(pSplitter);

		RecalcLayoutSplittedPage(i);
	}

	return TRUE;
}

//-----------------------------------------------------------------------------
void CWoormDocMng::RestoreCurrentLayout()
{
	m_pCurrentObj = NULL;
	m_arSplittedObjects.RemoveAll();
	m_nCurrentSplittedPage = -1;
	m_nSplittedPageMaxWidth = 0;

	if (!m_pFullLayoutSplitted)
		return;

	m_Objects = m_pFullLayoutSplitted;
	m_pFullLayoutSplitted = NULL;

	if (!m_pRDEmanager->IsClosed())
		ReadSelectedPage(m_pRDEmanager->LastPage());

	//Invalidate(TRUE);
}

//-----------------------------------------------------------------------------
void CWoormDocMng::RecalcLayoutSplittedPage(int nSplittedPage)
{
	if (nSplittedPage < 0 || nSplittedPage >= m_arSplittedObjects.GetSize())
	{
		ASSERT(FALSE);
		return;
	}
	CLayout* pSplitter = (CLayout*) m_arSplittedObjects[nSplittedPage];

	int bTableFound = FALSE;
	for (int i = 0; i < pSplitter->GetSize(); i++)
	{
		BaseObj* pObj = (*pSplitter)[i];

		if (pObj->IsKindOf(RUNTIME_CLASS(Table)))
		{
			if (!bTableFound)
			{
				RecalcLayoutSplittedPage(nSplittedPage, (Table*)pObj);
				bTableFound = TRUE;
			}
			else
			{
				ASSERT(FALSE);
				continue;
			}
		}
		else if (pObj->IsKindOf(RUNTIME_CLASS(BaseRect)))
		{
			RecalcLayoutSplittedPage(nSplittedPage, (BaseRect*)pObj);
		}
	}
}

//-----------------------------------------------------------------------------
void CWoormDocMng::RecalcLayoutSplittedPage (int nSplittedPage, Table* pTable)
{
	int nPrevSplittedPage = 0;
	for (int nCol = 0; nCol < pTable->m_Columns.GetSize(); nCol++)
	{
		TableColumn* pCol = pTable->m_Columns[nCol];
		
		if (!pCol->IsPinned() && nPrevSplittedPage != nSplittedPage)
		{
			SAFE_DELETE(pCol->m_pHideExpr);
			pTable->SetColumnHiddenStatus(nCol, TRUE);
		}

		if (pCol->IsSplitter())
		{
			nPrevSplittedPage++;
		}
	}	
}

//-----------------------------------------------------------------------------
LONG CWoormDocMng::GetPosXofAnchoredRect(BaseRect* pBaseRect, BOOL bLeft)
{	
	//restituisce la x del BaseRect sulla pagina fisica (non quella splitted) tenendo conto dell'offset
	//orizzontale tra left del baseRect e suo splitter piu vicino, a sx o dx a seconda del valore di bLeft
	CRect baseRect (pBaseRect->GetBaseRect());
	LONG xPos = baseRect.left;

	if (bLeft)  //offset rispetto a splitter a sx
	{
		xPos = baseRect.left;  //se e' nella prima pagina, lo posiziona alla x in cui e' adesso.
		for (int i = 0; i < m_PageInfo.m_arHPageSplitter.GetSize(); i++ )
		{
			if ( baseRect.left > m_PageInfo.m_arHPageSplitter.GetAt(i) )
			{
				xPos = baseRect.left - m_PageInfo.m_arHPageSplitter.GetAt(i);
				break;
			}
		}
	}
	else		//offset rispetto a splitter a dx
	{
		int xOffset = m_PageInfo.GetPageSize_LP().cx - baseRect.left;     //lo inizializzo come fosse nell'ultima pagina
		for (int i = 0; i < m_PageInfo.m_arHPageSplitter.GetSize(); i++ )
		{
			if ( m_PageInfo.m_arHPageSplitter.GetAt(i) > baseRect.left)
			{
				xOffset = m_PageInfo.m_arHPageSplitter.GetAt(i) - baseRect.left;
				break;
			}
		}
		
		CSize prtPageSize(m_PageInfo.GetPrinterPageSize_LP());
		if (xOffset + baseRect.Width() < prtPageSize.cx)
			xPos = prtPageSize.cx - xOffset;
		else
			xPos = prtPageSize.cx - baseRect.Width();
	}

	return xPos;
}

//-----------------------------------------------------------------------------
void CWoormDocMng::RecalcLayoutSplittedPage (int nSplittedPage, BaseRect* pBaseRect)
{
	if (pBaseRect->IsAnchored())	//per i campi ancorati alla colonna non devo fare nulla
		return;
	
	//algoritmo di riposizionamento campi ancorati a sx, a dx, oppure non ancorati ma fuori dalla pagina fisica 
	//(in questo caso vanno riposizionati solo per la pagina in cui sarebbero visibili normalmente)
	CRect baseRect (pBaseRect->GetBaseRect());
	LONG xPos = 0;
	
	if (pBaseRect->m_bAnchorPageRight)
	{
		xPos = GetPosXofAnchoredRect(pBaseRect, FALSE);
		pBaseRect->MoveBaseRect(
								 xPos,
								 baseRect.top,
								 xPos + baseRect.Width(),
								 baseRect.bottom
							   );
	}

	if (pBaseRect->m_bAnchorPageLeft)
	{
		xPos = GetPosXofAnchoredRect(pBaseRect, TRUE);
		pBaseRect->MoveBaseRect(
								xPos,
								baseRect.top,
								xPos + baseRect.Width(),
								baseRect.bottom
							);
	}
	//Quelli che non sono ancorati...li stampo solo nella pagina splittata in cui si trovano
	if (!pBaseRect->m_bAnchorPageRight && !pBaseRect->m_bAnchorPageLeft)
	{
		int nPage = 0;
		for (int i = 0; i < m_PageInfo.m_arHPageSplitter.GetSize(); i++ )
		{
			if ( baseRect.left > m_PageInfo.m_arHPageSplitter.GetAt(i))	
			{ 
				nPage = i+1;	
			}
		}

		if (nPage == nSplittedPage)
		{
			if (nPage > 0)
			{	
				xPos = 	GetPosXofAnchoredRect(pBaseRect, TRUE);
					
				pBaseRect->MoveBaseRect(
								xPos,
								baseRect.top,
								xPos + baseRect.Width(),
								baseRect.bottom
							);
			}
		}
		else
		{
			SAFE_DELETE(pBaseRect->m_pHideExpr);
			pBaseRect->m_bHidden = TRUE;
		}

	}
}

//-----------------------------------------------------------------------------
void CWoormDocMng::CalcSplittedPageMaxWidth()
{
	for (int s = 0; s < m_arSplittedObjects.GetSize(); s++)
	{
		CLayout* pSplitter = (CLayout*) m_arSplittedObjects[s];

		int bTableFound = FALSE;
		for (int i = 0; i < pSplitter->GetSize(); i++)
		{
			BaseObj* pObj = (*pSplitter)[i];

			if (pObj->IsKindOf(RUNTIME_CLASS(Table)) && !bTableFound)
			{
				Table* pTable = (Table*) pObj;
				pTable->CheckColumnsHiddenStatus(); //ATTENZIONE: check con i dati delle prima pagina

				int w = pTable->GetBaseRect().right;
				if (m_nSplittedPageMaxWidth < w)
					m_nSplittedPageMaxWidth = w;
				bTableFound = TRUE;
			}		
			else if (pObj->IsKindOf(RUNTIME_CLASS(BaseRect)))
			{
				BaseRect* pRect = (BaseRect*) pObj;
				if (pRect->m_bHidden)
					continue;
				//sono gia stati traslati in fase di splitting
				int w = pObj->GetBaseRect().right;
				if (m_nSplittedPageMaxWidth < w)
					m_nSplittedPageMaxWidth = w;
			}
		}
	}
}

//-----------------------------------------------------------------------------
BOOL CWoormDocMng::SetFirstSplitterPage()
{
	m_pCurrentObj = NULL;
	if (m_arSplittedObjects.GetSize() == 0 || !m_pFullLayoutSplitted)
		return FALSE;

	m_nCurrentSplittedPage = 0;
	m_Objects = (CLayout*) m_arSplittedObjects[m_nCurrentSplittedPage];
	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CWoormDocMng::SetLastSplitterPage()
{
	m_pCurrentObj = NULL;
	if (m_arSplittedObjects.GetSize() == 0 || !m_pFullLayoutSplitted)
		return FALSE;

	m_nCurrentSplittedPage = m_arSplittedObjects.GetUpperBound();
	m_Objects = (CLayout*) m_arSplittedObjects[m_nCurrentSplittedPage];
	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CWoormDocMng::SetNextSplitterPage()
{
	if (!AllowNextSplitterPage())
		return FALSE;

	m_Objects = (CLayout*) m_arSplittedObjects[++m_nCurrentSplittedPage];
	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CWoormDocMng::SetPrevSplitterPage()
{
	if (!AllowPrevSplitterPage())
		return FALSE;

	m_Objects = (CLayout*) m_arSplittedObjects[--m_nCurrentSplittedPage];
	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CWoormDocMng::AllowNextSplitterPage()
{
	return m_arSplittedObjects.GetSize() && m_pFullLayoutSplitted && m_nCurrentSplittedPage < m_arSplittedObjects.GetUpperBound();
}

//-----------------------------------------------------------------------------
BOOL CWoormDocMng::AllowPrevSplitterPage()
{
	return m_arSplittedObjects.GetSize() && m_pFullLayoutSplitted && m_nCurrentSplittedPage > 0;
}

//Metodo che restituisce il numero di oggetti grafici di tipo tabella presenti nel report
//-----------------------------------------------------------------------------
int CWoormDocMng::GetTableCount()
{
	BaseObj* pObj;
	int i, nTables = 0;
	for (i = 0; i <= GetObjects().GetUpperBound (); i++)
	{
		pObj = GetObjects()[i];
		if ( pObj -> IsKindOf (RUNTIME_CLASS(Table)) ) 
			nTables++;
	}
	return nTables;
}
//-----------------------------------------------------------------------------
CList<Table*>* CWoormDocMng::GetTables()
{
	CList<Table*>* tables =  new CList<Table*>();
	BaseObj* pObj;
	for (int i = 0; i <= GetObjects().GetUpperBound(); i++)
	{
		pObj = GetObjects()[i];
		if (pObj->IsKindOf(RUNTIME_CLASS(Table)))
			tables->AddTail((Table*)pObj);
	}
	return tables;
}

//-----------------------------------------------------------------------------
Table* CWoormDocMng::GetTableFromColumnId(WORD columnId)
{
	CList<Table*>* pTables = GetTables();
	Table* returnedTable = NULL;

	for (POSITION pos = pTables->GetHeadPosition(); pos != NULL;)
	{
		Table* pTable = pTables->GetNext(pos);
		ASSERT_VALID(pTable);
		
		for (int nCol = 0; nCol < pTable->GetColumns().GetSize(); nCol++)
		{
			const TableColumn* pCol = pTable->GetColumn(nCol);
			if (pCol->GetInternalID() == columnId)
				returnedTable = pTable;
		}
	}

	delete pTables;

	return returnedTable;
}

//=============================================================================
//Test Manager

BOOL CWoormDocMng::CheckReportData(CWoormDocMng* pTesterWDoc, CStringArray& arMsgs) 
{ 
	if ( !(pTesterWDoc && pTesterWDoc->m_pRDEmanager && !pTesterWDoc->IsAborted() && pTesterWDoc->IsFilledWithData()))
	{
		ASSERT(FALSE);
		arMsgs.Add(_TB("The tester report is in wrong state"));

		return FALSE;
	}

	if 
		(
			this->m_pRDEmanager == NULL ||
			this->IsAborted() ||
			!this->IsFilledWithData()
		)
	{
		arMsgs.Add(_TB("The tested report failed to run"));
		
		return FALSE;
	};

	if (pTesterWDoc->m_pRDEmanager->LastPage() != this->m_pRDEmanager->LastPage())
	{
		arMsgs.Add(
			cwsprintf(_TB("Report have a different number of pages: tester report has %d pages, tested report has %d pages"), 
			pTesterWDoc->m_pRDEmanager->LastPage(), this->m_pRDEmanager->LastPage())
			);
	};

	int nMsg = arMsgs.GetSize();

	//IMPORTANTE
	//L'agoritmo verifica che i dati contenuti nel report di Test (pTesterWDoc e' il testER report) 
	//siano anche contenuti nel report corrente (this e' il testED report).
	//Questo permette che il report che subisce la verifica (this)  possa avere dei campi in piu' rispetto a quello di test
	for (int nPage = 0; nPage <= pTesterWDoc->m_pRDEmanager->LastPage() && nPage <= this->m_pRDEmanager->LastPage(); nPage++)
	{
		//leggo i dati della pagina 
		pTesterWDoc->ReadSelectedPage (nPage, FALSE);
		this->ReadSelectedPage (nPage, FALSE);

		for (int nObj = 0; nObj <= pTesterWDoc->GetObjects().GetUpperBound(); nObj++)
		{
			BaseObj* pObj = pTesterWDoc->GetObjects()[nObj];
			int nA = pObj->GetInternalID();

			if (pObj->IsKindOf(RUNTIME_CLASS(Table)))
			{
				Table* pTesterTable = STATIC_DOWNCAST(Table, pObj);

				for (int nCol = 0; nCol <= pTesterTable->m_Columns.GetUpperBound(); nCol++)
				{
					TableColumn* pTesterCol =  pTesterTable->m_Columns[nCol];
					int nACol = pTesterCol->GetInternalID();
					for (int nRow = 0; nRow <= pTesterCol->m_Cells.GetUpperBound(); nRow++)
					{
						TableCell* pTesterCell = pTesterCol->m_Cells[nRow];

						CheckReportCompareData(pTesterWDoc, arMsgs, pTesterCell, NULL, nPage, nACol, nRow);
					}

					if (pTesterCol->m_pTotalCell)
					{
						TotalCell* pTesterCell = pTesterCol->m_pTotalCell;
						
						CheckReportCompareData(pTesterWDoc, arMsgs, pTesterCell, NULL, nPage, nACol);
					}
				}
			}
			else if (pObj->IsKindOf(RUNTIME_CLASS(Repeater)))
			{
				Repeater* pTesterRep = STATIC_DOWNCAST(Repeater, pObj);

				for (int nRow = 0; nRow <= pTesterRep->RowsNumber(); nRow++)
				{
					//TODO REPEATER
					CLayout* pRows = pTesterRep->GetRowObjects (nRow);
					ASSERT_VALID(pRows);
					for (int nCol = 0; nCol <= pRows->GetUpperBound(); nCol++)
					{
						BaseObj* pObj = (BaseObj*) pRows->GetAt(nCol);
						if (pObj->IsKindOf(RUNTIME_CLASS(FieldRect)))
						{
							FieldRect* pTesterField = STATIC_DOWNCAST(FieldRect, pObj);				
							CheckReportCompareData(pTesterWDoc, arMsgs, NULL, pTesterField, nPage, pTesterField->GetInternalID(), nRow);
						}
					}
				}
			}
			else if (pObj->IsKindOf(RUNTIME_CLASS(FieldRect)))
			{
				FieldRect* pTesterField = STATIC_DOWNCAST(FieldRect, pObj);
				if (pTesterField->GetInternalID() >= SpecialReportField::REPORT_LOWER_SPECIAL_ID || pTesterField->m_AnchorRepeaterID)
					continue;
				
				CheckReportCompareData(pTesterWDoc, arMsgs, NULL, pTesterField, nPage, nA);
			}
		}
	}
	return nMsg == arMsgs.GetSize(); 
}

//-----------------------------------------------------------------------------
BOOL CWoormDocMng::CheckReportCompareData
	(
		CWoormDocMng* pTesterWDoc, 
		CStringArray& arMsgs, 
		TableCell* pTesterCell, 
		FieldRect* pTesterField, 
		int nPage, 
		int nA, 
		int nRow/* = -1*/
	) 
{ 
	RDEData* pTesterRde = NULL;
	ASSERT((pTesterCell == NULL && pTesterField != NULL) || (pTesterCell != NULL && pTesterField == NULL));
	if (pTesterCell)
	{
		if (!pTesterCell->GetRDEValue()->IsValid() || !pTesterCell->GetRDEValue()->GetData())
		{
			pTesterRde = NULL;
		}
		else 
		{
			pTesterRde = pTesterCell->GetRDEValue();
		}
	}
	else if (pTesterField)
	{
		if (!pTesterField->GetRDEValue()->IsValid() || !pTesterField->GetRDEValue()->GetData())
		{
			pTesterRde = NULL;
		}
		else
		{
			pTesterRde = pTesterField->GetRDEValue();
		}
	}

	WoormField* sfTester =  pTesterWDoc->m_ViewSymbolTable.GetFieldByID(nA);
	if (!sfTester)
	{
		ASSERT (FALSE);
		arMsgs.Add(cwsprintf(_TB("Tested report has internal error, unresolve column/field with ID=%d. tested Report has not value"), nA));
		return FALSE;
	}
	CString sTesterName = sfTester->GetName();

	WoormField* sf =  this->m_ViewSymbolTable.GetFieldByID(nA);
	if (!sf)
	{
		arMsgs.Add(cwsprintf(_TB("Tested report has not column/field with Name=%s, ID=%d. tested Report has not value"), sTesterName, nA));
		return FALSE;
	}
	CString sName = sf->GetName();

	//check equal datatype
	if (sfTester->GetDataType() != sf->GetDataType())
	{
		arMsgs.Add(cwsprintf(_TB("The reports are different: they have different data-type for column/field with Name=%s, ID=%d. Checked Report has not value"), sTesterName, nA));
		return FALSE;
	}

	//check field/column's value are equal
	BOOL bEqual = FALSE;
	BOOL bCompatibleType = FALSE;

	RDEData* pRde = this->GetRDEDataFromAlias(nA, nRow);

	if (pRde != NULL && pTesterRde == NULL && (sf->IsLowerLimit() || sf->IsUpperLimit()))
	{
		DataObj* pData = DataObj::DataObjCreate(sf->GetDataType());
		pData->Assign(*pRde);

		if (pData->IsKindOf(RUNTIME_CLASS(DataStr)))
		{
			//TODO: la stringa risulta sempre vuota
			bEqual = pData->Str().IsEmpty();

			//if (sf->m_bLowerLimit)
			//	bEqual = pData->GetString()->IsEmpty();
			//else //if (sf->m_bUpperLimit)
			//	bEqual = pData->GetString() == AfxGetCultureInfo()->PadUpperLimitString(nAllocSize);
		}
		else
		{
			if (sf->IsLowerLimit())
				bEqual = pData->IsLowerValue();
			else //if (sf->IsUpperLimit())
				bEqual = pData->IsUpperValue();
		}
		if (!bEqual)
			arMsgs.Add(cwsprintf(_TB("Tester report has not a value for column/field with Name=%s, ID=%d, page=%d, row=%d. Checker Report has not value"), sTesterName, nA, nPage, nRow));
		delete pData;
	}
	else if (pTesterRde == NULL && pRde == NULL)
	{
		bEqual = TRUE;
	}
	else if (pTesterRde == NULL)
	{
		arMsgs.Add(cwsprintf(_TB("Tester report has not a value for column/field with Name=%s, ID=%d, page=%d, row=%d. Checker Report has not value"), sTesterName, nA, nPage, nRow));
	}
	else if (pRde == NULL)
	{
		arMsgs.Add(cwsprintf(_TB("Tested report has not a value for column/field with Name=%s, ID=%d, page=%d, row=%d. Checked Report has not value"), sTesterName, nA, nPage, nRow));
	}
	else
	{
		bCompatibleType = 
			DataType::IsCompatible(sfTester->GetDataType(), sf->GetDataType()) 
			||
			DataType::IsCompatible(sf->GetDataType(),		sfTester->GetDataType());
		if (bCompatibleType)
		{
			DataObj* pTesterData = DataObj::DataObjCreate(sfTester->GetDataType());
			pTesterData->Assign(*pTesterRde);

			DataObj* pData = DataObj::DataObjCreate(sf->GetDataType());
			pData->Assign(*pRde);

			if (pData->IsKindOf(RUNTIME_CLASS(DataStr)))
			{
				ASSERT_KINDOF(DataStr, pTesterData);
				//DataStr::Trim() ritorna il valore Trimmato ma non modifica il dato interno
				bEqual = (((DataStr*)pData)->Trim() == ((DataStr*)pTesterData)->Trim());
			}
			else 
				bEqual = (*pTesterData == *pData);

			delete pTesterData;
			delete pData;
		}
		else
			bEqual = FALSE;
	}

	//mark wrong object
	if (!bEqual && pRde && pTesterRde)
	{
		FieldRect* pField = NULL;
		TableCell* pCell = NULL;

		BOOL bOk = this->GetObjectFromAlias (nA, nRow, pField, pCell);
		ASSERT(bOk);
		ASSERT(
			(pTesterCell == NULL && pCell == NULL && pTesterField != NULL && pField != NULL)
			||
			(pTesterCell != NULL && pCell != NULL && pTesterField == NULL && pField == NULL)
			);

		if (pTesterCell)
		{
			pCell->m_arWrongOnPage.Add(nPage);
			pCell->m_arTooltip.SetAtGrow(nPage, pTesterCell->GetText());
			if (!bCompatibleType)
				arMsgs.Add(cwsprintf(_TB("The reports have a different type in column cell with Name=%s, ID=%d, page=%d. Checked has type [%s] instead the checker has type [%s]"), sTesterName, nA, nPage, FromDataTypeToDescr(sf->GetDataType()), FromDataTypeToDescr(sfTester->GetDataType())));
			else
				arMsgs.Add(cwsprintf(_TB("The reports have a different value in column cell with Name=%s, ID=%d, page=%d, row=%d. Checked has [%s] instead the checker has [%s]"), sTesterName, nA, nPage, nRow, pCell->GetText(), pTesterCell->GetText()));
		}
		else if (pTesterField)
		{
			pField->m_arWrongOnPage.Add(nPage);
			pField->m_arTooltip.SetAtGrow(nPage, pTesterField->GetText());
			if (!bCompatibleType)
				arMsgs.Add(cwsprintf(_TB("The reports have a different type in field with Name=%s, ID=%d, page=%d. Checked has type [%s] instead the checker has type [%s]"), sTesterName, nA, nPage, FromDataTypeToDescr(sf->GetDataType()), FromDataTypeToDescr(sfTester->GetDataType())));
			else
				arMsgs.Add(cwsprintf(_TB("The reports have a different value in field with Name=%s, ID=%d, page=%d. Checked has [%s] instead the checker has [%s]"), sTesterName, nA, nPage, pField->GetText(), pTesterField->GetText()));
		}
	}

	return bEqual;
}

//////////////////////////////////////////////////////////////////////////////
WebCommandType CWoormDocMng::OnGetWebCommandType(UINT commandID)
{
	if (commandID == ID_EXPORT_OPENOFFICE_ODT ||
		commandID == ID_EXPORT_WORD_NET ||
		commandID == ID_EXPORT_XML ||
		commandID == ID_EXPORT_OPENOFFICE_ODS ||
		commandID == ID_ALLOW_EDITING ||
		commandID == ID_EXPORT_EXCELNET ||
		commandID == ID_EXPORT_TEXT ||
		commandID == ID_EXPORT_HTML ||
		commandID == ID_EXPORT_JSON ||
		commandID == ID_EXPORT_OPENXML_EXCEL ||
		commandID == ID_ARCHIVE_PDF_FORMAT||
		commandID == ID_EXPORT_XML_FULL)				//archiviazione del report via EasyAttachments
		return WEB_UNSUPPORTED;

	if (commandID == ID_SEND_MAIL)
	{
		//Disabilito l'invio mail da Web solo se sto usando MAPI(potrebbe apriore finestre sul server)
		MailConnectorParams* params = AfxGetIMailConnector()->GetParams();
		if (params->GetUseMapi())
			return WEB_UNSUPPORTED;
	}
	return __super::OnGetWebCommandType(commandID);
}

//////////////////////////////////////////////////////////////////////////////

//-----------------------------------------------------------------------------
void CWoormDocMng::OnToggleConcatPdf ()
{
	m_bDirectConcatPDF = !m_bDirectConcatPDF;
}

//----------------------------------------------------------------------------
CString CWoormDocMng::GetUICulture()
{
	if (m_pWoormInfo)
		return m_pWoormInfo->m_sUICulture;

	return __super::GetUICulture();
}

CString CWoormDocMng::SetUICulture(const CString& sCulture)
{
	if (m_pWoormInfo)
	{
		CString s = m_pWoormInfo->m_sUICulture; 
		m_pWoormInfo->m_sUICulture = sCulture.IsEmpty() ? _T("en") : sCulture;
		return s;
	}

	return __super::SetUICulture(sCulture);
}

void CWoormDocMng::SetCustomUICulture()
{
	if (m_pWoormInfo)
	{
		if (!m_bCultureVariable)
			m_bCultureVariable = m_sStartUICulture.CompareNoCase(m_pWoormInfo->m_sUICulture) != 0;

		//__super::SetUICulture(m_pWoormInfo->m_sUICulture);

		SetCultureStrings(m_pWoormInfo->m_sUICulture);
	}
}

void CWoormDocMng::ResetCustomUICulture()
{
	if (m_pWoormInfo)
	{
		//__super::SetUICulture(m_sStartUICulture);

		SetCultureStrings(m_sStartUICulture);
	}
}

//-----------------------------------------------------------------------------
void CWoormDocMng::SetCultureStrings(const CString& sCulture) 
{
	CString sPrevCulture = AfxGetThreadContext()->SetUICulture(sCulture);

	m_sCulture_FIRST = CParsedCtrl::Strings::FIRST();
	m_sCulture_LAST = CParsedCtrl::Strings::LAST();

	AfxGetThreadContext()->SetUICulture(sPrevCulture);
}

//////////////////////////////////////////////////////////////////////////////

//------------------------------------------------------------------------------
BOOL CWoormDocMng::BringToFrontPopup (BaseObj* pCurrent, CMenu& mainMenu)
{
	m_arBringToFrontObjects.RemoveAll();

	CMenu   menu;

	BOOL bExists = FALSE;
	int ub = GetObjects().GetUpperBound();
	UINT id = ID_BRING_TO_FRONT_0;
	for (int i = 0; i <= ub && id <= ID_BRING_TO_FRONT_4; i++)
	{
		BaseObj* pObj = GetObjects()[i];
		if (pObj != pCurrent && pObj->InMe(m_ptCurrPos))
		{
			CString sTitle = pObj->GetDescription();
			if (!sTitle.IsEmpty())
			{
				if (!bExists)
				{
					menu.CreatePopupMenu();
					bExists = TRUE;
				}

				m_arBringToFrontObjects.Add(pObj);
				menu.AppendMenu(MF_STRING, id++, sTitle);
			}
		}
	}
	//----

	BOOL bExists2 = FALSE;
	CRect rectCurrent (pCurrent->GetBaseRect());
	CRect rcInt;
	for (int i = 0; i <= ub ; i++)
	{
		BaseObj* pObj = GetObjects()[i];

		CRect rect (pObj->GetBaseRect());
		if (rcInt.IntersectRect(rectCurrent, rect))
		{
			bExists2 = TRUE;
			break;
		}
	}
	//----

	if (bExists || bExists2)
	{
		if (bExists)
			mainMenu.AppendMenu(MF_STRING | MF_POPUP, (UINT) menu.m_hMenu, _TB("Bring to front"));
		if (bExists2)
			mainMenu.AppendMenu(MF_STRING , ID_SEND_TO_BACK, _TB("Send to back"));

		return TRUE;
	}

	return FALSE;
}

//------------------------------------------------------------------------------
void CWoormDocMng::OnBringToFront (UINT nID)
{
	int idx = nID - ID_BRING_TO_FRONT_0;
	BaseObj* pObj = (BaseObj*) m_arBringToFrontObjects[idx];

	SetCurrentObj(pObj, m_ptCurrPos);

	CWoormView* pView = GetWoormView();
	CClientDC dc(pView);
	pView->OnPrepareDC(&dc);
	CPoint ptMouse (m_ptCurrPos);
	dc.LPtoDP(&ptMouse);

	if (pObj->IsKindOf(RUNTIME_CLASS(BaseRect)))
	{
		CRect rectObj (pObj->GetBaseRect());
		ptMouse.x += rectObj.Width() / 2;
		ptMouse.y += rectObj.Height() / 2;
	}

	pView->PostMessageW(WM_RBUTTONDOWN, 0,  MAKELONG(ptMouse.x, ptMouse.y));

	pView->ClientToScreen(&ptMouse);

	::SetCursorPos(ptMouse.x, ptMouse.y);
}

//------------------------------------------------------------------------------
void CWoormDocMng::OnSendToBack (BaseObj* pCurrent)
{
	int ub = GetObjects().GetUpperBound();
	for (int i = 0; i <= ub ; i++)
	{
		BaseObj* pObj = GetObjects()[i];

		if (pObj == pCurrent)
		{
			GetObjects().Swap(0, i);
			break;
		}
	}
	//scambiare con il primo elemnto intersecato
	//selezionare il nuovo elemento
	m_pActiveRect->Clear();
	SetModifiedFlag();

	Invalidate();
	UpdateWindow();
}

//-----------------------------------------------------------------------------
void CWoormDocMng::AddViewField(WoormField* pF, BOOL /*bDropDown = TRUE*/)
{
	m_ViewSymbolTable.Add(pF);
}

//-----------------------------------------------------------------------------
void CWoormDocMng::RemoveViewField(const CString& strName)
{
	m_ViewSymbolTable.DelField(strName);
}

//////////////////////////////////////////////////////////////////////////////

void CWoormDocMng::OnOpenAsText()
{
	if (!CanDoEditReport())
		return;

	if (!GetWoormFrame()->ShowNewEditor())
	{
		try
		{
			HKEY hKey = 0;
			long lRetCode = ::RegOpenKeyEx
				(
				HKEY_CURRENT_USER,
				_T("Software\\Classes\\.wrm"),
				0, KEY_READ, &hKey
				);
			if (hKey) RegCloseKey(hKey);
			if (lRetCode == ERROR_SUCCESS)
			{
				HINSTANCE hInst = ::TBShellExecute(m_strPathName);
				if (int(hInst) > 32)
					return;	//SUCCESS
			}
		}
		catch (...)
		{
			//resume next ;-)
		}
		::SpawnProgramExecution(_T("notepad.exe"), m_strPathName, FALSE);
	}
	else
	{
		OpenEditViewFullText();
	}
}

void CWoormDocMng::OnUpdateOpenAsText (CCmdUI* pCmdUI)
{
	pCmdUI->Enable(CanSaveReport());
}

//////////////////////////////////////////////////////////////////////////////

//------------------------------------------------------------------------------
void CWoormDocMng::OnUpdateLoadTemplate(CCmdUI* pCmdUI)
{
	pCmdUI->Enable(
		!m_bPlayback  && 
		m_Creating == NONE &&
		!m_bEngineRunning
		);
}

void CWoormDocMng::OnUpdateUnloadTemplate(CCmdUI* pCmdUI)
{
	pCmdUI->Enable(m_Template.m_pWoormTpl != NULL/* && CanSaveReport()*/);
}

//-----------------------------------------------------------------------------
void CWoormDocMng::OnUnloadTemplate ()
{
	UnloadTemplate ();

	Invalidate();
	UpdateWindow();
}

//-----------------------------------------------------------------------------
void CWoormDocMng::UnloadTemplate ()
{
	if (!m_Template.m_pWoormTpl) 
		return;

	if (m_pOptions->m_strBkgnBitmap == m_Template.m_pWoormTpl->m_pOptions->m_strBkgnBitmap)
	{
		m_pOptions->m_strBkgnBitmap.Empty();
		m_pOptions->m_BitmapOrigin.SetPoint(0,0);
	}

	m_Layouts.EnumObjects(&CWoormDocMng::RemoveInheritObjects);

	RemoveTemplateStyles();

	UnloadTemplateFontStyles();

	m_Template.Unload();

	//----
	m_Layouts.EnumObjects(&CWoormDocMng::ApplyRepeater);
}

//-----------------------------------------------------------------------------
void CWoormDocMng::RemoveTemplateStyles (CLayout* pObjects)
{
	Table* pTable = NULL;
	for (int i = pObjects->GetUpperBound(); i >= 0 ; i--)
	{
		BaseObj* pObj = (*pObjects)[i];

		if (pObj->IsKindOf(RUNTIME_CLASS(Table)))
		{
			pTable = (Table*)pObj;

			pTable->RemoveAllStyles();
		}
		else if (pObj->IsKindOf(RUNTIME_CLASS(BaseRect)))
		{
			BaseRect* pF = (FieldRect*) pObj;

			if (pF->m_sStyleClass.CompareNoCase(m_Template.s_sCopyStaticObject_StyleName) == 0)
			{
				pObjects->RemoveAt(i);	//delete pF; //NO: Automatico nella RenoveAt reimplementata 
			}
			else
			{
				pF->RemoveStyle();
			}
		}
	}
}

//-----------------------------------------------------------------------------
void CWoormDocMng::ClearCustomStyles (CLayout* pObjects)
{
	Table* pTable = NULL;
	for (int i = pObjects->GetUpperBound(); i >= 0 ; i--)
	{
		BaseObj* pObj = (*pObjects)[i];

		if (pObj->IsKindOf(RUNTIME_CLASS(Table)))
		{
			pTable = (Table*)pObj;

			pTable->ClearAllStyles();
		}
		else if (pObj->IsKindOf(RUNTIME_CLASS(BaseRect)))
		{
			BaseRect* pF = (FieldRect*) pObj;

			pF->ClearStyle();
		}
	}
}

//-----------------------------------------------------------------------------
void CWoormDocMng::RemoveTemplateStyles ()
{
	if (! m_Template.m_pWoormTpl) return;

	m_Layouts.EnumObjects(&CWoormDocMng::RemoveTemplateStyles);
}

//-----------------------------------------------------------------------------
void CWoormDocMng::OnClearCustomStyles ()
{
	if (AfxMessageBox(_TB("Are you sure to remove all graphical custom attributes from all report's objects ?"), MB_OKCANCEL | MB_DEFBUTTON2 | MB_ICONWARNING) == IDCANCEL)
		return;

	m_Layouts.EnumObjects(&CWoormDocMng::ClearCustomStyles);

	Invalidate();
	UpdateWindow();
}

//------------------------------------------------------------------------------
void CWoormDocMng::OnUpdateClearCustomStyles(CCmdUI* pCmdUI)
{
	pCmdUI->Enable(CanSaveReport());
}

//=============================================================================

CString	CWoormTemplate::s_sCustom_StyleName					(_T("<Custom>"));
CString	CWoormTemplate::s_sCopyStaticObject_StyleName		(_T("_CopyObject_"));
CString	CWoormTemplate::s_sCopiedStaticObject_StyleName		(_T("_CopiedObject_"));
CString	CWoormTemplate::s_sInheritStaticObject_StyleName	(_T("_InheritObject_"));
CString	CWoormTemplate::s_sEA_BarCode_StyleName				(_T("EA_BarCode"));

void CWoormDocMng::OnLoadTemplate ()
{
	CString nsm;
	if (AfxIsActivated(MAGONET_APP, _NS_ACT("Core")))
		nsm = _NS_MOD("Module.Erp.Core");
	else if (AfxIsActivated(OFM_APP, _NS_ACT("Core")))
		nsm = _NS_MOD("Module.Ofm.Core");

	CTBNamespace nsCore(nsm);
	nsCore.SetType(CTBNamespace::REPORT);
	ITBExplorer* pExplorer = AfxCreateTBExplorer(ITBExplorer::OPEN, nsCore);
	
	if (!pExplorer->Open())
	{
		delete pExplorer;
		return;
	}

	UnloadTemplate ();
		
	CStringArray aSelectedPaths;
	pExplorer->GetSelPathElements(&aSelectedPaths);
	
	CString sPathTemplate = aSelectedPaths.GetAt(0);
	if (! ::ExistFile(sPathTemplate))
	{
		//TODO TEMPLATE errors
		return;
	}

	CTBNamespace ns = AfxGetPathFinder()->GetNamespaceFromPath(sPathTemplate);
	m_Template.m_sNsTemplate = ns.ToString();

	if (pExplorer)
		delete pExplorer;

	LoadTemplate (TRUE);
}

void CWoormDocMng::LoadTemplate() 
{
	LoadTemplate(FALSE); 	
}

//-----------------------------------------------------------------------------
BOOL CWoormDocMng::LoadTemplate (BOOL bEditing, BOOL bShowDialog, BOOL bImportStaticObjects, BOOL bUpdateDoc /*= TRUE*/)
{
	/*if (running)
		return TRUE;*/
	m_bLoadingTemplate = TRUE;
	if (m_Template.m_sNsTemplate.IsEmpty())
		return FALSE;

	m_Template.m_pWoormTpl = new CWoormDocMng();
	BOOL bOk = m_Template.m_pWoormTpl->OpenReportTemplate(m_Template.m_sNsTemplate);
	if (!bOk)
	{
		//TODO TEMPLATE errors
		m_Template.Unload();
		return FALSE;
	}
	m_Template.m_bLoaded = TRUE;
	
	ApplyTemplateStyles (bEditing);

	if (bShowDialog)
	{
		//----
		if (bEditing && AfxMessageBox(_TB("Do you want to import template static fields ?"), MB_YESNO) == IDYES)
			m_Layouts.EnumObjects(&CWoormDocMng::AddAllStaticObjects);
		else
			m_Layouts.EnumObjects(&CWoormDocMng::AddInheritObjects);

		//----
	}
	else
	{
		if (bImportStaticObjects) 
			m_Layouts.EnumObjects(&CWoormDocMng::AddAllStaticObjects);
		else 
			m_Layouts.EnumObjects(&CWoormDocMng::AddInheritObjects);
	}
	m_Layouts.EnumObjects(&CWoormDocMng::ApplyRepeater);
	//----
	if (bEditing)
		GetWoormFrame()->ShowDDLTemplates();

	if (bUpdateDoc)
	{
		Invalidate();
		//UpdateWindow();
	}
	m_bLoadingTemplate = FALSE;
	return TRUE;
}

//-----------------------------------------------------------------------------
void CWoormDocMng::AddAllStaticObjects (CLayout* pObjects)
{
	AddStaticObjects (pObjects, FALSE);
}

//-----------------------------------------------------------------------------
void CWoormDocMng::AddInheritObjects (CLayout* pObjects)
{
	AddStaticObjects (pObjects, TRUE);
}

//-----------------------------------------------------------------------------
void CWoormDocMng::AddStaticObjects (CLayout* pObjects, BOOL bOnlyInherit)
{
	if (!m_Template.m_pWoormTpl) 
		return;

	CMultiLayouts* pTplLayouts = &(m_Template.m_pWoormTpl->m_Layouts);

	CString strName; CLayout* pTplObjects = NULL;
	for (POSITION pos = pTplLayouts->GetStartPosition(); pos != NULL; pTplObjects = NULL, strName.Empty())
	{
		pTplLayouts->GetNextAssoc (pos, strName, (CObject*&)pTplObjects);
		int idx = 0;
		for (int i = 0; i <= pTplObjects->GetUpperBound(); i++)
		{
			BaseObj* pTblObj = (*pTplObjects)[i];

			if (pTblObj->IsKindOf(RUNTIME_CLASS(BaseRect)))
			{
				BaseRect* pTblF = (BaseRect*) pTblObj;
				if (!pTblF->m_bTemplate)
					continue;

				if 
					(
						(
							pTblF->m_sStyleClass.CompareNoCase(m_Template.s_sCopyStaticObject_StyleName) == 0
							&&
							!bOnlyInherit
						) 
						||
						pTblF->m_sStyleClass.CompareNoCase(m_Template.s_sInheritStaticObject_StyleName) == 0
					)
				{
					BaseRect* pNewObj = NULL;

					if (pTblF->IsKindOf(RUNTIME_CLASS(FileRect)))
						pNewObj = new FileRect (*(FileRect*)pTblF);
					else if (pTblF->IsKindOf(RUNTIME_CLASS(TextRect)))
						pNewObj = new TextRect (*(TextRect*)pTblF);
					else if (pTblF->IsKindOf(RUNTIME_CLASS(GraphRect)))
						pNewObj = new GraphRect (*(GraphRect*)pTblF);
					else if (pTblF->IsKindOf(RUNTIME_CLASS(SqrRect)))
						pNewObj = new SqrRect (*(SqrRect*)pTblF);

					if (pNewObj)
					{
						if (pTblF->m_sStyleClass.CompareNoCase(m_Template.s_sCopyStaticObject_StyleName) == 0)
							pNewObj->m_sStyleClass = CWoormTemplate::s_sCopiedStaticObject_StyleName;
						else
							pNewObj->m_bInheritByTemplate = TRUE;

						pNewObj->Create(this);

						pObjects->InsertAt(idx++, pNewObj);
					}
				}
			}
		}
	}
}

//-----------------------------------------------------------------------------
void CWoormDocMng::RemoveInheritObjects (CLayout* pObjects)
{
	if (m_pMultipleSelObj) OnDeselectAll();
	m_pActiveRect->Clear();
	m_pCurrentObj = NULL;

	for (int i = pObjects->GetUpperBound(); i  >= 0; i--)
	{
		BaseObj* pObj = (*pObjects)[i];

		if (pObj->IsKindOf(RUNTIME_CLASS(BaseRect)))
		{
			BaseRect* pBr = (BaseRect*) pObj;
			if (!pBr->m_bInheritByTemplate)
				continue;

			pBr->DeleteEditorEntry();

			pObjects->RemoveAt(i);
		}
	}
}

//-----------------------------------------------------------------------------
void CWoormDocMng::ApplyTemplateStyles (BOOL /*bEditing = FALSE*/)
{
	if (!m_Template.m_pWoormTpl) return;

	LoadTemplateFontStyles();

	ASSERT(m_pOptions && m_Template.m_pWoormTpl->m_pOptions);
	if (m_pOptions->m_strBkgnBitmap.IsEmpty())
	{
		m_pOptions->m_strBkgnBitmap = m_Template.m_pWoormTpl->m_pOptions->m_strBkgnBitmap;
		m_pOptions->m_BitmapOrigin = m_Template.m_pWoormTpl->m_pOptions->m_BitmapOrigin;
	}

	m_Layouts.EnumObjects(&CWoormDocMng::ApplyTemplateStyles);
}

//-----------------------------------------------------------------------------
void CWoormDocMng::ApplyTemplateStyles (CLayout* pObjects)
{
	for (int i = 0; i <= pObjects->GetUpperBound(); i++)
	{
		BaseObj* pO = (*pObjects)[i];

		ApplyTemplateStyles(pO);
	}
}

//-----------------------------------------------------------------------------
void CWoormDocMng::ApplyTemplateStyles (BaseObj* pO)
{
	SetTemplateStyle(pO);

	if (pO->IsKindOf(RUNTIME_CLASS(Table)))
	{
		Table *pTable = (Table*) pO;

		for (int c = 0; c <= pTable->m_Columns.GetUpperBound(); c++)
		{
			TableColumn* pCol = pTable->m_Columns[c];

			SetTemplateStyle(pCol);
		}
	}
}

//-----------------------------------------------------------------------------
BOOL CWoormDocMng::SetTemplateStyle(CObject* pCurrObj)
{
	if (!m_Template.m_pWoormTpl) return FALSE;

	BaseRect* pRect = dynamic_cast<BaseRect*> (pCurrObj);
	if (pRect && pRect->m_sStyleClass.CompareNoCase(CWoormTemplate::s_sCustom_StyleName) == 0)
		return TRUE;

	Table* pTable = dynamic_cast<Table*> (pCurrObj);
	if (pTable && pTable->m_sStyleClass.CompareNoCase(CWoormTemplate::s_sCustom_StyleName) == 0)
		return TRUE;

	TableColumn* pCol = dynamic_cast<TableColumn*> (pCurrObj);
	if (pCol && pCol->m_sStyleClass.CompareNoCase(CWoormTemplate::s_sCustom_StyleName) == 0)
		return TRUE;

	CMultiLayouts* pTplLayouts = &(m_Template.m_pWoormTpl->m_Layouts);

	CString strName; CLayout* pTplObjects = NULL;
	for (POSITION pos = pTplLayouts->GetStartPosition(); pos != NULL; pTplObjects = NULL, strName.Empty())
	{
		 pTplLayouts->GetNextAssoc (pos, strName, (CObject*&)pTplObjects);

		for (int i = 0; i <= pTplObjects->GetUpperBound(); i++)
		{
			BaseObj* pTblObj = (*pTplObjects)[i];

			if (pCurrObj->IsKindOf(RUNTIME_CLASS(BaseRect)) && pTblObj->IsKindOf(RUNTIME_CLASS(BaseRect)))
			{
				BaseRect* pTblF = (BaseRect*) pTblObj;

				if (!pTblF->m_bTemplate)
					continue;

				if (pRect->GetRuntimeClass() != pTblF->GetRuntimeClass())
					continue;

				if (pRect->m_sStyleClass.CompareNoCase(pTblF->m_sStyleClass))
					continue;
			
				pRect->SetStyle(pTblF);

				return TRUE;
			}
			else if (pCurrObj->IsKindOf(RUNTIME_CLASS(Table)) && pTblObj->IsKindOf(RUNTIME_CLASS(Table)))
			{
				Table* pTplTable = (Table*) pTblObj;

				if (!pTplTable->m_bTemplate)
					continue;

				if (pTable->m_sStyleClass.CompareNoCase(pTplTable->m_sStyleClass))
					continue;

				pTable->SetStyle(pTplTable);

				return TRUE;
			}
			else if (pCurrObj->IsKindOf(RUNTIME_CLASS(TableColumn)) && pTblObj->IsKindOf(RUNTIME_CLASS(Table)))
			{
				Table* pTable = (Table*) pTblObj;

				for (int c = 0; c <= pTable->m_Columns.GetUpperBound(); c++)
				{
					TableColumn* pTblCol = pTable->m_Columns[c];

					if (!pTblCol->m_bTemplate)
						continue;
					if (pCol->m_sStyleClass.CompareNoCase(pTblCol->m_sStyleClass))
						continue;

					pCol->SetStyle(pTblCol);

					return TRUE;
				}
			}
		}
	}
/*	TODO RICCARDO TEMPLATE mancano dei wrmt di ERP alcuni oggetti template
	//non ho trovato lo stile nel template, ripiego sullo stile <default> 
	if (pRect && !pRect->m_sStyleClass.IsEmpty())
	{
		CString sSaveStyle = pRect->m_sStyleClass;
		pRect->m_sStyleClass.Empty();
		BOOL bOk = SetTemplateStyle(pCurrObj);
		pRect->m_sStyleClass = sSaveStyle;
		return bOk;
	}
	if (pTable && !pTable->m_sStyleClass.IsEmpty())
	{
		CString sSaveStyle = pTable->m_sStyleClass;
		pTable->m_sStyleClass.Empty();
		BOOL bOk = SetTemplateStyle(pCurrObj);
		pTable->m_sStyleClass = sSaveStyle;
		return bOk;
	}
	if (pCol && !pCol->m_sStyleClass.IsEmpty())
	{
		CString sSaveStyle = pCol->m_sStyleClass;
		pCol->m_sStyleClass.Empty();
		BOOL bOk = SetTemplateStyle(pCurrObj);
		pCol->m_sStyleClass = sSaveStyle;
		return bOk;
	}
*/
	return FALSE;
}

//-----------------------------------------------------------------------------
void CWoormDocMng::LoadTemplateFontStyles		()
{
	if (!m_Template.m_pWoormTpl) return;
//----
	FontStyleTablePtr FT_src(m_Template.m_pWoormTpl->m_pFontStyles, FALSE);
	FontStyleTablePtr FT_current(m_pFontStyles, FALSE);
	for (int j=0; j <= FT_src->GetUpperBound(); j++)
	{
		FontStylesGroup* pGroup = FT_src->GetAt(j);
		if (!pGroup)
			continue;

		for (int i = pGroup->GetFontStyles().GetUpperBound(); i >= 0; i--)
		{
			FontStyle* pStyle = (FontStyle*) pGroup->GetFontStyles().GetAt(i);
			if (pStyle->GetSource() == FontStyle::FROM_WOORM) 
			{
				FontStyle* pTplStyle = new FontStyle(*pStyle);
				pTplStyle->SetSource(FontStyle::FROM_WOORM_TEMPLATE);

				FT_current->AddFont(pTplStyle);
			}
		}
	}
}

//-----------------------------------------------------------------------------
void CWoormDocMng::UnloadTemplateFontStyles	()
{
	if (!m_Template.m_pWoormTpl) return;
	//----
	FontStyleTablePtr FT(m_pFontStyles, FALSE);
	for (int j=0; j <= FT->GetUpperBound(); j++)
	{
		FontStylesGroup* pGroup = FT->GetAt(j);
		if (!pGroup)
			continue;

		for (int i = pGroup->GetFontStyles().GetUpperBound(); i >= 0; i--)
		{
			FontStyle* pStyle = (FontStyle*) pGroup->GetFontStyles().GetAt(i);
			if (pStyle->GetSource() == FontStyle::FROM_WOORM_TEMPLATE) 
			{
				const_cast<Array&>(pGroup->GetFontStyles()).RemoveAt(i);
			}
		}
	}
}

//=============================================================================
void CWoormDocMng::FillComboTemplates (UINT nID, LPCTSTR sNs)
{
	CTBNamespace aNameSpace(sNs);	//TODO leggere dai setting
	aNameSpace.SetType(CTBNamespace::REPORT);

	CStringArray aUserSel; aUserSel.Add(AfxGetLoginInfos()->m_strUserName);
	CStringArray aAllModulesPath;
	CStringArray aAllObjPaths;
	AfxGetPathFinder()->GetAllModulePath(aAllModulesPath, aNameSpace, aUserSel, TRUE, TRUE, TRUE, TRUE, aNameSpace.GetObjectName(CTBNamespace::MODULE));	
	AfxGetPathFinder()->GetAllObjInFolder(aAllObjPaths, aNameSpace, aAllModulesPath, _T("*"), _T("*.wrmt"));
	
	CTBToolBar* pToolBar = GetWoormFrame()->GetToolBar(NAME_MAIN_EDIT_TOOLBAR);
	ASSERT(pToolBar);

	for (int s = 0; s <= aAllObjPaths.GetUpperBound(); s++)
	{
		CString strFullName = aAllObjPaths.GetAt(s);
		CString sSubObjName = ::GetName(strFullName);
		
		if (!::IsValidObjName(sSubObjName))
			continue;// invalid character into object name filter
		if (!::PathFileExists(strFullName))
			continue;

		pToolBar->AddComboItem(nID, sSubObjName, GetWoormFrame()->m_cbxTemplatesStr.AddEntry(strFullName) );
	}	
}

BOOL CWoormDocMng::FillComboTemplates()
{
	GetWoormFrame()->m_cbxTemplatesStr.ResetContent();
	CTBToolBar* pToolBar = GetWoormFrame()->GetToolBar(NAME_MAIN_EDIT_TOOLBAR);
	ASSERT(pToolBar);
	if (!pToolBar) return FALSE;
	int nIdCb = IDC_DROPDOWN_TEMPLATES;
	pToolBar->RemoveAllComboItems(nIdCb);
	pToolBar->AddComboItem(nIdCb, cwsprintf(_T(" <%s>"),	_TB("Select a Template")),		0);
	
	CString sCurrent, sPath;
	if (m_Template.m_pWoormTpl)
	{
		sPath = AfxGetPathFinder()->GetFileNameFromNamespace(m_Template.m_pWoormTpl->GetNamespace(), AfxGetLoginInfos()->m_strUserName);
		sCurrent = ::GetName(sPath);
	}

	if (
			!sPath.IsEmpty() 
			&&
			(
				(
					AfxIsActivated(MAGONET_APP, _NS_ACT("Core")) 
					&&
					(
						m_Template.m_pWoormTpl->GetNamespace().GetApplicationName().CompareNoCase(MAGONET_APP) ||
						m_Template.m_pWoormTpl->GetNamespace().GetModuleName().CompareNoCase(_NS_MOD("Core"))
					)
				)
				||
				(
					AfxIsActivated(OFM_APP, _NS_ACT("Core")) 
					&&
					(
						m_Template.m_pWoormTpl->GetNamespace().GetApplicationName().CompareNoCase(OFM_APP) ||
						m_Template.m_pWoormTpl->GetNamespace().GetModuleName().CompareNoCase(_NS_MOD("Core"))
					)
				)
			)
		)	
	{
		pToolBar->AddComboItem(nIdCb, sCurrent, GetWoormFrame()->m_cbxTemplatesStr.AddEntry(sPath) );
	}
	else
	{
		if (AfxIsActivated(MAGONET_APP, _NS_ACT("Core")))
			FillComboTemplates(nIdCb, _NS_MOD("Module.Erp.Core"));
		else if (AfxIsActivated(OFM_APP, _NS_ACT("Core")))
			FillComboTemplates(nIdCb, _NS_MOD("Module.Ofm.Core"));
	}

	pToolBar->AddComboItem(nIdCb, cwsprintf(_T("[%s]"), _TB("Unload Template")), -1);
	pToolBar->AddComboItem(nIdCb, cwsprintf(_T("[%s...]"), _TB("Browse for Templates")), -2);

	int curr = pToolBar->FindComboStringExact(nIdCb, sCurrent);
	pToolBar->SetComboItemSel(nIdCb,curr);
	return TRUE;
}

//-----------------------------------------------------------------------------
void CWoormDocMng::OnComboTemplatesSelChanged()
{
	if 
		(
			m_bPlayback ||
			m_bEngineRunning
		)
	{
		return;
	}
	
	CTBToolBar* pToolBar = GetWoormFrame()->GetToolBar(NAME_MAIN_EDIT_TOOLBAR);
	ASSERT(pToolBar);
	int	nCurSel  = pToolBar->GetComboItemSel(IDC_DROPDOWN_TEMPLATES);
	if (nCurSel < 0) return;
	DWORD dw = pToolBar->GetComboItemData(IDC_DROPDOWN_TEMPLATES, nCurSel);

	if (dw == 0)
	{
		return;
	}
	if (dw == -1)
	{
		GetWoormFrame()->PostMessage(WM_COMMAND, ID_UNLOAD_TEMPLATE);
		return;
	}
	if (dw == -2)
	{
		GetWoormFrame()->PostMessage(WM_COMMAND, ID_LOAD_TEMPLATE);
		return;
	}

	CString strPathName = GetWoormFrame()->m_cbxTemplatesStr.m_arStringItemData.GetAt(nCurSel);

	UnloadTemplate ();

	m_Template.m_sNsTemplate = AfxGetPathFinder()->GetNamespaceFromPath(strPathName).ToString();


	GetWoormFrame()->PostMessage(WM_COMMAND, ID_OPEN_TEMPLATE);
	
}

//=============================================================================

void CWoormDocMng::ChangeLayout(const CString& sName)
	{
	// remove selections
	if (m_pMultipleSelObj)
		OnDeselectAll();              
	CPoint pt;
	SetCurrentObj(0, pt);
	m_pCurrentObj = NULL;
	m_pActiveRect->Clear();
	//----

	m_Layouts.SetCurrent(sName);
	
	Invalidate(TRUE);
	UpdateWindow();
}

//-----------------------------------------------------------------------------
void CWoormDocMng::UpdateLayout	()
{
	PrepareLayout();

	Invalidate(TRUE);
	//UpdateWindow ();
}

//-----------------------------------------------------------------------------
/*
void CWoormDocMng::OnRenameLayout ()
{
	if 
		(
			m_bPlayback ||
			m_bEngineRunning ||
			!m_bAllowEditing
		)
	{
		return;
	}

	CSetNameDlg   dialog(this, GetFirstView());

	if (dialog.DoModal() == IDOK && !dialog.m_sName.IsEmpty())
	{
		CString sPrevName (m_Objects->m_strLayoutName);

		if (!m_Layouts.Rename(sPrevName, dialog.m_sName))
			return;

		m_dsCurrentLayoutView = dialog.m_sName;

		if (m_pEditorManager)
		{
			DisplayTables* pDT = m_pEditorManager->GetDispTable ();
			ASSERT(pDT);
			pDT->LayoutRename(sPrevName, dialog.m_sName);
		}

		SetModifiedFlag();
	}
}

//-----------------------------------------------------------------------------
void CWoormDocMng::OnUpdateRenameLayout (CCmdUI* pCmdUI)
{
	pCmdUI->Enable(CanSaveReport());
}
*/
//-----------------------------------------------------------------------------
void CWoormDocMng::OnUpdateLayoutCopyMoveObject (CCmdUI* pCmdUI)
{
	pCmdUI->Enable (m_Layouts.GetCount() > 1);
}

//-----------------------------------------------------------------------------
BOOL CWoormDocMng::GetLayoutSourceObject (CLayout*& pSourceObjects, BaseObj*& pSourceObj)
{
	CChooseObjectDlg   dialog(this, GetFirstView(), TRUE);
	if (dialog.DoModal() == IDOK)
	{
		ASSERT(!dialog.m_pSelected || dialog.m_pSelected->IsKindOf(RUNTIME_CLASS(BaseRect)));
		pSourceObj = dialog.m_pSelectedBaseRect;
		if (!pSourceObj)
		{
			ASSERT(dialog.m_pSelectedTable);
			pSourceObj = dialog.m_pSelectedTable;
		}
		ASSERT(dialog.m_pSelectedLayout);
		pSourceObjects = dialog.m_pSelectedLayout;
		//----

		WORD id = pSourceObj->GetInternalID();
		if (id && GetObjects().FindByID(id))
		{
			//TODO message Object exists with same ID
			return FALSE;
		}

		return TRUE;
	}
	return FALSE;
}

//-----------------------------------------------------------------------------
void CWoormDocMng::LayoutCopyObject (CLayout* pSourceObjects, CLayout* pTargetObjects, BaseObj* pSourceObj)
{
	ASSERT_VALID(pSourceObjects);
	ASSERT_VALID(pTargetObjects);
	ASSERT_VALID(pSourceObj);

	//TODO check pSourceObj has dynamic expression with pSourceObjects
	WORD id = pSourceObj->GetInternalID();
	if (id && pTargetObjects->FindByID(id))
		return;

	BaseObj* pClone = pSourceObj->Clone();
	ASSERT(pClone);

	int nStart = pTargetObjects->Add(pClone);

	if (pSourceObj->IsKindOf(RUNTIME_CLASS(BaseRect)))
	{
		BaseRect* pB = (BaseRect*) pClone;

		pB->m_AnchorLeftColumnID = pB->m_AnchorRightColumnID = 0;
		pB->m_AnchorRepeaterID = 0;  pB->m_nRepeaterRow = -1;

		if (pSourceObj->IsKindOf(RUNTIME_CLASS(Repeater)))
		{
			Repeater* pRep = (Repeater*) pClone;
			CLayout* arChilds = pRep->GetChildObjects();

			for (int i = 0; i < arChilds->GetSize(); i++)
			{
				BaseObj* pChild = (*arChilds)[i];
				WORD id = pChild->GetInternalID();
				if (id && pTargetObjects->FindByID(id))
				{
					continue;
				}
				pTargetObjects->Add(pChild->Clone());
			}
			pRep->Rebuild(pTargetObjects);

			//----
			if (this->m_pEditorManager)
			{
				DisplayTables* pDT = m_pEditorManager->GetDispTable ();
				ASSERT(pDT);
				pDT->LayoutClone(pRep->GetInternalID(), pSourceObjects->m_strLayoutName, pTargetObjects->m_strLayoutName);
			}
			else
				ASSERT(FALSE);

		}
	}
	else if (pSourceObj->IsKindOf(RUNTIME_CLASS(Table)))
	{
		Table* pT = (Table*) pClone;

		if (this->m_pEditorManager)
		{
			DisplayTables* pDT = m_pEditorManager->GetDispTable ();
			ASSERT(pDT);
			pDT->LayoutClone(pT->GetInternalID(), pSourceObjects->m_strLayoutName, pTargetObjects->m_strLayoutName);
		}
		else
			ASSERT(FALSE);
		//----

		CArray<BaseRect*, BaseRect*>	arAnchoredFields;
		pT->GetAnchoredFields(arAnchoredFields);
		if (arAnchoredFields.GetCount())
		{
			//copy anchored fields too
			for (int i= 0; i < arAnchoredFields.GetSize(); i++)
			{
				BaseObj* pF = arAnchoredFields[i];

				WORD id = pF->GetInternalID();
				if (id && pTargetObjects->FindByID(id))
				{
					continue;
				}

				BaseObj* pClone = pF->Clone();

				pTargetObjects->Add(pClone);
			}
			AnchorFieldToColumn(pTargetObjects, nStart);
		}
	}

	//----
	RefreshRSTree(ERefreshEditor::Layouts, pClone);

	m_pCurrentObj = NULL;
	m_pActiveRect->Clear();

	SetModifiedFlag();
	Invalidate(TRUE);
	UpdateWindow ();
}

//-----------------------------------------------------------------------------
void CWoormDocMng::LayoutMoveObject (CLayout* pSourceObjects, CLayout* pTargetObjects, BaseObj* pSourceObj)
{
	ASSERT_VALID(pSourceObjects);
	ASSERT_VALID(pTargetObjects);
	ASSERT_VALID(pSourceObj);

	WORD id = pSourceObj->GetInternalID();
	if (id && pTargetObjects->FindByID(id))
		return;
	//TODO check pSourceObjects has dynamic expression with pSourceObj

	//TODO check pSourceObj has dynamic expression with pSourceObjects

	if (pSourceObj->IsKindOf(RUNTIME_CLASS(BaseRect)))
	{
		BaseRect* pB = (BaseRect*) pSourceObj;
		if (pB->IsAnchored())
			pB->FreeFieldFromColumn(pSourceObjects);
		
		pB->m_AnchorRepeaterID = 0;  pB->m_nRepeaterRow = -1;

		if (pSourceObj->IsKindOf(RUNTIME_CLASS(Repeater)))
		{
			Repeater* pRep = (Repeater*) pSourceObj;
			CLayout* arChilds = pRep->GetChildObjects();

			for (int i = 0; i < arChilds->GetSize(); i++)
			{
				BaseObj* pChild = (*arChilds)[i];
				WORD id = pChild->GetInternalID();
				if (id && pTargetObjects->FindByID(id))
				{
					continue;
				}

				pTargetObjects->Add(pChild);

				int idx = pSourceObjects->FindPtr(pChild);
				ASSERT(idx >= 0);
				if (idx >= 0)
					pSourceObjects->CObArray::RemoveAt(idx);
			}

			ASSERT_VALID(m_pEditorManager);
			if (this->m_pEditorManager)
			{
				DisplayTables* pDT = m_pEditorManager->GetDispTable ();
				ASSERT(pDT);
				VERIFY(pDT->LayoutMove(pRep->GetInternalID(), pSourceObjects->m_strLayoutName, pTargetObjects->m_strLayoutName));
			}
		}
	}

	int nStart = pTargetObjects->Add(pSourceObj);

	if (pSourceObj->IsKindOf(RUNTIME_CLASS(Repeater)))
	{
		((Repeater*)pSourceObj)->Rebuild(pTargetObjects);
	}	
	else if (pSourceObj->IsKindOf(RUNTIME_CLASS(Table)))
	{
		Table* pT = (Table*) pSourceObj;
			
		if (this->m_pEditorManager)
		{
			DisplayTables* pDT = m_pEditorManager->GetDispTable ();
			ASSERT(pDT);
			VERIFY(pDT->LayoutMove(pT->GetInternalID(), pSourceObjects->m_strLayoutName, pTargetObjects->m_strLayoutName));
		}
		else
			ASSERT(FALSE);

		CArray<BaseRect*, BaseRect*>	arAnchoredFields;
		pT->GetAnchoredFields(arAnchoredFields);
		pT->RemoveAnchoredFields();
		if (arAnchoredFields.GetCount())
		{
			//move anchored fields too
			for (int i= 0; i < arAnchoredFields.GetSize(); i++)
			{
				BaseObj* pF = arAnchoredFields[i];

				WORD id = pF->GetInternalID();
				if (id && pTargetObjects->FindByID(id))
				{
					//non posso portarlo, quindi lo sgancio dalla tabella
					BaseRect* pB = (BaseRect*) pF;
					pB->m_AnchorLeftColumnID = pB->m_AnchorRightColumnID = 0;
					continue;
				}

				pTargetObjects->Add(pF);
					
				int idx = pSourceObjects->FindPtr(pF);
				ASSERT(idx >= 0);
				if (idx >= 0)
					pSourceObjects->CObArray::RemoveAt(idx);
			}
			AnchorFieldToColumn(pTargetObjects, nStart);
		}
	}

	int idx = pSourceObjects->FindPtr(pSourceObj);
	ASSERT(idx >= 0);
	if (idx >= 0)
		pSourceObjects->CObArray::RemoveAt(idx);

	//----
	RefreshRSTree(ERefreshEditor::Layouts, m_pCurrentObj);

	m_pCurrentObj = NULL;
	m_pActiveRect->Clear();

	SetModifiedFlag();
	Invalidate(TRUE);
	UpdateWindow ();
}

//-----------------------------------------------------------------------------
void CWoormDocMng::OnLayoutCopyObject ()
{
	CLayout* pSourceObjects = NULL;
	BaseObj* pSourceObj = NULL;
	if (GetLayoutSourceObject(pSourceObjects, pSourceObj))
	{
		ASSERT(pSourceObjects && pSourceObj);

		LayoutCopyObject(pSourceObjects, &GetObjects(), pSourceObj);
	}
}

//-----------------------------------------------------------------------------
void CWoormDocMng::OnLayoutMoveObject ()
{
	CLayout* pSourceObjects = NULL;
	BaseObj* pSourceObj = NULL;
	if (GetLayoutSourceObject(pSourceObjects, pSourceObj))
	{
		ASSERT(pSourceObjects && pSourceObj);

		LayoutMoveObject(pSourceObjects, &GetObjects(), pSourceObj);
	}
}

//=============================================================================

BOOL CWoormDocMng::CanDeleteField (LPCTSTR pszFieldName, CString& sLog, BOOL* onlyUI) const
{
	BOOL bCan =  
		m_Layouts.CanDeleteField(pszFieldName, sLog) &&
		m_arWoormLinks.CanDeleteField(pszFieldName, sLog);

	if (!bCan)
		return FALSE;

	WoormField*	pRepField = GetEditorManager() ? GetEditorManager()->GetSymTable()->GetField(pszFieldName) : NULL;
	if (pRepField)
	{
		//TODO forse bastava !pRepField->IsHidden()
		if (ExistsFieldID(pRepField->GetId(), FALSE))
		{
			sLog = _TB("The field is showed on page");
			if (onlyUI) *onlyUI = TRUE;
			return FALSE;
		}
	}

	return TRUE;
}

//-----------------------------------------------------------------------------
void CWoormDocMng::OnCopyText	() 
{
	if (m_sCopyText.IsEmpty())
		return;
	::CopyToClipboard(m_sCopyText);
}

//-----------------------------------------------------------------------------
CView* CWoormDocMng::CreateSlaveView(CRuntimeClass* pViewClass)
{
	//CWnd* pParent = this->GetWoormFrame();

	// enable other view only if not already created
	CView* pView = ViewAlreadyPresent(pViewClass, TRUE);
	if (pView)
	{
		Activate(pView);
		return pView;
	}

	ASSERT(GetDocTemplate());
	ASSERT(GetDocTemplate()->IsKindOf(RUNTIME_CLASS(CSingleExtDocTemplate)));

	// cerca il template che e' composta dalla view indicata e dal documento in oggetto (this)
	CSingleExtDocTemplate* pTemplate;
	pTemplate = const_cast<CSingleExtDocTemplate*>(AfxGetTemplate(pViewClass, 0/*nViewID*/, GetRuntimeClass()));

	if (!pTemplate)
	{
		TRACE("CWoormDocMng::CreateSlaveView: not registered template. View: %s. Check interface file\n", pViewClass->m_lpszClassName);
		return NULL;
	}
	ASSERT(pTemplate->IsKindOf(RUNTIME_CLASS(CSingleExtDocTemplate)));

	m_nsCurrentViewParent = GetNamespace();

	CFrameWnd* pNewFrame = pTemplate->CreateNewFrame(this, NULL);

	m_nsCurrentViewParent.Clear();

	if (pNewFrame)
	{
		//mi serve il puntatore alla View per aggiungerci una informazione strutturale
		CView* pView = NULL;
		CWnd* pWnd = pNewFrame->GetDescendantWindow(AFX_IDW_PANE_FIRST, TRUE);
		if (pWnd != NULL && pWnd->IsKindOf(RUNTIME_CLASS(CView)))
		{
			pView = (CView*)pWnd;
		}
		ASSERT(pView);

		pTemplate->InitialUpdateFrame(pNewFrame, this);

		return pView;
	}

	ASSERT(pNewFrame);
	return NULL;
}

//-----------------------------------------------------------------------------
CFontStylesDlg* CWoormDocMng::CreateFontStyleDlg(int& nFontIdx)
{
	return new CFontStylesDlg (*(m_pFontStyles), nFontIdx, FALSE, NULL, GetNamespace(), FALSE, m_Template.m_bIsTemplate);
}

//-----------------------------------------------------------------------------
EditorManager*	CWoormDocMng::GetEditorManager() const
{
	return m_pEditorManager;
}

//-----------------------------------------------------------------------------
/*
ERefreshEditor CWoormDocMng::GetRefreshType(CNodeTree::ENodeType nt)
{
	switch(nt)
	{
		case CNodeTree::ENodeType::NT_ASKFIELD:
		case CNodeTree::ENodeType::NT_ASKFIELD:

	}
	return 	ERefreshEditor::Layouts;
}
*/
//-----------------------------------------------------------------------------
CRSTreeCtrl* CWoormDocMng::GetRSTree(ERefreshEditor e)
{
	switch (e)
	{
		case ERefreshEditor::Layouts:
		case ERefreshEditor::Links:
		{
			if (GetWoormFrame()->GetLayoutTreeView())
				return &GetWoormFrame()->GetLayoutTreeView()->m_TreeCtrl;
			break;
		}
		case ERefreshEditor::Variables:
		case ERefreshEditor::Rules:
		case ERefreshEditor::RuleGroup:
		case ERefreshEditor::Events:
		case ERefreshEditor::Procedures:
		case ERefreshEditor::Queries:
		case ERefreshEditor::Dialogs:
		{
			if (GetWoormFrame()->GetEngineTreeView())
				return &GetWoormFrame()->GetEngineTreeView()->m_TreeCtrl;
			break;
		}
	}
	ASSERT(FALSE);
	return NULL;
}

//-----------------------------------------------------------------------
void CWoormDocMng::RSTreeEnsureVisibleItem(ERefreshEditor e, CObject* pObj, BOOL bSelect, BOOL bExpand, BOOL bPassive)
{
	CRSTreeCtrl* pTreeCtrl = GetRSTree(e);
	if (!pTreeCtrl) return;


	HTREEITEM ht = pTreeCtrl->FindItemData((DWORD)pObj, pTreeCtrl->m_htLayouts);
	if (!ht)
		return;

	pTreeCtrl->EnsureVisible(ht);

	if (bExpand)
		pTreeCtrl->Expand(ht, TVE_EXPAND);
	if (bSelect && !bPassive)
		pTreeCtrl->SelectItem(ht);
	else if (bSelect && bPassive)
	{
		BOOL bTreeWasPassive = pTreeCtrl->m_bPassive;
		pTreeCtrl->m_bPassive = TRUE;
		pTreeCtrl->SelectItem(ht);
		pTreeCtrl->m_bPassive = bTreeWasPassive;
	}
}

//-----------------------------------------------------------------------
void CWoormDocMng::UpdateRSTreeNode(ERefreshEditor e, CNodeTree* pNode, BOOL bSelect)
{
	CRSTreeCtrl* pTreeCtrl = GetRSTree(e);
	if (pTreeCtrl)
	{
		pTreeCtrl->UpdateRSTreeNode(pNode);
		if (bSelect)
			pTreeCtrl->SelectItem(pNode->m_ht);
	}
}

//-----------------------------------------------------------------------
void CWoormDocMng::UpdateRSTreeNode(ERefreshEditor e, BaseObj* pObj, BOOL bSelect)
{
	CRSTreeCtrl* pTreeCtrl = GetRSTree(e);
	if (!pTreeCtrl) return;

	
	HTREEITEM ht = pTreeCtrl->FindItemData((DWORD)pObj, pTreeCtrl->m_htLayouts);
	if (!ht)
		return;
	CNodeTree* pNode = (CNodeTree*)pTreeCtrl->GetItemData(ht);
	pTreeCtrl->UpdateRSTreeNode(pNode);
	if (bSelect)
		pTreeCtrl->SelectItem(pNode->m_ht);
}

//-----------------------------------------------------------------------
void CWoormDocMng::UpdateRSTreeNode(ERefreshEditor e, MultiColumnSelection* pColumns)
{
	CRSTreeCtrl* pTreeCtrl = GetRSTree(e);
	if (!pTreeCtrl) return;

	TableColumn* pCol;
	for (int i = 0; i < pColumns->GetSize(); i++)
	{
		pCol = pColumns->GetAt(i);
		HTREEITEM ht = pTreeCtrl->FindItemData((DWORD)pCol, pTreeCtrl->m_htLayouts);
	if (!ht)
			continue;
		CNodeTree* pNode = (CNodeTree*)pTreeCtrl->GetItemData(ht);
		pTreeCtrl->UpdateRSTreeNode(pNode);
	}
}

//-----------------------------------------------------------------------
void CWoormDocMng::UpdateRSTreeNode(ERefreshEditor e, SelectionRect* pMulSel)
{
	CRSTreeCtrl* pTreeCtrl = GetRSTree(e);
	if (!pTreeCtrl) return;

	for (int i = 0; i < pMulSel->GetSize(); i++)
	{
		BaseObj* pBase = pMulSel->GetObjAt(i);
		ASSERT_VALID(pBase);

		HTREEITEM ht = pTreeCtrl->FindItemData((DWORD)pBase, pTreeCtrl->m_htLayouts);
		if (!ht)
			continue;
		CNodeTree* pNode = pTreeCtrl->GetNode(ht);
		if (pNode) pTreeCtrl->UpdateRSTreeNode(pNode);
	}
}

//-----------------------------------------------------------------------------
HTREEITEM CWoormDocMng::RefreshRSTree(ERefreshEditor e, CObject* itemDataToSelect /*= NULL*/, BOOL selectRoot/* = FALSE*/)
{
	CRSTreeCtrl* pTreeCtrl = NULL;
	HTREEITEM htRoot = NULL;

	switch(e)
	{
	case ERefreshEditor::ToolBox:
	{	 		
		 GetWoormFrame()->m_pToolBoxTreeView->m_TreeCtrl.FillToolBox(TRUE);
		 break;
		
	}
	case ERefreshEditor::Layouts:
	{
		if (GetWoormFrame()->GetLayoutTreeView())
		{
			pTreeCtrl = &GetWoormFrame()->GetLayoutTreeView()->m_TreeCtrl;

				pTreeCtrl->FillLayouts();

				htRoot = pTreeCtrl->m_htLayouts;
			}
			break;
		}
		case ERefreshEditor::Links:
		{
			if (GetWoormFrame()->GetLayoutTreeView())
			{
				pTreeCtrl = &GetWoormFrame()->GetLayoutTreeView()->m_TreeCtrl;

				pTreeCtrl->FillLinks();

				htRoot = pTreeCtrl->m_htLinks;
			}
			break;
		}
		case ERefreshEditor::Variables:
		{
			if (GetWoormFrame()->GetEngineTreeView())
			{
				pTreeCtrl = &GetWoormFrame()->GetEngineTreeView()->m_TreeCtrl;

				pTreeCtrl->FillVariables(TRUE, FALSE);

				htRoot = pTreeCtrl->m_htVariables;
			}
			break;
		}
		case ERefreshEditor::Rules:
		{
			if (GetWoormFrame()->GetEngineTreeView())
			{
				pTreeCtrl = &GetWoormFrame()->GetEngineTreeView()->m_TreeCtrl;

				pTreeCtrl->FillRules();

				htRoot = pTreeCtrl->m_htRules;
			}
			break;
		}
		case ERefreshEditor::RuleGroup:
		{
			if (GetWoormFrame()->GetEngineTreeView())
			{
				pTreeCtrl = &GetWoormFrame()->GetEngineTreeView()->m_TreeCtrl;

				pTreeCtrl->FillTupleRules();

				htRoot = pTreeCtrl->m_htTupleRules;
			}
			break;
		}
		case ERefreshEditor::Events:
		{
			if (GetWoormFrame()->GetEngineTreeView())
			{
				pTreeCtrl = &GetWoormFrame()->GetEngineTreeView()->m_TreeCtrl;

				pTreeCtrl->FillEvents();

				htRoot = pTreeCtrl->m_htEvents;
			}
			break;
		}
		case ERefreshEditor::Procedures:
		{
			if (GetWoormFrame()->GetEngineTreeView())
			{
				pTreeCtrl = &GetWoormFrame()->GetEngineTreeView()->m_TreeCtrl;

				pTreeCtrl->FillProcedures();

				htRoot = pTreeCtrl->m_htProcedures;
			}
			break;
		}
		case ERefreshEditor::Queries:
		{
			if (GetWoormFrame()->GetEngineTreeView())
			{
				pTreeCtrl = &GetWoormFrame()->GetEngineTreeView()->m_TreeCtrl;

				pTreeCtrl->FillQueries();

				htRoot = pTreeCtrl->m_htQueries;
			}
			break;
		}

		case ERefreshEditor::Dialogs:
		{
			if (GetWoormFrame()->GetEngineTreeView())
			{
				pTreeCtrl = &GetWoormFrame()->GetEngineTreeView()->m_TreeCtrl;

				pTreeCtrl->FillDialogs();

				htRoot = pTreeCtrl->m_htAskDialogs;
			}
			break;
		}
	}

	if (pTreeCtrl && htRoot)
	{ 
		if (itemDataToSelect)
		{
			ASSERT_VALID(itemDataToSelect);
			HTREEITEM ht = pTreeCtrl->SelectRSTreeItemData(itemDataToSelect, htRoot);
			ASSERT(!selectRoot);
			return ht;
		}
		else if (selectRoot) 
		{
			pTreeCtrl->SelectItem(htRoot);
			pTreeCtrl->EnsureVisible(htRoot);
			pTreeCtrl->Expand(htRoot, TVE_EXPAND);
			return htRoot;
		}
	}
	return NULL;
}

//-----------------------------------------------------------------------------
HTREEITEM CWoormDocMng::SelectRSTreeItemData(ERefreshEditor e, CObject* itemDataToSelect, BOOL bAppendToSelected)
{
	switch (e)
	{
	case ERefreshEditor::Layouts:
	{
		if (GetWoormFrame()->GetLayoutTreeView())
		{
			CRSTreeCtrl* pTreeCtrl = &GetWoormFrame()->GetLayoutTreeView()->m_TreeCtrl;
			ASSERT(pTreeCtrl);
			return pTreeCtrl->SelectRSTreeItemData(itemDataToSelect, pTreeCtrl->m_htLayouts, bAppendToSelected);
		}
		break;
	}
	case ERefreshEditor::Links:
	{
		if (GetWoormFrame()->GetLayoutTreeView())
		{
			CRSTreeCtrl* pTreeCtrl = &GetWoormFrame()->GetLayoutTreeView()->m_TreeCtrl;
			ASSERT(pTreeCtrl);
			return pTreeCtrl->SelectRSTreeItemData(itemDataToSelect, pTreeCtrl->m_htLinks, bAppendToSelected);
		}
		break;
	}
	case ERefreshEditor::Variables:
	{
		if (GetWoormFrame()->GetEngineTreeView())
		{
			CRSTreeCtrl* pTreeCtrl = &GetWoormFrame()->GetEngineTreeView()->m_TreeCtrl;
			ASSERT(pTreeCtrl);
			return pTreeCtrl->SelectRSTreeItemData(itemDataToSelect, pTreeCtrl->m_htVariables, bAppendToSelected);
		}
		break;
	}
	case ERefreshEditor::Rules:
	{
		if (GetWoormFrame()->GetEngineTreeView())
		{
			CRSTreeCtrl* pTreeCtrl = &GetWoormFrame()->GetEngineTreeView()->m_TreeCtrl;
			ASSERT(pTreeCtrl);
			return pTreeCtrl->SelectRSTreeItemData(itemDataToSelect, pTreeCtrl->m_htRules, bAppendToSelected);
		}
		break;
	}
	case ERefreshEditor::RuleGroup:
	{
		if (GetWoormFrame()->GetEngineTreeView())
		{
			CRSTreeCtrl* pTreeCtrl = &GetWoormFrame()->GetEngineTreeView()->m_TreeCtrl;
			ASSERT(pTreeCtrl);
			return pTreeCtrl->SelectRSTreeItemData(itemDataToSelect, pTreeCtrl->m_htTupleRules, bAppendToSelected);
		}
		break;
	}
	case ERefreshEditor::Events:
	{
		if (GetWoormFrame()->GetEngineTreeView())
		{
			CRSTreeCtrl* pTreeCtrl = &GetWoormFrame()->GetEngineTreeView()->m_TreeCtrl;
			ASSERT(pTreeCtrl);
			return pTreeCtrl->SelectRSTreeItemData(itemDataToSelect, pTreeCtrl->m_htEvents, bAppendToSelected);
		}
		break;
	}
	case ERefreshEditor::Procedures:
	{
		if (GetWoormFrame()->GetEngineTreeView())
		{
			CRSTreeCtrl* pTreeCtrl = &GetWoormFrame()->GetEngineTreeView()->m_TreeCtrl;
			ASSERT(pTreeCtrl);
			return pTreeCtrl->SelectRSTreeItemData(itemDataToSelect, pTreeCtrl->m_htProcedures, bAppendToSelected);
		}
		break;
	}
	case ERefreshEditor::Queries:
	{
		if (GetWoormFrame()->GetEngineTreeView())
		{
			CRSTreeCtrl* pTreeCtrl = &GetWoormFrame()->GetEngineTreeView()->m_TreeCtrl;
			ASSERT(pTreeCtrl);
			return pTreeCtrl->SelectRSTreeItemData(itemDataToSelect, pTreeCtrl->m_htQueries, bAppendToSelected);
		}
		break;
	}

	case ERefreshEditor::Dialogs:
	{
		if (GetWoormFrame()->GetEngineTreeView())
		{
			CRSTreeCtrl* pTreeCtrl = &GetWoormFrame()->GetEngineTreeView()->m_TreeCtrl;
			ASSERT(pTreeCtrl);
			return pTreeCtrl->SelectRSTreeItemData(itemDataToSelect, pTreeCtrl->m_htAskDialogs, bAppendToSelected);
		}
		break;
	}
	}
	return NULL;
}


//-----------------------------------------------------------------------------
HTREEITEM CWoormDocMng::DeselectRSTreeItemData(ERefreshEditor e, CObject* itemDataToSelect)
{
	switch (e)
	{
	case ERefreshEditor::Layouts:
	{
		if (GetWoormFrame()->GetLayoutTreeView())
		{
			CRSTreeCtrl* pTreeCtrl = &GetWoormFrame()->GetLayoutTreeView()->m_TreeCtrl;
			ASSERT(pTreeCtrl);
			return pTreeCtrl->DeselectRSTreeItemData(itemDataToSelect, pTreeCtrl->m_htLayouts);
		}
		break;
	}
	case ERefreshEditor::Links:
	{
		if (GetWoormFrame()->GetLayoutTreeView())
		{
			CRSTreeCtrl* pTreeCtrl = &GetWoormFrame()->GetLayoutTreeView()->m_TreeCtrl;
			ASSERT(pTreeCtrl);
			return pTreeCtrl->DeselectRSTreeItemData(itemDataToSelect, pTreeCtrl->m_htLinks);
		}
		break;
	}
	case ERefreshEditor::Variables:
	{
		if (GetWoormFrame()->GetEngineTreeView())
		{
			CRSTreeCtrl* pTreeCtrl = &GetWoormFrame()->GetEngineTreeView()->m_TreeCtrl;
			ASSERT(pTreeCtrl);
			return pTreeCtrl->DeselectRSTreeItemData(itemDataToSelect, pTreeCtrl->m_htVariables);
		}
		break;
	}
	case ERefreshEditor::Rules:
	{
		if (GetWoormFrame()->GetEngineTreeView())
		{
			CRSTreeCtrl* pTreeCtrl = &GetWoormFrame()->GetEngineTreeView()->m_TreeCtrl;
			ASSERT(pTreeCtrl);
			return pTreeCtrl->DeselectRSTreeItemData(itemDataToSelect, pTreeCtrl->m_htRules);
		}
		break;
	}
	case ERefreshEditor::RuleGroup:
	{
		if (GetWoormFrame()->GetEngineTreeView())
		{
			CRSTreeCtrl* pTreeCtrl = &GetWoormFrame()->GetEngineTreeView()->m_TreeCtrl;
			ASSERT(pTreeCtrl);
			return pTreeCtrl->DeselectRSTreeItemData(itemDataToSelect, pTreeCtrl->m_htTupleRules);
		}
		break;
	}
	case ERefreshEditor::Events:
	{
		if (GetWoormFrame()->GetEngineTreeView())
		{
			CRSTreeCtrl* pTreeCtrl = &GetWoormFrame()->GetEngineTreeView()->m_TreeCtrl;
			ASSERT(pTreeCtrl);
			return pTreeCtrl->DeselectRSTreeItemData(itemDataToSelect, pTreeCtrl->m_htEvents);
		}
		break;
	}
	case ERefreshEditor::Procedures:
	{
		if (GetWoormFrame()->GetEngineTreeView())
		{
			CRSTreeCtrl* pTreeCtrl = &GetWoormFrame()->GetEngineTreeView()->m_TreeCtrl;
			ASSERT(pTreeCtrl);
			return pTreeCtrl->DeselectRSTreeItemData(itemDataToSelect, pTreeCtrl->m_htProcedures);
		}
		break;
	}
	case ERefreshEditor::Queries:
	{
		if (GetWoormFrame()->GetEngineTreeView())
		{
			CRSTreeCtrl* pTreeCtrl = &GetWoormFrame()->GetEngineTreeView()->m_TreeCtrl;
			ASSERT(pTreeCtrl);
			return pTreeCtrl->DeselectRSTreeItemData(itemDataToSelect, pTreeCtrl->m_htQueries);
		}
		break;
	}

	case ERefreshEditor::Dialogs:
	{
		if (GetWoormFrame()->GetEngineTreeView())
		{
			CRSTreeCtrl* pTreeCtrl = &GetWoormFrame()->GetEngineTreeView()->m_TreeCtrl;
			ASSERT(pTreeCtrl);
			return pTreeCtrl->DeselectRSTreeItemData(itemDataToSelect, pTreeCtrl->m_htAskDialogs);
		}
		break;
	}
	}
	return NULL;
}

//-----------------------------------------------------------------------------
//Return if a tree branch contains this item data
HTREEITEM CWoormDocMng::FindRSTreeItemData(ERefreshEditor eRe, CObject* itemDataToSelect)
{
	switch (eRe)
	{
	case ERefreshEditor::Layouts:
	{
		if (GetWoormFrame()->GetLayoutTreeView())
		{
			CRSTreeCtrl* pTreeCtrl = &GetWoormFrame()->GetLayoutTreeView()->m_TreeCtrl;
			ASSERT(pTreeCtrl);
			return pTreeCtrl->FindItemData((DWORD)itemDataToSelect, pTreeCtrl->m_htLayouts);
		}
		break;
	}
	case ERefreshEditor::Links:
	{
		if (GetWoormFrame()->GetLayoutTreeView())
		{
			CRSTreeCtrl* pTreeCtrl = &GetWoormFrame()->GetLayoutTreeView()->m_TreeCtrl;
			ASSERT(pTreeCtrl);
			return pTreeCtrl->FindItemData((DWORD)itemDataToSelect, pTreeCtrl->m_htLinks);
		}
		break;
	}
	case ERefreshEditor::Variables:
	{
		if (GetWoormFrame()->GetEngineTreeView())
		{
			CRSTreeCtrl* pTreeCtrl = &GetWoormFrame()->GetEngineTreeView()->m_TreeCtrl;
			ASSERT(pTreeCtrl);
			return pTreeCtrl->FindItemData((DWORD)itemDataToSelect, pTreeCtrl->m_htVariables);
		}
		break;
	}
	case ERefreshEditor::Rules:
	{
		if (GetWoormFrame()->GetEngineTreeView())
		{
			CRSTreeCtrl* pTreeCtrl = &GetWoormFrame()->GetEngineTreeView()->m_TreeCtrl;
			ASSERT(pTreeCtrl);
			return pTreeCtrl->FindItemData((DWORD)itemDataToSelect, pTreeCtrl->m_htRules);
		}
		break;
	}
	case ERefreshEditor::RuleGroup:
	{
		if (GetWoormFrame()->GetEngineTreeView())
		{
			CRSTreeCtrl* pTreeCtrl = &GetWoormFrame()->GetEngineTreeView()->m_TreeCtrl;
			ASSERT(pTreeCtrl);
			return pTreeCtrl->FindItemData((DWORD)itemDataToSelect, pTreeCtrl->m_htTupleRules);
		}
		break;
	}
	case ERefreshEditor::Events:
	{
		if (GetWoormFrame()->GetEngineTreeView())
		{
			CRSTreeCtrl* pTreeCtrl = &GetWoormFrame()->GetEngineTreeView()->m_TreeCtrl;
			ASSERT(pTreeCtrl);
			return pTreeCtrl->FindItemData((DWORD)itemDataToSelect, pTreeCtrl->m_htEvents);
		}
		break;
	}
	case ERefreshEditor::Procedures:
	{
		if (GetWoormFrame()->GetEngineTreeView())
		{
			CRSTreeCtrl* pTreeCtrl = &GetWoormFrame()->GetEngineTreeView()->m_TreeCtrl;
			ASSERT(pTreeCtrl);
			return pTreeCtrl->FindItemData((DWORD)itemDataToSelect, pTreeCtrl->m_htProcedures);
		}
		break;
	}
	case ERefreshEditor::Queries:
	{
		if (GetWoormFrame()->GetEngineTreeView())
		{
			CRSTreeCtrl* pTreeCtrl = &GetWoormFrame()->GetEngineTreeView()->m_TreeCtrl;
			ASSERT(pTreeCtrl);
			return pTreeCtrl->FindItemData((DWORD)itemDataToSelect, pTreeCtrl->m_htQueries);
		}
		break;
	}
	case ERefreshEditor::Dialogs:
	{
		if (GetWoormFrame()->GetEngineTreeView())
		{
			CRSTreeCtrl* pTreeCtrl = &GetWoormFrame()->GetEngineTreeView()->m_TreeCtrl;
			ASSERT(pTreeCtrl);
			return pTreeCtrl->FindItemData((DWORD)itemDataToSelect, pTreeCtrl->m_htAskDialogs);
		}
		break;
	}
	}
	return NULL;
}


//-----------------------------------------------------------------------------
//Return if a tree branch contains this item data
BOOL CWoormDocMng::RSTreeContainsItemData(ERefreshEditor eRe, CObject* itemDataToSelect)
{
	HTREEITEM ht = FindRSTreeItemData(eRe, itemDataToSelect);
	
	return ht ? TRUE : FALSE;
}

//-----------------------------------------------------------------------------
//Return if a tree branch contains this item data
BOOL CWoormDocMng::RSTreeIsSelectedItemData(ERefreshEditor eRe, CObject* itemDataToSelect)
{
	HTREEITEM ht = FindRSTreeItemData(eRe, itemDataToSelect);
	if (!ht)
		return FALSE;

	CRSTreeCtrl* pTreeCtrl = NULL;

	switch (eRe)
	{
		case ERefreshEditor::Layouts:
		case ERefreshEditor::Links:
		{
			if (GetWoormFrame()->GetLayoutTreeView())
				pTreeCtrl = &GetWoormFrame()->GetLayoutTreeView()->m_TreeCtrl;
			break;
		}
		case ERefreshEditor::Variables:
		case ERefreshEditor::Rules:
		case ERefreshEditor::RuleGroup:
		case ERefreshEditor::Events:
		case ERefreshEditor::Procedures:
		case ERefreshEditor::Queries:
		case ERefreshEditor::Dialogs:
		{
			if (GetWoormFrame()->GetEngineTreeView())
				pTreeCtrl = &GetWoormFrame()->GetEngineTreeView()->m_TreeCtrl;	
			break;
		}
	}

	if (!pTreeCtrl)
	{
		ASSERT(FALSE);
		return FALSE;
	}

	if (pTreeCtrl->GetItemState(ht, TVIS_SELECTED) & TVIS_SELECTED)
		return TRUE;
	return FALSE;
}

//-----------------------------------------------------------------------------
void  CWoormDocMng::ClearMultiSelection(BOOL ifMultiselect)
{
	//rimuovo la multiselezione delle colonne
	if (m_pMultiColumns)
		m_pMultiColumns->RemoveAllSelectedColumns();
	//rimuovo la multiselezione degli oggetti grafici generici
	if (m_pMultipleSelObj)
		m_pMultipleSelObj->ClearMultipleSelObjects();
	//rimuovo la multiselezione degli oggetti grafici generici
	if (m_pNodesSelection && !ifMultiselect)
		m_pNodesSelection->RemoveAll();
}

//-----------------------------------------------------------------------------
void  CWoormDocMng::ClearSelectionFromAllTrees(CRSTreeCtrl* pTree)
{
	//SKIP tree dell' EDITVIEW
	CRSEditView* pEditView = dynamic_cast<CRSEditView*>(ViewAlreadyPresent(RUNTIME_CLASS(CRSEditView), TRUE));
	if (pEditView)
	{
		CRSEditorToolView* pToolTreeView = pEditView->GetEditorFrame(TRUE)->m_pToolTreeView;
		if (pToolTreeView)
		{ 
			CRSTreeCtrl* pEditTree = &pToolTreeView->m_TreeCtrl;
			if (pEditTree == pTree)
				return;
		}
	}

	if(pTree)
		ClearMultiSelection(pTree->m_bMultiSelectCustom);

	//database
	if (GetWoormFrame()->m_pToolBoxDBView)
	{
		CRSTreeCtrl* pDbTree = &GetWoormFrame()->m_pToolBoxDBView->m_TreeCtrl;
		ASSERT_VALID(pDbTree);
		if (pDbTree == pTree)
			return;
	}


	//toolbox
	if (GetWoormFrame()->m_pToolBoxTreeView)
	{
		CRSTreeCtrl* pToolsTree = &GetWoormFrame()->m_pToolBoxTreeView->m_TreeCtrl;
		ASSERT_VALID(pToolsTree);
		if (pToolsTree == pTree)
			return;
	}

	//engine
	if (GetWoormFrame()->GetEngineTreeView())
	{ 
		CRSTreeCtrl* pEngineTree = &GetWoormFrame()->GetEngineTreeView()->m_TreeCtrl;
		ASSERT_VALID(pEngineTree);
		if (pEngineTree && (!pTree || pEngineTree!=pTree))
			pEngineTree->ClearSelection();
	}

	//layout
	if (GetWoormFrame()->GetLayoutTreeView())
	{
		CRSTreeCtrl* pLayoutTree = &GetWoormFrame()->GetLayoutTreeView()->m_TreeCtrl;
		ASSERT_VALID(pLayoutTree);
		if (pLayoutTree && (!pTree || pLayoutTree != pTree))
			pLayoutTree->ClearSelection();
	}
	
}

//-----------------------------------------------------------------------------

void CWoormDocMng::OnUpdateSwitchTo(CCmdUI* pCmdUI) 
{ 
	//pCmdUI->Enable(!m_bAllowEditing && !m_bEngineRunning);
	pCmdUI->Enable(TRUE);
}
//-----------------------------------------------------------------------------
 void CWoormDocMng::OnAddLink() 
{
	 GetWoormFrame()->m_pObjectPropertyView->NewHyperLink();
}

 //-----------------------------------------------------------------------------
void CWoormDocMng::OnUpdateBackToMenu(CCmdUI* pCmdUI) 
{ 
	//pCmdUI->Enable(!m_bAllowEditing && !m_bEngineRunning);
	pCmdUI->Enable(TRUE);
}

//------------------------------------------------------------------------------
COLORREF CWoormDocMng::GetObjectSelectionColor()
{
	if (m_pWoormIni)
		return m_pWoormIni->GetObjectSelectionColor();

	ASSERT(FALSE);

	return RGB(0, 0, 0);
}

//-----------------------------------------------------------------------------
TblRuleData* CWoormDocMng::AddDBColumns_GetOrCreateTblRule(CNodeTree* pNode)
{
	if (pNode->m_NodeType != CNodeTree::ENodeType::NT_LIST_COLUMN_INFO)
	{
		ASSERT(FALSE);
		return NULL;
	}

	SqlCatalogEntry* pCatEntry = dynamic_cast<SqlCatalogEntry*>(pNode->m_pParentItemData);
	ASSERT_VALID(pCatEntry);
	if (!pCatEntry)
	{
		return NULL;
	}

	RuleDataArray* pRuleData = m_pEditorManager->GetPrgData()->GetRuleData();
	TblRuleData* pTblRule = pRuleData->GetTblRuleData(pCatEntry->m_strTableName);
	if (!pTblRule)
	{
		pTblRule = new TblRuleData(*GetEditorSymTable(), AfxGetDefaultSqlConnection(), pCatEntry->m_strTableName);
		pRuleData->Add(pTblRule);
		pRuleData->m_pLastTblRule = pTblRule;
	}
	return pTblRule;
}

//-----------------------------------------------------------------------------
WoormField* CWoormDocMng::AddDBColumns_CreateField(CNodeTree* pNode, TblRuleData* pTblRule, int idxTable)
{
	SqlColumnInfo* pColumn = dynamic_cast<SqlColumnInfo*>(pNode->m_pItemData);

	return AddDBColumns_CreateField(pColumn, pTblRule, idxTable);
}

WoormField* CWoormDocMng::AddDBColumns_CreateField(SqlColumnInfo* pColumn, TblRuleData* pTblRule, int idxTable)
{
	if (pTblRule->m_arSqlTableJoinInfoArray.GetAt(idxTable)->GetTableName().CompareNoCase(pColumn->GetTableName()))
		return NULL;
	if (!pTblRule->m_arSqlTableJoinInfoArray.GetAt(idxTable)->ExistColumn(pColumn->GetColumnName()))
		return NULL;

	CString varPrefix = pColumn->m_strTableName + '_'; // L"w_";
	CString varName = GetEditorSymTable()->GetAdviseName(varPrefix + pColumn->GetColumnName());
	DataType dType = pColumn->GetDataObjType();
	if (dType == DataType::Null)
	{
		CWordArray dataTypes;
		VERIFY(pColumn->GetDataObjTypes(dataTypes));
		if (dataTypes.GetCount() == 0)
		{
			return NULL;
		}

		dType = dataTypes[0];
	}

	// search if the table was already added
	BOOL bQualified = pTblRule->m_arSqlTableJoinInfoArray.GetSize() > 1;

	CString phName = bQualified ? pColumn->GetQualifiedColumnName() : pColumn->GetColumnName();

	if (pTblRule->m_arSqlTableJoinInfoArray.m_arFieldLinks[idxTable]->Find(phName))
		return NULL;

	/*DataFieldLink* pFL = */pTblRule->AddLink(phName, varName, FALSE, dType, idxTable);

	WoormTable*	pSymTable = pTblRule->GetSymTable();

	WoormField* pRepField = new WoormField(varName, WoormField::RepFieldType::FIELD_NORMAL, dType);
		pRepField->SetHidden(TRUE);
		pRepField->SetPrecision(AfxGetFormatStyleTable()->GetInputCharLen(dType, &GetNamespace()), pColumn->GetColumnDecimal());
		pRepField->SetTableRuleField(TRUE);
		pRepField->SetSpecialField(pColumn->m_bSpecial);
		pRepField->SetPhysicalName(phName);
	pSymTable->Add(pRepField);

	return pRepField;
}

//-----------------------------------------------------------------------------
WoormField* CWoormDocMng::AddDBColumns_FromToolBar(CNodeTree* pNode, BOOL bSelectIt, TblRuleData* pCurrTblRule, int idxTable, BOOL bRefresh/*=TRUE*/, CStringArray* varNames)
{
	WoormField* pRepField = AddDBColumns_CreateField(pNode, pCurrTblRule, idxTable);
	if (!pRepField)
		return NULL;
	if (bRefresh)
	{
		RefreshRSTree(ERefreshEditor::Variables);
		RefreshRSTree(ERefreshEditor::Rules, bSelectIt ? pRepField : NULL);
	}
	if (varNames)
		varNames->Add(pRepField->GetName());

	return pRepField;
}

//-----------------------------------------------------------------------------
WoormField* CWoormDocMng::AddDBColumns_FromToolBar(CNodeTreeArray* ar, BOOL bSelectLast, TblRuleData* pCurrTblRule, int idxTable, CStringArray* varNames)
{
	WoormField* pLastNewField = NULL;
	
	int n = ar->GetCount();
	for (int i = 0; i < n; i ++)
	{
		ASSERT_VALID(ar);
		CNodeTree* pNode = (*ar)[i];
		ASSERT_VALID(pNode);

		BOOL b = (i == ar->GetUpperBound());

		WoormField* pF = AddDBColumns_FromToolBar(pNode, b, pCurrTblRule, idxTable, b, varNames);
		if (pF) pLastNewField = pF;
	}
	return pLastNewField;
}

//-----------------------------------------------------------------------------
void CWoormDocMng::PreDeleteField(LPCTSTR pszFieldName)
{
	CRSTreeCtrl* pTreeVar = GetRSTree(ERefreshEditor::Variables);

	pTreeVar->RemoveFieldFromItemData(pszFieldName);

	CRSTreeCtrl* pTreeLayout = GetRSTree(ERefreshEditor::Layouts);

	if (pTreeVar != pTreeLayout)
	{
		pTreeLayout->RemoveFieldFromItemData(pszFieldName);
	}
}

//-----------------------------------------------------------------------------
void CWoormDocMng::RemoveObjectFromItemData(CObject* pItemData)
{
	CRSTreeCtrl* pTreeVar = GetRSTree(ERefreshEditor::Variables);

	pTreeVar->RemoveObjectFromItemData(pItemData);

	CRSTreeCtrl* pTreeLayout = GetRSTree(ERefreshEditor::Layouts);

	if (pTreeVar != pTreeLayout)
	{
		pTreeLayout->RemoveObjectFromItemData(pItemData);
	}
}

//=============================================================================
BOOL CWoormDocMng::CanCloseFrame(CFrameWnd* pFrameArg)
// permission to close all views using this frame
//  (at least one of our views must be in this frame)
{
	ASSERT_VALID(pFrameArg);
	if (pFrameArg->IsKindOf(RUNTIME_CLASS(CRSEditorFrame)))
		return TRUE;
	return __super::CanCloseFrame(pFrameArg);
}

//-----------------------------------------------------------------------------
//chiamata dall'engine
BOOL CWoormDocMng::OpenDebugger(ActionObj* pCurrCmd)
{
	BOOL bOk = FALSE;
	if (this->IsAWoormRunningMultithread())
	{
		HWND hwndThread = GetFrameHandle();
		if ( GetWindowThreadProcessId(hwndThread, NULL) != ::GetCurrentThreadId() ) 
			return AfxInvokeThreadFunction<DataBool, CWoormDocMng, DataLng>(hwndThread, this, &CWoormDocMng::DoOpenDebuggerWrp, DataLng((long)pCurrCmd));
	}
	return DoOpenDebugger(pCurrCmd);
}

DataBool CWoormDocMng::DoOpenDebuggerWrp(DataLng dl)
{
	ActionObj* pCurrCmd = dynamic_cast<ActionObj*>((ActionObj*)(long)dl);
	if (!pCurrCmd)
		return FALSE;
	
	return DoOpenDebugger(pCurrCmd);
}

BOOL CWoormDocMng::DoOpenDebugger (ActionObj* pCurrCmd)
{
	CRSEditViewDebug* pDebug = GetWoormFrame()->CreateEditViewDebug();
	
	if (!pDebug)
		return FALSE;
	ASSERT_VALID(pDebug);

	pDebug->OpenDebugger(pCurrCmd);
	
	return TRUE;
}

//=============================================================================

DataArray* CWoormDocMng::GetDataArrayFromId(WORD nID, DataArray* ownerBag)
{
	SymField* pF = m_ViewSymbolTable.GetFieldByID(nID);
	if (!pF)
	{
		ASSERT(FALSE); return NULL;
	}

	if (pF->IsArray())
	{
		return dynamic_cast<DataArray*>(pF->GetData());	
	}
	else
	{
		const TableColumn* pCol = GetObjects().FindColumnByID(nID);
		if (pCol)
		{
			DataArray* arrayFromCol = pCol->GetDataArrayFromId(nID);
			if (!ownerBag) ownerBag = new DataArray();
			ownerBag->Add(arrayFromCol);
			return arrayFromCol;
		}
	}
	{ ASSERT(FALSE); return NULL; }
}

//-----------------------------------------------------------------------------
BOOL CWoormDocMng::FillSeries(CBCGPChartSeries* pSeries, WORD nID)
{
	pSeries->RemoveAllDataPoints();
	SymField* pF = m_ViewSymbolTable.GetFieldByID(nID);
	if (!pF)
	{
		ASSERT(FALSE); return FALSE;
	}

	if (pF->IsArray())
	{
		double val = 0;
		DataArray* pAr = dynamic_cast<DataArray*>(pF->GetData());
		if (!pAr)
		{
			ASSERT(FALSE); return FALSE;
		}

		for (int i = 0; i < pAr->GetSize(); i++)
		{
			DataObj* pVal = pAr->GetAt(i);

			if (pVal->IsKindOf(RUNTIME_CLASS(DataDbl)))
			{
				val = (double) *(DataDbl*)pVal;
				pSeries->AddDataPoint(val);
			}
			else if (pVal->IsKindOf(RUNTIME_CLASS(DataInt)))
			{
				val = (short) *(DataInt*)pVal;
				pSeries->AddDataPoint(val);
			}
			else if (pVal->IsKindOf(RUNTIME_CLASS(DataLng)))
			{
				val = (long) *(DataLng*)pVal;
				pSeries->AddDataPoint(val);
			}
			else
			{
				//ASSERT(FALSE);
				val = 0;
			}
		}
		return TRUE;
	}
	else
	{
		const TableColumn* pCol = GetObjects().FindColumnByID(nID);;
		if (pCol)
			return pCol->FillSeries(pSeries, nID);
	}
	{ ASSERT(FALSE); return FALSE; }
}

//-----------------------------------------------------------------------------
BOOL CWoormDocMng::FillSeries(CBCGPChartSeries* pSeries, WORD nID_x, WORD nID_y)
{
	pSeries->RemoveAllDataPoints();

	SymField* pF_x = m_ViewSymbolTable.GetFieldByID(nID_x);
	SymField* pF_y = m_ViewSymbolTable.GetFieldByID(nID_y);
	if (!pF_x || !pF_y)
	{
		ASSERT(FALSE); return FALSE;
	}

	DataArray* pAr_x;
	if (pF_x->IsArray())
	{
		pAr_x = dynamic_cast<DataArray*>(pF_x->GetData());
	}
	else
	{
		const TableColumn* pCol = GetObjects().FindColumnByID(nID_x);
		if (pCol)
			pAr_x =pCol->GetDataArrayFromId(nID_x);
	}
	if (!pAr_x)
	{
		ASSERT(FALSE); return FALSE;
	}	

	DataArray* pAr_y;
	if (pF_y->IsArray())
	{
		pAr_y = dynamic_cast<DataArray*>(pF_y->GetData());
	}
	else
	{
		const TableColumn* pCol = GetObjects().FindColumnByID(nID_y);
		if (pCol)
			pAr_y = pCol->GetDataArrayFromId(nID_y);
	}
	if (!pAr_y || pAr_x->GetSize()!= pAr_y->GetSize())
	{
		ASSERT(FALSE); return FALSE;
	}

	for (int i = 0; i < pAr_x->GetSize(); i++)
	{
		DataObj* pVal_x = pAr_x->GetAt(i);
		DataObj* pVal_y = pAr_y->GetAt(i);
		double val_x = 0.0;
		double val_y = 0.0;

		if (pVal_x->IsKindOf(RUNTIME_CLASS(DataDbl)))
			val_x = (double) *(DataDbl*)pVal_x;
		else if (pVal_x->IsKindOf(RUNTIME_CLASS(DataInt)))
			val_x = (short) *(DataInt*)pVal_x;
		else if (pVal_x->IsKindOf(RUNTIME_CLASS(DataLng)))
			val_x = (long) *(DataLng*)pVal_x;
		else
			val_x = 0;
		if (pVal_y->IsKindOf(RUNTIME_CLASS(DataDbl)))
			val_y = (double) *(DataDbl*)pVal_y;
		else if (pVal_y->IsKindOf(RUNTIME_CLASS(DataInt)))
			val_y = (short) *(DataInt*)pVal_y;
		else if (pVal_y->IsKindOf(RUNTIME_CLASS(DataLng)))
			val_y = (long) *(DataLng*)pVal_y;
		else
			val_y = 0;

	pSeries->AddDataPoint(val_y,val_x);
	}

	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CWoormDocMng::FillSeries(CStringArray& pCategories, WORD nID) 
{
	pCategories.RemoveAll();

	SymField* pF = m_ViewSymbolTable.GetFieldByID(nID);
	if (!pF)
	{
		ASSERT(FALSE); return FALSE;
	}

	if (pF->IsArray())
	{
		double val = 0;
		DataArray* pAr = dynamic_cast<DataArray*>(pF->GetData());
		if (!pAr)
		{
			ASSERT(FALSE); return FALSE;
		}

		for (int i = 0; i < pAr->GetSize(); i++)
		{
			DataObj* pVal = pAr->GetAt(i);

			pCategories.Add(pVal->Str());
		}
		return TRUE;
	}
	else
	{
		const TableColumn* pCol = GetObjects().FindColumnByID(nID);
		if (pCol)
			return pCol->FillSeries(pCategories, nID);
	}
	{ ASSERT(FALSE); return FALSE; }
};

//=============================================================================
