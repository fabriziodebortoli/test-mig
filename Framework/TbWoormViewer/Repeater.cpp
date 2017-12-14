#include "stdafx.h"

#include <math.h>
#include <afxpriv.h>

#include <TBXmlCore\XMLTags.h>

#include <TbNameSolver\PathFinder.h>

#include <TbClientCore\ClientObjects.h>
#include <TbWebServicesWrappers\LoginManagerInterface.h>

#include <TbGeneric\minmax.h>
#include <TbGeneric\GeneralFunctions.h>
#include <TbGeneric\FontsTable.h>
#include <TbGeneric\DataTypesFormatters.h>
#include <TbGeneric\TbStrings.h>

#include <TbGenlibUI\FormatDialog.h>
#include <TbGenlibUI\FontsDialog.h>
#include <TbGenlib\generic.h>
#include <TbGenlib\baseapp.h>
#include <TbGenlib\const.h>
#include <TbGenlib\Expr.h>

//#include <TbGenlib\TbExplorerInterface.h>

#include <TbWoormEngine\edtmng.h>
#include <TbWoormEngine\RpSymTbl.h>
#include <TbWoormEngine\MultiLayout.h>

#include "export.h"           
#include "mclrdlg.h"
#include "viewpars.h"
#include "singleob.h"
#include "woormdoc.h"
#include "woormini.h"
#include "woormvw.h"
#include "rectobj.h"
//#include "docproperties.h"
//#include "EditorLinksDlg.h"
//#include "column.h"
#include "repeater.h"
// resources
#include "listdlg.hjson" //JSON AUTOMATIC UPDATE
#include "rectobj.hjson" //JSON AUTOMATIC UPDATE
#include "woormdoc.hjson" //JSON AUTOMATIC UPDATE

#ifdef _DEBUG
#undef THIS_FILE
static const char  THIS_FILE[] = __FILE__;
#endif


///////////////////////////////////////////////////////////////////////////////

//==============================================================================
//          Class RepeaterObjects implementation
//==============================================================================

IMPLEMENT_DYNAMIC (RepeaterObjects, Array)

RepeaterObjects::RepeaterObjects(Repeater* pRep)
	:
	m_pRepeater (pRep)
{ 
	ASSERT_VALID(pRep);

	CLayout* pMasterObjs = new CLayout(L"", pRep->m_pDocument);
	pMasterObjs->SetOwns(FALSE);

	Add(pMasterObjs); 
}

RepeaterObjects::RepeaterObjects(Repeater* pRep, const RepeaterObjects& source)
	:
	m_pRepeater (pRep)
{ 
	ASSERT_VALID(pRep);
	for (int i = 0; i < source.GetSize(); i++)
	{
		CLayout* pObjs = (CLayout*) (source[i]);
		ASSERT_VALID(pObjs);

		Add(pObjs->Clone());
	}
}

//------------------------------------------------------------------------------
RepeaterObjects::~RepeaterObjects()
{
}

//------------------------------------------------------------------------------
CLayout* RepeaterObjects::GetMasterObjects()
{
	if (GetSize() == 0)
	{
		CLayout* pMasterObjs = new CLayout(L"", m_pRepeater->m_pDocument, FALSE);
		pMasterObjs->SetOwns(FALSE);

		Add(pMasterObjs); 
	}
	return (CLayout*) GetAt(0);
}

//------------------------------------------------------------------------------
void RepeaterObjects::AddChild (BaseObj* pObj) 
{ 
	ASSERT_VALID(pObj);
	ASSERT_VALID(m_pRepeater);
	//ASSERT(pObj->m_AnchorRepeaterID == 0);

	pObj->m_AnchorRepeaterID = m_pRepeater->m_wInternalID;

	GetMasterObjects()->Add (pObj);

	if (!pObj->IsKindOf(RUNTIME_CLASS(FieldRect)))
		return;
	//----

	ASSERT_VALID(m_pRepeater->m_pDocument);
	ASSERT_VALID(m_pRepeater->m_pDocument->m_pEditorManager);
	WoormTable* pSymTable = (
		m_pRepeater->m_pDocument->m_pEditorManager 
		? 
		m_pRepeater->m_pDocument->m_pEditorManager->GetSymTable() 
		: NULL);
	ASSERT_VALID(pSymTable);
	if (pSymTable && pObj->GetInternalID())
	{
		WoormField* pF = pSymTable->GetFieldByID(pObj->GetInternalID());
		if (pF && pF->GetFieldType() == WoormField::FIELD_NORMAL)
			pF->SetDispTable(m_pRepeater->GetName());
	}
}

