using System;
//
using Microarea.Library.XmlPersister;

namespace Microarea.Library.CommonDeploymentFunctions.States
{
	/// <summary>
	/// Un'istanza di questa classe rappresenta una fotografia
	/// dello stato dello scheduler.
	/// Tutti i suoi membri sono public r/w perché così facendo si possono
	/// serializzare in formato XML utilizzando XmlSerializer.
	/// </summary>
	[Serializable]
	public class InstallationSettings : State
	{
		// Aggiornamenti Running Image
		public bool				UpgradeEnabled;
		public string			Email			= string.Empty;
		public EnumPolicyType	RunningPolicy	= EnumPolicyType.ServicePack;

		//---------------------------------------------------------------------
		public static InstallationSettings GetFromXml(string installationSettingsFile)
		{
			return InstallationSettings.GetFromXml(installationSettingsFile, typeof(InstallationSettings)) as InstallationSettings;
		}
	}
}
