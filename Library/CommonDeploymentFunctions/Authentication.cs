using System;
using System.Diagnostics;
using System.Management;	// wrap su WMI, mi serve per reperire MAC Address
using System.Xml;
//
using Microarea.Library.Hardware;
using Microarea.Library.Licence;
using Microarea.Library.CommonDeploymentFunctions.States;
using Microarea.Library.CommonDeploymentFunctions.Strings;
using Consts = Microarea.Library.CommonDeploymentFunctions.Strings.Consts;	// TEMP

namespace Microarea.Library.CommonDeploymentFunctions
{
	/// <summary>
	/// Classe che incapsula la logica di autenticazione; è usata sia lato server che client.
	/// </summary>
	public class Authentication
	{
		/// <summary>
		/// Ricava a run-time l'indirizzo MAC della prima scheda di rete trovata
		/// installata sulla macchina corrente.
		/// </summary>
		/// <returns>Mac Address</returns>
		//---------------------------------------------------------------------
		private static string GetMacAddress()
		{
			string mac = string.Empty;
			try
			{
				return LocalMachine.GetMacAddress();
			}
			catch (Exception ex)
			{
				Debug.Fail(ex.Message);
				return "Unknown"; // LOCALIZE ?
			}
		}

		/// <summary>
		/// Dal DOM di security inviato dal client legge l'indirizzo MAC del client
		/// </summary>
		/// <param name="securityDoc">DOM di security</param>
		/// <returns>MAC Address, stringa vuota in caso di errore</returns>
		//---------------------------------------------------------------------
		public static string GetMacAddress(XmlDocument securityDoc)
		{
			return GetUniqueTagAttributeValue(securityDoc, Consts.TagMacAddress, Consts.AttributeValue);
		}

		/// <summary>
		/// Costruisce un DOM cui appende informazioni aggiuntive atte ad individuare
		/// univocamente l'utente in base alla sua dotazione hardware
		/// </summary>
		/// <remarks>
		/// Il metodo è nella dll comune e non nella classe specializzata del client
		/// per minimizzare i problemi di disallineamento tra dll
		/// </remarks>
		/// <returns>DOM di security</returns>
		//---------------------------------------------------------------------
		public static XmlDocument CollectClientSecurity()
		{
			XmlDocument clientSecurity = new XmlDocument();
			XmlElement root = clientSecurity.CreateElement(Consts.TagSecurityInfo);
			clientSecurity.AppendChild(root);

			// aggiungo MAC address
			string mac = GetMacAddress();
			XmlElement macEl = clientSecurity.CreateElement(Consts.TagMacAddress);
			macEl.SetAttribute(Consts.AttributeValue, mac);
			root.AppendChild(macEl);

			// NOTE - aggiungere qui ogni altra voce desiderata

			return clientSecurity;
		}

		//---------------------------------------------------------------------
		private static string GetUniqueTagAttributeValue
			(
				XmlDocument aDom,
				string tagName,
				string attributeName
			)
		{
			try
			{
				XmlElement userEl = aDom.GetElementsByTagName(tagName)[0] as XmlElement;
				return userEl.GetAttribute(attributeName);
			}
			catch (Exception ex)
			{
				Debug.Fail(ex.Message);
				Debug.WriteLine(ex.Message);
				// NOTE - potrei accettare che il client abbia una dll vecchia che non aggiunge alcune info
				return string.Empty;
			}
		}

		//---------------------------------------------------------------------
	}
}