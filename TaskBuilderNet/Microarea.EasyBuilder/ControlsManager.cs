using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using Microarea.EasyBuilder.ComponentModel;
using Microarea.Framework.TBApplicationWrapper;
using Microarea.TaskBuilderNet.Core.EasyBuilder;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Interfaces.Model;

namespace Microarea.EasyBuilder
{
	//=========================================================================
	/// <summary>
	/// Responsabile della sincronizzazione dell'object model in memoria e
	/// del code namespace che fa da modello.
	/// </summary>
	internal class ControlsManager : ModuleObjectsManager
	{
		private FormEditor editor;

		private static readonly string propertyZIndex = GetPropertyZIndex();
		private static readonly string propertyTabStop = GetPropertyTabStop();
		private static readonly string propertyVisible = GetPropertyVisible();

		//---------------------------------------------------------------------
		private static string GetPropertyZIndex()
		{
			BaseWindowWrapper bww = null; //usato solo per recuperare il nome della property, può essere nullo
			//la stringa relativa alla proprietà "Name"
			return ReflectionUtils.GetPropertyName(() => bww.TabOrder);
		}
		//---------------------------------------------------------------------
		private static string GetPropertyTabStop()
		{
			BaseWindowWrapper bww = null; //usato solo per recuperare il nome della property, può essere nullo
			//la stringa relativa alla proprietà "Name"
			return ReflectionUtils.GetPropertyName(() => bww.TabStop);
		}
		//---------------------------------------------------------------------
		private static string GetPropertyVisible()
		{
			BaseWindowWrapper bww = null; //usato solo per recuperare il nome della property, può essere nullo
			//la stringa relativa alla proprietà "Name"
			return ReflectionUtils.GetPropertyName(() => bww.Visible);
		}

		//---------------------------------------------------------------------
		public ControlsManager(FormEditor editor)
		{
			if (editor == null)
				throw new ArgumentNullException("editor");

			this.editor = editor;
			//azzero la mappa delle proprietà originarie se cambia il controller
			this.editor.ControllerChanged += new EventHandler((object sender, EventArgs e) => this.OriginalProperties.Clear());
		}

		//---------------------------------------------------------------------
		protected internal override void ComponentChanged(object sender, ComponentChangedEventArgs e)
		{
			base.ComponentChanged(sender, e);

			if (e == null)
				return;

			EasyBuilderComponent component = e.Component as EasyBuilderComponent;
			if (
				component == null ||
				e.Member == null ||
				String.IsNullOrWhiteSpace(e.Member.Name) ||
				e.Member is TBEventPropertyDescriptor
				)
				return;

			string name = e.Member.Name;

			//se e` cambiato anche solo una virgola di un MLocalSqlRecordItem, devo rigenerare
			//il controller perche cambiano le proprieta` tipizzate
			if (e.Component is MLocalSqlRecordItem && component.Site != null)
			{
				component.Site.Name = component.SerializedName;
				editor.RefreshDocumentClassAndSelectObject(component);
				return;
			}

			if (propertyName == name && component.Site != null)
			{
				//se è cambiato il ControlName, dobbiamo cambiare anche il Site.Name dell'oggetto
				//in modo che in serializzazione venga creato il control con il nome giusto.
				component.Site.Name = component.SerializedName;
				if (component.Site.Container != null)
					editor.UpdateObjectViewModel(component);
			}

			if (
				(propertyZIndex == name || propertyTabStop == name) &&
                component != null && component.Site != null &&
				component.Site.Container != null
				)
			{
				//se è cambiato lo ZIndex, dobbiamo aggiornare l'ordine del tree dell'object model
				editor.UpdateObjectViewModel(component);
				//se sto editando graficamente il tab order, devo riaggiornare la vista
				if (editor.EditTabOrder)
					editor.InvalidateEditor();
			}

			if (name == propertyVisible)
				editor.UpdateObjectViewModel(component);		
		}

		//---------------------------------------------------------------------
		protected internal override void ComponentAdded(object sender, ComponentEventArgs e)
		{
			base.ComponentAdded(sender, e);

			IComponent addedComponent = e.Component;
			if (addedComponent == null)
				return;

			IContainer parentContainer = sender as IContainer;
			EasyBuilderComponent addedWrapper = addedComponent as EasyBuilderComponent;

			if (parentContainer != null && addedWrapper != null)
			{
				string serializedName = addedWrapper is IDocumentDataManager ? EasyBuilderSerializer.DocumentPropertyName : addedWrapper.SerializedName;
				addedWrapper.Site = new TBSite(addedWrapper, parentContainer, ((TBSite)Site).Editor, serializedName);
			}

			editor.UpdateObjectViewModel(addedComponent);
		}

		//---------------------------------------------------------------------
		protected internal override void ComponentRemoved(object sender, ComponentEventArgs e)
		{
			base.ComponentRemoved(sender, e);

			editor.UpdateObjectViewModel(e.Component);
		}
	}
}
