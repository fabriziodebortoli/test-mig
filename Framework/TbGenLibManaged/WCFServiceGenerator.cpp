#include "stdafx.h" 

#include <TbGeneric\FunctionObjectsInfo.h>
#include <TbGeneric\GeneralFunctions.h>
#include <TbNameSolver\Diagnostic.h>

#include "wcfservicegenerator.h"
#include "wcfhelper.h"

using namespace System;
using namespace System::IO;
using namespace System::CodeDom::Compiler;
using namespace Microsoft::CSharp;
using namespace Microarea::TaskBuilderNet::Core::SoapCall;
using namespace Microarea::TaskBuilderNet::Interfaces;
using namespace System::Collections::Generic;

ref class Logger : ILogWriter
{
	List<String^>^ messages;
public:

	property List<String^>^ Messages
		{
			List<String^>^ get() { return messages; }
		}

	Logger()
	{
		messages = gcnew List<String^>();
	}

	virtual void WriteLine(System::String ^ text, ...cli::array<Object^> ^args)
	{
		messages->Add(String::Format(text, args));
	}
};


//------------------------------------------------------------------
Assembly^ CServiceGenerator::Compile()
{
	try
	{
		CompilerParameters^ par = gcnew CompilerParameters();
		par->GenerateInMemory = false;
		par->GenerateExecutable = false;
	   
		// should start displaying warnings.
		par->WarningLevel = 3;
		par->ReferencedAssemblies->Add("System.ServiceModel.dll");
		par->ReferencedAssemblies->Add("System.dll");
		
		String^ curreAsm = Assembly::GetExecutingAssembly()->Location;
		String^ folderName = Path::GetDirectoryName(curreAsm);
		
		// Set the assembly file name to generate.
		par->OutputAssembly = ServiceClientCache::GetServicesAssemblyPath();

		for each(String^ s in referencedAssemblies)
			par->ReferencedAssemblies->Add(Path::Combine(folderName, s));
		
		par->ReferencedAssemblies->Add(Path::Combine(folderName, "Microarea.TaskBuilderNet.Interfaces.dll"));
		par->ReferencedAssemblies->Add(Path::Combine(folderName, "Microarea.TaskBuilderNet.Core.dll"));
		par->ReferencedAssemblies->Add(Path::Combine(folderName, "TBApplicationWrapper.dll"));
		par->ReferencedAssemblies->Add(curreAsm);
		// Set whether to treat all warnings as errors.
		par->TreatWarningsAsErrors = false; 

	#ifdef DEBUG
		par->IncludeDebugInformation  = true;
	#else
		// Set compiler argument to optimize output.
		par->CompilerOptions = "/optimize";
	#endif

		Dictionary<String^, String^>^ options = gcnew Dictionary<String^, String^>();
		options->Add("CompilerVersion", "v4.0");
		CSharpCodeProvider^ provider = gcnew CSharpCodeProvider(options);

		StringBuilder^ sb = GetFileStringBuilder();
	#ifdef DEBUG
		String^ sourceFile = Path::Combine(folderName, "DynamicWCFServices.cs");
		StreamWriter^ sw = gcnew StreamWriter(sourceFile);
		sw->Write(sb->ToString());
		sw->Close();
		CompilerResults^ res = provider->CompileAssemblyFromFile(par, sourceFile);
	#else
		CompilerResults^ res = provider->CompileAssemblyFromSource(par, sb->ToString());
	#endif
		if (res->Errors->HasErrors)
		{
			for each (CompilerError^ error in res->Errors)
				AfxGetDiagnostic()->Add(error->ErrorText + "(line " + error->Line + ")", CDiagnostic::Error);

			return nullptr;
		}
		
		return res->CompiledAssembly;
	}
	catch (Exception^ ex)
	{
		AfxGetDiagnostic()->Add(ex->ToString());
		return nullptr;
	}
}

