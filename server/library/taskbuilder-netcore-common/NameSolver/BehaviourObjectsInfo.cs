using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Xml;

using Microarea.Common.StringLoader;
using TaskBuilderNetCore.Interfaces;

namespace Microarea.Common.NameSolver
{
    /// <summary>
    /// Nodo Entita'
    /// </summary>
    //================================================================================
    public class Entity
	{
		private string nameSpace;
		private string localize;
		private string service;

		//--------------------------------------------------------------------------------
		public string NameSpace { get { return nameSpace; } }
		public string Localize { get { return localize; } }
		public string Service { get { return service; } }

		//--------------------------------------------------------------------------------
		public Entity(string nameSpace, string localize, string service)
		{
			this.nameSpace = nameSpace;
			this.localize = localize;
			this.service = service;
		}
	}

	/// <summary>
	/// Nodo Service
	/// </summary>
	//================================================================================
	public class Service
	{
		private string nameSpace;
		private string localize;

		//--------------------------------------------------------------------------------
		public string NameSpace { get { return nameSpace; } }
		public string Localize { get { return localize; } }

		//--------------------------------------------------------------------------------
		public Service(string nameSpace, string localize)
		{
			this.nameSpace = nameSpace;
			this.localize = localize;
		}
	}

	/// <summary>
	/// Parse file BehaviourObjects.xml
	/// </summary>
	//================================================================================
	public class BehaviourObjectsInfo
	{
		private string filePath;
		private ModuleInfo parentModuleInfo;
		List<Entity> entitiesList = new List<Entity>();
		List<Service> servicesList = new List<Service>();

		//--------------------------------------------------------------------------------
		public string FilePath { get { return filePath; } }
		public ModuleInfo ParentModuleInfo { get { return parentModuleInfo; } }

		public List<Entity> Entities { get { return entitiesList; } }
		public List<Service> Services { get { return servicesList; } }

		//--------------------------------------------------------------------------------
		public BehaviourObjectsInfo(string aFilePath, ModuleInfo aParentModuleInfo)
		{
			if (string.IsNullOrWhiteSpace(aFilePath))
				Debug.Fail("Error in BehaviourObjectsInfo file");

			filePath = aFilePath;
			parentModuleInfo = aParentModuleInfo;
		}

		/// <summary>
		/// Legge il file e crea la struttura in memoria con le informazioni in esso contenute
		/// </summary>
		/// <returns>true se la lettura ha avuto successo</returns>
		//---------------------------------------------------------------------
		public bool Parse()
		{
			if (
				!PathFinder.PathFinderInstance.ExistFile(filePath)||
				parentModuleInfo == null ||
				parentModuleInfo.ParentApplicationInfo == null
				)
				return false;

			LocalizableXmlDocument xmlDoc = new LocalizableXmlDocument
				(
				parentModuleInfo.ParentApplicationInfo.Name,
				parentModuleInfo.Name,
				parentModuleInfo.CurrentPathFinder
                );

			try
			{
				//leggo il file
				xmlDoc.Load(filePath);

				//root di BehaviourObjects.xml
				XmlElement root = xmlDoc.DocumentElement;

				// cerco il nodo Entities
				XmlNodeList entitiesElem = root.GetElementsByTagName(BehaviourObjectsXML.Element.Entities);
				if (entitiesElem != null && entitiesElem.Count > 0)
				{
					//cerco il tag Entity e il suo parse
					XmlNodeList entityNodes = ((XmlElement)entitiesElem[0]).GetElementsByTagName(BehaviourObjectsXML.Element.Entity);
					ParseNodes(entityNodes);
				}

				// cerco il nodo Services
				XmlNodeList servicesElem = root.GetElementsByTagName(BehaviourObjectsXML.Element.Services);
				if (servicesElem != null && servicesElem.Count > 0)
				{
					//cerco il tag Service e il suo parse
					XmlNodeList serviceNodes = ((XmlElement)servicesElem[0]).GetElementsByTagName(BehaviourObjectsXML.Element.Service);
					ParseNodes(serviceNodes);
				}
			}
			catch (XmlException err)
			{
				Debug.Fail(err.Message);
				return false;
			}
			catch (Exception e)
			{
				Debug.Fail(e.Message);
				return false;
			}

			return true;
		}

		///<summary>
		/// Parse dei nodi figli
		///</summary>
		//---------------------------------------------------------------------
		public bool ParseNodes(XmlNodeList nodes)
		{
			if (nodes == null)
				return false;

			try
			{
				foreach (XmlElement xNode in nodes)
				{
					string nameSpace = xNode.GetAttribute(BehaviourObjectsXML.Attribute.Namespace);
					string localize = xNode.GetAttribute(BehaviourObjectsXML.Attribute.Localize);

					// faccio questo if per avere solo un metodo di Parse (visto che i nodi differenziano solo per questo attributo)
					if (xNode.HasAttribute(BehaviourObjectsXML.Attribute.Service)) 
					{
						string service = xNode.GetAttribute(BehaviourObjectsXML.Attribute.Service);
						Entity entityNode = new Entity(nameSpace, localize, service);
						entitiesList.Add(entityNode);
						continue;
					}

					Service serviceNode = new Service(nameSpace, localize);
					servicesList.Add(serviceNode);
				}
			}
			catch (XmlException xmlEx)
			{
				throw (xmlEx);
			}
			catch (Exception ex)
			{
				throw (ex);
			}

			return true;
		}
	}
}
