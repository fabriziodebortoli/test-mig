using System;
using System.Collections;
using System.Xml;
using System.IO;
using System.Windows.Forms;
using Microarea.TaskBuilderNet.Core.NameSolver;

namespace Microarea.Library.TranslationManager
{
	public class CppMiscParser : SolutionManagerItems
	{
		private ArrayList macros = new ArrayList();
		private string curModName = string.Empty;

		public CppMiscParser()
		{
			defaultLookUpType = LookUpFileType.Misc;
			macros.Add("_NS_DF");
			macros.Add("_NS_FMT");
			macros.Add("_NS_EH");
			macros.Add("_NS_LFLD");
			macros.Add("_NS_VIEW");
			macros.Add("_NS_BE");
			macros.Add("_NS_TABMNG");
			macros.Add("_NS_TABDLG");
			macros.Add("_NS_CLN");
			macros.Add("_NS_LNK");
			macros.Add("_SET_FILE");
			macros.Add("_SET_SECTION");
			macros.Add("_SET_NAME");
		}

		public override string ToString()
		{
			return "CPP miscellanous namespace parser";
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

					bool containstokens = false;
					Hashtable tokensFiles = new Hashtable();
					ArrayList tokensSort = new ArrayList();
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
						foreach (string macro in macros)
						{
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

								CreaNodoMig_Net(ns, macro);
								
								if (!tokensFiles.ContainsKey(ns))
								{
									tokensFiles.Add(ns, fi.Name);
									tokensSort.Add(ns);
								}
							}
						}
					}

					tokensSort.Sort();

					foreach (string dbtName in tokensSort)
					{
						CreaNodoToken(nLibrary, dbtName, tokensFiles[dbtName].ToString());
						containstokens = true;
					}

					if (containstokens)
					{
						nModule.AppendChild(nLibrary);
						containsLibraries = true;
					}
				}

				if (containsLibraries)
					nMain.AppendChild(nModule);
			}

			SaveDocMigration("_NS_FMT");
			EndRun(true);
		}

		private XmlNode CreaNodoToken(XmlNode nLibrary, string dbtName, string fileName)
		{
			XmlNode nRes = CreaNodoLookUp(nLibrary, dbtName, string.Empty, curModName);
			if (nRes != null && nRes.Attributes["file"] == null)
			{
				XmlAttribute aFile = xLookUpDoc.CreateAttribute(string.Empty, "file", string.Empty);
				aFile.Value = fileName;
				nRes.Attributes.Append(aFile);
			}

			return nRes;
		}
	}
}
