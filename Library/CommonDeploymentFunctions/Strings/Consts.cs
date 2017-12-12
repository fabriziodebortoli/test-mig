using System;
using System.Resources;
//
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.Library.Licence;
using ConfConst = Microarea.Library.Licence.Consts;

namespace Microarea.Library.CommonDeploymentFunctions.Strings
{
	//=========================================================================
	public class Consts
	{
		// TODO - usa per tutti NameSolverStrings (con nuova convenzioni di naming)

		// PROPOSAL - per le const usate negli XML usare stesso namespace
		// ma nuovo assembly in modo da evitare troppi cross-references.

		#region XML tags
		public const string TagImage			= "Image";
		public const string TagMacAddress		= "MacAddress";
		public const string TagRunningImage		= "RunningImage"; //NameSolverStrings.Running; //"Running";
		public const string TagSecurityInfo		= "SecurityInfo";
		#endregion

		#region XML attributes
		public const string AttributeDate			= "date";
		public const string AttributeRelease		= ConfConst.AttributeRelease;
		public const string AttributeVersion		= ConfConst.AttributeVersion;
		public const string AttributeName			= "name"; //NameSolverXmlStrings.Name;
		public const string AttributeValue			= "value";
		#endregion

		#region File names
		// Altri file
		public const string FileDependenciesMap			= "DependenciesMap" + NameSolverStrings.XmlExtension;	// TEMP - rimappare, e farlo fare anche in ModuleDependencies.dll
		public const string FileZipUpdates				= NameSolverStrings.FileZipUpdates;
		public const string FileServicesSetup			= "ServicesSetup.msi";
		public const string FileUpdatingServicesHelp	= "UpdatingMicroareaServices.htm";
		// Extension
		public const string ExtensionProductSolution	= ".Solution.xml";	// TODO - spostare in Licence
		public const string ExtensionLicensed			= NameSolverStrings.LicensedExtension;
		public const string ExtensionRtp				= NameSolverStrings.RtpExtension;
		#endregion

		#region Directories
		public const string DirUniversalImages	= "UniversalImages";	// dir contenente immgini di prodotti	
		public const string DirServicesImages	= "Services";			// dir contenente immagini di servizi
		private const string DirImages			= "Images";
		public const string DirServices			= NameSolverStrings.Services;
		public const string DirMicroareaServer	= NameSolverStrings.MicroareaServer;
		public const string DirRunning			= NameSolverStrings.Running; //"Running";
		private const string DirStandard		= NameSolverStrings.Standard; //"Standard";
		public const string DirBeforeRunning	= NameSolverStrings.BeforeRunning;	// TEMP - usare pathFinder.Get...
		public const string DirDownLoadCache	= NameSolverStrings.DownLoadCache;	// TEMP - usare pathFinder.Get...
		public const string DirDownLoadImage	= NameSolverStrings.DownLoadImage;	// TEMP - usare pathFinder.Get...
		public const string DirDictionary		= NameSolverStrings.Dictionary;
		public const string DirSolutions		= NameSolverStrings.Solutions;
		public const string DirApplications		= NameSolverStrings.TaskBuilderApplications;
		public const string DirTaskBuilder		= NameSolverStrings.TaskBuilder;
		public const string DirWebFramework		= NameSolverStrings.WebFramework;
		public const string DirSolutionsUpdate	= NameSolverStrings.SolutionsUpdate;
		public const string DirCustom			= NameSolverStrings.Custom;
		public const string DirLocalState		= NameSolverStrings.LocalState;
		public const string DirBin				= NameSolverStrings.Bin;
		public const string DirSetup			= "Setup";
		public const string DirLicenses			= NameSolverStrings.Licenses;
		#endregion

		#region Paths
		public const string SubPathImages			= DirMicroareaServer + "\\" + DirImages;// "MicroareaServer\Images";
		#endregion

		#region Generic values
		public const string XmlDeclarationEncoding	= "UTF-8";
		public const string XmlDeclarationVersion	= "1.0";
		//public const string Microarea = NameSolverStrings.Microarea;
		public const string HttpPrefix				= "http://";
		#endregion
	}

	//=========================================================================
	public class Strings
	{
		private static ResourceManager resources = new ResourceManager(typeof(Strings));
		
		public static string Result {  get { return resources.GetString("Result"); } }
		public static string Hour {  get { return resources.GetString("Hour"); } }
		public static string Date {  get { return resources.GetString("Date"); } }
		public static string Details {  get { return resources.GetString("Details"); } }
		public static string Installation {  get { return resources.GetString("Installation"); } }
		public static string Product {  get { return resources.GetString("Product"); } }
		
	}
}