using System;
using System.IO;
using System.Xml;

using Microarea.TaskBuilderNet.Core.NameSolver;

namespace Microarea.Library.TranslationManager
{
	public class ModCopy : SolutionManagerItems
	{
		private bool bNonTraduco = false; //TODO Fabio: da decidere se i report sono da rimominare qui o dal migration kit
		private XmlNode nCurrentMod = null;
		private string curAppName, curModName = string.Empty;

		public ModCopy()
		{
			defaultLookUpType = LookUpFileType.Structure;
		}

		public override void Run(TranslationManager tManager)
		{
			transManager = tManager;

			if (!OpenLookUpDocument(false))
				return;

			XmlNode nMain = xLookUpDoc.SelectSingleNode("Application");

			curAppName = nMain.Attributes["source"].Value.ToString();
			string oldAppDir = Path.Combine(transManager.GetApplicationParentPath(), nMain.Attributes["source"].Value.ToString());
			string newAppDir = Path.Combine(transManager.GetApplicationParentPath(), nMain.Attributes["target"].Value.ToString());
			if (!Directory.Exists(newAppDir))
				Directory.CreateDirectory(newAppDir);

			//Leggo i moduli
			foreach (XmlNode nModule in nMain.SelectNodes("Module"))
			{
				nCurrentMod = nModule;
				curModName = nModule.Attributes["source"].Value.ToString();
				string oldModDir = Path.Combine(oldAppDir, nModule.Attributes["source"].Value.ToString());
				string newModDir = Path.Combine(newAppDir, nModule.Attributes["target"].Value.ToString());
				if (!Directory.Exists(newModDir))
					Directory.CreateDirectory(newModDir);

				SetProgressMessage(string.Format("Elaborazione in corso: modulo {0}", nModule.Attributes["target"].Value.ToString()));

				DirectoryInfo di = new DirectoryInfo(oldModDir);
				foreach (DirectoryInfo cdi in di.GetDirectories())
				{
					if (!IsLibrary(nCurrentMod, cdi.Name))
					{
						FolderCopy(cdi.FullName, Path.Combine(newModDir, cdi.Name));
					}
				}

				foreach (FileInfo fi in di.GetFiles("*.h"))
				{
					string newFileName = GetConversion(fi.Name, nModule.Attributes["source"].Value.ToString());
					if (!File.Exists(Path.Combine(newModDir, newFileName)))
					{
						try
						{
							string newName = Path.Combine(newModDir, newFileName);
							File.Copy(fi.FullName, newName, false);
							File.SetAttributes(newName, FileAttributes.Normal);
						}
						catch (Exception ex)
						{
							SetLogError("Errore: " + ex.Message, ToString());
						}
					}
				}

				if (!File.Exists(Path.Combine(newModDir, "Module.config")) && File.Exists(Path.Combine(oldModDir, "Module.config")))
				{
					try
					{
						File.Copy(Path.Combine(oldModDir, "Module.config"), Path.Combine(newModDir, "Module.config"), false);
						File.SetAttributes(Path.Combine(newModDir, "Module.config"), FileAttributes.Normal);
					}
					catch (Exception ex)
					{
						SetLogError("Errore: " + ex.Message, ToString());
					}
				}

				if (!File.Exists(Path.Combine(newModDir, "LocalizableApplication.config")) && File.Exists(Path.Combine(oldModDir, "LocalizableApplication.config")))
				{
					try
					{
						File.Copy(Path.Combine(oldModDir, "LocalizableApplication.config"), Path.Combine(newModDir, "LocalizableApplication.config"), false);
						File.SetAttributes(Path.Combine(newModDir, "LocalizableApplication.config"), FileAttributes.Normal);
					}
					catch (Exception ex)
					{
						SetLogError("Errore: " + ex.Message, ToString());
					}
				}
			}

			if (!File.Exists(Path.Combine(newAppDir, "Application.config")) && File.Exists(Path.Combine(oldAppDir, "Application.config")))
			{
				try
				{
					File.Copy(Path.Combine(oldAppDir, "Application.config"), Path.Combine(newAppDir, "Application.config"), false);
					File.SetAttributes(Path.Combine(newAppDir, "Application.config"), FileAttributes.Normal);
				}
				catch (Exception ex)
				{
					SetLogError("Errore: " + ex.Message, ToString());
				}
			}

			EndRun(false);
		}

		private bool CheckFolder(string folderName)
		{
			return	folderName.ToLower() != "upgrade"		&& 
					folderName.ToLower() != "dictionary"	;
		}

		private void FolderCopy(string sourceFolderPath, string destinationFolderPath)
		{
			if (!Directory.Exists(sourceFolderPath))
				return;

			if (!CheckFolder(sourceFolderPath.Substring(sourceFolderPath.LastIndexOf(@"\") + 1)))
				return;

			if (Directory.Exists(destinationFolderPath))
				return;

			Directory.CreateDirectory(destinationFolderPath);

			DirectoryInfo di = new DirectoryInfo(sourceFolderPath);
			foreach (FileInfo fi in di.GetFiles())
			{
				if (!File.Exists(Path.Combine(destinationFolderPath, fi.Name)))
				{
					string fileName = fi.Name;
					if (di.Name.ToLower() == "report")
						fileName = GetReportFileName(fileName);

					try
					{
						File.Copy(fi.FullName, Path.Combine(destinationFolderPath, fileName), false);
						File.SetAttributes(Path.Combine(destinationFolderPath, fileName), FileAttributes.Normal);
					}
					catch (Exception ex)
					{
						SetLogError("Errore: " + ex.Message, ToString());
					}
				}
			}

			foreach (DirectoryInfo cdi in di.GetDirectories())
			{
				if (!Directory.Exists(Path.Combine(destinationFolderPath, cdi.Name)))
				{
					if (string.Compare(cdi.Name, "Base", true) == 0)
						FolderCopy(cdi.FullName, Path.Combine(destinationFolderPath, "Basic"));
					else if  (string.Compare(cdi.Name, "Manufacturing-Base", true) == 0)
						FolderCopy(cdi.FullName, Path.Combine(destinationFolderPath, "Manufacturing-Basic"));
					else
						FolderCopy(cdi.FullName, Path.Combine(destinationFolderPath, cdi.Name));
				}
			}
		}

		private string GetReportFileName(string fileName)
		{
			if (fileName.ToLower().IndexOf(".wrm") < 0)
				return fileName;

			if (bNonTraduco)
				return fileName;

			string tmpFileName = fileName.Split('.')[0];

			string ns = curAppName + "." + curModName + "." + tmpFileName;

			tmpFileName = transManager.GetNameSpaceConversion(LookUpFileType.Report, ns).Split('.')[2];

			return tmpFileName + ".wrm";
		}

		public override string ToString()
		{
			return "Copia delle cartelle di modulo";
		}
	}
}
