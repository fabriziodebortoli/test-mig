#include "StdAfx.h"

#include <afxcoll.h>
#include <TbGenlib\TbCommandInterface.h>
#include <TbGenlib\basedoc.h>
#include <TbWoormViewer\export.H>
#include <TbWoormViewer\woormdoc.H>

#include "MDataObj.h"
#include "MWoormInfo.h"
#include "MStringList.h"

using namespace System;
using namespace System::Collections::Generic;
using namespace Microarea::Framework::TBApplicationWrapper;
using namespace Microarea::TaskBuilderNet::Core::EasyBuilder;
/////////////////////////////////////////////////////////////////////////////
//							MEMailInfo
/////////////////////////////////////////////////////////////////////////////
//---------------------------------------------------------------------------
MEMailMessage::MEMailMessage(CMapiMessage* pMail, CDiagnostic* pDiagnostic)
	:
	m_pMail (pMail),
	m_pDiagnostic(pDiagnostic)
{
	hasCodeBehind = true;
}

//---------------------------------------------------------------------------
MEMailMessage::MEMailMessage()
	:
	m_pDiagnostic(AfxGetDiagnostic())
{
	m_pMail = new CMapiMessage();
}

//---------------------------------------------------------------------------
MEMailMessage::~MEMailMessage()
{
	this->!MEMailMessage();
	GC::SuppressFinalize(this);
}

//---------------------------------------------------------------------------
MEMailMessage::!MEMailMessage()
{
	if (!hasCodeBehind)
		SAFE_DELETE(m_pMail);
}

//---------------------------------------------------------------------------
System::String^ MEMailMessage::Subject::get()
{
	return m_pMail ? gcnew System::String(m_pMail->m_sSubject) : System::String::Empty;
}

//---------------------------------------------------------------------------
void MEMailMessage::Subject::set(System::String^ value)
{
	if (m_pMail)
		m_pMail->m_sSubject = CString(value);
}

//---------------------------------------------------------------------------
System::String^ MEMailMessage::Body::get()
{
	return m_pMail ? gcnew System::String(m_pMail->m_sBody) : System::String::Empty;
}

//---------------------------------------------------------------------------
void MEMailMessage::Body::set(System::String^ value)
{
	if (m_pMail)
		m_pMail->m_sBody = CString(value);
}

//---------------------------------------------------------------------------
System::String^ MEMailMessage::From::get()
{
	return m_pMail ? gcnew System::String(m_pMail->m_sFrom) : System::String::Empty;
}

//---------------------------------------------------------------------------
void MEMailMessage::From::set(System::String^ value)
{
	if (m_pMail)
		m_pMail->m_sFrom = CString(value);
}

//---------------------------------------------------------------------------
System::String^ MEMailMessage::Identity::get()
{
	return m_pMail ? gcnew System::String(m_pMail->m_sIdentity) : System::String::Empty;
}

//---------------------------------------------------------------------------
void MEMailMessage::Identity::set(System::String^ value)
{
	if (m_pMail)
		m_pMail->m_sIdentity = CString(value);
}

//---------------------------------------------------------------------------
System::String^ MEMailMessage::FromName::get()
{
	return m_pMail ? gcnew System::String(m_pMail->m_sFromName) : System::String::Empty;
}

//---------------------------------------------------------------------------
void MEMailMessage::FromName::set(System::String^ value)
{
	if (m_pMail)
		m_pMail->m_sFromName = CString(value);
}

//---------------------------------------------------------------------------
System::String^ MEMailMessage::ReplyTo::get()
{
	return m_pMail ? gcnew System::String(m_pMail->m_sReplayTo) : System::String::Empty;
}

//---------------------------------------------------------------------------
void MEMailMessage::ReplyTo::set(System::String^ value)
{
	if (m_pMail)
		m_pMail->m_sReplayTo = CString(value);
}

//---------------------------------------------------------------------------
System::Collections::Generic::IList<System::String^>^ MEMailMessage::Tos::get()
{
	if (tos == nullptr)
		tos = gcnew MStringList (&m_pMail->m_To);
	
	return tos;
}

//---------------------------------------------------------------------------
System::Collections::Generic::IList<System::String^>^ MEMailMessage::Ccs::get()
{
	if (ccs == nullptr)
		ccs = gcnew MStringList (&m_pMail->m_CC);
	
	return ccs;
}

