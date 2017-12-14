using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Resources;
using System.Text;
using System.Xml;

using Microarea.Library.CommonDeploymentFunctions.States;
using Microarea.Library.CommonDeploymentFunctions.Strings;
using Microarea.TaskBuilderNet.Core.NameSolver;

namespace Microarea.Library.CommonDeploymentFunctions
{
	//=========================================================================
	public sealed class Events
	{
		private static ResourceManager resources = new ResourceManager(typeof(Events));

		// NOTE - le quattro stringhe sottostanti devono essere definite per prime!
		public static string DownloadImageName					{  get { return resources.GetString("DownloadImageName"); } }
		public static string RunningImageName					{  get { return resources.GetString("RunningImageName"); } }
//		public static string LocalizableBackupDownloadImageName	{  get { return string.Format(CultureInfo.CurrentCulture, resources.GetString("BackupImageName"), DownloadImageName); } }
		public static string LocalizableBackupRunningImageName	{  get { return string.Format(CultureInfo.CurrentCulture, resources.GetString("BackupImageName"), RunningImageName); } }
		public static string MicroareaServicesName				{  get { return resources.GetString("MicroareaServicesName"); } }


		// Messaggi di step UpdateDownload
		public const	string StepMessage			= "LocalizableStepMessage";
		public static	string LocalizableStepMessage			{  get { return resources.GetString("StepMessage"); } }

		public const	string UpdatesInflated		= "LocalizableUpdatesInflated";
		public static	string LocalizableUpdatesInflated		{  get { return resources.GetString("UpdatesInflated"); } }
		
		public const	string HavingFtpCredentials	= "LocalizableHavingFtpCredentials";
		public static	string LocalizableHavingFtpCredentials	{  get { return resources.GetString("HavingFtpCredentials"); } }
		
		public const	string Downloading			= "LocalizableDownloading";
		public static	string LocalizableDownloading			{  get { return resources.GetString("Downloading"); } }
		
		public const	string EndDownloadSignaled	= "LocalizableEndDownloadSignaled";
		public static	string LocalizableEndDownloadSignaled	{  get { return resources.GetString("EndDownloadSignaled"); } }
		
		public const	string InflatingUpdates		= "LocalizableInflatingUpdates";
		public static	string LocalizableInflatingUpdates		{  get { return resources.GetString("InflatingUpdates"); } }
		
		public const	string Updating				= "LocalizableUpdating";
		public static	string LocalizableUpdating				{  get { return resources.GetString("Updating"); } }
		
		public const	string Copying				= "LocalizableCopying";
		public static	string LocalizableCopying				{  get { return string.Format(CultureInfo.CurrentCulture, resources.GetString("Copying"), DownloadImageName); } }
		
		public const	string WaitingForFtpCredentials		= "LocalizableWaitingForFtpCredentials";
		public static	string LocalizableWaitingForFtpCredentials		{  get { return resources.GetString("WaitingForFtpCredentials"); } }

		public const	string UpdatesAreAvailable		= "LocalizableUpdatesAreAvailable";
		public static	string LocalizableUpdatesAreAvailable		{  get { return resources.GetString("UpdatesAreAvailable"); } }

		public const	string Idle		= "LocalizableIdle";
		public static	string LocalizableIdle		{  get { return string.Empty;/*resources.GetString("Idle");*/ } }	// TODOFEDE - mettere un msg opportuno nelle risorse
		// fine eventi di step UpdateDownload

		// Messaggi di step di DeleteInstallation
		internal static string GetDeleteInstallationPhaseString(DeleteInstallationPhases phase)
		{
			return "Localizable" + phase.ToString();
		}
		public static	string LocalizableRemoveUpdaters				{  get { return resources.GetString("RemoveUpdaters"); } }
		public static	string LocalizableUnloadWebApps					{  get { return resources.GetString("UnloadWebApps"); } }
		public static	string LocalizableRemoveInstallationShares		{  get { return resources.GetString("RemoveInstallationShares"); } }
		public static	string LocalizableRemoveWebShares				{  get { return resources.GetString("RemoveWebShares"); } }
		public static	string LocalizableRemoveInstallationDirectory	{  get { return resources.GetString("RemoveInstallationDirectory"); } }
		public static	string LocalizableRemovingLocalClientDirectory	{  get { return resources.GetString("RemovingLocalClientDirectory"); } }
		// fine eventi di step DeleteInstallation

		// Messaggi di step di RemoveAddOnProduct
		internal static string GetRemoveAddOnPhaseString(RemoveAddOnPhases phase)
		{
			return "Localizable" + phase.ToString();
		}
		// note: others are same as for UpdateRunning
		public static	string LocalizableRemoveUpdater			{  get { return resources.GetString("RemoveUpdater"); } }
		public static	string LocalizableRemoveConfigFiles		{  get { return resources.GetString("RemoveConfigFiles"); } }
		public static	string LocalizableRemoveClientCaches 	{  get { return resources.GetString("RemoveClientCaches"); } }
		// fine eventi di step RemoveAddOnProduct

		// Messaggi di errore/warning legati a CreateNewInstallation
		public const	string PathError					= "LocalizablePathError";
		public static	string LocalizablePathError						{  get { return resources.GetString("PathError"); } }
		public const	string InvalidInstallationName		= "LocalizableInvalidInstallationName";
		public static	string LocalizableInvalidInstallationName		{  get { return resources.GetString("InvalidInstallationName"); } }
		public const	string StandardExisting				= "LocalizableStandardExisting";
		public static	string LocalizableStandardExisting				{  get { return resources.GetString("StandardExisting"); } }
		public const	string CreatingInstallationDirError	= "LocalizableCreatingInstallationDirError";
		public static	string LocalizableCreatingInstallationDirError	{  get { return resources.GetString("CreatingInstallationDirError"); } }
		public const	string CreatingDirError				= "LocalizableCreatingDirError";
		public static	string LocalizableCreatingDirError				{  get { return resources.GetString("CreatingDirError"); } }
		public const	string SettingAclError				= "LocalizableSettingAclError";
		public static	string LocalizableSettingAclError				{  get { return resources.GetString("SettingAclError"); } }
		public const	string UnableToCreateWebFolder		= "LocalizableUnableToCreateWebFolder";
		public static	string LocalizableUnableToCreateWebFolder		{  get { return resources.GetString("UnableToCreateWebFolder"); } }
		public const	string UnableToAddWebMappings		= "LocalizableUnableToAddWebMappings";
		public static	string LocalizableUnableToAddWebMappings		{  get { return resources.GetString("UnableToAddWebMappings"); } }
		public const	string UnableToRemoveWebMappings	= "LocalizableUnableToRemoveWebMappings";
		public static	string LocalizableUnableToRemoveWebMappings		{ get { return resources.GetString("UnableToRemoveWebMappings"); } }
		public const	string UnableToCreateShare			= "LocalizableUnableToCreateShare";
		public static	string LocalizableUnableToCreateShare			{  get { return resources.GetString("UnableToCreateShare"); } }
		
		public const	string UnableToCheckWWW1			= "LocalizableUnableToCheckWWW1";
		public static	string LocalizableUnableToCheckWWW1			{  get { return resources.GetString("UnableToCheckWWW1"); } }
		public const	string UnableToCheckWWW2			= "LocalizableUnableToCheckWWW2";
		public static	string LocalizableUnableToCheckWWW2			{  get { return resources.GetString("UnableToCheckWWW2"); } }
		public const	string UnableToCheckWWW3			= "LocalizableUnableToCheckWWW3";
		public static	string LocalizableUnableToCheckWWW3			{  get { return resources.GetString("UnableToCheckWWW3"); } }
		public const	string WWWServiceNotInstalled		= "LocalizableWWWServiceNotInstalled";
		public static	string LocalizableWWWServiceNotInstalled	{  get { return resources.GetString("WWWServiceNotInstalled"); } }
		public const	string UnableToStartWWW1			= "LocalizableUnableToStartWWW1";
		public static	string LocalizableUnableToStartWWW1			{  get { return resources.GetString("UnableToStartWWW1"); } }
		public const	string UnableToStartWWW2			= "LocalizableUnableToStartWWW2";
		public static	string LocalizableUnableToStartWWW2			{  get { return resources.GetString("UnableToStartWWW2"); } }
		public const	string UnableToStartWWW3			= "LocalizableUnableToStartWWW3";
		public static	string LocalizableUnableToStartWWW3			{  get { return resources.GetString("UnableToStartWWW3"); } }
		public const	string NonSupportedFileSystem1		= "LocalizableNonSupportedFileSystem1";
		public static	string LocalizableNonSupportedFileSystem1	{  get { return resources.GetString("NonSupportedFileSystem1"); } }
		public const	string NonSupportedFileSystem2		= "LocalizableNonSupportedFileSystem2";
		public static	string LocalizableNonSupportedFileSystem2	{  get { return resources.GetString("NonSupportedFileSystem2"); } }
		public const	string UnableToCheckFileSystem		= "LocalizableUnableToCheckFileSystem";
		public static	string LocalizableUnableToCheckFileSystem	{  get { return resources.GetString("UnableToCheckFileSystem"); } }
		public const	string UnableToRetrieveDrive		= "LocalizableUnableToRetrieveDrive";
		public static	string LocalizableUnableToRetrieveDrive	{  get { return resources.GetString("UnableToRetrieveDrive"); } }
		public const	string UnableToCheckAspNetAccount	= "LocalizableUnableToCheckAspNetAccount";
		public static	string LocalizableUnableToCheckAspNetAccount	{  get { return resources.GetString("UnableToCheckAspNetAccount"); } }
		public const	string UnableToUnlockAspNetAccount	= "LocalizableUnableToUnlockAspNetAccount";
		public static	string LocalizableUnableToUnlockAspNetAccount	{  get { return resources.GetString("UnableToUnlockAspNetAccount"); } }
		public const	string NoDefaultWebSiteFound	= "LocalizableNoDefaultWebSiteFound";
		public static	string LocalizableNoDefaultWebSiteFound	{  get { return resources.GetString("NoDefaultWebSiteFound"); } }
		public const	string UnableToCheckDWSErrorStatus	= "LocalizableUnableToCheckDWSErrorStatus";
		public static	string LocalizableUnableToCheckDWSErrorStatus	{  get { return resources.GetString("UnableToCheckDWSErrorStatus"); } }
		public const	string DefaultWebSiteErrorStatus	= "LocalizableDefaultWebSiteErrorStatus";
		public static	string LocalizableDefaultWebSiteErrorStatus	{  get { return resources.GetString("DefaultWebSiteErrorStatus"); } }
		public const	string FailedToStartDefaultWebSite	= "LocalizableFailedToStartDefaultWebSite";
		public static	string LocalizableFailedToStartDefaultWebSite	{  get { return resources.GetString("FailedToStartDefaultWebSite"); } }
		public const	string CannotRetrieveDWSInfo	= "LocalizableCannotRetrieveDWSInfo";
		public static	string LocalizableCannotRetrieveDWSInfo	{  get { return resources.GetString("CannotRetrieveDWSInfo"); } }
		public const	string CannotRetrieveUserInfoFile	= "LocalizableCannotRetrieveUserInfoFile";
		public static	string LocalizableCannotRetrieveUserInfoFile	{  get { return resources.GetString("CannotRetrieveUserInfoFile"); } }
		public const	string CannotReadUserInfoFile	= "LocalizableCannotReadUserInfoFile";
		public static	string LocalizableCannotReadUserInfoFile	{  get { return resources.GetString("CannotReadUserInfoFile"); } }
		public const	string CannotReadUserInfoCountry	= "LocalizableCannotReadUserInfoCountry";
		public static	string LocalizableCannotReadUserInfoCountry	{  get { return resources.GetString("CannotReadUserInfoCountry"); } }

