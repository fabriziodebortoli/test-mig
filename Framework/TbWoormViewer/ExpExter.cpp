
#include "stdafx.h"

#include <TbNameSolver\Chars.h>

#include <TbGeneric\GeneralFunctions.h>
#include <TbGeneric\tools.h>
#include <TbGeneric\DataTypesFormatters.h>

#include <TbParser\Parser.h>
#include <TbGenlib\const.h>
#include <TbGenlib\generic.hjson> //JSON AUTOMATIC UPDATE

#include <TbGenlibUI\SettingsTableManager.h>
#include <TbGenlibManaged\MExport.h>

#include "baseobj.h"
#include "table.h" 
#include "cell.h"
#include "column.h"
#include "rectobj.h"
#include "woormdoc.h"
#include "export.h"
#include "expexter.h"
#include "export.hjson" //JSON AUTOMATIC UPDATE


// -----------------------------------------------
//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char BASED_CODE THIS_FILE[] = __FILE__;
#endif


//==============================================================================
//          Class CExportData implementation
//==============================================================================
IMPLEMENT_DYNAMIC(CExportData, CObject)

//------------------------------------------------------------------------------
CExportData::CExportData(CWoormDocMng* pWoormDoc)
	:
	m_pWoormDoc(pWoormDoc),
	m_pItem(NULL),
	m_bModified(FALSE)
{
}

//------------------------------------------------------------------------------
CExportData::~CExportData()
{
	if (m_pItem)
	{
		ASSERT_VALID(m_pItem);
		delete m_pItem;
	}
}

// Aggiunge l'ID della tabella alla struttura correlata di selezione, allocandola
//  se serve. La struttura funziona da flag per inibire la selezione di campi
// liberi in contemporanea con le tabelle. E' possibile selezionare solo una tabella
// alla volta. Della tabella posso selezionare tutto, solo alcune colonne o solo
// alcune righe
//------------------------------------------------------------------------------
BOOL CExportData::AddRemoveTable(BaseObj* pBaseObj, CPoint point, BOOL bSelForTitles /*= FALSE*/, BOOL bClickAsShift /*= FALSE*/)
{
	// se c'e' gia' uno o piu' FieldRect non posso aggiungere la tabella
	if (m_pItem && m_pItem->IsKindOf(RUNTIME_CLASS(CWordArray)))
		return FALSE;

	WORD wID = pBaseObj->GetInternalID();

	if (!m_pItem)
		m_pItem = new CExportTableItem(wID);

	// Ammetto solo una tabella
	if (((CExportTableItem*)m_pItem)->m_nTableID != wID)
		return FALSE;

	m_bModified = TRUE;
	Table* pTable = (Table*)pBaseObj;
	CExportTableItem* pItem = (CExportTableItem*)m_pItem;
	if (pItem) ASSERT_VALID(pItem);

	int nRow = -1;
	int wAlias = 0;
	switch (pTable->GetPosition(point, nRow, &wAlias))
	{
		// Marca o smarca l'intera tabella
	case Table::POS_WHOLE_TABLE:
	{
		if (bSelForTitles)
		{
			m_bModified = FALSE;
			break;
		}
		// smarca tutte le righe e le colonne
		if
			(
				(pItem->m_Rows.GetSize() +
					pItem->m_SelectedCol.GetSize()) > 0
				)
		{
			delete m_pItem;
			m_pItem = NULL;

			return m_bModified;
		}

		// Aggiunge tutte le righe e le colonne
		int i = 0;
		for (i = 0; i <= pTable->LastColumn(); i++)
		{
			FormatIdx	nFmtIdx = pTable->GetColumnFormatIdx(i);
			DataType 	SelDataType = m_pWoormDoc->m_pFormatStyles->GetDataType(nFmtIdx);
			pItem->m_SelectedCol.Add(CSelectedColumn(i, pTable->GetColumn(i)->GetInternalID(), FALSE));
		}
		for (i = 0; i <= pTable->LastRow(); i++)
			pItem->m_Rows.Add((WORD)i);

		break;
	}

	case Table::POS_COLUMN:
	{
		ASSERT(nRow == -1);
		int idxCol = pTable->GetIdxColFromAlias(wAlias);
		if (idxCol == -1)
		{
			ASSERT(FALSE);
			break;
		}
		BOOL bColFound = FALSE;
		BOOL bAlternateColFound = FALSE;

		int i = 0;
		for (i = 0; i < pItem->m_SelectedCol.GetSize(); i++)
			if (pItem->m_SelectedCol[i].m_ColIdx == idxCol)
			{
				if (pItem->IsAlternate(i))
					bAlternateColFound = TRUE;
				pItem->m_SelectedCol.RemoveAt(i);
				bColFound = TRUE;
				break;
			}

		FormatIdx	nFmtIdx = pTable->GetColumnFormatIdx(idxCol);
		DataType 	SelDataType = m_pWoormDoc->m_pFormatStyles->GetDataType(nFmtIdx);

		// Non trova allora aggiungo l'ID e se non ci sono righe le seleziona tutte
		if (!bColFound || (!bAlternateColFound && bClickAsShift))
		{
			pItem->m_SelectedCol.Add(CSelectedColumn(idxCol, pTable->GetColumn(idxCol)->GetInternalID(), (!bAlternateColFound && bClickAsShift)));

			if (pItem->m_Rows.GetSize() == 0)
				for (i = 0; i <= pTable->LastRow(); i++)
					pItem->m_Rows.Add((WORD)i);
		}
	}
	break;

	case Table::POS_ROW:

		if (bSelForTitles)
		{
			m_bModified = FALSE;
			break;
		}

		// Selezione righe abilitata solo se ci sono colonne...
		if (pItem->m_SelectedCol.GetSize())
		{
			BOOL bFound = FALSE;
			int	 nArrayIdx = 0;
			for (int i = 0; i <= pItem->m_Rows.GetUpperBound(); i++)
			{
				if (pItem->m_Rows[i] == (WORD)nRow)
				{
					pItem->m_Rows.RemoveAt(i);
					bFound = TRUE;
					break;
				}
				nArrayIdx = i;
				if (pItem->m_Rows[i] > (WORD)nRow)
					break;
			}
			// Non trova allora aggiungo l'ID
			if (!bFound)
				pItem->m_Rows.InsertAt(nArrayIdx, (WORD)nRow);
		}
		break;
	}
	// se dopo la cancellazione di righe e colonne allora posso eliminare la
	// struttura base per abilitare eventuali FieldRect	
	if (pItem->m_SelectedCol.GetSize() == 0)
	{
		delete m_pItem;
		m_pItem = NULL;
	}

	return m_bModified;
}

