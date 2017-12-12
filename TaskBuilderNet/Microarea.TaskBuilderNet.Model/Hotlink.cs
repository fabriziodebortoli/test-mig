using System;
using Microarea.Framework.TBApplicationWrapper;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Interfaces;
using Microarea.TaskBuilderNet.Interfaces.Model;

namespace Microarea.TaskBuilderNet.Model
{

	//=========================================================================
	/// <summary>
	/// Hotlink
	/// </summary>
	public class Hotlink<TRecord> : MHotLink
		where TRecord : MSqlRecord
	{
		//-------------------------------------------------------------------------
		/// <summary>
		/// Gets the associated record
		/// </summary>
		public new TRecord Record
		{
			get
			{
				if (this.record == null)
				{
					this.Add((TRecord)Activator.CreateInstance(typeof(TRecord), (base.GetRecordPtr())));
				}
				return (TRecord)record;
			}
		}

		//-------------------------------------------------------------------------
		/// <summary>
		/// Gets the current data obj
		/// </summary>
		public override IDataObj CurrentDataObj
		{
			get
			{
				return base.CurrentDataObj;
			}
			set
			{
				if (base.CurrentDataObj != null)
					((MDataObj)base.CurrentDataObj).ValueChanged -= new EventHandler<EasyBuilderEventArgs>(ValueChanged_FindData);
				base.CurrentDataObj = value;
				if (base.CurrentDataObj != null)
					((MDataObj)base.CurrentDataObj).ValueChanged += new EventHandler<EasyBuilderEventArgs>(ValueChanged_FindData);
			}
		}

		//-------------------------------------------------------------------------
		private void ValueChanged_FindData(object sender, EasyBuilderEventArgs e)
		{
			if (ReadOnDataLoaded && CurrentDataObj != null && Document.FormMode == FormModeType.Browse)
				FindRecord(((MDataObj)CurrentDataObj));

		}

		//-------------------------------------------------------------------------
		/// <summary>
		/// Creates a new hot link instance
		/// </summary>
		public Hotlink(string name, IDocumentDataManager document)
			:
			base(TableAttribute.GetTableName(typeof(TRecord)), name, document, false)
		{
			CallCreateComponents(false);
			AttachToDocument(document);
			TRecord rec = Record;
		}

		//-------------------------------------------------------------------------
		/// <summary>
		/// Creates a new hot link instance
		/// </summary>
		public Hotlink(INameSpace hlkNamespace, string hklName, IDocumentDataManager document)
			: base(hlkNamespace as NameSpace, hklName, document, false)
		{
			AttachToDocument(document);
			TRecord rec = Record;
		}

		//-------------------------------------------------------------------------
		private void AttachToDocument(IDocumentDataManager document)
		{
			MAbstractFormDoc doc = document as MAbstractFormDoc;
			if (doc != null)
			{
				doc.Add(this);
			}
		}

		//-------------------------------------------------------------------------
		/// <summary>
		/// Clear components
		/// </summary>
		public override void ClearComponents()
		{
			base.ClearComponents();
		}
	}
}
