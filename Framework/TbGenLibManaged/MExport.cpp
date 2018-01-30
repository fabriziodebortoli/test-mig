#include "StdAfx.h"

#include "StaticFunctions.h"
#include "MExport.h"

///////////////////////////////////////////////////////////////////////////////

IMPLEMENT_DYNAMIC(CPlaceHolderExportInfo, CObject)

CPlaceHolderExportInfo::CPlaceHolderExportInfo(const CPlaceHolderExportInfo& ph)
:
m_sTagID(ph.m_sTagID),
m_sColumnFilterName(ph.m_sColumnFilterName),
m_pFilterValue(NULL),
m_ColumnFilterIndex(ph.m_ColumnFilterIndex),
m_bIsLike(ph.m_bIsLike)
{
	m_arSelectedColumnName.Copy(ph.m_arSelectedColumnName);
	if (ph.m_pFilterValue)
		m_pFilterValue = ph.m_pFilterValue->DataObjClone();

	m_arColumnIndex.Copy(ph.m_arColumnIndex);
}

///////////////////////////////////////////////////////////////////////////////

using namespace System;
using namespace System::Reflection;
using namespace System::Collections::Generic;
using namespace Microarea::TaskBuilderNet::UI::ReportsRenders;
using namespace Microarea::TaskBuilderNet::UI::DocumentMerge;

//-----------------------------------------------------------------------------
BOOL MExportNet::IsEnabledOpenOffice()
{
	OpenOfficeSheetRender^ render = gcnew OpenOfficeSheetRender();
	return render->IsEnabledOpenOffice();
}

