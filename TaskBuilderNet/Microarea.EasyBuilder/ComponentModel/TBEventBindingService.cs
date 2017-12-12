using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Globalization;
using System.Text;
using ICSharpCode.NRefactory.CSharp;
using Microarea.TaskBuilderNet.Core.EasyBuilder;
using Microarea.TaskBuilderNet.Core.Generic;


namespace Microarea.EasyBuilder.ComponentModel
{
	//================================================================================
	class TBEventBindingService : EventBindingService, IEventBindingService
	{
		private Sources sources;
		private ISite site;

		//--------------------------------------------------------------------------------
		public TBEventBindingService(Sources sources, ISite site)
			: base(site)
		{
			this.sources = sources;
			this.site = site;
		}

		/// <summary>
		/// Crea un nome univoco per l'evento selezionato(se ne trova uno con lo stesso nome
		/// viene generato un nome con un postfisso numerico progressivo
		/// </summary>
		/// <param name="component"></param>
		/// <param name="e"></param>
		/// <returns></returns>
		//--------------------------------------------------------------------------------
		protected override string CreateUniqueMethodName(IComponent component, EventDescriptor e)
		{
            EasyBuilderEventDescriptor eventDescr = e as EasyBuilderEventDescriptor;
            if (eventDescr == null)
                throw new NotSupportedException("Unable to create unique name");

            IComponent theComponent = eventDescr.GetComponent(component);

            if (theComponent == null)
				throw new ArgumentNullException("component");
			if (e == null)
				throw new ArgumentNullException("e");

			int counter = 0;
            string methodName = ReflectionUtils.GenerateName(theComponent, e);
			string currentMethodName = methodName;

			while (sources.FindCodeMemberMethod(currentMethodName, e) != null)
				currentMethodName = String.Concat(methodName, ++counter);

			return currentMethodName;
		}

		/// <summary>
		/// Ritorna la lista dei metodi aventi la stessa signature del metodo selezionato.
		/// In questo modo "dovrebbe" essere popolato un menù a tendina nella property grid
		/// in modo da favorire la selezione di metodi già esistenti
		/// </summary>
		/// <param name="e"></param>
		/// <returns></returns>
		//--------------------------------------------------------------------------------
		protected override System.Collections.ICollection GetCompatibleMethods(EventDescriptor e)
		{
			if (e == null)
				throw new ArgumentNullException("e");

            return sources == null ? null : sources.FindCompatibleMethodsName(e);
		}

		/// <summary>
		/// Ritorna l'event descriptor relativo alla proprietà correntemente selezionata
		/// </summary>
		/// <param name="property"></param>
		/// <returns></returns>
		//--------------------------------------------------------------------------------
		public EventDescriptor GetEvent(PropertyDescriptor property)
		{
			TBEventPropertyDescriptor tbPropDescr = property as TBEventPropertyDescriptor;
			return (tbPropDescr != null) ? tbPropDescr.EventDescriptor : null;
		}

