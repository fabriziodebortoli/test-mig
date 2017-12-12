using System;
using System.Web;
using Microarea.TaskBuilderNet.Core.Applications;
using Microarea.TaskBuilderNet.Core.MenuManagerLoader;
using Microarea.TaskBuilderNet.Core.WebServicesWrapper;
using Microarea.TaskBuilderNet.Core.Generic;


namespace Microarea.Web.EasyLook
{
    /// <summary>
    /// Summary description for HomeFrame.
    /// </summary>
    public partial class Default : System.Web.UI.Page
    {

        //------------------------------------------------------------------------------
        /// <summary>
        /// Evento di Load della pagina setta la Cache a NoCache
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Page_Load(object sender, System.EventArgs e)
        {
            // Disabilita la cache
            this.Response.Cache.SetCacheability(HttpCacheability.NoCache);
            // Imposta il titolo della pagina
            if (!this.IsPostBack)
            {
                UserInfo ui = UserInfo.FromSession();
                if (ui == null)
                {
                    this.RedirectToLogin();
                    return;
                }
                MenuXmlParser parserDomMenu = Helper.LoadMenuXmlParser();

                Page.Title = CommonFunctions.GetBrandedTitle();
            }
        }




        #region Web Form Designer generated code
        /// <summary>
        /// Questa chiamata è richiesta da Progettazione Web Form ASP.NET.
        /// </summary>
        /// <param name="e"></param>
        override protected void OnInit(EventArgs e)
        {
            UserInfo ui = UserInfo.FromSession();
            if (ui == null)
            {
                this.RedirectToLogin();
                return;
            }

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
