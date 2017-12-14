
#include "stdafx.h" 

#include <TbNameSolver\FileSystemFunctions.h>
#include <TbGeneric\WebServiceStateObjects.h>
#include <TbGes\ExtDoc.h>
#include <TbGes\DBT.h>
#include <TbOleDb\sqlrec.h>
#include <TbOleDb\OleDbMng.h>
#include <TbGenlib\BaseApp.h>

#include "TbRepositoryManager.h"
#include "TbDMSFunctions.h"
#include "CDDMS.h"
#include "CDDMS.hjson" //JSON AUTOMATIC UPDATE

#ifdef _DEBUG
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

using namespace System;
using namespace System::Threading;
using namespace System::Globalization;

//[TBWebMethod(name = GetEasyAttachmentTempPath, woorm_method=false, securityhidden="true")]
///<summary>
///
///</summary>
/// <remarks>Return the user temp path used by Easy Attachment</remarks>
/// <returns></returns>
//----------------------------------------------------------------------------
DataStr GetEasyAttachmentTempPath()
{
	return DataStr(AfxGetTbRepositoryManager()->GetEasyAttachmentTempPath());
}

//-----------------------------------------------------------------------------
//[TBWebMethod(name = OpenBBTraylet, woorm_method=false)]
///<summary>
///Opening BrainBusiness Traylet of the forms
///</summary>
///<remarks>Opening BrainBusiness Traylet of the forms</remarks>
DataBool OpenBBTraylet()
{
	//chiamata necessaria per trasformare la library in AddOnLibrary ed inserirla nelle strutture dell'applicazione 
	//visto che si tratta di una dll in mixedmode e non esiste la dllmain
	//la chiamata LoadNeededLibraries viene effettuata anche in fase di caricamento del clientdoc CDDMS.
	//Ma se per caso l'utente accede prima al repository od ai settings è necessaria la creazione della AddOnLibrary 
	CTBNamespace aNS(CTBNamespace::LIBRARY, _T("Extensions.EasyAttachment.TbDMS"));
	AfxGetTbCmdManager()->LoadNeededLibraries(aNS);

	Thread::CurrentThread->CurrentUICulture = gcnew CultureInfo(gcnew String(AfxGetCulture()));

	if (AfxGetOleDbMng()->EasyAttachmentEnable())
	{
		AfxGetApp()->BeginWaitCursor();
		AfxGetTbRepositoryManager()->OpenBBTraylet();
		AfxGetApp()->EndWaitCursor();
	}
	else
		AfxMessageBox(_TB("Impossible to open BrainBusiness's Settings!\r\nPlease, check in Administration Console if this company uses EasyAttachment"));
	return TRUE;
}

//-----------------------------------------------------------------------------
//[TBWebMethod(name = OpenBBSettings, woorm_method=false)]
///<summary>
///Opening BrainBusiness Settings of the forms
///</summary>
///<remarks>Opening BrainBusiness Settings of the forms</remarks>
DataBool OpenBBSettings()
{
	//chiamata necessaria per trasformare la library in AddOnLibrary ed inserirla nelle strutture dell'applicazione 
	//visto che si tratta di una dll in mixedmode e non esiste la dllmain
	//la chiamata LoadNeededLibraries viene effettuata anche in fase di caricamento del clientdoc CDDMS.
	//Ma se per caso l'utente accede prima al repository od ai settings è necessaria la creazione della AddOnLibrary 
	CTBNamespace aNS(CTBNamespace::LIBRARY, _T("Extensions.EasyAttachment.TbDMS"));
	AfxGetTbCmdManager()->LoadNeededLibraries(aNS);

	Thread::CurrentThread->CurrentUICulture = gcnew CultureInfo(gcnew String(AfxGetCulture()));

	if (AfxGetOleDbMng()->EasyAttachmentEnable())
	{
		AfxGetApp()->BeginWaitCursor();
		AfxGetTbRepositoryManager()->OpenBBSettings();
		AfxGetApp()->EndWaitCursor();
	}
	else
		AfxMessageBox(_TB("Impossible to open BrainBusiness's Settings!\r\nPlease, check in Administration Console if this company uses EasyAttachment"));
	return TRUE;
}


// Functions called on document's thread by AfxInvokeThreadGlobalFunction 
//----------------------------------------------------------------------------
void PostCreateDocumentAndBrowse(CAbstractFormDoc* pDocument)
{
	pDocument->OnRadarRecordSelected(FALSE);
	/*if (pDocument->GetMasterFrame())
	pDocument->GetMasterFrame()->ShowWindow(SW_HIDE);*/
}

//----------------------------------------------------------------------------------------------
BOOL PostAttachSingleFile(CAbstractFormDoc* pDocument, DataStr& fileName, DataStr& description, DataStr& result)
{
	if (!ExistFile(fileName))
	{
		result = cwsprintf(_TB("The file {0-%s} doesn't exist"), fileName);
		return FALSE;
	}

	int nAttachmentId = -1;
	CString strMsg;

	ArchiveResult archiveResult = pDocument->GetDMSAttachmentManager()->AttachFile(fileName, description, nAttachmentId, strMsg);	
	result.Assign(strMsg);

	return (archiveResult != TerminatedWithError);
}

