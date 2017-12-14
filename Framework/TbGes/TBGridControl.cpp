#include "stdafx.h"

#include "TBGridControl.h"
#include <TbGeneric\EnumsTable.h>
#include "Dbt.h"

//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"
#include "comutil.h"
#include <TbGenlib\OslBaseInterface.h>

#ifdef _DEBUG
#undef THIS_FILE
static const char BASED_CODE THIS_FILE[] = __FILE__;
#endif

/////////////////////////////////////////////////////////////////////////////
//					CTBGridHyperlink 
/////////////////////////////////////////////////////////////////////////////
//
//==============================================================================
IMPLEMENT_DYNCREATE(CTBGridHyperlink, CBCGPGridURLItem)

//------------------------------------------------------------------------------
BOOL CTBGridHyperlink::OnClickValue (UINT uiMsg, CPoint point)
{
	if (uiMsg == WM_LBUTTONUP)
	{
		CString strValue = (LPCTSTR)(_bstr_t) m_varValue;
		m_pColInfo->DoFollowHyperlink(strValue);
	}
	return TRUE;
}

// the standard CBCGPGridURLItem would show a "link" (hand) cursor also on empty values
//------------------------------------------------------------------------------
BOOL CTBGridHyperlink::OnSetCursor () const
{
	CString strValue = (LPCTSTR)(_bstr_t) m_varValue;
	if (strValue.IsEmpty())
		return CBCGPGridItem::OnSetCursor ();
	else
		return __super::OnSetCursor();
}

////////////////////////////////////////////////////////////////////////////////
//				class CTBGridColumnInfoArray implementation
////////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
int	CTBGridColumnInfoArray::GetColumnIdx(CTBGridColumnInfo* pInfo)
{
	for (int i=0; i <= GetUpperBound(); i++)
		if (GetAt(i) == pInfo)
			return i;
	return -1;
}

//-----------------------------------------------------------------------------
void CTBGridColumnInfoArray::Add (CTBGridColumnInfo* pInfo)
{
	int nIdx = GetColumnIdx(pInfo);
	if (nIdx < 0)
		__super::Add (pInfo);
}

//-----------------------------------------------------------------------------
void CTBGridColumnInfoArray::Remove (CTBGridColumnInfo* pInfo)
{
	int nIdx = GetColumnIdx(pInfo);
	if (nIdx >= 0)
		__super::RemoveAt (nIdx);
}

//==============================================================================
//						Class CTBGridColumnInfo
//==============================================================================

//-----------------------------------------------------------------------------
CTBGridColumnInfo::CTBGridColumnInfo
	(
		const	CString&		strName,
				DataObj*		pDataObj,
		const	CString&		strColTitle,
				int				nWidth, /*= 0*/
				CRuntimeClass*	pHKLClass /*= NULL*/
	)
	:
	m_pDataObj			(pDataObj),
	m_strTitle			(strColTitle),
	m_nPixelsWidth		(0),
	m_nWidth			(nWidth),
	m_nDataInfoIdx		(-1),
	m_pHKLClass			(pHKLClass),
	m_pHotKeyLink		(NULL),
	m_pLinkedDocument	(NULL),
	m_pTBGridControl	(NULL),
	m_pMasterColumn		(NULL)
{
	this->GetNamespace().SetChildNamespace(CTBNamespace::GRIDCOLUMN, strName, GetNamespace());
}

//-----------------------------------------------------------------------------
CTBGridColumnInfo::~CTBGridColumnInfo()
{
	SAFE_DELETE(m_pHotKeyLink);
	// m_pLinkedDocument  do not need to be deleted, the HotKeyLink manages it
}

//-----------------------------------------------------------------------------
HotKeyLink* CTBGridColumnInfo::GetHotKeyLink()
{
	ASSERT_TRACE(m_pHKLClass, "Missing HKL class to create HotKeyLink");
	if (!m_pHKLClass)
		return NULL;

	if (!m_pHotKeyLink)
	{
		m_pHotKeyLink = (HotKeyLink*)m_pHKLClass->CreateObject();
		m_pHotKeyLink->AttachDocument(m_pTBGridControl->GetDocument());
	}
	return m_pHotKeyLink;
}