		// fine messaggi di errore/warning legati a CreateNewInstallation

		// Messaggi di eventi legati a files:
		public static string MsgBackingUpFile		{  get { return resources.GetString("MsgBackingUpFile"); } }
		public static string MsgDeletingFile		{  get { return resources.GetString("MsgDeletingFile"); } }
		public static string MsgMovingFile			{  get { return resources.GetString("MsgMovingFile"); } }
		public static string MsgCopyingFile			{  get { return resources.GetString("MsgCopyingFile"); } }
		public static string MsgMovingDirectory		{  get { return resources.GetString("MsgMovingDirectory"); } }
		public static string MsgCopyingModuleFiles	{  get { return resources.GetString("MsgCopyingModuleFiles"); } }
		public static string MsgPatchingUnit		{  get { return resources.GetString("MsgPatchingUnit"); } }
		public static string MsgExtractingFile		{  get { return resources.GetString("MsgExtractingFile"); } }
		
		public const	string MsgDownloadPercent = "LocalizableMsgDownloadPercent";
		public static	string LocalizableMsgDownloadPercent	{  get { return resources.GetString("MsgDownloadPercent"); } }

		// Messaggi di step UpdateRunning
		public const	string CheckingInterProductCompatibility = "LocalizableCheckingInterProductCompatibility";
		public static	string LocalizableCheckingInterProductCompatibility	{  get { return resources.GetString("CheckingInterProductCompatibility"); } }

		public const	string LockingDownload = "LocalizableLockingDownload";
		public static	string LocalizableLockingDownload	{  get { return string.Format(CultureInfo.CurrentCulture, resources.GetString("LockingImage"), DownloadImageName); } }

		public const	string UnlockingDownload = "LocalizableUnlockingDownload";
		public static	string LocalizableUnlockingDownload	{  get { return string.Format(CultureInfo.CurrentCulture, resources.GetString("UnlockingImage"), DownloadImageName); } }
		
		public const	string SavingUpdatedState = "LocalizableSavingUpdatedState";
		public static	string LocalizableSavingUpdatedState{  get { return resources.GetString("SavingUpdatedState"); } }
		
		public const	string LockingRunning = "LocalizableLockingRunning";
		public static	string LocalizableLockingRunning	{  get { return string.Format(CultureInfo.CurrentCulture, resources.GetString("LockingImage"), RunningImageName); } }
		
		public const	string RollingbackRunning = "LocalizableRollingbackRunning";
		public static	string LocalizableRollingbackRunning{  get { return string.Format(CultureInfo.CurrentCulture, resources.GetString("RollingbackImage"), RunningImageName); } }
		
		public const	string UnlockingRunning = "LocalizableUnlockingRunning";
		public static	string LocalizableUnlockingRunning	{  get { return string.Format(CultureInfo.CurrentCulture, resources.GetString("UnlockingImage"), RunningImageName); } }

		public const	string SynchronizingContent = "LocalizableSynchronizingContent";
		public static	string LocalizableSynchronizingContent	{  get { return string.Format(CultureInfo.CurrentCulture, resources.GetString("CopyingImageContent"), DownloadImageName, RunningImageName); } }
		
		public const	string ApplyingPatches = "LocalizableApplyingPatches";
		public static	string LocalizableApplyingPatches		{  get { return resources.GetString("ApplyingPatches"); } }
		
		public const	string SettingServices = "LocalizableSettingServices";
		public static	string LocalizableSettingServices		{  get { return resources.GetString("SettingServices"); } }
		
		public const	string CheckingRequirementsChanges = "LocalizableCheckingRequirementsChanges";
		public static	string LocalizableCheckingRequirementsChanges	{  get { return resources.GetString("CheckingRequirementsChanges"); } }
		
		public const	string CheckingDatabaseChanges = "LocalizableCheckingDatabaseChanges";
		public static	string LocalizableCheckingDatabaseChanges	{  get { return resources.GetString("CheckingDatabaseChanges"); } }
		
		public const	string RemovingUnnecessaryFiles = "LocalizableRemovingUnnecessaryFiles";
		public static	string LocalizableRemovingUnnecessaryFiles	{  get { return resources.GetString("RemovingUnnecessaryFiles"); } }
		
		public const	string RemovingUnusedModules = "LocalizableRemovingUnusedModules";
		public static	string LocalizableRemovingUnusedModules	{  get { return resources.GetString("RemovingUnusedModules"); } }
		
		public const	string UpdateMicroareaClient = "LocalizableUpdateMicroareaClient";
		public static	string LocalizableUpdateMicroareaClient	{  get { return resources.GetString("UpdateMicroareaClient"); } }
		
		public const	string UpdateMicroareaClientFailed = "LocalizableUpdateMicroareaClientFailed";
		public static	string LocalizableUpdateMicroareaClientFailed	{  get { return resources.GetString("UpdateMicroareaClientFailed"); } }
		// fine messaggi di step UpdateRunning

		public static string LocalizableFileAlreadyExists			{  get { return resources.GetString("FileAlreadyExists"); } }
		public const string FtpDownloadFailed = "LocalizableFtpDownloadFailed";
		public static string LocalizableFtpDownloadFailed			{  get { return resources.GetString("FtpDownloadFailed"); } }
		public const string FtpDownloadSucceeded = "LocalizableFtpDownloadSucceeded";
		public static string LocalizableFtpDownloadSucceeded		{  get { return resources.GetString("FtpDownloadSucceeded"); } }
		public const string BackToIdleDueToError = "LocalizableBackToIdleDueToError";
		public static string LocalizableBackToIdleDueToError		{  get { return resources.GetString("BackToIdleDueToError"); } }
		public const string BackToIdleAfterAttempts = "LocalizableBackToIdleAfterAttempts";
		public static string LocalizableBackToIdleAfterAttempts		{  get { return resources.GetString("BackToIdleAfterAttempts"); } }
		public const string TickEnded = "LocalizableTickEnded";
		public static string LocalizableTickEnded					{  get { return resources.GetString("TickEnded"); } }
		public const string UpdateDownloadStarted = "LocalizableUpdateDownloadStarted";
		public static string LocalizableUpdateDownloadStarted		{  get { return string.Format(CultureInfo.CurrentCulture, resources.GetString("UpdateProcessStarted"), DownloadImageName); } }
		public const string UpdateDownloadAlreadyInProgress = "LocalizableUpdateDownloadAlreadyInProgress";
		public static string LocalizableUpdateDownloadAlreadyInProgress	{  get { return string.Format(CultureInfo.CurrentCulture, resources.GetString("UpdateAlreadyInProgress"), DownloadImageName); } }
		public const string UserGuidedClientAbort = "LocalizableUserGuidedClientAbort";
		public static string LocalizableUserGuidedClientAbort		{  get { return resources.GetString("UserGuidedClientAbort"); } }
		public const string ClientAbortFailed = "LocalizableClientAbortFailed";
		public static string LocalizableClientAbortFailed			{  get { return resources.GetString("ClientAbortFailed"); } }
		public const string TimerCallBack = "LocalizableTimerCallBack";
		public static string LocalizableTimerCallBack				{  get { return resources.GetString("TimerCallBack"); } }
		public const string ConnectingToServerForDownload = "LocalizableConnectingToServerForDownload";
		public static string LocalizableConnectingToServerForDownload { get { return resources.GetString("ConnectingToServerForDownload"); } }
		public const string SearchingForPatches = "LocalizableSearchingForPatches";
		public static string LocalizableSearchingForPatches			{  get { return resources.GetString("SearchingForPatches"); } }
		public const string PreparingToDelete = "LocalizablePreparingToDelete";
		public static string LocalizablePreparingToDelete			{  get { return resources.GetString("PreparingToDelete"); } }
		
