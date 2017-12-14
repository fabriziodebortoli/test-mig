using System;
using System.Web;
using System.Web.UI;

using Microarea.TaskBuilderNet.Core.Applications;
using Microarea.TaskBuilderNet.Woorm.WoormWebControl;

namespace Microarea.Web.EasyLook
{
	/// <summary>
	/// Descrizione di riepilogo per WoormWebForm.
	/// </summary>
	public partial class WoormWebForm : System.Web.UI.Page
	{
		/// <summary>
		/// WoormWebControl che visualizzarà il report
		/// </summary>
		protected WoormWebControl woorm;
	
		/// <summary>
		/// Evento di Load della pagina, disabilita la Cache
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		//-----------------------------------------------------------------------------------------
		protected void Page_Load(object sender, System.EventArgs e)
		{
			// Disabilita la cache
			this.Response.Cache.SetCacheability(HttpCacheability.NoCache);
			UserInfo ui = UserInfo.FromSession();
			if (ui == null)
				return;

			//Se non ha il titolo del report, gli imposto il titolo di default (mago.net - easylook) 
			if (String.IsNullOrEmpty(Page.Title))
				Page.Title = CommonFunctions.GetBrandedTitle();
		}

		//-----------------------------------------------------------------------------------------
		protected override void OnError(EventArgs e)
		{
			base.OnError(e);
			Helper.RedirectToErrorPageIfPossible();
		}

		#region Web Form Designer generated code
		/// <summary>
		/// Questa chiamata è richiesta da Progettazione Web Form ASP.NET.
		/// </summary>
		/// <param name="e"></param>
		override protected void OnInit(EventArgs e)
		{
			InitializeComponent();
			base.OnInit(e);

			UserInfo ui = UserInfo.FromSession();
			if (ui == null)
				Page.ClientScript.RegisterStartupScript(GetType(), "CloseWindow", "window.close();", true);
		}
		
		/// <summary>
		/// Metodo necessario per il supporto della finestra di progettazione. Non modificare
		/// il contenuto del metodo con l'editor di codice.
		/// </summary>
		private void InitializeComponent()
		{    

		}
		#endregion
	}
}
