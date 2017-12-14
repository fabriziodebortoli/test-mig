
#include "stdafx.h"

#include <TbGeneric\DataObj.h>
#include <TbGeneric\GeneralFunctions.h>
#include <TbGeneric\tools.h>
#include <TbGeneric\FormatsTable.h>
#include <TbGeneric\Linefile.h>

#include <TbGenlib\TbStrings.h>
#include <TbGenlib\generic.hjson> //JSON AUTOMATIC UPDATE

#include <TbGenlibUI\SettingsTableManager.h>

#include "baseobj.h"
#include "table.h"
#include "cell.h"
#include "column.h"
#include "rectobj.h" 
#include "woormdoc.h"
#include "colorcbx.h"
#include "ExpExter.h"
#include "export.h"
#include "export.hjson" //JSON AUTOMATIC UPDATE

// ----------------------------------------------------------

//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char BASED_CODE THIS_FILE[] = __FILE__;
#endif



//==============================================================================
//          Class CExportInfoDlg implementation
//==============================================================================
IMPLEMENT_DYNAMIC(CExportInfoDlg, CParsedDialog)
	BEGIN_MESSAGE_MAP(CExportInfoDlg, CParsedDialog)
	//{{AFX_MSG_MAP(CExportInfoDlg)
	ON_BN_CLICKED		(IDC_CURRPAGE_RADIOBTN,	OnCurrentPage)
	ON_BN_CLICKED		(IDC_SELPAGES_RADIOBTN,	OnSelPages)
	ON_BN_CLICKED		(IDC_ALLPAGES_RADIOBTN,	OnAllPages)

	ON_CONTROL			(EN_VALUE_CHANGED,		IDC_FROMPAGE_EDIT,	OnSetFromPage)
	ON_CONTROL			(EN_VALUE_CHANGED,		IDC_TOPAGE_EDIT,	OnSetToPage)

	ON_BN_CLICKED		(IDC_EXP_FINDFILE,			OnFindFile )
	ON_BN_CLICKED		(IDC_EXP_COLUMN_TITLE,		OnClickColumnTitles)
	ON_BN_CLICKED		(IDC_EXP_HIDDEN_COLUMNS,	OnClickHiddenColumns)
	ON_BN_CLICKED		(IDC_EXP_CSV,				OnSetTypeTextExport)
	ON_BN_CLICKED		(IDC_EXP_TXTCLIPBOARD,		OnSetTypeTextExport)

	ON_BN_CLICKED		(IDC_RAD_NEWDOC,		OnSetOpenDoc)
	ON_BN_CLICKED		(IDC_RAD_APPEND_DOC,	OnSetOpenDoc)

	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//------------------------------------------------------------------------------
CExportInfoDlg::CExportInfoDlg (CExportInfo& aExportInfo, BOOL bShowByColBtn, CWnd* aParent)
	:
	CParsedDialog	(IDD_EXPORTINFO, aParent),
	m_ExportInfo	(aExportInfo),

	m_bShowByColBtn	(bShowByColBtn),
	m_FromPageEdit  (BTN_SPIN_ID),
	m_ToPageEdit    (BTN_SPIN_ID)
{

}

