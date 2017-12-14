using System;
using System.Collections;
using System.Xml;
using System.IO;
using System.Windows.Forms;
using Microarea.TaskBuilderNet.Core.NameSolver;

namespace Microarea.Library.TranslationManager
{
	public class ClientDocumentsParser : SolutionManagerItems
	{
		public ClientDocumentsParser()
		{
			defaultLookUpType = LookUpFileType.ClientDocuments;
		}

		public override string ToString()
		{
			return "ClientDocuments namespace parser";
		}

		public override void Run(TranslationManager tManager)
		{
			TxtReader = new RichTextBox();

			transManager = tManager;
			
			OpenLookUpDocument(true);

			nMain = CreaNodoApplication(transManager.GetApplicationInfo().Name, false);
			
			foreach (BaseModuleInfo mi in transManager.GetApplicationInfo().Modules)
			{
				if (mi.ClientDocumentsObjectInfo == null)
					continue;

				SetProgressMessage(string.Format("Ricerca ClientDocuments del modulo: {0}", mi.Name));

				foreach (ServerDocumentInfo sdi in mi.ClientDocumentsObjectInfo.ServerDocuments)
				{
					foreach (ClientDocumentInfo cdi in sdi.ClientDocsInfos)
					{
						string ns = cdi.NameSpace.ToString();

						XmlNode nModule = CreaNodoModule(ns.Split('.')[2], true, false);

						CreaNodoLookUp(nModule, ns, string.Empty, mi.Name);
					}
				}
			}

			foreach (BaseModuleInfo mi in transManager.GetApplicationInfo().Modules)
			{
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
							int startString = TxtReader.Text.IndexOf("_NS_CD_BSP(\"", start, end - start);
							if (startString < 0)
								break;
							startString += 12;
							int endString = TxtReader.Text.IndexOf("\"", startString, end - startString);
							string ns = TxtReader.Text.Substring(startString, endString - startString);
							start = endString;
							
							string[] fullNs = GetFullNamespace(ns, transManager.GetApplicationInfo().Name, mi.Name, li.Name);
							
							AddCDToLookUp(fullNs);
						}

						start = 0;
						end = TxtReader.Text.Length;
						while (true)
						{
							int startString = TxtReader.Text.IndexOf("_NS_CD(\"", start, end - start);
							if (startString < 0)
								break;
							startString += 8;
							int endString = TxtReader.Text.IndexOf("\"", startString, end - startString);
							string ns = TxtReader.Text.Substring(startString, endString - startString);
							start = endString;
							
							string[] fullNs = GetFullNamespace(ns, transManager.GetApplicationInfo().Name, mi.Name, li.Name);
							
							if (fullNs != null && !FindTransation(fullNs))
							{
								string errorMessage = string.Format("Non trovo il ClientDocument {0}\n referenziato nel file {1}\\{2}\\{3}", ns, mi.Name, li.Name, fi.Name);
								SetLogError(errorMessage, ToString());
							}
						}
					}
				}
			}

			EndRun(true);
		}

		private void AddCDToLookUp(string[] ns)
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