//------------------------------------------------------------------------------
void CExportData::AutoAddTable(CArray<CExportTableItem*>& m_parTables )
{
	for (int i = 0; i <= m_pWoormDoc->GetObjects().GetUpperBound(); i++)
	{
		BaseObj* pObj = m_pWoormDoc->GetObjects()[i];

		if (pObj->IsKindOf(RUNTIME_CLASS(Table)))
		{
			WORD wID = pObj->GetInternalID();
			m_pItem = new CExportTableItem(wID);
			
			Table* pTable = (Table*)pObj;
			CExportTableItem* pItem = (CExportTableItem*)m_pItem;

			if (pItem->m_SelectedCol.GetSize() == 0)
			{
				// Marca l'intera tabella: aggiunge tutte le righe e le colonne
				for (int ii = 0; ii <= pTable->LastColumn(); ii++)
					pItem->m_SelectedCol.Add(CSelectedColumn(ii, pTable->GetColumn(ii)->GetInternalID(), FALSE));

				for (int iii = 0; iii <= pTable->LastRow(); iii++)
					pItem->m_Rows.Add(iii);
			}

			m_parTables.Add(pItem);
			return;
		}
	}
}

//------------------------------------------------------------------------------
BOOL CExportData::AutoAddAllFields()
{
	//se c'e' gia' una tabella non posso aggiungerlo
	//if (m_pItem && m_pItem->IsKindOf(RUNTIME_CLASS(CExportTableItem)))
	//	return FALSE;

	SAFE_DELETE(m_pItem);
	m_pItem = new CWordArray();

	for (int i = 0; i <= m_pWoormDoc->GetObjects().GetUpperBound(); i++)
	{
		BaseObj* pBaseObj = m_pWoormDoc->GetObjects()[i];

		WORD		wID = pBaseObj->GetInternalID();
		if (wID > SpecialReportField::REPORT_LOWER_SPECIAL_ID)
			continue;

		if (pBaseObj->IsKindOf(RUNTIME_CLASS(FieldRect)))
		{
			if (!AddRemoveField(pBaseObj))
			{
				ASSERT(FALSE);
				return FALSE;
			}
		}
	}

	return TRUE;
}

//------------------------------------------------------------------------------
// Aggiunge l'ID del FieldRect nel vettore relativo, allocandolo se serve. Lo
// stesso vettore funziona da flag per inibire la selezione di tabelle in contemporanea
// con campi liberi (viceversa per le tabelle)
BOOL CExportData::AddRemoveField(BaseObj* pBaseObj)
{
	// se c'e' gia' una tabella non posso aggiungerlo
	if (m_pItem && m_pItem->IsKindOf(RUNTIME_CLASS(CExportTableItem)))
		return FALSE;

	WORD		wID = pBaseObj->GetInternalID();
	FormatIdx	nFmtIdx = ((FieldRect*)pBaseObj)->GetFormatIdx();
	DataType 	SelDataType = m_pWoormDoc->m_pFormatStyles->GetDataType(nFmtIdx);

	/* @@Riccardo: per l'esportazione verso il chart vengono tolte in seguito
	if
	(
	SelDataType.m_wType != DATA_INT_TYPE 	&&
	SelDataType.m_wType != DATA_LNG_TYPE 	&&
	SelDataType.m_wType != DATA_DBL_TYPE 	&&
	SelDataType.m_wType != DATA_MON_TYPE 	&&
	SelDataType.m_wType != DATA_PERC_TYPE	&&
	SelDataType.m_wType != DATA_QTA_TYPE
	)
	return FALSE;
	*/

	if (!m_pItem)
		m_pItem = new CWordArray();

	CWordArray* pItem = (CWordArray*)m_pItem;
	m_bModified = TRUE;

	for (int i = 0; i <= pItem->GetUpperBound(); i++)
	{
		if (pItem->GetAt(i) == wID)
		{
			pItem->RemoveAt(i);
			if (pItem->GetSize() == 0)
			{
				delete m_pItem;
				m_pItem = NULL;
			}
			return m_bModified;
		}
	}

	pItem->Add(wID);
	return m_bModified;
}

//------------------------------------------------------------------------------
BOOL CExportData::AddRemoveElement(BaseObj* pBaseObj, CPoint point, BOOL bSelForTitles /*= FALSE*/, BOOL bClickAsShift /*= FALSE*/)
{
	CDC dcDummy; //used only as parameter for GetDrawMode(CDC*) method 
	if (pBaseObj->IsKindOf(RUNTIME_CLASS(Table)))		
		return AddRemoveTable(pBaseObj, point, bSelForTitles, bClickAsShift);

	if (
		pBaseObj->IsKindOf(RUNTIME_CLASS(FieldRect))
		&&
		!(((FieldRect*)pBaseObj)->GetDrawMode(&dcDummy) == BaseRect::HIDDEN)
		&&
		!(((FieldRect*)pBaseObj)->GetDrawMode(&dcDummy) == BaseRect::HIDDENEDIT)
		)
		return AddRemoveField(pBaseObj);

	// Elementi senza dati non sono accettati
	return FALSE;
}

//------------------------------------------------------------------------------
BOOL CExportData::IncludeField(WORD wID) const
{
	// se c'e' gia' uno o piu' FieldRect non posso aggiungere la tabella
	if (!m_pItem || m_pItem->IsKindOf(RUNTIME_CLASS(CExportTableItem)))
		return FALSE;

	CWordArray* pItem = (CWordArray*)m_pItem;

	for (int i = 0; i <= pItem->GetUpperBound(); i++)
	{
		if (pItem->GetAt(i) == wID)
			return TRUE;
	}

	return FALSE;
}

//------------------------------------------------------------------------------
BOOL CExportData::IncludeTable(WORD wID) const
{
	// se c'e' gia' uno o piu' FieldRect non posso aggiungere la tabella
	if (!m_pItem || m_pItem->IsKindOf(RUNTIME_CLASS(CWordArray)))
		return FALSE;

	return ((CExportTableItem*)m_pItem)->m_nTableID == wID;
}

//------------------------------------------------------------------------------
BOOL CExportData::IncludeColumn(int idxCol) const
{
	ASSERT(m_pItem);
	ASSERT(m_pItem->IsKindOf(RUNTIME_CLASS(CExportTableItem)));

	CExportTableItem* pItem = (CExportTableItem*)m_pItem;

	if (IsTitlesColumn(idxCol))
		return TRUE;

	for (int i = 0; i <= pItem->m_SelectedCol.GetUpperBound(); i++)
		if (pItem->m_SelectedCol[i].m_ColIdx == idxCol)
			return TRUE;

	return FALSE;
}

//------------------------------------------------------------------------------
BOOL CExportData::IsTitlesColumn(int col) const
{
	ASSERT(m_pItem);
	ASSERT(m_pItem->IsKindOf(RUNTIME_CLASS(CExportTableItem)));

	CExportTableItem* pItem = (CExportTableItem*)m_pItem;
	for (int i = 0; i < pItem->m_SelectedCol.GetSize(); i++)
		if (pItem->m_SelectedCol[i].m_ColIdx == col)
			return pItem->m_SelectedCol[i].m_bAlternate;

	return FALSE;
}