//----------------------------------------------------------------------------
BOOL PostAttachArchivedDocument(int archivedDocId, CAbstractFormDoc* pDoc, int& attachmentId, CString& result)
{
	ArchiveResult archiveResult = pDoc->GetDMSAttachmentManager()->AttachArchivedDocument(archivedDocId, attachmentId, result);

	return  archiveResult != TerminatedWithError;
}


// Function called on document's thread by AfxInvokeThreadGlobalFunction to attach operations on document
//----------------------------------------------------------------------------------------------
BOOL PostAttachFolder(CAbstractFormDoc* pDocument, DataStr& folder, DataStr& result)
{
	if (!ExistPath(folder))
	{
		result = cwsprintf(_TB("The path {0-%s} doesn't exist"), folder);
		return FALSE;
	}

	CClientDoc* pClientDoc = pDocument->GetClientDoc(_T("extensions.easyattachment.tbdms.CDDMS"));
	if (!pDocument->ValidCurrentRecord())
	{
		result = _TB("The document has no valid record.");
		return FALSE;
	}
	CString strFolder = folder;
	if (pClientDoc && pClientDoc->IsKindOf(RUNTIME_CLASS(CDDMS)))
	{
		ArchiveResult archiveResult = TerminatedSuccess;
		CFileFind finder;
		BOOL bWorking = finder.FindFile(strFolder + URL_SLASH_CHAR + _T("*.*"));
		CStringArray arFilesToArchvie;

		while (bWorking)
		{
			bWorking = finder.FindNextFile();
			// evito "." e ".." per evitare ricorsione
			if (finder.IsDots() || finder.IsDirectory())
				continue;

			arFilesToArchvie.Add(finder.GetFilePath());
		}
		finder.Close();

		if (arFilesToArchvie.GetSize() > 0)
			archiveResult = ((CDDMS*)pClientDoc)->AttachFiles(&arFilesToArchvie);

		return  archiveResult != TerminatedWithError;
	}
	result = _TB("No attachment created.");
	return FALSE;
}

// Function called on document's thread by AfxInvokeThreadGlobalFunction to attach operations on document
//----------------------------------------------------------------------------------------------
BOOL PostAttachFromTable(CAbstractFormDoc* pDocument, DataStr& result)
{
	CString strResult;
	ArchiveResult archiveResult = pDocument->GetDMSAttachmentManager()->AttachFromTable(strResult);
	result.Assign(strResult);

	return archiveResult != TerminatedWithError;
}

//----------------------------------------------------------------------------
BOOL PostAttachBinaryContent(CAbstractFormDoc* pDocument, const DataBlob& binaryContent, const DataStr& sourceFileName, const DataStr& description, DataLng& attachmentID, DataStr& result)
{
	int nAttachmentID = -1;
	CString strResult; 
	::ArchiveResult archiveResult =  pDocument->GetDMSAttachmentManager()->AttachBinaryContent(binaryContent, sourceFileName, description, nAttachmentID, strResult);
	attachmentID.Assign(nAttachmentID);
	result.Assign(strResult);	

	return archiveResult != TerminatedWithError;
}


// Function called on document's thread by AfxInvokeThreadGlobalFunction to attach papery on document
//----------------------------------------------------------------------------------------------
BOOL PostAttachPapery(CAbstractFormDoc* pDocument, const DataStr& fileName, const DataStr& description, const DataStr& barcode, DataStr& result)
{
	if (!pDocument->ValidCurrentRecord())
	{
		result = _TB("The document has no valid record.");
		return FALSE;
	}
	CString strResult;
	ArchiveResult archiveResult = pDocument->GetDMSAttachmentManager()->AttachPapery(barcode, description, fileName, strResult);
	result.Assign(strResult);

	return archiveResult != TerminatedWithError;
}

//[TBWebMethod(name = AttachFile, woorm_method=false, securityhidden="true")]
///<summary>
///Archives the file and create a new attachments for the document with documentHandle.
/// If the field description is empty the file name is used as description for the attachment.
///</summary>
/// <remarks>
///The possible errors due to attach operation are in document CMessages
///The document must be in BROWSE on the record choosed for attaching the file to 
///in result the error messages of this function
///</remarks>/// <returns></returns>
//----------------------------------------------------------------------------
DataBool AttachFile(DataStr fileName, DataStr description, DataLng documentHandle, DataStr& result)
{
	//chiamata necessaria per trasformare la library in AddOnLibrary ed inserirla nelle strutture dell'applicazione 
	//in  visto che si tratta di una dll in mixedmode e non esiste la dllmain
	//la chiamata LoadNeededLibraries viene effettuata anche in fase di caricamento del clientdoc CDDMS.
	//Ma se per caso l'utente archivia prima un report di menù è necessaria la creazione della AddOnLibrary 
	CTBNamespace aNS(CTBNamespace::LIBRARY, _T("Extensions.EasyAttachment.TbDMS"));		
	AfxGetTbCmdManager()->LoadNeededLibraries(aNS);

	Thread::CurrentThread->CurrentUICulture = gcnew CultureInfo(gcnew String(AfxGetCulture()));

	result = _T("");	
	if (documentHandle <= 0)
	{
		result = _TB("No document handle was specified.");
		return FALSE;
	}
	
	CObject* pObj = (CObject*)(long)documentHandle;
	if (!AfxExistWebServiceStateObject(pObj) || !pObj->IsKindOf(RUNTIME_CLASS(CAbstractFormDoc)))
	{
		result = _TB("Not exists active document corrispondent to shown identifier");
		return FALSE;
	}

	CString strResult;
	BOOL bOK = FALSE;
	CAbstractFormDoc* pDocument = (CAbstractFormDoc*)pObj;
	BOOL bOk = AfxInvokeThreadGlobalFunction<BOOL, CAbstractFormDoc*, DataStr&, DataStr&, DataStr&>
		(
			pDocument->GetThreadId(),
			&PostAttachSingleFile,
			pDocument,
			fileName,
			description,
			result
			);
	
	result = strResult;
	return bOK;
}		



