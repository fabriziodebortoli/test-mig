using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Xml;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.TaskBuilderNet.Core.NameSolver
{
	/// <summary>
	/// DBObjects - classe che espone la lista degli oggetti di database dichiarati dal modulo
	/// </summary>
	//=========================================================================
	public class DBObjects : IDBObjects
	{
		private IBaseModuleInfo parentModuleInfo;

		protected List<IDbObjectInfo> tableInfoList;
		protected List<IDbObjectInfo> viewInfoList;
		protected List<IDbObjectInfo> procedureInfoList;
		protected List<IAddOnDbObjectInfo> extraAddedColsList;

		// Properties
		//--------------------------------------------------------------------------------
		public IBaseModuleInfo ParentModuleInfo { get { return parentModuleInfo; } }

		// Liste delle tabelle, view, procedure, additionalColumns
		//--------------------------------------------------------------------------------
		public List<IDbObjectInfo> TableInfoList			{ get { return tableInfoList; } }
		public List<IDbObjectInfo> ViewInfoList				{ get { return viewInfoList; } }
		public List<IDbObjectInfo> ProcedureInfoList		{ get { return procedureInfoList; } }
		public List<IAddOnDbObjectInfo> ExtraAddedColsList	{ get { return extraAddedColsList; } }

		//--------------------------------------------------------------------------------
		public DBObjects(IBaseModuleInfo parentModuleInfo)
		{
			this.parentModuleInfo = parentModuleInfo;
			tableInfoList = new List<IDbObjectInfo>();
			viewInfoList = new List<IDbObjectInfo>();
			procedureInfoList = new List<IDbObjectInfo>();
			extraAddedColsList = new List<IAddOnDbObjectInfo>();
		}
	}

	///<summary>
	/// class DBObjectInfo
	/// Classe generica che si occupa del parse di un singolo file xml di descrizione di 
	/// oggetti di database, previsto per la versione 3.0
	/// In un file xml potrebbero essere descritti più oggetti, infatti la classe espone la
	/// visibilità degli oggetti con le apposite proprietà
	///</summary>
	//=========================================================================
	public class DBObjectInfo
	{
		private XmlDocument myDocument = new XmlDocument();

		private IBaseModuleInfo parentModuleInfo;

		protected List<IDbObjectInfo> tableInfoList;
		protected List<IDbObjectInfo> viewInfoList;
		protected List<IDbObjectInfo> procedureInfoList;
		protected List<IAddOnDbObjectInfo> extraAddedColsList;

		// Properties
		//--------------------------------------------------------------------------------
		public IBaseModuleInfo ParentModuleInfo { get { return parentModuleInfo; } }

		// Liste delle tabelle, view, procedure, additionalColumns
		//--------------------------------------------------------------------------------
		public List<IDbObjectInfo> TableInfoList { get { return tableInfoList; } }
		public List<IDbObjectInfo> ViewInfoList { get { return viewInfoList; } }
		public List<IDbObjectInfo> ProcedureInfoList { get { return procedureInfoList; } }
		public List<IAddOnDbObjectInfo> ExtraAddedColsList { get { return extraAddedColsList; } }

		///<summary>
		/// Costruttore
		///</summary>
		//---------------------------------------------------------------------
		public DBObjectInfo(IBaseModuleInfo parentModuleInfo)
		{
			this.parentModuleInfo = parentModuleInfo;

			tableInfoList = new List<IDbObjectInfo>();
			viewInfoList = new List<IDbObjectInfo>();
			procedureInfoList = new List<IDbObjectInfo>();
			extraAddedColsList = new List<IAddOnDbObjectInfo>();
		}

		///<summary>
		/// Parse di un file .dbxml per estrapolare i nomi degli oggetti in esso dichiarati
		///</summary>
		//---------------------------------------------------------------------
		public bool ParseObjectsFromFile(string filePath)
		{
			if (!File.Exists(filePath))
				return false;

			try
			{
				myDocument.Load(filePath);

				// check nome root
				if (string.Compare(myDocument.DocumentElement.Name, DBObjectXML.Element.RootElement, StringComparison.InvariantCultureIgnoreCase) != 0)
				{
					Debug.Fail(string.Format("No valid root element in file {0}", filePath));
					return false;
				}

				// nodi di tipo <Tables>
				XmlElement tables = myDocument.DocumentElement.SelectSingleNode(DBObjectXML.Element.Tables) as XmlElement;
				if (tables != null)
				{
					XmlNodeList tableList = tables.SelectNodes(DBObjectXML.Element.Table);
					if (tableList != null && tableList.Count > 0)
						ParseObjects(tableList);
				}

				// nodi di tipo <Views>
				XmlElement views = myDocument.DocumentElement.SelectSingleNode(DBObjectXML.Element.Views) as XmlElement;
				if (views != null)
				{
					XmlNodeList viewList = views.SelectNodes(DBObjectXML.Element.View);
					if (viewList != null && viewList.Count > 0)
						ParseObjects(viewList);
				}

				// nodi di tipo <Procedures>
				XmlElement procedures = myDocument.DocumentElement.SelectSingleNode(DBObjectXML.Element.Procedures) as XmlElement;
				if (procedures != null)
				{
					XmlNodeList procedureList = procedures.SelectNodes(DBObjectXML.Element.Procedure);
					if (procedureList != null && procedureList.Count > 0)
						ParseObjects(procedureList);
				}

				// nodi di tipo <ExtraAddedColumns>
				XmlElement addedColumns = myDocument.DocumentElement.SelectSingleNode(DBObjectXML.Element.ExtraAddedColumns) as XmlElement;
				if (addedColumns != null)
				{
					XmlNodeList addedColList = addedColumns.SelectNodes(DBObjectXML.Element.ExtraAddedColumn);
					if (addedColList != null && addedColList.Count > 0)
						ParseAddOnColumns(addedColList);
				}
			}
			catch (XmlException xmlEx)
			{
				Debug.Fail(xmlEx.Message);
				return false;
			}
			catch (Exception ex)
			{
				Debug.Fail(ex.Message);
				return false;
			}

			return true;
		}

		///<summary>
		/// ParseObjects
		/// Parse dei singoli oggetti suddivisi per tipo (Table, View, Procedure)
		///</summary>
		//---------------------------------------------------------------------
		private void ParseObjects(XmlNodeList objectList)
		{
			foreach (XmlElement objectNode in objectList)
			{
				string objectName = objectNode.GetAttribute(DBObjectXML.Attribute.Name);
				if (string.IsNullOrEmpty(objectName))
					continue;

				string objectNameSpace = objectNode.GetAttribute(DBObjectXML.Attribute.TbNamespace);

				int release;
				int.TryParse(objectNode.GetAttribute(DBObjectXML.Attribute.DbReleaseNumber), out release);

               switch (objectNode.Name)
				{ 
					case DBObjectXML.Element.Table:
						TableInfo table = new TableInfo(parentModuleInfo, objectName, release, 0, objectNameSpace, false);
						tableInfoList.Add(table);
						break;

					case DBObjectXML.Element.View:
						ViewInfo view = new ViewInfo(objectName, release, 0, objectNameSpace);
						viewInfoList.Add(view);
						break;

					case DBObjectXML.Element.Procedure:
						ProcedureInfo procedure = new ProcedureInfo(objectName, release, 0, objectNameSpace);
						procedureInfoList.Add(procedure);
						break;
				}
			}
		}

		/// <summary>
		/// ParseAddOnColumns
		/// Si occupa di parsare i nodi relativi alle AddOnColumns
		/// </summary>
		//---------------------------------------------------------------------
		private void ParseAddOnColumns(XmlNodeList addedColList)
		{
			foreach (XmlElement extraNode in addedColList)
			{
				// leggo i nodi di tipo <ExtraAddedColumn> e i loro attributi
				string tableName = extraNode.GetAttribute(DBObjectXML.Attribute.TableName);
				if (string.IsNullOrEmpty(tableName))
					continue;

				string tbNamespace = extraNode.GetAttribute(DBObjectXML.Attribute.TbNamespace);
				string libNamespace = extraNode.GetAttribute(DBObjectXML.Attribute.LibraryNamespace);

				// estraggo i nodi di tipo <Column>
				XmlNodeList cols = extraNode.SelectNodes(DBObjectXML.Element.Columns + "/" + DBObjectXML.Element.Column);
				if (cols != null && cols.Count > 0)
				{
					foreach (XmlElement col in cols)
					{
						string colName = col.GetAttribute(DBObjectXML.Attribute.Name);
						if (string.IsNullOrEmpty(colName))
							continue;

						int release;
						int.TryParse(col.GetAttribute(DBObjectXML.Attribute.DbReleaseNumber), out release);

						int createstep;
						int.TryParse(col.GetAttribute(DBObjectXML.Attribute.CreateStep), out createstep);

						ExtraAddedColumnInfo myColumn = new ExtraAddedColumnInfo
							(colName, 
							libNamespace,
							release, 
							createstep, 
							tableName, 
							tbNamespace
							);
						extraAddedColsList.Add(myColumn);
					}
				}
			}
		}
	}
}
