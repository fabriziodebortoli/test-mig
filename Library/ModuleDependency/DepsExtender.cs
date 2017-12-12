using System;
using System.Collections.Generic;
using System.Xml;

using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.Library.ModuleDependency
{
	public class DepsExtender
	{
		private Dictionary<string, Aggregate> dic = new Dictionary<string, Aggregate>(StringComparer.InvariantCultureIgnoreCase);

		public DepsExtender(XmlElement aggregations)
		{
			if (aggregations == null)
				throw new ArgumentNullException();
			foreach (XmlElement aggregateElement in aggregations.GetElementsByTagName("AggregateName"))
			{
				Aggregate aggregate = new Aggregate(aggregateElement);
				dic[aggregate.DllName] = aggregate;
			}
		}

		public Aggregate GetAggregatedDefinition(string dllName)
		{
			Aggregate aggregate;
			if (!dic.TryGetValue(dllName, out aggregate))
				return null;
			return aggregate;
		}

		// TODO - optimize with a hashtable, now it just works
		public IDeployModule GetCompoundModule(IModule module)
		{
			foreach (Aggregate aggregate in dic.Values)
			{
				if (string.Compare(module.Application, aggregate.Application, StringComparison.InvariantCultureIgnoreCase) != 0)
					continue; // should not even be allowed, should I return null?
				foreach (string moduleName in aggregate.Modules)
					if (string.Compare(module.Name, moduleName, StringComparison.InvariantCultureIgnoreCase) == 0)
						return new ModuleDependency.ModuleDescriptor(aggregate.Module, aggregate.Application, module.Container, PolicyType.Full);
			}
			return null;
		}

		public Dictionary<string, IDeployModule> GetAggregatingModules(IModule module)
		{
			Dictionary<string, IDeployModule> mods = new Dictionary<string, IDeployModule>(StringComparer.InvariantCultureIgnoreCase);
			AddAggregatingModules(module, mods);
			return mods;
		}
		public void AddAggregatingModules(IModule module, Dictionary<string, IDeployModule> mods)
		{
			foreach (Aggregate aggregate in dic.Values)
			{
				if (string.Compare(module.Application, aggregate.Application, StringComparison.InvariantCultureIgnoreCase) != 0 ||
					string.Compare(module.Name, aggregate.Module, StringComparison.InvariantCultureIgnoreCase) != 0)
					continue;
				foreach (string moduleName in aggregate.Modules)
				{
					if (string.Compare(moduleName, aggregate.Module, StringComparison.InvariantCultureIgnoreCase) == 0)
						continue;
					string key = ModuleDependency.ModuleDescriptor.GetDescriptionString(moduleName, aggregate.Application, module.Container);
					IDeployModule aMod;
					mods.TryGetValue(key, out aMod);
					if (aMod == null)
						mods[key] = new ModuleDependency.ModuleDescriptor(moduleName, aggregate.Application, module.Container, PolicyType.Base);
				}
			}
		}
	}
}