//[TBWebMethod(name = AttachArchivedDocument, woorm_method=false, securityhidden="true")]
///<summary>
///Create attachment from archivedDocId
///</summary>
/// <remarks>
///The possible errors due to attach operation are in document CMessages
///The document must be in BROWSE on the record choosed for attaching the file to 
///in result the error messages of this function
///</remarks>/// <returns></returns>
//----------------------------------------------------------------------------
DataBool AttachArchivedDocument(DataLng archivedDocId, DataLng documentHandle, DataStr& result)
{
	//chiamata necessaria per trasformare la library in AddOnLibrary ed inserirla nelle strutture dell'applicazione 
	//in  visto che si tratta di una dll in mixedmode e non esiste la dllmain
	//la chiamata LoadNeededLibraries viene effettuata anche in fase di caricamento del clientdoc CDDMS.
	//Ma se per caso l'utente archivia prima un report di menù è necessaria la creazione della AddOnLibrary 
	CTBNamespace aNS(CTBNamespace::LIBRARY, _T("Extensions.EasyAttachment.TbDMS"));		
	AfxGetTbCmdManager()->LoadNeededLibraries(aNS);

	Thread::CurrentThread->CurrentUICulture = gcnew CultureInfo(gcnew String(AfxGetCulture()));

	result = _T("");
	if (documentHandle <= 0)
	{
		result = _TB("No document handle was specified.");
		return FALSE;
	}
	
	CObject* pObj = (CObject*)(long)documentHandle;
	if (!AfxExistWebServiceStateObject(pObj) || !pObj->IsKindOf(RUNTIME_CLASS(CAbstractFormDoc)))
	{
		result = _TB("Not exists active document corrispondent to shown identifier");
		return FALSE;
	}
	int archDocId = archivedDocId;
	int attachmentId = -1;	
	CString strResult;
	BOOL bOK = FALSE;
	CAbstractFormDoc* pDocument = (CAbstractFormDoc*)pObj;
	bOK = AfxInvokeThreadGlobalFunction<BOOL, int, CAbstractFormDoc*, int&, CString&>
		(
			pDocument->GetThreadId(),
			&PostAttachArchivedDocument,
			archDocId,
			pDocument,
			attachmentId,
			strResult
		);

	result = strResult;
	return bOK;
}


//[TBWebMethod(name = AttachFolder , woorm_method=false, securityhidden="true")]
///<summary>
///Archives the files in folder and creates a new attachments for each of them for the document with documentHandle.
///</summary>
/// <remarks>
///The possible errors due to attach operation are in document CMessages
///The document must be in BROWSE on the record choosed for attaching the file to 
///in result the error messages of this function
///</remarks>
/// <returns></returns>
//----------------------------------------------------------------------------
DataBool AttachFolder(DataStr folder,  DataLng documentHandle, DataStr& result)
{
	//chiamata necessaria per trasformare la library in AddOnLibrary ed inserirla nelle strutture dell'applicazione 
	//in  visto che si tratta di una dll in mixedmode e non esiste la dllmain
	//la chiamata LoadNeededLibraries viene effettuata anche in fase di caricamento del clientdoc CDDMS.
	//Ma se per caso l'utente archivia prima un report di menù è necessaria la creazione della AddOnLibrary 
	CTBNamespace aNS(CTBNamespace::LIBRARY, _T("Extensions.EasyAttachment.TbDMS"));		
	AfxGetTbCmdManager()->LoadNeededLibraries(aNS);

	Thread::CurrentThread->CurrentUICulture = gcnew CultureInfo(gcnew String(AfxGetCulture()));

	
	result = _T("");
	if (documentHandle <= 0)
	{
		result = _TB("No document handle was specified.");
		return FALSE;
	}
	
	CObject* pObj = (CObject*)(long)documentHandle;
	if (!AfxExistWebServiceStateObject(pObj) || !pObj->IsKindOf(RUNTIME_CLASS(CAbstractFormDoc)))
	{
		result = _TB("Not exists active document corrispondent to shown identifier");
		return FALSE;
	}
	CString strResult;
	BOOL bOK = FALSE;
	CAbstractFormDoc* pDocument = (CAbstractFormDoc*)pObj;

	BOOL bOk = AfxInvokeThreadGlobalFunction<BOOL, CAbstractFormDoc*, DataStr&, DataStr&>
		(
			pDocument->GetThreadId(),
			&PostAttachFolder,
			pDocument,
			folder,
			result
			);
	return bOk;
}		

