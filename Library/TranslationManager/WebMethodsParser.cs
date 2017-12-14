using System;
using System.Collections;
using System.Xml;
using System.IO;
using Microarea.TaskBuilderNet.Core.NameSolver;

namespace Microarea.Library.TranslationManager
{
	public class WebMethodsParser : SolutionManagerItems
	{
		public WebMethodsParser()
		{
			defaultLookUpType = LookUpFileType.WebMethods;
		}

		public override string ToString()
		{
			return "WebMethods namespace parser";
		}

		public override void Run(TranslationManager tManager)
		{
			transManager = tManager;

			OpenLookUpDocument(true);

			nMain = CreaNodoApplication(transManager.GetApplicationInfo().Name, false);
			
			foreach (BaseModuleInfo mi in transManager.GetApplicationInfo().Modules)
			{
				SetProgressMessage(string.Format("Elaborazione in corso: modulo {0}", mi.Name));

				if (!File.Exists(mi.GetWebMethodsPath()))
					continue;

				XmlNode nModule = CreaNodoModule(mi.Name, true, false);

				XmlDocument xWebMethods = new XmlDocument();
				xWebMethods.Load(mi.GetWebMethodsPath());

				foreach (XmlNode n in xWebMethods.SelectNodes("FunctionObjects/Functions/Function/@namespace"))
				{
					CreaNodoLookUp(nModule, n.InnerText, string.Empty, mi.Name);
					CreaNodoMig_Net(n.InnerText);
				}
			}

			SaveDocMigration();
			EndRun(true);
		}
	}
}