//-----------------------------------------------------------------------------
void CTBGridColumnInfo::DoFollowHyperlink(CString strData, BOOL bActivate /* = TRUE*/)
{
	if (!GetHotKeyLink()->IsHotLinkEnabled() || !GetHotKeyLink()->CanDoSearchOnLink())
		return;
	
	DataStr aData(strData);

	// NOTE: the linked document type should not change during the columninfo lifecycle.
	// if a "reattach" feature is needed, it must be implemented in the same way of Hyperlink.cpp

	m_pLinkedDocument = (CBaseDocument*)GetHotKeyLink()->BrowserLink(&aData, m_pLinkedDocument, NULL, bActivate);
}

//==============================================================================
//						Virtual mode callback
//==============================================================================
//------------------------------------------------------------------------------
static BOOL CALLBACK GridCallback (BCGPGRID_DISPINFO* pdi, LPARAM lp)
{
	ASSERT (pdi != NULL);

	CTBGridControl* pGridCtrl = (CTBGridControl*) lp;

	// column == -1 -> Request row info
	if (pdi->item.nCol < 0)
		pGridCtrl->SetCurrentRecord(pdi->item.nRow);

	pdi->item.dwData = (DWORD_PTR) pGridCtrl->GetCurrentRecord(pdi->item.nRow);

	return TRUE;
}

//==============================================================================
//						Class CTBGridControl
//==============================================================================
IMPLEMENT_DYNAMIC(CTBGridControl, CTBGridControlObj)

BEGIN_MESSAGE_MAP(CTBGridControl, CTBGridControlObj)
	//{{AFX_MSG_MAP(CAskRuleDlg)
	ON_MESSAGE			(UM_RECALC_CTRL_SIZE,	OnRecalcCtrlSize)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//------------------------------------------------------------------------------
CTBGridControl::CTBGridControl(const CString sName)
	:
	IOSLObjectManager(OSLType_BodyEdit),
	m_sName(sName),
	m_pParentForm(NULL),
	m_pGridDataSource(NULL),
	m_pDoc(NULL)
{
	this->SetVisualManagerColorTheme();
	
	CBCGPGridColors colors;
	colors.m_clrHorzLine = AfxGetThemeManager()->GetBESeparatorColor();
	colors.m_clrVertLine = AfxGetThemeManager()->GetBESeparatorColor();
	colors.m_clrBackground = AfxGetThemeManager()->GetEnabledControlBkgColor();
	colors.m_SelColors.m_clrBackground = AfxGetThemeManager()->GetBERowSelectedBkgColor();
	//colors.m_clrHeader = AfxGetThemeManager()->GetBackgroundColor();
	//colors.m_HeaderColors.m_clrBackground = AfxGetThemeManager()->GetBackgroundColor();
	colors.m_HeaderColors.m_clrGradient = AfxGetThemeManager()->GetBESeparatorColor();
	colors.m_HeaderColors.m_clrBorder= AfxGetThemeManager()->GetBESeparatorColor();
	colors.m_HeaderColors.m_nGradientAngle = 0;

	this->SetColorTheme(colors);
	//this->SetReadOnly(TRUE);

	this->EnableMarkSortedColumn (FALSE);
	this->EnableHeader (TRUE, BCGP_GRID_HEADER_MOVE_ITEMS /*| BCGP_GRID_HEADER_SORT*/);
	this->EnableRowHeader (TRUE);
	this->EnableLineNumbers ();

	m_pGridColumnInfoArray = new CTBGridColumnInfoArray();
}

//------------------------------------------------------------------------------
CTBGridControl::~CTBGridControl(void)
{
	SAFE_DELETE(m_pGridColumnInfoArray);
	SAFE_DELETE(m_pGridDataSource);
}

//------------------------------------------------------------------------------
LRESULT CTBGridControl::OnRecalcCtrlSize(WPARAM, LPARAM)
{
	DoRecalcCtrlSize();
	return 0L;
}