//------------------------------------------------------------------------------
BOOL CExportData::IncludeRow(int row) const
{
	ASSERT(m_pItem);
	ASSERT(m_pItem->IsKindOf(RUNTIME_CLASS(CExportTableItem)));

	CExportTableItem* pItem = (CExportTableItem*)m_pItem;
	for (int i = 0; i <= pItem->m_Rows.GetUpperBound(); i++)
		if (pItem->m_Rows[i] == row)
			return TRUE;

	return FALSE;
}

//------------------------------------------------------------------------------
BOOL CExportData::AdjustRowsNumber(int nRowsNumber)
{
	ASSERT(m_pItem);
	ASSERT(m_pItem->IsKindOf(RUNTIME_CLASS(CExportTableItem)));

	CExportTableItem* pItem = (CExportTableItem*)m_pItem;
	CWordArray TmpRowsIDs;

	int i = 0;
	for (i = 0; i <= pItem->m_Rows.GetUpperBound(); i++)
		if (pItem->m_Rows[i] < nRowsNumber)
			TmpRowsIDs.Add(pItem->m_Rows[i]);

	if (TmpRowsIDs.GetUpperBound() == pItem->m_Rows.GetUpperBound())
		return FALSE;

	pItem->m_Rows.RemoveAll();
	for (i = 0; i <= TmpRowsIDs.GetUpperBound(); i++)
		pItem->m_Rows.Add(TmpRowsIDs[i]);

	return TRUE;
}

// Se non esiste il documento da interfacciare lo istanzia e poi gli passa i dati
// Il modo dipende dal tipo di esportazione
//------------------------------------------------------------------------------
BOOL CExportData::RunExport(LPCTSTR pszFileName /*= NULL*/, BOOL bOverwrite /* = FALSE */)
{
	CWaitCursor wc;

	switch (m_ExportInfo.m_nExportType)
	{
		case TypeOfExport::EXPORT_WORDNET_MERGE_TYPE:
		case TypeOfExport::EXPORT_WORDNET_TYPE:
		case TypeOfExport::EXPORT_OPENOFFICE_ODS_TYPE:
		case TypeOfExport::EXPORT_OPENOFFICE_ODT_TYPE:
		case TypeOfExport::EXPORT_EXCELNET_TYPE:
		case TypeOfExport::EXPORT_HTML_TYPE:
		case TypeOfExport::EXPORT_XML_TYPE:
		case TypeOfExport::EXPORT_CSV_TYPE:
		case TypeOfExport::EXPORT_TXTCLIPBOARD_TYPE:
		case TypeOfExport::EXPORT_JSON_TYPE:
		case TypeOfExport::EXPORT_OPENXML_EXCEL_TYPE:
		{
			if (IsTableItem())
				return RunGenericNetTableExport(&m_ExportInfo.m_nExportType);
			else
				return RunFieldsExport(&m_ExportInfo.m_nExportType);
		}
		case TypeOfExport::EXPORT_FIELDS_TO_TEMPLATE:
		{
			AutoAddAllFields();
			return RunFieldsExportForBasilea(&m_ExportInfo.m_nExportType);
		}
		case TypeOfExport::EXPORT_FIELDS_TO_BASILEA:
		{
			AutoAddAllFields();
			return RunFieldsExportForBasilea(&m_ExportInfo.m_nExportType);
		}
		case TypeOfExport::EXPORT_XML_FULL_TYPE:
		{
			RunExportForXMLFULL();
			return TRUE;
		}
	}

	return FALSE;
}