//---------------------------------------------------------------------------
System::Collections::Generic::IList<System::String^>^ MEMailMessage::Bccs::get()
{
	if (bccs == nullptr)
		bccs = gcnew MStringList (&m_pMail->m_BCC);
	
	return bccs;
}

//---------------------------------------------------------------------------
System::Collections::Generic::IList<System::String^>^ MEMailMessage::Attachments::get()
{
	if (attachments == nullptr)
		attachments = gcnew MStringList (&m_pMail->m_Attachments);
	
	return attachments;
}

//---------------------------------------------------------------------------
System::Collections::Generic::IList<System::String^>^ MEMailMessage::AttachmentsTitles::get()
{
	if (attachmentsTitles == nullptr)
		attachmentsTitles = gcnew MStringList (&m_pMail->m_AttachmentTitles);
	
	return attachmentsTitles;
}

//---------------------------------------------------------------------------
System::String^ MEMailMessage::MapiProfile::get()
{
	return m_pMail ? gcnew System::String(m_pMail->m_sMapiProfile) : System::String::Empty;
}

//---------------------------------------------------------------------------
void MEMailMessage::MapiProfile::set(System::String^ value)
{
	if (m_pMail)
		m_pMail->m_sMapiProfile = CString(value);
}

//---------------------------------------------------------------------------
bool MEMailMessage::ReadNotification::get()
{
	return readNotification;
}

//---------------------------------------------------------------------------
void MEMailMessage::ReadNotification::set(bool value)
{
	readNotification = value;
}

//---------------------------------------------------------------------------
bool MEMailMessage::DeliveryNotification::get()
{
	return deliveryNotification;
}

//---------------------------------------------------------------------------
void MEMailMessage::DeliveryNotification::set(bool value)
{
	deliveryNotification = value;
}

//---------------------------------------------------------------------------
bool MEMailMessage::Send()
{
	BOOL aDeliveryNotification = deliveryNotification == true;
	BOOL aReadNotification = readNotification == true;

	return AfxGetIMailConnector()->SendMail (*m_pMail, &aDeliveryNotification, &aReadNotification, m_pDiagnostic) == TRUE;
}

/////////////////////////////////////////////////////////////////////////////
//							CDisposableReports
/////////////////////////////////////////////////////////////////////////////
public class CDisposableReports : public  CArray<WoormDocPtr, WoormDocPtr>
{
public:
	CDisposableReports () {}

	void Run	(CWoormInfo* pReport, CBaseDocument* pCallerDoc = NULL);
	void Close	();
};

//---------------------------------------------------------------------------
void CDisposableReports::Run (CWoormInfo* pInfo, CBaseDocument* pCallerDoc /*NULL*/)
{
	// utilizzo l'unico RunWoormReport e faccio gestire l'array
	// di report al CWoormInfo xchè altrimento non riesco a 
	// fare la condivisione dei parametri tra un report e l'altro
	CWoormDoc* pDoc = AfxGetTbCmdManager()->RunWoormReport(pInfo, pCallerDoc);
	if (pDoc)
		Add(pDoc);
}

//---------------------------------------------------------------------------
void CDisposableReports::Close ()
{
	for (int i= GetUpperBound(); i >= 0; i--)
	{
		CWoormDoc* pDoc = (CWoormDoc*) GetAt(i);
		if (pDoc)
			AfxGetTbCmdManager()->CloseWoormReport(pDoc);
	}
}

/////////////////////////////////////////////////////////////////////////////
//							MWoormInfo
/////////////////////////////////////////////////////////////////////////////
//---------------------------------------------------------------------------
MWoormInfo::MWoormInfo()
{
	m_pInfo = new CWoormInfo();
	m_pInfo->m_bOwnedByReport = FALSE;
	m_pReportDocs = new CDisposableReports();
	mail = gcnew MEMailMessage(&m_pInfo->m_Email, AfxGetDiagnostic());
}

//---------------------------------------------------------------------------
MWoormInfo::MWoormInfo(System::IntPtr ptrInfo)
	: 
	m_pInfo((CWoormInfo*)ptrInfo.ToInt64())
{
	hasCodeBehind = true;
	m_pReportDocs = new CDisposableReports();
	mail = gcnew MEMailMessage(&m_pInfo->m_Email, AfxGetDiagnostic());
}

//---------------------------------------------------------------------------
MWoormInfo::~MWoormInfo(void)
{
	this->!MWoormInfo();
	GC::SuppressFinalize(this);
}

//---------------------------------------------------------------------------
MWoormInfo::!MWoormInfo()
{
	if (!hasCodeBehind)
		SAFE_DELETE(m_pInfo)
	SAFE_DELETE(m_pReportDocs);
}

