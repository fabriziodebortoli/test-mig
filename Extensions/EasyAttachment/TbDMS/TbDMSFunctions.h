
#pragma once

#include <TbGeneric\DataObj.h>

#include "beginh.dex"

class CAbstractFormDoc;
class CBaseDocument;
class CAttachmentsArray;


DataBool OpenBBTraylet();
DataBool OpenBBSettings();

DataStr  GetEasyAttachmentTempPath();
DataBool AttachFile(DataStr fileName, DataStr description, DataLng documentHandle, DataStr& result);
DataBool AttachArchivedDocument(DataLng archivedDocId, DataLng documentHandle, DataStr& result);
DataBool AttachFolder(DataStr folder, DataLng documentHandle, DataStr& result);
DataBool AttachFileInDocument(DataStr documentNamespace, DataStr documentKey, DataStr fileName, DataStr description, DataStr& result );
DataBool AttachFolderInDocument(DataStr documentNamespace, DataStr documentKey, DataStr folder, DataStr& result);
DataBool AttachFromTable(DataStr documentNamespace, DataStr documentKey, DataStr& result);

DataBool ArchiveFile(DataStr fileName, DataStr description, DataStr& result);
DataBool ArchiveFolder(DataStr folder, DataStr& result);

//permettono di archiviare direttamente il binario senza passare da file system
DataBool ArchiveBinaryContent(DataBlob binaryContent, DataStr sourceFileName, DataStr description, DataLng& archivedDocId, DataStr& result);
DataBool AttachBinaryContent(DataBlob binaryContent, DataStr sourceFileName, DataStr description, DataLng documentHandle, DataLng& attachmentId, DataStr& result);
DataBool AttachBinaryContentInDocument(DataBlob binaryContent, DataStr sourceFileName, DataStr description, DataStr documentNamespace, DataStr documentKey, DataLng& attachmentId, DataStr& result);

//permette di ottenere il contenuto binario dell'attachment la cui chiave è passata come argomento
//viene restituito anche il nome del file (utile per le aperture e gli eventuali salvataggi del file)
// e se eventualmente il file è un bigFile o meno
DataBool GetAttachmentBinaryContent(DataLng attachmentID, DataBlob& binaryContent, DataStr& fileName, DataBool& veryLargeFile);

DataBool DeleteAttachment(DataLng attachmentID);
DataBool DeleteDocumentAttachments(DataStr documentNamespace, DataStr documentKey, DataStr& result);
DataBool DeleteAllERPDocumentInfo(DataStr documentNamespace, DataStr documentKey, DataStr& result);


DataStr  GetNewBarcodeValue();
DataBool GetDefaultBarcodeType(DataStr& type, DataStr& prefix);

DataBool AttachPaperyInDocument(DataStr documentNamespace, DataStr documentKey, DataStr barcode, DataStr description, DataStr& result);
DataLng  RunDocumentWithEAPanel(DataStr documentNamespace, DataStr documentKey, DataStr& result);
DataStr  GetAttachmentTemporaryFile(DataLng attachmentID);
DataBool AttachPaperyBarcode(DataLng documentHandle, DataStr barcode);
DataBool MassiveAttachUnattendedMode(DataStr folder, DataBool splitFile, DataStr& result);
DataLng  GetAttachmentIDByFileName(DataStr documentNamespace, DataStr documentKey, DataStr fileName);


//metodo che permette di ricercare attachment di uno specifico documento con i seguenti criteri
//- eventuale testo da ricercare
//- dove effettuare la ricerca del testo
//- lista degli eventuali chiavi di ricerca da utilizzare (formato: "bookmark1:valore1;bookmark2:valore2;bookmark3:valore4;"
//restituisce gli id degli attachment trovati
DataArray/*long*/ SearchAttachmentsForDocument(DataStr documentNamespace, DataStr documentKey, DataStr searchText, DataLng location, DataStr searchFields);

//permette di salvare il temporaneo dell'allegato in un folder (può essere anche shared)
DataStr SaveAttachmentFileInFolder(DataLng attachmentID,  DataStr folder);
BOOL	AttachArchivedDocInDocumentFunct(int archivedDocId, CString docNamespace, CString docKey, int& attachmentId, CString& result);

#include "endh.dex"