		/// <summary>
		/// Ritorna la collection di tutti i PropertyDescriptor a partire da una lista di eventi.
		/// Ritorna la lista di eventi cacheata, per cui se un evento è già stato esplorato
		/// la ricerca è più efficiente
		/// </summary>
		/// <param name="events"></param>
		/// <returns></returns>
		//--------------------------------------------------------------------------------
		internal PropertyDescriptorCollection GetEventProperties(EventDescriptorCollection events)
		{
			TBEventPropertyDescriptor dummy = null;
			string hashCode = null;

			List<PropertyDescriptor> list = new List<PropertyDescriptor>(events.Count);
			IDictionaryService dictionary = (IDictionaryService)site.GetService(typeof(IDictionaryService));
			EasyBuilderComponent ebComponent = this.site.Component as EasyBuilderComponent;
            EasyBuilderEventDescriptor ed = null;
			for (int i = 0; i < events.Count; i++)
			{
                ed = events[i] as EasyBuilderEventDescriptor;
                if (ed == null)
                    continue;

				TBEventFilterAttribute tbAttribute = (TBEventFilterAttribute)ed.Attributes[typeof(TBEventFilterAttribute)];
				if (tbAttribute != null && !ebComponent.CanShowEvent(ed.Name, tbAttribute))
					continue;

                TBEventPropertyDescriptor tbDescr = new TBEventPropertyDescriptor(ed);
				hashCode = GetEventDescriptorHashCode(ed);

				dummy = dictionary.GetValue(hashCode) as TBEventPropertyDescriptor;
				if (dummy == null)
				{
					dictionary.SetValue(hashCode, tbDescr);
					list.Add(tbDescr);
				}
				else
				{
                    EasyBuilderComponent theComponent = ed.GetComponent(ebComponent) as EasyBuilderComponent;
                  	bool found = false;
					System.Collections.ICollection coll = GetCompatibleMethods(ed);
					if (coll != null)
					{
						foreach (string name in coll)
						{
							if (
								sources.IsThereEventHandlerRegistrationStatement(
									name,
									ed.Name,
									theComponent
									)
								)
							{
								dummy.SetValue(theComponent, name);
								found = true;
								break;
							}
						}
					}
					if (!found)
                        dummy.ResetValue(theComponent);

					list.Add(dummy);
				}
			}

			return new PropertyDescriptorCollection(list.ToArray());
		}

		/// <summary>
		/// Ritorna la property descriptor relativo all'evento selezionato
		/// </summary>
		/// <param name="e"></param>
		/// <returns></returns>
		//--------------------------------------------------------------------------------
		protected PropertyDescriptor GetEventProperty(EventDescriptor e)
		{
			IDictionaryService dictionary = (IDictionaryService)site.GetService(typeof(IDictionaryService));
			return dictionary.GetValue(GetEventDescriptorHashCode(e)) as TBEventPropertyDescriptor;
		}

		//--------------------------------------------------------------------------------
		protected override bool ShowCode()
		{
			return false;
		}

		//--------------------------------------------------------------------------------
		protected override bool ShowCode(IComponent component, EventDescriptor e, string methodName)
		{
			MethodDeclaration method = sources.FindCodeMemberMethod(methodName, e);
			if (method != null)
				sources.OnCodeMethodEdit(new CodeMethodEditorEventArgs(method));

			return true;
		}

		//--------------------------------------------------------------------------------
		protected override bool ShowCode(int lineNumber)
		{
			return false;
		}
		/// <summary>
		/// Ritorna un hashcode a partire da un event descriptor in modo da evitare 
		/// omonimia qualora si lavori su property descriptori relativi ad oggetti diversi
		/// </summary>
		/// <param name="eventDesc"></param>
		/// <returns></returns>
		//--------------------------------------------------------------------------------
		private string GetEventDescriptorHashCode(EventDescriptor eventDesc)
		{
			StringBuilder builder = new StringBuilder(ReflectionUtils.GenerateName(site.Component, eventDesc));
			builder.Append(eventDesc.EventType.GetHashCode().ToString(CultureInfo.InvariantCulture));

			foreach (Type parameterType in ReflectionUtils.GetParametersTypes(eventDesc))
				builder.Append(parameterType.GetHashCode().ToString(CultureInfo.InvariantCulture));

			return builder.ToString();
		}

		#region IEventBindingService Members

		//--------------------------------------------------------------------------------
		PropertyDescriptorCollection IEventBindingService.GetEventProperties(EventDescriptorCollection events)
		{
			return GetEventProperties(events);
		}

		//--------------------------------------------------------------------------------
		bool IEventBindingService.ShowCode(IComponent component, EventDescriptor e)
		{
			TBEventPropertyDescriptor desc = GetEventProperty(e) as TBEventPropertyDescriptor;
			if (desc == null)
				return false;
			return this.ShowCode(component, e, desc.MethodName);
		}

		#endregion
	}
}
