using System;
using System.Deployment.Application;
using System.Windows.Forms;

namespace Microarea.TaskBuilderNet.Core.Generic
{
	//================================================================================
	/// <summary>
	/// Classe che gestisce lo scaricamento on demand delle dll deployate con Click Once
	/// </summary>
	/// <remarks>
	/// Ha un metodo statico DownloadGroup che controlla se tutte le dll appartenenti ad un gruppo
	/// di un nome dato sono aggiornate e, in caso contrario, le scarica sul client aprendo una form
	/// con barra di progresso che si chiude automaticamente al termine del download, salvo errori nel qual caso rimane 
	/// aperta per segnalare l'errore all'utente
	/// esempio di utilizzo:
	/// ClickOnceDeploy.DownloadGroup("it-it"); 
	/// (scarica i dizionari italiani)
	/// </remarks>
	public partial class ClickOnceDeploy : Form
	{
		/// <summary>
		/// semaforo per sapere quando il download è terminato
		/// </summary>
		private bool downloadCompleted = false;
		/// <summary>
		/// indica che si e' verificato un errore di download
		/// </summary>
		private bool downloadError = false;
		/// <summary>
		/// nome del gruppo di componenti da scaricare
		/// </summary>
		private string groupName = null;
		/// <summary>
		/// descrizione del gruppo di componenti da scaricare
		/// </summary>
		private string groupTitle;

		/// <summary>
		/// Mi dice se sono un client clickonce (la metto in cache per motivi di performance)
		/// </summary>
		public static readonly bool IsClickOnceClient = ApplicationDeployment.IsNetworkDeployed;

		//--------------------------------------------------------------------------------
		/// <summary>
		/// Indica se il download è terminato
		/// </summary>
		private bool DownloadCompleted
		{
			get { return downloadCompleted; }
		}

		//--------------------------------------------------------------------------------
		/// <summary>
		/// Indica se il download è terminato con errori
		/// </summary>
		private bool DownloadError
		{
			get { return downloadError; }
		}

		//--------------------------------------------------------------------------------
		/// <summary>
		/// Costruisce l'oggetto
		/// </summary>
		/// <param name="groupName">Nome del gruppo di componenti da scaricare</param>
		/// <param name="groupTitle">Descrizione del gruppo di componenti da scaricare</param>
		private ClickOnceDeploy (string groupName, string groupTitle)
		{
			this.groupName = groupName;
			this.groupTitle = groupTitle;

			InitializeComponent();
		}

		//--------------------------------------------------------------------------------
		/// <summary>
		/// Effettua l'aggiornamento, se necessario, delle componenti appartenenti al gruppo dato
		/// </summary>
		/// <param name="groupName">Nome del gruppo di componenti da aggiornare</param>
		/// <param name="groupTitle">Descrizione del gruppo di componenti</param>
		public static void DownloadGroup (string groupName, string groupTitle)
		{
			if (IsClickOnceClient)
			{
				ApplicationDeployment deploy = ApplicationDeployment.CurrentDeployment;

				bool downloadNeeded = false;
				try
				{
					downloadNeeded = !deploy.IsFileGroupDownloaded(groupName);
				}
				catch (InvalidDeploymentException) //il gruppo potrebbe non esistere (es culture inesistente)
				{
					downloadNeeded = false; //se il gruppo non esiste, non devo fare download
				}

				//se il gruppo risulta da scaricare, avvio il download
				if (downloadNeeded) 
				{
					ClickOnceDeploy f = new ClickOnceDeploy(groupName, groupTitle);
					//visualizzo la form che, una volta caricata, iniziera` il download dando feedback del progresso
					f.Show();
					//finche' il download non e' completo, processo i messaggi di Windows
					while (!f.DownloadCompleted)
						Application.DoEvents();

					//chiudo la finestra al termine del download, se non ho avuto errori
					if (!f.DownloadError)
						f.Close();
				}
			}
		}

