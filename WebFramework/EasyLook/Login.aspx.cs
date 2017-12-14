using System;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using Microarea.TaskBuilderNet.Core.Applications;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Core.WebServicesWrapper;
using Microarea.TaskBuilderNet.Interfaces;
using Microarea.TaskBuilderNet.UI.EasyLookCustomization;
using Microarea.TaskBuilderNet.Data.DatabaseLayer;
using System.Data;
using System.Net;
using System.IO;


namespace Microarea.Web.EasyLook
{
	/// <summary>
	/// Summary description for Login.
	/// </summary>
	//================================================================================
	public partial class Login : System.Web.UI.Page
	{

		/// <summary>
		/// Immagine contenente l'icona della login
		/// </summary>
		protected System.Web.UI.WebControls.Image Image1;
		/// <summary>
		/// Label contenente la stringa localizzata "Utente"
		/// </summary>
		protected System.Web.UI.WebControls.Label UserLabel;
		/// <summary>
		/// Casella di testo nella quale verrà inserito l'Utente
		/// </summary>
		protected System.Web.UI.WebControls.TextBox UserTextBox;
		/// <summary>
		/// Label contenente la stringa localizzata "Password"
		/// </summary>
		protected System.Web.UI.WebControls.Label PasswordLabel;
		/// <summary>
		/// Casella di testo nella quale verrà inserita la Password
		/// </summary>
		protected System.Web.UI.WebControls.TextBox PasswordTextBox;
		/// <summary>
		/// ComboBox che conterrà l'insieme delle company associate all'Utente
		/// </summary>
		protected System.Web.UI.WebControls.DropDownList CompanyComboBox;
		/// <summary>
		/// Bottone di "Ok"
		/// </summary>
		protected System.Web.UI.WebControls.Button OkButton;
		/// <summary>
		/// Label contenente la stringa localizzata "Aziende"
		/// </summary>
		protected System.Web.UI.WebControls.Label CompanyLabel;
		/// <summary>
		/// Label contenete il titolo della pagina
		/// </summary>
		protected System.Web.UI.WebControls.Label TitleLabel;
		/// <summary>
		/// Pannello di background che contiene i controlli per inserire user e password
		/// </summary>
		protected System.Web.UI.WebControls.Panel BkgndPanel;
		/// <summary>
		/// Label che conterrà una breve spiegazione localizzata della pagina
		/// </summary>
		protected System.Web.UI.WebControls.Label TitleLabel1;
		/// <summary>
		/// Label posizionata sotto il Panel contenente i browser compatibili
		/// </summary>
		protected System.Web.UI.WebControls.Label BottomLabel;
		/// <summary>
		/// Label dedita alla visualizzazione dei messaggi di errore
		/// </summary>
		protected System.Web.UI.WebControls.Label MsgLabel;



		//--------------------------------------------------------------------------
		/// <summary>
		/// Evento di Load della pagina.
		/// Setta la CaChe a NoCaChe e la culture leggendola dal ServerConnectionConfig
		/// visto che non si è ancora autenticato nessuno 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void Page_Load (object sender, System.EventArgs e)
		{

			HttpContext.Current.Response.Cache.SetCacheability(HttpCacheability.NoCache);

			if (!this.IsPostBack)
			{
				DictionaryFunctions.SetCultureInfo
					(
					 InstallationData.ServerConnectionInfo.PreferredLanguage,
					 InstallationData.ServerConnectionInfo.ApplicationLanguage
					);

				LoginManager log = new LoginManager();

				if (!log.IsActivated(Strings.WebFramework, Strings.EasyLookFull))
					NotLicensedLayout();

				if (log.IsDeveloperActivation())
				{
					Form1.Style.Add("BACKGROUND-IMAGE", BuildUrl("DevelopmentBkgnd.jpg"));
				}

				if (log.IsDemo())
				{
					Form1.Style.Add("BACKGROUND-IMAGE", BuildUrl("DemoBkgnd.jpg"));
				}

				if (log.IsReseller())
				{
					Form1.Style.Add("BACKGROUND-IMAGE", BuildUrl("ResellerBkgnd.jpg"));
				}

				if (log.IsDistributor())
				{
					Form1.Style.Add("BACKGROUND-IMAGE", BuildUrl("DistributorBkgnd.jpg"));
				}
				SetLabel();
				UserInfo.ToSession(null);
			}
/* TODO RSWEB
            if (InstallationData.BrandLoader.GetMainBrandInfo().IsMagoNet())
			{
				Image imageLogo = new Image();
				imageLogo.ImageUrl = Helper.GetImageUrl("logoMagoNetEasylook.png"); 
				ImageLogoCell.Controls.Add(imageLogo);
			}
*/			
			//Imposto il titolo
			Page.Title = CommonFunctions.GetBrandedTitle();
            //DoLogin("sa", "", "MagoWebLook");
		}