//------------------------------------------------------------------------------
BOOL CExportInfoDlg::OnInitDialog ()
{
	m_ExportSubTotals	.SubclassDlgItem	(IDC_CHECK_EXPORTSUBTOTAL, this);
	m_ExportTotals		.SubclassDlgItem	(IDC_CHECK_EXPORT, this);
	m_MultiRows			.SubclassDlgItem	(IDC_EXP_MULTIROW, this);
	m_HiddenColumns		.SubclassDlgItem	(IDC_EXP_HIDDEN_COLUMNS, this);
	m_ColumnTitles		.SubclassDlgItem	(IDC_EXP_COLUMN_TITLE, this);
	m_RepeatColumnTitles.SubclassDlgItem	(IDC_EXP_REPEAT_COLUMN_TITLE, this);
	m_FromPageEdit		.SubclassEdit		(IDC_FROMPAGE_EDIT, this);
	m_ToPageEdit		.SubclassEdit		(IDC_TOPAGE_EDIT, 	this);
	
	m_NewDoc			.SubclassDlgItem(IDC_RAD_NEWDOC, this);
	m_AppendExistsDoc	.SubclassDlgItem(IDC_RAD_APPEND_DOC, this);
	m_CurrPage			.SubclassDlgItem(IDC_CURRPAGE_RADIOBTN, this);
	m_AllPages			.SubclassDlgItem(IDC_ALLPAGES_RADIOBTN, this);
	m_RangePages		.SubclassDlgItem(IDC_SELPAGES_RADIOBTN, this);
	m_ClipBoard			.SubclassDlgItem(IDC_EXP_TXTCLIPBOARD, this);
	m_Text				.SubclassDlgItem(IDC_EXP_CSV, this);
	__super::OnInitDialog ();	

	CString strTmp;

	m_ExportTotals.SetCheck(m_ExportInfo.m_bExportTotals);
	m_ExportSubTotals.SetCheck(m_ExportInfo.m_bExportSubTotals);

	m_MultiRows.SetCheck (m_ExportInfo.m_bMultiRows);
	m_MultiRows.ShowWindow (SW_HIDE);

	m_ColumnTitles.SetCheck (1);

	m_RepeatColumnTitles.SetCheck (0);
	m_RepeatColumnTitles.ShowWindow (SW_HIDE);

	m_FromPageEdit.SetRange (1, m_ExportInfo.m_nPagesNum);
	m_FromPageEdit.SetValue (m_ExportInfo.m_nFromPage);
	
	m_ToPageEdit.SetRange (m_ExportInfo.m_nFromPage, m_ExportInfo.m_nPagesNum);
	m_ToPageEdit.SetValue (m_ExportInfo.m_nToPage);

	m_HiddenColumns.SetCheck(m_ExportInfo.m_bHiddenColumns);

	GetDlgItem(IDC_EXP_FILENAME)->EnableWindow(FALSE);

	switch(m_ExportInfo.m_nExportType)	
	{
	case TypeOfExport::EXPORT_OPENOFFICE_ODS_TYPE:
	{
		SetWindowText(_TB("Export data to OpenOffice Sheet"));

		GetDlgItem(IDC_EXP_SHEETNAME)->ShowWindow(SW_SHOWNORMAL);
		GetDlgItem(IDC_EXP_OFFSETROW)->ShowWindow(SW_SHOWNORMAL);
		GetDlgItem(IDC_EXP_OFFSETCOL)->ShowWindow(SW_SHOWNORMAL);
		GetDlgItem(IDC_EXP_STSHEETNAME)->ShowWindow(SW_SHOWNORMAL);
		GetDlgItem(IDC_EXP_STSETROW)->ShowWindow(SW_SHOWNORMAL);
		GetDlgItem(IDC_EXP_STSETCOL)->ShowWindow(SW_SHOWNORMAL);

		GetDlgItem(IDC_RAD_NEWDOC)->ShowWindow(SW_SHOWNORMAL);
		GetDlgItem(IDC_RAD_APPEND_DOC)->ShowWindow(SW_SHOWNORMAL);
		CheckDlgButton(IDC_RAD_NEWDOC, 1);
		GetDlgItem(IDC_EXP_FILENAME)->EnableWindow(FALSE);
		GetDlgItem(IDC_EXP_FINDFILE)->EnableWindow(FALSE);

		GetDlgItem(IDC_EXP_FILENAME)->SetWindowText(m_ExportInfo.m_strFileName);
		GetDlgItem(IDC_EXP_SHEETNAME)->SetWindowText(m_ExportInfo.m_strSheetName);

		strTmp.Format(_T("%d"), m_ExportInfo.m_nOffsetRow + 1);
		GetDlgItem(IDC_EXP_OFFSETROW)->SetWindowText(strTmp);
		strTmp.Format(_T("%d"), m_ExportInfo.m_nOffsetCol + 1);
		GetDlgItem(IDC_EXP_OFFSETCOL)->SetWindowText(strTmp);
		if (!m_bShowByColBtn)
			m_MultiRows.ShowWindow(SW_SHOWNORMAL);

		m_RepeatColumnTitles.ShowWindow(SW_SHOWNORMAL);

		//----
		TbGenlibSettings params;

		m_ExportInfo.m_sDateTimeTypeFormat = params.GetExcelDateTimeFormat();
		m_ExportInfo.m_sDateTypeFormat = params.GetExcelDateFormat();
		m_ExportInfo.m_sTimeTypeFormat = params.GetExcelTimeFormat();
		//----

		//((CButton*)	GetDlgItem (IDC_EXP_COLUMN_TITLE))	->EnableWindow (TRUE);
		break;
	}
	case TypeOfExport::EXPORT_OPENXML_EXCEL_TYPE:
	case TypeOfExport::EXPORT_EXCELNET_TYPE:
			{
				SetWindowText(_TB("Export data to Microsoft Excel"));

				GetDlgItem(IDC_EXP_SHEETNAME)		->ShowWindow (SW_SHOWNORMAL);
				GetDlgItem(IDC_EXP_OFFSETROW)		->ShowWindow (SW_SHOWNORMAL);
				GetDlgItem(IDC_EXP_OFFSETCOL)		->ShowWindow (SW_SHOWNORMAL);
				GetDlgItem(IDC_EXP_STSHEETNAME)		->ShowWindow (SW_SHOWNORMAL);
				GetDlgItem(IDC_EXP_STSETROW)		->ShowWindow (SW_SHOWNORMAL);
				GetDlgItem(IDC_EXP_STSETCOL)		->ShowWindow (SW_SHOWNORMAL);
			
				GetDlgItem(IDC_RAD_NEWDOC)			->ShowWindow (SW_SHOWNORMAL);
				GetDlgItem(IDC_RAD_APPEND_DOC)		->ShowWindow (SW_SHOWNORMAL);
				CheckDlgButton(IDC_RAD_NEWDOC, 1);
				GetDlgItem (IDC_EXP_FILENAME)->EnableWindow(FALSE);
				GetDlgItem (IDC_EXP_FINDFILE)->EnableWindow(FALSE);

				GetDlgItem(IDC_EXP_FILENAME)		->SetWindowText (m_ExportInfo.m_strFileName);
				GetDlgItem(IDC_EXP_SHEETNAME)		->SetWindowText (m_ExportInfo.m_strSheetName);

				strTmp.Format(_T("%d"), m_ExportInfo.m_nOffsetRow + 1);
				GetDlgItem(IDC_EXP_OFFSETROW)->SetWindowText (strTmp);
				strTmp.Format(_T("%d"), m_ExportInfo.m_nOffsetCol + 1);
				GetDlgItem(IDC_EXP_OFFSETCOL)->SetWindowText (strTmp);
				if (!m_bShowByColBtn) 
					m_MultiRows.ShowWindow(SW_SHOWNORMAL);
			
				m_RepeatColumnTitles.ShowWindow(SW_SHOWNORMAL);

				//----
				TbGenlibSettings params;

				m_ExportInfo.m_sDateTimeTypeFormat = params.GetExcelDateTimeFormat();
				m_ExportInfo.m_sDateTypeFormat	 = params.GetExcelDateFormat();
				m_ExportInfo.m_sTimeTypeFormat	 = params.GetExcelTimeFormat();
				//----

				//((CButton*)	GetDlgItem (IDC_EXP_COLUMN_TITLE))	->EnableWindow (TRUE);
				break;
			}
	case TypeOfExport::EXPORT_OPENOFFICE_ODT_TYPE:
	{
		SetWindowText(_TB("Export data to OpenOffice Document"));

		GetDlgItem(IDC_RAD_NEWDOC)->ShowWindow(SW_SHOWNORMAL);
		GetDlgItem(IDC_RAD_APPEND_DOC)->ShowWindow(SW_SHOWNORMAL);
		CheckDlgButton(IDC_RAD_NEWDOC, 1);
		GetDlgItem(IDC_EXP_FILENAME)->EnableWindow(FALSE);
		GetDlgItem(IDC_EXP_FINDFILE)->EnableWindow(FALSE);
		m_RepeatColumnTitles.ShowWindow(SW_SHOWNORMAL);
		GetDlgItem(IDC_EXP_FILENAME)->SetWindowText(m_ExportInfo.m_strFileName);
		if (!m_bShowByColBtn)
			m_MultiRows.ShowWindow(SW_SHOWNORMAL);

		//((CButton*)	GetDlgItem (IDC_EXP_COLUMN_TITLE))	->EnableWindow (TRUE);
		break;
	}
	case TypeOfExport::EXPORT_WORDNET_TYPE:
			{
				SetWindowText(_TB("Export data to Microsoft Word"));

				GetDlgItem(IDC_RAD_NEWDOC)			->ShowWindow (SW_SHOWNORMAL);
				GetDlgItem(IDC_RAD_APPEND_DOC)		->ShowWindow (SW_SHOWNORMAL);
				CheckDlgButton(IDC_RAD_NEWDOC, 1);
				GetDlgItem (IDC_EXP_FILENAME)->EnableWindow(FALSE);
				GetDlgItem (IDC_EXP_FINDFILE)->EnableWindow(FALSE);
				m_RepeatColumnTitles.ShowWindow(SW_SHOWNORMAL);
				GetDlgItem (IDC_EXP_FILENAME)		->SetWindowText (m_ExportInfo.m_strFileName);
				if (!m_bShowByColBtn ) 
					m_MultiRows.ShowWindow (SW_SHOWNORMAL);

				//((CButton*)	GetDlgItem (IDC_EXP_COLUMN_TITLE))	->EnableWindow (TRUE);
				break;
			}
		case TypeOfExport::EXPORT_CSV_TYPE:
			{
				//Riempo la combo dei formati file
				m_fileFormat.SubclassDlgItem(IDC_FILEFORMAT_COMBO, this);

				int idx = m_fileFormat.AddString(_T("UTF8"));
				m_fileFormat.SetItemData(idx, CLineFile::UTF8);

				idx = m_fileFormat.AddString(_T("ANSI"));
				m_fileFormat.SetItemData(idx, CLineFile::ANSI);

				idx = m_fileFormat.AddString(_T("UTF16 BE"));
				m_fileFormat.SetItemData(idx, CLineFile::UTF16_BE);

				idx = m_fileFormat.AddString(_T("UTF16 LE"));
				m_fileFormat.SetItemData(idx, CLineFile::UTF16_LE);
				m_fileFormat.SetCurSel(3);
				SetWindowText(_TB("Export data on plain text files"));

				((CStatic*)GetDlgItem(IDC_EXP_STSEP))->ShowWindow(SW_SHOWNORMAL);
				((CEdit*)GetDlgItem(IDC_EXP_CSVSEPARATOR))->ShowWindow(SW_SHOWNORMAL);


				((CEdit*)GetDlgItem(IDC_EXP_CSVSEPARATOR))->SetWindowText(m_ExportInfo.m_strCSVSep);
				((CEdit*)GetDlgItem(IDC_EXP_FILENAME))->SetWindowText(m_ExportInfo.m_strFileName);
				((CBCGPButton*)GetDlgItem(IDC_EXP_FINDFILE))->SetWindowText(_TB("File Name in which to save..."));
				m_MultiRows.SetCheck(FALSE);

				((CBCGPButton*)GetDlgItem(IDC_EXP_CSV))->ShowWindow(SW_SHOWNORMAL);
				((CBCGPButton*)GetDlgItem(IDC_EXP_CSV_ENCODE))->ShowWindow(SW_SHOWNORMAL);
				((CBCGPButton*)GetDlgItem(IDC_EXP_CSV))->SetCheck(1);
				((CBCGPButton*)GetDlgItem(IDC_EXP_CSV_ENCODE))->SetCheck(m_ExportInfo.m_bEncodeCSV ? 1 : 0);

				((CBCGPButton*)GetDlgItem(IDC_EXP_TXTCLIPBOARD))->ShowWindow(SW_SHOWNORMAL);
				((CBCGPButton*)GetDlgItem(IDC_EXP_TXTCLIPBOARD))->SetCheck(0);

				((CBCGPButton*)GetDlgItem(IDC_EXP_COLUMN_TITLE))->EnableWindow(TRUE);

				((CBCGPButton*)GetDlgItem(IDC_EXPTIPE_STRING))->ShowWindow(SW_SHOWNORMAL);
				((CBCGPButton*)GetDlgItem(IDC_FILEFORMAT_COMBO))->ShowWindow(SW_SHOWNORMAL);
				GetDlgItem(IDC_EXP_FILENAME)->EnableWindow(FALSE);
				m_ExportTotals.ShowWindow(SW_HIDE);
				m_ExportSubTotals.ShowWindow(SW_HIDE);

				break;
			}
		case TypeOfExport::EXPORT_HTML_TYPE:
		{
			SetWindowText(_TB("Export data on HTML format file"));
			((CBCGPButton*)GetDlgItem(IDC_EXP_FINDFILE))->SetWindowText(_TB("File Name in which to save..."));
			((CEdit*)GetDlgItem(IDC_EXP_FILENAME))->SetWindowText(m_ExportInfo.m_strFileName);
			//if ( ! m_bShowByColBtn ) m_MultiRows.ShowWindow (SW_SHOWNORMAL);
			m_MultiRows.SetCheck(FALSE);
			m_ExportTotals.ShowWindow(SW_HIDE);
			m_ExportSubTotals.ShowWindow(SW_HIDE);
			//((CButton*)	GetDlgItem (IDC_EXP_COLUMN_TITLE))	->EnableWindow (TRUE);
			break;
		}
		case TypeOfExport::EXPORT_XML_TYPE:
		{
			GetDlgItem(IDC_EXP_STSHEETNAME)->SetWindowText(_TB("Table Name"));
			GetDlgItem(IDC_EXP_STSHEETNAME)->ShowWindow(SW_SHOWNORMAL);
			GetDlgItem(IDC_EXP_SHEETNAME)->ShowWindow(SW_SHOWNORMAL);
			GetDlgItem(IDC_EXP_SHEETNAME)->SetWindowText(m_ExportInfo.m_strSheetName);
			SetWindowText(_TB("Export data on XML format file"));
			((CBCGPButton*)GetDlgItem(IDC_EXP_FINDFILE))->SetWindowText(_TB("File Name in which to save..."));
			((CEdit*)GetDlgItem(IDC_EXP_FILENAME))->SetWindowText(m_ExportInfo.m_strFileName);
			//if ( ! m_bShowByColBtn ) m_MultiRows.ShowWindow (SW_SHOWNORMAL);
			m_MultiRows.SetCheck(FALSE);
			m_ExportTotals.ShowWindow(SW_HIDE);
			m_ExportSubTotals.ShowWindow(SW_HIDE);
			m_ColumnTitles.ShowWindow(SW_HIDE);
			//((CButton*)	GetDlgItem (IDC_EXP_COLUMN_TITLE))	->EnableWindow (TRUE);
			GetDlgItem(IDC_EXP_FILENAME)->EnableWindow(FALSE);
			break;
		}
		case TypeOfExport::EXPORT_JSON_TYPE:
		{
			GetDlgItem(IDC_EXP_STSHEETNAME)->SetWindowText(_TB("Table Name"));
			GetDlgItem(IDC_EXP_STSHEETNAME)->ShowWindow(SW_SHOWNORMAL);
			GetDlgItem(IDC_EXP_SHEETNAME)->ShowWindow(SW_SHOWNORMAL);
			GetDlgItem(IDC_EXP_SHEETNAME)->SetWindowText(m_ExportInfo.m_strSheetName);
			SetWindowText(_TB("Export data on JSON format file"));
			((CBCGPButton*)GetDlgItem(IDC_EXP_FINDFILE))->SetWindowText(_TB("File Name in which to save..."));
			((CEdit*)GetDlgItem(IDC_EXP_FILENAME))->SetWindowText(m_ExportInfo.m_strFileName);
			//if ( ! m_bShowByColBtn ) m_MultiRows.ShowWindow (SW_SHOWNORMAL);
			m_MultiRows.SetCheck(FALSE);
			m_ExportTotals.ShowWindow(SW_HIDE);
			m_ExportSubTotals.ShowWindow(SW_HIDE);
			m_ColumnTitles.ShowWindow(SW_HIDE);
			//((CButton*)	GetDlgItem (IDC_EXP_COLUMN_TITLE))	->EnableWindow (TRUE);
			GetDlgItem(IDC_EXP_FILENAME)->EnableWindow(FALSE);
			break;
		}
		default:
			break;
	}

	((CBCGPButton*)GetDlgItem(IDC_ALLPAGES_RADIOBTN))->SetCheck(TRUE);
	m_FromPageEdit.EnableWindow (FALSE);
	m_ToPageEdit.EnableWindow (FALSE);
	
	return TRUE; 
}