//---------------------------------------------------------------------------
void MWoormInfo::AddParameter(System::String^ name, MDataObj^ value)
{
	m_pInfo->AddParam(name, value->GetDataObj());
}

//---------------------------------------------------------------------------
cli::array<System::String^>^ MWoormInfo::GetInputParamNames()
{
	if (!m_pInfo)
		return gcnew cli::array<System::String^>{};
	List<System::String^>^ list = gcnew List<System::String^>();
	for (int i = 0; i < m_pInfo->GetParameters().GetCount(); i++)
	{
		CDataObjDescription* pParam = m_pInfo->GetParamDescription(i);
		if (pParam->IsPassedModeOut())
			continue;
		list->Add(gcnew System::String(pParam->GetName()));
	}
	return list->ToArray();
}
//---------------------------------------------------------------------------
Object^ MWoormInfo::GetInputParamValue(System::String^ name)
{
	if (!m_pInfo)
		return nullptr;
	CDataObjDescription* pParam = m_pInfo->GetParamDescription(name);
	
	DataObj* pData = m_pInfo->GetParamValue(name);

	if (!pParam || pParam->IsPassedModeOut())
		return nullptr;
	MDataObj^ pObj = MDataObj::Create(pParam->GetValue());
	return pObj->Value;
}

//---------------------------------------------------------------------------
IList<System::String^>^ MWoormInfo::Reports::get()
{
	if (reports == nullptr)
		reports = gcnew MStringList (&m_pInfo->m_ReportNames);
	return reports;
}

//---------------------------------------------------------------------------
void MWoormInfo::RunReports	()
{
	if (m_pReportDocs)
	{
		CBaseDocument* pContextDoc = context != nullptr && context->TbHandle != 0 ? 
										(CBaseDocument*) (long) context->TbHandle : NULL;		
		((CDisposableReports*) m_pReportDocs)->Run(m_pInfo, pContextDoc);
	}
}

//---------------------------------------------------------------------------
void MWoormInfo::CloseReports ()
{
	// già chiusa
	if (m_pReportDocs)
		((CDisposableReports*) m_pReportDocs)->Close();
}

//---------------------------------------------------------------------------
void MWoormInfo::Copies::set(int value)
{
	if (m_pInfo)
		m_pInfo->SetCopies(value);
}

//---------------------------------------------------------------------------
bool MWoormInfo::UseMultiCopy::get()
{
	return m_pInfo ? m_pInfo->m_bMultiCopies == TRUE : false;
}

//---------------------------------------------------------------------------
void MWoormInfo::UseMultiCopy::set(bool value)
{
	if (m_pInfo)
		m_pInfo->m_bMultiCopies = value;
}

//---------------------------------------------------------------------------
MWoormInfo::PrintDialogUI MWoormInfo::PrintDialog::get()
{
	if (!m_pInfo)
		return MWoormInfo::PrintDialogUI::Default;

	if (m_pInfo->m_bOnePrintDialog)
		return MWoormInfo::PrintDialogUI::OnlyOne;

	if (m_pInfo->m_bNoPrintDialog)
		return MWoormInfo::PrintDialogUI::NotShown;

	if (m_pInfo->m_bShowPrintDialogBeforeRunning)
		MWoormInfo::PrintDialogUI::ShowBeforeRunning;

	return MWoormInfo::PrintDialogUI::Default;
}

//---------------------------------------------------------------------------
void  MWoormInfo::PrintDialog::set(MWoormInfo::PrintDialogUI value)
{
	if (!m_pInfo)
		return;

	m_pInfo->m_bOnePrintDialog = (value == PrintDialogUI::OnlyOne || value == PrintDialogUI::ShowBeforeRunning);
	m_pInfo->m_bNoPrintDialog  = (value == PrintDialogUI::NotShown);
	m_pInfo->m_bShowPrintDialogBeforeRunning = (value == PrintDialogUI::ShowBeforeRunning);
}

//---------------------------------------------------------------------------
bool  MWoormInfo::CloseAfterPrint::get()
{
	return m_pInfo ? m_pInfo->m_bCloseOnEndPrint == TRUE : false;
}

//---------------------------------------------------------------------------
void  MWoormInfo::CloseAfterPrint::set(bool value)
{
	if (m_pInfo)
		m_pInfo->m_bCloseOnEndPrint = value;
}

