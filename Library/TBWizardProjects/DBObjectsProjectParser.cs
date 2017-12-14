using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Xml;
using System.Xml.XPath;
using Microarea.Library.SqlScriptUtility;

namespace Microarea.Library.TBWizardProjects
{
	/// <summary>
	/// classe DBObjectsProjectParser
	/// derivata da TBWizardProjectParser e specializzata per i nostri usi
	/// </summary>
	//=========================================================================
	public class DBObjectsProjectParser : TBWizardProjectParser
	{
		//---------------------------------------------------------------------------
		protected override string RootTag { get { return "DBObjects"; } }

		//---------------------------------------------------------------------------
		public DBObjectsProjectParser(string aProjectFileName)
			: base(aProjectFileName)
		{ }

		//---------------------------------------------------------------------------
		public DBObjectsProjectParser()
			: this(String.Empty)
		{ }

		///<summary>
		/// La funzione ritorna sempre true perchè non abbiamo bisogno di parsare le applicazioni
		/// ma partiamo direttamente 
		///</summary>
		//---------------------------------------------------------------------------
		protected override bool ParseApplicationInfo(XmlElement aProjectRoot)
		{
			return true;
		}

		# region ParseTablesInfoNode (lettura nodi di tipo tabella)
		///<summary>
		/// Parse nodi di tipo tabella (al suo interno richiama la ParseTableInfoNode del papà)
		///</summary>
		//---------------------------------------------------------------------------
		public IList<WizardTableInfo> ParseTablesInfoNode(XmlDocument xmlDBObjectsDoc, bool addMandatoryColumns)
		{
			if (xmlDBObjectsDoc == null || xmlDBObjectsDoc.DocumentElement == null)
				return null;

			XmlElement tables = xmlDBObjectsDoc.DocumentElement.SelectSingleNode(XML_TAG_TABLES) as XmlElement;
			if (tables == null)
				return null;

			XmlNodeList tableList = tables.SelectNodes(XML_TAG_TABLE);
			if (tableList == null)
				return null;

			IList<WizardTableInfo> list = new List<WizardTableInfo>();

			foreach (XmlElement aTableInfoNode in tableList)
			{
				WizardTableInfo wti = ParseTableInfoNode(aTableInfoNode);
				if (wti == null)
					continue;

                // aggiungo in coda le colonne obbligatorie TBCreated, TBModified, TBCreatedID e TBModifiedID
				// (previste solo x il database aziendale e non in quello di sistema)
				if (addMandatoryColumns)
					wti.AddTbMandatoryColumnInfo();

				IList<WizardForeignKeyInfo> FKList = ParseTableForeignKeysInfo(aTableInfoNode);
				if (FKList != null)
					foreach (WizardForeignKeyInfo wfki in FKList)
						wti.AddForeignKeyInfo(wfki);

				list.Add(wti);

				IList<WizardTableIndexInfo> IndexList = ParseTableIndexInfo(aTableInfoNode);
				wti.Indexes = IndexList;
			}

			return list;
		}

		///<summary>
		/// GetDBReleaseNumber
		/// Metodo di override per portarci dietro il numero di release dell'oggetto
		/// [nel wizard tale informazione viene caricata contestualmente al caricamento delle libraries]
		///</summary>
		//---------------------------------------------------------------------------
		protected override void GetDBReleaseNumber(XmlElement aTableElement, WizardTableInfo tableInfo)
		{
			if (aTableElement == null || tableInfo == null)
				return;

			string dbReleaseNumberText = ((XmlElement)aTableElement).GetAttribute(XML_DB_REL_NUMBER_ATTRIBUTE);

			uint dbReleaseNumber = 0;
			if (!string.IsNullOrEmpty(dbReleaseNumberText))
				UInt32.TryParse(dbReleaseNumberText, out dbReleaseNumber);

			tableInfo.CreationDbReleaseNumber = dbReleaseNumber;
		}
		# endregion

		# region Parse e Unparse nodi di tipo TableUpdate
		//---------------------------------------------------------------------------
		public IList<TableUpdate> ParseTableUpdate(string path)
		{
			if (String.IsNullOrEmpty(path) || !File.Exists(path))
				return null;

			XmlDocument xmlDBObjectsDoc = new XmlDocument();

			try
			{
				xmlDBObjectsDoc.Load(path);
				return ParseTableUpdate(xmlDBObjectsDoc);
			}
			catch
			{
				return null;
			}
		}

		///<summary>
		/// Parse nodi di tipo TableUpdate
		///</summary>
		//---------------------------------------------------------------------------
		public IList<TableUpdate> ParseTableUpdate(XmlDocument xmlDBObjectsDoc)
		{
			if (xmlDBObjectsDoc == null || xmlDBObjectsDoc.DocumentElement == null)
				return null;

			XmlNodeList updateNodes =
				xmlDBObjectsDoc.DocumentElement.SelectNodes(XML_TAG_TABLES_UPDATE + "/" + XML_TAG_TABLE_UPDATE);
			if (updateNodes == null)
				return null;

			IList<TableUpdate> tableUpdates = new List<TableUpdate>();
			foreach (XmlElement updateNode in updateNodes)
			{
				string tableName = updateNode.GetAttribute(XML_TABLE_NAME_ATTRIBUTE);
				if (String.IsNullOrEmpty(tableName))
					continue;

				TableUpdate tu = new TableUpdate(tableName);
				tu.SetColumnName = updateNode.GetAttribute(XML_SET_COLUMN_NAME_ATTRIBUTE);
				tu.SetValueForSql = updateNode.GetAttribute(XML_SET_SQL_VALUE_ATTRIBUTE);
				tu.SetValueForOracle = updateNode.GetAttribute(XML_SET_ORACLE_VALUE_ATTRIBUTE);
				tu.SetValueAsString = false;//la stringa la recuperiamo già con gli apici e quindi non ci serve impostare la proprietà

				XmlElement whereNode = updateNode.SelectSingleNode(XML_TAG_WHERE_UPDATE) as XmlElement;
				if (whereNode != null)
				{
					tu.WhereColumnName = whereNode.GetAttribute(XML_WHERE_COLUMN_NAME_ATTRIBUTE);
					tu.WhereTableName = whereNode.GetAttribute(XML_WHERE_TABLE_NAME_ATTRIBUTE);
					tu.WhereValueForSql = whereNode.GetAttribute(XML_WHERE_SQL_VALUE_ATTRIBUTE);
					tu.WhereValueForOracle = whereNode.GetAttribute(XML_WHERE_ORACLE_VALUE_ATTRIBUTE);
				}
				tableUpdates.Add(tu);
			}

			return tableUpdates;
		}

