using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TBMobile
{
	public class MobileEventArgs : EventArgs
	{
	}
	public class DataManagerEventArgs : MobileEventArgs
	{
		public DataManagerEventArgs(MSqlTable table)
		{
			Table = table;
		}

		public virtual MSqlTable Table { get; set; }
	}
	public class RowEventArgs : MobileEventArgs
	{
		public RowEventArgs(int nRow, MSqlRecord record) { Record = record; RowNumber = nRow; }
		public bool Cancel { get; set; }
		public MSqlRecord Record { get; set; }
		public int RowNumber { get; set; }
	}
	public enum DataRelationType
	{
		None = 0,
		Master = 1,
		OneToOne = 2,
		OneToMany = 3,
		ForeignKey = 4,
	}

	public enum DelayReadType
	{
		Immediate = 0,
		DalayedUntilBrowse = 1,
		DalayedUntilEdit = 2,
		Delayed = 3,
	}

	public abstract class MGenericDataManager
	{

		public MGenericDataManager() { }

		public virtual string FilterQuery { get; set; }
		public abstract
			MSqlRecord Record { get; }

		public event EventHandler<DataManagerEventArgs> DefiningQuery;

		public void OnDefineQuery(MSqlTable mSqlTable)
		{
			if (DefiningQuery != null)
				DefiningQuery(this, new DataManagerEventArgs(mSqlTable));
		}
		//public virtual void OnQueried();
		//public virtual void OnQuerying();
	}


	public abstract class MDBTObject : MGenericDataManager
	{
		internal enum DBTType { MASTER = 1, SLAVE = 2, SLAVE_BUFFERED = 3}
		private MSqlRecord record;
		private MSqlRecord oldRecord;
		protected MSqlTable table;
		protected MDocument document;
		private string name;
		private bool queryPrepared = false;

		internal event EventHandler DataAvailable;

		public MDBTObject(MSqlRecord record, string name, MDocument document)
		{
			this.record = record;
			this.oldRecord = record.Clone();
			this.table = new MSqlTable(record);
			this.name = name;
		}
		public MDocument Document { get { return document; } set { document = value; } }


		//public bool IsUpdatable { get; set; }
		public string Name { get { return name; } }
		public MSqlRecord OldRecord { get { return oldRecord; } }
		public override MSqlRecord Record { get { return record; } }
		public MSqlTable Table { get { return table; } }
		public string TableName { get { return record.Name; } }
		//public virtual string Title { get; }
		public virtual bool Modified { get; set; }
		//public virtual event EventHandler<CancelEventArgs> CheckingPrimaryKey;
		//public event EventHandler<BadDataRowEventArgs> CheckPrimaryKey;
		//public virtual event EventHandler<EventArgs> ControlsEnabled;
		//public virtual event EventHandler<CancelEventArgs> DataForTransactionChecked;
		public virtual void OnPreparePrimaryKey(MSqlTable table) { }

		internal virtual void SetReadOnlyFields(bool readOnly)
		{
			foreach (MSqlRecordItem item in Record.Fields)
				item.DataObj.SetReadOnly(readOnly);
			
		}
		/// <summary>
		/// Prepares the structure of the dbt queries, so as they can be sent to server in JSON format
		/// </summary>
		internal virtual void PrepareQueries()
		{ 
			OnDefineQuery(table);
				
		}
		internal virtual JToken ToJSONObject()
		{
			if (!queryPrepared)
			{
				PrepareQueries();
				queryPrepared = true;
			}

			JObject obj = new JObject(
				 new JProperty("name", Name),
				 new JProperty("table", TableName),
				  new JProperty("type", Type),
				 new JProperty("fields", Record.ToJSONObject())			 
				 );
			if (!Table.IsEmpty)
				obj["query"] = Table.ToJSONObject();
			return obj;
		}
		void OnGetDataResponse(ResponseEventArgs e)
		{
			if (!e.Success)
				return;
			FillData(e.ResponseObject);
			if (DataAvailable != null)
				DataAvailable(this, EventArgs.Empty);
		}


		internal virtual JObject GetJSONData()
		{
			JObject dbt = new JObject();
			JArray ar = new JArray();
			dbt["name"] = Name;
			dbt["records"] = ar;
			JObject rec = record.GetJSONData();
			ar.Add(rec);
			return dbt;
		}

		/// <summary>
		/// parses json data and fill in the record fields
		/// </summary>
		/// <param name="json"></param>
		private void FillData(string json)
		{
			try
			{
				JObject dbt = JObject.Parse(json);
				FillData(dbt);
			}
			catch
			{


			}
		}
		internal virtual void FillData(JObject dbt)
		{
			JArray ar = (JArray)dbt["records"];
			JObject rec = (JObject)ar[0];
			record.Assign(rec);
		}
		internal void RefreshData()
		{
			Document.Connector.GetData(Document.Id, Name, OnGetDataResponse);
		}

		internal abstract DBTType Type { get; }
	}
	
}