//---------------------------------------------------------------------------
bool  MWoormInfo::AutoPrint::get()
{
	return m_pInfo ? m_pInfo->m_bAutoPrint == TRUE : false;
}

//---------------------------------------------------------------------------
void  MWoormInfo::AutoPrint::set(bool value)
{
	if (m_pInfo)
		m_pInfo->m_bAutoPrint = value;
}

//---------------------------------------------------------------------------
MWoormInfo::ReportUI MWoormInfo::UI::get()
{
	if (!m_pInfo)
		return MWoormInfo::ReportUI::Default;
	
	if (m_pInfo->m_bHideFrame)
		return MWoormInfo::ReportUI::Hidden;

	if (m_pInfo->m_bIconized)
		return MWoormInfo::ReportUI::Minimized;

	return MWoormInfo::ReportUI::Default;
}

//---------------------------------------------------------------------------
void  MWoormInfo::UI::set(MWoormInfo::ReportUI value)
{
	if (!m_pInfo)
		return;

	m_pInfo->m_bHideFrame = (value == ReportUI::Hidden);
	m_pInfo->m_bIconized = (value == ReportUI::Minimized);
}

//---------------------------------------------------------------------------
MWoormInfo::FileFormats MWoormInfo::AttachmentsFormat::get()
{
	if (!m_pInfo)
		return MWoormInfo::FileFormats::None;
	
	if (m_pInfo->m_bAttachPDF)
		return MWoormInfo::FileFormats::Pdf;

	if (m_pInfo->m_bAttachOther)
		return ConvertToFileFormats();

	return MWoormInfo::FileFormats::None;
}

//---------------------------------------------------------------------------
void  MWoormInfo::AttachmentsFormat::set(MWoormInfo::FileFormats value)
{
	if (!m_pInfo)
		return;

	m_pInfo->m_bAttachPDF = (value == FileFormats::Pdf);
	if (m_pInfo->m_bAttachPDF)
		m_pInfo->m_arExportInfo.RemoveAll();

	m_pInfo->m_bAttachOther = (value > FileFormats::Pdf);

	if (m_pInfo->m_bAttachOther)
		ConvertToExportInfo(value);
}

//---------------------------------------------------------------------------
bool MWoormInfo::AttachmentsCompressed::get()
{
	return m_pInfo ? m_pInfo->m_bCompressAttach == TRUE : false;
}

//---------------------------------------------------------------------------
void MWoormInfo::AttachmentsCompressed::set(bool value)
{
	if (m_pInfo)
		m_pInfo->m_bCompressAttach = value;
}

//---------------------------------------------------------------------------
System::Collections::Generic::IList<System::String^>^ MWoormInfo::OutputFiles::get()
{
	if (outputFiles == nullptr)
		outputFiles = gcnew MStringList (&m_pInfo->m_arstrOutputFileNames);
	
	return outputFiles;
}

//---------------------------------------------------------------------------
MWoormInfo::FileFormats MWoormInfo::GeneratesFile::get()
{
	if (!m_pInfo)
		return MWoormInfo::FileFormats::None;
	
	if (m_pInfo->m_bPDFOutput)
		return MWoormInfo::FileFormats::Pdf;

	return ConvertToFileFormats();
}

//---------------------------------------------------------------------------
void  MWoormInfo::GeneratesFile::set(MWoormInfo::FileFormats value)
{
	if (!m_pInfo)
		return;

	m_pInfo->m_bPDFOutput = (value == FileFormats::Pdf);
	if (m_pInfo->m_bPDFOutput)
		m_pInfo->m_arExportInfo.RemoveAll();

	if (value > FileFormats::Pdf)
		ConvertToExportInfo(value);
}

//---------------------------------------------------------------------------
System::String^ MWoormInfo::PrinterName::get()
{
	return m_pInfo ? gcnew System::String(m_pInfo->m_strPrinterName) : System::String::Empty;
}

//---------------------------------------------------------------------------
void MWoormInfo::PrinterName::set(System::String^ value)
{
	if (m_pInfo)
		m_pInfo->m_strPrinterName = CString(value);
}

//---------------------------------------------------------------------------
bool MWoormInfo::SendMail::get()
{
	return m_pInfo ? m_pInfo->m_bSendEmail == TRUE : false;
}

//---------------------------------------------------------------------------
void MWoormInfo::SendMail::set(bool value)
{
	if (m_pInfo)
		m_pInfo->m_bSendEmail = value;
}

