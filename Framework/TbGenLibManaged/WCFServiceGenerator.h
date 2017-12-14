

#include "beginh.dex"

using namespace Microarea::TaskBuilderNet::Core::SoapCall;
using namespace System::Text;
using namespace System::Reflection;

class CFunctionDescription;

ref class CServiceGenerator
{	
	System::Collections::Generic::Dictionary<System::String^, StringBuilder^>^ serviceMap;
	System::Collections::Generic::List<System::String^>^  functionNamespaces;
	System::Collections::Generic::List<System::String^>^  serviceNamespaces;
	System::Collections::Generic::List<System::String^>^  referencedAssemblies;
public:
	CServiceGenerator()
	{
		functionNamespaces = gcnew System::Collections::Generic::List<System::String^>();
		serviceNamespaces = gcnew System::Collections::Generic::List<System::String^>();
		referencedAssemblies = gcnew System::Collections::Generic::List<System::String^>();
		serviceMap = gcnew System::Collections::Generic::Dictionary<System::String^, StringBuilder^>();
	}

	void AppendFunctions(const CBaseDescriptionArray &functions);
	Assembly^ Compile();

	static System::String^ GetServiceName(CFunctionDescription* pF);
	static System::String^ GetLibNamespace(CFunctionDescription* pF);
	static System::String^ GetOutType(CFunctionDescription* pF);
	static System::String^ GetFunctionName(CFunctionDescription* pF);
	static System::String^ GetFunctionNamespace(CFunctionDescription* pF);
	static System::String^ GetDllName(CFunctionDescription* pF);

	void RegisterNamespaces(const CString& strUser, int nStartPort);
private:
	StringBuilder^ GetClassStringBuilder(System::String ^serviceName, System::String ^libNamespace);
	StringBuilder^ GetFileStringBuilder();
	Object^		   GetAttribute(System::Type^ attrType);
	void AddFunction(CFunctionDescription* pF);

	StringBuilder^ GetFunctionParamatersDeclaration(CFunctionDescription* pF, bool attributes, bool types, bool appendToExisting, bool skipHandle);
	StringBuilder^ GetOutAssignment(CFunctionDescription* pF);
	StringBuilder^ GetFunctionParamatersUse(CFunctionDescription* pF);
	StringBuilder^ GetOutVariableDeclaration(CFunctionDescription* pF, System::String^ strFunctionName);
	StringBuilder^ GetInVariableDeclaration(CFunctionDescription* pF, System::String^ strFunctionName);
	StringBuilder^ CServiceGenerator::GetFunctionNamespaceFunction(CFunctionDescription* pF);
	StringBuilder^ GetContextHandleFunction(CFunctionDescription* pF);
	StringBuilder^ GetOutVarUpdate(CFunctionDescription* pF);
};



#include "endh.dex"