		//---------------------------------------------------------------------
		private static string BuildUrl (string image)
		{
			return string.Format("url({0})", Helper.GetImageUrl(image));
		}

		//---------------------------------------------------------------------
		private void NotLicensedLayout ()
		{
			MsgLabel.Visible = true;
			MsgLabel.Text = LabelStrings.NoLicesed;
			UserTextBox.Enabled = false;
			PasswordTextBox.Enabled = false;
			CompanyComboBox.Enabled = false;
			OkButton.Enabled = false;
		}

		#region Web Form Designer generated code
		/// <summary>
		/// Questa chiamata è richiesta da Progettazione Web Form ASP.NET.
		/// </summary>
		/// <param name="e"></param>
		override protected void OnInit (EventArgs e)
		{
			InitializeComponent();
			base.OnInit(e);
		}

		/// <summary>
		/// Metodo necessario per il supporto della finestra di progettazione. Non modificare
		/// il contenuto del metodo con l'editor di codice.
		/// </summary>
		private void InitializeComponent ()
		{

		}
		#endregion

		//--------------------------------------------------------------------------
		/// <summary>
		/// Funzione che setta le Label presenti nella pagina localizandole secondo la 
		/// culture corrente 
		/// </summary>
		private void SetLabel ()
		{
			this.TitleLabel.Text = LabelStrings.ConnectionLabel;
			this.UserLabel.Text = LabelStrings.Users;
			this.PasswordLabel.Text = LabelStrings.Password;
			this.CompanyLabel.Text = LabelStrings.Company;
			this.OkButton.Text = LabelStrings.Enter;

			//stringhe dei controlli per cambio password
			this.LabelOldPwd.Text = LabelStrings.OldPassword;
			this.LabelNewPwd.Text = LabelStrings.NewPassword;
			this.LabelConfirmNewPwd.Text = LabelStrings.ConfirmNewPassword;
			this.ChangePwdBtn.Text = LabelStrings.ChangePassword;
			

			//this.TitleLabel1.Text = LabelStrings.LoginTitle1;
			this.BottomLabel.Text = ""; //TODO mettere browser compatibilil

			if (Request.Params["Invalid"] == "true")
			{
				this.MsgLabel.Visible = true;
				this.MsgLabel.Text = LabelStrings.SessionNoLongerValid;
			}
		}
		//--------------------------------------------------------------------------
		/// <summary>
		/// Evento di Click sul pulsante Ok.
		/// Effettua i controlli di autenticazione 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void OkButton_Click (object sender, System.EventArgs e)
		{
			string user = this.UserTextBox.Text;
			string password = this.PasswordTextBox.Text;
			string company = CompanyComboBox.SelectedValue;
            DoLogin(user, password, company);
		}