		///<summary>
		/// Scrive nodi di tipo TableUpdate
		///</summary>
		//---------------------------------------------------------------------------
		public void AddTableUpdateNodes(TableUpdate tableUpdate)
		{
			if (tableUpdate == null || string.IsNullOrEmpty(tableUpdate.TableName))
				return;

			InitProject();

			XmlElement aTablesElement = projectDocument.DocumentElement.SelectSingleNode(XML_TAG_TABLES_UPDATE) as XmlElement;
			if (aTablesElement == null)
			{
				aTablesElement = projectDocument.CreateElement(XML_TAG_TABLES_UPDATE);
				projectDocument.DocumentElement.AppendChild(aTablesElement);
			}

			XmlElement aTableElement = projectDocument.CreateElement(XML_TAG_TABLE_UPDATE);
			aTableElement.SetAttribute(XML_TABLE_NAME_ATTRIBUTE, tableUpdate.TableName);
			aTableElement.SetAttribute(XML_SET_COLUMN_NAME_ATTRIBUTE, tableUpdate.SetColumnName);
			if (!string.IsNullOrEmpty(tableUpdate.SetValueForSql))
				aTableElement.SetAttribute(XML_SET_SQL_VALUE_ATTRIBUTE, tableUpdate.SetValueForSql);
			if (!string.IsNullOrEmpty(tableUpdate.SetValueForOracle))
				aTableElement.SetAttribute(XML_SET_ORACLE_VALUE_ATTRIBUTE, tableUpdate.SetValueForOracle);

			if (tableUpdate.ExistsWhereClause)
			{
				XmlElement aWhereElement = projectDocument.CreateElement(XML_TAG_WHERE_UPDATE);
				aWhereElement.SetAttribute(XML_WHERE_TABLE_NAME_ATTRIBUTE, tableUpdate.WhereTableName);
				aWhereElement.SetAttribute(XML_WHERE_COLUMN_NAME_ATTRIBUTE, tableUpdate.WhereColumnName);
				if (!string.IsNullOrEmpty(tableUpdate.WhereValueForSql))
					aWhereElement.SetAttribute(XML_WHERE_SQL_VALUE_ATTRIBUTE, tableUpdate.WhereValueForSql);
				if (!string.IsNullOrEmpty(tableUpdate.WhereValueForOracle))
					aWhereElement.SetAttribute(XML_WHERE_ORACLE_VALUE_ATTRIBUTE, tableUpdate.WhereValueForOracle);
				aTableElement.AppendChild(aWhereElement);
			}

			aTablesElement.AppendChild(aTableElement);
		}
		# endregion

		# region Parse e Unparse ExtraAddedColumn

		//---------------------------------------------------------------------------
		public IList<WizardExtraAddedColumnsInfo> ParseExtraAddedColumnsInfo(string path)
		{
			if (String.IsNullOrEmpty(path) || !File.Exists(path))
				return null;

			XmlDocument xmlDBObjectsDoc = new XmlDocument();

			try
			{
				xmlDBObjectsDoc.Load(path);
				return ParseExtraAddedColumnsInfo(xmlDBObjectsDoc);
			}
			catch
			{
				return null;
			}
		}

		///<summary>
		/// Parse nodi delle AdditionalColumns
		///</summary>
		//---------------------------------------------------------------------------
		public IList<WizardExtraAddedColumnsInfo> ParseExtraAddedColumnsInfo(XmlDocument xmlDBObjectsDoc)
		{
			if (xmlDBObjectsDoc == null || xmlDBObjectsDoc.DocumentElement == null)
				return null;

			XmlNodeList extraAddedColumns =
				xmlDBObjectsDoc.DocumentElement.SelectNodes(XML_TAG_EXTRA_ADDED_COLUMNS + "/" + XML_TAG_EXTRA_ADDED_COLUMN);
			if (extraAddedColumns == null)
				return null;

			IList<WizardExtraAddedColumnsInfo> alterList = new List<WizardExtraAddedColumnsInfo>();
			foreach (XmlNode extraAddedNode in extraAddedColumns)
			{
				string tableNameSpace = ((XmlElement)extraAddedNode).GetAttribute(XML_TB_NAMESPACE_ATTRIBUTE);
				WizardExtraAddedColumnsInfo list = ParseExtraAddedColumnInfo(extraAddedNode);
				alterList.Add(list);
			}

			return alterList;
		}

		//---------------------------------------------------------------------------
		protected override WizardExtraAddedColumnsInfo ParseExtraAddedColumnInfo(XmlNode extraAddedColumnNode)
		{
			if (extraAddedColumnNode == null || !(extraAddedColumnNode is XmlElement))
				return null;

			string tableNameSpace = ((XmlElement)extraAddedColumnNode).GetAttribute(XML_TB_NAMESPACE_ATTRIBUTE);
			if (string.IsNullOrEmpty(tableNameSpace))
				return null;

			string tableName = ((XmlElement)extraAddedColumnNode).GetAttribute(XML_TABLE_NAME_ATTRIBUTE);
			if (string.IsNullOrEmpty(tableName))
				return null;

			string libNamespace = ((XmlElement)extraAddedColumnNode).GetAttribute(XML_LIBRARY_NAMESPACE_ATTRIBUTE);

			XmlNode columnsInfoNode = extraAddedColumnNode.SelectSingleNode("child::" + XML_TAG_COLUMNS);
			if (columnsInfoNode == null || !(columnsInfoNode is XmlElement) || !columnsInfoNode.HasChildNodes)
				return null;

			XmlNodeList columnsList = columnsInfoNode.SelectNodes("child::" + XML_TAG_COLUMN);
			if (columnsList == null || columnsList.Count == 0)
				return null;

			DBObjectsExtraAddedColumnsInfo extraAddedColumnInfo = new DBObjectsExtraAddedColumnsInfo(tableNameSpace, tableName);
			extraAddedColumnInfo.LibraryNameSpace = libNamespace;

			foreach (XmlNode columnInfoNode in columnsList)
			{
				if (columnInfoNode == null || !(columnInfoNode is XmlElement))
					continue;

				WizardTableColumnInfo parsedColumnInfo = ParseBaseColumnInfoNode(extraAddedColumnInfo.TableName, (XmlElement)columnInfoNode);
				if (parsedColumnInfo == null)
					continue;

				extraAddedColumnInfo.AddColumnInfo(parsedColumnInfo);
			}

			return extraAddedColumnInfo;
		}