//------------------------------------------------------------------------------
void RepeaterObjects::Detach (BaseObj* pObj)
{
	ASSERT_VALID(pObj);
	ASSERT_VALID(m_pRepeater);

	pObj->m_AnchorRepeaterID = 0;   pObj->m_nRepeaterRow = -1;

	if (!pObj->IsKindOf(RUNTIME_CLASS(FieldRect)))
		return;
	//----

	ASSERT_VALID(m_pRepeater->m_pDocument);
	ASSERT_VALID(m_pRepeater->m_pDocument->m_pEditorManager);
	WoormTable* pSymTable = (
		m_pRepeater->m_pDocument->m_pEditorManager 
		? 
		m_pRepeater->m_pDocument->m_pEditorManager->GetSymTable() 
		: NULL);
	ASSERT_VALID(pSymTable);	
	if (pSymTable && pObj->GetInternalID())
	{
		WoormField* pF = pSymTable->GetFieldByID(pObj->GetInternalID());
		if (pF && pF->GetFieldType() == WoormField::FIELD_COLUMN)
		{
			pF->RemoveDispTable();
		}
	}
}

//------------------------------------------------------------------------------
void RepeaterObjects::RemoveChild (BaseObj* pObj)
{
	ASSERT_VALID(pObj);
	ASSERT_VALID(m_pRepeater);
	ASSERT_VALID(m_pRepeater->m_pDocument);

	int idx = GetMasterObjects()->FindPtr(pObj);
	if (idx < 0) 
		return;

	ASSERT(pObj->m_AnchorRepeaterID != 0);

	GetMasterObjects()->RemoveAt(idx);	//delete automatica DISABILITATA nel primo array

	idx += 1; //in posizione 0 c'e' un SqrRect di cornice
	for (int r = 1; r < GetSize(); r++)
	{
		CLayout* pRow = (CLayout*) GetAt(r);
		ASSERT_VALID(pRow);

		ASSERT(idx < pRow->GetSize());
		pRow->RemoveAt(idx);	//delete automatica
	}
	//----
	Detach(pObj);
}

//------------------------------------------------------------------------------
void RepeaterObjects::RemoveAll ()
{
	CLayout* pMasterObjs = GetMasterObjects();
	ASSERT_VALID(pMasterObjs);
	//----

	for (int i = 0; i < pMasterObjs->GetSize(); i++)
	{
		BaseObj* pO = (*pMasterObjs)[i];
		ASSERT_VALID(pO);

		Detach(pO);
	}

	//----
	__super::RemoveAll();
}

//------------------------------------------------------------------------------
void RepeaterObjects::Replicate()
{
	ASSERT_VALID(m_pRepeater);
	ASSERT(GetSize() == 1);

	CLayout* pMasterObjs = GetMasterObjects();
	ASSERT_VALID(pMasterObjs);

	int w = m_pRepeater->m_BaseRect.Width() + m_pRepeater->m_nXOffset;
	int h = m_pRepeater->m_BaseRect.Height() + m_pRepeater->m_nYOffset;

	CSize pageSize = m_pRepeater->m_pDocument->m_PageInfo.GetPageSize_LP();//todo vedere se landscape
	int maxCol = pageSize.cx / w + 1;
	int maxRow = pageSize.cy / h + 1;
	m_pRepeater->m_nColumns = m_pRepeater->m_nColumns > maxCol ? maxCol : m_pRepeater->m_nColumns;
	m_pRepeater->m_nRows = m_pRepeater->m_nRows > maxRow ? maxRow : m_pRepeater->m_nRows;

	//creo i blocchi di oggetti ripetuti (all'indice 0 ci sono gli originali in aliasing)
	int nr = (m_pRepeater->m_nColumns * m_pRepeater->m_nRows);
	for (int r = 1; r < nr; r++)
	{
		CLayout* pRow = pMasterObjs->Clone();
		pRow->SetOwns(TRUE);

		SqrRect* pR = new SqrRect(*m_pRepeater);
		pR->m_AnchorRepeaterID = m_pRepeater->GetInternalID();
		pRow->InsertAt(0,  pR);

		int x = m_pRepeater->m_bByColumn ?
			w * (r / (m_pRepeater->m_nRows))
			: 
			w * (r % (m_pRepeater->m_nColumns));

		int y = m_pRepeater->m_bByColumn ?
			h * (r % (m_pRepeater->m_nRows))
			: 
			h * (r / (m_pRepeater->m_nColumns));
		//remove Top borders
		if (m_pRepeater->m_nYOffset == 0 && y != 0)
		{
			pR->m_Borders.top = FALSE;
			RemoveDuplicateTopBorders(pRow);
		}
		//remove left borders
		if (m_pRepeater->m_nXOffset == 0 && x != 0)
		{
			pR->m_Borders.left = FALSE;
			RemoveDuplicateLeftBorders(pRow);
		}	

		pRow->MoveBaseRect(x, y, TRUE);

		for (int i = 0; i < pRow->GetSize(); i++)
		{
			(*pRow)[i]->m_nRepeaterRow = r;
		}

		Add(pRow);
	}

	CLayout* pRow = GetMasterObjects();
	for (int i = 0; i < pRow->GetSize(); i++)
	{
		(*pRow)[i]->m_nRepeaterRow = 0;
	}
}

