using System;
using System.Diagnostics;

using Microarea.Library.CommonDeploymentFunctions;
using Microarea.Library.CommonDeploymentFunctions.States;
using ImageVersion = Microarea.Library.CommonDeploymentFunctions.Version;

namespace Microarea.Library.ImagesManagement
{
	/// <summary>
	/// Classe che implementa le logiche di comportamento basate sul policies di versioni
	/// </summary>
	public class Policy
	{
		/// <summary>
		/// reperisce una versione tra n disponibili compatibilmente con la policy specificata
		/// </summary>
		/// <param name="policy">policy da applicare alla scelta</param>
		/// <param name="availableReleases">opzioni disponibili</param>
		/// <param name="currentReleaseString">release corrente</param>
		/// <returns>stringa con release scelta, null se la policy non ne permette alcuna</returns>
		//---------------------------------------------------------------------
		public static string GetReleaseByPolicy(EnumPolicyType policy, string[] availableReleases, string currentReleaseString)
		{
			if (availableReleases == null || availableReleases.Length == 0)
				return null;

			// l'elenco dovrebbe essere già stato ordinato dal back-end, ma per sicurezza
			// occorre ordinarlo anche lato client, casomai cambi implementazione
			// sul back-end)
			ImageVersion[] sortedUpdates = ImageVersion.GetSortedReleases(availableReleases);
			ImageVersion currentRelease = new ImageVersion(currentReleaseString);
			
			switch (policy)
			{
				case EnumPolicyType.ServicePack :
					// sceglie l'immagine omonima, se presente o quella immediatamente superiore
					ImageVersion releaseSP;
					for (int i = availableReleases.Length -  1; i >= 0; i--)
					{
						releaseSP = sortedUpdates[i];
						if (releaseSP.Major == currentRelease.Major && releaseSP.Minor == currentRelease.Minor)
							return releaseSP.ToString();
					}
					break;
				case EnumPolicyType.Minor :
					// sceglie l'immagine con stessa Major e la più alta Minor
					ImageVersion aRelease;
					for (int i = availableReleases.Length -  1; i >= 0; i--)
					{
						aRelease = sortedUpdates[i];
						if (aRelease.Major == currentRelease.Major)
							return aRelease.ToString();
					}
					break;
				case EnumPolicyType.Major :
					// sceglie la release con Major.Minor più alta
					return sortedUpdates[sortedUpdates.Length - 1].ToString();
			}
			return null;
		}

		//---------------------------------------------------------------------
		public static bool CanUpdate(string sourceReleaseAsString, string destinationReleaseAsString, EnumPolicyType policy)
		{
			if (sourceReleaseAsString == null || sourceReleaseAsString.Length == 0)
				return false;	// non dovrebbe capitare mai
			if (destinationReleaseAsString == null || destinationReleaseAsString.Length == 0)
				return true;

			ImageVersion sourceRelease		= new ImageVersion(sourceReleaseAsString);
			ImageVersion destinationRelease	= new ImageVersion(destinationReleaseAsString);
			
			switch (policy)
			{
				case EnumPolicyType.ServicePack :	// concessi solo aggiornamenti SP, no Minor, no Major
					if (sourceRelease.Major > destinationRelease.Major)
						return false;
					if (sourceRelease.Major == destinationRelease.Major &&
						sourceRelease.Minor > destinationRelease.Minor)
						return false;
					return true;

				case EnumPolicyType.Minor :			// concessi solo aggiornamenti SP e Minor, no Major
					if (sourceRelease.Major > destinationRelease.Major)
						return false;
					return true;

				case EnumPolicyType.Major :			// qualunque aggiornamento è accettabile
					return true;

				default :
					Debug.Fail("case non gestito in costrutto switch");
					return true; // solo per potere compilare
			}
		}
	}
}
