#include "stdafx.h" 
#include  <io.h>

#include <TBGeneric\GeneralFunctions.h>
#include <TBNameSolver\chars.h>
#include <TBXMLCore\XMLGeneric.h>

#include "XEngineObject.h"
#include "genfunc.h"

static const TCHAR	szXML[]					=	_T("XmlData");
static const TCHAR	szXMLExport[]			=	_T("Export");
static const TCHAR	szXMLImport[]			=	_T("Import");
static const TCHAR	szXMLTX[]				= 	_T("TX");
static const TCHAR	szXMLRX[]				= 	_T("RX");
static const TCHAR	szXMLFailure[]			= 	_T("Failure");
static const TCHAR	szXMLPending[]			= 	_T("Pending");
static const TCHAR	szXMLSuccess[]			= 	_T("Success");
static const TCHAR	szXMLPartialSuccess[]	=	_T("PartiallyProcessed");


//----------------------------------------------------------------------------------------------
CString CompletePath(const CString& strPath, BOOL bAppendLastSlash, BOOL bCreate)
{
	if (bCreate && !ExistPath(strPath) && !RecursiveCreateFolders(strPath))
		return _T("");
	
	return bAppendLastSlash ? strPath + SLASH_CHAR : strPath;
}


// directory principale per le operazioni di import/export
// <DynamicInstancePath>\XmlData
//----------------------------------------------------------------------------------------------
CString GetXMLDataPath(BOOL bAppendLastSlash /*=TRUE*/, BOOL bCreate /*= TRUE*/)
{
	CString strXMLPath = AfxGetDynamicInstancePath();
	if (strXMLPath.IsEmpty())
		return _T("");
	
	strXMLPath += szXML;

	return CompletePath(strXMLPath, bAppendLastSlash, bCreate);
}

// directory principale per l'esportazione
// .\XmlData\Export
//----------------------------------------------------------------------------------------------
CString GetXMLExportPath(BOOL bAppendLastSlash /*=TRUE*/, BOOL bCreate /*= TRUE*/)
{
	CString strXMLPath = GetXMLDataPath(TRUE, bCreate);
	if (strXMLPath.IsEmpty())
		return _T("");
	
	strXMLPath += szXMLExport;

	return CompletePath(strXMLPath, bAppendLastSlash, bCreate);

}

// directory principale per i trasferimenti di envelope
// .\XmlData\Export\TX
//----------------------------------------------------------------------------------------------
CString GetXMLTXTargetPath(BOOL bAppendLastSlash /*=TRUE*/, BOOL bCreate /*= TRUE*/)
{
	CString strXMLPath = GetXMLExportPath(TRUE, bCreate);
	if (strXMLPath.IsEmpty())
		return _T("");
	
	strXMLPath += szXMLTX;

	return CompletePath(strXMLPath, bAppendLastSlash, bCreate);

}

// directory principale per i trasferimenti di envelope del singolo site
// .\XmlData\\ExportTX\<site>
//----------------------------------------------------------------------------------------------
CString GetXMLTXTargetSitePath(const CString& strSiteName, BOOL bAppendLastSlash /*=TRUE*/, BOOL bCreate /*= TRUE*/)
{
	CString strXMLPath = GetXMLTXTargetPath(TRUE, bCreate);
	if (strXMLPath.IsEmpty())
		return _T("");
	
	strXMLPath +=  strSiteName;

	return CompletePath(strXMLPath, bAppendLastSlash, bCreate);

}

// directory principale per i l'importazione
// .\XmlData\Import
//----------------------------------------------------------------------------------------------
CString GetXMLImportPath(BOOL bAppendLastSlash /*=TRUE*/, BOOL bCreate /*= TRUE*/)
{
	CString strXMLPath = GetXMLDataPath(TRUE, bCreate);
	if (strXMLPath.IsEmpty())
		return _T("");
	
	strXMLPath += szXMLImport;

	return CompletePath(strXMLPath, bAppendLastSlash, bCreate);

}

// directory principale per i ricevimenti di envelope
// .\XmlData\Import\RX
//----------------------------------------------------------------------------------------------
CString GetXMLRXSourcePath(BOOL bAppendLastSlash /*=TRUE*/, BOOL bCreate /*= TRUE*/)
{
	CString strXMLPath = GetXMLImportPath(TRUE, bCreate);
	if (strXMLPath.IsEmpty())
		return _T("");
	
	strXMLPath += szXMLRX;

	return CompletePath(strXMLPath, bAppendLastSlash, bCreate);

}

// directory principale per i trasferimenti di envelope del singolo site
// .\XML\Import\RX\<site>
//----------------------------------------------------------------------------------------------
CString GetXMLRXSourceSitePath(const CString& strSiteName, BOOL bAppendLastSlash /*=TRUE*/, BOOL bCreate /*= TRUE*/)
{
	CString strXMLPath = GetXMLRXSourcePath(TRUE, bCreate);
	if (strXMLPath.IsEmpty())
		return _T("");
	
	strXMLPath +=  strSiteName;

	return CompletePath(strXMLPath, bAppendLastSlash, bCreate);

}