//------------------------------------------------------------------
void CServiceGenerator::AppendFunctions(const CBaseDescriptionArray &functions)
{
	for (int nF2 = 0; nF2 <= functions.GetUpperBound(); nF2++)
	{
		CFunctionDescription* pF = (CFunctionDescription*) functions.GetAt(nF2);
		AddFunction(pF);
	}
}

//------------------------------------------------------------------
Object^ CServiceGenerator::GetAttribute(System::Type^ attrType)
{
	array<Object^>^ o = TBWCFService::typeid->Assembly->GetCustomAttributes(attrType, false);
	if (o->Length == 1)
		return o[0];
	throw gcnew ApplicationException(gcnew String(cwsprintf(_TB("Attribute {0-%s} not found in assembly {1-%s}"), CString(attrType->ToString()), CString(TBWCFService::typeid->Assembly->FullName))));
}

//------------------------------------------------------------------
StringBuilder^ CServiceGenerator::GetFileStringBuilder()
{
	StringBuilder^ sb = gcnew StringBuilder();
	sb->AppendFormat("\r\nusing System;");
	sb->AppendFormat("\r\nusing System.ServiceModel;");
	sb->AppendFormat("\r\nusing System.Runtime.InteropServices;");
	sb->AppendFormat("\r\nusing System.Reflection;");
	sb->AppendFormat("\r\nusing System.Runtime.CompilerServices;");
	sb->AppendFormat("\r\nusing Microarea.TaskBuilderNet.Core.SoapCall;");
	sb->AppendFormat("\r\nusing Microarea.TaskBuilderNet.Core.EasyBuilder;");

	sb->AppendFormat("\r\n[assembly: AssemblyCompany(\"{0}\")]", ((AssemblyCompanyAttribute^)GetAttribute(AssemblyCompanyAttribute::typeid))->Company);
	sb->AppendFormat("\r\n[assembly: AssemblyProduct(\"{0}\")]", ((AssemblyProductAttribute^)GetAttribute(AssemblyProductAttribute::typeid))->Product);
	sb->AppendFormat("\r\n[assembly: AssemblyCopyright(\"{0}\")]", ((AssemblyCopyrightAttribute^)GetAttribute(AssemblyCopyrightAttribute::typeid))->Copyright);
	sb->AppendFormat("\r\n[assembly: AssemblyTrademark(\"{0}\")]", ((AssemblyTrademarkAttribute^)GetAttribute(AssemblyTrademarkAttribute::typeid))->Trademark);
	sb->AppendFormat("\r\n[assembly: AssemblyFileVersion(\"{0}\")]", ((AssemblyFileVersionAttribute^)GetAttribute(AssemblyFileVersionAttribute::typeid))->Version);
	sb->AppendFormat("\r\n[assembly: AssemblyVersion(\"{0}\")]", TBWCFService::typeid->Assembly->GetName()->Version);

	sb->AppendFormat("\r\nnamespace {0}", ServiceClientCache::AssemblyNamespace);
	sb->AppendFormat("\r\n{{");
	sb->Append("delegate object DoGeneric();\r\n");

	for each (StringBuilder^ sbc in serviceMap->Values)
	{
		sb->Append(sbc);
		sb->AppendFormat("\r\n}}");
	}
	sb->AppendFormat("\r\n}}");
	return sb;
}
//------------------------------------------------------------------
StringBuilder^ CServiceGenerator::GetClassStringBuilder(String ^serviceName, String ^libNamespace)
{
	StringBuilder^ sb = nullptr;
	if (!serviceMap->TryGetValue(serviceName, sb))
	{
		sb = gcnew StringBuilder();
		sb->AppendFormat("\r\n[ServiceBehavior(Namespace = \"urn:Microarea.Web.Services\", UseSynchronizationContext = false, IncludeExceptionDetailInFaults = true, InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]");
		sb->AppendFormat("\r\n[ServiceContract(Namespace = \"urn:Microarea.Web.Services\")]");
		sb->AppendFormat("\r\n[ExcludeFromIntellisense]");
		sb->AppendFormat("\r\npublic class {0} : TBWCFService", serviceName);
		sb->AppendFormat("\r\n{{");
		sb->AppendFormat("\r\npublic override string GetName(){{ return \"{0}\"; }}", serviceName);
		sb->AppendFormat("\r\npublic override string GetNamespace(){{ return \"{0}\"; }}", libNamespace);
		serviceMap[serviceName] = sb;
		serviceNamespaces->Add(libNamespace);
	}

	return sb;
}

