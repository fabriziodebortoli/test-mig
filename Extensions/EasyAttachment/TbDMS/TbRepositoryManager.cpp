#include "stdafx.h" 

#include <TbClientCore\ClientObjects.h>
#include <TBNameSolver\ThreadContext.h>
#include <TBNameSolver\PathFinder.h>
#include <TBNameSolver\LoginContext.h>
#include <tbnamesolver\FileSystemFunctions.h>
#include <TbGenlib\BarCode.h>
#include <TBGENLIB\baseapp.h>
#include <TBGENERIC\globals.h>
#include <TBGENERIC\dataobj.h>
#include <TBGENERIC\array.h>
#include <TbOleDb\OleDbMng.h>
#include <TbFrameworkImages\GeneralFunctions.h>
#include <TbFrameworkImages\CommonImages.h>

#include "TbDMSFunctions.h"
#include "CDDMS.h"
#include "TBDMSEnums.h"
#include "TbDMSInterface.h"
#include "CommonObjects.h"
#include "SOSObjects.h"
#include "DMSSearchFilter.h"
#include "BDDMSRepository.h"
#include "BDSOSDocSender.h"
#include "BDMassiveArchive.h"

#include "BDSOSAdjustAttachments.h"
#include "TbRepositoryManager.h"

#ifdef _DEBUG
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

using namespace System;
using namespace System::Collections;
using namespace System::Collections::Generic;
using namespace System::Threading;
using namespace System::Globalization;
using namespace System::Diagnostics;
using namespace System::Data;
using namespace System::Collections::Specialized;
using namespace System::Xml;

using namespace Microarea::TaskBuilderNet::Core::Generic;
using namespace Microarea::TaskBuilderNet::Core::WebServicesWrapper;
using namespace Microarea::TaskBuilderNet::Interfaces::Model;
using namespace Microarea::Framework::TBApplicationWrapper;
using namespace Microarea::EasyAttachment;
using namespace Microarea::EasyAttachment::BusinessLogic;
using namespace Microarea::EasyAttachment::Components;
using namespace Microarea::EasyAttachment::Core;

///////////////////////////////////////////////////////////////////////////////
//					MDMSEventMng definition & declaration
//				Gestore eventi del DMSOrchestrator
///////////////////////////////////////////////////////////////////////////////
//
ref class MDMSOrchestratorEventMng
{
public:
	void  OnAdminSettingsSaved(System::Object^, System::EventArgs^);
};

//-----------------------------------------------------------------------------
void MDMSOrchestratorEventMng::OnAdminSettingsSaved(System::Object^, System::EventArgs^)
{
	InitDMSManager(NULL);
}

//ho dovuto incapsulare l'oggetto managed dmsOrchestrator in una classe mixed a causa del file unmanager UnmanagedFuncions.cpp
//che include TBRepositoryManaged.h. Se veniva utilizzato direttamente dmsOrchestrator in TbRepositoryManager
// il compilatore segnalava errore
///////////////////////////////////////////////////////////////////////////////
//								MDMSOrchestrator declaration
///////////////////////////////////////////////////////////////////////////////
//
class MDMSOrchestrator
{
public: 
	MDMSOrchestrator() 
	: 
	  dmsOrchestrator		(nullptr)
	 {
		dmsOrchestrator = gcnew DMSOrchestrator();
		dmsOrchestrator->CompanyCustomPath = gcnew String(AfxGetPathFinder()->GetCompanyPath());
		dmsOrchestrator->DMSConnectionString = gcnew String(AfxGetOleDbMng()->GetDMSConnectionString());
		dmsOrchestrator->WorkersTable = gcnew MWorkersTable((IntPtr)AfxGetWorkersTable());
		dmsOrchestrator->WorkerId = (Int32)AfxGetWorkerId();
		dmsOrchestrator->IsAdmin = AfxGetLoginInfos()->m_bAdmin == TRUE;
		
		dmsOrchestrator->AuthenticationToken = gcnew String(AfxGetAuthenticationToken());
		dmsOrchestrator->ServerName = gcnew String(AfxGetLoginManager()->GetServer());
		dmsOrchestrator->ServicePort = (Int32)AfxGetCommonClientObjects()->GetServerConnectionInfo ()->m_nWebServicesPort;

		const CLoginInfos *pInfos = AfxGetLoginInfos();	
		dmsOrchestrator->SecurityEnabled = (pInfos->m_bSecurity && AfxIsActivated(szExtensionsApp, TBSECURITY_ACT)) || AfxGetLoginManager()->IsSecurityLightEnabled();
		dmsOrchestrator->MailConnectorEnabled = AfxIsActivated(szExtensionsApp, MAILCONNECTOR_ACT) == TRUE;
		dmsOrchestrator->SosConnectorActivated = (AfxIsActivated(szExtensionsApp, SOSCONNECTOR_FUNCTIONALITY) == TRUE && AfxGetOleDbMng()->DMSSOSEnable());
		dmsOrchestrator->SID = (Int32)pInfos->m_wDataBaseCultureLCID;
		dmsOrchestrator->CompanyID = (Int32)pInfos->m_nCompanyId;
		dmsOrchestrator->LoginId = (Int32)pInfos->m_nLoginId;
		dmsOrchestrator->InitializeManager(gcnew String(AfxGetAuthenticationToken()), gcnew String(AfxGetLoginInfos()->m_strUserName));		
	 }

	//------------------------------------------------------------------------------------
	~MDMSOrchestrator() 
	{
		//in fase di chiusura pulisco i file temporanei
		dmsOrchestrator->RemoveTemporaryFiles();
		dmsOrchestrator->DestroyManager();
		delete dmsOrchestrator;
	}

public:
	gcroot<Microarea::EasyAttachment::BusinessLogic::DMSOrchestrator^>	dmsOrchestrator;
};

///////////////////////////////////////////////////////////////////////////////
//						TbRepositoryManager declaration
///////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(TbRepositoryManager, CObject)

//-----------------------------------------------------------------------------
TbRepositoryManager::TbRepositoryManager()
	:
	m_pDMSOrchestrator	(NULL),
	m_pDMSSettings		(NULL),
	m_pSOSConfiguration	(NULL),
	m_pDMSSqlConnection	(NULL),
	b_IsTemporary		(FALSE)
{
	if (AfxGetOleDbMng()->EasyAttachmentEnable() || AfxGetOleDbMng()->GetDMSStatus() == StorageInvalid)
		m_pDMSOrchestrator = new MDMSOrchestrator();
}

//-----------------------------------------------------------------------------
TbRepositoryManager::~TbRepositoryManager()
{
	if (m_pDMSOrchestrator)
		delete m_pDMSOrchestrator;

	if (m_pDMSSettings)
		delete m_pDMSSettings;

	if (m_pSOSConfiguration)
		delete m_pSOSConfiguration;
}

//-----------------------------------------------------------------------------
SqlConnection* TbRepositoryManager::GetDMSSqlConnection()
{
	if (!m_pDMSSqlConnection)
	{
		// é il login manager che mi fornisce la login 
		LPCWSTR szConnectionString = T2W((LPTSTR)((LPCTSTR)cwsprintf(_T("Provider=SQLOLEDB;{0-%s}"), AfxGetOleDbMng()->GetDMSConnectionString())));
		TRY
		{
			m_pDMSSqlConnection = AfxGetOleDbMng()->MakeNewConnection(szConnectionString, false, false, false);
		}
		CATCH(SqlException, e)
		{
			return NULL;
		}
		END_CATCH
	}

	return m_pDMSSqlConnection;
}

//-----------------------------------------------------------------------------
CAttachmentInfo* CreateCAttachmentInfo(AttachmentInfo^ attachment)
{
	CAttachmentInfo* attInfo = new CAttachmentInfo();

	DataDate archivedDate(attachment->ArchivedDate.Day, attachment->ArchivedDate.Month, attachment->ArchivedDate.Year, attachment->ArchivedDate.Hour, attachment->ArchivedDate.Minute, attachment->ArchivedDate.Second);
	DataDate attachedDate(attachment->AttachedDate.Day, attachment->AttachedDate.Month, attachment->AttachedDate.Year, attachment->AttachedDate.Hour, attachment->AttachedDate.Minute, attachment->AttachedDate.Second);
	attInfo->m_attachmentID = DataLng(attachment->AttachmentId);
	attInfo->m_ArchivedDocId = DataLng(attachment->ArchivedDocId);
	attInfo->m_CollectionId = DataLng(attachment->CollectionID);
	attInfo->m_ErpDocumentId = DataLng(attachment->ErpDocumentID);
	attInfo->m_Name = DataStr((CString)attachment->Name);
	attInfo->m_OriginalPath = DataStr((CString)attachment->OriginalPath);
	attInfo->m_Description = DataStr((CString)attachment->Description);
	attInfo->m_ExtensionType = DataStr((CString)attachment->ExtensionType);
	attInfo->m_ArchivedDate = archivedDate;
	attInfo->m_AttachedDate = attachedDate;
	attInfo->m_ModifiedBy = DataStr((CString)attachment->ModifiedBy);
	attInfo->m_CreatedBy = DataStr((CString)attachment->CreatedBy);
	attInfo->m_IsAPapery = DataBool(attachment->IsAPapery);
	attInfo->m_ERPDocNamespace = DataStr((CString)attachment->ERPDocNamespace);
	attInfo->m_ERPPrimaryKeyValue = DataStr((CString)attachment->ERPPrimaryKeyValue);
	attInfo->m_StorageFile = DataStr((CString)attachment->StorageFile);
	// TODO: devo mettere anche qui attachment->IsWoormReport????

	return attInfo;
}

//implementazione dell'interfaccia IDMSRepositoryManager
//-----------------------------------------------------------------------------
CAttachmentInfo* TbRepositoryManager::GetAttachmentInfo(int attachmentID)
{
	CAttachmentInfo* attInfo = NULL;
	if (m_pDMSOrchestrator)
	{
		Microarea::EasyAttachment::Components::AttachmentInfo^  attachment = m_pDMSOrchestrator->dmsOrchestrator->GetAttachmentInfoFromAttachmentId(attachmentID);
		if (attachment)
		{
			DataDate archivedDate(attachment->ArchivedDate.Day, attachment->ArchivedDate.Month, attachment->ArchivedDate.Year, attachment->ArchivedDate.Hour, attachment->ArchivedDate.Minute, attachment->ArchivedDate.Second);
			DataDate attachedDate(attachment->AttachedDate.Day, attachment->AttachedDate.Month, attachment->AttachedDate.Year, attachment->AttachedDate.Hour, attachment->AttachedDate.Minute, attachment->AttachedDate.Second);
			attInfo = CreateCAttachmentInfo(attachment);
		}
	}
	return attInfo;
}

//-----------------------------------------------------------------------------
CString TbRepositoryManager::GetAttachmentTempFile(int attachmentID)
{
	return (m_pDMSOrchestrator) ? CString(m_pDMSOrchestrator->dmsOrchestrator->GetAttachmentTempFile(attachmentID)) : _T("");
}

//-----------------------------------------------------------------------------
void TbRepositoryManager::OpenAttachment(int attachmentID)
{
	if (m_pDMSOrchestrator)
	{
		String^ tempPath = m_pDMSOrchestrator->dmsOrchestrator->GetAttachmentTempFile(attachmentID);
		OpenDocument(CString(tempPath));
	}
}

