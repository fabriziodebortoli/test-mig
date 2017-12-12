using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using Microarea.Framework.TBApplicationWrapper;
using Microarea.TaskBuilderNet.Core.EasyBuilder;
using Microarea.TaskBuilderNet.Core.Validation;
using Microarea.TaskBuilderNet.Interfaces;
using Microarea.TaskBuilderNet.Interfaces.Model;
using Microarea.TaskBuilderNet.Interfaces.Validation;
using Microarea.TaskBuilderNet.Interfaces.View;

namespace Microarea.TaskBuilderNet.Model.TBCUI
{
	//=========================================================================
	public abstract class TBCUIControl : TBCUI, ITBUIExtenderConsumer, ITBInputStrategyConsumer, ITBCUIControl
	{
		IDataBinding dataBinding;
		ITBFormatterProvider formatter;
		ITBInputStrategy inputStrategy;
		ITBValidationProvider rangeValidationProvider;
		IUIExtendersManager uiExtendersManager;
		ITBHotLinkUIProvider hklUIProvider;
		ITBHyperLinkUIProvider hyperLink;

		public event EventHandler BeforeValidating;

		public IUIControl			Control				{ get { return Component as IUIControl; } }
		public IUIExtendersManager	ExtendersManager	{ get { return uiExtendersManager; } }
		public ITBInputStrategy		InputStrategy		{ get { return inputStrategy; } set { inputStrategy = value; } }
	
		//---------------------------------------------------------------------
		public MDataObj DataObj
		{
			get
			{
				if (DataBinding != null)
					return dataBinding.Data as MDataObj;

				return null;
			}
		}

		//---------------------------------------------------------------------
		public override IMAbstractFormDoc Document
		{
			set
			{
				if (base.Document != null)
					base.Document.FormModeChanged -= new EventHandler<EventArgs>(DocumentFormModeChanged);

				base.Document = value;

				if (Document != null)
					Document.FormModeChanged += new EventHandler<EventArgs>(DocumentFormModeChanged);
			}
		}
		
		//---------------------------------------------------------------------
		public string FormatStyle
		{
			get { return Formatter == null ? string.Empty : ((TBFormatterProvider) Formatter).StyleName;}
			set { Formatter = TBFormatterProvider.Create(value, this.Namespace); }
		}

		//---------------------------------------------------------------------
		public virtual IDataBinding DataBinding
		{
			get { return dataBinding; }
			set
			{
				IDataBinding oldValue = dataBinding;
				dataBinding = value;

				Formatter = (DataObj != null) ? TBFormatterProvider.Create(DataObj) : null;
				
				DatabindingChanged(this, new DataBindingChangedEventArgs(oldValue));
			}
		}

		//---------------------------------------------------------------------
		public ITBFormatterProvider Formatter
		{
			get { return formatter; }
			set
			{
				ITBFormatterProvider oldValue = formatter;
				formatter = value;

				ApplyInputStrategy();
			}
		}

		//---------------------------------------------------------------------
		protected TBCUIControl(IUIControl control, NameSpaceObjectType nameSpaceType)
			:
			base(control, nameSpaceType)
		{
			control.Validating += new CancelEventHandler(UIValidating);
			control.Validated += new EventHandler(UIValidated);
			control.GotFocus += new EventHandler(CtrlGotFocus);
		}

		//---------------------------------------------------------------------
		protected override void Dispose(bool disposing)
		{
			if (uiExtendersManager != null)
			{
				uiExtendersManager.Dispose();
				uiExtendersManager = null;
			}

			//La DetachDataObj va fatta dopo la distruzione degli extender perche`
			//qualche extender potrebbe aver bisogno di qualcosa che viene ditrutto proprio li dentro
			//(per esempio DataBinding).
			DetachDataObj();

			if (Component != null)
			{
				Control.Validating -= new CancelEventHandler(UIValidating);
				Control.Validated -= new EventHandler(UIValidated);
				Control.GotFocus -= new EventHandler(CtrlGotFocus);
			}

			if (Document != null)
				Document = null;

			if (InputStrategy != null)
			{
				InputStrategy.Dispose();
				InputStrategy = null;
			}

			base.Dispose(disposing);
		}

		//---------------------------------------------------------------------
		protected abstract ITBInputStrategy			CreateInputStrategy();
		protected abstract ITBUIExtenderProvider	CreateDiagnosticProvider();
		protected abstract ITBHotLinkUIProvider		CreateHotLinkUIProvider();
		protected abstract ITBHyperLinkUIProvider	CreateHyperLinkUIProvider();
		protected abstract IUIExtendersManager		CreateExtendersManager();
		protected abstract void						CreateDefaultBindingList();

		//---------------------------------------------------------------------
		protected abstract void AddTbBinding
			(
			IUIControl c, 
			String controlPropertyName, 
			String fieldName,
			Object dataSource, 
			PropertyInfo pi
			);

		//---------------------------------------------------------------------
		public void AddTbBinding(IUIControl c, String controlPropertyName, IList records, String fieldName)
		{
			AddTbBinding(c, controlPropertyName, fieldName, records);
		}

