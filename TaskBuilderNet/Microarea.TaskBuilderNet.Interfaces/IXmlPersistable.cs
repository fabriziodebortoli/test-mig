
namespace Microarea.TaskBuilderNet.Interfaces
{
	/// <summary>
	/// L'interfaccia IXmlPersistable impone alle classi che da essa derivano
	/// di implementare dei metodi per serializzare il proprio stato in XML.
	/// Consiglio di realizzare ciò incapsulando tutti i data member che
	/// rappresentano lo stato in una classe derivata da State.
	/// </summary>
	public interface IXmlPersistable
	{
		/// <summary>
		/// Path completo del file XML contenente lo stato serializzato
		/// </summary>
		string StatePath
		{
			get;
			set;
		}

		//---------------------------------------------------------------------

		/// <summary>
		/// Carica lo stato dell'oggetto da file XML.
		/// Il path utilizzato è quello indicato nella proprietà StatePath
		/// </summary>
		/// <returns>un booleano che indica il successo dell'operazione</returns>
		bool LoadXmlState();

		//---------------------------------------------------------------------

		/// <summary>
		/// Salva lo stato dell'oggetto su file XML.
		/// Il path utilizzato è quello indicato nella proprietà StatePath
		/// </summary>
		/// <returns>un booleano che indica il successo dell'operazione</returns>
		bool SaveXmlState();

		//---------------------------------------------------------------------

	}
}
