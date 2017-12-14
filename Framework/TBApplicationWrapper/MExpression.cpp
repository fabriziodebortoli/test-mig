#include "StdAfx.h"


#include <TbOleDb\SqlCatalog.h>
#include <TbOleDb\SqlRec.h>
#include <TbParser\SymTable.h>
#include <TbWoormEngine\MultiLayout.h>
#include <TbWoormEngine\RPSYMTBL.H>
#include <TbWoormEngine\RepTable.h>
#include <TbGenlib\BaseApp.h>
#include <TbGenLibManaged\Main.h>

#include "MSqlRecord.h"
#include "MExpression.h"

using namespace Microarea::Framework::TBApplicationWrapper;
using namespace System;
using namespace System::Collections::Generic;
using namespace Microarea::TaskBuilderNet::Interfaces::Model;
using namespace Microarea::TaskBuilderNet::UI::CodeEditor;
using namespace Microarea::TaskBuilderNet::Core::CoreTypes;
using namespace Microarea::TaskBuilderNet::Interfaces;
using namespace Microarea::TaskBuilderNet::Core::Applications;

void DocumentMessageProvider::Message(System::String^ message)
{
	m_pDocument->Message(CString(message));
}


ref class CExpressionEditorManager
{
	CObject* m_pObject;
	CHECK_EXPRESSION m_pCheckExpression;
public:
	CExpressionEditorManager(CObject* pObject, CHECK_EXPRESSION pCheckExpression)
	 : m_pCheckExpression(pCheckExpression), m_pObject(pObject)
	{
	
	}
	bool OnCheckExpression(System::String^ expression, System::String^% errorMessage)
	{
		errorMessage = "";
		if (!m_pObject || !m_pCheckExpression)
			return true;
		CString strError;
		bool b = (m_pObject->*m_pCheckExpression)(CString(expression), strError);
		errorMessage = gcnew System::String(strError);
		return b;
	}
};
bool DoExpressionEditor(
	const SqlTableInfoArray* pArTableInfo, 
	const SymTable* pSymTable, 
	CString& strOutExpression, 
	bool onlyQueryLanguageItems,
	CObject* pObject, 
	CHECK_EXPRESSION lpCheckExpression)
{
	SymbolTable^ table = gcnew SymbolTable(nullptr);
	for (int i = 0; pSymTable && i < pSymTable->GetSize(); i++)
	{
		SymField* pF = pSymTable->GetAt(i);

		//if (
		//	pF->GetId() == SYMTABLE_SELF_REFERENCE_ID
		//	)
		//	continue;

		if (pF->IsKindOf(RUNTIME_CLASS(WoormField)))
		{
			WoormField* pRfd = (WoormField*) pF;
			ExtendedVariable^ var = gcnew ExtendedVariable(gcnew System::String(pRfd->GetName()));
			var->IsTableRuleField = pRfd->IsTableRuleField() == TRUE;
			var->IsExpressionRuleField = pRfd->IsExprRuleField() == TRUE ;
			var->IsFunctionField = pRfd->HasAFunction() == TRUE;
			var->IsAskField = pRfd->IsInput() == TRUE;
			table->Add(var);
		} 
		else
			table->Add(gcnew ExtendedVariable(gcnew System::String(pF->GetName())));
	}

	return DoExpressionEditor(pArTableInfo, table, strOutExpression, onlyQueryLanguageItems, pObject, lpCheckExpression);
}

bool DoExpressionEditor(
	const SqlTableInfoArray* pArTableInfo, 
	Microarea::TaskBuilderNet::Core::CoreTypes::SymbolTable^ table,
	CString& strOutExpression, 
	bool onlyQueryLanguageItems,
	CObject* pObject, 
	CHECK_EXPRESSION lpCheckExpression)
{
	InitThreadCulture();

	List<IRecord^>^ list = gcnew List<IRecord^>();
	for (int i = 0; pArTableInfo && i < pArTableInfo->GetSize(); i++)
	{
		const SqlTableInfo* pInfo = pArTableInfo->GetAt(i);
		SqlRecord* pRecord = pInfo->GetSqlCatalogEntry()->CreateRecord();
		MSqlRecord^ record = gcnew MSqlRecord((System::IntPtr)pRecord);
		record->HasCodeBehind = false;

		list->Add(record);
	}

	Microarea::TaskBuilderNet::Core::CoreTypes::FunctionsList^ functions = gcnew Microarea::TaskBuilderNet::Core::CoreTypes::FunctionsList();
	/*if (!onlyQueryLanguageItems)  //lo fa direttamente il costruttore
	{
		TbReportSession::AddInternalWoormFunction(functions);
		functions->LoadPrototypes();
	}*/
	
	Enums^ enums = gcnew Enums();
	enums->LoadXml();
	
	List<System::String^>^ keywords = gcnew List<System::String^>();
	if (!onlyQueryLanguageItems)
	{
		CStringArray arKeywords;
//TODO		AddEventActionsKeywords(arKeywords);
		for (int i = 0; i < arKeywords.GetCount(); i++)
			keywords->Add(gcnew System::String(arKeywords[i]));
	}
	
	System::String^ expression = gcnew System::String(strOutExpression);
	CExpressionEditorManager^ expMng = gcnew CExpressionEditorManager(pObject, lpCheckExpression);
	if (ScriptEditorManager::DoEditor
		(
		expression,
		keywords,
		list, 
		table,
		functions->Prototypes,
		enums,
		gcnew CheckExpression(expMng, &CExpressionEditorManager::OnCheckExpression)))
	{
		strOutExpression = expression;
		return true;
	}
	return false;
}