//------------------------------------------------------------------------------
void RepeaterObjects::Draw(CDC& DC, BOOL bPreview, int nStart/* = 1*/)
{
	for (int r = nStart; r < GetSize(); r++)
	{
		CLayout* pRow = (CLayout*) (this->GetAt(r));
		pRow->Draw(DC, bPreview);
	}
}

//------------------------------------------------------------------------------
void RepeaterObjects::ClearDynamicAttributes()
{
	for (int r = 0; r < GetSize(); r++)
	{
		CLayout* pRow = (CLayout*) (this->GetAt(r));
		pRow->ClearDynamicAttributes();
	}
}

//------------------------------------------------------------------------------
void RepeaterObjects::DisableData()
{
	for (int r = 1; r < GetSize(); r++)
	{
		CLayout* pRow = (CLayout*) (this->GetAt(r));
		pRow->DisableData();
	}
}

//------------------------------------------------------------------------------
int RepeaterObjects::GetFieldRow (BaseObj* pO)
{
	for (int r = 0; r < GetSize(); r++)
	{
		CLayout* pRow = (CLayout*) (this->GetAt(r));
		if (pRow->FindPtr(pO) >= 0)
			return r;
	}
	return -1;
}

//------------------------------------------------------------------------------
BaseObj* RepeaterObjects::GetFieldByPosition (const CPoint& pt, int& nRow)
{
	for (int r = 0; r < GetSize(); r++)
	{
		CLayout* pRow = (CLayout*) (this->GetAt(r));
		//devo skippare gli SqrRect contenitori
		for (int j = (r ? 1 : 0); j < pRow->GetSize(); j++)
		{
			BaseObj* pO = (*pRow)[j];
			ASSERT_VALID(pO);

			if (pO->InMe(pt))
			{
				nRow = r;
				return pO;
			}
		}
	}
	return NULL;
}

//------------------------------------------------------------------------------
BaseObj* RepeaterObjects::GetMasterFieldByPosition (const CPoint& pt)
{
	CLayout* pRow = (CLayout*) (this->GetAt(0));
	//devo skippare gli SqrRect contenitori
	for (int j = 0; j < pRow->GetSize(); j++)
	{
		BaseObj* pO = (*pRow)[j];
		ASSERT_VALID(pO);

		if (pO->InMe(pt))
		{
			return pO;
		}
	}
	return NULL;
}

////------------------------------------------------------------------------------
void RepeaterObjects::MoveBaseRect	(int xOffset, int yOffset, BOOL bIgnoreBorder /*=FALSE*/)
{
	for (int i = 0; i < GetSize(); i++)
	{
		CLayout* pObjs = (CLayout*) (this->GetAt(i));

		pObjs->MoveBaseRect	(xOffset, yOffset, bIgnoreBorder);
	}
}

