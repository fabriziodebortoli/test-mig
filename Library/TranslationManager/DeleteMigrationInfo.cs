using System;
using System.IO;

using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.Library.TranslationManager;


namespace Microarea.Library.TranslationManager
{


	public class DeleteMigrationInfo: BaseTranslator
	{

		protected TranslationManager	tm;

		public DeleteMigrationInfo()
		{
			//
			// TODO: Add constructor logic here
			//
		}
		public override string ToString()
		{
			return "Delete MigrationInfo.XML e cartelle Upgrade";
		}

		//---------------------------------------------------------------------------
		public override void Run(TranslationManager	tm)
		{
			this.tm = tm;

			SetProgressMessage("	Cerco e cancello i MigrationInfo.xml ");
			SeachFilesToDelete();
			SetProgressMessage("	Cancello cartelle Upgrade ");
			DeleteDataBaseScriptUpGrade();
			EndRun(false);
		}

		//---------------------------------------------------------------------------
		private void DeleteDataBaseScriptUpGrade()
		{
			if(tm == null || tm.GetApplicationInfo().PathFinder == null)
				return;

			string enumsFile = string.Empty;
			//Loop su ogni modulo dell'applizaione
			foreach(BaseModuleInfo mod in tm.GetApplicationInfo().Modules)
			{
				if (mod == null)
					return;

				string upGradePath = mod.GetDatabaseScriptPath();
				if (upGradePath == string.Empty)
					continue;

				upGradePath = Path.Combine(upGradePath, "Upgrade");

				if (!Directory.Exists(upGradePath))
					continue;
				
				RecoursiveDeleteDirectory(upGradePath);
				
			}
		}

		//---------------------------------------------------------------------------
		private void RecoursiveDeleteDirectory(string upGradePath)
		{

			if (upGradePath == null || upGradePath == string.Empty ) 
				return;

			if (!Directory.Exists(upGradePath))
				return;

			foreach(string dir in Directory.GetDirectories(upGradePath))
				RecoursiveDeleteDirectory(dir);

			foreach(string file in Directory.GetFiles(upGradePath))
				File.Delete(file);

			Directory.Delete(upGradePath);
		}
		
		//---------------------------------------------------------------------------
		public void SeachFilesToDelete()
		{
			if(tm == null || tm.GetApplicationInfo().PathFinder == null)
				return;

			string enumsFile = string.Empty;
			//Loop su ogni modulo dell'applizaione
			foreach(BaseModuleInfo mod in tm.GetApplicationInfo().Modules)
			{
				if (mod == null)
					return;

				string path  = mod.GetMigrationNetPath();
					if (path == string.Empty)
						continue;

				if (Directory.Exists(path))
					DeleteFile(path);

				path  = mod.GetMigrationXpPath();
				if (path == string.Empty)
					continue;

				if (Directory.Exists(path))
					DeleteFile(path);
				
			}
		}

		//---------------------------------------------------------------------------
		private void DeleteFile(string path)
		{
			string file = Path.Combine(path, "MigrationInfo.xml");
			if (File.Exists(file))
				File.Delete(file);
		}

	}
}