//-----------------------------------------------------------------------------
bool TbRepositoryManager::GetAttachmentBinaryContent(int attachmentID, DataBlob& binaryContent, DataStr& fileName, DataBool& veryLargeFile)
{
	binaryContent.Clear();
	fileName.Clear();

	if (!m_pDMSOrchestrator)
		return false;

	bool veryBigFile = false;
	String^ name = gcnew String("");
	array<Byte>^ content = nullptr;

	pin_ptr<Byte> temp = nullptr;
	content = m_pDMSOrchestrator->dmsOrchestrator->GetBinaryContent(attachmentID, veryBigFile, name);
	if (content != nullptr)
	{
		temp = &content[0];
		binaryContent.Assign((void*)temp, content->Length);
		delete content;
	}
	fileName.Assign(CString(name));
	veryLargeFile.Assign(veryBigFile == true);

	return true;
}

//-----------------------------------------------------------------------------
CAttachmentsArray* TbRepositoryManager::GetAttachments(const CString& strDocNamespace, const CString& strDocKey, AttachmentFilterTypeEnum filterType)
{
	CAttachmentsArray* attachments = new CAttachmentsArray();
	if (m_pDMSOrchestrator)
	{
		List<Microarea::EasyAttachment::Components::AttachmentInfo^>^ attachmentList =
			m_pDMSOrchestrator->dmsOrchestrator->GetAttachments(gcnew String(strDocNamespace), gcnew String(strDocKey), (Microarea::EasyAttachment::Components::AttachmentFilterType)filterType);

		for each (AttachmentInfo^ attachment in attachmentList)
			attachments->Add(CreateCAttachmentInfo(attachment));
	}
	return attachments;
}

//-----------------------------------------------------------------------------
CAttachmentsArray* TbRepositoryManager::GetAttachmentsByGuid(const CString& strDocNamespace, const CString& strTbGuid, AttachmentFilterTypeEnum filterType)
{
	CAttachmentsArray* attachments = new CAttachmentsArray();
	if (m_pDMSOrchestrator)
	{
		List<Microarea::EasyAttachment::Components::AttachmentInfo^>^ attachmentList =
			m_pDMSOrchestrator->dmsOrchestrator->GetAttachmentsByGuid(gcnew String(strDocNamespace), gcnew String(strTbGuid), (Microarea::EasyAttachment::Components::AttachmentFilterType)filterType);

		for each (AttachmentInfo^ attachment in attachmentList)
			attachments->Add(CreateCAttachmentInfo(attachment));
	}
	return attachments;
}

//-----------------------------------------------------------------------------
int TbRepositoryManager::GetAttachmentsCount(const CString& strDocNamespace, const CString& strDocKey, AttachmentFilterTypeEnum filterType)
{
	return
		(m_pDMSOrchestrator)
		? m_pDMSOrchestrator->dmsOrchestrator->GetAttachmentsCount(gcnew String(strDocNamespace), gcnew String(strDocKey), (Microarea::EasyAttachment::Components::AttachmentFilterType)filterType)
		: -1;
}

//-----------------------------------------------------------------------------------------------------
bool TbRepositoryManager::DeleteAttachment(int attachmentID)
{
	if (!m_pDMSOrchestrator)
		return false;

	//questa chiamata è effettuata via webmethod. Per renderla autonoma (vedi problematica stato unattended mode per messaggi) istanzio un nuovo dmsOrchestrator che poi verrà disposato
	DMSOrchestrator^ dmsOrch = gcnew DMSOrchestrator();
	dmsOrch->InitializeManager(m_pDMSOrchestrator->dmsOrchestrator);
	dmsOrch->InUnattendedMode = true;

	return dmsOrch->DeleteAttachment(attachmentID, -1);
}

//-----------------------------------------------------------------------------------------------------
bool TbRepositoryManager::DeleteDocumentAttachments(const CString& strDocNamespace, const CString& strDocKey, CString& strMsg)
{
	if (!m_pDMSOrchestrator)
		return false;

	//questa chiamata è effettuata via webmethod. Per renderla autonoma (vedi problematica stato unattended mode per messaggi) istanzio un nuovo dmsOrchestrator che poi verrà disposato
	DMSOrchestrator^ dmsOrch = gcnew DMSOrchestrator();
	dmsOrch->InitializeManager(m_pDMSOrchestrator->dmsOrchestrator);
	dmsOrch->InUnattendedMode = true;

	String^ tempmsg = gcnew String("");
	bool ok = dmsOrch->DeleteAttachments(gcnew String(strDocNamespace), gcnew String(strDocKey), tempmsg);
	strMsg = tempmsg;

	return ok;
}

//-----------------------------------------------------------------------------------------------------
bool TbRepositoryManager::DeleteAllERPDocumentInfo(const CString& strDocNamespace, const CString& strDocKey, CString& strMsg)
{
	if (!m_pDMSOrchestrator)
		return false;

	//questa chiamata è effettuata via webmethod. Per renderla autonoma (vedi problematica stato unattended mode per messaggi) istanzio un nuovo dmsOrchestrator che poi verrà disposato
	DMSOrchestrator^ dmsOrch = gcnew DMSOrchestrator();
	dmsOrch->InitializeManager(m_pDMSOrchestrator->dmsOrchestrator);
	dmsOrch->InUnattendedMode = true;

	String^ tempmsg = gcnew String("");
	bool ok = dmsOrch->DeleteAllErpDocumentInfo(gcnew String(strDocNamespace), gcnew String(strDocKey), tempmsg);
	strMsg = tempmsg;

	return ok;
}

//-----------------------------------------------------------------------------------------------------
bool TbRepositoryManager::CheckIn(int nAttachmentID)
{
	if (!m_pDMSOrchestrator)
		return false;

	Microarea::EasyAttachment::Components::AttachmentInfo^ attachment = m_pDMSOrchestrator->dmsOrchestrator->GetAttachmentInfoFromAttachmentId(nAttachmentID);
	return attachment && m_pDMSOrchestrator->dmsOrchestrator->CheckIn(attachment);
}

//-----------------------------------------------------------------------------------------------------
bool TbRepositoryManager::CheckOut(int nAttachmentID)
{
	if (!m_pDMSOrchestrator)
		return false;

	Microarea::EasyAttachment::Components::AttachmentInfo^ attachment = m_pDMSOrchestrator->dmsOrchestrator->GetAttachmentInfoFromAttachmentId(nAttachmentID);
	return attachment && m_pDMSOrchestrator->dmsOrchestrator->CheckOut(attachment);
}

//-----------------------------------------------------------------------------------------------------
bool TbRepositoryManager::Undo(int nAttachmentID)
{
	if (!m_pDMSOrchestrator)
		return false;

	Microarea::EasyAttachment::Components::AttachmentInfo^ attachment = m_pDMSOrchestrator->dmsOrchestrator->GetAttachmentInfoFromAttachmentId(nAttachmentID);
	return attachment && m_pDMSOrchestrator->dmsOrchestrator->Undo(attachment);
}

//-----------------------------------------------------------------------------
bool TbRepositoryManager::BarcodeEnabled()
{
	return m_pDMSOrchestrator && m_pDMSOrchestrator->dmsOrchestrator->BarcodeEnabled;
}

//-----------------------------------------------------------------------------
bool TbRepositoryManager::SosConnectorEnabled()
{
	return m_pDMSOrchestrator && m_pDMSOrchestrator->dmsOrchestrator->SosConnectorEnabled;
}

//Return the complete pdf file name used to archive the report with reportNamespace</remarks>
//----------------------------------------------------------------------------
CString TbRepositoryManager::GetPdfFileName(const CString& strReportNamespace, CBaseDocument* pCallerDoc, const CString& strAlternativeName)
{
	CString strTempPath(GetEasyAttachmentTempPath());
	CString key;
	CTBNamespace aNsReport(strReportNamespace.GetString());
	CString reportName = (strAlternativeName.IsEmpty()) ? cwsprintf(_T("{0-%s}{1-%s}"), aNsReport.GetModuleName(), aNsReport.GetObjectName()) : strAlternativeName;
	reportName.Replace(_T(".wrm"), _T(""));
	if (pCallerDoc  && pCallerDoc->IsKindOf(RUNTIME_CLASS(CAbstractFormDoc)))
	{
		CAbstractFormDoc* pDoc = (CAbstractFormDoc*)pCallerDoc;
		if (pDoc && pDoc->ValidCurrentRecord())
			key = (pDoc) ? pDoc->GetKeyInXMLFormat() : _T("");
		reportName = cwsprintf(_T("{0-%s}{1-%s}"), reportName, key);
	}

	// sostituisco con _ i caratteri riservati potrebbero invalidare il salvataggio su filesystem di un file
	reportName.Replace(_T("\\"), _T("_"));
	reportName.Replace(_T("/"), _T("_"));
	reportName.Replace(_T(":"), _T("_"));
	reportName.Replace(_T("*"), _T("_"));
	reportName.Replace(_T("?"), _T("_"));
	reportName.Replace(_T("<"), _T("_"));
	reportName.Replace(_T(">"), _T("_"));
	reportName.Replace(_T("|"), _T("_"));
	reportName.Replace(_T("\r"), _T("_"));
	reportName.Replace(_T("\n"), _T("_"));
	//

	return DataStr(MakeFilePath(strTempPath, reportName, _T(".pdf")));
}

//restituisce il barcode da associare al report di woorm.
//se il report è in fase di archiviazione allora viene restituito il barcode del report se già archiviato oppure un nuovo barcode
// se il report è in fase di running viene restituito il barcode solo se il report è già sato archiviato altrimenti barcode vuoto
//----------------------------------------------------------------------------
CTypedBarcode TbRepositoryManager::GetBarcodeValue(const CString& strReportNamespace, CBaseDocument* pCallerDoc, const CString& strAlternativeName, bool isArchiving)
{
	CTypedBarcode aTypedBarcode;
	if (!AfxGetTbRepositoryManager()->BarcodeEnabled() || !m_pDMSOrchestrator)
		return aTypedBarcode;

	CString pdfFileName = GetPdfFileName(strReportNamespace, pCallerDoc, strAlternativeName);
	if (pdfFileName.IsEmpty())
		return aTypedBarcode;

	Microarea::EasyAttachment::BusinessLogic::TypedBarcode^ tb = m_pDMSOrchestrator->dmsOrchestrator->GetBarcodeForReport(gcnew String(pdfFileName), isArchiving);
	CTypedBarcode ctb(tb->Value, tb->TypeDescription);
	return ctb;
}

//-----------------------------------------------------------------------------
::ArchiveResult TbRepositoryManager::ArchiveReport(const CString& strPdfFileName, const CString& strReportTitle, CBaseDocument* pCallerDoc, const CString& strBarcode, bool deletePdfFileName, CString& strMessage)
{
	if (!AfxGetOleDbMng()->EasyAttachmentEnable())
		return ::ArchiveResult::Cancel;

	if (!ExistFile(strPdfFileName))
	{
		strMessage = cwsprintf(_TB("The file {0-%s} doesn't exist"), strPdfFileName);
		return ::ArchiveResult::Cancel;
	}

	::ArchiveResult archRes = TerminatedSuccess;
	strMessage = _T("");
	if (pCallerDoc  && pCallerDoc->IsKindOf(RUNTIME_CLASS(CAbstractFormDoc)))
	{
		CAbstractFormDoc* pDoc = (CAbstractFormDoc*)pCallerDoc;
		int attachmentID = -1;
		archRes = pDoc->GetDMSAttachmentManager()->AttachReport(strPdfFileName, strReportTitle, strBarcode, attachmentID, strMessage);
		if (deletePdfFileName)
			RemoveFile(strPdfFileName);
		if (archRes == TerminatedSuccess)
			strMessage = cwsprintf(_TB("The report {0-%s} is successfully attached to current document."), strReportTitle);
		else
			if (archRes == ::ArchiveResult::Cancel)
				strMessage = _TB("No report attached to current document.");
		return archRes;
	}

	//istanzio un nuovo dmsOrchestrator  che poi verrà disposato (SERVE????)
	//DMSOrchestrator^ dmsOrch = gcnew DMSOrchestrator();
	//dmsOrch->InitializeManager(m_pDMSOrchestrator->dmsOrchestrator);
	//dmsOrch->InUnattendedMode = true;
	archRes = (::ArchiveResult)(int)m_pDMSOrchestrator->dmsOrchestrator->ArchiveFile(gcnew String(strPdfFileName), gcnew String(strReportTitle), true, true, gcnew String(strBarcode));
	//delete dmsOrch;

	if (deletePdfFileName)
		RemoveFile(strPdfFileName);

	if (archRes == TerminatedSuccess)
		strMessage = cwsprintf(_TB("The report {0-%s} is successfully archived in the repository."), strReportTitle);

	return archRes;
}