		///<summary>
		/// Scrive nodi per le colonne aggiuntive
		///</summary>
		//---------------------------------------------------------------------------
		public void AddExtraAddedColumnInfoToColumnsNode(WizardExtraAddedColumnsInfo aExtraAddedColumnInfo)
		{
			if (aExtraAddedColumnInfo == null)
				return;

			InitProject();
			XmlElement aTablesElement = projectDocument.DocumentElement.SelectSingleNode("child::" + XML_TAG_EXTRA_ADDED_COLUMNS) as XmlElement;

			if (aTablesElement == null)
			{
				aTablesElement = projectDocument.CreateElement(XML_TAG_EXTRA_ADDED_COLUMNS);
				projectDocument.DocumentElement.AppendChild(aTablesElement);
			}

			AddExtraAddedColumnInfoToColumnsNode(aTablesElement, aExtraAddedColumnInfo);
		}

		//---------------------------------------------------------------------------
		protected override void AddExtraAddedColumnInfoToColumnsNode(XmlElement aExtraAddedColumnsElement, WizardExtraAddedColumnsInfo aWizardExtraAddedColumnInfo)
		{
			if (projectDocument == null ||
				aWizardExtraAddedColumnInfo == null ||
				aWizardExtraAddedColumnInfo.ColumnsCount == 0 ||
				aExtraAddedColumnsElement == null ||
				String.Compare(aExtraAddedColumnsElement.Name, XML_TAG_EXTRA_ADDED_COLUMNS) != 0)
				return;

			DBObjectsExtraAddedColumnsInfo aExtraAddedColumnInfo = aWizardExtraAddedColumnInfo as DBObjectsExtraAddedColumnsInfo;
			if (aExtraAddedColumnInfo == null)
				return;
			XmlElement extraAddedColumnElement = null;

			XmlNodeList extraAddedColumnsList = aExtraAddedColumnsElement.SelectNodes("child::" + XML_TAG_EXTRA_ADDED_COLUMN);
			if (extraAddedColumnsList != null && extraAddedColumnsList.Count > 0)
			{
				foreach (XmlNode extraAddedColumnNode in extraAddedColumnsList)
				{
					if (extraAddedColumnNode == null ||
						!(extraAddedColumnNode is XmlElement) ||
						!((XmlElement)extraAddedColumnNode).HasAttribute(XML_TB_NAMESPACE_ATTRIBUTE) ||
						!((XmlElement)extraAddedColumnNode).HasAttribute(XML_LIBRARY_NAMESPACE_ATTRIBUTE)
						)
						continue;

					string tableNameSpaceText = ((XmlElement)extraAddedColumnNode).GetAttribute(XML_TB_NAMESPACE_ATTRIBUTE);
					if (string.IsNullOrEmpty(tableNameSpaceText))
						continue;

					string libraryNameSpaceText = ((XmlElement)extraAddedColumnNode).GetAttribute(XML_LIBRARY_NAMESPACE_ATTRIBUTE);
					if (string.IsNullOrEmpty(libraryNameSpaceText))
						continue;

					/*   NameSpace tableNameSpace = new NameSpace(tableNameSpaceText, NameSpaceObjectType.Table);
					   if (!tableNameSpace.IsValid())
						   continue;
					   */

					if (String.Compare(tableNameSpaceText, aExtraAddedColumnInfo.TbNameSpace) == 0 &&
						String.Compare(libraryNameSpaceText, aExtraAddedColumnInfo.LibraryNameSpace) == 0
						)
					{
						extraAddedColumnElement = (XmlElement)extraAddedColumnNode;
						break;
					}
				}
			}

			if (extraAddedColumnElement == null)
			{
				extraAddedColumnElement = projectDocument.CreateElement(XML_TAG_EXTRA_ADDED_COLUMN);
				aExtraAddedColumnsElement.AppendChild(extraAddedColumnElement);
			}

			SetNodeExtraAddedColumnsInfo(extraAddedColumnElement, aExtraAddedColumnInfo);
		}

		//---------------------------------------------------------------------------
		protected override void SetNodeExtraAddedColumnsInfo(XmlElement aExtraAddedColumnElement, WizardExtraAddedColumnsInfo aWizardExtraAddedColumnInfo)
		{
			if (projectDocument == null ||
				aWizardExtraAddedColumnInfo == null ||
				aWizardExtraAddedColumnInfo.ColumnsCount == 0 ||
				aExtraAddedColumnElement == null ||
				String.Compare(aExtraAddedColumnElement.Name, XML_TAG_EXTRA_ADDED_COLUMN) != 0)
				return;

			DBObjectsExtraAddedColumnsInfo aExtraAddedColumnInfo = aWizardExtraAddedColumnInfo as DBObjectsExtraAddedColumnsInfo;
			if (aExtraAddedColumnInfo == null)
				return;

			aExtraAddedColumnElement.RemoveAllAttributes();
			aExtraAddedColumnElement.SetAttribute(XML_TABLE_NAME_ATTRIBUTE, aExtraAddedColumnInfo.TableName);
			aExtraAddedColumnElement.SetAttribute(XML_LOCALIZE_ATTRIBUTE, aExtraAddedColumnInfo.TableName);
			aExtraAddedColumnElement.SetAttribute(XML_TB_NAMESPACE_ATTRIBUTE, aExtraAddedColumnInfo.TbNameSpace);
			aExtraAddedColumnElement.SetAttribute(XML_LIBRARY_NAMESPACE_ATTRIBUTE, aExtraAddedColumnInfo.LibraryNameSpace);

			XmlElement columnsElement = null;
			XmlNode columnsInfoNode = aExtraAddedColumnElement.SelectSingleNode("child::" + XML_TAG_COLUMNS);
			if (columnsInfoNode == null || !(columnsInfoNode is XmlElement))
			{
				columnsElement = projectDocument.CreateElement(XML_TAG_COLUMNS);
				aExtraAddedColumnElement.AppendChild(columnsElement);
			}
			else
				columnsElement = (XmlElement)columnsInfoNode;

			columnsElement.RemoveAll();

			foreach (WizardTableColumnInfo aColumnInfo in aExtraAddedColumnInfo.ColumnsInfo)
			{
				XmlElement columnElement = null;
				XmlNode columnNode = columnsElement.SelectSingleNode("child::" + XML_TAG_COLUMN + "[@" + XML_NAME_ATTRIBUTE + "='" + aColumnInfo.Name + "']");
				if (columnNode == null || !(columnNode is XmlElement))
				{
					columnElement = projectDocument.CreateElement(XML_TAG_COLUMN);
					columnsElement.AppendChild(columnElement);
				}
				else
					columnElement = (XmlElement)columnNode;
				SetNodeTableColumnInfo(columnElement, aColumnInfo, aExtraAddedColumnInfo.CreationDbReleaseNumber);
			}
		}
		# endregion

