using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Microarea.TaskBuilderNet.DataSynchroUtilities;

namespace Microarea.TaskBuilderNet.DataSynchroProviders
{
	///<summary>
	/// Classe che si occupa di parsare il contenuto xml memorizzato dal CDNotification
	/// nella colonna ActionData della tabella DS_ActionsLog
	///</summary>
	//================================================================================
	internal class ActionDataParser
	{
		///<summary>
		/// A seconda del tipo azione richiamo il parse specifico
		/// Le informazioni dentro la ActionData mi servono solo in caso di Update/Delete
		///</summary>
		//--------------------------------------------------------------------------------
		public static ActionDataInfo ParseActionData(SynchroActionType actionType, string actionData)
		{
			ActionDataInfo adi = null;

			switch (actionType)
			{
				case SynchroActionType.NewAttachment:
				case SynchroActionType.NewCollection:
				case SynchroActionType.UpdateCollection:
				case SynchroActionType.UpdateProvider:
					adi = ParseDMSAction(actionData);
					break;
				case SynchroActionType.Insert:
					break;
				case SynchroActionType.Update:
					//adi = ParseUpdate(actionData);
					break;
				case SynchroActionType.Delete:
					adi = ParseDelete(actionData);
					break;
				case SynchroActionType.DeleteAttachment:
					adi = ParseDeleteAttachment(actionData);
					break;
				// TODO gestire le azioni per EA ed estrapolazione della chiave che ci serve nelle query
			}

			return adi;
		}

		/// <summary>
		/// Estrae la chiave per DMS
		/// </summary>
		/// <param name="actionData"></param>
		/// <returns></returns>
		//--------------------------------------------------------------------------------
		private static ActionDataInfo ParseDMSAction(string actionData)
		{
			ActionDataInfo ati = new ActionDataInfo();
			XDocument xDoc = null;

			try
			{
				xDoc = XDocument.Parse(actionData);
			}
			catch (Exception)
			{
				return ati;
			}

			if (xDoc == null)
				return ati;

			try
			{
				ati.DmsKey = xDoc.Element("Attachments").Element("AttachmentID").Value;
				return ati;
			}
			catch { }

			try
			{
				ati.DmsKey = xDoc.Element("Collections").Element("CollectionID").Value;
				return ati;
			}
			catch { }

			return ati;
		}

		///<summary>
		/// Update:
		/// <?xml version="1.0"?>
		/// <UpdatedDbtS> 
		///		<UpdatedDbt tablename="MA_CurrenciesFixing"> 
		///			<NewRows> 
		///				<Row Currency="AAA" ReferredCurrency="EUR" FixingDate="2014-03-03T00:00:00"/> 
		///			</NewRows> 
		///			<ModifiedRows>
		///				<Row Currency="BBB" ReferredCurrency="BB" />
		///				<Row Currency="CCC" ReferredCurrency="DOL" FixingDate="2014-05-03T00:00:00"/>
		///			</ModifiedRows>
		///			<DeletedRows>
		///				<Row Currency="DDD" ReferredCurrency="DER"/>
		///			</DeletedRows>
		///		</UpdatedDbt> 
		///		<UpdatedDbt tablename="MA_CurrenciesFixing2"> 
		///		....
		///		....
		///		</UpdatedDbt> 
		///	</UpdatedDbtS>
		///</summary>
		//--------------------------------------------------------------------------------
		private static ActionDataInfo ParseUpdate(string actionData)
		{
			try
			{
				XDocument xDoc = XDocument.Load(new StringReader(actionData));

				XElement xElemRoot = (XElement)xDoc.FirstNode;
				if (string.Compare(xElemRoot.Name.LocalName, DSActionDataXML.Element.UpdatedDbtS, StringComparison.InvariantCultureIgnoreCase) != 0)
					return null;

				// leggo tutti i nodi figli di tipo <UpdatedDbt>
				IEnumerable<XElement> dbts = from dbt in xElemRoot.Descendants()
											 where (string.Compare(dbt.Name.LocalName, DSActionDataXML.Element.UpdatedDbt, StringComparison.InvariantCultureIgnoreCase) == 0)
											 select dbt;

				if (dbts == null || dbts.Count() <= 0)
					return null;

				ActionDataInfo adi = new ActionDataInfo();

				foreach (XElement dbtElem in dbts)
					ParseUpdatedDbtElement(dbtElem, adi); // eseguo il parse dei figli <UpdatedDbt>

				return adi;
			}
			catch (Exception e)
			{
				throw new DSException("ActionDataParser.ParseUpdate", e.Message, actionData);
			}
		}