//-----------------------------------------------------------------------------
::ArchiveResult TbRepositoryManager::GeneratePapery(const CString& reportName, const CString& strBarcode, CBaseDocument* pCallerDoc, CString& strMessage)
{
	strMessage.Empty();
	::ArchiveResult archRes = ::ArchiveResult::Cancel;
	if (pCallerDoc  && pCallerDoc->IsKindOf(RUNTIME_CLASS(CAbstractFormDoc)))
	{
		CAbstractFormDoc* pDoc = (CAbstractFormDoc*)pCallerDoc;
		CString description = cwsprintf(_TB("Create barcode from Generate Papery of report {0-%s}"), GetName(reportName));
		::ArchiveResult archRes = pDoc->GetDMSAttachmentManager()->AttachPapery(strBarcode, description, reportName, strMessage);
		if (archRes == ::ArchiveResult::TerminatedSuccess)
			strMessage = cwsprintf(_TB("The papery with barcode {0-%s} is successfully attached to current document."), strBarcode);
		else
			if (archRes == ::ArchiveResult::Cancel)
				strMessage = _TB("No papery attached to current document.");
	}

	return archRes;
}

//-------------------------------------------------------------------------------------------------------
::ArchiveResult	TbRepositoryManager::ArchiveFile(CString fileName, CString description, int& archivedDocID, CString& strMessages)
{
	if (!AfxGetOleDbMng()->EasyAttachmentEnable())
		return ::ArchiveResult::Cancel;

	//questa chiamata è effettuata via webmethod. Per renderla autonoma (vedi problematica stato unattended mode per messaggi) istanzio un nuovo dmsOrchestrator  che poi verrà disposato
	//(SERVE????)
	DMSOrchestrator^ dmsOrch = gcnew DMSOrchestrator();
	dmsOrch->InitializeManager(m_pDMSOrchestrator->dmsOrchestrator);
	dmsOrch->InUnattendedMode = true;
	::ArchiveResult result = (::ArchiveResult)(int)dmsOrch->ArchiveFile(gcnew String(fileName), gcnew String(description), false, false, gcnew String(""));
	delete dmsOrch;
	return result;
}

//-----------------------------------------------------------------------------
::ArchiveResult	TbRepositoryManager::AttachArchivedDocInDocument(int archivedDocID, CString documentNamespace, CString primaryKey, int& attachmentID, CString& strMessages)
{
	return (AttachArchivedDocInDocumentFunct(archivedDocID, documentNamespace, primaryKey, attachmentID, strMessages)) ? ::ArchiveResult::TerminatedSuccess : ::ArchiveResult::TerminatedWithError;
}

//-----------------------------------------------------------------------------
DataEnum GetDeletingEnum(int deletingOption)
{
	DataEnum eDelOption(E_DELETE_ATTACH_ACTION_DEFAULT);
	switch ((DeletingAttachmentAction)deletingOption)
	{
		case DeletingAttachmentAction::AskBeforeDeleteArchivedDoc:
			eDelOption.Assign(E_DELETE_ATTACH_ASK); break;
		case DeletingAttachmentAction::KeepArchivedDoc:
			eDelOption.Assign(E_DELETE_ATTACH_KEEP); break;
		case DeletingAttachmentAction::DeleteArchivedDoc:
			eDelOption.Assign(E_DELETE_ATTACH_DELETE); break;
	}
	return eDelOption;
}

//-----------------------------------------------------------------------------
DataEnum GetDuplicateEnum(int duplicateOption)
{
	DataEnum eDuplicate(E_DUPLICATE_ACTION_DEFAULT);
	switch ((DuplicateDocumentAction)duplicateOption)
	{
	case DuplicateDocumentAction::AskMeBeforeAttachDoc:
		eDuplicate.Assign(E_DUPLICATE_ASK); break;
	case DuplicateDocumentAction::ReplaceExistingDoc:
		eDuplicate.Assign(E_DUPLICATE_REPLACE); break;
	case DuplicateDocumentAction::ArchiveAndKeepBothDocs:
		eDuplicate.Assign(E_DUPLICATE_KEEP_BOTH); break;
	case DuplicateDocumentAction::UseExistingDoc:
		eDuplicate.Assign(E_DUPLICATE_USE_EXISTING); break;
	case DuplicateDocumentAction::RefuseAttachmentOperation:
		eDuplicate.Assign(E_DUPLICATE_REFUSE_ATTACH); break;
	case DuplicateDocumentAction::Cancel:
		eDuplicate.Assign(E_DUPLICATE_CANCEL); break;
	}
	return eDuplicate;
}

//-----------------------------------------------------------------------------
DataEnum GetBarcodeEnum(int barcodeType)
{
	if (barcodeType == (int)BarCodeType::BC_CODE39)
		return E_BARCODE_TYPE_ALFA39;

	if (barcodeType == (int)BarCodeType::BC_CODE128)
		return E_BARCODE_TYPE_CODE_128_AUTO;

	if (barcodeType == (int)BarCodeType::BC_EAN128)
		return E_BARCODE_TYPE_EAN128;

	/*if (barcodeType == (int)BarCodeType::BC_DATAMATRIX)
		return E_BARCODE_TYPE_DATAMATRIX;

	if (barcodeType == (int)BarCodeType::BC_MICROQR)
		return E_BARCODE_TYPE_MICROQR;

	if (barcodeType == (int)BarCodeType::BC_QR)
		return E_BARCODE_TYPE_QR;

	if (barcodeType == (int)BarCodeType::BC_PDF417)
		return E_BARCODE_TYPE_PDF417;*/

	return E_BARCODE_TYPE_ALFA39;
}

//-----------------------------------------------------------------------------
DataEnum GetBCDetectionEnum(int bcDetectionAction)
{
	DataEnum eBCDetection(E_BARCODE_DETECT_ACTION_DEFAULT);
	switch ((BarcodeDetectionAction)bcDetectionAction)
	{
	case BarcodeDetectionAction::DetectOnlyInFirstPage:
		eBCDetection.Assign(E_BARCODE_DETECT_FIRST_PAGE); break;
	case BarcodeDetectionAction::DetectTillOneValidBarcodeIsFound:
		eBCDetection.Assign(E_BARCODE_DETECT_TILL_ONE_VALID); break;
	}
	return eBCDetection;
}

//-----------------------------------------------------------------------------
int GetDeletingOption(DataEnum eDelOption)
{		
	if (eDelOption == E_DELETE_ATTACH_ASK)
		return (int)DeletingAttachmentAction::AskBeforeDeleteArchivedDoc;
	if (eDelOption == E_DELETE_ATTACH_KEEP)
		return (int)DeletingAttachmentAction::KeepArchivedDoc;
	if (eDelOption == E_DELETE_ATTACH_DELETE)
		return (int)DeletingAttachmentAction::DeleteArchivedDoc;

	return (int)DeletingAttachmentAction::AskBeforeDeleteArchivedDoc;
}

//-----------------------------------------------------------------------------
int GetDuplicateAction(DataEnum eDuplicate)
{
	if (eDuplicate == E_DUPLICATE_ASK)
		return (int)DuplicateDocumentAction::AskMeBeforeAttachDoc;
	if (eDuplicate == E_DUPLICATE_REPLACE)
		return (int)DuplicateDocumentAction::ReplaceExistingDoc;
	if (eDuplicate == E_DUPLICATE_KEEP_BOTH)
		return (int)DuplicateDocumentAction::ArchiveAndKeepBothDocs;
	if (eDuplicate == E_DUPLICATE_USE_EXISTING)
		return (int)DuplicateDocumentAction::UseExistingDoc;
	if (eDuplicate == E_DUPLICATE_REFUSE_ATTACH)
		return (int)DuplicateDocumentAction::RefuseAttachmentOperation;
	if (eDuplicate == E_DUPLICATE_CANCEL)
		return (int)DuplicateDocumentAction::Cancel;
	return (int)DuplicateDocumentAction::AskMeBeforeAttachDoc;
}

//-----------------------------------------------------------------------------
int GetBarcodeType(DataEnum eBarcode)
{
	if (eBarcode == E_BARCODE_TYPE_ALFA39)
		return (int)BarCodeType::BC_CODE39;
	
	if (eBarcode == E_BARCODE_TYPE_CODE_128_AUTO)
		return (int)BarCodeType::BC_CODE128;

	if (eBarcode == E_BARCODE_TYPE_EAN128)
		return (int)BarCodeType::BC_EAN128;

	/*if (eBarcode == E_BARCODE_TYPE_DATAMATRIX)
		return (int)BarCodeType::BC_DATAMATRIX;

	if (eBarcode == E_BARCODE_TYPE_MICROQR)
		return (int)BarCodeType::BC_MICROQR;

	if (eBarcode == E_BARCODE_TYPE_QR)
		return (int)BarCodeType::BC_QR;

	if (eBarcode == E_BARCODE_TYPE_PDF417)
		return (int)BarCodeType::BC_PDF417;*/

	return (int)BarCodeType::BC_CODE128;
}

//-----------------------------------------------------------------------------
int GetBCDetectionAction(DataEnum eBCDetection)
{
	if (eBCDetection == E_BARCODE_DETECT_FIRST_PAGE)
		return (int)BarcodeDetectionAction::DetectOnlyInFirstPage; 
	if (eBCDetection == E_BARCODE_DETECT_TILL_ONE_VALID)
		return (int)BarcodeDetectionAction::DetectTillOneValidBarcodeIsFound;
	return (int)BarcodeDetectionAction::DetectOnlyInFirstPage;
}

