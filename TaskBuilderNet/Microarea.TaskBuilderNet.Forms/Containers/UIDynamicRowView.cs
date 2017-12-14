using System;
using System.Collections.Generic;
using System.ComponentModel;
using Microarea.Framework.TBApplicationWrapper;
using Microarea.TaskBuilderNet.Core.EasyBuilder;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Interfaces.Model;

namespace Microarea.TaskBuilderNet.Forms.Containers
{

	public partial class UIDynamicRowView : UIForm
	{
		MDBTSlaveBuffered dbt;

		//---------------------------------------------------------------------
		public UIDynamicRowView(IntPtr hWndOwner, IntPtr documentPtr, IntPtr dbtPtr)
		{
			while (hWndOwner != IntPtr.Zero && (ExternalAPI.GetWindowLong(hWndOwner, (int)ExternalAPI.GetWindowLongIndex.GWL_STYLE) & (int)ExternalAPI.WindowStyles.WS_CHILD) == (int)ExternalAPI.WindowStyles.WS_CHILD)
				hWndOwner = ExternalAPI.GetParent(hWndOwner);
			ExternalAPI.SetWindowLong(Handle, (int)ExternalAPI.GetWindowLongIndex.GWL_HWNDPARENT, (int)hWndOwner);

			dbt = new MyDBT(dbtPtr);
			dbt.SetDBTObjectProxy();
			dbt.CurrentRowChanged += new EventHandler<RowEventArgs>(dbt_CurrentRowChanged);
			this.Document = new MAbstractFormDoc(documentPtr);

			InitializeComponent();
			dynamicView.CreateAutomaticControls(dbt);

			BindingContext[dbt.BindableDataSource].Position = dbt.CurrentRow;
		}

		//---------------------------------------------------------------------
		void dbt_CurrentRowChanged(object sender, RowEventArgs e)
		{
			BindingContext[dbt.BindableDataSource].Position = e.RowNumber;
		}
	}

	//================================================================================================================
	public class MyDBT : MDBTSlaveBuffered
	{
		//---------------------------------------------------------------------
		public MyDBT(IntPtr dbtPtr)
			: base(dbtPtr)
		{
		}

		//---------------------------------------------------------------------
		public override System.Collections.IList Rows
		{
			get
			{
				if (rows == null)
					rows = new MyRecordBindingList<MSqlRecord>(Record);
				return rows;

			}
		}
	}

	//================================================================================================================
	public class MyRecordBindingList<T> : BindingList<T>, ITypedList
		where T : IRecord
	{
		PropertyDescriptorCollection propertyDescriptors;
		private IRecord record;
		private string listName;

		//---------------------------------------------------------------------
		public MyRecordBindingList(IRecord Record)
		{
			this.record = Record;
		}

		//---------------------------------------------------------------------
		/// <summary>
		/// Returns the property descriptors collection
		/// </summary>
		public PropertyDescriptorCollection GetItemProperties(PropertyDescriptor[] listAccessors)
		{
			if (propertyDescriptors == null)
			{
				List<PropertyDescriptor> properties = new List<PropertyDescriptor>();
				//aggiungo le proprietà fittizie del SqlRecord corrispondenti ai suoi campi
				for (int i = 0; i < record.Fields.Count; i++)
				{
					IRecordField field = (IRecordField)record.Fields[i];
					properties.Add(new RecordFieldPropertyDescriptor<MSqlRecord>(field.Name, field.DataObj.Value.GetType()));
				}
				propertyDescriptors = new PropertyDescriptorCollection(properties.ToArray());
			}
			return propertyDescriptors;
		}

		//---------------------------------------------------------------------
		/// <summary>
		/// Returns the list name
		/// </summary>
		public string GetListName(PropertyDescriptor[] listAccessors)
		{
			if (listName == null)
			{
				listName = typeof(T).Name;
			}
			return listName;
		}
	}

	//================================================================================================================
	public class DynamicRowViewLoader
	{
		//---------------------------------------------------------------------
		static DynamicRowViewLoader()
		{
			StaticFunctions.onShowRowView = new StaticFunctions.ShowRowView(ShowRowView);
		}

		//---------------------------------------------------------------------
		static bool ShowRowView(IntPtr hWndOwner, IntPtr documentPtr, IntPtr dbtPtr)
		{
			UIDynamicRowView form = new UIDynamicRowView(hWndOwner, documentPtr, dbtPtr);
			form.Show();
			return true;
		}
	}
}
