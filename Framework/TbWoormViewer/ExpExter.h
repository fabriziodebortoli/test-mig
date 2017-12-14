#pragma once

#include <afxadv.h>
#include <TbGenLibManaged/MExport.h>
#include "Export.H"
#include "beginh.dex"

//==========================================================================================
class TB_EXPORT CExportData : public CObject
{
	DECLARE_DYNAMIC(CExportData)
	friend class CWoormDocMng;
	friend class CFindWordDlg;

protected:
	CWoormDocMng*	m_pWoormDoc = NULL;					// Punta al document di Woorm
	CExportInfo		m_ExportInfo ;

	CObject* 		m_pItem = NULL; 					// Puo essere un CWordArray o un CExportTableItem

	BOOL			m_bModified = FALSE;				// Indica che e' stato aggiunto o rimosso un ID (Spare, riga o colonna)

public:
	CExportData(CWoormDocMng*);
	virtual ~CExportData();

public:
	// cast & type info useful functions
	CExportTableItem*	GetExportTableItem() 	const { return (CExportTableItem*)m_pItem; }
	CWordArray*			GetSpareItem()	const { return (CWordArray*)m_pItem; }
	BOOL				IsTableItem()	const { return m_pItem && m_pItem->IsKindOf(RUNTIME_CLASS(CExportTableItem)); }
	BOOL				IsSynchronous()	const { return m_ExportInfo.m_bSync; }
	//CExportInfo &		GetExportInfo		()	const { return m_ExportInfo; }

public:
	void SetReportNameSpace( CString sNameSpace) { m_ExportInfo.m_strReportNameSpace = sNameSpace; }
	void SetReportName(CString sReportName) { m_ExportInfo.m_strReportName = sReportName; }

	BOOL AddRemoveElement(BaseObj*, CPoint point, BOOL bSelForTitles = FALSE, BOOL bClickAsShift = FALSE);
	BOOL AddRemoveTable(BaseObj*, CPoint point, BOOL bSelForTitles = FALSE, BOOL bClickAsShift = FALSE);
	BOOL AddRemoveField(BaseObj*);

	void AutoAddTable(CArray<CExportTableItem*>& m_parTables);

	BOOL AutoAddAllFields();

	BOOL IncludeField(WORD wID)	const;
	BOOL IncludeTable(WORD wID)	const;

	BOOL IncludeColumn(int col)	const;
	BOOL IncludeRow(int row)	const;
	BOOL IsTitlesColumn(int col)	const;

	BOOL AdjustRowsNumber(int);

public:
//	void	SetUseRunningApplication(BOOL bSet) { m_bUseRunningApplication = bSet; }
	BOOL	RunExport(LPCTSTR pszFileName = NULL, BOOL bOverwrite = FALSE);

	BOOL	RunGenericNetTableExport(TypeOfExport* typ);
	BOOL	RunFieldsExport(TypeOfExport* pExport);
	BOOL	RunFieldsExportForBasilea(TypeOfExport* pExport);
	CString	RunExportForXMLFULL();// TypeOfExport* pExport, int aPageNum);

	BOOL ConvertFieldDataToDataObj(CWoormDocMng* pWDoc, DataType& oDataType, DataObj*& pDataValue, FieldRect* pField, int nPageIdx, int nRowIdx, int nColIdx);
	BOOL ExportTitles(CExportTableItem* pItem, Table* pTable, BOOL bMultiLayout, CExportDataNetTable& exportRenderingData, BOOL& titleOK);
	BOOL ExportTotals(CExportTableItem* pItem, Table* pTable, BOOL  bMultiLayout, CWoormDocMng* m_pWoormDoc, CExportDataNetTable& exportRenderingData);
	BOOL ExportSingleTable(Table* table, int nPagesToExport, CExportDataNetTable & aCExportDataNetTable, BOOL aSubTotal, BOOL aTotal, CExportTableItem*	pItem);
	VOID CExportData::LoopFieldsExport(CExportDataNetFull & m_ExportDataNetFull, int aPageNum);

	static	BOOL	ConvertCellDataToString
		(
			CWoormDocMng* pWDoc,
			CString& strValue,
			Table* pTable,
			int nRowIdx, int nColIdx,
			DataType& SelDataType,
			void*& pDataValue,
			BOOL& bIsTailMultiLine,
			BOOL bPad = FALSE
			);
	static	BOOL	ConvertCellDataToDataObj
		(
			CWoormDocMng* pWDoc,
			Table* pTable,
			int nRowIdx, int nColIdx,
			DataType& oDataType,
			DataObj*& pDataValue,
			BOOL& bIsTailMultiLine,
			BOOL& bIsSubTotal
			);

#ifdef _DEBUG
public:
	void Dump(CDumpContext& dc) const { ASSERT_VALID(this); AFX_DUMP0(dc, "CExportData\n"); }
	void AssertValid() const { CObject::AssertValid(); }
#endif //_DEBUG
};
#include "endh.dex"

