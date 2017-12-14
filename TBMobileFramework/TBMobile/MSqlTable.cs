using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TBMobile
{

	// Summary:
	//     Wrapper class to taskbuilder sql table class
	public class MSqlTable : IDisposable
	{
		private MSqlRecord record;
		//private FromStatement fromStatement = new FromStatement();
		//private GroupByStatement groupByStatement = new GroupByStatement();
		//private HavingStatement havingStatement = new HavingStatement();
		private OrderByStatement orderByStatement = new OrderByStatement();
		//private SelectStatement selectStatement = new SelectStatement();
		private WhereStatement whereStatement = new WhereStatement();
		public MSqlTable(MSqlRecord record)
		{
			this.record = record;
		}

		public bool IsEmpty { get { return Where.IsEmpty; } }
		//public FromStatement From { get { return fromStatement; } }
		//public GroupByStatement GroupBy { get { return groupByStatement; } }
		//public HavingStatement Having { get { return havingStatement; } }
		public OrderByStatement OrderBy { get { return orderByStatement; } }
		public string PlainQuery { get; set; }
		public MSqlRecord Record { get { return record; } set { record = value; } }
		//public SelectStatement Select { get { return selectStatement; } }
		public WhereStatement Where { get { return whereStatement; } }

		/*	public void AddNew();
			public void Close();
			public void Delete();
			public void Delete(MSqlRecord oldRecord);
			public override sealed void Dispose();
			protected virtual void Dispose(bool A_0);
			public void Edit();
			public void ExecuteQuery();
			public void FirstResult();
			public bool IsBOF();
			public bool IsEmpty();
			public bool IsEOF();
			public void LastResult();
			public bool LockCurrent();
			public bool LockCurrent(bool useMessageBox);
			public void NextResult();
			public void PrevResult();
			public bool UnlockAll();
			public bool UnlockCurrent();
			public int Update();
			public int Update(MSqlRecord oldRecord);*/

		public void Dispose()
		{
			
		}

		internal JToken ToJSONObject()
		{
			JObject obj = new JObject();
			if (!Where.IsEmpty)
				obj["where"] = Where.ToJSONObject();
			return obj;
		}

		
	}
}