//------------------------------------------------------------------------------
void RepeaterObjects::RemoveDuplicateTopBorders(CLayout* pRow)
{
	if (m_pRepeater->m_nYOffset != 0)
		return;

	CObject* pObj = pRow->GetAt(0);
	SqrRect* pRep = dynamic_cast<SqrRect*>(pObj);
	if (!pRep)
	{
		ASSERT(FALSE);
		return;
	}
	ASSERT(pRep->m_AnchorRepeaterID == m_pRepeater->GetInternalID());

	for (int i = 1; i < pRow->GetCount(); i++)
	{
		pObj = pRow->GetAt(i);
		if (!pObj->IsKindOf(RUNTIME_CLASS(BaseObj)))
			continue;
		BaseRect* pBaseObj = (BaseRect*)pObj;
		//elimino bordo TOP se coincidente con quello del repeater
		if (pBaseObj->GetBaseRect().top == pRep->GetBaseRect().top)
			pBaseObj->m_Borders.top = FALSE;
		////elimino bordo SX se coincidente con quello del repeater
		//if (pBaseObj->GetBaseRect().left == pRep->GetBaseRect().left)
		//	pBaseObj->m_Borders.left = FALSE;
		////elimino bordo DX se coincidente con quello del repeater
		//if (pBaseObj->GetBaseRect().right == pRep->GetBaseRect().right)
		//	pBaseObj->m_Borders.right = FALSE;
		////elimino bordo BOTTOM se coincidente con quello del repeater
		//if (pBaseObj->GetBaseRect().bottom == pRep->GetBaseRect().bottom)
		//	pBaseObj->m_Borders.bottom = FALSE;
	}
}

//------------------------------------------------------------------------------
void RepeaterObjects::RemoveDuplicateLeftBorders(CLayout* pRow)
{
	if (m_pRepeater->m_nXOffset != 0)
		return;

	CObject* pObj = pRow->GetAt(0);
	SqrRect* pRep = dynamic_cast<SqrRect*>(pObj);
	if (!pRep)
	{
		ASSERT(FALSE);
		return;
	}
	ASSERT(pRep->m_AnchorRepeaterID == m_pRepeater->GetInternalID());

	for (int i = 1; i < pRow->GetCount(); i++)
	{
		pObj = pRow->GetAt(i);
		if (!pObj->IsKindOf(RUNTIME_CLASS(BaseObj)))
			continue;
		BaseRect* pBaseObj = (BaseRect*)pObj;
		
		//elimino bordo SX se coincidente con quello del repeater
		if (pBaseObj->GetBaseRect().left == pRep->GetBaseRect().left)
			pBaseObj->m_Borders.left = FALSE;
	}
}

//==============================================================================
//          Class Repeater implementation
//==============================================================================

//------------------------------------------------------------------------------
IMPLEMENT_DYNAMIC (Repeater, SqrRect)

//------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(Repeater, SqrRect)

	ON_COMMAND(ID_REPEATER_APPLY,			OnRebuild)
	ON_COMMAND(ID_REPEATER_PROPERTIES,		OnProperties)

END_MESSAGE_MAP()

//------------------------------------------------------------------------------
Repeater::Repeater (CPoint ptCurrPos, CWoormDocMng* pDocument, WORD wID)
	:
	SqrRect    	(ptCurrPos, pDocument),

	m_nRows		(1),
	m_nColumns	(1),
	m_nXOffset	(60),
	m_nYOffset	(30),
	m_bByColumn	(FALSE),
	m_Objects	(this),
	m_nCurrentRow (0),
	m_nViewCurrentRow (-1)
{
	this->m_wInternalID  = wID;
	this->m_bTransparent = TRUE;
}

Repeater::Repeater	(const Repeater& source) 
	: 
	SqrRect		(source),

	m_nRows		(source.m_nRows),
	m_nColumns	(source.m_nColumns),
	m_nXOffset	(source.m_nXOffset),
	m_nYOffset	(source.m_nYOffset),
	m_bByColumn	(source.m_bByColumn),
	m_Objects   (this, source.m_Objects),
	m_nCurrentRow (source.m_nCurrentRow),
	m_nViewCurrentRow (source.m_nViewCurrentRow)
{
}

Repeater::~Repeater()
{
}

//------------------------------------------------------------------------------
CString Repeater::GetDescription (BOOL /*= TRUE*/) const 
{ 
	CString sName;

	if (m_pDocument && m_pDocument->m_pEditorManager)
	{
		DisplayTables* pTbls = m_pDocument->m_pEditorManager->GetDispTable();
		if (pTbls)
		{
			sName = pTbls->GetName(GetInternalID());
		}
	}
	ASSERT(sName.CompareNoCase(GetName()) == 0);
	//return cwsprintf(_T("%s (Id:%d)"), sName, m_wInternalID); 
	return cwsprintf(sName);
}

