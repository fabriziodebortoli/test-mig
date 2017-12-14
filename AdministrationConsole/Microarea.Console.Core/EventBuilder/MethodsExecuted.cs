using System.Collections;

namespace Microarea.Console.Core.EventBuilder
{
	/// <summary>
	/// MethodsExecuted
	/// Metodi da eseguire in risposta agli eventi comuni dei PlugIns
	/// Per essere caricati (by Reflection) devono essere taggati con
	/// il custom attribute
	/// </summary>
	//=========================================================================
	public class MethodsExecuted
	{
		#region Variaibli private

		private string		assemblyName		= string.Empty;
		private ArrayList	methodAfterEvent	= new ArrayList();
		private ArrayList	definedIntoAssembly = new ArrayList();
		private ArrayList	afterEvent			= new ArrayList();

		#endregion

		#region Proprieta'

		//Properties
		//---------------------------------------------------------------------
		//Nome dell'assembly in cui il metodo è definito
		public string	 AssemblyName		 { get { return assemblyName;		 } set { assemblyName		 = value; }}
		// Metodo/i da eseguire dopo l'evento specificato
		public ArrayList MethodAfterEvent	 { get { return methodAfterEvent;	 } set { methodAfterEvent	 = value; }}
		/// Se è ALL significa che il metodo può essere presente in più PlugIn,
		/// altrimenti specifica in quale plugIn il metodo è definito
		public ArrayList DefinedIntoAssembly { get { return definedIntoAssembly; } set { definedIntoAssembly = value; }}
		/// Specifica il nome dell'evento dopo il quale il metodo deve essere
		/// eseguito
		public ArrayList AfterEvent			 { get { return afterEvent;			 } set { afterEvent			 = value; }}

		#endregion

		#region Costruttore (vuoto)

		/// <summary>
		/// Costruttore vuoto
		/// </summary>
		//---------------------------------------------------------------------
		public MethodsExecuted() {}

		#endregion

		#region Count - Ritorna il numero di metodi da eseguire dopo l'evento

		/// <summary>
		/// Count
		/// Ritorna il numero di metodi da eseguire dopo l'evento
		/// </summary>
		/// <returns></returns>
		//---------------------------------------------------------------------
		public int Count()
		{
			return methodAfterEvent.Count;
		}

		#endregion

	}
	
}
