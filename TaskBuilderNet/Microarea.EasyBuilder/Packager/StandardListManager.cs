using System;
using System.IO;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.EasyBuilder.Packager
{
	//=========================================================================
	class StandardListManager : AbstractListManager
	{
		//---------------------------------------------------------------------
		public StandardListManager(string application, string module, string myEasyBuilderAppFolder)
			: base(application, module, myEasyBuilderAppFolder)
		{
			CustomListFullPath =
				Path.Combine(
					Path.GetDirectoryName(MyEasyBuilderAppFolder),//la custom list sta nella cartella che contiene la customizzazione.
					module + NameSolverStrings.StandardListFileExtension
					);
		}

		//-----------------------------------------------------------------------------
		protected override ItemSource GetDefaultValueForItemSource()
		{
			return ItemSource.Standard;
		}

		//--------------------------------------------------------------------------------
		protected override string PurgePath(string fullPath)
		{
			return fullPath.ReplaceNoCase(BasePathFinder.BasePathFinderInstance.GetStandardPath() + "\\", String.Empty);
		}

		//-----------------------------------------------------------------------------
		protected override string RestorePath(string relativePath, ItemSource itemSource)
		{
			//la standardizzazione non salva nulla nella custom.
			return Path.Combine(BasePathFinder.BasePathFinderInstance.GetStandardPath(), relativePath);
		}

		//---------------------------------------------------------------------
		protected override string GetApplicationNameFromPurgedPath(string path)
		{
			string[] tokens = path.Split('\\');
			if (tokens != null && tokens.Length > 2)
				return tokens[1];

			return String.Empty;
		}

		//---------------------------------------------------------------------
		protected override string GetModuleNameFromPurgedPath(string path)
		{
			string[] tokens = path.Split('\\');
			if (tokens != null && tokens.Length > 3)
				return tokens[2];

			return String.Empty;
		}

		//---------------------------------------------------------------------
		protected override string GetDocumentNameFromPurgedPath(string path)
		{
			string[] tokens = path.Split('\\');
			if (tokens != null && tokens.Length > 4)
				return tokens[4];

			return String.Empty;
		}

		//-----------------------------------------------------------------------------
		protected override string GetRootXmlTag()
		{
			return EasyBuilderAppListXML.Element.StandardList;
		}

		//-----------------------------------------------------------------------------
		protected override string GetItemXmlTag()
		{
			return EasyBuilderAppListXML.Element.StandardListItem;
		}
	}
}