//-----------------------------------------------------------------------------
BOOL MExportNet::Export (CExportDataNet* pData, TypeOfExport m_nExportType)
{
	//pData->se
	int nColsexp = pData->m_Table.m_Cells[0]->GetSize();

	int nRows = pData->m_Table.m_Cells.GetSize();
	int nCols = pData->m_Table.m_arColumnTitles.GetSize();
	int nCollsTot = pData->m_Table.m_TotalsRow.GetSize();
	int nRowForPage = pData->m_Table.m_arItemForPages.GetSize();
	int fileFormat = pData->m_FileFormatType;

	array<String^>^ strarrayColumnName = gcnew array<String^> (nColsexp);
	array<String^>^ strarrayTitles = gcnew array<String^> (nCols);
	array<Object^, 2>^ cells = gcnew array<Object^, 2> (nRows, nColsexp);
	array<int>^ intrarrayRowsForPage = gcnew array<int>(nRowForPage);

	array<Object^>^ tot = gcnew array<Object^> (nCollsTot);

	int nph = pData->m_parPlaceHolders.GetCount();

	System::Collections::Generic::List<SharpPlaceHolderExportInfo^>^ placeholder = gcnew System::Collections::Generic::List<SharpPlaceHolderExportInfo^> ();

	for (int i = 0; i < nph; i++)
	{
		SharpPlaceHolderExportInfo^ sharp = gcnew SharpPlaceHolderExportInfo();
		CPlaceHolderExportInfo* infos = pData->m_parPlaceHolders[i];
		sharp->IsLike = infos->m_bIsLike;
		sharp->TagID = gcnew System::String(infos->m_sTagID);
		sharp->SelectedColumnName = gcnew array<String^> (infos->m_arSelectedColumnName.GetSize());
		
		for (int ii = 0; ii < infos->m_arSelectedColumnName.GetSize(); ii++)
			sharp->SelectedColumnName[ii] = gcnew String( infos->m_arSelectedColumnName[ii] );
		
		sharp->ColumnFilterName = gcnew System::String(infos->m_sColumnFilterName);
		sharp->FilterValue =  ConverDataObj(infos->m_pFilterValue);
		sharp->ColumnFilterIndex = (int)(infos->m_ColumnFilterIndex);

		List<Int32>^ list = gcnew List<Int32>();
		int nCols2 = pData->m_parPlaceHolders[i]->m_arColumnIndex.GetSize();

		for (int ii = 0; ii < nCols2; ii++)
		{
			int a = pData->m_parPlaceHolders[i]->m_arColumnIndex[ii];
			list->Add(a);
		}

		sharp->ColumnIndex = list;
		placeholder->Add(sharp);
	}

	for (int i = 0; i < nCols; i++)
	{
		strarrayTitles[i]		= gcnew String(pData->m_Table.m_arColumnTitles[i] );

		if (pData->m_Table.m_arAllColumnNames.GetSize() >0)
			strarrayColumnName[i]	= gcnew String (pData->m_Table.m_arAllColumnNames[i]);
	}

	for (int i = 0; i < nRowForPage; i++)
		intrarrayRowsForPage[i] = (int)pData->m_Table.m_arItemForPages[i];

	for (int i = 0; i < nCollsTot; i++)
	{
		Object^ o = gcnew String("");
		DataObj* pObj = pData->m_Table.m_TotalsRow.GetAt(i);

		if (pObj != NULL)
			o = ConverDataObj(pObj);
		
		tot[i]= o;
	}

	for (int r = 0; r < nRows; r++)
	{
		for (int c = 0; c < nColsexp; c++)
		{
			Object^ o = gcnew String("");
			DataObj* pObj = NULL;

			if (pData->m_Table.m_Cells.GetAt(r, c) != NULL)
				pObj = pData->m_Table.m_Cells.GetAt(r, c);

			if (pObj != NULL)
				o = ConverDataObj(pObj);

			cells [r, c] = o;
		}
	}

	System::String^ s				= gcnew System::String(pData->m_sFileName);
	System::String^ d				= gcnew System::String(pData->m_Sheet);
	System::String^ templateName	= gcnew System::String(pData->m_sTemplate);

	int		firstRow		= pData->firstRow + 1;
	int		firstCol		= pData->firstColumn + 1;
	bool	repeatTitles	= (pData->m_bRepeatColumnTitles == TRUE);
	bool	exportTotal		= (pData->m_bExportTotals == TRUE);
	bool	autoSave		= (pData->m_bAutoSave == TRUE);
	bool	cvsFormat		= (pData->m_bCSVFormat == TRUE);
	bool	isClipboard		= (pData->m_bisClipboard == TRUE);

	System::String^ dataf			= gcnew System::String(pData->m_sDataFormat);
	System::String^ dataftimef		= gcnew System::String(pData->m_SDataTimeFormat);
	System::String^ timef			= gcnew System::String(pData->m_STimeFormat);
	
	System::String^ separator = gcnew System::String(pData->m_strCSVSep);

	if (m_nExportType == TypeOfExport::EXPORT_EXCELNET_TYPE) 
	{
		ExcelReportsRender^ render = gcnew ExcelReportsRender(s, d, firstRow, firstCol, dataf, dataftimef, timef, repeatTitles, exportTotal, autoSave); 
		render->CreateExcel(cells, strarrayTitles, tot, intrarrayRowsForPage);
		delete render;
	}
	else if (m_nExportType == TypeOfExport::EXPORT_OPENXML_EXCEL_TYPE) 
	{
		OpenXmlExcelRender^ render = gcnew OpenXmlExcelRender(s, d, firstRow, firstCol, dataf, dataftimef, timef, repeatTitles, exportTotal, autoSave);
		render->GetExcelDataPage(cells, strarrayTitles, tot, intrarrayRowsForPage);
		delete render;
	}
	else if (m_nExportType == TypeOfExport::EXPORT_OPENOFFICE_ODS_TYPE)
	{
		OpenOfficeSheetRender^ render = gcnew OpenOfficeSheetRender(s, d, firstRow, firstCol, dataf, dataftimef, timef, repeatTitles, exportTotal);
		render->CreateODS(cells, strarrayTitles, tot, intrarrayRowsForPage);
		delete render;
	}
	else if (m_nExportType == TypeOfExport::EXPORT_WORDNET_TYPE)
	{
		DocxReportsRender^ docx = gcnew  DocxReportsRender(s, dataf, dataftimef, timef, repeatTitles, exportTotal);
		docx->CreateDocx(cells, strarrayTitles, tot, intrarrayRowsForPage);
		delete docx;
	}
	else if (m_nExportType == TypeOfExport::EXPORT_XML_TYPE)
	{
		XMLReportsRender^ xmlReportsRender = gcnew XMLReportsRender(s, dataf, dataftimef, timef, d);
		xmlReportsRender->CreateXML(cells, strarrayColumnName);
		delete xmlReportsRender;
	}
	else if (m_nExportType == TypeOfExport::EXPORT_JSON_TYPE)
	{
		//TODO GIULIA
		//JsonReportsRender^ jsonReportsRender = gcnew JsonReportsRender(s, dataf, dataftimef, timef, d);
		//jsonReportsRender->CreateJson(cells, strarrayColumnName);
		//delete jsonReportsRender;
	}
	else if (m_nExportType == TypeOfExport::EXPORT_WORDNET_MERGE_TYPE)
	{
		DocxReportsRender^ docx = gcnew  DocxReportsRender(s, dataf, dataftimef, timef, repeatTitles, exportTotal);
		docx->CreateDocxForMerge(cells, strarrayTitles, placeholder, tot);
		delete docx;
	}
	else if (m_nExportType == TypeOfExport::EXPORT_HTML_TYPE)
	{
		HTMLReportsRender^ docx = gcnew  HTMLReportsRender(s, dataf, dataftimef, timef);
		docx->CreateHTMLForMerge(cells, strarrayTitles);
		delete docx;
	}
	else if (m_nExportType == TypeOfExport::EXPORT_CSV_TYPE || m_nExportType == TypeOfExport::EXPORT_TXTCLIPBOARD_TYPE)
	{
		TxtReportsRender^ docx = gcnew  TxtReportsRender(s, dataf, dataftimef, timef, fileFormat, cvsFormat, isClipboard, separator);
		docx->CreateTxtForMerge(cells, strarrayTitles);
		delete docx;
	}
	else if (m_nExportType == TypeOfExport::EXPORT_OPENOFFICE_ODT_TYPE)
	{
		OpenOfficeWriterRender^ openOfficeWriterRender = gcnew OpenOfficeWriterRender(s, d, firstRow, firstCol, dataf, dataftimef, timef, repeatTitles, exportTotal);
		openOfficeWriterRender->CreateODT(cells, strarrayTitles, tot, intrarrayRowsForPage);
		delete openOfficeWriterRender;
	}
	else if (m_nExportType == TypeOfExport::EXPORT_FIELDS_TO_BASILEA)
	{
		ExcelBasileaReportsRender^ excelBasileaReportsRender = gcnew ExcelBasileaReportsRender(s,  dataf, dataftimef, timef); //TODO LARA
		excelBasileaReportsRender->CreateExcel(cells);
		delete excelBasileaReportsRender;
	}
	else if (m_nExportType == TypeOfExport::EXPORT_FIELDS_TO_TEMPLATE)
	{
		ExcelTemplateReportsRender^ excelFieldsReportsRender = gcnew ExcelTemplateReportsRender(s, templateName); 
		excelFieldsReportsRender->CreateExcel(cells);
		delete excelFieldsReportsRender;
	}
	else
	{
		ASSERT(FALSE);
		return FALSE;
	}
	System::GC::Collect();
	return TRUE;
}