// directory principale per import/export in esecuzione
// .\XmlData\<Import|Export>\Pending
//----------------------------------------------------------------------------------------------
CString GetXMLPendingPath(BOOL bIsForImport, BOOL bAppendLastSlash /*=TRUE*/, BOOL bCreate /*= TRUE*/)
{
	CString strXMLPath = bIsForImport ? 
		GetXMLImportPath(TRUE, bCreate):
		GetXMLExportPath (TRUE, bCreate);

	if (strXMLPath.IsEmpty())
		return _T("");
	
	strXMLPath += szXMLPending;

	return CompletePath(strXMLPath, bAppendLastSlash, bCreate);

}

// directory principale per import/export in esecuzione
// .\XML\<Import|Export>\Pending\<site>
//----------------------------------------------------------------------------------------------
CString GetXMLPendingSitePath(BOOL bIsForImport, const CString& strSiteName, BOOL bAppendLastSlash /*=TRUE*/, BOOL bCreate /*= TRUE*/)
{
	CString strXMLPath = GetXMLPendingPath(bIsForImport, TRUE, bCreate);
	if (strXMLPath.IsEmpty())
		return _T("");
	
	strXMLPath +=  strSiteName;

	return CompletePath(strXMLPath, bAppendLastSlash, bCreate);

}

// directory principale per import/export falliti
// .\XmlData\<Import|Export>\Failure
//----------------------------------------------------------------------------------------------
CString GetXMLFailurePath(BOOL bIsForImport, BOOL bAppendLastSlash /*=TRUE*/, BOOL bCreate /*= TRUE*/)
{
	CString strXMLPath = bIsForImport ? 
		GetXMLImportPath(TRUE, bCreate):
		GetXMLExportPath (TRUE, bCreate);

	if (strXMLPath.IsEmpty())
		return _T("");
	
	strXMLPath += szXMLFailure;

	return CompletePath(strXMLPath, bAppendLastSlash, bCreate);

}

// directory principale per import/export falliti del singolo site
// .\XML\<Import|Export>\Failure\<site>
//----------------------------------------------------------------------------------------------
CString GetXMLFailureSitePath(BOOL bIsForImport, const CString& strSiteName, BOOL bAppendLastSlash /*=TRUE*/, BOOL bCreate /*= TRUE*/)
{
	CString strXMLPath = GetXMLFailurePath(bIsForImport, TRUE, bCreate);
	if (strXMLPath.IsEmpty())
		return _T("");
	
	strXMLPath +=  strSiteName;

	return CompletePath(strXMLPath, bAppendLastSlash, bCreate);

}

// directory principale per import/export a buon fine
// .\XmlData\<Import|Export>\Success
//----------------------------------------------------------------------------------------------
CString GetXMLSuccessPath(BOOL bIsForImport, BOOL bAppendLastSlash /*=TRUE*/, BOOL bCreate /*= TRUE*/)
{
	CString strXMLPath = bIsForImport ? 
		GetXMLImportPath(TRUE, bCreate):
		GetXMLExportPath (TRUE, bCreate);

	if (strXMLPath.IsEmpty())
		return _T("");
	
	strXMLPath += szXMLSuccess;

	return CompletePath(strXMLPath, bAppendLastSlash, bCreate);

}

// directory principale per import/export a buon fine
// .\XML\<Import|Export>\Success\<site>
//----------------------------------------------------------------------------------------------
CString GetXMLSuccessSitePath(BOOL bIsForImport, const CString& strSiteName, BOOL bAppendLastSlash /*=TRUE*/, BOOL bCreate /*= TRUE*/)
{
	CString strXMLPath = GetXMLSuccessPath(bIsForImport, TRUE, bCreate);
	if (strXMLPath.IsEmpty())
		return _T("");
	
	strXMLPath +=  strSiteName;

	return CompletePath(strXMLPath, bAppendLastSlash, bCreate);

}

// directory principale per import/export parzialmente a buon fine
// .\XmlData\<Import|Export>\PartialSuccess
//----------------------------------------------------------------------------------------------
CString GetXMLPartialSuccessPath(BOOL bIsForImport, BOOL bAppendLastSlash /*=TRUE*/, BOOL bCreate /*= TRUE*/)
{
	CString strXMLPath = bIsForImport ? 
		GetXMLImportPath(TRUE, bCreate):
		GetXMLExportPath (TRUE, bCreate);

	if (strXMLPath.IsEmpty())
		return _T("");
	
	strXMLPath += szXMLPartialSuccess;

	return CompletePath(strXMLPath, bAppendLastSlash, bCreate);

}

// directory principale per import/export parzialmente a buon fine
// .\XML\<Import|Export>\PartialSuccess\<site>
//----------------------------------------------------------------------------------------------
CString GetXMLPartialSuccessSitePath(BOOL bIsForImport, const CString& strSiteName, BOOL bAppendLastSlash /*=TRUE*/, BOOL bCreate /*= TRUE*/)
{
	CString strXMLPath = GetXMLPartialSuccessPath(bIsForImport, TRUE, bCreate);
	if (strXMLPath.IsEmpty())
		return _T("");
	
	strXMLPath +=  strSiteName;

	return CompletePath(strXMLPath, bAppendLastSlash, bCreate);

}

//---------------------------------------------------------------------------
CString MakeSchemaName(const CString& strFileName) 
{
	return ::GetName(strFileName) + szXsdExt;
}

//----------------------------------------------------------------
CString GetXMLSchemasPath()
{
	return AfxGetPathFinder()->GetTaskBuilderXmlPath();
}