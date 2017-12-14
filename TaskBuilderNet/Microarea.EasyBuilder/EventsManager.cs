using System;

using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms.Design;
using Microarea.EasyBuilder.ComponentModel;
using Microarea.EasyBuilder.Properties;
using Microarea.TaskBuilderNet.Core.EasyBuilder;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Interfaces.EasyBuilder;
using EBEventInfo = Microarea.TaskBuilderNet.Core.EasyBuilder.EventInfo;
using Microarea.TaskBuilderNet.Interfaces.Model;
using Microarea.Framework.TBApplicationWrapper;

using System.Collections.Generic;
using System.Diagnostics;
using ICSharpCode.NRefactory.CSharp;

namespace Microarea.EasyBuilder
{
	//=========================================================================
	/// <summary>
	/// Controls adding and removing event handler registrations to object
	/// model objects.
	/// </summary>
	/// <remarks>
	/// When adding an event handler registration, if the event handler does
	/// not exist in the source code, then it will be added to the
	/// DocumentController class
	/// </remarks>
	/// <seealso cref="Microarea.EasyBuilder.MVC.DocumentController"/>
	public class EventsManager : IDisposable
	{
		private IComponentChangeService service;
		private IServiceProvider serviceProvider;
		private Sources sources;

		readonly string notAllowedCharsPattern = "[^a-zA-Z0-9_]+";
		Regex notAllowedCharsRegex;
		

		//---------------------------------------------------------------------
		/// <summary>
		/// Initializes a new instance of the EventsManager.
		/// </summary>
		/// <param name="serviceProvider">A service provider to retrieve service.</param>
		/// <param name="sources">An istance of the class responsible for source code generation.</param>
		/// <seealso cref="Microarea.EasyBuilder.ControllerSources"/>
		internal EventsManager(IServiceProvider serviceProvider, Sources sources)
		{
			this.serviceProvider = serviceProvider;
			this.sources = sources;

			notAllowedCharsRegex = new Regex(notAllowedCharsPattern);
		}

		//---------------------------------------------------------------------
		/// <summary>
		/// Releases all resources used by the EventsManager
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		//---------------------------------------------------------------------
		/// <remarks/>
		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
				UnlistenToComponentEvents();
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
			TBEventPropertyDescriptor desc = e.Member as TBEventPropertyDescriptor;
			if (desc == null)//non si tratta di un evento
				return;

            IComponent component = desc.EventDescriptor.GetComponent(e.Component as IComponent);

			if (component == null)//non si tratta di un nostro component
				return;

			string newMethodName = e.NewValue as string;
			if (newMethodName == null)
				newMethodName = String.Empty;

			string oldMethodName = e.OldValue as string;
			if (oldMethodName == null)
				oldMethodName = String.Empty;

			//Se il vecchio nome è diverso da nullo e diverso dal nuovo nome allora rimuovo la sottoscrizione del vecchio
			if (
				!String.IsNullOrWhiteSpace(oldMethodName) &&
				String.Compare(oldMethodName, newMethodName, StringComparison.InvariantCulture) != 0
				)
				RemoveEventHandlerRegistration(component, desc.EventDescriptor, oldMethodName);

			//Se il nuovo nome è nullo allora non sottoscrivo nulla.
			if (String.IsNullOrWhiteSpace(newMethodName))
				return;

			if (ContainsInvalidChars(newMethodName))
			{
				desc.SetValue(component, oldMethodName);
				throw new ArgumentException(Resources.InvalidMethodName);
			}

			//Se il nuovo nome è uguale al vecchio allora non sottoscrivo nulla.
			if (String.Compare(oldMethodName, newMethodName, StringComparison.InvariantCulture) == 0)
				return;

			IModelRoot modelRoot = this.serviceProvider.GetService(typeof(IModelRoot)) as IModelRoot;

			if (modelRoot == null)
				return;

			if (!modelRoot.BelongsToObjectModel(component))
			{
				//Cancello l'eventuale valore che l'utente ha inserito nella cella della property grid.
				desc.SetValue(component, e.OldValue);

				IUIService uiService = this.serviceProvider.GetService(typeof(IUIService)) as IUIService;
				if (uiService != null)
				{
					uiService.ShowError(Resources.CannotAdEventHandlerBecauseObjectDoesNotBelongToObjectModel);
				}
				return;
			}

			AddEventHandlerAndRegistration(component as EasyBuilderComponent, desc.EventDescriptor, newMethodName);
		}

		//--------------------------------------------------------------------------------
		private bool ContainsInvalidChars(string newMethodName)
		{
			if (newMethodName == null || newMethodName.Trim().Length == 0)
			{
				return true;
			}
			Match aMatch = notAllowedCharsRegex.Match(newMethodName);
			if (aMatch.Success)
			{
				return true;
			}

			return Char.IsDigit(newMethodName[0]);
		}