		public const string UpdateDownloadSucceeded = "LocalizableUpdateDownloadSucceeded";
		public const string UpdateRunningSucceeded = "LocalizableUpdateRunningSucceeded";
		public static string LocalizableUpdateDownloadSucceeded		{  get { return string.Format(CultureInfo.CurrentCulture, resources.GetString("UpdateSucceeded"), DownloadImageName); } }
		public static string LocalizableUpdateRunningSucceeded		{  get { return string.Format(CultureInfo.CurrentCulture, resources.GetString("UpdateSucceeded"), RunningImageName); } }
		//public static string LocalizableUpdateSucceeded			{  get { return resources.GetString("UpdateSucceeded"); } }
		
		public const string NoDownloadDueToPolicies = "LocalizableNoDownloadDueToPolicies";
		public static string LocalizableNoDownloadDueToPolicies		{  get { return resources.GetString("NoDownloadDueToPolicies"); } }
		public const string ScheduledUpdateFired = "LocalizableScheduledUpdateFired";
		public static string LocalizableScheduledUpdateFired		{  get { return resources.GetString("ScheduledUpdateFired"); } }
		public const string ServicesAreNotUpToDate = "LocalizableServicesAreNotUpToDate";
		public static string LocalizableServicesAreNotUpToDate		{  get { return string.Format(CultureInfo.CurrentCulture, resources.GetString("ServicesAreNotUpToDate"), MicroareaServicesName); } }
		public const string UpdatesAreNotAvailable = "LocalizableUpdatesAreNotAvailable";
		public static string LocalizableUpdatesAreNotAvailable		{  get { return resources.GetString("UpdatesAreNotAvailable"); } }
		public const string HighestImageMinorThanInstalled = "LocalizableHighestImageMinorThanInstalled";
		public static string LocalizableHighestImageMinorThanInstalled		{  get { return resources.GetString("HighestImageMinorThanInstalled"); } }
		public const string HighestImageMinorThanInstalled2 = "LocalizableHighestImageMinorThanInstalled2";
		public static string LocalizableHighestImageMinorThanInstalled2		{  get { return resources.GetString("HighestImageMinorThanInstalled2"); } }
		public const string ContactingWebService = "LocalizableContactingWebService";
		public static string LocalizableContactingWebService		{  get { return resources.GetString("ContactingWebService"); } }
		public const string WebServiceContacted = "LocalizableWebServiceContacted";
		public static string LocalizableWebServiceContacted			{  get { return resources.GetString("WebServiceContacted"); } }
		public const string DownloadFailed = "LocalizableDownloadFailed";
		public static string LocalizableDownloadFailed				{  get { return resources.GetString("DownloadFailed"); } }
		public const string ImageNotAvailable = "LocalizableImageNotAvailable";
		public static string LocalizableImageNotAvailable			{  get { return resources.GetString("ImageNotAvailable"); } }
		public const string OldReleaseRequired = "LocalizableOldReleaseRequired";
		public static string LocalizableOldReleaseRequired			{  get { return resources.GetString("OldReleaseRequired"); } }
		public const string ProductNotAvailable = "LocalizableProductNotAvailable";
		public static string LocalizableProductNotAvailable			{  get { return resources.GetString("ProductNotAvailable"); } }
		public const string UnknownError = "LocalizableUnknownError";
		public static string LocalizableUnknownError				{  get { return resources.GetString("UnknownError"); } }
		public const string UserNotAuthorized = "LocalizableUserNotAuthorized";
		public static string LocalizableUserNotAuthorized			{  get { return resources.GetString("UserNotAuthorized"); } }
		public const string UserNotAuthenticated = "LocalizableUserNotAuthenticated";
		public static string LocalizableUserNotAuthenticated		{  get { return resources.GetString("UserNotAuthenticated"); } }
		public const string ZipError = "LocalizableZipError";
		public static string LocalizableZipError					{  get { return resources.GetString("ZipError"); } }
		public const string IOError = "LocalizableIOError";
		public static string LocalizableIOError						{  get { return resources.GetString("IOError"); } }
		public const string UnexpectedException = "LocalizableUnexpectedException";
		public static string LocalizableUnexpectedException			{  get { return resources.GetString("UnexpectedException"); } }

		public const string StatusMessage = "LocalizableStatusMessage";
		public static string LocalizableStatusMessage				{  get { return resources.GetString("StatusMessage"); } }
		private static string UpdateFailed							{  get { return resources.GetString("UpdateFailed"); } }
		public const string UpdateRunningStandardFailed = "LocalizableUpdateRunningStandardFailed";
		public static string LocalizableUpdateRunningStandardFailed	{  get { return string.Format(CultureInfo.CurrentCulture, UpdateFailed, RunningImageName); } }
		public const string UpdateDownloadFailed = "LocalizableUpdateDownloadFailed";
		public static string LocalizableUpdateDownloadFailed	{  get { return string.Format(CultureInfo.CurrentCulture, UpdateFailed, DownloadImageName); } }
		
		public const string EmptyingDownload = "LocalizableEmptyingDownload";
		public static string LocalizableEmptyingDownload		{  get { return string.Format(CultureInfo.CurrentCulture, resources.GetString("EmptyingImage"), DownloadImageName); } }
		public const string UpdateRunningStarted = "LocalizableUpdateRunningStarted";
		public static string LocalizableUpdateRunningStarted		{  get { return string.Format(CultureInfo.CurrentCulture, resources.GetString("UpdateProcessStarted"), RunningImageName); } }

		public const string ImageChangedDetails = "LocalizableImageChangedDetails";
		public static string LocalizableImageChangedDetails			{  get { return resources.GetString("ImageChangedDetails"); } }
		public const string RunningStandardLocked = "LocalizableRunningStandardLocked";
		public static string LocalizableRunningStandardLocked		{  get { return string.Format(CultureInfo.CurrentCulture, resources.GetString("LocalImageLocked"), RunningImageName); } }
		public const string DownloadLocked = "LocalizableDownloadLocked";
		public static string LocalizableDownloadLocked				{  get { return string.Format(CultureInfo.CurrentCulture, resources.GetString("LocalImageLocked"), DownloadImageName); } }

		public const string DeleteInstallationFailed			= "LocalizableDeleteInstallationFailed";
		public static string LocalizableDeleteInstallationFailed	{  get { return resources.GetString("DeleteInstallationFailed"); } }
		public const string DeleteInstallationSucceeded			= "LocalizableDeleteInstallationSucceeded";
		public static string LocalizableDeleteInstallationSucceeded	{  get { return resources.GetString("DeleteInstallationSucceeded"); } }
		public const string PleaseRestartIis					= "LocalizablePleaseRestartIis";
		public static string LocalizablePleaseRestartIis			{  get { return resources.GetString("PleaseRestartIis"); } }
		public const string LockingPrograms						= "LocalizableLockingPrograms";
		public static string LocalizableLockingPrograms				{  get { return resources.GetString("LockingPrograms"); } }
		public const string RequirementsChangesPreventUpdate	= "LocalizableRequirementsChangesPreventUpdate";
		public static string LocalizableRequirementsChangesPreventUpdate		{  get { return resources.GetString("RequirementsChangesPreventUpdate"); } }
		public const string LockRunningUnexpectedError			= "LocalizableLockRunningUnexpectedError";
		public static string LocalizableLockRunningUnexpectedError	{  get { return resources.GetString("LockRunningUnexpectedError"); } }
		public const string LockingUsers						= "LocalizableLockingUsers";
		public static string LocalizableLockingUsers				{  get { return resources.GetString("LockingUsers"); } }
		public const string RequirementsNotMet					= "LocalizableRequirementsNotMet";
		public static string LocalizableRequirementsNotMet			{  get { return resources.GetString("RequirementsNotMet"); } }

		public const string DbChangesPreventAutoUpdate = "LocalizableDbChangesPreventAutoUpdate";
		public static string LocalizableDbChangesPreventAutoUpdate	{  get { return string.Format(CultureInfo.CurrentCulture, resources.GetString("DbChangesPreventAutoUpdate"), DownloadImageName, RunningImageName); } }
		public const string PreDbChangesPreventAppUsage = "LocalizablePreDbChangesPreventAppUsage";
		public static string LocalizablePreDbChangesPreventAppUsage	{  get { return string.Format(CultureInfo.CurrentCulture, resources.GetString("PreDbChangesPreventAppUsage"), DownloadImageName, RunningImageName); } }
		public const string PostDbChangesPreventAppUsage = "LocalizablePostDbChangesPreventAppUsage";
		public static string LocalizablePostDbChangesPreventAppUsage	{  get { return string.Format(CultureInfo.CurrentCulture, resources.GetString("PostDbChangesPreventAppUsage"), RunningImageName); } }

		public const string ProcedureNotify = "LocalizableProcedureNotify";
		public static string LocalizableProcedureNotify				{  get { return resources.GetString("ProcedureNotify"); } }

		public const string DataSourceUnreachable = "LocalizableDataSourceUnreachable";
		public static string LocalizableDataSourceUnreachable				{  get { return resources.GetString("DataSourceUnreachable"); } }

		public static string QualifiedMessage						{  get { return resources.GetString("QualifiedMessage"); } }
		public static string TableReleaseChangesRowMask = "{0} --> {1} | {2}" + Environment.NewLine;//{  get { return resources.GetString("TableReleaseChangesRowMask"); } }
	