		# region Parse e Unparse nodi di tipo View
		///<summary>
		/// Lettura nodi di tipo View (ritorna solo le definizioni con sintassi SQL e ORACLE)
		///</summary>
		//---------------------------------------------------------------------------
		public SqlViewList ParseViewsInfoNode(XmlDocument xmlDBObjectsDoc)
		{
			if (xmlDBObjectsDoc == null || xmlDBObjectsDoc.DocumentElement == null)
				return null;

			XmlNodeList viewNodes = xmlDBObjectsDoc.DocumentElement.SelectNodes(XML_TAG_VIEWS + "/" + XML_TAG_VIEW);
			if (viewNodes == null)
				return null;

			SqlViewList viewsList = new SqlViewList();

			foreach (XmlElement viewNode in viewNodes)
			{
				string viewName = viewNode.GetAttribute(XML_NAME_ATTRIBUTE);
				if (String.IsNullOrEmpty(viewName))
					continue;

				SqlView sv = new SqlView(viewName);
				sv.TbNameSpace = viewNode.GetAttribute(XML_TB_NAMESPACE_ATTRIBUTE);
				//statement in sezione cdata
				XmlElement sqlNode = viewNode.SelectSingleNode(XML_TAG_SQLSCRIPT) as XmlElement;
				if (sqlNode != null)
					sv.SqlDefinition = sqlNode.InnerText;
				XmlElement oraNode = viewNode.SelectSingleNode(XML_TAG_ORACLESCRIPT) as XmlElement;
				if (oraNode != null)
					sv.OracleDefinition = oraNode.InnerText;

				if (viewNode.HasAttribute(XML_DB_REL_NUMBER_ATTRIBUTE))
				{
					string creationDbReleaseNumberText = viewNode.GetAttribute(XML_DB_REL_NUMBER_ATTRIBUTE);
					if (!string.IsNullOrEmpty(creationDbReleaseNumberText))
					{
						try
						{
							sv.CreationDbReleaseNumber = Convert.ToInt32(creationDbReleaseNumberText);
						}
						catch (FormatException)
						{
						}
						catch (OverflowException)
						{
						}
					}
				}

				ParseViewColumnsNode(sv, viewNode); // @@ MADE BY CARLOTTA

				viewsList.Add(sv);
			}

			return viewsList;
		}

		//---------------------------------------------------------------------------
		public SqlViewList ParseViewsInfoNode(string path)
		{
			if (String.IsNullOrEmpty(path) || !File.Exists(path))
				return null;

			XmlDocument xmlDBObjectsDoc = new XmlDocument();

			try
			{
				xmlDBObjectsDoc.Load(path);
				return ParseViewsInfoNode(xmlDBObjectsDoc);
			}
			catch
			{
				return null;
			}
		}

		//---------------------------------------------------------------------------
		// @@ MADE BY CARLOTTA 
		//---------------------------------------------------------------------------
		public bool ParseViewColumnsNode(SqlView aViewInfo, XmlElement aViewNode)
		{
			if (aViewInfo == null || aViewNode == null || String.Compare(aViewNode.Name, XML_TAG_VIEW) != 0)
				return false;

			try
			{
				XmlNode columnsInfoNode = aViewNode.SelectSingleNode("child::" + XML_TAG_COLUMNS);
				if (columnsInfoNode == null || !(columnsInfoNode is XmlElement) || !columnsInfoNode.HasChildNodes)
					return false;

				XmlNodeList columnsList = columnsInfoNode.SelectNodes("child::" + XML_TAG_COLUMN);
				if (columnsList == null || columnsList.Count == 0)
					return false;

				foreach (XmlNode columnInfoNode in columnsList)
					ParseViewColumnNode(aViewInfo, columnInfoNode as XmlElement);

				return true;
			}
			catch (XPathException exception)
			{
				throw new TBWizardException(TBWizardProjectsStrings.ExceptionRaisedDuringLoadErrMsg, exception);
			}
		}

		//---------------------------------------------------------------------------
		// @@ MADE BY CARLOTTA 
		//---------------------------------------------------------------------------
		public bool ParseViewColumnNode(SqlView aViewInfo, XmlElement aViewColumnNode)
		{
			if (aViewInfo == null || aViewColumnNode == null || String.Compare(aViewColumnNode.Name, XML_TAG_COLUMN) != 0)
				return false;

			try
			{
				string columnName = aViewColumnNode.GetAttribute(XML_NAME_ATTRIBUTE);
				if (String.IsNullOrEmpty(columnName))
					return false;

				string columnDataType = aViewColumnNode.GetAttribute(XML_COLUMN_DATATYPE_ATTRIBUTE);
				if (String.IsNullOrEmpty(columnDataType))
					return false;

				uint columnLength = 0;
				if (aViewColumnNode.HasAttribute(XML_COLUMN_LENGTH_ATTRIBUTE))
				{
					string columnLengthText = aViewColumnNode.GetAttribute(XML_COLUMN_LENGTH_ATTRIBUTE);
					if (!string.IsNullOrEmpty(columnLengthText))
					{
						try
						{
							columnLength = Convert.ToUInt32(columnLengthText);
						}
						catch (FormatException)
						{
						}
						catch (OverflowException)
						{
						}
					}
				}

				ViewColumn parsedColumn = new ViewColumn(columnName, columnDataType, columnLength);

				if (aViewColumnNode.HasAttribute(XML_COLUMN_COLLATE_SENSITIVE_ATTRIBUTE))
				{
					string isCollateSensitiveText = aViewColumnNode.GetAttribute(XML_COLUMN_COLLATE_SENSITIVE_ATTRIBUTE);
					if (!string.IsNullOrEmpty(isCollateSensitiveText))
					{
						try
						{
							parsedColumn.IsCollateSensitive = Convert.ToBoolean(isCollateSensitiveText);
						}
						catch (FormatException)
						{
						}
					}
				}

				aViewInfo.Columns.Add(parsedColumn);

				return true;
			}
			catch (XPathException exception)
			{
				throw new TBWizardException(TBWizardProjectsStrings.ExceptionRaisedDuringLoadErrMsg, exception);
			}
		}

