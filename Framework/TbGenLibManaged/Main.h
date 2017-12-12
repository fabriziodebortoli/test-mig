#pragma once

#include "stdafx.h"

class CBaseDescriptionArray;
class CFunctionDescription;
class IHostApplication;

#include <TbGeneric/DataObj.h>

#include "beginh.dex"

TB_EXPORT void InitAssembly();
TB_EXPORT void FreeAssemblyObjects();
TB_EXPORT bool WCFServicesCreationNeeded();
TB_EXPORT void DeleteWCFServicesAssembly();
TB_EXPORT void InitTimer();
TB_EXPORT bool CopyTBLinkToClipboard(const CString& tblink);
TB_EXPORT bool ApplicationLock();

TB_EXPORT void GenerateEasyBuilderEnumsDllAsync();

TB_EXPORT void CreateWCFServices(const CBaseDescriptionArray &functions, int soapPort, int tcpPort, const CString& strUserForRegisteringNamespaces, int startPortForRegisteringNamespaces, IHostApplication* pHost);
TB_EXPORT void StartWCFServices(int soapPort, int tcpPort, IHostApplication* pHost);
TB_EXPORT void StopWCFServices();
TB_EXPORT void SetWebServicesTimeout(int nTimeout);
TB_EXPORT bool InvokeWCFInternalFunction(CFunctionDescription* pFunctionDescription);
TB_EXPORT bool InvokeWCFExternalFunction(CFunctionDescription* pFunctionDescription, bool forceAssemblyCreation = false);
TB_EXPORT bool InvokeWCFFunction(CFunctionDescription* pFunctionDescription, BOOL bInternalCall);
TB_EXPORT bool EnableSoapExecutionControl(bool bEnable);
TB_EXPORT void GetServerInstallationInfo(CString& strFileServer, CString& strWebServer, CString &strInstallation, CString &strMasterSolutionName);

TB_EXPORT int GetTbLoaderSOAPPort ();
TB_EXPORT int GetTbLoaderTCPPort ();
TB_EXPORT void SetAdminAuthenticationToken(const CString& strToken);
TB_EXPORT bool SetUseExpect100ContinueInWCFCalls(bool bSet);
TB_EXPORT DataDate GetInstallationDate();
TB_EXPORT void InitThreadCulture();
TB_EXPORT void ApplicationDoEvents();
TB_EXPORT void ApplicationRaiseIdle();
TB_EXPORT void ApplicationRun(CWinThread* pThread);
TB_EXPORT bool ApplicationFilterMessage(LPMSG lpMsg);

TB_EXPORT void PushToClients(const CString& sClientId, const CString& sMessage);
TB_EXPORT void MakeTransparent(const BYTE* sourceBuffer, int nSourceSize, COLORREF transparentColor, BYTE*& buffer, int& nSize );
TB_EXPORT int GetWebSocketsConnectorPort();        
#include "endh.dex"