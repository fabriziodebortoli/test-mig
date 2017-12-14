#pragma once

#include <afxtempl.h>
#include <TbGenlib\parsctrl.h>
#include <TbGeneric\Linefile.h>
#include <TbGenlib\BaseDoc.h>
#include <TbWoormEngine\Report.h>
#include "table.h"
#include "rectobj.h"

//includere alla fine degli include del .H
#include "beginh.dex"

class CExportData;
class CExportInfoDlg;
class BaseObj;
class CExportInfo;


//==============================================================================
class TB_EXPORT CExportInfoDlg : public CParsedDialog
{
	DECLARE_DYNAMIC(CExportInfoDlg)
	
protected:
	CExportInfo& 	m_ExportInfo;

	CIntEdit		m_FromPageEdit;
	CIntEdit		m_ToPageEdit;
	CBCGPButton		m_MultiRows;
	CBCGPButton		m_RepeatColumnTitles;
	CBCGPButton		m_ColumnTitles;
	CBCGPButton		m_HiddenColumns;
	CBCGPButton		m_ExportTotals;
	CBCGPButton		m_ExportSubTotals;
	CBCGPComboBox	m_fileFormat;
	BOOL			m_bShowByColBtn;

	CBCGPButton			m_NewDoc;
	CBCGPButton			m_AppendExistsDoc;
	CBCGPButton			m_CurrPage;
	CBCGPButton			m_AllPages;
	CBCGPButton			m_RangePages;
	CBCGPButton			m_Text;
	CBCGPButton			m_ClipBoard;

public:
	CExportInfoDlg(CExportInfo&, BOOL bShowByColBtn = FALSE, CWnd* = NULL);

protected:
	virtual BOOL OnInitDialog	();
	virtual void OnOK			();

	afx_msg void	OnCurrentPage	();
	afx_msg void	OnSelPages		();
	afx_msg void	OnAllPages		();
	afx_msg void	OnSetFromPage	();
	afx_msg void	OnSetToPage		();
	afx_msg void	OnFindFile		();
	afx_msg void	OnSetTypeTextExport	();
	afx_msg void	OnClickColumnTitles	();
	afx_msg void	OnClickHiddenColumns();
	afx_msg void	OnSetOpenDoc	();
		
	DECLARE_MESSAGE_MAP()
};

//==============================================================================
class TB_EXPORT CFindWordDlg : public CParsedDialog
{
	DECLARE_DYNAMIC(CFindWordDlg)

protected:
	CString			m_strFindWord ;
	CStrEdit		m_FindWordEdit; 
	CWoormDocMng*		m_pWoormDoc ;	

	BOOL			m_bFindDown;

public:
	CFindWordDlg(LPCTSTR, CWnd* = NULL, CWoormDocMng* = NULL);
	CString		GetFindString();

protected:
	virtual BOOL OnInitDialog	();
	virtual void OnOK			();
	virtual void OnCancel		();
	virtual void PostNcDestroy	();

	afx_msg void	OnRadioUp	();
	afx_msg void	OnRadioDown	();

	

	DECLARE_MESSAGE_MAP()
};


//==============================================================================
class TB_EXPORT CSelectedColumn
{ 
public:
	int m_ColIdx; //indice in Table.Columns
	WORD m_ColID; //indice in Table.Columns
	BOOL m_bAlternate; 

	CSelectedColumn () 
		: 
		m_ColIdx		(-1), 
		m_ColID			(0), 
		m_bAlternate	(FALSE) 
		{}

	CSelectedColumn (int idx, WORD id, BOOL bAlternate/* = FALSE*/) 
		: 
		m_ColIdx		(idx), 
		m_ColID			(id), 
		m_bAlternate	(bAlternate) 
		{}
};

//------------------------------------------------------------------------------
class TB_EXPORT CExportTableItem : public CObject
{
	DECLARE_DYNAMIC(CExportTableItem)
	
	friend CExportData;

public:
	WORD 		m_nTableID;
	CWordArray  m_Rows;		// Numeri (anche sparsi) delle righe selezionate

	CArray<CSelectedColumn, CSelectedColumn> m_SelectedCol;   // Id della colonna usata per i titoli di riga o colonne selezionate per ordinamento discendente
	
public:
	CExportTableItem			(WORD nTableID);
	virtual ~CExportTableItem	();

	WORD GetTableID				() const		{ return m_nTableID; }

	int GetSizeSelectedColumns	() const		{ return m_SelectedCol.GetSize(); }
 	const CWordArray& GetRows () const		{ return m_Rows; }
  	//int GetColID				(int idx) const { return m_SelectedCol[idx].m_ColID; }
  	BOOL IsAlternate			(int idx) const { return m_SelectedCol[idx].m_bAlternate; }
   
// diagnostics
#ifdef _DEBUG
public:
	void Dump(CDumpContext& dc) const {	ASSERT_VALID(this); AFX_DUMP0(dc, "CExportTableItem\n"); }
	void AssertValid() const{ CObject::AssertValid(); }
#endif //_DEBUG
};


#include "endh.dex"
