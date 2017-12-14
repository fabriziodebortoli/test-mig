using System;
using System.Collections;
using System.Xml;
using System.IO;
using System.Windows.Forms;
using Microarea.TaskBuilderNet.Core.NameSolver;

namespace Microarea.Library.TranslationManager
{
	public class ReferenceObjectsParser : SolutionManagerItems
	{
		public ReferenceObjectsParser()
		{
			defaultLookUpType = LookUpFileType.ReferenceObjects;
		}

		public override string ToString()
		{
			return "ReferenceObjects namespace parser";
		}

		public override void Run(TranslationManager tManager)
		{
			TxtReader = new RichTextBox();

			transManager = tManager;

			OpenLookUpDocument(true);

			nMain = CreaNodoApplication(transManager.GetApplicationInfo().Name, false);
			
			foreach (BaseModuleInfo mi in transManager.GetApplicationInfo().Modules)
			{
				SetProgressMessage(string.Format("Elaborazione in corso: modulo {0}", mi.Name));
				bool containsReferenceObjects = false;
				XmlNode nModule = CreaNodoModule(mi.Name, false, false);

				string referenceObjectsFolder = Path.Combine(mi.Path, "ReferenceObjects");

				if (!Directory.Exists(referenceObjectsFolder))
					continue;

				DirectoryInfo di = new DirectoryInfo(referenceObjectsFolder);

				foreach (FileInfo fi in di.GetFiles("*.xml"))
				{
					XmlDocument xRef = new XmlDocument();
					xRef.Load(fi.FullName);

					foreach (XmlNode nFunction in xRef.SelectNodes("HotKeyLink/Function"))
					{
						CreaNodoLookUp(nModule, nFunction.Attributes["namespace"].Value.ToString(), string.Empty, mi.Name);
						CreaNodoMig_Net(nFunction.Attributes["namespace"].Value.ToString());
						containsReferenceObjects = true;
					}
				}

				if (containsReferenceObjects)
					nMain.AppendChild(nModule);
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
							int startString = TxtReader.Text.IndexOf("_NS_HKL(\"", start, end - start);
							if (startString < 0)
								break;
							startString += 9;
							int endString = TxtReader.Text.IndexOf("\"", startString, end - startString);
							string ns = TxtReader.Text.Substring(startString, endString - startString);
							start = endString;
							
							string[] fullNs = GetFullNamespace(ns, transManager.GetApplicationInfo().Name, mi.Name, li.Name);
							
							if (fullNs != null && !FindTransation(fullNs))
							{
								string errorMessage = string.Format("Non trovo il ReferenceObject {0}\n referenziato nel file {1}\\{2}\\{3}", ns, mi.Name, li.Name, fi.Name);
								SetLogError(errorMessage, ToString());
							}
						}
					}
				}
			}

			SaveDocMigration();
			EndRun(true);
		}
	}
}