//-----------------------------------------------------------------------------
void CExportInfoDlg::OnClickHiddenColumns()
{
	//if (m_HiddenColumns.GetCheck() == 0)
	//{
	//	m_HiddenColumns.SetCheck(0);
	//	m_HiddenColumns.EnableWindow(FALSE);
	//}
	//else
	//	m_HiddenColumns.EnableWindow(TRUE);
}

//-----------------------------------------------------------------------------
void CExportInfoDlg::OnClickColumnTitles()
{
	if (m_ColumnTitles.GetCheck() == 0)
	{
		m_RepeatColumnTitles.SetCheck(0);
		m_RepeatColumnTitles.EnableWindow(FALSE);
	}
	else
		m_RepeatColumnTitles.EnableWindow(TRUE);
}
//-----------------------------------------------------------------------------

void CExportInfoDlg::OnCurrentPage()
{
	((CBCGPButton*)GetDlgItem(IDC_SELPAGES_RADIOBTN))->SetCheck(FALSE);
	m_FromPageEdit.EnableWindow(FALSE);
	m_ToPageEdit.EnableWindow(FALSE);
	((CBCGPButton*)GetDlgItem(IDC_ALLPAGES_RADIOBTN))->SetCheck(FALSE);
}

//------------------------------------------------------------------------------
void CExportInfoDlg::OnSelPages()
{
	((CBCGPButton*)GetDlgItem(IDC_CURRPAGE_RADIOBTN))->SetCheck(FALSE);
	m_FromPageEdit.EnableWindow(TRUE);
	m_ToPageEdit.EnableWindow(TRUE);
	((CBCGPButton*)GetDlgItem(IDC_ALLPAGES_RADIOBTN))->SetCheck(FALSE);
}