//-----------------------------------------------------------------------------
CDMSSettings* TbRepositoryManager::GetDMSSettings()
{
	if (m_pDMSSettings)
		return m_pDMSSettings;
	
	SettingState^ settingState = m_pDMSOrchestrator->dmsOrchestrator->SettingsManager->UsersSettingState;
	m_pDMSSettings = new CDMSSettings();

	VSettings* pRecSettings = m_pDMSSettings->GetSettings();

	pRecSettings->l_SettingType.Assign((int)settingState->Type);
	pRecSettings->l_WorkerID.	Assign(settingState->WorkerID);
	
	pRecSettings->l_DeletingAttachmentAction		= GetDeletingEnum((int)settingState->Options->DeleteOptionsState->DeletingAttachmentAction);
	pRecSettings->l_DuplicateActionForDoc			= GetDuplicateEnum((int)settingState->Options->DuplicateOptionsState->ActionForDocument);
	pRecSettings->l_DuplicateActionForBatch			= GetDuplicateEnum((int)settingState->Options->DuplicateOptionsState->ActionForBatch);
	pRecSettings->l_EnableBookmarkEmptyValues		= settingState->Options->BookmarksOptionsState->EnableEmptyValues == true;
	pRecSettings->l_DpiQualityImage					= settingState->Options->RepositoryOptionsState->DpiQualityImage;
	pRecSettings->l_EnableToAttachFromRepository	= settingState->Options->RepositoryOptionsState->EnableToAttachFromRepository == true;
	pRecSettings->l_ShowOnlyMyArchivedDocs			= settingState->Options->RepositoryOptionsState->ShowOnlyMyArchivedDocs == true;
	pRecSettings->l_ExcludedExtensions				= CString(settingState->Options->RepositoryOptionsState->ExcludedExtensions);
	pRecSettings->l_DisableAttachFromReport			= settingState->Options->RepositoryOptionsState->DisableAttachFromReport == true;
	pRecSettings->l_EnableBarcode					= settingState->Options->BarcodeDetectionOptionsState->EnableBarcode == true;
	pRecSettings->l_AutomaticBarcodeDetection		= settingState->Options->BarcodeDetectionOptionsState->AutomaticBarcodeDetection == true;
	pRecSettings->l_BarcodeDetectionAction			= GetBCDetectionEnum((int)settingState->Options->BarcodeDetectionOptionsState->BarcodeDetectionAction);
	pRecSettings->l_BCActionForDocument				= GetDuplicateEnum((int)settingState->Options->BarcodeDetectionOptionsState->BCActionForDocument);
	pRecSettings->l_BCActionForBatch				= GetDuplicateEnum((int)settingState->Options->BarcodeDetectionOptionsState->BCActionForBatch);
	pRecSettings->l_BarcodeType.Assign(GetBarcodeEnum((int)settingState->Options->BarcodeDetectionOptionsState->BarcodeType));
	pRecSettings->l_BarcodePrefix					= CString(settingState->Options->BarcodeDetectionOptionsState->BarcodePrefix);
	pRecSettings->l_PrintBarcodeInReport			= settingState->Options->BarcodeDetectionOptionsState->PrintBarcodeInReport == true;
	pRecSettings->l_EnableFTS						= (m_pDMSOrchestrator->dmsOrchestrator->FullTextSearchEnabled) ? (settingState->Options->FTSOptionsState->EnableFTS == true) : FALSE;
	pRecSettings->l_FTSNotConsiderPdF				= (pRecSettings->l_EnableFTS && settingState->Options->FTSOptionsState->FTSNotConsiderPdF == true);
	pRecSettings->l_EnableSOS						= settingState->Options->SOSOptionsState->EnableSOS == true;
	pRecSettings->l_MaxElementsInEnvelope			= settingState->Options->SOSOptionsState->MaxElementsInEnvelope;
	pRecSettings->l_StorageToFileSystem				= settingState->Options->StorageOptionsState->StorageToFileSystem;
	pRecSettings->l_StorageFolderPath				= settingState->Options->StorageOptionsState->StorageFolderPath;
	pRecSettings->l_MaxDocNumber					= settingState->Options->AttachmentPanelOptionsState->MaxDocNumber;

	VExtensionMaxSize* pRecExtension;
	for each(ExtensionSize^ es in settingState->Options->RepositoryOptionsState->Extensions)
	{
		pRecExtension = new VExtensionMaxSize();
		pRecExtension->l_Extension = es->Name;
		pRecExtension->l_MaxSize = es->Size;
		m_pDMSSettings->GetExtensions()->Add(pRecExtension);
	}

	return m_pDMSSettings;
}

//-----------------------------------------------------------------------------
BOOL TbRepositoryManager::SaveDMSSettings()
{
	if (!m_pDMSSettings)
		return FALSE;

	VSettings* pNewRecSettings = m_pDMSSettings->GetSettings();

	SettingState^ settingState = m_pDMSOrchestrator->dmsOrchestrator->SettingsManager->UsersSettingState;
	
	settingState->Options->DeleteOptionsState->DeletingAttachmentAction				= (DeletingAttachmentAction)GetDeletingOption(pNewRecSettings->l_DeletingAttachmentAction);
	settingState->Options->DuplicateOptionsState->ActionForDocument					= (DuplicateDocumentAction)GetDuplicateAction(pNewRecSettings->l_DuplicateActionForDoc);
	settingState->Options->DuplicateOptionsState->ActionForBatch					= (DuplicateDocumentAction)GetDuplicateAction(pNewRecSettings->l_DuplicateActionForBatch);
	settingState->Options->BookmarksOptionsState->EnableEmptyValues					= ((BOOL)pNewRecSettings->l_EnableBookmarkEmptyValues == TRUE);
	settingState->Options->RepositoryOptionsState->DpiQualityImage					= pNewRecSettings->l_DpiQualityImage;
	settingState->Options->RepositoryOptionsState->EnableToAttachFromRepository		= ((BOOL)pNewRecSettings->l_EnableToAttachFromRepository == TRUE);
	settingState->Options->RepositoryOptionsState->ShowOnlyMyArchivedDocs			= ((BOOL)pNewRecSettings->l_ShowOnlyMyArchivedDocs == TRUE);
	settingState->Options->RepositoryOptionsState->ExcludedExtensions				= (gcnew String(pNewRecSettings->l_ExcludedExtensions.GetString()))->ToLowerInvariant(); // metto tutto minuscolo
	settingState->Options->RepositoryOptionsState->DisableAttachFromReport			= ((BOOL)pNewRecSettings->l_DisableAttachFromReport == TRUE);
	settingState->Options->BarcodeDetectionOptionsState->EnableBarcode				= ((BOOL)pNewRecSettings->l_EnableBarcode == TRUE);
	settingState->Options->BarcodeDetectionOptionsState->AutomaticBarcodeDetection	= ((BOOL)pNewRecSettings->l_AutomaticBarcodeDetection == TRUE);
	settingState->Options->BarcodeDetectionOptionsState->BarcodeDetectionAction		= (BarcodeDetectionAction)GetBCDetectionAction(pNewRecSettings->l_BarcodeDetectionAction);
	settingState->Options->BarcodeDetectionOptionsState->BCActionForDocument		= (DuplicateDocumentAction)GetDuplicateAction(pNewRecSettings->l_BCActionForDocument);
	settingState->Options->BarcodeDetectionOptionsState->BCActionForBatch			= (DuplicateDocumentAction)GetDuplicateAction(pNewRecSettings->l_BCActionForBatch);
	settingState->Options->BarcodeDetectionOptionsState->BarcodeType				= (BarCodeType)GetBarcodeType(pNewRecSettings->l_BarcodeType);
	settingState->Options->BarcodeDetectionOptionsState->BarcodePrefix				= gcnew String(pNewRecSettings->l_BarcodePrefix.GetString());
	settingState->Options->BarcodeDetectionOptionsState->PrintBarcodeInReport		= ((BOOL)pNewRecSettings->l_PrintBarcodeInReport == TRUE);
	settingState->Options->FTSOptionsState->EnableFTS								= (m_pDMSOrchestrator->dmsOrchestrator->FullTextSearchEnabled) ? ((BOOL)pNewRecSettings->l_EnableFTS == TRUE) : FALSE;
	settingState->Options->FTSOptionsState->FTSNotConsiderPdF						= (settingState->Options->FTSOptionsState->EnableFTS && ((BOOL)pNewRecSettings->l_FTSNotConsiderPdF == TRUE));
	settingState->Options->SOSOptionsState->EnableSOS								= ((BOOL)pNewRecSettings->l_EnableSOS == TRUE);
	settingState->Options->SOSOptionsState->MaxElementsInEnvelope					= pNewRecSettings->l_MaxElementsInEnvelope;
	settingState->Options->StorageOptionsState->StorageToFileSystem					= ((BOOL)pNewRecSettings->l_StorageToFileSystem == TRUE);
	settingState->Options->StorageOptionsState->StorageFolderPath					= gcnew String(pNewRecSettings->l_StorageFolderPath.GetString());
	settingState->Options->AttachmentPanelOptionsState->MaxDocNumber				= pNewRecSettings->l_MaxDocNumber;

	// devo pulire la lista delle estensioni e riempirla nuovamente con quelle inserite dall'utente
	settingState->Options->RepositoryOptionsState->Extensions->Clear();

	VExtensionMaxSize* pRecExtension;
	for (int i = 0; i < m_pDMSSettings->GetExtensions()->GetCount(); i++)
	{
		pRecExtension = (VExtensionMaxSize*)m_pDMSSettings->GetExtensions()->GetAt(i);
		if (pRecExtension->l_Extension.IsEmpty() || pRecExtension->l_MaxSize < 1) //skippo righe non valide
			continue;

		String^ sExtension = gcnew String(pRecExtension->l_Extension.GetString());
		// faccio il Trim e se necessario aggiunto il . (cosi faccio un Compare con la property Extension del FileInfo, che comprende il punto)
		sExtension = sExtension->StartsWith(".") ? sExtension->Trim() : ("." + sExtension->Trim());

		ExtensionSize^ es = gcnew ExtensionSize();
		es->Name = sExtension->ToLowerInvariant(); // metto minuscolo
		es->Size = pRecExtension->l_MaxSize;
		settingState->Options->RepositoryOptionsState->Extensions->Add(es);
	}
	
	return m_pDMSOrchestrator->dmsOrchestrator->SettingsManager->SaveSettings();
}

// per impostare il nr max di documenti da caricare nel pannello allegati prima del refresh
//-----------------------------------------------------------------------------
void TbRepositoryManager::SetAttachmentPanelOptions(int nMaxDocNr) 
{
	SettingState^ settingState = m_pDMSOrchestrator->dmsOrchestrator->SettingsManager->UsersSettingState;
	settingState->Options->AttachmentPanelTempOptionsState->MaxDocNumber = nMaxDocNr;
}

