#include "stdafx.h"
#include <TbGeneric\GeneralFunctions.h>
#include <TbGeneric\TBStrings.h>
#include "SourceControl.h"

#using <Microsoft.TeamFoundation.Client.dll>
#using <Microsoft.TeamFoundation.VersionControl.Client.dll> 

using namespace System;
using namespace System::IO;
using namespace System::Reflection;
using namespace System::Windows::Forms;
using namespace Microsoft::TeamFoundation::Client;
using namespace Microsoft::TeamFoundation::VersionControl::Client;
using namespace Microarea::TaskBuilderNet::Core::EasyBuilder;

CSourceControl::CSourceControl()
{

}

//---------------------------------------------------------------------------------------
Assembly^ OnAssemblyResolve(Object^ sender, ResolveEventArgs^ args)
{
	//C:\Program Files (x86)\Microsoft Visual Studio 14.0\Common7\IDE\CommonExtensions\Microsoft\TeamFoundation\Team Explorer\Microsoft.TeamFoundation.Client.dll

	AssemblyName ^an = gcnew AssemblyName(args->Name);
	String^ asmName = an->Name + ".dll";

	String^ sPath = Environment::GetEnvironmentVariable("ProgramFiles(x86)");
	String ^file = Path::Combine(sPath, "Microsoft Visual Studio 14.0\\Common7\\IDE\\CommonExtensions\\Microsoft\\TeamFoundation\\Team Explorer", asmName);

	if (File::Exists(file))
	{
		//Se esiste, lo carico da li nella stessa maniera in cui carico le dll delle
		//customizzazioni per non lock-are i file.
		//Inoltre evito il problema di cui a http://connect.microsoft.com/VisualStudio/feedback/details/545190/assemblyname-getassemblyname-followed-by-assembly-load-do-not-work-with-an-unc-name
		//in cui si incappava con Assembly::Load(an) di cui sotto
		Assembly^ a = AssembliesLoader::Load(file);
		return a;
	}
	return nullptr;
}

bool CSourceControl::CheckOutIfNeeded(System::String^ file)
{
	try
	{
		AppDomain::CurrentDomain->AssemblyResolve += gcnew ResolveEventHandler(&OnAssemblyResolve);

		if ((File::GetAttributes(file) & FileAttributes::ReadOnly) == FileAttributes::ReadOnly)
			return CheckOut(file);
		return true;
	}
	catch (Exception^)
	{
		return false;
	}
	finally
	{
		AppDomain::CurrentDomain->AssemblyResolve -= gcnew ResolveEventHandler(&OnAssemblyResolve);
	}
}
bool CSourceControl::CheckOut(System::String^ file)
{
	WorkspaceInfo^ workspaceInfo = Workstation::Current->GetLocalWorkspaceInfo(file);
	if (workspaceInfo == nullptr)
		return false;
	/*if (MessageWindow::ShowDialog(
		owner,
		gcnew String(_TB("Cannot save because the file is under source control; do you want to check it out?")),
		nullptr,
		MessageBoxButtons::YesNo,
		MessageWindow::ImageType::Warning) != DialogResult::Yes)
		return false;*/
	TfsTeamProjectCollection^ server = nullptr;
	try
	{
		server = gcnew TfsTeamProjectCollection(workspaceInfo->ServerUri);

		Workspace^ workspace = workspaceInfo->GetWorkspace(server);
		return workspace->PendEdit(file) == 1;
	}
	finally
	{
		delete server;
	}
}