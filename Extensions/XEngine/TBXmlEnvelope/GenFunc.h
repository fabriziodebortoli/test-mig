
#pragma once

//includere alla fine degli include del .H
#include "beginh.dex"

/////////////////////////////////////////////////////////////////////////////

// path per la memorizzazione degli envelope da inviare e ricevuti
// il parametro booleano mi permette di fare creare o meno la path
// nel caso non esistesse

TB_EXPORT CString CompletePath(const CString& strPath, BOOL bAppendLastSlash, BOOL bCreate);

TB_EXPORT CString GetXMLExportPath					(BOOL bAppendLastSlash = TRUE, BOOL bCreate = TRUE);
TB_EXPORT CString GetXMLTXTargetPath					(BOOL bAppendLastSlash = TRUE, BOOL bCreate = TRUE);
TB_EXPORT CString GetXMLTXTargetSitePath				(const CString& strSiteName, BOOL bAppendLastSlash = TRUE, BOOL bCreate = TRUE);

TB_EXPORT CString GetXMLImportPath					(BOOL bAppendLastSlash = TRUE, BOOL bCreate = TRUE);
TB_EXPORT CString GetXMLRXSourcePath					(BOOL bAppendLastSlash = TRUE, BOOL bCreate = TRUE);
TB_EXPORT CString GetXMLRXSourceSitePath				(const CString& strSiteName, BOOL bAppendLastSlash = TRUE, BOOL bCreate = TRUE);

TB_EXPORT CString GetXMLPendingPath					(BOOL bIsForImport, BOOL bAppendLastSlash = TRUE, BOOL bCreate = TRUE);
TB_EXPORT CString GetXMLPendingSitePath				(BOOL bIsForImport,const CString& strSiteName, BOOL bAppendLastSlash = TRUE, BOOL bCreate = TRUE);

TB_EXPORT CString GetXMLFailurePath					(BOOL bIsForImport, BOOL bAppendLastSlash = TRUE, BOOL bCreate = TRUE);
TB_EXPORT CString GetXMLFailureSitePath				(BOOL bIsForImport,const CString& strSiteName, BOOL bAppendLastSlash = TRUE, BOOL bCreate = TRUE);

TB_EXPORT CString GetXMLSuccessPath					(BOOL bIsForImport, BOOL bAppendLastSlash = TRUE, BOOL bCreate = TRUE);
TB_EXPORT CString GetXMLSuccessSitePath				(BOOL bIsForImport,const CString& strSiteName, BOOL bAppendLastSlash = TRUE, BOOL bCreate = TRUE);

TB_EXPORT CString GetXMLPartialSuccessPath			(BOOL bIsForImport, BOOL bAppendLastSlash = TRUE, BOOL bCreate = TRUE);
TB_EXPORT CString GetXMLPartialSuccessSitePath		(BOOL bIsForImport,const CString& strSiteName, BOOL bAppendLastSlash = TRUE, BOOL bCreate = TRUE);

TB_EXPORT CString MakeSchemaName					(const CString& strFileName);
TB_EXPORT CString GetXMLSchemasPath					();

/////////////////////////////////////////////////////////////////////////////

#include "endh.dex"