		//--------------------------------------------------------------------------------
		public static bool GetServerInstallationInfo (out string server, out string installation)
		{
			server = installation = string.Empty;

			//se sono un client clickonce...
			if (IsClickOnceClient)
			{
				ApplicationDeployment deploy = ApplicationDeployment.CurrentDeployment;

				server = deploy.UpdateLocation.Host;
				string path = deploy.UpdateLocation.AbsolutePath.Trim('/');
				//il primo token del path e' il nome dell'installazione
				installation = path.Substring(0, path.IndexOf("/"));

				return true;
			}

			return false;
		}
		//--------------------------------------------------------------------------------
		protected override void OnLoad (EventArgs e)
		{
			base.OnLoad(e);

			Text = string.Format("Updating {0}", Application.ProductName);
			labelDescription.Text = string.Format(GenericStrings.DownloadingComponents, groupTitle);
			ApplicationDeployment deploy = ApplicationDeployment.CurrentDeployment;

			//aggancio gli eventi di download
			deploy.DownloadFileGroupProgressChanged += new DeploymentProgressChangedEventHandler(DownloadFileGroupProgressChanged);
			deploy.DownloadFileGroupCompleted += new DownloadFileGroupCompletedEventHandler(DownloadFileGroupCompleted);

			//avvio il download asincrono
			deploy.DownloadFileGroupAsync(groupName);
		}

		//--------------------------------------------------------------------------------
		/// <summary>
		/// </summary>
		protected override void OnFormClosing (FormClosingEventArgs e)
		{
			//se non ho finito il download non posso uscire
			e.Cancel = !DownloadCompleted;
			base.OnFormClosing(e);
		}

		//--------------------------------------------------------------------------------
		private void DownloadFileGroupCompleted (object sender, DownloadFileGroupCompletedEventArgs e)
		{
			try
			{
				//serializzo la chiamata sul thread di finestra
				BeginInvoke(new FinishedDelegate(SetFinished), new object[] { e });
			}
			catch //potrebbe lanciare un'eccezione per handle non ancora valido, ignorare
			{
				
			}
		}

		//--------------------------------------------------------------------------------
		private void DownloadFileGroupProgressChanged (object sender, DeploymentProgressChangedEventArgs e)
		{
			try
			{
				//serializzo la chiamata sul thread di finestra
				BeginInvoke(new ProgressDelegate(SetProgress), new object[] { e });
			}
			catch //potrebbe lanciare un'eccezione per handle non ancora valido, ignorare
			{
				
			}
		}

		private delegate void ProgressDelegate (DeploymentProgressChangedEventArgs e);
		//--------------------------------------------------------------------------------
		/// <summary>
		/// Funzione chiamata in fase di aggiornamento del download, rinfresca il feedback all'utente
		/// </summary>
		/// <param name="e"></param>
		private void SetProgress (DeploymentProgressChangedEventArgs e)
		{
			try
			{
				//aggiorno i dati di download
				downloadProgress.Value = e.ProgressPercentage;
				messageLabel.Text = string.Format(GenericStrings.DownloadProgress, e.BytesCompleted.ToString("N0"), e.BytesTotal.ToString("N0"));

			}
			catch //potrebbe lanciare un'eccezione per handle non ancora valido, ignorare
			{
				
			}
		}

		private delegate void FinishedDelegate (DownloadFileGroupCompletedEventArgs e);
		//--------------------------------------------------------------------------------
		/// <summary>
		/// Funzione chiamata quando il download è terminato, rinfresca il feedback all'utente
		/// </summary>
		/// <param name="e"></param>
		private void SetFinished (DownloadFileGroupCompletedEventArgs e)
		{
			try
			{
				//imposto i dati di fine download
				downloadError = e.Error != null;
				messageLabel.Text = downloadError
					? e.Error.Message
					: GenericStrings.DownloadSuccess;

				//libero il thread in attesa del download
				downloadCompleted = true;
			}
			catch //potrebbe lanciare un'eccezione per handle non ancora valido, ignorare
			{
				
			}
		}
	}
}