// ritorna il record dalla tabella SOSConfiguration
//-----------------------------------------------------------------------------
CSOSConfiguration* TbRepositoryManager::GetSOSConfiguration()
{
	if (m_pSOSConfiguration)
		return m_pSOSConfiguration;

	// se il record nella tabella non esiste ne viene inserito uno d'ufficio con ParamID = 0
	SOSConfigurationState^ sosConfigurationState = m_pDMSOrchestrator->dmsOrchestrator->SOSConfigurationState;
	
	m_pSOSConfiguration = new CSOSConfiguration();

	m_pSOSConfiguration->m_pRecSOSConfiguration->l_ParamID				= sosConfigurationState->SOSConfiguration->ParamID;
	m_pSOSConfiguration->m_pRecSOSConfiguration->l_SubjectCode			= sosConfigurationState->SOSConfiguration->SubjectCode;
	m_pSOSConfiguration->m_pRecSOSConfiguration->l_AncestorCode			= sosConfigurationState->SOSConfiguration->AncestorCode;
	m_pSOSConfiguration->m_pRecSOSConfiguration->l_KeeperCode			= sosConfigurationState->SOSConfiguration->KeeperCode;
	m_pSOSConfiguration->m_pRecSOSConfiguration->l_MySOSUser			= sosConfigurationState->SOSConfiguration->MySOSUser;
	m_pSOSConfiguration->m_pRecSOSConfiguration->l_MySOSPassword		= Crypto::Decrypt(sosConfigurationState->SOSConfiguration->MySOSPassword);
	m_pSOSConfiguration->m_pRecSOSConfiguration->l_SOSWebServiceUrl		= sosConfigurationState->SOSConfiguration->SOSWebServiceUrl;
	m_pSOSConfiguration->m_pRecSOSConfiguration->l_ChunkDimension		= (int)sosConfigurationState->SOSConfiguration->ChunkDimension;
	m_pSOSConfiguration->m_pRecSOSConfiguration->l_EnvelopeDimension	= (int)sosConfigurationState->SOSConfiguration->EnvelopeDimension;
	m_pSOSConfiguration->m_pRecSOSConfiguration->l_FTPSend				= (sosConfigurationState->SOSConfiguration->FTPSend) ? (bool)sosConfigurationState->SOSConfiguration->FTPSend : FALSE;
	m_pSOSConfiguration->m_pRecSOSConfiguration->l_FTPSharedFolder		= sosConfigurationState->SOSConfiguration->FTPSharedFolder;
	m_pSOSConfiguration->m_pRecSOSConfiguration->l_FTPUpdateDayOfWeek	= (int)sosConfigurationState->SOSConfiguration->FTPUpdateDayOfWeek;
	
	VSOSDocClass* pRecDocClass;
	for each(DocClass^ dc in sosConfigurationState->DocumentClasses->DocClassesList)
	{
		pRecDocClass = new VSOSDocClass();
		pRecDocClass->l_ParamID		= sosConfigurationState->SOSConfiguration->ParamID;
		pRecDocClass->l_Code		= dc->Code->ToString();
		pRecDocClass->l_Description = dc->Description->ToString();
		pRecDocClass->docClass		= dc; // assegno il puntatore

		m_pSOSConfiguration->m_pSOSDocClassesArray->Add(pRecDocClass);
	}

	return m_pSOSConfiguration;
}

// riempie il SOSConfigurationState e demanda il salvataggio lato c#
//-----------------------------------------------------------------------------
BOOL TbRepositoryManager::SaveSOSConfiguration()
{
	if (!m_pSOSConfiguration)
		return FALSE;

	VSOSConfiguration* pNewRec = m_pSOSConfiguration->m_pRecSOSConfiguration;

	SOSConfigurationState^ sosConfigurationState = m_pDMSOrchestrator->dmsOrchestrator->SOSConfigurationState;

	sosConfigurationState->SOSConfiguration->ParamID			= m_pSOSConfiguration->m_pRecSOSConfiguration->l_ParamID;
	sosConfigurationState->SOSConfiguration->SubjectCode		= gcnew String(m_pSOSConfiguration->m_pRecSOSConfiguration->l_SubjectCode.GetString());
	sosConfigurationState->SOSConfiguration->AncestorCode		= gcnew String(m_pSOSConfiguration->m_pRecSOSConfiguration->l_AncestorCode.GetString());
	sosConfigurationState->SOSConfiguration->KeeperCode			= gcnew String(m_pSOSConfiguration->m_pRecSOSConfiguration->l_KeeperCode.GetString());
	sosConfigurationState->SOSConfiguration->MySOSUser			= gcnew String(m_pSOSConfiguration->m_pRecSOSConfiguration->l_MySOSUser.GetString());
	sosConfigurationState->SOSConfiguration->MySOSPassword		= Crypto::Encrypt(gcnew String(m_pSOSConfiguration->m_pRecSOSConfiguration->l_MySOSPassword.GetString()));
	sosConfigurationState->SOSConfiguration->SOSWebServiceUrl	= gcnew String(m_pSOSConfiguration->m_pRecSOSConfiguration->l_SOSWebServiceUrl.GetString());
	sosConfigurationState->SOSConfiguration->ChunkDimension		= (int)m_pSOSConfiguration->m_pRecSOSConfiguration->l_ChunkDimension;
	sosConfigurationState->SOSConfiguration->EnvelopeDimension	= (int)m_pSOSConfiguration->m_pRecSOSConfiguration->l_EnvelopeDimension;
	sosConfigurationState->SOSConfiguration->FTPSend			= ((BOOL)m_pSOSConfiguration->m_pRecSOSConfiguration->l_FTPSend == TRUE);
	sosConfigurationState->SOSConfiguration->FTPSharedFolder	= gcnew String(m_pSOSConfiguration->m_pRecSOSConfiguration->l_FTPSharedFolder.GetString());
	sosConfigurationState->SOSConfiguration->FTPUpdateDayOfWeek = (int)m_pSOSConfiguration->m_pRecSOSConfiguration->l_FTPUpdateDayOfWeek;

	// devo pulire la lista delle classi documentali e riempirla nuovamente con quelle caricate
	sosConfigurationState->DocumentClasses->DocClassesList->Clear();

	VSOSDocClass* pRecDocClass;
	for (int i = 0; i < m_pSOSConfiguration->m_pSOSDocClassesArray->GetCount(); i++)
	{
		pRecDocClass = (VSOSDocClass*)m_pSOSConfiguration->m_pSOSDocClassesArray->GetAt(i);
		if (pRecDocClass->l_Code.IsEmpty()) // non dovrebbe mai succedere
			continue;
		DocClass^ dClass	= gcnew DocClass();
		dClass->Code		= gcnew String(pRecDocClass->l_Code.GetString());
		dClass->Description = gcnew String(pRecDocClass->l_Description.GetString());
		sosConfigurationState->DocumentClasses->DocClassesList->Add(dClass);
	}

	BOOL result = m_pDMSOrchestrator->dmsOrchestrator->SosManager->SaveSOSConfiguration(sosConfigurationState);
	if (result)
	{
		VSOSDocClass* pRecDocClass;
		for (int i = 0; i < m_pSOSConfiguration->m_pSOSDocClassesArray->GetCount(); i++)
		{
			pRecDocClass = (VSOSDocClass*)m_pSOSConfiguration->m_pSOSDocClassesArray->GetAt(i);
			if (pRecDocClass->l_Code.IsEmpty()) // non dovrebbe mai succedere
				continue;
			// devo assegnare il valore all'oggetto docClass solo dopo il salvataggio
			// perche' e' in quel momento che vengono caricate informazioni aggiuntive per ogni classe documento
			pRecDocClass->docClass = m_pDMSOrchestrator->dmsOrchestrator->SOSConfigurationState->DocumentClasses->GetDocClass(gcnew String(pRecDocClass->l_Code.GetString()));
		}
	}

	return result;
}

// carica l'elenco delle classi documentali
//-----------------------------------------------------------------------------
BOOL TbRepositoryManager::LoadSOSDocumentClasses()
{
	if (!m_pSOSConfiguration)
		return FALSE;
	
	// ogni volta che carico le classi documentali devo ripulire l'array in memoria
	m_pSOSConfiguration->m_pSOSDocClassesArray->RemoveAll();

	List<ClasseDocumentale^>^ docClasseslist;

	BOOL bLoaded = m_pDMSOrchestrator->dmsOrchestrator->SosManager->ElencoClassiDocumentali
		(
			gcnew String(m_pSOSConfiguration->m_pRecSOSConfiguration->l_KeeperCode.GetString()),
			gcnew String(m_pSOSConfiguration->m_pRecSOSConfiguration->l_SubjectCode.GetString()),
			gcnew String(m_pSOSConfiguration->m_pRecSOSConfiguration->l_MySOSUser.GetString()),
			gcnew String(m_pSOSConfiguration->m_pRecSOSConfiguration->l_MySOSPassword.GetString()),
			gcnew String(m_pSOSConfiguration->m_pRecSOSConfiguration->l_SOSWebServiceUrl.GetString()),
			docClasseslist
		);

	if (bLoaded)
	{
		if (docClasseslist && docClasseslist->Count > 0)
		{
			VSOSDocClass* pRecDocClass;
			for each(ClasseDocumentale^ dc in docClasseslist)
			{
				pRecDocClass = new VSOSDocClass();
				pRecDocClass->l_ParamID		= m_pSOSConfiguration->m_pRecSOSConfiguration->l_ParamID;
				pRecDocClass->l_Code		= dc->CodiceClasseDocumentale->ToString();
				pRecDocClass->l_Description = dc->Descrizione->ToString();
				pRecDocClass->docClass		= m_pDMSOrchestrator->dmsOrchestrator->SOSConfigurationState->DocumentClasses->GetDocClass(dc->CodiceClasseDocumentale->ToString());
				m_pSOSConfiguration->m_pSOSDocClassesArray->Add(pRecDocClass);
			}
		}
	}

	return bLoaded;
}

//-----------------------------------------------------------------------------
void TbRepositoryManager::OpenQuickAttachForm()
{
	if (m_pDMSOrchestrator)
		m_pDMSOrchestrator->dmsOrchestrator->OpenQuickAttachForm((IntPtr)(int)AfxGetMenuWindowHandle());
}

//-----------------------------------------------------------------------------
void TbRepositoryManager::OpenBBTraylet()
{
	if (m_pDMSOrchestrator)
		m_pDMSOrchestrator->dmsOrchestrator->OpenBBTrayletForm((IntPtr)(int)AfxGetMenuWindowHandle());
}

//-----------------------------------------------------------------------------
void TbRepositoryManager::OpenBBSettings()
{
	if (m_pDMSOrchestrator)
		m_pDMSOrchestrator->dmsOrchestrator->OpenBBSettings((IntPtr)(int)AfxGetMenuWindowHandle());
}

//-----------------------------------------------------------------------------
bool TbRepositoryManager::ArchiveFolder(CString folderName)
{
	if (!AfxGetOleDbMng()->EasyAttachmentEnable())
		return FALSE;

	CFileFind finder;  
	BOOL bWorking =  finder.FindFile(folderName + URL_SLASH_CHAR + szAllFiles);
	bool archiveResult = true;
	
	//questa chiamata è effettuata via webmethod. Per renderla autonoma (vedi problematica stato unattended mode per messaggi) istanzio un nuovo dmsOrchestrator che poi verrà disposato
	DMSOrchestrator^ dmsOrch = gcnew DMSOrchestrator();
	dmsOrch->InitializeManager(m_pDMSOrchestrator->dmsOrchestrator);
	dmsOrch->InUnattendedMode = true;

	while (bWorking)
	{     
		bWorking = finder.FindNextFile();
		// evito "." e ".." per evitare ricorsione
		if (finder.IsDots() || finder.IsDirectory())
			continue;
		if ((::ArchiveResult)dmsOrch->ArchiveFile(gcnew String(finder.GetFilePath()), gcnew String(finder.GetFileTitle()), false, false, gcnew String("")) == TerminatedWithError)
			archiveResult = false;
	}
	finder.Close ();

	delete dmsOrch;
	return 	archiveResult;
}

//-----------------------------------------------------------------------------
::ArchiveResult TbRepositoryManager::ArchiveBinaryContent(DataBlob binaryContent, DataStr sourceFileName, DataStr description, DataLng& archivedDocID, DataStr& messages, CString barcode /*= _T("")*/)
{
	DataSize pDataSize;

	System::IntPtr intPointer = (System::IntPtr)((void*)binaryContent.GetRawData(&pDataSize));
	String^ msg = gcnew String("");
	int archDocId = -1;
	DMSOrchestrator^ dmsOrch = gcnew DMSOrchestrator();
	dmsOrch->InitializeManager(m_pDMSOrchestrator->dmsOrchestrator);
	dmsOrch->InUnattendedMode = true;

	::ArchiveResult result = (::ArchiveResult)(int)dmsOrch->ArchiveBinaryContent(intPointer, (int)pDataSize, gcnew String(sourceFileName.GetString()), gcnew String(description.GetString()), false, false, gcnew String(barcode.GetString()), archDocId, msg);
	archivedDocID.Assign(archDocId);
	messages.Assign(CString(msg));
	delete dmsOrch;
	return result;
}