//[TBWebMethod(name = ArchiveFile, woorm_method=false, securityhidden="true")]
///<summary>
///Uses to archive a file in EasyAttachment repository. If the field desciption is empty the file name is used as description for archived document
///</summary>
/// <remarks>
///in result the error messages of this function
///</remarks>
/// <returns></returns>
//----------------------------------------------------------------------------
DataBool ArchiveFile(DataStr fileName, DataStr description, DataStr& result)
{
	//chiamata necessaria per trasformare la library in AddOnLibrary ed inserirla nelle strutture dell'applicazione 
	//in  visto che si tratta di una dll in mixedmode e non esiste la dllmain
	//la chiamata LoadNeededLibraries viene effettuata anche in fase di caricamento del clientdoc CDDMS.
	//Ma se per caso l'utente archivia prima un report di menù è necessaria la creazione della AddOnLibrary 
	CTBNamespace aNS(CTBNamespace::LIBRARY, _T("Extensions.EasyAttachment.TbDMS"));		
	AfxGetTbCmdManager()->LoadNeededLibraries(aNS);

	Thread::CurrentThread->CurrentUICulture = gcnew CultureInfo(gcnew String(AfxGetCulture()));

	CString file = fileName.GetString();
	if (!ExistFile(file))
	{
		result = cwsprintf(_TB("The file {0-%s} doesn't exist"), file);
		return FALSE;
	}
	
	CString strMsg = _T("");
	int archivedDocID = -1;
	ArchiveResult archiveResult = (ArchiveResult)AfxGetTbRepositoryManager()->ArchiveFile(file, description.GetString(), archivedDocID, strMsg);
	result.AssignFromXMLString(strMsg);
	
	return archiveResult != ::ArchiveResult::TerminatedWithError;
}

//[TBWebMethod(name = ArchiveFolder, woorm_method=false, securityhidden="true")]
///<summary>
///Uses to archive in EasyAttachment repository ach files in folder.
///</summary>
/// <remarks>
///in result the error messages of this function
///</remarks>
/// <returns></returns>
//----------------------------------------------------------------------------
DataBool ArchiveFolder(DataStr folder, DataStr& result)
{
	//chiamata necessaria per trasformare la library in AddOnLibrary ed inserirla nelle strutture dell'applicazione 
	//in  visto che si tratta di una dll in mixedmode e non esiste la dllmain
	//la chiamata LoadNeededLibraries viene effettuata anche in fase di caricamento del clientdoc CDDMS.
	//Ma se per caso l'utente archivia prima un report di menù è necessaria la creazione della AddOnLibrary 
	CTBNamespace aNS(CTBNamespace::LIBRARY, _T("Extensions.EasyAttachment.TbDMS"));		
	AfxGetTbCmdManager()->LoadNeededLibraries(aNS);

	Thread::CurrentThread->CurrentUICulture = gcnew CultureInfo(gcnew String(AfxGetCulture()));

	if (!ExistPath(folder))
	{
		result = cwsprintf(_TB("The path {0-%s} doesn't exist"), folder);
		return FALSE;
	}

	result = _T("");
	return (ArchiveResult)AfxGetTbRepositoryManager()->ArchiveFolder(folder);
}



//----------------------------------------------------------------------------
CAbstractFormDoc* CreateDocumentAndBrowse(DataStr documentNamespace, DataStr documentKey, DataStr& result)
{
	//chiamata necessaria per trasformare la library in AddOnLibrary ed inserirla nelle strutture dell'applicazione 
	//in  visto che si tratta di una dll in mixedmode e non esiste la dllmain
	//la chiamata LoadNeededLibraries viene effettuata anche in fase di caricamento del clientdoc CDDMS.
	//Ma se per caso l'utente archivia prima un report di menù è necessaria la creazione della AddOnLibrary 
	CTBNamespace aNS(CTBNamespace::LIBRARY, _T("Extensions.EasyAttachment.TbDMS"));		
	AfxGetTbCmdManager()->LoadNeededLibraries(aNS);


	// Prima di tutto istanzio il documento mediante il namespace passato
	CTBNamespace aNsDocument(documentNamespace.GetString());
	const CDocumentDescription* docDescription = AfxGetDocumentDescription(aNsDocument);
	
	if (docDescription)
	{ 
		const CBaseDocument* pBaseDoc = AfxGetTbCmdManager()->RunDocument
														(
															documentNamespace.GetString(), 
															szBackgroundViewMode,
															FALSE
														);
		if (pBaseDoc && pBaseDoc->IsKindOf(RUNTIME_CLASS(CAbstractFormDoc)))
		{
			CAbstractFormDoc* pDocument = (CAbstractFormDoc*)pBaseDoc;
			
			SqlRecord* pMasterRec = (pDocument->m_pDBTMaster) ? pDocument->m_pDBTMaster->GetRecord() : NULL;
			result = _T("");
			if (!pMasterRec)
			{
				result = cwsprintf(_TB("The document {0-%s} has no DBTMaster. Impossible to browse it."), pDocument->GetNamespace());
				pDocument->CloseDocument();	
				return NULL;
			}

			pMasterRec->SetPrimaryKeyNameValue(documentKey.Str());
			AfxInvokeThreadGlobalProcedure<CAbstractFormDoc*>(pDocument->GetThreadId(), &PostCreateDocumentAndBrowse, pDocument);
			return pDocument;
		}
	}

	return NULL;		
}