		//--------------------------------------------------------------------------------
		private void RemoveEventHandlerRegistration(
			IComponent component,
			EventDescriptor e,
			string eventHandlerMethodName
			)
		{
			MethodDeclaration toBeRemovedMethod = sources.FindCodeMemberMethod(eventHandlerMethodName, e);
			if (toBeRemovedMethod == null)
				return;

			EasyBuilderComponent ebComponent = component as EasyBuilderComponent;
			sources.RemoveEventHandlerRegistration(toBeRemovedMethod, ebComponent);
		}

		//--------------------------------------------------------------------------------
		/// <summary>
		/// Adds an event handler to the DocumentController using the given
		/// methodName.
		/// Registers it to handle the event raised by the
		/// EasyBuilderComponent described by the EventDescriptor
		/// </summary>
		/// <param name="component">
		/// The EasyBuilderComponent instance raising the event to be handled.
		/// </param>
		/// <param name="e">
		/// The EventDescriptor describing the event to be handled.
		/// </param>
		/// <param name="methodName">
		/// The method name for the event handler to be created if needed.
		/// </param>
		/// <exception cref="System.ArgumentNullException">
		/// The EasyBuilderComponent is null.
		/// </exception>
		/// <exception cref="System.InvalidOperationException">
		/// The EasyBuilderComponent is not an instance of DocumentController and have no ISite or no IContainer
		/// </exception>
		/// <seealso cref="System.ComponentModel.ISite"/>
		/// <seealso cref="System.ComponentModel.IContainer"/>
		/// <seealso cref="Microarea.TaskBuilderNet.Core.EasyBuilder.EasyBuilderComponent"/>
		/// <seealso cref="Microarea.EasyBuilder.MVC.DocumentController"/>
		/// <seealso cref="System.ComponentModel.EventDescriptor"/>
		public void AddEventHandlerAndRegistration(
			EasyBuilderComponent component,
			EventDescriptor e,
			string methodName
			)
		{
			if (component == null)
				throw new ArgumentNullException("component");

			bool codeChanged = false;
			//Il controller è il nodo radice del nostro object model per cui è l'unico autorizzato a non aver parent.
			//Se il componente non è un controller allora non può non avere un site e un container.
			Type typeOfComponent = component.GetType();
			Type modelRootType = typeof(IModelRoot);
			if (
				typeOfComponent.GetInterface(modelRootType.FullName) == null &&
				(component.Site == null || component.Site.Container == null)
				)
				throw new InvalidOperationException("Cannot add an event handler to this object because it is not a part of the EasyBuilder object model.");

			MethodDeclaration eventHandler = null;
            string nameForTheSourceEvent = component.SerializedName;
            EasyBuilderExtenderEventDescriptor evt = e as EasyBuilderExtenderEventDescriptor;
            if (evt != null)
                nameForTheSourceEvent = string.Concat(component.ParentComponent.SerializedName, ".", component.SerializedName);

			StringBuilder sourceEventBuilder = new StringBuilder(String.Format("{0}.{1}", nameForTheSourceEvent, e.Name));
			EasyBuilderComponent ebComponent = component as EasyBuilderComponent;

			//Aggiungo il nome dell'evento all'elenco delle proprietà cambiate
			//in modo che il controllo venga serializzato.
			if (ebComponent == null)
				return;

			string ebComponentFullPath = ReflectionUtils.GetComponentFullPath(ebComponent);
			string sourceEvent = sourceEventBuilder.ToString();

			//Aggiunge il metodo al controller.
			TypeDeclaration controllerTypeDecl = EasyBuilderSerializer.GetControllerTypeDeclaration(sources.CustomizationInfos.UserMethodsCompilationUnit);
			eventHandler = sources.AddEventHandlerMethodToType(
				controllerTypeDecl,
				methodName,
				sourceEvent,
				EventDefaulter.GetDefaultEventCode(component, e),
				ReflectionUtils.GetEventArgsType(e),
				ref codeChanged
				);

			EBEventInfo ei = new EBEventInfo(sourceEvent, methodName, e.EventType.ToString(), ebComponentFullPath, controllerTypeDecl.Name, e.EventType.IsGenericType);
            var changeEventsSource = ebComponent as IChangedEventsSource;
            var relatedComponent = changeEventsSource.EventSourceComponent as EasyBuilderComponent;
			if (!relatedComponent.ContainsChangedEvent(ei))
			{
                relatedComponent.AddChangedEvent(ei);
			}
            relatedComponent.IsChanged = true;
            var parent = relatedComponent.Site?.Container as EasyBuilderComponent;
            if (parent != null)
            {
                parent.IsChanged = true;
            }
		}
	}
}
