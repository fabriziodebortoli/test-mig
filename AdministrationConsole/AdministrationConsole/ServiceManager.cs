using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.ServiceProcess;
using System.Windows.Forms;
using Microarea.Console.Core.PlugIns;
using Microarea.TaskBuilderNet.Core.DiagnosticManager;
using System.Collections.Generic;

namespace Microarea.Console
{
	/// <summary>
	/// ServiceManager
	/// Gestione Servizi Database (Start/Stop/Pause dei servizi)
	/// </summary>
	// ========================================================================
	public partial class ServiceManager : System.Windows.Forms.Form
	{
		public event EventHandler OnChangedState;

		static	string NameSpaceConsoleIcon	= "Microarea.Console.Icons.";
		private List<DatabaseService> serviceDatabase = new List<DatabaseService>();
		private Diagnostic diagnostic = new Diagnostic("ServiceManager");
		private System.ComponentModel.Container components = null;

		# region Constructor
		/// <summary>
		/// ServiceManager (costruttore)
		/// </summary>
		//---------------------------------------------------------------------
		public ServiceManager(List<DatabaseService> servicesDatabase)
		{
			InitializeComponent();
			this.serviceDatabase = servicesDatabase;
		}
		# endregion

		# region Load and Closing form
		/// <summary>
		/// ServiceManager_Closing
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		//---------------------------------------------------------------------
		private void ServiceManager_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if (sender == this)
			{
				e.Cancel = true;
				this.Hide();
			}
		}

		//---------------------------------------------------------------------
		private void ServiceManager_Load(object sender, System.EventArgs e)
		{
			LoadDatabaseServices();
		}
		# endregion

		# region LoadDatabaseServices
		/// <summary>
		/// LoadDatabaseServices
		/// </summary>
		//---------------------------------------------------------------------
		private void LoadDatabaseServices()
		{
			foreach (DatabaseService current in serviceDatabase)
			{
				serviceCombo.Items.Add(current.ServiceDisplayName);
				serverCombo.Items.Add(current.ComputerName);
				serviceCombo.SelectedIndex = serverCombo.SelectedIndex = 0;
				
				switch (current.ServiceStatus)
				{
					case StatusType.SqlServiceRun:
						ServiceIsRunning(current.ServiceController);
						break;

					case StatusType.SqlServiceStop:
						ServiceIsStopped(current.ServiceController);
						break;

					case StatusType.SqlServicePause:
						ServiceIsPaused(current.ServiceController);
						break;
				
					default:
						ServiceIsUndefined(current.ServiceController);
						break;
				}
			}
		}
		# endregion

		# region Set images for Service State
		//---------------------------------------------------------------------
		private void ServiceIsRunning(ServiceController service)
		{
			if (service == null)
				return;

			BtnStart.Enabled			= false;
			BtnStop.Enabled				= true;
			BtnPause.Enabled			= true;
			Stream iconStream			= Assembly.GetExecutingAssembly().GetManifestResourceStream(NameSpaceConsoleIcon + "SqlServiceRunLarge.ico");
			statePicture.SizeMode		= PictureBoxSizeMode.StretchImage;
			statePicture.Image			= Image.FromStream(iconStream);
			statusBar.Panels[0].Text	= String.Format("{0} - {1} - {2}", Strings.Running, service.MachineName.ToUpper(CultureInfo.InvariantCulture), service.ServiceName);
		}

		//---------------------------------------------------------------------
		private void ServiceIsPaused(ServiceController service)
		{
			if (service == null)
				return;

			BtnStart.Enabled			= true;
			BtnStop.Enabled				= true;
			BtnPause.Enabled			= false;
			Stream iconStream			= Assembly.GetExecutingAssembly().GetManifestResourceStream(NameSpaceConsoleIcon + "SqlServicePauseLarge.ico");
			statePicture.SizeMode		= PictureBoxSizeMode.StretchImage;
			statePicture.Image			= Image.FromStream(iconStream);
			statusBar.Panels[0].Text	= String.Format("{0} - {1} - {2}", Strings.Paused, service.MachineName.ToUpper(CultureInfo.InvariantCulture), service.ServiceName);
		}

		//---------------------------------------------------------------------
		private void ServiceIsStopped(ServiceController service)
		{
			if (service == null)
				return;
			
			BtnStart.Enabled			= true;
			BtnStop.Enabled				= false;
			BtnPause.Enabled			= true;
			Stream iconStream			= Assembly.GetExecutingAssembly().GetManifestResourceStream(NameSpaceConsoleIcon + "SqlServiceStopLarge.ico");
			statePicture.SizeMode		= PictureBoxSizeMode.StretchImage;
			statePicture.Image			= Image.FromStream(iconStream);
			statusBar.Panels[0].Text	= String.Format("{0} - {1} - {2}", Strings.Stopped, service.MachineName.ToUpper(CultureInfo.InvariantCulture), service.ServiceName);
		}

