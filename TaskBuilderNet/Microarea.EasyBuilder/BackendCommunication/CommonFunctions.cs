using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Xml;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Licence.Licence;
using Microarea.TaskBuilderNet.Licence.Licence.ConfigurationInfoProvider;

namespace Microarea.EasyBuilder.BackendCommunication
{
	//=========================================================================
	internal class CommonFunctions
	{
		//---------------------------------------------------------------------
		private CommonFunctions()
		{
		}

		//---------------------------------------------------------------------
		public static ProxySettings GetProxySettings()
		{
			return ProxySettings.GetServerProxySetting(
				BasePathFinder.BasePathFinderInstance.GetProxiesFilePath()
				);
		}

		//---------------------------------------------------------------------
		public static ActivationObject GetActivationObject()
		{
			IConfigurationInfoProvider provider = new FSProviderForInstalled(BasePathFinder.BasePathFinderInstance);
			ActivationObject ao;
			try
			{
				ao = new ActivationObject(provider);
			}
			catch (Exception exc)
			{
				Debug.WriteLine(exc.Message);
				return null;
			}
			return ao;
		}

		//---------------------------------------------------------------------
		public static string[] GetSalesModulesPaths(string solutionName)
		{
			List<string> fileNames = new List<string>();
			
			string solution = BasePathFinder.BasePathFinderInstance.GetSolutionFile(solutionName);
			
			XmlDocument doc = new XmlDocument();
			doc.Load(solution);
			
			XmlNodeList list = doc.SelectNodes("//SalesModule/@name");
			if (list == null || list.Count == 0)
				return null;

			string modulesPath = BasePathFinder.BasePathFinderInstance.GetSolutionsModulesPath(solutionName);
			foreach (XmlNode n in list)
			{
				if (n == null || n.Value == null || n.Value.Length == 0)
					continue;
				
				string filename = Path.Combine(modulesPath, String.Format("{0}.xml", n.Value));
				if (!File.Exists(filename))
					continue;
				
				fileNames.Add(filename);
			}
			return fileNames.ToArray();
		}
    }
}