//------------------------------------------------------------------------------
CString Repeater::GetName (BOOL bStringName /*= FALSE*/) const
{ 
	CString strName;
	if (bStringName && m_pDocument->m_pEngine && m_pDocument->m_pEngine->GetSymTable())
	{
		WoormTable* pST = m_pDocument->m_pEngine->GetSymTable();
		int idx = pST && pST->GetDisplayTables() ?
			pST->GetDisplayTables()->Find(GetInternalID(), m_pDocument->m_dsCurrentLayoutView)
			: -1;
		if (idx > -1)
		{
			strName = pST->GetDisplayTables()->GetAt(idx)->GetTableName();
		}
	}
	else if (bStringName && m_pDocument->m_pEditorManager)
	{
		ASSERT_VALID(m_pDocument->m_pEditorManager);
		DisplayTables* pTbls = m_pDocument->m_pEditorManager->GetDispTable();
		if (pTbls)
		{
			ASSERT_VALID(pTbls);
			strName = pTbls->GetName(this->GetInternalID());
		}
	}
	if (strName.IsEmpty())
		strName = cwsprintf(_T("Repeater_%d"), m_wInternalID);
	return strName;
}

//------------------------------------------------------------------------------
CLayout* Repeater::GetRowObjects	(int nRow)
{
	if (nRow < 0 || nRow > m_Objects.GetUpperBound())
		return NULL;
	return (CLayout*) m_Objects.GetAt(nRow);
}

//------------------------------------------------------------------------------
void Repeater::Attach	(BaseObj* pObj, BOOL bRepaint /*= FALSE*/)
{
	ASSERT_VALID(pObj);
	if (
			pObj->IsKindOf(RUNTIME_CLASS(Table)) ||
			pObj->IsKindOf(RUNTIME_CLASS(Repeater))
		)
		return;	//Oggetti complessi non gestiti

	if (pObj->GetInternalID() >= SpecialReportField::REPORT_LOWER_SPECIAL_ID)
	{
		return;
	}

	m_Objects.AddChild (pObj);

	if (bRepaint) 
		UpdateDocument();
}

//------------------------------------------------------------------------------
void Repeater::Detach	(BaseObj* pObj, BOOL bRepaint/* = FALSE*/)
{
	m_Objects.RemoveChild(pObj);

	if (bRepaint) 
		UpdateDocument();
}

//------------------------------------------------------------------------------
BOOL Rect_Contain (const CRect& big, const CRect& in)
{
	int eps = 6;

	return (
			in.left		>= (big.left	- eps) && 
			in.right	<= (big.right	+ eps) &&
			in.top		>= (big.top		- eps) &&
			in.bottom	<= (big.bottom	+ eps)
		);
}

void Repeater::Rebuild	(CLayout* pObjs)
{
	//mi salvo gli oggetti dentro al repeater
	CList<BaseObj*> oldObjs;
	for (int i = 0; i < m_Objects.GetMasterObjects()->GetCount(); i++)
		oldObjs.AddTail((BaseObj*)m_Objects.GetMasterObjects()->GetAt(i));

	/*CLayout oldObjs(*(m_Objects.GetMasterObjects()));*/
	m_Objects.RemoveAll();

	for (int i = 0; i < pObjs->GetSize(); i++)
	{
		BaseObj* pO = (*pObjs)[i];

		if (Rect_Contain(m_BaseRect, pO->GetBaseRect()))
			Attach(pO);
	}

	if (m_Objects.GetMasterObjects()->GetSize())
		m_Objects.Replicate();

	ASSERT_VALID(m_pDocument);
	ASSERT_VALID(m_pDocument->m_pEditorManager);

	if (m_pDocument->m_pEditorManager->ExistsTable(m_wInternalID))
	{
		if (!m_pDocument->m_pEditorManager->SetTableRows(m_wInternalID, m_nRows * m_nColumns))
		{
			// table wID not found must signal error but must delete table
			m_pDocument->Message(_TB("Id table not found"));
		}
	}

	m_pDocument->Invalidate(FALSE);

	int currSize = m_Objects.GetMasterObjects()->GetSize();
	int oldSize = oldObjs.GetSize();
	//--------------------------------------------------------------------------------------------Per aggiornare il tree
	//controllo se gli oggetti dentro al repeater sono cambiati in numero
	if(currSize != oldSize)
		m_pDocument->RefreshRSTree(ERefreshEditor::Layouts, this);
	//anche se c'è lo stesso numero di oggetti, potrebbero essere diversi, quindi controllo se devo aggiornare il tree del layout
	else
	{
		CList<BaseObj*> currObjs;
		for (int i = 0; i < m_Objects.GetMasterObjects()->GetCount(); i++)
			currObjs.AddTail((BaseObj*)m_Objects.GetMasterObjects()->GetAt(i));
		POSITION oldPos;
		oldPos = oldObjs.GetHeadPosition();

		int sameObjs = 0;
		while (oldPos)
		{
			BaseObj* oldObj = oldObjs.GetNext(oldPos);
			if(Contains(oldObj))
				sameObjs++;
		}

		if(currSize != sameObjs)
			m_pDocument->RefreshRSTree(ERefreshEditor::Layouts);
		currObjs.RemoveAll();
	}
	//--------------------------------------------------------------------------------------------
	oldObjs.RemoveAll();
}

