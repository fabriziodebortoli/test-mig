using System;
using System.IO;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.EasyBuilder.Packager
{
	//=========================================================================
	class CustomListManager : AbstractListManager
	{
		//---------------------------------------------------------------------
		public CustomListManager(string application, string module, string myEasyBuilderAppFolder)
			:base(application, module, myEasyBuilderAppFolder)
		{
			CustomListFullPath =
				Path.Combine(
					Path.GetDirectoryName(myEasyBuilderAppFolder),//la custom list sta nella cartella che contiene la customizzazione.
					module + NameSolverStrings.CustomListFileExtension
					);
		}

		//-----------------------------------------------------------------------------
		protected override string GetRootXmlTag()
		{
			return EasyBuilderAppListXML.Element.CustomList;
		}

		//-----------------------------------------------------------------------------
		protected override string GetItemXmlTag()
		{
			return EasyBuilderAppListXML.Element.CustomListItem;
		}

		//-----------------------------------------------------------------------------
		protected override ItemSource GetDefaultValueForItemSource()
		{
			return ItemSource.Custom;
		}

		//-----------------------------------------------------------------------------
		protected override string RestorePath(string relativePath, ItemSource itemSource)
		{
			return Path.Combine(BasePathFinder.BasePathFinderInstance.GetCustomPath(), relativePath);
		}
		
		//--------------------------------------------------------------------------------
		protected override string PurgePath(string fullPath)
		{
			return fullPath.ReplaceNoCase(BasePathFinder.BasePathFinderInstance.GetCustomPath() + "\\", String.Empty);
		}

		//---------------------------------------------------------------------
		protected override string GetApplicationNameFromPurgedPath(string path)
		{
			string[] tokens = path.Split('\\');
			if (tokens != null && tokens.Length > 4)
				return tokens[3];

			return String.Empty;
		}

		//---------------------------------------------------------------------
		protected override string GetModuleNameFromPurgedPath(string path)
		{
			string[] tokens = path.Split('\\');
			if (tokens != null && tokens.Length > 5)
				return tokens[4];

			return String.Empty;
		}

		//---------------------------------------------------------------------
		protected override string GetDocumentNameFromPurgedPath(string path)
		{
			string[] tokens = path.Split('\\');
			if (tokens != null && tokens.Length > 7)
				return tokens[6];

			return String.Empty;
		}
	}
}