		public const string SaveInstalledProductStateFailed = "LocalizableSaveInstalledProductStateFailed";
		public static string LocalizableSaveInstalledProductStateFailed		{  get { return resources.GetString("SaveInstalledProductStateFailed"); } }

		public const string StepDelayedMessage = "LocalizableStepDelayedMessage";
		public static string LocalizableStepDelayedMessage			{  get { return resources.GetString("StepDelayedMessage"); } }

		public const string StepDelayedWsBusy = "LocalizableStepDelayedWsBusy";
		public static string LocalizableStepDelayedWsBusy			{  get { return resources.GetString("StepDelayedWsBusy"); } }

		public const string ClientConfigCalculationFailed = "LocalizableClientConfigCalculationFailed";
		public static string LocalizableClientConfigCalculationFailed			{  get { return resources.GetString("ClientConfigCalculationFailed"); } }

		public const string UnableToCompleteClientConfigCalculation = "LocalizableUnableToCompleteClientConfigCalculation";
		public static string LocalizableUnableToCompleteClientConfigCalculation	{  get { return resources.GetString("UnableToCompleteClientConfigCalculation"); } }

		public const string UnableToCompleteClientConfigCalculation2 = "LocalizableUnableToCompleteClientConfigCalculation2";
		public static string LocalizableUnableToCompleteClientConfigCalculation2	{  get { return resources.GetString("UnableToCompleteClientConfigCalculation2"); } }

		public const string UnableToRemoveUnusedModules = "LocalizableUnableToRemoveUnusedModules";
		public static string LocalizableUnableToRemoveUnusedModules				{  get { return resources.GetString("UnableToRemoveUnusedModules"); } }

		public const string UpdateRunningBlockedByUser = "LocalizableUpdateRunningBlockedByUser";
		public static string LocalizableUpdateRunningBlockedByUser				{  get { return string.Format(CultureInfo.CurrentCulture, resources.GetString("UpdateRunningBlockedByUser"), DownloadImageName, RunningImageName); } }

		public const string ServicesNotUpdatedInhibitScheduledUpdate =				"LocalizableServicesNotUpdatedInhibitScheduledUpdate";
		public static string LocalizableServicesNotUpdatedInhibitScheduledUpdate	{  get { return resources.GetString("ServicesNotUpdatedInhibitScheduledUpdate"); } }
		public const string ServicesSetupAvailableAt =								"LocalizableServicesSetupAvailableAt";
		public static string LocalizableServicesSetupAvailableAt					{  get { return resources.GetString("ServicesSetupAvailableAt"); } }

		public const string BackendReturnedAuthError =								"LocalizableBackendReturnedAuthError";
		public static string LocalizableBackendReturnedAuthError					{  get { return resources.GetString("BackendReturnedAuthError"); } }

		public const string VerticalChecksFailed =									"LocalizableVerticalChecksFailed";
		public static string LocalizableVerticalChecksFailed						{  get { return resources.GetString("VerticalChecksFailed"); } }

		public const string CheckServerEventLog =									"LocalizableCheckServerEventLog";
		public static string LocalizableCheckServerEventLog							{  get { return resources.GetString("CheckServerEventLog"); } }

		public const string DBSizeExceedsLimit =									"LocalizableDBSizeExceedsLimit";
		public static string LocalizableDBSizeExceedsLimit							{ get { return resources.GetString("DBSizeExceedsLimit"); } }

		public const string RequiredReleaseNotAllowed =								"LocalizableRequiredReleaseNotAllowed";
		public static string LocalizableRequiredReleaseNotAllowed					{ get { return resources.GetString("RequiredReleaseNotAllowed"); } }

		public const string IncompatibleMasterProductRelease =						"LocalizableIncompatibleMasterProductRelease";
		public static string LocalizableIncompatibleMasterProductRelease			{  get { return resources.GetString("IncompatibleMasterProductRelease"); } }

		public const string MasterProductNotInstalled =								"LocalizableMasterProductNotInstalled";
		public static string LocalizableMasterProductNotInstalled					{  get { return resources.GetString("MasterProductNotInstalled"); } }

		public const string UnableToLoadLicensedConfiguration =						"LocalizableUnableToLoadLicensedConfiguration";
		public static string LocalizableUnableToLoadLicensedConfiguration			{  get { return resources.GetString("UnableToLoadLicensedConfiguration"); } }

		public const string MasterInstallationNotExisting =							"LocalizableMasterInstallationNotExisting";
		public static string LocalizableMasterInstallationNotExisting				{  get { return resources.GetString("MasterInstallationNotExisting"); } }

		public const string UnableToParseVerticalCompatibility =					"LocalizableUnableToParseVerticalCompatibility";
		public static string LocalizableUnableToParseVerticalCompatibility			{  get { return resources.GetString("UnableToParseVerticalCompatibility"); } }

		public const string MasterProductNotPresent =								"LocalizableMasterProductNotPresent";
		public static string LocalizableMasterProductNotPresent						{  get { return resources.GetString("MasterProductNotPresent"); } }

		public const string MasterProductNotProperlyInstalled =						"LocalizableMasterProductNotProperlyInstalled";
		public static string LocalizableMasterProductNotProperlyInstalled			{  get { return resources.GetString("MasterProductNotProperlyInstalled"); } }

		public const string DependenciesNotMet =									"LocalizableDependenciesNotMet";
		public static string LocalizableDependenciesNotMet							{  get { return resources.GetString("DependenciesNotMet"); } }

		public const string CalculatingPhysicalConfiguration =						"LocalizableCalculatingPhysicalConfiguration";
		public static string LocalizableCalculatingPhysicalConfiguration			{  get { return resources.GetString("CalculatingPhysicalConfiguration"); } }

		public const string UpdateDownloadWouldBreakSlavesCompatibility =					"LocalizableUpdateDownloadWouldBreakSlavesCompatibility";
		public static string LocalizableUpdateDownloadWouldBreakSlavesCompatibility			{  get { return resources.GetString("UpdateDownloadWouldBreakSlavesCompatibility"); } }
		public const string UpdateDownloadWouldBreakSlavesCompatibilityIgnored =			"LocalizableUpdateDownloadWouldBreakSlavesCompatibilityIgnored";
		public static string LocalizableUpdateDownloadWouldBreakSlavesCompatibilityIgnored	{  get { return resources.GetString("UpdateDownloadWouldBreakSlavesCompatibilityIgnored"); } }

		public const string UpdateRunningCompatibilityFailure =								"LocalizableUpdateRunningCompatibilityFailure";
		public static string LocalizableUpdateRunningCompatibilityFailure					{  get { return resources.GetString("UpdateRunningCompatibilityFailure"); } }
		public const string UpdateRunningCompatibilityFailureIgnored =						"LocalizableUpdateRunningCompatibilityFailureIgnored";
		public static string LocalizableUpdateRunningCompatibilityFailureIgnored			{  get { return resources.GetString("UpdateRunningCompatibilityFailureIgnored"); } }

		public const string UpdateRunningCompatibilityPreserved =							"LocalizableUpdateRunningCompatibilityPreserved";
		public static string LocalizableUpdateRunningCompatibilityPreserved					{  get { return resources.GetString("UpdateRunningCompatibilityPreserved"); } }

		public const string UpdateRunningOrphanPathsArePresent =							"LocalizableUpdateRunningOrphanPathsArePresent";
		public static string LocalizableUpdateRunningOrphanPathsArePresent					{  get { return resources.GetString("UpdateRunningOrphanPathsArePresent"); } }

		public const string DevEnvironmentPreventAutoUpdate =								"LocalizableDevEnvironmentPreventAutoUpdate";
		public static string LocalizableDevEnvironmentPreventAutoUpdate						{  get { return resources.GetString("DevEnvironmentPreventAutoUpdate"); } }
		public const string DevEnvironmentInstallationUpdate =								"LocalizableDevEnvironmentInstallationUpdate";
		public static string LocalizableDevEnvironmentInstallationUpdate					{  get { return resources.GetString("DevEnvironmentInstallationUpdate"); } }

		public const string IncompatibleActivationVersion =									"LocalizableIncompatibleActivationVersion";
		public static string LocalizableIncompatibleActivationVersion						{  get { return resources.GetString("IncompatibleActivationVersion"); } }

		public const string UnableToReadEula =				"LocalizableUnableToReadEula";
		public static string LocalizableUnableToReadEula	{  get { return resources.GetString("UnableToReadEula"); } }
		public const string UnableToReadMlu =				"LocalizableUnableToReadMlu";
		public static string LocalizableUnableToReadMlu		{  get { return resources.GetString("UnableToReadMlu"); } }

		public const string LoginManagerNotExistingException =				"LocalizableLoginManagerNotExistingException";
		public static string LocalizableLoginManagerNotExistingException	{  get { return resources.GetString("LoginManagerNotExistingException"); } }
		public const string LoginManagerCommunicationException =				"LocalizableLoginManagerCommunicationException";
		public static string LocalizableLoginManagerCommunicationException		{  get { return resources.GetString("LoginManagerCommunicationException"); } }

	
		public const string AddOnRemovedDetails = "LocalizableAddOnRemovedDetails";
		public static string LocalizableAddOnRemovedDetails					{  get { return resources.GetString("AddOnRemovedDetails"); } }
		public const string AddOnRemovalFailed = "LocalizableAddOnRemovalFailed";
		public static string LocalizableAddOnRemovalFailed					{  get { return resources.GetString("AddOnRemovalFailed"); } }
		public const string AddOnRemovalOnMasterFailure = "LocalizableAddOnRemovalOnMasterFailure";
		public static string LocalizableAddOnRemovalOnMasterFailure			{  get { return resources.GetString("AddOnRemovalOnMasterFailure"); } }
		public const string AddOnRemovalOnDevInstanceFailure = "LocalizableAddOnRemovalOnDevInstanceFailure";
		public static string LocalizableAddOnRemovalOnDevInstanceFailure	{  get { return resources.GetString("AddOnRemovalOnDevInstanceFailure"); } }
	}