//------------------------------------------------------------------
void CServiceGenerator::RegisterNamespaces(const CString& strUser, int nStartPort)
{
	Logger^ logger = gcnew Logger();
	WCFServiceRegister::RegisterServicesForUser(serviceNamespaces, nStartPort, gcnew String(strUser), logger);
}

//------------------------------------------------------------------
String^ CServiceGenerator::GetServiceName(CFunctionDescription* pF)
{
	if (pF->GetNamespace().IsFromTaskBuilder())
		return gcnew String (pF->GetNamespace().GetObjectName(CTBNamespace::LIBRARY));
	else
		return gcnew String (pF->GetNamespace().GetObjectName (CTBNamespace::MODULE) + pF->GetNamespace().GetObjectName (CTBNamespace::LIBRARY));
}

//------------------------------------------------------------------
String^ CServiceGenerator::GetLibNamespace(CFunctionDescription* pF)
{
	return gcnew String(pF->GetNamespace().Left(CTBNamespace::LIBRARY));
}

//------------------------------------------------------------------
void CServiceGenerator::AddFunction(CFunctionDescription* pF)
{
	String^ fNamespace = gcnew String(pF->GetNamespace().ToString());
	fNamespace = fNamespace->ToLower();
	if (functionNamespaces->Contains(fNamespace))
		return;
	
	String^ outType = GetOutType(pF);

	functionNamespaces->Add(fNamespace);

	String ^serviceName = GetServiceName(pF);
	String ^libNamespace = GetLibNamespace(pF);

	StringBuilder^ sb = GetClassStringBuilder(serviceName, libNamespace);


	String^ strFunctionName = GetFunctionName(pF);
	String^ strDllName = GetDllName(pF);
	if (pF->IsManaged())
	{
		sb->AppendFormat("\r\nprivate static {0} __{1}(IntPtr __hwnd{2}){{", outType, strFunctionName, GetFunctionParamatersDeclaration(pF, true, true, true, false));
		sb->Append("\r\n\t");
		if (outType != "void")
			sb->AppendFormat("return ({0}) ", outType);
		sb->Append("CWCFHelper.Invoke(__hwnd, (DoGeneric)delegate{");
		int commaIdx = pF->GetManagedType().Find(_T(","));
		String^ sType = gcnew String(pF->GetManagedType().Mid(0, commaIdx));
		String^ sAsm = gcnew String(pF->GetManagedType().Mid(commaIdx + 1).Trim()  + _T(".dll"));
		if (!referencedAssemblies->Contains(sAsm))
			referencedAssemblies->Add(sAsm);
		if (strFunctionName->EndsWith("_Create"))
		{
			sb->AppendFormat("return CUtility.CreateWebServiceStateObject(\"{0}\", {1});", gcnew String(pF->GetManagedType()), GetFunctionParamatersDeclaration(pF, false, false, false, false));
		}
		else if (pF->IsThisCallMethods())
		{
			if (strFunctionName->EndsWith("_Dispose"))
			{
				sb->AppendFormat("return CUtility.RemoveWebServiceStateObject(handle);");
			}
			else
			{
				sb->AppendFormat("{0} obj = ({0})CUtility.GetWebServiceStateObject(handle);\r\n", sType);
				if (outType != "void")
				{
					sb->Append(" return obj == null ? null : (object) ");
				}
				else
				{
					sb->Append(" if (obj != null) ");
				}
				sb->AppendFormat(" obj.{0}({1});", strFunctionName->Substring(strFunctionName->IndexOf("_") + 1), GetFunctionParamatersDeclaration(pF, false, false, false, true));
			}
		}
		else
		{
			if (outType != "void")
				sb->Append(" return");
			sb->AppendFormat(" {0}.{1}({2});", sType, strFunctionName, GetFunctionParamatersDeclaration(pF, false, false, false, false));
		}
		sb->Append("});");
		sb->Append("\r\n}");
	}
	else
	{
		sb->AppendFormat("\r\n[DllImport (\"{0}.dll\", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]\r\n", strDllName);
	
		CDataObjDescription retVal = pF->GetReturnValueDescription();
		if (CWCFHelper::IsArrayType(&retVal))
			sb->AppendFormat("[return: MarshalAs(UnmanagedType.SafeArray)]");
		else if (CWCFHelper::ManagedTypeFromDataType(retVal.GetDataType()) == String::typeid) 
			sb->AppendFormat("[return: MarshalAs(UnmanagedType.BStr)]");
		else if (CWCFHelper::ManagedTypeFromDataType(retVal.GetDataType()) == bool::typeid)
			sb->AppendFormat("[return: MarshalAs(UnmanagedType.U1)]");
	
		sb->AppendFormat("\r\nprivate static extern {0} __{1}(IntPtr __hwnd{2});", outType, strFunctionName, GetFunctionParamatersDeclaration(pF, true, true, true, false));
	}
	
	sb->AppendFormat("\r\npublic static {0} {1}({2})", outType, strFunctionName, GetFunctionParamatersDeclaration(pF, false, true, false, false));
	sb->AppendFormat("\r\n{{\r\n\t");
	if (outType != "void")
		sb->AppendFormat("\r\n\treturn ");
	else
		sb->AppendFormat("\r\n\t");
	sb->AppendFormat("  ____{0}(CWCFHelper.GetThreadMainWnd(){1});", strFunctionName, GetFunctionParamatersDeclaration(pF, false, false, true, false));
	sb->AppendFormat("\r\n}}");	
	
	sb->Append("\r\n[ExcludeFromIntellisense]");
	sb->AppendFormat("\r\npublic static {0} ____{1}(IntPtr __hwnd{2})", outType, strFunctionName, GetFunctionParamatersDeclaration(pF, false, true, true, false));
	sb->AppendFormat("\r\n{{\r\n\t");
	
	if (outType != "void")
		sb->AppendFormat("{0} ____ret = ", outType);
	
	sb->AppendFormat("__{0}(__hwnd{1});", strFunctionName, GetFunctionParamatersDeclaration(pF, false, false, true, false));

	if (outType != "void")
		sb->AppendFormat("\r\nreturn ____ret;");
	sb->AppendFormat("\r\n}}");	
	
	sb->AppendFormat("\r\n\r\n[MessageContract(IsWrapped = false)]");
	sb->AppendFormat("\r\n[ExcludeFromIntellisense]");
	sb->AppendFormat("\r\npublic class {0}Out : TbSoapArgument", strFunctionName);
	sb->AppendFormat("\r\n{{");
	sb->AppendFormat("\r\n\t{0}", GetOutVariableDeclaration(pF, strFunctionName), strFunctionName);
	sb->AppendFormat("{0}", GetFunctionNamespaceFunction(pF));
	sb->AppendFormat("\r\n}}");

	sb->AppendFormat("\r\n\r\n[MessageContract(IsWrapped = false)]");
	sb->AppendFormat("\r\n[ExcludeFromIntellisense]");
	sb->AppendFormat("\r\npublic class {0}In : TbSoapArgument", strFunctionName);
	sb->AppendFormat("\r\n{{");
	sb->AppendFormat("\r\n\t{0}", GetInVariableDeclaration(pF, strFunctionName));
	sb->AppendFormat("{0}", GetContextHandleFunction(pF));
	sb->AppendFormat("{0}", GetFunctionNamespaceFunction(pF));
	sb->AppendFormat("\r\n}}");
	sb->AppendFormat("\r\n\r\n[OperationContract(Action=\"#{0}\")] [FaultContract(typeof(TBSoapFault))]", strFunctionName);
	sb->AppendFormat("\r\npublic {0}Out {0}({0}In __input)", strFunctionName);
	sb->AppendFormat("\r\n{{");
	if (!pF->IsManaged())
		sb->AppendFormat("\r\n\tLoadTBDll(\"{0}\");\r\n\t", strDllName);

	sb->AppendFormat("\r\n\t{0}Out __ret = new {0}Out();", strFunctionName);
	sb->AppendFormat("\r\ntry {{");
	sb->AppendFormat("\r\n\t{0} __{1}(__input.GetThreadHwnd(){2});", GetOutAssignment(pF), strFunctionName, GetFunctionParamatersUse(pF));
	sb->Append(GetOutVarUpdate(pF));
	sb->AppendFormat("\r\n}} catch (Exception ex) {{ throw new FaultException<TBSoapFault>(new TBSoapFault(ex), ex.Message); }}");
	sb->AppendFormat("\r\n\treturn __ret;");
	sb->AppendFormat("\r\n}}");

}