		/// <summary>
		/// Si occupa di parsare ogni nodo <UpdatedDbt> e relativi figli 
		/// </summary>
		//--------------------------------------------------------------------------------
		private static void ParseUpdatedDbtElement(XElement dbtElem, ActionDataInfo adi)
		{
			// leggo il primo attributo, che deve essere tablename
			XAttribute tblAttr = dbtElem.FirstAttribute;

			if (tblAttr == null || string.IsNullOrWhiteSpace(tblAttr.Value) ||
				string.Compare(tblAttr.Name.LocalName, DSActionDataXML.Attribute.Tablename, StringComparison.InvariantCultureIgnoreCase) != 0)
				return;

			UpdatedDbt uDbt = new UpdatedDbt(tblAttr.Value);
			adi.UpdatedDbts.Add(uDbt); // aggiungo il dbt

			// leggo tutti i nodi figli di tipo <NewRows>
			IEnumerable<XElement> newRowsElem = from nr in dbtElem.Descendants()
												where (string.Compare(nr.Name.LocalName, DSActionDataXML.Element.NewRows, StringComparison.InvariantCultureIgnoreCase) == 0)
												select nr;

			if (newRowsElem != null && newRowsElem.Count() > 0)
				ParseSingleRow(uDbt, newRowsElem, DSActionDataXML.Element.NewRows);

			// leggo tutti i nodi figli di tipo <ModifiedRows>
			IEnumerable<XElement> modifiedRowsElem = from mr in dbtElem.Descendants()
													 where (string.Compare(mr.Name.LocalName, DSActionDataXML.Element.ModifiedRows, StringComparison.InvariantCultureIgnoreCase) == 0)
													 select mr;

			if (modifiedRowsElem != null && modifiedRowsElem.Count() > 0)
				ParseSingleRow(uDbt, modifiedRowsElem, DSActionDataXML.Element.ModifiedRows);

			// leggo tutti i nodi figli di tipo <DeletedRows>
			IEnumerable<XElement> deletedRowsElem = from dr in dbtElem.Descendants()
													where (string.Compare(dr.Name.LocalName, DSActionDataXML.Element.DeletedRows, StringComparison.InvariantCultureIgnoreCase) == 0)
													select dr;

			if (deletedRowsElem != null && deletedRowsElem.Count() > 0)
				ParseSingleRow(uDbt, deletedRowsElem, DSActionDataXML.Element.DeletedRows);
		}

		/// <summary>
		/// Si occupa di parsare il nodo <Row> e tutti i suoi attributi e li mette nella lista del tipo di appartenenza
		/// </summary>
		//--------------------------------------------------------------------------------
		private static void ParseSingleRow(UpdatedDbt uDbt, IEnumerable<XElement> elements, string rowsType)
		{
			foreach (XElement row in elements.Descendants())
			{
				Row xRow = new Row();

				foreach (XAttribute attr in row.Attributes())
					xRow.Values.Add(attr.Name.LocalName, attr.Value);

				if (rowsType == DSActionDataXML.Element.NewRows)
					uDbt.NewRows.Add(xRow);
				else if (rowsType == DSActionDataXML.Element.ModifiedRows)
					uDbt.ModifiedRows.Add(xRow);
				else if (rowsType == DSActionDataXML.Element.DeletedRows)
					uDbt.DeletedRows.Add(xRow);
			}
		}

