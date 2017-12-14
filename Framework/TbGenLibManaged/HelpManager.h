#pragma once

#include "beginh.dex"

TB_EXPORT BOOL		ShowHelp							(const CString& strNamespace = _T(""));
TB_EXPORT CString	GetOnlineHelpLink					(const CString& strNamespace, bool fromEasyLook = false);
TB_EXPORT void		ConnectToProducerSite				();
TB_EXPORT void		ConnectToProducerSiteLoginPage		();	
TB_EXPORT void		ConnectToProducerSitePrivateArea	();

#include "endh.dex"