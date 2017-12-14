using System;
using System.Collections;
using System.IO;
using Microarea.TaskBuilderNet.Core.DiagnosticManager;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.Console.Core.FileConverter
{
	//================================================================================
	public interface ISourceSafeWrapper
	{
		bool CheckOutFile(string file);
	}

	//================================================================================
	public class GlobalContext
	{
		public static IBasePathFinder			PathFinder			= null;
		public static ISourceSafeWrapper		SSafe				= null;
		public static LogManager				LogManager				= null;	// per la diagnostica
		
		public static ArrayList					ReportExternalPaths     = null;

		public static DestinationReportsData	ReportsData				= null;

		public static ITableTranslator			TableTranslator			= null;	// per la conversione dei nomi di colonne e tabelle
		public static IFontTranslator			FontTranslator			= null;	// per la conversione dei nomi dei font
		public static IFormatterTranslator		FormatterTranslator		= null;	// per la conversione dei nomi dei formattatori
		public static IFunctionTranslator		FunctionTranslator		= null;	// per la conversione dei nomi delle funzioni
		public static IHotLinkTranslator		HotLinkTranslator		= null;	// per la conversione dei nomi degli hot link
		public static ILinkTranslator			LinkFormTranslator		= null;	// per la conversione dei nomi di LinkForm
		public static ILinkTranslator			LinkReportTranslator	= null;	// per la conversione dei nomi di LinkReport 
		public static IFileObjectTranslator		FileObjectTranslator	= new FileObjectTranslator();		// per la conversione di riferimenti a file
		public static IEnumTranslator			EnumTranslator			= null;	// per la conversione degli enumerativi
		public static IDocumentsTranslator		DocumentsTranslator		= null;	// per la conversione dei namespace di documento
		public static IReportsTranslator		ReportsTranslator		= null;	// per la conversione dei namespace di report 
		public static ILibrariesTranslator		LibrariesTranslator		= null;	// per la conversione dei nomi application-module-library
		public static IActivationsTranslator	ActivationsTranslator	= null;	// per la conversione delle IsActivated
		public static IWoormInfoVariablesTranslator	WoormInfoVariablesTranslator	= null;	// per la conversione delle IsActivated
	
		public static bool BuildTargetPath ()
		{
			string [] p = ReportsData.SourcePath.Split(new Char [] {'/', '\\'} );
			
			int idxApp = ReportsData.Standard ? 6 : 8;
			int idxMod = idxApp + 1;

			string app = p[idxApp];
			string module = app + "." + p[idxMod];

			string newModule = LibrariesTranslator.TranslateModule(module);
			p[idxMod] = newModule.Substring(newModule.IndexOf('.') + 1);
			p[idxApp] = newModule.Substring(0, newModule.IndexOf('.'));

			if (string.Compare(p[2], PathFinder.Installation, true) == 0)
			{
				string prevBak = ReportsData.SourcePath + ".bak";
				if (File.Exists(prevBak))
				{
					// si assicura che non sia read-only prima di cancellarlo
					File.SetAttributes(prevBak, ~FileAttributes.ReadOnly);
					File.Delete(prevBak);
				}
				File.Move(ReportsData.SourcePath, prevBak);
				ReportsData.SourcePath = prevBak;
			}
			else
				p[2] = PathFinder.Installation;

			string tb = "taskbuilder";
			if (p[5].ToLower().StartsWith(tb) && string.Compare(p[4], "standard", true) == 0)
				p[5] = p[5].Substring(tb.Length);
			if (p[7].ToLower().StartsWith(tb) && string.Compare(p[4], "custom", true) == 0)
				p[7] = p[7].Substring(tb.Length);

			ReportsData.TargetPath = p[0];
			for (int i = 1; i <= p.GetUpperBound(0); i++)
				ReportsData.TargetPath += "\\" + p[i];

			return true;
		}
	}	

	// ========================================================================
	public class DestinationReportsData
	{
		public string Installation      = string.Empty;
		public string Name				= string.Empty;
		public string Path				= string.Empty;
		public string SourcePath		= string.Empty;
		public string TargetPath		= string.Empty;
		public string Module			= string.Empty;
		public string Application		= string.Empty;
		public bool   Standard			= false;
		public string Company           = string.Empty;
		public string User				= string.Empty;
		public Diagnostic diagnostic	= null;

		//---------------------------------------------------------------------
		public DestinationReportsData(string name)
		{
			this.Name = name;
			diagnostic = new Diagnostic(name);
		}

		//---------------------------------------------------------------------
		public void Clear()
		{
			Name		= string.Empty;
			Path		= string.Empty;
			SourcePath  = string.Empty;
			TargetPath  = string.Empty;
			Module		= string.Empty;
			Application = string.Empty;
			Standard	= false;
			Company		= string.Empty;
			User		= string.Empty;
			diagnostic.Clear();
		}
	}
}
