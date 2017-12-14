using System;
using System.IO;
using System.Web;
using System.Web.Caching;

using Microarea.TaskBuilderNet.Core.NameSolver;

namespace Microarea.WebServices.LoginManager 
{
	//=========================================================================
	public class Global : System.Web.HttpApplication
	{
		
		private System.ComponentModel.IContainer components = null;
		private Cache cache;
		
		//---------------------------------------------------------------------
		public Global()
		{
			InitializeComponent();
		}	
		
		//---------------------------------------------------------------------
		protected void Application_Start(Object sender, EventArgs e)
		{
			string appDataFolder = Server.MapPath("App_Data");
			if (!Directory.Exists(appDataFolder))
				Directory.CreateDirectory(appDataFolder);
			
			LoginApplication.LoginEngine.Init(
				true,
				new InitEventArgs(
					InitReason.ApplicationStart,
					LoginApplication.LoginEngine.LoginManagerVersion,
					string.Empty
					)
				);

			//metto gli oggetti in cache
			//Licensed
			FileInfo[] licensedInfos = BasePathFinder.BasePathFinderInstance.GetLicensedFiles();
			if (licensedInfos != null)
			{
				foreach (FileInfo info in licensedInfos)
				{
					Context.Cache.Insert
						(	
						info.FullName,
						info,  
						new CacheDependency(info.FullName),
						Cache.NoAbsoluteExpiration,
						Cache.NoSlidingExpiration, 
						CacheItemPriority.AboveNormal, 
						new CacheItemRemovedCallback(OnCacheItemRemoved)
						);
				}
			}

			//UserInfo
			string userInfo = BasePathFinder.BasePathFinderInstance.GetUserInfoFile();
			if (userInfo != string.Empty)
			{
				Context.Cache.Insert
					(	
					userInfo,
					userInfo,  
					new CacheDependency(userInfo),
					Cache.NoAbsoluteExpiration,
					Cache.NoSlidingExpiration, 
					CacheItemPriority.AboveNormal, 
					new CacheItemRemovedCallback(OnCacheItemRemoved)
					);
			}
			cache = HttpContext.Current.Cache;	
		}
 
		//---------------------------------------------------------------------
		public void OnCacheItemRemoved(string key, object aValue, CacheItemRemovedReason reason)
		{
			//Si fa il reinserimento in cache solo se si 
			//tratta di una DependencyChanged, perchè in caso di Removed 
			//- che capita per esempio in caso di aggiornamento di dll a caldo -
			//non è necessario
			if (reason != CacheItemRemovedReason.DependencyChanged)
				return;

			//reinserisco subito l'oggetto nella cache
			cache.Insert
				(
				key, 
				aValue,  
				new CacheDependency(key), 
				Cache.NoAbsoluteExpiration,
				Cache.NoSlidingExpiration, 
				CacheItemPriority.AboveNormal, 
				new CacheItemRemovedCallback(OnCacheItemRemoved)	
				);

			//non si riavvia più login manager, per evitare fastidiosi riavvi consecutivi
			//Che potrebbero far passare giorni di attività.
			//Magari neanche dovuti a noi, 
			//ma a tool esterni, vedi antivirus, che maneggiano il file system
			//Comunuque diagnostichiamo il fatto che i file siano statai toccati.
			LoginApplication.LoginEngine.LogInitEventArgs
					(new InitEventArgs(
					InitReason.OnCacheItemRemoved,
					LoginApplication.LoginEngine.LoginManagerVersion,
					aValue.ToString()
					));
		}

		//---------------------------------------------------------------------
		protected void Session_Start(Object sender, EventArgs e)
		{
		}

		//---------------------------------------------------------------------
		protected void Application_BeginRequest(Object sender, EventArgs e)
		{
		}

		//---------------------------------------------------------------------
		protected void Application_EndRequest(Object sender, EventArgs e)
		{
		}

		//---------------------------------------------------------------------
		protected void Application_AuthenticateRequest(Object sender, EventArgs e)
		{
		}

		//---------------------------------------------------------------------
		protected void Application_Error(Object sender, EventArgs e)
		{
		}

		//---------------------------------------------------------------------
		protected void Session_End(Object sender, EventArgs e)
		{
		}

		//---------------------------------------------------------------------
		protected void Application_End(Object sender, EventArgs e)
		{
            LoginApplication.LoginEngine.SaveNewProductHash();
			LoginApplication.LoginEngine.Dispose();
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

