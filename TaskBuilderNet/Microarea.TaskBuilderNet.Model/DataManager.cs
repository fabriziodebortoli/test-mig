using System;
using Microarea.Framework.TBApplicationWrapper;

namespace Microarea.TaskBuilderNet.Model
{
	
	//=========================================================================
	/// <summary>
	/// Generic DataManager
	/// </summary>
	public class DataManager<TRecord> : MDataManager
		where TRecord : MSqlRecord
	{
		private TRecord _record;

		//-------------------------------------------------------------------------
		/// <summary>
		/// Gets the associated record
		/// </summary>
		public new TRecord Record
		{
			get
			{
				if (this._record == null)
				{
					this._record = (TRecord)Activator.CreateInstance(typeof(TRecord), (base.GetRecordPtr()));
					this.Add(this._record);
				}
				return this._record;
			}
		}

		//-------------------------------------------------------------------------
		/// <summary>
		/// Creates a new instance of DataManager
		/// </summary>
		public DataManager(String name)
			:
			base(TableAttribute.GetTableName(typeof(TRecord)), name)
		{
			CreateComponents();
		}

		//-------------------------------------------------------------------------
		/// <summary>
		/// Create al components
		/// </summary>
		public override void CreateComponents()
		{
			this.ApplyResources();
		}

		//-------------------------------------------------------------------------
		/// <summary>
		/// Clear all components
		/// </summary>
		public override void ClearComponents()
		{
			base.ClearComponents();
		}

		//-------------------------------------------------------------------------
		/// <summary>
		/// Define the query 
		/// </summary>
		public override void DefineQuery(MSqlTable mSqlTable)
		{
			mSqlTable.Select.All();

			base.DefineQuery(mSqlTable);
		}

		//-------------------------------------------------------------------------
		/// <summary>
		/// Prepare the query
		/// </summary>
		public override void PrepareQuery(MSqlTable mSqlTable)
		{
			base.PrepareQuery(mSqlTable);
		}
	}
}