//-----------------------------------------------------------------------------
void CTBGridControl::SetParentForm (CParsedForm* pParentForm)
{
	m_pParentForm = pParentForm;
	GetInfoOSL()->m_pParent = pParentForm->GetInfoOSL();
	GetNamespace().SetChildNamespace(CTBNamespace::GRID, m_sName, pParentForm->GetNamespace());
}

//-----------------------------------------------------------------------------
void CTBGridControl::SetName(const CString& strName)
{
	if (!strName.IsEmpty())
		m_sName = strName;

	if (strName.IsEmpty() && m_sName.IsEmpty())
	{
		TRACE(_T("Empty name not allowed %s\n"), GetRuntimeClass()->m_lpszClassName);
		ASSERT(FALSE);
		m_sName = GetRuntimeClass()->m_lpszClassName;
	}
}

//------------------------------------------------------------------------------
CTBGridColumnInfo* CTBGridControl::AddColumn
	(
		const	CString&		strName, 
				DataObj*		pDataObj, 
		const	CString&		strColTitle,
				int				nWidth, /*= 0*/
				CRuntimeClass*	pHKLClass /*= NULL*/
	)
{
	CTBGridColumnInfo* pCTBGridColumnInfo = new CTBGridColumnInfo(strName, pDataObj, strColTitle, nWidth, pHKLClass);
	pCTBGridColumnInfo->Attach(this);
	
	CInfoOSL* pInfoOSL = pCTBGridColumnInfo->GetInfoOSL();
	pInfoOSL->m_pParent = GetInfoOSL();
	AfxGetSecurityInterface()->GetObjectGrant (pInfoOSL);

	if (IsOslVisible(pInfoOSL))
	{
		m_pGridColumnInfoArray->Add(pCTBGridColumnInfo);
	}

	SqlRecord* pRec = m_pGridDataSource->GetPrototypeRecord();

	// lookup data position in the current record
	// get associated field informations from record and dictionary	
	int nDataInfoIdx = -1;
	SqlRecordItem* pRecItem = NULL;
	if (pDataObj && pRec)
	{
		int idx = pRec->Lookup(pDataObj);
		pCTBGridColumnInfo->SetDataInfoIdx(idx);
		if (pCTBGridColumnInfo->GetColWidth() == 0 && idx != -1)
			pCTBGridColumnInfo->SetColWidth(pRec->GetAt(idx)->GetColumnLength());
	}

	return pCTBGridColumnInfo;
}

//------------------------------------------------------------------------------
bool CTBGridControl::IsOslVisible(CInfoOSL* pInfoOSL)
{
	return OSL_CAN_DO(pInfoOSL, OSL_GRANT_BROWSE);
}

//------------------------------------------------------------------------------
CAbstractFormDoc* CTBGridControl::GetDocument()
{
	if (m_pDoc)
	{
		return m_pDoc;
	}

	CAbstractFormView* pParentView = NULL;
	
	CWnd* pParentWnd = GetParent();
	while (pParentWnd)
	{
		pParentView = dynamic_cast<CAbstractFormView*>(pParentWnd);
		if (pParentView)
		{
			m_pDoc = pParentView->GetDocument();
			break;
		}
		pParentWnd = pParentWnd->GetParent();
	}
	return m_pDoc;
}

//------------------------------------------------------------------------------
void CTBGridControl::Customize()
{
	SqlRecord* pRecord = m_pGridDataSource->GetPrototypeRecord();
	int columns = pRecord->GetCount();
	for (int i = 0; i < columns; i++)
	{
		AddColumn(pRecord->GetAt(i)->GetColumnName(), pRecord->GetAt(i)->GetDataObj(), pRecord->GetAt(i)->GetColumnName());
	}
}