//---------------------------------------------------------------------------------------
VOID CExportData::LoopFieldsExport(CExportDataNetFull & m_ExportDataNetFull, int aPageNum)
{
	ASSERT(m_pWoormDoc);
	ASSERT(m_pItem);
	ASSERT(m_pItem->IsKindOf(RUNTIME_CLASS(CWordArray)));

	DataObjArray* row;
	CString strValue;
	int nValidRows = 0;
	CWordArray* pItem = (CWordArray*)m_pItem;
	int	nRowCount = 1;
	DataType pDataType;
	DataObj* pDataValue;
	bool haveTitles = false;

	if (m_pWoormDoc->m_Layouts.GetCount() > 1)
	{
		AfxTBMessageBox(_TB("Report is not exportable"), MB_ICONINFORMATION | MB_OK);
		return;
	}

	//*** LOOP SULLE PAGINE ***
	for (int nPageIdx = 0; nPageIdx < aPageNum; nPageIdx++)
	{
		/*if (!m_ExportInfo.m_bCurrentPage)
		{
			CString strPageLabel = cwsprintf(_TB("Page {0-%d}"), m_ExportInfo.m_nFromPage + nPageIdx);
			if (nPageIdx || m_ExportInfo.m_nFromPage != m_ExportInfo.m_nCurrPage)
				m_pWoormDoc->ReadSelectedPage(m_ExportInfo.m_nFromPage - 1 + nPageIdx, FALSE);
		}*/

		m_pWoormDoc->ReadSelectedPage(0 + nPageIdx, TRUE);

		BOOL bOneFieldInPageValid = FALSE;
		int tot = 0;
		for (int i = 0; i <= pItem->GetUpperBound(); i++)
		{

			// Scorro gli oggetti del Documento di Woorm finchè non trovo il campo selezionato
			for (int j = 0; j <= m_pWoormDoc->GetObjects().GetUpperBound(); j++)
			{

				if (m_pWoormDoc->GetObjects()[j]->GetInternalID() == pItem->GetAt(i) && m_pWoormDoc->GetObjects()[j]->IsKindOf(RUNTIME_CLASS(FieldRect)))
				{
					FieldRect* pField = (FieldRect*)m_pWoormDoc->GetObjects()[j];
					DataStr *field = new DataStr(pField->GetTagName());

					row = new DataObjArray;

					if (pField)
					{
						bOneFieldInPageValid = TRUE;

						if (ConvertFieldDataToDataObj(m_pWoormDoc, pDataType, pDataValue, pField, nPageIdx, 0, i))
							row->Add(pDataValue);
						else
							row->Add(NULL);

						row->Add(field);
						tot = tot + 1;

#ifdef _DEBUG
						if (m_ExportDataNetFull.m_SingleCells.GetSize() > 0)
						{
							ASSERT(m_ExportDataNetFull.m_SingleCells.GetAt(m_ExportDataNetFull.m_SingleCells.GetSize() - 1) != row);
						}
#endif
						m_ExportDataNetFull.m_SingleCells.Add(row);
					}
				}
			}
		}
		//		m_ExpData.m_arItemForPages.Add(tot);

		if (bOneFieldInPageValid)
			nValidRows++;
	}
	if (!m_ExportInfo.m_bCurrentPage && m_ExportInfo.m_nToPage != m_ExportInfo.m_nCurrPage)
		m_pWoormDoc->ReadSelectedPage(m_ExportInfo.m_nCurrPage - 1);


	if (m_ExportInfo.m_bSync)
		m_ExportInfo.m_nOffsetRow += nValidRows;
}
//---------------------------------------------------------------------------------------
BOOL CExportData::RunFieldsExport(TypeOfExport* pExport)
{
	ASSERT(m_pWoormDoc);
	ASSERT(m_pItem);
	ASSERT(m_pItem->IsKindOf(RUNTIME_CLASS(CWordArray)));

	int	nPagesToExport = m_ExportInfo.m_nToPage - m_ExportInfo.m_nFromPage + 1;
	DataObjArray* row;
	CString strValue;
	int nValidRows = 0;
	CWordArray* pItem = (CWordArray*)m_pItem;
	int	nRowCount = 1;
	int	nColumnCount = pItem->GetSize() * nPagesToExport;
	DataType pDataType;
	DataObj* pDataValue;
	bool haveTitles = false;

	CExportDataNet  m_ExpData;

	m_ExpData.m_Table.m_arColumnTitles.Add(_T(""));

	m_ExpData.m_Sheet				= m_ExportInfo.m_strSheetName;
	m_ExpData.firstColumn			= m_ExportInfo.m_nOffsetCol;
	m_ExpData.firstRow				= m_ExportInfo.m_nOffsetRow;
	m_ExpData.m_sDataFormat			= m_ExportInfo.m_sDateTypeFormat;
	m_ExpData.m_SDataTimeFormat		= m_ExportInfo.m_sDateTimeTypeFormat;
	m_ExpData.m_STimeFormat			= m_ExportInfo.m_sTimeTypeFormat;
	m_ExpData.m_sFileName			= m_ExportInfo.m_strFileName;
	m_ExpData.m_bRepeatColumnTitles = m_ExportInfo.m_bRepeatColumnTitles;
	m_ExpData.m_bExportTotals		= FALSE;
	m_ExpData.m_bAutoSave			= m_ExportInfo.m_bAutoSave;
	m_ExpData.m_strCSVSep			= m_ExportInfo.m_strCSVSep;
	m_ExpData.m_bExportSubTotals	= FALSE;
	m_ExpData.m_FileFormatType		= m_ExportInfo.m_FileFormat;

	if (m_pWoormDoc->m_Layouts.GetCount() > 1)
	{
		AfxTBMessageBox(_TB("Report is not exportable"), MB_ICONINFORMATION | MB_OK);
		return true;
	}

	//*** LOOP SULLE PAGINE ***
	for (int nPageIdx = 0; nPageIdx < nPagesToExport; nPageIdx++)
	{
		if (!m_ExportInfo.m_bCurrentPage)
		{
			CString strPageLabel = cwsprintf(_TB("Page {0-%d}"), m_ExportInfo.m_nFromPage + nPageIdx);
			if (nPageIdx || m_ExportInfo.m_nFromPage != m_ExportInfo.m_nCurrPage)
				m_pWoormDoc->ReadSelectedPage(m_ExportInfo.m_nFromPage - 1 + nPageIdx, FALSE);
		}

		BOOL bOneFieldInPageValid = FALSE;
		int tot = 0;
		for (int i = 0; i <= pItem->GetUpperBound(); i++)
		{
			// Scorro gli oggetti del Documento di Woorm finchè non trovo il campo selezionato
			for (int j = 0; j <= m_pWoormDoc->GetObjects().GetUpperBound(); j++)
			{
				if (m_pWoormDoc->GetObjects()[j]->GetInternalID() == pItem->GetAt(i) && m_pWoormDoc->GetObjects()[j]->IsKindOf(RUNTIME_CLASS(FieldRect)))
				{
					FieldRect* pField = (FieldRect*)m_pWoormDoc->GetObjects()[j];
					DataStr *field = new DataStr(pField->GetTagName());

					row = new DataObjArray;

					if (pField)
					{
						bOneFieldInPageValid = TRUE;

						if (ConvertFieldDataToDataObj(m_pWoormDoc, pDataType, pDataValue, pField, nPageIdx, 0, i))
							row->Add(pDataValue);
						else
							row->Add(NULL);

						tot = tot + 1;
#ifdef _DEBUG
						if (m_ExpData.m_Table.m_Cells.GetSize() > 0)
						{
							ASSERT(m_ExpData.m_Table.m_Cells.GetAt(m_ExpData.m_Table.m_Cells.GetSize() - 1) != row);
						}
#endif
						m_ExpData.m_Table.m_Cells.Add(row);
					}
				}
			}	
		}
//		m_ExpData.m_arItemForPages.Add(tot);

		if (bOneFieldInPageValid)
			nValidRows++;
	}
	if (!m_ExportInfo.m_bCurrentPage && m_ExportInfo.m_nToPage != m_ExportInfo.m_nCurrPage)
		m_pWoormDoc->ReadSelectedPage(m_ExportInfo.m_nCurrPage - 1);

	if (m_ExportInfo.m_bSync)
		m_ExportInfo.m_nOffsetRow += nValidRows;

	//*** PASSAGGIO AL C MANAGED ***
	MExportNet Exp;
	return Exp.Export(&m_ExpData, m_ExportInfo.m_nExportType);
}