//------------------------------------------------------------------
String^ CServiceGenerator::GetDllName(CFunctionDescription* pF)
{
	return gcnew String(AfxGetPathFinder()->GetDllNameFromNamespace(pF->GetNamespace()));
}
//------------------------------------------------------------------
String^ CServiceGenerator::GetOutType(CFunctionDescription* pF)
{
	return CWCFHelper::ManagedTypeFromDataType(&pF->GetReturnValueDescription());

}

//------------------------------------------------------------------
String^ CServiceGenerator::GetFunctionName(CFunctionDescription* pF)
{
	return gcnew String(pF->GetName());

}

//------------------------------------------------------------------
StringBuilder^ CServiceGenerator::GetFunctionParamatersDeclaration(CFunctionDescription* pF, bool attributes, bool types, bool appendToExisting, bool skipHandle)
{
	StringBuilder^ sb = gcnew StringBuilder();
	bool addComma = appendToExisting ? true : false; //la prima volta, devo aggiungere la virgola solo se è già presente un parametro nel prototipo
	for (int i = 0; i < pF->GetParameters().GetCount(); i++)
	{
		CDataObjDescription *pParam = pF->GetParamDescription(i);
		if (skipHandle && pParam->GetName() == _T("handle"))
			continue;
		sb->AppendFormat("{0}{1}{2}{3} {4}", 
			addComma ? "," : "",
			attributes
			? (CWCFHelper::IsArrayType(pParam)
					? "[MarshalAs(UnmanagedType.SafeArray)] " 
					: ( CWCFHelper::ManagedTypeFromDataType(pParam->GetDataType()) == String::typeid 
						? "[MarshalAs(UnmanagedType.BStr)] "
						:CWCFHelper::ManagedTypeFromDataType(pParam->GetDataType()) == bool::typeid
						? "[MarshalAs(UnmanagedType.U1)] "	
						: "" )
						)
				: "",
			pParam->IsPassedModeIn() ? "" : "ref ",
			types
				? CWCFHelper::ManagedTypeFromDataType(pParam)
				: "",
			gcnew String(pParam->GetName())
			);
		addComma = true;
	}
	return sb;
}
//------------------------------------------------------------------
StringBuilder^ CServiceGenerator::GetOutAssignment(CFunctionDescription* pF)
{
	StringBuilder^ sb = gcnew StringBuilder();

	if (pF->GetReturnValueDataType() != DataType::Void)
	{
		sb->AppendFormat( "__ret.ret{0} = ", gcnew String(pF->GetName()));
	}

	return sb;
}
//------------------------------------------------------------------
StringBuilder^ CServiceGenerator::GetOutVarUpdate(CFunctionDescription* pF)
{
	StringBuilder^ sb = gcnew StringBuilder();

	for (int i = 0; i < pF->GetParameters().GetCount(); i++)
	{
		CDataObjDescription *pParam = pF->GetParamDescription(i);
		if (pParam->IsPassedModeIn())
			continue;

		sb->AppendFormat("\r\n\t__ret.{0} = __input.{0};", gcnew String(pParam->GetName()));
	}

	sb->Append("\r\n\t__ret.HeaderInfo = __input.HeaderInfo;");
	return sb;
}

