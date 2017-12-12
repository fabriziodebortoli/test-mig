using System;
using System.Collections;
using System.IO;
using System.Windows.Forms;
using System.Xml;

using Microarea.TaskBuilderNet.Core.NameSolver;

namespace Microarea.Library.TranslationManager
{
	public class LibCopy : SolutionManagerItems
	{
		public LibCopy()
		{
			defaultLookUpType = LookUpFileType.Structure;
		}

		public override void Run(TranslationManager tManager)
		{
			TxtReader = new RichTextBox();

			bool bMove = true;

			transManager = tManager;

			if (!OpenLookUpDocument(false))
				return;

			XmlNode nMain = xLookUpDoc.SelectSingleNode("Application");
			string oldAppDir = Path.Combine(transManager.GetApplicationParentPath(), nMain.Attributes["source"].Value.ToString());
			string newAppDir = Path.Combine(transManager.GetApplicationParentPath(), nMain.Attributes["target"].Value.ToString());
			if (!Directory.Exists(newAppDir))
				Directory.CreateDirectory(newAppDir);

			//Leggo i moduli
			foreach (XmlNode nModule in nMain.SelectNodes("Module"))
			{
				string oldModDir = Path.Combine(oldAppDir, nModule.Attributes["source"].Value.ToString());
				string newModDir = Path.Combine(newAppDir, nModule.Attributes["target"].Value.ToString());
				if (!Directory.Exists(newModDir))
					Directory.CreateDirectory(newModDir);

				//Leggo le library
				foreach (XmlNode nLibrary in nModule.SelectNodes("Library"))
				{
					string oldLibDir = Path.Combine(oldModDir, nLibrary.Attributes["sourcefolder"].Value.ToString());
					string newLibDir = Path.Combine(newModDir, nLibrary.Attributes["destinationfolder"].Value.ToString());
					if (!Directory.Exists(newLibDir))
						Directory.CreateDirectory(newLibDir);

					SetProgressMessage(string.Format("Elaborazione in corso: libreria {0}", nLibrary.Attributes["target"].Value.ToString()));

					//Copio i files
					DirectoryInfo di = new DirectoryInfo(oldLibDir);
					foreach (FileInfo fi in di.GetFiles())
					{
						if (!File.Exists(Path.Combine(newLibDir, GetConversion(fi.Name, nLibrary))))
						{
							try
							{
								string newName = Path.Combine(newLibDir, GetConversion(fi.Name, nLibrary));
								File.Copy(fi.FullName, newName, bMove);
								if (bMove && string.Compare(fi.Name, GetConversion(fi.Name, nLibrary), true) != 0)
								{
									File.SetAttributes(fi.FullName, FileAttributes.Normal);
									File.Delete(fi.FullName);
								}
								File.SetAttributes(newName, FileAttributes.Normal);
								if (fi.Name.ToLower() == "beginh.dex")
									ChangeBeginH(Path.Combine(newLibDir, GetConversion(fi.Name, nLibrary)), nLibrary.Attributes["source"].Value.ToString(), nLibrary.Attributes["target"].Value.ToString());
							}
							catch (Exception ex)
							{
								SetLogError("Errore: " + ex.Message, ToString());
							}
						}
					}

					//Cerco e copio la folder res
					foreach (DirectoryInfo cdi in di.GetDirectories())
					{
						if (cdi.Name.ToLower() != "res")
							continue;

						if (!Directory.Exists(Path.Combine(Path.Combine(newLibDir, "res"), cdi.Name)))
						{
							FolderCopy(cdi.FullName, Path.Combine(newLibDir, "res"));
						}

						DirectoryInfo resDI = new DirectoryInfo(Path.Combine(newLibDir, "res"));

						foreach (FileInfo resFI in resDI.GetFiles("*.bmp"))
						{
							File.SetAttributes(resFI.FullName, FileAttributes.Normal);
							File.Move(resFI.FullName, Path.Combine(resDI.FullName, GetConversion(resFI.Name)));
						}
					}

					//Cancello i files relativi a SourceSafe
					di = new DirectoryInfo(newLibDir);
					foreach (FileInfo fi in di.GetFiles("*.scc"))
					{
						File.SetAttributes(fi.FullName, FileAttributes.Normal);
						File.Delete(fi.FullName);
					}
					foreach (FileInfo fi in di.GetFiles("*.vspscc"))
					{
						File.SetAttributes(fi.FullName, FileAttributes.Normal);
						File.Delete(fi.FullName);
					}
				}
			}
			
			EndRun(false);
		}

		private void FolderCopy(string sourceFolderPath, string destinationFolderPath)
		{
			if (Directory.Exists(destinationFolderPath))
				return;

			Directory.CreateDirectory(destinationFolderPath);

			DirectoryInfo di = new DirectoryInfo(sourceFolderPath);
			foreach (FileInfo fi in di.GetFiles())
			{
				if (!File.Exists(Path.Combine(destinationFolderPath, fi.Name)))
				{
					try
					{
						File.Copy(fi.FullName, Path.Combine(destinationFolderPath, fi.Name), false);
						File.SetAttributes(Path.Combine(destinationFolderPath, fi.Name), FileAttributes.Normal);
					}
					catch (Exception ex)
					{
						SetLogError("Errore: " + ex.Message, ToString());
					}
				}
			}
		}

		private void ChangeBeginH(string fileName, string oldLibName, string newLibName)
		{
			TxtReader.LoadFile(fileName, RichTextBoxStreamType.PlainText);

			string sSource = "_" + oldLibName + "EXT";
			string sTarget = "_" + newLibName.ToUpper() + "EXT";

			int startDir = TxtReader.Text.ToLower().IndexOf(sSource.ToLower());
			if (startDir < 0)
				return;

			string inizio = TxtReader.Text.Substring(0, startDir);
			string fine = TxtReader.Text.Substring(startDir + sSource.Length);

			TxtReader.Text = inizio + sTarget + fine;
			
			TxtReader.SaveFile(fileName, RichTextBoxStreamType.PlainText);
		}

		public override string ToString()
		{
			return "Copia delle cartelle di library";
		}
	}
}