//---------------------------------------------------------------------------------------
BOOL CExportData::ExportSingleTable(Table* pTable, int nPagesToExport, CExportDataNetTable & aCExportDataNetTable, BOOL aExpSubTotal, BOOL aExportTotals, CExportTableItem*	pItem)
{
	ASSERT(pTable);
	if (!pTable)
		return FALSE;

	DataType oDataType;
	BOOL bIsTailMultiLine = FALSE;
	BOOL bIsSubTotal = FALSE;
	DataObj* pDataValue = NULL;
//	CExportTableItem*	pItem = (CExportTableItem*)m_pItem;
	BOOL bMultiLayout = (m_pWoormDoc->m_Layouts.GetCount() > 1);
	int nAliasTable;
	int maxRows = 0;
	BOOL bValidRow = FALSE;
	bool isEmpty = TRUE;
	BOOL bMultiline = FALSE;
	DataObjArray* row;
	int nRow;
	CString sVal;
	int nCol = 0;
	int totVisibleColumns = 0;
	const TableColumn* pCol = NULL;
	int itemForPage = 0;
	int nValidRows = 0;
	CString strLabel;
	int nCharLen = 10;
	BOOL bOutputTitle = false;
	int hiddenColumns = 0;

	int nPageIdx = m_ExportInfo.m_nFromPage - 1;

	nAliasTable = pTable->GetInternalID();

	//***LOOP SULLE PAGINE ***
	for (int i =0;  i < nPagesToExport; i++)
	{
		//rileggo la pagina RDE necessaria
		m_pWoormDoc->ReadSelectedPage(0 + nPageIdx, TRUE);

		if (bMultiLayout)
		{
			//la tabella potrebbe non esserci od avere una struttura differente (numero righe e/o colonne)
			pTable = (Table*)(m_pWoormDoc->GetObjects().FindByID(nAliasTable));

			if (pTable == NULL || !pTable->IsKindOf(RUNTIME_CLASS(Table)))
				continue;
		}

		nAliasTable = pTable->GetInternalID();
		m_pWoormDoc->UpdateViewSymbolTable();

		//*** ESPORTAZIONE DEI TITOLI ***
		if (m_ExportInfo.m_bColumnTitles && !bOutputTitle)
			ExportTitles(pItem, pTable, bMultiLayout, aCExportDataNetTable, bOutputTitle);

		//*** LOOP SULLE RIGHE ***
		maxRows = min(pTable->LastRow(), pItem->m_Rows.GetUpperBound());

		for (int nRowIdx = 0; nRowIdx <= maxRows; nRowIdx++)
		{
			hiddenColumns = 0;
			nRow = ((CExportTableItem*)pItem)->m_Rows.GetAt(nRowIdx);

			row = new DataObjArray;
			m_pWoormDoc->UpdateViewSymbolTable(pTable, nRow);

			bValidRow = FALSE;
			isEmpty = TRUE;
			bMultiline = FALSE;

			//*** LOOP COLONNE ***
			for (int nColIdx = 0; nColIdx <= pItem->m_SelectedCol.GetUpperBound(); nColIdx++)
			{
				totVisibleColumns = pItem->m_SelectedCol.GetUpperBound();
				nCol = pItem->m_SelectedCol[nColIdx].m_ColIdx;
				pCol = NULL;

				if (bMultiLayout)
				{
					WORD id = pItem->m_SelectedCol[nColIdx].m_ColID;
					pCol = pTable->GetColumnByAlias(id, &nCol);
				}
				else
				{
					if (nCol < 0 || nCol >= pTable->GetColumns().GetCount())
						continue;

					pCol = pTable->GetColumn(nCol);
					ASSERT(pCol && pCol->GetInternalID() == pItem->m_SelectedCol[nColIdx].m_ColID);
				}

				//if (!pCol || (pCol->IsHidden() && m_ExportInfo.m_nExportType == TypeOfExport::EXPORT_XML_FULL_TYPE))
				//	continue;

				if ((pCol->IsHidden() && m_ExportInfo.m_bHiddenColumns == FALSE) && isEmpty == FALSE && nColIdx == pItem->m_SelectedCol.GetUpperBound())
				{
					if (!bMultiline)
						aCExportDataNetTable.m_Cells.Add(row);

					totVisibleColumns = totVisibleColumns - 1;
					continue;
				}

				if (m_ExportInfo.m_nExportType == TypeOfExport::EXPORT_XML_TYPE || m_ExportInfo.m_nExportType == TypeOfExport::EXPORT_XML_FULL_TYPE)
				{
					if (ConvertCellDataToDataObj(m_pWoormDoc, pTable, nRow, nCol, oDataType, pDataValue, bIsTailMultiLine, bIsSubTotal))
					{
						if (pDataValue != NULL && pDataValue->IsKindOf(RUNTIME_CLASS(DataEnum)))
						{
							sVal = cwsprintf(_T("%u"), ((DataEnum*)pDataValue)->GetValue());
							delete pDataValue;
							pDataValue = new DataStr(sVal);
						}
						else if (pDataValue != NULL && pDataValue->IsKindOf(RUNTIME_CLASS(DataDate)))
						{
							pDataValue->SetFullDate();
							sVal = pDataValue->FormatDataForXML(FALSE);
							delete pDataValue;
							pDataValue = new DataStr(sVal);
						}
						else if (pDataValue != NULL && !pDataValue->IsKindOf(RUNTIME_CLASS(DataStr)))
						{
							sVal = pDataValue->FormatDataForXML(FALSE);
							delete pDataValue;
							pDataValue = new DataStr(sVal);
						}

						row->Add(pDataValue);
						isEmpty = FALSE;
					}
					else
						row->Add(NULL);
				}
				else
				{
					if ((pCol->IsHidden() && m_ExportInfo.m_bHiddenColumns == FALSE) && m_ExportInfo.m_bHiddenColumns == FALSE)
					{
						hiddenColumns = hiddenColumns + 1;
						continue;
					}
						
					if (ConvertCellDataToDataObj(m_pWoormDoc, pTable, nRow, nCol, oDataType, pDataValue, bIsTailMultiLine, bIsSubTotal))
					{
						if (!aExpSubTotal && bIsSubTotal)
						{
							row = NULL;
							break;
						}
						if (bIsTailMultiLine)
						{
							int size = aCExportDataNetTable.m_Cells.GetSize();
							int p = aCExportDataNetTable.m_Cells.GetCount();

							if (!bMultiline)
							{
								if (p > 0)
									row = aCExportDataNetTable.m_Cells[aCExportDataNetTable.m_Cells.GetCount() - 1];
								else
									break;
							}							
							CString a;
							if (m_ExportInfo.m_bMultiRows)
								a = row->GetAt(nColIdx - hiddenColumns)->Str() + _T("\n") + pDataValue->Str();
							else
								a = row->GetAt(nColIdx - hiddenColumns)->Str() + _T(" ") + pDataValue->Str();
							
							pDataValue = DataObj::DataObjCreate(oDataType);
							pDataValue->Assign(a);
							*row->GetAt(nColIdx - hiddenColumns) = *pDataValue;
							bMultiline = TRUE;
						}
						else
							row->Add(pDataValue);

						isEmpty = FALSE;
					}
					else if (!bMultiline)
						row->Add(NULL);
				}

				if (isEmpty == FALSE && nColIdx == totVisibleColumns)
				{
					if (!bMultiline)
						aCExportDataNetTable.m_Cells.Add(row);
				}
			} //end for colonne

			if (isEmpty == FALSE)
				itemForPage = itemForPage + 1;

			if (bValidRow)
			nValidRows++;
			} //end for righe di una pagina
			nPageIdx = nPageIdx + 1;
			aCExportDataNetTable.m_arItemForPages.Add(itemForPage);
			itemForPage = 0;
		}//end for pagine

	 //*** GESTIONE RIGA DEI TOTALI ***
	if (aExportTotals && aCExportDataNetTable.m_Cells.GetCount() > 0 && pTable && pTable->HasTotal())
		ExportTotals(pItem, pTable, bMultiLayout, m_pWoormDoc, aCExportDataNetTable);

	if (!m_ExportInfo.m_bCurrentPage && m_ExportInfo.m_nToPage != m_ExportInfo.m_nCurrPage)
		m_pWoormDoc->ReadSelectedPage(m_ExportInfo.m_nCurrPage - 1);

	if (m_ExportInfo.m_bSync)
		m_ExportInfo.m_nOffsetRow += nValidRows + 1;

	return TRUE;
}