	//=========================================================================
	[Serializable]
	public class LocalizedString
	{
		protected string stringName;
		protected object[] stringParams;
		protected static Type type = typeof(Events);
		public string UnlocalizableMessage;

		//---------------------------------------------------------------------
		public string StringName
		{
			get
			{
				return stringName;
			}
		}

		//---------------------------------------------------------------------
		public object[] StringParams
		{
			get
			{
				return stringParams;
			}
		}

		//---------------------------------------------------------------------
		public LocalizedString(string stringName, object[] stringParams) : this(stringName, stringParams, null)
		{
		}
		public LocalizedString(string stringName, object[] stringParams, string unlocalizableMessage)
		{
			this.stringName = stringName;
			this.stringParams = stringParams;
			this.UnlocalizableMessage = unlocalizableMessage;
		}

		//---------------------------------------------------------------------
		public override string ToString()
		{
			string message;
			System.Reflection.PropertyInfo pi = null;
			try { pi = type.GetProperty(stringName); } 
			catch {}
			if (pi == null)
			{
				Debug.Fail("Errore: Stringa non definita!");
				return string.Empty;
			}
			message = (string)pi.GetValue(null, null);
			if (stringParams != null)
				return string.Format(CultureInfo.CurrentCulture, message, stringParams);
			else
				return message;
		}
	}

	//=========================================================================
	public delegate void UpdaterEventHandler(object sender, UpdaterEventArgs e);
	public delegate void MachineEventHandler(object sender, SynchDownloadEventArgs e);
	public delegate void UpdaterFileHandlingEventHandler(object sender, UpdaterFileHandlingEventArgs e);
	public delegate void StepEventHandler(object sender, StepEventArgs e);
	public delegate void StepDelayedEventHandler(object sender, StepDelayedEventArgs e);

	//=========================================================================
	/// <summary>
	/// Eventi sollevati da InstallationUpdater.
	/// </summary>
	[Serializable]
	public class UpdaterEventArgs : System.EventArgs
	{
		protected LocalizedString message;	// messaggio testuale
		protected LocalizedString details;	// dettagli del messaggio
		
		private DateTime time;	// ora in cui è avvenuto l'evento
		public readonly string Installation;
		public string Product;
		public readonly string BrandedProduct;
		private bool isSucceeded = false;

		#region Constructors
		/// <summary>
		/// Crea un evento senza messaggio testuale
		/// </summary>
		/// <param name="updaterQualifier"></param>
		//---------------------------------------------------------------------
		public UpdaterEventArgs(UpdaterQualifier updaterQualifier)
			: this(updaterQualifier, null)
		{
		}

		/// <summary>
		/// Crea un evento specificando un messaggio testuale senza dettagli
		/// </summary>
		/// <param name="updaterQualifier"></param>
		/// <param name="message">messaggio testuale</param>
		//---------------------------------------------------------------------
		public UpdaterEventArgs(UpdaterQualifier updaterQualifier, LocalizedString message) 
			: this(updaterQualifier, message, null)
		{
		}

		/// <summary>
		/// Crea un evento specificando un messaggio testuale senza dettagli
		/// </summary>
		/// <param name="updaterQualifier"></param>
		/// <param name="message">messaggio testuale</param>
		/// <param name="details"></param>
		//---------------------------------------------------------------------
		public UpdaterEventArgs(UpdaterQualifier updaterQualifier, LocalizedString message, LocalizedString details) 
			: this(updaterQualifier, message, details, false)
		{
		}

		/// <summary>
		/// Crea un evento specificando un messaggio testuale e dettagli
		/// </summary>
		/// <param name="updaterQualifier"></param>
		/// <param name="message">messaggio testuale</param>
		/// <param name="details">dettagli del messaggio</param>
		/// <param name="succeeded">booleano che indica se è un msg di successo (default false)</param>
		//---------------------------------------------------------------------
		public UpdaterEventArgs(UpdaterQualifier updaterQualifier, LocalizedString message, LocalizedString details, bool succeeded)
		{
			this.time			= DateTime.Now; // cannot be initialized as UTC as the xml serialization would fail
			this.Installation	= updaterQualifier.Installation;
			this.Product		= updaterQualifier.Product;
			this.BrandedProduct	= updaterQualifier.BrandedProduct;
			this.message		= message;
			this.Details		= details;
			this.isSucceeded	= succeeded;
		}
		#endregion

		#region Properties
		/// <summary>
		/// Legge o imposta il messaggio testuale
		/// </summary>
		//---------------------------------------------------------------------
		public string Message	{ get { return (message != null) ? message.ToString() : string.Empty;	}/*	set { message = value; }*/}
		public virtual string MessageExtended
		{
			get
			{
				if (message != null)
				{
					string pName = (this.BrandedProduct != null && this.BrandedProduct.Length != 0) ? this.BrandedProduct : this.Product;
					object[] args = new object[] {message.ToString(), this.Installation, pName};
					return string.Format(CultureInfo.CurrentCulture, Events.QualifiedMessage, args);
				}
				else return null;
				//return (message != null) ? message.ToString() : null;	
			}
			/*	set { message = value; }*/
		}

		/// <summary>
		/// Legge o imposta il messaggio testuale
		/// </summary>
		//---------------------------------------------------------------------
		public LocalizedString Details	{ get { return details;	}	set { details = value; }}

		/// <summary>
		/// Legge l'ora in cui è sorto l'evento
		/// </summary>
		//---------------------------------------------------------------------
		public DateTime Time	{ get { return time;	}}

		/// <summary>
		/// Specifica se è un evento di successo o meno
		/// </summary>
		//---------------------------------------------------------------------
		public bool IsSucceeded	{ get { return isSucceeded;}}
		#endregion
	
		//---------------------------------------------------------------------------
		public virtual string GetLogFragment()
		{
			using (StringWriter writerString = new StringWriter(CultureInfo.InvariantCulture))
			{
				XmlTextWriter writer = new XmlTextWriter(writerString);
				writer.WriteStartElement(Element.Event);
				WriteLogFragment(writer);
				writer.WriteEndElement();
				writer.Flush();
				writer.Close();
				return writerString.ToString();
			}
		}

		//---------------------------------------------------------------------------
		protected virtual void WriteLogFragment(XmlTextWriter writer)
		{
			writer.WriteElementString(Element.UtcTime,	this.Time.ToUniversalTime().ToString("s", CultureInfo.InvariantCulture));
			string[] typeNS = this.GetType().ToString().Split('.');
			writer.WriteElementString(Element.Type,		typeNS[typeNS.Length - 1]);
			if (this.message != null)
				WriteLocalizableStringFragment(writer, Element.Message, this.message);
			if (this.details != null)
				WriteLocalizableStringFragment(writer, Element.Details, this.details);
		}

		//---------------------------------------------------------------------------
		protected virtual void WriteLocalizableStringFragment(XmlTextWriter writer, string elName, LocalizedString localizable)
		{
			writer.WriteStartElement(elName);
			writer.WriteString(localizable.ToString());
			if (localizable.UnlocalizableMessage != null && localizable.UnlocalizableMessage.Length != 0)
			{
				writer.WriteStartElement(Element.Unlocalizable);
				writer.WriteString(localizable.UnlocalizableMessage);
				writer.WriteEndElement();
			}
			writer.WriteEndElement();
		}

		// definizione dei propri tag necessari per il log xml
		public class Element	// TODO ? spostare in namesolver?
		{
			public const string Event			= "Event";
			public const string UtcTime			= "UtcTime";
			public const string Type			= "Type";
			public const string Message			= "Message";
			public const string Details			= "Details";
			public const string Unlocalizable	= "Unlocalizable";
		}
	}

	//=========================================================================
	[Serializable]
	public class StepDelayedEventArgs : UpdaterEventArgs
	{
		public readonly double NewDelayMilliseconds;
		public readonly bool WebServerBusy;

		//---------------------------------------------------------------------
		public StepDelayedEventArgs
			(
			UpdaterQualifier updaterQualifier,
			double newDeplay,
			bool wsBusy
			)
			: base(updaterQualifier)
		{
			this.NewDelayMilliseconds = newDeplay;
			this.WebServerBusy = wsBusy;
			
			double secs = Math.Ceiling(this.NewDelayMilliseconds/1000);
			if (wsBusy)
				this.message = new LocalizedString(Events.StepDelayedWsBusy, new object[]{secs});
			else
				this.message = new LocalizedString(Events.StepDelayedMessage, new object[]{secs});
		}
	}

	//=========================================================================
	[Serializable]
	public abstract class StepEventArgs : UpdaterEventArgs
	{
		public readonly ImageTask Task = ImageTask.Unknown;
		public readonly int ActualStep;
		public readonly int TotalSteps;

		//---------------------------------------------------------------------
		public StepEventArgs
			(
			ImageTask task,
			UpdaterQualifier updaterQualifier,
			int actualStep,
			int totalSteps,
			LocalizedString message
			)
			: base(updaterQualifier, message)
		{
			Task = task;
			TotalSteps = totalSteps;
			ActualStep = actualStep;

			if (message == null && totalSteps != -1)
				this.message = new LocalizedString(Events.StepMessage, new object[] {actualStep, totalSteps});
		}

