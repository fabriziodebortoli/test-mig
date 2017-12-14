using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using Microarea.TaskBuilderNet.Core.DiagnosticManager;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.Console.Core.EventBuilder
{
	/// <summary>
	/// EventsBuilder
	/// Contiene gli array degli eventi e dei metodi collegati agli eventi
	/// </summary>
	//=========================================================================
	public class EventsBuilder
	{
		#region Variaibli private

		//Array per gli eventi e i metodi dei plugIns e Console
		//---------------------------------------------------------------------
		private ArrayList			eventsList			= new ArrayList();
		private ArrayList			methodsList			= new ArrayList();
		//Classe per la gestione della diagnostica
		//---------------------------------------------------------------------
		private Diagnostic diagnostic	= new Diagnostic("Library.EventBuilder");
		
		//StringheStatiche
		//---------------------------------------------------------------------
		private static string       AllPlugIns			= "All";

		#endregion

		#region Proprieta'

		//Gestione della diagnostica
		//---------------------------------------------------------------------
		public Diagnostic Diagnostic { get { return diagnostic;}}	

		/// EventsList - Lista degli eventi caricati by reflection
		//---------------------------------------------------------------------
		public ArrayList EventsList	   { get { return eventsList; } set { eventsList = value;}	}
		
		
		/// MethodsList - Lista dei metodi caricati by reflection
		//---------------------------------------------------------------------
		public ArrayList MethodsList { get { return methodsList; } set { methodsList = value; }	}

		#endregion

		#region Costruttore (vuoto)

		//Costruttore
		//---------------------------------------------------------------------
		public EventsBuilder() {}

		#endregion
		
		#region AddEvents - Carica gli eventi dell'assembly nella struttura EventsList

		/// <summary>
		/// AddEvents
		/// Carica gli eventi dell'assembly nella struttura EventsList
		/// </summary>
		/// <param name="assemblyObj"></param>
		/// <param name="assemblyType"></param>
		//---------------------------------------------------------------------
		public void AddEvents(object assemblyObj, Type assemblyType)
		{
			EventDescriptorCollection events = TypeDescriptor.GetEvents(assemblyObj);
			if (events.Count > 0)
			{
				EventsList eventsList = new EventsList(assemblyType.FullName,events);
				this.eventsList.Add(eventsList);
			}
		}

		#endregion

		#region AddMethods - Carica i metodi specificati con il custom attribute nella struttura  MethodsExecuted

		/// <summary>
		/// AddMethods
		/// Carica i metodi specificati con il custom attribute nella struttura 
		/// MethodsExecuted
		/// </summary>
		/// <param name="assemblyType"></param>
		//---------------------------------------------------------------------
		public void AddMethods(Type assemblyType, ArrayList assemblies)
		{
			//trovo tutti i metodi definiti nell'assembly
			MethodInfo[] methods		= assemblyType.GetMethods();
			MethodsExecuted methodsList	= new MethodsExecuted();
			for (int i = 0; i < methods.Length; i++)
			{
				//aggiungo il metodo solo se c'è il custom attribute
				Object[] attributeHandler = methods[i].GetCustomAttributes(typeof(AssemblyEvent), true);
				if (attributeHandler.Length > 0)
				{
					string definedIntoAssembly = ((AssemblyEvent)attributeHandler[0]).AssemblyName;
					if (String.Compare(definedIntoAssembly, AllPlugIns, true, CultureInfo.InvariantCulture) != 0)
					{
						methodsList.AssemblyName = assemblyType.FullName;
						methodsList.MethodAfterEvent.Add(methods[i]);
						methodsList.DefinedIntoAssembly.Add(((AssemblyEvent)attributeHandler[0]).AssemblyName);
						methodsList.AfterEvent.Add(((AssemblyEvent)attributeHandler[0]).EventName);
					}
					else
					{
						//attacca questo metodo a tutti i plugIn
						for (int j = 0; j < assemblies.Count; j++)
						{
							methodsList.AssemblyName = assemblyType.FullName;
							methodsList.MethodAfterEvent.Add(methods[i]);
							methodsList.DefinedIntoAssembly.Add(definedIntoAssembly);
							methodsList.AfterEvent.Add(((AssemblyEvent)attributeHandler[0]).EventName);
							
						}
					}
				}
			}
			if (methodsList.Count() > 0) 
				this.methodsList.Add(methodsList);
		}

		#endregion

		#region AddMethodsConsole - Aggiunge i metodi definiti nella console, i quali devono essere intercettati dai PlugIn
 
		/// <summary>
		/// AddMethodsConsole
		/// Aggiunge i metodi definiti nella console, i quali 
		/// devono essere intercettati da tutti i plugIn
		/// La condizione è che il custom attribute del metodo
		/// sia impostato su All
		/// </summary>
		/// <param name="assemblyType"></param>
		/// <param name="assemblies"></param>
		//---------------------------------------------------------------------
		public void AddMethodsConsole(Type assemblyType, ArrayList assemblies)
		{
			//trovo tutti i metodi definiti nell'assembly
			MethodInfo[] methods		= assemblyType.GetMethods();
			MethodsExecuted methodsList	= new MethodsExecuted();
			for (int i = 0; i < methods.Length; i++)
			{
				
				//aggiungo il metodo solo se c'è il custom attribute
				Object[] attributeHandler = methods[i].GetCustomAttributes(typeof(AssemblyEvent),true);
				if (attributeHandler.Length > 0)
				{
					string definedIntoAssembly = ((AssemblyEvent)attributeHandler[0]).AssemblyName;
					if (String.Compare(definedIntoAssembly, AllPlugIns, true, CultureInfo.InvariantCulture) != 0)
					{
						methodsList.AssemblyName = assemblyType.FullName;
						methodsList.MethodAfterEvent.Add(methods[i]);
						methodsList.DefinedIntoAssembly.Add(((AssemblyEvent)attributeHandler[0]).AssemblyName);
						methodsList.AfterEvent.Add(((AssemblyEvent)attributeHandler[0]).EventName);
					}
					else
					{
						//attacca questo metodo a tutti i plugIn
						for (int j = 0; j < assemblies.Count; j++)
						{
							methodsList.AssemblyName = assemblyType.FullName;
							methodsList.MethodAfterEvent.Add(methods[i]);
							methodsList.DefinedIntoAssembly.Add(assemblies[j].GetType().FullName);
							methodsList.AfterEvent.Add(((AssemblyEvent)attributeHandler[0]).EventName);
							
						}
					}
					
				}			   
			}
			if (methodsList.Count() > 0) 
				this.methodsList.Add(methodsList);
		}

		#endregion

		#region BuildEvents - Date le stutture eventi e metodi, esegue il += tra evento e delegate

		/// <summary>
		/// BuildEvents
		/// Date le stutture eventi e metodi, esegue il += tra evento e delegate
		/// </summary>
		/// <param name="assemblies"></param>
		//---------------------------------------------------------------------
		public bool BuildEvents(ArrayList assemblies)
		{
			bool succesBuildEvents = true;
			for (int i = 0; i < assemblies.Count; i++)
			{
				EventDescriptorCollection events;
				Object plugInFired		= (Object) assemblies[i];
				Type plugInFiredType	= plugInFired.GetType();
				for (int j = 0; j < eventsList.Count; j++)
				{
					EventsList eventList = (EventsList)eventsList[j];
					//esistono eventi per questo PlugIn?
					if (String.Compare(eventList.AssemblyName, plugInFiredType.FullName, true, CultureInfo.InvariantCulture) == 0)
					{
						events = eventList.Events;
						//per ogni evento vado a vedere se esiste un metodo definito in qualche altro plugin che deve rispondergli
						for (int h = 0; h < events.Count; h++)
						{
							string eventName	= events[h].Name;
							Type eventType		= events[h].EventType;
							for (int k=0; k < methodsList.Count; k++)
							{
								MethodsExecuted methodsHandle = (MethodsExecuted)methodsList[k];
								for (int s = 0; s < methodsHandle.AfterEvent.Count; s++)
								{
									if (String.Compare(methodsHandle.DefinedIntoAssembly[s].ToString(), AllPlugIns, true, CultureInfo.InvariantCulture) == 0)
									{
										//questo metodo deve essere eseguito in risposta a un evento che può
										//essere definito in ogni plugIn
										if (String.Compare(methodsHandle.AfterEvent[s].ToString(), eventName, true, CultureInfo.InvariantCulture) == 0)
										{
											string errorMessage = String.Empty;
											//ho trovato il metodo
											MethodInfo methodHandle = (MethodInfo)methodsHandle.MethodAfterEvent[s];
											if (methodHandle != null)
											{
												try
												{
													Object objMethodHandle = FindAssembly(assemblies, methodHandle.DeclaringType.Name);
													//se + già bindato non faccio niente
													Delegate delegateToAdd = Delegate.CreateDelegate(eventType, objMethodHandle, methodHandle.Name,true);
													
													events[h].RemoveEventHandler(plugInFired,delegateToAdd);
													events[h].AddEventHandler(plugInFired, delegateToAdd);
													
													
												}
												//catch dei possibili errori
												catch(System.ArgumentException eArgumentException)
												{
													Debug.Fail(eArgumentException.Message);
													errorMessage = String.Format(EventBuilderStrings.JoinEventAndMethod, eventName, plugInFiredType.Name, methodHandle.Name, methodHandle.DeclaringType.Name);
													ExtendedInfo extendedInfo = new ExtendedInfo();
													extendedInfo.Add(EventBuilderStrings.Description, eArgumentException.Message);
													extendedInfo.Add(EventBuilderStrings.Event, eventName);
													extendedInfo.Add(EventBuilderStrings.DefinedIn, plugInFiredType.Name);
													extendedInfo.Add(EventBuilderStrings.JoinWithMethod, methodHandle.Name);
													extendedInfo.Add(EventBuilderStrings.DefinedIn, methodHandle.DeclaringType.Name);
													extendedInfo.Add(EventBuilderStrings.Library, "Microarea.Console.Core.EventBuilder");
													extendedInfo.Add(EventBuilderStrings.Function, "BuildEvents");
													extendedInfo.Add(EventBuilderStrings.Source, eArgumentException.Source);
													extendedInfo.Add(EventBuilderStrings.StackTrace, eArgumentException.StackTrace);
													if (eArgumentException.InnerException != null)
														extendedInfo.Add(EventBuilderStrings.InnerException, eArgumentException.InnerException.Message);
													Diagnostic.Set(DiagnosticType.Warning, errorMessage, extendedInfo);
													return false;
												}
												catch(System.InvalidOperationException eInvalidOperationException)
												{
													Debug.Fail(eInvalidOperationException.Message);
													errorMessage = String.Format(EventBuilderStrings.JoinEventAndMethod, eventName, plugInFiredType.Name, methodHandle.Name, methodHandle.DeclaringType.Name);
													ExtendedInfo extendedInfo = new ExtendedInfo();
													extendedInfo.Add(EventBuilderStrings.Description, eInvalidOperationException.Message);
													extendedInfo.Add(EventBuilderStrings.Event, eventName);
													extendedInfo.Add(EventBuilderStrings.DefinedIn, plugInFiredType.Name);
													extendedInfo.Add(EventBuilderStrings.JoinWithMethod, methodHandle.Name);
													extendedInfo.Add(EventBuilderStrings.DefinedIn, methodHandle.DeclaringType.Name);
													extendedInfo.Add(EventBuilderStrings.Library, "Microarea.Console.Core.EventBuilder");
													extendedInfo.Add(EventBuilderStrings.Function, "BuildEvents");
													extendedInfo.Add(EventBuilderStrings.Source, eInvalidOperationException.Source);
													extendedInfo.Add(EventBuilderStrings.StackTrace, eInvalidOperationException.StackTrace);
													if (eInvalidOperationException.InnerException != null)
														extendedInfo.Add(EventBuilderStrings.InnerException, eInvalidOperationException.InnerException.Message);
													Diagnostic.Set(DiagnosticType.Warning, errorMessage, extendedInfo);
													return false;
												}
												catch(System.MethodAccessException eMethodAccessException)
												{
													Debug.Fail(eMethodAccessException.Message);
													errorMessage = String.Format(EventBuilderStrings.JoinEventAndMethod, eventName, plugInFiredType.Name, methodHandle.Name, methodHandle.DeclaringType.Name);
													ExtendedInfo extendedInfo = new ExtendedInfo();
													extendedInfo.Add(EventBuilderStrings.Description, eMethodAccessException.Message);
													extendedInfo.Add(EventBuilderStrings.Event, eventName);
													extendedInfo.Add(EventBuilderStrings.DefinedIn, plugInFiredType.Name);
													extendedInfo.Add(EventBuilderStrings.JoinWithMethod, methodHandle.Name);
													extendedInfo.Add(EventBuilderStrings.DefinedIn, methodHandle.DeclaringType.Name);
													extendedInfo.Add(EventBuilderStrings.Library, "Microarea.Console.Core.EventBuilder");
													extendedInfo.Add(EventBuilderStrings.Function, "BuildEvents");
													extendedInfo.Add(EventBuilderStrings.Source, eMethodAccessException.Source);
													extendedInfo.Add(EventBuilderStrings.StackTrace, eMethodAccessException.StackTrace);
													if (eMethodAccessException.InnerException != null)
														extendedInfo.Add(EventBuilderStrings.InnerException, eMethodAccessException.InnerException.Message);
													Diagnostic.Set(DiagnosticType.Warning, errorMessage, extendedInfo);
													return false;
												}
											}
										}
									}
									else if (String.Compare(methodsHandle.DefinedIntoAssembly[s].ToString(), plugInFiredType.FullName, true, CultureInfo.InvariantCulture) == 0)
									{
										//esiste un metodo da eseguire in risposta a un evento definito in un assembly
										//è l'evento che mi interessa
										if (String.Compare(methodsHandle.AfterEvent[s].ToString(), eventName, true, CultureInfo.InvariantCulture) == 0)
										{
											string errorMessage = String.Empty;
											//ho trovato il metodo
											MethodInfo methodHandle = (MethodInfo)methodsHandle.MethodAfterEvent[s];
											if (methodHandle !=null)
											{
												try
												{
													Object objMethodHandle = FindAssembly(assemblies,methodHandle.DeclaringType.Name);
													//se + già bindato non faccio niente
													Delegate delegateToAdd = Delegate.CreateDelegate(eventType,objMethodHandle,methodHandle.Name,true);
													events[h].RemoveEventHandler(plugInFired,delegateToAdd);
													events[h].AddEventHandler(plugInFired, delegateToAdd);
													
													
													
												}
												//catch dei possibili errori
												catch(System.ArgumentException eArgumentException)
												{
													Debug.Fail(eArgumentException.Message);
													errorMessage = String.Format(EventBuilderStrings.JoinEventAndMethod, eventName, plugInFiredType.Name, methodHandle.Name, methodHandle.DeclaringType.Name);
													ExtendedInfo extendedInfo = new ExtendedInfo();
													extendedInfo.Add(EventBuilderStrings.Description, eArgumentException.Message);
													extendedInfo.Add(EventBuilderStrings.Event, eventName);
													extendedInfo.Add(EventBuilderStrings.DefinedIn, plugInFiredType.Name);
													extendedInfo.Add(EventBuilderStrings.JoinWithMethod, methodHandle.Name);
													extendedInfo.Add(EventBuilderStrings.DefinedIn, methodHandle.DeclaringType.Name);
													extendedInfo.Add(EventBuilderStrings.Library, "Microarea.Console.Core.EventBuilder");
													extendedInfo.Add(EventBuilderStrings.Function, "BuildEvents");
													extendedInfo.Add(EventBuilderStrings.Source, eArgumentException.Source);
													extendedInfo.Add(EventBuilderStrings.StackTrace, eArgumentException.StackTrace);
													if (eArgumentException.InnerException != null)
														extendedInfo.Add(EventBuilderStrings.InnerException, eArgumentException.InnerException.Message);
													Diagnostic.Set(DiagnosticType.Warning, errorMessage, extendedInfo);
													
													return false;
												}
												catch(System.InvalidOperationException eInvalidOperationException)
												{
													Debug.Fail(eInvalidOperationException.Message);
													errorMessage = String.Format(EventBuilderStrings.JoinEventAndMethod, eventName, plugInFiredType.Name, methodHandle.Name, methodHandle.DeclaringType.Name);
													ExtendedInfo extendedInfo = new ExtendedInfo();
													extendedInfo.Add(EventBuilderStrings.Description, eInvalidOperationException.Message);
													extendedInfo.Add(EventBuilderStrings.Event, eventName);
													extendedInfo.Add(EventBuilderStrings.DefinedIn, plugInFiredType.Name);
													extendedInfo.Add(EventBuilderStrings.JoinWithMethod, methodHandle.Name);
													extendedInfo.Add(EventBuilderStrings.DefinedIn, methodHandle.DeclaringType.Name);
													extendedInfo.Add(EventBuilderStrings.Library, "Microarea.Console.Core.EventBuilder");
													extendedInfo.Add(EventBuilderStrings.Function, "BuildEvents");
													extendedInfo.Add(EventBuilderStrings.Source, eInvalidOperationException.Source);
													extendedInfo.Add(EventBuilderStrings.StackTrace, eInvalidOperationException.StackTrace);
													if (eInvalidOperationException.InnerException != null)
														extendedInfo.Add(EventBuilderStrings.InnerException, eInvalidOperationException.InnerException.Message);
													Diagnostic.Set(DiagnosticType.Warning, errorMessage, extendedInfo);
													return false;
												}
												catch(System.MethodAccessException eMethodAccessException)
												{
													Debug.Fail(eMethodAccessException.Message);
													errorMessage = String.Format(EventBuilderStrings.JoinEventAndMethod, eventName, plugInFiredType.Name, methodHandle.Name, methodHandle.DeclaringType.Name);
													ExtendedInfo extendedInfo = new ExtendedInfo();
													extendedInfo.Add(EventBuilderStrings.Description, eMethodAccessException.Message);
													extendedInfo.Add(EventBuilderStrings.Event, eventName);
													extendedInfo.Add(EventBuilderStrings.DefinedIn, plugInFiredType.Name);
													extendedInfo.Add(EventBuilderStrings.JoinWithMethod, methodHandle.Name);
													extendedInfo.Add(EventBuilderStrings.DefinedIn, methodHandle.DeclaringType.Name);
													extendedInfo.Add(EventBuilderStrings.Library, "Microarea.Console.Core.EventBuilder");
													extendedInfo.Add(EventBuilderStrings.Function, "BuildEvents");
													extendedInfo.Add(EventBuilderStrings.Source, eMethodAccessException.Source);
													extendedInfo.Add(EventBuilderStrings.StackTrace, eMethodAccessException.StackTrace);
													if (eMethodAccessException.InnerException != null)
														extendedInfo.Add(EventBuilderStrings.InnerException, eMethodAccessException.InnerException.Message);
													Diagnostic.Set(DiagnosticType.Warning, errorMessage, extendedInfo);
													
													return false;
												}
											}
										}
									}
								}
							}	
						}
					}
				}
			}
			return succesBuildEvents;
		}

		#endregion
		
		#region FindMethodHandle - Cerca nella struttura MethodsExecuted se per l'evento specificato esiste un metodo da agganciare

		/// <summary>
		/// FindMethodHandle
		/// Cerca nella struttura MethodsExecuted se per l'evento specificato
		/// esiste un metodo da caricare
		/// </summary>
		/// <param name="eventName"></param>
		/// <param name="assemblyName"></param>
		/// <returns></returns>
		//---------------------------------------------------------------------
		private MethodInfo FindMethodHandle(string eventName, string assemblyName)
		{
			for (int i = 0; i < methodsList.Count; i++)
			{
				MethodsExecuted methodsHandle = (MethodsExecuted)methodsList[i];
				for (int j = 0; j < methodsHandle.AfterEvent.Count; j++)
				{				
					if (String.Compare(methodsHandle.DefinedIntoAssembly[j].ToString(), assemblyName, true, CultureInfo.InvariantCulture) == 0)
					{
						//esiste un metodo da eseguire in risposta a un evento definito in un assembly
						//è l'evento che mi interessa
						if (String.Compare(methodsHandle.AfterEvent[j].ToString(), eventName, true, CultureInfo.InvariantCulture) == 0)
						{
							//ho trovato il metodo
							return (MethodInfo) methodsHandle.MethodAfterEvent[j];
						}
					}
				}
			}
			return null;
		}

		#endregion
		
		#region FindAssembly - Dalla lista degli assembly caricati trova un determinato assembly

		/// <summary>
		/// FindAssembly
		/// Dalla lista degli assembly caricati,
		/// trova un determinato assembly
		/// attraverso il nome
		/// </summary>
		/// <param name="assemblies"></param>
		/// <param name="assemblyName"></param>
		/// <returns></returns>
		//---------------------------------------------------------------------
		private object FindAssembly(ArrayList assemblies, string assemblyName)
		{
			for (int i = 0; i < assemblies.Count; i++)
			{
				object	plugIn		= (object) assemblies[i];
				Type	plugInType	= plugIn.GetType();
				if (String.Compare(plugInType.Name, assemblyName, true, CultureInfo.InvariantCulture) == 0)
					return plugIn;
			}
			return null;
		}	

		#endregion
	}
	
	
}