//[TBWebMethod(name = AttachFileInDocument, woorm_method=false, securityhidden="true")]
///<summary>
///Opens the document using documentNamespace value and browse it using documentKey value. Archive and attach to document
//the file in filaName. 
// If the description value is empty the file name is used as description for the attachment
///</summary>
/// <remarks>
///in result the error messages of this function
// documentNamespace value must be the namespace of the document to open. i.e:  ERP.CustomersSuppliers.Documents.Customers
//the syntax for documentKey value must be like this: segKeyName1:value1;segKeyName2:value2. i.e.CustSuppType:3211264;CustSupp:0051
///</remarks>
/// <returns></returns>
//----------------------------------------------------------------------------
DataBool AttachFileInDocument(DataStr documentNamespace, DataStr documentKey, DataStr fileName, DataStr description, DataStr& result )
{
	CAbstractFormDoc* pDocument = CreateDocumentAndBrowse(documentNamespace, documentKey, result);
	if (pDocument)
	{
		BOOL bOk = AfxInvokeThreadGlobalFunction<BOOL, CAbstractFormDoc*, DataStr&, DataStr&, DataStr&>
				(
					pDocument->GetThreadId(),
					&PostAttachSingleFile,
					pDocument,
					fileName,
					description,
					result
				);
		
		pDocument->CloseDocument();	
		return bOk;
	}	
	result = cwsprintf(_TB("Impossibile to open the document {0-%s}. Attaching failed."), documentNamespace.GetString());
	return FALSE;
}


//[TBWebMethod(name = AttachFolderInDocument, woorm_method=false, securityhidden="true")]
///<summary>
///Opens the document using documentNamespace value and browse it using documentKey value. Archive and attach to document
//the files in folder
// As description for each attachment is used the file name 
///</summary>
/// <remarks>
///in result the error messages of this function
// documentNamespace value must be the namespace of the document to open. i.e:  ERP.CustomersSuppliers.Documents.Customers
//the syntax for documentKey value must be like this: segKeyName1:value1;segKeyName2:value2. i.e.CustSuppType:3211264;CustSupp:0051
///</remarks>
/// <returns></returns>
//----------------------------------------------------------------------------
DataBool AttachFolderInDocument(DataStr documentNamespace, DataStr documentKey, DataStr folder, DataStr& result )
{
	CAbstractFormDoc* pDocument = CreateDocumentAndBrowse(documentNamespace, documentKey,result);
	if (pDocument)
	{
		BOOL bOk = AfxInvokeThreadGlobalFunction<BOOL, CAbstractFormDoc*, DataStr&, DataStr&>
				(
					pDocument->GetThreadId(),
					&PostAttachFolder,
					pDocument,
					folder,
					result
				);
		pDocument->CloseDocument();	
		return bOk;
	}	
	result = cwsprintf(_TB("Impossibile to open the document {0-%s}. Attaching failed."), documentNamespace.GetString());
	return FALSE;
}


//[TBWebMethod(name = ArchiveBinaryContent, woorm_method=false, securityhidden="true")]
///<summary>
///Archive int the DMS repository the binary content
///</summary>
/// <remarks>
/// in archivedDocID the funciotn returns the key of the archived document
///in result the error messages of this function 
// Please specify an exist or not exist well formed path in sourceFileName and a description.
///</remarks>
/// <returns></returns>
//----------------------------------------------------------------------------
DataBool ArchiveBinaryContent(DataBlob binaryContent, DataStr sourceFileName, DataStr description, DataLng& archivedDocID, DataStr& result)
{
	ArchiveResult archiveResult = (ArchiveResult)AfxGetTbRepositoryManager()->ArchiveBinaryContent(binaryContent, sourceFileName, description, archivedDocID, result);
	return archiveResult != ::ArchiveResult::TerminatedWithError;
}



//[TBWebMethod(name = AttachFromTable, woorm_method=false, securityhidden="true")]
///<summary>
///Opens the document using documentNamespace value and browse it using documentKey value. Archive and attach the binaries in DMS_DocumentToArchive
//with documentNamespace and documentKey. 
///</summary>
/// <remarks>
///in result the error messages of this function
// documentNamespace value must be the namespace of the document to open. i.e:  ERP.CustomersSuppliers.Documents.Customers
//the syntax for documentKey value must be like this: segKeyName1:value1;segKeyName2:value2. i.e.CustSuppType:3211264;CustSupp:0051
///</remarks>
/// <returns></returns>
//----------------------------------------------------------------------------
DataBool AttachFromTable(DataStr documentNamespace, DataStr documentKey, DataStr& result)
{
	CAbstractFormDoc* pDocument = CreateDocumentAndBrowse(documentNamespace, documentKey, result);
	if (pDocument)
	{
		BOOL bOk = AfxInvokeThreadGlobalFunction<BOOL, CAbstractFormDoc*, DataStr&>
			(
				pDocument->GetThreadId(),
				&PostAttachFromTable,
				pDocument,
				result
				);
		pDocument->CloseDocument();
		return bOk;
	}
	result = cwsprintf(_TB("Impossibile to open the document {0-%s}. Attaching failed."), documentNamespace.GetString());
	return FALSE;
}


