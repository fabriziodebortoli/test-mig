
#pragma once

//includere alla fine degli include del .H
#include "beginh.dex"

// definizione delle funzioni standard predefinite
//=============================================================================

// funzioni relative al MailConnector
#define	IDF_WOORM_SETFROM							_T("MailSetFrom")
#define	IDF_WOORM_SETSUBJECT						_T("MailSetSubject")
#define	IDF_WOORM_SETBODY							_T("MailSetBody")
#define	IDF_WOORM_SETMAPIPROFILE					_T("MailSetMapiProfile")
#define	IDF_WOORM_SETTEMPLATEFILENAME				_T("MailSetBodyTemplateFileName")
#define	IDF_WOORM_SETTOCERTIFIED					_T("MailSetToCertified")
#define	IDF_WOORM_SETTO								_T("MailSetTo")
#define	IDF_WOORM_SETCC								_T("MailSetCc")
#define	IDF_WOORM_SETBCC							_T("MailSetBcc")
#define	IDF_WOORM_SETATTACHMENT						_T("MailSetAttachment")
#define	IDF_WOORM_SETBODYPARAMETER					_T("MailSetBodyParameter")
#define	IDF_WOORM_MAILSEND							_T("MailSend")
#define	IDF_WOORM_MAILSEND_EX						_T("MailSendEx")
#define	IDF_WOORM_SETATTACHMENTREPORTNAME			_T("MailSetAttachmentReportName")
#define	IDF_WOORM_SETCERTIFIEDFILTER				_T("MailSetCertifiedFilter")

#define	IDF_WOORM_POSTALITE_SETADDRESSEE			_T("PostaLiteSetAddressee")
#define	IDF_WOORM_POSTALITE_SETSENDTYPE				_T("PostaLiteSetSendType")
#define	IDF_WOORM_POSTALITE_SEND					_T("PostaLiteSend")

#define	IDF_WOORM_REPORT_SAVE_AS					_T("ReportSaveAs")
#define	IDF_WOORM_REPORT_SAVE_AS_AND_ATTACH			_T("ReportSaveAsAndAttach")
#define	IDF_WOORM_ARCHIVE_REPORT					_T("ArchiveReport")
#define	IDF_WOORM_GENERATE_DMS_PAPERY				_T("GeneratePapery")

#define	IDF_WOORM_SEND_MAIL							_T("SendMail")

#define	IDF_WOORM_RUNREPORT							_T("RunReport")
#define	IDF_WOORM_RUNPROGRAM						_T("RunProgram")
#define	IDF_WOORM_RUNDOCUMENT						_T("RunDocument")
#define	IDF_WOORM_BROWSEDOCUMENT					_T("BrowseDocument")
#define	IDF_WOORM_CLOSEDOCUMENT						_T("CloseDocument")
#define	IDF_WOORM_EXPANDTEMPLATE					_T("ExpandTemplate")

#define	IDF_WOORM_GETREPORTPATH						_T("GetReportPath")
#define	IDF_WOORM_GETREPORTNAME						_T("GetReportName")
#define	IDF_WOORM_GETREPORTNAMESPACE				_T("GetReportNamespace")
#define	IDF_WOORM_GETREPORTMODULENAMESPACE			_T("GetReportModuleNamespace")
#define	IDF_WOORM_GETOWNERNAMESPACE					_T("GetOwnerNamespace")
#define	IDF_WOORM_GETPRINTERNAME					_T("GetPrinterName")
#define	IDF_WOORM_SETPRINTERNAME					_T("SetPrinterName")
#define	IDF_WOORM_ISAUTOPRINT						_T("IsAutoPrint")
#define	IDF_WOORM_UpdateOutputParametersEvenIfReportDoesNotFetchRecords _T("UpdateOutputParametersEvenIfReportDoesNotFetchRecords")
#define	IDF_WOORM_SKIPCONTEXT						_T("SkipContext")

#define	IDF_WOORM_GetTableRows						_T("GetTableRows")
#define	IDF_WOORM_GetTableCurrentRow				_T("GetTableCurrentRow")
#define	IDF_WOORM_CurrentRowContainsCellTail		_T("CurrentRowContainsCellTail")
#define	IDF_WOORM_CurrentRowContainsCellSubTotal	_T("CurrentRowContainsCellSubTotal")
#define	IDF_WOORM_CurrentRowIsEmpty					_T("CurrentRowIsEmpty")
#define	IDF_WOORM_GetRows							_T("GetRows")
#define	IDF_WOORM_GetCurrentRow						_T("GetCurrentRow")

#define	IDF_WOORM_QueryOpen 						_T("QueryOpen")
#define	IDF_WOORM_QueryClose 						_T("QueryClose")
#define	IDF_WOORM_QueryRead		 					_T("QueryRead")
#define	IDF_WOORM_QueryReadOne		 				_T("QueryReadOne")
#define	IDF_WOORM_QueryExecute 						_T("QueryExecute")
#define	IDF_WOORM_QueryCall							_T("QueryCall")
#define	IDF_WOORM_QueryIsOpen 						_T("QueryIsOpen")

#define	IDF_WOORM_QuerySetConnection		 		_T("QuerySetConnection")
#define	IDF_WOORM_QuerySetCursorType		 		_T("QuerySetCursorType")

#define	IDF_WOORM_GetConnection		 				_T("GetConnection")
#define	IDF_WOORM_OpenConnection		 			_T("OpenConnection")
#define	IDF_WOORM_CloseConnection		 			_T("CloseConnection")

#define	IDF_WOORM_ConnectionBeginTrans		 		_T("ConnectionBeginTrans")
#define	IDF_WOORM_ConnectionCommit		 			_T("ConnectionCommit")
#define	IDF_WOORM_ConnectionRollback		 		_T("ConnectionRollback")

#define	IDF_WOORM_ConnectionLockRecord		 		_T("ConnectionLockRecord")
#define	IDF_WOORM_ConnectionUnlockRecord		 	_T("ConnectionUnlockRecord")
#define	IDF_WOORM_ConnectionUnlockAll		 		_T("ConnectionUnlockAll")

#define	IDF_WOORM_QueryIsEof 						_T("QueryIsEof")
#define	IDF_WOORM_QueryIsFailed 					_T("QueryIsFailed")
#define	IDF_WOORM_QueryGetErrorInfo					_T("QueryGetErrorInfo")
#define	IDF_WOORM_QueryGetSqlString					_T("QueryGetSqlString")
// con WCF interface
#define	IDF_WOORM_GetCompanyInfo					_T("GetCompanyInfo")

#define IDF_WOORM_GROUP_MAIL						_T("Mail")
#define IDF_WOORM_GROUP_POSTALITE					_T("PostaLite")
#define IDF_WOORM_GROUP_MINIHTML					_T("MiniHtml")
#define IDF_WOORM_GROUP_ADVANCED					_T("Advanced")
//=======================================================================================
#include "endh.dex"