//-----------------------------------------------------------------------------
MExportNet::~MExportNet(void) { System::GC::Collect(); }

//-----------------------------------------------------------------------------
CString MExportNet::ExportFullXml(CExportDataNetFull* pData, TypeOfExport m_nExportType)
{

	int tableNumber = pData->m_arTables.GetSize();
	
	int nRows = 0;
	int nCols = 0;
	int nCollsTot = 0;
	int nRowForPage = 0;


	System::String^ m_sReportNameSpace = gcnew System::String(pData->m_sReportNameSpace);

	XMLReportsRender^ xmlReportsRender = gcnew XMLReportsRender();
	String^ dom = xmlReportsRender->CreateXMLFull(m_sReportNameSpace);
	
	for (int x = 0; x < tableNumber; x++)
	{

		System::String^ tableName = gcnew System::String(pData->m_arTables[x]->m_sTableName);

		nRows = pData->m_arTables[x]->m_Cells.GetSize();
		nCols = pData->m_arTables[x]->m_arColumnTitles.GetSize();
		nCollsTot = pData->m_arTables[x]->m_TotalsRow.GetSize();

		array<String^>^ strarrayColumnName = gcnew array<String^>(nCols);
		array<Object^, 2>^ cells = gcnew array<Object^, 2>(nRows, nCols);
		array<Object^>^ tot = gcnew array<Object^>(nCollsTot);

		for (int i = 0; i < nCols; i++)
		{
			if (pData->m_arTables[x]->m_arAllColumnNames.GetSize() >0)
				strarrayColumnName[i] = gcnew String(pData->m_arTables[x]->m_arAllColumnNames[i]);
		}

		for (int i = 0; i < nCollsTot; i++)
		{
			Object^ o = gcnew String("");
			DataObj* pObj = pData->m_arTables[x]->m_TotalsRow.GetAt(i);

			if (pObj != NULL)
				o = ConverDataObj(pObj);

			tot[i] = o;
		}

		for (int r = 0; r < nRows; r++)
		{
			for (int c = 0; c < nCols; c++)
			{
				Object^ o = gcnew String("");
				DataObj* pObj = NULL;

				if (pData->m_arTables[x]->m_Cells.GetAt(r, c) != NULL)
					pObj = pData->m_arTables[x]->m_Cells.GetAt(r, c);

				if (pObj != NULL)
					o = ConverDataObj(pObj);

				cells[r, c] = o;
			}
		}

		dom = xmlReportsRender->CreateTable(cells, strarrayColumnName, tableName);
	}

	nRows = pData->m_SingleCells.GetSize();
	array<Object^, 2>^ singleCells = gcnew array<Object^, 2>(nRows, 2);

	for (int r = 0; r < nRows; r++)
	{
		for (int c = 0; c < 2; c++)
		{
			Object^ o = gcnew String("");
			DataObj* pObj = NULL;

			if (pData->m_SingleCells.GetAt(r, c) != NULL)
				pObj = pData->m_SingleCells.GetAt(r, c);

			if (pObj != NULL)
				o = ConverDataObj(pObj);

			singleCells[r, c] = o;
		}
	}

	dom = xmlReportsRender->CreateSingleCells(singleCells);

	dom = xmlReportsRender->StartParametersSection();
	
	nRows = pData->m_arAskDialogs.GetSize();
	int groupNumber;
	int entryNumber;
	for (int x = 0; x < nRows; x++)
	{
		System::String^ askDialogName = gcnew System::String(pData->m_arAskDialogs[x]->m_Names);
		System::String^ askDialogTitle = gcnew System::String(pData->m_arAskDialogs[x]->m_Title);
		dom = dom + xmlReportsRender->WriteAskDialogTag(askDialogName, askDialogTitle);
		groupNumber = pData->m_arAskDialogs[x]->m_arGroup.GetSize();
		for (int y = 0; y < groupNumber; y++)
		{
			System::String^ groupName = gcnew System::String(pData->m_arAskDialogs[x]->m_arGroup[y]->m_Names);
			System::String^ groupTitle = gcnew System::String(pData->m_arAskDialogs[x]->m_arGroup[y]->m_Title);

			dom = dom + xmlReportsRender->WriteGroupTag(groupName, groupTitle);
			entryNumber = pData->m_arAskDialogs[x]->m_arGroup[y]->m_arEntry.GetSize();
			for (int z = 0; z < entryNumber; z++)
			{
				System::String^ entryName = gcnew System::String(pData->m_arAskDialogs[x]->m_arGroup[y]->m_arEntry[z]->m_Names);
				System::String^ entryTitle = gcnew System::String(pData->m_arAskDialogs[x]->m_arGroup[y]->m_arEntry[z]->m_Title);
				System::String^ entryType = gcnew System::String(pData->m_arAskDialogs[x]->m_arGroup[y]->m_arEntry[z]->m_Type);

				System::String^ entryLength = gcnew System::String(pData->m_arAskDialogs[x]->m_arGroup[y]->m_arEntry[z]->m_Length);
				System::String^ entryControlType = gcnew System::String(pData->m_arAskDialogs[x]->m_arGroup[y]->m_arEntry[z]->m_ControlType);
				System::String^ entryValue = gcnew System::String(pData->m_arAskDialogs[x]->m_arGroup[y]->m_arEntry[z]->m_Value);

				dom =  xmlReportsRender->WriteEntryTag(entryName, entryTitle, entryType, entryLength, entryControlType, entryValue);
			}

		}
	}
//	dom = dom + xmlReportsRender->CloseParametersSection();




	xmlReportsRender->SaveFullXMLL(gcnew String(""));
	return dom;
}