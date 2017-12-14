using Microarea.EasyBuilder.Properties;
using Microarea.EasyBuilder.UI;
using Microarea.TaskBuilderNet.Core.EasyBuilder;
using Microarea.TaskBuilderNet.Interfaces;
using Microarea.TaskBuilderNet.Core.NameSolver;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Microarea.EasyBuilder
{

	///===============================================================================================================
	internal class SourcesChangesSeeker
	{
		enum SourceExternalChanges { GenericError, NoSources, NoChanges, UserMethodChanged, DesignerSourceChanged };

		public SourcesChangesSeeker()
		{
			
		}

		//-----------------------------------------------------------------------------
		private static SourceExternalChanges CheckForExternalCodeModifications(string filePath)
		{
			string path = BasePathFinder.GetDesignerSourcesPathFromDll(filePath);
			string[] files = BasePathFinder.GetCSharpFilesIn(path);
			
			if (files == null || files.Length == 0)
				return SourceExternalChanges.NoSources;

			FileInfo fi = new FileInfo(filePath);
			if (!fi.Exists)
			{
				return SourceExternalChanges.NoSources;
			}
			DateTime dllLastWriteTime = fi.LastWriteTimeUtc;

			for (int i = files.Length - 1; i >= 0; i--)
			{
				fi = new FileInfo(files[i]);
				if (!fi.Exists)
					continue;

				if (fi.Name.EndsWith("usermethods.cs") && fi.LastWriteTimeUtc > dllLastWriteTime)
					return SourceExternalChanges.UserMethodChanged;

				if (fi.LastWriteTimeUtc > dllLastWriteTime)
					return SourceExternalChanges.DesignerSourceChanged;
			}

			return SourceExternalChanges.NoChanges;
		}

		//-----------------------------------------------------------------------------
		private static bool ThereAreChanges(SourceExternalChanges externalChanges)
		{
			return (externalChanges == SourceExternalChanges.UserMethodChanged || externalChanges == SourceExternalChanges.DesignerSourceChanged);
		}

		//-----------------------------------------------------------------------------
		private static Results ApplyExternalCodeModifications(Control parent, SourceExternalChanges externalChanges, string filePath)
		{
			DialogResult res;
			if (externalChanges == SourceExternalChanges.UserMethodChanged)
				res = MessageBox.Show(parent, Resources.SourcesUpdatedAskForBuildAndRun, NameSolverStrings.EasyStudioDesigner, MessageBoxButtons.OKCancel);
			else
				res = MessageBox.Show(parent, Resources.SourcesUpdatedAskForBuildAndRun, NameSolverStrings.EasyStudioDesigner, MessageBoxButtons.YesNoCancel);

			
			if (res == DialogResult.Cancel || res == DialogResult.No)
				return new Results(res, false);


			string fileBackup = string.Concat(filePath, NameSolverStrings.BakExtension);

			try
			{
				//backuppo la dll vecchia
				if (File.Exists(fileBackup))
					File.Delete(fileBackup);

				File.Move(filePath, fileBackup);
				// nel metodo ManageBuildFiles ci sarà il passaggio da bak a dll
			}
			catch (Exception)
			{
			}
			return new Results( res, true, filePath); ; 
		}

		//-----------------------------------------------------------------------------
		public static Results SeekChanges(Control parent, string sourcesPath)
		{
			SourceExternalChanges externalChanges = CheckForExternalCodeModifications(sourcesPath);

			if (ThereAreChanges(externalChanges))
				return ApplyExternalCodeModifications(parent, externalChanges, sourcesPath);

			return new Results(DialogResult.No, false);
		}

	}

	///===============================================================================================================
	internal class Results
	{
		string path = "";
		bool cancel;
		bool executeBuild = false;

		//-----------------------------------------------------------------------------
		public bool Cancel { get { return cancel; } }

		//-----------------------------------------------------------------------------
		public bool ExecuteBuild { get { return executeBuild; } }

		//-----------------------------------------------------------------------------
		public string Path { get { return path; } }

		//-----------------------------------------------------------------------------
		public Results( DialogResult r, bool executeBuild, string p = "")
		{
			path = p;
			cancel = (r == DialogResult.Cancel);
			this.executeBuild = executeBuild;
		}

	}

}
