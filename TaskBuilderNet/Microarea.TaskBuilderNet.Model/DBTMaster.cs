using Microarea.Framework.TBApplicationWrapper;
using Microarea.TaskBuilderNet.Interfaces.Model;

namespace Microarea.TaskBuilderNet.Model
{
	
	/// <summary>
	/// DBTMaster
	/// </summary>
	public class DBTMaster<TRecord> : MDBTMaster
		where TRecord: MSqlRecord
	{
		//-------------------------------------------------------------------------
		/// <summary>
		/// Creates a new instance of DBTMaster
		/// </summary>
		public DBTMaster(string dbtName, IDocumentDataManager document)
			:
			base(TableAttribute.GetTableName(typeof(TRecord)), dbtName, document, false)
		{
			this.InitializeRecord<TRecord>();
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

	}
}