//-----------------------------------------------------------------------------
CString TbRepositoryManager::MassiveArchiveUnattendedMode(CString folder, bool splitFile)
{
	if (!AfxGetOleDbMng()->EasyAttachmentEnable())
		return _T("");

	//questa chiamata è effettuata via webmethod. Per renderla autonoma (vedi problematica stato unattended mode per messaggi) istanzio un nuovo dmsOrchestrator  che poi verrà disposato
	DMSOrchestrator^ dmsOrch = gcnew DMSOrchestrator();
	dmsOrch->InitializeManager(m_pDMSOrchestrator->dmsOrchestrator);
	dmsOrch->InUnattendedMode = true;
	Microarea::EasyAttachment::Components::MassiveAttachInfo^  massiveAttachInfo = dmsOrch->MassiveAttachUnattendedMode(gcnew String(folder), splitFile);
	String^ mRes = massiveAttachInfo->Serialize();
	CString result = CString(mRes);
	delete dmsOrch;
	return result;
}

//-----------------------------------------------------------------------------
void TbRepositoryManager::InitializeManager(BDSOSDocSender* pBDSOSDocSender)
{
	if (m_pDMSOrchestrator)
		pBDSOSDocSender->dmsOrchestrator->InitializeManager(m_pDMSOrchestrator->dmsOrchestrator);
}

//-----------------------------------------------------------------------------
void TbRepositoryManager::InitializeManager(BDSOSAdjustAttachments* pBDSOSAdjustAttachments)
{
	if (m_pDMSOrchestrator)
		pBDSOSAdjustAttachments->dmsOrchestrator->InitializeManager(m_pDMSOrchestrator->dmsOrchestrator);
}

//-----------------------------------------------------------------------------
void TbRepositoryManager::InitializeManager(BDDMSRepository* pBDDMSRepository)
{
	if (m_pDMSOrchestrator)
		pBDDMSRepository->dmsOrchestrator->InitializeManager(m_pDMSOrchestrator->dmsOrchestrator);
}

//-----------------------------------------------------------------------------
void TbRepositoryManager::InitializeManager(BDMassiveArchive* pBDMassiveArchive)
{
	if (m_pDMSOrchestrator)
		pBDMassiveArchive->dmsOrchestrator->InitializeManager(m_pDMSOrchestrator->dmsOrchestrator);
}

//-----------------------------------------------------------------------------
void TbRepositoryManager::InitializeManager(CDDMS* pCDDMS)
{
	if (m_pDMSOrchestrator)		
		//il setting manager deve essere quello globale instanziato dal dmsOrchestrator di TbRepositoryManager
		pCDDMS->dmsDocOrchestrator->InitializeManager(m_pDMSOrchestrator->dmsOrchestrator);
}

//-----------------------------------------------------------------------------
DMSStatusEnum TbRepositoryManager::CheckDMSStatus(int dbRelease, CString& strMsg)
{
	Thread::CurrentThread->CurrentUICulture = gcnew CultureInfo(gcnew String(AfxGetCulture()));
	String^ msg = gcnew String("");
	DMSStatusEnum status = (DMSStatusEnum)DMSChecker::CheckDMSStatus(gcnew String(AfxGetOleDbMng()->GetDMSConnectionString()), dbRelease, (Int32)AfxGetWorkerId(), msg);
	strMsg = msg;
	return status;
}

//-----------------------------------------------------------------------------
bool TbRepositoryManager::ExistEmptyTBGuidValuesInERPDocument()
{
	return DMSChecker::ExistEmptyTBGuidValuesInERPDocument(gcnew String(AfxGetOleDbMng()->GetDMSConnectionString()));
}

// metodo per aggiornare il valore della colonna TBGuid nella DMS_ErpDocument
// leggendo dalle tabelle master di ERP che contengono degli allegati
//-----------------------------------------------------------------------------
bool TbRepositoryManager::UpdateTBGuidInERPDocument()
{
	DMSOrchestrator^ dmsOrch = gcnew DMSOrchestrator();
	if (dmsOrch == nullptr)
		return false;

	dmsOrch->InitializeManager(m_pDMSOrchestrator->dmsOrchestrator);
	dmsOrch->InUnattendedMode = true;
	return dmsOrch->UpdateTBGuidInERPDocument();
}

//-----------------------------------------------------------------------------
bool TbRepositoryManager::GetDefaultBarcodeType(DataStr& type, DataStr& prefix)
{
	if (!m_pDMSOrchestrator)
		return false;
		
	String^ temptype = gcnew String("");
	String^ tempprefix = gcnew String("");

	bool ok = m_pDMSOrchestrator->dmsOrchestrator->GetDefaultBarcodeType(temptype, tempprefix);
	type.Assign(CString(temptype));
	prefix.Assign(CString(tempprefix));
	return ok;
}

//-----------------------------------------------------------------------------
CTypedBarcode TbRepositoryManager::GetNewBarcodeValue()
{
	if (!m_pDMSOrchestrator)
		return CTypedBarcode();

	Microarea::EasyAttachment::BusinessLogic::TypedBarcode^ tb = (m_pDMSOrchestrator->dmsOrchestrator->CreateRandomBarcodeValue());
	CTypedBarcode ctb(tb->Value, tb->TypeDescription);
	return ctb;
}

// esegue un check sul valore del barcode inserito dall'utente in base ai parametri
// e mostra o meno un msg a seconda del secondo parametro
//-----------------------------------------------------------------------------
bool TbRepositoryManager::IsValidEABarcodeValue(CString sBarcode, BOOL bShowMessage /*= TRUE*/)
{
	if (!m_pDMSOrchestrator)
		return FALSE;

	bool bResult = m_pDMSOrchestrator->dmsOrchestrator->IsValidEABarcodeValue(gcnew String(sBarcode));

	if (!bResult && bShowMessage)
		AfxMessageBox(cwsprintf(_TB("The barcode value you have specified is not valid.\r\nThis value must be start with prefix {0-%s} and its total length must not exceed 17 uppercase characters."),
			GetDMSSettings()->GetSettings()->l_BarcodePrefix.GetString()));

	return bResult;
}

//-----------------------------------------------------------------------------
CString TbRepositoryManager::GetEasyAttachmentTempPath()
{
	CString sPath = AfxGetPathFinder()->GetAppDataPath() + SLASH_CHAR + _T("DMS") + SLASH_CHAR + AfxGetLoginInfos()->m_strCompanyName;
	if (!ExistPath(sPath))
		RecursiveCreateFolders(sPath);
	return sPath;	
}

//-----------------------------------------------------------------------------
bool TbRepositoryManager::SearchInContentEnabled()
{
	return m_pDMSOrchestrator &&  m_pDMSOrchestrator->dmsOrchestrator->SearchInContentEnabled;
}

//-----------------------------------------------------------------------------
DMSAttachmentsList* TbRepositoryManager::GetArchivedDocuments(CSearchFilter* pSearchFilter /*=NULL*/)
{
	ArchivedDocDataTable^ archivedDocsDT;

	if (m_pDMSOrchestrator)
	{
		if (!pSearchFilter)
			archivedDocsDT = m_pDMSOrchestrator->dmsOrchestrator->GetArchivedDocuments();
		else
		{
			Microarea::EasyAttachment::Components::FilterEventArgs^ filterEventArgs = gcnew Microarea::EasyAttachment::Components::FilterEventArgs();
			// filtro date
			System::DateTime startDate(pSearchFilter->m_StartDate.Year(), pSearchFilter->m_StartDate.Month(), pSearchFilter->m_StartDate.Day(), 0, 0, 0);
			System::DateTime endDate(pSearchFilter->m_EndDate.Year(), pSearchFilter->m_EndDate.Month(), pSearchFilter->m_EndDate.Day(), 23, 59, 59);
			filterEventArgs->StartDate = startDate;
			filterEventArgs->EndDate = endDate;
			// filtro nr documenti
			filterEventArgs->TopDocsNumber = pSearchFilter->m_TopDocsNumber;
			// filtro freetag
			filterEventArgs->FreeTag = gcnew String(pSearchFilter->m_FreeTag.GetString());
			filterEventArgs->SearchLocation = (SearchLocation)pSearchFilter->m_wSearchLocation;
			// filtro sulla collection
			filterEventArgs->CollectionID = (int)pSearchFilter->m_CollectionID;
			// filtri estensione file (se ho scelto All o empty imposto il valore corretto che si aspetta il codice C#)
			if (pSearchFilter->m_DocExtensionType.CompareNoCase(szAllFiles) == 0 || pSearchFilter->m_DocExtensionType.IsEmpty())
				filterEventArgs->DocExtensionType = gcnew String(szAllFiles);
			else
				filterEventArgs->DocExtensionType = gcnew String(pSearchFilter->m_DocExtensionType.GetString());

			// filtri worker
			if (pSearchFilter->m_arWorkers.IsEmpty())
			{
				if (filterEventArgs->Workers)
					filterEventArgs->Workers->Clear();
			}
			else
			{
				CString aWorkerId;
				for (int i = 0; i < pSearchFilter->m_arWorkers.GetCount(); i++)
				{
					aWorkerId = pSearchFilter->m_arWorkers.GetAt(i);
					if (!filterEventArgs->Workers)
						filterEventArgs->Workers = gcnew List<int>();
					filterEventArgs->Workers->Add(_tstoi(aWorkerId));
				}
			}

			// filtri sui bookmark
			filterEventArgs->SearchFieldsConditionsDT = gcnew SearchFieldsConditionsDataTable();
			if (pSearchFilter->m_pDBTSearchFieldsConditions)
			{
				VSearchFieldCondition* pSearchCondition = NULL;
				for (int i = 0; i < pSearchFilter->m_pDBTSearchFieldsConditions->GetSize(); i++)
				{
					pSearchCondition = pSearchFilter->m_pDBTSearchFieldsConditions->GetSearchFieldConditionAt(i);
					if (!pSearchCondition || !pSearchCondition->IsValidCondition()) 
						continue;
								
					filterEventArgs->SearchFieldsConditionsDT->AddSearchFilterCondition
					(
						gcnew String(pSearchCondition->l_FieldName.GetString()),
						(int)pSearchCondition->l_SearchFieldID
					);
				}
			}

			archivedDocsDT = m_pDMSOrchestrator->dmsOrchestrator->GetArchivedDocuments(filterEventArgs);
		}
	}

	DMSAttachmentsList* archivedDocs = new DMSAttachmentsList();
	DMSAttachmentInfo* archivedDoc;

	if (archivedDocsDT != nullptr)
	{
		for each (DataRow^ row in archivedDocsDT->Rows)
		{
			archivedDoc = new DMSAttachmentInfo();

			archivedDoc->m_ArchivedDocId = DataLng((int)row[CommonStrings::ArchivedDocID]);
			archivedDoc->m_Name = row[CommonStrings::Name]->ToString();
			archivedDoc->m_Description = row[CommonStrings::Description]->ToString();

			if (row[CommonStrings::TBCreated] != DBNull::Value)
			{
				DateTime tbCreated = Convert::ToDateTime(row[CommonStrings::TBCreated]);
				DataDate convertedDate;
				convertedDate.SetFullDate();
				convertedDate.SetDate(tbCreated.Day, tbCreated.Month, tbCreated.Year);
				convertedDate.SetTime(tbCreated.Hour, tbCreated.Minute, tbCreated.Second);
				archivedDoc->m_ArchivedDate = convertedDate;
			}

			if (row[CommonStrings::TBModified] != DBNull::Value)
			{
				DateTime tbModified = Convert::ToDateTime(row[CommonStrings::TBModified]);
				DataDate convertedDate;
				convertedDate.SetFullDate();
				convertedDate.SetDate(tbModified.Day, tbModified.Month, tbModified.Year);
				convertedDate.SetTime(tbModified.Hour, tbModified.Minute, tbModified.Second);
				archivedDoc->m_ModifiedDate = convertedDate;
			}

			// valorizzato solo se un worker ha in out il file (altrimenti = NULL)
			archivedDoc->m_CurrentCheckOutWorker = (row[CommonStrings::ModifierID] != DBNull::Value) ? row[CommonStrings::ModifierID]->ToString() : _T("");

			archivedDoc->m_ModifiedBy = row[CommonStrings::WorkerName]->ToString();
			archivedDoc->m_IsWoormReport = DataBool((bool)row[CommonStrings::IsWoormReport]);

			bool attached = (bool)row[CommonStrings::Attached];
			archivedDoc->m_attachmentID = (attached) ? DataLng(1) : DataLng(-1);

			archivedDocs->Add(archivedDoc);
		}
	}

	return archivedDocs;
}