//------------------------------------------------------------------------------
void CExportInfoDlg::OnAllPages()
{
	((CBCGPButton*)GetDlgItem(IDC_CURRPAGE_RADIOBTN))->SetCheck(FALSE);
	((CBCGPButton*)GetDlgItem(IDC_SELPAGES_RADIOBTN))->SetCheck(FALSE);
	m_FromPageEdit.EnableWindow(FALSE);
	m_ToPageEdit.EnableWindow(FALSE);
}

//------------------------------------------------------------------------------
void CExportInfoDlg::OnSetFromPage()
{
	int nNewFromPage = m_FromPageEdit.GetValue();

	m_ToPageEdit.SetRange(nNewFromPage, m_ExportInfo.m_nPagesNum);
	if ( nNewFromPage > m_ToPageEdit.GetValue())
		m_ToPageEdit.SetValue(nNewFromPage);
}

//------------------------------------------------------------------------------
void CExportInfoDlg::OnSetToPage()
{
	int nNewToPage = m_ToPageEdit.GetValue();

	m_FromPageEdit.SetRange(1, nNewToPage);
	if ( nNewToPage  < m_FromPageEdit.GetValue())
		m_FromPageEdit.SetValue(nNewToPage);
}

//------------------------------------------------------------------------------
void CExportInfoDlg::OnSetTypeTextExport()
{
	if (((CBCGPButton*)GetDlgItem(IDC_EXP_TXTCLIPBOARD))->GetCheck() == 1)
	{
		((CEdit*)	GetDlgItem (IDC_EXP_FILENAME))		->EnableWindow(FALSE);
		((CBCGPButton*)GetDlgItem(IDC_EXP_FINDFILE))->EnableWindow(FALSE);
	}
	else
	{
		((CEdit*)	GetDlgItem (IDC_EXP_FILENAME))		->EnableWindow(TRUE);
		((CBCGPButton*)GetDlgItem(IDC_EXP_FINDFILE))->EnableWindow(TRUE);
	}

	((CEdit*) GetDlgItem (IDC_EXP_CSVSEPARATOR))	->EnableWindow(TRUE);
}

