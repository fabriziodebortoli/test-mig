using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

using Microarea.Console.Core.PlugIns;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Core.WebServicesWrapper;
using Microarea.TaskBuilderNet.Data.DatabaseLayer;
using Microarea.TaskBuilderNet.Interfaces;
using Microarea.TaskBuilderNet.Licence.Licence.Forms;

namespace Microarea.Console
{
	/// <summary>
	/// AboutBox
	/// Info sull'AdministrationConsole
	/// </summary>
	// ========================================================================
	public partial class AboutBox : System.Windows.Forms.Form
	{
		private LoginManager loginManager;
		
		/// <summary>
		/// Costruttore (con parametri)
		/// </summary>
		//---------------------------------------------------------------------
		public AboutBox(bool runningFromServer, LicenceInfo licenceInfo)
		{
			InitializeComponent();

			LblServerName.Text = runningFromServer ? Strings.LblServer : Strings.LblClient;
			txtMachine.Text = System.Net.Dns.GetHostName().ToUpper(CultureInfo.InvariantCulture);
			
			//Stream bitmapStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Microarea.Console.Images.AboutBox.gif");
			//if (bitmapStream != null)
			//{
			//	System.Drawing.Bitmap bitmap = new Bitmap(bitmapStream);
			//	if (bitmap != null)
			//	{
			//		bitmap.MakeTransparent(Color.Magenta);
			//		LogoPictureBox.Image = bitmap;
			//	}
			//}

			IBaseApplicationInfo baseApp = BasePathFinder.BasePathFinderInstance.GetApplicationInfoByName(DatabaseLayerConsts.MicroareaConsole);// signature as in Application.config
			txtEdition.Text	= string.IsNullOrEmpty(licenceInfo.EditionTextualType) ? Strings.Undefined : licenceInfo.EditionTextualType;

			// visualizzo i DBMS supportati
			if (licenceInfo.DBNetworkType == DBNetworkType.Undefined)
				TxtDBMS.Text = Strings.Undefined;
			//6176 
			else
				TxtDBMS.Text =
					ConstStrings.SQLServer2000Product	+ "\r\n" +
					ConstStrings.SQLServer2005Product	+ "\r\n" +
					ConstStrings.SQLServer2008Product	+ "\r\n" +
					ConstStrings.SQLServer2008R2Product + "\r\n" +
					ConstStrings.SQLServer2012Product	+ "\r\n" +
					ConstStrings.SQLServer2014Product	+ "\r\n" +
					ConstStrings.SQLServer2016Product	+ "\r\n" +
					ConstStrings.MSDE2000Product		+ "\r\n" +
					ConstStrings.ExpressEd2005Product	+ "\r\n" +
					ConstStrings.ExpressEd2008Product	+ "\r\n" +
					ConstStrings.ExpressEd2008R2Product + "\r\n" +
					ConstStrings.ExpressEd2012Product	+ "\r\n" +
					ConstStrings.ExpressEd2014Product	+ "\r\n" +
					ConstStrings.ExpressEd2016Product	+ "\r\n" +
					ConstStrings.Oracle10gProduct		+ "\r\n" +
					ConstStrings.Oracle11gProduct		+ "\r\n" +
					ConstStrings.Oracle12cProduct;

			txtIso.Text = string.IsNullOrEmpty(licenceInfo.IsoState) ? Strings.Undefined : licenceInfo.IsoState; 

			loginManager = new LoginManager();

			LblVersion.Text = String.Format(Strings.LblVersions, loginManager.GetInstallationVersion());

			string regMessage = string.Empty;
			ActivationState state;
			loginManager.IsRegisteredTrapped(out regMessage, out state);
			txtRegistrationStatus.Text = regMessage;		
		}
		
		//---------------------------------------------------------------------
		private void BtnOK_Click(object sender, EventArgs e)
		{
			this.Close();
		}

		//---------------------------------------------------------------------
		private void LnkProxy_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			SetProxySetting(ProxyFirewallManager.Show(GetProxySetting()));
		}

		//---------------------------------------------------------------------
		private ProxySettings GetProxySetting()
		{
			return loginManager.GetProxySettings();
		}

		//---------------------------------------------------------------------
		private void SetProxySetting(ProxySettings proxySettings)
		{
			loginManager.SetProxySettings(proxySettings);
		}		
	}
}