//[TBWebMethod(name = AttachBinaryContent, woorm_method=false, securityhidden="true")]
///<summary>
///Archive and attach the binary content to the document with documentHandle. 
///</summary>
/// <remarks>
/// in attachmentId the function returns the key of the new attachment
///in result the error messages of this function
// The document must have a valid current record.
// Please specify an exist or not exist well formed path in sourceFileName and a description.
///</remarks>
/// <returns></returns>
//----------------------------------------------------------------------------
DataBool AttachBinaryContent(DataBlob binaryContent, DataStr sourceFileName, DataStr description, DataLng documentHandle, DataLng& attachmentId, DataStr& result)
{
	//chiamata necessaria per trasformare la library in AddOnLibrary ed inserirla nelle strutture dell'applicazione 
	//in  visto che si tratta di una dll in mixedmode e non esiste la dllmain
	//la chiamata LoadNeededLibraries viene effettuata anche in fase di caricamento del clientdoc CDDMS.
	//Ma se per caso l'utente archivia prima un report di menù è necessaria la creazione della AddOnLibrary 
	CTBNamespace aNS(CTBNamespace::LIBRARY, _T("Extensions.EasyAttachment.TbDMS"));
	AfxGetTbCmdManager()->LoadNeededLibraries(aNS);

	Thread::CurrentThread->CurrentUICulture = gcnew CultureInfo(gcnew String(AfxGetCulture()));
	result = _T("");
	if (documentHandle <= 0)
	{
		result = _TB("No document handle was specified.");
		return FALSE;
	}

	CObject* pObj = (CObject*)(long)documentHandle;
	if (!AfxExistWebServiceStateObject(pObj) || !pObj->IsKindOf(RUNTIME_CLASS(CAbstractFormDoc)))
	{
		result = _TB("Not exists active document corrispondent to shown identifier");
		return FALSE;
	}
	CAbstractFormDoc* pDoc = (CAbstractFormDoc*)pObj;
	BOOL bOK = AfxInvokeThreadGlobalFunction<BOOL, CAbstractFormDoc*, const DataBlob&, const DataStr&, const DataStr&, DataLng&, DataStr&>
		(
			pDoc->GetThreadId(),
			&PostAttachBinaryContent,
			pDoc,
			binaryContent,
			sourceFileName,
			description,
			attachmentId,
			result
			);
	return bOK;
}

//[TBWebMethod(name = AttachBinaryContentInDocument, woorm_method=false, securityhidden="true")]
///<summary>
///Opens the document using documentNamespace value and browse it using documentKey value. Archive and attach the binaries in DMS_DocumentToArchive
//with documentNamespace and documentKey. 
///</summary>
/// <remarks>
/// in attachmentId the function returns the key of the new attachment
///in result the error messages of this function
// documentNamespace value must be the namespace of the document to open. i.e:  ERP.CustomersSuppliers.Documents.Customers
//the syntax for documentKey value must be like this: segKeyName1:value1;segKeyName2:value2. i.e.CustSuppType:3211264;CustSupp:0051
// Please specify an exist or not exist well formed path in sourceFileName and a description.
///</remarks>
/// <returns></returns>
//----------------------------------------------------------------------------
DataBool AttachBinaryContentInDocument(DataBlob binaryContent, DataStr sourceFileName, DataStr description, DataStr documentNamespace, DataStr documentKey, DataLng& attachmentId, DataStr& result)
{
	CAbstractFormDoc* pDocument = CreateDocumentAndBrowse(documentNamespace, documentKey, result);
	if (pDocument)
	{
		BOOL bOk = AfxInvokeThreadGlobalFunction<BOOL, CAbstractFormDoc*, const DataBlob&, const DataStr&, const DataStr&, DataLng&, DataStr&>
			(
				pDocument->GetThreadId(),
				&PostAttachBinaryContent,
				pDocument,
				binaryContent,
				sourceFileName,
				description,
				attachmentId,
				result
				);
		pDocument->CloseDocument();
		return bOk;
	}
	result = cwsprintf(_TB("Impossibile to open the document {0-%s}. Attaching failed."), documentNamespace.GetString());
	return FALSE;
}



//[TBWebMethod(name = DeleteAttachment, woorm_method=true, securityhidden="true")]
///<summary>
///delete the single attachment with attachmentId
///</summary>
/// <returns></returns>
//----------------------------------------------------------------------------
DataBool DeleteAttachment(DataLng attachmentID)
{
	return AfxGetTbRepositoryManager()->DeleteAttachment(attachmentID);
}

//[TBWebMethod(name = DeleteDocumentAttachments, woorm_method=true, securityhidden="true")]
///<summary>
///delete only attachments of the erp document with documentNamespace and documentKey
///</summary>
/// <returns></returns>
//----------------------------------------------------------------------------
DataBool DeleteDocumentAttachments(DataStr documentNamespace, DataStr documentKey, DataStr& result)
{
	CString strRes;
	BOOL bOK = AfxGetTbRepositoryManager()->DeleteDocumentAttachments(documentNamespace.Str(), documentKey.Str(), strRes);
	result.Assign(strRes);
	return bOK;
}