//------------------------------------------------------------------
StringBuilder^ CServiceGenerator::GetFunctionParamatersUse(CFunctionDescription* pF)
{
	StringBuilder^ sb = gcnew StringBuilder();

	for (int i = 0; i < pF->GetParameters().GetCount(); i++)
	{
		CDataObjDescription *pParam = pF->GetParamDescription(i);
		
		sb->AppendFormat(", {0} __input.{1}",
			pParam->IsPassedModeIn() ? "" : "ref ",
			gcnew String(pParam->GetName()));
	}
	return sb;
}
//------------------------------------------------------------------
StringBuilder^ CServiceGenerator::GetOutVariableDeclaration(CFunctionDescription* pF, String^ strFunctionName)
{
	StringBuilder^ sb = gcnew StringBuilder();

	int order = 0;
	if (pF->GetReturnValueDataType() != DataType::Void)
	{
		String^ paramName = String::Concat("ret", gcnew String(pF->GetName()));
		sb->AppendFormat("\r\n\t[MessageBodyMember(Order = {0}, Name=\"return\", Namespace=\"urn:Microarea.Web.Services.{1}\")]", order++, strFunctionName);
		sb->AppendFormat( "\r\n\tpublic {0} {1};", GetOutType(pF), paramName);
	}
	else
	{
	}
	for (int i = 0; i < pF->GetParameters().GetCount(); i++)
	{
		CDataObjDescription *pParam = pF->GetParamDescription(i);
		if (pParam->IsPassedModeIn())
			continue;

		String^ paramName = gcnew String(pParam->GetName());
		sb->AppendFormat("\r\n\t[MessageBodyMember(Order = {0}, Namespace=\"urn:Microarea.Web.Services.{1}\")]", order++, strFunctionName);
		sb->AppendFormat("\r\n\tpublic {0} {1};", 
			CWCFHelper::ManagedTypeFromDataType(pParam),
			paramName
			);
	}

	return sb;
}
//------------------------------------------------------------------
StringBuilder^ CServiceGenerator::GetContextHandleFunction(CFunctionDescription* pF)
{
	StringBuilder^ sb = gcnew StringBuilder();
	if (pF->IsContextFunction())
		sb->AppendFormat("\r\n\tpublic override int GetContextHandle() {{ return handle;  }}");
	return sb;
}
	
