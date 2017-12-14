using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Xml;

using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.Library.ModuleDependency
{
	/// <summary>
	/// classe che raccoglie le informazioni del singolo modulo
	/// </summary>
	//============================================================================
	public class Module : IComparable, IModule, IDeployModule
	{
		private readonly string		moduleComposedName;
		private readonly ArrayList	dllsList;
		private readonly string		applicationName;
		private readonly string		moduleDirName;
		private readonly string		container;

		public Module(string moduleComposedName, string container)
		{
			this.moduleComposedName = moduleComposedName;
			this.dllsList = new ArrayList();

			string[] pieces = moduleComposedName.Split('.');
			applicationName = pieces[0];
			moduleDirName = pieces[1];
			this.container = container;
		}

		public Dll[]	DllsList   { get { return (Dll[])this.dllsList.ToArray(typeof(Dll)); } }

		public void AddDll(Dll dll)
		{
			this.dllsList.Add(dll);
		}

		public string	ApplicationName { get { return this.applicationName; } }
		public string	ModuleDirName { get { return this.moduleDirName; } }

		#region overridden object methods

		//---------------------------------------------------------------------
		public override bool Equals(object obj)
		{
			return this.CompareTo(obj) == 0;
		}
		
		//---------------------------------------------------------------------
		public override int GetHashCode()
		{
			return this.moduleComposedName.ToLower(CultureInfo.InvariantCulture).GetHashCode();
		}
		
		//---------------------------------------------------------------------
		public override string ToString()
		{
			return this.moduleComposedName; // I don't love it
		}

		#endregion

		#region IComparable Members

		public int CompareTo(object obj)
		{
			Module mod = obj as Module;
			if (mod == null)
				return 1; // This instance is greater than obj
			return string.Compare(this.moduleComposedName, mod.moduleComposedName, true, CultureInfo.InvariantCulture);
		}

		#endregion

		#region IModule Members

		public string Name
		{
			get { return this.moduleDirName; }
		}

		public string Application
		{
			get { return this.applicationName; }
		}

		public string Container
		{
			get { return this.container; }
		}

		#endregion

		#region IDeployModule Members

		public PolicyType DeploymentPolicy
		{
			get
			{
				// we need just a single dll with full deployment policy to make the module dependency full
				bool hasAFull = false;
				foreach (Dll dll in this.dllsList)
					if (dll.Policy == Dll.DllDeploymentPolicy.Full)
					{
						hasAFull = true;
						break;
					}
				return hasAFull ? PolicyType.Full : PolicyType.Base;
			}
		}

		#endregion
	}

	/// <summary>
	/// Classe che, dato in input un array di moduli funzionali, restituisce una
	/// struttura con l'elenco di tutte le dll da cui essi dipendono, cercando le
	/// informazioni nel file DependenciesMap.xml.
	/// </summary>
	//============================================================================
	public class ModuleDependency
	{
		private readonly Hashtable dependencyModules;
		public IDeployModule[] DependencyModules
		{
			get
			{
				ArrayList list = new ArrayList(dependencyModules.Count);
				list.AddRange(dependencyModules.Values);
				list.Sort();
				return (IDeployModule[])list.ToArray(typeof(IDeployModule));
			}
		}

		private DependenciesMapInfo dependenciesMapInfo = null;
		private ArrayList depsDllList = new ArrayList(); // array di supporto

		private ArrayList discardedLibraries = new ArrayList();
		public Dll[] DiscardedLibraries { get { return (Dll[])this.discardedLibraries.ToArray(typeof(Dll)); } }

		private bool discardAddOns;
		public bool DiscardAddOns { get { return this.discardAddOns; } set { this.discardAddOns = value; } }

		private readonly XmlDocument dependenciesMapDom;
		private readonly DepsExtender depsExtender;

		//---------------------------------------------------------------------
		public ModuleDependency(XmlDocument dependenciesMapDom)
		{
			this.dependencyModules = new Hashtable(StringComparer.InvariantCultureIgnoreCase);
			this.dependenciesMapDom = dependenciesMapDom;

			string aggregationsXPath = "Aggregations";
			Debug.Assert(dependenciesMapDom != null && dependenciesMapDom.DocumentElement != null);
			XmlElement aggregationsElement = dependenciesMapDom.DocumentElement.SelectSingleNode(aggregationsXPath) as XmlElement;
			if (aggregationsElement != null)
				this.depsExtender = new DepsExtender(aggregationsElement);
		}

		/// <summary>
		/// Cerca le informazioni di ogni modulo funzionale.
		/// </summary>
		/// <param name="moduleNamesList">array di moduli funzionali in input</param>
		//---------------------------------------------------------------------
		public void CreateDependencyModulesList(IModule[] moduleList, string cppAppsContainer)
		{
			ArrayList moduleNamesList = new ArrayList(moduleList.Length);
			foreach (IModule mod in moduleList)
			{
				string moduleName = mod.Application + "." + mod.Name;
				if (!moduleNamesList.Contains(moduleName))
				{
					moduleNamesList.Add(moduleName);
					Debug.WriteLine(moduleName);
				}
			}
			
			//cleans up the results of previous calculations
			this.discardedLibraries.Clear();
			this.dependencyModules.Clear();
			
			// Effettua il parsing del file di configurazione per avere
			// a disposizione una struttura in memoria con le informazioni.
			this.dependenciesMapInfo = new DependenciesMapInfo(this.dependenciesMapDom);
			this.dependenciesMapInfo.Parse();
			// NOTE: I could do it in the constructor just once, but at the moment I do it
			//       now because I'd have to clean all the .Visited values in dependenciesMapInfo

			foreach (string modName in moduleNamesList)
				WriteInfo(modName);

			depsDllList.Sort();

			// beginning of sections for tests only
			if (this.discardAddOns)
			{
				foreach (Dll lib in depsDllList)
					if (discardedLibraries.Contains(lib))
						discardedLibraries.Remove(lib);
				discardedLibraries.Sort();
			}
			// end of section dedicated for tests

			Module module;
			foreach (Dll stDll in depsDllList)
			{
				string modNamespace = stDll.ModuleNamespace;
				module = dependencyModules[modNamespace] as Module;

				if (module == null)
					module = new Module(modNamespace, cppAppsContainer);

				module.AddDll(stDll);

				if (!dependencyModules.Contains(module.ToString()))
					dependencyModules.Add(module.ToString(), module);
			}

			// removes modules which were in the input array
			foreach (string modName in moduleNamesList)
				dependencyModules.Remove(modName);
		}

		//---------------------------------------------------------------------
		private void WriteInfo(string module)
		{
			if (this.dependenciesMapInfo.Libraries == null)
				return;
			foreach (LibrariesInfo info in this.dependenciesMapInfo.Libraries)
			{
				if (string.Compare(info.ModuleNamespace, module, true, CultureInfo.InvariantCulture) != 0)
					continue;

				if (info.Visited == true)
					continue;

				info.Visited = true;

				if (!depsDllList.Contains(info))
					depsDllList.Add(info);

				if (info.DependenciesInfos == null || info.DependenciesInfos.Length == 0)
					continue;

				if (this.discardAddOns &&
					info.Policy == Dll.DllDeploymentPolicy.AddOn)
				{
					foreach (DependenciesInfo depInfo in info.DependenciesInfos)
						if (!this.discardedLibraries.Contains(depInfo))
							this.discardedLibraries.Add(depInfo);
				}
				else
				{
					foreach (DependenciesInfo depInfo in info.DependenciesInfos)
						if (!this.depsDllList.Contains(depInfo))
							this.depsDllList.Add(depInfo);
				}
			}
		}

		public IDeployModule[] GetAggregatingDependencies(List<IModule> depModules, string cppAppsContainer)
		{
			Dictionary<string, IDeployModule> dic = new Dictionary<string, IDeployModule>(StringComparer.InvariantCultureIgnoreCase);

			if (this.depsExtender != null)
			{
				foreach (IModule depModule in depModules)
					depsExtender.AddAggregatingModules(depModule, dic);

				// old implementation

			    /*
				Debug.WriteLine("Extending dependency list with aggregate modules");
				foreach (IDeployModule depMod in depModules)
				{
					Module moduleObj = depMod as Module;
					if (moduleObj == null)
					{
						Debug.Fail("collection item should be of type ModuleDependency.Module");
						continue;
					}
					foreach (Dll dll in moduleObj.DllsList)
					{
						Aggregate aggregate = depsExtender.GetAggregatedDefinition(dll.Name);
						if (aggregate == null)
							continue;
						string aApp = aggregate.Application;
						string[] modules = aggregate.Modules;
						Debug.Assert(modules != null && modules.Length != 0);
						//Debug.WriteLine(aggregate.DllName + " aggregates:");
						foreach (string modName in modules)
						{
							IDeployModule aMod;
							string key = ModuleDescriptor.GetDescriptionString(modName, aApp, cppAppsContainer);
							dic.TryGetValue(key, out aMod);
							PolicyType policy = string.Compare(modName, depMod.Name, StringComparison.InvariantCultureIgnoreCase) == 0
								? PolicyType.Full : PolicyType.Base;
							if (aMod == null)
								dic[key] = new ModuleDescriptor(modName, aApp, cppAppsContainer, policy);
						}
					}
				}
				*/
			}
			List<IDeployModule> mods = new List<IDeployModule>(dic.Count);
			mods.AddRange(dic.Values);
			return mods.ToArray();
		}

		public IDeployModule GetCompoundModule(IModule module)
		{
			if (this.depsExtender == null)
				return null;
			return this.depsExtender.GetCompoundModule(module);
		}

		public class ModuleDescriptor : IDeployModule
		{
			readonly string name;
			readonly string application;
			readonly string container;
			readonly PolicyType policy;

			public ModuleDescriptor(string name, string application, string container)
				: this(name, application, container, PolicyType.Unknown) { }
			public ModuleDescriptor(string name, string application, string container, PolicyType policy)
			{
				this.name = name;
				this.application = application;
				this.container = container;
				this.policy = policy;
			}

			#region IModule Members

			public string Name			{ get { return this.name; } }
			public string Application	{ get { return this.application; } }
			public string Container		{ get { return this.container; } }

			#endregion

			#region IDeployModule Members

			public PolicyType DeploymentPolicy { get { return this.policy; } }

			#endregion

			#region overridden object methods
			//---------------------------------------------------------------------
			public override bool Equals(object obj)
			{
				if (Object.ReferenceEquals(obj, null))
					return false;
				IModule comp = obj as IModule;
				if (Object.ReferenceEquals(comp, null))
					return false;

				if (string.Compare(name, comp.Name, StringComparison.InvariantCultureIgnoreCase) == 0 &&
					string.Compare(application, comp.Application, StringComparison.InvariantCultureIgnoreCase) == 0 &&
					string.Compare(container, comp.Container, StringComparison.InvariantCultureIgnoreCase) == 0)
					return true;

				return false;
			}

			//---------------------------------------------------------------------
			public override int GetHashCode()
			{
				return this.ToString().ToLower(CultureInfo.InvariantCulture).GetHashCode();
			}

			public override string ToString()
			{
				return GetDescriptionString(name, application, container);
			}
			#endregion

			public static string GetDescriptionString(string name, string application, string container)
			{
				return string.Concat(container, '/', application, '/', name);
			}
		}

		//---------------------------------------------------------------------
	}
}