//------------------------------------------------------------------------------
CRect Repeater::GetRectToInvalidate ()
{
	CRect r (m_BaseRect);

	r.right		+= (m_nColumns - 1) * (m_BaseRect.Width()	+ m_nXOffset);
	r.bottom	+= (m_nRows - 1)	* (m_BaseRect.Height()	+ m_nYOffset);

	return r;
}

//------------------------------------------------------------------------------
void Repeater::Draw (CDC& DC, BOOL bPreview)
{           
	if (!PreDraw(&DC))
		return;

	CRect inside;
	/*COLORREF bkgColor = */DoDraw(DC, bPreview, inside);
	//----

	m_Objects.Draw(DC, bPreview);

	//----
	PostDraw(DC, bPreview, m_BaseRect);
}

//------------------------------------------------------------------------------
BOOL Repeater::AssignData (WORD wID, RDEManager* pRDEmanager)
{
	if (m_nCurrentRow > m_Objects.GetUpperBound())
	{
		// no ID found in this Repeater
		if (m_Objects.GetMasterObjects()->FindByID(wID) == NULL)
			return FALSE;

		ASSERT(FALSE);
		m_pDocument->Message
		(	cwsprintf
			(
				_TB("There is not repeater panel to enter data. (Repeater ID:%d, panel:%d)\nCheck Repeater's rows equal to Repeater's rows in Report/Tables section too"), 
				m_wInternalID, m_nCurrentRow
			),
			MB_OK | MB_ICONEXCLAMATION
		);
		m_nCurrentRow  = 0;
	}

	CLayout* pRow = (CLayout*) m_Objects[m_nCurrentRow];
	BaseObj* pCell = pRow->FindByID(wID);
	if (pCell)
	{
		pCell->m_AnchorRepeaterID = 0;	//altrimenti esce
		BOOL bRet = pCell->AssignData(wID, pRDEmanager);
		pCell->m_AnchorRepeaterID = GetInternalID();
		return bRet;
	}

	// no ID found in this Repeater
	return FALSE;
}

//------------------------------------------------------------------------------
BOOL Repeater::ExecCommand (WORD wID, RDEManager* pRDEmanager)
{
	// execute command over all table
	if (m_wInternalID == wID)
	{
		switch (pRDEmanager->GetCommand())
		{
			case RDEManager::NEXT_LINE:
				m_nCurrentRow++;
				break;

			case RDEManager::TITLE_LINE:
			case RDEManager::CUSTOM_TITLE_LINE :
				//Non applicabili, Vengono ignorati - m_nCurrentRow++;
				break;

			case RDEManager::INTER_LINE :
				//TODO questo comando potrebbe essere utile se num colonne == 1 (per le tabelle simulate)
				break;

			default: break;
		}

		// ignore data
		pRDEmanager->SkipData();
		return TRUE;
	}

	//----
	// command non for table but must investigate over columns
	// try all columns to find right wID column for execute command
	if (m_nCurrentRow < m_Objects.GetSize())
	{
		CLayout* pRow = (CLayout*) m_Objects[m_nCurrentRow];
		BaseObj* pCell = pRow->FindByID(wID);
		if (pCell)
			return pCell->ExecCommand(wID, pRDEmanager);
	}

	// no ID found in this table
	return FALSE;
}

//------------------------------------------------------------------------------
void Repeater::ResetCounters()
{
	m_nCurrentRow = 0;
}

//------------------------------------------------------------------------------
void Repeater::DisableData()
{
	m_Objects.DisableData();
}

//------------------------------------------------------------------------------
void Repeater::ClearDynamicAttributes()
{
	m_Objects.ClearDynamicAttributes();
}

//---------------------------------------------------------------------------
BOOL Repeater::ExistChildID (WORD wID) 
{
	return m_Objects.GetMasterObjects()->FindByID(wID) != NULL;
}

//---------------------------------------------------------------------------
int Repeater::GetFieldRow (BaseObj* pObj)
{
	return m_Objects.GetFieldRow (pObj);
}

