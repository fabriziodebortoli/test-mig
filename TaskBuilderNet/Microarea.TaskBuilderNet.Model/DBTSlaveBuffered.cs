using System;
using System.Collections;
using System.ComponentModel;
using Microarea.Framework.TBApplicationWrapper;
using Microarea.TaskBuilderNet.Core.EasyBuilder;
using Microarea.TaskBuilderNet.Interfaces.Model;
using Microarea.TaskBuilderNet.Interfaces.Validation;

namespace Microarea.TaskBuilderNet.Model
{
	
	//=========================================================================
	/// <summary>
	/// Generic DbtSlaveBuffered
	/// </summary>
	public class DBTSlaveBuffered<TRecord, TDbtMaster> : MDBTSlaveBuffered, IListSource
		where TRecord : MSqlRecord, new()
		where TDbtMaster : IDocumentMasterDataManager
	{
		IDocumentDataManager document;

		//---------------------------------------------------------------------
		/// <summary>
		/// Gets the document that owns this object
		/// </summary>
		public override IDocumentDataManager Document
		{
			get
			{
				return document;
			}
		}
		//---------------------------------------------------------------------
		/// <summary>
		/// Gets the associated master record
		/// </summary>
		public new TDbtMaster Master
		{
			get { return (TDbtMaster)base.Master; }
			set { base.Master = value; }
		}

		//---------------------------------------------------------------------
		/// <summary>
		/// Get the list of records
		/// </summary>
		public override IList Rows
		{
			get
			{
				if (rows == null)
				{
					rows = new RecordBindingList<TRecord>();
				}

				return rows;
			}
		}

		//-------------------------------------------------------------------------
		/// <summary>
		/// Creates a new DbtSlaveBuffered instance
		/// </summary>
		public DBTSlaveBuffered(string dbtName, IDocumentDataManager document)
			:
			base(TableAttribute.GetTableName(typeof(TRecord)), dbtName, document, false)
		{
			this.document = document;
			Initialize();
		}
		
		//---------------------------------------------------------------------
		/// <summary>
		///  Creates a new DbtSlaveBuffered instance
		/// </summary>
		/// <param name="dbtPtr"></param>
		/// <param name="document"></param>
		protected DBTSlaveBuffered(IntPtr dbtPtr, IDocumentDataManager document)
			: base(dbtPtr)
		{
			this.document = document;
			Initialize();
		}
		
		//---------------------------------------------------------------------
		private void Initialize()
		{
			this.AuxColumnsPrepared += new EventHandler<RowEventArgs>(DBTSlaveBuffered_AuxColumnsPrepared);
			this.DeletingRow += new EventHandler<RowEventArgs>(DBTSlaveBuffered_DeletingRow);
			this.RemovingRecord += new EventHandler<RowEventArgs>(DBTSlaveBuffered_DeletingRow);

			this.InitializeRecord<TRecord>();
		}
		//---------------------------------------------------------------------
		/// <summary>
		/// Creates an instance of this object, used when slave of a DBTSlaveBuffered
		/// </summary>
		/// <param name="dbtPtr"></param>
		/// <returns></returns>
		public override MDBTObject CreateAndAttach(IntPtr dbtPtr)
		{
			return (MDBTObject)Activator.CreateInstance(GetType(), new object[] { dbtPtr, Document });
		}

		//---------------------------------------------------------------------
		void DBTSlaveBuffered_DeletingRow(object sender, RowEventArgs e)
		{
			RemoveValidators(e.Record);
		}
		
		//-----------------------------------------------------------------------------
		void RemoveValidators(IRecord actualRecord)
		{
			//Qui non viene testato il DocumentFormMode come nella AddValidators, perche' per esempio da Browse a Edit, il form mode e' gia edit, 
			//quindi non abbiamo l'informazione per capire se i validatori sono presenti (e percio' andrebbero rimossi)
			ITBValidationConsumer validationConsumer = Document as ITBValidationConsumer;
			if (validationConsumer != null)
			{
				foreach (IRecordField recordField in actualRecord.Fields)
				{
					validationConsumer.ValidationManager.Remove(recordField.DataObj);
					MDataObj dataObj = recordField.DataObj as MDataObj;
					if (dataObj != null)
					{
						dataObj.ValueChanged -= new EventHandler<EasyBuilderEventArgs>(DataObj_ValueChanged);
					}
				}
			}
		}

		//---------------------------------------------------------------------
		void DBTSlaveBuffered_AuxColumnsPrepared(object sender, RowEventArgs e)
		{
			if (Document == null || (Document.FormMode != FormModeType.New && Document.FormMode != FormModeType.Edit))
				return;

			ITBValidationConsumer validationConsumer = Document as ITBValidationConsumer;
			if (validationConsumer == null)
				return;
			
			MDataObj protoTypeDataObj = null;
			bool hasValidators = false;
			for (int i = 0; i < e.Record.Fields.Count; i++)
			{
				MDataObj dataObj = ((IRecordField)e.Record.Fields[i]).DataObj as MDataObj;
				protoTypeDataObj = Record.GetField(dataObj.Name).DataObj as MDataObj;

				hasValidators = validationConsumer.ValidationManager.CloneFocusChangeValidators(protoTypeDataObj, dataObj);

				if (hasValidators)
				{
					dataObj.ValueChanged += new EventHandler<EasyBuilderEventArgs>(DataObj_ValueChanged);
				}
			}
		}

		//---------------------------------------------------------------------
		void DataObj_ValueChanged(object sender, EasyBuilderEventArgs e)
		{
			//Addvalidator su singolo obj pescandolo da l prototipo
			//e lo scolleghiamo

			MDataObj dataObj = sender as MDataObj;
			if (dataObj == null)
				return;

			//ci scolleghiamo perche non dovremo piu aggiungere validatori
			dataObj.ValueChanged -= new EventHandler<EasyBuilderEventArgs>(DataObj_ValueChanged);

			//Aggiungiamo i validatori
			ITBValidationConsumer validationConsumer = Document as ITBValidationConsumer;
			if (validationConsumer != null)
			{
				IRecord prototypeRecord = Record;
				IDataObj prototypeDataObj = prototypeRecord.GetField(dataObj.Name).DataObj;
				validationConsumer.ValidationManager.UpdateSubmitValidators(prototypeDataObj, dataObj);
			}
			
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

		//-------------------------------------------------------------------------
		/// <summary>
        /// Gets the current record
        /// </summary>
		public virtual TRecord GetCurrentRow()
		{
			return ((TRecord)(this.GetCurrentRecord()));
		}

		//-------------------------------------------------------------------------
		/// <summary>
		/// Adds a new record
		/// </summary>
		/// <returns>Added record</returns>
		public virtual TRecord AddRow()
		{
			return ((TRecord)(this.AddRecord()));
		}

		//-------------------------------------------------------------------------
		/// <summary>
		/// Insert a new record in the specified position
		/// </summary>
		/// <returns>Added record</returns>
		public virtual TRecord InsertRow(int rowNumber)
		{
			return ((TRecord)(this.InsertRecord(rowNumber)));
		}

		//-------------------------------------------------------------------------
		/// <summary>
		/// Gets the specified record
		/// </summary>
		public virtual TRecord GetRow(int rowNumber)
		{
			return ((TRecord)(this.GetRecord(rowNumber)));
		}

		//-------------------------------------------------------------------------
		/// <summary>
		/// Gets the specified old record
		/// </summary>
		public virtual TRecord GetOldRow(int rowNumber)
		{
			return ((TRecord)(this.GetOldRecord(rowNumber)));
		}

		//---------------------------------------------------------------------
		/// <summary>
		/// Gets a boolean indicating if this class contains a colleciton
		/// </summary>
		public bool ContainsListCollection
		{
			get { return true; }
		}

		//---------------------------------------------------------------------
		/// <summary>
		/// Gets the encapslated collection
		/// </summary>
		public IList GetList()
		{
			return this.Rows;
		}
	}
}
