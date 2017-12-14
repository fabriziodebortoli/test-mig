using System;
using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Xml;

using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Interfaces;

/// esempio di struttura di un file DependenciesMap.xml
///<Library module="MagoNet.Divise" name="DiviseDbl" policy="full" />
///
///<Library module="MagoNet.Partite" name="PartiteDbl" policy="full">
///	<Dependency module="MagoNet.IdsMng" name="IdsMngDbl" policy="full" />
///	<Dependency module="MagoNet.Core" name="CoreDbl" policy="base" />
///</Library>

namespace Microarea.Library.ModuleDependency
{
	//============================================================================
	public class LibrariesInfo : Dll
	{
		public bool			Visited = false;
		private ArrayList	dependenciesInfos;

		public LibrariesInfo(string module, string name, DllDeploymentPolicy policy)
			: base(module, name, policy)
		{
			this.dependenciesInfos = new ArrayList();
		}

		public DependenciesInfo[] DependenciesInfos
		{
			get { return (DependenciesInfo[])this.dependenciesInfos.ToArray(typeof(DependenciesInfo)); }
		}

		public void AddDependency(DependenciesInfo dependenciesInfo)
		{
			this.dependenciesInfos.Add(dependenciesInfo);
		}
	}

	//============================================================================
	public class DependenciesInfo : Dll
	{
		public DependenciesInfo(string module, string name, DllDeploymentPolicy policy)
			: base(module, name, policy) {}
	}

	//============================================================================
	public class Dll : IComparable
	{
		private string moduleNamespace;
		private string name;
		private DllDeploymentPolicy policy;

		public Dll(string moduleNamespace, string name, DllDeploymentPolicy policy)
		{
			this.moduleNamespace = moduleNamespace;
			this.name   = name;
			this.policy = policy;
		}

		public string ModuleNamespace { get { return this.moduleNamespace; } }
		public string Name   { get { return this.name; } }
		public DllDeploymentPolicy Policy { get { return this.policy; } }

		#region overridden object methods

		//---------------------------------------------------------------------
		public override string ToString()
		{
			return this.moduleNamespace + "-" + this.Name + "-" + this.Policy;
		}

		//---------------------------------------------------------------------
		public override bool Equals(object obj)
		{
			return this.CompareTo(obj) == 0;
		}
		
		//---------------------------------------------------------------------
		public override int GetHashCode()
		{
			return this.ToString().ToLower(CultureInfo.InvariantCulture).GetHashCode();
		}
		#endregion

		#region IComparable Members

		//---------------------------------------------------------------------
		public int CompareTo(object obj)
		{
			Dll bdi = obj as Dll;
			if (bdi == null)
				return 1; // This instance is greater than obj
			return string.Compare(this.ToString(), bdi.ToString(), true, CultureInfo.InvariantCulture);
		}

		#endregion

		public enum DllDeploymentPolicy { Full, Base, AddOn }
	}

	/// <summary>
	/// classe che si occupa di parsare il file DependenciesMap.xml e di inserire le info
	/// in una struttura in memoria, utilizzabile da altri componenti.
	/// </summary>
	//============================================================================
	public class DependenciesMapInfo
	{
		private string		dependenciesMapFile;
		private ArrayList	libraries;

		public string		DependenciesMapFile	{ get {return dependenciesMapFile;} }
		public ArrayList	Libraries			{ get {return libraries;} }

		private	XmlDocument	xDoc = null;

		//---------------------------------------------------------------------
		public DependenciesMapInfo(string dependenciesMapFileFullName)
		{
			dependenciesMapFile = dependenciesMapFileFullName;
		}
		public DependenciesMapInfo(XmlDocument dependenciesMapDom)
		{
			xDoc = dependenciesMapDom;
		}

		/// <summary>
		/// Effettua il parse del file DependenciesMap.xml.
		/// </summary>
		//---------------------------------------------------------------------
		public void Parse()
		{
			if (xDoc == null)
			{
				FileInfo fileInfo = new FileInfo(dependenciesMapFile);
				if (!fileInfo.Exists)
				{
					Debug.Fail
					(
						"Errore in DependenciesMapInfo.Parse: il file di dipendenze " +
						dependenciesMapFile +
						" non esiste."
					);

					return;
				}

				xDoc = new XmlDocument();

				try
				{
					xDoc.Load(dependenciesMapFile);
				}
				catch(XmlException e)
				{
					Debug.Fail("Errore in DependenciesMapInfo.Parse: " + e.Message);
					string error =
						string.Format(CultureInfo.InvariantCulture, Strings.ErrorReadingFile, dependenciesMapFile);
					return;
				}
			}

			XmlElement root = xDoc.DocumentElement;
			if (root == null)
			{
				Debug.Fail("Errore in DependenciesMapInfo.Parse: Sintassi del file errata");
				return;
			}

			XmlNodeList libList = root.GetElementsByTagName(DependenciesMapXML.Element.Library);

			if (libList != null && libList.Count > 0)
			{
				this.libraries = new ArrayList();

				foreach (XmlElement lib in libList)
				{
					string libPolicyString = lib.GetAttribute(DependenciesMapXML.Attribute.Policy);
					Debug.Assert(libPolicyString.Length != 0);
					LibrariesInfo librariesInfo = new LibrariesInfo
						(
						lib.GetAttribute(DependenciesMapXML.Attribute.Module),
						lib.GetAttribute(DependenciesMapXML.Attribute.Name),
						(Dll.DllDeploymentPolicy)Enum.Parse(typeof(Dll.DllDeploymentPolicy), libPolicyString, true)
						);

					XmlNodeList depList = lib.GetElementsByTagName(DependenciesMapXML.Element.Dependency);

					// anche se non ci sono nodi di dipendenza, devo cmq inserire nell'array il nodo Library analizzato
					if ((depList == null || depList.Count <= 0) && librariesInfo.ModuleNamespace != string.Empty)
					{
						this.libraries.Add(librariesInfo);
						continue;
					}
			
					foreach (XmlElement dep in depList)
					{
						string depPolicyString = dep.GetAttribute(DependenciesMapXML.Attribute.Policy);
						Debug.Assert(depPolicyString.Length != 0);
						DependenciesInfo dependenciesInfo = new DependenciesInfo
							(
							dep.GetAttribute(DependenciesMapXML.Attribute.Module),
							dep.GetAttribute(DependenciesMapXML.Attribute.Name),
							(Dll.DllDeploymentPolicy)Enum.Parse(typeof(Dll.DllDeploymentPolicy), depPolicyString, true)
							);

						librariesInfo.AddDependency(dependenciesInfo);
					}

					this.libraries.Add(librariesInfo);
				} // foreach (XmlElement lib in libList)
			} // if (libList != null && libList.Count > 0)
		}
	}
}