//------------------------------------------------------------------------------
void CExportInfoDlg::OnFindFile ()
{
	CString strExt;
	CString	strAny;
	CString	strFilter;
	bool bOpen = false;

	switch ( m_ExportInfo.m_nExportType )
	{
		case TypeOfExport::EXPORT_OPENOFFICE_ODT_TYPE:
			strExt = FileExtension::ODT_EXT();
			strAny = FileExtension::ANY_ODT();
			strFilter = FileExtension::ODT_FILTER();
			bOpen = true;
			break;

		case TypeOfExport::EXPORT_OPENOFFICE_ODS_TYPE:
			strExt		= FileExtension::ODS_EXT();
			strAny		= FileExtension::ANY_ODS();
			strFilter	= FileExtension::ODS_FILTER();
			bOpen = true;
			break;

		case TypeOfExport::EXPORT_EXCELNET_TYPE:
			strExt		= FileExtension::EXCELNET_EXT();
			strAny		= FileExtension::ANY_EXCELNET();
			strFilter	= FileExtension::EXCELNET_FILTER();
			bOpen = true;
			break;

		case TypeOfExport::EXPORT_WORDNET_TYPE:
			strExt		= FileExtension::WORDNET_EXT();
			strAny		= FileExtension::ANY_WORDNET();
			strFilter	= FileExtension::WORDNET_FILTER();
			bOpen = true;
			break;

		case TypeOfExport::EXPORT_HTML_TYPE:
			strExt		= FileExtension::HTML_EXT();
			strAny		= FileExtension::ANY_HTML();
			strFilter	= FileExtension::HTML_FILTER();
			bOpen = false;
			break;

		case TypeOfExport::EXPORT_XML_FULL_TYPE:
		case TypeOfExport::EXPORT_XML_TYPE:
			strExt		= FileExtension::XML_EXT();
			strAny		= FileExtension::ANY_XML();
			strFilter	= FileExtension::XML_FILTER();
			bOpen = false;
			break;

		case TypeOfExport::EXPORT_JSON_TYPE:
			strExt = FileExtension::JSON_EXT();
			strAny = FileExtension::ANY_JSON();
			strFilter = FileExtension::JSON_FILTER();
			bOpen = false;
			break;

		case TypeOfExport::EXPORT_CSV_TYPE:
			strExt		= FileExtension::CSV_EXT();
			strAny		= FileExtension::ANY_CSV();
			strFilter	= FileExtension::CSV_FILTER();
			bOpen = false;
			break;

		default:
			return;
	}

	// Consento di browsare su tutto il disco, senza utilizzare 
	// la TBExplorer che limiterebbe la funzionalità
	CFileDialog dialog( bOpen, strExt, strAny, WRITEFLAGS, strFilter );

	if ( dialog.DoModal() != IDOK ) return;
	 
	((CEdit*) GetDlgItem(IDC_EXP_FILENAME)) -> SetWindowText( dialog.GetPathName() );
}

