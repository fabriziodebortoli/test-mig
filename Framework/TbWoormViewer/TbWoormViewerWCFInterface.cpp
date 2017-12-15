//BEGIN_HEADING - file version: 1.0
//WARNING - automatically generated code - DO NOT EDIT THIS FILE!

#include "stdafx.h"

//BEGIN_SOURCE_INCLUDE
#include	"externalfunctionstestsuite.h"
#include	"woormdoc.h"
#include	"soapfunctions.h"
//END_SOURCE_INCLUDE

#include <atlsafe.h>
#include <TbGeneric\WebServiceStateObjects.h>
#include <TBNameSolver\Templates.h>
//END_HEADING


namespace TbWoormViewerTbWoormViewer {
	//File: Framework\TbWoormViewer\WOORMDOC.CPP.
	extern "C" __declspec(dllexport) long __CWoormInfo_Create(HWND _hwnd, BSTR reportNamespace) throw(...)
	{
		if (_hwnd != GetThreadMainWnd())
			return AfxInvokeThreadGlobalFunction<long, HWND, BSTR>(_hwnd, &__CWoormInfo_Create, _hwnd, reportNamespace);
		DataStr reportNamespaceParam;
		reportNamespaceParam.SetCollateCultureSensitive(TRUE);
		reportNamespaceParam.SetSoapValue(reportNamespace);
		CWoormInfo* pObj = new CWoormInfo(reportNamespaceParam);
		AfxAddWebServiceStateObject(pObj);
		return (long)pObj;
	}
	//File: Framework\TbWoormViewer\WOORMDOC.CPP.
	extern "C" __declspec(dllexport) bool __CWoormInfo_Dispose(HWND _hwnd, long handle) throw(...)
	{
		if (_hwnd != GetThreadMainWnd())
			return AfxInvokeThreadGlobalFunction<bool, HWND, long>(_hwnd, &__CWoormInfo_Dispose, _hwnd, handle);
		if (!AfxExistWebServiceStateObject((CObject*)handle))
		{
			return false;
		}
		AfxRemoveWebServiceStateObject((CObject*)handle, TRUE);
		return true;
	}
	//File: Framework\TbWoormViewer\WOORMDOC.CPP.
	extern "C" __declspec(dllexport) void __CWoormInfo_AddReport(HWND _hwnd, long handle, BSTR reportNamespace) throw(...)
	{
		DataStr reportNamespaceParam;
		reportNamespaceParam.SetCollateCultureSensitive(TRUE);
		reportNamespaceParam.SetSoapValue(reportNamespace);
		CObject* pObj = ((CObject*)handle);
		if (!pObj->IsKindOf(RUNTIME_CLASS(CWoormInfo)))
			throw new CApplicationErrorException(_T("Invalid context object: CWoormInfo"));
		CWoormInfo& obj = *((CWoormInfo*)pObj);
		AfxInvokeThreadProcedure<CWoormInfo, DataStr>(_hwnd, &obj, &CWoormInfo::AddReport, reportNamespaceParam);
	}
	//File: Framework\TbWoormViewer\WOORMDOC.CPP.
	extern "C" __declspec(dllexport) void __CWoormInfo_SetAutoPrint(HWND _hwnd, long handle, bool autoPrint) throw(...)
	{
		DataBool autoPrintParam;
		autoPrintParam.SetSoapValue(autoPrint);
		CObject* pObj = ((CObject*)handle);
		if (!pObj->IsKindOf(RUNTIME_CLASS(CWoormInfo)))
			throw new CApplicationErrorException(_T("Invalid context object: CWoormInfo"));
		CWoormInfo& obj = *((CWoormInfo*)pObj);
		AfxInvokeThreadProcedure<CWoormInfo, DataBool>(_hwnd, &obj, &CWoormInfo::SetAutoPrint, autoPrintParam);
	}
	//File: Framework\TbWoormViewer\WOORMDOC.CPP.
	extern "C" __declspec(dllexport) void __CWoormInfo_SetNoPrintDialog(HWND _hwnd, long handle, bool noPrintDialog) throw(...)
	{
		DataBool noPrintDialogParam;
		noPrintDialogParam.SetSoapValue(noPrintDialog);
		CObject* pObj = ((CObject*)handle);
		if (!pObj->IsKindOf(RUNTIME_CLASS(CWoormInfo)))
			throw new CApplicationErrorException(_T("Invalid context object: CWoormInfo"));
		CWoormInfo& obj = *((CWoormInfo*)pObj);
		AfxInvokeThreadProcedure<CWoormInfo, DataBool>(_hwnd, &obj, &CWoormInfo::SetNoPrintDialog, noPrintDialogParam);
	}
	//File: Framework\TbWoormViewer\WOORMDOC.CPP.
	extern "C" __declspec(dllexport) void __CWoormInfo_SetCloseOnEndPrint(HWND _hwnd, long handle, bool closeOnEndPrint) throw(...)
	{
		DataBool closeOnEndPrintParam;
		closeOnEndPrintParam.SetSoapValue(closeOnEndPrint);
		CObject* pObj = ((CObject*)handle);
		if (!pObj->IsKindOf(RUNTIME_CLASS(CWoormInfo)))
			throw new CApplicationErrorException(_T("Invalid context object: CWoormInfo"));
		CWoormInfo& obj = *((CWoormInfo*)pObj);
		AfxInvokeThreadProcedure<CWoormInfo, DataBool>(_hwnd, &obj, &CWoormInfo::SetCloseOnEndPrint, closeOnEndPrintParam);
	}
	//File: Framework\TbWoormViewer\WOORMDOC.CPP.
	extern "C" __declspec(dllexport) void __CWoormInfo_SetIconized(HWND _hwnd, long handle, bool iconized) throw(...)
	{
		DataBool iconizedParam;
		iconizedParam.SetSoapValue(iconized);
		CObject* pObj = ((CObject*)handle);
		if (!pObj->IsKindOf(RUNTIME_CLASS(CWoormInfo)))
			throw new CApplicationErrorException(_T("Invalid context object: CWoormInfo"));
		CWoormInfo& obj = *((CWoormInfo*)pObj);
		AfxInvokeThreadProcedure<CWoormInfo, DataBool>(_hwnd, &obj, &CWoormInfo::SetIconized, iconizedParam);
	}
	//File: Framework\TbWoormViewer\WOORMDOC.CPP.
	extern "C" __declspec(dllexport) void __CWoormInfo_SetHideFrame(HWND _hwnd, long handle, bool hideFrame) throw(...)
	{
		DataBool hideFrameParam;
		hideFrameParam.SetSoapValue(hideFrame);
		CObject* pObj = ((CObject*)handle);
		if (!pObj->IsKindOf(RUNTIME_CLASS(CWoormInfo)))
			throw new CApplicationErrorException(_T("Invalid context object: CWoormInfo"));
		CWoormInfo& obj = *((CWoormInfo*)pObj);
		AfxInvokeThreadProcedure<CWoormInfo, DataBool>(_hwnd, &obj, &CWoormInfo::SetHideFrame, hideFrameParam);
	}
	//File: Framework\TbWoormViewer\WOORMDOC.CPP.
	extern "C" __declspec(dllexport) void __CWoormInfo_SetSilentMode(HWND _hwnd, long handle, bool silent) throw(...)
	{
		DataBool silentParam;
		silentParam.SetSoapValue(silent);
		CObject* pObj = ((CObject*)handle);
		if (!pObj->IsKindOf(RUNTIME_CLASS(CWoormInfo)))
			throw new CApplicationErrorException(_T("Invalid context object: CWoormInfo"));
		CWoormInfo& obj = *((CWoormInfo*)pObj);
		AfxInvokeThreadProcedure<CWoormInfo, DataBool>(_hwnd, &obj, &CWoormInfo::SetSilentMode, silentParam);
	}
	//File: Framework\TbWoormViewer\WOORMDOC.CPP.
	extern "C" __declspec(dllexport) void __CWoormInfo_SetSendEmail(HWND _hwnd, long handle, bool sendEmail) throw(...)
	{
		DataBool sendEmailParam;
		sendEmailParam.SetSoapValue(sendEmail);
		CObject* pObj = ((CObject*)handle);
		if (!pObj->IsKindOf(RUNTIME_CLASS(CWoormInfo)))
			throw new CApplicationErrorException(_T("Invalid context object: CWoormInfo"));
		CWoormInfo& obj = *((CWoormInfo*)pObj);
		AfxInvokeThreadProcedure<CWoormInfo, DataBool>(_hwnd, &obj, &CWoormInfo::SetSendEmail, sendEmailParam);
	}
	//File: Framework\TbWoormViewer\WOORMDOC.CPP.
	extern "C" __declspec(dllexport) void __CWoormInfo_SetAttachRDE(HWND _hwnd, long handle, bool attachRDE) throw(...)
	{
		DataBool attachRDEParam;
		attachRDEParam.SetSoapValue(attachRDE);
		CObject* pObj = ((CObject*)handle);
		if (!pObj->IsKindOf(RUNTIME_CLASS(CWoormInfo)))
			throw new CApplicationErrorException(_T("Invalid context object: CWoormInfo"));
		CWoormInfo& obj = *((CWoormInfo*)pObj);
		AfxInvokeThreadProcedure<CWoormInfo, DataBool>(_hwnd, &obj, &CWoormInfo::SetAttachRDE, attachRDEParam);
	}
	//File: Framework\TbWoormViewer\WOORMDOC.CPP.
	extern "C" __declspec(dllexport) void __CWoormInfo_SetAttachPDF(HWND _hwnd, long handle, bool attachPDF) throw(...)
	{
		DataBool attachPDFParam;
		attachPDFParam.SetSoapValue(attachPDF);
		CObject* pObj = ((CObject*)handle);
		if (!pObj->IsKindOf(RUNTIME_CLASS(CWoormInfo)))
			throw new CApplicationErrorException(_T("Invalid context object: CWoormInfo"));
		CWoormInfo& obj = *((CWoormInfo*)pObj);
		AfxInvokeThreadProcedure<CWoormInfo, DataBool>(_hwnd, &obj, &CWoormInfo::SetAttachPDF, attachPDFParam);
	}
	//File: Framework\TbWoormViewer\WOORMDOC.CPP.
	extern "C" __declspec(dllexport) void __CWoormInfo_SetAttachOther(HWND _hwnd, long handle, bool attachOther, BSTR expType) throw(...)
	{
		DataBool attachOtherParam;
		attachOtherParam.SetSoapValue(attachOther);
		DataStr expTypeParam;
		expTypeParam.SetCollateCultureSensitive(TRUE);
		expTypeParam.SetSoapValue(expType);
		CObject* pObj = ((CObject*)handle);
		if (!pObj->IsKindOf(RUNTIME_CLASS(CWoormInfo)))
			throw new CApplicationErrorException(_T("Invalid context object: CWoormInfo"));
		CWoormInfo& obj = *((CWoormInfo*)pObj);
		AfxInvokeThreadProcedure<CWoormInfo, DataBool, DataStr>(_hwnd, &obj, &CWoormInfo::SetAttachOther, attachOtherParam, expTypeParam);
	}
	//File: Framework\TbWoormViewer\WOORMDOC.CPP.
	extern "C" __declspec(dllexport) void __CWoormInfo_SetArchivePdfFormat(HWND _hwnd, long handle, bool archive) throw(...)
	{
		DataBool archiveParam;
		archiveParam.SetSoapValue(archive);
		CObject* pObj = ((CObject*)handle);
		if (!pObj->IsKindOf(RUNTIME_CLASS(CWoormInfo)))
			throw new CApplicationErrorException(_T("Invalid context object: CWoormInfo"));
		CWoormInfo& obj = *((CWoormInfo*)pObj);
		AfxInvokeThreadProcedure<CWoormInfo, DataBool>(_hwnd, &obj, &CWoormInfo::SetArchivePdfFormat, archiveParam);
	}
	//File: Framework\TbWoormViewer\WOORMDOC.CPP.
	extern "C" __declspec(dllexport) void __CWoormInfo_SetCompressAttach(HWND _hwnd, long handle, bool compressAttach) throw(...)
	{
		DataBool compressAttachParam;
		compressAttachParam.SetSoapValue(compressAttach);
		CObject* pObj = ((CObject*)handle);
		if (!pObj->IsKindOf(RUNTIME_CLASS(CWoormInfo)))
			throw new CApplicationErrorException(_T("Invalid context object: CWoormInfo"));
		CWoormInfo& obj = *((CWoormInfo*)pObj);
		AfxInvokeThreadProcedure<CWoormInfo, DataBool>(_hwnd, &obj, &CWoormInfo::SetCompressAttach, compressAttachParam);
	}
	//File: Framework\TbWoormViewer\WOORMDOC.CPP.
	extern "C" __declspec(dllexport) void __CWoormInfo_SetOnePrintDialog(HWND _hwnd, long handle, bool onePrintDialog) throw(...)
	{
		DataBool onePrintDialogParam;
		onePrintDialogParam.SetSoapValue(onePrintDialog);
		CObject* pObj = ((CObject*)handle);
		if (!pObj->IsKindOf(RUNTIME_CLASS(CWoormInfo)))
			throw new CApplicationErrorException(_T("Invalid context object: CWoormInfo"));
		CWoormInfo& obj = *((CWoormInfo*)pObj);
		AfxInvokeThreadProcedure<CWoormInfo, DataBool>(_hwnd, &obj, &CWoormInfo::SetOnePrintDialog, onePrintDialogParam);
	}
	//File: Framework\TbWoormViewer\WOORMDOC.CPP.
	extern "C" __declspec(dllexport) void __CWoormInfo_SetUniqueMail(HWND _hwnd, long handle, bool uniqueMail) throw(...)
	{
		DataBool uniqueMailParam;
		uniqueMailParam.SetSoapValue(uniqueMail);
		CObject* pObj = ((CObject*)handle);
		if (!pObj->IsKindOf(RUNTIME_CLASS(CWoormInfo)))
			throw new CApplicationErrorException(_T("Invalid context object: CWoormInfo"));
		CWoormInfo& obj = *((CWoormInfo*)pObj);
		AfxInvokeThreadProcedure<CWoormInfo, DataBool>(_hwnd, &obj, &CWoormInfo::SetUniqueMail, uniqueMailParam);
	}
	//File: Framework\TbWoormViewer\WOORMDOC.CPP.
	extern "C" __declspec(dllexport) void __CWoormInfo_SetConcatPDF(HWND _hwnd, long handle, bool concatPDF) throw(...)
	{
		DataBool concatPDFParam;
		concatPDFParam.SetSoapValue(concatPDF);
		CObject* pObj = ((CObject*)handle);
		if (!pObj->IsKindOf(RUNTIME_CLASS(CWoormInfo)))
			throw new CApplicationErrorException(_T("Invalid context object: CWoormInfo"));
		CWoormInfo& obj = *((CWoormInfo*)pObj);
		AfxInvokeThreadProcedure<CWoormInfo, DataBool>(_hwnd, &obj, &CWoormInfo::SetConcatPDF, concatPDFParam);
	}
	//File: Framework\TbWoormViewer\WOORMDOC.CPP.
	extern "C" __declspec(dllexport) void __CWoormInfo_SetPDFOutput(HWND _hwnd, long handle, bool PDFOutput) throw(...)
	{
		DataBool PDFOutputParam;
		PDFOutputParam.SetSoapValue(PDFOutput);
		CObject* pObj = ((CObject*)handle);
		if (!pObj->IsKindOf(RUNTIME_CLASS(CWoormInfo)))
			throw new CApplicationErrorException(_T("Invalid context object: CWoormInfo"));
		CWoormInfo& obj = *((CWoormInfo*)pObj);
		AfxInvokeThreadProcedure<CWoormInfo, DataBool>(_hwnd, &obj, &CWoormInfo::SetPDFOutput, PDFOutputParam);
	}
	//File: Framework\TbWoormViewer\WOORMDOC.CPP.
	extern "C" __declspec(dllexport) void __CWoormInfo_SetPDFOutputPreview(HWND _hwnd, long handle, bool PDFOutputPreview) throw(...)
	{
		DataBool PDFOutputPreviewParam;
		PDFOutputPreviewParam.SetSoapValue(PDFOutputPreview);
		CObject* pObj = ((CObject*)handle);
		if (!pObj->IsKindOf(RUNTIME_CLASS(CWoormInfo)))
			throw new CApplicationErrorException(_T("Invalid context object: CWoormInfo"));
		CWoormInfo& obj = *((CWoormInfo*)pObj);
		AfxInvokeThreadProcedure<CWoormInfo, DataBool>(_hwnd, &obj, &CWoormInfo::SetPDFOutputPreview, PDFOutputPreviewParam);
	}
	//File: Framework\TbWoormViewer\WOORMDOC.CPP.
	extern "C" __declspec(dllexport) void __CWoormInfo_AddOutputFileName(HWND _hwnd, long handle, BSTR fileName) throw(...)
	{
		DataStr fileNameParam;
		fileNameParam.SetCollateCultureSensitive(TRUE);
		fileNameParam.SetSoapValue(fileName);
		CObject* pObj = ((CObject*)handle);
		if (!pObj->IsKindOf(RUNTIME_CLASS(CWoormInfo)))
			throw new CApplicationErrorException(_T("Invalid context object: CWoormInfo"));
		CWoormInfo& obj = *((CWoormInfo*)pObj);
		AfxInvokeThreadProcedure<CWoormInfo, DataStr>(_hwnd, &obj, &CWoormInfo::AddOutputFileName, fileNameParam);
	}
	//File: Framework\TbWoormViewer\WOORMDOC.CPP.
	extern "C" __declspec(dllexport) void __CWoormInfo_SetRDEOutput(HWND _hwnd, long handle, bool RDEOutput) throw(...)
	{
		DataBool RDEOutputParam;
		RDEOutputParam.SetSoapValue(RDEOutput);
		CObject* pObj = ((CObject*)handle);
		if (!pObj->IsKindOf(RUNTIME_CLASS(CWoormInfo)))
			throw new CApplicationErrorException(_T("Invalid context object: CWoormInfo"));
		CWoormInfo& obj = *((CWoormInfo*)pObj);
		AfxInvokeThreadProcedure<CWoormInfo, DataBool>(_hwnd, &obj, &CWoormInfo::SetRDEOutput, RDEOutputParam);
	}
	//File: Framework\TbWoormViewer\WOORMDOC.CPP.
	extern "C" __declspec(dllexport) void __CWoormInfo_SetExcelOutput(HWND _hwnd, long handle, bool ExcelOutput) throw(...)
	{
		DataBool ExcelOutputParam;
		ExcelOutputParam.SetSoapValue(ExcelOutput);
		CObject* pObj = ((CObject*)handle);
		if (!pObj->IsKindOf(RUNTIME_CLASS(CWoormInfo)))
			throw new CApplicationErrorException(_T("Invalid context object: CWoormInfo"));
		CWoormInfo& obj = *((CWoormInfo*)pObj);
		AfxInvokeThreadProcedure<CWoormInfo, DataBool>(_hwnd, &obj, &CWoormInfo::SetExcelOutput, ExcelOutputParam);
	}
	//File: Framework\TbWoormViewer\WOORMDOC.CPP.
	extern "C" __declspec(dllexport) void __CWoormInfo_MailTo(HWND _hwnd, long handle, BSTR to) throw(...)
	{
		DataStr toParam;
		toParam.SetCollateCultureSensitive(TRUE);
		toParam.SetSoapValue(to);
		CObject* pObj = ((CObject*)handle);
		if (!pObj->IsKindOf(RUNTIME_CLASS(CWoormInfo)))
			throw new CApplicationErrorException(_T("Invalid context object: CWoormInfo"));
		CWoormInfo& obj = *((CWoormInfo*)pObj);
		AfxInvokeThreadProcedure<CWoormInfo, DataStr>(_hwnd, &obj, &CWoormInfo::MailTo, toParam);
	}
	//File: Framework\TbWoormViewer\WOORMDOC.CPP.
	extern "C" __declspec(dllexport) void __CWoormInfo_MailCc(HWND _hwnd, long handle, BSTR cc) throw(...)
	{
		DataStr ccParam;
		ccParam.SetCollateCultureSensitive(TRUE);
		ccParam.SetSoapValue(cc);
		CObject* pObj = ((CObject*)handle);
		if (!pObj->IsKindOf(RUNTIME_CLASS(CWoormInfo)))
			throw new CApplicationErrorException(_T("Invalid context object: CWoormInfo"));
		CWoormInfo& obj = *((CWoormInfo*)pObj);
		AfxInvokeThreadProcedure<CWoormInfo, DataStr>(_hwnd, &obj, &CWoormInfo::MailCc, ccParam);
	}
	//File: Framework\TbWoormViewer\WOORMDOC.CPP.
	extern "C" __declspec(dllexport) void __CWoormInfo_MailBcc(HWND _hwnd, long handle, BSTR bcc) throw(...)
	{
		DataStr bccParam;
		bccParam.SetCollateCultureSensitive(TRUE);
		bccParam.SetSoapValue(bcc);
		CObject* pObj = ((CObject*)handle);
		if (!pObj->IsKindOf(RUNTIME_CLASS(CWoormInfo)))
			throw new CApplicationErrorException(_T("Invalid context object: CWoormInfo"));
		CWoormInfo& obj = *((CWoormInfo*)pObj);
		AfxInvokeThreadProcedure<CWoormInfo, DataStr>(_hwnd, &obj, &CWoormInfo::MailBcc, bccParam);
	}
	//File: Framework\TbWoormViewer\WOORMDOC.CPP.
	extern "C" __declspec(dllexport) void __CWoormInfo_MailAttach(HWND _hwnd, long handle, BSTR file, BSTR title) throw(...)
	{
		DataStr fileParam;
		fileParam.SetCollateCultureSensitive(TRUE);
		fileParam.SetSoapValue(file);
		DataStr titleParam;
		titleParam.SetCollateCultureSensitive(TRUE);
		titleParam.SetSoapValue(title);
		CObject* pObj = ((CObject*)handle);
		if (!pObj->IsKindOf(RUNTIME_CLASS(CWoormInfo)))
			throw new CApplicationErrorException(_T("Invalid context object: CWoormInfo"));
		CWoormInfo& obj = *((CWoormInfo*)pObj);
		AfxInvokeThreadProcedure<CWoormInfo, DataStr, DataStr>(_hwnd, &obj, &CWoormInfo::MailAttach, fileParam, titleParam);
	}
	//File: Framework\TbWoormViewer\WOORMDOC.CPP.
	extern "C" __declspec(dllexport) void __CWoormInfo_MailSubject(HWND _hwnd, long handle, BSTR subject) throw(...)
	{
		DataStr subjectParam;
		subjectParam.SetCollateCultureSensitive(TRUE);
		subjectParam.SetSoapValue(subject);
		CObject* pObj = ((CObject*)handle);
		if (!pObj->IsKindOf(RUNTIME_CLASS(CWoormInfo)))
			throw new CApplicationErrorException(_T("Invalid context object: CWoormInfo"));
		CWoormInfo& obj = *((CWoormInfo*)pObj);
		AfxInvokeThreadProcedure<CWoormInfo, DataStr>(_hwnd, &obj, &CWoormInfo::MailSubject, subjectParam);
	}
	//File: Framework\TbWoormViewer\WOORMDOC.CPP.
	extern "C" __declspec(dllexport) void __CWoormInfo_MailBody(HWND _hwnd, long handle, BSTR body, bool isHtml) throw(...)
	{
		DataStr bodyParam;
		bodyParam.SetCollateCultureSensitive(TRUE);
		bodyParam.SetSoapValue(body);
		DataBool isHtmlParam;
		isHtmlParam.SetSoapValue(isHtml);
		CObject* pObj = ((CObject*)handle);
		if (!pObj->IsKindOf(RUNTIME_CLASS(CWoormInfo)))
			throw new CApplicationErrorException(_T("Invalid context object: CWoormInfo"));
		CWoormInfo& obj = *((CWoormInfo*)pObj);
		AfxInvokeThreadProcedure<CWoormInfo, DataStr, DataBool>(_hwnd, &obj, &CWoormInfo::MailBody, bodyParam, isHtmlParam);
	}
	//File: Framework\TbWoormViewer\WOORMDOC.CPP.
	extern "C" __declspec(dllexport) void __CWoormInfo_AddParameter(HWND _hwnd, long handle, BSTR paramName, long value) throw(...)
	{
		DataStr paramNameParam;
		paramNameParam.SetCollateCultureSensitive(TRUE);
		paramNameParam.SetSoapValue(paramName);
		DataObj* valueParam;
		valueParam = (DataObj*)value;
		CObject* pObj = ((CObject*)handle);
		if (!pObj->IsKindOf(RUNTIME_CLASS(CWoormInfo)))
			throw new CApplicationErrorException(_T("Invalid context object: CWoormInfo"));
		CWoormInfo& obj = *((CWoormInfo*)pObj);
		AfxInvokeThreadProcedure<CWoormInfo, DataStr, DataObj*>(_hwnd, &obj, &CWoormInfo::AddParameter, paramNameParam, valueParam);
	}
	//File: Framework\TbWoormViewer\WOORMDOC.CPP.
	extern "C" __declspec(dllexport) void __CWoormInfo_SetParamValue(HWND _hwnd, long handle, BSTR paramName, long value) throw(...)
	{
		DataStr paramNameParam;
		paramNameParam.SetCollateCultureSensitive(TRUE);
		paramNameParam.SetSoapValue(paramName);
		DataObj* valueParam;
		valueParam = (DataObj*)value;
		CObject* pObj = ((CObject*)handle);
		if (!pObj->IsKindOf(RUNTIME_CLASS(CWoormInfo)))
			throw new CApplicationErrorException(_T("Invalid context object: CWoormInfo"));
		CWoormInfo& obj = *((CWoormInfo*)pObj);
		AfxInvokeThreadProcedure<CWoormInfo, DataStr, DataObj*>(_hwnd, &obj, &CWoormInfo::SetParamValue, paramNameParam, valueParam);
	}
	//File: Framework\TbWoormViewer\WOORMDOC.CPP.
	extern "C" __declspec(dllexport) void __CWoormInfo_SetUICulture(HWND _hwnd, long handle, BSTR culture) throw(...)
	{
		DataStr cultureParam;
		cultureParam.SetCollateCultureSensitive(TRUE);
		cultureParam.SetSoapValue(culture);
		CObject* pObj = ((CObject*)handle);
		if (!pObj->IsKindOf(RUNTIME_CLASS(CWoormInfo)))
			throw new CApplicationErrorException(_T("Invalid context object: CWoormInfo"));
		CWoormInfo& obj = *((CWoormInfo*)pObj);
		AfxInvokeThreadProcedure<CWoormInfo, DataStr>(_hwnd, &obj, &CWoormInfo::SetUICulture, cultureParam);
	}
	//File: Framework\TbWoormViewer\WOORMDOC.CPP.
	extern "C" __declspec(dllexport) void __CWoormInfo_SetPrinterName(HWND _hwnd, long handle, BSTR name) throw(...)
	{
		DataStr nameParam;
		nameParam.SetCollateCultureSensitive(TRUE);
		nameParam.SetSoapValue(name);
		CObject* pObj = ((CObject*)handle);
		if (!pObj->IsKindOf(RUNTIME_CLASS(CWoormInfo)))
			throw new CApplicationErrorException(_T("Invalid context object: CWoormInfo"));
		CWoormInfo& obj = *((CWoormInfo*)pObj);
		AfxInvokeThreadProcedure<CWoormInfo, DataStr>(_hwnd, &obj, &CWoormInfo::SetPrinterName, nameParam);
	}
	//File: Framework\TbWoormViewer\WOORMDOC.CPP.
	extern "C" __declspec(dllexport) void __CWoormInfo_SetExportOutputType(HWND _hwnd, long handle, BSTR expType) throw(...)
	{
		DataStr expTypeParam;
		expTypeParam.SetCollateCultureSensitive(TRUE);
		expTypeParam.SetSoapValue(expType);
		CObject* pObj = ((CObject*)handle);
		if (!pObj->IsKindOf(RUNTIME_CLASS(CWoormInfo)))
			throw new CApplicationErrorException(_T("Invalid context object: CWoormInfo"));
		CWoormInfo& obj = *((CWoormInfo*)pObj);
		AfxInvokeThreadProcedure<CWoormInfo, DataStr>(_hwnd, &obj, &CWoormInfo::SetExportOutputType, expTypeParam);
	}
	//File: Framework\TbWoormViewer\WOORMDOC.CPP.
	extern "C" __declspec(dllexport) bool __CWoormDoc_Close(HWND _hwnd, long handle) throw(...)
	{
		DataBool returnValueParam;
		CObject* pObj = ((CObject*)handle);
		if (!pObj->IsKindOf(RUNTIME_CLASS(CWoormDocMng)))
			throw new CApplicationErrorException(_T("Invalid context object: CWoormDocMng"));
		CWoormDocMng& obj = *((CWoormDocMng*)pObj);
		returnValueParam = AfxInvokeThreadFunction<DataBool, CWoormDocMng>(_hwnd, &obj, &CWoormDocMng::Close);
		return returnValueParam.GetSoapValue();
	}
	//File: Framework\TbWoormViewer\WOORMDOC.CPP.
	extern "C" __declspec(dllexport) bool __CWoormDoc_Print(HWND _hwnd, long handle, bool bClose) throw(...)
	{
		DataBool returnValueParam;
		DataBool bCloseParam;
		bCloseParam.SetSoapValue(bClose);
		CObject* pObj = ((CObject*)handle);
		if (!pObj->IsKindOf(RUNTIME_CLASS(CWoormDocMng)))
			throw new CApplicationErrorException(_T("Invalid context object: CWoormDocMng"));
		CWoormDocMng& obj = *((CWoormDocMng*)pObj);
		returnValueParam = AfxInvokeThreadFunction<DataBool, CWoormDocMng, DataBool>(_hwnd, &obj, &CWoormDocMng::Print, bCloseParam);
		return returnValueParam.GetSoapValue();
	}
	//File: Framework\TbWoormViewer\SoapFunctions.cpp.
	extern "C" __declspec(dllexport) bool __ExecNewReport(HWND _hwnd, long* docHandle) throw(...)
	{
		DataBool returnValueParam;
		DataLng docHandleParam;
		docHandleParam.SetSoapValue(*docHandle);
		returnValueParam = AfxInvokeThreadGlobalFunction<DataBool, DataLng&>(_hwnd, &ExecNewReport, docHandleParam);
		*docHandle = docHandleParam.GetSoapValue();
		return returnValueParam.GetSoapValue();
	}
	//File: Framework\TbWoormViewer\SoapFunctions.cpp.
	extern "C" __declspec(dllexport) bool __ExecOpenReport(HWND _hwnd, BSTR reportNameSpace, long* docHandle) throw(...)
	{
		DataBool returnValueParam;
		DataStr reportNameSpaceParam;
		reportNameSpaceParam.SetSoapValue(reportNameSpace);
		DataLng docHandleParam;
		docHandleParam.SetSoapValue(*docHandle);
		returnValueParam = AfxInvokeThreadGlobalFunction<DataBool, DataStr, DataLng&>(_hwnd, &ExecOpenReport, reportNameSpaceParam, docHandleParam);
		*docHandle = docHandleParam.GetSoapValue();
		return returnValueParam.GetSoapValue();
	}
	//File: Framework\TbWoormViewer\SoapFunctions.cpp.
	extern "C" __declspec(dllexport) bool __ExecUpgradeReport(HWND _hwnd) throw(...)
	{
		DataBool returnValueParam;
		returnValueParam = AfxInvokeThreadGlobalFunction<DataBool>(_hwnd, &ExecUpgradeReport);
		return returnValueParam.GetSoapValue();
	}
	//File: Framework\TbWoormViewer\SoapFunctions.cpp.
	extern "C" __declspec(dllexport) bool __IsUserReportsDeveloper(HWND _hwnd) throw(...)
	{
		DataBool returnValueParam;
		returnValueParam = AfxInvokeThreadGlobalFunction<DataBool>(_hwnd, &IsUserReportsDeveloper);
		return returnValueParam.GetSoapValue();
	}
	//File: Framework\TbWoormViewer\SoapFunctions.cpp.
	extern "C" __declspec(dllexport) BSTR __GetCompanyInfo(HWND _hwnd, BSTR tagProperty) throw(...)
	{
		DataStr returnValueParam;
		returnValueParam.SetCollateCultureSensitive(TRUE);
		DataStr tagPropertyParam;
		tagPropertyParam.SetSoapValue(tagProperty);
		returnValueParam = AfxInvokeThreadGlobalFunction<DataStr, DataStr>(_hwnd, &GetCompanyInfo, tagPropertyParam);
		return returnValueParam.GetSoapValue();
	}
	//File: Framework\TbWoormViewer\SoapFunctions.cpp.
	extern "C" __declspec(dllexport) bool __GetDocumentMethods(HWND _hwnd, long handleDoc, SAFEARRAY** arMethods) throw(...)
	{
		DataBool returnValueParam;
		DataLng handleDocParam;
		handleDocParam.SetSoapValue(handleDoc);
		DataArray arMethodsParam;
		CComSafeArray<BSTR> sfararMethods;
		if (*arMethods) {
			sfararMethods.Attach(*arMethods);
			for (long i = 0; i < (long)sfararMethods.GetCount(); i++) {
				DataStr *pObj = new DataStr();
				pObj->SetCollateCultureSensitive(TRUE);
				pObj->SetSoapValue((sfararMethods)[i]);
				arMethodsParam.Add(pObj);
			}
		}
		returnValueParam = AfxInvokeThreadGlobalFunction<DataBool, DataLng, DataArray&>(_hwnd, &GetDocumentMethods, handleDocParam, arMethodsParam);
		if (sfararMethods.m_psa)
			sfararMethods.Resize(arMethodsParam.GetSize());
		else
			sfararMethods.Create(arMethodsParam.GetSize());
		for (long i = 0; i < (long)sfararMethods.GetCount(); i++)
		{
			BSTR bstrarMethods = ((DataStr*)arMethodsParam.GetAt(i))->GetSoapValue();
			(sfararMethods)[i] = bstrarMethods;
			::SysFreeString(bstrarMethods);
		}
		*arMethods = sfararMethods.Detach();
		return returnValueParam.GetSoapValue();
	}
	//File: Framework\TbWoormViewer\SoapFunctions.cpp.
	extern "C" __declspec(dllexport) bool __IsPostaLiteEnabled(HWND _hwnd) throw(...)
	{
		DataBool returnValueParam;
		returnValueParam = AfxInvokeThreadGlobalFunction<DataBool>(_hwnd, &IsPostaLiteEnabled);
		return returnValueParam.GetSoapValue();
	}
	//File: Framework\TbWoormViewer\SoapFunctions.cpp.
	extern "C" __declspec(dllexport) BSTR __PostaLiteDecodeDeliveryType(HWND _hwnd, int deliveryType) throw(...)
	{
		DataStr returnValueParam;
		returnValueParam.SetCollateCultureSensitive(TRUE);
		DataInt deliveryTypeParam;
		deliveryTypeParam.SetSoapValue(deliveryType);
		returnValueParam = AfxInvokeThreadGlobalFunction<DataStr, DataInt>(_hwnd, &PostaLiteDecodeDeliveryType, deliveryTypeParam);
		return returnValueParam.GetSoapValue();
	}
	//File: Framework\TbWoormViewer\SoapFunctions.cpp.
	extern "C" __declspec(dllexport) BSTR __PostaLiteDecodePrintType(HWND _hwnd, int printType) throw(...)
	{
		DataStr returnValueParam;
		returnValueParam.SetCollateCultureSensitive(TRUE);
		DataInt printTypeParam;
		printTypeParam.SetSoapValue(printType);
		returnValueParam = AfxInvokeThreadGlobalFunction<DataStr, DataInt>(_hwnd, &PostaLiteDecodePrintType, printTypeParam);
		return returnValueParam.GetSoapValue();
	}
	//File: Framework\TbWoormViewer\SoapFunctions.cpp.
	extern "C" __declspec(dllexport) BSTR __PostaLiteDecodeStatus(HWND _hwnd, int status) throw(...)
	{
		DataStr returnValueParam;
		returnValueParam.SetCollateCultureSensitive(TRUE);
		DataInt statusParam;
		statusParam.SetSoapValue(status);
		returnValueParam = AfxInvokeThreadGlobalFunction<DataStr, DataInt>(_hwnd, &PostaLiteDecodeStatus, statusParam);
		return returnValueParam.GetSoapValue();
	}
	//File: Framework\TbWoormViewer\SoapFunctions.cpp.
	extern "C" __declspec(dllexport) BSTR __PostaLiteDecodeCodeState(HWND _hwnd, int codeState) throw(...)
	{
		DataStr returnValueParam;
		returnValueParam.SetCollateCultureSensitive(TRUE);
		DataInt codeStateParam;
		codeStateParam.SetSoapValue(codeState);
		returnValueParam = AfxInvokeThreadGlobalFunction<DataStr, DataInt>(_hwnd, &PostaLiteDecodeCodeState, codeStateParam);
		return returnValueParam.GetSoapValue();
	}
	//File: Framework\TbWoormViewer\SoapFunctions.cpp.
	extern "C" __declspec(dllexport) BSTR __PostaLiteDecodeStatusExt(HWND _hwnd, long status) throw(...)
	{
		DataStr returnValueParam;
		returnValueParam.SetCollateCultureSensitive(TRUE);
		DataLng statusParam;
		statusParam.SetSoapValue(status);
		returnValueParam = AfxInvokeThreadGlobalFunction<DataStr, DataLng>(_hwnd, &PostaLiteDecodeStatusExt, statusParam);
		return returnValueParam.GetSoapValue();
	}
	//File: Framework\TbWoormViewer\SoapFunctions.cpp.
	extern "C" __declspec(dllexport) BSTR __PostaLiteDecodeErrorExt(HWND _hwnd, long error) throw(...)
	{
		DataStr returnValueParam;
		returnValueParam.SetCollateCultureSensitive(TRUE);
		DataLng errorParam;
		errorParam.SetSoapValue(error);
		returnValueParam = AfxInvokeThreadGlobalFunction<DataStr, DataLng>(_hwnd, &PostaLiteDecodeErrorExt, errorParam);
		return returnValueParam.GetSoapValue();
	}

