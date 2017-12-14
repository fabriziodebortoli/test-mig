using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Xml;

namespace Microarea.Library.SqlScriptUtility
{
	//============================================================
	public enum DBObjectType { Table, View, Procedure }

	///<summary>
	/// Classe che parsa il file SqlRecordStructures.xml
	/// Contiene tutte le informazioni degli oggetti di database registrati dalle dll
	/// delle Applications e relative colonne, nonchè tipi di dato, tag enumerativo ed
	/// eventuale dll di AddOnField.
	/// La sintassi è la seguente:
	/// <SqlRecordStructures>
	/// 	<Object name="MA_PaymentTerms" type="table/view/proc">
	/// 	<Element name="Payment" tb_type="string"></Element>
	/// 	<Element name="NoOfInstallments" tb_type="integer"></Element>
	/// 	<Element name="FixedDayRounding" tb_type="enum" tb_enum="43"></Element>
	/// 	<Element name="BusinessYear" tb_type="bool"></Element>
	///		<Element name="CollectionCharges" tb_type="money"></Element>
	/// 	<Element name="Discount1" tb_type="percent"></Element>
	/// 	<Element name="TBGuid" tb_type="uuid"></Element>
	/// 	<Element name="SaleOrdId" tb_type="long"></Element>
	/// 	<Element name="EstimatedUseDate" tb_type="date"></Element>
	/// 	<Element name="DNQuantity" tb_type="quantity"></Element>
	/// 	<Element name="Notes" tb_type="text"></Element>
	/// 	<Element name="AuthorCode" tb_type="string" tb_addonlibrary_namespace="ERP.PublishingTax.AddOnsSales"></Element>
	/// </SqlRecordStructures>
	///</summary>
	//============================================================
	public class SqlRecordStructuresInfo
	{
		#region Tag and Attributes
		
		public const string XML_SQLRECSTRUCT_TAG= "SqlRecordStructures";
		public const string XML_OBJECT_TAG	= "Object";
		public const string XML_ELEMENT_TAG = "Element";

		public const string XML_NAME_ATTR		= "name";
		public const string XML_TYPE_ATTR		= "type";
		public const string XML_TBTYPE_ATTR		= "tb_type";
		public const string XML_TBENUM_ATTR		= "tb_enum";
		public const string XML_TBADDONLIB_ATTR = "tb_addonlibrary_namespace";

		public const string TABLE_ELEMENT	= "table";
		public const string VIEW_ELEMENT	= "view";
		public const string PROC_ELEMENT	= "proc";
		
		#endregion

		# region Private variables

		private string filePath;
		private string parsingError;

		private Dictionary<string, DBObjectInfo> dbObjects = new Dictionary<string, DBObjectInfo>();

		# endregion

		// Properties
		//---------------------------------------------------------------------
		public Dictionary<string, DBObjectInfo> DBObjects { get { return dbObjects; } }

		///<summary>
		/// Constructor
		///</summary>
		//---------------------------------------------------------------------
		public SqlRecordStructuresInfo(string filePath)
		{
			if (String.IsNullOrEmpty(filePath))
				Debug.Fail("Error in SqlRecordStructuresInfo");

			this.filePath = filePath;
			parsingError = string.Empty;
		}

		# region Parse methods
		///<summary>
		/// Funzione principale di Parse
		///</summary>
		//---------------------------------------------------------------------
		public bool Parse()
		{
			bool success = false;

			if (!File.Exists(filePath))
			{
				Debug.Fail("Il file specificato non esiste!");
				return success;
			}

			XmlDocument myDom = new XmlDocument();

			try
			{ 
				// loading file
				myDom.Load(filePath);

				// root document
				XmlElement root = myDom.DocumentElement;
				if (string.Compare(root.Name, XML_SQLRECSTRUCT_TAG, StringComparison.InvariantCultureIgnoreCase) != 0)
					return success;

				success = ParseObjects(root);
			}
			catch (XmlException err)
			{
				Debug.Fail(err.Message);
				parsingError = err.Message;
				return success;
			}
			catch (Exception e)
			{
				Debug.Fail(e.Message);
				parsingError = e.Message;
				return success;
			}

			return success;
		}