//------------------------------------------------------------------------------
BaseObj* Repeater::GetFieldByPosition (const CPoint& pt, int& nRow)
{
	return m_Objects.GetFieldByPosition (pt, nRow);
}

//------------------------------------------------------------------------------
BaseObj* Repeater::GetMasterFieldByPosition (const CPoint& pt)
{
	return m_Objects.GetMasterFieldByPosition (pt);
}

//---------------------------------------------------------------------------
FieldRect* Repeater::GetCellFromID(int nRow, WORD wID)
{
	if (nRow < 0 || nRow >= m_Objects.GetSize())
	{
		//ASSERT(FALSE);
		return NULL;
	}
	CLayout* pRow = (CLayout*) m_Objects[nRow];
	BaseObj* pObj = pRow->FindByID(wID);
	if (!pObj)
		return NULL;
	if (pObj->IsKindOf(RUNTIME_CLASS(FieldRect)))
		return (FieldRect*) pObj;
	ASSERT(FALSE);
	return NULL;
}

//------------------------------------------------------------------------------
BOOL Repeater::IsEmptyRow(int nRow)
{
	BOOL bEmpty = TRUE;
	if (nRow < 0 || nRow >= m_Objects.GetSize())
	{
		//ASSERT(FALSE);
		return TRUE;
	}
	CLayout* pRow = (CLayout*)m_Objects[nRow];

	for (int i = 0; i < pRow->GetSize(); i++)
	{
		FieldRect* pFR = dynamic_cast<FieldRect*>(pRow->GetAt(i));
		if (!pFR) 
			continue;
		if (pFR->m_Value.m_RDEdata.IsValid())
			return FALSE;
	}

	return bEmpty;
}

//------------------------------------------------------------------------------
BOOL Repeater::Parse (ViewParser& lex)
{
	BOOL bOk = lex.ParseRepeater (m_nRows, m_nColumns);

	m_bByColumn = lex.Matched(T_COLUMN);

	bOk = bOk &&
		lex.ParseAlias		(m_wInternalID)    &&
		lex.ParseTR			(T_INTERLINE, m_nYOffset, m_nXOffset) &&
		lex.ParseRect		(m_BaseRect) &&
		lex.ParseRatio		(m_nHRatio, m_nVRatio) &&
		ParseBlock (lex);

	ASSERT_VALID(m_pDocument);
	ASSERT_VALID(m_pDocument->m_pEditorManager);
	m_pDocument->m_pEditorManager->SetLastId(m_wInternalID);

	return bOk;
}

//------------------------------------------------------------------------------
void Repeater::Unparse (ViewUnparser& ofile)
{
	//---- Template Override
	BaseRect* pDefault = m_pDefault;
	if (m_bTemplate && m_pDocument->m_Template.m_bIsSavingTemplate)
	{
		m_pDefault = NULL;
	}
	//----
	ofile.UnparseRepeater (m_nRows, m_nColumns, FALSE);

	if (m_bByColumn)
		ofile.UnparseTag (T_COLUMN, FALSE);

	ofile.UnparseAlias              (m_wInternalID, FALSE);
	ofile.UnparseTR					(T_INTERLINE, m_nYOffset, m_nXOffset, FALSE);
	ofile.UnparseRect               (m_BaseRect, FALSE);
	
	if (IsNotDefaultRatio())
		ofile.UnparseRatio			(m_nHRatio, m_nVRatio,	FALSE);

	UnparseProp(ofile);

	//----
	m_pDefault = pDefault;
}

//------------------------------------------------------------------------------
void Repeater::OnRebuild ()
{
	Rebuild	(&(m_pDocument->GetObjects()));

	UpdateDocument ();
}

//------------------------------------------------------------------------------
void Repeater::OnFitTheContent()
{
	//rettangolo con size negativa
	CRect rect(this->m_BaseRect.right, this->m_BaseRect.bottom, this->m_BaseRect.left, this->m_BaseRect.top );
	CLayout* layout = GetChildObjects();
	/* ciclo su m_Objects per capire quale sia il reattangolo minore per contenerli */
	for (int i = 0; i < layout->GetCount(); i++)
	{
		BaseObj* pBaseObj = (BaseObj*)layout->GetAt(i);
		CRect baseRect = pBaseObj->GetBaseRect();

		//left
		if (baseRect.left < rect.left)
			rect.left = baseRect.left;
		//top
		if (baseRect.top < rect.top)
			rect.top = baseRect.top;
		//right
		if (baseRect.right > rect.right)
			rect.right = baseRect.right;
		//bottom
		if (baseRect.bottom > rect.bottom)
			rect.bottom = baseRect.bottom;
	}

	SetBaseRect(rect);

	OnRebuild();
}

