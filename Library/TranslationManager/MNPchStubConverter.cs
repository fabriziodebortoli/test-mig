using System;
using System.Collections;
using System.Xml;
using System.IO;
using System.Windows.Forms;
using Microarea.TaskBuilderNet.Core.NameSolver;

namespace Microarea.Library.TranslationManager
{
	public class MNPchStubConverter : SolutionManagerItems
	{
		public MNPchStubConverter()
		{
			defaultLookUpType = LookUpFileType.Structure;
		}

		public override string ToString()
		{
			return "MNPchStub Converter";
		}

		public override void Run(TranslationManager tManager)
		{
			TxtReader = new RichTextBox();

			transManager = tManager;

			if (!OpenLookUpDocument(false))
				return;

			XmlNode nMain = xLookUpDoc.SelectSingleNode("Application");

			string oldAppDir = transManager.GetApplicationInfo().Path;
			string newAppDir = transManager.GetNewApplicationInfo().Path;
			
			if (!Directory.Exists(newAppDir))
			{
				EndRun(false);
				return;
			}

			FolderCopy(Path.Combine(oldAppDir, "MNPchStub"), Path.Combine(newAppDir, "ERPPchStub"));

			foreach (BaseModuleInfo mi in transManager.GetNewApplicationInfo().Modules)
			{
				foreach (LibraryInfo li in mi.Libraries)
				{
					DirectoryInfo di = new DirectoryInfo(li.FullPath);
					foreach (FileInfo fi in di.GetFiles("*.vcproj"))
					{
						FindAndReplaceNoCaseInFile(fi.FullName, "MNPchStub", "ERPPchStub");
					}
				}
			}

			EndRun(false);
		}

		private void FolderCopy(string sourceFolderPath, string destinationFolderPath)
		{
			if (!Directory.Exists(sourceFolderPath))
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
					fileName = fileName.Replace("MNPchStub", "ERPPchStub");

					try
					{
						File.Copy(fi.FullName, Path.Combine(destinationFolderPath, fileName), false);
						File.SetAttributes(Path.Combine(destinationFolderPath, fileName), FileAttributes.Normal);
					}
					catch (Exception ex)
					{
						SetLogError("Errore: " + ex.Message, ToString());
					}

					FindAndReplaceNoCaseInFile(Path.Combine(destinationFolderPath, fileName), "MNPchStub", "ERPPchStub");
				}
			}
		}

		private void FindAndReplaceInFile(string fileName, string source, string destination)
		{
			try
			{
				TxtReader.LoadFile(fileName, RichTextBoxStreamType.PlainText);
				TxtReader.Text = TxtReader.Text.Replace(source, destination);
				File.SetAttributes(fileName, FileAttributes.Normal);
				TxtReader.SaveFile(fileName, RichTextBoxStreamType.PlainText);
			}
			catch (Exception ex)
			{
				SetLogError("Errore: " + ex.Message, ToString());
			}
		}

		private void FindAndReplaceNoCaseInFile(string fileName, string source, string destination)
		{
			try
			{
				TxtReader.LoadFile(fileName, RichTextBoxStreamType.PlainText);
				TxtReader.Text = ReplaceNoCase(TxtReader.Text, source, destination);
				File.SetAttributes(fileName, FileAttributes.Normal);
				TxtReader.SaveFile(fileName, RichTextBoxStreamType.PlainText);
			}
			catch (Exception ex)
			{
				SetLogError("Errore: " + ex.Message, ToString());
			}
		}

		private string ReplaceNoCase(string sFull, string sSource, string sTarget)
		{
			int startDir = sFull.ToLower().IndexOf(sSource.ToLower());
			while (startDir >= 0)
			{
				string inizio = sFull.Substring(0, startDir);
				string fine = sFull.Substring(startDir + sSource.Length);

				sFull = inizio + sTarget + fine;
				startDir = sFull.ToLower().IndexOf(sSource.ToLower(), startDir + sSource.Length);
			}
				return sFull;
		}
	}
}
