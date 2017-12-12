using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.TaskBuilderNet.UI.WinControls.AdvertisementRenderer
{
	/// <summary>
	/// AdvertisementRenderer.
	/// </summary>
	//=========================================================================
	public class HtmlAdvRenderer : BaseAdvRenderer
	{
		private System.ComponentModel.Container components = null;
		private const string br = "<br/>";
		private bool onLine = true;
		private WebBrowser axWebBrowser;
		private string loginManagerUrl;

		public bool OnLine {get {return onLine;} set {onLine = value;}}
		public string LoginManagerUrl {get {return loginManagerUrl;} set {loginManagerUrl = value;}}
       
        public delegate void TbNavigateEventHandler(TbNavigateEventArgs e);
        public event TbNavigateEventHandler TbNavigate;

		//--------------------------------------------------------------------
		public HtmlAdvRenderer()
		{
			InitializeComponent();

			axWebBrowser.Navigated += new WebBrowserNavigatedEventHandler(axWebBrowser_Navigated);
            axWebBrowser.Navigating +=new WebBrowserNavigatingEventHandler(axWebBrowser_Navigating);
		}

        //--------------------------------------------------------------------
        private void axWebBrowser_Navigating(object sender, WebBrowserNavigatingEventArgs e)
        {
            if (TbNavigateEventArgs.IsTbNavigateUrl(e.Url))
            {
                e.Cancel = true;

                if (TbNavigate != null)
                    TbNavigate(new TbNavigateEventArgs(e.Url));
            }
        }

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		//--------------------------------------------------------------------
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
				if (axWebBrowser != null)
				{
					axWebBrowser.Dispose();
					axWebBrowser = null;
				}
			}
			base.Dispose( disposing );
		}

		#region Component Designer generated code
		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		//--------------------------------------------------------------------
		private void InitializeComponent()
		{
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(HtmlAdvRenderer));
            this.axWebBrowser = new System.Windows.Forms.WebBrowser();
            this.SuspendLayout();
            // 
            // axWebBrowser
            // 
            resources.ApplyResources(this.axWebBrowser, "axWebBrowser");
            this.axWebBrowser.MinimumSize = new System.Drawing.Size(20, 20);
            this.axWebBrowser.Name = "axWebBrowser";
            
            // 
            // HtmlAdvRenderer
            // 
            this.BackColor = System.Drawing.SystemColors.Control;
            this.Controls.Add(this.axWebBrowser);
            this.Name = "HtmlAdvRenderer";
            resources.ApplyResources(this, "$this");
            this.ResumeLayout(false);

		}
		#endregion

		/// <summary>
		/// Salva il contenuto del messaggio in unu file temporaneo,
		/// lo renderizza e rimuove il file creato.
		/// </summary>
		/// <param name="advertisement"></param>
		//--------------------------------------------------------------------
		public override void RenderAdvertisement(IAdvertisement advertisement)
		{
			if (advertisement == null)
				return;

			if (advertisement.Body.LocalizationBag != null)
			{
				string ns = "Microarea.TaskBuilderNet.UI.WinControls.AdvertisementRenderer.Template.html";
				Stream s = Assembly.GetExecutingAssembly().GetManifestResourceStream(ns);
				if (s == null)
					return;//todo log
				string html = new StreamReader(s).ReadToEnd();
				
				advertisement.Body.Html = ComposeMessage(advertisement.Body.LocalizationBag, html, br);
				/*using (StreamWriter sw = new StreamWriter(@"C:\messages\" + advertisement.ID+ ".htm"))
					sw.Write(advertisement.Body.Html);*/
			}

			string tempPath = Path.ChangeExtension(Path.GetTempFileName(), ".html");
            using (StreamWriter sw = new StreamWriter(tempPath, true, Encoding.Unicode))
                sw.Write(advertisement.Body.Html);


			//salvo anche i css e htc se non ci sono già
			string dir = Path.GetDirectoryName(tempPath);
			string resNamespace = "Microarea.TaskBuilderNet.UI.WinControls.AdvertisementRenderer.{0}";
			string microareanewcss = "microareanew.css";
			string microareatable  = "microareatable.css";
			string htc			   = "webservice.htc";
			string microareanewcssPath = Path.Combine(dir, "microareanew.css");
			string microareatablePath  = Path.Combine(dir, "microareatable.css");
			string htcPath			   = Path.Combine(dir, "webservice.htc");

			if (!File.Exists(microareanewcssPath))
			{
				Stream s1 = Assembly.GetExecutingAssembly().GetManifestResourceStream(string.Format(resNamespace, microareanewcss));
				using (FileStream sw = File.Create(microareanewcssPath))
				{
					byte[] buffer = new byte[s1.Length];
					 s1.Read(buffer, 0, buffer.Length);
					sw.Write(buffer, 0, buffer.Length);			
				}
			}

			if (!File.Exists(microareatablePath))
			{
				Stream s1 = Assembly.GetExecutingAssembly().GetManifestResourceStream(string.Format(resNamespace, microareatable));
				using (FileStream sw = File.Create(microareatablePath))
				{
					byte[] buffer = new byte[s1.Length];
					s1.Read(buffer, 0, buffer.Length);
					sw.Write(buffer, 0, buffer.Length);			
				}
			}

			if (!File.Exists(htcPath))
			{
				Stream s1 = Assembly.GetExecutingAssembly().GetManifestResourceStream(string.Format(resNamespace, htc));
				using (FileStream sw = File.Create(htcPath))
				{
					byte[] buffer = new byte[s1.Length];
					s1.Read(buffer, 0, buffer.Length);
					sw.Write(buffer, 0, buffer.Length);			
				}
			}

			try
			{
				axWebBrowser.Navigate(tempPath);
			}
			catch(Exception e)
			{
				System.Diagnostics.Debug.WriteLine(e.ToString());
			}
		}

		//--------------------------------------------------------------------
		public virtual string ComposeMessage(ILocalizationBag localizationBag, string html, string newline)
		{
			if (localizationBag == null || html == null || html.Length == 0)
				return String.Empty;

			string dearcustomer = string.Format(WinControlsStrings.DearCustomer, newline);
			string clickHereLink = string.Format("<a href=\"javascript:callWebService()\">{0}</a>", " " + WinControlsStrings.ClickHere);
			string microareaLink = string.Format("<a href=\"http://www.microarea.it\" target=\"_blank\">{0}</a>" , ConstString.MicroareaSite);
			string toReceiveEmail = onLine ? string.Format(WinControlsStrings.ToReceiveMail, clickHereLink, localizationBag.UserEmail, newline) : String.Empty;
			decimal monthsApprox = new TimeSpan(localizationBag.RenewalPeriodTicks).Days/30;
			decimal months = Math.Round(monthsApprox, 0);
			string javaScripts = ConstString.JavaScripts.Replace("##LOGINMANAGERURL##", loginManagerUrl);

			switch (localizationBag.Key)
			{
				case "ContractBeforeExpiring":
					//nome prodotto, giorni che mancano alla scadenza
					string contract13_1 = String.Format(WinControlsStrings.Contract13_1, localizationBag.ProductName, localizationBag.Days);
					//clicca qui con link, user email, microarea.it, a capo
					string contract13_2 = String.Format(WinControlsStrings.Contract13_2, toReceiveEmail, microareaLink, newline);
					//formattazione del template html coi vari componenti
					return string.Format(
						html,//"{0}{1}{2}{3}{4}{5}{6}",
						WinControlsStrings.ContractExpiring,
						String.Concat(dearcustomer, contract13_1),
						contract13_2,
						WinControlsStrings.Ignore,
						WinControlsStrings.Thanks,
						WinControlsStrings.ClickHere, 
						javaScripts);

				case "FreePeriodBeforeExpiring":
					//nome prodotto, giorni che mancano alla scadenza
					string freePeriod11_1 = String.Format(WinControlsStrings.FreePeriod11_1, localizationBag.ProductName, localizationBag.Days);
					//clicca qui con link, user email, microarea.it, a capo
					string freePeriod11_2 = String.Format(WinControlsStrings.FreePeriod11_2, toReceiveEmail, microareaLink, newline);

					//formattazione del template html coi vari componenti
					return string.Format(
						html,
						WinControlsStrings.FreePeriodExpiring,
						String.Concat(dearcustomer, freePeriod11_1),
						freePeriod11_2,
						WinControlsStrings.Ignore,
						WinControlsStrings.Thanks,
						WinControlsStrings.ClickHere, 
						javaScripts);

				case "InstalmentsPeriodBeforeExpiring":
					//nome prodotto, giorni che mancano alla scadenza
					string instalment12_1 = String.Format(WinControlsStrings.Instalment12_1, localizationBag.ProductName, localizationBag.Days);
					//clicca qui con link, user email, microarea.it, a capo
					string instalment12_2 = String.Format(WinControlsStrings.Instalment12_2, toReceiveEmail, microareaLink, newline, months);

					//formattazione del template html coi vari componenti
					return string.Format(
						html,
						WinControlsStrings.InstalmentExpiring,
						String.Concat(dearcustomer, instalment12_1),
						instalment12_2,
						WinControlsStrings.Ignore,
						WinControlsStrings.Thanks,
						WinControlsStrings.ClickHere, 
						javaScripts);

				case "ContractExpiresToday":
					//nome prodotto, giorni che mancano alla scadenza
					string contract33_1 = String.Format(WinControlsStrings.Contract33_1, localizationBag.ProductName);
					//clicca qui con link, user email, microarea.it, a capo
					string contract33_2 = String.Format(WinControlsStrings.Contract33_2, toReceiveEmail, microareaLink, newline);

					//formattazione del template html coi vari componenti
					return string.Format(
						html,
						WinControlsStrings.ContractExpiring,
						String.Concat(dearcustomer, contract33_1),
						contract33_2,
						WinControlsStrings.Ignore,
						WinControlsStrings.Thanks,
						WinControlsStrings.ClickHere, 
						javaScripts);

				case "FreePeriodExpiresToday":
					//nome prodotto, giorni che mancano alla scadenza
					string freePeriod31_1 = String.Format(WinControlsStrings.FreePeriod31_1, localizationBag.ProductName);
					//clicca qui con link, user email, microarea.it, a capo
					string freePeriod31_2 = String.Format(WinControlsStrings.FreePeriod31_2, toReceiveEmail, microareaLink, newline);

					//formattazione del template html coi vari componenti
					return string.Format(
						html,
						WinControlsStrings.FreePeriodExpiring,
						String.Concat(dearcustomer, freePeriod31_1),
						freePeriod31_2,
						WinControlsStrings.Ignore,
						WinControlsStrings.Thanks,
						WinControlsStrings.ClickHere, 
						javaScripts);

				case "InstalmentsExpiresToday":
					//nome prodotto, giorni che mancano alla scadenza
					string instalment32_1 = String.Format(WinControlsStrings.Instalment32_1, localizationBag.ProductName);
					//clicca qui con link, user email, microarea.it, a capo
					string instalment32_2 = String.Format(WinControlsStrings.Instalment32_2, toReceiveEmail, microareaLink, newline, months);

					//formattazione del template html coi vari componenti
					return string.Format(
						html,
						WinControlsStrings.InstalmentExpiring,
						String.Concat(dearcustomer, instalment32_1),
						instalment32_2,
						WinControlsStrings.Ignore,
						WinControlsStrings.Thanks,
						WinControlsStrings.ClickHere, 
						javaScripts);

				case "ContractExpired":
					//nome prodotto, giorni che mancano alla scadenza
					string contract23_1 = String.Format(WinControlsStrings.Contract23_1, localizationBag.ProductName, localizationBag.Days);
					//clicca qui con link, user email, microarea.it, a capo
					string contract23_2 = String.Format(WinControlsStrings.Contract23_2, toReceiveEmail, microareaLink, newline);
					//formattazione del template html coi vari componenti
					return string.Format(
						html,
						WinControlsStrings.ContractExpired,
						String.Concat(dearcustomer, contract23_1),
						contract23_2,
						WinControlsStrings.Ignore,
						WinControlsStrings.Thanks,
						WinControlsStrings.ClickHere, 
						javaScripts);

				case "FreePeriodExpired":
					//nome prodotto, giorni che mancano alla scadenza
					string freePeriod21_1 = String.Format(WinControlsStrings.FreePeriod21_1, localizationBag.ProductName, localizationBag.Days);
					//clicca qui con link, user email, microarea.it, a capo
					string freePeriod21_2 = String.Format(WinControlsStrings.FreePeriod21_2, toReceiveEmail, microareaLink, newline);
					//formattazione del template html coi vari componenti
					return string.Format(
						html,
						WinControlsStrings.FreePeriodExpired,
						String.Concat(dearcustomer, freePeriod21_1),
						freePeriod21_2,
						WinControlsStrings.Ignore,
						WinControlsStrings.Thanks,
						WinControlsStrings.ClickHere, 
						javaScripts);

				case "InstalmentsExpired":
					//nome prodotto, giorni che mancano alla scadenza
					string instalment22_1 = String.Format(WinControlsStrings.Instalment22_1, localizationBag.ProductName, localizationBag.Days);
					//clicca qui con link, user email, microarea.it, a capo
					string instalment22_2 = String.Format(WinControlsStrings.Instalment22_2, toReceiveEmail, microareaLink, newline, months);

					//formattazione del template html coi vari componenti
					return string.Format(
						html,
						WinControlsStrings.InstalmentExpired,
						String.Concat(dearcustomer, instalment22_1),
						instalment22_2,
						WinControlsStrings.Ignore,
						WinControlsStrings.Thanks,
						WinControlsStrings.ClickHere, 
						javaScripts);
			}
			return String.Empty;
		}

		/// <summary>
		/// Salva il contenuto del messaggio in unu file temporaneo,
		/// lo renderizza e rimuove il file creato.
		/// </summary>
		/// <param name="advertisement"></param>
		//--------------------------------------------------------------------
		public override void RenderAdvertisement(Uri documentUri)
		{
			if (documentUri == null)
				return;

			axWebBrowser.Navigate(documentUri.ToString());
		}

		//--------------------------------------------------------------------
		public override void Clear()
		{
			axWebBrowser.Navigate("about:blank");
		}

		//--------------------------------------------------------------------
		void axWebBrowser_Navigated(object sender, WebBrowserNavigatedEventArgs e)
        {
            
			try
			{
				Uri tempUri = new Uri(e.Url.ToString());
				if (tempUri.IsFile)
					File.Delete(tempUri.AbsolutePath);
			}
			catch (Exception exc)
			{
				System.Diagnostics.Debug.WriteLine(exc.ToString());
			}
		}
	}
    
    //=========================================================================
    public class TbNavigateEventArgs : EventArgs
    {
        /*template del body per messaggi con link a mago
            Namespace = "Document.ERP.Company.Documents.Company";
            Key = "CompanyId:0;";*/

        public string Key; 
        public string Namespace;
        public NameSpaceObjectType Type = NameSpaceObjectType.NotValid;

        //--------------------------------------------------------------------
        public TbNavigateEventArgs(Uri url)
        {
            Init(url);
        }

        //--------------------------------------------------------------------
        public TbNavigateEventArgs(string urlval)
        {
            Init(new Uri(urlval));
        }
        //--------------------------------------------------------------------
        public TbNavigateEventArgs()
        {
            
        }

        //--------------------------------------------------------------------
        private  void Init(Uri url)
        {
            if (IsTbNavigateUrl(url))
            {
                //l'uri è interpretato correttamente e ne possiamo usare le proprietà
                Namespace = url.Host;

                //tolgo punto interrogativo iniziale e decodo
                Key = System.Web.HttpUtility.UrlDecode(url.Query.Replace("?", ""));

                //utilizzo la classe namespace per recuperarmi il tipo, per sapere poi cosa fare con questo oggetto
                NameSpace ns = new NameSpace(Namespace);
                if (ns != null && !string.IsNullOrWhiteSpace(ns.FullNameSpace))
                    Type = ns.NameSpaceType.Type;
            }
        }
       
        //--------------------------------------------------------------------
        public static bool IsTbNavigateUrl(Uri url)
        {
            return (
                url != null && 
                !string.IsNullOrWhiteSpace(url.AbsolutePath) && 
                (
                string.Compare(url.Scheme, "tb", StringComparison.InvariantCultureIgnoreCase) == 0 ||
                string.Compare(url.Scheme, "imago", StringComparison.InvariantCultureIgnoreCase) == 0
                ));
        }
    }
}