///////////////////////////////////////////////////////////////////////////////
int GetFormattingSize
	(
		CDC*		pDC,
		DataType	aDataType,
		UINT		nCols
	)
{
	int nFormatterWidth = 0;

	if (aDataType.m_wType == DATA_ENUM_TYPE)
	{
		ASSERT(aDataType.m_wTag);

		DataEnum data(aDataType.m_wTag, AfxGetEnumsTable()->GetEnumLongerItemValue(aDataType.m_wTag));
		// gli enumerativi vengono sempre formattati con il font delle form, in quanto non contengono
		// dati inseriti dall'utente
		CString strData = data.FormatData();
		nFormatterWidth = GetEditSize(pDC, AfxGetThemeManager()->GetControlFont(), strData, TRUE).cx;
	}
	else
	{
		const Formatter* pFormatter = AfxGetFormatStyleTable()->GetFormatter(aDataType, NULL);
		if (pFormatter)
			nFormatterWidth = const_cast<Formatter*>(pFormatter)->GetInputWidth(pDC, nCols, AfxGetThemeManager()->GetControlFont()).cx; 
	}

	return nFormatterWidth;	// valore precalcolato dal formattatore associato
}

//------------------------------------------------------------------------------
void CTBGridControl::CreateAllColumns()
{
	int columns = this->m_pGridColumnInfoArray->GetCount();
	if (columns == 0)
		return; // no columns yet

	CDC* pDC = GetDC();
	for (int i = 0; i < columns; i++)
	{
		CTBGridColumnInfo* pInfo = this->m_pGridColumnInfoArray->GetAt(i);

		int nPixelsWidth = max (
									::GetFormattingSize(pDC, pInfo->GetDataObj()->GetDataType(), pInfo->GetColWidth()),
									::GetFormattingSize(pDC, DataType::String, pInfo->GetTitle().GetLength())
								);

		this->InsertColumn(i, pInfo->GetColumnName(), nPixelsWidth > 0 ? nPixelsWidth : 80);
		this->SetColumnName(i, pInfo->GetTitle());
		this->SetHeaderAlign(i, HDF_CENTER);
		if (pInfo->GetDataObj()->IsKindOf(RUNTIME_CLASS(DataDbl)))
			SetColumnAlign(i, HDF_RIGHT);
	}
	ReleaseDC(pDC);
}

//------------------------------------------------------------------------------
void CTBGridControl::SetDataSource(DBTSlaveBuffered* pDBTSlaveBuffered)
{
	this->m_pGridDataSource = new DBTSlaveBufferedGridDataSource(pDBTSlaveBuffered, this);

	this->Attach();
}

//------------------------------------------------------------------------------
void CTBGridControl::SetDataSource(SqlTable* pSqlTable)
{
	this->m_pGridDataSource = new SqlTableGridDataSource(pSqlTable, this);

	this->Attach();
}

//------------------------------------------------------------------------------
void CTBGridControl::SetDataSource(RecordArray* records)
{
	this->m_pGridDataSource = new RecordArrayGridDataSource(records, this);

	this->Attach();
}

//------------------------------------------------------------------------------
void CTBGridControl::Attach()
{
	AfxGetSecurityInterface()->GetObjectGrant (GetInfoOSL());
	
	this->m_pGridColumnInfoArray->RemoveAll();

	this->Customize();

	CAbstractFormDoc* pDoc = GetDocument(); 
	if (pDoc)
		pDoc->CustomizeGridControl(this);

	this->CreateAllColumns();

	InitSizeInfo(this);
}

//------------------------------------------------------------------------------
void CTBGridControl::Reload()
{
	ASSERT(this->m_pGridDataSource);

	this->EnableGridLines(TRUE);
	this->RemoveAll();

	this->m_pGridDataSource->Reload();

	this->AdjustLayout();
}

//------------------------------------------------------------------------------
void CTBGridControl::OnUpdateControls(BOOL bParentIsVisible)
{
	Reload();
}

