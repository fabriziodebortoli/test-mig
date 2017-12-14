using System;
using System.Collections;
using System.Xml;
using System.IO;
using System.Windows.Forms;
using Microarea.TaskBuilderNet.Core.NameSolver;

namespace Microarea.Library.TranslationManager
{
	public class ActivationParser : SolutionManagerItems
	{
		private ArrayList macros = new ArrayList();
		private string curModName = string.Empty;

		public ActivationParser()
		{
			defaultLookUpType = LookUpFileType.Activation;
		}

		public override string ToString()
		{
			return "Activations parser";
		}

		public override void Run(TranslationManager tManager)
		{
			TxtReader = new RichTextBox();

			transManager = tManager;

			OpenLookUpDocument(true);

			nMain = CreaNodoApplication(transManager.GetApplicationInfo().Name, false);
			
			foreach (BaseModuleInfo mi in transManager.GetApplicationInfo().Modules)
			{
				curModName = mi.Name;
				bool containsLibraries = false;
				XmlNode nModule = CreaNodoModule(mi.Name, false, false);
				
				foreach (LibraryInfo li in mi.Libraries)
				{
					bool containsActivations = false;
					Hashtable activationsFiles = new Hashtable();
					ArrayList activationsSort = new ArrayList();
					XmlNode nLibrary = CreaNodoLibrary(nModule, li.Name, false, null);

					string libDir = Path.Combine(mi.Path, li.FullPath);
					DirectoryInfo di = new DirectoryInfo(libDir);
					foreach (FileInfo fi in di.GetFiles("*.cpp"))
					{
						TxtReader.LoadFile(fi.FullName, RichTextBoxStreamType.PlainText);
						string macro = "_NS_ACT";
						
						int start = 0;
						int end = TxtReader.Text.Length;
						while (true)
						{
							int startString = TxtReader.Text.IndexOf(string.Format("{0}(\"", macro) , start, end - start);
							if (startString < 0)
								break;
							startString += macro.Length + 2;
							int endString = TxtReader.Text.IndexOf("\"", startString, end - startString);
							string ns = TxtReader.Text.Substring(startString, endString - startString);
							start = endString;
							if (!activationsFiles.ContainsKey(ns))
							{
								activationsFiles.Add(ns, fi.Name);
								activationsSort.Add(ns);
							}
						}
					}

					activationsSort.Sort();

					foreach (string activationName in activationsSort)
					{
						CreaNodoActivation(nLibrary, activationName, activationsFiles[activationName].ToString());
						containsActivations = true;
					}

					if (containsActivations)
					{
						nModule.AppendChild(nLibrary);
						containsLibraries = true;
					}

					SetProgressMessage(string.Format("Elaborazione in corso: library {0}", li.Name));
				}

				if (containsLibraries)
					nMain.AppendChild(nModule);
			}

			SaveDocMigration();
			EndRun(true);
		}

		private XmlNode CreaNodoActivation(XmlNode nLibrary, string activationName, string fileName)
		{
			XmlNode nRes = CreaNodoLookUp(nLibrary, activationName, string.Empty, curModName);
			CreaNodoMig_Net(activationName);
			if (nRes.Attributes["file"] == null)
			{
				XmlAttribute aFile = xLookUpDoc.CreateAttribute(string.Empty, "file", string.Empty);
				aFile.Value = fileName;
				nRes.Attributes.Append(aFile);
			}

			return nRes;
		}
	}
}
