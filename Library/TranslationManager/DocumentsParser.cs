using System;
using System.Collections;
using System.Xml;
using System.IO;
using System.Windows.Forms;

using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.Library.TranslationManager
{
	public class DocumentsParser : SolutionManagerItems
	{
		public DocumentsParser()
		{
			defaultLookUpType = LookUpFileType.Documents;
		}

		public override string ToString()
		{
			return "Documents namespace parser";
		}

		public override void Run(TranslationManager tManager)
		{
			TxtReader = new RichTextBox();

			transManager = tManager;

			OpenLookUpDocument(true);

			nMain = CreaNodoApplication(transManager.GetApplicationInfo().Name, false);
			
			foreach (BaseModuleInfo mi in transManager.GetApplicationInfo().Modules)
			{
				bool containsDocuments = false;
				XmlNode nModule = CreaNodoModule(mi.Name, false, false);

				if (mi.Documents == null)
					continue;

				SetProgressMessage(string.Format("Elaborazione in corso del modulo: {0}", mi.Name));

				foreach (DocumentInfo di in mi.Documents)
				{
					CreaNodoLookUp(nModule, di.NameSpace.ToString(), string.Empty, mi.Name);
					CreaNodoMig_Net(di.NameSpace.ToString());
					containsDocuments = true;
				}

				foreach (LibraryInfo li in mi.Libraries)
				{
					SetProgressMessage(string.Format("Leggo i sorgenti della library {0}", li.Name));

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
							int startString = TxtReader.Text.IndexOf("_NS_DOC(\"", start, end - start);
							if (startString < 0)
								break;
							startString += 9;
							int endString = TxtReader.Text.IndexOf("\"", startString, end - startString);
							string ns = TxtReader.Text.Substring(startString, endString - startString);
							start = endString;
							
							string[] fullNs = GetFullNamespace(ns, transManager.GetApplicationInfo().Name, mi.Name, li.Name);
							
							if (fullNs != null && !FindTransation(fullNs) && !FindXmlDescription(fullNs))
							{
								if (FindDescriptionFolder(fullNs))
								{
									AddDocumentToLookUp(fullNs);
								}
								else
								{
									string errorMessage = string.Format("Non trovo la cartella di descrizione per il documento {0}\n nel file {1}\\{2}\\{3}", ns, mi.Name, li.Name, fi.Name);
									SetLogError(errorMessage, ToString());
								}
							}
						}
					}
				}

				if (containsDocuments)
					nMain.AppendChild(nModule);
			}

			SaveDocMigration();

			EndRun(true);
		}

		private bool FindXmlDescription(string[] ns)
		{
			IBaseApplicationInfo ai = transManager.GetApplicationInfo().PathFinder.GetApplicationInfoByName(ns[0]);
			if (ai == null)
				return false;

			IBaseModuleInfo mi = ai.GetModuleInfoByName(ns[1]);
			if (mi == null)
				return false;

			string fullNs = "Document";

			foreach (string token in ns)
				fullNs += "." + token;

			IDocumentInfo di = mi.GetDocumentInfoByNameSpace(fullNs);
			if (di == null)
				return false;

			return true;
		}

		private bool FindDescriptionFolder(string[] ns)
		{
			IBaseApplicationInfo ai = transManager.GetApplicationInfo().PathFinder.GetApplicationInfoByName(ns[0]);
			if (ai == null)
				return false;

			IBaseModuleInfo mi = ai.GetModuleInfoByName(ns[1]);
			if (mi == null)
				return false;

			string path = Path.Combine(mi.GetModuleObjectPath(), ns[3]);

			if (Directory.Exists(path))
				return true;

			return false;
		}

		private void AddDocumentToLookUp(string[] ns)
		{
			string fullNs = string.Empty;

			foreach (string token in ns)
				if (fullNs == string.Empty)
					fullNs = token;
				else
					fullNs += "." + token;	

			XmlNode nModule = CreaNodoModule(ns[1], true, false);
			CreaNodoLookUp(nModule, fullNs, string.Empty, ns[1]);
		}
	}
	
}
