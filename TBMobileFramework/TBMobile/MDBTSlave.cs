using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TBMobile
{
	public class MDBTSlave : MDBTObject
	{
		public MDBTSlave(MSqlRecord record, string dbtName, MDocument document) :
			base(record, dbtName, document)
		{
		}

		public bool AllowEmpty { get; set; }
		public override string FilterQuery { get; set; }
		public MDBTObject Master { get; set; }
		public bool OnlyDeleteAction { get; set; }
		public int PreloadStep { get; set; }
		public DelayReadType ReadBehaviour { get; set; }
		internal override DBTType Type
		{
			get { return DBTType.SLAVE; }
		}
		//public override DataRelationType Relation { get; }

		//public event EventHandler<MobileEventArgs> PrimaryKeyPrepared;

	}


	public class TDBTSlave<TRecord, TMaster> : MDBTSlave
		where TRecord : MSqlRecord
		where TMaster : MDBTObject
	{
		public TDBTSlave(TRecord record, string dbtName, MDocument document)
			: base(record, dbtName, document)
		{ }

		new public TRecord Record { get { return (TRecord)base.Record; } }
		new public TRecord OldRecord { get { return (TRecord)base.OldRecord; } }
		new public TMaster Master { get { return (TMaster)base.Master; } }
	}
}