		///<summary>
		/// Scrive nodi di tipo Views
		///</summary>
		//---------------------------------------------------------------------------
		public void AddViewNodes(SqlView aSqlView)
		{
			if (aSqlView == null)
				return;

			InitProject();

			XmlElement aViewsElement = projectDocument.DocumentElement.SelectSingleNode("child::" + XML_TAG_VIEWS) as XmlElement;
			if (aViewsElement == null)
			{
				aViewsElement = projectDocument.CreateElement(XML_TAG_VIEWS);
				projectDocument.DocumentElement.AppendChild(aViewsElement);
			}

			XmlElement aViewElement = aViewsElement.SelectSingleNode("child::" + XML_TAG_VIEW + "[@" + XML_NAME_ATTRIBUTE + "='" + aSqlView.Name + "']") as XmlElement;
			if (aViewElement == null)
			{
				aViewElement = projectDocument.CreateElement(XML_TAG_VIEW);
				aViewsElement.AppendChild(aViewElement);
			}

			SetNodeViewInfo(aViewElement, aSqlView);
		}

		///<summary>
		/// Scrive nodi di tipo View
		///</summary>
		//---------------------------------------------------------------------------
		private void SetNodeViewInfo(XmlElement aViewElement, SqlView aSqlView)
		{
			if (projectDocument == null || aSqlView == null || aViewElement == null ||
				String.Compare(aViewElement.Name, XML_TAG_VIEW) != 0)
				return;

			aViewElement.RemoveAllAttributes();

			aViewElement.SetAttribute(XML_NAME_ATTRIBUTE, aSqlView.Name);
			aViewElement.SetAttribute(XML_LOCALIZE_ATTRIBUTE, aSqlView.Name);
			aViewElement.SetAttribute(XML_DB_REL_NUMBER_ATTRIBUTE, aSqlView.CreationDbReleaseNumber.ToString());
			aViewElement.SetAttribute(XML_TB_NAMESPACE_ATTRIBUTE, aSqlView.TbNameSpace);
			//statement in sezione cdata
			XmlElement sqlNode = projectDocument.CreateElement(XML_TAG_SQLSCRIPT);
			XmlCDataSection cdatasql = projectDocument.CreateCDataSection(aSqlView.SqlDefinition);
			sqlNode.AppendChild(cdatasql);
			aViewElement.AppendChild(sqlNode);

			XmlElement oraNode = projectDocument.CreateElement(XML_TAG_ORACLESCRIPT);
			XmlCDataSection cdataora = projectDocument.CreateCDataSection(aSqlView.OracleDefinition);
			oraNode.AppendChild(cdataora);
			aViewElement.AppendChild(oraNode);

			SetNodeViewColumnsInfo(aViewElement, aSqlView);
		}


		///<summary>
		/// Scrive nodi di tipo Columns come sottonodi alla View
		///</summary>
		//---------------------------------------------------------------------------
		private void SetNodeViewColumnsInfo(XmlElement aViewElement, SqlView aSqlView)
		{
			if (projectDocument == null || aSqlView == null || aViewElement == null ||
				String.Compare(aViewElement.Name, XML_TAG_VIEW) != 0)
				return;

			XmlNode columnsInfoNode = aViewElement.SelectSingleNode("child::" + XML_TAG_COLUMNS);

			XmlElement columnsElement = null;
			if (columnsInfoNode == null || !(columnsInfoNode is XmlElement))
			{
				columnsElement = projectDocument.CreateElement(XML_TAG_COLUMNS);
				aViewElement.AppendChild(columnsElement);
			}
			else
				columnsElement = (XmlElement)columnsInfoNode;

			columnsElement.RemoveAll();

			foreach (ViewColumn aColumnInfo in aSqlView.Columns)
			{
				XmlElement addedColumnElement = AddViewColumnInfoToColumnsNode(columnsElement, aColumnInfo);//, aTableInfo.CreationDbReleaseNumber);
				if (addedColumnElement == null)
					continue;
			}
		}

		///<summary>
		/// Scrive nodi di tipo Column come sottonodi alla View
		///</summary>
		//---------------------------------------------------------------------------
		private XmlElement AddViewColumnInfoToColumnsNode(XmlElement aColumnsElement, ViewColumn aColumnInfo)//, uint tableCreationDbReleaseNumber)
		{
			if (projectDocument == null ||
				aColumnInfo == null ||
				aColumnsElement == null ||
				String.Compare(aColumnsElement.Name, XML_TAG_COLUMNS) != 0)
				return null;

			XmlElement columnElement = aColumnsElement.SelectSingleNode("child::" + XML_TAG_COLUMN + "[@" + XML_NAME_ATTRIBUTE + "='" + aColumnInfo.Name + "']") as XmlElement;
			if (columnElement == null)
			{
				columnElement = projectDocument.CreateElement(XML_TAG_COLUMN);
				aColumnsElement.AppendChild(columnElement);
			}

			SetNodeViewColumnInfo(columnElement, aColumnInfo);

			return columnElement;
		}

