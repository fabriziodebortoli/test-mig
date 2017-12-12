using System;
using Microarea.Framework.TBApplicationWrapper;
using Microarea.TaskBuilderNet.Interfaces.Model;

namespace Microarea.TaskBuilderNet.Model
{

	//=========================================================================
	/// <summary>
	/// DBTSlave
	/// </summary>
	public class DBTSlave<TRecord, TDbtMaster> : MDBTSlave
		where TRecord : MSqlRecord
		where TDbtMaster : IDocumentMasterDataManager
	{

		//---------------------------------------------------------------------
		/// <summary>
		/// Creates a DBTSlave instance
		/// </summary>
		public DBTSlave()
		{
			this.InitializeRecord<TRecord>();
		}


		//---------------------------------------------------------------------
		/// <summary>
		/// Creates a new DBTSlave instance
		/// </summary>
		/// <param name="dbtPtr"></param>
		protected DBTSlave(IntPtr dbtPtr)
			: base(dbtPtr)
		{
			this.InitializeRecord<TRecord>();
		}

		//---------------------------------------------------------------------
		/// <summary>
		/// Creates a new DBTSlave instance
		/// </summary>
		public DBTSlave(string dbtName, IDocumentDataManager document)
			: base(TableAttribute.GetTableName(typeof(TRecord)), dbtName, document, false)
		{
			this.InitializeRecord<TRecord>();
		}

		//---------------------------------------------------------------------
		/// <summary>
		/// Gets the associated master
		/// </summary>
		public new TDbtMaster Master
		{
			get { return (TDbtMaster)base.Master; }
			set { base.Master = value; }
		}

		//-------------------------------------------------------------------------
		/// <summary>
		/// Gets the associated record
		/// </summary>
		public new TRecord Record
		{
			get
			{
				return (TRecord)record;
			}
		}

		//-------------------------------------------------------------------------
		/// <summary>
		/// Get the old record
		/// </summary>
		public new TRecord OldRecord
		{
			get
			{
				return (TRecord)oldRecord;
			}
		}

		//---------------------------------------------------------------------
		/// <summary>
		/// Creates an instance of this object, used when slave of a DBTSlaveBuffered
		/// </summary>
		/// <param name="dbtPtr"></param>
		/// <returns></returns>
		public override MDBTObject CreateAndAttach(IntPtr dbtPtr)
		{
			return (MDBTObject)Activator.CreateInstance(GetType(), new object[] { dbtPtr });

		}
	}
}