		///<summary>
		/// Parso i singoli nodi <Object>
		///</summary>
		//---------------------------------------------------------------------
		private bool ParseObjects(XmlElement root)
		{
			if (root == null)
				return false;

			//cerco il tag Object
			XmlNodeList objectElems = root.GetElementsByTagName(XML_OBJECT_TAG);

			if (objectElems == null || objectElems.Count == 0)
				return false;

			// per ogni tag <Object> 
			foreach (XmlElement xObject in objectElems)
			{
				string name = xObject.GetAttribute(XML_NAME_ATTR);
				string type = xObject.GetAttribute(XML_TYPE_ATTR);
				
				if (string.IsNullOrEmpty(name))
					continue;
				
				DBObjectInfo myDbObject = null;

				switch (type)
				{
					case TABLE_ELEMENT:
						myDbObject = new DBObjectInfo(DBObjectType.Table);
						break;

					case VIEW_ELEMENT:
						myDbObject = new DBObjectInfo(DBObjectType.View);
						break;

					case PROC_ELEMENT:
						myDbObject = new DBObjectInfo(DBObjectType.Procedure);
						break;

					default:
						break;
				}

				// mi è arrivato un tipo non riconosciuto
				if (myDbObject == null)
					continue;

				//cerco il tag <Element>
				XmlNodeList elements = xObject.GetElementsByTagName(XML_ELEMENT_TAG);
				if (elements == null || elements.Count == 0)
					continue;

				// parso ogni singolo tag <Element>
				ParseElements(elements, myDbObject);

				dbObjects.Add(name, myDbObject);
			}

			return true;
		}

		///<summary>
		/// Parso i singoli nodi <Element>
		///</summary>
		//---------------------------------------------------------------------
		private void ParseElements(XmlNodeList elements, DBObjectInfo myDbObject)
		{
			// Per ogni nodo di tipo <Element> scrivo gli attributi
			foreach (XmlElement xElem in elements)
			{
				string colName = xElem.GetAttribute(XML_NAME_ATTR);
				string tbType = xElem.GetAttribute(XML_TBTYPE_ATTR);

				if (string.IsNullOrEmpty(colName) || string.IsNullOrEmpty(tbType))
					continue;

				ColumnInfo cInfo = new ColumnInfo(tbType);

				string tbEnum = xElem.GetAttribute(XML_TBENUM_ATTR);
				if (!string.IsNullOrEmpty(tbEnum))
					cInfo.TbEnum = Convert.ToUInt32(tbEnum);

				string tbAddOn = xElem.GetAttribute(XML_TBADDONLIB_ATTR);
				if (!string.IsNullOrEmpty(tbAddOn))
					cInfo.Tb_AddOnLibrary = tbAddOn;

				myDbObject.Elements.Add(colName, cInfo);
			}
		}
		# endregion

		# region Get methods
		//---------------------------------------------------------------------
		public DBObjectInfo GetObjectInfo(string dbObject)
		{
			DBObjectInfo currentObject;

			if (dbObjects.TryGetValue(dbObject, out currentObject))
				return currentObject;

			return null;
		}

		//---------------------------------------------------------------------
		public ColumnInfo GetColumnInfo(string dbObject, string element)
		{
			DBObjectInfo currentObject;
			ColumnInfo currentColumn;

			if (dbObjects.TryGetValue(dbObject, out currentObject))
			{
				if (currentObject != null &&
					currentObject.Elements.TryGetValue(element, out currentColumn))
					return currentColumn;				
			}

			return null;
		}
		# endregion
	}

	///<summary>
	/// Elenco degli oggetti dichiarati nel file 
	///</summary>
	//============================================================
	public class DBObjectInfo
	{
		private DBObjectType type = DBObjectType.Table;
		private Dictionary<string, ColumnInfo> elements = new Dictionary<string, ColumnInfo>();

		//---------------------------------------------------------------------
		public DBObjectType Type { get { return type; } }
		public Dictionary<string, ColumnInfo> Elements { get { return elements; } }

		//---------------------------------------------------------------------
		public DBObjectInfo(DBObjectType type)
		{
			this.type = type;
		}
	}

	///<summary>
	/// Elenco degli elementi (colonne) contenuti in un oggetto
	///</summary>
	//============================================================
	public class ColumnInfo
	{
		private string tbType = String.Empty;
		public uint TbEnum = 0;
		public string Tb_AddOnLibrary = String.Empty;

		//---------------------------------------------------------------------
		public string TbType { get { return tbType; } }

		//---------------------------------------------------------------------
		public ColumnInfo(string tbType)
		{
			this.tbType = tbType;
		}
	}
}