//------------------------------------------------------------------------------
//void Repeater::AfterAction ()
//{
//	__super::AfterAction ();
//
//	Rebuild	(&(m_pDocument->GetObjects()));
//}
void Repeater::ChangedAction ()
{
	if (m_rectStartMouseMove.Width() == m_BaseRect.Width() && m_rectStartMouseMove.Height() == m_BaseRect.Height())
	{
		m_Objects.MoveBaseRect (m_BaseRect.left - m_rectStartMouseMove.left, m_BaseRect.top - m_rectStartMouseMove.top);
	}

	Rebuild	(&(m_pDocument->GetObjects()));

	__super::ChangedAction ();
}

//------------------------------------------------------------------------------
void Repeater::MoveBaseRect	(int x1, int y1, int x2, int y2, BOOL bRepaint /*= TRUE*/, BOOL bIgnoreBorder /*= FALSE*/)
{
	CRect rc = m_BaseRect;
	m_Objects.MoveBaseRect	(x1 - rc.left, y1 - rc.top, bIgnoreBorder);

	__super::MoveBaseRect	(x1, y1, x2, y2, bRepaint, bIgnoreBorder);
}

//------------------------------------------------------------------------------
void Repeater::MoveBaseRect	(int xOffset, int yOffset, BOOL bIgnoreBorder /*= FALSE*/)
{
	m_Objects.MoveBaseRect	(xOffset, yOffset, bIgnoreBorder);

	__super::MoveBaseRect	(xOffset, yOffset, bIgnoreBorder); 
}

//------------------------------------------------------------------------------
void Repeater::MoveObjects(int xOffset, int yOffset, BOOL bIgnoreBorder /*= FALSE*/)
{
	m_Objects.MoveBaseRect(xOffset, yOffset, bIgnoreBorder);
}

//------------------------------------------------------------------------------
void Repeater::MoveObject(CSize size /*= FALSE*/)
{
	SingleItemObj::MoveObject(size);
	this->m_Objects.GetMasterObjects()->MoveBaseRect(size.cx, size.cy, TRUE);
	OnRebuild();
}

//------------------------------------------------------------------------------
BOOL Repeater::DeleteEditorEntry ()
{
	m_Objects.RemoveAll();

	// delete table entry
	if (m_wInternalID)
	{
		if (!m_pDocument->m_Layouts.ExistsFieldID(m_wInternalID, TRUE))
		{
			ASSERT_VALID(m_pDocument->m_pEditorManager);
			if (!m_pDocument->m_pEditorManager->DeleteTable(m_wInternalID))
			{
				// table wID not found must signal error but must delete table
				m_pDocument->Message(_TB("Id Repeater not found"));
			}
		}
	}

	return __super::DeleteEditorEntry ();
}

//------------------------------------------------------------------------------
BOOL Repeater::OnShowPopup (CMenu&   menu)
{
	CString s = GetDescription(FALSE);
	if (!s.IsEmpty())
		menu.AppendMenu(MF_STRING|MF_GRAYED, 0, s);
	
	menu.AppendMenu(MF_SEPARATOR);

	menu.AppendMenu (MF_STRING, ID_REPEATER_APPLY,	_TB("Rebuild"));
	menu.AppendMenu (MF_SEPARATOR);

	return TRUE;
}

//------------------------------------------------------------------------------
void Repeater::OnProperties ()
{
	CRepeaterDlg dlg(m_nRows, m_nColumns, m_nYOffset, m_nXOffset, m_bByColumn, this);
	if (dlg.DoModal() == IDOK)
	{
		m_nRows		= dlg.m_nRows;
		m_nColumns	= dlg.m_nColumns;
		m_nYOffset	= dlg.m_nYOffset;
		m_nXOffset	= dlg.m_nXOffset;
		m_bByColumn = dlg.m_bByColumn;

		OnRebuild();

		UpdateDocument ();
	}
}


//------------------------------------------------------------------------------
void Repeater::Redraw()
{
	OnRebuild();
}

//------------------------------------------------------------------------------
BOOL Repeater::Contains(BaseObj* pObj)
{
	CObject* foundObj = this->m_Objects.GetMasterObjects()->FindObjectByID(pObj->GetInternalID());
	return foundObj != NULL;
}