	//File: Framework\TbWoormViewer\ExternalFunctionsTestSuite.cpp.
	extern "C" __declspec(dllexport) long __TestDataLngParameter(HWND _hwnd, long longParamIn, long* longParamInOut) throw(...)
	{

		DataLng returnValueParam;

		DataLng longParamInParam;
		longParamInParam.SetSoapValue(longParamIn);

		DataLng longParamInOutParam;
		longParamInOutParam.SetSoapValue(*longParamInOut);

		returnValueParam = AfxInvokeThreadGlobalFunction<DataLng, DataLng, DataLng&>(_hwnd, &TestDataLngParameter, longParamInParam, longParamInOutParam);

		*longParamInOut = longParamInOutParam.GetSoapValue();

		return returnValueParam.GetSoapValue();
	}

	//File: Framework\TbWoormViewer\ExternalFunctionsTestSuite.cpp.
	extern "C" __declspec(dllexport) BSTR __TestDataStrParameter(HWND _hwnd, BSTR strParamIn, BSTR* strParamInOut) throw(...)
	{

		DataStr returnValueParam;
		returnValueParam.SetCollateCultureSensitive(TRUE);

		DataStr strParamInParam;
		strParamInParam.SetCollateCultureSensitive(TRUE);
		strParamInParam.SetSoapValue(strParamIn);

		DataStr strParamInOutParam;
		strParamInOutParam.SetCollateCultureSensitive(TRUE);
		strParamInOutParam.SetSoapValue(*strParamInOut);

		returnValueParam = AfxInvokeThreadGlobalFunction<DataStr, DataStr, DataStr&>(_hwnd, &TestDataStrParameter, strParamInParam, strParamInOutParam);

		SysFreeString(*strParamInOut);
		*strParamInOut = strParamInOutParam.GetSoapValue();

		return returnValueParam.GetSoapValue();
	}