		///<summary>
		/// Imposta gli attributi ai nodi di tipo Column (sottonodi alla View)
		///</summary>
		//---------------------------------------------------------------------------
		private void SetNodeViewColumnInfo(XmlElement aColumnElement, ViewColumn aColumnInfo)
		{
			if (projectDocument == null || aColumnInfo == null || aColumnElement == null ||
				String.Compare(aColumnElement.Name, XML_TAG_COLUMN) != 0)
				return;

			aColumnElement.RemoveAllAttributes();

			aColumnElement.SetAttribute(XML_NAME_ATTRIBUTE, aColumnInfo.Name);
			aColumnElement.SetAttribute(XML_LOCALIZE_ATTRIBUTE, aColumnInfo.Name);
			aColumnElement.SetAttribute(XML_COLUMN_DATATYPE_ATTRIBUTE, aColumnInfo.DataType);

			// l'attributo data_length lo mettiamo solo se si tratta di stringhe
			if (string.Compare(aColumnInfo.DataType, WizardTableColumnDataType.DataType.String.ToString(), StringComparison.InvariantCultureIgnoreCase) == 0 &&
				aColumnInfo.DataLength > 0)
				aColumnElement.SetAttribute(XML_COLUMN_LENGTH_ATTRIBUTE, aColumnInfo.DataLength.ToString(NumberFormatInfo.InvariantInfo));

			// nel caso in cui la colonna sia Enum
			if (string.Compare(aColumnInfo.DataType, WizardTableColumnDataType.DataType.Enum.ToString(), StringComparison.InvariantCultureIgnoreCase) == 0)
				aColumnElement.SetAttribute(XML_BASETYPE_ATTRIBUTE, aColumnInfo.TbEnum.ToString(NumberFormatInfo.InvariantInfo));

			// da gestire il collate sensitive!!!
			if (!aColumnInfo.IsCollateSensitive)
				aColumnElement.SetAttribute(XML_COLUMN_COLLATE_SENSITIVE_ATTRIBUTE, Boolean.FalseString.ToLower());
		}
		# endregion

		# region Parse e Unparse nodi di tipo Procedures

		///<summary>
		/// Lettura nodi di tipo Procedure (ritorna solo le definizioni con sintassi SQL e ORACLE)
		///</summary>
		//---------------------------------------------------------------------------
		public SqlProcedureList ParseProceduresInfoNode(XmlDocument xmlDBObjectsDoc)
		{
			if (xmlDBObjectsDoc == null)
				return null;

			XmlNodeList procNodes = xmlDBObjectsDoc.DocumentElement.SelectNodes(XML_TAG_PROCEDURES + "/" + XML_TAG_PROCEDURE);
			if (procNodes == null)
				return null;

			SqlProcedureList procList = new SqlProcedureList();

			foreach (XmlElement procNode in procNodes)
			{
				string procName = procNode.GetAttribute(XML_NAME_ATTRIBUTE);
				if (String.IsNullOrEmpty(procName))
					continue;

				SqlProcedure sp = new SqlProcedure(procName);
				sp.TbNameSpace = procNode.GetAttribute(XML_TB_NAMESPACE_ATTRIBUTE);
				//statement in sezione cdata
				XmlElement sqlNode = procNode.SelectSingleNode(XML_TAG_SQLSCRIPT) as XmlElement;
				if (sqlNode != null)
					sp.SqlDefinition = sqlNode.InnerText;
				XmlElement oraNode = procNode.SelectSingleNode(XML_TAG_ORACLESCRIPT) as XmlElement;
				if (oraNode != null)
					sp.OracleDefinition = oraNode.InnerText;

				if (procNode.HasAttribute(XML_DB_REL_NUMBER_ATTRIBUTE))
				{
					string creationDbReleaseNumberText = procNode.GetAttribute(XML_DB_REL_NUMBER_ATTRIBUTE);
					if (!string.IsNullOrEmpty(creationDbReleaseNumberText))
					{
						try
						{
							sp.CreationDbReleaseNumber = Convert.ToInt32(creationDbReleaseNumberText);
						}
						catch (FormatException)
						{
						}
						catch (OverflowException)
						{
						}
					}
				}

				ParseProcedureParametersNode(sp, procNode); // @@ MADE BY CARLOTTA 

				procList.Add(sp);
			}

			return procList;
		}

		//---------------------------------------------------------------------------
		public SqlProcedureList ParseProceduresInfoNode(string path)
		{
			if (String.IsNullOrEmpty(path) || !File.Exists(path))
				return null;

			XmlDocument xmlDBObjectsDoc = new XmlDocument();

			try
			{
				xmlDBObjectsDoc.Load(path);
				return ParseProceduresInfoNode(xmlDBObjectsDoc);
			}
			catch
			{
				return null;
			}
		}

		//---------------------------------------------------------------------------
		// @@ MADE BY CARLOTTA 
		//---------------------------------------------------------------------------
		public bool ParseProcedureParametersNode(SqlProcedure aProcedureInfo, XmlElement aProcedureNode)
		{
			if (aProcedureInfo == null || aProcedureNode == null || String.Compare(aProcedureNode.Name, XML_TAG_PROCEDURE) != 0)
				return false;

			try
			{
				XmlNode parametersInfoNode = aProcedureNode.SelectSingleNode("child::" + XML_TAG_PARAMETERS);
				if (parametersInfoNode == null || !(parametersInfoNode is XmlElement) || !parametersInfoNode.HasChildNodes)
					return false;

				XmlNodeList parametersList = parametersInfoNode.SelectNodes("child::" + XML_TAG_PARAMETER);
				if (parametersList == null || parametersList.Count == 0)
					return false;

				foreach (XmlNode parameterInfoNode in parametersList)
					ParseProcedureParameterNode(aProcedureInfo, parameterInfoNode as XmlElement);

				return true;
			}
			catch (XPathException exception)
			{
				throw new TBWizardException(TBWizardProjectsStrings.ExceptionRaisedDuringLoadErrMsg, exception);
			}
		}