//---------------------------------------------------------------------------------------
CString CExportData::RunExportForXMLFULL()
{
	ASSERT(m_pWoormDoc);

	Table* pTable = NULL; //Tabella selezinata in worm
	CExportDataNetFull m_ExportDataNetFull;
	CExportTableItem*	pItem;

	m_ExportDataNetFull.m_sReportNameSpace = m_ExportInfo.m_strReportNameSpace;
	m_ExportDataNetFull.m_sReportName = m_ExportInfo.m_strReportName;
	m_ExportInfo.m_bHiddenColumns = FALSE;

	CArray<CExportTableItem*> m_parTables;
	AutoAddTable(m_parTables);

	int i;
	BaseObj* pObj;
	for (i = 0; i <= m_pWoormDoc->GetObjects().GetUpperBound(); i++)
	{
		pObj = m_pWoormDoc->GetObjects()[i];
		if (pObj->IsKindOf(RUNTIME_CLASS(Table)))
			break;
	}

	try
	{
		
		for (int y = 0; y < m_parTables.GetSize(); y++)
		{
			pItem = (CExportTableItem*)m_parTables[y];
	
			// Scorro gli oggetti del Documento di Woorm finchè non trovo la tabella selezionata
			pTable = NULL;

			for (int i = 0; i <= m_pWoormDoc->GetObjects().GetUpperBound(); i++)
			{
				if (m_pWoormDoc->GetObjects()[i]->GetInternalID() == pItem->m_nTableID	&& m_pWoormDoc->GetObjects()[i]->IsKindOf(RUNTIME_CLASS(Table)))
				{
					CExportDataNetTable * m_ExportDataNetTable = new CExportDataNetTable();
					pTable = (Table*)m_pWoormDoc->GetObjects()[i];

					m_ExportDataNetTable->m_sTableName = pTable->GetName();
					ExportSingleTable(pTable, this->m_pWoormDoc->m_pRDEmanager->LastPage() + 1,  *m_ExportDataNetTable, FALSE, FALSE, pItem);
					m_ExportDataNetFull.m_arTables.Add(m_ExportDataNetTable);
					break;
				}
			}
		}

		AutoAddAllFields();

		LoopFieldsExport(m_ExportDataNetFull, this->m_pWoormDoc->m_pRDEmanager->LastPage() + 1);

		m_pWoormDoc->GetAskDialogsParameters(m_ExportDataNetFull.m_arAskDialogs);

		//*** PASSAGGIO AL C MANAGED ***
		MExportNet Exp;
		return Exp.ExportFullXml(&m_ExportDataNetFull, m_ExportInfo.m_nExportType);
	}
	catch (_com_error & e)
	{
		AfxMessageBox(DecodeComException(&e));
	}
	catch (CException* e)
	{
		AfxTBMessageBox(_TB("Data Format not valid, Export data stopped"), MB_ICONERROR | MB_OK);
		e->Delete();
	}
	return FALSE;
}

//---------------------------------------------------------------------------------------
BOOL CExportData::RunFieldsExportForBasilea(TypeOfExport* pExport)
{
	ASSERT(m_pWoormDoc);
	ASSERT(m_pItem);
	ASSERT(m_pItem->IsKindOf(RUNTIME_CLASS(CWordArray)));

	DataObjArray* row;
	CString strValue;
	int nValidRows = 0;
	CWordArray* pItem = (CWordArray*)m_pItem;
	int	nPagesToExport = m_ExportInfo.m_nToPage - m_ExportInfo.m_nFromPage + 1;
	int	nRowCount = 1;
	int	nColumnCount = pItem->GetSize() * nPagesToExport;
	DataType pDataType;
	DataObj* pDataValue;
	bool haveTitles = false;
	CExportDataNet  m_ExpData;
	bool forAdd = false;
	
	m_ExpData.m_Sheet = m_ExportInfo.m_strSheetName;
	m_ExpData.firstColumn = m_ExportInfo.m_nOffsetCol;
	m_ExpData.firstRow = m_ExportInfo.m_nOffsetRow;
	m_ExpData.m_sDataFormat = m_ExportInfo.m_sDateTypeFormat;
	m_ExpData.m_SDataTimeFormat = m_ExportInfo.m_sDateTimeTypeFormat;
	m_ExpData.m_STimeFormat = m_ExportInfo.m_sTimeTypeFormat;
	m_ExpData.m_sFileName = m_ExportInfo.m_strFileName;
	m_ExpData.m_bRepeatColumnTitles = m_ExportInfo.m_bRepeatColumnTitles;
	m_ExpData.m_bExportTotals = FALSE;
	m_ExpData.m_bExportSubTotals = FALSE;
	m_ExpData.m_FileFormatType = m_ExportInfo.m_FileFormat;

	if (m_pWoormDoc->m_Layouts.GetCount() > 1)
	{
		AfxTBMessageBox(_TB("Report is not exportable"), MB_ICONINFORMATION | MB_OK);
		return true;
	}

//	m_ExpData.m_arColumnTitles.Add(_T(""));
//	m_ExpData.m_arColumnTitles.Add(_T(""));

	//*** LOOP SULLE PAGINE ***
	for (int nPageIdx = 0; nPageIdx < nPagesToExport; nPageIdx++)
	{
		if (!m_ExportInfo.m_bCurrentPage)
		{
			CString strPageLabel = cwsprintf(_TB("Page {0-%d}"), m_ExportInfo.m_nFromPage + nPageIdx);
			if (nPageIdx || m_ExportInfo.m_nFromPage != m_ExportInfo.m_nCurrPage)
				m_pWoormDoc->ReadSelectedPage(m_ExportInfo.m_nFromPage - 1 + nPageIdx, FALSE);
		}

		BOOL bOneFieldInPageValid = FALSE;
		int tot = 0;
		for (int i = 0; i <= pItem->GetUpperBound(); i++)
		{
			// Scorro gli oggetti del Documento di Woorm finchè non trovo il campo selezionato
			for (int j = 0; j <= m_pWoormDoc->GetObjects().GetUpperBound(); j++)
			{

				if (m_pWoormDoc->GetObjects()[j]->GetInternalID() == pItem->GetAt(i) && m_pWoormDoc->GetObjects()[j]->IsKindOf(RUNTIME_CLASS(FieldRect)))
				{
					FieldRect* pField = (FieldRect*)m_pWoormDoc->GetObjects()[j];
					DataStr *field = new DataStr(pField->GetTagName());
					
					row = new DataObjArray;

					if (pField)
					{
						bOneFieldInPageValid = TRUE;

						if (ConvertFieldDataToDataObj(m_pWoormDoc, pDataType, pDataValue, pField, nPageIdx, 0, i))
							row->Add(pDataValue);
						else
							row->Add(NULL);

						row->Add(field);
						tot = tot + 1;
#ifdef _DEBUG
						if (m_ExpData.m_Table.m_Cells.GetSize() > 0)
						{
							ASSERT(m_ExpData.m_Table.m_Cells.GetAt(m_ExpData.m_Table.m_Cells.GetSize() - 1) != row);
						}
#endif
						m_ExpData.m_Table.m_Cells.Add(row);
					}
				}
			}
		}
//		m_ExpData.m_arItemForPages.Add(tot);

		if (bOneFieldInPageValid)
			nValidRows++;
	}
	if (!m_ExportInfo.m_bCurrentPage && m_ExportInfo.m_nToPage != m_ExportInfo.m_nCurrPage)
		m_pWoormDoc->ReadSelectedPage(m_ExportInfo.m_nCurrPage - 1);

	if (m_ExportInfo.m_bSync)
		m_ExportInfo.m_nOffsetRow += nValidRows;

	//*** PASSAGGIO AL C MANAGED ***
	MExportNet Exp;

	BOOL b = Exp.Export(&m_ExpData, m_ExportInfo.m_nExportType);

	return b;
}