//[TBWebMethod(name = DeleteAllERPDocumentInfo, woorm_method=true, securityhidden="true")]
///<summary>
///delete all information (attachments, paperies and record in DMS_ERPDocument) about the erp document with documentNamespace and documentKey
///</summary>
/// <returns></returns>
//----------------------------------------------------------------------------
DataBool DeleteAllERPDocumentInfo(DataStr documentNamespace, DataStr documentKey, DataStr& result)
{
	CString strRes;
	BOOL bOK = AfxGetTbRepositoryManager()->DeleteAllERPDocumentInfo(documentNamespace.Str(), documentKey.Str(), strRes);
	result.Assign(strRes);
	return bOK;
}

//[TBWebMethod(name = GetNewBarcodeValue, woorm_method=true, securityhidden="true")]
///<summary>
///Returns a random value to generate a new barcode
///</summary>
/// <returns></returns>
//----------------------------------------------------------------------------
DataStr GetNewBarcodeValue()
{
	return DataStr(AfxGetTbRepositoryManager()->GetNewBarcodeValue().m_strBarcodeValue);
}

//[TBWebMethod(name = GetDefaultBarcodeType, woorm_method=true, securityhidden="true")]
///<summary>
///Returns a the default type for barcodes in easyattachment
///</summary>
/// <returns></returns>
//----------------------------------------------------------------------------
DataBool GetDefaultBarcodeType(DataStr& type, DataStr& prefix)
{
	return DataBool(AfxGetTbRepositoryManager()->GetDefaultBarcodeType(type, prefix));
}





//[TBWebMethod(name = AttachPaperyBarcode, woorm_method=true,  securityhidden="true")]
///<summary>
///Returns the barcode of a new papery attached to the current erp document
///</summary>
/// <returns></returns>
//----------------------------------------------------------------------------
DataBool AttachPaperyBarcode(DataLng documentHandle, DataStr barcode)
{
	DataStr description = _TB("Papery attached by report Barcode labels of document");
	DataStr result = _T("");
	DataStr fileName = _T("");
	if (documentHandle > 0)
	{
		CObject* pObj = (CObject*)(long)documentHandle;
		if (!AfxExistWebServiceStateObject(pObj) || !pObj->IsKindOf(RUNTIME_CLASS(CAbstractFormDoc)))
		{
			//result = _TB("Not exists active document corrispondent to shown identifier");
			return FALSE;
		}

		CAbstractFormDoc* pDocument = (CAbstractFormDoc*)pObj;
		return AfxInvokeThreadGlobalFunction<BOOL, CAbstractFormDoc*, const DataStr&, const DataStr&, const DataStr&, DataStr&>
			(
				pDocument->GetThreadId(),
				&PostAttachPapery,
				pDocument,
				fileName,				
				description, 
				barcode,
				result
				);
	}

	return TRUE;
}



//[TBWebMethod(name = AttachPaperyInDocument, woorm_method=false,  securityhidden="true")]
///<summary>
///Attach the papery with barcode to the ERP document with documentNamespace and documentKey
///</summary>
/// <returns></returns>
//----------------------------------------------------------------------------
DataBool AttachPaperyInDocument(DataStr documentNamespace, DataStr documentKey, DataStr barcode, DataStr description, DataStr& result)
{
	CAbstractFormDoc* pDocument = CreateDocumentAndBrowse(documentNamespace, documentKey,result);
	DataStr fileName = _T("");
	if (pDocument)
	{

		BOOL bOk = AfxInvokeThreadGlobalFunction<BOOL, CAbstractFormDoc*, const DataStr&, const DataStr&, const DataStr&, DataStr&>
			(
				pDocument->GetThreadId(),
				&PostAttachPapery,
				pDocument,
				fileName,
				description,
				barcode,
				result
				);	

		pDocument->CloseDocument();
		return bOk;
	}
	result = cwsprintf(_TB("Impossibile to open the document {0-%s}. Attaching failed."), documentNamespace.GetString());
	return FALSE;
}


//[TBWebMethod(name = RunDocumentWithEAPanel, woorm_method=false,  securityhidden="true")]
///<summary>
///Returns the barcode value for report with reportNamespace
///</summary>
/// <returns></returns>
//----------------------------------------------------------------------------
DataLng RunDocumentWithEAPanel(DataStr documentNamespace, DataStr documentKey, DataStr& result)
{
	CAbstractFormDoc* pDocument = CreateDocumentAndBrowse(documentNamespace, documentKey, result);
	/*if (pDocument)
	{
		pDocument->OnRadarRecordSelected(FALSE);		
		pDocument->PostMessage(WM_COMMAND, ID_OPEN_DMS, 0); 
		return (long)pDocument;
	}
*/
	return 0;
}

//[TBWebMethod(name = GetAttachmentTemporaryFilePath, woorm_method=false,  securityhidden="true")]
///<summary>
///Returns the complete path of the temporary file of the attachment with attachmentID as key
///</summary>
/// <returns></returns>
//----------------------------------------------------------------------------
DataStr GetAttachmentTemporaryFile(DataLng attachmentID)
{
	return AfxGetTbRepositoryManager()->GetAttachmentTempFile(attachmentID);
}


