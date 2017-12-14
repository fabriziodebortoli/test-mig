#pragma once

#include "beginh.dex"

//---------------------------------------------------------------------------
TB_EXPORT BOOL UncompressFile(const CString & strFileIn, const CString & strFileOut, CString* sError = NULL);
TB_EXPORT BOOL CompressFile(const CString& strFileIn, const CString& strFileOut, const CString& strTitle, CString* sError = NULL);
TB_EXPORT BOOL UncompressFolder(const CString & strPath, const CString & strOutputPath, CString* sError = NULL);
TB_EXPORT BOOL CompressFolder(const CString& strPath, const CString& strFileOut, BOOL bRecursive = TRUE, const CString& strRelativePathFrom = _T(""), CString* sError = NULL);


//---------------------------------------------------------------------------
TB_EXPORT BOOL UncompressFileV2(const CString & strFileIn, const CString & strFileOut, CString* sError = NULL);
TB_EXPORT BOOL CompressFileV2(const CString& strFileIn, const CString& strFileOut, const CString& strTitle, CString* sError = NULL);
TB_EXPORT BOOL UncompressFolderV2(const CString & strPath, const CString & strOutputPath, CString* sError = NULL);
TB_EXPORT BOOL CompressFolderV2(const CString& strPath, const CString& strFileOut, BOOL bRecursive = TRUE, const CString& strRelativePathFrom = _T(""), CString* sError = NULL);

#include "endh.dex"
