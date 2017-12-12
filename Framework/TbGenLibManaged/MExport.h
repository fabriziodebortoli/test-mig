#pragma once

#include <TbGeneric/dataobj.h>
#include <TbGeneric/Linefile.h>

#include "beginh.dex"

///////////////////////////////////////////////////////////////////////////////
enum TypeOfExport
{
	EXPORT_CSV_TYPE = 1,
	EXPORT_HTML_TYPE,
	EXPORT_XML_TYPE,
	EXPORT_TXTCLIPBOARD_TYPE,
	EXPORT_EXCELNET_TYPE,
	EXPORT_OPENOFFICE_ODS_TYPE,
	EXPORT_WORDNET_TYPE,
	EXPORT_WORDNET_MERGE_TYPE,
	EXPORT_OPENOFFICE_ODT_TYPE,
	EXPORT_FIELDS_TO_TEMPLATE,
	EXPORT_FIELDS_TO_BASILEA,
	EXPORT_JSON_TYPE,
	EXPORT_OPENXML_EXCEL_TYPE,
	EXPORT_XML_FULL_TYPE
};

//=============================================================================
class TB_EXPORT CPlaceHolderExportInfo : public CObject 
{
	DECLARE_DYNAMIC(CPlaceHolderExportInfo);
public:
	CString			m_sTagID;
	CStringArray	m_arSelectedColumnName;
	CString			m_sColumnFilterName;
	DataObj*		m_pFilterValue;
	CArray<int>		m_arColumnIndex;
	int				m_ColumnFilterIndex;
	bool			m_bIsLike;

	CPlaceHolderExportInfo() : m_pFilterValue(NULL) { m_bIsLike = FALSE; }

	CPlaceHolderExportInfo(const CPlaceHolderExportInfo&);

	virtual ~CPlaceHolderExportInfo() { SAFE_DELETE(m_pFilterValue); }
};


//=============================================================================
class TB_EXPORT CExportDataNetTable
{
public:
	CStringArray		m_arAllColumnNames;
	CStringArray		m_arColumnTitles;
	DataObjArrayOfArray m_Cells;
	DataObjArray		m_TotalsRow;
	CArray<int>			m_arItemForPages;

	CString m_sTableName;
public:
	CExportDataNetTable() {}

	virtual ~CExportDataNetTable(void) {}
};

//=============================================================================
class TB_EXPORT CExportDataNetAskObject
{
public:
	CString				m_Names;
	CString				m_Type;
	CString				m_Length;
	CString				m_Title;
	CString				m_ControlType;
	CString				m_InputLimint;
	CString				m_Value;

public:
	CExportDataNetAskObject() {}

	virtual ~CExportDataNetAskObject(void) {}
};

//=============================================================================
class TB_EXPORT CExportDataNetAskDialogGroup
{
public:
	CString								m_Names;
	CString								m_Title;
	CArray<CExportDataNetAskObject*>	m_arEntry;

public:
	CExportDataNetAskDialogGroup() {}

	virtual ~CExportDataNetAskDialogGroup(void) 
	{
		for (int i = 0; i < m_arEntry.GetSize(); i++)
			delete(m_arEntry[i]);

		m_arEntry.RemoveAll();
	}
};


//=============================================================================
class TB_EXPORT CExportDataNetAskDialog
{
public:
	CString									m_Names;
	CString									m_Title;
	CArray<CExportDataNetAskDialogGroup*>	m_arGroup;

public:
	CExportDataNetAskDialog() {}

	virtual ~CExportDataNetAskDialog(void) 
	{
		for (int i = 0; i < m_arGroup.GetSize(); i++)
			delete(m_arGroup[i]);

		m_arGroup.RemoveAll();
	}
};

//=============================================================================
class TB_EXPORT CExportDataNet
{
	public: 
		CExportDataNetTable	m_Table;

		CString				m_sFileName;
		CString				m_sTemplate;
		CString				m_Sheet;

		int					firstRow = 0;
		int					firstColumn = 0;

		BOOL				m_bExportTotals = FALSE;
		BOOL				m_bExportSubTotals = FALSE;
		BOOL				m_bAutoSave = FALSE;
		BOOL				m_bRepeatColumnTitles = FALSE;
		BOOL				m_bCSVFormat = FALSE;
		BOOL				m_bisClipboard = FALSE;

		CString				m_strCSVSep;

		CString m_sDataFormat;
		CString m_SDataTimeFormat;
		CString m_STimeFormat;
		CLineFile::FileFormat m_FileFormatType = CLineFile::FileFormat::UTF8;

		CArray<CPlaceHolderExportInfo*>	m_parPlaceHolders;
public:
		CExportDataNet() 
			: firstRow(0), firstColumn(0) {}

		virtual ~CExportDataNet(void) {}
};

//=============================================================================
class TB_EXPORT CExportDataNetFull
{
public:
	CArray<CExportDataNetTable*>		m_arTables;
	DataObjArrayOfArray					m_SingleCells;
	CString								m_sReportName;
	CString								m_sReportNameSpace;
	CArray<CExportDataNetAskDialog*>	m_arAskDialogs;
	
public:
	CExportDataNetFull(){}

	virtual ~CExportDataNetFull(void) 
	{
		for (int i = 0; i < m_arAskDialogs.GetSize(); i++)
			delete(m_arAskDialogs[i]);

		m_arAskDialogs.RemoveAll();

		for (int i = 0; i < m_arTables.GetSize(); i++)
			delete(m_arTables[i]);

		m_arTables.RemoveAll();

	}
};

//=============================================================================
class TB_EXPORT MExportNet
{
public:
	MExportNet() {}

	virtual ~MExportNet(void) ;//{}

	BOOL	Export(CExportDataNet*, TypeOfExport m_nExportType);
	CString	ExportFullXml(CExportDataNetFull* pData, TypeOfExport m_nExportType);
	BOOL	IsEnabledOpenOffice(); 
};

//=============================================================================
#include "endh.dex"