	//File: Framework\TbWoormViewer\ExternalFunctionsTestSuite.cpp.
	extern "C" __declspec(dllexport) SAFEARRAY* __TestDataArrayParameter(HWND _hwnd, SAFEARRAY* arIntegerParamIn, SAFEARRAY** arIntegerParamInOut, SAFEARRAY* arStringParamIn, SAFEARRAY** arStringParamInOut) throw(...)
	{

		DataArray returnValueParam;

		DataArray arIntegerParamInParam;

		CComSafeArray<int> sfararIntegerParamIn;
		if (arIntegerParamIn) {
			sfararIntegerParamIn.Attach(arIntegerParamIn);
			for (long i = 0; i < (long)sfararIntegerParamIn.GetCount(); i++) {
				DataInt *pObj = new DataInt();
				pObj->SetSoapValue((sfararIntegerParamIn)[i]);
				arIntegerParamInParam.Add(pObj);
			}


			sfararIntegerParamIn.Detach();
		}
		DataArray arIntegerParamInOutParam;

		CComSafeArray<int> sfararIntegerParamInOut;
		if (*arIntegerParamInOut) {
			sfararIntegerParamInOut.Attach(*arIntegerParamInOut);
			for (long i = 0; i < (long)sfararIntegerParamInOut.GetCount(); i++) {
				DataInt *pObj = new DataInt();
				pObj->SetSoapValue((sfararIntegerParamInOut)[i]);
				arIntegerParamInOutParam.Add(pObj);
			}


		}
		DataArray arStringParamInParam;

		CComSafeArray<BSTR> sfararStringParamIn;
		if (arStringParamIn) {
			sfararStringParamIn.Attach(arStringParamIn);
			for (long i = 0; i < (long)sfararStringParamIn.GetCount(); i++) {
				DataStr *pObj = new DataStr();
				pObj->SetCollateCultureSensitive(TRUE);
				pObj->SetSoapValue((sfararStringParamIn)[i]);
				arStringParamInParam.Add(pObj);
			}


			sfararStringParamIn.Detach();
		}
		DataArray arStringParamInOutParam;

		CComSafeArray<BSTR> sfararStringParamInOut;
		if (*arStringParamInOut) {
			sfararStringParamInOut.Attach(*arStringParamInOut);
			for (long i = 0; i < (long)sfararStringParamInOut.GetCount(); i++) {
				DataStr *pObj = new DataStr();
				pObj->SetCollateCultureSensitive(TRUE);
				pObj->SetSoapValue((sfararStringParamInOut)[i]);
				arStringParamInOutParam.Add(pObj);
			}


		}
		returnValueParam = AfxInvokeThreadGlobalFunction<DataArray, DataArray, DataArray&, DataArray, DataArray&>(_hwnd, &TestDataArrayParameter, arIntegerParamInParam, arIntegerParamInOutParam, arStringParamInParam, arStringParamInOutParam);


		CComSafeArray<BSTR> sfarreturnValue;
		sfarreturnValue.Create(returnValueParam.GetSize());

		for (long i = 0; i < (long)sfarreturnValue.GetCount(); i++)
		{
			BSTR bstrreturnValue = ((DataStr*)returnValueParam.GetAt(i))->GetSoapValue();
			(sfarreturnValue)[i] = bstrreturnValue;
			::SysFreeString(bstrreturnValue);
		}


		if (sfararIntegerParamInOut.m_psa)
			sfararIntegerParamInOut.Resize(arIntegerParamInOutParam.GetSize());
		else
			sfararIntegerParamInOut.Create(arIntegerParamInOutParam.GetSize());

		for (long i = 0; i < (long)sfararIntegerParamInOut.GetCount(); i++)
		{
			(sfararIntegerParamInOut)[i] = ((DataInt*)arIntegerParamInOutParam.GetAt(i))->GetSoapValue();
		}


		*arIntegerParamInOut = sfararIntegerParamInOut.Detach();
		if (sfararStringParamInOut.m_psa)
			sfararStringParamInOut.Resize(arStringParamInOutParam.GetSize());
		else
			sfararStringParamInOut.Create(arStringParamInOutParam.GetSize());

		for (long i = 0; i < (long)sfararStringParamInOut.GetCount(); i++)
		{
			BSTR bstrarStringParamInOut = ((DataStr*)arStringParamInOutParam.GetAt(i))->GetSoapValue();
			(sfararStringParamInOut)[i] = bstrarStringParamInOut;
			::SysFreeString(bstrarStringParamInOut);
		}


		*arStringParamInOut = sfararStringParamInOut.Detach();
		return sfarreturnValue.Detach();
	}
}

TB_REGISTER_SOAP_SERVICE(TbWoormViewerTbWoormViewer::CTbWoormViewerTbWoormViewer)