        //--------------------------------------------------------------------------
		private void DoLogin(string user, string password, string company)
        {
            bool useNTAuth = false;

            if (string.Compare(user, SessionKey.Anonimous) == 0)
                password = NameSolverStrings.GuestPwd;

            UserInfo ui = new UserInfo();

            //se ho gia` visualizzato la richiesta di sovrascrivere la login, la password e' salvata in sessione
            ui.OverwriteLogin = Request.Params["OverwriteLogin"] == "true";
            if (ui.OverwriteLogin)
                password = Session["PasswordValue"] as string;

            useNTAuth = UserInfo.IsIntegratedSecurityUser(user);

            string message = string.Empty;
            //effettuo la login, la prima volta senza sovrascrivere,e controllo integrita del database di sistema
            if (ui.Login(user, password, company, useNTAuth) && IsDatabaseValid(out message, ui.LoginManager))
            {
                UserInfo.ToSession(ui);
                EasyLookCustomSettings easyLookCustomSettings = new EasyLookCustomSettings(ui.PathFinder, ui.CompanyId, ui.LoginId);
                Session.Add(EasyLookCustomSettings.SessionKey, easyLookCustomSettings);
                Response.SetCookie(new HttpCookie("authtoken", ui.LoginManager.AuthenticationToken));
                FormsAuthentication.RedirectFromLoginPage(ui.LoginManager.UserName, false);

#if TBWEB

                //mando una chiamata asincrona per effettuare la login in TB
                UriBuilder uri = new UriBuilder(Request.Url);
                uri.Path = string.Concat(Path.GetDirectoryName(uri.Path), "/tbloader/login/");
                HttpWebRequest client = (HttpWebRequest)WebRequest.Create(uri.Uri);
                client.CookieContainer = new CookieContainer(Request.Cookies.Count);
                client.CookieContainer.Add(new Cookie("authtoken", ui.LoginManager.AuthenticationToken, uri.Path, uri.Host));
                foreach (string ck in Request.Cookies)
                {
                    //string ck = "ASP.NET_SessionId";
                    HttpCookie cook = Request.Cookies[ck];
                    client.CookieContainer.Add(new Cookie(ck, cook.Value, cook.Path, uri.Host));
                }

                client.BeginGetResponse(null, null);

#endif

                Application.AddUserInfoToApplication(ui);

            }
            else if (ui.ErrorCode == (int)LoginReturnCodes.UserAlreadyLoggedError)
            {
                //se poi l'utente e' gia` connesso, faccio un postback chiedendo via
                //javascript se si vuole sovrascrivere la login

                string script = string.Format(@"AskForOverWrite('{0}')", LabelStrings.OverwriteLogin);
                System.Web.UI.ScriptManager.RegisterStartupScript(this, BkgndPanel.GetType(), "AskForOverWrite", script, true);
                Session["PasswordValue"] = password;
            }
            else if (ui.ErrorCode == (int)LoginReturnCodes.UserMustChangePasswordError)
            {
                PanelChangePwd.Visible = true;
                PanelLogin.Visible = false;
                OkButton.Visible = false;
                TitleLabel.Text = LabelStrings.ChangePassword;
                OkPanel.Visible = false;
                OuterUpdatePanel.Update();
                UserInfo.ToSession(ui);
            }
            else
            {
                MsgLabel.Text = !string.IsNullOrEmpty(ui.ErrorExplain) ? ui.ErrorExplain : message;
                MsgLabel.Visible = true;
            }
        }

		/// <summary>
		/// Dice se il database aziendale e' valido, e in caso contrario valorizza un messaggio da visualizzare a video.
		/// </summary>
		//--------------------------------------------------------------------------
		private bool IsDatabaseValid(out string msg, LoginManager loginManager)
		{
			// effettuo il check di versione release sul database
			DatabaseCheckError checkDBRet = DatabaseCheckError.NoDatabase;
			msg = String.Empty;

			try
			{
				checkDBRet = CheckDatabase(out msg, loginManager);
			}
			catch (TBException e)
			{
				loginManager.LogOff();
				msg = e.Message;
				return false;
			}

			switch (checkDBRet)
			{
				case DatabaseCheckError.NoError:
					return true;

				case DatabaseCheckError.NoDatabase:
					loginManager.LogOff();
					msg = string.IsNullOrEmpty(msg) ? WebServicesWrapperStrings.CompanyDatabaseNotPresent : msg;
					return false;

				case DatabaseCheckError.NoTables:
					loginManager.LogOff();
					msg = WebServicesWrapperStrings.CompanyDatabaseTablesNotPresent;
					return false;

				case DatabaseCheckError.NoActivatedDatabase:
					if (Microarea.TaskBuilderNet.Core.Generic.Functions.IsDebug() && loginManager != null && loginManager.IsDeveloperActivation())
						goto case DatabaseCheckError.NoError;
					loginManager.LogOff();
					msg = WebServicesWrapperStrings.InvalidDatabaseForActivation;
					return false;

				case DatabaseCheckError.InvalidModule:
					msg = WebServicesWrapperStrings.InvalidDatabaseError;
					loginManager.LogOff();
					return false;

				case DatabaseCheckError.DBSizeError:
					msg = WebServicesWrapperStrings.DBSizeError;
					loginManager.LogOff();
					return false;

                case DatabaseCheckError.Sql2012NotAllowedForDMS:
                    msg = WebServicesWrapperStrings.Sql2012NotAllowedForDMS;
                    loginManager.LogOff();
                    return false;

                case DatabaseCheckError.Sql2012NotAllowedForCompany:
                    msg = WebServicesWrapperStrings.Sql2012NotAllowedForCompany;
                    loginManager.LogOff();
                    return false;

				default:
					msg = WebServicesWrapperStrings.ErrLoginFailed;
					loginManager.LogOff();
					return false;
			}
		}

