using System;
using System.Globalization;
//
using Microarea.Library.XmlPersister;

namespace Microarea.Library.CommonDeploymentFunctions.States
{
	/// <summary>
	/// Tutti i suoi membri sono public r/w, perché così facendo si possono
	/// serializzare in formato XML utilizzando XmlSerializer
	/// </summary>
	[Serializable]
	public class DataSourceState : State
	{
		public const bool		DefaultPatchApplication = false;
		public DataSourceType	DataSource = DataSourceType.CompactDisc;
		public bool				UsePatches = DefaultPatchApplication;
		public char				CdUnit = 'D';	// unità CD per aggiornamento da CD
		public string			NetworkPath = string.Empty;

		// indirizzo pubblico del WS (solo nome macchina, senza presisso http né path locale)
		public string			WebServiceMachineAddress = "";

		//---------------------------------------------------------------------
		public static string GetWebServiceUrl(string machineAddress)
		{
			return string.Format(CultureInfo.InvariantCulture, "http://{0}/WebDeployer/WebDeployer.asmx", machineAddress);
		}
	}

	//=========================================================================
	public enum DataSourceType { WebService, CompactDisc, Network }
}