//[TBWebMethod(name = MassiveAttachUnattendedMode, woorm_method=false,  securityhidden="true")]
///<summary>
///in result the 
///</summary>
/// <returns></returns>
//----------------------------------------------------------------------------
DataBool MassiveAttachUnattendedMode(DataStr folder, DataBool splitFile, DataStr& result)
{
	result = AfxGetTbRepositoryManager()->MassiveArchiveUnattendedMode(folder.GetString(), (splitFile) ? true : false);
	return TRUE;
}



///<summary>
///metodo che permette di ricercare attachment di uno specifico documento con i seguenti criteri
/// searchText: eventuale testo da ricercare
/// location: dove effettuare la ricerca del testo
/// searchFields: lista degli eventuali chiavi di ricerca da utilizzare (formato: "bookmark1:valore1;bookmark2:valore2;bookmark3:valore4;"
///</summary>
/// <returns>array degli attachmentID che verificano la condizione di ricerca</returns>

//[TBWebMethod(name = SearchAttachmentsForDocument, woorm_method=false,  securityhidden="true")]
//----------------------------------------------------------------------------
DataArray/*long*/ SearchAttachmentsForDocument(DataStr documentNamespace, DataStr documentKey, DataStr searchText, DataLng location, DataStr searchFields)  
{
	CAttachmentsArray* attachmentsList =  AfxGetTbRepositoryManager()->SearchAttachmentsForDocument(documentNamespace.GetString(), documentKey.GetString(), searchText.GetString(), (SearchLocationEnum)(int)location, searchFields.GetString());  
	
	DataArray attachmentIDS;
	for (int i = 0; i < attachmentsList->GetCount(); i++)
		attachmentIDS.Add(new DataLng(attachmentsList->GetAt(i)->m_attachmentID));
	
	SAFE_DELETE(attachmentsList);
	return attachmentIDS;
}

///<summary>
///permette di salvare il temporaneo dell'allegato in un folder shared
/// attachmentID: attachment di cui copiare il contenuto
/// sharedFolder: folder in cui salvare il contenuto del file
///</summary>
/// <returns>fullname del file salvato</returns>
//[TBWebMethod(name = SaveAttachmentFileInFolder, woorm_method=false,  securityhidden="true")]
//----------------------------------------------------------------------------
DataStr SaveAttachmentFileInFolder(DataLng attachmentID, DataStr sharedFolder)
{
	return AfxGetTbRepositoryManager()->SaveAttachmentFileInFolder(attachmentID, sharedFolder.GetString());
}

///<summary>
/// return the binary content of the attachment with key = attachmentID 
/// attachmentID: the key of the attachment
/// binaryContent: the binary content. It is null if veryLargeFile is TRUE
//  fileName: the name of the attachment
//  veryLargeFile: if the size of the attachment > 500MB
///</summary>
/// <returns>TRUE if binary extraction successful completed; FALSE otherwise</returns>
//[TBWebMethod(name = GetAttachmentBinaryContent, woorm_method=false,  securityhidden="true")]
//-----------------------------------------------------------------------------
DataBool GetAttachmentBinaryContent(DataLng attachmentID, DataBlob& binaryContent, DataStr& fileName, DataBool& veryLargeFile)
{
	return AfxGetTbRepositoryManager()->GetAttachmentBinaryContent(attachmentID, binaryContent, fileName, veryLargeFile);
}

///<summary>
/// return the attachmentID of the archived file with fileName attached to document with documentNamespace and documentKey
///in result the error messages of this function
// documentNamespace value must be the namespace of the document to open. i.e:  ERP.CustomersSuppliers.Documents.Customers
//the syntax for documentKey value must be like this: segKeyName1:value1;segKeyName2:value2. i.e.CustSuppType:3211264;CustSupp:0051
//  fileName: the name of the attachment searching for
///</summary>
/// <returns>the attachmentID if the file is attached to document OTHERWISE -1</returns>
//[TBWebMethod(name = GetAttachmentIDByFileName, woorm_method=true,  securityhidden="true")]
//-----------------------------------------------------------------------------
DataLng  GetAttachmentIDByFileName(DataStr documentNamespace, DataStr documentKey, DataStr fileName)
{
	return AfxGetTbRepositoryManager()->GetAttachmentIDByFileName(documentNamespace, documentKey, fileName);
}


//----------------------------------------------------------------------------
BOOL AttachArchivedDocInDocumentFunct(int archivedDocId, CString docNamespace, CString docKey, int& attachmentId, CString& result)
{
	if (!AfxGetOleDbMng()->EasyAttachmentEnable())
		return FALSE;

	attachmentId = -1;
	result = _T("");

	CAbstractFormDoc* pDocument = CreateDocumentAndBrowse(DataStr(docNamespace), DataStr(docKey), DataStr(result));
	if (pDocument)
	{
		BOOL bOk =  AfxInvokeThreadGlobalFunction<BOOL, int, CAbstractFormDoc*, int&, CString&>
		(
			pDocument->GetThreadId(),
			&PostAttachArchivedDocument,
			archivedDocId,
			pDocument,
			attachmentId,
			result
		);

		pDocument->CloseDocument();	
		return bOk;
	}	
	result = cwsprintf(_TB("Impossibile to open the document {0-%s}. Attaching failed."), docNamespace.GetString());
	return FALSE;
}








