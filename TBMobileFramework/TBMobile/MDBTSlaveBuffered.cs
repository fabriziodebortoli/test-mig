using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TBMobile
{
	public class MDBTSlaveBuffered : MDBTSlave
	{
		protected ObservableCollection<MSqlRecord> rows = new ObservableCollection<MSqlRecord>();
		protected ObservableCollection<MSqlRecord> oldRows = new ObservableCollection<MSqlRecord>();
		public MDBTSlaveBuffered(MSqlRecord record, string dbtName, MDocument document) :
			base(record, dbtName, document)
		{
		}
		public bool CheckDuplicateKey { get; set; }
		public int CurrentRow { get; set; }
		public override string FilterQuery { get; set; }

		internal override MDBTObject.DBTType Type
		{
			get { return DBTType.SLAVE_BUFFERED; }
		}
		public bool ReadOnly { get; set; }
		public ObservableCollection<MSqlRecord> Rows { get { return rows; } }
		public ObservableCollection<MSqlRecord> OldRows { get { return oldRows; } }

		public event EventHandler<RowEventArgs> PrimaryKeyPrepared;
		public event EventHandler<RowEventArgs> RowPrepared;
		public event EventHandler<RowEventArgs> DeletingRow;
		/*public event EventHandler<RowEventArgs> AddingRow;
		public event EventHandler<RowEventArgs> InsertingRow;
		public event EventHandler<RowEventArgs> RowAdded;
		public event EventHandler<RowEventArgs> RowDeleted;
		public event EventHandler<RowEventArgs> RowInserted;

		public event EventHandler<RowEventArgs> AuxColumnsPrepared;
		public event EventHandler<RowEventArgs> CurrentRowChanged;
		public event EventHandler<EventArgs> DataLoaded;
		public event EventHandler<EventArgs> LoadingData;
		public event EventHandler ReadOnlyChanged;
		public event EventHandler<RowEventArgs> RecordAdded;
		public event EventHandler<RowEventArgs> RemovingRecord;
		*/

		internal override void FillData(JObject dbt)
		{

			JArray ar = (JArray)dbt["records"];
			Clear();
			foreach (JObject rec in ar)
			{
				MSqlRecord r = AddRecord();
				r.Assign(rec);
			}

		}

		internal override JObject GetJSONData()
		{
			JObject dbt = new JObject();
			JArray ar = new JArray();
			dbt["name"] = Name;
			dbt["records"] = ar;
			foreach (MSqlRecord r in Rows)
				ar.Add(r.GetJSONData());
			return dbt;
		}

		public MSqlRecord AddRecord()
		{
			MSqlRecord rec = (MSqlRecord)Activator.CreateInstance(Record.GetType());

			int n = rows.Count;
			RowEventArgs args = new RowEventArgs (n, rec);
			if (PrimaryKeyPrepared != null)
				PrimaryKeyPrepared (this, args);
			if (args.Cancel)
				return null;
			if (RowPrepared != null)
				RowPrepared (this, args);
			if (args.Cancel)
				return null;

			rows.Add(rec);
			return rec;
		}

		public MSqlRecord InsertRecord(int nRow)
		{
			MSqlRecord rec = (MSqlRecord)Activator.CreateInstance(Record.GetType());

			RowEventArgs args = new RowEventArgs (nRow, rec);
			if (PrimaryKeyPrepared != null)
				PrimaryKeyPrepared (this, args);
			if (args.Cancel)
				return null;
			if (RowPrepared != null)
				RowPrepared (this, args);
			if (args.Cancel)
				return null;

			rows.Insert(nRow, rec);
			return rec;
		}
		public virtual void Clear()
		{
			rows.Clear();
		}
		internal override void SetReadOnlyFields(bool readOnly)
		{
			foreach (MSqlRecord row in Rows)
				foreach (MSqlRecordItem item in row.Fields)
					item.DataObj.SetReadOnly(readOnly);
		}
	  
		public bool DeleteRow(MSqlRecord record)
		{
			int nRow = rows.IndexOf (record);
			if (!OnBeforeDeleteRow (nRow, rows [nRow]))
				return false;
			rows.RemoveAt (nRow);
			OnAfterDeleteRow (nRow);
			return true;
		}

		public bool DeleteRow(int nRow)
		{
			if (nRow < 0 || nRow >= rows.Count)
				return false;
			if (!OnBeforeDeleteRow(nRow, rows[nRow]))
				return false;
			rows.RemoveAt(nRow);
			OnAfterDeleteRow(nRow);
			return true;
		}

		//public MSqlRecord GetCurrentRecord();
		//public MDBTSlave GetCurrentSlave();
		//public MDBTSlave GetCurrentSlave(string name);
		//public MDBTSlave GetDBTSlave(string name, int idx);
		//public virtual string GetDuplicateKeyMsg(SqlRecord* pRec);
		//public virtual DataObj* GetDuplicateKeyPos(SqlRecord* pRec);
		public MSqlRecord GetOldRecord(int nRow)
		{
			if (nRow < 0 || nRow >= oldRows.Count)
				return null;
			return oldRows[nRow];
		}
		public MSqlRecord GetRecord(int nRow)
		{
			if (nRow < 0 || nRow >= rows.Count)
				return null;
			return rows[nRow];
		}
		//public int GetRecordIndex(MSqlRecord record);

		//public bool LoadMoreRows(int preloadStep);
		public virtual bool OnBeforeDeleteRow(int nRow, MSqlRecord record)
		{
			if (DeletingRow != null)
			{
				RowEventArgs args = new RowEventArgs(nRow, record);
				DeletingRow(this, args);
				return !args.Cancel;
			}
			return true;
		}

		public virtual void OnAfterAddRow(int nRow) { }
		public virtual void OnAfterDeleteRow(int nRow) { }
		public virtual void OnAfterInsertRow(int nRow, MSqlRecord record) { }
		public virtual bool OnBeforeAddRow(int nRow) { return true; }
		public virtual bool OnBeforeInsertRow(int nRow) { return true; }
		public virtual void OnPrepareAuxColumns(int nRow, MSqlRecord record) { }
		public virtual void OnPreparePrimaryKey(int nRow, MSqlRecord record) { }
		public virtual void OnPrepareRow(int nRow, MSqlRecord record) { }
		public virtual void OnSetCurrentRow(int nRow) { }
	}

	public class TDBTSlaveBuffered<TRecord, TMaster> : MDBTSlaveBuffered
		where TRecord : MSqlRecord
		where TMaster : MDBTObject
	{
		public TDBTSlaveBuffered(TRecord record, string dbtName, MDocument document)
			: base(record, dbtName, document)
		{ }

		new public TRecord Record { get { return (TRecord)base.Record; } }
		new public TRecord OldRecord { get { return (TRecord)base.OldRecord; } }
		new public TMaster Master { get { return (TMaster)base.Master; } }
	}
}
