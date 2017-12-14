using System;
using System.IO;
using System.Web.Security;
using Microarea.TaskBuilderNet.Core.Applications;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.Library.TBApplicationWrapper;
using Microarea.TaskBuilderNet.Core.NameSolver;
using System.Web;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.Web.EasyLook 
{
	/// <summary>
	/// Summary description for Global.
	/// </summary>
	public class Global : System.Web.HttpApplication
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;
		
		public Global()
		{
			InitializeComponent();
		}	
		
		protected void Application_Start(Object sender, EventArgs e)
		{
            // Su un 2003 con .NET Framework 4.0x sembrerebbe che fare questa in questo momento sia prematuro e l'HttpContext
            // (vedi ImagesHelper.cs) non sia ancora a posto per usare la MapPath - Germano & Silvano
            //CleanTempImages();
		}
 
		protected void Session_Start(Object sender, EventArgs e)
		{
			try
			{
				InstallationInfo.TestInstallation();
			}
			catch (Exception ex)
			{
				Helper.RedirectToErrorPage(ex);
			}
		}

		protected void Application_Error(Object sender, EventArgs e)
		{
		    Helper.RedirectToErrorPageIfPossible();
		}

		// Bisogna fare un logoff per informare il Loginmanager che la sessione è morta
		// per non lasciare appeso il conteggio delle CAL
		protected void Session_End(Object sender, EventArgs e)
		{
			UserInfo ui = UserInfo.FromSession();
			if (ui != null)
			{
				if (ui.Valid)
					Application.RemoveUserInfoFromApplication(ui);

				//Se l'utente non ha volutamente mantenuto la sessione aperta (decidendolo dalla finestra di logout)
				//faccio il logoff (An. 18073)
				if (Session["KeepAlive"] as string != "true")
				{
					ui.LogOff();
				}
			}
		}

		protected void Application_End(Object sender, EventArgs e)
		{
			CleanTempImages();
 		}

		private void CleanTempImages ()
		{
			try
			{
				string imagesPath = ImagesHelper.TempImagesPath;
				
				//cancello la cartella per eliminarne il contenuto
				if (Directory.Exists(imagesPath))
					Directory.Delete(imagesPath, true);
				
				Directory.CreateDirectory(imagesPath);
			}
			catch 
			{
				//does nothing
			}
		}
			
		#region Web Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{    
			this.components = new System.ComponentModel.Container();
		}
		#endregion
	}
}

