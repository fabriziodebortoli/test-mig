using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Web;
using System.Web.UI;
using Microarea.TaskBuilderNet.Core.Applications;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Interfaces;
using Microarea.TaskBuilderNet.Core.MenuManagerLoader;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Core.WebServicesWrapper;

namespace Microarea.Web.EasyLook
{
	//================================================================================
	public static class Helper
	{
		private	const string errorPageName = "ErrorPage.aspx";
		private const string filesPath = @"./Files/Images";
        private const string physicalFilesPath = @"Files\Images";
        public const string LatestExceptionKey = @"LatestException";

        //--------------------------------------------------------------------------------
		public static string GetImageUrl (string imageFile)
		{
			return string.Format("{0}/{1}", filesPath, imageFile);
		}

        public static string GetImagePhysicalPath(string imageFile)
        {
            return string.Format(@"{0}\{1}", physicalFilesPath, imageFile);
        }


		//--------------------------------------------------------------------------------
		public static void RedirectToErrorPageIfPossible ()
		{ 
			if (HttpContext.Current == null || HttpContext.Current.Session == null)
				return;
            
            // serve ad evitare il loop nel caso di errore a basso livello all'interno della 
            // error page in ASP.NET
			if (HttpContext.Current.Request != null && HttpContext.Current.Request.Url.AbsoluteUri.IndexOf(errorPageName) >= 0)
                return;
	
			Exception objErr = HttpContext.Current.Server.GetLastError();
			if (objErr == null)
				return;
			
			//se l'eccezione e' di tipo InvalidSessionException viene gestito il redirect sulla pagina di login
			if (objErr is InvalidSessionException)
				RedirectToLogin();

			RedirectToErrorPage(objErr);
		}
		//--------------------------------------------------------------------------------
		public static void RedirectToErrorPage (Exception ex)
		{
			HttpContext.Current.Server.ClearError();
			HttpContext.Current.Session[LatestExceptionKey] = ex;
			HttpContext.Current.Response.Redirect("~/" + errorPageName, true);
		}

		//--------------------------------------------------------------------------------
		public static void RedirectToLogin()
		{
            string returnURL = HttpContext.Current.Request.RawUrl.IsNullOrEmpty() ? "default.aspx" : HttpContext.Current.Request.RawUrl;
            HttpContext.Current.Response.Redirect(string.Format("~/login.aspx?ReturnUrl={0}&Invalid=true", returnURL), true);
        }

		//--------------------------------------------------------------------------------
		public static void RedirectToLogin (this Page page)
		{
			string scriptValue = string.Format(@"parent.location.href = 'login.aspx?ReturnUrl=default.aspx&Invalid=true';");

			page.ClientScript.RegisterStartupScript(page.ClientScript.GetType(), "ReloadLogin", scriptValue, true);
		}
		
		//--------------------------------------------------------------------------
		/// <summary>
		/// Aggiunge le informazioni utente nell'applicazione; le informazioni utente sono in
		/// applicazione perche' devo poter capire se, all'avvio di una nuova sessione, l'utente e' gia` collegato
		/// </summary>
		/// <param name="Application"></param>
		/// <param name="ui"></param>
		public static void AddUserInfoToApplication (this HttpApplicationState Application, UserInfo ui)
		{
			Application.Lock();
			try
			{
				UserInfo existingUser = Application[ui.ApplicationKey] as UserInfo;
				//Se l'utente e' gia` presente in application, vuol dire che ha un'altra sessione attiva, quindi la invalido...
				if (existingUser != null)
					existingUser.Valid = false;
				//metto la nuova sessione in application
				Application[ui.ApplicationKey] = ui;
			}
			finally
			{
				Application.UnLock();
			}
		}

