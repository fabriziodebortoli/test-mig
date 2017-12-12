using System;
using System.Collections;
using System.ComponentModel;
using System.Configuration.Install;
using System.IO;
using System.Net;
using Microarea.TaskBuilderNet.Core.Generic;


namespace Microarea.Web.EasyLook
{
	//================================================================================
	/// <summary>
	/// Classe per inizializzare il file di configurazione per il path finder di Easylook
	/// </summary>
	[RunInstaller(true)]
	public partial class EasyLookInstaller : Installer
	{
		//--------------------------------------------------------------------------------
		public EasyLookInstaller ()
		{
			InitializeComponent();
		}

		//--------------------------------------------------------------------------------
		public override void Install (IDictionary stateSaver)
		{
            //attivare per entrare in debug dal EasyLook Standalone Setup
            string installation = Context.Parameters["installation"];
			string fileServer = Context.Parameters["fileServer"];
			string webServer = Context.Parameters["webServer"];

			CheckServer(fileServer);
			if (webServer != fileServer)
				CheckServer(webServer);
			InstallationInfo info = new InstallationInfo(webServer, fileServer, installation);
			info.Save();

			try
			{
				InstallationInfo.TestInstallation();
			}
			catch
			{
				info.Delete();
				throw;
			}
			base.Install(stateSaver);
		}

		private static void CheckServer (string fileServer)
		{
			try
			{
				if (Dns.GetHostEntry(fileServer) == null)
					throw new ApplicationException(string.Format(LabelStrings.InvalidServer, fileServer));
			}
			catch
			{
				throw new ApplicationException(string.Format(LabelStrings.InvalidServer, fileServer));
			}
		}

		//--------------------------------------------------------------------------------
		protected override void OnAfterUninstall (IDictionary savedState)
		{
			string file = InstallationInfo.FilePath;
			if (File.Exists(file))
				File.Delete(file);

			base.OnAfterUninstall(savedState);
		}
	}
}