//-----------------------------------------------------------------------------
void TbRepositoryManager::OpenDocument(const CString& strFilePath)
{
	CString errorMsg;
	BOOL bShowMessage = FALSE;

	try
	{
		ProcessStartInfo^ psi = gcnew ProcessStartInfo(gcnew String(strFilePath));
		psi->UseShellExecute = true;
		psi->WindowStyle = ProcessWindowStyle::Normal;
		Process::Start(psi);
	}
	catch (System::IO::FileNotFoundException^ fe)
	{
		errorMsg = CString(fe->Message);
		bShowMessage = TRUE;
	}
	catch (System::ComponentModel::Win32Exception^ we)
	{
		errorMsg = CString(we->Message);
		bShowMessage = TRUE;
	}
	catch (System::Exception^ e)
	{
		errorMsg = CString(e->Message);
		bShowMessage = TRUE;
	}

	if (bShowMessage)
		AfxMessageBox(cwsprintf(_TB("Unable to open current file ({0-%s})."), errorMsg));
}

//metodo che permette di ricercare attachment di uno specifico documento con i seguenti criteri
	//- eventuale testo da ricercare
	//- dove effettuare la ricerca del testo
	//- lista degli eventuali chiavi di ricerca da utilizzare (formato: "bookmark1:valore1;bookmark2:valore2;bookmark3:valore4;"
//-----------------------------------------------------------------------------
CAttachmentsArray* TbRepositoryManager::SearchAttachmentsForDocument(CString docNamespace, CString docKey, CString searchText, SearchLocationEnum location, CString searchFields)
{
	CAttachmentsArray* attachments = new CAttachmentsArray();
	
	if (!m_pDMSOrchestrator)
		return attachments;

	List<Microarea::EasyAttachment::Components::AttachmentInfo^>^ attachmentList = m_pDMSOrchestrator->dmsOrchestrator->SearchAttachmentsForDocument
		(
		gcnew String(docNamespace), 
		gcnew String(docKey), 
		gcnew String(searchText), 
		(Microarea::EasyAttachment::Components::SearchLocation)location, 
		gcnew String(searchFields)
		);
	
	for each (AttachmentInfo^ attachment in attachmentList)
		attachments->Add(CreateCAttachmentInfo(attachment));

	return attachments;
}

// dato un collector ed una colletion, effettua la ricerca degli attachment  che verificano i seguenti criteri:
// appartengono al collector il cui nome è collectorName
// appartengono alla collection il cui nome è collectionName
//-----------------------------------------------------------------------------
CAttachmentsArray* TbRepositoryManager::SearchAttachments
		(
		DataStr collectorName, 
		DataStr collectionName, 
		DataStr extType, 
		DataDate fromDate, 
		DataDate toDate, 
		DataStr searchText, 
		int location, 
		DataStr searchFields
		)
{
	CAttachmentsArray* attachments = new CAttachmentsArray();
	
	if (!m_pDMSOrchestrator)
		return attachments;

	//conversione date
	/*System::DateTime^ startDate = gcnew System::DateTime(fromDate.Day(), fromDate.Month(), fromDate.Year(), 0, 0, 0);
	System::DateTime^ endDate = gcnew System::DateTime(toDate.Day(), toDate.Month(), toDate.Year(), 23, 59, 59);	*/
	System::DateTime startDate(fromDate.Year(), fromDate.Month(), fromDate.Day(), 0, 0, 0);
	System::DateTime endDate(toDate.Year(), toDate.Month(), toDate.Day(), 23, 59, 59);	

	List<Microarea::EasyAttachment::Components::AttachmentInfo^>^  attachmentList = 
	m_pDMSOrchestrator->dmsOrchestrator->SearchAttachments
	(
	gcnew String(collectorName.GetString()), 
	gcnew String(collectionName.GetString()),
	gcnew String(extType.GetString()),
	startDate, 
	endDate,  
	gcnew String(searchText.GetString()),
	(Microarea::EasyAttachment::Components::SearchLocation)location, 
	gcnew String(searchFields.GetString())
	);

	for each (AttachmentInfo^ attachment in attachmentList)
		attachments->Add(CreateCAttachmentInfo(attachment));

	return attachments;
}

//-----------------------------------------------------------------------------
DMSCollectionList* TbRepositoryManager::GetUsedCollections()
{
	DMSCollectionList* pCollectionList = new DMSCollectionList();
	if (!m_pDMSOrchestrator)
		return pCollectionList;
	CollectionResultDataTable^ collectionDT = m_pDMSOrchestrator->dmsOrchestrator->GetUsedCollections();
	DMSCollectionInfo* collectionInfo = NULL;
	CTBNamespace docNamespace;
	for each(DataRow^ row in collectionDT->Rows)
	{
		collectionInfo = new DMSCollectionInfo();
		collectionInfo->m_CollectorID = DataLng((int)row[CommonStrings::CollectorID]);
		collectionInfo->m_CollectionID = DataLng((int)row[CommonStrings::CollectionID]);
		collectionInfo->m_Name = row[CommonStrings::Name]->ToString();
		collectionInfo->m_IsStandard = DataBool((bool)row[CommonStrings::IsStandard]);
		collectionInfo->m_DocNamespace = row[CommonStrings::DocNamespace]->ToString();
		docNamespace.SetNamespace(collectionInfo->m_DocNamespace);
		const CDocumentDescription* pDocDescri = docNamespace.IsValid() ? AfxGetDocumentDescription(docNamespace) : NULL;
		collectionInfo->m_DocTitle = (pDocDescri) ? pDocDescri->GetTitle() : (docNamespace.IsValid()) ? docNamespace.GetObjectName() : collectionInfo->m_DocNamespace;
		pCollectionList->Add(collectionInfo);
	}
	return pCollectionList;
}

//-----------------------------------------------------------------------------
CStringArray* TbRepositoryManager::GetAllExtensions()
{
	CStringArray* pAllExtensions = new CStringArray();
	if (m_pDMSOrchestrator)
	{
		List<String^>^ allExtensions = m_pDMSOrchestrator->dmsOrchestrator->GetAllExtensions();
		for each(String^ ext in allExtensions)
			pAllExtensions->Add(ext);
	}
	return pAllExtensions;
}

//-----------------------------------------------------------------------------
CDMSCategories* TbRepositoryManager::GetCategories()
{
	CDMSCategories* pCategoryList = new CDMSCategories();
	if (m_pDMSOrchestrator)
	{
		DTCategories^ categories = m_pDMSOrchestrator->dmsOrchestrator->GetCategories();
		for each(DataRow^ cat in categories->Rows)
		{
			CDMSCategory* dmsCat = new CDMSCategory(cat[CommonStrings::Name]->ToString());
			dmsCat->m_Description.Assign((CString)cat[CommonStrings::FieldDescription]->ToString());
			dmsCat->m_DefaultValue.Assign((CString)cat[CommonStrings::Value]->ToString());
			dmsCat->m_bDisabled.Assign((bool)cat[CommonStrings::Disable]);
			//dmsCat->m_Color = (DWORD)(int)cat[CommonStrings::Color];
			DTCategoriesValues^ catValues = (DTCategoriesValues^)(cat[CommonStrings::ValueSet]);
			for each(DataRow^ val in catValues->Rows)
				dmsCat->AddValue(val[CommonStrings::Value]->ToString(), (bool)val[CommonStrings::IsDefault]);

			pCategoryList->Add(dmsCat);
		}
	}

	return pCategoryList;
}

//-----------------------------------------------------------------------------
CDMSCategory* TbRepositoryManager::GetDMSCategory(const CString& name) const
{
	CDMSCategory* dmsCat = NULL;
	if (m_pDMSOrchestrator)
	{
		CategoryInfo^ categoryInfo = m_pDMSOrchestrator->dmsOrchestrator->GetCategory(gcnew String(name));
		{
			if (categoryInfo != nullptr)
			{
				dmsCat = new CDMSCategory(categoryInfo->Name);
				dmsCat->m_Description = CString(categoryInfo->Description);
				dmsCat->m_bDisabled.Assign(categoryInfo->Disabled);
				dmsCat->m_bInUse.Assign(categoryInfo->InUse);
				//color
				for each(DataRow^ row in categoryInfo->CategoriesValuesDataTable->Rows)
					dmsCat->AddValue(row[CommonStrings::Value]->ToString(), (bool)row[CommonStrings::IsDefault]);				
			}
		}
	}
	return dmsCat;
}

//-----------------------------------------------------------------------------
bool TbRepositoryManager::DeleteDMSCategory(const CString& name) const
{
	return m_pDMSOrchestrator->dmsOrchestrator->DeleteCategory(gcnew String(name));
}

//-----------------------------------------------------------------------------
bool TbRepositoryManager::SaveDMSCategory(CDMSCategory* pCategory) const
{
	if (m_pDMSOrchestrator && pCategory)
	{
		CategoryInfo^ categoryInfo = gcnew CategoryInfo();
		categoryInfo->Name = gcnew String(pCategory->m_Name.GetString());
		categoryInfo->Description = gcnew String(pCategory->m_Description.GetString());
		categoryInfo->Disabled = ((BOOL)pCategory->m_bDisabled == TRUE);
		
		for (int i = 0; i < pCategory->m_arCategoryValues.GetSize(); i++)
		{
			VCategoryValues* pNewRec = pCategory->GetValueAt(i);
			DataRow^ newValue = categoryInfo->CategoriesValuesDataTable->NewRow();
			newValue[CommonStrings::Value] = gcnew String(pNewRec->l_Value.GetString());
			newValue[CommonStrings::IsDefault] = (pNewRec->l_Value.GetString().CompareNoCase(pCategory->m_DefaultValue.GetString()) == 0);
			try
			{
				categoryInfo->CategoriesValuesDataTable->Rows->Add(newValue);
			}
			catch (System::Data::ConstraintException^ ex)
			{
				AfxMessageBox(cwsprintf(_TB("One or more values are duplicated. Unable to save category. ({0-%s})"), CString(ex->Message)));
				return false;
			}
		}

		return m_pDMSOrchestrator->dmsOrchestrator->SaveCategoryInfo(categoryInfo);
	}

	return true;
}

