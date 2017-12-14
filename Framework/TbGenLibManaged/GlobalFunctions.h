#pragma once

#include "beginh.dex"

TB_EXPORT BOOL LoadInstalledLanguages (CStringArray& names, CStringArray& descriptions);
TB_EXPORT CString ConvertToBase64 (const CString& sFileName);
TB_EXPORT CString HTTPGet(const CString& sUrl);
TB_EXPORT bool CallDynamicRowFormView(HWND hwndOwner, void* pDocument, void* pDBT);
TB_EXPORT CString OpenCrsFile(const CString& sFileName);
TB_EXPORT CString StringFormat(CString formatString, CStringArray& args);
TB_EXPORT CString ConvertToBase64Str(const CString& cmd);
TB_EXPORT CString ConvertToAESStr(const CString& cmd);
#include "endh.dex"
