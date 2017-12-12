using Microarea.TaskBuilderNet.Core.Generic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;
using System.Reflection;

namespace Microarea.TaskBuilderNet.Core.EasyBuilder
{
	class DescriptionProvider
	{
		static Dictionary<Assembly, AssemblyDescriptions> descriptions = new Dictionary<Assembly, AssemblyDescriptions>();
		//--------------------------------------------------------------------------------
		internal static string GetPropertyDescription(Type componentType, string propertyName)
		{
			AssemblyDescriptions desc = GetAssemblyDescriptions(componentType.Assembly);
			string entry = string.Concat("P:", componentType.FullName, ".", propertyName);
			string s = string.Empty;
			desc.TryGetValue(entry, out s);
			return s;
		}

		//--------------------------------------------------------------------------------
		private static AssemblyDescriptions GetAssemblyDescriptions(Assembly assembly)
		{
			lock (typeof(DescriptionProvider))
			{
				AssemblyDescriptions desc;
				if (descriptions.TryGetValue(assembly, out desc))
					return desc;
				desc = new AssemblyDescriptions(assembly);
				descriptions[assembly] = desc;
				return desc;
			}
		}
	}

	//--------------------------------------------------------------------------------
	class AssemblyDescriptions : Dictionary<string, string>
	{
		public AssemblyDescriptions(Assembly assembly)
		{
			string file = Path.Combine(Functions.GetAssemblyPath(assembly), assembly.GetName().Name + ".xml");
			if (!File.Exists(file))
				return;
			XmlDocument doc = new XmlDocument();
			try
			{
				doc.Load(file);
			}
			catch
			{
			}
			foreach (XmlElement el in doc.SelectNodes("doc/members/member"))
			{
				XmlNodeList nodes = el.GetElementsByTagName("summary");
				if (nodes.Count == 1)
				{
					string name = el.GetAttribute("name");
					if (!string.IsNullOrEmpty(name))
						this[name] = nodes[0].InnerText;
				}
			}
			
		} 
	}
}