		//---------------------------------------------------------------------
		private void ServiceIsUndefined(ServiceController service)
		{
			if (service == null)
				return;

			BtnStart.Enabled			= true;
			BtnStop.Enabled				= false;
			BtnPause.Enabled			= true;
			Stream iconStream			= Assembly.GetExecutingAssembly().GetManifestResourceStream(NameSpaceConsoleIcon + "SqlServiceUndefinedLarge.ico");
			statePicture.SizeMode		= PictureBoxSizeMode.StretchImage;
			statePicture.Image			= Image.FromStream(iconStream);
			statusBar.Panels[0].Text	= String.Format("{0} - {1} - {2}", Strings.UndefinedService, service.MachineName.ToUpper(CultureInfo.InvariantCulture), service.ServiceName);
		}
		# endregion

		# region Start Service
		//---------------------------------------------------------------------
		private void BtnStart_Click(object sender, System.EventArgs e)
		{
			string serviceName			= serviceCombo.SelectedItem.ToString();
			string serverCompleteName	= serverCombo.SelectedItem.ToString();
			string serverName			= String.Empty;
			
			if (serverCompleteName.Split(Path.DirectorySeparatorChar).Length > 1)
			{
				string[] temp = serverCompleteName.Split(Path.DirectorySeparatorChar);
				serverName	  = temp[0];
			}
			else
				serverName = serverCompleteName;
			
			if (SyntaxCheck.CheckMachineName(serverName))
			{
				ServiceController[] services = ServiceController.GetServices(serverName);
				foreach (ServiceController service in services)
				{
					if (String.Compare(service.DisplayName, serviceName, true, CultureInfo.InvariantCulture) == 0)
					{
						if (service.Status == ServiceControllerStatus.Stopped)
							service.Start();
						else if (service.Status == ServiceControllerStatus.Paused)
							service.Continue();
						service.WaitForStatus(ServiceControllerStatus.Running);
						ServiceIsRunning(service);
						ChangeState(service);
						break;
					}
				}
			}
		}
		# endregion

		# region Pause Service
		//---------------------------------------------------------------------
		private void BtnPause_Click(object sender, System.EventArgs e)
		{
			string serviceName			= serviceCombo.SelectedItem.ToString();
			string serverCompleteName	= serverCombo.SelectedItem.ToString();
			string serverName			= String.Empty;
			
			if (serverCompleteName.Split(Path.DirectorySeparatorChar).Length > 1)
			{
				string[] temp	= serverCompleteName.Split(Path.DirectorySeparatorChar);
				serverName		= temp[0];
			}
			else
				serverName = serverCompleteName;
			
			if (SyntaxCheck.CheckMachineName(serverName))
			{
				ServiceController[] services = ServiceController.GetServices(serverName);
				foreach (ServiceController service in services)
				{
					if (String.Compare(service.DisplayName, serviceName, true, CultureInfo.InvariantCulture) == 0)
					{
						if (service.Status == ServiceControllerStatus.Running)
							service.Pause();
						service.WaitForStatus(ServiceControllerStatus.Paused);
						ServiceIsPaused(service);
						ChangeState(service);
						break;
					}
				}
			}
		}
		# endregion

		# region Stop Service
		//---------------------------------------------------------------------
		private void BtnStop_Click(object sender, System.EventArgs e)
		{
			string serviceName			= serviceCombo.SelectedItem.ToString();
			string serverCompleteName	= serverCombo.SelectedItem.ToString();
			string serverName			= String.Empty;
			
			if (serverCompleteName.Split(Path.DirectorySeparatorChar).Length > 1)
			{
				string[] temp	= serverCompleteName.Split(Path.DirectorySeparatorChar);
				serverName		= temp[0];
			}
			else
				serverName = serverCompleteName;
			
			if (SyntaxCheck.CheckMachineName(serverName))
			{
				ServiceController[] services = ServiceController.GetServices(serverName);
				foreach (ServiceController service in services)
				{
					if (String.Compare(service.DisplayName, serviceName, true, CultureInfo.InvariantCulture) == 0)
					{
						if ((service.Status == ServiceControllerStatus.Running) || (service.Status == ServiceControllerStatus.Paused))
							service.Stop();
						service.WaitForStatus(ServiceControllerStatus.Stopped);
						ServiceIsStopped(service);
						ChangeState(service);
						break;
					}
				}
			}
		}
		# endregion

		# region Event to change service controller state
		//---------------------------------------------------------------------
		private void ChangeState(ServiceController serviceChanged)
		{
			if (OnChangedState != null)
				OnChangedState(serviceChanged, new EventArgs());
		}
		# endregion
	}
}
