using System.ComponentModel;

namespace Microarea.Console.Core.EventBuilder
{
	
	/// <summary>
	/// EventsList
	/// Eventi letti by reflection dai plugIns, via via che vengono caricati
	/// Vengono letti solo gli eventi taggati con il customAttribute (ovvero quelli
	/// che devono essere intercettati anche dagli altri plugIns e console) e
	/// presenti nella classe principale del plugIns (es SysAdmin piuttosto che
	/// SysAdmin.User). Scelta architetturale, può essere modificata
	/// </summary>
	//=========================================================================
	public class EventsList
	{
		#region Variabili private

		private string						assemblyName = string.Empty;
		private EventDescriptorCollection	events		 = null;

		#endregion

		#region Proprieta'

		//Properties
		//---------------------------------------------------------------------
		public string				     AssemblyName	{ get { return assemblyName; } set { assemblyName = value;}}
		public EventDescriptorCollection Events			{ get { return events;		 } set { events		  = value;}}

		#endregion

		#region Costruttore (vuoto)

		/// <summary>
		/// Costruttore (vuoto)
		/// </summary>
		//---------------------------------------------------------------------
		public EventsList() {}

		#endregion
		
		#region Costruttore (con parametri)

		/// <summary>
		/// Costruttore con parametri
		/// </summary>
		/// <param name="assemblyName"></param>
		/// <param name="eventsList"></param>
		//---------------------------------------------------------------------
		public EventsList(string assemblyName, EventDescriptorCollection eventsList) 
		{
			this.assemblyName = assemblyName;
			this.events		  = eventsList;
		}

		#endregion
	}

}
