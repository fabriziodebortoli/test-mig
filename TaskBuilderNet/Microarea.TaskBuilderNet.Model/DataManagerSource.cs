using System;
using System.Collections;
using System.ComponentModel;
using Microarea.Framework.TBApplicationWrapper;
using Microarea.TaskBuilderNet.Core.ComponentModel;
using Microarea.TaskBuilderNet.Interfaces.Model;

namespace Microarea.TaskBuilderNet.Model
{
	//=========================================================================
	public class DataManagerSource : MDataManager, IListSource
	{
		public event EventHandler<ValidItemEventArgs> ValidItem;

		bool enabled = true;
		IList items;

		string columnNameForKey;
		string columnNameForDescription;

		//-------------------------------------------------------------------------
		public bool Enabled 
		{
			get { return enabled; } 
			set 
			{
				if (!enabled && items != null)
				{
					items.Clear();
				}
				enabled = value; 
			}
		}

		//-------------------------------------------------------------------------
		public IList Items 
		{
			get 
			{
				if (items == null)
					items = new TBBindingList<ITBBindingListItem>();
				return items;
			} 
		}
		
		//-------------------------------------------------------------------------
		public DataManagerSource(string tableName, string dataManagerName, string columnNameForKey, string columnNameForDescription)
			: base(tableName, dataManagerName)
		{
			this.columnNameForKey = columnNameForKey;
			this.columnNameForDescription = columnNameForDescription;
		}

		//-------------------------------------------------------------------------
		protected override void Dispose(bool value)
		{
			base.Dispose(value);

			if (items != null)
			{
				items.Clear();
				items = null;
			}
		}

		//-------------------------------------------------------------------------
		protected virtual void OnValidItem(ValidItemEventArgs e)
		{
			if (ValidItem != null)
				ValidItem(this, e);
		}

		//-------------------------------------------------------------------------
		public bool ContainsListCollection
		{
			get { return true; }
		}

		//-------------------------------------------------------------------------
		public IList GetList()
		{
			return this.Items;
		}

		//-------------------------------------------------------------------------
		public void Load()
		{ 
			//if (!enabled)
			//	return;
	
			IRecordField keyField;
			IRecordField descriptionField;
			TBBindingListItem item;
			ValidItemEventArgs args;
			Items.Clear();
			while(Read() == ReadResult.Found)
			{
				keyField = Record.GetField(columnNameForKey);
				descriptionField = Record.GetField(columnNameForDescription);

				if (keyField == null || descriptionField == null)
					return;
		
				item = new TBBindingListItem(keyField.Value, descriptionField.Value.ToString());
				args = new ValidItemEventArgs(item);
				
				OnValidItem(args);

				if (args.IsValid)
					Items.Add(item);
			}
		}
	}
}