		//--------------------------------------------------------------------------
		/// <summary>
		/// Elimina le informazioni utente dall'applicazione; le informazioni utente sono in
		/// applicazione perche' devo poter capire se, all'avvio di una nuova sessione, l'utente e' gia` collegato
		/// </summary>
		/// <param name="Application"></param>
		/// <param name="ui"></param>
		public static void RemoveUserInfoFromApplication (this HttpApplicationState Application, UserInfo ui)
		{
			Application.Lock();
			try
			{
				UserInfo existingUser = Application[ui.ApplicationKey] as UserInfo;
				//if user info differs from mine, another session has been activated meanwhile
				if (existingUser != ui)
					return;
				Debug.Assert(ui.Valid);

				Application.Remove(ui.ApplicationKey);

				ui.Dispose();
			}
			finally
			{
				Application.UnLock();
			}
		}

		

		//--------------------------------------------------------------------------------
		internal static MenuXmlParser GetMenuXmlParser()
		{
			return (MenuXmlParser)HttpContext.Current.Session[SessionKey.Parser];
		}

		//---------------------------------------------------------------------
		/// <summary>
		/// Funzione che carica il MenuXmlParser e lo filtra secondo i criteri della security
		/// </summary>
		/// <returns></returns>
		internal static MenuXmlParser LoadMenuXmlParser()
		{
			// l'impostazione della culture va effettuata prima di chiamare il parser
			// (altrimenti al form non viene tradotta)
			UserInfo ui = UserInfo.FromSession();
			if (ui == null)
				return null;

			if (ui != null && ui.LoginManager != null && ui.LoginManager.LoginManagerState == LoginManagerState.Logged)
				DictionaryFunctions.SetCultureInfo
				(
					ui.LoginManager.PreferredLanguage,
					ui.LoginManager.ApplicationLanguage
				);

			if (ui.PathFinder == null || ui.LoginManager == null)
				return null;

			return LoadMenuXmlParser(ui.LoginManager, ui.PathFinder);
		}

		//---------------------------------------------------------------------
		public static String NewCPCCCHK()
		{
			Random rnd = new Random(DateTime.Now.Millisecond);

			char[] arrayOfChar = new char[10];
			for (int i = 0; i < 10; i++)
				arrayOfChar[i] = (char)(int)(rnd.NextDouble() * 26.0D + 97.0D);

			return new String(arrayOfChar).ToUpperInvariant();
		}

		//---------------------------------------------------------------------
		internal static MenuXmlParser LoadMenuXmlParser(LoginManager lm, IPathFinder pf)
		{
			object o = HttpContext.Current.Session[SessionKey.Parser];
			if (o != null)
				return (MenuXmlParser)o;

			MenuLoader mySearch = new MenuLoader(pf, lm, false);

			mySearch.LoadAllMenus(false, false);
			MenuXmlParser parserDomMenu = mySearch.AppsMenuXmlParser;

			
			
			bool applySecurityFilter = false;
			bool showDocuments = false;
			bool webGDImode = false;

			if (lm != null)
			{
				applySecurityFilter = lm.IsActivated("MicroareaConsole", "SecurityAdmin") &&
					(
					lm.LoginManagerState != LoginManagerState.Logged ||
					lm.Security
					);

				showDocuments = true;
			}
			
			MenuSecurityFilter menuSecurityFilter = new MenuSecurityFilter(InstallationData.ServerConnectionInfo.SysDBConnectionString, pf.Company, pf.User, applySecurityFilter);

			// if in session WebGdi is null equal at False
			if (HttpContext.Current.Session["WebGdi"] != null)
				webGDImode = (bool) HttpContext.Current.Session["WebGdi"];

			//DP: disable filter menu easyLook for gdi web menu version
			if (!webGDImode)
				menuSecurityFilter.FilterEasylook(parserDomMenu, showDocuments);
			
			if (applySecurityFilter)
				menuSecurityFilter.Filter(parserDomMenu);
			
			HttpContext.Current.Session[SessionKey.Parser] = parserDomMenu;
			return parserDomMenu;
		}
	}
}
