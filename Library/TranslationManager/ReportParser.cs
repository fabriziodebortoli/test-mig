using System;
using System.Collections;
using System.Xml;
using System.IO;
using System.Windows.Forms;
using Microarea.TaskBuilderNet.Core.NameSolver;

namespace Microarea.Library.TranslationManager
{
	public class ReportParser : SolutionManagerItems
	{
		public ReportParser()
		{
			defaultLookUpType = LookUpFileType.Report;
		}

		public override string ToString()
		{
			return "Report parser";
		}

		public override void Run(TranslationManager tManager)
		{
			TxtReader = new RichTextBox();

			transManager = tManager;

			OpenLookUpDocument(true);

			nMain = CreaNodoApplication(transManager.GetApplicationInfo().Name, false);
			
			foreach (BaseModuleInfo mi in transManager.GetApplicationInfo().Modules)
			{
				DirectoryInfo di = new DirectoryInfo(Path.Combine(mi.Path, "Report"));

				if (di == null || !di.Exists)
					continue;

				SetProgressMessage(string.Format("Elaborazione in corso: library {0}", mi.Name));

				XmlNode nModule = CreaNodoModule(mi.Name, true, false);

				foreach (FileInfo fi in di.GetFiles("*.wrm"))
				{
					string fileName = fi.Name.Split('.')[0];
					CreaNodoLookUp(nModule, fileName, string.Empty, mi.Name);
					CreaNodoMig_Net(transManager.GetApplicationInfo().Name + "." + mi.Name + "." + fileName);
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
							int startString = TxtReader.Text.IndexOf("_NS_WRM(\"", start, end - start);
							if (startString < 0)
								break;
							startString += 9;
							int endString = TxtReader.Text.IndexOf("\"", startString, end - startString);
							string ns = TxtReader.Text.Substring(startString, endString - startString);
							start = endString;
							
							string[] fullNs = GetFullNamespace(ns, transManager.GetApplicationInfo().Name, mi.Name);
							
							if (fullNs != null && !FindTransation(fullNs))
							{
								string errorMessage = string.Format("Non trovo il report {0}\n referenziato nel file {1}\\{2}\\{3}", ns, mi.Name, li.Name, fi.Name);
								SetLogError(errorMessage, ToString());
							}
						}
					}
				}
			}

			SaveDocMigration();
			EndRun(true);
		}

		protected string[] GetFullNamespace(string ns, string appName, string modName)
		{
			if (ns.Trim() == string.Empty)
				return null;

			string[] res = new string[] {appName, modName, string.Empty};

			string[] tokens = ns.Split('.');

			if (tokens.Length == 1)
				res[2] = ns;
			else
			{
				int idy = 2;
				for (int idx = tokens.Length - 1; idx >= 0 && idy >= 0; idx --)
				{
					res[idy--] = tokens[idx];
				}
			}

			return res;
		}

		protected override bool FindTransation(string[] ns)
		{
			XmlNode nApplication = xLookUpDoc.SelectSingleNode("Application");
			if (nApplication.Attributes["source"].Value.ToString().ToLower() != ns[0].ToLower())
			{
				return false;
			}

			foreach (XmlNode nMod in nApplication.SelectNodes("Module"))
			{
				if (nMod.Attributes["source"].Value.ToString().ToLower() == ns[1].ToLower())
				{
					foreach (XmlNode n in nMod.ChildNodes)
					{
						if (n.Attributes["source"].Value.ToString().ToLower() == ns[2].ToLower())
						{
							return true;
						}
					}
				}
			}
			return false;
		}
	}
}
