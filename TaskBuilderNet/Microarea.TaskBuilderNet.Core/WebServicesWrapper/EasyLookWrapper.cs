using System;

namespace Microarea.TaskBuilderNet.Core.WebServicesWrapper
{
	public class EasyLook
	{
		private easylook.EasyLookService easyLook = new easylook.EasyLookService();

		/// <summary>
		/// Costruttore
		/// </summary>
		/// <param name="lockManagerUrl">Indirizzo url di easylook</param>
		//---------------------------------------------------------------------------
		public EasyLook(string easyLookUrl, int timeout)
		{
			easyLook.Url = easyLookUrl;
			easyLook.Timeout = timeout;
		}

		/// <summary>
		/// Ritorna il report selezionato sotto forma di array di stringhe
		/// </summary>
		/// <param name="authenticationToken">token di autenticazion</param>
		/// <param name="parameters">Parametri in formato xml, inclusi namespace 
		/// del documento e parametri di estrazione</param>
		/// <param name="applicationDate">Data di applicazione</param>
		/// <param name="impersonatedUser">Utente per cui estrarre il report</param>
		/// <param name="useApproximation">Specifica se utilizzare l'approssimazione</param>
		/// <returns></returns>
		//---------------------------------------------------------------------------
		public string[] XmlExecuteReport(string authenticationToken, string parameters, DateTime applicationDate, string impersonatedUser, bool useApproximation)
		{
			return easyLook.XmlExecuteReport(authenticationToken, parameters, applicationDate, impersonatedUser, useApproximation);
		}

		/// <summary>
		/// Ritorna la stringa dei parametri del report selezionato
		/// </summary>
		/// <param name="authenticationToken">token di autenticazion</param>
		/// <param name="parameters">Parametri in formato xml, inclusi namespace 
		/// del documento e parametri di estrazione</param>
		/// <param name="applicationDate">Data di applicazione</param>
		/// <param name="impersonatedUser">Utente per cui estrarre il report</param>
		/// <param name="useApproximation">Specifica se utilizzare l'approssimazione</param>
		/// <returns>Xml contenente la stringa di formattazione dei parametri</returns>
		//---------------------------------------------------------------------------
		public string XmlGetParameters(string authenticationToken, string parameters, DateTime applicationDate, string impersonatedUser, bool useApproximation)
		{
			return easyLook.XmlGetParameters(authenticationToken, parameters, applicationDate, impersonatedUser, useApproximation);
		}
	}
}