		/// <summary>
		/// Effettua il controllo sulla struttura del database aziendale.
		/// </summary>
		//-----------------------------------------------------------------------
		private DatabaseCheckError CheckDatabase(out string msg, LoginManager loginManager)
		{
			DatabaseCheckError result = 0;
			msg = String.Empty;
			TBConnection myConnection = null;

			try
			{
				myConnection = new TBConnection(loginManager.NonProviderCompanyConnectionString, TBDatabaseType.GetDBMSType(loginManager.ProviderName));
				myConnection.Open();

                //*** todo DOPO USCITA 3.9 LO SPOSTERò in loginmanager . ila
                // esegue le verifiche di SQL2012 solo se la licenza non e' corretta
					if (!loginManager.Sql2012Allowed("sbirulino"))
					{
						// prima controllo il database aziendale
                        if (TBCheckDatabase.IsSql2012Edition(myConnection))
							return DatabaseCheckError.Sql2012NotAllowedForCompany;

						// se e' attivato EasyAttachment
						if (loginManager.IsActivated(NameSolverStrings.Extensions, DatabaseLayerConsts.EasyAttachment))
						{
							// mi faccio ritornare la stringa di connessione al DMS
							string dmsConnString = loginManager.GetDMSConnectionString(loginManager.AuthenticationToken);
							if (!string.IsNullOrWhiteSpace(dmsConnString))
							{
								using (TBConnection dmsConnection = new TBConnection(dmsConnString, DBMSType.SQLSERVER))
								{
									dmsConnection.Open();
									// controllo l'edizione del db del DMS
									if (TBCheckDatabase.IsSql2012Edition(dmsConnection))
										return DatabaseCheckError.Sql2012NotAllowedForDMS;
								}
							}
						}
					}
                //***

				result = TBCheckDatabase.CheckDatabase
				(
				myConnection,
				loginManager.GetDBNetworkType(),
				BasePathFinder.BasePathFinderInstance,
				loginManager.IsDeveloperActivation()
				);
			}
			catch (TBException e)
			{
				msg = e.Message;
				return (int)DatabaseCheckError.NoDatabase;
			}
			finally
			{
				if (myConnection != null && myConnection.State != ConnectionState.Open)
					myConnection.Close();
			}

			return result;
		}


		/// <summary>
		/// Evento di TextChange sul campo User.
		/// Carica all'interno della combo l'elenco delle Azienda alle quali è associato
		/// l'Utente
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		//--------------------------------------------------------------------------
		protected void UserTextBox_TextChanged (object sender, System.EventArgs e)
		{
			MsgLabel.Text = "";
			MsgLabel.Visible = false;
			LoadCompanies();
		}


