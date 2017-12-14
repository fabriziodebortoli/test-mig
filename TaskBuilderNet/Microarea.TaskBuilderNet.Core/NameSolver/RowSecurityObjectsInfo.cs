using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Xml;

using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Core.StringLoader;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.TaskBuilderNet.Core.NameSolver
{
	///<summary>
	/// Classe che mappa in memoria un nodo di tipo Entity "base", ovvero quello utilizzato 
	/// come figlio dei nodi Table
	///</summary>
	//================================================================================
	public class RSEntityBase
	{
		private string name;
		private List<RSColumns> rsColumns = new List<RSColumns>();

		//--------------------------------------------------------------------------------
		public string Name { get { return name; } }
		public List<RSColumns> RsColumns { get { return rsColumns; } set { rsColumns = value; } }

		//--------------------------------------------------------------------------------
		public RSEntityBase (string name)
		{
			this.name = name;
		}
	}

	///<summary>
	/// Classe che mappa in memoria un nodo di tipo Entity "full", ovvero quello che identifica
	/// la struttura in memoria del nodo di definizione dell'entita'
	///</summary>
	//================================================================================
	public class RSEntity
	{
		private IBaseModuleInfo parentModuleInfo;
		
		private string name;
		private string masterTableNamespace;
		private string documentNamespace;
		private string description;
		private int priority;

		private List<RSColumn> rsColumns = new List<RSColumn>();

		// Properties
		//--------------------------------------------------------------------------------
		public IBaseModuleInfo ParentModuleInfo { get { return parentModuleInfo; } }
		
		public string Name { get { return name; } }
		public string MasterTableNamespace { get { return masterTableNamespace; } }
		public string DocumentNamespace { get { return documentNamespace; }	}
		public string Description { get { return description; } }
		public int Priority { get { return priority; } set { priority = value; } }

		public List<RSColumn> RsColumns { get { return rsColumns; } set { rsColumns = value; } }

		public string MasterTableName
		{
			get
			{
				NameSpace namespaceTable = new NameSpace(masterTableNamespace, NameSpaceObjectType.Table);
				return namespaceTable.GetTokenValue(NameSpaceObjectType.Table); // ritorna il nome della tabella
			}
		}

		///<summary>
		/// Constructor
		///</summary>
		//--------------------------------------------------------------------------------
		public RSEntity(string name, string masterTableNs, string documentNs, string description, int priority, IBaseModuleInfo parentModuleInfo)
		{
			this.name = name;
			this.masterTableNamespace = masterTableNs;
			this.documentNamespace = documentNs;
			this.description = description;
			this.priority = priority;
			this.parentModuleInfo = parentModuleInfo;
		}
	}

	///<summary>
	/// Classe che mappa in memoria un nodo di tipo Table
	///</summary>
	//================================================================================
	public class RSTable
	{
		private string nameSpace;
		private List<RSEntityBase> rsEntityBaseList = new List<RSEntityBase>();

		// Properties
		//--------------------------------------------------------------------------------
		public string NameSpace { get { return nameSpace; } }
		public List<RSEntityBase> RsEntityBaseList { get { return rsEntityBaseList; } set { rsEntityBaseList = value; } }

		public string Name
		{
			get
			{
				NameSpace namespaceTable = new NameSpace(nameSpace, NameSpaceObjectType.Table);
				return namespaceTable.GetTokenValue(NameSpaceObjectType.Table);
			}
		}

		///<summary>
		/// Constructor
		///</summary>
		//--------------------------------------------------------------------------------
		public RSTable(string nameSpace)
		{
			this.nameSpace = nameSpace;
		}
	}

	///<summary>
	/// Classe che mappa in memoria un nodo di tipo RSColumns
	/// che contiene una lista di nodi di tipo RSColumn
	///</summary>
	//================================================================================
	public class RSColumns
	{
		public List<RSColumn> RSColumnList = new List<RSColumn>();
	}

	///<summary>
	/// Classe che mappa in memoria un nodo di tipo RSColumn
	///</summary>
	//================================================================================
	public class RSColumn
	{
		private string name;
		private string entityColumn;

		// Properties
		//--------------------------------------------------------------------------------
		public string Name { get { return name; } }
		public string EntityColumn { get { return entityColumn; } }

		///<summary>
		/// Constructors
		///</summary>
		//--------------------------------------------------------------------------------
		public RSColumn(string name)
			: this(name, string.Empty)
		{ }
		
		//--------------------------------------------------------------------------------
		public RSColumn(string name, string entity)
		{
			this.name = name;
			this.entityColumn = entity;
		}
	}

	/// <summary>
	/// Parse / unparse file RowSecurityObjectsInfo
	/// </summary>
	//================================================================================
	public class RowSecurityObjectsInfo
	{
		private string filePath;
		private IBaseModuleInfo parentModuleInfo;

		private List<RSEntity> rsEntitiesList = new List<RSEntity>();
		private List<RSTable> rsTablesList = new List<RSTable>();

		//--------------------------------------------------------------------------------
		public string FilePath { get { return filePath; } }
		public IBaseModuleInfo ParentModuleInfo { get { return parentModuleInfo; } }

		public List<RSEntity> RSEntities { get { return rsEntitiesList; } }
		public List<RSTable> RSTables { get { return rsTablesList; } }

		/// <summary>
		/// constructor
		/// </summary>
		//---------------------------------------------------------------------
		public RowSecurityObjectsInfo(string aFilePath, IBaseModuleInfo aParentModuleInfo)
		{
			if (string.IsNullOrWhiteSpace(aFilePath))
				Debug.Fail("Error in RowSecurityObjectsInfo file");

			filePath = aFilePath;
			parentModuleInfo = aParentModuleInfo;
		}

		# region Metodi di Parse
		/// <summary>
		/// Legge il file e crea la struttura in memoria con le informazioni in esso contenute
		/// </summary>
		/// <returns>true se la lettura ha avuto successo</returns>
		//---------------------------------------------------------------------
		public bool Parse()
		{
			if (
				!File.Exists(filePath) ||
				parentModuleInfo == null ||
				parentModuleInfo.ParentApplicationInfo == null
				)
				return false;

			LocalizableXmlDocument xmlDoc = new LocalizableXmlDocument
				(
				parentModuleInfo.ParentApplicationInfo.Name,
				parentModuleInfo.Name,
				parentModuleInfo.PathFinder
				);

			try
			{
				//leggo il file
				xmlDoc.Load(filePath);

				//root di RowSecurityObjectsInfo.xml
				XmlElement root = xmlDoc.DocumentElement;

				// cerco il nodo Entities
				XmlNodeList entitiesElem = root.GetElementsByTagName(RowSecurityObjectsXML.Element.Entities);
				if (entitiesElem != null && entitiesElem.Count > 0)
				{
					//cerco il tag Entity
					XmlNodeList entityNodes = ((XmlElement)entitiesElem[0]).GetElementsByTagName(RowSecurityObjectsXML.Element.Entity);
					// richiamo il parse dei nodi Entity
					ParseEntityNodes(entityNodes);
				}

				// cerco il nodo Tables
				XmlNodeList tablesElem = root.GetElementsByTagName(RowSecurityObjectsXML.Element.Tables);
				if (tablesElem != null && tablesElem.Count > 0)
				{
					//cerco il tag Table
					XmlNodeList tableNodes = ((XmlElement)tablesElem[0]).GetElementsByTagName(RowSecurityObjectsXML.Element.Table);
					// richiamo il parse dei nodi Table
					ParseTableNodes(tableNodes);
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
		/// Parse dei nodi di tipo Entity
		///</summary>
		//---------------------------------------------------------------------
		public bool ParseEntityNodes(XmlNodeList entityNodes)
		{
			if (entityNodes == null)
				return false;

			string name, masterTableNs = string.Empty, documentNs = string.Empty, description = string.Empty;
			int priority = 0;

			try
			{
				foreach (XmlElement xEntity in entityNodes)
				{
					name = xEntity.GetAttribute(RowSecurityObjectsXML.Attribute.Name);

					// tag MasterTableNamespace
					XmlNodeList masterTableNsNode = xEntity.GetElementsByTagName(RowSecurityObjectsXML.Element.MasterTableNamespace);
					if (masterTableNsNode != null && masterTableNsNode.Count > 0)
						masterTableNs = ((XmlElement)masterTableNsNode[0]).InnerText;

					// cerco il tag DocumentNamespaces
					XmlNodeList documentNssNode = xEntity.GetElementsByTagName(RowSecurityObjectsXML.Element.DocumentNamespaces);
					if (documentNssNode != null && documentNssNode.Count > 0)
					{
						// tag DocumentNamespace
						XmlNodeList documentNsNode = xEntity.GetElementsByTagName(RowSecurityObjectsXML.Element.DocumentNamespace);
						if (documentNsNode != null && documentNsNode.Count > 0)
							documentNs = ((XmlElement)documentNsNode[0]).InnerText; // per ora prendo il primo
					}

					// tag Description
					XmlNodeList descriptionNode = xEntity.GetElementsByTagName(RowSecurityObjectsXML.Element.Description);
					if (descriptionNode != null && descriptionNode.Count > 0)
						description = ((XmlElement)descriptionNode[0]).InnerText;

					// tag priority
					XmlNodeList priorityNode = xEntity.GetElementsByTagName(RowSecurityObjectsXML.Element.Priority);
					if (priorityNode != null && priorityNode.Count > 0)
					{
						try
						{
							priority = Int32.Parse(((XmlElement)priorityNode[0]).InnerText);
						}
						catch
						{ }
					}

					RSEntity rsEntity = new RSEntity(name, masterTableNs, documentNs, description, priority, this.parentModuleInfo);
					rsEntitiesList.Add(rsEntity);

					rsEntity.RsColumns = ParseColumns(xEntity);
				}
			}
			catch (XmlException xmlEx)
			{
				throw(xmlEx);
			}
			catch (Exception ex)
			{
				throw (ex);
			}

			return true;
		}

		///<summary>
		/// Parse dei nodi di tipo Table
		///</summary>
		//---------------------------------------------------------------------
		public bool ParseTableNodes(XmlNodeList tableNodes)
		{
			if (tableNodes == null)
				return false;

			try
			{
				foreach (XmlElement xTable in tableNodes)
				{
                    string nameSpace = xTable.GetAttribute(RowSecurityObjectsXML.Attribute.Namespace);
		
					RSTable rsTable = new RSTable(nameSpace);
					
					// cerco i nodi Entity
					XmlNodeList rsEntityNodes = xTable.GetElementsByTagName(RowSecurityObjectsXML.Element.Entity);
					if (rsEntityNodes != null && rsEntityNodes.Count > 0)
					{
						foreach (XmlElement xEntityBase in rsEntityNodes)
							rsTable.RsEntityBaseList.Add(ParseEntityBaseNodes(xEntityBase));
					}

					rsTablesList.Add(rsTable);
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

		///<summary>
		/// Parse dei nodi di tipo Entity "base", ovvero figli dei nodi Table
		///</summary>
		//---------------------------------------------------------------------
		private RSEntityBase ParseEntityBaseNodes(XmlElement xEntityBase)
		{
			RSEntityBase rseb = new RSEntityBase(xEntityBase.GetAttribute(RowSecurityObjectsXML.Attribute.Name));

			XmlNodeList xRSColumnsList = xEntityBase.GetElementsByTagName(RowSecurityObjectsXML.Element.RSColumns);

			if (xRSColumnsList != null && xRSColumnsList.Count > 0)
			{
				for (int i = 0; i < xRSColumnsList.Count; i++)
				{
					XmlNodeList rsColumnNodes = xRSColumnsList[i].SelectNodes(RowSecurityObjectsXML.Element.RSColumn);

					if (rsColumnNodes != null && rsColumnNodes.Count > 0)
					{
						RSColumns rsColumnList = new RSColumns();
						rseb.RsColumns.Add(rsColumnList);

						foreach (XmlElement xCol in rsColumnNodes)
						{
							string colName = xCol.InnerText;
							string colEntity = xCol.GetAttribute(RowSecurityObjectsXML.Attribute.EntityColumn);

							RSColumn rsCol = new RSColumn(colName, colEntity);
							rsColumnList.RSColumnList.Add(rsCol);
						}

					}
				}
			}

			return rseb;
		}

		///<summary>
		/// Parse dei nodi di tipo RSColumn, figli dei nodi Entity
		///</summary>
		//---------------------------------------------------------------------
		private List<RSColumn> ParseColumns(XmlElement xElement)
		{
			List<RSColumn> columns = new List<RSColumn>();

			//cerco il tag RSColumns (attenzione che e' presente solo per i nodi Entity)
			XmlNodeList rsColumnsNode = xElement.GetElementsByTagName(RowSecurityObjectsXML.Element.RSColumns);

			if (rsColumnsNode != null && rsColumnsNode.Count > 0)
			{
				// cerco i nodi Column
				foreach (XmlElement xColumn in rsColumnsNode)
				{
					XmlNodeList rsColumnList = xColumn.GetElementsByTagName(RowSecurityObjectsXML.Element.RSColumn);

					if (rsColumnList != null && rsColumnList.Count > 0)
					{
						foreach (XmlElement xCol in rsColumnList)
						{
							string colName = xCol.InnerText;
							string colEntity = xCol.GetAttribute(RowSecurityObjectsXML.Attribute.EntityColumn);

							RSColumn rsCol = new RSColumn(colName, colEntity);
							columns.Add(rsCol);
						}
					}
				}
			}

			return columns;
		}
		#endregion

		#region Metodi di Unparse
		///<summary>
		/// Metodo per scrivere nodi nel file
		///<Entities>
		///	<Entity name = "AGENTE">
		///		<MasterTableNamespace>ERP.SalesPeople.Dbl.MA_SalesPeople</MasterTableNamespace>
		///		<DocumentNamespaces>
		///      <DocumentNamespace>ERP.SalesPeople.Documents.SalesPeople</DocumentNamespace>
		///		</DocumentNamespaces>
		///		<HKLNamespace>ERP.SalesPeople.Dbl.SalesPeople</HKLNamespace>
		///		<NumbererNamespace></NumbererNamespace>
		///		<Description></Description>
		///		<Priority>2</Priority>
		///		<RSColumns>
		///			<RSColumn>Salesperson</RSColumn>
		///		</RSColumns>
		///	</Entity>
		///</Entities>
		///<Tables>
		///	<Table namespace="ERP.SalesPeople.Dbl.MA_SalesPeople">
		///	<Entity name="AGENTE">
		///	<RSColumns>
		///		<RSColumn entitycolumn="Salesperson">Salesperson</RSColumn>
		///  </RSColumns>
		/// </Entity>
		///</Table>
		///</Tables>
		///</summary>
		//---------------------------------------------------------------------
		public bool Unparse()
		{
			try
			{
				// XMLDocument + XmlDeclaration
				XmlDocument rowSecurityObjectsXmlDoc = new XmlDocument();
				XmlDeclaration xmlDeclaration = rowSecurityObjectsXmlDoc.CreateXmlDeclaration(NameSolverStrings.XmlDeclarationVersion, NameSolverStrings.XmlDeclarationEncoding, "yes");
				rowSecurityObjectsXmlDoc.AppendChild(xmlDeclaration);
				// root dell'xmldoc
				XmlElement rootElement = rowSecurityObjectsXmlDoc.CreateElement(RowSecurityObjectsXML.Element.RowSecurityObjects);
				rowSecurityObjectsXmlDoc.AppendChild(rootElement);
				// nodo <Entities>
				XmlElement entitiesElement = rowSecurityObjectsXmlDoc.CreateElement(RowSecurityObjectsXML.Element.Entities);
				rootElement.AppendChild(entitiesElement);

				// ri-scrivo i nodi <Entity> 
				foreach (RSEntity e in this.rsEntitiesList)
					UnparseEntityNode(e, entitiesElement, rowSecurityObjectsXmlDoc);

				// nodo <Tables>
				XmlElement tablesElement = rowSecurityObjectsXmlDoc.CreateElement(RowSecurityObjectsXML.Element.Tables);
				rootElement.AppendChild(tablesElement);
				// vado a ri-scrivere i nodi <Table> 
				foreach (RSTable t in this.rsTablesList)
					UnparseTableNode(t, tablesElement, rowSecurityObjectsXmlDoc);

				// salvataggio del file (sovrascrivendo il contenuto del file se gia' esistente)
				if (File.Exists(this.filePath) && (File.GetAttributes(this.filePath) & FileAttributes.ReadOnly)== FileAttributes.ReadOnly)
					File.SetAttributes(this.filePath, FileAttributes.Normal);

				rowSecurityObjectsXmlDoc.Save(this.filePath);

				// Anna mi ha detto di non mettere il readonly
				//File.SetAttributes(this.filePath, FileAttributes.ReadOnly);

				// avendo effettuato l'Unparse devo forzare il reload del file, quindi metto il RowSecurityObjectsInfo a null
				BaseModuleInfo modInfo = parentModuleInfo as BaseModuleInfo;
				modInfo.RowSecurityObjectsInfo = null;
			}
			catch (XmlException xe)
			{
				Debug.Fail(xe.ToString());
				return false;
			}
			catch (Exception err)
			{
				Debug.Fail(err.ToString());
				return false;
			}

			return true;
		}

		///<summary>
		/// Scrive un nodo di tipo Entity e le sue RSColumns
		///</summary>
		//---------------------------------------------------------------------
		private void UnparseEntityNode(RSEntity e, XmlElement parentElement, XmlDocument xmlDoc)
		{
			try
			{
				// nodo <Entity>
				XmlElement entity = xmlDoc.CreateElement(RowSecurityObjectsXML.Element.Entity);
				entity.SetAttribute(RowSecurityObjectsXML.Attribute.Name, e.Name);

				// tag <MasterTableNamespace>
				XmlElement masterTableNs = xmlDoc.CreateElement(RowSecurityObjectsXML.Element.MasterTableNamespace);
				masterTableNs.InnerText = e.MasterTableNamespace;
				entity.AppendChild(masterTableNs);

				// tag <DocumentNamespaces>
				XmlElement documentNss = xmlDoc.CreateElement(RowSecurityObjectsXML.Element.DocumentNamespaces);

				// tag singoli <DocumentNamespace>
				XmlElement documentNs = xmlDoc.CreateElement(RowSecurityObjectsXML.Element.DocumentNamespace);
				documentNs.InnerText = e.DocumentNamespace;
				documentNss.AppendChild(documentNs);

				entity.AppendChild(documentNss);

				// nodo <Description>
				XmlElement entityDescri = xmlDoc.CreateElement(RowSecurityObjectsXML.Element.Description);
				entityDescri.InnerText = e.Description;
				entity.AppendChild(entityDescri);

				// nodo <Priority>
				XmlElement priority = xmlDoc.CreateElement(RowSecurityObjectsXML.Element.Priority);
				priority.InnerText = e.Priority.ToString();
				entity.AppendChild(priority);

				// nodo <RSColumns>
				XmlElement rsCols = xmlDoc.CreateElement(RowSecurityObjectsXML.Element.RSColumns);
				foreach (RSColumn col in e.RsColumns)
				{
					// nodo <RSColumn>
					XmlElement rsCol = xmlDoc.CreateElement(RowSecurityObjectsXML.Element.RSColumn);
					rsCol.InnerText = col.Name;
					rsCols.AppendChild(rsCol);
				}
				entity.AppendChild(rsCols);
				parentElement.AppendChild(entity);
			}
			catch (XmlException xe)
			{
				throw(xe);
			}
		}

		///<summary>
		/// Scrive un nodo di tipo Table e le sue RSColumns
		///</summary>
		//---------------------------------------------------------------------
		private void UnparseTableNode(RSTable t, XmlElement parentElement, XmlDocument xmlDoc)
		{
			try
			{
				// nodo <Table>
				XmlElement table = xmlDoc.CreateElement(RowSecurityObjectsXML.Element.Table);
				table.SetAttribute(RowSecurityObjectsXML.Attribute.Namespace, t.NameSpace);

				foreach (RSEntityBase rse in t.RsEntityBaseList)
				{
					// nodo <Entity>
					XmlElement entity = xmlDoc.CreateElement(RowSecurityObjectsXML.Element.Entity);
					entity.SetAttribute(RowSecurityObjectsXML.Attribute.Name, rse.Name);

					foreach (RSColumns rsCols in rse.RsColumns)
					{
						// creo il nodo <RSColumns>
						XmlElement rsColsElement = xmlDoc.CreateElement(RowSecurityObjectsXML.Element.RSColumns);

						foreach (RSColumn column in rsCols.RSColumnList)
						{
							// creo il nodo <RSColumn>
							XmlElement rsCol = xmlDoc.CreateElement(RowSecurityObjectsXML.Element.RSColumn);
							rsCol.SetAttribute(RowSecurityObjectsXML.Attribute.EntityColumn, column.EntityColumn);
							rsCol.InnerText = column.Name;
							rsColsElement.AppendChild(rsCol);
						}
						entity.AppendChild(rsColsElement);
					}
					table.AppendChild(entity);
				}
				parentElement.AppendChild(table);
			}
			catch (XmlException xe)
			{
				throw(xe);
			}
		}
		# endregion
	}
}
