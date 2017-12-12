
#pragma once

#include <tbgeneric\dataobj.h>

#include "beginh.dex"

TB_EXPORT void ExtractMessages(CDiagnostic* pDiagnostic, DataArray& messages, DataArray& types);

TB_EXPORT DataInt				GetLogins();
TB_EXPORT void					SetUserInteractionMode(DataInt mode);
TB_EXPORT void					UseRemoteInterface(DataBool set);
TB_EXPORT DataBool				Login  (DataStr authenticationToken);
TB_EXPORT DataLng				GetProcessID();
TB_EXPORT DataStr				GetActiveThreads();
TB_EXPORT DataStr				GetLoginActiveThreads();
TB_EXPORT void					KillThread(DataLng ThreadId);
TB_EXPORT DataBool				StopThread(DataLng ThreadId);
TB_EXPORT DataBool				CanStopThread(DataLng ThreadId);
TB_EXPORT DataArray				GetDocumentThreads();
TB_EXPORT void					SetMenuHandle(DataLng menuWindowHandle);
TB_EXPORT DataBool				SetDocumentInForeground(DataLng documentHandle);
TB_EXPORT DataBool				GetCurrentUser(DataStr& strUser, DataStr& strCompany);
TB_EXPORT DataLng				RunDocument(DataStr documentNamespace, DataStr arguments);
TB_EXPORT DataLng  				RunReport(DataStr reportNamespace, DataStr arguments);
TB_EXPORT DataBool				RunFunction(DataStr functionNamespace, DataStr arguments);
TB_EXPORT void					RunFunctionInNewThread(DataStr/*[ciString]*/ functionNamespace, DataStr arguments);
TB_EXPORT DataBool				RunEditor(DataStr functionNamespace);
TB_EXPORT DataLng				RunReportFromWoormInfo(DataLng woormInfoHandle, DataStr arguments);
TB_EXPORT DataBool				CloseDocument(DataLng documentHandle);
TB_EXPORT DataBool				RunIconizedDocument(DataLng documentHandle);
TB_EXPORT DataBool				CanCloseDocument(DataLng documentHandle);
TB_EXPORT DataBool				ExistDocument(DataLng documentHandle);
TB_EXPORT DataBool				DestroyDocument(DataLng documentHandle);
TB_EXPORT DataBool				CloseAllDocuments();
TB_EXPORT DataObjArray/*[long]*/	GetOpenDocuments();
TB_EXPORT void					GetApplicationContextMessages(DataBool clearMessages, DataArray&/*[string]*/ messages, DataArray&/*integer*/ types);
TB_EXPORT void					GetLoginContextMessages(DataBool clearMessages, DataArray&/*[string]*/ messages, DataArray&/*integer*/ types);
TB_EXPORT DataBool				OnBeforeCanCloseTB();
TB_EXPORT DataBool				CanCloseTB();
TB_EXPORT DataBool				CanCloseLogin();
TB_EXPORT DataBool				IsLoginValid();
TB_EXPORT void					CloseLogin();
TB_EXPORT void					CloseTB();
TB_EXPORT void					DestroyTB();
TB_EXPORT void					AdminDocumentReports();
TB_EXPORT void					AdminDocumentProfiles();
TB_EXPORT void					XMLDocumentDescription();
TB_EXPORT DataStr				GetHotlinkQuery	(DataStr hotLinkNamespace, DataStr arguments, DataInt action);
TB_EXPORT DataBool				GetDocumentParameters(DataStr documentNamespace, DataStr& xmlParams, DataStr code);
TB_EXPORT DataBool				GetReportParameters(DataStr reportNamespace, DataStr& xmlParams, DataStr code);
TB_EXPORT DataBool				RunBatchInUnattendedMode(DataStr documentNamespace, DataStr xmlParams, DataLng& documentHandle, DataObjArray& messages );
TB_EXPORT DataBool				RunReportInUnattendedMode(DataLng woormInfoHandle, DataStr xmlParams, DataLng& reportHandle, DataObjArray& messages );

TB_EXPORT DataDate				GetApplicationDate();
TB_EXPORT DataInt				GetApplicationYear();
TB_EXPORT DataInt				GetApplicationMonth();
TB_EXPORT DataInt				GetApplicationDay();

TB_EXPORT DataBool				EnableSoapFunctionExecutionControl(DataBool enable);

TB_EXPORT DataBool				CanChangeLogin	(DataBool bLock);
TB_EXPORT DataInt				ChangeLogin		(DataStr oldAuthenticationToken, DataStr newAuthenticationToken, DataBool bUnLock);
TB_EXPORT DataBool				IsTBLocked		();
TB_EXPORT DataBool				LockTB			(DataStr authenticationToken);
TB_EXPORT DataBool				UnLockTB		(DataStr authenticationToken);
TB_EXPORT void					ClearCache		();

TB_EXPORT DataInt				DisconnectCompany	(DataStr authenticationToken);
TB_EXPORT DataInt				ReconnectCompany	(DataStr authenticationToken);

void RunFunctionProcedure		(DataStr functionNamespace, DataStr arguments);

#include "endh.dex"