//------------------------------------------------------------------------------
BOOL CExportData::ConvertFieldDataToDataObj(CWoormDocMng* pWDoc, DataType& oDataType, DataObj*& pDataValue, FieldRect* pField, int nPageIdx, int nRowIdx, int nColIdx)
{
	FormatIdx	nFmtIdx = pField->GetFormatIdx();
	Formatter *	pFormatter = m_pWoormDoc->GetFormatter(nFmtIdx);
	oDataType = pWDoc->m_pFormatStyles->GetDataType(nFmtIdx);

	if (pFormatter == NULL)
		return FALSE;

	if (!pField->GetText().IsEmpty())
	{
		RDEData* ptrRDE = pField->GetRDEValue();
		if (ptrRDE == NULL)
		{
			TRACE("Unable to retrieve Cell value. Row = %d, Col = %d.\n", nRowIdx, nColIdx);
			return FALSE;
		}
		if (oDataType == DataType::Enum)
		{
			DWORD dwEnum = *(DWORD*)ptrRDE->GetData();
			oDataType.m_wTag = GET_TAG_VALUE(dwEnum);
		}
		pDataValue = DataObj::DataObjCreate(oDataType);
		pDataValue->Assign(*ptrRDE);
		return TRUE;
	}
	return FALSE;
}

//-----------------------------------------------------------------------------
BOOL CExportData::ExportTitles(CExportTableItem* pItem, Table* pTable, BOOL  bMultiLayout, CExportDataNetTable&  m_ExpData, BOOL & titleOk)
{
	CString strLabel;
	int nCol = 0;
	const TableColumn* pCol = NULL;

	for (int nColIdx = 0; nColIdx <= pItem->m_SelectedCol.GetUpperBound(); nColIdx++)
	{
		strLabel.Empty();
		nCol = pItem->m_SelectedCol[nColIdx].m_ColIdx;

		if (bMultiLayout)
		{
			WORD id = pItem->m_SelectedCol[nColIdx].m_ColID;
			pCol = pTable->GetColumnByAlias(id, &nCol);
		}
		else
		{
			if (nCol < 0 || nCol >= pTable->GetColumns().GetCount())
				continue;

			pCol = pTable->GetColumn(nCol);
			ASSERT(pCol && pCol->GetInternalID() == pItem->m_SelectedCol[nColIdx].m_ColID);
		}

		if (pCol)
		{
			if ((pCol->IsHidden() && m_ExportInfo.m_bHiddenColumns == FALSE) && m_ExportInfo.m_bHiddenColumns == FALSE)
				continue;

			strLabel = pTable->GetDynamicColumnLocalizedTitleText(nCol);
			strLabel.Replace(_T("\n"), _T(" "));
			strLabel.Replace(_T("\r"), _T(" "));
			strLabel.Replace(_T("\r\n"), _T(" "));

			m_ExpData.m_arColumnTitles.Add(strLabel);
			m_ExpData.m_arAllColumnNames.Add(pCol->GetFieldName());
		}
	}
	titleOk = true;
	return true;
}

