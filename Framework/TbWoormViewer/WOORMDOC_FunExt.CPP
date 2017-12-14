#include "stdafx.h"

#include <io.h>
#include <sys\stat.h>
#include <mmsystem.h>
#include <stdlib.h>

#include <TbGeneric\dib.h>
#include <TbGeneric\RdeProtocol.h>
#include <TbGeneric\FontsTable.h>
#include <TbGeneric\stack.h>
#include <TbGeneric\globals.h>
#include <TbGeneric\dibitmap.h>
#include <TbGeneric\ParametersSections.h>

#include <TbGenlib\baseapp.h>
#include <TbGenlibUI\FormatDialog.h>
#include <TbGenlib\funproto.h>
#include <TbGenlib\reswalk.h>
#include <TbGenlib\addonmng.h>
#include <TbGenlib\oslinfo.h> 
#include <TbGenlib\baseapp.h>

#include <TbGenlibManaged\ExpandTemplateMng.h>

#include <TbWebServicesWrappers\LockManagerInterface.h>
 
#include <TbWoormEngine\repfield.h>
#include <TbWoormEngine\reptable.h>
#include <TbWoormEngine\report.h>
#include <TbWoormEngine\repengin.h> 
#include <TbWoormEngine\ExportSymbols.h>
#include <TbWoormEngine\procdata.h>
#include <TbWoormEngine\edtmng.h>

#include <TbWebServicesWrappers\LoginManagerInterface.h>

#include "pageinfo.h"
#include "PredefinedWoormFunctions.h"
#include "BaseObj.h"
#include "table.h"
#include "repeater.h"
#include "woormdoc.h"
#include "export.h"

//................................. resources
#include "woormdoc.hjson" //JSON AUTOMATIC UPDATE
#include "listdlg.hjson" //JSON AUTOMATIC UPDATE

//includere come ultimo include all'inizio del cpp
#include "beginh.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char  THIS_FILE[] = __FILE__;
#endif

#define new DEBUG_NEW

/////////////////////////////////////////////////////////////////////////////
//							CWoormFileDialog:save PDF
/////////////////////////////////////////////////////////////////////////////
//
//-----------------------------------------------------------------------------                      
IMPLEMENT_DYNAMIC(CWoormFileDialogPDF, CFileDialog)

//-----------------------------------------------------------------------------                      
CWoormFileDialogPDF::CWoormFileDialogPDF
			(
				BOOL bOpenFileDialog, 
				LPCTSTR lpszDefExt, 
				LPCTSTR lpszFileName,	
				DWORD dwFlags, 
				LPCTSTR lpszFilter, 
				CWnd* pParentWnd
			) 
			:
			CFileDialog(bOpenFileDialog, lpszDefExt, lpszFileName, dwFlags, lpszFilter, pParentWnd)
{
	m_ofn.Flags |= OFN_ENABLETEMPLATE;
	m_ofn.hInstance = AfxFindResourceHandle(MAKEINTRESOURCE(IDD_CUSTOMFILEDIALOG_PDF), RT_DIALOG);	 
	m_ofn.lpTemplateName = MAKEINTRESOURCE(IDD_CUSTOMFILEDIALOG_PDF);

	MailConnectorParams* params = AfxGetIMailConnector()->GetParams();

	m_bCompress   = params->GetMailCompress();

	m_bPreview = FALSE;
}

//-----------------------------------------------------------------------------                      
BEGIN_MESSAGE_MAP(CWoormFileDialogPDF, CFileDialog)
	//{{AFX_MSG_MAP(CWoormFileDialog)
		// NOTE - the ClassWizard will add and remove mapping macros here.
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------                      

void CWoormFileDialogPDF::OnInitDone()
{
	CFileDialog::OnInitDone();	

	((CButton*)GetDlgItem(IDC_CUSTOMFILEDIALOG_PDF_COMPRESS))->SetCheck(m_bCompress);
	((CButton*)GetDlgItem(IDC_CUSTOMFILEDIALOG_PDF_PREVIEW))->SetCheck(m_bPreview);
}

//-----------------------------------------------------------------------------                      
BOOL CWoormFileDialogPDF::OnFileNameOK()
{
	m_bCompress = IsDlgButtonChecked(IDC_CUSTOMFILEDIALOG_PDF_COMPRESS);
	m_bPreview = IsDlgButtonChecked(IDC_CUSTOMFILEDIALOG_PDF_PREVIEW);

	return CFileDialog::OnFileNameOK();
}

//=====================================================================================