//------------------------------------------------------------------------------
void CExportInfoDlg::OnSetOpenDoc()
{
	if (!GetDlgItem (IDC_RAD_APPEND_DOC)->IsWindowVisible())
		return;

	BOOL b = IsDlgButtonChecked (IDC_RAD_APPEND_DOC) == 1;
	
//	GetDlgItem (IDC_EXP_FILENAME)->EnableWindow(b);
	GetDlgItem (IDC_EXP_FINDFILE)->EnableWindow(b);
}

//------------------------------------------------------------------------------
void CExportInfoDlg::OnOK()
{
	CString strTmp;

	switch ( m_ExportInfo.m_nExportType ) 
	{

	case TypeOfExport::EXPORT_OPENOFFICE_ODS_TYPE:
	case TypeOfExport::EXPORT_EXCELNET_TYPE:
	case TypeOfExport::EXPORT_OPENXML_EXCEL_TYPE:
	{
		((CEdit*)GetDlgItem(IDC_EXP_FILENAME))->GetWindowText(m_ExportInfo.m_strFileName);
		((CEdit*)GetDlgItem(IDC_EXP_SHEETNAME))->GetWindowText(m_ExportInfo.m_strSheetName);
		
		((CEdit*)GetDlgItem(IDC_EXP_OFFSETROW))->GetWindowText(strTmp);
		m_ExportInfo.m_nOffsetRow = max(0, _tstoi(strTmp) -1 );
		((CEdit*)GetDlgItem(IDC_EXP_OFFSETCOL))->GetWindowText(strTmp);
		m_ExportInfo.m_nOffsetCol = max(0, _tstoi(strTmp) -1 );
		//----

		((CEdit*)GetDlgItem(IDC_EXP_SHEETNAME))->GetWindowText(m_ExportInfo.m_strSheetName);

		break;
		}
	case TypeOfExport::EXPORT_WORDNET_TYPE:
		((CEdit*)GetDlgItem(IDC_EXP_FILENAME))->GetWindowText(m_ExportInfo.m_strFileName);
		break;

	case TypeOfExport::EXPORT_CSV_TYPE:
		m_ExportInfo.m_FileFormat = (CLineFile::FileFormat) this->m_fileFormat.GetItemData(m_fileFormat.GetCurSel());
		((CEdit*)GetDlgItem(IDC_EXP_FILENAME)) ->GetWindowText(m_ExportInfo.m_strFileName);
		((CEdit*)GetDlgItem(IDC_EXP_CSVSEPARATOR)) ->GetWindowText(m_ExportInfo.m_strCSVSep);
		m_ExportInfo.m_bEncodeCSV = ((CBCGPButton*)GetDlgItem(IDC_EXP_CSV_ENCODE))->GetCheck() == 1;
		m_ExportInfo.m_FileFormat = (CLineFile:: FileFormat) this->m_fileFormat.GetItemData( m_fileFormat.GetCurSel());

		if (((CBCGPButton*)GetDlgItem(IDC_EXP_TXTCLIPBOARD))->GetCheck() == 1)
		{
			m_ExportInfo.m_nExportType = TypeOfExport::EXPORT_TXTCLIPBOARD_TYPE;
			m_ExportInfo.m_bisClipboard = TRUE;
		}
		else if (((CBCGPButton*)GetDlgItem(IDC_EXP_CSV))->GetCheck() == 1)
		{
			//if ( m_ExportInfo.m_strFileName.IsEmpty() )
			//{
			//	AfxMessageBox( _TB("You must specify a file name in which to save data"));
			//	return;
			//}
			m_ExportInfo.m_nExportType = TypeOfExport::EXPORT_CSV_TYPE;
			m_ExportInfo.m_bisClipboard = FALSE;
		}

		break;

	case TypeOfExport::EXPORT_XML_FULL_TYPE:
	case TypeOfExport::EXPORT_HTML_TYPE:

		((CEdit*)GetDlgItem(IDC_EXP_FILENAME))->GetWindowText(m_ExportInfo.m_strFileName);
		break;

	case TypeOfExport::EXPORT_JSON_TYPE:
	case TypeOfExport::EXPORT_XML_TYPE:

		((CEdit*)GetDlgItem(IDC_EXP_FILENAME))->GetWindowText(m_ExportInfo.m_strFileName);
		((CEdit*)GetDlgItem(IDC_EXP_SHEETNAME))->GetWindowText(m_ExportInfo.m_strSheetName);
		break;

	default:
		EndDialog(IDCANCEL);
	}
	// ----
	m_ExportInfo.m_bExportSubTotals = m_ExportSubTotals.GetCheck();
	m_ExportInfo.m_bExportTotals = m_ExportTotals.GetCheck();
	m_ExportInfo.m_bMultiRows = m_MultiRows.GetCheck();
	m_ExportInfo.m_bCurrentPage = ((CBCGPButton*)GetDlgItem(IDC_CURRPAGE_RADIOBTN))->GetCheck();
	if (m_ExportInfo.m_bAllPages)
		m_ExportInfo.SetPageRange(m_ExportInfo.m_nCurrPage, m_ExportInfo.m_nCurrPage);
	m_ExportInfo.m_bAllPages = ((CBCGPButton*)GetDlgItem(IDC_ALLPAGES_RADIOBTN))->GetCheck();
	if (m_ExportInfo.m_bAllPages)
		m_ExportInfo.SetPageRange(1, m_ExportInfo.m_nPagesNum);

	if (((CBCGPButton*)GetDlgItem(IDC_SELPAGES_RADIOBTN))->GetCheck())
		m_ExportInfo.SetPageRange(m_FromPageEdit.GetValue(), m_ToPageEdit.GetValue());
			
	m_ExportInfo.m_bRepeatColumnTitles = m_RepeatColumnTitles.GetCheck();
	m_ExportInfo.m_bColumnTitles = m_ColumnTitles.GetCheck();
	m_ExportInfo.m_bHiddenColumns = m_HiddenColumns.GetCheck();
	

	EndDialog(IDOK);
}

