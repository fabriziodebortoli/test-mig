using System;
using System.Xml;
using Microarea.Library.XmlPersister;

namespace Microarea.Library.CommonDeploymentFunctions.States
{
	/// <summary>
	/// Un'istanza di questa classe rappresenta una fotografia istantanea
	/// dello stato della macchina a stati.
	/// Tutti i suoi membri sono public r/w, perché così facendo si possono
	/// serializzare in formato XML utilizzando XmlSerializer
	/// </summary>
	[Serializable]
	public class UpdateMachineBaseState : State
	{
		public MachineStates Status = MachineStates.Idle;	// stato della macchina
		public int FailedAttempts = 0;		// numero tentativi successivi andati a vuoto
		public string SessionGuid;			// guid da specificare nelle richieste di aggiornamento
		public string FtpUser;				// nome utente FTP da usare per il download
		public string FtpPassword;			// password utente FTP da usare per il download
		public string FtpUri;				// Uri cui accedere via FTP per il download
		public long FileSize;				// dimensione in bytes del file ZIP

		public DataSourceState DataSourceState = new DataSourceState();
	}

	//=========================================================================
	/// <summary>
	/// Specializzazione della macchina a stati per l'aggiornamento della DownloadImage
	/// da immagini di deployment organizzate per [product, release]
	/// </summary>
	[Serializable]
	public class UpdateMachineState : UpdateMachineBaseState
	{
		public string[] AvailableUpdates;	// lista degli aggiornamenti disponibili
		public string StorageName;			// nome della configurazione installata, come nota al server Microarea
		public string StorageRelease;		// release della configurazione installata (major.minor)
		public string RequiredRelease;		// release richiesta (major.minor)
		public string ServedReleaseExtended	= string.Empty;	// release fornita (major.minor.sp.date)
		public bool IgnoreSlaveCompatibilityBreaks = false;	// In presenza di verticali che perderebbero
															// compatibilità con la RequiredRelease, inibisce
															// di default l'aggiornamento
		public bool ProcessingFirstInstallation = false;	// Indica se (quando state non idle) si sta installando il prodotto
												// anziché aggiornarlo
	}

	//=========================================================================
	/// <summary>
	/// Anagrafica degli stati che la macchina a stati può assumere.
	/// </summary>
	public enum MachineStates
	{
		Idle,
		UpdatesExistenceConfirmed,
		WaitingForFtpCredentials,
		HavingFtpCredentials,
		Downloading,
		DownloadFailed,
		Downloaded,
		EndDownloadSignaled,
		InflatingUpdates,
		UpdatesInflated,
		Copying,
		Updating
	}

	public enum DownloaderType { Ftp, Bits }

}