		///<summary>
		/// Delete:
		/// <?xml version="1.0"?> 
		/// <DeleteRecord> 
		///		<KeysForDeleteAction InternationalCode="ZF"/> 
		///	</DeleteRecord>
		///</summary>
		//--------------------------------------------------------------------------------
		private static ActionDataInfo ParseDelete(string actionData)
		{
			try
			{
				XDocument xDoc = XDocument.Load(new StringReader(actionData));

				XElement xElemRoot = (XElement)xDoc.FirstNode;
				if (string.Compare(xElemRoot.Name.LocalName, DSActionDataXML.Element.DeleteRecord, StringComparison.InvariantCultureIgnoreCase) != 0)
					return null;

				// leggo tutti i nodi figli di tipo <KeysForDeleteAction>
				IEnumerable<XElement> keys = from k in xElemRoot.Descendants()
											 where (string.Compare(k.Name.LocalName, DSActionDataXML.Element.KeysForDeleteAction, StringComparison.InvariantCultureIgnoreCase) == 0)
											 select k;

				if (keys == null || keys.Count() <= 0)
					return null;

				ActionDataInfo adi = new ActionDataInfo();

				// TODO: non so se e' possibile che ci siano piu' nodi di tipo <KeysForDeleteAction>
				foreach (XElement kfdElem in keys)
				{
					if (!kfdElem.HasAttributes)
						continue;

					Row xRow = new Row();

					foreach (XAttribute attr in kfdElem.Attributes())
						xRow.Values.Add(attr.Name.LocalName, attr.Value);

					adi.KeysForDeleteActionList.Add(xRow);
				}

				return adi;
			}
			catch (Exception e)
			{
				throw new DSException("ActionDataParser.ParseDelete", e.Message, actionData);
			}
		}

		/// <summary>
		/// Delete Attachment
		/// </summary>
		/// <param name="actionData"></param>
		/// <returns></returns>
		//--------------------------------------------------------------------------------
		private static ActionDataInfo ParseDeleteAttachment(string actionData)
		{
			ActionDataInfo ati = new ActionDataInfo();

			XDocument xDoc = null;
			Row xRow = new Row();

			try
			{
				xDoc = XDocument.Parse(actionData);
			}
			catch (Exception)
			{
				return ati;
			}

			try
			{
				xRow.Values.Add("AttachmentID", xDoc.Element("Attachments").Element("AttachmentID").Value);
			}
			catch
			{
				return ati;
			}

			ati.KeysForDeleteActionList.Add(xRow);

			return ati;
		}
	}

	///<summary>
	/// Classe che rappresenta in memoria il contenuto xml della colonna ActionData
	///</summary>
	//================================================================================
	internal class ActionDataInfo
	{
		public List<UpdatedDbt> UpdatedDbts = new List<UpdatedDbt>(); // lista dei dbtslavebuffered modificati
		public List<Row> KeysForDeleteActionList = new List<Row>(); // lista di chiavi da eliminare
		public string DmsKey = string.Empty;
	}

	///<summary>
	/// Ogni DBTSlaveBuffered ha potenzialmente delle righe aggiunte, modificate e/o cancellate
	/// Queste vengono memorizzate in tre liste separate
	///</summary>
	//================================================================================
	internal class UpdatedDbt
	{
		public string TableName { get; private set; }

		public List<Row> NewRows = new List<Row>();
		public List<Row> ModifiedRows = new List<Row>();
		public List<Row> DeletedRows = new List<Row>();

		//--------------------------------------------------------------------------------
		public UpdatedDbt(string tableName)
		{
			TableName = tableName;
		}
	}

	///<summary>
	/// Ogni riga ha una sequenza di attributi e relativi valori letti dal database
	///</summary>
	//================================================================================
	internal class Row
	{
		public Dictionary<string, string> Values = new Dictionary<string, string>();
	}

	///<summary>
	/// Tag e attributi per il parse del testo xml memorizzato nella colonna ActionData 
	/// della tabella DS_ActionsLog
	///</summary>
	//=========================================================================
	internal sealed class DSActionDataXML
	{
		//-----------------------------------------------------------------
		private DSActionDataXML()
		{ }

		internal sealed class Element
		{
			//-----------------------------------------------------------------
			private Element()
			{ }

			public const string DeleteRecord = "DeleteRecord";
			public const string KeysForDeleteAction = "KeysForDeleteAction";
			public const string UpdatedDbtS = "UpdatedDbtS";
			public const string UpdatedDbt = "UpdatedDbt";
			public const string NewRows = "NewRows";
			public const string ModifiedRows = "ModifiedRows";
			public const string DeletedRows = "DeletedRows";
			public const string Row = "Row";
		}

		internal sealed class Attribute
		{
			//-----------------------------------------------------------------
			private Attribute()
			{ }

			public const string Tablename = "tablename";
		}
	}
}