BOOL ParseFlags(CWoormInfo* pWoormInfo, CString strFlags, int& nWaitEnd, int& bReturnValue, BOOL& bReturnAttach, BOOL& bTempConcatPDF)
{
	CString strEntry, strValue;
	int nSep,nEq;
	nWaitEnd = 0;

	if(strFlags.IsEmpty())
		return TRUE;

	MailConnectorParams* params = AfxGetIMailConnector()->GetParams();

	do 
	{
		nSep = strFlags.Find('|');
		if (nSep >= 0)
		{
			strEntry = strFlags.Left(nSep);
			strFlags = strFlags.Mid(nSep + 1);
		}
		else
		{
			strEntry = strFlags;
		}
		
		nEq = strEntry.Find('=');
		if (nEq >= 0)
		{
			strValue = strEntry.Right(strEntry.GetLength() - nEq - 1);
			strEntry = strEntry.Left(nEq);

			strEntry.Trim();
			strValue.Trim();

			int nV = _tstoi(strValue);
			if (errno == ERANGE)
				nV = 0;
			if (nV == 0 && strValue.CompareNoCase(_T("true")) ==0)
				nV = 1;

			if (strEntry.CompareNoCase(_T("WaitEnd")) == 0)
			{
				if (nV == 1)
					nWaitEnd = 1;
				else if (nV == 2)
					nWaitEnd = 2;

				if (nV != 0 && nV != 1 && nV != 2) return FALSE;
			}
			else if (strEntry.CompareNoCase(_T("ReturnValue")) == 0)
			{
				bReturnValue = (nV == 1);
				if (nV != 0 && nV != 1) return FALSE;
			}
			else if (strEntry.CompareNoCase(_T("Iconized")) == 0)
			{
				pWoormInfo->m_bIconized = (nV == 1);
				if (nV != 0 && nV != 1) return FALSE;
			}
			else if (strEntry.CompareNoCase(_T("Hide")) == 0)
			{
				pWoormInfo->m_bHideFrame = (nV == 1);
				if (nV != 0 && nV != 1) return FALSE;
			}
			else if (strEntry.CompareNoCase(_T("AutoPrint")) == 0) 
			{
				pWoormInfo->m_bAutoPrint = nV; //(nV == 1);
				if (nV != 0 && nV != 1 && nV != 2) return FALSE;
			}
			else if (strEntry.CompareNoCase(_T("CloseOnEndPrint")) == 0) 
			{
				pWoormInfo->m_bCloseOnEndPrint = (nV == 1);
				if (nV != 0 && nV != 1) return FALSE;
			}
			else if (strEntry.CompareNoCase(_T("ShowPrintDialog")) == 0) 
			{
				pWoormInfo->m_bNoPrintDialog = (nV == 0);
				if (nV != 0 && nV != 1) return FALSE;
			}
			else if (strEntry.CompareNoCase(_T("NumberOfCopies")) == 0) 
			{
				if (nV > 0)
					pWoormInfo->SetCopies(nV);
				else return FALSE;
			}
			else if (strEntry.CompareNoCase(_T("Zip")) == 0) 
			{
				pWoormInfo->m_bCompressAttach = (nV == 1);
				if (nV != 0 && nV != 1) return FALSE;
			}
			else if (strEntry.CompareNoCase(_T("ConcatPDF")) == 0) 
			{
				pWoormInfo->m_bConcatPDF = (nV == 1);
				
				if (nV != 0 && nV != 1) return FALSE;
			}
			else if (strEntry.CompareNoCase(_T("PostaLite")) == 0) 
			{
				pWoormInfo->m_bSendEmail = (nV == 1);
				
				pWoormInfo->m_bAttachPDF = TRUE;
				pWoormInfo->m_bAttachRDE = FALSE;
				pWoormInfo->m_bAttachOther = FALSE;
				pWoormInfo->m_bCompressAttach = FALSE;

				if (nV != 0 && nV != 1) return FALSE;
			}
			else if (strEntry.CompareNoCase(_T("SendMail")) == 0)
			{
				if (nV < 0 || nV > 8) return FALSE;

				pWoormInfo->m_bSendEmail = (nV != 0);

				pWoormInfo->m_bAttachPDF = FALSE;
				pWoormInfo->m_bAttachRDE = FALSE;
				pWoormInfo->m_bAttachOther = FALSE;
				pWoormInfo->m_bCompressAttach = FALSE;

				switch (nV)
				{
					case 0:
						//suppongo nV == 0 perche in strValue c'e' una stringa
						if (strValue.CompareNoCase(_T("PDF")) == 0) 
						{
							pWoormInfo->m_bSendEmail = pWoormInfo->m_bAttachPDF = TRUE;
							break;
						}
						else if (strValue.CompareNoCase(_T("RDE")) == 0) 
						{
							pWoormInfo->m_bSendEmail = pWoormInfo->m_bAttachRDE = TRUE;
							break;
						} 
						//----
						pWoormInfo->m_bSendEmail = pWoormInfo->m_bAttachOther = TRUE;

						if (strValue.CompareNoCase(_T("XLS")) == 0 || strValue.CompareNoCase(_T("XLSX")) == 0)
						{
							CExportInfo* pEInfo = new CExportInfo();
							pEInfo->SetExportType(TypeOfExport::EXPORT_EXCELNET_TYPE);
							pWoormInfo->m_arExportInfo.Add(pEInfo);
						}
						else if (strValue.CompareNoCase(_T("OXLSX")) == 0)
						{
							CExportInfo* pEInfo = new CExportInfo();
							pEInfo->SetExportType(TypeOfExport::EXPORT_OPENXML_EXCEL_TYPE);
							pWoormInfo->m_arExportInfo.Add(pEInfo);
						}
						else if (strValue.CompareNoCase(_T("DOC")) == 0 || strValue.CompareNoCase(_T("DOCX")) == 0)
						{
							CExportInfo* pEInfo = new CExportInfo();
							pEInfo->SetExportType(TypeOfExport::EXPORT_WORDNET_TYPE);
							pWoormInfo->m_arExportInfo.Add(pEInfo);
						}
						else if (strValue.CompareNoCase(_T("ODS")) == 0) 
						{		
							CExportInfo* pEInfo = new CExportInfo();
							pEInfo->SetExportType(TypeOfExport::EXPORT_OPENOFFICE_ODS_TYPE);
							pWoormInfo->m_arExportInfo.Add(pEInfo);
						}
						else if (strValue.CompareNoCase(_T("ODT")) == 0)
						{
							CExportInfo* pEInfo = new CExportInfo();
							pEInfo->SetExportType(TypeOfExport::EXPORT_OPENOFFICE_ODT_TYPE);
							pWoormInfo->m_arExportInfo.Add(pEInfo);
						}
						else if (strValue.CompareNoCase(_T("CSV")) == 0 || strValue.CompareNoCase(_T("TXT")) == 0) 
						{
							CExportInfo* pEInfo = new CExportInfo();
							pEInfo->SetExportType(TypeOfExport::EXPORT_CSV_TYPE);
							pWoormInfo->m_arExportInfo.Add(pEInfo);
						}
						else if (strValue.CompareNoCase(_T("HTML")) == 0 || strValue.CompareNoCase(_T("HTM")) == 0) 
						{
							CExportInfo* pEInfo = new CExportInfo();
							pEInfo->SetExportType(TypeOfExport::EXPORT_HTML_TYPE);
							pWoormInfo->m_arExportInfo.Add(pEInfo);
						}
						else if (strValue.CompareNoCase(_T("XML")) == 0) 
						{
							CExportInfo* pEInfo = new CExportInfo();
							pEInfo->SetExportType(TypeOfExport::EXPORT_XML_TYPE);
							pWoormInfo->m_arExportInfo.Add(pEInfo);
						}
						else if (strValue.CompareNoCase(_T("FXML")) == 0)
						{
							CExportInfo* pEInfo = new CExportInfo();
							pEInfo->SetExportType(TypeOfExport::EXPORT_XML_FULL_TYPE);
							pWoormInfo->m_arExportInfo.Add(pEInfo);
						}
						else	//formato non riconosciuto
							/*pWoormInfo->m_bSendEmail = */pWoormInfo->m_bAttachOther = FALSE;
						break;

					case 5:
						break;

					case 1:
						pWoormInfo->m_bAttachPDF = TRUE;
						pWoormInfo->m_bCompressAttach = TRUE;
						break;
					case 2:
						pWoormInfo->m_bAttachPDF = TRUE;
						break;
					case 3:
						pWoormInfo->m_bAttachRDE = TRUE;
						pWoormInfo->m_bCompressAttach = TRUE;
						break;
					case 4:
						pWoormInfo->m_bAttachRDE = TRUE;
						break;
					case 6:
						pWoormInfo->m_bAttachPDF = TRUE;
						pWoormInfo->m_bCompressAttach = params->GetMailCompress();
						break;
					case 7:
						pWoormInfo->m_bAttachRDE = TRUE;
						pWoormInfo->m_bCompressAttach = params->GetMailCompress();
						break;
					case 8:
						pWoormInfo->m_bAttachPDF = TRUE;
						bTempConcatPDF = pWoormInfo->m_bConcatPDF = TRUE;
						break;
				}
			}
			else if (strEntry.CompareNoCase(_T("TemplateEmail")) ==0) 
			{
				if (nV == 0 )
				{
					// Se ho messo il tag ma non ho un valore non esco con errore ma proseguo.
					if (!strValue.IsEmpty())
						pWoormInfo->m_Email.SetTemplateFileName(strValue);
				}
				else return FALSE;
			}
			else if (strEntry.CompareNoCase(_T("PrinterName")) == 0)
			{
				if (nV == 0)
				{
					if (!strValue.IsEmpty())
					{
						pWoormInfo->m_strPrinterName = strValue;
						pWoormInfo->m_bNoPrintDialog = TRUE;
					}
				}
				else return FALSE;
			}
			else if (strEntry.CompareNoCase(_T("AttachmentReportName")) == 0)
			{
				if (nV == 0)
				{
					// Se ho messo il tag ma non ho un valore non esco con errore ma proseguo.
					if (!strValue.IsEmpty())
						pWoormInfo->m_Email.SetAttachmentReportName(strValue);
				}
				else return FALSE;
			}
			else if (strEntry.CompareNoCase(_T("AttachmentPath")) ==0)
			{
				if (nV == 0)
				{
					// Se ho messo il tag ma non ho un valore non esco con errore ma proseguo.
					if (!strValue.IsEmpty())
						pWoormInfo->AddOutputFileName(strValue);
				}
				else return FALSE;
			}
			else if (strEntry.CompareNoCase(_T("ReturnAttach")) ==0)
			{
				if (nV < 0 || nV == 7 || nV > 8) return FALSE;

				pWoormInfo->m_bSendEmail = FALSE;

				pWoormInfo->m_bAttachPDF = FALSE;
				pWoormInfo->m_bAttachRDE = FALSE;
				pWoormInfo->m_bAttachOther = FALSE;
				pWoormInfo->m_bCompressAttach = FALSE;

				bReturnAttach = TRUE;
				nWaitEnd = 2;
				switch(nV)
				{
					case 0:
						//suppongo nV == 0 perche in strValue c'e' una stringa
						if (strValue.CompareNoCase(_T("PDF")) == 0) 
						{
							pWoormInfo->m_bAttachPDF = TRUE;
							break;
						}
						else if (strValue.CompareNoCase(_T("RDE")) == 0) 
						{
							pWoormInfo->m_bAttachRDE = TRUE;
							break;
						}
						//----
						pWoormInfo->m_bAttachOther = TRUE;

						if (strValue.CompareNoCase(_T("XLS")) == 0 || strValue.CompareNoCase(_T("XLSX")) == 0)
						{
							CExportInfo* pEInfo = new CExportInfo();
							pEInfo->SetExportType(TypeOfExport::EXPORT_EXCELNET_TYPE);
							pWoormInfo->m_arExportInfo.Add(pEInfo);
						}
						else if (strValue.CompareNoCase(_T("OXLSX")) == 0)
						{
							CExportInfo* pEInfo = new CExportInfo();
							pEInfo->SetExportType(TypeOfExport::EXPORT_OPENXML_EXCEL_TYPE);
							pWoormInfo->m_arExportInfo.Add(pEInfo);
						}
						else if (strValue.CompareNoCase(_T("DOC")) == 0 || strValue.CompareNoCase(_T("DOCX")) == 0)
						{
							CExportInfo* pEInfo = new CExportInfo();
							pEInfo->SetExportType(TypeOfExport::EXPORT_WORDNET_TYPE);
							pWoormInfo->m_arExportInfo.Add(pEInfo);
						}

						else if (strValue.CompareNoCase(_T("ODS")) == 0) 
						{
							CExportInfo* pEInfo = new CExportInfo();
							pEInfo->SetExportType(TypeOfExport::EXPORT_OPENOFFICE_ODS_TYPE);
							pWoormInfo->m_arExportInfo.Add(pEInfo);
						}
						else if (strValue.CompareNoCase(_T("ODT")) == 0)
						{
							CExportInfo* pEInfo = new CExportInfo();
							pEInfo->SetExportType(TypeOfExport::EXPORT_OPENOFFICE_ODT_TYPE);
							pWoormInfo->m_arExportInfo.Add(pEInfo);
						}

						else if (strValue.CompareNoCase(_T("CSV")) == 0 || strValue.CompareNoCase(_T("TXT")) == 0) 
						{
							CExportInfo* pEInfo = new CExportInfo();
							pEInfo->SetExportType(TypeOfExport::EXPORT_CSV_TYPE);
							pWoormInfo->m_arExportInfo.Add(pEInfo);
						}
						else if (strValue.CompareNoCase(_T("HTML")) == 0 || strValue.CompareNoCase(_T("HTM")) == 0) 
						{
							CExportInfo* pEInfo = new CExportInfo();
							pEInfo->SetExportType(TypeOfExport::EXPORT_HTML_TYPE);
							pWoormInfo->m_arExportInfo.Add(pEInfo);
						}
						else if (strValue.CompareNoCase(_T("XML")) == 0) 
						{
							CExportInfo* pEInfo = new CExportInfo();
							pEInfo->SetExportType(TypeOfExport::EXPORT_XML_TYPE);
							pWoormInfo->m_arExportInfo.Add(pEInfo);
						}
						else if (strValue.CompareNoCase(_T("FXML")) == 0)
						{
							CExportInfo* pEInfo = new CExportInfo();
							pEInfo->SetExportType(TypeOfExport::EXPORT_XML_FULL_TYPE);
							pWoormInfo->m_arExportInfo.Add(pEInfo);
						}
						else	//formato non riconosciuto
							pWoormInfo->m_bAttachOther = FALSE;
						break;

					case 1:
						pWoormInfo->m_bAttachPDF = TRUE;
						pWoormInfo->m_bCompressAttach = TRUE;
						break;
					case 2:
						pWoormInfo->m_bAttachPDF = TRUE;
						break;
					case 3:
						pWoormInfo->m_bAttachRDE = TRUE;
						pWoormInfo->m_bCompressAttach = TRUE;
						break;
					case 4:
						pWoormInfo->m_bAttachRDE = TRUE;
						break;
					case 5:
						pWoormInfo->m_bAttachPDF = TRUE;
						pWoormInfo->m_bCompressAttach = params->GetMailCompress();
						break;
					case 6:
						pWoormInfo->m_bAttachRDE = TRUE;
						pWoormInfo->m_bCompressAttach = params->GetMailCompress();
						break;
					case 8:
						pWoormInfo->m_bAttachPDF = TRUE;
						bTempConcatPDF = pWoormInfo->m_bConcatPDF = TRUE;
						break;
				}
			}
			else if (strEntry.CompareNoCase(_T("MultiCopies")) ==0)
			{
				if (nV == 1)
				{
					pWoormInfo->m_bIconized = TRUE;
					pWoormInfo->m_bCloseOnEndPrint = TRUE;
					pWoormInfo->m_bAutoPrint = 2;
					nWaitEnd = 2;
				}
				if (nV != 0 && nV != 1) return FALSE;
			}
			else if (strEntry.CompareNoCase(_T("UICulture")) ==0)
			{
				if (nV == 0)
				{
					if (!strValue.IsEmpty())
						pWoormInfo->m_sUICulture = strValue;
				}
				else return FALSE;
			}
			else if (strEntry.CompareNoCase(_T("Archive")) ==0)
			{
				if (nV != 0 && nV != 1 && nV != 2) return FALSE;

				pWoormInfo->m_bIconized = TRUE;
				pWoormInfo->m_bCloseOnEndPrint = TRUE;
				pWoormInfo->m_bArchivePdfFormat = nV;
				nWaitEnd = 2;
			}
			else return FALSE;	
		}
		else return FALSE;
	} 
	while (nSep >= 0 && strFlags.GetLength());
	
	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL ParseFlags(CWoormInfo* pWoormInfo, CString strFlags)
{
	if (strFlags.IsEmpty())
		return TRUE;

	CString strEntry, strValue;
	int nSep,nEq;

	do 
	{
		nSep = strFlags.Find('|');
		if (nSep >= 0)
		{
			strEntry = strFlags.Left(nSep);
			strFlags = strFlags.Mid(nSep + 1);
		}
		else
		{
			strEntry = strFlags;
		}
		
		nEq = strEntry.Find('=');
		if (nEq >= 0)
		{
			strValue = strEntry.Mid(nEq + 1);
			strEntry = strEntry.Left(nEq);

			strEntry.Trim();
			strValue.Trim();

			int nV = _tstoi(strValue);
			if (errno == ERANGE)
				nV = 0;
			if (nV == 0 && strValue.CompareNoCase(_T("true")) == 0)
				nV = 1;

			if (strEntry.CompareNoCase(_T("CloseOnEndPrint")) == 0) 
			{
				pWoormInfo->m_bCloseOnEndPrint = (nV == 1);
				if (nV != 0 && nV != 1) return FALSE;
			}
			else if (strEntry.CompareNoCase(_T("Zip")) == 0) 
			{
				pWoormInfo->m_bCompressAttach = (nV == 1);
				if (nV != 0 && nV != 1) return FALSE;
			}
			else if (strEntry.CompareNoCase(_T("ConcatPDF")) == 0) 
			{
				pWoormInfo->m_bConcatPDF = (nV == 1);
				
				if (nV != 0 && nV != 1) return FALSE;
			}

			else return FALSE;	
		}
		else return FALSE;
	} 
	while (nSep >= 0 && strFlags.GetLength());
	
	return TRUE;
}

//-------------------------------------------------------------
BOOL CWoormDocMng::DispatchFunctionCall(CFunctionDescription* pRDI)
{
	BOOL bOk = TRUE;
	CString strName;
	BOOL nWaitEnd = 0;
	BOOL bReturnValue = FALSE;

	CString sNs = pRDI->GetNamespace().ToUnparsedString();
	CString strF = pRDI->GetName();
	int nIdx = sNs.ReverseFind('.');
	if (nIdx > 0)
	{
		CString sPrefix = sNs.Left(nIdx);
		if (
			sPrefix.CompareNoCase(_T("Woorm")) 
			&& 
			sPrefix.CompareNoCase(_T("RS"))
			&&
			sPrefix.CompareNoCase(_T("Framework.TbWoormViewer.TbWoormViewer"))
			)
			return FALSE;
	}

	//se il nome coincide con il namespace potrebbbe essere una procedure
	if (sNs.IsEmpty() && m_pEngine && m_pEngine->m_pSymTable)
	{
		ProcedureObjItem* proc = m_pEngine->m_pSymTable->FindProcedure(strF);
		if (proc)
		{
			if (!proc->m_pProcedure)
				return FALSE;
			if (!proc->m_pProcedure->GetFun())
				return FALSE;
			
			if (!proc->m_pProcedure->GetFun()->SetParametersValue	(pRDI->GetParameters()))
				return FALSE;

			if (!proc->m_pProcedure->Exec())
				return FALSE;

			if (
				!DataType::IsCompatible(
					proc->m_pProcedure->GetFun()->GetReturnValue()->GetDataType(), 
					pRDI->GetReturnValue()->GetDataType())
				)
				return FALSE;

			DataObj* pObj = proc->m_pProcedure->GetFun()->GetReturnValue();
			ASSERT(pObj->IsValid());
			pRDI->GetReturnValue()->Assign(*pObj);

			return TRUE;
		}
	}
	//-------------------------------
	bOk = DispatchFunctionCallSync(pRDI, strF);

	if (!bOk)
	{
		if (this->IsAWoormRunningMultithread())
		{
			HWND hwndThread = GetFrameHandle();
			bOk = AfxInvokeThreadFunction<DataBool, CWoormDocMng, DataLng, DataStr>(hwndThread, this, &CWoormDocMng::DispatchFunctionCallAsyncWrp, DataLng((long)pRDI), DataStr(strF));
		}
		else
			bOk = DispatchFunctionCallAsync(pRDI, strF);
	}

	return bOk;
}

//-------------------------------------------------------------
DataBool CWoormDocMng::DispatchFunctionCallAsyncWrp(DataLng dlRDI, DataStr strF)
{
	CFunctionDescription* pRDI = (CFunctionDescription*)(long) dlRDI;

	BOOL bOk = DispatchFunctionCallAsync(pRDI, strF.GetString());

	return bOk;
}
//-------------------------------------------------------------
BOOL CWoormDocMng::DispatchFunctionCallAsync(CFunctionDescription* pRDI, const CString& strF)
{
	BOOL bOk = TRUE;
	CString strName;
	BOOL nWaitEnd = 0;
	BOOL bReturnValue = FALSE;

	if (strF == IDF_WOORM_RUNREPORT)
	{    
		if (pRDI->GetParameters().GetSize() < 1)
		{
			pRDI->SetReturnValue((DataBool)false);
			return TRUE;
		}

		strName =  ((DataStr*)(pRDI->GetParamValue(0)))->GetString();
		CString strFlags;
		int nSep = strName.Find(_T("|"));
		if (nSep > -1)
		{
			strFlags = strName.Right(strName.GetLength() -nSep-1);
			strName = strName.Left(nSep).Trim();
		}

		CWoormInfo* pWoormInfo = new CWoormInfo;
		pWoormInfo->m_bIsChildReport = TRUE;
		pWoormInfo->m_ReportNames.Add(strName);
		
		pWoormInfo->m_bNoPrintDialog	= TRUE;
		pWoormInfo->m_nCopies			= m_pWoormInfo ? m_pWoormInfo->m_nCopies : 1;

		pWoormInfo->m_bCloseOnEndPrint	= m_bCloseChildOnEndPrint || (m_pWoormInfo ? m_pWoormInfo->m_bCloseOnEndPrint : FALSE);
		pWoormInfo->m_bAutoPrint		= m_pWoormInfo ? m_pWoormInfo->m_bAutoPrint : FALSE;
		pWoormInfo->m_bArchivePdfFormat	= m_pWoormInfo ? m_pWoormInfo->m_bArchivePdfFormat : FALSE;
		pWoormInfo->m_bIconized			= m_pWoormInfo ? m_pWoormInfo->m_bIconized : FALSE;
		pWoormInfo->m_bHideFrame		= m_pWoormInfo ? m_pWoormInfo->m_bHideFrame : FALSE;
		pWoormInfo->m_strPrinterName	= m_pWoormInfo ? m_pWoormInfo->m_strPrinterName : _T("");
		pWoormInfo->m_bLockedCopies		= m_pWoormInfo ? m_pWoormInfo->m_bLockedCopies : FALSE;
		pWoormInfo->m_bMultiCopies		= m_bMultiCopies || (m_pWoormInfo ? m_pWoormInfo->m_bMultiCopies : FALSE);
		pWoormInfo->m_bConcatPDF		= m_pWoormInfo ? m_pWoormInfo->m_bConcatPDF : FALSE;
		pWoormInfo->m_sUICulture		= m_pWoormInfo ? m_pWoormInfo->m_sUICulture : m_sStartUICulture;

		BOOL bReturnAttach = FALSE;
		int nOldCopies = pWoormInfo->m_nCopies;
		if (!ParseFlags(pWoormInfo, strFlags, nWaitEnd, bReturnValue, bReturnAttach, m_bDirectConcatPDF))
		{
			if (m_pWoormInfo ? !m_pWoormInfo->m_bHideFrame : TRUE)
				AfxMessageBox(_TB("RunReport fails because it was called with wrong arguments") + L" - " + strFlags);
			delete pWoormInfo;
			pRDI->SetReturnValue((DataBool)false);
			return TRUE;
		}
		
		if (nOldCopies !=  pWoormInfo->m_nCopies && !pWoormInfo->m_bLockedCopies)
			pWoormInfo->m_bLockedCopies = TRUE;

		if (bReturnValue && nWaitEnd == 0) 
			nWaitEnd = 1;

		//se i report figli sono nascosti devono chiudersi in automatico
		if (pWoormInfo->m_bHideFrame) 
		{
			pWoormInfo->m_bCloseOnEndPrint = TRUE; 
			/*if (nWaitEnd == 0) */nWaitEnd = 2;
		}
		//se devo stampare i report figli non possono essere nascosti 
		if (pWoormInfo->m_bAutoPrint || pWoormInfo->m_bArchivePdfFormat) 
		{
			if (pWoormInfo->m_bHideFrame)
			{
				pWoormInfo->m_bHideFrame = FALSE;
				pWoormInfo->m_bIconized = TRUE; 
			}
		}

		pWoormInfo->m_bOwnedByReport = (nWaitEnd == 0); 

		//Se non siamo in stampa 
		CWoormView* pView = GetWoormView();

		if ( 
				(m_bRunWithOutChild || (m_pWoormInfo ? m_pWoormInfo->m_bAutoPrint : FALSE))
				&& 
				pWoormInfo->m_bAutoPrint == 0
				&& 
				!bReturnValue 
				&& 
				!bReturnAttach
			)
		{
			//questo report figlio parte solo se si è in stampa
			delete pWoormInfo;
			pRDI->SetReturnValue((DataBool)true);
			return TRUE;
		}

		if ( 
				(m_bRunWithOutChild || (m_pWoormInfo ? m_pWoormInfo->m_bArchivePdfFormat : FALSE))
				&& 
				pWoormInfo->m_bArchivePdfFormat == 0
				&& 
				!bReturnValue 
				&& 
				!bReturnAttach
			)
		{
			//questo report figlio parte solo se si è in stampa
			delete pWoormInfo;
			pRDI->SetReturnValue((DataBool)true);
			return TRUE;
		}

		if (
				m_pWoormInfo 
				&& 
				m_pWoormInfo->m_bAutoPrint == 0
				&&
				pWoormInfo->m_bAutoPrint == 2
				&&
				!bReturnValue
				&& 
				!bReturnAttach
			)
		{
			if (pWoormInfo->m_bArchivePdfFormat)
			{
				pWoormInfo->m_bAutoPrint = FALSE;
			}
			else
			{
				//questo report figlio parte solo se si è in stampa
				delete pWoormInfo;
				pRDI->SetReturnValue((DataBool)true);
				return TRUE;
			}
		}
		if (pWoormInfo->m_bAutoPrint == 2)
			pWoormInfo->m_bAutoPrint = TRUE;

		if (
				m_pWoormInfo 
				&& 
				m_pWoormInfo->m_bArchivePdfFormat == 0
				&&
				pWoormInfo->m_bArchivePdfFormat == 2
				&& 
				!bReturnValue
				&& 
				!bReturnAttach
			)
		{
			if (pWoormInfo->m_bAutoPrint)
			{
				pWoormInfo->m_bArchivePdfFormat = FALSE;
			}
			else
			{
				//questo report figlio parte solo se si è in archiviazione
				delete pWoormInfo;
				pRDI->SetReturnValue((DataBool)true);
				return TRUE;
			}
		}
		if (pWoormInfo->m_bArchivePdfFormat == 2)
			pWoormInfo->m_bArchivePdfFormat = TRUE;

		BOOL bAllegaReportFigli = FALSE;
		if 
			(
				m_bAttachReportChild &&
				m_pWoormInfo && 
				!m_pWoormInfo->m_bHideFrame && 
				!pWoormInfo->m_bHideFrame && 
				(
					m_pWoormInfo->m_bSendEmail ||
					m_pWoormInfo->m_bAttachPDF ||
					m_pWoormInfo->m_bAttachRDE ||
					pWoormInfo->m_bSendEmail ||
					pWoormInfo->m_bAttachPDF ||
					pWoormInfo->m_bAttachRDE
				)
			)
		{
			CString strTemplateEmail = pWoormInfo->m_Email.m_sTemplateFileName;
			pWoormInfo->m_Email = m_pWoormInfo->m_Email;
			if (pWoormInfo->m_bSendEmail)
				pWoormInfo->m_Email.m_sTemplateFileName = strTemplateEmail;

			pWoormInfo->m_bAttachPDF = m_pWoormInfo->m_bAttachPDF;
			pWoormInfo->m_bAttachRDE = m_pWoormInfo->m_bAttachRDE;
			pWoormInfo->m_bAttachOther = m_pWoormInfo->m_bAttachOther;
			pWoormInfo->m_bCompressAttach = m_pWoormInfo->m_bCompressAttach;

			pWoormInfo->m_bCloseOnEndPrint = TRUE;
			pWoormInfo->m_bIconized = TRUE;
			nWaitEnd = 2; //se non aspetta la chiusura non può recuperare la "stampa"
			pWoormInfo->m_bOwnedByReport = FALSE;

			bAllegaReportFigli = TRUE;
		}
		if (bReturnAttach) 
			bAllegaReportFigli = TRUE;

		//passaggio dei parametri
		int i = 1;
		for (; i < pRDI->GetParameters().GetSize() - 1; i += 2)
		{
			CDataObjDescription* parName	= pRDI->GetParamDescription(i);
			CDataObjDescription* par		= pRDI->GetParamDescription(i + 1);

			//ASSERT (par->GetPassedMode() == CDataObjDescription::_INOUT);

			if (parName->GetDataType() != DataType::String || parName->GetValue() == NULL)
			{
				ASSERT(FALSE);
				TRACE(_T("The parameter %d of RunReport function must have string type and it must be equal to the field name that will be valorizated in the called report\n"), i);
				delete pWoormInfo;
				pRDI->SetReturnValue((DataBool)false);
				return TRUE;
			}
			CString strName = ((DataStr*)(parName->GetValue()))->GetString();
			CDataObjDescription* parDst = new CDataObjDescription(strName, par->GetValue()->DataObjClone(), TRUE);
			parDst->SetPassedMode(CDataObjDescription::_INOUT);

			pWoormInfo->AddParam(parDst);
		}

		//RUN sub-report
		TDisposablePtr<CWoormDocMng> pSubReport = (CWoormDocMng*)AfxGetTbCmdManager()->RunWoormReport(pWoormInfo, m_pCallerDocument);
		bOk = pSubReport;
		if (!bOk)
		{
			delete pWoormInfo;
			pRDI->SetReturnValue((DataBool)false);
			return TRUE;
		}

		if (bOk && nWaitEnd != 0)
		{
			if (m_pWoormInfo)
			{
				//In questo contesto m_pWoormInfo->m_bPrinted è sempre == a false
				//m_pWoormInfo->m_bPrinted = m_pWoormInfo->m_bPrinted ? pWoormInfo->m_bPrinted : FALSE;
				m_pWoormInfo->m_bPrintAborted = m_pWoormInfo->m_bPrintAborted || pWoormInfo->m_bPrintAborted;
				m_pWoormInfo->m_bErrorOnSendEmail = m_pWoormInfo->m_bErrorOnSendEmail || pWoormInfo->m_bErrorOnSendEmail;
				if (m_pOldWoormInfo)
				{
					//m_pOldWoormInfo->m_bPrinted = /*m_pOldWoormInfo->m_bPrinted &&*/ pWoormInfo->m_bPrinted;
					m_pOldWoormInfo->m_bPrintAborted = m_pOldWoormInfo->m_bPrintAborted || pWoormInfo->m_bPrintAborted;
					m_pOldWoormInfo->m_bErrorOnSendEmail = m_pOldWoormInfo->m_bErrorOnSendEmail || pWoormInfo->m_bErrorOnSendEmail;
				}
			}

			//TODO occorre trasformare m_pWaitingSubReport in un ARRAY
			//ASSERT(!m_pWaitingSubReport);
			m_pWaitingSubReport = pSubReport;

			if (nWaitEnd == 2)
			{
				AfxGetTbCmdManager()->WaitReportEnd (pSubReport);
			}
			else if (nWaitEnd == 1)
			{
				//altrimenti il report potrebbe recuperare tramite i parametri
				//dei valori di ritorno che non sono ancora completamente calcolati
				AfxGetTbCmdManager()->WaitReportRunning (pSubReport);
			}
			
			//----
			for (i = 0; i < pWoormInfo->GetParameters().GetSize(); i++)
			{
				DataObj* p = pRDI->GetParamValue(i * 2 + 2);
				DataObj* pRetVal = pWoormInfo->GetParamValue(i);
				CDataObjDescription* pRetValDescr = pWoormInfo->GetParamDescription(i);
				ASSERT (pRetValDescr->GetPassedMode() == CDataObjDescription::_INOUT);

				if (p && pRetVal && DataType::IsCompatible(pRetVal->GetDataType(), p->GetDataType()))
				{
					p->Assign(*pRetVal);
				}
			}

			if (bAllegaReportFigli)
			{
				ASSERT(pWoormInfo->m_Email.m_Attachments.GetSize());
				m_Email.m_Attachments.Append(pWoormInfo->m_Email.m_Attachments);
				m_Email.m_AttachmentTitles.Append(pWoormInfo->m_Email.m_AttachmentTitles);
			}

			if (nWaitEnd == 2)
				delete pWoormInfo;
			else if (!m_pWaitingSubReport)
				delete pWoormInfo;	//possibile Memory Leak, ma evita crash: TODO
		}

		pRDI->SetReturnValue(DataBool(bOk));
		return TRUE;
	}	
	else if (strF == IDF_WOORM_RUNDOCUMENT)
	{    
		if (pRDI->GetParameters().GetSize() < 1)
		{
			pRDI->SetReturnValue((DataLng)0);
			return TRUE;
 		}

		CString sNs = ((DataStr*)(pRDI->GetParamValue(0)))->GetString();
		CString viewMode = szDefaultViewMode;
		if (pRDI->GetParameters().GetSize() > 1)
			viewMode = ((DataStr*)(pRDI->GetParamValue(1)))->GetString();

		CBaseDocument* pDocHandle = AfxGetTbCmdManager()->RunDocument(sNs, viewMode);
		pRDI->SetReturnValue((DataLng)(long)pDocHandle);
		return TRUE;
	}
	else if (strF == IDF_WOORM_BROWSEDOCUMENT)
	{    
		if (pRDI->GetParameters().GetSize() < 3)
		{
			pRDI->SetReturnValue((DataBool)FALSE);
			return TRUE;
 		}

		CBaseDocument* pDoc = (CBaseDocument*)(long)*((DataLng*)(pRDI->GetParamValue(0)));
		if (!pDoc || !pDoc->IsKindOf(RUNTIME_CLASS(CBaseDocument)))
		{
			pRDI->SetReturnValue((DataBool)FALSE);
			return TRUE;
 		}

		//passaggio dei parametri
		CFunctionDescription FD;
		int i = 1;
		for (; i < pRDI->GetParameters().GetSize() - 1; i += 2)
		{
			CDataObjDescription* parName	= pRDI->GetParamDescription(i);
			CDataObjDescription* par		= pRDI->GetParamDescription(i + 1);

			if (parName->GetDataType() != DataType::String || parName->GetValue() == NULL)
			{
				ASSERT(FALSE);
				TRACE(_T("The parameter %d of BrowseDocument function must have string type and it must be equal to the field name that will be valorizated in the called report\n"), i);
				pRDI->SetReturnValue((DataBool)false);
				return TRUE;
			}

			CString strName = ((DataStr*)(parName->GetValue()))->GetString();
			CDataObjDescription* parDst = new CDataObjDescription(strName, par->GetValue()->DataObjClone(), TRUE);
			//parDst->SetPassedMode(CDataObjDescription::_INOUT);

			FD.AddParam(parDst);
		}

		BOOL bOk = pDoc->GoInBrowserMode(&FD);

		pRDI->SetReturnValue((DataBool)bOk);
		return TRUE;
	}
	else if (strF == IDF_WOORM_CLOSEDOCUMENT)
	{
		CBaseDocument* pDoc = (CBaseDocument*)(long)*((DataLng*)(pRDI->GetParamValue(0)));
		if (!pDoc || !pDoc->IsKindOf(RUNTIME_CLASS(CBaseDocument)))
		{
			pRDI->SetReturnValue((DataBool)FALSE);
			return TRUE;
		}

		AfxGetTbCmdManager()->CloseDocument((const CBaseDocument*)pDoc);
		pRDI->SetReturnValue((DataBool)TRUE);
		return TRUE;
	}
	else if (strF == IDF_WOORM_RUNPROGRAM)
	{
		if (pRDI->GetParameters().GetSize() < 2)
		{
			pRDI->SetReturnValue((DataBool)FALSE);
			return TRUE;
		}
		CString strProg		= ((DataStr*)(pRDI->GetParamValue(0)))->GetString();
		CString strCmdLine	= ((DataStr*)(pRDI->GetParamValue(1)))->GetString();

		CString sExt = strProg.Right(4); 
		if (sExt.CompareNoCase(_T(".exe")))
		{
			HINSTANCE hInst = ::TBShellExecute(strProg, strCmdLine);
			//if (hInst <= (HINSTANCE)32)
			//	AfxMessageBox (ShellExecuteErrMsg((int)hInst));;
			pRDI->SetReturnValue(DataBool(hInst == 0));
			return TRUE;
		}
		
		BOOL bWaitEnd = FALSE;
		int nSep = strCmdLine.ReverseFind('|');
		if (nSep > -1)
		{
			CString strFlags = strCmdLine.Right(strCmdLine.GetLength() - nSep - 1);
			strCmdLine = strCmdLine.Left(nSep);

			strFlags.MakeLower();
			bWaitEnd = strFlags.Find(_T("waitend")) >= 0;
		}
	
		bOk = SpawnProgramExecution(strProg, strCmdLine, bWaitEnd);

		pRDI->SetReturnValue(DataBool(bOk));
		return TRUE;
	}
	else if (strF == IDF_WOORM_EXPANDTEMPLATE)
	{
		if (pRDI->GetParameters().GetSize() < 3)
		{
			pRDI->SetReturnValue((DataBool)FALSE);
			return TRUE;
		}
		CString strTemplate		= ((DataStr*)(pRDI->GetParamValue(0)))->GetString();
		CString strTarget	= ((DataStr*)(pRDI->GetParamValue(1)))->GetString();

		//passaggio dei parametri
		CFunctionDescription FD;
		int i = 2;
		for (; i < pRDI->GetParameters().GetSize() - 1; i += 2)
		{
			CDataObjDescription* parName	= pRDI->GetParamDescription(i);
			CDataObjDescription* par		= pRDI->GetParamDescription(i + 1);

			if (parName->GetDataType() != DataType::String || parName->GetValue() == NULL)
			{
				ASSERT(FALSE);
				TRACE(_T("The parameter %d of BrowseDocument function must have string type and it must be equal to the field name that will be valorizated in the called report\n"), i);
				pRDI->SetReturnValue(DataBool());
				return TRUE;
			}
			CString strName = ((DataStr*)(parName->GetValue()))->GetString();
			CDataObjDescription* parDst = new CDataObjDescription(strName, par->GetValue()->DataObjClone(), TRUE);
			parDst->SetPassedMode(CDataObjDescription::_INOUT);

			FD.AddParam(parDst);
		}
		//----

		CExpandTemplateMng ExpandTpl(&FD);

		if (!ExpandTpl.SetTemplate (strTemplate))
		{
			pRDI->SetReturnValue(DataBool());
			return TRUE;
		}
		if (!ExpandTpl.SetTarget (strTarget))
		{
			pRDI->SetReturnValue(DataBool());
			return TRUE;
		}

		BOOL bOk = ExpandTpl.Expand();

		pRDI->SetReturnValue(DataBool(bOk));
		return TRUE;
	}
	else if (strF == IDF_WOORM_GENERATE_DMS_PAPERY)
	{
		if (!AfxGetOleDbMng()->EasyAttachmentEnable())
		{
			pRDI->SetReturnValue((DataBool)FALSE);
			return FALSE;
		}
		pRDI->SetReturnValue((DataBool)GeneratePapery());
		return TRUE;
	}
	return FALSE;
}

//-------------------------------------------------------------
BOOL CWoormDocMng::DispatchFunctionCallSync(CFunctionDescription* pRDI, const CString& strF)
{
	BOOL bOk = TRUE;
	CString strName;
	BOOL nWaitEnd = 0;
	BOOL bReturnValue = FALSE;

	// MailConnector --------------------------------------------------
	if(strF ==  IDF_WOORM_MAILSEND)
	{
		m_bDirectAttachRDE = (BOOL)(*((DataBool*)(pRDI->GetParamValue(0))));
		m_bDirectAttachPDF = (BOOL)(*((DataBool*)(pRDI->GetParamValue(1))));
		m_bDirectCompressAttach = (BOOL)(*((DataBool*)(pRDI->GetParamValue(2))));

		m_bDirectSendMail = TRUE;

		pRDI->SetReturnValue((DataBool)true);
		return TRUE;
	}

	else if(strF ==  IDF_WOORM_MAILSEND_EX)
	{
		//m_pWoormInfo->m_bPDFOutput = m_pWoormInfo->m_bRDEOutput = FALSE;
		//m_pWoormInfo->m_arExportInfo.RemoveAll();

		CString sExportType = ((DataStr*)(pRDI->GetParamValue(0)))->GetString();
		int nIdx = sExportType.Find('|');
		if (nIdx > 0)
		{
			CString sFlags = sExportType.Mid(nIdx + 1);
			sExportType.Truncate(nIdx);

			if (!ParseFlags(m_pWoormInfo, sFlags))
			{
				if (!IsInUnattendedMode() && (m_pWoormInfo ? !m_pWoormInfo->m_bHideFrame : TRUE))
					AfxMessageBox(_TB("mailSendEx was called with wrong arguments") + L" - " + sFlags);
			}
		}

		if (sExportType.IsEmpty())
			sExportType = _T("PDF");

		if (sExportType.CompareNoCase(_T("PDF")) == 0) 
		{
			m_bDirectAttachPDF = TRUE;
		}
		else if (sExportType.CompareNoCase(_T("RDE")) == 0) 
		{
			m_bDirectAttachRDE = TRUE;
		}
		else if (!m_pWoormInfo)
		{
			if (!IsInUnattendedMode() && (m_pWoormInfo ? !m_pWoormInfo->m_bHideFrame : TRUE))
				AfxMessageBox(_TB("MailSendEx was called without woorminfo") );

			pRDI->SetReturnValue((DataBool)FALSE);
			return TRUE;
		}	
		else if (sExportType.CompareNoCase(_T("XLS")) == 0 || sExportType.CompareNoCase(_T("XLSX")) == 0)
		{
			CExportInfo* pEInfo = new CExportInfo();
			pEInfo->SetExportType(TypeOfExport::EXPORT_EXCELNET_TYPE);
			m_pWoormInfo->m_arExportInfo.Add(pEInfo);
		}
		else if (sExportType.CompareNoCase(_T("OXLSX")) == 0)
		{
			CExportInfo* pEInfo = new CExportInfo();
			pEInfo->SetExportType(TypeOfExport::EXPORT_OPENXML_EXCEL_TYPE);
			m_pWoormInfo->m_arExportInfo.Add(pEInfo);
		}
		else if (sExportType.CompareNoCase(_T("DOC")) == 0 || sExportType.CompareNoCase(_T("DOCX")) == 0)
		{
			CExportInfo* pEInfo = new CExportInfo();
			pEInfo->SetExportType(TypeOfExport::EXPORT_WORDNET_TYPE);
			m_pWoormInfo->m_arExportInfo.Add(pEInfo);
		}
		else if (sExportType.CompareNoCase(_T("ODS")) == 0)
		{
			CExportInfo* pEInfo = new CExportInfo();
			pEInfo->SetExportType(TypeOfExport::EXPORT_OPENOFFICE_ODS_TYPE);
			m_pWoormInfo->m_arExportInfo.Add(pEInfo);
		}
		else if (sExportType.CompareNoCase(_T("ODT")) == 0)
		{
			CExportInfo* pEInfo = new CExportInfo();
			pEInfo->SetExportType(TypeOfExport::EXPORT_OPENOFFICE_ODT_TYPE);
			m_pWoormInfo->m_arExportInfo.Add(pEInfo);
		}
		else if (sExportType.CompareNoCase(_T("CSV")) == 0 || sExportType.CompareNoCase(_T("TXT")) == 0) 
		{
			CExportInfo* pEInfo = new CExportInfo();
			pEInfo->SetExportType(TypeOfExport::EXPORT_CSV_TYPE);
			m_pWoormInfo->m_arExportInfo.Add(pEInfo);
		}
		else if (sExportType.CompareNoCase(_T("HTML")) == 0 || sExportType.CompareNoCase(_T("HTM")) == 0) 
		{
			CExportInfo* pEInfo = new CExportInfo();
			pEInfo->SetExportType(TypeOfExport::EXPORT_HTML_TYPE);
			m_pWoormInfo->m_arExportInfo.Add(pEInfo);
		}
		else if (sExportType.CompareNoCase(_T("XML")) == 0) 
		{
			CExportInfo* pEInfo = new CExportInfo();
			pEInfo->SetExportType(TypeOfExport::EXPORT_XML_TYPE);
			m_pWoormInfo->m_arExportInfo.Add(pEInfo);
		}
		else if (sExportType.CompareNoCase(_T("FXML")) == 0)
		{
			CExportInfo* pEInfo = new CExportInfo();
			pEInfo->SetExportType(TypeOfExport::EXPORT_XML_FULL_TYPE);
			m_pWoormInfo->m_arExportInfo.Add(pEInfo);
		}
		else //errore: TIPO NON RICONOSCIUTO
		{
			if (!IsInUnattendedMode() && (m_pWoormInfo ? !m_pWoormInfo->m_bHideFrame : TRUE))
				AfxMessageBox(_TB("MailSendEx was called with wrong arguments") );

			pRDI->SetReturnValue((DataBool)FALSE);
			return TRUE;
		}

		//m_pWoormInfo->m_arstrOutputFileNames.SetAtGrow(m_pWoormInfo->m_nNextReport - 1, strFileName);

		m_bDirectSendMail = TRUE;

		pRDI->SetReturnValue((DataBool)true);
		return TRUE;
	}

	else if(strF ==  IDF_WOORM_SETMAPIPROFILE)
	{
		m_Email.SetMapiProfile(((DataStr*)(pRDI->GetParamValue(0)))->GetString());
		pRDI->SetReturnValue((DataBool)true);
		return TRUE;
	}

	else if(strF ==  IDF_WOORM_SETFROM)
	{
		m_Email.SetFrom(((DataStr*)(pRDI->GetParamValue(0)))->GetString());

		pRDI->SetReturnValue((DataBool)true);
		return TRUE;
	}
	else if(strF ==  IDF_WOORM_SETSUBJECT)
	{
		m_Email.SetSubject(((DataStr*)(pRDI->GetParamValue(0)))->GetString(), (BOOL)(*(DataBool*)(pRDI->GetParamValue(1))));
		pRDI->SetReturnValue((DataBool)true);
		return TRUE;
	}
	else if(strF ==  IDF_WOORM_SETTEMPLATEFILENAME)
	{
		m_Email.SetTemplateFileName(((DataStr*)(pRDI->GetParamValue(0)))->GetString());
		pRDI->SetReturnValue((DataBool)true);
		return TRUE;
	}
	else if(strF ==  IDF_WOORM_SETATTACHMENTREPORTNAME)
	{
		CString str = ((DataStr*)(pRDI->GetParamValue(0)))->GetString();
		str.Trim();
		if (str.IsEmpty())
			pRDI->SetReturnValue((DataBool)FALSE);
		else
		{
			m_Email.SetAttachmentReportName(str);
			pRDI->SetReturnValue((DataBool)TRUE);
		}
		return TRUE;
	}
	else if(strF == IDF_WOORM_SETBODY)
	{
		//m_Email.SetHtml(_T(""), FALSE,FALSE); no troppo invasiva
		m_Email.m_sHtml.Empty();
		m_Email.SetBody(((DataStr*)(pRDI->GetParamValue(0)))->GetString(), (BOOL)(*(DataBool*)(pRDI->GetParamValue(1))));
		
		pRDI->SetReturnValue((DataBool)true);
		return TRUE;
	}
	else if(strF ==  IDF_WOORM_SETCERTIFIEDFILTER)
	{
		DataEnum de = *((DataEnum*)(pRDI->GetParamValue(0)));
		m_Email.m_deEmailAddressType = de;
		pRDI->SetReturnValue((DataBool)true);
		return TRUE;
	}
	else if(strF ==  IDF_WOORM_SETTOCERTIFIED)
	{
		CString sTo = ((DataStr*)(pRDI->GetParamValue(0)))->GetString();
		m_Email.SetTo(sTo, (BOOL)(*(DataBool*)(pRDI->GetParamValue(1))), CMapiMessage::TAG_CERTIFIED);
		pRDI->SetReturnValue((DataBool)true);
		return TRUE;
	}

	else if(strF ==  IDF_WOORM_SETTO)
	{
		CString sTo = ((DataStr*)(pRDI->GetParamValue(0)))->GetString();
		//CString sTag;
		//if (pRDI->GetParameters().GetCount() == 3)
		//{
		//	sTag = ((DataStr*)(pRDI->GetParamValue(2)))->GetString();
		//}
		m_Email.SetTo(sTo, (BOOL)(*(DataBool*)(pRDI->GetParamValue(1)))/*, sTag*/);

		pRDI->SetReturnValue((DataBool)true);
		return TRUE;
	}
	else if(strF ==  IDF_WOORM_SETCC)
	{
		m_Email.SetCc(((DataStr*)(pRDI->GetParamValue(0)))->GetString(), (BOOL)(*(DataBool*)(pRDI->GetParamValue(1))));
		pRDI->SetReturnValue((DataBool)true);
		return TRUE;
	}
	else if(strF ==  IDF_WOORM_SETBCC)
	{
		m_Email.SetBcc(((DataStr*)(pRDI->GetParamValue(0)))->GetString(), (BOOL)(*(DataBool*)(pRDI->GetParamValue(1))));
		pRDI->SetReturnValue((DataBool)true);
		return TRUE;
	}

	else if(strF ==  IDF_WOORM_SETATTACHMENT)
	{
		BOOL bOk = AddEmailAttachments
						(
							((DataStr*)(pRDI->GetParamValue(0)))->GetString(),
							((DataStr*)(pRDI->GetParamValue(1)))->GetString(),
							(BOOL)(*(DataBool*)(pRDI->GetParamValue(2)))
						);
		
		pRDI->SetReturnValue((DataBool)bOk);
		return TRUE;
	}

	else if(strF ==  IDF_WOORM_SETBODYPARAMETER)
	{
		m_Email.SetBodyParameter
			(
				((DataStr*)(pRDI->GetParamValue(0)))->GetString(),
				pRDI->GetParamValue(1)->FormatData()
			);
		pRDI->SetReturnValue((DataBool)true);
		return TRUE;
	}

	// PostaLite --------------------------------------------------
	else if (strF == IDF_WOORM_POSTALITE_SEND)
	{
		DataEnum deDeliveryType	= (*((DataEnum*)(pRDI->GetParamValue(0))));
		DataEnum dePrintType	= (*((DataEnum*)(pRDI->GetParamValue(1))));

		if (m_Email.m_PostaLiteMsg.GetCount() == 0)
			m_Email.m_PostaLiteMsg.Add(L"", L"");
		CPostaLiteAddress* pAddr = (CPostaLiteAddress*) m_Email.m_PostaLiteMsg.GetAt(m_Email.m_PostaLiteMsg.GetUpperBound());
		
		pAddr->m_deDeliveryType = deDeliveryType;
		pAddr->m_dePrintType	= dePrintType;

		m_Email.m_PostaLiteMsg.m_bUsePostaLite = TRUE;

		m_bDirectSendMail = TRUE;

		pRDI->SetReturnValue((DataBool)true);
		return TRUE;
	}
	else if(strF == IDF_WOORM_POSTALITE_SETADDRESSEE)
	{
		CPostaLiteAddress* pAddr = NULL;
		BOOL bAppend = (BOOL) (*((DataBool*)(pRDI->GetParamValue(8))));
		if (bAppend || m_Email.m_PostaLiteMsg.GetCount() == 0)
		{
			m_Email.m_PostaLiteMsg.Add(L"", L"");
		}
		else if (m_Email.m_PostaLiteMsg.GetCount() > 1)
		{
			m_Email.m_PostaLiteMsg.SetSize(1);
		}
		pAddr = (CPostaLiteAddress*) m_Email.m_PostaLiteMsg.GetAt(m_Email.m_PostaLiteMsg.GetUpperBound());
		
		pAddr->m_sAddressee = (*((DataStr*)(pRDI->GetParamValue(0))));
		pAddr->m_sAddress	= (*((DataStr*)(pRDI->GetParamValue(1))));
		pAddr->m_sCity		= (*((DataStr*)(pRDI->GetParamValue(2))));
		pAddr->m_sCounty	= (*((DataStr*)(pRDI->GetParamValue(3))));
		pAddr->m_sCountry	= (*((DataStr*)(pRDI->GetParamValue(4))));
		pAddr->m_sZipCode	= (*((DataStr*)(pRDI->GetParamValue(5))));
		pAddr->m_sFax		= (*((DataStr*)(pRDI->GetParamValue(6))));
		pAddr->m_sISOCode	= (*((DataStr*)(pRDI->GetParamValue(7))));

		pRDI->SetReturnValue((DataBool)true);
		return TRUE;
	}
	else if (strF == IDF_WOORM_POSTALITE_SETSENDTYPE)
	{
		DataEnum deDeliveryType	= (*((DataEnum*)(pRDI->GetParamValue(0))));
		DataEnum dePrintType	= (*((DataEnum*)(pRDI->GetParamValue(1))));

		if (m_Email.m_PostaLiteMsg.GetCount() == 0)
			m_Email.m_PostaLiteMsg.Add(L"", L"");
		CPostaLiteAddress* pAddr = (CPostaLiteAddress*) m_Email.m_PostaLiteMsg.GetAt(m_Email.m_PostaLiteMsg.GetUpperBound());
		
		pAddr->m_deDeliveryType = deDeliveryType;
		pAddr->m_dePrintType	= dePrintType;

		pRDI->SetReturnValue((DataBool)true);
		return TRUE;
	}

	// --------------------------------------
	else if(strF == IDF_WOORM_SEND_MAIL)
	{
		CMapiMessage msg;

		msg.m_sFrom			= (*((DataStr*)(pRDI->GetParamValue(0))));
		msg.SetTo			(((DataStr*)(pRDI->GetParamValue(1)))->GetString());
		msg.SetCc			(((DataStr*)(pRDI->GetParamValue(2)))->GetString());
		msg.SetBcc			(((DataStr*)(pRDI->GetParamValue(3)))->GetString());
		msg.m_sSubject		= (*((DataStr*)(pRDI->GetParamValue(4))));
		msg.m_sBody			= (*((DataStr*)(pRDI->GetParamValue(5))));
		msg.SetAttachment	(((DataStr*)(pRDI->GetParamValue(6)))->GetString());

		BOOL bOk = AfxGetIMailConnector()->SendMail (msg);

		pRDI->SetReturnValue((DataBool)bOk);
		return TRUE;
	}

	else if (strF == IDF_WOORM_ARCHIVE_REPORT)
	{
		if (!m_pWoormInfo || !AfxGetOleDbMng()->EasyAttachmentEnable())
		{
			pRDI->SetReturnValue((DataBool)FALSE);
			return FALSE;
		}		
		m_pWoormInfo->m_bConcatPDF = TRUE;
		m_pWoormInfo->m_bArchivePdfFormat = TRUE;
		m_bDirectConcatPDF = TRUE;	
		pRDI->SetReturnValue((DataBool)TRUE);
		return TRUE;
	}
	else if (strF == IDF_WOORM_REPORT_SAVE_AS || strF == IDF_WOORM_REPORT_SAVE_AS_AND_ATTACH)
	{
		if (!m_pWoormInfo)
		{
			CString sMsg = cwsprintf(_TB("The report must be saved before use this function"), (LPCTSTR)strF);
			TRACE(sMsg);

			if (!IsInUnattendedMode() && (m_pWoormInfo ? !m_pWoormInfo->m_bHideFrame : TRUE))
				AfxMessageBox(sMsg);

			pRDI->SetReturnValue((DataBool)FALSE);
			return TRUE;
		}

		CString strFileName = ((DataStr*)(pRDI->GetParamValue(0)))->GetString();
		strFileName.Trim();
		if (strFileName.IsEmpty())
		{
			pRDI->SetReturnValue((DataBool)FALSE);
			return TRUE;
		}
		//An. 19472  
		if (!::IsValidObjName(strFileName))
		{
			CString sMsg = cwsprintf(_TB("ReportSaveAs has an invalid file name: {0-%s}. File name cannot contain any of the following characters:\r\n /?*\\<>|\" \".\r\n"), (LPCTSTR)strFileName);
			TRACE(sMsg);

			if (!IsInUnattendedMode() && (m_pWoormInfo ? !m_pWoormInfo->m_bHideFrame : TRUE))
				AfxMessageBox(sMsg);

			pRDI->SetReturnValue((DataBool)FALSE);
			return TRUE;
		}

		if (m_pWoormInfo)
		{
			m_pWoormInfo->m_bPDFOutput = m_pWoormInfo->m_bRDEOutput = FALSE;
			m_pWoormInfo->m_arExportInfo.RemoveAll();
		}

		CString sExportType = ((DataStr*)(pRDI->GetParamValue(1)))->GetString();
		int nIdx = sExportType.Find('|');
		if (nIdx > 0)
		{
			CString sFlags = sExportType.Mid(nIdx + 1);
			sExportType.Truncate(nIdx);

			if (!ParseFlags(m_pWoormInfo, sFlags))
			{
				CString sMsg = _TB("ReportSaveAs was called with wrong arguments") + L" - " + sFlags;
				TRACE(sMsg);

				if (!IsInUnattendedMode() && (m_pWoormInfo ? !m_pWoormInfo->m_bHideFrame : TRUE))
					AfxMessageBox(sMsg);
			}
		}

		CString sExt = strFileName.Right(4).MakeLower();
		if (sExportType.IsEmpty())
			sExportType = sExt.Mid(1);

		if (sExportType.CompareNoCase(_T("PDF")) == 0) 
		{
			m_pWoormInfo->m_bPDFOutput = TRUE;

			if (sExt.CompareNoCase(_T(".pdf")))
				strFileName += _T(".pdf");
		}
		else if (sExportType.CompareNoCase(_T("RDE")) == 0) 
		{
			m_pWoormInfo->m_bRDEOutput = TRUE;

			if (sExt.CompareNoCase(_T(".rde")))
				strFileName += _T(".rde");
		}
		else if (sExportType.CompareNoCase(_T("XLS")) == 0 || sExportType.CompareNoCase(_T("XLSX")) == 0) 
		{
			CExportInfo* pEInfo = new CExportInfo();
			pEInfo->SetExportType(TypeOfExport::EXPORT_EXCELNET_TYPE);
			m_pWoormInfo->m_arExportInfo.Add(pEInfo);
			
			if (sExt.CompareNoCase(_T(".xls")) && sExt.CompareNoCase(_T(".xlsx")))
				strFileName += _T(".xlsx");
		}
		else if (sExportType.CompareNoCase(_T("OXLSX")) == 0)
		{
			CExportInfo* pEInfo = new CExportInfo();
			pEInfo->SetExportType(TypeOfExport::EXPORT_OPENXML_EXCEL_TYPE);
			m_pWoormInfo->m_arExportInfo.Add(pEInfo);

			if (sExt.CompareNoCase(_T(".xlsx")))
				strFileName += _T(".xlsx");
		}
		else if (sExportType.CompareNoCase(_T("DOC")) == 0 || sExportType.CompareNoCase(_T("DOCX")) == 0)
		{
			CExportInfo* pEInfo = new CExportInfo();
			pEInfo->SetExportType(TypeOfExport::EXPORT_WORDNET_TYPE);
			m_pWoormInfo->m_arExportInfo.Add(pEInfo);

			if (sExt.CompareNoCase(_T(".doc")) && sExt.CompareNoCase(_T(".docx")))
				strFileName += _T(".docx");
		}
		else if (sExportType.CompareNoCase(_T("ODS")) == 0) 
		{
			CExportInfo* pEInfo = new CExportInfo();
			pEInfo->SetExportType(TypeOfExport::EXPORT_OPENOFFICE_ODS_TYPE);
			m_pWoormInfo->m_arExportInfo.Add(pEInfo);
			
			if (sExt.CompareNoCase(_T(".ods")))
				strFileName += _T(".ods");
		}
		else if (sExportType.CompareNoCase(_T("ODT")) == 0)
		{
			CExportInfo* pEInfo = new CExportInfo();
			pEInfo->SetExportType(TypeOfExport::EXPORT_OPENOFFICE_ODT_TYPE);
			m_pWoormInfo->m_arExportInfo.Add(pEInfo);

			if (sExt.CompareNoCase(_T(".odt")))
				strFileName += _T(".odt");
		}
		else if (sExportType.CompareNoCase(_T("CSV")) == 0 || sExportType.CompareNoCase(_T("TXT")) == 0)
		{
			CExportInfo* pEInfo = new CExportInfo();
			pEInfo->SetExportType(TypeOfExport::EXPORT_CSV_TYPE);
			m_pWoormInfo->m_arExportInfo.Add(pEInfo);

			if (sExt.CompareNoCase(_T(".csv")) && sExt.CompareNoCase(_T(".txt")))
				strFileName += _T(".csv");
		}
		else if (sExportType.CompareNoCase(_T("HTML")) == 0 || sExportType.CompareNoCase(_T("HTM")) == 0) 
		{
			CExportInfo* pEInfo = new CExportInfo();
			pEInfo->SetExportType(TypeOfExport::EXPORT_HTML_TYPE);
			m_pWoormInfo->m_arExportInfo.Add(pEInfo);

			if (sExt.CompareNoCase(_T(".htm")) && sExt.CompareNoCase(_T(".html")))
				strFileName += _T(".html");
		}
		else if (sExportType.CompareNoCase(_T("XML")) == 0) 
		{
			CExportInfo* pEInfo = new CExportInfo();
			pEInfo->SetExportType(TypeOfExport::EXPORT_XML_TYPE);
			m_pWoormInfo->m_arExportInfo.Add(pEInfo);

			if (sExt.CompareNoCase(_T(".xml")))
				strFileName += _T(".xml");
		}
		else if (sExportType.CompareNoCase(_T("FXML")) == 0)
		{
			CExportInfo* pEInfo = new CExportInfo();
			pEInfo->SetExportType(TypeOfExport::EXPORT_XML_FULL_TYPE);
			m_pWoormInfo->m_arExportInfo.Add(pEInfo);

			if (sExt.CompareNoCase(_T(".xml")))
				strFileName += _T(".xml");
		}
		else //errore: TIPO NON RICONOSCIUTO
		{
			CString sMsg = _TB("ReportSaveAs was called with wrong arguments");
			TRACE(sMsg);

			if (!IsInUnattendedMode() && (m_pWoormInfo ? !m_pWoormInfo->m_bHideFrame : TRUE))
				AfxMessageBox(sMsg);

			pRDI->SetReturnValue((DataBool)FALSE);
			return TRUE;
		}

		if (strF == IDF_WOORM_REPORT_SAVE_AS_AND_ATTACH)
		{
			if (!m_pWoormInfo || pRDI->GetParameters().GetCount() < 3)
			{
				pRDI->SetReturnValue((DataBool)FALSE);
				return TRUE;
			}

			DataLng* pObj = dynamic_cast<DataLng*>(pRDI->GetParamValue(2));
			if (!pObj)
			{
				pRDI->SetReturnValue((DataBool)FALSE);
				return TRUE;
			}
			CBaseDocument* pDoc = dynamic_cast<CBaseDocument*>((CObject*)((long)*(pObj)));
			if (!pDoc)
			{
				pRDI->SetReturnValue((DataBool)FALSE);
				return TRUE;
			}
			ASSERT_VALID(pDoc);

			#pragma warning(disable: 4996) 
			if (!AfxGetTbCmdManager()->ExistDocument(pDoc))
			{
				pRDI->SetReturnValue((DataBool)FALSE);
				return TRUE;
			}
			#pragma warning(default: 4996)

			ASSERT_VALID(m_pWoormInfo);
			m_pWoormInfo->m_pDocumentAttachTo = pDoc;
			m_pWoormInfo->m_bConcatPDF = TRUE;
			m_pWoormInfo->m_bArchivePdfFormat = TRUE;
			m_pWoormInfo->m_bUseOutputFileName = TRUE;

			m_bDirectConcatPDF = TRUE;	
		}

		m_pWoormInfo->m_arstrOutputFileNames.SetAtGrow(m_pWoormInfo->m_nNextReport - 1, strFileName);

		pRDI->SetReturnValue((DataBool)TRUE);
		return TRUE;	
	}
	//-----------------------------------------
	else if(strF ==  IDF_WOORM_GETREPORTPATH)
	{
		pRDI->SetReturnValue((DataStr)GetPath(m_strPathName, TRUE));
		return TRUE;
	}
	else if(strF ==  IDF_WOORM_GETREPORTNAMESPACE)
	{
		pRDI->SetReturnValue((DataStr)GetReportNamespace());
		return TRUE;
	}
	else if(strF ==  IDF_WOORM_GETREPORTNAME)
	{
		CString sName = GetInfoOSL()->m_Namespace.GetObjectName();
		BOOL bStripExt = (BOOL)(*(DataBool*)(pRDI->GetParamValue(0)));
		if (bStripExt)
		{
			int idx = sName.ReverseFind('.');
			if (idx > 0)
			{
				sName = sName.Left(idx);
			}
		}
		pRDI->SetReturnValue((DataStr)sName);
		return TRUE;
	}
	else if(strF ==  IDF_WOORM_GETREPORTMODULENAMESPACE)
	{
		CTBNamespace ns
			(
				CTBNamespace::MODULE,
				GetInfoOSL()->m_Namespace.GetApplicationName() + _T(".") + GetInfoOSL()->m_Namespace.GetModuleName()
			);

		pRDI->SetReturnValue((DataStr)(ns.ToUnparsedString()));
		return TRUE;
	}
	else if(strF ==  IDF_WOORM_GETOWNERNAMESPACE)
	{
		CString sNs = m_pCallerDocument ? m_pCallerDocument->GetNamespace().ToUnparsedString() : _T("");
		pRDI->SetReturnValue((DataStr)sNs);
		return TRUE;
	}
	else if(strF ==  IDF_WOORM_GETPRINTERNAME)
	{
		CString sName;
		if (m_pWoormInfo)
		{
			sName = m_pWoormInfo->m_strPrinterName;
			if (sName.IsEmpty() && m_pWoormInfo->m_hDevNames)
			{
				LPDEVNAMES lpDevNames = (LPDEVNAMES)::GlobalLock(m_pWoormInfo->m_hDevNames);
				ASSERT(lpDevNames != NULL);
				if (lpDevNames)
					sName = ((LPCTSTR)lpDevNames + lpDevNames->wDeviceOffset);
				::GlobalUnlock(m_pWoormInfo->m_hDevNames);
			}
		}
		pRDI->SetReturnValue((DataStr)sName);
		return TRUE;
	}
	else if(strF ==  IDF_WOORM_SETPRINTERNAME)
	{
		pRDI->SetReturnValue((DataStr)(m_pWoormInfo ? m_pWoormInfo->m_strPrinterName : _T("")));
		if (m_pWoormInfo)
			m_pWoormInfo->m_strPrinterName = *(DataStr*)(pRDI->GetParamValue(0));
		return TRUE;
	}
	else if(strF ==  IDF_WOORM_ISAUTOPRINT)
	{
		pRDI->SetReturnValue((DataBool)(m_pWoormInfo && m_pWoormInfo->m_bAutoPrint));
		return TRUE;
	}
	else if(strF ==  IDF_WOORM_UpdateOutputParametersEvenIfReportDoesNotFetchRecords)
	{
		m_bUpdateOutputParametersEvenIfReportDoesNotFetchRecords = TRUE;
		
		pRDI->SetReturnValue((DataBool)(TRUE));
		return TRUE;
	}
	else if(strF ==  IDF_WOORM_SKIPCONTEXT)
	{
		m_bSkipContext = TRUE;
		
		pRDI->SetReturnValue((DataBool)(TRUE));
		return TRUE;
	}
	//-------------------------------------------------------------------------

	else if (strF == IDF_WOORM_GetTableRows)
	{
		if (pRDI->GetParameters().GetSize() != 1)
		{
			pRDI->SetReturnValue((DataInt)0);
			return TRUE;
		}
		strName = ((DataStr*)(pRDI->GetParamValue(0)))->GetString();

		if (m_pEngine && m_pEngine->m_bRunning && m_pEngine->m_pSymTable && m_pEngine->m_pSymTable->GetDisplayTables())
		{
			int idx = m_pEngine->m_pSymTable->GetDisplayTables()->Find(strName, DisplayTables::CURRENT_LAYOUT);
			if (idx < 0)
			{
				pRDI->SetReturnValue((DataInt)0);
				return TRUE;
			}
			short nRows = m_pEngine->m_pSymTable->GetDisplayTables()->GetRowsFromIdx(idx);

			pRDI->SetReturnValue(DataInt(nRows));
			return TRUE;
		}
		else if (m_pEditorManager)
		{
			DisplayTables* pTbls = m_pEditorManager->GetDispTable();
			if (pTbls)
			{
				int idx = pTbls->Find(strName, DisplayTables::CURRENT_LAYOUT);
				if (idx >= 0)
				{
					pRDI->SetReturnValue(DataInt(pTbls->GetRowsFromIdx(idx)));
					return TRUE;
				}
			}
		}

		pRDI->SetReturnValue(DataInt(0));
		return TRUE;
	}
	else if (strF == IDF_WOORM_GetTableCurrentRow)
	{
		if (pRDI->GetParameters().GetSize() != 1)
		{
			pRDI->SetReturnValue((DataInt)0);
			return TRUE;
		}
		strName = ((DataStr*)(pRDI->GetParamValue(0)))->GetString();

		if (m_pEngine && m_pEngine->m_bRunning && m_pEngine->m_pSymTable && m_pEngine->m_pSymTable->GetDisplayTables())
		{
			ASSERT_VALID(m_pEngine);		
			int idx = m_pEngine->m_pSymTable->GetDisplayTables()->Find(strName, DisplayTables::CURRENT_LAYOUT);
			if (idx < 0)
			{
				pRDI->SetReturnValue((DataInt)0);
				return TRUE;
			}
			DisplayTableEntryEngine* pDispTable = (DisplayTableEntryEngine*) m_pEngine->m_pSymTable->GetDisplayTables()->GetAt(idx);
			ASSERT_VALID(pDispTable);		
			short nRow = pDispTable->GetCurrentRow();

			pRDI->SetReturnValue(DataInt(nRow));
			return TRUE;
		}
		else if (m_pEditorManager)
		{
			ASSERT_VALID(m_pEditorManager);
			DisplayTables* pTbls = m_pEditorManager->GetDispTable();
			if (pTbls)
			{
				ASSERT_VALID(pTbls);
				int idx = pTbls->Find(strName, DisplayTables::CURRENT_LAYOUT);
				if (idx >= 0)
				{
					DisplayTableEntry* pTE = pTbls->GetAt(idx);
					ASSERT_VALID(pTE);
					WORD wID = pTE->GetId();
					BaseObj* pO = this->GetObjects().FindByID(wID);
					ASSERT_VALID(pO);
					if (pO)
					{
						if (pO->IsKindOf(RUNTIME_CLASS(Table)))
						{
							Table* pT = (Table*) pO;
							pRDI->SetReturnValue(DataInt(pT->m_nViewCurrentRow));
							return TRUE;
						}
						else if (pO->IsKindOf(RUNTIME_CLASS(Repeater)))
						{
							Repeater* pR = (Repeater*) pO;
							pRDI->SetReturnValue(DataInt(pR->m_nViewCurrentRow));
							return TRUE;
						}
						else
						{
							ASSERT(FALSE);
							pRDI->SetReturnValue(DataInt(0));
							return TRUE;
						}
					}
				}
			}
		}

		pRDI->SetReturnValue(DataInt(0));
		return TRUE;
	}

	else if (strF == IDF_WOORM_CurrentRowContainsCellTail || 
			strF == IDF_WOORM_CurrentRowContainsCellSubTotal || 
			strF == IDF_WOORM_CurrentRowIsEmpty)
	{
		if (pRDI->GetParameters().GetSize() != 1)
		{
			pRDI->SetReturnValue(DataBool());
			return TRUE;
		}
		strName = ((DataStr*)(pRDI->GetParamValue(0)))->GetString();

		if (m_pEngine && m_pEngine->m_bRunning && m_pEngine->m_pSymTable && m_pEngine->m_pSymTable->GetDisplayTables())
		{
			ASSERT_VALID(m_pEngine);		
			int idx = m_pEngine->m_pSymTable->GetDisplayTables()->Find(strName, DisplayTables::CURRENT_LAYOUT);
			if (idx < 0)
			{
				pRDI->SetReturnValue(DataBool());
				return TRUE;
			}
			DisplayTableEntryEngine* pDispTable = (DisplayTableEntryEngine*) m_pEngine->m_pSymTable->GetDisplayTables()->GetAt(idx);
			ASSERT_VALID(pDispTable);		
			short nRow = pDispTable->GetCurrentRow();
			//TODO
			pRDI->SetReturnValue(DataBool());
			return TRUE;
		}
		else if (m_pEditorManager)
		{
			ASSERT_VALID(m_pEditorManager);
			DisplayTables* pTbls = m_pEditorManager->GetDispTable();
			if (pTbls)
			{
				ASSERT_VALID(pTbls);
				int idx = pTbls->Find(strName, DisplayTables::CURRENT_LAYOUT);
				if (idx >= 0)
				{
					DisplayTableEntry* pTE = pTbls->GetAt(idx);
					ASSERT_VALID(pTE);
					WORD wID = pTE->GetId();
					BaseObj* pO = this->GetObjects().FindByID(wID);
					ASSERT_VALID(pO);
					if (pO)
					{
						if (pO->IsKindOf(RUNTIME_CLASS(Table)))
						{
							Table* pT = (Table*) pO;

							BOOL b = FALSE;
							if (strF == IDF_WOORM_CurrentRowContainsCellTail)
								 b = pT->ExistsCellTail(pT->m_nViewCurrentRow);
							else if (strF == IDF_WOORM_CurrentRowContainsCellTail)
								b =	pT->ExistsCellSubTotal(pT->m_nViewCurrentRow);
							else if (strF == IDF_WOORM_CurrentRowIsEmpty)
								b = pT->IsEmptyRow(pT->m_nViewCurrentRow);
							return TRUE;
						}
						else if (pO->IsKindOf(RUNTIME_CLASS(Repeater)) && strF == IDF_WOORM_CurrentRowIsEmpty)
						{
							Repeater* pR = (Repeater*) pO;
							pRDI->SetReturnValue(DataBool(pR->IsEmptyRow(pR->m_nViewCurrentRow)));
							return TRUE;
						}
						else
						{
							ASSERT(FALSE);
							pRDI->SetReturnValue(DataBool());
							return TRUE;
						}
					}
				}
			}
		}

		pRDI->SetReturnValue(DataBool());
		return TRUE;
	}

	else if (strF == IDF_WOORM_GetRows)
	{
		if (pRDI->GetParameters().GetSize() != 1)
		{
			pRDI->SetReturnValue((DataInt)0);
			return TRUE;
		}
		WORD id = (WORD) (short) *((DataInt*)(pRDI->GetParamValue(0)));

		if (m_pEngine && m_pEngine->m_bRunning && m_pEngine->m_pSymTable && m_pEngine->m_pSymTable->GetDisplayTables())
		{
			int idx = m_pEngine->m_pSymTable->GetDisplayTables()->Find(id, DisplayTables::CURRENT_LAYOUT);
			if (idx < 0)
			{
				pRDI->SetReturnValue((DataInt)0);
				return TRUE;
			}
			short nRows = m_pEngine->m_pSymTable->GetDisplayTables()->GetRowsFromIdx(idx);

			pRDI->SetReturnValue(DataInt(nRows));
			return TRUE;
		}
		else 
		{
			BaseObj* obj = GetObjects().FindByID(id);
			if (obj)
			{
				pRDI->SetReturnValue(DataInt(obj->RowsNumber ()));
				return TRUE;
			}
		}

		pRDI->SetReturnValue(DataInt(0));
		return TRUE;
	}
	else if (strF == IDF_WOORM_GetCurrentRow)
	{
		if (pRDI->GetParameters().GetSize() != 1)
		{
			pRDI->SetReturnValue((DataInt)0);
			return TRUE;
		}
		WORD id = (WORD) (short) *((DataInt*)(pRDI->GetParamValue(0)));

		if (m_pEngine && m_pEngine->m_bRunning && m_pEngine->m_pSymTable && m_pEngine->m_pSymTable->GetDisplayTables())
		{
			ASSERT_VALID(m_pEngine);

			int idx = m_pEngine->m_pSymTable->GetDisplayTables()->Find(id, DisplayTables::CURRENT_LAYOUT);
			if (idx < 0)
			{
				pRDI->SetReturnValue((DataInt)0);
				return TRUE;
			}
			DisplayTableEntryEngine* pDispTable = (DisplayTableEntryEngine*) m_pEngine->m_pSymTable->GetDisplayTables()->GetAt(idx);
			ASSERT_VALID(pDispTable);		
			short nRow = pDispTable->GetCurrentRow();

			pRDI->SetReturnValue(DataInt(nRow));
			return TRUE;
		}
		else 
		{
			BaseObj* obj = GetObjects().FindByID(id);
			if (obj)
			{
				pRDI->SetReturnValue(DataInt(obj->GetViewCurrentRow()));
				return TRUE;
			}
		}

		pRDI->SetReturnValue(DataInt(0));
		return TRUE;
	}
	
	//-------------------------------------------------------------------------
	// Query Object 
	else if(strF == IDF_WOORM_QueryOpen)
	{
		CString strQueryName = ((DataStr*)(pRDI->GetParamValue(0)))->GetString();

		QueryObjItem* pQ = m_pEngine->GetSymTable()->FindQuery(strQueryName);
		if (pQ == NULL)
		{
			if (!IsInUnattendedMode() && (m_pWoormInfo ? !m_pWoormInfo->m_bHideFrame : TRUE))
				AfxMessageBox(_TB("Query unknown") + L" - " + strQueryName);

			pRDI->SetReturnValue((DataLng)0);
			return TRUE;
		}

		BOOL bOk = pQ->m_Query.Open();

		pRDI->SetReturnValue((DataBool)bOk);
		return TRUE;
	}
	else if(strF == IDF_WOORM_QueryExecute)
	{
		CString strQueryName = ((DataStr*)(pRDI->GetParamValue(0)))->GetString();

		QueryObjItem* pQ = m_pEngine->GetSymTable()->FindQuery(strQueryName);
		if (pQ == NULL)
		{
			if (!IsInUnattendedMode() && (m_pWoormInfo ? !m_pWoormInfo->m_bHideFrame : TRUE))
				AfxMessageBox(_TB("Query unknown") + L" - " + strQueryName);

			pRDI->SetReturnValue((DataLng)0);
			return TRUE;
		}

		BOOL bOk = m_pEngine->m_pScheduler->ExecuteSQLCommandRetBool(&pQ->m_Query, (BOOL_FUNC) &QueryObject::Execute);

		pRDI->SetReturnValue((DataBool)bOk);
		return TRUE;
	}
	else if(strF == IDF_WOORM_QueryCall)
	{
		CString strQueryName = ((DataStr*)(pRDI->GetParamValue(0)))->GetString();

		QueryObjItem* pQ = m_pEngine->GetSymTable()->FindQuery(strQueryName);
		if (pQ == NULL)
		{
			if (!IsInUnattendedMode() && (m_pWoormInfo ? !m_pWoormInfo->m_bHideFrame : TRUE))
				AfxMessageBox(_TB("Query unknown") + L" - " + strQueryName);

			pRDI->SetReturnValue((DataLng)0);
			return TRUE;
		}

		BOOL bOk = m_pEngine->m_pScheduler->ExecuteSQLCommandRetBool(&pQ->m_Query, (BOOL_FUNC) &QueryObject::Call);

		pRDI->SetReturnValue((DataBool)bOk);
		return TRUE;
	}
	else if(strF == IDF_WOORM_QueryClose)
	{
		CString strQueryName = ((DataStr*)(pRDI->GetParamValue(0)))->GetString();

		QueryObjItem* pQ = m_pEngine->GetSymTable()->FindQuery(strQueryName);
		if (pQ == NULL)
		{
			if (!IsInUnattendedMode() && (m_pWoormInfo ? !m_pWoormInfo->m_bHideFrame : TRUE))
				AfxMessageBox(_TB("Query unknown") + L" - " + strQueryName);

			pRDI->SetReturnValue((DataLng)0);
			return TRUE;
		}

		pQ->m_Query.Close();

		pRDI->SetReturnValue((DataBool)true);
		return TRUE;
	}
	else if(strF == IDF_WOORM_QueryIsEof)
	{
		CString strQueryName = ((DataStr*)(pRDI->GetParamValue(0)))->GetString();

		QueryObjItem* pQ = m_pEngine->GetSymTable()->FindQuery(strQueryName);
		if (pQ == NULL)
		{
			if (!IsInUnattendedMode() && (m_pWoormInfo ? !m_pWoormInfo->m_bHideFrame : TRUE))
				AfxMessageBox(_TB("Query unknown") + L" - " + strQueryName);

			pRDI->SetReturnValue((DataLng)0);
			return TRUE;
		}

		BOOL bEof = pQ->m_Query.IsEof();

		pRDI->SetReturnValue((DataBool)bEof);
		return TRUE;
	}
	else if(strF == IDF_WOORM_QueryRead)
	{
		CString strQueryName = ((DataStr*)(pRDI->GetParamValue(0)))->GetString();

		QueryObjItem* pQ = m_pEngine->GetSymTable()->FindQuery(strQueryName);
		if (pQ == NULL)
		{
			if (!IsInUnattendedMode() && (m_pWoormInfo ? !m_pWoormInfo->m_bHideFrame : TRUE))
				AfxMessageBox(_TB("Query unknown") + L" - " + strQueryName);

			pRDI->SetReturnValue((DataLng)0);
			return TRUE;
		}

		BOOL bOk = m_pEngine->m_pScheduler->ExecuteSQLCommandRetBool(&pQ->m_Query, (BOOL_FUNC) &QueryObject::Read);

		pRDI->SetReturnValue((DataBool)bOk);
		return TRUE;
	}
	else if(strF == IDF_WOORM_QueryReadOne)
	{
		CString strQueryName = ((DataStr*)(pRDI->GetParamValue(0)))->GetString();

		QueryObjItem* pQ = NULL;

		WoormTable* symTb = m_pEngine->GetSymTable();		
		if(symTb)
			pQ = symTb->FindQuery(strQueryName);
		else
			TRACE(_T("PREVIEW DIALOG: null symtable\n"));

		if (pQ == NULL)
		{
			if (!IsInUnattendedMode() && (m_pWoormInfo ? !m_pWoormInfo->m_bHideFrame : TRUE))
				AfxMessageBox(_TB("Query unknown") + L" - " + strQueryName);

			pRDI->SetReturnValue((DataLng)0);
			return TRUE;
		}

		BOOL bOk = m_pEngine->m_pScheduler->ExecuteSQLCommandRetBool(&pQ->m_Query, (BOOL_FUNC) &QueryObject::ReadOne);

		pRDI->SetReturnValue((DataBool)bOk);
		return TRUE;
	}
	else if(strF == IDF_WOORM_QueryIsOpen)
	{
		CString strQueryName = ((DataStr*)(pRDI->GetParamValue(0)))->GetString();

		QueryObjItem* pQ = m_pEngine->GetSymTable()->FindQuery(strQueryName);
		if (pQ == NULL)
		{
			if (!IsInUnattendedMode() && (m_pWoormInfo ? !m_pWoormInfo->m_bHideFrame : TRUE))
				AfxMessageBox(_TB("Query unknown") + L" - " + strQueryName);

			pRDI->SetReturnValue((DataLng)0);
			return TRUE;
		}

		BOOL bOk = pQ->m_Query.IsOpen();

		pRDI->SetReturnValue((DataBool)bOk);
		return TRUE;
	}

	else if(strF ==  IDF_WOORM_QuerySetConnection)
	{
		CString strQueryName = ((DataStr*)(pRDI->GetParamValue(0)))->GetString();

		QueryObjItem* pQ = m_pEngine->GetSymTable()->FindQuery(strQueryName);
		if (pQ == NULL)
		{
			if (!IsInUnattendedMode() && (m_pWoormInfo ? !m_pWoormInfo->m_bHideFrame : TRUE))
				AfxMessageBox(_TB("Query unknown") + L" - " + strQueryName);

			pRDI->SetReturnValue((DataLng)0);
			return TRUE;
		}

		long handle = (long)*((DataLng*)(pRDI->GetParamValue(1)));
		if (handle == 0)
		{
			pRDI->SetReturnValue((DataBool)FALSE);
			return TRUE;
		}
		SqlSession* pSqlSession = (SqlSession*)handle;
		if (!pSqlSession->IsKindOf(RUNTIME_CLASS(SqlSession)))
		{
			pRDI->SetReturnValue((DataBool)FALSE);
			return TRUE;
		}

		pQ->m_Query.SetSqlSession(pSqlSession);

		pRDI->SetReturnValue((DataBool)true);
		return TRUE;
	}
	else if(strF == IDF_WOORM_QuerySetCursorType)
	{
		CString strQueryName = ((DataStr*)(pRDI->GetParamValue(0)))->GetString();

		QueryObjItem* pQ = m_pEngine->GetSymTable()->FindQuery(strQueryName);
		if (pQ == NULL)
		{
			if (!IsInUnattendedMode() && (m_pWoormInfo ? !m_pWoormInfo->m_bHideFrame : TRUE))
				AfxMessageBox(_TB("Query unknown") + L" - " + strQueryName);

			pRDI->SetReturnValue((DataLng)0);
			return TRUE;
		}

		long ct = (long)*((DataLng*)(pRDI->GetParamValue(1)));
		if (ct < 0 || ct > 4)
		{
			pRDI->SetReturnValue((DataBool)FALSE);
			return TRUE;
		}
		pQ->m_Query.SetCursorType((CursorType)ct);

		pRDI->SetReturnValue((DataBool)true);
		return TRUE;
	}

	else if(strF ==  IDF_WOORM_GetConnection)
	{
		long handle = (long)*((DataLng*)(pRDI->GetParamValue(0)));

		SqlSession* pSqlSession = NULL;
		if (handle == 0)
			pSqlSession = this->m_pTbContext->GetUpdatableSqlSession();
		else if (handle == -1)
			pSqlSession = this->m_pTbContext->GetSqlConnection()->GetNewSqlSession();
		else
		{
			CBaseDocument* pDoc = (CBaseDocument*)handle;
			if (!pDoc->IsKindOf(RUNTIME_CLASS(CBaseDocument)))
			{
				pRDI->SetReturnValue((DataLng)0);
				return TRUE;
			}
			pSqlSession = pDoc->m_pTbContext->GetUpdatableSqlSession();
		}

		pRDI->SetReturnValue(DataLng(long(pSqlSession)));
		return TRUE;
	}
	else if(strF ==  IDF_WOORM_OpenConnection)
	{
		DataStr dsConn = *((DataStr*)(pRDI->GetParamValue(0)));

		SqlSession* pSqlSession = AfxOpenSqlSession(dsConn);
		
		pRDI->SetReturnValue(DataLng(long(pSqlSession)));
		return TRUE;
	}
	else if(strF ==  IDF_WOORM_CloseConnection)
	{
		long handle = (long)*((DataLng*)(pRDI->GetParamValue(0)));
		if (handle == 0)
		{
			pRDI->SetReturnValue((DataBool)FALSE);
			return TRUE;
		}
		SqlSession* pSqlSession = (SqlSession*)handle;
		if (!pSqlSession->IsKindOf(RUNTIME_CLASS(SqlSession)))
		{
			pRDI->SetReturnValue((DataBool)FALSE);
			return TRUE;
		}

		pSqlSession->Close();
		delete pSqlSession;

		pRDI->SetReturnValue((DataBool)true);
		return TRUE;
	}

	// prototipi ------------------------------------------
	else if(strF ==  IDF_WOORM_QueryIsFailed)
	{
		//TODO
		pRDI->SetReturnValue((DataBool)false);
		return TRUE;
	}
	else if(strF ==  IDF_WOORM_QueryGetErrorInfo)
	{
		//TODO
		pRDI->SetReturnValue(DataStr());
		return TRUE;
	}
	else if(strF ==  IDF_WOORM_QueryGetSqlString)
	{
		//TODO
		pRDI->SetReturnValue(DataStr());
		return TRUE;
	}
	else if(strF == IDF_WOORM_ConnectionLockRecord)
	{
		CWaitCursor wc;
		
		DataObj* pS = AfxGetSettingValue(snsTbOleDb, szLockManager, szMaxReportLockRetry, DataInt(8), szTbDefaultSettingFileName);
		int nMaxReportLockRetry = pS ? *((DataInt*) pS) : 8;
		
		pS = AfxGetSettingValue(snsTbOleDb, szLockManager, szMaxReportLockTime, DataInt(4000), szTbDefaultSettingFileName);
		int nMaxReportLockTime = pS ? *((DataInt*) pS)  : 4000;

		void* hConn = (void*)((long)*((DataLng*)(pRDI->GetParamValue(0))));
		BOOL bOk = hConn && AfxIsValidAddress(hConn, sizeof(SqlConnection)) && ((SqlConnection*)hConn)->IsKindOf(RUNTIME_CLASS(SqlConnection));
		
		SqlConnection* pSqlConn = bOk ? (SqlConnection*) hConn : AfxGetDefaultSqlConnection();
		CString strAddress = cwsprintf(_T("%lp"), pSqlConn->m_pContext); 
		CString strCompanyDBName = pSqlConn->GetDatabaseName();

		CString strTableName = ((DataStr*)(pRDI->GetParamValue(1)))->GetString();
		const SqlCatalogEntry* pEntry = pSqlConn->GetCatalogEntry(strTableName);
		if (!pEntry)
		{
			TRACE(_T("The parameter %d of LockRecord function must be the name of table to lock\n"));
			ASSERT(FALSE);
			return FALSE;
		}
		SqlRecord* pRecord = pEntry->CreateRecord();

		pRDI->GetParamValue(2)->Clear();

		//passaggio dei parametri
		for (int i = 3; i < pRDI->GetParameters().GetUpperBound(); i += 2)
		{
			CDataObjDescription* parName = pRDI->GetParamDescription(i);
			CDataObjDescription* par = pRDI->GetParamDescription(i+1);

			if (parName->GetDataType() != DataType::String || parName->GetValue() == NULL)
			{
				TRACE(_T("The parameter %d of LockRecord function must have string type and it must be equal to the key column name of record to lock\n"), i);
				ASSERT(FALSE);
				pRDI->SetReturnValue((DataBool)false);
				delete pRecord;
				return TRUE;
			}

			CString strName = ((DataStr*)(parName->GetValue()))->GetString();
			
			//check congruenta dei parametri con nome e tipo dei key fields del record
			SqlRecordItem* pRecItem = pRecord->GetItemByColumnName (strName);
			if (!pRecItem)
			{
				TRACE(_T("The parameter %d of LockRecord function must be column of table\n"), i);
				ASSERT(FALSE);
				pRDI->SetReturnValue((DataBool)false);
				delete pRecord;
				return TRUE;
			}
			if (!pRecItem->IsSpecial())
			{
				TRACE(_T("The parameter %d of LockRecord function must be a primary key segment of table\n"), i);
				ASSERT(FALSE);
				pRDI->SetReturnValue((DataBool)false);
				delete pRecord;
				return TRUE;
			}

			DataObj* pParObj = par->GetValue();
			DataObj* pRecObj = pRecItem->GetDataObj();

			if (!DataType::IsCompatible(pParObj->GetDataType(), pRecObj->GetDataType()))
			{
				TRACE(_T("The parameter %d of LockRecord function must have a data type compatible to relative column\n"), i+1);
				ASSERT(FALSE);
				pRDI->SetReturnValue((DataBool)false);
				delete pRecord;
				return TRUE;
			}

			pRecObj->Assign(*pParObj);
		}

		CString strLockKey; 
		SqlLockMng::SetKey(pRecord, strLockKey);
		delete pRecord;

		CLockManagerInterface* pILock = AfxGetLockManager();

		CString strLockMsg;
		DWORD swStartTime = ::GetTickCount();
		for (int i = 0; i < nMaxReportLockRetry; i++)
		{
			bOk = pILock->LockCurrent 
								(
									strCompanyDBName, 
									strTableName, 
									strLockKey, 
									strAddress, 
									strLockMsg
								);
			if (bOk) 
				break;
			::Sleep(nMaxReportLockTime);
		}

		((DataStr*)pRDI->GetParamValue(2))->Assign(strLockMsg);

		pRDI->SetReturnValue(DataBool(bOk));
		return TRUE;
	}
	else if(strF ==  IDF_WOORM_ConnectionUnlockRecord)
	{
		void* hConn = (void*)((long)*((DataLng*)(pRDI->GetParamValue(0))));
		BOOL bOk = hConn && AfxIsValidAddress(hConn, sizeof(SqlConnection)) && ((SqlConnection*)hConn)->IsKindOf(RUNTIME_CLASS(SqlConnection));
		
		SqlConnection* pSqlConn = bOk ? (SqlConnection*) hConn : AfxGetDefaultSqlConnection();
		CString strAddress = cwsprintf(_T("%lp"), pSqlConn->m_pContext); //cwsprintf(_T("%lp"), pTable->m_pContext)
		CString strCompanyDBName = pSqlConn->GetDatabaseName();

		CString strTableName = ((DataStr*)(pRDI->GetParamValue(1)))->GetString();
		const SqlCatalogEntry* pEntry = pSqlConn->GetCatalogEntry(strTableName);
		if (!pEntry)
			return FALSE;
		SqlRecord* pRecord = pEntry->CreateRecord();

		//passaggio dei parametri
		for (int i = 2; i < pRDI->GetParameters().GetUpperBound(); i += 2)
		{
			CDataObjDescription* parName = pRDI->GetParamDescription(i);
			CDataObjDescription* par = pRDI->GetParamDescription(i+1);

			if (parName->GetDataType() != DataType::String || parName->GetValue() == NULL)
			{
				TRACE(_T("The parameter %d of LockRecord function must have string type and it must be equal to the key column name of record to lock\n"), i);
				ASSERT(FALSE);
				pRDI->SetReturnValue((DataBool)false);
				delete pRecord;
				return TRUE;
			}

			CString strName = ((DataStr*)(parName->GetValue()))->GetString();
			
			//check congruenta dei parametri con nome e tipo dei key fields del record
			SqlRecordItem* pRecItem = pRecord->GetItemByColumnName (strName);
			if (!pRecItem)
			{
				TRACE(_T("The parameter %d of LockRecord function must be column of table\n"), i);
				ASSERT(FALSE);
				pRDI->SetReturnValue((DataBool)false);
				delete pRecord;
				return TRUE;
			}
			if (!pRecItem->IsSpecial())
			{
				TRACE(_T("The parameter %d of LockRecord function must be a primary key segment of table\n"), i);
				ASSERT(FALSE);
				pRDI->SetReturnValue((DataBool)false);
				delete pRecord;
				return TRUE;
			}

			DataObj* pParObj = par->GetValue();
			DataObj* pRecObj = pRecItem->GetDataObj();

			if (!DataType::IsCompatible(pParObj->GetDataType(), pRecObj->GetDataType()))
			{
				TRACE(_T("The parameter %d of LockRecord function must have a data type compatible to relative column\n"), i+1);
				ASSERT(FALSE);
				pRDI->SetReturnValue((DataBool)false);
				delete pRecord;
				return TRUE;
			}

			pRecObj->Assign(*pParObj);
		}

		CString strLockKey; 
		SqlLockMng::SetKey(pRecord, strLockKey);
		delete pRecord;

		CLockManagerInterface* pILock = AfxGetLockManager();

		bOk = pILock->UnlockCurrent 
							(
								strCompanyDBName, 
								strTableName, 
								strLockKey, 
								strAddress
							);

		pRDI->SetReturnValue(DataBool(bOk));
		return TRUE;
	}
	else if(strF ==  IDF_WOORM_ConnectionUnlockAll)
	{
		void* hConn = (void*)((long)*((DataLng*)(pRDI->GetParamValue(0))));
		BOOL bOk = hConn && AfxIsValidAddress(hConn, sizeof(SqlConnection)) && ((SqlConnection*)hConn)->IsKindOf(RUNTIME_CLASS(SqlConnection));
		
		SqlConnection* pSqlConn = bOk ? (SqlConnection*) hConn : AfxGetDefaultSqlConnection();
		CString strAddress = cwsprintf(_T("%lp"), pSqlConn->m_pContext); //cwsprintf(_T("%lp"), pTable->m_pContext)

		CString strCompanyDBName = pSqlConn->GetDatabaseName();

		CLockManagerInterface* pILock = AfxGetLockManager();

		CString strLockMsg;
		bOk = pILock->UnlockAllContext 
								(
									strCompanyDBName, 
									strAddress
								);

		pRDI->SetReturnValue(DataBool(bOk));
		return TRUE;
	}
	else if(strF ==  IDF_WOORM_ConnectionBeginTrans)
	{
		void* hConn = (void*)((long)*((DataLng*)(pRDI->GetParamValue(0))));
		BOOL bOk = hConn && AfxIsValidAddress(hConn, sizeof(SqlConnection)) && ((SqlConnection*)hConn)->IsKindOf(RUNTIME_CLASS(SqlConnection));
		
		SqlConnection* pSqlConn = bOk ? (SqlConnection*) hConn : AfxGetDefaultSqlConnection();

		pRDI->SetReturnValue((DataBool)false);
		return TRUE;
	}
	else if(strF ==  IDF_WOORM_ConnectionCommit)
	{
		void* hConn = (void*)((long)*((DataLng*)(pRDI->GetParamValue(0))));
		BOOL bOk = hConn && AfxIsValidAddress(hConn, sizeof(SqlConnection)) && ((SqlConnection*)hConn)->IsKindOf(RUNTIME_CLASS(SqlConnection));
		
		SqlConnection* pSqlConn = bOk ? (SqlConnection*) hConn : AfxGetDefaultSqlConnection();

		pRDI->SetReturnValue((DataBool)false);
		return TRUE;
	}
	else if(strF ==  IDF_WOORM_ConnectionRollback)
	{
		void* hConn = (void*)((long)*((DataLng*)(pRDI->GetParamValue(0))));
		BOOL bOk = hConn && AfxIsValidAddress(hConn, sizeof(SqlConnection)) && ((SqlConnection*)hConn)->IsKindOf(RUNTIME_CLASS(SqlConnection));
		
		SqlConnection* pSqlConn = bOk ? (SqlConnection*) hConn : AfxGetDefaultSqlConnection();

		pRDI->SetReturnValue((DataBool)false);
		return TRUE;
	}

	return FALSE; //m_pCallerDocument && m_pCallerDocument->DispatchFunctionCall(pRDI);
}

//-----------------------------------------------------------------------------
BOOL CWoormDocMng::GetReportVariables(CTBNamespace& nsReport, DataTypeNamedArray& arReportColumns, DataTypeNamedArray& arReportAskFields)
{
	if (!nsReport.IsValid())
		return FALSE;

	// istanzia il report per farne fare il Parse 
	CString strFilePath = AfxGetPathFinder()->GetReportFullName(nsReport, AfxGetLoginInfos()->m_strUserName);
	
	CWoormInfo aWoormInfo;
	aWoormInfo.AddReport(strFilePath);
	aWoormInfo.m_bOwnedByReport = FALSE;
	aWoormInfo.m_bHideFrame = TRUE;

	AfxGetApp()->BeginWaitCursor();	
	AfxGetExportSymbols()->Activate();

	BOOL bOk = TRUE;
	// apro il report senza farne la run
	CWoormDocMng* pDoc = (CWoormDocMng*) AfxGetTbCmdManager()->RunWoormReport(&aWoormInfo, NULL, NULL, FALSE, FALSE);
	if (pDoc)
	{
		if (!pDoc->ForceEngineParse())
		{
			bOk = FALSE;
			TRACE(_T("Error on parsing report: %s\n"), strFilePath);
		}
		
		AfxGetTbCmdManager()->CloseDocument(pDoc);
	}
	else 
	{
		bOk = FALSE;
		TRACE(_T("Missing report: %s\n"), strFilePath);
	}

	if (bOk)
	{
		AfxGetExportSymbols()->GetItemsFileFilteredByType(strFilePath, EXP_SYMB_VAR_COLUMN, arReportColumns);	
		AfxGetExportSymbols()->GetItemsFileFilteredByType(strFilePath, EXP_SYMB_VAR_ASK, arReportAskFields);	
	}

	AfxGetExportSymbols()->DeActivate();

	AfxGetApp()->EndWaitCursor();	
	return bOk;
}

//-----------------------------------------------------------------------------
BOOL CWoormDocMng::AddEmailAttachments
						(
							const CString& sAttachmens,
							const CString& sTitles,
							BOOL bAppend
						)
{
	BOOL bOk = TRUE;

	CStringArray arFiles;
	CStringArray_Split(arFiles, sAttachmens);

	CStringArray arTitles;
	CStringArray_Split(arTitles, sTitles);

	for (int i = 0; i < arFiles.GetSize(); i++)
	{
		CString strFile (arFiles[i]); strFile.Trim();

		CString strTitle;
		if (i <= arTitles.GetUpperBound())
		{
			strTitle = arTitles[i] ; strTitle.Trim();
		}

		if (!::ExistFile(strFile))
		{
			CTBNamespace aFileNs(strFile);
			if (!aFileNs.IsValid())
			{
				bOk = FALSE;
				continue;
			}
			if (strTitle.IsEmpty())
			{
				strTitle = aFileNs.GetObjectName();
			}
			CString sUser = AfxGetLoginInfos()->m_strUserName;
			strFile = AfxGetPathFinder()->GetFileNameFromNamespace(aFileNs, sUser);
			if (!::ExistFile(strFile))
			{
				bOk = FALSE;
				continue;
			}
		}
		if (strTitle.IsEmpty())
		{
			strTitle = ::GetNameWithExtension(strFile);
		}

		m_Email.SetAttachment
			(
				strFile,
				strTitle,
				bAppend
			);

		bAppend = TRUE;
	}
	return bOk;
}
//=============================================================================
