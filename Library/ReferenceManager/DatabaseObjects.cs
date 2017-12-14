using System;
//using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Xml;

namespace Microarea.Library.ReferenceManager
{
	public class DatabaseItem : IDependentItemBuilder
	{
		private readonly string		applicationName;
		private readonly string		moduleName;
		private readonly string[] references;
		private readonly string lookUpName;

		public bool Missing { get { return false; } }

		#region Constructors
		//---------------------------------------------------------------------
		public DatabaseItem(string applicationName, string moduleName, XmlElement xmlElement)
		{
			if (applicationName == null || applicationName.Length == 0 || moduleName == null || moduleName.Length == 0)
				throw new ArgumentNullException();

			this.applicationName = applicationName;
			this.moduleName = moduleName;
			this.references = BuildReferences(xmlElement, this.applicationName);
			this.lookUpName = string.Concat(applicationName, ".", moduleName);
		}
		#endregion

		//---------------------------------------------------------------------
		public string GetLookUpName()
		{
			return this.lookUpName;
		}
		//---------------------------------------------------------------------
		public string[] GetReferences()
		{
			return this.references;
		}

		//---------------------------------------------------------------------
		public static string[] BuildReferences(XmlElement xmlElement, string applicationName)
		{
			// Esempio di file DatabaseScript\Create\CreateInfo.xml:
			//<CreateInfo>
			//	<ModuleInfo name="Manufacturing" />
			//	<Level1>
			//		<Step ... >
			//			<Dependency app="ERP" module="SaleOrders" />
			//		</Step>
			//		<Step ... />
			//	</Level1>
			//	<Level2>
			//	</Level2>
			//</CreateInfo>
			//
			// In teoria bisognerebbe analizzare anche gli script di upgrade
			// Upgrade\UpgradeInfo.xml.
			// In prima battuta ignoriamo il caso ed assumiamo che eventuali
			// dipendenze listate in UpgradeInfo.xml siano già presenti nel
			// corrispondente CreateInfo.xml.

			List<string> list = new List<string>();

			if (xmlElement != null)
			{
				// TODO: Stringhe cablate! (BuildReferences)
				XmlNodeList dependencies = xmlElement.GetElementsByTagName("Dependency");
				foreach (XmlElement dependency in dependencies)
				{
					string app = dependency.GetAttribute("app");
					if (string.Compare(app, applicationName, StringComparison.InvariantCultureIgnoreCase) == 0)
					{
						string module = dependency.GetAttribute("module");
						string key = string.Concat(app, ".", module);
						if (!list.Contains(key))
							list.Add(key);
					}
				}

				list.Sort();
			}

			return (string[])list.ToArray();
		}

		//---------------------------------------------------------------------
		public string GetShortName()
		{
			return moduleName;
		}
	}
}
