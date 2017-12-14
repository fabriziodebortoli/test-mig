
using namespace System::Collections;
using namespace System::Reflection;

class IHostApplication;
class CDataObjDescription;

public ref class CWCFHelper
{
	//mappa funzioni invocabili senza avere authentication token
	static System::Collections::Generic::List<System::String^>^ freeMap;	
	//lista assembly non caricabili da TaskBuilder perche non nel suo percorso di esecuzione (cache per evitare troppi accessi a file system) 
	static System::Collections::Generic::List<System::String^>^ nonResolvableAssemblies;
	//mappa funzioni invocabili senza avere authentication token che corrisponde ad un login context 
	//(tipicamente quello della Console di amministrazione)
	static System::Collections::Generic::List<System::String^>^ freeMapForAdmin;
	static System::String^ adminToken;
	static CWCFHelper();
public: 
	static void SetAdminToken(System::String^ token) { adminToken = token; }
	static void SetHost(IHostApplication* pHost, int nTbLoaderSOAPPort, int nTbLoaderTCPPort);
	static void SetWebServicesTimeout(int nTimeout);
	static bool EnableSoapExecutionControl(bool bEnable);
	static bool IsFreeWcfFunction(System::String^ functionNamespace, System::String^ authenticationToken);
	static Object^ DataObjToObject(DataObj* pDataObj, System::Type^ destType);
	static void AssignObjectToDataObj(Object^ obj, DataObj* pDataObj);
	static System::String^ ManagedTypeFromDataType(CDataObjDescription* pParam);
	static System::Type^ ManagedTypeFromDataType(DataType type);
	static bool IsArrayType(CDataObjDescription* pParam);
	static System::IntPtr GetThreadHwndFunction(System::String^ authenticationToken, int contextHandle, System::String^ functionNamespace, bool checkAuthToken);
	static void LoadTBDllFunction(System::String^ dllName);
	static int GetSOAPPort();
	static int GetTCPPort();
	static System::TimeSpan GetWebServicesTimeout();
	static void AddNonResolvableAssembly(System::String^ asmName);
	static bool IsNonResolvableAssembly(System::String^ asmName);
	static System::IntPtr GetThreadMainWnd();
	static System::Object^ Invoke(System::IntPtr hwnd, System::Delegate^ method);
};