//---------------------------------------------------------------------------
bool MWoormInfo::ShowMailUI::get()
{
	return m_pInfo ? m_pInfo->m_bShowUI2SendMail == TRUE : false;
}

//---------------------------------------------------------------------------
void MWoormInfo::ShowMailUI::set(bool value)
{
	if (m_pInfo)
		m_pInfo->m_bShowUI2SendMail = value;
}

//---------------------------------------------------------------------------
MEMailMessage^ MWoormInfo::EMail::get()
{
	return mail;
}

//---------------------------------------------------------------------------
MWoormInfo::PrintStatus MWoormInfo::Status::get()
{
	if (!m_pInfo)
		return MWoormInfo::PrintStatus::Undefined;

	if (m_pInfo->m_bPrintAborted)
		return MWoormInfo::PrintStatus::Aborted;

	if (m_pInfo->m_bPrinted)
		return MWoormInfo::PrintStatus::Executed;

	return MWoormInfo::PrintStatus::Undefined;
}

//---------------------------------------------------------------------------
MWoormInfo::FileFormats MWoormInfo::ConvertToFileFormats()
{
	if (!m_pInfo || !m_pInfo->m_arExportInfo.GetSize())
		return MWoormInfo::FileFormats::None;

	CExportInfo* pInfo = (CExportInfo*) m_pInfo->m_arExportInfo.GetAt(0);
		
	switch (pInfo->GetExportType())
	{
	case TypeOfExport::EXPORT_OPENOFFICE_ODS_TYPE:	return	MWoormInfo::FileFormats::OpenOfficeSheet;
	case TypeOfExport::EXPORT_EXCELNET_TYPE:		return	MWoormInfo::FileFormats::ExcelNet;
	case TypeOfExport::EXPORT_WORDNET_TYPE:		return	MWoormInfo::FileFormats::WordNet;
	case TypeOfExport::EXPORT_CSV_TYPE:			return	MWoormInfo::FileFormats::Csv;
	case TypeOfExport::EXPORT_HTML_TYPE:			return	MWoormInfo::FileFormats::Html;
	case TypeOfExport::EXPORT_XML_TYPE:			return	MWoormInfo::FileFormats::Xml;
	}

	return	MWoormInfo::FileFormats::None;
}

//---------------------------------------------------------------------------
void MWoormInfo::ConvertToExportInfo(MWoormInfo::FileFormats ff)
{
	TypeOfExport nExportType = TypeOfExport::EXPORT_TXTCLIPBOARD_TYPE;

	switch (ff)
	{
	case MWoormInfo::FileFormats::OpenOfficeSheet:	nExportType = TypeOfExport::EXPORT_OPENOFFICE_ODS_TYPE;	break;
	case MWoormInfo::FileFormats::OpenOfficeDoc:	nExportType = TypeOfExport::EXPORT_OPENOFFICE_ODT_TYPE;	break;
	case MWoormInfo::FileFormats::ExcelNet:			nExportType = TypeOfExport::EXPORT_EXCELNET_TYPE;		break;
	case MWoormInfo::FileFormats::OpenXmlExcel:		nExportType = TypeOfExport::EXPORT_OPENXML_EXCEL_TYPE;	break;
	case MWoormInfo::FileFormats::Csv:				nExportType = TypeOfExport::EXPORT_CSV_TYPE;			break;
	case MWoormInfo::FileFormats::Html:				nExportType = TypeOfExport::EXPORT_HTML_TYPE;			break;
	case MWoormInfo::FileFormats::Xml:				nExportType = TypeOfExport::EXPORT_XML_TYPE;			break;
	case MWoormInfo::FileFormats::ReportXml:		nExportType = TypeOfExport::EXPORT_XML_FULL_TYPE;		break;
	case MWoormInfo::FileFormats::Json:				nExportType = TypeOfExport::EXPORT_JSON_TYPE;			break;
	case MWoormInfo::FileFormats::WordNet:			nExportType = TypeOfExport::EXPORT_WORDNET_TYPE;		break;
		default: break;
	}

	if (nExportType != TypeOfExport::EXPORT_TXTCLIPBOARD_TYPE)
	{
		CExportInfo* pInfo = new CExportInfo();
		pInfo->SetExportType(nExportType);
		m_pInfo->m_arExportInfo.Add(pInfo);
	}
}

//---------------------------------------------------------------------------
IDocumentDataManager^ MWoormInfo::Context::get ()
{
	return context;
}

//---------------------------------------------------------------------------
void MWoormInfo::Context::set (IDocumentDataManager^ value)
{
	context = value;
}