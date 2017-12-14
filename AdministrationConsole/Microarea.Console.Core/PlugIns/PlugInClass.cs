using System;
using Microarea.Console.Core.EventBuilder;
using Microarea.TaskBuilderNet.Data.DatabaseLayer;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.Console.Core.PlugIns
{
	[Flags]
	public enum StatusType 
	{ 
		None					= 0, 
		StartUp					= 1, 
		Administration			= 2, 
		RemoteServerError		= 4, 
		SchedulerAgentIsRunning = 8,
		SqlServiceStop			= 32,
        SqlServicePause			= 64,
        SqlServiceRun			= 128,
		SqlServiceUndefined		= 256
	};

	/// <summary>
	/// PlugIn
	/// Classe abstract da cui tutti i PlugIns devono derivare
	/// Implementa un metodo Load, i cui parametri sono gli oggetti di console che ogni
	/// plugIn può modificare, come il tree o la working area, e alcuni delegate comuni
	/// a tutti (Load, UnLoad e modifica del testo visualizzato nella StatusBar)
	/// </summary>
	//=========================================================================
	public abstract class PlugIn
	{
		#region Eventi e Delegati di cui il PlugIn può fare il firing
		public delegate bool	BeforeAddFormHandle (object sender, int width, int height);
		public event			BeforeAddFormHandle	OnBeforeAddFormHandle;       

		//Delegates comuni a tutti i PlugIns
		//---------------------------------------------------------------------
		public delegate void PlugInLoad				(object sender, DynamicEventsArgs e);
		public delegate void PlugInUnLoad			(object sender, DynamicEventsArgs e);
		public delegate void ChangeStatusBar		(object sender, DynamicEventsArgs e);
		public delegate void ChangeConnectionString	(object sender, DynamicEventsArgs e);

		public delegate void EnableProgressBar(object sender);
		public event EnableProgressBar OnEnableProgressBar;
		
		public delegate void DisableProgressBar(object sender);
		public event DisableProgressBar OnDisableProgressBar;
		
		public delegate void SetProgressBarMaxValue(object sender, int max);
		public event SetProgressBarMaxValue OnSetProgressBarMaxValue;
		
		public delegate void SetProgressBarMinValue(object sender, int min);
		public event SetProgressBarMinValue OnSetProgressBarMinValue;
		
		public delegate void SetProgressBarValue(object sender, int currentValue);
		public event SetProgressBarValue OnSetProgressBarValue;

		public delegate void SetProgressBarStep(object sender, int step);
		public event SetProgressBarStep OnSetProgressBarStep;
		
		public delegate void SetProgressBarText(object sender, string message);
		public event SetProgressBarText OnSetProgressBarText;

		public delegate int	GetProgressBarMaxValue(object sender);
		public event GetProgressBarMaxValue OnGetProgressBarMaxValue;
		
		public delegate int GetProgressBarMinValue(object sender);
		public event GetProgressBarMinValue OnGetProgressBarMinValue;
		
		public delegate int GetProgressBarValue(object sender);
		public event GetProgressBarValue OnGetProgressBarValue;
		
		public delegate int GetProgressBarStep (object sender);
		public event GetProgressBarStep OnGetProgressBarStep;
		
		public delegate void SetProgressCyclicStep();
		public event SetProgressCyclicStep OnSetProgressCyclicStep;

		public delegate void PerformStepProgressBar();
		public event PerformStepProgressBar OnPerformStepProgressBar;

		public delegate void SetConsoleTreeViewEnabled(bool enable);
		public event SetConsoleTreeViewEnabled OnSetConsoleTreeViewEnabled;

		public delegate bool IsUserAuthenticated(string login, string password, string serverName);
		public event IsUserAuthenticated OnIsUserAuthenticated;

		public delegate void AddUserAuthenticated(string login, string password, string serverName, DBMSType dbType);
		public event AddUserAuthenticated OnAddUserAuthenticated;

		public delegate string GetUserAuthenticatedPwd(string login, string serverName);
		public event GetUserAuthenticatedPwd OnGetUserAuthenticatedPwd;
		
		public delegate void CallHelpFromPopUp(object sender, string nameSpace, string searchParameter);
		public event CallHelpFromPopUp OnCallHelpFromPopUp;
		#endregion
		
		/// <summary>
		/// Load - Metodo che viene invocato quando il plugIn viene caricato dalla Console 
		/// </summary>
		/// <param name="consoleGUIObjects">Oggetti di console su cui i PlugIns possono intervenire</param>
		/// <param name="consoleEnvironmentInfo">Variabili di ambiente di console</param>
		/// <param name="licenceInformations">Informazioni di licenza </param>
		//---------------------------------------------------------------------
		public abstract void Load
			(
				ConsoleGUIObjects		consoleGUIObjects,
			    ConsoleEnvironmentInfo	consoleEnvironmentInfo,
			    LicenceInfo				licenceInfo
			);

		# region Funzioni per la verifica dell'autenticazione di un utente (viene interrogata la Console)
		/// <summary>
		/// IsUserAuthenticatedFromConsole
		/// Richiede alla Console se l'utente specificato è stato già autenticato
		/// </summary>
		//---------------------------------------------------------------------
		protected bool IsUserAuthenticatedFromConsole(string login, string password, string serverName)
		{
			bool result = false;
			if (OnIsUserAuthenticated != null)
				result = OnIsUserAuthenticated(login, password, serverName);
			return result;
		}

		/// <summary>
		/// AddUserAuthenticatedFromConsole
		/// Aggiunge l'utente specificato alla lista degli utenti autenticati della Console
		/// </summary>
		//---------------------------------------------------------------------
		protected void AddUserAuthenticatedFromConsole(string login, string password, string serverName, DBMSType dbType)
		{
			if (OnAddUserAuthenticated != null)
				OnAddUserAuthenticated(login, password, serverName, dbType);
		}
		
		/// <summary>
		/// GetUserAuthenticatedPwdFromConsole
		/// Richiede alla Console la pwd dell'utente già autenticato
		/// </summary>
		//---------------------------------------------------------------------
		protected string GetUserAuthenticatedPwdFromConsole(string login, string serverName)
		{
			string password = string.Empty;
			if (OnGetUserAuthenticatedPwd != null)
				password = OnGetUserAuthenticatedPwd(login, serverName);
			return password;
		}
		# endregion

		/// <summary>
		/// Evento di ShutDown dalla MicroareaConsole
		/// Fino a che il plugIn non torna true, la console non viene chiusa
		/// Re-implementare questa funzione nel proprio plugIn se è necessario fare qualcosa
		/// </summary>
		//---------------------------------------------------------------------
		public virtual bool ShutDownFromPlugIn()
		{
			return true;
		}

		/// <summary>
		/// OnBeforeAddFormFromPlugIn
		/// </summary>
		//---------------------------------------------------------------------
		protected bool OnBeforeAddFormFromPlugIn(object sender, int width, int height)
		{
			bool result = false;
			if (OnBeforeAddFormHandle != null)
				result = OnBeforeAddFormHandle(sender, width, height);
			return result;
		}

		/// <summary>
		/// SetConsoleTreeViewEnabledFromPlugIn
		/// Funzione da richiamare dai PlugIns per disabilitare il tree della Console durante un'elaborazione
		/// </summary>
		//---------------------------------------------------------------------
		protected void SetConsoleTreeViewEnabledFromPlugIn(bool enable)
		{
			if (OnSetConsoleTreeViewEnabled != null)
				OnSetConsoleTreeViewEnabled(enable);
		}

		#region Eventi per accedere alla ProgressBar (Il PlugIn deve interrogare la Console)

		#region EnableProgressBarFromPlugIn - Abilito la Progress Bar della Console
		/// <summary>
		/// EnableProgressBarFromPlugIn
		/// Funzione da richiamare dai plugIns per abilitare la progressBar
		/// </summary>
		//---------------------------------------------------------------------
		protected void EnableProgressBarFromPlugIn(object sender)
		{
			if (OnEnableProgressBar != null)
				OnEnableProgressBar(sender);
		}
		#endregion

		#region SetProgressBarMaxValueFromPlugIn - Settare il MaxValue della ProgressBar
		/// <summary>
		/// SetProgressBarMaxValueFromPlugIn
		/// Funzione da richiamare dai plugIns per settare il MaxValue della progressBar
		/// </summary>
		//---------------------------------------------------------------------
		protected void SetProgressBarMaxValueFromPlugIn(object sender, int max)
		{
			if (OnSetProgressBarMaxValue != null)
				OnSetProgressBarMaxValue(sender, max);
		}
		#endregion

		#region GetProgressBarMaxValueFromPlugIn - Prendo il MaxValue della ProgressBar
		/// <summary>
		/// GetProgressBarMaxValueFromPlugIn
		/// Leggo il valore Di MaxValue impostato per la ProgressBar
		/// </summary>
		//---------------------------------------------------------------------
		protected int GetProgressBarMaxValueFromPlugIn(object sender)
		{
			if (OnGetProgressBarMaxValue != null)
				return OnGetProgressBarMaxValue(sender);
			else
				return 0;
		}
		#endregion

		#region SetProgressBarMinValueFromPlugIn - Setto il MinValue della ProgressBar
		/// <summary>
		/// SetProgressBarMinValueFromPlugIn
		/// Funzione da richiamare dai plugIns per settare il MinValue della progressBar
		/// </summary>
		//---------------------------------------------------------------------
		protected void SetProgressBarMinValueFromPlugIn(object sender, int min)
		{
			if (OnSetProgressBarMinValue != null)
				OnSetProgressBarMinValue(sender, min);
		}
		#endregion

		#region GetProgressBarMinValueFromPlugIn - Prendo il MinValue della ProgressBar
		/// <summary>
		/// GetProgressBarMinValueFromPlugIn
		/// Leggo il MinValue impostato dalla ProgressBar
		/// </summary>
		//---------------------------------------------------------------------
		protected int GetProgressBarMinValueFromPlugIn(object sender)
		{
			if (OnGetProgressBarMinValue != null)
				return OnGetProgressBarMinValue(sender);
			else
				return 0;
		}
		#endregion

		#region SetProgressBarValueFromPlugIn - Setto il Value della ProgressBar
		/// <summary>
		/// SetProgressBarValueFromPlugIn
		/// Funzione da richiamare dai plugIns per settare il Value della progressBar
		/// </summary>
		//---------------------------------------------------------------------
		protected void SetProgressBarValueFromPlugIn(object sender, int currentValue)
		{
			if (OnSetProgressBarValue != null)
				OnSetProgressBarValue(sender, currentValue);
		}
		#endregion

		#region GetProgressBarValueFromPlugIn - Prendo il Value della ProgressBar
		/// <summary>
		/// GetProgressBarValueFromPlugIn
		/// Leggo il Value impostato sulla ProgressBar
		/// </summary>
		//---------------------------------------------------------------------
		protected int GetProgressBarValueFromPlugIn(object sender)
		{
			if (OnGetProgressBarValue != null)
				return OnGetProgressBarValue(sender);
			else
				return 0;
		}
		#endregion

		#region SetProgressBarStepFromPlugIn - Setto lo Step della ProgressBar
		/// <summary>
		/// SetProgressBarStepFromPlugIn
		/// Funzione da richiamare dai plugIns per settare lo Step della progressBar
		/// </summary>
		//---------------------------------------------------------------------
		protected void SetProgressBarStepFromPlugIn(object sender, int step)
		{
			if (OnSetProgressBarStep != null)
				OnSetProgressBarStep(sender, step);
		}
		#endregion

		#region GetProgressBarStepFromPlugIn - Prendo lo Step della ProgressBar
		/// <summary>
		/// GetProgressBarStepFromPlugIn
		/// Leggo lo step impostato sulla progressBar
		/// </summary>
		//---------------------------------------------------------------------
		protected int GetProgressBarStepFromPlugIn(object sender)
		{
			if (OnGetProgressBarStep != null)
				return OnGetProgressBarStep(sender);
			else
				return 0;
		}
		#endregion

		#region SetProgressBarText - Setto il Text accanto alla ProgressBar
		/// <summary>
		/// SetProgressBarText
		/// Funzione da richiamare dai plugIns per settare la stringa di messaggio della progressBar
		/// </summary>
		//---------------------------------------------------------------------
		protected void SetProgressBarTextFromPlugIn(object sender, string message)
		{
			if (OnSetProgressBarText != null)
				OnSetProgressBarText(sender, message);
		}
		#endregion

		#region SetCyclicStepProgressBarFromPlugIn - Setto l'incremento ciclico la ProgressBar
		/// <summary>
		/// SetCyclicStepProgressBarFromPlugIn
		/// Funzione da richiamare dai PlugIns per disabilitare la progressBar
		/// </summary>
		//---------------------------------------------------------------------
		protected void SetCyclicStepProgressBarFromPlugIn()
		{
			if (OnSetProgressCyclicStep != null)
				OnSetProgressCyclicStep();
		}
		#endregion

		#region PerformStepProgressBarFromPlugIn - Aggiungo uno step nella progressbar
		/// <summary>
		/// PerformStepProgressBarFromPlugIn
		/// Funzione da richiamare dai PlugIns per aggiungere uno step alla progressBar
		/// </summary>
		//---------------------------------------------------------------------
		protected void PerformStepProgressBarFromPlugIn()
		{
			if (OnPerformStepProgressBar != null)
				OnPerformStepProgressBar();
		}
		#endregion

		#region DisableProgressBarFromPlugIn - Disabilito la ProgressBar
		/// <summary>
		/// DisableProgressBarFromPlugIn
		/// Funzione da richiamare dai PlugIns per disabilitare la progressBar
		/// </summary>
		//---------------------------------------------------------------------
		protected void DisableProgressBarFromPlugIn(object sender)
		{
			if (OnDisableProgressBar != null)
				OnDisableProgressBar(sender);
		}
		#endregion

		#endregion

		/// <summary>
		/// CallHelpPopUp
		/// Chiama l'help di console da una finestra popUp del plugIn
		/// </summary>
		//---------------------------------------------------------------------
		protected void HelpFromPopUp(object sender, string nameSpace, string searchParameter)
		{
			if (OnCallHelpFromPopUp != null)
				OnCallHelpFromPopUp(sender, nameSpace, searchParameter);
		}
	}
}