// dato un archiveDocID ritorna il path del file temporaneo salvato su file system
//-----------------------------------------------------------------------------
CString TbRepositoryManager::GetArchivedDocTempFile(int archivedDocID)
{
	if (m_pDMSOrchestrator)
		return m_pDMSOrchestrator->dmsOrchestrator->GetArchivedDocTempFile(archivedDocID);

	return _T("");
}

// dato un archiveDocID ritorna un oggetto di tipo DMSAttachmentInfo
//-----------------------------------------------------------------------------
DMSAttachmentInfo* TbRepositoryManager::GetAttachmentInfoFromArchivedDocId(int archivedDocId)
{
	DMSAttachmentInfo* attInfo = NULL;
	if (m_pDMSOrchestrator)
	{
		Microarea::EasyAttachment::Components::AttachmentInfo^ attachment = m_pDMSOrchestrator->dmsOrchestrator->GetCompletedAttachmentInfoFromArchivedDocId(archivedDocId);
		if (attachment != nullptr)
			attInfo = CreateDMSAttachmentInfo(attachment, TRUE);
	}

	return attInfo;
}

//-----------------------------------------------------------------------------
int TbRepositoryManager::GetAttachmentIDByFileName(CString documentNamespace, CString documentKey, CString fileName)
{
	return (m_pDMSOrchestrator) ? m_pDMSOrchestrator->dmsOrchestrator->GetAttachmentIDByFileName(gcnew String(documentNamespace), gcnew String(documentKey), gcnew String(fileName)) : -1;
}

//-----------------------------------------------------------------------------
bool TbRepositoryManager::UpdateArchivedDoc(DMSAttachmentInfo* pAttachmentInfo, CString newDescription, CString newFreeTag, CString newBarcode)
{
	if (m_pDMSOrchestrator && pAttachmentInfo && pAttachmentInfo)
	{
		//il metodo UpdateAttachment controllo se è necessario o meno effettuare le modifiche andando a confrontare con lo stato precedente 
		return m_pDMSOrchestrator->dmsOrchestrator->UpdateArchivedDoc((AttachmentInfo^)pAttachmentInfo->attachmentInfo, gcnew String(newDescription), gcnew String(newFreeTag), gcnew String(newBarcode));
	}
	return false;
}

// dato un archiveDocID ritorna un lista degli allegati ad esso correlati
//-----------------------------------------------------------------------------
::Array* TbRepositoryManager::GetERPDocumentAttachment(int archivedDocId)
{
	::Array* arAttachmentLinks = new ::Array();

	if (!m_pDMSOrchestrator)
		return arAttachmentLinks;

	SearchResultDataTable^ dtSearchResult = m_pDMSOrchestrator->dmsOrchestrator->GetERPDocumentAttachment(archivedDocId);

	VAttachmentLink* pAttLink;
	for each (DataRow^ row in dtSearchResult->Rows)
	{
		pAttLink = new VAttachmentLink();
		pAttLink->l_Image				= TBIcon(szIconDocument, TOOLBAR);
		pAttLink->l_ArchivedDocId		= DataLng((int)row[CommonStrings::ArchivedDocID]);
		pAttLink->l_AttachmentId		= DataLng((int)row[CommonStrings::AttachmentID]);
		pAttLink->l_DocKeyDescription	= row[CommonStrings::DocKeyDescription]->ToString();
		pAttLink->l_TBDocNamespace		= row[CommonStrings::DocNamespace]->ToString();
		pAttLink->l_TBPrimaryKey		= row[CommonStrings::TBPrimaryKey]->ToString();
		pAttLink->l_DocKeyDescription = pAttLink->l_DocKeyDescription.IsEmpty() ? pAttLink->l_TBPrimaryKey : pAttLink->l_DocKeyDescription;
		String^ documentTitle = CUtility::GetDocumentTitle(gcnew String(pAttLink->l_TBDocNamespace.GetString()));
		// compongo la stringa con titolo documento e descrizione chiavi da visualizzare nel bodyedit su 2 righe
		pAttLink->l_DocumentDescription = documentTitle->ToString() + _T("\r\n") + pAttLink->l_DocKeyDescription;

		arAttachmentLinks->Add(pAttLink);
	}

	return arAttachmentLinks;
}

// elimina il documento archiviato e/o i documenti allegati (dipende dai parametri)
//-----------------------------------------------------------------------------
bool TbRepositoryManager::DeleteArchiveDocInCascade(DMSAttachmentInfo* pAttachmentInfo)
{
	return (m_pDMSOrchestrator) ? m_pDMSOrchestrator->dmsOrchestrator->DeleteArchiveDocInCascade((AttachmentInfo^)pAttachmentInfo->attachmentInfo) : false;
}

// esegue il check-in
//-----------------------------------------------------------------------------
bool TbRepositoryManager::CheckIn(DMSAttachmentInfo* pAttachmentInfo)
{
	if (m_pDMSOrchestrator)
	{
		if (m_pDMSOrchestrator->dmsOrchestrator->CheckIn(((AttachmentInfo^)pAttachmentInfo->attachmentInfo)))
		{
			// devo pulire l'id del worker che ha fatto check in del file
			pAttachmentInfo->m_CurrentCheckOutWorker = DataStr((CString)(pAttachmentInfo->attachmentInfo->ModifierID.ToString()));
			return true;
		}
	}

	return false;
}

// esegue il check-out
//-----------------------------------------------------------------------------
bool TbRepositoryManager::CheckOut(DMSAttachmentInfo* pAttachmentInfo)
{
	if (m_pDMSOrchestrator)
	{
		if (m_pDMSOrchestrator->dmsOrchestrator->CheckOut(((AttachmentInfo^)pAttachmentInfo->attachmentInfo)))
		{
			// devo assegnare l'id del worker che ha preso in out il file
			pAttachmentInfo->m_CurrentCheckOutWorker = DataStr((CString)(pAttachmentInfo->attachmentInfo->ModifierID.ToString()));

			// se non esiste il file temporaneo lo creo al volo	e visualizzo il documento che ho preso in out
			if (pAttachmentInfo->m_TemporaryPathFile.IsEmpty())
				pAttachmentInfo->m_TemporaryPathFile = GetArchivedDocTempFile(pAttachmentInfo->m_ArchivedDocId);
					OpenDocument(pAttachmentInfo->m_TemporaryPathFile.GetString());

			return true;
		}
	}

	return false;
}

// esegue l'undo del check-out
//-----------------------------------------------------------------------------
bool TbRepositoryManager::Undo(DMSAttachmentInfo* pAttachmentInfo)
{
	if (m_pDMSOrchestrator)
	{
		if (m_pDMSOrchestrator->dmsOrchestrator->Undo(((AttachmentInfo^)pAttachmentInfo->attachmentInfo)))
		{
			// devo azzerare l'id del worker che aveva in out il file
			pAttachmentInfo->m_CurrentCheckOutWorker = DataStr((CString)(pAttachmentInfo->attachmentInfo->ModifierID.ToString()));
			return true;
		}
	}

	return false;
}

// invio documento/i via email
//-----------------------------------------------------------------------------
void TbRepositoryManager::SendAsAttachments(DMSAttachmentsList* pAttachmentsList)
{
	List<String^>^ files = gcnew List<String^>();
	List<String^>^ titles = gcnew List<String^>();
	String^ errors = gcnew String("");

	DMSAttachmentInfo* pAttachmentInfo;

	for (int i = 0; i < pAttachmentsList->GetCount(); i++)
	{
		pAttachmentInfo = pAttachmentsList->GetAt(i);

		if (((AttachmentInfo^)pAttachmentInfo->attachmentInfo)->SaveAttachmentFile())
		{
			files->Add(pAttachmentInfo->attachmentInfo->TempPath);
			titles->Add(pAttachmentInfo->attachmentInfo->Name);
		}
	}

	CUtility::SendAsAttachments(files, titles, errors);
}

// salva una copia del documento archiviato in folder specificato dall'utente
//-----------------------------------------------------------------------------
void TbRepositoryManager::SaveArchiveDocFileInFolder(int archivedDocID)
{
	if (m_pDMSOrchestrator)
		m_pDMSOrchestrator->dmsOrchestrator->SaveArchiveDocFileInFolder(archivedDocID);
}

// selezione multipla di documenti archiviati in folder specificato dall'utente
//-----------------------------------------------------------------------------
void TbRepositoryManager::SaveMultipleArchiveDocFileInFolder(CUIntArray* pArchivedDocIds)
{
	List<int>^ archiveDocIdsList = gcnew List<int>();
	
	for (int i = 0; i < pArchivedDocIds->GetCount(); i++)
		archiveDocIdsList->Add(pArchivedDocIds->GetAt(i));

	if (m_pDMSOrchestrator)
		m_pDMSOrchestrator->dmsOrchestrator->SaveMultipleArchiveDocFileInFolder(archiveDocIdsList);
}

//-----------------------------------------------------------------------------
CString TbRepositoryManager::SaveAttachmentFileInFolder(int attachmentID, const CString& strFolder)
{
	return (m_pDMSOrchestrator) ? m_pDMSOrchestrator->dmsOrchestrator->SaveAttachmentFileInFolder(attachmentID, gcnew String(strFolder)) : _T(""); 	
}

//-----------------------------------------------------------------------------
CString TbRepositoryManager::GetDocumentKeyDescription(int attachmentID)
{
	return (m_pDMSOrchestrator) ? CString(m_pDMSOrchestrator->dmsOrchestrator->GetDocumentKeyDescription(attachmentID)) : _T(""); 	
}

//-----------------------------------------------------------------------------
void TbRepositoryManager::MultipleScanWithBarcode(CString sFileName, CString sExtension, CStringArray* pAcquiredFiles)
{
	if (!m_pDMSOrchestrator)
		return;

	List<String^>^ documentList = m_pDMSOrchestrator->dmsOrchestrator->MultipleScanWithBarcode(gcnew String(sFileName), gcnew String(sExtension));
	if (documentList)
		for each (String^ doc in documentList)
			pAcquiredFiles->Add(doc);
}

//-----------------------------------------------------------------------------
bool TbRepositoryManager::IsDocumentNamespaceInSOS(CString docNamespace)
{
	return m_pDMSOrchestrator && m_pDMSOrchestrator->dmsOrchestrator->IsDocumentNamespaceInSos(gcnew String(docNamespace));
}

//------------------------------------------------------------------------------------------------------------------------
void TbRepositoryManager::AddERPFilter(CBaseDocument* pDocument, const CString& name, DataObj* pFromData, DataObj* pToData)
{
	if (!pDocument)
		return;

	if (pDocument->IsKindOf(RUNTIME_CLASS(BDSOSDocSender)))
	{
		BDSOSDocSender* pSOSDocSender = (BDSOSDocSender*)pDocument;
		pSOSDocSender->AddERPFilter(name, pFromData, pToData);	
	}

	if (pDocument->IsKindOf(RUNTIME_CLASS(BDSOSAdjustAttachments)))
	{
		BDSOSAdjustAttachments* pSOSAdjustAttachments = (BDSOSAdjustAttachments*)pDocument;
		pSOSAdjustAttachments->AddERPFilter(name, pFromData, pToData);
	}
}

//API
//-----------------------------------------------------------------------------
SqlConnection*	AFXAPI AfxGetDMSSqlConnection()
{
	return AfxGetTbRepositoryManager()->GetDMSSqlConnection();
}

//-----------------------------------------------------------------------------
TbRepositoryManager* AFXAPI AfxGetTbRepositoryManager()
{
	return (TbRepositoryManager*)AfxGetIDMSRepositoryManager();
}