//==============================================================================
//          Class CFindWordDlg implementation
//==============================================================================
IMPLEMENT_DYNAMIC(CFindWordDlg, CParsedDialog )

BEGIN_MESSAGE_MAP(CFindWordDlg, CParsedDialog)
	//{{AFX_MSG_MAP(CFindWordDlg)
	ON_BN_CLICKED		(IDC_FINDWORDRADIOUP,	OnRadioUp)
	ON_BN_CLICKED		(IDC_FINDWORDRADIODOWN,	OnRadioDown)
	
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//------------------------------------------------------------------------------
CFindWordDlg::CFindWordDlg(LPCTSTR strFindWord, CWnd* aParent, CWoormDocMng* pWoormDoc) 
	:
	CParsedDialog	(IDD_FINDWORD, aParent),
	m_pWoormDoc		(pWoormDoc),
	m_strFindWord	(strFindWord)
{
}

//------------------------------------------------------------------------------
BOOL CFindWordDlg::OnInitDialog ()
{
	CParsedDialog::OnInitDialog ();	

	m_FindWordEdit.SubclassEdit(IDC_FINDWORDEDIT,this);
	m_FindWordEdit.SetValue(m_strFindWord);

	((CBCGPButton*)GetDlgItem(IDC_FINDWORDRADIODOWN))->SetCheck(TRUE);
	((CBCGPButton*)GetDlgItem(IDC_FINDWORDRADIOUP))->SetCheck(FALSE);
	m_bFindDown = TRUE;

	return TRUE;
}