//-----------------------------------------------------------------------------
void CTBGridControl::AddRow(SqlRecord* pRec)
{
	CBCGPGridRow*		pRow = NULL;
	CTBGridColumnInfo*	colInfo = NULL;
	DataObj*			obj = NULL;

	int columns = GetColumnsCount();

	pRow = CreateRow (columns);
	for (int i = 0; i < columns; i++)
	{
		colInfo = GetColumnInfo(i);
			
		obj = pRec->GetDataObjAt(colInfo->GetDataInfoIdx());
		if (obj)
		{
			if (!obj->IsKindOf(RUNTIME_CLASS(DataBool)))
			{
				CString str;
				if (!colInfo->GetMasterColumn())
					str = obj->FormatData();
				else
				{
					// automatic HKL description
					DataObj* objKey = pRec->GetDataObjAt(colInfo->GetMasterColumn()->GetDataInfoIdx());
					colInfo->GetMasterColumn()->GetHotKeyLink()->FindRecord(objKey);
					str = colInfo->GetMasterColumn()->GetHotKeyLink()->GetDescriptionDataObj()->FormatData();
				}

				if (colInfo->GetHKLClass())
				{
					pRow->ReplaceItem (i, new CTBGridHyperlink(str, colInfo), FALSE, TRUE);
					pRow->GetItem(i)->AllowEdit(FALSE);
				}
				else
					pRow->GetItem(i)->SetValue (_variant_t(str));
			}
			else
			{
				pRow->ReplaceItem (i, new CBCGPGridCheckItem (*((DataBool*)obj) ? true : false ), FALSE, TRUE);
			}
			pRow->GetItem(i)->SetReadOnly(IsColumnReadOnly(i));
		}
	}

	__super::AddRow (pRow, FALSE);
}

//------------------------------------------------------------------------------
void CTBGridControl::SetCurrentRecord(int nRow)
{
	m_pCurrentRecord = m_pGridDataSource->GetRecordAt(nRow);
}

//------------------------------------------------------------------------------
void CTBGridControl::EnableVirtualMode()
{
	__super::EnableVirtualMode (GridCallback, (LPARAM) this);
	SetVirtualRows (0);
}

//------------------------------------------------------------------------------
CBCGPGridItem* CTBGridControl::OnCreateVirtualItem (BCGPGRID_DISPINFO *pdi)
{
	// if the virtual mode is enabled, the grid callback must set the SqlRecord* 
	// of the referred row in pdi->item.dwData
	ASSERT_TRACE(pdi->item.dwData,"In virtual mode the grid callback must set the SqlRecord* into pdi->item.dwData");
	if (!pdi->item.dwData)
		return __super::OnCreateVirtualItem(pdi);

	SqlRecord* pRec = DYNAMIC_DOWNCAST(SqlRecord, (CObject*)pdi->item.dwData);
	ASSERT_TRACE(pRec,"pdi->item.dwData does not contain a valid SqlRecord*");
	if (!pRec)
		return __super::OnCreateVirtualItem(pdi);

	CBCGPGridItem* pItem = NULL;

	CTBGridColumnInfo* colInfo = GetColumnInfo(pdi->item.nCol);
	DataObj* obj = pRec->GetDataObjAt(colInfo->GetDataInfoIdx());
	if (!obj->IsKindOf(RUNTIME_CLASS(DataBool)))
	{
		CString str;
		if (!colInfo->GetMasterColumn() || !colInfo->GetMasterColumn()->GetHotKeyLink())
			str = obj->FormatData();
		else
		{
			// automatic HKL description
			DataObj* objKey = pRec->GetDataObjAt(colInfo->GetMasterColumn()->GetDataInfoIdx());
			colInfo->GetMasterColumn()->GetHotKeyLink()->FindRecord(objKey);
			str = colInfo->GetMasterColumn()->GetHotKeyLink()->GetDescriptionDataObj()->FormatData();
		}
		if (colInfo->GetHKLClass())
		{
			pItem = new CTBGridHyperlink(str, colInfo);
			pItem->AllowEdit(FALSE);
		}
		else
		{
			pItem = __super::OnCreateVirtualItem(pdi);
			pItem->SetValue (_variant_t(str));
		}
	}
	else
		pItem = new CBCGPGridCheckItem (*((DataBool*)obj) ? true : false );
		
	pItem->SetReadOnly(IsColumnReadOnly(pdi->item.nCol));

	return pItem;
}