		//---------------------------------------------------------------------------
		// @@ MADE BY CARLOTTA 
		//---------------------------------------------------------------------------
		public bool ParseProcedureParameterNode(SqlProcedure aProcedureInfo, XmlElement aProcedureParameterNode)
		{
			if (aProcedureInfo == null || aProcedureParameterNode == null || String.Compare(aProcedureParameterNode.Name, XML_TAG_PARAMETER) != 0)
				return false;

			try
			{
				string parameterName = aProcedureParameterNode.GetAttribute(XML_NAME_ATTRIBUTE);
				if (String.IsNullOrEmpty(parameterName))
					return false;

				string paramDataType = aProcedureParameterNode.GetAttribute(XML_COLUMN_DATATYPE_ATTRIBUTE);
				if (String.IsNullOrEmpty(paramDataType))
					return false;

				uint paramDataLength = 0;
				if (aProcedureParameterNode.HasAttribute(XML_COLUMN_LENGTH_ATTRIBUTE))
				{
					string paramDataLengthText = aProcedureParameterNode.GetAttribute(XML_COLUMN_LENGTH_ATTRIBUTE);
					if (!string.IsNullOrEmpty(paramDataLengthText))
					{
						try
						{
							paramDataLength = Convert.ToUInt32(paramDataLengthText);
						}
						catch (FormatException)
						{
						}
						catch (OverflowException)
						{
						}
					}
				}
				bool paramIsOut = false;
				if (aProcedureParameterNode.HasAttribute(XML_OUT_PARAMETER_ATTRIBUTE))
				{
					string isOutText = aProcedureParameterNode.GetAttribute(XML_OUT_PARAMETER_ATTRIBUTE);
					if (!string.IsNullOrEmpty(isOutText))
					{
						try
						{
							paramIsOut = Convert.ToBoolean(isOutText);
						}
						catch (FormatException)
						{
						}
					}
				}

				ProcedureParameter parsedParameter = new ProcedureParameter(parameterName, paramDataType, paramDataLength, paramIsOut);

				if (aProcedureParameterNode.HasAttribute(XML_COLUMN_COLLATE_SENSITIVE_ATTRIBUTE))
				{
					string isCollateSensitiveText = aProcedureParameterNode.GetAttribute(XML_COLUMN_COLLATE_SENSITIVE_ATTRIBUTE);
					if (!string.IsNullOrEmpty(isCollateSensitiveText))
					{
						try
						{
							parsedParameter.IsCollateSensitive = Convert.ToBoolean(isCollateSensitiveText);
						}
						catch (FormatException)
						{
						}
					}
				}

				aProcedureInfo.Parameters.Add(parsedParameter);

				return true;
			}
			catch (XPathException exception)
			{
				throw new TBWizardException(TBWizardProjectsStrings.ExceptionRaisedDuringLoadErrMsg, exception);
			}
		}

		///<summary>
		/// Scrive nodi di tipo Procedures
		///</summary>
		//---------------------------------------------------------------------------
		public void AddProcedureNodes(SqlProcedure aSqlProcedure)
		{
			if (aSqlProcedure == null)
				return;

			InitProject();

			XmlElement aProceduresElement = projectDocument.DocumentElement.SelectSingleNode("child::" + XML_TAG_PROCEDURES) as XmlElement;
			if (aProceduresElement == null)
			{
				aProceduresElement = projectDocument.CreateElement(XML_TAG_PROCEDURES);
				projectDocument.DocumentElement.AppendChild(aProceduresElement);
			}

			XmlElement aProcedureElement = aProceduresElement.SelectSingleNode("child::" + XML_TAG_PROCEDURE
				+ "[@" + XML_NAME_ATTRIBUTE + "='" + aSqlProcedure.Name + "']") as XmlElement;
			if (aProcedureElement == null)
			{
				aProcedureElement = projectDocument.CreateElement(XML_TAG_PROCEDURE);
				aProceduresElement.AppendChild(aProcedureElement);
			}

			SetNodeProcedureInfo(aProcedureElement, aSqlProcedure);
		}

		///<summary>
		/// Scrive nodi di tipo Procedure
		///</summary>
		//---------------------------------------------------------------------------
		private void SetNodeProcedureInfo(XmlElement aProcedureElement, SqlProcedure aSqlProcedure)
		{
			if (projectDocument == null || aSqlProcedure == null || aProcedureElement == null ||
				String.Compare(aProcedureElement.Name, XML_TAG_PROCEDURE) != 0)
				return;

			aProcedureElement.RemoveAllAttributes();

			aProcedureElement.SetAttribute(XML_NAME_ATTRIBUTE, aSqlProcedure.Name);
			aProcedureElement.SetAttribute(XML_LOCALIZE_ATTRIBUTE, aSqlProcedure.Name);
			aProcedureElement.SetAttribute(XML_DB_REL_NUMBER_ATTRIBUTE, aSqlProcedure.CreationDbReleaseNumber.ToString());
			aProcedureElement.SetAttribute(XML_TB_NAMESPACE_ATTRIBUTE, aSqlProcedure.TbNameSpace);
			//statement in sezione cdata
			XmlElement sqlNode = projectDocument.CreateElement(XML_TAG_SQLSCRIPT);
			XmlCDataSection cdatasql = projectDocument.CreateCDataSection(aSqlProcedure.SqlDefinition);
			sqlNode.AppendChild(cdatasql);
			aProcedureElement.AppendChild(sqlNode);

			XmlElement oraNode = projectDocument.CreateElement(XML_TAG_ORACLESCRIPT);
			XmlCDataSection cdataora = projectDocument.CreateCDataSection(aSqlProcedure.OracleDefinition);
			oraNode.AppendChild(cdataora);
			aProcedureElement.AppendChild(oraNode);

			SetNodeProcedureParametersInfo(aProcedureElement, aSqlProcedure);
		}

		///<summary>
		/// Scrive nodi di tipo Parameters come sottonodi alla Procedure
		///</summary>
		//---------------------------------------------------------------------------
		private void SetNodeProcedureParametersInfo(XmlElement aProcedureElement, SqlProcedure aSqlProcedure)
		{
			if (projectDocument == null || aSqlProcedure == null || aProcedureElement == null ||
				String.Compare(aProcedureElement.Name, XML_TAG_PROCEDURE) != 0)
				return;

			XmlElement parametersElement = aProcedureElement.SelectSingleNode("child::" + XML_TAG_PARAMETERS) as XmlElement;
			if (parametersElement == null)
			{
				parametersElement = projectDocument.CreateElement(XML_TAG_PARAMETERS);
				aProcedureElement.AppendChild(parametersElement);
			}
			else
				parametersElement.RemoveAll();

			foreach (ProcedureParameter aParameterInfo in aSqlProcedure.Parameters)
			{
				XmlElement addedParameterElement = AddProcedureParamInfoToParametersNode(parametersElement, aParameterInfo);//, aTableInfo.CreationDbReleaseNumber);
				if (addedParameterElement == null)
					continue;
			}
		}