//------------------------------------------------------------------
String^ CServiceGenerator::GetFunctionNamespace(CFunctionDescription* pF)
{
	String^ ns = gcnew String(pF->GetNamespace().ToString());
	return ns->Replace("Function.", "");
}

//------------------------------------------------------------------
StringBuilder^ CServiceGenerator::GetFunctionNamespaceFunction(CFunctionDescription* pF)
{
	StringBuilder^ sb = gcnew StringBuilder();
	sb->AppendFormat("\r\n\tpublic override string GetFunctionNamespace() {{ return \"{0}\";  }}", GetFunctionNamespace(pF));
	return sb;
}
	
//------------------------------------------------------------------
StringBuilder^ CServiceGenerator::GetInVariableDeclaration(CFunctionDescription* pF, String^ strFunctionName)
{
	StringBuilder^ sb = gcnew StringBuilder();

	int order = 0;
	for (int i = 0; i < pF->GetParameters().GetCount(); i++)
	{
		CDataObjDescription *pParam = pF->GetParamDescription(i);
		if (pParam->IsPassedModeOut())
			continue;
	
		String ^paramName = gcnew String(pParam->GetName());
		sb->AppendFormat("\r\n\t[MessageBodyMember(Order = {0}, Namespace=\"urn:Microarea.Web.Services.{1}\")]", order++, strFunctionName);
		sb->AppendFormat("\r\n\tpublic {0} {1};", 
			CWCFHelper::ManagedTypeFromDataType(pParam),
			paramName
			);
	}
	return sb;
}