		//---------------------------------------------------------------------------
		public virtual bool Abortable	{ get { return false; } }

		//---------------------------------------------------------------------------
		public static int GetStepNumber(SynchAndRollPhases step, SynchAndRollPhases[] steps)
		{
			for (int i = 0; i < steps.Length; i++)
				if (step == steps[i])
					return i + 1;
			return 0;
		}

		//---------------------------------------------------------------------------
		public static string GetStatusMessage(SynchAndRollPhases phase)
		{
			switch (phase)
			{
				case SynchAndRollPhases.LockingDownload :
					return Events.LockingDownload;
				case SynchAndRollPhases.UnlockingDownload :
					return Events.UnlockingDownload;
				case SynchAndRollPhases.SavingUpdatedState :
					return Events.SavingUpdatedState;
				case SynchAndRollPhases.LockingRunning :
					return Events.LockingRunning;
				case SynchAndRollPhases.RollingbackRunning :
					return Events.RollingbackRunning;
				case SynchAndRollPhases.UnlockingRunning :
					return Events.UnlockingRunning;
				case SynchAndRollPhases.EmptyingDownload :
					return Events.EmptyingDownload;
				case SynchAndRollPhases.SynchronizingContent :
					return Events.SynchronizingContent;
				case SynchAndRollPhases.ApplyingPatches :
					return Events.ApplyingPatches;
				case SynchAndRollPhases.SettingServices :
					return Events.SettingServices;
				case SynchAndRollPhases.CheckingRequirementsChanges :
					return Events.CheckingRequirementsChanges;
				case SynchAndRollPhases.CheckingDatabaseChanges :
					return Events.CheckingDatabaseChanges;
				case SynchAndRollPhases.RemovingUnnecessaryFiles :
					return Events.RemovingUnnecessaryFiles;
				case SynchAndRollPhases.RemovingUnnecessaryFilesAfterPatching :
					return Events.RemovingUnnecessaryFiles;
				case SynchAndRollPhases.RemovingUnusedModules :
					return Events.RemovingUnusedModules;
				case SynchAndRollPhases.UpdateMicroareaClient :
					return Events.UpdateMicroareaClient;
				case SynchAndRollPhases.CheckingInterProductCompatibility :
					return Events.CheckingInterProductCompatibility;
				default :
					Debug.Fail("case non gestito in costrutto switch.");
					return string.Empty;
			}
		}

		//---------------------------------------------------------------------------
	}
	
	#region Classi specializzazioni di step
	//=========================================================================
	/// <summary>
	/// </summary>
	[Serializable]
	public class SynchDownloadEventArgs : StepEventArgs
	{
		public readonly MachineStates CurrentStatus;
		private DataSourceType dataSource;

		private static MachineStates[] WsSteps =
			{
				MachineStates.UpdatesExistenceConfirmed,
				MachineStates.WaitingForFtpCredentials,
				MachineStates.HavingFtpCredentials,
				MachineStates.Downloading,
				MachineStates.EndDownloadSignaled,
				MachineStates.InflatingUpdates,
				MachineStates.UpdatesInflated,
				MachineStates.Updating,
				MachineStates.Idle
			};

		private static MachineStates[] CdSteps =
			{
				MachineStates.UpdatesExistenceConfirmed,
				MachineStates.Copying,
				MachineStates.Updating,
				MachineStates.Idle
			};

		#region Constructors
		/// <summary>
		/// Crea un evento specificando un messaggio testuale
		/// </summary>
		/// <param name="installation">nome dell'installazione</param>
		/// <param name="product">nome del prodotto</param>
		/// <param name="message">messaggio testuale</param>
		/// <param name="dataSource">sorgente dati</param>
		/// <param name="status">stato attuale</param>
		//---------------------------------------------------------------------
		public SynchDownloadEventArgs
			(
			UpdaterQualifier updaterQualifier,
			DataSourceType dataSource, 
			MachineStates status
			)
			: base
				(
				ImageTask.SynchDownload,
				updaterQualifier, 
				GetStepNumber(status, dataSource), 
				GetTotalSteps(dataSource), 
				null
				)
		{
			this.dataSource = dataSource;
			CurrentStatus = status;
			this.message = new LocalizedString(GetStatusMessage(status), null);
		}
		#endregion

		//---------------------------------------------------------------------
		#region Properties
		public override bool Abortable
		{
			get
			{
				if (CurrentStatus == MachineStates.UpdatesExistenceConfirmed	||
					CurrentStatus == MachineStates.WaitingForFtpCredentials	||
					CurrentStatus == MachineStates.HavingFtpCredentials	||
					CurrentStatus == MachineStates.Downloading	||
					CurrentStatus == MachineStates.EndDownloadSignaled	||
					//CurrentStatus == MachineStates.InflatingUpdates	||
					//CurrentStatus == MachineStates.UpdatesInflated	||	// dura troppo poco
					//CurrentStatus == MachineStates.Updating	|| // sicuramente no perché tocca la download
					CurrentStatus == MachineStates.Idle
					)
					return true;
				return false;
			}
		}
		#endregion

		//---------------------------------------------------------------------
		public static string GetStatusMessage(MachineStates status)
		{
			switch (status)
			{
				case MachineStates.UpdatesExistenceConfirmed :
					return Events.UpdatesAreAvailable;
				case MachineStates.WaitingForFtpCredentials :
					return Events.WaitingForFtpCredentials;
				case MachineStates.HavingFtpCredentials :
					return Events.HavingFtpCredentials;
				case MachineStates.Downloading :
					return Events.Downloading;
				case MachineStates.EndDownloadSignaled :
					return Events.EndDownloadSignaled;
				case MachineStates.InflatingUpdates :
					return Events.InflatingUpdates;
				case MachineStates.UpdatesInflated :
					return Events.UpdatesInflated;
				case MachineStates.Copying :
					return Events.Copying;
				case MachineStates.Updating :
					return Events.Updating;
				case MachineStates.Idle :
					return Events.Idle;
				default:
					Debug.Fail("switch case non gestito.");
					return status.ToString();
			}
		}

		//---------------------------------------------------------------------
		public static int GetStepNumber(MachineStates step, DataSourceType dataSource)
		{
			MachineStates[] steps;
			switch (dataSource)
			{
				case DataSourceType.WebService :
					steps = WsSteps;
					break;
				case DataSourceType.CompactDisc :
					steps = CdSteps;
					break;
				default :
//					Debug.Fail("switch case non gestito");
//					return 0;
					steps = CdSteps;
					break;
			}

			for (int i = 0; i < steps.Length; i++)
				if (step == steps[i])
					return i + 1;
			return 0;
		}

		//---------------------------------------------------------------------
		public static int GetTotalSteps(DataSourceType dataSource)
		{
			switch (dataSource)
			{
				case DataSourceType.WebService :
					return WsSteps.Length;
				case DataSourceType.CompactDisc :
					return CdSteps.Length;
				default :
//					Debug.Fail("switch case non gestito");
//					return 0;
					return CdSteps.Length;
			}
		}

		//---------------------------------------------------------------------
	}

	//=========================================================================
	public enum SynchAndRollPhases
	{
		LockingDownload,
		UnlockingDownload,
		SavingUpdatedState,
		LockingRunning,
		RollingbackRunning,
		UnlockingRunning,
		EmptyingDownload,
		SynchronizingContent,
		ApplyingPatches,
		SettingServices,
		CheckingRequirementsChanges,
		CheckingDatabaseChanges,
		RemovingUnnecessaryFiles,
		RemovingUnnecessaryFilesAfterPatching,
		RemovingUnusedModules,
		UpdateMicroareaClient,
		CheckingInterProductCompatibility,
		Unknown = -1
	}
	
	//=========================================================================
	/// <summary>
	/// </summary>
	[Serializable]
	public class SynchRunningEventArgs : StepEventArgs
	{
		public readonly SynchAndRollPhases CurrentStatus;

		//---------------------------------------------------------------------------
		private static SynchAndRollPhases[] SynchRunningPhases =
		{
			SynchAndRollPhases.CheckingInterProductCompatibility,
			SynchAndRollPhases.CheckingRequirementsChanges,
			SynchAndRollPhases.CheckingDatabaseChanges,
			SynchAndRollPhases.LockingRunning,
			SynchAndRollPhases.SynchronizingContent,
			SynchAndRollPhases.RemovingUnnecessaryFiles,
			SynchAndRollPhases.ApplyingPatches,
			SynchAndRollPhases.RemovingUnnecessaryFilesAfterPatching,
			SynchAndRollPhases.RemovingUnusedModules,
			SynchAndRollPhases.SavingUpdatedState,
			SynchAndRollPhases.SettingServices,
			SynchAndRollPhases.UpdateMicroareaClient,
			SynchAndRollPhases.UnlockingRunning
		};

		//---------------------------------------------------------------------------
		public SynchRunningEventArgs
			(
				UpdaterQualifier updaterQualifier,
				SynchAndRollPhases phase
			)
			: base
			(
				ImageTask.SynchRunning,
				updaterQualifier, 
				GetStepNumber(phase, SynchRunningPhases), 
				SynchRunningPhases.Length, 
				null
			)
		{
			CurrentStatus = phase;
			this.message = new LocalizedString(GetStatusMessage(phase), null);
		}

		//---------------------------------------------------------------------------
		public static SynchAndRollPhases[] Phases { get { return SynchRunningPhases; } }
	}

