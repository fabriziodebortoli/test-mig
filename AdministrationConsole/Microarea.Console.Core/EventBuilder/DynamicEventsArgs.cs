using System;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using Microarea.TaskBuilderNet.Core.DiagnosticManager;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.Console.Core.EventBuilder
{
	/// <summary>
	/// DynamicEventsArgs
	/// Usata dagli eventi dei plugIn : Trova gli args degli eventi tramite reflection
	/// </summary>
	//==========================================================================
	public class DynamicEventsArgs : System.EventArgs
	{
		#region Variabili private

		private Object				dataArgument			= null;
		private Diagnostic diagnostic						= new Diagnostic("Library.EventBuilder");
		
		#endregion

		#region Proprieta'

		/// <summary>
		/// Imposta / ritorna gli argomenti dell'evento
		/// </summary>
		//---------------------------------------------------------------------
		public object		DataArgument { get { return dataArgument;	} set { dataArgument = value; }	}
		public Diagnostic	Diagnostic	 { get { return diagnostic;		}}	
		#endregion

		#region Costruttore (vuoto)

		/// <summary>
		/// Costruttore vuoto
		/// </summary>
		//---------------------------------------------------------------------
		public DynamicEventsArgs()
		{
		}

		#endregion

		#region Costruttore con inizializzazione

		/// <summary>
		/// Inizializza il data argument dell'evento
		/// </summary>
		/// <param name="data"></param>
		//---------------------------------------------------------------------
		public DynamicEventsArgs(object data)
		{
			dataArgument = data;
		}

		#endregion

		#region Add 

		/// <summary>
		/// Aggiunge l'elemento alla classe
		/// elemento = una classe, un object,...
		/// </summary>
		/// <param name="data"></param>
		//---------------------------------------------------------------------
		public void Add(object data)
		{
			dataArgument = data;
		}

		#endregion

		#region Get
		
		/// <summary>
		/// Get
		/// dato il nome della propietà, lo cerca tramite reflection (ogni evento 
		/// di plugIn definisce la propria classe Args, a "priori" non so
		/// come è fatta
		/// </summary>
		/// <param name="propertyName"></param>
		/// <returns></returns>
		//---------------------------------------------------------------------
		public object Get(string propertyName)
		{
			string message = "";
			Type dataType = dataArgument.GetType();
			//se è una classe vado a cercare le properties
			if (dataType.IsClass || dataType.IsAnsiClass)
			{
				PropertyInfo property = dataType.GetProperty(propertyName);
				if (property.CanRead)
				{
					try
					{
						MethodInfo getProperty = property.GetGetMethod(false);
						if (getProperty != null)
						{
							Object objectClass     = Convert.ChangeType(dataArgument,getProperty.ReflectedType);
							Object objectsearched  = getProperty.Invoke(objectClass,BindingFlags.InvokeMethod,null,null,null);
							return objectsearched;
						}
					}
					catch(InvalidCastException eInvalidCastException)
					{

						message = String.Format(EventBuilderStrings.InvalidCastException, propertyName);
						ExtendedInfo extendedInfo = new ExtendedInfo();
						extendedInfo.Add(EventBuilderStrings.Description, eInvalidCastException.Message);
						extendedInfo.Add(EventBuilderStrings.Library, "Microarea.Console.Core.EventBuilder");
						extendedInfo.Add(EventBuilderStrings.Function, "Get");
						extendedInfo.Add(EventBuilderStrings.Source, eInvalidCastException.Source);
						extendedInfo.Add(EventBuilderStrings.StackTrace, eInvalidCastException.StackTrace);
						if (eInvalidCastException.InnerException != null)
							extendedInfo.Add(EventBuilderStrings.InnerException, eInvalidCastException.InnerException.Message);
						Diagnostic.Set(DiagnosticType.Warning, message, extendedInfo); 
						Debug.Fail(eInvalidCastException.Message);
					}
					catch(ArgumentException eArgumentException)
					{
						message = String.Format(EventBuilderStrings.ArgumentException, propertyName);
						ExtendedInfo extendedInfo = new ExtendedInfo();
						extendedInfo.Add(EventBuilderStrings.Description, eArgumentException.Message);
						extendedInfo.Add(EventBuilderStrings.Library, "Microarea.Console.Core.EventBuilder");
						extendedInfo.Add(EventBuilderStrings.Function, "Get");
						extendedInfo.Add(EventBuilderStrings.Source, eArgumentException.Source);
						extendedInfo.Add(EventBuilderStrings.StackTrace, eArgumentException.StackTrace);
						if (eArgumentException.InnerException != null)
							extendedInfo.Add(EventBuilderStrings.InnerException, eArgumentException.InnerException.Message);
						Diagnostic.Set(DiagnosticType.Warning, message, extendedInfo); 
						Debug.Fail(eArgumentException.Message);
					}
					catch(TargetException eTargetException)
					{
						message = String.Format(EventBuilderStrings.TargetException, "Get", propertyName);
						ExtendedInfo extendedInfo = new ExtendedInfo();
						extendedInfo.Add(EventBuilderStrings.Description, eTargetException.Message);
						extendedInfo.Add(EventBuilderStrings.Library, "Microarea.Console.Core.EventBuilder");
						extendedInfo.Add(EventBuilderStrings.Function, "Get");
						extendedInfo.Add(EventBuilderStrings.Source, eTargetException.Source);
						extendedInfo.Add(EventBuilderStrings.StackTrace, eTargetException.StackTrace);
						if (eTargetException.InnerException != null)
							extendedInfo.Add(EventBuilderStrings.InnerException, eTargetException.InnerException.Message);
						Diagnostic.Set(DiagnosticType.Warning, message, extendedInfo); 
						Debug.Fail(eTargetException.Message);
					}
					catch(TargetParameterCountException eTargetParameterCountException)
					{
						message = String.Format(EventBuilderStrings.TargetParameterCountException, "Get");
						ExtendedInfo extendedInfo = new ExtendedInfo();
						extendedInfo.Add(EventBuilderStrings.Description, eTargetParameterCountException.Message);
						extendedInfo.Add(EventBuilderStrings.Library, "Microarea.Console.Core.EventBuilder");
						extendedInfo.Add(EventBuilderStrings.Function, "Get");
						extendedInfo.Add(EventBuilderStrings.Source, eTargetParameterCountException.Source);
						extendedInfo.Add(EventBuilderStrings.StackTrace, eTargetParameterCountException.StackTrace);
						if (eTargetParameterCountException.InnerException != null)
							extendedInfo.Add(EventBuilderStrings.InnerException, eTargetParameterCountException.InnerException.Message);
						Diagnostic.Set(DiagnosticType.Warning, message, extendedInfo); 
						Debug.Fail(eTargetParameterCountException.Message);
					}
					catch(TargetInvocationException eTargetInvocationException)
					{
						message = String.Format(EventBuilderStrings.TargetInvocationException, "Get");	
						ExtendedInfo extendedInfo = new ExtendedInfo();
						extendedInfo.Add(EventBuilderStrings.Description, eTargetInvocationException.Message);
						extendedInfo.Add(EventBuilderStrings.Library, "Microarea.Console.Core.EventBuilder");
						extendedInfo.Add(EventBuilderStrings.Function, "Get");
						extendedInfo.Add(EventBuilderStrings.Source, eTargetInvocationException.Source);
						extendedInfo.Add(EventBuilderStrings.StackTrace, eTargetInvocationException.StackTrace);
						if (eTargetInvocationException.InnerException != null)
							extendedInfo.Add(EventBuilderStrings.InnerException, eTargetInvocationException.InnerException.Message);
						Diagnostic.Set(DiagnosticType.Warning, message, extendedInfo); 
						Debug.Fail(eTargetInvocationException.Message);
					}
					catch(MethodAccessException eMethodAccessException)
					{
						message = String.Format(EventBuilderStrings.MethodAccessException, "Get");	
						ExtendedInfo extendedInfo = new ExtendedInfo();
						extendedInfo.Add(EventBuilderStrings.Description, eMethodAccessException.Message);
						extendedInfo.Add(EventBuilderStrings.Library, "Microarea.Console.Core.EventBuilder");
						extendedInfo.Add(EventBuilderStrings.Function, "Get");
						extendedInfo.Add(EventBuilderStrings.Source, eMethodAccessException.Source);
						extendedInfo.Add(EventBuilderStrings.StackTrace, eMethodAccessException.StackTrace);
						if (eMethodAccessException.InnerException != null)
							extendedInfo.Add(EventBuilderStrings.InnerException, eMethodAccessException.InnerException.Message);
						Diagnostic.Set(DiagnosticType.Warning, message, extendedInfo); 
						Debug.Fail(eMethodAccessException.Message);
					}
				}
			}
			return null;
		}

		#endregion

		#region Is - ritorna true se il dato è del tipo specificato, false altrimenti

		/// <summary>
		/// Is
		/// ritorna true se il dato è del tipo specificato, false
		/// altrimenti
		/// </summary>
		/// <param name="nome"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		//---------------------------------------------------------------------
		public bool Is(string nome, string type)
		{
			bool result		= false;
			Type objectType = dataArgument.GetType();
			if (String.Compare(objectType.Name, type, true, CultureInfo.InvariantCulture) == 0)
				result = true;
			return result;
		}

		#endregion
	
	}
}