		//---------------------------------------------------------------------
		public void AddTbBinding(IUIControl c, String controlPropertyName, IRecord record, String fieldName)
		{
			EnsureValidRecord(record);
			AddTbBinding(c, controlPropertyName, fieldName, record);
		}

		//---------------------------------------------------------------------
		public void AddTbBinding(IUIControl c, String controlPropertyName, String fieldName, Object dataSource)
		{
			PropertyInfo pi = dataSource.GetType().GetProperty(fieldName, BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance);
			AddTbBinding(c, controlPropertyName, fieldName, dataSource, pi);
		}

		//---------------------------------------------------------------------
		public void DetachDataObj()
		{
			if (Document != null && DataObj != null)
				((ITBValidationConsumer)Document).ValidationManager.Remove(DataObj, rangeValidationProvider);

			if (this.uiExtendersManager != null)
			{
				this.uiExtendersManager.ClearExtenders();
			}
			InputStrategy = null;

			if (DataBinding != null && DataObj != null)
			{
				if (DataObj != null)
				{
					DataObj.ReadOnlyChanged -= new EventHandler<EasyBuilderEventArgs>(EnabledChanged);
					DataObj.VisibleChanged -= new EventHandler<EasyBuilderEventArgs>(VisibleChanged);
					DataObj.HotLinkReattached -= new EventHandler<HotLinkReattachEventArgs>(HotLinkReattached);
				}
				DataBinding = null;
			}
		}

		//---------------------------------------------------------------------
		public virtual void AttachDataObj(MDataObj dataObj, IMAbstractFormDoc document)
		{
			if (Component == null)
				return;

			DetachDataObj();

			this.Document = document;

			if (dataObj != null)
			{
				DataBinding = new FieldDataBinding(dataObj);

				if (this.uiExtendersManager == null)
					this.uiExtendersManager = CreateExtendersManager();


				// gestione della InputStrategy
				IMaskedInput maskedInput = Component as IMaskedInput;
				if (DataBinding != null && DataBinding.Data != null && maskedInput != null)
					InputStrategy = CreateInputStrategy();

				dataObj.ReadOnlyChanged += new EventHandler<EasyBuilderEventArgs>(EnabledChanged);
				dataObj.VisibleChanged += new EventHandler<EasyBuilderEventArgs>(VisibleChanged);
				dataObj.HotLinkReattached += new EventHandler<HotLinkReattachEventArgs>(HotLinkReattached);

				if (dataObj.CurrentHotLink != null)
					InternalHotlinkReattach(dataObj, new HotLinkReattachEventArgs(null, dataObj.CurrentHotLink));

				//validatore range 
				ITBValidationConsumer validationConsumer = document as ITBValidationConsumer;
				if (validationConsumer != null)
				{
					TBValidationProvider rangeValidation = new TBValidationProvider();
					rangeValidation.ValidationTypes = ValidationTypes.Range;
					rangeValidation.ValidationModes = ValidationModes.FocusChange;
					rangeValidation.MinRange = dataObj.LowerValue;
					rangeValidation.MaxRange = dataObj.UpperValue;
					validationConsumer.ValidationManager.Add(dataObj, rangeValidation);
					rangeValidationProvider = rangeValidation;
				}

				ITBUIExtenderProvider diagnosticProvider = CreateDiagnosticProvider();
				if (diagnosticProvider != null)
					this.uiExtendersManager.AddExtender(diagnosticProvider);
			}

			CreateDefaultBindingList();

			OnInitialize();
		}

		//---------------------------------------------------------------------
		public override void SetModified()
		{
			base.SetModified();

			if (DataBinding != null)
				DataBinding.Modified = true;
		}

		//---------------------------------------------------------------------
		public virtual void UIValueToDataObj(MDataObj dataObj)
		{
			if (Security != null && !Security.IsVisible)
				return;

			ITBBindableObject control = Component as ITBBindableObject;

			if (dataBinding == null || control.UIValue == null)
				return;

			String unformattedData = (Formatter != null)
										? Formatter.UnformatData(control.UIValue.ToString())
										: control.UIValue.ToString();
			if (unformattedData != null && unformattedData.Trim().Length > 0)
				dataObj.SetValueFromString(unformattedData);
			else
				dataObj.Clear();
		}

		//---------------------------------------------------------------------
		public virtual void UIValidating(Object sender, CancelEventArgs e)
		{
			if (Document == null)
				return;
		
			if (Document.FormMode == FormModeType.Browse || DataObj == null)
				return;

			OnBeforeValidating();

			ITBBindableObject bindableObject = Component as ITBBindableObject;
			ITBValidationConsumer validationConsumer = (ITBValidationConsumer)Document;
			String s = bindableObject.UIValue == null ? "" : bindableObject.UIValue.ToString();
			if (validationConsumer != null && !validationConsumer.ValidationManager.Validate(DataObj, Formatter.UnformatData(s), ValidationModes.FocusChange))
				e.Cancel = true;
		}