	//=========================================================================
	/// <summary>
	/// </summary>
	[Serializable]
	public class DeleteInstallationEventArgs : StepEventArgs
	{
		public readonly DeleteInstallationPhases CurrentStatus;
		private static Type enumType = typeof(DeleteInstallationPhases);

		public DeleteInstallationEventArgs
			(
				string installation, 
				DeleteInstallationPhases phase
			)
			: base
			(
				ImageTask.Unknown,
				new UpdaterQualifier(installation, string.Empty), 
				((int)phase),// + 1), 
				Enum.GetValues(typeof(DeleteInstallationPhases)).Length, 
				null
			)
		{
			CurrentStatus = phase;
			this.message = new LocalizedString(Events.GetDeleteInstallationPhaseString(phase), null);
		}
	}
	public enum DeleteInstallationPhases
			{
				LockingRunning,
				RemoveUpdaters,
				RemoveInstallationDirectory,
				UnlockingRunning,
				RemovingLocalClientDirectory
			};


	//=========================================================================
	/// <summary>
	/// </summary>
	[Serializable]
	public class RemoveAddOnEventArgs : StepEventArgs
	{
		public readonly RemoveAddOnPhases CurrentStatus;
		private static Type enumType = typeof(DeleteInstallationPhases);

		public RemoveAddOnEventArgs
			(
				UpdaterQualifier updaterQualifier, 
				RemoveAddOnPhases phase
			)
			: base
			(
				ImageTask.RemoveAddOnProduct,
				updaterQualifier, 
				((int)phase),// + 1), 
				Enum.GetValues(typeof(RemoveAddOnPhases)).Length, 
				null
			)
		{
			CurrentStatus = phase;
			this.message = new LocalizedString(Events.GetRemoveAddOnPhaseString(phase), null);
		}
	}
	public enum RemoveAddOnPhases
	{
		LockingRunning,
		RemoveUpdater,
		RemoveConfigFiles,
		RemovingUnusedModules,
		RemovingUnnecessaryFiles,
		SettingServices,
		RemoveClientCaches,
		UnlockingRunning
	};
	#endregion

	#region Classi di gestione files
	//=========================================================================
	[Serializable]
	public abstract class UpdaterFileHandlingEventArgs : UpdaterEventArgs
	{
		protected readonly string File;
		public string RelativeFilePath = string.Empty;
		public readonly ImageTask Task = ImageTask.Unknown;
		public int TotalProgress	= -1;
		public int CurrentProgress	= -1;

		public UpdaterFileHandlingEventArgs
			(
			ImageTask task,
			string file,
			UpdaterQualifier updaterQualifier
			)
			: base (updaterQualifier)
		{
			this.File = file;
			this.Task = task;
		}
		protected string GetRelativeFilePath(string file, string basePath)
		{
			return file.Substring(basePath.Length);
		}

		public abstract string FileMessage { get; }

		//---------------------------------------------------------------------------
		protected override void WriteLogFragment(XmlTextWriter writer)
		{
			base.WriteLogFragment(writer);
			writer.WriteElementString(Element.FileMessage,	this.FileMessage);
			writer.WriteElementString(Element.Task,			this.Task.ToString());
		}
		
		// definizione dei propri tag necessari per il log xml
		public new class Element: UpdaterEventArgs.Element	// TODO ? spostare in namesolver?
		{
			public const string FileMessage	= "FileMessage";
			public const string Task		= "Task";
		}
	}

	//=========================================================================
	[Serializable]
	public class UpdaterFileBackupEventArgs : UpdaterFileHandlingEventArgs
	{
		public readonly string BackupFile;
		public UpdaterFileBackupEventArgs
			(
			ImageTask task,
			string file,
			string backupFile,
			string basePath,
			UpdaterQualifier updaterQualifier
			)
			: base (task, file, updaterQualifier)
		{
			this.BackupFile = backupFile;
			this.RelativeFilePath = GetRelativeFilePath(file, basePath);
		}

		public string OriginalFile { get { return this.File; }}
		public override string FileMessage { get { return string.Format(CultureInfo.CurrentCulture, Events.MsgBackingUpFile, this.RelativeFilePath); } }
	}

	//=========================================================================
	[Serializable]
	public class UpdaterFileDeleteEventArgs : UpdaterFileHandlingEventArgs
	{
		public UpdaterFileDeleteEventArgs
			(
			ImageTask task,
			string file,
			string basePath, 
			UpdaterQualifier updaterQualifier
			)
			: base (task, file, updaterQualifier)
		{
			this.RelativeFilePath = GetRelativeFilePath(file, basePath);
		}

		public string OriginalFile { get { return this.File; }}
		public override string FileMessage { get { return string.Format(CultureInfo.CurrentCulture, Events.MsgDeletingFile, this.RelativeFilePath); } }
	}

	//=========================================================================
	[Serializable]
	public class UpdaterFileMoveEventArgs : UpdaterFileHandlingEventArgs
	{
		public readonly string DestinationFile;
		public UpdaterFileMoveEventArgs
			(
			ImageTask task,
			string file,
			string destinationFile, 
			string basePath,
			UpdaterQualifier updaterQualifier
			)
			: base (task, file, updaterQualifier)
		{
			this.DestinationFile = destinationFile;
			this.RelativeFilePath = GetRelativeFilePath(file, basePath);
		}

		public string OriginalFile { get { return this.File; }}
		public override string FileMessage { get { return string.Format(CultureInfo.CurrentCulture, Events.MsgMovingFile, this.RelativeFilePath); } }
	}

	//=========================================================================
	[Serializable]
	public class UpdaterFileCopyEventArgs : UpdaterFileHandlingEventArgs
	{
		public readonly string DestinationFile;
		public UpdaterFileCopyEventArgs
			(
			ImageTask task,
			string file,
			string destinationFile, 
			string basePath,
			UpdaterQualifier updaterQualifier
			)
			: base (task, file, updaterQualifier)
		{
			this.DestinationFile = destinationFile;
			this.RelativeFilePath = GetRelativeFilePath(destinationFile, basePath);
		}
		public UpdaterFileCopyEventArgs
			(
			string relativeFilePath,
			ImageTask task,
			string originBasePath, 
			string destinationBasePath, 
			UpdaterQualifier updaterQualifier
			)
			: base (task, Path.Combine(originBasePath, relativeFilePath), updaterQualifier)
		{
			this.DestinationFile = Path.Combine(destinationBasePath, relativeFilePath);
			this.RelativeFilePath = relativeFilePath;
		}

		public string OriginalFile { get { return this.File; }}
		public override string FileMessage { get { return string.Format(CultureInfo.CurrentCulture, Events.MsgCopyingFile, this.RelativeFilePath); } }
	}

	//=========================================================================
	[Serializable]
	public class UpdaterDirectoryMoveEventArgs : UpdaterFileHandlingEventArgs
	{
		public readonly string DestinationFile;
		public UpdaterDirectoryMoveEventArgs
			(
			ImageTask task,
			string file,
			string destinationFile, 
			string basePath,
			UpdaterQualifier updaterQualifier
			)
			: base (task, file, updaterQualifier)
		{
			this.DestinationFile = destinationFile;
			this.RelativeFilePath = GetRelativeFilePath(destinationFile, basePath);
		}

		public string OriginalFile { get { return this.File; }}
		public override string FileMessage { get { return string.Format(CultureInfo.CurrentCulture, Events.MsgMovingDirectory, this.RelativeFilePath); } }
	}

	//=========================================================================
	[Serializable]
	public class UpdaterModuleCopyUpdatesEventArgs : UpdaterFileHandlingEventArgs
	{
		public UpdaterModuleCopyUpdatesEventArgs
			(
			ImageTask task,
			string moduleRelPath,
			int totModules,
			int currentModule,
			UpdaterQualifier updaterQualifier
			)
			: base (task, moduleRelPath, updaterQualifier)
		{
			this.RelativeFilePath	= moduleRelPath;
			this.TotalProgress		= totModules;
			this.CurrentProgress	= currentModule;
		}

		public override string FileMessage { get { return string.Format(CultureInfo.CurrentCulture, Events.MsgCopyingModuleFiles, this.RelativeFilePath); } }
	}

	//=========================================================================
	[Serializable]
	public class UpdaterPatchingUnitEventArgs : UpdaterFileHandlingEventArgs
	{
		public UpdaterPatchingUnitEventArgs
			(
			string unitRelPath,
			int totPatches,
			int currentPatch,
			string installation
			)
			: base (ImageTask.SynchRunning, unitRelPath, new UpdaterQualifier(installation, string.Empty))
		{
			this.RelativeFilePath	= unitRelPath;
			this.TotalProgress		= totPatches;
			this.CurrentProgress	= currentPatch;
		}

		public override string FileMessage { get { return string.Format(CultureInfo.CurrentCulture, Events.MsgPatchingUnit, this.RelativeFilePath); } }
	}

	//=========================================================================
	[Serializable]
	public class UpdaterFileUnzipEventArgs : UpdaterFileHandlingEventArgs
	{
		public UpdaterFileUnzipEventArgs
			(
			ImageTask task,
			string file,
			UpdaterQualifier updaterQualifier,
			int totalFiles,
			int currentFiles
			)
			: base (task, file, updaterQualifier)
		{
			this.RelativeFilePath = file;
			this.TotalProgress		= totalFiles;
			this.CurrentProgress	= currentFiles;
		}

		public override string FileMessage { get { return string.Format(CultureInfo.CurrentCulture, Events.MsgExtractingFile, this.RelativeFilePath); } }
	}
	#endregion

