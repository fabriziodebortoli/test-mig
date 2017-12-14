using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using Microarea.EasyBuilder.ComponentModel;
using Microarea.TaskBuilderNet.Core.EasyBuilder;
using Microarea.TaskBuilderNet.Core.Generic;

namespace Microarea.EasyBuilder
{
	internal class OriginalProperties : Dictionary<string, object>
	{
	}
	//=========================================================================
	/// <summary>
	/// Responsabile della sincronizzazione dell'object model in memoria e
	/// del code namespace che fa da modello.
	/// </summary>
	internal class ModuleObjectsManager : IComponent
	{
		internal static readonly string propertyName = GetPropertyName();
		//---------------------------------------------------------------------
		private static string GetPropertyName()
		{
			EasyBuilderComponent cmp = null; //usato solo per recuperare il nome della property, può essere nullo
			//la stringa relativa alla proprietà "Name"
			return ReflectionUtils.GetPropertyName(() => cmp.Name);
		}

		public event EventHandler Disposed;

		private ISite site;
		private Dictionary<object, OriginalProperties> originalProperties = new Dictionary<object, OriginalProperties>();
		private IComponentChangeService service;

		//---------------------------------------------------------------------
		internal protected Dictionary<object, OriginalProperties> OriginalProperties
		{
			get { return originalProperties; }
		}

		//---------------------------------------------------------------------
		protected virtual void OnDisposed()
		{
			if (Disposed != null)
				Disposed(this, EventArgs.Empty);
		}

		//---------------------------------------------------------------------
		public virtual ISite Site
		{
			get
			{
				return site;
			}
			set
			{
				site = value;
			}
		}

		//---------------------------------------------------------------------
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		//---------------------------------------------------------------------
		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
				UnlistenToComponentEvents();

			OnDisposed();
		}

		//---------------------------------------------------------------------
		public virtual void ListenToComponentEvents(IComponentChangeService service)
		{
			if (service == null)
				return;

			UnlistenToComponentEvents();

			this.service = service;
			service.ComponentAdded += new ComponentEventHandler(ComponentAdded);
			service.ComponentRemoved += new ComponentEventHandler(ComponentRemoved);
			service.ComponentChanged += new ComponentChangedEventHandler(ComponentChanged);
		}

		//---------------------------------------------------------------------
		public virtual void UnlistenToComponentEvents()
		{
			if (service == null)
				return;

			service.ComponentAdded -= new ComponentEventHandler(ComponentAdded);
			service.ComponentRemoved -= new ComponentEventHandler(ComponentRemoved);
			service.ComponentChanged -= new ComponentChangedEventHandler(ComponentChanged);
		}

		//---------------------------------------------------------------------
		protected internal virtual void ComponentChanged(object sender, ComponentChangedEventArgs e)
		{
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

			// Name non va serializzata perchè è sempre nei costruttori
			if (name == propertyName)
				return;

			//per ogni proprietà di ogni oggetto tengo il valore originario (creato a richiesta)
			//se successivi cambiamenti di una proprietà la riportano al valore originario,
			//lo posso togliere dalle changed properties
			//i valori originari vengono azzerati quando cambia il controller (tipicamente, dopo ogni salvataggio e all'inizio)
			object originalValue = null;
			OriginalProperties op;
			if (!this.OriginalProperties.TryGetValue(component, out op))
			{
				op = new OriginalProperties();
				this.OriginalProperties[component] = op;
			}
			if (!op.TryGetValue(name, out originalValue))
				op[name] = e.OldValue;

			object newValue = e.Member is PropertyDescriptor ? ((PropertyDescriptor)e.Member).GetValue(e.Component) : e.NewValue;
			//se il valore è uguale a quello originario, lo tolgo dalle changed properties, non ha senso serializzarlo!
			//Lo tolgo anche se il nuovo valore impostato è NULL, anche in questo caso non ha senso serializzarlo.
			if ((originalValue != null && originalValue.Equals(newValue)) || newValue == null)
				component.RemoveChangedProperty(name);
			else
				component.AddChangedProperty(name);
		}

		//---------------------------------------------------------------------
		protected internal virtual void ComponentAdded(object sender, ComponentEventArgs e)
		{
			
		}

		//---------------------------------------------------------------------
		protected internal virtual void ComponentRemoved(object sender, ComponentEventArgs e)
		{
			
		}
	}
}
