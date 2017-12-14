using System;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using Microarea.TaskBuilderNet.Core.Applications;
using Microarea.TaskBuilderNet.UI.EasyLookCustomization;

namespace Microarea.Web.EasyLook
{
	/// <summary>
	/// Summary description for LogOut.
	/// </summary>
	//================================================================================
	public partial class LogOut : System.Web.UI.Page
	{

		/// <summary>
		/// Evento che fa il Load della Pagina effettua il LogOff da LoginManager
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		//--------------------------------------------------------------------------------
		protected void Page_Load (object sender, System.EventArgs e)
		{
			//in chiusura della finestra di logoff viene mandato un AutoDisconnect
			if (Request.Params["AutoDisconnect"] == "true")
			{
				//se pero' la chiusura era stata provocata dal bottone KeepAlive, allora 
				//in httpSession mi sono segnato che non devo disconnettermi
				if (Session["KeepAlive"] as string != "true")
					Disconnect();

				RegisterCloseWindowScript();
				return;
			}

			UserInfo ui = UserInfo.FromSession();
			if ((ui == null) || (ui.LoginManager == null))
			{
				RegisterCloseWindowScript();
				return;
			}

			//se l'utente sta usando una cal concurrent o un cal Easylook solo report non devo chiedergli se vuole mantenere 
			//la sessione e lo devo disconnettere
			bool isFloatingUser, isWebUser;
			ui.LoginManager.IsFloatingUser(ui.User,out isFloatingUser);
			ui.LoginManager.IsWebUser(ui.User,out isWebUser);
			
			if (isFloatingUser || isWebUser)
			{
				Disconnect();
				RegisterCloseWindowScript();
			}

			ui.SetCulture();
			buttonKeepAlive.Focus();
			SetLocalizedLabels(ui);
			
		}

		//---------------------------------------------------------------------
		private void SetLocalizedLabels (UserInfo ui)
		{
			EasyLookCustomSettings easyLookCustomSettings = (EasyLookCustomSettings)Session[EasyLookCustomSettings.SessionKey];

			if (easyLookCustomSettings == null)
				return;

			if (easyLookCustomSettings.FontFamily != string.Empty || easyLookCustomSettings.FontFamily != "")
			{
				FirstLabel.Font.Name = easyLookCustomSettings.FontFamily;
				FirstLabel.Font.Size = FontUnit.Medium;
				SecondLabel.Font.Name = easyLookCustomSettings.FontFamily;
				SecondLabel.Font.Size = FontUnit.Medium;
				buttonDisconnect.Style.Add("FONT-FAMILY", easyLookCustomSettings.FontFamily);
				buttonKeepAlive.Style.Add("FONT-FAMILY", easyLookCustomSettings.FontFamily);
			}

			FirstLabel.Text = string.Format(LabelStrings.Thanks, ui.User);
			SecondLabel.Text = LabelStrings.ClosingMessage;
			buttonDisconnect.Text = LabelStrings.Close;
			buttonKeepAlive.Text = LabelStrings.KeepAlive;

		}


		#region Web Form Designer generated code
		/// <summary>
		/// Questa chiamata è richiesta da Progettazione Web Form ASP.NET.
		/// </summary>
		/// <param name="e"></param>
		//--------------------------------------------------------------------------------
		override protected void OnInit (EventArgs e)
		{
			InitializeComponent();
			base.OnInit(e);
		}

		/// <summary>
		/// Metodo necessario per il supporto della finestra di progettazione. Non modificare
		/// il contenuto del metodo con l'editor di codice.
		/// </summary>
		//--------------------------------------------------------------------------------
		private void InitializeComponent ()
		{

		}
		#endregion

		//--------------------------------------------------------------------------------
		protected void OnDisconnect (object sender, EventArgs e)
		{
			Disconnect();
			RegisterCloseWindowScript();
		}

		//--------------------------------------------------------------------------------
		private void Disconnect ()
		{
			UserInfo ui = UserInfo.FromSession();
			if (ui != null)
				ui.LogOff();
			FormsAuthentication.SignOut();
		}

		//--------------------------------------------------------------------------------
		protected void OnKeepAlive (object sender, EventArgs e)
		{
			Session["KeepAlive"] = "true";
			RegisterCloseWindowScript();
		}

		//--------------------------------------------------------------------------------
		private void RegisterCloseWindowScript ()
		{
			ScriptManager.RegisterStartupScript(this, GetType(), "Close", "window.close();", true);
		}
	}
}
