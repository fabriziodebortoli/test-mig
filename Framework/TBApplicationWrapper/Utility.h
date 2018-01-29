#pragma once

#ifdef GetEnvironmentVariable
#undef GetEnvironmentVariable
#endif

#include "EventsMacros.h"
#include <tbgenlibmanaged\usercontrolwrappers.h>
#include <TbGenlib\BaseTileDialog.h>
#include "MWorkers.h"

using namespace Microarea::TaskBuilderNet::Interfaces::Model;
using namespace Microarea::TaskBuilderNet::Interfaces::View;
using namespace Microarea::TaskBuilderNet::Interfaces;

//=============================================================================
public enum class ETileDialogSize {
	Mini = 1,		// TILE_MINI
	Standard = 2,	//TILE_STANDARD
	Wide = 4,		//TILE_WIDE
	AutoFill = 5	//TILE_AUTOFILL
};
/// <summary>
/// Internal Use
/// </summary>
public ref class CUtility
{
private:
	/// <summary>
	/// Internal Use
	/// </summary>
	static bool ReloadApplicationWithManagedState	(System::String^ appName);
public:

	/// <summary>
	/// Internal Use
	/// </summary>
	static void AddEnvironmentVariable(System::String^ name, System::String^ newVar)
	{
		System::String^ var = System::Environment::GetEnvironmentVariable(gcnew System::String(name));
		if (var == nullptr)
			var = "";
		if (!var->Contains(newVar))
		{
			var = newVar + ";" + var;
			System::Environment::SetEnvironmentVariable(name, var);
		}
	}

	/// <summary>
	/// Internal Use
	/// </summary>
	static WeifenLuo::WinFormsUI::Docking::DockPanel^ GetHostingPanel(System::IntPtr documentPtr);

	/// <summary>
	/// Internal Use
	/// </summary>
	static WeifenLuo::WinFormsUI::Docking::DockPanel^ GetHostingPanelFromHandle(System::IntPtr dockPanelHandle);

	/// <summary>
	/// Internal Use
	/// </summary>
	static System::String^ GetDocumentTitle(System::String^ docNamespace);

	/// <summary>
	/// Internal Use
	/// </summary>
	static bool CanUseDataEntryNamespace(System::String^ docNamespace);	//using in EasyAttachment to manage archived document security.If the user can run dataentry he can read the dataentry attachment

	/// <summary>
	/// Internal Use
	/// </summary>
	static System::IntPtr CUtility::GetMainFormHandle();

	/// <summary>
	/// Internal Use
	/// </summary>
	static bool IsAdmin();
	
	/// <summary>
	/// Internal Use
	/// </summary>
	static bool ReloadApplication	(System::String^ appName);

	/// <summary>
	/// Internal Use
	/// </summary>
	static void ReinitActivationInfos();
	
	/// <summary>
	/// Internal Use
	/// </summary>
	static System::String^ GetUser();

	static bool IsActivated(System::String^ application, System::String^ module);

	static bool IsRemoteInterface();
	
	/// <summary>
	/// Internal Use
	/// </summary>
	static System::Data::IDbConnection^ OpenConnectionToCurrentCompany();

	/// <summary>
	/// Internal Use
	/// </summary>
	static System::String^ GetCompany();
	
	/// <summary>
	/// Internal Use
	/// </summary>
	static System::String^ GetAuthenticationToken(); 
	
	/// <summary>
	/// Internal Use
	/// </summary>
	static int GetCompanyId();

	/// <summary>
	/// Internal Use
	/// </summary>
	static int GetCompanyId(System::String^ token);
	
	/// <summary>
	/// Internal Use
	/// </summary>
	static System::DateTime GetApplicationDate();
	
	/// <summary>
	/// Internal Use
	/// </summary>
	static int GetDataBaseCultureLCID();
	
	/// <summary>
	/// Internal Use
	/// </summary>
	static int GetAllOpenDocumentNumber();

	/// <summary>
	/// Internal Use
	/// </summary>
	static int GetAllOpenDocumentNumberEditMode();
	
	/// <summary>
	/// Internal Use
	/// </summary>
	static int GetLoginSessionCount();
	
	/// <summary>
	/// Internal Use
	/// </summary>
	static void RefreshMenuDocument	();

	/// <summary>
	/// Internal Use
	/// </summary>
	static void RefreshLogin();

	/// <summary>
	/// Internal Use
	/// </summary>
	static void ShowImmediateBalloon();

	/// <summary>
	/// Internal Use
	/// </summary>
	static void ReloadAllMenus();
	/// <summary>
	/// Internal Use
	/// </summary>
	static void AddWindowRef(System::IntPtr  hwndModalWindow, bool modal);
	/// <summary>
	/// Internal Use
	/// </summary>
	static void RemoveWindowRef(System::IntPtr  hwndModalWindow, bool modal);

	/// <summary>
	/// Internal Use
	/// </summary>
	static void GetItemSources(System::Collections::Generic::List<System::String^>^ appTitles, System::Collections::Generic::List<System::String^>^ modTitles, System::Collections::Generic::List<System::String^>^ titles, System::Collections::Generic::List<System::String^>^ namespaces);

	/// <summary>
	/// Internal Use
	/// </summary>
	static void GetValidators(System::Collections::Generic::List<System::String^>^ appTitles, System::Collections::Generic::List<System::String^>^ modTitles, System::Collections::Generic::List<System::String^>^ titles, System::Collections::Generic::List<System::String^>^ namespaces);

	/// <summary>
	/// Internal Use
	/// </summary>
	static void GetHotLinks(System::Collections::Generic::List<System::String^>^ appTitles, System::Collections::Generic::List<System::String^>^ modTitles, System::Collections::Generic::List<System::String^>^ titles, System::Collections::Generic::List<System::String^>^ namespaces);

	/// <summary>
	/// Internal Use
	/// </summary>
	static bool SendAsAttachments(System::Collections::Generic::List<System::String^>^ attachmentsFiles, System::Collections::Generic::List<System::String^>^ attachmentsTitles, System::String^ errorMsg);

	/// <summary>
	/// Internal Use
	/// </summary>
	static System::String^ GetAppTitleByAppName(System::String^ appName);

	///<summary>
	/// Internal Use
	///</summary>
	static System::String^ GetModuleTitleByAppAndModuleName(System::String^ appName, System::String^ modName);
	
	/// <summary>
	/// Internal Use
	/// </summary>
	static void GetDocuments
					(
						System::Collections::Generic::List<System::Tuple<System::String^, System::String^, System::String^, System::String^>^>^ docsInfos
					);

	
	/// <summary>
	/// Internal Use
	/// </summary>
	//static bool AttachFile(System::String^ fileName, System::String^ description, System::Int64 documentPtr, System::String^ errorMsg);
	/// <summary>
	/// Internal Use%
	/// </summary>
	static bool AttachArchivedDocument(System::Int32 archiveDocId, System::Int64 documentPtr, System::Int32% attachmentId, System::String^ errorMsg);

	/// <summary>
	/// Internal Use%
	/// </summary>
	static bool CUtility::CreateNewSosDocument(System::Int64 documentPtr, System::Int32 attachmentId, System::String^% errorMsg);

	/// <summary>
	/// Internal Use
	/// </summary>
	static bool AddEnumTag (Microarea::TaskBuilderNet::Core::Applications::EnumTag^ enumTag);
	/// <summary>
	/// Internal Use
	/// </summary>
	static bool DeleteEnumTag (Microarea::TaskBuilderNet::Core::Applications::EnumTag^ enumTag);

	/// <summary>
	/// Internal Use
	/// </summary>
	static bool AddEnumItem (Microarea::TaskBuilderNet::Core::Applications::EnumItem^ enumItem);
	/// <summary>
	/// Internal Use
	/// </summary>
	static bool DeleteEnumItem (Microarea::TaskBuilderNet::Core::Applications::EnumItem^ enumItem);

	/// <summary>
	/// Internal Use
	/// </summary>
	static void GetDataFiles(System::Collections::Generic::List<System::String^>^ appTitles, System::Collections::Generic::List<System::String^>^ modTitles, System::Collections::Generic::List<System::String^>^ namespaces, bool useCountry);

	/// <summary>
	/// Internal Use
	/// </summary>
	static void FireDMS_MassiveRowProcessed(System::Int32 archiveDocId, System::Int32 actionToDo, System::Int32 massiveResult, System::Collections::Generic::List<System::Int32>^ attachmentsList);

	/// <summary>
	/// Internal Use
	/// </summary>
	static void FireDMS_MassiveProcessTerminated();
	/// <summary>
	/// Internal Use
	/// </summary>
	static System::String^ CryptString(System::String^ s);

	/// <summary>
	/// Internal Use
	/// </summary>
	static Object^ GetWebServiceStateObject(long handle);
	/// <summary>
	/// Internal Use
	/// </summary>
	static long CreateWebServiceStateObject(System::String^ type, ...array<Object^> ^args);
	/// <summary>
	/// Internal Use
	/// </summary>
	static bool RemoveWebServiceStateObject(long handle);

	/// <summary>
	/// Returns extended information about current worker logged
	/// </summary>
	static Microarea::Framework::TBApplicationWrapper::MWorker^ GetCurrentWorker();

	/// <summary>
	/// Returns current worker ID
	/// </summary>
	static int GetWorkerId();

	/// <summary>
	/// Returns worker ID of a specific authentication token
	/// </summary>
	static int  GetWorkerId(System::String^ token);

	/// <summary>
	/// Internal Use
	/// </summary>
	static System::String^ GetViewManagedTypeName(int documentHandle, INameSpace^ documentPartNamespace);

	/// <summary>
	/// Internal Use
	/// </summary>
	static bool RunDocument(System::String^ command, System::String^ arguments);

	/// <summary>
	/// Internal Use
	/// </summary>
	static bool IsStaticArea(UINT nID) { return (nID == IDC_STATIC_AREA || nID == IDC_STATIC_AREA_2 || nID == IDC_STATIC_AREA_3); }

	/// <summary>
	/// Internal Use
	/// </summary>
	static bool IsLoginContextValid(System::String^ token);

	/// <summary>
	/// Internal Use
	/// </summary>
	static System::String^ GetJsonContext(System::String^ file);

	static TileDialogSize TransfertEnumTileSize(ETileDialogSize tdSize);

	/*inizio get per le min size dei controlli*/
	static System::Drawing::Size GetIdealTileSizeLU(ETileDialogSize eSize);

	static System::Drawing::Size GetIdealLabelSize();

	static System::Drawing::Size GetIdeaGenericComboBoxSize();

	static System::Drawing::Size GetIdealCheckRadioButtonSize();

	static System::Drawing::Size Get100x100Size();

	static System::Drawing::Size Get200x200Size();

	static System::Drawing::Size GetIdealBodyEdiytSize();

	static System::Drawing::Size GetIdealBaseWindowWrapperSize();

	static System::Drawing::Size GetIdealTabberTileManagerSize();

	static System::Drawing::Size GetIdealToolbarSize(bool getMinSize);

	static System::Drawing::Size GetIdealStaticAreaSize();

	static System::Drawing::Size GetIdealTreeViewSize();

	static System::Drawing::Size GetIdealViewSize();

	static System::Drawing::Size GetIdealTileGroupSize();

	static System::Drawing::Size GetIdealHeaderStripSize();

	/*fine get per le min size dei controlli*/
};

class CObjectWrapper : public CObjectWrapperObj
{
	gcroot<System::Object^> m_WrappedObject;
public:
	CObjectWrapper(System::Object^ obj) 
	{
		m_WrappedObject = obj;
	}

	virtual ~CObjectWrapper()
	{
		m_WrappedObject = nullptr;
	}

};

//=============================================================================
public ref class PathFinderWrapper
{
public:
	PathFinderWrapper();

	/// <summary>
	/// Internal Use
	/// </summary>	
public:
	static System::String^ GetCustomApplicationsPath();
	static bool ExistFile(System::String^ path);
	static bool ExistFolder(System::String^ path);
	static System::String^ GetTemplatesPath(bool inCustom);
	static System::Collections::Generic::List<System::String^>^ GetFiles(System::String^ path, System::String^ searchKey);
};

