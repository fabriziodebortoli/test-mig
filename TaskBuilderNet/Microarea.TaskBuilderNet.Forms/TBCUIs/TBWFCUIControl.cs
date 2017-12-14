using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using System.Windows.Forms;
using Microarea.Framework.TBApplicationWrapper;
using Microarea.TaskBuilderNet.Core.ComponentModel;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Forms.Controls;
using Microarea.TaskBuilderNet.Forms.DataBinding;
using Microarea.TaskBuilderNet.Forms.InputStrategies;
using Microarea.TaskBuilderNet.Interfaces;
using Microarea.TaskBuilderNet.Interfaces.Model;
using Microarea.TaskBuilderNet.Interfaces.View;
using Microarea.TaskBuilderNet.Model.TBCUI;

namespace Microarea.TaskBuilderNet.Forms
{
	//================================================================================================================
	/// <summary>
	///   Tb Window Control 
	/// </summary>
	internal class TBWFCUIControl : TBCUIControl
	{
		//---------------------------------------------------------------------------
		/// <summary>
		/// </summary>
		internal TBWFCUIControl(IUIControl ctrl, NameSpaceObjectType nameSpaceType)
			:
			base(ctrl, nameSpaceType)
		{
		}

		/// <summary>
		/// </summary>
		//-------------------------------------------------------------------------
		protected override ITBInputStrategy CreateInputStrategy()
		{
			return new TBInputStrategy(this);
		}

		/// <summary>
		/// </summary>
		//-------------------------------------------------------------------------
		protected override ITBUIExtenderProvider CreateDiagnosticProvider()
		{
			return new TBWinDiagnosticExtender(this);
		}
		/// <summary>
		/// </summary>
		//-------------------------------------------------------------------------
		protected override ITBHotLinkUIProvider CreateHotLinkUIProvider()
		{
			return new TBWinHotLinkExtender(this);
		}

		/// <summary>
		/// </summary>
		//-------------------------------------------------------------------------
		protected override ITBHyperLinkUIProvider CreateHyperLinkUIProvider()
		{
			return new TBWinHyperLinkExtender(this);
		}

		//-------------------------------------------------------------------------
		protected override IUIExtendersManager CreateExtendersManager()
		{
			return new WFUIExtendersManager(this);
		}

		//---------------------------------------------------------------------------
		protected override void CreateDefaultBindingList()
		{
			if (DataObj == null)
				return;

			//se ho già un datasource appiccicato, non faccio niente
			ITBDataSource tbDataSource = Component as ITBDataSource;
			if (tbDataSource == null || tbDataSource.DataSource != null)
				return;
	
			TBBindingList<TBBindingListItem> list = new TBBindingList<TBBindingListItem>();
			tbDataSource.ValueMember = TBBindingListItem.KeyPropertyName;

			// se c'è un hotlink vince
			if (DataObj != null && DataObj.CurrentHotLink != null)
			{
				tbDataSource.DataSource = DataBindingHelper.CreateHotlinkBindingList(DataObj.CurrentHotLink);
				tbDataSource.DisplayMember = TBBindingListItem.KeyPropertyName;
				return;
			}

			// se no controllo gli enumerativi
			if (DataObj.DataType.IsEnum)
			{
				tbDataSource.DataSource = DataBindingHelper.CreateEnumBindingList(DataObj);
				tbDataSource.DisplayMember = TBBindingListItem.DescriptionPropertyName;
				return;
			}
		}

		//-------------------------------------------------------------------------
		public void AddTbBinding<TRecord, TProperty>(IUIControl control, string controlPropertyName, TRecord record, Expression<Func<TRecord, TProperty>> property) where TRecord : IRecord
		{
			TBCUIControl.EnsureValidRecord(record);

			PropertyInfo pi = ReflectionUtils.GetPropertyInfo<TRecord, TProperty>(record, property);
			AddTbBinding(control, controlPropertyName, pi.Name, record, pi);
		}

		//-------------------------------------------------------------------------
		public void AddTbBinding(IUIControl control, string controlPropertyName, IDataManager dataManager, string fieldName)
		{
			object dataSource = dataManager.BindableDataSource;
			AddTbBinding(control, controlPropertyName, fieldName, dataSource);
		}