		/// <summary>
		/// Evento di Cambio Password.
		/// </summary>
		//--------------------------------------------------------------------------
		protected void ChangePwdButton_Click(object sender, System.EventArgs e)
		{
			string message = String.Empty;
			UserInfo ui = UserInfo.FromSession();
			int changePwdResult;

			if (ui == null)
				return;

            string newPwd = TextBoxNewPwd.Text;
			if (newPwd.CompareTo(TextBoxConfirmNewPwd.Text) != 0)
			{
				ShowUserMessage(LabelStrings.MatchPassword);
				return;
			}

			try
			{
				changePwdResult = ui.LoginManager.ChangePassword(ui.User, TextBoxOldPwd.Text, newPwd);
			}

			catch(WebException exc)
			{
				if (exc.Response != null ) 
				{
					HttpWebResponse webResponse = (HttpWebResponse)exc.Response;
					if (webResponse.StatusDescription.Length > 0)
						message = string.Format(WebServicesWrapperStrings.ServerDown, webResponse.StatusDescription);
					else
						message = string.Format(WebServicesWrapperStrings.ServerDown, webResponse.StatusCode.ToString());
					
					webResponse.Close();
				}
				else
				{
					message = string.Format(WebServicesWrapperStrings.ServerDown, exc.Status.ToString());
				}
				return;
			}

			switch(changePwdResult)
			{
				case (int)LoginReturnCodes.NoError:
						{
                            ui.Login(ui.User, newPwd, ui.Company, UserInfo.IsIntegratedSecurityUser(ui.User));
							UserInfo.ToSession(ui);
							EasyLookCustomSettings easyLookCustomSettings = new EasyLookCustomSettings(ui.PathFinder, ui.CompanyId, ui.LoginId);
							Session.Add(EasyLookCustomSettings.SessionKey, easyLookCustomSettings);
							Response.SetCookie(new HttpCookie("authtoken", ui.LoginManager.AuthenticationToken));
							FormsAuthentication.RedirectFromLoginPage(ui.LoginManager.UserName, false);
							Application.AddUserInfoToApplication(ui);
							return;
						}
				case (int)LoginReturnCodes.InvalidUserError:
						{
							//Non è riuscito a cambiare la password. Utente non valido
							message = WebServicesWrapperStrings.ErrInvalidUser;
							break;
						}

				case (int)LoginReturnCodes.PasswordTooShortError:
						{
							//Non è riuscito a cambiare la password. Password troppo corta
							message = WebServicesWrapperStrings.ErrPwdLength;
							break;
						}
				case (int)LoginReturnCodes.CannotChangePasswordError:
				case (int)LoginReturnCodes.PasswordExpiredError:
						{
							//Non è riuscito a cambiare la password.
							message = WebServicesWrapperStrings.ErrUserCannotChangePwdButMust;
							break;
						}
				case (int)LoginReturnCodes.PasswordAlreadyChangedToday:
						{
							message = WebServicesWrapperStrings.PasswordAlreadyChangedToday;
							break;
						}
			}

			ShowUserMessage(message);
		}

		/// <summary>
		/// mostra il messaggio all'utente
		/// </summary>
		/// <returns></returns>
		//---------------------------------------------------------------------
		private void ShowUserMessage(string message)
		{
			if (!message.IsNullOrEmpty())
			{
				ChangePwdLabelMessage.Text = message;
				ChangePwdLabelMessage.Visible = true;
				OkPanel.Visible = false;
				OuterUpdatePanel.Update();
			}
		}

		/// <summary>
		/// Inserisce all'interno della combo l'elenco delle compani associate 
		/// all'Utente selezionato.
		/// </summary>
		/// <returns></returns>
		//---------------------------------------------------------------------
		private bool LoadCompanies ()
		{
			//if (UserInfo.IsIntegratedSecurityUser(UserTextBox.Text))
			//{
			//    MsgLabel.Text = LoginManagerWrapperStrings.AuthenticationTypeError;
			//    MsgLabel.Visible = true;
			//    return false;
			//}
			string[] companies = UserInfo.EnumCompanies(UserTextBox.Text);

			CompanyComboBox.Items.Clear();
			if (companies == null)
			{
				MsgLabel.Text = ApplicationsStrings.SoapError;
				MsgLabel.Visible = true;
				return false;
			}

			int companyNo = companies.GetLength(0);
			if (companyNo == 0)
			{
				MsgLabel.Text = ApplicationsStrings.NoCompanies;
				MsgLabel.Visible = true;
				return false;
			}

			if (companyNo > 0)
			{
				OkButton.Enabled = true;
				PasswordTextBox.Enabled = true;
				System.Web.UI.ScriptManager.GetCurrent(this).SetFocus(PasswordTextBox);
				CompanyComboBox.Visible = true;
				CompanyLabel.Visible = true;
				CompanyComboBox.Enabled = false;
				CompanyLabel.Enabled = false;
			}

			for (int i = 0; i < companyNo; i++)
			{
				ListItem li = new ListItem(companies[i]);
				if (lastCompanyUsed.Value.CompareNoCase(companies[i]))
					li.Selected = true;
				CompanyComboBox.Items.Add(li);
			}
			
			if (companyNo > 1)
			{
				CompanyComboBox.Enabled = true;
				CompanyLabel.Enabled = true;
			}

			return true;
		}
	}
}
