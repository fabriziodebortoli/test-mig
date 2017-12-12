#include "StdAfx.h"

#include "StaticFunctions.h"
#include "MIMagoMailConnector.h"


///////////////////////////////////////////////////////////////////////////////

using namespace System;
using namespace System::Reflection;
using namespace System::Collections::Generic;


using namespace Microarea::TaskBuilderNet::UI::InifnityMailer;


//-----------------------------------------------------------------------------
BOOL MIMagoMailConnector::OpenInfinityMailer(CString infinityToken, CString connectionString, CString subject, CString mailText, CString sFrom, const CStringArray& sTo, const CStringArray& sBCC, const CStringArray& sCC, const CStringArray& attachments, bool deferrer)
{
	int sToSize = sTo.GetSize();
	int sBCCSize = sBCC.GetSize();
	int sCCSize = sCC.GetSize();
	int sAttachSize = attachments.GetSize();

	array<String^>^ strarrayTo = gcnew array<String^> (sToSize);
	array<String^>^ strarrayBcc= gcnew array<String^>(sBCCSize);
	array<String^>^ strarrayCC = gcnew array<String^>(sCCSize);
	array<String^>^ strarrayAttach = gcnew array<String^>(sAttachSize);

	for (int i = 0; i < sToSize; i++)
		strarrayTo[i]	= gcnew String(sTo[i]);

	for (int i = 0; i < sBCCSize; i++)
		strarrayBcc[i] = gcnew String(sBCC[i]);

	for (int i = 0; i < sCCSize; i++)
		strarrayCC[i] = gcnew String(sCC[i]);

	for (int i = 0; i < sAttachSize; i++)
		strarrayAttach[i] = gcnew String(attachments[i]);

	InfinityMailer^ infinityMailer = gcnew InfinityMailer(gcnew System::String(infinityToken), gcnew System::String(connectionString));

	infinityMailer->OpenInfinityMailer(gcnew System::String(subject), gcnew System::String(mailText), gcnew System::String(sFrom), strarrayTo, strarrayBcc, strarrayCC, strarrayAttach, deferrer);
	//int nColsexp = pData->m_Table.m_Cells[0]->GetSize();


	//int nRows = pData->m_Table.m_Cells.GetSize();
	//int nCols = pData->m_Table.m_arColumnTitles.GetSize();
	//int nCollsTot = pData->m_Table.m_TotalsRow.GetSize();
	//int nRowForPage = pData->m_Table.m_arItemForPages.GetSize();
	//int fileFormat = pData->m_FileFormatType;

	//array<String^>^ strarrayColumnName = gcnew array<String^> (nColsexp);
	//array<String^>^ strarrayTitles = gcnew array<String^> (nCols);
	//array<Object^, 2>^ cells = gcnew array<Object^, 2> (nRows, nColsexp);
	//array<int>^ intrarrayRowsForPage = gcnew array<int>(nRowForPage);

	//array<Object^>^ tot = gcnew array<Object^> (nCollsTot);

	//int nph = pData->m_parPlaceHolders.GetCount();

	//System::Collections::Generic::List<SharpPlaceHolderExportInfo^>^ placeholder = gcnew System::Collections::Generic::List<SharpPlaceHolderExportInfo^> ();

	//for (int i = 0; i < nph; i++)
	//{
	//	SharpPlaceHolderExportInfo^ sharp = gcnew SharpPlaceHolderExportInfo();
	//	CPlaceHolderExportInfo* infos = pData->m_parPlaceHolders[i];
	//	sharp->IsLike = infos->m_bIsLike;
	//	sharp->TagID = gcnew System::String(infos->m_sTagID);
	//	sharp->SelectedColumnName = gcnew array<String^> (infos->m_arSelectedColumnName.GetSize());
	//	
	//	for (int ii = 0; ii < infos->m_arSelectedColumnName.GetSize(); ii++)
	//		sharp->SelectedColumnName[ii] = gcnew String( infos->m_arSelectedColumnName[ii] );
	//	
	//	sharp->ColumnFilterName = gcnew System::String(infos->m_sColumnFilterName);
	//	sharp->FilterValue =  ConverDataObj(infos->m_pFilterValue);
	//	sharp->ColumnFilterIndex = (int)(infos->m_ColumnFilterIndex);

	//	List<Int32>^ list = gcnew List<Int32>();
	//	int nCols2 = pData->m_parPlaceHolders[i]->m_arColumnIndex.GetSize();

	//	for (int ii = 0; ii < nCols2; ii++)
	//	{
	//		int a = pData->m_parPlaceHolders[i]->m_arColumnIndex[ii];
	//		list->Add(a);
	//	}

	//	sharp->ColumnIndex = list;
	//	placeholder->Add(sharp);
	//}

	//for (int i = 0; i < nCols; i++)
	//{
	//	strarrayTitles[i]		= gcnew String(pData->m_Table.m_arColumnTitles[i] );

	//	if (pData->m_Table.m_arAllColumnNames.GetSize() >0)
	//		strarrayColumnName[i]	= gcnew String (pData->m_Table.m_arAllColumnNames[i]);
	//}

	//for (int i = 0; i < nRowForPage; i++)
	//	intrarrayRowsForPage[i] = (int)pData->m_Table.m_arItemForPages[i];

	//
	//for (int i = 0; i < nCollsTot; i++)
	//{
	//	Object^ o = gcnew String("");
	//	DataObj* pObj = pData->m_Table.m_TotalsRow.GetAt(i);

	//	if (pObj != NULL)
	//		o = ConverDataObj(pObj);
	//	
	//	tot[i]= o;
	//}

	//for (int r = 0; r < nRows; r++)
	//{
	//	for (int c = 0; c < nColsexp; c++)
	//	{
	//		Object^ o = gcnew String("");
	//		DataObj* pObj = NULL;

	//		if (pData->m_Table.m_Cells.GetAt(r, c) != NULL)
	//			pObj = pData->m_Table.m_Cells.GetAt(r, c);

	//		if (pObj != NULL)
	//			o = ConverDataObj(pObj);

	//		cells [r, c] = o;
	//	}
	//}

	//System::String^ s				= gcnew System::String(pData->m_sFileName);
	//System::String^ d				= gcnew System::String(pData->m_Sheet);
	//System::String^ templateName	= gcnew System::String(pData->m_sTemplate);

	//int		firstRow		= pData->firstRow + 1;
	//int		firstCol		= pData->firstColumn + 1;
	//bool	repeatTitles	= (pData->m_bRepeatColumnTitles == TRUE);
	//bool	exportTotal		= (pData->m_bExportTotals == TRUE);
	//bool	autoSave		= (pData->m_bAutoSave == TRUE);
	//bool	autoPrint		= (pData->m_bAutoPrint == TRUE);
	//bool	cvsFormat		= (pData->m_bCSVFormat == TRUE);
	//bool	isClipboard		= (pData->m_bisClipboard == TRUE);

	//System::String^ dataf			= gcnew System::String(pData->m_sDataFormat);
	//System::String^ dataftimef		= gcnew System::String(pData->m_SDataTimeFormat);
	//System::String^ timef			= gcnew System::String(pData->m_STimeFormat);
	//
	//

	//if (m_nExportType == 4)
	//{
	//	ExcelReportsRender^ excelReportsRender = gcnew ExcelReportsRender(s, d, firstRow, firstCol, dataf, dataftimef, timef, repeatTitles, exportTotal, autoSave, autoPrint);
	//	excelReportsRender->CreateExcel(cells, strarrayTitles, tot, intrarrayRowsForPage);
	//}

	//if (m_nExportType == 5)
	//{
	//	OpenOfficeReportsRender^ openOfficeReportsRender = gcnew OpenOfficeReportsRender(s, d, firstRow, firstCol, dataf, dataftimef, timef, repeatTitles, exportTotal);
	//	openOfficeReportsRender->CreateODS(cells, strarrayTitles, tot, intrarrayRowsForPage);
	//}
	//
	//if (m_nExportType == 6)
	//{
	//	DocxReportsRender^ docx = gcnew  DocxReportsRender(s, dataf, dataftimef, timef, repeatTitles, exportTotal);
	//	docx->CreateDocx(cells, strarrayTitles, tot, intrarrayRowsForPage);
	//}

	//if (m_nExportType == 2)
	//{
	//	XMLReportsRender^ xmlReportsRender = gcnew XMLReportsRender(s, dataf, dataftimef, timef, d);
	//	xmlReportsRender->CreateXML(cells, strarrayColumnName);
	//}

	//
	//if (m_nExportType == 7)
	//{
	//	DocxReportsRender^ docx = gcnew  DocxReportsRender(s, dataf, dataftimef, timef, repeatTitles, exportTotal);
	//	docx->CreateDocxForMerge(cells, strarrayTitles, placeholder, tot);
	//}
	//
	//if (m_nExportType ==1)
	//{
	//	HTMLReportsRender^ docx = gcnew  HTMLReportsRender(s, dataf, dataftimef, timef);
	//	docx->CreateHTMLForMerge(cells, strarrayTitles);
	//}

	//if (m_nExportType == 0 || m_nExportType == 3)
	//{
	//	TxtReportsRender^ docx = gcnew  TxtReportsRender(s, dataf, dataftimef, timef, fileFormat, cvsFormat, isClipboard);
	//	docx->CreateTxtForMerge(cells, strarrayTitles);
	//}

	//if (m_nExportType == 8)
	//{
	//	OpenOfficeWriterRender^ openOfficeWriterRender = gcnew OpenOfficeWriterRender(s, d, firstRow, firstCol, dataf, dataftimef, timef, repeatTitles, exportTotal, autoPrint);
	//	openOfficeWriterRender->CreateODT(cells, strarrayTitles, tot, intrarrayRowsForPage);
	//}

	//if (m_nExportType == 10)
	//{
	//	ExcelBasileaReportsRender^ excelBasileaReportsRender = gcnew ExcelBasileaReportsRender(s,  dataf, dataftimef, timef); //TODO LARA
	//	excelBasileaReportsRender->CreateExcel(cells);
	//}

	//if (m_nExportType == 9)
	//{
	//	ExcelTemplateReportsRender^ excelFieldsReportsRender = gcnew ExcelTemplateReportsRender(s, templateName); 
	//	excelFieldsReportsRender->CreateExcel(cells);
	//}
	return TRUE;
}