		//---------------------------------------------------------------------
		public virtual void UIValidated(Object sender, EventArgs e)
		{
			SetModified();
		}

		////---------------------------------------------------------------------
		//public virtual string Format()
		//{
		//    if (dataBinding == null || !Security.IsVisible)
		//        return "?????";

		//    return (Formatter != null) ? Formatter.FormatData(DataObj) : "?????";
		//}

		//---------------------------------------------------------------------
		public void OnBeforeValidating()
		{
			if (BeforeValidating != null)
				BeforeValidating(Component, EventArgs.Empty);
		}

		//---------------------------------------------------------------------
		public void AddContextMenuItem(IMenuItemGeneric item)
		{
			if (this.UIManager != null)
			{
				this.UIManager.AddContextMenuItem(this.Component, item);
			}
		}

		//---------------------------------------------------------------------
		public void RemoveContextMenuItem(IMenuItemGeneric item)
		{
			if (this.UIManager != null)
			{
				this.UIManager.RemoveContextMenuItem(this.Component, item);
			}
		}

		//---------------------------------------------------------------------
		protected virtual void DocumentFormModeChanged(object sender, EventArgs e)
		{
			if (sender == null)
				return;

			IMAbstractFormDoc doc = sender as IMAbstractFormDoc;
			if (doc == null || doc.Batch)
				return;

			if (DataBinding == null)
			{
				Component.Enabled = false;
				return;
			}

			switch (doc.FormMode)
			{
				case FormModeType.Browse:
				case FormModeType.None:
				case FormModeType.Design:
					Component.Enabled = false;
					break;
				case FormModeType.New:
				case FormModeType.Edit:
				case FormModeType.Find:
					Component.Enabled = IsEnabled && this.uiExtendersManager.CanEnableExtendee();
					break;
				default:
					break;
			}
		}

		//---------------------------------------------------------------------
		protected virtual void EnabledChanged(object sender, EasyBuilderEventArgs e)
		{
			if (DataObj != null)
				Component.Enabled = !DataObj.ReadOnly;
		}

		//---------------------------------------------------------------------
		protected virtual void VisibleChanged(object sender, EasyBuilderEventArgs e)
		{
			if (DataObj != null)
				Component.Visible = ((MDataObj)DataObj).Visible;
		}

		//---------------------------------------------------------------------
		protected virtual void DatabindingChanged(object sender, DataBindingChangedEventArgs e)
		{
			if (Security != null)
				Security.DataBinding = dataBinding;

			ApplyInputStrategy();
		}

		//---------------------------------------------------------------------
		protected virtual void ApplyInputStrategy()
		{
			if (InputStrategy != null)
				InputStrategy.ApplyTo();
		}

		//---------------------------------------------------------------------
		protected virtual void CtrlGotFocus(object sender, EventArgs e)
		{
			ApplyInputStrategy();
		}
		//---------------------------------------------------------------------
		protected virtual void HotLinkReattached(object sender, HotLinkReattachEventArgs e)
		{
			MDataObj dataObj = (MDataObj)sender;
			InternalHotlinkReattach(dataObj, e);
		}
		
		//---------------------------------------------------------------------
		protected virtual void OnInitialize() { }
		
		//---------------------------------------------------------------------
		void InternalHotlinkReattach(MDataObj dataObj, HotLinkReattachEventArgs e)
		{
			ITBValidationConsumer consumer = Document as ITBValidationConsumer;
			if (e.OldValue != null)
			{
				if (consumer != null)
				{
					consumer.ValidationManager.Remove(
						dataObj,
						this.hklUIProvider.ValidationProvider
						);

					this.uiExtendersManager.RemoveExtender(this.hklUIProvider);

					this.uiExtendersManager.RemoveExtender(this.hyperLink);
				}
			}

			if (e.NewValue != null)
			{
				this.hklUIProvider = CreateHotLinkUIProvider();
				this.uiExtendersManager.AddExtender(this.hklUIProvider);

				if (consumer != null)
				{
					consumer.ValidationManager.Add(
						dataObj,
						this.hklUIProvider.ValidationProvider,
						((IMHotLink)dataObj.CurrentHotLink).ValidationProvider
						);
				}

				this.hyperLink = CreateHyperLinkUIProvider();
				this.uiExtendersManager.AddExtender(this.hyperLink);
			}
		}

		//---------------------------------------------------------------------
		[Conditional("DEBUG")]
		public static void EnsureValidRecord(IRecord record)
		{
			EasyBuilderComponent mRecord = record as EasyBuilderComponent;
			if (mRecord != null)
			{
				IDocumentSlaveBufferedDataManager parentDbt = mRecord.ParentComponent as IDocumentSlaveBufferedDataManager;
				bool isBufferedPrototype = parentDbt != null && record == parentDbt.Record;
				System.Diagnostics.Debug.Assert(!isBufferedPrototype, "OOOOPS!! You are binding a DBTslavebuffered prototype Record, are you sure?");
			}
		}

	}
}
