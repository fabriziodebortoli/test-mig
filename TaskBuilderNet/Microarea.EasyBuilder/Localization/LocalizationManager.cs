using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Threading;
using Microarea.TaskBuilderNet.Core.EasyBuilder;
using Microarea.TaskBuilderNet.Core.Generic;

namespace Microarea.EasyBuilder.Localization
{
	//=========================================================================
	/// <summary>
	/// Controls localization for the customization.
	/// </summary>
	public class LocalizationManager : IDisposable
	{
		//Elenco dei nomi delle proprietà incompatbili con il databinding
		private readonly string[] dataBindingIncompatiblePropertiesName = new string[] { "Text" };
		private IComponentChangeService service;
		private Sources sources;

		/// <summary>
		/// Occurs when references to assembly are updated.
		/// </summary>
		public event EventHandler<EventArgs> LocalizationChanged;

		//-------------------------------------------------------------------------------
		private void OnLocalizationChanged()
		{
			if (LocalizationChanged != null)
				LocalizationChanged(this, new EventArgs());
		}

		//---------------------------------------------------------------------
		/// <summary>
		/// Initializes a new instance of the LocalizationManager.
		/// </summary>
		/// <param name="sources">An istance of the class responsible for source code generation.</param>
		/// <seealso cref="Microarea.EasyBuilder.ControllerSources"/>
		internal LocalizationManager(Sources sources)
		{
			this.sources = sources;
		}

		//---------------------------------------------------------------------
		/// <summary>
		/// Registers an event handler to the
		/// IComponentChangeService.ComponentChanged event.
		/// </summary>
		/// <param name="service">An istance of the IComponentChangeService
		/// to register the ComponentChanged event.</param>
		/// <seealso cref="System.ComponentModel.Design.IComponentChangeService"/>
		public void ListenToComponentEvents(IComponentChangeService service)
		{
			if (service == null)
				return;

			UnlistenToComponentEvents();

			this.service = service;
			service.ComponentChanged += new ComponentChangedEventHandler(ComponentChanged);
		}

		//---------------------------------------------------------------------
		/// <summary>
		/// Unregisters the event handler to the
		/// IComponentChangeService.ComponentChanged event registered
		/// with the call to the ListenToComponentEvents method.
		/// </summary>
		/// <seealso cref="System.ComponentModel.Design.IComponentChangeService"/>
		public void UnlistenToComponentEvents()
		{
			if (service == null)
				return;

			service.ComponentChanged -= new ComponentChangedEventHandler(ComponentChanged);
		}

		//---------------------------------------------------------------------
		void ComponentChanged(object sender, ComponentChangedEventArgs e)
		{
			PropertyDescriptor pDesc = e.Member as PropertyDescriptor;
			if (pDesc == null)
				return;

			EasyBuilderComponent ctrl = e.Component as EasyBuilderComponent;
			if (ctrl == null)
				return;
			
			bool isLocalizable = pDesc.IsLocalizable;
			bool isDataBinding = pDesc.PropertyType == typeof(Microarea.TaskBuilderNet.Interfaces.Model.IDataBinding);

			//se è cambiata la proprietà "Name", devo aggiungere tutte le property localizzabili del control alle
			//stringhe localizzabili
			string nameProperty = ReflectionUtils.GetPropertyName(() => ctrl.Name);
			if (pDesc.Name.CompareNoCase(nameProperty))
			{
				//cerco tutte le properties localizzabili
				Attribute[] attrs = new Attribute[]{ new LocalizableAttribute(true) };
				foreach (PropertyDescriptor prop in ctrl.GetProperties(attrs))
				{
					if (!prop.IsLocalizable)
						continue;

					//es ctrl_MParsedEdit1 diventa ctrl_MParsedEdit2
					string newName = string.Concat(ctrl.Site.Name,'.', prop.Name );
					string oldSiteName = ctrl.Site.Name.Replace(e.NewValue.ToString(), e.OldValue.ToString());
					string oldName = string.Concat(oldSiteName, '.', prop.Name);
					//Se la proprietà è localizzabile applico la gestione delle risorse.
                    sources.Localization.SubstituteDictionaryKey(oldName, newName);
				}

				OnLocalizationChanged();
				return;
			}

			if (!isLocalizable && !isDataBinding)
				return;

			if (isDataBinding)
			{
				//Se la proprietà cambata è il DataBinding devo rimuovere tutte le eventuali proprietà che ne sono affette
				//(per esempio il testo di un parsed edit). Infatti se un controllo è bindato allora il suo testo è quello dato
				//dal valore del data obj e quindi l'eventuale risorsa localizzata va eliminata.

				foreach (string propertyName in dataBindingIncompatiblePropertiesName)
				{
                    sources.Localization.RemoveLocalizableString(
						Thread.CurrentThread.CurrentUICulture.Name,
						String.Concat(ctrl.Site.Name, '.', propertyName),
						true
					);
					//La rimuovo anche dalle changed properties in modo che non venga serializzata
					ctrl.RemoveChangedProperty(propertyName);
				}
			}
			else if (isLocalizable)
			{
				//Se la proprietà è localizzabile applico la gestione delle risorse.
				sources.Localization.AddLocalizableString(
					Thread.CurrentThread.CurrentUICulture.Name,
					String.Concat(ctrl.Site.Name, '.', pDesc.Name),
					e.NewValue as string,
					false
					);
				
				OnLocalizationChanged();
			}
		}

		//---------------------------------------------------------------------
		/// <summary>
		/// Releases all resources used by the LocalizationManager
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this); ;
		}

		//---------------------------------------------------------------------
		/// <remarks/>
		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				UnlistenToComponentEvents();
				service = null;
			}
		}
	}
}
