using System;

namespace Microarea.Console.Core.EventBuilder
{
	/// <summary>
	/// AssemblyEvent
	/// Attributo impostato su eventi e/o metodi di un plugIn
	/// Identifica l'evento dell'assembly a cui deve rispondere il metodo
	/// immediatamente successivo a questa dichiarazione
	//=========================================================================
	/// </summary>
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Event, AllowMultiple = false, Inherited = true)]
	public class AssemblyEvent: System.Attribute
	{
		#region Variabili private

		private string assemblyName;
		private string eventName;

		#endregion

		#region Proprietà

		/// <summary>
		/// Nome dell'assembly in cui è definito l'evento eventName a cui il metodo
		/// sotto la definizione di questo Attributo deve rispondere
		/// </summary>
		//---------------------------------------------------------------------
		public string AssemblyName { get {return assemblyName; }}

		/// <summary>
		/// Nome dell'evento definito in assemblyName a cui il metodo
		/// sotto la definizione di questo Attributo deve rispondere
		/// </summary>
		//---------------------------------------------------------------------
		public string EventName { get { return this.eventName; }}

		#endregion
		
		#region Costruttore

		/// <summary>
		/// Costruttore
		/// </summary>
		/// <param name="assemblyName"></param>
		/// <param name="eventName"></param>
		//---------------------------------------------------------------------
		public AssemblyEvent( string assemblyName, string eventName)
		{
			this.assemblyName	= assemblyName;
			this.eventName		= eventName;
		}

		#endregion

	}
		
}
