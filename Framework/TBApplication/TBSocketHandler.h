#pragma once
#include <TbNameSolver\JsonSerializer.h>

#include "beginh.dex"

class CBaseDocument;
class TB_EXPORT CTBSocketHandler
{
	typedef void(CTBSocketHandler::*FUNCPTR)(CJsonParser& json);
	typedef CMap<CString, LPCTSTR, FUNCPTR, FUNCPTR> CTbSocketHandlerFunctionMap;

	CTbSocketHandlerFunctionMap functionMap;
public:
	CTBSocketHandler();
	~CTBSocketHandler();
	void Execute(CString& sSocketName, CString& sMessage);
private:
	void ExecuteFunction(FUNCPTR fn, CJsonParser* pParser);
	void DoCommand(CJsonParser& json);
	void DoClose(CJsonParser& json);
	void DoValueChanged(CJsonParser& json);
	void DoCloseMessage(CJsonParser& json);
	void DoCloseDiagnostic(CJsonParser& json);
	void GetOpenDocuments(CJsonParser& json);
	void GetDocumentData(CJsonParser& json);
	void GetWindowStrings(CJsonParser& json);
	void CheckMessageDialog(CJsonParser& json);
	void DoFillListBox(CJsonParser& json);
	void SetReportResult(CJsonParser& json);
	void RunDocument(CJsonParser& json);
	void BrowseRecord(CJsonParser& json);
	void OpenHyperLink(CJsonParser& json);
	void QueryHyperLink(CJsonParser& json);
	void OpenNewHyperLink(CJsonParser& json);
	void DoControlCommand(CJsonParser& json);
};

#include "endh.dex"