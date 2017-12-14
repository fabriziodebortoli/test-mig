using System;
using System.Collections;
using System.Xml;
using System.IO;
using System.Windows.Forms;
using Microarea.TaskBuilderNet.Core.NameSolver;

namespace Microarea.Library.TranslationManager
{
	public class DbtsParser : SolutionManagerItems
	{
		private string curModName = string.Empty;

		public DbtsParser()
		{
			defaultLookUpType = LookUpFileType.Dbts;
		}

		public override string ToString()
		{
			return "Dbts namespace parser";
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
					SetProgressMessage(string.Format("Elaborazione in corso: library {0}", li.Name));

					bool containsDbts = false;
					Hashtable dbtsFiles = new Hashtable();
					ArrayList dbtsSort = new ArrayList();
					XmlNode nLibrary = CreaNodoLibrary(nModule, li.Name, false, null);

					string libDir = Path.Combine(mi.Path, li.FullPath);
					DirectoryInfo di = new DirectoryInfo(libDir);

					ArrayList fiList = new ArrayList();

					foreach (FileInfo fi in di.GetFiles("*.cpp"))
						fiList.Add(fi);
					
					foreach (FileInfo fi in di.GetFiles("*.h"))
						fiList.Add(fi);
					
					foreach (FileInfo fi in fiList)
					{
						TxtReader.LoadFile(fi.FullName, RichTextBoxStreamType.PlainText);
						int start = 0;
						int end = TxtReader.Text.Length;
						while (true)
						{
							int startString = TxtReader.Text.IndexOf("_NS_DBT(\"", start, end - start);
							if (startString < 0)
								break;
							startString += 9;
							int endString = TxtReader.Text.IndexOf("\"", startString, end - startString);
							string ns = TxtReader.Text.Substring(startString, endString - startString);
							start = endString;
							if (!dbtsFiles.ContainsKey(ns))
							{
								dbtsFiles.Add(ns, fi.Name);
								dbtsSort.Add(ns);
							}
						}
					}

					dbtsSort.Sort();

					foreach (string dbtName in dbtsSort)
					{
						CreaNodoDbt(nLibrary, dbtName, dbtsFiles[dbtName].ToString());
						containsDbts = true;
					}

					if (containsDbts)
					{
						nModule.AppendChild(nLibrary);
						containsLibraries = true;
					}
				}

				if (containsLibraries)
					nMain.AppendChild(nModule);
			}

			EndRun(true);
		}

		private XmlNode CreaNodoDbt(XmlNode nLibrary, string dbtName, string fileName)
		{
			XmlNode nRes = CreaNodoLookUp(nLibrary, dbtName, string.Empty, curModName);
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
