using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TBMobile
{
	public class MDBTMaster : MDBTObject
	{
		private List<MDBTSlave> slaves = new List<MDBTSlave>();
		private MSqlTable browserTable;
		//public event EventHandler<DataManagerEventArgs> PreparingFindQuery;
		public event EventHandler<DataManagerEventArgs> PreparingBrowser;
		public MDBTMaster(MSqlRecord record, string dbtName, MDocument document) :
			base(record, dbtName, document)
		{
			browserTable = new MSqlTable(Record);
		}

		/// <summary>
		/// Prepares the structure of the dbt queries, so as they can be sent to server in JSON format
		/// </summary>
		internal override void PrepareQueries()
		{
			base.PrepareQueries();
			OnPrepareBrowser(browserTable);
			//OnPrepareFindQuery(browserTable);

		}
		internal override JToken ToJSONObject()
		{
			JToken obj = base.ToJSONObject();
			if (!browserTable.IsEmpty)
				obj["browserQuery"] = browserTable.ToJSONObject();
			JArray ar = new JArray();
			foreach (MDBTSlave slave in slaves)
				ar.Add(slave.ToJSONObject());
			obj["dbts"] = ar;
			return obj;
		}


		internal override JObject GetJSONData()
		{
			JObject obj = base.GetJSONData();
			JArray ar = new JArray();
			obj["dbts"] = ar;
			foreach (MDBTSlave sl in slaves)
			{
				ar.Add(sl.GetJSONData());
			}
			return obj;
		}

		internal override void FillData(JObject dbt)
		{
			base.FillData(dbt);
			JArray ar = (JArray)dbt["dbts"];
			if (ar == null)
				return;
			for (int i = 0; i < ar.Count; i++)
			{
				JObject slave = (JObject)ar[i];
				MDBTSlave dbtSlave = GetSlave(slave["name"].ToString());
				if (dbtSlave != null)
					dbtSlave.FillData(slave);
			}

		}
		internal override MDBTObject.DBTType Type
		{
			get { return DBTType.MASTER; }
		}
		public virtual string BrowserQuery { get; set; }
		public override string FilterQuery { get; set; }
		//public override DataRelationType Relation { get; }

		/*internal void OnPrepareFindQuery(MSqlTable mSqlTable)
		{
			if (PreparingFindQuery != null)
				PreparingFindQuery(this, new DataManagerEventArgs(mSqlTable));
		}*/
		internal void OnPrepareBrowser(MSqlTable mSqlTable)
		{
			if (PreparingBrowser != null)
				PreparingBrowser(this, new DataManagerEventArgs(mSqlTable));
		}

		public void AttachSlave(MDBTSlave dbtSlave)
		{
			slaves.Add(dbtSlave);
			dbtSlave.Master = this;
			dbtSlave.Document = Document;
		}

		public T GetSlave<T>(string name = null) where T : MDBTSlave
		{
			foreach (MDBTSlave slave in slaves)
			{
				if (slave.GetType() == typeof(T))
				{
					if (name == null || slave.Name.Equals(name))
						return (T)slave;
				}
			}

			return null;
		}
		public MDBTSlave GetSlave(string name)
		{
			foreach (MDBTSlave slave in slaves)
				if (slave.Name.Equals(name))
					return slave;
			return null;
		}

		internal override void SetReadOnlyFields(bool readOnly)
		{
			base.SetReadOnlyFields(readOnly);
			foreach (MDBTSlave slave in slaves)
				slave.SetReadOnlyFields(readOnly);
		}
	}

	public class TDBTMaster<TRecord> : MDBTMaster where TRecord : MSqlRecord
	{
		public TDBTMaster(TRecord record, string dbtName, MDocument document)
			: base(record, dbtName, document)
		{ }

		new public TRecord Record { get { return (TRecord)base.Record; } }
		new public TRecord OldRecord { get { return (TRecord)base.OldRecord; } }
	}

}