//-----------------------------------------------------------------------------
BOOL CExportData::ExportTotals(CExportTableItem* pItem, Table* pTable, BOOL  bMultiLayout, CWoormDocMng* m_pWoormDoc, CExportDataNetTable&  m_ExpData)
{
	int nCol = 0;
	const TableColumn* pCol = NULL;
	DataType oDataType;
	BOOL bIsTailMultiLine = FALSE;
	BOOL bIsSubTotal = FALSE;
	DataObj* pDataValue = NULL;

	m_pWoormDoc->UpdateViewSymbolTable(pTable, -1);

	for (int nColIdx = 0; nColIdx <= pItem->m_SelectedCol.GetUpperBound(); nColIdx++)
	{
		nCol = pItem->m_SelectedCol[nColIdx].m_ColIdx;
		if (bMultiLayout)
		{
			WORD id = pItem->m_SelectedCol[nColIdx].m_ColID;
			pCol = pTable->GetColumnByAlias(id, &nCol);
		}
		else
		{
			if (nCol < 0 || nCol >= pTable->GetColumns().GetCount())
				continue;

			pCol = pTable->GetColumn(nCol);
			ASSERT(pCol && pCol->GetInternalID() == pItem->m_SelectedCol[nColIdx].m_ColID);
		}

		if (!pCol || (pCol->IsHidden()&& m_ExportInfo.m_bHiddenColumns == FALSE))
			continue;

		if (ConvertCellDataToDataObj(m_pWoormDoc, pTable, -1, nCol, oDataType, pDataValue, bIsTailMultiLine, bIsSubTotal))
			m_ExpData.m_TotalsRow.Add(pDataValue);
		else //non ce il totale in quella colonna 
			m_ExpData.m_TotalsRow.Add(NULL);
	}
	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CExportData::RunGenericNetTableExport(TypeOfExport* typ)
{
	CExportDataNet  m_ExpData;

	Table* pTable = NULL; //Tabella selezionata in woorm
	int nValidRows = 0;
	CExportTableItem*	pItem = dynamic_cast<CExportTableItem*>(m_pItem);
	ASSERT_VALID(pItem);
	if (!m_pItem) 
		return FALSE;

	int	nPagesToExport = m_ExportInfo.m_nToPage - m_ExportInfo.m_nFromPage + 1;    

	m_ExpData.m_Sheet				= m_ExportInfo.m_strSheetName;
	m_ExpData.firstColumn			= m_ExportInfo.m_nOffsetCol;
	m_ExpData.firstRow				= m_ExportInfo.m_nOffsetRow;
	m_ExpData.m_sDataFormat			= m_ExportInfo.m_sDateTypeFormat;
	m_ExpData.m_SDataTimeFormat		= m_ExportInfo.m_sDateTimeTypeFormat;
	m_ExpData.m_STimeFormat			= m_ExportInfo.m_sTimeTypeFormat;
	m_ExpData.m_sFileName			= m_ExportInfo.m_strFileName;
	m_ExpData.m_bRepeatColumnTitles = m_ExportInfo.m_bRepeatColumnTitles;
	m_ExpData.m_bExportTotals		= m_ExportInfo.m_bExportTotals;
	m_ExpData.m_bExportSubTotals	= m_ExportInfo.m_bExportSubTotals;
	m_ExpData.m_FileFormatType		= m_ExportInfo.m_FileFormat;
	m_ExpData.m_bCSVFormat			= m_ExportInfo.m_bEncodeCSV;
	m_ExpData.m_bisClipboard		= m_ExportInfo.m_bisClipboard;
	m_ExpData.m_bAutoSave			= m_ExportInfo.m_bAutoSave;
	m_ExpData.m_strCSVSep			= m_ExportInfo.m_strCSVSep;
	try 
	{	
		// Scorro gli oggetti del Documento di Woorm finchè non trovo la tabella selezionata
		pTable = NULL;
		
		for (int i = 0; i <= m_pWoormDoc->GetObjects().GetUpperBound(); i++)
		{
			if (m_pWoormDoc->GetObjects()[i]->GetInternalID() == pItem->m_nTableID	&& m_pWoormDoc->GetObjects()[i]->IsKindOf(RUNTIME_CLASS(Table)))
			{
				pTable = (Table*) m_pWoormDoc->GetObjects()[i];
				break;
			}
		}

		ASSERT(pTable);
		if (!pTable)
			return FALSE;

		ExportSingleTable(pTable, nPagesToExport, m_ExpData.m_Table, m_ExpData.m_bExportSubTotals, m_ExpData.m_bExportTotals, pItem);

		//*** PASSAGGIO AL C MANAGED ***
		MExportNet Exp;
		BOOL b = Exp.Export(&m_ExpData, m_ExportInfo.m_nExportType);

		return b;
	} 
	catch ( _com_error & e ) 
	{
 		AfxMessageBox( DecodeComException( &e) );
	}
	catch (CException* e)
	{
		AfxTBMessageBox(_TB("Data Format not valid, Export data stopped"), MB_ICONERROR | MB_OK);
		e->Delete();
	}
	return FALSE;
}

//-----------------------------------------------------------------------------
BOOL CExportData::ConvertCellDataToString
	(
		CWoormDocMng* pWDoc, CString& strValue, Table* pTable, int nRowIdx, int nColIdx, 
		DataType& oDataType, void*& pDataValue, BOOL& bIsTailMultiLine, BOOL bPad /* = FALSE */
	)
{
	strValue = "";
	
	TableCell*	pCell = pTable->GetTableCell(nRowIdx, nColIdx);
	if (!pCell)
	{
		TRACE("Unable to retrieve TableCell. Row = %d, Col = %d.\n", nRowIdx, nColIdx);
		return FALSE;
	}

	if (!pCell->IsEnabledRDEData())
	{
		return FALSE;
	}
	
	/* an.#5439
	if( pCell->GetText().IsEmpty() )
	{
		return FALSE;
	}*/

	bIsTailMultiLine = pCell->IsTailMultiLineString ();

	FormatIdx nFmtIdx		= pTable->GetColumnFormatIdx (nColIdx);
	oDataType				= pWDoc->m_pFormatStyles->GetDataType (nFmtIdx);
	Formatter*	pFormatter  = pWDoc->GetFormatter(nFmtIdx);	
	if (pFormatter == NULL) 
	{
		TRACE("Unable to retrieve Cell Formatter. Row = %d, Col = %d.\n", nRowIdx, nColIdx);
		return FALSE;
	}

	if (!pCell->GetText().IsEmpty())
	{
		pDataValue = pCell->GetRDEData ();
		if (pDataValue == NULL)
		{
			TRACE("Unable to retrieve Cell value. Row = %d, Col = %d.\n", nRowIdx, nColIdx);
			return FALSE;
		}
		pFormatter->Format ( pDataValue, strValue );
		strValue.Replace(_T("\\n"), _T(""));
	}

	if (bPad)  
	{
		int nCharLen = pFormatter->GetOutputCharLen () ;
		CString strTmp1 = strValue;
		CString strTmp2;
		strTmp2.Format(_T("%%-%ds"), nCharLen);
		strValue.Format( strTmp2, strTmp1 );
	}

	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CExportData::ConvertCellDataToDataObj
(
	CWoormDocMng* pWDoc,
	Table* pTable,
	int nRowIdx, int nColIdx,
	DataType& oDataType,
	DataObj*& pDataValue,
	BOOL& bIsTailMultiLine,
	BOOL& bIsSubTotal
)
{
	pDataValue = NULL;

	TableCell*   pCell = nRowIdx >= 0 ?
		pTable->GetTableCell(nRowIdx, nColIdx)
		:
		pTable->GetTotalCell(nColIdx)
		;
	if (!pCell)
	{
		if (nRowIdx >= 0)
		{
			TRACE("Unable to retrieve TableCell. Row = %d, Col = %d.\n", nRowIdx, nColIdx);
		}
		return FALSE;
	}

	if (!pCell->IsEnabledRDEData())
	{
		return FALSE;
	}

	/* an.#5439
	if( pCell->GetText().IsEmpty() )
	{
	return FALSE;
	}*/

	bIsTailMultiLine = pCell->IsTailMultiLineString();
	bIsSubTotal = pCell->IsSubTotal();

	FormatIdx nFmtIdx = pTable->GetColumnFormatIdx(nColIdx);
	oDataType = pWDoc->m_pFormatStyles->GetDataType(nFmtIdx);


	if (!pCell->GetText().IsEmpty())
	{
		RDEData* ptrRDE = pCell->GetRDEValue();
		if (ptrRDE == NULL)
		{
			TRACE("Unable to retrieve Cell value. Row = %d, Col = %d.\n", nRowIdx, nColIdx);
			return FALSE;
		}

		if (oDataType == DataType::Enum)
		{
			DWORD dwEnum = *(DWORD*)ptrRDE->GetData();
			oDataType.m_wTag = GET_TAG_VALUE(dwEnum);
		}

		//anomalia  22851 BENELLI
		if (oDataType == DataType::Bool)
		{
			DataBool db;
			db.Assign(*ptrRDE);

			oDataType = DataType::String;
			pDataValue = DataObj::DataObjCreate(oDataType);
			ASSERT_VALID(pDataValue);

			pDataValue->Assign(db.FormatData());
		}
		else
		{
			pDataValue = DataObj::DataObjCreate(oDataType);
			ASSERT_VALID(pDataValue);

			pDataValue->Assign(*ptrRDE);
		}

		if (oDataType == DataType::Money || oDataType == DataType::Double || oDataType == DataType::Quantity || oDataType == DataType::Percent)
		{
			if (pDataValue->IsEmpty())
				((DataDbl*)pDataValue)->Assign(0.0);    //elimina decimali
														//else
														//     pDataValue->Assign(*ptrRDE);
		}
		//else
		//     pDataValue->Assign(*ptrRDE);
	}

	return TRUE;
}

