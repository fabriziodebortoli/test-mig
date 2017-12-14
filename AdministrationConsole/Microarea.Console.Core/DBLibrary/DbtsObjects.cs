using System;
using System.Collections;
using System.IO;
using System.Xml;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Core.StringLoader;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.Console.Core.DBLibrary
{
	///<summary>
	/// Mini-parser ad-hoc per i file Dbts.xml
	/// (ad uso e consumo dell'Auditing e del RowSecurity)
	///</summary>
	//=========================================================================
	public class DbtsObjects
	{
		private string filePath = string.Empty;
		private FixedColumnsObject fixedColumnsObject;
		private string dbtMasterTable;

		// Properties
		public FixedColumnsObject FixedColumns { get { return fixedColumnsObject; } }
		public string DBTMasterTable { get { return dbtMasterTable; } }

		///<summary>
		/// constructor
		///</summary>
		//---------------------------------------------------------------------
		public DbtsObjects(string aFilePath)
		{
			if (string.IsNullOrWhiteSpace(aFilePath))
				return;
			filePath = aFilePath;
		}

		///<summary>
		/// Utilizzato dall'Auditing per individuare le colonne dichiarate nel file
		///</summary>
		//---------------------------------------------------------------------
		public bool ParseFixedColumns()
		{
			if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
				return false;

			try
			{
				XmlDocument dom = new XmlDocument();
				dom.Load(filePath);

				// cerca con XPath solo le funzioni con un dato nome per poi selezionare quella con i parametri giusti
				XmlNode root = dom.DocumentElement;
				if (root == null)
					return false;

				string xpath = string.Format
						(
						"/{0}/{1}/{2}",
						DBTSXML.Element.DBTs,
						DBTSXML.Element.Master,
						DBTSXML.Element.Table
						);

				XmlNodeList table = root.SelectNodes(xpath);
				if (table == null)
					return false;

				string tableName = ((XmlElement)table[0]).InnerText;

				fixedColumnsObject = new FixedColumnsObject(tableName);

				xpath = string.Format
					(
					"/{0}/{1}/{2}/{3}/{4}",
					DBTSXML.Element.DBTs,
					DBTSXML.Element.Master,
					DBTSXML.Element.UniversalKeys,
					DBTSXML.Element.UniversalKey,
					DBTSXML.Element.Segment
					);

				XmlNodeList tracedColumnsNodes = root.SelectNodes(xpath);
				if (tracedColumnsNodes == null)
					return false;

				string name = string.Empty;
				foreach (XmlElement tracedColumn in tracedColumnsNodes)
				{
					name = tracedColumn.GetAttributeNode(DBTSXML.Attribute.Name).InnerText;
					if (!fixedColumnsObject.ExistTracedColumn(name))
						fixedColumnsObject.TracedColumns.Add(name);
				}

				//FixedKeys
				xpath = string.Format
					(
					"/{0}/{1}/{2}/{3}",
					DBTSXML.Element.DBTs,
					DBTSXML.Element.Master,
					DBTSXML.Element.FixedKeys,
					DBTSXML.Element.Segment
					);

				tracedColumnsNodes = root.SelectNodes(xpath);
				if (tracedColumnsNodes == null)
					return false;

				name = string.Empty;
				foreach (XmlElement tracedColumn in tracedColumnsNodes)
				{
					name = tracedColumn.GetAttributeNode(DBTSXML.Attribute.Name).Value;
					if (!fixedColumnsObject.ExistTracedColumn(name))
						fixedColumnsObject.TracedColumns.Add(name);
				}
			}
			catch (XmlException)
			{
				return false;
			}
			catch (Exception)
			{
				return false;
			}

			return true;
		}

		///<summary>
		/// Utilizzato dal RowSecurity per individuare il documento DBTMaster legato al nome
		/// di una tabella
		///</summary>
		//---------------------------------------------------------------------
		public bool ParseMasterTable()
		{
			if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
				return false;

			try
			{
				XmlDocument dom = new XmlDocument();
				dom.Load(filePath);
				
				// cerca con XPath solo le funzioni con un dato nome per poi selezionare quella con i parametri giusti
				XmlNode root = dom.DocumentElement;
				if (root == null)
					return false;

				string xpath = string.Format
					(
					"/{0}/{1}/{2}",
					DBTSXML.Element.DBTs,
					DBTSXML.Element.Master,
					DBTSXML.Element.Table
					);

				XmlNodeList table = root.SelectNodes(xpath);
				if (table == null)
					return false;

				dbtMasterTable = ((XmlElement)table[0]).InnerText;
			}
			catch (XmlException)
			{
				return false;
			}
			catch (Exception)
			{
				return false;
			}

			return true;
		}
	}

	//=========================================================================
	public class FixedColumnsObject
	{
		private string		tableName;
		private ArrayList   tracedColumns;

		// Properties
		public	ArrayList	TracedColumns	{ get { return tracedColumns; } }
		public  string		TableName		{ get { return tableName; } }
	
		//---------------------------------------------------------------------
		public FixedColumnsObject(string aTableName)
		{
			tableName = aTableName;
			
			if (tracedColumns == null)
				tracedColumns = new ArrayList();
		}

		//---------------------------------------------------------------------
		public bool ExistTracedColumn(string newName)
		{
			foreach (string name in tracedColumns)
				if (name == newName)
					return true;
			return false;
		}
	}
}
