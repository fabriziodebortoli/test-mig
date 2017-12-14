#pragma once



class SqlTableInfoArray;
class SymTable;

using namespace Microarea::TaskBuilderNet::Woorm::ExpressionManager;


//TODO 
typedef bool (CObject::*CHECK_EXPRESSION) (const CString& strOutExpression, CString& strError);
		
//================================================================================
public ref class DocumentMessageProvider : IMessageProvider
{
	CBaseDocument* m_pDocument;
public:
	DocumentMessageProvider(CBaseDocument* pDocument) : m_pDocument(pDocument){}
	virtual void Message(System::String^ message);
};

bool DoExpressionEditor(
	const SqlTableInfoArray* pArTableInfo,
	const SymTable* pSymTable,
	CString& strOutExpression, 
	bool onlyQueryLanguageItems,
	CObject* pObject,
	CHECK_EXPRESSION lpCheckExpression);

bool DoExpressionEditor(
	const SqlTableInfoArray* pArTableInfo,
	Microarea::TaskBuilderNet::Core::CoreTypes::SymbolTable^ table,
	CString& strOutExpression, 
	bool onlyQueryLanguageItems,
	CObject* pObject,
	CHECK_EXPRESSION lpCheckExpression);