//------------------------------------------------------------------------------
void CFindWordDlg::OnRadioUp()
{
	((CBCGPButton*)GetDlgItem(IDC_FINDWORDRADIODOWN))->SetCheck(FALSE);
	((CBCGPButton*)GetDlgItem(IDC_FINDWORDRADIOUP))->SetCheck(TRUE);
	m_bFindDown = FALSE;
}

//------------------------------------------------------------------------------
void CFindWordDlg::OnRadioDown()
{
	((CBCGPButton*)GetDlgItem(IDC_FINDWORDRADIOUP))->SetCheck(FALSE);
	((CBCGPButton*)GetDlgItem(IDC_FINDWORDRADIODOWN))->SetCheck(TRUE);
	m_bFindDown = TRUE;

}

//------------------------------------------------------------------------------
void CFindWordDlg::OnOK()
{
	m_strFindWord = m_FindWordEdit.GetValue();

	if (m_pWoormDoc && !m_strFindWord.IsEmpty())
	{
		ASSERT_VALID(m_pWoormDoc);

		m_pWoormDoc->FindWord(m_strFindWord, m_bFindDown);
	}
}

//------------------------------------------------------------------------------
void CFindWordDlg::OnCancel()
{
	if (m_pWoormDoc)
	{		
		ASSERT_VALID(m_pWoormDoc);

		m_pWoormDoc->m_Layouts.EnumObjects(&CWoormDocMng::ClearFindCellArray);
		m_pWoormDoc->m_findWordInfo.Clear();
		m_pWoormDoc->m_FindDlg = NULL;

		if (m_pWoormDoc->m_pExportData)
			SAFE_DELETE(m_pWoormDoc->m_pExportData->m_pItem);

		m_pWoormDoc->Invalidate		();
		m_pWoormDoc->UpdateWindow	();
	}

	DestroyWindow();
}

//------------------------------------------------------------------------------
void CFindWordDlg::PostNcDestroy()
{
	if (m_pWoormDoc)
	{		
		m_pWoormDoc->m_FindDlg = NULL;		
	}
	delete this;
}
//------------------------------------------------------------------------------
CString CFindWordDlg::GetFindString()
{
	return m_strFindWord;

}

//==============================================================================
//          Class CExportTableItem implementation
//==============================================================================
IMPLEMENT_DYNAMIC(CExportTableItem, CObject)

//------------------------------------------------------------------------------
CExportTableItem::CExportTableItem(WORD nTableID)
	:
	m_nTableID		(nTableID)
{
}

//------------------------------------------------------------------------------
CExportTableItem::~CExportTableItem()
{
}