	//=========================================================================
	[Serializable]
	public class MachineFtpEventArgs : UpdaterEventArgs
	{
		public readonly int BytesTransferred;
		public readonly int FileSize;
		public readonly DateTime StartTime;
		public readonly DateTime EventTime;

		//---------------------------------------------------------------------
		public MachineFtpEventArgs
			(
				UpdaterQualifier updaterQualifier,
				int bytesTransferred, 
				int fileSize,
				DateTime startTime,
				DateTime eventTime
				)
			: base (updaterQualifier)
		{
			this.BytesTransferred	= bytesTransferred;
			this.FileSize			= fileSize;
			this.StartTime			= startTime.ToLocalTime();
			this.EventTime			= eventTime.ToLocalTime();

			this.message = new LocalizedString
					(
					Events.MsgDownloadPercent,
					new object[]
						{
							bytesTransferred, 
							fileSize, 
							String.Format(CultureInfo.CurrentCulture, "{0:F2}", this.EstimatedSpeed), 
							String.Format(CultureInfo.CurrentCulture, "{0:F1}", this.Percentage)
						}
					);
		}
		
		//---------------------------------------------------------------------
		public int Percentage
		{
			get { return GetPercent(BytesTransferred, FileSize); }
		}
		
		//---------------------------------------------------------------------
		public static int GetPercent(int bytesTransferred, int fileSize)
		{
			return unchecked((int)((100 * (long)bytesTransferred) / fileSize));
		}

		//---------------------------------------------------------------------
		public double EstimatedSpeed	// KB/s
		{
			get { return ((long)BytesTransferred * 1000) / (ElapsedTime.TotalMilliseconds * 1024); }
		}

		//---------------------------------------------------------------------
		public TimeSpan ElapsedTime
		{
			get { return EventTime.Subtract(StartTime); }
		}

		/*
		//---------------------------------------------------------------------
		public TimeSpan EstimatedTotalTime
		{
			get { return new TimeSpan((long)(ElapsedTime.Ticks * EstimatedSpeed)); }
		}

		//---------------------------------------------------------------------
		public TimeSpan EstimatedLeftTime
		{
			get { return EstimatedTotalTime.Subtract(ElapsedTime); }
		}
		*/
	}

	//=========================================================================
	[Serializable]
	public abstract class UpdateRunningEventArgs : UpdaterEventArgs
	{
		public readonly IgnoreParameters IgnoreParameters;

		public UpdateRunningEventArgs
			(
			string installation,
			IgnoreParameters ignoreParameters
			)
			: this(new UpdaterQualifier(installation, string.Empty), ignoreParameters) { }
		public UpdateRunningEventArgs
			(
			UpdaterQualifier updaterQualifier,
			IgnoreParameters ignoreParameters
			)
			: base(updaterQualifier)
		{
			this.IgnoreParameters = ignoreParameters;
		}
	}

	[Serializable]
	public struct IgnoreParameters
	{
		public readonly bool IgnoreRequirementChanges;
		public readonly bool IgnoreDatabaseChanges;
		public readonly bool IgnoreLoggedUsers;
		public readonly bool IgnoreCompatibilityFailure;
		public readonly bool DoNotWarnForDevEnvironment;

		public IgnoreParameters
			(
			bool ignoreRequirementChanges, 
			bool ignoreDatabaseChanges,
			bool ignoreLoggedUsers,
			bool ignoreCompatibilityFailure,
			bool doNotWarnForDevEnvironment
			)
		{
			this.IgnoreRequirementChanges	= ignoreRequirementChanges;
			this.IgnoreDatabaseChanges		= ignoreDatabaseChanges;
			this.IgnoreLoggedUsers			= ignoreLoggedUsers;
			this.IgnoreCompatibilityFailure	= ignoreCompatibilityFailure;
			this.DoNotWarnForDevEnvironment = doNotWarnForDevEnvironment;
		}
	}

	[Serializable]
	public class UpdateRunningRequirementsChangedEventArgs : UpdateRunningEventArgs
	{
		public readonly string[] NewRequirementsList = null;

		public UpdateRunningRequirementsChangedEventArgs
			(
			string installation, 
			IgnoreParameters ignoreParameters,
			string[] newRequirementsList
			)
			: base (installation, ignoreParameters)
		{
			this.NewRequirementsList = newRequirementsList;
			this.message = new LocalizedString(Events.RequirementsChangesPreventUpdate, new object[] {Events.DownloadImageName, Events.RunningImageName});

			StringBuilder txt = new StringBuilder();
			foreach (string requirement in this.NewRequirementsList)
				txt.Append(Environment.NewLine + requirement);
			this.details = new LocalizedString(Events.RequirementsNotMet, new object[] {txt.ToString()});
		}
		public void SetMessage(LocalizedString message)
		{
			this.message = message;
		}
	}

	[Serializable]
	public class UpdateRunningDevEnvironmentEventArgs : UpdateRunningEventArgs
	{
		public UpdateRunningDevEnvironmentEventArgs
			(
			string installation, 
			IgnoreParameters ignoreParameters,
			bool scheduled
			)
			: base (installation, ignoreParameters)
		{
			if (scheduled)
				this.message = new LocalizedString(Events.DevEnvironmentPreventAutoUpdate, new string[] {installation});
			else
				this.message = new LocalizedString(Events.DevEnvironmentInstallationUpdate, new string[] {installation});
		}
	}

	[Serializable]
	public class UpdateRunningDatabaseChangedEventArgs : UpdateRunningEventArgs
	{
		public UpdateRunningDatabaseChangedEventArgs
			(
			string installation, 
			IgnoreParameters ignoreParameters,
			bool scheduled
			)
			: base (installation, ignoreParameters)
		{
			if (scheduled)
				this.message = new LocalizedString(Events.DbChangesPreventAutoUpdate, null);
			else
				this.message = new LocalizedString(Events.PreDbChangesPreventAppUsage, null);
		}
		public void SetMessage(LocalizedString message)
		{
			this.message = message;
		}
	}
	[Serializable]
	public class RunningImageLockedEventArgs : UpdateRunningEventArgs
	{
		public string[] LoggedUsers;
		public RunningImageLockedEventArgs
			(
			string installation,
			IgnoreParameters ignoreParameters,
			bool scheduled,
			string[] loggedUsers,
			LocalizedString details
			)
			: this(new UpdaterQualifier(installation, string.Empty), ignoreParameters, scheduled, loggedUsers, details) { }
		public RunningImageLockedEventArgs
			(
			UpdaterQualifier updaterQualifier, 
			IgnoreParameters ignoreParameters,
			bool scheduled,
			string[] loggedUsers,
			LocalizedString details
			)
			: base(updaterQualifier, ignoreParameters)
		{
			this.LoggedUsers = loggedUsers;
			this.message = new LocalizedString(Events.RunningStandardLocked, null);
			this.details = details;
		}
		public static LocalizedString GetLoggedUsersDetail(string[] loggedUsers)
		{
			StringBuilder usersText = new StringBuilder();
			foreach (string user in loggedUsers)
				usersText.Append(user + Environment.NewLine);
			return new LocalizedString(Events.LockingUsers, new object[] {Events.RunningImageName, usersText.ToString()});
		}
	}
	[Serializable]
	public class UpdateRunningCompatibilityFailureEventArgs : UpdateRunningEventArgs
	{
		public string[] IncompatibleSlaveProducts;
		public UpdateRunningCompatibilityFailureEventArgs
			(
			string installation, 
			IgnoreParameters ignoreParameters,
			string[] incompatibleSlaveProducts,
			LocalizedString message
			)
			: base (installation, ignoreParameters)
		{
			this.IncompatibleSlaveProducts = incompatibleSlaveProducts;
			this.message = message;
		}
		public void SetMessage(LocalizedString message)
		{
			this.message = message;
		}
	}
	[Serializable]
	public class UpdateRunningOrphansFailureEventArgs : UpdateRunningEventArgs
	{
		public string[] OrphanPaths;
		public UpdateRunningOrphansFailureEventArgs
			(
			string installation, 
			IgnoreParameters ignoreParameters,
			string[] orphanPaths,
			LocalizedString message
			)
			: base (installation, ignoreParameters)
		{
			this.OrphanPaths = orphanPaths;
			this.message = message;
		}
		public void SetMessage(LocalizedString message)
		{
			this.message = message;
		}
	}

	//=========================================================================
	[Serializable]
	public class UpdateRunningSucceededEventArgs : UpdaterEventArgs
	{
		public readonly bool		DatabaseChange		= false;	// indicherà se l'aggiornamento implica cambi al db
		public readonly string[]	NewRequirementsList	= new string[] {};
		public UpdateRunningSucceededEventArgs
			(
			string installation, 
			LocalizedString details,
			bool databaseChange,
			string[] newRequirementsList
			)
			: base
			(
			new UpdaterQualifier(installation, string.Empty), 
			new LocalizedString(Events.UpdateRunningSucceeded, null), 
			details, 
			true
			)
		{
			this.DatabaseChange = databaseChange;
			if (newRequirementsList != null)
				this.NewRequirementsList = newRequirementsList;
		}
	}

	//=========================================================================
	[Serializable]
	public class UpdateDownloadPreventedEventArgs : UpdaterEventArgs
	{
		public UpdateDownloadPreventedEventArgs
			(
			UpdaterQualifier updaterQualifier,
			LocalizedString message,
			LocalizedString details
			)
			: base
			(
			updaterQualifier, 
			message,
			details
			)
		{
			// TODOFEDE - roba specializzata?
		}
	}

	//=========================================================================
	public enum ImageTask {SynchDownload, SynchRunning, RollRunning, RemoveAddOnProduct, Unknown = -1}

}