		//-------------------------------------------------------------------------
		protected override void AddTbBinding(IUIControl control, string controlPropertyName, string fieldName, object dataSource, PropertyInfo pi)
		{
			Control c = control as Control;

			Control layoutTarget = c.Parent;
			layoutTarget.SuspendLayout();

			MDataObj dataObj = null;
			IRecord rec = dataSource as IRecord;
			if (pi != null)
			{
				object obj = pi.GetValue(dataSource, null);
				dataObj = obj as MDataObj;
			}
			else if (rec != null)
			{
				IRecordField field = rec.GetField(fieldName);
				dataObj = field == null ? null : (MDataObj)field.DataObj;
			}
			if (dataObj == null)
			{
				CurrencyManager cm = c.BindingContext[dataSource] as CurrencyManager;
				if (cm != null && cm.Position != -1 && rec != null)
				{
					IRecordField field = rec.GetField(fieldName);
					dataObj = field == null ? null : (MDataObj)field.DataObj;
				}
			}
			AttachDataObj(dataObj, Document);

			TBCUIControl tbControl = control.CUI as TBCUIControl;

			TBBinding binding = new TBBinding(controlPropertyName, dataSource, fieldName, true, DataSourceUpdateMode.OnValidation, tbControl == null ? null : tbControl.Formatter as IFormatProvider);
			binding.TBControl = tbControl;
			c.DataBindings.Add(binding);

			if (dataSource is IBindingList)
			{
				ListCurrencyManager mng = new ListCurrencyManager(fieldName, binding, control, c.BindingContext[dataSource]);
				// il BindingContext del control è quello di default della container superiore
				c.BindingContext[dataSource].PositionChanged += new EventHandler(mng.PositionChanged);
				c.Disposed += new EventHandler(mng.ControlDisposed);
			}

			layoutTarget.ResumeLayout();
		}

		//-------------------------------------------------------------------------
		public static IUIControl CreateControl(Type dataObjtype)
		{
			if (dataObjtype == typeof(MDataEnum))
			{
				return new UISingleSelectionDropDownList();
			}
			else if (dataObjtype == typeof(MDataBool))
			{
				return new UICheckBox();
			}
			else if (dataObjtype == typeof(MDataDate))
			{
				return new UIDateTimePicker();
			}
			else
			{
				return new UITextBox();
			}
		}
	}

	//-------------------------------------------------------------------------
	internal class ListCurrencyManager
	{
		PropertyInfo pi;
		IRecord current;
		TBBinding binding;
		IUIControl control;
		string fieldName;
		BindingManagerBase bindingManager;
		bool propertyTested;

		//-------------------------------------------------------------------------
		internal ListCurrencyManager(string fieldName, TBBinding binding, IUIControl control, BindingManagerBase bindingManager)
		{
			this.fieldName = fieldName;
			this.binding = binding;
			this.control = control;
			this.bindingManager = bindingManager;
		}

		//-------------------------------------------------------------------------
		internal void ControlDisposed(object sender, EventArgs e)
		{
			bindingManager.PositionChanged -= new EventHandler(PositionChanged);
			((Control)control).Disposed -= new EventHandler(ControlDisposed);
		}

		//-------------------------------------------------------------------------
		internal void PositionChanged(object sender, EventArgs e)
		{
			if (((Control)control).Parent == null)
				return;

			CurrencyManager cm = (CurrencyManager)sender;

			TBWFCUIControl cui = control.CUI as TBWFCUIControl;
			if (cm.Position == -1)
			{
				cui.DetachDataObj();
				binding.TBControl = control.CUI as TBCUIControl;
				return;
			}
			current = cm.Current as IRecord;
			if (current != null)
			{
				MDataObj dataObj = null;
				if (!propertyTested)
				{
					pi = current.GetType().GetProperty(fieldName, BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance);
					propertyTested = true;
				}

				if (pi != null)
				{
					dataObj = pi.GetValue(current, null) as MDataObj;
				}
				else
				{
					IRecordField field = current.GetField(fieldName);
					if (field != null)
						dataObj = field.DataObj as MDataObj;
				}
				Debug.Assert(dataObj != null);
				cui.AttachDataObj(dataObj, cui.Document);
				binding.TBControl = control.CUI as TBCUIControl;
			}
		}
	}
}