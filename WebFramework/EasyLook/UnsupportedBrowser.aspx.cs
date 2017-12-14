using System;

namespace Microarea.Web.EasyLook
{
	/// <summary>
	/// Summary description for UnsupportedBrowser.
	/// </summary>
	public partial class UnsupportedBrowser : System.Web.UI.Page
	{
	
		/// <summary>
		/// Evento di Load della pagina setta le stringhe localizzate
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void Page_Load(object sender, System.EventArgs e)
		{
			Label1.Text = LabelStrings.BrowserNotValid;
			Label2.Text = LabelStrings.EasyLookDefaultBrowser;
			Label3.Text = LabelStrings.InstallationLabel;
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
