#pragma once
#include <TbNameSolver\JsonSerializer.h>

#include "beginh.dex"

class CBaseDocument;

class TB_EXPORT CTBSocketHandler
{
	typedef void(CTBSocketHandler::*FUNCPTR)(CJsonParser& json);
	typedef CMap<CString, LPCTSTR, FUNCPTR, FUNCPTR> CTbSocketHandlerFunctionMap;

	CMap <int, int, TDisposablePtr<CBaseDocument>, TDisposablePtr<CBaseDocument>> m_arDocuments;
	CCriticalSection m_Critical;
	CTbSocketHandlerFunctionMap functionMap;
public:
	CTBSocketHandler();
	~CTBSocketHandler();
	void Execute(CString& sSocketName, CString& sMessage);
private:
	bool IsCancelableCommand(const CString& sCommand);
	void ExecuteFunction(FUNCPTR fn, CJsonParser* pParser, CAbstractFormDoc* pDoc);
	void DoCommand(CJsonParser& json);
	void DoClose(CJsonParser& json);
	void DoValueChanged(CJsonParser& json);
	void DoCloseMessage(CJsonParser& json);
	void DoCloseDiagnostic(CJsonParser& json);
	void GetOpenDocuments(CJsonParser& json);
	void GetDocumentData(CJsonParser& json);
	void GetActivationData(CJsonParser& json);
	void GetWindowStrings(CJsonParser& json);
	void CheckMessageDialog(CJsonParser& json);
	void DoFillListBox(CJsonParser& json);
	void DoCheckListBoxAction(CJsonParser& json);
	void SetReportResult(CJsonParser& json);
	void RunDocument(CJsonParser& json);
	void BrowseRecord(CJsonParser& json);
	void OpenHyperLink(CJsonParser& json);
	void QueryHyperLink(CJsonParser& json);
	void DoControlCommand(CJsonParser& json);
	void DoActivateClientContainer(CJsonParser& json);
	void DoPinUnpin(CJsonParser& json);
	void DoUpdateTitle(CJsonParser& json);

	void pushCheckListBoxItemSource(CJsonParser& json, CAbstractFormDoc* pDoc, const CString& controlId);
	void RunDocumentOnThreadDocument(const CString& sNamespace, const CString& sArguments);
	CBaseDocument* GetDocument(int cmpId);
};

#include "endh.dex"