		///<summary>
		/// Scrive nodi di tipo Parameter come sottonodi alla Procedure
		///</summary>
		//---------------------------------------------------------------------------
		private XmlElement AddProcedureParamInfoToParametersNode(XmlElement aParametersElement, ProcedureParameter aColumnInfo)//, uint tableCreationDbReleaseNumber)
		{
			if (projectDocument == null || aColumnInfo == null ||
				aParametersElement == null || String.Compare(aParametersElement.Name, XML_TAG_PARAMETERS) != 0)
				return null;

			XmlElement parameterElement = aParametersElement.SelectSingleNode("child::" + XML_TAG_PARAMETER + "[@" + XML_NAME_ATTRIBUTE + "='" + aColumnInfo.Name + "']") as XmlElement;

			if (parameterElement == null)
			{
				parameterElement = projectDocument.CreateElement(XML_TAG_PARAMETER);
				aParametersElement.AppendChild(parameterElement);
			}

			SetNodeProcedureParameterInfo(parameterElement, aColumnInfo);//, tableCreationDbReleaseNumber);

			return parameterElement;
		}

		///<summary>
		/// Imposta gli attributi ai nodi di tipo Parameter (sottonodi alla Procedure)
		///</summary>
		//---------------------------------------------------------------------------
		private void SetNodeProcedureParameterInfo(XmlElement aProcedureParameterElement, ProcedureParameter aParameterInfo)//, uint tableCreationDbReleaseNumber)
		{
			if (projectDocument == null || aParameterInfo == null || aProcedureParameterElement == null ||
				String.Compare(aProcedureParameterElement.Name, XML_TAG_PARAMETER) != 0)
				return;

			aProcedureParameterElement.RemoveAllAttributes();

			aProcedureParameterElement.SetAttribute(XML_NAME_ATTRIBUTE, aParameterInfo.Name);
			aProcedureParameterElement.SetAttribute(XML_LOCALIZE_ATTRIBUTE, aParameterInfo.Name);
			aProcedureParameterElement.SetAttribute(XML_COLUMN_DATATYPE_ATTRIBUTE, aParameterInfo.DataType);

			// l'attributo data_length lo mettiamo solo se si tratta di stringhe
			if (string.Compare(aParameterInfo.DataType, WizardTableColumnDataType.DataType.String.ToString(), StringComparison.InvariantCultureIgnoreCase) == 0 &&
				aParameterInfo.DataLength > 0)
				aProcedureParameterElement.SetAttribute(XML_COLUMN_LENGTH_ATTRIBUTE, aParameterInfo.DataLength.ToString(NumberFormatInfo.InvariantInfo));

			// nel caso in cui la colonna sia Enum
			if (string.Compare(aParameterInfo.DataType, WizardTableColumnDataType.DataType.Enum.ToString(), StringComparison.InvariantCultureIgnoreCase) == 0)
				aProcedureParameterElement.SetAttribute(XML_BASETYPE_ATTRIBUTE, aParameterInfo.TbEnum.ToString(NumberFormatInfo.InvariantInfo));

			aProcedureParameterElement.SetAttribute(XML_OUT_PARAMETER_ATTRIBUTE, (aParameterInfo.IsOut) ? Boolean.TrueString.ToLower() : Boolean.FalseString.ToLower());

			// da gestire il collate sensitive!!!
			if (!aParameterInfo.IsCollateSensitive)
				aProcedureParameterElement.SetAttribute(XML_COLUMN_COLLATE_SENSITIVE_ATTRIBUTE, Boolean.FalseString.ToLower());
		}
		# endregion

		# region Parse nodi di tipo TBAfterScript
		//---------------------------------------------------------------------------
		public IList<TBAfterScript> ParseTBAfterScript(string path)
		{
			if (String.IsNullOrEmpty(path) || !File.Exists(path))
				return null;

			XmlDocument xmlDBObjectsDoc = new XmlDocument();

			try
			{
				xmlDBObjectsDoc.Load(path);
				return ParseTBAfterScript(xmlDBObjectsDoc);
			}
			catch
			{
				return null;
			}
		}

		///<summary>
		/// Parse nodi di tipo TBAfterScript
		///</summary>
		//---------------------------------------------------------------------------
		public IList<TBAfterScript> ParseTBAfterScript(XmlDocument xmlDBObjectsDoc)
		{
			if (xmlDBObjectsDoc == null || xmlDBObjectsDoc.DocumentElement == null)
				return null;

			XmlNodeList afterNodes =
				xmlDBObjectsDoc.DocumentElement.SelectNodes(XML_TAG_TBAFTERSCRIPTS + "/" + XML_TAG_TBAFTERSCRIPT);
			if (afterNodes == null)
				return null;

			IList<TBAfterScript> afterScripts = new List<TBAfterScript>();
			foreach (XmlElement an in afterNodes)
			{
				int step = -1;
				int.TryParse(an.GetAttribute(XML_STEP_ATTRIBUTE), out step);

				string sqlValue = string.Empty, oracleValue = string.Empty;

				XmlNode sqlDbmsNode = an.SelectSingleNode(XML_TAG_SQLSCRIPT);
				if (sqlDbmsNode != null)
					sqlValue = sqlDbmsNode.InnerText;

				XmlNode oraDbmsNode = an.SelectSingleNode(XML_TAG_ORACLESCRIPT);
				if (oraDbmsNode != null)
					oracleValue = oraDbmsNode.InnerText;

				TBAfterScript afterScript = new TBAfterScript(step, sqlValue, oracleValue);

				afterScripts.Add(afterScript);
			}

			return afterScripts;
		}

		# endregion

		//---------------------------------------------------------------------------
		protected override void WriteReadOnly(XmlElement aColumnElement)
		{
			return;
		}
		//---------------------------------------------------------------------------
		protected override void WriteClassName(XmlElement aElement, string aValue)
		{
			return;
		}
	}
}
